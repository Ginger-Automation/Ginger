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
        private readonly XmlDocument _xmlDocument;

        public ActivityXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(Activity activity)
        {
            XmlElement activityElement = _xmlDocument.CreateElement(nameof(Activity));

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
                XmlElement actsElement = _xmlDocument.CreateElement(nameof(Activity.Acts));
                ActXMLSerializer actXMLSerializer = new(_xmlDocument);
                foreach (Act act in activity.Acts)
                {
                    XmlElement actElement = actXMLSerializer.Serialize(act);
                    actsElement.AppendChild(actElement);
                }
                activityElement.AppendChild(actsElement);
            }

            return activityElement;
        }
    }
}
