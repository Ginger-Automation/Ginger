using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class MessageFlow : Flow
    {
        public string MessageRef { get; set; }

        public MessageFlow(string name, IFlowSource source, IFlowTarget target) : base(name, source, target) { }

        public MessageFlow(Guid guid, string name, IFlowSource source, IFlowTarget target) : base(guid, name, source, target) { }

        public MessageFlow(string guid, string name, IFlowSource source, IFlowTarget target) : base(guid, name, source, target) 
        {
            MessageRef = string.Empty;
        }
    }
}
