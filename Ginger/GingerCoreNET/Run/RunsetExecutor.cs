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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GingerCore;
using Ginger.Reports;
using GingerCore.Variables;
using Ginger.Run.RunSetActions;
using System.ComponentModel;
using System.Reflection;
using GingerCoreNET.SourceControl;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger;
using Amdocs.Ginger.CoreNET.Execution;

using Amdocs.Ginger.CoreNET;
using GingerCore.Environments;
using Ginger.Reports.GingerExecutionReport;


namespace Ginger.Run
{
    public class RunsetExecutor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private Stopwatch mStopwatch = new Stopwatch();
        public TimeSpan Elapsed { get { return mStopwatch.Elapsed; } }
        private ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = new ExecutionLoggerConfiguration();
        public bool mStopRun;

        private string _currentRunSetLogFolder = string.Empty;
        private string _currentHTMLReportFolder = string.Empty;
        ObservableList<DefectSuggestion> mDefectSuggestionsList = new ObservableList<DefectSuggestion>();

        RunSetConfig mRunSetConfig = null;
        public RunSetConfig RunSetConfig
        {
            get
            {
                return mRunSetConfig;
            }
            set
            {
                if (mRunSetConfig == null || mRunSetConfig.Equals(value) == false)
                {
                    mRunSetConfig = value;
                    OnPropertyChanged(nameof(RunSetConfig));
                }
            }
        }

        
        public ObservableList<GingerRunner> Runners
        {
            get
            {
                return mRunSetConfig.GingerRunners;
            }
        }

        private ProjEnvironment mRunsetExecutionEnvironment = null;
        public ProjEnvironment RunsetExecutionEnvironment
        {
            get
            {
                return mRunsetExecutionEnvironment;
            }
            set
            {
                mRunsetExecutionEnvironment = value;
                if (mRunsetExecutionEnvironment != null)
                   WorkSpace.UserProfile.RecentEnvironment = mRunsetExecutionEnvironment.Guid;
                OnPropertyChanged(nameof(this.RunsetExecutionEnvironment));
            }
        }

        public void ConfigureAllRunnersForExecution()
        {
            foreach (GingerRunner runner in Runners)
            {
                ConfigureRunnerForExecution(runner);
            }
        }

        public ObservableList<DefectSuggestion> DefectSuggestionsList
        {
            get { return mDefectSuggestionsList; }
            set { mDefectSuggestionsList = value; }
        }

        public void ConfigureRunnerForExecution(GingerRunner runner)
        {
            runner.SetExecutionEnvironment(RunsetExecutionEnvironment, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>());
            runner.CurrentSolution = WorkSpace.Instance.Solution;
            runner.SolutionAgents = RepositoryItemHelper.RepositoryItemFactory.GetAllIAgents();
            runner.DSList = RepositoryItemHelper.RepositoryItemFactory.GetDatasourceList();
            runner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            runner.SolutionFolder = WorkSpace.Instance.Solution.Folder;

            // runner.RunListeners.Add(new ExecutionLogger(RunsetExecutionEnvironment));
        }

        public void InitRunner(GingerRunner runner)
        {
            //Configure Runner for execution
            runner.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            ConfigureRunnerForExecution(runner);

            //Set the Apps agents
            foreach (IApplicationAgent p in runner.ApplicationAgents)
            {
                if (p.AgentName != null)
                {
                    ObservableList<IAgent> IAgents = RepositoryItemHelper.RepositoryItemFactory.GetAllIAgents();
                    p.Agent = (from a in IAgents where a.Name == p.AgentName select a).FirstOrDefault();
                }
            }

            //Load the biz flows     
            runner.BusinessFlows.Clear();
            foreach (BusinessFlowRun bf in runner.BusinessFlowsRunList)
            {
                ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                BusinessFlow BF1 = (from bfr in businessFlows where bfr.Guid == bf.BusinessFlowGuid select bfr).FirstOrDefault();
                if (BF1 == null)
                    BF1 = (from bfr in businessFlows where bfr.Name == bf.BusinessFlowName select bfr).FirstOrDefault();
                if (BF1 == null)
                {
                    //Reporter.ToUser(eUserMsgKey.CannontFindBusinessFlow, bf.BusinessFlowName);
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Can not find the '{0}' {1} for the '{2}' {3}", bf.BusinessFlowName, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), mRunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    continue;
                }
                else
                {
                    // Very slow
                    BusinessFlow BFCopy = (BusinessFlow)BF1.CreateCopy(false);
                    BFCopy.Reset();
                    BFCopy.Active = bf.BusinessFlowIsActive;
                    BFCopy.Mandatory = bf.BusinessFlowIsMandatory;
                    BFCopy.AttachActivitiesGroupsAndActivities();
                    if (bf.BusinessFlowInstanceGuid == Guid.Empty) {
                        BFCopy.InstanceGuid = Guid.NewGuid();
                    }
                    else {
                        BFCopy.InstanceGuid = bf.BusinessFlowInstanceGuid;
                    }
                    if (bf.BusinessFlowCustomizedRunVariables != null && bf.BusinessFlowCustomizedRunVariables.Count > 0)
                    {
                        foreach (VariableBase varb in BFCopy.GetBFandActivitiesVariabeles(true))
                        {
                            VariableBase runVar = bf.BusinessFlowCustomizedRunVariables.Where(v => v.ParentGuid == varb.ParentGuid && v.ParentName == varb.ParentName && v.Name == varb.Name).FirstOrDefault();
                            if (runVar != null)
                            {
                                RepositoryItemBase.ObjectsDeepCopy(runVar, varb);
                                varb.DiffrentFromOrigin = runVar.DiffrentFromOrigin;
                                varb.MappedOutputVariable = runVar.MappedOutputVariable;
                            }
                            else
                            {
                                varb.DiffrentFromOrigin = false;
                                varb.MappedOutputVariable = null;
                            }
                        }
                    }
                    BFCopy.RunDescription = bf.BusinessFlowRunDescription;
                    BFCopy.BFFlowControls = bf.BFFlowControls;
                    runner.BusinessFlows.Add(BFCopy);
                }
            }
        }     

