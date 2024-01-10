using Amdocs.Ginger.Repository;
using GingerCore;
using MethodTimer;
using RepositorySerializerBenchmarks.Enhancements.LiteXML;
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

            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
            XmlElement repositoryItemHeaderElement = repositoryItemHeaderXMLSerializer.Serialize(businessFlow.RepositoryItemHeader!, xmlDocument);
            gingerRepositoryItemElement.AppendChild(repositoryItemHeaderElement);

            BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
            XmlElement businessFlowElement = businessFlowXMLSerializer.Serialize(businessFlow, xmlDocument);
            gingerRepositoryItemElement.AppendChild(businessFlowElement);

            xmlDocument.AppendChild(gingerRepositoryItemElement);

            return XmlDocumentToString(xmlDocument);
        }

        public TRepositoryItemBase DeserializeViaXmlDocument<TRepositoryItemBase>(string repositoryItemBaseXML, bool lazyLoad) where TRepositoryItemBase : RepositoryItemBase
        {
            if(!typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");
            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(new StringReader(repositoryItemBaseXML));

            XmlElement gingerRepositoryItemElement = (XmlElement)xmlDocument.ChildNodes[1]!;
            
            XmlElement repositoryItemHeaderElement = (XmlElement)gingerRepositoryItemElement.ChildNodes[0]!;
            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
            RepositoryItemHeader repositoryItemHeader = repositoryItemHeaderXMLSerializer.Deserialize(repositoryItemHeaderElement);

            XmlElement businessFlowElement = (XmlElement)gingerRepositoryItemElement.ChildNodes[1]!;
            BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
            BusinessFlow businessFlow = businessFlowXMLSerializer.Deserialize(businessFlowElement, lazyLoad);

            businessFlow.RepositoryItemHeader = repositoryItemHeader;

            return (TRepositoryItemBase)(RepositoryItemBase)businessFlow;
        }

        //[Time]
        //public TRepositoryItemBase DeserializeViaXmlReader<TRepositoryItemBase>(string repositoryItemBaseXML, bool lazyLoad) where TRepositoryItemBase : RepositoryItemBase
        //{
        //    if (!typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
        //        throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");

        //    RepositoryItemHeader repositoryItemHeader = null!;
        //    BusinessFlow businessFlow = null!;

        //    using XmlReader xmlReader = XmlReader.Create(new StringReader(repositoryItemBaseXML));

        //    xmlReader.MoveToContent();
        //    if (xmlReader.NodeType != XmlNodeType.Element)
        //        throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");
        //    if (!string.Equals(xmlReader.Name, GingerRepositoryItemXMLElementName))
        //        throw new Exception($"Expected element {GingerRepositoryItemXMLElementName} but found {xmlReader.Name}.");

        //    int startDepth = xmlReader.Depth;
        //    while(xmlReader.Read())
        //    {
        //        bool reachedEndOfFile = xmlReader.EOF;
        //        bool reachedSibling = xmlReader.Depth == startDepth && !string.Equals(xmlReader.Name, GingerRepositoryItemXMLElementName);
        //        bool reachedParent = xmlReader.Depth < startDepth;
        //        if (reachedEndOfFile || reachedSibling || reachedParent)
        //            break;

        //        if (xmlReader.NodeType != XmlNodeType.Element)
        //            continue;

        //        if (xmlReader.Depth != startDepth + 1)
        //            continue;

        //        if (string.Equals(xmlReader.Name, "Header"))
        //        {
        //            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
        //            repositoryItemHeader = repositoryItemHeaderXMLSerializer.Deserialize(xmlReader);
        //        }
        //        else if (string.Equals(xmlReader.Name, nameof(BusinessFlow)))
        //        {
        //            BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
        //            businessFlow = businessFlowXMLSerializer.Deserialize(xmlReader, lazyLoad);
        //        }
        //    }

        //    businessFlow.RepositoryItemHeader = repositoryItemHeader;

        //    return (TRepositoryItemBase)(RepositoryItemBase)businessFlow;
        //}

        [Time]
        public TRepositoryItemBase DeserializeViaXmlReader<TRepositoryItemBase>(string repositoryItemBaseXML, bool lazyLoad) where TRepositoryItemBase : RepositoryItemBase
        {
            if (!typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");

            BusinessFlow.LazyLoad = lazyLoad;

            RepositoryItemHeader repositoryItemHeader = null!;
            BusinessFlow businessFlow = null!;

            using XmlReader xmlReader = XmlReader.Create(new StringReader(repositoryItemBaseXML), new XmlReaderSettings()
            {
                IgnoreWhitespace = true,
            });

            if (!xmlReader.IsStartElement())
                throw new Exception($"Expected a start element.");
            if (!string.Equals(xmlReader.Name, GingerRepositoryItemXMLElementName))
                throw new Exception($"Expected element {GingerRepositoryItemXMLElementName} but found {xmlReader.Name}.");

            if (!xmlReader.IsEmptyElement)
            {
                int startDepth = xmlReader.Depth;
                while (xmlReader.Read())
                {
                    bool reachedEndOfElement = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.EndElement;
                    if (reachedEndOfElement)
                        break;

                    if (!xmlReader.IsStartElement())
                        continue;

                    bool isGrandChild = xmlReader.Depth > startDepth + 1;
                    if (isGrandChild)
                        continue;

                    if (string.Equals(xmlReader.Name, "Header"))
                    {
                        RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
                        repositoryItemHeader = repositoryItemHeaderXMLSerializer.Deserialize(xmlReader);
                    }
                    else if (string.Equals(xmlReader.Name, nameof(BusinessFlow)))
                    {
                        BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
                        businessFlow = businessFlowXMLSerializer.Deserialize(xmlReader, lazyLoad);
                    }
                }
            }

            businessFlow.RepositoryItemHeader = repositoryItemHeader;

            return (TRepositoryItemBase)(RepositoryItemBase)businessFlow;
        }

        public TRepositoryItemBase DeserializeViaLiteXMLElement<TRepositoryItemBase>(string repositoryItemBaseXML, bool lazyLoad) where TRepositoryItemBase : RepositoryItemBase
        {
            if (!typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");

            using XmlReader xmlReader = XmlReader.Create(new StringReader(repositoryItemBaseXML));
            LiteXMLElement gingerRepositoryItemElement = LiteXMLElement.Parse(xmlReader);

            LiteXMLElement repositoryItemHeaderElement = gingerRepositoryItemElement.ChildElements.ElementAt(0);
            RepositoryItemHeaderXMLSerializer repositoryItemHeaderXMLSerializer = new();
            RepositoryItemHeader repositoryItemHeader = repositoryItemHeaderXMLSerializer.Deserialize(repositoryItemHeaderElement);

            LiteXMLElement businessFlowElement = gingerRepositoryItemElement.ChildElements.ElementAt(1);
            BusinessFlowXMLSerializer businessFlowXMLSerializer = new();
            BusinessFlow businessFlow = businessFlowXMLSerializer.Deserialize(businessFlowElement, lazyLoad);

            businessFlow.RepositoryItemHeader = repositoryItemHeader;

            return (TRepositoryItemBase)(RepositoryItemBase)businessFlow;
        }

        [Time]
        public TRepositoryItemBase DeserializeViaRIBXmlReader<TRepositoryItemBase>(string repositoryItemBaseXML, bool lazyLoad) where TRepositoryItemBase : RepositoryItemBase
        {
            if (!typeof(TRepositoryItemBase).IsAssignableTo(typeof(BusinessFlow)))
                throw new NotImplementedException($"{nameof(BetterRepositorySerializer)} implementation for type {typeof(TRepositoryItemBase).FullName} is not implemented yet.");

            BusinessFlow.LazyLoad = lazyLoad;

            RepositoryItemHeader repositoryItemHeader = null!;
            BusinessFlow businessFlow = null!;

            using XmlReader xmlReader = XmlReader.Create(new StringReader(repositoryItemBaseXML));

            if (!xmlReader.IsStartElement())
                throw new Exception($"Expected a start element.");
            if (!string.Equals(xmlReader.Name, GingerRepositoryItemXMLElementName))
                throw new Exception($"Expected element {GingerRepositoryItemXMLElementName} but found {xmlReader.Name}.");

            if (!xmlReader.IsEmptyElement)
            {
                RIBXmlReader ribXmlReader = new(xmlReader);

                int startDepth = xmlReader.Depth;
                while (xmlReader.Read())
                {
                    bool reachedEndOfElement = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.EndElement;
                    if (reachedEndOfElement)
                        break;

                    if (!xmlReader.IsStartElement())
                        continue;

                    bool isGrandChild = xmlReader.Depth > startDepth + 1;
                    if (isGrandChild)
                        continue;

                    if (string.Equals(xmlReader.Name, "Header"))
                    {
                        repositoryItemHeader = new(ribXmlReader);
                    }
                    else if (string.Equals(xmlReader.Name, nameof(BusinessFlow)))
                    {
                        businessFlow = new(ribXmlReader);
                    }
                }
            }

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
