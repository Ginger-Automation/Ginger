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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.POMModelLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Mobile;
using Amdocs.Ginger.CoreNET.ActionsLib.UI.Web;
using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using Amdocs.Ginger.Plugin.Core;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.VisualTesting;
using GingerCore.Drivers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUglify.Html;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Android.Enums;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using AppiumInteractions = OpenQA.Selenium.Appium.Interactions;
using PointerInputDevice = OpenQA.Selenium.Interactions.PointerInputDevice;

namespace Amdocs.Ginger.CoreNET
{
    public class GenericAppiumDriver : DriverBase, IWindowExplorer, IRecord, IDriverWindow, IMobileDriverWindow, IVisualTestingDriver
    {
        public override ePlatformType Platform { get { return ePlatformType.Mobile; } }

        public override string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA, IEnumerable<DriverConfigParam> driverConfigParams = null)
        {
            return "AppiumDriverEditPage";
        }

        public string GetDriverWindowName(Agent.eDriverType driverSubType = Agent.eDriverType.NA)
        {
            return "MobileDriverWindow";
        }

        //Mobile Driver Configurations
        [UserConfigured]
        [UserConfiguredDefault(@"http://127.0.0.1:4723/")]
        [UserConfiguredDescription("Full Appium 2.x server address including port if needed, default address is: 'https://ServerIP:Port/'")]
        public String AppiumServer { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Proxy to be used for Appium server connection, need to include the server and port")]
        public String Proxy { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Default")]
        [UserConfiguredDescription("Screen Scale Factor Correction for X coordinate, needed for fixing screen mouse click/point accuracy (decimal number)")]
        public String ScreenScaleFactorCorrectionX { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Default")]
        [UserConfiguredDescription("Screen Scale Factor Correction for Y coordinate, needed for fixing screen mouse click/point accuracy (decimal number)")]
        public String ScreenScaleFactorCorrectionY { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Auto")]
        [UserConfiguredDescription("Define the Height to set for the mobile device screenshot")]
        public String ScreenshotHeight { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Auto")]
        [UserConfiguredDescription("Define the Width to set for the mobile device screenshot")]
        public String ScreenshotWidth { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(eDevicePlatformType))]
        [UserConfiguredDefault("Android")]
        [UserConfiguredDescription("Device platform type 'Android' or 'iOS'")]
        public eDevicePlatformType DevicePlatformType { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(eDeviceSource))]
        [UserConfiguredDefault("LocalAppium")]
        [UserConfiguredDescription("Device Source is Local Appium or Devices Lab")]
        public eDeviceSource DeviceSource { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(eAppType))]
        [UserConfiguredDefault("NativeHybride")]
        [UserConfiguredDescription("The tested application type 'NativeHybride' or 'Web'")]
        public eAppType AppType { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Determine if the Ginger device window will be loaded with the Agent")]
        public bool LoadDeviceWindow { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(eAutoScreenshotRefreshMode))]
        [UserConfiguredDefault("Live")]
        [UserConfiguredDescription("Determine if the Ginger device screen image will be refresh automatically during use")]
        public eAutoScreenshotRefreshMode DeviceAutoScreenshotRefreshMode { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Determine if auto set the default capabilities based on OS and application type selection")]
        public bool AutoSetCapabilities { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set UFTM Server capabilities autumatically")]
        public bool UFTMServerCapabilities { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("false")]
        [UserConfiguredDescription("Set UFTM simulations automatically")]
        public bool UFTMSupportSimulationsCapabiliy { get; set; }

        [UserConfigured]
        [UserConfiguredMultiValues]
        [UserConfiguredDescription("Appium capabilities")]
        public ObservableList<DriverConfigParam> AppiumCapabilities { get; set; }

        protected IWebDriver webDriver;


        bool mIsDeviceConnected = false;
        string mDefaultURL = null;

        public bool IsDeviceConnected
        {
            get => mIsDeviceConnected;
            set => mIsDeviceConnected = value;
        }

        public bool IsUftLabDevice
        {
            get
            {
                if (DeviceSource == eDeviceSource.MicroFoucsUFTMLab)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        RestClient restClient;

        public bool ShowWindow
        {
            get => LoadDeviceWindow;
        }

        public override ePomElementCategory? PomCategory
        {
            get
            {
                if (AppType == eAppType.NativeHybride)
                {
                    return DevicePlatformType switch
                    {
                        eDevicePlatformType.iOS => (ePomElementCategory?)ePomElementCategory.iOS,
                        _ => (ePomElementCategory?)ePomElementCategory.Android,
                    };
                }
                else
                {
                    return ePomElementCategory.Web;
                }
            }

            set => base.PomCategory = value;
        }

        private AppiumDriver Driver;//appium 
        private SeleniumDriver mSeleniumDriver;//selenium 
        public override bool StopProcess
        {
            get
            {
                if (AppType == eAppType.Web)
                {
                    return mSeleniumDriver.StopProcess;
                }
                else
                {
                    return base.StopProcess;
                }
            }
            set
            {
                if (AppType == eAppType.Web)
                {
                    mSeleniumDriver.StopProcess = value;
                }
                else
                {
                    base.StopProcess = value;
                }
            }
        }

        public GenericAppiumDriver(BusinessFlow BF)
        {
            BusinessFlow = BF;
        }

        public override void StartDriver()
        {
            mIsDeviceConnected = ConnectToAppium();
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        public bool ConnectToAppium()
        {
            try
            {
                Uri serverUri = null;
                try
                {
                    serverUri = new Uri(AppiumServer);
                }
                catch (Exception ex)
                {
                    string error = string.Format("Appium URL configured is not valid, URL: '{0}', Error: '{1}'", AppiumServer, ex.Message);
                    Reporter.ToLog(eLogLevel.ERROR, error, ex);
                    ErrorMessageFromDriver = error;
                    return false;
                }

                //Setting capabilities                                
                DriverOptions driverOptions = this.GetCapabilities();

                //creating driver
                switch (DevicePlatformType)
                {
                    case eDevicePlatformType.Android:
                        if (string.IsNullOrEmpty(Proxy))
                        {
                            Driver = new AndroidDriver(serverUri, driverOptions, TimeSpan.FromSeconds(DriverLoadWaitingTime));
                        }
                        else
                        {
                            Driver = new AndroidDriver(new HttpCommandExecutor(serverUri, TimeSpan.FromSeconds(DriverLoadWaitingTime)) { Proxy = new WebProxy(this.Proxy) }, driverOptions);
                        }
                        break;
                    case eDevicePlatformType.iOS:
                        if (string.IsNullOrEmpty(Proxy))
                        {
                            Driver = new IOSDriver(serverUri, driverOptions, TimeSpan.FromSeconds(DriverLoadWaitingTime));
                        }
                        else
                        {
                            Driver = new IOSDriver(new HttpCommandExecutor(serverUri, TimeSpan.FromSeconds(DriverLoadWaitingTime)) { Proxy = new WebProxy(this.Proxy) }, driverOptions);
                        }
                        break;
                }

                if (String.IsNullOrEmpty(Driver.SessionId.ToString()))
                {
                    string error = "Failed to start Appium session, created Driver not seems to be valid (it SessionId is null), please validate the Appium server URL and capabilities";
                    Reporter.ToLog(eLogLevel.ERROR, error);
                    ErrorMessageFromDriver = error;
                    return false;
                }

                if (Driver.Capabilities.HasCapability("message") && Driver.Capabilities.GetCapability("message").ToString() == "Could not find available device")
                {
                    string error = string.Format("Failed to start Appium session.{0}Error: Mobile device is already in use. Please close all other sessions and try again.", System.Environment.NewLine);
                    Reporter.ToLog(eLogLevel.ERROR, error);
                    ErrorMessageFromDriver = error;
                    return false;
                }

                if (DevicePlatformType == eDevicePlatformType.Android || DevicePlatformType == eDevicePlatformType.Android)
                {

                }
                else
                {

                }
                mSeleniumDriver = new SeleniumDriver(Driver)//used for running regular Selenium actions
                {
                    isAppiumSession = true,
                    BusinessFlow = this.BusinessFlow,
                    PomCategory = this.PomCategory
                };
                mSeleniumDriver.StopProcess = this.StopProcess;

                if (AppType == eAppType.Web && mDefaultURL != null)
                {
                    try
                    {
                        Driver.Navigate().GoToUrl(mDefaultURL);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to load default mobile web app URL, please validate the URL is valid", ex);
                    }
                }

                //Pull device screen sizes for calculations
                CalculateMobileDeviceScreenSizes();

                return true;

            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to start Appium session.{0}Error: '{1}'", System.Environment.NewLine, ex.Message);
                Reporter.ToLog(eLogLevel.ERROR, error, ex);
                ErrorMessageFromDriver = error;
                return false;
            }
        }

        private DriverOptions GetCapabilities()
        {
            //see http://appium.io/slate/en/master/?csharp#appium-server-capabilities for full list of capabilities values
            mDefaultURL = null;
            AppiumOptions driverOptions = new AppiumOptions();

            //User customized capabilities
            foreach (DriverConfigParam UserCapability in AppiumCapabilities)
            {
                try
                {
                    if (String.IsNullOrWhiteSpace(UserCapability.Parameter) || String.IsNullOrWhiteSpace(UserCapability.Value))
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("The Appium Capability '{0}'='{1}' is not valid, avoiding it.", UserCapability.Parameter, UserCapability.Value));
                        continue;
                    }

                    if (UserCapability.Parameter.ToLower().Trim() is "defaulturl" or "ginger:defaulturl")
                    {
                        mDefaultURL = UserCapability.Value;

                        continue;
                    }

                    bool boolValue;
                    int intValue = 0;
                    if (bool.TryParse(UserCapability.Value, out boolValue))
                    {
                        driverOptions.AddAdditionalAppiumOption(UserCapability.Parameter, boolValue);
                    }
                    else if (!isContainQuotationMarks(UserCapability) && int.TryParse(UserCapability.Value, out intValue))
                    {
                        driverOptions.AddAdditionalAppiumOption(UserCapability.Parameter, intValue);
                    }
                    else if (UserCapability.Value.Contains("{"))
                    {
                        try
                        {
                            JObject json = JObject.Parse(UserCapability.Value);
                            driverOptions.AddAdditionalAppiumOption(UserCapability.Parameter, json);//for Json value to work properly, need to convert it into specific object type like: json.ToObject<selector>());
                        }
                        catch (Exception)
                        {
                            driverOptions.AddAdditionalAppiumOption(UserCapability.Parameter, UserCapability.Value);
                        }
                    }
                    else
                    {
                        if (UserCapability.Parameter.Trim().ToLower() == "automationName".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:automationName".ToLower())
                        {
                            driverOptions.AutomationName = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "platformName".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:platformName".ToLower())
                        {
                            driverOptions.PlatformName = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "platformVersion".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:platformVersion".ToLower())
                        {
                            driverOptions.PlatformVersion = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "deviceName".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:deviceName".ToLower())
                        {
                            driverOptions.DeviceName = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "app".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:app".ToLower())
                        {
                            driverOptions.App = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "browserName".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:browserName".ToLower())
                        {
                            driverOptions.BrowserName = UserCapability.Value;
                        }
                        else if (UserCapability.Parameter.Trim().ToLower() == "browserVersion".ToLower() || UserCapability.Parameter.Trim().ToLower() == "appium:browserVersion".ToLower())
                        {
                            driverOptions.BrowserVersion = UserCapability.Value;
                        }
                        else
                        {
                            driverOptions.AddAdditionalAppiumOption(UserCapability.Parameter, UserCapability.Value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to set Appium capability '{0}'='{1}'", UserCapability.Parameter, UserCapability.Value), ex);
                }
            }

            return driverOptions;
        }

        private bool isContainQuotationMarks(DriverConfigParam capability)
        {
            if (capability.Value[0] == '\"' && capability.Value[^1] == '\"')
            {
                capability.Value = capability.Value[1..^1];
                return true;
            }
            return false;
        }


        public override void CloseDriver()
        {
            if (Driver != null)
            {
                Driver.Quit();
            }
            mIsDeviceConnected = false;
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        public IWebElement LocateElement(Act act)
        {
            //need to override regular selenium driver locator if needed, 
            //if not then to run the regular selenium driver locator for it to avoid duplication   

            if (AppType == eAppType.Web)
            {
                return mSeleniumDriver.LocateElement(act);
            }

            eLocateBy locateBy = act is ActUIElement ? (act as ActUIElement).ElementLocateBy : act.LocateBy;
            IWebElement elem = null;
            string locateValue = act is ActUIElement ? (act as ActUIElement).ElementLocateValue : act.LocateValue;
            switch (locateBy)
            {
                case eLocateBy.ByResourceID:
                    elem = Driver.FindElement(By.Id(locateValue));
                    break;

                case eLocateBy.ByRelXPath:
                case eLocateBy.ByXPath:
                    elem = Driver.FindElement(By.XPath(locateValue));
                    break;

                default:
                    return mSeleniumDriver.LocateElement(act);
            }

            return elem;
        }

        public override Act GetCurrentElement()
        {
            return mSeleniumDriver.GetCurrentElement();
        }

        public override void RunAction(Act act)
        {
            Type actionType = act.GetType();

            try
            {
                //Generic
                if (actionType == typeof(ActScreenShot))
                {
                    ActScreenShotHandler(act);
                    return;
                }

                if (actionType == typeof(ActMobileDevice))
                {
                    MobileDeviceActionHandler((ActMobileDevice)act);
                    return;
                }

                //Web
                if (AppType == eAppType.Web)//Keep here to make sure Web handling will be done by Selenium driver
                {
                    mSeleniumDriver.RunAction(act);
                    return;
                }

                //Mobile
                if (actionType == typeof(ActAccessibilityTesting))
                {
                    ActAccessibilityTestingHandler((ActAccessibilityTesting)act);
                    return;
                }

                //Naitive/Hybrid
                if (actionType == typeof(ActUIElement))
                {
                    UIElementActionHandler((ActUIElement)act);
                    return;
                }

                if (actionType == typeof(ActSmartSync))
                {
                    mSeleniumDriver.SmartSyncHandler((ActSmartSync)act);
                    return;
                }

                //Legacy
                if (actionType == typeof(ActGenElement))
                {
                    GenElementHandler((ActGenElement)act);
                    return;
                }

                if (actionType == typeof(ActVisualTesting))
                {
                    ((ActVisualTesting)act).Execute(this);
                    return;
                }

                act.Error = "Mobile Agent configuration do not support this Action.";
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            catch (Exception ex)
            {
                act.Error = ex.Message;
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            finally
            {
                OnDriverMessage(eDriverMessageType.ActionPerformed);
            }
        }

        // Add this method to handle ActAccessibilityTesting for mobile
        private void ActAccessibilityTestingHandler(ActAccessibilityTesting act)
        {
            try
            {
                IWebElement elementInfo = null;

                if (act.GetAccessibilityTarget() == ActAccessibilityTesting.eTarget.Element)
                {
                    elementInfo = LocateElement(act);  // Locate specific element
                }
                else if (act.GetAccessibilityTarget() == ActAccessibilityTesting.eTarget.Page)
                {

                    // For 'Page', use the root element to analyze the full screen
                    elementInfo = (AppiumElement)Driver.FindElement(By.ClassName("android.widget.FrameLayout"));
                }

                if (elementInfo != null)
                {
                    act.AnalyzerMobileAccessibility(Driver, elementInfo);
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
                else
                {
                    act.Error = "Element not found.";
                    act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                act.Error = ex.Message;
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
        }

        private void UIElementActionHandler(ActUIElement act)
        {
            try
            {
                IWebElement e = null;
                //adjusting operations to fit native apps
                switch (act.ElementAction)
                {
                    case ActUIElement.eElementAction.JavaScriptClick:
                    case ActUIElement.eElementAction.Submit:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            e.Click();
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.SetValue:
                    case ActUIElement.eElementAction.SetText:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.GetText:
                    case ActUIElement.eElementAction.GetFont:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            /// As text attribute does not exist on iOS devices
                            if (DevicePlatformType == eDevicePlatformType.iOS)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("value"));
                            }
                            else
                            {
                                act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("text"));
                            }
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.GetTextLength:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            /// As text attribute does not exist on iOS devices
                            if (DevicePlatformType == eDevicePlatformType.iOS)
                            {
                                act.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("value").Length).ToString());
                            }
                            else
                            {
                                act.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("text").Length).ToString());
                            }
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.Select:
                    case ActUIElement.eElementAction.SelectByIndex:
                    case ActUIElement.eElementAction.SelectByText:
                        act.Error = "Operation not supported for this mobile OS or application type. Please use 'Click' operation.";
                        break;

                    case ActUIElement.eElementAction.IsValuePopulated:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            switch (act.ElementType)
                            {
                                case eElementType.ComboBox:
                                    OpenQA.Selenium.Support.UI.SelectElement seIsPrepopulated = new OpenQA.Selenium.Support.UI.SelectElement(e);
                                    act.AddOrUpdateReturnParamActual("Actual", (seIsPrepopulated.SelectedOption.ToString().Trim() != "").ToString());
                                    break;
                                case eElementType.TextBox:
                                    act.AddOrUpdateReturnParamActual("Actual", (!string.IsNullOrEmpty(e.Text)).ToString());
                                    break;
                            }
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.GetSize:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("contentSize").ToString());
                        }
                        else
                        {
                            act.Error = "Element not found.";
                        }
                        break;

                    case ActUIElement.eElementAction.ScrollToElement:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            int element_X = e.Location.X;
                            int element_Y = e.Location.Y;
                            int numberOfMaxLoops = (element_Y - 500) / 200;
                            while (!e.Displayed && numberOfMaxLoops > 0)
                            {
                                SwipeByXY(element_X, 500, element_X, 300, TimeSpan.FromMilliseconds(200));
                                numberOfMaxLoops--;
                            }
                        }
                        else
                        {
                            int numberOfMaxLoops = 12;
                            while (numberOfMaxLoops > 0)
                            {
                                SwipeByXY(500, 500, 500, 300, TimeSpan.FromMilliseconds(200));
                                e = LocateElement(act);
                                if (e != null && e.Displayed)
                                {
                                    break;
                                }
                                numberOfMaxLoops--;
                            }
                        }
                        break;

                    case ActUIElement.eElementAction.GetXY:
                        PullXandYofElement(act);
                        break;

                    case ActUIElement.eElementAction.ClickXY:
                        if (string.IsNullOrEmpty(act.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate)) || string.IsNullOrEmpty(act.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate)))
                        {
                            //get X and Y from element
                            if (PullXandYofElement(act))
                            {
                                TapXY(Convert.ToInt32(act.GetReturnParam("X")), Convert.ToInt32(act.GetReturnParam("Y")));
                            }
                        }
                        else
                        {
                            TapXY(Convert.ToInt32(act.GetInputParamCalculatedValue(ActUIElement.Fields.XCoordinate)), Convert.ToInt32(act.GetInputParamCalculatedValue(ActUIElement.Fields.YCoordinate)));
                        }
                        break;

                    default:
                        mSeleniumDriver.HandleActUIElement(act);
                        break;
                }
            }
            catch (Exception ex)
            {
                act.Error = ex.Message;
            }
        }

        private bool PullXandYofElement(ActUIElement act)
        {
            IWebElement e = null;
            e = LocateElement(act);
            if (e != null)
            {
                act.AddOrUpdateReturnParamActual("X", e.Location.X.ToString());
                act.AddOrUpdateReturnParamActual("Y", e.Location.Y.ToString());
                return true;
            }
            else
            {
                act.Error = "Element not found.";
                return false;
            }
        }


        /// <summary>
        /// Legacy Support
        /// </summary>
        /// <param name="act"></param>
        private void GenElementHandler(ActGenElement act)
        {
            try
            {
                IWebElement e;
                long x = 0, y = 0;

                switch (act.GenElementAction)
                {
                    //need to override regular selenium driver actions only if needed, 
                    //if not then to run the regular selenium driver actions handler for it to avoid duplication

                    case ActGenElement.eGenElementAction.Click:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            e.Click();
                        }
                        else if (act.LocateBy == eLocateBy.ByXY)
                        {
                            try
                            {
                                x = Convert.ToInt64(act.LocateValueCalculated.Split(',')[0]);
                                y = Convert.ToInt64(act.LocateValueCalculated.Split(',')[1]);
                            }
                            catch { x = 0; y = 0; }
                            TapXY((int)x, (int)y);
                        }
                        else
                        {
                            act.Error = "Error: Element not found: '" + act.LocateBy + "'- '" + act.LocateValueCalculated + "'";
                        }
                        break;

                    case ActGenElement.eGenElementAction.TapElement:
                        try
                        {
                            e = LocateElement(act);
                            TapElement(e, 1, 1);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;


                    case ActGenElement.eGenElementAction.DoubleTapElement:
                        try
                        {
                            e = LocateElement(act);
                            DoubleTapElement(e, 1, 1);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;

                    case ActGenElement.eGenElementAction.PressElement:
                        try
                        {
                            e = LocateElement(act);
                            PressElement(e, 1, 1, TimeSpan.FromSeconds(1));
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;

                    case ActGenElement.eGenElementAction.SetValue:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            e.Clear();
                            //make sure value was cleared- trying to handle clear issue in WebViews
                            try
                            {
                                //TODO: Need to add a flag in the action for this case, as sometimes the value is clear but show text under like 'Search, or say "OK Google".
                                //Wasting time when not needed
                                string elemntContent = e.Text; //.GetAttribute("name");
                                if (string.IsNullOrEmpty(elemntContent) == false)
                                {
                                    for (int indx = 1; indx <= elemntContent.Length; indx++)
                                    {
                                        //Driver.KeyEvent(22);//"KEYCODE_DPAD_RIGHT"- move marker to right
                                        ((AndroidDriver)Driver).PressKeyCode(22);
                                        //Driver.KeyEvent(67);//"KEYCODE_DEL"- delete 1 character
                                        ((AndroidDriver)Driver).PressKeyCode(67);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, "Failed to clear element value", ex);
                            }
                            switch (DevicePlatformType)
                            {
                                case eDevicePlatformType.Android:
                                    e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                    break;
                                case eDevicePlatformType.iOS:
                                    e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                    break;
                            }
                        }
                        else
                        {
                            act.Error = "Error: Element not found: '" + act.LocateBy + "'- '" + act.LocateValueCalculated + "'";
                        }
                        break;

                    case ActGenElement.eGenElementAction.GetValue:
                    case ActGenElement.eGenElementAction.GetInnerText:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            act.AddOrUpdateReturnParamActual("Actual", e.Text);
                        }
                        else
                        {
                            act.Error = "Error: Element not found: '" + act.LocateBy + "'- '" + act.LocateValueCalculated + "'";
                            return;
                        }
                        break;

                    case ActGenElement.eGenElementAction.GetContexts:
                        int i = 0;
                        foreach (var c in Driver.Contexts)
                        {
                            act.AddOrUpdateReturnParamActual("Actual " + i, c.ToString());
                        }
                        break;

                    case ActGenElement.eGenElementAction.SetContext:
                        Driver.Context = act.GetInputParamCalculatedValue("Value");
                        break;

                    case ActGenElement.eGenElementAction.GetCustomAttribute:
                        e = LocateElement(act);
                        if (e != null)
                        {
                            string attribute = string.Empty;
                            try
                            {
                                attribute = e.GetAttribute(act.Value);
                            }
                            catch (Exception ex)
                            {
                                string value = act.Value.ToLower();
                                switch (value)
                                {
                                    case "content-desc":
                                        value = "name";
                                        break;
                                    case "resource-id":
                                        value = "resourceId";
                                        break;
                                    case "class":
                                        act.AddOrUpdateReturnParamActual("Actual", e.TagName);
                                        return;
                                    case "source":
                                        act.AddOrUpdateReturnParamActual("source", this.GetPageSource().Result);
                                        return;
                                    case "x":
                                    case "X":
                                        ActGenElement tempact = new ActGenElement();
                                        act.AddOrUpdateReturnParamActual("X", e.Location.X.ToString());
                                        return;
                                    case "y":
                                    case "Y":
                                        act.AddOrUpdateReturnParamActual("Y", e.Location.Y.ToString());
                                        return;
                                    default:
                                        if (act.LocateBy == eLocateBy.ByXPath)
                                        {
                                            XmlDocument PageSourceXml = new XmlDocument();
                                            PageSourceXml.LoadXml(this.GetPageSource().Result);
                                            XmlNode node = PageSourceXml.SelectSingleNode(act.LocateValueCalculated);

                                            foreach (XmlAttribute XA in node.Attributes)
                                            {
                                                if (XA.Name == act.ValueForDriver)
                                                {
                                                    act.AddOrUpdateReturnParamActual("Actual", XA.Value);
                                                    break;
                                                }
                                            }
                                        }
                                        return;
                                }
                                attribute = e.GetAttribute(value);
                                Reporter.ToLog(eLogLevel.ERROR, "Error happend", ex);
                            }
                            act.AddOrUpdateReturnParamActual("Actual", attribute);
                        }
                        else
                        {
                            act.Error = "Error: Element not found - " + act.LocateBy + "- '" + act.LocateValueCalculated + "'";
                            return;
                        }
                        break;
                    default:
                        mSeleniumDriver.GenElementHandler(act);
                        break;
                }
            }
            catch (Exception ex)
            {
                act.Error = ex.Message;
            }
        }

        private string GetAppPackage(ActMobileDevice act)
        {
            string appPackage = null;
            if (string.IsNullOrEmpty(act.ActionAppPackage.ValueForDriver) || act.ActionAppPackage.ValueForDriver.ToLower().Trim() == "default")
            {
                DriverConfigParam param = null;
                if (DevicePlatformType == eDevicePlatformType.Android)
                {
                    param = AppiumCapabilities.FirstOrDefault(x => x.Parameter is "appPackage" or "appium:appPackage");
                }
                else
                {
                    param = AppiumCapabilities.FirstOrDefault(x => x.Parameter is "bundleId" or "appium:bundleId");
                }
                if (param != null)
                {
                    appPackage = param.Value;
                }
                else
                {
                    act.Error = "App Package is not defined in the capabilities.";
                }
            }
            else if (string.IsNullOrEmpty(act.ActionAppPackage.ValueForDriver) || act.ActionAppPackage.ValueForDriver.ToLower().Trim() == "current")
            {
                appPackage = GetCurrentPackage();
                if (string.IsNullOrEmpty(appPackage))
                {
                    act.Error = "Failed to get the current application package.";
                }
            }
            else
            {
                appPackage = act.ActionAppPackage.ValueForDriver;
                if (string.IsNullOrEmpty(appPackage))
                {
                    act.Error = "App Package is not defined.";
                }
            }
            if (!string.IsNullOrEmpty(appPackage))
            {
                act.AddOrUpdateReturnParamActual("App Package", appPackage);
            }
            return appPackage;
        }

        private void MobileDeviceActionHandler(ActMobileDevice act)
        {
            string appPackage = string.Empty;
            try
            {
                switch (act.MobileDeviceAction)
                {

                    case ActMobileDevice.eMobileDeviceAction.TapXY:
                        TapXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.DoubleTapXY:
                        DoubleTapXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressXY:
                        PressXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver),
                                        TimeSpan.FromMilliseconds(Convert.ToInt32(act.PressDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                        PressXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver), TimeSpan.FromSeconds(3));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.DragXYXY:
                        DragAndDropByXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver),
                                           Convert.ToInt32(act.X2.ValueForDriver), Convert.ToInt32(act.Y2.ValueForDriver),
                                            TimeSpan.FromMilliseconds(Convert.ToDouble(act.PressDuration.ValueForDriver)),
                                            TimeSpan.FromMilliseconds(Convert.ToDouble(act.DragDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:
                        SwipeByXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver),
                                  Convert.ToInt32(act.X2.ValueForDriver), Convert.ToInt32(act.Y2.ValueForDriver),
                                  TimeSpan.FromMilliseconds(Convert.ToInt32(act.SwipeDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeDown:
                        SwipeScreen(eSwipeSide.Down, Convert.ToDouble(act.SwipeScale.ValueForDriver), TimeSpan.FromMilliseconds(Convert.ToInt32(act.SwipeDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeUp:
                        SwipeScreen(eSwipeSide.Up, Convert.ToDouble(act.SwipeScale.ValueForDriver), TimeSpan.FromMilliseconds(Convert.ToInt32(act.SwipeDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeLeft:
                        SwipeScreen(eSwipeSide.Left, Convert.ToDouble(act.SwipeScale.ValueForDriver), TimeSpan.FromMilliseconds(Convert.ToInt32(act.SwipeDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeRight:
                        SwipeScreen(eSwipeSide.Right, Convert.ToDouble(act.SwipeScale.ValueForDriver), TimeSpan.FromMilliseconds(Convert.ToInt32(act.SwipeDuration.ValueForDriver)));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressBackButton:
                        PerformBackButtonPress();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressHomeButton:
                        if (AppType == eAppType.NativeHybride)
                        {
                            PerformHomeButtonPress();
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressMenuButton:
                        if (AppType == eAppType.NativeHybride && DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformMenuButtonPress();
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.OpenCamera:
                        if (AppType == eAppType.NativeHybride && DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformOpenCamera();
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressVolumeUp:
                        PerformVolumeButtonPress(eVolumeOperation.Up);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressVolumeDown:
                        PerformVolumeButtonPress(eVolumeOperation.Down);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressKey:
                        if (DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformKeyPress(act.GetInputParamCalculatedValue(nameof(ActMobileDevice.MobilePressKey)));
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LongPressKey:
                        if (DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformLongKeyPress(act.GetInputParamCalculatedValue(nameof(ActMobileDevice.MobilePressKey)));
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.TakeScreenShot:
                        ActScreenShotHandler(act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetCurrentApplicationInfo:
                        act.AddOrUpdateReturnParamActual("Current Application Identifiers", GetURL());
                        break;

                    case ActMobileDevice.eMobileDeviceAction.OpenApp:
                        if (AppType == eAppType.NativeHybride)
                        {
                            appPackage = GetAppPackage(act);
                            if (!string.IsNullOrEmpty(appPackage))
                            {
                                Driver.ActivateApp(appPackage);
                            }
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.CloseApp:
                        if (AppType == eAppType.NativeHybride)
                        {
                            appPackage = GetAppPackage(act);
                            if (!string.IsNullOrEmpty(appPackage))
                            {
                                Driver.TerminateApp(appPackage);
                            }
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetPageSource:
                        act.AddOrUpdateReturnParamActual("Page Source", Driver.PageSource);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LockDevice:
                        PerformLockButtonPress(eLockOperation.Lock);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.UnlockDevice:
                        PerformLockButtonPress(eLockOperation.UnLock,act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceBattery:
                        AddReturnParamFromDict(GetDeviceBatteryInfo(), act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceCPUUsage:
                        AddReturnParamFromDict(GetDeviceCPUInfo(), act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceNetwork:
                        act.AddOrUpdateReturnParamActual("Device's network information", GetDeviceNetworkInfo()?.Result);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceRAMUsage:
                        AddReturnParamFromDict(GetDeviceMemoryInfo(), act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceGeneralInfo:
                        foreach (KeyValuePair<string, object> entry in GetDeviceGeneralInfo())
                        {
                            act.AddOrUpdateReturnParamActual(entry.Key, entry.Value.ToString());
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SimulatePhoto:
                        string photoString = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(act.GetOrCreateInputParam(nameof(act.SimulatedPhotoPath)).ValueForDriver);
                        string photoSimulationResponse = SimulatePhotoOrBarcode(photoString, "camera");
                        if (!string.IsNullOrEmpty(photoSimulationResponse) && photoSimulationResponse != "success")
                        {
                            act.Error = photoSimulationResponse;
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SimulateBarcode:
                        string barcodeString = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(act.GetOrCreateInputParam(nameof(act.SimulatedPhotoPath)).ValueForDriver);
                        string barcodeSimulationResponse = SimulatePhotoOrBarcode(barcodeString, "barcode");
                        if (!string.IsNullOrEmpty(barcodeSimulationResponse) && barcodeSimulationResponse != "success")
                        {
                            act.Error = barcodeSimulationResponse;
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SimulateBiometrics:
                    {
                        string biometricsAnswer = string.Empty;
                        switch (act.AuthResultSimulation)
                        {
                            case ActMobileDevice.eAuthResultSimulation.Success:
                            {
                                biometricsAnswer = BiometricSimulation(act.AuthResultSimulation.ToString(), "");
                                break;
                            }
                            case ActMobileDevice.eAuthResultSimulation.Failure:
                            {
                                biometricsAnswer = BiometricSimulation(act.AuthResultSimulation.ToString(), act.AuthResultDetailsFailureSimulation.ToString());
                                break;
                            }
                            case ActMobileDevice.eAuthResultSimulation.Cancel:
                            {
                                biometricsAnswer = BiometricSimulation(act.AuthResultSimulation.ToString(), act.AuthResultDetailsCancelSimulation.ToString());
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(biometricsAnswer) && biometricsAnswer != "success")
                        {
                            act.Error = "An Error occurred during biometrics simulation. Error2: " + biometricsAnswer;
                        }
                        break;
                    }

                    case ActMobileDevice.eMobileDeviceAction.StopSimulatePhotoOrVideo:
                        CameraAndBarcodeSimulationRequest(null, ImageFormat.Png, contentType: "image", fileName: "image.png", action: "camera");
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetAvailableContexts:
                        int i = 0;
                        foreach (var c in Driver.Contexts)
                        {
                            act.AddOrUpdateReturnParamActual("Context " + i + 1, c.ToString());
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SetContext:
                        Driver.Context = act.ActionInput.ValueForDriver;
                        break;

                    case ActMobileDevice.eMobileDeviceAction.OpenDeeplink:
                        appPackage = GetAppPackage(act);
                        if (!string.IsNullOrEmpty(appPackage))
                        {
                            OpenDeeplink(act.ActionInput.ValueForDriver, appPackage, GetAppPackageNameByOs());
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.IsKeyboardVisible:
                        act.AddOrUpdateReturnParamActual("Is Keyboard Visible", IsDeviceKeyboardVisible().ToString());
                        break;

                    case ActMobileDevice.eMobileDeviceAction.IsLocked:
                        act.AddOrUpdateReturnParamActual("Is Device Locked", IsDeviceLocked().ToString());
                        break;

                    case ActMobileDevice.eMobileDeviceAction.IsAppInstalled:
                        appPackage = GetAppPackage(act);
                        if (!string.IsNullOrEmpty(appPackage))
                        {
                            act.AddOrUpdateReturnParamActual("Is App Installed", IsDeviceAppInstalled(appPackage).ToString());
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.RemoveApp:
                        appPackage = GetAppPackage(act);
                        if (!string.IsNullOrEmpty(appPackage))
                        {
                            RemoveDeviceApp(appPackage);
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.QueryAppState:
                        appPackage = GetAppPackage(act);
                        if (!string.IsNullOrEmpty(appPackage))
                        {
                            act.AddOrUpdateReturnParamActual("App State", QueryAppState(appPackage).ToString());
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.RotateSimulation:
                        switch (act.RotateDeviceState)
                        {
                            case ActMobileDevice.eRotateDeviceState.Landscape:
                            {
                                SwitchToLandscape();
                                break;
                            }
                            case ActMobileDevice.eRotateDeviceState.Portrait:
                            {
                                SwitchToPortrait();
                                break;
                            }
                        }
                        NotifyDeviceRotation();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.RunScript:
                        RunScriptOnDevice(act.ActionInput.ValueForDriver);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.StartRecordingScreen:
                        StartDeviceScreenRecording();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.StopRecordingScreen:
                        string pathRecording = StopRecordingScreen(act.FolderPathInput.ValueForDriver).ToString();
                        act.AddOrUpdateReturnParamActual("ScreenRecordingFilePath", pathRecording);
                        Act.AddArtifactToAction(Path.GetFileName(pathRecording), act, pathRecording);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.HideKeyboard:
                        HideKeyboard();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PushFileToDevice:
                        PushFileToDevice(act.FilePathInput.ValueForDriver, act.FolderPathInput.ValueForDriver);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PullFileFromDevice:
                        PullFileFromDevice(act.FilePathInput.ValueForDriver, act.FolderPathInput.ValueForDriver);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SetClipboardText:
                        SetClipboardText(act.ActionInput.ValueForDriver);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetClipboardText:
                        act.AddOrUpdateReturnParamActual("Get Clipboard Text", GetClipboardText());
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetDeviceLogs:
                        string deviceLogsPath = GetDeviceLogs(act.FolderPathInput.ValueForDriver);
                        act.AddOrUpdateReturnParamActual("DeviceLogFilePath", deviceLogsPath);
                        Act.AddArtifactToAction(Path.GetFileName(deviceLogsPath), act, deviceLogsPath);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetSpecificPerformanceData:
                        appPackage = GetAppPackage(act);
                        if (!string.IsNullOrEmpty(appPackage))
                        {
                            switch (act.PerformanceTypes)
                            {
                                case ActMobileDevice.ePerformanceTypes.Cpuinfo:
                                {
                                    GetSpecificPerformanceData(appPackage, act.PerformanceTypes.ToString(), act);
                                    break;
                                }
                                case ActMobileDevice.ePerformanceTypes.Memoryinfo:
                                {
                                    GetSpecificPerformanceData(appPackage, act.PerformanceTypes.ToString(), act);
                                    break;
                                }
                                case ActMobileDevice.ePerformanceTypes.Batteryinfo:
                                {
                                    GetSpecificPerformanceData(appPackage, act.PerformanceTypes.ToString(), act);
                                    break;
                                }
                                case ActMobileDevice.ePerformanceTypes.Networkinfo:
                                {
                                    GetSpecificPerformanceData(appPackage, act.PerformanceTypes.ToString(), act);
                                    break;
                                }
                                case ActMobileDevice.ePerformanceTypes.Diskinfo:
                                {
                                    GetSpecificPerformanceData(appPackage, act.PerformanceTypes.ToString(), act);
                                    break;
                                }
                            }
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PerformMultiTouch:
                        PerformMultiTouch(act.MobileTouchOperations);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.TypeUsingkeyboard:
                        TypeUsingIOSkeyboard(act.ActionInput.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ClearAppData:
                        ClearAppData(act.ActionAppPackage.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GrantAppPermission:
                        GrantAppPermission(act.ActionAppPackage.ValueForDriver, act.ActionInput.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ScreenSize:
                        ScreenSize(act);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.OpenNotificationsPanel:
                        OpenNotificationsPanel();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetDeviceTime:
                        act.AddOrUpdateReturnParamActual("Device Time", GetDeviceTime());
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetOrientation:
                        act.AddOrUpdateReturnParamActual("Orientation", GetOrientation().ToString());
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetAppPackage:
                        GetAppId(act);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetCurrentActivityDetails:
                        act.AddOrUpdateReturnParamActual("Current Activity", GetCurrentActivityDetails());
                        break;
                    case ActMobileDevice.eMobileDeviceAction.LockForDuration:
                        LockForDuration(act.ActionInput.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetSettings:
                        GetSettings(act);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ToggleLocationServices:
                        ToggleLocationServices();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ToggleData:
                        ToggleData();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ToggleAirplaneMode:
                        ToggleAirplaneMode();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.ToggleWifi:
                        ToggleWifi();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.IsIMEActive:
                        act.AddOrUpdateReturnParamActual("IsIMEActive", IsIMEActive().ToString());
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetIMEActiveEngine:
                        act.AddOrUpdateReturnParamActual("IME Active Engine", GetIMEActiveEngine().ToString());
                        break;
                    case ActMobileDevice.eMobileDeviceAction.StartActivity:
                        StartActivity(act.ActionAppPackage.ValueForDriver, act.ActionInput.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetGeoLocation:
                        GetGeoLocation(act);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SendAppToBackground:
                        SendAppToBackground(act.ActionInput.ValueForDriver);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SetNetworkConnection:
                        SetNetworkConnection();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.GetDeviceOSType:
                        act.AddOrUpdateReturnParamActual("Os Type", GetDeviceOSType());
                        break;

                    default:
                        throw new Exception("Action unknown/not implemented for the Driver: '" + this.GetType().ToString() + "'");
                }
            }
            catch (Exception ex)
            {
                act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
            }
        }

        public string SimulatePhotoOrBarcode(string photoString, string action)
        {
            Bitmap picture = null;
            string response = string.Empty;
            if (isValidPhotoExtention(photoString))
            {
                picture = new Bitmap(photoString);
                response = CameraAndBarcodeSimulationRequest(picture, ImageFormat.Png, contentType: "image", fileName: "image.png", action: action);
                if (response != "success")
                {
                    return "An Error occurred during " + action + " simulation. Error: " + response;
                }
            }
            else
            {
                return "File is not supported. Upload a supported file to use " + action + " simulation";
            }
            return response;
        }

        public bool isValidPhotoExtention(string photo)
        {
            if (string.IsNullOrEmpty(photo))
            {
                return false;
            }
            string extention = photo[(photo.LastIndexOf('.') + 1)..];
            if (extention is "jpg" or "jpeg" or "png")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public string CameraAndBarcodeSimulationRequest(Bitmap picture, ImageFormat format, string contentType, string fileName, string action)
        {
            string encodeString = "0";
            Byte[] bytes = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    if (picture != null)
                    {
                        picture.Save(ms, format);
                        bytes = ms.ToArray();
                        encodeString = Convert.ToBase64String(bytes);
                    }
                }
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }
            }

            Dictionary<string, string> sensorSimulationMap = new Dictionary<string, string>
            {
                { "uploadMedia", encodeString },
                { "contentType", contentType },
                { "fileName", fileName },
                { "action", action }
            };

            string simulationResult = JsonConvert.SerializeObject(Driver.ExecuteScript("mc:sensorSimulation", sensorSimulationMap));
            return JToken.Parse(simulationResult)["message"].ToString();
        }

        public string BiometricSimulation(string authResult, string authResultDetails)
        {
            Dictionary<string, object> sensorSimulationMap = [];
            Dictionary<string, string> simulationData = new Dictionary<string, string>
            {
                { "authResult", authResult },
                { "authType", "Fingerprint" },
                { "authResultDetails", authResultDetails }
            };
            sensorSimulationMap.Add("simulationData", simulationData);
            sensorSimulationMap.Add("action", "authentication");

            string simulationResult = JsonConvert.SerializeObject(Driver.ExecuteScript("mc:sensorSimulation", sensorSimulationMap));
            return JToken.Parse(simulationResult)["message"].ToString();
        }

        private string DictionaryToString(Dictionary<string, string> dict)
        {
            string returnString = string.Empty;
            foreach (KeyValuePair<string, string> entry in dict)
            {
                returnString = new StringBuilder(returnString + entry.Key + ": " + entry.Value + ", ").ToString();
            }
            returnString = returnString[..^2];
            return returnString;
        }

        private void AddReturnParamFromDict(Dictionary<string, string> dict, ActMobileDevice act)
        {
            foreach (KeyValuePair<string, string> entry in dict)
            {
                act.AddOrUpdateReturnParamActual(entry.Key, entry.Value);
            }
        }

        public override void HighlightActElement(Act act)
        {
        }

        private void ActScreenShotHandler(Act act)
        {
            try
            {
                Tuple<int?, int?> customSize = GetUserCustomeScreenshotSize();
                act.AddScreenShot(GetScreenshotImageFromDriver(customSize.Item1, customSize.Item2), "Device Screenshot");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while taking device screen shot", ex);
                act.Error = "Error occurred while taking device screen shot, Details: " + ex.Message;
            }
        }

        private static byte[] ScreenshotToBytes(Bitmap bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.ToArray();
            }
        }
        private static void ScrollDown(AppiumDriver driver)
        {
            try
            {
                var windowSize = driver.Manage().Window.Size;

                int startX = windowSize.Width / 2;
                int startY = (int)(windowSize.Height * 0.8);
                int endY = (int)(windowSize.Height * 0.2);

                var touch = new PointerInputDevice(PointerKind.Touch);
                var sequence = new ActionSequence(touch, 0);

                sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
                sequence.AddAction(touch.CreatePointerDown(0));
                sequence.AddAction(touch.CreatePause(TimeSpan.FromMilliseconds(300)));
                sequence.AddAction(touch.CreatePointerMove(CoordinateOrigin.Viewport, startX, endY, TimeSpan.FromMilliseconds(500)));
                sequence.AddAction(touch.CreatePointerUp(0));

                driver.PerformActions(new List<ActionSequence> { sequence });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while scrolling for screen-shot", ex);
            }
        }

        public Bitmap CaptureFullPageCroppedScreenshot()
        {
            List<Bitmap> screenshots = new List<Bitmap>();
            int scrollCount = 0;
            int maxScrolls = 10;
            HashSet<string> capturedScreens = new HashSet<string>();

            try
            {
                while (scrollCount < maxScrolls)
                {

                    Screenshot screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                    Bitmap bmp = new Bitmap(new MemoryStream(screenshot.AsByteArray));

                    // Dynamically calculate crop sizes based on image height
                    int cropTopPixels = (int)(bmp.Height * 0.05);     // ~5% for status bar
                    int cropBottomPixels = (int)(bmp.Height * 0.07);  // ~5% for gesture bar

                    Rectangle cropArea = new Rectangle(0, cropTopPixels, bmp.Width, bmp.Height - cropTopPixels - cropBottomPixels);
                    Bitmap croppedBmp = bmp.Clone(cropArea, bmp.PixelFormat);

                    bmp.Dispose();

                    string hash = Convert.ToBase64String(ScreenshotToBytes(croppedBmp));
                    if (!capturedScreens.Add(hash))
                    {
                        croppedBmp.Dispose();
                        break;
                    }

                    screenshots.Add(croppedBmp);// keep reference for merge

                    try
                    {
                        ScrollDown(Driver);
                    }
                    catch
                    {
                        break;
                    }

                    scrollCount++;
                    Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while capturing full page screen-shot", ex);
            }

            Bitmap fullImage = MergeScreenshotsVertically(screenshots);
            return fullImage;
        }

        private static Bitmap MergeScreenshotsVertically(List<Bitmap> images)
        {
            int totalHeight = images.Sum(img => img.Height);
            int width = images[0].Width;

            Bitmap finalImage = new Bitmap(width, totalHeight);
            using (Graphics g = Graphics.FromImage(finalImage))
            {
                try
                {
                    int offset = 0;
                    if (images == null || images.Count == 0)
                    {
                        throw new ArgumentException("images collection must contain at least one Bitmap", nameof(images));
                    }
                    foreach (var image in images)
                    {
                        g.DrawImage(image, new Rectangle(0, offset, image.Width, image.Height));
                        offset += image.Height;
                        image.Dispose();          // free as soon as we are done
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while merging screen-shots", ex);
                }
            }
            return finalImage;
        }





        private Tuple<int?, int?> GetUserCustomeScreenshotSize()
        {
            int? customeWidth = null;
            int? customeHeight = null;

            //override with user configured sizes
            if (ScreenshotWidth.ToLower().Trim() != "auto")
            {
                //convert from int
                int userConfiguredWidth;
                if (int.TryParse(ScreenshotWidth, out userConfiguredWidth))
                {
                    //validate user configured width is at least 300 pixels, if not than use the calculated width and write Warn message
                    if (userConfiguredWidth < 200)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The user configured screenshot width is less than 200 pixels, using the calculated width '" + mWindowWidth.ToString() + "' instead.");
                    }
                    else
                    {
                        customeWidth = userConfiguredWidth;
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The user configured screenshot width is not valid, using the calculated width.");
                }
            }
            if (ScreenshotHeight.ToLower().Trim() != "auto")
            {
                //convert from int
                int userConfiguredHeight;
                if (int.TryParse(ScreenshotHeight, out userConfiguredHeight))
                {
                    //validate user configured height is at least 300 pixels, if not than use the calculated height and write Warn message
                    if (userConfiguredHeight < 200)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The user configured screenshot height is less than 200 pixels, using the calculated height '" + mWindowHeight.ToString() + "' instead.");
                    }
                    else
                    {
                        customeHeight = userConfiguredHeight;
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "The user configured screenshot height is not valid, using the calculated height");
                }
            }

            return new Tuple<int?, int?>(customeWidth, customeHeight);
        }

        public ICollection<IWebElement> GetAllElements()
        {
            return (ICollection<IWebElement>)Driver.FindElements(By.XPath(".//*"));
        }

        public void TapXY(int pageX, int pageY)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageX, pageY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void TapElement(IWebElement element, int elementX, int elementY)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(element, elementX, elementY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void DoubleTapXY(int pageX, int pageY)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageX, pageY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(100)));// small wait before second tap
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void DoubleTapElement(IWebElement element, int elementX, int elementY)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(element, elementX, elementY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(100)));// small wait before second tap
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }


        public void PressXY(int pageX, int pageY, TimeSpan pressDuration)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageX, pageY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePause(pressDuration));// Hold for specified duration
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void PerformLongPress(long x, long y, TimeSpan? clickDuration = null)
        {
            if (clickDuration == null)
            {
                clickDuration = TimeSpan.FromSeconds(3);
            }

            PressXY((int)x, (int)y, (TimeSpan)clickDuration);

            if (IsRecording)
            {
                var mobDevAction = GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.LongPressXY);
                mobDevAction.X1.Value = x.ToString();
                mobDevAction.Y1.Value = y.ToString();
                mobDevAction.PressDuration.Value = ((TimeSpan)clickDuration).TotalMilliseconds.ToString();
                RecordingOperations(mobDevAction);
            }
        }

        public void PressElement(IWebElement element, int elementX, int elementY, TimeSpan pressDuration)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(element, elementX, elementY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePause(pressDuration));// Hold for specified duration
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void DragAndDropByXY(int pageDragX, int pageDragY, int pageDropX, int pageDropY, TimeSpan pressDuration, TimeSpan dragDuration)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageDragX, pageDragY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePause(pressDuration));
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageDropX, pageDropY, dragDuration));
            actionBuilder.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(200)));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void SwipeByXY(int pageStartX, int pageStartY, int pageEndX, int pageEndY, TimeSpan swipeDuration)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageStartX, pageStartY, TimeSpan.Zero));
            actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, pageEndX, pageEndY, swipeDuration));
            actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void PerformMultiTouch(ObservableList<MobileTouchOperation> operations)
        {
            AppiumInteractions.PointerInputDevice finger = new AppiumInteractions.PointerInputDevice(PointerKind.Touch);
            ActionBuilder actionBuilder = new ActionBuilder();
            foreach (MobileTouchOperation operation in operations)
            {
                int xCoordinate = 0;
                int yCoordinate = 0;
                TimeSpan duration = TimeSpan.Zero;
                if (operation.OperationDuration != null)
                {
                    duration = TimeSpan.FromMilliseconds(int.Parse(operation.OperationDuration.ToString()));
                }
                if (operation.MoveXcoordinate != null)
                {
                    xCoordinate = int.Parse(operation.MoveXcoordinate.ToString());
                }
                if (operation.MoveYcoordinate != null)
                {
                    yCoordinate = int.Parse(operation.MoveYcoordinate.ToString());
                }
                switch (operation.OperationType)
                {
                    //all operations 
                    case MobileTouchOperation.eFingerOperationType.FingerMove:
                        actionBuilder.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, xCoordinate, yCoordinate, duration));
                        break;
                    case MobileTouchOperation.eFingerOperationType.FingerDown:
                        actionBuilder.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
                        break;
                    case MobileTouchOperation.eFingerOperationType.FingerUp:
                        actionBuilder.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));
                        break;
                    case MobileTouchOperation.eFingerOperationType.Pause:
                        actionBuilder.AddAction(finger.CreatePause(duration));
                        break;
                    case MobileTouchOperation.eFingerOperationType.Cancel:
                        actionBuilder.AddAction(finger.CreatePointerCancel());
                        break;
                }
            }
            Driver.PerformActions(actionBuilder.ToActionSequenceList());
        }

        public void PerformBackButtonPress()
        {
            Driver.Navigate().Back();

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.PressBackButton));
            }
        }

        public void RecordingOperations(ActMobileDevice RecordedAction)
        {
            RecordedAction.Active = true;
            BusinessFlow.CurrentActivity.Acts.Add(RecordedAction);
        }

        public ActMobileDevice GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction ActionPerformed)
        {
            ActMobileDevice mobDevAct = new ActMobileDevice()
            {
                MobileDeviceAction = ActionPerformed,
            };

            mobDevAct.Description = mobDevAct.ActionType;

            return mobDevAct;
        }

        public void PerformHomeButtonPress()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Home);
                    //((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(3);
                    break;
                case eDevicePlatformType.iOS:
                    Dictionary<string, object> commandArgs = new Dictionary<string, object>
                    {
                        { "name", "home" }
                    };
                    Driver.ExecuteScript("mobile: pressButton", commandArgs);
                    break;
            }

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.PressHomeButton));
            }
        }

        public void PerformMenuButtonPress()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Keycode_APP_SWITCH);
                    break;
            }

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.PressMenuButton));
            }
        }

        public void PerformOpenCamera()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Keycode_CAMERA);
                    break;
            }

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.OpenCamera));
            }
        }

        public void PerformVolumeButtonPress(eVolumeOperation volumeOperation)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    switch (volumeOperation)
                    {
                        case eVolumeOperation.Up:
                            ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Keycode_VOLUME_UP);
                            break;
                        case eVolumeOperation.Down:
                            ((AndroidDriver)Driver).PressKeyCode(AndroidKeyCode.Keycode_VOLUME_DOWN);
                            break;
                    }
                    break;
                case eDevicePlatformType.iOS:
                    Dictionary<string, object> commandArgs = [];
                    switch (volumeOperation)
                    {
                        case eVolumeOperation.Up:
                            commandArgs.Add("name", "volumeup");
                            Driver.ExecuteScript("mobile: pressButton", commandArgs);
                            break;
                        case eVolumeOperation.Down:
                            commandArgs.Add("name", "volumedown");
                            Driver.ExecuteScript("mobile: pressButton", commandArgs);
                            break;
                    }
                    break;
            }

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(volumeOperation == eVolumeOperation.Up ? ActMobileDevice.eMobileDeviceAction.PressVolumeUp : ActMobileDevice.eMobileDeviceAction.PressVolumeDown));
            }
        }

        private void AndroidUnlock(ActMobileDevice act, string unlockKind)
        {
            try
            {
                if (!((AndroidDriver)Driver).IsLocked()) return;
                Reporter.ToLog(eLogLevel.DEBUG, $"Device is locked. Unlocking with {unlockKind}...");
                ((AndroidDriver)Driver).Unlock(act.ActionInput.ValueForDriver, unlockKind, "uiautomator", 5000);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unlock attempt failed: " + ex.Message);
                act.Error = "Unlock attempt failed: " + ex.Message;
            }
        }

        public void PerformLockButtonPress(eLockOperation LockOperation, ActMobileDevice act=null)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    switch (LockOperation)
                    {
                        case eLockOperation.Lock:
                            ((AndroidDriver)Driver).Lock();
                            break;
                        case eLockOperation.UnLock:
                            if(act != null)
                            {
                                try
                                {
                                    switch (act.UnLockType)
                                    {
                                        case ActMobileDevice.eUnlockType.none:
                                            {
                                                if (((AndroidDriver)Driver).IsLocked())
                                                {
                                                    ((AndroidDriver)Driver).Unlock("none", "none", "uiautomator", 5000);
                                                }
                                                Thread.Sleep(200);
                                                SwipeScreen(eSwipeSide.Up, 1, TimeSpan.FromMilliseconds(200));
                                                break;
                                            }
                                        case ActMobileDevice.eUnlockType.pin:
                                            AndroidUnlock(act, nameof(ActMobileDevice.eUnlockType.pin));
                                            break;
                                        case ActMobileDevice.eUnlockType.pattern:
                                            AndroidUnlock(act, nameof(ActMobileDevice.eUnlockType.pattern));
                                            break;
                                        case ActMobileDevice.eUnlockType.password:
                                            AndroidUnlock(act, nameof(ActMobileDevice.eUnlockType.password));
                                            break;
                                    }
                                    break;
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Unlock attempt failed: " + ex.Message);
                                    act.Error = "Unlock attempt failed: " + ex.Message;
                                }
                            }
                            break;
                    }
                    break;
                case eDevicePlatformType.iOS:
                    switch (LockOperation)
                    {
                        case eLockOperation.Lock:
                            ((IOSDriver)Driver).Lock();
                            break;
                        case eLockOperation.UnLock:
                            try
                            {
                                if (act != null)
                                {
                                    switch (act.UnLockType)
                                    {
                                        case ActMobileDevice.eUnlockType.none:
                                            {
                                                ((IOSDriver)Driver).Unlock();
                                            }
                                            break;
                                        case ActMobileDevice.eUnlockType.pin:
                                        case ActMobileDevice.eUnlockType.password:
                                            System.Threading.Thread.Sleep(200);
                                            Reporter.ToLog(eLogLevel.DEBUG, "Device is locked. Unlocking with pin/password...");
                                            SwipeScreen(eSwipeSide.Up, 1, TimeSpan.FromMilliseconds(200));
                                            new WebDriverWait(Driver, TimeSpan.FromSeconds(5))
                                                .Until(_ => Driver.IsKeyboardShown());
                                            TypeUsingIOSkeyboard(act.ActionInput.ValueForDriver);
                                            break;
                                        default:
                                            Reporter.ToLog(eLogLevel.ERROR, "iOS devices cannot be unlocked by Appium – run tests with the device already unlocked.");
                                            act.Error = "Unlock attempt failed: unsupported unlock type on iOS.";
                                            break;
                                    }
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Unlock attempt failed: " + ex.Message);
                                act.Error = "Unlock attempt failed: " + ex.Message;
                            }
                            break;
                    }
                    break;
            }

            if (IsRecording)
            {
                RecordingOperations(GetMobileActionforRecording(LockOperation == eLockOperation.Lock ? ActMobileDevice.eMobileDeviceAction.LockDevice : ActMobileDevice.eMobileDeviceAction.UnlockDevice));
            }
        }

        public void PerformKeyPress(string key)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).PressKeyCode(Convert.ToInt32(Enum.Parse(typeof(ActMobileDevice.ePressKey), key)));
                    break;
                    //case eDevicePlatformType.iOS:
                    //    Dictionary<string, object> commandArgs = new Dictionary<string, object>();
                    //    commandArgs.Add("name", key);
                    //    Driver.ExecuteScript("mobile: pressButton", commandArgs);
                    //    break;
            }
        }

        public void PerformLongKeyPress(string key)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).LongPressKeyCode(Convert.ToInt32(Enum.Parse(typeof(ActMobileDevice.ePressKey), key)));
                    break;
            }
        }

        public async Task<string> GetPageSource()
        {
            string Pagesource = String.Empty;
            await Task.Run(() =>
            {
                try
                {
                    Pagesource = Driver.PageSource;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the mobile application page source", ex);
                    Pagesource = string.Empty;//failed to get the Page Source
                }
            });
            return Pagesource;
        }

        public void SwipeScreen(eSwipeSide side, double swipeScale, TimeSpan swipeDuration)
        {
            System.Drawing.Size sz = Driver.Manage().Window.Size;
            double startX;
            double startY;
            double endX;
            double endY;
            //valide swipeScale is between 0.1 and 2 and if not set it to 1 and write warn messaage
            if (swipeScale < 0.1 || swipeScale > 2)
            {
                Reporter.ToLog(eLogLevel.WARN, "Mobile action swipeScreen(): swipeScale: '" + swipeScale + "' is out of range (0.1-2), setting it to 1");
                swipeScale = 1;
            }
            //validate swipeDuration is bigger than 0 and if not set it to 200 and write warn messaage
            if (swipeDuration.TotalMilliseconds < 0)
            {
                Reporter.ToLog(eLogLevel.WARN, "Mobile action swipeScreen(): swipeDuration: '" + swipeDuration + "' is out of range, setting it to 200");
                swipeDuration = TimeSpan.FromMilliseconds(200);
            }
            switch (side)
            {
                case eSwipeSide.Down: // center of footer
                    startX = sz.Width * 0.5;
                    startY = sz.Height * 0.3;
                    endX = sz.Width * 0.5;
                    endY = startY + (sz.Height * 0.4 * swipeScale);
                    break;
                case eSwipeSide.Up: // center of header
                    startX = sz.Width * 0.5;
                    startY = sz.Height * 0.7;
                    endX = sz.Width * 0.5;
                    endY = startY - (sz.Height * 0.4 * swipeScale);
                    break;
                case eSwipeSide.Right: // center of left side
                    startX = sz.Width * 0.3;
                    startY = sz.Height * 0.5;
                    endX = startX + (sz.Width * 0.4 * swipeScale);
                    endY = sz.Height * 0.5;
                    break;
                case eSwipeSide.Left: // center of right side
                    startX = sz.Width * 0.7;
                    startY = sz.Height * 0.5;
                    endX = startX - (sz.Width * 0.4 * swipeScale);
                    endY = sz.Height * 0.5;
                    break;
                default:
                    throw new ArgumentException("swipeScreen(): dir: '" + side + "' NOT supported");
            }

            SwipeByXY((int)startX, (int)startY, (int)endX, (int)endY, swipeDuration);
        }


        private string GetCurrentPackage()
        {
            try
            {
                string currentPackage = string.Empty;
                if (Driver is AndroidDriver)
                {
                    currentPackage = string.Format("{0}", ((AndroidDriver)Driver).CurrentPackage);
                }
                else
                {
                    currentPackage = string.Format("{0}", ((Dictionary<string, object>)Driver.ExecuteScript("mobile: activeAppInfo"))["bundleId"]);
                }

                return currentPackage;
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.WARN, "An error ocured while fetching the current Package details", exc);
                return null;
            }
        }

        public string GetCurrentActivityDetails()
        {
            try
            {
                if (DevicePlatformType == eDevicePlatformType.Android)
                {
                    return string.Format("{0} | {1}", ((AndroidDriver)Driver).CurrentPackage.Split('.').Last(),
                        ((AndroidDriver)Driver).CurrentActivity.Split('.').Last());
                }
                else if (DevicePlatformType == eDevicePlatformType.iOS)
                {
                    var detail = ((IOSDriver)Driver).GetSessionDetail("CFBundleIdentifier");
                    if (detail != null)
                    {
                        return string.Format("{0}", ((IOSDriver)Driver).GetSessionDetail("CFBundleIdentifier").ToString());
                    }
                    else
                    {
                        return "Package | Activity";
                    }
                }
                else
                {
                    return string.Format("{0} | {1}", Driver.Capabilities.GetCapability("appPackage").ToString(),
                                                    Driver.Capabilities.GetCapability("appActivity").ToString());
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.WARN, "An error ocured while fetching the current App details", exc);
                return "Package | Activity";
            }
        }

        public string GetCurrentActivityCompleteDetails()
        {
            try
            {
                if (DevicePlatformType == eDevicePlatformType.Android)
                {
                    return string.Format("{0} | {1}", ((AndroidDriver)Driver).CurrentPackage,
                        ((AndroidDriver)Driver).CurrentActivity);
                }
                else if (DevicePlatformType == eDevicePlatformType.iOS)
                {
                    return string.Format("{0}", ((IOSDriver)Driver).GetSessionDetail("CFBundleIdentifier")?.ToString());
                }
                else
                {
                    return string.Format("{0} | {1}", Driver.Capabilities.GetCapability("appPackage").ToString(),
                                                    Driver.Capabilities.GetCapability("appActivity").ToString());
                }
            }
            catch (Exception exc)
            {
                Reporter.ToLog(eLogLevel.WARN, "An error ocured while fetching the current App details", exc);
                return "Package | Activity";
            }
        }

        public override string GetURL()
        {
            if (AppType == eAppType.Web)
            {
                return mSeleniumDriver.GetURL();
            }

            return GetCurrentActivityCompleteDetails();
        }

        public override bool IsRunning()
        {
            return mIsDeviceConnected;
        }

        public override bool IsWindowExplorerSupportReady()
        {
            return true;
        }

        List<AppWindow> IWindowExplorer.GetAppWindows()
        {
            List<AppWindow> list = [];

            AppWindow AW = new AppWindow
            {
                WindowType = AppWindow.eWindowType.Appium,
                Title = (AppType == eAppType.Web) ? GetURL() : GetCurrentActivityDetails()   // TODO: add device name and info
            };

            list.Add(AW);
            return list;
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            //NA
        }

        async void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false, IList<ElementInfo> MappedUIElements = null)
        {

            ElementInfo filteredElementInfo = POMExecutionUtils.FilterElementDetailsByCategory(ElementInfo, PomCategory);

            if (AppType == eAppType.Web)
            {
                ((IWindowExplorer)mSeleniumDriver).HighLightElement(filteredElementInfo, locateElementByItLocators);
                return;
            }

            if (filteredElementInfo.X == 0 && filteredElementInfo.Properties.FirstOrDefault(p => p.Name == "x") != null)
            {
                filteredElementInfo.X = Convert.ToInt32(filteredElementInfo.Properties.FirstOrDefault(p => p.Name == "x").Value);
            }
            if (filteredElementInfo.Y == 0 && filteredElementInfo.Properties.FirstOrDefault(p => p.Name == "y") != null)
            {
                filteredElementInfo.Y = Convert.ToInt32(filteredElementInfo.Properties.FirstOrDefault(p => p.Name == "y").Value);
            }

            if (filteredElementInfo.ElementObject == null)
            {
                filteredElementInfo.ElementObject = await FindElementXmlNodeByXY(filteredElementInfo.X, filteredElementInfo.Y);
            }

            OnDriverMessage(eDriverMessageType.HighlightElement, filteredElementInfo);
        }

        private void RemoveElemntRectangle()
        {
        }


        string IWindowExplorer.GetFocusedControl()
        {
            return null;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {
            return GetElementAtMousePosition();
        }

        private ElementInfo GetElementAtMousePosition()
        {
            Point mousePosCurrent = new Point(-1, -1);
            XmlNode foundNode = null;
            ElementInfo foundElement = null;
            var mousePos = OnSpyingElementEvent();
            if (mousePos is not null and Point)
            {
                mousePosCurrent = (Point)mousePos;  // new Point((mousePos as Point).X, (mousePos as Point).Y);
            }

            if (mousePosCurrent.X > -1 && mousePosCurrent.Y > -1)
            {
                if (AppType == eAppType.Web)
                {
                    foundElement = ((IVisualTestingDriver)mSeleniumDriver).GetElementAtPoint(mousePosCurrent.X, mousePosCurrent.Y).Result;
                }
                else
                {
                    foundNode = FindElementXmlNodeByXY(mousePosCurrent.X, mousePosCurrent.Y, false).Result;

                    if (foundNode != null)
                    {
                        foundElement = GetElementInfoforXmlNode(foundNode).Result;

                        if (foundElement != null)
                        {
                            OnDriverMessage(eDriverMessageType.HighlightElement, foundElement);
                        }
                    }
                }
            }

            return foundElement;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).GetActiveWindow();
            }

            if (Driver != null)
            {
                AppWindow aw = new AppWindow
                {
                    Title = GetCurrentActivityDetails()
                };
                return aw;
            }
            else
            {
                return null;
            }
        }
        Screenshot fullScreenshot = null; // ((ITakesScreenshot)Driver).GetScreenshot(); 
        async Task<List<ElementInfo>> IWindowExplorer.GetVisibleControls(PomSetting pomSetting, ObservableList<ElementInfo> foundElementsList = null, ObservableList<POMPageMetaData> PomMetaData = null)
        {
            if (AppType == eAppType.Web)
            {
                mSeleniumDriver.ExtraLocatorsRequired = !(pomSetting.RelativeXpathTemplateList == null || pomSetting.RelativeXpathTemplateList.Count == 0);

                return await Task.Run(() => ((IWindowExplorer)mSeleniumDriver).GetVisibleControls(pomSetting, foundElementsList));
            }

            try
            {
                mIsDriverBusy = true;

                if (foundElementsList == null)
                {
                    foundElementsList = [];
                }

                await GetPageSourceDocument(true);

                fullScreenshot = ((ITakesScreenshot)Driver).GetScreenshot();

                //Get all elements but only clickable elements= user can interact with them
                XmlNodeList nodes = pageSourceXml.SelectNodes("//*");
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (StopProcess)
                    {
                        return foundElementsList.ToList();
                    }

                    //Show only clickable elements
                    //if (nodes[i].Attributes != null)
                    //{
                    //    var cattr = nodes[i].Attributes["clickable"];
                    //    if (cattr != null)
                    //    {
                    //        if (cattr.Value == "false") continue;
                    //    }
                    //}

                    if (nodes[i].Attributes != null && nodes[i].Attributes.Count == 0)
                    {
                        continue;
                    }

                    ElementInfo EI = await GetElementInfoforXmlNode(nodes[i]);
                    EI.IsAutoLearned = true;

                    if (pomSetting.RelativeXpathTemplateList != null && pomSetting.RelativeXpathTemplateList.Count > 0)
                    {
                        foreach (var template in pomSetting.RelativeXpathTemplateList)
                        {
                            eLocateBy CustomLocLocateBy = eLocateBy.ByRelXPath;

                            if (template.Value.Contains('{'))
                            {
                                CustomLocLocateBy = eLocateBy.iOSPredicateString;
                            }

                            var customLocator = GetUserDefinedCustomLocatorFromTemplates(template.Value, CustomLocLocateBy, EI.Properties.ToList());

                            if (customLocator != null)
                            {
                                EI.Locators.Add(customLocator);
                            }
                            //CreateXpathFromUserTemplate(template, foundElemntInfo);
                        }
                    }

                    //set the POM category
                    EI.SetLocatorsAndPropertiesCategory(this.PomCategory);

                    if (pomSetting.FilteredElementType == null ||
                        (pomSetting.FilteredElementType != null && pomSetting.FilteredElementType.Any(x => x.ElementType.Equals(EI.ElementTypeEnum))))
                    {
                        if ((EI.XPath).Contains("android") || (EI.XPath).Contains("XCUIElement"))
                        {
                            AppiumElement realElement = Driver.FindElement(By.XPath(EI.XPath));
                            EI.ScreenShotImage = TakeElementScreenShot(realElement);
                        }
                        foundElementsList.Add(EI);
                    }
                }

                return foundElementsList.ToList();
            }
            finally
            {
                mIsDriverBusy = false;
            }
        }
        private string TakeElementScreenShot(IWebElement element)
        {         
            using (Bitmap fullImage = ScreenshotToImage(fullScreenshot))
            {
                var location = element.Location;
                var size = element.Size;

                // Ensure the crop rectangle is within bounds
                Rectangle cropRect = new Rectangle(
                Math.Max(location.X, 0),
                Math.Max(location.Y, 0),
                Math.Min(size.Width, fullImage.Width - location.X),
                Math.Min(size.Height, fullImage.Height - location.Y)
            );
                using (Bitmap elementImage = new Bitmap(cropRect.Width, cropRect.Height))
                {
                    using (Graphics g = Graphics.FromImage(elementImage))
                    {
                        g.DrawImage(fullImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height), cropRect, GraphicsUnit.Pixel);
                    }

                    using (MemoryStream ms = new MemoryStream())
                    {
                        elementImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }
        private Bitmap ScreenshotToImage(Screenshot screenshot)
        {
            using (MemoryStream ms = new MemoryStream(screenshot.AsByteArray))
            {
                return new Bitmap(ms);
            }
        }
        private async Task<ElementInfo> GetElementInfoforXmlNode(XmlNode xmlNode)
        {
            ElementInfo EI = new ElementInfo();
            Tuple<string, eElementType> Elementype = GetElementTypeEnum(xmlNode);
            EI.ElementType = Elementype.Item1;           //GetAttrValue(xmlNode, "class");
            EI.ElementTypeEnum = Elementype.Item2;
            EI.ElementTitle = GetNameFor(xmlNode);
            //EI.ElementTitle = string.Format("{0}-{1}", Elementype.Item2, GetNameFor(xmlNode));
            EI.Value = GetAttrValue(xmlNode, "text");

            if (string.IsNullOrEmpty(EI.Value))
            {
                EI.Value = GetAttrValue(xmlNode, "content-desc");
            }

            if (string.IsNullOrEmpty(EI.Value))
            {
                EI.Value = GetAttrValue(xmlNode, "value");
            }

            EI.ElementObject = xmlNode;
            EI.XPath = await GetNodeXPath(xmlNode);
            EI.WindowExplorer = this;

            if (xmlNode.Attributes["bounds"] != null)
            {
                string bounds = xmlNode.Attributes["bounds"].Value;
                bounds = bounds.Replace("[", ",");
                bounds = bounds.Replace("]", ",");
                string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (boundsXY.Length == 4)
                {
                    EI.X = Convert.ToInt32(boundsXY[0]);
                    EI.Y = Convert.ToInt32(boundsXY[1]);
                }
            }

            EI.Locators = EI.GetElementLocators();
            EI.Properties = EI.GetElementProperties();

            return EI;
        }

        public static Tuple<string, eElementType> GetElementTypeEnum(XmlNode xmlNode)
        {
            Tuple<string, eElementType> returnTuple;
            eElementType elementType = eElementType.Unknown;
            string elementTagName = string.Empty;
            string elementTypeAtt;

            if (xmlNode != null)
            {
                elementTagName = xmlNode.Name;
                elementTypeAtt = GetAttrValue(xmlNode, "type");
                if (string.IsNullOrEmpty(elementTypeAtt))
                {
                    elementTypeAtt = GetAttrValue(xmlNode, "class");
                }
            }
            else
            {
                returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);
                return returnTuple;
            }

            if (!string.IsNullOrEmpty(elementTagName))
            {
                elementType = GetElementTypeFromTag(elementTagName);
            }

            if (elementType == eElementType.Unknown && !string.IsNullOrEmpty(elementTypeAtt))
            {
                elementType = GetElementTypeFromTag(elementTypeAtt);
            }

            returnTuple = new Tuple<string, eElementType>(elementTagName, elementType);

            return returnTuple;
        }

        public static eElementType GetElementTypeFromTag(string ElementTag)
        {
            return ElementTag.ToLower() switch
            {
                "android.widget.edittext" or "xcuielementtypetextfield" or "xcuielementtypesearchfield" => eElementType.TextBox,
                "android.widget.button" or "android.widget.ratingbar" or "android.widget.framelayout" or "android.widget.imageview" or "android.widget.imagebutton" or "android.widget.switch" or "xcuielementtypebutton" or "xcuielementtypeswitch" => eElementType.Button,
                "android.widget.spinner" or "xcuielementtypecombobox" => eElementType.ComboBox,
                "android.widget.checkbox" or "xcuielementtypecheckbox" => eElementType.CheckBox,
                "android.widget.view" or "android.widget.textview" or "android.widget.checkedtextview" or "xcuielementtypecell" or "xcuielementtypestatictext" => eElementType.Label,
                "android.widget.image" or "xcuielementtypeimage" => eElementType.Image,
                "android.widget.radiobutton" or "xcuielementtyperadiobutton" => eElementType.RadioButton,
                "android.widget.canvas" or "android.widget.linearlayout" or "android.widget.relativelayout" or "xcuielementtypeother" => eElementType.Canvas,
                "xcuielementtypetab" => eElementType.Tab,
                "xcuielementtypebrowser" => eElementType.Browser,
                "xcuielementtypetable" => eElementType.Table,
                "xcuielementtypetablerow" or "xcuielementtypetablecolumn" => eElementType.TableItem,
                "xcuielementtypelink" => eElementType.HyperLink,
                "android.widget.form" => eElementType.Form,
                "android.view.view" => eElementType.Div,
                //case "android.widget.checkedtextview":
                //case "xcuielementtypecell":
                //    return eElementType.ListItem;
                "xcuielementtypewindow" => eElementType.Window,
                _ => eElementType.Unknown,
            };
        }
        private async Task<string> GetNodeXPath(XmlNode xmlNode)
        {
            return await GetXPathToNode(xmlNode);
        }

        /// Gets the X-Path to a given Node
        /// </summary>
        /// <param name="node">The Node to get the X-Path from</param>
        /// <returns>The X-Path of the Node</returns>
        public async Task<string> GetXPathToNode(XmlNode node)
        {
            //TODO: verify XPath return 1 item back to same xmlnode.

            //string resid = GetAttrValue(node, "resource-id");
            //if (!string.IsNullOrEmpty(resid))
            //{
            //    return string.Format("//*[@resource-id='{0}']", resid);
            //}

            if (node.ParentNode == null)
            {
                // the only node with no parent is the root node, which has no path
                return "";
            }

            // Get the Index
            int indexInParent = 1;
            XmlNode siblingNode = node.PreviousSibling;
            // Loop thru all Siblings
            while (siblingNode != null)
            {
                // Increase the Index if the Sibling has the same Name
                if (siblingNode.Name == node.Name)
                {
                    indexInParent++;
                }
                siblingNode = siblingNode.PreviousSibling;
            }

            // the path to a node is the path to its parent, plus "/node()[n]", where n is its position among its siblings.
            if (node.Name.ToLower() == "appiumaut")
            {
                return string.Format("/");
            }
            else
            {
                return String.Format("{0}/{1}[{2}]", await GetXPathToNode(node.ParentNode), node.Name, indexInParent);          //Testing Async
            }
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            List<ElementInfo> list = [];

            //AppiumElementInfo EI = (AppiumElementInfo)ElementInfo;
            //XmlNode node = EI.XmlNode;
            //XmlNodeList nodes = node.ChildNodes;
            //for(int i=0;i<nodes.Count;i++)
            //{
            //    AppiumElementInfo AEI = GetElementInfoforXmlNode(nodes[i]);                                
            //    list.Add(AEI);
            //}

            return list;
        }

        private string GetNameFor(XmlNode xmlNode)
        {
            string Name;
            if (DevicePlatformType == eDevicePlatformType.Android)
            {
                Name = GetAttrValue(xmlNode, "content-desc");

                if (string.IsNullOrEmpty(Name))
                {
                    Name = GetAttrValue(xmlNode, "text");
                }

                if (string.IsNullOrEmpty(Name))
                {
                    string resid = GetAttrValue(xmlNode, "resource-id");
                    if (!string.IsNullOrEmpty(resid))
                    {
                        // if we have resource id then get just the id out of it
                        string[] a = resid.Split('/');
                        Name = a[^1];
                    }
                }
            }
            else
            {
                Name = GetAttrValue(xmlNode, "name");

                if (string.IsNullOrEmpty(Name))
                {
                    Name = GetAttrValue(xmlNode, "label");
                }

                if (string.IsNullOrEmpty(Name))
                {
                    Name = GetAttrValue(xmlNode, "value");
                }
            }

            if (!string.IsNullOrEmpty(Name))
            {
                return Name;
            }

            return xmlNode.Name;
        }

        static string GetAttrValue(XmlNode xmlNode, string attr)
        {
            if (xmlNode == null || xmlNode.Attributes == null)
            {
                return null;
            }

            if (xmlNode.Attributes[attr] == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(xmlNode.Attributes[attr].Value))
            {
                return null;
            }

            return xmlNode.Attributes[attr].Value;
        }

        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).GetElementLocators(ElementInfo, pomSetting);
            }

            ObservableList<ElementLocator> list = [];

            //if (DevicePlatformType == eDevicePlatformType.iOS)
            //{
            //    string selector = string.Format("type == '{0}' AND value BEGINSWITH[c] '{1}' AND visible == {2}",
            //        ElementInfo.ElementType, GetAttrValue(ElementInfo.ElementObject as XmlNode, "value"), GetAttrValue(ElementInfo.ElementObject as XmlNode, "visible") == "true" ? 1 : 0);
            //    ElementLocator iOSPredStrLoc = new ElementLocator()
            //    {
            //        Active = true,
            //        LocateBy = eLocateBy.iOSPredicateString,
            //        LocateValue = selector,
            //        IsAutoLearned = true,
            //        Help = "Highly Recommended as Predicate Matching is built into XCUITest, it has the potential to be much faster than Appium's XPath strategy"
            //    };

            //    if(LocateElementByLocator(iOSPredStrLoc) != null)
            //        list.Add(iOSPredStrLoc);

            //    ElementLocator iOSClassChainLoc = new ElementLocator()
            //    {
            //        Active = true,
            //        LocateBy = eLocateBy.iOSClassChain,
            //        LocateValue = string.Format("**/{0}/{1}", ElementInfo.XPath.Split('/')[ElementInfo.XPath.Split('/').Length - 2], ElementInfo.XPath.Split('/').Last()),
            //        IsAutoLearned = true,
            //        Help = "Highly Recommended as Class Chain strategy is built into XCUITest, it has the potential to be much faster than Appium's XPath strategy"
            //    };

            //    if (LocateElementByLocator(iOSClassChainLoc) != null)
            //        list.Add(iOSPredStrLoc);
            //}

            //Only by Resource ID
            string resid = GetAttrValue(ElementInfo.ElementObject as XmlNode, "resource-id");
            string residXpath = string.Format("//*[@resource-id='{0}']", resid);
            if (!string.IsNullOrEmpty(resid) && residXpath != ElementInfo.XPath) // We show by res id when it is different then the elem XPath, so not to show twice the same, the AE.Apath can include relative info
            {
                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = residXpath,
                    IsAutoLearned = true,
                    Help = "Highly Recommended when resourceid exist, long path with relative information is sensitive to screen changes"
                });

                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByResourceID,
                    LocateValue = resid,
                    IsAutoLearned = true,
                    Help = "Highly Recommended for Resource-Ids being unique"
                });
            }

            //by Name
            string elemName = GetAttrValue(ElementInfo.ElementObject as XmlNode, "name");
            if (!string.IsNullOrEmpty(elemName)) // We show by res id when it is different then the elem XPath, so not to show twice the same, the AE.Apath can include relative info
            {
                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByName,
                    LocateValue = elemName,
                    IsAutoLearned = true,
                    Help = "Use Name only when you don't want XPath with relative info, but the resource-id is unique"
                });

                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = string.Format("//{0}[@name=\"{1}\"]", ElementInfo.ElementType, elemName),
                    IsAutoLearned = true,
                    Help = "Highly Recommended locator for XCUITest Driver, long path with relative information is sensitive to screen changes"
                });
            }

            //By Content-desc
            string contentdesc = GetAttrValue(ElementInfo.ElementObject as XmlNode, "content-desc");
            if (!string.IsNullOrEmpty(contentdesc))
            {
                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = string.Format("//*[@content-desc='{0}']", contentdesc),
                    IsAutoLearned = true,
                    Help = "content-desc is Recommended when resource-id not exist"
                });
            }

            // By Class and text
            string eClass = GetAttrValue(ElementInfo.ElementObject as XmlNode, "class");
            string eText = GetAttrValue(ElementInfo.ElementObject as XmlNode, "text");
            if (!string.IsNullOrEmpty(eClass) && !string.IsNullOrEmpty(eText))
            {
                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.ByRelXPath,
                    LocateValue = string.Format("//{0}[@text='{1}']", eClass, eText),    // like: //android.widget.RadioButton[@text='Ginger']" 
                    IsAutoLearned = true,
                    Help = "use class and text when you have list of items and no resource-id to use"
                });
            }

            // Show XPath
            list.Add(new ElementLocator()
            {
                Active = true,
                LocateBy = eLocateBy.ByXPath,
                LocateValue = ElementInfo.XPath,
                IsAutoLearned = true,
                Help = "Highly Recommended when resourceid exist, long path with relative information is sensitive to screen changes"
            });

            return list;
        }

        // Get the data of the element
        // For Combo box: will return all valid values - options available - List<ComboBoxElementItem>
        // For Table: will return list of rows data: List<TableElementItem>        
        object IWindowExplorer.GetElementData(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            return null;
        }

        ObservableList<ControlProperty> IWindowExplorer.GetElementProperties(ElementInfo ElementInfo)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).GetElementProperties(ElementInfo);
            }

            ObservableList<ControlProperty> list = [];


            if (ElementInfo.ElementObject is not XmlNode node)
            {
                return list;
            }

            XmlAttributeCollection attrs = node.Attributes;

            if (attrs == null)
            {
                return list;
            }

            for (int i = 0; i < attrs.Count; i++)
            {
                ControlProperty CP = new ControlProperty
                {
                    Name = attrs[i].Name,
                    Value = attrs[i].Value
                };
                list.Add(CP);

                if (CP.Name == "x")
                {
                    ElementInfo.X = Convert.ToInt32(CP.Value);
                }
                else if (CP.Name == "y")
                {
                    ElementInfo.Y = Convert.ToInt32(CP.Value);
                }
            }

            return list;
        }




        public event RecordingEventHandler RecordingEvent;

        void IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        bool IsRecording = false;

        void IRecord.StartRecording(bool learnAdditionalChanges)
        {
            //if(AppType == eAppType.Web)
            //{
            //    mSeleniumDriver.StartRecording();
            //}
            //else
            //IsRecording = true;

            IsRecording = true;

            OnDriverMessage(eDriverMessageType.RecordingEvent, IsRecording);
            //Dispatcher.Invoke(() =>
            //{
            //    if (DriverWindow != null) DriverWindow.StartRecording();
            //});
        }

        void IRecord.StopRecording()
        {
            //if (AppType == eAppType.Web)
            //{
            //    mSeleniumDriver.StopRecording();
            //}

            IsRecording = false;
            OnDriverMessage(eDriverMessageType.RecordingEvent, IsRecording);

            //Dispatcher.Invoke(() =>
            //{
            //    if (DriverWindow != null) DriverWindow.StopRecording();
            //});
        }

        public override void StartRecording()
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (DriverWindow != null) DriverWindow.StartRecording();
            //});            
        }

        public override void StopRecording()
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (DriverWindow != null) DriverWindow.StopRecording();
            //});   
        }

        ObservableList<ElementInfo> IWindowExplorer.GetElements(ElementLocator EL)
        {
            throw new Exception("Not implemented yet for this driver");
        }

        void IWindowExplorer.UpdateElementInfoFields(ElementInfo eI)
        {
        }

        public bool IsElementObjectValid(object obj)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).IsElementObjectValid(obj);
            }

            return true;
        }

        public void UnHighLightElements()
        {
            if (AppType == eAppType.Web)
            {
                ((IWindowExplorer)mSeleniumDriver).UnHighLightElements();
                return;
            }

            OnDriverMessage(eDriverMessageType.UnHighlightElement);
        }
        public void NotifyDeviceRotation()
        {
            OnDriverMessage(eDriverMessageType.RotateEvent);
        }
        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false, ApplicationPOMModel mPOM = null)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).TestElementLocators(EI, GetOutAfterFoundElement);
            }

            try
            {
                mIsDriverBusy = true;

                foreach (ElementLocator el in EI.Locators)
                {
                    el.LocateStatus = ElementLocator.eLocateStatus.Pending;
                }

                List<ElementLocator> activesElementLocators = EI.Locators.Where(x => x.Active == true).ToList();
                Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0);

                foreach (ElementLocator el in activesElementLocators)
                {
                    object elem;
                    if (!el.IsAutoLearned)
                    {
                        elem = LocateElementIfNotAutoLeared(el);
                    }
                    else
                    {
                        elem = LocateElementByLocator(el);
                    }

                    if (elem != null)
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

                if (activesElementLocators.Any(x => x.LocateStatus == ElementLocator.eLocateStatus.Passed))
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

                mIsDriverBusy = false;
            }
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            if (AppType == eAppType.Web)
            {
                ((IWindowExplorer)mSeleniumDriver).CollectOriginalElementsDataForDeltaCheck(originalList);
                return;
            }

            try
            {
                mIsDriverBusy = true;

                originalList.Select(e => { e.ElementStatus = ElementInfo.eElementStatus.Pending; return e; }).ToList();
                foreach (ElementInfo EI in originalList)
                {
                    try
                    {
                        object e = LocateElementByLocators(EI.Locators);
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
                mIsDriverBusy = false;
            }
        }

        public object LocateElementByLocators(ObservableList<ElementLocator> Locators)
        {
            object elem = null;
            foreach (ElementLocator locator in Locators)
            {
                locator.StatusError = string.Empty;
                locator.LocateStatus = ElementLocator.eLocateStatus.Pending;
            }

            foreach (ElementLocator locator in Locators.Where(x => x.Active == true).ToList())
            {
                if (!locator.IsAutoLearned)
                {
                    elem = LocateElementIfNotAutoLeared(locator);
                }
                else
                {
                    elem = LocateElementByLocator(locator);
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

            return null;
        }

        public object LocateElementByLocator(ElementLocator EL)
        {
            //need to override regular selenium driver locator if needed, 
            //if not then to run the regular selenium driver locator for it to avoid duplication   
            object elem;

            try
            {
                elem = EL.LocateBy switch
                {
                    eLocateBy.ByResourceID => Driver.FindElement(By.Id(EL.LocateValue)),
                    eLocateBy.iOSPredicateString => Driver.FindElement(MobileBy.IosNSPredicate(EL.LocateValue)),
                    eLocateBy.iOSClassChain => Driver.FindElement(MobileBy.IosClassChain(EL.LocateValue)),
                    eLocateBy.ByRelXPath or eLocateBy.ByXPath => Driver.FindElement(By.XPath(EL.LocateValue)),
                    _ => mSeleniumDriver.LocateElementByLocator(EL, Driver),
                };
            }
            catch (Exception exc)
            {
                elem = null;
                EL.StatusError = exc.Message;
                EL.LocateStatus = ElementLocator.eLocateStatus.Failed;
            }

            return elem;
        }

        private object LocateElementIfNotAutoLeared(ElementLocator el)
        {
            ElementLocator evaluatedLocator = el.CreateInstance() as ElementLocator;
            //ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);
            //evaluatedLocator.LocateValue = VE.Calculate(evaluatedLocator.LocateValue);
            return LocateElementByLocator(evaluatedLocator);
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            if (AppType == eAppType.Web)
            {
                return mSeleniumDriver.GetMatchingElement(latestElement, originalElements);
            }

            //try using online IWebElement Objects comparison
            //ElementInfo OriginalElementInfo = originalElements.Where(x => (x.ElementObject != null) && (latestElement.ElementObject != null) && (x.ElementObject.ToString() == latestElement.ElementObject.ToString())).FirstOrDefault();//comparing IWebElement ID's

            ElementInfo OriginalElementInfo = originalElements.FirstOrDefault(x => (x.ElementTypeEnum == latestElement.ElementTypeEnum)
                                                                    && (x.XPath == latestElement.XPath)
                                                                    && (x.Path == latestElement.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(latestElement.Path)))
                                                                    && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                        || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                            && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                            )
                                                                        )
);

            return OriginalElementInfo;
        }

        bool mSpying = false;
        public bool IsSpying
        {
            get
            {
                return mSpying;
            }
            set
            {
                if (value != mSpying)
                {
                    mSpying = value;
                }
            }
        }

        public void StartSpying()
        {
            if (AppType == eAppType.Web)
            {
                ((IWindowExplorer)mSeleniumDriver).StartSpying();
                return;
            }

            IsSpying = true;
        }

        public ElementInfo LearnElementInfoDetails(ElementInfo EI, PomSetting pomSetting = null)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).LearnElementInfoDetails(EI, pomSetting);
            }

            EI = GetElementInfoforXmlNode(EI.ElementObject as XmlNode).Result;
            return EI;
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }

        List<AppWindow> IWindowExplorer.GetWindowAllFrames()
        {
            throw new NotImplementedException();
        }

        public eDevicePlatformType GetDevicePlatformType()
        {
            return DevicePlatformType;
        }

        public eAppType GetAppType()
        {
            return AppType;
        }

        int checkSessionCounter = 0;
        public Byte[] GetScreenshotImage()
        {
            checkSessionCounter++;
            //check session is still valid
            if (AppType == eAppType.NativeHybride && checkSessionCounter % 30 == 0)
            {
                if (Driver.SessionDetails != null)
                {
                    return GetScreenshotImageFromDriver();
                }
            }
            else
            {
                return GetScreenshotImageFromDriver();
            }

            return null;
        }

        private Byte[] GetScreenshotImageFromDriver(int? width = null, int? height = null)
        {
            int screenshotWidth = 0;
            int screenshotHeight = 0;

            //Take screen shot
            Screenshot screenshot = Driver.GetScreenshot();
            //Update screen size for iOS as it changed per app
            if (DevicePlatformType == eDevicePlatformType.iOS)
            {
                CalculateMobileDeviceScreenSizes();
            }
            screenshotWidth = mWindowWidth;
            screenshotHeight = mWindowHeight;

            //ovveride with user configured width/height if such
            if (width != null)
            {
                screenshotWidth = width.Value;
            }
            if (height != null)
            {
                screenshotHeight = height.Value;
            }

            // Convert screenshot to Image for resizing
            using (var stream = new MemoryStream(screenshot.AsByteArray))
            using (var image = Image.FromStream(stream))
            {
                // Create a new bitmap with the native device size
                using (var resizedImage = new Bitmap(screenshotWidth, screenshotHeight))
                {
                    // Draw the original image onto the new bitmap
                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.DrawImage(image, 0, 0, screenshotWidth, screenshotHeight);
                    }
                    // Convert the resized image to byte array
                    using (var ms = new MemoryStream())
                    {
                        resizedImage.Save(ms, ImageFormat.Png);
                        return ms.ToArray();
                    }
                }
            }
        }

        public void PerformTap(long x, long y)
        {
            if (IsRecording)
            {
                //ElementInfo elemInfo = GetElementAtPoint(x, y).Result;
                ElementInfo elemInfo = GetElementAtMousePosition();

                if (elemInfo != null)
                {
                    RecordingEventArgs args = new RecordingEventArgs
                    {
                        EventType = eRecordingEvent.ElementRecorded
                    };

                    ElementActionCongifuration configArgs = new ElementActionCongifuration();

                    if (TestLocatorOutput(elemInfo, elemInfo.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByXPath)))
                    {
                        configArgs.LocateBy = eLocateBy.ByXPath;
                        configArgs.LocateValue = elemInfo.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByXPath).LocateValue;
                    }
                    else
                    {
                        configArgs.LocateBy = eLocateBy.ByXY;
                        configArgs.LocateValue = string.Format("{0},{1}", elemInfo.X, elemInfo.Y);
                    }
                    configArgs.ElementValue = elemInfo.Value;
                    configArgs.Type = elemInfo.ElementTypeEnum;
                    configArgs.LearnedElementInfo = elemInfo;
                    configArgs.Operation = ActUIElement.eElementAction.Click.ToString();

                    args.EventArgs = configArgs;
                    RecordingEvent?.Invoke(this, args);
                }
            }
            TapXY((int)x, (int)y);
        }

        public bool TestLocatorOutput(ElementInfo Elem, ElementLocator LocatorToTest)
        {
            if (LocateElementByLocator(LocatorToTest) is IWebElement elem)
            {
                return elem.Location.X == Elem.X && elem.Location.Y == Elem.Y;
            }

            return false;
        }



        public void PerformDrag(Point start, Point end, TimeSpan pressDuration, TimeSpan dragDuration)
        {
            DragAndDropByXY(start.X, start.Y, end.X, end.Y, pressDuration, dragDuration);

            if (IsRecording)
            {
                var mobDevAction = GetMobileActionforRecording(ActMobileDevice.eMobileDeviceAction.DragXYXY);

                mobDevAction.X1.Value = start.X.ToString();
                mobDevAction.Y1.Value = start.Y.ToString();

                mobDevAction.X2.Value = end.X.ToString();
                mobDevAction.Y2.Value = end.Y.ToString();

                mobDevAction.PressDuration.Value = pressDuration.TotalMilliseconds.ToString();
                mobDevAction.DragDuration.Value = dragDuration.TotalMilliseconds.ToString();
                RecordingOperations(mobDevAction);
            }
        }

        public void SwitchToLandscape()
        {
            try
            {
                Driver.Orientation = ScreenOrientation.Landscape;
            }
            finally
            {
                CalculateMobileDeviceScreenSizes();
            }
        }

        public void SwitchToPortrait()
        {
            try
            {
                Driver.Orientation = ScreenOrientation.Portrait;
            }
            finally
            {
                CalculateMobileDeviceScreenSizes();
            }
        }

        public eDeviceOrientation GetOrientation()
        {
            return (eDeviceOrientation)Enum.Parse(typeof(eDeviceOrientation), Driver.Orientation.ToString());
        }

        public Bitmap GetScreenShot(Tuple<int, int> setScreenSize = null, bool IsFullPageScreenshot = false)
        {
            try
            {
                // Get the screenshot as a byte array
                Tuple<int?, int?> customSize = null;
                if (setScreenSize != null)
                {
                    customSize = new Tuple<int?, int?>(setScreenSize.Item1, setScreenSize.Item2);
                }
                else
                {
                    customSize = GetUserCustomeScreenshotSize();
                }
                byte[] screenshotBytes = GetScreenshotImageFromDriver(customSize.Item1, customSize.Item2);

                if (screenshotBytes == null || screenshotBytes.Length == 0)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to capture screenshot.");
                    throw new Exception("Failed to capture screenshot.");
                }

                // Convert the byte array to a Bitmap
                using (MemoryStream ms = new MemoryStream(screenshotBytes))
                {
                    Bitmap bitmap = new Bitmap(ms);

                    // Optionally resize the bitmap if setScreenSize is provided
                    if (setScreenSize != null)
                    {
                        Bitmap resizedBitmap = new Bitmap(bitmap, setScreenSize.Item1, setScreenSize.Item2);
                        bitmap.Dispose();
                        return resizedBitmap;
                    }

                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                Reporter.ToLog(eLogLevel.ERROR, "Failed to capture screenshot.", ex);
                return null;
            }
        }

        XmlDocument pageSourceXml = null;
        string pageSourceString = null;

        public async Task<XmlNode> FindElementXmlNodeByXY(long pointOnMobile_X, long pointOnMobile_Y, bool IsAsyncCall = true)
        {
            try
            {
                //get screen elements nodes
                XmlNodeList ElmsNodes;
                // Do once?
                // if XMLSOurce changed we need to refresh
                if (IsAsyncCall)
                {
                    pageSourceString = await GetPageSource();
                }
                else
                {
                    pageSourceString = Driver.PageSource;
                }

                pageSourceXml = new XmlDocument();
                pageSourceXml.LoadXml(pageSourceString);

                ElmsNodes = pageSourceXml.SelectNodes("//*");

                ///get the selected element from screen
                if (ElmsNodes != null && ElmsNodes.Count > 0)
                {
                    //move to collection for getting last node which fits to bounds
                    ObservableList<XmlNode> ElmsNodesColc = [];
                    foreach (XmlNode elemNode in ElmsNodes)
                    {
                        //if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.iOS && elemNode.LocalName == "UIAWindow") continue;                        
                        //try { if (mDriver.DriverPlatformType == SeleniumAppiumDriver.ePlatformType.Android && elemNode.Attributes["focusable"].Value == "false") continue; }catch (Exception ex) { }
                        bool skipElement = false;
                        //if (FilterElementsChK.IsChecked == true)
                        //{
                        //    string[] filterList = FilterElementsTxtbox.Text.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        //    try
                        //    {
                        //        for (int indx = 0; indx < filterList.Length; indx++)
                        //            if (elemNode.Name.Contains(filterList[indx].Trim()) ||
                        //                   elemNode.LocalName.Contains(filterList[indx].Trim()))
                        //            {
                        //                skipElement = true;
                        //                break;
                        //            }
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); 
                        //    }
                        //}

                        if (!skipElement)
                        {
                            ElmsNodesColc.Add(elemNode);
                        }
                    }

                    Dictionary<XmlNode, long> foundElements = [];
                    foreach (XmlNode elementNode in ElmsNodesColc.Reverse())
                    {
                        //get the element location
                        long element_Start_X = -1;
                        long element_Start_Y = -1;
                        long element_Max_X = -1;
                        long element_Max_Y = -1;

                        switch (DevicePlatformType)
                        {
                            case eDevicePlatformType.Android:   // SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                try
                                {
                                    if (elementNode.Attributes["bounds"] != null)
                                    {
                                        string bounds = elementNode.Attributes["bounds"].Value;
                                        bounds = bounds.Replace("[", ",");
                                        bounds = bounds.Replace("]", ",");
                                        string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        if (boundsXY.Length == 4)
                                        {
                                            element_Start_X = Convert.ToInt64(boundsXY[0]);
                                            element_Start_Y = Convert.ToInt64(boundsXY[1]);
                                            element_Max_X = Convert.ToInt64(boundsXY[2]);
                                            element_Max_Y = Convert.ToInt64(boundsXY[3]);
                                        }
                                    }
                                    else
                                    {
                                        element_Start_X = -1;
                                        element_Start_Y = -1;
                                        element_Max_X = -1;
                                        element_Max_Y = -1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    element_Start_X = -1;
                                    element_Start_Y = -1;
                                    element_Max_X = -1;
                                    element_Max_Y = -1;
                                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {System.Reflection.MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                                }
                                break;

                            case eDevicePlatformType.iOS:    // SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                try
                                {
                                    if (elementNode.Attributes.Count > 0)
                                    {
                                        element_Start_X = Convert.ToInt64(elementNode.Attributes["x"].Value);
                                        element_Start_Y = Convert.ToInt64(elementNode.Attributes["y"].Value);
                                        element_Max_X = element_Start_X + Convert.ToInt64(elementNode.Attributes["width"].Value);
                                        element_Max_Y = element_Start_Y + Convert.ToInt64(elementNode.Attributes["height"].Value);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    element_Start_X = -1;
                                    element_Start_Y = -1;
                                    element_Max_X = -1;
                                    element_Max_Y = -1;
                                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {System.Reflection.MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                                }
                                break;
                        }


                        if (((pointOnMobile_X >= element_Start_X) && (pointOnMobile_X <= element_Max_X))
                                   && ((pointOnMobile_Y >= element_Start_Y) && (pointOnMobile_Y <= element_Max_Y)))
                        {
                            //object found                                
                            //return elementNode;
                            foundElements.Add(elementNode, ((element_Max_X - element_Start_X) * (element_Max_Y - element_Start_Y)));
                        }
                    }

                    //getting the small node size found
                    XmlNode foundNode = null;
                    long foundNodeSize = 0;
                    if (foundElements.Count > 0)
                    {
                        foundNode = foundElements.Keys.First();
                        foundNodeSize = foundElements.Values.First();
                    }
                    for (int indx = 0; indx < foundElements.Keys.Count; indx++)
                    {
                        if (foundElements.Values.ElementAt(indx) < foundNodeSize)
                        {
                            foundNode = foundElements.Keys.ElementAt(indx);
                            foundNodeSize = foundElements.Values.ElementAt(indx);
                        }
                    }
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                //Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }

        public VisualElementsInfo GetVisualElementsInfo()
        {
            throw new NotImplementedException();
        }

        public void ChangeAppWindowSize(int Width, int Height)
        {
            throw new NotImplementedException();
        }

        public async Task<ElementInfo> GetElementAtPoint(long ptX, long ptY)
        {
            if (AppType == eAppType.Web)
            {
                return await Task.Run(() => ((IVisualTestingDriver)mSeleniumDriver).GetElementAtPoint(ptX, ptY));
            }

            XmlNode foundNode = await FindElementXmlNodeByXY(ptX, ptY);

            return foundNode != null ? await GetElementInfoforXmlNode(foundNode) : null;
        }

        int mWindowWidth = 0;
        public int WindowWidth { get { return mWindowWidth; } }
        int mWindowHeight = 0;
        public int WindowHeight { get { return mWindowHeight; } }
        double mWindowScaleFactor = 0;
        public double WindowScaleFactor { get { return mWindowScaleFactor; } }

        private void CalculateMobileDeviceScreenSizes()
        {
            try
            {
                var windowSize = Driver.Manage().Window.Size;
                if (DevicePlatformType == eDevicePlatformType.iOS)
                {
                    var nativeSize = (Dictionary<string, object>)Driver.ExecuteScript("mobile: viewportRect");
                    mWindowScaleFactor = Convert.ToInt32(nativeSize["width"]) / windowSize.Width;
                }
                else
                {
                    mWindowScaleFactor = 1;
                }
                mWindowWidth = (int)(windowSize.Width * mWindowScaleFactor);
                mWindowHeight = (int)(windowSize.Height * mWindowScaleFactor);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Mobile Device Screen Sizes", ex);
            }
        }

        public override Point GetPointOnAppWindow(Point clickedPoint, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight)
        {
            Point pointOnMobile = new Point();

            double xRatio = (double)(SrcWidth / ActWidth);
            double yRatio = (double)(SrcHeight / ActHeight);

            pointOnMobile.X = (int)(clickedPoint.X * xRatio / WindowScaleFactor);
            pointOnMobile.Y = (int)(clickedPoint.Y * yRatio / WindowScaleFactor);

            return pointOnMobile;
        }

        public override bool SetRectangleProperties(ref Point ElementStartPoints, ref Point ElementMaxPoints, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight, ElementInfo clickedElementInfo)
        {
            double scale_factor_x, scale_factor_y;
            XmlNode rectangleXmlNode = clickedElementInfo.ElementObject as XmlNode;

            scale_factor_x = (double)(SrcWidth / ActWidth);
            scale_factor_y = (double)(SrcHeight / ActHeight);

            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:

                    if (AppType == eAppType.Web)
                    {
                        scale_factor_x = (SrcWidth * 3) / ActWidth;
                        scale_factor_y = (SrcHeight * 3) / ActHeight;

                        ElementStartPoints.X = (int)(ElementStartPoints.X * scale_factor_x);
                        ElementStartPoints.Y = (int)(ElementStartPoints.Y * scale_factor_y);

                        ElementMaxPoints.X = (int)(ElementMaxPoints.X * scale_factor_x);
                        ElementMaxPoints.Y = (int)(ElementMaxPoints.Y * scale_factor_y);
                    }
                    else
                    {

                        string bounds = rectangleXmlNode != null ? (rectangleXmlNode.Attributes["bounds"] != null ? rectangleXmlNode.Attributes["bounds"].Value : "") : "";
                        bounds = bounds.Replace("[", ",");
                        bounds = bounds.Replace("]", ",");
                        string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (boundsXY.Length == 4)
                        {
                            ElementStartPoints.X = (int)(Convert.ToInt64(boundsXY[0]) / scale_factor_x);
                            ElementStartPoints.Y = (int)(Convert.ToInt64(boundsXY[1]) / scale_factor_y);

                            ElementMaxPoints.X = (int)(Convert.ToInt64(boundsXY[2]) / scale_factor_x);
                            ElementMaxPoints.Y = (int)(Convert.ToInt64(boundsXY[3]) / scale_factor_y);
                        }
                    }
                    break;

                case eDevicePlatformType.iOS:
                    if (AppType == eAppType.Web)
                    {
                        ElementStartPoints.X = (int)(ElementStartPoints.X / scale_factor_x);
                        ElementStartPoints.Y = (int)(ElementStartPoints.Y / scale_factor_y);
                        ElementMaxPoints.X = (int)(ElementMaxPoints.X / scale_factor_x);
                        ElementMaxPoints.Y = (int)(ElementMaxPoints.Y / scale_factor_y);
                    }
                    else
                    {
                        string x = GetAttrValue(rectangleXmlNode, "x");
                        string y = GetAttrValue(rectangleXmlNode, "y");
                        string hgt = GetAttrValue(rectangleXmlNode, "height");
                        string wdth = GetAttrValue(rectangleXmlNode, "width");

                        ElementStartPoints.X = (int)((Convert.ToInt32(x) / scale_factor_x) * mWindowScaleFactor);
                        ElementStartPoints.Y = (int)((Convert.ToInt32(y) / scale_factor_y) * mWindowScaleFactor);

                        ElementMaxPoints.X = (int)(ElementStartPoints.X + Convert.ToInt32(Convert.ToInt32(wdth) / scale_factor_x) * mWindowScaleFactor);
                        ElementMaxPoints.Y = (int)(ElementStartPoints.Y + Convert.ToInt32(Convert.ToInt32(hgt) / scale_factor_y) * mWindowScaleFactor);
                    }

                    break;
            }
            return true;
        }

        public override double ScreenShotInitialZoom()
        {
            return 0.25;
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
            return false;
        }

        public List<eTabView> SupportedViews()
        {
            return [eTabView.Screenshot, eTabView.GridView, eTabView.PageSource];
        }

        public eTabView DefaultView()
        {
            return eTabView.Screenshot;
        }

        public string SelectionWindowText()
        {
            if (AppType == eAppType.Web)
            {
                return "Page:";
            }
            else
            {
                return "Window:";
            }
        }

        public void PerformScreenSwipe(eSwipeSide swipeSide, double swipeScale, TimeSpan swipeDuration)
        {
            SwipeScreen(swipeSide, swipeScale, swipeDuration);

            if (IsRecording)
            {
                ActMobileDevice mobAct = new ActMobileDevice();
                mobAct.SwipeScale.Value = swipeScale.ToString();
                mobAct.SwipeDuration.Value = swipeDuration.TotalMilliseconds.ToString();
                switch (swipeSide)
                {
                    case eSwipeSide.Up:
                        mobAct.MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeUp;
                        mobAct.Description = "Perform Swipe Up";
                        break;

                    case eSwipeSide.Down:
                        mobAct.MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeDown;
                        mobAct.Description = "Perform Swipe Down";
                        break;

                    case eSwipeSide.Right:
                        mobAct.MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeRight;
                        mobAct.Description = "Perform Swipe Right";
                        break;

                    case eSwipeSide.Left:
                        mobAct.MobileDeviceAction = ActMobileDevice.eMobileDeviceAction.SwipeLeft;
                        mobAct.Description = "Perform Swipe Left";
                        break;
                }

                RecordingOperations(mobAct);
            }
        }

        public void PerformSendKey(string key)
        {
            IWebElement currentElement = Driver.SwitchTo().ActiveElement();
            currentElement.SendKeys(key);
        }

        public async Task<object> GetPageSourceDocument(bool ReloadHtmlDoc)
        {
            if (AppType == eAppType.Web)
            {
                return await ((IWindowExplorer)mSeleniumDriver).GetPageSourceDocument(ReloadHtmlDoc);
            }

            if (ReloadHtmlDoc)
            {
                pageSourceXml = null;
            }

            if (pageSourceXml == null)
            {
                pageSourceXml = new XmlDocument();
                pageSourceString = await GetPageSource();
                pageSourceXml.LoadXml(pageSourceString);
            }

            return pageSourceXml;
        }

        public string GetCurrentPageSourceString()
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).GetCurrentPageSourceString();
            }

            return Driver.PageSource;
        }

        public void OpenDeviceSettings()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver)Driver).PressKeyCode(Convert.ToInt32(ActMobileDevice.ePressKey.Keycode_SETTINGS));
                    break;
                case eDevicePlatformType.iOS:
                    Driver.ActivateApp("com.apple.Preferences");
                    break;
            }
        }

        public void OpenDeviceExternalView()
        {
            //open URL in browser
            try
            {
                //build the URL to use
                Uri serverUri = new Uri(this.AppiumServer);
                var uribuilder = new UriBuilder
                {
                    Scheme = serverUri.Scheme,
                    Port = serverUri.Port,
                    Host = serverUri.Host,
                    //Path = string.Format(@"/integration8/en/#/remote?deviceId={0}", GetDeviceUDID())
                };
                string deviceViewUrl = string.Format(@"{0}integration8/en/#/remote?deviceId={1}", uribuilder.Uri.ToString(), GetDeviceUDID().ToString());

                // Launch the browser with the URL
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = deviceViewUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to open the external device URL in browser", ex);
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Failed to open the external device URL in browser, Error: " + ex.Message);
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

        public IWebDriver GetWebDriver()
        {
            return Driver;
        }

        public Dictionary<string, string> GetDeviceCPUInfo()
        {
            return GetDeviceMetricsOverApiAsDict("cpuinfo").Result;
        }

        public Dictionary<string, string> GetDeviceMemoryInfo()
        {
            return GetDeviceMetricsOverApiAsDict("memoryinfo").Result;
        }

        public async Task<string> GetDeviceNetworkInfo()
        {
            return GetDeviceMetricOverAPI("networkinfo").Result;
            //string url = this.AppiumServer + "/session/" + Driver.SessionId + "/appium/getPerformanceData";
            //string package = GetCurrentPackage();
            //if (!string.IsNullOrEmpty(package))
            //{
            //    string requestBody = "{\"packageName\": \"" + package + "\", \"dataType\": \"networkinfo\"}";
            //    restClient = new RestClient(url);
            //    string response = await SendRestRequestAndGetResponse(url, requestBody).ConfigureAwait(false);
            //    if (response.Contains("error"))
            //    {
            //        return null;
            //    }
            //    return response;
            //}
            //else
            //{
            //    return null;
            //}
        }

        public Dictionary<string, string> GetDeviceBatteryInfo()
        {
            return GetDeviceMetricsOverApiAsDict("batteryinfo").Result;
        }

        public Dictionary<string, object> GetDeviceGeneralInfo()
        {
            try
            {
                return (Dictionary<string, object>)Driver.ExecuteScript("mobile: deviceInfo");
            }
            catch (Exception e)
            {
                return [];
            }
        }

        private async Task<Dictionary<string, string>> GetDeviceMetricsOverApiAsDict(string dataType, string AppPackage = "")
        {
            string response = await GetDeviceMetricOverAPI(dataType, AppPackage);
            if (response == null || response.Contains("error"))
            {
                return [];
            }
            try
            {
                Dictionary<string, string> dict = StringToDictionary(response);
                return dict;
            }
            catch (Exception e)
            {
                return [];
            }
        }

        private async Task<string> GetDeviceMetricOverAPI(string dataType, string AppPackage = "")
        {
            string url = this.AppiumServer + "/session/" + Driver.SessionId + "/appium/getPerformanceData";
            if (string.IsNullOrEmpty(AppPackage))
            {
                AppPackage = GetCurrentPackage();
            }
            if (!string.IsNullOrEmpty(AppPackage))
            {
                string requestBody = "{\"packageName\": \"" + AppPackage + "\", \"dataType\": \"" + dataType + "\"}";
                restClient = new RestClient(url);
                return await SendRestRequestAndGetResponse(url, requestBody).ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        private async Task<string> SendRestRequestAndGetResponse(string api, string requestBody)
        {
            try
            {
                Method method = Method.Post;
                RestRequest restRequest = new RestRequest(api, method) { RequestFormat = DataFormat.Json }.AddJsonBody(requestBody);
                RestResponse response = await restClient.ExecuteAsync(restRequest);
                if (response.IsSuccessful)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Successfully sent " + api);
                    return response.Content;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "Failed to send " + api + "Response: " + response.Content);
                    return response.Content;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception when sending " + api, ex);
                return "Unable to connect to: " + api;
            }
        }

        private Dictionary<string, string> StringToDictionary(string response)
        {
            response = response[10..^2];
            List<string> responseList = [];
            while (!string.IsNullOrEmpty(response))
            {
                int startIndex = response.IndexOf("[") + 1;
                int endIndex = response.IndexOf("]");
                responseList.Add(response[startIndex..endIndex]);
                if (endIndex + 2 > response.Length)
                {
                    response = response[(endIndex + 1)..];

                }
                else
                {
                    response = response[(endIndex + 2)..];
                }
            }
            responseList[0] = responseList[0].Replace("\"", "");
            string[] keyArray = responseList[0].Split(',');
            responseList[1] = responseList[1].Replace("\"", "");
            string[] valueArray = responseList[1].Split(',');
            Dictionary<string, string> dict = [];
            for (int i = 0; i < keyArray.Length; i++)
            {
                dict.Add(keyArray[i], valueArray[i]);
            }
            return dict;
        }

        public Dictionary<string, string> GetDeviceActivityAndPackage()
        {
            if (DevicePlatformType == eDevicePlatformType.Android)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    { "Activity",string.Concat(((AndroidDriver)Driver).CurrentPackage, ((AndroidDriver)Driver).CurrentActivity) },
                    { "Package", ((AndroidDriver)Driver).CurrentPackage }
                };
                return dict;
            }
            else
            {
                Dictionary<string, string> dict2 = new Dictionary<string, string>
                {
                    { "Bundle ID", (string)((IOSDriver)Driver).ExecuteScript("mobile: currentApp") }
                };
                return dict2;
            }
        }

        public async Task<bool> IsRealDeviceAsync()
        {
            try
            {
                string url = this.AppiumServer + "/session/" + Driver.SessionId + "/appium/device/network_speed";
                string requestBody = "{\"netspeed\": \"lte\"}";
                restClient = new RestClient(url);
                string response = await SendRestRequestAndGetResponse(url, requestBody).ConfigureAwait(false);
                if (response.Contains("error"))
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
                return true;
            }
        }

        public string GetDeviceUDID()
        {
            object udid = null;
            if (Driver.SessionDetails != null)
            {
                Driver.SessionDetails.TryGetValue("udid", out udid);
            }
            if (string.IsNullOrEmpty((string)udid))
            {
                return null;
            }
            else
            {
                return (string)udid;
            }
        }

        public Bitmap GetElementScreenshot(Act act)
        {
            throw new NotImplementedException();
        }

        public string GetAgentAppName()
        {
            return this.Platform.ToString();
        }

        public string GetViewport()
        {
            return Driver.Manage().Window.Size.ToString();
        }

        public ObservableList<ElementLocator> GetElementFriendlyLocators(ElementInfo ElementInfo, PomSetting pomSetting = null)
        {
            throw new NotImplementedException();
        }

        public string GetAppPackageNameByOs()
        {
            try
            {
                if (Driver is AndroidDriver)
                {
                    return "package";
                }
                else if (Driver is IOSDriver)
                {
                    return "bundleId";
                }
                else
                {
                    throw new InvalidOperationException($"Unsupported driver type");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get app identifier attribute name: {ex.Message}");
            }
        }

        public string SetFilePath(string FileType, string FilePath, string FileName)
        {
            if (!string.IsNullOrEmpty(FilePath) && Directory.Exists(FilePath))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = Path.GetFileName(FilePath);
                string targetFile = string.IsNullOrEmpty(fileName) ? Path.Combine(FilePath, $"{FileName}_{timestamp}.{FileType}") : $"{FilePath}.{FileType}";
                return targetFile;
            }
            else
            {
                throw new InvalidOperationException($"An error occurred while running script: Got invalid Directory or empty folder");
            }
        }
        public object OpenDeeplink(string appLink, string appPackage, string osType)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(appLink))
                throw new ArgumentException("Deeplink URL cannot be null or empty", nameof(appLink));

            if (string.IsNullOrEmpty(appPackage))
                throw new ArgumentException("App package/bundle identifier cannot be null or empty", nameof(appPackage));

            if (string.IsNullOrEmpty(osType))
                throw new ArgumentException("OS type cannot be null or empty", nameof(osType));
            try
            {


                var args = new Dictionary<string, string>
                {
                    { "url", appLink }, { osType, appPackage }
                };
                return Driver is AndroidDriver ? ((AndroidDriver)Driver).ExecuteScript("mobile:deepLink", args) : ((IOSDriver)Driver).ExecuteScript("mobile:deepLink", args);

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to open deeplink '{appLink}': {ex.Message}", ex);
            }
        }

        public bool IsDeviceKeyboardVisible()
        {
            try
            {
                return Driver?.IsKeyboardShown() ?? throw new InvalidOperationException("Driver is not initialized");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check keyboard visibility: {ex.Message}", ex);
            }
        }

        public bool IsDeviceLocked()
        {
            try
            {
                return Driver != null
                    ? (bool)Driver.ExecuteScript("mobile:isLocked")
                    : throw new InvalidOperationException("Driver is not initialized");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check device lock state: {ex.Message}", ex);
            }
        }

        public bool IsDeviceAppInstalled(string appPackage)
        {
            if (string.IsNullOrEmpty(appPackage))
                throw new ArgumentException("App package cannot be null or empty", nameof(appPackage));

            try
            {
                return Driver?.IsAppInstalled(appPackage) ?? throw new InvalidOperationException("Driver is not initialized");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check if app '{appPackage}' is installed: {ex.Message}", ex);
            }
        }

        public void RemoveDeviceApp(string appPackage)
        {
            if (string.IsNullOrEmpty(appPackage))
                throw new ArgumentException("App package cannot be null or empty", nameof(appPackage));

            try
            {
                if (Driver == null)
                    throw new InvalidOperationException("Driver is not initialized");

                Driver.RemoveApp(appPackage);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove app '{appPackage}': {ex.Message}", ex);
            }
        }

        public AppState QueryAppState(string appId)
        {
            if (string.IsNullOrEmpty(appId))
                throw new ArgumentException("App ID cannot be null or empty", nameof(appId));

            try
            {
                if (Driver == null)
                    throw new InvalidOperationException("Driver is not initialized");

                return Driver is AndroidDriver
                ? ((AndroidDriver)Driver).GetAppState(appId)
                : Driver is IOSDriver
                    ? ((IOSDriver)Driver).GetAppState(appId)
                    : throw new InvalidOperationException($"Unsupported driver type");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to query app state for '{appId}': {ex.Message}", ex);
            }
        }

        public void RotateSimulation(string orientation)
        {
            if (string.IsNullOrEmpty(orientation))
                throw new ArgumentException("Orientation cannot be null or empty", nameof(orientation));
            try
            {
                switch (orientation.ToLowerInvariant())
                {
                    case "landscape":
                        SwitchToLandscape();
                        break;
                    case "portrait":
                        SwitchToPortrait();
                        break;
                    default:
                        throw new ArgumentException($"Invalid orientation: '{orientation}'. Expected 'Landscape' or 'Portrait'", nameof(orientation));
                }
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new InvalidOperationException($"Failed to rotate device to {orientation}: {ex.Message}", ex);
            }
        }

        public void RunScriptOnDevice(string script)
        {
            if (string.IsNullOrEmpty(script))
                throw new ArgumentException("Script cannot be null or empty", nameof(script));

            try
            {
                if (Driver == null)
                    throw new InvalidOperationException("Driver is not initialized");

                Driver.ExecuteScript(script);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to execute script: {ex.Message}", ex);
            }
        }

        public void StartDeviceScreenRecording()
        {
            try
            {
                if (Driver is AndroidDriver)
                {
                    var androidOptions = new AndroidStartScreenRecordingOptions().WithTimeLimit(TimeSpan.FromSeconds(1800)).WithBitRate(4000000).WithVideoSize("720x1280"); // max duration recording: 30 min
                    ((AndroidDriver)Driver).StartRecordingScreen(androidOptions);
                }
                else if (Driver is IOSDriver)
                {
                    {
                        var iosOptions = new IOSStartScreenRecordingOptions()
                            .WithTimeLimit(TimeSpan.FromSeconds(1800)) // Set the same time limit
                            .WithVideoType("h264") // Use "h264" for efficient compression
                            .WithVideoQuality(IOSStartScreenRecordingOptions.VideoQuality.HIGH) // High quality for better resolution
                            .WithVideoScale("720:1280"); // Scale the video to match Android's resolution
                        ((IOSDriver)Driver).StartRecordingScreen(iosOptions);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while starting recording screen: {ex.Message}");
            }
        }

        public string StopRecordingScreen(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Local file path cannot be null or empty", nameof(path));
            try
            {
                string targetFile = SetFilePath("mp4", path, "Mobile_Recording");
                string videoBase64 = string.Empty;
                if (Driver is AndroidDriver)
                {
                    videoBase64 = ((AndroidDriver)Driver).StopRecordingScreen();
                }
                else
                {
                    videoBase64 = ((IOSDriver)Driver).StopRecordingScreen();
                }

                if (!string.IsNullOrEmpty(videoBase64))
                {
                    byte[] videoBytes = Convert.FromBase64String(videoBase64);
                    File.WriteAllBytes(targetFile, videoBytes); //"format mp4"
                }

                return targetFile;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while stop recording screen: {ex.Message}");
            }
        }

        public void HideKeyboard()
        {
            try
            {
                if (Driver is AndroidDriver)
                {
                    ((AndroidDriver)Driver).HideKeyboard("Done");
                }
                else if (Driver is IOSDriver)
                {
                    ((IOSDriver)Driver).HideKeyboard();
                }
                else
                {
                    throw new NotSupportedException($"Driver type is not supported for keyboard operations");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to hide the keyboard: {ex.Message}", ex);
            }
        }

        public void PushFileToDevice(string LocalFilePath, string DeviceTargerFolder)
        {
            if (string.IsNullOrEmpty(LocalFilePath))
                throw new ArgumentException("Local file path cannot be null or empty", nameof(LocalFilePath));

            if (string.IsNullOrEmpty(DeviceTargerFolder))
                throw new ArgumentException("Device target folder cannot be null or empty", nameof(DeviceTargerFolder));

            bool isDirectory = Directory.Exists(LocalFilePath);
            bool isFile = File.Exists(LocalFilePath);

            if (!isDirectory && !isFile)
                throw new ArgumentException($"Path not found: {LocalFilePath}", nameof(LocalFilePath));

            // Create array of strings to store file paths to be processed
            string[] FilePathList;

            if (isDirectory)
            {
                FilePathList = Directory.GetFiles(LocalFilePath);
            }
            else
            {
                FilePathList = new string[] { LocalFilePath };
            }

            foreach (var filePath in FilePathList)
            {
                var remoteFilePath = Path.Combine(DeviceTargerFolder, Path.GetFileName(filePath));
                try
                {
                    Driver.PushFile(remoteFilePath, new FileInfo(filePath));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to push file {filePath}: {ex.Message}", ex);
                }
            }
        }

        public void PullFileFromDevice(string DeviceFilePath, string LocalFolderPath)
        {
            if (string.IsNullOrEmpty(DeviceFilePath))
                throw new ArgumentException("Device file path cannot be null or empty", nameof(DeviceFilePath));

            if (string.IsNullOrEmpty(LocalFolderPath))
                throw new ArgumentException("Local folder path cannot be null or empty", nameof(LocalFolderPath));

            var remoteFilePath = Path.Combine(LocalFolderPath, Path.GetFileName(DeviceFilePath));

            try
            {
                byte[] FileContent = Driver.PullFile(DeviceFilePath);

                // Save the file content to the local file path
                System.IO.File.WriteAllBytes(remoteFilePath, FileContent);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to pull file from device: {ex.Message}", ex);
            }
        }

        public void SetClipboardText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }
            try
            {
                if (Driver is IOSDriver)
                {
                    ((IOSDriver)Driver).SetClipboardText(text, "");
                }
                else if (Driver is AndroidDriver)
                {
                    ((AndroidDriver)Driver).SetClipboardText(text, "");
                }
                else
                {
                    throw new NotSupportedException($"Driver type is not supported for clipboard operations");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set clipboard text: {ex.Message}", ex);
            }
        }

        public void GetSpecificPerformanceData(string appPackage, string specificData, ActMobileDevice act)
        {
            if (string.IsNullOrEmpty(appPackage))
            {
                throw new ArgumentException("App package name cannot be null or empty", nameof(appPackage));
            }

            if (string.IsNullOrEmpty(specificData))
            {
                throw new ArgumentException("Performance data type cannot be null or empty", nameof(specificData));
            }

            if (act == null)
            {
                throw new ArgumentNullException(nameof(act), "ActMobileDevice cannot be null");
            }
            try
            {
                if (Driver is AndroidDriver)
                {
                    IList<object> perfData = ((AndroidDriver)Driver).GetPerformanceData(appPackage, specificData, 5);
                    var dict = new Dictionary<string, object>();
                    var keys = (IList<object>)perfData[0]; // keys data
                    var values = (IList<object>)perfData[1]; // values data
                    for (int i = 0; i < keys.Count; i++)
                    {
                        string key = keys[i].ToString();
                        object value = values[i];
                        dict[key] = value;
                    }
                    foreach (var entry in dict)
                    {
                        if (entry.Key != null)
                        {
                            string valueStr = entry.Value?.ToString() ?? string.Empty; // Convert null value to empty string
                            act.AddOrUpdateReturnParamActual(entry.Key.ToString(), valueStr);
                        }
                    }
                }
                else
                {
                    throw new InvalidOperationException("This method is only supported for Android devices");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve performance data: {ex.Message}", ex);
            }
        }

        public string GetDeviceLogs(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Device target folder cannot be null or empty", nameof(path));
            try
            {
                string targetFile = SetFilePath("txt", path, "DeviceLogs");
                // Get device logs
                var logEntries = Driver.Manage().Logs.GetLog("logcat").ToList();
                // Create and write to the file
                using (StreamWriter writer = new StreamWriter(targetFile, append: true))
                {
                    foreach (var logEntry in logEntries)
                    {
                        writer.WriteLine($"{logEntry.Timestamp}: {logEntry.Message}");
                    }
                }
                return targetFile;
            }
            catch (Exception ex)
            {
                // Handle exceptions gracefully
                throw new ApplicationException($"Failed to retrieve device logs: {ex.Message}", ex);
            }
        }
        public void ChosenKey(string charToFind)
        {
            // Update XPath for numbers/symbols/Alpha/space
            // Handle special cases for single quote and double quote
            var specialChars = new Dictionary<string, string>
            {
                 { "\'", "\'" },        // Single quote
                { "\"", "\"" },         // Double quote
                { "&", "ampersand" },   // Ampersand
                {" ", "space" },        // Space 
            };

            string searchValue;

            if (specialChars.TryGetValue(charToFind, out searchValue))
            {
                // Use the mapped value for special characters
                searchValue = specialChars[charToFind];
            }
            else
            {
                // For standard characters, use the character itself
                searchValue = charToFind;
            }

            // Build XPath using the determined search value
            string xpath = char.Parse(charToFind) == '\'' ?
                $"//*[@name=\"{searchValue}\" or @label=\"{searchValue}\" or @value=\"{searchValue}\" or @hint=\"{searchValue}\" or @content-desc=\"{searchValue}\"]"
                : $"//*[@name='{searchValue}' or @label='{searchValue}' or @value='{searchValue}' or @hint='{searchValue}' or @content-desc='{searchValue}']";

            ((IOSDriver)Driver).FindElements(By.XPath(xpath))[0].Click();
        }
        public void switchToNumbersOrSymbols()
        {
            var numbersKey = ((IOSDriver)Driver).FindElement(By.XPath("//XCUIElementTypeKey[@name='123' or @label='numbers']"));
            bool isNumbersActive = numbersKey.GetAttribute("enabled") == "true";
            if (isNumbersActive)
            {
                numbersKey.Click();
            }
        }
        public void switchToLetters(char c)
        {
            if (c != '\'')
            {
                ((IOSDriver)Driver).FindElement(By.XPath("//XCUIElementTypeKey[@name='more' or @label='ABC']")).Click();
            }
        }
        public void switchToSpecialKey()
        {
            var specialKey = ((IOSDriver)Driver).FindElement(By.XPath("//XCUIElementTypeButton[@name='shift' or @label='symbols']"));
            bool isSpecialActive = specialKey.GetAttribute("enabled") == "true";
            if (isSpecialActive)
            {
                specialKey.Click();
            }
        }
        public void switchToCapitalLetter(char c)
        {
            var shiftKey = ((IOSDriver)Driver).FindElement(By.XPath("//*[contains(@label,'shift') or contains(@value,'shift')]"));
            bool isShiftActive = shiftKey.GetAttribute("selected") == "true";
            if ((c.IsAlphaUpper() && !isShiftActive) || (Char.IsLower(c) && isShiftActive))
            {
                shiftKey.Click();
            }
        }
        public void SelectSubKeyboard(char c)
        {
            char[] FirstSpecialCharacters = ['-', '/', ':', ';', '(', ')', '$', '&', '@', '"', '\''];
            char[] SecondSpecialCharacters = { '|', '~', '<', '>', '^', '+', '=', '¥', '£', '€' };


            if (c == ' ')
            {
                ChosenKey(c.ToString());
            }

            // Handle numbers or special characters by switching keyboards if necessary
            else if (char.IsDigit(c) || Array.Exists(FirstSpecialCharacters, element => element == c))
            {
                // Switch to numbers/symbols keyboard
                switchToNumbersOrSymbols();
                ChosenKey(c.ToString());
                switchToLetters(c);
            }

            else if (char.IsPunctuation(c) || Array.Exists(SecondSpecialCharacters, element => element == c))
            {
                // Switch to numbers/symbols keyboard
                switchToNumbersOrSymbols();

                // Switch to secondary symbols keyboard (#+=)
                switchToSpecialKey();
                ChosenKey(c.ToString());
                switchToLetters(c);
            }

            else if (Char.IsLetter(c))
            {
                // Switch to capital letters keyboard
                switchToCapitalLetter(c);
                ChosenKey(c.ToString());
            }
        }

        public void TypeUsingIOSkeyboard(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }
            try
            {
                if (Driver is IOSDriver)
                {
                    foreach (char c in text)
                    {
                        SelectSubKeyboard(c);
                    }
                }
                else if (Driver is AndroidDriver)
                {
                    ((AndroidDriver)Driver).ExecuteScript("mobile: type", new Dictionary<string, object> { { "text", text } });
                }
                else
                {
                    throw new NotSupportedException($"Driver type is not supported for keyboard typing");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while typing text: {ex.Message}");
            }
        }
        public string GetDeviceOSType()
        {
            if (Driver is AndroidDriver)
            {
                return "Android";
            }
            else if (Driver is IOSDriver)
            {
                return "iOS";
            }
            else
            {
                throw new InvalidOperationException($"Failed to determine the device OS type: the driver type is not AndroidDriver or IOSDriver");
            }

        }
        public void ClearAppData(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
            {
                throw new ArgumentException("App ID cannot be null or empty", nameof(appId));
            }

            try
            {
                if (Driver is IOSDriver)
                {
                    throw new NotSupportedException($"Driver type is not supported for clearing app data");
                }
                else
                {
                    ((AndroidDriver)Driver).ExecuteScript("mobile: clearApp", new Dictionary<string, object> { { "appId", appId } });
                }

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to clear app data for '{appId}': {ex.Message}", ex);
            }
        }

        public string GetClipboardText()
        {
            if (Driver is IOSDriver)
            {
                return ((IOSDriver)Driver).GetClipboardText();
            }
            else if (Driver is AndroidDriver)
            {
                return ((AndroidDriver)Driver).GetClipboardText();
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void GetAppId(ActMobileDevice act)
        {
            if (Driver is AndroidDriver)
            {
                try
                {
                    string appPackage = ((AndroidDriver)Driver).CurrentPackage;
                    act.AddOrUpdateReturnParamActual("App Package", appPackage);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while retrieving the app package: {ex.Message}", ex);
                }
            }
            else if (Driver is IOSDriver)
            {
                try
                {
                    var bundleId = ((Dictionary<string, object>)Driver.ExecuteScript("mobile: activeAppInfo"))["bundleId"].ToString();
                    act.AddOrUpdateReturnParamActual("Bundle Id", bundleId);
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while retrieving the bundle ID: {ex.Message}", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void ScreenSize(ActMobileDevice act)
        {
            try
            {
                Size sizeWindow = Driver.Manage().Window.Size;
                act.AddOrUpdateReturnParamActual("Width", sizeWindow.Width.ToString());
                act.AddOrUpdateReturnParamActual("Height", sizeWindow.Height.ToString());
            }
            catch (Exception ex)
            {
                // Wrap and rethrow for better error context
                throw new InvalidOperationException("Failed to retrieve or store screen dimensions.", ex);
            }

        }
        public void OpenNotificationsPanel()
        {

            if (Driver is AndroidDriver)
            {
                ((AndroidDriver)Driver).OpenNotifications();
            }
            else if (Driver is IOSDriver)
            {
                Dictionary<string, object> swipeArgs = new Dictionary<string, object>();
                swipeArgs.Add("direction", "down");
                Driver.ExecuteScript("mobile: swipe", swipeArgs);
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public string GetDeviceTime()
        {
            try
            {
                return (string)Driver.ExecuteScript("mobile: getDeviceTime");
            }
            catch (Exception ex)
            {
                // General error handling
                throw new InvalidOperationException("An unexpected error occurred while fetching device time.", ex);
            }
        }

        public void LockForDuration(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                throw new ArgumentException("Time parameter cannot be null or empty.", nameof(time));
            }

            // Check if time contains only digits
            if (!time.All(char.IsDigit))
            {
                throw new ArgumentException("Time must contain only numeric characters.", nameof(time));
            }

            if (!int.TryParse(time, out int seconds) || seconds < 0)
            {
                throw new ArgumentException("Time must be a non-negative integer (in seconds).", nameof(time));
            }

            if (Driver is AndroidDriver)
            {
                ((AndroidDriver)Driver).Lock();
                Thread.Sleep(1000 * int.Parse(time));
                ((AndroidDriver)Driver).Unlock("none", "none");
            }
            else if (Driver is IOSDriver)
            {
                ((IOSDriver)Driver).Lock();
                Thread.Sleep(1000 * int.Parse(time));
                ((IOSDriver)Driver).Unlock();
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void GetSettings(ActMobileDevice act)
        {
            try
            {
                Dictionary<string, object> settings;

                if (Driver is AndroidDriver)
                {
                    settings = ((AndroidDriver)Driver).Settings;
                }
                else
                {
                    settings = ((IOSDriver)Driver).Settings;
                }
                foreach (KeyValuePair<string, object> entry in settings)
                {
                    if (entry.Key != null)
                    {
                        string valueStr = entry.Value?.ToString() ?? string.Empty; // Convert null value to empty string
                        act.AddOrUpdateReturnParamActual(entry.Key.ToString(), valueStr);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve device settings.", ex);
            }
        }
        public void GetDeviceSystemBars(ActMobileDevice act) //working only on android, extract key,value
        {
            Dictionary<string, object> systemBars;

            if (Driver is AndroidDriver)
            {
                systemBars = ((AndroidDriver)Driver).ExecuteScript("mobile: getSystemBars") as Dictionary<string, object>;
                foreach (var bar in systemBars)
                {
                    string barType = bar.Key;
                    var barProperties = bar.Value as Dictionary<string, object>;
                    act.AddOrUpdateReturnParamActual(barType.ToString(), "");
                    foreach (var entry in barProperties)
                    {
                        if (entry.Key != null)
                        {
                            string valueStr = entry.Value?.ToString() ?? string.Empty; // Convert null value to empty string
                            act.AddOrUpdateReturnParamActual(entry.Key.ToString(), valueStr);
                        }
                    }
                }
            }
            else
            {
                throw new NotSupportedException("Failed to retrieve system bars information.");
            }
        }
        public void StartNetworkCapture() // working on IOS, need to check the 'network' name with xcrun xctrace list templates run command via terminal
        {
            if (Driver is AndroidDriver)
            {
                ((AndroidDriver)Driver).ExecuteScript("mobile: startLogsBroadcast", new Dictionary<string, object> {
                { "options", new Dictionary<string, object>{{ "logType", "logcat" } }}});
                Thread.Sleep(1000 * 10);
            }

            else if (Driver is IOSDriver)
            {
                ((IOSDriver)Driver).ExecuteScript("mobile: startPerfRecord", new Dictionary<string, object>{
                { "profileName", "Network" }});
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public string StopNetworkCapture() // working on IOS, need to check the 'network' name with xcrun xctrace list templates run command via terminal
        {
            if (Driver is AndroidDriver)
            {
                string logNetwork = (string)((AndroidDriver)Driver).ExecuteScript("mobile: stopLogsBroadcast");
                return logNetwork;
            }
            else if (Driver is IOSDriver)
            {
                string logsNetwork = (string)((IOSDriver)Driver).ExecuteScript("mobile: stopPerfRecord", new Dictionary<string, object>{
                { "profileName", "Network" }});
                return logsNetwork;
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }

        public void ToggleLocationServices()
        {
            if (Driver is AndroidDriver)
            {
                ((AndroidDriver)Driver).ToggleLocationServices();
            }
            else if (Driver is IOSDriver)
            {

                throw new NotSupportedException("Toggling location services is not directly supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }

        }
        public void ToggleData() //not working, no SIM to the device
        {
            if (Driver is AndroidDriver)
            {
                ConnectionType currentConnection = ((AndroidDriver)Driver).ConnectionType;

                if (currentConnection.HasFlag(ConnectionType.DataOnly))
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.None;   // disable
                }
                else
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.DataOnly; //enable
                }
            }
            else if (Driver is IOSDriver)
            {

                throw new NotSupportedException("Toggling data services is not directly supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }

        public void ToggleAirplaneMode()
        {
            if (Driver is AndroidDriver)
            {
                ConnectionType currentConnection = ((AndroidDriver)Driver).ConnectionType;

                if (currentConnection.HasFlag(ConnectionType.AirplaneMode))
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.None;   // disable
                }
                else
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.AirplaneMode; //enable
                }
            }
            else if (Driver is IOSDriver)
            {

                throw new NotSupportedException("Toggle Airplane Mode is not directly supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void ToggleWifi()
        {
            if (Driver is AndroidDriver)
            {
                ConnectionType currentConnection = ((AndroidDriver)Driver).ConnectionType;

                if (currentConnection.HasFlag(ConnectionType.WifiOnly))
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.None;   // disable
                }
                else
                {
                    ((AndroidDriver)Driver).ConnectionType = ConnectionType.WifiOnly; //enable
                }
            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("Toggle Wifi is not directly supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public bool IsIMEActive()
        {
            if (Driver is AndroidDriver)
            {
                return Driver.IsIMEActive();
            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("IME Active is not supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public string GetIMEActiveEngine()
        {
            if (Driver is AndroidDriver)
            {
                return Driver.GetIMEActiveEngine();
            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("IME Active is not supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void StartActivity(string appPackage, string appActivity)// working on android   com.facebook.katana // com.facebook.katana.LoginActivity(required automatically details login)
        {
            // com.google.android.youtube // .HomeActivity

            if (Driver is AndroidDriver)
            {
                ((AndroidDriver)Driver).StartActivity(appPackage, appActivity);

            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("Start Activity is not supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
        public void GetGeoLocation(ActMobileDevice act) // working only on android(required connecting to WIFI) , for ios need to find "WebDriverAgentRunner" in location services screen and set its location access to "Always."
        {
            try
            {
                Location location = Driver switch
                {
                    AndroidDriver => ((AndroidDriver)Driver).Location,
                    IOSDriver => ((IOSDriver)Driver).Location,
                    _ => throw new InvalidOperationException("Unsupported driver type")
                };
                act.AddOrUpdateReturnParamActual("Latitude", location.Latitude.ToString());
                act.AddOrUpdateReturnParamActual("longitude", location.Longitude.ToString());
                act.AddOrUpdateReturnParamActual("Altitude", location.Altitude.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve or store geolocation data.", ex);
            }
        }

        public void GrantAppPermission(string appPackage, string permission, bool grant = true)
        {
            if (string.IsNullOrEmpty(appPackage) || appPackage == "default")
            {
                throw new ArgumentException("Invalid app package name", nameof(appPackage));
            }
            if (string.IsNullOrEmpty(permission) || string.IsNullOrEmpty(appPackage))
            {
                throw new ArgumentException("Permission cannot be null or empty", nameof(permission));
            }

            if (Driver is AndroidDriver)
            {
                if (permission.Contains("android.permission."))
                {
                    ((AndroidDriver)Driver).ExecuteScript("mobile: changePermissions", new Dictionary<string, object>{ { "action", grant ? "grant" : "revoke" },{ "appPackage", appPackage },
                    { "permissions", new[] { permission } }});
                }

                else
                {
                    throw new InvalidOperationException($"Failed to {(grant ? "grant" : "revoke")} permission to '{appPackage}' with command '{permission}'");
                }
            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("Grant App Permission services are not directly supported on iOS");
            }
        }
        public void SendAppToBackground(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
            {
                throw new ArgumentException("Time parameter cannot be null or empty.", nameof(time));
            }
            if (!time.All(char.IsDigit))
            {
                throw new ArgumentException("Time must contain only numeric characters.", nameof(time));
            }
            if (!int.TryParse(time, out int seconds) || seconds < 0)
            {
                throw new ArgumentException("Time must be a non-negative integer (in seconds).", nameof(time));
            }
            try
            {
                Driver.BackgroundApp(TimeSpan.FromSeconds(int.Parse(time)));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send the app to the background.", ex);
            }

        }
        public void SetNetworkConnection()
        {
            if (Driver is AndroidDriver)
            {
                ToggleWifi();
                ToggleAirplaneMode();
                ToggleData();
                ToggleLocationServices();
            }
            else if (Driver is IOSDriver)
            {
                throw new NotSupportedException("Set Connection for Wifi/Airplane Mode/Data/Location services are not directly supported on iOS");
            }
            else
            {
                throw new InvalidOperationException("Unsupported driver type");
            }
        }
    }

}