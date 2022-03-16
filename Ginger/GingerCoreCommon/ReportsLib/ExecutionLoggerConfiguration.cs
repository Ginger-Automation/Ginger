#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.Common;
using GingerCore;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Reports
{
    public class ExecutionLoggerConfiguration : RepositoryItemBase
    {
        public static partial class Fields
        {
            public static string Parameter = "Parameter";
            public static string Value = "Value";
            public static string Description = "Description";
        }
        public enum AutomationTabContext
        {
            None,
            ActionRun,
            ActivityRun,
            BussinessFlowRun,
            ContinueRun,
            Reset
        }
        public enum DataRepositoryMethod
        {
            TextFile=1,
            LiteDB=0,
            Remote = 2
        }
               
        public enum ePublishToCentralDB
        {
            Yes,
            No
        }

        public enum eSealightsLog
        {
            Yes,
            No
        }

        public enum eDeleteLocalDataOnPublish
        {
            Yes,
            No
        }

        public enum eDataPublishingPhase
        {
            [EnumValueDescription("Post Execution")]
            PostExecution,
            [EnumValueDescription("During Execution")]
            DuringExecution
        }

        // Why we serialzie!!?

        [IsSerializedForLocalRepository]
        public long Seq { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }

        [IsSerializedForLocalRepository]
        public bool ExecutionLoggerConfigurationIsEnabled { get; set; }

        string mExecutionLoggerConfigurationExecResultsFolder;
        [IsSerializedForLocalRepository]
        public string ExecutionLoggerConfigurationExecResultsFolder
        {
            get
            {
                return mExecutionLoggerConfigurationExecResultsFolder;
            }
            set
            {
                mExecutionLoggerConfigurationExecResultsFolder = value;
                OnPropertyChanged(nameof(ExecutionLoggerConfigurationExecResultsFolder));
            }
        }

        public string CalculatedLoggerFolder { get; set; }

        long mExecutionLoggerConfigurationMaximalFolderSize;
        [IsSerializedForLocalRepository]
        public long ExecutionLoggerConfigurationMaximalFolderSize
        {
            get
            {
                return mExecutionLoggerConfigurationMaximalFolderSize;
            }
            set
            {                
                mExecutionLoggerConfigurationMaximalFolderSize = value;
                OnPropertyChanged(nameof(ExecutionLoggerConfigurationMaximalFolderSize));
            }
        }

        private ePublishToCentralDB mPublishLogToCentralDB = ePublishToCentralDB.No;

        [IsSerializedForLocalRepository]
        public ePublishToCentralDB PublishLogToCentralDB 
        { 
            get
            {
                return mPublishLogToCentralDB;
            }
            set
            {
                mPublishLogToCentralDB = value;               
            }
        }


        // Gideon
        private eSealightsLog mSealightsLog = eSealightsLog.No;

        [IsSerializedForLocalRepository]
        public eSealightsLog SealightsLog
        {
            get
            {
                return mSealightsLog;
            }
            set
            {
                mSealightsLog = value;
            }
        }
        // Gideon End

        private eDataPublishingPhase mDataPublishingPhase = eDataPublishingPhase.PostExecution;

        [IsSerializedForLocalRepository]
        public eDataPublishingPhase DataPublishingPhase
        {
            get
            {
                return mDataPublishingPhase;
            }
            set
            {
                mDataPublishingPhase = value;
            }
        }

        private eDeleteLocalDataOnPublish mDeleteLocalDataOnPublish = eDeleteLocalDataOnPublish.No;

        [IsSerializedForLocalRepository]
        public eDeleteLocalDataOnPublish DeleteLocalDataOnPublish
        {
            get
            {
                return mDeleteLocalDataOnPublish;
            }
            set
            {
                mDeleteLocalDataOnPublish = value;
            }
        }


        private string mCentralLoggerEndPointUrl;
        [IsSerializedForLocalRepository]
        public string CentralLoggerEndPointUrl
        {
            get
            {
                return mCentralLoggerEndPointUrl;
            }
            set
            {
                mCentralLoggerEndPointUrl = value;
                OnPropertyChanged(nameof(CentralLoggerEndPointUrl));
            }
        }

        public bool IsPublishToCentralDBRunning { get; set; }

        public string ExecutionLoggerConfigurationHTMLReportsFolder { get; set; }

        public bool ExecutionLoggerHTMLReportsAutomaticProdIsEnabled { get; set; }

        public ObservableList<IHTMLReportConfiguration> temporaryPlacedHTMLReportConfigurationList = new ObservableList<IHTMLReportConfiguration>();

        public AutomationTabContext ExecutionLoggerAutomationTabContext { get; set; }

        private string _ExecutionLoggerConfigurationSetName = string.Empty;
        private DataRepositoryMethod mDataRepositoryMethod;
        [IsSerializedForLocalRepository]
        public DataRepositoryMethod SelectedDataRepositoryMethod
        {
            get { return mDataRepositoryMethod; }
            set
            {
                if (mDataRepositoryMethod != value)
                {
                    mDataRepositoryMethod = value;                    
                    OnPropertyChanged(nameof(mDataRepositoryMethod));
                }
            }
        }
        public override string ItemName
        {
            get
            {
                return _ExecutionLoggerConfigurationSetName;
            }
            set
            {
                _ExecutionLoggerConfigurationSetName = value;
            }
        }

        #region General

        #endregion
    }
}
