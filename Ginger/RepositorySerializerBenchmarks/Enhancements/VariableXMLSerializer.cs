using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
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

            foreach(XmlAttribute attribute in variableStringElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(VariableString.Description)))
                    variableString.Description = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(VariableString.Guid)))
                    variableString.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(VariableString.InitialStringValue)))
                    variableString.InitialStringValue = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(VariableString.MappedOutputType)))
                    variableString.MappedOutputType = Enum.Parse<VariableString.eOutputType>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(VariableString.Name)))
                    variableString.Name = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(VariableString.ParentGuid)))
                    variableString.ParentGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(VariableString.ParentName)))
                    variableString.ParentName = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(VariableString.ParentType)))
                    variableString.ParentType = attribute.Value;
            }

            return variableString;
        }
    }
}
