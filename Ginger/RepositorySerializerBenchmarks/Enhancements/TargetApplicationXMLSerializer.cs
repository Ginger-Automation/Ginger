using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Platforms;
using LiteDB;
using Org.BouncyCastle.Asn1.Cms;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class TargetApplicationXMLSerializer
    {
        public TargetApplicationXMLSerializer() { }

        public XmlElement Serialize(TargetApplication targetApplication, XmlDocument xmlDocument)
        {
            XmlElement targetApplicationElement = xmlDocument.CreateElement(nameof(TargetApplication));
            
            if(!string.IsNullOrEmpty(targetApplication.AppName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.AppName), targetApplication.AppName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.Guid), targetApplication.Guid.ToString());
            
            if(!string.IsNullOrEmpty(targetApplication.LastExecutingAgentName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.LastExecutingAgentName), targetApplication.LastExecutingAgentName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.ParentGuid), targetApplication.ParentGuid.ToString());
            
            return targetApplicationElement;
        }

        public TargetApplication Deserialize(XmlElement targetApplicationElement)
        {
            TargetApplication targetApplication = new();

            foreach (XmlAttribute attribute in targetApplicationElement.Attributes)
                SetTargetApplicationPropertyFromAttribute(targetApplication, attribute.Name, attribute.Value);

            return targetApplication;
        }

        private void SetTargetApplicationPropertyFromAttribute(TargetApplication targetApplication, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(TargetApplication.AppName)))
                targetApplication.AppName = attributeValue;
            else if (string.Equals(attributeName, nameof(TargetApplication.Guid)))
                targetApplication.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(TargetApplication.ParentGuid)))
                targetApplication.ParentGuid = Guid.Parse(attributeValue);
        }

        public TargetApplication Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(TargetApplication)))
                throw new Exception($"Expected element {nameof(TargetApplication)} but found {xmlReader.Name}.");

            TargetApplication targetApplication = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetTargetApplicationPropertyFromAttribute(targetApplication, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
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

            return targetApplication;
        }

        public TargetApplication Deserialize(LiteXMLElement targetApplicationElement)
        {
            TargetApplication targetApplication = new();

            foreach (LiteXMLAttribute attribute in targetApplicationElement.Attributes)
                SetTargetApplicationPropertyFromAttribute(targetApplication, attribute.Name, attribute.Value);

            return targetApplication;
        }
    }
}
