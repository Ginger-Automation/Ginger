using System;
using System.Collections.Generic;
using System.Text;
using Ginger.Run;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IRunsetExecutor
    {
        TimeSpan Elapsed { get;  }

        ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool reportOnlyExecuted);
    }
}
