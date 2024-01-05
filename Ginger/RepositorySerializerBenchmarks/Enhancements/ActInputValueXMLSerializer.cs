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
        private readonly XmlDocument _xmlDocument;

        public ActInputValueXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(ActInputValue actInputValue)
        {
            XmlElement actInputValueElement = _xmlDocument.CreateElement(nameof(ActInputValue));

            actInputValueElement.SetAttribute(nameof(ActInputValue.Guid), actInputValue.Guid.ToString());
            
            if(!string.IsNullOrEmpty(actInputValue.Param))
                actInputValueElement.SetAttribute(nameof(ActInputValue.Param), actInputValue.Param);
            
            actInputValueElement.SetAttribute(nameof(ActInputValue.ParentGuid), actInputValue.ParentGuid.ToString());

            return actInputValueElement;
        }
    }
}
