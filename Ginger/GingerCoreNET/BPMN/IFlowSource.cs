using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public interface IFlowSource : IProcessEntitiy
    {
        public FlowCollection OutgoingFlows { get; }
    }
}
