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

        public Task AddTask(AddTaskArguments args)
        {
            Task task = new(processId: Id, guid: args.Guid, name: args.Name);
            _tasks.Add(task);
            return task;
        }

        public UserTask AddUserTask(AddTaskArguments args)
        {
            UserTask userTask = new(processId: Id, guid: args.Guid, name: args.Name);
            _tasks.Add(userTask);
            return userTask;
        }

        public EndEvent AddEndEvent(string name)
        {
            EndEvent = new(name, processId: Id);
            return EndEvent;
        }

        public IEnumerable<SequenceFlow> GetSequenceFlows()
        {
            IEnumerable<SequenceFlow> sequenceFlows = GetFlows()
                .Where(flow => flow is SequenceFlow)
                .Select(flow => (SequenceFlow)flow);

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

        public sealed class AddTaskArguments
        {
            public string Guid { get; }

            public string Name { get; }

            public AddTaskArguments(Guid guid, string name) : this(guid.ToString(), name) { }

            public AddTaskArguments(string guid, string name)
            {
                Guid = guid;
                Name = name;
            }
        }
    }
}
