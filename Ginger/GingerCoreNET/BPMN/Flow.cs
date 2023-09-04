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
        public string Guid { get; }

        public string Id { get; }

        public string Name { get; set; }

        public IFlowSource Source { get; }

        public IFlowTarget Target { get; }

        protected Flow(string name, IFlowSource source, IFlowTarget target) :
            this(System.Guid.NewGuid(), name, source, target) { }

        protected Flow(Guid guid, string name, IFlowSource source, IFlowTarget target) :
            this(guid.ToString(), name, source, target) { }

        protected Flow(string guid, string name, IFlowSource source, IFlowTarget target)
        {
            Guid = guid;
            Id = $"flow_{Guid}";
            Name = name;
            Source = source;
            Target = target;
        }

        public static Flow Create(string name, IFlowSource source, IFlowTarget target)
        {
            return Create(System.Guid.NewGuid(), name, source, target);
        }

        public static Flow Create(Guid guid, string name, IFlowSource source, IFlowTarget target)
        {
            return Create(guid.ToString(), name, source, target);
        }

        public static Flow Create(string guid, string name, IFlowSource source, IFlowTarget target)
        {
            Flow flow;
            if (string.Equals(source.ProcessId, target.ProcessId))
                flow = new SequenceFlow(guid, name, source, target);
            else
                flow = new MessageFlow(guid, name, source, target);

            source.OutgoingFlows.Add(flow);
            target.IncomingFlows.Add(flow);

            return flow;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj is not Flow)
                return false;

            Flow flow = (Flow)obj;

            return string.Equals(Id, flow.Id) && string.Equals(Name, flow.Name);
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            hashCode.Add(Id);
            hashCode.Add(Name);
            return hashCode.ToHashCode();
        }
    }
}
