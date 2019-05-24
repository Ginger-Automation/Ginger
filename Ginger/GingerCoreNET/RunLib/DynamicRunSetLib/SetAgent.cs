using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class SetAgent
    {
        [XmlAttribute]
        public string ApplicationName { get; set; }

        [XmlAttribute]
        public string AgentName { get; set; }
    }
}
