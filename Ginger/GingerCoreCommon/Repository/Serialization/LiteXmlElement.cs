using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public readonly struct LiteXmlElement
    {
        public string Name { get; init; }

        public IReadOnlyList<LiteXmlAttribute> Attributes { get; init; }

        public IReadOnlyList<LiteXmlElement> ChildElements { get; init; }
    }
}
