#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace GingerCoreNET.Application_Models
{
    public class PomDeltaUtils
    {
        public ApplicationPOMModel POM = null;
        public ObservableList<ElementInfo> POMElementsCopy = new ObservableList<ElementInfo>();
        public ObservableList<DeltaElementInfo> DeltaViewElements = new ObservableList<DeltaElementInfo>();
        public ObservableList<ElementInfo> POMLatestElements = new ObservableList<ElementInfo>();
        public PomSetting pomSetting = null;
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
                    return ((IWindowExplorer)(((AgentOperations)Agent.AgentOperations).Driver));
                }
                else
                {
                    return null;
                }
            }
        }

        public string SpecificFramePath { get; set; }
        public List<eElementType> SelectedElementTypesList { get;  set; }

        public PomDeltaUtils(ApplicationPOMModel pom, Agent agent)
        {
            POM = pom;            
            Agent = agent;
            PomLearnUtils = new PomLearnUtils(pom, agent);
            POMLatestElements.CollectionChanged += ElementsListCollectionChanged;
        }

        public async Task LearnDelta()
        {
            try
            {
                IsLearning = true;
                mIWindowExplorerDriver.UnHighLightElements();
                ((DriverBase)((AgentOperations)Agent.AgentOperations).Driver).StopProcess = false;
                POMElementsCopy.Clear();
                DeltaViewElements.Clear();
                PomLearnUtils.PrepareLearningConfigurations();
                PomLearnUtils.LearnScreenShot();//this will set screen size to be same as in learning time
                PrepareCurrentPOMElementsData();
                ObservableList<POMPageMetaData> PomMetaData = new ObservableList<POMPageMetaData>();
                if (PomLearnUtils.LearnOnlyMappedElements)
                {
                    List<eElementType> selectedElementList = GetSelectedElementList();

                    await mIWindowExplorerDriver.GetVisibleControls(PomLearnUtils.pomSetting, POMLatestElements,PomMetaData);
                }
                else
                {
                   await mIWindowExplorerDriver.GetVisibleControls(PomLearnUtils.pomSetting,POMLatestElements);
                }
                SetUnidentifiedElementsDeltaDetails();
                DoEndOfRelearnElementsSorting();
            }
            finally
            {
                IsLearning = false;
            }            
        }

        private List<eElementType> GetSelectedElementList()
        {
            var uIElementList = new List<UIElementFilter>();
            uIElementList.AddRange(PomLearnUtils.AutoMapBasicElementTypesList.ToList());
            uIElementList.AddRange(PomLearnUtils.AutoMapAdvanceElementTypesList.ToList());

            var selectedElementList = uIElementList.Where(x => x.Selected).Select(y => y.ElementType).ToList();
            
            if (selectedElementList.Count == 0)
            {
                selectedElementList = SelectedElementTypesList;
            }

            return selectedElementList;
        }

        private List<string> GetRelativeXpathTemplateList()
        {
            var customRelXpathTemplateList = new List<string>();

            foreach (var item in POM.RelativeXpathTemplateList)
            {
                customRelXpathTemplateList.Add(item.Value);
            }

            return customRelXpathTemplateList;
        }
        public void StopLearning()
        {
            ((DriverBase)((AgentOperations)Agent.AgentOperations).Driver).StopProcess = true;
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
           
            //Task.Run(() =>
            //{
                mIWindowExplorerDriver.CollectOriginalElementsDataForDeltaCheck(POMElementsCopy);
            //});
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
            foreach (ElementLocator Flocator in elementInfo.FriendlyLocators)
            {
                DeltaElementLocator deltaFLocator = new DeltaElementLocator();
                deltaFLocator.ElementLocator = Flocator;
                deltaFLocator.ElementLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaFLocator.DeltaStatus = deltaStatus;
                newDeltaElement.FriendlyLocators.Add(deltaFLocator);
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

        public void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement)
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

                ElementLocator matchingExistingLocator = null;

                if (latestLocator.LocateBy == eLocateBy.ByRelXPath)
                {
                    matchingExistingLocator = existingElement.Locators.Where(x => x.LocateBy == latestLocator.LocateBy && x.LocateValue == latestLocator.LocateValue).FirstOrDefault();
                }
                else
                {
                    matchingExistingLocator = existingElement.Locators.Where(x => x.LocateBy == latestLocator.LocateBy).FirstOrDefault();
                }

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

            ////////------------------ Delta Friendly Locators
            foreach (ElementLocator latestFLocator in latestElement.FriendlyLocators)
            {
                latestFLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                DeltaElementLocator deltaFLocator = new DeltaElementLocator();
                latestFLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaFLocator.ElementLocator = latestFLocator;

                ElementLocator matchingExistingFLocator = null;

                if (latestFLocator.LocateBy == eLocateBy.ByRelXPath)
                {
                    matchingExistingFLocator = existingElement.FriendlyLocators.Where(x => x.LocateBy == latestFLocator.LocateBy && x.LocateValue == latestFLocator.LocateValue).FirstOrDefault();
                }
                else
                {
                    matchingExistingFLocator = existingElement.FriendlyLocators.Where(x => x.LocateBy == latestFLocator.LocateBy).FirstOrDefault();
                }

                if (matchingExistingFLocator != null)
                {
                    latestFLocator.Guid = matchingExistingFLocator.Guid;
                    if (matchingExistingFLocator.LocateBy == eLocateBy.ByXPath)
                    {
                        //fiting previous learned Xpath to latest structure to avoid false change indication
                        if (!matchingExistingFLocator.LocateValue.StartsWith("/"))
                        {
                            string updatedXpath = string.Empty;
                            string[] xpathVals = matchingExistingFLocator.LocateValue.Split('/');
                            for (int indx = 0; indx < xpathVals.Count(); indx++)
                            {
                                if (indx == 0)
                                {
                                    xpathVals[0] = xpathVals[0] + "[1]";
                                }
                                updatedXpath += "/" + xpathVals[indx];
                            }

                            matchingExistingFLocator.LocateValue = updatedXpath;
                        }
                    }
                    //compare value
                    if ((string.IsNullOrWhiteSpace(matchingExistingFLocator.LocateValue) && string.IsNullOrWhiteSpace(latestFLocator.LocateValue))
                        || matchingExistingFLocator.LocateValue.Equals(latestFLocator.LocateValue, StringComparison.OrdinalIgnoreCase))//Unchanged
                    {
                        deltaFLocator.DeltaStatus = eDeltaStatus.Unchanged;
                    }
                    else//Changed
                    {
                        deltaFLocator.DeltaStatus = eDeltaStatus.Changed;
                        deltaFLocator.DeltaExtraDetails = string.Format("Previous value was: '{0}'", matchingExistingFLocator.LocateValue);
                    }
                }
                else//new locator
                {
                    deltaFLocator.DeltaStatus = eDeltaStatus.Added;
                }
                matchedDeltaElement.FriendlyLocators.Add(deltaFLocator);
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
                for(int i = 0; i < existingElement.Locators.Count; i++)
                {
                    ElementLocator originalLocator = existingElement.Locators[i];
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

                for (int i = 0; i < existingElement.FriendlyLocators.Count; i++)
                {
                    ElementLocator originalFLocator = existingElement.FriendlyLocators[i];
                    ElementLocator latestFLocator = latestElement.FriendlyLocators.Where(x => x.Guid == originalFLocator.Guid).FirstOrDefault();

                    if (latestFLocator != null)
                    {
                        latestFLocator.Active = originalFLocator.Active;
                        int originalIndex = existingElement.FriendlyLocators.IndexOf(originalFLocator);
                        if (originalIndex <= latestElement.FriendlyLocators.Count)
                        {
                            latestElement.FriendlyLocators.Move(latestElement.FriendlyLocators.IndexOf(latestFLocator), originalIndex);
                            matchedDeltaElement.FriendlyLocators.Move(matchedDeltaElement.FriendlyLocators.IndexOf(matchedDeltaElement.FriendlyLocators.Where(x => x.ElementLocator == latestFLocator).First()), originalIndex);
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
                        || (matchingExistingProperty.Value != null &&  matchingExistingProperty.Value.Equals(latestProperty.Value, StringComparison.OrdinalIgnoreCase)))//Unchanged
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
            List<DeltaElementLocator> modifiedFriendlyLocatorsList = matchedDeltaElement.FriendlyLocators.Where(x => x.DeltaStatus == eDeltaStatus.Changed || x.DeltaStatus == eDeltaStatus.Deleted).ToList();
            List<DeltaControlProperty> modifiedPropertiesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus == eDeltaStatus.Changed || x.DeltaStatus == eDeltaStatus.Deleted).ToList();
            if (modifiedLocatorsList.Count > 0 || modifiedPropertiesList.Count > 0 || modifiedFriendlyLocatorsList.Count > 0)
            {
                matchedDeltaElement.DeltaStatus = eDeltaStatus.Changed;
                matchedDeltaElement.IsSelected = true;
                if (modifiedLocatorsList.Count > 0 && modifiedPropertiesList.Count > 0 && modifiedFriendlyLocatorsList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Locators,Friendly Locators & Properties changed";
                }
                else if (modifiedLocatorsList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Locators changed";
                }
                else if(modifiedFriendlyLocatorsList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Friendly Locators changed";
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
                List<DeltaElementLocator> minorFriednlyLocatorsChangesList = matchedDeltaElement.FriendlyLocators.Where(x => x.DeltaStatus == eDeltaStatus.Avoided || x.DeltaStatus == eDeltaStatus.Added || x.DeltaStatus == eDeltaStatus.Unknown).ToList();
                List<DeltaControlProperty> minorPropertiesChangesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus == eDeltaStatus.Avoided || x.DeltaStatus == eDeltaStatus.Added || x.DeltaStatus == eDeltaStatus.Unknown).ToList();
                if (minorLocatorsChangesList.Count > 0 || minorPropertiesChangesList.Count > 0 || minorFriednlyLocatorsChangesList.Count > 0 )
                {
                    matchedDeltaElement.DeltaExtraDetails = "Unimportant differences exists";
                }
            }

            DeltaViewElements.Add(matchedDeltaElement);
        }

        private void SetUnidentifiedElementsDeltaDetails()
        {
            List<ElementInfo> unidentifiedElements = POMElementsCopy.Where(x => DeltaViewElements.Where(y => y.ElementInfo != null && y.ElementInfo.Guid == x.Guid).FirstOrDefault() == null).ToList();
            
            var addedElements = DeltaViewElements.Where(d => d.DeltaStatus == eDeltaStatus.Added).ToList();

            foreach (ElementInfo deletedElement in unidentifiedElements)
            {
                if (deletedElement.ElementStatus == ElementInfo.eElementStatus.Failed)
                {
                    bool matchingElementFound = false;

                    DeltaElementInfo toRemoveAddedFoundItem = null;
                    foreach (var newElement in addedElements)
                    {
                        //var newElementLocdiff = newElement.Locators.ToDictionary(x => x.LocateBy, x => x.LocateValue);
                        //var oldElementLocdiff = deletedElement.Locators.ToDictionary(x => x.LocateBy, x => x.LocateValue);
                        
                        //bool comparisonResult = newElement.Locators(deletedElement, (x, y) => new { x, y }).All(z => Compare(z.x, z.y));

                        bool comparisonResult = deletedElement.Locators.All(x => newElement.Locators.Any(y => Compare(x, y)));

                        if (comparisonResult)
                        {
                            //check property and update
                            var newElementPropeties = newElement.Properties.ToDictionary(x => x.Name, x => x.Value);
                            var oldElementPropeties = deletedElement.Properties.ToDictionary(x => x.Name, x => x.Value);
                            var diffProperties = newElementPropeties.Except(oldElementPropeties);

                            // check if Parent iframe changed
                            var parentIFrame = diffProperties.Where(x => x.Key.Contains(ElementProperty.ParentIFrame)).FirstOrDefault();
                            
                            var deltaControlProp = CovertToDeltaControlProperty(deletedElement.Properties);
                            if (!string.IsNullOrEmpty(parentIFrame.Value))
                            {
                                foreach (var changedProprty in diffProperties.ToList())
                                {
                                    foreach (var existingProperty in deltaControlProp.Where(x => x.ElementProperty.Name == changedProprty.Key))
                                    {
                                        existingProperty.ElementProperty.Value = changedProprty.Value;
                                        existingProperty.DeltaStatus = eDeltaStatus.Avoided;
                                        existingProperty.DeltaExtraDetails = "Property changed";
                                    }
                                }
                            }
                            matchingElementFound = true;

                            var mathchedItemIndex = DeltaViewElements.IndexOf(DeltaViewElements.Where(x => x.ElementInfo.Guid.Equals(newElement.ElementInfo.Guid)).FirstOrDefault());
                            //update path of element
                            deletedElement.Path = newElement.ElementInfo.Path;

                            var item = ConvertElementToDelta(deletedElement, eDeltaStatus.Changed, deletedElement.ElementGroup, true, "Property Changed");
                            item.Properties = deltaControlProp;
                            if (mathchedItemIndex != -1)
                                DeltaViewElements[mathchedItemIndex] = item;

                            // add found newElment in removeitem 
                            toRemoveAddedFoundItem = newElement;
                            //element found and updated, so exit from loop
                            break;

                        }

                    }

                    if (toRemoveAddedFoundItem != null)
                    {
                        addedElements.Remove(toRemoveAddedFoundItem);
                    }

                    if (!matchingElementFound)
                    {
                        //Deleted
                        DeltaViewElements.Add(ConvertElementToDelta(deletedElement, eDeltaStatus.Deleted, deletedElement.ElementGroup, true, "Element not found on page"));
                    }

                }
                else
                {
                    //unknown
                    DeltaViewElements.Add(ConvertElementToDelta(deletedElement, eDeltaStatus.Unknown, deletedElement.ElementGroup, false, "Element exist on page but could not be compared"));
                }
            }

        }

        public static bool Compare(ElementLocator A, DeltaElementLocator B)
        {
            return A.LocateBy == B.LocateBy && A.LocateValue == B.LocateValue;
        }
        private ObservableList<DeltaControlProperty> CovertToDeltaControlProperty(ObservableList<ControlProperty> properties)
        {
            ObservableList<DeltaControlProperty> deltaControlProperties = new ObservableList<DeltaControlProperty>();
            foreach (ControlProperty property in properties)
            {
                deltaControlProperties.Add(new DeltaControlProperty() { ElementProperty = property,DeltaStatus= eDeltaStatus.Unchanged});
            }
            return deltaControlProperties;
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
            //selected elements
            var elementsToUpdate = DeltaViewElements.Where(x => x.IsSelected == true);

            MapDeletedElementWithNewAddedElement(elementsToUpdate.ToList());

            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate.ToList())
            {
                //Add the New onces to the last of the list
                if (elementToUpdate.DeltaStatus == eDeltaStatus.Added)
                {
                    elementToUpdate.ElementInfo.ParentGuid = POM.Guid;
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

        private void MapDeletedElementWithNewAddedElement(List<DeltaElementInfo> elementsToUpdate)
        {
            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate)
            {
                if (elementToUpdate.MappedElementInfo != null && elementToUpdate.DeltaStatus.Equals(eDeltaStatus.Deleted))
                {
                    var deltaElementToUpdateProp = DeltaViewElements.Where(x => x.IsSelected == true && x.DeltaStatus.Equals(eDeltaStatus.Added) && x.ElementInfo.Guid.ToString().Equals(elementToUpdate.MappedElementInfo)).FirstOrDefault();
                    if (deltaElementToUpdateProp != null)
                    {
                        elementToUpdate.ElementInfo.Properties = deltaElementToUpdateProp.ElementInfo.Properties;
                        elementToUpdate.ElementInfo.Locators = deltaElementToUpdateProp.ElementInfo.Locators;
                        elementToUpdate.ElementInfo.ElementType = deltaElementToUpdateProp.ElementInfo.ElementType;
                        elementToUpdate.DeltaStatus = eDeltaStatus.Changed;

                        var index = DeltaViewElements.IndexOf(deltaElementToUpdateProp);
                        DeltaViewElements.RemoveAt(index);
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

        public ObservableList<ElementInfo> GetElementInfoListFromDeltaElementInfo(ObservableList<DeltaElementInfo> deltaElementInfos)
        {
            ObservableList<ElementInfo> elementInfos = new ObservableList<ElementInfo>();
            foreach (var deltaElementInfo in deltaElementInfos)
            {
                elementInfos.Add(deltaElementInfo.ElementInfo);
            }

            return elementInfos;
        }

    }
}
