using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class UsedActionDetail
    {
        public string Name { get; set; }
        public int CountTotal { get; set; } = 0;
        public int CountPassed { get; set; } = 0;
        public int CountFailed { get; set; } = 0;

    }
}
