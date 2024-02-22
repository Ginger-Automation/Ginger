using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public readonly struct LiteXmlElement
    {
        public string Name { get; init; }

        public IReadOnlyList<LiteXmlAttribute> Attributes { get; init; }

        public IReadOnlyList<LiteXmlElement> ChildElements { get; init; }

        private LiteXmlElement(string name, IReadOnlyList<LiteXmlAttribute> attributes, IReadOnlyList<LiteXmlElement> childElements)
        {
            Name = name;
            Attributes = attributes;
            ChildElements = childElements;
        }

        public static LiteXmlElement Load(XmlReader reader)
        {
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

            return new LiteXmlElement()
            {
                Name = name,
                Attributes = attributes,
                ChildElements = childElements
            };
        }
    }
}
