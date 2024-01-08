using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActivityXMLSerializer
    {
        public ActivityXMLSerializer() { }

        public XmlElement Serialize(Activity activity, XmlDocument xmlDocument)
        {
            XmlElement activityElement = xmlDocument.CreateElement(nameof(Activity));

            if(activity.ActionRunOption != null)
                activityElement.SetAttribute(nameof(Activity.ActionRunOption), activity.ActionRunOption.ToString());

            activityElement.SetAttribute(nameof(Activity.Active), activity.Active.ToString());

            if(!string.IsNullOrEmpty(activity.ActivitiesGroupID))
                activityElement.SetAttribute(nameof(Activity.ActivitiesGroupID), activity.ActivitiesGroupID);

            if(!string.IsNullOrEmpty(activity.ActivityName))
                activityElement.SetAttribute(nameof(Activity.ActivityName), activity.ActivityName);
            
            if(activity.AutomationStatus != null)
                activityElement.SetAttribute(nameof(Activity.AutomationStatus), activity.AutomationStatus.ToString());
            
            if(!string.IsNullOrEmpty(activity.Description))
                activityElement.SetAttribute(nameof(Activity.Description), activity.Description);
            
            activityElement.SetAttribute(nameof(Activity.ErrorHandlerMappingType), activity.ErrorHandlerMappingType.ToString());
            
            activityElement.SetAttribute(nameof(Activity.Guid), activity.Guid.ToString());
            
            activityElement.SetAttribute(nameof(Activity.ParentGuid), activity.ParentGuid.ToString());
            
            if(!string.IsNullOrEmpty(activity.PercentAutomated))
                activityElement.SetAttribute(nameof(Activity.PercentAutomated), activity.PercentAutomated);
            
            activityElement.SetAttribute(nameof(Activity.POMMetaDataId), activity.POMMetaDataId.ToString());
            
            if(!string.IsNullOrEmpty(activity.TargetApplication))
                activityElement.SetAttribute(nameof(Activity.TargetApplication), activity.TargetApplication);
            
            activityElement.SetAttribute(nameof(Activity.Type), activity.Type.ToString());

            if (activity.Acts.Any())
            {
                XmlElement actsElement = xmlDocument.CreateElement(nameof(Activity.Acts));
                ActXMLSerializer actXMLSerializer = new();
                foreach (Act act in activity.Acts)
                {
                    XmlElement actElement = actXMLSerializer.Serialize(act, xmlDocument);
                    actsElement.AppendChild(actElement);
                }
                activityElement.AppendChild(actsElement);
            }

            return activityElement;
        }

        public Activity Deserialize(XmlElement activityElement)
        {
            Activity activity = new();

            foreach(XmlAttribute attribute in activityElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(Activity.ActionRunOption)))
                    activity.ActionRunOption = Enum.Parse<eActionRunOption>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.Active)))
                    activity.Active = bool.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.ActivitiesGroupID)))
                    activity.ActivitiesGroupID = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(Activity.ActivityName)))
                    activity.ActivityName = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(Activity.AutomationStatus)))
                    activity.AutomationStatus = Enum.Parse<eActivityAutomationStatus>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.ErrorHandlerMappingType)))
                    activity.ErrorHandlerMappingType = Enum.Parse<eHandlerMappingType>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.Guid)))
                    activity.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.ParentGuid)))
                    activity.ParentGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.PercentAutomated)))
                    activity.PercentAutomated = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(Activity.POMMetaDataId)))
                    activity.POMMetaDataId = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(Activity.TargetApplication)))
                    activity.TargetApplication = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(Activity.Type)))
                    activity.Type = Enum.Parse<eSharedItemType>(attribute.Value);
            }

            foreach(XmlElement childElement in activityElement.ChildNodes.Cast<XmlElement>())
            {
                if(string.Equals(childElement.Name, nameof(Activity.Acts)))
                {
                    ActXMLSerializer actXMLSerializer = new();
                    List<Act> acts = new();
                    foreach(XmlElement actElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        Act act = actXMLSerializer.Deserialize(actElement);
                        acts.Add(act);
                    }
                    activity.Acts = new(acts);
                }
            }

            return activity;
        }
    }
}
