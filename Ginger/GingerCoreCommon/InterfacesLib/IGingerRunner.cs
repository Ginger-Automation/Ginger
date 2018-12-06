using System;
using System.Collections.Generic;
using System.Text;
using Ginger.Run;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IGingerRunner
    {
        double? Elapsed { get;  }
        IExecutionLogger ExecutionLogger { get; }

        ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "");
    }
}
