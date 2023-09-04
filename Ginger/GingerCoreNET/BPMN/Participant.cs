using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class Participant
    {
        public string Guid { get; }

        public string Id { get; }
        
        public string Name { get; set; }

        public string SystemRef { get; set; }
        
        public Process Process { get; }

        public Participant(Guid guid) : this(guid.ToString()) { }

        public Participant(string guid)
        {
            Guid = guid;
            Id = $"participant_{Guid}";
            Name = string.Empty;
            SystemRef = string.Empty;
            Process = new Process(Id);
        }
    }
}
