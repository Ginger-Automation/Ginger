using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ConflictResolve
{
    public sealed class ConflictResolver
    {
        public ICollection<Comparison> Compare<T>(T? local, T? remote, string? name = null) where T : RepositoryItemBase
        {
            if (string.IsNullOrEmpty(name))
                name = GetDefaultNameForChangesForType<T>();

            return RIBCompare.Compare<T>(name, local, remote);
        }

        private string GetDefaultNameForChangesForType<T>()
        {
            string defaultName;
            
            Attribute? displayNameAttribute = typeof(T).GetCustomAttribute(typeof(DisplayNameAttribute));

            if (displayNameAttribute != null)
                defaultName = ((DisplayNameAttribute)displayNameAttribute).DisplayName;
            else
                defaultName = typeof(T).Name;

            return defaultName;
        }

        public T? Merge<T>(ICollection<Comparison> changes) where T : RepositoryItemBase
        {
            return RIBMerge.Merge<T>(changes);
        }
    }
}
