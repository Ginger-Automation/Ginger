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

using Amdocs.Ginger.Common;
using Ginger.Reports;
using GingerCore;

using GingerCore.Variables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Common.InterfacesLib;
using Ginger.Run;
using GingerCoreNET.ReporterLib;

namespace Ginger.Run
{
    // Each ExecutionLogger instance should be attach to one GingerRunner
    // Create new ExecutionLogger for each run 

    public enum eExecutionPahse { Start, End }
    public class ExecutionLogger
    {
 

        public static string defaultAutomationTabLogName = "AutomationTab_LastExecution";
        public static string defaultAutomationTabOfflineLogName = "AutomationTab_OfflineExecution";
        public static string defaultRunTabBFName = "RunTab_BusinessFlowLastExecution";
        public static string defaultRunTabRunConsolidatedName = "RunTab_ConsolidatedReportLastExecution";
        public static string defaultRunTabLogName = "DefaultRunSet";
        static JsonSerializer mJsonSerializer;
        public static string mLogsFolder;
        public string ExecutionLogfolder { get; set; }
        string mLogsFolderName;
        DateTime mCurrentExecutionDateTime;
        int BFCounter = 0;
        private Amdocs.Ginger.Common.eExecutedFrom ExecutedFrom;
        public IBusinessFlow CurrentBusinessFlow;

        IValueExpression mVE;

        private Ginger.Reports.GingerReport gingerReport = new GingerReport();
        //public bool gingerReportClosed = false;
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

       
        public ParentGingerData GingerData { get; set; } = new ParentGingerData();

