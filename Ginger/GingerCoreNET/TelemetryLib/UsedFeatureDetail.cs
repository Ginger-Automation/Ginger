using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class UsedFeatureDetail
    {
        public string Name { get; set; }
        public bool IsConfigured { get; set; } = false;
        public bool IsUsed { get; set; } = false;

    }
}
