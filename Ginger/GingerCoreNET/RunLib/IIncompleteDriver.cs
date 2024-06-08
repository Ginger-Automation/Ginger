using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public interface IIncompleteDriver
    {
        public bool IsActionSupported(Act act, out string message);
    }
}
