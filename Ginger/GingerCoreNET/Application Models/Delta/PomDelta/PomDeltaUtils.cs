#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.CoreNET.NewSelfHealing;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
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
        public ObservableList<ElementInfo> POMElementsCopy = [];
        public ObservableList<DeltaElementInfo> DeltaViewElements = [];
        public ObservableList<ElementInfo> POMLatestElements = [];
        public PomSetting pomSetting = null;
        public bool IsLearning { get; set; }
        List<string> mVisualPropertiesList = ["X", "Y", "Height", "Width"];
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
        public List<eElementType> SelectedElementTypesList { get; set; }

        public bool AcceptElementFoundByMatcher { get; set; } = true;

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
                ((AgentOperations)Agent.AgentOperations).Driver.StopProcess = false;
                POMElementsCopy.Clear();
                DeltaViewElements.Clear();
                if (PomLearnUtils.AutoMapBasicElementTypesList.Count == 0)
                {
                    var elementList = PlatformInfoBase.GetPlatformImpl(ePlatformType.Web).GetUIElementFilterList();
                    PomLearnUtils.AutoMapBasicElementTypesList = elementList["Basic"];
                    PomLearnUtils.AutoMapAdvanceElementTypesList = elementList["Advanced"];
                }
                PomLearnUtils.PrepareLearningConfigurations();
                PomLearnUtils.LearnScreenShot();//this will set screen size to be same as in learning time
                PrepareCurrentPOMElementsData();
                ObservableList<POMPageMetaData> PomMetaData = [];
                if (PomLearnUtils.LearnOnlyMappedElements)
                {
                    List<eElementType> selectedElementList = GetSelectedElementList();

                    await mIWindowExplorerDriver.GetVisibleControls(PomLearnUtils.POM.PomSetting, POMLatestElements, PomMetaData);
                }
                else
                {
                    await mIWindowExplorerDriver.GetVisibleControls(PomLearnUtils.POM.PomSetting, POMLatestElements);
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

            foreach (var item in POM.PomSetting.RelativeXpathTemplateList)
            {
                customRelXpathTemplateList.Add(item.Value);
            }

            return customRelXpathTemplateList;
        }
        public void StopLearning()
        {
            ((AgentOperations)Agent.AgentOperations).Driver.StopProcess = true;
            IsLearning = false;
        }

        private void PrepareCurrentPOMElementsData()
        {
            //get original elements
            ObservableList<ElementInfo> AllCurrentOriginalPOMs = POM.GetUnifiedElementsList();
            //copy original so won't be effected
            foreach (ElementInfo originalElement in AllCurrentOriginalPOMs)
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
                    ElementInfo matchingOriginalElement = FindMatchingOldElementByDriver(latestElement, POMElementsCopy);
                    bool elementFoundWithoutMatcher = matchingOriginalElement != null;

                    bool usePropertyMatching = ShouldUsePropertyMatcherToFindOriginalElement();
                    bool useImageMatching = ShouldUseImageMatcherToFindOriginalElement();

                    string matchDetails = string.Empty;
                    if (usePropertyMatching && matchingOriginalElement == null)
                    {
                        ePlatformType platform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(POM.TargetApplicationKey);
                        matchingOriginalElement = FindMatchingOldElementByProperty(latestElement, POMElementsCopy, platform, out double maxMatchScore);
                        if (matchingOriginalElement != null)
                        {
                            Reporter.ToLog(eLogLevel.INFO, $"predicted original element({matchingOriginalElement.ElementName}) for new element({latestElement.ElementName}) by property with {maxMatchScore * 100}% match");
                            matchDetails = $"found by property ({maxMatchScore * 100}%)";
                        }
                    }
                    if (useImageMatching && matchingOriginalElement == null)
                    {
                        matchingOriginalElement = FindMatchingOldElementByImage(latestElement, POMElementsCopy, out double maxMatchScore);
                        if (matchingOriginalElement != null)
                        {
                            Reporter.ToLog(eLogLevel.INFO, $"predicted original element({matchingOriginalElement.ElementName}) for new element({latestElement.ElementName}) by image with {maxMatchScore * 100}% match");
                            matchDetails = $"found by image ({maxMatchScore * 100}%)";
                        }
                    }

                    //Set element details
                    PomLearnUtils.SetLearnedElementDetails(latestElement);

                    if (matchingOriginalElement == null)//New element
                    {
                        object groupToAddTo;
                        if (PomLearnUtils.SelectedElementTypesList.Any(x=>x.ElementType.Equals(latestElement.ElementTypeEnum)))
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Mapped;
                        }
                        else
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Unmapped;
                        }
                        DeltaViewElements.Add(ConvertElementToDelta(latestElement, eDeltaStatus.Added, groupToAddTo, true, "New element"));
                    }
                    else if (matchingOriginalElement != null && (elementFoundWithoutMatcher || AcceptElementFoundByMatcher))//Existing Element
                    {
                        SetMatchingElementDeltaDetails(matchingOriginalElement, latestElement, matchDetails);
                    }
                    else if (matchingOriginalElement != null)
                    {
                        object groupToAddTo;
                        if (PomLearnUtils.SelectedElementTypesList.Any(x => x.ElementType.Equals(latestElement.ElementTypeEnum)))
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Mapped;
                        }
                        else
                        {
                            groupToAddTo = ApplicationPOMModel.eElementGroup.Unmapped;
                        }
                        DeltaElementInfo delta = ConvertElementToDelta(latestElement, eDeltaStatus.Added, groupToAddTo, true, "New element");
                        delta.PredictedElementInfo = matchingOriginalElement;
                        DeltaViewElements.Add(delta);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("POM Delta- failed to compare new learned element '{0}' with existing elements", latestElement.ElementName), ex);
                }
            }
        }

        private void SuggestPredictedElementForDelta(DeltaElementInfo originalElementDelta, ElementInfo predictedLatestElement)
        {
            originalElementDelta.MappedElementInfoName = predictedLatestElement.ElementName;
            originalElementDelta.MappedElementInfo = predictedLatestElement.Guid.ToString();
            originalElementDelta.MappingElementStatus = DeltaElementInfo.eMappingStatus.ReplaceExistingElement;
        }

        private bool ShouldUsePropertyMatcherToFindOriginalElement()
        {
            //TODO: allow to override this property per POM level
            return WorkSpace.Instance.Solution.SelfHealingConfig.UsePropertyMatcher;
        }

        private bool ShouldUseImageMatcherToFindOriginalElement()
        {
            //TODO: allow to override this property per POM level
            return WorkSpace.Instance.Solution.SelfHealingConfig.UseImageMatcher;
        }

        private double GetPropertyMatcherAcceptableScore()
        {
            //TODO: allow to override this property per POM level
            return ((double)WorkSpace.Instance.Solution.SelfHealingConfig.PropertyMatcherAcceptableScore) / 100;
        }

        private double GetImageMatcherAcceptableScore()
        {
            //TODO: allow to override this property per POM level
            return ((double)WorkSpace.Instance.Solution.SelfHealingConfig.ImageMatcherAcceptableScore) / 100;
        }

        private ElementInfo FindMatchingOldElementByDriver(ElementInfo newElement, ObservableList<ElementInfo> oldElements)
        {
            return mIWindowExplorerDriver.GetMatchingElement(newElement, oldElements);
        }

        private ElementInfo FindMatchingOldElementByProperty(ElementInfo newElement, IEnumerable<ElementInfo> oldElements, ePlatformType platform, out double maxMatchScore)
        {
            double acceptableMatchScore = GetPropertyMatcherAcceptableScore();

            ElementInfo bestMatchingOldElement = null;
            maxMatchScore = 0;

            ElementPropertyMatcher elementPropertyMatcher;
            if (platform == ePlatformType.Mobile)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"using mobile property-matcher for finding match for new element({newElement.ElementName})");
                elementPropertyMatcher = new MobileElementPropertyMatcher();
            }
            else
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"using generic property-matcher for finding match for new element({newElement.ElementName})");
                elementPropertyMatcher = new();
            }

            foreach (ElementInfo oldElement in oldElements)
            {
                try
                {
                    ePomElementCategory? expectedCategory = newElement.Properties?.FirstOrDefault()?.Category;

                    double matchScore = elementPropertyMatcher.Match(expected: newElement, actual: oldElement, expectedCategory: expectedCategory);
                    if (matchScore >= acceptableMatchScore && matchScore > maxMatchScore)
                    {
                        maxMatchScore = matchScore;
                        bestMatchingOldElement = oldElement;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"error occurred while matching properties of new element({newElement.ElementName}) with old element({oldElement.ElementName})", ex);
                }
            }

            return bestMatchingOldElement;
        }

        private ElementInfo FindMatchingOldElementByImage(ElementInfo newElement, IEnumerable<ElementInfo> oldElements, out double maxMatchScore)
        {
            maxMatchScore = 0;

            double acceptableMatchScore = GetImageMatcherAcceptableScore();

            ElementInfo bestMatchingOldElement = null;

            ElementImageMatcher elementImageMatcher = new();
            foreach (ElementInfo oldElement in oldElements)
            {
                try
                {
                    double matchScore = elementImageMatcher.Match(expected: newElement, actual: oldElement);
                    if (matchScore >= acceptableMatchScore && matchScore > maxMatchScore)
                    {
                        maxMatchScore = matchScore;
                        bestMatchingOldElement = oldElement;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"error occurred while matching image of new element({newElement.ElementName}) with old element({oldElement.ElementName})", ex);
                }
            }

            return bestMatchingOldElement;
        }

        private DeltaElementInfo ConvertElementToDelta(ElementInfo elementInfo, eDeltaStatus deltaStatus, object group, bool isSelected, string deltaExtraDetails)
        {
            //copy element and convert it to Delta
            DeltaElementInfo newDeltaElement = new DeltaElementInfo
            {
                ElementInfo = elementInfo
            };

            elementInfo.ElementStatus = ElementInfo.eElementStatus.Unknown;
            newDeltaElement.DeltaStatus = deltaStatus;
            newDeltaElement.DeltaExtraDetails = deltaExtraDetails;
            newDeltaElement.SelectedElementGroup = group;
            newDeltaElement.IsSelected = isSelected;
            foreach (ElementLocator locator in elementInfo.Locators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator
                {
                    ElementLocator = locator
                };
                deltaLocator.ElementLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.DeltaStatus = deltaStatus;
                newDeltaElement.Locators.Add(deltaLocator);
            }
            foreach (ElementLocator Flocator in elementInfo.FriendlyLocators)
            {
                DeltaElementLocator deltaFLocator = new DeltaElementLocator
                {
                    ElementLocator = Flocator
                };
                deltaFLocator.ElementLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaFLocator.DeltaStatus = deltaStatus;
                newDeltaElement.FriendlyLocators.Add(deltaFLocator);
            }
            foreach (ControlProperty propery in elementInfo.Properties)
            {
                DeltaControlProperty deltaPropery = new DeltaControlProperty
                {
                    ElementProperty = propery,
                    DeltaStatus = deltaStatus
                };
                newDeltaElement.Properties.Add(deltaPropery);
            }
            return newDeltaElement;
        }

        public void SetMatchingElementDeltaDetails(ElementInfo existingElement, ElementInfo latestElement, string matchDetails = "")
        {
            ePomElementCategory? expectedCategory = latestElement.Properties.FirstOrDefault()?.Category;
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
                    matchingExistingLocator = existingElement.Locators.FirstOrDefault(x => x.LocateBy == latestLocator.LocateBy && x.LocateValue == latestLocator.LocateValue && x.Category != null && expectedCategory.HasValue && x.Category.Equals(expectedCategory));
                }
                else
                {
                    matchingExistingLocator = existingElement.Locators.FirstOrDefault(x => x.LocateBy == latestLocator.LocateBy && x.Category != null && expectedCategory.HasValue && x.Category.Equals(expectedCategory));
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
                            for (int indx = 0; indx < xpathVals.Length; indx++)
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
                    matchingExistingFLocator = existingElement.FriendlyLocators.FirstOrDefault(x => x.LocateBy == latestFLocator.LocateBy && x.LocateValue == latestFLocator.LocateValue && x.Category != null && expectedCategory.HasValue && x.Category.Equals(expectedCategory));
                }
                else
                {
                    matchingExistingFLocator = existingElement.FriendlyLocators.FirstOrDefault(x => x.LocateBy == latestFLocator.LocateBy && x.Category != null && expectedCategory.HasValue && x.Category.Equals(expectedCategory));
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
                            for (int indx = 0; indx < xpathVals.Length; indx++)
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
            List<ElementLocator> notLearnedLocators = existingElement.Locators.Where(x => latestElement.Locators.FirstOrDefault(y => y.Guid == x.Guid) == null).ToList();
            foreach (ElementLocator notLearnedLocator in notLearnedLocators)
            {
                DeltaElementLocator deltaLocator = new DeltaElementLocator();
                notLearnedLocator.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                deltaLocator.ElementLocator = notLearnedLocator;
                if (notLearnedLocator.IsAutoLearned == true && notLearnedLocator.Category != null && expectedCategory.HasValue && notLearnedLocator.Category.Equals(expectedCategory))//deleted
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
                        latestElement.Locators.Add(notLearnedLocator);
                    }
                }
                matchedDeltaElement.Locators.Add(deltaLocator);
            }
            if (KeepOriginalLocatorsOrderAndActivation == true)
            {
                for (int i = 0; i < existingElement.Locators.Count; i++)
                {
                    ElementLocator originalLocator = existingElement.Locators[i];
                    ElementLocator latestLocator = latestElement.Locators.FirstOrDefault(x => x.Guid == originalLocator.Guid);

                    if (latestLocator != null)
                    {
                        latestLocator.Active = originalLocator.Active;
                        int originalIndex = existingElement.Locators.IndexOf(originalLocator);
                        if (originalIndex < latestElement.Locators.Count)
                        {
                            latestElement.Locators.Move(latestElement.Locators.IndexOf(latestLocator), originalIndex);
                            matchedDeltaElement.Locators.Move(matchedDeltaElement.Locators.IndexOf(matchedDeltaElement.Locators.First(x => x.ElementLocator == latestLocator)), originalIndex);
                        }
                    }
                }

                for (int i = 0; i < existingElement.FriendlyLocators.Count; i++)
                {
                    ElementLocator originalFLocator = existingElement.FriendlyLocators[i];
                    ElementLocator latestFLocator = latestElement.FriendlyLocators.FirstOrDefault(x => x.Guid == originalFLocator.Guid);

                    if (latestFLocator != null)
                    {
                        latestFLocator.Active = originalFLocator.Active;
                        int originalIndex = existingElement.FriendlyLocators.IndexOf(originalFLocator);
                        if (originalIndex <= latestElement.FriendlyLocators.Count)
                        {
                            latestElement.FriendlyLocators.Move(latestElement.FriendlyLocators.IndexOf(latestFLocator), originalIndex);
                            matchedDeltaElement.FriendlyLocators.Move(matchedDeltaElement.FriendlyLocators.IndexOf(matchedDeltaElement.FriendlyLocators.First(x => x.ElementLocator == latestFLocator)), originalIndex);
                        }
                    }
                }
            }

            ////////--------------- Properties
            foreach (ControlProperty latestProperty in latestElement.Properties)
            {
                DeltaControlProperty deltaProperty = new DeltaControlProperty();
                ControlProperty matchingExistingProperty = existingElement.Properties.FirstOrDefault(x => x.Name == latestProperty.Name);
                if (matchingExistingProperty != null)
                {
                    latestProperty.Guid = matchingExistingProperty.Guid;
                    deltaProperty.ElementProperty = latestProperty;
                    if ((string.IsNullOrWhiteSpace(matchingExistingProperty.Value) == true && string.IsNullOrWhiteSpace(latestProperty.Value) == true)
                        || (matchingExistingProperty.Value != null && matchingExistingProperty.Value.Equals(latestProperty.Value, StringComparison.OrdinalIgnoreCase)))//Unchanged
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
            List<ControlProperty> deletedProperties = existingElement.Properties.Where(x => latestElement.Properties.FirstOrDefault(y => y.Name == x.Name) == null).ToList();
            foreach (ControlProperty deletedProperty in deletedProperties)
            {
                DeltaControlProperty deltaProp = new DeltaControlProperty
                {
                    ElementProperty = deletedProperty
                };
                if (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.None
                            || (PropertiesChangesToAvoid == DeltaControlProperty.ePropertiesChangesToAvoid.OnlySizeAndLocationProperties && mVisualPropertiesList.Contains(deletedProperty.Name) == false) && deletedProperty.Category != null && expectedCategory.HasValue && deletedProperty.Category.Equals(expectedCategory))
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
            List<DeltaElementLocator> modifiedLocatorsList = matchedDeltaElement.Locators.Where(x => x.DeltaStatus is eDeltaStatus.Changed or eDeltaStatus.Deleted).ToList();
            List<DeltaElementLocator> modifiedFriendlyLocatorsList = matchedDeltaElement.FriendlyLocators.Where(x => x.DeltaStatus is eDeltaStatus.Changed or eDeltaStatus.Deleted).ToList();
            List<DeltaControlProperty> modifiedPropertiesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus is eDeltaStatus.Changed or eDeltaStatus.Deleted).ToList();
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
                else if (modifiedFriendlyLocatorsList.Count > 0)
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
                List<DeltaElementLocator> minorLocatorsChangesList = matchedDeltaElement.Locators.Where(x => x.DeltaStatus is eDeltaStatus.Avoided or eDeltaStatus.Added or eDeltaStatus.Unknown).ToList();
                List<DeltaElementLocator> minorFriednlyLocatorsChangesList = matchedDeltaElement.FriendlyLocators.Where(x => x.DeltaStatus is eDeltaStatus.Avoided or eDeltaStatus.Added or eDeltaStatus.Unknown).ToList();
                List<DeltaControlProperty> minorPropertiesChangesList = matchedDeltaElement.Properties.Where(x => x.DeltaStatus is eDeltaStatus.Avoided or eDeltaStatus.Added or eDeltaStatus.Unknown).ToList();
                if (minorLocatorsChangesList.Count > 0 || minorPropertiesChangesList.Count > 0 || minorFriednlyLocatorsChangesList.Count > 0)
                {
                    matchedDeltaElement.DeltaExtraDetails = "Unimportant differences exists";
                }
            }

            if (!string.IsNullOrWhiteSpace(matchDetails))
            {
                if (!string.IsNullOrEmpty(matchedDeltaElement.DeltaExtraDetails))
                {
                    matchedDeltaElement.DeltaExtraDetails += ", ";
                }
                matchedDeltaElement.DeltaExtraDetails += matchDetails;
            }

            DeltaViewElements.Add(matchedDeltaElement);
        }

        private void SetUnidentifiedElementsDeltaDetails()
        {
            List<ElementInfo> unidentifiedElements = POMElementsCopy.Where(x => DeltaViewElements.FirstOrDefault(y => y.ElementInfo != null && y.ElementInfo.Guid == x.Guid) == null).ToList();

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
                            var parentIFrame = diffProperties.FirstOrDefault(x => x.Key.Contains(ElementProperty.ParentIFrame));

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

                            var mathchedItemIndex = DeltaViewElements.IndexOf(DeltaViewElements.FirstOrDefault(x => x.ElementInfo.Guid.Equals(newElement.ElementInfo.Guid)));
                            //update path of element
                            deletedElement.Path = newElement.ElementInfo.Path;

                            var item = ConvertElementToDelta(deletedElement, eDeltaStatus.Changed, deletedElement.ElementGroup, true, "Property Changed");
                            item.Properties = deltaControlProp;
                            if (mathchedItemIndex != -1)
                            {
                                DeltaViewElements[mathchedItemIndex] = item;
                            }

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
                        DeltaElementInfo delta = ConvertElementToDelta(deletedElement, eDeltaStatus.Deleted, deletedElement.ElementGroup, true, "Element not found on page");
                        ElementInfo predictedLatestElement = DeltaViewElements.FirstOrDefault(d => d.PredictedElementInfo == deletedElement)?.ElementInfo;
                        if (predictedLatestElement != null)
                        {
                            SuggestPredictedElementForDelta(delta, predictedLatestElement);
                        }
                        DeltaViewElements.Add(delta);
                    }

                }
                else
                {
                    //unknown
                    DeltaElementInfo delta = ConvertElementToDelta(deletedElement, eDeltaStatus.Unknown, deletedElement.ElementGroup, false, "Element exist on page but could not be compared");
                    ElementInfo predictedLatestElement = DeltaViewElements.FirstOrDefault(d => d.PredictedElementInfo == deletedElement)?.ElementInfo;
                    if (predictedLatestElement != null)
                    {
                        SuggestPredictedElementForDelta(delta, predictedLatestElement);
                        delta.DeltaStatus = eDeltaStatus.Deleted;
                        delta.IsSelected = true;
                    }
                    DeltaViewElements.Add(delta);
                }
            }

        }

        public static bool Compare(ElementLocator A, DeltaElementLocator B)
        {
            return A.LocateBy == B.LocateBy && A.LocateValue == B.LocateValue;
        }
        private ObservableList<DeltaControlProperty> CovertToDeltaControlProperty(ObservableList<ControlProperty> properties)
        {
            ObservableList<DeltaControlProperty> deltaControlProperties = [];
            foreach (ControlProperty property in properties)
            {
                deltaControlProperties.Add(new DeltaControlProperty() { ElementProperty = property, DeltaStatus = eDeltaStatus.Unchanged });
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

            List<List<DeltaElementInfo>> ElementsLists = [deletedMappedElements, modifiedMappedElements, newMappedElements, unknownMappedElements, deletedUnMappedElements, modifiedUnMappedElements, newUnMappedElements, unknownUnMappedElements, unchangedMapped, unchangedUnmapped];
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
                if (elementToUpdate.DeltaStatus is eDeltaStatus.Changed or eDeltaStatus.Unchanged)
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

                    ElementInfo originalElementInfo = originalGroup.First(x => x.Guid == elementToUpdate.ElementInfo.Guid);

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

        private static ObservableList<T> MergeByCategory<T>(
            ObservableList<T> existing,
            ObservableList<T> latest,
           ePomElementCategory mergeCategory,
            Func<T, ePomElementCategory?> categorySelector)
        {
            var itemsWithMissingCategory = existing
                .Where(x => !categorySelector(x).Equals(mergeCategory))
                .ToList();
            return [.. latest, .. itemsWithMissingCategory];
        }

        private void MapDeletedElementWithNewAddedElement(List<DeltaElementInfo> elementsToUpdate)
        {
            foreach (DeltaElementInfo elementToUpdate in elementsToUpdate)
            {
                if (elementToUpdate.MappedElementInfo != null && (elementToUpdate.DeltaStatus.Equals(eDeltaStatus.Deleted) || elementToUpdate.DeltaStatus.Equals(eDeltaStatus.Unknown)))
                {
                    var deltaElementToUpdateProp = DeltaViewElements.FirstOrDefault(x => x.IsSelected == true && x.DeltaStatus.Equals(eDeltaStatus.Added) && x.ElementInfo.Guid.ToString().Equals(elementToUpdate.MappedElementInfo));
                    if (deltaElementToUpdateProp != null)
                    {
                        ePomElementCategory mergeCategory = deltaElementToUpdateProp.ElementInfo.Properties.FirstOrDefault().Category.Value;
                        if (elementToUpdate.MappingElementStatus == DeltaElementInfo.eMappingStatus.ReplaceExistingElement)
                        {
                            if (mergeCategory != null)
                            {
                                deltaElementToUpdateProp.ElementInfo.Properties = MergeByCategory(
                                    elementToUpdate.ElementInfo.Properties,
                                    deltaElementToUpdateProp.ElementInfo.Properties,
                                    mergeCategory,
                                    p => p.Category);

                                deltaElementToUpdateProp.ElementInfo.Locators = MergeByCategory(
                                elementToUpdate.ElementInfo.Locators,
                                deltaElementToUpdateProp.ElementInfo.Locators,
                                mergeCategory,
                                l => l.Category);

                                deltaElementToUpdateProp.ElementInfo.FriendlyLocators = MergeByCategory(
                                elementToUpdate.ElementInfo.FriendlyLocators,
                                deltaElementToUpdateProp.ElementInfo.FriendlyLocators,
                                mergeCategory,
                                l => l.Category);
                            }


                            elementToUpdate.ElementInfo.Properties = deltaElementToUpdateProp.ElementInfo.Properties;
                            elementToUpdate.ElementInfo.Locators = deltaElementToUpdateProp.ElementInfo.Locators;
                            elementToUpdate.ElementInfo.FriendlyLocators = deltaElementToUpdateProp.ElementInfo.FriendlyLocators;
                            elementToUpdate.ElementInfo.ElementType = deltaElementToUpdateProp.ElementInfo.ElementType;
                            elementToUpdate.DeltaStatus = eDeltaStatus.Changed;
                        }
                        else if (elementToUpdate.MappingElementStatus == DeltaElementInfo.eMappingStatus.MergeExistingElement)
                        {
                            MergeElements(elementToUpdate.ElementInfo, deltaElementToUpdateProp.ElementInfo);
                            elementToUpdate.DeltaStatus = eDeltaStatus.Changed;
                        }

                        if (elementToUpdate.DeltaStatus == eDeltaStatus.Changed)
                        {
                            var index = DeltaViewElements.IndexOf(deltaElementToUpdateProp);
                            DeltaViewElements.RemoveAt(index);
                        }
                    }
                }
            }
        }

        public static void MergeElements(ElementInfo originalElement, ElementInfo secondElement)
        {
            //Merge Properties
            for (int i = secondElement.Properties.Count - 1; i >= 0; i--)
            {
                ControlProperty originalProp = originalElement.Properties.FirstOrDefault(x => x.Name == secondElement.Properties[i].Name && x.Category == secondElement.Properties[i].Category);
                if (originalProp == null)
                {
                    originalElement.Properties.Add(secondElement.Properties[i]);
                }
            }

            //Merge Locators
            for (int i = secondElement.Locators.Count - 1; i >= 0; i--)
            {
                ElementLocator originalLocator = originalElement.Locators.FirstOrDefault(x => x.LocateBy == secondElement.Locators[i].LocateBy && x.LocateValue == secondElement.Locators[i].LocateValue && x.Category == secondElement.Locators[i].Category && x.IsAutoLearned == true);
                if (originalLocator == null)
                {
                    originalElement.Locators.Add(secondElement.Locators[i]);
                }
            }
        }

        private int GetOriginalItemIndex(ObservableList<ElementInfo> group, ElementInfo copiedItem)
        {
            return group.IndexOf(group.First(x => x.Guid == copiedItem.Guid));
        }

        private ElementInfo GetOriginalItem(ObservableList<ElementInfo> group, ElementInfo copiedItem)
        {
            return group.First(x => x.Guid == copiedItem.Guid);
        }

        public ObservableList<ElementInfo> GetElementInfoListFromDeltaElementInfo(ObservableList<DeltaElementInfo> deltaElementInfos)
        {
            ObservableList<ElementInfo> elementInfos = [];
            foreach (var deltaElementInfo in deltaElementInfos)
            {
                elementInfos.Add(deltaElementInfo.ElementInfo);
            }

            return elementInfos;
        }

    }
}
