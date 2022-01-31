using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using GingerCore.Environments;

namespace Ginger.Run
{
    public enum eContinueLevel
    {
        StandalonBusinessFlow,
        Runner
    }
    public enum eContinueFrom
    {
        LastStoppedAction,
        SpecificAction,
        SpecificActivity,
        SpecificBusinessFlow
    }
    public enum eRunLevel
    {
        NA,
        Runner,
        BusinessFlow
    }
    public  class GingerRunner : RepositoryItemBase
    {
        public enum eActionExecutorType
        {
            RunWithoutDriver,
            RunOnDriver,
            RunInSimulationMode,
            RunOnPlugIn
        }

        public IGingerExecutionEngine Executor;
        public enum eRunOptions
        {
            [EnumValueDescription("Continue Business Flows Run on Failure ")]
            ContinueToRunall = 0,
            [EnumValueDescription("Stop Business Flows Run on Failure ")]
            StopAllBusinessFlows = 1,
        }

        public enum eResetStatus
        {
            All,
            FromSpecificActionOnwards,
            FromSpecificActivityOnwards,
        }

        private eRunOptions mRunOption;
        [IsSerializedForLocalRepository]
        public eRunOptions RunOption
        {
            get
            {
                return mRunOption;
            }
            set
            {
                mRunOption = value;
                OnPropertyChanged(nameof(GingerRunner.RunOption));
            }
        }
        [IsSerializedForLocalRepository]
        public ObservableList<IApplicationAgent> ApplicationAgents { get; set; } = new ObservableList<IApplicationAgent>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> FilterExecutionTags = new ObservableList<Guid>();




        [IsSerializedForLocalRepository]
        public int AutoWait { get; set; } = 0;

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _Selected { get; set; }

        [IsSerializedForLocalRepository]
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }

        private bool mActive = true;
        [IsSerializedForLocalRepository(true)]
        public bool Active
        {
            get { return mActive; }
            set
            {
                if (mActive != value)
                {
                    mActive = value;
                    OnPropertyChanged(nameof(GingerRunner.Active));
                }
            }
        }

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus mStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool UseSpecificEnvironment { get; set; }

        string mSpecificEnvironmentName;
        [IsSerializedForLocalRepository]
        public string SpecificEnvironmentName
        {
            get
            {
                return mSpecificEnvironmentName;
            }
            set
            {
                mSpecificEnvironmentName = value;
                OnPropertyChanged(nameof(SpecificEnvironmentName));
            }
        }

        [IsSerializedForLocalRepository]
        public bool FilterExecutionByTags { get; set; }

        ProjEnvironment mProjEnvironment;
        public ProjEnvironment ProjEnvironment
        {
            get
            {
                return mProjEnvironment;
            }
            set
            {
                mProjEnvironment = (ProjEnvironment)value;
                //ExecutionLogger.ExecutionEnvironment = (ProjEnvironment)value;
                Executor.Context.Environment = mProjEnvironment;
                Executor.NotifyEnvironmentChanged();
            }
        }



        public ObservableList<DataSourceBase> DSList { get; set; }


        private bool mRunInSimulationMode;
        [IsSerializedForLocalRepository]
        public bool RunInSimulationMode
        {
            get
            {
                return mRunInSimulationMode;
            }
            set
            {
                mRunInSimulationMode = value;
                OnPropertyChanged(nameof(GingerRunner.RunInSimulationMode));
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<BusinessFlowRun> BusinessFlowsRunList { get; set; } = new ObservableList<BusinessFlowRun>();

        private string mItemName;
        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                mItemName = value;
            }
        }
    }
}
