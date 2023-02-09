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

//using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.Run;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Environments;
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
        public IAgentOperations AgentOperations;
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

        //public DriverInfo DriverInfo { get; set; }
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
            [Description("Appium")]
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
                    OnPropertyChanged(nameof(Agent.Name));
                }
            }
        }
        public eStatus Status 
        {
            get
            {
                return AgentOperations.Status;
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
                        {
                            return true;
                        }
                    }
                    break;
                default:
                    //not implemented
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
                case eDriverType.Appium:
                case eDriverType.JavaDriver:
                    isSupport = true;
                    break;
                default:
                    //not implemented
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

        public Task MSTATask = null;  // For STA Driver we keep the STA task 
        public CancellationTokenSource CTS = null;
        BackgroundWorker CancelTask;

        public override string GetNameForFileName()
        {
            return Name;
        }

       
        public void CancelTMSTATask(object sender, DoWorkEventArgs e)
        {
            if (MSTATask == null)
            {
                return;
            }
            // Using cancellation token source to cancel  getting exceptions when trying to close agent and task is in running condition

            while (MSTATask != null && !(MSTATask.Status == TaskStatus.RanToCompletion || MSTATask.Status == TaskStatus.Faulted || MSTATask.Status == TaskStatus.Canceled))
            {

                CTS.Cancel();
                Thread.Sleep(100);
            }
            CTS.Dispose();
            MSTATask = null;
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
                driverTypes.Add(Agent.eDriverType.Appium);
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

        public override string GetItemType()
        {
            return nameof(Agent);
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


        public bool UsedForAutoMapping { get; set; } = false;


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

        public bool IsVirtual { get; set; }

        public Type DriverClass = null;

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
            catch (Exception e)
            {


            }
            return false;
        }

        public override void PostDeserialization()
        {

            if (DriverType == eDriverType.WindowsAutomation)
            {
                //Upgrading Action timeout for windows driver from default 10 secs to 30 secs
                DriverConfigParam actionTimeoutParameter = DriverConfiguration.Where(x => x.Parameter == "ActionTimeout").FirstOrDefault();

                if (actionTimeoutParameter != null && actionTimeoutParameter.Value == "10" && actionTimeoutParameter.Description.Contains("10"))
                {
                    actionTimeoutParameter.Value = "30";
                    actionTimeoutParameter.Description = actionTimeoutParameter.Description.Replace("10", "30");
                }
            }
        }

        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.SetValueException)
            {
                if (value == "MobileAppiumAndroid" || value == "PerfectoMobileAndroid" || value == "MobileAppiumIOS" || value == "PerfectoMobileIOS"
                       || value == "MobileAppiumAndroidBrowser" || value == "PerfectoMobileAndroidWeb" || value == "MobileAppiumIOSBrowser" || value == "PerfectoMobileIOSWeb")
                {
                    this.DriverType = Agent.eDriverType.Appium;
                    this.DriverConfiguration = new ObservableList<DriverConfigParam>();
                    //this.GetOrCreateParam(nameof(AppiumServer), @"http://127.0.0.1:4723/wd/hub");
                    this.GetOrCreateParam("LoadDeviceWindow", "true");
                    this.GetOrCreateParam("DeviceAutoScreenshotRefreshMode", eAutoScreenshotRefreshMode.Live.ToString());
                    this.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified;

                    if (value == "MobileAppiumAndroid" || value == "PerfectoMobileAndroid")
                    {
                        this.GetOrCreateParam("DevicePlatformType", eDevicePlatformType.Android.ToString());
                        this.GetOrCreateParam("AppType", eAppType.NativeHybride.ToString());
                    }
                    else if (value == "MobileAppiumIOS" || value == "PerfectoMobileIOS")
                    {
                        this.GetOrCreateParam("DevicePlatformType", eDevicePlatformType.iOS.ToString());
                        this.GetOrCreateParam("AppType", eAppType.NativeHybride.ToString());
                    }
                    else if (value == "MobileAppiumAndroidBrowser" || value == "PerfectoMobileAndroidWeb")
                    {
                        this.GetOrCreateParam("DevicePlatformType", eDevicePlatformType.Android.ToString());
                        this.GetOrCreateParam("AppType", eAppType.Web.ToString());
                    }
                    else if (value == "MobileAppiumIOSBrowser" || value == "PerfectoMobileIOSWeb")
                    {
                        this.GetOrCreateParam("DevicePlatformType", eDevicePlatformType.iOS.ToString());
                        this.GetOrCreateParam("AppType", eAppType.Web.ToString());
                    }
                    return true;
                }
            }

            return false;
        }

    }
}