        public ExecutionLogger(Amdocs.Ginger.Common.eExecutedFrom executedFrom = Amdocs.Ginger.Common.eExecutedFrom.Run)
        {
            mJsonSerializer = new JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            ExecutedFrom = executedFrom;
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
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while creating temporary folder", ex);
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
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
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
            {                
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

        public void ExecutionStart(ReportInfo RI)
        {

        }

        public void ExecutionEnd(ReportInfo RI)
        {

        }

        public void ActivityGroupStart(IActivitiesGroup currentActivityGroup, IBusinessFlow businessFlow)
        {
            currentActivityGroup.StartTimeStamp = DateTime.Now.ToUniversalTime();
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                AddExecutionDetailsToLog(eExecutionPahse.Start, "Activity Group", currentActivityGroup.Name, null);
            }
        }

        public void ActivityGroupEnd(IActivitiesGroup currentActivityGroup, IBusinessFlow businessFlow, bool offlineMode = false)
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                if (currentActivityGroup.ExecutedActivities.Count > 0)
                {
                    currentActivityGroup.EndTimeStamp = currentActivityGroup.ExecutedActivities.Max(x => x.Value);
                    currentActivityGroup.Elapsed = (currentActivityGroup.EndTimeStamp - currentActivityGroup.StartTimeStamp).TotalMilliseconds;
                }

                ActivityGroupReport AGR = new ActivityGroupReport(currentActivityGroup, businessFlow);
                AGR.Seq = businessFlow.ActivitiesGroups.IndexOf(currentActivityGroup) + 1;
                AGR.ExecutionLogFolder = ExecutionLogfolder + businessFlow.ExecutionLogFolder;
                if (offlineMode && currentActivityGroup.ExecutionLogFolder != null)
                {
                    SaveObjToJSonFile(AGR, currentActivityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                    File.AppendAllText(currentActivityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
                }
                else
                {
                    if(offlineMode)
                    {
                        SaveObjToJSonFile(AGR, businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                        File.AppendAllText(businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
                    }
                    else
                    {
                        SaveObjToJSonFile(AGR, ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                        File.AppendAllText(ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
                    }
                }

                if (!offlineMode)
                    AddExecutionDetailsToLog(eExecutionPahse.End, "Activity Group", currentActivityGroup.Name, AGR);
            }
        }

        public void GingerStart()
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
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

                AddExecutionDetailsToLog(eExecutionPahse.Start, "Ginger Runner", this.GingerData.GingerName.ToString(), null);
            }
        }


        public void GingerEnd(IGingerRunner GRN = null, string filename = null, int runnerCount = 0)
        {
            GingerRunner GR = (GingerRunner)GRN;
            if (GR == null)
            {
                if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
                {
                    gingerReport.Seq = this.GingerData.Seq;
                    gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                    gingerReport.GUID = this.GingerData.Ginger_GUID.ToString();
                    gingerReport.Name = this.GingerData.GingerName.ToString();
                    gingerReport.ApplicationAgentsMappingList = this.GingerData.GingerAggentMapping;
                    gingerReport.EnvironmentName = this.GingerData.GingerEnv != null ? this.GingerData.GingerEnv.ToString() : string.Empty;
                    gingerReport.Elapsed = (double)gingerReport.Watch.ElapsedMilliseconds / 1000;
                    SaveObjToJSonFile(gingerReport, gingerReport.LogFolder + @"\Ginger.txt");
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    this.BFCounter = 0;
                    AddExecutionDetailsToLog(eExecutionPahse.End, "Ginger Runner", gingerReport.Name, gingerReport);
                }
            }

            else
            {
                gingerReport.Seq = runnerCount;
                gingerReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                gingerReport.GUID = GR.Guid.ToString();
                gingerReport.Name = GR.Name;
                gingerReport.ApplicationAgentsMappingList = GR.ApplicationAgents.Select(a => a.AgentName+ "_:_" + a.AppName).ToList();
                gingerReport.EnvironmentName = GR.ProjEnvironment != null ? GR.ProjEnvironment.Name : string.Empty;
                gingerReport.Elapsed = (double)GR.Elapsed / 1000;
                gingerReport.LogFolder = filename;
                SaveObjToJSonFile(gingerReport, gingerReport.LogFolder + @"\Ginger.txt");
                this.ExecutionLogBusinessFlowsCounter = 0;
                this.BFCounter = 0;
                AddExecutionDetailsToLog(eExecutionPahse.End, "Ginger Runner", gingerReport.Name, gingerReport);
            }
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
                AddExecutionDetailsToLog(eExecutionPahse.Start, "Run Set", RunSetReport.Name, null);
            }
        }

        public static void RunSetEnd(string LogFolder=null)
        {
            if (RunSetReport != null)
            {
                RunSetReport.EndTimeStamp = DateTime.Now.ToUniversalTime();
                RunSetReport.Elapsed = (double)RunSetReport.Watch.ElapsedMilliseconds / 1000;
                RunSetReport.MachineName = Environment.MachineName.ToString();
                RunSetReport.ExecutedbyUser = Environment.UserName.ToString();               
                if (LogFolder==null)
                 SaveObjToJSonFile(RunSetReport, RunSetReport.LogFolder + @"\RunSet.txt");
                else
                    SaveObjToJSonFile(RunSetReport, LogFolder + @"\RunSet.txt");
                AddExecutionDetailsToLog(eExecutionPahse.End, "Run Set", RunSetReport.Name, RunSetReport);
                if (WorkSpace.RunningFromConfigFile)
                {
                    //Amdocs.Ginger.CoreNET.Execution.eRunStatus.TryParse(RunSetReport.RunSetExecutionStatus, out App.RunSetExecutionStatus);//saving the status for determin Ginger exit code
                    WorkSpace.RunSetExecutionStatus = RunSetReport.RunSetExecutionStatus;
                }
                if(WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null && WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder.Equals("-1"))
                    WorkSpace.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;
                //App.RunPage.RunSetConfig.LastRunsetLoggerFolder = RunSetReport.LogFolder;
                RunSetReport = null;
            }
        }

        public void BusinessFlowStart(IBusinessFlow BusinessFlow)
        {
            CurrentBusinessFlow = BusinessFlow;
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                this.BFCounter++;
                BusinessFlow.StartTimeStamp = DateTime.Now.ToUniversalTime();
                string BFFolder = string.Empty;
                this.ExecutionLogBusinessFlowsCounter++;
                switch (this.ExecutedFrom)
                {
                    case Amdocs.Ginger.Common.eExecutedFrom.Automation:
                        if (Configuration.ExecutionLoggerAutomationTabContext == ExecutionLoggerConfiguration.AutomationTabContext.BussinessFlowRun)
                        {
                            ExecutionLogfolder = GetLoggerDirectory(ExecutionLogfolder);
                            CleanDirectory(ExecutionLogfolder);
                        }
                        else
                            return;
                        break;
                    default:
                        BFFolder = BFCounter + " " + folderNameNormalazing(BusinessFlow.Name);
                        break;
                }
                BusinessFlow.VariablesBeforeExec = BusinessFlow.Variables.Select(a => a.Name+ "_:_" + a.Value + "_:_" + a.Description).ToList();
                BusinessFlow.SolutionVariablesBeforeExec = BusinessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                BusinessFlow.ExecutionLogFolder = BFFolder;

                System.IO.Directory.CreateDirectory(ExecutionLogfolder + BFFolder);

                AddExecutionDetailsToLog(eExecutionPahse.Start, "Business Flow", BusinessFlow.Name, null);
            }
        }

        public void BusinessFlowEnd(IBusinessFlow BusinessFlow, bool offlineMode = false)
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                if (!offlineMode)
                    BusinessFlow.EndTimeStamp = DateTime.Now.ToUniversalTime();
                
                BusinessFlowReport BFR = new BusinessFlowReport(BusinessFlow);
                BFR.VariablesBeforeExec = BusinessFlow.VariablesBeforeExec; /// Why??
                BFR.SolutionVariablesBeforeExec = BusinessFlow.SolutionVariablesBeforeExec;
                BFR.Seq = this.ExecutionLogBusinessFlowsCounter;
                if ((BusinessFlow.RunDescription != null) && (BusinessFlow.RunDescription != string.Empty))
                {
                    if (mVE == null)
                    {
                        mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(WorkSpace.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<IDataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                    }
                    mVE.Value = BusinessFlow.RunDescription;
                    BFR.RunDescription = mVE.ValueCalculated;
                }

                if (offlineMode)
                {
                    SaveObjToJSonFile(BFR, BusinessFlow.ExecutionLogFolder + @"\BusinessFlow.txt");
                    BusinessFlow.ExecutionFullLogFolder = BusinessFlow.ExecutionLogFolder;
                }
                else
                {
                    SaveObjToJSonFile(BFR, ExecutionLogfolder + BusinessFlow.ExecutionLogFolder + @"\BusinessFlow.txt");
                    BusinessFlow.ExecutionFullLogFolder = ExecutionLogfolder + BusinessFlow.ExecutionLogFolder;
                }
                if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                {
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    this.BFCounter = 0;
                }
                if (!offlineMode)
                    AddExecutionDetailsToLog(eExecutionPahse.End, "Business Flow", BusinessFlow.Name, BFR);
            }
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

        public void ActivityStart(IBusinessFlow BusinessFlow, IActivity Activity)
        {
            Activity.StartTimeStamp = DateTime.Now.ToUniversalTime();
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
                    BusinessFlow.ExecutionLogActivityCounter++;
                    ActivityFolder = BusinessFlow.ExecutionLogFolder + @"\" + BusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(Activity.ActivityName);
                }
                else
                {
                    BusinessFlow.ExecutionLogActivityCounter++;
                    ActivityFolder = BusinessFlow.ExecutionLogFolder + @"\" + BusinessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(Activity.ActivityName);
                }

                Activity.ExecutionLogFolder = ActivityFolder;
                System.IO.Directory.CreateDirectory(ExecutionLogfolder + ActivityFolder);
                Activity.VariablesBeforeExec = Activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();

                AddExecutionDetailsToLog(eExecutionPahse.Start, GingerDicser.GetTermResValue(eTermResKey.Activity), Activity.ActivityName, null);
            }
        }

