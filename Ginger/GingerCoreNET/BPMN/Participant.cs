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
        
        public string Name { get; }
        
        public Process Process { get; }

        public Participant(Guid guid, string name) : this(guid.ToString(), name) { }

        public Participant(string guid, string name)
        {
            Guid = guid;
            Id = $"participant_{Guid}";
            Name = name;
            Process = new Process(Id);
        }
    }
}
