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
