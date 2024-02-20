using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.Serialization
{
    public readonly struct LiteXmlAttribute
    {
        public string Name { get; init; }

        public string Value { get; init; }
    }
}
