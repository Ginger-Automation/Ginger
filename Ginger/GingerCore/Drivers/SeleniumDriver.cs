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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.Selenium.SeleniumBMP;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Protractor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WindowsInput;

namespace GingerCore.Drivers
{
    public class SeleniumDriver : DriverBase, IWindowExplorer, IVisualTestingDriver, IXPath, IPOM, IRecord
    {
        public enum eBrowserType
        {
            IE,
            FireFox,
            Chrome,
            Edge,
            RemoteWebDriver,
        }
        [UserConfigured]
        [UserConfiguredDescription("Proxy Server:Port")]
        public string Proxy { get; set; }

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
        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Disable Chrome Extension. This feature is not available anymore")]
        public bool DisableExtension { get; set; }
        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Ignore Internet Explorer protected mode")]
        public bool IgnoreIEProtectedMode { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use 64Bit Browser")]
        public bool Use64Bitbrowser { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Use Browser In Private/Incognito Mode (Please use 64bit Browse with Internet Explorer ")]
        public bool BrowserPrivateMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Only for Chrome & Firefox | Set \"true\" to run the browser in background (headless mode) for faster Execution")]
        public bool HeadlessBrowserMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set \"true\" to Launch the Browser minimized")]
        public bool BrowserMinimized { get; set; }

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
        [UserConfiguredDescription("Only for Chrome & Firefox | Full path for the User Profile folder")]
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
        [UserConfiguredDescription("Selenium line arguments || Set Selenium arguments separated with ; sign")]
        public string SeleniumUserArguments { get; set; }


        [UserConfigured]
        [UserConfiguredDefault("False")]
        [UserConfiguredDescription("Applitool - Set to true if you want to use Applitools for visual testing")]
        public Boolean UseApplitools { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Applitool View Key number")]
        public String ApplitoolsViewKey { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Change to Iframe automatically in case of POM Element execution ")]
        public bool HandelIFramShiftAutomaticallyForPomElement { get; set; }

        protected IWebDriver Driver;

        protected eBrowserType mBrowserTpe;
        protected NgWebDriver ngDriver;
        private String DefaultWindowHandler = null;

        private Proxy mProxy = new Proxy();

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

        private bool IsRecording = false;

        IWebElement LastHighLightedElement;
        XPathHelper mXPathHelper;

        List<ElementInfo> allReadElem = new List<ElementInfo>();

        private string CurrentFrame;

        public SeleniumDriver()
        {

        }

        ~SeleniumDriver()
        {
            if (Driver != null)
            {
                CloseDriver();
            }
        }

        public SeleniumDriver(eBrowserType BrowserType)
        {
            mBrowserTpe = BrowserType;
        }

        public SeleniumDriver(object driver)
        {
            this.Driver = (IWebDriver)driver;
        }

        public override void StartDriver()
        {
            if (StartBMP)
            {
                BMPServer = new Server(StartBMPBATFile, StartBMPPort);
                BMPServer.Start();
                BMPClient = BMPServer.CreateProxy();
            }

            if (!string.IsNullOrEmpty(ProxyAutoConfigUrl))
            {
                mProxy = new Proxy();
                mProxy.ProxyAutoConfigUrl = ProxyAutoConfigUrl;
            }
            else if (!string.IsNullOrEmpty(Proxy))
            {
                mProxy = new Proxy();
                mProxy.Kind = ProxyKind.Manual;
                mProxy.HttpProxy = Proxy;
                mProxy.FtpProxy = Proxy;
                mProxy.SslProxy = Proxy;
                mProxy.SocksProxy = Proxy;
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
                ImplicitWait = 30;

            String[] SeleniumUserArgs = null;
            if (!string.IsNullOrEmpty(SeleniumUserArguments))
                SeleniumUserArgs = SeleniumUserArguments.Split(';');

            //TODO: launch the driver/agent per combo selection
            try
            {
                switch (mBrowserTpe)
                {
                    //TODO: refactor closing the extra tabs

                    #region Internet Explorer
                    case eBrowserType.IE:
                        InternetExplorerOptions ieoptions = new InternetExplorerOptions();

                        ieoptions.EnsureCleanSession = true;
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
                            ieoptions.BrowserCommandLineArguments += "," + SeleniumUserArguments;

                        InternetExplorerDriverService IEService = null;

                        if (Use64Bitbrowser)
                        {
                            string IEdriver64bitpath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Locati‌​on) + @"\StaticDrivers\IE64BitDriver");
                            IEService = InternetExplorerDriverService.CreateDefaultService(IEdriver64bitpath);
                        }
                        else
                        {
                            IEService = InternetExplorerDriverService.CreateDefaultService();
                        }

                        IEService.HideCommandPromptWindow = HideConsoleWindow;
                        Driver = new InternetExplorerDriver(IEService, ieoptions, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));

                        break;
                    #endregion

                    #region Mozilla Firefox
                    case eBrowserType.FireFox:
                        string geckoDriverExePath2 = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\geckodriver.exe";
                        System.Environment.SetEnvironmentVariable("webdriver.gecko.driver", geckoDriverExePath2, EnvironmentVariableTarget.Process);

                        FirefoxOptions FirefoxOption = new FirefoxOptions();
                        if (HeadlessBrowserMode == true)
                            FirefoxOption.AddArgument("--headless");

                        if (!string.IsNullOrEmpty(UserProfileFolderPath) && System.IO.Directory.Exists(UserProfileFolderPath))
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


                        FirefoxDriverService FFService = FirefoxDriverService.CreateDefaultService();
                        FFService.HideCommandPromptWindow = HideConsoleWindow;
                        Driver = new FirefoxDriver(FFService, FirefoxOption, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                        break;

                    #endregion

                    #region Chrome
                    case eBrowserType.Chrome:
                        ChromeOptions options = new ChromeOptions();
                        options.AddArgument("--start-maximized");

                        if (!string.IsNullOrEmpty(UserProfileFolderPath) && System.IO.Directory.Exists(UserProfileFolderPath))
                            options.AddArguments("user-data-dir=" + UserProfileFolderPath);
                        else if (!string.IsNullOrEmpty(ExtensionPath))
                            options.AddExtension(Path.GetFullPath(ExtensionPath));

                        //setting proxy
                        SetProxy(options);

                        //DownloadFolderPath
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

                        if (HeadlessBrowserMode == true)
                            options.AddArgument("--headless");

                        if (SeleniumUserArgs != null)
                            foreach (string arg in SeleniumUserArgs)
                                options.AddArgument(arg);

                        if (!string.IsNullOrEmpty(EmulationDeviceName))
                        {
                            options.EnableMobileEmulation(EmulationDeviceName);
                        }
                        else if(!string.IsNullOrEmpty(BrowserUserAgent))
                        {
                            ChromeMobileEmulationDeviceSettings chromeMobileEmulationDevice = new ChromeMobileEmulationDeviceSettings()
                            { UserAgent = BrowserUserAgent.Trim() };
                            options.EnableMobileEmulation(chromeMobileEmulationDevice);
                        }

                        ChromeDriverService ChService = ChromeDriverService.CreateDefaultService();
                        if (HideConsoleWindow)
                        {
                            ChService.HideCommandPromptWindow = HideConsoleWindow;
                        }
                        Driver = new ChromeDriver(ChService, options, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                        break;

                    #endregion

                    #region EDGE
                    case eBrowserType.Edge:

                        EdgeDriverService EDService = EdgeDriverService.CreateDefaultService();
                        EDService.HideCommandPromptWindow = HideConsoleWindow;
                        EdgeOptions EDOpts = new EdgeOptions();
                        EDOpts.UnhandledPromptBehavior = UnhandledPromptBehavior.Default;
                        Driver = new EdgeDriver(EDService, EDOpts, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));

                        break;
                    #endregion



                    #region Safari - To be Added
                    //TODO: add Safari
                    #endregion

                    #region Remote Browser/Web Driver
                    case eBrowserType.RemoteWebDriver:
                        if (RemoteBrowserName.Equals("internet explorer"))
                        {
                            ieoptions = new InternetExplorerOptions();
                            ieoptions.EnsureCleanSession = true;
                            ieoptions.IgnoreZoomLevel = true;
                            ieoptions.Proxy = mProxy == null ? null : mProxy;
                            ieoptions.EnableNativeEvents = true;
                            ieoptions.IntroduceInstabilityByIgnoringProtectedModeSettings = true;
                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                            {
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), ieoptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
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
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), fxOptions.ToCapabilities(), TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            else
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), fxOptions.ToCapabilities());
                            // TODO: make Sauce lab driver/config

                            //TODO: For sauce lab - externalize - try without amdocs proxy hot spot works then it is proxy issue
                            break;
                        }
                        else
                        {
                            DesiredCapabilities capability = new DesiredCapabilities();
                            capability.SetCapability(CapabilityType.BrowserName, RemoteBrowserName);
                            if (!string.IsNullOrEmpty(RemotePlatform))
                            {
                                capability.SetCapability(SeleniumDriver.RemotePlatformParam, RemotePlatform);
                            }
                            if (!string.IsNullOrEmpty(RemoteVersion))
                            {
                                capability.SetCapability(SeleniumDriver.RemoteVersionParam, RemoteVersion);
                            }

                            if (Convert.ToInt32(HttpServerTimeOut) > 60)
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), capability, TimeSpan.FromSeconds(Convert.ToInt32(HttpServerTimeOut)));
                            else
                                Driver = new RemoteWebDriver(new Uri(RemoteGridHub + "/wd/hub"), capability);

