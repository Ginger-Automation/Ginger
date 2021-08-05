#region License
/*
Copyright © 2014-2021 European Support Limited

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
Copyright © 2014-2021 European Support Limited

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
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Amdocs.Ginger.CoreNET
{
    public class GenericAppiumDriver : DriverBase, IWindowExplorer, IRecord, IDriverWindow, IMobileDriverWindow, IVisualTestingDriver
    {
        public override ePlatformType Platform { get { return ePlatformType.Mobile; } }

        public override string GetDriverConfigsEditPageName(Agent.eDriverType driverSubType = Agent.eDriverType.NA)
        {
            return "AppiumDriverEditPage";
        }

        public string GetDriverWindowName(Agent.eDriverType driverSubType = Agent.eDriverType.NA)
        {
            return "MobileDriverWindow";
        }

        //Mobile Driver Configurations
        [UserConfigured]
        [UserConfiguredDefault(@"http://127.0.0.1:4723/wd/hub")]
        [UserConfiguredDescription("Full Appium server address including port if needed, default address is: 'https://ServerIP:Port/wd/hub'")]
        public String AppiumServer { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Proxy to be used for Appium server connection, need to include the server and port")]
        public String Proxy { get; set; }

        [UserConfigured]
        [UserConfiguredEnumType(typeof(eDevicePlatformType))]
        [UserConfiguredDefault("Android")]
        [UserConfiguredDescription("Device platform type 'Android' or 'iOS'")]
        public eDevicePlatformType DevicePlatformType { get; set; }

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
        [UserConfiguredMultiValues]
        [UserConfiguredDescription("Appium capabilities")]
        public ObservableList<DriverConfigParam> AppiumCapabilities { get; set; }

        bool mIsDeviceConnected = false;
        string mDefaultURL = null;

        public bool IsDeviceConnected
        {
            get => mIsDeviceConnected;
            set => mIsDeviceConnected = value;
        }

        public bool ShowWindow
        {
            get => LoadDeviceWindow;
        }

        private AppiumDriver<AppiumWebElement> Driver;//appium 
        private SeleniumDriver mSeleniumDriver;//selenium 
        public SeleniumDriver AppiumSeleniumDriver
        {
            get
            {
                return mSeleniumDriver;
            }
            set
            {
                mSeleniumDriver = value;
                mSeleniumDriver.StopProcess = StopProcess;
            }
        }

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
                            Driver = new AndroidDriver<AppiumWebElement>(serverUri, driverOptions, TimeSpan.FromSeconds(DriverLoadWaitingTime));
                        }
                        else
                        {
                            Driver = new AndroidDriver<AppiumWebElement>(new HttpCommandExecutor(serverUri, TimeSpan.FromSeconds(DriverLoadWaitingTime)) { Proxy = new WebProxy(this.Proxy) }, driverOptions);
                        }
                        break;
                    case eDevicePlatformType.iOS:
                        if (string.IsNullOrEmpty(Proxy))
                        {
                            Driver = new IOSDriver<AppiumWebElement>(serverUri, driverOptions, TimeSpan.FromSeconds(DriverLoadWaitingTime));
                        }
                        else
                        {
                            Driver = new IOSDriver<AppiumWebElement>(new HttpCommandExecutor(serverUri, TimeSpan.FromSeconds(DriverLoadWaitingTime)) { Proxy = new WebProxy(this.Proxy) }, driverOptions);
                        }
                        break;
                }

                mSeleniumDriver = new SeleniumDriver(Driver); //used for running regular Selenium actions
                mSeleniumDriver.StopProcess = StopProcess;

                if (AppType == eAppType.Web && mDefaultURL != null)
                {
                    try
                    {
                        Driver.Navigate().GoToUrl(mDefaultURL);
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to load default mobile web app URL, please validate the URL is valid", ex);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to start Appium session.{0}Error: '{1}'", System.Environment.NewLine, ex.Message);
                Reporter.ToLog(eLogLevel.ERROR, error, ex);
                ErrorMessageFromDriver = error;

                if (!WorkSpace.Instance.RunningInExecutionMode)
                {
                    Reporter.ToUser(eUserMsgKey.MobileConnectionFailed, ex.Message);
                }
                return false;
            }
        }

        //private static ICommandExecutor CreateRealExecutor(Uri remoteAddress, TimeSpan commandTimeout, IWebProxy proxy)
        //{
        //    var seleniumAssembly = Assembly.Load("WebDriver");
        //    var commandType = seleniumAssembly.GetType("OpenQA.Selenium.Remote.HttpCommandExecutor");
        //    ICommandExecutor commandExecutor = null;

        //    if (null != commandType)
        //    {
        //        commandExecutor =
        //            Activator.CreateInstance(commandType, new object[] { remoteAddress, commandTimeout }) as
        //                ICommandExecutor;              
        //    }

        //    commandExecutor = new HttpCommandExecutor(remoteAddress, commandTimeout);
        //    ((HttpCommandExecutor)commandExecutor).Proxy = proxy;

        //    return commandExecutor;
        //}

        private DriverOptions GetCapabilities()
        {
            //see http://appium.io/slate/en/master/?csharp#appium-server-capabilities for full list of capabilities values
            mDefaultURL = null;
            DriverOptions driverOptions = new AppiumOptions();

            //User customized capabilities
            foreach (DriverConfigParam UserCapability in AppiumCapabilities)
            {
                if (String.IsNullOrWhiteSpace(UserCapability.Parameter))
                {
                    continue;
                }

                if (UserCapability.Parameter.ToLower().Trim() == "defaulturl")
                {
                    mDefaultURL = UserCapability.Value;
                    continue;
                }

                bool boolValue;
                int intValue = 0;
                if (bool.TryParse(UserCapability.Value, out boolValue))
                {
                    driverOptions.AddAdditionalCapability(UserCapability.Parameter, boolValue);
                }
                else if (int.TryParse(UserCapability.Value, out intValue))
                {
                    driverOptions.AddAdditionalCapability(UserCapability.Parameter, intValue);
                }
                else if (UserCapability.Value.Contains("{"))
                {
                    try
                    {
                        JObject json = JObject.Parse(UserCapability.Value);
                        driverOptions.AddAdditionalCapability(UserCapability.Parameter, json);//for Json value to work properly, need to convert it into specific object type like: json.ToObject<selector>());
                    }
                    catch (Exception)
                    {
                        driverOptions.AddAdditionalCapability(UserCapability.Parameter, UserCapability.Value);
                    }
                }
                else
                {
                    driverOptions.AddAdditionalCapability(UserCapability.Parameter, UserCapability.Value);
                }
            }

            return driverOptions;
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

        //public List<IWebElement> LocateElements(eLocateBy LocatorType, string LocValue)
        //{
        //    if (AppType == eAppType.Web)
        //        return mSeleniumDriver.LocateElements(LocatorType, LocValue);

        //        IReadOnlyCollection<IWebElement> elem = null;

        //    switch (LocatorType)
        //    {
        //        //need to override regular selenium driver locator if needed, 
        //        //if not then to run the regular selenium driver locator for it to avoid duplication

        //        default:
        //            elem = mSeleniumDriver.LocateElements(LocatorType, LocValue);
        //            break;
        //    }

        //    return elem.ToList();           
        //}

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
                    elem = Driver.FindElementById(locateValue);
                    break;

                case eLocateBy.ByRelXPath:
                case eLocateBy.ByXPath:
                    elem = Driver.FindElementByXPath(locateValue);
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
                        e.Click();
                        break;

                    case ActUIElement.eElementAction.SetValue:
                    case ActUIElement.eElementAction.SetText:
                        e = LocateElement(act);
                        e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                        break;

                    case ActUIElement.eElementAction.GetText:
                    case ActUIElement.eElementAction.GetFont:
                        e = LocateElement(act);
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("text"));
                        break;

                    case ActUIElement.eElementAction.GetTextLength:
                        e = LocateElement(act);
                        act.AddOrUpdateReturnParamActual("Actual", (e.GetAttribute("text").Length).ToString());
                        break;

                    case ActUIElement.eElementAction.Select:
                    case ActUIElement.eElementAction.SelectByIndex:
                    case ActUIElement.eElementAction.SelectByText:
                        act.Error = "Operation not supported for this mobile OS or application type. Please use 'Click' operation.";
                        break;

                    case ActUIElement.eElementAction.IsValuePopulated:
                        e = LocateElement(act);
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
                        break;

                    case ActUIElement.eElementAction.GetSize:
                        e = LocateElement(act);
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("contentSize").ToString());
                        break;

                    case ActUIElement.eElementAction.ScrollToElement:
                        e = LocateElement(act);
                        int x = e.Location.X;
                        int y = e.Location.Y;
                        TouchAction action = new TouchAction(Driver);
                        action.Press(x, y).MoveTo(x + 1000, y).Release().Perform();
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
                            TapXY(x, y);
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
                            TouchAction t = new TouchAction(Driver);
                            t.Tap(e, 1, 1);
                            Driver.PerformTouchAction(t);
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
                                //TODO: Need to add a flag in the action for this case, as sometimes the value is clear but show text under like 'Searc, or say "OK Google".
                                //Wasting time when not needed
                                string elemntContent = e.Text; //.GetAttribute("name");
                                if (string.IsNullOrEmpty(elemntContent) == false)
                                {
                                    for (int indx = 1; indx <= elemntContent.Length; indx++)
                                    {
                                        //Driver.KeyEvent(22);//"KEYCODE_DPAD_RIGHT"- move marker to right
                                        ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(22);
                                        //Driver.KeyEvent(67);//"KEYCODE_DEL"- delete 1 character
                                        ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(67);
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
                                    //e.Clear();
                                    e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                    break;
                                case eDevicePlatformType.iOS:
                                    //e.Clear();
                                    e.SendKeys(act.GetInputParamCalculatedValue("Value"));
                                    //((IOSElement)e).SetImmediateValue(act.GetInputParamCalculatedValue("Value"));
                                    break;
                            }
                            //if (DriverWindow != null) DriverWindow.ShowActionEfect(true, 100);
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

        private void MobileDeviceActionHandler(ActMobileDevice act)
        {
            ITouchAction tc;
            try
            {
                switch (act.MobileDeviceAction)
                {
                    case ActMobileDevice.eMobileDeviceAction.PressXY:
                        tc = new TouchAction(Driver);
                        tc.Press(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver)).Perform();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                        tc = new TouchAction(Driver);
                        tc.LongPress(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver)).Perform();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.TapXY:
                        TapXY(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver));
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

                    case ActMobileDevice.eMobileDeviceAction.SwipeDown:
                        SwipeScreen(eSwipeSide.Down, 0.25);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeUp:
                        SwipeScreen(eSwipeSide.Up, 0.25);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeLeft:
                        SwipeScreen(eSwipeSide.Left);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeRight:
                        SwipeScreen(eSwipeSide.Right);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.TakeScreenShot:
                        ActScreenShotHandler(act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.DragXYXY:
                        DoDrag(Convert.ToInt32(act.X1.ValueForDriver), Convert.ToInt32(act.Y1.ValueForDriver),
                            Convert.ToInt32(act.X2.ValueForDriver), Convert.ToInt32(act.Y2.ValueForDriver));
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetCurrentApplicationInfo:
                        act.AddOrUpdateReturnParamActual("Current Application Identifiers", GetURL());
                        break;

                    case ActMobileDevice.eMobileDeviceAction.OpenApp:
                        if (AppType == eAppType.NativeHybride)
                        {
                            Driver.LaunchApp();
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.CloseApp:
                        if (AppType == eAppType.NativeHybride)
                        {
                            Driver.CloseApp();
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:
                        ITouchAction swipe;
                        swipe = BuildDragAction(Driver,
                            Convert.ToInt32(act.X1.ValueForDriver),
                            Convert.ToInt32(act.Y1.ValueForDriver),
                            Convert.ToInt32(act.X2.ValueForDriver),
                            Convert.ToInt32(act.Y2.ValueForDriver), 1000);
                        swipe.Perform();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.GetPageSource:
                        act.AddOrUpdateReturnParamActual("Page Source", Driver.PageSource);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LockDevice:
                        if (DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformLockButtonPress(eLockOperation.Lock);
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.UnlockDevice:
                        if (DevicePlatformType == eDevicePlatformType.Android)
                        {
                            PerformLockButtonPress(eLockOperation.UnLock);
                        }
                        else
                        {
                            act.Error = "Operation not supported for this mobile OS or application type.";
                        }
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

        public override void HighlightActElement(Act act)
        {
        }

        private void ActScreenShotHandler(Act act)
        {
            try
            {
                Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
                string filename = Path.GetTempFileName();
                ss.SaveAsFile(filename, ScreenshotImageFormat.Png);
                Bitmap tmp = new System.Drawing.Bitmap(filename);
                act.AddScreenShot(tmp);
                return;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while taking device screen shot", ex);
                act.Error = "Error occured while taking device screen shot, Details: " + ex.Message;
            }
        }

        //private void CreateScreenShot(Act act)
        //{
        //    Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
        //    string filename = Path.GetTempFileName();
        //    ss.SaveAsFile(filename, ScreenshotImageFormat.Png);
        //    Bitmap tmp = new System.Drawing.Bitmap(filename);
        //    act.AddScreenShot(tmp);
        //}

        //public Screenshot GetScreenShot()
        //{
        //    Screenshot ss=null;
        //    try
        //    {
        //        ss = Driver.GetScreenshot ();

        //    }
        //    catch
        //    {
        //        Bitmap bmp = new Bitmap (1024, 768);
        //        var ms = new MemoryStream ();
        //        bmp.Save (ms, System.Drawing.Imaging.ImageFormat.Png);
        //        var byteImage = ms.ToArray ();
        //        ss = new Screenshot (Convert.ToBase64String (byteImage));
        //    }
        //     return ss;
        //}

        public ICollection<IWebElement> GetAllElements()
        {
            return (ICollection<IWebElement>)Driver.FindElementsByXPath(".//*");
        }

        public void TapXY(long x, long y)
        {
            TouchAction t = new TouchAction(Driver);
            t.Tap(x, y);
            Driver.PerformTouchAction(t);
        }

        public void PerformBackButtonPress()
        {
            Driver.Navigate().Back();
        }

        public void PerformHomeButtonPress()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Home);
                    //((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(3);
                    break;
                case eDevicePlatformType.iOS:
                    Dictionary<string, object> commandArgs = new Dictionary<string, object>();
                    commandArgs.Add("name", "home");
                    Driver.ExecuteScript("mobile: pressButton", commandArgs);
                    break;
            }
        }

        public void PerformMenuButtonPress()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Menu);
                    break;
            }
        }

        public void PerformOpenCamera()
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Keycode_CAMERA);
                    break;
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
                            ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Keycode_VOLUME_UP);
                            break;
                        case eVolumeOperation.Down:
                            ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Keycode_VOLUME_DOWN);
                            break;
                    }
                    break;
                case eDevicePlatformType.iOS:
                    Dictionary<string, object> commandArgs = new Dictionary<string, object>();
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
        }

        public void PerformLockButtonPress(eLockOperation LockOperation)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    switch (LockOperation)
                    {
                        case eLockOperation.Lock:
                            ((AndroidDriver<AppiumWebElement>)Driver).Lock();
                            break;
                        case eLockOperation.UnLock:
                            ((AndroidDriver<AppiumWebElement>)Driver).Unlock();
                            break;
                    }
                    break;
                case eDevicePlatformType.iOS:
                    break;
            }
        }

        public void PerformKeyPress(string key)
        {
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(Convert.ToInt32(Enum.Parse(typeof(ActMobileDevice.ePressKey), key)));
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
                    ((AndroidDriver<AppiumWebElement>)Driver).LongPressKeyCode(Convert.ToInt32(Enum.Parse(typeof(ActMobileDevice.ePressKey), key)));
                    break;
                    //case eDevicePlatformType.iOS:
                    //    Dictionary<string, object> commandArgs = new Dictionary<string, object>();
                    //    commandArgs.Add("name", key);
                    //    Driver.ExecuteScript("mobile: pressButton", commandArgs);
                    //    break;
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

        public void SwipeScreen(eSwipeSide side, double impact=1)
        {
            System.Drawing.Size sz = Driver.Manage().Window.Size;
            double startX;
            double startY;
            double endX;
            double endY;
            switch (side)
            {
                case eSwipeSide.Down: // center of footer
                    startX = sz.Width * 0.5;
                    startY = sz.Height * 0.3;
                    endX = sz.Width * 0.5;
                    endY = sz.Height * 0.7 * impact;
                    break;
                case eSwipeSide.Up: // center of header
                    startX = sz.Width * 0.5;
                    startY = sz.Height * 0.7 * impact;
                    endX = sz.Width * 0.5;
                    endY = sz.Height * 0.3;
                    break;
                case eSwipeSide.Right: // center of left side
                    startX = sz.Width * 0.8 * impact;
                    startY = sz.Height * 0.5;
                    endX = sz.Width * 0.1;
                    endY = sz.Height * 0.5;
                    break;
                case eSwipeSide.Left: // center of right side
                    startX = sz.Width * 0.1;
                    startY = sz.Height * 0.5;
                    endX = sz.Width * 0.8 * impact;
                    endY = sz.Height * 0.5;
                    break;
                default:
                    throw new ArgumentException("swipeScreen(): dir: '" + side + "' NOT supported");
            }
            TouchAction drag = new TouchAction(Driver);
            drag.Press(startX, startY).Wait(200).MoveTo(endX, endY).Release().Perform();
        }

        public ITouchAction BuildDragAction(AppiumDriver<AppiumWebElement> driver, int startX, int startY, int endX, int endY, int duration)
        {
            ITouchAction touchAction = new TouchAction(driver)
                .Press(startX, startY)
                .Wait(duration)
                .MoveTo(endX, endY)
                .Release();

            return touchAction;
        }

        public void DoDrag(int startX, int startY, int endX, int endY)
        {
            TouchAction drag = new TouchAction(Driver);
            drag.Press(startX, startY).MoveTo(endX, endY).Release();
            drag.Perform();
        }

        public string GetCurrentActivityDetails()
        {
            try
            {
                if (DevicePlatformType == eDevicePlatformType.Android)
                {
                    return string.Format("{0} | {1}", ((AndroidDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).CurrentPackage.Split('.').Last(),
                        ((AndroidDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).CurrentActivity.Split('.').Last());
                }
                else if (DevicePlatformType == eDevicePlatformType.iOS)
                {
                    return string.Format("{0}", ((IOSDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).GetSessionDetail("CFBundleIdentifier").ToString());
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
                    return string.Format("{0} | {1}", ((AndroidDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).CurrentPackage,
                        ((AndroidDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).CurrentActivity);
                }
                else if (DevicePlatformType == eDevicePlatformType.iOS)
                {
                    return string.Format("{0}", ((IOSDriver<OpenQA.Selenium.Appium.AppiumWebElement>)Driver).GetSessionDetail("CFBundleIdentifier").ToString());
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
            List<AppWindow> list = new List<AppWindow>();

            AppWindow AW = new AppWindow();
            AW.WindowType = AppWindow.eWindowType.Appium;
            AW.Title = (AppType == eAppType.Web) ? GetURL() : GetCurrentActivityDetails();   // TODO: add device name and info

            list.Add(AW);
            return list;
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            //NA
        }

        async void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            if (AppType == eAppType.Web)
            {
                ((IWindowExplorer)mSeleniumDriver).HighLightElement(ElementInfo, locateElementByItLocators);
                return;
            }

            if (ElementInfo.ElementObject == null)
            {
                ElementInfo.ElementObject = await FindElementXmlNodeByXY(ElementInfo.X, ElementInfo.Y);
            }

            OnDriverMessage(eDriverMessageType.HighlightElement, ElementInfo);
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
            Point mousePosCurrent = new Point(-1, -1);
            XmlNode foundNode = null;
            ElementInfo foundElement = null;
            var mousePos = OnSpyingElementEvent();
            if (mousePos != null && mousePos is Point)
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
                AppWindow aw = new AppWindow();
                aw.Title = GetCurrentActivityDetails();
                return aw;
            }
            else
                return null;
        }

        async Task<List<ElementInfo>> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false, string specificFramePath = null, List<string> relativeXpathTemplateList = null)
        {
            if (AppType == eAppType.Web)
            {
                return await Task.Run(() => ((IWindowExplorer)mSeleniumDriver).GetVisibleControls(filteredElementType, foundElementsList, isPOMLearn, specificFramePath));
            }

            if (foundElementsList == null)
                foundElementsList = new ObservableList<ElementInfo>();

            await GetPageSourceDocument(true);

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
                ElementInfo EI = await GetElementInfoforXmlNode(nodes[i]);
                EI.IsAutoLearned = true;

                if (relativeXpathTemplateList != null && relativeXpathTemplateList.Count > 0)
                {
                    foreach (var template in relativeXpathTemplateList)
                    {
                        eLocateBy CustomLocLocateBy = eLocateBy.ByRelXPath;

                        if (template.Contains('{'))
                            CustomLocLocateBy = eLocateBy.iOSPredicateString;

                        var customLocator = GetUserDefinedCustomLocatorFromTemplates(template, CustomLocLocateBy, EI.Properties.ToList());

                        if(customLocator != null)
                            EI.Locators.Add(customLocator);
                        //CreateXpathFromUserTemplate(template, foundElemntInfo);
                    }
                }

                if (filteredElementType == null ||
                    (filteredElementType != null && filteredElementType.Contains(EI.ElementTypeEnum)))
                    foundElementsList.Add(EI);
            }

            return foundElementsList.ToList();
        }

        private async Task<ElementInfo> GetElementInfoforXmlNode(XmlNode xmlNode)
        {
            ElementInfo EI = new ElementInfo();
            Tuple<string, eElementType> Elementype = GetElementTypeEnum(xmlNode);
            EI.ElementType = Elementype.Item1;           //GetAttrValue(xmlNode, "class");
            EI.ElementTypeEnum = Elementype.Item2;
            EI.ElementTitle = string.Format("{0}-{1}", Elementype.Item2, GetNameFor(xmlNode));
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
                if (boundsXY.Count() == 4)
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
            switch (ElementTag.ToLower())
            {
                case "android.widget.edittext":
                case "xcuielementtypetextfield":
                    return eElementType.TextBox;

                case "android.widget.button":
                case "android.widget.ratingbar":
                case "android.widget.framelayout":
                case "android.widget.imageview":
                case "android.widget.imagebutton":
                case "android.widget.switch":
                case "xcuielementtypebutton":
                case "xcuielementtypeswitch":
                    return eElementType.Button;

                case "android.widget.spinner":
                case "xcuielementtypecombobox":
                    return eElementType.ComboBox;

                case "android.widget.checkbox":
                case "xcuielementtypecheckbox":
                    return eElementType.CheckBox;

                case "android.widget.view":
                case "android.widget.textview":
                case "xcuielementtypestatictext":
                    return eElementType.Label;

                case "android.widget.image":
                case "xcuielementtypeimage":
                    return eElementType.Image;

                case "android.widget.radiobutton":
                case "xcuielementtyperadiobutton":
                    return eElementType.RadioButton;

                case "android.widget.canvas":
                case "android.widget.linearlayout":
                case "android.widget.relativelayout":
                case "xcuielementtypeother":
                    return eElementType.Canvas;

                case "xcuielementtypetab":
                    return eElementType.Tab;

                case "xcuielementtypebrowser":
                    return eElementType.Browser;

                case "xcuielementtypetable":
                    return eElementType.Table;

                case "xcuielementtypetablerow":
                case "xcuielementtypetablecolumn":
                    return eElementType.TableItem;

                case "xcuielementtypelink":
                    return eElementType.HyperLink;

                case "android.widget.form":
                    return eElementType.Form;

                case "android.view.view":
                    return eElementType.Div;

                case "android.widget.checkedtextview":
                    return eElementType.ListItem;

                default:
                    return eElementType.Unknown;
            }
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
                return string.Format("//");
            else
                return String.Format("{0}/{1}[{2}]", await GetXPathToNode(node.ParentNode), node.Name, indexInParent);          //Testing Async
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {
            List<ElementInfo> list = new List<ElementInfo>();

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
                        Name = a[a.Length - 1];
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
                return Name;

            return xmlNode.Name;
        }

        static string GetAttrValue(XmlNode xmlNode, string attr)
        {
            if (xmlNode.Attributes == null) return null;
            if (xmlNode.Attributes[attr] == null) return null;
            if (string.IsNullOrEmpty(xmlNode.Attributes[attr].Value)) return null;
            return xmlNode.Attributes[attr].Value;
        }


        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            ObservableList<ElementLocator> list = new ObservableList<ElementLocator>();

            if (DevicePlatformType == eDevicePlatformType.iOS)
            {
                string selector = string.Format("type == '{0}' AND value BEGINSWITH[c] '{1}' AND visible == {2}",
                    ElementInfo.ElementType, GetAttrValue(ElementInfo.ElementObject as XmlNode, "value"), GetAttrValue(ElementInfo.ElementObject as XmlNode, "visible") == "true" ? 1 : 0);

                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.iOSPredicateString,
                    LocateValue = selector,
                    IsAutoLearned = true,
                    Help = "Highly Recommended as Predicate Matching is built into XCUITest, it has the potential to be much faster than Appium's XPath strategy"
                });

                list.Add(new ElementLocator()
                {
                    Active = true,
                    LocateBy = eLocateBy.iOSClassChain,
                    LocateValue = string.Format("**/{0}/{1}", ElementInfo.XPath.Split('/')[ElementInfo.XPath.Split('/').Length - 2], ElementInfo.XPath.Split('/').Last()),
                    IsAutoLearned = true,
                    Help = "Highly Recommended as Class Chain strategy is built into XCUITest, it has the potential to be much faster than Appium's XPath strategy"
                });
            }

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

            list.Add(new ElementLocator()
            {
                Active = true,
                LocateBy = eLocateBy.ByXPath,
                LocateValue = string.Format("//{0}/{1}", ElementInfo.XPath.Split('/')[ElementInfo.XPath.Split('/').Length - 2], ElementInfo.XPath.Split('/').Last()),
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
            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

            XmlNode node = ElementInfo.ElementObject as XmlNode;

            if (node == null) return list;

            XmlAttributeCollection attrs = node.Attributes;

            if (attrs == null) return list;

            for (int i = 0; i < attrs.Count; i++)
            {
                ControlProperty CP = new ControlProperty();
                CP.Name = attrs[i].Name;
                CP.Value = attrs[i].Value;
                list.Add(CP);
            }

            return list;
        }

        public event RecordingEventHandler RecordingEvent;

        void IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        void IRecord.StartRecording(bool learnAdditionalChanges)
        {
            //Dispatcher.Invoke(() =>
            //{
            //    if (DriverWindow != null) DriverWindow.StartRecording();
            //});
        }

        void IRecord.StopRecording()
        {
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

        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
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

                foreach (ElementInfo EI in originalList)
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Pending;
                }


                foreach (ElementInfo EI in originalList)
                {
                    try
                    {
                        if (LocateElementByLocators(EI.Locators) != null)
                        //if (e != null)
                        {
                            //EI.ElementObject = e;
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
                switch (EL.LocateBy)
                {
                    case eLocateBy.ByResourceID:
                        elem = Driver.FindElementById(EL.LocateValue);
                        break;

                    case eLocateBy.iOSPredicateString:
                        elem = Driver.FindElement(MobileBy.IosNSPredicate(EL.LocateValue));
                        break;

                    case eLocateBy.iOSClassChain:
                        elem = Driver.FindElement(MobileBy.IosClassChain(EL.LocateValue));
                        break;

                    case eLocateBy.ByRelXPath:
                    case eLocateBy.ByXPath:
                        elem = Driver.FindElementByXPath(EL.LocateValue);
                        break;

                    default:
                        elem = mSeleniumDriver.LocateElementByLocator(EL);
                        break;
                }
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

            ElementInfo OriginalElementInfo = originalElements.Where(x => (x.ElementTypeEnum == latestElement.ElementTypeEnum)
                                                                    && (x.XPath == latestElement.XPath)
                                                                    && (x.Path == latestElement.Path || (string.IsNullOrEmpty(x.Path) && string.IsNullOrEmpty(latestElement.Path)))
                                                                    && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) == null
                                                                        || (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null && latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath) != null
                                                                            && (x.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue == latestElement.Locators.FirstOrDefault(l => l.LocateBy == eLocateBy.ByRelXPath).LocateValue)
                                                                            )
                                                                        )
                                                                  ).FirstOrDefault();

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

        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            if (AppType == eAppType.Web)
            {
                return ((IWindowExplorer)mSeleniumDriver).LearnElementInfoDetails(EI);
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

        public Byte[] GetScreenshotImage()
        {
            return Driver.GetScreenshot().AsByteArray;
        }

        public void PerformTap(long x, long y)
        {
            TapXY(x, y);
        }

        public void PerformLongPress(long x, long y)
        {
            TouchAction tc = new TouchAction(Driver);
            tc.LongPress(x, y).Perform();
        }

        public void PerformDrag(Point start, Point end)
        {
            DoDrag(start.X, start.Y, end.X, end.Y);
        }

        public void SwitchToLandscape()
        {
            Driver.Orientation = ScreenOrientation.Landscape;
        }

        public void SwitchToPortrait()
        {
            Driver.Orientation = ScreenOrientation.Portrait;
        }

        public eDeviceOrientation GetOrientation()
        {
            return (eDeviceOrientation)Enum.Parse(typeof(eDeviceOrientation), Driver.Orientation.ToString());
        }

        public Bitmap GetScreenShot(Tuple<int, int> setScreenSize = null)
        {
            Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
            string filename = Path.GetTempFileName();
            ss.SaveAsFile(filename, ScreenshotImageFormat.Png);
            return new System.Drawing.Bitmap(filename);
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
                    pageSourceString = await GetPageSource();
                else
                    pageSourceString = Driver.PageSource;

                pageSourceXml = new XmlDocument();
                pageSourceXml.LoadXml(pageSourceString);

                ElmsNodes = pageSourceXml.SelectNodes("//*");

                ///get the selected element from screen
                if (ElmsNodes != null && ElmsNodes.Count > 0)
                {
                    //move to collection for getting last node which fits to bounds
                    ObservableList<XmlNode> ElmsNodesColc = new ObservableList<XmlNode>();
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
                            ElmsNodesColc.Add(elemNode);
                    }

                    Dictionary<XmlNode, long> foundElements = new Dictionary<XmlNode, long>();
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
                                        if (boundsXY.Count() == 4)
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
                        return foundNode;
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

        public override Point GetPointOnAppWindow(Point clickedPoint, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight)
        {
            Point pointOnAppScreen = new Point();
            double ratio_X = 1, ratio_Y = 1;

            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:

                    if (AppType == eAppType.Web)
                    {
                        ratio_X = (SrcWidth / 3) / ActWidth;
                        ratio_Y = (SrcHeight / 3) / ActHeight;
                    }
                    else
                    {
                        ratio_X = SrcWidth / ActWidth;
                        ratio_Y = SrcHeight / ActHeight;
                    }

                    break;
                case eDevicePlatformType.iOS:

                    if (AppType == eAppType.Web)
                    {
                        ratio_X = (SrcWidth / 3) / ActWidth;
                        ratio_Y = (SrcHeight / 3) / ActHeight;
                    }
                    else
                    {
                        ratio_X = (SrcWidth / 2) / ActWidth;
                        ratio_Y = (SrcHeight / 2) / ActHeight;
                    }

                    break;
            }

            pointOnAppScreen.X = (int)(clickedPoint.X * ratio_X);
            pointOnAppScreen.Y = (int)(clickedPoint.Y * ratio_Y);

            return pointOnAppScreen;
        }

        public override bool SetRectangleProperties(ref Point ElementStartPoints, ref Point ElementMaxPoints, double SrcWidth, double SrcHeight, double ActWidth, double ActHeight, ElementInfo clickedElementInfo, bool AutoCorrectRectPropRequired)
        {
            double ratio_X, ratio_Y;
            int AutoCorrectRectProp = 1;

            XmlNode rectangleXmlNode = clickedElementInfo.ElementObject as XmlNode;
            switch (DevicePlatformType)
            {
                case eDevicePlatformType.Android:

                    if (AppType == eAppType.Web)
                    {
                        ratio_X = (SrcWidth * 3) / ActWidth;
                        ratio_Y = (SrcHeight * 3) / ActHeight;

                        ElementStartPoints.X = (int)(ElementStartPoints.X * ratio_X);
                        ElementStartPoints.Y = (int)(ElementStartPoints.Y * ratio_Y);

                        ElementMaxPoints.X = (int)(ElementMaxPoints.X * ratio_X);
                        ElementMaxPoints.Y = (int)(ElementMaxPoints.Y * ratio_Y);
                    }
                    else
                    {
                        ratio_X = SrcWidth / ActWidth;
                        ratio_Y = SrcHeight / ActHeight;

                        string bounds = rectangleXmlNode != null ? rectangleXmlNode.Attributes["bounds"].Value : "";
                        bounds = bounds.Replace("[", ",");
                        bounds = bounds.Replace("]", ",");
                        string[] boundsXY = bounds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (boundsXY.Count() == 4)
                        {
                            ElementStartPoints.X = (int)(Convert.ToInt64(boundsXY[0]) / ratio_X);
                            ElementStartPoints.Y = (int)(Convert.ToInt64(boundsXY[1]) / ratio_Y);

                            ElementMaxPoints.X = (int)(Convert.ToInt64(boundsXY[2]) / ratio_X);
                            ElementMaxPoints.Y = (int)(Convert.ToInt64(boundsXY[3]) / ratio_Y);
                        }
                    }

                    break;

                case eDevicePlatformType.iOS:
                    if (AppType == eAppType.Web)
                    {
                        //ratio_X = SrcWidth / ActWidth;
                        //ratio_Y = SrcHeight / ActHeight;
                        ratio_X = (SrcWidth / 2) / ActWidth;
                        ratio_Y = (SrcHeight / 3) / ActHeight;

                        ElementStartPoints.X = (int)(ElementStartPoints.X / ratio_X);
                        ElementStartPoints.Y = (int)(ElementStartPoints.Y / ratio_Y);
                        ElementMaxPoints.X = (int)(ElementMaxPoints.X / ratio_X);
                        ElementMaxPoints.Y = (int)(ElementMaxPoints.Y / ratio_Y);
                    }
                    else
                    {
                        ratio_X = (SrcWidth / 2) / ActWidth;
                        ratio_Y = (SrcHeight / 2) / ActHeight;

                        string x = rectangleXmlNode.Attributes["x"].Value;
                        string y = rectangleXmlNode.Attributes["y"].Value;
                        string hgt = rectangleXmlNode.Attributes["height"].Value;
                        string wdth = rectangleXmlNode.Attributes["width"].Value;

                        ElementStartPoints.X = (int)(Convert.ToInt32(x) / ratio_X);
                        ElementStartPoints.Y = (int)(Convert.ToInt32(y) / ratio_Y);

                        if(AutoCorrectRectPropRequired)
                            AutoCorrectRectProp = 2;

                        ElementMaxPoints.X = ElementStartPoints.X + (Convert.ToInt32(wdth) * AutoCorrectRectProp);
                        ElementMaxPoints.Y = ElementStartPoints.Y + (Convert.ToInt32(hgt) * AutoCorrectRectProp);
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
            return false;
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
            return new List<eTabView>() { eTabView.Screenshot, eTabView.GridView, eTabView.PageSource };
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

        public override bool SerializationError(Agent agent, SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.SetValueException)
            {
                if (value == "MobileAppiumAndroid" || value == "PerfectoMobileAndroid" || value == "MobileAppiumIOS" || value == "PerfectoMobileIOS"
                       || value == "MobileAppiumAndroidBrowser" || value == "PerfectoMobileAndroidWeb" || value == "MobileAppiumIOSBrowser" || value == "PerfectoMobileIOSWeb")
                {
                    agent.DriverType = Agent.eDriverType.Appium;
                    agent.DriverConfiguration = new ObservableList<DriverConfigParam>();
                    //agent.GetOrCreateParam(nameof(AppiumServer), @"http://127.0.0.1:4723/wd/hub");
                    agent.GetOrCreateParam(nameof(LoadDeviceWindow), "true");
                    agent.GetOrCreateParam(nameof(DeviceAutoScreenshotRefreshMode), eAutoScreenshotRefreshMode.Live.ToString());
                    agent.DirtyStatus = Common.Enums.eDirtyStatus.Modified;

                    if (value == "MobileAppiumAndroid" || value == "PerfectoMobileAndroid")
                    {
                        agent.GetOrCreateParam(nameof(DevicePlatformType), eDevicePlatformType.Android.ToString());
                        agent.GetOrCreateParam(nameof(AppType), eAppType.NativeHybride.ToString());
                    }
                    else if (value == "MobileAppiumIOS" || value == "PerfectoMobileIOS")
                    {
                        agent.GetOrCreateParam(nameof(DevicePlatformType), eDevicePlatformType.iOS.ToString());
                        agent.GetOrCreateParam(nameof(AppType), eAppType.NativeHybride.ToString());
                    }
                    else if (value == "MobileAppiumAndroidBrowser" || value == "PerfectoMobileAndroidWeb")
                    {
                        agent.GetOrCreateParam(nameof(DevicePlatformType), eDevicePlatformType.Android.ToString());
                        agent.GetOrCreateParam(nameof(AppType), eAppType.Web.ToString());
                    }
                    else if (value == "MobileAppiumIOSBrowser" || value == "PerfectoMobileIOSWeb")
                    {
                        agent.GetOrCreateParam(nameof(DevicePlatformType), eDevicePlatformType.iOS.ToString());
                        agent.GetOrCreateParam(nameof(AppType), eAppType.Web.ToString());
                    }
                    return true;
                }
            }
            return false;
        }

        public void PerformScreenSwipe(eSwipeSide swipeSide, double impact = 1)
        {
            SwipeScreen(swipeSide, impact);
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
                pageSourceXml = null;

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
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(Convert.ToInt32(ActMobileDevice.ePressKey.Keycode_SETTINGS));
                    break;
                case eDevicePlatformType.iOS:
                    Driver.ActivateApp("com.apple.Preferences");
                    break;
            }
        }
    }
}
