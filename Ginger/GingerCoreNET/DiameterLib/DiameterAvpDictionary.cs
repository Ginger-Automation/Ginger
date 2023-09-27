using Amdocs.Ginger.Common;
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
