using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.Serialization.Exceptions
{
    public sealed class UnexpectedDeserializedPropertyType : GingerSerializationException
    {
        public UnexpectedDeserializedPropertyType() : base() { }

        public UnexpectedDeserializedPropertyType(string msg) : base(msg) { }

        public static UnexpectedDeserializedPropertyType WithDefaultMsg(string propertyName, string expectedType)
        {
            return new UnexpectedDeserializedPropertyType($"Property '{propertyName}' was not serialized as '{expectedType}'.");
        }
    }
}
