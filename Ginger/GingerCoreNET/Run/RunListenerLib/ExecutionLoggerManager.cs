#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.FlowControlLib;
using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ginger.Run
{
    // Each ExecutionLoggerManager instance should be added to GingerRunner Listeneres
    // Create new ExecutionLoggerManager for each run 

    public class ExecutionLoggerManager : RunListenerBase
    {
        public static string defaultAutomationTabLogName = "AutomationTab_LastExecution";
        public static string defaultAutomationTabOfflineLogName = "AutomationTab_OfflineExecution";
        public static string defaultRunTabBFName = "RunTab_BusinessFlowLastExecution";
        public static string defaultRunTabRunConsolidatedName = "RunTab_ConsolidatedReportLastExecution";
        public static string defaultRunTabLogName = "DefaultRunSet";
        static Newtonsoft.Json.JsonSerializer mJsonSerializer;
        public static string mLogsFolder;      //!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public string ExecutionLogfolder { get; set; }
        string mLogsFolderName;
        DateTime mCurrentExecutionDateTime;
        int BFCounter = 0;
        private eExecutedFrom ExecutedFrom;
        public BusinessFlow mCurrentBusinessFlow;
        public Activity mCurrentActivity;
        uint meventtime;
        

        int mBusinessFlowCounter { get; set; }
        public Context mContext;
        public ExecutionLogger mExecutionLogger;
        public ExecutionLoggerHelper executionLoggerHelper;
        

        private GingerReport gingerReport = new GingerReport();

        public static Ginger.Reports.RunSetReport RunSetReport;

        public int ExecutionLogBusinessFlowsCounter = 0;

        GingerRunnerLogger mGingerRunnerLogger;

        public ExecutionLoggerConfiguration Configuration
        {
            get { return mConfiguration; }
            set
            {
                if (value != null)
                {
                    mConfiguration = value;
                    mConfiguration.CalculatedLoggerFolder = executionLoggerHelper.GetLoggerDirectory(mConfiguration.ExecutionLoggerConfigurationExecResultsFolder);
                    if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                    {

                        switch (this.ExecutedFrom)
                        {
                            case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                                mExecutionLogger.ExecutionLogfolder = Path.Combine(mConfiguration.CalculatedLoggerFolder, defaultAutomationTabLogName);
                                break;
                            case Amdocs.Ginger.Common.eExecutedFrom.Run:

                                if ((WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name != null) && (WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name != string.Empty))
                                {
                                    mLogsFolderName = folderNameNormalazing(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name) + "_" + mCurrentExecutionDateTime.ToString("MMddyyyy_HHmmss");
                                }
                                else
                                {
                                    RunSetReport.Name = defaultRunTabLogName;
                                    mLogsFolderName = defaultRunTabLogName + "_" + mCurrentExecutionDateTime.ToString("MMddyyyy_HHmmss");
                                }
                                mExecutionLogger.ExecutionLogfolder = Path.Combine(mConfiguration.CalculatedLoggerFolder, mLogsFolderName, this.GingerData.Seq.ToString() + " " + this.GingerData.GingerName);

                                break;
                        }
                        mExecutionLogger.ExecutionLogfolder = executionLoggerHelper.GetLoggerDirectory(mExecutionLogger.ExecutionLogfolder);
                        executionLoggerHelper.CleanDirectory(mExecutionLogger.ExecutionLogfolder);
                    }
                    else
                    {
                        if (ExecutedFrom == eExecutedFrom.Run)
                        {
                            RunSetReport.Name = defaultRunTabLogName;
                        }
                    }

                }

            }
        }
        public string CurrentLoggerFolder
        {
            get { return mLogsFolder; }
        }

        public string CurrentLoggerFolderName
        {
            get { return mLogsFolderName; }
        }
        public DateTime CurrentExecutionDateTime
        {
            get { return mCurrentExecutionDateTime; }
            set { mCurrentExecutionDateTime = value; }
        }

        private ExecutionLoggerConfiguration mConfiguration = new ExecutionLoggerConfiguration();

        public class ParentGingerData
        {
            public int Seq;
            public string GingerName;
            public string GingerEnv;
            public List<string> GingerAggentMapping;
            public Guid Ginger_GUID;
        };
        public ParentGingerData GingerData = new ParentGingerData();

        // TODO: remove the need for env - get it from notify event !!!!!!
        public ExecutionLoggerManager(Context context, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            mContext = context;
            mJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            ExecutedFrom = executedFrom;
            if(WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
            {
                mExecutionLogger = new LiteDBRepository();
            }
            else
            {
                mExecutionLogger = new TextFileRepository();
            }
            executionLoggerHelper = new ExecutionLoggerHelper();
        }
        
        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            activityGroup.StartTimeStamp = eventTime; // DateTime.Now.ToUniversalTime();

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, null);
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            //ActivityGroupReport AGR = new ActivityGroupReport(activityGroup, mContext.BusinessFlow);
            object AGR = mExecutionLogger.SetReportActivityGroup(activityGroup, mContext.BusinessFlow, offlineMode);
        
            if (!offlineMode)
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, AGR);
        }

        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner, bool offlineMode = false)
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                gingerReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
                gingerReport.Watch.Start();
                gingerReport.LogFolder = string.Empty;

                switch (this.ExecutedFrom)
                {
                    case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                        gingerReport.LogFolder = mExecutionLogger.ExecutionLogfolder;
                        break;
                    default:
                        gingerReport.LogFolder = mExecutionLogger.ExecutionLogfolder;
                        break;
                }
                mExecutionLogger.CreateNewDirectory(gingerReport.LogFolder);

                if (!offlineMode)
                {
                    ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, "Runner", gingerRunner.Name, null);
                }
            }
        }



        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0, bool offlineMode = false)
        {
            gingerRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerConfigurationIsEnabled = Configuration.ExecutionLoggerConfigurationIsEnabled;
            mExecutionLogger.SetReportRunner(gingerRunner, gingerReport, GingerData, mContext, filename, runnerCount);
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                this.ExecutionLogBusinessFlowsCounter = 0;
                mExecutionLogger.ExecutionLogBusinessFlowsCounter = 0;
                this.BFCounter = 0;

                if (!offlineMode)
                {
                    ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, "Runner", gingerRunner.Name, gingerReport);
                }
            }
        }

        public void RunSetStart(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline = false)
        {
            if (RunSetReport == null)
            {
                RunSetReport = new RunSetReport();

                if ((WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name != null) && (WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name != string.Empty))
                {
                    RunSetReport.Name = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;
                }
                else
                {
                    RunSetReport.Name = defaultRunTabLogName;
                }
                RunSetReport.Description = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Description;
                RunSetReport.GUID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Guid.ToString();
                RunSetReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
                RunSetReport.Watch.Start();
                mExecutionLogger.SetRunsetFolder(execResultsFolder, maxFolderSize, currentExecutionDateTime, offline);
            }

            if (!offline)
            {
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.RunSet), WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, null);
            }
        }

        public void RunSetEnd(string LogFolder = null, bool offline = false)
        {
            if (RunSetReport != null)
            {
                mExecutionLogger.SetReportRunSet(RunSetReport, LogFolder);
                
                // AddExecutionDetailsToLog(eExecutionPhase.End, "Run Set", RunSetReport.Name, RunSetReport);
                if (WorkSpace.Instance.RunningInExecutionMode)
                {
                    //Amdocs.Ginger.CoreNET.Execution.eRunStatus.TryParse(RunSetReport.RunSetExecutionStatus, out App.RunSetExecutionStatus);//saving the status for determin Ginger exit code
                    WorkSpace.Instance.RunsetExecutor.RunSetExecutionStatus = RunSetReport.RunSetExecutionStatus;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder.Equals("-1"))
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;
                }
                //App.RunPage.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;

                if (!offline)
                {
                    ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.RunSet), WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, RunSetReport);
                }
                RunSetReport = null;
            }
        }
        //to delete
        public static void SaveToJsonFile(object obj, string FileName, bool toAppend = false)
        {
            using (StreamWriter SW = new StreamWriter(FileName, toAppend))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);
            }
        }
        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            mCurrentBusinessFlow = businessFlow;
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                this.BFCounter++;
                string BFFolder = string.Empty;
                this.ExecutionLogBusinessFlowsCounter++;
                mExecutionLogger.ExecutionLogBusinessFlowsCounter++;
                switch (this.ExecutedFrom)
                {
                    case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                        //if (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun) // Not Sure why it is added, not working at some points, removing it for now
                        //{
                        mExecutionLogger.ExecutionLogfolder = mExecutionLogger.SetExecutionLogFolder(mExecutionLogger.ExecutionLogfolder, true);
                        // }

                        break;
                    case Amdocs.Ginger.Common.eExecutedFrom.Run:
                        if (ContinueRun == false)
                        {
                            BFFolder = BFCounter + " " + folderNameNormalazing(businessFlow.Name);
                        }
                        break;
                    default:
                        BFFolder = BFCounter + " " + folderNameNormalazing(businessFlow.Name);
                        break;
                }
                businessFlow.VariablesBeforeExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.SolutionVariablesBeforeExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.ExecutionLogFolder = BFFolder;
                mExecutionLogger.CreateNewDirectory(Path.Combine(Configuration.CalculatedLoggerFolder,BFFolder));

                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, null);
            }
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            mContext.BusinessFlow = businessFlow;
            Object BFR = mExecutionLogger.SetReportBusinessFlow(mContext, offlineMode, ExecutedFrom, this.Configuration.ExecutionLoggerConfigurationIsEnabled);

            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                {
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    mExecutionLogger.ExecutionLogBusinessFlowsCounter = 0;
                    this.BFCounter = 0;
                }
            }

            if (!offlineMode)
            {                
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, BFR);
            }
        }
        // fix add to listener/loader class
        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun = false)
        {
            mCurrentActivity = activity;
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                string ActivityFolder = string.Empty;
                if ((this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation) && (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun))
                {
                    mExecutionLogger.ExecutionLogfolder = mExecutionLogger.SetExecutionLogFolder(mExecutionLogger.ExecutionLogfolder, true);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                }
                else if ((Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun))
                {
                    mExecutionLogger.ExecutionLogfolder = mExecutionLogger.SetExecutionLogFolder(mExecutionLogger.ExecutionLogfolder, false);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                    mCurrentBusinessFlow.ExecutionLogActivityCounter++;

                    ActivityFolder = Path.Combine(mCurrentBusinessFlow.ExecutionLogFolder,mCurrentBusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName));
                }
                else
                {
                    if (this.ExecutedFrom == eExecutedFrom.Run && continuerun == false)
                    {
                        mCurrentBusinessFlow.ExecutionLogActivityCounter++;
                    }
                    else if (this.ExecutedFrom == eExecutedFrom.Automation && continuerun == false)
                    {
                        mCurrentBusinessFlow.ExecutionLogActivityCounter++;
                    }

                    ActivityFolder = Path.Combine(mCurrentBusinessFlow.ExecutionLogFolder,mCurrentBusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName));
                }

                activity.ExecutionLogFolder = mExecutionLogger.GetLogFolder(ActivityFolder);
                mExecutionLogger.CreateNewDirectory(Path.Combine(mExecutionLogger.ExecutionLogfolder,ActivityFolder));
                activity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
            }

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, null);
        }

        // fix
        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            object AR = mExecutionLogger.SetReportActivity(activity, mContext, offlineMode, Configuration.ExecutionLoggerConfigurationIsEnabled);

            if (!offlineMode)
            {               
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, AR);
            }
        }

        // same function in extention
        private static string folderNameNormalazing(string folderName)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            folderName = folderName.Replace(@".", "");
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            if (folderName.Length > 30)
            {
                folderName = folderName.Substring(0, 30);
            }
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            return folderName;
        }

        public override void ActionStart(uint eventTime, Act action)
        {
            SetActionFolder(action);
            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, "Action", action.Description, null);
        }
        // remove
        public void SetActionFolder(Act action)
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                string ActionFolder = string.Empty;
                if ((this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation) && (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ActionRun))
                {
                    mExecutionLogger.ExecutionLogfolder = mExecutionLogger.SetExecutionLogFolder(mExecutionLogger.ExecutionLogfolder, true);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                }
                else if ((Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun))
                {
                    mExecutionLogger.ExecutionLogfolder = mExecutionLogger.SetExecutionLogFolder(mExecutionLogger.ExecutionLogfolder, true);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                    mCurrentActivity.ExecutionLogActionCounter++;
                    ActionFolder = Path.Combine(mCurrentActivity.ExecutionLogFolder,mCurrentActivity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description));
                }
                else
                {
                    mCurrentActivity.ExecutionLogActionCounter++;
                    ActionFolder = Path.Combine(mCurrentActivity.ExecutionLogFolder,mCurrentActivity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description));
                }
                action.ExecutionLogFolder = mExecutionLogger.GetLogFolder(ActionFolder);
                mExecutionLogger.CreateNewDirectory(Path.Combine(mExecutionLogger.ExecutionLogfolder,ActionFolder));

            }
        }
        // fix
        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {
            // if user set special action log in output
            if (action.EnableActionLogConfig)
            {
                if (mGingerRunnerLogger == null)
                {
                    string loggerFile = Path.Combine(mExecutionLogger.ExecutionLogfolder, FileSystem.AppendTimeStamp("GingerLog.txt"));
                    mGingerRunnerLogger = new GingerRunnerLogger(loggerFile);
                }
                mGingerRunnerLogger.LogAction(action);
            }

            try
            {
                string executionLogFolder = string.Empty;
                //if offline mode then execution logger path exists in action object so making executionLogFolder empty to avoid duplication of path.
                if (!offlineMode)
                    executionLogFolder = mExecutionLogger.ExecutionLogfolder;
                //ActionReport AR = new ActionReport(action, mContext);                
                mContext.Activity = mCurrentActivity; //!!!!
                Object AR = null;
                if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
                {
                    AR = mExecutionLogger.SetReportAction(action, mContext, this.ExecutedFrom, offlineMode);
                    //
                    // Defects Suggestion section (to be considered to remove to separate function)
                    //
                    if (action.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        if (WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Where(z => z.FailedActionGuid == action.Guid).ToList().Count > 0)
                            return;

                        //
                        ActivitiesGroup currrentGroup = mContext.BusinessFlow.ActivitiesGroups.Where(x => x.Name == mCurrentActivity.ActivitiesGroupID).FirstOrDefault();
                        string currrentGroupName = string.Empty;
                        if (currrentGroup != null)
                            currrentGroupName = currrentGroup.Name;

                        //
                        List<string> screenShotsPathes = new List<string>();
                        bool isScreenshotButtonEnabled = false;
                        if ((action.ScreenShots != null) && (action.ScreenShots.Count > 0))
                        {
                            screenShotsPathes = action.ScreenShots;
                            isScreenshotButtonEnabled = true;
                        }
                        // 
                        bool automatedOpeningFlag = false;
                        if (action.FlowControls.Where(x => x.FlowControlAction == eFlowControlAction.FailureIsAutoOpenedDefect && action.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).ToList().Count > 0)
                            automatedOpeningFlag = true;

                        // OMG !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        //
                        StringBuilder description = new StringBuilder();
                        description.Append("&#60;html&#62;&#60;body&#62;&#60;b&#62;" + this.GingerData.GingerName + "&#60;b&#62;&#60;br&#62;");
                        description.Append("&#60;div&#62;&#60;ul style='list - style - type:circle'&#62;&#60;li&#62;&#60;b&#62;" + mContext.BusinessFlow.Name + " (failed)&#60;b&#62;&#60;/li&#62;");
                        if (currrentGroupName != string.Empty)
                        {
                            description.Append("&#60;ul style = 'list - style - type:square'&#62;");
                            this.mCurrentBusinessFlow.ActivitiesGroups.ToList().TakeWhile(x => x.Name != mCurrentActivity.ActivitiesGroupID).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Name + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;" + currrentGroupName + " (failed)&#60;b&#62;&#60;/li&#62;");
                            description.Append("&#60;ul style = 'list - style - type:upper-roman'&#62;");
                            this.mCurrentBusinessFlow.Activities.Where(x => currrentGroup.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(x.Guid)).ToList().TakeWhile(x => x.Guid != mCurrentActivity.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.ActivityName + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;" + mCurrentActivity.ActivityName + " (failed)&#60;b&#62;&#60;/li&#62;");
                            description.Append("&#60;ul style = 'list - style - type:disc'&#62;");
                            mCurrentActivity.Acts.TakeWhile(x => x.Guid != action.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Description + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;&#60;font color='#ff0000'b&#62;" + action.Description + " (failed)&#60;/font&#62;&#60;b&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/div&#62;&#60;/body&#62;&#60;/html&#62;");
                        }
                        else
                        {
                            description.Append("&#60;ul style = 'list - style - type:upper-roman'&#62;");
                            mContext.BusinessFlow.Activities.TakeWhile(x => x.Guid != mCurrentActivity.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.ActivityName + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;" + mCurrentActivity.ActivityName + " (failed)&#60;b&#62;&#60;/li&#62;");
                            description.Append("&#60;ul style = 'list - style - type:disc'&#62;");
                            mCurrentActivity.Acts.TakeWhile(x => x.Guid != action.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Description + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;&#60;font color='#ff0000'b&#62;" + action.Description + " (failed)&#60;/font&#62;&#60;b&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/div&#62;&#60;/body&#62;&#60;/html&#62;");
                        }

                        if(GingerData.GingerName == null)
                        {
                            GingerData.GingerName = "";
                        }
                        WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.Add(new DefectSuggestion(action.Guid, this.GingerData.GingerName, mContext.BusinessFlow.Name, currrentGroupName,
                                                                                            mContext.BusinessFlow.ExecutionLogActivityCounter, mCurrentActivity.ActivityName, mCurrentActivity.ExecutionLogActionCounter,
                                                                                            action.Description, action.RetryMechanismCount, action.Error, action.ExInfo, screenShotsPathes,
                                                                                            isScreenshotButtonEnabled, automatedOpeningFlag, description.ToString()));
                    }
                    //
                    // Defects Suggestion section - end
                    //                   
                }

                if (!offlineMode)
                {                    
                    ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, "Action", action.Description, AR);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in ExecutionLogger Action end", ex);
            }
        }
        
        public string GetRunSetLastExecutionLogFolderOffline()
        {

            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
            {                
                return WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;

            }
            else
            {             
                ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;

                if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
                {
                    //TODO   AppReporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                    return string.Empty;
                }

                string exec_folder = folderNameNormalazing(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name) + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
                exec_folder = executionLoggerHelper.GetLoggerDirectory(Path.Combine(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder,exec_folder));
                WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = exec_folder;
                int RunnerCount = 1;
                RunSetStart(exec_folder, _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, CurrentExecutionDateTime, true);

                foreach (GingerRunner gingerrunner in WorkSpace.Instance.RunsetExecutor.RunSetConfig.GingerRunners)
                {
                    string folder = Path.Combine(exec_folder,RunnerCount.ToString() + " " + gingerrunner.Name);
                    if (System.IO.Directory.Exists(folder))
                    {
                        executionLoggerHelper.CleanDirectory(folder);
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(folder);
                    }

                    mContext = gingerrunner.Context;
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus gingerRunnerStatus = gingerrunner.RunsetStatus;
                    if (gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }

                    Configuration.ExecutionLoggerConfigurationIsEnabled = true;
                    mExecutionLogger.ExecutionLogfolder = folder;
                    gingerReport = new GingerReport();
                    RunnerRunStart(0, gingerrunner, true);
                    OfflineRunnerExecutionLog(gingerrunner, folder, RunnerCount);
                    RunnerCount++;
                }
                RunSetEnd(exec_folder, true);
                return exec_folder;
            }
        }

        // Move all report items from here !!!!!!!!!!!!!!!!
        public void GenerateRunSetOfflineReport()
        {
            try
            {
                HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                string exec_folder = GetRunSetLastExecutionLogFolderOffline();
                string reportsResultFolder = string.Empty;
                reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false, null, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);
                if (reportsResultFolder == string.Empty)
                {
                    //TODO     AppReporter.ToUser(eUserMsgKey.StaticWarnMessage, "Failed to generate the report for the '" + WorkSpace.Businessflow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
                    return;
                }
                else
                {
                    foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                    {
                        string fileName = Path.GetFileName(txt_file);
                        if (fileName.Contains(".html"))
                        {
                            Process.Start(reportsResultFolder);
                            Process.Start(reportsResultFolder + "\\" + fileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
        //Move to GingerRunnerLogger
        public void GenerateBusinessFlowOfflineReport(Context context, string reportsResultFolder, string RunsetName = null)
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            string exec_folder = string.Empty;
            exec_folder = GenerateBusinessflowOfflineExecutionLogger(context, RunsetName);
            if (string.IsNullOrEmpty(exec_folder))
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false, null, reportsResultFolder, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.AutomationTabExecResultsNotExists);
                return;
            }
            else
            {
                foreach (string txt_file in System.IO.Directory.GetFiles(reportsResultFolder))
                {
                    string fileName = System.IO.Path.GetFileName(txt_file);
                    if (fileName.Contains(".html"))
                    {
                        Process.Start(reportsResultFolder);
                        Process.Start(reportsResultFolder + "\\" + fileName);
                    }
                }
            }
        }
        public string GenerateBusinessFlowOfflineFolder(string executionLoggerConfFolder, string businessFlowName, string RunsetName = null)
        { 
            string BFExtention = businessFlowName + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
            string exec_folder = (string.IsNullOrEmpty(RunsetName)) ? BFExtention : folderNameNormalazing(RunsetName) + "_" + BFExtention;
            exec_folder = executionLoggerHelper.GetLoggerDirectory(executionLoggerConfFolder + "\\" + exec_folder);
            return exec_folder;
        }
        public string GenerateBusinessflowOfflineExecutionLogger(Context context, string RunsetName = null)
        {
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                return string.Empty;
            }
            string exec_folder = string.Empty;
            exec_folder = GenerateBusinessFlowOfflineFolder(_selectedExecutionLoggerConfiguration.CalculatedLoggerFolder, context.BusinessFlow.Name, RunsetName);
            Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            context.Runner.SetBFOfflineData(context.BusinessFlow, (context.Runner.RunListeners[0] as ExecutionLoggerManager), exec_folder);
            return exec_folder;
        }
        public bool OfflineRunnerExecutionLog(GingerRunner runner, string logFolderPath, int runnerCount = 0)
        {
            try
            {
                mBusinessFlowCounter = 0;
                ObservableList<BusinessFlow> listBF = runner.BusinessFlows;
                int counter = 1;
                foreach (BusinessFlow bf in listBF)
                {
                    string reportpath = Path.Combine(logFolderPath,counter.ToString() + " " + folderNameNormalazing(bf.Name));
                    System.IO.Directory.CreateDirectory(reportpath);
                    this.ExecutionLogBusinessFlowsCounter = counter;
                    mExecutionLogger.ExecutionLogBusinessFlowsCounter = counter;
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
                    runner.CalculateBusinessFlowFinalStatus(bf, true);
                    if (bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    runner.SetBFOfflineData(bf, this, reportpath);
                    mBusinessFlowCounter++;
                    runner.ExecutionLogFolder = Path.Combine(logFolderPath, mBusinessFlowCounter + " " + folderNameNormalazing(bf.Name));
                    counter++;
                }
                RunnerRunEnd(meventtime, runner, logFolderPath, runnerCount, true);
                // GingerEnd(runner, logFolderPath, runnerCount);  // !!!!!!!!!!!!!!!!!!!!! FIXME
                runner.ExecutionLogFolder = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Execution Logger Failed to do Offline BusinessFlow Execution Log", ex);
                return false;
            }
        }

        public override void ExecutionContext(uint eventTime, ExecutionLoggerConfiguration.AutomationTabContext automationTabContext, BusinessFlow businessFlow)
        {
            mCurrentBusinessFlow = businessFlow;
            mCurrentActivity = businessFlow.CurrentActivity;
            meventtime = eventTime;
        }
        // remove to GinngerRunner.SetBFOfflineData
        public bool OfflineBusinessFlowExecutionLog(BusinessFlow businessFlow, string logFolderPath)
        {
            try
            {
                //handle root directory
                if (Directory.Exists(logFolderPath))
                    executionLoggerHelper.CleanDirectory(logFolderPath);
                else
                    Directory.CreateDirectory(logFolderPath);
                GingerRunner Gr = new GingerRunner();
                mCurrentBusinessFlow = businessFlow;
                businessFlow.OffilinePropertiesPrep(logFolderPath);
                System.IO.Directory.CreateDirectory(businessFlow.ExecutionLogFolder);
                foreach (Activity activity in businessFlow.Activities)
                {
                    ActivitiesGroup currentActivityGroup = businessFlow.ActivitiesGroups.Where(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid)).FirstOrDefault();
                    if (currentActivityGroup != null)
                    {
                        currentActivityGroup.ExecutionLogFolder = logFolderPath;
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case executionLoggerStatus.NotStartedYet:
                                ActivityGroupStart(meventtime, currentActivityGroup);
                                break;
                        }
                    }

                    Gr.CalculateActivityFinalStatus(activity);
                    if (activity.GetType() == typeof(IErrorHandler))
                    {
                        continue;
                    }
                    if (activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }

                    mCurrentActivity = activity;
                    activity.OfflinePropertiesPrep(businessFlow.ExecutionLogFolder, businessFlow.ExecutionLogActivityCounter, ExtensionMethods.folderNameNormalazing(activity.ActivityName));
                    System.IO.Directory.CreateDirectory(activity.ExecutionLogFolder);
                    foreach (Act action in activity.Acts)
                    {
                        if (action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored)
                        {
                            continue;
                        }
                        activity.ExecutionLogActionCounter++;
                        action.ExecutionLogFolder = Path.Combine(activity.ExecutionLogFolder,activity.ExecutionLogActionCounter + " " + ExtensionMethods.folderNameNormalazing(action.Description));
                        System.IO.Directory.CreateDirectory(action.ExecutionLogFolder);

                        ActionEnd(meventtime, action, true);
                    }
                    ActivityEnd(meventtime, activity, true);
                    businessFlow.ExecutionLogActivityCounter++;
                }
                Gr.SetActivityGroupsExecutionStatus(businessFlow, true);
                Gr.CalculateBusinessFlowFinalStatus(businessFlow);

                BusinessFlowEnd(meventtime, businessFlow, true);
                businessFlow.ExecutionLogFolder = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Execution Logger Failed to do Offline BusinessFlow Execution Log", ex);
                return false;
            }
        }

        public void RunnerRunUpdate(ObjectId RunnerLiteDbId)
        {
            throw new NotImplementedException();
        }
    }
}
