using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
