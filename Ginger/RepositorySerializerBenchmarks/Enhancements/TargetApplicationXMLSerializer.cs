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
        public TargetApplicationXMLSerializer() { }

        public XmlElement Serialize(TargetApplication targetApplication, XmlDocument xmlDocument)
        {
            XmlElement targetApplicationElement = xmlDocument.CreateElement(nameof(TargetApplication));
            
            if(!string.IsNullOrEmpty(targetApplication.AppName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.AppName), targetApplication.AppName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.Guid), targetApplication.Guid.ToString());
            
            if(!string.IsNullOrEmpty(targetApplication.LastExecutingAgentName))
                targetApplicationElement.SetAttribute(nameof(TargetApplication.LastExecutingAgentName), targetApplication.LastExecutingAgentName);
            
            targetApplicationElement.SetAttribute(nameof(TargetApplication.ParentGuid), targetApplication.ParentGuid.ToString());
            
            return targetApplicationElement;
        }

        public TargetApplication Deserialize(XmlElement targetApplicationElement)
        {
            TargetApplication targetApplication = new();

            foreach(XmlAttribute attribute in targetApplicationElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(TargetApplication.AppName)))
                    targetApplication.AppName = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(TargetApplication.Guid)))
                    targetApplication.Guid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(TargetApplication.ParentGuid)))
                    targetApplication.ParentGuid = Guid.Parse(attribute.Value);
            }

            return targetApplication;
        }
    }
}
