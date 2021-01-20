namespace GingerCore.ALM.Octane
{
    public class OctaneSettings
    {
        public string CertificateFilePath { get; set; }

        public string CertificatePassword { get; set; }

        public bool IsCertificatePasswordEncrypted { get; set; } = false;
    }
}
