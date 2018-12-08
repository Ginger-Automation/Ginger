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

using GingerCore.Actions;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Reflection;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Xml;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using static GingerCore.Actions.ActMobileDevice;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Safari;

namespace GingerCore.Drivers.Mobile.Perfecto
{

    public class PerfectoDriver : DriverBase
    {

        [UserConfigured]
        [UserConfiguredDefault("partners.perfectomobile.com")]
        [UserConfiguredDescription("Perfecto Cloud URL")]
        public String Perfecto_Host_URL { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto Cloud Username")]
        public String Perfecto_User_Name { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto Cloud Password")]
        public String Perfecto_Password { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto Cloud device ID")]
        public String Perfecto_Device_ID { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Driver Implicit Wait")]
        public int ImplicitWait { get; set; }

        public bool ConnectedToDevice = false;
        private RemoteWebDriver mDriver;
        private SeleniumDriver mSeleniumDriver;//selenium base


        public enum eContextType
        {
            NativeAndroid,
            NativeIOS,
            WebAndroid,
            WebIOS
        }



        private eContextType mContextType { get; set; }


        public enum eSwipeSide
        {
            Up,
            Down,
            Left,
            Right
        }

        public PerfectoDriver(eContextType ContextType, BusinessFlow BF)
        {
            mContextType = ContextType;
            BusinessFlow = BF;
        }

        public override void StartDriver()
        {
            ConnectedToDevice = ConnectToPerfecto();
        }

        //Connect to Perfecto mobile driver
        public Boolean ConnectToPerfecto()
        {

            //Show Perfecto device dashboard in order to view running test flow
            String devicesDashboard = "https://" + Perfecto_Host_URL + "/nexperience/dashboard.jsp";
            System.Diagnostics.Process.Start(devicesDashboard);

            var url = new Uri(string.Format("https://{0}/nexperience/perfectomobile/wd/hub", Perfecto_Host_URL));

            try
            {
                if (mContextType == eContextType.NativeAndroid)
                {
                    DriverOptions DO = GetNativeCapabilities();
                    mDriver = new AndroidDriver<IWebElement>(url, DO);
                }
                else if (mContextType == eContextType.NativeIOS)
                {
                    DriverOptions DO = GetNativeCapabilities();
                    mDriver = new IOSDriver<IWebElement>(url, DO);
                }
                else if (mContextType == eContextType.WebAndroid || mContextType == eContextType.WebIOS)
                {
                    DesiredCapabilities capabilities = GetWebCapabilities();
                    mDriver = new RemoteWebDriver(url, capabilities);
                }

                mSeleniumDriver = new SeleniumDriver(mDriver);
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error, Could not create Perfecto Mobile Automation Driver, " + e.Message);
            }

            if (mContextType != eContextType.WebAndroid && mContextType != eContextType.WebIOS)
            {
                //Context to be switch back to "WEBVIEW" if web element should be searched
                ((AppiumDriver<IWebElement>)mDriver).Context = "NATIVE_APP";
            }


            mDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(ImplicitWait);


            return true;
        }

        private DesiredCapabilities GetWebCapabilities()
        {
            DesiredCapabilities capabilities = new DesiredCapabilities("mobileOS", string.Empty, new Platform(PlatformType.Any));

            capabilities.SetCapability("user", Perfecto_User_Name);
            capabilities.SetCapability("password", Perfecto_Password);
            //capabilities.SetCapability("securityToken", Perfecto_Password);
            capabilities.SetCapability("deviceName", Perfecto_Device_ID);
            capabilities.SetPerfectoLabExecutionId(Perfecto_Host_URL);
            return capabilities;
        }

        private DriverOptions GetNativeCapabilities()
        {
            DriverOptions driverOptions = null;
            switch (mContextType)
            {
                case eContextType.NativeIOS:
                case eContextType.NativeAndroid:
                    driverOptions = new AppiumOptions();
                    break;
                case eContextType.WebAndroid:
                    driverOptions = new ChromeOptions();
                    break;
                case eContextType.WebIOS:
                    driverOptions = new SafariOptions();
                    break;
                default:
                    break;
            }

            driverOptions.AddAdditionalCapability("user", Perfecto_User_Name);
            driverOptions.AddAdditionalCapability("password", Perfecto_Password);
            driverOptions.AddAdditionalCapability("deviceName", Perfecto_Device_ID);

            return driverOptions;
        }


        public override void CloseDriver()
        {
            mDriver.Quit();
            ConnectedToDevice = false;
        }

        public override void RunAction(Act act)
        {
            //Handle all actions types
            Type ActType = act.GetType();

            if (ActType == typeof(ActMobileDevice))
            {
                MobileDeviceActionHandler((ActMobileDevice)act);
                return;
            }
            if (ActType == typeof(ActUIElement))
            {
                mSeleniumDriver.HandleActUIElement((ActUIElement)act);
                return;
            }
            if (ActType == typeof(ActBrowserElement))
            {
                mSeleniumDriver.ActBrowserElementHandler((ActBrowserElement)act);
                return;
            }

        }


      

        private void MobileDeviceActionHandler(ActMobileDevice act)
        {
            try
            {
                switch (act.MobileDeviceAction)
                {
                    case ActMobileDevice.eMobileDeviceAction.PressKey:
                        PressBtn(act.MobilePressKey);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressBackButton:
                        PressBackBtn();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressHomeButton:
                        PressHomeBtn();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressCamera:
                       PressCamera();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressVolumeUp:
                       PressVolumeUp();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressVolumeDown:
                        PressVolumeDown();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressSwitchApp:
                        PressSwitchApp();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressLongHome:
                        PressLongHome();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SwipeDown:
                        SwipeScreen(eSwipeSide.Down);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SwipeUp:
                        SwipeScreen(eSwipeSide.Up);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SwipeLeft:
                        SwipeScreen(eSwipeSide.Left);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.SwipeRight:
                        SwipeScreen(eSwipeSide.Right);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.Wait:
                        Thread.Sleep(Convert.ToInt32(act.ValueForDriver) * 1000);
                        break;
                    case ActMobileDevice.eMobileDeviceAction.OpenAppByName:
                        OpenAppByName(act.ValueForDriver);
                        break;
                    default:
                        act.Error = "Error: This operation is missing implementation";
                        break;
                }
            }
            catch (Exception ex)
            {
                act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
            }
        }

        private void PressLongHome()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "LONGHOME");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void PressSwitchApp()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "APP_SWITCH");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void PressVolumeDown()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "VOL_DOWN");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void PressVolumeUp()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "VOL_UP");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void PressCamera()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "CAMERA");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void OpenAppByName(string valueForDriver)
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("name", valueForDriver);
            mDriver.ExecuteScript("mobile:application:open", params1);
        }

        private void PressBtn(ePressKey PressKey)
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", PressKey.ToString());
            mDriver.ExecuteScript("mobile:presskey", params1);
        }


        //Back Click Action
        private void PressBackBtn()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "BACK");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        private void PressMenuBtn()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "Menu");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        //Home Click Action
        private void PressHomeBtn()
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "HOME");
            mDriver.ExecuteScript("mobile:presskey", params1);
        }

        //Swipe Action
        private void SwipeScreen(eSwipeSide side)
        {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            switch (side)
            {
                case eSwipeSide.Down:
                    params1.Add("start", "50%,80%");
                    params1.Add("end", "50%,20%");
                    mDriver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Up:
                    params1.Add("start", "50%,20%");
                    params1.Add("end", "50%,80%");
                    mDriver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Left:
                    params1.Add("start", "20%,50%");
                    params1.Add("end", "80%,50%");
                    mDriver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Right:
                    params1.Add("start", "80%,50%");
                    params1.Add("end", "20%,50%");
                    mDriver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                default:
                    break;
            }
        }



        public override ePlatformType Platform { get { return ePlatformType.Mobile; } }

        public override bool IsRunning()
        {
            return ConnectedToDevice;
        }


        public override Act GetCurrentElement()
        {
            try
            {
                Act act = null;
                IWebElement currentElement = mDriver.SwitchTo().ActiveElement();

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
                        default:
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);

                return null;
            }
        }

        //----------------------------getAct... methods---------------------------------------------
        //TODO:change to use ActUIElement
        private Act getActTextBox(IWebElement currentElement)
        {
            ActTextBox a = new ActTextBox();
            setActLocator(currentElement, a);
            a.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
            a.Value = currentElement.GetAttribute("value");
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
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
            a.Value = currentElement.GetAttribute("value");
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
        }

        private Act getActCheckbox(IWebElement currentElement)
        {
            ActCheckbox act = new ActCheckbox();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActRadioButton(IWebElement currentElement)
        {
            ActRadioButton act = new ActRadioButton();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActLink(IWebElement currentElement)
        {
            ActLink al = new ActLink();
            setActLocator(currentElement, al);
            al.Value = currentElement.Text;
            return al;
        }

        //---------------------End of getAct... methods-----------------------------

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

        public override string GetURL()
        {
            return "TBD";
        }

        public override List<ActButton> GetAllButtons()
        {
            return null;
        }

        public override List<ActWindow> GetAllWindows()
        {
            return null;
        }

        public override List<ActLink> GetAllLinks()
        {
            return null;
        }

        public override void HighlightActElement(Act act)
        {
            act.Error = "Method iot implemented";
        }
    }
}