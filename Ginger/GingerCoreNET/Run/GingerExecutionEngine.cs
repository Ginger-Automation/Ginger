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
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.BusinessFlowLib;
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.SealightsExecutionLogger;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.GeneralLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.RosLynLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using Activity = GingerCore.Activity;

namespace Ginger.Run
{
    public enum eRunSource
    {
        Runner,
        BusinessFlow,
        Activity,
        Action
    }

    public class GingerExecutionEngine : IGingerExecutionEngine
    {
        IContext IGingerExecutionEngine.Context { get => mContext; set => mContext = (Context)value; }

        Context mContext = new Context();
        public Context Context
        {
            get
            {
                return mContext;
            }
            set
            {
                mContext = value;
                ExecutionLoggerManager.mContext = value;
            }
        }



        public PublishToALMConfig PublishToALMConfig = null;

        List<RunListenerBase> mRunListeners = [];
        public List<RunListenerBase> RunListeners { get { return mRunListeners; } }

        private bool mStopRun = false;
        private bool mStopBusinessFlow = false;

        private bool mCurrentActivityChanged = false;
        //private bool mErrorHandlerExecuted = false;

        BusinessFlow mExecutedBusinessFlowWhenStopped = null;
        Activity mExecutedActivityWhenStopped = null;
        Act mExecutedActionWhenStopped = null;

        public BusinessFlow ExecutedBusinessFlowWhenStopped => mExecutedBusinessFlowWhenStopped;
        public Activity ExecutedActivityWhenStopped => mExecutedActivityWhenStopped;
        public Act ExecutedActionWhenStopped => mExecutedActionWhenStopped;

        Activity mLastExecutedActivity;

        private eRunSource? mRunSource = null;
        private bool mErrorPostExecutionActionFlowBreaker;
        eErrorHandlerPostExecutionAction handlerPostExecutionAction;


        //!!! remove from here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public string ExecutionLogFolder { get; set; }
        private BusinessFlow mCurrentBusinessFlow;
        public BusinessFlow CurrentBusinessFlow
        {
            get
            {
                return mCurrentBusinessFlow;
            }
            set
            {
                mCurrentBusinessFlow = value;
                mContext.BusinessFlow = mCurrentBusinessFlow;
                mContext.Runner = this;
            }
        }

        private BusinessFlow mPreviousBusinessFlow;
        public BusinessFlow PreviousBusinessFlow
        {
            get
            {
                return mPreviousBusinessFlow;
            }
            set
            {
                mPreviousBusinessFlow = value;
            }
        }


        private BusinessFlow mLastFailedBusinessFlow;
        public BusinessFlow LastFailedBusinessFlow
        {
            get
            {
                return mLastFailedBusinessFlow;
            }
            set
            {
                mLastFailedBusinessFlow = value;
            }
        }

        public bool AgentsRunning = false;
        public ExecutionWatch RunnerExecutionWatch = new ExecutionWatch();
        public eExecutedFrom ExecutedFrom;
        public string CurrentGingerLogFolder = string.Empty;
        public string CurrentHTMLReportFolder = string.Empty;



        public eRunLevel RunLevel { get; set; }
        public string SolutionFolder { get; set; }
        public bool HighLightElement { get; set; }

        private bool mIsRunning = false;
        public bool IsRunning
        {
            get
            {
                return mIsRunning;
            }
            set
            {
                mIsRunning = value;
                GingerRunner.OnPropertyChanged(nameof(IsRunning));
            }
        }

        // Only for Run time, no need to serialize        
        public DateTime StartTimeStamp { get; set; }

        public DateTime EndTimeStamp { get; set; }

        public double? Elapsed
        {
            get
            {
                return RunnerExecutionWatch.runWatch.ElapsedMilliseconds;
            }
        }

        private int mTotalBusinessflow;
        public int TotalBusinessflow
        {
            get
            {
                mTotalBusinessflow = BusinessFlows.Count;
                return mTotalBusinessflow;
            }
            set
            {
                mTotalBusinessflow = value;
                GingerRunner.OnPropertyChanged("TotalBusinessflow");
            }
        }



        // public SelfHealingConfig SelfHealingConfiguration = new SelfHealingConfig();

        public ObservableList<Platform> Platforms = [];//TODO: delete me once projects moved to new Apps/Platform config, meanwhile enable to load old run set config, but ignore the value


        public ObservableList<Agent> SolutionAgents { get; set; } = [];

        public ObservableList<ApplicationPlatform> SolutionApplications { get; set; }

        private ExecutionLoggerConfiguration mSelectedExecutionLoggerConfiguration
        {
            get
            {
                if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null)
                {
                    return WorkSpace.Instance.Solution.LoggerConfigurations;
                }
                else
                {
                    return null;
                }
            }
        }

        public eRunStatus Status
        {
            get { return mGingerRunner.Status; }
            set
            {
                mGingerRunner.Status = value;
            }
        }

        public bool IsUpdateBusinessFlowRunList { get; set; }

        /// <summary>
        /// ID which been provided for each execution instance on the Activity
        /// </summary>
        public Guid ExecutionId { get; set; }

        public Guid ParentExecutionId { get; set; }
        public int ExecutionLogBusinessFlowsCounter { get; set; }
        private GingerRunner mGingerRunner;

