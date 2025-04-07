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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.Application_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.Application_Models.Execution.POM
{
    public class POMExecutionUtils
    {
        Act mAct = null;
        eExecutedFrom ExecutedFrom;
        private string[] PomElementGUID;// => mAct.ElementLocateValue.ToString().Split('_');
        public POMExecutionUtils(Act act, string elementLocateValue)
        {

            mAct = act;
            var context = Context.GetAsContext(mAct.Context);
            ExecutedFrom = context.ExecutedFrom;
            if (!string.IsNullOrEmpty(elementLocateValue))
            {
                PomElementGUID = elementLocateValue.ToString().Split('_');
            }

        }

        public POMExecutionUtils()
        {

        }

        public POMExecutionUtils(eExecutedFrom executedFrom, string elementLocateValue)
        {
            ExecutedFrom = executedFrom;
            PomElementGUID = elementLocateValue.ToString().Split('_');
        }



        public virtual ApplicationPOMModel GetCurrentPOM()
        {
            Guid selectedPOMGUID = new Guid(PomElementGUID[0]);
            ApplicationPOMModel currentPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);

            if (currentPOM == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element Page Objects Model with GUID '{0}'", selectedPOMGUID.ToString());
                return null;
            }
            return currentPOM;

        }

        public virtual ElementInfo GetCurrentPOMElementInfo(ePomElementCategory? category = null)
        {
            Guid currentPOMElementInfoGUID = new Guid(PomElementGUID[1]);
            ElementInfo selectedPOMElementInfo = GetCurrentPOM().MappedUIElements.FirstOrDefault(z => z.Guid == currentPOMElementInfoGUID);

            if (selectedPOMElementInfo == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element with GUID '{0}' inside the Page Objects Model", currentPOMElementInfoGUID.ToString());
                return null;
            }
            else
            {
                if (category != null)
                {
                    return FilterElementDetailsByCategory(selectedPOMElementInfo, category);
                }
            }

            return selectedPOMElementInfo;
        }

        public static ElementInfo FilterElementDetailsByCategory(ElementInfo originalElementInfo, ePomElementCategory? category)
        {
            //copy original element info for not impacting the original element info
            ElementInfo ElementInfoCopy = (ElementInfo)originalElementInfo.CreateCopy(setNewGUID: false, deepCopy: true);
            ElementInfoCopy.Properties = new ObservableList<ControlProperty>(originalElementInfo.Properties);
            ElementInfoCopy.Locators = new ObservableList<ElementLocator>(originalElementInfo.Locators);

            //pull only Properties and Locators which match to the Driver Category
            //foreach (var prop in selectedPOMElementInfo.Properties)
            for (int i = ElementInfoCopy.Properties.Count - 1; i >= 0; i--)
            {
                var prop = ElementInfoCopy.Properties[i];
                if (prop.Category != null && prop.Category != category)
                {
                    ElementInfoCopy.Properties.RemoveAt(i);
                }
            }

            for (int i = ElementInfoCopy.Locators.Count - 1; i >= 0; i--)
            {
                var locator = ElementInfoCopy.Locators[i];
                if (locator.Category != null && locator.Category != category)
                {
                    ElementInfoCopy.Locators.RemoveAt(i);
                }
            }

            return ElementInfoCopy;
        }

        public ElementInfo GetFriendlyElementInfo(Guid elementGuid)
        {
            ElementInfo selectedPOMElementInfo = GetCurrentPOM().MappedUIElements.FirstOrDefault(z => z.Guid == elementGuid);

            if (selectedPOMElementInfo == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element with GUID '{0}' inside the Page Objects Model", elementGuid.ToString());
                return null;
            }

            return selectedPOMElementInfo;
        }

        public void SetPOMProperties(Act elementAction, ElementInfo elementInfo, ElementActionCongifuration actConfig)
        {
            if (actConfig.AddPOMToAction)
            {
                elementAction.Description = actConfig.Operation + " - " + elementInfo.ElementName;
                PropertyInfo pLocateBy = elementAction.GetType().GetProperty(nameof(ActUIElement.ElementLocateBy));
                if (pLocateBy != null)
                {
                    if (pLocateBy.PropertyType.IsEnum)
                    {
                        pLocateBy.SetValue(elementAction, Enum.Parse(pLocateBy.PropertyType, nameof(eLocateBy.POMElement)));
                    }
                }

                PropertyInfo pLocateVal = elementAction.GetType().GetProperty(nameof(ActUIElement.ElementLocateValue));
                if (pLocateVal != null)
                {
                    pLocateVal.SetValue(elementAction, string.Format("{0}_{1}", actConfig.POMGuid, actConfig.ElementGuid));
                }

                PropertyInfo pElementType = elementAction.GetType().GetProperty(nameof(ActUIElement.ElementType));
                if (pElementType != null && pElementType.PropertyType.IsEnum)
                {
                    pElementType.SetValue(elementAction, ((ElementInfo)actConfig.LearnedElementInfo).ElementTypeEnum);
                }
            }
        }

        public bool PriotizeLocatorPosition()
        {
            if (!IsSelfHealingConfiguredForPriotizeLocatorPosition())
            {
                return false;
            }

            var locatorPriotize = false;
            try
            {
                var locatorIndex = GetCurrentPOMElementInfo().Locators.ToList().FindIndex(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed && x.LocateBy != eLocateBy.ByXPath && x.LocateBy != eLocateBy.ByTagName);
                if (locatorIndex > 0)
                {
                    locatorPriotize = true;
                    GetCurrentPOMElementInfo().Locators.Move(locatorIndex, 0);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.INFO, "Error occurred during self healing locator position", ex);
            }

            return locatorPriotize;
        }

        private bool IsSelfHealingConfiguredForPriotizeLocatorPosition()
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //when executing from runset
                var runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;

                if (runSetConfig != null && runSetConfig.SelfHealingConfiguration.EnableSelfHealing)
                {
                    if (runSetConfig.SelfHealingConfiguration.ReprioritizePOMLocators)
                    {
                        return true;
                    }
                }
            }
            else if (ExecutedFrom == eExecutedFrom.Automation)
            {
                //when running from automate tab
                var selfHealingConfigAutomateTab = WorkSpace.Instance.AutomateTabSelfHealingConfiguration;
                if (selfHealingConfigAutomateTab.EnableSelfHealing)
                {
                    if (selfHealingConfigAutomateTab.ReprioritizePOMLocators)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsSelfHealingConfiguredForAutoUpdateCurrentPOM(bool checkForceUpdate=false)
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //when executing from runset
                var runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;

                if (runSetConfig != null && runSetConfig.SelfHealingConfiguration.EnableSelfHealing)
                {
                    if (runSetConfig.SelfHealingConfiguration.AutoUpdateApplicationModel)
                    {
                        if (checkForceUpdate)
                        {
                            if (runSetConfig.SelfHealingConfiguration.ForceUpdateApplicationModel)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            else if (ExecutedFrom == eExecutedFrom.Automation)
            {
                //when running from automate tab
                var selfHealingConfigAutomateTab = WorkSpace.Instance.AutomateTabSelfHealingConfiguration;
                if (selfHealingConfigAutomateTab.EnableSelfHealing)
                {
                    if (selfHealingConfigAutomateTab.AutoUpdateApplicationModel)
                    {
                        if (checkForceUpdate)
                        {
                            if (selfHealingConfigAutomateTab.ForceUpdateApplicationModel)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }


            return false;
        }

        private bool IsSelfHealingConfiguredForForceAutoUpdateCurrentPOM()
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //when executing from runset
                var runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;

                if (runSetConfig.SelfHealingConfiguration.ForceUpdateApplicationModel)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (ExecutedFrom == eExecutedFrom.Automation)
            {
                //when running from automate tab
                var selfHealingConfigAutomateTab = WorkSpace.Instance.AutomateTabSelfHealingConfiguration;

                if (selfHealingConfigAutomateTab.ForceUpdateApplicationModel)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                        
            }


            return false;
        }

        public ElementInfo AutoUpdateCurrentPOM(Common.InterfacesLib.IAgent currentAgent,bool CheckForForceUpdate = false)
        {
            if (!IsSelfHealingConfiguredForAutoUpdateCurrentPOM())
            {
                return null;
            }

            if (CheckForForceUpdate)
            {
                if (!IsSelfHealingConfiguredForForceAutoUpdateCurrentPOM())
                {
                    return null;
                }

                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.AutoUpdatedPOMList.Any(x => x.Equals(this.GetCurrentPOM().Guid)))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Self healing operation skipped as the POM was already updated during the run{this.GetCurrentPOM().Guid.ToString()} {this.GetCurrentPOMElementInfo().ElementName}");
                    return null;
                }
            }

            var passedLocator = GetCurrentPOMElementInfo().Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed);


            if (passedLocator == null || !passedLocator.Any())
            {
                // Get start time stamp of RunSet or BusinessFlow
                DateTime? startTimeStamp = ExecutedFrom == eExecutedFrom.Run
                                             ? WorkSpace.Instance.RunsetExecutor.RunSetConfig?.StartTimeStamp
                                             : ((GingerCore.Agent)currentAgent).BusinessFlow?.StartTimeStamp;

                // Check if the POM was updated after the start time stamp, if yes, don't update the POM
                if (startTimeStamp.HasValue && Convert.ToDateTime(GetCurrentPOMElementInfo().LastUpdatedTime).ToUniversalTime() > startTimeStamp.Value)
                {
                    return null;
                }
            }

            var deltaElementInfos = GetUpdatedVirtulPOM(currentAgent,CheckForForceUpdate);

            if (deltaElementInfos.Count > 0)
            {
                UpdateElementSelfHealingDetails([.. deltaElementInfos]);
            }

            Guid currentPOMElementInfoGUID = new(PomElementGUID[1]);
            var currentElementDelta = deltaElementInfos.FirstOrDefault(x => x.ElementInfo.Guid == currentPOMElementInfoGUID);
            if (currentElementDelta == null || currentElementDelta.DeltaStatus == eDeltaStatus.Deleted)
            {
                mAct.ExInfo += "Element not found during self healing process.";
                return null;
            }
            else
            {
                return currentElementDelta.ElementInfo;
            }
        }
        /// <summary>
        /// Updates element details during self-healing, preserving locators with categories
        /// that are not present in the new data to maintain backward compatibility.
        /// </summary>
        /// <param name="deltaElementInfos">List of delta information for elements</param>
        private void UpdateElementSelfHealingDetails(List<DeltaElementInfo> deltaElementInfos)
        {
            foreach (var elementInfo in GetCurrentPOM().MappedUIElements)
            {
                try
                {
                    var deltaInfo = deltaElementInfos.FirstOrDefault(x => x.ElementInfo.Guid == elementInfo.Guid);
                    if (deltaInfo != null)
                    {
                        if (deltaInfo.DeltaStatus == eDeltaStatus.Changed)
                        {

                            //Merge Both Category Properties

                            elementInfo.Properties = CategoryMergingUtils.MergeByCategory(
                                                        elementInfo.Properties,
                                                        deltaInfo.ElementInfo.Properties,
                                                        p => p.Category);

                            //Merge Both Category Locators

                            elementInfo.Locators = CategoryMergingUtils.MergeByCategory(
                                                    elementInfo.Locators,
                                                    deltaInfo.ElementInfo.Locators,
                                                    l => l.Category);

                            elementInfo.LastUpdatedTime = DateTime.Now.ToString();
                            elementInfo.SelfHealingInfo = SelfHealingInfoEnum.ElementModified;
                        }
                        else if (deltaInfo.DeltaStatus == eDeltaStatus.Deleted)
                        {
                            elementInfo.LastUpdatedTime = DateTime.Now.ToString();
                            elementInfo.SelfHealingInfo = SelfHealingInfoEnum.ElementDeleted;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Error occurred during self healing POM update operation..", ex);
                }
            }
        }

        private ObservableList<DeltaElementInfo> GetUpdatedVirtulPOM(Common.InterfacesLib.IAgent currentAgent, bool CheckForForceUpdate = false)
        {
            var waitToCompleteLearnProcess = false;
            while (this.GetCurrentPOM().IsLearning)
            {
                waitToCompleteLearnProcess = true;
            }
            this.GetCurrentPOM().IsLearning = true;

            if (waitToCompleteLearnProcess)
            {
                var pomLastUpdatedTimeSpan = (DateTime.Now - Convert.ToDateTime(GetCurrentPOMElementInfo().LastUpdatedTime)).Minutes;
                if (pomLastUpdatedTimeSpan < 5)
                {
                    return [];
                }
            }

            GingerCore.Agent agent = (GingerCore.Agent)currentAgent;
            var pomDeltaUtils = new PomDeltaUtils(this.GetCurrentPOM(), agent);

            try
            {
                //set element type
                var elementList = this.GetCurrentPOM().MappedUIElements.Where(x => x.ElementTypeEnum != eElementType.Unknown).Select(y => y.ElementTypeEnum).Distinct().ToList();
                pomDeltaUtils.PomLearnUtils.LearnOnlyMappedElements = true;
                pomDeltaUtils.SelectedElementTypesList = elementList;
                pomDeltaUtils.PomLearnUtils.ElementLocatorsSettingsList = GingerCore.Platforms.PlatformsInfo.PlatformInfoBase.GetPlatformImpl(agent.Platform).GetLearningLocators();
                pomDeltaUtils.KeepOriginalLocatorsOrderAndActivation = true;
                pomDeltaUtils.PropertiesChangesToAvoid = DeltaControlProperty.ePropertiesChangesToAvoid.All;
                pomDeltaUtils.AcceptElementFoundByMatcher = true;

                mAct.ExInfo += DateTime.Now.ToString() + " Self healing operation attempting to auto update application model";
                this.GetCurrentPOM().StartDirtyTracking();

                pomDeltaUtils.LearnDelta().Wait();
                mAct.ExInfo += DateTime.Now + " Self healing operation application model was updated";
                if(CheckForForceUpdate)
                {
                    if(WorkSpace.Instance.RunsetExecutor.RunSetConfig?.AutoUpdatedPOMList != null)
                    {
                        if(!WorkSpace.Instance.RunsetExecutor.RunSetConfig.AutoUpdatedPOMList.Any(x=>x.Equals(this.GetCurrentPOM().Guid)))
                        {
                            WorkSpace.Instance.RunsetExecutor.RunSetConfig.AutoUpdatedPOMList.Add(this.GetCurrentPOM().Guid);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                mAct.ExInfo += DateTime.Now + $"Self healing operation failed to auto update application model{this.GetCurrentPOM().Guid}";
                Reporter.ToLog(eLogLevel.DEBUG, $"Self healing operation failed to auto update application model{this.GetCurrentPOM().Guid}", ex);
            }
            finally
            {
                this.GetCurrentPOM().IsLearning = false;
            }

            return pomDeltaUtils.DeltaViewElements;
        }

        public ElementInfo AutoForceUpdateCurrentPOM(Common.InterfacesLib.IAgent currentAgent,Act act)
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, $"Forcefully updating the application model based on the self-healing configuration before Execution");
                act.ExInfo += "Forcefully updating the application model based on the self-healing configuration before Execution";

                return AutoUpdateCurrentPOM(currentAgent, true);
            }
            catch (Exception ex)
            {
                mAct.ExInfo += DateTime.Now + " Auto force update POM operation failed to auto update application model";
                Reporter.ToLog(eLogLevel.DEBUG, "Auto force update POM operation failed to auto update application model", ex);
                return null;
            }
            
        }
    }
}
