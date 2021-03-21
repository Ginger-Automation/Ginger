#region License
/*
Copyright © 2014-2020 European Support Limited

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

    public class Agent : RepositoryItemBase, IAgent
    {

        public enum eAgentType
        {
            Driver,  // old legacy drivers - default
            Service // new Plugin Service driver with session
        }

        [IsSerializedForLocalRepository]
        public eAgentType AgentType { get; set; }

        [IsSerializedForLocalRepository]
        public string PluginId { get; set; }   // only for AgentType plugin

        [IsSerializedForLocalRepository]
        public string ServiceId { get; set; }   // only for AgentType plugin

        public DriverInfo DriverInfo { get; set; }
        public enum eDriverType
        {
            //Web
            [Description("Internal Browser")]
            InternalBrowser,
            [Description("Internet Explorer Browser(Selenium)")]
            SeleniumIE,
            [Description("Fire Fox Browser(Selenium)")]
            SeleniumFireFox,
            [Description("Chrome Browser(Selenium)")]
            SeleniumChrome,
            [Description("Remote Browser(Selenium)")]
            SeleniumRemoteWebDriver,
            [Description("Edge Browser(Selenium)")]
            SeleniumEdge,
            [Description("PhantomJS Browser(Selenium)")]
            SeleniumPhantomJS,//Not been used any more, leaving here to avoid exception on solution load

            //Java
            [Description("Amdocs Smart Client Framework(ASCF)")]
            ASCF,
            [Description("Java")]
            JavaDriver,

            //Console
            [Description("DOS Console")]
            DOSConsole,
            [Description("Unix Shell")]
            UnixShell,

            //Web Service
            [Description("Web Services")]
            WebServices,

            //Windows
            [Description("Windows (UIAutomation)")]           
            WindowsAutomation,
           
            //PowerBuilder          
            [Description("Power Builder")]
            PowerBuilder,

            //Mobile
            [Description("Mobile Appium Android")]
            MobileAppiumAndroid,
            [Description("Mobile Appium IOS")]
            MobileAppiumIOS,
            [Description("Mobile Appium Android Browser")]
            MobileAppiumAndroidBrowser,
            [Description("Mobile Appium IOS Browser")]
            MobileAppiumIOSBrowser,
            [Description("Mobile Perfecto Android")]
            PerfectoMobileAndroid,
            [Description("Mobile Perfecto Android Browser")]
            PerfectoMobileAndroidWeb,
            [Description("Mobile Perfecto IOS")]
            PerfectoMobileIOS,
            [Description("Mobile Perfecto IOS Browser")]
            PerfectoMobileIOSWeb,
            [Description("Generic Appium")]
            Appium,

            //MF
            [Description("MainFrame 3270")]
            MainFrame3270,

            NA
        }

        public enum eStatus
        {
            [EnumValueDescription("Not Started")]
            NotStarted,
            Starting,
            Ready,
            Running,
            Completed,
            FailedToStart
        }

        public static class Fields
        {
            public static string Guid = "Guid";
            public static string Active = "Active";
            public static string Name = "Name";
            public static string Host = "Host";
            public static string Port = "Port";
            public static string Status = "Status";
            public static string DriverType = "DriverType";
            public static string Remote = "Remote";
            public static string Notes = "Notes";
            public static string Platform = "Platform";
            public static string IsWindowExplorerSupportReady = "IsWindowExplorerSupportReady";
            public static string DriverInfo = "DriverInfo";
        }

        public bool IsWindowExplorerSupportReady
        {
            get
            {
                if (Driver != null) return Driver.IsWindowExplorerSupportReady();

                else return false;
            }
        }
        internal Type DriverClass = null;
        public bool IsShowWindowExplorerOnStart
        {
            get
            {
                if (Driver != null) return Driver.IsShowWindowExplorerOnStart();

                else return false;
            }
        }

        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<DriverConfigParam> AdvanceAgentConfigurations = new ObservableList<DriverConfigParam>();

        public bool mIsStarting = false;

        private string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(Fields.Name);
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = new ObservableList<Guid>();

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).Where(x => tagGuid.Equals(x) == true).FirstOrDefault();
                        if (!guid.Equals(Guid.Empty))
                            return true;
                    }
                    break;
            }
            return false;
        }

        #region Remote Agent
        //Used for Remote Agent - if this is remote agent then the Host and Port should be specified
        // DO NOT USE this is old, need to use new plugin in on remote grid
        [IsSerializedForLocalRepository]
        public bool Remote { get; set; }


        [IsSerializedForLocalRepository]
        public string Host { get; set; }

        [IsSerializedForLocalRepository]
        public int Port { get; set; }

        #endregion

        public bool IsFailedToStart = false;
        private static Object thisLock = new Object();

        public eStatus Status
        {
            get
            {
                if (IsFailedToStart) return eStatus.FailedToStart;

                if (mIsStarting) return eStatus.Starting;

                if (AgentType == eAgentType.Service)
                {
                    if (gingerNodeInfo != null || GingerNodeProxy != null)
                    {
                        // TODO: verify the correct status from above
                        return eStatus.Running;
                    }
                    else
                    {
                        return eStatus.NotStarted;
                    }
                }


                if (Driver == null) return eStatus.NotStarted;
                //TODO: fixme  running called too many - and get stuck
                bool DriverIsRunning = false;
                lock (thisLock)
                {
                    DriverIsRunning = Driver.IsRunning();
                    //Reporter.ToLog(eAppReporterLogLevel.INFO, $"Method - {"get Status"}, IsRunning - {DriverIsRunning}"); //TODO: if needed so need to be more informative including Agent type & name and to be written only in Debug mode of Log
                }
                if (DriverIsRunning) return eStatus.Running;

                return eStatus.NotStarted;
            }
        }
        
        private eDriverType mDriverType;
        [IsSerializedForLocalRepository]
        public eDriverType DriverType
        {
            get
            {
                return mDriverType;
            }
            set
            {
                if (mDriverType != value)
                {
                    mDriverType = value;
                    OnPropertyChanged(nameof(DriverType));
                }
            }
        }

        /// <summary>
        /// This method is used to check if the paltform supports POM
        /// </summary>
        /// <returns></returns>
        public bool IsSupportRecording()
        {
            bool isSupport = false;
            switch (DriverType)
            {
                case eDriverType.SeleniumFireFox:
                case eDriverType.SeleniumChrome:
                case eDriverType.SeleniumIE:
                case eDriverType.SeleniumRemoteWebDriver:
                case eDriverType.SeleniumEdge:
                case eDriverType.ASCF:
                case eDriverType.MobileAppiumAndroid:
                case eDriverType.MobileAppiumIOS:
                case eDriverType.MobileAppiumAndroidBrowser:
                case eDriverType.MobileAppiumIOSBrowser:
                case eDriverType.JavaDriver:
                    isSupport = true;
                    break;
            }
            return isSupport;
        }

        private string mNotes;
        [IsSerializedForLocalRepository]
        public string Notes { get { return mNotes; } set { if (mNotes != value) { mNotes = value; OnPropertyChanged(nameof(Notes)); } } }

        [IsSerializedForLocalRepository]
        public ObservableList<DriverConfigParam> DriverConfiguration { get; set; }

        public ProjEnvironment ProjEnvironment { get; set; }

        public string SolutionFolder { get; set; }

        public ObservableList<DataSourceBase> DSList { get; set; }



        private BusinessFlow mBusinessFlow;
        public BusinessFlow BusinessFlow
        {
            get { return mBusinessFlow; }
            set
            {
                if (!object.ReferenceEquals(mBusinessFlow, value))
                {
                    mBusinessFlow = value;
                }
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



        public void StartDriver()
        {
            WorkSpace.Instance.Telemetry.Add("startagent", new { AgentType = AgentType.ToString(), DriverType = DriverType.ToString() });

            if (AgentType == eAgentType.Service)
            {
                StartPluginService();
                GingerNodeProxy.StartDriver();
                OnPropertyChanged(Fields.Status);
            }
            else
            {
                try
                {
                    mIsStarting = true;
                    OnPropertyChanged(nameof(Agent.Status));
                    try
                    {
                        if (Remote)
                        {
                            throw new Exception("Remote is Obsolete, use GingerGrid");
                        }
                        else
                        {
                            Driver = (DriverBase)TargetFrameworkHelper.Helper.GetDriverObject(this);
                        }
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to set Agent Driver", e);
                        return;
                    }

                    if (AgentType == Agent.eAgentType.Service)
                    {
                        throw new Exception("Error - Agent type is service and trying to launch from Ginger.exe"); // we should never get here with service
                    }
                    else
                    {
                        Driver.InitDriver(this);
                        Driver.BusinessFlow = BusinessFlow;
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
                            VirtualDriver.DriverStarted(this.Guid.ToString());
                        }
                    }
                }
                finally
                {
                    if (AgentType == Agent.eAgentType.Service)
                    {
                        mIsStarting = false;
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

                        mIsStarting = false;
                        OnPropertyChanged(nameof(Agent.Status));
                        OnPropertyChanged(nameof(Agent.IsWindowExplorerSupportReady));
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
                gingerNodeInfo = FindFreeNode(ServiceId);
                
                if (gingerNodeInfo == null)
                {
                    // Try to find service on Remote Grid                    
                    ObservableList<RemoteServiceGrid> remoteServiceGrids = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RemoteServiceGrid>();                                        
                    GingerNodeProxy = GingerNodeProxy.FindRemoteNode(ServiceId, remoteServiceGrids);
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
                    mProcess = WorkSpace.Instance.PlugInsManager.StartService(PluginId, ServiceId);
                }

                Stopwatch st = Stopwatch.StartNew();
                while (gingerNodeInfo == null && st.ElapsedMilliseconds < 30000) // max 30 seconds to wait
                {
                    gingerNodeInfo = FindFreeNode(ServiceId);
                    if (gingerNodeInfo != null) break;
                    Thread.Sleep(100);
                }

                if (gingerNodeInfo == null)
                {
                    throw new Exception("Plugin not started " + PluginId);
                }


                gingerNodeInfo.Status = GingerNodeInfo.eStatus.Reserved;
                // TODO: add by which agent to GNI

                // Keep GNP on agent
                GingerNodeProxy = WorkSpace.Instance.LocalGingerGrid.GetNodeProxy(gingerNodeInfo);
 
 //TODO: Ginger Grid  CHeck if required                GingerNodeProxy.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
                GingerNodeProxy.StartDriver(DriverConfiguration);
            }
            catch(Exception ex)
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
            gingerNodeInfo = (from x in WorkSpace.Instance.LocalGingerGrid.NodeList where x.ServiceId == ServiceId && x.Status == GingerNodeInfo.eStatus.Ready select x).FirstOrDefault();  // Keep First!!!
            return gingerNodeInfo;
        }

        public void driverMessageEventHandler(object sender, DriverMessageEventArgs e)
        {
            if (e.DriverMessageType == DriverBase.eDriverMessageType.DriverStatusChanged)
            {
                OnPropertyChanged(Fields.Status);
            }
        }
        /// <summary>
        /// Check if agent support virtual drivers 
        /// </summary>
        /// <returns></returns>
        public bool SupportVirtualAgent()
        {
            try
            {

                if (DriverClass == null)
            {
                DriverClass = TargetFrameworkHelper.Helper.GetDriverType(this);
                    if (DriverClass == null)
                    {
                        return false;
                    }
            }

 
                if (DriverClass.GetInterfaces().Contains(typeof(IVirtualDriver)))
                {
                    return true;
                }
            }
            //if the exceptions are throws we consider it to be not supportable for virtual agents
            catch(Exception e)
            {
                

            }
            return false;
        }
        public void SetDriverConfiguration()
        {
            if (DriverConfiguration == null)
            {
                return;
            }

            if (ProjEnvironment == null)
            {
                ProjEnvironment = new Environments.ProjEnvironment();//to avoid value expertion exception
            }
            if (BusinessFlow == null)
            {
                BusinessFlow = new GingerCore.BusinessFlow();//to avoid value expertion exception
            }
            ValueExpression ve = new ValueExpression(ProjEnvironment, BusinessFlow, DSList);

            if (AgentType == eAgentType.Service)
            {
                SetServiceConfiguration();
            }
            else
            {
                DriverClass = TargetFrameworkHelper.Helper.GetDriverType(this);

                SetDriverMissingParams(DriverClass);

                foreach (DriverConfigParam DCP in DriverConfiguration)
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

                Driver.AdvanceDriverConfigurations = this.AdvanceAgentConfigurations;
            }
        }

        

        public void InitDriverConfigs()
        {
            if (DriverConfiguration == null)
            {
                DriverConfiguration = new ObservableList<DriverConfigParam>();
            }
            else
            {
                DriverConfiguration.Clear();
            }

            if (AgentType == eAgentType.Driver)
            {
                Type driverType = TargetFrameworkHelper.Helper.GetDriverType(this);
                SetDriverDefualtParams(driverType);
            }
            else if (AgentType == eAgentType.Service)
            {
                SetServiceConfiguration();
            }
        }

        private void SetServiceConfiguration()
        {
            DriverConfiguration.Clear();
            SetServiceMissingParams();           
        }

        private void SetServiceMissingParams()
        {

            ObservableList<PluginPackage> Plugins = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            IEnumerable<PluginServiceInfo> Services = Plugins.SelectMany(x => x.Services);
            PluginServiceInfo PSI = Services.Where(x => x.ServiceId == ServiceId).FirstOrDefault();

            PluginPackage PP = Plugins.Where(x => x.Services.Contains(PSI)).First();
            PP.LoadServicesFromJSON();
            PSI = PP.Services.Where(x => x.ServiceId == ServiceId).FirstOrDefault();

            foreach (var config in PSI.Configs)
            {
                if (DriverConfiguration.Where(x => x.Parameter == config.Name).Count() == 0)
                {
                    DriverConfigParam DI = new DriverConfigParam();
                    DI.Parameter = config.Name;
                    DI.Value = config.DefaultValue;
                    DI.Description = config.Description;
                    DI.OptionalValues = config.OptionalValues;
                    DI.Type = config.Type;
                    DriverConfiguration.Add(DI);
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
          
                DriverConfiguration.Add(DI);


                DriverConfigParam DI2 = new DriverConfigParam();
                DI2.Parameter = "Auto Switch Frame";
                DI2.Value = bool.TrueString;
                DI2.Description = "Automatic Switch Frame for POM Element";

                DI2.IsPlatformParameter = true;

                DriverConfiguration.Add(DI2);


            }
            else if (PSI.Interfaces.Where(x => x == "IWebServicePlatform").Count() > 0)
            {
                DriverConfigParam DI = new DriverConfigParam();
                DI.Parameter = "Save Request";
                DI.Value = bool.FalseString;
                DI.Description = "Save Request";

                DI.IsPlatformParameter = true;

                DriverConfiguration.Add(DI);


                DriverConfigParam DI2 = new DriverConfigParam();
                DI2.Parameter = "Save Response";
                DI2.Value = bool.TrueString;
                DI2.Description = "Save Response";

                DI2.IsPlatformParameter = true;

                DriverConfiguration.Add(DI2);


                DriverConfigParam DI3 = new DriverConfigParam();
                DI3.Parameter = "Path To Save";
                DI3.Value = @"~\Documents";
                DI3.Description = "Path to Save Request/Response Files";

                DI3.IsPlatformParameter = true;

                DriverConfiguration.Add(DI3);


            
       
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

                DriverConfiguration.Add(configParam);
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

                if (DriverConfiguration.Where(x => x.Parameter == configParam.Parameter).FirstOrDefault() == null)
                {
                    DriverConfiguration.Add(configParam);
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

        public override string GetNameForFileName()
        {
            return Name;
        }

        public void Close()
        {
            try
            {
                if (AgentType == eAgentType.Service)
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
                    Driver.CloseDriver();
                }
                if (MSTATask != null)
                {
                    // Using cancellation token source to cancel 
                    CancelTask = new BackgroundWorker();
                    CancelTask.DoWork += new DoWorkEventHandler(CancelTMSTATask);
                    CancelTask.RunWorkerAsync();
                }

                Driver = null;
            }
            finally
            {
                OnPropertyChanged(Fields.Status);
                OnPropertyChanged(Fields.IsWindowExplorerSupportReady);
            }
        }

        public void ResetAgentStatus(eStatus status)
        {
            if (status == eStatus.FailedToStart)
            {
                IsFailedToStart = false;
            }
        }
        private void CancelTMSTATask(object sender, DoWorkEventArgs e)
        {
            if (MSTATask == null)
                return;
            
                // Using cancellation token source to cancel  getting exceptions when trying to close agent and task is in running condition

                while(MSTATask!=null &&   !(MSTATask.Status==TaskStatus.RanToCompletion ||MSTATask.Status== TaskStatus.Faulted ||MSTATask.Status== TaskStatus.Canceled))
                {
                   
                       CTS.Cancel();
                    Thread.Sleep(100);
                }
               CTS.Dispose();
                MSTATask = null;
        }
        public void HighLightElement(Act act)
        {
            if(Driver!=null)
            Driver.HighlightActElement(act);
        }

        private ePlatformType? mPlatform;

        [IsSerializedForLocalRepository]
        public ePlatformType Platform
        {
            get
            {
                if (mPlatform != null)
                {
                    return mPlatform.Value;
                }
                else
                {
                    if (AgentType == eAgentType.Service)
                    {
                        return ePlatformType.NA;
                    }
                    else
                    {
                        return GetDriverPlatformType(DriverType);
                    }
                }
            }
            set
            {

                mPlatform = value;
            }
        }

        public static ePlatformType GetDriverPlatformType(eDriverType driver)
        {
            switch (driver)
            {
                case eDriverType.InternalBrowser:
                case eDriverType.SeleniumFireFox:
                case eDriverType.SeleniumChrome:
                case eDriverType.SeleniumIE:
                case eDriverType.SeleniumRemoteWebDriver:
                case eDriverType.SeleniumEdge:
                    return ePlatformType.Web;               
                case eDriverType.ASCF:
                    return ePlatformType.ASCF;
                case eDriverType.DOSConsole:
                    return ePlatformType.DOS;
                case eDriverType.UnixShell:
                    return ePlatformType.Unix;
                case eDriverType.WebServices:
                    return ePlatformType.WebServices;
                case eDriverType.WindowsAutomation:
                    return ePlatformType.Windows;             
                case eDriverType.MobileAppiumAndroid:
                case eDriverType.MobileAppiumIOS:
                case eDriverType.PerfectoMobileAndroid:
                case eDriverType.PerfectoMobileAndroidWeb:
                case eDriverType.PerfectoMobileIOS:
                case eDriverType.PerfectoMobileIOSWeb:
                case eDriverType.MobileAppiumAndroidBrowser:
                case eDriverType.MobileAppiumIOSBrowser:
                case eDriverType.Appium:
                    return ePlatformType.Mobile;
                case eDriverType.PowerBuilder:
                    return ePlatformType.PowerBuilder;
                case eDriverType.JavaDriver:
                    return ePlatformType.Java;
                case eDriverType.MainFrame3270:
                    return ePlatformType.MainFrame;
                //case eDriverType.AndroidADB:
                //    return ePlatformType.AndroidDevice;               
                default:
                    return ePlatformType.NA;
            }                
        }

       public List<object> GetDriverTypesByPlatfrom(string platformType)
        {
            List<object> driverTypes = new List<object>();

            if (platformType == ePlatformType.Web.ToString())
            {
                driverTypes.Add(Agent.eDriverType.InternalBrowser);
                driverTypes.Add(Agent.eDriverType.SeleniumChrome);
                driverTypes.Add(Agent.eDriverType.SeleniumFireFox);
                driverTypes.Add(Agent.eDriverType.SeleniumIE);
                driverTypes.Add(Agent.eDriverType.SeleniumRemoteWebDriver);
                driverTypes.Add(Agent.eDriverType.SeleniumEdge);      
                


            }
            else if (platformType == ePlatformType.Java.ToString())
            {
                driverTypes.Add(Agent.eDriverType.JavaDriver);

            }
            else if (platformType == ePlatformType.Mobile.ToString())
            {
                driverTypes.Add(Agent.eDriverType.MobileAppiumAndroid);
                driverTypes.Add(Agent.eDriverType.MobileAppiumIOS);
                driverTypes.Add(Agent.eDriverType.PerfectoMobileAndroid);
                driverTypes.Add(Agent.eDriverType.PerfectoMobileAndroidWeb);
                driverTypes.Add(Agent.eDriverType.PerfectoMobileIOS);
                driverTypes.Add(Agent.eDriverType.PerfectoMobileIOSWeb);
                driverTypes.Add(Agent.eDriverType.MobileAppiumAndroidBrowser);
                driverTypes.Add(Agent.eDriverType.MobileAppiumIOSBrowser);
            }
            else if (platformType == ePlatformType.Windows.ToString())
            {
                driverTypes.Add(Agent.eDriverType.WindowsAutomation);                
            }
            else if (platformType == ePlatformType.PowerBuilder.ToString())
            {
                driverTypes.Add(Agent.eDriverType.PowerBuilder);                
            }

            else if (platformType == ePlatformType.Unix.ToString())
            {
                driverTypes.Add(Agent.eDriverType.UnixShell);

            }
            else if (platformType == ePlatformType.DOS.ToString())
            {
                driverTypes.Add(Agent.eDriverType.DOSConsole);
            }

            else if (platformType == ePlatformType.WebServices.ToString())
            {
                driverTypes.Add(Agent.eDriverType.WebServices);
            }

            //else if (platformType == ePlatformType.AndroidDevice.ToString())
            //{
            //    driverTypes.Add(Agent.eDriverType.AndroidADB);
            //}
            else if (platformType == ePlatformType.ASCF.ToString())
            {
                driverTypes.Add(Agent.eDriverType.ASCF);
            }

            else if (platformType == ePlatformType.MainFrame.ToString())
            {
                driverTypes.Add(Agent.eDriverType.MainFrame3270);
            }           
            else
            {
                driverTypes.Add(Agent.eDriverType.NA);
            }

            return driverTypes;
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        
        public DriverConfigParam GetOrCreateParam(string parameter, string defaultValue = null)
        {
            DriverConfigParam configParam = DriverConfiguration.Where(x => x.Parameter == parameter).FirstOrDefault();
            if (configParam != null)
            {
                return configParam;
            }
            else
            {
                configParam = new DriverConfigParam() { Parameter = parameter, Value = defaultValue };
                DriverConfiguration.Add(configParam);
                return configParam;
            }
        }

        public string GetParamValue(string Parameter)
        {
            foreach (DriverConfigParam DCP1 in DriverConfiguration)
            {
                if (DCP1.Parameter == Parameter)
                {
                    return DCP1.Value;
                }
            }

            return null;
        }


        public void WaitForAgentToBeReady()
        {

            int Counter = 0;
            while (Status != Agent.eStatus.Running && String.IsNullOrEmpty(Driver.ErrorMessageFromDriver))
            {
                // TODO: run on another thread?? !!!!!!!!!!!!!!!!!!!!
                //GingerCore.General.DoEvents ();
                Thread.Sleep (100);
                Counter++;

                int waitingTime = 30;// 30 seconds
                if (Driver.DriverLoadWaitingTime > 0)
                    waitingTime = Driver.DriverLoadWaitingTime;
                Double waitingTimeInMilliseconds = waitingTime * 10;
                if (Counter > waitingTimeInMilliseconds)
                {                   
                    if(Driver!=null && string.IsNullOrEmpty(Driver.ErrorMessageFromDriver))
                    {
                        Driver.ErrorMessageFromDriver = "Failed to start the agent after waiting for " + waitingTime + " seconds"; 
                    }
                    return;
                }
            }         
        }
        public bool UsedForAutoMapping { get; set; } = false;


        public void Test()
        {
            if (Status == Agent.eStatus.Running) Close();

            //ProjEnvironment = App.AutomateTabEnvironment;
            //BusinessFlow = App.BusinessFlow; ;
            //SolutionFolder =  WorkSpace.Instance.Solution.Folder;
            //DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            SolutionFolder =WorkSpace.Instance.SolutionRepository.SolutionFolder;
            try
            {
                StartDriver();
                if (Driver != null)
                {
                    WaitForAgentToBeReady();
                }

                if (Status == Agent.eStatus.Running)
                {                    
                    Reporter.ToUser(eUserMsgKey.SuccessfullyConnectedToAgent);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, Name, "Invalid Agent Configurations");
                }
            }
            catch (Exception AgentStartException)
            {                
                Reporter.ToUser(eUserMsgKey.FailedToConnectAgent, Name, "Agent Test failed due to: " + AgentStartException.Message);
            }
            finally
            {
                Close();

            }
        }

        

        public object Tag;
       
        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Agent;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Name);
            }
        }

        public bool IsVirtual { get; internal set; }

        public List<DriverBase> VirtualAgentsStarted()
        {
            List<DriverBase> CurrentDrivers = new List<DriverBase>();

            foreach (var drv in DriverBase.VirtualDrivers.Where(x => x.Key == this.Guid.ToString()||x.Key==this.ParentGuid.ToString()))
            {
                CurrentDrivers.Add(drv.Value);
            }
    



            return CurrentDrivers;
        }


        public override void PostDeserialization()
        {

            if(DriverType == eDriverType.WindowsAutomation)
            {
                //Upgrading Action timeout for windows driver from default 10 secs to 30 secs
                DriverConfigParam actionTimeoutParameter = DriverConfiguration.Where(x => x.Parameter == nameof(DriverBase.ActionTimeout)).FirstOrDefault();

                if (actionTimeoutParameter!=null && actionTimeoutParameter.Value== "10" && actionTimeoutParameter.Description.Contains("10"))
                {
                    actionTimeoutParameter.Value = "30";
                    actionTimeoutParameter.Description=actionTimeoutParameter.Description.Replace("10", "30");
                }
            }          
        }

    }
}
