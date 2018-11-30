using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Amdocs.Ginger.CoreNET.Run.RunsetActions
{
    public class EmailAttachment : RepositoryItemBase, IRepositoryItem
    {
        public enum eAttachmentType
        {
            [EnumValueDescription("Report")]
            Report,
            [EnumValueDescription("File")]
            File
        }

        public new static class Fields
        {
            public static string AttachmentType = "AttachmentType";
            public static string Name = "Name";
            public static string CalculatedName = "CalculatedName";
            public static string ExtraInformation = "ExtraInformation";
            public static string CalculatedExtraInformation = "CalculatedExtraInformation";
            public static string ZipIt = "ZipIt";
        }

        [IsSerializedForLocalRepository]
        public eAttachmentType AttachmentType { get; set; }

        string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                OnPropertyChanged(Fields.Name);
            }
        }

        public static string CalculatedName { get; set; }
        public static string CalculatedExtraInformation { get; set; }

        //TODO: Check if it is need to serialize or not.
        string mExtraInformation = string.Empty;
        [IsSerializedForLocalRepository]
        public string ExtraInformation
        {
            get
            {
                return mExtraInformation;
            }
            set
            {
                mExtraInformation = value;
                OnPropertyChanged(Fields.ExtraInformation);
            }
        }

        bool mZipit = true;
        [IsSerializedForLocalRepository]
        public bool ZipIt
        {
            get
            {
                if (AttachmentType == eAttachmentType.Report)
                    return true;
                else
                    return mZipit;
            }
            set
            {
                mZipit = value;
                OnPropertyChanged(Fields.ZipIt);
            }
        }

        public bool IsReport
        {
            get
            {
                if (AttachmentType == eAttachmentType.Report)
                    return false;
                else
                    return true;
            }
        }
        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
    }
}