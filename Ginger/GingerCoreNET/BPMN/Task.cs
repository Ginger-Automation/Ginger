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

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public class Task : IFlowSource, IFlowTarget
    {
        public string Guid { get; }

        public string Id { get; }

        public string ProcessId { get; }

        public string Name { get; }

        public FlowCollection IncomingFlows { get; }

        public FlowCollection OutgoingFlows { get; }

        public Task(string processId, Guid guid, string name) : this(processId, guid.ToString(), name) { }

        public Task(string processId, string guid, string name)
        {
            Guid = guid;
            Id = $"task_{Guid}";
            Name = name;
            ProcessId = processId;
            IncomingFlows = new();
            OutgoingFlows = new();
        }

        public static TTask Create<TTask>(string processId, Guid guid, string name) where TTask : Task
        {
            return Create<TTask>(processId, guid.ToString(), name);
        }

        public static TTask Create<TTask>(string processId, string guid, string name) where TTask : Task
        {
            Type taskType = typeof(TTask);
            Task newTask;
            if (taskType == typeof(UserTask))
            {
                newTask = new UserTask(processId, guid, name);
            }
            else if (taskType == typeof(ReceiveTask))
            {
                newTask = new ReceiveTask(processId, guid, name);   
            }
            else if (taskType == typeof(ScriptTask))
            {
                newTask = new ScriptTask(processId, guid, name);
            }
            else if (taskType == typeof(SendTask))
            {
                newTask = new SendTask(processId, guid, name);
            }
            else if (taskType == typeof(ServiceTask))
            {
                newTask = new ServiceTask(processId, guid, name);
            }
            else if (taskType == typeof(ManualTask))
            {
                newTask = new ManualTask(processId, guid, name);
            }
            else
            {
                newTask = new Task(processId, guid, name);
            }

            return (TTask)newTask;
        }
    }
}
