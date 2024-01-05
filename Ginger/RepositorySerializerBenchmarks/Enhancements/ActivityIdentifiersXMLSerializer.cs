using GingerCore.Activities;
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
        private readonly XmlDocument _xmlDocument;

        public ActivityIdentifiersXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(ActivityIdentifiers activityIdentifiers)
        {
            XmlElement activityIdentifiersElement = _xmlDocument.CreateElement(nameof(ActivityIdentifiers));

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
    }
}
