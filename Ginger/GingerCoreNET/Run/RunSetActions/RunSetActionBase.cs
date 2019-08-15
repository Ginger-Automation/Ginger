#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System.Diagnostics;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.Reports;
using amdocs.ginger.GingerCoreNET;
using GingerCore;

namespace Ginger.Run.RunSetActions
{
    public abstract class RunSetActionBase : RepositoryItemBase
    {
        public enum eRunAt
        {
            [EnumValueDescription("Execution Start")]
            ExecutionStart,
            [EnumValueDescription("Execution End")]
            ExecutionEnd,
            [EnumValueDescription("During Execution")]
            DuringExecution,
        }

        public abstract List<eRunAt> GetRunOptions();

        public abstract bool SupportRunOnConfig { get; }

        public abstract void PrepareDuringExecAction(ObservableList<GingerRunner> Gingers);

        public enum eRunSetActionStatus
        {
            Pending,
            Completed,
            Running,
            Failed
        }

        public enum eRunSetActionCondition
        {
            [EnumValueDescription("Always Run")]
            AlwaysRun,
            [EnumValueDescription("All Business Flows Passed")]
            AllBusinessFlowsPassed,
            [EnumValueDescription("One or More Business Flows Failed")]
            OneOrMoreBusinessFlowsFailed,
        }

        public  static class Fields
        {
            public static string Name = "Name";
            public static string Type = "Type";
            public static string Active = "Active";
            public static string RunAt = "RunAt";
            public static string Elapsed = "Elapsed";
            public static string ElapsedSecs = "ElapsedSecs";
            public static string Status = "Status";
            public static string Condition = "Condition";
            public static string Errors = "Errors";
        }

        public RunSetActionBase()
        {
            //set fields default values
            mStatus = eRunSetActionStatus.Pending;
            Active = true;
        }

        private bool mActive = true;
        [IsSerializedForLocalRepository(true)]
        public Boolean Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(Fields.Active); } } }

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
                    OnPropertyChanged(Fields.Name);
                }
            }
        }

        eRunAt mRunAt;
        [IsSerializedForLocalRepository]
        public eRunAt RunAt
        {
            get { return mRunAt; }
            set
            {
                if (mRunAt != value)
                {
                    mRunAt = value;
                    OnPropertyChanged(Fields.RunAt);
                }
            }
        }

        eRunSetActionStatus mStatus = eRunSetActionStatus.Pending;
        public eRunSetActionStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(Fields.Status);
                }
            }
        }

        private long? mElapsed = null;
        public long? Elapsed
        {
            get { return mElapsed; }
            set
            {
                mElapsed = value;
                OnPropertyChanged(Fields.Elapsed);
                OnPropertyChanged(Fields.ElapsedSecs);
            }
        }

        public Single? ElapsedSecs
        {
            get
            {
                if (Elapsed != null)
                {
                    return ((Single)Elapsed / 1000);
                }
                else
                {
                    return null;
                }
            }
        }

        //TODO: how about using static?
        public string SolutionFolder { get; set; }


        private eRunSetActionCondition mCondition;
        [IsSerializedForLocalRepository]
        public eRunSetActionCondition Condition { get { return mCondition; } set { if (mCondition != value) { mCondition = value; OnPropertyChanged(Fields.Condition); } } }


        private string mErrors;
        [IsSerializedForLocalRepository]
        public string Errors
        {
            get { return mErrors; }
            set
            {
                if (mErrors != value)
                {
                    mErrors = value;
                    OnPropertyChanged(Fields.Errors);
                }
            }
        }

        public abstract void Execute(ReportInfo RI);

        public abstract string GetEditPage();

        public void ExecuteWithRunPageBFES()
        {
            ReportInfo RI = new ReportInfo(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment, WorkSpace.Instance.RunsetExecutor);
            RunAction(RI);
        }

        internal void RunAction(ReportInfo RI)
        {
            Reporter.ToStatus(eStatusMsgKey.ExecutingRunSetAction, null, this.Name);
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("--> Execution Started for {0} Operation from Type '{1}' and Name '{2}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), this.Type, this.Name));
                Status = RunSetActionBase.eRunSetActionStatus.Running;
                Errors = null;

                Stopwatch st = new Stopwatch();
                st.Reset();
                st.Start();
                Execute(RI);
                st.Stop();
                Elapsed = st.ElapsedMilliseconds;

                // we change to completed only if still running and not changed to fail or soemthing else            
                if (Status == eRunSetActionStatus.Running)
                {
                    Status = RunSetActionBase.eRunSetActionStatus.Completed;
                }

                Reporter.ToLog(eLogLevel.DEBUG, string.Format("<-- Execution Ended for {0} Operation from Type '{1}' and Name '{2}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), this.Type, this.Name) + Environment.NewLine
                                                                + "Details:" + Environment.NewLine
                                                                + string.Format("Status= {0}", Status) + Environment.NewLine
                                                                + string.Format("Errors= {0}", Errors) + Environment.NewLine
                                                                + string.Format("Elapsed= {0}", Elapsed));
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("<-- Execution Failed with exception for {0} Operation from Type '{1}' and Name '{2}' Exception: " +ex.Message, GingerDicser.GetTermResValue(eTermResKey.RunSet), this.Type, this.Name), ex);
                Status = RunSetActionBase.eRunSetActionStatus.Failed;
            }
            finally
            {
                Reporter.HideStatusMessage();
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

        public abstract String Type { get; }
    }
}

