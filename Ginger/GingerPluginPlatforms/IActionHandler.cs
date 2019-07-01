using Amdocs.Ginger.CoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin
{
    interface IActionHandler
    {
        void ExecuteAction(ref NodePlatformAction platformAction);
        
    }
}
