using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class ReceiveTask : Task
    {
        public ReceiveTask(string processId, Guid guid, string name) :
           this(processId, guid.ToString(), name) { }

        public ReceiveTask(string processId, string guid, string name) : 
            base(processId, guid, name) { }
    }
}
