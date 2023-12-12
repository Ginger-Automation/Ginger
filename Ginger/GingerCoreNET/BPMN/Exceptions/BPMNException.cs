using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Exceptions
{
    public class BPMNException : Exception
    {
        public BPMNException(string message) : base(message) { }
    }
}
