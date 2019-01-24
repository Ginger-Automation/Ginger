#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
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
    // Each ExecutionLogger instance should be added to GingerRunner Listeneres
    // Create new ExecutionLogger for each run 

    public class ExecutionLogger : RunListenerBase
    {
        public static string defaultAutomationTabLogName = "AutomationTab_LastExecution";
        public static string defaultAutomationTabOfflineLogName = "AutomationTab_OfflineExecution";
        public static string defaultRunTabBFName = "RunTab_BusinessFlowLastExecution";
        public static string defaultRunTabRunConsolidatedName = "RunTab_ConsolidatedReportLastExecution";
        public static string defaultRunTabLogName = "DefaultRunSet";
        static JsonSerializer mJsonSerializer;
        public static string mLogsFolder;      //!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public string ExecutionLogfolder { get; set; }
        string mLogsFolderName;
        DateTime mCurrentExecutionDateTime;
        int BFCounter = 0;
        private eExecutedFrom ExecutedFrom;
        public BusinessFlow mCurrentBusinessFlow;
        public Activity mCurrentActivity;
        uint meventtime;
        IValueExpression mVE;

        ProjEnvironment mExecutionEnvironment = null;

        int mBusinessFlowCounter { get; set; }       

        public ProjEnvironment ExecutionEnvironment
        {
            get
            {
                // !!!!!!!!!!!!! called many time ??
                if (mExecutionEnvironment == null)//not supposed to be null but in case it is
                {
                    // !!!!!!!!!!!!!!!!! remove logger should get the env from GR
                    if (this.ExecutedFrom == eExecutedFrom.Automation)
                    {
                        mExecutionEnvironment = WorkSpace.AutomateTabEnvironment;
                    }
                    else
                    {
                        mExecutionEnvironment = WorkSpace.RunsetExecutor.RunsetExecutionEnvironment;
                    }
                }
                return mExecutionEnvironment;
            }
            set
            {
                mExecutionEnvironment = value;
            }
        }

        private GingerReport gingerReport = new GingerReport();        
        public static Ginger.Reports.RunSetReport RunSetReport;

        public int ExecutionLogBusinessFlowsCounter = 0;

        GingerRunnerLogger mGingerRunnerLogger;

        public ExecutionLoggerConfiguration Configuration
        {
            get { return mConfiguration; }
            set
            {
                mConfiguration = value;

                if(!CheckOrCreateDirectory(mConfiguration.ExecutionLoggerConfigurationExecResultsFolder))
                {
                    mConfiguration.ExecutionLoggerConfigurationExecResultsFolder= mConfiguration.ExecutionLoggerConfigurationExecResultsFolder = @"~\ExecutionResults\"; 
                }

              
                switch (this.ExecutedFrom)
                {
                    case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                        ExecutionLogfolder = mConfiguration.ExecutionLoggerConfigurationExecResultsFolder + @"\\" + defaultAutomationTabLogName;
                        break;
                    case Amdocs.Ginger.Common.eExecutedFrom.Run:

                        if ((WorkSpace.RunsetExecutor.RunSetConfig.Name!= null) && (WorkSpace.RunsetExecutor.RunSetConfig.Name != string.Empty))
                        {
                            mLogsFolderName = folderNameNormalazing(WorkSpace.RunsetExecutor.RunSetConfig.Name) + "_" + mCurrentExecutionDateTime.ToString("MMddyyyy_HHmmss");
                        }
                        else
                        {
                            RunSetReport.Name = defaultRunTabLogName;
                            mLogsFolderName = defaultRunTabLogName + "_" + mCurrentExecutionDateTime.ToString("MMddyyyy_HHmmss");
                        }
                        ExecutionLogfolder = mConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + mLogsFolderName + "\\" + this.GingerData.Seq.ToString() + " " + this.GingerData.GingerName + "\\";

                        break;
                }
                ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                CleanDirectory(ExecutionLogfolder);
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
        public ExecutionLogger(ProjEnvironment environment, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            mJsonSerializer = new JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            ExecutedFrom = executedFrom;
            ExecutionEnvironment = environment;//needed for supporting diffrent env config per Runner
        }

        private static void CleanDirectory(string folderName, bool isCleanFile= true)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
            if (isCleanFile)
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        private static void CreateTempDirectory()
        {
            try
            {
                if (!Directory.Exists(WorkSpace.TempFolder))
                {
                    System.IO.Directory.CreateDirectory(WorkSpace.TempFolder);
                }
                else
                {
                    CleanDirectory(WorkSpace.TempFolder);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating temporary folder", ex);
            }

        }
        public static string GetLoggerDirectory(string logsFolder)
        {
            logsFolder = logsFolder.Replace(@"~", WorkSpace.Instance.Solution.Folder);
            try
            {
                if(CheckOrCreateDirectory(logsFolder))
                {
                    return logsFolder;
                }
                else
                {
                    //If the path configured by user in the logger is not accessible, we set the logger path to default path
                    logsFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"ExecutionResults\");
                    System.IO.Directory.CreateDirectory(logsFolder);
                    
                    WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationExecResultsFolder = @"~\ExecutionResults\";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            return logsFolder;
        }

        private static Boolean CheckOrCreateDirectory(string directoryPath)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryPath))
                {
                    return true;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                    return true;
                }
            }
            catch(Exception ex)
            {    Reporter.ToLog(eLogLevel.ERROR, "failed to CheckOrCreateDirectory",ex); 
                return false;
            }
            
        }

        private static void SaveObjToJSonFile(object obj, string FileName, bool toAppend = false)
        {
            //TODO: for speed we can do it async on another thread...
            using (StreamWriter SW = new StreamWriter(FileName, toAppend))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);

            }
        }

        public static object LoadObjFromJSonFile(string FileName, Type t)
        {
          return  JsonLib.LoadObjFromJSonFile(FileName,t,mJsonSerializer);
        }

        public static object LoadObjFromJSonString(string str, Type t)
        {
            return JsonLib.LoadObjFromJSonString(str, t, mJsonSerializer);
        }

        public override void ActivityGroupStart(uint eventTime, ActivitiesGroup activityGroup)
        {
            activityGroup.StartTimeStamp = eventTime; // DateTime.Now.ToUniversalTime();

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, null);
        }

        public override void ActivityGroupEnd(uint eventTime, ActivitiesGroup activityGroup)
        {
            bool offlineMode = true;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!
           
            ActivityGroupReport AGR = new ActivityGroupReport(activityGroup, mCurrentBusinessFlow);
            AGR.Seq = mCurrentBusinessFlow.ActivitiesGroups.IndexOf(activityGroup) + 1;
            AGR.ExecutionLogFolder = ExecutionLogfolder + mCurrentBusinessFlow.ExecutionLogFolder;
            if (offlineMode && activityGroup.ExecutionLogFolder != null)
            {
                SaveObjToJSonFile(AGR, activityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                File.AppendAllText(activityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
            }
            else
            {
                if(offlineMode)
                {
                    SaveObjToJSonFile(AGR, ExecutionLogfolder + mCurrentBusinessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                    File.AppendAllText(ExecutionLogfolder + mCurrentBusinessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
                }
                else
                {
                    SaveObjToJSonFile(AGR, ExecutionLogfolder + mCurrentBusinessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                    File.AppendAllText(ExecutionLogfolder + mCurrentBusinessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
                }               
            }

            if (!offlineMode)
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activityGroup.Name, AGR);
        }

        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner)
        {
            gingerReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
            gingerReport.Watch.Start();
            gingerReport.LogFolder = string.Empty;

            switch (this.ExecutedFrom)
            {
                case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                    gingerReport.LogFolder = ExecutionLogfolder;
                    break;
                default:
                    gingerReport.LogFolder = ExecutionLogfolder;
                    break;
            }
            System.IO.Directory.CreateDirectory(gingerReport.LogFolder);

            
            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, "Runner", gingerRunner.Name, null);
        }



        public override void RunnerRunEnd(uint eventTime, GingerRunner gingerRunner, string filename = null, int runnerCount = 0)
        {
            if (gingerRunner == null)
            {
                if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
                {
                    gingerReport.Seq = this.GingerData.Seq;
                    gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                    gingerReport.GUID = this.GingerData.Ginger_GUID.ToString();
                    gingerReport.Name = this.GingerData.GingerName.ToString();
                    gingerReport.ApplicationAgentsMappingList = this.GingerData.GingerAggentMapping;
                    gingerReport.EnvironmentName = ExecutionEnvironment != null ? ExecutionEnvironment.Name : string.Empty;
                    gingerReport.Elapsed = (double)gingerReport.Watch.ElapsedMilliseconds / 1000;
                    SaveObjToJSonFile(gingerReport, gingerReport.LogFolder + @"\Ginger.txt");
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    this.BFCounter = 0;
                }
            }
            else
            {
                if (runnerCount != 0)
                {
                    gingerReport.Seq = runnerCount;
                }
                else
                {
                    gingerReport.Seq = this.GingerData.Seq;  //!!!
                }
                gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                gingerReport.GUID = gingerRunner.Guid.ToString();
                gingerReport.Name = gingerRunner.Name;
                gingerReport.ApplicationAgentsMappingList = gingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                gingerReport.EnvironmentName = gingerRunner.ProjEnvironment != null ? gingerRunner.ProjEnvironment.Name : string.Empty;
                gingerReport.Elapsed = (double)gingerRunner.Elapsed / 1000;
                if (gingerReport.LogFolder == null && !(string.IsNullOrEmpty(filename)))
                    {
                    gingerReport.LogFolder = filename;
                }
                
                SaveObjToJSonFile(gingerReport, gingerReport.LogFolder + @"\Ginger.txt");
                this.ExecutionLogBusinessFlowsCounter = 0;
                this.BFCounter = 0;
            }

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, "Runner", gingerRunner.Name, gingerReport);
        }

        public static void RunSetStart(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline = false)
        {
            if (RunSetReport == null)
            {
                RunSetReport = new RunSetReport();

                if ((WorkSpace.RunsetExecutor.RunSetConfig.Name != null) && (WorkSpace.RunsetExecutor.RunSetConfig.Name != string.Empty))
                {
                    RunSetReport.Name = WorkSpace.RunsetExecutor.RunSetConfig.Name;
                }
                else
                {
                    RunSetReport.Name = defaultRunTabLogName;
                }
                RunSetReport.Description = WorkSpace.RunsetExecutor.RunSetConfig.Description;
                RunSetReport.GUID = WorkSpace.RunsetExecutor.RunSetConfig.Guid.ToString();
                RunSetReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
                RunSetReport.Watch.Start();
                if (!offline)
                    RunSetReport.LogFolder = ExecutionLogger.GetLoggerDirectory(execResultsFolder + "\\" + folderNameNormalazing(RunSetReport.Name.ToString()) + "_" + currentExecutionDateTime.ToString("MMddyyyy_HHmmss"));
                else
                    RunSetReport.LogFolder = ExecutionLogger.GetLoggerDirectory(execResultsFolder);

                DeleteFolderContentBySizeLimit DeleteFolderContentBySizeLimit = new DeleteFolderContentBySizeLimit(RunSetReport.LogFolder, maxFolderSize);
                
                CreateTempDirectory();                
            }

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.RunSet), WorkSpace.RunsetExecutor.RunSetConfig.Name, null);
        }

        public static void RunSetEnd(string LogFolder=null)
        {
            if (RunSetReport != null)
            {
                RunSetReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                RunSetReport.Elapsed = (double)RunSetReport.Watch.ElapsedMilliseconds / 1000;
                RunSetReport.MachineName = Environment.MachineName.ToString();
                RunSetReport.ExecutedbyUser = Environment.UserName.ToString();

                if (LogFolder == null)
                {
                    SaveObjToJSonFile(RunSetReport, RunSetReport.LogFolder + @"\RunSet.txt");
                }
                else
                {
                    SaveObjToJSonFile(RunSetReport, LogFolder + @"\RunSet.txt");
                }
                // AddExecutionDetailsToLog(eExecutionPhase.End, "Run Set", RunSetReport.Name, RunSetReport);
                if (WorkSpace.RunningInExecutionMode)
                {
                    //Amdocs.Ginger.CoreNET.Execution.eRunStatus.TryParse(RunSetReport.RunSetExecutionStatus, out App.RunSetExecutionStatus);//saving the status for determin Ginger exit code
                    WorkSpace.RunSetExecutionStatus = RunSetReport.RunSetExecutionStatus;
                }
                if (WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null && WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder.Equals("-1"))
                {
                    WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;
                }
                //App.RunPage.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;

                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.RunSet), WorkSpace.RunsetExecutor.RunSetConfig.Name, RunSetReport);
                RunSetReport = null;
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
                switch (this.ExecutedFrom)
                {
                    case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                        //if (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun) // Not Sure why it is added, not working at some points, removing it for now
                        //{
                            ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                            CleanDirectory(ExecutionLogfolder);
                        // }
                        
                            return;
                        break;
                    case Amdocs.Ginger.Common.eExecutedFrom.Run:
                        if(ContinueRun==false)
                        {
                            BFFolder = BFCounter + " " + folderNameNormalazing(businessFlow.Name);
                        }
                        break;
                    default:
                        BFFolder = BFCounter + " " + folderNameNormalazing(businessFlow.Name);
                        break;
                }
                businessFlow.VariablesBeforeExec = businessFlow.Variables.Select(a => a.Name+ "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.SolutionVariablesBeforeExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.ExecutionLogFolder = BFFolder;

                System.IO.Directory.CreateDirectory(ExecutionLogfolder + BFFolder);

                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, null);
            }
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode= false)
        {
            BusinessFlowReport BFR = new BusinessFlowReport(businessFlow);

            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {                                
                BFR.VariablesBeforeExec = businessFlow.VariablesBeforeExec; 
                BFR.SolutionVariablesBeforeExec = businessFlow.SolutionVariablesBeforeExec;
                BFR.Seq = this.ExecutionLogBusinessFlowsCounter;
                if (!string.IsNullOrEmpty(businessFlow.RunDescription))
                {
                    if (mVE == null)
                    {
                        mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(ExecutionEnvironment, null, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                    }
                    mVE.Value = businessFlow.RunDescription;
                    BFR.RunDescription = mVE.ValueCalculated;
                }

                if (offlineMode)
                {
                    // To check whether the execution is from Runset/Automate tab
                    if((this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation))
                    {
                        businessFlow.ExecutionFullLogFolder = businessFlow.ExecutionLogFolder;
                    }
                    else if ((WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)  )
                    {
                        businessFlow.ExecutionFullLogFolder = businessFlow.ExecutionLogFolder;
                    }
                    SaveObjToJSonFile(BFR, businessFlow.ExecutionFullLogFolder + @"\BusinessFlow.txt");
                    
                }
                else
                {
                    // use Path.cOmbine
                    SaveObjToJSonFile(BFR, ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\BusinessFlow.txt");
                    businessFlow.ExecutionFullLogFolder = ExecutionLogfolder + businessFlow.ExecutionLogFolder;
                }
                if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                {
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    this.BFCounter = 0;
                }               
            }

            if (!offlineMode)
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, BFR);
        }

        public BusinessFlowReport LoadBusinessFlow(string FileName)
        {
            try
            {
                BusinessFlowReport BFR = (BusinessFlowReport)LoadObjFromJSonFile(FileName, typeof(BusinessFlowReport));
                BFR.GetBusinessFlow().ExecutionLogFolder = System.IO.Path.GetDirectoryName(FileName);
                return BFR;
            }
            catch
            {
                return new BusinessFlowReport();
            }
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun=false)
        {
            mCurrentActivity = activity;
            // move to Ginger Runner not here !!!!!!!!!!!!!!!!!!  do not change attr 
            // activity.StartTimeStamp = DateTime.Now.ToUniversalTime();


            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                string ActivityFolder = string.Empty;

                if ((this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation) && (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ActivityRun))
                {
                    ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                    CleanDirectory(ExecutionLogfolder);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                }
                else if ((Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun))
                {
                    ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                    CleanDirectory(ExecutionLogfolder, false);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                    mCurrentBusinessFlow.ExecutionLogActivityCounter++;

                    // use Path.combine !!!!
                    ActivityFolder = mCurrentBusinessFlow.ExecutionLogFolder + @"\" + mCurrentBusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName);
                }
                else
                {
                    if (this.ExecutedFrom == eExecutedFrom.Run && continuerun == false)
                    {
                        mCurrentBusinessFlow.ExecutionLogActivityCounter++;
                    }
                    else if(this.ExecutedFrom== eExecutedFrom.Automation && continuerun== false)
                    {
                        mCurrentBusinessFlow.ExecutionLogActivityCounter++;
                    }
                    
                    // use Path.combine !!!!
                    ActivityFolder = mCurrentBusinessFlow.ExecutionLogFolder + @"\" + mCurrentBusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName);
                }

                activity.ExecutionLogFolder = ActivityFolder;
                System.IO.Directory.CreateDirectory(ExecutionLogfolder + ActivityFolder);
                activity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();                
            }

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, null);
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode= false)
        {
            ActivityReport AR = new ActivityReport(activity);

            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {                                
                AR.Seq = mCurrentBusinessFlow.ExecutionLogActivityCounter;
                AR.VariablesBeforeExec = activity.VariablesBeforeExec;

                if ((activity.RunDescription != null) && (activity.RunDescription != string.Empty))
                {
                    if (mVE == null)
                    {
                        mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(ExecutionEnvironment, null, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                    }
                    mVE.Value = activity.RunDescription;
                    AR.RunDescription = mVE.ValueCalculated;
                }

                if (offlineMode)
                    // use Path.combine !!!!
                    SaveObjToJSonFile(AR, activity.ExecutionLogFolder + @"\Activity.txt");
                else
                    // use Path.combine !!!!
                    SaveObjToJSonFile(AR, ExecutionLogfolder + activity.ExecutionLogFolder + @"\Activity.txt");                
            }

            if (!offlineMode)
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.Activity), activity.ActivityName, AR);
        }

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
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                string ActionFolder = string.Empty;
                // action.StartTimeStamp = DateTime.Now.ToUniversalTime();

                if ((this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation) && (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ActionRun))
                {
                    ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                    CleanDirectory(ExecutionLogfolder);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                }
                else if ((Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.ContinueRun))
                {
                    ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                    CleanDirectory(ExecutionLogfolder);
                    Configuration.ExecutionLoggerAutomationTabContext = ExecutionLoggerConfiguration.AutomationTabContext.None;
                    mCurrentActivity.ExecutionLogActionCounter++;
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    ActionFolder = mCurrentActivity.ExecutionLogFolder + @"\" + mCurrentActivity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description);
                }
                else
                {
                    mCurrentActivity.ExecutionLogActionCounter++;
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    ActionFolder = mCurrentActivity.ExecutionLogFolder + @"\" + mCurrentActivity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description);
                }
                action.ExecutionLogFolder = ActionFolder;
                System.IO.Directory.CreateDirectory(ExecutionLogfolder + ActionFolder);

            }

            ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.Start, "Action", action.Description, null);
        }

        public override void ActionEnd(uint eventTime, Act action, bool offlineMode=false)
        {                    
            // if user set special action log in output
            if (action.EnableActionLogConfig)
            {                                             
                if (mGingerRunnerLogger == null)
                {
                    string loggerFile = Path.Combine(ExecutionLogfolder, FileSystem.AppendTimeStamp("GingerLog.txt"));
                    mGingerRunnerLogger = new GingerRunnerLogger(loggerFile);
                }
                mGingerRunnerLogger.LogAction(action);
            }

            try
            {
                string executionLogFolder = string.Empty;
                //if offline mode then execution logger path exists in action object so making executionLogFolder empty to avoid duplication of path.
                if (!offlineMode)
                    executionLogFolder = ExecutionLogfolder;

                ActionReport AR = new ActionReport(action, ExecutionEnvironment);
                if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
                {
                    if (System.IO.Directory.Exists(executionLogFolder + action.ExecutionLogFolder))
                    {
                                    
                        ProjEnvironment environment = null;

                        if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                        {
                            environment = WorkSpace.AutomateTabEnvironment;
                        }
                        else
                        {
                            environment = WorkSpace.RunsetExecutor.RunsetExecutionEnvironment;
                        }
                        
                        AR.Seq = mCurrentActivity.ExecutionLogActionCounter;
                        if ((action.RunDescription != null) && (action.RunDescription != string.Empty))
                        {
                            if (mVE == null)
                            {
                                mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(ExecutionEnvironment, null, new ObservableList<GingerCore.DataSource.DataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                            }
                            mVE.Value = action.RunDescription;
                            AR.RunDescription = mVE.ValueCalculated;
                        }

                        if (!string.IsNullOrEmpty(action.ExecutionLogFolder))
                        {
                            SaveObjToJSonFile(AR, executionLogFolder + action.ExecutionLogFolder + @"\Action.txt");
                        }

                        // Save screenShots
                        int screenShotCountPerAction = 0;
                        for (var s = 0; s < action.ScreenShots.Count; s++)
                        {
                            try
                            {
                                screenShotCountPerAction++;
                                if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                                {
                                    System.IO.File.Copy(action.ScreenShots[s], executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png", true);
                                }
                                else
                                {
                                    System.IO.File.Move(action.ScreenShots[s], executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png");
                                    action.ScreenShots[s] = executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png";
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to move screen shot of the action:'" + action.Description + "' to the Execution Logger folder", ex);
                                screenShotCountPerAction--;
                            }
                        }
                            
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + action.Description + " because directory not exists :" + executionLogFolder + action.ExecutionLogFolder);
                    }                    

                    //
                    // Defects Suggestion section (to be considered to remove to separate function)
                    //
                    if (action.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        if (WorkSpace.RunsetExecutor.DefectSuggestionsList.Where(z => z.FailedActionGuid == action.Guid).ToList().Count > 0)
                            return;

                        //
                        ActivitiesGroup currrentGroup = this.mCurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == mCurrentActivity.ActivitiesGroupID).FirstOrDefault();
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
                        if (action.FlowControls.Where(x => x.FlowControlAction == eFlowControlAction.FailureIsAutoOpenedDefect && x.Condition == "\"{ActionStatus}\" = \"Failed\"").ToList().Count > 0)
                            automatedOpeningFlag = true;

                        // OMG !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        //
                        StringBuilder description = new StringBuilder();
                        description.Append("&#60;html&#62;&#60;body&#62;&#60;b&#62;" + this.GingerData.GingerName + "&#60;b&#62;&#60;br&#62;");
                        description.Append("&#60;div&#62;&#60;ul style='list - style - type:circle'&#62;&#60;li&#62;&#60;b&#62;" + this.mCurrentBusinessFlow.Name + " (failed)&#60;b&#62;&#60;/li&#62;");
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
                            this.mCurrentBusinessFlow.Activities.TakeWhile(x => x.Guid != mCurrentActivity.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.ActivityName + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;" + mCurrentActivity.ActivityName + " (failed)&#60;b&#62;&#60;/li&#62;");
                            description.Append("&#60;ul style = 'list - style - type:disc'&#62;");
                            mCurrentActivity.Acts.TakeWhile(x => x.Guid != action.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Description + "&#60;/li&#62;"); });
                            description.Append("&#60;li&#62;&#60;b&#62;&#60;font color='#ff0000'b&#62;" + action.Description + " (failed)&#60;/font&#62;&#60;b&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/div&#62;&#60;/body&#62;&#60;/html&#62;");
                        }


                        WorkSpace.RunsetExecutor.DefectSuggestionsList.Add(new DefectSuggestion(action.Guid, this.GingerData.GingerName, this.mCurrentBusinessFlow.Name, currrentGroupName,
                                                                                            mCurrentBusinessFlow.ExecutionLogActivityCounter, mCurrentActivity.ActivityName, mCurrentActivity.ExecutionLogActionCounter,
                                                                                            action.Description, action.RetryMechanismCount, action.Error, action.ExInfo, screenShotsPathes,
                                                                                            isScreenshotButtonEnabled, automatedOpeningFlag, description.ToString()));
                    }
                    //
                    // Defects Suggestion section - end
                    //                   
                }

                if (!offlineMode)
                    ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, "Action", action.Description, AR);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in ExecutionLogger Action end", ex);
            }                   
        }


        // Moved to ProgressReportListener
        //private static void AddExecutionDetailsToLog(eExecutionPhase objExecutionPhase, string objType, string objName, object obj)
        //{
        //    if (Reporter.RunningInExecutionMode || Reporter.AppLoggingLevel == eAppReporterLoggingLevel.Debug)
        //    {
        //        string prefix = string.Empty;
        //        switch (objExecutionPhase)
        //        {
        //            case eExecutionPhase.Start:
        //                prefix = "--> Execution Started for the " + objType + ": '" + objName + "'";
        //                break;
        //            case eExecutionPhase.End:
        //                prefix = "<-- Execution Ended for the " + objType + ": '" + objName + "'";
        //                break;
        //        }

        //        //get the execution fields and their values
        //        if (obj != null)
        //        {
        //            List<KeyValuePair<string, string>> fieldsAndValues = new List<KeyValuePair<string, string>>();
        //            try
        //            {
        //                PropertyInfo[] props = obj.GetType().GetProperties();
        //                foreach (PropertyInfo prop in props)
        //                {
        //                    try
        //                    {
        //                        FieldParamsFieldType attr = ((FieldParamsFieldType)prop.GetCustomAttribute(typeof(FieldParamsFieldType)));
        //                        if (attr == null)
        //                        {
        //                            continue;
        //                        }
        //                        FieldsType ftype = attr.FieldType;
        //                        if (ftype == FieldsType.Field)
        //                        {
        //                            string propName = prop.Name;
        //                            string propFullName = ((FieldParamsNameCaption)prop.GetCustomAttribute(typeof(FieldParamsNameCaption))).NameCaption;
        //                            string propValue = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.Instance).GetValue(obj).ToString();
        //                            fieldsAndValues.Add(new KeyValuePair<string, string>(propFullName, propValue));
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        //TODO: !!!!!!!!!!!!!!!!!! FIXME
        //                    }
        //                }
        //            }
        //            catch (Exception)
        //            {
        //                //TODO: !!!!!!!!!!!!!!!!!! FIXME
        //            }

        //            //add to Console
        //            string details = string.Empty;
        //            foreach (KeyValuePair<string, string> det in fieldsAndValues)
        //                details += det.Key + "= " + det.Value + System.Environment.NewLine;
        //            Reporter.ToLog(eLogLevel.DEBUG, prefix + System.Environment.NewLine + "Details:" + System.Environment.NewLine + details);
        //        }
        //        else
        //        {
        //            Reporter.ToLog(eLogLevel.DEBUG, prefix + System.Environment.NewLine);
        //        }
        //    }

        //}


        public void VariableChanged(VariableBase VB, string OriginalValue)
        {

        }

        public ReportInfo GetReportInfo()
        {
            //TODO: check what is needed.
            ReportInfo RI = new ReportInfo(ExecutionLogfolder);
            return RI;
        }

        public String GetLastExecutedActivityRunStatus(Activity CurrentActivity)
        {
            if (!String.IsNullOrEmpty(CurrentActivity.ExecutionLogFolder))
            {
                string currentActivityDirName = Path.GetFileName(CurrentActivity.ExecutionLogFolder);

                if (!String.IsNullOrEmpty(currentActivityDirName))
                {
                    int currentActivitySeq = Int32.Parse(currentActivityDirName.Substring(0, 1));

                    if (currentActivitySeq > 1)
                    {
                        string previoustActivityFolderSearchString = currentActivitySeq - 1 + "*";

                        DirectoryInfo dirInfo = new DirectoryInfo(ExecutionLogfolder);
                        DirectoryInfo[] matchingDirInfo = dirInfo.GetDirectories(previoustActivityFolderSearchString);
                        if (matchingDirInfo.Length != 0)
                        {
                            string LastActivityStatusFile = ExecutionLogfolder + Path.DirectorySeparatorChar + matchingDirInfo[0] + @"\" + "Activity.txt";
                            if (File.Exists(LastActivityStatusFile))
                            {
                                try
                                {
                                    ActivityReport ri = (ActivityReport)LoadObjFromJSonFile(LastActivityStatusFile, typeof(ActivityReport));
                                    return ri.RunStatus;
                                }
                                catch { return Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA.ToString(); }
                            }
                        }
                    }
                    else
                        return "Last executed Activity status not available as this is first activity";
                }
            }
            return "Last Executed activity status is not available. Please ensure Execution logger is enabled";
        }

        public static string GetRunSetLastExecutionLogFolderOffline()
        {

            if (WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null)
            {
                AutoLogProxy.UserOperationStart("Online Report");
                return WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder;
                
            }
            else
            {
                AutoLogProxy.UserOperationStart("Offline Report");
                ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();

                if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
                {
                 //TODO   AppReporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                    return string.Empty;
                }

                string exec_folder = folderNameNormalazing(WorkSpace.RunsetExecutor.RunSetConfig.Name) + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
                exec_folder = GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + exec_folder);
                WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = exec_folder;
                int RunnerCount = 1;

                foreach (GingerRunner gingerrunner in WorkSpace.RunsetExecutor.RunSetConfig.GingerRunners)
                {
                    string folder = exec_folder + "\\" + RunnerCount.ToString() + " " + gingerrunner.Name + "\\";
                    if (System.IO.Directory.Exists(folder))
                    {
                        CleanDirectory(folder);
                    }
                    else
                        System.IO.Directory.CreateDirectory(folder);

                    ExecutionLogger ExecutionLogger = new ExecutionLogger(gingerrunner.ProjEnvironment);
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus gingerRunnerStatus = gingerrunner.RunsetStatus;
                    if (gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    RunSetStart(exec_folder, _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, ExecutionLogger.CurrentExecutionDateTime, true);
                    ExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
                    ExecutionLogger.OfflineRunnerExecutionLog(gingerrunner, folder, RunnerCount);
                    RunnerCount++;
                }
                RunSetEnd(exec_folder);
                return exec_folder;
            }
        }

        // Move all report items from here !!!!!!!!!!!!!!!!
        public static void GenerateRunSetOfflineReport()
        {
            try
            {
                HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
                string exec_folder = GetRunSetLastExecutionLogFolderOffline();
                string reportsResultFolder = string.Empty;
                reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false,null, null,false, currentConf.HTMLReportConfigurationMaximalFolderSize);
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

        public static void GenerateBusinessFlowOfflineReport(ProjEnvironment environment, string reportsResultFolder, BusinessFlow BusinessFlow, string RunsetName = null)
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            string exec_folder = string.Empty;
            exec_folder = GenerateBusinessflowOfflineExecutionLogger(environment, BusinessFlow, RunsetName);
            if(string.IsNullOrEmpty(exec_folder))
            {
                Reporter.ToUser(eUserMsgKey.ExecutionsResultsProdIsNotOn);
                return;
            }
            reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false, null, reportsResultFolder,false,currentConf.HTMLReportConfigurationMaximalFolderSize);

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
        public static string GenerateBusinessflowOfflineExecutionLogger(ProjEnvironment environment, BusinessFlow BusinessFlow, string RunsetName = null)
        {            
            ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();

            string exec_folder = string.Empty;
            if(!string.IsNullOrEmpty(RunsetName))
            {
                exec_folder = folderNameNormalazing(RunsetName)  + "_" + BusinessFlow.Name + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
            }
            else
            {
                exec_folder = BusinessFlow.Name + "_" + DateTime.Now.ToString("MMddyyyy_HHmmss");
            }
            exec_folder = GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + exec_folder);
            if (!_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {               
                return string.Empty;
            }            
            if (System.IO.Directory.Exists(exec_folder))
                General.ClearDirectoryContent(exec_folder);
            else
                System.IO.Directory.CreateDirectory(exec_folder);
            ExecutionLogger ExecutionLogger = new ExecutionLogger(environment);
            ExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            ExecutionLogger.OfflineBusinessFlowExecutionLog(BusinessFlow, exec_folder);
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
                    string reportpath = logFolderPath + "\\" + counter.ToString() +" "+ folderNameNormalazing(bf.Name);
                    System.IO.Directory.CreateDirectory(reportpath);
                    this.ExecutionLogBusinessFlowsCounter = counter;       
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
                    runner.CalculateBusinessFlowFinalStatus(bf, true);
                    if (bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    OfflineBusinessFlowExecutionLog(bf, reportpath);
                    mBusinessFlowCounter++;
                    runner.ExecutionLogFolder = runner.ExecutionLogFolder + @"\" + mBusinessFlowCounter + " " + folderNameNormalazing(bf.Name);
                    counter++;
                }
                RunnerRunEnd(meventtime, runner, logFolderPath, runnerCount);
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

        public bool OfflineBusinessFlowExecutionLog(BusinessFlow businessFlow, string logFolderPath)
        {
            try
            {
                //handle root directory
                if (Directory.Exists(logFolderPath))
                    CleanDirectory(logFolderPath);
                else
                    Directory.CreateDirectory(logFolderPath);
                GingerRunner Gr =new GingerRunner();
                mCurrentBusinessFlow = businessFlow;
                businessFlow.ExecutionLogFolder = logFolderPath;
                businessFlow.VariablesBeforeExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.SolutionVariablesBeforeExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                System.IO.Directory.CreateDirectory(businessFlow.ExecutionLogFolder);
                businessFlow.ExecutionLogActivityCounter = 0;
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
                    
                    businessFlow.ExecutionLogActivityCounter++;
                    mCurrentActivity = activity;
                    activity.ExecutionLogFolder = businessFlow.ExecutionLogFolder + @"\" + businessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName);
                    System.IO.Directory.CreateDirectory(activity.ExecutionLogFolder);
                    activity.ExecutionLogActionCounter = 0;
                    activity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                    foreach (Act action in activity.Acts)
                    {
                        if (action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored)
                        {
                            continue;
                        }
                        activity.ExecutionLogActionCounter++;
                        action.ExecutionLogFolder = activity.ExecutionLogFolder + @"\" + activity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description);
                        System.IO.Directory.CreateDirectory(action.ExecutionLogFolder);
                        
                        ActionEnd(meventtime, action, true);
                    }
                    ActivityEnd(meventtime, activity, true);
                }
                Gr.SetActivityGroupsExecutionStatus(businessFlow, true, this);
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
    }
}
