using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class EndEvent : IFlowTarget
    {
        public string Id { get; }

        public string Name { get; }

        public string ProcessId { get; }

        public FlowCollection IncomingFlows { get; }

        public EndEvent(string name, string processId)
        {
            Id = $"event_{Guid.NewGuid()}";
            Name = name;
            ProcessId = processId;
            IncomingFlows = new();
        }
    }
}
