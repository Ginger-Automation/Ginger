﻿#region License
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
using static Ginger.Configurations.SealightsConfiguration;

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


        /// <summary>
        /// Do NOT Remove below fields 
        /// Needed for old Ginger Version Solutions 
        /// </summary>
        #region Sealights Data ***---Do NOT Remove---*** 
        private eSealightsLog mSealightsLog = eSealightsLog.No;
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

        private string mSealightsURL;
        public string SealightsURL
        {
            get
            {
                return mSealightsURL;
            }
            set
            {
                mSealightsURL = value;
                OnPropertyChanged(nameof(SealightsURL));
            }
        }


        private string mSealightsAgentToken;
        public string SealightsAgentToken
        {
            get
            {
                return mSealightsAgentToken;
            }
            set
            {
                mSealightsAgentToken = value;
                OnPropertyChanged(nameof(SealightsAgentToken));
            }
        }

        private string mSealightsLabId;
        public string SealightsLabId
        {
            get
            {
                return mSealightsLabId;
            }
            set
            {
                mSealightsLabId = value;
                OnPropertyChanged(nameof(SealightsLabId));
            }
        }

        private string mSealightsTestStage;
        public string SealightsTestStage
        {
            get
            {
                return mSealightsTestStage;
            }
            set
            {
                mSealightsTestStage = value;
                OnPropertyChanged(nameof(SealightsTestStage));
            }
        }

        private string mSealightsBuildSessionID;
        public string SealightsBuildSessionID
        {
            get
            {
                return mSealightsBuildSessionID;
            }
            set
            {
                mSealightsBuildSessionID = value;
                OnPropertyChanged(nameof(SealightsBuildSessionID));
            }
        }        

        private string mSealightsSessionTimeout;
        public string SealightsSessionTimeout
        {
            get
            {
                return mSealightsSessionTimeout;
            }
            set
            {
                mSealightsSessionTimeout = value;
                OnPropertyChanged(nameof(SealightsSessionTimeout));
            }
        }

        private eSealightsEntityLevel mSealightsReportedEntityLevel;
        public eSealightsEntityLevel SealightsReportedEntityLevel
        {
            get
            {
                return mSealightsReportedEntityLevel;
            }
            set
            {
                mSealightsReportedEntityLevel = value;
                OnPropertyChanged(nameof(SealightsReportedEntityLevel));
            }
        }
        #endregion


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
