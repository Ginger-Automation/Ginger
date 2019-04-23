using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    
    public class AddBusinessFlow
    {        
        [XmlAttribute]
        public string Name { get; set; }

        public List<SetBusinessFlowVariable> Variables { get; set; } = new List<SetBusinessFlowVariable>();
    }
}
