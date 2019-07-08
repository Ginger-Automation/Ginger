using Amdocs.Ginger.CoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Plugin
{
    interface IActionHandler
    {
         Dictionary<string, object> InputParams { get; set; }
        void ExecuteAction(ref NodePlatformAction platformAction);
        
    }
}
