using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public class Task : IFlowSource, IFlowTarget
    {
        public string Guid { get; }

        public string Id { get; }

        public string ProcessId { get; }

        public string Name { get; }

        public FlowCollection IncomingFlows { get; }

        public FlowCollection OutgoingFlows { get; }

        public Task(string processId, Guid guid, string name) : this(processId, guid.ToString(), name) { }

        public Task(string processId, string guid, string name)
        {
            Guid = guid;
            Id = $"task_{Guid}";
            Name = name;
            ProcessId = processId;
            IncomingFlows = new();
            OutgoingFlows = new();
        }
    }
}
