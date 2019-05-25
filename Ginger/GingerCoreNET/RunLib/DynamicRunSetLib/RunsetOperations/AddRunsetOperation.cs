using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    [XmlInclude(typeof(MailReport))]
    public class AddRunsetOperation
    {
        [XmlAttribute]
        public string Condition { get; set; } = "AlwaysRun";

        [XmlAttribute]
        public string RunAt { get; set; } = "ExecutionEnd";
    }
}
