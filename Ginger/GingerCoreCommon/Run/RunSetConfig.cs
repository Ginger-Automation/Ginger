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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.SolutionCategories;
using Amdocs.Ginger.Common.SelfHealingLib;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.GeneralLib;
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
        /// List of all the agents and the Virtual ones mapped during run 
        /// </summary>
        public List<IAgent> ActiveAgentList = new List<IAgent>();
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
        [IsSerializedForLocalRepository]
        public string ExecutionServiceURLUsed
        {
            get
            {
                return mExecutionServiceURLUsed;
            }
            set
            {
                if (mExecutionServiceURLUsed != value)
                {
                    mExecutionServiceURLUsed = value;
                    OnPropertyChanged(nameof(ExecutionServiceURLUsed));
                }
            }
        }
        // Only for Run time, no need to serialize        
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        public double? Elapsed { get; set; }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus runSetExecutionStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunSetExecutionStatus
        {
            get
            {

                if ((from x in GingerRunners.ToList() where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if ((from x in GingerRunners.ToList() where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if ((from x in GingerRunners.ToList() where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if ((from x in GingerRunners.ToList()
                          where (x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed ||
x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)
                          select x).Count() == GingerRunners.Count)
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
        public bool GingerRunnersLazyLoad { get { return (mGingerRunners != null) ? mGingerRunners.LazyLoad : false; } }
        [IsLazyLoad(LazyLoadListConfig.eLazyLoadType.NodePath)]
        [IsSerializedForLocalRepository]
        public ObservableList<GingerRunner> GingerRunners
        {
            get
            {
                if (mGingerRunners == null)
                {
                    mGingerRunners = new ObservableList<GingerRunner>();
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
        public ObservableList<RunSetActionBase> RunSetActions = new ObservableList<RunSetActionBase>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

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

        [IsSerializedForLocalRepository]
        public ObservableList<SolutionCategoryDefinition> CategoriesDefinitions = new ObservableList<SolutionCategoryDefinition>();

        [IsSerializedForLocalRepository]
        public SelfHealingConfig SelfHealingConfiguration = new SelfHealingConfig();

        public override void PostDeserialization()
        {
            AddCategories();
        }

        public void AddCategories()
        {
            if (CategoriesDefinitions.Count == 0)
            {
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.Product));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.TestType));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.Release));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.Iteration));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.UserCategory1));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.UserCategory2));
                CategoriesDefinitions.Add(new SolutionCategoryDefinition(eSolutionCategories.UserCategory3));
            }
        }
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
            }
            return false;
        }
    }
}
