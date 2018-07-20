//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using amdocs.ginger.GingerCoreNET;
//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.CoreNET.GingerConsoleLib;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.CommandProcessorLib;
//using GingerCoreNET.Dictionaries;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReporterLib;
//using GingerCoreNET.ScriptLib.VBSLib;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.DataSourceLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.EnvironmentsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using GingerCoreNET.UsageTrackingLib;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.ComponentModel;
//using System.Data;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;

//namespace GingerCoreNET.RunLib
//{
//    public class NewGingerRunner : RepositoryItem
//    {
//        public bool AgentsRunning = false;
//        public bool GiveUserFeedback = false;
//        private bool bCurrentActivityChanged = false;
//        private bool mErrorHandled = false;
//        private bool mIsRunning = false;

//        public enum eActionExecutorType
//        {
//            RunWithoutDriver,
//            RunOnDriver,
//            RunInSumulationMode
//        }
//        public string CurrentGingerLogFolder = string.Empty;
//        public string CurrentHTMLReportFolder = string.Empty;

//        public delegate void GingerRunnerEventHandler(GingerRunnerEventArgs EventArgs);
//        public event GingerRunnerEventHandler GingerRunnerEvent;

//        public void OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType EvType, Object obj)
//        {
//            GingerRunnerEventHandler handler = GingerRunnerEvent;
//            if (handler != null)
//            {
//                handler(new GingerRunnerEventArgs(EvType, obj));
//            }
//        }

//        public bool IsRunning
//        {
//            get { return mIsRunning; }
//        }
//        public enum eStatus
//        {
//            Pending = 1,
//            Started = 2,
//            Running = 3,
//            Stopped = 4,
//            Completed = 5
//        }
//        public enum eRunOptions
//        {
//            [EnumValueDescription("Continue Business Flows Run on Failure ")]
//            ContinueToRunall = 0,
//            [EnumValueDescription("Stop Business Flows Run on Failure ")]
//            StopAllBusinessFlows = 1,
//        }

//        public enum eExecutedFrom
//        {
//            Automation,
//            Run
//        }
//        public eExecutedFrom ExecutedFrom;
//        public ExecutionLogger ExecutionLogger;

//        public NewGingerRunner()
//        {
//            ExecutedFrom = eExecutedFrom.Run;
//            ExecutionLogger = new ExecutionLogger(eExecutedFrom.Run);
//            ApplicationAgentsCollectionChanged();
//        }

//        public NewGingerRunner(eExecutedFrom executedFrom)
//        {
//            ExecutedFrom = executedFrom;
//            ExecutionLogger = new ExecutionLogger(ExecutedFrom);
//            ApplicationAgentsCollectionChanged();
//        }

//        //TODO:  temp flag so Beta users will not be impacted, removed when it is working and tested to be good 
//        public static bool UseExeuctionLogger = false;

//        // Whoever generate GR need to give it place to log
//        public string ExecutionLogFolder { get; set; }

//        private BusinessFlow mCurrentBusinessFlow { get; set; }
//        public BusinessFlow CurrentBusinessFlow
//        {
//            get { return mCurrentBusinessFlow; }
//            set
//            {
//                if (mCurrentBusinessFlow != value)
//                {
//                    mCurrentBusinessFlow = value;
//                    UpdateApplicationAgents();
//                }
//            }
//        }

//        private int mSpeed = 0;
//        private bool mStopRun = false;
//        private bool mStopBusinessFlow = false;

//        public string SolutionFolder { get; set; }
//        public DateTime StartRun { get; set; }
//        public DateTime EndRun { get; set; }
//        public bool HighLightElement { get; set; }

//        public double? Elapsed
//        {
//            get
//            {
//                double? T = 0;
//                foreach (BusinessFlow BF in BusinessFlows)
//                {
//                    if (BF.Active && !(BF.RunStatus == eRunStatus.Pending || BF.RunStatus == eRunStatus.Blocked))
//                        T += BF.Elapsed;
//                }
//                return T;
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public eRunOptions RunOption { get; set; }

//        //TODO: delete me once projects moved to new Apps/Platform config, meanwhile enable to load old run set config, but ignore the value
//        public ObservableList<Platform> Platforms = new ObservableList<Platform>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<ApplicationAgent> ApplicationAgents = new ObservableList<ApplicationAgent>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> FilterExecutionTags = new ObservableList<Guid>();

//        public ObservableList<NewAgent> SolutionAgents;
//        public ObservableList<ApplicationPlatform> SolutionApplications;

//        private string mName;
//        [IsSerializedForLocalRepository]
//        public string Name
//        {
//            get { return mName; }
//            set
//            {
//                mName = value;
//                OnPropertyChanged(nameof(Name));
//            }
//        }

//        private bool _Selected { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool Selected
//        {
//            get { return _Selected; }
//            set
//            {
//                if (_Selected != value)
//                {
//                    _Selected = value;
//                    OnPropertyChanged("Selected");
//                }
//            }
//        }

