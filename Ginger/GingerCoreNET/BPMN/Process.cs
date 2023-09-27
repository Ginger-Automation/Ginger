#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

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
