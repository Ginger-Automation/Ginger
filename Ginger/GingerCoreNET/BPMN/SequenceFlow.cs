using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class SequenceFlow : Flow
    {
        public SequenceFlow(string name, IFlowSource source, IFlowTarget target) : base(name, source, target) { }

        public SequenceFlow(Guid guid, string name, IFlowSource source, IFlowTarget target) : base(guid, name, source, target) { }

        public SequenceFlow(string guid, string name, IFlowSource source, IFlowTarget target) : base(guid, name, source, target) { }
    }
}
