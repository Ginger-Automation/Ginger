using System.Xml.Serialization;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace Amdocs.Ginger.CoreNET.DiameterLib
{
    public class DiameterAvpDictionaryItem
    {
        [XmlAttribute("code")]
        public int Code { get; set; }
        [XmlAttribute("name")]
        public string Name { get; set; }
        [XmlAttribute("isMandatory")]
        public bool IsMandatory { get; set; }
        [XmlAttribute("isVendorSpecific")]
        public bool IsVendorSpecific { get; set; }
        [XmlAttribute("type")]
        public eDiameterAvpDataType AvpDataType { get; set; }
        [XmlAttribute("enumName")]
        public string? AvpEnumName { get; set; }
    }
}
