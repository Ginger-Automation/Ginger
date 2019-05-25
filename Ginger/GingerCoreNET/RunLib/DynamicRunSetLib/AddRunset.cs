using System.Collections.Generic;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class AddRunset
    {
        [XmlAttribute]
        public string Name { get; set; }

        public string Environment { get; set; }

        public bool RunAnalyzer { get; set; }

        public bool RunInParallel { get; set; }

        public List<AddRunner> AddRunners { get; set; } = new List<AddRunner>();

        public List<AddRunsetOperation> AddRunsetOperations { get; set; } = new List<AddRunsetOperation>();
    }
}
