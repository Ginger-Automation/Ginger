using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    
    public class BusinessFlow
    {        
        [XmlAttribute]
        public string Name { get; set; }

        public List<InputVariable> InputVariables { get; set; } = new List<InputVariable>();
    }
}
