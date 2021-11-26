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
            if (WorkSpace.Instance.RunsetExecutor != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig != null
                && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID != null)
            {
                runsetConfig.ExecutionID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID;
            }
            Task.Run(() => SendRunsetExecutionDataToCentralDbTaskAsync(runsetConfig));
        }

        public async Task SendRunsetExecutionDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetStartData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet);
        }

        public void RunSetEnd(RunSetConfig runsetConfig)
        {
            Task.Run(() => SendRunsetEndDataToCentralDbTaskAsync(runsetConfig));
            Reporter.HideStatusMessage();
        }

        public async Task SendRunsetEndDataToCentralDbTaskAsync(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetEndData(runsetConfig, mContext);
            await AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet, true);
        }
        #endregion RunSet   


        #region Runner
        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            Task.Run(() => RunnerRunStartTask(gingerRunner));
        }

        private async Task RunnerRunStartTask(GingerRunner gingerRunner)
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
            Task.Run(() => RunnerRunEndTask(gingerRunner));
        }

        private async Task RunnerRunEndTask(GingerRunner gingerRunner)
        {
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerEndData(gingerRunner, mContext);
            await AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner, true);
        }

        #endregion Runner

        #region BusinessFlow
        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            Task.Run(() => BusinessFlowStartTask(businessFlow));
        }

        private async Task BusinessFlowStartTask(BusinessFlow businessFlow)
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
            Task.Run(() => BusinessFlowEndTask(businessFlow));
        }

        private async Task BusinessFlowEndTask(BusinessFlow businessFlow)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowEndData(businessFlow, mContext);
            await AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow, true);
        }

        #endregion BusinessFlow

        #region Activity
        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
            Task.Run(() => ActivityStartTask(activity));
        }

        private async Task ActivityStartTask(Activity activity)
        {
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityStartData(activity, mContext);
            await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity);
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            if (!activity.Active || activity.ExecutionId == Guid.Empty || activity.Status == Execution.eRunStatus.Blocked)
            {
                return;
            }

            Task.Run(() => ActivityEndTask(activity));
        }

        private async Task ActivityEndTask(Activity activity)
        {
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityEndData(activity, mContext);
            //accountReportActivity.UpdateData = true;
            await AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity, true);
        }

        #endregion Activity

        #region Activity Group 
        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            Task.Run(() => ActivityGroupStartTask(activityGroup));
        }

        private async Task ActivityGroupStartTask(ActivitiesGroup activityGroup)
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
            Task.Run(() => SendActivityGroupDataActionTask(activityGroup));
        }

        private async Task SendActivityGroupDataActionTask(ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupEndData(activityGroup, mContext);
            await AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup, true);
        }

        #endregion Activity Group

        #region Action
        public override void ActionStart(uint eventTime, Act action)
        {
            Task.Run(() => ActionStartTask(action));
        }

        private async Task ActionStartTask(Act action)
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
            Task.Run(() => SendDataOnActionEndTask(action));
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