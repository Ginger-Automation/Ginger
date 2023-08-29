using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public abstract class Flow
    {
        public string Id { get; }

        public string Name { get; }

        public IFlowSource Source { get; }

        public IFlowTarget Target { get; }

        protected Flow(string name, IFlowSource source, IFlowTarget target)
        {
            Id = $"flow_{Guid.NewGuid()}";
            Name = name;
            Source = source;
            Target = target;
        }

        public static void Create(string name, IFlowSource source, IFlowTarget target)
        {
            Flow flow;
            if (string.Equals(source.ProcessId, target.ProcessId))
                flow = new SequenceFlow(name, source, target);
            else
                flow = new MessageFlow(name, source, target);

            source.OutgoingFlows.Add(flow);
            target.IncomingFlows.Add(flow);
        }
    }
}
