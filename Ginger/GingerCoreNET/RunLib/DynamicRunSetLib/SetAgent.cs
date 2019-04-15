using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class SetAgent
    {
        [XmlAttribute]
        public string TargetApplication { get; set; }

        [XmlAttribute]
        public string Agent { get; set; }

    }
}
