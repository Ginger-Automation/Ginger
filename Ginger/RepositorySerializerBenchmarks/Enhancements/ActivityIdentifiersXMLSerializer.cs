using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using OpenQA.Selenium.DevTools.V115.Target;
using Org.BouncyCastle.Asn1.Cms;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            foreach (XmlAttribute attribute in activityIdentifiersElement.Attributes)
                SetActivityIdentifiersPropertyFromAttribute(activityIdentifiers, attribute.Name, attribute.Value);

            return activityIdentifiers;
        }

        private void SetActivityIdentifiersPropertyFromAttribute(ActivityIdentifiers activityIdentifiers, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(ActivityIdentifiers.ActivityAutomationStatus)))
                activityIdentifiers.ActivityAutomationStatus = Enum.Parse<eActivityAutomationStatus>(attributeValue);
            else if (string.Equals(attributeName, nameof(ActivityIdentifiers.ActivityGuid)))
                activityIdentifiers.ActivityGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActivityIdentifiers.ActivityName)))
                activityIdentifiers.ActivityName = attributeValue;
            else if (string.Equals(attributeName, nameof(ActivityIdentifiers.ActivityParentGuid)))
                activityIdentifiers.ActivityParentGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActivityIdentifiers.Guid)))
                activityIdentifiers.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActivityIdentifiers.ParentGuid)))
                activityIdentifiers.ParentGuid = Guid.Parse(attributeValue);
        }

        public ActivityIdentifiers Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(ActivityIdentifiers)))
                throw new Exception($"Expected element {nameof(ActivityIdentifiers)} but found {xmlReader.Name}.");

            ActivityIdentifiers activityIdentifiers = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetActivityIdentifiersPropertyFromAttribute(activityIdentifiers, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            if (!xmlReader.IsEmptyElement)
            {
                int startDepth = xmlReader.Depth;
                while (xmlReader.Read())
                {
                    xmlReader.MoveToContent();

                    bool reachedEndOfElement = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.EndElement;
                    if (reachedEndOfElement)
                        break;

                    if (!xmlReader.IsStartElement())
                        continue;

                    bool isGrandChild = xmlReader.Depth > startDepth + 1;
                    if (isGrandChild)
                        continue;

                }
            }

            return activityIdentifiers;
        }

        public ActivityIdentifiers Deserialize(LiteXMLElement activityIdentifiersElement)
        {
            ActivityIdentifiers activityIdentifiers = new();

            foreach (LiteXMLAttribute attribute in activityIdentifiersElement.Attributes)
                SetActivityIdentifiersPropertyFromAttribute(activityIdentifiers, attribute.Name, attribute.Value);

            return activityIdentifiers;
        }
    }
}
