using System.Collections.Generic;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class DynamicGingerExecution
    {        
        public SolutionDetails SolutionDetails { get; set; }

        public bool ShowAutoRunWindow { get; set; }

        public List<AddRunset> AddRunsets { get; set; } = new List<AddRunset>();
    }
}
