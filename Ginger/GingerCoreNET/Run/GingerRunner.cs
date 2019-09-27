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
using Amdocs.Ginger.Common.Expressions;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Repository.TargetLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Run;
using Amdocs.Ginger.CoreNET.TelemetryLib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.PlugIns;
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
using static GingerCoreNET.ALMLib.ALMIntegration;

namespace Ginger.Run
{
    public enum eContinueLevel
    {
        StandalonBusinessFlow,
        Runner
    }
    public enum eContinueFrom
    {
        LastStoppedAction,
        SpecificAction,
        SpecificActivity,
        SpecificBusinessFlow
    }
    public enum eRunLevel
    {
        NA,
        Runner,
        BusinessFlow
    }

    public class GingerRunner : RepositoryItemBase
    {
        

        public enum eActionExecutorType
        {
            RunWithoutDriver,
            RunOnDriver,
            RunInSimulationMode,
            RunOnPlugIn
        }

        public enum eRunOptions
        {
            [EnumValueDescription("Continue Business Flows Run on Failure ")]
            ContinueToRunall = 0,
            [EnumValueDescription("Stop Business Flows Run on Failure ")]
            StopAllBusinessFlows = 1,
        }


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
        
        public enum eResetStatus
        {
            All,
            FromSpecificActionOnwards,
            FromSpecificActivityOnwards,
        }

        List<RunListenerBase> mRunListeners = new List<RunListenerBase>();
        public List<RunListenerBase> RunListeners { get { return mRunListeners; } }


        [IsSerializedForLocalRepository]
        public int AutoWait { get; set; } = 0;

        private bool mStopRun = false;
        private bool mStopBusinessFlow = false;

        private bool mCurrentActivityChanged = false;
        private bool mErrorHandlerExecuted = false;        

        BusinessFlow mExecutedBusinessFlowWhenStopped=null;
        Activity mExecutedActivityWhenStopped=null;
        Act mExecutedActionWhenStopped=null;

