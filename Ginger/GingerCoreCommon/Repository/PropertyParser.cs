using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Repository;

namespace Amdocs.Ginger.Common.Repository
{

    public readonly struct PropertyParser<TOwner, TValue>
    {
        public delegate void ParsingHandler(TOwner owner, TValue value);

        public string Name { get; init; }

        public ParsingHandler Parser { get; init; }

        public PropertyParser(string name, ParsingHandler parser)
        {
            Name = name;
            Parser = parser;
        }
    }
}
