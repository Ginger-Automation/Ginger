using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public interface IFlowTarget : IProcessEntitiy
    {
        public FlowCollection IncomingFlows { get; }
    }
}
