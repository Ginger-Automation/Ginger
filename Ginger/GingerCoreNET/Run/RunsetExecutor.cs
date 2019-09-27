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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run.ExecutionSummary;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

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

        public eRunStatus RunSetExecutionStatus = eRunStatus.Failed;

        private Stopwatch mStopwatch = new Stopwatch();
        public TimeSpan Elapsed { get { return mStopwatch.Elapsed; } }
        private ExecutionLoggerConfiguration mSelectedExecutionLoggerConfiguration = new ExecutionLoggerConfiguration();
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
        public List<LiteDbRunner> liteDbRunnerList = new List<LiteDbRunner>();
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
                {
                    // TODO: remove from here, do it were we change the env
                    if (WorkSpace.Instance.UserProfile != null)
                    {
                        WorkSpace.Instance.UserProfile.RecentEnvironment = mRunsetExecutionEnvironment.Guid;
                    }
                }
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
            runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            runner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            runner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            runner.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            //runner.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList;
        }

        public void InitRunners()
        {
            foreach (GingerRunner gingerRunner in Runners)
            {
                InitRunner(gingerRunner);
            }
        }

        public void InitRunner(GingerRunner runner)
        {
            //Configure Runner for execution
            runner.Status = eRunStatus.Pending;
            ConfigureRunnerForExecution(runner);

            //Set the Apps agents
            foreach (ApplicationAgent appagent in runner.ApplicationAgents.ToList())
            {
                if (appagent.AgentName != null)
                {
                    ObservableList<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                    appagent.Agent = (from a in agents where a.Name == appagent.AgentName select a).FirstOrDefault();
                }
            }

            //Load the biz flows     
            ObservableList<BusinessFlow> runnerFlows = new ObservableList<BusinessFlow>();
            foreach (BusinessFlowRun businessFlowRun in runner.BusinessFlowsRunList.ToList())
            {
                ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
                BusinessFlow businessFlow = (from x in businessFlows where x.Guid == businessFlowRun.BusinessFlowGuid select x).FirstOrDefault();
                //Fail over to try and find by name
                if (businessFlow == null)
                {
                    businessFlow = (from x in businessFlows where x.Name == businessFlowRun.BusinessFlowName select x).FirstOrDefault();
                }
                if (businessFlow == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Can not find the '{0}' {1} for the '{2}' {3}", businessFlowRun.BusinessFlowName, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), mRunSetConfig.Name, GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    continue;
                }
                else
                {
                    // Very slow
                    BusinessFlow BFCopy = (BusinessFlow)businessFlow.CreateCopy(false);
                    BFCopy.ContainingFolder = businessFlow.ContainingFolder;
                    BFCopy.Reset();
                    BFCopy.Active = businessFlowRun.BusinessFlowIsActive;
                    BFCopy.Mandatory = businessFlowRun.BusinessFlowIsMandatory;
                    if (businessFlowRun.BusinessFlowInstanceGuid == Guid.Empty)
                    {
                        BFCopy.InstanceGuid = Guid.NewGuid();
                    }
                    else
                    {
                        BFCopy.InstanceGuid = businessFlowRun.BusinessFlowInstanceGuid;
                    }
                    if (businessFlowRun.BusinessFlowCustomizedRunVariables != null && businessFlowRun.BusinessFlowCustomizedRunVariables.Count > 0)
                    {
                        foreach (VariableBase varb in BFCopy.GetBFandActivitiesVariabeles(true))
                        {
                            VariableBase runVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentGuid == varb.ParentGuid && v.ParentName == varb.ParentName && v.Name == varb.Name).FirstOrDefault();
                            if (runVar == null)//for supporting dynamic run set XML in which we do not have GUID
                            {
                                runVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentName == varb.ParentName && v.Name == varb.Name).FirstOrDefault();
                                if (runVar == null)
                                {
                                    runVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.Name == varb.Name).FirstOrDefault();
                                }
                            }
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
                    BFCopy.RunDescription = businessFlowRun.BusinessFlowRunDescription;
                    BFCopy.BFFlowControls = businessFlowRun.BFFlowControls;
                    runnerFlows.Add(BFCopy);
                }
            }

            runner.BusinessFlows = runnerFlows;
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
            mSelectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;

            if (mSelectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                DateTime currentExecutionDateTime = DateTime.Now;
                Runners[0].ExecutionLoggerManager.RunSetStart(mSelectedExecutionLoggerConfiguration.CalculatedLoggerFolder, mSelectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, currentExecutionDateTime);

                int gingerIndex = 0;
                while (Runners.Count > gingerIndex)
                {
                    Runners[gingerIndex].ExecutionLoggerManager.GingerData.Seq = gingerIndex + 1;
                    Runners[gingerIndex].ExecutionLoggerManager.GingerData.GingerName = Runners[gingerIndex].Name;
                    Runners[gingerIndex].ExecutionLoggerManager.GingerData.Ginger_GUID = Runners[gingerIndex].Guid;
                    Runners[gingerIndex].ExecutionLoggerManager.GingerData.GingerAggentMapping = Runners[gingerIndex].ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                    Runners[gingerIndex].ExecutionLoggerManager.GingerData.GingerEnv = Runners[gingerIndex].ProjEnvironment.Name.ToString();
                    Runners[gingerIndex].ExecutionLoggerManager.CurrentExecutionDateTime = currentExecutionDateTime;
                    Runners[gingerIndex].ExecutionLoggerManager.Configuration = mSelectedExecutionLoggerConfiguration;
                    //gingerIndex++;
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.GingerData.Seq = gingerIndex + 1;
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.GingerData.GingerName = Runners[gingerIndex].Name;
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.GingerData.Ginger_GUID = Runners[gingerIndex].Guid;
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.GingerData.GingerAggentMapping = Runners[gingerIndex].ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.GingerData.GingerEnv = Runners[gingerIndex].ProjEnvironment.Name.ToString();
                    Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.CurrentExecutionDateTime = currentExecutionDateTime;
                    //Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.Configuration = mSelectedExecutionLoggerConfiguration;
                    gingerIndex++;

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

        public void RunRunset(bool doContinueRun = false)
        {
            try
            {
                mRunSetConfig.IsRunning = true;

                //reset run       
                if (doContinueRun == false)
                {
                    RunSetConfig.LastRunsetLoggerFolder = "-1";   // !!!!!!!!!!!!!!!!!!
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
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("Configuring {0} elements for execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                ConfigureAllRunnersForExecution();

                //Process all pre execution Run Set Operations
                if (doContinueRun == false)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running Pre-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    WorkSpace.Instance.RunsetExecutor.ProcessRunSetActions(new List<RunSetActionBase.eRunAt> { RunSetActionBase.eRunAt.ExecutionStart, RunSetActionBase.eRunAt.DuringExecution });
                }

                //Start Run 
                if (doContinueRun == false)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Started: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
                    SetRunnersExecutionLoggerConfigs();//contains ExecutionLogger.RunSetStart()
                }
                else
                {
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Continuation: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
                }

                mStopwatch.Start();
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## {0} Runners Execution Started", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                List<Task> runnersTasks = new List<Task>();
                if (RunSetConfig.RunModeParallel)
                {
                    foreach (GingerRunner GR in Runners)
                    {
                        if (mStopRun)
                        {
                            return;
                        }

                        Task t = new Task(() =>
                        {
                            if (doContinueRun == false)
                            {
                                GR.RunRunner();
                            }
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
                            if (mStopRun)
                            {
                                return;
                            }

                            if (doContinueRun == false)
                            {
                                GR.RunRunner();
                            }
                            else
                                if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)//we continue only Stopped Runners
                            {
                                GR.ResetRunnerExecutionDetails(doNotResetBusFlows: true);//reset stopped runners only and not their BF's
                                GR.ContinueRun(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
                            }
                            else if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)//continue the runners flow
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

                Task.WaitAll(runnersTasks.ToArray());
                mStopwatch.Stop();

                //Do post execution items
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## {0} Runners Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //ExecutionLoggerManager.RunSetEnd();
                Runners[0].ExecutionLoggerManager.RunSetEnd();
                if (mStopRun == false)
                {
                    // Process all post execution RunSet Operations
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## Running Post-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    WorkSpace.Instance.RunsetExecutor.ProcessRunSetActions(new List<RunSetActionBase.eRunAt> { RunSetActionBase.eRunAt.ExecutionEnd });
                }
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## Creating {0} Execution Report", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                CreateGingerExecutionReportAutomaticly();
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("######## Doing {0} Execution Cleanup", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                CloseAllEnvironments();
                Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## {0} Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            }
            finally
            {
                mRunSetConfig.IsRunning = false;
            }
        }
        public void CreateGingerExecutionReportAutomaticly()
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            mSelectedExecutionLoggerConfiguration = WorkSpace.Instance.Solution.LoggerConfigurations;
            if ((mSelectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled) && (Runners != null) && (Runners.Count > 0))
            {
                if (mSelectedExecutionLoggerConfiguration.ExecutionLoggerHTMLReportsAutomaticProdIsEnabled)
                {
                    string runSetReportName;
                    if (!string.IsNullOrEmpty(RunSetConfig.Name))
                    {
                        runSetReportName = RunSetConfig.Name;
                    }
                    else
                    {
                        runSetReportName = ExecutionLoggerManager.defaultRunTabLogName;
                    }
                    string exec_folder = new ExecutionLoggerHelper().GetLoggerDirectory(Path.Combine(mSelectedExecutionLoggerConfiguration.CalculatedLoggerFolder,runSetReportName + "_" + Runners[0].ExecutionLoggerManager.CurrentExecutionDateTime.ToString("MMddyyyy_HHmmss")));                    
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
                {
                    runner.StopRun();
                }
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
                            if (RSA is RunSetActions.RunSetActionPublishToQC)
                                RSA.PrepareDuringExecAction(Runners);
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


        /// <summary>
        /// Create a summary json of the execution 
        /// Example:
        /// {
        /// "StartTime": "0001-01-01T00:00:00",
        /// "EndTime": "0001-01-01T00:00:00",
        /// "Elapsed": "00:00:00",
        /// "Runners": {
        /// "Total": 1,
        ///"Parallel": false
        ///},
        ///"BusinessFlowsSummary": {
        ///"Total": 1,
        ///"Pass": 1,
        ///"Fail": 0,
        ///"Blocked": 0
        ///},
        ///"ActivitiesSummary": {
        ///"Total": 1,
        ///"Pass": 1,
        ///"Fail": 0,
        ///"Blocked": 0
        ///},
        ///"ActionsSummary": {
        ///"Total": 1,
        ///"Pass": 1,
        ///"Fail": 0,
        ///"Blocked": 0
        ///}
        ///}
        /// </summary>
        /// <returns>json string</returns>
        public string CreateSummary()
        {
            ExecutionSummary executionSummary = new ExecutionSummary();
            string json = executionSummary.Create(this);
            return json;
        }




    }
}

