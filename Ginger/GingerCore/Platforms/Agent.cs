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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.ASCF;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.ConsoleDriverLib;
using GingerCore.Drivers.InternalBrowserLib;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.MainFrame;
using GingerCore.Drivers.Mobile.Perfecto;
using GingerCore.Drivers.PBDriver;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Drivers.WindowsLib;
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
using amdocs.ginger.GingerCoreNET;

namespace GingerCore
{
    //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //
    //   >>>>DO NOT put any reference to App.*  - Agent should not refer any UI element or App as we can have several agent running in paralel
    //
    //   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public class Agent : RepositoryItemBase
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

        public enum eDriverType
        {
            // web
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
            SeleniumPhantomJS,

            // ASCF - Java
            [Description("Amdocs Smart Client Framework(ASCF)")]
            ASCF,
            //QTP = 6,
            //QTP_ASAP = 7,
            //QTP_ANIS = 8,

            // UXF = 9,
            //SeleniumFireFoxAppliTools = 10,
            [Description("DOS Console")]
            DOSConsole,
            //SeleniumAppiumChrome = 12,
            //SeleniumAppiumGoogleDialer = 13,            
            [Description("Unix Shell")]
            UnixShell,
            //TODO: all enums should have description and this is what the user should see
            [Description("Web Services")]
            WebServices,
            [Description("Windows (UIAutomation)")]
            WindowsAutomation,
            //SeleniumAppiumHTCLauncher = 17,
            [Description("Mobile Appium Android")]
            MobileAppiumAndroid,
            //VBScript = 19,
            [Description("Windows (FlaUI)")]
            FlaUIWindow,
            [Description("Power Builder (FlaUI)")]
            FlaUIPB,
            //PB
            [Description("Power Builder")]
            PowerBuilder,

            //Mobile
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

            //Java
            [Description("Java")]
            JavaDriver,

            //MF
            [Description("MainFrame 3270")]
            MainFrame3270,

            //Android
            [Description("Android ADB")]
            AndroidADB,
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
        }

