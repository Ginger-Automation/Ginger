using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositorySerializerBenchmarks.Enhancements.LiteXML
{
    public readonly struct LiteXMLAttribute
    {
        public string Name { get; }

        public string Value { get; }

        public LiteXMLAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
