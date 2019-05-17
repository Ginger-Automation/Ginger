using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{    
    public class DynamicRunSet
    {
        public string Name { get; set; }
        public string Environemnt { get; set; }

        public List<AddRunner> Runners { get; set; } = new List<AddRunner>();
        

    }
}
