#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.GeneralLib;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using LiteDB;
using System;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    // Each ExecutionLogger instance should be added to GingerRunner Listeneres
    // Create new ExecutionLogger for each run 

    public abstract class ExecutionLogger : IExecutionLogger
    {
        // static JsonSerializer mJsonSerializer;
        public static string mLogsFolder;      //!!!!!!!!!!!!!!!!!!!
        public string ExecutionLogfolder { get; set; }
        string mLogsFolderName;
        DateTime mCurrentExecutionDateTime;

        public BusinessFlow mCurrentBusinessFlow;
        public Activity mCurrentActivity;
        // uint meventtime;
        public ExecutionLoggerHelper executionLoggerHelper = new ExecutionLoggerHelper();
        ProjEnvironment mExecutionEnvironment = null;

        public ProjEnvironment ExecutionEnvironment
        {
            get
            {
                return mExecutionEnvironment;
            }
            set
            {
                mExecutionEnvironment = value;
            }
        }

        private GingerReport gingerReport = new GingerReport();

        public int ExecutionLogBusinessFlowsCounter = 0;

        public string CurrentLoggerFolder
        {
            get { return mLogsFolder; }
        }


        public DateTime CurrentExecutionDateTime
        {
            get { return mCurrentExecutionDateTime; }
            set { mCurrentExecutionDateTime = value; }
        }

        ParentGingerData IExecutionLogger.GingerData => GingerData;

        private ExecutionLoggerConfiguration mConfiguration = new ExecutionLoggerConfiguration();

        //public class ParentGingerData
        //{
        //    public int Seq;
        //    public string GingerName;
        //    public string GingerEnv;
        //    public List<string> GingerAggentMapping;
        //    public Guid Ginger_GUID;
        //};
        public ParentGingerData GingerData = new ParentGingerData();

        // TODO: remove the need for env - get it from notify event !!!!!!
        //public ExecutionLogger(ProjEnvironment environment, eExecutedFrom executedFrom = eExecutedFrom.Run)
        //{
        //    mJsonSerializer = new JsonSerializer();
        //    mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
        //    ExecutedFrom = executedFrom;
        //    ExecutionEnvironment = environment;//needed for supporting diffrent env config per Runner
        //}
        //public void SaveObjToJSonFile(object obj, string FileName, bool toAppend = false)
        //{
        //    //TODO: for speed we can do it async on another thread...
        //    using (StreamWriter SW = new StreamWriter(FileName, toAppend))
        //    using (JsonWriter writer = new JsonTextWriter(SW))
        //    {
        //        mJsonSerializer.Serialize(writer, obj);

        //    }
        //}
        public abstract void SaveObjToReporsitory(object obj, string FileName = "", bool toAppend = false);
        public abstract object SetReportAction(Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool offlineMode = false);
        internal ActionReport GetActionReportData(Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            ActionReport AR = new ActionReport(action, context);
            if (context.Activity != null)
            {
                AR.Seq = context.Activity.ExecutionLogActionCounter;
            }

            AR.RunDescription = ConvertValueExpressionToString(context.Environment, context.BusinessFlow, action.RunDescription);
            return AR;
        }
        internal ActivityReport GetActivityReportData(Activity activity, Context context, bool offlineMode)
        {
            ActivityReport AR = new ActivityReport(activity)
            {
                Seq = context.BusinessFlow.ExecutionLogActivityCounter,
                VariablesBeforeExec = activity.VariablesBeforeExec,
                RunDescription = ConvertValueExpressionToString(context.Environment, context.BusinessFlow, activity.RunDescription)
            };
            return AR;
        }
        internal ActivityGroupReport GetAGReportData(ActivitiesGroup activityGroup, IContext context)
        {
            ActivityGroupReport AGR = new(activityGroup, context.BusinessFlow)
            {
                Seq = context.BusinessFlow.ActivitiesGroups.IndexOf(activityGroup) + 1,
                ExecutionLogFolder = ExecutionLogfolder + context.BusinessFlow.ExecutionLogFolder,
                ExternalID = ConvertValueExpressionToString(context.Environment, context.BusinessFlow, activityGroup.ExternalID)
            };

            return AGR;
        }
        internal BusinessFlowReport GetBFReportData(BusinessFlow businessFlow, ProjEnvironment environment)
        {
            BusinessFlowReport BFR = new BusinessFlowReport(businessFlow)
            {
                VariablesBeforeExec = businessFlow.VariablesBeforeExec,
                SolutionVariablesBeforeExec = businessFlow.SolutionVariablesBeforeExec,
                Seq = this.ExecutionLogBusinessFlowsCounter,
                RunDescription = ConvertValueExpressionToString(environment, businessFlow, businessFlow.RunDescription),
                ExternalID = ConvertValueExpressionToString(environment, businessFlow, businessFlow.ExternalID)
            };
            return BFR;
        }

        //public static object LoadObjFromJSonFile(string FileName, Type t)
        //{
        //    return JsonLib.LoadObjFromJSonFile(FileName, t, mJsonSerializer);
        //}

        //public static object LoadObjFromJSonString(string str, Type t)
        //{
        //    return JsonLib.LoadObjFromJSonString(str, t, mJsonSerializer);
        //}

        public abstract object SetReportActivity(Activity activity, Context context, eExecutedFrom executedFrom = eExecutedFrom.Run, bool offlineMode = false, bool isConfEnable = false);

        public abstract object SetReportBusinessFlow(Context context, bool offlineMode = false, eExecutedFrom executedFrom = eExecutedFrom.Run, bool isConfEnable = false);
        public abstract object SetReportActivityGroup(IContext context, ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode = false);
        public virtual void SetReportRunner(GingerExecutionEngine gingerRunner, GingerReport gingerReport, ParentGingerData gingerData, Context mContext, string filename, int runnerCount)
        {
            if (gingerRunner == null)
            {
                gingerReport.Seq = this.GingerData.Seq;
                gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                gingerReport.GUID = this.GingerData.Ginger_GUID.ToString();
                gingerReport.Name = this.GingerData.GingerName.ToString();
                gingerReport.ApplicationAgentsMappingList = this.GingerData.GingerAggentMapping;
                gingerReport.EnvironmentName = mContext.Environment != null ? mContext.Environment.Name : string.Empty;
                gingerReport.Elapsed = (double)gingerReport.Watch.ElapsedMilliseconds / 1000;
            }
            else
            {
                gingerReport.GingerRunner = gingerRunner.GingerRunner;
                if (runnerCount != 0)
                {
                    gingerReport.Seq = runnerCount;
                }
                else
                {
                    gingerReport.Seq = this.GingerData.Seq;  //!!!
                }
                gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                gingerReport.GUID = gingerRunner.GingerRunner.Guid.ToString();
                gingerReport.Name = gingerRunner.GingerRunner.Name;
                gingerReport.ApplicationAgentsMappingList = gingerRunner.GingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                gingerReport.EnvironmentName = gingerRunner.GingerRunner.ProjEnvironment != null ? gingerRunner.GingerRunner.ProjEnvironment.Name : string.Empty;
                gingerReport.Elapsed = (double)gingerRunner.Elapsed / 1000;
                if (gingerReport.LogFolder == null && !(string.IsNullOrEmpty(filename)))
                {
                    gingerReport.LogFolder = filename;
                }
            }
        }

        public abstract void CreateNewDirectory(string logFolder);

        public virtual void SetReportRunSet(RunSetReport runSetReport, string logFolder, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            runSetReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
            runSetReport.Elapsed = runSetReport.EndTimeStamp.Subtract(runSetReport.StartTimeStamp).TotalSeconds;
            runSetReport.MachineName = Environment.MachineName;
            runSetReport.ExecutedbyUser = Environment.UserName;
            runSetReport.GingerVersion = ApplicationInfo.ApplicationUIversion;
        }
        public abstract void RunSetUpdate(LiteDB.ObjectId runSetLiteDbId, LiteDB.ObjectId runnerLiteDbId, GingerExecutionEngine gingerRunner);
        public abstract void SetRunsetFolder(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline);
        public abstract void StartRunSet();
        public abstract void EndRunSet();
        public abstract void ResetLastRunSetDetails();
        public abstract string SetExecutionLogFolder(string executionLogfolder, bool isCleanFile);
        public abstract string GetLogFolder(string folder);



        public abstract string CalculateExecutionJsonData(LiteDBFolder.LiteDbRunSet liteDbRunSet, HTMLReportConfiguration reportTemplate);

        protected static string ConvertValueExpressionToString(ProjEnvironment environment, BusinessFlow businessFlow, string ValueExpresionString)
        {
            if (!string.IsNullOrEmpty(ValueExpresionString))
            {
                IValueExpression mVE = new GingerCore.ValueExpression(environment, businessFlow, [], false, "", false)
                {
                    Value = ValueExpresionString
                };

                return mVE.ValueCalculated;
            }


            return string.Empty;
        }
        public abstract void DeleteLocalData(string logFolder, ObjectId runsetId, Guid executionId);
    }
}
