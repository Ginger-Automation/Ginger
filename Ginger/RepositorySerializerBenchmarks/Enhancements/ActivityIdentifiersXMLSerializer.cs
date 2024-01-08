using GingerCore;
using GingerCore.Activities;
using OpenQA.Selenium.DevTools.V115.Target;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActivityIdentifiersXMLSerializer
    {
        public ActivityIdentifiersXMLSerializer() { }

        public XmlElement Serialize(ActivityIdentifiers activityIdentifiers, XmlDocument xmlDocument)
        {
            XmlElement activityIdentifiersElement = xmlDocument.CreateElement(nameof(ActivityIdentifiers));

            if(activityIdentifiers.ActivityAutomationStatus != null)
                activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.ActivityAutomationStatus), activityIdentifiers.ActivityAutomationStatus.ToString());
            
            activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.ActivityGuid), activityIdentifiers.ActivityGuid.ToString());
            
            if(!string.IsNullOrEmpty(activityIdentifiers.ActivityName))
                activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.ActivityName), activityIdentifiers.ActivityName);
            
            activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.ActivityParentGuid), activityIdentifiers.ActivityParentGuid.ToString());
            
            activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.Guid), activityIdentifiers.Guid.ToString());
            
            activityIdentifiersElement.SetAttribute(nameof(ActivityIdentifiers.ParentGuid), activityIdentifiers.ParentGuid.ToString());

            return activityIdentifiersElement;
        }

        public ActivityIdentifiers Deserialize(XmlElement activityIdentifiersElement)
        {
            ActivityIdentifiers activityIdentifiers = new();

            foreach(XmlAttribute attribute in activityIdentifiersElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.ActivityAutomationStatus)))
                    activityIdentifiers.ActivityAutomationStatus = Enum.Parse<eActivityAutomationStatus>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.ActivityGuid)))
                    activityIdentifiers.ActivityGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.ActivityName)))
                    activityIdentifiers.ActivityName = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.ActivityParentGuid)))
                    activityIdentifiers.ActivityParentGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.Guid)))
                    activityIdentifiers.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActivityIdentifiers.ParentGuid)))
                    activityIdentifiers.ParentGuid = Guid.Parse(attribute.Value);
            }

            return activityIdentifiers;
        }
    }
}
