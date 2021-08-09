#region License
/*
Copyright © 2014-2021 European Support Limited

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
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCoreNET.Application_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Application_Models.Execution.POM
{
    public class POMExecutionUtils
    {
        ActUIElement mAct = null;
        eExecutedFrom ExecutedFrom;
        public POMExecutionUtils(Act act)
        {
            mAct = (ActUIElement)act;

            var context = Context.GetAsContext(mAct.Context);
            ExecutedFrom = context.ExecutedFrom;
        }

        public POMExecutionUtils()
        {

        }

        private string[] PomElementGUID => mAct.ElementLocateValue.ToString().Split('_');


        public ApplicationPOMModel GetCurrentPOM()
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

        public ElementInfo GetCurrentPOMElementInfo()
        {
            Guid currentPOMElementInfoGUID = new Guid(PomElementGUID[1]);
            ElementInfo selectedPOMElementInfo = GetCurrentPOM().MappedUIElements.Where(z => z.Guid == currentPOMElementInfoGUID).FirstOrDefault();

            if (selectedPOMElementInfo == null)
            {
                mAct.ExInfo = string.Format("Failed to find the mapped element with GUID '{0}' inside the Page Objects Model", selectedPOMElementInfo.ToString());
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
            if (!IsSelfHealingConfigured())
            {
                return false;
            }

            var locatorPriotize = false;
            try
            {
                var locatorIndex = GetCurrentPOMElementInfo().Locators.ToList().FindIndex(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed);
                if (locatorIndex > 0)
                {
                    locatorPriotize = true;
                    GetCurrentPOMElementInfo().Locators.Move(locatorIndex, 0);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.INFO, "Error occured during self healing locator position", ex);
            }

            return locatorPriotize;
        }

        private bool IsSelfHealingConfigured()
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //when executing from runset
                var runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;

                if (runSetConfig != null)
                {
                    if (!runSetConfig.SelfHealingConfiguration.EnableSelfHealing || !runSetConfig.SelfHealingConfiguration.AutoUpdateApplicationModel)
                    {
                        return false;
                    }
                    return true;
                }
            }
            
            //when running from automate tab
            var selfHealingConfigAutomateTab = WorkSpace.Instance.AutomateTabSelfHealingConfiguration;
            if (!selfHealingConfigAutomateTab.EnableSelfHealing || !selfHealingConfigAutomateTab.AutoUpdateApplicationModel)
            {
                return false;
            }

            return true;
        }

        public ElementInfo AutoUpdateCurrentPOM(Common.InterfacesLib.IAgent currentAgent)
        {
            if (!IsSelfHealingConfigured())
            {
                return null;
            }

            var passedLocator = GetCurrentPOMElementInfo().Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed);


            if (passedLocator == null || passedLocator.Count() == 0 )
            {
                var pomLastUpdatedTimeSpan = (System.DateTime.Now - Convert.ToDateTime(GetCurrentPOMElementInfo().LastUpdatedTime)).TotalHours;

                if (pomLastUpdatedTimeSpan < 5)
                {
                    return null;
                }
            }

            var deltaElementInfos = GetUpdatedVirtulPOM(currentAgent);

            if (deltaElementInfos.Count > 0)
            {
                UpdateElementSelfHealingDetails(deltaElementInfos.ToList());
            }

            Guid currentPOMElementInfoGUID = new Guid(PomElementGUID[1]);
            var currentElementDelta = deltaElementInfos.Where(x => x.ElementInfo.Guid == currentPOMElementInfoGUID).FirstOrDefault();
            if (currentElementDelta == null || currentElementDelta.DeltaStatus == eDeltaStatus.Deleted)
            {
                return null;
            }
            else
            {
                return currentElementDelta.ElementInfo;
            }
        }

        private void UpdateElementSelfHealingDetails(List<DeltaElementInfo> deltaElementInfos)
        {
            foreach (var elementInfo in GetCurrentPOM().MappedUIElements)
            {
                try
                {
                    var deltaInfo = deltaElementInfos.Where(x => x.ElementInfo.Guid == elementInfo.Guid).FirstOrDefault();
                    if (deltaInfo != null)
                    {
                        if (deltaInfo.DeltaStatus == eDeltaStatus.Changed)
                        {
                            elementInfo.Properties = deltaInfo.ElementInfo.Properties;
                            elementInfo.Locators = deltaInfo.ElementInfo.Locators;
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
                    Reporter.ToLog(eLogLevel.DEBUG, "Error occured during self healing POM update operation..", ex);
                }
            }
        }

        private ObservableList<DeltaElementInfo> GetUpdatedVirtulPOM(Common.InterfacesLib.IAgent currentAgent)
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
                    return new ObservableList<DeltaElementInfo>();
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

                Reporter.ToLog(eLogLevel.INFO, "POM update process started during self healing operation..");
                this.GetCurrentPOM().StartDirtyTracking();

                pomDeltaUtils.LearnDelta().Wait();
                Reporter.ToLog(eLogLevel.INFO, "POM updated");
            }
            catch (Exception ex)
            {
            }
            finally
            {
                this.GetCurrentPOM().IsLearning = false;
            }

            return pomDeltaUtils.DeltaViewElements;
        }
    }
}
