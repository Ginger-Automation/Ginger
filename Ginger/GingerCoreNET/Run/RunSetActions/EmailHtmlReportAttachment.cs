using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;

namespace Ginger.Run.RunSetActions
{
    public class EmailHtmlReportAttachment : EmailAttachment
    {
        public new static class Fields
        {

            public static string IsAlternameFolderUsed = "IsAlternameFolderUsed";
            public static string SelectedHTMLReportTemplateID = "SelectedHTMLReportTemplateID";
            public static string IsLinkEnabled = "IsLinkEnabled";
        }

        [IsSerializedForLocalRepository]
        public int SelectedHTMLReportTemplateID { get; set; }

        bool mIsLinkEnabled;
        [IsSerializedForLocalRepository]
        public bool IsLinkEnabled
        {
            get
            {
                return mIsLinkEnabled;
            }
            set
            {
                mIsLinkEnabled = value;
                OnPropertyChanged(Fields.IsLinkEnabled);
            }
        }

        [IsSerializedForLocalRepository]
        public bool IsAlternameFolderUsed { get; set; }
        public override string GetNameForFileName() { return Name; }
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
