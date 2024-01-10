using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RepositorySerializerBenchmarks.Enhancements.LiteXML
{
    public readonly struct LiteXMLElement
    {
        public string Name { get; }

        public IEnumerable<LiteXMLAttribute> Attributes { get; }

        public IEnumerable<LiteXMLElement> ChildElements { get; }

        private LiteXMLElement(string name, IEnumerable<LiteXMLAttribute> attributes, IEnumerable<LiteXMLElement> childElements)
        {
            Name = name;
            Attributes = attributes;
            ChildElements = childElements;
        }

        public static LiteXMLElement Parse(XmlReader xmlReader)
        {
            xmlReader.MoveToContent();

            if (xmlReader.NodeType != XmlNodeType.Element)
                throw new Exception($"Expected a node with type {XmlNodeType.Element} but found {xmlReader.NodeType}.");

            string name = xmlReader.Name;

            LiteXMLAttribute[] attributes = new LiteXMLAttribute[xmlReader.AttributeCount];
            for(int attrIndex = 0; attrIndex < xmlReader.AttributeCount; attrIndex++)
            {
                xmlReader.MoveToAttribute(attrIndex);
                attributes[attrIndex] = new LiteXMLAttribute(xmlReader.Name, xmlReader.Value);
            }
            xmlReader.MoveToElement();

            List<LiteXMLElement> childElements = new();
            string elementName = xmlReader.Name;
            int elementDepth = xmlReader.Depth;
            while (xmlReader.Read())
            {
                bool reachedEndOfFile = xmlReader.EOF;
                bool reachedSibling = xmlReader.Depth == elementDepth && !string.Equals(xmlReader.Name, elementName);
                bool reachedParent = xmlReader.Depth < elementDepth;
                if (reachedEndOfFile || reachedSibling || reachedParent)
                    break;

                if(xmlReader.NodeType == XmlNodeType.Element)
                    childElements.Add(Parse(xmlReader));
            }

            return new LiteXMLElement(name, attributes, childElements);
        }
    }
}
