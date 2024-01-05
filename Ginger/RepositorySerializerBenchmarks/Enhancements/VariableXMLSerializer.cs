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
        private readonly XmlDocument _xmlDocument;

        public VariableXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(VariableBase variable)
        {
            if (variable is VariableString variableString)
                return Serialize(variableString);
            else
                throw new NotImplementedException($"{nameof(VariableXMLSerializer)} implementation for type {variable.GetType().FullName} is not implemented yet.");
        }

        private XmlElement Serialize(VariableString variableString)
        {
            XmlElement variableStringElement = _xmlDocument.CreateElement(nameof(VariableString));
            
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
    }
}
