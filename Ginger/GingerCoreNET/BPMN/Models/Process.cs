#region License
/*
Copyright © 2014-2026 European Support Limited

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
namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public sealed class Process
    {
        private readonly ICollection<IProcessEntity> _childEntities;

        public string Id { get; }

        public StartEvent? StartEvent { get; private set; }

        //TODO: BPMN - StartEvent and EndEvents should also be part of ChildEntities
        public IEnumerable<IProcessEntity> ChildEntities => _childEntities;

        private ICollection<EndEvent> _endEvents;
        public IEnumerable<EndEvent> EndEvents => _endEvents;

        internal Process(string participantId)
        {
            Id = $"{participantId}_process";
            _childEntities = [];
            _endEvents = [];
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
            ExclusiveGateway exclusiveGateway = new(processId: Id, guid, name);
            _childEntities.Add(exclusiveGateway);
            return exclusiveGateway;
        }

        public CallActivity AddCallActivity(string name)
        {
            return AddCallActivity(Guid.NewGuid(), name);
        }

        public CallActivity AddCallActivity(Guid guid, string name)
        {
            CallActivity callActivity = new(processId: Id, guid, name);
            _childEntities.Add(callActivity);
            return callActivity;
        }

        public EndEvent AddEndEvent(string name)
        {
            return AddEndEvent(name, EndEventType.Default);
        }

        public EndEvent AddEndEvent(string name, EndEventType endEventType)
        {
            EndEvent endEvent = new(name, endEventType, processId: Id);
            _endEvents.Add(endEvent);
            return endEvent;
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

            flows = flows.Concat(_childEntities.Where(pe => pe is IFlowTarget).Cast<IFlowTarget>().SelectMany(ft => ft.IncomingFlows));

            flows = flows.Concat(_childEntities.Where(pe => pe is IFlowSource).Cast<IFlowSource>().SelectMany(fs => fs.OutgoingFlows));

            if (StartEvent != null)
            {
                flows = flows.Concat(StartEvent.OutgoingFlows);
            }

            flows = flows.Concat(EndEvents.SelectMany(e => e.IncomingFlows));

            return flows;
        }

        public IEnumerable<TProcessEntity> GetChildEntitiesByType<TProcessEntity>() where TProcessEntity : IProcessEntity
        {
            return _childEntities.Where(entity => entity is TProcessEntity).Cast<TProcessEntity>();
        }
    }
}
