using Amdocs.Ginger.Repository;
using System;

namespace Amdocs.Ginger.CoreNET.GeneralLib
{
    public class ModifiedRepositoryFileInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string FileType { get; set; }
        public bool Selected { get; set; }
        public Guid guid { get; set; }
        public RepositoryItemBase item { get; set; }
    }
}
