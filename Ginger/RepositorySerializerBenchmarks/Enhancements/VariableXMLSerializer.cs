using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Variables;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class VariableXMLSerializer
    {
        public VariableXMLSerializer() { }

        public XmlElement Serialize(VariableBase variable, XmlDocument xmlDocument)
        {
            if (variable is VariableString variableString)
                return Serialize(variableString, xmlDocument);
            else
                throw new NotImplementedException($"{nameof(VariableXMLSerializer)} implementation for type {variable.GetType().FullName} is not implemented yet.");
        }

        private XmlElement Serialize(VariableString variableString, XmlDocument xmlDocument)
        {
            XmlElement variableStringElement = xmlDocument.CreateElement(nameof(VariableString));
            
            if(!string.IsNullOrEmpty(variableString.Description))
                variableStringElement.SetAttribute(nameof(VariableString.Description), variableString.Description);
            
            variableStringElement.SetAttribute(nameof(VariableString.Guid), variableString.Guid.ToString());
            
            if(!string.IsNullOrEmpty(variableString.InitialStringValue))
                variableStringElement.SetAttribute(nameof(VariableString.InitialStringValue), variableString.InitialStringValue);
            
            variableStringElement.SetAttribute(nameof(VariableString.MappedOutputType), variableString.MappedOutputType.ToString());
            
            if(!string.IsNullOrEmpty(variableString.Name))
                variableStringElement.SetAttribute(nameof(VariableString.Name), variableString.Name);

            variableStringElement.SetAttribute(nameof(VariableString.ParentGuid), variableString.ParentGuid.ToString());

            if(!string.IsNullOrEmpty(variableString.ParentName))
                variableStringElement.SetAttribute(nameof(VariableString.ParentName), variableString.ParentName);

            if(!string.IsNullOrEmpty(variableString.ParentType))
                variableStringElement.SetAttribute(nameof(VariableString.ParentType), variableString.ParentType);
            
            return variableStringElement;
        }

        public VariableBase Deserialize(XmlElement variableElement)
        {
            if (string.Equals(variableElement.Name, nameof(VariableBase)))
                return DeserializeVariableString(variableElement);
            else
                throw new NotImplementedException($"{nameof(VariableXMLSerializer)} implementation for type {variableElement.Name} is not implemented yet.");
        }

        private VariableString DeserializeVariableString(XmlElement variableStringElement)
        {
            VariableString variableString = new();

            foreach (XmlAttribute attribute in variableStringElement.Attributes)
                SetVariableStringPropertyFromAttribute(variableString, attribute.Name, attribute.Value);

            return variableString;
        }

        private void SetVariableStringPropertyFromAttribute(VariableString variableString, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(VariableString.Description)))
                variableString.Description = attributeValue;
            else if (string.Equals(attributeName, nameof(VariableString.Guid)))
                variableString.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(VariableString.InitialStringValue)))
                variableString.InitialStringValue = attributeValue;
            else if (string.Equals(attributeName, nameof(VariableString.MappedOutputType)))
                variableString.MappedOutputType = Enum.Parse<VariableString.eOutputType>(attributeValue);
            else if (string.Equals(attributeName, nameof(VariableString.Name)))
                variableString.Name = attributeValue;
            else if (string.Equals(attributeName, nameof(VariableString.ParentGuid)))
                variableString.ParentGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(VariableString.ParentName)))
                variableString.ParentName = attributeValue;
            else if (string.Equals(attributeName, nameof(VariableString.ParentType)))
                variableString.ParentType = attributeValue;
        }
    
        public VariableBase Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (string.Equals(xmlReader.Name, nameof(VariableString)))
                return DeserializeVariableString(xmlReader);
            else
                throw new NotImplementedException($"{nameof(VariableXMLSerializer)} implementation for type {xmlReader.Name} is not implemented yet.");
        }

        private VariableString DeserializeVariableString(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(VariableString)))
                throw new Exception($"Expected element {nameof(VariableString)} but found {xmlReader.Name}.");

            VariableString variableString = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetVariableStringPropertyFromAttribute(variableString, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, nameof(VariableString));
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

            }

            return variableString;
        }
    }
}
