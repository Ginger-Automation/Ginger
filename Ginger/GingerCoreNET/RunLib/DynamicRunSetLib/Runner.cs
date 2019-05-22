using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class Runner
    {
        public string Name { get; set; }        

        public string Environment { get; set; }

        public string RunMode{ get; set; }        

        public List<Agent> Agents { get; set; } = new List<Agent>();

        public List<BusinessFlow> BusinessFlows { get; set; } = new List<BusinessFlow>();
    }
}
