using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public sealed class LiteXmlElement
    {
        public string Name { get; init; }

        public List<LiteXmlAttribute> Attributes { get; init; }

        public List<LiteXmlElement> ChildElements { get; init; }

        private LiteXmlElement(string name, List<LiteXmlAttribute> attributes, List<LiteXmlElement> childElements)
        {
            Name = name;
            Attributes = attributes;
            ChildElements = childElements;
        }

        public static LiteXmlElement Load(XmlReader reader)
        {
            reader.MoveToContent();

            string name = reader.Name;

            List<LiteXmlAttribute> attributes = [];
            reader.ReadAttributes((name, value) =>
                attributes.Add(new()
                {
                    Name = name,
                    Value = value
                }));

            List<LiteXmlElement> childElements = [];
            reader.ReadChildElements(childReader =>
                childElements.Add(Load(childReader)));

            return new LiteXmlElement(name, attributes, childElements);
        }
    }
}
