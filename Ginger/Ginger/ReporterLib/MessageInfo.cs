using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ReporterLib
{
    public class MessageInfo
    {
        public eUserMsgKey MessageKey { get; set; }

        public string message { get; set; }

        public string caption { get; set; }

        public string keys { get; set; }

        public string status { get; set; }
    }
}
