using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActInputValueXMLSerializer
    {
        public ActInputValueXMLSerializer() { }

        public XmlElement Serialize(ActInputValue actInputValue, XmlDocument xmlDocument)
        {
            XmlElement actInputValueElement = xmlDocument.CreateElement(nameof(ActInputValue));

            actInputValueElement.SetAttribute(nameof(ActInputValue.Guid), actInputValue.Guid.ToString());
            
            if(!string.IsNullOrEmpty(actInputValue.Param))
                actInputValueElement.SetAttribute(nameof(ActInputValue.Param), actInputValue.Param);
            
            actInputValueElement.SetAttribute(nameof(ActInputValue.ParentGuid), actInputValue.ParentGuid.ToString());

            return actInputValueElement;
        }

        public ActInputValue Deserialize(XmlElement actInputValueElement)
        {
            ActInputValue actInputValue = new();

            foreach (XmlAttribute attribute in actInputValueElement.Attributes)
                SetActInputValuePropertyFromAttribute(actInputValue, attribute.Name, attribute.Value);

            return actInputValue;
        }

        private void SetActInputValuePropertyFromAttribute(ActInputValue actInputValue, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(ActInputValue.Guid)))
                actInputValue.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActInputValue.Param)))
                actInputValue.Param = attributeValue;
            else if (string.Equals(attributeName, nameof(ActInputValue.ParentGuid)))
                actInputValue.ParentGuid = Guid.Parse(attributeValue);
        }

        public ActInputValue Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(ActInputValue)))
                throw new Exception($"Expected element {nameof(ActInputValue)} but found {xmlReader.Name}.");

            ActInputValue actInputValue = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetActInputValuePropertyFromAttribute(actInputValue, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(ActInputValue));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                if (xmlReader.Depth != startDepth + 1)
                    continue;
            }

            return actInputValue;
        }
    }
}
