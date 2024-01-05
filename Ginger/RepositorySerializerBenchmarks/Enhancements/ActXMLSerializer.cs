using Amdocs.Ginger.Repository;
using GingerCore.Actions;
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

        private readonly XmlDocument _xmlDocument;

        public ActXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(Act act)
        {
            if (act is ActDummy actDummy)
                return Serialize(actDummy);
            else
                throw new NotImplementedException($"{nameof(ActXMLSerializer)} implementation for type {act.GetType().FullName} is not implemented yet.");
        }

        private XmlElement Serialize(ActDummy actDummy)
        {
            XmlElement actDummyElement = _xmlDocument.CreateElement(nameof(ActDummy));

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
                XmlElement inputValuesElement = _xmlDocument.CreateElement(InputValueXMLElementName);
                ActInputValueXMLSerializer actInputValueXMLSerializer = new(_xmlDocument);
                foreach(ActInputValue actInputValue in actDummy.ActInputValues)
                {
                    XmlElement actInputValueElement = actInputValueXMLSerializer.Serialize(actInputValue);
                    inputValuesElement.AppendChild(actInputValueElement);
                }
                actDummyElement.AppendChild(inputValuesElement);
            }

            return actDummyElement;
        }
    }
}