                            break;
                        }
                        #endregion
                }

                if (BrowserMinimized == true && mBrowserTpe != eBrowserType.Edge)
                    Driver.Manage().Window.Minimize();

                if (!string.IsNullOrEmpty(BrowserHeight) && !string.IsNullOrEmpty(BrowserWidth))
                {
                    Driver.Manage().Window.Size = new Size() { Height = Convert.ToInt32(BrowserHeight),Width=Convert.ToInt32(BrowserWidth) };
                }

                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));

                //set pageLoad timeout limit
                if ((int)PageLoadTimeOut == 0)
                    PageLoadTimeOut = 60;

                Driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds((int)PageLoadTimeOut);


                DefaultWindowHandler = Driver.CurrentWindowHandle;
                InitXpathHelper();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception in start driver", ex);
                ErrorMessageFromDriver = ex.Message;
            }
        }

        private void SetProxy(dynamic options)
        {
            if (mProxy == null) return;
            options.Proxy = new Proxy();
            switch (mProxy.Kind)
            {
                case ProxyKind.Manual:
                    options.Proxy.Kind = ProxyKind.Manual;
                    options.Proxy.HttpProxy = mProxy.HttpProxy;
                    options.Proxy.SslProxy = mProxy.SslProxy;

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
                if (Driver != null)
                {
                    Driver.Quit();
                    Driver = null;
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured while getting current element", ex);
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
            if (sURL.ToLower().StartsWith("www"))
            {
                sURL = "http://" + sURL;
            }

            Uri uri = ValidateURL(sURL);
            if (uri != null)
            {
                Driver.Navigate().GoToUrl(uri.AbsoluteUri);
            }
            else
            {
                act.Error = "Error: Invalid URL. Give valid URL(Complete URL)";
            }
            string winTitle = Driver.Title;
            if (Driver.GetType() == typeof(InternetExplorerDriver) && winTitle.IndexOf("Certificate Error", StringComparison.CurrentCultureIgnoreCase) >= 0)
            {
                Thread.Sleep(100);
                try
                {
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
                    Driver.Navigate().GoToUrl("javascript:document.getElementById('overridelink').click()");
                }
                catch { }
                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds((int)ImplicitWait);
            }

            //just to be sure the page is fully loaded
            CheckifPageLoaded();
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
            bool runActHandlerDirect = act is ActHandleBrowserAlert || (isActBrowser && (actBrowserObj.ControlAction == ActBrowserElement.eControlAction.SwitchToDefaultWindow
                                    || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.AcceptMessageBox
                                        || actBrowserObj.ControlAction == ActBrowserElement.eControlAction.DismissMessageBox));

            if (!runActHandlerDirect)
            {
                //implicityWait must be done on actual window so need to make sure the driver is pointing on window
                try
                {
                    string aa = Driver.Title;//just to make sure window attributes do not throw exception
                }
                catch (Exception ex)
                {
                    if (Driver.WindowHandles.Count == 1)
                    {
                        Driver.SwitchTo().Window(Driver.WindowHandles[0]);
                    }
                    Reporter.ToLog(eLogLevel.ERROR, "Selenium Driver is not accessible, probably because there is Alert window open", ex);
                }

                if (act.Timeout != null && act.Timeout != 0)
                {
                    //if we have time out on action then set it on the driver
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds((int)act.Timeout);
                }
                else
                {
                    // use the driver config timeout
                    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds((int)ImplicitWait);
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
                    act.ExInfo += "Action HAR file saved at: " + filename;
                }
            }

            DoRunAction(act);
        }

        private void DoRunAction(Act act)
        {
            Type ActType = act.GetType();
            //find Act handler, code is more readable than if/else...

            if (ActType == typeof(ActUIElement))
            {
                HandleActUIElement((ActUIElement)act);
                return;
            }

            if (ActType == typeof(ActGotoURL))
            {
                GotoURL((ActGotoURL)act, act.GetInputParamCalculatedValue("Value"));
                return;
            }
            if (ActType == typeof(ActGenElement))
            {
                GenElementHandler((ActGenElement)act);
                return;
            }
            if (ActType == typeof(ActSmartSync))
            {
                SmartSyncHandler((ActSmartSync)act);
                return;
            }
            if (ActType == typeof(ActTextBox))
            {
                ActTextBoxHandler((ActTextBox)act);
                return;
            }
            if (ActType == typeof(ActPWL))
            {
                PWLElementHandler((ActPWL)act);
                return;
            }
            if (ActType == typeof(ActHandleBrowserAlert))
            {
                HandleBrowserAlert((ActHandleBrowserAlert)act);
                return;
            }
            if (ActType == typeof(ActVisualTesting))
            {
                HandleActVisualTesting((ActVisualTesting)act);
                return;
            }

            //TODO: please create small function for each Act

            if (ActType == typeof(ActPassword))
            {
                ActPasswordHandler((ActPassword)act);
                return;
            }

            if (ActType == typeof(ActLink))
            {
                ActLinkHandler((ActLink)act);
                return;
            }

            if (ActType == typeof(ActButton))
            {
                ActButtonHandler((ActButton)act);
                return;
            }

            if (ActType == typeof(ActCheckbox))
            {
                ActCheckboxHandler((ActCheckbox)act);
                return;
            }

            if (ActType == typeof(ActDropDownList))
            {
                ActDropDownListHandler((ActDropDownList)act);
                return;
            }

            if (ActType == typeof(ActRadioButton))
            {
                ActRadioButtonHandler((ActRadioButton)act);
                return;
            }

            if (ActType == typeof(ActMultiselectList))
            {
                ActMultiselectList el = (ActMultiselectList)act;
                string csv = act.GetInputParamValue("Value"); string[] parts = csv.Split('|'); //TODO: make sure the values passed are separated by '|'
                List<string> optionList = new List<string>(parts);
                switch (el.ActMultiselectListAction)
                {
                    case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByIndex:
                        SelectMultiselectListOptionsByIndex(el, optionList.ConvertAll(s => Int32.Parse(s))); // list<string> has to get converted to list<int>
                        break;
                    case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByText:
                        SelectMultiselectListOptionsByText(el, optionList);
                        break;
                    case ActMultiselectList.eActMultiselectListAction.SetSelectedValueByValue:
                        SelectMultiselectListOptionsByValue(el, optionList);
                        break;
                    case ActMultiselectList.eActMultiselectListAction.ClearAllSelectedValues:
                        DeSelectMultiselectListOptions(el);

                        //TODO: implement ClearAllSelectedValues for ActMultiselectList
                        break;
                }

                return;
            }


            if (ActType == typeof(ActHello))
            {
                //TODO: return hellow from...
                return;
            }

            if (ActType == typeof(ActScreenShot))
            {
                ScreenshotHandler((ActScreenShot)act);
                return;
            }

            if (ActType == typeof(ActSubmit))
            {
                ActsubmitHandler((ActSubmit)act);
                return;
            }

            if (ActType == typeof(ActLabel))
            {
                ActLabelHandler((ActLabel)act);
                return;
            }

            if (ActType == typeof(ActWebSitePerformanceTiming))
            {
                ActWebSitePerformanceTiming ABPT = (ActWebSitePerformanceTiming)act;
                ActWebSitePerformanceTimingHandler(ABPT);
                return;
            }

            if (ActType == typeof(ActSwitchWindow))
            {
                ActSwitchWindowHandler((ActSwitchWindow)act);
                return;
            }

            if (ActType == typeof(ActBrowserElement))
            {
                ActBrowserElementHandler((ActBrowserElement)act);
                return;
            }

            if (ActType == typeof(ActAgentManipulation))
            {
                ActAgentManipulationHandler((ActAgentManipulation)act);
                return;
            }

            act.Error = "Run Action Failed due to unrecognized action type - " + ActType.ToString();
            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
        }

        private void ScreenshotHandler(ActScreenShot act)
        {
            try
            {
                if (act.WindowsToCapture == Act.eWindowsToCapture.OnlyActiveWindow || TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure)
                {
                    AddCurrentScreenShot(act);
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
                    actLabel.AddOrUpdateReturnParamActual("Actual", "True");
                else
                    actLabel.AddOrUpdateReturnParamActual("Actual", e.Text);
            }
            else
            {
                if (actLabel.LabelAction == ActLabel.eLabelAction.IsVisible)
                    actLabel.AddOrUpdateReturnParamActual("Actual", "False");
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
                            e.SendKeys(actTextBox.GetInputParamCalculatedValue("Value"));
                        else
                            //TODO: How do we check for errors? do negative UT check for below
                            // + Why FF is different? what happened?
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, actTextBox.GetInputParamCalculatedValue("Value"));
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
                        actTextBox.AddOrUpdateReturnParamActual("Actual", e.Text);
                    else
                        actTextBox.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("value"));

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

        public void SmartSyncHandler(ActSmartSync act)
        {
            IWebElement e = LocateElement(act, true);
            Stopwatch st = new Stopwatch();
            int MaxTimeout = 0;
            try
            {
                if (act.WaitTime.HasValue == true)
                {
                    MaxTimeout = act.WaitTime.GetValueOrDefault();
                }
                else if (string.IsNullOrEmpty(act.GetInputParamValue("Value")))
                {
                    MaxTimeout = 5;
                }
                else
                {
                    MaxTimeout = Convert.ToInt32(act.GetInputParamCalculatedValue("Value"));
                }
            }
            catch (Exception)
            {
                MaxTimeout = 5;
            }
            switch (act.SmartSyncAction)
            {
                case ActSmartSync.eSmartSyncAction.WaitUntilDisplay:


                    st.Reset();
                    st.Start();

                    while (!(e != null && (e.Displayed || e.Enabled)))
                    {
                        Thread.Sleep(100);
                        e = LocateElement(act, true);
                        if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                        {
                            act.Error = "Smart Sync of WaitUntilDisplay is timeout";
                            break;
                        }
                    }


                    break;
                case ActSmartSync.eSmartSyncAction.WaitUntilDisapear:
                    st.Reset();

                    if (e == null)
                    {
                        return;
                    }
                    else
                    {
                        st.Start();

                        while (e != null && e.Displayed)
                        {
                            Thread.Sleep(100);
                            e = LocateElement(act, true);
                            if (st.ElapsedMilliseconds > MaxTimeout * 1000)
                            {
                                act.Error = "Smart Sync of WaitUntilDisapear is timeout";
                                break;
                            }
                        }

                    }
                    break;
            }
            return;
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
                        act.AddOrUpdateReturnParamActual("Actual", title);
                    else
                        act.AddOrUpdateReturnParamActual("Actual", "");
                    break;
                case ActGenElement.eGenElementAction.MouseClick:
                    e = LocateElement(act);
                    InputSimulator inp = new InputSimulator();
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
                                act.AddOrUpdateReturnParamActual("Actual", e.Text);
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
                    if (e == null) return;

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
                            act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("innerText"));
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
                            Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing SelectFromDropDown operation", ex);
                            try
                            {
                                se.SelectByValue(act.GetInputParamCalculatedValue("Value"));
                            }
                            catch (Exception ex2)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing SelectFromDropDown operation", ex2);
                                try
                                {
                                    se.SelectByIndex(Convert.ToInt32(act.GetInputParamCalculatedValue("Value")));
                                }
                                catch (Exception ex3)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing SelectFromDropDown operation", ex3);
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
                            Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing AsyncSelectFromDropDownByIndex operation", ex3);
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
                        Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing SelectFromDijitList operation", ex);
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
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occured while performing SetValue operation", ex);
                            }
                        }
                        else
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
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
                            act.AddOrUpdateReturnParamActual("Actual", a.ToString());
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
                        if (vals.Count() != 2)
                            throw new Exception(@"Inot string should be in the format : attribute=value");
                        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0]." + vals[0] + "=arguments[1]", e, vals[1]);
                    }
                    break;
                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());

            }
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
                if (e == null) return;
                SelectElement se = new SelectElement(e);
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured in ActDropDownListHandler", ae);
                return;
            }
        }

        private string GetKeyName(string skey)
        {
            switch (skey)
            {
                case "Keys.Alt":
                    return Keys.Alt;
                case "Keys.ArrowDown":
                    return Keys.ArrowDown;
                case "Keys.ArrowLeft":
                    return Keys.ArrowLeft;
                case "Keys.ArrowRight":
                    return Keys.ArrowRight;
                case "Keys.ArrowUp":
                    return Keys.ArrowUp;
                case "Keys.Backspace":
                    return Keys.Backspace;

                case "Keys.Cancel":
                    return Keys.Cancel;

                case "Keys.Clear":
                    return Keys.Clear;

                case "Keys.Command":
                    return Keys.Command;

                case "Keys.Control":
                    return Keys.Control;

                case "Keys.Decimal":
                    return Keys.Decimal;

                case "Keys.Delete":
                    return Keys.Delete;

                case "Keys.Divide":
                    return Keys.Divide;

                case "Keys.Down":
                    return Keys.Down;

                case "Keys.End":
                    return Keys.End;

                case "Keys.Enter":
                    return Keys.Enter;

                case "Keys.Equal":
                    return Keys.Equal;

                case "Keys.Escape":
                    return Keys.Escape;

                case "Keys.F1":
                    return Keys.F1;

                case "Keys.F10":
                    return Keys.F10;

                case "Keys.F11":
                    return Keys.F11;

                case "Keys.F12":
                    return Keys.F12;

                case "Keys.F2":
                    return Keys.F2;

                case "Keys.F3":
                    return Keys.F3;

                case "Keys.F4":
                    return Keys.F4;

                case "Keys.F5":
                    return Keys.F5;

                case "Keys.F6":
                    return Keys.F6;

                case "Keys.F7":
                    return Keys.F7;

                case "Keys.F8":
                    return Keys.F8;

                case "Keys.F9":
                    return Keys.F9;

                case "Keys.Help":
                    return Keys.Help;

                case "Keys.Home":
                    return Keys.Home;

                case "Keys.Insert":
                    return Keys.Insert;

                case "Keys.Left":
                    return Keys.Left;

                case "Keys.LeftAlt":
                    return Keys.LeftAlt;

                case "Keys.LeftControl":
                    return Keys.LeftControl;

                case "Keys.LeftShift":
                    return Keys.LeftShift;

                case "Keys.Meta":
                    return Keys.Meta;

                case "Keys.Multiply":
                    return Keys.Multiply;

                case "Keys.Null":
                    return Keys.Null;

                case "Keys.NumberPad0":
                    return Keys.NumberPad0;

                case "Keys.NumberPad1":
                    return Keys.NumberPad1;

                case "Keys.NumberPad2":
                    return Keys.NumberPad2;

                case "Keys.NumberPad3":
                    return Keys.NumberPad3;

                case "Keys.NumberPad4":
                    return Keys.NumberPad4;

                case "Keys.NumberPad5":
                    return Keys.NumberPad5;

                case "Keys.NumberPad6":
                    return Keys.NumberPad6;

                case "Keys.NumberPad7":
                    return Keys.NumberPad7;

                case "Keys.NumberPad8":
                    return Keys.NumberPad8;

                case "Keys.NumberPad9":
                    return Keys.NumberPad9;

                case "Keys.PageDown":
                    return Keys.PageDown;

                case "Keys.PageUp":
                    return Keys.PageUp;

                case "Keys.Pause":
                    return Keys.Pause;

                case "Keys.Return":
                    return Keys.Return;

                case "Keys.Right":
                    return Keys.Right;

                case "Keys.Semicolon":
                    return Keys.Semicolon;

                case "Keys.Separator":
                    return Keys.Separator;

                case "Keys.Shift":
                    return Keys.Shift;

                case "Keys.Space":
                    return Keys.Space;

                case "Keys.Subtract":
                    return Keys.Subtract;

                case "Keys.Tab":
                    return Keys.Tab;

                case "Keys.Up":
                    return Keys.Up;
                default:
                    return skey;

            }
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
                        aValues = elm.GetAttribute("value") + "|" + aValues;
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
                actButton.Error = "Error: Element not found - " + actButton.LocateBy + " " + actButton.LocateValue;
                return;
            }
            if (actButton.ButtonAction == ActButton.eButtonAction.GetValue)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("Value"));
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.IsDisabled)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("Disabled"));
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.GetFont)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("font"));
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.IsDisplayed)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
                return;
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.GetStyle)
            {
                try
                {
                    actButton.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("style"));
                }
                catch
                {
                    actButton.AddOrUpdateReturnParamActual("Actual", "no such attribute");
                }
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.GetHeight)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.Size.Height.ToString());
            }
            else if (actButton.ButtonAction == ActButton.eButtonAction.GetWidth)
            {
                actButton.AddOrUpdateReturnParamActual("Actual", e.Size.Width.ToString());
            }
            else
            {
                ClickButton(actButton);
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
                        actLink.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("href"));
                    else
                        actLink.AddOrUpdateReturnParamActual("Actual", "");
                }
                catch (Exception)
                { }

            }

            if (actLink.LinkAction == ActLink.eLinkAction.Visible)
            {
                try
                {
                    if (e != null)
                        actLink.AddOrUpdateReturnParamActual("Actual", e.Displayed + "");
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
                catch (OpenQA.Selenium.ElementNotVisibleException)
                {
                    /* not sure what causes this */
                }
            }
            else
                Button.Error = "Error: Element not found - " + Button.LocateBy + " " + Button.LocateValue;
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
            General.DoEvents();
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
                            if (reg.Matches(e.GetAttribute("id")).Count > 0)
                                return e;
                    }
                    break;
                case eLocateBy.ByName:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("name") != null)
                            if (reg.Matches(e.GetAttribute("name")).Count > 0)
                                return e;
                    }
                    break;
                case eLocateBy.ByLinkText:
                    foreach (IWebElement e in elem)
                    {
                        if (e.Text != null)
                            if (reg.Matches(e.Text).Count > 0)
                                return e;
                    }
                    break;
                case eLocateBy.ByValue:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("value") != null)
                            if (reg.Matches(e.GetAttribute("value")).Count > 0)
                                return e;
                    }
                    break;
                case eLocateBy.ByHref:
                    foreach (IWebElement e in elem)
                    {
                        if (e.GetAttribute("href") != null)
                            if (reg.Matches(e.GetAttribute("href")).Count > 0 && e.Text != "")
                                return e;
                    }
                    break;
            }
            return Driver.FindElements(By.XPath("//*[@value=\"" + LocValue + "\"]")).FirstOrDefault();
        }

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

            if (act is ActUIElement && (ValidationElementLocateBy == null || ValidationElementLocateValue == null))
            {
                ActUIElement aev = (ActUIElement)act;
                Enum.TryParse<eLocateBy>(aev.ElementLocateBy.ToString(), true, out locateBy);
                locateValue = aev.ElementLocateValueForDriver;
            }

            if (locateBy == eLocateBy.POMElement)
            {
                var pomExcutionUtil = new POMExecutionUtils(act);
                var currentPOM = pomExcutionUtil.GetCurrentPOM();

                if (currentPOM != null)
                {
                    ElementInfo currentPOMElementInfo = pomExcutionUtil.GetCurrentPOMElementInfo();
                    if (currentPOMElementInfo != null)
                    {
                        if (HandelIFramShiftAutomaticallyForPomElement)
                        {
                            SwitchFrame(currentPOMElementInfo);
                        }
                        elem = LocateElementByLocators(currentPOMElementInfo.Locators);
                        currentPOMElementInfo.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Failed).ToList().ForEach(y => act.ExInfo += System.Environment.NewLine + string.Format("Failed to locate the element with LocateBy='{0}' and LocateValue='{1}', Error Details:'{2}'", y.LocateBy, y.LocateValue, y.LocateStatus));
                    }
                    
                }
            }
            else
            {
                ElementLocator locator = new ElementLocator();
                locator.LocateBy = locateBy;
                locator.LocateValue = locateValue;
                elem = LocateElementByLocator(locator, AlwaysReturn);
                if (elem == null)
                {
                    act.ExInfo += System.Environment.NewLine + string.Format("Failed to locate the element with LocateBy='{0}' and LocateValue='{1}', Error Details:'{2}'", locator.LocateBy, locator.LocateValue, locator.LocateStatus);
                }
            }

            return elem;
        }

        private void SwitchFrame(ElementInfo EI)
        {
            UnhighlightLast();
            Driver.SwitchTo().DefaultContent();
            if (EI.Path != null)
            {
                string[] spliter = new string[] { "," };
                string[] iframesPathes = EI.Path.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                foreach (string iframePath in iframesPathes)
                {
                    Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(iframePath)));
                }
            }
        }

        public IWebElement LocateElementByLocators(ObservableList<ElementLocator> Locators)
        {
            IWebElement elem = null;
            foreach (ElementLocator locator in Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            foreach (ElementLocator locator in Locators.Where(x => x.Active == true).ToList())
            {
                if (!locator.IsAutoLearned)
                {
                    ElementLocator evaluatedLocator = locator.CreateInstance() as ElementLocator;
                    ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);
                    evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
                    elem = LocateElementByLocator(evaluatedLocator, true);
                }
                else
                    elem = LocateElementByLocator(locator, true);

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

            return null;
        }

        private IWebElement LocateElementByLocator(ElementLocator locator, bool AlwaysReturn = true)
        {
            IWebElement elem = null;
            locator.StatusError = "";
            locator.LocateStatus = ElementLocator.eLocateStatus.Pending;

            try
            {
                try
                {
                    Protractor.NgWebDriver ngDriver = null;
                    if (locator.LocateBy == eLocateBy.ByngRepeat || locator.LocateBy == eLocateBy.ByngSelectedOption || locator.LocateBy == eLocateBy.ByngBind || locator.LocateBy == eLocateBy.ByngModel)
                    {
                        ngDriver = new Protractor.NgWebDriver(Driver);
                        ngDriver.WaitForAngular();
                    }
                    if (locator.LocateBy == eLocateBy.ByngRepeat)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Repeater(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngSelectedOption)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.SelectedOption(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngBind)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Binding(locator.LocateValue));
                    }
                    if (locator.LocateBy == eLocateBy.ByngModel)
                    {
                        elem = ngDriver.FindElement(Protractor.NgBy.Model(locator.LocateValue));
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception occured when LocateElementByLocator", ex);
                    if (AlwaysReturn)
                    {
                        elem = null;
                        locator.StatusError = ex.Message;
                        locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                        return elem;
                    }
                    else
                        throw;
                }

                if (locator.LocateBy == eLocateBy.ByID)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                        elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                    else
                        elem = Driver.FindElement(By.Id(locator.LocateValue));
                }

                if (locator.LocateBy == eLocateBy.ByName)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                        elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                    else
                        elem = Driver.FindElement(By.Name(locator.LocateValue));
                }

                if (locator.LocateBy == eLocateBy.ByHref)
                {
                    string pattern = @".+:\/\/[^\/]+";
                    string sel = "//a[contains(@href, '@RREEPP')]";
                    sel = sel.Replace("@RREEPP", new Regex(pattern).Replace(locator.LocateValue, ""));
                    try
                    {
                        if (locator.LocateValue.IndexOf("{RE:") >= 0)
                            elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                        else
                            elem = Driver.FindElement(By.XPath(sel));
                    }
                    catch (NoSuchElementException ex)
                    {
                        try
                        {
                            sel = "//a[href='@']";
                            sel = sel.Replace("@", locator.LocateValue);
                            elem = Driver.FindElement(By.XPath(sel));
                            locator.StatusError = ex.Message;
                        }
                        catch (Exception)
                        { }
                    }
                    catch (Exception)
                    { }
                }

                if (locator.LocateBy == eLocateBy.ByLinkText)
                {
                    locator.LocateValue = locator.LocateValue.Trim();
                    try
                    {
                        if (locator.LocateValue.IndexOf("{RE:") >= 0)
                            elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                        else
                        {
                            elem = Driver.FindElement(By.LinkText(locator.LocateValue));
                            if (elem == null)
                                elem = Driver.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (ex.GetType() == typeof(NoSuchElementException))
                            {
                                elem = Driver.FindElement(By.XPath("//*[text()='" + locator.LocateValue + "']"));

                            }
                        }
                        catch (Exception ex2)
                        {
                            locator.StatusError = ex2.Message;
                        }
                    }

                }
                if (locator.LocateBy == eLocateBy.ByXPath || locator.LocateBy == eLocateBy.ByRelXPath)
                {
                    elem = Driver.FindElement(By.XPath(locator.LocateValue));
                }

                if (locator.LocateBy == eLocateBy.ByValue)
                {
                    if (locator.LocateValue.IndexOf("{RE:") >= 0)
                        elem = FindElementReg(locator.LocateBy, locator.LocateValue);
                    else
                        elem = Driver.FindElement(By.XPath("//*[@value=\"" + locator.LocateValue + "\"]"));
                }

                if (locator.LocateBy == eLocateBy.ByAutomationID)
                {
                    elem = Driver.FindElement(By.XPath("//*[@data-automation-id=\"" + locator.LocateValue + "\"]"));
                }

                if (locator.LocateBy == eLocateBy.ByCSS)
                {
                    elem = Driver.FindElement(By.CssSelector(locator.LocateValue));
                }

                if (locator.LocateBy == eLocateBy.ByClassName)
                {
                    elem = Driver.FindElement(By.ClassName(locator.LocateValue));
                }

                if (locator.LocateBy == eLocateBy.ByMulitpleProperties)
                {
                    elem = GetElementByMutlipleAttributes(locator.LocateValue);
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (AlwaysReturn == true)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                if (AlwaysReturn == true)
                {
                    elem = null;
                    locator.StatusError = ex.Message;
                    locator.LocateStatus = ElementLocator.eLocateStatus.Failed;
                    return elem;
                }
                else
                    throw ex;
            }

            if (elem != null)
            {
                locator.LocateStatus = ElementLocator.eLocateStatus.Passed;
            }

            return elem;
        }

        private IWebElement GetElementByMutlipleAttributes(string LocValue)
        {
            //Fix me
            //put in hash map
            // find by id or common then by other attrs
            string[] a = LocValue.Split(';');
            string[] a0 = a[0].Split('=');

            string id = null;
            if (a0[0] == "id") id = a0[1];

            string[] a1 = a[1].Split('=');
            string attr = a1[0];
            string val = a1[1];

            if (id == null)
            {
                return null;
            }
            ReadOnlyCollection<IWebElement> list = Driver.FindElements(By.Id(id));

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

                    HTMLElementInfo elementInfo = new HTMLElementInfo();
                    elementInfo.ElementObject = e;

                    HighlightElement(elementInfo);
                }
            }
        }

        public override ePlatformType Platform
        {
            get { return ePlatformType.Web; }
        }
        private int exceptioncount = 0;



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

                try
                {
                    int count = 0;
                    IAsyncResult result;
                    Action action = () =>
                    {
                        try
                        {
                            Thread.Sleep(100);
                            count = Driver.WindowHandles.ToList().Count;
                        }
                        catch (System.InvalidCastException ex)
                        {
                            exceptioncount = 0;
                            count = Driver.CurrentWindowHandle.Count();
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception occured while casting when we are checking IsRunning", ex);
                        }
                        catch (System.NullReferenceException ex)
                        {
                            count = Driver.CurrentWindowHandle.Count();
                            Reporter.ToLog(eLogLevel.DEBUG, "Null refrence exception occured when we are checking IsRunning", ex);
                        }
                        catch (Exception ex)
                        {
                            //throw exception to outer catch
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception occured when we are checking IsRunning", ex);
                            throw;
                        }

                    };
                    result = action.BeginInvoke(null, null);

                    if (result.AsyncWaitHandle.WaitOne(10000, true))
                    {
                        if (count == 0)
                            return false;
                        if (count > 0)
                            return true;
                    }
                    else
                    {
                        if (exceptioncount < 5)
                        {
                            exceptioncount++;
                            return (IsRunning());
                        }
                        var currentWindow = Driver.CurrentWindowHandle;
                        if (!string.IsNullOrEmpty(currentWindow))
                            return true;
                    }
                    if (count == 0)
                        return false;
                }
                catch (OpenQA.Selenium.UnhandledAlertException)
                {
                    return true;
                }
                catch (OpenQA.Selenium.NoSuchWindowException ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception occured when we are checking IsRunning", ex);
                    var currentWindow = Driver.CurrentWindowHandle;
                    if (!string.IsNullOrEmpty(currentWindow))
                        return true;
                    if (exceptioncount < 5)
                    {
                        exceptioncount++;
                        return (IsRunning());
                    }
                }
                catch (OpenQA.Selenium.WebDriverTimeoutException ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Timeout exception occured when we are checking IsRunning", ex);
                    var currentWindow = Driver.CurrentWindowHandle;
                    if (!string.IsNullOrEmpty(currentWindow))
                        return true;
                    if (exceptioncount < 5)
                    {
                        exceptioncount++;
                        return (IsRunning());
                    }
                }
                catch (OpenQA.Selenium.WebDriverException ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Webdriver exception occured when we are checking IsRunning", ex);

                    if (PreviousRunStopped && ex.Message == "Unexpected error. Error 404: Not Found\r\nNot Found")
                        return true;
                    if (exceptioncount < 5)
                    {
                        exceptioncount++;
                        return (IsRunning());
                    }
                    return false;
                }
                catch (Exception ex2)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Exception occured when we are checking IsRunning", ex2);
                    if (ex2.Message.ToString().ToUpper().Contains("DIALOG"))
                        return true;

                    return false;
                }
                return true;
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
                List<AppWindow> list = new List<AppWindow>();

                ReadOnlyCollection<string> windows = Driver.WindowHandles;
                //TODO: get current win and keep, later on set in combo
                foreach (string window in windows)
                {
                    Driver.SwitchTo().Window(window);
                    AppWindow AW = new AppWindow();
                    AW.Title = Driver.Title;
                    AW.WindowType = AppWindow.eWindowType.SeleniumWebPage;
                    list.Add(AW);
                }
                return list;
            }
            return null;
        }

        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false)
        {
            mIsDriverBusy = true;

            try
            {
                UnhighlightLast();

                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                List<ElementInfo> list = new List<ElementInfo>();
                Driver.SwitchTo().DefaultContent();
                allReadElem.Clear();
                list = General.ConvertObservableListToList<ElementInfo>(GetAllElementsFromPage("", filteredElementType, foundElementsList));
                allReadElem.Clear();
                CurrentFrame = "";
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan();
                Driver.SwitchTo().DefaultContent();
                return list;
            }
            finally
            {
                mIsDriverBusy = false;
            }

        }


        private ObservableList<ElementInfo> GetAllElementsFromPage(string path, List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null)
        {
            if (foundElementsList == null)
                foundElementsList = new ObservableList<ElementInfo>();

            string documentContents = Driver.PageSource;
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(documentContents);
            IEnumerable<HtmlNode> htmlElements = htmlDoc.DocumentNode.Descendants().Where(x => !x.Name.StartsWith("#"));

            if (htmlElements.Count() != 0)
            {
                foreach (HtmlNode htmlElemNode in htmlElements)
                {
                    try
                    {
                        if (mStopProcess)
                        {
                            return foundElementsList;
                        }

                        //get Element Type
                        Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(htmlNode: htmlElemNode);

                        // set the Flag in case you wish to learn the element or not
                        bool learnElement = true;

                        //filter element if needed, in case we need to learn only the MappedElements .i.e., LearnMappedElementsOnly is checked
                        if (filteredElementType != null)
                        {
                            //Case Learn Only Mapped Element : set learnElement to false in case element doesn't exist in the filteredElementType List AND element is not frame element
                            if (!filteredElementType.Contains(elementTypeEnum.Item2))
                                learnElement = false;
                        }

                        if (learnElement)
                        {
                            IWebElement el = Driver.FindElement(By.XPath(htmlElemNode.XPath));
                            if (el == null)
                            {
                                continue;
                            }

                            //filter none visible elements
                            if (!el.Displayed || el.Size.Width == 0 || el.Size.Height == 0)
                            {
                                continue;
                            }

                            HTMLElementInfo foundElemntInfo = new HTMLElementInfo();
                            foundElemntInfo.ElementType = elementTypeEnum.Item1;
                            foundElemntInfo.ElementTypeEnum = elementTypeEnum.Item2;
                            foundElemntInfo.ElementObject = el;
                            foundElemntInfo.Path = path;
                            foundElemntInfo.XPath = htmlElemNode.XPath;
                            foundElemntInfo.HTMLElementObject = htmlElemNode;
                            ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo);

                            foundElemntInfo.IsAutoLearned = true;
                            foundElementsList.Add(foundElemntInfo);

                            allReadElem.Add(foundElemntInfo);
                        }

                        if (eElementType.Iframe == elementTypeEnum.Item2)
                        {
                            string xpath = htmlElemNode.XPath;
                            Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(xpath)));
                            string newPath = string.Empty;
                            if (path == string.Empty)
                            {
                                newPath = xpath;
                            }
                            else
                            {
                                newPath = path + "," + xpath;
                            }
                            GetAllElementsFromPage(newPath, filteredElementType, foundElementsList);
                            Driver.SwitchTo().ParentFrame();
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, string.Format("Failed to learn the Web Element '{0}'", htmlElemNode.Name), ex);
                    }
                }
            }

            return foundElementsList;
        }

        public static Tuple<string, eElementType> GetElementTypeEnum(IWebElement el = null, string jsType = null, HtmlNode htmlNode = null)
        {
            Tuple<string, eElementType> returnTuple;
            eElementType elementType = eElementType.Unknown;
            string elementTagName = string.Empty;
            string elementTypeAtt = string.Empty;

            if ((el == null) && (jsType != null))
            {
                elementTagName = jsType;
                elementTypeAtt = string.Empty;
            }
            else if ((el != null) && (jsType == null))
            {
                if ((el.TagName != null) && (el.TagName != string.Empty))
                    elementTagName = el.TagName.ToUpper();
                else
                    elementTagName = "INPUT";
                elementTypeAtt = el.GetAttribute("type");
            }
            else if (htmlNode != null)
            {
                elementTagName = htmlNode.Name;
                if (htmlNode.Attributes.Where(x => x.Name == "type").FirstOrDefault() != null)
                {
                    elementTypeAtt = htmlNode.Attributes["type"].Value;
                }
            }
            else
            {
                returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);
                return returnTuple;
            }

            if ((elementTagName.ToUpper() == "INPUT" && (elementTypeAtt.ToUpper() == "UNDEFINED" || elementTypeAtt.ToUpper() == "TEXT" || elementTypeAtt.ToUpper() == "PASSWORD" || elementTypeAtt.ToUpper() == "EMAIL"
                                                        || elementTypeAtt.ToUpper() == "TEL" || elementTypeAtt.ToUpper() == "SEARCH" || elementTypeAtt.ToUpper() == "NUMBER" || elementTypeAtt.ToUpper() == "URL"
                                                        || elementTypeAtt.ToUpper() == "DATE")) || elementTagName.ToUpper() == "TEXTAREA" || elementTagName.ToUpper() == "TEXT")
            {
                elementType = eElementType.TextBox;
            }
            else if ((elementTagName.ToUpper() == "INPUT" && (elementTypeAtt.ToUpper() == "IMAGE" || elementTypeAtt.ToUpper() == "SUBMIT" || elementTypeAtt.ToUpper() == "BUTTON")) ||
                    elementTagName.ToUpper() == "BUTTON" || elementTagName.ToUpper() == "SUBMIT" || elementTagName.ToUpper() == "RESET")
            {
                elementType = eElementType.Button;
            }
            else if (elementTagName.ToUpper() == "TD" || elementTagName.ToUpper() == "TH" || elementTagName.ToUpper() == "TR")
            {
                elementType = eElementType.TableItem;
            }
            else if (elementTagName.ToUpper() == "LINK" || elementTagName.ToUpper() == "A" || elementTagName.ToUpper() == "LI")
            {
                elementType = eElementType.HyperLink;
            }
            else if (elementTagName.ToUpper() == "LABEL" || elementTagName.ToUpper() == "TITLE")
            {
                elementType = eElementType.Label;
            }
            else if (elementTagName.ToUpper() == "SELECT" || elementTagName.ToUpper() == "SELECT-ONE")
            {
                elementType = eElementType.ComboBox;
            }
            else if (elementTagName.ToUpper() == "TABLE" || elementTagName.ToUpper() == "CAPTION")
            {
                elementType = eElementType.Table;
            }
            else if (elementTagName.ToUpper() == "JEDITOR.TABLE")
            {
                elementType = eElementType.EditorPane;
            }
            else if (elementTagName.ToUpper() == "DIV")
            {
                elementType = eElementType.Div;
            }
            else if (elementTagName.ToUpper() == "SPAN")
            {
                elementType = eElementType.Span;
            }
            else if (elementTagName.ToUpper() == "IMG" || elementTagName.ToUpper() == "MAP")
            {
                elementType = eElementType.Image;
            }
            else if ((elementTagName.ToUpper() == "INPUT" && elementTypeAtt.ToUpper() == "CHECKBOX") || (elementTagName.ToUpper() == "CHECKBOX"))
            {
                elementType = eElementType.CheckBox;
            }
            else if (elementTagName.ToUpper() == "OPTGROUP" || elementTagName.ToUpper() == "OPTION")
            {

                elementType = eElementType.ComboBoxOption;
            }
            else if ((elementTagName.ToUpper() == "INPUT" && elementTypeAtt.ToUpper() == "RADIO") || (elementTagName.ToUpper() == "RADIO"))
            {
                elementType = eElementType.RadioButton;
            }
            else if (elementTagName.ToUpper() == "IFRAME" || elementTagName.ToUpper() == "FRAME" || elementTagName.ToUpper() == "FRAMESET")
            {
                elementType = eElementType.Iframe;
            }
            else if (elementTagName.ToUpper() == "CANVAS")
            {
                elementType = eElementType.Canvas;
            }
            else if (elementTagName.ToUpper() == "FORM")
            {
                elementType = eElementType.Form;
            }
            else if (elementTagName.ToUpper() == "UL" || elementTagName.ToUpper() == "OL" || elementTagName.ToUpper() == "DL")
            {
                elementType = eElementType.List;
            }
            else if (elementTagName.ToUpper() == "LI" || elementTagName.ToUpper() == "DT" || elementTagName.ToUpper() == "DD")
            {
                elementType = eElementType.ListItem;
            }
            else if (elementTagName.ToUpper() == "MENU")
            {
                elementType = eElementType.MenuBar;
            }
            else if (elementTagName.ToUpper() == "H1" || elementTagName.ToUpper() == "H2" || elementTagName.ToUpper() == "H3" || elementTagName.ToUpper() == "H4" || elementTagName.ToUpper() == "H5" || elementTagName.ToUpper() == "H6" || elementTagName.ToUpper() == "P")
            {
                elementType = eElementType.Text;
            }
            else
                elementType = eElementType.Unknown;
            returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);

            return returnTuple;
        }

        ElementInfo IWindowExplorer.LearnElementInfoDetails(ElementInfo EI)
        {
            if (string.IsNullOrEmpty(EI.ElementType) || EI.ElementTypeEnum == eElementType.Unknown)
            {
                Tuple<string, eElementType> elementTypeEnum = GetElementTypeEnum(EI.ElementObject as IWebElement);
                EI.ElementType = elementTypeEnum.Item1;
                EI.ElementTypeEnum = elementTypeEnum.Item2;
            }


            if (string.IsNullOrEmpty(EI.XPath) || EI.XPath == "/")
            {
                EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, EI.Path);
            }

            EI.ElementName = GetElementName(EI as HTMLElementInfo);
            ((HTMLElementInfo)EI).RelXpath = mXPathHelper.GetElementRelXPath(EI);
            EI.Locators = ((IWindowExplorer)this).GetElementLocators(EI);
            EI.Properties = ((IWindowExplorer)this).GetElementProperties(EI);// improve code inside

            return EI;
        }

        //private HTMLElementInfo GetElementInfoWithIWebElement(IWebElement el, HtmlNode elNode, string path, bool setFullElementInfoDetails = false)
        //{
        //    HTMLElementInfo EI = new HTMLElementInfo();
        //    EI.WindowExplorer = this;
        //    EI.ElementTitle = GenerateElementTitle(el);
        //    EI.ID = GenerateElementID(el);
        //    EI.Value = GenerateElementValue(el);
        //    EI.Name = GenerateElementName(el);
        //    EI.ElementType = GenerateElementType(el);
        //    EI.ElementTypeEnum = GetElementTypeEnum(el).Item2;
        //    EI.Path = path;
        //    if (elNode != null)
        //    {
        //        EI.XPath = elNode.XPath;
        //    }
        //    else
        //    {
        //        EI.XPath = string.Empty;
        //    }
        //    EI.ElementObject = el;
        //    EI.HTMLElementObject = elNode;

        //    if (setFullElementInfoDetails)
        //    {
        //        EI.RelXpath = mXPathHelper.GetElementRelXPath(EI);
        //        EI.ElementName = GetElementName(EI);
        //        EI.Locators = ((IWindowExplorer)this).GetElementLocators(EI);
        //        ((IWindowExplorer)this).UpdateElementInfoFields(EI);
        //        EI.Properties = ((IWindowExplorer)this).GetElementProperties(EI);
        //    }

        //    return EI;
        //}

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

                string text = ((IWebElement)EI.ElementObject).Text;


                if (text.Count() > 15)
                {
                    text = string.Empty;
                }

                List<string> list = new List<string>() { tagName, text, type, name, title, title, id, value };

                foreach (string att in list)
                {
                    if (!string.IsNullOrEmpty(att) && !bestElementName.Contains(att))
                        bestElementName = bestElementName + " " + att;
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
            string xPath = GenerateXpathForIWebElement(el, "");
            HTMLElementInfo EI = new HTMLElementInfo();
            EI.ElementTitle = GenerateElementTitle(el);
            EI.WindowExplorer = this;
            EI.ID = GenerateElementID(el);
            EI.Value = GenerateElementValue(el);
            EI.Name = GenerateElementName(el);
            EI.ElementType = GenerateElementType(el);
            EI.ElementTypeEnum = GetElementTypeEnum(el).Item2;
            EI.Path = path;
            EI.XPath = xPath;
            EI.ElementObject = el;
            return EI;
        }

        private ElementInfo GetRootElement()
        {
            ElementInfo RootEI = new ElementInfo();
            RootEI.ElementTitle = "html";
            RootEI.ElementType = "root";
            RootEI.Value = string.Empty;
            RootEI.Path = string.Empty;
            RootEI.XPath = "html";
            return RootEI;
        }

        private void SwitchFrameFromCurrent(ElementInfo ElementInfo)
        {
            string[] spliter = new string[] { "/" };
            string[] elementsTypesPath = ElementInfo.XPath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[elementsTypesPath.Length - 1];

            int index = elementType.IndexOf("[");
            if (index != -1)
                elementType = elementType.Substring(0, index);

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
                    CurrentFrame = ElementInfo.XPath;
                else
                    CurrentFrame = CurrentFrame + "," + ElementInfo.XPath;
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
            string[] spliter = new string[] { "," };
            string[] iframesPathes = ElementInfo.Path.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            foreach (string iframePath in iframesPathes)
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(iframePath)));
            }
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            allReadElem.Clear();
            allReadElem.Add(ElementInfo);
            List<ElementInfo> list = new List<ElementInfo>();
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

        private string GeneratePath(string xpath)
        {
            string[] spliter = new string[] { "/" };
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[elementsTypesPath.Length - 1];

            string path = string.Empty;
            int index = elementType.IndexOf("[");
            if (index != -1)
                elementType = elementType.Substring(0, index);

            if (elementType == "iframe" || elementType == "frame")
                path = "/html/*";
            else
                path = xpath + "/*";

            return path;
        }

        private void SwitchFrame(string path, string xpath, bool otherThenGetElementChildren = false)
        {
            string elementType = string.Empty;
            if (!string.IsNullOrEmpty(xpath))
            {
                string[] xpathSpliter = new string[] { "/" };
                string[] elementsTypesPath = xpath.Split(xpathSpliter, StringSplitOptions.RemoveEmptyEntries);
                if (elementsTypesPath.Count() == 0)
                {
                    return;
                }
                elementType = elementsTypesPath[elementsTypesPath.Length - 1];

                int index = elementType.IndexOf("[");
                if (index != -1)
                    elementType = elementType.Substring(0, index);
            }

            if ((elementType == "iframe" || elementType == "frame") && string.IsNullOrEmpty(path) && !otherThenGetElementChildren)
            {
                Driver.SwitchTo().Frame(Driver.FindElement(By.XPath(xpath)));
                return;
            }

            if (path != null)
            {
                string[] spliter = new string[] { "," };
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
            List<ElementInfo> list = new List<ElementInfo>();
            Dictionary<string, int> ElementsIndexes = new Dictionary<string, int>();
            Dictionary<string, int> ElementsCount = new Dictionary<string, int>();

            if (string.IsNullOrEmpty(path))
                path = string.Empty;

            foreach (IWebElement EL in ElementsList)
            {

                if (!ElementsCount.ContainsKey(EL.TagName))
                    ElementsCount.Add(EL.TagName, 1);
                else
                    ElementsCount[EL.TagName] += 1;
            }

            foreach (IWebElement EL in ElementsList)
            {
                if (!ElementsIndexes.ContainsKey(EL.TagName))
                    ElementsIndexes.Add(EL.TagName, 0);
                else
                    ElementsIndexes[EL.TagName] += 1;
                HTMLElementInfo EI = new HTMLElementInfo();
                EI.ElementObject = EL;
                EI.ElementTitle = GenerateElementTitle(EL);
                EI.WindowExplorer = this;
                EI.Name = GenerateElementName(EL);
                EI.ID = GenerateElementID(EL);
                EI.Value = GenerateElementValue(EL);
                EI.Path = GenetratePath(path, xpath, EL.TagName);
                EI.XPath = GenerateXpath(path, xpath, EL.TagName, ElementsIndexes[EL.TagName], ElementsCount[EL.TagName]); /*EI.GetAbsoluteXpath(); */
                EI.ElementType = GenerateElementType(EL);
                EI.ElementTypeEnum = GetElementTypeEnum(EL).Item2;
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
                name = EL.GetAttribute("name");
            if (string.IsNullOrEmpty(name))
                name = string.Empty;
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
            string[] spliter = new string[] { "/" };
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[elementsTypesPath.Length - 1];

            int index = elementType.IndexOf("[");
            if (index != -1)
                elementType = elementType.Substring(0, index);

            if (elementType == "iframe" || elementType == "frame")
                if (!string.IsNullOrEmpty(path))
                    return path + "," + xpath;
                else
                    return xpath;
            else
                return path;
        }

        private string GenerateXpath(string path, string xpath, string tagName, int id, int totalSameTags)
        {
            string[] spliter = new string[] { "/" };
            string[] elementsTypesPath = xpath.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            string elementType = elementsTypesPath[elementsTypesPath.Length - 1];

            int index = elementType.IndexOf("[");
            if (index != -1)
                elementType = elementType.Substring(0, index);


            string returnXPath = string.Empty;
            if (elementType == "iframe" || elementType == "frame")
                xpath = "html";

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

            if (tagName.ToUpper() == "TABLE")
                return "Table";

            if (!string.IsNullOrEmpty(name))
                return name + " " + tagName;

            if (!string.IsNullOrEmpty(id))
                return id + " " + tagName;

            if (!string.IsNullOrEmpty(value))
                return GetShortName(value) + " " + tagName;

            return tagName;
        }

        private string GetShortName(string value)
        {
            string returnString = value;
            if (value.Length > 50)
                returnString = value.Substring(0, 50) + "...";
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
                    ElementValue = EL.Text;
                else
                    ElementValue = value;
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
                elementType = tagName + "." + type;
            else if (tagName == "a" || tagName == "li")
                elementType = "link";
            else
                elementType = tagName;

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
                Driver.SwitchTo().Window(winHandle);
                string winTitle = Driver.Title;
                if (winTitle == Title)
                {
                    windowfound = true;
                    break;
                }
            }
            if (!windowfound)
            {
                Driver.SwitchTo().Window(currentWindow);
            }
        }



        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {

            HighlightElement(ElementInfo, locateElementByItLocators);
        }



        private void HighlightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            try
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);
                UnhighlightLast();

                Driver.SwitchTo().DefaultContent();
                SwitchFrame(ElementInfo.Path, ElementInfo.XPath, true);

                //Find element 
                if (locateElementByItLocators)
                {
                    ElementInfo.ElementObject = LocateElementByLocators(ElementInfo.Locators);
                }
                else
                {
                    if (string.IsNullOrEmpty(ElementInfo.XPath))
                    {
                        ElementInfo.XPath = GenerateXpathForIWebElement((IWebElement)ElementInfo.ElementObject, "");
                    }
                    if (ElementInfo is HTMLElementInfo && string.IsNullOrEmpty(((HTMLElementInfo)ElementInfo).RelXpath))
                    {
                        ((HTMLElementInfo)ElementInfo).RelXpath = mXPathHelper.GetElementRelXPath(ElementInfo);
                    }
                    ElementInfo.ElementObject = Driver.FindElement(By.XPath(ElementInfo.XPath));
                }
                if ((IWebElement)ElementInfo.ElementObject == null)
                {
                    return;
                }

                //Highlight element
                IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)Driver;

                List<string> attributesList = new List<string>() { "arguments[0].style.outline='3px dashed rgb(239, 183, 247)'", "arguments[0].style.backgroundColor='rgb(239, 183, 247)'", "arguments[0].style.border='3px dashed rgb(239, 183, 247)'" };

                foreach (string attribuet in attributesList)
                {
                    javascriptDriver.ExecuteScript(attribuet, new object[] { (IWebElement)ElementInfo.ElementObject });
                }

                LastHighLightedElement = (IWebElement)ElementInfo.ElementObject;
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));
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
                    List<string> attributesList = new List<string>() { "arguments[0].style.outline=''", "arguments[0].style.backgroundColor=''", "arguments[0].style.border=''" };
                    IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)Driver;
                    foreach (string attribuet in attributesList)
                    {
                        javascriptDriver.ExecuteScript(attribuet, new object[] { LastHighLightedElement });
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
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);

                ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

                IWebElement el = null;
                if (ElementInfo.ElementObject != null)
                {
                    el = (IWebElement)ElementInfo.ElementObject;
                }
                else
                {
                    if (string.IsNullOrEmpty(ElementInfo.XPath))
                        ElementInfo.XPath = GenerateXpathForIWebElement((IWebElement)ElementInfo.ElementObject, "");
                    el = Driver.FindElement(By.XPath(ElementInfo.XPath));
                    ElementInfo.ElementObject = el;
                }

                //Base properties 
                if (!string.IsNullOrWhiteSpace(ElementInfo.ElementType))
                {
                    list.Add(new ControlProperty() { Name = "Platform Element Type", Value = ElementInfo.ElementType });
                }
                list.Add(new ControlProperty() { Name = "Element Type", Value = ElementInfo.ElementTypeEnum.ToString() });
                if (!string.IsNullOrWhiteSpace(ElementInfo.Path))
                {
                    list.Add(new ControlProperty() { Name = "Parent IFrame", Value = ElementInfo.Path });
                }
                if (!string.IsNullOrWhiteSpace(ElementInfo.XPath))
                {
                    list.Add(new ControlProperty() { Name = "XPath", Value = ElementInfo.XPath });
                }
                if (!string.IsNullOrWhiteSpace(((HTMLElementInfo)ElementInfo).RelXpath))
                {
                    list.Add(new ControlProperty() { Name = "Relative XPath", Value = ((HTMLElementInfo)ElementInfo).RelXpath });
                }
                list.Add(new ControlProperty() { Name = "Height", Value = ((IWebElement)ElementInfo.ElementObject).Size.Height.ToString() });
                list.Add(new ControlProperty() { Name = "Width", Value = ((IWebElement)ElementInfo.ElementObject).Size.Width.ToString() });
                list.Add(new ControlProperty() { Name = "X", Value = ((IWebElement)ElementInfo.ElementObject).Location.X.ToString() });
                list.Add(new ControlProperty() { Name = "Y", Value = ((IWebElement)ElementInfo.ElementObject).Location.Y.ToString() });
                if (!string.IsNullOrWhiteSpace(ElementInfo.Value))
                {
                    list.Add(new ControlProperty() { Name = "Value", Value = ElementInfo.Value });
                }

                if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null)
                {
                    if (ElementInfo.IsElementTypeSupportingOptionalValues(ElementInfo.ElementTypeEnum))
                    {
                        foreach (HtmlNode childNode in ((HTMLElementInfo)ElementInfo).HTMLElementObject.ChildNodes)
                        {
                            if (!childNode.Name.StartsWith("#") && !string.IsNullOrEmpty(childNode.InnerText))
                            {
                                string[] tempOpVals = childNode.InnerText.Split('\n');
                                foreach (string cuVal in tempOpVals)
                                {
                                    ElementInfo.OptionalValuesObjectsList.Add(new OptionalValue() { Value = cuVal, IsDefault = false });
                                }
                            }
                        }
                        if (ElementInfo.OptionalValuesObjectsList.Count > 0)
                        {
                            ElementInfo.OptionalValuesObjectsList[0].IsDefault = true;
                            list.Add(new ControlProperty() { Name = "Optional Values", Value = ElementInfo.OptionalValuesObjectsListAsString.Replace("*", "") });
                        }

                    }
                    if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null)
                    {
                        HtmlAttributeCollection htmlAttributes = ((HTMLElementInfo)ElementInfo).HTMLElementObject.Attributes;

                        foreach (HtmlAttribute htmlAttribute in htmlAttributes)
                        {
                            ControlProperty existControlProperty = list.Where(x => x.Name == htmlAttribute.Name && x.Value == htmlAttribute.Value).FirstOrDefault();
                            if (existControlProperty == null)
                            {
                                ControlProperty controlProperty = new ControlProperty() { Name = htmlAttribute.Name, Value = htmlAttribute.Value };
                                list.Add(controlProperty);
                            }
                        }
                    }
                }
                else if (el != null)
                {
                    if (ElementInfo.IsElementTypeSupportingOptionalValues(ElementInfo.ElementTypeEnum))
                    {
                        ReadOnlyCollection<IWebElement> elementsList = el.FindElements(By.XPath("*"));
                        foreach (IWebElement val in elementsList)
                        {
                            if (!string.IsNullOrEmpty(val.Text))
                            {
                                string[] tempOpVals = val.Text.Split('\n');
                                if (tempOpVals != null && tempOpVals.Length > 1)
                                {
                                    foreach (string cuVal in tempOpVals)
                                    {
                                        ElementInfo.OptionalValuesObjectsList.Add(new OptionalValue() { Value = cuVal, IsDefault = false });
                                    }
                                }
                                else
                                {
                                    ElementInfo.OptionalValuesObjectsList.Add(new OptionalValue() { Value = val.Text, IsDefault = false });
                                }
                            }
                        }

                        if (ElementInfo.OptionalValuesObjectsList.Count > 0)
                        {
                            ElementInfo.OptionalValuesObjectsList[0].IsDefault = true;
                            list.Add(new ControlProperty() { Name = "Optional Values", Value = ElementInfo.OptionalValuesObjectsListAsString.Replace("*", "") });
                        }

                    }

                    IJavaScriptExecutor javascriptDriver = (IJavaScriptExecutor)Driver;
                    Dictionary<string, object> attributes = javascriptDriver.ExecuteScript("var items = {}; for (index = 0; index < arguments[0].attributes.length; ++index) { items[arguments[0].attributes[index].name] = arguments[0].attributes[index].value }; return items;", el) as Dictionary<string, object>;
                    if (attributes != null)
                    {
                        foreach (KeyValuePair<string, object> kvp in attributes)
                        {
                            if (kvp.Key != "style" && (kvp.Value.ToString() != "border: 3px dashed red;" || kvp.Value.ToString() != "outline: 3px dashed red;"))
                            {
                                string PName = kvp.Key;
                                string PValue = kvp.Value.ToString();
                                list.Add(new ControlProperty() { Name = PName, Value = PValue });
                            }
                        }
                    }
                }

                return list;
            }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));
            }

        }

        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            IWebElement e = Driver.FindElement(By.XPath(ElementInfo.XPath));
            if (e.TagName == "select")  // combo box
            {
                return GetComboValues(ElementInfo);
            }
            if (e.TagName == "table")  // Table
            {
                return GetTableData(ElementInfo);
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
            List<ComboBoxElementItem> ComboValues = new List<ComboBoxElementItem>();
            IWebElement e = Driver.FindElement(By.XPath(ElementInfo.XPath));
            SelectElement se = new SelectElement(e);
            IList<IWebElement> options = se.Options;
            foreach (IWebElement o in options)
            {
                ComboValues.Add(new ComboBoxElementItem() { Value = o.GetAttribute("value"), Text = o.Text });
            }
            return ComboValues;
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            ObservableList<ElementLocator> locatorsList = new Platforms.PlatformsInfo.WebPlatform().GetLearningLocators();
            IWebElement e = null;

            if (ElementInfo.ElementObject != null)
            {
                e = (IWebElement)ElementInfo.ElementObject;
            }
            else
            {
                //e = LocateElementByLocators(ElementInfo.Locators);
                e = Driver.FindElement(By.XPath(ElementInfo.XPath));
                ElementInfo.ElementObject = e;
            }

            foreach (ElementLocator elemLocator in locatorsList)
            {
                switch (elemLocator.LocateBy)
                {
                    // Organize based on better locators at start
                    case eLocateBy.ByID:
                        string id = string.Empty;
                        if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null && !string.IsNullOrEmpty(((HTMLElementInfo)ElementInfo).ID))
                        {
                            HtmlAttribute idAttribute = ((HTMLElementInfo)ElementInfo).HTMLElementObject.Attributes.Where(x => x.Name == "id").FirstOrDefault();
                            if (idAttribute != null)
                            {
                                id = idAttribute.Value;
                            }
                            else
                            {
                                id = ((HTMLElementInfo)ElementInfo).ID;
                            }
                        }
                        else
                        {
                            id = e.GetAttribute("id");
                        }
                        if (!string.IsNullOrWhiteSpace(id))
                        {
                            elemLocator.LocateValue = id;
                            elemLocator.IsAutoLearned = true;
                        }
                        break;

                    case eLocateBy.ByName:

                        string name = string.Empty;
                        if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null && !string.IsNullOrEmpty(((HTMLElementInfo)ElementInfo).Name))
                        {
                            HtmlAttribute nameAttribute = ((HTMLElementInfo)ElementInfo).HTMLElementObject.Attributes.Where(x => x.Name == "name").FirstOrDefault();
                            if (nameAttribute != null)
                            {
                                name = nameAttribute.Value;
                            }
                            else
                            {
                                name = ((HTMLElementInfo)ElementInfo).Name;
                            }
                        }
                        else
                        {
                            name = e.GetAttribute("name");
                        }

                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            elemLocator.LocateValue = name;
                            elemLocator.IsAutoLearned = true;
                        }
                        break;

                    case eLocateBy.ByRelXPath:
                        elemLocator.LocateValue = ((HTMLElementInfo)ElementInfo).RelXpath;
                        elemLocator.IsAutoLearned = true;
                        break;

                    case eLocateBy.ByXPath:
                        elemLocator.LocateValue = ElementInfo.XPath;
                        elemLocator.IsAutoLearned = true;
                        break;
                }
            }
            locatorsList = new ObservableList<ElementLocator>(locatorsList.Where(x => x.IsAutoLearned).ToList());
            return locatorsList;
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
                }
                catch
                {
                    mListnerCanBeStarted = false;
                    Reporter.ToLog(eLogLevel.DEBUG, "Spy Listener cannot be started");
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
                    string XPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetXPoint();");
                    string YPoint = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.GetYPoint();");
                    el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.elementFromPoint(" + XPoint + ", " + YPoint + ");");

                }
                else
                {
                    el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");
                }
                HTMLElementInfo foundElemntInfo = new HTMLElementInfo();

                foundElemntInfo.ElementObject = el;
                foundElemntInfo.Path = string.Empty;

                if (el.TagName == "iframe" || el.TagName == "frame")
                {
                    foundElemntInfo.Path = string.Empty;
                    foundElemntInfo.XPath = GenerateXpathForIWebElement(el, "");
                    return GetElementFromIframe(foundElemntInfo);
                }
                return foundElemntInfo;
            }
            catch
            { }
            finally
            {
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan();
            }
            return null;
        }

        private ElementInfo GetElementFromIframe(ElementInfo IframeElementInfo)
        {
            SwitchFrame(IframeElementInfo.Path, IframeElementInfo.XPath, false);
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
            string IframePath;
            if (IframeElementInfo.Path != string.Empty)
                IframePath = IframeElementInfo.Path + "," + IframeElementInfo.XPath;
            else
                IframePath = IframeElementInfo.XPath;

            HTMLElementInfo foundElemntInfo = new HTMLElementInfo();

            foundElemntInfo.ElementObject = elInsideIframe;
            if (elInsideIframe.TagName == "iframe" || elInsideIframe.TagName == "frame")
            {
                Driver.SwitchTo().DefaultContent();
                foundElemntInfo.Path = string.Empty;
                foundElemntInfo.XPath = GenerateXpathForIWebElement(elInsideIframe, "");
                return GetElementFromIframe(foundElemntInfo);
            }
            return foundElemntInfo;
        }

        //public ElementInfo GetHTMLElementInfoFromIWebElement(IWebElement EL, string path)
        //{
        //    string xpath = GenerateXpathForIWebElement(EL, "");
        //    HTMLElementInfo EI = new HTMLElementInfo();
        //    EI.ElementObject = EL;
        //    EI.ElementTitle = GenerateElementTitle(EL);
        //    EI.WindowExplorer = this;
        //    EI.ID = GenerateElementID(EL);
        //    EI.Value = GenerateElementValue(EL);
        //    EI.Name = GenerateElementName(EL);
        //    EI.ElementType = GenerateElementType(EL);
        //    EI.ElementTypeEnum = GetElementTypeEnum(EL).Item2;
        //    EI.Path = path;
        //    EI.XPath = xpath;
        //    EI.RelXpath = mXPathHelper.GetElementRelXPath(EI);
        //    return EI;
        //}

        public string GenerateXpathForIWebElement(IWebElement IWE, string current)
        {
            if (IWE.TagName == "html")
                return "/" + IWE.TagName + "[1]" + current;

            IWebElement parentElement = IWE.FindElement(By.XPath(".."));
            ReadOnlyCollection<IWebElement> childrenElements = parentElement.FindElements(By.XPath("./" + IWE.TagName));
            int count = 1;
            foreach (IWebElement childElement in childrenElements)
            {
                try
                {
                    if (IWE.Equals(childElement))
                    {
                        return GenerateXpathForIWebElement(parentElement, "/" + IWE.TagName + "[" + count + "]" + current);
                    }
                    else
                    {
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    if (mBrowserTpe == eBrowserType.FireFox && ex.Message != null && ex.Message.Contains("did not match a known command"))
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
                AppWindow aw = new AppWindow();
                aw.Title = Driver.Title;
                return aw;
            }
            return null;

        }

        public void InjectGingerLiveSpy()
        {
            string GingerLiveSpyScript = Properties.Resources.GingerLiveSpy;
            AddJavaScriptToPage(GingerLiveSpyScript);

            ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerLibLiveSpy();", null);
            string rc = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLibLiveSpy.AddScript(arguments[0]);", Properties.Resources.jquery_min);
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
            string GingerPayLoadJS = Properties.Resources.PayLoad;
            AddJavaScriptToPage(GingerPayLoadJS);
            string GingerHTMLHelperScript = Properties.Resources.GingerHTMLHelper;
            AddJavaScriptToPage(GingerHTMLHelperScript);
            ((IJavaScriptExecutor)Driver).ExecuteScript("define_GingerLib();", null);

            //Inject JQuery
            string rc = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", Properties.Resources.jquery_min);

            // Inject XPath
            string rc2 = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", MinifyJS(Properties.Resources.GingerLibXPath));


            // Inject code which can find element by XPath
            string rc3 = (string)((IJavaScriptExecutor)Driver).ExecuteScript("return GingerLib.AddScript(arguments[0]);", Properties.Resources.wgxpath_install);
        }


        public void InjectGingerHTMLRecorder()
        {
            //do once
            string GingerHTMLRecorderScript = Properties.Resources.GingerHTMLRecorder;
            AddJavaScriptToPage(GingerHTMLRecorderScript);
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
                StopRecordingIfAgentClosed();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured while adding javascript to page", ex);
            }
        }

        void CheckifPageLoaded()
        {
            //TODO: slow function, try to check alternatives or let the user config wait for
            try
            {
                bool DomElementIncreasing = true;
                int CurrentDomElementSize = 0;
                int SameSizzeCounter = 0;
                while (DomElementIncreasing)
                {
                    Thread.Sleep(300);

                    int instanceSize = Driver.FindElements(By.CssSelector("*")).Count;

                    if (instanceSize > CurrentDomElementSize)
                    {
                        CurrentDomElementSize = instanceSize;
                        SameSizzeCounter = 0;
                        continue;
                    }
                    else
                    {
                        SameSizzeCounter++;
                        if (SameSizzeCounter == 5)
                        {
                            DomElementIncreasing = false;
                        }
                    }
                }
            }
            catch
            {
                // Do nothing...
            }
        }

        private string MinifyJS(string script)
        {
            //TODO: cache if possible
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
            var minifiedString = minifier.MinifyJavaScript(script);
            if (minifier.Errors.Count > 0)
            {
                //There are ERRORS !!!
                Console.WriteLine(script);
                return null;
            }
            return minifiedString + ";";
        }

        String GetInjectJSSCript(string script)
        {
            string ScriptMin = MinifyJS(script);
            // Get the Inject code
            string script2 = Properties.Resources.InjectJavaScript;
            script2 = MinifyJS(script2);
            //Note minifier change ' to ", so we change it back, so the script can have ", but we wrap it all with '
            string script3 = script2.Replace("\"%SCRIPT%\"", "'" + ScriptMin + "'");

            return script3;
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

            if (el.TagName == "iframe" || el.TagName == "frame")
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

                Act switchAct = new ActBrowserElement();
                switchAct.LocateBy = eLocateBy.ByXPath;
                ((ActBrowserElement)switchAct).ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
                switchAct.Description = "Switch Window to Iframe";
                switchAct.LocateValue = ElementInfo.XPath;
                this.BusinessFlow.AddAct(switchAct);
            }
            else if (el.TagName == "body" && IframeClicked && !IsLooped)
            {
                Driver.SwitchTo().DefaultContent();
                el = (IWebElement)((IJavaScriptExecutor)Driver).ExecuteScript("return document.activeElement;");

                if (el.TagName == "iframe" || el.TagName == "frame")
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

                        Act switchActionDefult = new ActBrowserElement();
                        switchActionDefult.LocateBy = eLocateBy.ByXPath;
                        ((ActBrowserElement)switchActionDefult).ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;
                        switchActionDefult.Description = "Switch Window to Default Iframe";
                        this.BusinessFlow.AddAct(switchActionDefult);

                        Act switchActionFrame = new ActBrowserElement();
                        switchActionFrame.LocateBy = eLocateBy.ByXPath;
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
                Act switchAct = new ActBrowserElement();
                switchAct.LocateBy = eLocateBy.ByXPath;
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
                                    RecordingEventArgs args = new RecordingEventArgs();
                                    args.EventType = eRecordingEvent.ElementRecorded;
                                    args.EventArgs = configArgs;
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
                        StopRecordingIfAgentClosed();
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
            ActUIElement actUI = new ActUIElement();
            actUI.Description = GetDescription(configArgs.Operation, configArgs.LocateValue, configArgs.ElementValue, Convert.ToString(configArgs.Type));
            actUI.ElementLocateBy = GetLocateBy(Convert.ToString(configArgs.LocateBy));
            actUI.ElementLocateValue = configArgs.LocateValue;
            actUI.ElementType = GetElementTypeEnum(null, Convert.ToString(configArgs.Type)).Item2;
            if (Enum.IsDefined(typeof(ActUIElement.eElementAction), configArgs.Operation))
                actUI.ElementAction = (ActUIElement.eElementAction)Enum.Parse(typeof(ActUIElement.eElementAction), configArgs.Operation);
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
        private void StopRecordingIfAgentClosed()
        {
            IsRecording = false;
            RecordingEventArgs args = new RecordingEventArgs();
            args.EventType = eRecordingEvent.StopRecording;
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

                        RecordingEventArgs args = new RecordingEventArgs();
                        args.EventType = eRecordingEvent.PageChanged;
                        args.EventArgs = pageArgs;
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
            switch (Type)
            {
                case "radio":
                    return ElemValue;
            }

            return LocateValue;
        }

        //Returns description for action recorder from HTML element
        public static string GetDescription(string ControlAction, string LocateValue, string ElemValue, string Type)
        {
            switch (Type)
            {
                case "button":
                    return "Click Button '" + LocateValue + "'";

                case "text":
                    return "Set Text '" + LocateValue + "'";

                case "textarea":
                    return "Set TextArea '" + LocateValue + "'";

                case "select-one":
                    return "Set Select '" + LocateValue + "'";

                case "checkbox":
                    return "Click Checkbox '" + LocateValue + "'";

                case "radio":
                    return "Click Radio '" + LocateValue + "'";

                case "SPAN":
                    return "Click SPAN '" + LocateValue + "'";

                case "li":
                    return "Click li '" + LocateValue + "'";
            }

            return "Set Web Element '" + LocateValue + "'";
        }

        //Returns Action for HTML element on PL
        public static ActGenElement.eGenElementAction GetElemAction(string ControlAction)
        {
            switch (ControlAction)
            {
                case "Click":
                    return ActGenElement.eGenElementAction.Click;

                case "SetValue":
                    return ActGenElement.eGenElementAction.SetValue;

                case "SendKeys":
                    return ActGenElement.eGenElementAction.SendKeys;
            }

            return ActGenElement.eGenElementAction.Wait;
        }

        //Returns LocatorType for HTML element on PL
        public static eLocateBy GetLocateBy(string LocateBy)
        {
            switch (LocateBy)
            {
                case "ByID":
                    return eLocateBy.ByID;

                case "ByName":
                    return eLocateBy.ByName;

                case "ByValue":
                    return eLocateBy.ByValue;

                case "ByXPath":
                    return eLocateBy.ByXPath;

                case "ByClassName":
                    return eLocateBy.ByClassName;
            }
            return eLocateBy.NA;
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
                            act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
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
            if (act is ActSwitchWindow)
            {
                if (((ActSwitchWindow)act).WaitTime >= 0)
                {
                    waitTime = ((ActSwitchWindow)act).WaitTime;
                }
            }
            else if(act is ActUIElement)
            {
                // adding to support actuielement switch window action synctime
                var syncTime = Convert.ToInt32(((ActUIElement)act).GetInputParamCalculatedValue(ActUIElement.Fields.SyncTime));
                if(syncTime >= 0)
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
                    if (BFound) return;
                    Thread.Sleep(500);
                }
            }
            if (BFound)
                return;//window found
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

                var list = ((IEnumerable<KeyValuePair<string, object>>)rc3).OrderBy(kp => Convert.ToInt32(kp.Key)).Select(kp => kp.Value).ToList();
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
            List<KeyValuePair<string, object>> rc3 = new List<KeyValuePair<string, object>>();
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
                if (act.LocateValue != "" && act.LocateValue != null)
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
                    if (act.GetInputParamCalculatedValue("Value").Trim().ToUpper() != "DEFAULT")
                    {
                        Driver.SwitchTo().Frame(act.GetInputParamCalculatedValue("Value"));
                        return;
                    }
                    else
                    {
                        Driver.SwitchTo().DefaultContent();
                        return;
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

        public void ActBrowserElementHandler(ActBrowserElement act)
        {
            switch (act.ControlAction)
            {
                case ActBrowserElement.eControlAction.Maximize:
                    Driver.Manage().Window.Maximize();
                    break;

                case ActBrowserElement.eControlAction.OpenURLNewTab:
                    OpenUrlNewTab();

                    if ((act.GetInputParamValue(ActBrowserElement.Fields.URLSrc) == ActBrowserElement.eURLSrc.UrlPOM.ToString()))
                    {
                        string POMGuid = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.PomGUID);
                        string POMUrl = "";
                        if (!string.IsNullOrEmpty(POMGuid))
                        {
                            ApplicationPOMModel SelectedPOM = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().Where(p => p.Guid.ToString() == POMGuid).FirstOrDefault();
                            if (SelectedPOM != null)
                            {
                                POMUrl = ValueExpression.Calculate(this.Environment, this.BusinessFlow, SelectedPOM.PageURL, null);             // SelectedPOM.PageURL;
                            }
                        }
                        GotoURL(act, POMUrl);
                    }
                    else
                    {
                        GotoURL(act, act.GetInputParamCalculatedValue("Value"));
                    }
                    break;

                case ActBrowserElement.eControlAction.GotoURL:

                    if ((act.GetInputParamValue(ActBrowserElement.Fields.GotoURLType) == ActBrowserElement.eGotoURLType.NewTab.ToString()))
                    {
                        OpenUrlNewTab();
                    }
                    else if ((act.GetInputParamValue(ActBrowserElement.Fields.GotoURLType) == ActBrowserElement.eGotoURLType.NewWindow.ToString()))
                    {
                        IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Driver;
                        javaScriptExecutor.ExecuteScript("newwindow=window.open('about:blank','newWindow','height=250,width=350');if (window.focus) { newwindow.focus()}return false; ");
                        Driver.SwitchTo().Window(Driver.WindowHandles[Driver.WindowHandles.Count - 1]);
                        Driver.Manage().Window.Maximize();
                    }

                    if ((act.GetInputParamValue(ActBrowserElement.Fields.URLSrc) == ActBrowserElement.eURLSrc.UrlPOM.ToString()))
                    {
                        string POMGuid = act.GetInputParamCalculatedValue(ActBrowserElement.Fields.PomGUID);
                        string POMUrl = "";
                        if (!string.IsNullOrEmpty(POMGuid))
                        {
                            ApplicationPOMModel SelectedPOM = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>().Where(p => p.Guid.ToString() == POMGuid).FirstOrDefault();
                            POMUrl = ValueExpression.Calculate(this.Environment, this.BusinessFlow, SelectedPOM.PageURL, null);     // SelectedPOM?.PageURL;
                        }
                        GotoURL(act, POMUrl);
                    }
                    else
                    {
                        GotoURL(act, act.GetInputParamCalculatedValue("Value"));
                    }
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
                        act.AddOrUpdateReturnParamActual("Actual", title);
                    else
                        act.AddOrUpdateReturnParamActual("Actual", "");
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
                    Uri url = new Uri(Driver.Url);
                    act.AddOrUpdateReturnParamActual("Host", url.Host);
                    act.AddOrUpdateReturnParamActual("Path", url.LocalPath);
                    act.AddOrUpdateReturnParamActual("PathWithQuery", url.PathAndQuery);
                    break;
                case ActBrowserElement.eControlAction.InjectJS:
                    AddJavaScriptToPage(act.ActInputValues[0].Value);
                    break;
                case ActBrowserElement.eControlAction.RunJavaScript:
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
                            act.AddOrUpdateReturnParamActual("Actual", a.ToString());
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
                    Driver.Quit();
                    break;
                case ActBrowserElement.eControlAction.GetBrowserLog:

                    String scriptToExecute = "var performance = window.performance || window.mozPerformance || window.msPerformance || window.webkitPerformance || {}; var network = performance.getEntries() || {}; return network;";
                    var networkLogs = ((IJavaScriptExecutor)Driver).ExecuteScript(scriptToExecute) as ReadOnlyCollection<object>;

                    foreach (var item in networkLogs)
                    {
                        Dictionary<string, object> dict = item as Dictionary<string, object>;
                        if (dict != null)
                        {
                            if(dict.ContainsKey("name"))
                            {
                                var urlArray = dict.Where(x => x.Key == "name").FirstOrDefault().Value.ToString().Split('/');

                                var urlString = string.Empty;
                                if (urlArray.Length>0)
                                {
                                    urlString = urlArray[urlArray.Length - 1];
                                    if(string.IsNullOrEmpty(urlString) && urlArray.Length>1)
                                    {
                                        urlString = urlArray[urlArray.Length - 2];
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
                            act.AddOrUpdateReturnParamActual("Actual", AlertBoxText);
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

                default:
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }
        }

        private void OpenUrlNewTab()
        {
            IJavaScriptExecutor javaScriptExecutor = (IJavaScriptExecutor)Driver;
            javaScriptExecutor.ExecuteScript("window.open();");
            Driver.SwitchTo().Window(Driver.WindowHandles[Driver.WindowHandles.Count - 1]);
        }

        public string GetSearchedWinTitle(Act act)
        {
            string searchedWinTitle = string.Empty;

            if (act is ActUIElement)
            {
                var actUIElement = (ActUIElement)act;
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
                    searchedWinTitle = act.LocateValueCalculated;
                else
                    searchedWinTitle = act.ValueForDriver;
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
                    Driver.Quit();
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
                e = LocateElement(act);
                if (e == null && act.ElementAction != ActUIElement.eElementAction.IsVisible)
                {
                    act.Error += "Element not found: " + act.ElementLocateBy + "=" + act.ElementLocateValueForDriver;
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
                                act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("href"));
                            else
                                act.AddOrUpdateReturnParamActual("Actual", "");
                        }
                        else
                        {
                            act.AddOrUpdateReturnParamActual("Actual", GetElementValue(e));
                        }
                        break;

                    case ActUIElement.eElementAction.IsVisible:
                        if(e!=null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.Displayed.ToString());
                        }
                        else
                        {
                            act.ExInfo += "Element not found: " + act.ElementLocateBy + "=" + act.ElementLocateValueForDriver;
                            act.AddOrUpdateReturnParamActual("Actual","False");
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
                                e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                            }
                            catch (InvalidOperationException ex)
                            {
                                ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                                Reporter.ToLog(eLogLevel.ERROR, "Exception occured when HandleActUIElement");
                            }
                        }
                        else
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.GetInputParamCalculatedValue("Value"));
                        break;

                    case ActUIElement.eElementAction.SendKeys:
                        e.SendKeys(GetKeyName(act.GetInputParamCalculatedValue("Value")));
                        break;

                    case ActUIElement.eElementAction.Submit:
                        e.SendKeys("");
                        e.Submit();
                        break;

                    case ActUIElement.eElementAction.GetSize:
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("size").ToString());
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
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", e);
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
                                        a = ((IJavaScriptExecutor)Driver).ExecuteScript(script, e);
                                }
                                else
                                {
                                    a = ((IJavaScriptExecutor)Driver).ExecuteScript(script);
                                }

                                if (a != null)
                                    act.AddOrUpdateReturnParamActual("Actual", a.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Failed to run the JavaScript: '" + script + "', Error: '" + ex.Message + "', if element need to be embedded in the script so make sure you use the 'arguments[0]' place holder for it.";
                        }
                        break;


                    case ActUIElement.eElementAction.DoubleClick:
                        OpenQA.Selenium.Interactions.Actions actionDoubleClick = new OpenQA.Selenium.Interactions.Actions(Driver);
                        actionDoubleClick.Click(e).Click(e).Build().Perform();
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
                            act.Error = "Failed to count number of elements for - " + act.LocateBy + " " + act.LocateValueCalculated;
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
                                string script = Properties.Resources.Html5DragAndDrop;
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

            ActUIElement.eElementAction clickType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ClickType).ToString(), out clickType) == false)
            {
                act.Error = "Unknown Click Type";
                return false;
            }

            // Validation Element locate by:
            eLocateBy validationElementLocateby;
            if (Enum.TryParse<eLocateBy>(act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocateBy).ToString(), out validationElementLocateby) == false)
            {
                act.Error = "Unknown Validation Element Locate By";
                return false;
            }

            //Validation Element Locator Value:
            string validationElementLocatorValue = act.GetInputParamValue(ActUIElement.Fields.ValidationElementLocatorValue.ToString());

            //Validation Type:
            ActUIElement.eElementAction validationType;
            if (Enum.TryParse<ActUIElement.eElementAction>(act.GetInputParamValue(ActUIElement.Fields.ValidationType).ToString(), out validationType) == false)
            {
                act.Error = "Unknown Validation Type";
                return false;
            }

            //Loop through clicks flag check:
            bool ClickLoop = false;
            if ((act.GetInputParamValue(ActUIElement.Fields.LoopThroughClicks).ToString()) == "True")
                ClickLoop = true;

            //Do click:
            DoUIElementClick(clickType, clickElement);
            //check if validation element exists
            IWebElement elmToValidate = LocateElement(act, true, validationElementLocateby.ToString(), validationElementLocatorValue);

            if (elmToValidate != null)
                return true;
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
                            DoUIElementClick((ActUIElement.eElementAction)singleclick, clickElement);
                            elmToValidate = LocateElement(act, true, validationElementLocateby.ToString(), validationElementLocatorValue);
                            if (elmToValidate != null)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            act.Error = "Error:  Validation Element not found - " + validationElementLocateby + " Using Value : " + validationElementLocatorValue;
            return false;
        }

        public Bitmap GetScreenShot()
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
                using (var ms = new MemoryStream(ss.AsByteArray))
                {
                    using (MemoryStream outStream = new MemoryStream())
                    {
                        BitmapEncoder enc = new BmpBitmapEncoder();
                        enc.Frames.Add(BitmapFrame.Create(ms));
                        enc.Save(outStream);
                        Bitmap bitmap = new Bitmap(outStream);
                        return new Bitmap(bitmap);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Selenium WebDriver Browser Page Screenshot", ex);
                return null;
            }
        }

        Bitmap IVisualTestingDriver.GetScreenShot(Tuple<int, int> setScreenSize = null)
        {
            if (setScreenSize != null)
            {
                try
                {
                    //Driver.Manage().Window.Position = new System.Drawing.Point(0, 0);
                    //System.Drawing.Size originalSize = Driver.Manage().Window.Size;
                    Driver.Manage().Window.Size = new System.Drawing.Size(setScreenSize.Item1, setScreenSize.Item2);
                    //Bitmap screenShot = GetScreenShot();
                    //Driver.Manage().Window.Size = originalSize;
                    //return screenShot;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to set browser screen size before taking screen shot", ex);
                    return GetScreenShot();
                }
            }

            return GetScreenShot();
        }

        VisualElementsInfo IVisualTestingDriver.GetVisualElementsInfo()
        {
            VisualElementsInfo VEI = new VisualElementsInfo();

            VEI.Bitmap = GetScreenShot();

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
                EI.XPath = GenerateXpathForIWebElement((IWebElement)EI.ElementObject, "");

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
            List<string> importantProperties = new List<string>();
            importantProperties.Add("SeleniumDriver");
            importantProperties.Add("Web");
            mXPathHelper = new XPathHelper(this, importantProperties);
        }

        XPathHelper IXPath.GetXPathHelper(ElementInfo info)
        {
            return mXPathHelper;
        }

        ElementInfo IXPath.GetRootElement()
        {
            ElementInfo RootEI = new ElementInfo();
            RootEI.ElementTitle = "html/body";
            RootEI.ElementType = "root";
            RootEI.Value = string.Empty;
            RootEI.Path = string.Empty;
            RootEI.XPath = "html/body";
            return RootEI;
        }

        ElementInfo IXPath.UseRootElement()
        {
            Driver.SwitchTo().DefaultContent();
            return GetRootElement();
        }

        ElementInfo IXPath.GetElementParent(ElementInfo ElementInfo)
        {
            ElementInfo parentEI = null;
            IWebElement parentElementIWebElement = null;
            HtmlNode parentElementHtmlNode = null;
            if (((HTMLElementInfo)ElementInfo).HTMLElementObject != null)
            {
                parentElementHtmlNode = ((HTMLElementInfo)ElementInfo).HTMLElementObject.ParentNode;
                parentEI = allReadElem.Find(el => el is HTMLElementInfo && ((HTMLElementInfo)el).HTMLElementObject != null && ((HTMLElementInfo)el).HTMLElementObject.Equals(parentElementHtmlNode));
            }
            else
            {
                if (ElementInfo.ElementObject == null)
                    ElementInfo.ElementObject = Driver.FindElement(By.XPath(ElementInfo.XPath));

                parentElementIWebElement = ((IWebElement)ElementInfo.ElementObject).FindElement(By.XPath(".."));
                parentEI = allReadElem.Find(el => el.ElementObject != null && el.ElementObject.Equals(parentElementIWebElement));
            }

            if (parentEI != null)
            {
                return parentEI;
            }

            IWebElement parentElementObject = Driver.FindElement(By.XPath(parentElementHtmlNode.XPath));

            HTMLElementInfo foundElemntInfo = new HTMLElementInfo();
            foundElemntInfo.ElementObject = parentElementObject;
            foundElemntInfo.HTMLElementObject = parentElementHtmlNode;
            ((IWindowExplorer)this).LearnElementInfoDetails(foundElemntInfo);

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
            else if (EI is HTMLElementInfo && ((HTMLElementInfo)EI).HTMLElementObject != null)
            {
                return (((HTMLElementInfo)EI).HTMLElementObject).Name;
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

            if (ElementInfo.ElementObject == null)
            {
                ElementInfo.ElementObject = Driver.FindElement(By.XPath(ElementInfo.XPath));
            }

            if (ElementInfo.ElementObject != null)
            {
                elementProperty = ((IWebElement)ElementInfo.ElementObject).GetAttribute(PropertyName);
            }
            return elementProperty;
        }

        List<ElementInfo> IXPath.GetElementChildren(ElementInfo ElementInfo)
        {
            List<ElementInfo> list = new List<ElementInfo>();
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
                                        allTestsPassed = false;
                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.Less:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue < elementvalue)
                                        allTestsPassed = false;
                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.More:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue > elementvalue)
                                        returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
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
            List<ElementInfo> list = new List<ElementInfo>();
            list = FindAll(ElementInfo, conditions);
            Driver.SwitchTo().DefaultContent();
            CurrentFrame = string.Empty;
            return list;
        }

        private List<ElementInfo> FindAll(ElementInfo ElementInfo, List<XpathPropertyCondition> conditions)
        {
            List<ElementInfo> list = new List<ElementInfo>();
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
                                        allTestsPassed = false;
                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.Less:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue < elementvalue)
                                        allTestsPassed = false;
                                    break;
                                case XpathPropertyCondition.XpathConditionOperator.More:
                                    elementInfoValue = Convert.ToInt32(ElementInfo.Value);
                                    elementvalue = Convert.ToInt32(value);
                                    if (elementInfoValue > elementvalue)
                                        returnElementInfo = GetElementInfoWithIWebElementWithXpath(el, "");
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
            if (childrenElements[childrenElements.Count - 1].Equals(childElement))
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

        bool IWindowExplorer.TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
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
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);

                foreach (ElementLocator el in activesElementLocators)
                {
                    if (LocateElementByLocator(el, true) != null)
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

                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));

                if (activesElementLocators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed).Count() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                foreach (ElementLocator el in EI.Locators.Where(x => x.LocateStatus == ElementLocator.eLocateStatus.Pending).ToList())
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Unknown;
                }
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));
                mIsDriverBusy = false;
            }
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
                        IWebElement e = LocateElementByLocators(EI.Locators);
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
                Driver.Manage().Timeouts().ImplicitWait = (TimeSpan.FromSeconds((int)ImplicitWait));
                Driver.SwitchTo().DefaultContent();
                mIsDriverBusy = false;
            }
        }

        public ElementInfo GetMatchingElement(ElementInfo element, ObservableList<ElementInfo> existingElemnts)
        {
            //try using online IWebElement Objects comparison
            ElementInfo OriginalElementInfo = existingElemnts.Where(x => (x.ElementObject != null) && (element.ElementObject != null) && (x.ElementObject.ToString() == element.ElementObject.ToString())).FirstOrDefault();//comparing IWebElement ID's


            if (OriginalElementInfo == null)
            {
                //try by type and Xpath comparison
                OriginalElementInfo = existingElemnts.Where(x => (x.ElementTypeEnum == element.ElementTypeEnum)
                                                                    && (x.XPath == element.XPath)
                                                                    && (x.Path == element.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(element.Path)))
                                                                    && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                        || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                            && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == element.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                            )
                                                                        )
                                                                  ).FirstOrDefault();
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
            return GenerateXpathForIWebElement((IWebElement)EI.ElementObject, EI.Path);
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }
    }
}
