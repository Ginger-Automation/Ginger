using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class Agent
    {
        [XmlAttribute]
        public string ApplicationName { get; set; }

        [XmlAttribute]
        public string AgentName { get; set; }
    }
}
