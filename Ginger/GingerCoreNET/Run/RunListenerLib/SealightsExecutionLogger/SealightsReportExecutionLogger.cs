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
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.SealightsExecutionLogger
{
    public class SealightsReportExecutionLogger : RunListenerBase
    {
        public Context mContext;
        private long StartTime;
        private long EndTime;

        private SealightsReportApiHandler mSealightsApiHandler;
        public SealightsReportApiHandler SealightsReportApiHandler
        {
            get
            {
                if (mSealightsApiHandler == null)
                {
                    mSealightsApiHandler = new SealightsReportApiHandler();
                }
                return mSealightsApiHandler;
            }
        }

        public SealightsReportExecutionLogger(Context context)
        {
            mContext = context;
        }

        #region RunSet
        public async Task RunSetStart(RunSetConfig runsetConfig)
        {
            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, "Sealights Session Creation");
            if (WorkSpace.Instance.RunsetExecutor != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig != null
                && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID != null)
            {
                runsetConfig.ExecutionID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            }
             await SealightsReportApiHandler.SendCreationTestSessionToSealightsAsync();
        }

    

        public async Task RunSetEnd(RunSetConfig runsetConfig)
        {
            await SealightsReportApiHandler.SendDeleteSessionToSealightsAsync();
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
            await SealightsReportApiHandler.SendDeleteSessionToSealightsAsync();
        }

        #endregion Runner

        #region BusinessFlow
        public override async void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
           //await BusinessFlowStartTask(businessFlow);
        }

        

        public override async void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SealightsReportedEntityLevel == eSealightsEntityLevel.BusinessFlow)
            {
                if (!businessFlow.Active || businessFlow.RunStatus == Execution.eRunStatus.Blocked)
                {
                    return;
                }

                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(businessFlow.Name, businessFlow.StartTimeStamp, businessFlow.EndTimeStamp, businessFlow.Status.ToString());
            }
        }

        #endregion BusinessFlow

        #region Activity
        public override async void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
            //await ActivityStartTask(activity);
        }

      
        public override async void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SealightsReportedEntityLevel == eSealightsEntityLevel.Activity)
            {
                if (!activity.Active || activity.Status == Execution.eRunStatus.Blocked)
                {
                    return;
                }

                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activity.ActivityName, activity.StartTimeStamp, activity.EndTimeStamp, activity.Status.ToString());
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
            if (WorkSpace.Instance.Solution.LoggerConfigurations.SealightsReportedEntityLevel == eSealightsEntityLevel.ActivitiesGroup)
            {
                if (activityGroup.RunStatus == Common.InterfacesLib.eActivitiesGroupRunStatus.Blocked)
                {
                    return;
                }

                await SealightsReportApiHandler.SendingTestEventsToSealightsAsync(activityGroup.Name, activityGroup.StartTimeStamp, activityGroup.EndTimeStamp, activityGroup.RunStatus.ToString());
            }
        }

      

        #endregion Activity Group

        #region Action
        public override async void ActionStart(uint eventTime, Act action)
        {
            //await ActionStartTask(action);
        }

 
        public override async void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            if (!action.Active || action.ExecutionId == Guid.Empty || action.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
            //await SendDataOnActionEndTask(action);
        }


        #endregion Action       
    }
}