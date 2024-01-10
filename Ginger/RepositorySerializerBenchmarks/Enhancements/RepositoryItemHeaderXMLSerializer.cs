using Amdocs.Ginger.Repository;
using GingerCore;
using NPOI.OpenXmlFormats.Dml;
using Org.BouncyCastle.Asn1.Cms;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
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

            if (!string.IsNullOrEmpty(repositoryItemHeader.ItemType))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.ItemType), repositoryItemHeader.ItemType);

            if (!string.IsNullOrEmpty(repositoryItemHeader.CreatedBy))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.CreatedBy), repositoryItemHeader.CreatedBy);

            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.Created), repositoryItemHeader.Created.ToString("yyyyMMddHHmm"));

            if (!string.IsNullOrEmpty(repositoryItemHeader.GingerVersion))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.GingerVersion), repositoryItemHeader.GingerVersion);

            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.Version), repositoryItemHeader.Version.ToString());

            if (!string.IsNullOrEmpty(repositoryItemHeader.LastUpdateBy))
                repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.LastUpdateBy), repositoryItemHeader.LastUpdateBy);

            repositoryItemHeaderElement.SetAttribute(nameof(RepositoryItemHeader.LastUpdate), repositoryItemHeader.LastUpdate.ToString("yyyyMMddHHmm"));

            return repositoryItemHeaderElement;
        }

        public RepositoryItemHeader Deserialize(XmlElement repositoryItemHeaderElement)
        {
            RepositoryItemHeader repositoryItemHeader = new();

            foreach (XmlAttribute attribute in repositoryItemHeaderElement.Attributes)
                SetRepositoryItemHeaderPropertyFromAttribute(repositoryItemHeader, attribute.Name, attribute.Value);

            return repositoryItemHeader;
        }

        private void SetRepositoryItemHeaderPropertyFromAttribute(RepositoryItemHeader repositoryItemHeader, string attributeName, string attributeValue)
        {
            if (string.Equals(attributeName, nameof(RepositoryItemHeader.ItemGuid)))
                repositoryItemHeader.ItemGuid = Guid.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.ItemType)))
                repositoryItemHeader.ItemType = attributeValue;
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.CreatedBy)))
                repositoryItemHeader.CreatedBy = attributeValue;
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.Created)))
                repositoryItemHeader.Created = DateTime.ParseExact(attributeValue, "yyyyMMddHHmm", provider: null);
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.GingerVersion)))
                repositoryItemHeader.GingerVersion = attributeValue;
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.Version)))
                repositoryItemHeader.Version = int.Parse(attributeValue);
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.LastUpdateBy)))
                repositoryItemHeader.LastUpdateBy = attributeValue;
            else if (string.Equals(attributeName, nameof(RepositoryItemHeader.LastUpdate)))
                repositoryItemHeader.LastUpdate = DateTime.ParseExact(attributeValue, "yyyyMMddHHmm", provider: null);
        }

        public RepositoryItemHeader Deserialize(XmlReader xmlReader)
        {
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
            if (!string.Equals(xmlReader.Name, HeaderXMLElementName))
                throw new Exception($"Expected element {HeaderXMLElementName} but found {xmlReader.Name}.");

            RepositoryItemHeader repositoryItemHeader = new();

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                SetRepositoryItemHeaderPropertyFromAttribute(repositoryItemHeader, attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            return repositoryItemHeader;
        }

        public RepositoryItemHeader Deserialize(LiteXMLElement repositoryItemHeaderElement)
        {
            RepositoryItemHeader repositoryItemHeader = new();

            foreach (LiteXMLAttribute attribute in repositoryItemHeaderElement.Attributes)
                SetRepositoryItemHeaderPropertyFromAttribute(repositoryItemHeader, attribute.Name, attribute.Value);

            return repositoryItemHeader;
        }

        public RepositoryItemHeader Deserialize(RIBXmlReader reader)
        {
            return Deserialize(reader.XmlReader);
        }
    }
}
