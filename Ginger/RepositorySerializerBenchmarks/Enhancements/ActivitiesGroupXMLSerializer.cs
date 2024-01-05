using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActivitiesGroupXMLSerializer
    {
        private readonly XmlDocument _xmlDocument;

        public ActivitiesGroupXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(ActivitiesGroup activitiesGroup)
        {
            XmlElement activitiesGroupElement = _xmlDocument.CreateElement(nameof(ActivitiesGroup));
            
            if(!string.IsNullOrEmpty(activitiesGroup.ExternalID))
                activitiesGroupElement.SetAttribute(nameof(ActivitiesGroup.ExternalID), activitiesGroup.ExternalID);
            
            if(!string.IsNullOrEmpty(activitiesGroup.ExternalID2))
                activitiesGroupElement.SetAttribute(nameof(ActivitiesGroup.ExternalID2), activitiesGroup.ExternalID2);
            
            activitiesGroupElement.SetAttribute(nameof(ActivitiesGroup.Guid), activitiesGroup.Guid.ToString());
            
            if(!string.IsNullOrEmpty(activitiesGroup.Name))
                activitiesGroupElement.SetAttribute(nameof(ActivitiesGroup.Name), activitiesGroup.Name);
            
            activitiesGroupElement.SetAttribute(nameof(ActivitiesGroup.ParentGuid), activitiesGroup.ParentGuid.ToString());

            if (activitiesGroup.ActivitiesIdentifiers.Any())
            {
                XmlElement activitiesIdentifiersElement = _xmlDocument.CreateElement(nameof(ActivitiesGroup.ActivitiesIdentifiers));
                ActivityIdentifiersXMLSerializer activityIdentifiersXMLSerializer = new(_xmlDocument);
                foreach (ActivityIdentifiers activityIdentifiers in activitiesGroup.ActivitiesIdentifiers)
                {
                    XmlElement activityIdentifiersElement = activityIdentifiersXMLSerializer.Serialize(activityIdentifiers);
                    activitiesIdentifiersElement.AppendChild(activityIdentifiersElement);
                }
                activitiesGroupElement.AppendChild(activitiesIdentifiersElement);
            }

            return activitiesGroupElement;
        }
    }
}
