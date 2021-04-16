﻿#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using System;
using System.Collections.Generic;

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
        public List<Agent> ActiveAgentList = new List<Agent>();
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

        private ObservableList<GingerRunner> mGingerRunners;
        /// <summary>
        /// Been used to identify if Activity Variables were lazy loaded already or not
        /// </summary>
        public bool GingerRunnersLazyLoad { get { return (mGingerRunners != null) ? mGingerRunners.LazyLoad : false; } }
        [IsLazyLoad (LazyLoadListConfig.eLazyLoadType.NodePath)]
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

        public string LastRunsetLoggerFolder { get; set;}
        public bool RunsetExecLoggerPopulated
        {
            get
            {
                if(System.IO.Directory.Exists(LastRunsetLoggerFolder))
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
        public bool SendEmail { get; set; }     
        /// <summary>
        /// DO_NOT_USE
        /// </summary>
        public Email Email{ get; set; }

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
                mRunModeParallel = value;
                OnPropertyChanged(nameof(this.RunModeParallel));
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
                mStopRunnersOnFailure = value;
                OnPropertyChanged(nameof(this.StopRunnersOnFailure));
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
                mRunWithAnalyzer = value;
                OnPropertyChanged(nameof(this.RunWithAnalyzer));
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
                if (GR.IsUpdateBusinessFlowRunList)
                {
                    GR.UpdateBusinessFlowsRunList();
                }
            }
        }

        public override void UpdateBeforeSave()
        {
            UpdateRunnersBusinessFlowRunsList();
            base.UpdateBeforeSave();
        }
    }
}
