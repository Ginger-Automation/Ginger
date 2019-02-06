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
                    else
                    {
                        mPOM.UnMappedUIElements.Add(elementToUpdate.LatestMatchingElementInfo);
                    }
                    continue;
                }

                ElementInfo originalElementInfo = null;
                ObservableList<ElementInfo> originalGroup = null;
                originalElementInfo = mPOM.MappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                if (originalElementInfo != null)
                {
                    originalGroup = mPOM.MappedUIElements;
                }
                else
                {
                    originalElementInfo = mPOM.UnMappedUIElements.Where(x => x.Guid == elementToUpdate.Guid).FirstOrDefault();
                    originalGroup = mPOM.UnMappedUIElements;
                }
                if (originalElementInfo == null || originalGroup == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("POM Delta- failed to find the element '{0}' in POM original existing items", elementToUpdate.ElementName));
                    continue;
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
                    latestMatchingElement.Guid = originalElementInfo.Guid;
                    latestMatchingElement.ElementName = originalElementInfo.ElementName;
                    latestMatchingElement.Description = originalElementInfo.Description;
                    //Locators customizations
                    foreach (ElementLocator originalLocator in originalElementInfo.Locators)
                    {
                        int originalLocatorIndex = originalLocatorIndex = originalElementInfo.Locators.IndexOf(originalLocator);

                        if (originalLocator.IsAutoLearned)
                        {
                            ElementLocator matchingLatestLocatorType = latestMatchingElement.Locators.Where(x => x.LocateBy == originalLocator.LocateBy).FirstOrDefault();
                            if (matchingLatestLocatorType != null)
                            {
                                matchingLatestLocatorType.Active = originalLocator.Active;
                                if (originalLocatorIndex <= originalElementInfo.Locators.Count)
                                {
                                    latestMatchingElement.Locators.Move(latestMatchingElement.Locators.IndexOf(matchingLatestLocatorType), originalLocatorIndex);
                                }
                            }
                        }
                        else
                        {
                            if (originalLocatorIndex <= originalElementInfo.Locators.Count)
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
                    int originalItemIndex = originalGroup.IndexOf(originalElementInfo);
                    originalGroup.Remove(originalElementInfo);
                    if ((ApplicationPOMModel.eElementGroup)elementToUpdate.ElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        if (originalItemIndex <= mPOM.MappedUIElements.Count)
                        {
                            mPOM.MappedUIElements.Insert(originalItemIndex, latestMatchingElement);
                        }
                        else
                        {
                            mPOM.MappedUIElements.Add(latestMatchingElement);
                        }
                    }
                    else
                    {
                        if (originalItemIndex <= mPOM.UnMappedUIElements.Count)
                        {
                            mPOM.UnMappedUIElements.Insert(originalItemIndex, latestMatchingElement);
                        }
                        else
                        {
                            mPOM.UnMappedUIElements.Add(latestMatchingElement);
                        }
                    }
                }

                //TODO: to allow move of unchanged elements to diffrent group

            }
        }

    }
}
