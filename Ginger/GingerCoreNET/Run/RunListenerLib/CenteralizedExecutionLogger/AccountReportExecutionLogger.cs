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
        public void RunSetStart(RunSetConfig runsetConfig)
        {
            Reporter.ToStatus(eStatusMsgKey.PublishingToCentralDB, "Publishing Execution data to central DB");
            runsetConfig.ExecutionID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;

            SendRunsetExecutionDataToCentralDbTaskAsync(runsetConfig);
        }

        public async void SendRunsetExecutionDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetStartData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet);
        }

        public void RunSetEnd(RunSetConfig runsetConfig)
        {
            SendRunsetEndDataToCentralDbTaskAsync(runsetConfig);
            Reporter.HideStatusMessage();
        }

        public async void SendRunsetEndDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetEndData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet, true);
        }
        #endregion RunSet   


        #region Runner
        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            RunnerRunStartTask(gingerRunner);
        }

        private async void RunnerRunStartTask(GingerRunner gingerRunner)
        {
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerStartData(gingerRunner, mContext);
            await AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner);
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            if (!gingerRunner.Active || gingerRunner.ExecutionId == Guid.Empty || gingerRunner.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
            RunnerRunEndTask(gingerRunner);
        }

        private async void RunnerRunEndTask(GingerRunner gingerRunner)
        {
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerEndData(gingerRunner, mContext);
            await AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner, true);
        }

        #endregion Runner

        #region BusinessFlow
        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            BusinessFlowStartTask(businessFlow);
        }

        private async void BusinessFlowStartTask(BusinessFlow businessFlow)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowStartData(businessFlow, mContext);
            await AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow);
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            if (!businessFlow.Active || businessFlow.ExecutionId == Guid.Empty || businessFlow.RunStatus == Execution.eRunStatus.Blocked)
            {
                return;
            }
            BusinessFlowEndTask(businessFlow);
        }

        private async void BusinessFlowEndTask(BusinessFlow businessFlow)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowEndData(businessFlow, mContext);
            await AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow, true);
        }

        #endregion BusinessFlow

        #region Activity
        public async override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
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

            ActivityEndTask(activity);
        }

        private async void ActivityEndTask(Activity activity)
        {
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityEndData(activity, mContext);
            //accountReportActivity.UpdateData = true;
            await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity, true);
        }

        #endregion Activity

        #region Activity Group 
        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            ActivityGroupStartTask(activityGroup);
        }

        private async void ActivityGroupStartTask(ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupStartData(activityGroup, mContext);
            await AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup);
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            if (activityGroup.ExecutionId == Guid.Empty || activityGroup.RunStatus == Common.InterfacesLib.eActivitiesGroupRunStatus.Blocked)
            {
                return;
            }
            SendActivityGroupDataActionTask(activityGroup);
        }

        private async void SendActivityGroupDataActionTask(ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupEndData(activityGroup, mContext);
            await AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup, true);
        }

        #endregion Activity Group

        #region Action
        public async override void ActionStart(uint eventTime, Act action)
        {
            ActionStartTask(action);
        }

        private async void ActionStartTask(Act action)
        {
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionStartData(action, mContext);
            await AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction);
        }

        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            if (!action.Active || action.ExecutionId == Guid.Empty || action.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }
            SendDataOnActionEndTask(action);
        }

        private async void SendDataOnActionEndTask(Act action)
        {
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionEndData(action, mContext);
            await AccountReportApiHandler.SendScreenShotsToCentralDBAsync((Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID, action.ScreenShots.ToList());
            await AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction, true);
        }

        #endregion Action       
    }
}