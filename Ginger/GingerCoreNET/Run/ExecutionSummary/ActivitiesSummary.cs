using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.ExecutionSummary
{
    public class ActivitiesSummary
    {
        public int Total { get; set; }
        public int Pass { get; set; }
        public int Fail { get; set; }
        public int Blocked { get; set; }
    }
}
