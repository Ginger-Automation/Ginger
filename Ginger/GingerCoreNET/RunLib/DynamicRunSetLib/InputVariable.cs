using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class InputVariable
    {
        [XmlAttribute]
        public string VariableParentType;

        [XmlAttribute]
        public string VariableParentName;

        [XmlAttribute]
        public string VariableName;

        [XmlAttribute]
        public string VariableValue;
    }
}
