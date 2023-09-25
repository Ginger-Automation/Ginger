using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class ScriptTask : Task
    {
        public ScriptTask(string processId, Guid guid, string name) : 
            this(processId, guid.ToString(), name) { }

        public ScriptTask(string processId, string guid, string name) : 
            base(processId, guid, name) { }
    }
}
