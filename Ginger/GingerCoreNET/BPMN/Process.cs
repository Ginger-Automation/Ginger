using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class Process
    {
        public string Id { get; }

        public StartEvent? StartEvent { get; private set; }

        private readonly ICollection<Task> _tasks;
        public IEnumerable<Task> Tasks => _tasks;

        public EndEvent? EndEvent { get; private set; }

        internal Process(string participantId)
        {
            Id = $"{participantId}_process";
            _tasks = new List<Task>();
        }

        public StartEvent AddStartEvent(string name)
        {
            StartEvent = new(name, processId: Id);
            return StartEvent;
        }

        public TTask AddTask<TTask>(string name) where TTask : Task
        {
            return AddTask<TTask>(Guid.NewGuid(), name);
        }

        public TTask AddTask<TTask>(Guid guid, string name) where TTask : Task
        {
            return AddTask<TTask>(guid.ToString(), name);
        }

        public TTask AddTask<TTask>(string guid, string name) where TTask : Task
        {
            TTask task = Task.Create<TTask>(processId: Id, guid: guid, name: name);
            _tasks.Add(task);
            return task;
        }

        public EndEvent AddEndEvent(string name)
        {
            return AddEndEvent(name, EndEventType.Default);
        }

        public EndEvent AddEndEvent(string name, EndEventType endEventType)
        {
            EndEvent = new(name, endEventType, processId: Id);
            return EndEvent;
        }

        public IEnumerable<SequenceFlow> GetSequenceFlows()
        {
            IEnumerable<SequenceFlow> sequenceFlows = GetFlows()
                .Where(flow => flow is SequenceFlow)
                .Select(flow => (SequenceFlow)flow)
                .Distinct();

            return sequenceFlows;
        }

        internal IEnumerable<Flow> GetFlows()
        {
            IEnumerable<Flow> flows = Array.Empty<Flow>();

            flows = flows.Concat(Tasks.SelectMany(task => task.IncomingFlows));

            flows = flows.Concat(Tasks.SelectMany(task => task.OutgoingFlows));

            if (StartEvent != null)
            {
                flows = flows.Concat(StartEvent.OutgoingFlows);
            }

            if (EndEvent != null)
            {
                flows = flows.Concat(EndEvent.IncomingFlows);
            }

            return flows;
        }
    }
}
