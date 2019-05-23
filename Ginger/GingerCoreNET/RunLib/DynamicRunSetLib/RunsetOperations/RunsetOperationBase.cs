using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class RunsetOperationBase
    {
        public string Condition { get; set; } = "AlwaysRun";

        public string RunAt { get; set; } = "ExecutionEnd";
    }
}
