using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.CentralExecutionLogger;
using Amdocs.Ginger.Run;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger
{
    public class AccountReportExecutionLogger : RunListenerBase
    {
        public Context mContext;
        public AccountReportEntitiesDataMapping mAccountReportEntitiesDataMapping;

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
            mAccountReportEntitiesDataMapping = new AccountReportEntitiesDataMapping();
        }

        #region RunSet
        public void RunSetStart(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = mAccountReportEntitiesDataMapping.MapRunsetStartData(runsetConfig, mContext);
            runsetConfig.StartTimeStamp = DateTime.Now.ToUniversalTime();
        }
        public void RunSetEnd(RunSetConfig runsetConfig)
        {
            AccountReportRunSet accountReportRunSet = mAccountReportEntitiesDataMapping.MapRunsetEndData(runsetConfig, mContext);
            runsetConfig.EndTimeStamp = DateTime.Now.ToUniversalTime();
        }
        #endregion RunSet   

        #region Runner
        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            AccountReportRunner accountReportRunner = mAccountReportEntitiesDataMapping.MapRunnerStartData(gingerRunner, mContext);
            gingerRunner.StartTimeStamp = DateTime.Now.ToUniversalTime();
        }

        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            AccountReportRunner accountReportRunner = mAccountReportEntitiesDataMapping.MapRunnerEndData(gingerRunner, mContext);
            gingerRunner.EndTimeStamp = DateTime.Now.ToUniversalTime();
            //gingerRunner.Elapsed = gingerRunner.RunnerExecutionWatch.runWatch.ElapsedMilliseconds;
        }

        #endregion Runner

        #region BusinessFlow
        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = mAccountReportEntitiesDataMapping.MapBusinessFlowStartData(businessFlow, mContext);
        }
        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            AccountReportBusinessFlow accountReportBusinessFlow = mAccountReportEntitiesDataMapping.MapBusinessFlowEndData(businessFlow, mContext);
        }       
        #endregion BusinessFlow

        #region Activity
        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
           AccountReportActivity accountReportActivity = mAccountReportEntitiesDataMapping.MapActivityStartData(activity, mContext);
        }
        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            AccountReportActivity accountReportActivity = mAccountReportEntitiesDataMapping.MapActivityEndData(activity, mContext);
        }     
        #endregion Activity

        #region Action
        public override void ActionStart(uint eventTime, Act action)
        {
            AccountReportAction accountReportAction = mAccountReportEntitiesDataMapping.MapActionStartData(action, mContext);
            // CentralExecutionLoggerHelper centralExecutionLogger = new CentralExecutionLoggerHelper(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);
            //AccountReportApiHandler.SendActionExecutionDataToCentralDBAsync(Workspace.instanc.RunsetExecuter.Runset.ExecutionID, mContext.Activity.ExecutionID, action.ExecutionID, accountReportAction);

        }
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode= false)
        {
            AccountReportAction accountReportAction = mAccountReportEntitiesDataMapping.MapActionEndData(action, mContext);            
        }

        public virtual void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            AccountReportActivityGroup accountReportActivityGroup = mAccountReportEntitiesDataMapping.MapActivityGroupStartData(activityGroup, mContext);
        }

        public virtual void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            AccountReportActivityGroup accountReportActivityGroup = mAccountReportEntitiesDataMapping.MapActivityGroupEndData(activityGroup, mContext);
        }
        #endregion Action       
    }
}
