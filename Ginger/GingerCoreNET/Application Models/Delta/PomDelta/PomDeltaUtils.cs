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

                PrepareCurrentPOMElementsData();
                mIWindowExplorerDriver.GetVisibleControls(null, POMLatestElements, true);
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
                        DeltaViewElements.Add(ConvertElementToDelta(null, latestElement, latestElement, eDeltaStatus.Added, groupToAddTo, true, "New element"));
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

        private DeltaElementInfo ConvertElementToDelta(ElementInfo originalElement, ElementInfo latestElement, ElementInfo toShowElement, eDeltaStatus deltaStatus, object group, bool isSelected, string deltaExtraDetails)
        {
            //copy element and convert it to Delta
            DeltaElementInfo newDeltaElement = new DeltaElementInfo();
            newDeltaElement.OriginalElementInfo = originalElement;
            newDeltaElement.LatestMatchingElementInfo = latestElement;
            newDeltaElement.ElementInfoToShow = latestElement;

            toShowElement.ElementStatus = ElementInfo.eElementStatus.Unknown;
            newDeltaElement.DeltaStatus = deltaStatus;
            newDeltaElement.DeltaExtraDetails = deltaExtraDetails;
            newDeltaElement.SelectedElementGroup = group;
            newDeltaElement.IsSelected = isSelected;
            foreach (ElementLocator locator in toShowElement.Locators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                deltaLocator.OriginalElementLocator = locator;
                deltaLocator.OriginalElementLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.DeltaStatus = deltaStatus;
                newDeltaElement.Locators.Add(deltaLocator);
            }
            foreach (ControlProperty propery in toShowElement.Properties)
            {
                DeltaControlProperty deltaPropery = new DeltaControlProperty();
                deltaPropery.OriginalElementProperty = propery;
                deltaPropery.DeltaStatus = deltaStatus;
                newDeltaElement.Properties.Add(deltaPropery);
            }
            return newDeltaElement;
        }

        private void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement)
        {
            DeltaElementInfo matchedDeltaElement = new DeltaElementInfo();
            matchedDeltaElement.OriginalElementInfo = existingElement;
            matchedDeltaElement.LatestMatchingElementInfo = latestElement;
            matchedDeltaElement.ElementInfoToShow = latestElement;
            //copy possible customized fields from original
            latestElement.Guid = existingElement.Guid;
            latestElement.ElementName = existingElement.ElementName;
            latestElement.Description = existingElement.Description;
            ////////------------------ Locators
            foreach (ElementLocator latestLocator in latestElement.Locators)
            {
                latestLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                DeltaElementLocator deltaLocator = new DeltaElementLocator();

                ElementLocator matchingExistingLocator = existingElement.Locators.Where(x => x.IsAutoLearned == true && x.LocateBy == latestLocator.LocateBy).FirstOrDefault();
                if (matchingExistingLocator != null)
                {
                    matchingExistingLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                    deltaLocator.OriginalElementLocator = matchingExistingLocator;
                    deltaLocator.LatestMatchingElementLocator = latestLocator;
                    deltaLocator.ElementLocatorToShow = latestLocator;
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
                    deltaLocator.LatestMatchingElementLocator = latestLocator;
                    deltaLocator.ElementLocatorToShow = latestLocator;
                    deltaLocator.DeltaStatus = eDeltaStatus.Added;
                }
                matchedDeltaElement.Locators.Add(deltaLocator);
            }
            //deleted Locators
            List<ElementLocator> deletedLocators = existingElement.Locators.Where(x => x.IsAutoLearned == true && matchedDeltaElement.Locators.Where(y => y.OriginalElementLocator.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ElementLocator deletedlocator in deletedLocators)
            {
                deletedlocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                deltaLocator.OriginalElementLocator = deletedlocator;
                deltaLocator.ElementLocatorToShow = deletedlocator;
                deltaLocator.DeltaStatus = eDeltaStatus.Deleted;
                deltaLocator.DeltaExtraDetails = "Locator not exist on latest";
                matchedDeltaElement.Locators.Add(deltaLocator);
            }

            ////////--------------- Properties
            foreach (ControlProperty latestProperty in latestElement.Properties)
            {
                DeltaControlProperty deltaProperty = new DeltaControlProperty();
                ControlProperty matchingExistingProperty = existingElement.Properties.Where(x => x.Name == latestProperty.Name).FirstOrDefault();
                if (matchingExistingProperty != null)
                {
                    deltaProperty.OriginalElementProperty = matchingExistingProperty;
                    deltaProperty.LatestMatchingElementProperty = latestProperty;
                    deltaProperty.ElementPropertyToShow = latestProperty;
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
                    deltaProperty.LatestMatchingElementProperty = latestProperty;
                    deltaProperty.ElementPropertyToShow = latestProperty;
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
            List<ControlProperty> deletedProperties = existingElement.Properties.Where(x => matchedDeltaElement.Properties.Where(y => y.OriginalElementProperty.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ControlProperty deletedProperty in deletedProperties)
            {
                DeltaControlProperty deltaProp = new DeltaControlProperty();
                deltaProp.OriginalElementProperty = deletedProperty;
                deltaProp.ElementPropertyToShow = deletedProperty;
                deltaProp.DeltaStatus = eDeltaStatus.Deleted;
                deltaProp.DeltaExtraDetails = "Property not exist on latest";
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
            }

            DeltaViewElements.Add(matchedDeltaElement);
        }

        private void SetUnidentifiedElementsDeltaDetails()
        {
            List<ElementInfo> unidentifiedElements = POMElementsCopy.Where(x => DeltaViewElements.Where(y => y.OriginalElementInfo.Guid == x.Guid).FirstOrDefault() == null).ToList();
            foreach (ElementInfo unidentifiedElement in unidentifiedElements)
            {
                if (unidentifiedElement.ElementStatus == ElementInfo.eElementStatus.Failed)
                {  
                    //Deleted
                    DeltaViewElements.Add(ConvertElementToDelta(unidentifiedElement, null, unidentifiedElement, eDeltaStatus.Deleted, unidentifiedElement.ElementGroup, true, "Element not found on page"));
                }
                else
                {
                    //unknown
                    DeltaViewElements.Add(ConvertElementToDelta(unidentifiedElement, null, unidentifiedElement, eDeltaStatus.Unknown, unidentifiedElement.ElementGroup, true, "Element exist on page but could not be compared"));
                }
            }
        }

        private void DoEndOfRelearnElementsSorting()
        {
            List<DeltaElementInfo> deletedMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> modifiedMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> newMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Added && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();

            List<DeltaElementInfo> deletedUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Deleted && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> modifiedUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Changed && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();
            List<DeltaElementInfo> newUnMappedElements = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Added && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<DeltaElementInfo> unchangedMapped = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Mapped).ToList();
            List<DeltaElementInfo> unchangedUnmapped = DeltaViewElements.Where(x => x.DeltaStatus == eDeltaStatus.Unchanged && (ApplicationPOMModel.eElementGroup)x.SelectedElementGroup == ApplicationPOMModel.eElementGroup.Unmapped).ToList();

            List<List<DeltaElementInfo>> ElementsLists = new List<List<DeltaElementInfo>>() { deletedMappedElements, modifiedMappedElements, newMappedElements, deletedUnMappedElements, modifiedUnMappedElements, newUnMappedElements, unchangedMapped, unchangedUnmapped };
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
                        POM.MappedUIElements.Add(elementToUpdate.OriginalElementInfo);
                    }
                    else
                    {
                        POM.UnMappedUIElements.Add(elementToUpdate.OriginalElementInfo);
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
                    originalGroup.Remove(GetOriginalItem(originalGroup, elementToUpdate.OriginalElementInfo));
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

                    ElementInfo latestMatchingElement = elementToUpdate.LatestMatchingElementInfo;
                    ////copy possible customized fields from original
                    //latestMatchingElement.Guid = elementToUpdate.OriginalElementInfo.Guid;
                    //latestMatchingElement.ElementName = elementToUpdate.OriginalElementInfo.ElementName;
                    //latestMatchingElement.Description = elementToUpdate.OriginalElementInfo.Description;
                    if (elementToUpdate.OriginalElementInfo.OptionalValuesObjectsList.Count>0 && latestMatchingElement.OptionalValuesObjectsList.Count == 0)
                    {
                        latestMatchingElement.OptionalValuesObjectsList = elementToUpdate.OriginalElementInfo.OptionalValuesObjectsList;
                    }
                    //Locators customizations
                    foreach (ElementLocator originalLocator in elementToUpdate.OriginalElementInfo.Locators)
                    {
                        int originalLocatorIndex = elementToUpdate.OriginalElementInfo.Locators.IndexOf(originalLocator);

                        if (originalLocator.IsAutoLearned)
                        {
                            if (KeepOriginalLocatorsOrderAndActivation == true)
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
                        }
                        else
                        {
                            if (KeepOriginalLocatorsOrderAndActivation == true && originalLocatorIndex <= elementToUpdate.OriginalElementInfo.Locators.Count)
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
                    int originalItemIndex = GetOriginalItemIndex(originalGroup, elementToUpdate.OriginalElementInfo);
                    originalGroup.RemoveAt(originalItemIndex);
                    if (originalItemIndex <= selectedGroup.Count)
                    {
                        selectedGroup.Insert(originalItemIndex, latestMatchingElement);
                    }
                    else
                    {
                        selectedGroup.Add(latestMatchingElement);
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
