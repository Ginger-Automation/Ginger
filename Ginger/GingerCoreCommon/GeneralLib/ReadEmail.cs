using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCore.GeneralLib
{
    public sealed class ReadEmail
    {
        public string From { get; init; }
        public string Subject { get; init; }
        public string Body { get; init; }
        public DateTime ReceivedDateTime { get; init; }
        public bool HasAttachments { get; init; }
        public IEnumerable<Attachment> Attachments { get; init; }
        
        public sealed class Attachment
        {
            public string Name { get; init; }
            public string ContentType { get; init; }
            public byte[] ContentBytes { get; init; }
        }
    }
}
