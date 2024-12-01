using System;

namespace Amdocs.Ginger.CoreNET.External.Katalon.Conversion
{
    internal abstract class KatalonElementEntity
    {
        internal Guid ElementGuidId { get; }

        internal string Name { get; }

        internal KatalonElementEntity(Guid elementGuidId, string name)
        {
            ElementGuidId = elementGuidId;
            Name = name;
        }
    }
}
