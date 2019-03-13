#region License
/*
Copyright © 2014-2019 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET.Application_Models
{
    public class PomDeltaUtils
    {
        public ApplicationPOMModel POM = null;
        public ObservableList<ElementInfo> POMElementsCopy = new ObservableList<ElementInfo>();
        public ObservableList<DeltaElementInfo> DeltaViewElements = new ObservableList<DeltaElementInfo>();
        public ObservableList<ElementInfo> POMLatestElements = new ObservableList<ElementInfo>();
        public bool IsLearning { get; set; }
        List<string> mVisualPropertiesList = new List<string> { "X", "Y", "Height", "Width"};        
        public DeltaControlProperty.ePropertiesChangesToAvoid PropertiesChangesToAvoid;
        public bool? KeepOriginalLocatorsOrderAndActivation = true;
        public PomLearnUtils PomLearnUtils = null;
        public Agent Agent = null;
        IWindowExplorer mIWindowExplorerDriver
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

        public PomDeltaUtils(ApplicationPOMModel pom, Agent agent)
        {
            POM = pom;            
            Agent = agent;
            PomLearnUtils = new PomLearnUtils(pom, agent);
            POMLatestElements.CollectionChanged += ElementsListCollectionChanged;
        }

        public void LearnDelta()
        {
            try
            {
                IsLearning = true;
                mIWindowExplorerDriver.UnHighLightElements();
                ((DriverBase)Agent.Driver).mStopProcess = false;
                POMElementsCopy.Clear();
                DeltaViewElements.Clear();
                PomLearnUtils.PrepareLearningConfigurations();
                PomLearnUtils.LearnScreenShot();//this will set screen size to be same as in learning time
                PrepareCurrentPOMElementsData();
                if (PomLearnUtils.LearnOnlyMappedElements)
                {
                    mIWindowExplorerDriver.GetVisibleControls(PomLearnUtils.AutoMapElementTypesList.Where(x => x.Selected).ToList().Select(y => y.ElementType).ToList(), POMLatestElements, true);
                }
                else
                {
                    mIWindowExplorerDriver.GetVisibleControls(null, POMLatestElements, true);
                }
                SetUnidentifiedElementsDeltaDetails();
                DoEndOfRelearnElementsSorting();
            }
            finally
            {
                IsLearning = false;
            }            
        }

        public void StopLearning()
        {            
            ((DriverBase)Agent.Driver).mStopProcess = true;
            IsLearning = false;
        }

        private void PrepareCurrentPOMElementsData()
        {
            //get original elements
            ObservableList<ElementInfo> AllCurrentOriginalPOMs = POM.GetUnifiedElementsList();
            //copy original so won't be effected
            foreach(ElementInfo originalElement in AllCurrentOriginalPOMs)
            {
                ElementInfo copiedElement = (ElementInfo)originalElement.CreateCopy(false);
                copiedElement.ElementGroup = originalElement.ElementGroup;
                POMElementsCopy.Add(copiedElement);
            }
            //try to locate them and pull real IWebElement for them for later comparison
            mIWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(POMElementsCopy);
        }

        private void ElementsListCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                ElementInfo latestElement = (ElementInfo)e.NewItems[0];
                try
                {
                    ElementInfo matchingOriginalElement = (ElementInfo)mIWindowExplorerDriver.GetMatchingElement(latestElement, POMElementsCopy);
                    //Set element details
                    PomLearnUtils.SetLearnedElementDetails(latestElement);

                    if (matchingOriginalElement == null)//New element
                    {                        
                        object groupToAddTo;
                        if (PomLearnUtils.SelectedElementTypesList.Contains(latestElement.ElementTypeEnum))
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Mapped;
                        }
                        else
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Unmapped;
                        }
                        DeltaViewElements.Add(ConvertElementToDelta(latestElement, eDeltaStatus.Added, groupToAddTo, true, "New element"));
                    }
                    else//Existing Element
                    {
                        SetMatchingElementDeltaDetails(matchingOriginalElement, latestElement);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("POM Delta- failed to compare new learned element '{0}' with existing elements", latestElement.ElementName), ex);
                }
            }
        }

        private DeltaElementInfo ConvertElementToDelta(ElementInfo elementInfo, eDeltaStatus deltaStatus, object group, bool isSelected, string deltaExtraDetails)
        {
            //copy element and convert it to Delta
            DeltaElementInfo newDeltaElement = new DeltaElementInfo();
            newDeltaElement.ElementInfo = elementInfo;

            elementInfo.ElementStatus = ElementInfo.eElementStatus.Unknown;
            newDeltaElement.DeltaStatus = deltaStatus;
            newDeltaElement.DeltaExtraDetails = deltaExtraDetails;
            newDeltaElement.SelectedElementGroup = group;
            newDeltaElement.IsSelected = isSelected;
            foreach (ElementLocator locator in elementInfo.Locators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                deltaLocator.ElementLocator = locator;
                deltaLocator.ElementLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.DeltaStatus = deltaStatus;
                newDeltaElement.Locators.Add(deltaLocator);
            }
            foreach (ControlProperty propery in elementInfo.Properties)
            {
                DeltaControlProperty deltaPropery = new DeltaControlProperty();
                deltaPropery.ElementProperty = propery;
                deltaPropery.DeltaStatus = deltaStatus;
                newDeltaElement.Properties.Add(deltaPropery);
            }
            return newDeltaElement;
        }

        private void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement)
        {
            DeltaElementInfo matchedDeltaElement = new DeltaElementInfo();            
            //copy possible customized fields from original
            latestElement.Guid = existingElement.Guid;
            latestElement.ElementName = existingElement.ElementName;
            latestElement.Description = existingElement.Description;
            latestElement.ElementGroup = existingElement.ElementGroup;
            if (existingElement.OptionalValuesObjectsList.Count > 0 && latestElement.OptionalValuesObjectsList.Count == 0)
            {
                latestElement.OptionalValuesObjectsList = existingElement.OptionalValuesObjectsList;
            }
            matchedDeltaElement.ElementInfo = latestElement;
            ////////------------------ Delta Locators
            foreach (ElementLocator latestLocator in latestElement.Locators)
            {
                latestLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                latestLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.ElementLocator = latestLocator;
                ElementLocator matchingExistingLocator = existingElement.Locators.Where(x => x.LocateBy == latestLocator.LocateBy).FirstOrDefault();
                if (matchingExistingLocator != null)
                {
                    latestLocator.Guid = matchingExistingLocator.Guid;
                    if (matchingExistingLocator.LocateBy == eLocateBy.ByXPath)
                    {
                        //fiting previous learned Xpath to latest structure to avoid false change indication
                        if (matchingExistingLocator.LocateValue.StartsWith("/") == false)
                        {
                            string updatedXpath = string.Empty;
                            string[] xpathVals = matchingExistingLocator.LocateValue.Split(new char[] { '/' });
                            for (int indx = 0; indx < xpathVals.Count(); indx++)
                            {
                                if (indx == 0)
                                {
                                    xpathVals[0] = xpathVals[0] + "[1]";
                                }
                                updatedXpath += "/" + xpathVals[indx];
                            }

                            matchingExistingLocator.LocateValue = updatedXpath;
                        }
                    }
                    //compare value
                    if ((string.IsNullOrWhiteSpace(matchingExistingLocator.LocateValue) == true && string.IsNullOrWhiteSpace(latestLocator.LocateValue) == true)
                        || matchingExistingLocator.LocateValue.Equals(latestLocator.LocateValue, StringComparison.OrdinalIgnoreCase))//Unchanged
                    {
                        deltaLocator.DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        deltaLocator.DeltaStatus = eDeltaStatus.Changed;
                        deltaLocator.DeltaExtraDetails = string.Format("Previous value was: '{0}'", matchingExistingLocator.LocateValue);
                    }
                }
                else//new locator
                {
                    deltaLocator.DeltaStatus = eDeltaStatus.Added;
                }
                matchedDeltaElement.Locators.Add(deltaLocator);
            }
            //not Learned Locators
            List<ElementLocator> notLearnedLocators = existingElement.Locators.Where(x => latestElement.Locators.Where(y => y.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ElementLocator notLearedLocator in notLearnedLocators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                notLearedLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.ElementLocator = notLearedLocator;
                if (notLearedLocator.IsAutoLearned == true)//deleted
                {
                    deltaLocator.DeltaStatus = eDeltaStatus.Deleted;
                    deltaLocator.DeltaExtraDetails = "Locator not exist on latest";                    
                }
                else//customized locator so avoid it
                {
                    deltaLocator.DeltaStatus = eDeltaStatus.Avoided;
                    deltaLocator.DeltaExtraDetails = "Customized locator not exist on latest";
                    if (KeepOriginalLocatorsOrderAndActivation == true)
                    {
                        latestElement.Locators.Add(notLearedLocator);
                    }
                }
                matchedDeltaElement.Locators.Add(deltaLocator);
            }
            if (KeepOriginalLocatorsOrderAndActivation == true)
            {
                foreach (ElementLocator originalLocator in existingElement.Locators)
                {
                    ElementLocator latestLocator = latestElement.Locators.Where(x => x.Guid == originalLocator.Guid).FirstOrDefault();

                    if (latestLocator != null)
                    {
                        latestLocator.Active = originalLocator.Active;
                        int originalIndex = existingElement.Locators.IndexOf(originalLocator);
                        if (originalIndex <= latestElement.Locators.Count)
                        {
                            latestElement.Locators.Move(latestElement.Locators.IndexOf(latestLocator), originalIndex);
                            matchedDeltaElement.Locators.Move(matchedDeltaElement.Locators.IndexOf(matchedDeltaElement.Locators.Where(x => x.ElementLocator == latestLocator).First()), originalIndex);
                        }
                    }
                }
            }

            ////////--------------- Properties
            foreach (ControlProperty latestProperty in latestElement.Properties)
            {
                DeltaControlProperty deltaProperty = new DeltaControlProperty();
                ControlProperty matchingExistingProperty = existingElement.Properties.Where(x => x.Name == latestProperty.Name).FirstOrDefault();
                if (matchingExistingProperty != null)
                {
                    latestProperty.Guid = matchingExistingProperty.Guid;
                    deltaProperty.ElementProperty = latestProperty;
                    if ((string.IsNullOrWhiteSpace(matchingExistingProperty.Value) == true && string.IsNullOrWhiteSpace(latestProperty.Value) == true) 
                        ||  matchingExistingProperty.Value.Equals(latestProperty.Value, StringComparison.OrdinalIgnoreCase))//Unchanged
                    {
                        deltaProperty.DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        if (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.None
                            || (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.OnlySizeAndLocationProperties && mVisualPropertiesList.Contains(deltaProperty.Name) == false))
                        {
                            deltaProperty.DeltaStatus = eDeltaStatus.Changed;
                            deltaProperty.DeltaExtraDetails = string.Format("Previous value was: '{0}'", matchingExistingProperty.Value);                            
                        }
                        else
                        {
                            deltaProperty.DeltaStatus = eDeltaStatus.Avoided;
                            deltaProperty.DeltaExtraDetails = string.Format("Previous value was: '{0}' but change was avoided", matchingExistingProperty.Value);
                        }                        
                    }
                }
                else//new Property
                {
                    deltaProperty.ElementProperty = latestProperty;
                    if (string.IsNullOrWhiteSpace(latestProperty.Value) == false)
                    {
                        deltaProperty.DeltaStatus = eDeltaStatus.Added;
                    }
                    else
                    {
                        deltaProperty.DeltaStatus = eDeltaStatus.Avoided;
                        deltaProperty.DeltaExtraDetails = "New property but value is empty so it was avoided";
                    }
                }
                matchedDeltaElement.Properties.Add(deltaProperty);
            }
            //deleted Properties
            List<ControlProperty> deletedProperties = existingElement.Properties.Where(x => latestElement.Properties.Where(y => y.Name == x.Name).FirstOrDefault() == null).ToList();
            foreach (ControlProperty deletedProperty in deletedProperties)
            {
                DeltaControlProperty deltaProp = new DeltaControlProperty();
                deltaProp.ElementProperty = deletedProperty;
                if (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.None
                            || (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.OnlySizeAndLocationProperties && mVisualPropertiesList.Contains(deletedProperty.Name) == false))
                {
                    deltaProp.DeltaStatus = eDeltaStatus.Deleted;
                    deltaProp.DeltaExtraDetails = "Property not exist on latest";
                }
                else
                {
                    deltaProp.DeltaStatus = eDeltaStatus.Avoided;
                    deltaProp.DeltaExtraDetails = "Property not exist on latest but avoided";
                }
                matchedDeltaElement.Properties.Add(deltaProp);
            }

            //------------ General Status set     
            List<DeltaElementLocator> modifiedLocatorsList = matchedDeltaElement.Locators.Where(x => x.DeltaStatus == eDeltaStatus.Changed || x.DeltaStatus == eDeltaStatus.Deleted).ToList();
            List<DeltaControlProperty> modifiedPropertiesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus == eDeltaStatus.Changed || x.DeltaStatus == eDeltaStatus.Deleted).ToList();
            if (modifiedLocatorsList.Count > 0 || modifiedPropertiesList.Count > 0)
            {
                matchedDeltaElement.DeltaStatus = eDeltaStatus.Changed;
                matchedDeltaElement.IsSelected = true;
                if (modifiedLocatorsList.Count > 0 && modifiedPropertiesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Locators & Properties changed";
                }
                else if (modifiedLocatorsList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Locators changed";
                }
                else if (modifiedPropertiesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Properties changed";
                }
            }
            else
            {
                matchedDeltaElement.DeltaStatus = eDeltaStatus.Unchanged;
                matchedDeltaElement.IsSelected = false;
                List<DeltaElementLocator> minorLocatorsChangesList = matchedDeltaElement.Locators.Where(x => x.DeltaStatus == eDeltaStatus.Avoided || x.DeltaStatus == eDeltaStatus.Added || x.DeltaStatus == eDeltaStatus.Unknown).ToList();
                List<DeltaControlProperty> minorPropertiesChangesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus == eDeltaStatus.Avoided || x.DeltaStatus == eDeltaStatus.Added || x.DeltaStatus == eDeltaStatus.Unknown).ToList();
                if (minorLocatorsChangesList.Count > 0 || minorPropertiesChangesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Unimportant differences exists";
                }
            }

            DeltaViewElements.Add(matchedDeltaElement);
        }

        private void SetUnidentifiedElementsDeltaDetails()
        {
            List<ElementInfo> unidentifiedElements = POMElementsCopy.Where(x => DeltaViewElements.Where(y => y.ElementInfo != null && y.ElementInfo.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ElementInfo unidentifiedElement in unidentifiedElements)
            {
                if (unidentifiedElement.ElementStatus == ElementInfo.eElementStatus.Failed)
                {  
                    //Deleted
                    DeltaViewElements.Add(ConvertElementToDelta(unidentifiedElement, eDeltaStatus.Deleted, unidentifiedElement.ElementGroup, true, "Element not found on page"));
                }
                else
                {
                    //unknown
                    DeltaViewElements.Add(ConvertElementToDelta(unidentifiedElement, eDeltaStatus.Unknown, unidentifiedElement.ElementGroup, false, "Element exist on page but could not be compared"));
                }
            }
        }

        private void DoEndOfRelearnElementsSorting()
        {
            List<DeltaElementInfo> deletedMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> modifiedMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> newMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Added && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> unknownMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unknown && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();

            List<DeltaElementInfo> deletedUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> modifiedUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> newUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Added && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> unknownUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unknown && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<DeltaElementInfo> unchangedMapped = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> unchangedUnmapped = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<List<DeltaElementInfo>> ElementsLists = new List<List<DeltaElementInfo>>() { deletedMappedElements, modifiedMappedElements, newMappedElements, unknownMappedElements, deletedUnMappedElements, modifiedUnMappedElements, newUnMappedElements, unknownUnMappedElements, unchangedMapped, unchangedUnmapped };
            DeltaViewElements.Clear();
            foreach (List<DeltaElementInfo> elementsList in ElementsLists)
            {
                foreach (DeltaElementInfo EI in elementsList)
                {
                    DeltaViewElements.Add(EI);
                }
            }
        }

        public void UpdateOriginalPom()
        {
            //Updating selected elements
            List<DeltaElementInfo> elementsToUpdate = DeltaViewElements.Where(x => x.IsSelected == true).ToList();
            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate)
            {
                //Add the New onces to the last of the list
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Added)
                {
                    if ((ApplicationPOMModel.eElementGroup)elementToUpdate.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        POM.MappedUIElements.Add(elementToUpdate.ElementInfo);
                    }
                    else
                    {
                        POM.UnMappedUIElements.Add(elementToUpdate.ElementInfo);
                    }
                    continue;
                }

                ObservableList<ElementInfo> originalGroup = null;
                if ((ApplicationPOMModel.eElementGroup)elementToUpdate.OriginalElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                {
                    originalGroup = POM.MappedUIElements;
                }
                else
                {
                    originalGroup = POM.UnMappedUIElements;
                }

                //Deleting deleted elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Deleted)
                {
                    originalGroup.Remove(GetOriginalItem(originalGroup, elementToUpdate.ElementInfo));
                    continue;
                }

                //Replacing Modified elements
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Changed || elementToUpdate.DeltaStatus == eDeltaStatus.Unchanged)
                {
                    ObservableList<ElementInfo> selectedGroup = null;
                    if ((ApplicationPOMModel.eElementGroup)elementToUpdate.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped)
                    {
                        selectedGroup = POM.MappedUIElements;
                    }
                    else
                    {
                        selectedGroup = POM.UnMappedUIElements;
                    }
                    
                    ElementInfo originalElementInfo = originalGroup.Where(x => x.Guid == elementToUpdate.ElementInfo.Guid).First();
                    
                    //enter it to POM elements instead of existing one
                    int originalItemIndex = GetOriginalItemIndex(originalGroup, originalElementInfo);
                    originalGroup.RemoveAt(originalItemIndex);
                    if (originalItemIndex <= selectedGroup.Count)
                    {
                        selectedGroup.Insert(originalItemIndex, elementToUpdate.ElementInfo);
                    }
                    else
                    {
                        selectedGroup.Add(elementToUpdate.ElementInfo);
                    }
                }
            }
        }

        private int GetOriginalItemIndex(ObservableList<ElementInfo> group, ElementInfo copiedItem)
        {
            return group.IndexOf(group.Where(x => x.Guid == copiedItem.Guid).First());
        }

        private ElementInfo GetOriginalItem(ObservableList<ElementInfo> group, ElementInfo copiedItem)
        {
            return group.Where(x => x.Guid == copiedItem.Guid).First();
        }

    }
}
