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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Environments;
using Ginger.GeneralLib;
using Ginger.Repository;
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
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static Amdocs.Ginger.CoreNET.RunLib.NodeActionOutputValue;

//   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//
//   DO NOT put any reference to App.*  - GingerRunner should not refer any UI element or App as we can have several GRs running in paralel
//
//    if needed there is local projenv and Biz flow
//   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

//TODO: move this class to GingerCore
namespace Ginger.Run
{
    public class GingerRunner : RepositoryItem
    {
        public enum eExecutedFrom
        {
            Automation,
            Run
        }

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

        public enum eContinueLevel
        {
            StandalonBusinessFlow,
            Runner
        }
        public PublishToALMConfig PublishToALMConfig = null;
        public enum eContinueFrom
        {
            LastStoppedAction,
            SpecificAction,
            SpecificActivity,
            SpecificBusinessFlow
        }
        public enum eResetStatus
        {
            All,
            FromSpecificActionOnwards,
            FromSpecificActivityOnwards,
        }

        public new static class Fields
        {
            public static string RunOption = "RunOption";
            public static string Name = "Name";
            public static string Selected = "Selected";
            public static string Status = "Status";
            public static string CyclesToRun = "CyclesToRun";
            public static string CurrentCycle = "CurrentCycle";
            public static string UseSpecificEnvironment = "UseSpecificEnvironment";
            public static string SpecificEnvironmentName = "SpecificEnvironmentName";
            public static string FilterExecutionByTags = "FilterExecutionByTags";
            public static string RunInSimulationMode = "RunInSimulationMode";
        }

        private float App = 12345;  // NOT FOR USE - DO NOT REMOVE !!! -  This var in order to avoid using the real App. - see comment at header!!!        

        private int mSpeed = 0;
        private bool mStopRun = false;
        private bool mStopBusinessFlow = false;

        private bool mCurrentActivityChanged = false;
        private bool mErrorHandlerExecuted = false;
        private bool mIsRunning=false;

        BusinessFlow mExecutedBusinessFlowWhenStopped=null;
        Activity mExecutedActivityWhenStopped=null;
        Act mExecutedActionWhenStopped=null;

        Activity mLastExecutedActivity;

        public string ExecutionLogFolder { get; set; }
        public BusinessFlow CurrentBusinessFlow { get; set; }
        public bool GiveUserFeedback = false;
        public bool AgentsRunning = false;
        public ExecutionWatch RunnerExecutionWatch = new ExecutionWatch();        
        public eExecutedFrom ExecutedFrom;
        public ExecutionLogger ExecutionLogger;
        public string CurrentGingerLogFolder = string.Empty;
        public string CurrentHTMLReportFolder = string.Empty;
        public int ExecutionLogBusinessFlowCounter { get; set; }
        // public static bool UseExecutionLogger = false;//TODO:  temp flag so Beta users will not be impacted, removed when it is working and tested to be good 
        public string SolutionFolder { get; set; }
        public bool HighLightElement { get; set; }

        public event GingerRunnerEventHandler GingerRunnerEvent;
        public delegate void GingerRunnerEventHandler(GingerRunnerEventArgs EventArgs);
        

        public void OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType EvType, Object obj)
        {
            GingerRunnerEventHandler handler = GingerRunnerEvent;
            if (handler != null)
            {
                handler(new GingerRunnerEventArgs(EvType, obj));
            }
        }

