using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class ExclusiveGateway : IFlowSource, IFlowTarget
    {
        public Guid Guid { get; }

        public string Id { get; }

        public string ProcessId { get; }

        public string Name { get; }

        public FlowCollection IncomingFlows { get; }

        public FlowCollection OutgoingFlows { get; }

        public ExclusiveGateway(string processId, Guid guid, string name)
        {
            Guid = guid;
            ProcessId = processId;
            Id = $"gateway_{guid}";
            Name = name;
            IncomingFlows = new();
            OutgoingFlows = new();
        }
    }
}
