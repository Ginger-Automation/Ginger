using Amdocs.Ginger.Repository;
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

            foreach(XmlAttribute attribute in actInputValueElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(ActInputValue.Guid)))
                    actInputValue.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(ActInputValue.Param)))
                    actInputValue.Param = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(ActInputValue.ParentGuid)))
                    actInputValue.ParentGuid = Guid.Parse(attribute.Value);
            }

            return actInputValue;
        }
    }
}
