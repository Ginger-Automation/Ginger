#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore;

namespace Ginger.Reports
{

    /// <summary>
    /// HTML Report General Configrations
    /// </summary>
    
    public class HTMLReportsConfiguration : RepositoryItemBase
    {        

     
        
        [IsSerializedForLocalRepository]
        public long Seq { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }


        private bool mLimitReportFolderSize;
        [IsSerializedForLocalRepository]
        public bool LimitReportFolderSize {
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