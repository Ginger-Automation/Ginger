using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    public class PomRelearnWizard : WizardBase
    {
        public ApplicationPOMModel mOriginalPOM;

        public override string Title { get { return "Update POM Elements Wizard"; } }

        private Agent mAgent = null;

        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }

        public bool IsLearningWasDone { get; set; }
        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (Agent != null)
                {
                    return ((IWindowExplorer)(Agent.Driver));
                }
                else
                {
                    return null;
                }
            }
        }

        public ObservableList<ElementInfo> mPOMAllCurrentElements = new ObservableList<ElementInfo>();


        public PomRelearnWizard(ApplicationPOMModel POM, Agent agent)
        {
            mOriginalPOM = POM;
            mAgent = agent;
            PopulateDuplicatedUnienedElementsList();
            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Comparison Status of Elements with Latest", Page: new POMDeltaWizardPage());
        }


        public void PopulateDuplicatedUnienedElementsList()
        {
            mPOMAllCurrentElements.Clear();
            foreach (ElementInfo EI in mOriginalPOM.MappedUIElements)
            {
                ElementInfo DuplicatedEI = (ElementInfo)EI.CreateCopy(false);
                DuplicatedEI.DeltaStatus = ElementInfo.eDeltaStatus.Unchanged;
                DuplicatedEI.DeltaExtraDetails = ElementInfo.eDeltaExtraDetails.NA;
                DuplicatedEI.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                CorrectControlPropertyTypes(DuplicatedEI);
                mPOMAllCurrentElements.Add(DuplicatedEI);
            }
            foreach (ElementInfo EI in mOriginalPOM.UnMappedUIElements)
            {
                ElementInfo DuplicatedEI = (ElementInfo)EI.CreateCopy(false);
                DuplicatedEI.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
                DuplicatedEI.DeltaStatus = ElementInfo.eDeltaStatus.Unchanged;
                DuplicatedEI.DeltaExtraDetails = ElementInfo.eDeltaExtraDetails.NA;
                CorrectControlPropertyTypes(DuplicatedEI);
                mPOMAllCurrentElements.Add(DuplicatedEI);
            }
        }

        private void CorrectControlPropertyTypes(ElementInfo EI)
        {
            ObservableList<ControlProperty> newProperties = new ObservableList<ControlProperty>();

            foreach (ControlProperty property in EI.Properties)
            {
                if (property is POMElementProperty)
                {
                    return;
                }
                else
                {
                   POMElementProperty hTMLElementProperety = new POMElementProperty() { Name = property.Name, Value = property.Value };
                    newProperties.Add(hTMLElementProperety);
                }
            }

            EI.Properties = newProperties;
        }


        public override void Finish()
        {
            //Updating selected elements
            List<ElementInfo> ElementsToUpdate = mPOMAllCurrentElements.Where(x => x.IsSelected == true).ToList();
            foreach (ElementInfo EI in ElementsToUpdate)
            {
                //Add the New onces to the last of the list
                if (EI.DeltaStatus == ElementInfo.eDeltaStatus.New)
                {
                    if ((ApplicationPOMModel.eElementGroup)EI.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        mOriginalPOM.MappedUIElements.Add(EI);
                    }
                    else if ((ApplicationPOMModel.eElementGroup)EI.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped)
                    {
                        mOriginalPOM.UnMappedUIElements.Add(EI);
                    }
                    continue;
                }

                //Deleting deleted elements
                if (EI.DeltaStatus == ElementInfo.eDeltaStatus.Deleted)
                {
                    ElementInfo CorrespondingElementInfo = mOriginalPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
                    if (CorrespondingElementInfo != null)
                    {
                        mOriginalPOM.MappedUIElements.Remove(CorrespondingElementInfo);
                    }
                    else
                    {
                        CorrespondingElementInfo = mOriginalPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
                        mOriginalPOM.UnMappedUIElements.Remove(EI);
                    }

                    continue;
                }

                //Deleting deleted locators
                List<ElementLocator> LocatorrsToRemove = EI.Locators.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < LocatorrsToRemove.Count; i++)
                {
                    EI.Locators.Remove(LocatorrsToRemove[i]);
                }

                //Deleting deleted properties
                List<ControlProperty> PropertiesToRemove = EI.Properties.Where(x => ((POMElementProperty)x).DeltaElementProperty == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < PropertiesToRemove.Count; i++)
                {
                    EI.Properties.Remove(PropertiesToRemove[i]);
                }

                //Updating modified locators
                foreach (ElementLocator EL in EI.Locators)
                {
                    if (EL.DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        EL.LocateValue = EL.UpdatedValue;
                    }
                }

                //Updating modified properties
                foreach (ControlProperty CP in EI.Properties)
                {
                    if (((POMElementProperty)CP).DeltaElementProperty == ElementInfo.eDeltaStatus.Modified)
                    {
                        CP.Value = ((POMElementProperty)CP).UpdatedValue;
                    }
                }

                //Updating original element
                ReplaceOldElementWithNewOnce(EI);
            }

            //Performing replace to all equals because ElementGroup can be changed
            List<ElementInfo> EqualElementsToUpdate = mPOMAllCurrentElements.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Unchanged).ToList();
            foreach (ElementInfo EI in EqualElementsToUpdate)
            {
                //Updating original element
                ReplaceOldElementWithNewOnce(EI);
            }
        }

        private void ReplaceOldElementWithNewOnce(ElementInfo EI)
        {
            bool MappedOriginaly = false;
            ElementInfo CorrespondingMappedElementInfo = mOriginalPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();

            if (CorrespondingMappedElementInfo != null)
            {
                MappedOriginaly = true;
                int mappedElementIndex = mOriginalPOM.MappedUIElements.IndexOf(CorrespondingMappedElementInfo);
                mOriginalPOM.MappedUIElements.Remove(CorrespondingMappedElementInfo);
                InsertElementPerGroup(EI, mappedElementIndex, MappedOriginaly);
            }
            else
            {
                CorrespondingMappedElementInfo = mOriginalPOM.UnMappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
                int unMappedElementIndex = mOriginalPOM.UnMappedUIElements.IndexOf(CorrespondingMappedElementInfo);
                mOriginalPOM.UnMappedUIElements.Remove(CorrespondingMappedElementInfo);
                InsertElementPerGroup(EI, unMappedElementIndex, MappedOriginaly);
            }
        }

        private void InsertElementPerGroup(ElementInfo EI, int Index,bool MappedOriginaly)
        {
            if ((ApplicationPOMModel.eElementGroup)EI.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
            {
                if (MappedOriginaly)
                {
                    mOriginalPOM.MappedUIElements.Insert(Index, EI);
                }
                else
                {
                    mOriginalPOM.MappedUIElements.Add(EI);
                }

            }
            else
            {
                if (!MappedOriginaly)
                {
                    mOriginalPOM.UnMappedUIElements.Insert(Index, EI);
                }
                else
                {
                    mOriginalPOM.UnMappedUIElements.Add(EI);
                }
            }
        }
    }
}
