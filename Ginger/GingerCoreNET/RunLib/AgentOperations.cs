#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Run;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Environments;
using GingerCoreNET.RunLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace GingerCore
{

    public class AgentOperations : IAgentOperations
    {
        public Agent Agent;
        public AgentOperations(Agent Agent)
        {
            this.Agent = Agent;
            this.Agent.AgentOperations = this;
        }
        public DriverInfo DriverInfo { get; set; }

        public bool IsWindowExplorerSupportReady
        {
            get
            {
                if (Driver != null) return Driver.IsWindowExplorerSupportReady();

                else return false;
            }
        }
        public bool IsShowWindowExplorerOnStart
        {
            get
            {
                if (Driver != null) return Driver.IsShowWindowExplorerOnStart();

                else return false;
            }
        }

        public bool IsFailedToStart = false;
        private static Object thisLock = new Object();

        public Agent.eStatus Status
        {
            get
            {
                if (IsFailedToStart) { return Agent.eStatus.FailedToStart; }

                if (Agent.mIsStarting) { return Agent.eStatus.Starting; }

                if (Agent.AgentType == Agent.eAgentType.Service)
                {
                    if (gingerNodeInfo != null || GingerNodeProxy != null)
                    {
                        // TODO: verify the correct status from above
                        return Agent.eStatus.Running;
                    }
                    else
                    {
                        return Agent.eStatus.NotStarted;
                    }
                }


                if (Driver == null) { return Agent.eStatus.NotStarted; }
                //TODO: fixme  running called too many - and get stuck
                bool DriverIsRunning = false;
                lock (thisLock)
                {
                    DriverIsRunning = Driver.IsRunning();
                    //Reporter.ToLog(eAppReporterLogLevel.INFO, $"Method - {"get Status"}, IsRunning - {DriverIsRunning}"); //TODO: if needed so need to be more informative including Agent type & name and to be written only in Debug mode of Log
                }
                if (DriverIsRunning) { return Agent.eStatus.Running; }

                return Agent.eStatus.NotStarted;
            }
        }

        private DriverBase mDriver;
        public DriverBase Driver
        {
            get
            {
                return mDriver;
            }
            set
            {
                mDriver = value;
            }
        }

        public Task MSTATask = null;  // For STA Driver we keep the STA task 
        public CancellationTokenSource CTS = null;
        BackgroundWorker CancelTask;



        public async void StartDriver()
        {
            //WorkSpace.Instance.Telemetry.Add("startagent", new { AgentType = Agent.AgentType.ToString(), DriverType = Agent.DriverType.ToString() });

            if (Agent.AgentType == Agent.eAgentType.Service)
            {
                StartPluginService();
                GingerNodeProxy.StartDriver();
                Agent.OnPropertyChanged(nameof(Agent.Status));
            }
            else
            {
                try
                {
                    Agent.mIsStarting = true;
                    Agent.OnPropertyChanged(nameof(Agent.Status));
                    try
                    {
                        if (Agent.Remote)
                        {
                            throw new Exception("Remote is Obsolete, use GingerGrid");
                        }
                        else
                        {
                            Driver = (DriverBase)TargetFrameworkHelper.Helper.GetDriverObject(Agent);
                        }
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to set Agent Driver", e);
                        return;
                    }

                    if (Agent.AgentType == Agent.eAgentType.Service)
                    {
                        throw new Exception("Error - Agent type is service and trying to launch from Ginger.exe"); // we should never get here with service
                    }
                    else
                    {
                        Driver.InitDriver(Agent);
                        Driver.BusinessFlow = Agent.BusinessFlow;
                        SetDriverConfiguration();

                        IVirtualDriver VirtualDriver = null;
                        if (Driver is IVirtualDriver VD)
                        {
                            VirtualDriver = VD;
                            string ErrorMessage;
                            if (!VirtualDriver.CanStartAnotherInstance(out ErrorMessage))
                            {
                                throw new NotSupportedException(ErrorMessage);

                            }
                        }
                        //if STA we need to start it on seperate thread, so UI/Window can be refreshed: Like IB, Mobile, Unix
                        if (Driver is IDriverWindow && ((IDriverWindow)Driver).ShowWindow)
                        {
                            DriversWindowUtils.OnDriverWindowEvent(DriverWindowEventArgs.eEventType.ShowDriverWindow, Driver, this);
                            Driver.StartDriver();
                        }
                        else if (Driver.IsSTAThread())
                        {
                            CTS = new CancellationTokenSource();
                            MSTATask = new Task(() =>
                            {
                                Driver.StartDriver();
                            }
                            , CTS.Token, TaskCreationOptions.LongRunning);
                            MSTATask.Start();
                        }
                        else
                        {
                                Driver.StartDriver();
                        }
                        if (VirtualDriver != null)
                        {
                            VirtualDriver.DriverStarted(Agent.Guid.ToString());
                        }
                    }
                }
                finally
                {
                    if (Agent.AgentType == Agent.eAgentType.Service)
                    {
                        Agent.mIsStarting = false;
                    }
                    else
                    {
                        if (Driver != null)
                        {
                            // Give the driver time to start            
                            Thread.Sleep(500);
                            Driver.IsDriverRunning = true;
                            Driver.DriverMessageEvent += driverMessageEventHandler;
                        }

                        Agent.mIsStarting = false;
                        Agent.OnPropertyChanged(nameof(Agent.Status));
                        Agent.OnPropertyChanged(nameof(IsWindowExplorerSupportReady));
                    }
                }
            }
        }

        System.Diagnostics.Process mProcess;
        Mutex mutex = new Mutex();
        // TODO: move to ExecuteOnPlugin
        public void StartPluginService()
        {
            try
            {
                // Enable to start one plugin each time so will let the plugin reserve and avoid race condition
                // TODO: consider using lock
                mutex.WaitOne();

                // First we try on local Ginger Grid
                gingerNodeInfo = FindFreeNode(Agent.ServiceId);

                if (gingerNodeInfo == null)
                {
                    // Try to find service on Remote Grid                    
                    ObservableList<RemoteServiceGrid> remoteServiceGrids = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>();
                    GingerNodeProxy = GingerNodeProxy.FindRemoteNode(Agent.ServiceId, remoteServiceGrids);
                    if (GingerNodeProxy != null)
                    {
                        // We found the service on remote grid
                        return;
                    }
                }

                // Service not found start new one on local
                // Add plugin config start if not exist and more depends on the config 
                if (gingerNodeInfo == null)
                {
                    // Dup with GR consolidate with timeout
                    mProcess = WorkSpace.Instance.PlugInsManager.StartService(Agent.PluginId, Agent.ServiceId);
                }

                Stopwatch st = Stopwatch.StartNew();
                while (gingerNodeInfo == null && st.ElapsedMilliseconds < 30000) // max 30 seconds to wait
                {
                    gingerNodeInfo = FindFreeNode(Agent.ServiceId);
                    if (gingerNodeInfo != null) break;
                    Thread.Sleep(100);
                }

                if (gingerNodeInfo == null)
                {
                    throw new Exception("Plugin not started " + Agent.PluginId);
                }


                gingerNodeInfo.Status = GingerNodeInfo.eStatus.Reserved;
                // TODO: add by which agent to GNI

                // Keep GNP on agent
                GingerNodeProxy = WorkSpace.Instance.LocalGingerGrid.GetNodeProxy(gingerNodeInfo);

                //TODO: Ginger Grid  CHeck if required                GingerNodeProxy.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
                GingerNodeProxy.StartDriver(Agent.DriverConfiguration);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                mutex.ReleaseMutex();
            }

        }



        private GingerNodeInfo FindFreeNode(string serviceId)
        {
            // TODO: add more filters from agent config like OS, machine id ,plugin version and more

            // Find the first free service which match and Ready (not reserved)
            gingerNodeInfo = (from x in WorkSpace.Instance.LocalGingerGrid.NodeList where x.ServiceId == Agent.ServiceId && x.Status == GingerNodeInfo.eStatus.Ready select x).FirstOrDefault();  // Keep First!!!
            return gingerNodeInfo;
        }

        public void driverMessageEventHandler(object sender, DriverMessageEventArgs e)
        {
            if (e.DriverMessageType == DriverBase.eDriverMessageType.DriverStatusChanged)
            {
                Agent.OnPropertyChanged(nameof(AgentOperations.Status));
            }
        }


        public void SetDriverConfiguration()
        {
            if (Agent.DriverConfiguration == null)
            {
                return;
            }

            if (Agent.ProjEnvironment == null)
            {
                Agent.ProjEnvironment = new Environments.ProjEnvironment();//to avoid value expertion exception
            }
            if (Agent.BusinessFlow == null)
            {
                Agent.BusinessFlow = new GingerCore.BusinessFlow();//to avoid value expertion exception
            }
            ValueExpression ve = new ValueExpression(Agent.ProjEnvironment, Agent.BusinessFlow, Agent.DSList);

            if (Agent.AgentType == Agent.eAgentType.Service)
            {
                SetServiceConfiguration();
            }
            else
            {
                Agent.DriverClass = TargetFrameworkHelper.Helper.GetDriverType(Agent);

                SetDriverMissingParams(Agent.DriverClass);


                foreach (DriverConfigParam DCP in Agent.DriverConfiguration)
                {
                    string value = null;
                    ObservableList<DriverConfigParam> multiValues = null;

                    //process Value expression in case used                    
                    if (DCP.MultiValues != null)
                    {
                        multiValues = new ObservableList<DriverConfigParam>();
                        foreach (DriverConfigParam subValue in DCP.MultiValues)
                        {
                            ve.Value = subValue.Value;
                            multiValues.Add(new DriverConfigParam() { Parameter = subValue.Parameter, Value = ve.ValueCalculated });
                        }
                    }
                    else
                    {
                        ve.Value = DCP.Value;
                        value = ve.ValueCalculated;
                    }

                    PropertyInfo driverProp = Driver.GetType().GetProperty(DCP.Parameter);
                    if (driverProp != null)
                    {
                        //set multi values prop
                        if (DCP.MultiValues != null)
                        {
                            Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, multiValues);
                            continue;
                        }

                        //set eNum prop
                        UserConfiguredEnumTypeAttribute enumTypeConfig = null;
                        try
                        {
                            MemberInfo[] mf = Driver.GetType().GetMember(DCP.Parameter);
                            if (mf != null)
                            {
                                enumTypeConfig = Attribute.GetCustomAttribute(mf[0], typeof(UserConfiguredEnumTypeAttribute), false) as UserConfiguredEnumTypeAttribute;
                                if (enumTypeConfig != null)
                                {
                                    Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, Enum.Parse(enumTypeConfig.EnumType, value));
                                    continue;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to check if the driver configuration '{0}' is from EnumType", DCP.Parameter), ex);
                        }

                        //set standard prop types
                        string driverPropType = driverProp.PropertyType.Name;
                        switch (driverPropType)
                        {
                            case "String":
                                Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, value);
                                break;
                            case "Boolean":
                                try
                                {
                                    Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, Convert.ToBoolean(value));
                                }
                                catch (Exception)
                                {
                                    Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, true);
                                }
                                break;
                            case "Int32":
                                Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, int.Parse(value));
                                break;
                            default:
                                Reporter.ToUser(eUserMsgKey.SetDriverConfigTypeNotHandled, DCP.GetType().ToString());
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("The driver configuration '{0}' field type '{1}' is unknown", DCP.Parameter, driverPropType));
                                break;
                        }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, string.Format("The driver configuration '{0}' was not found on the driver class", DCP.Parameter));
                    }
                }

                Driver.AdvanceDriverConfigurations = Agent.AdvanceAgentConfigurations;
            }
        }



        public void InitDriverConfigs()
        {
            if (Agent.DriverConfiguration == null)
            {
                Agent.DriverConfiguration = new ObservableList<DriverConfigParam>();
            }
            else
            {
                Agent.DriverConfiguration.Clear();
            }

            if (Agent.AgentType == Agent.eAgentType.Driver)
            {
                Type driverType = TargetFrameworkHelper.Helper.GetDriverType(Agent);
                SetDriverDefualtParams(driverType);
            }
            else if (Agent.AgentType == Agent.eAgentType.Service)
            {
                SetServiceConfiguration();
            }
        }

        private void SetServiceConfiguration()
        {
            Agent.DriverConfiguration.Clear();
            SetServiceMissingParams();
        }

        private void SetServiceMissingParams()
        {

            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            foreach (PluginPackage pluginPackage in Plugins)
            {
                pluginPackage.PluginPackageOperations = new PluginPackageOperations(pluginPackage);
            }
            IEnumerable<PluginServiceInfo> Services = Plugins.SelectMany(x => ((PluginPackageOperations)x.PluginPackageOperations).Services);
            PluginServiceInfo PSI = Services.Where(x => x.ServiceId == Agent.ServiceId).FirstOrDefault();

            PluginPackage PP = Plugins.Where(x => ((PluginPackageOperations)x.PluginPackageOperations).Services.Contains(PSI)).First();
            PP.PluginPackageOperations = new PluginPackageOperations(PP);

            PP.PluginPackageOperations.LoadServicesFromJSON();
            PSI = ((PluginPackageOperations)PP.PluginPackageOperations).Services.Where(x => x.ServiceId == Agent.ServiceId).FirstOrDefault();

            foreach (var config in PSI.Configs)
            {
                if (Agent.DriverConfiguration.Where(x => x.Parameter == config.Name).Count() == 0)
                {
                    DriverConfigParam DI = new DriverConfigParam();
                    DI.Parameter = config.Name;
                    DI.Value = config.DefaultValue;
                    DI.Description = config.Description;
                    DI.OptionalValues = config.OptionalValues;
                    DI.Type = config.Type;
                    Agent.DriverConfiguration.Add(DI);
                }
            }

            SetPlatformParameters(PSI);

        }
        /// <summary>
        /// Set AGent Configuration with default values in addition to the configurations asked by Service 
        /// </summary>
        /// <param name="PSI"></param>
        private void SetPlatformParameters(PluginServiceInfo PSI)
        {
            if (PSI.Interfaces.Where(x => x == "IWebPlatform").Count() > 0)
            {
                DriverConfigParam DI = new DriverConfigParam();
                DI.Parameter = "Max Agent Load Time";
                DI.Value = "30";
                DI.Description = "Max Time allowed in seconds to start the agent0";

                DI.IsPlatformParameter = true;

                Agent.DriverConfiguration.Add(DI);


                DriverConfigParam DI2 = new DriverConfigParam();
                DI2.Parameter = "Auto Switch Frame";
                DI2.Value = bool.TrueString;
                DI2.Description = "Automatic Switch Frame for POM Element";

                DI2.IsPlatformParameter = true;

                Agent.DriverConfiguration.Add(DI2);


            }
            else if (PSI.Interfaces.Where(x => x == "IWebServicePlatform").Count() > 0)
            {
                DriverConfigParam DI = new DriverConfigParam();
                DI.Parameter = "Save Request";
                DI.Value = bool.FalseString;
                DI.Description = "Save Request";

                DI.IsPlatformParameter = true;

                Agent.DriverConfiguration.Add(DI);


                DriverConfigParam DI2 = new DriverConfigParam();
                DI2.Parameter = "Save Response";
                DI2.Value = bool.TrueString;
                DI2.Description = "Save Response";

                DI2.IsPlatformParameter = true;

                Agent.DriverConfiguration.Add(DI2);


                DriverConfigParam DI3 = new DriverConfigParam();
                DI3.Parameter = "Path To Save";
                DI3.Value = @"~\Documents";
                DI3.Description = "Path to Save Request/Response Files";

                DI3.IsPlatformParameter = true;

                Agent.DriverConfiguration.Add(DI3);




            }
        }
        private void SetDriverDefualtParams(Type t)
        {
            MemberInfo[] members = t.GetMembers();
            UserConfiguredAttribute token = null;

            foreach (MemberInfo mi in members)
            {
                token = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredAttribute), false) as UserConfiguredAttribute;

                if (token == null)
                    continue;
                DriverConfigParam configParam = GetDriverConfigParam(mi);

                Agent.DriverConfiguration.Add(configParam);
            }
        }
        /// <summary>
        /// This function will add missing Driver config parameters to Driver configuration
        /// </summary>
        /// <param name="driverType"> Type of the driver</param>
        private void SetDriverMissingParams(Type driverType)
        {

            MemberInfo[] members = driverType.GetMembers();
            UserConfiguredAttribute token = null;

            foreach (MemberInfo mi in members)
            {
                token = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredAttribute), false) as UserConfiguredAttribute;

                if (token == null)
                    continue;
                DriverConfigParam configParam = GetDriverConfigParam(mi);

                if (Agent.DriverConfiguration.Where(x => x.Parameter == configParam.Parameter).FirstOrDefault() == null)
                {
                    Agent.DriverConfiguration.Add(configParam);
                }

            }
        }


        private DriverConfigParam GetDriverConfigParam(MemberInfo mi)
        {
            UserConfiguredDefaultAttribute defaultVal = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredDefaultAttribute), false) as UserConfiguredDefaultAttribute;
            UserConfiguredDescriptionAttribute desc = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredDescriptionAttribute), false) as UserConfiguredDescriptionAttribute;
            UserConfiguredMultiValuesAttribute muliValues = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredMultiValuesAttribute), false) as UserConfiguredMultiValuesAttribute;

            DriverConfigParam DCP = new DriverConfigParam();
            DCP.Parameter = mi.Name;
            if (defaultVal != null)
            {
                DCP.Value = defaultVal.DefaultValue;
            }
            if (muliValues != null)
            {
                DCP.MultiValues = new ObservableList<DriverConfigParam>();
            }
            if (desc != null)
            {
                DCP.Description = desc.Description;
            }
            return DCP;
        }


        // We cache the GingerNodeProxy
        public GingerNodeProxy GingerNodeProxy { get; set; }

        // We keep the GingerNodeInfo for Plugin driver
        private GingerNodeInfo gingerNodeInfo;

        public void RunAction(Act act)
        {
            try
            {
                if (Driver != null)
                {
                    if (Driver.IsSTAThread())
                    {
                        Driver.Dispatcher.Invoke(() =>
                        {
                            Driver.RunAction(act);
                        });
                    }
                    else
                    {
                        Driver.RunAction(act);
                    }
                }
            }
            catch (Exception ex)
            {
                act.Status = eRunStatus.Failed;
                act.Error += ex.Message;
            }
        }



        public async void Close()
        {
            try
            {
                if (Agent.AgentType == Agent.eAgentType.Service)
                {
                    if (gingerNodeInfo != null)
                    {
                        // this is plugin driver on local machine

                        GingerNodeProxy.GingerGrid = WorkSpace.Instance.LocalGingerGrid;

                        GingerNodeProxy.CloseDriver();


                        gingerNodeInfo.Status = GingerNodeInfo.eStatus.Ready;


                        if (mProcess != null) // it means a new plugin process was started for this agent - so we close it
                        {
                            // Remove the node from the grid
                            WorkSpace.Instance.LocalGingerGrid.NodeList.Remove(gingerNodeInfo);

                            // Close the plugin process
                            mProcess.CloseMainWindow();
                        }

                        GingerNodeProxy = null;
                        gingerNodeInfo = null;

                        return;
                    }
                    else
                    {
                        if (GingerNodeProxy != null)
                        {
                            // Running on Remote Grid
                            GingerNodeProxy.CloseDriver();
                            GingerNodeProxy.Disconnect();
                            GingerNodeProxy = null;
                        }
                    }
                }
                if (Driver == null) return;

                Driver.IsDriverRunning = false;
                if (Driver is IDriverWindow && ((IDriverWindow)Driver).ShowWindow)
                {
                    DriversWindowUtils.OnDriverWindowEvent(DriverWindowEventArgs.eEventType.CloseDriverWindow, Driver, this);
                    Driver.CloseDriver();
                }
                else if (Driver.Dispatcher != null)
                {
                    Driver.Dispatcher.Invoke(() =>
                   {
                       Driver.CloseDriver();
                       Thread.Sleep(1000);
                   });
                }
                else
                {
                    await Task.Run(() =>
                    {
                        Driver.CloseDriver();
                    });
                }
                if (MSTATask != null)
                {
                    // Using cancellation token source to cancel 
                    CancelTask = new BackgroundWorker();
                    CancelTask.DoWork += new DoWorkEventHandler(Agent.CancelTMSTATask);
                    CancelTask.RunWorkerAsync();
                }

                Driver = null;
            }
            finally
            {
                Agent.OnPropertyChanged(nameof(AgentOperations.Status));
                Agent.OnPropertyChanged(nameof(AgentOperations.IsWindowExplorerSupportReady));
            }
        }

        public void ResetAgentStatus(Agent.eStatus status)
        {
            if (status == Agent.eStatus.FailedToStart)
            {
                IsFailedToStart = false;
            }
        }
        //private void CancelTMSTATask(object sender, DoWorkEventArgs e)
        //{
        //    if (MSTATask == null)
        //        return;

        //    // Using cancellation token source to cancel  getting exceptions when trying to close agent and task is in running condition

        //    while (MSTATask != null && !(MSTATask.Status == TaskStatus.RanToCompletion || MSTATask.Status == TaskStatus.Faulted || MSTATask.Status == TaskStatus.Canceled))
        //    {

        //        CTS.Cancel();
        //        Thread.Sleep(100);
        //    }
        //    CTS.Dispose();
        //    MSTATask = null;
        //}
        public void HighLightElement(Act act)
        {
            if (Driver != null)
            {
                Driver.HighlightActElement(act);
            }
        }



        public void WaitForAgentToBeReady()
        {            
            int Counter = 0;
            while (Status != Agent.eStatus.Running && String.IsNullOrEmpty(Driver.ErrorMessageFromDriver))
            {
                // TODO: run on another thread?? !!!!!!!!!!!!!!!!!!!!
                //GingerCore.General.DoEvents ();
                Thread.Sleep(100);
                Counter++;

                int waitingTime = 30;// 30 seconds
                if (Driver.DriverLoadWaitingTime > 0)
                    waitingTime = Driver.DriverLoadWaitingTime;
                Double waitingTimeInMilliseconds = waitingTime * 10;
                if (Counter > waitingTimeInMilliseconds)
                {
                    if (Driver != null && string.IsNullOrEmpty(Driver.ErrorMessageFromDriver))
                    {
                        Driver.ErrorMessageFromDriver = "Failed to start the agent after waiting for " + waitingTime + " seconds";
                    }
                    return;
                }
            }
        }
        //public bool UsedForAutoMapping { get; set; } = false;


        public void Test()
        {
            if (Status == Agent.eStatus.Running)
            {
                Close();
            }

            Agent.SolutionFolder = WorkSpace.Instance.SolutionRepository.SolutionFolder;
            try
            {
                StartDriver();
                if (Driver != null && String.IsNullOrEmpty(Driver.ErrorMessageFromDriver))
                {
                    WaitForAgentToBeReady();
                }

                if (Status == Agent.eStatus.Running)
                {
                    Reporter.ToUser(eUserMsgKey.TestagentSucceed);
                }
                else if (Driver.ErrorMessageFromDriver!=null && Driver.ErrorMessageFromDriver.Contains("Chrome driver version mismatch"))
                {
                    Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, Agent.Name, "Chrome driver version mismatch. Please run Ginger as Admin to Auto update the chrome driver.");
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, Agent.Name, "Please confirm Agent configurations are valid");
                }
            }
            catch (Exception AgentStartException)
            {
                Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, Agent.Name, AgentStartException.Message);
            }
            finally
            {
                Close();
            }
        }


        public List<DriverBase> VirtualAgentsStarted()
        {
            List<DriverBase> CurrentDrivers = new List<DriverBase>();

            foreach (var drv in DriverBase.VirtualDrivers.Where(x => x.Key == Agent.Guid.ToString() || x.Key == Agent.ParentGuid.ToString()))
            {
                CurrentDrivers.Add(drv.Value);
            }




            return CurrentDrivers;
        }

    }
}
