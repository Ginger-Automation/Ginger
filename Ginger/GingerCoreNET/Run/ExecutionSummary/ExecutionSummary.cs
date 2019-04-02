using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.ExecutionSummary
{
    public class ExecutionSummary
    {
        public DateTime StartTime { get; set; } 
        public DateTime EndTime { get; set; }
        public TimeSpan Elapsed { get; set; }
        public Runners Runners { get;  } = new Runners();
        public BusinessFlows BusinessFlows { get; } = new BusinessFlows();
        
    }
}
