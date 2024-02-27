using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Exceptions
{
    public class BPMNExportationException : BPMNException
    {
        public BPMNExportationException(string message) : base(message) { }
    }
}
