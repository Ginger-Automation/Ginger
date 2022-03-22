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

        private SealightsReportApiHandler mSealightsApiHandler;
        public SealightsReportApiHandler SealightsReportApiHandler
        {
            get
            {
                if (mSealightsApiHandler == null)
                {
                    mSealightsApiHandler = new SealightsReportApiHandler(WorkSpace.Instance.Solution.LoggerConfigurations.SealightsURL);

                    //mSealightsApiHandler = new SealightsReportApiHandler("https://amdocs.sealights.co");
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
            await SendCreationTestSessionToSealightsAsync(runsetConfig);
        }

        public async Task SendCreationTestSessionToSealightsAsync(RunSetConfig runsetConfig)
        {
            await SealightsReportApiHandler.SendCreationTestSessionToSealightsAsync();

        }

        public async Task RunSetEnd(RunSetConfig runsetConfig)
        {
            await SendDeleteTestSessionToSealightsAsync(runsetConfig);
        }

        public async Task SendDeleteTestSessionToSealightsAsync(RunSetConfig runsetConfig)
        {
            await SealightsReportApiHandler.SendDeleteSessionToSealightsAsync();
        }
        #endregion RunSet   


        #region Runner
        public override async void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            await RunnerRunStartTask(gingerRunner);
        }

        private async Task RunnerRunStartTask(GingerRunner gingerRunner)
        {
            
        }

        public override async void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            if (!gingerRunner.Active || gingerRunner.Executor.ExecutionId == Guid.Empty || gingerRunner.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
            await RunnerRunEndTask((GingerExecutionEngine)gingerRunner.Executor);
        }

        private async Task RunnerRunEndTask(GingerExecutionEngine gingerRunner)
        {
            await SealightsReportApiHandler.SendDeleteSessionToSealightsAsync();
        }

        #endregion Runner

        #region BusinessFlow
        public override async void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
           await BusinessFlowStartTask(businessFlow);
        }

        private async Task BusinessFlowStartTask(BusinessFlow businessFlow)
        {
            
        }

        public override async void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            if (!businessFlow.Active || businessFlow.ExecutionId == Guid.Empty || businessFlow.RunStatus == Execution.eRunStatus.Blocked)
            {
                return;
            }
           await BusinessFlowEndTask(businessFlow);
        }

        private async Task BusinessFlowEndTask(BusinessFlow businessFlow)
        {
            
        }

        #endregion BusinessFlow

        #region Activity
        public override async void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
            await ActivityStartTask(activity);
        }

        private async Task ActivityStartTask(Activity activity)
        {
            //AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityStartData(activity, mContext);
            //await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity);
        }

        public override async void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            if (!activity.Active || activity.ExecutionId == Guid.Empty || activity.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }

            await ActivityEndTask(activity);
        }

        private async Task ActivityEndTask(Activity activity)
        {
            
        }

        #endregion Activity

        #region Activity Group 
        public override async void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            await ActivityGroupStartTask(activityGroup);
        }

        private async Task ActivityGroupStartTask(ActivitiesGroup activityGroup)
        {
            
        }

        public override async void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            if (activityGroup.ExecutionId == Guid.Empty || activityGroup.RunStatus == Common.InterfacesLib.eActivitiesGroupRunStatus.Blocked)
            {
                return;
            }
            await SendActivityGroupDataActionTask(activityGroup);
        }

        private async Task SendActivityGroupDataActionTask(ActivitiesGroup activityGroup)
        {
            
        }

        #endregion Activity Group

        #region Action
        public override async void ActionStart(uint eventTime, Act action)
        {
            await ActionStartTask(action);
        }

        private async Task ActionStartTask(Act action)
        {
            
        }

        public override async void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            if (!action.Active || action.ExecutionId == Guid.Empty || action.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
            await SendDataOnActionEndTask(action);
        }

        private async Task SendDataOnActionEndTask(Act action)
        {
            
        }

        #endregion Action       
    }
}