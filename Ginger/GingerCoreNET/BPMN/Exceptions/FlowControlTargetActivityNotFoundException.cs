using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Exceptions
{
    public sealed class FlowControlTargetActivityNotFoundException : BPMNConversionException
    {
        public FlowControlTargetActivityNotFoundException(string message) : base(message) { }
    }
}
