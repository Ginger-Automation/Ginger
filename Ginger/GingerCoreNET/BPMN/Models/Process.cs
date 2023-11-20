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

using MongoDB.Driver.Core.Operations;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class Process
    {
        private readonly ICollection<IProcessEntity> _childEntities;

        public string Id { get; }

        public StartEvent? StartEvent { get; private set; }

        public IEnumerable<IProcessEntity> ChildEntities => _childEntities;

        public EndEvent? EndEvent { get; private set; }

        internal Process(string participantId)
        {
            Id = $"{participantId}_process";
            _childEntities = new List<IProcessEntity>();
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
            return AddTask<TTask>(guid, name, conditions: Array.Empty<Task.Condition>());
        }

        public TTask AddTask<TTask>(string name, IEnumerable<Task.Condition> conditions) where TTask : Task
        {
            return AddTask<TTask>(Guid.NewGuid(), name, conditions);
        }

        public TTask AddTask<TTask>(Guid guid, string name, IEnumerable<Task.Condition> conditions) where TTask : Task
        {
            TTask task = Task.Create<TTask>(processId: Id, guid, name, conditions);
            _childEntities.Add(task);
            return task;
        }

        public ExclusiveGateway AddExclusiveGateway(string name)
        {
            return AddExclusiveGateway(Guid.NewGuid(), name);
        }

        public ExclusiveGateway AddExclusiveGateway(Guid guid, string name)
        {
            return AddExclusiveGateway(guid.ToString(), name);
        }

        public ExclusiveGateway AddExclusiveGateway(string guid, string name)
        {
            ExclusiveGateway exclusiveGateway = new(processId: Id, guid, name);
            _childEntities.Add(exclusiveGateway);
            return exclusiveGateway;
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

            flows = flows.Concat(GetChildEntitiesByType<Task>().SelectMany(task => task.IncomingFlows));

            flows = flows.Concat(GetChildEntitiesByType<Task>().SelectMany(task => task.OutgoingFlows));

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

        public IEnumerable<TProcessEntity> GetChildEntitiesByType<TProcessEntity>() where TProcessEntity : IProcessEntity
        {
            return _childEntities.Where(entity => entity is TProcessEntity).Cast<TProcessEntity>();
        }
    }
}
