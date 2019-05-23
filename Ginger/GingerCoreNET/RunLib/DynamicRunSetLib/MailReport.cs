using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class MailReport
    {
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string Subject { get; set; }       
        public string ReportTemplateName { get; set; }

        public string SmtpPort { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPassword { get; set; }
    }
}
