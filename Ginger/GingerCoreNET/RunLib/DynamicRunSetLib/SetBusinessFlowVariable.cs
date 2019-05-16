using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class SetBusinessFlowVariable
    {
        [XmlAttribute]
        public string Name;

        [XmlAttribute]
        public string Value;
    }
}
