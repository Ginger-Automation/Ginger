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

        private ObservableList<ElementInfo> mCopiedUnienedList = new ObservableList<ElementInfo>();

        public ObservableList<ElementInfo> CopiedUnienedList
        {
            get
            {
                return mCopiedUnienedList;
            }
        }


        public PomRelearnWizard(ApplicationPOMModel POM, Agent agent)
        {
            mOriginalPOM = POM;
            mAgent = agent;
            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Comparison Status of Elements with Latest", Page: new POMDeltaWizardPage());
        }

        public override void Finish()
        {
            //Updating selected elements
            List<ElementInfo> ElementsToUpdate = CopiedUnienedList.Where(x => x.IsSelected == true).ToList();
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
                    mOriginalPOM.MappedUIElements.Remove(EI);
                    mOriginalPOM.UnMappedUIElements.Remove(EI);
                    continue;
                }

                //Deleting deleted locators
                List<ElementLocator> LocatorrsToRemove = EI.Locators.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < LocatorrsToRemove.Count; i++)
                {
                    EI.Locators.Remove(LocatorrsToRemove[i]);
                }

                //Deleting deleted properties
                List<ControlProperty> PropertiesToRemove = EI.Properties.Where(x => ((HTMLElementProperty)x).DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
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
                    if (((HTMLElementProperty)CP).DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        CP.Value = ((HTMLElementProperty)CP).UpdatedValue;
                    }
                }

                //Updating original element
                ReplaceOldElementWithNewOnce(EI);
            }

            //Performing replace to all equals because ElementGroup can be changed
            List<ElementInfo> EqualElementsToUpdate = CopiedUnienedList.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Unchanged).ToList();
            foreach (ElementInfo EI in EqualElementsToUpdate)
            {
                //Updating original element
                ReplaceOldElementWithNewOnce(EI);
            }

            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mOriginalPOM);
        }

        private void ReplaceOldElementWithNewOnce(ElementInfo EI)
        {
            ElementInfo CorrespondingMappedElementInfo = mOriginalPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
            int mappedElementIndex = mOriginalPOM.MappedUIElements.IndexOf(CorrespondingMappedElementInfo);

            ElementInfo CorrespondingUnMappedElementInfo = mOriginalPOM.UnMappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
            int unMappedElementIndex = mOriginalPOM.UnMappedUIElements.IndexOf(CorrespondingUnMappedElementInfo);

            mOriginalPOM.MappedUIElements.Remove(CorrespondingMappedElementInfo);
            mOriginalPOM.UnMappedUIElements.Remove(CorrespondingUnMappedElementInfo);

            if (EI.ElementGroup.ToString() == ApplicationPOMModel.eElementGroup.Mapped.ToString())
            {
                if (mappedElementIndex != -1)
                {
                    mOriginalPOM.MappedUIElements.Insert(mappedElementIndex, EI);
                }
                else
                {
                    mOriginalPOM.MappedUIElements.Add(EI);
                }
            }
            else
            {
                if (unMappedElementIndex != -1)
                {
                    mOriginalPOM.UnMappedUIElements.Insert(unMappedElementIndex, EI);
                }
                else
                {
                    mOriginalPOM.UnMappedUIElements.Add(EI);
                }
            }
        }
    }
}
