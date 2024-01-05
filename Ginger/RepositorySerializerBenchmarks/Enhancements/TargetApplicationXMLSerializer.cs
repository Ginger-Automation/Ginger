using GingerCore.Platforms;
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
        private readonly XmlDocument _xmlDocument;

        public TargetApplicationXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(TargetApplication targetApplication)
        {
            XmlElement targetApplicationElement = _xmlDocument.CreateElement(nameof(TargetApplication));
            
            if(!string.IsNullOrEmpty(targetApplication.AppName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.AppName), targetApplication.AppName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.Guid), targetApplication.Guid.ToString());
            
            if(!string.IsNullOrEmpty(targetApplication.LastExecutingAgentName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.LastExecutingAgentName), targetApplication.LastExecutingAgentName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.ParentGuid), targetApplication.ParentGuid.ToString());
            
            return targetApplicationElement;
        }
    }
}
