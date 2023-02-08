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
//using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using LiteDB;
using System;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public interface IExecutionLogger
    {
        void CreateNewDirectory(string logFolder);
        DateTime CurrentExecutionDateTime { get; set; }
        string CurrentLoggerFolder { get; }
        void EndRunSet();
        ProjEnvironment ExecutionEnvironment { get; set; }
        string ExecutionLogfolder { get; set; }

        //string CalculateExecutionJsonData(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration reportTemplate);
        string GetLogFolder(string folder);

        ParentGingerData GingerData { get; }

        void ResetLastRunSetDetails();

        // void RunSetUpdate(ObjectId runSetLiteDbId, ObjectId runnerLiteDbId, GingerExecutionEngine gingerRunner);
        void SaveObjToReporsitory(object obj, string FileName = "", bool toAppend = false);
        Task<bool> SendExecutionLogToCentralDBAsync(ObjectId runsetId, Guid executionId, ExecutionLoggerConfiguration.eDeleteLocalDataOnPublish deleteLocalData);
        string SetExecutionLogFolder(string executionLogfolder, bool isCleanFile);
        //object SetReportAction(Act action, IContext context, eExecutedFrom executedFrom, bool offlineMode = false);
        //object SetReportActivity(Activity activity, IContext context, bool offlineMode = false, bool isConfEnable = false);
        object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode = false);
        //object SetReportBusinessFlow(IContext context, bool offlineMode = false, eExecutedFrom executedFrom = eExecutedFrom.Run, bool isConfEnable = false);
        //void SetReportRunner(IGingerExecutionEngine gingerRunner, GingerReport gingerReport, ParentGingerData gingerData, IContext mContext, string filename, int runnerCount);
        //void SetReportRunSet(RunSetReport runSetReport, string logFolder);
        void StartRunSet();
    }
}
