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
        void ActivitySkipped(uint eventTime, Activity activity, bool offlineMode = false);
        void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false);
        void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false);
        void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup);
        void ActivityGroupSkipped(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false);
        void ActivityStart(uint eventTime, Activity activity, bool continuerun = false);
        void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false);
        void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false);
        void BusinessFlowSkipped(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false);
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
