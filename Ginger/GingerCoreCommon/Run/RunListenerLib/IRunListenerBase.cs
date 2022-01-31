using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;

namespace Amdocs.Ginger.Run
{
    public interface IRunListenerBase
    {
        void ActionEnd(uint eventTime, Act action, bool offlineMode = false);
        void ActionStart(uint eventTime, Act action);
        void ActionUpdatedEnd(uint eventTime, Act action);
        void ActionUpdatedStart(uint eventTime, Act action);
        void ActionWaitEnd(uint eventTime, Act action);
        void ActionWaitStart(uint eventTime, Act action);
        void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false);
        void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false);
        void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup);
        void ActivityStart(uint eventTime, Activity activity, bool continuerun = false);
        void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false);
        void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false);
        void BusinessflowWasReset(uint eventTime, BusinessFlow businessFlow);
        void DynamicActivityWasAddedToBusinessflow(uint eventTime, BusinessFlow businessFlow);
        void EnvironmentChanged(uint eventTime, ProjEnvironment mProjEnvironment);
        void ExecutionContext(uint eventTime, ExecutionLoggerConfiguration.AutomationTabContext automationTabContext, BusinessFlow businessFlow);
        void GiveUserFeedback(uint eventTime);
        void PrepActionEnd(uint eventTime, Act action);
        void PrepActionStart(uint eventTime, Act action);
        void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false);
        void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false);
    }
}