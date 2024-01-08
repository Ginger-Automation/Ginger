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
        public ActivitiesGroupXMLSerializer() { }

        public XmlElement Serialize(ActivitiesGroup activitiesGroup, XmlDocument xmlDocument)
        {
            XmlElement activitiesGroupElement = xmlDocument.CreateElement(nameof(ActivitiesGroup));
            
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
                XmlElement activitiesIdentifiersElement = xmlDocument.CreateElement(nameof(ActivitiesGroup.ActivitiesIdentifiers));
                ActivityIdentifiersXMLSerializer activityIdentifiersXMLSerializer = new();
                foreach (ActivityIdentifiers activityIdentifiers in activitiesGroup.ActivitiesIdentifiers)
                {
                    XmlElement activityIdentifiersElement = activityIdentifiersXMLSerializer.Serialize(activityIdentifiers, xmlDocument);
                    activitiesIdentifiersElement.AppendChild(activityIdentifiersElement);
                }
                activitiesGroupElement.AppendChild(activitiesIdentifiersElement);
            }

            return activitiesGroupElement;
        }

        public ActivitiesGroup Deserialize(XmlElement activitiesGroupElement)
        {
            ActivitiesGroup activitiesGroup = new();

            foreach(XmlAttribute attribute in activitiesGroupElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(ActivitiesGroup.ExternalID)))
                    activitiesGroup.ExternalID = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(ActivitiesGroup.ExternalID2)))
                    activitiesGroup.ExternalID2 = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(ActivitiesGroup.Guid)))
                    activitiesGroup.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActivitiesGroup.Name)))
                    activitiesGroup.Name = attribute.Value;
            }

            foreach(XmlElement childElement in activitiesGroupElement.ChildNodes.Cast<XmlElement>())
            {
                if(string.Equals(childElement.Name, nameof(ActivitiesGroup.ActivitiesIdentifiers)))
                {
                    ActivityIdentifiersXMLSerializer activityIdentifiersXMLSerializer = new();
                    List<ActivityIdentifiers> activityIdentifiersList = new();
                    foreach(XmlElement activityIdentifiersElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        ActivityIdentifiers activityIdentifiers = activityIdentifiersXMLSerializer.Deserialize(activityIdentifiersElement);
                        activityIdentifiersList.Add(activityIdentifiers);
                    }
                    activitiesGroup.ActivitiesIdentifiers = new(activityIdentifiersList);
                }
            }

            return activitiesGroup;
        }
    }
}
