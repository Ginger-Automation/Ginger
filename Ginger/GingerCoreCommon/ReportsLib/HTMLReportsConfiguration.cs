#region License
/*
Copyright © 2014-2023 European Support Limited

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
                if (mLimitReportFolderSize != value)
                {
                    mLimitReportFolderSize = value;
                    OnPropertyChanged(nameof(LimitReportFolderSize));
                }
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
                if (mHTMLReportsFolder != value)
                {
                    mHTMLReportsFolder = value;
                    OnPropertyChanged(nameof(HTMLReportsFolder));
                }
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
                if (mHTMLReportsAutomaticProdIsEnabled != value)
                {
                    mHTMLReportsAutomaticProdIsEnabled = value;
                    OnPropertyChanged(nameof(HTMLReportsAutomaticProdIsEnabled));
                }
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
                if (mHTMLReportConfigurationMaximalFolderSize != value)
                {
                    mHTMLReportConfigurationMaximalFolderSize = value;
                    OnPropertyChanged(nameof(HTMLReportConfigurationMaximalFolderSize));
                }
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
                if (mHTMLReportTemplatesSeq != value)
                {
                    mHTMLReportTemplatesSeq = value;
                    OnPropertyChanged(nameof(HTMLReportTemplatesSeq));
                }
            }
        }

        private string mCentralizedReportDataServiceURL;
        [IsSerializedForLocalRepository]
        public string CentralizedReportDataServiceURL
        {
            get
            {
                return mCentralizedReportDataServiceURL;
            }
            set
            {
                if (mCentralizedReportDataServiceURL != value)
                {
                    mCentralizedReportDataServiceURL = value;
                    OnPropertyChanged(nameof(CentralizedReportDataServiceURL));
                }
            }
        }

        private string mCentralizedHtmlReportServiceURL;
        [IsSerializedForLocalRepository]
        public string CentralizedHtmlReportServiceURL
        {
            get
            {
                return mCentralizedHtmlReportServiceURL;
            }
            set
            {
                if (mCentralizedHtmlReportServiceURL != value)
                {
                    mCentralizedHtmlReportServiceURL = value;
                    OnPropertyChanged(nameof(CentralizedHtmlReportServiceURL));
                }
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
                if (_HTMLReportsConfigurationSetName != value)
                {
                    _HTMLReportsConfigurationSetName = value;
                    OnPropertyChanged(nameof(_HTMLReportsConfigurationSetName));
                }
            }
        }
    }
}
