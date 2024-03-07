using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.Serialization.Exceptions
{
    public class GingerSerializationException : Exception
    {
        public GingerSerializationException() : base() { }

        public GingerSerializationException(string msg) : base(msg) { }
    }
}
