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

using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using Ginger.Configurations;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.SealightsExecutionLogger
{
    public class SealightsReportExecutionLogger : RunListenerBase
    {
        public Context mContext;
        private long StartTime;
        private long EndTime;
        private bool RunningInRunsetMode = false;

        private IEnumerable<dynamic> statusItemList;

        private SealightsReportApiHandler mSealightsApiHandler;
        public SealightsReportApiHandler SealightsReportApiHandler
        {
            get
            {
                if (mSealightsApiHandler == null)
                {
                    mSealightsApiHandler = new SealightsReportApiHandler(mContext);
                }
                return mSealightsApiHandler;
            }
        }

        public SealightsReportExecutionLogger(Context context)
        {
            statusItemList = new[]
                {
                    new { Status = "NA", Type = "Skipped" },
                    new { Status = "Pending", Type = "Skipped" },
                    new { Status = "Skipped", Type = "Skipped" },
                    new { Status = "Wait", Type = "Skipped" },
                    new { Status = "Blocked", Type = "Skipped" },

                    new { Status = "Started", Type = "Failed" },
                    new { Status = "Running", Type = "Failed" },
                    new { Status = "Canceling", Type = "Failed" },
                    new { Status = "Stopped", Type = "Failed" },
                    new { Status = "Failed", Type = "Failed" },

                    new { Status = "FailIgnored", Type = "Passed" },
                    new { Status = "Passed", Type = "Passed" },
                    new { Status = "Completed", Type = "Passed" },
                    new { Status = "Automated", Type = "Passed" },
                };

            mContext = context;
        }

        #region RunSet
        public void RunSetStart(RunSetConfig runsetConfig)
        {
            try
            {
                RunningInRunsetMode = true;

                Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, "Sealights Session Creation");

                SealightsReportApiHandler.SendCreationTestSessionToSealightsAsync();
            }
            catch (Exception err)
            {
                string error = err.Message;
            }
        }



        public async Task RunSetEnd(RunSetConfig runsetConfig)
        {
            RunningInRunsetMode = false;

            await SealightsReportApiHandler.SendDeleteSessionToSealightsAsync(); // Delete Sealights session
        }

        #endregion RunSet   


        #region Runner
        public override async void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            //await RunnerRunStartTask(gingerRunner);
        }

        public override async void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            if (!gingerRunner.Active || gingerRunner.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
        }

        #endregion Runner

        #region BusinessFlow
        public override async void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            //await BusinessFlowStartTask(businessFlow);
        }



        public override async void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            if (RunningInRunsetMode == false && SealightsReportApiHandler.TestSessionId == null) // this will make sure not reporting to Sealight for partial execution done from Run set page
            {
                return;
            }

            if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel == SealightsConfiguration.eSealightsEntityLevel.BusinessFlow)
            {
                if (businessFlow.RunStatus == Execution.eRunStatus.Skipped) // We are tracking the 'skipped' seperatly (in NotifyOnSkippedRunnerEntities(..))
                {
                    return;
                }

                var statusItem = statusItemList.ToList().Where(a => a.Status == businessFlow.RunStatus.ToString()).ToList();

                if (statusItem.Count > 0)
                {
                    await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(businessFlow.Name, businessFlow.StartTimeStamp, businessFlow.EndTimeStamp, statusItem[0].Type);
                }
            }
        }

        public override async void BusinessFlowSkipped(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SealightsReportedEntityLevel == eSealightsEntityLevel.BusinessFlow)
            {
                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(businessFlow.Name, businessFlow.StartTimeStamp, businessFlow.EndTimeStamp, "Skipped");
            }
        }

        #endregion BusinessFlow

        #region Activity
        public override async void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
        }


        public override async void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            if (RunningInRunsetMode == false && SealightsReportApiHandler.TestSessionId == null) // this will make sure not reporting to Sealight for partial execution done from Run set page
            {
                return;
            }

            if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel == SealightsConfiguration.eSealightsEntityLevel.Activity)
            {
                if (!activity.Active || activity.Status == Execution.eRunStatus.Blocked || activity.Status == Execution.eRunStatus.Skipped) // We are tracking the 'skipped' seperatly (in NotifyOnSkippedRunnerEntities(..))
                {
                    return;
                }

                var statusItem = statusItemList.ToList().Where(a => a.Status == activity.Status.ToString()).ToList();

                if (statusItem.Count > 0)
                {
                    await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activity.ActivityName, activity.StartTimeStamp, activity.EndTimeStamp, statusItem[0].Type);
                }
            }
        }



        #endregion Activity

        #region Activity Group 
        public override async void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            //await ActivityGroupStartTask(activityGroup);
        }

        public override async void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            if (RunningInRunsetMode == false && SealightsReportApiHandler.TestSessionId == null) // this will make sure not reporting to Sealight for partial execution done from Run set page
            {
                return;
            }

            if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel == SealightsConfiguration.eSealightsEntityLevel.ActivitiesGroup)
            {
                if (activityGroup.RunStatus == Common.InterfacesLib.eActivitiesGroupRunStatus.Blocked || activityGroup.RunStatus == Common.InterfacesLib.eActivitiesGroupRunStatus.Skipped) // We are tracking the 'skipped' seperatly (in NotifyOnSkippedRunnerEntities(..))
                {
                    return;
                }

                var statusItem = statusItemList.ToList().Where(a => a.Status == activityGroup.RunStatus.ToString()).ToList();

                if (statusItem.Count > 0)
                {
                    await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activityGroup.Name, activityGroup.StartTimeStamp, activityGroup.EndTimeStamp, statusItem[0].Type);
                }
            }
        }

        public override async void ActivityGroupSkipped(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SealightsReportedEntityLevel == eSealightsEntityLevel.ActivitiesGroup)
            {
                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activityGroup.Name, activityGroup.StartTimeStamp, activityGroup.EndTimeStamp, "Skipped");
            }
        }

        #endregion Activity Group

        #region Action
        public override async void ActionStart(uint eventTime, Act action)
        {
        }


        public override async void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            if (!action.Active || action.ExecutionId == Guid.Empty || action.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
        }


        #endregion Action  

        public override async void ActivitySkipped(uint eventTime, Activity activity, bool offlineMode = false)
        {
            if (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel == SealightsConfiguration.eSealightsEntityLevel.Activity)
            {
                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activity.ActivityName, activity.StartTimeStamp, activity.EndTimeStamp, "Skipped");
            }
        }
    }
}