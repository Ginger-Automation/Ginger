using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class ActXMLSerializer
    {
        private const string InputValueXMLElementName = "InputValues";

        public ActXMLSerializer() { }

        public XmlElement Serialize(Act act, XmlDocument xmlDocument)
        {
            if (act is ActDummy actDummy)
                return Serialize(actDummy, xmlDocument);
            else
                throw new NotImplementedException($"{nameof(ActXMLSerializer)} implementation for type {act.GetType().FullName} is not implemented yet.");
        }

        private XmlElement Serialize(ActDummy actDummy, XmlDocument xmlDocument)
        {
            XmlElement actDummyElement = xmlDocument.CreateElement(nameof(ActDummy));

            actDummyElement.SetAttribute(nameof(ActDummy.Active), actDummy.Active.ToString());

            if(!string.IsNullOrEmpty(actDummy.Description))
                actDummyElement.SetAttribute(nameof(ActDummy.Description), actDummy.Description);
            
            actDummyElement.SetAttribute(nameof(ActDummy.Guid), actDummy.Guid.ToString());
            
            actDummyElement.SetAttribute(nameof(ActDummy.ParentGuid), actDummy.ParentGuid.ToString());
            
            actDummyElement.SetAttribute(nameof(ActDummy.Platform), actDummy.Platform.ToString());
            
            actDummyElement.SetAttribute(nameof(ActDummy.RetryMechanismInterval), actDummy.RetryMechanismInterval.ToString());
            
            actDummyElement.SetAttribute(nameof(ActDummy.StatusConverter), actDummy.StatusConverter.ToString());
            
            actDummyElement.SetAttribute(nameof(ActDummy.WindowsToCapture), actDummy.WindowsToCapture.ToString());

            if(actDummy.ActInputValues.Any())
            {
                XmlElement inputValuesElement = xmlDocument.CreateElement(InputValueXMLElementName);
                ActInputValueXMLSerializer actInputValueXMLSerializer = new();
                foreach(ActInputValue actInputValue in actDummy.ActInputValues)
                {
                    XmlElement actInputValueElement = actInputValueXMLSerializer.Serialize(actInputValue, xmlDocument);
                    inputValuesElement.AppendChild(actInputValueElement);
                }
                actDummyElement.AppendChild(inputValuesElement);
            }

            return actDummyElement;
        }

        public Act Deserialize(XmlElement actElement)
        {
            if (string.Equals(actElement.Name, nameof(ActDummy)))
                return DeserializeActDummy(actElement);
            else
                throw new NotImplementedException($"{nameof(ActXMLSerializer)} implementation for type {actElement.Name} is not implemented yet.");
        }

        private ActDummy DeserializeActDummy(XmlElement actDummyElement)
        {
            ActDummy actDummy = new();

            foreach(XmlAttribute attribute in actDummyElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(ActDummy.Active)))
                    actDummy.Active = bool.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.Description)))
                    actDummy.Description = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(ActDummy.Guid)))
                    actDummy.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.ParentGuid)))
                    actDummy.ParentGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.Platform)))
                    actDummy.Platform = Enum.Parse<ePlatformType>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.RetryMechanismInterval)))
                    actDummy.RetryMechanismInterval = int.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.StatusConverter)))
                    actDummy.StatusConverter = Enum.Parse<eStatusConverterOptions>(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActDummy.WindowsToCapture)))
                    actDummy.WindowsToCapture= Enum.Parse<Act.eWindowsToCapture>(attribute.Value);
            }

            foreach(XmlElement childElement in actDummyElement.ChildNodes.Cast<XmlElement>())
            {
                if(string.Equals(childElement.Name, InputValueXMLElementName))
                {
                    ActInputValueXMLSerializer actInputValueXMLSerializer = new();
                    List<ActInputValue> actInputValues = new();
                    foreach(XmlElement actInputValueElement in childElement.ChildNodes.Cast<XmlElement>())
                    {
                        ActInputValue actInputValue = actInputValueXMLSerializer.Deserialize(actInputValueElement);
                        actInputValues.Add(actInputValue);
                    }
                    actDummy.InputValues = new(actInputValues);
                }
            }

            return actDummy;
        }
    }
}
