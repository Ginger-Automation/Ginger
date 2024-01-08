using GingerCore;
using GingerCore.Activities;
using LiteDB;
using Org.BouncyCastle.Asn1.Cms;
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

            foreach (XmlAttribute attribute in activitiesGroupElement.Attributes)
                SetActivitiesGroupPropertyFromAttribute(activitiesGroup, attribute.Name, attribute.Value);

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

        private void SetActivitiesGroupPropertyFromAttribute(ActivitiesGroup activitiesGroup, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(ActivitiesGroup.ExternalID)))
                activitiesGroup.ExternalID = attributeValue;
            else if (string.Equals(attributeName, nameof(ActivitiesGroup.ExternalID2)))
                activitiesGroup.ExternalID2 = attributeValue;
            else if (string.Equals(attributeName, nameof(ActivitiesGroup.Guid)))
                activitiesGroup.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActivitiesGroup.Name)))
                activitiesGroup.Name = attributeValue;
        }

        public ActivitiesGroup Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(ActivitiesGroup)))
                throw new Exception($"Expected element {nameof(ActivitiesGroup)} but found {xmlReader.Name}.");

            ActivitiesGroup activitiesGroup = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetActivitiesGroupPropertyFromAttribute(activitiesGroup, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(ActivitiesGroup));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (string.Equals(xmlReader.Name, nameof(ActivitiesGroup.ActivitiesIdentifiers)))
                    activitiesGroup.ActivitiesIdentifiers = new(DeserializeActivitiesIdentifiersElement(xmlReader));
            }

            return activitiesGroup;
        }

        private IEnumerable<ActivityIdentifiers> DeserializeActivitiesIdentifiersElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(ActivitiesGroup.ActivitiesIdentifiers)))
                throw new Exception($"Expected element {nameof(ActivitiesGroup.ActivitiesIdentifiers)} but found {xmlReader.Name}.");

            List<ActivityIdentifiers> activitiesIDentifiers = new();
            ActivityIdentifiersXMLSerializer activityIdentifiersXMLSerializer = new();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(ActivitiesGroup.ActivitiesIdentifiers));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;

                if (string.Equals(xmlReader.Name, nameof(ActivityIdentifiers)))
                {
                    ActivityIdentifiers activityIdentifiers = activityIdentifiersXMLSerializer.Deserialize(xmlReader);
                    activitiesIDentifiers.Add(activityIdentifiers);
                }
            }

            return activitiesIDentifiers;
        }
    }
}
