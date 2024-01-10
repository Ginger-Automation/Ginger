using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NpgsqlTypes;
using Org.BouncyCastle.Asn1.Cms;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            foreach (XmlAttribute attribute in actDummyElement.Attributes)
                SetActDummyPropertyFromAttribute(actDummy, attribute.Name, attribute.Value);

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

        private void SetActDummyPropertyFromAttribute(ActDummy actDummy, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(ActDummy.Active)))
                actDummy.Active = bool.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.Description)))
                actDummy.Description = attributeValue;
            else if (string.Equals(attributeName, nameof(ActDummy.Guid)))
                actDummy.Guid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.ParentGuid)))
                actDummy.ParentGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.Platform)))
                actDummy.Platform = Enum.Parse<ePlatformType>(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.RetryMechanismInterval)))
                actDummy.RetryMechanismInterval = int.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.StatusConverter)))
                actDummy.StatusConverter = Enum.Parse<eStatusConverterOptions>(attributeValue);
            else if (string.Equals(attributeName, nameof(ActDummy.WindowsToCapture)))
                actDummy.WindowsToCapture = Enum.Parse<Act.eWindowsToCapture>(attributeValue);
        }

        public Act Deserialize(XmlReader xmlReader)
        {
            //return (Act)Act.Create(new RIBXmlReader(xmlReader));
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (string.Equals(xmlReader.Name, nameof(ActDummy)))
                return DeserializeActDummy(xmlReader);
            else
                throw new NotImplementedException($"{nameof(ActXMLSerializer)} implementation for type {xmlReader.Name} is not implemented yet.");
        }

        private ActDummy DeserializeActDummy(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(ActDummy)))
                throw new Exception($"Expected element {nameof(ActDummy)} but found {xmlReader.Name}.");

            ActDummy actDummy = new();
            //Assembly gingerCoreNETAssembly = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName!.Contains("GingerCoreNET"));
            //Type? actDummyType = gingerCoreNETAssembly.GetType($"GingerCore.Actions.{nameof(ActDummy)}");
            //ActDummy actDummy = (ActDummy)Activator.CreateInstance(actDummyType!)!;

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetActDummyPropertyFromAttribute(actDummy, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
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

                    if (string.Equals(xmlReader.Name, nameof(Act.InputValues)))
                        actDummy.InputValues = new(DeserializeInputValuesElement(xmlReader));
                }
            }

            return actDummy;
        }

        private IEnumerable<ActInputValue> DeserializeInputValuesElement(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, nameof(Act.InputValues)))
                throw new Exception($"Expected element {nameof(Act.InputValues)} but found {xmlReader.Name}.");

            List<ActInputValue> actInputValues = new();
            ActInputValueXMLSerializer actInputValueXMLSerializer = new();

            if (xmlReader.IsEmptyElement)
                return Array.Empty<ActInputValue>();

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

                ActInputValue actInputValue = actInputValueXMLSerializer.Deserialize(xmlReader);
                actInputValues.Add(actInputValue);
            }

            return actInputValues;
        }

        public Act Deserialize(LiteXMLElement actElement)
        {
            if (string.Equals(actElement.Name, nameof(ActDummy)))
                return DeserializeActDummy(actElement);
            else
                throw new NotImplementedException($"{nameof(ActXMLSerializer)} implementation for type {actElement.Name} is not implemented yet.");
        }

        private ActDummy DeserializeActDummy(LiteXMLElement actDummyElement)
        {
            ActDummy actDummy = new();

            foreach (LiteXMLAttribute attribute in actDummyElement.Attributes)
                SetActDummyPropertyFromAttribute(actDummy, attribute.Name, attribute.Value);

            foreach (LiteXMLElement childElement in actDummyElement.ChildElements)
            {
                if (string.Equals(childElement.Name, InputValueXMLElementName))
                {
                    List<ActInputValue> actInputValues = new();
                    ActInputValueXMLSerializer actInputValueXMLSerializer = new();
                    foreach (LiteXMLElement actInputValueElement in childElement.ChildElements)
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
