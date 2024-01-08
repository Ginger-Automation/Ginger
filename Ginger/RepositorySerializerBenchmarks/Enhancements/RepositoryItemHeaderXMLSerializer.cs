using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class RepositoryItemHeaderXMLSerializer
    {
        private const string HeaderXMLElementName = "Header";

        public RepositoryItemHeaderXMLSerializer() { }

        public XmlElement Serialize(RepositoryItemHeader repositoryItemHeader, XmlDocument xmlDocument)
        {
            XmlElement repositoryItemHeaderElement = xmlDocument.CreateElement(HeaderXMLElementName);

            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.ItemGuid), repositoryItemHeader.ItemGuid.ToString());
            
            if(!string.IsNullOrEmpty(repositoryItemHeader.ItemType))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.ItemType), repositoryItemHeader.ItemType);
            
            if(!string.IsNullOrEmpty(repositoryItemHeader.CreatedBy))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.CreatedBy), repositoryItemHeader.CreatedBy);
            
            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.Created), repositoryItemHeader.Created.ToString("yyyyMMddHHmm"));
            
            if(!string.IsNullOrEmpty(repositoryItemHeader.GingerVersion))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.GingerVersion), repositoryItemHeader.GingerVersion);
            
            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.Version), repositoryItemHeader.Version.ToString());
            
            if(!string.IsNullOrEmpty(repositoryItemHeader.LastUpdateBy))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.LastUpdateBy), repositoryItemHeader.LastUpdateBy);
            
            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.LastUpdate), repositoryItemHeader.LastUpdate.ToString("yyyyMMddHHmm"));

            return repositoryItemHeaderElement;
        }

        public RepositoryItemHeader Deserialize(XmlElement repositoryItemHeaderElement)
        {
            RepositoryItemHeader repositoryItemHeader = new();

            foreach(XmlAttribute attribute in repositoryItemHeaderElement.Attributes)
            {
                if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.ItemGuid)))
                    repositoryItemHeader.ItemGuid = Guid.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.ItemType)))
                    repositoryItemHeader.ItemType = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.CreatedBy)))
                    repositoryItemHeader.CreatedBy = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.Created)))
                    repositoryItemHeader.Created = DateTime.ParseExact(attribute.Value, "yyyyMMddHHmm", provider: null);
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.GingerVersion)))
                    repositoryItemHeader.GingerVersion = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.Version)))
                    repositoryItemHeader.Version = int.Parse(attribute.Value);
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.LastUpdateBy)))
                    repositoryItemHeader.LastUpdateBy = attribute.Value;
                else if (string.Equals(attribute.Name, nameof(RepositoryItemHeader.LastUpdate)))
                    repositoryItemHeader.LastUpdate = DateTime.ParseExact(attribute.Value, "yyyyMMddHHmm", provider: null);
            }

            return repositoryItemHeader;
        }
    }
}
