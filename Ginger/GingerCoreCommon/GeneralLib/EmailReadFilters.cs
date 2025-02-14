#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
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
        public bool ReadUnread { get; set; }
        public int ReadCount { get; set; }
        public bool MarkRead { get; set; }

        public bool Equals(EmailReadFilters other)
        {
            return base.Equals(other);
        }
    }
}
