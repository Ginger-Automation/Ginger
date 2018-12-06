using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ginger.Reports
{
    public class HTMLReportsConfiguration : RepositoryItemBase
    {



        [IsSerializedForLocalRepository]
        public long Seq { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }


        private bool mLimitReportFolderSize;
        [IsSerializedForLocalRepository]
        public bool LimitReportFolderSize
        {
            get
            {
                return mLimitReportFolderSize;
            }
            set
            {
                mLimitReportFolderSize = value;
                OnPropertyChanged(nameof(LimitReportFolderSize));
            }
        }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }


        private string mHTMLReportsFolder;
        [IsSerializedForLocalRepository]
        public string HTMLReportsFolder
        {
            get
            {
                return mHTMLReportsFolder;
            }
            set
            {
                mHTMLReportsFolder = value;
                OnPropertyChanged(nameof(HTMLReportsFolder));
            }
        }

        private bool mHTMLReportsAutomaticProdIsEnabled;
        [IsSerializedForLocalRepository]
        public bool HTMLReportsAutomaticProdIsEnabled
        {
            get
            {
                return mHTMLReportsAutomaticProdIsEnabled;
            }
            set
            {
                mHTMLReportsAutomaticProdIsEnabled = value;
                OnPropertyChanged(nameof(HTMLReportsAutomaticProdIsEnabled));
            }
        }

        private long mHTMLReportConfigurationMaximalFolderSize;
        [IsSerializedForLocalRepository]
        public long HTMLReportConfigurationMaximalFolderSize
        {
            get
            {
                return mHTMLReportConfigurationMaximalFolderSize;
            }
            set
            {
                mHTMLReportConfigurationMaximalFolderSize = value;
                OnPropertyChanged(nameof(HTMLReportConfigurationMaximalFolderSize));
            }
        }

        private int mHTMLReportTemplatesSeq;

        [IsSerializedForLocalRepository]
        public int HTMLReportTemplatesSeq
        {
            get
            {
                return mHTMLReportTemplatesSeq;
            }
            set
            {
                mHTMLReportTemplatesSeq = value;
                OnPropertyChanged(nameof(HTMLReportTemplatesSeq));
            }
        }

        private string _HTMLReportsConfigurationSetName = string.Empty;

        public override string ItemName
        {
            get
            {
                return _HTMLReportsConfigurationSetName;
            }
            set
            {
                _HTMLReportsConfigurationSetName = value;
                OnPropertyChanged(nameof(_HTMLReportsConfigurationSetName));
            }
        }
    }
}
