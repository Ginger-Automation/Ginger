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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.ExecutionSummary;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Ginger.Configurations;
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;

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
        private ExecutionLoggerConfiguration mSelectedExecutionLoggerConfiguration
        {
            get
            {
                return WorkSpace.Instance.Solution.LoggerConfigurations;
            }
        }
        public bool mStopRun;

        ObservableList<DefectSuggestion> mDefectSuggestionsList = [];
        private List<BusinessFlowRun> AllPreviousBusinessFlowRuns = [];
        RunSetConfig mRunSetConfig = null;
        internal List<Task> ALMResultsPublishTaskPool;
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
        public List<LiteDbRunner> liteDbRunnerList = [];
        private ProjEnvironment mRunsetExecutionEnvironment = null;

        private List<BusinessFlow> deactivatedBF = [];



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
                ConfigureRunnerForExecution((GingerExecutionEngine)runner.Executor);
            }
        }

        public ObservableList<DefectSuggestion> DefectSuggestionsList
        {
            get { return mDefectSuggestionsList; }
            set { mDefectSuggestionsList = value; }
        }

        public void ConfigureRunnerForExecution(GingerExecutionEngine runner)
        {
            runner.SetExecutionEnvironment(RunsetExecutionEnvironment, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>());
            runner.CurrentSolution = WorkSpace.Instance.Solution;
            runner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            runner.GingerRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            runner.SolutionApplications = WorkSpace.Instance.Solution.ApplicationPlatforms;
            runner.SolutionFolder = WorkSpace.Instance.Solution.Folder;
            //runner.ExecutionLoggerManager.Configuration = WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList;
        }

        public void InitRunners()
        {

            AllPreviousBusinessFlowRuns.Clear();
            foreach (GingerRunner gingerRunner in Runners)
            {
                GingerExecutionEngine ExecutorEngine = new GingerExecutionEngine(gingerRunner);
                InitRunner(gingerRunner, ExecutorEngine);
            }
        }

        public void InitRunner(GingerRunner runner, GingerExecutionEngine ExecutorEngine)
        {
            //Configure Runner for execution
            runner.Status = eRunStatus.Pending;
            if (runner.Executor is not null and GingerExecutionEngine previousExectionEngine)
            {
                previousExectionEngine.ClearBindings();
            }
            runner.Executor = ExecutorEngine;
            ConfigureRunnerForExecution((GingerExecutionEngine)runner.Executor);

            //Set the Apps agents
            foreach (ApplicationAgent appagent in runner.ApplicationAgents)
            {
                if (appagent.AgentName != null)
                {
                    ObservableList<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                    appagent.Agent = agents.FirstOrDefault(a => a.Name == appagent.AgentName);
                }
            }

            //Load the biz flows     
            ObservableList<BusinessFlow> runnerFlows = [];
            foreach (BusinessFlowRun businessFlowRun in runner.BusinessFlowsRunList)
            {
                ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                BusinessFlow businessFlow = businessFlows.FirstOrDefault(x => x.Guid == businessFlowRun.BusinessFlowGuid);
                //Fail over to try and find by name
                if (businessFlow == null)
                {
                    businessFlow = businessFlows.FirstOrDefault(x => x.Name == businessFlowRun.BusinessFlowName);
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
                        ObservableList<VariableBase> allBfVars = BFCopy.GetBFandActivitiesVariabeles(true);
                        Parallel.ForEach(businessFlowRun.BusinessFlowCustomizedRunVariables, customizedVar =>
                        {
                            //This is needed to handle updating the outputvariable mappedoutvalues to new style
                            UpdateOldOutputVariableMappedValues(customizedVar);

                            VariableBase originalVar = allBfVars.FirstOrDefault(v => v.ParentGuid == customizedVar.ParentGuid && v.Guid == customizedVar.Guid);
                            if (originalVar == null)//for supporting dynamic run set XML in which we do not have GUID
                            {
                                originalVar = allBfVars.FirstOrDefault(v => v.ParentName == customizedVar.ParentName && v.Name == customizedVar.Name);
                                if (originalVar == null)
                                {
                                    originalVar = allBfVars.FirstOrDefault(v => v.Name == customizedVar.Name);
                                }
                            }
                            if (originalVar != null)
                            {
                                CopyCustomizedVariableConfigurations(customizedVar, originalVar);
                            }
                        });
                    }
                    AllPreviousBusinessFlowRuns.Add(businessFlowRun);
                    BFCopy.RunDescription = businessFlowRun.BusinessFlowRunDescription;
                    BFCopy.BFFlowControls = businessFlowRun.BFFlowControls;
                    runnerFlows.Add(BFCopy);
                }
            }

            runner.Executor.IsUpdateBusinessFlowRunList = true;
            runner.Executor.BusinessFlows = runnerFlows;
        }


        private void UpdateOldOutputVariableMappedValues(VariableBase var)
        {
            //For BusinessFlowCustomizedRunVariables the output variable mappedvalue was storing only variable GUID
            //But if there 2 variables with same name then users were not able to map it to the desired instance 
            //So mappedValue for output variable type mapping was enhanced to store the BusinessFlowInstanceGUID_VariabledGuid
            //Below code is for backward support for old runset with output variable mapping having only guid.

            try
            {
                if (var.MappedOutputType == VariableBase.eOutputType.OutputVariable && !var.MappedOutputValue.Contains('_'))
                {
                    for (int i = AllPreviousBusinessFlowRuns.Count - 1; i >= 0; i--)//doing in reverse for sorting by latest value in case having the same var more than once
                    {
                        Guid guid = AllPreviousBusinessFlowRuns[i].BusinessFlowGuid;
                        BusinessFlow bf = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(guid);

                        if (bf.GetBFandActivitiesVariabeles(false, false, true).Any(x => x.Guid.ToString().Equals(var.MappedOutputValue)))
                        {
                            var.MappedOutputValue = AllPreviousBusinessFlowRuns[i].BusinessFlowInstanceGuid + "_" + var.MappedOutputValue;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Exception occurred when trying to update outputvariable mapped value ", ex);
            }


        }

        private void CopyCustomizedVariableConfigurations(VariableBase customizedVar, VariableBase originalVar)
        {
            //keep original description values
            VariableBase originalCopy = (VariableBase)originalVar.CreateCopy(false);

            //ovveride original variable configurations with user customizations
            string varType = customizedVar.GetType().Name;
            bool skipType = false;
            if (varType == "VariableSelectionList")
            {
                skipType = true;
            }

            AutomapData.AutoMapVariableData(customizedVar, ref originalVar, skipType);

            originalVar.DiffrentFromOrigin = customizedVar.DiffrentFromOrigin;
            originalVar.MappedOutputVariable = customizedVar.MappedOutputVariable;


            //Fix for Empty variable are not being saved in Run Configuration (when variable has value in BusinessFlow but is changed to empty in RunSet)
            if (customizedVar.DiffrentFromOrigin && string.IsNullOrEmpty(customizedVar.MappedOutputVariable))
            {
                originalVar.Value = customizedVar.Value;
            }

            //Restore original description values
            originalVar.Name = originalCopy.Name;
            originalVar.Description = originalCopy.Description;
            originalVar.Tags = originalCopy.Tags;
            originalVar.SetAsInputValue = originalCopy.SetAsInputValue;
            originalVar.SetAsOutputValue = originalCopy.SetAsOutputValue;
            originalVar.LinkedVariableName = originalCopy.LinkedVariableName;
            originalVar.Publish = originalCopy.Publish;
        }

        public ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false)
        {
            ObservableList<BusinessFlowExecutionSummary> BFESs = [];
            foreach (GingerRunner ARC in Runners)
            {
                BFESs.Append(ARC.Executor.GetAllBusinessFlowsExecutionSummary(GetSummaryOnlyForExecutedFlow, ARC.Name));
            }
            return BFESs;
        }

        private void CloseAllAgents()
        {
            foreach (GingerRunner runner in Runners)
            {
                if (runner.Executor != null)
                {
                    runner.Executor.CloseAgents();
                }
            }
        }

        internal void CloseAllEnvironments()
        {
            foreach (GingerRunner gr in Runners)
            {
                if (gr.UseSpecificEnvironment)
                {
                    if (gr.ProjEnvironment != null && gr.ProjEnvironment.Applications != null)
                    {
                        foreach (EnvApplication ea in gr.ProjEnvironment.Applications)
                        {
                            if (ea.Dbs != null)
                            {
                                foreach (Database db in ea.Dbs)
                                {
                                    if (db.DatabaseOperations == null)
                                    {
                                        db.DatabaseOperations = new DatabaseOperations(db);
                                    }
                                }
                            }
                        }
                        gr.ProjEnvironment.CloseEnvironment();
                    }
                }
            }

            if (RunsetExecutionEnvironment != null)
            {
                foreach (EnvApplication ea in RunsetExecutionEnvironment.Applications)
                {
                    foreach (Database db in ea.Dbs)
                    {
                        if (db.DatabaseOperations == null)
                        {
                            DatabaseOperations databaseOperations = new DatabaseOperations(db);
                            db.DatabaseOperations = databaseOperations;
                        }
                    }
                }
                RunsetExecutionEnvironment.CloseEnvironment();
            }
        }


        public void SetRunnersEnv(ProjEnvironment defualtEnv, ObservableList<ProjEnvironment> allEnvs)
        {
            foreach (GingerRunner GR in Runners)
            {
                GR.Executor.SetExecutionEnvironment(defualtEnv, allEnvs);
            }
        }


        public void SetRunnersExecutionLoggerConfigs()
        {
            if (mSelectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled)
            {
                DateTime currentExecutionDateTime = DateTime.Now;
                Runners[0].Executor.ExecutionLoggerManager.RunSetStart(mSelectedExecutionLoggerConfiguration.CalculatedLoggerFolder, mSelectedExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize, currentExecutionDateTime);

                int gingerIndex = 0;
                while (Runners.Count > gingerIndex)
                {
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.GingerData.Seq = gingerIndex + 1;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.GingerData.GingerName = Runners[gingerIndex].Name;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.GingerData.Ginger_GUID = Runners[gingerIndex].Guid;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.GingerData.GingerAggentMapping = Runners[gingerIndex].ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.GingerData.GingerEnv = Runners[gingerIndex].ProjEnvironment.Name.ToString();
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.CurrentExecutionDateTime = currentExecutionDateTime;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.Configuration = mSelectedExecutionLoggerConfiguration;
                    //gingerIndex++;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.GingerData.Seq = gingerIndex + 1;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.GingerData.GingerName = Runners[gingerIndex].Name;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.GingerData.Ginger_GUID = Runners[gingerIndex].Guid;
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.GingerData.GingerAggentMapping = Runners[gingerIndex].ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.GingerData.GingerEnv = Runners[gingerIndex].ProjEnvironment.Name.ToString();
                    Runners[gingerIndex].Executor.ExecutionLoggerManager.mExecutionLogger.CurrentExecutionDateTime = currentExecutionDateTime;
                    //Runners[gingerIndex].ExecutionLoggerManager.mExecutionLogger.Configuration = mSelectedExecutionLoggerConfiguration;
                    gingerIndex++;

                }
            }
        }

        public async Task<int> RunRunsetAsync(bool doContinueRun = false)
        {
            var result = await Task.Run(async () =>
            {
                RunSetConfig.SourceApplication = "Ginger Desktop";
                RunSetConfig.SourceApplicationUser = System.Environment.UserName;
                await RunRunset(doContinueRun);
                return 1;
            });
            return result;
        }

        public async Task RunRunset(bool doContinueRun = false)
        {
            try
            {
                mRunSetConfig.IsRunning = true;
                //reset run       
                if (doContinueRun == false)
                {
                    if (WorkSpace.Instance.RunningInExecutionMode == false || RunSetConfig.ExecutionID == null)
                    {
                        RunSetConfig.ExecutionID = Guid.NewGuid();

                    }
                    else
                    {
                        if (mSelectedExecutionLoggerConfiguration.PublishLogToCentralDB == ExecutionLoggerConfiguration.ePublishToCentralDB.Yes && !string.IsNullOrEmpty(GingerPlayEndPointManager.GetAccountReportServiceUrl()))
                        {
                            AccountReportApiHandler accountReportApiHandler = new AccountReportApiHandler(GingerPlayEndPointManager.GetAccountReportServiceUrl());
                            bool isValidated = accountReportApiHandler.ExecutionIdValidation((Guid)RunSetConfig.ExecutionID);
                            if (!isValidated)
                            {
                                RunSetConfig.ExecutionID = Guid.NewGuid();
                                Reporter.ToLog(eLogLevel.WARN, string.Format("Duplicate execution id used, creating new execution id : {0}", RunSetConfig.ExecutionID));
                            }
                        }
                    }
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Ginger Execution Id: {0}", RunSetConfig.ExecutionID));
                    Reporter.ToLog(eLogLevel.INFO, string.Format("{0} Source Application: '{1}' And Source Application User: '{2}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.SourceApplication, RunSetConfig.SourceApplicationUser));

                    RunSetConfig.LastRunsetLoggerFolder = "-1";   // !!!!!!!!!!!!!!!!!!
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Reseting {0} elements", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    mStopwatch.Reset();
                    if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.ReferenceExecutionID != null)
                    {
                        ResetRunnersExecutionDetails();
                    }
                }
                else
                {
                    RunSetConfig.LastRunsetLoggerFolder = null;
                }

                mStopRun = false;

                //configure Runners for run
                Reporter.ToLog(eLogLevel.INFO, string.Format("Configuring {0} elements for execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                ConfigureAllRunnersForExecution();

                //Process all pre execution Run Set Operations
                if (doContinueRun == false)
                {
                    RunSetConfig.StartTimeStamp = DateTime.UtcNow;
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Running Pre-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    WorkSpace.Instance.RunsetExecutor.ProcessRunSetActions([RunSetActionBase.eRunAt.ExecutionStart, RunSetActionBase.eRunAt.DuringExecution]);
                }

                if (mSelectedExecutionLoggerConfiguration != null && mSelectedExecutionLoggerConfiguration.PublishLogToCentralDB == ePublishToCentralDB.Yes && Runners.Count > 0)
                {
                    if (((GingerExecutionEngine)Runners[0].Executor).Centeralized_Logger != null)
                    {
                        await ((GingerExecutionEngine)Runners[0].Executor).Centeralized_Logger.RunSetStart(RunSetConfig);
                    }
                }

                if (mSelectedExecutionLoggerConfiguration != null && WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == Configurations.SealightsConfiguration.eSealightsLog.Yes && Runners.Count > 0)
                {
                    string[] testsToExclude = ((GingerExecutionEngine)Runners[0].Executor).Sealights_Logger.RunSetStart(RunSetConfig);
                    if (testsToExclude != null && testsToExclude.Length > 0)
                    {
                        DisableTestsExecution(testsToExclude, RunSetConfig);
                    }
                }

                //Start Run 
                if (doContinueRun == false)
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("########################## {0} Execution Started: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
                    SetRunnersExecutionLoggerConfigs();//contains ExecutionLogger.RunSetStart()
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, string.Format("########################## {0} Execution Continuation: '{1}'", GingerDicser.GetTermResValue(eTermResKey.RunSet), RunSetConfig.Name));
                }

                mStopwatch.Start();
                Reporter.ToLog(eLogLevel.INFO, string.Format("######## {0} Runners Execution Started", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                List<Task> runnersTasks = [];
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
                            if (!doContinueRun)
                            {
                                GR.Executor.RunRunner();
                            }
                            else if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)//we continue only Stopped Runners
                            {
                                GR.Executor.ResetRunnerExecutionDetails(doNotResetBusFlows: true);//reset stopped runners only and not their BF's
                                GR.Executor.ContinueRun(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
                            }
                        }, TaskCreationOptions.LongRunning);
                        runnersTasks.Add(t);
                        t.Start();
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
                                GR.Executor.RunRunner();
                            }
                            else
                            {
                                //continue
                                if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)//we continue only Stopped Runners
                                {
                                    GR.Executor.ResetRunnerExecutionDetails(doNotResetBusFlows: true);//reset stopped runners only and not their BF's
                                    GR.Executor.ContinueRun(eContinueLevel.Runner, eContinueFrom.LastStoppedAction);
                                }
                                else if (GR.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)//continue the runners flow
                                {
                                    GR.Executor.RunRunner();
                                }
                            }

                            if (GR.Status == eRunStatus.Failed && mRunSetConfig.StopRunnersOnFailure)
                            {
                                //marking next Runners as blocked and stopping execution
                                for (int indx = Runners.IndexOf(GR) + 1; indx < Runners.Count; indx++)
                                {
                                    Runners[indx].Executor.SetNextBusinessFlowsBlockedStatus();
                                    Runners[indx].Status = eRunStatus.Blocked;
                                    Runners[indx].Executor.GiveUserFeedback();//for getting update for runner stats on runset page
                                }

                                break;
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
                RunSetConfig.Elapsed = mStopwatch.ElapsedMilliseconds;
                RunSetConfig.EndTimeStamp = DateTime.UtcNow;
                //Do post execution items
                Reporter.ToLog(eLogLevel.INFO, string.Format("######## {0} Runners Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //ExecutionLoggerManager.RunSetEnd();
                Runners[0].Executor.ExecutionLoggerManager.RunSetEnd();

                bool isReportStoredToRemote = false;
                if (mSelectedExecutionLoggerConfiguration != null && mSelectedExecutionLoggerConfiguration.PublishLogToCentralDB == ePublishToCentralDB.Yes && Runners.Count > 0)
                {
                    if (((GingerExecutionEngine)Runners[0].Executor).Centeralized_Logger != null)
                    {
                        isReportStoredToRemote = await ((GingerExecutionEngine)Runners[0].Executor).Centeralized_Logger.RunSetEnd(RunSetConfig, Runners[0].Executor.ExecutionLoggerManager);

                    }
                }
                if (mSelectedExecutionLoggerConfiguration != null && WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == Configurations.SealightsConfiguration.eSealightsLog.Yes && Runners.Count > 0)
                {
                    if (deactivatedBF != null && deactivatedBF.Count > 0)
                    {
                        ReactivateBF(deactivatedBF);
                        deactivatedBF.Clear();
                    }
                    await ((GingerExecutionEngine)Runners[0].Executor).Sealights_Logger.RunSetEnd(RunSetConfig);
                }

                FinishPublishResultsToAlmTask();

                if (mStopRun == false)
                {
                    // Process all post execution RunSet Operations
                    Reporter.ToLog(eLogLevel.INFO, string.Format("######## Running Post-Execution {0} Operations", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    WorkSpace.Instance.RunsetExecutor.ProcessRunSetActions([RunSetActionBase.eRunAt.ExecutionEnd]);
                }
                if (isReportStoredToRemote && Runners[0].Executor.ExecutionLoggerManager.Configuration.DeleteLocalDataOnPublish.Equals(eDeleteLocalDataOnPublish.Yes))
                {
                    Runners[0].Executor.ExecutionLoggerManager.mExecutionLogger.DeleteLocalData(RunSetConfig.LastRunsetLoggerFolder, RunSetConfig.LiteDbId, RunSetConfig.ExecutionID ?? Guid.Empty);
                }
                Reporter.ToLog(eLogLevel.INFO, string.Format("######## Creating {0} Execution Report", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                CreateGingerExecutionReportAutomaticly();
                Reporter.ToLog(eLogLevel.INFO, string.Format("######## Doing {0} Execution Cleanup", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                //CloseAllAgents();
                CloseAllEnvironments();
                Reporter.ToLog(eLogLevel.INFO, string.Format("########################## {0} Execution Ended", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when trying to Run runset ", ex);
            }
            finally
            {
                mRunSetConfig.IsRunning = false;

            }
        }


        private void FinishPublishResultsToAlmTask()
        {
            if (ALMResultsPublishTaskPool != null && ALMResultsPublishTaskPool.Count > 0)
            {
                // Wait for all ALM publish tasks to complete
                Reporter.ToLog(eLogLevel.INFO, "######## Finishing Update Execution Results to ALM...");
                Task.WaitAll(ALMResultsPublishTaskPool.ToArray());
                ALMResultsPublishTaskPool.Clear();
                ALMResultsPublishTaskPool = null;
                var runsetAction = WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions.FirstOrDefault(f => f is RunSetActionPublishToQC && f.RunAt.Equals(RunSetActionBase.eRunAt.DuringExecution));
                if (runsetAction != null)
                {

                    if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions.Any(f => f is RunSetActionPublishToQC && f.RunAt.Equals(RunSetActionBase.eRunAt.DuringExecution) && !string.IsNullOrEmpty(f.Errors)))
                    {
                        runsetAction.Status = RunSetActionBase.eRunSetActionStatus.Failed;
                    }
                    else { runsetAction.Status = RunSetActionBase.eRunSetActionStatus.Completed; }

                }
            }
        }

        public void CreateGingerExecutionReportAutomaticly()
        {
            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.FirstOrDefault(x => (x.IsSelected == true));
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
                    string exec_folder = new ExecutionLoggerHelper().GetLoggerDirectory(Path.Combine(mSelectedExecutionLoggerConfiguration.CalculatedLoggerFolder, runSetReportName + "_" + Runners[0].Executor.ExecutionLoggerManager.CurrentExecutionDateTime.ToString("MMddyyyy_HHmmssfff")));
                }
            }
        }

        public void ResetRunnersExecutionDetails()
        {
            foreach (GingerRunner runner in Runners)
            {
                runner.Executor.ResetRunnerExecutionDetails();
                runner.Executor.CloseAgents();
            }
            ResetRunsetActions();
        }

        public void StopRun()
        {
            mStopRun = true;
            foreach (GingerRunner runner in Runners)
            {
                if (runner.Executor.IsRunning)
                {
                    runner.Executor.StopRun();
                }
            }
        }

        internal void ResetRunsetActions()
        {
            foreach (RunSetActionBase RSA in RunSetConfig.RunSetActions)
            {
                RSA.Errors = "";
                RSA.Status = RunSetActionBase.eRunSetActionStatus.Pending;
                RSA.Elapsed = 0;
            }
        }

        internal void ProcessRunSetActions(List<RunSetActionBase.eRunAt> runAtList)
        {
            //Not closing Ginger Helper and not executing all actions
            ReportInfo RI = new ReportInfo(RunsetExecutionEnvironment, this);

            foreach (RunSetActionBase RSA in RunSetConfig.RunSetActions)
            {
                //set Operations object to call execute, specific to runsetAtion 
                if (RSA is RunSetActionHTMLReport)
                {
                    RunSetActionHTMLReportOperations runSetActionHTMLReport = new RunSetActionHTMLReportOperations((RunSetActionHTMLReport)RSA);
                    ((RunSetActionHTMLReport)RSA).RunSetActionHTMLReportOperations = runSetActionHTMLReport;
                }
                if (RSA is RunSetActionAutomatedALMDefects)
                {
                    RunSetActionAutomatedALMDefectsOperations runSetActionAutomatedALMDefects = new RunSetActionAutomatedALMDefectsOperations((RunSetActionAutomatedALMDefects)RSA);
                    ((RunSetActionAutomatedALMDefects)RSA).RunSetActionAutomatedALMDefectsOperations = runSetActionAutomatedALMDefects;
                }
                if (RSA is RunSetActionGenerateTestNGReport)
                {
                    RunSetActionGenerateTestNGReportOperations runSetActionGenerateTestNGReport = new RunSetActionGenerateTestNGReportOperations((RunSetActionGenerateTestNGReport)RSA);
                    ((RunSetActionGenerateTestNGReport)RSA).RunSetActionGenerateTestNGReportOperations = runSetActionGenerateTestNGReport;
                }
                if (RSA is RunSetActionHTMLReportSendEmail)
                {
                    RunSetActionHTMLReportSendEmailOperations runSetActionHTMLReportSendEmail = new RunSetActionHTMLReportSendEmailOperations((RunSetActionHTMLReportSendEmail)RSA);
                    ((RunSetActionHTMLReportSendEmail)RSA).RunSetActionHTMLReportSendEmailOperations = runSetActionHTMLReportSendEmail;
                }
                if (RSA is RunSetActionPublishToQC)
                {
                    RunSetActionPublishToQCOperations runSetActionPublishToQC = new RunSetActionPublishToQCOperations((RunSetActionPublishToQC)RSA);
                    ((RunSetActionPublishToQC)RSA).RunSetActionPublishToQCOperations = runSetActionPublishToQC;
                }
                if (RSA is RunSetActionSaveResults)
                {
                    RunSetActionSaveResultsOperations runSetActionSaveResults = new RunSetActionSaveResultsOperations((RunSetActionSaveResults)RSA);
                    ((RunSetActionSaveResults)RSA).RunSetActionSaveResultsOperations = runSetActionSaveResults;
                }
                if (RSA is RunSetActionScript)
                {
                    RunSetActionScriptOperations runSetActionScript = new RunSetActionScriptOperations((RunSetActionScript)RSA);
                    ((RunSetActionScript)RSA).RunSetActionScriptOperations = runSetActionScript;
                }
                if (RSA is RunSetActionJSONSummary)
                {
                    RunSetActionJSONSummaryOperations runSetActionJSONSummary = new RunSetActionJSONSummaryOperations((RunSetActionJSONSummary)RSA);
                    ((RunSetActionJSONSummary)RSA).RunSetActionJSONSummaryOperations = runSetActionJSONSummary;
                }
                if (RSA is RunSetActionSendDataToExternalSource)
                {
                    RunSetActionSendDataToExternalSourceOperations runSetActionSendDataToExternalSource = new RunSetActionSendDataToExternalSourceOperations((RunSetActionSendDataToExternalSource)RSA);
                    ((RunSetActionSendDataToExternalSource)RSA).RunSetActionSendDataToExternalSourceOperations = runSetActionSendDataToExternalSource;
                }
                if (RSA is RunSetActionSendEmail)
                {
                    RunSetActionSendEmailOperations runSetActionSendEmail = new RunSetActionSendEmailOperations((RunSetActionSendEmail)RSA);
                    ((RunSetActionSendEmail)RSA).RunSetActionSendEmailOperations = runSetActionSendEmail;
                }
                if (RSA is RunSetActionSendFreeEmail)
                {
                    RunSetActionSendFreeEmailOperations runSetActionSendFreeEmail = new RunSetActionSendFreeEmailOperations((RunSetActionSendFreeEmail)RSA);
                    ((RunSetActionSendFreeEmail)RSA).RunSetActionSendFreeEmailOperations = runSetActionSendFreeEmail;
                }
                if (RSA is RunSetActionSendSMS)
                {
                    RunSetActionSendSMSOperations runSetActionSendSMS = new RunSetActionSendSMSOperations((RunSetActionSendSMS)RSA);
                    ((RunSetActionSendSMS)RSA).RunSetActionSendSMSOperations = runSetActionSendSMS;
                }


                RunSetActionBaseOperations runSetActionBaseOperations = new RunSetActionBaseOperations(RSA);
                RSA.runSetActionBaseOperations = runSetActionBaseOperations;

                if (RSA.Active == true && runAtList.Contains(RSA.RunAt))
                {
                    switch (RSA.RunAt)
                    {
                        case RunSetActionBase.eRunAt.DuringExecution:
                            if (RSA is RunSetActions.RunSetActionPublishToQC)
                            {
                                RSA.PrepareDuringExecAction(Runners);
                                ALMResultsPublishTaskPool = [];
                                RSA.Errors = "";
                            }

                            break;

                        case RunSetActionBase.eRunAt.ExecutionStart:
                        case RunSetActionBase.eRunAt.ExecutionEnd:
                            bool b = CheckRSACondition(RI, RSA);
                            if (b)
                            {
                                RSA.runSetActionBaseOperations.RunAction(RI);
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


        static volatile object locker = new Object();
        public static void ClearAndResetVirtualAgents(RunSetConfig runset, GingerExecutionEngine runner)
        {
            lock (locker)
            {
                if (runner.GingerRunner.KeepAgentsOn && !WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunModeParallel)
                {
                    return;
                }
                else
                {
                    var appAgents = runner.GingerRunner.ApplicationAgents.Where(x => x.Agent != null && ((Agent)x.Agent).IsVirtual).ToList();

                    if (appAgents.Count > 0)
                    {
                        for (var i = 0; i < appAgents.Count; i++)
                        {
                            var virtualAgent = (Agent)appAgents[i].Agent;

                            List<IAgent> agentslist = runset.ActiveAgentListWithRunner.Values.SelectMany(l => l).ToList();

                            var realAgent = agentslist != null ? agentslist.FirstOrDefault(x => ((Agent)x).Guid.Equals(virtualAgent.ParentGuid)) : null;

                            if (realAgent != null)
                            {
                                var VirtualAgentList = runset.ActiveAgentListWithRunner.Where(entry => entry.Key == runner.GingerRunner.Guid).Select(y => y.Value).ToList().Select(x => x.FirstOrDefault(k => ((Agent)k).Guid.Equals(virtualAgent.Guid)));

                                var runsetVirtualAgent = VirtualAgentList != null ? VirtualAgentList.FirstOrDefault(x => ((Agent)x).Guid.Equals(virtualAgent.Guid)) : null;
                                appAgents[i].Agent = realAgent;

                                if (runsetVirtualAgent != null)
                                {
                                    runset.ActiveAgentListWithRunner = runset.ActiveAgentListWithRunner
                                             .Where(kvp => kvp.Value.Any(x => !((Agent)x).Guid.Equals(((Agent)runsetVirtualAgent).Guid)))
                                             .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                                }

                            }
                        }
                    }
                }
            }
        }
        private void DisableTestsExecution(string[] testsToExclude, RunSetConfig runsetConfig)
        {
            switch (WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel)
            {
                case SealightsConfiguration.eSealightsEntityLevel.BusinessFlow:
                {
                    DisableBFExecution(testsToExclude, runsetConfig);
                    break;
                }
                case SealightsConfiguration.eSealightsEntityLevel.ActivitiesGroup:
                {
                    DisableActivitiesGroupExecution(testsToExclude, runsetConfig);
                    break;
                }
                case SealightsConfiguration.eSealightsEntityLevel.Activity:
                {
                    DisableActivitiesExecution(testsToExclude, runsetConfig);
                    break;
                }
                default:
                {
                    throw new InvalidEnumArgumentException("Not a valid value");
                }
            }
        }

        private void DisableBFExecution(string[] testsToExclude, RunSetConfig runsetConfig)
        {
            foreach (GingerRunner GR in runsetConfig.GingerRunners)
            {
                if (GR.Active)
                {
                    foreach (BusinessFlow BF in GR.Executor.BusinessFlows)
                    {
                        if (testsToExclude.Contains(BF.Guid.ToString()) && BF.Active)
                        {
                            BF.Active = false;
                            deactivatedBF.Add(BF);
                        }
                    }
                }
            }
        }
        private void DisableActivitiesGroupExecution(string[] testsToExclude, RunSetConfig runsetConfig)
        {
            foreach (GingerRunner GR in runsetConfig.GingerRunners)
            {
                if (GR.Active)
                {
                    foreach (BusinessFlow BF in GR.Executor.BusinessFlows)
                    {
                        if (BF.Active)
                        {
                            foreach (GingerCore.Activities.ActivitiesGroup AG in BF.ActivitiesGroups)
                            {
                                if (testsToExclude.Contains(AG.Guid.ToString()))
                                {
                                    foreach (GingerCore.Activities.ActivityIdentifiers AI in AG.ActivitiesIdentifiers)
                                    {
                                        if (AI.IdentifiedActivity.Active)
                                        {
                                            AI.IdentifiedActivity.Active = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void DisableActivitiesExecution(string[] testsToExclude, RunSetConfig runsetConfig)
        {
            foreach (GingerRunner GR in runsetConfig.GingerRunners)
            {
                if (GR.Active)
                {
                    foreach (BusinessFlow BF in GR.Executor.BusinessFlows)
                    {
                        if (BF.Active)
                        {
                            foreach (GingerCore.Activity Activity in BF.Activities)
                            {
                                if (testsToExclude.Contains(Activity.Guid.ToString()) && Activity.Active)
                                {
                                    Activity.Active = false;
                                }
                            }
                        }
                    }
                }
            }
        }
        private void ReactivateBF(List<BusinessFlow> deactivatedBF)
        {
            foreach (BusinessFlow BF in deactivatedBF)
            {
                BF.Active = true;
            }
        }
    }
}

