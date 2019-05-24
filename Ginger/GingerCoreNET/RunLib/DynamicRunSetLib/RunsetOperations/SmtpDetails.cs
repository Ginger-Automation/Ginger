using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class SmtpDetails
    {
        [XmlAttribute]
        public string Server { get; set; }

        [XmlAttribute]
        public string Port { get; set; }

        [XmlAttribute]
        public string User { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string EnableSSL { get; set; }
    }
}
