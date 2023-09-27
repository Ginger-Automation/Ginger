using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    [XmlRoot("DiameterAvps")]
    public class DiameterAvpDictionary
    {
        [XmlElement("Avp")]
        public ObservableList<DiameterAvpDictionaryItem> AvpDictionaryList { get; set; }
    }
}
