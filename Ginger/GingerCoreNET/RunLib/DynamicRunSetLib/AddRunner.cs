using System.Collections.Generic;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class AddRunner
    {
        [XmlAttribute]
        public string Name { get; set; }

        public string Environment { get; set; }

        public string RunMode{ get; set; }        

        public List<SetAgent> SetAgents { get; set; } = new List<SetAgent>();

        public List<AddBusinessFlow> AddBusinessFlows { get; set; } = new List<AddBusinessFlow>();
    }
}
