using Amdocs.Ginger.Repository;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements
{
    public sealed class BetterRepositorySerializer
    {
        private const string GingerRepositoryItemXMLElementName = "GingerRepositoryItem";

        public string Serialize(RepositoryItemBase repositoryItemBase)
        {
            if (repositoryItemBase is not BusinessFlow businessFlow)
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {repositoryItemBase.GetType().FullName} is not implemented yet.");

            XmlDocument xmlDocument = new();

            XmlElement gingerRepositoryItemElement = xmlDocument.CreateElement(GingerRepositoryItemXMLElementName);

            if (businessFlow.RepositoryItemHeader == null)
                businessFlow.InitHeader();
            else
                businessFlow.UpdateHeader();

            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new(xmlDocument);
            XmlElement repositoryItemHeaderElement = repositoryItemHeaderXMLSerializer.Serialize(businessFlow.RepositoryItemHeader!);
            gingerRepositoryItemElement.AppendChild(repositoryItemHeaderElement);

            BusinessFlowXMLSerializer businessFlowXMLSerializer = new(xmlDocument);
            XmlElement businessFlowElement = businessFlowXMLSerializer.Serialize(businessFlow);
            gingerRepositoryItemElement.AppendChild(businessFlowElement);

            xmlDocument.AppendChild(gingerRepositoryItemElement);

            return XmlDocumentToString(xmlDocument);
        }

        public TRepositoryItemBase Deserialize<TRepositoryItemBase>(string repositoryItemBaseXML) where TRepositoryItemBase : RepositoryItemBase
        {
            if(typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(new StringReader(repositoryItemBaseXML));

            XmlElement gingerRepositoryItemElement = (XmlElement)xmlDocument.ChildNodes[1]!;
            
            XmlElement repositoryItemHeaderElement = (XmlElement)gingerRepositoryItemElement.ChildNodes[0]!;
            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
            RepositoryItemHeader repositoryItemHeader = repositoryItemHeaderXMLSerializer.Deserialize(repositoryItemHeaderElement);

            XmlElement businessFlowElement = (XmlElement)gingerRepositoryItemElement.ChildNodes[1]!;
            BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
            BusinessFlow businessFlow = businessFlowXMLSerializer.Deserialize(businessFlowElement);

            businessFlow.RepositoryItemHeader = repositoryItemHeader;

            return (TRepositoryItemBase)(RepositoryItemBase)businessFlow;
        }

        private string XmlDocumentToString(XmlDocument xmlDocument)
        {
            XmlWriterSettings xmlWriterSettings = new()
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n"
            };
            using MemoryStream memoryStream = new(1024);
            using StreamWriter streamWriter = new(memoryStream);
            using XmlWriter xmlTextWriter = XmlWriter.Create(streamWriter, xmlWriterSettings);
            xmlDocument.WriteTo(xmlTextWriter);
            xmlTextWriter.Flush();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
    }
}
