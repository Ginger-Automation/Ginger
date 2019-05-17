using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class AddRunner
    {
        public string Name { get; set; }

        // TODO: add env if we want to use specific env for runner
        // public string Environemnt { get; set; }

        public List<SetAgent> Agents { get; set; } = new List<SetAgent>();

        public List<AddBusinessFlow> BusinessFlows { get; set; } = new List<AddBusinessFlow>();

        

    }
}
