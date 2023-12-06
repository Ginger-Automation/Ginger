using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class CallActivity : IFlowSource, IFlowTarget
    {
        public Guid Guid { get; }

        public string Id { get; }

        public string ProcessId { get; }

        public string Name { get; }

        public string ProcessRef { get; set; }

        public FlowCollection IncomingFlows { get; }

        public FlowCollection OutgoingFlows { get; }

        public CallActivity(string processId, Guid guid, string name)
        {
            Guid = guid;
            Id = $"callActivity_{Guid}";
            Name = name;
            ProcessId = processId;
            IncomingFlows = new();
            OutgoingFlows = new();
        }
    }
}
