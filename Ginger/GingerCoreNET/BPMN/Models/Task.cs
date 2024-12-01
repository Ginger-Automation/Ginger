#region License
/*
Copyright © 2014-2024 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public class Task : IFlowSource, IFlowTarget
    {
        public Guid Guid { get; }

        public string Id { get; }

        public string ProcessId { get; }

        public string Name { get; }

        public IEnumerable<Condition> Conditions { get; }

        public FlowCollection IncomingFlows { get; }

        public FlowCollection OutgoingFlows { get; }

        public Task(string processId, Guid guid, string name) : this(processId, guid, name, conditions: Array.Empty<Condition>()) { }

        public Task(string processId, Guid guid, string name, IEnumerable<Condition> conditions)
        {
            Guid = guid;
            Id = $"task_{Guid}";
            Name = name;
            ProcessId = processId;
            Conditions = new List<Condition>(conditions).AsReadOnly();
            IncomingFlows = [];
            OutgoingFlows = [];
        }

        public static TTask Create<TTask>(string processId, Guid guid, string name) where TTask : Task
        {
            return Create<TTask>(processId, guid, name, conditions: Array.Empty<Condition>());
        }

        public static TTask Create<TTask>(string processId, Guid guid, string name, IEnumerable<Condition> conditions) where TTask : Task
        {
            Type taskType = typeof(TTask);
            Task newTask;
            if (taskType == typeof(UserTask))
            {
                newTask = new UserTask(processId, guid, name, conditions);
            }
            else if (taskType == typeof(ReceiveTask))
            {
                newTask = new ReceiveTask(processId, guid, name, conditions);
            }
            else if (taskType == typeof(ScriptTask))
            {
                newTask = new ScriptTask(processId, guid, name, conditions);
            }
            else if (taskType == typeof(SendTask))
            {
                newTask = new SendTask(processId, guid, name, conditions);
            }
            else if (taskType == typeof(ServiceTask))
            {
                newTask = new ServiceTask(processId, guid, name, conditions);
            }
            else if (taskType == typeof(ManualTask))
            {
                newTask = new ManualTask(processId, guid, name, conditions);
            }
            else
            {
                newTask = new Task(processId, guid, name, conditions);
            }

            return (TTask)newTask;
        }

        public abstract class Condition
        {
            public string NameFieldTag { get; }

            protected Condition(string nameFieldTag)
            {
                NameFieldTag = nameFieldTag;
            }
        }

        public sealed class FieldValueCondition : Condition
        {
            public string ValueFieldTag { get; }

            public FieldValueCondition(string nameFieldTag, string valueFieldTag) : base(nameFieldTag)
            {
                ValueFieldTag = valueFieldTag;
            }
        }

        public sealed class ParameterValueCondition : Condition
        {
            public enum OperationType
            {
                In,
                Override,
                NotIn
            }
            public string ValueParameterName { get; }

            public OperationType Operation { get; }

            public ParameterValueCondition(string nameFieldTag, string valueParameterName, OperationType operation) : base(nameFieldTag)
            {
                ValueParameterName = valueParameterName;
                Operation = operation;
            }

            public static string GetOperationSymbol(OperationType operation)
            {
                return operation switch
                {
                    OperationType.In => "",
                    OperationType.Override => "!",
                    OperationType.NotIn => "~",
                    _ => throw new InvalidOperationException($"No operation symbol is known for {typeof(OperationType).FullName} of type {operation}."),
                };
            }
        }
    }
}
