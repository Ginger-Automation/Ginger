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

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public class AccountReportExecutionLogger : RunListenerBase
    {
        public Context mContext; 

        private AccountReportApiHandler mAccountReportApiHandler;
        public AccountReportApiHandler AccountReportApiHandler
        {
            get
            {
                if (mAccountReportApiHandler == null)
                {
                    mAccountReportApiHandler = new AccountReportApiHandler(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);
                }
                return mAccountReportApiHandler;
            }
        }

        public AccountReportExecutionLogger(Context context)
        {
            mContext = context;
        }

        #region RunSet
        public async Task RunSetStart(RunSetConfig runsetConfig)
        {
            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, null, "Publishing Execution data to central DB");
            if (WorkSpace.Instance.RunsetExecutor != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig != null
                && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID != null)
            {
                runsetConfig.ExecutionID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            }
            await SendRunsetExecutionDataToCentralDbTaskAsync(runsetConfig);
        }

        public async Task SendRunsetExecutionDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetStartData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet);
        }

        public async Task RunSetEnd(RunSetConfig runsetConfig)
        {
            await SendRunsetEndDataToCentralDbTaskAsync(runsetConfig);
            Reporter.HideStatusMessage();
        }

        public async Task SendRunsetEndDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetEndData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet, true);
        }
        #endregion RunSet   


        #region Runner
        public override async void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            await RunnerRunStartTask(gingerRunner);
        }

        private async Task RunnerRunStartTask(GingerRunner gingerRunner)
        {
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerStartData(gingerRunner, mContext);
            await AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner);
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
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerEndData(gingerRunner.GingerRunner, mContext);
            await AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner, true);
        }

        #endregion Runner

        #region BusinessFlow
        public override async void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
           await BusinessFlowStartTask(businessFlow);
        }

        private async Task BusinessFlowStartTask(BusinessFlow businessFlow)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowStartData(businessFlow, mContext);
            await AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow);
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
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowEndData(businessFlow, mContext);
            await AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow, true);
        }

        #endregion BusinessFlow

        #region Activity
        public override async void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
            await ActivityStartTask(activity);
        }

        private async Task ActivityStartTask(Activity activity)
        {
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityStartData(activity, mContext);
            await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity);
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
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityEndData(activity, mContext);
            //accountReportActivity.UpdateData = true;
            await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity, true);
        }

        #endregion Activity

        #region Activity Group 
        public override async void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            await ActivityGroupStartTask(activityGroup);
        }

        private async Task ActivityGroupStartTask(ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupStartData(activityGroup, mContext);
            await AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup);
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
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupEndData(activityGroup, mContext);
            await AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup, true);
        }

        #endregion Activity Group

        #region Action
        public override async void ActionStart(uint eventTime, Act action)
        {
            await ActionStartTask(action);
        }

        private async Task ActionStartTask(Act action)
        {
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionStartData(action, mContext);
            await AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction);
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
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionEndData(action, mContext);
            await AccountReportApiHandler.SendScreenShotsToCentralDBAsync((Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID, action.ScreenShots.ToList());
            await AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction, true);
        }

        #endregion Action       
    }
}