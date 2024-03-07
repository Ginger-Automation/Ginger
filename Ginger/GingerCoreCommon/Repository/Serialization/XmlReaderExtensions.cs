using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Amdocs.Ginger.Common.Repository.Serialization.Exceptions;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public static class XmlReaderExtensions
    {
        public delegate void NodeProcessor(XmlReader nodeReader);
        public delegate void AttributeProcessor(string name, string value);

        public static void ValidateNodeType(this XmlReader reader, XmlNodeType expected)
        {
            ValidateNodeType(reader, expected,
                msg: $"Expected {nameof(XmlNodeType)} of '{Enum.GetName(expected)}' but found '{Enum.GetName(reader.NodeType)}'.");
        }

        public static void ValidateNodeType(this XmlReader reader, XmlNodeType expected, string msg)
        {
            if (reader.NodeType != expected)
                throw new UnexpectedXmlNodeException(msg);
        }

        public static void ValidateNodeName(this XmlReader reader, string expected)
        {
            ValidateNodeName(reader, expected,
                msg: $"Expected {nameof(XmlNode)} with name '{expected}' but found '{reader.Name}'.");
        }

        public static void ValidateNodeName(this XmlReader reader, string expected, string msg)
        {
            if (!string.Equals(reader.Name, expected))
                throw new UnexpectedXmlNodeException(msg);
        }

        public static void ReadChildElements(this XmlReader reader, NodeProcessor nodeProcessor)
        {
            reader.ValidateNodeType(XmlNodeType.Element);

            if (reader.IsEmptyElement)
                return;

            //take a new subtree reader to avoid going beyond the current element
            //reader = reader.ReadSubtree();

            //positions the reader on the node that was current before the call to ReadSubtree
            //reader.Read();

            int rootDepth = reader.Depth;

            bool reachedElementEnd(XmlReader r) =>
                r.Depth == rootDepth &&
                r.NodeType != XmlNodeType.Element;

            bool isStartElement(XmlReader r) =>
                r.NodeType == XmlNodeType.Element;

            bool isGrandChild(XmlReader r, int parentDepth) =>
                r.Depth > parentDepth + 1;

            //move to first child
            reader.Read();

            while (!reachedElementEnd(reader))
            {
                //skip all non start element or grandchild nodes
                if (!isStartElement(reader) || isGrandChild(reader, rootDepth))
                {
                    reader.Skip();
                    continue;
                }

                //using XmlReader childReader = reader.ReadSubtree();

                //positions the childReader on the node that was current before the call to ReadSubtree
                //childReader.Read();

                //nodeProcessor(childReader);
                nodeProcessor(reader);

                //move to next child or parent node
                reader.Skip();
            }

            //reader.Dispose();
        }

        public static void ReadAttributes(this XmlReader reader, AttributeProcessor attributeProcessor)
        {
            ValidateNodeType(reader, XmlNodeType.Element);

            if (!reader.HasAttributes)
                return;

            reader.MoveToFirstAttribute();
            do
            {
                attributeProcessor.Invoke(reader.Name, reader.Value);
            } while (reader.MoveToNextAttribute());

            reader.MoveToElement();
        }

        public static bool HasName(this XmlReader reader, string expected)
        {
            return string.Equals(reader.Name, expected);
        }

        public static bool HasNodeType(this XmlReader reader, XmlNodeType expected)
        {
            return reader.NodeType == expected;
        }
    }
}
