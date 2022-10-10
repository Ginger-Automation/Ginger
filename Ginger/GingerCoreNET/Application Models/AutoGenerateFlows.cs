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
            foreach (POMMetaData metaData in POM.ApplicationPOMMetaData)
            {
                Activity activity = new Activity();
                activity.Active = true;
                activity.IsAutoLearned = true;
                activity.ActivityName = metaData.Name;

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
                foreach (ElementMetaData elementMetaData in metaData.ElementsMetaData)//we can use orderby if needed
                {
                    ElementInfo foundElemntInfo = (ElementInfo)POM.MappedUIElements.Where(z => z.Guid == elementMetaData.ElementGuid).FirstOrDefault();
                    if (foundElemntInfo == null)
                    {
                        foundElemntInfo = (ElementInfo)POM.UnMappedUIElements.Where(z => z.Guid == elementMetaData.ElementGuid).FirstOrDefault();
                    }
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
