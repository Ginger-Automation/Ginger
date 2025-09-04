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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.Common.ReRunControlLib;
using Amdocs.Ginger.Common.SelfHealingLib;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ginger.Run
{
    public class RunSetConfig : RepositoryItemBase
    {
        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                //
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        /// <summary>
        /// List of all the agents and the Virtual ones mapped during run with runner guid
        /// </summary>
        public Dictionary<Guid, List<IAgent>> ActiveAgentListWithRunner = [];


        private bool mIsRunning;
        public bool IsRunning
        {
            get { return mIsRunning; }
            set
            {
                if (mIsRunning != value)
                {
                    mIsRunning = value;
                    OnPropertyChanged(nameof(IsRunning));
                }
            }
        }


        private string mDescription;
        [IsSerializedForLocalRepository]
        [AllowUserToEdit("Description")]
        public string Description
        {
            get { return mDescription; }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }


        private string mSealightsTestStage;
        [IsSerializedForLocalRepository]
        public string SealightsTestStage
        {
            get { return mSealightsTestStage; }
            set
            {
                if (mSealightsTestStage != value)
                {
                    mSealightsTestStage = value;
                    OnPropertyChanged(nameof(SealightsTestStage));
                }
            }
        }

        private string mSealightsLabId;
        [IsSerializedForLocalRepository]
        public string SealightsLabId
        {
            get { return mSealightsLabId; }
            set
            {
                if (mSealightsLabId != value)
                {
                    mSealightsLabId = value;
                    OnPropertyChanged(nameof(SealightsLabId));
                }
            }
        }

        private string mSealightsBuildSessionID;
        [IsSerializedForLocalRepository]
        public string SealightsBuildSessionID
        {
            get { return mSealightsBuildSessionID; }
            set
            {
                if (mSealightsBuildSessionID != value)
                {
                    mSealightsBuildSessionID = value;
                    OnPropertyChanged(nameof(SealightsBuildSessionID));
                }
            }
        }

        private bool mSealightsTestRecommendationsRunsetOverrideFlag;
        public bool SealightsTestRecommendationsRunsetOverrideFlag
        {
            get { return mSealightsTestRecommendationsRunsetOverrideFlag; }
            set
            {
                if (mSealightsTestRecommendationsRunsetOverrideFlag != value)
                {
                    mSealightsTestRecommendationsRunsetOverrideFlag = value;
                    OnPropertyChanged(nameof(mSealightsTestRecommendationsRunsetOverrideFlag));
                }
            }
        }

        private Configurations.SealightsConfiguration.eSealightsTestRecommendations mSealightsTestRecommendations = Configurations.SealightsConfiguration.eSealightsTestRecommendations.No;
        [IsSerializedForLocalRepository]
        public Configurations.SealightsConfiguration.eSealightsTestRecommendations SealightsTestRecommendations
        {
            get { return mSealightsTestRecommendations; }
            set
            {
                if (mSealightsTestRecommendations != value)
                {
                    mSealightsTestRecommendations = value;
                    OnPropertyChanged(nameof(mSealightsTestRecommendations));
                }
            }
        }

        //-------------------



        private string mRunDescription;
        /// <summary>
        /// Used by the user to describe the logic of the Runset run with a specific set of variables values
        /// </summary>
        [IsSerializedForLocalRepository]
        public string RunDescription
        {
            get
            {
                return mRunDescription;
            }
            set
            {
                if (mRunDescription != value)
                {
                    mRunDescription = value;
                    OnPropertyChanged(nameof(RunDescription));
                }
            }
        }

        private Guid? mExecutionID;
        public Guid? ExecutionID
        {
            get { return mExecutionID; }
            set
            {
                if (mExecutionID != value)
                {
                    mExecutionID = value;
                    OnPropertyChanged(nameof(ExecutionID));
                }
            }
        }

        private string mExecutionServiceURLUsed;

        public string GetExecutionServiceURLUsed() => mExecutionServiceURLUsed;

        // Only for Run time, no need to serialize        
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        public double? Elapsed { get; set; }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus runSetExecutionStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunSetExecutionStatus
        {
            get
            {

                if (GingerRunners.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if (GingerRunners.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if (GingerRunners.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if (GingerRunners.Count(x => x.Status is Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed or Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped) == GingerRunners.Count)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
                else
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                }
            }
            set
            {
                runSetExecutionStatus = value;
            }
        }

        private ObservableList<GingerRunner> mGingerRunners;
        /// <summary>
        /// Been used to identify if Activity Variables were lazy loaded already or not
        /// </summary>
        public bool GingerRunnersLazyLoad { get { return (mGingerRunners != null) && mGingerRunners.LazyLoad; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<GingerRunner> GingerRunners
        {
            get
            {
                if (mGingerRunners == null)
                {
                    mGingerRunners = [];
                }
                if (mGingerRunners.LazyLoad)
                {
                    CheckIfLazyLoadInfoNeedsUpdate();
                    mGingerRunners.LoadLazyInfo();
                    if (this.DirtyStatus != eDirtyStatus.NoTracked)
                    {
                        this.TrackObservableList(mGingerRunners);
                    }
                }
                return mGingerRunners;
            }
            set
            {
                mGingerRunners = value;
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<RunSetActionBase> RunSetActions = [];

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = [];

        public override string GetNameForFileName() { return Name; }

        public string LastRunsetLoggerFolder { get; set; }
        public bool RunsetExecLoggerPopulated
        {
            get
            {
                if (System.IO.Directory.Exists(LastRunsetLoggerFolder))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        //TODO: remove make obsolete and use Run Set Actions only 
        /// <summary>
        /// DO_NOT_USE
        /// </summary>
        //public bool SendEmail { get; set; }
        /// <summary>
        /// DO_NOT_USE
        /// </summary>
        //public Email Email { get; set; }

        public bool mRunModeParallel = true;
        [IsSerializedForLocalRepository(true)]
        public bool RunModeParallel
        {
            get
            {
                return mRunModeParallel;
            }
            set
            {
                if (mRunModeParallel != value)
                {
                    mRunModeParallel = value;
                    OnPropertyChanged(nameof(this.RunModeParallel));
                }
            }
        }

        public bool mStopRunnersOnFailure = false;
        [IsSerializedForLocalRepository]
        public bool StopRunnersOnFailure
        {
            get
            {
                return mStopRunnersOnFailure;
            }
            set
            {
                if (mStopRunnersOnFailure != value)
                {
                    mStopRunnersOnFailure = value;
                    OnPropertyChanged(nameof(this.StopRunnersOnFailure));
                }
            }
        }

        public bool mRunWithAnalyzer = true;
        [IsSerializedForLocalRepository(true)]
        [AllowUserToEdit("Run with Analyzer")]
        public bool RunWithAnalyzer
        {
            get
            {
                return mRunWithAnalyzer;
            }
            set
            {
                if (mRunWithAnalyzer != value)
                {
                    mRunWithAnalyzer = value;
                    OnPropertyChanged(nameof(this.RunWithAnalyzer));
                }
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

        public override string GetItemType()
        {
            return nameof(RunSetConfig);
        }

        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.RunSet;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }

        private bool _isVirtual;
        public bool IsVirtual
        {
            get => _isVirtual;
            set
            {
                if (_isVirtual != value)
                {
                    _isVirtual = value;
                    OnPropertyChanged(nameof(IsVirtual));
                }
            }
        }

        public void UpdateRunnersBusinessFlowRunsList()
        {
            foreach (GingerRunner GR in GingerRunners)
            {
                if (GR.Executor != null && GR.Executor.IsUpdateBusinessFlowRunList)
                {
                    GR.Executor.UpdateBusinessFlowsRunList();
                }
            }
        }

        public override void UpdateBeforeSave()
        {
            UpdateRunnersBusinessFlowRunsList();
            base.UpdateBeforeSave();
        }

        public Action DynamicPostSaveHandler;

        public override void PostSaveHandler()
        {
            base.PostSaveHandler();
            DynamicPostSaveHandler?.Invoke();
        }

        [IsSerializedForLocalRepository]
        public ObservableList<SolutionCategoryDefinition> CategoriesDefinitions = [];

        public ObservableList<SolutionCategoryDefinition> MergedCategoriesDefinitions
        {
            get 
            {
                return General.MergeCategories(CategoriesDefinitions);
            }
            set 
            {
                General.UpdateStoredCategories(CategoriesDefinitions, value);
            }
        }

        [IsSerializedForLocalRepository]
        public SelfHealingConfig SelfHealingConfiguration { get; set; } = new SelfHealingConfig();

        private void CheckIfLazyLoadInfoNeedsUpdate()
        {
            string folderName = this.ContainingFolderFullPath;
            if (mGingerRunners != null && mGingerRunners.LazyLoadDetails != null && !string.IsNullOrEmpty(mGingerRunners.LazyLoadDetails.XmlFilePath))
            {
                string previousFilePath = mGingerRunners.LazyLoadDetails.XmlFilePath;
                string directoryName = Path.GetDirectoryName(previousFilePath);
                if (!directoryName.Equals(folderName))
                {
                    string fileName = Path.GetFileName(previousFilePath);
                    mGingerRunners.LazyLoadDetails.XmlFilePath = Path.Combine(folderName, fileName);
                }
            }
        }
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound)
            {
                if (name == "SealighsLabId")
                {
                    this.mSealightsLabId = value;
                    return true;
                }
                if (name == "SealighsBuildSessionID")
                {
                    this.mSealightsBuildSessionID = value;
                    return true;
                }
                if (string.Equals("ExecutionServiceURLUsed", name))
                {
                    this.mExecutionServiceURLUsed = value;
                    return true;
                }
            }
            return false;
        }

        public ReRunConfig ReRunConfigurations = new ReRunConfig();

        public bool AllowInterActivityFlowControls { get; set; } = true;

        //adding source app and user field for account level report 
        public string SourceApplication { get; set; }
        public string SourceApplicationUser { get; set; }

        public List<Guid> AutoUpdatedPOMList { 
            get; 
            set; }

    }
}