        public bool IsRunning
        {
            get { return mIsRunning; }
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
        public ObservableList<ApplicationAgent> ApplicationAgents = new ObservableList<ApplicationAgent>();

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> FilterExecutionTags = new ObservableList<Guid>();

        public ObservableList<Agent> SolutionAgents;

        public ObservableList<ApplicationPlatform> SolutionApplications;

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                mName = value;
                OnPropertyChanged(Fields.Name);
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
                    OnPropertyChanged("Selected");
                }
            }
        }
        private bool mActive= true;
        [IsSerializedForLocalRepository]
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
                    OnPropertyChanged(Fields.Status);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool UseSpecificEnvironment { get; set; }

        [IsSerializedForLocalRepository]
        public string SpecificEnvironmentName { get; set; }

        [IsSerializedForLocalRepository]
        public bool FilterExecutionByTags { get; set; }

        public ProjEnvironment ProjEnvironment { get; set; }

        public LocalRepository SolutionLocalRepository { get; set; }

        public ObservableList<DataSourceBase> DSList { get; set; }

        private bool mRunInSimulationMode;
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
            ExecutionLogger = new ExecutionLogger(eExecutedFrom.Run);            
        }
       
        public GingerRunner(eExecutedFrom executedFrom)
        {
            ExecutedFrom = executedFrom;
            ExecutionLogger = new ExecutionLogger(ExecutedFrom);
        }


        public void SetExecutionEnvironment(ProjEnvironment defualtEnv, ObservableList<ProjEnvironment> allEnvs)
        {
            ProjEnvironment = null;
            if (UseSpecificEnvironment == true && string.IsNullOrEmpty(SpecificEnvironmentName) == false)
            {
                ProjEnvironment specificEnv = (from x in allEnvs where x.Name == SpecificEnvironmentName select x).FirstOrDefault();
                if (specificEnv != null)
                    ProjEnvironment = specificEnv;
            }

            if (ProjEnvironment == null)
                ProjEnvironment = defualtEnv;
        }

        public Solution CurrentSolution { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<BusinessFlowRun> BusinessFlowsRunList = new ObservableList<BusinessFlowRun>();

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunsetStatus
        {
            get
            {
                if (BusinessFlows.Count() == 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
                else if ((from x in BusinessFlows where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if ((from x in BusinessFlows where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if ((from x in BusinessFlows where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if (((from x in BusinessFlows where (x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped) select x).Count() == BusinessFlows.Count) && BusinessFlows.Count > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
                else if (((from x in BusinessFlows where (x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed || x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped) select x).Count() == BusinessFlows.Count)&& BusinessFlows.Count>0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
                else
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
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
                    if (var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputValue) == false)//save only variables which were modified in this run configurations
                        BFR.BusinessFlowCustomizedRunVariables.Add(var);
                BFR.BusinessFlowRunDescription = bf.RunDescription;

                BFR.BFFlowControls = bf.BFFlowControls ;
                BusinessFlowsRunList.Add(BFR);
            }
        }

        public ObservableList<BusinessFlow> BusinessFlows = new ObservableList<BusinessFlow>();

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
            try
            {
                if (!Active)                                
                    return;                
                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.RunnerRunStart, null);
                //Init 
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Started;
                mIsRunning = true;
                mStopRun = false;
                if (doContinueRun == false)
                    RunnerExecutionWatch.StartRunWatch();
                else
                {
                    RunnerExecutionWatch.ContinueWatch();

                    ContinueTimerVariables(BusinessFlow.SolutionVariables);
                }

                //do Validations

                //Do execution preparations
                if (doContinueRun == false)
                    UpdateApplicationAgents();

                //Start execution
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;

                int startingBfIndx = 0;
                if (doContinueRun == false)
                {
                    startingBfIndx = 0;
                    ExecutionLogger.GingerStart();
                }
                else
                {
                    startingBfIndx = BusinessFlows.IndexOf(CurrentBusinessFlow);//skip BFs which already executed
                }

                int? flowControlIndx = null;                
                for (int bfIndx = startingBfIndx; bfIndx < BusinessFlows.Count; CalculateNextBFIndx(ref flowControlIndx, ref bfIndx))
                {
                    BusinessFlow executedBusFlow = BusinessFlows[bfIndx];

                    //stop if needed before executing next BF
                    if (mStopRun)
                        break;

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
                        ALM.ALMIntegration.Instance.ExportBusinessFlowsResultToALM(bfs, ref result, PublishToALMConfig, ALM.ALMIntegration.eALMConnectType.Silence);
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

                    if (executedBusFlow.BFFlowControls.Count>0)
                    {
                        //doing execution logger again for recording the flow control status
                        ExecutionLogger.BusinessFlowEnd(executedBusFlow);
                    }
                }
            }
            finally
            {
                //Post execution items to do
                SetPendingBusinessFlowsSkippedStatus();
                
                if (Active)
                {

                    if (!mStopRun)//not on stop run
                    {
                        CloseAgents();
                        if (ProjEnvironment != null)
                            ProjEnvironment.CloseEnvironment();
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Completed;
                    }
                    PostScopeVariableHandling(BusinessFlow.SolutionVariables);
                    mIsRunning = false;
                    RunnerExecutionWatch.StopRunWatch();
                    Status = RunsetStatus;

                    if (doContinueRun == false)
                    {
                        ExecutionLogger.GingerEnd();
                    }
                }   
                else
                {
                    Status = RunsetStatus;
                }                

                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.RunnerRunEnd, null);
            }
        }
        //Calculate Next bfIndex for RunRunner Function
        private void CalculateNextBFIndx(ref int? flowControlIndx, ref int bfIndx)
        {
            if (flowControlIndx != null) //set bfIndex in case of BfFlowControl
                bfIndx = (int)flowControlIndx;
            else
                bfIndx++;
        }

        private void SetBusinessFlowInputVarsWithOutputValues(BusinessFlow bfToUpdate)
        {
            //set the vars to update
            List<VariableBase> inputVarsToUpdate = bfToUpdate.GetBFandActivitiesVariabeles(false, true, false).Where(x => (string.IsNullOrEmpty(x.MappedOutputValue)) == false).ToList();

            //set the vars to get value from
            List<VariableBase> outputVariables;
            if (BusinessFlow.SolutionVariables != null)
                outputVariables = BusinessFlow.SolutionVariables.ToList();
            else
                outputVariables = new List<VariableBase>();
            ObservableList<BusinessFlow> prevBFs = new ObservableList<BusinessFlow>();
            for (int i = 0; i < BusinessFlows.IndexOf(bfToUpdate); i++)
                prevBFs.Add(BusinessFlows[i]);
            foreach (BusinessFlow bf in prevBFs.Reverse())//doing in reverse for passing the most updated value of variables with similar name
                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(false, false, true))
                    outputVariables.Add(var);

            //do actual value update
            foreach(VariableBase inputVar in inputVarsToUpdate)
            {
                string mappedValue = "";
                if (inputVar.MappedOutputType == VariableBase.eOutputType.Variable)
                {
                    VariableBase outputVar = outputVariables.Where(x => x.Name == inputVar.MappedOutputValue).FirstOrDefault();
                    if (outputVar != null)                    
                        mappedValue = outputVar.Value;                    
                }                    
                else if(inputVar.MappedOutputType == VariableBase.eOutputType.DataSource)
                    mappedValue=GingerCore.ValueExpression.Calculate(ProjEnvironment, CurrentBusinessFlow, inputVar.MappedOutputValue, DSList);
                                
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
                            ((VariableSelectionList)inputVar).Value = mappedValue;
                        continue;
                    }
                    if (inputVar.GetType() == typeof(VariableList))
                    {
                        string[] possibleVals = ((VariableList)inputVar).Formula.Split(',');
                        if (possibleVals != null && possibleVals.Contains(mappedValue))
                            ((VariableList)inputVar).Value = mappedValue;
                        continue;
                    }
                    if (inputVar.GetType() == typeof(VariableDynamic))
                    {
                        ((VariableDynamic)inputVar).ValueExpression = mappedValue;
                    }
                }
            }
        }

        public void StartAgent(Agent Agent)
        {
            try
            {
                Agent.ProjEnvironment = ProjEnvironment;
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
                if (AA.Agent != null)
                {
                    AA.Agent.Close();
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
                if (AA.Agent == null)
                {
                    //TODO: else ask user to define agent  
                }
                else
                {    
                    if (AA.Agent.Driver == null)
                    {
                        //what is this? hard coded? - if we need to set agents order let's add a way to sort the agents
                        if (AA.Agent.DriverType == Agent.eDriverType.UnixShell)
                            ApplicationAgentsToStartLast.Add(AA);
                        else
                            StartAgent(AA.Agent);
                    }
                }
            }

            if(ApplicationAgentsToStartLast.Count>0)
            {
                foreach (ApplicationAgent AA in ApplicationAgentsToStartLast)
                {
                    if (AA.Agent.Driver == null)
                        StartAgent(AA.Agent);
                }
            }

            //Wait for all agents to be running            
            foreach (ApplicationAgent AA in ApplicationAgents)
            {
                if (AA.Agent != null)
                {
                    AA.Agent.WaitForAgentToBeReady();
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
                //init
                act.SolutionFolder = SolutionFolder;
                //set Runner details if running in stand alone mode (Automate tab)
                if (standaloneExecution)
                {
                    mIsRunning = true;
                    mStopRun = false;
                }
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
                        if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
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
                    //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                    SetDriverPreviousRunStoppedFlag(true);
                }
                else
                    SetDriverPreviousRunStoppedFlag(false);
            }
            finally
            {
                if (standaloneExecution)
                {
                    mIsRunning = false;
                }

                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActionEnd, null);
               
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
                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        SetDriverPreviousRunStoppedFlag(true);
                        return;
                    }
                    else
                    {
                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        SetDriverPreviousRunStoppedFlag(false);
                    }
                    UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Wait, st);
                    Thread.Sleep(100);  // Multiply * 10 to get 1 sec                    
                }
            }
        }

        private void RunActionWithRetryMechanism(Act act, bool checkIfActionAllowedToRun = true)
        {
            //Keep DebugStopWatch when testing performence
            //Stopwatch DebugStopWatch = new Stopwatch();
            //DebugStopWatch.Reset();
            //DebugStopWatch.Start();
            //Not suppose to happen but just in case
            
                if (act == null)
                {
                    Reporter.ToUser(eUserMsgKeys.AskToSelectAction);
                    return;
                }

                if (checkIfActionAllowedToRun)//to avoid duplicate checks in case the RunAction function is called from RunActvity
                {
                    if (!act.Active)
                    {
                        ResetAction(act);
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                        act.ExInfo = "Action is not active.";
                        return;
                    }
                    if (act.CheckIfVaribalesDependenciesAllowsToRun(CurrentBusinessFlow.CurrentActivity, true) == false)
                        return;
                }
                if (act.BreakPoint)
                {
                    StopRun();
                }
                if (mStopRun) return;
                eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
                Stopwatch st = new Stopwatch();
                st.Start();

                PrepAction(act, ref ActionExecutorType, st);

                if (mStopRun)
                {
                    return;
                }
                if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);

                ExecutionLogger.ActionStart(CurrentBusinessFlow, CurrentBusinessFlow.CurrentActivity, act);

                while (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
                {
                    RunActionWithTimeOutControl(act, ActionExecutorType);
                    CalculateActionFinalStatus(act);
                    // fetch all pop-up handlers
                    ObservableList<ErrorHandler> lstPopUpHandlers = GetAllErrorHandlersByType(ErrorHandler.eHandlerType.Popup_Handler);
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
                        OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActionStart, null);
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
                act.ExInfo = DateTime.Now.ToString() + " - " + act.ExInfo;

                ProcessScreenShot(act, ActionExecutorType);


                mErrorHandlerExecuted = false;
                try
                {
                    AutoLogProxy.LogAction(CurrentBusinessFlow, act);
                }
                catch { }

                // Stop the counter before DoFlowControl
                st.Stop();

                // final timing of the action
                act.Elapsed = st.ElapsedMilliseconds;
                act.ElapsedTicks = st.ElapsedTicks;

                //check if we have retry mechanism if yes go till max
                if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed && act.EnableRetryMechanism && act.RetryMechanismCount < act.MaxNumberOfRetries)
                {
                    //since we retrun and don't do flow control the action is going to run again
                    ExecutionLogger.ActionEnd(CurrentBusinessFlow.CurrentActivity, act);
                    return;
                }
                // we capture current activity and action to use it for execution logger,
                // because in DoFlowControl(act) it will point to the other action/activity(as flow control will be applied)
                Activity activity = CurrentBusinessFlow.CurrentActivity;
                Act action = act;

                DoFlowControl(act);
                DoStatusConversion(act);   //does it need to be here or earlier?

                ExecutionLogger.ActionEnd(activity, action);
            }
            
        private ObservableList<ErrorHandler> GetAllErrorHandlersByType(ErrorHandler.eHandlerType errHandlerType)
        {
            ObservableList<ErrorHandler> lstErrorHandler = new ObservableList<ErrorHandler>(CurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true
              && ((GingerCore.ErrorHandler)a).HandlerType == errHandlerType).Cast<ErrorHandler>().ToList());

            return lstErrorHandler;
        }

        private ObservableList<ErrorHandler> GetErrorHandlersForCurrentActivity()
        {
            ObservableList<ErrorHandler> errorHandlersToExecute = new ObservableList<ErrorHandler>();

            ErrorHandler.eHandlerMappingType typeHandlerMapping = (ErrorHandler.eHandlerMappingType)CurrentBusinessFlow.CurrentActivity.ErrorHandlerMappingType;

            if (typeHandlerMapping == ErrorHandler.eHandlerMappingType.None)
            {
                // if set to NONE then, return an empty list. Nothing to execute!
                return errorHandlersToExecute;
            }

            if (typeHandlerMapping == ErrorHandler.eHandlerMappingType.SpecificErrorHandlers)
            {
                // fetch list of all error handlers in the current business flow
                ObservableList<ErrorHandler> allCurrentBusinessFlowErrorHandlers = GetAllErrorHandlersByType(ErrorHandler.eHandlerType.Error_Handler);

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
            else if (typeHandlerMapping == ErrorHandler.eHandlerMappingType.AllAvailableHandlers)
            {
                // pass all error handlers, by default
                errorHandlersToExecute = GetAllErrorHandlersByType(ErrorHandler.eHandlerType.Error_Handler);
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

                if (DataSource.FilePath.StartsWith("~"))
                {
                    DataSource.FileFullPath = DataSource.FilePath.Replace("~", "");
                    DataSource.FileFullPath = Ginger.App.UserProfile.Solution.Folder + DataSource.FileFullPath;
                }
                DataSource.Init(DataSource.FileFullPath);
                ObservableList<DataSourceTable> dstTables = DataSource.DSC.GetTablesList();
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
                        DataSource.DSC.AddColumn(DataSourceTable.Name, ADSC.TableColumn, "Text");
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
                        DataSource.DSC.RunQuery(sQuery);
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
                                DataTable dtOut = DataSource.DSC.GetQueryOutput("Select Count(*) from " + DataSourceTable.Name + " Where GINGER_KEY_NAME='" + sKeyName + "'");
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
                                sQuery = "INSERT INTO " + DataSourceTable.Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME,GINGER_USED) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "',false)";
                            }
                            DataSource.DSC.RunQuery(sQuery);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured in UpdateDSReturnValues : ", ex);
            }
        }
        public void ProcessReturnValueForDriver(Act act)
        {
            //Handle all output values, create Value for Driver for each
            ValueExpression VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);

            foreach (ActReturnValue ARV in act.ActReturnValues)
            {
                VE.Value = ARV.Param;
                ARV.ParamCalculated = VE.ValueCalculated;
                VE.Value = ARV.Path;
                ARV.PathCalculated = VE.ValueCalculated;
            }
        }

        public void ProcessInputValueForDriver(Act act)
        {
            //Handle all input values, create Value for Driver for each
            ValueExpression VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);

            VE.DecryptFlag = true;
            foreach (var IV in act.InputValues)
            {
                if (!string.IsNullOrEmpty(IV.Value))
                {
                    VE.Value = IV.Value;
                    IV.ValueForDriver = VE.ValueCalculated;
                }
                else
                {
                    IV.ValueForDriver = string.Empty;
                }
            }

            //Handle actions which needs VE processing like Tuxedo, we need to calcualte the UD file values before execute, which is in differnt list not in ACT.Input list
            List<ObservableList<ActInputValue>> list = act.GetInputValueListForVEProcessing();
            if (list != null) // Will happen only if derived action implemented this function, since it needs processing for VEs
            {
                foreach (var subList in list)
                {
                    foreach (var IV in subList)
                    {
                        if (!string.IsNullOrEmpty(IV.Value))
                        {
                            VE.Value = IV.Value;
                            IV.ValueForDriver = VE.ValueCalculated;
                        }
                        else
                        {
                            IV.ValueForDriver = string.Empty;
                        }
                    }
                }
            }
        }

        private void ProcessWait(Act act, Stopwatch st)
        {
            ValueExpression valueExpression = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);

            valueExpression.Value = act.WaitVE;
            if (!String.IsNullOrEmpty(valueExpression.ValueCalculated))
            {
                try
                {
                    act.Wait = Int32.Parse(valueExpression.ValueCalculated);
                }
                catch (System.FormatException ex)
                {
                    act.Wait = 0;
                    act.ExInfo = "Invalid value for Wait time : " + valueExpression.ValueCalculated;
                    Reporter.ToLog(eLogLevel.INFO, "", ex);
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
            act.Status = eStatus;
            act.Elapsed = st.ElapsedMilliseconds;
            act.ElapsedTicks = st.ElapsedTicks;

            if (GiveUserFeedback)
            {
                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                Thread.Sleep(10);
            }
        }

        private void executeErrorAndPopUpHandler(ObservableList<ErrorHandler> errorHandlerActivity)
        {
            
            eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
            foreach (ErrorHandler errActivity in errorHandlerActivity)
            {
                Stopwatch stE = new Stopwatch();
                stE.Start();                
                foreach (Act act in errActivity.Acts)
                {
                    Stopwatch st = new Stopwatch();
                    st.Start();
                    if (act.Active)
                    {
                        if (errActivity.HandlerType == ErrorHandler.eHandlerType.Popup_Handler)
                            act.Timeout = 1;
                        PrepAction(act, ref ActionExecutorType, st);
                        RunActionWithTimeOutControl(act, ActionExecutorType);
                        CalculateActionFinalStatus(act);
                    }
                    st.Stop();
                }
                SetBusinessFlowActivitiesAndActionsSkipStatus();
                CalculateActivityFinalStatus(errActivity);
                stE.Stop();
                errActivity.Elapsed = stE.ElapsedMilliseconds;
            }
        }

        private void PrepAction(Act act, ref eActionExecutorType ActExecutorType, Stopwatch st)
        {
            PrepDynamicVariables();

            ResetAction(act);

            ProcessWait(act, st);

            if (mStopRun)
            {
                UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped, st);
                //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                SetDriverPreviousRunStoppedFlag(true);
                return;
            }
            else
            {
                //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                SetDriverPreviousRunStoppedFlag(false);
            }
            UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Started, st);

            //No need for agent for some actions like DB and read for excel 
            if ((RunInSimulationMode) && (act.SupportSimulation))
            {
                ActExecutorType = eActionExecutorType.RunInSimulationMode;
            }
            else
            {
                if (typeof(ActPlugIn).IsAssignableFrom(act.GetType()))
                    ActExecutorType = eActionExecutorType.RunOnPlugIn;
                else if (typeof(ActWithoutDriver).IsAssignableFrom(act.GetType()))
                    ActExecutorType = eActionExecutorType.RunWithoutDriver;
                else
                    ActExecutorType = eActionExecutorType.RunOnDriver;
            }

            PrepActionVE(act);


            UpdateActionStatus(act, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running, st);
            if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);

        }

        public void PrepActionVE(Act act)
        {
            ValueExpression VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);
            if (act.LocateValue != null)
            {
                VE.Value = act.LocateValue;
                act.LocateValueCalculated = VE.ValueCalculated;
            }

            ProcessInputValueForDriver(act);

            ProcessReturnValueForDriver(act);
            
            //TODO: remove from here
            if (act.GetType() == typeof(ActGenerateFileFromTemplate))
                ((ActGenerateFileFromTemplate)act).VE = VE;
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
                            ActScreenShot ASS = new ActScreenShot();
                            ASS.LocateBy = eLocateBy.NA;
                            ASS.WindowsToCapture = act.WindowsToCapture;

                            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                                ASS.WindowsToCapture = ActScreenShot.eWindowsToCapture.AllAvailableWindows;
                            Agent a = CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                            if (a == null)
                            {
                                msg = "Missing Agent for taking screen shot for the action: '" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += Environment.NewLine + msg;
                            }
                            else if (a.Status != Agent.eStatus.Running)
                            {
                                msg = "Screenshot not captured because agent is not running for the action:'" + act.Description + "'";
                                Reporter.ToLog(eLogLevel.WARN, msg);
                                act.ExInfo += Environment.NewLine + msg;
                            }
                            else
                            {
                                a.RunAction(ASS);//TODO: Use IVisal driver to get screen shot instead of running action                         
                                if (string.IsNullOrEmpty(ASS.Error))//make sure the screen shot succeed
                                {
                                    act.ScreenShots.AddRange(ASS.ScreenShots);
                                    act.ScreenShotsNames.AddRange(ASS.ScreenShotsNames);
                                }
                                else
                                {
                                    act.ExInfo += Environment.NewLine + ASS.Error;
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
                screenShotsPaths = GingerCore.General.TakeDesktopScreenShot(true);
                if (screenShotsPaths == null)
                {
                    if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)//log the error only if user asked for desktop screen shot to avoid confution 
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
                if (act.WindowsToCapture == Act.eWindowsToCapture.DesktopScreen)//log the error only if user asked for desktop screen shot to avoid confution 
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
                    GingerCore.Drivers.DriverBase driver = CurrentBusinessFlow.CurrentActivity.CurrentAgent.Driver;
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

                Agent currentAgent = CurrentBusinessFlow.CurrentActivity.CurrentAgent;
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
                                else if (currentAgent.Status != Agent.eStatus.Running)
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
                                    CurrentBusinessFlow.CurrentActivity.CurrentAgent.RunAction(act);
                                }
                            }
                            break;

                        case eActionExecutorType.RunWithoutDriver:
                            RunWithoutAgent(act);
                            break;

                        case eActionExecutorType.RunOnPlugIn:
                            ExecutePlugInAction((ActPlugIn)act);
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
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Cancelling;
                    act.Error += "Timeout Occurred, Elapsed > " + act.Timeout;
                    if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                }
                //TODO: remove specific action type make it flag on action 
                if (currentAgent != null && currentAgent.Driver != null && currentAgent.DriverType == Agent.eDriverType.UnixShell)
                    ((GingerCore.Drivers.ConsoleDriverLib.ConsoleDriverBase)currentAgent.Driver).taskFinished = true;
                else if (currentAgent != null && (currentAgent.DriverType == Agent.eDriverType.PowerBuilder || currentAgent.DriverType == Agent.eDriverType.WindowsAutomation))
                {
                    if(currentAgent.DriverType == Agent.eDriverType.WindowsAutomation)                    
                        ((GingerCore.Drivers.WindowsLib.WindowsDriver)currentAgent.Driver).mUIAutomationHelper.taskFinished = true;
                    else
                        ((GingerCore.Drivers.PBDriver.PBDriver)currentAgent.Driver).mUIAutomationHelper.taskFinished = true;
                    if (!String.IsNullOrEmpty(act.Error) && act.Error.StartsWith("Time out !"))
                        Thread.Sleep(1000);
                }
                    
            }
            catch (Exception e)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.Error = e.Message + Environment.NewLine + e.InnerException;
            }
            finally
            {
                if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
            }
        }


        private bool ExecuteActionWithTimeLimit(Act act, TimeSpan timeSpan, Action codeBlock)
        {
            Stopwatch st = new Stopwatch();
            int count = 0;
            st.Start();

            //TODO: Cancel the task after timeout
            try
            {            
                Task task = Task.Factory.StartNew(() => codeBlock());
                // Adaptive sleep, first one second second sleep 10ms, then 50ms, more than one sec do 1000ms
                int Sleep = 10;
                while (!task.IsCompleted)
                {
                    if (st.ElapsedMilliseconds > 500)
                    {
                        Sleep = 100;
                    }
                    
                    Thread.Sleep(Sleep);
                    count += Sleep;

                    if (count > 1000)
                    {
                        OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                        count = 0;
                    }

                    // give user feedback every 200ms
                    if (GiveUserFeedback && count > 200)
                    {
                        act.Elapsed = st.ElapsedMilliseconds;
                        OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                        count = 0;
                    }

                    if (st.Elapsed > timeSpan)
                    {                          
                        if (String.IsNullOrEmpty(act.Error))
                            act.Error = "Time out !";                        
                        break;
                    }

                    if (mStopRun)
                    {
                        act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                        act.ExInfo += "Stopped";
                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        //TODO: J.G: Enhance the mechanism to notify the Driver that action is stopped. Today driver still running the action unitl timeout even after stopping it.
                        SetDriverPreviousRunStoppedFlag(true);                   
                        break;
                    }
                    else
                    {
                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
                        SetDriverPreviousRunStoppedFlag(false);
                    }
                }
                return true;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }
        }

        private void RunActionInSimulationMode(Act act, bool checkIfActionAllowedToRun = true)
        {
            if (act.ReturnValues.Count == 0)
                return;

            ValueExpression VE = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);
            foreach (ActReturnValue item in act.ActReturnValues)
            {
                if (item.SimulatedActual != null)
                {
                    VE.Value = item.SimulatedActual;
                    item.Actual = VE.ValueCalculated;
                }
            }

            act.ExInfo += "Executed in Simulation Mode";
        }

        public void SetCurrentActivityAgent()
        {
            // We take it based on the Activity target App
            string AppName = CurrentBusinessFlow.CurrentActivity.TargetApplication;
            //For unit test cases, solution applicaitons will be always null
            if (SolutionApplications != null)
            {
                if (SolutionApplications.Where(x => x.AppName == AppName && x.Platform == ePlatformType.NA).FirstOrDefault() != null)
                    return;
            }
            if (string.IsNullOrEmpty(AppName))
            {
                // If we don't have Target App on activity then take first App from BF
                if (CurrentBusinessFlow.TargetApplications.Count() > 0)
                    AppName = CurrentBusinessFlow.TargetApplications[0].AppName;
            }

            if (string.IsNullOrEmpty(AppName))
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Please select Target Application for the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and " + GingerDicser.GetTermResValue(eTermResKey.Activity));
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            ApplicationAgent AA = (from x in ApplicationAgents where x.AppName == AppName select x).FirstOrDefault();
            if (AA == null || AA.Agent == null)
            {

                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "The current target application, " + AppName + ", doesn't have a mapped agent assigned to it");
                CurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
                return;
            }

            AA.Agent.BusinessFlow = CurrentBusinessFlow;
            // Verify the Agent for the action is running 
            Agent.eStatus agentStatus = AA.Agent.Status;
            if (agentStatus != Agent.eStatus.Running && agentStatus != Agent.eStatus.Starting && agentStatus != Agent.eStatus.FailedToStart)
            {
                StartAgent(AA.Agent);
            }

            CurrentBusinessFlow.CurrentActivity.CurrentAgent = AA.Agent;
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
                    GingerCore.ValueExpression.Calculate(ProjEnvironment, CurrentBusinessFlow, item.StoreToValue, DSList, true, item.Actual);
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

            AWD.RunOnEnvironment = ProjEnvironment;
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


        private void ExecutePlugInAction(ActPlugIn actPlugin)     
        {
            // first verify we have service ready or start service
            Stopwatch st = Stopwatch.StartNew();
            GingerNodeInfo GNI = GetGingerNode(actPlugin);

            if (GNI == null)
            {
                // call plugin to start service and wait for ready
                WorkSpace.Instance.PlugInsManager.StartService(actPlugin.PluginId);  

                Stopwatch stopwatch = Stopwatch.StartNew();
                while (GNI == null && stopwatch.ElapsedMilliseconds < 30000)  // max 30 seconds for service to start
                {
                    Thread.Sleep(500);
                    GNI = GetGingerNode(actPlugin);
                }
                if (GNI == null)
                {
                    throw new Exception("Timeout waiting for service to start");
                }
            }

            GNI.Status = "Reserved";

            // Pack the action to payload
            NewPayLoad p = CreateActionPayload(actPlugin);

            GingerNodeProxy GNP = new GingerNodeProxy(GNI);
            
            GNP.GingerGrid = WorkSpace.Instance.LocalGingerGrid; // FIXME for remote grid

            NewPayLoad RC = GNP.RunAction(p);

            // After we send it we parse the driver response

            if (RC.Name == "ActionResult")
            {
                // We read the ExInfo, Err and output params
                actPlugin.ExInfo = RC.GetValueString();
                string error = RC.GetValueString();
                if (!string.IsNullOrEmpty(error))
                {
                    actPlugin.Error += error;
                }

                List<NewPayLoad> OutpuValues = RC.GetListPayLoad();
                foreach (NewPayLoad OPL in OutpuValues)
                {
                //    //TODO: change to use PL AddValueByObjectType

                //    // it is param name, type and value
                    string PName = OPL.GetValueString();
                    string mOutputValueType = OPL.GetValueEnum();
                    
                    switch (mOutputValueType)
                    {
                        case nameof(OutputValueType.String):
                            string v = OPL.GetValueString();
                            //GA.Output.Values.Add(new NodeActionOutputValue() { Param = PName, ValueString = v });
                            actPlugin.ReturnValues.Add(new ActReturnValue() { Param = PName, Actual = v});
                            break;
                        case nameof(OutputValueType.ByteArray):
                            byte[] b = OPL.GetBytes();
                            // GA.Output.Values.Add(new NodeActionOutputValue() { Param = PName, ValueByteArray = b });
                            actPlugin.ReturnValues.Add(new ActReturnValue() { Param = PName, Actual = "aaaaaaa" });   //FIXME!!! when act can have values types
                            break;
                        default:
                            throw new Exception("Unknown param type: " + mOutputValueType);
                    }
                }
            }
            else
            {
                // The RC is not OK when we faced some unexpected exception 
                //TODO: 
                string Err = RC.GetValueString();
                actPlugin.Error += Err;
            }

            GNI.IncreaseActionCount();
            GNI.Status = "Ready";

            st.Stop();
            long millis = st.ElapsedMilliseconds;
            actPlugin.ExInfo += Environment.NewLine + millis;
        }

        private GingerNodeInfo GetGingerNode(ActPlugIn actPlugin)
        {
            GingerGrid gingerGrid = WorkSpace.Instance.LocalGingerGrid;

            GingerNodeInfo GNI = (from x in gingerGrid.NodeList
                                    where x.ServiceId == actPlugin.ServiceId
                                         && x.Status == "Ready"
                                    select x).FirstOrDefault();

            return GNI;
        }

        // temp public
        private NewPayLoad CreateActionPayload(ActPlugIn ActPlugIn)
        {
            // Here we decompose the GA and create Payload to transfer it to the agent
            NewPayLoad PL = new NewPayLoad("RunAction");
            PL.AddValue(ActPlugIn.GingerActionId);
            List<NewPayLoad> Params = new List<NewPayLoad>();
            foreach (ActInputValue AP in ActPlugIn.InputValues)
            {
                // Why we need GA?
                if (AP.Param == "PluginID" || AP.Param == "GA") continue;
                // TODO: use const
                NewPayLoad p = new NewPayLoad("P");   // To save network trafic we send just one letter
                p.AddValue(AP.Param);
                p.AddValue(AP.ValueForDriver.ToString());
                p.ClosePackage();
                Params.Add(p);
            }

            PL.AddListPayLoad(Params);
            PL.ClosePackage();
            return PL;
            //// TODO: use function which goes to local grid or remote grid
            //NewPayLoad RC = SendRequestPayLoad(PL);

            //// After we send it we parse the driver response

            //if (RC.Name == "ActionResult")
            //{
            //    // We read the ExInfo, Err and output params
            //    GA.ExInfo = RC.GetValueString();
            //    string Error = RC.GetValueString();
            //    if (!string.IsNullOrEmpty(Error))
            //    {
            //        GA.AddError("Driver", Error);   // We need to get Error even if Payload is OK - since it might be in
            //    }

            //    List<NewPayLoad> OutpuValues = RC.GetListPayLoad();
            //    foreach (NewPayLoad OPL in OutpuValues)
            //    {
            //        //TODO: change to use PL AddValueByObjectType

            //        // it is param name, type and value
            //        string PName = OPL.GetValueString();
            //        string mOutputValueType = OPL.GetValueEnum();

            //        switch (mOutputValueType)
            //        {
            //            case nameof(OutputValueType.String):
            //                string v = OPL.GetValueString();
            //                GA.Output.Values.Add(new ActionOutputValue() { Param = PName, ValueString = v });
            //                break;
            //            case nameof(OutputValueType.ByteArray):
            //                byte[] b = OPL.GetBytes();
            //                GA.Output.Values.Add(new ActionOutputValue() { Param = PName, ValueByteArray = b });
            //                break;
            //            default:
            //                throw new Exception("Unknown param type: " + mOutputValueType);
            //        }
            //    }
            //}
            //else
            //{
            //    // The RC is not OK when we faced some unexpected exception 
            //    //TODO: 
            //    string Err = RC.GetValueString();
            //    GA.AddError("RunAction", Err);
            //}
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
                ValueExpression VE = new ValueExpression(this.ProjEnvironment, this.CurrentBusinessFlow, this.DSList);

                foreach (FlowControl FC in act.FlowControls)
                {
                    if (FC.Active == false)
                    {
                        FC.Status = FlowControl.eStatus.Skipped;
                        continue;
                    }

                    FC.CalculateCondition(CurrentBusinessFlow, ProjEnvironment, act, this.DSList);

                    //TODO: Move below condition inside calucate condition once move execution logger to Ginger core

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
                    FC.CalcualtedValue(CurrentBusinessFlow, ProjEnvironment, this.DSList);

                    string rc = VBS.ExecuteVBSEval(FC.ConditionCalculated.Trim());

                    bool IsConditionTrue;
                    if (rc == "-1")
                    {
                        FC.ConditionCalculated += " is True";
                        IsConditionTrue = true;
                    }
                    else
                    {
                        FC.ConditionCalculated += " is False";
                        IsConditionTrue = false;
                    }

                    if (IsConditionTrue)
                    {
                        //Perform the action as condition is true
                        switch (FC.FlowControlAction)
                        {
                            case FlowControl.eFlowControlAction.MessageBox:
                                VE.Value = FC.Value;                                
                                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, VE.ValueCalculated);
                                break;
                            case FlowControl.eFlowControlAction.GoToAction:
                                if (GotoAction(FC, act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                break;
                            case FlowControl.eFlowControlAction.GoToNextAction:
                                if (FlowControlGotoNextAction(act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                break;
                            case FlowControl.eFlowControlAction.GoToActivity:
                            case FlowControl.eFlowControlAction.GoToActivityByName:
                                if (GotoActivity(FC, act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                break;
                            case FlowControl.eFlowControlAction.GoToNextActivity:
                                if (FlowControlGotoNextActivity(act))
                                    IsStopLoop = true;
                                else
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                break;
                            case FlowControl.eFlowControlAction.RerunAction:
                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                                IsStopLoop = true;
                                break;
                            case FlowControl.eFlowControlAction.RerunActivity:
                                ResetActivity(CurrentBusinessFlow.CurrentActivity);
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts[0];
                                IsStopLoop = true;
                                break;
                            case FlowControl.eFlowControlAction.StopBusinessFlow:
                                mStopBusinessFlow = true;
                                CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                IsStopLoop = true;
                                break;
                            case FlowControl.eFlowControlAction.FailActionAndStopBusinessFlow:
                                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                                act.Error = "Failed due to Flow Control rule";
                                act.ExInfo += FC.ConditionCalculated;
                                mStopBusinessFlow = true;
                                CurrentBusinessFlow.CurrentActivity = CurrentBusinessFlow.Activities.LastOrDefault();
                                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                                CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = CurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
                                IsStopLoop = true;
                                break;
                            case FlowControl.eFlowControlAction.StopRun:
                                StopRun();
                                IsStopLoop = true;
                                break;
                            case FlowControl.eFlowControlAction.SetVariableValue:
                                try
                                {
                                    VE.Value = FC.Value;
                                    string[] vals = VE.ValueCalculated.Split(new char[] { '=' });
                                    if (vals.Count() == 2)
                                    {
                                        ActSetVariableValue setValueAct = new ActSetVariableValue();
                                        setValueAct.VariableName = vals[0];
                                        setValueAct.SetVariableValueOption = ActSetVariableValue.eSetValueOptions.SetValue;
                                        setValueAct.Value = vals[1];
                                        setValueAct.RunOnBusinessFlow = this.CurrentBusinessFlow;
                                        setValueAct.DSList = this.DSList;
                                        setValueAct.Execute();
                                    }
                                    else
                                    {
                                        FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to do Set Variable Value Flow Control", ex);
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                }
                                break;
                            case FlowControl.eFlowControlAction.RunSharedRepositoryActivity:
                                try
                                {
                                    if (RunSharedRepositoryActivity(FC))
                                        IsStopLoop = true;
                                    else
                                        FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to do RunSharedRepositoryActivity Flow Control", ex);
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                }
                                break;

                            default:
                                //TODO:
                                break;
                        }

                        if (FC.Status == FlowControl.eStatus.Pending)
                        {
                            FC.Status = FlowControl.eStatus.Action_Executed;
                        }
                    }
                    else
                    {
                        FC.Status = FlowControl.eStatus.Action_Not_Executed;
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
                            // if exceution has been stopped externally, stop at current action
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured in DoFlowControl", ex);
            }
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
            Act a = CurrentBusinessFlow.CurrentActivity.GetAct(fc.GetGuidFromValue(), fc.GetNameFromValue());

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
            Activity sharedActivity = SolutionLocalRepository.GetSolutionRepoActivities().Where(x => x.ActivityName.ToUpper() == activityName).FirstOrDefault();

            if (sharedActivity != null)
            {
                //add activity
                Activity sharedActivityInstance = (Activity)sharedActivity.CreateInstance();
                sharedActivityInstance.Active = true;
                sharedActivityInstance.AddDynamicly = true;
                CurrentBusinessFlow.SetActivityTargetApplication(sharedActivityInstance);
                CurrentBusinessFlow.AddActivity(sharedActivityInstance);
                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DynamicActivityWasAddedToBusinessflow, CurrentBusinessFlow);
                //move activity after current activity
                int aIndex = CurrentBusinessFlow.Activities.IndexOf(sharedActivityInstance);
                int currentIndex = CurrentBusinessFlow.Activities.IndexOf(CurrentBusinessFlow.CurrentActivity);
                CurrentBusinessFlow.Activities.Move(aIndex, currentIndex + 1);

                //set it as next activity to run
                CurrentBusinessFlow.CurrentActivity = sharedActivityInstance;
                CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
                sharedActivityInstance.Acts.CurrentItem = sharedActivityInstance.Acts.FirstOrDefault();

                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + fc.GetNameFromValue() + "' was not found in Shared Repository");
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
            foreach (ActReturnValue ARC in act.ReturnValues)
            {
                //if expected is empty then no check and mark it NA
                if (String.IsNullOrEmpty(ARC.Expected))
                {
                    ARC.Status = ActReturnValue.eStatus.NA;
                }
                else if (!ARC.Active)
                {
                    ARC.Status = ActReturnValue.eStatus.Skipped;
                }
                else
                {
                    //get Expected Calculated
                    ValueExpression ve = new ValueExpression(ProjEnvironment, CurrentBusinessFlow, DSList);
                    ve.Value = ARC.Expected;
                    //replace {Actual} placce holder with real Actual value
                    if (ve.Value.Contains("{Actual}"))
                    {
                        //Replace to 
                        if ((ARC.Actual != null) && GingerCore.General.IsNumeric(ARC.Actual))
                        {
                            ve.Value = ve.Value.Replace("{Actual}", ARC.Actual);
                        }
                        else
                        {
                            ve.Value = ve.Value.Replace("{Actual}", "\"" + ARC.Actual + "\"");
                        }
                    }
                    //calculate the expected value
                    ARC.ExpectedCalculated = ve.ValueCalculated;

                    //calculate Model Parameter expected value
                    CalculateModelParameterExpectedValue(act, ARC);

                    //compare Actual vs Expected (calcualted)
                    CalculateARCStatus(ARC);

                    if (ARC.Status == ActReturnValue.eStatus.Failed)
                    {
                        string formatedExpectedCalculated = ARC.ExpectedCalculated;
                        if (ARC.ExpectedCalculated.Length >= 9 && (ARC.ExpectedCalculated.Substring(ARC.ExpectedCalculated.Length - 9, 9)).Contains("is False"))
                            formatedExpectedCalculated = ARC.ExpectedCalculated.ToString().Substring(0, ARC.ExpectedCalculated.Length - 9);

                        act.Error += "Output Value validation failed for the Parameter '" + ARC.Param + "' , Expected value is " + formatedExpectedCalculated + " while Actual value is '" + ARC.Actual +"'"+ System.Environment.NewLine;
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
            if (act.StatusConverter == Act.eStatusConverterOptions.None)
            {
                return;
            }
            if (act.StatusConverter == Act.eStatusConverterOptions.AlwaysPass)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
            else if (act.StatusConverter == Act.eStatusConverterOptions.IgnoreFail && act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored;
            }
            else if (act.StatusConverter == Act.eStatusConverterOptions.InvertStatus)
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
        
        private void CalculateModelParameterExpectedValue(Act act, ActReturnValue ARC)
        {
            //TODO: make all of this to be generic and not per action type
            if (act is GingerCore.Actions.WebServices.WebAPI.ActWebAPIModel == false) return;

            if (ARC.ExpectedCalculated.Contains("AppModelParam"))
            {
                GingerCore.Actions.WebServices.WebAPI.ActWebAPIModel modelAct = (GingerCore.Actions.WebServices.WebAPI.ActWebAPIModel)act;
                List<AppModelParameter> usedParams = modelAct.ActAppModelParameters.Where(x => ARC.ExpectedCalculated.Contains(x.PlaceHolder)).ToList();
                foreach (AppModelParameter param in usedParams)
                {
                    ARC.ExpectedCalculated = ARC.ExpectedCalculated.Replace(("{AppModelParam Name = " + param.PlaceHolder + "}"), param.ExecutionValue);
                }
            }
        }

        public static void CalculateARCStatus(ActReturnValue ARC)
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
            
            //TODO: docuemnt in help, maybe remove this compare takes time and not sure if needed/use case!?
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
            
            if (ARC.ExpectedCalculated.ToUpper().Trim() == "TRUE" && ARC.Actual.ToUpper().Trim() != "TRUE")
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
            if (GingerCore.General.IsNumeric(ARC.Actual))
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

        public async Task<int> RunActivityAsync(Activity activity, bool Continue=false )
        {
            var result = await Task.Run(() => {
                RunActivity(activity, false);
                return 1;  
            });
            return result;
        }

        public void RunActivity(Activity Activity, bool doContinueRun = false)
        {
            bool statusCalculationIsDone = false;

            //check if Activity is allowed to run
            if (CurrentBusinessFlow == null ||
                    Activity.Acts.Count == 0 || //no Actions to run
                        Activity.GetType() == typeof(ErrorHandler) ||//don't run error handler from RunActivity
                            Activity.CheckIfVaribalesDependenciesAllowsToRun(CurrentBusinessFlow, true) == false || //Variables-Dependencies not allowing to run
                                (FilterExecutionByTags == true && CheckIfActivityTagsMatch() == false))//add validation for Ginger runner tags
            {
                CalculateActivityFinalStatus(Activity);
                return;
            }

            // handling ActivityGroup execution
            ActivitiesGroup currentActivityGroup = CurrentBusinessFlow.ActivitiesGroups.Where(x => x.ActivitiesIdentifiers.Select(z => z.ActivityGuid).ToList().Contains(Activity.Guid)).FirstOrDefault();
            if (currentActivityGroup != null)
            {
                switch (currentActivityGroup.ExecutionLoggerStatus)
                {
                    case ActivitiesGroup.executionLoggerStatus.NotStartedYet:
                        currentActivityGroup.ExecutionLoggerStatus = ActivitiesGroup.executionLoggerStatus.StartedNotFinishedYet;
                        ExecutionLogger.ActivityGroupStart(currentActivityGroup, CurrentBusinessFlow);
                        break;
                    case ActivitiesGroup.executionLoggerStatus.StartedNotFinishedYet:
                        // do nothing
                        break;
                    case ActivitiesGroup.executionLoggerStatus.Finished:
                        // do nothing
                        break;
                }
            }
            
            //add validation for Ginger runner tags
            if (FilterExecutionByTags)
                if (CheckIfActivityTagsMatch() == false) return;

            if (!doContinueRun)
            {
                // We reset the activitiy unless we are in continue mode where user can start from middle of Activity
                ResetActivity(CurrentBusinessFlow.CurrentActivity);
            }
            else
            {
                // since we are in continue mode - only for first activity of continue mode
                // Just change the status to Pending
                CurrentBusinessFlow.CurrentActivity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;

                ContinueTimerVariables(CurrentBusinessFlow.CurrentActivity.Variables);
            }

            

            //Do not disable the following two lines. thse helping the FC run proper activities
            CurrentBusinessFlow.Activities.CurrentItem = CurrentBusinessFlow.CurrentActivity;
            CurrentBusinessFlow.PropertyChanged += CurrentBusinessFlow_PropertyChanged;

            mStopRun = false;//needed with out check for standalone execution???
            mStopBusinessFlow = false;
            Stopwatch st = new Stopwatch();
            mCurrentActivityChanged = false;

            //Run the Activity
            CurrentBusinessFlow.CurrentActivity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
            OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActivityStart, null);
            if (SolutionApplications != null && SolutionApplications.Where(x => (x.AppName == Activity.TargetApplication && x.Platform == ePlatformType.NA)).FirstOrDefault() == null)
            {
                //load Agent only if Activity includes Actions which needs it
                List<Act> driverActs = Activity.Acts.Where(x => (x is ActWithoutDriver && x.GetType() != typeof(ActAgentManipulation)) == false && x.Active == true).ToList();
                if (driverActs.Count >0)
                {
                    //make sure not running in Simulation mode
                    if (!RunInSimulationMode ||
                        (RunInSimulationMode==true && driverActs.Where(x => x.SupportSimulation == false).ToList().Count() > 0))
                    {
                        //Set the Agent to run actions with  
                        SetCurrentActivityAgent();
                    }
                }
            }

            Activity.ExecutionLogActionCounter = 0;
            ExecutionLogger.ActivityStart(CurrentBusinessFlow, Activity);

            //Run the Activity Actions
            st.Start();

            try
            {
                Act act = null;

                // if it is not continue mode then goto first Action
                if (!doContinueRun)
                    act = Activity.Acts.FirstOrDefault();
                else
                    act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;

                bool bHasMoreActions = true;
                while (bHasMoreActions)
                {
                    CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;

                    if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                    if (act.Active && act.CheckIfVaribalesDependenciesAllowsToRun(Activity, true) == true)
                    {
                        RunAction(act, false);
                        if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null); 
                        if (mCurrentActivityChanged)
                        {
                            CurrentBusinessFlow.CurrentActivity.Elapsed = st.ElapsedMilliseconds;
                            if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                            {
                                Activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

                                if (Activity.ActionRunOption == Activity.eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
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
                            Activity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

                            if (Activity.ActionRunOption == Activity.eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
                            {
                                SetNextActionsBlockedStatus();
                                statusCalculationIsDone = true;
                                return;
                            }
                        }
                        if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                        // If the user selected slower speed then do wait
                        if (mSpeed > 0)
                        {
                            // TODO: sleep 100 and do events
                            Thread.Sleep(mSpeed * 1000);
                        }

                        if (mStopRun || mStopBusinessFlow)
                        {
                            CalculateActivityFinalStatus(Activity);
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
                            act.ExInfo = "Action is not active.";
                        }
                        if (!Activity.Acts.IsLastItem())
                        {
                            GotoNextAction();
                            ((Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                        }
                    }

                    act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                    // As long as we have more action we keep the loop, until no more actions available
                    if (act.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending && Activity.Acts.IsLastItem())
                    {
                        bHasMoreActions = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // temporary handling of exception
                SetNextActionsBlockedStatus();
                ExecutionLogger.ActivityEnd(CurrentBusinessFlow, Activity);

                
                //TODO: Throw execption don't cover in log, so user will see it in report
                Reporter.ToLog(eLogLevel.ERROR, "Run Activity got error ", ex);
                throw ex;
            }
            finally
            {
                st.Stop();
                Activity.Elapsed = st.ElapsedMilliseconds;
                if (!statusCalculationIsDone)
                {
                    CalculateActivityFinalStatus(Activity);
                }
                PostScopeVariableHandling(Activity.Variables);
                ExecutionLogger.ActivityEnd(CurrentBusinessFlow, Activity);
                mLastExecutedActivity = Activity;
                if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
                AutoLogProxy.LogActivity(CurrentBusinessFlow, Activity);

                // handling ActivityGroup execution 
                if (currentActivityGroup != null)
                {
                    switch (currentActivityGroup.ExecutionLoggerStatus)
                    {
                        case ActivitiesGroup.executionLoggerStatus.NotStartedYet:
                            // do nothing
                            break;
                        case ActivitiesGroup.executionLoggerStatus.StartedNotFinishedYet:
                            if (currentActivityGroup.ExecutedActivities.ContainsKey(Activity.Guid))
                            {
                                currentActivityGroup.ExecutedActivities[Activity.Guid] = DateTime.Now.ToUniversalTime();
                            }
                            else
                            {
                                currentActivityGroup.ExecutedActivities.Add(Activity.Guid, DateTime.Now.ToUniversalTime());
                            }
                            // do nothing
                            break;
                        case ActivitiesGroup.executionLoggerStatus.Finished:
                            // do nothing
                            break;
                    }
                }

                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActivityEnd, null);
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

        public bool ContinueRun(eContinueLevel continueLevel, eContinueFrom continueFrom, BusinessFlow specificBusinessFlow = null, Activity specificActivity = null, Act specificAction = null)
        {
            switch (continueFrom)
            {
                case eContinueFrom.LastStoppedAction:
                    if (mExecutedBusinessFlowWhenStopped != null && BusinessFlows.Contains(mExecutedBusinessFlowWhenStopped))
                        CurrentBusinessFlow = mExecutedBusinessFlowWhenStopped;
                    else
                        return false;//can't do continue
                    if (mExecutedActivityWhenStopped != null && mExecutedBusinessFlowWhenStopped.Activities.Contains(mExecutedActivityWhenStopped))
                        CurrentBusinessFlow.CurrentActivity = mExecutedActivityWhenStopped;
                    else
                        return false;//can't do continue
                    if (mExecutedActionWhenStopped != null && mExecutedActivityWhenStopped.Acts.Contains(mExecutedActionWhenStopped))
                        CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = mExecutedActionWhenStopped;
                    else
                        return false;//can't do continue
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
                RunRunner(true);
            else
                RunBusinessFlow(null, true, true);

            return true;
        }
        
        public async Task<int> RunBusinessFlowAsync(BusinessFlow businessFlow, bool standaloneBfExecution = false, bool doContinueRun = false)
        {
            var result = await Task.Run(() =>
            {
                RunBusinessFlow(businessFlow, standaloneBfExecution, doContinueRun);
                return 1;
            });
            return result;
        }

        public void RunBusinessFlow(BusinessFlow businessFlow, bool standaloneExecution = false, bool doContinueRun = false)
        {
           Stopwatch st = new Stopwatch();
            try
            {
                //set Runner details if running in stand alone mode (Automate tab)
                if (standaloneExecution)
                {
                    mIsRunning = true;
                    mStopRun = false;
                }

                //set the BF to execute
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow = businessFlow;
                    Activity bfFirstActivity = CurrentBusinessFlow.Activities.FirstOrDefault();
                    CurrentBusinessFlow.Activities.CurrentItem = bfFirstActivity;
                    CurrentBusinessFlow.CurrentActivity = bfFirstActivity;
                    bfFirstActivity.Acts.CurrentItem = bfFirstActivity.Acts.FirstOrDefault();
                }

                //Init
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow.Reset();
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
                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.BusinessFlowStart, null);
                mStopBusinessFlow = false;
                CurrentBusinessFlow.Elapsed = null;                
                st.Start();

                //Do Run validations
                if (CurrentBusinessFlow.Activities.Count == 0)
                {
                    return;//no Activities to run
                }

                //Do run prepaerations
                SetBusinessFlowInputVarsWithOutputValues(CurrentBusinessFlow);
                UpdateLastExecutingAgent();
                CurrentBusinessFlow.Environment = ProjEnvironment == null ? "" : ProjEnvironment.Name;
                PrepDynamicVariables();
                
                //Start execution
                if (doContinueRun == false)
                {
                    CurrentBusinessFlow.ExecutionLogActivityCounter = 0;
                    ExecutionLogger.BusinessFlowStart(CurrentBusinessFlow);
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
                        ExecutingActivity.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running;
                        if (GiveUserFeedback) OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.DoEventsRequired, null);
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
                            //If not equal means flow control updatet current item to target activity, no need to do next activity
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
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected error occurred during the execution of the '{0}' Business Flow", CurrentBusinessFlow), ex);
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
                
                if (doContinueRun == false)
                {
                    ExecutionLogger.BusinessFlowEnd(CurrentBusinessFlow);
                }

                if (standaloneExecution)
                {
                    mIsRunning = false;
                    Status = RunsetStatus;            
                }
                OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.BusinessFlowEnd, null);
                AutoLogProxy.LogBusinessFlow(CurrentBusinessFlow);
            }
        }

        private void UpdateLastExecutingAgent()
        {
            foreach (TargetApplication TA in CurrentBusinessFlow.TargetApplications)
            {
                string a = (from x in ApplicationAgents where x.AppName == TA.AppName select x.AgentName).FirstOrDefault();
                TA.LastExecutingAgentName = a;
            }
        }

        public void CalculateBusinessFlowFinalStatus(BusinessFlow BF, bool considrePendingAsSkipped= false)
        {
            // A flow is blocked if some activitiy failed and all the activities after it failed
            // A Flow is failed if one or more activities failed

            // Add Blocked
            bool Failed = false;
            bool Blocked = false;
            bool Stopped = false;

            // Assume pass unless error
            BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;

            if (BF.Activities.Count == 0 ||
                BF.Activities.Where(x=>x.GetType()== typeof(Activity) && x.Status== Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped).ToList().Count == BF.Activities.Where(x => x.GetType() == typeof(Activity)).ToList().Count)               
            {
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                return;
            }

            if (considrePendingAsSkipped &&
                BF.Activities.Where(x => x.GetType() == typeof(Activity) && x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending).ToList().Count == BF.Activities.Where(x => x.GetType() == typeof(Activity)).ToList().Count)
            {
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                return;
            }
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
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
            }
            else if(Failed)
            {
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            else if(Blocked)
            {
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
            }
            else
            {
                BF.RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
        }

        public void CalculateActivitiesGroupFinalStatus(ActivitiesGroup AG, BusinessFlow BF)
        {
            // A flow is blocked if some activitiy failed and all the activities after it failed
            // A Flow is failed if one or more activities failed

            // Add Blocked
            bool Failed = false;
            bool Blocked = false;
            bool Stopped = false;

            // Assume pass unless error
            AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Passed;
            

            if (AG.ActivitiesIdentifiers.Count == 0 ||
            AG.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped).ToList().Count == AG.ActivitiesIdentifiers.Count)
            {
                AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Skipped;
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
                AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Blocked;
            }
            else if (Failed)
            {
                AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Failed;
            }
            else if (Stopped)
            {
                AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Stopped;
            }
            else
            {
                AG.RunStatus = ActivitiesGroup.eActivitiesGroupRunStatus.Passed;
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

        public void SetActivityGroupsExecutionStatus(BusinessFlow automateTab = null, bool offlineMode = false, ExecutionLogger executionLogger = null)
        {
            if ((CurrentBusinessFlow == null) && (automateTab != null) && offlineMode)
            {
                CurrentBusinessFlow = automateTab;
                CurrentBusinessFlow.ActivitiesGroups.ToList().ForEach(x => x.ExecutionLoggerStatus = ActivitiesGroup.executionLoggerStatus.StartedNotFinishedYet);
            }
            foreach (ActivitiesGroup currentActivityGroup in CurrentBusinessFlow.ActivitiesGroups)
            {
                CalculateActivitiesGroupFinalStatus(currentActivityGroup, CurrentBusinessFlow);
                if (currentActivityGroup != null)
                {
                    if (currentActivityGroup.RunStatus != ActivitiesGroup.eActivitiesGroupRunStatus.Passed && currentActivityGroup.RunStatus != ActivitiesGroup.eActivitiesGroupRunStatus.Failed && currentActivityGroup.RunStatus != ActivitiesGroup.eActivitiesGroupRunStatus.Stopped)
                    {
                        currentActivityGroup.ExecutionLoggerStatus = ActivitiesGroup.executionLoggerStatus.NotStartedYet;
                    }
                    else
                    {
                        switch (currentActivityGroup.ExecutionLoggerStatus)
                        {
                            case ActivitiesGroup.executionLoggerStatus.NotStartedYet:
                                // do nothing
                                break;
                            case ActivitiesGroup.executionLoggerStatus.StartedNotFinishedYet:
                                currentActivityGroup.ExecutionLoggerStatus = ActivitiesGroup.executionLoggerStatus.Finished;
                                if (executionLogger != null)
                                {
                                    executionLogger.ActivityGroupEnd(currentActivityGroup, CurrentBusinessFlow, offlineMode);
                                }
                                else
                                {
                                    ExecutionLogger.ActivityGroupEnd(currentActivityGroup, CurrentBusinessFlow, offlineMode);
                                }
                                break;
                            case ActivitiesGroup.executionLoggerStatus.Finished:
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
                if (CurrentBusinessFlow.CurrentActivity.Acts.IsLastItem()) break;

                else
                {
                    CurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
                    act = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
                }
            }
        }

        public void SetSpeed(int Speed)
        {
            mSpeed = Speed;
        }

        public void StopRun()
        {
            mStopRun = true;
            
            mExecutedActivityWhenStopped = (Activity)CurrentBusinessFlow.CurrentActivity;
            mExecutedActionWhenStopped = (Act)CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
            mExecutedBusinessFlowWhenStopped = (BusinessFlow)CurrentBusinessFlow;
        }

        public void ResetRunnerExecutionDetails(bool doNotResetBusFlows=false)
        {
            Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
            mStopRun = false;
            mIsRunning = false;
            PublishToALMConfig = null;
            if (doNotResetBusFlows == false)
            {
                foreach (BusinessFlow bf in BusinessFlows)
                {
                    bf.Reset();
                    OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.BusinessflowWasReset, bf);
                }                    
            }            
        }

        internal void CloseAgents()
        {
            foreach (ApplicationAgent p in ApplicationAgents)
            {
                if (p.Agent != null)
                {
                    try
                    {
                        p.Agent.Close();
                    }
                    catch (Exception ex)
                    {
                        if (p.Agent.Name != null)
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Close the '{0}' Agent", p.Agent.Name), ex);
                        else
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Close the Agent", ex);
                    }
                    p.Agent.IsFailedToStart = false;
                }
            }
            AgentsRunning = false;
        }

        internal void ResetFailedToStartFlagForAgents()
        {
            foreach (ApplicationAgent p in ApplicationAgents)
            {
                if (p.Agent != null)
                {                   
                    p.Agent.IsFailedToStart = false;
                }
            }           
        }

        internal void HighlightActElement(Act act)
        {
            if (HighLightElement)
            {
                if (act != null)
                {
                    SetCurrentActivityAgent();
                        Agent a = CurrentBusinessFlow.CurrentActivity.CurrentAgent;
                        if (a != null)
                            a.HighLightElement(act);
                }
            }
        }

        private void ResetActivity(Activity a)
        {
            a.Reset();
        }
        
        internal void ClearAgents()
        {
            CloseAgents();
            ApplicationAgents.Clear();
        }
        
        internal void UpdateApplicationAgents()
        {
            // Make sure Ginger Runner have all Application/Platforms mapped to agent - create the list based on selected BFs to run
            // Make it based on currnt if we run from automate tab
             
            //Get the TargetApplication list
            ObservableList<TargetApplication> bfsTargetApplications = new ObservableList<TargetApplication>();
            if (BusinessFlows.Count() != 0)// Run Tab
            {
                foreach (BusinessFlow BF in BusinessFlows)
                {
                    foreach (TargetApplication TA in BF.TargetApplications)
                    {
                        if (bfsTargetApplications.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
                            bfsTargetApplications.Add(TA);
                    }
                }
            }
            else if (CurrentBusinessFlow != null) // Automate Tab
            {
                foreach (TargetApplication TA in CurrentBusinessFlow.TargetApplications)
                {
                    if (bfsTargetApplications.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
                        bfsTargetApplications.Add(TA);
                }
            }

            //Remove the non relevant ApplicationAgents
            for (int indx = 0; indx < ApplicationAgents.Count; )
            {
                if (bfsTargetApplications.Where(x => x.AppName == ApplicationAgents[indx].AppName).FirstOrDefault() == null)
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
            foreach (TargetApplication TA in bfsTargetApplications)
            {
                // make sure GR got it covered
                if (ApplicationAgents.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
                {
                    ApplicationAgent ag = new ApplicationAgent();
                    ag.AppName = TA.AppName;

                    //Map agent to the application
                    ApplicationPlatform ap=null;
                    if (CurrentSolution.ApplicationPlatforms != null)
                    {
                        ap = CurrentSolution.ApplicationPlatforms.Where(x => x.AppName == ag.AppName).FirstOrDefault();
                    }
                    if (ap != null)
                    {
                        List<Agent> platformAgents = (from p in SolutionAgents where p.Platform == ap.Platform && p.UsedForAutoMapping == false select p).ToList();

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
            //special case when we create new solution and the agent is not mapped
            if (ApplicationAgents != null && ApplicationAgents.Count == 1 &&  SolutionAgents.Count ==1 & ApplicationAgents[0].Agent == null)
            {
                ApplicationAgents[0].Agent = SolutionAgents[0];
            }

            this.OnPropertyChanged(nameof(ApplicationAgents));//to notify who shows this list
        }

        internal ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false,string GingerRunnerName = "")
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
                        BFES.BusinessFlowExecLoggerFolder = this.ExecutionLogger.ExecutionLogfolder + BF.ExecutionLogFolder;

                        BFESs.Add(BFES);
                    }
                }
            }
            return BFESs;
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
            //check if Activity or The parent Acitivites Group has at least 1 tag from filter tags
            //first check Activity
            foreach(Guid tagGuid in CurrentBusinessFlow.CurrentActivity.Tags)
                if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid)==true).FirstOrDefault() != Guid.Empty)
                    return true;

            //check in Activity Group
            if (string.IsNullOrEmpty(CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID) == false)
            {
                ActivitiesGroup group = CurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == CurrentBusinessFlow.CurrentActivity.ActivitiesGroupID).FirstOrDefault();
                if(group != null)
                    foreach (Guid tagGuid in group.Tags)
                        if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid) == true).FirstOrDefault() != Guid.Empty)
                            return true;
            }

            return false;
        }

        private void SetDriverPreviousRunStoppedFlag(bool flagValue)
        {
            if (CurrentBusinessFlow.CurrentActivity.CurrentAgent != null && CurrentBusinessFlow.CurrentActivity.CurrentAgent.Driver != null)
                CurrentBusinessFlow.CurrentActivity.CurrentAgent.Driver.PreviousRunStopped = flagValue;
        }

        //Function to Fo Flow Control on Business Flow in RunSet
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
                    FC.Status = FlowControl.eStatus.Skipped;
                    continue;
                }
                else
                {
                    FC.Status = FlowControl.eStatus.Pending;
                }

                FC.CalculateCondition(CurrentBusinessFlow, ProjEnvironment, this.DSList);

                FC.CalcualtedValue(CurrentBusinessFlow, ProjEnvironment, this.DSList);

                string rc = VBS.ExecuteVBSEval(FC.ConditionCalculated.Trim());

                bool IsConditionTrue;
                if (rc == "-1")
                {
                    FC.ConditionCalculated += " is True";
                    IsConditionTrue = true;
                }
                else
                {
                    FC.ConditionCalculated += " is False";
                    IsConditionTrue = false;
                }

                if (IsConditionTrue)
                {
                    //Perform the action as condition is true
                    switch (FC.BusinessFlowControlAction)
                    {

                        case FlowControl.eBusinessFlowControlAction.GoToBusinessFlow:
                            if (GotoBusinessFlow(FC, bf, ref fcReturnIndex))
                            {
                                IsStopLoop = true;
                            }
                            else
                                FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                            break;

                        case FlowControl.eBusinessFlowControlAction.RerunBusinessFlow:
                            fcReturnIndex = BusinessFlows.IndexOf(bf);
                            IsStopLoop = true;
                            break;

                        case FlowControl.eBusinessFlowControlAction.StopRun:
                            StopRun();
                            IsStopLoop = true;
                            break;
                        case FlowControl.eBusinessFlowControlAction.SetVariableValue:
                            try
                            {
                                VE.Value = FC.Value;
                                string[] vals = VE.ValueCalculated.Split(new char[] { '=' });
                                if (vals.Count() == 2)
                                {
                                    ActSetVariableValue setValueAct = new ActSetVariableValue();
                                    setValueAct.VariableName = vals[0];
                                    setValueAct.SetVariableValueOption = ActSetVariableValue.eSetValueOptions.SetValue;
                                    setValueAct.Value = vals[1];
                                    setValueAct.RunOnBusinessFlow = this.CurrentBusinessFlow;
                                    setValueAct.DSList = this.DSList;
                                    setValueAct.Execute();
                                }
                                else
                                {
                                    FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Set Variable Value Flow Control", ex);
                                FC.Status = FlowControl.eStatus.Action_Execution_Failed;
                            }
                            break;

                        default:
                            //TODO:
                            break;
                    }

                    if (FC.Status == FlowControl.eStatus.Pending)
                    {
                        FC.Status = FlowControl.eStatus.Action_Executed;
                    }
                }
                else
                {
                    FC.Status = FlowControl.eStatus.Action_Not_Executed;
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
                lstBusinessFlow = BusinessFlows.Where(x => x.InstanceGuid == guidToLookBy).ToList();
            
            if (lstBusinessFlow == null || lstBusinessFlow.Count == 0)
                bf = null;
            else if (lstBusinessFlow.Count == 1)
                bf = lstBusinessFlow[0];
            else//we have more than 1
            {
                BusinessFlow firstActive = lstBusinessFlow.Where(x => x.Active == true).FirstOrDefault();
                if (firstActive != null)
                    bf = firstActive;
                else
                    bf = lstBusinessFlow[0];//no one is Active so returning the first one
            }

            if (bf != null)
            {
                bfExecutionIndex = BusinessFlows.IndexOf(bf);
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Business Flow Name not found - " + Name);
                return false;
            }
        }
    }
}
