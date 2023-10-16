using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class BPMNExportException : Exception
    {
        public BPMNExportException(string message) : base(message) { }
    }
}
