using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class BPMNConversionException : Exception
    {
        public BPMNConversionException(string message) : base(message) { }
    }
}
