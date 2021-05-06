#region License
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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
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

        public override void PostDeserialization()
        {

            //For BusinessFlowCustomizedRunVariables the output variable mappedvalue was storing only variable GUID
            //But if there 2 variables with same name then users were not able to map it to the desired instance 
            //So mappedValue for output variable type mapping was enhanced to store the BusinessFlowInstanceGUID_VariabledGuid
            //Below code is for backward support for old runset with output variable mapping having only guid.
            List<BusinessFlowRun> previousBusinessFlowRuns = new List<BusinessFlowRun>();

            foreach (GingerRunner gingerRunner in this.GingerRunners)
            {
                foreach (BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    foreach (VariableBase var in businessFlowRun.BusinessFlowCustomizedRunVariables)
                    {
                        try
                        {
                            if (var.MappedOutputType == VariableBase.eOutputType.OutputVariable && !var.MappedOutputValue.Contains("_"))
                            {
                                for (int i = previousBusinessFlowRuns.Count - 1; i >= 0; i--)//doing in reverse for sorting by latest value in case having the same var more than once
                                {
                                    Guid guid = previousBusinessFlowRuns[i].BusinessFlowGuid;
                                    BusinessFlow bf = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(guid);

                                    if (bf.GetBFandActivitiesVariabeles(false, false, true).Where(x => x.Guid.ToString() == var.MappedOutputValue).FirstOrDefault() != null)
                                    {
                                        var.MappedOutputValue = previousBusinessFlowRuns[i].BusinessFlowInstanceGuid + "_" + var.MappedOutputValue;
                                        break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.WARN,"Exception occured during post serialize operation of runset", ex);
                        }
                    }
                    previousBusinessFlowRuns.Add(businessFlowRun);
                }
            }
        }
    }
}
