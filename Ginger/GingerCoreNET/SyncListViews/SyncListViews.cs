using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET
{
    public class SyncListViews
    {
        public string TargetSite { get; set; }
        public int Index { get; set; }
        public bool IsDoubleClick { get; set; } = false;
    }
}
