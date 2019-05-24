using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class SourceControlDetails
    {
        [XmlAttribute]
        public string Type { get; set; }

        [XmlAttribute]
        public string Url { get; set; }

        [XmlAttribute]
        public string User { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string PasswordEncrypted { get; set; }

        [XmlAttribute]
        public string ProxyServer { get; set; }

        [XmlAttribute]
        public string ProxyPort { get; set; }
    }
}
