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
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.OS;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.POM;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.GeneralLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.Selenium.SeleniumBMP;
using GingerCoreNET.Drivers.CommonLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using InputSimulatorStandard;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Common;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using Protractor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static GingerCoreNET.GeneralLib.General;
using DevToolsDomains = OpenQA.Selenium.DevTools.V139.DevToolsSessionDomains;
using DevToolsVersion = OpenQA.Selenium.DevTools.V139;



namespace GingerCore.Drivers
{
    public class SeleniumDriver : GingerWebDriver, IVirtualDriver, IWindowExplorer, IVisualTestingDriver, IXPath, IPOM, IRecord, INotifyPropertyChanged
    {
        protected IDevToolsSession Session;
        DevToolsSession devToolsSession;
        DevToolsDomains devToolsDomains;
        IDevTools devTools;
        List<Tuple<string, object>> networkResponseLogList;
        List<Tuple<string, object>> networkRequestLogList;
        INetwork interceptor;
        public bool isNetworkLogMonitoringStarted = false;
        ActBrowserElement mAct;
        BrowserHelper _BrowserHelper;
        private int mDriverProcessId = 0;
        private readonly ShadowDOM shadowDOM = new();
        private const string CHROME_DRIVER_NAME = "chromedriver";
        private const string EDGE_DRIVER_NAME = "msedgedriver";
        private const string FIREFOX_DRIVER_NAME = "geckodriver";
        private const string TRANSLATOR_FOR_CASE_INSENSITIVE_MATCH = "translate(text(), 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz')";
        private const string BRAVE_32BIT_BINARY_PATH = "C:\\Program Files (x86)\\BraveSoftware\\Brave-Browser\\Application\\brave.exe";
        private const string BRAVE_64BIT_BINARY_PATH = "C:\\Program Files\\BraveSoftware\\Brave-Browser\\Application\\brave.exe";
        private const string EDGE_32BIT_BINARY_PATH = "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe";
        private const string EDGE_64BIT_BINARY_PATH = "C:\\Program Files\\Microsoft\\Edge\\Application\\msedge.exe";
        String[] SeleniumUserArgs = null;
        DriverService driverService = null;
        public POMUtils POMUtils = new POMUtils();
        private readonly List<string> HighlightStyleList = ["arguments[0].style.outline='3px dashed rgb(239, 183, 247)'", "arguments[0].style.backgroundColor='rgb(239, 183, 247)'", "arguments[0].style.border='3px dashed rgb(239, 183, 247)'"];
        static List<ActWebSmartSync.eSyncOperation> operationsWithoutLocator =
          [
              ActWebSmartSync.eSyncOperation.AlertIsPresent,
                ActWebSmartSync.eSyncOperation.PageHasBeenLoaded,
                ActWebSmartSync.eSyncOperation.UrlMatches
          ];

        public enum eBrowserType
        {
            IE,
            FireFox,
            Chrome,
            Brave,
            Edge,
            RemoteWebDriver,
        }
        public enum ePageLoadStrategy
        {
            Normal,
            Eager,
            None,
        }
        public enum eUnhandledPromptBehavior
        {
            [EnumValueDescription("Default")]
            Default,
            [EnumValueDescription("Ignore")]
            Ignore,
            [EnumValueDescription("Accept")]
            Accept,
            [EnumValueDescription("Dismiss")]
            Dismiss,
            [EnumValueDescription("Accept And Notify")]
            AcceptAndNotify,
            [EnumValueDescription("Dismiss And Notify")]
            DismissAndNotify
        }
        eUnhandledPromptBehavior mUnhandledPromptBehavior;

        public eUnhandledPromptBehavior UnhandledPromptBehavior1
        {
            get { return mUnhandledPromptBehavior; }
            set
            {
                if (mUnhandledPromptBehavior != value)
                {
                    mUnhandledPromptBehavior = value;

                }
            }
        }
        public enum eBrowserLogLevel
        {

            All = 0,
            Debug = 1,
            Info = 2,
            Warning = 3,
            Severe = 4
        }


        ConcurrentQueue<ElementInfo> processingQueue = new ConcurrentQueue<ElementInfo>();


        public event PropertyChangedEventHandler PropertyChanged;

        private readonly object lockObj = new object();

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged(nameof(IsProcessing));
                }
            }
        }

        public override string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA, IEnumerable<DriverConfigParam> driverConfigParams = null)
        {
            if (driverConfigParams == null)
            {
                return null;
            }

            DriverConfigParam browserTypeParam = driverConfigParams.FirstOrDefault(param => string.Equals(param.Parameter, nameof(BrowserType)));

            if (browserTypeParam == null || !Enum.TryParse(browserTypeParam.Value, out WebBrowserType browserType))
            {
                return null;
            }

            if (browserType == WebBrowserType.RemoteWebDriver)
            {
                return "SeleniumRemoteWebDriverEditPage";
            }
            else if (browserType is WebBrowserType.Chrome or WebBrowserType.Brave or WebBrowserType.Edge or WebBrowserType.InternetExplorer or WebBrowserType.FireFox)
            {
                return "WebAgentConfigEditPage";
            }
            else
            {
                return null;
            }
        }

        [UserConfigured]
        [UserConfiguredDescription("Proxy Auto Config Url")]
        public string ProxyAutoConfigUrl { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("EnableNativeEvents(true) so as to perform native events smoothly on IE ")]
        public bool EnableNativeEvents { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Auto Detect Proxy Setting?")]
        public bool AutoDetect { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Path to extension to be enabled")]
        public string ExtensionPath { get; set; }
        // Note: ExtensionPath is a semi-colon delimited string containing one or more extension paths

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Only for Internet Explorer |  Set \"false\" if dont want to clear the Internet Explorer cache before launching the browser")]
        public bool EnsureCleanSession { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Ignore Internet Explorer protected mode")]
        public bool IgnoreIEProtectedMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Only for Internet Explorer & Firefox | Set \"true\" for using 64Bit Browser")]
        public bool Use64Bitbrowser { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Use specific Version of browser. Only Testing browser versions are supported.")]
        public string BrowserVersion { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use Browser In Private/Incognito Mode (Please use 64bit Browse with Internet Explorer ")]
        public bool BrowserPrivateMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set \"true\" to Launch the Browser minimized")]
        public bool BrowserMinimized { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("3")]
        [UserConfiguredDescription("Set the minimum log level to read the console logs from the browser console.")]
        public string BrowserLogLevel { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Only for Edge: Open Edge browser in IE Mode")]
        public bool OpenIEModeInEdge { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Provide the path to the browser executable.")]
        public string BrowserExecutablePath { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]//"driver is failing to launch when the mode is true"
        [UserConfiguredDescription("Hide the Driver Console (Command Prompt) Window")]
        public bool HideConsoleWindow { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Only For Chrome : Use a valid device name from the DevTools Emulation panel.")]
        public string EmulationDeviceName { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Only For Chrome & Firefox : A browser's user agent string (UA) helps identify which browser is being used, what version, and on which operating system")]
        public string BrowserUserAgent { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("The height in pixels of the browser's viewable area")]
        public string BrowserHeight { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("The width in pixels of the browser's viewable area")]
        public string BrowserWidth { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Only for Chrome, Firefox, Edge and Brave | Full path for the User Profile folder")]
        public string UserProfileFolderPath { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Only for Chrome | Define Download Folder path")]
        public string DownloadFolderPath { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Amount of time the driver should wait when searching for an element if it is not immediately present")]
        public int ImplicitWait { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("60")]
        [UserConfiguredDescription("HttpServer Timeout for Web Action Completion. Default/Recommended is minimum 60 secs")]
        public int HttpServerTimeOut { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("60")]
        [UserConfiguredDescription("PageLoad Timeout for Web Action Completion")]
        public int PageLoadTimeOut { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Normal")]
        [UserConfiguredDescription("Defines the current session’s page loading strategy.you can change from the default parameter of normal to eager or none")]
        public string PageLoadStrategy { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Start BMP - Browser Mob Proxy (true/false)")]
        public bool StartBMP { get; set; }
        [UserConfigured]
        [UserConfiguredDefault(@"C:\...\browsermob\bin\browsermob-proxy.bat")]
        [UserConfiguredDescription("Start BMP .BAT File - full path to BMP BAT file")]
        public string StartBMPBATFile { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("9090")]
        [UserConfiguredDescription("Start BMP Port")]
        public int StartBMPPort { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Take Only Active Frame Or Window Screen Shot In Case Of Failure")]
        public bool TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Start network monitoring log.")]
        public bool StartNetworkMonitoring { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Selenium line arguments || Set Selenium arguments separated with ; sign")]
        public string SeleniumUserArguments { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Change to Iframe automatically in case of POM Element execution ")]
        public bool HandelIFramShiftAutomaticallyForPomElement { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Sample Value is 'localhost:9222'.This allows to Connect to existing browser session on specific debug port instead of Launching a new browser")]
        public string DebugAddress { get; set; }

        [UserConfigured]
        // Changed the default from ignore to Actual Default suggested by Selenium i.e. dismissAndNotify
        [UserConfiguredDefault("DismissAndNotify")]
        [UserConfiguredDescription("Specifies the state of current session’s user prompt handler, You can change it from dismiss, accept, dismissAndNotify, acceptAndNotify, ignore")]
        public string UnhandledPromptBehavior { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Use custom browser driver. Provide complete driver path with file extension")]
        public string DriverFilePath { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(WebBrowserType))]
        [UserConfiguredDefault("Chrome")]
        [UserConfiguredDescription("Browser Type")]
        public override WebBrowserType BrowserType
        {
            get
            {
                return mBrowserType switch
                {
                    eBrowserType.Chrome => WebBrowserType.Chrome,
                    eBrowserType.FireFox => WebBrowserType.FireFox,
                    eBrowserType.Edge => WebBrowserType.Edge,
                    eBrowserType.Brave => WebBrowserType.Brave,
                    eBrowserType.IE => WebBrowserType.InternetExplorer,
                    eBrowserType.RemoteWebDriver => WebBrowserType.RemoteWebDriver,
                    _ => throw new Exception($"Unknown browser type '{mBrowserType}'"),
                };
            }
            set
            {
                mBrowserType = value switch
                {
                    WebBrowserType.Chrome => eBrowserType.Chrome,
                    WebBrowserType.FireFox => eBrowserType.FireFox,
                    WebBrowserType.Edge => eBrowserType.Edge,
                    WebBrowserType.Brave => eBrowserType.Brave,
                    WebBrowserType.InternetExplorer => eBrowserType.IE,
                    WebBrowserType.RemoteWebDriver => eBrowserType.RemoteWebDriver,
                    _ => throw new Exception($"Unknown browser type '{value}'"),
                };
            }
        }


        [UserConfigured]
        [UserConfiguredDescription("Remote Web Driver Url")]
        public string RemoteWebDriverUrl { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Allow ZAP to Perform Security Testing")]
        public bool UseSecurityTesting { get; set; }

        protected IWebDriver Driver;
        protected AppiumDriver MobDriver;
        protected eBrowserType mBrowserType;
        protected NgWebDriver ngDriver;
        private String DefaultWindowHandler = null;

        private Proxy mProxy = new();

        // FOr BMP - Browser Mob Proxy
        Server BMPServer;
        Client BMPClient;


        // Only for RemoteWebDriver, have config screen dedicated, being saved in agent DriverConfiguration
        public static string RemoteGridHubParam = "RemoteGridHub";
        public static string RemoteBrowserNameParam = "RemoteBrowserName";
        public static string RemotePlatformParam = "RemotePlatform";
        public static string RemoteVersionParam = "RemoteVersion";

        public string RemoteGridHub { get; set; }
        public string RemoteBrowserName { get; set; }
        public string RemotePlatform { get; set; }
        public string RemoteVersion { get; set; }
        private bool RestartRetry = true;
        private bool IsRecording = false;
        public bool IsHealenium = false;
        public string HealeniumUrl { get; set; }

        IWebElement LastHighLightedElement;
        XPathHelper mXPathHelper;

        private List<ElementInfo> allReadElem = [];

        private string CurrentFrame;

        public override ePomElementCategory? PomCategory
        {
            get
            {
                if (base.PomCategory == null)
                {
                    return ePomElementCategory.Web;
                }
                else
                {
                    return base.PomCategory;
                }
            }

            set => base.PomCategory = value;
        }
        public bool isAppiumSession { get; set; }

        public SeleniumDriver()
        {
            POMUtils = new POMUtils();
            POMUtils.ProcessingStatusChanged += POMUtils_ProcessingStatusChanged;
        }

        ~SeleniumDriver()
        {
            if (Driver != null)
            {
                CloseDriver();
            }
        }

        private void POMUtils_ProcessingStatusChanged(object sender, bool isProcessing)
        {
            if (IsProcessing != isProcessing)
            {
                IsProcessing = isProcessing;
                OnPropertyChanged(nameof(IsProcessing));
            }
        }

        public SeleniumDriver(eBrowserType BrowserType)
        {
            mBrowserType = BrowserType;
        }

        public SeleniumDriver(object driver)
        {
            this.Driver = (IWebDriver)driver;
        }
        public override void InitDriver(Agent agent)
        {
            if (BrowserType == WebBrowserType.RemoteWebDriver)
            {
                if (agent.DriverConfiguration == null)
                {
                    agent.DriverConfiguration = [];
                }
                RemoteGridHub = agent.GetParamValue(SeleniumDriver.RemoteGridHubParam);
                RemoteBrowserName = agent.GetParamValue(SeleniumDriver.RemoteBrowserNameParam);
                RemotePlatform = agent.GetParamValue(SeleniumDriver.RemotePlatformParam);
                RemoteVersion = agent.GetParamValue(SeleniumDriver.RemoteVersionParam);
                if (WorkSpace.Instance.BetaFeatures.ShowHealenium)
                {
                    IsHealenium = agent.Healenium;
                    HealeniumUrl = agent.HealeniumURL;
                }
            }
        }

        public IWebDriver GetWebDriver()
        {
            return Driver;
        }

        public eBrowserType GetBrowserType()
        {
            return mBrowserType;
        }

        /// <summary>
        ///  This attribute is used to keep track of Shadow Root, when user manually creates actions to switch to a shadow root.
        /// </summary>
        /// 

        private ISearchContext _searchContext = null;
        private ISearchContext CurrentContext
        {
            get
            {
                return _searchContext ?? Driver;
            }
            set
            {
                _searchContext = value;
            }
        }

        public override void StartDriver()
        {
            //Add localhost to no proxy so that driver service can be started with proxy
            //System.Environment.SetEnvironmentVariable("NO_PROXY", @"http://localhost");



            if (StartBMP)
            {
                BMPServer = new Server(StartBMPBATFile, StartBMPPort);
                BMPServer.Start();
                BMPClient = BMPServer.CreateProxy();
            }

            if (!string.IsNullOrEmpty(ProxyAutoConfigUrl))
            {
                mProxy = new Proxy
                {
                    ProxyAutoConfigUrl = ProxyAutoConfigUrl
                };
            }
            else if (!string.IsNullOrEmpty(Proxy))
            {
                mProxy = new Proxy
                {
                    Kind = ProxyKind.Manual,
                    HttpProxy = Proxy,
                    FtpProxy = Proxy,
                    SslProxy = Proxy,
                    SocksProxy = Proxy,
                    SocksVersion = 5
                };

                if (!string.IsNullOrEmpty(ByPassProxy))
                {
                    mProxy.AddBypassAddresses(AddByPassAddress());
                }
            }
            else if (string.IsNullOrEmpty(Proxy) && AutoDetect != true && string.IsNullOrEmpty(ProxyAutoConfigUrl))
            {
                mProxy = null;
            }
            else
            {
                if (StartBMP)
                {
                    mProxy.HttpProxy = BMPClient.SeleniumProxy;
                }
                else
                {
                    mProxy.IsAutoDetect = AutoDetect;
                }
            }


            if (ImplicitWait == 0)
            {
                ImplicitWait = 30;
            }


            if (!string.IsNullOrEmpty(SeleniumUserArguments))
            {
                SeleniumUserArgs = SeleniumUserArguments.Split(';');
            }

            if (this.UseSecurityTesting)
            {

                ZAPConfiguration zAPConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ZAPConfiguration>().Count == 0 ? new ZAPConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ZAPConfiguration>();

                if (string.IsNullOrEmpty(zAPConfiguration.ZAPUrl))
                {
                    Reporter.ToLog(eLogLevel.WARN, "UseZAP is enabled but ZAP Url is empty. Traffic will not be routed through ZAP.");
                }
                else
                {

                    // Normalize to host:port for Selenium Proxy
                    string zapHostPort = CoerceZapHostPort(zAPConfiguration.ZAPUrl);
                    Proxy = zapHostPort; // keep legacy string in sync if used elsewhere
                    if (mProxy == null)
                    {

                        mProxy = new Proxy();
                    }
                    mProxy.Kind = ProxyKind.Manual;
                    mProxy.HttpProxy = Proxy;
                    mProxy.FtpProxy = Proxy;
                    mProxy.SslProxy = Proxy;
                    mProxy.SocksProxy = Proxy;

                    if (!string.IsNullOrEmpty(ByPassProxy))
                    {
                        mProxy.AddBypassAddresses(AddByPassAddress());
                    }
                }
            }

            //TODO: launch the driver/agent per combo selection
            try
            {
                switch (mBrowserType)
                {
                    //TODO: refactor closing the extra tabs
                    #region Internet Explorer
                    case eBrowserType.IE:
                        InternetExplorerOptions ieoptions = new InternetExplorerOptions();
                        SetCurrentPageLoadStrategy(ieoptions);
                        SetBrowserLogLevel(ieoptions);

                        if (EnsureCleanSession == true)
                        {
                            ieoptions.EnsureCleanSession = true;
                        }
                        ieoptions.IgnoreZoomLevel = true;
                        ieoptions.Proxy = mProxy == null ? null : mProxy;
                        ieoptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                        if (IgnoreIEProtectedMode == true)
                        {
                            ieoptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                            ieoptions.ElementScrollBehavior = InternetExplorerElementScrollBehavior.Bottom;
                        }
                        if (BrowserPrivateMode == true)
                        {
                            ieoptions.ForceCreateProcessApi = true;
                            ieoptions.BrowserCommandLineArguments = "-private";
                        }
                        if (EnableNativeEvents == true)
                        {
                            ieoptions.EnableNativeEvents = true;
                        }
                        if (!(String.IsNullOrEmpty(SeleniumUserArguments) && String.IsNullOrWhiteSpace(SeleniumUserArguments)))
                        {
                            ieoptions.BrowserCommandLineArguments += "," + SeleniumUserArguments;
                        }

                        if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey)))
                        {
                            ieoptions.BrowserCommandLineArguments += "," + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
                        }

                        if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl)))
                        {
                            ieoptions.BrowserCommandLineArguments += "," + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl;
                        }

                        driverService = InternetExplorerDriverService.CreateDefaultService(GetDriversPathPerOS());
                        AddCustomDriverPath(driverService);
                        driverService.HideCommandPromptWindow = HideConsoleWindow;
                        Driver = new InternetExplorerDriver((InternetExplorerDriverService)driverService, ieoptions, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                        break;
                    #endregion

                    #region Mozilla Firefox
                    case eBrowserType.FireFox:

                        FirefoxOptions FirefoxOption = new FirefoxOptions
                        {
                            AcceptInsecureCertificates = true
                        };
                        SetCurrentPageLoadStrategy(FirefoxOption);
                        SetBrowserLogLevel(FirefoxOption);
                        SetUnhandledPromptBehavior(FirefoxOption);
                        SetBrowserVersion(FirefoxOption);

                        if (HeadlessBrowserMode == true || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            FirefoxOption.AddArgument("--headless");                           
                        }

                        if (IsUserProfileFolderPathValid())
                        {
                            FirefoxProfile ffProfile2 = new FirefoxProfile();
                            ffProfile2 = new FirefoxProfile(UserProfileFolderPath);

                            FirefoxOption.Profile = ffProfile2;
                        }
                        else
                        {
                            SetProxy(FirefoxOption);
                        }

                        if (!string.IsNullOrEmpty(BrowserUserAgent))
                        {
                            var profile = new FirefoxProfile();
                            profile.SetPreference("general.useragent.override", BrowserUserAgent.Trim());
                            FirefoxOption.Profile = profile;
                        }
                        if (SeleniumUserArgs != null)
                        {
                            foreach (string arg in SeleniumUserArgs)
                            {
                                FirefoxOption.AddArgument(arg);
                            }
                        }
                        if (BrowserPrivateMode)
                        {
                            // This is correct way of setting private mode in Firefox, it doesn't preserve history of ongoing session
                            FirefoxOption.SetPreference("browser.privatebrowsing.autostart", true);
                        }

                        driverService = FirefoxDriverService.CreateDefaultService();
                        AddCustomDriverPath(driverService);
                        driverService.HideCommandPromptWindow = HideConsoleWindow;
                        Driver = new FirefoxDriver((FirefoxDriverService)driverService, FirefoxOption, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                        this.mDriverProcessId = driverService.ProcessId;
                        break;
                    #endregion

                    #region Chrome
                    case eBrowserType.Chrome:
                        ChromeOptions options = new ChromeOptions();
                        configChromeDriverAndStart(options);
                        break;

                    #endregion

                    #region Brave
                    //checking the windows 32 and 64 bit version exists or not. if not then user can provide the path mannually.
                    case eBrowserType.Brave:
                        ChromeOptions brave_options = new ChromeOptions();
                        if (BrowserExecutablePath != null && BrowserExecutablePath.Trim().Length > 0 && File.Exists(BrowserExecutablePath))
                        {
                            brave_options.BinaryLocation = BrowserExecutablePath;
                        }
                        else if (File.Exists(BRAVE_64BIT_BINARY_PATH))
                        {
                            brave_options.BinaryLocation = BRAVE_64BIT_BINARY_PATH;
                        }
                        else if (File.Exists(BRAVE_32BIT_BINARY_PATH))
                        {
                            brave_options.BinaryLocation = BRAVE_32BIT_BINARY_PATH;
                        }
                        else
                        {
                            throw new Exception("The Brave browser path is not available in default path. Please install it or provide the valid executable path in 'Browser Executable Path' parameter in agent configuration.");
                        }

                        configChromeDriverAndStart(brave_options);

                        break;

                    #endregion

                    #region EDGE
                    case eBrowserType.Edge:
                        if (OpenIEModeInEdge)
                        {
                            var ieOptions = new InternetExplorerOptions
                            {
                                AttachToEdgeChrome = true
                            };
                            if (BrowserExecutablePath != null && BrowserExecutablePath.Trim().Length > 0 && File.Exists(BrowserExecutablePath) && BrowserExecutablePath.Contains("msedge.exe"))
                            {
                                ieOptions.EdgeExecutablePath = BrowserExecutablePath;
                            }
                            else if (File.Exists(EDGE_32BIT_BINARY_PATH))
                            {
                                ieOptions.EdgeExecutablePath = EDGE_32BIT_BINARY_PATH;
                            }
                            else if (File.Exists(EDGE_64BIT_BINARY_PATH))
                            {
                                ieOptions.EdgeExecutablePath = EDGE_64BIT_BINARY_PATH;
                            }
                            else
                            {
                                throw new Exception("The Edge browser path is not available in default path. Please install it or provide the valid executable path in 'Browser Executable Path' parameter in agent configuration.");

                            }

                            SetBrowserLogLevel(ieOptions);
                            SetBrowserVersion(ieOptions);
                            if (EnsureCleanSession == true)
                            {
                                ieOptions.EnsureCleanSession = true;
                            }

                            SetProxy(ieOptions);
                            ieOptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                            if (IgnoreIEProtectedMode == true)
                            {
                                ieOptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                                ieOptions.ElementScrollBehavior = InternetExplorerElementScrollBehavior.Bottom;
                            }
                            if (BrowserPrivateMode == true)
                            {
                                ieOptions.ForceCreateProcessApi = true;
                                ieOptions.BrowserCommandLineArguments = "-private";
                            }
                            if (EnableNativeEvents == true)
                            {
                                ieOptions.EnableNativeEvents = true;
                            }
                            if (!(String.IsNullOrEmpty(SeleniumUserArguments) && String.IsNullOrWhiteSpace(SeleniumUserArguments)))
                            {
                                ieOptions.BrowserCommandLineArguments += "," + SeleniumUserArguments;
                            }

                            if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey)))
                            {
                                ieOptions.BrowserCommandLineArguments += "," + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
                            }

                            if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl)))
                            {
                                ieOptions.BrowserCommandLineArguments += "," + WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl;
                            }

                            SetCurrentPageLoadStrategy(ieOptions);
                            ieOptions.IgnoreZoomLevel = true;
                            driverService = InternetExplorerDriverService.CreateDefaultService(GetDriversPathPerOS());
                            AddCustomDriverPath(driverService);
                            driverService.HideCommandPromptWindow = HideConsoleWindow;
                            if (!string.IsNullOrEmpty(RemoteWebDriverUrl))
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteWebDriverUrl), ieOptions);
                            }
                            else
                            {
                                Driver = new InternetExplorerDriver((InternetExplorerDriverService)driverService, ieOptions, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                        }
                        else
                        {
                            EdgeOptions EDOpts = new EdgeOptions();
                            SetBrowserLogLevel(EDOpts);
                            SetBrowserVersion(EDOpts);
                            //EDOpts.AddAdditionalEdgeOption("UseChromium", true);
                            //EDOpts.UseChromium = true;
                            if (HeadlessBrowserMode == true || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            {
                                EDOpts.AddArgument("--headless=new");
                            }
                            SetUnhandledPromptBehavior(EDOpts);
                            if (IsUserProfileFolderPathValid())
                            {
                                EDOpts.AddArgument($"--user-data-dir={UserProfileFolderPath}");
                            }
                            else
                            {
                                SetProxy(EDOpts);
                            }
                            if (BrowserPrivateMode)
                            {
                                EDOpts.AddArgument("-inprivate");
                            }

                            if (SeleniumUserArgs != null)
                            {
                                foreach (string arg in SeleniumUserArgs)
                                {
                                    EDOpts.AddArgument(arg);
                                }
                            }
                            SetCurrentPageLoadStrategy(EDOpts);
                            driverService = EdgeDriverService.CreateDefaultService();//CreateDefaultServiceFromOptions(EDOpts);
                            AddCustomDriverPath(driverService);
                            driverService.HideCommandPromptWindow = HideConsoleWindow;
                            if (!string.IsNullOrEmpty(RemoteWebDriverUrl))
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteWebDriverUrl), EDOpts);
                            }
                            else
                            {
                                Driver = new EdgeDriver((EdgeDriverService)driverService, EDOpts, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            this.mDriverProcessId = driverService.ProcessId;
                        }

                        break;
                    #endregion

                    #region Safari - To be Added
                    //TODO: add Safari
                    #endregion

                    #region Remote Browser/Web Driver
                    case eBrowserType.RemoteWebDriver:
                        if (RemoteBrowserName.Equals("internet explorer"))
                        {
                            ieoptions = new InternetExplorerOptions
                            {
                                EnsureCleanSession = true,
                                IgnoreZoomLevel = true,
                                Proxy = mProxy == null ? null : mProxy,
                                EnableNativeEvents = true,
                                IntroduceInstabilityByIgnoringProtectedModeSettings = true
                            };
                            SetCurrentPageLoadStrategy(ieoptions);
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), ieoptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), ieoptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), ieoptions.ToCapabilities());
                            }

                            break;
                        }
                        else if (RemoteBrowserName.Equals("firefox"))
                        {
                            FirefoxOptions fxOptions = new FirefoxOptions();
                            fxOptions.SetPreference("network.proxy.type", (int)ProxyKind.AutoDetect);

                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), fxOptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), fxOptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), fxOptions.ToCapabilities());
                            }
                            // TODO: make Sauce lab driver/config

                            //TODO: For sauce lab - externalize - try without amdocs proxy hot spot works then it is proxy issue
                            break;
                        }
                        else if (RemoteBrowserName.Equals("chrome"))
                        {
                            ChromeOptions chromeOptions = new ChromeOptions
                            {
                                Proxy = mProxy == null ? null : mProxy
                            };
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), chromeOptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }

                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), chromeOptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), chromeOptions.ToCapabilities());
                            }

                            break;
                        }
                        else if (RemoteBrowserName.Equals("MicrosoftEdge"))
                        {
                            EdgeOptions edgeOptions = new EdgeOptions
                            {
                                Proxy = mProxy
                            };
                            if (!string.IsNullOrEmpty(RemotePlatform))
                            {
                                edgeOptions.AddAdditionalOption(RemotePlatformParam, RemotePlatform);
                            }
                            if (!string.IsNullOrEmpty(RemoteVersion))
                            {
                                edgeOptions.AddAdditionalOption(SeleniumDriver.RemoteVersionParam, RemoteVersion);
                            }

                            SetUnhandledPromptBehavior(edgeOptions);
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), edgeOptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), edgeOptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), edgeOptions.ToCapabilities());
                            }

                            break;
                        }
                        else if (RemoteBrowserName.Equals("safari"))
                        {
                            SafariOptions safariOptions = new SafariOptions
                            {
                                Proxy = mProxy
                            };
                            if (!string.IsNullOrEmpty(RemotePlatform))
                            {
                                safariOptions.AddAdditionalOption(RemotePlatformParam, RemotePlatform);
                            }
                            if (!string.IsNullOrEmpty(RemoteVersion))
                            {
                                safariOptions.AddAdditionalOption(SeleniumDriver.RemoteVersionParam, RemoteVersion);
                            }

                            SetUnhandledPromptBehavior(safariOptions);
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), safariOptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), safariOptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), safariOptions.ToCapabilities());
                            }

                            break;
                        }
                        else
                        {

                            InternetExplorerOptions internetExplorerOptions = new InternetExplorerOptions();
                            if (!string.IsNullOrEmpty(RemotePlatform))
                            {
                                internetExplorerOptions.AddAdditionalOption(RemotePlatformParam, RemotePlatform);
                            }
                            if (!string.IsNullOrEmpty(RemoteVersion))
                            {
                                internetExplorerOptions.AddAdditionalOption(SeleniumDriver.RemoteVersionParam, RemoteVersion);
                            }
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), (ICapabilities)internetExplorerOptions, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            }
                            else if (WorkSpace.Instance.BetaFeatures.ShowHealenium && IsHealenium)
                            {
                                Driver = new RemoteWebDriver(new Uri(HealeniumUrl), internetExplorerOptions.ToCapabilities());
                            }
                            else
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), internetExplorerOptions);
                            }

                            break;
                        }
                        #endregion
                }

                if (BrowserMinimized == true && mBrowserType != eBrowserType.Edge)
                {
                    Driver.Manage().Window.Minimize();
                }

                if (!string.IsNullOrEmpty(BrowserHeight) && !string.IsNullOrEmpty(BrowserWidth))
                {
                    Driver.Manage().Window.Size = new Size() { Height = Convert.ToInt32(BrowserHeight), Width = Convert.ToInt32(BrowserWidth) };
                }
                else if (HeadlessBrowserMode || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Driver.Manage().Window.Size = new Size() { Height = Convert.ToInt32(1080), Width = Convert.ToInt32(1920) };
                }
                else
                {
                    Driver.Manage().Window.Maximize();
                }
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));

                //set pageLoad timeout limit
                if (PageLoadTimeOut == 0)
                {
                    PageLoadTimeOut = 60;
                }

                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(PageLoadTimeOut);

                DefaultWindowHandler = Driver.CurrentWindowHandle;
                InitXpathHelper();

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in start driver", ex);
                ErrorMessageFromDriver = ex.Message;

                CloseDriverProcess(driverService);
                //If driver is mismatched, use selenium manager to get the latest driver
                if (RestartRetry && (ex.Message.Contains("session not created: This version of", StringComparison.InvariantCultureIgnoreCase) ||
                    ex.Message.StartsWith("unable to obtain", StringComparison.InvariantCultureIgnoreCase) ||
                    ex.Message.StartsWith("error starting process", StringComparison.InvariantCultureIgnoreCase)))
                {
                    RestartRetry = false;
                    UpdateDriver(mBrowserType);
                    StartDriver();
                }
            }
        }
        //created common method for Chrome and Brave browser because both support ChromeDriver
        private void configChromeDriverAndStart(ChromeOptions options)
        {

            options.AddArgument("--start-maximized");
            SetCurrentPageLoadStrategy(options);
            SetBrowserLogLevel(options);
            SetUnhandledPromptBehavior(options);
            SetBrowserVersion(options);
            if (IsUserProfileFolderPathValid())
            {
                options.AddArguments("user-data-dir=" + UserProfileFolderPath);
            }
            else if (!string.IsNullOrEmpty(ExtensionPath))
            {
                string[] extensionPaths = ExtensionPath.Split(';');
                options.AddExtensions(extensionPaths);
            }

            SetProxy(options);

            if (!string.IsNullOrEmpty(DownloadFolderPath))
            {
                if (!System.IO.Directory.Exists(DownloadFolderPath))
                {
                    System.IO.Directory.CreateDirectory(DownloadFolderPath);
                }
                options.AddUserProfilePreference("download.default_directory", DownloadFolderPath);
            }

            if (BrowserPrivateMode == true)
            {
                options.AddArgument("--incognito");
            }

            if (HeadlessBrowserMode == true || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                options.AddArgument("--headless=new");
            }

            if (SeleniumUserArgs != null)
            {
                foreach (string arg in SeleniumUserArgs)
                {
                    options.AddArgument(arg);
                }
            }

            if (!string.IsNullOrEmpty(EmulationDeviceName))
            {
                options.EnableMobileEmulation(EmulationDeviceName);
            }
            else if (!string.IsNullOrEmpty(BrowserUserAgent))
            {
                options.AddArgument("--user-agent=" + BrowserUserAgent.Trim());
            }

            if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey)))
            {
                options.AddArgument(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey);
            }

            if (!(String.IsNullOrEmpty(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl) && String.IsNullOrWhiteSpace(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl)))
            {
                options.AddArgument(WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl);
            }

            if (!string.IsNullOrEmpty(DebugAddress))
            {
                options.DebuggerAddress = DebugAddress.Trim();
            }

            driverService = ChromeDriverService.CreateDefaultService();

            AddCustomDriverPath(driverService);

            if (GetIsRunningInDocker())
            {
                options.AddArgument("--headless");
                options.AddArgument("--no-sandbox");
            }

            if (HideConsoleWindow)
            {
                driverService.HideCommandPromptWindow = HideConsoleWindow;
            }

            try
            {
                if (!string.IsNullOrEmpty(RemoteWebDriverUrl))
                {
                    Driver = new RemoteWebDriver(new Uri(RemoteWebDriverUrl), options);
                }
                else
                {
                    Driver = new ChromeDriver((ChromeDriverService)driverService, options, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                }

                this.mDriverProcessId = driverService.ProcessId;
            }
            catch (Exception ex)
            {

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && ex.Message.Contains("no such file or directory", StringComparison.CurrentCultureIgnoreCase))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error while launching chromedriver", ex);
                    Reporter.ToLog(eLogLevel.INFO, "Chrome binary isn't found at default location, checking for Chromium...");

                    if (Directory.GetFiles(@"/usr/bin", "chromium-browser.*").Length > 0 && Directory.GetFiles(@"/usr/lib/chromium", "chromedriver.*").Length > 0)
                    {
                        options.BinaryLocation = @"/usr/bin/chromium-browser";

                        //List of Chromium Command Line Switches
                        //https://peter.sh/experiments/chromium-command-line-switches/
                        options.AddArgument("--headless");
                        options.AddArgument("--no-sandbox");
                        options.AddArgument("--start-maximized");
                        options.AddArgument("--disable-dev-shm-usage");
                        options.AddArgument("--remote-debugging-port=9222");
                        options.AddArgument("--disable-gpu");
                        Driver = new ChromeDriver(@"/usr/lib/chromium", options, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                    }
                    else
                    {
                        throw ex;
                    }
                }
                else
                {
                    throw ex;
                }
            }
        }
        private void AddCustomDriverPath(DriverService driverService)
        {
            if (!string.IsNullOrWhiteSpace(DriverFilePath))
            {
                if (File.Exists(DriverFilePath))
                {
                    driverService.DriverServicePath = Path.GetDirectoryName(DriverFilePath);
                    driverService.DriverServiceExecutableName = Path.GetFileName(DriverFilePath);
                }
                else
                {
                    throw new FileNotFoundException($"Invalid path provided in Custom Driver File in {nameof(DriverFilePath)} in Agent Configuration. Please provide valid path or consider removing it if not needed.");
                }
            }
        }
        private static bool GetIsRunningInDocker()
        {
            var env = System.Environment.GetEnvironmentVariable("GINGER_EXE_ENV");
            return env == "docker";
        }

        private string UpdateDriver(eBrowserType browserType)
        {
            try
            {
                Reporter.ToLog(eLogLevel.INFO, $"Failed to Download latest {mBrowserType} driver. Attempting to Update {mBrowserType} driver to latest using System Proxy Settings....");
                DriverOptions driverOptions = null;

                driverOptions = mBrowserType switch
                {
                    eBrowserType.Chrome => new ChromeOptions(),
                    eBrowserType.Edge => new EdgeOptions(),
                    eBrowserType.FireFox => new FirefoxOptions(),
                    _ => null
                };

                if (driverOptions == null)
                {
                    // Other browsers not supported, return without update
                    return "";
                }

                //Try get system proxy to send it to Selenium manager to update the driver.
                string systemProxy = null;
                try
                {
                    systemProxy = OperatingSystemBase.GetSystemProxy();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get System Proxy Settings.", ex);
                }

                if (!string.IsNullOrEmpty(systemProxy))
                {
                    var proxy = new Proxy
                    {
                        Kind = ProxyKind.Manual,
                        HttpProxy = systemProxy.ToString(),
                        SslProxy = systemProxy.ToString()
                    };
                    driverOptions.Proxy = proxy;
                }

                SetBrowserVersion(driverOptions);
                var driverFinder = new DriverFinder(driverOptions);
                var driverpath = driverFinder.GetDriverPath();
                Reporter.ToLog(eLogLevel.INFO, $"Updated {browserType} driver to latest and placed in {driverpath}.");
                return driverpath;
            }
            catch (Exception ex)
            {
                if (!WorkSpace.Instance.RunningInExecutionMode && !WorkSpace.Instance.RunningFromUnitTest)
                {
                    Reporter.ToUser(eUserMsgKey.FailedToDownloadDriver, mBrowserType);
                }
                Reporter.ToLog(eLogLevel.ERROR, string.Format(Reporter.UserMsgsPool[eUserMsgKey.FailedToDownloadDriver].Message, mBrowserType), ex);
                throw;
            }
        }

        public string GetDriverPath(eBrowserType browserType)
        {
            string DriverPath = string.Empty;
            DriverPath = UpdateDriver(browserType);
            return DriverPath;
        }
        private static void CloseDriverProcess(DriverService driverService)
        {
            //Close launched driver process as it does not gets closed by Selenium in case of exception
            if (driverService != null && driverService.ProcessId != 0)
            {
                try
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Closing Driver process which was left open after failure to start driver.");
                    System.Diagnostics.Process.GetProcessById(driverService.ProcessId)?.Kill();
                }
                catch { }
            }
        }

        public string GetDriversPathPerOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Use64Bitbrowser && (mBrowserType == eBrowserType.IE || mBrowserType == eBrowserType.FireFox))
                {
                    return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Win64");
                }
                else
                {
                    return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("The '{0}' OS is not supported by Ginger Selenium", RuntimeInformation.OSDescription));
                return null;
            }
        }

        /// <summary>
        /// Gets browser driver file with path
        /// </summary>
        /// <param name="fileName">driver name</param>
        /// <returns></returns>
        private string DriverServiceFileNameWithPath(string fileName)
        {
            return Path.Combine(GetDriversPathPerOS(), DriverServiceFileName(fileName));

        }

        /// <summary>
        /// Gets browser driver name according to running oS
        /// </summary>
        /// <param name="fileName">driver name</param>
        /// <returns></returns>
        private static string DriverServiceFileName(string fileName)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return $"{fileName}.exe";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return fileName;
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("The '{0}' OS is not supported by Ginger Selenium", RuntimeInformation.OSDescription));
                return "";
            }
        }

        private string[] AddByPassAddress()
        {
            return ByPassProxy.Split(';');
        }

        private void SetProxy(dynamic options)
        {
            if (mProxy == null)
            {
                return;
            }

            var proxy = new Proxy();

            options.Proxy = proxy;
            if (this.UseSecurityTesting)
            {
                options.AcceptInsecureCertificates = true;
            }


            switch (mProxy.Kind)
            {
                case ProxyKind.Manual:
                    options.Proxy.Kind = ProxyKind.Manual;
                    options.Proxy.HttpProxy = mProxy.HttpProxy;
                    options.Proxy.SslProxy = mProxy.SslProxy;

                    if (!string.IsNullOrEmpty(ByPassProxy))
                    {
                        options.Proxy.AddBypassAddresses(AddByPassAddress());
                    }

                    //TODO: GETTING ERROR LAUNCHING BROWSER 
                    // options.Proxy.SocksProxy = mProxy.SocksProxy;
                    break;

                case ProxyKind.ProxyAutoConfigure:
                    options.Proxy.Kind = ProxyKind.ProxyAutoConfigure;
                    options.Proxy.ProxyAutoConfigUrl = mProxy.ProxyAutoConfigUrl;
                    break;

                case ProxyKind.Direct:
                    options.Proxy.Kind = ProxyKind.Direct;
                    break;

                case ProxyKind.AutoDetect:
                    options.Proxy.Kind = ProxyKind.AutoDetect;
                    break;

                case ProxyKind.System:
                    options.Proxy.Kind = ProxyKind.System;
                    break;

                default:
                    options.Proxy.Kind = ProxyKind.System;
                    break;
            }
        }

        public override void CloseDriver()
        {
            try
            {
                if (POMUtils != null)
                {
                    POMUtils.ProcessingStatusChanged -= POMUtils_ProcessingStatusChanged;
                }

                if (Driver != null)
                {
                    Driver.Close();
                }
                if (StartBMP)
                {
                    BMPClient.Close();
                    BMPServer.Stop();
                }
            }
            catch (System.InvalidOperationException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Got System.InvalidOperationException when trying to close Selenium Driver", ex);
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close Selenium Driver", e);
            }

            try
            {
                if (isNetworkLogMonitoringStarted)
                {
                    StopNetworkLog().GetAwaiter().GetResult();
                }
                if (Driver != null)
                {
                    Driver.Quit();
                    Driver = null;
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to quit Selenium Driver", e);
            }
        }

        public override Act GetCurrentElement()
        {
            try
            {
                Act act = null;
                IWebElement currentElement = Driver.SwitchTo().ActiveElement();

                string tagname = currentElement.TagName;

                if (tagname == "input")
                {
                    string ctlType = currentElement.GetAttribute("type");

                    switch (ctlType)
                    {
                        case "text":
                            act = getActTextBox(currentElement);
                            break;
                        case "button":
                            act = getActButton(currentElement);
                            break;
                        case "submit":
                            act = getActButton(currentElement);
                            break;
                        case "reset":
                            //TODO: add missing Act get() method
                            break;
                        case "file":
                            //TODO: add missing Act get() method
                            break;
                        case "hidden": // does type this apply?
                            //TODO: add missing Act get() method
                            break;
                        case "password":
                            act = getActPassword(currentElement);
                            break;
                        case "checkbox":
                            act = getActCheckbox(currentElement);
                            break;
                        case "radio":
                            act = getActRadioButton(currentElement);
                            break;
                    }
                    return act;
                }

                if (tagname == "a")
                {
                    act = getActLink(currentElement);
                    return act;
                }
                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while getting current element", ex);
                return null;
            }
        }

        private Act getActButton(IWebElement currentElement)
        {
            ActButton act = new ActButton();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActPassword(IWebElement currentElement)
        {
            ActPassword a = new ActPassword();
            setActLocator(currentElement, a);
            a.PasswordAction = ActPassword.ePasswordAction.SetValue;
            a.AddOrUpdateInputParamValue("Value", currentElement.GetAttribute("value"));
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
        }

        private Act getActRadioButton(IWebElement currentElement)
        {
            ActRadioButton act = new ActRadioButton();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActCheckbox(IWebElement currentElement)
        {
            ActCheckbox act = new ActCheckbox();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        public void setActLocator(IWebElement currentElement, Act act)
        {
            //order by priority

            // By ID
            string locVal = currentElement.GetAttribute("id");
            if (locVal != "")
            {
                act.LocateBy = eLocateBy.ByID;
                act.LocateValue = locVal;
                return;
            }

            // By name
            locVal = currentElement.GetAttribute("name");
            if (locVal != "")
            {
                act.LocateBy = eLocateBy.ByName;
                act.LocateValue = locVal;
                return;
            }

            //TODO: CSS....

            //By href
            locVal = currentElement.GetAttribute("href");
            if (locVal != "")
            {
                act.LocateBy = eLocateBy.ByHref;
                act.LocateValue = locVal;
                return;
            }

            //By Value
            locVal = currentElement.GetAttribute("value");
            if (locVal != "")
            {
                act.LocateBy = eLocateBy.ByValue;
                act.LocateValue = locVal;
                return;
            }

            // by text
            locVal = currentElement.Text;
            if (locVal != "")
            {
                act.LocateBy = eLocateBy.ByLinkText;
                act.LocateValue = locVal;
                return;
            }
            //TODO: add XPath
        }



        private Act getActLink(IWebElement currentElement)
        {
            ActLink al = new ActLink();
            setActLocator(currentElement, al);
            al.AddOrUpdateInputParamValue("Value", currentElement.Text);
            return al;
        }

        private Act getActTextBox(IWebElement currentElement)
        {
            ActTextBox a = new ActTextBox();
            setActLocator(currentElement, a);
            a.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
            a.AddOrUpdateInputParamValue("Value", currentElement.GetAttribute("value"));
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
        }

        public Uri ValidateURL(String sURL)
        {
            Uri myurl;
            if (Uri.TryCreate(sURL, UriKind.Absolute, out myurl))
            {
                return myurl;
            }
            return null;
        }

        private void GotoURL(Act act, string sURL)
        {
            if (string.IsNullOrEmpty(sURL))
            {
                act.Error = "Error: Provided URL is empty. Please provide valid URL.";
                return;
            }

            if (sURL.StartsWith("www", StringComparison.CurrentCultureIgnoreCase))
            {
                sURL = "http://" + sURL;
            }
            try
            {
                Uri uri = ValidateURL(sURL);
                if (uri != null)
                {
                    Driver.Navigate().GoToUrl(uri.AbsoluteUri);
                }
                else
                {
                    act.Error = "Error: Invalid URL. Give valid URL(Complete URL)";
                }
                if (Driver.GetType() == typeof(InternetExplorerDriver) && Driver.Title.Contains("Certificate Error", StringComparison.CurrentCultureIgnoreCase))
                {
                    Thread.Sleep(100);
                    try
                    {
                        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                        Driver.Navigate().GoToUrl("javascript:document.getElementById('overridelink').click()");
                    }
                    catch { }
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWait);
                }

                //just to be sure the page is fully loaded
                CheckifPageLoaded();
            }
            catch (Exception ex)
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.Error += ex.Message;
            }
        }

        public override string GetURL()
        {
            return Driver.Url;
        }


        public override void RunAction(Act act)
        {
            //Checking if Alert handling is asked to be performed (in that case we can't modify anything on driver before handling the Alert)
            bool isActBrowser = act is ActBrowserElement;
            ActBrowserElement actBrowserObj = isActBrowser ? (act as ActBrowserElement) : null;
            bool runActHandlerDirect = act is ActHandleBrowserAlert ||
                                      (isActBrowser && (actBrowserObj.ControlAction == ActBrowserElement.eControlAction.SwitchToDefaultWindow
                                      || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.AcceptMessageBox
                                      || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.DismissMessageBox
                                      //Added below 2 conditions for comparision for Alert Text Box
                                      || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.GetMessageBoxText
                                      || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.SetAlertBoxText));

            if (!runActHandlerDirect)
            {
                //implicityWait must be done on actual window so need to make sure the driver is pointing on window
                try
                {
                    //it's wait until all page gets load 
                    if (act is not ActWebSmartSync { SyncOperations: ActWebSmartSync.eSyncOperation.PageHasBeenLoaded })
                    {
                        _ = Driver.Title;//just to make sure window attributes do not throw exception
                    }
                }
                catch (Exception ex)
                {
                    if (Driver.WindowHandles.Count == 1)
                    {
                        Driver.SwitchTo().Window(Driver.WindowHandles[0]);
                    }
                    Reporter.ToLog(eLogLevel.ERROR, "Selenium Driver is not accessible, probably because there is Alert window open", ex);
                }

                if (act.Timeout is not null and not 0)
                {
                    //if we have time out on action then set it on the driver
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds((int)act.Timeout);
                }
                else
                {
                    // use the driver config timeout
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWait);
                }

                if (StartBMP)
                {
                    // Create new HAR for each action, so it will clean the history
                    BMPClient.NewHar("aaa");

                    DoRunAction(act);

                    //TODO: call GetHARData and add it as screen shot or...
                    // GetHARData();

                    // TODO: save it in the solution docs... 
                    string filename = @"c:\temp\har\" + act.Description + " - " + DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss_fff") + ".har";
                    BMPClient.SaveHAR(filename);

                    Act.AddArtifactToAction(Path.GetFileName(filename), act, filename);

                    act.ExInfo += "Action HAR file saved at: " + filename;
                }
            }

            DoRunAction(act);
        }

        private void DoRunAction(Act act)
        {
            // Get the type of the act  
            Type actType = act.GetType();

            // Use switch expression based on the type of act  
            switch (act)
            {
                case ActUIElement uiElement:
                    HandleActUIElement(uiElement);
                    break;

                case ActGotoURL gotoUrl:
                    GotoURL(gotoUrl, act.GetInputParamCalculatedValue("Value"));
                    break;

                case ActGenElement genElement:
                    GenElementHandler(genElement);
                    break;

                case ActSmartSync smartSync:
                    SmartSyncHandler(smartSync);
                    break;

                case ActWebSmartSync webSmartSync:
                    WebSmartSyncHandler(webSmartSync);
                    break;

                case ActTextBox textBox:
                    ActTextBoxHandler(textBox);
                    break;

                case ActPWL pwl:
                    PWLElementHandler(pwl);
                    break;

                case ActHandleBrowserAlert handleBrowserAlert:
                    HandleBrowserAlert(handleBrowserAlert);
                    break;

                case ActVisualTesting visualTesting:
                    HandleActVisualTesting(visualTesting);
                    break;

                case ActPassword password:
                    ActPasswordHandler(password);
                    break;

                case ActLink link:
                    ActLinkHandler(link);
                    break;

                case ActButton button:
                    ActButtonHandler(button);
                    break;

                case ActCheckbox checkbox:
                    ActCheckboxHandler(checkbox);
                    break;

                case ActDropDownList dropdown:
                    ActDropDownListHandler(dropdown);
                    break;

                case ActRadioButton radioButton:
                    ActRadioButtonHandler(radioButton);
                    break;

                case ActMultiselectList multiselectList:
                    string csv = act.GetInputParamValue("Value");
                    string[] parts = csv.Split('|'); // Make sure values are separated by '|'  
                    List<string> optionList = [.. parts];

                    switch (multiselectList.ActMultiselectListAction)
                    {
                        case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByIndex:
                            SelectMultiselectListOptionsByIndex(multiselectList, optionList.ConvertAll(int.Parse));
                            break;

                        case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByText:
                            SelectMultiselectListOptionsByText(multiselectList, optionList);
                            break;

                        case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByValue:
                            SelectMultiselectListOptionsByValue(multiselectList, optionList);
                            break;

                        case ActMultiselectList.eActMultiselectListAction.ClearAllSelectedValues:
                            DeSelectMultiselectListOptions(multiselectList);
                            break;
                    }
                    break;

                case ActHello hello:
                    // TODO: return hello from...  
                    break;

                case ActScreenShot screenShot:
                    ScreenshotHandler(screenShot);
                    break;

                case ActSubmit submit:
                    ActsubmitHandler(submit);
                    break;

                case ActLabel label:
                    ActLabelHandler(label);
                    break;

                case ActWebSitePerformanceTiming websitePerformanceTiming:
                    ActWebSitePerformanceTimingHandler(websitePerformanceTiming);
                    break;

                case ActSwitchWindow switchWindow:
                    ActSwitchWindowHandler(switchWindow);
                    break;

                case ActBrowserElement browserElement:
                    ActBrowserElementHandler(browserElement);
                    break;

                case ActAgentManipulation agentManipulation:
                    ActAgentManipulationHandler(agentManipulation);
                    break;

                case ActAccessibilityTesting accessibilityTesting:
                    ActAccessibility(accessibilityTesting);
                    break;
                case ActSecurityTesting securityTesting:
                    ActSecurity(securityTesting);
                    break;
                default:
                    act.Error = "Run Action Failed due to unrecognized action type - " + actType.ToString();
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    break;
            }
        }

        private void ActSecurity(ActSecurityTesting act)
        {
            string testURL = Driver.Url;
            if (act.ScanType == ActSecurityTesting.eScanType.Active)
            {
                act.ExecuteActiveZapScan(testURL);
            }
            else
            {
                act.ExecutePassiveZapScan("", act);
            }
        }

        private void ActAccessibility(ActAccessibilityTesting act)
        {
            string gotoUrl = string.Empty;
            IWebElement e = null;
            if (act.GetInputParamValue(ActAccessibilityTesting.Fields.Target) == nameof(ActAccessibilityTesting.eTarget.Element))
            {
                if (!string.IsNullOrEmpty(act.LocateValueCalculated) && act.LocateBy != eLocateBy.NA)
                {
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error += "Element not found: " + act.LocateBy + "=" + act.LocateValueCalculated;
                        return;
                    }
                }
            }
            act.AnalyzerAccessibility(Driver, e);
        }

        private void ScreenshotHandler(ActScreenShot act)
        {
            try
            {
                if (act.WindowsToCapture == Act.eWindowsToCapture.OnlyActiveWindow)
                {
                    AddCurrentScreenShot(act);
                }
                else if (act.WindowsToCapture == Act.eWindowsToCapture.FullPage || TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure)
                {
                    AddScreenshotIntoAct(act, true);
                }
                else if (act.WindowsToCapture == Act.eWindowsToCapture.FullPageWithUrlAndTimestamp)
                {
                    TakeFullPageWithDesktopScreenScreenShot(act);
                }
                else
                {
                    //keep the current window and switch back at the end
                    String currentWindow = Driver.CurrentWindowHandle;

                    ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
                    foreach (String winHandle in openWindows)
                    {
                        Driver.SwitchTo().Window(winHandle);
                        AddCurrentScreenShot(act);
                    }
                    //Switch back to the last window
                    Driver.SwitchTo().Window(currentWindow);
                }

            }
            catch (Exception ex)
            {
                act.Error = "Failed to create Selenuim WebDriver browser page screenshot. Error= " + ex.Message;
                return;
            }
        }

        private void TakeFullPageWithDesktopScreenScreenShot(Act act)
        {
            List<Bitmap> bitmapsToMerge = [];
            string filepath = null;
            try
            {
                using (Bitmap browserHeaderScreenshot = GetBrowserHeaderScreenShot())
                {
                    if (browserHeaderScreenshot != null)
                    {
                        bitmapsToMerge.Add(browserHeaderScreenshot);
                    }
                    using (Bitmap browserFullPageScreenshot = GetScreenShot(true))
                    {
                        if (browserFullPageScreenshot != null)
                        {
                            bitmapsToMerge.Add(browserFullPageScreenshot);
                        }
                        using (Bitmap taskbarScreenshot = TargetFrameworkHelper.Helper.GetTaskbarScreenshot())
                        {
                            if (taskbarScreenshot != null)
                            {
                                bitmapsToMerge.Add(taskbarScreenshot);
                            }
                            filepath = TargetFrameworkHelper.Helper.MergeVerticallyAndSaveBitmaps(bitmapsToMerge.ToArray());
                        }
                    }
                }
                if (!string.IsNullOrEmpty(filepath))
                {
                    act.ScreenShotsNames.Add(Driver.Title);
                    act.ScreenShots.Add(filepath);
                }
            }
            catch (Exception ex)
            {
                act.Error = "Failed to create Selenuim WebDriver browser page screenshot. Error= " + ex.Message;
                return;
            }
            finally
            {
                bitmapsToMerge.Clear();
            }
        }

        private Bitmap GetBrowserHeaderScreenShot()
        {
            if (HeadlessBrowserMode)
            {
                return null;
            }

            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Driver;

            Point browserWindowPosition = Driver.Manage().Window.Position;
            Size browserWindowSize = GetWindowSize();
            Size viewportSize = new()
            {
                Width = (int)(long)javaScriptExecutor.ExecuteScript("return window.innerWidth"),
                Height = (int)(long)javaScriptExecutor.ExecuteScript("return window.innerHeight")
            };
            object devicePixelRatioAsObject = javaScriptExecutor.ExecuteScript("return window.devicePixelRatio");
            double devicePixelRatio;
            if (double.TryParse(devicePixelRatioAsObject.ToString(), out double devicePixelRatioAsDouble))
            {
                devicePixelRatio = devicePixelRatioAsDouble;
            }
            else if (long.TryParse(devicePixelRatioAsObject.ToString(), out long devicePixelRatioAsLong))
            {
                devicePixelRatio = devicePixelRatioAsLong;
            }
            else
            {
                throw new NotImplementedException($"Cannot cast device pixel ratio value {devicePixelRatioAsObject}. Value is of type {devicePixelRatioAsObject.GetType().FullName}.");
            }

            return TargetFrameworkHelper.Helper.GetBrowserHeaderScreenshot(browserWindowPosition, browserWindowSize, viewportSize, devicePixelRatio);
        }

        private void AddCurrentScreenShot(ActScreenShot act)
        {
            Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
            act.AddScreenShot(ss.AsByteArray, Driver.Title);
        }



        // private void createScreenShot(Act act)
        // {
        //TODO: FIXME !!!!!
        // if (GingerRunner.UseExeuctionLogger)
        // {


        //TODO: uncomment when we use exec log, and delete the below
        //if FlagsAttribute...
        //{
        //        Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
        //string FileName = act.GetFileNameForScreenShot();
        //ss.SaveAsFile(FileName, System.Drawing.Imaging.ImageFormat.Png);
        //act.AddScreenShot(FileName);
        //}

        // Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();

        // string filename = Path.GetTempFileName();

        // ss.SaveAsFile(filename, System.Drawing.Imaging.ImageFormat.Png);
        //ss.SaveAsFile(filename, System.Drawing.Imaging.ImageFormat.MemoryBmp);
        // Bitmap tmp = new System.Drawing.Bitmap(filename);
        //tmp = new Bitmap(tmp, new System.Drawing.Size(tmp.Width / 2, tmp.Height / 2));
        //act.ScreenShots.Add(filename);

        //  }

        private void ActSwitchWindowHandler(ActSwitchWindow act)
        {
            SwitchWindow(act);
        }

        private void ActWebSitePerformanceTimingHandler(ActWebSitePerformanceTiming ABPT)
        {
            // Get perf timing object and loop over the values adding them to return vals
            //fixed for IE Driver as it was throwing error "Unable to cast object of type 'OpenQA.Selenium.Remote.RemoteWebElement' to type 'System.Collections.Generic.Dictionary`2[System.String,System.Object]'."
            var scriptToExecute = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var timings = performance.timing || {}; return timings.toJSON();";
            Dictionary<string, object> dic = (Dictionary<string, object>)((IJavaScriptExecutor)Driver).ExecuteScript(scriptToExecute);

            ABPT.AddNewReturnParams = true;
            foreach (KeyValuePair<string, object> entry in dic)
            {
                if (entry.Key != "toJSON")
                {
                    ABPT.AddOrUpdateReturnParamActual(entry.Key, entry.Value.ToString());
                }
            }

            ABPT.SetInfo();
        }

        private void GetDropDownListOptions(Act act, IWebElement e)
        {
            // there is better way to get the options
            ReadOnlyCollection<IWebElement> elems = e.FindElements(By.TagName("option"));
            string s = "";
            foreach (IWebElement e1 in elems)
            {
                s = s + e1.Text + "|";
            }
            act.AddOrUpdateReturnParamActual("Actual", s);
        }

        private void ActsubmitHandler(ActSubmit actSubmit)
        {
            IWebElement e = LocateElement(actSubmit);
            if (e != null)
            {
                e.SendKeys("");
                e.Submit();
            }
            else
            {
                actSubmit.Error = "Submit Element not found - " + actSubmit.LocateBy + "-" + actSubmit.LocateValue;
            }
        }

        private void ActLabelHandler(ActLabel actLabel)
        {
            IWebElement e = LocateElement(actLabel);
            if (e != null)
            {
                if (actLabel.LabelAction == ActLabel.eLabelAction.IsVisible)
                {
                    actLabel.AddOrUpdateReturnParamActual("Actual", "True");
                }
                else
                {
                    actLabel.AddOrUpdateReturnParamActual("Actual", e.Text);
                }
            }
            else
            {
                if (actLabel.LabelAction == ActLabel.eLabelAction.IsVisible)
                {
                    actLabel.AddOrUpdateReturnParamActual("Actual", "False");
                }
            }
        }

        private void ActTextBoxHandler(ActTextBox actTextBox)
        {
            //TODO: all other places must set error in case element not found
            IWebElement e = LocateElement(actTextBox);
            if (e == null || e.Displayed == false)
            {
                actTextBox.Error = "Error: Element not found - " + actTextBox.LocateBy + " " + actTextBox.LocateValue;
                return;
            }

            switch (actTextBox.TextBoxAction)
            {
                case ActTextBox.eTextBoxAction.SetValueFast:
                    e.Clear();
                    //Check if there is faster way to set value
                    if (!String.IsNullOrEmpty(actTextBox.GetInputParamCalculatedValue("Value")))
                    {
                        if (Driver.GetType() == typeof(FirefoxDriver))
                        {
                            e.SendKeys(actTextBox.GetInputParamCalculatedValue("Value"));
                        }
                        else
                        {
                            //TODO: How do we check for errors? do negative UT check for below
                            // + Why FF is different? what happened?
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, actTextBox.GetInputParamCalculatedValue("Value"));
                        }
                    }
                    break;
                case ActTextBox.eTextBoxAction.Clear:
                    e.Clear();

                    break;
                case ActTextBox.eTextBoxAction.SetValue:
                    e.Clear();
                    //Check if there is faster way to set value
                    if (!String.IsNullOrEmpty(actTextBox.GetInputParamCalculatedValue("Value")))
                    {
                        e.SendKeys(actTextBox.GetInputParamCalculatedValue("Value"));
                    }
                    break;
                case ActTextBox.eTextBoxAction.GetValue:
                    //TODO: New Style - Jack update all other actions
                    if (!string.IsNullOrEmpty(e.Text))
                    {
                        actTextBox.AddOrUpdateReturnParamActual("Actual", e.Text);
                    }
                    else
                    {
                        actTextBox.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("value"));
                    }

                    //Do it once after actions Actual -> to Var
                    //if (actTextBox.VarbObj != null) actTextBox.VarbObj.Value = actTextBox.Actual;
                    break;
                default:
                    //TODO: err
                    break;

            }

            if (actTextBox.TextBoxAction == ActTextBox.eTextBoxAction.IsDisabled)
            {
                if (e != null)
                {
                    actTextBox.AddOrUpdateReturnParamActual("Actual", !e.Enabled + "");
                }
            }
            if (actTextBox.TextBoxAction == ActTextBox.eTextBoxAction.GetFont)
            {

                if (e != null)
                {
                    actTextBox.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("font"));
                }
            }
            if (actTextBox.TextBoxAction == ActTextBox.eTextBoxAction.IsDisplayed)
            {
                if (e != null)
                {
                    actTextBox.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
                }
            }
            if (actTextBox.TextBoxAction == ActTextBox.eTextBoxAction.IsPrepopulated)
            {
                if (e != null)
                {
                    actTextBox.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("value").Trim() != "").ToString());
                }
            }
            if (actTextBox.TextBoxAction == ActTextBox.eTextBoxAction.GetInputLength)
            {
                if (e != null)
                {
                    actTextBox.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("value").Length).ToString());
                }
            }
        }

        private void ActPasswordHandler(ActPassword actPassword)
        {
            IWebElement e = LocateElement(actPassword);
            if (e == null || e.Displayed == false)
            {
                actPassword.Error = "Error: Element not found - " + actPassword.LocateBy + " " + actPassword.LocateValue;
                return;
            }

            if (actPassword.PasswordAction == ActPassword.ePasswordAction.SetValue)
            {
                e.Clear();
                e.SendKeys(actPassword.GetInputParamCalculatedValue("Value"));
            }
            if (actPassword.PasswordAction == ActPassword.ePasswordAction.IsDisabled)
            {
                actPassword.AddOrUpdateReturnParamActual("Actual", !e.Enabled + "");
            }
            if (actPassword.PasswordAction == ActPassword.ePasswordAction.GetSize)
            {
                actPassword.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("size").ToString());
            }
            if (actPassword.PasswordAction == ActPassword.ePasswordAction.GetStyle)
            {
                try
                {
                    actPassword.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                }
                catch
                {
                    actPassword.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                }
            }

            if (actPassword.PasswordAction == ActPassword.ePasswordAction.GetHeight)
            {
                actPassword.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
            }

            if (actPassword.PasswordAction == ActPassword.ePasswordAction.GetWidth)
            {
                actPassword.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
            }
        }

        /// <summary>
        /// Retrieves the appropriate locators for the ActWebSmartSync action based on the provided parameters.
        /// </summary>
        /// <param name="act">The ActWebSmartSync action containing the synchronization details.</param>
        /// <param name="pomExecutionUtil">The POMExecutionUtils object used to retrieve information about the current POM.</param>
        /// <returns>A tuple containing the locateBy and locateValue for the ActWebSmartSync action.</returns>
        internal List<ElementLocator> GetLocatorsForWebSmartSync(ActWebSmartSync act, POMExecutionUtils pomExecutionUtil)
        {
            List<ElementLocator> activeSupportedLocators = [];
            if (act.ElementLocateBy == eLocateBy.POMElement)
            {
                if (pomExecutionUtil.GetCurrentPOM() == null)
                {
                    throw new Exception("Relevant POM not found. Ensure that the POM context is correctly initialized before invoking this operation.");

                }

                ElementInfo currentPOMElementInfo = pomExecutionUtil.GetCurrentPOMElementInfo();
                if (currentPOMElementInfo == null)
                {
                    throw new Exception("Unable to find details about the POM. Check if the POM element information is correctly set.");
                }


                if (act.UseAllLocators)
                {
                    activeSupportedLocators = currentPOMElementInfo.Locators
                        .Where(l => l.Active && ActWebSmartSync.SupportedLocatorsTypeList.Contains(l.LocateBy))
                        .ToList();
                }
                else
                {
                    var singleLocator = currentPOMElementInfo.Locators
                        .FirstOrDefault(l => l.Active && ActWebSmartSync.SupportedLocatorsTypeList.Contains(l.LocateBy));

                    if (singleLocator != null)
                    {
                        activeSupportedLocators.Add(singleLocator);
                    }
                }
                if (activeSupportedLocators.Count == 0)
                {
                    throw new Exception("No active or supported locators found in the current POM. Verify the POM configuration.");
                }

            }
            else
            {
                ElementLocator elementLocator = new ElementLocator
                {
                    LocateBy = act.ElementLocateBy,
                    LocateValue = act.ElementLocateValueForDriver
                };
                activeSupportedLocators.Add(elementLocator);
            }

            return activeSupportedLocators;
        }

        /// <summary>
        /// Retrieves the appropriate Selenium By object based on the provided locateBy and locateValue.
        /// </summary>
        /// <param name="locateBy">The eLocateBy value representing the type of locator.</param>
        /// <param name="locateValue">The value of the locator.</param>
        /// <returns>The Selenium By object representing the locator.</returns>
        internal static By GetElementLocatorForWebSmartSync(eLocateBy locateBy, string locateValue)
        {
            By elementLocator = locateBy switch
            {
                eLocateBy.ByXPath or eLocateBy.ByRelXPath => By.XPath(locateValue),
                eLocateBy.ByID => By.Id(locateValue),
                eLocateBy.ByName => By.Name(locateValue),
                eLocateBy.ByClassName => By.ClassName(locateValue),
                eLocateBy.ByCSSSelector => By.CssSelector(locateValue),
                eLocateBy.ByLinkText => By.LinkText(locateValue),
                eLocateBy.ByTagName => By.TagName(locateValue),
                _ => throw new Exception("Unsupported locator type. Supported locator types include: ByXPath, ByID, ByName, ByClassName, ByCssSelector, ByLinkText, ByRelativeXpath, and ByTagName."),
            };
            return elementLocator;
        }

        /// <summary>
        /// Waits for the specified synchronization operation to complete using the provided elementLocator.
        /// </summary>
        /// <param name="act">The ActWebSmartSync action containing the synchronization details.</param>
        /// <param name="elementLocator">The Selenium By object representing the locator.</param>
        /// <param name="VE">The ValueExpression object used to evaluate dynamic values.</param>
        /// <param name="wait">The WebDriverWait object used for waiting.</param>
        internal void WebSmartSyncWaitForLocator(ActWebSmartSync act, By elementLocator, ValueExpression VE, WebDriverWait wait)
        {
            switch (act.SyncOperations)
            {
                case ActWebSmartSync.eSyncOperation.ElementIsVisible:
                    wait.Until(ExpectedConditions.ElementIsVisible(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.ElementExists:
                    wait.Until(ExpectedConditions.ElementExists(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.AlertIsPresent:
                    //no need for locators
                    wait.Until(ExpectedConditions.AlertIsPresent());
                    break;
                case ActWebSmartSync.eSyncOperation.ElementIsSelected:
                    wait.Until(ExpectedConditions.ElementIsSelected(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.PageHasBeenLoaded:
                    wait.Until(ExpectedConditions.PageHasBeenLoaded());
                    break;
                case ActWebSmartSync.eSyncOperation.ElementToBeClickable:
                    wait.Until(ExpectedConditions.ElementToBeClickable(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.TextMatches:
                    VE.Value = act.TxtMatchInput;
                    string textToMatch = VE.ValueCalculated;
                    if (String.IsNullOrEmpty(textToMatch))
                    {
                        throw new InvalidDataException("For TextMatches operation,The input value is missing or invalid input.");
                    }
                    wait.Until(ExpectedConditions.TextMatches(elementLocator, textToMatch));
                    break;
                case ActWebSmartSync.eSyncOperation.AttributeMatches:
                    VE.Value = act.AttributeName;
                    string attributeName = VE.ValueCalculated;
                    VE = new ValueExpression(GetCurrentProjectEnvironment(), this.BusinessFlow)
                    {
                        Value = act.AttributeValue
                    };
                    string attributeValue = VE.ValueCalculated;
                    if (string.IsNullOrEmpty(attributeValue) || string.IsNullOrEmpty(attributeName))
                    {
                        throw new InvalidDataException("For AttributeMatches operation,The input value is missing or invalid input.");
                    }
                    wait.Until(ExpectedConditions.AttributeMatches(elementLocator, attributeName, attributeValue));
                    break;
                case ActWebSmartSync.eSyncOperation.EnabilityOfAllElementsLocatedBy:
                    wait.Until(ExpectedConditions.EnabilityOfAllElementsLocatedBy(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.FrameToBeAvailableAndSwitchToIt:
                    wait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.InvisibilityOfAllElementsLocatedBy:
                    wait.Until(ExpectedConditions.InvisibilityOfAllElementsLocatedBy(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.InvisibilityOfElementLocated:
                    wait.Until(ExpectedConditions.InvisibilityOfElementLocated(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.PresenceOfAllElementsLocatedBy:
                    wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.SelectedOfAllElementsLocatedBy:
                    wait.Until(ExpectedConditions.SelectedOfAllElementsLocatedBy(elementLocator));
                    break;
                case ActWebSmartSync.eSyncOperation.UrlMatches:
                    VE.Value = act.UrlMatches;
                    string urlMatches = VE.ValueCalculated;
                    if (String.IsNullOrEmpty(urlMatches))
                    {
                        throw new InvalidDataException("For UrlMatches operation,The input value is missing or invalid input.");
                    }
                    wait.Until(ExpectedConditions.UrlMatches(urlMatches));
                    break;
                case ActWebSmartSync.eSyncOperation.VisibilityOfAllElementsLocatedBy:
                    wait.Until(ExpectedConditions.VisibilityOfAllElementsLocatedBy(elementLocator));
                    break;
                default:
                    act.Error = "Unsupported operation.";
                    break;
            }
        }

        /// <summary>
        /// Handles the synchronization of web elements using various synchronization operations.
        /// </summary>
        /// <param name="act">The ActWebSmartSync action containing the synchronization details.</param>
        public void WebSmartSyncHandler(ActWebSmartSync act)
        {
            List<ElementLocator> locatorList = [];
            By elementLocator = null;
            try
            {
                if (!operationsWithoutLocator.Contains(act.SyncOperations))
                {
                    locatorList = GetLocatorsForWebSmartSync(act, new POMExecutionUtils(act, act.ElementLocateValue));

                    if (act.ElementLocateBy != eLocateBy.POMElement && string.IsNullOrEmpty(locatorList[0].LocateValue))
                    {
                        throw new Exception($"For {act.SyncOperations} operation Locate value is missing or invalid input.");
                    }
                }
            }
            catch (Exception ex)
            {
                act.Error = ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                return;
            }
            int MaxTimeout = WebSmartSyncGetMaxTimeout(act);
            //store agent's implicit wait in a variable
            int implicitWait = (int)Driver.Manage().Timeouts().ImplicitWait.TotalSeconds;
            //set agent's implicit wait to 1 second
            Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(1));
            WebDriverWait wait;
            ValueExpression VE = new(GetCurrentProjectEnvironment(), this.BusinessFlow);
            if (act.UseAllLocators && act.ElementLocateBy == eLocateBy.POMElement)
            {
                for (int i = 0; i < locatorList.Count; i++)
                {
                    try
                    {
                        wait = new(Driver, TimeSpan.FromSeconds(MaxTimeout))
                        {
                            PollingInterval = TimeSpan.FromMilliseconds(500)
                        };
                        elementLocator = GetElementLocatorForWebSmartSync(locatorList[i].LocateBy, locatorList[i].LocateValue);
                        WebSmartSyncWaitForLocator(act, elementLocator, VE, wait);
                        break;
                    }
                    catch (WebDriverTimeoutException ex)
                    {
                        if (i == locatorList.Count - 1)
                        {
                            act.Error = $"{act.SyncOperations} was not completed within the allotted time or Unable to locate element.";
                            Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                            break;
                        }
                        continue;
                    }
                    catch (InvalidSelectorException ex)
                    {
                        act.Error = $"Invalid input provided for {act.SyncOperations} operation.";
                        Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                        break;
                    }
                    catch (InvalidDataException ex)
                    {
                        act.Error = $"Invalid input provided for {act.SyncOperations} operation.";
                        Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        act.Error = $"unexpected error occured!";
                        Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                    }
                    finally
                    {
                        //set agent's implicit wait to the original value from the variable above
                        Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(implicitWait));
                    }
                }
            }
            else
            {
                try
                {
                    wait = new(Driver, TimeSpan.FromSeconds(MaxTimeout))
                    {
                        PollingInterval = TimeSpan.FromMilliseconds(500)
                    };
                    if (!operationsWithoutLocator.Contains(act.SyncOperations))
                        elementLocator = GetElementLocatorForWebSmartSync(locatorList[0].LocateBy, locatorList[0].LocateValue);
                    WebSmartSyncWaitForLocator(act, elementLocator, VE, wait);
                }
                catch (InvalidSelectorException ex)
                {
                    act.Error = $"Invalid input provided for {act.SyncOperations} operation.";
                    Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                }
                catch (WebDriverTimeoutException ex)
                {
                    act.Error = $"{act.SyncOperations} was not completed within the allotted {MaxTimeout} seconds.";
                    Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                }
                catch (Exception ex)
                {
                    act.Error = $"unexpected error occured!";
                    Reporter.ToLog(eLogLevel.ERROR, act.Error, ex);
                }
                finally
                {
                    //set agent's implicit wait to the original value from the variable above
                    Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(implicitWait));
                }
            }

        }



        /// <summary>
        /// Retrieves the maximum timeout value for the ActWebSmartSync action.
        /// </summary>
        /// <param name="act">The ActWebSmartSync action containing the synchronization details.</param>
        /// <returns>The maximum timeout value in seconds.</returns>
        internal int WebSmartSyncGetMaxTimeout(ActWebSmartSync act)
        {
            if (act.Timeout > 0)
            {
                return act.Timeout.GetValueOrDefault();
            }
            else
            {
                return ImplicitWait;
            }

        }

        public void SmartSyncHandler(ActSmartSync act)
        {
            int MaxTimeout = GetMaxTimeout(act);

            try
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, MaxTimeout);
                IWebElement e = null;

                Stopwatch st = new Stopwatch();

                st.Reset();
                st.Start();

                switch (act.SmartSyncAction)
                {
                    case ActSmartSync.eSmartSyncAction.WaitUntilDisplay:
                        do
                        {
                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisplay is timeout";
                                break;
                            }

                            Thread.Sleep(100);

                            e = LocateElement(act, true);

                        } while (!(e != null && (e.Displayed || e.Enabled)));
                        break;

                    case ActSmartSync.eSmartSyncAction.WaitUntilDisapear:
                        do
                        {
                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisapear is timeout";
                                break;
                            }

                            Thread.Sleep(100);

                            e = LocateElement(act, true);

                        } while (e != null && e.Displayed);
                        break;
                }

                st.Stop();
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
            }

        }

        public void PWLElementHandler(ActPWL act)
        {
            IWebElement e, e1;
            e = LocateElement(act);
            e1 = LocateElement(new ActPWL() { LocateBy = act.OLocateBy, LocateValue = act.OLocateValue, LocateValueCalculated = act.OLocateValue });

            switch (act.PWLAction)
            {
                case ActPWL.ePWLAction.GetHDistanceLeft2Left:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.X - e1.Location.X).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceLeft2Right:
                    e = LocateElement(act);
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.X - e1.Location.X - e1.Size.Width).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceRight2Right:
                    e = LocateElement(act);
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.X - e1.Location.X - e1.Size.Width + e.Size.Width).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetHDistanceRight2Left:
                    e = LocateElement(act);
                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.X - e1.Location.X + e.Size.Width).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceTop2Top:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.Y - e1.Location.Y).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceTop2Bottom:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", (Math.Abs(e.Location.Y - e1.Location.Y) + e1.Size.Height).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceBottom2Bottom:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.Y + e.Size.Height - e1.Location.Y - e1.Size.Height).ToString());
                    }
                    break;
                case ActPWL.ePWLAction.GetVDistanceBottom2Top:

                    if (e == null || e1 == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", Math.Abs(e.Location.X - e1.Location.X + e.Size.Height).ToString());
                    }
                    break;
            }
        }

        private void HandleActVisualTesting(ActVisualTesting act)
        {
            act.Execute(this);
        }

        public void GenElementHandler(ActGenElement act)
        {
            //TODO: make sure each action if err/exception update act.Errore
            //TODO: put each action in function
            IWebElement e;
            SelectElement se;

            switch (act.GenElementAction)
            {
                case ActGenElement.eGenElementAction.Click:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].click()", e);
                        }
                        catch (Exception)
                        {
                            e.Click();
                        }
                    }
                    break;

                case ActGenElement.eGenElementAction.SimpleClick:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        try
                        {
                            e.Click();
                        }
                        catch
                        {
                            try
                            {
                                e = LocateElement(act);
                                if (e == null)
                                {
                                    act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                                    return;
                                }
                                e.Click();
                            }
                            catch (Exception ex)
                            {
                                act.Error = "Error: " + ex.Message + " " + act.LocateBy + " " + act.LocateValue;
                            }
                        }
                    }
                    break;

                case ActGenElement.eGenElementAction.AsyncClick:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("var el=arguments[0]; setTimeout(function() { el.click(); }, 100);", e);
                        }
                        catch (Exception)
                        {
                            e.Click();
                        }
                    }
                    break;

                case ActGenElement.eGenElementAction.ClickAt:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Click().Build().Perform();
                    }
                    break;

                case ActGenElement.eGenElementAction.Focus:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        Boolean b = Driver.SwitchTo().ActiveElement().Equals(e);
                        act.AddOrUpdateReturnParamActual("Is Focused", b.ToString());
                    }
                    break;

                case ActGenElement.eGenElementAction.DeleteAllCookies:  //TODO: FIXME: This action should not be part of GenElement
                    Driver.Manage().Cookies.DeleteAllCookies();
                    break;
                case ActGenElement.eGenElementAction.GetWindowTitle: //TODO: FIXME: This action should not be part of GenElement
                    string title = Driver.Title;
                    if (!string.IsNullOrEmpty(title))
                    {
                        act.AddOrUpdateReturnParamActual("Actual", title);
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", "");
                    }

                    break;
                case ActGenElement.eGenElementAction.MouseClick:
                    e = LocateElement(act);
                    InputSimulator inp = new InputSimulator();//Oct/2020- Nuget was replaced to InputSimulatorStandard so need to test if still working as eexpected
                    inp.Mouse.MoveMouseTo(1.0, 1.0);
                    inp.Mouse.MoveMouseBy((int)((e.Location.X + 5) / 1.33), (int)((e.Location.Y + 5) / 1.33));
                    inp.Mouse.LeftButtonClick();
                    break;

                case ActGenElement.eGenElementAction.KeyboardInput:
                    e = LocateElement(act);

                    if (e != null)
                    {
                        e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.CloseBrowser: //TODO: FIXME: This action should not be part of GenElement
                    Driver.Close();
                    break;

                case ActGenElement.eGenElementAction.StartBrowser: //TODO: FIXME: This action should not be part of GenElement
                    if (this.IsRunning() == false)
                    {
                        this.StartDriver();
                        act.ExInfo = "Browser was started";
                    }
                    else
                    {
                        act.ExInfo = "Browser already running";
                    }
                    break;

                case ActGenElement.eGenElementAction.MsgBox: //TODO: FIXME: This action should not be part of GenElement
                    string msg = act.GetInputParamCalculatedValue("Value");
                    Reporter.ToUser(eUserMsgKey.ScriptPaused);
                    break;

                case ActGenElement.eGenElementAction.GetStyle:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        try
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                        }
                        catch
                        {
                            act.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                        }
                    }
                    break;

                case ActGenElement.eGenElementAction.GetHeight:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
                    }
                    break;

                case ActGenElement.eGenElementAction.GetWidth:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
                    }
                    break;

                case ActGenElement.eGenElementAction.XYClick:
                    MoveToElementActions(act);
                    break;

                case ActGenElement.eGenElementAction.XYDoubleClick:
                    MoveToElementActions(act);
                    break;

                case ActGenElement.eGenElementAction.XYSendKeys:
                    MoveToElementActions(act);
                    break;

                case ActGenElement.eGenElementAction.Visible:
                    e = LocateElement(act, true);
                    if (e == null)
                    {
                        act.ExInfo = "Element not found - " + act.LocateBy + " " + act.LocateValue;
                        act.AddOrUpdateReturnParamActual("Actual", "False");
                        return;
                    }
                    else
                    { act.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString()); }
                    break;

                case ActGenElement.eGenElementAction.Enabled:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        act.AddOrUpdateReturnParamActual("Enabled", "False");
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Enabled", e.Enabled.ToString());
                    }
                    break;

                case ActGenElement.eGenElementAction.SwitchWindow:
                    SwitchWindow(act);
                    break;

                case ActGenElement.eGenElementAction.DismissMessageBox:
                    try
                    {
                        Driver.SwitchTo().Alert().Dismiss();
                    }
                    catch (Exception ex)
                    {
                        act.Error = "Error: " + ex.Message;
                    }
                    break;

                case ActGenElement.eGenElementAction.AcceptMessageBox:
                    try
                    {
                        Driver.SwitchTo().Alert().Accept();
                    }
                    catch (Exception ex)
                    {
                        act.Error = "Error: " + ex.Message;
                    }
                    break;

                case ActGenElement.eGenElementAction.Hover:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Build().Perform();
                    }
                    break;

                case ActGenElement.eGenElementAction.DoubleClick:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.Click(e).Click(e).Build().Perform();
                    }
                    break;

                case ActGenElement.eGenElementAction.Doubleclick2:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.DoubleClick(e).Build().Perform();
                    }
                    break;

                case ActGenElement.eGenElementAction.RightClick:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.ContextClick(e).Build().Perform();
                    }
                    break;

                case ActGenElement.eGenElementAction.SwitchFrame: //TODO: FIXME: This action should not be part of GenElement
                    HandleSwitchFrame(act);
                    break;

                case ActGenElement.eGenElementAction.SwitchToDefaultFrame: //TODO: FIXME: This action should not be part of GenElement
                    Driver.SwitchTo().DefaultContent();
                    break;

                case ActGenElement.eGenElementAction.SwitchToParentFrame: //TODO: FIXME: This action should not be part of GenElement
                    Driver.SwitchTo().ParentFrame();
                    break;

                case ActGenElement.eGenElementAction.GetValue:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        try
                        {
                            OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                            action.MoveToElement(e).Build().Perform();
                            act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("value"));
                            if (act.GetReturnParam("Actual") == null)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", e.Text);
                            }
                        }
                        // TODO: its a workaround when running from firefox to handle an exception(https://github.com/nightwatchjs/nightwatch/issues/1272). Need to remove 
                        catch
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.Text);
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.Disabled:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        return;
                    }

                    if ((e.Displayed && e.Enabled))
                    {
                        act.AddOrUpdateReturnParamActual("Actual", "False");
                        act.ExInfo = "Element displayed property is " + e.Displayed + "Element Enabled property is:" + e.Enabled;
                        return;
                    }
                    else
                    {
                        act.AddOrUpdateReturnParamActual("Actual", "true");
                    }
                    break;
                case ActGenElement.eGenElementAction.GetInnerText:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Build().Perform();
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("textContent"));
                        if (act.GetReturnParam("Actual") == null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("innerText"));
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.SelectFromDropDown:
                    //TODO: do it better without the fail, let the user decide based on what to select
                    e = LocateElement(act);
                    if (e != null)
                    {
                        se = null;
                        try
                        {
                            se = new SelectElement(e);
                            se.SelectByText(act.GetInputParamCalculatedValue("Value"));
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing SelectFromDropDown operation", ex);
                            try
                            {
                                se.SelectByValue(act.GetInputParamCalculatedValue("Value"));
                            }
                            catch (Exception ex2)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing SelectFromDropDown operation", ex2);
                                try
                                {
                                    se.SelectByIndex(Convert.ToInt32(act.GetInputParamCalculatedValue("Value")));
                                }
                                catch (Exception ex3)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing SelectFromDropDown operation", ex3);
                                }
                            }
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.AsyncSelectFromDropDownByIndex:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        string value = act.GetInputParamCalculatedValue("Value");
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("var el=arguments[0], val=arguments[1]; setTimeout(function() { el.selectedIndex = val; }, 100);", e, value);
                        }
                        catch (Exception ex3)
                        {
                            act.Error = "Error: Failed to select the value ' + " + value + "' for the object - " + act.LocateBy + " " + act.LocateValue;
                            Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing AsyncSelectFromDropDownByIndex operation", ex3);
                            return;
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    break;


                case ActGenElement.eGenElementAction.SelectFromDijitList: //TODO: FIXME: This action should not be part of GenElement
                    try
                    {
                        ((IJavaScriptExecutor)Driver).ExecuteScript("dijit.byId('" + act.LocateValue + "').set('value','" + act.GetInputParamCalculatedValue("Value") + "')");
                    }
                    catch (Exception ex)
                    {
                        act.Error = "Error: Failed to select value using digit from object with ID: '" + act.LocateValue + "' and Value: '" + act.GetInputParamCalculatedValue("Value") + "'";
                        Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing SelectFromDijitList operation", ex);
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.GotoURL:
                    //TODO: FIXME: This action should not be part of GenElement
                    GotoURL(act, act.GetInputParamCalculatedValue("Value"));
                    break;

                case ActGenElement.eGenElementAction.SetValue:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        if (e.TagName == "select")
                        {
                            SelectElement combobox = new SelectElement(e);
                            string val = act.GetInputParamCalculatedValue("Value");
                            combobox.SelectByText(val);
                            act.ExInfo += "Selected Value - " + val;
                            return;
                        }
                        if (e.TagName == "input" && e.GetAttribute("type") == "checkbox")
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('checked',arguments[1])", e, act.ValueForDriver);
                            return;
                        }

                        //Special case for FF 
                        if (Driver.GetType() == typeof(FirefoxDriver) && e.TagName == "input" && e.GetAttribute("type") == "text")
                        {
                            e.Clear();
                            try
                            {
                                e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                            }
                            catch (InvalidOperationException ex)
                            {
                                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while performing SetValue operation", ex);
                            }
                        }
                        else
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.SendKeys:
                    e = LocateElement(act);
                    if (e != null)
                    {

                        e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));

                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.SelectFromListScr:
                    List<IWebElement> els = LocateElements(act.LocateBy, act.LocateValueCalculated);
                    if (els != null)
                    {
                        try
                        {
                            els[Convert.ToInt32(act.GetInputParamCalculatedValue("Value"))].Click();
                        }
                        catch (Exception)
                        {
                            act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        }
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.GetNumberOfElements:
                    try
                    {
                        List<IWebElement> elements = LocateElements(act.LocateBy, act.LocateValueCalculated);
                        if (elements != null)
                        {
                            act.AddOrUpdateReturnParamActual("Elements Count", elements.Count.ToString());
                        }
                        else
                        {
                            act.AddOrUpdateReturnParamActual("Elements Count", "0");
                        }
                    }
                    catch (Exception ex)
                    {
                        act.Error = "Failed to count number of elements for - " + act.LocateBy + " " + act.LocateValueCalculated;
                        act.ExInfo = ex.Message;
                    }
                    break;
                case ActGenElement.eGenElementAction.BatchClicks:
                    List<IWebElement> eles = LocateElements(act.LocateBy, act.LocateValueCalculated);

                    if (eles != null)
                    {
                        try
                        {
                            foreach (IWebElement el in eles)
                            {
                                el.Click();
                                Thread.Sleep(2000);
                            }
                        }
                        catch (Exception)
                        {
                            act.Error = "One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        }
                    }
                    else
                    {
                        act.Error = "Error: One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.BatchSetValues:

                    List<IWebElement> textels = LocateElements(act.LocateBy, act.LocateValueCalculated);

                    if (textels != null)
                    {
                        try
                        {
                            foreach (IWebElement el in textels)
                            {
                                el.Clear();
                                el.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                Thread.Sleep(2000);
                            }
                        }
                        catch (Exception)
                        {
                            act.Error = "Error: One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        }
                    }
                    else
                    {
                        act.Error = "Error: One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.Wait:
                    try
                    {
                        int number = Int32.Parse(act.GetInputParamCalculatedValue("Value"));
                        Thread.Sleep(number * 1000);
                    }
                    catch (FormatException)
                    {
                        //TODO: give message to user in grid
                    }
                    catch (OverflowException)
                    {
                        //TODO: give message to user in grid
                    }
                    break;

                case ActGenElement.eGenElementAction.KeyType:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        e.Clear();
                        e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.Back:
                    Driver.Navigate().Back();
                    break;

                case ActGenElement.eGenElementAction.Refresh:
                    Driver.Navigate().Refresh();
                    break;

                case ActGenElement.eGenElementAction.GetCustomAttribute:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Build().Perform();
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute(act.Value));
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                case ActGenElement.eGenElementAction.ScrollToElement:
                    e = LocateElement(act);
                    if (e == null)
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else
                    {
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", e);
                        }
                        catch (Exception)
                        {
                            act.Error = "Error: Failed to scroll to element - " + act.LocateBy + " " + act.LocateValue;
                        }
                    }
                    break;

                case ActGenElement.eGenElementAction.RunJavaScript:
                    string script = act.GetInputParamCalculatedValue("Value");
                    try
                    {
                        object a = null;
                        if (!script.ToUpper().StartsWith("RETURN"))
                        {
                            script = "return " + script;
                        }
                        a = ((IJavaScriptExecutor)Driver).ExecuteScript(script);
                        if (a != null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", a.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        act.Error = "Error: Failed to run the JavaScript: '" + script + "', Error: '" + ex.Message + "'";
                    }
                    break;

                case ActGenElement.eGenElementAction.GetElementAttributeValue:
                    e = LocateElement(act);
                    if (e != null)
                    {
                        string val = e.GetAttribute(act.ValueForDriver);
                        act.AddOrUpdateReturnParamActual("Actual", val);
                    }
                    else
                    {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;
                case ActGenElement.eGenElementAction.SetAttributeUsingJs:
                {
                    e = LocateElement(act);
                    char[] delimit = new char[] { '=' };
                    string insertval = act.GetInputParamCalculatedValue("Value");
                    string[] vals = insertval.Split(delimit, 2);
                    if (vals.Length != 2)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, @"Input string should be in the format : attribute=value");
                        return;
                    }
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0]." + vals[0] + "=arguments[1]", e, vals[1]);
                }
                break;
                default:
                    Reporter.ToLog(eLogLevel.DEBUG, "Action unknown/not implemented for the Driver: " + this.GetType().ToString());
                    break;

            }
        }
        private string EscapeCssAttributeValue(string value)
        {
            // Escape single quotes by replacing with escaped version
            return value?.Replace("'", "\\'") ?? string.Empty;
        }
        private void MoveToElementActions(ActGenElement act)
        {
            IWebElement e = LocateElement(act, true);
            int x = 0;
            int y = 0;
            if (!Int32.TryParse(act.GetOrCreateInputParam(ActGenElement.Fields.Xoffset).ValueForDriver, out x) || !Int32.TryParse(act.GetOrCreateInputParam(ActGenElement.Fields.Yoffset).ValueForDriver, out y))
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.ExInfo = "Cannot Click by XY with String Value, X Value: " + act.GetOrCreateInputParam(ActGenElement.Fields.Xoffset).ValueForDriver + ", Y Value: " + act.GetOrCreateInputParam(ActGenElement.Fields.Yoffset).ValueForDriver + "  ";
            }
            if (e == null)
            {
                act.ExInfo += "Element not found - " + act.LocateBy + " " + act.LocateValue;
                act.AddOrUpdateReturnParamActual("Actual", "False");
                return;
            }
            else
            {
                switch (act.GenElementAction)
                {
                    case ActGenElement.eGenElementAction.XYClick:
                        ClickXY(e, x, y);
                        break;
                    case ActGenElement.eGenElementAction.XYSendKeys:
                        SendKeysXY(e, x, y, GetKeyName(act.GetInputParamCalculatedValue("Value")));
                        break;
                    case ActGenElement.eGenElementAction.XYDoubleClick:
                        DoubleClickXY(e, x, y);
                        break;
                }
            }
        }

        private void MoveToElementActions(ActUIElement act)
        {
            IWebElement e = LocateElement(act, true);
            int x = 0;
            int y = 0;
            if (!Int32.TryParse(act.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate).ValueForDriver, out x) || !Int32.TryParse(act.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate).ValueForDriver, out y))
            {
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                act.ExInfo = "Cannot Click by XY with String Value, X Value: " + act.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate).ValueForDriver + ", Y Value: " + act.GetOrCreateInputParam(ActUIElement.Fields.YCoordinate).ValueForDriver + "  ";
            }
            if (e == null)
            {
                act.ExInfo += "Element not found - " + act.LocateBy + " " + act.LocateValue;
                act.AddOrUpdateReturnParamActual("Actual", "False");
                return;
            }
            else
            {
                switch (act.ElementAction)
                {
                    case ActUIElement.eElementAction.ClickXY:
                        ClickXY(e, x, y);
                        break;
                    case ActUIElement.eElementAction.SendKeysXY:
                        SendKeysXY(e, x, y, GetKeyName(act.GetInputParamCalculatedValue("Value")));
                        break;
                    case ActUIElement.eElementAction.DoubleClickXY:
                        DoubleClickXY(e, x, y);
                        break;
                }
            }
        }

        private void ClickXY(IWebElement iwe, int x, int y)
        {
            OpenQA.Selenium.Interactions.Actions actionClick = new OpenQA.Selenium.Interactions.Actions(Driver);
            actionClick.MoveToElement(iwe, x, y).Click().Build().Perform();
        }
        private void SendKeysXY(IWebElement iwe, int x, int y, string Value)
        {
            OpenQA.Selenium.Interactions.Actions actionSetValue = new OpenQA.Selenium.Interactions.Actions(Driver);
            actionSetValue.MoveToElement(iwe, x, y).SendKeys(Value).Build().Perform();
        }
        private void DoubleClickXY(IWebElement iwe, int x, int y)
        {
            OpenQA.Selenium.Interactions.Actions actionDoubleClick = new OpenQA.Selenium.Interactions.Actions(Driver);
            actionDoubleClick.MoveToElement(iwe, x, y).DoubleClick().Build().Perform();
        }

        private void ActCheckboxHandler(ActCheckbox actCheckbox)
        {
            IWebElement e = LocateElement(actCheckbox);
            if (e == null || e.Displayed == false)
            {
                actCheckbox.Error = "Error: Element not found - " + actCheckbox.LocateBy + " " + actCheckbox.LocateValue;
                return;
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.Check)
            {
                if (e.Selected == false)
                {
                    e.Click();
                }
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.IsDisabled)
            {
                actCheckbox.AddOrUpdateReturnParamActual("Actual", (!e.Enabled).ToString());
            }

            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.Uncheck)
            {
                if (e.Selected == true)
                {
                    e.Click();
                }
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.Click)
            {
                e.Click();
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.GetValue)
            {
                actCheckbox.AddOrUpdateReturnParamActual("Actual", e.Selected.ToString());
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.IsDisplayed)
            {
                actCheckbox.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
            }
            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.GetStyle)
            {
                try
                {
                    actCheckbox.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                }
                catch
                {
                    actCheckbox.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                }
            }

            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.GetHeight)
            {
                actCheckbox.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
            }

            if (actCheckbox.CheckboxAction == ActCheckbox.eCheckboxAction.GetWidth)
            {
                actCheckbox.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
            }
        }

        private void ActDropDownListHandler(ActDropDownList dd)
        {
            try
            {
                IWebElement e = LocateElement(dd);
                if (e == null)
                {
                    return;
                }

                SelectElement se = new(e);
                switch (dd.ActDropDownListAction)
                {
                    case ActDropDownList.eActDropDownListAction.SetSelectedValueByValue:
                        SelectDropDownListOptionByValue(dd, dd.GetInputParamCalculatedValue("Value"), se);
                        break;
                    case ActDropDownList.eActDropDownListAction.GetValidValues:
                        GetDropDownListOptions(dd, e);
                        break;
                    case ActDropDownList.eActDropDownListAction.SetSelectedValueByText:
                        SelectDropDownListOptionByText(dd, dd.GetInputParamCalculatedValue("Value"), e);
                        break;
                    case ActDropDownList.eActDropDownListAction.SetSelectedValueByIndex:
                        SelectDropDownListOptionByIndex(dd, Int32.Parse(dd.GetInputParamCalculatedValue("Value")), se);
                        break;
                    case ActDropDownList.eActDropDownListAction.GetSelectedValue:
                        dd.AddOrUpdateReturnParamActual("Actual", se.SelectedOption.Text);
                        break;
                    case ActDropDownList.eActDropDownListAction.IsPrepopulated:
                        dd.AddOrUpdateReturnParamActual("Actual", (se.SelectedOption.ToString().Trim() != "").ToString());
                        break;
                    case ActDropDownList.eActDropDownListAction.GetFont:
                        dd.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("font"));
                        break;
                    case ActDropDownList.eActDropDownListAction.GetHeight:
                        dd.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
                        break;
                    case ActDropDownList.eActDropDownListAction.GetWidth:
                        dd.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
                        break;
                    case ActDropDownList.eActDropDownListAction.GetStyle:
                        try { dd.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style")); }
                        catch { dd.AddOrUpdateReturnParamActual("Actual", "no such attribute"); }
                        break;
                    case ActDropDownList.eActDropDownListAction.SetFocus:
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Build().Perform();
                        break;
                }
            }
            catch (System.ArgumentException ae)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred in ActDropDownListHandler", ae);
                return;
            }
        }

        private string GetKeyName(string skey)
        {
            return skey switch
            {
                "Keys.Alt" => Keys.Alt,
                "Keys.ArrowDown" => Keys.ArrowDown,
                "Keys.ArrowLeft" => Keys.ArrowLeft,
                "Keys.ArrowRight" => Keys.ArrowRight,
                "Keys.ArrowUp" => Keys.ArrowUp,
                "Keys.Backspace" => Keys.Backspace,
                "Keys.Cancel" => Keys.Cancel,
                "Keys.Clear" => Keys.Clear,
                "Keys.Command" => Keys.Command,
                "Keys.Control" => Keys.Control,
                "Keys.Decimal" => Keys.Decimal,
                "Keys.Delete" => Keys.Delete,
                "Keys.Divide" => Keys.Divide,
                "Keys.Down" => Keys.Down,
                "Keys.End" => Keys.End,
                "Keys.Enter" => Keys.Enter,
                "Keys.Equal" => Keys.Equal,
                "Keys.Escape" => Keys.Escape,
                "Keys.F1" => Keys.F1,
                "Keys.F10" => Keys.F10,
                "Keys.F11" => Keys.F11,
                "Keys.F12" => Keys.F12,
                "Keys.F2" => Keys.F2,
                "Keys.F3" => Keys.F3,
                "Keys.F4" => Keys.F4,
                "Keys.F5" => Keys.F5,
                "Keys.F6" => Keys.F6,
                "Keys.F7" => Keys.F7,
                "Keys.F8" => Keys.F8,
                "Keys.F9" => Keys.F9,
                "Keys.Help" => Keys.Help,
                "Keys.Home" => Keys.Home,
                "Keys.Insert" => Keys.Insert,
                "Keys.Left" => Keys.Left,
                "Keys.LeftAlt" => Keys.LeftAlt,
                "Keys.LeftControl" => Keys.LeftControl,
                "Keys.LeftShift" => Keys.LeftShift,
                "Keys.Meta" => Keys.Meta,
                "Keys.Multiply" => Keys.Multiply,
                "Keys.Null" => Keys.Null,
                "Keys.NumberPad0" => Keys.NumberPad0,
                "Keys.NumberPad1" => Keys.NumberPad1,
                "Keys.NumberPad2" => Keys.NumberPad2,
                "Keys.NumberPad3" => Keys.NumberPad3,
                "Keys.NumberPad4" => Keys.NumberPad4,
                "Keys.NumberPad5" => Keys.NumberPad5,
                "Keys.NumberPad6" => Keys.NumberPad6,
                "Keys.NumberPad7" => Keys.NumberPad7,
                "Keys.NumberPad8" => Keys.NumberPad8,
                "Keys.NumberPad9" => Keys.NumberPad9,
                "Keys.PageDown" => Keys.PageDown,
                "Keys.PageUp" => Keys.PageUp,
                "Keys.Pause" => Keys.Pause,
                "Keys.Return" => Keys.Return,
                "Keys.Right" => Keys.Right,
                "Keys.Semicolon" => Keys.Semicolon,
                "Keys.Separator" => Keys.Separator,
                "Keys.Shift" => Keys.Shift,
                "Keys.Space" => Keys.Space,
                "Keys.Subtract" => Keys.Subtract,
                "Keys.Tab" => Keys.Tab,
                "Keys.Up" => Keys.Up,
                _ => skey,
            };
        }
        private void ActRadioButtonHandler(ActRadioButton actRadioButton)
        {
            string cssSelectorVal = "input[type='radio'][" + actRadioButton.LocateValue + "]";
            IWebElement e = Driver.FindElement(By.CssSelector(cssSelectorVal));

            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetValue)
            {
                if (Driver.FindElement(By.CssSelector(cssSelectorVal)).Selected)
                {
                    actRadioButton.AddOrUpdateReturnParamActual("Actual", Driver.FindElement(By.CssSelector(cssSelectorVal)).GetAttribute("value") + "");
                }
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.IsDisabled)
            {
                if (Driver.FindElement(By.CssSelector(cssSelectorVal)).Selected)
                {
                    actRadioButton.AddOrUpdateReturnParamActual("Actual", Driver.FindElement(By.CssSelector(cssSelectorVal)).GetAttribute("Disabled") + "");
                }
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.SelectByIndex)
            {
                SelectRadioButtonByIndex(actRadioButton, Int32.Parse(actRadioButton.GetInputParamCalculatedValue("Value")));
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.SelectByValue)
            {
                SelectRadioButtonByValue(actRadioButton, actRadioButton.GetInputParamCalculatedValue("Value"));
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.IsDisplayed)
            {
                if (Driver.FindElement(By.CssSelector(cssSelectorVal)) != null)
                {
                    actRadioButton.AddOrUpdateReturnParamActual("Actual", Driver.FindElement(By.CssSelector(cssSelectorVal)).Displayed.ToString());
                }
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetAvailableValues)
            {
                string aValues = "";
                foreach (IWebElement elm in Driver.FindElements(By.CssSelector(cssSelectorVal)))
                {
                    if (elm != null)
                    {
                        aValues = elm.GetAttribute("value") + "|" + aValues;
                    }
                }
                actRadioButton.AddOrUpdateReturnParamActual("Actual", aValues);
            }
            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetStyle)
            {
                try
                {
                    actRadioButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                }
                catch
                {
                    actRadioButton.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                }
            }

            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetHeight)
            {
                actRadioButton.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
            }

            if (actRadioButton.RadioButtonAction == ActRadioButton.eActRadioButtonAction.GetWidth)
            {
                actRadioButton.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
            }
        }

        private void ActButtonHandler(ActButton actButton)
        {
            IWebElement e = LocateElement(actButton);
            if (e == null)
            {
                actButton.Error = $"Error: Element not found - {actButton.LocateBy} {actButton.LocateValue}";
                return;
            }

            switch (actButton.ButtonAction)
            {
                case ActButton.eButtonAction.GetValue:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("Value"));
                    break;
                case ActButton.eButtonAction.IsDisabled:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("Disabled"));
                    break;
                case ActButton.eButtonAction.GetFont:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("font"));
                    break;
                case ActButton.eButtonAction.IsDisplayed:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
                    break;
                case ActButton.eButtonAction.GetStyle:
                    try
                    {
                        actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                    }
                    catch
                    {
                        actButton.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                    }
                    break;
                case ActButton.eButtonAction.GetHeight:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
                    break;
                case ActButton.eButtonAction.GetWidth:
                    actButton.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
                    break;
                default:
                    ClickButton(actButton);
                    break;
            }
        }

        private void ActLinkHandler(ActLink actLink)
        {
            IWebElement e = LocateElement(actLink);
            if (e == null || e.Displayed == false)
            {
                actLink.Error = "Error: Element not found - " + actLink.LocateBy + " " + actLink.LocateValue;
                return;
            }

            if (actLink.LinkAction == ActLink.eLinkAction.Click)
            {
                try
                {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].click()", e);
                }
                catch (Exception)
                {
                    e.Click();
                }
            }

            if (actLink.LinkAction == ActLink.eLinkAction.GetValue)
            {
                try
                {
                    if (e != null)
                    {
                        actLink.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("href"));
                    }
                    else
                    {
                        actLink.AddOrUpdateReturnParamActual("Actual", "");
                    }
                }
                catch (Exception)
                { }

            }

            if (actLink.LinkAction == ActLink.eLinkAction.Visible)
            {
                try
                {
                    if (e != null)
                    {
                        actLink.AddOrUpdateReturnParamActual("Actual", e.Displayed + "");
                    }
                }
                catch (Exception)
                { }
            }

            if (actLink.LinkAction == ActLink.eLinkAction.Hover)
            {
                HoverOverLink(actLink);
            }

            if (actLink.LinkAction == ActLink.eLinkAction.GetStyle)
            {
                try
                {
                    actLink.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                }
                catch
                {
                    actLink.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                }
            }

            if (actLink.LinkAction == ActLink.eLinkAction.GetHeight)
            {
                actLink.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
            }

            if (actLink.LinkAction == ActLink.eLinkAction.GetWidth)
            {
                actLink.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
            }

        }
        private void ClickButton(ActButton Button)
        {
            IWebElement e = LocateElement(Button);
            if (e != null)
            {
                try
                {
                    try
                    {
                        ((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].click()", e);
                    }
                    catch (Exception)
                    {
                        e.Click();
                    }
                }
                catch (OpenQA.Selenium.NoSuchElementException)
                {
                    /* not sure what causes this */
                    Reporter.ToLog(eLogLevel.ERROR, $"Error: Element not found - {Button.LocateBy} {Button.LocateValue}");
                }
            }
            else
            {
                Button.Error = "Error: Element not found - " + Button.LocateBy + " " + Button.LocateValue;
            }

            return;
        }


        #region MultiselectList methods

        private void DeSelectMultiselectListOptions(ActMultiselectList l)
        {
            IWebElement e = LocateElement(l);
            if (e == null)
            {
                l.Error = "Error: Element not found - ";
                return;
            }
            SelectElement se = new SelectElement(e);
            se.DeselectAll();
        }

        private void SelectMultiselectListOptionsByIndex(ActMultiselectList l, List<int> vals)
        {
            foreach (int v in vals)
            {
                IWebElement e = LocateElement(l);
                if (e == null)
                {
                    l.Error = "Error: Element not found - " + l.LocateBy + " " + l.LocateValue;
                    return;
                }
                SelectElement se = new SelectElement(e);
                se.SelectByIndex(v);
            }
        }

        private void SelectMultiselectListOptionsByText(ActMultiselectList l, List<string> vals)
        {
            foreach (string v in vals)
            {
                IWebElement e = LocateElement(l);
                if (e == null)
                {
                    l.Error = "Error: Element not found - " + l.LocateBy + " " + l.LocateValue;
                    return;
                }
                SelectElement se = new SelectElement(e);
                se.SelectByText(v);
            }
        }

        private void SelectMultiselectListOptionsByValue(ActMultiselectList l, List<string> vals)
        {
            foreach (string v in vals)
            {
                IWebElement e = LocateElement(l);
                if (e == null)
                {
                    l.Error = "Error: Element not found - " + l.LocateBy + " " + l.LocateValue;
                    return;
                }
                SelectElement se = new SelectElement(e);
                se.SelectByValue(v);
            }
        }
        #endregion //MultiselectList methods

        #region Radio Button methods

        private void SelectRadioButtonByIndex(ActRadioButton rb, int selectedIndex)
        {
            string cssSelectorVal = "input[type='radio'][" + selectedIndex.ToString() + "]";
            if (!Driver.FindElement(By.CssSelector(cssSelectorVal)).Selected)
            {
                Driver.FindElement(By.CssSelector(cssSelectorVal)).Click();
            }
        }

        //TODO: Can radio buttons that aren't accompanied by labels be selected by text? 
        //private void SelectRadioButtonByText(ActRadioButton rb, string val)
        //{
        //    string cssSelectorVal = "input[id='" + rb.Value + "'][type='radio']";
        //    List<IWebElement> RBs = LocateRadioButtonElements(rb.LocateBy, rb.LocateValue);
        //    for (int i = 0; i < RBs.Count; i++)
        //    {
        //        if (RBs[i].Text == val)
        //        {
        //            RBs[i].Click();
        //            i = RBs.Count;
        //        }
        //    }
        //}

        private void SelectRadioButtonByValue(ActRadioButton rb, string val)
        {
            string cssSelectorVal = "input[value='" + val + "'][type='radio']";
            if (!Driver.FindElement(By.CssSelector(cssSelectorVal)).Selected)
            {
                Driver.FindElement(By.CssSelector(cssSelectorVal)).Click();
            }
        }
        #endregion // Radio Button methods

        #region DropDownList methods
        private void SelectDropDownListOptionByIndex(Act dd, int i, SelectElement se)
        {
            se.SelectByIndex(i);
        }
        private void SelectDropDownListOptionByText(Act dd, string s, IWebElement e)
        {
            ElementScrollIntoView(e);
            SelectElement se = new SelectElement(e);
            se.SelectByText(s);
        }

        private void ElementScrollIntoView(IWebElement e)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", e);
            //General.DoEvents();
        }

        private void SelectDropDownListOptionByValue(Act dd, string s, SelectElement se)
        {
            se.SelectByValue(s);
        }

        #endregion

        //public override List<ActLink> GetAllLinks()
        //{
        //    //TODO: dummy - write real code
        //    List<ActLink> ActLinks = new List<ActLink>();

        //    return ActLinks;
        //}

        private void HoverOverLink(ActLink Link)
        {
            IWebElement e = LocateElement(Link);
            if (e == null)
            {
                Link.Error = "Error: Element not found - " + Link.LocateBy + " " + Link.LocateValue;
                return;
            }
            OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.MoveToElement(e).Build().Perform();
        }

        private IWebElement FindElementReg(eLocateBy LocatorType, string LocValue)
        {
            Regex reg = new Regex(LocValue.Replace("{RE:", "").Replace("}", ""), RegexOptions.Compiled);

            var searchTags = new[] { "a", "link", "h1", "h2", "h3", "h4", "h5", "h6", "label", "input", "selection", "p" };
            var elem = Driver.FindElements(By.XPath("//*")).Where(e => searchTags.Contains(e.TagName.ToLower()));

            switch (LocatorType)
            {
                case eLocateBy.ByID:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("id") != null)
                        {
                            if (reg.Matches(e.GetAttribute("id")).Count > 0)
                            {
                                return e;
                            }
                        }
                    }
                    break;
                case eLocateBy.ByName:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("name") != null)
                        {
                            if (reg.Matches(e.GetAttribute("name")).Count > 0)
                            {
                                return e;
                            }
                        }
                    }
                    break;
                case eLocateBy.ByLinkText:
                    foreach (IWebElement e in elem)
                    {
                        if (e.Text != null)
                        {
                            if (reg.Matches(e.Text).Count > 0)
                            {
                                return e;
                            }
                        }
                    }
                    break;
                case eLocateBy.ByValue:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("value") != null)
                        {
                            if (reg.Matches(e.GetAttribute("value")).Count > 0)
                            {
                                return e;
                            }
                        }
                    }
                    break;
                case eLocateBy.ByHref:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("href") != null)
                        {
                            if (reg.Matches(e.GetAttribute("href")).Count > 0 && e.Text != "")
                            {
                                return e;
                            }
                        }
                    }
                    break;
            }
            return Driver.FindElements(By.XPath("//*[@value=\"" + LocValue + "\"]")).FirstOrDefault();
        }


        // This function is called not only by WebDrivers but Other Drivers as well(eg Mobile Driver).
        // If changes are made here or any functions that are called in this function, Please test for all the Calling Drivers.
        public IWebElement LocateElement(Act act, bool AlwaysReturn = false, string ValidationElementLocateBy = null, string ValidationElementLocateValue = null)
        {
            IWebElement elem = null;
            eLocateBy locateBy = act.LocateBy;
            string locateValue = act.LocateValueCalculated;
            if (ValidationElementLocateBy != null)
            {
                Enum.TryParse<eLocateBy>(ValidationElementLocateBy, true, out locateBy);
            }
            if (ValidationElementLocateValue != null)
            {
                locateValue = ValidationElementLocateValue;
            }

            if (act is ActUIElement actUIElement && (ValidationElementLocateBy == null || ValidationElementLocateValue == null))
            {
                Enum.TryParse<eLocateBy>(actUIElement.ElementLocateBy.ToString(), true, out locateBy);
                locateValue = actUIElement.ElementLocateValueForDriver;
            }

            if (locateBy == eLocateBy.POMElement)
            {
                POMExecutionUtils pomExcutionUtil;
                ApplicationPOMModel currentPOM;
                GetCurrentPOM(act, out pomExcutionUtil, out currentPOM);

                if (currentPOM != null)
                {
                    ElementInfo currentPOMElementInfo = null;
                    if (isAppiumSession)
                    {
                        currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo(this.PomCategory);//consider the Category only in case of Mobile flow for now
                    }
                    else
                    {
                        currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo();
                    }

                    if (currentPOMElementInfo != null)
                    {
                        if (HandelIFramShiftAutomaticallyForPomElement)
                        {
                            SwitchFrame(currentPOMElementInfo);
                        }
                        // Check if the application model needs to be forcefully updated based on the self-healing configuration
                        // Automatically update the current Page Object Model (POM) for the current agent in the current activity
                        // Add the GUID of the updated POM to the list of auto-updated POMs in the runset configuration
                        pomExcutionUtil.AutoForceUpdateCurrentPOM(this.BusinessFlow.CurrentActivity.CurrentAgent, act);

                        elem = LocateElementByLocators(currentPOMElementInfo, currentPOM.MappedUIElements, false, pomExcutionUtil);

                        if (elem == null && pomExcutionUtil.AutoUpdateCurrentPOM(this.BusinessFlow.CurrentActivity.CurrentAgent) != null)
                        {
                            if (isAppiumSession)
                            {
                                currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo(this.PomCategory);
                            }
                            elem = LocateElementByLocators(currentPOMElementInfo, currentPOM.MappedUIElements, false, pomExcutionUtil);
                            if (elem != null)
                            {
                                if (currentPOM != null)
                                {
                                    currentPOM.AllowAutoSave = true;
                                    SaveHandler.Save(currentPOM);
                                }
                                else
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, $"Cannot find POM with GUID '{currentPOM.Guid}' to save");
                                }
                                act.ExInfo += "Broken element was auto updated by Self healing operation";
                            }
                        }

                        if (elem != null && currentPOMElementInfo.SelfHealingInfo == SelfHealingInfoEnum.ElementDeleted)
                        {
                            currentPOMElementInfo.SelfHealingInfo = SelfHealingInfoEnum.None;
                        }

                        currentPOMElementInfo.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Failed).ToList().ForEach(y => act.ExInfo += System.Environment.NewLine + string.Format("Failed to locate the element with LocateBy='{0}' and LocateValue='{1}', Error Details:'{2}'", y.LocateBy, y.LocateValue, y.LocateStatus));

                        if (pomExcutionUtil.PriotizeLocatorPosition())
                        {
                            act.ExInfo += "Locator prioritized during self healing operation";
                        }
                    }
                }
            }
            else
            {
                ElementLocator locator = new ElementLocator
                {
                    LocateBy = locateBy,
                    LocateValue = locateValue
                };
                elem = LocateElementByLocator(locator, CurrentContext, null, AlwaysReturn);
                if (elem == null)
                {
                    act.ExInfo += string.Format("Failed to locate the element with LocateBy='{0}' and LocateValue='{1}', Error Details:'{2}'", locator.LocateBy, locator.LocateValue, locator.LocateStatus);
                }
            }

            return elem;
        }

        private static void GetCurrentPOM(Act act, out POMExecutionUtils pomExcutionUtil, out ApplicationPOMModel currentPOM)
        {
            pomExcutionUtil = new POMExecutionUtils(act, RetrieveActionValue(act));
            currentPOM = pomExcutionUtil.GetCurrentPOM();
        }

        private static string RetrieveActionValue(Act act)
        {
            if (act is ActUIElement)
            {
                return ((ActUIElement)act).ElementLocateValue;
            }
            else
            {
                return act.LocateValue;
            }
        }

        private void SwitchFrame(ElementInfo EI)
        {
            UnhighlightLast();
            Driver.SwitchTo().DefaultContent();
            if (!string.IsNullOrEmpty(EI.Path))
            {
                if (!EI.IsAutoLearned)
                {
                    ValueExpression VE = new ValueExpression(null, null);
                    EI.Path = VE.Calculate(EI.Path);
                    if (EI.Path == null)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Concat("Expression : ", EI.Path, " evaluated to null value."));
                        return;
                    }
                }
                //split Path by commo outside of brackets 
                var spliter = new Regex(@",(?![^\[]*[\]])");
                string[] iframesPathes = spliter.Split(EI.Path);
                foreach (string iframePath in iframesPathes)
                {

                    Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(iframePath)));
                }
            }
        }

        // TODO: convert recursion to iteration    
        public IWebElement LocateElementByLocators(ElementInfo currentPOMElementInfo, IList<ElementInfo> MappedUIElements, bool iscallfromFriendlyLocator = false, POMExecutionUtils POMExecutionUtils = null)
        {
            IWebElement elem = null;
            foreach (ElementLocator locator in currentPOMElementInfo.Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            ISearchContext shadowRoot = null;
            ISearchContext ParentContext = Driver;
            TimeSpan ImpWait = Driver.Manage().Timeouts().ImplicitWait;

            if (currentPOMElementInfo is HTMLElementInfo htmlCurrentElementInfo)
            {
                var ParentPOMGuid = htmlCurrentElementInfo.FindParentPOMGuid();

                if (!ParentPOMGuid.Equals(Guid.Empty.ToString()))
                {
                    HTMLElementInfo ParentElement = null;
                    ParentElement = htmlCurrentElementInfo.FindParentElementUsingGuid(MappedUIElements);

                    if (ParentElement != null)
                    {
                        ParentContext = LocateElementByLocators(ParentElement, MappedUIElements, iscallfromFriendlyLocator, POMExecutionUtils);
                    }
                    shadowRoot = shadowDOM.GetShadowRootIfExists(ParentContext);
                }

            }

            try
            {
                foreach (ElementLocator locator in currentPOMElementInfo.Locators.Where(x => x.Active))
                {
                    List<FriendlyLocatorElement> friendlyLocatorElementlist = [];
                    if (locator.EnableFriendlyLocator && !iscallfromFriendlyLocator)
                    {
                        IWebElement targetElement = null;

                        foreach (ElementLocator FLocator in currentPOMElementInfo.FriendlyLocators.Where(x => x.Active))
                        {
                            if (!FLocator.IsAutoLearned)
                            {
                                ElementLocator evaluatedLocator = FLocator.CreateInstance() as ElementLocator;
                                ValueExpression VE = new(GetCurrentProjectEnvironment(), this.BusinessFlow);
                                FLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
                            }

                            if (FLocator.LocateBy == eLocateBy.POMElement && POMExecutionUtils != null)
                            {
                                ElementInfo ReferancePOMElementInfo = POMExecutionUtils.GetFriendlyElementInfo(new Guid(FLocator.LocateValue));

                                targetElement = LocateElementByLocators(ReferancePOMElementInfo, MappedUIElements, true, POMExecutionUtils);
                            }
                            else
                            {
                                if (shadowRoot != null)
                                {

                                    targetElement = LocateElementByLocator(locator, shadowRoot, friendlyLocatorElementlist, true);

                                }

                                else
                                {
                                    if (ParentContext != null)
                                    {
                                        targetElement = LocateElementByLocator(locator, ParentContext);
                                    }
                                }

                            }
                            if (targetElement != null)
                            {
                                FriendlyLocatorElement friendlyLocatorElement = new FriendlyLocatorElement
                                {
                                    position = FLocator.Position,
                                    FriendlyElement = targetElement
                                };
                                friendlyLocatorElementlist.Add(friendlyLocatorElement);
                            }
                        }

                    }



                    if (!locator.IsAutoLearned)
                    {
                        if (shadowRoot != null)
                        {

                            elem = LocateElementByLocator(locator, shadowRoot, friendlyLocatorElementlist, true);

                        }
                        else
                        {
                            if (ParentContext != null)
                            {
                                elem = LocateElementIfNotAutoLeared(locator, ParentContext, friendlyLocatorElementlist);
                            }
                        }
                    }
                    else
                    {

                        if (shadowRoot != null)
                        {

                            elem = LocateElementByLocator(locator, shadowRoot, friendlyLocatorElementlist, true);

                        }

                        else
                        {
                            if (ParentContext != null)
                            {
                                elem = LocateElementByLocator(locator, ParentContext, friendlyLocatorElementlist, true);
                                Reporter.ToLog(eLogLevel.DEBUG, $"{locator.StatusError} timespan : {Driver.Manage().Timeouts().ImplicitWait}");
                                if (elem == null && locator.LocateBy != eLocateBy.ByID && locator.LocateBy != eLocateBy.ByName)
                                {
                                    // Get current implicit wait timeout
                                    TimeSpan currentTimeout = Driver.Manage().Timeouts().ImplicitWait;

                                    // Decrease by 25%
                                    TimeSpan reducedTimeout = TimeSpan.FromMilliseconds(currentTimeout.TotalMilliseconds * 0.75);

                                    // Set the new reduced timeout //decrease Implicit wait by 25% when locater gets failed. (exception for ByID,ByName) 
                                    Driver.Manage().Timeouts().ImplicitWait = reducedTimeout;
                                }
                            }
                        }
                    }

                    if (elem != null)
                    {
                        locator.StatusError = string.Empty;
                        locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
                        return elem;
                    }
                    else
                    {

                        locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to locate element '{currentPOMElementInfo?.ElementName}' ActiveLocators=[{string.Join(", ", currentPOMElementInfo?.Locators?.Where(l => l.Active).Select(l => $"{l.LocateBy}='{l.LocateValue}'") ?? Enumerable.Empty<string>())}]", ex);
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = ImpWait;//reset Implicit wait
            }

            return elem;
        }

        public IWebElement LocateElementByLocator(ElementLocator locator, ISearchContext searchContext, List<FriendlyLocatorElement> friendlyLocatorElements = null, bool AlwaysReturn = true)
        {
            IWebElement elem = null;
            locator.StatusError = "";
            locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            try
            {
                try
                {
                    Protractor.NgWebDriver ngDriver = null;

                    // Check if the locator requires interaction with Angular elements
                    if (locator.LocateBy is eLocateBy.ByngRepeat or
                        eLocateBy.ByngSelectedOption or
                        eLocateBy.ByngBind or
                        eLocateBy.ByngModel)
                    {
                        // Initialize NgWebDriver and wait for Angular
                        ngDriver = new Protractor.NgWebDriver(Driver);
                        ngDriver.WaitForAngular();
                    }

                    // Perform element location based on NgBy types
                    if (ngDriver != null)
                    {
                        switch (locator.LocateBy)
                        {
                            case eLocateBy.ByngRepeat:
                                elem = ngDriver.FindElement(Protractor.NgBy.Repeater(locator.LocateValue));
                                break;
                            case eLocateBy.ByngSelectedOption:
                                elem = ngDriver.FindElement(Protractor.NgBy.SelectedOption(locator.LocateValue));
                                break;
                            case eLocateBy.ByngBind:
                                elem = ngDriver.FindElement(Protractor.NgBy.Binding(locator.LocateValue));
                                break;
                            case eLocateBy.ByngModel:
                                elem = ngDriver.FindElement(Protractor.NgBy.Model(locator.LocateValue));
                                break;
                            default:
                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when LocateElementByLocator", ex);
                    if (AlwaysReturn)
                    {
                        elem = null;
                        locator.StatusError = ex.Message;
                        locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                        return elem;
                    }
                    else
                    {
                        throw;
                    }
                }

                switch (locator.LocateBy)
                {
                    case eLocateBy.ByID:
                    case eLocateBy.ByName:
                    case eLocateBy.ByValue:
                    case eLocateBy.ByAutomationID:
                    case eLocateBy.ByCSS:
                    case eLocateBy.ByClassName:
                    case eLocateBy.ByTagName:
                    case eLocateBy.ByTitle:
                    case eLocateBy.ByAriaLabel:
                    case eLocateBy.ByDataTestId:
                    case eLocateBy.ByPlaceholder:
                    case eLocateBy.ByCSSSelector:
                        elem = LocateElementBySingleProperty(locator, friendlyLocatorElements, searchContext);
                        break;

                    case eLocateBy.ByHref:
                        elem = LocateElementByHref(locator, friendlyLocatorElements, searchContext, shadowDOM);
                        break;

                    case eLocateBy.ByLinkText:
                        elem = LocateElementByLinkText(locator, friendlyLocatorElements, searchContext);
                        break;

                    case eLocateBy.ByXPath:
                    case eLocateBy.ByRelXPath:
                        elem = LocateElementByXPath(locator, friendlyLocatorElements, searchContext, shadowDOM);
                        break;

                    case eLocateBy.ByMulitpleProperties:
                        elem = GetElementByMutlipleAttributes(locator.LocateValue, searchContext);
                        break;
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (AlwaysReturn)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                if (AlwaysReturn)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                {
                    throw;
                }
            }

            if (elem != null)
            {
                locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
            }

            return elem;
        }
        private IWebElement LocateElementBySingleProperty(ElementLocator locator, IList<FriendlyLocatorElement> friendlyLocatorElements, ISearchContext searchContext)
        {
            IWebElement elem = null;

            // Check for regular or regex-based search
            if (locator.LocateValue.Contains("{RE:"))
            {
                elem = FindElementReg(locator.LocateBy, locator.LocateValue);
            }
            else
            {
                // Create appropriate By locator
                By by = null;
                switch (locator.LocateBy)
                {
                    case eLocateBy.ByID:
                        by = By.Id(locator.LocateValue);
                        break;
                    case eLocateBy.ByName:
                        by = By.Name(locator.LocateValue);
                        break;
                    case eLocateBy.ByValue:
                        by = By.XPath("//*[@value=\"" + locator.LocateValue + "\"]");
                        break;
                    case eLocateBy.ByAutomationID:
                        by = By.XPath("//*[@data-automation-id=\"" + locator.LocateValue + "\"]");
                        break;
                    case eLocateBy.ByCSS:
                        by = By.CssSelector(locator.LocateValue);
                        break;
                    case eLocateBy.ByClassName:
                        by = By.ClassName(locator.LocateValue);
                        break;
                    case eLocateBy.ByTagName:
                        by = By.TagName(locator.LocateValue);
                        break;
                    case eLocateBy.ByCSSSelector:
                        by = By.CssSelector(locator.LocateValue);
                        break;
                    case eLocateBy.ByTitle:
                        by = By.CssSelector($"[title='{EscapeCssAttributeValue(locator.LocateValue)}']");
                        break;
                    case eLocateBy.ByAriaLabel:
                        by = By.CssSelector($"[aria-label='{EscapeCssAttributeValue(locator.LocateValue)}']");
                        break;
                    case eLocateBy.ByDataTestId:
                        by = By.CssSelector($"[data-testid='{EscapeCssAttributeValue(locator.LocateValue)}']");
                        break;
                    case eLocateBy.ByPlaceholder:
                        by = By.CssSelector($"[placeholder='{EscapeCssAttributeValue(locator.LocateValue)}']");
                        break;

                }

                // Use friendly locator if enabled and available
                if (locator.EnableFriendlyLocator && friendlyLocatorElements?.Count > 0)
                {
                    elem = GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                }
                else
                {
                    // Find element using the By locator
                    elem = searchContext.FindElement(by);
                }
            }

            return elem;
        }

        private IWebElement LocateElementByHref(ElementLocator locator, IList<FriendlyLocatorElement> friendlyLocatorElements, ISearchContext searchContext, ShadowDOM shadowDOM)
        {
            IWebElement elem = null;

            string pattern = @".+:\/\/[^\/]+";
            string sel = "//a[contains(@href, '@RREEPP')]";
            sel = sel.Replace("@RREEPP", new Regex(pattern).Replace(locator.LocateValue, ""));
            try
            {
                if (locator.LocateValue.IndexOf("{RE:") >= 0)
                {
                    elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                }
                else
                {
                    // Use friendly locator if enabled and available
                    if (locator.EnableFriendlyLocator && friendlyLocatorElements?.Count > 0)
                    {
                        By by = By.XPath(sel);
                        elem = GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }

                    // Handling for Shadow DOM elements
                    if (searchContext is ShadowRoot)
                    {
                        elem ??= searchContext.FindElement(By.CssSelector($"a[href='{sel}']"));
                    }
                    else
                    {
                        elem ??= searchContext.FindElement(By.XPath(sel));
                    }
                }
            }
            catch (NoSuchElementException ex)
            {
                sel = "//a[href='@']";
                sel = sel.Replace("@", locator.LocateValue);
                elem = searchContext.FindElement(By.XPath(sel));
                locator.StatusError = ex.Message;
            }

            return elem;
        }

        private IWebElement LocateElementByLinkText(ElementLocator locator, IList<FriendlyLocatorElement> friendlyLocatorElements, ISearchContext searchContext)
        {
            IWebElement elem = null;

            locator.LocateValue = locator.LocateValue.Trim();
            try
            {
                if (locator.LocateValue.Contains("{RE:"))
                {
                    elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                }
                else
                {
                    // Use friendly locator if enabled and available
                    if (locator.EnableFriendlyLocator && friendlyLocatorElements?.Count > 0)
                    {
                        By by = By.LinkText(locator.LocateValue);
                        elem = GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                    }
                    elem ??= searchContext.FindElement(By.LinkText(locator.LocateValue));

                    if (elem == null)
                    {
                        // Try finding by text content if not found by link text
                        if (locator.EnableFriendlyLocator && friendlyLocatorElements?.Count > 0)
                        {
                            By by = By.XPath("//*[text()='" + locator.LocateValue + "']");
                            elem = GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
                        }
                        elem ??= searchContext.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (ex.GetType() == typeof(NoSuchElementException))
                    {
                        elem = searchContext.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));
                    }
                }
                catch (Exception ex2)
                {
                    locator.StatusError = ex2.Message;
                }
            }

            return elem;
        }

        private IWebElement LocateElementByXPath(ElementLocator locator, IList<FriendlyLocatorElement> friendlyLocatorElements, ISearchContext searchContext, ShadowDOM shadowDOM)
        {
            IWebElement elem = null;

            string _locatorValue = locator.LocateValue;

            // Adjust locator value for case-insensitive matching if necessary
            if (locator.LocateBy.Equals(eLocateBy.ByRelXPath) && locator.IsAutoLearned && _locatorValue.Contains("text()"))
            {
                _locatorValue = _locatorValue.ToLower().Replace("text()", TRANSLATOR_FOR_CASE_INSENSITIVE_MATCH);
            }

            // Use friendly locators if enabled and available
            if (locator.EnableFriendlyLocator && friendlyLocatorElements?.Count > 0)
            {
                By by = By.XPath(_locatorValue);
                elem = GetElementByFriendlyLocatorlist(friendlyLocatorElements, by);
            }

            // Handling for Shadow DOM elements
            if (searchContext is ShadowRoot)
            {
                if (locator.LocateBy.Equals(eLocateBy.ByXPath))
                {
                    // Convert XPath to CSS selector for Shadow DOM
                    string cssSelector = shadowDOM.ConvertXPathToCssSelector(_locatorValue);
                    elem ??= searchContext.FindElement(By.CssSelector(cssSelector));
                }
            }
            else
            {
                // Regular search context, find element by XPath
                elem ??= searchContext.FindElement(By.XPath(_locatorValue));
            }

            return elem;
        }
        private IWebElement GetElementByFriendlyLocatorlist(IList<FriendlyLocatorElement> friendlyLocatorElements, By by)
        {
            Dictionary<string, object> dictionary = [];
            List<object> filters = [];
            List<object> Args = [];
            foreach (FriendlyLocatorElement friendlyLocatorElement in friendlyLocatorElements)
            {
                dictionary["kind"] = friendlyLocatorElement.position.ToString();
                dictionary["args"] = new List<object>
                            {
                                friendlyLocatorElement.FriendlyElement
                            };
                filters.Add(dictionary);

            }
            string arg = string.Empty;
            arg = GingerCoreNET.GeneralLib.General.GetDataByassemblyNameandResource("WebDriver", "find-elements.js");
            string wrappedAtom = string.Format(CultureInfo.InvariantCulture, "return ({0}).apply(null, arguments);", arg);
            Dictionary<string, object> dictionary2 = [];
            Dictionary<string, object> mydictionary = [];
            Dictionary<string, object> rootdictionary = [];
            if (by != null)
            {
                rootdictionary[by.Mechanism] = by.Criteria;
            }
            dictionary2["root"] = rootdictionary;
            dictionary2["filters"] = filters;
            mydictionary["relative"] = dictionary2;

            object output = ((IJavaScriptExecutor)Driver).ExecuteScript(wrappedAtom, mydictionary);

            return output is ReadOnlyCollection<IWebElement> nodes && nodes.Count > 0 ? nodes[0] : null;
        }



        private IWebElement GetElementByMutlipleAttributes(string LocValue, ISearchContext searchContext)
        {
            //Fix me
            //put in hash map
            // find by id or common then by other attrs
            string[] a = LocValue.Split(';');
            string[] a0 = a[0].Split('=');

            string id = null;
            if (a0[0] == "id")
            {
                id = a0[1];
            }

            string[] a1 = a[1].Split('=');
            string attr = a1[0];
            string val = a1[1];

            if (id == null)
            {
                return null;
            }
            ReadOnlyCollection<IWebElement> list = searchContext.FindElements(By.Id(id));

            foreach (IWebElement e in list)
            {
                if (e.GetAttribute(attr) == val)
                {
                    return e;
                }
            }
            return null;
        }

        public List<IWebElement> LocateElements(eLocateBy LocatorType, string LocValue)
        {
            IReadOnlyCollection<IWebElement> elem = null; //TODO: Not found
            //TODO: switch case By.. - order by most common first
            switch (LocatorType)
            {
                case eLocateBy.ByID:

                    elem = Driver.FindElements(By.Id(LocValue));
                    break;

                case eLocateBy.ByName:

                    elem = Driver.FindElements(By.Name(LocValue));
                    break;

                case eLocateBy.ByHref:

                    string sel = "a[href='@']";
                    sel = sel.Replace("@", LocValue);
                    elem = Driver.FindElements(By.CssSelector(sel));
                    break;

                case eLocateBy.ByClassName:

                    elem = Driver.FindElements(By.ClassName(LocValue));
                    break;

                case eLocateBy.ByLinkText:

                    LocValue = LocValue.Trim();
                    try
                    {
                        elem = Driver.FindElements(By.LinkText(LocValue));
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (ex.GetType() == typeof(NoSuchElementException))
                            {
                                elem = Driver.FindElements(By.XPath("//*[text()='" + LocValue + "']"));
                            }
                        }
                        catch { }
                    }
                    break;

                case eLocateBy.ByXPath:
                case eLocateBy.ByRelXPath:

                    elem = Driver.FindElements(By.XPath(LocValue));
                    break;

                case eLocateBy.ByValue:

                    elem = Driver.FindElements(By.XPath("//*[@value=\"" + LocValue + "\"]"));
                    break;

                case eLocateBy.ByCSS:

                    elem = Driver.FindElements(By.CssSelector(LocValue));
                    break;
            }


            if (elem != null)
            {
                return elem.ToList();
            }
            else
            {
                return null;
            }

        }

        //public override List<ActButton> GetAllButtons()
        //{
        //    List<ActButton> Buttons = new List<ActButton>();
        //    System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> elements;
        //    //add all other buttons
        //    elements = Driver.FindElements(By.TagName("button"));
        //    foreach (IWebElement e in elements)
        //    {
        //        // TODO: locators...
        //        string id = e.GetAttribute("id");
        //        ActButton a = new ActButton();
        //        a.LocateBy = eLocateBy.ByID;
        //        a.LocateValue = id;

        //        Buttons.Add(a);
        //    }
        //    return Buttons;
        //}

        public override void HighlightActElement(Act act)
        {
            //TODO: make it work with all locators
            // Currently will work with XPath and when GingerLib Exist

            List<IWebElement> elements = LocateElements(act.LocateBy, act.LocateValueCalculated);
            if (elements != null)
            {
                foreach (IWebElement e in elements)
                {
                    //ElementInfo elementInfo = GetElementInfoWithIWebElement(e, null, string.Empty);

                    //string highlightJavascript = string.Empty;
                    //if (elementInfo.ElementType == "INPUT.CHECKBOX" || elementInfo.ElementType == "TR" || elementInfo.ElementType == "TBODY")
                    //        highlightJavascript = "arguments[0].style.outline='3px dashed red'";
                    //else
                    //    highlightJavascript = "arguments[0].style.border='3px dashed red'";
                    //((IJavaScriptExecutor)Driver).ExecuteScript(highlightJavascript, new object[] { e });
                    //LastHighLightedElementInfo = elementInfo;
                    //elementInfo.ElementObject = e;

                    HTMLElementInfo elementInfo = new HTMLElementInfo
                    {
                        ElementObject = e
                    };

                    HighlightElement(elementInfo);
                }
            }
        }

        public override ePlatformType Platform
        {
            get { return ePlatformType.Web; }
        }


        private static string handleExePath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "StaticDrivers", "handle.exe");

        public string BrowserExeName
        {
            get { return ((OpenQA.Selenium.WebDriver)Driver).Capabilities.GetCapability(OpenQA.Selenium.CapabilityType.BrowserName).ToString() + ".exe"; }
        }

        /// <summary>
        /// Supported only on Windows. Checks if browser opened by driver is still open or closed manually or by other application. It used Handle exe to check attached handles the driver exe by drivers process id
        /// </summary>
        /// <returns></returns>
        private bool IsBrowserAlive()
        {
            Thread.Sleep(100);

            var processHandle = Process.Start(new ProcessStartInfo() { FileName = handleExePath, Arguments = $" -accepteula -a -p {this.mDriverProcessId} \"{this.BrowserExeName}\"", UseShellExecute = false, RedirectStandardOutput = true });

            string cliOut = processHandle.StandardOutput.ReadToEnd();
            processHandle.WaitForExit();
            processHandle.Close();

            if (!cliOut.Contains($"{this.BrowserExeName}"))
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"{this.BrowserExeName} Browser not found for PID {this.mDriverProcessId}");
                return false;
            }
            return true;
        }

        public override bool IsRunning()
        {
            if (Driver != null)
            {
                //TOCHECK
                /* IF Driver.windowhandles or Driver.Title such properties fails try the following approach
                 * After evry switch window get current window handler and store it we are already saving default window handler when launching driver 
                 * now try to switch to current handle if no exception return true
                 * if exception try to switch to default window handle if no exception return true
                 * if exception then try the current mechanism
                 * */

                int maxAttempts = 5;
                int attemptCount = 0;

                while (attemptCount <= maxAttempts)
                {
                    try
                    {
                        int count = 0;

                        try
                        {
                            count = Driver.WindowHandles.Count;
                        }
                        catch (System.InvalidCastException ex)
                        {
                            count = Driver.CurrentWindowHandle.Length;
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception occurred while casting when we are checking IsRunning", ex);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            count = Driver.CurrentWindowHandle.Length;
                            Reporter.ToLog(eLogLevel.DEBUG, "Null reference exception occurred when we are checking IsRunning", ex);
                        }
                        catch (System.ObjectDisposedException ex)
                        {
                            ErrorMessageFromDriver = "Agent is closed. Action on closed agent is not allowed.";
                            Reporter.ToLog(eLogLevel.DEBUG, "Driver object is already disposed ", ex);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception occurred when we are checking IsRunning", ex);
                            throw;
                        }

                        if (count == 0)
                        {
                            return false;
                        }
                        else if (count > 0)
                        {
                            return true;
                        }

                        if (attemptCount < maxAttempts)
                        {
                            attemptCount++;
                            continue;
                        }

                        var currentWindow = Driver.CurrentWindowHandle;
                        if (!string.IsNullOrEmpty(currentWindow))
                        {
                            return true;
                        }

                        if (count == 0)
                        {
                            return false;
                        }
                    }
                    catch (OpenQA.Selenium.UnhandledAlertException)
                    {
                        return false;
                    }
                    catch (OpenQA.Selenium.NoSuchWindowException ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Exception occurred when we are checking IsRunning", ex);
                        var currentWindow = Driver.CurrentWindowHandle;
                        if (!string.IsNullOrEmpty(currentWindow))
                        {

                            return true;
                        }

                        if (attemptCount < maxAttempts)
                        {
                            attemptCount++;
                            continue;
                        }
                    }
                    catch (OpenQA.Selenium.WebDriverTimeoutException ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Timeout exception occurred when we are checking IsRunning", ex);
                        var currentWindow = Driver.CurrentWindowHandle;
                        if (!string.IsNullOrEmpty(currentWindow))
                        {
                            return true;
                        }

                        if (attemptCount < maxAttempts)
                        {
                            attemptCount++;
                            continue;
                        }
                    }
                    catch (OpenQA.Selenium.WebDriverException ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Webdriver exception occurred when we are checking IsRunning", ex);

                        if (PreviousRunStopped && ex.Message == "Unexpected error. Error 404: Not Found\r\nNot Found")
                        {
                            return true;
                        }

                        if (attemptCount < maxAttempts)
                        {
                            attemptCount++;
                            continue;
                        }

                        CloseDriver();
                        break;
                    }
                    catch (Exception ex2)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Exception occurred when we are checking IsRunning", ex2);
                        if (ex2.Message.ToString().ToUpper().Contains("DIALOG"))
                        {
                            return true;
                        }

                        CloseDriver();
                        break;
                    }

                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            if (Driver != null)
            {
                UnhighlightLast();
                LastHighLightedElement = null;
                List<AppWindow> list = [];

                ReadOnlyCollection<string> windows = Driver.WindowHandles;
                //TODO: get current win and keep, later on set in combo
                foreach (string window in windows)
                {
                    try
                    {
                        if (!window.Equals(Driver.CurrentWindowHandle))
                        {
                            Driver.SwitchTo().Window(window);
                        }
                        AppWindow AW = new AppWindow
                        {
                            Title = Driver.Title,
                            WindowType = AppWindow.eWindowType.WebPage
                        };
                        list.Add(AW);
                    }
                    catch (Exception ex)
                    {
                        string wt = Driver.Title; //if Switch window throw exception then reading current driver title to avoid exception for next window handle in loop
                        Reporter.ToLog(eLogLevel.ERROR, "Error occurred during GetAppWindows.", ex);
                    }
                }
                return list.ToList();
            }
            return null;
        }

        /// <summary>
        /// For Mobile Web Elements Learning process is too slow due to increased Driver usage
        /// Hence, we'll learn extra lcoators only in cases where Custom Relative XPath is checked by user for Mobile Platform
        /// Else, it'll be skipped - Checking the performance
        /// </summary>
        public bool ExtraLocatorsRequired = true;
        async Task<List<ElementInfo>> IWindowExplorer.GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null, Bitmap ScreenShot = null)
        {
            return await Task.Run(async () =>
            {
                mIsDriverBusy = true;

                try
                {
                    UnhighlightLast();

                    Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                    Driver.SwitchTo().DefaultContent();
                    allReadElem.Clear();
                    List<ElementInfo> list = General.ConvertObservableListToList<ElementInfo>(FindAllElementsFromPOM("", pomSetting, Driver, Guid.Empty, foundElementsList, PomMetaData, ScreenShot: ScreenShot));
                    ElementWrapperInfo elementWrapperInfo = new ElementWrapperInfo();
                    elementWrapperInfo.elements = new List<ElementWrapper>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        ElementInfo elementInfo = list[i];
                        if (elementInfo.FriendlyLocators.Count > 0)
                        {
                            for (int j = 0; j < elementInfo.FriendlyLocators.Count; j++)
                            {
                                ElementLocator felementLocator = elementInfo.FriendlyLocators[j];
                                ElementInfo newelementinfo = list.FirstOrDefault(x => x.XPath == felementLocator.LocateValue);
                                if (newelementinfo != null)
                                {
                                    felementLocator.LocateValue = newelementinfo.Guid.ToString();
                                    felementLocator.ReferanceElement = newelementinfo.ElementName + "[" + newelementinfo.ElementType + "]";
                                }
                                else
                                {
                                    elementInfo.FriendlyLocators.Remove(felementLocator);
                                    j--;
                                }
                            }
                        }

                        // If no friendly locators are found, disable the By Tag name locator if its not the only one
                        if (elementInfo.FriendlyLocators.Count == 0 && elementInfo.Locators.Count > 1)
                        {
                            var tagLocator = elementInfo.Locators.FirstOrDefault(f => f.LocateBy == eLocateBy.ByTagName);
                            if (tagLocator != null)
                            {
                                tagLocator.Active = false;
                            }
                        }
                    }
                    allReadElem.Clear();
                    CurrentFrame = "";
                    Driver.Manage().Timeouts().ImplicitWait = new TimeSpan();
                    Driver.SwitchTo().DefaultContent();
                    return list;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while getting visible controls", ex);
                    return new List<ElementInfo>();
                }
                finally
                {
                    mIsDriverBusy = false;
                    Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
                }
            });
        }

        public void EnhanceElementLocators(ElementInfo element)
        {
            // Skip visually hidden or off-screen elements
            if (element.Width <= 0 || element.Height <= 0 || element.X < 0 || element.Y < 0)
                return;

            string primary = null;
            string fallback = null;
            double score = 0.0;

            var idLocator = element.Locators?.FirstOrDefault(l => l.LocateBy == eLocateBy.ByID);
            if (idLocator != null)
            {
                primary = $"//*[@id='{idLocator.LocateValue}']";
                score += 0.9;
            }
            else
            {
                var nameLocator = element.Locators?.FirstOrDefault(l => l.LocateBy == eLocateBy.ByName);
                if (nameLocator != null)
                {
                    primary = $"//*[@name='{nameLocator.LocateValue}']";
                    score += 0.8;
                }
                else
                {
                    var cssLocator = element.Locators?.FirstOrDefault(l => l.LocateBy == eLocateBy.ByCSS);
                    if (cssLocator != null)
                    {
                        primary = cssLocator.LocateValue;
                        score += 0.7;
                    }
                }
            }

            var xpathLocator = element.Locators?.FirstOrDefault(l => l.LocateBy == eLocateBy.ByXPath);
            if (xpathLocator != null)
            {
                fallback = xpathLocator.LocateValue;
                score += 0.5;
            }

            if (element.IsAutoLearned) score += 0.1;
            if (element.Active) score += 0.1;

            string finalLocator = primary ?? fallback ?? "unknown";

            // Inject enhanced locator into Locators list as RelativePath
            if (element.Locators == null)
                element.Locators = new ObservableList<ElementLocator>();

            if (!string.IsNullOrEmpty(finalLocator) && CheckElementLocateStatus(finalLocator))
            {
                var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = finalLocator, IsAutoLearned = true };
                element.Locators.Add(elementLocator);
            }
        }
        // Define a collection of excluded element names
        internal static HashSet<string> excludedElementNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "noscript", "script", "style", "meta", "head", "link", "html", "body","br"
        };

        public void AddElementLocators(ObservableList<ElementLocator> locatorList, dynamic locators)
        {
            void AddIfNotNull(eLocateBy locateBy, string value)
            {
                // Escape CSS attribute values for CSS-based selectors
                if (locateBy == eLocateBy.ByTitle || locateBy == eLocateBy.ByAriaLabel || locateBy == eLocateBy.ByDataTestId || locateBy == eLocateBy.ByPlaceholder)
                {
                    value = EscapeCssAttributeValue(value);
                }
                if (!string.IsNullOrEmpty(value))
                {
                    locatorList.Add(new ElementLocator { LocateBy = locateBy, LocateValue = value, IsAutoLearned = true });
                }
            }

            AddIfNotNull(eLocateBy.ByID, locators.ByID);
            AddIfNotNull(eLocateBy.ByName, locators.ByName);
            AddIfNotNull(eLocateBy.ByRelXPath, locators.ByRelXPath);
            AddIfNotNull(eLocateBy.ByXPath, locators.ByXPath);
            AddIfNotNull(eLocateBy.ByCSS, locators.ByCSS);
            AddIfNotNull(eLocateBy.ByClassName, locators.ByClassName);
            AddIfNotNull(eLocateBy.ByTitle, locators.ByTitle);
            AddIfNotNull(eLocateBy.ByCSSSelector, locators.ByCSSSelector);
            AddIfNotNull(eLocateBy.ByDataTestId, locators.ByDataTestId);
            AddIfNotNull(eLocateBy.ByPlaceholder, locators.ByPlaceholder);
            AddIfNotNull(eLocateBy.ByTagName, locators.ByTagName);
            AddIfNotNull(eLocateBy.ByLinkText, locators.ByLinkText);
            AddIfNotNull(eLocateBy.ByHref, locators.ByHref);
            AddIfNotNull(eLocateBy.ByAriaLabel, locators.ByAriaLabel);
        }

        public void AddControlProperties(ObservableList<ControlProperty> propertyList, dynamic properties)
        {
            void AddIfNotNull(string name, string value)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    propertyList.Add(new ControlProperty { Name = name, Value = value });
                }
            }

            AddIfNotNull("name", properties.name);
            AddIfNotNull("Xpath", properties.Xpath);
            AddIfNotNull("class", properties.@class);
            AddIfNotNull("RelativeXpath", properties.RelativeXpath);
            AddIfNotNull("TagName", properties.TagName);
            AddIfNotNull("Displayed", properties.Displayed);
            AddIfNotNull("Enabled", properties.Enabled);
            AddIfNotNull("placeholder", properties.placeholder);
            AddIfNotNull("title", properties.title);
            AddIfNotNull("type", properties.type);
            AddIfNotNull("value", properties.value);
            AddIfNotNull("Selected", properties.Selected);
            AddIfNotNull("Text", properties.Text);
            AddIfNotNull("autocorrect", properties.autocorrect);
            AddIfNotNull("autocapitalize", properties.autocapitalize);
            AddIfNotNull("DataTest", properties.DataTest);
            AddIfNotNull("id", properties.id);
            AddIfNotNull("style", properties.style);
            AddIfNotNull("Width", properties.Width);
            AddIfNotNull("Height", properties.Height);
            AddIfNotNull("X", properties.X);
            AddIfNotNull("Y", properties.Y);
        }



        public HTMLElementInfo ConvertToHTMLElementInfo(Element element)
        {
            if (element?.Properties == null || element.locators == null)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"{nameof(element)}, Element or its properties cannot be null");
                return null;
            }
            var foundElementInfo = new HTMLElementInfo
            {
                ElementName = element.Properties.name,
                ElementType = element.Properties.TagName,
                ElementTypeEnum = GetElementTypeEnum(jsType: element.Properties.TagName, TypeAtt: element.Properties.type).Item2, // Map to your enum if needed\ 
                ElementObject = null, // Set if you have the actual WebElement
                Path = "",            // Set if applicable
                HTMLElementObject = null, // Set if you have the HTML node
                XPath = element.locators.ByRelXPath,
                IsAutoLearned = true,
                Width = TryParseInt(element.Properties.Width),
                Height = TryParseInt(element.Properties.Height),
                Active = true
            };


            AddElementLocators(foundElementInfo.Locators, element.locators);
            AddControlProperties(foundElementInfo.Properties, element.Properties);


            if (element.Properties.attributes != null)
            {
                foreach (var attr in element.Properties.attributes)
                {
                    foundElementInfo.Properties.Add(new ControlProperty { Name = attr.Key, Value = attr.Value });
                }
            }

            // Optional: Screenshot
            foundElementInfo.ScreenShotImage = ""; // Set if you have a base64 image or path

            return foundElementInfo;
        }


        private int TryParseInt(string value)
        {
            return int.TryParse(value, out int result) ? result : 0;
        }


        /// <summary>
        /// Finds all elements from the POM based on the provided settings and context.
        /// </summary>
        /// <param name="path">The path to the POM file.</param>
        /// <param name="pomSetting">The settings for learning the POM.</param>
        /// <param name="parentContext">The parent search context.</param>
        /// <param name="ParentGUID">The GUID of the parent element.</param>
        /// <param name="foundElementsList">List to store found elements.</param>
        /// <param name="PomMetaData">Metadata for the POM pages.</param>
        /// <param name="isShadowRootDetected">Indicates if shadow root is detected.</param>
        /// <param name="pageSource">HTML source of the page.</param>
        /// <param name="ScreenShot">Screenshot of the page.</param>
        /// <returns>List of found elements.</returns>
        private ObservableList<ElementInfo> FindAllElementsFromPOM(string path, PomSetting pomSetting, ISearchContext parentContext, Guid ParentGUID, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null, bool isShadowRootDetected = false, string pageSource = null, Bitmap ScreenShot = null)
        {
            // Initialize lists if null
            PomMetaData ??= [];
            foundElementsList ??= [];

            // Parse HTML document
            string documentContents = pageSource ?? Driver.PageSource;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(documentContents);

            IEnumerable<HtmlNode> htmlElements = htmlDoc.DocumentNode
                    .Descendants()
                    .Where(x => !x.Name.StartsWith('#') && !excludedElementNames.Contains(x.Name)
                                && !x.XPath.Contains("/noscript", StringComparison.OrdinalIgnoreCase));
            List<HtmlNode> formElementsList = [];

            foreach (HtmlNode htmlElemNode in htmlElements)
            {
                try
                {
                    if (StopProcess)
                    {
                        return foundElementsList;
                    }

                    // Determine element type
                    Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(htmlNode: htmlElemNode);

                    // Check if element should be learned
                    bool learnElement = ShouldLearnElement(pomSetting, elementTypeEnum.Item2);

                    // Learn element if required
                    if (learnElement)
                    {

                        IWebElement webElement = GetWebElement(parentContext, htmlElemNode, elementTypeEnum.Item2, isShadowRootDetected);

                        /// Skip invisible elements
                        if (!IsElementVisible(webElement))
                        {
                            continue;
                        }

                        HTMLElementInfo foundElementInfo = CreateHTMLElementInfo(webElement, path, htmlElemNode, elementTypeEnum.Item1, elementTypeEnum.Item2, ParentGUID, pomSetting, foundElementsList.Count.ToString(), ScreenShot: ScreenShot);

                        //set the POM category
                        foundElementInfo.SetLocatorsAndPropertiesCategory(this.PomCategory);

                        // Add element to found elements list
                        foundElementsList.Add(foundElementInfo);
                        allReadElem.Add(foundElementInfo);
                        // Special handling for SVG elements to capture child elements
                        if (IsSvgElement(elementTypeEnum.Item2))
                        {
                            ProcessSvgChildElements(parentContext, pomSetting, htmlElemNode, foundElementInfo, path, foundElementsList, PomMetaData, isShadowRootDetected, ScreenShot);
                        }

                        POMUtils.TriggerFineTuneWithAI(pomSetting, foundElementInfo,this.PomCategory,null);


                        // Recursively find elements within shadow DOM
                        if (pomSetting.LearnShadowDomElements && elementTypeEnum.Item2 != eElementType.Iframe)
                        {
                            ProcessShadowDOMElements(pomSetting, webElement, foundElementInfo, path, foundElementsList, PomMetaData, ScreenShot: ScreenShot);
                        }
                    }

                    // Handle iframe elements
                    if (elementTypeEnum.Item2 == eElementType.Iframe)
                    {
                        ProcessIframeElement(parentContext, htmlElemNode, path, pomSetting, ParentGUID, foundElementsList, PomMetaData, isShadowRootDetected, pageSource, ScreenShot: ScreenShot);
                    }

                    // Collect form elements
                    if (elementTypeEnum.Item2 == eElementType.Form)
                    {
                        CollectFormElements(formElementsList, htmlElemNode);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Failed to learn the Web Element '{htmlElemNode.Name}'", ex);
                }
            }
            POMUtils.TriggerDelayProcessingfinetuneWithAI(pomSetting, this.PomCategory);
            // Process form elements and add metadata
            ProcessFormElements(formElementsList, Driver, pomSetting, foundElementsList, PomMetaData);

            return foundElementsList;
        }


        private bool IsSvgElement(eElementType elementType)
        {
            return elementType switch
            {
                eElementType.Svg => true,
                _ => false
            };
        }

        private void ProcessSvgChildElements(ISearchContext searchContext,PomSetting pomSetting, HtmlNode svgHtmlNode, HTMLElementInfo svgElementInfo, string path, ObservableList<ElementInfo> foundElementsList, ObservableList<POMPageMetaData> PomMetaData, bool isShadowRootDetected = false, Bitmap ScreenShot = null)
        {
            try
            {
                // Get all child elements of the SVG
                var svgChildElements = svgHtmlNode.Descendants()
                    .Where(x => !x.Name.StartsWith('#') && IsSvgChildElement(x.Name));

                foreach (HtmlNode svgChildNode in svgChildElements)
                {
                    try
                    {
                        Tuple<string, eElementType> childElementType = GetElementTypeEnum(htmlNode: svgChildNode,TypeAtt:nameof(eElementType.Svg));

                        // Check if this SVG child element should be learned
                        if (ShouldLearnElement(pomSetting, childElementType.Item2))
                        {
                            // Create enhanced XPath for SVG child element
                            string svgChildXPath = CreateEnhancedSvgElementXPath(svgChildNode);

                            try
                            {
                                // Try to find the SVG child element (ShadowRoot doesn't support XPath)
                                IWebElement svgChildWebElement = null;
                                if (searchContext is ShadowRoot)
                                {
                                    string css = shadowDOM.ConvertXPathToCssSelector(svgChildXPath);
                                    svgChildWebElement = ((ShadowRoot)searchContext).FindElement(By.CssSelector(css));
                                }
                                else
                                {
                                    svgChildWebElement = searchContext.FindElement(By.XPath(svgChildXPath));
                                }
                                //skip svg child invisible elements
                                if (!IsElementVisible(svgChildWebElement))
                                {
                                    continue;
                                }
                                if (svgChildWebElement != null)
                                {
                                    HTMLElementInfo svgChildElementInfo = CreateHTMLElementInfo(
                                        svgChildWebElement,
                                        path,
                                        svgChildNode,
                                        childElementType.Item1,
                                        childElementType.Item2,
                                        svgElementInfo.Guid,
                                        pomSetting,
                                        foundElementsList.Count.ToString(),
                                        ScreenShot: ScreenShot
                                    );

                                    // Add SVG-specific locators and properties
                                    AddSvgSpecificLocators(svgChildElementInfo, svgChildNode);
                                    AddSvgSpecificProperties(svgChildElementInfo, svgChildNode);

                                    // Set parent SVG element
                                    svgChildElementInfo.Properties.Add(new ControlProperty
                                    {
                                        Name = "ParentSVGElement",
                                        Value = svgElementInfo.Guid.ToString(),
                                        ShowOnUI = false
                                    });

                                    foundElementsList.Add(svgChildElementInfo);
                                    allReadElem.Add(svgChildElementInfo);

                                    POMUtils.TriggerFineTuneWithAI(pomSetting, svgChildElementInfo, this.PomCategory);
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, $"Failed to locate SVG child element '{svgChildNode.Name}': {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Failed to process SVG child element '{svgChildNode.Name}': {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to process SVG child elements: {ex.Message}");
            }
        }

        private static readonly HashSet<string> SvgChildElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // Basic SVG shapes
            "rect", "path", "g", "text", "circle", "ellipse", "line", "polygon", "polyline",

            // SVG containers and groups
            "defs", "use", "symbol", "marker", "pattern", "clipPath", "mask", "image",
            "foreignObject", "switch", "title", "desc",

            // SVG text elements
            "tspan", "textPath",

            // SVG gradients and filters
            "linearGradient", "radialGradient", "stop", "filter", "feGaussianBlur",

            // SVG animations
            "animate", "animateTransform", "animateMotion"
        };

        private bool IsSvgChildElement(string elementName)
        {
            return SvgChildElements.Contains(elementName);
        }

        private string CreateEnhancedSvgElementXPath(HtmlNode svgElement)
        {
            // First try to create XPath using unique attributes
            string xpathWithAttributes = CreateSvgXPathWithAttributes(svgElement);
            if (!string.IsNullOrEmpty(xpathWithAttributes))
            {
                return xpathWithAttributes;
            }

            // Fallback to position-based XPath
            return CreateSvgElementXPath(svgElement);
        }

        private string CreateSvgElementXPath(HtmlNode svgElement)
        {
            // Build XPath using local-name() for SVG elements
            var pathParts = new List<string>();
            HtmlNode current = svgElement;

            while (current != null && current.Name != "#document")
            {
                string nodeName = current.Name.ToLower();

                if (IsSvgChildElement(nodeName) || nodeName == "svg")
                {
                    // Count siblings with same name
                    int position = 1;
                    var siblings = current.ParentNode?.ChildNodes
                        .Where(n => n.Name.Equals(current.Name, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (siblings?.Count > 1)
                    {
                        position = siblings.IndexOf(current) + 1;
                        pathParts.Insert(0, $"*[local-name()='{nodeName}'][{position}]");
                    }
                    else
                    {
                        pathParts.Insert(0, $"*[local-name()='{nodeName}']");
                    }
                }
                else
                {
                    // For non-SVG elements, use regular XPath
                    pathParts.Insert(0, current.Name.ToLower());
                }

                current = current.ParentNode;

                // Stop at SVG root or HTML element
                if (current?.Name.Equals("svg", StringComparison.OrdinalIgnoreCase) == true ||
                    current?.Name.Equals("html", StringComparison.OrdinalIgnoreCase) == true)
                {
                    break;
                }
            }

            return "//" + string.Join("/", pathParts);
        }

        private string CreateSvgXPathWithAttributes(HtmlNode svgElement)
        {
            string nodeName = svgElement.Name.ToLower();
            var conditions = new List<string>();

            // Priority order for SVG attributes
            var svgAttributes = new[] { "data-node-id", "data-backend-id", "data-parent-id", "class", "id", "transform", "href" };

            foreach (var attr in svgAttributes)
            {
                string value = svgElement.GetAttributeValue(attr, null);
                if (!string.IsNullOrEmpty(value))
                {
                    if (attr == "class")
                    {
                        // Handle multiple classes
                        var classes = value.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c));
                        foreach (var className in classes)
                        {
                            conditions.Add($"contains(@class, {EscapeXPathString(className)})");
                        }
                    }
                    else if (attr == "href" && value.StartsWith("#"))
                    {
                        conditions.Add($"@{attr}={EscapeXPathString(value)}");
                    }
                    else if (!IsLikelyDynamic(value))
                    {
                        conditions.Add($"@{attr}={EscapeXPathString(value)}");
                    }
                }
            }

            if (conditions.Count > 0)
            {
                string xpath = $"//*[local-name()='{nodeName}' and {string.Join(" and ", conditions)}]";
                return xpath;
            }

            return null;
        }

        private void AddSvgSpecificLocators(HTMLElementInfo elementInfo, HtmlNode svgNode)
        {
            // Add data-node-id locator
            string dataNodeId = svgNode.GetAttributeValue("data-node-id", null);
            if (!string.IsNullOrEmpty(dataNodeId))
            {
                elementInfo.Locators.Add(new ElementLocator
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = $"//*[local-name()='{svgNode.Name.ToLower()}' and @data-node-id='{EscapeXPathString(dataNodeId)}']",
                    IsAutoLearned = true,
                    Active = true
                });
            }

            // Add data-backend-id locator
            string dataBackendId = svgNode.GetAttributeValue("data-backend-id", null);
            if (!string.IsNullOrEmpty(dataBackendId))
            {
                elementInfo.Locators.Add(new ElementLocator
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = $"//*[local-name()='{svgNode.Name.ToLower()}' and @data-backend-id='{EscapeXPathString(dataBackendId)}']",
                    IsAutoLearned = true,
                    Active = true
                });
            }

            // Add class-based locator for SVG
            string className = svgNode.GetAttributeValue("class", null);
            if (!string.IsNullOrEmpty(className))
            {
                var classes = className.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c));
                foreach (var cls in classes.Take(2)) // Take first 2 classes to avoid overly complex locators
                {
                    elementInfo.Locators.Add(new ElementLocator
                    {
                        LocateBy = eLocateBy.ByXPath,
                        LocateValue = $"//*[local-name()='{svgNode.Name.ToLower()}' and contains(@class, '{EscapeXPathString(cls)}')]",
                        IsAutoLearned = true,
                        Active = false // Set as backup locator
                    });
                }
            }

            // Add relative XPath based on parent
            string relativeXPath = CreateSvgRelativeXPath(svgNode);
            if (!string.IsNullOrEmpty(relativeXPath))
            {
                elementInfo.Locators.Add(new ElementLocator
                {
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = relativeXPath,
                    IsAutoLearned = true,
                    Active = true
                });
            }
        }

        private string CreateSvgRelativeXPath(HtmlNode svgNode)
        {
            try
            {
                // Create relative XPath using parent context
                var parent = svgNode.ParentNode;
                if (parent != null)
                {
                    string parentSelector = "";
                    
                    // Try to find unique parent identifier
                    string parentClass = parent.GetAttributeValue("class", null);
                    string parentDataNodeId = parent.GetAttributeValue("data-node-id", null);
                    
                    if (!string.IsNullOrEmpty(parentDataNodeId))
                    {
                        parentSelector = $"*[@data-node-id='{EscapeXPathString(parentDataNodeId)}']";
                    }
                    else if (!string.IsNullOrEmpty(parentClass))
                    {
                        var firstClass = parentClass.Split(' ').FirstOrDefault();
                        if (!string.IsNullOrEmpty(firstClass))
                        {
                            parentSelector = $"*[contains(@class, '{EscapeXPathString(firstClass)}')]";
                        }
                    }

                    if (!string.IsNullOrEmpty(parentSelector))
                    {
                        string childSelector = svgNode.Name.ToLower();
                        string dataNodeId = svgNode.GetAttributeValue("data-node-id", null);
                        string className = svgNode.GetAttributeValue("class", null);

                        if (!string.IsNullOrEmpty(dataNodeId))
                        {
                            return $"//{parentSelector}//*[local-name()='{childSelector}' and @data-node-id='{EscapeXPathString(dataNodeId)}']";
                        }
                        else if (!string.IsNullOrEmpty(className))
                        {
                            var firstClass = className.Split(' ').FirstOrDefault();
                            if (!string.IsNullOrEmpty(firstClass))
                            {
                                return $"//{parentSelector}//*[local-name()='{childSelector}' and contains(@class, '{EscapeXPathString(firstClass)}')]";
                            }
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Failed to create SVG relative XPath: {ex.Message}");
                return null;
            }
        }

        private void AddSvgSpecificProperties(HTMLElementInfo elementInfo, HtmlNode svgNode)
        {
            // Add SVG-specific attributes as properties
            var svgSpecificAttributes = new[] 
            { 
                "data-node-id", "data-backend-id", "data-parent-id", 
                "transform", "href", "fill", "style", "text-anchor",
                "x", "y", "class"
            };

            foreach (var attr in svgSpecificAttributes)
            {
                string value = svgNode.GetAttributeValue(attr, null);
                if (!string.IsNullOrEmpty(value))
                {
                    elementInfo.Properties.Add(new ControlProperty
                    {
                        Name = $"svg-{attr}",
                        Value = value,
                        ShowOnUI = true
                    });
                }
            }

            // Add text content if available
            if (!string.IsNullOrEmpty(svgNode.InnerText?.Trim()))
            {
                elementInfo.Properties.Add(new ControlProperty
                {
                    Name = "svg-text",
                    Value = svgNode.InnerText.Trim(),
                    ShowOnUI = true
                });
            }

            // Add position information from transform attribute
            string transform = svgNode.GetAttributeValue("transform", null);
            if (!string.IsNullOrEmpty(transform))
            {
                var translateMatch = System.Text.RegularExpressions.Regex.Match(transform, @"translate\(([^)]+)\)");
                if (translateMatch.Success)
                {
                    elementInfo.Properties.Add(new ControlProperty
                    {
                        Name = "svg-translate",
                        Value = translateMatch.Groups[1].Value,
                        ShowOnUI = true
                    });
                }
            }
        }



        private bool ShouldLearnElement(PomSetting pomSetting, eElementType elementType)
        {
            if (pomSetting == null || pomSetting.FilteredElementType == null)
            {
                return true; // Learn all elements if no filtering is specified
            }
            return pomSetting.FilteredElementType.Any(x => x.ElementType.Equals(elementType));
        }

        // Method to retrieve the web element corresponding to the HTML node
        private IWebElement GetWebElement(ISearchContext parentContext, HtmlNode htmlElemNode, eElementType elementType, bool isShadowRootDetected)
        {
            string xpath = htmlElemNode.XPath;
            if (elementType == eElementType.Svg)
            {
                if (!isShadowRootDetected)
                {
                    xpath = string.Concat(htmlElemNode.ParentNode.XPath, "//*[local-name()='svg']");
                }
            }
            else
            {
                // For SVG child elements, use local-name() in XPath
                xpath = CreateSvgElementXPath(htmlElemNode);
            }

            return parentContext is ShadowRoot shadowRoot ? shadowRoot.FindElement(By.CssSelector(shadowDOM.ConvertXPathToCssSelector(xpath))) :
                                                            parentContext.FindElement(By.XPath(xpath));
        }

        // Method to check if an element is visible
        private bool IsElementVisible(IWebElement webElement)
        {
            if (!webElement.Displayed || webElement.Size.Width == 0 || webElement.Size.Height == 0)
            {
                if (webElement.GetCssValue("display").Equals("none", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
                else if (webElement.GetCssValue("width").Equals("auto") || webElement.GetCssValue("height").Equals("auto"))
                {
                    return false;
                }
            }
            return true;
        }

        // Method to create HTMLElementInfo object
        private HTMLElementInfo CreateHTMLElementInfo(IWebElement webElement, string path, HtmlNode htmlElemNode, string elementType, eElementType elementTypeEnum, Guid parentGUID, PomSetting pomSetting, string Sequence = "", Bitmap ScreenShot = null)
        {
            string xpath = htmlElemNode.XPath;
            var parentPOMGuid = parentGUID == Guid.Empty ? Guid.Empty.ToString() : parentGUID.ToString();
            HTMLElementInfo foundElementInfo = new HTMLElementInfo()
            {
                ElementType = elementType,
                ElementTypeEnum = elementTypeEnum,
                ElementObject = webElement,
                Path = path,
                HTMLElementObject = htmlElemNode,
                XPath = xpath,
                IsAutoLearned = true
            };


            ((IWindowExplorer)this).LearnElementInfoDetails(foundElementInfo, pomSetting);


            foundElementInfo.Properties.AddRange(new[]
             {
                new ControlProperty { Name = ElementProperty.ParentPOMGUID, Value = parentPOMGuid, ShowOnUI = false },
                new ControlProperty { Name = ElementProperty.Sequence, Value = Sequence, ShowOnUI = false }
             });


            if (ExtraLocatorsRequired)
            {
                GetRelativeXpathElementLocators(foundElementInfo);
                //relative xpath with robust one 
                GenerateRobustRelativeXPath(webElement, foundElementInfo);
                //relative xpath with Enhance one
                EnhanceElementLocators(foundElementInfo);
                if (pomSetting?.RelativeXpathTemplateList?.Count > 0)
                {
                    foreach (var template in pomSetting.RelativeXpathTemplateList)
                    {
                        CreateXpathFromUserTemplate(template.Value, foundElementInfo);
                    }
                }
            }
            // Element Screenshot only mapped elements
            if (pomSetting.LearnScreenshotsOfElements && pomSetting.FilteredElementType.Any(x => x.ElementType.Equals(foundElementInfo.ElementTypeEnum)))
            {
                foundElementInfo.ScreenShotImage = GingerCoreNET.GeneralLib.General.TakeElementScreenShot(foundElementInfo, ScreenShot);
            }
            return foundElementInfo;
        }

        // Method to process elements within shadow DOM
        private void ProcessShadowDOMElements(PomSetting pomSetting, IWebElement webElement, HTMLElementInfo foundElementInfo, string path, ObservableList<ElementInfo> foundElementsList, ObservableList<POMPageMetaData> PomMetaData, Bitmap ScreenShot = null)
        {
            ISearchContext shadowRoot = shadowDOM.GetShadowRootIfExists(webElement);
            if (shadowRoot == null)
            {
                return;
            }

            string innerHTML = shadowDOM.GetHTML(shadowRoot, Driver);
            if (!string.IsNullOrEmpty(innerHTML))
            {
                FindAllElementsFromPOM(path, pomSetting, shadowRoot, foundElementInfo.Guid, foundElementsList, PomMetaData, true, innerHTML, ScreenShot: ScreenShot);
            }
        }

        // Method to process iframe elements
        private void ProcessIframeElement(ISearchContext parentContext, HtmlNode htmlElemNode, string path, PomSetting pomSetting, Guid parentGUID, ObservableList<ElementInfo> foundElementsList, ObservableList<POMPageMetaData> PomMetaData, bool isShadowRootDetected = false, string pageSource = null, Bitmap ScreenShot = null)
        {
            string xpath = htmlElemNode.XPath;
            IWebElement webElement = parentContext is ShadowRoot shadowRoot ? shadowRoot.FindElement(By.CssSelector(shadowDOM.ConvertXPathToCssSelector(xpath))) :
                                                                                parentContext.FindElement(By.XPath(xpath));
            Driver.SwitchTo().Frame(webElement);
            string newPath = path == string.Empty ? xpath : path + "," + xpath;
            FindAllElementsFromPOM(newPath, pomSetting, parentContext, parentGUID, foundElementsList, PomMetaData, isShadowRootDetected, pageSource, ScreenShot: ScreenShot);
            Driver.SwitchTo().ParentFrame();
        }

        // Method to collect form elements
        private void CollectFormElements(List<HtmlNode> formElementsList, HtmlNode formElement)
        {
            formElementsList.Add(formElement);
        }

        // Method to process form elements and add metadata
        private void ProcessFormElements(List<HtmlNode> formElementsList, IWebDriver driver, PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList, ObservableList<POMPageMetaData> PomMetaData)
        {
            int pomActivityIndex = 1;
            foreach (HtmlNode formElement in formElementsList)
            {
                POMPageMetaData pomMetaData = new POMPageMetaData
                {
                    Type = POMPageMetaData.MetaDataType.Form,
                    Name = formElement.GetAttributeValue("name", string.Empty) != string.Empty ? formElement.GetAttributeValue("name", string.Empty) : formElement.GetAttributeValue("id", string.Empty)
                };

                if (string.IsNullOrEmpty(pomMetaData.Name))
                {
                    pomMetaData.Name = "POM Activity - " + driver.Title + " " + pomActivityIndex++;
                }
                else
                {
                    pomMetaData.Name += " " + driver.Title;
                }

                IEnumerable<HtmlNode> formInputElements = formElement.Descendants().Where(x => x.Name.StartsWith("input"));
                CreatePOMMetaData(foundElementsList, formInputElements.ToList(), pomMetaData, pomSetting);

                IEnumerable<HtmlNode> formButtonElements = formElement.Descendants().Where(x => x.Name.StartsWith("button"));
                CreatePOMMetaData(foundElementsList, formButtonElements.ToList(), pomMetaData, pomSetting);

                PomMetaData.Add(pomMetaData);
            }
        }

        private void CreatePOMMetaData(ObservableList<ElementInfo> foundElementsList, List<HtmlNode> formChildElements, POMPageMetaData pomMetaData, PomSetting pomSetting = null)
        {

            string radioButtoNameOrID = string.Empty;
            for (int i = 0; i < formChildElements.Count; i++)
            {
                HtmlNode formChildElement = formChildElements[i];
                IWebElement childElement = null;
                if (formChildElement.Attributes.Contains("type"))
                {
                    if (formChildElement.GetAttributeValue("type", "hidden") == "hidden")
                    {
                        continue;
                    }
                    // Add only one action for each radio group
                    if (formChildElement.GetAttributeValue("type", "radio") == "radio")
                    {
                        string radioName = formChildElement.GetAttributeValue("name", "") != null ? formChildElement.GetAttributeValue("name", "") : formChildElement.GetAttributeValue("id", "");

                        if (radioButtoNameOrID != radioName)
                        {
                            radioButtoNameOrID = radioName;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                childElement = Driver.FindElement(By.XPath(formChildElement.XPath));
                if (childElement == null)
                {
                    continue;
                }
                Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(htmlNode: formChildElement);
                HTMLElementInfo foundElemntInfo = new HTMLElementInfo
                {
                    ElementType = elementTypeEnum.Item1,
                    ElementTypeEnum = elementTypeEnum.Item2,
                    ElementObject = childElement,
                    Path = String.Empty,
                    XPath = formChildElement.XPath,
                    HTMLElementObject = formChildElement
                };

                ElementInfo matchingOriginalElement = ((IWindowExplorer)this).GetMatchingElement(foundElemntInfo, foundElementsList);
                if (matchingOriginalElement == null)
                {
                    ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo, pomSetting);
                    matchingOriginalElement = ((IWindowExplorer)this).GetMatchingElement(foundElemntInfo, foundElementsList);
                }

                if (!foundElementsList.Contains(matchingOriginalElement))
                {
                    foundElemntInfo.IsAutoLearned = true;
                    foundElementsList.Add(foundElemntInfo);
                    foundElemntInfo.Properties.Add(new ControlProperty() { Name = ElementProperty.Sequence, Value = foundElementsList.Count.ToString(), ShowOnUI = false });
                    matchingOriginalElement = ((IWindowExplorer)this).GetMatchingElement(foundElemntInfo, foundElementsList);
                }

                matchingOriginalElement.Properties.Add(new ControlProperty() { Name = ElementProperty.ParentFormId, Value = pomMetaData.Guid.ToString(), ShowOnUI = false });
            }
        }

        Regex AttRegex = new("@[a-zA-Z]*", RegexOptions.Compiled);
        private void CreateXpathFromUserTemplate(string xPathTemplate, HTMLElementInfo hTMLElement)
        {
            try
            {
                var relXpath = string.Empty;

                var attributeCount = 0;

                var attList = AttRegex.Matches(xPathTemplate);
                var strList = new List<string>();
                foreach (var item in attList)
                {
                    strList.Add(item.ToString().Remove(0, 1));
                }

                foreach (var item in hTMLElement.HTMLElementObject.Attributes)
                {
                    if (strList.Contains(item.Name))
                    {
                        relXpath = xPathTemplate.Replace(item.Name + "=\'\'", item.Name + "=\'" + item.Value + "\'");

                        xPathTemplate = relXpath;
                        attributeCount++;
                    }
                }

                if (relXpath != string.Empty && attributeCount == attList.Count && CheckElementLocateStatus(xPathTemplate))
                {
                    var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relXpath, IsAutoLearned = true };
                    hTMLElement.Locators.Add(elementLocator);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred during pom learning", ex);
            }
        }

        private void GetRelativeXpathElementLocators(HTMLElementInfo foundElemntInfo)
        {
            if (foundElemntInfo.ElementTypeEnum == eElementType.Svg)
            {
                return;
            }
            //relative xpath with multiple attribute and tagname
            var relxPathWithMultipleAtrrs = mXPathHelper.CreateRelativeXpathWithTagNameAndAttributes(foundElemntInfo);
            if (!string.IsNullOrEmpty(relxPathWithMultipleAtrrs) && CheckElementLocateStatus(relxPathWithMultipleAtrrs))
            {
                var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relxPathWithMultipleAtrrs, IsAutoLearned = true };
                foundElemntInfo.Locators.Add(elementLocator);
            }
            var innerText = foundElemntInfo.HTMLElementObject.InnerText;
            if (!string.IsNullOrEmpty(innerText))
            {
                //relative xpath with Innertext Exact Match
                var relXpathwithExactTextMatch = mXPathHelper.CreateRelativeXpathWithTextMatch(foundElemntInfo, isExactMatch: true);
                if (!string.IsNullOrEmpty(relXpathwithExactTextMatch) && CheckElementLocateStatus(relXpathwithExactTextMatch))
                {
                    var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relXpathwithExactTextMatch, IsAutoLearned = true };
                    foundElemntInfo.Locators.Add(elementLocator);
                    //relative xpath with Contains Innertext, skip checkelemtlocatestatus as it is already checked in exact match
                    var relXpathwithContainsText = mXPathHelper.CreateRelativeXpathWithTextMatch(foundElemntInfo, isExactMatch: false);
                    if (!string.IsNullOrEmpty(relXpathwithContainsText))
                    {
                        elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relXpathwithContainsText, IsAutoLearned = true };
                        foundElemntInfo.Locators.Add(elementLocator);
                    }
                }
            }

            //relative xpath with Sibling Text
            var relXpathwithSiblingText = mXPathHelper.CreateRelativeXpathWithSibling(foundElemntInfo);
            if (!string.IsNullOrEmpty(relXpathwithSiblingText) && CheckElementLocateStatus(relXpathwithSiblingText))
            {
                var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = relXpathwithSiblingText, IsAutoLearned = true };
                foundElemntInfo.Locators.Add(elementLocator);
            }

        }

        private bool CheckElementLocateStatus(string relXPath)
        {
            try
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 3);
                IWebElement webElement = Driver.FindElement(By.XPath(relXPath));
                if (webElement != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred when creating relative xpath with attributes values", ex);
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
            }
            return false;
        }

        public static Tuple<string, eElementType> GetElementTypeEnum(IWebElement el = null, string jsType = null, HtmlNode htmlNode = null, string TypeAtt = null)
        {
            Tuple<string, eElementType> returnTuple;
            eElementType elementType = eElementType.Unknown;
            string elementTagName = string.Empty;
            string elementTypeAtt = string.Empty;

            if ((el == null) && (jsType != null))
            {
                elementTagName = jsType;
                elementTypeAtt = !string.IsNullOrEmpty(TypeAtt) ? TypeAtt : string.Empty;
            }
            else if ((el != null) && (jsType == null))
            {
                if ((el.TagName != null) && (el.TagName != string.Empty))
                {
                    elementTagName = el.TagName.ToUpper();
                }
                else
                {
                    elementTagName = "INPUT";
                }

                elementTypeAtt = el.GetAttribute("type");
            }
            else if (htmlNode != null)
            {
                elementTagName = htmlNode.Name;
                if (htmlNode.Attributes.Any(x => x.Name == "type"))
                {
                    elementTypeAtt = htmlNode.Attributes["type"].Value;
                }
                else
                {
                    elementTypeAtt = !string.IsNullOrEmpty(TypeAtt) ? TypeAtt : string.Empty;
                }
            }
            else
            {
                returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);
                return returnTuple;
            }

            elementType = GetElementType(elementTagName, elementTypeAtt);

            returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);

            return returnTuple;
        }

        private static eElementType GetElementType(string elementTagName, string elementTypeAtt)//elementTag = g ,typeatt = svg
        {
            eElementType elementType;
            elementType = elementTagName.ToUpper() switch
            {
                "INPUT" => elementTypeAtt.ToUpper() switch
                {
                    "UNDEFINED" or "TEXT" or "PASSWORD" or "EMAIL" or "TEL" or "SEARCH" or "NUMBER" or "URL" or "DATE" => eElementType.TextBox,
                    "IMAGE" or "SUBMIT" or "BUTTON" => eElementType.Button,
                    "CHECKBOX" => eElementType.CheckBox,
                    "RADIO" => eElementType.RadioButton,
                    _ => eElementType.Unknown,
                },
                "TEXTAREA" or "TEXT" => eElementType.TextBox,
                "RESET" or "SUBMIT" or "BUTTON" => eElementType.Button,
                "TD" or "TH" or "TR" => eElementType.TableItem,
                "LINK" or "A" or "LI" => eElementType.HyperLink,
                "LABEL" or "TITLE" => eElementType.Label,
                "SELECT" or "SELECT-ONE" => eElementType.ComboBox,
                "TABLE" or "CAPTION" => eElementType.Table,
                "JEDITOR.TABLE" => eElementType.EditorPane,
                "DIV" => eElementType.Div,
                "SPAN" => eElementType.Span,
                "IMG" or "MAP" => eElementType.Image,
                "CHECKBOX" => eElementType.CheckBox,
                "OPTGROUP" or "OPTION" => eElementType.ComboBoxOption,
                "RADIO" => eElementType.RadioButton,
                "IFRAME" or "FRAME" or "FRAMESET" => eElementType.Iframe,
                "CANVAS" => eElementType.Canvas,
                "FORM" => eElementType.Form,
                "UL" or "OL" or "DL" => eElementType.List,
                "LI" or "DT" or "DD" => eElementType.ListItem,
                "MENU" => eElementType.MenuBar,
                "H1" or "H2" or "H3" or "H4" or "H5" or "H6" or "P" => eElementType.Text,
                "SVG" => eElementType.Svg,
                "G" or "PATH" or "RECT" or "CIRCLE" or "ELLIPSE" or "LINE" or "POLYGON" or "POLYLINE" or "USE" when !string.IsNullOrEmpty(elementTypeAtt) && elementTypeAtt.Equals(nameof(eElementType.Svg),StringComparison.InvariantCultureIgnoreCase) => eElementType.Svg,

               _ => eElementType.Unknown,
            };
            return elementType;
        }

        ElementInfo IWindowExplorer.LearnElementInfoDetails(ElementInfo EI, PomSetting pomSetting)
        {
            if (string.IsNullOrEmpty(EI.ElementType) || EI.ElementTypeEnum == eElementType.Unknown)
            {
                Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(EI.ElementObject as IWebElement);
                EI.ElementType = elementTypeEnum.Item1;
                EI.ElementTypeEnum = elementTypeEnum.Item2;
            }


            if (pomSetting == null)
            {
                IList<string> XPaths = [];

                if (string.IsNullOrWhiteSpace(EI.Path) || (EI.Path.Split('/')[^1].Contains("frame") || EI.Path.Split('/')[^1].Contains("iframe")))
                {
                    EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, string.Empty, XPaths);
                }
                else
                {
                    EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, EI.Path, XPaths);
                }

                ((HTMLElementInfo)EI).XPathList = XPaths;
            }

            else
            {
                if ((string.IsNullOrEmpty(EI.XPath) || EI.XPath == "/") && EI.ElementObject != null)
                {
                    if (string.IsNullOrWhiteSpace(EI.Path) || (EI.Path.Split('/')[^1].Contains("frame") || EI.Path.Split('/')[^1].Contains("iframe")))
                    {
                        EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, string.Empty);
                    }
                    else
                    {
                        EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, EI.Path);
                    }
                }
            }

            EI.ElementName = GetElementName(EI as HTMLElementInfo);
            if (mXPathHelper == null)
            {
                InitXpathHelper();
            }

            ((HTMLElementInfo)EI).RelXpath = mXPathHelper.GetElementRelXPath(EI, pomSetting);
            EI.Locators = ((IWindowExplorer)this).GetElementLocators(EI, pomSetting);
            if (EI.Locators.Any(x => x.EnableFriendlyLocator))
            {
                EI.FriendlyLocators = ((IWindowExplorer)this).GetElementFriendlyLocators(EI, pomSetting);
            }
            EI.Properties = ((IWindowExplorer)this).GetElementProperties(EI);// improve code inside

            if (EI.FriendlyLocators.Count == 0 && EI.Locators.Count >= 1)
            {
                ElementLocator byTagNameLocator = EI.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByTagName);
                if (byTagNameLocator != null)
                {
                    byTagNameLocator.Active = false;
                }
            }

            return EI;
        }

        string GetElementName(HTMLElementInfo EI)
        {
            string bestElementName = string.Empty;

            if (EI.HTMLElementObject != null)
            {
                string tagName = EI.HTMLElementObject.Name;
                string name = string.Empty;
                string title = string.Empty;
                string id = string.Empty;
                string value = string.Empty;
                string type = string.Empty;
                foreach (HtmlAttribute att in EI.HTMLElementObject.Attributes)
                {
                    if (att.Name == "name")
                    {
                        name = att.Value;
                    }
                    else if (att.Name == "title")
                    {
                        title = att.Value;
                    }
                    else if (att.Name == "type")
                    {
                        type = att.Value;
                    }
                    else if (att.Name == "id")
                    {
                        id = att.Value;
                    }
                    else if (att.Name == "value")
                    {
                        value = att.Value;
                    }
                }

                string text = "";

                if (EI.ElementObject != null)
                {
                    text = ((IWebElement)EI.ElementObject).Text;
                }

                if (text.Length > 15)
                {
                    text = string.Empty;
                }

                List<string> list = [tagName, text, type, name, title, title, id, value];

                foreach (string att in list)
                {
                    if (!string.IsNullOrEmpty(att) && !bestElementName.Contains(att))
                    {
                        bestElementName = bestElementName + " " + att;
                    }
                }


            }
            else
            {
                if (string.IsNullOrEmpty(EI.Value))
                {
                    if (!string.IsNullOrEmpty(EI.ElementTypeEnumDescription))
                    {
                        return EI.ElementTypeEnumDescription;
                    }
                    else
                    {
                        return null;
                    }

                }
            }

            return bestElementName;
        }

        private ElementInfo GetElementInfoWithIWebElementWithXpath(IWebElement el, string path)
        {
            HTMLElementInfo EI = new HTMLElementInfo
            {
                ElementTitle = GenerateElementTitle(el),
                WindowExplorer = this,
                ID = GenerateElementID(el),
                Value = GenerateElementValue(el),
                Name = GenerateElementName(el),
                ElementType = GenerateElementType(el),
                ElementTypeEnum = GetElementTypeEnum(el).Item2,
                Path = path,
                ElementObject = el,
                XPath = GenerateXpathForIWebElement(el, "")
            };
            return EI;
        }

        private ElementInfo GetRootElement()
        {
            ElementInfo RootEI = new ElementInfo
            {
                ElementTitle = "html",
                ElementType = "root",
                Value = string.Empty,
                Path = string.Empty,
                XPath = "html"
            };
            return RootEI;
        }

        private void SwitchFrameFromCurrent(ElementInfo ElementInfo)
        {
            string[] spliter = ["/"];
            string[] elementsTypesPath = ElementInfo.XPath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[^1];

            int index = elementType.IndexOf('[');
            if (index != -1)
            {
                elementType = elementType[..index];
            }

            if (CurrentFrame != ElementInfo.Path && !(elementType == "iframe" || elementType == "frame"))
            {
                Driver.SwitchTo().DefaultContent();
                SwitchAllFramePathes(ElementInfo);
                CurrentFrame = ElementInfo.Path;
            }
            else if (CurrentFrame == ElementInfo.Path && (elementType == "iframe" || elementType == "frame"))
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(ElementInfo.XPath)));
                if (string.IsNullOrEmpty(CurrentFrame))
                {
                    CurrentFrame = ElementInfo.XPath;
                }
                else
                {
                    CurrentFrame = CurrentFrame + "," + ElementInfo.XPath;
                }
            }
            else if (CurrentFrame != ElementInfo.Path && (elementType == "iframe" || elementType == "frame"))
            {
                Driver.SwitchTo().DefaultContent();
                SwitchAllFramePathes(ElementInfo);
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(ElementInfo.XPath)));
                CurrentFrame = ElementInfo.Path + "," + ElementInfo.XPath;
            }
        }

        private void SwitchAllFramePathes(ElementInfo ElementInfo)
        {
            string[] spliter = [","];
            string[] iframesPathes = ElementInfo.Path.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            foreach (string iframePath in iframesPathes)
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(iframePath)));
            }
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            try
            {
                allReadElem.Clear();
                allReadElem.Add(ElementInfo);
                List<ElementInfo> list = [];
                ReadOnlyCollection<IWebElement> el;
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                Driver.SwitchTo().DefaultContent();
                SwitchFrame(ElementInfo.Path, ElementInfo.XPath);
                string elementPath = GeneratePath(ElementInfo.XPath);
                el = Driver.FindElements(By.XPath(elementPath));
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan();
                list = GetElementsFromIWebElementList(el, ElementInfo.Path, ElementInfo.XPath);
                return list;
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
            }
        }

        private string GeneratePath(string xpath)
        {
            string[] spliter = [","];
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[^1];

            string path = string.Empty;
            int index = elementType.IndexOf('[');
            if (index != -1)
            {
                elementType = elementType[..index];
            }

            if (elementType is "iframe" or "frame")
            {
                path = "/html/*";
            }
            else
            {
                path = xpath + "/*";
            }

            return path;
        }

        private void SwitchFrame(string path, string xpath, bool otherThenGetElementChildren = false)
        {
            string elementType = string.Empty;
            if (!string.IsNullOrEmpty(xpath))
            {
                string[] xpathSplitter = new string[] { "/" };
                string[] elementsTypesPath = xpath.Split(xpathSplitter, StringSplitOptions.RemoveEmptyEntries);
                if (elementsTypesPath.Length == 0)
                {
                    return;
                }
                elementType = elementsTypesPath[^1];

                int index = elementType.IndexOf('[');
                if (index != -1)
                {
                    elementType = elementType[..index];
                }
            }

            if ((elementType == "iframe" || elementType == "frame") && string.IsNullOrEmpty(path) && !otherThenGetElementChildren)
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(xpath)));
                return;
            }

            if (path != null)
            {
                string[] spliter = [","];
                string[] iframesPathes = path.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string iframePath in iframesPathes)
                {
                    Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(iframePath)));
                }
            }

            if ((elementType == "iframe" || elementType == "frame") && !otherThenGetElementChildren)
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(xpath)));
                return;
            }
        }

        private List<ElementInfo> GetElementsFromIWebElementList(ReadOnlyCollection<IWebElement> ElementsList, string path, string xpath)
        {
            List<ElementInfo> list = [];
            Dictionary<string, int> ElementsIndexes = [];
            Dictionary<string, int> ElementsCount = [];

            if (string.IsNullOrEmpty(path))
            {
                path = string.Empty;
            }

            foreach (IWebElement EL in ElementsList)
            {

                if (!ElementsCount.ContainsKey(EL.TagName))
                {
                    ElementsCount.Add(EL.TagName, 1);
                }
                else
                {
                    ElementsCount[EL.TagName] += 1;
                }
            }

            foreach (IWebElement EL in ElementsList)
            {
                if (!ElementsIndexes.ContainsKey(EL.TagName))
                {
                    ElementsIndexes.Add(EL.TagName, 0);
                }
                else
                {
                    ElementsIndexes[EL.TagName] += 1;
                }

                HTMLElementInfo EI = new HTMLElementInfo
                {
                    ElementObject = EL,
                    ElementTitle = GenerateElementTitle(EL),
                    WindowExplorer = this,
                    Name = GenerateElementName(EL),
                    ID = GenerateElementID(EL),
                    Value = GenerateElementValue(EL),
                    Path = GenetratePath(path, xpath, EL.TagName),
                    XPath = GenerateXpath(path, xpath, EL.TagName, ElementsIndexes[EL.TagName], ElementsCount[EL.TagName]), /*EI.GetAbsoluteXpath(); */
                    ElementType = GenerateElementType(EL),
                    ElementTypeEnum = GetElementTypeEnum(EL).Item2
                };
                EI.RelXpath = mXPathHelper.GetElementRelXPath(EI);
                list.Add(EI);
            }
            return list;
        }

        private string GenerateRealXpath(IWebElement EL)
        {
            string xpath = string.Empty;
            string tagName = EL.TagName;
            string id = EL.GetAttribute("id");
            if (string.IsNullOrEmpty(id))
            {
                xpath = "//" + tagName + "[@id=\'" + id + "\']" + xpath;
                return xpath;
            }

            string text = EL.GetAttribute("text");
            if (string.IsNullOrEmpty(text))
            {
                xpath = "//" + tagName + "[@id=\'" + text + "\']" + xpath;
                return xpath;
            }
            return string.Empty;
        }

        private string GenerateElementName(IWebElement EL)
        {
            string name = EL.TagName;
            if (string.IsNullOrEmpty(name))
            {
                name = EL.GetAttribute("name");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = string.Empty;
            }

            return name;
        }

        private string GenerateElementID(object EL)
        {
            string id = EL is IWebElement ? ((IWebElement)EL).GetAttribute("id") : ((HtmlNode)EL).GetAttributeValue("id", "");


            if (string.IsNullOrEmpty(id))
            {
                return string.Empty;
            }
            return id;
        }

        private string GenetratePath(string path, string xpath, string tagName)
        {
            string[] spliter = ["/"];
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[^1];

            int index = elementType.IndexOf('[');
            if (index != -1)
            {
                elementType = elementType[..index];
            }

            if (elementType is "iframe" or "frame")
            {
                if (!string.IsNullOrEmpty(path))
                {
                    return path + "," + xpath;
                }
                else
                {
                    return xpath;
                }
            }
            else
            {
                return path;
            }
        }

        private string GenerateXpath(string path, string xpath, string tagName, int id, int totalSameTags)
        {
            string[] spliter = ["/"];
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[^1];

            int index = elementType.IndexOf('[');
            if (index != -1)
            {
                elementType = elementType[..index];
            }

            string returnXPath = string.Empty;
            if (elementType is "iframe" or "frame")
            {
                xpath = "html";
            }

            id += 1;
            returnXPath = xpath + "/" + tagName + "[" + id + "]";
            return returnXPath;
        }

        private string GenerateElementTitle(IWebElement EL)
        {
            string tagName = EL.TagName;
            string name = EL.GetAttribute("name");
            string id = EL.GetAttribute("id");
            string value = EL.GetAttribute("value");

            if (tagName.Equals("TABLE", StringComparison.CurrentCultureIgnoreCase))
            {
                return "Table";
            }

            if (!string.IsNullOrEmpty(name))
            {
                return name + " " + tagName;
            }

            if (!string.IsNullOrEmpty(id))
            {
                return id + " " + tagName;
            }

            if (!string.IsNullOrEmpty(value))
            {
                return GetShortName(value) + " " + tagName;
            }

            return tagName;
        }

        private string GetShortName(string value)
        {
            string returnString = value;
            if (value.Length > 50)
            {
                returnString = value[..50] + "...";
            }

            return returnString;
        }

        private string GenerateElementValue(IWebElement EL)
        {
            string ElementValue = string.Empty;
            string tagName = EL.TagName;
            string text = EL.Text;
            string type = EL.GetAttribute("type");
            if (tagName == "select")
            {
                return "set to " + GetSelectedValue(EL);
            }
            if (tagName == "span")
            {
                return "set to " + text;
            }
            if (tagName == "input" && type == "checkbox")
            {
                return "set to " + EL.Selected.ToString();
            }
            else
            {
                string value = EL.GetAttribute("value");
                if (value == null || tagName == "button")
                {
                    ElementValue = EL.Text;
                }
                else
                {
                    ElementValue = value;
                }
            }
            return ElementValue;
        }

        private string GetSelectedValue(IWebElement EL)
        {
            SelectElement selectList = new SelectElement(EL);
            IList<IWebElement> options = selectList.Options;
            foreach (IWebElement option in options)
            {
                if (option.Selected)
                {
                    return option.Text;
                }
            }
            return "";
        }

        private string GenerateElementType(IWebElement EL)
        {
            string elementType = string.Empty;
            string tagName = EL.TagName;
            string type = EL.GetAttribute("type");
            if (tagName == "input")
            {
                elementType = tagName + "." + type;
            }
            else if (tagName is "a" or "li")
            {
                elementType = "link";
            }
            else
            {
                elementType = tagName;
            }

            return elementType.ToUpper();
        }



        void IWindowExplorer.SwitchWindow(string Title)
        {
            UnhighlightLast();
            String currentWindow;
            currentWindow = Driver.CurrentWindowHandle;
            bool windowfound = false;
            ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
            foreach (String winHandle in openWindows)
            {
                try
                {
                    if (!winHandle.Equals(currentWindow))
                    {
                        Driver.SwitchTo().Window(winHandle);
                    }
                    string winTitle = Driver.Title;
                    if (winTitle == Title)
                    {
                        windowfound = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    var wt = Driver.Title; //if Switch window throw exception then reading current driver title to avoid exception for next window handle in loop
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred during Switchwindow", ex);
                }

            }
            if (!windowfound)
            {
                Driver.SwitchTo().Window(currentWindow);
            }
        }



        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false, IList<ElementInfo> MappedUIElements = null)
        {

            HighlightElement(ElementInfo, locateElementByItLocators, MappedUIElements);
        }

        private void HighlightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false, IList<ElementInfo> AllPomElementInfo = null)
        {
            try
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                UnhighlightLast();

                Driver.SwitchTo().DefaultContent();
                if (!string.IsNullOrEmpty(ElementInfo.Path))
                {
                    SwitchFrame(ElementInfo);
                }
                else
                {
                    SwitchFrame(ElementInfo.Path, ElementInfo.XPath, true);
                }


                //Find element 
                if (locateElementByItLocators)
                {
                    ElementInfo.ElementObject = LocateElementByLocators(ElementInfo, AllPomElementInfo);
                }
                else
                {
                    if (string.IsNullOrEmpty(ElementInfo.XPath))
                    {
                        ElementInfo.XPath = GenerateXpathForIWebElement((IWebElement)ElementInfo.ElementObject, "");
                    }
                    if (ElementInfo is HTMLElementInfo htmlElementInfo && string.IsNullOrEmpty(htmlElementInfo.RelXpath))
                    {
                        htmlElementInfo.RelXpath = mXPathHelper.GetElementRelXPath(ElementInfo);
                    }
                    if (!string.IsNullOrEmpty(ElementInfo.XPath))
                    {

                        if (((HTMLElementInfo)ElementInfo).XPathList != null)
                        {
                            IList<string> XPaths = ((HTMLElementInfo)ElementInfo).XPathList;
                            ISearchContext tempContext = Driver;
                            int startPointer = XPaths.Count - 1;

                            while (startPointer > 0 && tempContext != null)
                            {
                                tempContext = tempContext.FindElement(By.XPath(XPaths[startPointer]));
                                tempContext = shadowDOM.GetShadowRootIfExists(tempContext);
                                startPointer--;
                            }
                            if (tempContext != null)
                            {
                                if (tempContext is ShadowRoot)
                                {
                                    string cssSelector = shadowDOM.ConvertXPathToCssSelector(ElementInfo.XPath);
                                    ElementInfo.ElementObject = tempContext.FindElement(By.CssSelector(cssSelector));
                                }
                                else
                                {
                                    ElementInfo.ElementObject = tempContext.FindElement(By.XPath(ElementInfo.XPath));

                                }
                            }
                        }
                        else
                        {
                            ElementInfo.ElementObject = Driver.FindElement(By.XPath(ElementInfo.XPath));
                        }
                    }
                }
                IWebElement webElement = (IWebElement)ElementInfo.ElementObject;

                if (webElement == null)
                {
                    return;
                }
                //Highlight element
                IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)Driver;


                foreach (string attribuet in HighlightStyleList)
                {
                    javascriptDriver.ExecuteScript(attribuet, new object[] { webElement });
                }

                LastHighLightedElement = webElement;

            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
            }

        }


        void IWindowExplorer.UnHighLightElements()
        {
            UnhighlightLast();
        }

        public void UnhighlightLast()
        {
            try
            {
                if (LastHighLightedElement != null)
                {
                    //ElementInfo elementInfo = GetElementInfoWithIWebElement(LastHighLightedElement, null, string.Empty);
                    List<string> attributesList = ["arguments[0].style.outline=''", "arguments[0].style.backgroundColor=''", "arguments[0].style.border=''"];
                    IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)Driver;
                    foreach (string attribuet in attributesList)
                    {
                        javascriptDriver.ExecuteScript(attribuet, [LastHighLightedElement]);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "failed to unhighlight object", ex);
            }
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            try
            {
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
                ObservableList<ControlProperty> list = new();

                IWebElement el = GetOrFindElement(ElementInfo);
                if (el == null) return list;

                AddBasicProperties(ElementInfo, el, list);
                AddSizeAndPosition(ElementInfo, el, list);
                AddStateProperties(el, list);
                AddOptionalValues(ElementInfo, el, list);
                AddHtmlAttributes(el, list);
                AddCssProperties(el, list);
                AddBoundingClientRect(el, list);

                return list;
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWait);
            }
        }

        private IWebElement GetOrFindElement(ElementInfo ei)
        {
            if (ei.ElementObject is IWebElement el && el != null)
                return el;

            if (ei != null && string.IsNullOrEmpty(ei.XPath))
            {
                ei.XPath = GenerateXpathForIWebElement((IWebElement)ei.ElementObject, "");
            }


            el = Driver.FindElement(By.XPath(ei.XPath));
            ei.ElementObject = el;
            return el;
        }

        private void AddBasicProperties(ElementInfo ei, IWebElement el, ObservableList<ControlProperty> list)
        {
            if (!string.IsNullOrWhiteSpace(ei.ElementType))
                list.Add(new ControlProperty() { Name = ElementProperty.PlatformElementType, Value = ei.ElementType });

            list.Add(new ControlProperty() { Name = ElementProperty.ElementType, Value = ei.ElementTypeEnum.ToString() });

            if (!string.IsNullOrWhiteSpace(ei.Path))
                list.Add(new ControlProperty() { Name = ElementProperty.ParentIFrame, Value = ei.Path });

            if (!string.IsNullOrWhiteSpace(ei.XPath))
                list.Add(new ControlProperty() { Name = ElementProperty.XPath, Value = ei.XPath });

            if (!string.IsNullOrWhiteSpace(((HTMLElementInfo)ei).RelXpath))
                list.Add(new ControlProperty() { Name = ElementProperty.RelativeXPath, Value = ((HTMLElementInfo)ei).RelXpath });

            if (!string.IsNullOrWhiteSpace(ei.Value))
                list.Add(new ControlProperty() { Name = ElementProperty.Value, Value = ei.Value });
        }

        private void AddSizeAndPosition(ElementInfo ei, IWebElement el, ObservableList<ControlProperty> list)
        {
            var size = el.Size;
            var location = el.Location;

            ei.Height = size.Height;
            ei.Width = size.Width;
            ei.X = location.X;
            ei.Y = location.Y;


            AddIfNotEmpty(list, ElementProperty.Height, ei.Height.ToString());
            AddIfNotEmpty(list, ElementProperty.Width, ei.Width.ToString());
            AddIfNotEmpty(list, ElementProperty.X, ei.X.ToString());
            AddIfNotEmpty(list, ElementProperty.Y, ei.Y.ToString());

        }
        private void AddStateProperties(IWebElement el, ObservableList<ControlProperty> list)
        {
            AddIfNotEmpty(list, "TagName", el.TagName);
            AddIfNotEmpty(list, "Displayed", el.Displayed.ToString());
            AddIfNotEmpty(list, "Enabled", el.Enabled.ToString());
            if (el.GetAttribute("type") == "checkbox" || el.GetAttribute("type") == "radio")
            {
                AddIfNotEmpty(list, "Selected", el.Selected.ToString());
            }
            AddIfNotEmpty(list, "Text", el.Text);
        }


        private void AddOptionalValues(ElementInfo ei, IWebElement el, ObservableList<ControlProperty> list)
        {
            if (!ElementInfo.IsElementTypeSupportingOptionalValues(ei.ElementTypeEnum)) return;
            var htmlElementObject = ((HTMLElementInfo)ei).HTMLElementObject;
            if (htmlElementObject == null) return;
            foreach (HtmlNode childNode in htmlElementObject.ChildNodes)
            {
                if (childNode.NodeType != HtmlNodeType.Text && !string.IsNullOrEmpty(childNode.InnerText))
                {
                    var tempOpVals = childNode.InnerText
                        .Split('\n')
                        .Select(line => line.Trim().Replace("\r", ""))
                        .Where(line => !string.IsNullOrEmpty(line));
                    foreach (var cuVal in tempOpVals)
                    {
                        ei.OptionalValuesObjectsList.Add(new OptionalValue() { Value = cuVal, IsDefault = false });
                    }
                }
            }

            if (ei.OptionalValuesObjectsList.Count > 0)
            {
                ei.OptionalValuesObjectsList[0].IsDefault = true;
                list.Add(new ControlProperty() { Name = "Optional Values", Value = ei.OptionalValuesObjectsListAsString.Replace("*", "") });
            }
        }

        private void AddHtmlAttributes(IWebElement el, ObservableList<ControlProperty> list)
        {
            var js = (IJavaScriptExecutor)Driver;
            var attributes = js.ExecuteScript(@"
        var items = {};
        for (var i = 0; i < arguments[0].attributes.length; ++i) {
            items[arguments[0].attributes[i].name] = arguments[0].attributes[i].value;
        }
        return items;", el) as Dictionary<string, object>;

            foreach (var kvp in attributes)
            {
                var value = kvp.Value?.ToString();
                if (kvp.Key != "style" && !string.IsNullOrEmpty(value) && !value.Contains("dashed red"))
                {
                    list.Add(new ControlProperty() { Name = kvp.Key, Value = kvp.Value.ToString() });
                }
            }
        }

        private void AddCssProperties(IWebElement el, ObservableList<ControlProperty> list)
        {
            var js = (IJavaScriptExecutor)Driver;
            var cssProps = new[] { "color", "background-color", "font-size", "display", "visibility", "z-index" };

            foreach (var prop in cssProps)
            {
                var value = js.ExecuteScript($"return window.getComputedStyle(arguments[0]).getPropertyValue('{prop}');", el)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    list.Add(new ControlProperty() { Name = $"CSS: {prop}", Value = value?.ToString() });
                }

            }
        }

        private void AddBoundingClientRect(IWebElement el, ObservableList<ControlProperty> list)
        {
            var js = (IJavaScriptExecutor)Driver;
            var rect = js.ExecuteScript("return arguments[0].getBoundingClientRect();", el) as Dictionary<string, object>;

            foreach (var kvp in rect)
            {
                var value = kvp.Value?.ToString();
                // Exclude 'toJson' method and empty values
                if (!kvp.Key.Equals("toJson", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(value))
                {
                    list.Add(new ControlProperty() { Name = $"BoundingRect: {kvp.Key}", Value = kvp.Value.ToString() });
                }
            }
        }


        private static void LearnPropertiesFromHtmlElementObject(ElementInfo ElementInfo, ObservableList<ControlProperty> list)
        {
            var htmlElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject;

            if (ElementInfo.IsElementTypeSupportingOptionalValues(ElementInfo.ElementTypeEnum))
            {
                foreach (HtmlNode childNode in htmlElementObject.ChildNodes)
                {
                    if (!childNode.Name.StartsWith('#') && !string.IsNullOrEmpty(childNode.InnerText))
                    {
                        var tempOpVals = childNode.InnerText
                            .Split('\n')
                            .Where(f => !string.IsNullOrEmpty(f.Trim()) && !f.Trim().Equals('\r'))
                            .Select(g => g.Trim().Replace("\r", ""));

                        foreach (string cuVal in tempOpVals)
                        {
                            ElementInfo.OptionalValuesObjectsList.Add(new OptionalValue() { Value = cuVal, IsDefault = false });
                        }
                    }
                }

                if (ElementInfo.OptionalValuesObjectsList.Count > 0)
                {
                    ElementInfo.OptionalValuesObjectsList[0].IsDefault = true;
                    list.Add(new ControlProperty()
                    {
                        Name = ElementProperty.OptionalValues,
                        Value = ElementInfo.OptionalValuesObjectsListAsString.Replace("*", "")
                    });
                }
            }

            HtmlAttributeCollection htmlAttributes = htmlElementObject.Attributes;
            foreach (HtmlAttribute htmlAttribute in htmlAttributes)
            {
                ControlProperty existControlProperty = list.FirstOrDefault(x => x.Name == htmlAttribute.Name && x.Value == htmlAttribute.Value);
                if (existControlProperty == null)
                {
                    ControlProperty controlProperty = new ControlProperty() { Name = htmlAttribute.Name, Value = htmlAttribute.Value };
                    list.Add(controlProperty);
                }
            }

            if (!string.IsNullOrEmpty(htmlElementObject.InnerText) && ElementInfo.OptionalValues.Count == 0 && htmlElementObject.ChildNodes.Count == 0)
            {
                list.Add(new ControlProperty() { Name = ElementProperty.InnerText, Value = htmlElementObject.InnerText.ToString() });
            }

        }


        private void AddIfNotEmpty(ObservableList<ControlProperty> list, string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                list.Add(new ControlProperty() { Name = name, Value = value });
            }
        }


        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            IList<string> XPaths = ((HTMLElementInfo)ElementInfo).XPathList;
            ISearchContext tempContext = Driver;
            int startPointer = 0;
            if (XPaths != null)
            {
                startPointer = XPaths.Count - 1;
            }
            IWebElement e = null;

            if (startPointer > 0)
            {
                while (startPointer > 0 && tempContext != null)
                {
                    tempContext = tempContext.FindElement(By.XPath(XPaths[startPointer]));
                    tempContext = shadowDOM.GetShadowRootIfExists(tempContext);
                    startPointer--;
                }
                if (tempContext != null)
                {
                    if (tempContext is ShadowRoot)
                    {
                        string cssSelector = shadowDOM.ConvertXPathToCssSelector(ElementInfo.XPath);
                        e = tempContext.FindElement(By.CssSelector(cssSelector));
                    }
                    else
                    {
                        e = tempContext.FindElement(By.XPath(ElementInfo.XPath));
                    }
                }
            }
            else
            {
                e = Driver.FindElement(By.XPath(ElementInfo.XPath));
            }

            string tagName = e?.TagName ?? string.Empty;
            if (tagName == "select")  // combo box
            {
                return GetComboValues(ElementInfo);
            }
            if (tagName == "table")  // Table
            {
                return GetTableData(ElementInfo);
            }
            if (tagName == "canvas")
            {
                ((SeleniumDriver)ElementInfo.WindowExplorer).InjectGingerLiveSpyAndStartClickEvent(ElementInfo);
                return GetXAndYpointsfromClickEvent(ElementInfo);
            }
            return null;
        }

        private object GetTableData(ElementInfo ElementInfo)
        {
            IWebElement table = Driver.FindElement(By.XPath(ElementInfo.XPath));
            DataTable dt = new DataTable("data");
            //Create headers          /
            //assume we have one header tr, so get all TDs, if we have more than one TR in THead, then need to adjust
            ReadOnlyCollection<IWebElement> HeaderTDs = table.FindElement(By.TagName("tr")).FindElements(By.TagName("td"));
            ReadOnlyCollection<IWebElement> HeaderTHs = table.FindElement(By.TagName("tr")).FindElements(By.TagName("th"));
            foreach (IWebElement cell in HeaderTDs)
            {
                dt.Columns.Add(cell.Text);
            }
            foreach (IWebElement cell in HeaderTHs)
            {
                dt.Columns.Add(cell.Text);
            }
            //Create the data rows
            ReadOnlyCollection<IWebElement> allRows = table.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr"));
            foreach (IWebElement row in allRows)
            {
                ReadOnlyCollection<IWebElement> Cells = row.FindElements(By.TagName("td"));
                ReadOnlyCollection<IWebElement> BoldCells = row.FindElements(By.TagName("th"));
                object[] rowdata = new object[Cells.Count + BoldCells.Count];
                int counter = 0;
                foreach (IWebElement Cell in Cells)
                {
                    rowdata[counter] = Cell.Text;
                    counter++;
                }

                foreach (IWebElement BoldCell in BoldCells)
                {
                    rowdata[counter] = BoldCell.Text;
                    counter++;
                }
                dt.Rows.Add(rowdata);
            }
            return dt;
        }

        private object GetComboValues(ElementInfo ElementInfo)
        {
            List<ComboBoxElementItem> ComboValues = [];
            IWebElement e = Driver.FindElement(By.XPath(ElementInfo.XPath));
            SelectElement se = new SelectElement(e);
            IList<IWebElement> options = se.Options;
            foreach (IWebElement o in options)
            {
                ComboValues.Add(new ComboBoxElementItem() { Value = o.GetAttribute("value"), Text = o.Text });
            }
            return ComboValues;
        }

        /// <summary>
        /// Extracts a list of potential locators for a given web element using various strategies.
        /// </summary>
        /// <param name="ElementInfo">The element metadata containing DOM and runtime information.</param>
        /// <param name="pomSetting">Optional POM settings to control locator preferences.</param>
        /// <returns>A list of auto-learned element locators.</returns>
        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            ObservableList<ElementLocator> locatorsList = new Platforms.PlatformsInfo.WebPlatform().GetLearningLocators();
            IWebElement e = null;

            if (ElementInfo.ElementObject != null)
            {
                e = (IWebElement)ElementInfo.ElementObject;
            }
            else
            {
                e = Driver.FindElement(By.XPath(((HTMLElementInfo)ElementInfo).HTMLElementObject?.XPath));
                ElementInfo.ElementObject = e;
            }

            foreach (ElementLocator elemLocator in locatorsList)
            {
                switch (elemLocator.LocateBy)
                {
                    case eLocateBy.ByID:
                        string id = ((HTMLElementInfo)ElementInfo).HTMLElementObject?.Attributes
                            .FirstOrDefault(x => x.Name == "id")?.Value ?? ((HTMLElementInfo)ElementInfo).ID ?? e?.GetAttribute("id");
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            elemLocator.LocateValue = id;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByID, elemLocator);
                        }
                        break;

                    case eLocateBy.ByName:
                        string name = ((HTMLElementInfo)ElementInfo).HTMLElementObject?.Attributes
                            .FirstOrDefault(x => x.Name == "name")?.Value ?? ((HTMLElementInfo)ElementInfo).Name ?? e?.GetAttribute("name");
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            elemLocator.LocateValue = name;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByName, elemLocator);
                        }
                        break;

                    case eLocateBy.ByRelXPath:
                        string relXPath = ((HTMLElementInfo)ElementInfo).RelXpath;
                        if (!string.IsNullOrWhiteSpace(relXPath))
                        {
                            elemLocator.LocateValue = relXPath;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByRelXPath, elemLocator);
                        }
                        break;

                    case eLocateBy.ByXPath:
                        if (!string.IsNullOrWhiteSpace(ElementInfo.XPath))
                        {
                            elemLocator.LocateValue = ElementInfo.XPath;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByXPath, elemLocator);
                        }
                        break;

                    case eLocateBy.ByTagName:
                        if (!string.IsNullOrWhiteSpace(ElementInfo.ElementType))
                        {
                            elemLocator.LocateValue = ElementInfo.ElementType;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByTagName, elemLocator);
                        }
                        break;

                    case eLocateBy.ByClassName:
                        string className = e?.GetAttribute("class");
                        if (!string.IsNullOrWhiteSpace(className))
                        {
                            // Only use the first class name to avoid compound class issues
                            string firstClass = className.Split(' ').FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(firstClass))
                            {
                                elemLocator.LocateValue = firstClass;
                                elemLocator.IsAutoLearned = true;
                                elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByClassName, elemLocator);
                            }
                        }
                        break;

                    case eLocateBy.ByCSSSelector:
                        string cssSelector = GenerateCssSelector(e);
                        if (!string.IsNullOrWhiteSpace(cssSelector))
                        {
                            elemLocator.LocateValue = cssSelector;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByCSSSelector, elemLocator);
                        }
                        break;

                    case eLocateBy.ByLinkText:
                        string linkText = e?.Text;
                        if (!string.IsNullOrWhiteSpace(linkText) && e?.TagName.ToLower() == "a")
                        {
                            elemLocator.LocateValue = linkText;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByLinkText, elemLocator);
                        }
                        break;

                    case eLocateBy.ByHref:
                        string href = e?.GetAttribute("href");
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            elemLocator.LocateValue = href;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByHref, elemLocator);
                        }
                        break;

                    case eLocateBy.ByAriaLabel:
                        string ariaLabel = e?.GetAttribute("aria-label");
                        if (!string.IsNullOrWhiteSpace(ariaLabel))
                        {
                            elemLocator.LocateValue = ariaLabel;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByAriaLabel, elemLocator);
                        }
                        break;

                    case eLocateBy.ByDataTestId:
                        string testId = e?.GetAttribute("data-testid");
                        if (!string.IsNullOrWhiteSpace(testId))
                        {
                            elemLocator.LocateValue = testId;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByDataTestId, elemLocator);
                        }
                        break;

                    case eLocateBy.ByPlaceholder:
                        string placeholder = e?.GetAttribute("placeholder");
                        if (!string.IsNullOrWhiteSpace(placeholder))
                        {
                            elemLocator.LocateValue = placeholder;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByPlaceholder, elemLocator);
                        }
                        break;

                    case eLocateBy.ByTitle:
                        string title = e?.GetAttribute("title");
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            elemLocator.LocateValue = title;
                            elemLocator.IsAutoLearned = true;
                            elemLocator.EnableFriendlyLocator = GetFriendlyLocatorSetting(pomSetting, eLocateBy.ByTitle, elemLocator);
                        }
                        break;
                }
            }

            // Return only the locators that were successfully auto-learned
            locatorsList = new ObservableList<ElementLocator>(locatorsList.Where(x => x.IsAutoLearned).ToList());
            return locatorsList;
        }


        private bool GetFriendlyLocatorSetting(PomSetting pomSetting, eLocateBy locateBy, ElementLocator defaultLocator)
        {
            return pomSetting?.ElementLocatorsSettingsList?.FirstOrDefault(x => x.LocateBy == locateBy)?.EnableFriendlyLocator
                   ?? defaultLocator.EnableFriendlyLocator;
        }

        /// <summary>
        /// Generates a CSS selector for the given element using tag, ID, or class names.
        /// </summary>
        /// <param name="element">The web element to generate a selector for.</param>
        /// <returns>A valid CSS selector string.</returns>
        private string GenerateCssSelector(IWebElement element)
        {
            if (element == null) return string.Empty;

            string tag = element.TagName ?? "*";
            string id = element.GetAttribute("id");
            string classAttr = element.GetAttribute("class");

            if (!string.IsNullOrEmpty(id))
            {
                // ID is unique and preferred
                return $"{tag}#{EscapeCssIdentifier(id)}";
            }
            else if (!string.IsNullOrEmpty(classAttr))
            {
                // Handle multiple class names
                var classNames = classAttr
                    .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => "." + EscapeCssIdentifier(c));

                return $"{tag}{string.Join("", classNames)}";
            }
            else
            {
                // Fallback to tag only
                return tag;
            }
        }

        /// <summary>
        /// Escapes special characters in CSS identifiers.
        /// </summary>
        private string EscapeCssIdentifier(string identifier)
        {
            // Basic escaping for CSS selectors
            return Regex.Replace(identifier, @"([^\w-])", "\\$1");
        }

        /// <summary>
        /// Generates a robust relative XPath using available attributes of the element.
        /// </summary>
        private void GenerateRobustRelativeXPath(IWebElement element, ElementInfo foundElemntInfo)
        {
            if (element == null)
            {
                return;
            }

            string tag = element.TagName ?? "*";
            var attributes = new[] { "data-testid", "aria-label", "placeholder", "title", "type", "name", "id", "class" };
            var conditions = new List<string>();

            foreach (var attr in attributes)
            {
                string value = element.GetAttribute(attr);
                if (!string.IsNullOrWhiteSpace(value) && !IsLikelyDynamic(value))
                {
                    if (attr == "class")
                    {
                        string firstClass = value.Split(' ').FirstOrDefault();
                        if (!string.IsNullOrWhiteSpace(firstClass) && !IsLikelyDynamic(firstClass))
                            conditions.Add($"contains(@class, '{firstClass}')");
                    }
                    else
                    {
                        conditions.Add($"@{attr}={EscapeXPathString(value)}");
                    }
                }
            }

            string text = element.Text;
            if (!string.IsNullOrWhiteSpace(text) && text.Length < 30)
            {
                conditions.Add($"contains(text(), {EscapeXPathString(text.Trim())})");
            }

            string conditionString = conditions.Count > 0 ? $"[{string.Join(" and ", conditions)}]" : "";
            if (!string.IsNullOrEmpty(conditionString) && CheckElementLocateStatus(conditionString))
            {
                var elementLocator = new ElementLocator() { LocateBy = eLocateBy.ByRelXPath, LocateValue = $"//{tag}{conditionString}", IsAutoLearned = true };
                foundElemntInfo.Locators.Add(elementLocator);
            }
        }

        private string EscapeXPathString(string value)
        {
            if (value.Contains("'"))
            {
                // If string contains single quotes, use concat() to handle them
                return "concat('" + value.Replace("'", "',\"'\",'") + "')";
            }
            return $"'{value}'";
        }

        private bool IsLikelyDynamic(string value)
        {
            // Heuristics to detect dynamic values
            return Regex.IsMatch(value, @"\b\d{3,}\b") || // long numeric suffix
                   Regex.IsMatch(value, @"\b[a-f0-9]{8,}\b", RegexOptions.IgnoreCase); // UUID-like
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {

            ObservableList<ElementLocator> locatorsList = [];
            try
            {
                if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null)
                {
                    if (((HTMLElementInfo)ElementInfo).HTMLElementObject.NextSibling != null && ((HTMLElementInfo)ElementInfo).HTMLElementObject.NextSibling.Name.StartsWith("#"))
                    {
                        ((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.NextSibling.NextSibling;
                        if (((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject, ref locatorsList, ePosition.left, pomSetting);
                        }

                    }
                    else
                    {
                        ((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.NextSibling;
                        if (((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).LeftofHTMLElementObject, ref locatorsList, ePosition.left, pomSetting);
                        }

                    }

                    if (((HTMLElementInfo)ElementInfo).HTMLElementObject.PreviousSibling != null && ((HTMLElementInfo)ElementInfo).HTMLElementObject.PreviousSibling.Name.StartsWith("#"))
                    {
                        ((HTMLElementInfo)ElementInfo).RightofHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.PreviousSibling.PreviousSibling;
                        if (((HTMLElementInfo)ElementInfo).RightofHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).RightofHTMLElementObject, ref locatorsList, ePosition.right, pomSetting);
                        }

                    }
                    else
                    {
                        ((HTMLElementInfo)ElementInfo).RightofHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.PreviousSibling;
                        if (((HTMLElementInfo)ElementInfo).RightofHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).RightofHTMLElementObject, ref locatorsList, ePosition.right, pomSetting);
                        }

                    }

                    if (((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode != null && ((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode.Name.StartsWith("#"))
                    {
                        ((HTMLElementInfo)ElementInfo).BelowHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode.ParentNode;
                        if (((HTMLElementInfo)ElementInfo).BelowHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).BelowHTMLElementObject, ref locatorsList, ePosition.below, pomSetting);
                        }

                    }

                    else
                    {
                        ((HTMLElementInfo)ElementInfo).BelowHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode;
                        if (((HTMLElementInfo)ElementInfo).BelowHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).BelowHTMLElementObject, ref locatorsList, ePosition.below, pomSetting);
                        }

                    }

                    if (((HTMLElementInfo)ElementInfo).HTMLElementObject.FirstChild != null && ((HTMLElementInfo)ElementInfo).HTMLElementObject.FirstChild.Name.StartsWith("#"))
                    {
                        ((HTMLElementInfo)ElementInfo).AboveHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.FirstChild.FirstChild;
                        if (((HTMLElementInfo)ElementInfo).AboveHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).AboveHTMLElementObject, ref locatorsList, ePosition.above, pomSetting);
                        }

                    }
                    else
                    {
                        ((HTMLElementInfo)ElementInfo).AboveHTMLElementObject = ((HTMLElementInfo)ElementInfo).HTMLElementObject.FirstChild;
                        if (((HTMLElementInfo)ElementInfo).AboveHTMLElementObject != null)
                        {
                            GetLocatorlistforFriendlyLocator(((HTMLElementInfo)ElementInfo).AboveHTMLElementObject, ref locatorsList, ePosition.above, pomSetting);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when learn LocateElementByFriendlyLocator", ex);
            }

            return locatorsList;
        }

        public void GetLocatorlistforFriendlyLocator(HtmlNode currentHtmlNode, ref ObservableList<ElementLocator> locatorsList, ePosition position, PomSetting pomSetting = null)
        {
            Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(htmlNode: currentHtmlNode);

            // set the Flag in case you wish to add the element or not to friendly locator
            bool learnElement = true;

            //filter element if needed, in case we need to learn only the MappedElements .i.e., LearnMappedElementsOnly is checked
            if (pomSetting?.FilteredElementType != null)
            {
                //Case Learn Only Mapped Element : set learnElement to false in case element doesn't exist in the filteredElementType List AND element is not frame element
                //if (!pomSetting.FilteredElementType.Contains(elementTypeEnum.Item2))
                if (!pomSetting.FilteredElementType.Any(x => x.ElementType.Equals(elementTypeEnum.Item2)))
                {
                    learnElement = false;
                }
            }

            ElementLocator elementLocator = new()
            {
                Active = true,
                Position = position,
                LocateBy = eLocateBy.POMElement,
                LocateValue = learnElement ? currentHtmlNode.XPath : String.Empty,
                IsAutoLearned = true
            };

            if (!string.IsNullOrEmpty(elementLocator.LocateValue))
            {
                locatorsList.Add(elementLocator);
            }
        }
        string IWindowExplorer.GetFocusedControl()
        {
            return null;
        }

        public void InjectSpyIfNotIngected()
        {
            string isSpyExist = "no";
            try
            {
                isSpyExist = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.IsLiveSpyExist();", null);
            }
            catch
            {
            }

            if (isSpyExist == "no")
            {
                InjectGingerLiveSpy();
                try
                {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("GingerLibLiveSpy.StartEventListner()");
                    // NEW: Enhance SVG detection
                    EnhanceSvgSpyDetection();
                    CurrentPageURL = string.Empty;
                    
                }
                catch
                {
                    mListnerCanBeStarted = false;
                    Reporter.ToLog(eLogLevel.DEBUG, "Spy Listener cannot be started");

                    var url = Driver.Title;
                    if (CurrentPageURL != url)
                    {
                        CurrentPageURL = Driver.Title;
                        Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Failed to start Live Spy Listner.Please click on the desired element to retrieve element details.");
                    }
                    ((IJavaScriptExecutor)Driver).ExecuteScript("return console.log('Failed to start Live Spy Listner.Please click on the desired element to retrieve element details.')");
                }
            }
        }

        public void InjectRecordingIfNotInjected()
        {
            string isRecordExist = "no";
            try
            {
                isRecordExist = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerRecorderLib.IsRecordExist();", null);
            }
            catch
            {
            }

            if (isRecordExist == "no")
            {
                InjectGingerHTMLHelper();
                InjectGingerHTMLRecorder();
            }
        }

        bool mListnerCanBeStarted = true;

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            return SpyControlAndGetElement();
        }

        private ElementInfo SpyControlAndGetElement()
        {
            Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
            try
            {
                UnhighlightLast();
                Driver.SwitchTo().DefaultContent();
                IWebElement el;
                InjectSpyIfNotIngected();
                if (mListnerCanBeStarted)
                {
                    el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.ElementFromPoint()");
                }
                else
                {
                    el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");
                }

                if (el == null)
                {
                    return null;
                }
                // NEW: Check if it's an SVG element
                bool isSvgElement = IsSvgElementByWebElement(el);

                HTMLElementInfo foundElemntInfo = new HTMLElementInfo
                {
                    ElementObject = el,
                    Path = string.Empty,
                    ScreenShotImage = TakeElementScreenShot(el)
                };

                // NEW: Handle SVG elements with enhanced XPath
                if (isSvgElement)
                {
                    foundElemntInfo.XPath = GenerateSvgElementXPathFromWebElement(el);
                    // Add SVG-specific properties
                    AddSvgSpyProperties(foundElemntInfo, el);
                }

                if (el.TagName is "iframe" or "frame")
                {
                    foundElemntInfo.Path = string.Empty;
                    foundElemntInfo.XPath = GenerateXpathForIWebElement(el, "");
                    return GetElementFromIframe(foundElemntInfo);
                }

                ElementInfo learnedElement = ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo);

                return learnedElement;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
            }
            return null;
        }
        /// <summary>
        /// Take specific element screenshot
        /// </summary>
        /// <param name="element">IWebElement</param>
        /// <returns>String image base64</returns>
        private string TakeElementScreenShot(IWebElement element)
        {
            IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
            js.ExecuteScript("arguments[0].style.transition = 'none'", element);

            var screenshot = ((ITakesScreenshot)element).GetScreenshot();
            Bitmap image = ScreenshotToImage(screenshot);
            byte[] byteImage;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format: System.Drawing.Imaging.ImageFormat.Png);
                byteImage = ms.ToArray();
            }
            return Convert.ToBase64String(byteImage);
        }

        private ElementInfo GetElementFromIframe(ElementInfo IframeElementInfo)
        {
            SwitchFrame(string.Empty, IframeElementInfo.XPath, false);

            InjectSpyIfNotIngected();
            bool listnerCanBeStarted = true;
            try
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript("GingerLibLiveSpy.StartEventListner()");
            }
            catch
            {
                listnerCanBeStarted = false;
            }

            IWebElement elInsideIframe;
            if (listnerCanBeStarted)
            {
                string XPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetXPoint();");
                string YPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetYPoint();");
                elInsideIframe = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.elementFromPoint(" + XPoint + ", " + YPoint + ");");
            }
            else
            {
                elInsideIframe = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");

            }

            string IframePath = string.Empty;
            if (IframeElementInfo.Path != string.Empty)
            {
                IframePath = IframeElementInfo.Path + "," + IframeElementInfo.XPath;
            }
            else
            {
                IframePath = IframeElementInfo.XPath;
            }

            HTMLElementInfo foundElemntInfo = new HTMLElementInfo
            {
                Path = IframePath,
                ElementObject = elInsideIframe
            };

            if (elInsideIframe.TagName is "iframe" or "frame")
            {
                if (!string.IsNullOrEmpty(foundElemntInfo.Path))
                {
                    SwitchFrame(foundElemntInfo);
                }
                else
                {
                    Driver.SwitchTo().DefaultContent();
                }

                foundElemntInfo.XPath = GenerateXpathForIWebElement(elInsideIframe, "");
                return GetElementFromIframe(foundElemntInfo);
            }

            return foundElemntInfo;
        }
        /// <summary>
        /// The algorithm to find the xpath uses the bottom to top DFS approach until the 'html' tag is located viz the topmost element in HTML Document.
        /// </summary>
        /// <param name="IWE">Web Element for which the Xpath should be generated</param>
        /// <param name="Driver"></param>
        /// <param name="current">Current Xpath</param>
        /// <param name="XPathsToDetectShadowElement">if shadow dom is detected, the xpath is added to the list (which as of now is only used for Live Spy)</param>
        /// <param name="isBrowserFireFox"></param>
        /// <returns>the xpath relative to the  nearest shadow dom else returns the full xpath</returns>

        public string GenerateXpathForIWebElement(IWebElement IWE, string current, IList<string> XPaths = null)
        {
            Stack<ISearchContext> stack = new();
            stack.Push(IWE);
            ISearchContext parentElement = null;
            ReadOnlyCollection<IWebElement> childrenElements = null;
            bool isShadowRootDetected = false;
            XPaths ??= [];

            while (stack.Count > 0)
            {

                ISearchContext context = stack.Pop();
                if (context is IWebElement webElement)
                {
                    string tagName = webElement.TagName;

                    if (tagName.Equals("html"))
                    {
                        string resultXPath = $"/html[1]{current}";
                        current = resultXPath;

                        if (isShadowRootDetected)
                        {
                            XPaths.Add(resultXPath);
                        }
                        continue;
                    }
                    parentElement = (ISearchContext)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].parentNode", webElement);

                    if (parentElement is ShadowRoot)
                    {
                        childrenElements = parentElement.FindElements(By.CssSelector(tagName));
                    }
                    else
                    {
                        childrenElements = parentElement.FindElements(By.XPath("./" + tagName));
                    }
                    int count = 1;

                    foreach (IWebElement childElement in childrenElements)
                    {

                        try
                        {
                            if (context.Equals(childElement))
                            {
                                string resultXPath = string.Empty;
                                if (string.IsNullOrEmpty(tagName))
                                {
                                    resultXPath = current;
                                }
                                else
                                {
                                    resultXPath = $"/{tagName}[{count}]{current}";
                                }
                                stack.Push(parentElement);
                                current = resultXPath;
                                break;
                            }
                            else
                            {
                                count++;
                            }

                        }

                        catch (Exception ex)
                        {
                            if (eBrowserType.FireFox.Equals(mBrowserType) && ex.Message != null && ex.Message.Contains("did not match a known command"))
                            {
                                continue;
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }
                }


                else if (context is ShadowRoot shadowRoot)
                {
                    parentElement = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].host", shadowRoot);
                    XPaths.Add(current);
                    stack.Push(parentElement);
                    current = string.Empty;
                    isShadowRootDetected = true;
                }

            }

            return XPaths.Count > 0 ? XPaths[0] : current;
        }

        public async Task<string> GenerateXpathForIWebElementAsync(IWebElement IWE, string current)
        {
            if (IWE.TagName == "html")
            {
                return "/" + IWE.TagName + "[1]" + current;
            }

            IWebElement parentElement = IWE.FindElement(By.XPath(".."));
            ReadOnlyCollection<IWebElement> childrenElements = parentElement.FindElements(By.XPath("./" + IWE.TagName));
            int count = 1;
            foreach (IWebElement childElement in childrenElements)
            {
                try
                {
                    if (IWE.Equals(childElement))
                    {
                        return await GenerateXpathForIWebElementAsync(parentElement, "/" + IWE.TagName + "[" + count + "]" + current);
                    }
                    else
                    {
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    if (mBrowserType == eBrowserType.FireFox && ex.Message != null && ex.Message.Contains("did not match a known command"))
                    {
                        continue;
                    }
                    else
                    {
                        throw ex;
                    }
                }

            }
            return "";
        }


        AppWindow IWindowExplorer.GetActiveWindow()
        {
            if (Driver != null)
            {
                AppWindow aw = new AppWindow
                {
                    Title = Driver.Title
                };
                return aw;
            }
            return null;

        }

        public void InjectGingerLiveSpy()
        {
            try
            {
                AddJavaScriptToPage(JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerLiveSpy));
                ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerLibLiveSpy();", null);
                string rc = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.AddScript(arguments[0]);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.jquery_min));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred during InjectGingerLiveSpy", ex);
            }
        }

        public string XPoint;
        public string YPoint;

        public string GetXAndYpointsfromClickEvent(ElementInfo elementInfo)
        {
            XPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetClickedXPoint();");
            YPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetClickedYPoint();");
            return XPoint + "," + YPoint;
        }

        public void StartClickEvent(ElementInfo elementInfo)
        {
            SwitchFrame(elementInfo.Path, elementInfo.XPath, true);
            ((IJavaScriptExecutor)Driver).ExecuteScript("GingerLibLiveSpy.StartClickEventListner()");
            Driver.SwitchTo().DefaultContent();
        }

        public void InjectGingerLiveSpyAndStartClickEvent(ElementInfo elementInfo)
        {
            string isSpyExist = "no";
            try
            {
                isSpyExist = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.IsLiveSpyExist();", null);
            }
            catch
            { }

            if (isSpyExist == "no")
            {
                InjectGingerLiveSpy();
            }
            ((IJavaScriptExecutor)Driver).ExecuteScript("GingerLibLiveSpy.StartClickEventListner()");
        }

        public void InjectGingerHTMLHelper()
        {
            //do once
            string GingerPayLoadJS = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.PayLoad);
            AddJavaScriptToPage(GingerPayLoadJS);
            string GingerHTMLHelperScript = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerHTMLHelper);
            AddJavaScriptToPage(GingerHTMLHelperScript);
            ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerLib();", null);

            //Inject JQuery
            string rc = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.jquery_min));

            // Inject XPath
            string rc2 = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerLibXPath, performManifyJS: true));


            // Inject code which can find element by XPath
            string rc3 = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.wgxpath_install));
        }


        public void InjectGingerHTMLRecorder()
        {
            //do once
            AddJavaScriptToPage(JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.GingerHTMLRecorder));
        }

        void AddJavaScriptToPage(string script)
        {
            try
            {
                //Note minifier change ' to ", so we change it back, so the script can have ", but we wrap it all with '
                string script3 = GetInjectJSSCript(script);
                var v = ((IJavaScriptExecutor)Driver).ExecuteScript(script3, null);
            }
            catch (OpenQA.Selenium.WebDriverException e)
            {
                StopRecordingIfAgentClosed(e.Message);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred while adding javascript to page", ex);
            }
        }

        void CheckifPageLoaded()
        {
            WebDriverWait webDriverWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(ImplicitWait));
            webDriverWait.Until((driver) =>
            {
                object jQuery = ((IJavaScriptExecutor)driver).ExecuteScript("return window.jQuery && jQuery.active == 0");

                bool IsJqueryCompleted = jQuery == null || jQuery.Equals(true);
                return
                ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete") && IsJqueryCompleted;
            });
        }

        string GetInjectJSSCript(string script)
        {
            string ScriptMin = JavaScriptHandler.MinifyJavaScript(script);
            // Get the Inject code
            string script2 = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.InjectJavaScript);
            script2 = JavaScriptHandler.MinifyJavaScript(script2);
            //Note minifier change ' to ", so we change it back, so the script can have ", but we wrap it all with '
            string script3 = script2.Replace("\"%SCRIPT%\"", "'" + ScriptMin + "'");
            return script3;
        }

        private bool IsSvgElementByWebElement(IWebElement element)
        {
            try
            {
                string tagName = element.TagName?.ToLower();
                if (tagName == "svg") return true;

                // Check if element is SVG child by checking namespace
                string namespaceURI = (string)((IJavaScriptExecutor)Driver).ExecuteScript(
                    "return arguments[0].namespaceURI;", element);
                return namespaceURI == "http://www.w3.org/2000/svg";
            }
            catch
            {
                return false;
            }
        }

        private void EnhanceSvgSpyDetection()
        {
            try
            {
                string svgSpyEnhancement = @"
                // Enhanced SVG element detection for complex nested structures like your example
                if (typeof GingerLibLiveSpy !== 'undefined') {
                    GingerLibLiveSpy.originalElementFromPoint = GingerLibLiveSpy.ElementFromPoint;
            
                    GingerLibLiveSpy.ElementFromPoint = function() {
                        var x = GingerLibLiveSpy.GetXPoint();
                        var y = GingerLibLiveSpy.GetYPoint();
                
                        // Get the most specific SVG element at coordinates
                        var element = GingerLibLiveSpy.GetMostSpecificSvgElement(x, y);
                
                        return element;
                    };
            
                    // Enhanced function to handle deeply nested SVG structures
                    GingerLibLiveSpy.GetMostSpecificSvgElement = function(x, y) {
                        var allCandidates = [];
                
                        // Method 1: Get all elements using elementsFromPoint (most accurate)
                        if (document.elementsFromPoint) {
                            var elementsAtPoint = document.elementsFromPoint(x, y);
                            allCandidates = allCandidates.concat(elementsAtPoint);
                        } else {
                            // Fallback: get element at point and traverse
                            var singleElement = document.elementFromPoint(x, y);
                            if (singleElement) {
                                allCandidates.push(singleElement);
                            }
                        }
                
                        // Method 2: Specifically search for SVG elements with your attributes
                        var specificSvgElements = GingerLibLiveSpy.FindSvgElementsWithSpecificAttributes(x, y);
                        allCandidates = allCandidates.concat(specificSvgElements);
                
                        // Method 3: Search all g elements and their children
                        var gElements = document.querySelectorAll('g[data-backend-id], g[data-node-id], g[class*= \'pnd\']');
                        for (var j = 0; j < gElements.length; j++)
                        {
                            var gElem = gElements[j];
                            if (GingerLibLiveSpy.IsElementAtCoordinates(gElem, x, y))
                            {
                                allCandidates.push(gElem);

                                // Also check all children of g elements
                                var gChildren = gElem.querySelectorAll('*');
                                for (var k = 0; k < gChildren.length; k++)
                                {
                                    var child = gChildren[k];
                                    if (GingerLibLiveSpy.IsElementAtCoordinates(child, x, y))
                                    {
                                        allCandidates.push(child);
                                    }
                                }
                            }
                        }

                        // Remove duplicates
                        var uniqueCandidates = GingerLibLiveSpy.RemoveDuplicates(allCandidates);

                        // Filter and score SVG elements
                        var svgCandidates = [];

                        for (var n = 0; n < uniqueCandidates.length; n++)
                        {
                            var candidate = uniqueCandidates[n];
                            if (GingerLibLiveSpy.IsSvgElement(candidate))
                            {
                                var candidateInfo = {
                                    element: candidate,
                                    tagName: candidate.tagName.toLowerCase(),
                                    depth: GingerLibLiveSpy.GetElementDepth(candidate),
                                    area: GingerLibLiveSpy.GetElementArea(candidate),
                                    specificity: GingerLibLiveSpy.GetEnhancedSvgElementSpecificity(candidate),
                                    hasInteractiveAttributes: GingerLibLiveSpy.HasInteractiveAttributes(candidate),
                                    isLeafNode: GingerLibLiveSpy.IsLeafNode(candidate)
                                };
                            svgCandidates.push(candidateInfo);
                        }
                    }


                        if (svgCandidates.length === 0)
                    {
                        // Fallback to regular element detection
                        return document.elementFromPoint(x, y);
                    }

                    // Sort by priority: interactive > leaf nodes > specificity > depth > smaller area
                    svgCandidates.sort(function(a, b) {
                        // Prioritize interactive elements
                        if (a.hasInteractiveAttributes !== b.hasInteractiveAttributes)
                        {
                            return b.hasInteractiveAttributes ? 1 : -1;
                        }

                        // Prioritize leaf nodes (actual content elements)
                        if (a.isLeafNode !== b.isLeafNode)
                        {
                            return b.isLeafNode ? 1 : -1;
                        }

                        // Then by specificity
                        if (a.specificity !== b.specificity)
                        {
                            return b.specificity - a.specificity;
                        }

                        // Then by depth (deeper = more specific)
                        if (a.depth !== b.depth)
                        {
                            return b.depth - a.depth;
                        }

                        // Finally by area (smaller = more precise)
                        return a.area - b.area;
                    });

                    return svgCandidates[0].element;
                };

                // Find SVG elements with specific attributes that match your structure
                GingerLibLiveSpy.FindSvgElementsWithSpecificAttributes = function(x, y)
                {
                    var foundElements = [];

                    // Search for elements with your specific data attributes
                    var selectors = [
                        'g[data-backend-id]',
                            'g[data-node-id]',
                            'g[data-parent-id]',
                            'g[class*=\'pnd\']',
                            'use[href]',
                            'text[class]',
                            'g[transform]'
                    ];

                    for (var i = 0; i < selectors.length; i++)
                    {
                        var elements = document.querySelectorAll(selectors[i]);
                        for (var j = 0; j < elements.length; j++)
                        {
                            var elem = elements[j];
                            if (GingerLibLiveSpy.IsElementAtCoordinates(elem, x, y))
                            {
                                foundElements.push(elem);
                            }
                        }
                    }

                    return foundElements;
                };

                // Remove duplicate elements from array
                GingerLibLiveSpy.RemoveDuplicates = function(elements)
                {
                    var unique = [];
                    for (var i = 0; i < elements.length; i++)
                    {
                        var found = false;
                        for (var j = 0; j < unique.length; j++)
                        {
                            if (unique[j] === elements[i])
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            unique.push(elements[i]);
                        }
                    }
                    return unique;
                };

                // Enhanced specificity calculation for complex SVG structures
                GingerLibLiveSpy.GetEnhancedSvgElementSpecificity = function(element)
                {
                    var specificity = 0;
                    var tagName = element.tagName.toLowerCase();

                    // Enhanced specificity map with better scoring for your nested elements
                    var specificityMap = {
                            // Interactive/content elements - highest priority
                            'text': 200,
                            'tspan': 190,
                    
                            // Functional elements - high priority
                            'use': 180,
                            'path': 170,
                            'rect': 160,
                            'circle': 160,
                            'ellipse': 160,
                            'line': 150,
                            'polygon': 150,
                            'polyline': 150,
                            'image': 140,
                    
                            // Group elements - medium priority (important for structure)
                            'g': 100,
                    
                            // Container elements - lower priority
                            'defs': 20,
                            'symbol': 30,
                            'marker': 25,
                            'clipPath': 15,
                            'mask': 15,
                    
                            // Root element - lowest priority
                            'svg': 10
                        };

                specificity += specificityMap[tagName] || 50;
                
                        // Boost for your specific attributes (data-backend-id, data-node-id, etc.)
                        if (element.getAttribute('data-backend-id')) specificity += 150;
                        if (element.getAttribute('data-node-id')) specificity += 140;
                        if (element.getAttribute('data-parent-id')) specificity += 130;
                        if (element.getAttribute('id')) specificity += 120;
                
                        // Boost for class attributes that match your pattern
                        var className = element.getAttribute('class') || '';
                        if (className.indexOf('pnd') >= 0) specificity += 100;
                        if (className.indexOf('Activity') >= 0) specificity += 90;
                        if (className.indexOf('Icon') >= 0) specificity += 80;
                        if (className.indexOf('Badge') >= 0) specificity += 70;
                
                        // Boost for elements with meaningful content
                        if (element.textContent && element.textContent.trim()) specificity += 80;
                        if (element.getAttribute('href')) specificity += 75;
                        if (element.getAttribute('transform')) specificity += 60;
                
                        // Boost for interactive attributes
                        if (element.getAttribute('onclick') || element.getAttribute('onmousedown') || element.getAttribute('onmouseup')) specificity += 120;
                        if (element.getAttribute('cursor') === 'pointer') specificity += 60;
                
                        // Boost for elements with visual properties
                        if (element.getAttribute('fill') && element.getAttribute('fill') !== 'none') specificity += 30;
                        if (element.getAttribute('stroke')) specificity += 25;
                
                        // Penalty for very large elements (likely containers)
                        var area = GingerLibLiveSpy.GetElementArea(element);
                        if (area > 10000) specificity -= 50;
                        else if (area > 5000) specificity -= 25;
                        else if (area< 100) specificity += 40; // Boost for small, precise elements
                
                        // Special handling for g elements based on children
                        if (tagName === 'g') {
                            var children = element.children;
                            if (children.length === 1) {
                                // g with single child - might be wrapper
                                specificity -= 20;
                            } else if (children.length > 3) {
                                // g with many children - likely important container
                                specificity += 30;
                            }

                    // Check if g has specific transform patterns
                    var transform = element.getAttribute('transform');
                    if (transform && transform.indexOf('translate') >= 0)
                    {
                        specificity += 25;
                    }
                                    }
                
                                    return Math.max(specificity, 0);
                                };

                    // Check if element has interactive attributes
                    GingerLibLiveSpy.HasInteractiveAttributes = function(element) {
                        var interactiveAttrs = [
                            'onclick', 'onmousedown', 'onmouseup', 'onmouseover',
                                        'href', 'data-backend-id', 'data-node-id'
                        ];

                        for (var i = 0; i < interactiveAttrs.length; i++)
                        {
                            if (element.getAttribute(interactiveAttrs[i]))
                            {
                                return true;
                            }
                        }

                        // Check class names for interactive patterns
                        var className = element.getAttribute('class') || '';
                        if (className.indexOf('pnd') >= 0 || className.indexOf('Activity') >= 0)
                        {
                            return true;
                        }

                        return false;
                    };

                    // Check if element is a leaf node (has no SVG children)
                    GingerLibLiveSpy.IsLeafNode = function(element) {
                        var children = element.children;
                        if (!children || children.length === 0)
                        {
                            return true;
                        }

                        // Check if any children are SVG elements
                        for (var i = 0; i < children.length; i++)
                        {
                            if (GingerLibLiveSpy.IsSvgElement(children[i]))
                            {
                                return false;
                            }
                        }

                        return true;
                    };

                    // Enhanced coordinate checking with better precision
                    GingerLibLiveSpy.IsElementAtCoordinates = function(element, x, y) {
                        try
                        {
                            var rect = element.getBoundingClientRect();

                            // Basic bounds check
                            var inBounds = (x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom);

                            if (!inBounds)
                            {
                                return false;
                            }

                            // For very small elements, be more lenient
                            if (rect.width < 5 || rect.height < 5)
                            {
                                var centerX = rect.left + rect.width / 2;
                                var centerY = rect.top + rect.height / 2;
                                var distance = Math.sqrt(Math.pow(x - centerX, 2) + Math.pow(y - centerY, 2));
                                return distance <= 10; // 10px tolerance for tiny elements
                            }

                            return inBounds;
                        }
                        catch (e)
                        {
                            return false;
                        }
                    };

                    // Rest of the helper functions
                    GingerLibLiveSpy.GetElementDepth = function(element) {
                        var depth = 0;
                        var current = element;
                        while (current && current.parentNode && current.parentNode !== document)
                        {
                            depth++;
                            current = current.parentNode;
                            // Stop counting if we reach the SVG root to normalize depths within SVG
                            if (current.tagName && current.tagName.toLowerCase() === 'svg')
                            {
                                break;
                            }
                        }
                        return depth;
                    };

                    GingerLibLiveSpy.GetElementArea = function(element) {
                        try
                        {
                            var rect = element.getBoundingClientRect();
                            return rect.width * rect.height;
                        }
                        catch (e)
                        {
                            return 0;
                        }
                    };

                    GingerLibLiveSpy.IsSvgElement = function(element) {
                        if (!element) return false;

                        return element.namespaceURI === 'http://www.w3.org/2000/svg' ||
                               element.tagName === 'svg' ||
                               element.ownerSVGElement ||
                               (element.parentNode && GingerLibLiveSpy.IsSvgElement(element.parentNode));
                    };
                }
                ";

                ((IJavaScriptExecutor)Driver).ExecuteScript(svgSpyEnhancement);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to enhance SVG spy detection", ex);
            }
        }

        private string GenerateSvgElementXPathFromWebElement(IWebElement svgElement)
        {
           return GenerateEnhancedSvgXPath(svgElement); 
        }

        /// <summary>
        /// Enhanced C# fallback method to generate SVG XPath
        /// </summary>
        private string GenerateEnhancedSvgXPath(IWebElement svgElement)
        {
            try
            {
                string tagName = svgElement.TagName?.ToLower();
                if (string.IsNullOrEmpty(tagName))
                {
                    return GenerateXpathForIWebElement(svgElement, "");
                }

                // Try attribute-based XPath first
                var attributeXPath = GenerateSvgAttributeBasedXPath(svgElement, tagName);
                if (!string.IsNullOrEmpty(attributeXPath))
                {
                    return attributeXPath;
                }

                // Fallback to regular XPath generation
                return GenerateXpathForIWebElement(svgElement, "");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Enhanced SVG XPath generation failed: {ex.Message}");
                return GenerateXpathForIWebElement(svgElement, "");
            }
        }

        /// <summary>
        /// Generate attribute-based XPath for SVG elements
        /// </summary>
        private string GenerateSvgAttributeBasedXPath(IWebElement svgElement, string tagName)
        {
            var attributes = new List<string>();

            // Priority order for SVG attributes
            string dataNodeId = svgElement.GetAttribute("data-node-id");
            if (!string.IsNullOrEmpty(dataNodeId))
            {
                return $"//*[local-name()='{tagName}' and @data-node-id='{EscapeXPathString(dataNodeId)}']";
            }

            string dataBackendId = svgElement.GetAttribute("data-backend-id");
            if (!string.IsNullOrEmpty(dataBackendId))
            {
                return $"//*[local-name()='{tagName}' and @data-backend-id='{EscapeXPathString(dataBackendId)}']";
            }

            string classAttr = svgElement.GetAttribute("class");
            if (!string.IsNullOrEmpty(classAttr))
            {
                var classes = classAttr.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c));
                var pndClass = classes.FirstOrDefault(c => c.Contains("pnd"));
                if (!string.IsNullOrEmpty(pndClass))
                {
                    return $"//*[local-name()='{tagName}' and contains(@class, '{EscapeXPathString(pndClass)}')]";
                }
            }

            string transform = svgElement.GetAttribute("transform");
            if (!string.IsNullOrEmpty(transform) && !IsLikelyDynamic(transform))
            {
                return $"//*[local-name()='{tagName}' and @transform='{EscapeXPathString(transform)}']";
            }

            string href = svgElement.GetAttribute("href");
            if (!string.IsNullOrEmpty(href))
            {
                return $"//*[local-name()='{tagName}' and @href='{EscapeXPathString(href)}']";
            }

            // If no unique attributes found, return null to use fallback
            return null;
        }

        /// <summary>
        /// Enhanced method to add SVG spy properties with more comprehensive attribute detection
        /// </summary>
        private void AddSvgSpyProperties(HTMLElementInfo elementInfo, IWebElement svgElement)
        {
            try
            {
                // Enhanced SVG attributes list
                string[] svgAttributes = {
                    "fill", "stroke", "stroke-width", "opacity",
                    "data-node-id", "data-backend-id", "data-parent-id",
                    "class", "id", "transform", "href", "xlink:href",
                    "x", "y", "width", "height", "cx", "cy", "r",
                    "d", "points", "text-anchor", "style"
                };

                foreach (string attr in svgAttributes)
                {
                    string attrValue = svgElement.GetAttribute(attr);
                    if (!string.IsNullOrEmpty(attrValue))
                    {
                        elementInfo.Properties.Add(new ControlProperty
                        {
                            Name = $"SVG-{attr}",
                            Value = attrValue,
                            ShowOnUI = true
                        });
                    }
                }

                // Add text content if present
                string textContent = svgElement.Text?.Trim();
                if (!string.IsNullOrEmpty(textContent))
                {
                    elementInfo.Properties.Add(new ControlProperty
                    {
                        Name = "SVG-TextContent",
                        Value = textContent,
                        ShowOnUI = true
                    });
                }

                // Add computed styles for SVG elements
                try
                {
                    var computedFill = ((IJavaScriptExecutor)Driver).ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).fill;", svgElement)?.ToString();
                    if (!string.IsNullOrEmpty(computedFill) && computedFill != "rgb(0, 0, 0)")
                    {
                        elementInfo.Properties.Add(new ControlProperty
                        {
                            Name = "SVG-ComputedFill",
                            Value = computedFill,
                            ShowOnUI = true
                        });
                    }

                    var computedStroke = ((IJavaScriptExecutor)Driver).ExecuteScript(
                        "return window.getComputedStyle(arguments[0]).stroke;", svgElement)?.ToString();
                    if (!string.IsNullOrEmpty(computedStroke) && computedStroke != "none")
                    {
                        elementInfo.Properties.Add(new ControlProperty
                        {
                            Name = "SVG-ComputedStroke",
                            Value = computedStroke,
                            ShowOnUI = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Failed to get computed SVG styles: {ex.Message}");
                }

                // Add bounding box information for SVG elements
                try
                {
                    var bbox = ((IJavaScriptExecutor)Driver).ExecuteScript(@"
                        try {
                            var bbox = arguments[0].getBBox();
                            return {
                                x: bbox.x,
                                y: bbox.y,
                                width: bbox.width,
                                height: bbox.height
                            };
                        } catch(e) {
                            return null;
                        }
                        ", svgElement) as Dictionary<string, object>;

                    if (bbox != null)
                    {
                        foreach (var kvp in bbox)
                        {
                            elementInfo.Properties.Add(new ControlProperty
                            {
                                Name = $"SVG-BBox-{kvp.Key}",
                                Value = kvp.Value?.ToString() ?? "0",
                                ShowOnUI = false
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Failed to get SVG bounding box: {ex.Message}");
                }

                // Add namespace information
                try
                {
                    var namespaceURI = ((IJavaScriptExecutor)Driver).ExecuteScript(
                        "return arguments[0].namespaceURI;", svgElement)?.ToString();
                    if (!string.IsNullOrEmpty(namespaceURI))
                    {
                        elementInfo.Properties.Add(new ControlProperty
                        {
                            Name = "SVG-NamespaceURI",
                            Value = namespaceURI,
                            ShowOnUI = false
                        });
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"Failed to get SVG namespace: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to add SVG properties during spy", ex);
            }
        }

        public override void StartRecording()
        {
            DoRecording();
        }

        void IRecord.StartRecording(bool learnAdditionalChanges)
        {
            DoRecording(learnAdditionalChanges);
        }

        private void DoRecording(bool learnAdditionalChanges = false)
        {
            CurrentFrame = string.Empty;
            Driver.SwitchTo().DefaultContent();
            InjectRecordingIfNotInjected();

            //TODO: put Ginger HTML Recorder.JS in Properties.Resources.GingerHTMLRecorder.js
            ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerRecorderLib();", null);

            PayLoad pl = new PayLoad("StartRecording");
            pl.ClosePackage();
            PayLoad plrc = ExceuteJavaScriptPayLoad(pl);
            // Handle in the JS to start recording

            // loop to get all recording until user click stop record
            IsRecording = true;
            IsLooped = false;
            LastFrameID = string.Empty;

            Task t = new Task(() =>
            {
                DoGetRecordings(learnAdditionalChanges);

            }, TaskCreationOptions.LongRunning);
            t.Start();
        }

        string LastFrameID = string.Empty;
        bool IsLooped = false;
        bool IframeClicked = false;

        private void HandleRedirectClick()
        {
            string recordStarted = "false";
            try
            {
                recordStarted = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerRecorderLib.IsRecordStarted();", null);
            }
            catch
            {
                ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerRecorderLib();", null);
            }

            if (!Convert.ToBoolean(recordStarted))
            {
                PayLoad pl = new PayLoad("StartRecording");
                pl.ClosePackage();
                PayLoad plrc = ExceuteJavaScriptPayLoad(pl);

            }
        }

        private void HandleIframeClicked()
        {
            IWebElement el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");
            el.ToString();

            if (el.TagName is "iframe" or "frame")
            {
                IframeClicked = true;
                LastFrameID = el.ToString();
                ElementInfo ElementInfo = GetElementInfoWithIWebElementWithXpath(el, CurrentFrame);
                SwitchFrameFromCurrent(ElementInfo);
                string recordStarted = "false";
                InjectRecordingIfNotInjected();
                try
                {
                    recordStarted = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerRecorderLib.IsRecordStarted();", null);
                }
                catch
                {
                    ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerRecorderLib();", null);
                }

                if (!Convert.ToBoolean(recordStarted))
                {
                    PayLoad pl = new PayLoad("StartRecording");
                    pl.ClosePackage();
                    PayLoad plrc = ExceuteJavaScriptPayLoad(pl);
                }

                Act switchAct = new ActBrowserElement
                {
                    LocateBy = eLocateBy.ByXPath
                };
                ((ActBrowserElement)switchAct).ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
                switchAct.Description = "Switch Window to Iframe";
                switchAct.LocateValue = ElementInfo.XPath;
                this.BusinessFlow.AddAct(switchAct);
            }
            else if (el.TagName == "body" && IframeClicked && !IsLooped)
            {
                Driver.SwitchTo().DefaultContent();
                el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");

                if (el.TagName is "iframe" or "frame")
                {
                    if (el.ToString() == LastFrameID)
                    {
                        ElementInfo ElementInfo = GetElementInfoWithIWebElementWithXpath(el, CurrentFrame);
                        SwitchAllFramePathes(ElementInfo);
                        IsLooped = true;
                        return;
                    }
                    else
                    {
                        LastFrameID = el.ToString();
                        CurrentFrame = string.Empty;
                        ElementInfo ElementInfo = GetElementInfoWithIWebElementWithXpath(el, CurrentFrame);
                        SwitchFrameFromCurrent(ElementInfo);

                        Act switchActionDefult = new ActBrowserElement
                        {
                            LocateBy = eLocateBy.ByXPath
                        };
                        ((ActBrowserElement)switchActionDefult).ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;
                        switchActionDefult.Description = "Switch Window to Default Iframe";
                        this.BusinessFlow.AddAct(switchActionDefult);

                        Act switchActionFrame = new ActBrowserElement
                        {
                            LocateBy = eLocateBy.ByXPath
                        };
                        ((ActBrowserElement)switchActionFrame).ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
                        switchActionFrame.Description = "Switch Window to Iframe";
                        switchActionFrame.LocateValue = ElementInfo.XPath;
                        this.BusinessFlow.AddAct(switchActionFrame);

                        IsLooped = true;
                        return;
                    }
                }

                CurrentFrame = string.Empty;
                IframeClicked = false;
                Act switchAct = new ActBrowserElement
                {
                    LocateBy = eLocateBy.ByXPath
                };
                ((ActBrowserElement)switchAct).ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;
                switchAct.Description = "Switch Window to Default Iframe";
                this.BusinessFlow.AddAct(switchAct);
            }
            else if (el.TagName != "body")
            {
                IsLooped = false;
            }
        }

        private void DoGetRecordings(bool learnAdditionalChanges)
        {
            try
            {
                IframeClicked = false;
                while (IsRecording)
                {
                    try
                    {
                        InjectRecordingIfNotInjected();
                        HandleIframeClicked();
                        HandleRedirectClick();
                        Thread.Sleep(1000);
                        // TODO: call JS to get the recording

                        PayLoad PLgerRC = new PayLoad("GetRecording");
                        PLgerRC.ClosePackage();
                        PayLoad plrcRec = ExceuteJavaScriptPayLoad(PLgerRC);

                        if (!PLgerRC.IsErrorPayLoad())
                        {
                            List<PayLoad> PLs = plrcRec.GetListPayLoad();

                            // Each Payload is one recording...
                            foreach (PayLoad PLR in PLs)
                            {
                                ElementActionCongifuration configArgs = new ElementActionCongifuration();
                                string locateBy = PLR.GetValueString();
                                configArgs.LocateBy = GetLocateBy(locateBy);
                                configArgs.LocateValue = PLR.GetValueString();
                                configArgs.ElementValue = PLR.GetValueString();
                                configArgs.Operation = PLR.GetValueString();
                                string type = PLR.GetValueString();
                                configArgs.Type = GetElementTypeEnum(null, type).Item2;
                                configArgs.Description = GetDescription(configArgs.Operation, configArgs.LocateValue, configArgs.ElementValue, type);
                                if (learnAdditionalChanges)
                                {
                                    string xCordinate = PLR.GetValueString();
                                    string yCordinate = PLR.GetValueString();
                                    ElementInfo eInfo = LearnRecorededElementFullDetails(xCordinate, yCordinate);

                                    if (eInfo != null)
                                    {
                                        configArgs.LearnedElementInfo = eInfo;
                                    }
                                    else
                                    {
                                        eInfo = GetElementInfoFromActionConfiguration(configArgs);
                                        configArgs.LearnedElementInfo = eInfo;
                                    }
                                }
                                if (learnAdditionalChanges && RecordingEvent != null)
                                {
                                    //New implementation supporting POM
                                    RecordingEventArgs args = new RecordingEventArgs
                                    {
                                        EventType = eRecordingEvent.ElementRecorded,
                                        EventArgs = configArgs
                                    };
                                    OnRecordingEvent(args);
                                }
                                else
                                {
                                    string url = Driver.Url;
                                    string title = Driver.Title;
                                    if (CurrentPageURL != url)
                                    {
                                        CurrentPageURL = url;
                                        AddBrowserAction(title, url);
                                    }

                                    //Temp existing implementation
                                    ActUIElement actUI = GetActUIElementAction(configArgs);
                                    this.BusinessFlow.AddAct(actUI);
                                    if (mActionRecorded != null)
                                    {
                                        mActionRecorded.Invoke(this, new POMEventArgs(Driver.Title, actUI));
                                    }
                                }
                            }
                        }
                    }
                    catch (OpenQA.Selenium.WebDriverException e)
                    {
                        StopRecordingIfAgentClosed(e.Message);
                    }
                    catch (Exception e)
                    {
                        if (e.Message == PayLoad.PAYLOAD_PARSING_ERROR)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while recording", e);
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error occurred while recording", e);
                        }
                    }
                }
                CurrentPageURL = string.Empty;
                RecordingEvent = null;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while recording", e);
            }
        }

        /// <summary>
        /// This method will create the element info object
        /// </summary>
        /// <param name="configArgs"></param>
        /// <returns></returns>
        private ElementInfo GetElementInfoFromActionConfiguration(ElementActionCongifuration configArgs)
        {
            ElementInfo eInfo = new ElementInfo();
            try
            {
                if (Enum.IsDefined(typeof(eElementType), Convert.ToString(configArgs.Type)))
                {
                    eInfo.ElementTypeEnum = (eElementType)Enum.Parse(typeof(eElementType), Convert.ToString(configArgs.Type));
                }
                eInfo.ElementName = configArgs.Description;
                eInfo.Locators.Add(new ElementLocator()
                {
                    ItemName = Convert.ToString(configArgs.LocateBy),
                    LocateValue = configArgs.LocateValue
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred creating the elementinfo object", ex);
            }
            return eInfo;
        }

        /// <summary>
        /// This method is used to get the ActUIElement action
        /// </summary>
        /// <param name="configArgs"></param>
        /// <returns></returns>
        private ActUIElement GetActUIElementAction(ElementActionCongifuration configArgs)
        {
            ActUIElement actUI = new ActUIElement
            {
                Description = GetDescription(configArgs.Operation, configArgs.LocateValue, configArgs.ElementValue, Convert.ToString(configArgs.Type)),
                ElementLocateBy = GetLocateBy(Convert.ToString(configArgs.LocateBy)),
                ElementLocateValue = configArgs.LocateValue,
                ElementType = (eElementType)configArgs.Type
            };
            if (Enum.IsDefined(typeof(ActUIElement.eElementAction), configArgs.Operation))
            {
                actUI.ElementAction = (ActUIElement.eElementAction)Enum.Parse(typeof(ActUIElement.eElementAction), configArgs.Operation);
            }
            else
            {
                actUI = null;
            }
            actUI.Value = configArgs.ElementValue;
            return actUI;
        }

        void IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        /// <summary>
        /// This method is used to stop recording if the agent is not reachable
        /// </summary>
        private void StopRecordingIfAgentClosed(string errorDetails)
        {
            if (this.IsRunning())
            {
                return;
            }
            IsRecording = false;
            RecordingEventArgs args = new RecordingEventArgs
            {
                EventType = eRecordingEvent.StopRecording,
                EventArgs = errorDetails
            };
            OnRecordingEvent(args);
        }

        public event RecordingEventHandler RecordingEvent;
        private string CurrentPageURL = string.Empty;

        protected void OnRecordingEvent(RecordingEventArgs e)
        {
            RecordingEvent?.Invoke(this, e);
        }

        ElementInfo LearnRecorededElementFullDetails(string xCordinate, string yCordinate)
        {
            ElementInfo eInfo = null;
            if (!string.IsNullOrEmpty(xCordinate) && !string.IsNullOrEmpty(yCordinate))
            {
                try
                {
                    string url = Driver.Url;
                    string title = Driver.Title;
                    if (CurrentPageURL != url)
                    {
                        CurrentPageURL = url;
                        PageChangedEventArgs pageArgs = new PageChangedEventArgs()
                        {
                            PageURL = url,
                            PageTitle = title,
                            ScreenShot = Amdocs.Ginger.Common.GeneralLib.General.BitmapToBase64(GetScreenShot())
                        };

                        RecordingEventArgs args = new RecordingEventArgs
                        {
                            EventType = eRecordingEvent.PageChanged,
                            EventArgs = pageArgs
                        };
                        OnRecordingEvent(args);
                    }

                    double xCord = 0;
                    double yCord = 0;
                    double.TryParse(xCordinate, out xCord);
                    double.TryParse(yCordinate, out yCord);

                    IWebElement el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.elementFromPoint(" + xCord + ", " + yCord + ");");
                    if (el != null)
                    {
                        string elementName = GenerateElementTitle(el);
                        HTMLElementInfo foundElemntInfo = new HTMLElementInfo
                        {
                            ElementObject = el
                        };
                        eInfo = ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo);
                        eInfo.ElementName = elementName;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while recording - while reading element", ex);
                }
            }

            return eInfo;
        }

        private void AddBrowserAction(string pageTitle, string pageURL)
        {
            try
            {
                ActBrowserElement browseAction = new ActBrowserElement()
                {
                    Description = "Go to Url - " + pageTitle,
                    ControlAction = ActBrowserElement.eControlAction.GotoURL,
                    LocateBy = eLocateBy.NA,
                    Value = pageURL
                };
                this.BusinessFlow.AddAct(browseAction);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while adding browser action", ex);
            }
        }

        public static string GetLocatedValue(string Type, string LocateValue, string ElemValue)
        {
            return Type switch
            {
                "radio" => ElemValue,
                _ => LocateValue,
            };
        }

        //Returns description for action recorder from HTML element
        public static string GetDescription(string ControlAction, string LocateValue, string ElemValue, string Type)
        {
            return Type switch
            {
                "button" => "Click Button '" + LocateValue + "'",
                "text" => "Set Text '" + LocateValue + "'",
                "textarea" => "Set TextArea '" + LocateValue + "'",
                "select-one" => "Set Select '" + LocateValue + "'",
                "checkbox" => "Click Checkbox '" + LocateValue + "'",
                "radio" => "Click Radio '" + LocateValue + "'",
                "SPAN" => "Click SPAN '" + LocateValue + "'",
                "li" => "Click li '" + LocateValue + "'",
                _ => "Set Web Element '" + LocateValue + "'",
            };
        }

        //Returns Action for HTML element on PL
        public static ActGenElement.eGenElementAction GetElemAction(string ControlAction)
        {
            return ControlAction switch
            {
                "Click" => ActGenElement.eGenElementAction.Click,
                "SetValue" => ActGenElement.eGenElementAction.SetValue,
                "SendKeys" => ActGenElement.eGenElementAction.SendKeys,
                _ => ActGenElement.eGenElementAction.Wait,
            };
        }

        //Returns LocatorType for HTML element on PL
        public static eLocateBy GetLocateBy(string LocateBy)
        {
            return LocateBy switch
            {
                "ByID" => eLocateBy.ByID,
                "ByName" => eLocateBy.ByName,
                "ByValue" => eLocateBy.ByValue,
                "ByXPath" => eLocateBy.ByXPath,
                "ByClassName" => eLocateBy.ByClassName,
                _ => eLocateBy.NA,
            };
        }

        public override void StopRecording()
        {
            EndRecordings();
        }

        void IRecord.StopRecording()
        {
            EndRecordings();
        }

        private void EndRecordings()
        {
            CurrentFrame = string.Empty;
            if (Driver != null)
            {
                Driver.SwitchTo().DefaultContent();

                PayLoad pl = new PayLoad("StopRecording");
                pl.ClosePackage();
                PayLoad plrc = ExceuteJavaScriptPayLoad(pl);
            }
            // Handle in the JS to stop recording
            IsRecording = false;
        }

        public Dictionary<string, object> GetElementAttributes(IWebElement elem)
        {
            return ((IJavaScriptExecutor)Driver).ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", elem) as Dictionary<string, object>;
        }

        public void HandleBrowserAlert(ActHandleBrowserAlert act)
        {
            switch (act.GenElementAction)
            {
                case ActHandleBrowserAlert.eHandleBrowseAlert.AcceptAlertBox:
                    try
                    {
                        Driver.SwitchTo().Alert().Accept();
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error when Accepting Alert Box - " + e.Message);
                        return;
                    }
                    break;

                case ActHandleBrowserAlert.eHandleBrowseAlert.DismissAlertBox:
                    try
                    {
                        Driver.SwitchTo().Alert().Dismiss();
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error when Dismiss Alert Box - " + e.Message);
                        return;
                    }
                    break;

                case ActHandleBrowserAlert.eHandleBrowseAlert.GetAlertBoxText:
                    try
                    {
                        string AlertBoxText = Driver.SwitchTo().Alert().Text;
                        act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                        if (act.GetReturnParam("Actual") == null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                        }
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error to Get Text Alert Box - " + e.Message);
                        return;
                    }
                    break;

                case ActHandleBrowserAlert.eHandleBrowseAlert.SendKeysAlertBox:
                    try
                    {
                        Driver.SwitchTo().Alert().SendKeys(act.GetInputParamCalculatedValue("Value"));
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Error to Get Text Alert Box - " + e.Message);
                        return;
                    }
                    break;
            }
        }

        private void SwitchWindow(Act act)
        {
            bool BFound = false;
            Stopwatch St = new Stopwatch();
            string searchedWinTitle = GetSearchedWinTitle(act);
            // retry mechanism for 20 seconds waiting for the window to open, 500ms intervals                  

            St.Reset();

            int waitTime = this.ImplicitWait;
            if (act is ActSwitchWindow actSwitchWindow)
            {
                if (actSwitchWindow.WaitTime >= 0)
                {
                    waitTime = actSwitchWindow.WaitTime;
                }
            }
            else if (act is ActUIElement actUiElement)
            {
                // adding to support actuielement switch window action synctime
                var syncTime = Convert.ToInt32(actUiElement.GetInputParamCalculatedValue(ActUIElement.Fields.SyncTime));
                if (syncTime >= 0)
                {
                    waitTime = syncTime;
                }
            }


            while (St.ElapsedMilliseconds < waitTime * 1000)
            {
                {
                    St.Start();
                    try
                    {
                        ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
                        foreach (String winHandle in openWindows)
                        {
                            if (act.LocateBy == eLocateBy.ByTitle || (act is ActUIElement && ((ActUIElement)act).ElementLocateBy.Equals(eLocateBy.ByTitle)))
                            {

                                string winTitle = Driver.SwitchTo().Window(winHandle).Title;
                                // We search windows titles based on contains
                                //TODO: maybe contains is better +  need exact match or other 
                                if (winTitle.IndexOf(searchedWinTitle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    // window found put some info in ExInfo
                                    act.ExInfo = winTitle;
                                    BFound = true;
                                    break;
                                }
                            }
                            if (act.LocateBy == eLocateBy.ByUrl || (act is ActUIElement && ((ActUIElement)act).ElementLocateBy.Equals(eLocateBy.ByUrl)))
                            {
                                string winurl = Driver.SwitchTo().Window(winHandle).Url;
                                // We search windows titles based on contains
                                //TODO: maybe contains is better +  need exact match or other 
                                if (winurl.IndexOf(searchedWinTitle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                {
                                    // window found put some info in ExInfo
                                    act.ExInfo = winurl;
                                    BFound = true;
                                    break;
                                }
                            }
                            if (act.LocateBy == eLocateBy.ByIndex || (act is ActUIElement && ((ActUIElement)act).ElementLocateBy.Equals(eLocateBy.ByIndex)))
                            {
                                int getWindowIndex = Int16.Parse(act.LocateValueCalculated);
                                string winIndexTitle = Driver.SwitchTo().Window(openWindows[getWindowIndex]).Title;
                                if (winIndexTitle != null)
                                {
                                    // window found put some info in ExInfo
                                    act.ExInfo = winIndexTitle;
                                    BFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    { break; }
                    if (BFound)
                    {
                        return;
                    }

                    Thread.Sleep(500);
                }
            }
            if (BFound)
            {
                return;//window found
            }
            else
            {
                // Added below code to verify if there is any window exist with blank title - 
                // It has been added to handle special scenario where window is not having title in IE but have in Chrome
                ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
                foreach (String winHandle in openWindows)
                {
                    //    if (winHandle == currentWindow)
                    //        continue;
                    string winTitle = Driver.SwitchTo().Window(winHandle).Title;

                    if (String.IsNullOrEmpty(winTitle))
                    {
                        act.ExInfo = "Switched to window having Empty Title.";
                        BFound = true;
                        return;
                    }
                }
                //Window not found
                // switch back to previous window
                //if (currentWindow != "Error")
                //    Driver.SwitchTo().Window(currentWindow);
                act.Error = "Error: Window with the title '" + searchedWinTitle + "' was not found.";

            }
        }

        public PayLoad ExceuteJavaScriptPayLoad(PayLoad RequestPL)
        {
            string script = "return GingerLib.ProcessPayLoad(arguments[0])";
            object[] Params = new object[1];
            byte[] b = RequestPL.GetPackage();

            //TODO: find faster way to convert the bytes to JS array
            object[] arr = new object[b.Length];
            for (int i = 0; i < b.Length; i++)
            {
                arr[i] = (int)b[i];
            }
            Params[0] = arr;
            try
            {

                dynamic rc = ((IJavaScriptExecutor)Driver).ExecuteScript(script, Params);
                PayLoad PLRC = PayLoadFromJSResponse(rc);
                return PLRC;
            }
            catch (Exception ex)
            {
                return PayLoad.Error("ExceuteJavaScriptPayLoad Failed - " + ex.Message);
            }
        }

        private bool IsDictionary(dynamic dict)
        {
            Type t = dict.GetType();
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        PayLoad PayLoadFromJSResponse(dynamic rc2)
        {
            if (IsDictionary(rc2))// For firefox execute script is returning a list dictionary
            {
                //This code is added to eleminate the additional keyvalue pair with key "toJSON", this key is getting added to dictionary
                List<KeyValuePair<string, object>> rc3 = GetCorrectedKeyValuePair(rc2);
                //------------------------------------------

                var list = rc3.OrderBy(kp => Convert.ToInt32(kp.Key)).Select(kp => kp.Value).ToList();
                return GetPayLoadfromList(list);

            }
            else//for chrome and IE execute is returning a list of object
            {
                //TODO: find faster way to do it
                ReadOnlyCollection<object> la = (ReadOnlyCollection<object>)rc2;
                return GetPayLoadfromList(la);
            }
        }

        /// <summary>
        /// This code is added to eleminate the additional keyvalue pair with key "toJSON", this key is getting added to dictionary
        /// </summary>
        /// <param name="rc2"></param>
        /// <returns></returns>
        private List<KeyValuePair<string, object>> GetCorrectedKeyValuePair(dynamic rc2)
        {
            List<KeyValuePair<string, object>> rc3 = [];
            foreach (var item in ((IEnumerable<KeyValuePair<string, object>>)rc2))
            {
                int val;
                if (int.TryParse(item.Key, out val))
                {
                    rc3.Add(item);
                }
            }
            return rc3;
        }

        PayLoad GetPayLoadfromList(dynamic list)
        {
            int len = list.Count;
            byte[] rcb = new byte[len];
            int i = 0;
            foreach (object o in list)
            {
                rcb[i] = byte.Parse(o.ToString());
                i++;
            }
            PayLoad RCPL = new PayLoad(rcb);
            return RCPL;
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }

        private void HandleSwitchFrame(Act act)
        {
            IWebElement e = null;
            try
            {
                if (act.LocateValue is not "" and not null)
                {
                    e = LocateElement(act);
                    if (e != null)
                    {
                        Driver.SwitchTo().Frame(e);
                        return;
                    }
                    else
                    {
                        act.Error = "Error: Unable to find the specified frame";
                        return;
                    }
                }
                else if (!string.IsNullOrEmpty(act.GetInputParamCalculatedValue("Value")))
                {
                    if (!act.GetInputParamCalculatedValue("Value").Trim().Equals("DEFAULT", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Driver.SwitchTo().Frame(act.GetInputParamCalculatedValue("Value"));
                        return;
                    }
                    else
                    {
                        Driver.SwitchTo().DefaultContent();
                        return;
                    }
                }
                else if ((act.GetInputParamCalculatedValue("Value") == "" || act.GetInputParamCalculatedValue("Value") == null) && (act.LocateValue == "" || act.LocateValue == null))
                {
                    act.Error = "Locate Value or Value is Empty";
                    return;
                }
            }
            catch
            {
                act.Error = "Error: Unable to find the specified frame";
                return;
            }
        }
        /// <summary>
        /// When the user enters a locator to locate a shadow root host (the parent node of the shadow root)
        /// the corresponding shadow root is provided to the current context attribute.
        /// </summary>
        /// <param name="act"></param>
        /// <exception cref="NoSuchElementException">Is thrown when no shadow root is found</exception>
        private void HandleSwitchToShadowDOM(Act act)
        {
            IWebElement shadowDOMHost = LocateElement(act);
            if (shadowDOMHost != null)
            {
                CurrentContext = shadowDOM.GetShadowRootIfExists(shadowDOMHost);
            }

            if (shadowDOMHost == null || CurrentContext == null)
            {
                throw new NoSuchElementException("Shadow Root Host cannot be located. Please try to find the appropriate Locator of the Element just above the shadow root (Shadow Root Host)");
            }

        }

        /// <summary>
        /// Is called when user wants to switch to the default DOM (which does not have direct access 
        /// to the shadow roots' elements but can access elements which are not in a shadow root)
        /// </summary>
        private void HandleSwitchToDefaultDOM()
        {
            CurrentContext = Driver;
        }

        public async void ActBrowserElementHandler(ActBrowserElement act)
        {
            try
            {
                string AgentType = GetAgentAppName();
                switch (act.ControlAction)
                {
                    case ActBrowserElement.eControlAction.Maximize:
                        try
                        {
                            Driver.Manage().Window.Maximize();
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, ex.Message);
                        }
                        break;

                    case ActBrowserElement.eControlAction.OpenURLNewTab:
                        string url = "";
                        if (!string.IsNullOrEmpty(act.GetInputParamValue(ActBrowserElement.Fields.URLSrc)) && act.GetInputParamValue(ActBrowserElement.Fields.URLSrc).Equals(ActBrowserElement.eURLSrc.UrlPOM.ToString()))
                        {
                            string POMGuid = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.PomGUID);

                            if (!string.IsNullOrEmpty(POMGuid) && Guid.TryParse(POMGuid, out Guid parsedPOMGuid))
                            {
                                ApplicationPOMModel SelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(parsedPOMGuid);
                                if (SelectedPOM != null)
                                {
                                    url = ValueExpression.Calculate(GetCurrentProjectEnvironment(), this.BusinessFlow, SelectedPOM.PageURL, null);
                                }
                                else
                                {
                                    act.Error = "Error: Selected POM not found. Please select valid POM.";
                                    return;
                                }
                            }
                            else
                            {
                                act.Error = "Error: Selected POM not found (Empty or Invalid POM Guid). Please select valid POM.";
                                return;
                            }
                        }
                        else
                        {
                            url = act.GetInputParamCalculatedValue("Value");
                        }

                        if (string.IsNullOrEmpty(url))
                        {
                            act.Error = "Error: Provided URL is empty. Please provide valid URL.";
                            return;
                        }
                        try
                        {
                            OpenNewTab();
                            GotoURL(act, url);
                        }
                        catch (Exception ex)
                        {
                            act.Status = eRunStatus.Failed;
                            act.Error += ex.Message;
                            Reporter.ToLog(eLogLevel.DEBUG, $"OpenNewTab {ex.Message} ", ex.InnerException);
                        }
                        break;

                    case ActBrowserElement.eControlAction.GotoURL:
                        string gotoUrl = "";
                        if (!string.IsNullOrEmpty(act.GetInputParamValue(ActBrowserElement.Fields.URLSrc)) && act.GetInputParamValue(ActBrowserElement.Fields.URLSrc).Equals(ActBrowserElement.eURLSrc.UrlPOM.ToString()))
                        {
                            string POMGuid = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.PomGUID);

                            if (!string.IsNullOrEmpty(POMGuid) && Guid.TryParse(POMGuid, out Guid parsedPOMGuid))
                            {
                                ApplicationPOMModel SelectedPOM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(parsedPOMGuid);
                                if (SelectedPOM != null)
                                {
                                    gotoUrl = ValueExpression.Calculate(GetCurrentProjectEnvironment(), this.BusinessFlow, SelectedPOM.PageURL, null);
                                }
                                else
                                {
                                    act.Error = "Error: Selected POM not found. Please select valid POM.";
                                    return;
                                }
                            }
                            else
                            {
                                act.Error = "Error: Selected POM not found (Empty or Invalid POM Guid). Please select valid POM.";
                                return;
                            }
                        }
                        else
                        {
                            gotoUrl = act.GetInputParamCalculatedValue("Value");
                        }

                        if (string.IsNullOrEmpty(gotoUrl))
                        {
                            act.Error = "Error: Provided URL is empty. Please provide valid URL.";
                            return;
                        }

                        if (act.GetInputParamValue(ActBrowserElement.Fields.GotoURLType) == nameof(ActBrowserElement.eGotoURLType.NewTab))
                        {
                            try
                            {
                                OpenNewTab();
                            }
                            catch (Exception ex)
                            {
                                act.Status = eRunStatus.Failed;
                                act.Error += ex.Message;
                                Reporter.ToLog(eLogLevel.DEBUG, $"OpenNewTab {ex.Message} ", ex.InnerException);
                            }

                        }
                        else if (act.GetInputParamValue(ActBrowserElement.Fields.GotoURLType) == nameof(ActBrowserElement.eGotoURLType.NewWindow))
                        {
                            try
                            {
                                OpenNewWindow();
                            }
                            catch (Exception ex)
                            {
                                act.Status = eRunStatus.Failed;
                                act.Error += ex.Message;
                                Reporter.ToLog(eLogLevel.DEBUG, $"OpenNewWindow {ex.Message} ", ex.InnerException);
                            }
                        }

                        GotoURL(act, gotoUrl);
                        break;
                    case ActBrowserElement.eControlAction.Close:
                        Driver.Close();
                        break;

                    case ActBrowserElement.eControlAction.InitializeBrowser:
                        this.StartDriver();
                        break;

                    case ActBrowserElement.eControlAction.SwitchFrame:
                        HandleSwitchFrame(act);
                        break;

                    case ActBrowserElement.eControlAction.SwitchToDefaultFrame:
                        Driver.SwitchTo().DefaultContent();
                        break;

                    case ActBrowserElement.eControlAction.SwitchToParentFrame:
                        Driver.SwitchTo().ParentFrame();
                        break;

                    case ActBrowserElement.eControlAction.Refresh:
                        Driver.Navigate().Refresh();
                        break;

                    case ActBrowserElement.eControlAction.SwitchWindow:
                        SwitchWindow(act);
                        break;

                    case ActBrowserElement.eControlAction.GetWindowTitle:
                        string title = Driver.Title;
                        if (!string.IsNullOrEmpty(title))
                        {
                            act.AddOrUpdateReturnParamActual("Actual", title);
                        }
                        else
                        {
                            act.AddOrUpdateReturnParamActual("Actual", "");
                        }

                        break;

                    case ActBrowserElement.eControlAction.DeleteAllCookies:
                        Driver.Manage().Cookies.DeleteAllCookies();
                        break;

                    case ActBrowserElement.eControlAction.GetPageSource:
                        act.AddOrUpdateReturnParamActual("PageSource", Driver.PageSource);
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultWindow:
                        Driver.SwitchTo().Window(DefaultWindowHandler);
                        break;

                    case ActBrowserElement.eControlAction.GetPageURL:
                        act.AddOrUpdateReturnParamActual("PageURL", Driver.Url);
                        Uri currentUrl = new Uri(Driver.Url);
                        act.AddOrUpdateReturnParamActual("Host", currentUrl.Host);
                        act.AddOrUpdateReturnParamActual("Path", currentUrl.LocalPath);
                        act.AddOrUpdateReturnParamActual("PathWithQuery", currentUrl.PathAndQuery);
                        break;
                    case ActBrowserElement.eControlAction.InjectJS:
                        AddJavaScriptToPage(act.ActInputValues[0].Value);
                        break;
                    case ActBrowserElement.eControlAction.RunJavaScript:
                        string script = act.GetInputParamCalculatedValue("Value");
                        try
                        {
                            object a = null;
                            if (!script.StartsWith("RETURN", StringComparison.CurrentCultureIgnoreCase))
                            {
                                script = "return " + script;
                            }
                            a = ((IJavaScriptExecutor)Driver).ExecuteScript(script);
                            if (a != null)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", a.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Failed to run the JavaScript: '" + script + "', Error: '" + ex.Message + "'";
                        }
                        break;
                    case ActBrowserElement.eControlAction.CheckPageLoaded:
                        CheckifPageLoaded();
                        break;
                    case ActBrowserElement.eControlAction.CloseTabExcept:
                        CloseAllTabsExceptOne(act);
                        break;
                    case ActBrowserElement.eControlAction.CloseAll:
                        CloseDriver();
                        break;
                    case ActBrowserElement.eControlAction.GetBrowserLog:

                        String scriptToExecute = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; return network;";
                        var networkLogs = ((IJavaScriptExecutor)Driver).ExecuteScript(scriptToExecute) as ReadOnlyCollection<object>;
                        act.AddOrUpdateReturnParamActual("Raw Response", Newtonsoft.Json.JsonConvert.SerializeObject(networkLogs));
                        foreach (var item in networkLogs)
                        {
                            if (item is Dictionary<string, object> dict)
                            {
                                if (dict.ContainsKey("name"))
                                {
                                    var urlArray = dict.FirstOrDefault(x => x.Key == "name").Value.ToString().Split('/');

                                    var urlString = string.Empty;
                                    if (urlArray.Length > 0)
                                    {
                                        urlString = urlArray[^1];
                                        if (string.IsNullOrEmpty(urlString) && urlArray.Length > 1)
                                        {
                                            urlString = urlArray[^2];
                                        }
                                        foreach (var val in dict)
                                        {
                                            act.AddOrUpdateReturnParamActual(Convert.ToString(urlString + ":[" + val.Key + "]"), Convert.ToString(val.Value));
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    case ActBrowserElement.eControlAction.GetConsoleLog:
                        string logs = Newtonsoft.Json.JsonConvert.SerializeObject(Driver.Manage().Logs.GetLog(OpenQA.Selenium.LogType.Browser));
                        string filePath = act.GetInputParamCalculatedValue("Value");
                        switch (CreateConsoleLogFile(filePath, logs, act))
                        {
                            case 1: // in case the file is created successfully, print a message
                                act.ExInfo = "Created a Console Log file in the path: " + filePath;
                                break;
                            case 0: // in case the filePath is null or empty, output the logs in Ginger
                                act.AddOrUpdateReturnParamActual("Console logs", logs);
                                break;
                        }
                        break;
                    case ActBrowserElement.eControlAction.StartMonitoringNetworkLog:
                        mAct = act;
                        if (ValidateBrowserCompatibility(Driver))
                        {
                            _BrowserHelper = new BrowserHelper(mAct);
                            SetUPDevTools(Driver);
                            StartMonitoringNetworkLog(Driver).GetAwaiter().GetResult();
                        }
                        break;
                    case ActBrowserElement.eControlAction.GetNetworkLog:
                        mAct = act;
                        if (ValidateBrowserCompatibility(Driver))
                        {
                            GetNetworkLogAsync(act).GetAwaiter().GetResult();
                        }
                        break;
                    case ActBrowserElement.eControlAction.StopMonitoringNetworkLog:
                        mAct = act;
                        if (ValidateBrowserCompatibility(Driver))
                        {
                            StopMonitoringNetworkLog(act).GetAwaiter().GetResult();
                        }
                        break;
                    case ActBrowserElement.eControlAction.ClearExistingNetworkLog:
                        mAct = act;
                        if (ValidateBrowserCompatibility(Driver))
                        {
                            ClearExistingNetworkLog();
                        }
                        break;
                    case ActBrowserElement.eControlAction.NavigateBack:
                        Driver.Navigate().Back();
                        break;

                    case ActBrowserElement.eControlAction.AcceptMessageBox:
                        try
                        {
                            Driver.SwitchTo().Alert().Accept();
                        }
                        catch (Exception e)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error when Accepting MessageBox - " + e.Message);
                            return;
                        }
                        break;

                    case ActBrowserElement.eControlAction.DismissMessageBox:
                        try
                        {
                            Driver.SwitchTo().Alert().Dismiss();
                        }
                        catch (Exception e)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error when Dismiss Alert Box - " + e.Message);
                            return;
                        }
                        break;

                    case ActBrowserElement.eControlAction.GetMessageBoxText:
                        try
                        {
                            string AlertBoxText = Driver.SwitchTo().Alert().Text;
                            act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                            if (act.GetReturnParam("Actual") == null)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
                            }
                        }
                        catch (Exception e)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error to Get Text Message Box - " + e.Message);
                            return;
                        }
                        break;

                    case ActBrowserElement.eControlAction.SetAlertBoxText:
                        try
                        {
                            Driver.SwitchTo().Alert().SendKeys(act.GetInputParamCalculatedValue("Value"));
                        }
                        catch (Exception e)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Error to Get Text Alert Box - " + e.Message);
                            return;
                        }
                        break;
                    case ActBrowserElement.eControlAction.SetBlockedUrls:
                    case ActBrowserElement.eControlAction.UnblockeUrls:
                        mAct = act;
                        SetUPDevTools(Driver);
                        GotoURL(act, Driver.Url);
                        break;
                    case ActBrowserElement.eControlAction.SwitchToShadowDOM:
                        HandleSwitchToShadowDOM(act);
                        break;
                    case ActBrowserElement.eControlAction.SwitchToDefaultDOM:
                        HandleSwitchToDefaultDOM();
                        break;
                    default:
                        throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
                }
            }
            catch (Exception ex)
            {
                act.Status = eRunStatus.Failed;
                act.Error += ex.Message;
                Reporter.ToLog(eLogLevel.DEBUG, $"ActBrowserElementHandler {ex.Message} ", ex.InnerException);
            }
        }

        private void OpenNewWindow()
        {
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Driver;
            javaScriptExecutor.ExecuteScript("newwindow=window.open('about:blank','newWindow','height=250,width=350');if (window.focus) { newwindow.focus()}return false; ");
            Driver.SwitchTo().Window(Driver.WindowHandles[^1]);
            Driver.Manage().Window.Maximize();
        }

        private void OpenNewTab()
        {
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Driver;
            javaScriptExecutor.ExecuteScript("window.open();");
            Driver.SwitchTo().Window(Driver.WindowHandles[^1]);
        }

        public string GetSearchedWinTitle(Act act)
        {
            string searchedWinTitle = string.Empty;

            if (act is ActUIElement actUIElement)
            {
                if (string.IsNullOrEmpty(actUIElement.ElementLocateValue))
                {
                    act.Error = "Error: The window title to search for is missing.";
                    return act.Error;
                }
                else
                {
                    return actUIElement.ElementLocateValue;
                }
            }

            if (String.IsNullOrEmpty(act.ValueForDriver) && String.IsNullOrEmpty(act.LocateValueCalculated))
            {
                act.Error = "Error: The window title to search for is missing.";
                return act.Error;
            }
            else
            {
                if (String.IsNullOrEmpty(act.LocateValueCalculated) == false)
                {
                    searchedWinTitle = act.LocateValueCalculated;
                }
                else
                {
                    searchedWinTitle = act.ValueForDriver;
                }
            }
            return searchedWinTitle;
        }

        public void CloseAllTabsExceptOne(Act act)
        {
            string originalHandle = string.Empty;
            string searchedWinTitle = GetSearchedWinTitle(act);
            ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
            foreach (String winHandle in openWindows)
            {
                if (act.LocateBy == eLocateBy.ByTitle)
                {
                    string winTitle = Driver.SwitchTo().Window(winHandle).Title;
                    if (winTitle.IndexOf(searchedWinTitle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        originalHandle = Driver.CurrentWindowHandle;
                        act.ExInfo = winTitle;
                        continue;
                    }
                    else
                    {
                        Driver.Close();
                    }
                }
                if (act.LocateBy == eLocateBy.ByUrl)
                {
                    string winurl = Driver.SwitchTo().Window(winHandle).Url;
                    if (winurl.IndexOf(searchedWinTitle, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        originalHandle = Driver.CurrentWindowHandle;
                        act.ExInfo = winurl;
                        continue;
                    }
                    else
                    {
                        Driver.Close();
                    }
                }
            }

            Driver.SwitchTo().Window(originalHandle);
        }
        public void ActAgentManipulationHandler(ActAgentManipulation act)
        {
            switch (act.AgentManipulationActionType)
            {

                case ActAgentManipulation.eAgenTManipulationActionType.CloseAgent:
                    CloseDriver();
                    break;

                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }
        }
        // ----------------------------------------------------------------------------------------------------------------------------------
        // New HandleActUIElement - will replace ActGenElement
        // ----------------------------------------------------------------------------------------------------------------------------------

        public void HandleActUIElement(ActUIElement act)
        {
            IWebElement e = null;

            if (act.ElementLocateBy != eLocateBy.NA && (!act.ElementType.Equals(eElementType.Window) && !act.ElementAction.Equals(ActUIElement.eElementAction.Switch)))
            {
                if (act.ElementAction.Equals(ActUIElement.eElementAction.IsVisible))
                {
                    e = LocateElement(act, true);
                }
                else
                {
                    e = LocateElement(act);
                    if (e == null)
                    {
                        if (act.ElementLocateBy == eLocateBy.POMElement)
                        {
                            POMExecutionUtils pomExcutionUtil;
                            ApplicationPOMModel currentPOM;
                            GetCurrentPOM(act, out pomExcutionUtil, out currentPOM);

                            if (currentPOM != null)
                            {
                                ElementInfo currentPOMElementInfo = null;
                                if (isAppiumSession)
                                {
                                    currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo(this.PomCategory);//consider the Category only in case of Mobile flow for now
                                }
                                else
                                {
                                    currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo();
                                }

                                if (currentPOMElementInfo != null)
                                {
                                    act.Error = $"{act.Error} POM Element not found: POM = '{currentPOM.Name}' , Element = '{currentPOMElementInfo.ElementName}'";
                                    return;
                                }
                            }
                        }
                        else
                        {
                            act.Error += "Element not found: " + act.ElementLocateBy + "=" + act.ElementLocateValueForDriver;
                            return;
                        }
                    }
                }
            }

            try
            {
                switch (act.ElementAction)
                {
                    case ActUIElement.eElementAction.Click:
                        DoUIElementClick(act.ElementAction, e);
                        break;

                    case ActUIElement.eElementAction.JavaScriptClick:
                        DoUIElementClick(act.ElementAction, e);
                        break;

                    case ActUIElement.eElementAction.GetValue:
                        if (act.ElementType == eElementType.HyperLink)
                        {
                            if (e != null)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("href"));
                            }
                            else
                            {
                                act.AddOrUpdateReturnParamActual("Actual", "");
                            }
                        }
                        else
                        {
                            act.AddOrUpdateReturnParamActual("Actual", GetElementValue(e));
                        }
                        break;

                    case ActUIElement.eElementAction.IsVisible:
                        if (e != null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
                        }
                        else
                        {
                            act.ExInfo += "Element not found: " + act.ElementLocateBy + "=" + act.ElementLocateValueForDriver;
                            act.AddOrUpdateReturnParamActual("Actual", "False");
                        }
                        break;

                    case ActUIElement.eElementAction.SetValue:
                        if (e.TagName == "select")
                        {
                            SelectElement combobox = new SelectElement(e);
                            string val = act.GetInputParamCalculatedValue("Value");
                            combobox.SelectByText(val);
                            act.ExInfo += "Selected Value - " + val;
                            return;
                        }
                        if (e.TagName == "input" && e.GetAttribute("type") == "checkbox")
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('checked',arguments[1])", e, act.ValueForDriver);
                            return;
                        }

                        //Special case for FF 
                        if (Driver.GetType() == typeof(FirefoxDriver) && e.TagName == "input" && e.GetAttribute("type") == "text")
                        {
                            e.Clear();
                            try
                            {
                                foreach (char i in act.GetInputParamCalculatedValue("Value"))
                                {
                                    e.SendKeys(GetKeyName(Char.ToString(i)));
                                }
                            }
                            catch (InvalidOperationException ex)
                            {
                                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occurred when HandleActUIElement");
                            }
                        }
                        else
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                        }

                        break;

                    case ActUIElement.eElementAction.SendKeys:
                        e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                        break;

                    case ActUIElement.eElementAction.Submit:
                        e.SendKeys("");
                        e.Submit();
                        break;

                    case ActUIElement.eElementAction.GetSize:
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("size")?.ToString());
                        break;

                    //case ActUIElement.eElementAction.SelectByIndex:
                    //    List<IWebElement> els = LocateElements(act.LocateBy, act.LocateValueCalculated);
                    //    if (els != null)
                    //    {
                    //        try
                    //        {
                    //            els[Convert.ToInt32(act.GetInputParamCalculatedValue("Value"))].Click();
                    //        }
                    //        catch (Exception)
                    //        {
                    //            act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                    //        return;
                    //    }
                    //    break;

                    case ActUIElement.eElementAction.GetText:
                        OpenQA.Selenium.Interactions.Actions actionGetText = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actionGetText.MoveToElement(e).Build().Perform();
                        string text = e.GetAttribute("textContent");
                        if (String.IsNullOrEmpty(text))
                        {
                            text = e.GetAttribute("innerText");
                        }
                        if (String.IsNullOrEmpty(text))
                        {
                            text = e.GetAttribute("value");
                        }
                        act.AddOrUpdateReturnParamActual("Actual", text);
                        break;

                    case ActUIElement.eElementAction.GetAttrValue:
                        OpenQA.Selenium.Interactions.Actions actionGetAttrValue = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actionGetAttrValue.MoveToElement(e).Build().Perform();
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute(act.ValueForDriver));
                        break;

                    case ActUIElement.eElementAction.ScrollToElement:
                        try
                        {
                            var js = (IJavaScriptExecutor)Driver;
                            string command = string.Empty;
                            string scrollAlignment = act.GetInputParamCalculatedValue(ActUIElement.Fields.VerticalScrollAlignment);

                            switch (scrollAlignment)
                            {
                                case nameof(eScrollAlignment.Center):
                                    try
                                    {
                                        long elementPosition = e.Location.Y;
                                        long viewportHeight = (long)js.ExecuteScript("return window.innerHeight");
                                        long scrollPosition = elementPosition - (viewportHeight / 2);
                                        command = $"window.scrollTo(0, {scrollPosition});";
                                    }
                                    catch (Exception ex)
                                    {
                                        // Log the exception with a detailed message  
                                        Reporter.ToLog(eLogLevel.DEBUG, $"An error occurred while calculating the scroll position: {ex.Message}");

                                        // Prepare the fallback scroll command  
                                        command = "arguments[0].scrollIntoView({ behavior: 'smooth', block: 'center', inline: 'nearest' });";
                                    }
                                    break;
                                case nameof(eScrollAlignment.End):
                                    command = "arguments[0].scrollIntoView(false);";
                                    break;
                                case nameof(eScrollAlignment.Nearest):
                                    command = "arguments[0].scrollIntoView({ behavior: 'auto', block: 'nearest', inline: 'nearest' });";
                                    break;
                                case nameof(eScrollAlignment.Start):
                                default:
                                    command = "arguments[0].scrollIntoView(true);";
                                    break;
                            }
                            js.ExecuteScript(command, e);
                        }
                        catch (Exception)
                        {
                            act.Error = "Error: Failed to scroll to element - " + act.LocateBy + " " + act.LocateValue;
                        }
                        break;

                    case ActUIElement.eElementAction.RunJavaScript:
                        string script = act.GetInputParamCalculatedValue("Value");
                        try
                        {
                            if (string.IsNullOrEmpty(script))
                            {
                                act.Error = "Script is empty";
                            }
                            else
                            {
                                object a = null;
                                if (!script.ToUpper().StartsWith("RETURN"))
                                {
                                    script = "return " + script;
                                }
                                if (act.ElementLocateBy != eLocateBy.NA)
                                {
                                    if (script.ToLower().Contains("arguments[0]") && e != null)
                                    {
                                        a = ((IJavaScriptExecutor)Driver).ExecuteScript(script, e);
                                    }
                                }
                                else
                                {
                                    a = ((IJavaScriptExecutor)Driver).ExecuteScript(script);
                                }

                                if (a != null)
                                {
                                    act.AddOrUpdateReturnParamActual("Actual", a.ToString());
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Failed to run the JavaScript: '" + script + "', Error: '" + ex.Message + "', if element need to be embedded in the script so make sure you use the 'arguments[0]' place holder for it.";
                        }
                        break;


                    case ActUIElement.eElementAction.DoubleClick:
                        DoUIElementClick(act.ElementAction, e);
                        break;

                    case ActUIElement.eElementAction.MouseRightClick:
                        OpenQA.Selenium.Interactions.Actions actionMouseRightClick = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actionMouseRightClick.ContextClick(e).Build().Perform();
                        break;

                    case ActUIElement.eElementAction.MultiClicks:
                        List<IWebElement> eles = LocateElements(act.ElementLocateBy, act.ElementLocateValueForDriver);
                        if (eles != null)
                        {
                            try
                            {
                                foreach (IWebElement el in eles)
                                {
                                    el.Click();
                                    Thread.Sleep(2000);
                                }
                            }
                            catch (Exception)
                            {
                                act.Error = "One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                            }
                        }
                        else
                        {
                            act.Error = "Error: One or more elements not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                            return;
                        }
                        break;

                    case ActUIElement.eElementAction.MultiSetValue:
                        List<IWebElement> textels = LocateElements(act.ElementLocateBy, act.ElementLocateValueForDriver);
                        if (textels != null)
                        {
                            try
                            {
                                foreach (IWebElement el in textels)
                                {
                                    el.Clear();
                                    el.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                    Thread.Sleep(2000);
                                }
                            }
                            catch (Exception)
                            {
                                act.Error = "Error: One or more elements not found - " + act.ElementLocateBy + " " + act.ElementLocateValueForDriver;
                            }
                        }
                        else
                        {
                            act.Error = "Error: One or more elements not found - " + act.ElementLocateBy + " " + act.ElementLocateValueForDriver;
                            return;
                        }
                        break;

                    case ActUIElement.eElementAction.IsDisabled:
                        if ((e.Displayed && e.Enabled))
                        {
                            act.AddOrUpdateReturnParamActual("Actual", "False");
                            act.ExInfo = "Element displayed property is " + e.Displayed + "Element Enabled property is:" + e.Enabled;
                            return;
                        }
                        else
                        {
                            act.AddOrUpdateReturnParamActual("Actual", "true");
                        }
                        break;

                    case ActUIElement.eElementAction.GetItemCount:
                        try
                        {
                            List<IWebElement> elements = LocateElements(act.ElementLocateBy, act.ElementLocateValueForDriver);
                            if (elements != null)
                            {
                                act.AddOrUpdateReturnParamActual("Elements Count", elements.Count.ToString());
                            }
                            else
                            {
                                act.AddOrUpdateReturnParamActual("Elements Count", "0");
                            }
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Failed to count number of elements for - " + act.ElementLocateBy + " " + act.ElementLocateValueForDriver;
                            act.ExInfo = ex.Message;
                        }
                        break;

                    case ActUIElement.eElementAction.ClickXY:
                        MoveToElementActions(act);
                        break;
                    case ActUIElement.eElementAction.DoubleClickXY:
                        MoveToElementActions(act);
                        break;
                    case ActUIElement.eElementAction.SendKeysXY:
                        MoveToElementActions(act);
                        break;
                    case ActUIElement.eElementAction.IsEnabled:
                        act.AddOrUpdateReturnParamActual("Enabled", e.Enabled.ToString());
                        break;

                    case ActUIElement.eElementAction.MouseClick:
                        DoUIElementClick(act.ElementAction, e);
                        break;
                    case ActUIElement.eElementAction.MousePressRelease:
                        DoUIElementClick(act.ElementAction, e);
                        break;
                    case ActUIElement.eElementAction.ClickAndValidate:
                        ClickAndValidteHandler(act);
                        break;
                    case ActUIElement.eElementAction.SetText:
                        try
                        {
                            ClearText(e);
                        }
                        finally
                        {
                            e.SendKeys(act.ValueForDriver);
                        }
                        break;
                    case ActUIElement.eElementAction.AsyncClick:
                        DoUIElementClick(act.ElementAction, e);
                        break;
                    case ActUIElement.eElementAction.DragDrop:
                        DoDragAndDrop(act, e);
                        break;
                    case ActUIElement.eElementAction.DrawObject:
                        DoDrawObject(act, e);
                        break;

                    case ActUIElement.eElementAction.Select:
                        SelectElement seSetSelectedValueByValu = new SelectElement(e);
                        SelectDropDownListOptionByValue(act, act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect), seSetSelectedValueByValu);
                        break;
                    case ActUIElement.eElementAction.GetValidValues:
                        GetDropDownListOptions(act, e);
                        break;
                    case ActUIElement.eElementAction.SelectByText:
                        SelectDropDownListOptionByText(act, act.GetInputParamCalculatedValue(ActUIElement.Fields.Value), e);
                        break;
                    case ActUIElement.eElementAction.SelectByIndex:
                        SelectElement seSetSelectedValueByIndex = new SelectElement(e);
                        SelectDropDownListOptionByIndex(act, Int32.Parse(act.GetInputParamCalculatedValue(ActUIElement.Fields.ValueToSelect)), seSetSelectedValueByIndex);
                        break;
                    case ActUIElement.eElementAction.GetSelectedValue:
                        SelectElement seGetSelectedValue = new SelectElement(e);
                        act.AddOrUpdateReturnParamActual("Actual", seGetSelectedValue.SelectedOption.Text);
                        break;
                    case ActUIElement.eElementAction.IsValuePopulated:
                        switch (act.ElementType)
                        {
                            case eElementType.ComboBox:
                                SelectElement seIsPrepopulated = new SelectElement(e);
                                act.AddOrUpdateReturnParamActual("Actual", (seIsPrepopulated.SelectedOption.ToString().Trim() != "").ToString());
                                break;
                            case eElementType.TextBox:
                                act.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("value").Trim() != "").ToString());
                                break;
                        }
                        break;
                    case ActUIElement.eElementAction.GetFont:
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("font"));
                        break;
                    case ActUIElement.eElementAction.ClearValue:
                        ClearText(e);
                        break;
                    case ActUIElement.eElementAction.GetHeight:
                        act.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
                        break;
                    case ActUIElement.eElementAction.GetWidth:
                        act.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
                        break;
                    case ActUIElement.eElementAction.GetStyle:
                        try { act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style")); }
                        catch { act.AddOrUpdateReturnParamActual("Actual", "no such attribute"); }
                        break;
                    case ActUIElement.eElementAction.SetFocus:
                    case ActUIElement.eElementAction.Hover:
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(e).Build().Perform();
                        break;
                    case ActUIElement.eElementAction.GetTextLength:
                        act.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("value").Length).ToString());
                        break;
                    case ActUIElement.eElementAction.Switch:
                        SwitchWindow(act);
                        break;
                    default:
                        act.Error = "Error: Unknown Action: " + act.ElementAction;
                        break;
                }
            }
            catch(Exception ex)
            {
                act.Status = eRunStatus.Failed;
                act.Error = $"Action '{act.ElementAction}' failed: {ex.Message}";
                Reporter.ToLog(eLogLevel.ERROR, $"Action '{act.ElementAction}' failed", ex);
            }
            finally
            {
                if (act.ElementLocateBy == eLocateBy.POMElement && HandelIFramShiftAutomaticallyForPomElement)
                {
                    Driver.SwitchTo().DefaultContent();
                }
            }
        }


        private string GetElementValue(IWebElement webElement)
        {
            if (!string.IsNullOrEmpty(webElement.Text))
            {
                return webElement.Text;
            }
            else
            {
                return webElement.GetAttribute("value");
            }
        }

        private void ClearText(IWebElement webElement)
        {
            webElement.Clear();
            string elementValue = GetElementValue(webElement);
            if (!string.IsNullOrEmpty(elementValue))
            {
                int length = elementValue.Length;

                for (int i = 0; i < length; i++)
                {
                    webElement.SendKeys(Keys.Backspace);
                }
            }
        }

        private void DoDrawObject(ActUIElement act, IWebElement e)
        {
            OpenQA.Selenium.Interactions.Actions actionBuilder = new OpenQA.Selenium.Interactions.Actions(Driver);
            Random rnd = new Random();

            OpenQA.Selenium.Interactions.IAction drawAction = actionBuilder.MoveToElement(e, rnd.Next(e.Size.Width / 98, e.Size.Width / 90), rnd.Next(e.Size.Height / 4, e.Size.Height / 3))
                               .Click()
                               .ClickAndHold(e)
                               .MoveByOffset(rnd.Next(e.Size.Width / 95, e.Size.Width / 75), -rnd.Next(e.Size.Height / 6, e.Size.Height / 3))
                               .MoveByOffset(-rnd.Next(e.Size.Width / 30, e.Size.Width / 15), rnd.Next(e.Size.Height / 12, e.Size.Height / 8))
                               .MoveByOffset(rnd.Next(e.Size.Width / 95, e.Size.Width / 80), rnd.Next(e.Size.Height / 12, e.Size.Height / 8))
                               .MoveByOffset(rnd.Next(e.Size.Width / 30, e.Size.Width / 10), -rnd.Next(e.Size.Height / 12, e.Size.Height / 8))
                               .MoveByOffset(-rnd.Next(e.Size.Width / 95, e.Size.Width / 65), rnd.Next(e.Size.Height / 6, e.Size.Height / 3))
                               .Release(e)
                .Build();
            drawAction.Perform();
        }

        private void DoDragAndDrop(ActUIElement act, IWebElement e)
        {
            var sourceElement = e;

            string TargetElementLocatorValue = act.GetInputParamCalculatedValue(ActUIElement.Fields.TargetLocateValue.ToString());

            if (act.TargetLocateBy != eLocateBy.ByXY)
            {
                string TargetElementLocator = act.TargetLocateBy.ToString();
                IWebElement targetElement = LocateElement(act, true, TargetElementLocator, TargetElementLocatorValue);
                if (targetElement != null)
                {
                    ActUIElement.eElementDragDropType dragDropType;
                    if (act.GetInputParamValue(ActUIElement.Fields.DragDropType) == null || Enum.TryParse<ActUIElement.eElementDragDropType>(act.GetInputParamValue(ActUIElement.Fields.DragDropType).ToString(), out dragDropType) == false)
                    {
                        act.Error = "Failed to perform drag and drop, invalid drag and drop type";
                    }
                    else
                    {
                        switch (dragDropType)
                        {
                            case ActUIElement.eElementDragDropType.DragDropSelenium:
                                OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                                OpenQA.Selenium.Interactions.IAction dragdrop = action.ClickAndHold(sourceElement).MoveToElement(targetElement).Release(targetElement).Build();
                                dragdrop.Perform();
                                break;
                            case ActUIElement.eElementDragDropType.DragDropJS:
                                string script = JavaScriptHandler.GetJavaScriptFileContent(JavaScriptHandler.eJavaScriptFile.draganddrop);//Correct JS?//Properties.Resources.Html5DragAndDrop;
                                IJavaScriptExecutor executor = (IJavaScriptExecutor)Driver;
                                executor.ExecuteScript(script, sourceElement, targetElement);
                                break;
                            default:
                                act.Error = "Failed to perform drag and drop, invalid drag and drop type";
                                break;

                        }
                        //TODO: Add validation to verify if Drag and drop is performed or not and fail the action if needed
                    }
                }
                else
                {
                    act.Error = "Target Element not found: " + TargetElementLocator + "=" + TargetElementLocatorValue;
                }
            }
            else
            {
                var xLocator = Convert.ToInt32(act.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate));
                var yLocator = Convert.ToInt32(act.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate));
                DoDragandDropByOffSet(sourceElement, xLocator, yLocator);
            }
        }

        private void DoDragandDropByOffSet(IWebElement sourceElement, int xLocator, int yLocator)
        {
            OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
            action.DragAndDropToOffset(sourceElement, xLocator, yLocator).Build().Perform();
        }

        public void DoUIElementClick(ActUIElement.eElementAction clickType, IWebElement clickElement)
        {
            switch (clickType)
            {
                case ActUIElement.eElementAction.Click:
                        clickElement.Click();
                    break;

                case ActUIElement.eElementAction.JavaScriptClick:                    
                        ((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].click()", clickElement);
                    break;

                case ActUIElement.eElementAction.MouseClick:                    
                        OpenQA.Selenium.Interactions.Actions action = new OpenQA.Selenium.Interactions.Actions(Driver);
                        action.MoveToElement(clickElement).Click().Build().Perform();                    
                    break;

                case ActUIElement.eElementAction.DoubleClick:
                        OpenQA.Selenium.Interactions.Actions actionDoubleClick = new OpenQA.Selenium.Interactions.Actions(Driver);
                    actionDoubleClick.DoubleClick(clickElement).Build().Perform();
                    break;

                case ActUIElement.eElementAction.MousePressRelease:
                    InputSimulator inp = new InputSimulator();
                    inp.Mouse.MoveMouseTo(1.0, 1.0);
                    inp.Mouse.MoveMouseBy((int)((clickElement.Location.X + 5) / 1.33), (int)((clickElement.Location.Y + 5) / 1.33));
                    inp.Mouse.LeftButtonClick();
                    break;
                case ActUIElement.eElementAction.AsyncClick:
                    try
                    {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("var el=arguments[0]; setTimeout(function() { el.click(); }, 100);", clickElement);
                    }
                    catch (Exception)
                    {
                        clickElement.Click();
                    }
                    break;
            }
        }

        public bool ClickAndValidteHandler(ActUIElement act)
        {
            IWebElement clickElement = LocateElement(act);

            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ClickType).ToString(), out var clickType) == false)
            {
                act.Error = "Unknown Click Type";
                return false;
            }

            // Validation Element locate by:
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString(), out var validationElementLocateby) == false)
            {
                act.Error = "Unknown Validation Element Locate By";
                return false;
            }

            //Validation Element Locator Value:
            string validationElementLocatorValue = act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocatorValue.ToString());

            //Validation Type:
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString(), out var validationType) == false)
            {
                act.Error = "Unknown Validation Type";
                return false;
            }

            //Loop through clicks flag check:
            bool ClickLoop = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.LoopThroughClicks).ToString()) == "True")
            {
                ClickLoop = true;
            }

            //Do click:
            DoUIElementClick(clickType, clickElement);
            //check if validation element exists
            IWebElement elmToValidate = LocateElement(act, true, validationElementLocateby.ToString(), validationElementLocatorValue);
            bool assertValidationType(IWebElement element, ActUIElement.eElementAction validationType)
            {
                bool validationResult = false;
                switch (validationType)
                {
                    case ActUIElement.eElementAction.IsEnabled:
                        validationResult = elmToValidate.Enabled;
                        break;
                    case ActUIElement.eElementAction.IsVisible:
                        validationResult = elmToValidate.Displayed;
                        break;
                }

                if (!validationResult)
                {
                    act.Error = $"Validation {validationType} failed";
                }
                return validationResult;
            }

            if (elmToValidate != null)
            {
                return assertValidationType(elmToValidate, validationType);
            }
            else
            {
                if (ClickLoop)
                {
                    Platforms.PlatformsInfo.WebPlatform webPlatform = new Platforms.PlatformsInfo.WebPlatform();
                    List<ActUIElement.eElementAction> clicks = webPlatform.GetPlatformUIClickTypeList();

                    ActUIElement.eElementAction executedClick = clickType;
                    foreach (ActUIElement.eElementAction singleclick in clicks)
                    {
                        if (singleclick != executedClick)
                        {
                            DoUIElementClick(singleclick, clickElement);
                            elmToValidate = LocateElement(act, true, validationElementLocateby.ToString(), validationElementLocatorValue);
                            if (elmToValidate != null)
                            {
                                return assertValidationType(elmToValidate, validationType);
                            }
                        }
                    }
                }
            }
            act.Error = "Error:  Validation Element not found - " + validationElementLocateby + " Using Value : " + validationElementLocatorValue;
            return false;
        }

        public HtmlDocument SSPageDoc = null;

        private Bitmap CaptureFullPageScreenshot()
        {
            try
            {
                // Scroll to Top
                ((IJavaScriptExecutor)Driver).ExecuteScript(string.Format("window.scrollTo(0,0)"));

                // Get the total size of the page
                var totalWidth = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return document.body.offsetWidth") + 380;
                var totalHeight = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return  document.body.parentNode.scrollHeight");

                // Get the size of the viewport
                var viewportWidth = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return document.body.clientWidth") + 380;
                var viewportHeight = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return window.innerHeight");

                // We only care about taking multiple images together if it doesn't already fit
                if ((totalWidth <= viewportWidth) && (totalHeight <= viewportHeight))
                {
                    var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                    return ScreenshotToImage(screenshot);
                }
                // Split the screen in multiple Rectangles
                var rectangles = new List<Rectangle>();
                // Loop until the totalHeight is reached
                for (var y = 0; y < totalHeight; y += viewportHeight)
                {
                    var newHeight = viewportHeight;
                    // Fix if the height of the element is too big
                    if (y + viewportHeight > totalHeight)
                    {
                        newHeight = totalHeight - y;
                    }
                    // Loop until the totalWidth is reached
                    for (var x = 0; x < totalWidth; x += viewportWidth)
                    {
                        var newWidth = viewportWidth;
                        // Fix if the Width of the Element is too big
                        if (x + viewportWidth > totalWidth)
                        {
                            newWidth = totalWidth - x;
                        }
                        // Create and add the Rectangle
                        var currRect = new Rectangle(x, y, newWidth, newHeight);
                        rectangles.Add(currRect);
                    }
                }
                // Build the Image
                var stitchedImage = new Bitmap(totalWidth, totalHeight);
                // Get all Screenshots and stitch them together
                var previous = Rectangle.Empty;
                foreach (var rectangle in rectangles)
                {
                    // Calculate the scrolling (if needed)
                    if (previous != Rectangle.Empty)
                    {
                        var xDiff = rectangle.Right - previous.Right;
                        var yDiff = rectangle.Bottom - previous.Bottom;
                        // Scroll
                        ((IJavaScriptExecutor)Driver).ExecuteScript(string.Format("window.scrollBy({0}, {1})", xDiff, yDiff));
                    }
                    // Take Screenshot
                    var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                    // Build an Image out of the Screenshot
                    var screenshotImage = ScreenshotToImage(screenshot);
                    // Calculate the source Rectangle
                    var sourceRectangle = new Rectangle(viewportWidth - rectangle.Width, viewportHeight - rectangle.Height, rectangle.Width, rectangle.Height);
                    // Copy the Image
                    using (var graphics = Graphics.FromImage(stitchedImage))
                    {
                        graphics.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    // Set the Previous Rectangle
                    previous = rectangle;
                }
                return stitchedImage;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Selenium WebDriver Browser Page Screenshot", ex);
                return null;
            }
        }

        public Bitmap GetScreenShot(bool IsFullPageScreenshot = false)
        {
            if (!IsFullPageScreenshot)
            {
                // return screenshot of what's visible currently in the viewport
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                return ScreenshotToImage(screenshot);
            }
            Bitmap bitmapImage = null;
            switch (mBrowserType)
            {
                case eBrowserType.FireFox:
                    var screenShot = ((FirefoxDriver)Driver).GetFullPageScreenshot();
                    bitmapImage = ScreenshotToImage(screenShot);
                    break;
                case eBrowserType.Edge:
                case eBrowserType.Chrome:
                    if (Driver is InternetExplorerDriver)
                    {
                        bitmapImage = CaptureFullPageScreenshot();
                    }
                    else
                    {
                        var screenshot = ((OpenQA.Selenium.Chromium.ChromiumDriver)Driver).GetFullPageScreenshot();
                        bitmapImage = ScreenshotToImage(screenshot);
                    }
                    break;
                default:
                    bitmapImage = CaptureFullPageScreenshot();
                    break;
            }
            return bitmapImage;

        }

        public void AddScreenshotIntoAct(ActScreenShot act, bool IsFullPageScreenshot = false)
        {

            if (!IsFullPageScreenshot)
            {
                // return screenshot of what's visible currently in the viewport
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                act.AddScreenShot(screenshot.AsByteArray, Driver.Title);
            }
            switch (mBrowserType)
            {
                case eBrowserType.FireFox:
                    var screenShot = ((FirefoxDriver)Driver).GetFullPageScreenshot();
                    act.AddScreenShot(screenShot.AsByteArray, Driver.Title);
                    break;
                case eBrowserType.Edge:
                case eBrowserType.Chrome:
                    if (Driver is InternetExplorerDriver)
                    {
                        using (Bitmap bitmapImage = CaptureFullPageScreenshot())
                        {
                            act.AddScreenShot(bitmapImage, Driver.Title);
                        }
                    }
                    else
                    {
                        var screenshot = ((OpenQA.Selenium.Chromium.ChromiumDriver)Driver).GetFullPageScreenshot();
                        act.AddScreenShot(screenshot.AsByteArray, Driver.Title);
                    }
                    break;
                default:
                    using (Bitmap bitmapImage = CaptureFullPageScreenshot())
                    {
                        act.AddScreenShot(bitmapImage, Driver.Title);
                    }
                    break;
            }
        }

        private Bitmap ScreenshotToImage(Screenshot screenshot)
        {
            TypeConverter tc = TypeDescriptor.GetConverter(typeof(Bitmap));
            return (Bitmap)tc.ConvertFrom(screenshot.AsByteArray);
        }
        async Task<ElementInfo> IVisualTestingDriver.GetElementAtPoint(long ptX, long ptY)
        {
            try
            {
                HTMLElementInfo elemInfo = null;

                string iframeXPath = string.Empty;
                Point parentElementLocation = new Point(0, 0);

                while (true)
                {
                    string s_Script = "return document.elementFromPoint(arguments[0], arguments[1]);";

                    IWebElement ele = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript(s_Script, ptX, ptY);

                    if (ele == null)
                    {
                        return null;
                    }
                    else
                    {
                        HtmlNode elemNode = null;
                        string elemId;
                        try
                        {
                            elemId = ele.GetDomProperty("id");
                            if (SSPageDoc == null)
                            {
                                SSPageDoc = new HtmlDocument();
                                SSPageDoc.LoadHtml(GetCurrentPageSourceString());
                            }
                            elemNode = SSPageDoc.DocumentNode.Descendants().FirstOrDefault(x => x.Id.Equals(elemId));
                        }
                        catch (Exception exc)
                        {
                            elemId = "";
                        }


                        elemInfo = new HTMLElementInfo();

                        var elemTypeEnum = GetElementTypeEnum(ele);
                        elemInfo.ElementType = elemTypeEnum.Item1;
                        elemInfo.ElementTypeEnum = elemTypeEnum.Item2;
                        elemInfo.ElementObject = ele;
                        elemInfo.Path = iframeXPath;
                        elemInfo.XPath = string.IsNullOrEmpty(elemId) ? GenerateXpathForIWebElement(ele, string.Empty) : elemNode.XPath;
                        elemInfo.HTMLElementObject = elemNode;

                        ((IWindowExplorer)this).LearnElementInfoDetails(elemInfo);
                    }

                    if (elemInfo.ElementTypeEnum != eElementType.Iframe)    // ele.TagName != "frame" && ele.TagName != "iframe")
                    {
                        Driver.SwitchTo().DefaultContent();

                        break;
                    }

                    if (string.IsNullOrEmpty(iframeXPath))
                    {
                        iframeXPath = elemInfo.XPath;
                    }
                    else
                    {
                        iframeXPath += "," + elemInfo.XPath;
                    }

                    parentElementLocation.X += elemInfo.X;
                    parentElementLocation.Y += elemInfo.Y;

                    Point p_Pos = GetElementPosition(ele);
                    ptX -= p_Pos.X;
                    ptY -= p_Pos.Y;

                    Driver.SwitchTo().Frame(ele);
                }

                elemInfo.X += parentElementLocation.X;
                elemInfo.Y += parentElementLocation.Y;

                return elemInfo;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Get Element At Point", ex);
                return null;
            }
        }

        public IWebElement GetElementFromPoint(long X, long Y)
        {
            while (true)
            {
                String s_Script = "return document.elementFromPoint(arguments[0], arguments[1]);";

                IWebElement i_Elem = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript(s_Script, X, Y);
                if (i_Elem == null)
                {
                    return null;
                }

                if (i_Elem.TagName is not "frame" and not "iframe")
                {
                    return i_Elem;
                }

                Point p_Pos = GetElementPosition(i_Elem);
                X -= p_Pos.X;
                Y -= p_Pos.Y;

                Driver.SwitchTo().Frame(i_Elem);
            }
        }

        public Point GetElementPosition(IWebElement i_Elem)
        {
            String s_Script = "var X, Y; "
                            + "if (window.pageYOffset) " // supported by most browsers 
                            + "{ "
                            + "  X = window.pageXOffset; "
                            + "  Y = window.pageYOffset; "
                            + "} "
                            + "else " // Internet Explorer 6, 7, 8
                            + "{ "
                            + "  var  Elem = document.documentElement; "         // <html> node (IE with DOCTYPE)
                            + "  if (!Elem.clientHeight) Elem = document.body; " // <body> node (IE in quirks mode)
                            + "  X = Elem.scrollLeft; "
                            + "  Y = Elem.scrollTop; "
                            + "} "
                            + "return new Array(X, Y);";

            RemoteWebDriver i_Driver = (RemoteWebDriver)((WebElement)i_Elem).WrappedDriver;
            IList<Object> i_Coord = (IList<Object>)i_Driver.ExecuteScript(s_Script);

            int s32_ScrollX = Convert.ToInt32(i_Coord[0]);
            int s32_ScrollY = Convert.ToInt32(i_Coord[1]);

            return new Point(i_Elem.Location.X - s32_ScrollX,
                             i_Elem.Location.Y - s32_ScrollY);
        }

        Bitmap IVisualTestingDriver.GetScreenShot(Tuple<int, int> setScreenSize = null, bool IsFullPageScreenshot = false)
        {
            if (setScreenSize != null)
            {
                try
                {
                    //Driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
                    //System.Drawing.Size originalSize = Driver.Manage().Window.Size;
                    if (setScreenSize == null)
                    {
                        Driver.Manage().Window.Maximize();
                    }
                    else
                    {
                        Driver.Manage().Window.Size = new System.Drawing.Size(setScreenSize.Item1, setScreenSize.Item2);
                    }
                    //Bitmap screenShot = GetScreenShot();
                    //Driver.Manage().Window.Size = originalSize;
                    //return screenShot;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set browser screen size before taking screen shot", ex);
                    return GetScreenShot(IsFullPageScreenshot);
                }
            }

            return GetScreenShot(IsFullPageScreenshot);
        }
        public Bitmap GetElementScreenshot(Act act)
        {
            WebElement element = (WebElement)LocateElement(act, false, null, null);
            return CaptureScrollableElementScreenshot(element);
        }

        private Bitmap CaptureScrollableElementScreenshot(WebElement element)
        {
            try
            {
                // Get the total size of the element
                var offsetWidth = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].offsetWidth", element);
                var scrollHeight = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].scrollHeight", element);

                // Get the size of the viewport
                var clientWidth = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].clientWidth", element);
                var clientHeihgt = (int)(long)((IJavaScriptExecutor)Driver).ExecuteScript("return arguments[0].clientHeight", element);

                // We only care about taking multiple images together if it doesn't already fit
                if ((offsetWidth <= clientWidth) && (scrollHeight <= clientHeihgt))
                {
                    // return screenshot of what's visible currently in the viewport
                    var screenshot = ((ITakesScreenshot)element).GetScreenshot();
                    return ScreenshotToImage(screenshot);
                }
                // Split the screen in multiple Rectangles
                var rectangles = new List<Rectangle>();
                // Loop until the totalHeight is reached
                for (var y = 0; y < scrollHeight; y += clientHeihgt)
                {
                    var newHeight = clientHeihgt;
                    // Fix if the height of the element is too big
                    if (y + clientHeihgt > scrollHeight)
                    {
                        newHeight = scrollHeight - y;
                    }
                    // Loop until the totalWidth is reached
                    for (var x = 0; x < offsetWidth; x += clientWidth)
                    {
                        var newWidth = clientWidth;
                        // Fix if the Width of the Element is too big
                        if (x + clientWidth > offsetWidth)
                        {
                            newWidth = offsetWidth - x;
                        }
                        // Create and add the Rectangle
                        var currRect = new Rectangle(x, y, newWidth, newHeight);
                        rectangles.Add(currRect);
                    }
                }
                // Build the Image
                var stitchedImage = new Bitmap(offsetWidth, scrollHeight);
                // Get all Screenshots and stitch them together
                var previous = Rectangle.Empty;
                foreach (var rectangle in rectangles)
                {
                    // Calculate the scrolling (if needed)
                    if (previous != Rectangle.Empty)
                    {
                        var xDiff = rectangle.Right - previous.Right;
                        var yDiff = rectangle.Bottom - previous.Bottom;
                        // Scroll
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollTo(arguments[1], arguments[2])", element, xDiff, yDiff);
                    }
                    // Take Screenshot
                    var screenshot = ((ITakesScreenshot)element).GetScreenshot();
                    // Build an Image out of the Screenshot
                    var screenshotImage = ScreenshotToImage(screenshot);
                    // Calculate the source Rectangle
                    var sourceRectangle = new Rectangle(clientWidth - rectangle.Width, clientHeihgt - rectangle.Height, rectangle.Width, rectangle.Height);
                    // Copy the Image
                    using (var graphics = Graphics.FromImage(stitchedImage))
                    {
                        graphics.DrawImage(screenshotImage, rectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    // Set the Previous Rectangle
                    previous = rectangle;
                }
                return stitchedImage;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to capture scrollable element screenshot", ex);
                var screenshot = ((ITakesScreenshot)element).GetScreenshot();
                return ScreenshotToImage(screenshot);
            }

        }
        VisualElementsInfo IVisualTestingDriver.GetVisualElementsInfo()
        {
            VisualElementsInfo VEI = new VisualElementsInfo
            {
                Bitmap = GetScreenShot()
            };

            //TODO: add function to get all tags available - below is missing some...
            List<IWebElement> elems = Driver.FindElements(By.TagName("a")).ToList();
            elems.AddRange(Driver.FindElements(By.TagName("input")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("select")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("label")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H1")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H2")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H3")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H4")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H5")).ToList());
            elems.AddRange(Driver.FindElements(By.TagName("H6")).ToList());
            elems.RemoveAll(i => !i.Displayed); //LAMBDA EXPRESSION


            foreach (IWebElement e in elems)
            {
                if (e.Displayed && e.Size.Width > 0 && e.Size.Height > 0)
                {
                    //TODO: add the rest which make sense
                    string txt = GetElementText(e);
                    VisualElement VE = new VisualElement() { ElementType = e.TagName, Text = txt, X = e.Location.X, Y = e.Location.Y, Width = e.Size.Width, Height = e.Size.Height };
                    VEI.Elements.Add(VE);
                }
            }
            return VEI;
        }

        private string GetElementText(IWebElement e)
        {
            string txt = e.Text;
            if (string.IsNullOrEmpty(txt))
            {

                //TODO: handle other types of elem
                if (e.TagName == "input")
                {
                    string ctlType = e.GetAttribute("type");

                    switch (ctlType)
                    {
                        case "text":
                            txt = e.GetAttribute("value");
                            break;
                        case "button":
                            txt = e.GetAttribute("value");
                            break;
                    }

                    if (string.IsNullOrEmpty(txt))
                    {
                        txt = e.GetAttribute("outerHTML");
                    }
                }
            }
            return txt;
        }

        void IVisualTestingDriver.ChangeAppWindowSize(int width, int height)
        {
            if (width == 0 && height == 0)
            {
                Driver.Manage().Window.Maximize();
            }
            else
            {
                Driver.Manage().Window.Size = new System.Drawing.Size(width, height);
            }

        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo EI)
        {
            //TODO: remove from here and put in EI - do lazy loading if needed.
            if (EI == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(EI.XPath))
            {
                EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, "");
            }

            IWebElement e = (IWebElement)EI.ElementObject;
            if (e != null)
            {
                EI.X = e.Location.X;
                EI.Y = e.Location.Y;
                EI.Width = e.Size.Width;
                EI.Height = e.Size.Height;
            }


        }

        private void InitXpathHelper()
        {
            List<string> importantProperties = ["SeleniumDriver", "Web"];
            mXPathHelper = new XPathHelper(this, importantProperties);
        }

        XPathHelper IXPath.GetXPathHelper(ElementInfo info)
        {
            return mXPathHelper;
        }

        ElementInfo IXPath.GetRootElement()
        {
            ElementInfo RootEI = new ElementInfo
            {
                ElementTitle = "html/body",
                ElementType = "root",
                Value = string.Empty,
                Path = string.Empty,
                XPath = "html/body"
            };
            return RootEI;
        }

        ElementInfo IXPath.UseRootElement()
        {
            Driver.SwitchTo().DefaultContent();
            return GetRootElement();
        }

        ElementInfo IXPath.GetElementParent(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            ElementInfo parentEI = null;
            IWebElement parentElementIWebElement = null;
            HtmlNode parentElementHtmlNode = null;
            if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null)
            {
                parentElementHtmlNode = ((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode;
                parentEI = allReadElem.Find(el => el is HTMLElementInfo parentElementInfo && parentElementInfo.HTMLElementObject != null && parentElementInfo.HTMLElementObject.Equals(parentElementHtmlNode));
            }
            else
            {
                ElementInfo.ElementObject ??= Driver.FindElement(By.XPath(ElementInfo.XPath));

                parentElementIWebElement = ((IWebElement)ElementInfo.ElementObject).FindElement(By.XPath(".."));
                parentEI = allReadElem.Find(el => el.ElementObject != null && el.ElementObject.Equals(parentElementIWebElement));
            }

            if (parentEI != null)
            {
                return parentEI;
            }

            IWebElement parentElementObject = parentElementHtmlNode != null ? Driver.FindElement(By.XPath(parentElementHtmlNode.XPath)) : null;

            HTMLElementInfo foundElemntInfo = new HTMLElementInfo
            {
                ElementObject = parentElementObject,
                HTMLElementObject = parentElementHtmlNode
            };
            ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo, pomSetting);

            return foundElemntInfo;
        }

        string IXPath.GetElementID(ElementInfo EI)
        {
            if (EI.ElementObject != null)
            {
                return GenerateElementID(EI.ElementObject);
            }
            else
            {
                return GenerateElementID(((HTMLElementInfo)EI).HTMLElementObject);
            }
        }

        string IXPath.GetElementTagName(ElementInfo EI)
        {
            if (EI.ElementObject != null)
            {
                return ((IWebElement)EI.ElementObject).TagName;
            }
            else if (EI is HTMLElementInfo info && info.HTMLElementObject != null)
            {
                return info.HTMLElementObject.Name;
            }
            return string.Empty;
        }

        List<object> IXPath.GetAllElementsByLocator(eLocateBy LocatorType, string LocValue)
        {
            return LocateElements(LocatorType, LocValue).ToList<object>();
        }

        //private ElementInfo GetElementInfoFromIWebElement(IWebElement el, HtmlNode htmlNode, ElementInfo ChildElementInfo)
        //{
        //    IWebElement webElement = null;
        //    if (el == null)
        //    {
        //        webElement = Driver.FindElement(By.XPath(htmlNode.XPath));
        //    }
        //    else
        //    {
        //        webElement = el;
        //    }
        //    HTMLElementInfo EI = new HTMLElementInfo();
        //    EI.ElementTitle = GenerateElementTitle(webElement);
        //    EI.WindowExplorer = this;
        //    EI.ID = GenerateElementID(webElement);
        //    EI.Value = GenerateElementValue(webElement);
        //    EI.Name = GenerateElementName(webElement);
        //    EI.ElementType = GenerateElementType(webElement);
        //    EI.ElementTypeEnum = GetElementTypeEnum(webElement).Item2;
        //    EI.Path = ChildElementInfo.Path;
        //    if (!string.IsNullOrEmpty(ChildElementInfo.XPath))
        //    {
        //        EI.XPath = ChildElementInfo.XPath.Substring(0, ChildElementInfo.XPath.LastIndexOf("/"));
        //    }
        //    EI.ElementObject = webElement;              // el;
        //    EI.RelXpath = mXPathHelper.GetElementRelXPath(EI);
        //    return EI;
        //}

        string IXPath.GetElementProperty(ElementInfo ElementInfo, string PropertyName)
        {
            string elementProperty = null;

            ElementInfo.ElementObject ??= Driver.FindElement(By.XPath(ElementInfo.XPath));

            if (ElementInfo.ElementObject != null)
            {
                elementProperty = ((IWebElement)ElementInfo.ElementObject).GetAttribute(PropertyName);
            }
            return elementProperty;
        }

        List<ElementInfo> IXPath.GetElementChildren(ElementInfo ElementInfo)
        {
            try
            {
                List<ElementInfo> list = [];
                ReadOnlyCollection<IWebElement> el;
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                SwitchFrame(ElementInfo.Path, ElementInfo.XPath);
                string elementPath = GeneratePath(ElementInfo.XPath);
                el = Driver.FindElements(By.XPath(elementPath));
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan();
                list = GetElementsFromIWebElementList(el, ElementInfo.Path, ElementInfo.XPath);
                Driver.SwitchTo().DefaultContent();
                return list;
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
            }
        }

        ElementInfo IXPath.FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            CurrentFrame = string.Empty;
            ElementInfo returnElementInfo = FindFirst(ElementInfo, conditions);
            Driver.SwitchTo().DefaultContent();
            CurrentFrame = string.Empty;
            return returnElementInfo;
        }

        private ElementInfo FindFirst(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            ReadOnlyCollection<IWebElement> ElementsList = Driver.FindElements(By.CssSelector("*"));
            ElementInfo returnElementInfo;
            int elementInfoValue;
            int elementvalue;
            if (ElementsList.Count != 0)
            {
                foreach (IWebElement el in ElementsList)
                {
                    if (el.TagName == "iframe")
                    {
                        ElementInfo iframeElementInfo = GetElementInfoWithIWebElementWithXpath(el, CurrentFrame);
                        SwitchFrameFromCurrent(iframeElementInfo);
                        FindFirst(ElementInfo, conditions);
                    }
                    else if (el.TagName == ElementInfo.ElementType)
                    {
                        bool allTestsPassed = true;
                        foreach (XpathPropertyCondition XPC in conditions)
                        {
                            string value = el.GetAttribute(XPC.PropertyName);
                            switch (XPC.Op)
                            {
                                case XpathPropertyCondition.XpathConditionOperator.Equel:
                                    if (ElementInfo.Value != value)
                                    {
                                        allTestsPassed = false;
                                    }

                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.Less:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue < elementvalue)
                                    {
                                        allTestsPassed = false;
                                    }

                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.More:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue > elementvalue)
                                    {
                                        returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
                                    }

                                    break;
                            }
                        }
                        if (allTestsPassed)
                        {
                            returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
                            return returnElementInfo;
                        }
                    }
                }

            }
            return null;
        }

        List<ElementInfo> IXPath.FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            CurrentFrame = string.Empty;
            List<ElementInfo> list = FindAll(ElementInfo, conditions);
            Driver.SwitchTo().DefaultContent();
            CurrentFrame = string.Empty;
            return list;
        }

        private List<ElementInfo> FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            List<ElementInfo> list = [];
            ReadOnlyCollection<IWebElement> ElementsList = Driver.FindElements(By.CssSelector("*"));
            ElementInfo returnElementInfo;
            int elementInfoValue;
            int elementvalue;
            if (ElementsList.Count != 0)
            {
                foreach (IWebElement el in ElementsList)
                {
                    if (el.TagName == "iframe")
                    {
                        ElementInfo iframeElementInfo = GetElementInfoWithIWebElementWithXpath(el, CurrentFrame);
                        SwitchFrameFromCurrent(iframeElementInfo);
                        list.AddRange(FindAll(ElementInfo, conditions));
                    }
                    else if (el.TagName == ElementInfo.ElementType)
                    {
                        bool allTestsPassed = true;
                        foreach (XpathPropertyCondition XPC in conditions)
                        {

                            string value = el.GetAttribute(XPC.PropertyName);
                            switch (XPC.Op)
                            {
                                case XpathPropertyCondition.XpathConditionOperator.Equel:
                                    if (ElementInfo.Value != value)
                                    {
                                        allTestsPassed = false;
                                    }
                                    break;

                                case XpathPropertyCondition.XpathConditionOperator.Less:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue < elementvalue)
                                    {
                                        allTestsPassed = false;
                                    }
                                    break;

                                case XpathPropertyCondition.XpathConditionOperator.More:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue > elementvalue)
                                    {
                                        returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
                                    }
                                    break;
                            }
                        }
                        if (allTestsPassed)
                        {
                            returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
                            list.Add(returnElementInfo);
                        }
                    }
                }
            }
            return list;
        }

        ElementInfo IXPath.GetPreviousSibling(ElementInfo EI)
        {
            SwitchFrameFromCurrent(EI);
            IWebElement childElement = Driver.FindElement(By.XPath(EI.XPath));
            IWebElement parentElement = childElement.FindElement(By.XPath(".."));
            ReadOnlyCollection<IWebElement> childrenElements = parentElement.FindElements(By.XPath("*"));
            if (childrenElements[0].Equals(childElement))
            {
                Driver.SwitchTo().DefaultContent();
                CurrentFrame = string.Empty;
                return null;
            }
            for (int i = 1; i < childrenElements.Count; i++)
            {
                if (childrenElements[i].Equals(childElement))
                {
                    Driver.SwitchTo().DefaultContent();
                    ElementInfo returnElementInfo = GetElementInfoWithIWebElementWithXpath(childrenElements[i - 1], CurrentFrame);
                    CurrentFrame = string.Empty;
                    return returnElementInfo;
                }
            }
            Driver.SwitchTo().DefaultContent();
            CurrentFrame = string.Empty;
            return null;
        }

        ElementInfo IXPath.GetNextSibling(ElementInfo EI)
        {
            SwitchFrameFromCurrent(EI);
            IWebElement childElement = Driver.FindElement(By.XPath(EI.XPath));
            IWebElement parentElement = childElement.FindElement(By.XPath(".."));
            ReadOnlyCollection<IWebElement> childrenElements = parentElement.FindElements(By.XPath("*"));
            if (childrenElements[^1].Equals(childElement))
            {
                Driver.SwitchTo().DefaultContent();
                CurrentFrame = string.Empty;
                return null;
            }
            for (int i = 1; i < childrenElements.Count; i++)
            {
                if (childrenElements[0].Equals(childElement))
                {
                    Driver.SwitchTo().DefaultContent();
                    ElementInfo returnElementInfo = GetElementInfoWithIWebElementWithXpath(childrenElements[i + 1], CurrentFrame);
                    CurrentFrame = string.Empty;
                    return returnElementInfo;
                }
            }
            Driver.SwitchTo().DefaultContent();
            CurrentFrame = string.Empty;
            return null;
        }


        POMEventHandler mActionRecorded;


        public void ActionRecordedCallback(POMEventHandler ActionRecorded)
        {
            mActionRecorded += ActionRecorded;
        }

        bool IWindowExplorer.IsElementObjectValid(object obj)
        {
            return true;
        }

        bool IWindowExplorer.TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null)
        {
            try
            {
                mIsDriverBusy = true;
                SwitchFrame(EI);
                foreach (ElementLocator el in EI.Locators)
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Pending;
                }

                List<ElementLocator> activesElementLocators = EI.Locators.Where(x => x.Active == true).ToList();
                List<ElementLocator> FriendlyLocator = EI.FriendlyLocators.Where(x => x.Active == true).ToList();
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);

                foreach (ElementLocator el in activesElementLocators)
                {
                    IWebElement webElement = null;
                    List<FriendlyLocatorElement> friendlyLocatorElementlist = [];
                    if (el.EnableFriendlyLocator)
                    {
                        IWebElement targetElement = null;

                        foreach (ElementLocator FLocator in FriendlyLocator)
                        {
                            if (!FLocator.IsAutoLearned)
                            {
                                ElementLocator evaluatedLocator = FLocator.CreateInstance() as ElementLocator;
                                ValueExpression VE = new ValueExpression(GetCurrentProjectEnvironment(), this.BusinessFlow);
                                FLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
                            }

                            if (FLocator.LocateBy == eLocateBy.POMElement && mPOM != null)
                            {
                                ElementInfo ReferancePOMElementInfo = mPOM.MappedUIElements.FirstOrDefault(x => x.Guid.ToString() == FLocator.LocateValue);

                                targetElement = LocateElementByLocators(ReferancePOMElementInfo, mPOM.MappedUIElements, true);
                            }
                            else
                            {
                                targetElement = LocateElementByLocator(FLocator, Driver);
                            }
                            if (targetElement != null)
                            {
                                FriendlyLocatorElement friendlyLocatorElement = new FriendlyLocatorElement
                                {
                                    position = FLocator.Position,
                                    FriendlyElement = targetElement
                                };
                                friendlyLocatorElementlist.Add(friendlyLocatorElement);
                            }
                        }

                    }
                    if (!el.IsAutoLearned)
                    {
                        webElement = LocateElementIfNotAutoLeared(el, Driver, friendlyLocatorElementlist);
                    }
                    else
                    {
                        // for POM element
                        if (mPOM != null)
                        {
                            webElement = LocateElementByLocators(EI, mPOM.MappedUIElements);
                        }
                        // For Live spy
                        else
                        {
                            webElement = LocateElementByLocator(el, Driver, friendlyLocatorElementlist, true);
                        }
                    }
                    if (webElement != null)
                    {
                        el.StatusError = string.Empty;
                        el.LocateStatus = ElementLocator.eLocateStatus.Passed;
                        if (GetOutAfterFoundElement)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        el.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    }
                }

                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));

                if (activesElementLocators.Any(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                EI.Locators.ForEach((locator) => locator.LocateStatus = ElementLocator.eLocateStatus.Failed);
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message, ex);
                return false;
            }
            finally
            {
                foreach (ElementLocator el in EI.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Pending).ToList())
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                }
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
                mIsDriverBusy = false;
            }
        }

        private IWebElement LocateElementIfNotAutoLeared(ElementLocator el, ISearchContext parentContext, List<FriendlyLocatorElement> friendlyLocatorElements = null)
        {
            ElementLocator evaluatedLocator = el.CreateInstance() as ElementLocator;
            ValueExpression VE = new ValueExpression(GetCurrentProjectEnvironment(), this.BusinessFlow);
            evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
            return LocateElementByLocator(evaluatedLocator, parentContext, friendlyLocatorElements, true);
        }
        private bool IsUserProfileFolderPathValid()
        {
            return !string.IsNullOrEmpty(UserProfileFolderPath) && System.IO.Directory.Exists(UserProfileFolderPath);
        }

        void IWindowExplorer.CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> mOriginalList)
        {
            try
            {
                mIsDriverBusy = true;
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);

                foreach (ElementInfo EI in mOriginalList)
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Pending;
                }

                foreach (ElementInfo EI in mOriginalList)
                {
                    try
                    {
                        SwitchFrame(EI);
                        IWebElement e = LocateElementByLocators(EI, mOriginalList);
                        if (e != null)
                        {
                            EI.ElementObject = e;
                            EI.ElementStatus = ElementInfo.eElementStatus.Passed;
                        }
                        else
                        {
                            EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        }
                    }
                    catch (Exception ex)
                    {
                        EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                        Console.WriteLine("CollectOriginalElementsDataForDeltaCheck error: " + ex.Message);
                    }
                }
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds(ImplicitWait));
                Driver.SwitchTo().DefaultContent();
                mIsDriverBusy = false;
            }
        }

        public ElementInfo GetMatchingElement(ElementInfo element, ObservableList<ElementInfo> existingElemnts)
        {
            //try using online IWebElement Objects comparison
            ElementInfo OriginalElementInfo = existingElemnts.FirstOrDefault(x => (x.ElementObject != null) && (element.ElementObject != null) && (x.ElementObject.ToString() == element.ElementObject.ToString()));//comparing IWebElement ID's


            if (OriginalElementInfo == null)
            {
                //try by type and Xpath comparison
                OriginalElementInfo = existingElemnts.FirstOrDefault(x => (x.ElementTypeEnum == element.ElementTypeEnum)
                                                                    && (x.XPath == element.XPath)
                                                                    && (x.Path == element.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(element.Path)))
                                                                    && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                        || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                            && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                            )
                                                                        )
);
            }

            return OriginalElementInfo;
        }

        void IWindowExplorer.StartSpying()
        {
            if (Driver != null)
            {
                Driver.SwitchTo().DefaultContent();
                InjectSpyIfNotIngected();
            }
        }

        public string GetElementXpath(ElementInfo EI)
        {
            if (EI.Path.Split('/')[^1].Contains("frame") || EI.Path.Split('/')[^1].Contains("iframe"))
            {
                return GenerateXpathForIWebElement((IWebElement)EI.ElementObject, string.Empty);
            }
            return GenerateXpathForIWebElement((IWebElement)EI.ElementObject, EI.Path);
        }

        public string GetInnerHtml(ElementInfo elementInfo)
        {
            var htmlElement = (HTMLElementInfo)elementInfo;

            return htmlElement.HTMLElementObject.InnerHtml;
        }

        public object GetElementParentNode(ElementInfo elementInfo)
        {
            return ((HTMLElementInfo)elementInfo).HTMLElementObject.ParentNode;
        }

        public string GetInnerText(ElementInfo elementInfo)
        {
            return ((HTMLElementInfo)elementInfo).HTMLElementObject.InnerText;
        }

        public string GetPreviousSiblingInnerText(ElementInfo elementInfo)
        {
            var htmlNode = ((HTMLElementInfo)elementInfo).HTMLElementObject;
            var prevSib = htmlNode.PreviousSibling;

            var innerText = string.Empty;

            //looking for text till two level up
            if (htmlNode.Name == "input" && prevSib == null)
            {
                prevSib = htmlNode.ParentNode;

                if (string.IsNullOrEmpty(prevSib.InnerText))
                {
                    prevSib = prevSib.PreviousSibling;
                }
            }

            if (prevSib != null && !string.IsNullOrEmpty(prevSib.InnerText) && prevSib.ChildNodes.Count == 1)
            {
                innerText = prevSib.InnerText;
            }

            return innerText;
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }


        public bool CanStartAnotherINstance()
        {
            throw new NotImplementedException();
        }

        public bool CanStartAnotherInstance(out string errorMessage)
        {
            switch (mBrowserType)
            {
                //TODO: filter on internetexplorer
                default:
                    errorMessage = string.Empty;
                    return true;
            }
        }

        public List<AppWindow> GetWindowAllFrames()
        {
            throw new NotImplementedException();
        }

        public bool IsRecordingSupported()
        {
            return true;
        }

        public bool IsPOMSupported()
        {
            return true;
        }

        public bool IsLiveSpySupported()
        {
            return true;
        }

        public bool IsWinowSelectionRequired()
        {
            return true;
        }

        public List<eTabView> SupportedViews()
        {
            return [eTabView.Screenshot, eTabView.GridView, eTabView.PageSource, eTabView.TreeView];
        }

        public eTabView DefaultView()
        {
            return eTabView.TreeView;
        }

        public string SelectionWindowText()
        {
            return "Page:";
        }

        async Task<object> IWindowExplorer.GetPageSourceDocument(bool ReloadHtmlDoc)
        {
            if (ReloadHtmlDoc)
            {
                SSPageDoc = null;
            }

            if (SSPageDoc == null)
            {
                SSPageDoc = new HtmlDocument();
                await Task.Run(() =>
                {
                    try
                    {
                        SSPageDoc.LoadHtml(Driver.PageSource);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to Page Source document", ex);
                    }
                });
            }

            return SSPageDoc;
        }

        public string GetCurrentPageSourceString()
        {
            return Driver.PageSource;
        }

        public void SetCurrentPageLoadStrategy(DriverOptions options)
        {
            if (PageLoadStrategy != null)
            {
                if (PageLoadStrategy.Equals(nameof(OpenQA.Selenium.PageLoadStrategy.Normal), StringComparison.InvariantCultureIgnoreCase))
                {
                    options.PageLoadStrategy = OpenQA.Selenium.PageLoadStrategy.Normal;
                }
                else if (PageLoadStrategy.Equals(nameof(OpenQA.Selenium.PageLoadStrategy.Eager), StringComparison.InvariantCultureIgnoreCase))
                {
                    options.PageLoadStrategy = OpenQA.Selenium.PageLoadStrategy.Eager;
                }
                else if (PageLoadStrategy.Equals(nameof(OpenQA.Selenium.PageLoadStrategy.None), StringComparison.InvariantCultureIgnoreCase))
                {
                    options.PageLoadStrategy = OpenQA.Selenium.PageLoadStrategy.None;
                }
                else
                {
                    options.PageLoadStrategy = OpenQA.Selenium.PageLoadStrategy.Default;
                }
            }
        }

        private void SetBrowserVersion(DriverOptions options)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(BrowserVersion))
                {
                    options.BrowserVersion = BrowserVersion;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error while setting browser version to driver options", ex);
            }
        }

        public void SetUnhandledPromptBehavior(DriverOptions options)
        {
            if (UnhandledPromptBehavior == null)
            {
                return;
            }

            if (Enum.TryParse(UnhandledPromptBehavior, true, out UnhandledPromptBehavior unhandledPromptBehavior))
            {
                options.UnhandledPromptBehavior = unhandledPromptBehavior switch
                {
                    OpenQA.Selenium.UnhandledPromptBehavior.Ignore => OpenQA.Selenium.UnhandledPromptBehavior.Ignore,
                    OpenQA.Selenium.UnhandledPromptBehavior.Accept => OpenQA.Selenium.UnhandledPromptBehavior.Accept,
                    OpenQA.Selenium.UnhandledPromptBehavior.Dismiss => OpenQA.Selenium.UnhandledPromptBehavior.Dismiss,
                    OpenQA.Selenium.UnhandledPromptBehavior.AcceptAndNotify => OpenQA.Selenium.UnhandledPromptBehavior.AcceptAndNotify,
                    OpenQA.Selenium.UnhandledPromptBehavior.DismissAndNotify => OpenQA.Selenium.UnhandledPromptBehavior.DismissAndNotify,
                    _ => OpenQA.Selenium.UnhandledPromptBehavior.Default,
                };
            }
        }

        private void SetBrowserLogLevel(DriverOptions options)
        {
            if (string.IsNullOrEmpty(BrowserLogLevel))
            {
                return;
            }
            int numberLogLevel = (int)Enum.Parse<eBrowserLogLevel>(BrowserLogLevel);
            if (!numberLogLevel.Equals(3))
            {
                options.SetLoggingPreference(OpenQA.Selenium.LogType.Browser, (OpenQA.Selenium.LogLevel)numberLogLevel);
            }
        }


        public string GetApplitoolServerURL()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiUrl;
        }

        public string GetApplitoolKey()
        {
            return WorkSpace.Instance.Solution.ApplitoolsConfiguration.ApiKey;
        }

        public ePlatformType GetPlatform()
        {
            return this.Platform;
        }

        public string GetEnvironment()
        {
            return this.BusinessFlow.Environment;
        }

        public Size GetWindowSize()
        {
            return Driver.Manage().Window.Size;
        }

        public string GetAgentAppName()
        {
            return GetBrowserType().ToString();
        }

        public string GetViewport()
        {
            return Driver.Manage().Window.Size.ToString();
        }

        private void SetUPDevTools(IWebDriver webDriver)
        {
            //Get DevTools
            devTools = webDriver as IDevTools;
            if (webDriver is ChromiumDriver)
            {
                try
                {
                    //DevTool Session
                    devToolsSession = devTools.GetDevToolsSession();
                    if (devToolsSession == null)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "DevTools session is not available; skipping CDP setup.");
                        mAct?.AddOrUpdateReturnParamActual("DevToolsInit", "SessionUnavailable");
                        return;
                    }
                    devToolsDomains = devToolsSession.GetVersionSpecificDomains<DevToolsDomains>();
                    if (devToolsDomains == null)
                    {
                        Reporter.ToLog(eLogLevel.WARN, "DevTools domains are not available for this CDP version.");
                        mAct?.AddOrUpdateReturnParamActual("DevToolsInit", "DomainsUnavailable");
                        return;
                    }
                    devToolsDomains.Network.Enable(new DevToolsVersion.Network.EnableCommandSettings()).GetAwaiter().GetResult();
                    blockOrUnblockUrls();
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                    mAct.Error = ex.Message;
                }
            }

        }

        /// <summary>
        /// Validates the compatibility of the browser for the current operation.
        /// </summary>
        /// <param name="webDriver">The WebDriver instance representing the browser.</param>
        /// <returns>
        /// Returns true if the browser is compatible with the current operation; otherwise, false.
        /// </returns>
        private bool ValidateBrowserCompatibility(IWebDriver webDriver)
        {

            // Check if browser type is not Chrome or Edge
            if (webDriver is not ChromiumDriver)
            {
                mAct.ExInfo = $"Action is Skipped, Selected browser operation: {mAct.ControlAction} is not supported for browser type: {mBrowserType}";
                mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                return false;
            }

            // Check if browser type is Edge and launched in IE mode
            if (mBrowserType == GingerCore.Drivers.SeleniumDriver.eBrowserType.Edge && OpenIEModeInEdge)
            {
                mAct.ExInfo = "Action is Skipped, Edge browser is launched in IE mode which is not supported for Network log operations.";
                mAct.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                return false;
            }

            return true;
        }

        private string[] getBlockedUrlsArray(string sUrlsToBeBlocked)
        {
            string[] arrBlockedUrls = [];
            if (!string.IsNullOrEmpty(sUrlsToBeBlocked))
            {
                arrBlockedUrls = sUrlsToBeBlocked.Trim(',').Split(",");
            }
            return arrBlockedUrls;
        }
        private void blockOrUnblockUrls()
        {
            if (mAct != null)
            {
                if (devToolsDomains == null)
                {
                    Reporter.ToLog(eLogLevel.WARN, "DevTools domains not initialized; cannot (un)block URLs.");
                    return;
                }
                if (mAct.ControlAction == ActBrowserElement.eControlAction.SetBlockedUrls)
                {
                    devToolsDomains.Network.SetBlockedURLs(new DevToolsVersion.Network.SetBlockedURLsCommandSettings() { Urls = getBlockedUrlsArray(mAct.GetInputParamCalculatedValue("sBlockedUrls")) }).GetAwaiter().GetResult();
                }
                else if (mAct.ControlAction == ActBrowserElement.eControlAction.UnblockeUrls)
                {
                    devToolsDomains.Network.SetBlockedURLs(new DevToolsVersion.Network.SetBlockedURLsCommandSettings() { Urls = [] }).GetAwaiter().GetResult();
                }
            }
        }



        public async Task StartMonitoringNetworkLog(IWebDriver webDriver)
        {
            try
            {
                if (isNetworkLogMonitoringStarted)
                {
                    mAct.ExInfo = "Network monitoring is already started";
                    return;
                }
                networkRequestLogList = [];
                networkResponseLogList = [];
                interceptor = webDriver.Manage().Network;
                ValueExpression VE = new ValueExpression(GetCurrentProjectEnvironment(), BusinessFlow);

                foreach (ActInputValue item in mAct.UpdateOperationInputValues)
                {
                    item.Value = item.Param;
                    item.ValueForDriver = VE.Calculate(item.Param);
                }

                interceptor.NetworkRequestSent += OnNetworkRequestSent;
                interceptor.NetworkResponseReceived += OnNetworkResponseReceived;

                await interceptor.StartMonitoring();
                isNetworkLogMonitoringStarted = true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        public async Task GetNetworkLogAsync(ActBrowserElement act)
        {
            try
            {
                if (isNetworkLogMonitoringStarted)
                {
                    _BrowserHelper.ProcessNetworkLogs(act, networkResponseLogList, networkRequestLogList);
                }
                else
                {
                    act.ExInfo = $"Action is skipped, {nameof(ActBrowserElement.eControlAction.StartMonitoringNetworkLog)} Action is not started";
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        public void ClearExistingNetworkLog()
        {
            networkResponseLogList = [];
            networkRequestLogList = [];
        }
        public async Task StopMonitoringNetworkLog(ActBrowserElement act)
        {
            try
            {
                act.Artifacts = [];

                if (isNetworkLogMonitoringStarted)
                {
                    await StopNetworkLog();

                    try
                    {
                        _BrowserHelper.ProcessNetworkLogs(act, networkResponseLogList, networkRequestLogList);
                    }
                    catch (Exception fileEx)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to create or attach network log files", fileEx);
                        act.ExInfo += "\nFailed to create or attach network log files.";
                        act.AddOrUpdateReturnParamActual("NetworkLogFileError", fileEx.Message);
                    }
                }
                else
                {
                    act.ExInfo = $"Action is skipped, {nameof(ActBrowserElement.eControlAction.StartMonitoringNetworkLog)} Action is not started";
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        private async Task StopNetworkLog()
        {
            try
            {
                await interceptor.StopMonitoring();
                interceptor.NetworkRequestSent -= OnNetworkRequestSent;
                interceptor.NetworkResponseReceived -= OnNetworkResponseReceived;
                interceptor.ClearRequestHandlers();
                interceptor.ClearResponseHandlers();
                if (devToolsDomains != null)
                {
                    await devToolsDomains.Network.Disable(new DevToolsVersion.Network.DisableCommandSettings());
                }
                if (devToolsSession != null)
                {
                    devToolsSession.Dispose();
                }
                devTools?.CloseDevToolsSession();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error - {ex.Message}", ex);
            }
        }

        private int CreateConsoleLogFile(string filePath, string logs, ActBrowserElement act)
        {
            string calculatedFilePath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(filePath);
            string fileDictionary = Path.GetDirectoryName(calculatedFilePath);

            if (!String.IsNullOrEmpty(calculatedFilePath))
            {
                try
                {
                    bool isRootedPath = Path.IsPathRooted(fileDictionary);
                    if (!isRootedPath)
                    {
                        calculatedFilePath = new Uri(calculatedFilePath).LocalPath;
                    }
                    using (FileStream fileStream = System.IO.File.Create(calculatedFilePath))
                    {
                        fileStream.Close();
                    }
                    System.IO.File.WriteAllText(calculatedFilePath, logs);
                }
                catch (Exception ex)
                {
                    act.Error = ex.Message;
                    return -1;
                }
            }
            else return 0;

            return 1;
        }

        private void OnNetworkRequestSent(object sender, NetworkRequestSentEventArgs e)
        {
            try
            {
                if (_BrowserHelper.ShouldMonitorAllUrls() || _BrowserHelper.ShouldMonitorUrl(e.RequestUrl))
                {
                    networkRequestLogList.Add(new Tuple<string, object>($"RequestUrl: {e.RequestUrl}", JsonConvert.SerializeObject(e, Formatting.Indented)));
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }


        private void OnNetworkResponseReceived(object sender, NetworkResponseReceivedEventArgs e)
        {
            try
            {
                string monitorType = mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eMonitorUrl)).Value;

                if (_BrowserHelper.ShouldMonitorAllUrls() || _BrowserHelper.ShouldMonitorUrl(e.ResponseUrl))
                {
                    if (mAct.GetOrCreateInputParam(nameof(ActBrowserElement.eRequestTypes)).Value == nameof(ActBrowserElement.eRequestTypes.FetchOrXHR))
                    {
                        if (e.ResponseResourceType.Equals("XHR", StringComparison.CurrentCultureIgnoreCase) || e.ResponseResourceType.Equals("FETCH", StringComparison.CurrentCultureIgnoreCase))
                        {
                            networkResponseLogList.Add(new Tuple<string, object>($"ResponseUrl:{e.ResponseUrl}", JsonConvert.SerializeObject(e, Formatting.Indented)));
                        }
                    }
                    else
                    {
                        networkResponseLogList.Add(new Tuple<string, object>($"ResponseUrl:{e.ResponseUrl}", JsonConvert.SerializeObject(e, Formatting.Indented)));
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        private protected override IBrowser GetBrowser()
        {
            //overridden method from GingerWebDriver, need to implement this when we refactor SeleniumDriver to be in the similar structure as PlaywrightDriver
            throw new NotImplementedException();
        }

        private protected override Task<IBrowserElement> FindBrowserElementAsync(eLocateBy locateBy, string locateValue)
        {
            //overridden method from GingerWebDriver, need to implement this when we refactor SeleniumDriver to be in the similar structure as PlaywrightDriver
            throw new NotImplementedException();
        }

        // for Security Testing
        // Helper: accept "localhost:8080", "http://localhost:8080", "https://zap:8443", or raw host
        private static string CoerceZapHostPort(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            if (Uri.TryCreate(input, UriKind.Absolute, out var uri))
            {
                var port = uri.IsDefaultPort ? 8080 : uri.Port;
                return $"{uri.Host}:{port}";
            }
            // If it already looks like host:port or host, return as-is
            var trimmed = input.Trim();
            trimmed = trimmed.Replace("http://", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("https://", "", StringComparison.OrdinalIgnoreCase)
                             .Trim('/');
            return trimmed;
        }
    }
}
