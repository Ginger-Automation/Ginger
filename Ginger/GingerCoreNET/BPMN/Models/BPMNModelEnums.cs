using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN.Models
{
    public enum CollaborationType
    {
        UseCase,
        SubProcess
    }

    public enum EndEventType
    {
        Default,
        Error,
        MessageSend,
        Termination
    }
}
