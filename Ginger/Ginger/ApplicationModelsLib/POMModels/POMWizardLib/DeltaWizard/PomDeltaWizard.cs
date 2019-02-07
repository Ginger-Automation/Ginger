using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    public class PomDeltaWizard : WizardBase
    {
        public ApplicationPOMModel mPOM;
        public ObservableList<ElementInfo> mPOMAllOriginalElements = new ObservableList<ElementInfo>();
        public ObservableList<DeltaElementInfo> mDeltaViewElements = new ObservableList<DeltaElementInfo>();
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
            mPOMAllOriginalElements = pom.GetUnifiedElementsList();
            mAgent = agent;

            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Comparison Status of Elements with Latest", Page: new PomDeltaElementComparePage());
        }

        public override void Finish()
        {
            //Updating selected elements
            List<DeltaElementInfo> elementsToUpdate = mDeltaViewElements.Where(x => x.IsSelected == true).ToList();
            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate)
            {
                //Add the New onces to the last of the list
                if (elementToUpdate.DeltaStatus == eDeltaStatus.New)
                {
                    if ((ApplicationPOMModel.eElementGroup)elementToUpdate.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        mPOM.MappedUIElements.Add(elementToUpdate.LatestMatchingElementInfo);
                    }
                    else
                    {
                        mPOM.UnMappedUIElements.Add(elementToUpdate.LatestMatchingElementInfo);
                    }
                    continue;
                }

                //ElementInfo originalElementInfo = null;
                //ObservableList<ElementInfo> originalGroup = null;
                //originalElementInfo = mPOM.MappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                //if (originalElementInfo != null)
                //{
                //    originalGroup = mPOM.MappedUIElements;
                //}
                //else
                //{
                //    originalElementInfo = mPOM.UnMappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                //    originalGroup = mPOM.UnMappedUIElements;
                //}
                //if (originalElementInfo == null || originalGroup == null)
                //{
                //    Reporter.ToLog(eLogLevel.ERROR, string.Format("POM Delta- failed to find the element '{0}' in POM original existing items", elementToUpdate.ElementName));
                //    continue;
                //}
                ObservableList<ElementInfo> originalGroup = null;
                if ((ApplicationPOMModel.eElementGroup)elementToUpdate.OriginalElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                {
                    originalGroup = mPOM.MappedUIElements;
                }
                else
                {
                    originalGroup = mPOM.UnMappedUIElements;
                }
                ObservableList<ElementInfo> selectedGroup = null;
                if ((ApplicationPOMModel.eElementGroup)elementToUpdate.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                {
                    selectedGroup = mPOM.MappedUIElements;
                }
                else
                {
                    selectedGroup = mPOM.UnMappedUIElements;
                }

                //Deleting deleted elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Deleted)
                {
                    originalGroup.Remove(elementToUpdate.OriginalElementInfo);
                    continue;
                }

                //Replacing Modified elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Changed)
                {
                    ElementInfo latestMatchingElement = elementToUpdate.LatestMatchingElementInfo;
                    //copy possible customized fields from original
                    latestMatchingElement.Guid = elementToUpdate.OriginalElementInfo.Guid;
                    latestMatchingElement.ElementName = elementToUpdate.OriginalElementInfo.ElementName;
                    latestMatchingElement.Description = elementToUpdate.OriginalElementInfo.Description;
                    //Locators customizations
                    foreach (ElementLocator originalLocator in elementToUpdate.OriginalElementInfo.Locators)
                    {
                        int originalLocatorIndex = originalLocatorIndex = elementToUpdate.OriginalElementInfo.Locators.IndexOf(originalLocator);

                        if (originalLocator.IsAutoLearned)
                        {
                            ElementLocator matchingLatestLocatorType = latestMatchingElement.Locators.Where(x => x.LocateBy == originalLocator.LocateBy).FirstOrDefault();
                            if (matchingLatestLocatorType != null)
                            {
                                matchingLatestLocatorType.Active = originalLocator.Active;
                                if (originalLocatorIndex <= elementToUpdate.OriginalElementInfo.Locators.Count)
                                {
                                    latestMatchingElement.Locators.Move(latestMatchingElement.Locators.IndexOf(matchingLatestLocatorType), originalLocatorIndex);
                                }
                            }
                        }
                        else
                        {
                            if (originalLocatorIndex <= elementToUpdate.OriginalElementInfo.Locators.Count)
                            {
                                latestMatchingElement.Locators.Insert(originalLocatorIndex, originalLocator);
                            }
                            else
                            {
                                latestMatchingElement.Locators.Add(originalLocator);
                            }
                        }
                    }
                    //enter it to POM elements instead of existing one
                    int originalItemIndex = originalGroup.IndexOf(elementToUpdate.OriginalElementInfo);
                    originalGroup.Remove(elementToUpdate.OriginalElementInfo);
                    if (originalItemIndex <= selectedGroup.Count)
                    {
                        selectedGroup.Insert(originalItemIndex, latestMatchingElement);
                    }
                    else
                    {
                        selectedGroup.Add(latestMatchingElement);
                    }                    
                }

                //TODO: to allow move of unchanged elements to diffrent group

            }
        }

    }
}
