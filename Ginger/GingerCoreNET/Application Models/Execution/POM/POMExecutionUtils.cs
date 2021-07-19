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
        public POMExecutionUtils(Act act)
        {
            mAct = (ActUIElement)act;
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
            //when executing from runset
            var runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;
            if (runSetConfig != null && !runSetConfig.SelfHealingConfiguration.RunFromAutomateTab)
            {
                if (!runSetConfig.SelfHealingConfiguration.EnableSelfHealing || !runSetConfig.SelfHealingConfiguration.PrioritizePOMLocator)
                {
                    return false;
                }
            }
            //when running from automate tab
            var selfHealingConfigAutomateTab = WorkSpace.Instance.AutomateTabSelfHealingConfiguration;
            if (!selfHealingConfigAutomateTab.EnableSelfHealing || (!selfHealingConfigAutomateTab.RunFromAutomateTab && !selfHealingConfigAutomateTab.PrioritizePOMLocator))
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

        internal ElementInfo AutoUpdateCurrentPOM()
        {
            var passedLocator = GetCurrentPOMElementInfo().Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed);

            var pomLastUpdatedTimeSpan = (System.DateTime.Now -  GetCurrentPOM().RepositoryItemHeader.LastUpdate).TotalHours;
            if (passedLocator == null || passedLocator.Count() > 0 || pomLastUpdatedTimeSpan < 5 )
            {
                return null;
            }
            
            var deltaElementInfos = GetUpdatedVirtulPOM();
            Guid currentPOMElementInfoGUID = new Guid(PomElementGUID[1]);
            var currentElementDelta = deltaElementInfos.Where(x => x.ElementInfo.Guid == currentPOMElementInfoGUID).FirstOrDefault();
           
            if (currentElementDelta == null || currentElementDelta.DeltaStatus == eDeltaStatus.Deleted)
            {
                Reporter.ToLog(eLogLevel.INFO, "Element not found on page during self healing process..");
                return null;
            }
            else
            {
               return currentElementDelta.ElementInfo;
            }

        }

        private ObservableList<DeltaElementInfo> GetUpdatedVirtulPOM()
        {
            var agent = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerCore.Agent>().Where(x => x.Guid == this.GetCurrentPOM().LastUsedAgent && x.Status == GingerCore.Agent.eStatus.Running).FirstOrDefault();
            var pomDeltaUtils = new PomDeltaUtils(this.GetCurrentPOM(), agent);

            //set element type
            var elementList = this.GetCurrentPOM().MappedUIElements.Select(y => y.ElementTypeEnum).Distinct().ToList();
            pomDeltaUtils.PomLearnUtils.LearnOnlyMappedElements = true;
            pomDeltaUtils.PomLearnUtils.SelectedElementTypesList = elementList;

            Reporter.ToLog(eLogLevel.INFO, "POM update process started during self healing operation..");
            pomDeltaUtils.LearnDelta().Wait();
            Reporter.ToLog(eLogLevel.INFO, "POM updated");

            return pomDeltaUtils.DeltaViewElements;
        }
    }
}
