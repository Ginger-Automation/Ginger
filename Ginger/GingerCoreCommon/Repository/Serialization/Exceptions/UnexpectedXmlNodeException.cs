using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.Common.Repository.Serialization.Exceptions
{
    public sealed class UnexpectedXmlNodeException : GingerSerializationException
    {
        public UnexpectedXmlNodeException() : base() { }

        public UnexpectedXmlNodeException(string msg) : base(msg) { }
    }
}
