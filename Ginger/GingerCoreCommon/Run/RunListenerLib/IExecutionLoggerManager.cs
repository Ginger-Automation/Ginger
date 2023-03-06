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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using LiteDB;
using System;
using System.Threading.Tasks;

namespace Ginger.Run
{
    public interface IExecutionLoggerManager
    {
        ExecutionLoggerConfiguration Configuration { get; set; }
        DateTime CurrentExecutionDateTime { get; set; }
        string CurrentLoggerFolder { get; }
        string CurrentLoggerFolderName { get; }
        string ExecutionLogfolder { get; set; }

        void ActionEnd(uint eventTime, Act action, bool offlineMode = false);
        void ActionStart(uint eventTime, Act action);
        void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false);
        void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false);
        void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup);
        void ActivityStart(uint eventTime, Activity activity, bool continuerun = false);
        void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false);
        void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false);
        void ExecutionContext(uint eventTime, ExecutionLoggerConfiguration.AutomationTabContext automationTabContext, BusinessFlow businessFlow);
        IExecutionLogger mExecutionLogger { get; set; }

        //string GenerateBusinessflowOfflineExecutionLogger(IContext context, string RunsetName = null);
        string GenerateBusinessFlowOfflineFolder(string executionLoggerConfFolder, string businessFlowName, string RunsetName = null);
        //void GenerateBusinessFlowOfflineReport(IContext context, string reportsResultFolder, string RunsetName = null);
        void GenerateRunSetOfflineReport();
        string GetRunSetLastExecutionLogFolderOffline();
        //bool OfflineBusinessFlowExecutionLog(BusinessFlow businessFlow, string logFolderPath);
        //bool OfflineRunnerExecutionLog(IGingerExecutionEngine runner, string logFolderPath, int runnerCount = 0);

        ParentGingerData GingerData { get; }

        Task PublishToCentralDBAsync(ObjectId runsetId, Guid executionId);
        //void RunnerRunEnd(uint eventTime, IGingerExecutionEngine gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false);
        //void RunnerRunStart(uint eventTime, IGingerExecutionEngine gingerRunner, bool offlineMode = false);
        //void RunnerRunUpdate(ObjectId RunnerLiteDbId);
        void RunSetEnd(string LogFolder = null, bool offline = false);
        void RunSetStart(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline = false);
        void SetActionFolder(Act action);
    }
}
