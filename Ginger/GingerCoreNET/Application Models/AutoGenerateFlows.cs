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

using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Plugin.Core;
using GingerCore.Actions;
using GingerCore.Drivers.Common;
using GingerCore.Platforms.PlatformsInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;
using GingerCore;

namespace Amdocs.Ginger.CoreNET.Application_Models
{
    public class AutoGenerateFlows
    {
        public static ObservableList<Activity> CreatePOMActivitiesFromMetadata(ApplicationPOMModel POM)
        {
            Reporter.ToLog(eLogLevel.INFO, "Started generating activities based on pom meta data.");
            ObservableList<Activity> activities = new ObservableList<Activity>();
            foreach (POMPageMetaData metaData in POM.ApplicationPOMMetaData)
            {
                Activity activity = new Activity();
                activity.Active = true;
                activity.IsAutoLearned = true;
                activity.ActivityName = metaData.Name;
                activity.POMMetaDataId = metaData.Guid;
                activity.TargetApplication = POM.TargetApplicationKey.ItemName;

                //add GoTo url action
                WebPlatform webPlatform = new WebPlatform();

                ElementActionCongifuration actConfigurations = null;
                actConfigurations = new ElementActionCongifuration()
                {
                    Description = "Go to Url - " + POM.Name,
                    Operation = "GotoURL",
                    ElementValue = POM.PageURL,
                    LocateBy = "NA"
                };
                ElementInfo einfo = new ElementInfo();
                einfo.ElementTypeEnum = eElementType.Iframe;
                Act gotoAction = (webPlatform as Amdocs.Ginger.CoreNET.IPlatformInfo).GetPlatformAction(einfo, actConfigurations);
                gotoAction.Active = true;
                activity.Acts.Add(gotoAction);

                //generate the action from found element
                foreach (ElementInfo foundElemntInfo in POM.MappedUIElements.Where(ele => ele.Properties.Any(p=>p.Name== ElementProperty.ParentFormId && p.Value == metaData.Guid.ToString())))                
                {
                    if (foundElemntInfo != null)
                    {
                        string elementVal = string.Empty;
                        if (foundElemntInfo.OptionalValuesObjectsList.Count > 0)
                        {
                            elementVal = Convert.ToString(foundElemntInfo.OptionalValuesObjectsList.Where(v => v.IsDefault).FirstOrDefault().Value);
                        }
                        actConfigurations = new ElementActionCongifuration
                        {
                            LocateBy = eLocateBy.POMElement,
                            LocateValue = foundElemntInfo.ParentGuid.ToString() + "_" + foundElemntInfo.Guid.ToString(),
                            ElementValue = elementVal,
                            AddPOMToAction = true,
                            POMGuid = foundElemntInfo.ParentGuid.ToString(),
                            ElementGuid = foundElemntInfo.Guid.ToString(),
                            LearnedElementInfo = foundElemntInfo,
                            Type = foundElemntInfo.ElementTypeEnum
                        };
                        Act pomAction = (webPlatform as Amdocs.Ginger.CoreNET.IPlatformInfo).GetPlatformAction(foundElemntInfo, actConfigurations);
                        pomAction.Active = true;
                        activity.Acts.Add(pomAction);
                    }
                }
                activities.Add(activity);//check activity has actions
            }
            Reporter.ToLog(eLogLevel.INFO, "Finished generating activities based on pom meta data.");
            return activities;
        }
    }
}
