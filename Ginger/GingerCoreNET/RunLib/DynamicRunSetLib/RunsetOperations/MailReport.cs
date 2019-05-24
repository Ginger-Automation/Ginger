using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{    
    public class MailReport : AddRunsetOperation
    {
        public string MailFrom { get; set; }
        public string MailTo { get; set; }
        public string MailCC { get; set; }

        public string Subject { get; set; }
        public string Comments { get; set; }

        public bool SendViaOutlook { get; set; }

        public SmtpDetails SmtpDetails { get; set; }

        public bool IncludeAttachmentReport { get; set; } = true;
    }
}
