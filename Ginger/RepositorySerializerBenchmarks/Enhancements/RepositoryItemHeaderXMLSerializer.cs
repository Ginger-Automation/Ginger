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

        private readonly XmlDocument _xmlDocument;

        public RepositoryItemHeaderXMLSerializer(XmlDocument xmlDocument)
        {
            _xmlDocument = xmlDocument;
        }

        public XmlElement Serialize(RepositoryItemHeader repositoryItemHeader)
        {
            XmlElement repositoryItemHeaderElement = _xmlDocument.CreateElement(HeaderXMLElementName);

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
    }
}
