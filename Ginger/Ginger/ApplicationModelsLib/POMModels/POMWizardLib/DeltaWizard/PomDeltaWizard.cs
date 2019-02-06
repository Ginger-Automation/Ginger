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
    public class PomDeltaWizard : WizardBase
    {
        public ApplicationPOMModel mPOM;
        public ObservableList<ElementInfo> mPOMCurrentElements = new ObservableList<ElementInfo>();
        public ObservableList<ElementInfo> mPOMLatestElements = new ObservableList<ElementInfo>();

        public override string Title { get { return "POM Elements Update Wizard"; } }

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

        public PomDeltaWizard(ApplicationPOMModel pom, Agent agent)
        {
            mPOM = pom;
            mAgent = agent;
            GetUnifiedPomElementsListForDeltaUse();
            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Comparison Status of Elements with Latest", Page: new PomDeltaElementComparePage());
        }


        public void GetUnifiedPomElementsListForDeltaUse()
        {
            mPOMCurrentElements.Clear();
            //TODO: check if should be done using Parallel.ForEach
            foreach (ElementInfo mappedElement in mPOM.MappedUIElements)
            {
                DeltaElementInfo deltaElement = ConvertElementInfoToDelta(mappedElement);
                deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Mapped;
                mPOMCurrentElements.Add(deltaElement);
            }
            foreach (ElementInfo unmappedElement in mPOM.UnMappedUIElements)
            {
                DeltaElementInfo deltaElement = ConvertElementInfoToDelta(unmappedElement);
                deltaElement.ElementGroup = ApplicationPOMModel.eElementGroup.Unmapped;
                mPOMCurrentElements.Add(deltaElement);
            }
        }

        private DeltaElementInfo ConvertElementInfoToDelta(ElementInfo element)
        {
            //copy element and convert it to Delta
            DeltaElementInfo deltaElement = (DeltaElementInfo)element.CreateCopy(false);//keeping original GUI            
            
            //convert Locators to Delta
            List<DeltaElementLocator> deltaLocators = deltaElement.Locators.Cast<DeltaElementLocator>().ToList();
            deltaElement.Locators.Clear();
            foreach (DeltaElementLocator deltaLocator in deltaLocators)
            {
                deltaElement.Locators.Add(deltaLocator);
            }

            //convert properties to Delta
            List<DeltaControlProperty> deltaProperties = deltaElement.Properties.Cast<DeltaControlProperty>().ToList();
            deltaElement.Properties.Clear();
            foreach (DeltaControlProperty deltaPropery in deltaProperties)
            {
                deltaElement.Properties.Add(deltaPropery);
            }

            return deltaElement;
        }


        public override void Finish()
        {
            //Updating selected elements
            List<ElementInfo> elementsToUpdate = mPOMCurrentElements.Where(x => x.IsSelected == true).ToList();
            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate)
            {
                //Add the New onces to the last of the list
                if (elementToUpdate.DeltaStatus == eDeltaStatus.New)
                {
                    if ((ApplicationPOMModel.eElementGroup)elementToUpdate.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        mPOM.MappedUIElements.Add(elementToUpdate.LatestMatchingElementInfo);
                    }
                    else if ((ApplicationPOMModel.eElementGroup)elementToUpdate.ElementGroup == ApplicationPOMModel.eElementGroup.Unmapped)
                    {
                        mPOM.UnMappedUIElements.Add(elementToUpdate.LatestMatchingElementInfo);
                    }
                    continue;
                }

                ObservableList<ElementInfo> originalGroup;
                ElementInfo originalElementInfo = mPOM.MappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                if (originalElementInfo != null)
                {
                    originalGroup = mPOM.MappedUIElements;
                }
                else
                {
                    originalElementInfo = mPOM.UnMappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                    originalGroup = mPOM.UnMappedUIElements;
                }

                //Deleting deleted elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Deleted)
                {
                    originalGroup.Remove(originalElementInfo);
                    continue;
                }

                //Replacing Modified elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Changed)
                {                    
                    ElementInfo latestMatchingElement = elementToUpdate.LatestMatchingElementInfo;
                    //copy possible customized fields from original
                    latestMatchingElement.ElementName = originalElementInfo.ElementName;

                }


                //Deleting deleted locators
                List<ElementLocator> LocatorrsToRemove = elementToUpdate.Locators.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < LocatorrsToRemove.Count; i++)
                {
                    elementToUpdate.Locators.Remove(LocatorrsToRemove[i]);
                }

                //Deleting deleted properties
                List<ControlProperty> PropertiesToRemove = elementToUpdate.Properties.Where(x => ((DeltaControlProperty)x).DeltaStatus == ElementInfo.eDeltaStatus.Deleted).ToList();
                for (int i = 0; i < PropertiesToRemove.Count; i++)
                {
                    elementToUpdate.Properties.Remove(PropertiesToRemove[i]);
                }

                //Updating modified locators
                foreach (ElementLocator EL in elementToUpdate.Locators)
                {
                    if (EL.DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        EL.LocateValue = EL.UpdatedValue;
                    }
                }

                //Updating modified properties
                foreach (ControlProperty CP in elementToUpdate.Properties)
                {
                    if (((DeltaControlProperty)CP).DeltaStatus == ElementInfo.eDeltaStatus.Modified)
                    {
                        CP.Value = ((DeltaControlProperty)CP).UpdatedValue;
                    }
                }

                //Updating original element
                ReplaceOldElementWithNewOnce(elementToUpdate);
            }

            //Performing replace to all equals because ElementGroup can be changed
            List<ElementInfo> EqualElementsToUpdate = mPOMCurrentElements.Where(x => x.DeltaStatus == ElementInfo.eDeltaStatus.Unchanged).ToList();
            foreach (ElementInfo EI in EqualElementsToUpdate)
            {
                //Updating original element
                ReplaceOldElementWithNewOnce(EI);
            }
        }

        private void ReplaceOldElementWithNewOnce(ElementInfo EI)
        {
            bool MappedOriginaly = false;
            ElementInfo CorrespondingMappedElementInfo = mPOM.MappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();

            if (CorrespondingMappedElementInfo != null)
            {
                MappedOriginaly = true;
                int mappedElementIndex = mPOM.MappedUIElements.IndexOf(CorrespondingMappedElementInfo);
                mPOM.MappedUIElements.Remove(CorrespondingMappedElementInfo);
                InsertElementPerGroup(EI, mappedElementIndex, MappedOriginaly);
            }
            else
            {
                CorrespondingMappedElementInfo = mPOM.UnMappedUIElements.Where(x => x.Guid == EI.Guid).FirstOrDefault();
                int unMappedElementIndex = mPOM.UnMappedUIElements.IndexOf(CorrespondingMappedElementInfo);
                mPOM.UnMappedUIElements.Remove(CorrespondingMappedElementInfo);
                InsertElementPerGroup(EI, unMappedElementIndex, MappedOriginaly);
            }
        }

        private void InsertElementPerGroup(ElementInfo EI, int Index,bool MappedOriginaly)
        {
            if ((ApplicationPOMModel.eElementGroup)EI.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
            {
                if (MappedOriginaly)
                {
                    mPOM.MappedUIElements.Insert(Index, EI);
                }
                else
                {
                    mPOM.MappedUIElements.Add(EI);
                }

            }
            else
            {
                if (!MappedOriginaly)
                {
                    mPOM.UnMappedUIElements.Insert(Index, EI);
                }
                else
                {
                    mPOM.UnMappedUIElements.Add(EI);
                }
            }
        }
    }
}