        public void ActivityEnd(IBusinessFlow BusinessFlow, IActivity Activity, bool offlineMode = false)
        {
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                if (!offlineMode)
                    Activity.EndTimeStamp = DateTime.Now.ToUniversalTime();
                ActivityReport AR = new ActivityReport(Activity);
                AR.Seq = BusinessFlow.ExecutionLogActivityCounter;
                AR.VariablesBeforeExec = Activity.VariablesBeforeExec;

                if ((Activity.RunDescription != null) && (Activity.RunDescription != string.Empty))
                {
                    if (mVE == null)
                    {
                        mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(WorkSpace.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<IDataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                    }
                    mVE.Value = Activity.RunDescription;
                    AR.RunDescription = mVE.ValueCalculated;
                }

                if (offlineMode)
                    SaveObjToJSonFile(AR, Activity.ExecutionLogFolder + @"\Activity.txt");
                else
                    SaveObjToJSonFile(AR, ExecutionLogfolder + Activity.ExecutionLogFolder + @"\Activity.txt");

                if (!offlineMode)
                    AddExecutionDetailsToLog(eExecutionPahse.End, GingerDicser.GetTermResValue(eTermResKey.Activity), Activity.ActivityName, AR);
            }
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


        public void ActionStart(IBusinessFlow businessFlow, IActivity Activity, IAct act)
        {
            CurrentBusinessFlow =businessFlow;
            if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                string ActionFolder = string.Empty;
                act.StartTimeStamp = DateTime.Now.ToUniversalTime();

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
                    Activity.ExecutionLogActionCounter++;
                    ActionFolder = Activity.ExecutionLogFolder + @"\" + Activity.ExecutionLogActionCounter + " " + folderNameNormalazing(act.Description);
                }
                else
                {
                    Activity.ExecutionLogActionCounter++;
                    ActionFolder = Activity.ExecutionLogFolder + @"\" + Activity.ExecutionLogActionCounter + " " + folderNameNormalazing(act.Description);
                }
             act.ExecutionLogFolder = ActionFolder;
                System.IO.Directory.CreateDirectory(ExecutionLogfolder + ActionFolder);

                AddExecutionDetailsToLog(eExecutionPahse.Start, "Action", act.Description, null);
            }
        }

        public void ActionEnd(IActivity Activity, IAct act, bool offlineMode = false)
        {
            // if user set special action log in output
            if (act.EnableActionLogConfig)
            {                                             
                if (mGingerRunnerLogger == null)
                {
                    string loggerFile = Path.Combine(ExecutionLogfolder, FileSystem.AppendTimeStamp("GingerLog.txt"));
                    mGingerRunnerLogger = new GingerRunnerLogger(loggerFile);
                }
                mGingerRunnerLogger.LogAction(act);
            }
            
            try
            {
                string executionLogFolder = string.Empty;
                //if offline mode then execution logger path exists in action object so making executionLogFolder empty to avoid duplication of path.
                if (!offlineMode)
                    executionLogFolder = ExecutionLogfolder;
                if (this.Configuration.ExecutionLoggerConfigurationIsEnabled)
                {
                    if (System.IO.Directory.Exists(executionLogFolder + act.ExecutionLogFolder))
                    {
                        if (!offlineMode)
                            act.EndTimeStamp = DateTime.Now.ToUniversalTime();
                        IProjEnvironment environment = null;

                        if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                        {
                            environment = WorkSpace.AutomateTabEnvironment;
                        }
                        else
                        {
                            environment = WorkSpace.RunsetExecutor.RunsetExecutionEnvironment;
                        }

                        ActionReport AR = new ActionReport(act,environment);
                        AR.Seq = Activity.ExecutionLogActionCounter;
                        if ((act.RunDescription != null) && (act.RunDescription != string.Empty))
                        {
                            if (mVE == null)
                            {
                                mVE = RepositoryItemHelper.RepositoryItemFactory.CreateValueExpression(WorkSpace.RunsetExecutor.RunsetExecutionEnvironment, null, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<IDataSourceBase>(), false, "", false, WorkSpace.Instance.Solution.Variables);
                            }
                            mVE.Value = act.RunDescription;
                            AR.RunDescription = mVE.ValueCalculated;
                        }

                        SaveObjToJSonFile(AR, executionLogFolder + act.ExecutionLogFolder + @"\Action.txt");

                        // Save screenShots
                        int screenShotCountPerAction = 0;
                        for (var s = 0; s < act.ScreenShots.Count; s++)
                        {
                            try
                            {
                                screenShotCountPerAction++;
                                if (this.ExecutedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                                {
                                    System.IO.File.Copy(act.ScreenShots[s], executionLogFolder + act.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png", true);
                                }
                                else
                                {
                                    System.IO.File.Move(act.ScreenShots[s], executionLogFolder + act.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png");
                                    act.ScreenShots[s] = executionLogFolder + act.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png";
                                }
                            }
                            catch (Exception ex)
                            {
                                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to move screen shot of the action:'" + act.Description + "' to the Execution Logger folder", ex);
                                screenShotCountPerAction--;
                            }
                        }

                        if (!offlineMode)
                            AddExecutionDetailsToLog(eExecutionPahse.End, "Action", act.Description, AR);
                    }
                    else
                    {
                        AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + act.Description + " because directory not exists :" + executionLogFolder + act.ExecutionLogFolder);
                    }
                }

                //
                // Defects Suggestion section (to be considered to remove to separate function)
                //
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    if (WorkSpace.RunsetExecutor.DefectSuggestionsList.Where(z => z.FailedActionGuid == act.Guid).ToList().Count > 0)
                        return;

                    //
                    IActivitiesGroup currrentGroup = this.CurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == Activity.ActivitiesGroupID).FirstOrDefault();
                    string currrentGroupName = string.Empty;
                    if (currrentGroup != null)
                        currrentGroupName = currrentGroup.Name;

                    //
                    List<string> screenShotsPathes = new List<string>();
                    bool isScreenshotButtonEnabled = false;
                    if ((act.ScreenShots != null) && (act.ScreenShots.Count > 0))
                    {
                        screenShotsPathes = act.ScreenShots;
                        isScreenshotButtonEnabled = true;
                    }
                    // 
                    bool automatedOpeningFlag = false;
                    if (act.FlowControls.Where(x => x.FlowControlAction == eFlowControlAction.FailureIsAutoOpenedDefect && x.Condition == "\"{ActionStatus}\" = \"Failed\"").ToList().Count > 0)
                        automatedOpeningFlag = true;

                    //
                    StringBuilder description = new StringBuilder();
                    description.Append("&#60;html&#62;&#60;body&#62;&#60;b&#62;" + this.GingerData.GingerName + "&#60;b&#62;&#60;br&#62;");
                    description.Append("&#60;div&#62;&#60;ul style='list - style - type:circle'&#62;&#60;li&#62;&#60;b&#62;" + this.CurrentBusinessFlow.Name + " (failed)&#60;b&#62;&#60;/li&#62;");
                    if (currrentGroupName != string.Empty)
                    {
                        description.Append("&#60;ul style = 'list - style - type:square'&#62;");
                        this.CurrentBusinessFlow.ActivitiesGroups.ToList().TakeWhile(x => x.Name != Activity.ActivitiesGroupID).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Name + "&#60;/li&#62;"); });
                        description.Append("&#60;li&#62;&#60;b&#62;" + currrentGroupName + " (failed)&#60;b&#62;&#60;/li&#62;");
                        description.Append("&#60;ul style = 'list - style - type:upper-roman'&#62;");
                        this.CurrentBusinessFlow.Activities.Where(x => currrentGroup.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(x.Guid)).ToList().TakeWhile(x => x.Guid != Activity.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.ActivityName + "&#60;/li&#62;"); });
                        description.Append("&#60;li&#62;&#60;b&#62;" + Activity.ActivityName + " (failed)&#60;b&#62;&#60;/li&#62;");
                        description.Append("&#60;ul style = 'list - style - type:disc'&#62;");
                        Activity.Acts.TakeWhile(x => x.Guid != act.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Description + "&#60;/li&#62;"); });
                        description.Append("&#60;li&#62;&#60;b&#62;&#60;font color='#ff0000'b&#62;" + act.Description + " (failed)&#60;/font&#62;&#60;b&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/div&#62;&#60;/body&#62;&#60;/html&#62;");
                    }
                    else
                    {
                        description.Append("&#60;ul style = 'list - style - type:upper-roman'&#62;");
                        this.CurrentBusinessFlow.Activities.TakeWhile(x => x.Guid != Activity.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.ActivityName + "&#60;/li&#62;"); });
                        description.Append("&#60;li&#62;&#60;b&#62;" + Activity.ActivityName + " (failed)&#60;b&#62;&#60;/li&#62;");
                        description.Append("&#60;ul style = 'list - style - type:disc'&#62;");
                        Activity.Acts.TakeWhile(x => x.Guid != act.Guid).ToList().ForEach(r => { description.Append("&#60;li&#62;" + r.Description + "&#60;/li&#62;"); });
                        description.Append("&#60;li&#62;&#60;b&#62;&#60;font color='#ff0000'b&#62;" + act.Description + " (failed)&#60;/font&#62;&#60;b&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/li&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/ul&#62;&#60;/div&#62;&#60;/body&#62;&#60;/html&#62;");
                    }


                    WorkSpace.RunsetExecutor.DefectSuggestionsList.Add(new DefectSuggestion(act.Guid, this.GingerData.GingerName, this.CurrentBusinessFlow.Name, currrentGroupName,
                                                                                        CurrentBusinessFlow.ExecutionLogActivityCounter, Activity.ActivityName, Activity.ExecutionLogActionCounter,
                                                                                        act.Description, act.RetryMechanismCount, act.Error, act.ExInfo, screenShotsPathes,
                                                                                        isScreenshotButtonEnabled, automatedOpeningFlag, description.ToString()));
                }
                //
                // Defects Suggestion section - end
                //
            }
            catch(Exception ex)
            {
               AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Exception occurred in ExecutionLogger Action end", ex);
            }                   
        }

        private static void AddExecutionDetailsToLog(eExecutionPahse objExecutionPhase, string objType, string objName, object obj)
{
    /*  if (AppReporter.CurrentAppLogLevel == eAppReporterLoggingLevel.Debug)
      {
          string prefix = string.Empty;
          switch (objExecutionPhase)
          {
              case eExecutionPahse.Start:
                  prefix = "--> Execution Started for the " + objType + ": '" + objName + "'";
                  break;
              case eExecutionPahse.End:
                  prefix = "<-- Execution Ended for the " + objType + ": '" + objName + "'";
                  break;
          }

          //get the execution fields and their values
          if (obj != null)
          {
              List<KeyValuePair<string, string>> fieldsAndValues = new List<KeyValuePair<string, string>>();
              try
              {
                  PropertyInfo[] props = obj.GetType(.GetProperties();
                  foreach (PropertyInfo prop in props)
                  {
                      try
                      {
                          FieldParamsFieldType attr = ((FieldParamsFieldType)prop.GetCustomAttribute(typeof(FieldParamsFieldType)));
                          if (attr == null)
                          {
                              continue;
                          }
                          FieldsType ftype = attr.FieldType;
                          if (ftype == FieldsType.Field)
                          {
                              string propName = prop.Name;
                              string propFullName = ((FieldParamsNameCaption)prop.GetCustomAttribute(typeof(FieldParamsNameCaption)).NameCaption;
                              string propValue = obj.GetType(.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance.GetValue(obj.ToString();
                              fieldsAndValues.Add(new KeyValuePair<string, string>(propFullName, propValue));
                          }
                      }
                      catch (Exception)
                      {
                          //TODO: !!!!!!!!!!!!!!!!!! FIXME
                      }
                  }
              }
              catch (Exception)
              {
                  //TODO: !!!!!!!!!!!!!!!!!! FIXME
              }

              //add to Console
              string details = string.Empty;
              foreach (KeyValuePair<string, string> det in fieldsAndValues)
                  details += det.Key + "= " + det.Value + System.Environment.NewLine;
              Reporter.ToLog(eAppReporterLogLevel.INFO, prefix + System.Environment.NewLine + "Details:" + System.Environment.NewLine + details);
          }
          else
          {
              Reporter.ToLog(eAppReporterLogLevel.INFO, prefix + System.Environment.NewLine);
          }
      }
     */
}


        public void VariableChanged(VariableBase VB, string OriginalValue)
        {

        }

        public ReportInfo GetReportInfo()
        {
            //TODO: check what is needed.
            ReportInfo RI = new ReportInfo(ExecutionLogfolder);
            return RI;
        }

        public String GetLastExecutedActivityRunStatus(IActivity CurrentActivity)
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
                 //TODO   AppReporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
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

                    ExecutionLogger ExecutionLogger = new ExecutionLogger();
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus gingerRunnerStatus = gingerrunner.RunsetStatus;
                    if (gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && gingerRunnerStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    Ginger.Run.ExecutionLogger.RunSetStart(exec_folder, _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, ExecutionLogger.CurrentExecutionDateTime, true);
                    ExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
                    ExecutionLogger.OfflineRunnerExecutionLog(gingerrunner, folder, RunnerCount);
                    RunnerCount++;
                }
                RunSetEnd(exec_folder);
                return exec_folder;
            }
        }

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
               //TODO     AppReporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Failed to generate the report for the '" + WorkSpace.Businessflow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ", please execute it fully first.");
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
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        public static void GenerateBusinessFlowOfflineReport(string reportsResultFolder, IBusinessFlow BusinessFlow, string RunsetName = null)
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            string exec_folder = string.Empty;
            exec_folder = GenerateBusinessflowOfflineExecutionLogger(BusinessFlow, RunsetName);
            if(string.IsNullOrEmpty(exec_folder))
            {
                Reporter.ToUser(eUserMsgKeys.ExecutionsResultsProdIsNotOn);
                return;
            }
            reportsResultFolder = Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false, null, reportsResultFolder,false,currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKeys.AutomationTabExecResultsNotExists);
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
        public static string GenerateBusinessflowOfflineExecutionLogger(IBusinessFlow BusinessFlow, string RunsetName = null)
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
            ExecutionLogger ExecutionLogger = new ExecutionLogger();
            ExecutionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            ExecutionLogger.OfflineBusinessFlowExecutionLog(BusinessFlow, exec_folder);
            return exec_folder;
        }
        public bool OfflineRunnerExecutionLog(IGingerRunner runner, string logFolderPath, int runnerCount = 0)
        {
            try
            {
                runner.ExecutionLogBusinessFlowCounter = 0;
                ObservableList<IBusinessFlow> listBF = runner.BusinessFlows;
                int counter = 1;
                foreach (IBusinessFlow bf in listBF)
                {
                    string reportpath = logFolderPath + "\\" + counter.ToString() +" "+ folderNameNormalazing(bf.Name);
                    System.IO.Directory.CreateDirectory(reportpath);
                    this.ExecutionLogBusinessFlowsCounter = counter;                    
                    runner.CalculateBusinessFlowFinalStatus(bf, true);
                    if (bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && bf.RunStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    OfflineBusinessFlowExecutionLog(bf, reportpath);
                    runner.ExecutionLogBusinessFlowCounter++;
                    runner.ExecutionLogFolder = runner.ExecutionLogFolder + @"\" + runner.ExecutionLogBusinessFlowCounter + " " + folderNameNormalazing(bf.Name);
                    counter++;
                }    
                
                GingerEnd(runner, logFolderPath, runnerCount);
                runner.ExecutionLogFolder = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
             //TODO   Reporter.ToLog(eAppReporterLogLevel.ERROR, "Execution Logger Failed to do Offline BusinessFlow Execution Log", ex);
                return false;
            }
        }
       
        public bool OfflineBusinessFlowExecutionLog(IBusinessFlow businessFlow, string logFolderPath)
        {
            try
            {
                //handle root directory
                if (Directory.Exists(logFolderPath))
                    CleanDirectory(logFolderPath);
                else
                    Directory.CreateDirectory(logFolderPath);
                GingerRunner Gr = new GingerRunner();

                businessFlow.ExecutionLogFolder = logFolderPath;
                businessFlow.VariablesBeforeExec = businessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                businessFlow.SolutionVariablesBeforeExec = businessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                System.IO.Directory.CreateDirectory(businessFlow.ExecutionLogFolder);
                businessFlow.ExecutionLogActivityCounter = 0;
                foreach (IActivity activity in businessFlow.Activities)
                {


                    IActivitiesGroup currentActivityGroup = businessFlow.ActivitiesGroups.Where(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid)).FirstOrDefault();
                    if (currentActivityGroup != null)
                    {
                        currentActivityGroup.ExecutionLogFolder = logFolderPath;
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case executionLoggerStatus.NotStartedYet:
                                ActivityGroupStart(currentActivityGroup, businessFlow);
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
                    activity.ExecutionLogFolder = businessFlow.ExecutionLogFolder + @"\" + businessFlow.ExecutionLogActivityCounter + " " + folderNameNormalazing(activity.ActivityName);
                    System.IO.Directory.CreateDirectory(activity.ExecutionLogFolder);
                    activity.ExecutionLogActionCounter = 0;
                    activity.VariablesBeforeExec = activity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                    foreach (IAct action in activity.Acts)
                    {
                        if (action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored)
                        {
                            continue;
                        }
                        activity.ExecutionLogActionCounter++;
                        action.ExecutionLogFolder = activity.ExecutionLogFolder + @"\" + activity.ExecutionLogActionCounter + " " + folderNameNormalazing(action.Description);
                        System.IO.Directory.CreateDirectory(action.ExecutionLogFolder);
                        ActionEnd(activity, action, true);
                    }
                    ActivityEnd(businessFlow, activity, true);
                }
                Gr.SetActivityGroupsExecutionStatus(businessFlow, true, this);
                Gr.CalculateBusinessFlowFinalStatus(businessFlow);
                BusinessFlowEnd(businessFlow, true);
                businessFlow.ExecutionLogFolder = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                AppReporter.ToLog(eAppReporterLogLevel.ERROR, "Execution Logger Failed to do Offline BusinessFlow Execution Log", ex);
                return false;
            }
        }
    }
}