        public GingerRunner GingerRunner { get { return mGingerRunner; } }
        public GingerExecutionEngine(GingerRunner GingerRunner)
        {
            mGingerRunner = GingerRunner;
            mGingerRunner.Executor = this;

            ExecutedFrom = eExecutedFrom.Run;
            // temp to be configure later !!!!!!!!!!!!!!!!!!!!!!!
            //RunListeners.Add(new ExecutionProgressReporterListener()); //Disabling till ExecutionLogger code will be enhanced
            RunListeners.Add(new ExecutionLoggerManager(mContext, ExecutedFrom));
            InitializeAccountReportExecutionLogger();
            InitializeSealightReportExecutionLogger();

            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations != null)
            {
                WorkSpace.Instance.Solution.LoggerConfigurations.PublishToCentralizedDbChanged -= InitializeAccountReportExecutionLogger;
                WorkSpace.Instance.Solution.LoggerConfigurations.PublishToCentralizedDbChanged += InitializeAccountReportExecutionLogger;
            }

            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.SealightsConfiguration != null)
            {
                WorkSpace.Instance.Solution.SealightsConfiguration.SealightsConfigChanged -= InitializeSealightReportExecutionLogger;
                WorkSpace.Instance.Solution.SealightsConfiguration.SealightsConfigChanged += InitializeSealightReportExecutionLogger;
            }
        }

        public void InitializeAccountReportExecutionLogger()
        {
            var accountReportExecutionLogger = RunListeners.Find((runListeners) => runListeners.GetType() == typeof(AccountReportExecutionLogger));

            if (mSelectedExecutionLoggerConfiguration != null
                && mSelectedExecutionLoggerConfiguration.PublishLogToCentralDB == ePublishToCentralDB.Yes &&
                accountReportExecutionLogger == null)
            {
                RunListeners.Add(new AccountReportExecutionLogger(mContext));
            }

            else if (
                mSelectedExecutionLoggerConfiguration != null
                && mSelectedExecutionLoggerConfiguration.PublishLogToCentralDB == ePublishToCentralDB.No &&
                accountReportExecutionLogger != null
                )
            {
                RunListeners.Remove(accountReportExecutionLogger);
            }
        }

        public void InitializeSealightReportExecutionLogger()
        {
            var seaLightReportExecutionLogger = RunListeners.Find((runListeners) => runListeners.GetType() == typeof(SealightsReportExecutionLogger));

            if (mSelectedExecutionLoggerConfiguration != null &&
               WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == Configurations.SealightsConfiguration.eSealightsLog.Yes &&
                seaLightReportExecutionLogger == null
                )
            {
                RunListeners.Add(new SealightsReportExecutionLogger(mContext));
            }

            else if (
                mSelectedExecutionLoggerConfiguration != null &&
               WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == Configurations.SealightsConfiguration.eSealightsLog.No &&
                seaLightReportExecutionLogger != null
                )
            {
                RunListeners.Remove(seaLightReportExecutionLogger);
            }
        }

        public GingerExecutionEngine(GingerRunner GingerRunner, Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            mGingerRunner = GingerRunner;
            mGingerRunner.Executor = this;

            ExecutedFrom = executedFrom;

            // temp to be configure later !!!!!!!!!!!!!!!!!!!!!!
            //RunListeners.Add(new ExecutionProgressReporterListener()); //Disabling till ExecutionLogger code will be enhanced
            RunListeners.Add(new ExecutionLoggerManager(mContext, ExecutedFrom));
            if (ExecutedFrom != eExecutedFrom.Automation)
            {
                RunListeners.Add(new AccountReportExecutionLogger(mContext));
            }

            if (ExecutedFrom != eExecutedFrom.Automation && WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog == Configurations.SealightsConfiguration.eSealightsLog.Yes)
            {
                RunListeners.Add(new SealightsReportExecutionLogger(mContext));
            }


            //if (WorkSpace.Instance != null && !WorkSpace.Instance.Telemetry.DoNotCollect)
            //{
            //    RunListeners.Add(new TelemetryRunListener());
            //}

        }


        public void SetExecutionEnvironment(ProjEnvironment defaultEnv, ObservableList<ProjEnvironment> allEnvs)
        {
            mGingerRunner.ProjEnvironment = null;
            if (mGingerRunner.UseSpecificEnvironment == true && string.IsNullOrEmpty(mGingerRunner.SpecificEnvironmentName) == false)
            {
                ProjEnvironment specificEnv = allEnvs
                    .FirstOrDefault(env => string.Equals(env.Name, mGingerRunner.SpecificEnvironmentName));
                if (specificEnv != null)
                {
                    //use Env copy to avoid parallel runners sharing runner exclusive resources like DB connections etc.
                    mGingerRunner.ProjEnvironment = (ProjEnvironment)specificEnv.CreateCopy(setNewGUID: false, deepCopy: true);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Runner Environment '{0}' was not found. Using default Environment instead", mGingerRunner.SpecificEnvironmentName));
                }
            }

            if (mGingerRunner.ProjEnvironment == null)
            {
                //use Env copy to avoid parallel runners sharing runner exclusive resources like DB connections etc.
                mGingerRunner.ProjEnvironment = (ProjEnvironment)defaultEnv.CreateCopy(setNewGUID: false, deepCopy: true);
            }
        }

        public ISolution CurrentSolution { get; set; }



        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunsetStatus
        {
            get
            {
                if (BusinessFlows.Count == 0)
                {
                    return eRunStatus.Skipped;
                }
                else if (BusinessFlows.Any(x => x.RunStatus == eRunStatus.Stopped))
                {
                    return eRunStatus.Stopped;
                }
                else if (BusinessFlows.Any(x => x.RunStatus == eRunStatus.Failed))
                {
                    return eRunStatus.Failed;
                }
                else if (BusinessFlows.Any(x => x.RunStatus == eRunStatus.Blocked))
                {
                    return eRunStatus.Blocked;
                }
                else if (BusinessFlows.Count != 0 && BusinessFlows.Count(x => x.RunStatus == eRunStatus.Skipped) == BusinessFlows.Count)
                {
                    return eRunStatus.Skipped;
                }
                else if (BusinessFlows.Count != 0 && BusinessFlows.Count(x => x.RunStatus is eRunStatus.Passed or eRunStatus.Skipped) == BusinessFlows.Count)
                {
                    return eRunStatus.Passed;
                }
                else
                {
                    return eRunStatus.Pending;
                }
            }
        }
        public void UpdateBusinessFlowsRunList()
        {
            List<BusinessFlowRun> oldBFRuns = new(GingerRunner.BusinessFlowsRunList);
            List<BusinessFlowRun> newBFRuns = [];

            foreach (BusinessFlow bf in BusinessFlows)
            {
                Guid newBFRunGuid = Guid.NewGuid();

                //reuse the same Guid for BusinessFlowRun if the wrapped BF is the same
                BusinessFlowRun oldBFRun = oldBFRuns.FirstOrDefault(bfr => bfr.BusinessFlowGuid == bf.Guid);
                if (oldBFRun != null)
                {
                    newBFRunGuid = oldBFRun.Guid;
                    oldBFRuns.Remove(oldBFRun);
                }

                BusinessFlowRun newBFRun = new()
                {
                    Guid = newBFRunGuid,
                    BusinessFlowName = bf.Name,
                    BusinessFlowGuid = bf.Guid,
                    BusinessFlowIsActive = bf.Active,
                    BusinessFlowIsMandatory = bf.Mandatory,
                    BusinessFlowInstanceGuid = bf.InstanceGuid,
                    BusinessFlowRunDescription = bf.RunDescription,
                    ExternalID = bf.ExternalID,
                    BFFlowControls = bf.BFFlowControls
                };

                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(true))
                {
                    //save only variables which were modified in this run configurations
                    if (var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputValue) == false)
                    {
                        VariableBase varCopy = (VariableBase)var.CreateCopy(false);
                        newBFRun.BusinessFlowCustomizedRunVariables.Add(varCopy);
                    }
                }

                newBFRuns.Add(newBFRun);
            }

            GingerRunner.BusinessFlowsRunList.Clear();

            foreach (BusinessFlowRun newBFRun in newBFRuns)
            {
                GingerRunner.BusinessFlowsRunList.Add(newBFRun);
            }
        }

        public ObservableList<BusinessFlow> BusinessFlows { get; set; } = [];

        public async Task<int> RunRunnerAsync()
        {
            var result = await Task.Run(() =>
            {
                RunRunner();
                return 1;
            });
            return result;
        }



        public void RunRunner(bool doContinueRun = false)
        {
            bool runnerExecutionSkipped = false;

            if (!string.IsNullOrEmpty(mGingerRunner.SpecificEnvironmentName))
            {
                Reporter.ToLog(eLogLevel.INFO, $"Selected Environment for {mGingerRunner.Name} is {mGingerRunner.SpecificEnvironmentName}");
            }

            try
            {
                if (mGingerRunner.Active == false || BusinessFlows.Count == 0 || !BusinessFlows.Any(x => x.Active))
                {
                    runnerExecutionSkipped = true;
                    return;
                }
                if (RunLevel == eRunLevel.Runner)
                {
                    ExecutionLoggerManager.mExecutionLogger.StartRunSet();
                }
                if (doContinueRun == false)
                {
                    NotifyRunnerRunstart();
                }
                //Init 
                mGingerRunner.Status = eRunStatus.Started;
                IsRunning = true;
                mStopRun = false;
                if (doContinueRun == false)
                {
                    SetupVirtualAgents();
                    RunnerExecutionWatch.StartRunWatch();
                }
                else
                {
                    RunnerExecutionWatch.ContinueWatch();
                    ContinueTimerVariables(BusinessFlow.SolutionVariables);
                }

                //do Validations

                //Do execution preparations
                if (doContinueRun == false && this.ExecutedFrom == eExecutedFrom.Automation)
                {
                    UpdateApplicationAgents();
                }
                //Start execution
                Status = eRunStatus.Running;

                int startingBfIndx = 0;
                if (doContinueRun == false)
                {
                    startingBfIndx = 0;
                }
                else
                {
                    startingBfIndx = BusinessFlows.IndexOf(CurrentBusinessFlow);//skip BFs which already executed
                }

                if (mRunSource == null)
                {
                    mRunSource = eRunSource.Runner;
                }
                int? flowControlIndx = null;
                for (int bfIndx = startingBfIndx; bfIndx < BusinessFlows.Count; CalculateNextBFIndx(ref flowControlIndx, ref bfIndx))
                {

                    //need to add code here to check previous Execution Deatils and ReRunFailed check 

                    BusinessFlow executedBusFlow = BusinessFlows[bfIndx];
                    ExecutionLogBusinessFlowsCounter = bfIndx;
                    //stop if needed before executing next BF
                    if (mStopRun)
                    {
                        break;
                    }

                    //validate BF run
                    if (!executedBusFlow.Active)// || (activeBusinessFlows.Count > 0 && !activeBusinessFlows.Any(x=>x.Guid == executedBusFlow.Guid)))
                    {
                        //set BF status as skipped                     
                        SetBusinessFlowActivitiesAndActionsSkipStatus(executedBusFlow);
                        CalculateBusinessFlowFinalStatus(executedBusFlow);
                        continue;
                    }
                    else if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active && executedBusFlow.RunStatus == eRunStatus.Skipped)
                    {
                        SetBusinessFlowActivitiesAndActionsSkipStatus(executedBusFlow);
                        CalculateBusinessFlowFinalStatus(executedBusFlow);
                        continue;
                    }

                    if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active && executedBusFlow.RunStatus == eRunStatus.Passed)
                    {
                        SetBusinessFlowActivitiesAndActionsSkipStatus(executedBusFlow);
                        CalculateBusinessFlowFinalStatus(executedBusFlow);
                        continue;
                    }

                    if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.ReRunConfigurations.Active && executedBusFlow.RunStatus == eRunStatus.Failed)
                    {
                        executedBusFlow.Reset();
                    }

                    //Run Bf
                    if (doContinueRun && bfIndx == startingBfIndx)//this is the BF to continue from
                    {
                        RunBusinessFlow(null, false, true);//Continue BF run
                    }
                    else
                    {
                        //Execute the Business Flow
                        RunBusinessFlow(executedBusFlow, doResetErrorHandlerExecutedFlag: true);// full BF run
                    }
                    //Do "During Execution" Run set Operations

                    StartPublishResultsToAlmTask(executedBusFlow);
                    //Call For Business Flow Control
                    flowControlIndx = DoBusinessFlowControl(executedBusFlow);
                    if (flowControlIndx == null && executedBusFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed) //stop if needed based on current BF failure
                    {
                        LastFailedBusinessFlow = executedBusFlow;
                        if ((executedBusFlow.Mandatory == true) || (executedBusFlow.Mandatory == false & mGingerRunner.RunOption == GingerRunner.eRunOptions.StopAllBusinessFlows))
                        {
                            SetNextBusinessFlowsBlockedStatus();
                            break;
                        }
                    }

                    if (executedBusFlow.RunStatus == eRunStatus.Failed && mErrorPostExecutionActionFlowBreaker)
                    {
                        mErrorPostExecutionActionFlowBreaker = false;
                        if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ContinueFromNextBusinessFlow)
                        {
                            flowControlIndx = null;
                        }
                        else if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.StopRun)
                        {
                            SetPendingBusinessFlowsSkippedStatus();
                            break;
                        }
                    }
                }
            }
            catch (Exception ec)
            {

            }
            finally
            {
                //Post execution items to do
                SetPendingBusinessFlowsSkippedStatus();

                if (!runnerExecutionSkipped)
                {
                    if (!mStopRun)//not on stop run
                    {
                        CloseAgents();
                        if (mGingerRunner.ProjEnvironment != null && mGingerRunner.ProjEnvironment.Applications != null)
                        {
                            //needed for db close connection
                            foreach (EnvApplication ea in mGingerRunner.ProjEnvironment.Applications)
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
                            mGingerRunner.ProjEnvironment.CloseEnvironment();
                        }
                        Status = eRunStatus.Completed;
                        RunsetExecutor.ClearAndResetVirtualAgents(WorkSpace.Instance.RunsetExecutor.RunSetConfig, this);
                    }

                    PostScopeVariableHandling(BusinessFlow.SolutionVariables);
                    IsRunning = false;
                    RunnerExecutionWatch.StopRunWatch();
                    Status = RunsetStatus;

                    NotifyOnSkippedRunnerEntities();

                    NotifyRunnerRunEnd(CurrentBusinessFlow.ExecutionFullLogFolder);

                    if (RunLevel == eRunLevel.Runner)
                    {
                        ExecutionLoggerManager.mExecutionLogger.EndRunSet();
                        RunLevel = eRunLevel.NA;
                    }
                }
                else
                {
                    NotifySkippedEntitiesWhenRunnerExecutionSkipped();
                    Status = RunsetStatus;
                }

                if (mRunSource == eRunSource.Runner)
                {
                    mRunSource = null;
                    mErrorPostExecutionActionFlowBreaker = false;
                }

            }
        }


        private void StartPublishResultsToAlmTask(BusinessFlow executedBusFlow)
        {
            if (PublishToALMConfig != null)
            {
                Task ExportResultTask = Task.Run(() =>
                {
                    var runsetAction = WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions.FirstOrDefault(f => f is RunSetActionPublishToQC && f.RunAt.Equals(RunSetActionBase.eRunAt.DuringExecution) && f.Active);

                    if (runsetAction != null)
                    {
                        var prevStatus = runsetAction.Status;
                        bool isSuccess = false;
                        runsetAction.Status = RunSetActionBase.eRunSetActionStatus.Running;
                        ObservableList<BusinessFlow> bfs = [executedBusFlow];
                        string result = "";
                        try
                        {
                            isSuccess = TargetFrameworkHelper.Helper.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig, eALMConnectType.Silence);
                            if (!isSuccess)
                            {
                                runsetAction.Errors += result + Environment.NewLine;
                            }
                        }
                        catch (Exception ex)
                        {
                            runsetAction.Errors += ex.Message + Environment.NewLine;
                            Reporter.ToLog(eLogLevel.ERROR, $"Failed to publish execution result to ALM for {executedBusFlow.Name}", ex);
                        }
                        finally
                        {
                            if (!isSuccess)
                            {
                                runsetAction.Status = RunSetActionBase.eRunSetActionStatus.Failed;
                            }
                            else
                            {
                                runsetAction.Status = prevStatus;
                            }
                        }
                    }
                });

                WorkSpace.Instance.RunsetExecutor.ALMResultsPublishTaskPool.Add(ExportResultTask);
            }
        }

        private void NotifyOnSkippedRunnerEntities()
        {
            // Get all 'Skipped' BF
            var businessFlowList = BusinessFlows.Where(x => x.RunStatus == eRunStatus.Skipped);

            foreach (BusinessFlow businessFlow in businessFlowList)
            {
                NotifyBusinessFlowSkipped(businessFlow);
            }

            // Saarch for Activities-Groups and Activities in All BF
            foreach (BusinessFlow businessFlow in BusinessFlows)
            {
                // 'Skipped' Activities Group
                var activitiesGroupList = businessFlow.ActivitiesGroups.Where(x => x.RunStatus == eActivitiesGroupRunStatus.Skipped && x.ActivitiesIdentifiers.Count > 0);

                foreach (ActivitiesGroup activitiesGroup in activitiesGroupList)
                {
                    NotifyActivityGroupSkipped(activitiesGroup);
                }

                // 'Skipped' Activities
                var activitiesList = businessFlow.Activities.Where(x => x.Status == eRunStatus.Skipped);

                foreach (Activity activity in activitiesList)
                {
                    NotifyActivitySkipped(activity);
                }
            }
        }
        private void NotifySkippedEntitiesWhenRunnerExecutionSkipped()
        {
            // Get all 'Skipped' BF
            var businessFlowList = BusinessFlows.Where(x => x.RunStatus == eRunStatus.Skipped);

            foreach (BusinessFlow businessFlow in businessFlowList)
            {
                NotifyBusinessFlowSkipped(businessFlow);
                // Search for Activities-Groups and Activities in All BF

                // 'Skipped' Activities Group
                var activitiesGroupList = businessFlow.ActivitiesGroups.Where(x => x.RunStatus == eActivitiesGroupRunStatus.Skipped && x.ActivitiesIdentifiers.Count > 0);
                foreach (ActivitiesGroup activitiesGroup in activitiesGroupList)
                {
                    NotifyActivityGroupSkipped(activitiesGroup);
                }
                // 'Skipped' Activities
                var activitiesList = businessFlow.Activities.Where(x => x.Status == eRunStatus.Skipped);
                foreach (Activity activity in activitiesList)
                {
                    NotifyActivitySkipped(activity);
                }
            }
        }


        private static readonly SemaphoreSlim semaphoreSlim = new(1, 1);
        private void SetupVirtualAgents()
        {
            if (WorkSpace.Instance != null && WorkSpace.Instance.RunsetExecutor != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig != null)
            {

                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunModeParallel)
                {
                    RunSetConfig runSetConfig = WorkSpace.Instance.RunsetExecutor.RunSetConfig;
                    List<IAgent> RunnerAgentList = [];
                    foreach (ApplicationAgent applicationAgent in mGingerRunner.ApplicationAgents)
                    {
                        if (applicationAgent.AgentName != null)
                        {
                            ObservableList<Agent> agents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();

                            var agent = agents.FirstOrDefault(a => a.Name.Equals(applicationAgent.AgentName));
                            try
                            {
                                semaphoreSlim.Wait();
                                if (agent != null)
                                {
                                    if (agent.AgentOperations == null)
                                    {
                                        AgentOperations agentOperations = new AgentOperations(agent);
                                        agent.AgentOperations = agentOperations;
                                    }
                                    /// <summary>
                                    /// logic for if need to assign virtual agent
                                    /// Second condition if any agent is used in different agent then only it will create virtual agent for that specific agent 
                                    /// </summary>
                                    if (agent.SupportVirtualAgent() && runSetConfig.ActiveAgentListWithRunner.Where(entry => entry.Key != mGingerRunner.Guid).Select(y => y.Value).Where(y => y != null).Any(x => (x.Any(k => ((Agent)k).Guid == agent.Guid || (((Agent)k).ParentGuid != null && ((Agent)k).ParentGuid == agent.Guid)))))
                                    {
                                        var virtualagent = agent.CreateCopy(true) as Agent;
                                        virtualagent.AgentOperations = new AgentOperations(virtualagent);
                                        virtualagent.ParentGuid = agent.Guid;
                                        virtualagent.Name = agent.Name + " Virtual";
                                        virtualagent.IsVirtual = true;
                                        virtualagent.DriverClass = agent.DriverClass;
                                        virtualagent.DriverType = agent.DriverType;
                                        applicationAgent.Agent = virtualagent;
                                        virtualagent.DriverConfiguration = agent.DriverConfiguration;
                                    }
                                }


                                if (applicationAgent.Agent != null)
                                {
                                    RunnerAgentList.Add(applicationAgent.Agent);

                                    if (runSetConfig.ActiveAgentListWithRunner.Any(kvp => kvp.Key.Equals(mGingerRunner.Guid)))
                                    {
                                        runSetConfig.ActiveAgentListWithRunner[mGingerRunner.Guid] = RunnerAgentList;
                                    }
                                    else
                                    {
                                        runSetConfig.ActiveAgentListWithRunner.Add(mGingerRunner.Guid, RunnerAgentList);
                                    }
                                }

                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }
                        }
                    }

                }
            }
        }


        //Calculate Next bfIndex for RunRunner Function
        private void CalculateNextBFIndx(ref int? flowControlIndx, ref int bfIndx)
        {
            if (flowControlIndx != null) //set bfIndex in case of BfFlowControl
            {
                bfIndx = (int)flowControlIndx;
            }
            else
            {
                bfIndx++;
            }
        }

        private bool PrepareVariables()
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //We need to set variable mapped values only when running run set
                if (SetVariableMappedValues() == false)
                {
                    return false;
                }
            }

            PrepDynamicVariables();

            return true;
        }

        private bool SetVariableMappedValues()
        {
            BusinessFlowRun businessFlowRun = GetCurrenrtBusinessFlowRun();

            List<VariableBase> cachedVariables = null;

            //set the vars to update
            var inputVars = CurrentBusinessFlow.GetBFandActivitiesVariabeles(true);
            List<VariableBase> variables = null;
            List<VariableBase> outputVariables = null;
            //do actual value update

            if (inputVars.Count > 0)
            {
                Reporter.ToLog(eLogLevel.INFO, $"Mapping {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} {GingerDicser.GetTermResValue(eTermResKey.Variable)} with customized values.");
            }

            foreach (VariableBase inputVar in inputVars)
            {
                try
                {
                    string mappedValue = null;
                    if (inputVar.MappedOutputType == VariableBase.eOutputType.Variable)//Legacy
                    {
                        if (variables == null)
                        {
                            variables = GetPossibleOutputVariables(WorkSpace.Instance.RunsetExecutor.RunSetConfig, CurrentBusinessFlow, includeGlobalVars: true, includePrevRunnersVars: false);
                        }
                        VariableBase var = variables.Find(x => x.Name == inputVar.MappedOutputValue);
                        if (var != null)
                        {
                            mappedValue = string.IsNullOrEmpty(var.Value) ? string.Empty : var.Value;
                        }
                    }
                    else if (inputVar.MappedOutputType == VariableBase.eOutputType.OutputVariable)
                    {
                        if (outputVariables == null)
                        {
                            outputVariables = GetPossibleOutputVariables(WorkSpace.Instance.RunsetExecutor.RunSetConfig, CurrentBusinessFlow, includeGlobalVars: false, includePrevRunnersVars: true);
                        }
                        VariableBase outputVar = outputVariables.Find(x => x.VariableInstanceInfo == inputVar.MappedOutputValue);
                        if (outputVar != null)
                        {
                            mappedValue = string.IsNullOrEmpty(outputVar.Value) ? string.Empty : outputVar.Value;
                        }
                    }
                    else if (inputVar.MappedOutputType == VariableBase.eOutputType.GlobalVariable)
                    {
                        Guid mappedVarGuid = Guid.Empty;
                        if (Guid.TryParse(inputVar.MappedOutputValue, out mappedVarGuid))
                        {
                            VariableBase globalVar = WorkSpace.Instance.Solution.Variables.FirstOrDefault(x => x.Guid == mappedVarGuid);
                            if (globalVar != null)
                            {
                                mappedValue = string.IsNullOrEmpty(globalVar.Value) ? string.Empty : globalVar.Value;
                            }
                        }
                    }
                    else if (inputVar.MappedOutputType == VariableBase.eOutputType.ApplicationModelParameter)
                    {
                        Guid mappedModelParamGuid = Guid.Empty;
                        if (Guid.TryParse(inputVar.MappedOutputValue, out mappedModelParamGuid))
                        {
                            GlobalAppModelParameter modelParam = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<GlobalAppModelParameter>(mappedModelParamGuid);
                            if (modelParam != null)
                            {
                                mappedValue = string.IsNullOrEmpty(modelParam.CurrentValue) ? string.Empty : modelParam.CurrentValue;
                            }
                        }
                    }
                    else if (inputVar.MappedOutputType == VariableBase.eOutputType.DataSource||inputVar.MappedOutputType == VariableBase.eOutputType.ValueExpression)
                    {
                        string calculatedValue = ValueExpression.Calculate(mGingerRunner.ProjEnvironment, CurrentBusinessFlow, inputVar.MappedOutputValue, mGingerRunner.DSList);
                        mappedValue = string.IsNullOrEmpty(calculatedValue) ? string.Empty : calculatedValue;
                    }
                    else if (inputVar.GetType() != typeof(VariablePasswordString) && inputVar.GetType() != typeof(VariableDynamic))//what this for???
                    {
                        //if input variable value mapped to none, and value is differt from origin                   
                        if (inputVar.DiffrentFromOrigin)
                        {
                            // we take value of customized variable from BusinessFlowRun
                            VariableBase runVar = businessFlowRun?.BusinessFlowCustomizedRunVariables?.FirstOrDefault(v => v.ParentGuid == inputVar.ParentGuid && v.ParentName == inputVar.ParentName && v.Name == inputVar.Name);
                            if (runVar != null)
                            {
                                mappedValue = string.IsNullOrEmpty(runVar.Value) ? string.Empty : runVar.Value;
                            }
                        }
                        else//????
                        {
                            if (cachedVariables == null)
                            {
                                BusinessFlow cachedBusinessFlow = WorkSpace.Instance?.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(CurrentBusinessFlow.Guid);
                                cachedVariables = cachedBusinessFlow?.GetBFandActivitiesVariabeles(true).ToList();
                            }

                            //If value is not different from origin we take original value from business flow on cache
                            VariableBase cacheVariable = cachedVariables?.Find(v => v.ParentGuid == inputVar.ParentGuid && v.ParentName == inputVar.ParentName && v.Name == inputVar.Name);
                            if (cacheVariable != null)
                            {
                                mappedValue = string.IsNullOrEmpty(cacheVariable.Value) ? string.Empty : cacheVariable.Value;
                            }
                        }
                    }

                    if (mappedValue != null)
                    {
                        if (inputVar.SupportSetValue)
                        {
                            if (inputVar.SetValue(mappedValue) == false)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to set Value '{0}' into '{1}' Input {2} in the '{3}' {4}. Mapped data type '{5}' mapped data value '{6}'", mappedValue, inputVar.Name, GingerDicser.GetTermResValue(eTermResKey.Variable), CurrentBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), inputVar.MappedOutputType, inputVar.MappedOutputValue));
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (inputVar.MappedOutputType != VariableBase.eOutputType.None)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to set '{0}' Input {1} mapped value in the '{2}' {3}. Mapped data type '{4}' mapped data value '{5}'", inputVar.Name, GingerDicser.GetTermResValue(eTermResKey.Variable), CurrentBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), inputVar.MappedOutputType, inputVar.MappedOutputValue));
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to set '{0}' Input {1} mapped value in '{2}' {3}. Mapped data type '{4}' mapped data value '{5}'", inputVar.Name, GingerDicser.GetTermResValue(eTermResKey.Variable), CurrentBusinessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), inputVar.MappedOutputType, inputVar.MappedOutputValue), ex);
                    return false;
                }
            }

            return true;
        }

        public List<VariableBase> GetPossibleOutputVariables(RunSetConfig runSetConfig, BusinessFlow businessFlow, bool includeGlobalVars = false, bool includePrevRunnersVars = true)
        {
            List<VariableBase> outputVariables;

            //Global Variabels
            if (includeGlobalVars && BusinessFlow.SolutionVariables != null)
            {
                outputVariables = BusinessFlow.SolutionVariables.ToList();
            }
            else
            {
                outputVariables = [];
            }

            Dictionary<string, int> variablePaths = [];
            //Previous Business Flows output variabels
            for (int i = BusinessFlows.IndexOf(businessFlow) - 1; i >= 0; i--)//doing in reverse for sorting by latest value in case having the same var more than once
            {
                foreach (VariableBase var in BusinessFlows[i].GetBFandActivitiesVariabeles(false, false, true))
                {
                    var.Path = var.Name + " [" + BusinessFlows[i].Name + "]";
                    if (variablePaths.ContainsKey(var.Path))
                    {
                        variablePaths[var.Path] += 1;
                    }
                    else
                    {
                        variablePaths.Add(var.Path, 1);
                    }

                    var.VariableInstanceInfo = BusinessFlows[i].InstanceGuid.ToString() + "_" + var.Guid;
                    outputVariables.Add(var);
                }
            }

            //Previous Runners Business Flows Output Variabels
            if (includePrevRunnersVars && runSetConfig.RunModeParallel == false && runSetConfig.GingerRunners.IndexOf(GingerRunner) > 0)
            {
                for (int j = runSetConfig.GingerRunners.IndexOf(GingerRunner) - 1; j >= 0; j--)//doing in reverse for sorting by latest value in case having the same var more than once
                {
                    int i = runSetConfig.GingerRunners[j].Executor.BusinessFlows.Count - 1;
                    foreach (BusinessFlow bf in runSetConfig.GingerRunners[j].Executor.BusinessFlows.Reverse())
                    {
                        foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(false, false, true))
                        {

                            var.Path = var.Name + " [" + runSetConfig.GingerRunners[j].Name + ": " + bf.Name + "]";
                            if (variablePaths.ContainsKey(var.Path))
                            {
                                variablePaths[var.Path] += 1;
                            }
                            else
                            {
                                variablePaths.Add(var.Path, 1);
                            }
                            var.VariableInstanceInfo = bf.InstanceGuid.ToString() + "_" + var.Guid;

                            outputVariables.Add(var);

                        }
                        i--;
                    }
                }
            }

            //Handle variables with duplicate path and add index
            foreach (KeyValuePair<string, int> duplicatePath in variablePaths)
            {
                int count = duplicatePath.Value;
                if (duplicatePath.Value > 1)
                {
                    foreach (VariableBase var in outputVariables.Where(x => x.Path == duplicatePath.Key))
                    {
                        var.Path = var.Path.Insert(var.Path.LastIndexOf("]"), " (" + count + ")");
                        count--;
                    }
                }
            }
            return outputVariables;
        }

        private BusinessFlowRun GetCurrenrtBusinessFlowRun()
        {
            BusinessFlowRun businessFlowRun = GingerRunner.BusinessFlowsRunList.FirstOrDefault(x => x.BusinessFlowInstanceGuid == CurrentBusinessFlow?.InstanceGuid);

            if (businessFlowRun == null)
            {
                businessFlowRun = GingerRunner.BusinessFlowsRunList.FirstOrDefault(x => x.BusinessFlowGuid == CurrentBusinessFlow?.Guid);
            }
            return businessFlowRun;
        }

        public void StartAgent(Agent Agent)
        {
            try
            {
                Agent.ProjEnvironment = mGingerRunner.ProjEnvironment;
                Agent.BusinessFlow = CurrentBusinessFlow;
                Agent.SolutionFolder = SolutionFolder;
                Agent.DSList = mGingerRunner.DSList;
                Agent.AgentOperations.StartDriver();
                Agent.AgentOperations.WaitForAgentToBeReady();
                if (((AgentOperations)Agent.AgentOperations).Status == Agent.eStatus.NotStarted)
                {
                    ((AgentOperations)Agent.AgentOperations).IsFailedToStart = true;
                }
            }
            catch (Exception e)
            {
                ((AgentOperations)Agent.AgentOperations).IsFailedToStart = true;
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }
        }

        public void StopAgents()
        {
            foreach (ApplicationAgent AA in mGingerRunner.ApplicationAgents)
            {
                if (AA.Agent != null)
                {
                    AA.Agent.AgentOperations.Close();
                }
            }
            AgentsRunning = false;
        }
        /*
        public void StartAgents()
        {
            ObservableList<ApplicationAgent> ApplicationAgentsToStartLast = new ObservableList<ApplicationAgent>();

            ApplicationAgentsToStartLast.Clear();
            //TODO: make it to something

            foreach (ApplicationAgent AA in ApplicationAgents)
            {
                if (((Agent)AA.Agent) == null)
                {
                    //TODO: else ask user to define agent  
                }
                else
                {    
                    if (((Agent)AA.Agent).Driver == null)
                    {
                        //what is this? hard coded? - if we need to set agents order let's add a way to sort the agents
                        if (((Agent)AA.Agent).DriverType == Agent.eDriverType.UnixShell)
                            ApplicationAgentsToStartLast.Add(AA);
                        else
                            StartAgent(((Agent)AA.Agent));
                    }
                }
            }

            if(ApplicationAgentsToStartLast.Count>0)
            {
                foreach (ApplicationAgent AA in ApplicationAgentsToStartLast)
                {
                    if (((Agent)AA.Agent).Driver == null)
                        StartAgent(((Agent)AA.Agent));
                }
            }

            //Wait for all agents to be running            
            foreach (ApplicationAgent AA in ApplicationAgents)
            {
                if (((Agent)AA.Agent) != null)
                {
                    ((Agent)AA.Agent).WaitForAgentToBeReady();
                }
            }
            AgentsRunning = true;
        }
        */
        public string GetAgentsNameToRun()
        {
            string agentsNames = string.Empty;
            foreach (ApplicationAgent AP in mGingerRunner.ApplicationAgents)
            {
                if (AP.Agent != null)
                {
                    agentsNames = agentsNames + " '" + AP.Agent.Name + "' for '" + AP.AppName + "',";
                }
                else
                {
                    agentsNames = agentsNames + " No Agent for - '" + AP.AppName + "',";
                }
            }
            agentsNames = agentsNames.TrimEnd(',');
            return agentsNames;
        }

        public async Task<int> RunActionAsync(Act act, bool checkIfActionAllowedToRun = true, bool moveToNextAction = true)
        {
            NotifyExecutionContext(AutomationTabContext.ActionRun);
            var result = await Task.Run(() =>
            {
                IsRunning = true;
                mStopRun = false;
                RunAction(act, checkIfActionAllowedToRun, moveToNextAction);
                IsRunning = false;
                return 1;
            });

            return result;
        }


        public void RunAction(Act act, bool checkIfActionAllowedToRun = true, bool moveToNextAction = true)
        {
            try
            {
                //act.PauseDirtyTracking();
                //init
                act.SolutionFolder = SolutionFolder;
                act.ExecutionParentGuid = CurrentBusinessFlow.InstanceGuid;

                if (mRunSource == null)
                {
                    mRunSource = eRunSource.Action;
                }

                //resetting the retry mechanism count before calling the function.
                act.RetryMechanismCount = 0;
                RunActionWithRetryMechanism(act, checkIfActionAllowedToRun, moveToNextAction);

                if ((act.EnableRetryMechanism & mStopRun == false) && !mErrorPostExecutionActionFlowBreaker)
                {
                    while (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && act.RetryMechanismCount < act.MaxNumberOfRetries & mStopRun == false)
                    {
                        //Wait
                        act.RetryMechanismCount++;
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Wait;

                        GiveUserFeedback();
                        ProcessIntervaleRetry(act);

                        if (mStopRun)
                        {
                            break;
                        }

                        //Run Again
                        RunActionWithRetryMechanism(act, checkIfActionAllowedToRun, moveToNextAction);
                    }
                }
                if (mStopRun)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                    //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                    SetDriverPreviousRunStoppedFlag(true);
                    NotifyActionEnd(act);
                }
                else
                {
                    SetDriverPreviousRunStoppedFlag(false);
                }
                SelfHealingExecuteInSimulationMode(act);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in Run Action", ex);
                act.Error = act.Error + "\nException in Run Action " + ex.Message;
                act.Status = eRunStatus.Failed;
            }
            finally
            {
                if (mRunSource == eRunSource.Action)
                {
                    mRunSource = null;
                    mErrorPostExecutionActionFlowBreaker = false;
                }
                //act.ResumeDirtyTracking();
            }
        }

        public void CheckAndExecutePostErrorHandlerAction()
        {
            if (!mErrorPostExecutionActionFlowBreaker)
            {
                return;
            }
            mErrorPostExecutionActionFlowBreaker = false;
            if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ReRunOriginActivity)
            {
                //CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.FirstOrDefault();
                //RunActivity(CurrentBusinessFlow.CurrentActivity, true, resetErrorHandlerExecutedFlag: false);
                RunActivity(CurrentBusinessFlow.CurrentActivity, resetErrorHandlerExecutedFlag: false);
            }
            else if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ReRunBusinessFlow)
            {
                RunBusinessFlow(CurrentBusinessFlow, doResetErrorHandlerExecutedFlag: false);
            }
        }

        private void SelfHealingExecuteInSimulationMode(Act act)
        {
            if (act.Status == eRunStatus.Failed && act.SupportSimulation && ((ExecutedFrom == eExecutedFrom.Automation && WorkSpace.Instance.AutomateTabSelfHealingConfiguration.AutoExecuteInSimulationMode) || (ExecutedFrom == eExecutedFrom.Run && WorkSpace.Instance.RunsetExecutor.RunSetConfig.SelfHealingConfiguration.AutoExecuteInSimulationMode)))
            {
                var isSimulationModeTemp = mGingerRunner.RunInSimulationMode;
                var actErrorBeforeSimulation = act.Error;
                var actExInfoBeforeSimulation = act.ExInfo;
                mGingerRunner.RunInSimulationMode = true;

                RunAction(act);

                mGingerRunner.RunInSimulationMode = isSimulationModeTemp;

                act.ExInfo = string.Concat(actExInfoBeforeSimulation, "\n", act.ExInfo, "\n", "Action Executed in simulation mode during self healing operation");

                act.Error = string.Concat(actErrorBeforeSimulation, "\n", act.Error);
            }
        }

        private void ProcessIntervaleRetry(Act act)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            if (act.RetryMechanismInterval > 0)
            {
                for (int i = 0; i < act.RetryMechanismInterval * 10; i++)
                {
                    if (mStopRun)
                    {
                        act.ExInfo += "Stopped";
                        //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        SetDriverPreviousRunStoppedFlag(true);
                        return;
                    }
                    else
                    {
                        //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        SetDriverPreviousRunStoppedFlag(false);
                    }
                    UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Wait, st);
                    Thread.Sleep(100);  // Multiply * 10 to get 1 sec                    
                }
            }
        }

        private void RunActionWithRetryMechanism(Act act, bool checkIfActionAllowedToRun = true, bool moveToNextAction = true)
        {
            bool actionExecuted = false;
            try
            {
                //Not suppose to happen but just in case        
                if (act == null)
                {
                    //Reporter.ToUser(eUserMsgKey.AskToSelectAction);
                    return;
                }

                if (checkIfActionAllowedToRun)//to avoid duplicate checks in case the RunAction function is called from RunActvity
                {
                    if (!act.Active)
                    {
                        SkipActionAndNotifyEnd(act);
                        act.ExInfo = "Action is not active.";
                        return;
                    }
                    if (!CheckRunInVisualTestingMode(act))
                    {
                        SkipActionAndNotifyEnd(act);
                        act.ExInfo = "Visual Testing Action Run Mode is Inactive.";
                    }
                    if (act.CheckIfVaribalesDependenciesAllowsToRun(CurrentBusinessFlow.CurrentActivity, true) == false)
                    {
                        return;
                    }
                }
                if (act.BreakPoint)
                {
                    StopRun();
                }
                if (mStopRun)
                {
                    return;
                }

                GingerRunner.eActionExecutorType ActionExecutorType = GingerRunner.eActionExecutorType.RunWithoutDriver;
                // !!!!!!!!!!!! Remove SW use eventtime
                Stopwatch st = new Stopwatch();
                st.Start();

                PrepAction(act, ref ActionExecutorType, st);


                if (mStopRun)
                {
                    return;
                }
                GiveUserFeedback();

                NotifyActionStart(act);
                actionExecuted = true;
                string actionStartTimeStr = string.Empty;
                while (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                {
                    // Add time stamp                     
                    actionStartTimeStr = string.Format("Execution Start Time: {0}", DateTime.Now.ToString());

                    RunActionWithTimeOutControl(act, ActionExecutorType);
                    CalculateActionFinalStatus(act);

                    // fetch all pop-up handlers
                    ObservableList<ErrorHandler> lstPopUpHandlers = GetAllErrorHandlersByType(eHandlerType.Popup_Handler);
                    if (lstPopUpHandlers.Count > 0 && !act.ErrorHandlerExecuted)
                    {
                        ExecuteErrorHandlerActivities(lstPopUpHandlers);
                        continue;
                    }

                    if (!act.ErrorHandlerExecuted
                        && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                    {
                        // returns list of mapped error handlers with the activity depending on type of error handling mapping chosen i.e. All Available Error Handlers, None or Specific Error Handlers
                        ObservableList<ErrorHandler> lstMappedErrorHandlers = GetErrorHandlersForCurrentActivity();

                        if (lstMappedErrorHandlers.Count <= 0)
                        {
                            break;
                        }

                        //for value expression calculations
                        CurrentBusinessFlow.ErrorHandlerOriginActivity = CurrentBusinessFlow.CurrentActivity;
                        Act orginAction = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                        CurrentBusinessFlow.ErrorHandlerOriginAction = (Act)orginAction.CreateCopy(false);
                        CurrentBusinessFlow.ErrorHandlerOriginAction.Status = orginAction.Status;
                        CurrentBusinessFlow.ErrorHandlerOriginAction.Error = orginAction.Error;
                        CurrentBusinessFlow.ErrorHandlerOriginAction.ExInfo = orginAction.ExInfo;
                        CurrentBusinessFlow.ErrorHandlerOriginAction.Elapsed = orginAction.Elapsed;

                        //ResetAction(act);
                        //act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
                        //NotifyActionStart(act);

                        ExecuteErrorHandlerActivities(lstMappedErrorHandlers);
                        // mErrorHandlerExecuted = true;

                        if (mErrorPostExecutionActionFlowBreaker || handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ContinueFromNextAction)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }

                }
                // Run any code needed after the action executed, used in ACTScreenShot save to file after driver took screen shot

                act.PostExecute();

                //Adding for new control
                ProcessStoretoValue(act);

                CalculateActionFinalStatus(act); //why we need to run it again?

                UpdateDSReturnValues(act);

                // Add time stamp                                 
                act.ExInfo = actionStartTimeStr + Environment.NewLine + act.ExInfo;

                ProcessScreenShot(act, ActionExecutorType);


                // Stop the counter before DoFlowControl
                st.Stop();

                // final timing of the action
                act.Elapsed = st.ElapsedMilliseconds;
                act.ElapsedTicks = st.ElapsedTicks;

                //check if we have retry mechanism if yes go till max
                if (act.Status == eRunStatus.Failed && act.EnableRetryMechanism && act.RetryMechanismCount < act.MaxNumberOfRetries)
                {
                    //since we return and don't do flow control the action is going to run again                
                    //NotifyActionEnd(act); //Needed?
                    return;
                }
                // we capture current activity and action to use it for execution logger,
                // because in DoFlowControl(act) it will point to the other action/activity(as flow control will be applied)
                Activity activity = CurrentBusinessFlow.CurrentActivity;
                Act action = act;

                if (!mErrorPostExecutionActionFlowBreaker)
                {
                    if (handlerPostExecutionAction != eErrorHandlerPostExecutionAction.ContinueFromNextAction)
                    {
                        DoFlowControl(act, moveToNextAction);
                    }
                    else
                    {
                        MoveToNextAction(act);
                    }
                }

                DoStatusConversion(act);   //does it need to be here or earlier?
            }
            finally
            {
                if (actionExecuted)
                {
                    NotifyActionEnd(act);
                }
                CurrentBusinessFlow.PreviousAction = act;
            }
        }

        private ObservableList<ErrorHandler> GetAllErrorHandlersByType(eHandlerType errHandlerType)
        {
            ObservableList<ErrorHandler> lstErrorHandler = new ObservableList<ErrorHandler>(CurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true
              && ((GingerCore.ErrorHandler)a).HandlerType == errHandlerType).Cast<ErrorHandler>().ToList());

            return lstErrorHandler;
        }


        private ObservableList<CleanUpActivity> GetCureentBusinessFlowCleanUpActivities()
        {
            ObservableList<CleanUpActivity> lstErrorHandler = new ObservableList<CleanUpActivity>(CurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(CleanUpActivity) && a.Active == true
              ).Cast<CleanUpActivity>().ToList());

            return lstErrorHandler;
        }

        private ObservableList<ErrorHandler> GetErrorHandlersForCurrentActivity()
        {
            switch (CurrentBusinessFlow.CurrentActivity.ErrorHandlerMappingType)
            {
                case eHandlerMappingType.AllAvailableHandlers:
                    // pass all error handlers, by default
                    return GetAllErrorHandlersByType(eHandlerType.Error_Handler);

                case eHandlerMappingType.SpecificErrorHandlers:
                    // fetch list of all error handlers in the current business flow
                    ObservableList<ErrorHandler> allCurrentBusinessFlowErrorHandlers = GetAllErrorHandlersByType(eHandlerType.Error_Handler);

                    ObservableList<ErrorHandler> specificErrorHandlers = [];
                    foreach (Guid _guid in CurrentBusinessFlow.CurrentActivity.MappedErrorHandlers)
                    {
                        // check if mapped error handlers are PRESENT in the current list of error handlers in the business flow i.e. allCurrentBusinessFlowErrorHandlers (checking for deletion, inactive etc.)
                        if (allCurrentBusinessFlowErrorHandlers.Any(x => x.Guid == _guid)) //.ToList().Exists(x => x.Guid == _guid))
                        {
                            var _activity = CurrentBusinessFlow.Activities.FirstOrDefault(x => x.Guid == _guid) as ErrorHandler;
                            specificErrorHandlers.Add(_activity);
                        }
                    }
                    // pass only specific mapped error handlers present the business flow
                    return specificErrorHandlers;

                case eHandlerMappingType.ErrorHandlersMatchingTrigger:
                    ObservableList<ErrorHandler> errorHandlersMatchingTrigger = [];
                    Act failedAction = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                    ObservableList<ErrorHandler> allErrorHandlers = GetAllErrorHandlersByType(eHandlerType.Error_Handler);

                    foreach (var errHandler in allErrorHandlers)
                    {
                        if (errHandler.TriggerType == eTriggerType.AnyError || errHandler.ErrorStringList.Any(er => !string.IsNullOrEmpty(failedAction.Error) && failedAction.Error.Trim().Contains(er.ErrorString.Trim())))
                        {
                            errorHandlersMatchingTrigger.Add(errHandler);
                        }
                    }

                    return errorHandlersMatchingTrigger;

                default:
                    return [];
            }
        }

        private void UpdateDSReturnValues(Act act)
        {
            try
            {
                if (act.ConfigOutputDS == false)
                {
                    return;
                }

                if (!act.ReturnValues.Any())
                {
                    return;
                }

                var mReturnValues = act.ReturnValues.Where(arc => arc.Active == true);
                if (!mReturnValues.Any())
                {
                    return;
                }

                if (act.DSOutputConfigParams.Count > 0 && (act.OutDataSourceName == null || act.OutDataSourceTableName == null))
                {
                    act.OutDataSourceName = act.DSOutputConfigParams[0].DSName;
                    act.OutDataSourceTableName = act.DSOutputConfigParams[0].DSTable;
                    if (act.DSOutputConfigParams[0].OutParamMap != null)
                    {
                        act.OutDSParamMapType = act.DSOutputConfigParams[0].OutParamMap;
                    }
                    else
                    {
                        act.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                    }
                }

                var mADCS = act.DSOutputConfigParams.Where(arc => arc.DSName == act.OutDataSourceName && arc.DSTable == act.OutDataSourceTableName && arc.Active == true);
                if (!mADCS.Any() && act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToRow.ToString())
                {
                    return;
                }

                DataSourceBase DataSource = null;
                DataSourceTable DataSourceTable = null;
                foreach (DataSourceBase ds in mGingerRunner.DSList)
                {
                    if (ds.Name == act.OutDataSourceName)
                    {
                        DataSource = ds;
                        break;
                    }
                }
                if (DataSource == null)
                {
                    return;
                }

                DataSource.FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DataSource.FilePath);

                ObservableList<DataSourceTable> dstTables = DataSource.GetTablesList();
                foreach (DataSourceTable dst in dstTables)
                {
                    if (dst.Name == act.OutDataSourceTableName)
                    {
                        DataSourceTable = dst;
                        break;
                    }
                }
                if (DataSourceTable == null)
                {
                    return;
                }

                List<string> mColList = DataSourceTable.DSC.GetColumnList(DataSourceTable.Name);
                if (act.OutDSParamMapType == null)
                {
                    act.OutDSParamMapType = act.DSOutputConfigParams[0].OutParamMap;
                }

                //Adding OutDataSurce Param at run time if not exist - Param to Col
                if (act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToCol.ToString())
                {
                    if (act.OutDataSourceName == null || act.OutDataSourceTableName == null)
                    {
                        act.OutDataSourceName = act.DSOutputConfigParams[0].DSName;
                        act.OutDataSourceTableName = act.DSOutputConfigParams[0].DSTable;
                    }

                    var mUniqueRVs = act.ReturnValues.Where(arc => string.IsNullOrEmpty(arc.Path) || arc.Path == "1");
                    // if in output values there is only 1 record with Path = null
                    if (!mUniqueRVs.Any() && act.ReturnValues != null)
                    {
                        mUniqueRVs = act.ReturnValues;
                    }
                    foreach (ActReturnValue item in mUniqueRVs)
                    {
                        mColList.Remove("GINGER_ID");
                        if (mColList.Contains("GINGER_LAST_UPDATED_BY"))
                        {
                            mColList.Remove("GINGER_LAST_UPDATED_BY");
                        }

                        if (mColList.Contains("GINGER_LAST_UPDATE_DATETIME"))
                        {
                            mColList.Remove("GINGER_LAST_UPDATE_DATETIME");
                        }

                        if (mColList.Contains("GINGER_USED"))
                        {
                            mColList.Remove("GINGER_USED");
                        }

                        act.AddOrUpdateOutDataSourceParam(act.OutDataSourceName, act.OutDataSourceTableName, item.Param.Replace(" ", "_"), item.Param.Replace(" ", "_"), "", mColList, act.OutDSParamMapType);
                    }

                    if (act.ConfigOutDSParamAutoCheck)
                    {
                        foreach (var item in act.DSOutputConfigParams)
                        {
                            item.Active = true;
                        }
                    }

                    mADCS = act.DSOutputConfigParams.Where(arc => arc.DSName == act.OutDataSourceName && arc.DSTable == act.OutDataSourceTableName && arc.Active == true && arc.OutParamMap == act.OutDSParamMapType);



                    if (!mADCS.Any())
                    {
                        return;
                    }
                }

                foreach (ActOutDataSourceConfig ADSC in mADCS)
                {
                    if (mColList.Contains(ADSC.TableColumn) == false)
                    {
                        DataSource.AddColumn(DataSourceTable.Name, ADSC.TableColumn, "Text");
                    }
                }
                if (act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToCol.ToString())
                {
                    string sQuery = "";
                    if (DataSourceTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                    {
                        return;
                    }

                    int iPathCount = 0;
                    var mOutRVs = act.ReturnValues.Where(arc => string.IsNullOrEmpty(arc.Path));
                    if (!mOutRVs.Any())
                    {
                        iPathCount++;
                        mOutRVs = act.ReturnValues.Where(arc => arc.Path == iPathCount.ToString());

                        // if in output values there is only 1 record with Path = null
                        if (!mOutRVs.Any() && act.ReturnValues != null)
                        {
                            mOutRVs = act.ReturnValues;
                        }
                    }

                    while (mOutRVs.Any())
                    {
                        string sColList = "";
                        string sColVals = "";
                        foreach (ActOutDataSourceConfig ADSC in mADCS)
                        {
                            var outReturnPath = mOutRVs.FirstOrDefault(arc => arc.Param.Replace(" ", "_") == ADSC.OutputType);
                            if (outReturnPath != null)
                            {
                                sColList = $"{sColList}{ADSC.TableColumn},";
                                string valActual = outReturnPath.Actual == null ? "" : outReturnPath.Actual.ToString();
                                //Replace from value like [,] and [']  as this break liteDB insertion query 

                                sColVals = $"{sColVals}'{valActual.Replace(",", "%2C").Replace("'", "%27").Replace("'", "''")}',";
                            }
                        }
                        if (sColList == "")
                        {
                            break;
                        }

                        sColList = $"{sColList}GINGER_ID,GINGER_USED,";
                        int rowCount = DataSource.GetRowCount(DataSourceTable.Name);
                        sColVals = $"{sColVals}{(rowCount + 1)}, 'False',";
                        sQuery = DataSource.UpdateDSReturnValues(DataSourceTable.Name, sColList, sColVals);

                        //ReplaceBack to value like [,] and [']  
                        DataSource.RunQuery(sQuery.Replace("%2C", ",").Replace("%27", "'"));

                        //Next Path
                        iPathCount++;
                        mOutRVs = act.ReturnValues.Where(arc => arc.Path == iPathCount.ToString());
                    }
                }
                else
                {
                    foreach (ActReturnValue item in act.ReturnValues)
                    {
                        if (item.Active == true)
                        {
                            string sQuery = "";
                            if (DataSourceTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                            {
                                string sKeyName = "";
                                string sKeyValue = "";
                                foreach (ActOutDataSourceConfig ADSC in mADCS)
                                {
                                    if (ADSC.OutputType == "Parameter_Path")
                                    {
                                        if (item.Path is not null and not "")
                                        {
                                            sKeyName = item.Param + "_" + item.Path;
                                        }
                                        else
                                        {
                                            sKeyName = item.Param;
                                        }
                                    }
                                    else if (ADSC.OutputType == "Parameter")
                                    {
                                        sKeyName = item.Param;
                                    }
                                    else
                                    {
                                        sKeyValue = item.Actual;
                                    }

                                    sKeyName = sKeyName.Replace("'", "''");
                                    sKeyValue = sKeyValue == null ? "" : sKeyValue.ToString();
                                    sKeyValue = sKeyValue.Replace("'", "''");
                                }

                                string sColList = "GINGER_ID,GINGER_KEY_NAME,GINGER_KEY_VALUE,";
                                int rowCount = DataSource.GetRowCount(DataSourceTable.Name);
                                string sColVals = (rowCount + 1) + "," + "'" + sKeyName + "','" + sKeyValue + "',";
                                sQuery = DataSource.UpdateDSReturnValues(DataSourceTable.Name, sColList, sColVals);
                            }
                            else
                            {
                                string sColList = "";
                                string sColVals = "";
                                foreach (ActOutDataSourceConfig ADSC in mADCS)
                                {
                                    sColList = sColList + ADSC.TableColumn + ",";
                                    if (ADSC.OutputType == "Parameter")
                                    {
                                        sColVals = sColVals + "'" + item.Param.Replace("'", "''") + "',";
                                    }
                                    else if (ADSC.OutputType == "Path")
                                    {
                                        sColVals = sColVals + "'" + item.Path + "',";
                                    }
                                    else if (ADSC.OutputType == "Actual")
                                    {
                                        string strActual = item.Actual == null ? "" : item.Actual.ToString();
                                        sColVals = sColVals + "'" + strActual.Replace("'", "''") + "',";
                                    }
                                }
                                sColList = sColList + "GINGER_ID,GINGER_USED,";
                                int rowCount = DataSource.GetRowCount(DataSourceTable.Name);
                                sColVals = sColVals + (rowCount + 1) + ",'False',";

                                sQuery = DataSource.UpdateDSReturnValues(DataSourceTable.Name, sColList, sColVals);
                                //sQuery = "INSERT INTO " + DataSourceTable.Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME,GINGER_USED) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "',false)";
                            }
                            DataSource.RunQuery(sQuery);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in UpdateDSReturnValues : ", ex);
            }
        }
        public void ProcessReturnValueForDriver(Act act)
        {
            //Handle all output values, create Value for Driver for each

            foreach (ActReturnValue ARV in act.ActReturnValues)
            {
                ARV.ParamCalculated = act.ValueExpression.Calculate(ARV.Param);
                ARV.PathCalculated = act.ValueExpression.Calculate(ARV.Path);
            }
        }

        public void ProcessInputValueForDriver(Act act)
        {
            //Handle all input values, create Value for Driver for each  
            if (act.ValueExpression == null)
            {
                act.ValueExpression = new ValueExpression(mGingerRunner.ProjEnvironment, CurrentBusinessFlow, mGingerRunner.DSList);
            }
            act.ValueExpression.DecryptFlag = true;
            foreach (var IV in act.InputValues)
            {
                if (!string.IsNullOrEmpty(IV.Value))
                {
                    try
                    {
                        IV.ValueForDriver = act.ValueExpression.Calculate(IV.Value);
                        IV.DisplayValue = act.ValueExpression.EncryptedValue;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to calculate VE for the Action Input value '{0}'", IV.Value), ex);
                    }
                }
                else
                {
                    IV.ValueForDriver = string.Empty;
                }
            }

            //Handle actions which needs VE processing like Tuxedo, we need to calculate the UD file values before execute, which is in different list not in ACT.Input list

            List<ObservableList<ActInputValue>> list = act.GetInputValueListForVEProcessing();
            if (list != null) // Will happen only if derived action implemented this function, since it needs processing for VEs
            {
                if (act is ActWebAPIModel)
                {
                    foreach (var subList in list)
                    {
                        foreach (var IV in subList)
                        {
                            if (!string.IsNullOrEmpty(IV.Value))
                            {
                                try
                                {
                                    string valueToEvaluate = EvaluateWebApiModelParameterValue(IV.Value, subList);
                                    if (!string.IsNullOrEmpty(valueToEvaluate))
                                    {
                                        IV.ValueForDriver = act.ValueExpression.Calculate(valueToEvaluate);
                                    }
                                    else
                                    {
                                        IV.ValueForDriver = act.ValueExpression.Calculate(IV.Value);
                                    }
                                    IV.DisplayValue = act.ValueExpression.EncryptedValue;
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to calculate VE for the Action Input value '{0}'", IV.Value), ex);
                                }
                            }
                            else
                            {
                                IV.ValueForDriver = string.Empty;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var subList in list)
                    {
                        foreach (var IV in subList)
                        {
                            if (!string.IsNullOrEmpty(IV.Value))
                            {
                                try
                                {
                                    IV.ValueForDriver = act.ValueExpression.Calculate(IV.Value);
                                    IV.DisplayValue = act.ValueExpression.EncryptedValue;
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to calculate VE for the Action Input value '{0}'", IV.Value), ex);
                                }
                            }
                            else
                            {
                                IV.ValueForDriver = string.Empty;
                            }
                        }
                    }
                }
            }

            act.ValueExpression.DecryptFlag = true;
        }

        private static string EvaluateWebApiModelParameterValue(string valueToEvaluate, ObservableList<ActInputValue> subList)
        {
            foreach (var item_toCompare in subList)
            {
                if (valueToEvaluate.Contains(item_toCompare.ItemName))
                {
                    if (item_toCompare.ValueForDriver != null)
                    {
                        return valueToEvaluate.Replace(item_toCompare.ItemName, item_toCompare.ValueForDriver);
                    }
                }
            }

            return string.Empty;
        }

        private void ProcessWait(Act act, Stopwatch st)
        {
            string wait = act.ValueExpression.Calculate(act.WaitVE);
            if (!String.IsNullOrEmpty(wait))
            {
                try
                {
                    act.Wait = Int32.Parse(wait);
                }
                catch (System.FormatException ex)
                {
                    act.Wait = 0;
                    act.ExInfo = "Invalid value for Wait time : " + wait;
                    Reporter.ToLog(eLogLevel.WARN, act.ExInfo, ex);
                }
            }
            else
            {
                act.Wait = 0;
            }

            if (act.Wait > 0)
            {
                DateTime startingTime = DateTime.Now;
                while ((DateTime.Now - startingTime).TotalSeconds <= act.Wait)
                {
                    if (mStopRun)
                    {
                        act.ExInfo += "Stopped";
                        return;
                    }
                    UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Wait, st);
                    Thread.Sleep(100);
                }
            }
        }


        private void UpdateActionStatus(Act act, Amdocs.Ginger.CoreNET.Execution.eRunStatus eStatus, Stopwatch st)
        {
            NotifyUpdateActionStatusStart(act);

            act.Status = eStatus;
            act.Elapsed = st.ElapsedMilliseconds;
            act.ElapsedTicks = st.ElapsedTicks;

            NotifyUpdateActionStatusEnd(act);
        }

        private void ExecuteCleanUpActivities()
        {
            ObservableList<CleanUpActivity> cleanUpActivities = GetCureentBusinessFlowCleanUpActivities();

            if (cleanUpActivities.Count > 0)
            {
                foreach (CleanUpActivity cleanUpActivity in cleanUpActivities)
                {
                    CurrentBusinessFlow.CurrentActivity = cleanUpActivity;

                    RunActivity(cleanUpActivity);
                }
            }
        }


        private void ExecuteErrorHandlerActivities(ObservableList<ErrorHandler> errorHandlerActivities)
        {
            Activity originActivity = CurrentBusinessFlow.CurrentActivity;
            handlerPostExecutionAction = eErrorHandlerPostExecutionAction.ReRunOriginAction;

            Reporter.ToLog(eLogLevel.INFO, "--> Error Handlers Execution Started");
            try
            {
                Act orginAction = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                orginAction.ErrorHandlerExecuted = true;
                GingerRunner.eActionExecutorType ActionExecutorType = GingerRunner.eActionExecutorType.RunWithoutDriver;

                var isInErrorList = false;

                foreach (ErrorHandler errActivity in errorHandlerActivities)
                {
                    if (errActivity.TriggerType == eTriggerType.SpecificError || originActivity.ErrorHandlerMappingType == eHandlerMappingType.ErrorHandlersMatchingTrigger)
                    {
                        foreach (var error in errActivity.ErrorStringList.Where(x => x.IsSelected).Select(y => y.ErrorString).ToList())
                        {
                            isInErrorList = CurrentBusinessFlow.ErrorHandlerOriginAction.Error.Contains(error);
                            if (isInErrorList)
                            {
                                break;
                            }
                        }
                        if (!isInErrorList && originActivity.ErrorHandlerMappingType != eHandlerMappingType.ErrorHandlersMatchingTrigger)
                        {
                            continue;
                        }
                    }
                    handlerPostExecutionAction = errActivity.ErrorHandlerPostExecutionAction;
                    CurrentBusinessFlow.CurrentActivity = errActivity;
                    SetCurrentActivityAgent();
                    Stopwatch stE = new Stopwatch();
                    stE.Start();
                    Reporter.ToLog(eLogLevel.INFO, "Error Handler '" + errActivity.ActivityName.ToString() + "' Started");
                    CurrentBusinessFlow.CurrentActivity.StartTimeStamp = DateTime.UtcNow;
                    foreach (Act act in errActivity.Acts)
                    {
                        Stopwatch st = new Stopwatch();
                        st.Start();
                        if (act.Active)
                        {
                            CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;
                            if (errActivity.HandlerType == eHandlerType.Popup_Handler)
                            {
                                act.Timeout = 1;
                            }

                            PrepAction(act, ref ActionExecutorType, st);
                            NotifyActionStart(act);
                            RunActionWithTimeOutControl(act, ActionExecutorType);
                            ProcessStoretoValue(act);
                            UpdateDSReturnValues(act);
                            CalculateActionFinalStatus(act);
                            NotifyActionEnd(act);
                        }
                        st.Stop();
                    }
                    SetBusinessFlowActivitiesAndActionsSkipStatus();
                    CalculateActivityFinalStatus(errActivity);
                    stE.Stop();
                    Reporter.ToLog(eLogLevel.INFO, "Error Handler '" + errActivity.ActivityName.ToString() + "' Ended");
                    CurrentBusinessFlow.CurrentActivity.EndTimeStamp = DateTime.UtcNow;
                    errActivity.Elapsed = stE.ElapsedMilliseconds;
                }

                if (handlerPostExecutionAction is eErrorHandlerPostExecutionAction.ReRunBusinessFlow or
                    eErrorHandlerPostExecutionAction.ReRunOriginActivity or
                    eErrorHandlerPostExecutionAction.ContinueFromNextActivity or
                    eErrorHandlerPostExecutionAction.ContinueFromNextBusinessFlow or
                    eErrorHandlerPostExecutionAction.StopRun)
                {
                    mErrorPostExecutionActionFlowBreaker = true;
                }
                else if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ReRunOriginAction)
                {
                    ResetAction(orginAction);
                    orginAction.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
                    NotifyActionStart(orginAction);
                }
                CurrentBusinessFlow.CurrentActivity = originActivity;
                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = orginAction;

                mCurrentActivityChanged = false;
                SetCurrentActivityAgent();
            }
            finally
            {
                Reporter.ToLog(eLogLevel.INFO, "<-- Error Handlers Execution Ended");
            }
        }

        private void PrepAction(Act action, ref GingerRunner.eActionExecutorType ActExecutorType, Stopwatch st)
        {
            NotifyPrepActionStart(action);
            PrepDynamicVariables();
            NotifyPrepActionEnd(action);

            ResetAction(action);

            PrepActionValueExpression(action);

            ProcessWait(action, st);

            if (mStopRun)
            {
                UpdateActionStatus(action, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped, st);
                ExecutionLoggerManager.SetActionFolder(action);
                //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                SetDriverPreviousRunStoppedFlag(true);
                return;
            }
            else
            {
                //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                SetDriverPreviousRunStoppedFlag(false);
            }
            UpdateActionStatus(action, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Started, st);

            //No need for agent for some actions like DB and read for excel 
            if ((mGingerRunner.RunInSimulationMode) && (action.SupportSimulation))
            {
                ActExecutorType = GingerRunner.eActionExecutorType.RunInSimulationMode;
            }
            else
            {
                if (typeof(ActPlugIn).IsAssignableFrom(action.GetType()))
                {
                    ActExecutorType = GingerRunner.eActionExecutorType.RunOnPlugIn;
                }
                else if (typeof(ActWithoutDriver).IsAssignableFrom(action.GetType()))
                {
                    ActExecutorType = GingerRunner.eActionExecutorType.RunWithoutDriver;
                }
                else
                {
                    ActExecutorType = GingerRunner.eActionExecutorType.RunOnDriver;
                }
            }


            UpdateActionStatus(action, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running, st);
            GiveUserFeedback();


            if (action.Context == null)
            {
                var mContext = new Context() { ExecutedFrom = this.ExecutedFrom };
                action.Context = mContext;
            }
            else
            {
                var mContext = Context.GetAsContext(action.Context);
                mContext.ExecutedFrom = this.ExecutedFrom;
            }

        }

        public void PrepActionValueExpression(Act act, BusinessFlow businessflow = null)
        {
            // We create VE for action only one time here
            ValueExpression VE = null;
            if (businessflow == null)
            {
                VE = new ValueExpression(mGingerRunner.ProjEnvironment, CurrentBusinessFlow, mGingerRunner.DSList);
            }
            else
            {
                VE = new ValueExpression(mGingerRunner.ProjEnvironment, businessflow, mGingerRunner.DSList);
            }

            act.ValueExpression = VE;




            // TODO: remove when we no longer use LocateValue in Action


            if (!string.IsNullOrEmpty(act.GetInputParamValue(Act.Fields.LocateValue)))
            {

                VE.Value = act.LocateValue;
                act.LocateValueCalculated = VE.ValueCalculated;
            }
            ProcessInputValueForDriver(act);
            ProcessReturnValueForDriver(act);


        }

        internal void PrepDynamicVariables()
        {
            IEnumerable<VariableBase> vars = CurrentBusinessFlow.GetAllHierarchyVariables().Where(v => v.GetType() == typeof(VariableDynamic));
            foreach (VariableBase v in vars)
            {
                VariableDynamic vd = (VariableDynamic)v;
                vd.Init(mGingerRunner.ProjEnvironment, CurrentBusinessFlow);
            }
        }

        private void ProcessScreenShot(Act act, GingerRunner.eActionExecutorType ActionExecutorType)
        {

            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
            {
                return;
            }

            // if action failed and user don't want screen shot on failure
            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && !act.AutoScreenShotOnFailure)
            {
                return;
            }

            if (ActionExecutorType == GingerRunner.eActionExecutorType.RunOnDriver)
            {
                if (act.TakeScreenShot || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    string msg;
                    try
                    {
                        if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)
                        {
                            TakeDesktopScreenShotIntoAction(act);
                        }
                        else
                        {
                            ActScreenShot screenShotAction = new ActScreenShot
                            {
                                LocateBy = eLocateBy.NA,
                                WindowsToCapture = act.WindowsToCapture
                            };

                            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                            {
                                screenShotAction.WindowsToCapture = ActScreenShot.eWindowsToCapture.AllAvailableWindows;
                            }

                            Agent a = (Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                            if (a == null)
                            {
                                msg = "Missing Agent for taking screen shot for the action: '" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += msg;
                            }
                            else if (((AgentOperations)a.AgentOperations).Status != Agent.eStatus.Running)
                            {
                                msg = "Screen shot not captured because agent is not running for the action:'" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += msg;
                            }
                            else
                            {
                                if (a.AgentType == Agent.eAgentType.Driver)
                                {
                                    a.AgentOperations.RunAction(screenShotAction);//TODO: Use IVisual driver to get screen shot instead of running action                         
                                    if (string.IsNullOrEmpty(screenShotAction.Error))//make sure the screen shot succeed
                                    {
                                        foreach (string screenShot in screenShotAction.ScreenShots)
                                        {
                                            act.ScreenShots.Add(screenShot);
                                        }
                                        foreach (string screenShotName in screenShotAction.ScreenShotsNames)
                                        {
                                            act.ScreenShotsNames.Add(screenShotName);
                                        }
                                    }
                                    else
                                    {
                                        act.ExInfo += screenShotAction.Error;
                                    }
                                }
                                else if (a.AgentType == Agent.eAgentType.Service)
                                {
                                    ExecuteOnPlugin.ExecutesScreenShotActionOnAgent(a, act);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        msg = "Failed to take driver screen shot for the action: '" + act.Description + "'";
                        Reporter.ToLog(eLogLevel.WARN, msg, ex);
                        act.ExInfo += msg;
                    }
                }
            }
            else if (act.TakeScreenShot && act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                TakeDesktopScreenShotIntoAction(act);
            }
        }

        private void TakeDesktopScreenShotIntoAction(Act act)
        {
            string msg;
            try
            {
                Dictionary<string, String> screenShotsPaths = [];
                screenShotsPaths = TargetFrameworkHelper.Helper.TakeDesktopScreenShot(true);
                if (screenShotsPaths == null)
                {
                    if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)//log the error only if user asked for desktop screen shot to avoid confusion 
                    {
                        msg = "Failed to take desktop screen shot for the action: '" + act.Description + "'";
                        act.ExInfo += msg;
                        return;
                    }
                }
                else if (screenShotsPaths.Keys.Count > 0)
                {
                    foreach (KeyValuePair<string, string> screenShot in screenShotsPaths)
                    {
                        act.ScreenShotsNames.Add(screenShot.Key);
                        act.ScreenShots.Add(screenShot.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)//log the error only if user asked for desktop screen shot to avoid confusion 
                {
                    msg = "Failed to take desktop screen shot for the action: '" + act.Description + "', it might be because the screen is locked.";
                    act.ExInfo += msg;
                    Reporter.ToLog(eLogLevel.WARN, msg, ex);
                }
            }
        }


        private void RunActionWithTimeOutControl(Act act, GingerRunner.eActionExecutorType ActExecutorType)
        {
            try
            {
                //set timeout time
                TimeSpan TS;
                if (CurrentBusinessFlow.CurrentActivity.CurrentAgent == null)
                {
                    if (act.Timeout == null)
                    {
                        // Default Action timeout is 30 mins
                        TS = TimeSpan.FromMinutes(30);
                    }
                    else if (act.Timeout == 0)
                    {
                        // Like no limit
                        TS = TimeSpan.FromDays(1);
                    }
                    else
                    {
                        TS = TimeSpan.FromSeconds((int)act.Timeout);
                    }
                }
                else
                {
                    GingerCore.Drivers.DriverBase driver = ((AgentOperations)((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Driver;
                    if (act.Timeout == null)
                    {
                        if (driver != null && driver.ActionTimeout != -1)
                        {
                            TS = TimeSpan.FromSeconds(driver.ActionTimeout);
                        }
                        else
                        {
                            TS = TimeSpan.FromMinutes(30);
                        }

                    }
                    else if (act.Timeout == 0)
                    {
                        // Like no limit
                        TS = TimeSpan.FromDays(1);
                    }
                    else
                    {
                        TS = TimeSpan.FromSeconds((int)act.Timeout);
                    }
                }

                Agent currentAgent = (Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                bool bCompleted = ExecuteActionWithTimeLimit(act, TS, () =>
                {
                    switch (ActExecutorType)
                    {
                        case GingerRunner.eActionExecutorType.RunOnDriver:
                        {
                            if (currentAgent == null)
                            {
                                if (string.IsNullOrEmpty(act.Error))
                                {
                                    act.Error = "No Agent was found for the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Application.";
                                }

                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            }
                            else
                            {
                                if (((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentType == Agent.eAgentType.Driver)
                                {
                                    if (((AgentOperations)currentAgent.AgentOperations).Status != Agent.eStatus.Running)
                                    {
                                        if (string.IsNullOrEmpty(act.Error))
                                        {
                                            if (((AgentOperations)currentAgent.AgentOperations).Driver != null && !string.IsNullOrEmpty(((AgentOperations)currentAgent.AgentOperations).Driver.ErrorMessageFromDriver))
                                            {
                                                act.Error = ((AgentOperations)currentAgent.AgentOperations).Driver.ErrorMessageFromDriver;
                                            }
                                            else
                                            {
                                                act.Error = $"Agent failed to start for the {GingerDicser.GetTermResValue(eTermResKey.Activity)} Application. Current Agent Status {((AgentOperations)currentAgent.AgentOperations).Status}";
                                            }
                                        }

                                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                    }
                                    else
                                    {
                                        using (IFeatureTracker rodFeatureTracker = Reporter.StartFeatureTracking(FeatureId.ActionExecution))
                                        {
                                            rodFeatureTracker.Metadata.Add("Type", act.GetType().Name);
                                            rodFeatureTracker.Metadata.Add("ExecutorType", GingerRunner.eActionExecutorType.RunOnDriver.ToString());
                                            rodFeatureTracker.Metadata.Add("IsSharedRepositoryInstance", act.IsSharedRepositoryInstance.ToString());
                                            ((AgentOperations)((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).RunAction(act);
                                        }
                                    }
                                }
                                else
                                {

                                    if (act is IActPluginExecution PluginAction)
                                    {

                                        Agent PluginAgent = (Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                                        ExecuteOnPlugin.ExecutePlugInActionOnAgent(PluginAgent, PluginAction);
                                    }

                                    else
                                    {
                                        act.Error = "Current Plugin Agent does not support execution for " + act.ActionDescription;
                                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

                                    }
                                }

                            }
                        }

                        break;

                        case GingerRunner.eActionExecutorType.RunWithoutDriver:
                            using (IFeatureTracker rwdFeatureTracker = Reporter.StartFeatureTracking(FeatureId.ActionExecution))
                            {
                                rwdFeatureTracker.Metadata.Add("Type", act.GetType().Name);
                                rwdFeatureTracker.Metadata.Add("ExecutorType", GingerRunner.eActionExecutorType.RunWithoutDriver.ToString());
                                rwdFeatureTracker.Metadata.Add("IsSharedRepositoryInstance", act.IsSharedRepositoryInstance.ToString());
                                RunWithoutAgent(act);
                            }
                            break;

                        case GingerRunner.eActionExecutorType.RunOnPlugIn:
                            using (IFeatureTracker ropFeatureTracker = Reporter.StartFeatureTracking(FeatureId.ActionExecution))
                            {
                                ropFeatureTracker.Metadata.Add("Type", act.GetType().Name);
                                ropFeatureTracker.Metadata.Add("ExecutorType", GingerRunner.eActionExecutorType.RunOnPlugIn.ToString());
                                ropFeatureTracker.Metadata.Add("IsSharedRepositoryInstance", act.IsSharedRepositoryInstance.ToString());
                                ExecuteOnPlugin.FindNodeAndRunAction((ActPlugIn)act);
                            }

                            break;

                        case GingerRunner.eActionExecutorType.RunInSimulationMode:
                            using (IFeatureTracker risFeatureTracker = Reporter.StartFeatureTracking(FeatureId.ActionExecution))
                            {
                                risFeatureTracker.Metadata.Add("Type", act.GetType().Name);
                                risFeatureTracker.Metadata.Add("ExecutorType", GingerRunner.eActionExecutorType.RunInSimulationMode.ToString());
                                risFeatureTracker.Metadata.Add("IsSharedRepositoryInstance", act.IsSharedRepositoryInstance.ToString());
                                RunActionInSimulationMode(act);
                            }
                            break;
                    }
                }
                );

                if (bCompleted)
                {

                }
                else if (mStopRun)
                {

                }
                else
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Canceling;
                    act.Error += "Timeout Occurred, Elapsed > " + act.Timeout;
                    GiveUserFeedback();
                }

                if (currentAgent != null && ((AgentOperations)currentAgent.AgentOperations).Driver != null)
                {
                    ((AgentOperations)currentAgent.AgentOperations).Driver.ActionCompleted(act);
                }

            }
            catch (Exception e)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.Error = e.Message + Environment.NewLine + e.InnerException;
            }
            finally
            {
                GiveUserFeedback();
            }
        }


        private bool ExecuteActionWithTimeLimit(Act act, TimeSpan timeSpan, Action codeBlock)
        {
            Stopwatch st = Stopwatch.StartNew();

            //TODO: Cancel the task after timeout
            try
            {
                Task task = Task.Factory.StartNew(() =>
                        {
                            codeBlock();
                        }
                    );

                while (!task.IsCompleted && st.ElapsedMilliseconds < timeSpan.TotalMilliseconds && !mStopRun)
                {
                    task.Wait(500);  // Give user feedback every 500ms
                    act.Elapsed = st.ElapsedMilliseconds;
                    GiveUserFeedback();
                }
                bool bCompleted = task.IsCompleted;

                if (mStopRun)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                    act.ExInfo += "Stopped";
                    //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                    //TODO: J.G: Enhance the mechanism to notify the Driver that action is stopped. Today driver still running the action until timeout even after stopping it.
                    SetDriverPreviousRunStoppedFlag(true);
                }
                else
                {
                    if (!bCompleted)
                    {
                        act.Error += "Time out !";
                    }
                    //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                    SetDriverPreviousRunStoppedFlag(false);
                }
                return bCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

        private void RunActionInSimulationMode(Act act, bool checkIfActionAllowedToRun = true)
        {
            if (act.ReturnValues.Count == 0)
            {
                return;
            }

            foreach (ActReturnValue item in act.ActReturnValues)
            {
                if (item.SimulatedActual != null)
                {
                    item.Actual = act.ValueExpression.Calculate(item.SimulatedActual);
                }
            }

            act.ExInfo += "Executed in Simulation Mode";
        }

        public void SetCurrentActivityAgent()
        {
            // We take it based on the Activity target App
            string AppName = CurrentBusinessFlow.CurrentActivity.TargetApplication;
            //For unit test cases, solution applications will be always null
            if (SolutionApplications != null)
            {
                if (SolutionApplications.Any(x => x.AppName == AppName && x.Platform == ePlatformType.NA))
                {
                    return;
                }
            }
            if (string.IsNullOrEmpty(AppName))
            {
                // If we don't have Target App on activity then take first App from BF
                if (CurrentBusinessFlow.TargetApplications.Any())
                {
                    AppName = CurrentBusinessFlow.TargetApplications[0].Name;
                }
            }

            if (string.IsNullOrEmpty(AppName))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"Please select {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} for the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} and {GingerDicser.GetTermResValue(eTermResKey.Activity)}");
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            ApplicationAgent AA = (ApplicationAgent)mGingerRunner.ApplicationAgents.FirstOrDefault(x => x.AppName.Equals(AppName));
            if (AA == null || AA.Agent == null)
            {

                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"The current {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} {AppName}, doesn't have a mapped agent assigned to it");
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            AA.Agent.BusinessFlow = CurrentBusinessFlow;
            AA.Agent.ProjEnvironment = mGingerRunner.ProjEnvironment;
            //check for null agent operations, found it was null in CLI dynamic file case
            if (AA.Agent.AgentOperations == null)
            {
                AA.Agent.AgentOperations = new AgentOperations(AA.Agent);
            }
            // Verify the Agent for the action is running 
            Agent.eStatus agentStatus = ((AgentOperations)AA.Agent.AgentOperations).Status;
            CurrentBusinessFlow.CurrentActivity.CurrentAgent = AA.Agent;
            if (agentStatus is not Agent.eStatus.Running and not Agent.eStatus.Starting and not Agent.eStatus.FailedToStart)
            {
                // start the agent if one of the action s is not subclass of  ActWithoutDriver = driver action
                if (CurrentBusinessFlow.CurrentActivity.Acts.Any(x => typeof(ActWithoutDriver).IsAssignableFrom(x.GetType()) == false))
                {
                    StartAgent(AA.Agent);
                }
            }
        }

        private void ProcessStoretoValue(Act act)
        {
            List<VariableBase> optionalVars = null;
            foreach (ActReturnValue item in act.ReturnValues.Where(x => x.Active == true && string.IsNullOrEmpty(x.StoreToValue) == false))
            {
                bool succeedToPerform = false;
                try
                {
                    switch (item.StoreTo)
                    {
                        case ActReturnValue.eStoreTo.Variable:
                            if (optionalVars == null)
                            {
                                optionalVars = CurrentBusinessFlow.GetAllVariables(CurrentBusinessFlow.CurrentActivity).Where(a => a.SupportSetValue == true).ToList();
                            }
                            VariableBase storeToVar = optionalVars.Find(x => x.Name == item.StoreToValue);
                            if (storeToVar != null)
                            {
                                if (!string.IsNullOrEmpty(storeToVar.LinkedVariableName))
                                {
                                    storeToVar = optionalVars.Find(x => x.Name == storeToVar.LinkedVariableName);
                                }
                                if (storeToVar != null)
                                {
                                    succeedToPerform = storeToVar.SetValue(item.Actual);
                                }
                            }
                            break;

                        case ActReturnValue.eStoreTo.GlobalVariable:
                            VariableBase globalVar = WorkSpace.Instance.Solution.Variables.FirstOrDefault(x => x.SupportSetValue && x.Guid.ToString() == item.StoreToValue);
                            if (globalVar != null)
                            {
                                succeedToPerform = globalVar.SetValue(item.Actual);
                            }
                            break;

                        case ActReturnValue.eStoreTo.ApplicationModelParameter:
                            if (Guid.TryParse(item.StoreToValue, out Guid gampGuid))
                            {
                                GlobalAppModelParameter globalAppModelParameter = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<GlobalAppModelParameter>(gampGuid);
                                if (globalAppModelParameter != null)
                                {
                                    globalAppModelParameter.CurrentValue = item.Actual;
                                    succeedToPerform = true;
                                }
                            }
                            break;

                        case ActReturnValue.eStoreTo.DataSource:
                            ValueExpression VE = new ValueExpression(mGingerRunner.ProjEnvironment, CurrentBusinessFlow, mGingerRunner.DSList, true, item.Actual);
                            VE.Calculate(item.StoreToValue);
                            succeedToPerform = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //Reporter.ToLog(eLogLevel.WARN, "Exception occurred while performing Return Value StoreTo operation", ex);
                    act.ExInfo += string.Format("Return Value StoreTo operation exception details: {0}", ex.Message);
                }

                if (succeedToPerform == false)
                {
                    string msg = string.Format("Failed to perform Return Value StoreTo operation for the '{0}' Return Parameter with value '{1}'--> StoreTo Type: '{2}' and StoreTo Value: '{3}'", item.Param, item.Actual, item.StoreTo, item.StoreToValue);
                    if (string.IsNullOrEmpty(act.Error))
                    {
                        act.Error = msg;
                    }
                    else
                    {
                        act.ExInfo += msg;
                    }
                }
            }
        }

        private void RunWithoutAgent(Act act)
        {
            //TODO: add handling for action time out

            // Remove from here
            //TODO: remove ref to App and Mainwindow + Why to minimize?
            //FIXME: Need to use window API to send click to specific window

            //TODO: fix the action to send to window HWND a click - not to minimize main window

            ActWithoutDriver AWD = (ActWithoutDriver)act;
            AWD.RunOnBusinessFlow = CurrentBusinessFlow;

            AWD.RunOnEnvironment = mGingerRunner.ProjEnvironment;
            // avoid NPE when running UT

            AWD.SolutionFolder = SolutionFolder;
            AWD.DSList = mGingerRunner.DSList;

            try
            {
                AWD.Execute();
            }
            catch (Exception ex)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                if (string.IsNullOrEmpty(act.Error))
                {
                    act.Error = ex.Message;
                }
            }
        }


        private void ResetAction(Act act)
        {
            /*
            if (act.DirtyTracking == Amdocs.Ginger.Common.Enums.eDirtyTracking.Paused)
            {
                act.Reset(isActionDirtyTrackingPaused: true);
            }
            else
            {
                act.Reset();
            }*/
            act.Reset();
        }

        private void DoFlowControl(Act act, bool moveToNextAction = true)
        {
            try
            {
                //TODO: on pass, on fail etc...
                bool isFlowChange = false;

                foreach (FlowControl FC in act.FlowControls)
                {
                    if (FC.Active == false)
                    {
                        FC.Status = eStatus.Skipped;
                        continue;
                    }

                    FC.CalculateCondition(CurrentBusinessFlow, mGingerRunner.ProjEnvironment, act, mLastExecutedActivity, mGingerRunner.DSList);
                    FC.CalcualtedValue(CurrentBusinessFlow, mGingerRunner.ProjEnvironment, mGingerRunner.DSList);

                    bool IsConditionTrue = CalculateFlowControlStatus(act, mLastExecutedActivity, CurrentBusinessFlow, FC.Operator, FC.ConditionCalculated);

                    if (IsConditionTrue)
                    {
                        bool allowInterActivityFlowControls = true;
                        if (WorkSpace.Instance.RunsetExecutor.RunSetConfig != null)
                        {
                            allowInterActivityFlowControls = WorkSpace.Instance.RunsetExecutor.RunSetConfig.AllowInterActivityFlowControls;
                        }
                        //Perform the action as condition is true
                        switch (FC.FlowControlAction)
                        {
                            case eFlowControlAction.MessageBox:
                                string txt = act.ValueExpression.Calculate(FC.Value);
                                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, txt);
                                break;
                            case eFlowControlAction.GoToAction:
                                if (GotoAction(FC, act))
                                {
                                    isFlowChange = true;
                                }
                                else
                                {
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }

                                break;
                            case eFlowControlAction.GoToNextAction:
                                if (FlowControlGotoNextAction(act))
                                {
                                    isFlowChange = true;
                                }
                                else
                                {
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }

                                break;
                            case eFlowControlAction.GoToActivity:
                            case eFlowControlAction.GoToActivityByName:
                                if (allowInterActivityFlowControls && GotoActivity(FC, act))
                                {
                                    isFlowChange = true;
                                }
                                else
                                {
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }

                                break;
                            case eFlowControlAction.GoToNextActivity:
                                if (FlowControlGotoNextActivity(act))
                                {
                                    isFlowChange = true;
                                }
                                else
                                {
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }

                                break;
                            case eFlowControlAction.RerunAction:
                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                                isFlowChange = true;
                                break;
                            case eFlowControlAction.RerunActivity:
                                if (allowInterActivityFlowControls)
                                {
                                    ResetActivity(CurrentBusinessFlow.CurrentActivity);
                                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts[0];
                                    isFlowChange = true;
                                }
                                break;
                            case eFlowControlAction.StopBusinessFlow:
                                if (allowInterActivityFlowControls)
                                {
                                    mStopBusinessFlow = true;
                                    CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                    CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                    isFlowChange = true;
                                }
                                break;
                            case eFlowControlAction.FailActionAndStopBusinessFlow:
                                if (allowInterActivityFlowControls)
                                {
                                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                    act.Error += " Failed due to Flow Control rule";
                                    act.ExInfo += FC.ConditionCalculated;
                                    mStopBusinessFlow = true;
                                    CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                    CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                    isFlowChange = true;
                                }
                                break;
                            case eFlowControlAction.StopRun:
                                if (allowInterActivityFlowControls)
                                {
                                    StopRun();
                                    isFlowChange = true;
                                }
                                break;
                            case eFlowControlAction.SetVariableValue:
                                try
                                {
                                    string[] vals = act.ValueExpression.Calculate(FC.Value).Split(new char[] { '=' });
                                    if (vals.Length == 2)
                                    {
                                        ActSetVariableValue setValueAct = new ActSetVariableValue();
                                        PrepActionValueExpression(setValueAct, CurrentBusinessFlow);
                                        setValueAct.VariableName = vals[0];
                                        setValueAct.SetVariableValueOption = VariableBase.eSetValueOptions.SetValue;
                                        setValueAct.Value = vals[1];
                                        setValueAct.RunOnBusinessFlow = this.CurrentBusinessFlow;
                                        setValueAct.DSList = mGingerRunner.DSList;
                                        setValueAct.Execute();
                                    }
                                    else
                                    {
                                        FC.Status = eStatus.Action_Execution_Failed;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to do Set " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " Value Flow Control", ex);
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }
                                break;
                            case eFlowControlAction.RunSharedRepositoryActivity:
                                try
                                {
                                    if (allowInterActivityFlowControls && RunSharedRepositoryActivity(FC))
                                    {
                                        isFlowChange = true;
                                    }
                                    else
                                    {
                                        FC.Status = eStatus.Action_Execution_Failed;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to do RunSharedRepositoryActivity Flow Control", ex);
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }
                                break;

                            default:
                                //TODO:
                                break;
                        }

                        if (FC.Status == eStatus.Pending)
                        {
                            FC.Status = eStatus.Action_Executed;
                        }
                    }
                    else
                    {
                        FC.Status = eStatus.Action_Not_Executed;
                    }

                    // Go out the foreach in case we have a goto so no need to process the rest of FCs
                    if (isFlowChange)
                    {
                        break;
                    }
                }


                // If all above completed and no change on flow then move to next in the activity unless it is the last one
                if (!isFlowChange && moveToNextAction)
                {
                    MoveToNextAction(act);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in DoFlowControl", ex);
            }
        }

        private void MoveToNextAction(Act act)
        {
            if (!IsLastActionOfActivity())// if running single action we don't want to move to next action
            {
                // if execution has been stopped externally, stop at current action
                if (!mStopRun)
                {
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;
                    GotoNextAction();
                    ((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                }
            }
        }

        public static bool CalculateFlowControlStatus(Act mAct, Activity mLastActivity, BusinessFlow CurrentBF, eFCOperator FCoperator, string Expression)
        {
            bool FCStatus;
            switch (FCoperator)
            {
                case eFCOperator.Legacy:
                    string rc = VBS.ExecuteVBSEval(Expression.Trim());
                    if (rc == "-1")
                    {
                        FCStatus = true;
                    }
                    else
                    {

                        FCStatus = false;
                    }
                    break;

                case eFCOperator.ActionPassed:
                    FCStatus = mAct.Status.Value == eRunStatus.Passed;
                    break;

                case eFCOperator.ActionFailed:
                    FCStatus = mAct.Status.Value == eRunStatus.Failed;
                    break;

                case eFCOperator.LastActivityPassed:
                    if (mLastActivity != null)
                    {
                        FCStatus = mLastActivity.Status == eRunStatus.Passed;
                    }
                    else
                    {
                        FCStatus = false;
                    }
                    break;

                case eFCOperator.LastActivityFailed:
                    if (mLastActivity != null)
                    {
                        FCStatus = mLastActivity.Status == eRunStatus.Failed;
                    }
                    else
                    {
                        FCStatus = false;
                    }
                    break;

                case eFCOperator.BusinessFlowPassed:
                    FCStatus = CurrentBF.RunStatus == eRunStatus.Passed;
                    break;

                case eFCOperator.BusinessFlowFailed:
                    FCStatus = CurrentBF.RunStatus == eRunStatus.Failed;
                    break;

                case eFCOperator.CSharp:
                    FCStatus = CodeProcessor.EvalCondition(Expression);
                    break;

                default:
                    FCStatus = false;
                    break;
            }

            return FCStatus;
        }

        private bool GotoActivity(FlowControl fc, Act act)
        {
            Activity a = CurrentBusinessFlow.GetActivity(fc.GetGuidFromValue(), fc.GetNameFromValue());

            if (a != null)
            {
                CurrentBusinessFlow.CurrentActivity = a;
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                a.Acts.CurrentItem = a.Acts.FirstOrDefault();
                return true;
            }
            else
            {
                act.ExInfo += "GotoActivity Flow Control Failed because the mapped " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " was not found";
                return false;
            }
        }

        private bool GotoAction(FlowControl fc, Act act)
        {
            //Act a = (from a1 in CurrentBusinessFlow.CurrentActivity.Acts where a1.Guid == GUID select a1).FirstOrDefault();
            Act a = (Act)CurrentBusinessFlow.CurrentActivity.GetAct(fc.GetGuidFromValue(), fc.GetNameFromValue());

            if (a != null)
            {
                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = a;
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                return true;
            }
            else
            {
                act.ExInfo += "GotoAction Flow Control failed because the mapped Action was not found.";
                return false;
            }
        }

        public bool GotoNextAction()
        {
            return CurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
        }

        private bool FlowControlGotoNextAction(Act act)
        {
            if (CurrentBusinessFlow.CurrentActivity.Acts.Count - 1 > CurrentBusinessFlow.CurrentActivity.Acts.IndexOf((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem))
            {
                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;
                CurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
                return true;
            }
            else
            {
                act.ExInfo += "GotoNextAction Flow Control Failed because current Action is last one.";
                return false;
            }
        }

        private void GotoNextActivity()
        {
            // to save last executed activity only if its Active            
            CurrentBusinessFlow.CurrentActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
            CurrentBusinessFlow.Activities.MoveNext();
            CurrentBusinessFlow.CurrentActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
        }

        private bool FlowControlGotoNextActivity(Act act)
        {
            if (CurrentBusinessFlow.Activities.Count - 1 > CurrentBusinessFlow.Activities.IndexOf((Activity)CurrentBusinessFlow.Activities.CurrentItem))
            {
                CurrentBusinessFlow.Activities.MoveNext();
                CurrentBusinessFlow.CurrentActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
                return true;
            }
            else
            {
                act.ExInfo += "GotoNextActivity Flow Control Failed because current " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " is last one.";
                return false;
            }
        }

        public void CalculateActivityFinalStatus(Activity a)
        {
            a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;

            //if there is one fail then Activity status is fail
            if (a.Acts.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped))   //
            {
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
            }
            else if (a.Acts.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed))
            {
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            else if (a.Acts.Count > 0 && a.Acts.Count(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked) == a.Acts.Count)
            {
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
            }
            else
            {
                // If we have at least 1 pass then it passed, otherwise will remain Skipped
                if (a.Acts.Any(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed))
                {
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
            }
        }

        private bool RunSharedRepositoryActivity(FlowControl fc)
        {
            //find activity            
            string activityName = fc.GetNameFromValue();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            Activity sharedActivity = activities.FirstOrDefault(x => x.ActivityName.Equals(activityName, StringComparison.CurrentCultureIgnoreCase));

            if (sharedActivity != null)
            {
                //add activity
                Activity sharedActivityInstance = (Activity)sharedActivity.CreateInstance();
                sharedActivityInstance.Active = true;
                sharedActivityInstance.AddDynamicly = true;
                sharedActivityInstance.VariablesDependencies = CurrentBusinessFlow.CurrentActivity.VariablesDependencies;
                eUserMsgSelection userSelection = eUserMsgSelection.None;
                CurrentBusinessFlow.MapTAToBF(userSelection, sharedActivityInstance, WorkSpace.Instance.Solution.ApplicationPlatforms, true);

                int index = CurrentBusinessFlow.Activities.IndexOf(CurrentBusinessFlow.CurrentActivity) + 1;
                ActivitiesGroup activitiesGroup = CurrentBusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID);
                CurrentBusinessFlow.AddActivity(sharedActivityInstance, activitiesGroup, index);
                ApplicationAgent appAgent = (ApplicationAgent)mGingerRunner.ApplicationAgents.FirstOrDefault(x => x.AppName.Equals(sharedActivityInstance.TargetApplication.ToString()
                   ));
                if (appAgent == null)
                {
                    UpdateApplicationAgents();
                }
                NotifyDynamicActivityWasAddedToBusinessflow(CurrentBusinessFlow);

                //set it as next activity to run                                  
                //CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                //sharedActivityInstance.Acts.CurrentItem = sharedActivityInstance.Acts.FirstOrDefault();
                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + fc.GetNameFromValue() + "' was not found in Shared Repository");
                return false;
            }
        }

        public void CalculateActionFinalStatus(Act act)
        {
            // Action pass if no error/exception and Expected = Actual
            if (!string.IsNullOrEmpty(act.Error) || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
            {
                //Adding Act.Error param to store error in Actual or variable
                // If the flow has been stopped , stop adding the list of Return values in the action 
                if (!mStopRun || mStopBusinessFlow)
                {
                    for (int i = 0; i < act.ReturnValues.Count; i++)
                    {
                        ActReturnValue item = act.ReturnValues[i];
                        if (item.Param != null && item.Param.ToUpper() == "ACT.ERROR".ToUpper())
                        {
                            item.Actual = act.Error;
                        }

                        //set status if not already set
                        if (item.Status == ActReturnValue.eStatus.Pending)
                        {
                            if (String.IsNullOrEmpty(item.Expected))
                            {
                                item.Status = ActReturnValue.eStatus.NA;
                            }
                            else
                            {
                                item.Status = ActReturnValue.eStatus.Skipped;
                            }
                        }
                    }
                }

                if (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }

                return;
            }

            //Go over all return value calculate each for final action result status
            foreach (ActReturnValue actReturnValue in act.ReturnValues)
            {
                //if expected is empty then no check and mark it NA
                if (String.IsNullOrEmpty(actReturnValue.Expected))
                {
                    actReturnValue.Status = ActReturnValue.eStatus.NA;
                }
                else if (!actReturnValue.Active)
                {
                    actReturnValue.Status = ActReturnValue.eStatus.Skipped;
                }
                else
                {
                    //get Expected Calculated
                    ValueExpression ve = (ValueExpression)act.ValueExpression;
                    ve.Value = actReturnValue.Expected;
                    //replace {Actual} place holder with real Actual value
                    if (ve.Value.Contains("{Actual}"))
                    {
                        //Replace to 
                        if ((actReturnValue.Actual != null) && Ginger.Utils.StringManager.IsNumeric(actReturnValue.Actual))
                        {
                            ve.Value = ve.Value.Replace("{Actual}", actReturnValue.Actual);
                        }
                        else
                        {
                            ve.Value = ve.Value.Replace("{Actual}", "\"" + actReturnValue.Actual + "\"");
                        }
                    }
                    //calculate the expected value
                    actReturnValue.ExpectedCalculated = ve.ValueCalculated;

                    //calculate Model Parameter expected value
                    CalculateModelParameterExpectedValue(act, actReturnValue);

                    //compare Actual vs Expected (calculated)
                    string ARCError = CalculateARCStatus(actReturnValue);

                    if (actReturnValue.Status == ActReturnValue.eStatus.Failed)
                    {
                        act.Error += ARCError + System.Environment.NewLine;
                    }
                }
            }

            int CountFail = act.ReturnValues.Count(x => x.Status == ActReturnValue.eStatus.Failed);
            if (CountFail > 0)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }

            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
            }
            else if (act.Status is not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
            else if (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
        }

        private void DoStatusConversion(Act act)
        {
            if (act.StatusConverter == eStatusConverterOptions.None)
            {
                return;
            }
            if (act.StatusConverter == eStatusConverterOptions.AlwaysPass)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
            else if (act.StatusConverter == eStatusConverterOptions.IgnoreFail && act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored;
            }
            else if (act.StatusConverter == eStatusConverterOptions.InvertStatus)
            {
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
            }
        }


        //used for Model param - GingerCore.Actions.WebServices.WebAPI.ActWebAPIModel
        private void CalculateModelParameterExpectedValue(Act act, ActReturnValue ARC)
        {
            act.CalculateModelParameterExpectedValue(ARC);


        }

        public static string CalculateARCStatus(ActReturnValue ARC)
        {


            string ErrorInfo;
            if (ARC.Operator == eOperator.Legacy)
            {
                CalculateARCStatusLegacy(ARC);

                string formatedExpectedCalculated = ARC.ExpectedCalculated;
                if (ARC.ExpectedCalculated.Length >= 9 && (ARC.ExpectedCalculated.Substring(ARC.ExpectedCalculated.Length - 9, 9)).Contains("is False"))
                {
                    formatedExpectedCalculated = ARC.ExpectedCalculated.ToString()[..(ARC.ExpectedCalculated.Length - 9)];
                }

                ErrorInfo = "Output Value validation failed for the Parameter '" + ARC.Param + "' , Expected value is " + formatedExpectedCalculated + " while Actual value is '" + ARC.Actual + "'";
            }

            else
            {
                if (string.IsNullOrEmpty(ARC.Actual) && !string.IsNullOrEmpty(ARC.ExpectedCalculated) && ARC.Operator != eOperator.Evaluate)
                {
                    ARC.Status = ActReturnValue.eStatus.Failed;
                    return "Actual or Expected is empty.";
                }

                bool? status = null;


                string Expression = string.Empty;

                switch (ARC.Operator)
                {
                    case eOperator.Contains:
                        status = ARC.Actual.Contains(ARC.ExpectedCalculated);
                        ErrorInfo = string.Format("Validation failed because '{0}' does not contain '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        break;
                    case eOperator.DoesNotContains:
                        status = !ARC.Actual.Contains(ARC.ExpectedCalculated);
                        ErrorInfo = string.Format("Validation failed because '{0}' contains '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        break;
                    case eOperator.Equals:
                        status = string.Equals(ARC.Actual, ARC.ExpectedCalculated);
                        ErrorInfo = string.Format("Validation failed because '{0}' does not equal '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        break;
                    case eOperator.Evaluate:
                        Expression = ARC.ExpectedCalculated;
                        ErrorInfo = "Validation failed because expression does not evaluate to 'true'.";
                        break;
                    case eOperator.GreaterThan:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Validation failed because both Actual and Expected values must be numeric.";
                        }
                        else
                        {
                            Expression = ARC.Actual + ">" + ARC.ExpectedCalculated;
                            ErrorInfo = string.Format("Validation failed because '{0}' is not greater than '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        }
                        break;
                    case eOperator.GreaterThanEquals:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Validation failed because both Actual and Expected values must be numeric.";
                        }
                        else
                        {
                            Expression = ARC.Actual + ">=" + ARC.ExpectedCalculated;
                            ErrorInfo = string.Format("Validation failed because '{0}' is neither greater than nor equal to '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        }
                        break;
                    case eOperator.LessThan:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Validation failed because both Actual and Expected values must be numeric.";
                        }
                        else
                        {
                            Expression = ARC.Actual + "<" + ARC.ExpectedCalculated;
                            ErrorInfo = string.Format("Validation failed because '{0}' is not less than '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        }
                        break;
                    case eOperator.LessThanEquals:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Validation failed because both Actual and Expected values must be numeric.";
                        }
                        else
                        {
                            Expression = ARC.Actual + "<=" + ARC.ExpectedCalculated;
                            ErrorInfo = string.Format("Validation failed because '{0}' is neither less than nor equal to '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        }
                        break;
                    case eOperator.NotEquals:
                        status = !string.Equals(ARC.Actual, ARC.ExpectedCalculated);
                        ErrorInfo = string.Format("Validation failed because '{0}' equals '{1}'.", ARC.Actual, ARC.ExpectedCalculated);
                        break;
                    default:
                        ErrorInfo = "Unsupported Operation!";
                        break;

                }
                if (status == null)
                {

                    status = CodeProcessor.EvalCondition(Expression);
                }


                if (status.Value)
                {
                    ARC.Status = ActReturnValue.eStatus.Passed;
                }
                else
                {
                    ARC.Status = ActReturnValue.eStatus.Failed;
                }

            }

            return ErrorInfo;
        }

        private static bool CheckIfValuesCanbecompared(string actual, string Expected)
        {
            try
            {
                double.Parse(actual);
                double.Parse(actual);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static void CalculateARCStatusLegacy(ActReturnValue ARC)
        {
            //TODO: Check Expected null or empty return with no change

            //check basic compare - most cases
            if (ARC.ExpectedCalculated == ARC.Actual)
            {
                ARC.Status = ActReturnValue.eStatus.Passed;
                return;
            }
            // To check Whether actual and expected are both NULL 
            if (String.IsNullOrEmpty(ARC.ExpectedCalculated) && String.IsNullOrEmpty(ARC.Actual))
            {
                ARC.Status = ActReturnValue.eStatus.Passed;
                return;
            }

            //TODO: document in help, maybe remove this compare takes time and not sure if needed/use case!?
            if (ARC.ExpectedCalculated.StartsWith("{Regex="))
            {
                string ExpectedRegex = ARC.ExpectedCalculated.Replace("{Regex=", "");
                ExpectedRegex = ExpectedRegex.Trim();
                if (ExpectedRegex.EndsWith("}"))
                {
                    ExpectedRegex = ExpectedRegex[..^1];
                }

                Regex rg = new Regex(ExpectedRegex);
                if (rg.IsMatch(ARC.Actual))
                {
                    ARC.Status = ActReturnValue.eStatus.Passed;
                    return;
                }
                else
                {
                    ARC.Status = ActReturnValue.eStatus.Failed;
                    return;
                }
            }

            if (ARC.ExpectedCalculated.ToUpper().Trim() == "TRUE" && string.IsNullOrEmpty(ARC.Actual))
            {
                ARC.Status = ActReturnValue.eStatus.Failed;
                return;
            }

            else if (ARC.ExpectedCalculated.ToUpper().Trim() == "TRUE" && ARC.Actual.ToUpper().Trim() != "TRUE")
            {
                ARC.Status = ActReturnValue.eStatus.Failed;
                return;
            }
            else
            {
                //do VBS compare for conditions like "7 > 5"
                bool b = EvalExpectedWithActual(ARC);
                if (b)
                {
                    ARC.Status = ActReturnValue.eStatus.Passed;
                    return;
                }
                else
                {
                    ARC.Status = ActReturnValue.eStatus.Failed;
                    return;
                }
            }
        }

        public static void ReplaceActualPlaceHolder(ActReturnValue ARC)//currently used only for unit tests
        {
            string sEval;
            if (Ginger.Utils.StringManager.IsNumeric(ARC.Actual))
            {
                sEval = ARC.ExpectedCalculated.Replace("{Actual}", ARC.Actual);
            }
            else
            {
                sEval = ARC.ExpectedCalculated.Replace("{Actual}", "\"" + ARC.Actual + "\"");
            }
            ARC.ExpectedCalculated = sEval;
            ARC.ExpectedCalculatedValue = ARC.ExpectedCalculated;
        }

        public static bool EvalExpectedWithActual(ActReturnValue ARC)
        {
            string rc = "";
            if (ARC.ExpectedCalculated.ToUpper().StartsWith("NOT "))
            {
                rc = VBS.ExecuteVBSEval("'" + ARC.ExpectedCalculated + "'");
            }
            else
            {
                rc = VBS.ExecuteVBSEval(ARC.ExpectedCalculated);
            }

            if (rc == "-1")
            {
                ARC.ExpectedCalculated = "'" + ARC.ExpectedCalculated + "' is True";
                return true;
            }
            else
            {
                ARC.ExpectedCalculated = "'" + ARC.ExpectedCalculated + "' is False";
                return false;
            }
        }

        public async Task<int> RunActivityAsync(Activity activity, bool Continue = false, bool standaloneExecution = false, bool resetErrorHandlerExecutedFlag = false)
        {
            NotifyExecutionContext(AutomationTabContext.ActivityRun);
            var result = await Task.Run(() =>
            {
                RunActivity(activity, false, standaloneExecution, resetErrorHandlerExecutedFlag);
                return 1;
            });
            return result;
        }

        private void SetMappedValuesToActivityVariables(Activity activity, Activity[] prevActivities)
        {
            if (prevActivities.Length == 0)
            {
                return;
            }

            IEnumerable<VariableBase> activityVariables = activity.GetVariables();
            if (!activityVariables.Any())
            {
                return;
            }

            VariableBase[] mappedTargetVars = activityVariables
                .Where(var => var.MappedOutputType == VariableBase.eOutputType.ActivityOutputVariable)
                .ToArray();

            if (mappedTargetVars.Length == 0)
            {
                return;
            }

            Reporter.ToLog(eLogLevel.INFO, $"Mapping {GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.Variables)} with customized values.");

            foreach (VariableBase mappedTargetVar in mappedTargetVars)
            {
                if (!Guid.TryParse(mappedTargetVar.MappedOutputValue, out Guid mappedSourceVarGuid))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Value '{mappedTargetVar.MappedOutputValue}' is not a valid GUID for mapping input {GingerDicser.GetTermResValue(eTermResKey.Variable)}.");
                    continue;
                }

                Activity mappedSourceActivity = prevActivities
                    .FirstOrDefault(prevActivity => prevActivity.Guid == mappedTargetVar.VariableReferenceEntity);

                if (mappedSourceActivity == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"No Activity('{mappedTargetVar.VariableReferenceEntity}') found by id in {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} before current {GingerDicser.GetTermResValue(eTermResKey.Activity)}({activity.Guid}-{activity.ActivityName}).");
                    continue;
                }

                VariableBase mappedSourceVar = mappedSourceActivity
                    .GetVariables()
                    .Where(var => var.SetAsOutputValue)
                    .FirstOrDefault(var => var.Guid == mappedSourceVarGuid);

                if (mappedSourceVar == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"No {GingerDicser.GetTermResValue(eTermResKey.Variable)}('{mappedSourceVarGuid}') found by id in {GingerDicser.GetTermResValue(eTermResKey.Activity)}('{mappedSourceActivity.Guid}-{mappedSourceActivity.ActivityName}') for mapping it's output value.");
                    continue;
                }

                Reporter.ToLog(eLogLevel.INFO, $"Setting value '{mappedSourceVar.Value}' from {GingerDicser.GetTermResValue(eTermResKey.Variable)}({mappedSourceVar.Guid}-{mappedSourceVar.Name}) to {GingerDicser.GetTermResValue(eTermResKey.Variable)}({mappedTargetVar.Guid}-{mappedTargetVar.Name}).");

                bool wasValueSet = mappedTargetVar.SetValue(mappedSourceVar.Value);
                if (!wasValueSet)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to set value '{mappedSourceVar.Value}' to {GingerDicser.GetTermResValue(eTermResKey.Variable)}({mappedTargetVar.Guid}-{mappedTargetVar.Name})");
                }
            }
        }


        public void RunActivity(Activity activity, bool doContinueRun = false, bool standaloneExecution = false, bool resetErrorHandlerExecutedFlag = false)
        {
            bool activityStarted = false;
            bool statusCalculationIsDone = false;
            ActivitiesGroup currentActivityGroup = null;
            Act act = null;
            Stopwatch st = new Stopwatch();

            //set Runner details if running in stand alone mode (Automate tab)
            if (standaloneExecution)
            {
                IsRunning = true;
                mStopRun = false;
            }

            try
            {
                activity.ExecutionParentGuid = CurrentBusinessFlow.InstanceGuid;
                if (activity.Active != false)
                {
                    //check if Activity is allowed to run
                    if (CurrentBusinessFlow == null ||
                        activity.Acts.Count == 0 || //no Actions to run
                            activity.GetType() == typeof(ErrorHandler) ||//don't run error handler from RunActivity                            
                                activity.CheckIfVaribalesDependenciesAllowsToRun(CurrentBusinessFlow, true) == false || //Variables-Dependencies not allowing to run
                                    (mGingerRunner.FilterExecutionByTags == true && CheckIfActivityTagsMatch() == false))//add validation for Ginger runner tags
                    {
                        CalculateActivityFinalStatus(activity);
                        return;
                    }

                    // handling ActivityGroup execution
                    currentActivityGroup = CurrentBusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid));
                    if (currentActivityGroup != null)
                    {
                        currentActivityGroup.ExecutionParentGuid = CurrentBusinessFlow.InstanceGuid;
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case executionLoggerStatus.NotStartedYet:
                                currentActivityGroup.ExecutionLoggerStatus = executionLoggerStatus.StartedNotFinishedYet;
                                NotifyActivityGroupStart(currentActivityGroup);
                                break;
                            case executionLoggerStatus.StartedNotFinishedYet:
                                // do nothing
                                break;
                            case executionLoggerStatus.Finished:
                                // do nothing
                                break;
                        }
                    }

                    //add validation for Ginger runner tags
                    if (mGingerRunner.FilterExecutionByTags)
                    {
                        if (CheckIfActivityTagsMatch() == false)
                        {
                            return;
                        }
                    }

                    if (!doContinueRun)
                    {
                        // We reset the activity unless we are in continue mode where user can start from middle of Activity
                        ResetActivity(CurrentBusinessFlow.CurrentActivity, resetErrorHandlerExecutedFlag);
                    }
                    else
                    {
                        // since we are in continue mode - only for first activity of continue mode
                        // Just change the status to Pending
                        CurrentBusinessFlow.CurrentActivity.Status = eRunStatus.Pending;

                        ContinueTimerVariables(CurrentBusinessFlow.CurrentActivity.Variables);
                    }

                    //Do not disable the following two lines. these helping the FC run proper activities
                    CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                    CurrentBusinessFlow.PropertyChanged += CurrentBusinessFlow_PropertyChanged;

                    mStopRun = false;//needed with out check for standalone execution???
                    mStopBusinessFlow = false;
                    mCurrentActivityChanged = false;

                    //Run the Activity

                    using (IFeatureTracker featureTracker = Reporter.StartFeatureTracking(FeatureId.ActivityExecution))
                    {
                        try { featureTracker.Metadata.Add("Platform", activity.TargetApplicationPlatformName); }
                        catch { }
                        featureTracker.Metadata.Add("IsSharedRepositoryInstance", activity.IsSharedRepositoryInstance.ToString());

                        activityStarted = true;
                        CurrentBusinessFlow.CurrentActivity.Status = eRunStatus.Running;
                        if (doContinueRun)
                        {
                            NotifyActivityStart(CurrentBusinessFlow.CurrentActivity, doContinueRun);
                        }
                        else
                        {
                            NotifyActivityStart(CurrentBusinessFlow.CurrentActivity);
                        }

                        if (SolutionApplications != null && !SolutionApplications.Any(x => (x.AppName == activity.TargetApplication && x.Platform == ePlatformType.NA)))
                        {
                            //load Agent only if Activity includes Actions which needs it
                            var driverActs = activity.Acts.Where(x => (x is ActWithoutDriver && x.GetType() != typeof(ActAgentManipulation)) == false && x.Active == true);
                            if (driverActs.Any())
                            {
                                //make sure not running in Simulation mode
                                if (!mGingerRunner.RunInSimulationMode ||
                                    (mGingerRunner.RunInSimulationMode == true && driverActs.Any(x => x.SupportSimulation == false)))
                                {
                                    //Set the Agent to run actions with  
                                    SetCurrentActivityAgent();
                                }
                            }
                        }

                        activity.ExecutionLogActionCounter = 0;

                        //Run the Activity Actions
                        st.Start();

                        // if it is not continue mode then goto first Action
                        if (!doContinueRun)
                        {
                            act = (Act)activity.Acts.FirstOrDefault();
                        }
                        else
                        {
                            act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                        }

                        bool bHasMoreActions = true;

                        if (mRunSource == null)
                        {
                            mRunSource = eRunSource.Activity;
                        }

                        while (bHasMoreActions)
                        {
                            CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;

                            GiveUserFeedback();
                            if (act.Active && act.CheckIfVaribalesDependenciesAllowsToRun(activity, true) == true && CheckRunInVisualTestingMode(act))
                            {
                                RunAction(act, false);
                                GiveUserFeedback();
                                if (mCurrentActivityChanged)
                                {
                                    CurrentBusinessFlow.CurrentActivity.Elapsed = st.ElapsedMilliseconds;
                                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                                    {
                                        activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

                                        if (activity.ActionRunOption == eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
                                        {
                                            SetNextActionsBlockedStatus();
                                            statusCalculationIsDone = true;
                                            return;
                                        }
                                    }
                                    break;
                                }
                                CurrentBusinessFlow.CurrentActivity.Elapsed = st.ElapsedMilliseconds;
                                // This Sleep is needed!
                                Thread.Sleep(1);
                                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                                {
                                    activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                    CurrentBusinessFlow.LastFailedAction = act;
                                    if (activity.ActionRunOption == eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
                                    {
                                        SetNextActionsBlockedStatus();
                                        statusCalculationIsDone = true;
                                        return;
                                    }
                                }
                                GiveUserFeedback();
                                // If the user selected slower speed then do wait
                                if (mGingerRunner.AutoWait > 0)
                                {
                                    // TODO: sleep 100 and do events
                                    Thread.Sleep(mGingerRunner.AutoWait * 1000);
                                }

                                if (mStopRun || mStopBusinessFlow)
                                {
                                    mExecutedActionWhenStopped = act;
                                    CalculateActivityFinalStatus(activity);
                                    statusCalculationIsDone = true;
                                    return;
                                }

                                if (mErrorPostExecutionActionFlowBreaker)
                                {
                                    break;
                                }
                            }
                            else
                            {
                                if (!act.Active)
                                {
                                    SkipActionAndNotifyEnd(act);
                                    act.ExInfo = "Action is not active.";
                                }
                                if (!CheckRunInVisualTestingMode(act))
                                {
                                    SkipActionAndNotifyEnd(act);
                                    act.ExInfo = "Visual Testing Action Run Mode is Inactive.";
                                }
                                if (!activity.Acts.IsLastItem())
                                {
                                    GotoNextAction();
                                    ((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                                }
                            }

                            act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                            // As long as we have more action we keep the loop, until no more actions available
                            if (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending && activity.Acts.IsLastItem())
                            {
                                bHasMoreActions = false;
                            }
                        }
                    }
                }
                else
                {
                    activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
            catch (Exception ex)
            {
                if (act != null)
                {
                    act.Error += ex.Message;
                    CalculateActionFinalStatus(act);
                    if (!activity.Acts.IsLastItem())
                    {
                        GotoNextAction();
                        SetNextActionsBlockedStatus();
                    }
                }

                //TODO: Throw exception don't cover in log, so user will see it in report
                Reporter.ToLog(eLogLevel.ERROR, "Run Activity got error ", ex);
                throw ex;
            }
            finally
            {
                if (activityStarted)
                {
                    st.Stop();
                    activity.Elapsed = st.ElapsedMilliseconds;
                    if (!statusCalculationIsDone)
                    {
                        CalculateActivityFinalStatus(activity);
                    }
                    PostScopeVariableHandling(activity.Variables);

                    NotifyActivityEnd(activity);

                    mLastExecutedActivity = activity;
                    CurrentBusinessFlow.PreviousActivity = activity;
                    GiveUserFeedback();

                    // handling ActivityGroup execution 
                    if (currentActivityGroup != null)
                    {
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case executionLoggerStatus.NotStartedYet:
                                // do nothing
                                break;
                            case executionLoggerStatus.StartedNotFinishedYet:
                                uint eventTime = RunListenerBase.GetEventTime();
                                if (currentActivityGroup.ExecutedActivities.ContainsKey(activity.Guid))
                                {
                                    currentActivityGroup.ExecutedActivities[activity.Guid] = eventTime;
                                }
                                else
                                {
                                    currentActivityGroup.ExecutedActivities.Add(activity.Guid, eventTime);
                                }
                                // do nothing
                                break;
                            case executionLoggerStatus.Finished:
                                // do nothing
                                break;
                        }
                    }
                }

                if (standaloneExecution)
                {
                    IsRunning = false;
                }

                if (mErrorPostExecutionActionFlowBreaker && handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ReRunOriginActivity)
                {
                    CheckAndExecutePostErrorHandlerAction();
                }

                if (mRunSource == eRunSource.Activity)
                {
                    mRunSource = null;
                    mErrorPostExecutionActionFlowBreaker = false;
                }
            }
        }

        private void SkipActionAndNotifyEnd(Act act)
        {
            ResetAction(act);
            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == DataRepositoryMethod.LiteDB)
            {
                NotifyActionEnd(act);
            }
        }

        private bool CheckRunInVisualTestingMode(Act act)
        {
            if ((act is ActVisualTesting) && !mGingerRunner.RunInVisualTestingMode)
            {
                return false;
            }
            else { return true; }
        }


        private void ContinueTimerVariables(ObservableList<VariableBase> variableList)
        {
            if (variableList == null || variableList.Count == 0)
            {
                return;
            }

            foreach (VariableBase variable in variableList)
            {
                if (variable.GetType() == typeof(VariableTimer) && ((VariableTimer)variable).IsStopped)
                {
                    ((VariableTimer)variable).ContinueTimer();
                }
            }
        }
        private void PostScopeVariableHandling(ObservableList<VariableBase> variableList)
        {
            try
            {
                if (variableList == null || variableList.Count == 0)
                {
                    return;
                }

                foreach (VariableBase variable in variableList)
                {
                    if (variable.GetType() == typeof(VariableTimer))
                    {
                        if (((VariableTimer)variable).RunWatch.IsRunning)
                        {
                            ((VariableTimer)variable).StopTimer();
                            ((VariableTimer)variable).IsStopped = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Post Scope variable Handling", ex);
            }

        }


        private void CurrentBusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentActivity")
            {
                mCurrentActivityChanged = true;
            }

        }

        private bool IsLastActionOfActivity()
        {
            if (CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem != CurrentBusinessFlow.CurrentActivity.Acts[^1])
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<int> ContinueRunAsync(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null)
        {
            var result = await Task.Run(() =>
            {
                ContinueRun(continueLevel, continueFrom, specificBusinessFlow, specificActivity, specificAction);
                return 1;
            });
            return result;
        }
        public void ResetStatus(eContinueLevel continueLevel, GingerRunner.eResetStatus resetFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null)
        {
            ResetRunStatus(continueLevel, resetFrom, specificBusinessFlow, specificActivity, specificAction);
        }

        private bool ResetRunStatus(eContinueLevel continueLevel, GingerRunner.eResetStatus resetFrom, BusinessFlow specificBusinessFlow, Activity specificActivity, Act specificAction)
        {
            switch (resetFrom)
            {

                case GingerRunner.eResetStatus.All:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.Reset();
                    break;

                case GingerRunner.eResetStatus.FromSpecificActivityOnwards:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = specificActivity;
                    bool continueToReset = false;
                    foreach (Activity activity in CurrentBusinessFlow.Activities)
                    {
                        if (activity.Equals(specificActivity))
                        {
                            continueToReset = true;
                        }
                        if (continueToReset)
                        {
                            activity.Reset();
                        }
                    }
                    break;

                case GingerRunner.eResetStatus.FromSpecificActionOnwards:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = specificActivity;
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = specificAction;
                    bool resetFromActivity = false, resetFromAction = false;
                    foreach (Activity activity in CurrentBusinessFlow.Activities)
                    {
                        if (activity.Equals(specificActivity))
                        {
                            resetFromActivity = true;
                            foreach (Act act in activity.Acts)
                            {
                                if (act.Equals(specificAction))
                                {
                                    resetFromAction = true;
                                }
                                if (resetFromAction)
                                {
                                    act.Reset();
                                }
                            }
                            activity.Elapsed = null;
                            activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                            continue;
                        }
                        if (resetFromActivity && resetFromAction)
                        {
                            activity.Reset();
                        }
                    }
                    break;
            }
            return true;
        }

        public bool ContinueRun(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, IAct specificAction = null)
        {
            try
            {
                switch (continueFrom)
                {
                    case eContinueFrom.LastStoppedAction:
                        if (mExecutedBusinessFlowWhenStopped != null && BusinessFlows.Contains(mExecutedBusinessFlowWhenStopped))
                        {
                            CurrentBusinessFlow = mExecutedBusinessFlowWhenStopped;
                        }
                        else
                        {
                            return false;//can't do continue
                        }

                        if (mExecutedActivityWhenStopped != null && mExecutedBusinessFlowWhenStopped.Activities.Contains(mExecutedActivityWhenStopped))
                        {
                            CurrentBusinessFlow.CurrentActivity = mExecutedActivityWhenStopped;
                            CurrentBusinessFlow.ExecutionLogActivityCounter--;
                        }
                        else
                        {
                            return false;//can't do continue
                        }

                        if (mExecutedActionWhenStopped != null && mExecutedActivityWhenStopped.Acts.Contains(mExecutedActionWhenStopped))
                        {
                            CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = mExecutedActionWhenStopped;
                        }
                        else
                        {
                            return false;//can't do continue
                        }

                        break;

                    case eContinueFrom.SpecificBusinessFlow:
                        CurrentBusinessFlow = specificBusinessFlow;
                        CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.FirstOrDefault();
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.FirstOrDefault();
                        break;

                    case eContinueFrom.SpecificActivity:
                        CurrentBusinessFlow = specificBusinessFlow;
                        CurrentBusinessFlow.CurrentActivity = specificActivity;
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = specificActivity.Acts.FirstOrDefault();
                        break;

                    case eContinueFrom.SpecificAction:
                        CurrentBusinessFlow = specificBusinessFlow;
                        CurrentBusinessFlow.CurrentActivity = specificActivity;
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = specificAction;
                        break;
                }

                if (continueLevel == eContinueLevel.Runner)
                {
                    RunRunner(true);
                }
                else
                {
                    RunBusinessFlow(null, true, true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Continue run", ex);
                return false;
            }
        }

        public async Task<int> RunBusinessFlowAsync(BusinessFlow businessFlow, bool standaloneBfExecution = false, bool doContinueRun = false)
        {
            NotifyExecutionContext(AutomationTabContext.BussinessFlowRun);
            var result = await Task.Run(() =>
            {
                RunBusinessFlow(businessFlow, standaloneBfExecution, doContinueRun, doResetErrorHandlerExecutedFlag: false);
                return 1;
            });
            return result;
        }

        public void RunBusinessFlow(BusinessFlow businessFlow, bool standaloneExecution = false, bool doContinueRun = false, bool doResetErrorHandlerExecutedFlag = false)
        {
            // !!
            // !!!!!!!!!! remove SW
            Stopwatch st = new Stopwatch();
            try
            {
                //set Runner details if running in stand alone mode (Automate tab)
                if (standaloneExecution)
                {
                    IsRunning = true;
                    mStopRun = false;
                }



                //set the BF to execute
                if (businessFlow != null)
                {
                    businessFlow.ExecutionParentGuid = this.GingerRunner.Guid;
                }
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow = businessFlow;
                    Activity bfFirstActivity = CurrentBusinessFlow.Activities.FirstOrDefault();
                    CurrentBusinessFlow.Activities.CurrentItem = bfFirstActivity;
                    CurrentBusinessFlow.CurrentActivity = bfFirstActivity;
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = bfFirstActivity.Acts.FirstOrDefault();
                }


                if (doContinueRun)
                {
                    ContinueTimerVariables(CurrentBusinessFlow.Variables);
                    if (standaloneExecution)
                    {
                        ContinueTimerVariables(BusinessFlow.SolutionVariables);
                    }
                }

                CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;

                if (!doContinueRun)
                {
                    NotifyBusinessFlowStart(CurrentBusinessFlow);
                }
                else
                {
                    NotifyBusinessFlowStart(CurrentBusinessFlow, doContinueRun);
                }

                //Do run preparations                
                UpdateLastExecutingAgent();
                CurrentBusinessFlow.Environment = mGingerRunner.ProjEnvironment == null ? "" : mGingerRunner.ProjEnvironment.Name;
                if (PrepareVariables() == false)
                {
                    if (CurrentBusinessFlow.Activities.Any())
                    {
                        NotifyActivityGroupStart(CurrentBusinessFlow.ActivitiesGroups[0]);
                        NotifyActivityStart(CurrentBusinessFlow.Activities[0]);
                        CurrentBusinessFlow.Activities[0].Status = eRunStatus.Failed;
                        if (CurrentBusinessFlow.Activities[0].Acts.Any())
                        {
                            NotifyActionStart((Act)CurrentBusinessFlow.Activities[0].Acts[0]);
                            CurrentBusinessFlow.Activities[0].Acts[0].Error = string.Format("Error occurred in Input {0} values setup, please make sure all configured {1} Input {0} Mapped Values are valid", GingerDicser.GetTermResValue(eTermResKey.Variables), GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                            CurrentBusinessFlow.Activities[0].Acts[0].Status = eRunStatus.Failed;
                            NotifyActionEnd((Act)CurrentBusinessFlow.Activities[0].Acts[0]);
                        }
                        NotifyActivityEnd(CurrentBusinessFlow.Activities[0]);
                        NotifyActivityGroupEnd(CurrentBusinessFlow.ActivitiesGroups[0]);
                    }
                    return;//failed to prepare BF inputes as expected
                }
                mStopBusinessFlow = false;
                CurrentBusinessFlow.Elapsed = null;
                st.Start();

                //Do Run validations
                if (!CurrentBusinessFlow.Activities.Any())
                {
                    return;//no Activities to run
                }

                //Start execution
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow.ExecutionLogActivityCounter = 1;
                }

                //Executing the Activities
                Activity ExecutingActivity = CurrentBusinessFlow.CurrentActivity;
                Activity FirstExecutedActivity = ExecutingActivity;

                if (mRunSource == null)
                {
                    mRunSource = eRunSource.BusinessFlow;
                }

                List<Activity> previouslyExecutedActivities = [];

                while (ExecutingActivity != null)
                {
                    if (ExecutingActivity is CleanUpActivity)
                    {
                        if (!CurrentBusinessFlow.Activities.IsLastItem())
                        {
                            GotoNextActivity();
                            ExecutingActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
                            continue;
                        }
                        else
                        {
                             ExecutingActivity = null;
                            break;
                        }
                    }
                    ExecutingActivity.Status = eRunStatus.Running;
                    GiveUserFeedback();
                    SetMappedValuesToActivityVariables(ExecutingActivity, previouslyExecutedActivities.ToArray());
                    if (doContinueRun && FirstExecutedActivity.Equals(ExecutingActivity))
                    {
                        // We run the first Activity in Continue mode, if it came from RunFlow, then it is set to first action
                        RunActivity(ExecutingActivity, true, resetErrorHandlerExecutedFlag: doResetErrorHandlerExecutedFlag);
                    }
                    else
                    {
                        RunActivity(ExecutingActivity, resetErrorHandlerExecutedFlag: doResetErrorHandlerExecutedFlag);
                    }
                    previouslyExecutedActivities.Add(ExecutingActivity);
                    //TODO: Why this is here? do we need to rehook
                    CurrentBusinessFlow.PropertyChanged -= CurrentBusinessFlow_PropertyChanged;
                    if (ExecutingActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        CurrentBusinessFlow.LastFailedActivity = ExecutingActivity;
                    }
                    if (ExecutingActivity.Mandatory && ExecutingActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        //CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                        CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                        if (!CurrentBusinessFlow.Activities.IsLastItem())
                        {
                            GotoNextActivity();
                            SetNextActivitiesBlockedStatus();
                        }
                        return;
                    }

                    if (mStopRun || mStopBusinessFlow)
                    {
                        //CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                        SetBusinessFlowActivitiesAndActionsSkipStatus();
                        SetActivityGroupsExecutionStatus();
                        CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                        return;
                    }

                    if (mErrorPostExecutionActionFlowBreaker)
                    {
                        if (handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ContinueFromNextActivity)
                        {
                            mErrorPostExecutionActionFlowBreaker = false;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if ((Activity)CurrentBusinessFlow.Activities.CurrentItem != ExecutingActivity)
                    {
                        //If not equal means flow control update current item to target activity, no need to do next activity
                        ExecutingActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
                    }
                    else
                    {
                        if (!CurrentBusinessFlow.Activities.IsLastItem())
                        {
                            Thread.Sleep(1);
                            GotoNextActivity();
                            ExecutingActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
                        }
                        else
                        {
                            ExecutingActivity = null;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected error occurred during the execution of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), CurrentBusinessFlow), ex);
            }
            finally
            {

                if (mStopRun == false)
                {
                    ExecuteCleanUpActivities();
                }
                SetBusinessFlowActivitiesAndActionsSkipStatus();
                if (doContinueRun == false)
                {
                    SetActivityGroupsExecutionStatus();
                }
                CalculateBusinessFlowFinalStatus(CurrentBusinessFlow);
                PostScopeVariableHandling(CurrentBusinessFlow.Variables);
                if (standaloneExecution)
                {
                    PostScopeVariableHandling(BusinessFlow.SolutionVariables);
                }
                st.Stop();
                CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                PreviousBusinessFlow = CurrentBusinessFlow;
                NotifyBusinessFlowEnd(CurrentBusinessFlow);

                if (standaloneExecution)
                {
                    IsRunning = false;
                    Status = RunsetStatus;
                }

                if (mErrorPostExecutionActionFlowBreaker && handlerPostExecutionAction == eErrorHandlerPostExecutionAction.ReRunBusinessFlow)
                {
                    CheckAndExecutePostErrorHandlerAction();
                }

                if (mRunSource == eRunSource.BusinessFlow)
                {
                    mRunSource = null;
                    mErrorPostExecutionActionFlowBreaker = false;
                }
            }

        }



        private void UpdateLastExecutingAgent()
        {
            foreach (TargetBase target in CurrentBusinessFlow.TargetApplications)
            {
                target.LastExecutingAgentName = mGingerRunner.ApplicationAgents.FirstOrDefault(x => x.AppName.Equals(target.Name))?.AgentName;
            }
        }


        // Make private !!!!!!!!!!!!!!! !!!
        public void CalculateBusinessFlowFinalStatus(BusinessFlow BF, bool considrePendingAsSkipped = false)
        {
            try
            {
                // A flow is blocked if some activity failed and all the activities after it failed
                // A Flow is failed if one or more activities failed
                // A flow is skipped if all acticities are marked skipped

                // Add Blocked
                bool Failed = false;
                bool Blocked = false;
                bool Stopped = false;

                // All activities skipped
                if (BF.Activities.Count == 0 ||
                    BF.Activities.Count(x => x.GetType() == typeof(Activity) && x.Status == eRunStatus.Skipped) == BF.Activities.Count(x => x.GetType() == typeof(Activity)))
                {
                    BF.RunStatus = eRunStatus.Skipped;
                    return;
                }

                if (considrePendingAsSkipped &&
                    BF.Activities.Count(x => x.GetType() == typeof(Activity) && x.Status == eRunStatus.Pending) == BF.Activities.Count(x => x.GetType() == typeof(Activity)))
                {
                    BF.RunStatus = eRunStatus.Skipped;
                    return;
                }


                // Assume pass unless error
                eRunStatus newStatus = eRunStatus.Passed;

                foreach (Activity a in BF.Activities.Where(a => a.GetType() != typeof(ErrorHandler)))
                {
                    if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        Stopped = true;
                        break;
                    }
                    else if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        Failed = true;

                    }
                    else if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked)
                    {
                        Blocked = true;
                    }
                }

                if (Stopped)
                {
                    newStatus = eRunStatus.Stopped;
                }
                else if (Failed)
                {
                    newStatus = eRunStatus.Failed;
                }
                else if (Blocked)
                {
                    newStatus = eRunStatus.Blocked;
                }
                else
                {
                    newStatus = eRunStatus.Passed;
                }

                BF.RunStatus = newStatus;
                CalculateBusinessFlowActivyGroupStaus(BF);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Calculating Business flow final execution status", ex);
            }

        }

        public void CalculateBusinessFlowActivyGroupStaus(BusinessFlow BF)
        {
            foreach (ActivitiesGroup currentActivityGroup in BF.ActivitiesGroups)
            {
                CalculateActivitiesGroupFinalStatus(currentActivityGroup, BF);
            }
        }

        public void CalculateActivitiesGroupFinalStatus(ActivitiesGroup AG, BusinessFlow BF)
        {
            // A flow is blocked if some activity failed and all the activities after it failed
            // A Flow is failed if one or more activities failed

            // Add Blocked
            bool Failed = false;
            bool Blocked = false;
            bool Stopped = false;

            // Assume pass unless error
            AG.RunStatus = eActivitiesGroupRunStatus.Passed;

            if (AG.ActivitiesIdentifiers.Count == 0 ||
                AG.ActivitiesIdentifiers.Count(x => x.IdentifiedActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped) == AG.ActivitiesIdentifiers.Count)
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Skipped;
                return;
            }

            //Why we are doing this?
            _ = BF.Activities.Where(x => AG.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(x.Guid)).ToList();

            foreach (Activity a in BF.Activities.Where(x => AG.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(x.Guid)).ToList())
            {
                if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    //  Blocked = true;  // We assume from here on it i blocked
                    Failed = true;  // We found one failed activity so the flow is failed or blocked, to be decided at the end
                    break;
                }
                else if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked)
                {
                    Blocked = true;
                }
                else if (a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                {
                    Stopped = true;
                }
            }

            if (Blocked)
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Blocked;
            }
            else if (Failed)
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Failed;
            }
            else if (Stopped)
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Stopped;
            }
            else
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Passed;
            }
        }

        public void SetBusinessFlowActivitiesAndActionsSkipStatus(BusinessFlow businessFlow = null, bool avoidCurrentStatus = false)
        {
            try
            {
                if (businessFlow == null)
                {
                    businessFlow = CurrentBusinessFlow;
                }

                foreach (Activity a in businessFlow.Activities)
                {
                    if (mStopRun)
                    {
                        break;
                    }

                    foreach (Act act in a.Acts)
                    {
                        if (mStopRun)
                        {
                            break;
                        }

                        if (avoidCurrentStatus || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                        {
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                        }
                    }

                    if (avoidCurrentStatus || a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                    {
                        a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    }
                }

                foreach (ActivitiesGroup group in businessFlow.ActivitiesGroups)
                {
                    if (avoidCurrentStatus || group.ActivitiesIdentifiers.Count(x => x.IdentifiedActivity.Status == eRunStatus.Skipped) == group.ActivitiesIdentifiers.Count)
                    {
                        group.RunStatus = eActivitiesGroupRunStatus.Skipped;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Marking Pending activities and actions as skipped", ex);
            }

        }

        public void SetActivityGroupsExecutionStatus(BusinessFlow automateTab = null, bool offlineMode = false, ExecutionLoggerManager ExecutionLoggerManager = null)
        {
            try
            {
                if ((CurrentBusinessFlow == null) && (automateTab != null) && offlineMode)
                {
                    CurrentBusinessFlow = automateTab;
                    CurrentBusinessFlow.ActivitiesGroups.ToList().ForEach(x => x.ExecutionLoggerStatus = executionLoggerStatus.StartedNotFinishedYet);
                }
                foreach (ActivitiesGroup currentActivityGroup in CurrentBusinessFlow.ActivitiesGroups)
                {
                    CalculateActivitiesGroupFinalStatus(currentActivityGroup, CurrentBusinessFlow);
                    if (currentActivityGroup != null)
                    {
                        if (currentActivityGroup.RunStatus is not eActivitiesGroupRunStatus.Passed and not eActivitiesGroupRunStatus.Failed and not eActivitiesGroupRunStatus.Stopped)
                        {
                            currentActivityGroup.ExecutionLoggerStatus = executionLoggerStatus.NotStartedYet;
                        }
                        else
                        {
                            switch (currentActivityGroup.ExecutionLoggerStatus)
                            {
                                case executionLoggerStatus.NotStartedYet:
                                    // do nothing
                                    break;
                                case executionLoggerStatus.StartedNotFinishedYet:
                                    currentActivityGroup.ExecutionLoggerStatus = executionLoggerStatus.Finished;
                                    //if (executionLogger != null)
                                    //{                                    
                                    //    NotifyActivityGroupEnd(currentActivityGroup);
                                    //}
                                    //else
                                    //{                                    
                                    //    NotifyActivityGroupEnd(currentActivityGroup);
                                    //}
                                    NotifyActivityGroupEnd(currentActivityGroup, offlineMode);
                                    break;
                                case executionLoggerStatus.Finished:
                                    // do nothing
                                    break;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Setting activity group execution status", ex);
            }

        }

        private void SetPendingBusinessFlowsSkippedStatus()
        {
            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                if (mStopRun)
                {
                    break;
                }

                bf.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;

                //bellow needed?
                CurrentBusinessFlow = bf;
                CurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                SetBusinessFlowActivitiesAndActionsSkipStatus(bf);
                CalculateBusinessFlowActivyGroupStaus(bf);
                StartPublishResultsToAlmTask(CurrentBusinessFlow);
            }
        }

        public void SetNextBusinessFlowsBlockedStatus()
        {
            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                if (mStopRun)
                {
                    break;
                }

                if (bf.Active)
                {
                    bf.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                CurrentBusinessFlow = bf;
                CurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                SetNextActivitiesBlockedStatus();
                CalculateBusinessFlowActivyGroupStaus(bf);
                //TODO: Move All code Block inside the if Loop 
                if (bf.Active)
                {
                    StartPublishResultsToAlmTask(CurrentBusinessFlow);
                }
            }
        }

        private void SetNextActivitiesBlockedStatus()
        {
            Activity a = CurrentBusinessFlow.CurrentActivity;
            a.Reset();
            while (true)
            {
                if (mStopRun)
                {
                    break;
                }

                if (a.Active & a.Acts.Count > 0)
                {
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                    foreach (Act act in a.Acts)
                    {
                        if (act.Active)
                        {
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                        }
                    }

                }
                else
                {
                    //If activity is not active, mark it as skipped
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    foreach (Act act in a.Acts)
                    {
                        if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                        {
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                        }
                    }
                }
                if (CurrentBusinessFlow.Activities.IsLastItem())
                {
                    break;
                }
                else
                {
                    GotoNextActivity();
                    a = CurrentBusinessFlow.CurrentActivity;
                }
            }
        }
        private void SetNextActionsBlockedStatus()
        {
            Act act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;

            while (true)
            {
                if (mStopRun)
                {
                    break;
                }

                if (act.Active && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == DataRepositoryMethod.LiteDB)
                {
                    //To avoid repeat call to NotifyActionEnd for the same action
                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked)
                    {
                        NotifyActionEnd(act);
                    }
                }
                if (CurrentBusinessFlow.CurrentActivity.Acts.IsLastItem())
                {
                    break;
                }
                else
                {
                    CurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
                    act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                }
            }
        }

        public void StopRun()
        {
            mStopRun = true;

            if (CurrentBusinessFlow != null)
            {
                mExecutedActivityWhenStopped = CurrentBusinessFlow.CurrentActivity;
                mExecutedActionWhenStopped = (Act)CurrentBusinessFlow.CurrentActivity?.Acts.CurrentItem;
                mExecutedBusinessFlowWhenStopped = CurrentBusinessFlow;
                Agent currentAgent = (Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                // Added an extra condition for the Application to not throw an error if the  ((AgentOperations)currentAgent.AgentOperations).Driver is null
                if (currentAgent != null && ((AgentOperations)currentAgent.AgentOperations).Driver != null)
                {
                    ((AgentOperations)currentAgent.AgentOperations).Driver.cancelAgentLoading = true;
                }
            }
        }

        public void ResetRunnerExecutionDetails(bool doNotResetBusFlows = false, bool reSetActionErrorHandlerExecutionStatus = false)
        {
            Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            mStopRun = false;
            IsRunning = false;
            PublishToALMConfig = null;
            if (doNotResetBusFlows == false)
            {
                foreach (BusinessFlow businessFlow in BusinessFlows)
                {
                    businessFlow.Reset(reSetActionErrorHandlerExecutionStatus);
                    NotifyBusinessflowWasReset(businessFlow);
                }
            }
            LastFailedBusinessFlow = null;
            ExecutionLoggerManager.mExecutionLogger.ResetLastRunSetDetails();
        }



        public void CloseAgents()
        {
            if (mGingerRunner.KeepAgentsOn && !WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunModeParallel)
            {
                return;
            }
            else
            {
                foreach (ApplicationAgent p in mGingerRunner.ApplicationAgents.DistinctBy(x => x.AgentID))
                {
                    if (p.Agent != null)
                    {
                        if (p.Agent.AgentOperations == null)
                        {
                            p.Agent.AgentOperations = new AgentOperations(p.Agent);
                        }
                        try
                        {
                            p.Agent.AgentOperations.Close();
                        }
                        catch (Exception ex)
                        {
                            if (p.Agent.Name != null)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Close the '{0}' Agent", p.Agent.Name), ex);
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to Close the Agent", ex);
                            }
                        }
                        ((AgentOperations)p.Agent.AgentOperations).IsFailedToStart = false;
                    }
                }
                AgentsRunning = false;
            }
        }

        public void ResetFailedToStartFlagForAgents()
        {
            foreach (ApplicationAgent p in mGingerRunner.ApplicationAgents)
            {
                if (p.Agent != null)
                {
                    ((AgentOperations)p.Agent.AgentOperations).IsFailedToStart = false;
                }
            }
        }

        public void HighlightActElement(Act act)
        {
            if (HighLightElement)
            {
                if (act != null)
                {
                    SetCurrentActivityAgent();
                    Agent a = (Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                    if (a != null)
                    {
                        a.AgentOperations.HighLightElement(act);
                    }
                }
            }
        }

        private void ResetActivity(Activity a, bool resetErrorHandlerExecutedFlag = false)
        {
            a.Reset(resetErrorHandlerExecutedFlag);
        }

        public void ClearAgents()
        {
            CloseAgents();
            mGingerRunner.ApplicationAgents.Clear();
        }

        public void UpdateApplicationAgents()
        {
            // Make sure Ginger Runner have all Application/Platforms mapped to agent - create the list based on selected BFs to run
            // Make it based on current if we run from automate tab

            //Get the TargetApplication list
            ObservableList<TargetBase> bfsTargetApplications = [];

            //we will trigger property change only if bTargetAppListModified=true
            bool bTargetAppListModified = false;
            if (BusinessFlows.Count != 0)// Run Tab
            {
                foreach (BusinessFlow BF in BusinessFlows)
                {
                    foreach (TargetBase TA in BF.TargetApplications)
                    {
                        if (TA is TargetPlugin)
                        {
                            continue;//FIX ME: workaround to avoid adding Plugin mapping till dev of it will be completed
                        }

                        if (bfsTargetApplications.FirstOrDefault(x => x.Name == TA.Name) == null)
                        {
                            bfsTargetApplications.Add(TA);
                        }
                    }


                }
            }
            else if (CurrentBusinessFlow != null) // Automate Tab
            {
                foreach (TargetBase TA in CurrentBusinessFlow.TargetApplications)
                {
                    if (bfsTargetApplications.FirstOrDefault(x => x.Name == TA.Name) == null)
                    {
                        bfsTargetApplications.Add(TA);
                    }
                }
            }

            Dictionary<string, Agent> appNameToAgentMapping = [];

            //Remove the non relevant ApplicationAgents
            for (int indx = 0; indx < mGingerRunner.ApplicationAgents.Count;)
            {
                ApplicationAgent applicationAgent = (ApplicationAgent)mGingerRunner.ApplicationAgents[indx];

                bool appNotMappedInBF = bfsTargetApplications.All(x => x.Name != applicationAgent.AppName);

                if (!appNameToAgentMapping.TryGetValue(applicationAgent.AppName, out Agent availableAgentForApp))
                {
                    appNameToAgentMapping.Add(applicationAgent.AppName, GetAgentForApplication(applicationAgent.AppName));
                }

                bool appHasNonNAPlatformButNoAgent =
                    applicationAgent.Agent == null &&
                    applicationAgent.AppPlatform != null &&
                    applicationAgent.AppPlatform.Platform != ePlatformType.NA &&
                    availableAgentForApp != null;

                if (appNotMappedInBF || appHasNonNAPlatformButNoAgent)
                {
                    bTargetAppListModified = true;
                    mGingerRunner.ApplicationAgents.RemoveAt(indx);
                }

                else
                {
                    indx++;
                }
            }

            if (SolutionAgents != null)
            {
                //make sure all mapped agents still exist
                for (int indx = 0; indx < mGingerRunner.ApplicationAgents.Count; indx++)
                {
                    if (mGingerRunner.ApplicationAgents[indx].Agent != null)
                    {
                        if (!SolutionAgents.Any(x => x.Guid == ((RepositoryItemBase)mGingerRunner.ApplicationAgents[indx].Agent).Guid))
                        {
                            bTargetAppListModified = true;
                            mGingerRunner.ApplicationAgents.RemoveAt(indx);
                            indx--;
                        }
                    }
                }

                //mark the already used Agents
                foreach (Agent solAgent in SolutionAgents)
                {
                    if (mGingerRunner.ApplicationAgents.Any(x => x.Agent == solAgent))
                    {
                        solAgent.UsedForAutoMapping = true;
                    }
                    else
                    {
                        solAgent.UsedForAutoMapping = false;
                    }
                }
            }

            //Set the ApplicationAgents
            foreach (TargetBase TA in bfsTargetApplications)
            {
                // make sure GR got it covered
                if (!mGingerRunner.ApplicationAgents.Any(x => x.AppName == TA.Name))
                {
                    ApplicationAgent ag = new ApplicationAgent
                    {
                        AppName = TA.Name
                    };
                    Agent agentForApp;
                    if (!appNameToAgentMapping.TryGetValue(ag.AppName, out agentForApp))
                    {
                        agentForApp = GetAgentForApplication(ag.AppName);
                        appNameToAgentMapping.Add(ag.AppName, agentForApp);
                    }
                    ag.Agent = agentForApp;
                    bTargetAppListModified = true;
                    mGingerRunner.ApplicationAgents.Add(ag);
                }
            }
            if (bTargetAppListModified)
            {
                this.GingerRunner.OnPropertyChanged(nameof(GingerRunner.ApplicationAgents));//to notify who shows this list
            }
        }

        private Agent GetAgentForApplication(string appName)
        {
            Agent agent = null;
            ApplicationPlatform appPlatform = null;
            if (CurrentSolution != null && CurrentSolution.ApplicationPlatforms != null)
            {
                appPlatform = CurrentSolution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == appName);
            }

            if (appPlatform != null)
            {
                List<Agent> platformAgents = SolutionAgents
                    .Where(solutionAgent => solutionAgent.Platform == appPlatform.Platform && (solutionAgent.SupportVirtualAgent() || !solutionAgent.UsedForAutoMapping))
                    .ToList();

                //Get the last used agent to this Target App if exist
                if (string.IsNullOrEmpty(appPlatform.LastMappedAgentName) == false)
                {
                    //check if the saved agent still valid for this application platform                          
                    Agent matchingAgent = platformAgents.Find(x => string.Equals(x.Name, appPlatform.LastMappedAgentName));
                    if (matchingAgent != null)
                    {
                        agent = matchingAgent;
                        agent.UsedForAutoMapping = true;
                    }
                }

                if (agent == null)
                {
                    //set default agent
                    if (platformAgents.Count > 0)
                    {
                        if (appPlatform.Platform == ePlatformType.Web)
                        {
                            agent = platformAgents.Find(x =>
                            {
                                string browserTypeString = x.GetParamValue(nameof(GingerWebDriver.BrowserType));
                                if (Enum.TryParse(browserTypeString, out WebBrowserType browserType))
                                {
                                    return browserType == WebBrowserType.InternetExplorer;
                                }
                                else
                                {
                                    return false;
                                }
                            });
                        }

                        if (agent == null)
                        {
                            agent = platformAgents[0];
                        }

                        if (agent != null)
                        {
                            agent.UsedForAutoMapping = true;
                        }
                    }
                }
            }

            return agent;
        }

        // move from here !!!!!!!!!!!!!!!!!!
        public ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "")
        {
            var BFESs = new ObservableList<BusinessFlowExecutionSummary>();

            foreach (var BF in BusinessFlows)
            {
                // Ignore BF which are not active so they will not be calculated
                if (BF.Active)
                {
                    if (!GetSummaryOnlyForExecutedFlow ||
                        (GetSummaryOnlyForExecutedFlow && !(BF.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending || BF.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked)))
                    {
                        var BFES = new BusinessFlowExecutionSummary
                        {
                            BusinessFlowName = BF.Name,
                            BusinessFlowRunDescription = BF.RunDescription,
                            GingerRunnerName = GingerRunnerName,
                            Status = BF.RunStatus,
                            Activities = BF.Activities.Count,
                            Actions = BF.GetActionsCount(),
                            Validations = BF.GetValidationsCount(),
                            ExecutionVariabeles = BF.GetBFandActivitiesVariabeles(true),
                            ExecutionBFFlowControls = BF.BFFlowControls,
                            BusinessFlow = BF,
                            Selected = true
                        };
                        if (ExecutionLoggerManager.mExecutionLogger.ExecutionLogfolder != null && BF.ExecutionFullLogFolder != null)
                        {
                            BFES.BusinessFlowExecLoggerFolder = System.IO.Path.Combine(this.ExecutionLoggerManager.mExecutionLogger.ExecutionLogfolder, string.IsNullOrEmpty(BF.ExecutionLogFolder) ? string.Empty : BF.ExecutionLogFolder);
                        }

                        BFESs.Add(BFES);
                    }
                }
            }
            return BFESs;
        }

        // !!!!!!!!!!!!! cache use something else pr not here
        // keep in workspace !?
        public ExecutionLoggerManager ExecutionLoggerManager
        {
            get
            {
                ExecutionLoggerManager ExecutionLoggerManager = (ExecutionLoggerManager)mRunListeners.Find(x => x.GetType() == typeof(ExecutionLoggerManager));
                return ExecutionLoggerManager;
            }
        }

        public AccountReportExecutionLogger Centeralized_Logger
        {
            get
            {
                AccountReportExecutionLogger centeralized_Logger = (AccountReportExecutionLogger)mRunListeners.Find(x => x.GetType() == typeof(AccountReportExecutionLogger));
                return centeralized_Logger;
            }
        }

        public SealightsReportExecutionLogger Sealights_Logger
        {
            get
            {
                SealightsReportExecutionLogger sealights_Logger = (SealightsReportExecutionLogger)mRunListeners.Find(x => x.GetType() == typeof(SealightsReportExecutionLogger));
                return sealights_Logger;
            }
        }

        IExecutionLoggerManager IGingerExecutionEngine.ExecutionLoggerManager => ExecutionLoggerManager;

        //List<IRunListenerBase> IGingerExecutionEngine.RunListeners => mRunListeners;

        private bool CheckIfActivityTagsMatch()
        {
            //check if Activity or The parent Activities Group has at least 1 tag from filter tags
            //first check Activity
            foreach (Guid tagGuid in CurrentBusinessFlow.CurrentActivity.Tags)
            {
                if (mGingerRunner.FilterExecutionTags.FirstOrDefault(x => Guid.Equals(x, tagGuid) == true) != Guid.Empty)
                {
                    return true;
                }
            }

            //check in Activity Group
            if (string.IsNullOrEmpty(CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID) == false)
            {
                ActivitiesGroup group = CurrentBusinessFlow.ActivitiesGroups.FirstOrDefault(x => x.Name == CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID);
                if (group != null)
                {
                    foreach (Guid tagGuid in group.Tags)
                    {
                        if (mGingerRunner.FilterExecutionTags.FirstOrDefault(x => Guid.Equals(x, tagGuid) == true) != Guid.Empty)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void SetDriverPreviousRunStoppedFlag(bool flagValue)
        {
            if (CurrentBusinessFlow.CurrentActivity.CurrentAgent != null && ((AgentOperations)((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Driver != null)
            {
                ((AgentOperations)((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentOperations).Driver.PreviousRunStopped = flagValue;
            }
        }

        //Function to Do Flow Control on Business Flow in RunSet
        private int? DoBusinessFlowControl(BusinessFlow bf)
        {
            int? fcReturnIndex = null;
            //TODO: on pass, on fail etc...
            bool IsStopLoop = false;
            ValueExpression VE = new ValueExpression(mGingerRunner.ProjEnvironment, this.CurrentBusinessFlow, mGingerRunner.DSList);

            foreach (FlowControl FC in bf.BFFlowControls)
            {
                if (FC.Active == false)
                {
                    FC.Status = eStatus.Skipped;
                    continue;
                }
                else
                {
                    FC.Status = eStatus.Pending;
                }

                FC.CalculateCondition(CurrentBusinessFlow, mGingerRunner.ProjEnvironment, mGingerRunner.DSList);

                FC.CalcualtedValue(CurrentBusinessFlow, mGingerRunner.ProjEnvironment, mGingerRunner.DSList);

                bool IsConditionTrue = CalculateFlowControlStatus(null, mLastExecutedActivity, CurrentBusinessFlow, FC.Operator, FC.ConditionCalculated);

                if (IsConditionTrue)
                {
                    //Perform the action as condition is true
                    switch (FC.BusinessFlowControlAction)
                    {
                        case eBusinessFlowControlAction.GoToBusinessFlow:
                            if (GotoBusinessFlow(FC, bf, ref fcReturnIndex))
                            {
                                IsStopLoop = true;
                            }
                            else
                            {
                                FC.Status = eStatus.Action_Execution_Failed;
                            }
                            break;

                        case eBusinessFlowControlAction.RerunBusinessFlow:
                            fcReturnIndex = BusinessFlows.IndexOf(bf);
                            IsStopLoop = true;
                            break;

                        case eBusinessFlowControlAction.StopRun:
                            StopRun();
                            IsStopLoop = true;
                            break;
                        case eBusinessFlowControlAction.SetVariableValue:
                            try
                            {
                                VE.Value = FC.Value;
                                string[] vals = VE.ValueCalculated.Split(['=']);
                                if (vals.Length == 2)
                                {
                                    ActSetVariableValue setValueAct = new ActSetVariableValue
                                    {
                                        VariableName = vals[0],
                                        SetVariableValueOption = VariableBase.eSetValueOptions.SetValue,
                                        Value = vals[1],
                                        RunOnBusinessFlow = this.CurrentBusinessFlow,
                                        DSList = mGingerRunner.DSList
                                    };
                                    setValueAct.Execute();
                                }
                                else
                                {
                                    FC.Status = eStatus.Action_Execution_Failed;
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Set " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " Value Flow Control", ex);
                                FC.Status = eStatus.Action_Execution_Failed;
                            }
                            break;

                        default:
                            //TODO:
                            break;
                    }

                    if (FC.Status == eStatus.Pending)
                    {
                        FC.Status = eStatus.Action_Executed;
                    }
                }
                else
                {
                    FC.Status = eStatus.Action_Not_Executed;
                }

                // Go out the foreach in case we have a goto so no need to process the rest of FCs
                if (IsStopLoop)
                {
                    break;
                }
            }


            // If all above completed and no change on flow then move to next in the activity unless it is the last one
            if (!IsStopLoop)
            {

            }

            return fcReturnIndex;
        }

        private bool GotoBusinessFlow(FlowControl fc, BusinessFlow busFlow, ref int? bfExecutionIndex)
        {

            BusinessFlow bf = null;
            Guid guidToLookBy = Guid.Empty;

            if (!string.IsNullOrEmpty(fc.GetGuidFromValue().ToString()))
            {
                guidToLookBy = Guid.Parse(fc.GetGuidFromValue().ToString());
            }

            List<BusinessFlow> lstBusinessFlow = null;
            if (guidToLookBy != Guid.Empty)
            {
                lstBusinessFlow = BusinessFlows.Where(x => x.InstanceGuid == guidToLookBy).ToList();
            }

            if (lstBusinessFlow == null || lstBusinessFlow.Count == 0)
            {
                bf = null;
            }
            else if (lstBusinessFlow.Count == 1)
            {
                bf = lstBusinessFlow[0];
            }
            else//we have more than 1
            {
                BusinessFlow firstActive = lstBusinessFlow.Find(x => x.Active == true);
                if (firstActive != null)
                {
                    bf = firstActive;
                }
                else
                {
                    bf = lstBusinessFlow[0]; //no one is Active so returning the first one
                }
            }

            if (bf != null)
            {
                bfExecutionIndex = BusinessFlows.IndexOf(bf);
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name not found - " + mGingerRunner.Name);
                return false;
            }
        }



        private void NotifyPrepActionStart(Act action)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.PrepActionStart(evetTime, action);
            }
        }

        private void NotifyActionStart(Act action)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            action.StartTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActionStart(eventTime, action);
            }
        }


        private void NotifyPrepActionEnd(Act action)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            action.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.PrepActionEnd(evetTime, action);
            }
        }



        void NotifyRunnerRunstart()
        {
            uint evetTime = RunListenerBase.GetEventTime();
            this.StartTimeStamp = DateTime.UtcNow;
            Parallel.ForEach(mRunListeners, runnerListener =>
            {
                {
                    try
                    {
                        runnerListener.RunnerRunStart(evetTime, this.GingerRunner);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "NotifyRunnerRunstart failed for RunListener " + runnerListener.GetType().Name, ex);
                    }
                }
            });
        }

        void NotifyRunnerRunEnd(string ExecutionLogFolder = null)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            this.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.RunnerRunEnd(evetTime, this.GingerRunner, ExecutionLogFolder);
            }
        }

        private void NotifyActionEnd(Act action)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            action.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActionEnd(eventTime, action);
            }
        }

        public void GiveUserFeedback()
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.GiveUserFeedback(eventTime);
            }
        }


        private void NotifyUpdateActionStatusStart(Act action)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActionUpdatedStart(eventTime, action);
            }
        }

        private void NotifyUpdateActionStatusEnd(Act action)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActionUpdatedEnd(eventTime, action);
            }
        }

        private void NotifyDynamicActivityWasAddedToBusinessflow(BusinessFlow businessFlow)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.DynamicActivityWasAddedToBusinessflow(evetTime, CurrentBusinessFlow);
            }
        }


        private void NotifyActivityStart(Activity activity, bool continuerun = false)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            activity.StartTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivityStart(evetTime, CurrentBusinessFlow.CurrentActivity, continuerun);
            }
        }

        private void NotifyActivityEnd(Activity activity)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            activity.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivityEnd(evetTime, activity);
            }
        }

        private void NotifyActivitySkipped(Activity activity)
        {
            uint evetTime = RunListenerBase.GetEventTime();

            activity.StartTimeStamp = DateTime.UtcNow;
            activity.EndTimeStamp = DateTime.UtcNow;



            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivitySkipped(evetTime, activity);
            }
        }


        private void NotifyBusinessFlowEnd(BusinessFlow businessFlow)
        {
            try
            {
                Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, "Updating status for pending activities...");
                uint eventTime = RunListenerBase.GetEventTime();
                businessFlow.EndTimeStamp = DateTime.UtcNow;
                foreach (RunListenerBase runnerListener in mRunListeners)
                {
                    runnerListener.BusinessFlowEnd(eventTime, CurrentBusinessFlow);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Notify Businessflow End", ex);
            }
            finally
            {
                Reporter.HideStatusMessage();
            }

        }

        private void NotifyBusinessFlowStart(BusinessFlow businessFlow, bool ContinueRun = false)
        {
            uint evetTime = RunListenerBase.GetEventTime();
            businessFlow.StartTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.BusinessFlowStart(evetTime, CurrentBusinessFlow, ContinueRun);
            }
        }

        private void NotifyBusinessFlowSkipped(BusinessFlow businessFlow, bool ContinueRun = false)
        {
            uint evetTime = RunListenerBase.GetEventTime();

            businessFlow.StartTimeStamp = DateTime.UtcNow;
            businessFlow.EndTimeStamp = DateTime.UtcNow;

            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.BusinessFlowSkipped(evetTime, businessFlow, ContinueRun);
            }
        }
        private void NotifyBusinessflowWasReset(BusinessFlow businessFlow)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.BusinessflowWasReset(eventTime, businessFlow);
            }
        }

        public void NotifyEnvironmentChanged()
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.EnvironmentChanged(eventTime, mGingerRunner.ProjEnvironment);
            }
        }
        private void NotifyActivityGroupStart(ActivitiesGroup activityGroup)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            activityGroup.StartTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivityGroupStart(eventTime, activityGroup);
            }
        }

        private void NotifyActivityGroupEnd(ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            activityGroup.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                if (runnerListener.ToString().Contains("Ginger.Run.ExecutionLExecutionLoggerManagerogger"))
                {
                    ((Ginger.Run.ExecutionLoggerManager)runnerListener).mCurrentBusinessFlow = CurrentBusinessFlow;
                }
                runnerListener.ActivityGroupEnd(eventTime, activityGroup, offlineMode);
            }
        }

        private void NotifyActivityGroupSkipped(ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            uint eventTime = RunListenerBase.GetEventTime();

            activityGroup.StartTimeStamp = DateTime.UtcNow;
            activityGroup.EndTimeStamp = DateTime.UtcNow;

            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivityGroupSkipped(eventTime, activityGroup, offlineMode);
            }
        }

        // Called per user click on Run BF/Activity/Action
        private void NotifyExecutionContext(AutomationTabContext automationTabContext)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ExecutionContext(eventTime, automationTabContext, CurrentBusinessFlow);
            }
        }
        public bool SetBFOfflineData(BusinessFlow BF, ExecutionLoggerManager executionLoggerManager, string logFolderPath)
        {
            uint eventTime = RunListenerBase.GetEventTime();

            if (Context.BusinessFlow == null)
            {
                Context.BusinessFlow = BF;
            }

            Context.BusinessFlow.ExecutionLogActivityCounter = 1;
            try
            {
                if (System.IO.Directory.Exists(logFolderPath))
                {
                    Ginger.Reports.GingerExecutionReport.ExtensionMethods.CleanDirectory(logFolderPath);
                }
                else
                {
                    System.IO.Directory.CreateDirectory(logFolderPath);
                }

                BF.OffilinePropertiesPrep(logFolderPath);
                foreach (Activity activity in BF.Activities)
                {
                    ActivitiesGroup currentActivityGroup = BF.ActivitiesGroups.FirstOrDefault(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid));
                    if (currentActivityGroup != null)
                    {
                        currentActivityGroup.ExecutionLogFolder = logFolderPath;
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case executionLoggerStatus.NotStartedYet:
                                executionLoggerManager.ActivityGroupStart(eventTime, currentActivityGroup);
                                break;
                        }
                    }

                    this.CalculateActivityFinalStatus(activity);
                    if (activity.GetType() == typeof(IErrorHandler))
                    {
                        continue;
                    }
                    if (activity.Status is not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    activity.OfflinePropertiesPrep(BF.ExecutionLogFolder, Context.BusinessFlow.ExecutionLogActivityCounter, Ginger.Reports.GingerExecutionReport.ExtensionMethods.folderNameNormalazing(activity.ActivityName));
                    System.IO.Directory.CreateDirectory(activity.ExecutionLogFolder);
                    foreach (Act action in activity.Acts)
                    {
                        if (action.Status is not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped and not Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored)
                        {
                            continue;
                        }
                        activity.ExecutionLogActionCounter++;
                        action.ExecutionLogFolder = Path.Combine(activity.ExecutionLogFolder, activity.ExecutionLogActionCounter + " " + Ginger.Reports.GingerExecutionReport.ExtensionMethods.folderNameNormalazing(action.Description));

                        if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.TextFile)
                        {
                            System.IO.Directory.CreateDirectory(action.ExecutionLogFolder);
                        }
                        executionLoggerManager.mCurrentActivity = activity;
                        executionLoggerManager.ActionEnd(eventTime, action, true);
                    }
                    executionLoggerManager.ActivityEnd(eventTime, activity, true);
                    //BF.ExecutionLogActivityCounter++;
                    Context.BusinessFlow.ExecutionLogActivityCounter++;
                }
                this.SetActivityGroupsExecutionStatus(BF, true);
                this.CalculateBusinessFlowFinalStatus(BF);

                executionLoggerManager.BusinessFlowEnd(eventTime, BF, true);
                BF.ExecutionLogFolder = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Offline BusinessFlow Execution Log", ex);
                return false;
            }
        }

        public void ClearBindings()
        {
            if (CurrentBusinessFlow != null)
            {
                CurrentBusinessFlow.PropertyChanged -= CurrentBusinessFlow_PropertyChanged;
            }
        }
    }
}
