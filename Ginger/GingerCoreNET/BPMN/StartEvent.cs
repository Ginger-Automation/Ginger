using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class StartEvent : IFlowSource
    {
        public string Id { get; }

        public string Name { get; }
        
        public string ProcessId { get; }

        public FlowCollection OutgoingFlows { get; }

        public StartEvent(string name, string processId)
        {
            Id = $"event_{Guid.NewGuid()}";
            Name = name;
            ProcessId = processId;
            OutgoingFlows = new();
        }
    }
}