        public ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false)
        {
            ObservableList<BusinessFlowExecutionSummary> BFESs = new ObservableList<BusinessFlowExecutionSummary>();
            foreach (GingerRunner ARC in Runners)
            {
                BFESs.Append(ARC.GetAllBusinessFlowsExecutionSummary(GetSummaryOnlyForExecutedFlow, ARC.Name));
            }
            return BFESs;
        }

        internal void CloseAllEnvironments()
        {
            foreach (GingerRunner gr in Runners)
            {
                if (gr.UseSpecificEnvironment)
                {
                    if (gr.ProjEnvironment != null)
                    {
                        gr.ProjEnvironment.CloseEnvironment();
                    }
                }
            }
            //if (Helper.RuntimeObjectFactory.RunExecutioFrom(eExecutedFrom.Automation).ProjEnvironment != null) { }
            //RepositoryItemHelper.RepositoryItemFactory.RunExecutioFrom(eExecutedFrom.Automation).ProjEnvironment.CloseEnvironment();
            if (RunsetExecutionEnvironment != null)
            {
                RunsetExecutionEnvironment.CloseEnvironment();
            }
        }


        public void SetRunnersEnv(ProjEnvironment defualtEnv, ObservableList<ProjEnvironment> allEnvs)
        {
            foreach (GingerRunner GR in Runners)
            {
                GR.SetExecutionEnvironment(defualtEnv, allEnvs);
            }
        }

        
        public void SetRunnersExecutionLoggerConfigs()
        {
            _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();

            if (_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                DateTime currentExecutionDateTime = DateTime.Now;

                ExecutionLogger.RunSetStart(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder, _selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, currentExecutionDateTime);

                int ginger_index = 0;
                while (Runners.Count > ginger_index)
                {
                    Runners[ginger_index].ExecutionLogger.GingerData.Seq = ginger_index + 1;
                    Runners[ginger_index].ExecutionLogger.GingerData.GingerName = Runners[ginger_index].Name;
                    Runners[ginger_index].ExecutionLogger.GingerData.Ginger_GUID = Runners[ginger_index].Guid;
                    Runners[ginger_index].ExecutionLogger.GingerData.GingerAggentMapping = Runners[ginger_index].ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                    Runners[ginger_index].ExecutionLogger.GingerData.GingerEnv = Runners[ginger_index].ProjEnvironment.Name.ToString();
                    Runners[ginger_index].ExecutionLogger.CurrentExecutionDateTime = currentExecutionDateTime;
                    Runners[ginger_index].ExecutionLogger.Configuration = _selectedExecutionLoggerConfiguration;
                    ginger_index++;
                }
            }
        }

        public async Task<int> RunRunsetAsync(bool doContinueRun = false)
        {
            var result = await Task.Run(() =>
            {
                RunRunset(doContinueRun);
                return 1;
            });
            return result;
        }
        
        public void RunRunset(bool doContinueRun=false)
        {
            List<Task> runnersTasks = new List<Task>();
            
            //reset run       
            if (doContinueRun == false)
            {
                RunSetConfig.LastRunsetLoggerFolder = "-1";
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Reseting {0} elements", GingerDicser.GetTermResValue(eTermResKey.RunSet)));                
                mStopwatch.Reset();
                ResetRunnersExecutionDetails();
            }
            else
            {
                RunSetConfig.LastRunsetLoggerFolder = null;
            }
            mStopRun = false;

            //configure Runners for run
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Configurating {0} elements for execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            ConfigureAllRunnersForExecution();
           
            //Process all pre execution Run Set Operations
            if (doContinueRun == false)
            {
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running Pre-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //Dispatcher.CurrentDispatcher.Invoke(() => //ToDO: Remove dependency on UI thread- it should run in backend
                //{
                    WorkSpace.RunsetExecutor.ProcessRunSetActions(new List<RunSetActionBase.eRunAt> { RunSetActionBase.eRunAt.ExecutionStart, RunSetActionBase.eRunAt.DuringExecution });
                //});
            }

            //Start Run 
            if (doContinueRun == false)
            {
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Started: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
                SetRunnersExecutionLoggerConfigs();//contains ExecutionLogger.RunSetStart()
            }
            else
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Continuation: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
            mStopwatch.Start();
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## {0} Runners Execution Started", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            if (RunSetConfig.RunModeParallel)
            {
                //running parallel 
                foreach (GingerRunner GR in Runners)
                {
                    if (mStopRun) return;

                    Task t = new Task(() => 
                    {
                        if (doContinueRun == false)
                            GR.RunRunner();
                        else
                            if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)//we continue only Stopped Runners
                            {
                                GR.ResetRunnerExecutionDetails(doNotResetBusFlows: true);//reset stopped runners only and not their BF's
                                GR.ContinueRun(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
                            }
                    }, TaskCreationOptions.LongRunning);
                    runnersTasks.Add(t);
                    t.Start();

                    // Wait one second before starting another runner
                    Thread.Sleep(1000);
                }
            }
            else
            {
                //running sequentially 
                Task t = new Task(() =>
                {
                    foreach (GingerRunner GR in Runners)
                    {
                        if (mStopRun) return;

                        if (doContinueRun == false)
                            GR.RunRunner();
                        else
                            if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)//we continue only Stopped Runners
                            {
                                GR.ResetRunnerExecutionDetails(doNotResetBusFlows: true);//reset stopped runners only and not their BF's
                                GR.ContinueRun(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
                            }
                            else if(GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)//continue the runners flow
                            {
                                GR.RunRunner();
                            }
                        // Wait one second before starting another runner
                        Thread.Sleep(1000);
                    }
                }, TaskCreationOptions.LongRunning);
                runnersTasks.Add(t);
                t.Start();
            }

            //Wait till end of run when all tasks are completed
            int count = 0;
            while (count < runnersTasks.Count) //removing dependency on stop because all Runners needs to stop first before exit
            {
                Thread.Sleep(100);
                count = (from x in runnersTasks where x.IsCompleted select x).Count();
            }
            mStopwatch.Stop();

            //Do post execution items
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## {0} Runners Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            ExecutionLogger.RunSetEnd();
            if (mStopRun == false)
            {
                // Process all post execution RunSet Operations
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## Running Post-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //Dispatcher.CurrentDispatcher.Invoke(() => //ToDO: Remove dependency on UI thread- it should run in backend
                //{
                    WorkSpace.RunsetExecutor.ProcessRunSetActions(new List<RunSetActionBase.eRunAt> { RunSetActionBase.eRunAt.ExecutionEnd });
                //});
            }
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## Doing {0} Execution Cleanup", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            CreateGingerExecutionReportAutomaticly();
            CloseAllEnvironments();
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
        }

        public void CreateGingerExecutionReportAutomaticly()
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            _selectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if ((_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled) && (Runners != null) && (Runners.Count > 0))
            {
                if (_selectedExecutionLoggerConfiguration.ExecutionLoggerHTMLReportsAutomaticProdIsEnabled)
                {
                    string runSetReportName;
                    if ((RunSetConfig.Name != null) && (RunSetConfig.Name != string.Empty))
                    {
                        runSetReportName = RunSetConfig.Name;
                    }
                    else
                    {
                        runSetReportName = ExecutionLogger.defaultRunTabLogName;
                    }
                    string exec_folder = ExecutionLogger.GetLoggerDirectory(_selectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder + "\\" + runSetReportName + "_" + Runners[0].ExecutionLogger.CurrentExecutionDateTime.ToString("MMddyyyy_HHmmss"));
                    //string reportsResultFolder = GingerExecutionReport.ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(exec_folder), false,null, null, false,currentConf.HTMLReportConfigurationMaximalFolderSize);
                }
            }
        }

        public void ResetRunnersExecutionDetails()
        {
            foreach (GingerRunner runner in Runners)
            {
                runner.ResetRunnerExecutionDetails();                
                runner.CloseAgents();
            }
        }

        public void StopRun()
        {
            mStopRun = true;
            foreach (GingerRunner runner in Runners)
            {
                if (runner.IsRunning)
                    runner.StopRun();
            }
        }
        

        internal void ProcessRunSetActions(List<RunSetActionBase.eRunAt> runAtList)
        {
            //Not closing Ginger Helper and not executing all actions
            ReportInfo RI = new ReportInfo(RunsetExecutionEnvironment, this);

            foreach (RunSetActionBase RSA in RunSetConfig.RunSetActions)
            {
                if (RSA.Active == true && runAtList.Contains(RSA.RunAt))
                {
                    switch (RSA.RunAt)
                    {
                        case RunSetActionBase.eRunAt.DuringExecution:
                            //if(RSA is RunSetActions.RunSetActionPublishToQC)
                            //    RSA.PrepareDuringExecAction(Runners);
                            break;

                        case RunSetActionBase.eRunAt.ExecutionStart:
                        case RunSetActionBase.eRunAt.ExecutionEnd:
                            bool b = CheckRSACondition(RI, RSA);
                            if (b)
                            {
                                RSA.RunAction(RI);
                            }
                            break;
                    }
                }
            }
        }

        private bool CheckRSACondition(ReportInfo RI, RunSetActionBase RSA)
        {
            //TODO: write UT to validate this function

            RSA.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            switch (RSA.Condition)
            {
                case RunSetActionBase.eRunSetActionCondition.AlwaysRun:
                    return true;
                case RunSetActionBase.eRunSetActionCondition.AllBusinessFlowsPassed:
                    if (RI.TotalBusinessFlowsPassed == RI.TotalBusinessFlows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                case RunSetActionBase.eRunSetActionCondition.OneOrMoreBusinessFlowsFailed:
                    if (RI.TotalBusinessFlowsFailed > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                default:
                    return false;
            }
        }


        public async Task<int> RunRunSetFromCommandLine()
        {
            //0- success
            //1- failure

            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Processing Command Line Arguments");
                if (ProcessCommandLineArgs() == false)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Processing Command Line Arguments failed");
                    return 1;
                }

                AutoLogProxy.UserOperationStart("AutoRunWindow", WorkSpace.RunsetExecutor.RunSetConfig.Name, WorkSpace.RunsetExecutor.RunsetExecutionEnvironment.Name);
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting {0} Automatic Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));

                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Loading {0} execution UI elements", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                try
                {
                    RepositoryItemHelper.RepositoryItemFactory.RunRunSetFromCommandLine();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed loading {0} execution UI elements, aborting execution.", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                    return 1;
                }

                //Running Runset Analyzer to look for issues
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running {0} Analyzer", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                try
                {
                    //run analyzer
                    int analyzeRes = await WorkSpace.RunsetExecutor.RunRunsetAnalyzerBeforeRun(true).ConfigureAwait(false);
                    if (analyzeRes == 1)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("{0} Analyzer found critical issues with the {0} configurations, aborting execution.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                        return 1;//cancel run because issues found
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed Running {0} Analyzer, still continue execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                    //return 1;
                }

                //Execute
                try
                {
                    await RunRunsetAsync();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Error occured during the {0} execution.", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                    return 1;
                }

                if (WorkSpace.RunSetExecutionStatus == eRunStatus.Passed)//TODO: improve
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Un expected error occured during the execution", ex);
                return 1;
            }
            finally
            {
                AutoLogProxy.UserOperationEnd();
            }
        }
      
        private bool ProcessCommandLineArgs()
        {
            // New option with one arg to config file
            // resole spaces and quotes mess in commnd arg + more user friednly to edit
            // We expect only AutoRun --> File location
            try
            {
                string[] Args = Environment.GetCommandLineArgs();

                // We expect Autorun as arg[1]
                string[] arg1 = Args[1].Split('=');

                if (arg1[0] != "ConfigFile")
                {
                    Reporter.ToLog(eLogLevel.ERROR, "'ConfigFile' argument was not found.");
                    return false;
                }

                string AutoRunFileName = arg1[1];

                Reporter.ToLog(eLogLevel.DEBUG, "Reading all arguments from the Config file placed at: '" + AutoRunFileName + "'");
                string[] lines = System.IO.File.ReadAllLines(AutoRunFileName);

                RepositoryItemHelper.RepositoryItemFactory.ProcessCommandLineArgs(lines);

                

                if (RunSetConfig != null && RunsetExecutionEnvironment != null)
                {
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Missing key arguments which required for execution");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred during command line arguments processing", ex);
                return false;
            }
        }

        public async Task<int> RunRunsetAnalyzerBeforeRun(bool runInSilentMode=false)
        {
            int x= 0;
            if (mRunSetConfig.RunWithAnalyzer)
            {
                //check if not including any High or Critical issues before execution
                Reporter.ToStatus(eStatusMsgKey.AnalyzerIsAnalyzing, null, mRunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet));
              
                     x = await RepositoryItemHelper.RepositoryItemFactory.AnalyzeRunset(mRunSetConfig, runInSilentMode);
                
            }
            return x;
        }
    }
}

