using AccountReport.Contracts.RequestModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{   

    public class AccountReportStatistics
    {
        public ExecutionStatistics ChildsExecutionStatistics { get; set; }
        public Guid EntityId { get; set; }
        public string Type { get; set; }
    }
}