//        public eStatus mStatus;
//        public eStatus Status
//        {
//            get { return mStatus; }
//            set
//            {
//                if (mStatus != value)
//                {
//                    mStatus = value;
//                    OnPropertyChanged(nameof(Status));
//                }
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public bool UseSpecificEnvironment { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SpecificEnvironmentName { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool FilterExecutionByTags { get; set; }

//        public ProjEnvironment ProjEnvironment { get; set; }

//        public SolutionRepository SolutionRepository { get; set; }

//        public ObservableList<DataSourceBase> DSList { get; set; }

//        public bool RunInSimulationMode { get; set; }

//        public void SetExecutionEnvironment(ProjEnvironment defualtEnv, ObservableList<ProjEnvironment> allEnvs)
//        {
//            ProjEnvironment = null;
//            if (UseSpecificEnvironment == true && string.IsNullOrEmpty(SpecificEnvironmentName) == false)
//            {
//                ProjEnvironment specificEnv = (from x in allEnvs where x.Name == SpecificEnvironmentName select x).FirstOrDefault();
//                if (specificEnv != null)
//                    ProjEnvironment = specificEnv;
//            }

//            if (ProjEnvironment == null)
//                ProjEnvironment = defualtEnv;
//        }

//        public Solution CurrentSolution { get; set; }

//        [IsSerializedForLocalRepository]
//        public ObservableList<BusinessFlowRun> BusinessFlowsRunList = new ObservableList<BusinessFlowRun>();

//        public void UpdateBusinessFlowsRunList()
//        {
//            BusinessFlowsRunList.Clear();
//            foreach (BusinessFlow bf in BusinessFlows)
//            {
//                BusinessFlowRun BFR = new BusinessFlowRun();
//                BFR.BusinessFlowName = bf.Name;
//                BFR.BusinessFlowGuid = bf.Guid;
//                BFR.BusinessFlowIsActive = bf.Active;

//                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(true))
//                    if (var.DiffrentFromOrigin == true || string.IsNullOrEmpty(var.MappedOutputValue) == false)//save only variables which were modified in this run configurations
//                        BFR.BusinessFlowCustomizedRunVariables.Add(var);
//                BFR.BusinessFlowRunDescription = bf.RunDescription;

//                BusinessFlowsRunList.Add(BFR);
//            }
//        }

//        public ObservableList<BusinessFlow> BusinessFlows = new ObservableList<BusinessFlow>();

//        public void Run()
//        {
//            StartRun = DateTime.Now;
//            Status = eStatus.Started;
//            mIsRunning = true;
//            mStopRun = false;
//            if (BusinessFlows.Count() == 0) return;

//            mCurrentBusinessFlow = BusinessFlows[0];

//            UpdateApplicationAgents();

//            Status = eStatus.Running;

//            ExecutionLogger.GingerStart();
//            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.Active == true))
//            {
//                //stop if needed before executing next BF
//                if (mStopRun)
//                    break;

//                //do execution preparations
//                mCurrentBusinessFlow = bf;
//                SetBusinessFlowInputVarsWithOutputValues(bf);

//                //Execute the Business Flow
//                RunFlow();

//                //stop if needed based on current BF failure
//                if (RunOption == eRunOptions.StopAllBusinessFlows & bf.RunStatus == eRunStatus.Failed)
//                {
//                    SetNextBusinessFlowsBlockedStatus();
//                    break;
//                }
//            }

//            SetPendingBusinessFlowsSkippedStatus();
//            CloseAgents();
//            if (ProjEnvironment != null)
//                ProjEnvironment.CloseEnvironment();
//            Status = eStatus.Completed;
//            EndRun = DateTime.Now;
//            mIsRunning = false;
//            ExecutionLogger.GingerEnd();
//        }

//        private void SetBusinessFlowInputVarsWithOutputValues(BusinessFlow bfToUpdate)
//        {
//            //set the vars to update
//            List<VariableBase> inputVarsToUpdate = bfToUpdate.GetBFandActivitiesVariabeles(false, true, false).Where(x => (string.IsNullOrEmpty(x.MappedOutputValue)) == false).ToList();

//            //set the vars to get value from
//            List<VariableBase> outputVariables;
//            if (BusinessFlow.SolutionVariables != null)
//                outputVariables = BusinessFlow.SolutionVariables.ToList();
//            else
//                outputVariables = new List<VariableBase>();
//            ObservableList<BusinessFlow> prevBFs = new ObservableList<BusinessFlow>();
//            for (int i = 0; i < BusinessFlows.IndexOf(bfToUpdate); i++)
//                prevBFs.Add(BusinessFlows[i]);
//            foreach (BusinessFlow bf in prevBFs.Reverse())//doing in reverse for passing the most updated value of variables with similar name
//                foreach (VariableBase var in bf.GetBFandActivitiesVariabeles(false, false, true))
//                    outputVariables.Add(var);

//            //do actual value update
//            foreach (VariableBase inputVar in inputVarsToUpdate)
//            {
//                string mappedValue = "";
//                if (inputVar.MappedOutputType == VariableBase.eOutputType.Variable)
//                {
//                    VariableBase outputVar = outputVariables.Where(x => x.Name == inputVar.MappedOutputValue).FirstOrDefault();
//                    if (outputVar != null)
//                        mappedValue = outputVar.Value;
//                }
//                else if (inputVar.MappedOutputType == VariableBase.eOutputType.DataSource)
//                    mappedValue = ValueExpression.Calculate(ProjEnvironment, mCurrentBusinessFlow, inputVar.MappedOutputValue, DSList);

//                if (mappedValue != "")
//                {
//                    if (inputVar.GetType() == typeof(VariableString))
//                    {
//                        ((VariableString)inputVar).Value = mappedValue;
//                        continue;
//                    }
//                    if (inputVar.GetType() == typeof(VariableSelectionList))
//                    {
//                        if (((VariableSelectionList)inputVar).OptionalValuesList.Where(pv => pv.Value == mappedValue).FirstOrDefault() != null)
//                            ((VariableSelectionList)inputVar).Value = mappedValue;
//                        continue;
//                    }
//                    if (inputVar.GetType() == typeof(VariableList))
//                    {
//                        string[] possibleVals = ((VariableList)inputVar).Formula.Split(',');
//                        if (possibleVals != null && possibleVals.Contains(mappedValue))
//                            ((VariableList)inputVar).Value = mappedValue;
//                        continue;
//                    }
//                    if (inputVar.GetType() == typeof(VariableDynamic))
//                    {
//                        ((VariableDynamic)inputVar).ValueExpression = mappedValue;
//                    }
//                }
//            }
//        }

//        private int GetRunningAgentsCount()
//        {
//            int RunningAgentCount = (from p in ApplicationAgents where p.Agent != null && p.Agent.Status == NewAgent.eStatus.Running select p).Count();
//            return RunningAgentCount;
//        }

//        public void StopAgents()
//        {
//            foreach (ApplicationAgent AA in ApplicationAgents)
//            {
//                if (AA.Agent != null)
//                {
//                    AA.Agent.CloseDriver();
//                }
//            }
//            AgentsRunning = false;
//        }

//        public string GetAgentsNameToRun()
//        {
//            string agentsNames = string.Empty;
//            foreach (ApplicationAgent AP in ApplicationAgents)
//            {
//                if (AP.Agent != null)
//                {
//                    agentsNames = agentsNames + " '" + AP.Agent.Name + "' for '" + AP.AppName + "',";
//                }
//                else
//                {
//                    agentsNames = agentsNames + " No Agent for - '" + AP.AppName + "',";
//                }
//            }
//            agentsNames = agentsNames.TrimEnd(',');
//            return agentsNames;
//        }

//        private int DoWork(Act act)
//        {
//            RunAction(act);
//            return 1;
//        }

//        public async Task<int> RunActionAsync(Act act)
//        {
//            var result = await Task.Run(() =>
//            {
//                RunAction(act);
//                return 1;   // TODO: return enum - Executed or interrpted
//            });
//            return result;
//        }

//        public async Task<int> RunActivityAsync(Activity activity)
//        {
//            var result = await Task.Run(() =>
//            {
//                RunActivity(activity);
//                return 1;   // TODO: return enum - Executed or interrpted
//            });
//            return result;
//        }

//        public async Task<int> RunFlowAsync()
//        {
//            var result = await Task.Run(() =>
//            {
//                RunFlow();
//                return 1;   // TODO: return enum - Executed or interrpted
//            });

//            return result;
//        }

//        public void RunAction(Act act, bool checkIfActionAllowedToRun = true)
//        {
//            if (!(act is StandAloneAction))
//            {
//                SetCurrentActivityAgent();
//            }

//            OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActionStart, act);

//            //TODO: remove from here !!??
//            act.SolutionFolder = SolutionFolder;
//            //resetting the retry mechanism count before calling the function.
//            act.RetryMechanismCount = 0;
//            RunActionWithRetryMechanism(act, checkIfActionAllowedToRun);
//            if (act.EnableRetryMechanism & mStopRun == false)
//            {
//                while (act.Status != eRunStatus.Passed && act.RetryMechanismCount < act.MaxNumberOfRetries & mStopRun == false)
//                {
//                    //Wait
//                    act.RetryMechanismCount++;
//                    act.Status = eRunStatus.Wait;
//                    ProcessIntervaleRetry(act);
//                    if (mStopRun)
//                        break;
//                    //Run Again
//                    RunActionWithRetryMechanism(act, checkIfActionAllowedToRun);
//                }
//            }
//            if (mStopRun)
//            {
//                act.Status = eRunStatus.Stopped;
//            }

//            OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActionEnd, act);
//        }

//        private void ProcessIntervaleRetry(Act act)
//        {
//            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//            st.Start();
//            if (act.RetryMechanismInterval > 0)
//            {
//                for (int i = 0; i < act.RetryMechanismInterval * 10; i++)
//                {
//                    if (mStopRun)
//                    {
//                        act.ExInfo += "Stopped";
//                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//                        return;
//                    }
//                    else
//                    {
//                        //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//                    }
//                    act.Status = eRunStatus.Wait;

//                    // TODO: use the Timer
//                    Thread.Sleep(100);  // Multiply * 10 to get 1 sec                    
//                }
//            }
//        }

//        public void RunActionWithRetryMechanism(Act act, bool checkIfActionAllowedToRun = true)
//        {
//            //Not suppose to happen but just in case
//            if (act == null)
//            {
//                Reporter.ToUser(eUserMsgKeys.AskToSelectAction);
//                return;
//            }

//            if (checkIfActionAllowedToRun)//to avoid duplicate checks in case the RunAction function is called from RunActvity
//            {
//                if (!act.Active)
//                {
//                    ResetAction(act);
//                    act.Status = eRunStatus.Skipped;
//                    act.ExInfo = "Action is not active.";
//                    return;
//                }
//                if (act.CheckIfVaribalesDependenciesAllowsToRun(mCurrentBusinessFlow.CurrentActivity, true) == false)
//                    return;
//            }

//            if (act.BreakPoint) mStopRun = true;
//            if (mStopRun) return;
//            eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
//            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//            st.Start();

//            PrepAction(act, ref ActionExecutorType, st);

//            if (mStopRun)
//            {
//                return;
//            }

//            ExecutionLogger.ActionStart(mCurrentBusinessFlow.CurrentActivity, act);

//            while (act.Status != eRunStatus.Passed)
//            {
//                RunActionWithTimeOutControl(act, ActionExecutorType);
//                CalculateActionFinalStatus(act);
//                if (mCurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true).Count() > 0)
//                    ErrAndPopupHandler(true);

//                if (!mErrorHandled && act.Status != eRunStatus.Passed && mCurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true).Count() > 0)
//                {
//                    ResetAction(act);
//                    act.Status = eRunStatus.Running;
//                    ErrAndPopupHandler();
//                }
//                else
//                    break;
//            }

//            // Run any code needed after the action executed, used in ACTScreenShot save to file after driver took screen shot
//            act.PostExecute();

//            //Adding for new control
//            ProcessStoretoValue(act);

//            CalculateActionFinalStatus(act); //why we need to run it again?

//            UpdateDSReturnValues(act);

//            // Add time stamp 
//            act.ExInfo = DateTime.Now.ToString() + " - " + act.ExInfo;

//            //Take screen shot marked or if action failed, and it is run by agent, no need to take for excel, Ejb or alike when fail except in case of failure
//            ProcessScreenShot(act, ActionExecutorType);

//            mErrorHandled = false;
//            try
//            {
//                AutoLogProxy.LogAction(mCurrentBusinessFlow, act);
//            }
//            catch { }

//            // Stop the counter before DoFlowControl
//            st.Stop();

//            // final timing of the action
//            act.Elapsed = st.ElapsedMilliseconds;
//            act.ElapsedTicks = st.ElapsedTicks;

//            //check if we have retry mechanism if yes go till max
//            if (act.Status == eRunStatus.Failed && act.EnableRetryMechanism && act.RetryMechanismCount < act.MaxNumberOfRetries)
//            {
//                //since we retrun and don't do flow control the action is going to run again
//                ExecutionLogger.ActionEnd(mCurrentBusinessFlow.CurrentActivity, act);
//                return;
//            }
//            // we capture current activity and action to use it for execution logger,
//            // because in DoFlowControl(act) it will point to the other action/activity(as flow control will be applied)
//            Activity activity = mCurrentBusinessFlow.CurrentActivity;
//            Act action = act;

//            DoFlowControl(act);
//            DoStatusConversion(act);   // does it need to be here or earlier?

//            ExecutionLogger.ActionEnd(activity, action);
//        }
//        private void UpdateDSReturnValues(Act act)
//        {
//            if (act.ConfigOutputDS == false)
//                return;

//            if (act.ReturnValues.Count == 0)
//                return;
//            List<ActReturnValue> mReturnValues = (from arc in act.ReturnValues where arc.Active == true select arc).ToList();
//            if (mReturnValues.Count == 0)
//                return;

//            if (act.DSOutputConfigParams.Count > 0 && (act.OutDataSourceName == null || act.OutDataSourceTableName == null))
//            {
//                act.OutDataSourceName = act.DSOutputConfigParams[0].DSName;
//                act.OutDataSourceTableName = act.DSOutputConfigParams[0].DSTable;
//            }

//            List<ActOutDataSourceConfig> mADCS = (from arc in act.DSOutputConfigParams where arc.DSName == act.OutDataSourceName && arc.DSTable == act.OutDataSourceTableName && arc.Active == true select arc).ToList();
//            if (mADCS.Count == 0)
//                return;
//            DataSourceBase DataSource = null;
//            DataSourceTable DataSourceTable = null;
//            foreach (DataSourceBase ds in DSList)
//            {
//                if (ds.Name == act.OutDataSourceName)
//                {
//                    DataSource = ds;
//                    break;
//                }
//            }
//            if (DataSource == null)
//                return;

//            DataSource.Init(DataSource.FileFullPath);
//            ObservableList<DataSourceTable> dstTables = DataSource.DSC.GetTablesList();
//            foreach (DataSourceTable dst in dstTables)
//            {
//                if (dst.Name == act.OutDataSourceTableName)
//                {
//                    DataSourceTable = dst;
//                    break;
//                }
//            }
//            if (DataSourceTable == null)
//                return;
//            List<string> mColList = DataSourceTable.DSC.GetColumnList(DataSourceTable.Name);
//            foreach (ActOutDataSourceConfig ADSC in mADCS)
//            {
//                if (mColList.Contains(ADSC.TableColumn) == false)
//                    DataSource.DSC.AddColumn(DataSourceTable.Name, ADSC.TableColumn, "Text");
//            }
//            foreach (ActReturnValue item in act.ReturnValues)
//            {
//                if (item.Active == true)
//                {
//                    string sQuery = "";
//                    if (DataSourceTable.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
//                    {
//                        string sKeyName = "";
//                        string sKeyValue = "";
//                        foreach (ActOutDataSourceConfig ADSC in mADCS)
//                        {
//                            if (ADSC.OutputType == "Parameter_Path")
//                            {
//                                if (item.Path != null && item.Path != "")
//                                    sKeyName = item.Param + "_" + item.Path;
//                                else
//                                    sKeyName = item.Param;
//                            }
//                            else if (ADSC.OutputType == "Parameter")
//                                sKeyName = item.Param;
//                            else
//                                sKeyValue = item.Actual;
//                        }
//                        DataTable dtOut = DataSource.DSC.GetQueryOutput("Select Count(*) from " + DataSourceTable.Name + " Where GINGER_KEY_NAME='" + sKeyName + "'");
//                        if (dtOut.Rows[0].ItemArray[0].ToString() == "1")
//                        {
//                            sQuery = "UPDATE " + DataSourceTable.Name + " SET GINGER_KEY_VALUE='" + sKeyValue + "',GINGER_LAST_UPDATED_BY='" + System.Environment.UserName + "',GINGER_LAST_UPDATE_DATETIME='" + DateTime.Now.ToString() + "' Where GINGER_KEY_NAME='" + sKeyName + "'";
//                        }
//                        else
//                        {
//                            sQuery = "INSERT INTO " + DataSourceTable.Name + "(GINGER_KEY_NAME,GINGER_KEY_VALUE,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME) VALUES ('" + sKeyName + "','" + sKeyValue + "','" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "')";
//                        }
//                    }
//                    else
//                    {
//                        string sColList = "";
//                        string sColVals = "";
//                        foreach (ActOutDataSourceConfig ADSC in mADCS)
//                        {
//                            sColList = sColList + ADSC.TableColumn + ",";
//                            if (ADSC.OutputType == "Parameter")
//                                sColVals = sColVals + "'" + item.Param + "',";
//                            else if (ADSC.OutputType == "Path")
//                                sColVals = sColVals + "'" + item.Path + "',";
//                            else if (ADSC.OutputType == "Actual")
//                                sColVals = sColVals + "'" + item.Actual + "',";
//                        }
//                        sQuery = "INSERT INTO " + DataSourceTable.Name + "(" + sColList + "GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME,GINGER_USED) VALUES (" + sColVals + "'" + System.Environment.UserName + "','" + DateTime.Now.ToString() + "',false)";
//                    }
//                    DataSource.DSC.RunQuery(sQuery);
//                }
//            }
//        }
//        public void ProcessReturnValueForDriver(Act act)
//        {
//            //Handle all output values, create Value for Driver for each
//            ValueExpression VE = new ValueExpression(ProjEnvironment, mCurrentBusinessFlow, DSList);

//            foreach (ActReturnValue ARV in act.ActReturnValues)
//            {
//                VE.Value = ARV.Param;
//                ARV.ParamCalculated = VE.ValueCalculated;
//                VE.Value = ARV.Path;
//                ARV.PathCalculated = VE.ValueCalculated;
//            }
//        }

//        public void ProcessInputValueForDriver(Act act)
//        {
//            //Handle all input values, create Value for Driver for each
//            ValueExpression VE = new ValueExpression(ProjEnvironment, mCurrentBusinessFlow, DSList);

//            VE.DecryptFlag = true;
//            foreach (var IV in act.InputValues)
//            {
//                if (!string.IsNullOrEmpty(IV.Value))
//                {
//                    VE.Value = IV.Value;
//                    IV.ValueForDriver = VE.ValueCalculated;
//                }
//                else
//                {
//                    IV.ValueForDriver = string.Empty;
//                }
//            }

//            //Handle actions which needs VE processing like Tuxedo, we need to calcualte the UD file values before execute, which is in differnt list not in ACT.Input list
//            List<ObservableList<ActInputValue>> list = act.GetInputValueListForVEProcessing();
//            if (list != null) // Will happen only if derived action implemented this function, since it needs processing for VEs
//            {
//                foreach (var subList in list)
//                {
//                    foreach (var IV in subList)
//                    {
//                        if (!string.IsNullOrEmpty(IV.Value))
//                        {
//                            VE.Value = IV.Value;
//                            IV.ValueForDriver = VE.ValueCalculated;
//                        }
//                        else
//                        {
//                            IV.ValueForDriver = string.Empty;
//                        }
//                    }
//                }
//            }
//        }

//        private void ProcessWait(Act act, System.Diagnostics.Stopwatch st)
//        {
//            if (act.Wait > 0)
//            {
//                act.Status = eRunStatus.Wait;

//                // TODO: use the timer instead
//                for (int i = 0; i < act.Wait * 10; i++)
//                {
//                    if (mStopRun)
//                    {
//                        act.ExInfo += "Stopped";
//                        return;
//                    }

//                    Thread.Sleep(100);  // Multiply * 10 to get 1 sec                    
//                }
//            }
//        }

//        private void ErrAndPopupHandler(bool handlingPopup = false)
//        {
//            //TODO: FIXME Why to repeat so many lines of code!!??
//            if (!handlingPopup)
//                mErrorHandled = true;
//            eActionExecutorType ActionExecutorType = eActionExecutorType.RunWithoutDriver;
//            foreach (ErrorHandler errhandler in mCurrentBusinessFlow.Activities.Where(a => a.GetType() == typeof(ErrorHandler) && a.Active == true))
//            {
//                if (handlingPopup && errhandler.HandlerType == ErrorHandler.eHandlerType.Error_Handler)
//                    continue;
//                else if (!handlingPopup && errhandler.HandlerType == ErrorHandler.eHandlerType.Popup_Handler)
//                    continue;
//                System.Diagnostics.Stopwatch stE = new System.Diagnostics.Stopwatch();
//                stE.Start();
//                foreach (Act act in errhandler.Acts)
//                {
//                    System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//                    st.Start();
//                    if (act.Active)
//                    {
//                        if (handlingPopup)
//                            act.Timeout = 1;
//                        PrepAction(act, ref ActionExecutorType, st);
//                        RunActionWithTimeOutControl(act, ActionExecutorType);
//                        CalculateActionFinalStatus(act);
//                    }
//                    st.Stop();
//                }
//                SetActionSkipStatus();
//                CalculateActivityFinalStatus(errhandler);
//                stE.Stop();
//                errhandler.Elapsed = stE.ElapsedMilliseconds;
//            }
//        }

//        private void PrepAction(Act act, ref eActionExecutorType ActExecutorType, System.Diagnostics.Stopwatch st)
//        {
//            PrepDynamicVariables();

//            ResetAction(act);

//            ProcessWait(act, st);

//            if (mStopRun)
//            {
//                act.Status = eRunStatus.Stopped;
//                //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//                return;
//            }
//            else
//            {
//                //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//            }

//            act.Status = eRunStatus.Started;

//            //No need for agent for some actions like DB and read for excel 
//            if ((RunInSimulationMode) && (act.SupportSimulation))
//            {
//                ActExecutorType = eActionExecutorType.RunInSumulationMode;
//            }
//            else
//            {
//                if (typeof(StandAloneAction).IsAssignableFrom(act.GetType()))
//                    ActExecutorType = eActionExecutorType.RunWithoutDriver;
//                else
//                    ActExecutorType = eActionExecutorType.RunOnDriver;
//            }

//            PrepActionVE(act);

//            act.Status = eRunStatus.Running;
//        }

//        public void PrepActionVE(Act act)
//        {
//            ProcessInputValueForDriver(act);
//            ProcessReturnValueForDriver(act);
//        }

//        internal void PrepDynamicVariables()
//        {
//            IEnumerable<VariableBase> vars = from v in mCurrentBusinessFlow.GetAllHierarchyVariables() where v.GetType() == typeof(VariableDynamic) select v;

//            foreach (VariableBase v in vars)
//            {
//                VariableDynamic vd = (VariableDynamic)v;
//                vd.Init(ProjEnvironment, mCurrentBusinessFlow);
//            }
//        }

//        private void ProcessScreenShot(Act act, eActionExecutorType ActionExecutorType)
//        {
//        }

//        private void RunActionWithTimeOutControl(Act act, eActionExecutorType ActExecutorType)
//        {
//            try
//            {
//                TimeSpan TS;
//                if (act.Timeout == null)
//                {
//                    // Default Action timeout is 30 mins
//                    TS = TimeSpan.FromMinutes(30);
//                }
//                else if (act.Timeout == 0)
//                {
//                    // Like no limit
//                    TS = TimeSpan.FromDays(1);
//                }
//                else
//                {
//                    TS = TimeSpan.FromSeconds((int)act.Timeout);
//                }

//                bool bCompleted = ExecuteWithTimeLimit(act, TS, () =>
//                {
//                    switch (ActExecutorType)
//                    {
//                        case eActionExecutorType.RunOnDriver:
//                            {
//                                if (mCurrentBusinessFlow.CurrentActivity.CurrentAgent != null)
//                                {
//                                    mCurrentBusinessFlow.CurrentActivity.CurrentAgent.RunAction(act);
//                                }
//                                else
//                                {
//                                    //TODO: Reporter
//                                    if (string.IsNullOrEmpty(act.Error))
//                                        act.Error = "No Agent was found for the" + GingerDicser.GetTermResValue(eTermResKey.Activity) + " Application.";
//                                    act.Status = eRunStatus.Failed;
//                                }
//                            }
//                            break;

//                        case eActionExecutorType.RunWithoutDriver:
//                            RunStandAloneAction(act);
//                            break;

//                        case eActionExecutorType.RunInSumulationMode:
//                            RunActionInSimulationMode(act);
//                            break;
//                    }
//                }
//                );

//                if (bCompleted)
//                {

//                }
//                else if (mStopRun)
//                {

//                }
//                else
//                {
//                    act.Status = eRunStatus.Cancelling;
//                    act.Error += "Timeout Occurred, Elapsed > " + act.Timeout;
//                }
//            }
//            catch (Exception e)
//            {
//                act.Status = eRunStatus.Failed;
//                act.Error = e.Message + Environment.NewLine + e.InnerException;
//            }
//            finally
//            {
//            }
//        }

//        Stopwatch ActionStopwatch;
//        public bool ExecuteWithTimeLimit(Act act, TimeSpan timeSpan, Action codeBlock)
//        {
//            ActionStopwatch = Stopwatch.StartNew();
//            System.Timers.Timer t = new System.Timers.Timer(100);
//            t.Elapsed += T_Elapsed;
//            t.Start();

//            try
//            {
//                // Run the code on seperate thead which we might kill/Abort if timeout
//                codeBlock.Invoke();

//                if (mStopRun)
//                {
//                    act.Status = eRunStatus.Stopped;
//                    act.ExInfo += "Stopped";
//                    //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//                }
//                else
//                {
//                    //To Handle Scenario which the Driver is still searching the element untill Implicity wait will be done, lates being used on SeleniumDriver.Isrunning method 
//                }
//                t.Stop();
//                act.Elapsed = ActionStopwatch.ElapsedMilliseconds;
//                return true;
//            }
//            catch (AggregateException ae)
//            {
//                throw ae.InnerExceptions[0];
//            }
//        }

//        private void T_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//        {
//            ((Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Elapsed = ActionStopwatch.ElapsedMilliseconds;
//        }

//        private void timertick(object state)
//        {
//            Stopwatch st = (Stopwatch)state;
//        }

//        public void RunActionInSimulationMode(Act act, bool checkIfActionAllowedToRun = true)
//        {
//            if (act.ReturnValues.Count == 0)
//                return;

//            ValueExpression VE = new ValueExpression(ProjEnvironment, mCurrentBusinessFlow, DSList);
//            foreach (ActReturnValue item in act.ActReturnValues)
//            {
//                if (item.SimulatedActual != null)
//                {
//                    VE.Value = item.SimulatedActual;
//                    item.Actual = VE.ValueCalculated;
//                }
//            }

//            act.ExInfo += "Executed in Simulation Mode";
//        }

//        public void SetCurrentActivityAgent()
//        {
//            // We take it based on the Activity target App
//            string AppName = mCurrentBusinessFlow.CurrentActivity.TargetApplication;
//            //For unit test cases, solution applicaitons will be always null

//            if (string.IsNullOrEmpty(AppName))
//            {
//                // If we don't have Target App on activity then take first App from BF
//                if (mCurrentBusinessFlow.TargetApplications.Count() > 0)
//                    AppName = mCurrentBusinessFlow.TargetApplications[0].AppName;
//            }

//            if (string.IsNullOrEmpty(AppName))
//            {
//                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Please select Target Application for the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and " + GingerDicser.GetTermResValue(eTermResKey.Activity));
//                mCurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
//                return;
//            }

//            ApplicationAgent AA = (from x in ApplicationAgents where x.AppName == AppName select x).FirstOrDefault();
//            if (AA == null || AA.Agent == null)
//            {

//                Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "The current target application, " + AppName + ", doesn't have a mapped agent assigned to it");
//                mCurrentBusinessFlow.CurrentActivity.CurrentAgent = null;
//                return;
//            }

//            // Verify the Agent for the action is running 
//            if (AA.Agent.Status != NewAgent.eStatus.Running && AA.Agent.Status != NewAgent.eStatus.Starting)
//            {
//                AA.Agent.StartDriver();
//            }

//            mCurrentBusinessFlow.CurrentActivity.CurrentAgent = AA.Agent;
//        }

//        private void ProcessStoretoValue(Act act)
//        {
//            foreach (ActReturnValue item in act.ReturnValues)
//            {
//                if (item.StoreTo == ActReturnValue.eStoreTo.Variable && !String.IsNullOrEmpty(item.StoreToValue))
//                {
//                    VariableBase varb = mCurrentBusinessFlow.GetHierarchyVariableByNameAndType(item.StoreToValue, "String");
//                    if (varb != null)
//                    {
//                        ((VariableString)varb).Value = item.Actual;
//                    }
//                    else
//                        Reporter.ToUser(eUserMsgKeys.CantStoreToVariable, item.StoreToValue);
//                }
//                //Adding for New Control
//                if (item.StoreTo == ActReturnValue.eStoreTo.DataSource && !String.IsNullOrEmpty(item.StoreToValue))
//                {
//                    ValueExpression.Calculate(ProjEnvironment, mCurrentBusinessFlow, item.StoreToValue, DSList, true, item.Actual);
//                }
//            }
//        }

//        private void RunStandAloneAction(Act act)
//        {
//            try
//            {
//                FindService(act.ID);
//                GNA.RunAction(act);
//            }
//            catch (Exception ex)
//            {
//                act.Status = eRunStatus.Failed;
//                act.Error += ex.Message;
//            }
//        }

//        private void FindService(string serviceName)
//        {
//            // all services running on local Grid for now

//            GingerNodeInfo GNI = (from x in WorkSpace.Instance.LocalGingerGrid.NodeList where x.Name == "ExcelService_1" select x).FirstOrDefault();

//            if (GNI == null)
//            {
//                // since it is local then start a new Ginger Node
//                StartService();
//            }
//            else
//            {
//                //TODO: keep list og GNA - no need to connect each time!?
//                // We found a valid agent use it
//            }
//        }

//        GingerNodeProxy GNA = null;

//        private void StartService()
//        {
//            GingerGrid GG = WorkSpace.Instance.LocalGingerGrid;
//            string Pluginfolder = @"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerCoreNETUnitTest\TestResources\PluginPackages\StandAlonePluginPackage.1.0.0";

//            string script = CommandProcessor.CreateLoadPluginScript(Pluginfolder);
//            script += CommandProcessor.CreateStartServiceScript("ExcelService", "ExcelService_1", SocketHelper.GetLocalHostIP(), GG.Port) + Environment.NewLine;
//            GingerConsoleHelper.Execute(script);

//            Stopwatch st = Stopwatch.StartNew();
//            GingerNodeInfo GNI = null;
//            while (GNI == null && st.ElapsedMilliseconds < 10000)
//            {
//                Thread.Sleep(1000);
//                GNI = (from x in GG.NodeList where x.Name == "ExcelService_1" select x).SingleOrDefault();
//            }

//            GNA = GG.CreateGingerNodeAgent(GNI);
//            GNA.Reserve();
//        }

//        private void ResetAction(Act act)
//        {
//            act.Reset();
//        }

//        private void DoFlowControl(Act act)
//        {
//            //TODO: on pass, on fail etc...
//            bool IsStopLoop = false;
//            ValueExpression VE = new ValueExpression(this.ProjEnvironment, this.mCurrentBusinessFlow, this.DSList);

//            foreach (FlowControl FC in act.FlowControls)
//            {
//                if (FC.Active == false)
//                {
//                    FC.Status = FlowControl.eStatus.Skipped;
//                    continue;
//                }

//                FC.CalculateCondition(mCurrentBusinessFlow, ProjEnvironment, act, this.DSList);

//                //TODO: Move below condition inside calucate condition once move execution logger to Ginger core

//                if (FC.ConditionCalculated.Contains("{LastActivityStatus}"))
//                    FC.ConditionCalculated = FC.ConditionCalculated.Replace("{LastActivityStatus}", ExecutionLogger.GetLastExecutedActivityRunStatus(mCurrentBusinessFlow.CurrentActivity));

//                FC.CalcualtedValue(mCurrentBusinessFlow, ProjEnvironment, this.DSList);

//                string rc = VBS.ExecuteVBSEval(FC.ConditionCalculated.Trim());

//                bool IsConditionTrue;
//                if (rc == "-1")
//                {
//                    FC.ConditionCalculated += " is True";
//                    IsConditionTrue = true;
//                }
//                else
//                {
//                    FC.ConditionCalculated += " is False";
//                    IsConditionTrue = false;
//                }

//                if (IsConditionTrue)
//                {
//                    //Perform the action as condition is true
//                    switch (FC.FlowControlAction)
//                    {
//                        case FlowControl.eFlowControlAction.MessageBox:
//                            VE.Value = FC.Value;
//                            MessageBox.Show(VE.ValueCalculated);
//                            break;
//                        case FlowControl.eFlowControlAction.GoToAction:
//                            GotoAction(FC);
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.GoToNextAction:
//                            GotoNextAction();
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.GoToActivity:
//                            GotoActivity(FC);
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.GoToNextActivity:
//                            GotoNextActivity();
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.RerunAction:
//                            act.Status = eRunStatus.Pending;
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.RerunActivity:
//                            ResetActivity(mCurrentBusinessFlow.CurrentActivity);
//                            mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = mCurrentBusinessFlow.CurrentActivity.Acts[0];
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.StopBusinessFlow:
//                            mStopBusinessFlow = true;
//                            mCurrentBusinessFlow.CurrentActivity = mCurrentBusinessFlow.Activities.LastOrDefault();
//                            mCurrentBusinessFlow.Activities.CurrentItem = mCurrentBusinessFlow.CurrentActivity;
//                            mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = mCurrentBusinessFlow.CurrentActivity.Acts.LastOrDefault();
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.StopRun:
//                            mStopRun = true;
//                            IsStopLoop = true;
//                            break;
//                        case FlowControl.eFlowControlAction.SetVariableValue:
//                            try
//                            {
//                                VE.Value = FC.Value;
//                                string[] vals = VE.ValueCalculated.Split(new char[] { '=' });
//                                if (vals.Count() == 2)
//                                {
//                                    ActSetVariableValue setValueAct = new ActSetVariableValue();
//                                    setValueAct.VariableName = vals[0];
//                                    setValueAct.SetVariableValueOption = ActSetVariableValue.eSetValueOptions.SetValue;
//                                    setValueAct.Value = vals[1];
//                                    setValueAct.RunOnBusinessFlow = this.mCurrentBusinessFlow;
//                                    setValueAct.DSList = this.DSList;
//                                    setValueAct.Execute();
//                                }
//                                else
//                                {
//                                    FC.Status = FlowControl.eStatus.Execution_Failed;
//                                }
//                            }
//                            catch (Exception ex)
//                            {
//                                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Set Variable Value Flow Control", ex);
//                                FC.Status = FlowControl.eStatus.Execution_Failed;
//                            }
//                            break;
//                        case FlowControl.eFlowControlAction.RunSharedRepositoryActivity:
//                            try
//                            {
//                                throw new Exception("FIXME RunSharedRepositoryActivity not impl");
//                            }
//                            catch (Exception ex)
//                            {
//                                Reporter.ToLog(eLogLevel.ERROR, "Failed to do RunSharedRepositoryActivity Flow Control", ex);
//                                FC.Status = FlowControl.eStatus.Execution_Failed;
//                            }
//                            break;

//                        default:
//                            //TODO:
//                            break;
//                    }

//                    if (FC.Status == FlowControl.eStatus.Pending)
//                    {
//                        FC.Status = FlowControl.eStatus.Executed_Action;
//                    }
//                }
//                else
//                {
//                    FC.Status = FlowControl.eStatus.Executed_NoAction;
//                }

//                // Go out the foreach in case we have a goto so no need to process the rest of FCs
//                if (IsStopLoop) break;
//            }

//            // If all above completed and no change on flow then move to next in the activity unless it is the last one
//            if (!IsStopLoop)
//            {
//                if (!IsLastActionOfActivity())
//                {
//                    if (act.IsSingleAction == null || act.IsSingleAction == false)
//                    {
//                        GotoNextAction();
//                        ((Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = eRunStatus.Pending;
//                    }
//                }
//            }
//        }

//        private void GotoActivity(FlowControl fc)
//        {
//            Activity a = mCurrentBusinessFlow.GetActivity(fc.GetGuidFromValue(), fc.GetNameFromValue());
//            if (a != null)
//            {
//                mCurrentBusinessFlow.CurrentActivity = a;
//                mCurrentBusinessFlow.Activities.CurrentItem = mCurrentBusinessFlow.CurrentActivity;
//                a.Acts.CurrentItem = a.Acts.FirstOrDefault();
//            }
//            else
//            {
//                MessageBox.Show(GingerDicser.GetTermResValue(eTermResKey.Activity) + " Name was not found - " + Name);
//            }
//        }

//        private void GotoAction(FlowControl fc)
//        {
//            Act a = mCurrentBusinessFlow.CurrentActivity.GetAct(fc.GetGuidFromValue(), fc.GetNameFromValue());

//            if (a != null)
//            {
//                mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = a;
//                a.Status = eRunStatus.Pending;
//            }
//            else
//            {
//                MessageBox.Show("Action Name not found - " + Name);
//            }
//        }

//        private bool GotoNextAction()
//        {
//            return mCurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
//        }

//        private void GotoNextActivity()
//        {
//            mCurrentBusinessFlow.Activities.MoveNext();
//            mCurrentBusinessFlow.CurrentActivity = (Activity)mCurrentBusinessFlow.Activities.CurrentItem;
//        }
//        public void CalculateActivityFinalStatus(Activity a)
//        {
//            a.Status = eRunStatus.Skipped;

//            //if there is one fail then Activity status is fail
//            if (a.Acts.Where(x => x.Status == eRunStatus.Failed).FirstOrDefault() != null)
//                a.Status = eRunStatus.Failed;
//            else if (a.Acts.Where(x => x.Status == eRunStatus.Stopped).FirstOrDefault() != null)   //
//                a.Status = eRunStatus.Stopped;                                     // to verify !!! TEMP
//            else
//            {
//                // If we have at least 1 pass then it passed, otherwise will remain Skipped
//                if (a.Acts.Where(x => x.Status == eRunStatus.Passed).FirstOrDefault() != null)
//                {
//                    a.Status = eRunStatus.Passed;
//                }
//            }
//        }

//        public void CalculateActionFinalStatus(Act act)
//        {

//            // Action pass if no error/exception and Expected = Actual
//            if (!string.IsNullOrEmpty(act.Error) || act.Status == eRunStatus.Stopped)
//            {
//                //Adding Act.Error param to store error in Actual or variable
//                foreach (ActReturnValue item in act.ReturnValues)
//                {
//                    if (item.Param.ToUpper() == "ACT.ERROR".ToUpper())
//                    {
//                        item.Actual = act.Error;
//                    }

//                    //set status if not already set
//                    if (item.Status == ActReturnValue.eStatus.Pending)
//                    {
//                        if (String.IsNullOrEmpty(item.Expected))
//                        {
//                            item.Status = ActReturnValue.eStatus.NA;
//                        }
//                        else
//                        {
//                            item.Status = ActReturnValue.eStatus.Skipped;
//                        }
//                    }
//                }

//                if (act.Status != eRunStatus.Stopped)
//                    act.Status = eRunStatus.Failed;
//                return;
//            }

//            //Go over all return value calculate each for final action result status
//            foreach (ActReturnValue ARC in act.ReturnValues)
//            {
//                //if expected is empty then no check and mark it NA
//                if (String.IsNullOrEmpty(ARC.Expected))
//                {
//                    ARC.Status = ActReturnValue.eStatus.NA;
//                }
//                else if (!ARC.Active)
//                {
//                    ARC.Status = ActReturnValue.eStatus.Skipped;
//                }
//                else
//                {
//                    //get Expected Calculated
//                    ValueExpression ve = new ValueExpression(ProjEnvironment, mCurrentBusinessFlow, DSList);
//                    ve.Value = ARC.Expected;
//                    //replace {Actual} placce holder with real Actual value
//                    if (ve.Value.Contains("{Actual}"))
//                    {
//                        //Replace to 
//                        if ((ARC.Actual != null) && General.IsNumeric(ARC.Actual))
//                        {
//                            ve.Value = ve.Value.Replace("{Actual}", ARC.Actual);
//                        }
//                        else
//                        {
//                            ve.Value = ve.Value.Replace("{Actual}", "\"" + ARC.Actual + "\"");
//                        }
//                    }
//                    //calculate the expected value
//                    ARC.ExpectedCalculated = ve.ValueCalculated;

//                    //compare Actual vs Expected (calcualted)
//                    CalculateARCStatus(ARC);

//                    if (ARC.Status == ActReturnValue.eStatus.Failed)
//                    {
//                        string formatedExpectedCalculated = ARC.ExpectedCalculated;
//                        if (ARC.ExpectedCalculated.Length >= 9 && (ARC.ExpectedCalculated.Substring(ARC.ExpectedCalculated.Length - 9, 9)).Contains("is False"))
//                            formatedExpectedCalculated = ARC.ExpectedCalculated.ToString().Substring(0, ARC.ExpectedCalculated.Length - 9);

//                        act.Error += "Output Value validation failed for the Parameter '" + ARC.Param + "' , Expected value is " + formatedExpectedCalculated + " while Actual value is '" + ARC.Actual + "'" + System.Environment.NewLine;
//                    }
//                }
//            }

//            int CountFail = (from x in act.ReturnValues where x.Status == ActReturnValue.eStatus.Failed select x).Count();
//            if (CountFail > 0)
//            {
//                act.Status = eRunStatus.Failed;
//            }

//            if (act.Status != eRunStatus.Failed && act.Status != eRunStatus.Passed)
//            {
//                act.Status = eRunStatus.Passed;
//            }
//            else if (act.Status != eRunStatus.Passed)
//            {
//                act.Status = eRunStatus.Failed;
//            }
//        }

//        private void DoStatusConversion(Act act)
//        {
//            if (act.StatusConverter == Act.eStatusConverterOptions.None)
//            {
//                return;
//            }
//            if (act.StatusConverter == Act.eStatusConverterOptions.AlwaysPass)
//            {
//                act.Status = eRunStatus.Passed;
//            }
//            else if (act.StatusConverter == Act.eStatusConverterOptions.IgnoreFail && act.Status == eRunStatus.Failed)
//            {
//                act.Status = eRunStatus.FailIgnored;
//            }
//            else if (act.StatusConverter == Act.eStatusConverterOptions.InvertStatus)
//            {
//                if (act.Status == eRunStatus.Passed)
//                {
//                    act.Status = eRunStatus.Failed;
//                }
//                else if (act.Status == eRunStatus.Failed)
//                {
//                    act.Status = eRunStatus.Passed;
//                }
//            }
//        }

//        public static void CalculateARCStatus(ActReturnValue ARC)
//        {
//            //TODO: Check Expected null or empty return with no change    

//            //check basic compare - most cases
//            if (ARC.ExpectedCalculated == ARC.Actual)
//            {
//                ARC.Status = ActReturnValue.eStatus.Passed;
//                return;
//            }

//            //fail in case empty Actual or Expected
//            if (String.IsNullOrEmpty(ARC.Actual))
//            {
//                ARC.Status = ActReturnValue.eStatus.Failed;
//                return;
//            }
//            if (String.IsNullOrEmpty(ARC.ExpectedCalculated))
//            {
//                ARC.Status = ActReturnValue.eStatus.Failed;
//                return;
//            }

//            //TODO: docuemnt in help, maybe remove this compare takes time and not sure if needed/use case!?
//            if (ARC.ExpectedCalculated.StartsWith("{Regex="))
//            {
//                string ExpectedRegex = ARC.ExpectedCalculated.Replace("{Regex=", "");
//                ExpectedRegex = ExpectedRegex.Trim();
//                if (ExpectedRegex.EndsWith("}"))
//                {
//                    ExpectedRegex = ExpectedRegex.Substring(0, ExpectedRegex.Length - 1);
//                }

//                Regex rg = new Regex(ExpectedRegex);
//                if (rg.IsMatch(ARC.Actual))
//                {
//                    ARC.Status = ActReturnValue.eStatus.Passed;
//                    return;
//                }
//                else
//                {
//                    ARC.Status = ActReturnValue.eStatus.Failed;
//                    return;
//                }
//            }

//            if (ARC.ExpectedCalculated.ToUpper().Trim() == "TRUE" && ARC.Actual.ToUpper().Trim() != "TRUE")
//            {
//                ARC.Status = ActReturnValue.eStatus.Failed;
//                return;
//            }
//            else
//            {
//                //do VBS compare for conditions like "7 > 5"
//                bool b = EvalExpectedWithActual(ARC);
//                if (b)
//                {
//                    ARC.Status = ActReturnValue.eStatus.Passed;
//                    return;
//                }
//                else
//                {
//                    ARC.Status = ActReturnValue.eStatus.Failed;
//                    return;
//                }
//            }
//        }

//        public static void ReplaceActualPlaceHolder(ActReturnValue ARC)//currently used only for unit tests
//        {
//            string sEval;
//            if (General.IsNumeric(ARC.Actual))
//            {
//                sEval = ARC.ExpectedCalculated.Replace("{Actual}", ARC.Actual);
//            }
//            else
//            {
//                // Add " " to wrap the string
//                sEval = ARC.ExpectedCalculated.Replace("{Actual}", "\"" + ARC.Actual + "\"");
//            }
//            ARC.ExpectedCalculated = sEval;
//            ARC.ExpectedCalculatedValue = ARC.ExpectedCalculated;
//        }

//        public static bool EvalExpectedWithActual(ActReturnValue ARC)
//        {
//            string rc = "";
//            if (ARC.ExpectedCalculated.ToUpper().StartsWith("NOT "))
//            {
//                rc = VBS.ExecuteVBSEval("'" + ARC.ExpectedCalculated + "'");
//            }
//            else
//            {
//                rc = VBS.ExecuteVBSEval(ARC.ExpectedCalculated);
//            }

//            if (rc == "-1")
//            {
//                ARC.ExpectedCalculated = "'" + ARC.ExpectedCalculated + "' is True";
//                return true;
//            }
//            else
//            {
//                ARC.ExpectedCalculated = "'" + ARC.ExpectedCalculated + "' is False";
//                return false;
//            }
//        }

//        public void RunActivity(Activity Activity, bool Continue = false)
//        {

//            OnGingerRunnerEvent(GingerRunnerEventArgs.eEventType.ActivityStart, Activity);

//            bool statusCalculationIsDone = false;

//            //check if Activity is allowed to run
//            if (mCurrentBusinessFlow == null ||
//                    Activity.Acts.Count == 0 || //no Actions to run
//                        Activity.GetType() == typeof(ErrorHandler) ||//don't run error handler from RunActivity
//                            Activity.CheckIfVaribalesDependenciesAllowsToRun(mCurrentBusinessFlow, true) == false || //Variables-Dependencies not allowing to run
//                                (FilterExecutionByTags == true && CheckIfActivityTagsMatch() == false))//add validation for Ginger runner tags
//            {
//                CalculateActivityFinalStatus(Activity);
//                return;
//            }


//            //add validation for Ginger runner tags
//            if (FilterExecutionByTags)
//                if (CheckIfActivityTagsMatch() == false) return;

//            if (!Continue)
//            {
//                // We reset the activitiy unless we are in continue mode where user can start from middle of Activity
//                ResetActivity(mCurrentBusinessFlow.CurrentActivity);
//            }
//            else
//            {
//                // since we are in continue mode - only for first activity of continue mode
//                // Just change the status to Pending
//                mCurrentBusinessFlow.CurrentActivity.Status = eRunStatus.Pending;
//            }
//            //Do not disable the following two lines. thse helping the FC run proper activities
//            mCurrentBusinessFlow.Activities.CurrentItem = mCurrentBusinessFlow.CurrentActivity;
//            mCurrentBusinessFlow.PropertyChanged += mCurrentBusinessFlow_PropertyChanged;
//            mStopRun = false;
//            mStopBusinessFlow = false;
//            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//            bCurrentActivityChanged = false;

//            if (SolutionApplications != null && SolutionApplications.Where(x => (x.AppName == Activity.TargetApplication && x.Platform == Platform.ePlatformType.NA)).FirstOrDefault() == null)
//            {
//                //load Agent only if Activity includes Actions which needs it
//                List<Act> driverActs = Activity.Acts.Where(x => (x is ActWithoutDriver && x.ActionType != "AgentManipulationActionType") == false).ToList();
//                if (driverActs.Count > 0)
//                {
//                    //make sure not running in Simulation mode
//                    if (!RunInSimulationMode ||
//                        (RunInSimulationMode == true && driverActs.Where(x => x.SupportSimulation == false).ToList().Count() > 0))
//                    {
//                        //Set the Agent to run actions with  
//                        SetCurrentActivityAgent();
//                    }
//                }
//            }

//            Activity.ExecutionLogActionCounter = 0;
//            ExecutionLogger.ActivityStart(mCurrentBusinessFlow, Activity);
//            // run the Actions
//            st.Start();

//            try
//            {
//                Act act = null;

//                // if it is not continue mode then goto first Action
//                if (!Continue)
//                    act = Activity.Acts.FirstOrDefault();
//                else
//                    act = (Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;

//                bool bHasMoreActions = true;
//                while (bHasMoreActions)
//                {
//                    mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = act;

//                    if (act.Active && act.CheckIfVaribalesDependenciesAllowsToRun(Activity, true) == true)
//                    {
//                        RunAction(act, false);
//                        if (bCurrentActivityChanged)
//                        {
//                            mCurrentBusinessFlow.CurrentActivity.Elapsed = st.ElapsedMilliseconds;
//                            if (act.Status == eRunStatus.Failed)
//                            {
//                                Activity.Status = eRunStatus.Failed;

//                                if (Activity.ActionRunOption == Activity.eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
//                                {
//                                    SetNextActionsBlockedStatus();
//                                    statusCalculationIsDone = true;
//                                    return;
//                                }
//                            }
//                            break;
//                        }
//                        mCurrentBusinessFlow.CurrentActivity.Elapsed = st.ElapsedMilliseconds;
//                        if (act.Status == eRunStatus.Failed)
//                        {
//                            Activity.Status = eRunStatus.Failed;

//                            if (Activity.ActionRunOption == Activity.eActionRunOption.StopActionsRunOnFailure && act.FlowControls.Count == 0)
//                            {
//                                SetNextActionsBlockedStatus();
//                                statusCalculationIsDone = true;
//                                return;
//                            }
//                        }
//                        // If the user selected slower speed then do wait
//                        if (mSpeed > 0)
//                        {
//                            // TODO: sleep 100 and do events
//                            Thread.Sleep(mSpeed * 1000);
//                        }

//                        if (mStopRun || mStopBusinessFlow)
//                        {
//                            CalculateActivityFinalStatus(Activity);
//                            statusCalculationIsDone = true;
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        if (!act.Active)
//                        {
//                            ResetAction(act);
//                            act.Status = eRunStatus.Skipped;
//                            act.ExInfo = "Action is not active.";
//                        }
//                        if (!Activity.Acts.IsLastItem())
//                        {
//                            GotoNextAction();
//                            ((Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem).Status = eRunStatus.Pending;
//                        }
//                    }

//                    act = (Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
//                    // As long as we have more action we keep the loop, until no more actions available
//                    if (act.Status != eRunStatus.Pending && Activity.Acts.IsLastItem())
//                    {
//                        bHasMoreActions = false;
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                SetNextActionsBlockedStatus();
//                ExecutionLogger.ActivityEnd(mCurrentBusinessFlow, Activity);

//                //TODO: Throw execption don't cover in log, so user will see it in report
//                Reporter.ToLog(eLogLevel.ERROR, "Run Activity got error ", ex);
//                throw ex;
//            }
//            finally
//            {
//                st.Stop();

//                Activity.Elapsed = st.ElapsedMilliseconds;
//                if (!statusCalculationIsDone)
//                {
//                    CalculateActivityFinalStatus(Activity);
//                }
//                ExecutionLogger.ActivityEnd(mCurrentBusinessFlow, Activity);
//                AutoLogProxy.LogActivity(mCurrentBusinessFlow, Activity);
//            }
//        }

//        private void mCurrentBusinessFlow_PropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            if (e.PropertyName == "CurrentActivity")
//            {
//                bCurrentActivityChanged = true;
//            }
//        }

//        private bool IsLastActionOfActivity()
//        {
//            if (mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem != mCurrentBusinessFlow.CurrentActivity.Acts[mCurrentBusinessFlow.CurrentActivity.Acts.Count - 1])
//            {
//                return false;
//            }
//            else
//            {
//                return true;
//            }
//        }

//        public void RunFlow()
//        {
//            mCurrentBusinessFlow.Reset();

//            //check if Business Flow is allowed to run
//            if (mCurrentBusinessFlow.Activities.Count == 0)//no Activities to run
//            {
//                CalculateBusinessFlowFinalStatus(mCurrentBusinessFlow);
//                return;
//            }

//            // Start from First Activity first Action, then do Continue
//            Activity FirstActivity = mCurrentBusinessFlow.Activities.FirstOrDefault();
//            FirstActivity.Reset();
//            mCurrentBusinessFlow.Activities.CurrentItem = FirstActivity;
//            mCurrentBusinessFlow.CurrentActivity = FirstActivity;
//            FirstActivity.Acts.CurrentItem = FirstActivity.Acts.FirstOrDefault();

//            mCurrentBusinessFlow.ExecutionLogActivityCounter = 0;
//            ExecutionLogger.BusinessFlowStart(mCurrentBusinessFlow);

//            ContinueRun();
//        }

//        internal void ContinueRun()
//        {
//            // Used from runFlow and Continue, so both will run same logic
//            System.Diagnostics.Stopwatch st = new System.Diagnostics.Stopwatch();
//            st.Start();

//            UpdateLastExecutingAgent();

//            try
//            {
//                mStopRun = false;
//                mStopBusinessFlow = false;
//                mCurrentBusinessFlow.RunStatus = eRunStatus.Running;
//                mCurrentBusinessFlow.Elapsed = null;
//                mCurrentBusinessFlow.Environment = ProjEnvironment == null ? "" : ProjEnvironment.Name;
//                PrepDynamicVariables();
//                Activity ExecutingActivity = (Activity)mCurrentBusinessFlow.CurrentActivity; // Activities.CurrentItem;

//                Activity FirstExecutedActivity = ExecutingActivity;
//                while (ExecutingActivity != null)
//                {
//                    if (ExecutingActivity.GetType() == typeof(ErrorHandler))
//                    {
//                        if (!mCurrentBusinessFlow.Activities.IsLastItem())
//                        {
//                            GotoNextActivity();
//                            ExecutingActivity = (Activity)mCurrentBusinessFlow.Activities.CurrentItem;
//                        }
//                        else
//                        {
//                            ExecutingActivity = null;
//                        }
//                        continue;
//                    }
//                    else
//                    {
//                        ExecutingActivity.Status = eRunStatus.Running;
//                        if (ExecutingActivity.Active != false)
//                        {
//                            // We run the first Activity in Continue mode, if it came from RunFlow, then it is set to first action
//                            if (FirstExecutedActivity.Equals(ExecutingActivity))
//                            {
//                                RunActivity(ExecutingActivity, true);
//                            }
//                            else
//                            {
//                                RunActivity(ExecutingActivity);
//                            }
//                            //TODO: Why this is here? do we need to rehook
//                            mCurrentBusinessFlow.PropertyChanged -= mCurrentBusinessFlow_PropertyChanged;

//                            if (ExecutingActivity.Mandatory && ExecutingActivity.Status == eRunStatus.Failed)
//                            {
//                                mCurrentBusinessFlow.RunStatus = eRunStatus.Failed;
//                                if (!(mCurrentBusinessFlow.Activities.IsLastItem()))
//                                {
//                                    GotoNextActivity();
//                                    SetNextActivitiesBlockedStatus();
//                                }
//                                return;
//                            }
//                        }
//                        else
//                        {
//                            ExecutingActivity.Status = eRunStatus.Skipped;
//                        }

//                        if (mStopRun || mStopBusinessFlow)
//                        {
//                            SetActionSkipStatus();
//                            mCurrentBusinessFlow.RunStatus = eRunStatus.Stopped;
//                            return;
//                        }

//                        if ((Activity)mCurrentBusinessFlow.Activities.CurrentItem != ExecutingActivity)
//                        {
//                            //If not equal means flow control updatet current item to target activity, no need to do next activity
//                            ExecutingActivity = (Activity)mCurrentBusinessFlow.Activities.CurrentItem;
//                        }
//                        else
//                        {
//                            if (!mCurrentBusinessFlow.Activities.IsLastItem())
//                            {
//                                GotoNextActivity();
//                                ExecutingActivity = (Activity)mCurrentBusinessFlow.Activities.CurrentItem;
//                            }
//                            else
//                            {
//                                ExecutingActivity = null;
//                            }
//                        }

//                    }
//                }
//                SetActionSkipStatus();
//                CalculateBusinessFlowFinalStatus(mCurrentBusinessFlow);
//            }
//            catch (Exception ex)
//            {
//                Reporter.ToLog(eLogLevel.ERROR, "Run flow got exception " + ex.Message);
//            }
//            finally
//            {
//                SetActionSkipStatus();
//                CalculateBusinessFlowFinalStatus(mCurrentBusinessFlow);

//                st.Stop();
//                mCurrentBusinessFlow.Elapsed = st.ElapsedMilliseconds;

//                ExecutionLogger.BusinessFlowEnd(mCurrentBusinessFlow);

//                //TODO: remove all AutoLogProxy - need to be a listener to GR Events
//                AutoLogProxy.LogBusinessFlow(mCurrentBusinessFlow);
//            }
//        }

//        private void UpdateLastExecutingAgent()
//        {
//            foreach (TargetApplication TA in mCurrentBusinessFlow.TargetApplications)
//            {
//                string a = (from x in ApplicationAgents where x.AppName == TA.AppName select x.AgentName).FirstOrDefault();
//                TA.LastExecutingAgentName = a;
//            }
//        }

//        public void CalculateBusinessFlowFinalStatus(BusinessFlow BF)
//        {
//            // A flow is blocked if some activitiy failed and all the activities after it failed
//            // A Flow is failed if one or more activities failed

//            // Add Blocked
//            bool Failed = false;
//            bool Blocked = false;
//            bool Stopped = false;

//            // Assume pass unless error
//            BF.RunStatus = eRunStatus.Passed;

//            if (BF.Activities.Count == 0)
//            {
//                BF.RunStatus = eRunStatus.Skipped;
//                return;
//            }

//            foreach (Activity a in BF.Activities.Where(a => a.GetType() != typeof(ErrorHandler)))
//            {
//                if (a.Status == eRunStatus.Failed)
//                {
//                    // We assume from here on it i blocked
//                    Failed = true;  // We found one failed activity so the flow is failed or blocked, to be decided at the end
//                    break;
//                }
//                else if (a.Status == eRunStatus.Blocked)
//                {
//                    Blocked = true;
//                }
//                else if (a.Status == eRunStatus.Stopped)
//                {
//                    Stopped = true;
//                }
//            }

//            if (Blocked)
//            {
//                BF.RunStatus = eRunStatus.Blocked;
//            }
//            else if (Failed)
//            {
//                BF.RunStatus = eRunStatus.Failed;
//            }
//            else if (Stopped)
//            {
//                BF.RunStatus = eRunStatus.Stopped;
//            }
//            else
//            {
//                BF.RunStatus = eRunStatus.Passed;
//            }
//        }

//        private void SetActionSkipStatus()
//        {
//            foreach (Activity a in mCurrentBusinessFlow.Activities)
//            {
//                if (mStopRun)
//                    break;

//                foreach (Act act in a.Acts)
//                {
//                    if (mStopRun)
//                        break;

//                    if (act.Status == eRunStatus.Pending)
//                        act.Status = eRunStatus.Skipped;
//                }

//                if (a.Status == eRunStatus.Pending)
//                {
//                    a.Status = eRunStatus.Skipped;

//                    // return;
//                }
//            }
//        }

//        private void SetPendingBusinessFlowsSkippedStatus()
//        {
//            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == eRunStatus.Pending))
//            {
//                if (mStopRun)
//                    break;

//                bf.RunStatus = eRunStatus.Skipped;

//                mCurrentBusinessFlow = bf;
//                mCurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
//                mCurrentBusinessFlow.Activities.CurrentItem = mCurrentBusinessFlow.CurrentActivity;
//                SetActionSkipStatus();
//            }
//        }

//        private void SetNextBusinessFlowsBlockedStatus()
//        {
//            foreach (BusinessFlow bf in BusinessFlows.Where(a => a.RunStatus == eRunStatus.Pending))
//            {
//                if (mStopRun)
//                    break;

//                if (bf.Active)
//                {
//                    bf.RunStatus = eRunStatus.Blocked;
//                }
//                mCurrentBusinessFlow = bf;
//                mCurrentBusinessFlow.CurrentActivity = bf.Activities.FirstOrDefault();
//                mCurrentBusinessFlow.Activities.CurrentItem = mCurrentBusinessFlow.CurrentActivity;
//                SetNextActivitiesBlockedStatus();
//            }
//        }

//        private void SetNextActivitiesBlockedStatus()
//        {
//            Activity a = (Activity)mCurrentBusinessFlow.CurrentActivity;
//            a.Reset();
//            while (true)
//            {
//                if (mStopRun)
//                    break;

//                if (a.Active)
//                {
//                    a.Status = eRunStatus.Blocked;
//                    foreach (Act act in a.Acts)
//                    {
//                        if (act.Active)
//                            act.Status = eRunStatus.Blocked;
//                    }
//                }
//                else
//                {
//                    //If activity is not active, mark it as skipped
//                    a.Status = eRunStatus.Skipped;
//                    foreach (Act act in a.Acts)
//                    {
//                        if (act.Status == eRunStatus.Pending)
//                            act.Status = eRunStatus.Skipped;
//                    }
//                }
//                if (mCurrentBusinessFlow.Activities.IsLastItem())
//                    break;
//                else
//                {
//                    GotoNextActivity();
//                    a = (Activity)mCurrentBusinessFlow.CurrentActivity;
//                }
//            }
//        }
//        private void SetNextActionsBlockedStatus()
//        {
//            Act act = (Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;

//            while (true)
//            {
//                if (mStopRun)
//                    break;

//                if (act.Active && act.Status != eRunStatus.Failed) act.Status = eRunStatus.Blocked;
//                if (mCurrentBusinessFlow.CurrentActivity.Acts.IsLastItem()) break;

//                else
//                {
//                    mCurrentBusinessFlow.CurrentActivity.Acts.MoveNext();
//                    act = (Act)mCurrentBusinessFlow.CurrentActivity.Acts.CurrentItem;
//                }
//            }
//        }

//        public void SetSpeed(int Speed)
//        {
//            mSpeed = Speed;
//        }

//        public void StopRun()
//        {
//            // TODO: raise event Stop request
//            mStopRun = true;
//        }
//        public void ResetRun()
//        {
//            mStopRun = false;
//        }

//        internal void CloseAgents()
//        {
//            foreach (ApplicationAgent p in ApplicationAgents)
//            {
//                if (p.Agent != null)
//                {
//                    p.Agent.CloseDriver();
//                }
//            }
//            AgentsRunning = false;
//        }

//        private void ResetActivity(Activity a)
//        {
//            a.Reset();
//        }

//        internal void ClearAgents()
//        {
//            CloseAgents();
//            ApplicationAgents.Clear();
//        }

//        internal void UpdateApplicationAgents()
//        {
//            // Make sure Ginger Runner have all Application/Platforms mapped to agent - create the list based on selected BFs to run
//            // Make it based on current if we run from automate tab

//            //Get the TargetApplication list
//            ObservableList<TargetApplication> bfsTargetApplications = new ObservableList<TargetApplication>();
//            if (BusinessFlows.Count() != 0)// Run Tab
//            {
//                foreach (BusinessFlow BF in BusinessFlows)
//                {
//                    foreach (TargetApplication TA in BF.TargetApplications)
//                    {
//                        if (bfsTargetApplications.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
//                            bfsTargetApplications.Add(TA);
//                    }
//                }
//            }
//            else if (mCurrentBusinessFlow != null) // Automate Tab
//            {
//                foreach (TargetApplication TA in mCurrentBusinessFlow.TargetApplications)
//                {
//                    if (bfsTargetApplications.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
//                        bfsTargetApplications.Add(TA);
//                }
//            }

//            //Remove the non relevant ApplicationAgents
//            for (int indx = 0; indx < ApplicationAgents.Count;)
//            {
//                if (bfsTargetApplications.Where(x => x.AppName == ApplicationAgents[indx].AppName).FirstOrDefault() == null)
//                    ApplicationAgents.RemoveAt(indx);
//                else
//                    indx++;
//            }

//            if (SolutionAgents != null)
//            {
//                //make sure all mapped agents still exist
//                for (int indx = 0; indx < ApplicationAgents.Count; indx++)
//                {
//                    if (ApplicationAgents[indx].Agent != null)
//                        if (SolutionAgents.Where(x => ((RepositoryItem)x).Guid == ((RepositoryItem)ApplicationAgents[indx].Agent).Guid).FirstOrDefault() == null)
//                        {
//                            ApplicationAgents.RemoveAt(indx);
//                            indx--;
//                        }
//                }
//            }

//            //Set the ApplicationAgents
//            foreach (TargetApplication TA in bfsTargetApplications)
//            {
//                // make sure GR got it covered
//                if (ApplicationAgents.Where(x => x.AppName == TA.AppName).FirstOrDefault() == null)
//                {
//                    ApplicationAgent ag = new ApplicationAgent();
//                    ag.AppName = TA.AppName;

//                    //Map agent to the application                    

//                    //TODO: change to find AP by GUID !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//                    // Get the solution AP matching app name
//                    ApplicationPlatform ap = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPlatform>() where x.AppName == ag.AppName select x).FirstOrDefault();

//                    if (ap != null)
//                    {
//                        //Get the last used agent to this Target App if exist
//                        if (string.IsNullOrEmpty(ap.LastMappedAgentName) == false)
//                        {
//                            //check if the saved agent still valid for this application platform      
//                        }
//                    }
//                    ApplicationAgents.Add(ag);
//                }
//            }
//            this.OnPropertyChanged(nameof(ApplicationAgents));//to notify who shows this list
//        }

//        internal ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false, string GingerRunnerName = "")
//        {
//            var BFESs = new ObservableList<BusinessFlowExecutionSummary>();

//            foreach (var BF in BusinessFlows)
//            {
//                // Ignore BF which are not active so they will not be calculated
//                if (BF.Active)
//                {
//                    if (!GetSummaryOnlyForExecutedFlow ||
//                        (GetSummaryOnlyForExecutedFlow && !(BF.RunStatus == eRunStatus.Pending || BF.RunStatus == eRunStatus.Blocked)))
//                    {
//                        var BFES = new BusinessFlowExecutionSummary();
//                        BFES.BusinessFlowName = BF.Name;
//                        BFES.BusinessFlowRunDescription = BF.RunDescription;
//                        BFES.GingerRunnerName = GingerRunnerName;
//                        BFES.Status = BF.RunStatus;
//                        BFES.Activities = BF.Activities.Count;
//                        BFES.Actions = BF.GetActionsCount();
//                        BFES.Validations = BF.GetValidationsCount();
//                        BFES.ExecutionVariabeles = BF.GetBFandActivitiesVariabeles(true);
//                        BFES.BusinessFlow = BF;
//                        BFES.Selected = true;
//                        BFES.BusinessFlowExecLoggerFolder = this.ExecutionLogger.ExecutionLogfolder + BF.ExecutionLogFolder;

//                        BFESs.Add(BFES);
//                    }
//                }
//            }
//            return BFESs;
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return string.Empty;
//            }
//            set
//            {
//                return;
//            }
//        }

//        private bool CheckIfActivityTagsMatch()
//        {
//            //check if Activity or The parent Acitivites Group has at least 1 tag from filter tags
//            //first check Activity
//            foreach (Guid tagGuid in mCurrentBusinessFlow.CurrentActivity.Tags)
//                if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid) == true).FirstOrDefault() != Guid.Empty)
//                    return true;

//            //check in Activity Group
//            if (string.IsNullOrEmpty(mCurrentBusinessFlow.CurrentActivity.ActivitiesGroupID) == false)
//            {
//                ActivitiesGroup group = mCurrentBusinessFlow.ActivitiesGroups.Where(x => x.Name == mCurrentBusinessFlow.CurrentActivity.ActivitiesGroupID).FirstOrDefault();
//                if (group != null)
//                    foreach (Guid tagGuid in group.Tags)
//                        if (this.FilterExecutionTags.Where(x => Guid.Equals(x, tagGuid) == true).FirstOrDefault() != Guid.Empty)
//                            return true;
//            }

//            return false;
//        }

//        void ApplicationAgentsCollectionChanged()
//        {
//            ApplicationAgents.CollectionChanged += ApplicationAgentsCollectionChanged;
//        }

//        private void ApplicationAgentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
//        {
//            foreach (ApplicationAgent applicationAgent in ApplicationAgents)
//            {
//                applicationAgent.PropertyChanged += ApplicationAgentPropertyChanged;
//            }
//        }

//        private void ApplicationAgentPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            if (e.PropertyName == nameof(ApplicationAgent.AppName) || e.PropertyName == nameof(ApplicationAgent.AgentName))
//            {
//                OnPropertyChanged(nameof(ApplicationAgentsInfo));
//            }
//        }

//        public string ApplicationAgentsInfo
//        {
//            get
//            {
//                string s = "";
//                foreach (ApplicationAgent AA in ApplicationAgents)
//                {
//                    if (s.Length > 0) s += ", ";
//                    s += AA.AppName + "/" + AA.AgentName;
//                }
//                return s;
//            }
//        }
//    }
//}