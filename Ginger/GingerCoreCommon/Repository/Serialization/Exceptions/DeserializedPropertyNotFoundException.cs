using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Common.Repository.Serialization.Exceptions
{
    public sealed class DeserializedPropertyNotFoundException : GingerSerializationException
    {
        public DeserializedPropertyNotFoundException() : base() { }

        public DeserializedPropertyNotFoundException(string msg) : base(msg) { }

        public static DeserializedPropertyNotFoundException WithDefaultMessage(string propertyName)
        {
            return new($"No deserialized property found by name '{propertyName}'.");
        }
    }
}
