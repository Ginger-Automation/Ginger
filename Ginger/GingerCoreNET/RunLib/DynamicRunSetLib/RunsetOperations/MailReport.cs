using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class MailReport : RunsetOperationBase
    {
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string MailCC { get; set; }

        public string Subject { get; set; }
        public string Comments { get; set; }

        public bool SendViaOutlook { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
        public string SmtpEnableSSL { get; set; }

        public bool IncludeAttachmentReport { get; set; } = true;
    }
}