        Activity mLastExecutedActivity;

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
                OnPropertyChanged(nameof(IsRunning));
            }
        }

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
                OnPropertyChanged("TotalBusinessflow");
            }
        }

        private eRunOptions mRunOption;
        [IsSerializedForLocalRepository]
        public eRunOptions RunOption
        {
            get
            {
                return mRunOption;
            }
            set
            {
                mRunOption = value;
                OnPropertyChanged(nameof(GingerRunner.RunOption));
            }
        }

        public ObservableList<Platform> Platforms = new ObservableList<Platform>();//TODO: delete me once projects moved to new Apps/Platform config, meanwhile enable to load old run set config, but ignore the value
        
        [IsSerializedForLocalRepository]
        public ObservableList<IApplicationAgent> ApplicationAgents { get; set; } = new ObservableList<IApplicationAgent>();
        
        [IsSerializedForLocalRepository]
        public ObservableList<Guid> FilterExecutionTags = new ObservableList<Guid>();


        public ObservableList<Agent> SolutionAgents { get; set; } = new ObservableList<Agent>();

        public ObservableList<ApplicationPlatform> SolutionApplications { get; set; }

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        private bool _Selected { get; set; }

        [IsSerializedForLocalRepository]
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                if (_Selected != value)
                {
                    _Selected = value;
                    OnPropertyChanged(nameof(Selected));
                }
            }
        }

        private bool mActive= true;
        [IsSerializedForLocalRepository(true)]
        public bool Active
        {
            get { return mActive; }
            set
            {
                if (mActive != value)
                {
                    mActive = value;
                    OnPropertyChanged(nameof(GingerRunner.Active));
                }
            }
        }

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus mStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus Status
        {
            get { return mStatus; }
            set
            {
                if (mStatus != value)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool UseSpecificEnvironment { get; set; }

        string mSpecificEnvironmentName;
        [IsSerializedForLocalRepository]
        public string SpecificEnvironmentName
        {
            get
            {
                return mSpecificEnvironmentName;
            }
            set
            {
                mSpecificEnvironmentName = value;
                OnPropertyChanged(nameof(SpecificEnvironmentName));
            }
        }

        [IsSerializedForLocalRepository]
        public bool FilterExecutionByTags { get; set; }

        ProjEnvironment mProjEnvironment;
        public ProjEnvironment ProjEnvironment
        {
            get
            {
                return mProjEnvironment;
            }
            set
            {
                mProjEnvironment = (ProjEnvironment)value;
                //ExecutionLogger.ExecutionEnvironment = (ProjEnvironment)value;
                mContext.Environment = mProjEnvironment;
                NotifyEnvironmentChanged();                
            }
        }

        

        public ObservableList<DataSourceBase> DSList {get; set;}
             

        private bool mRunInSimulationMode;
        [IsSerializedForLocalRepository]
        public bool RunInSimulationMode
        {
            get
            {                
                return mRunInSimulationMode;
            }
            set
            {
                mRunInSimulationMode = value;
                OnPropertyChanged(nameof(GingerRunner.RunInSimulationMode));
            }
        }

        public GingerRunner()
        {
            ExecutedFrom = eExecutedFrom.Run;

            // temp to be configure later !!!!!!!!!!!!!!!!!!!!!!!
            //RunListeners.Add(new ExecutionProgressReporterListener()); //Disabling till ExecutionLogger code will be enhanced

            RunListeners.Add(new ExecutionLoggerManager(mContext, ExecutedFrom));

            if (WorkSpace.Instance != null && !WorkSpace.Instance.Telemetry.DoNotCollect)
            {
                RunListeners.Add(new TelemetryRunListener());
            }
            
        }

        public GingerRunner(Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            ExecutedFrom = executedFrom;

            // temp to be configure later !!!!!!!!!!!!!!!!!!!!!!
            //RunListeners.Add(new ExecutionProgressReporterListener()); //Disabling till ExecutionLogger code will be enhanced
            RunListeners.Add(new ExecutionLoggerManager(mContext, ExecutedFrom));

            RunListeners.Add(new TelemetryRunListener());
        }


        public void SetExecutionEnvironment(ProjEnvironment defaultEnv, ObservableList<ProjEnvironment> allEnvs)
        {
            ProjEnvironment = null;
            if (UseSpecificEnvironment == true && string.IsNullOrEmpty(SpecificEnvironmentName) == false)
            {
                ProjEnvironment specificEnv = (from x in allEnvs where x.Name == SpecificEnvironmentName select (ProjEnvironment)x).FirstOrDefault();
                if (specificEnv != null)
                {
                    ProjEnvironment = specificEnv;
                }
            }

            if (ProjEnvironment == null)
            {
                ProjEnvironment = defaultEnv;
            }
        }

        public ISolution CurrentSolution { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<BusinessFlowRun> BusinessFlowsRunList { get; set; } = new ObservableList<BusinessFlowRun>();

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunsetStatus
        {
            get
            {
                if (BusinessFlows.Count() == 0)
                {
                    return eRunStatus.Skipped;
                }
                else if ((from x in BusinessFlows where x.RunStatus == eRunStatus.Stopped select x).Count() > 0)
                {
                    return eRunStatus.Stopped;
                }
                else if ((from x in BusinessFlows where x.RunStatus == eRunStatus.Failed select x).Count() > 0)
                {
                    return eRunStatus.Failed;
                }
                else if ((from x in BusinessFlows where x.RunStatus == eRunStatus.Blocked select x).Count() > 0)
                {
                    return eRunStatus.Blocked;
                }
                else if (((from x in BusinessFlows where (x.RunStatus == eRunStatus.Skipped) select x).Count() == BusinessFlows.Count) && BusinessFlows.Count > 0)
                {
                    return eRunStatus.Skipped;
                }
                else if (((from x in BusinessFlows where (x.RunStatus == eRunStatus.Passed || x.RunStatus == eRunStatus.Skipped) select x).Count() == BusinessFlows.Count)&& BusinessFlows.Count>0)
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
            BusinessFlowsRunList.Clear();
            foreach (BusinessFlow bf in BusinessFlows)
            {
                BusinessFlowRun BFR = new BusinessFlowRun();
                BFR.BusinessFlowName = bf.Name;
                BFR.BusinessFlowGuid = bf.Guid;
                BFR.BusinessFlowIsActive = bf.Active;
                BFR.BusinessFlowIsMandatory=bf.Mandatory;
                BFR.BusinessFlowInstanceGuid = bf.InstanceGuid;

                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(true))
                {
                    if (var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputValue) == false)//save only variables which were modified in this run configurations
                    {
                        VariableBase varCopy = (VariableBase)var.CreateCopy(false);
                        BFR.BusinessFlowCustomizedRunVariables.Add(varCopy);
                    }
                }
                BFR.BusinessFlowRunDescription = bf.RunDescription;
                BFR.BFFlowControls = bf.BFFlowControls ;
                BusinessFlowsRunList.Add(BFR);
            }
        }

        public ObservableList<BusinessFlow> BusinessFlows { get; set; } = new ObservableList<BusinessFlow>();

        public async Task<int> RunRunnerAsync()
        {
            var result = await Task.Run(() => {
                RunRunner();
                return 1;
            });
            return result;
        }

      


        public void RunRunner(bool doContinueRun = false)
        {
            bool runnerExecutionSkipped = false;
            try
            {
                if (Active == false || BusinessFlows.Count == 0)
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
                Status = eRunStatus.Started;
                IsRunning = true;
                mStopRun = false;
                if (doContinueRun == false)
                {
                    RunnerExecutionWatch.StartRunWatch();
                }
                else
                {
                    RunnerExecutionWatch.ContinueWatch();
                    ContinueTimerVariables(BusinessFlow.SolutionVariables);
                }

                //do Validations

                //Do execution preparations
                if (doContinueRun == false)
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

                int? flowControlIndx = null;                
                for (int bfIndx = startingBfIndx; bfIndx < BusinessFlows.Count; CalculateNextBFIndx(ref flowControlIndx, ref bfIndx))
                {
                    BusinessFlow executedBusFlow =(BusinessFlow) BusinessFlows[bfIndx];

                    //stop if needed before executing next BF
                    if (mStopRun)
                    {
                        break;
                    }

                    //validate BF run
                    if (!executedBusFlow.Active)
                    {
                        //set BF status as skipped                     
                        SetBusinessFlowActivitiesAndActionsSkipStatus(executedBusFlow);
                        CalculateBusinessFlowFinalStatus(executedBusFlow);
                        continue;
                    }

                    //Run Bf
                    if (doContinueRun && bfIndx == startingBfIndx)//this is the BF to continue from
                    {
                        RunBusinessFlow(null,false,true);//Continue BF run
                    }
                    else
                    {
                        //Execute the Business Flow
                        RunBusinessFlow(executedBusFlow);// full BF run
                    }
                    //Do "During Execution" Run set Operations
                    if (PublishToALMConfig!=null)
                    {
                        string result = string.Empty;
                        ObservableList<BusinessFlow> bfs = new ObservableList<BusinessFlow>();
                        bfs.Add(executedBusFlow);
                        RepositoryItemHelper.RepositoryItemFactory.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig, eALMConnectType.Silence);                        
                    }
                    //Call For Business Flow Control
                    flowControlIndx = DoBusinessFlowControl(executedBusFlow);
                    if (flowControlIndx == null && executedBusFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed) //stop if needed based on current BF failure
                    {
                        if ((executedBusFlow.Mandatory == true) || (executedBusFlow.Mandatory == false & RunOption == eRunOptions.StopAllBusinessFlows))
                        {
                            SetNextBusinessFlowsBlockedStatus();
                            break;
                        }
                    }

                }
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
                        if (ProjEnvironment != null)
                        {
                            ProjEnvironment.CloseEnvironment();
                        }
                        Status = eRunStatus.Completed;
                    }
                    PostScopeVariableHandling(BusinessFlow.SolutionVariables);
                    IsRunning = false;
                    RunnerExecutionWatch.StopRunWatch();
                    Status = RunsetStatus;

                    if (doContinueRun == false)
                    {                                        
                        NotifyRunnerRunEnd(CurrentBusinessFlow.ExecutionFullLogFolder);
                    }
                    if(RunLevel == eRunLevel.Runner)
                    {
                        ExecutionLoggerManager.mExecutionLogger.EndRunSet();
                        RunLevel = eRunLevel.NA;
                    }
                }   
                else
                {
                    Status = RunsetStatus;
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

        private void PrepareVariables()
        {
            if (ExecutedFrom == eExecutedFrom.Run)
            {
                //We need to set variable mapped values only when running run set
                SetVariableMappedValues();
            }

            PrepDynamicVariables();
        }

        private void SetVariableMappedValues()
        {
            BusinessFlowRun businessFlowRun = GetCurrenrtBusinessFlowRun();


            List<VariableBase> cachedVariables = null;

            //set the vars to update
            List<VariableBase> inputVars = CurrentBusinessFlow.GetBFandActivitiesVariabeles(true).ToList();
            List<VariableBase> outputVariables = null;

            //do actual value update
            foreach (VariableBase inputVar in inputVars)
            {
                string mappedValue = "";
                if (inputVar.MappedOutputType == VariableBase.eOutputType.Variable)
                {
                    if (outputVariables == null)
                    {
                        outputVariables = GetPossibleOutputVariables();
                    }

                    VariableBase outputVar = outputVariables.Where(x => x.Name == inputVar.MappedOutputValue).FirstOrDefault();
                    if (outputVar != null)
                    {
                        mappedValue = outputVar.Value;
                    }
                }
                else if(inputVar.MappedOutputType == VariableBase.eOutputType.DataSource)
                {
                    mappedValue = GingerCore.ValueExpression.Calculate(ProjEnvironment, CurrentBusinessFlow, inputVar.MappedOutputValue, DSList);
                }
                else if(inputVar.GetType() != typeof(VariablePasswordString) && inputVar.GetType() != typeof(VariableDynamic))
                {
                    //if input variable value mapped to none, and value is differt from origin                   
                    if (inputVar.DiffrentFromOrigin)
                    {
                        // we take value of customized variable from BusinessFlowRun
                        VariableBase runVar = businessFlowRun?.BusinessFlowCustomizedRunVariables?.Where(v => v.ParentGuid == inputVar.ParentGuid && v.ParentName == inputVar.ParentName && v.Name == inputVar.Name).FirstOrDefault();
                       
                        if(runVar!=null)
                        {
                           mappedValue = runVar.Value;
                        }                       
                    }
                    else
                    {
                        if(cachedVariables==null)
                        {
                            BusinessFlow cachedBusinessFlow = WorkSpace.Instance?.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(CurrentBusinessFlow.Guid);
                            cachedVariables = cachedBusinessFlow?.GetBFandActivitiesVariabeles(true).ToList();
                        }

                        //If value is not different from origin we take original value from business flow on cache
                        VariableBase cacheVariable= cachedVariables?.Where(v => v.ParentGuid == inputVar.ParentGuid && v.ParentName == inputVar.ParentName && v.Name == inputVar.Name).FirstOrDefault();
                        if(cacheVariable!= null)
                        {
                            mappedValue = cacheVariable.Value;
                        }                       
                    }
                }

                if (mappedValue != "")
                {
                    if (inputVar.GetType() == typeof(VariableString))
                    {
                        ((VariableString)inputVar).Value = mappedValue;
                        continue;
                    }
                    if (inputVar.GetType() == typeof(VariableSelectionList))
                    {
                        if (((VariableSelectionList)inputVar).OptionalValuesList.Where(pv => pv.Value == mappedValue).FirstOrDefault() != null)
                        {
                            ((VariableSelectionList)inputVar).Value = mappedValue;
                        }
                        continue;
                    }
                    if (inputVar.GetType() == typeof(VariableList))
                    {
                        string[] possibleVals = ((VariableList)inputVar).Formula.Split(',');
                        if (possibleVals != null && possibleVals.Contains(mappedValue))
                        {
                            ((VariableList)inputVar).Value = mappedValue;
                        }
                        continue;
                    }
                    if (inputVar.GetType() == typeof(VariableDynamic))
                    {
                        ((VariableDynamic)inputVar).ValueExpression = mappedValue;
                    }
                    if (inputVar.GetType() == typeof(VariableTimer))
                    {
                        ((VariableTimer)inputVar).Value = mappedValue;
                    }
                }
            }
        }

        private List<VariableBase> GetPossibleOutputVariables()
        {
            //set the vars to get value from
            List<VariableBase> outputVariables;
            if (BusinessFlow.SolutionVariables != null)
            {
                outputVariables = BusinessFlow.SolutionVariables.ToList();
            }
            else
            {
                outputVariables = new List<VariableBase>();
            }
            ObservableList<BusinessFlow> prevBFs = new ObservableList<BusinessFlow>();
            for (int i = 0; i < BusinessFlows.IndexOf(CurrentBusinessFlow); i++)
            {
                prevBFs.Add((BusinessFlow)BusinessFlows[i]);
            }
            foreach (BusinessFlow bf in prevBFs.Reverse())//doing in reverse for passing the most updated value of variables with similar name
            {
                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(false, false, true))
                {
                    outputVariables.Add(var);
                }
            }
            return outputVariables;
                 
        }      

        private BusinessFlowRun GetCurrenrtBusinessFlowRun()
        {
            BusinessFlowRun businessFlowRun = (from x in BusinessFlowsRunList where x.BusinessFlowInstanceGuid == CurrentBusinessFlow?.InstanceGuid select x).FirstOrDefault();

            if (businessFlowRun == null)
            {
                businessFlowRun = (from x in BusinessFlowsRunList where x.BusinessFlowName == CurrentBusinessFlow?.Name select x).FirstOrDefault();
            }
            return businessFlowRun;
        }

        public void StartAgent(Agent Agent)
        {
            try
            {
                Agent.ProjEnvironment = (ProjEnvironment)ProjEnvironment;
                Agent.BusinessFlow = CurrentBusinessFlow;
                Agent.SolutionFolder = SolutionFolder;
                Agent.DSList = DSList;                
                Agent.StartDriver();
                Agent.WaitForAgentToBeReady();
                if(Agent.Status== Agent.eStatus.NotStarted)
                {
                    Agent.IsFailedToStart = true;
                }
            }
            catch (Exception e)
            {
                Agent.IsFailedToStart = true;
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }
        }
        
        public void StopAgents()
        {
            foreach (ApplicationAgent AA in ApplicationAgents)
            {
                if (((Agent)AA.Agent) != null)
                {
                    ((Agent)AA.Agent).Close();
                }
            }
            AgentsRunning = false;
        }

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

        public string GetAgentsNameToRun()
        {
            string agentsNames = string.Empty;
            foreach (ApplicationAgent AP in ApplicationAgents)
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

        public async Task<int> RunActionAsync(Act act, bool checkIfActionAllowedToRun = true, bool standaloneExecution = false)
        {
            NotifyExecutionContext(AutomationTabContext.ActionRun);
            var result = await Task.Run(() => {
                RunAction(act, checkIfActionAllowedToRun, standaloneExecution);
                return 1;   
            });
            return result;
        }

        
        public void RunAction(Act act, bool checkIfActionAllowedToRun = true, bool standaloneExecution = false)
        {            
            try
            {
                //set Runner details if running in stand alone mode (Automate tab)
                if (standaloneExecution)
                {
                    IsRunning = true;
                    mStopRun = false;
                }

                //init
                act.SolutionFolder = SolutionFolder;

                //resetting the retry mechanism count before calling the function.
                act.RetryMechanismCount = 0;
                RunActionWithRetryMechanism(act, checkIfActionAllowedToRun);
                if (act.EnableRetryMechanism & mStopRun == false)
                {
                    while (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && act.RetryMechanismCount < act.MaxNumberOfRetries & mStopRun == false)
                    {
                        //Wait
                        act.RetryMechanismCount++;
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Wait;

                        GiveUserFeedback();
                        ProcessIntervaleRetry(act);

                        if (mStopRun)
                            break;

                        //Run Again
                        RunActionWithRetryMechanism(act, checkIfActionAllowedToRun);                        
                    }
                }
                if (mStopRun)
                {
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                    //To Handle Scenario which the Driver is still searching the element until Implicit wait will be done, lates being used on SeleniumDriver.Isrunning method 
                    SetDriverPreviousRunStoppedFlag(true);
                }
                else
                    SetDriverPreviousRunStoppedFlag(false);
            }
            finally
            {
                if (standaloneExecution)
                {
                    IsRunning = false;
                }                          
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

        private void RunActionWithRetryMechanism(Act act, bool checkIfActionAllowedToRun = true)
        {
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
                        ResetAction(act);
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                        if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == DataRepositoryMethod.LiteDB)
                        {
                            NotifyActionEnd(act);
                        }
                        act.ExInfo = "Action is not active.";
                        return;
                    }
                    if (act.CheckIfVaribalesDependenciesAllowsToRun((Activity)(CurrentBusinessFlow.CurrentActivity), true) == false)
                        return;
                }
                if (act.BreakPoint)
                {
                    StopRun();
                }
                if (mStopRun) return;
                eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
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

                while (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                {
                    RunActionWithTimeOutControl(act, ActionExecutorType);
                    CalculateActionFinalStatus(act);
                    // fetch all pop-up handlers
                    ObservableList<ErrorHandler> lstPopUpHandlers = GetAllErrorHandlersByType(eHandlerType.Popup_Handler);
                    if (lstPopUpHandlers.Count > 0)
                    {
                        executeErrorAndPopUpHandler(lstPopUpHandlers);
                    }

                    if (!mErrorHandlerExecuted
                        && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                    {
                        // returns list of mapped error handlers with the activity depending on type of error handling mapping chosen i.e. All Available Error Handlers, None or Specific Error Handlers
                        ObservableList<ErrorHandler> lstMappedErrorHandlers = GetErrorHandlersForCurrentActivity();

                        if (lstMappedErrorHandlers.Count <= 0)
                            break;

                    ResetAction(act);
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
                    
                    NotifyActionStart(act);
                    executeErrorAndPopUpHandler(lstMappedErrorHandlers);
                    mErrorHandlerExecuted = true;
                }
                else
                    break;

                }
                // Run any code needed after the action executed, used in ACTScreenShot save to file after driver took screen shot

                act.PostExecute();

                //Adding for new control
                ProcessStoretoValue(act);

                CalculateActionFinalStatus(act); //why we need to run it again?

                UpdateDSReturnValues(act);

                // Add time stamp 
                if (!string.IsNullOrEmpty(act.ExInfo))
                {
                    act.ExInfo = DateTime.Now.ToString() + " - " + act.ExInfo; 
                }
                else
                {
                    act.ExInfo = DateTime.Now.ToString();
                }
                ProcessScreenShot(act, ActionExecutorType);
                mErrorHandlerExecuted = false;

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
                Activity activity = (Activity)CurrentBusinessFlow.CurrentActivity;
                Act action = act;

                DoFlowControl(act);
                DoStatusConversion(act);   //does it need to be here or earlier?
            }
            finally
            {
                NotifyActionEnd(act); 
            }
        }
  
        private ObservableList<ErrorHandler> GetAllErrorHandlersByType(eHandlerType errHandlerType)
        {
            ObservableList<ErrorHandler> lstErrorHandler = new ObservableList<ErrorHandler>(CurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true
              && ((GingerCore.ErrorHandler)a).HandlerType == errHandlerType).Cast<ErrorHandler>().ToList());

            return lstErrorHandler;
        }

        private ObservableList<ErrorHandler> GetErrorHandlersForCurrentActivity()
        {
            ObservableList<ErrorHandler> errorHandlersToExecute = new ObservableList<ErrorHandler>();

            eHandlerMappingType typeHandlerMapping = (eHandlerMappingType)((Activity)CurrentBusinessFlow.CurrentActivity).ErrorHandlerMappingType;

            if (typeHandlerMapping == eHandlerMappingType.None)
            {
                // if set to NONE then, return an empty list. Nothing to execute!
                return errorHandlersToExecute;
            }

            if (typeHandlerMapping == eHandlerMappingType.SpecificErrorHandlers)
            {
                // fetch list of all error handlers in the current business flow
                ObservableList<ErrorHandler> allCurrentBusinessFlowErrorHandlers = GetAllErrorHandlersByType(eHandlerType.Error_Handler);

                ObservableList<ErrorHandler> lstMappedErrorHandler = new ObservableList<ErrorHandler>();
                foreach (Guid _guid in CurrentBusinessFlow.CurrentActivity.MappedErrorHandlers)
                {
                    // check if mapped error handlers are PRESENT in the current list of error handlers in the business flow i.e. allCurrentBusinessFlowErrorHandlers (checking for deletion, inactive etc.)
                    if (allCurrentBusinessFlowErrorHandlers.Where(x => x.Guid == _guid).Any()) //.ToList().Exists(x => x.Guid == _guid))
                    {
                        ErrorHandler _activity = CurrentBusinessFlow.Activities.Where(x => x.Guid == _guid).Cast<ErrorHandler>().FirstOrDefault();
                        lstMappedErrorHandler.Add(_activity);
                    }
                }
                // pass only specific mapped error handlers present the business flow
                errorHandlersToExecute = lstMappedErrorHandler;
            }
            else if (typeHandlerMapping == eHandlerMappingType.AllAvailableHandlers)
            {
                // pass all error handlers, by default
                errorHandlersToExecute = GetAllErrorHandlersByType(eHandlerType.Error_Handler);
            }
            return errorHandlersToExecute;

        }

        private void UpdateDSReturnValues(Act act)
        {
            try
            {
                if (act.ConfigOutputDS == false)
                    return;

                if (act.ReturnValues.Count == 0)
                    return;
                List<ActReturnValue> mReturnValues = (from arc in act.ReturnValues where arc.Active == true select arc).ToList();
                if (mReturnValues.Count == 0)
                    return;

                if (act.DSOutputConfigParams.Count > 0 && (act.OutDataSourceName == null || act.OutDataSourceTableName == null))
                {
                    act.OutDataSourceName = act.DSOutputConfigParams[0].DSName;
                    act.OutDataSourceTableName = act.DSOutputConfigParams[0].DSTable;
                    if(act.DSOutputConfigParams[0].OutParamMap != null)
                        act.OutDSParamMapType = act.DSOutputConfigParams[0].OutParamMap;
                    else
                        act.OutDSParamMapType = Act.eOutputDSParamMapType.ParamToRow.ToString();
                }

                List<ActOutDataSourceConfig> mADCS = (from arc in act.DSOutputConfigParams where arc.DSName == act.OutDataSourceName && arc.DSTable == act.OutDataSourceTableName && arc.Active == true select arc).ToList();
                if (mADCS.Count == 0 && act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToRow.ToString())
                    return;
                DataSourceBase DataSource = null;
                DataSourceTable DataSourceTable = null;
                foreach (DataSourceBase ds in DSList)
                {
                    if (ds.Name == act.OutDataSourceName)
                    {
                        DataSource = ds;
                        break;
                    }
                }
                if (DataSource == null)
                    return;

                DataSource.FileFullPath = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(DataSource.FilePath);

                ObservableList<DataSourceTable> dstTables = DataSource.GetTablesList();
                foreach(DataSourceTable dst in dstTables)
                {
                    if(dst.Name ==  act.OutDataSourceTableName)
                    {
                        DataSourceTable = dst;
                        break;
                    }
                }
                if (DataSourceTable == null)
                    return;

                List<string> mColList = DataSourceTable.DSC.GetColumnList(DataSourceTable.Name);
                if (act.OutDSParamMapType == null)
                    act.OutDSParamMapType = act.DSOutputConfigParams[0].OutParamMap;

                //Adding OutDataSurce Param at run time if not exist - Param to Col
                if (act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToCol.ToString())
                {
                    if (act.OutDataSourceName == null || act.OutDataSourceTableName == null)
                    {
                        act.OutDataSourceName = act.DSOutputConfigParams[0].DSName;
                        act.OutDataSourceTableName = act.DSOutputConfigParams[0].DSTable;
                    }

                    List<ActReturnValue> mUniqueRVs = (from arc in act.ReturnValues where arc.Path == "" || arc.Path == "1" select arc).ToList();
                    foreach (ActReturnValue item in mUniqueRVs)
                    {
                        mColList.Remove("GINGER_ID");
                        if (mColList.Contains("GINGER_LAST_UPDATED_BY"))
                            mColList.Remove("GINGER_LAST_UPDATED_BY");
                        if (mColList.Contains("GINGER_LAST_UPDATE_DATETIME"))
                            mColList.Remove("GINGER_LAST_UPDATE_DATETIME");
                        if (mColList.Contains("GINGER_USED"))
                            mColList.Remove("GINGER_USED");
                        act.AddOrUpdateOutDataSourceParam(act.OutDataSourceName, act.OutDataSourceTableName, item.Param, item.Param, "", mColList, act.OutDSParamMapType);
                    }
                    mADCS = (from arc in act.DSOutputConfigParams where arc.DSName == act.OutDataSourceName && arc.DSTable == act.OutDataSourceTableName && arc.Active == true && arc.OutParamMap == act.OutDSParamMapType select arc).ToList();
                    if (mADCS.Count == 0)
                        return;
                }

                foreach (ActOutDataSourceConfig ADSC in mADCS)
                {
                    if (mColList.Contains(ADSC.TableColumn) == false)
                        DataSource.AddColumn(DataSourceTable.Name, ADSC.TableColumn, "Text");
                }
                if (act.OutDSParamMapType == Act.eOutputDSParamMapType.ParamToCol.ToString())
                {
                    string sQuery = "";
                    if (DataSourceTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
                        return;

                    int iPathCount = 0;
                    List<ActReturnValue> mOutRVs = (from arc in act.ReturnValues where arc.Path == "" select arc).ToList();
                    if(mOutRVs.Count == 0)
                    {
                        iPathCount++;
                        mOutRVs = (from arc in act.ReturnValues where arc.Path == iPathCount.ToString() select arc).ToList();
                    }

                    while(mOutRVs.Count >0)
                    {
                        string sColList = "";
                        string sColVals = "";
                        foreach (ActOutDataSourceConfig ADSC in mADCS)
                        {
                            List<ActReturnValue> outReturnPath = (from arc in mOutRVs where arc.Param == ADSC.OutputType select arc).ToList();
                            if (outReturnPath.Count > 0)
                            {
                                sColList = sColList + ADSC.TableColumn + ",";
                                string valActual = outReturnPath[0].Actual == null ? "" : outReturnPath[0].Actual.ToString();
                                sColVals = sColVals + "'" + valActual.Replace("'","''") + "',";
                            }
                        }
                        if (sColList == "")
                            break;
                        sQuery = "INSERT INTO " + DataSourceTable.Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME,GINGER_USED) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "',false)";
                        DataSource.RunQuery(sQuery);
                        //Next Path
                        iPathCount++;
                        mOutRVs = (from arc in act.ReturnValues where arc.Path == iPathCount.ToString() select arc).ToList();
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
                                        if (item.Path != null && item.Path != "")
                                            sKeyName = item.Param + "_" + item.Path;
                                        else
                                            sKeyName = item.Param;
                                    }
                                    else if (ADSC.OutputType == "Parameter")
                                        sKeyName = item.Param;
                                    else
                                        sKeyValue = item.Actual;

                                    sKeyName = sKeyName.Replace("'", "''");
                                    sKeyValue = sKeyValue == null ? "" : sKeyValue.ToString();
                                    sKeyValue = sKeyValue.Replace("'", "''");
                                }
                                DataTable dtOut = DataSource.GetQueryOutput("Select Count(*) from " + DataSourceTable.Name + " Where GINGER_KEY_NAME='" + sKeyName + "'");
                                if (dtOut.Rows[0].ItemArray[0].ToString() == "1")
                                {
                                    sQuery = "UPDATE " + DataSourceTable.Name + " SET GINGER_KEY_VALUE='" + sKeyValue + "',GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "',GINGER_LAST_UPDATE_DATETIME='" + DateTime.Now.ToString() + "' Where GINGER_KEY_NAME='" + sKeyName + "'";
                                }
                                else
                                {
                                    sQuery = "INSERT INTO " + DataSourceTable.Name + "(GINGER_KEY_NAME,GINGER_KEY_VALUE,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME) VALUES ('" + sKeyName + "','" + sKeyValue + "','" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "')";
                                }
                            }
                            else
                            {
                                string sColList = "";
                                string sColVals = "";
                                foreach (ActOutDataSourceConfig ADSC in mADCS)
                                {
                                    sColList = sColList + ADSC.TableColumn + ",";
                                    if (ADSC.OutputType == "Parameter")
                                        sColVals = sColVals + "'" + item.Param.Replace("'","''") + "',";
                                    else if (ADSC.OutputType == "Path")
                                        sColVals = sColVals + "'" + item.Path + "',";
                                    else if (ADSC.OutputType == "Actual")
                                    {
                                        string strActual = item.Actual == null ? "" : item.Actual.ToString();
                                        sColVals = sColVals + "'" + strActual.Replace("'", "''") + "',";
                                    }

                                }
                                sQuery = DataSource.UpdateDSReturnValues(DataSourceTable.Name, sColList, sColVals);
                                //sQuery = "INSERT INTO " + DataSourceTable.Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME,GINGER_USED) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "',false)";
                            }
                            DataSource.RunQuery(sQuery);
                        }
                    }
                }
            }
            catch(Exception ex)
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
                act.ValueExpression = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);
            }
            act.ValueExpression.DecryptFlag = true;
            foreach (var IV in act.InputValues)
            {
                if (!string.IsNullOrEmpty(IV.Value))
                {                     
                    IV.ValueForDriver = act.ValueExpression.Calculate(IV.Value);
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
                foreach (var subList in list)
                {
                    foreach (var IV in subList)
                    {
                        if (!string.IsNullOrEmpty(IV.Value))
                        {                            
                            IV.ValueForDriver = act.ValueExpression.Calculate(IV.Value);
                        }
                        else
                        {
                            IV.ValueForDriver = string.Empty;
                        }
                    }
                }
            }
            act.ValueExpression.DecryptFlag = true;
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
                while((DateTime.Now - startingTime).TotalSeconds <= act.Wait)
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

        private void executeErrorAndPopUpHandler(ObservableList<ErrorHandler> errorHandlerActivity)
        {
            Activity originActivity = CurrentBusinessFlow.CurrentActivity;
            Act orginAction = (Act) CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;

            eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
            foreach (ErrorHandler errActivity in errorHandlerActivity)
            {
               CurrentBusinessFlow.CurrentActivity = errActivity;
                SetCurrentActivityAgent();
                Stopwatch stE = new Stopwatch();
                stE.Start();                
                foreach (Act act in errActivity.Acts)
                {
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    if (act.Active)
                    {
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;
                        if (errActivity.HandlerType == eHandlerType.Popup_Handler)
                            act.Timeout = 1;
                        PrepAction(act, ref ActionExecutorType, st);
                        RunActionWithTimeOutControl(act, ActionExecutorType);
                        ProcessStoretoValue(act);
                        UpdateDSReturnValues(act);
                        CalculateActionFinalStatus(act);
                    }
                    st.Stop();
                }
                SetBusinessFlowActivitiesAndActionsSkipStatus();
                CalculateActivityFinalStatus(errActivity);
                stE.Stop();
                errActivity.Elapsed = stE.ElapsedMilliseconds;
            }

            CurrentBusinessFlow.CurrentActivity = originActivity;
            CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = orginAction;
            mCurrentActivityChanged = false;
            SetCurrentActivityAgent();
        }

        private void PrepAction(Act action, ref eActionExecutorType ActExecutorType, Stopwatch st)
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
            if ((RunInSimulationMode) && (action.SupportSimulation))
            {
                ActExecutorType = eActionExecutorType.RunInSimulationMode;
            }
            else
            {
                if (typeof(ActPlugIn).IsAssignableFrom(action.GetType()))
                {
                    ActExecutorType = eActionExecutorType.RunOnPlugIn;
                }
                else if (typeof(ActWithoutDriver).IsAssignableFrom(action.GetType()))
                {
                    ActExecutorType = eActionExecutorType.RunWithoutDriver;
                }
                else
                {
                    ActExecutorType = eActionExecutorType.RunOnDriver;
                }
            }


            UpdateActionStatus(action, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running, st);
            GiveUserFeedback();

        }

        public void PrepActionValueExpression(Act act, BusinessFlow businessflow = null)
        {
            // We create VE for action only one time here
            ValueExpression VE = null;
            if (businessflow == null)
            {
                VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);
            }
            else
            {
                VE = new ValueExpression(ProjEnvironment, businessflow, DSList);
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
            IEnumerable<VariableBase> vars = from v in CurrentBusinessFlow.GetAllHierarchyVariables() where v.GetType() == typeof(VariableDynamic) select v;
            foreach (VariableBase v in vars)
            {
                VariableDynamic vd = (VariableDynamic)v;
                vd.Init(ProjEnvironment, CurrentBusinessFlow);
            }
        }

        private void ProcessScreenShot(Act act, eActionExecutorType ActionExecutorType)
        {
            string msg = string.Empty;

            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped) return;//user stopped it so no need screen shot

            if (ActionExecutorType == eActionExecutorType.RunOnDriver)
            {
                    if (act.TakeScreenShot || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                    try
                    {
                        if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)
                        {
                            TakeDesktopScreenShotIntoAction(act);
                        }
                        else
                        {
                            ActScreenShot screenShotAction = new ActScreenShot();
                            screenShotAction.LocateBy = eLocateBy.NA;
                            screenShotAction.WindowsToCapture = act.WindowsToCapture;

                            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                                screenShotAction.WindowsToCapture = ActScreenShot.eWindowsToCapture.AllAvailableWindows;
                            Agent a =(Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                            if (a == null)
                            {
                                msg = "Missing Agent for taking screen shot for the action: '" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += Environment.NewLine + msg;
                            }



                            else if (a.Status != Agent.eStatus.Running)
                            {
                                msg = "Screen shot not captured because agent is not running for the action:'" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += Environment.NewLine + msg;
                            }
                            else
                            {
                                if (a.AgentType == Agent.eAgentType.Driver)
                                {
                                    a.RunAction(screenShotAction);//TODO: Use IVisual driver to get screen shot instead of running action                         
                                    if (string.IsNullOrEmpty(screenShotAction.Error))//make sure the screen shot succeed
                                    {
                                        act.ScreenShots.AddRange(screenShotAction.ScreenShots);
                                        act.ScreenShotsNames.AddRange(screenShotAction.ScreenShotsNames);
                                    }
                                    else
                                    {
                                        act.ExInfo += Environment.NewLine + screenShotAction.Error;
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
                        act.ExInfo += Environment.NewLine + msg;
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
            string msg = string.Empty;

            try
            {
                Dictionary<string, String> screenShotsPaths = new Dictionary<string, String>();
                screenShotsPaths = RepositoryItemHelper.RepositoryItemFactory.TakeDesktopScreenShot(true);
                if (screenShotsPaths == null)
                {
                    if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)//log the error only if user asked for desktop screen shot to avoid confusion 
                    {
                        msg = "Failed to take desktop screen shot for the action: '" + act.Description + "'";
                        act.ExInfo += Environment.NewLine + msg;
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
                    act.ExInfo += Environment.NewLine + msg;
                    Reporter.ToLog(eLogLevel.WARN, msg, ex);
                }                                      
            }
        }


        private void RunActionWithTimeOutControl(Act act, eActionExecutorType ActExecutorType)
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
                    GingerCore.Drivers.DriverBase driver = ((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).Driver;
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
                        case eActionExecutorType.RunOnDriver:
                            {
                                if (currentAgent == null)
                                {
                                    if (string.IsNullOrEmpty(act.Error))
                                        act.Error = "No Agent was found for the" + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Application.";
                                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                }
                                else {
                                    if (((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).AgentType == Agent.eAgentType.Driver)
                                    {
                                        if (currentAgent.Status != Agent.eStatus.Running)
                                        {
                                            if (string.IsNullOrEmpty(act.Error))
                                            {
                                                if (currentAgent.Driver != null && !string.IsNullOrEmpty(currentAgent.Driver.ErrorMessageFromDriver))
                                                {
                                                    act.Error = currentAgent.Driver.ErrorMessageFromDriver;
                                                }
                                                else
                                                {
                                                    act.Error = "Agent failed to start for the " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Application.";
                                                }
                                            }

                                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                        }
                                        else
                                        {
                                            ((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).RunAction(act);
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
                                            act.Error = "Current Plugin Agent doesnot support execution for " + act.ActionDescription;
                                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                            
                                        }
                                    }

                                }
                            }
                          
                            break;

                        case eActionExecutorType.RunWithoutDriver:
                            RunWithoutAgent(act);
                            break;

                        case eActionExecutorType.RunOnPlugIn:
                            ExecuteOnPlugin.FindNodeAndRunAction((ActPlugIn)act);
                            
                            break;                        

                        case eActionExecutorType.RunInSimulationMode:
                            RunActionInSimulationMode(act);
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

                if (currentAgent != null && currentAgent.Driver != null)
                {
                    currentAgent.Driver.ActionCompleted(act);
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
                if (SolutionApplications.Where(x => x.AppName == AppName && x.Platform == ePlatformType.NA).FirstOrDefault() != null)
                    return;
            }
            if (string.IsNullOrEmpty(AppName))
            {
                // If we don't have Target App on activity then take first App from BF
                if (CurrentBusinessFlow.TargetApplications.Count() > 0)
                    AppName = CurrentBusinessFlow.TargetApplications[0].Name;
            }

            if (string.IsNullOrEmpty(AppName))
            {
                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Please select Target Application for the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and " + GingerDicser.GetTermResValue(eTermResKey.Activity));
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            ApplicationAgent AA = (ApplicationAgent)(from x in ApplicationAgents where x.AppName == AppName select x).FirstOrDefault();
            if (AA == null || ((Agent)AA.Agent) == null)
            {

                Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The current target application, " + AppName + ", doesn't have a mapped agent assigned to it");
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            ((Agent)AA.Agent).BusinessFlow = CurrentBusinessFlow;
            ((Agent)AA.Agent).ProjEnvironment = ProjEnvironment;
            // Verify the Agent for the action is running 
            Agent.eStatus agentStatus = ((Agent)AA.Agent).Status;
            if (agentStatus != Agent.eStatus.Running && agentStatus != Agent.eStatus.Starting && agentStatus != Agent.eStatus.FailedToStart)
            {
                // start the agent if one of the action s is not subclass of  ActWithoutDriver = driver action
                int count = (from x in CurrentBusinessFlow.CurrentActivity.Acts where typeof(ActWithoutDriver).IsAssignableFrom(x.GetType()) == false select x).Count();
                if (count > 0)
                {
                    StartAgent((Agent)AA.Agent);
                }
            }

            CurrentBusinessFlow.CurrentActivity.CurrentAgent = ((Agent)AA.Agent);
        }
        
        private void ProcessStoretoValue(Act act)
        {
            foreach (ActReturnValue item in act.ReturnValues)
            {
                if (item.StoreTo == ActReturnValue.eStoreTo.Variable && !String.IsNullOrEmpty(item.StoreToValue))
                {
                    VariableBase varb = CurrentBusinessFlow.GetHierarchyVariableByNameAndType(item.StoreToValue, "String");
                    if (varb != null)
                    {
                        ((VariableString)varb).Value = item.Actual;
                    }
                    else
                    {
                        string msg = string.Format("Cannot Store to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ", '{0}' - " + " not found", item.StoreToValue.ToString());
                        act.ExInfo += msg;
                        Reporter.ToLog(eLogLevel.WARN, msg);
                    }

                }
                //Adding for New Control
                else if (item.StoreTo == ActReturnValue.eStoreTo.DataSource && !String.IsNullOrEmpty(item.StoreToValue))
                {
                    
                    ValueExpression VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList, true, item.Actual);
                    VE.Calculate(item.StoreToValue);
                }
                else if(item.StoreTo == ActReturnValue.eStoreTo.ApplicationModelParameter && !string.IsNullOrEmpty(item.StoreToValue))
                {
                    GlobalAppModelParameter globalAppModelParameter = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>().Where(x => x.Guid.ToString() == item.StoreToValue).FirstOrDefault();
                    if (globalAppModelParameter != null)
                    {
                        globalAppModelParameter.CurrentValue = item.Actual;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to StoreTo the Model Parameter '{0}' for the Action: '{1}'", item.StoreToValue, act.Description));
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

            AWD.RunOnEnvironment = (ProjEnvironment)ProjEnvironment;
            // avoid NPE when running UT

            AWD.SolutionFolder = SolutionFolder;
            AWD.DSList = this.DSList;

            try
            {
                AWD.Execute();
            }
            catch (Exception ex)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                if (string.IsNullOrEmpty(act.Error))
                    act.Error = ex.Message;
            }
        }


        private void ResetAction(Act act)
        {
            act.Reset();
        }

        private void DoFlowControl(Act act)
        {            
            try
            {                                                 
                //TODO: on pass, on fail etc...
                bool IsStopLoop = false;                                

                foreach (FlowControl FC in act.FlowControls)
                {
                    if (FC.Active == false)
                    {
                        FC.Status = eStatus.Skipped;
                        continue;
                    }

                    FC.CalculateCondition(CurrentBusinessFlow, (ProjEnvironment)ProjEnvironment, act, this.DSList);

                    //TODO: Move below condition inside calculate condition once move execution logger to Ginger core

                    if (FC.ConditionCalculated.Contains("{LastActivityStatus}"))
                    {
                        if (mLastExecutedActivity != null)
                        {
                            FC.ConditionCalculated = FC.ConditionCalculated.Replace("{LastActivityStatus}", mLastExecutedActivity.Status.ToString());
                        }
                        else
                        {
                            FC.ConditionCalculated = FC.ConditionCalculated.Replace("{LastActivityStatus}", "Last executed Activity Status not available");
                        }
                    }
                    FC.CalcualtedValue(CurrentBusinessFlow, (ProjEnvironment)ProjEnvironment, this.DSList);

                

                    bool IsConditionTrue= CalculateFlowControlStatus(act, mLastExecutedActivity,CurrentBusinessFlow, FC.Operator,FC.ConditionCalculated);
                 
                    if (IsConditionTrue)
                    {
                        //Perform the action as condition is true
                        switch (FC.FlowControlAction)
                        {
                            case eFlowControlAction.MessageBox:
                                string txt = act.ValueExpression.Calculate(FC.Value);                                
                                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, txt);
                                break;
                            case eFlowControlAction.GoToAction:
                                if (GotoAction(FC, act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = eStatus.Action_Execution_Failed;
                                break;
                            case eFlowControlAction.GoToNextAction:
                                if (FlowControlGotoNextAction(act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = eStatus.Action_Execution_Failed;
                                break;
                            case eFlowControlAction.GoToActivity:
                            case eFlowControlAction.GoToActivityByName:
                                if (GotoActivity(FC, act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = eStatus.Action_Execution_Failed;
                                break;
                            case eFlowControlAction.GoToNextActivity:
                                if (FlowControlGotoNextActivity(act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = eStatus.Action_Execution_Failed;
                                break;
                            case eFlowControlAction.RerunAction:
                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                                IsStopLoop = true;
                                break;
                            case eFlowControlAction.RerunActivity:
                                ResetActivity(CurrentBusinessFlow.CurrentActivity);
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts[0];
                                IsStopLoop = true;
                                break;
                            case eFlowControlAction.StopBusinessFlow:
                                mStopBusinessFlow = true;
                                CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                IsStopLoop = true;
                                break;
                            case eFlowControlAction.FailActionAndStopBusinessFlow:
                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                act.Error = "Failed due to Flow Control rule";
                                act.ExInfo += FC.ConditionCalculated;
                                mStopBusinessFlow = true;
                                CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                IsStopLoop = true;
                                break;
                            case eFlowControlAction.StopRun:
                                StopRun();
                                IsStopLoop = true;
                                break;
                            case eFlowControlAction.SetVariableValue:
                                try
                                {                                    
                                    string[] vals = act.ValueExpression.Calculate(FC.Value).Split(new char[] { '=' });
                                    if (vals.Count() == 2)
                                    {
                                        ActSetVariableValue setValueAct = new ActSetVariableValue();
                                        PrepActionValueExpression(setValueAct, CurrentBusinessFlow);
                                        setValueAct.VariableName = vals[0];
                                        setValueAct.SetVariableValueOption = VariableBase.eSetValueOptions.SetValue;
                                        setValueAct.Value = vals[1];
                                        setValueAct.RunOnBusinessFlow = this.CurrentBusinessFlow;
                                        setValueAct.DSList = this.DSList;
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
                                    if (RunSharedRepositoryActivity(FC))
                                        IsStopLoop = true;
                                    else
                                        FC.Status = eStatus.Action_Execution_Failed;
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
                    if (IsStopLoop) break;
                }
                

                // If all above completed and no change on flow then move to next in the activity unless it is the last one
                if (!IsStopLoop)
                {
                    if (!IsLastActionOfActivity())
                    {
                        if (act.IsSingleAction == null || act.IsSingleAction == false)
                        {
                            // if execution has been stopped externally, stop at current action
                            if (!mStopRun)
                            {
                                GotoNextAction();
                                ((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in DoFlowControl", ex);
            }
        }

        public static bool CalculateFlowControlStatus(Act mAct,Activity mLastActivity,BusinessFlow CurrentBF,eFCOperator FCoperator,string Expression)
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
                    FCStatus = mAct.Status.Value == eRunStatus.Passed ? true : false;
                    break;

                case eFCOperator.ActionFailed:
                    FCStatus = mAct.Status.Value == eRunStatus.Failed ? true : false;
                    break;

                case eFCOperator.LastActivityPassed:
                    if (mLastActivity != null)
                    {
                        FCStatus = mLastActivity.Status == eRunStatus.Passed ? true : false;
                    }
                    else
                    {
                        FCStatus = false;
                    }
                    break;

                case eFCOperator.LastActivityFailed:
                    if (mLastActivity != null)
                    {
                        FCStatus = mLastActivity.Status == eRunStatus.Failed ? true : false;
                    }
                    else
                    {
                        FCStatus = false;
                    }
                    break;

                case eFCOperator.BusinessFlowPassed:
                    FCStatus = CurrentBF.RunStatus == eRunStatus.Passed ? true : false;
                    break;

                case eFCOperator.BusinessFlowFailed:
                    FCStatus = CurrentBF.RunStatus == eRunStatus.Failed ? true : false;
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
            Activity a =(Activity) CurrentBusinessFlow.GetActivity(fc.GetGuidFromValue(), fc.GetNameFromValue());

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
            Act a =(Act)((Activity)CurrentBusinessFlow.CurrentActivity).GetAct(fc.GetGuidFromValue(), fc.GetNameFromValue());

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

        private bool GotoNextAction()
        {
            return CurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
        }

        private bool FlowControlGotoNextAction(Act act)
        {
            if (CurrentBusinessFlow.CurrentActivity.Acts.Count - 1 > CurrentBusinessFlow.CurrentActivity.Acts.IndexOf((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem))
            {
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
            if (a.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped).FirstOrDefault() != null)   //
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
            else if (a.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).FirstOrDefault() != null)
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            else if (a.Acts.Count > 0 && a.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked).ToList().Count == a.Acts.Count)
                a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
            else
            {
                // If we have at least 1 pass then it passed, otherwise will remain Skipped
                if (a.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed).FirstOrDefault() != null)
                {
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
            }
        }

        private bool RunSharedRepositoryActivity(FlowControl fc)
        {
            //find activity            
            string activityName = fc.GetNameFromValue().ToUpper();
            ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            Activity sharedActivity = (Activity)activities.Where(x => x.ActivityName.ToUpper() == activityName).FirstOrDefault();

            if (sharedActivity != null)
            {
                //add activity
                Activity sharedActivityInstance = (Activity)sharedActivity.CreateInstance();
                sharedActivityInstance.Active = true;
                sharedActivityInstance.AddDynamicly = true;
                sharedActivityInstance.VariablesDependencies = CurrentBusinessFlow.CurrentActivity.VariablesDependencies;                
                CurrentBusinessFlow.SetActivityTargetApplication(sharedActivityInstance);
                                

                int index= CurrentBusinessFlow.Activities.IndexOf(CurrentBusinessFlow.CurrentActivity) + 1;
                ActivitiesGroup activitiesGroup = CurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID).FirstOrDefault();
                CurrentBusinessFlow.AddActivity(sharedActivityInstance, activitiesGroup, index);

                NotifyDynamicActivityWasAddedToBusinessflow(CurrentBusinessFlow);
                  
                //set it as next activity to run           
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                sharedActivityInstance.Acts.CurrentItem = sharedActivityInstance.Acts.FirstOrDefault();

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
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
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

            int CountFail = (from x in act.ReturnValues where x.Status == ActReturnValue.eStatus.Failed select x).Count();
            if (CountFail > 0)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }

            if (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
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
                    formatedExpectedCalculated = ARC.ExpectedCalculated.ToString().Substring(0, ARC.ExpectedCalculated.Length - 9);

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
                        ErrorInfo = ARC.Actual + " Does not Contains " + ARC.ExpectedCalculated;
                        break;
                    case eOperator.DoesNotContains:
                        status = !ARC.Actual.Contains(ARC.ExpectedCalculated);
                        ErrorInfo = ARC.Actual + " Contains " + ARC.ExpectedCalculated;
                        break;
                    case eOperator.Equals:
                        status = string.Equals(ARC.Actual, ARC.ExpectedCalculated);
                        ErrorInfo = ARC.Actual + " Does not equals " + ARC.ExpectedCalculated;
                        break;
                    case eOperator.Evaluate:
                        Expression = ARC.ExpectedCalculated;
                        ErrorInfo = "Function evaluation didn't resulted in True";
                        break;
                    case eOperator.GreaterThan:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Actual and Expected both values should be numeric";
                        }
                        else
                        {
                            Expression = ARC.Actual + ">" + ARC.ExpectedCalculated;
                            ErrorInfo = ARC.Actual + " is not greater than " + ARC.ExpectedCalculated;
                        }
                        break;
                    case eOperator.GreaterThanEquals:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Actual and Expected both values should be numeric";
                        }
                        else
                        {
                            Expression = ARC.Actual + ">=" + ARC.ExpectedCalculated;

                            ErrorInfo = ARC.Actual + " is not greater than equals to " + ARC.ExpectedCalculated;
                        }
                        break;
                    case eOperator.LessThan:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Actual and Expected both values should be numeric";
                        }
                        else
                        {
                            Expression = ARC.Actual + "<" + ARC.ExpectedCalculated;
                            ErrorInfo = ARC.Actual + " is not less than " + ARC.ExpectedCalculated;

                        }
                        break;
                    case eOperator.LessThanEquals:
                        if (!CheckIfValuesCanbecompared(ARC.Actual, ARC.ExpectedCalculated))
                        {
                            status = false;
                            ErrorInfo = "Actual and Expected both values should be numeric";
                        }
                        else
                        {
                            Expression = ARC.Actual + "<=" + ARC.ExpectedCalculated;
                            ErrorInfo = ARC.Actual + " is not less than equals to " + ARC.ExpectedCalculated;
                        }
                        break;
                    case eOperator.NotEquals:
                        status = !string.Equals(ARC.Actual, ARC.ExpectedCalculated);
                        ErrorInfo = ARC.Actual + " is equals to " + ARC.ExpectedCalculated;
                        break;
                    default:
                        ErrorInfo = "Not Supported Operation";
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

        private static bool CheckIfValuesCanbecompared(string actual,string Expected)
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
                    ExpectedRegex = ExpectedRegex.Substring(0, ExpectedRegex.Length - 1);
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

        public async Task<int> RunActivityAsync(Activity activity, bool Continue=false, bool standaloneExecution = false)
        {
            NotifyExecutionContext(AutomationTabContext.ActivityRun);
            var result = await Task.Run(() => {
                RunActivity(activity, false, standaloneExecution);
                return 1;  
            });
            return result;
        }


        public void RunActivity(Activity activity, bool doContinueRun = false, bool standaloneExecution = false)
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
                //check if Activity is allowed to run
                if (CurrentBusinessFlow == null ||
                    activity.Acts.Count == 0 || //no Actions to run
                        activity.GetType() == typeof(ErrorHandler) ||//don't run error handler from RunActivity
                            activity.CheckIfVaribalesDependenciesAllowsToRun(CurrentBusinessFlow, true) == false || //Variables-Dependencies not allowing to run
                                (FilterExecutionByTags == true && CheckIfActivityTagsMatch() == false))//add validation for Ginger runner tags
                {
                    CalculateActivityFinalStatus(activity);
                    return;
                }

                // handling ActivityGroup execution
                currentActivityGroup = (ActivitiesGroup)CurrentBusinessFlow.ActivitiesGroups.Where(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid)).FirstOrDefault();
                if (currentActivityGroup != null)
                {
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
                if (FilterExecutionByTags)
                {
                    if (CheckIfActivityTagsMatch() == false) return;
                }

                if (!doContinueRun)
                {
                    // We reset the activity unless we are in continue mode where user can start from middle of Activity
                    ResetActivity(CurrentBusinessFlow.CurrentActivity);
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

                if (SolutionApplications != null && SolutionApplications.Where(x => (x.AppName == activity.TargetApplication && x.Platform == ePlatformType.NA)).FirstOrDefault() == null)
                {
                    //load Agent only if Activity includes Actions which needs it
                    List<IAct> driverActs = activity.Acts.Where(x => (x is ActWithoutDriver && x.GetType() != typeof(ActAgentManipulation)) == false && x.Active == true).ToList();
                    if (driverActs.Count > 0)
                    {
                        //make sure not running in Simulation mode
                        if (!RunInSimulationMode ||
                            (RunInSimulationMode == true && driverActs.Where(x => x.SupportSimulation == false).ToList().Count() > 0))
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
                while (bHasMoreActions)
                {
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;

                    GiveUserFeedback();
                    if (act.Active && act.CheckIfVaribalesDependenciesAllowsToRun(activity, true) == true)
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

                            if (activity.ActionRunOption == eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
                            {
                                SetNextActionsBlockedStatus();
                                statusCalculationIsDone = true;
                                return;
                            }
                        }
                        GiveUserFeedback();
                        // If the user selected slower speed then do wait
                        if (AutoWait > 0)
                        {
                            // TODO: sleep 100 and do events
                            Thread.Sleep(AutoWait * 1000);
                        }

                        if (mStopRun || mStopBusinessFlow)
                        {
                            CalculateActivityFinalStatus(activity);
                            statusCalculationIsDone = true;
                            return;
                        }
                    }
                    else
                    {
                        if (!act.Active)
                        {
                            ResetAction(act);
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                            if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == DataRepositoryMethod.LiteDB)
                            {
                                NotifyActionEnd(act);
                            }
                            act.ExInfo = "Action is not active.";
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
            }
        }

        

        private void ContinueTimerVariables(ObservableList<VariableBase> variableList)
        {
            if (variableList == null || variableList.Count == 0)
                return;

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
            if (variableList == null || variableList.Count == 0)
                return;

            foreach(VariableBase variable in variableList)
            {
                if(variable.GetType()== typeof(VariableTimer))
                {
                    if(((VariableTimer)variable).RunWatch.IsRunning)
                    {
                        ((VariableTimer)variable).StopTimer();
                        ((VariableTimer)variable).IsStopped = true;
                    }
                   
                }               
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
            if (CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem != CurrentBusinessFlow.CurrentActivity.Acts[CurrentBusinessFlow.CurrentActivity.Acts.Count - 1])
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
                ContinueRun(continueLevel,continueFrom, specificBusinessFlow, specificActivity, specificAction);
                return 1;
            });
            return result;
        }
        public void ResetStatus(eContinueLevel continueLevel, eResetStatus resetFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null)
        {
            ResetRunStatus(continueLevel, resetFrom, specificBusinessFlow, specificActivity, specificAction);
        }

        private bool ResetRunStatus(eContinueLevel continueLevel, eResetStatus resetFrom, BusinessFlow specificBusinessFlow, Activity specificActivity, Act specificAction)
        {
            switch (resetFrom)
            {

                case eResetStatus.All:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.Reset();
                    break;

                case eResetStatus.FromSpecificActivityOnwards:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = specificActivity;
                    bool continueToReset = false;
                    foreach (Activity activity in CurrentBusinessFlow.Activities)
                    {
                        if(activity.Equals(specificActivity))
                        {
                            continueToReset=true;
                        }
                        if(continueToReset)
                        {
                            activity.Reset();
                        }
                    }
                    break;

                case eResetStatus.FromSpecificActionOnwards:
                    CurrentBusinessFlow = specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = specificActivity;
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = specificAction;
                    bool resetFromActivity = false, resetFromAction = false ;
                    foreach (Activity activity in CurrentBusinessFlow.Activities)
                    {
                        if (activity.Equals(specificActivity))
                        {
                            resetFromActivity = true;
                            foreach (Act act in activity.Acts)
                            {
                                if(act.Equals(specificAction))
                                {
                                    resetFromAction = true;
                                }
                                if (resetFromAction)
                                    act.Reset();
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
            switch (continueFrom)
            {
                case eContinueFrom.LastStoppedAction:
                    if (mExecutedBusinessFlowWhenStopped != null && BusinessFlows.Contains(mExecutedBusinessFlowWhenStopped))
                        CurrentBusinessFlow = mExecutedBusinessFlowWhenStopped;
                    else
                        return false;//can't do continue
                    if (mExecutedActivityWhenStopped != null && mExecutedBusinessFlowWhenStopped.Activities.Contains(mExecutedActivityWhenStopped))
                    {
                        CurrentBusinessFlow.CurrentActivity = mExecutedActivityWhenStopped;
                        CurrentBusinessFlow.ExecutionLogActivityCounter--;
                    }
                    else
                        return false;//can't do continue
                    if (mExecutedActionWhenStopped != null && mExecutedActivityWhenStopped.Acts.Contains(mExecutedActionWhenStopped))
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = mExecutedActionWhenStopped;
                    else
                        return false;//can't do continue
                    break;

                case eContinueFrom.SpecificBusinessFlow:
                    CurrentBusinessFlow = (BusinessFlow)specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.FirstOrDefault();
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.FirstOrDefault();
                    break;

                case eContinueFrom.SpecificActivity:
                    CurrentBusinessFlow = (BusinessFlow)specificBusinessFlow;
                    CurrentBusinessFlow.CurrentActivity = specificActivity;
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = specificActivity.Acts.FirstOrDefault();
                    break;

                case eContinueFrom.SpecificAction:
                    CurrentBusinessFlow = (BusinessFlow)specificBusinessFlow;
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
        
        public async Task<int> RunBusinessFlowAsync(BusinessFlow businessFlow, bool standaloneBfExecution = false, bool doContinueRun = false)
        {
            NotifyExecutionContext(AutomationTabContext.BussinessFlowRun);
            var result = await Task.Run(() =>
            {
                RunBusinessFlow(businessFlow, standaloneBfExecution, doContinueRun);
                return 1;
            });
            return result;
        }
   
        public void RunBusinessFlow(BusinessFlow businessFlow, bool standaloneExecution = false, bool doContinueRun = false)
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
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow = businessFlow;
                    Activity bfFirstActivity = (Activity)CurrentBusinessFlow.Activities.FirstOrDefault();
                    CurrentBusinessFlow.Activities.CurrentItem = bfFirstActivity;
                    CurrentBusinessFlow.CurrentActivity = bfFirstActivity;
                    bfFirstActivity.Acts.CurrentItem = bfFirstActivity.Acts.FirstOrDefault();
                }
             

                if(doContinueRun)
                {
                    ContinueTimerVariables(CurrentBusinessFlow.Variables);
                    if(standaloneExecution)
                    {
                        ContinueTimerVariables(BusinessFlow.SolutionVariables);
                    }
                }

                
                CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;

                //Do run preparations
                
                UpdateLastExecutingAgent();
                CurrentBusinessFlow.Environment = ProjEnvironment == null ? "" : ProjEnvironment.Name;
                PrepareVariables();


                if (!doContinueRun)
                {
                    NotifyBusinessFlowStart(CurrentBusinessFlow);
                }
                else
                {
                    NotifyBusinessFlowStart(CurrentBusinessFlow, doContinueRun);
                }

                mStopBusinessFlow = false;
                CurrentBusinessFlow.Elapsed = null;                
                st.Start();

                //Do Run validations
                if (CurrentBusinessFlow.Activities.Count == 0)
                {
                    return;//no Activities to run
                }

               
                
                //Start execution
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow.ExecutionLogActivityCounter = 1;
                    // ((ExecutionLogger)ExecutionLogger).BusinessFlowStart(CurrentBusinessFlow);
                }

                //Executing the Activities
                Activity ExecutingActivity = (Activity)CurrentBusinessFlow.CurrentActivity;
                Activity FirstExecutedActivity = ExecutingActivity;

                while (ExecutingActivity != null)
                {
                    if (ExecutingActivity.GetType() == typeof(ErrorHandler))
                    {
                        if (!CurrentBusinessFlow.Activities.IsLastItem())
                        {
                            GotoNextActivity();
                            ExecutingActivity = (Activity)CurrentBusinessFlow.Activities.CurrentItem;
                        }
                        else
                        {
                            ExecutingActivity = null;
                        }
                        continue;
                    }
                    else
                    {
                        ExecutingActivity.Status = eRunStatus.Running;
                        GiveUserFeedback();
                        if (ExecutingActivity.Active != false)
                        {
                            // We run the first Activity in Continue mode, if it came from RunFlow, then it is set to first action
                            if (FirstExecutedActivity.Equals(ExecutingActivity))
                            {
                                RunActivity(ExecutingActivity, true);
                            }
                            else
                            {
                                RunActivity(ExecutingActivity);
                            }
                            //TODO: Why this is here? do we need to rehook
                            CurrentBusinessFlow.PropertyChanged -= CurrentBusinessFlow_PropertyChanged;

                            if (ExecutingActivity.Mandatory && ExecutingActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                            {
                                //CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                                CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                if (!(CurrentBusinessFlow.Activities.IsLastItem()))
                                {
                                    GotoNextActivity();
                                    SetNextActivitiesBlockedStatus();
                                }
                                return;
                            }
                        }
                        else
                        {
                            ExecutingActivity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                        }

                        if (mStopRun || mStopBusinessFlow)
                        {
                            //CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                            SetBusinessFlowActivitiesAndActionsSkipStatus();
                            SetActivityGroupsExecutionStatus();
                            CurrentBusinessFlow.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                            return;
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
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected error occurred during the execution of the '{0}' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), CurrentBusinessFlow), ex);
            }
            finally
            {
                SetBusinessFlowActivitiesAndActionsSkipStatus();
                if (doContinueRun == false)
                {
                    SetActivityGroupsExecutionStatus();
                }
                CalculateBusinessFlowFinalStatus(CurrentBusinessFlow);
                PostScopeVariableHandling(CurrentBusinessFlow.Variables);
                if(standaloneExecution)
                {
                    PostScopeVariableHandling(BusinessFlow.SolutionVariables);
                }
                st.Stop();
                CurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;
                
                //if (doContinueRun == false)
                //{
                //    ExecutionLogger.BusinessFlowEnd(CurrentBusinessFlow);
                //}

                if (standaloneExecution)
                {
                    IsRunning = false;
                    Status = RunsetStatus;            
                }

                NotifyBusinessFlowEnd(CurrentBusinessFlow);

                                
            }
        }

      

        private void UpdateLastExecutingAgent()
        {
            foreach (TargetBase target in CurrentBusinessFlow.TargetApplications)
            {
                string a = (from x in ApplicationAgents where x.AppName == target.Name select x.AgentName).FirstOrDefault();
                target.LastExecutingAgentName = a;
            }
        }


        // Make private !!!!!!!!!!!!!!! !!!
        public void CalculateBusinessFlowFinalStatus(BusinessFlow BF, bool considrePendingAsSkipped= false)
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
                BF.Activities.Where(x=>x.GetType()== typeof(Activity) && x.Status== eRunStatus.Skipped).ToList().Count == BF.Activities.Where(x => x.GetType() == typeof(Activity)).ToList().Count)               
            {
                BF.RunStatus = eRunStatus.Skipped;
                return;
            }

            if (considrePendingAsSkipped &&
                BF.Activities.Where(x => x.GetType() == typeof(Activity) && x.Status == eRunStatus.Pending).ToList().Count == BF.Activities.Where(x => x.GetType() == typeof(Activity)).ToList().Count)
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
            else if(Failed)
            {
                newStatus = eRunStatus.Failed;
            }
            else if(Blocked)
            {
                newStatus = eRunStatus.Blocked;
            }
            else
            {
                newStatus = eRunStatus.Passed;
            }

            BF.RunStatus = newStatus;
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
            AG.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped).ToList().Count == AG.ActivitiesIdentifiers.Count)
            {
                AG.RunStatus = eActivitiesGroupRunStatus.Skipped;
                return;
            }

            BF.Activities.Where(x => AG.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(x.Guid)).ToList();

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

        public void SetBusinessFlowActivitiesAndActionsSkipStatus(BusinessFlow businessFlow=null, bool avoidCurrentStatus=false)
        {
            if (businessFlow == null)
                businessFlow = CurrentBusinessFlow;

            foreach (Activity a in businessFlow.Activities)
            {
                if (mStopRun)
                    break;

                foreach (Act act in a.Acts)
                {
                    if (mStopRun)
                        break;

                    if (avoidCurrentStatus || act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }

                if (avoidCurrentStatus || a.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                {
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
        }

        public void SetActivityGroupsExecutionStatus(BusinessFlow automateTab = null, bool offlineMode = false, ExecutionLoggerManager ExecutionLoggerManager = null)
        {
            if ((CurrentBusinessFlow == null) && (automateTab != null) && offlineMode)
            {
                CurrentBusinessFlow =(BusinessFlow) automateTab;
                CurrentBusinessFlow.ActivitiesGroups.ToList().ForEach(x => x.ExecutionLoggerStatus = executionLoggerStatus.StartedNotFinishedYet);
            }
            foreach (ActivitiesGroup currentActivityGroup in CurrentBusinessFlow.ActivitiesGroups)
            {
                CalculateActivitiesGroupFinalStatus(currentActivityGroup, CurrentBusinessFlow);
                if (currentActivityGroup != null)
                {
                    if (currentActivityGroup.RunStatus != eActivitiesGroupRunStatus.Passed && currentActivityGroup.RunStatus != eActivitiesGroupRunStatus.Failed && currentActivityGroup.RunStatus != eActivitiesGroupRunStatus.Stopped)
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

        private void SetPendingBusinessFlowsSkippedStatus()
        {
            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                if (mStopRun)
                    break;

                bf.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;

                //bellow needed?
                CurrentBusinessFlow = bf;
                CurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;

                SetBusinessFlowActivitiesAndActionsSkipStatus(bf);

            }
        }

        private void SetNextBusinessFlowsBlockedStatus()
        {
            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending))
            {
                if (mStopRun)
                    break;

                if (bf.Active)
                {
                    bf.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                CurrentBusinessFlow = bf;
                CurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                SetNextActivitiesBlockedStatus();
            }
        }

        private void SetNextActivitiesBlockedStatus()
        {
            Activity a = (Activity)CurrentBusinessFlow.CurrentActivity;
            a.Reset();
            while (true)
            {
                if (mStopRun)
                    break;

                if (a.Active & a.Acts.Count>0)
                {
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                    foreach (Act act in a.Acts)
                    {
                        if (act.Active)
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                    }

                }
                else
                {
                    //If activity is not active, mark it as skipped
                    a.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    foreach (Act act in a.Acts)
                    {
                        if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending)
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    }
                }
                if (CurrentBusinessFlow.Activities.IsLastItem())
                    break;
                else
                {
                    GotoNextActivity();
                    a = (Activity)CurrentBusinessFlow.CurrentActivity;
                }
            }
        }
        private void SetNextActionsBlockedStatus()
        {
            Act act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
            
            while (true)
            {
                if (mStopRun)
                    break;

                if (act.Active && act.Status!=Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed) act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                if (WorkSpace.Instance != null && WorkSpace.Instance.Solution != null && WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod == DataRepositoryMethod.LiteDB)
                {
                    NotifyActionEnd(act);
                }
                if (CurrentBusinessFlow.CurrentActivity.Acts.IsLastItem()) break;

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
            
            if(CurrentBusinessFlow != null)
            {
                mExecutedActivityWhenStopped = (Activity)CurrentBusinessFlow.CurrentActivity;
                mExecutedActionWhenStopped = (Act)CurrentBusinessFlow.CurrentActivity?.Acts.CurrentItem;
                mExecutedBusinessFlowWhenStopped = (BusinessFlow)CurrentBusinessFlow;
            }            
        }

        public void ResetRunnerExecutionDetails(bool doNotResetBusFlows=false)
        {
            Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            mStopRun = false;
            IsRunning = false;
            PublishToALMConfig = null;
            if (doNotResetBusFlows == false)
            {
                foreach (BusinessFlow businessFlow in BusinessFlows)
                {                    
                    businessFlow.Reset();    
                    NotifyBusinessflowWasReset(businessFlow);
                }
            }
        }

       

        public void CloseAgents()
        {
            foreach (ApplicationAgent p in ApplicationAgents)
            {
                if (p.Agent != null)
                {
                    try
                    {
                      ((Agent)p.Agent).Close();
                    }
                    catch (Exception ex)
                    {
                        if (p.Agent.Name != null)
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Close the '{0}' Agent", p.Agent.Name), ex);
                        else
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Close the Agent", ex);
                    }
                    ((Agent)p.Agent).IsFailedToStart = false;
                }
            }
            AgentsRunning = false;
        }

        public void ResetFailedToStartFlagForAgents()
        {
            foreach (ApplicationAgent p in ApplicationAgents)
            {
                if (p.Agent != null)
                {                   
                   ((Agent)p.Agent).IsFailedToStart = false;
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
                            a.HighLightElement(act);
                }
            }
        }

        private void ResetActivity(Activity a)
        {
            ((Activity)a).Reset();
        }
        
        public void ClearAgents()
        {
            CloseAgents();
            ApplicationAgents.Clear();
        }

        public void UpdateApplicationAgents()
        {
            // Make sure Ginger Runner have all Application/Platforms mapped to agent - create the list based on selected BFs to run
            // Make it based on current if we run from automate tab

            //Get the TargetApplication list
            ObservableList<TargetBase> bfsTargetApplications = new ObservableList<TargetBase>();
            if (BusinessFlows.Count() != 0)// Run Tab
            {
                foreach (BusinessFlow BF in BusinessFlows)
                {
                    foreach (TargetBase TA in BF.TargetApplications)
                    {
                        if (TA is TargetPlugin) continue;//FIX ME: workaround to avoid adding Plugin mapping till dev of it will be completed

                        if (bfsTargetApplications.Where(x => x.Name == TA.Name).FirstOrDefault() == null)
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
                    if (bfsTargetApplications.Where(x => x.Name == TA.Name).FirstOrDefault() == null)
                        bfsTargetApplications.Add(TA);
                }
            }

            //Remove the non relevant ApplicationAgents
            for (int indx = 0; indx < ApplicationAgents.Count;)
            {
                if (bfsTargetApplications.Where(x => x.Name == ApplicationAgents[indx].AppName).FirstOrDefault() == null || ((ApplicationAgent)ApplicationAgents[indx]).Agent == null)
                    ApplicationAgents.RemoveAt(indx);
                else
                    indx++;
            }

            if (SolutionAgents != null)
            {
                //make sure all mapped agents still exist
                for (int indx = 0; indx < ApplicationAgents.Count; indx++)
                {
                    if (ApplicationAgents[indx].Agent != null)
                        if (SolutionAgents.Where(x => ((RepositoryItemBase)x).Guid == ((RepositoryItemBase)ApplicationAgents[indx].Agent).Guid).FirstOrDefault() == null)
                        {
                            ApplicationAgents.RemoveAt(indx);
                            indx--;
                        }
                }

                //mark the already used Agents
                foreach (Agent solAgent in SolutionAgents)
                {
                    if (ApplicationAgents.Where(x => x.Agent == solAgent).FirstOrDefault() != null)
                        solAgent.UsedForAutoMapping = true;
                    else
                        solAgent.UsedForAutoMapping = false;
                }
            }

            //Set the ApplicationAgents
            foreach (TargetBase TA in bfsTargetApplications)
            {
                // make sure GR got it covered
                if (ApplicationAgents.Where(x => x.AppName == TA.Name).FirstOrDefault() == null)
                {
                    ApplicationAgent ag = new ApplicationAgent();
                    ag.AppName = TA.Name;

                    //Map agent to the application
                    ApplicationPlatform ap = null;
                    if (CurrentSolution!= null && CurrentSolution.ApplicationPlatforms != null)
                    {
                        ap = CurrentSolution.ApplicationPlatforms.Where(x => x.AppName == ag.AppName).FirstOrDefault();
                    }
                    if (ap != null)
                    {
                        List<Agent> platformAgents = (from p in SolutionAgents where p.Platform == ap.Platform && p.UsedForAutoMapping == false select (Agent)p).ToList();

                        //Get the last used agent to this Target App if exist
                        if (string.IsNullOrEmpty(ap.LastMappedAgentName) == false)
                        {
                            //check if the saved agent still valid for this application platform                          
                            Agent mathingAgent = platformAgents.Where(x => x.Name == ap.LastMappedAgentName).FirstOrDefault();
                            if (mathingAgent != null)
                            {
                                ag.Agent = mathingAgent;
                                ag.Agent.UsedForAutoMapping = true;
                            }
                        }

                        if (ag.Agent == null)
                        {
                            //set default agent
                            if (platformAgents.Count > 0)
                            {
                                if (ap.Platform == ePlatformType.Web)
                                    ag.Agent = platformAgents.Where(x => x.DriverType == Agent.eDriverType.SeleniumIE).FirstOrDefault();

                                if (ag.Agent == null)
                                    ag.Agent = platformAgents[0];

                                if (ag.Agent != null)
                                    ag.Agent.UsedForAutoMapping = true;
                            }
                        }
                    }

                    ApplicationAgents.Add(ag);
                }
            }

            this.OnPropertyChanged(nameof(ApplicationAgents));//to notify who shows this list
        }

        // move from here !!!!!!!!!!!!!!!!!!
        public ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false,string GingerRunnerName = "")
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
                        var BFES = new BusinessFlowExecutionSummary();
                        BFES.BusinessFlowName = BF.Name;
                        BFES.BusinessFlowRunDescription = BF.RunDescription;
                        BFES.GingerRunnerName = GingerRunnerName;
                        BFES.Status = BF.RunStatus;
                        BFES.Activities = BF.Activities.Count;
                        BFES.Actions = BF.GetActionsCount();
                        BFES.Validations = BF.GetValidationsCount();
                        BFES.ExecutionVariabeles = BF.GetBFandActivitiesVariabeles(true);
                        BFES.ExecutionBFFlowControls = BF.BFFlowControls;
                        BFES.BusinessFlow = BF;
                        BFES.Selected = true;
                        if (ExecutionLoggerManager.mExecutionLogger.ExecutionLogfolder != null && BF.ExecutionFullLogFolder != null)
                        {
                            BFES.BusinessFlowExecLoggerFolder = System.IO.Path.Combine(this.ExecutionLoggerManager.mExecutionLogger.ExecutionLogfolder,string.IsNullOrEmpty(BF.ExecutionLogFolder)?string.Empty: BF.ExecutionLogFolder);
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
                ExecutionLoggerManager ExecutionLoggerManager = (ExecutionLoggerManager)(from x in mRunListeners where x.GetType() == typeof(ExecutionLoggerManager) select x).SingleOrDefault();
                return ExecutionLoggerManager;
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }

        

        private bool CheckIfActivityTagsMatch()
        {
            //check if Activity or The parent Activities Group has at least 1 tag from filter tags
            //first check Activity
            foreach(Guid tagGuid in CurrentBusinessFlow.CurrentActivity.Tags)
                if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid)==true).FirstOrDefault() != Guid.Empty)
                    return true;

            //check in Activity Group
            if (string.IsNullOrEmpty(CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID) == false)
            {
                ActivitiesGroup group =(ActivitiesGroup) CurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID).FirstOrDefault();
                if(group != null)
                    foreach (Guid tagGuid in group.Tags)
                        if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid) == true).FirstOrDefault() != Guid.Empty)
                            return true;
            }

            return false;
        }

        private void SetDriverPreviousRunStoppedFlag(bool flagValue)
        {
            if (CurrentBusinessFlow.CurrentActivity.CurrentAgent != null && ((Agent)CurrentBusinessFlow.CurrentActivity.CurrentAgent).Driver != null)
               ((Agent) CurrentBusinessFlow.CurrentActivity.CurrentAgent).Driver.PreviousRunStopped = flagValue;
        }

        //Function to Do Flow Control on Business Flow in RunSet
        private int? DoBusinessFlowControl(BusinessFlow bf)
        {
            int? fcReturnIndex = null;
            //TODO: on pass, on fail etc...
            bool IsStopLoop = false;
            ValueExpression VE = new ValueExpression(this.ProjEnvironment, this.CurrentBusinessFlow, this.DSList);

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

                FC.CalculateCondition(CurrentBusinessFlow,(ProjEnvironment) ProjEnvironment, this.DSList);

                FC.CalcualtedValue(CurrentBusinessFlow, (ProjEnvironment)ProjEnvironment, this.DSList);

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
                                string[] vals = VE.ValueCalculated.Split(new char[] { '=' });
                                if (vals.Count() == 2)
                                {
                                    ActSetVariableValue setValueAct = new ActSetVariableValue();
                                    setValueAct.VariableName = vals[0];
                                    setValueAct.SetVariableValueOption = VariableBase.eSetValueOptions.SetValue;
                                    setValueAct.Value = vals[1];
                                    setValueAct.RunOnBusinessFlow = this.CurrentBusinessFlow;
                                    setValueAct.DSList = this.DSList;
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
                if (IsStopLoop) break;
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
                bf =(BusinessFlow) lstBusinessFlow[0];
            }
            else//we have more than 1
            {
                BusinessFlow firstActive = (BusinessFlow)lstBusinessFlow.Where(x => x.Active == true).FirstOrDefault();
                if (firstActive != null)
                {
                    bf = firstActive;
                }
                else
                {
                    bf = (BusinessFlow)lstBusinessFlow[0]; //no one is Active so returning the first one
                }
            }

            if (bf != null)
            {
                bfExecutionIndex = BusinessFlows.IndexOf(bf);
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Name not found - " + Name);
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
            Parallel.ForEach(mRunListeners, runnerListener =>
            {
                {
                    try
                    {
                        runnerListener.RunnerRunStart(evetTime, this);
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "NotifyRunnerRunstart failed for RunListener " + runnerListener.GetType().Name, ex);
                    }
                }
            });
        }

        void NotifyRunnerRunEnd(string ExecutionLogFolder= null)
        { 
            uint evetTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.RunnerRunEnd(evetTime, this, ExecutionLogFolder);
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

        private void GiveUserFeedback()
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


        private void NotifyActivityStart(Activity activity, bool continuerun=false)
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


        private void NotifyBusinessFlowEnd(BusinessFlow businessFlow)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            businessFlow.EndTimeStamp = DateTime.UtcNow;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.BusinessFlowEnd(eventTime, CurrentBusinessFlow);
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

        private void NotifyBusinessflowWasReset(BusinessFlow businessFlow)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.BusinessflowWasReset(eventTime, businessFlow);
            }
        }

        private void NotifyEnvironmentChanged()
        {
            uint eventTime = RunListenerBase.GetEventTime();
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.EnvironmentChanged(eventTime, mProjEnvironment);
            }
        }
        private void NotifyActivityGroupStart(ActivitiesGroup activityGroup)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            activityGroup.StartTimeStamp = eventTime; 
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                runnerListener.ActivityGroupStart(eventTime, activityGroup);
            }
        }

        private void NotifyActivityGroupEnd(ActivitiesGroup activityGroup, bool offlineMode = false)
        {
            uint eventTime = RunListenerBase.GetEventTime();
            activityGroup.EndTimeStamp = eventTime;
            foreach (RunListenerBase runnerListener in mRunListeners)
            {
                if(runnerListener.ToString().Contains("Ginger.Run.ExecutionLExecutionLoggerManagerogger"))
                {
                    ((Ginger.Run.ExecutionLoggerManager)runnerListener).mCurrentBusinessFlow = CurrentBusinessFlow;
                }
                runnerListener.ActivityGroupEnd(eventTime, activityGroup, offlineMode);
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
            try
            {
                if (System.IO.Directory.Exists(logFolderPath))
                    Ginger.Reports.GingerExecutionReport.ExtensionMethods.CleanDirectory(logFolderPath);
                else
                    System.IO.Directory.CreateDirectory(logFolderPath);
                BF.OffilinePropertiesPrep(logFolderPath);
                foreach (Activity activity in BF.Activities)
                {
                    ActivitiesGroup currentActivityGroup = BF.ActivitiesGroups.Where(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(activity.Guid)).FirstOrDefault();
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
                    if (activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && activity.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
                    {
                        continue;
                    }
                    activity.OfflinePropertiesPrep(BF.ExecutionLogFolder, BF.ExecutionLogActivityCounter, Ginger.Reports.GingerExecutionReport.ExtensionMethods.folderNameNormalazing(activity.ActivityName));
                    System.IO.Directory.CreateDirectory(activity.ExecutionLogFolder);
                    foreach (Act action in activity.Acts)
                    {
                        if (action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped && action.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored)
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
                    BF.ExecutionLogActivityCounter++;
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
    }
}
