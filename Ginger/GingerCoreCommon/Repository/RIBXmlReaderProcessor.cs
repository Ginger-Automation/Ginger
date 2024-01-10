using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Amdocs.Ginger.Repository;
using GingerCore;
using static Amdocs.Ginger.Repository.RepositoryItemBase;

namespace Amdocs.Ginger.Common.Repository
{
    internal static class RIBXmlReaderProcessor
    {
        internal delegate void AttributeParser(string attributeName, string attributeValue);

        internal delegate void ParseElement(string elementName, RIBXmlReader reader);

        internal static void Load(RIBXmlReader reader, AttributeParser attributeParser, ParseElement ParseElement)
        {
            XmlReader xmlReader = reader.XmlReader;
            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a element node type but found {xmlReader.NodeType}.");

            for (int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                attributeParser.Invoke(attributeName: xmlReader.Name, attributeValue: xmlReader.Value);
            }
            xmlReader.MoveToElement();

            int startDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == startDepth && xmlReader.NodeType == XmlNodeType.Element;
                bool reachedParent = xmlReader.Depth < startDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                ParseElement.Invoke(elementName: xmlReader.Name, reader);
            }
        }
    }
}
