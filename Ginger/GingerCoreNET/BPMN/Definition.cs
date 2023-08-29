using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.BPMN
{
    public sealed class Definition
    {
        public Collaboration Collaboration { get; }

        public Definition(Collaboration collaboration)
        {
            Collaboration = collaboration;
        }
    }
}
