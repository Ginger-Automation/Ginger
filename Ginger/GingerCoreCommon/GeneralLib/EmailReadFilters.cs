using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;

namespace GingerCore.GeneralLib
{
    public struct EmailReadFilters : IEquatable<EmailReadFilters>
    {
        public enum eFolderFilter
        {
            [EnumValueDescription("All")]
            All,
            [EnumValueDescription("Specific")]
            Specific
        }

        public enum eHasAttachmentsFilter
        {
            [EnumValueDescription(" ")]
            Either,
            [EnumValueDescription("Yes")]
            Yes,
            [EnumValueDescription("No")]
            No
        }

        public eFolderFilter Folder { get; set; }
        public string FolderNames { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public eHasAttachmentsFilter HasAttachments { get; set; }
        public string AttachmentContentType { get; set; }
        public string AttachmentDownloadPath { get; set; }
        public DateTime ReceivedStartDate { get; set; }
        public DateTime ReceivedEndDate { get; set; }

        public bool Equals(EmailReadFilters other)
        {
            return base.Equals(other);
        }
    }
}
