using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;

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
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetStartData(runsetConfig, mContext);
            AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet);            
        }
        public void RunSetEnd(RunSetConfig runsetConfig)
        {           
            AccountReportRunSet accountReportRunSet = AccountReportEntitiesDataMapping.MapRunsetEndData(runsetConfig, mContext);            
            //accountReportRunSet.UpdateData = true;
            AccountReportApiHandler.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet, true);
            Reporter.HideStatusMessage();
        }
        #endregion RunSet   


        #region Runner
        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {           
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerStartData(gingerRunner, mContext);            
            AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner);
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {         
            AccountReportRunner accountReportRunner = AccountReportEntitiesDataMapping.MapRunnerEndData(gingerRunner, mContext);          
            //gingerRunner.Elapsed = gingerRunner.RunnerExecutionWatch.runWatch.ElapsedMilliseconds;
            //accountReportRunner.UpdateData = true;
            AccountReportApiHandler.SendRunnerExecutionDataToCentralDBAsync(accountReportRunner, true);            
        }

        #endregion Runner

        #region BusinessFlow
        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowStartData(businessFlow, mContext);
            AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow);
        }
        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = AccountReportEntitiesDataMapping.MapBusinessFlowEndData(businessFlow, mContext);
            //accountReportBusinessFlow.UpdateData = true;
            AccountReportApiHandler.SendBusinessflowExecutionDataToCentralDBAsync(accountReportBusinessFlow, true);
        }       
        #endregion BusinessFlow

        #region Activity
        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
           AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityStartData(activity, mContext);
            AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity);
        }
        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            AccountReportActivity accountReportActivity = AccountReportEntitiesDataMapping.MapActivityEndData(activity, mContext);
            //accountReportActivity.UpdateData = true;
            AccountReportApiHandler.SendActivityExecutionDataToCentralDBAsync(accountReportActivity, true);
        }
        #endregion Activity

        #region Activity Group 
        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupStartData(activityGroup, mContext);
            AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup);
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            AccountReportActivityGroup accountReportActivityGroup = AccountReportEntitiesDataMapping.MapActivityGroupEndData(activityGroup, mContext);
            // accountReportActivityGroup.UpdateData = true;
            AccountReportApiHandler.SendActivityGroupExecutionDataToCentralDBAsync(accountReportActivityGroup, true);
        }

        #endregion Activity Group

        #region Action
        public override void ActionStart(uint eventTime, Act action)
        {
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionStartData(action, mContext);
            //AccountReportApiHandler accountReportApiHandler = new AccountReportApiHandler(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);
            //apiHandler.SendActionExecutionDataToCentralDBAsync(Workspace.instanc.RunsetExecuter.Runset.ExecutionID, mContext.Activity.ExecutionID, action.ExecutionID, accountReportAction);
            AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction);
        }
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode= false)
        {
            if (!action.Active)
            {
                return;
            }
            AccountReportAction accountReportAction = AccountReportEntitiesDataMapping.MapActionEndData(action, mContext);
            AccountReportApiHandler.SendScreenShotsToCentralDBAsync((Guid)WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID, action.ScreenShots.ToList());
            //  accountReportAction.UpdateData = true;
            AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(accountReportAction, true);
        }      
        #endregion Action       
    }
}