        public bool IsWindowExplorerSupportReady
        {
            get
            {
                if (Driver != null) return  Driver.IsWindowExplorerSupportReady();

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
        
        [IsSerializedForLocalRepository]
        public bool Active { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<DriverConfigParam> AdvanceAgentConfigurations = new ObservableList<DriverConfigParam>();

        bool mIsStarting = false;
        
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

        #region Remote Agent for Ginger Grid
        //Used for Remote Agent - if this is remote agent then the Host and Port should be specified

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
                    if (mGingerNodeInfo != null)
                    {
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
                            //Reporter.ToLog(eAppReporterLogLevel.INFO, $"Method - {"get Status"}, IsRunning - {DriverIsRunning}"); //TODO: if needed so need to be more informative icluding Agent type & name and to be writed only in Debug mode of Log
                        }
                if (DriverIsRunning) return eStatus.Running;
                              
                return eStatus.NotStarted;
            }
        }

        private eDriverType mDriverType;
        [IsSerializedForLocalRepository]                
        public eDriverType DriverType {get { return mDriverType; } set {if (mDriverType != value) {mDriverType = value; OnPropertyChanged(nameof(DriverType));}}}

        private string mNotes;
        [IsSerializedForLocalRepository]
        public string Notes { get { return mNotes; } set { if (mNotes != value) { mNotes = value; OnPropertyChanged(nameof(Notes)); } } }

        [IsSerializedForLocalRepository]        
        public ObservableList<DriverConfigParam> DriverConfiguration;

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

        private Task MSTATask = null;  // For STA Driver we keep the STA task 
        private CancellationTokenSource CTS = null;
        BackgroundWorker CancelTask;

        public void StartDriver()
        {
            mIsStarting = true;
            OnPropertyChanged(Fields.Status);
            try
            {
                try
                {
                    if (Remote)
                    {
                        throw new Exception("Remote is Obsolete, use GingerGrid");
                        //We pass the agent info
                    }
                    else
                    {
                        switch (DriverType)
                        {
                            case eDriverType.InternalBrowser:
                                Driver = new InternalBrowser(BusinessFlow);                              
                                break;

                            case eDriverType.SeleniumFireFox:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.FireFox);
                                break;
                            case eDriverType.SeleniumChrome:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
                                break;
                            case eDriverType.SeleniumIE:

                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.IE);

                                break;
                            case eDriverType.SeleniumRemoteWebDriver:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.RemoteWebDriver);
                                // set capabilities
                                if (DriverConfiguration == null) DriverConfiguration = new ObservableList<DriverConfigParam>();
                                ((SeleniumDriver)Driver).RemoteGridHub = GetParamValue(SeleniumDriver.RemoteGridHubParam);
                                ((SeleniumDriver)Driver).RemoteBrowserName = GetParamValue(SeleniumDriver.RemoteBrowserNameParam);
                                ((SeleniumDriver)Driver).RemotePlatform = GetParamValue(SeleniumDriver.RemotePlatformParam);
                                ((SeleniumDriver)Driver).RemoteVersion = GetParamValue(SeleniumDriver.RemoteVersionParam);
                                break;
                            case eDriverType.SeleniumEdge:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Edge);
                                break;
                            case eDriverType.SeleniumPhantomJS:
                                Driver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.PhantomJS);
                                break;
                            case eDriverType.ASCF:
                                Driver = new ASCFDriver(BusinessFlow, Name);
                                break;
                            case eDriverType.DOSConsole:
                                Driver = new DOSConsoleDriver(BusinessFlow);
                                break;
                            case eDriverType.UnixShell:
                                Driver = new UnixShellDriver(BusinessFlow, ProjEnvironment);
                                ((UnixShellDriver)Driver).SetScriptsFolder(System.IO.Path.Combine(SolutionFolder, @"Documents\sh\"));
                                break;

                            case eDriverType.MobileAppiumAndroid:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.Android, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumIOS:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOS, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumAndroidBrowser:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.AndroidBrowser, BusinessFlow);
                                break;
                            case eDriverType.MobileAppiumIOSBrowser:
                                Driver = new SeleniumAppiumDriver(SeleniumAppiumDriver.eSeleniumPlatformType.iOSBrowser, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileAndroid:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.NativeAndroid, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileAndroidWeb:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.WebAndroid, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileIOS:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.NativeIOS, BusinessFlow);
                                break;
                            case eDriverType.PerfectoMobileIOSWeb:
                                Driver = new PerfectoDriver(PerfectoDriver.eContextType.WebIOS, BusinessFlow);
                                break;

                            case eDriverType.WebServices:
                                WebServicesDriver WebServicesDriver = new WebServicesDriver(BusinessFlow);
                                Driver = WebServicesDriver;
                                break;
                            case eDriverType.WindowsAutomation:
                                Driver = new WindowsDriver(BusinessFlow);
                                break;

                            case eDriverType.FlaUIWindow:
                                Driver = new WindowsDriver(BusinessFlow, UIAutomationDriverBase.eUIALibraryType.FlaUI);
                                break;

                            case eDriverType.PowerBuilder:
                                Driver = new PBDriver(BusinessFlow);
                                break;

                            case eDriverType.FlaUIPB:
                                Driver = new PBDriver(BusinessFlow, UIAutomationDriverBase.eUIALibraryType.FlaUI);
                                break;

                            case eDriverType.JavaDriver:
                                Driver = new JavaDriver(BusinessFlow);
                                break;
                            case eDriverType.MainFrame3270:
                                Driver = new MainFrameDriver(BusinessFlow);
                                break;
                            case eDriverType.AndroidADB:
                                string DeviceConfigFolder = GetOrCreateParam("DeviceConfigFolder").Value;
                                if (!string.IsNullOrEmpty(DeviceConfigFolder))
                                {
                                    Driver = new AndroidADBDriver(BusinessFlow, System.IO.Path.Combine(SolutionFolder, @"Documents\Devices", DeviceConfigFolder, @"\"));
                                }
                                else
                                {
                                    //TODO: Load create sample folder/device, or start the wizard
                                    throw new Exception("Please set device config folder");
                                }
                                break;                            
                            //TODO: default mess
                        }
                    }
                }
                catch (Exception e)
                {                    
                    Reporter.ToUser(eUserMsgKeys.FailedToConnectAgent, Name, e.Message);
                }

                if (AgentType == eAgentType.Service)
                {                    
                    StartPluginService();
                    OnPropertyChanged(Fields.Status);
                }
                else
                {                   
                    Driver.BusinessFlow = this.BusinessFlow;
                    SetDriverConfiguration();

                    //if STA we need to start it on seperate thread, so UI/Window can be refreshed: Like IB, Mobile, Unix
                    if (Driver.IsSTAThread())
                    {
                        CTS = new CancellationTokenSource();

                        MSTATask = new Task(() => { Driver.StartDriver(); }, CTS.Token, TaskCreationOptions.LongRunning);
                        MSTATask.Start();
                    }
                    else
                    {
                        Driver.StartDriver();
                    }
                }
            }
            finally
            {
                if (AgentType == eAgentType.Service)
                {
                    mIsStarting = false;                    
                }
                else
                {
                    // Give the driver time to start            
                    Thread.Sleep(500);
                    mIsStarting = false;
                    Driver.IsDriverRunning = true;
                    OnPropertyChanged(Fields.Status);
                    Driver.driverMessageEventHandler += driverMessageEventHandler;
                    OnPropertyChanged(Fields.IsWindowExplorerSupportReady);
                }
            }
        }


        System.Diagnostics.Process mProcess;
        private void StartPluginService()
        {

            /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<< MyDriver
            // Find the first service which match
            mGingerNodeInfo = (from x in WorkSpace.Instance.LocalGingerGrid.NodeList where x.ServiceId == "SeleniumChromeDriver" select x).FirstOrDefault();  // Keep First!!!

            // Service not found start new one
            // Add plugin config start if not exist and more depeneds on the config 
            if (mGingerNodeInfo == null)
            {
                // Dup with GR consolidate with timeout
                mProcess = WorkSpace.Instance.PlugInsManager.StartService(PluginId);    
            }

            Stopwatch st = Stopwatch.StartNew();
            while (mGingerNodeInfo == null && st.ElapsedMilliseconds < 30000) // max 30 seconds to wait
            {

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

                mGingerNodeInfo = (from x in WorkSpace.Instance.LocalGingerGrid.NodeList where x.ServiceId == "SeleniumChromeDriver" select x).FirstOrDefault();  // Keep First!!!
                if (mGingerNodeInfo != null) break;
                Thread.Sleep(100);
            }

            if (mGingerNodeInfo == null)
            {
                throw new Exception("Plugin not started " + PluginId);
            }


            mGingerNodeInfo.Status = GingerNodeInfo.eStatus.Reserved;
            // TODO: add by which agent to GNI

            // Keep GNP on agent
            GingerNodeProxy GNP = new GingerNodeProxy(mGingerNodeInfo);
            GNP.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
            GNP.StartDriver();
        }

        private void driverMessageEventHandler(object sender, DriverMessageEventArgs e)
        {
            if (e.DriverMessageType == DriverBase.eDriverMessageType.DriverStatusChanged)
            {
                OnPropertyChanged(Fields.Status);
            }
        }

        private void SetDriverConfiguration()
        {
            Boolean bValue;
            if (DriverConfiguration == null) return;
            if (ProjEnvironment == null)
                ProjEnvironment = new Environments.ProjEnvironment();//to avoid value expertion exception
            if (BusinessFlow == null)
                BusinessFlow = new GingerCore.BusinessFlow();//to avoid value expertion exception
            foreach (DriverConfigParam DCP in DriverConfiguration)
            {
                //process Value expression in case used
                ValueExpression VE = new ValueExpression(ProjEnvironment, BusinessFlow, DSList);
                VE.Value = DCP.Value;
                string value = VE.ValueCalculated;

                //TODO: check if Convert.To is better option
                //TODO: hanlde other feilds type
                PropertyInfo tp = Driver.GetType().GetProperty(DCP.Parameter);
                if (tp != null)
                {
                    string tpName = tp.PropertyType.Name;
                    switch (tpName)
                    {

                        case "String":
                            Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, value);
                            break;
                        case "Boolean":
                            try
                            {
                                bValue = Convert.ToBoolean(value);
                            }
                            catch (Exception)
                            {
                                bValue = true;
                            }
                            Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, bValue);
                            break;
                        case "Int32":
                            int i = int.Parse(value);
                            Driver.GetType().GetProperty(DCP.Parameter).SetValue(Driver, i);
                            break;
                        case "eType":
                            //TODO: Handle enums later...
                            throw new Exception("Driver Config - Enum not supported yet");
                        default:
                            Reporter.ToUser(eUserMsgKeys.SetDriverConfigTypeNotHandled, DCP.GetType().ToString());
                            break;
                    }
                }
                else
                {
                    // TODO: show message to user to remove param - old
                }
            }
            Driver.AdvanceDriverConfigurations = this.AdvanceAgentConfigurations;
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

            //TODO: fix me to be OO style remove the ugly switch
            switch (DriverType)
            {
                case Agent.eDriverType.InternalBrowser:
                    SetDriverDefualtParams(typeof(InternalBrowser));
                    break;
                case Agent.eDriverType.SeleniumFireFox:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.SeleniumChrome:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.SeleniumIE:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.SeleniumRemoteWebDriver:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.SeleniumEdge:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.SeleniumPhantomJS:
                    SetDriverDefualtParams(typeof(SeleniumDriver));
                    break;
                case Agent.eDriverType.ASCF:
                    SetDriverDefualtParams(typeof(ASCFDriver));
                    break;
                case Agent.eDriverType.DOSConsole:
                    SetDriverDefualtParams(typeof(DOSConsoleDriver));
                    break;
                case Agent.eDriverType.UnixShell:
                    SetDriverDefualtParams(typeof(UnixShellDriver));
                    break;
                case Agent.eDriverType.MobileAppiumAndroid:
                    SetDriverDefualtParams(typeof(SeleniumAppiumDriver));
                    break;
                case Agent.eDriverType.MobileAppiumIOS:
                    SetDriverDefualtParams(typeof(SeleniumAppiumDriver));
                    break;
                case Agent.eDriverType.MobileAppiumAndroidBrowser:
                case Agent.eDriverType.MobileAppiumIOSBrowser:
                    SetDriverDefualtParams(typeof(SeleniumAppiumDriver));
                    break;
                case Agent.eDriverType.PowerBuilder:
                    SetDriverDefualtParams(typeof(PBDriver));
                    break;
                case Agent.eDriverType.WindowsAutomation:
                    SetDriverDefualtParams(typeof(WindowsDriver));
                    break;
                case Agent.eDriverType.WebServices:
                    SetDriverDefualtParams(typeof(WebServicesDriver));
                    break;
                case Agent.eDriverType.JavaDriver:
                    SetDriverDefualtParams(typeof(JavaDriver));
                    break;
                case Agent.eDriverType.MainFrame3270:
                    SetDriverDefualtParams(typeof(MainFrameDriver));
                    break;
                case Agent.eDriverType.AndroidADB:
                    SetDriverDefualtParams(typeof(AndroidADBDriver));
                    break;
                case Agent.eDriverType.PerfectoMobileAndroid:
                case Agent.eDriverType.PerfectoMobileAndroidWeb:
                case Agent.eDriverType.PerfectoMobileIOS:
                case Agent.eDriverType.PerfectoMobileIOSWeb:
                    SetDriverDefualtParams(typeof(PerfectoDriver));
                    break;
                default:
                    //UnKonwen Type!!!
                    break;
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

                UserConfiguredDefaultAttribute defaultVal = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredDefaultAttribute), false) as UserConfiguredDefaultAttribute;
                UserConfiguredDescriptionAttribute desc = Attribute.GetCustomAttribute(mi, typeof(UserConfiguredDescriptionAttribute), false) as UserConfiguredDescriptionAttribute;

                DriverConfigParam DCP = new DriverConfigParam();
                DCP.Parameter = mi.Name;
                if (defaultVal != null) DCP.Value = defaultVal.DefaultValue;
                if (desc != null) DCP.Description = desc.Description;
                DriverConfiguration.Add(DCP);
            }
        }
       
        // keep GingerNodeProxy here

        // We keep the GingerNodeInfo for Plugin driver
        private GingerNodeInfo mGingerNodeInfo;
        public GingerNodeInfo GingerNodeInfo
        {
            get
            {
                return mGingerNodeInfo;
            }
            set { mGingerNodeInfo = value;
            }
        }


        public void RunAction(Act act)
        {          
            try
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
            catch (Exception ex)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
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
                    if (mGingerNodeInfo != null)
                    {
                        // this is plugin driver
                        GingerNodeProxy GNP = new GingerNodeProxy(mGingerNodeInfo);
                        GNP.GingerGrid = WorkSpace.Instance.LocalGingerGrid;
                        GNP.CloseDriver();
                        if (mProcess != null)
                        {
                            // mProcess.Kill();
                            //mProcess.Close();
                            //GingerCore.General.DoEvents();
                            mProcess.CloseMainWindow();
                        }
                        mGingerNodeInfo = null;
                        // GNP.Shutdown();
                        return;
                    }
                }
                if (Driver == null) return;

                Driver.IsDriverRunning = false;                
                if (Driver.Dispatcher != null)
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
                    // Using Cancelleation token soucrce to cancel 
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
            
                // Using Cancelleation token soucrce to cancel  getting exceptions when trying to close agent and task is in running condition

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

        public ePlatformType Platform 
        { 
            get 
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

        public static ePlatformType GetDriverPlatformType(eDriverType driver)
        {
            switch (driver)
            {
                case eDriverType.InternalBrowser:
                    return ePlatformType.Web;
                case eDriverType.SeleniumFireFox:
                    return ePlatformType.Web;
                case eDriverType.SeleniumChrome:
                    return ePlatformType.Web;
                case eDriverType.SeleniumIE:
                    return ePlatformType.Web;
                case eDriverType.SeleniumRemoteWebDriver:
                    return ePlatformType.Web;
                case eDriverType.SeleniumEdge:
                    return ePlatformType.Web;
                case eDriverType.SeleniumPhantomJS:
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
                case eDriverType.FlaUIWindow:
                    return ePlatformType.Windows;
                case eDriverType.MobileAppiumAndroid:
                case eDriverType.MobileAppiumIOS:
                //Add Perfecto Mobile
                case eDriverType.PerfectoMobileAndroid:
                case eDriverType.PerfectoMobileAndroidWeb:
                case eDriverType.PerfectoMobileIOS:
                case eDriverType.PerfectoMobileIOSWeb:
                    return ePlatformType.Mobile;
                case eDriverType.MobileAppiumAndroidBrowser:
                case eDriverType.MobileAppiumIOSBrowser:
                    return ePlatformType.Mobile;
                case eDriverType.PowerBuilder:
                    return ePlatformType.PowerBuilder;
                case eDriverType.FlaUIPB:
                    return ePlatformType.PowerBuilder;
                case eDriverType.JavaDriver:
                    return ePlatformType.Java;
                case eDriverType.MainFrame3270:
                    return ePlatformType.MainFrame;
                case eDriverType.AndroidADB:
                    return ePlatformType.AndroidDevice;               
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
                driverTypes.Add(Agent.eDriverType.SeleniumPhantomJS);
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
                driverTypes.Add(Agent.eDriverType.FlaUIWindow);
            }
            else if (platformType == ePlatformType.PowerBuilder.ToString())
            {
                driverTypes.Add(Agent.eDriverType.PowerBuilder);
                driverTypes.Add(Agent.eDriverType.FlaUIPB);
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

            else if (platformType == ePlatformType.AndroidDevice.ToString())
            {
                driverTypes.Add(Agent.eDriverType.AndroidADB);
            }
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
        
        public DriverConfigParam GetOrCreateParam(string Parameter, string DefaultValue = null)
        {
            foreach(DriverConfigParam DCP1 in DriverConfiguration)
            {
                if (DCP1.Parameter == Parameter)
                {
                    return DCP1;
                }
            }

            DriverConfigParam DCP = new DriverConfigParam() { Parameter = Parameter, Value = DefaultValue };            
            DriverConfiguration.Add(DCP);                            
            return DCP;
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
                GingerCore.General.DoEvents ();
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
        public bool UsedForAutoMapping=false;


        public void Test()
        {
            if (Status == Agent.eStatus.Running) Close();

            //ProjEnvironment = App.AutomateTabEnvironment;
            //BusinessFlow = App.BusinessFlow; ;
            //SolutionFolder = App.UserProfile.Solution.Folder;
            //DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            SolutionFolder =WorkSpace.Instance.SolutionRepository.SolutionFolder;
            try
            {
                StartDriver();

                WaitForAgentToBeReady();

                if (Status == Agent.eStatus.Running)
                    Reporter.ToUser(eUserMsgKeys.SuccessfullyConnectedToAgent);

                else
                    Reporter.ToUser(eUserMsgKeys.FailedToConnectAgent, Name, "Invalid Agent Configuration");
            }

            catch (Exception AgentStartException)
            {
                Reporter.ToUser(eUserMsgKeys.FailedToConnectAgent, Name, "Agent Launch failed due to " + AgentStartException.Message);
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

    }
}
