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

namespace GingerCore.Drivers.Mobile.Perfecto {

    public class PerfectoDriver : DriverBase {

        [UserConfigured]
        [UserConfiguredDefault("partners.perfectomobile.com")]
        [UserConfiguredDescription("The URL of Perfecto cloud")]
        public String Perfecto_Host_URL { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto cloud Username")]
        public String Perfecto_User_Name { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto Password")]
        public String Perfecto_Password { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]
        [UserConfiguredDescription("Perfecto device ID")]
        public String Perfecto_Device_ID { get; set; }

        public bool ConnectedToDevice = false;
        private RemoteWebDriverExtended Driver;
        
        public enum eSwipeSide {
            Up, Down, Left, Right
        }

        public PerfectoDriver(BusinessFlow BF) {
            BusinessFlow = BF;
        }

        public override void StartDriver() {
            ConnectedToDevice = ConnectToPerfecto();
        }

        //Connect to Perfecto mobile driver
        public Boolean ConnectToPerfecto() {

            var browserName = "mobileOS";

            //Show Perfecto device dashboard in order to view running test flow
            String devicesDashboard = "https://" + Perfecto_Host_URL + "/nexperience/dashboard.jsp";

            System.Diagnostics.Process.Start(devicesDashboard);

            //Get List of devices available in Perfecto
            //XmlDocument XmlListOfDevices = new XmlDocument();
            //String urlToGetDevicesList = "https://" + Perfecto_Host_URL + "/services/handsets?operation=list&user=" + Perfecto_User_Name + "&password=" + Perfecto_Password + "&status=connected";
            //XmlListOfDevices.Load(urlToGetDevicesList);
            //ShowListOfDevices(XmlListOfDevices);

            DesiredCapabilities capabilities = new DesiredCapabilities(browserName, string.Empty, new Platform(PlatformType.Any));

            capabilities.SetCapability("user", Perfecto_User_Name);
            capabilities.SetCapability("password", Perfecto_Password);
            //capabilities.SetCapability("securityToken", Perfecto_Password);
            capabilities.SetCapability("deviceName", Perfecto_Device_ID);
            capabilities.SetPerfectoLabExecutionId(Perfecto_Host_URL);
          
            var url = new Uri(string.Format("https://{0}/nexperience/perfectomobile/wd/hub", Perfecto_Host_URL));
            try
            {
                Driver = new RemoteWebDriverExtended(url, capabilities);
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error, Could not create Perfecto Mobile Automation Driver, " + e.Message);
            }
            Driver.Manage().Timeouts().ImplicitWait=TimeSpan.FromSeconds(15);

            Dictionary<String, Object> pars = new Dictionary<String, Object>();
            pars.Add("sources", "Device");
            pars.Add("interval", 15);
            Driver.ExecuteScript("mobile:monitor:start", pars);

            return true;
        }

        private void ShowListOfDevices(XmlDocument XmlListOfDevices) {
        }

        public override void CloseDriver() {
            Driver.Quit();
            ConnectedToDevice = false;
        }

        public override void RunAction(Act act) {
            //Handle all actions types
            Type ActType = act.GetType();

            if (ActType == typeof(ActGotoURL)) {
                GotoURL((ActGotoURL)act);
                return;
            }
            if (ActType == typeof(ActMobileDevice)) {
                MobileDeviceActionHandler((ActMobileDevice)act);
                return;
            }
            if (ActType == typeof(ActGenElement)) {
                GenElementHandler((ActGenElement)act);
                return;
            }
            if (ActType == typeof(ActScreenShot)) {
                TakeScreenShot(act);
                return;
            }
        }

        private void GotoURL(ActGotoURL act) {
            string sURL = act.ValueForDriver.ToLower();
            if (sURL.StartsWith("www")) {
                sURL = "http://" + act.ValueForDriver;
            }
            else {
                sURL = act.ValueForDriver;
            }

            Driver.Navigate().GoToUrl(sURL);
        }

        //Handle all Generic Actions
        public void GenElementHandler(ActGenElement act) {
            IWebElement e;
            switch (act.GenElementAction) {
                //Click and ClickAt
                case ActGenElement.eGenElementAction.ClickAt:
                case ActGenElement.eGenElementAction.Click:
                    e = LocateElement(act);
                    if (e == null) {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    else {
                        e.Click();
                    }
                    break;

                //Set Value
                case ActGenElement.eGenElementAction.SetValue:
                    e = LocateElement(act);
                    if (e != null) {
                        if (e.TagName == "select") {
                            SelectElement combobox = new SelectElement(e);
                            string val = act.ValueForDriver;
                            combobox.SelectByText(val);
                            act.ExInfo += "Selected Value - " + val;
                            return;
                        }
                        if (e.TagName == "input" && e.GetAttribute("type") == "checkbox") {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('checked',arguments[1])", e, act.ValueForDriver);
                            return;
                        }

                        //Special case for FF 
                        if (Driver.GetType() == typeof(FirefoxDriver) && e.TagName == "input" && e.GetAttribute("type") == "text") {
                            e.Clear();
                            e.SendKeys(GetKeyName(act.ValueForDriver));
                        }
                        else
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].setAttribute('value',arguments[1])", e, act.ValueForDriver);
                    }
                    else {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                //KeyType - similar to Keyboard Input
                case ActGenElement.eGenElementAction.KeyType:
                    e = LocateElement(act);
                    if (e != null) {
                        e.Clear();
                        e.SendKeys(GetKeyName(act.ValueForDriver));
                    }
                    else {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                //Keyboard Input - similar to KeyType
                case ActGenElement.eGenElementAction.KeyboardInput:
                    e = LocateElement(act);

                    if (e != null) {
                        e.SendKeys(GetKeyName(act.ValueForDriver));
                    }
                    else {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValueCalculated;
                        return;
                    }
                    break;

                //Check this
                case ActGenElement.eGenElementAction.GetValue:
                    e = LocateElement(act);
                    if (e != null) {
                        act.AddOrUpdateReturnParamActual("Actual", e.GetAttribute("text"));
                        if (act.GetReturnParam("Actual") == null)
                            act.AddOrUpdateReturnParamActual("Actual", e.Text);
                    }
                    else {
                        act.Error = "Error: Element not found - " + act.LocateBy + " " + act.LocateValue;
                        return;
                    }
                    break;

                //GoToURL - implemented also as GoToURL action and not in GenElementHandler action
                case ActGenElement.eGenElementAction.GotoURL:
                    string sURL = act.ValueForDriver.ToLower();
                    if (sURL.StartsWith("www")) {
                        sURL = "http://" + act.ValueForDriver;
                    }
                    else {
                        sURL = act.ValueForDriver;
                    }

                    Driver.Navigate().GoToUrl(sURL);
                    break;

                //Wait Action
                case ActGenElement.eGenElementAction.Wait:
                    WaitAction(act);
                    break;

                //Delete all Cookies
                case ActGenElement.eGenElementAction.DeleteAllCookies:  //TODO: FIXME: This action should not be part of GenElement
                    Driver.Manage().Cookies.DeleteAllCookies();
                    break;

                //Back Click
                case ActGenElement.eGenElementAction.Back: //TODO: FIXME: This action should not be part of GenElement
                    PressBackBtn();
                    break;

                case ActGenElement.eGenElementAction.CloseBrowser:
                    Dictionary<String, Object> pars = new Dictionary<String, Object>();
                    Driver.ExecuteScript("mobile:monitor:stop", pars);
                    break;
            }
        }

        private void MobileDeviceActionHandler(ActMobileDevice act) {
            try {
                switch (act.MobileDeviceAction) {
                    case ActMobileDevice.eMobileDeviceAction.PressBackButton:
                        PressBackBtn();
                        break;
                    case ActMobileDevice.eMobileDeviceAction.PressHomeButton:
                        PressHomeBtn();
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
                    case ActMobileDevice.eMobileDeviceAction.TakeScreenShot:
                        TakeScreenShot(act);
                        break;
                }
            }
            catch (Exception ex) {
                act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
            }
        }

        //Back Click Action
        private void PressBackBtn() {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "BACK");
            Driver.ExecuteScript("mobile:presskey", params1);
        }

        //Home Click Action
        private void PressHomeBtn() {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            params1.Add("keySequence", "HOME");
            Driver.ExecuteScript("mobile:presskey", params1);
        }

        //Swipe Action
        private void SwipeScreen(eSwipeSide side) {
            Dictionary<String, Object> params1 = new Dictionary<String, Object>();
            switch (side) {
                case eSwipeSide.Down:
                    params1.Add("start", "50%,80%");
                    params1.Add("end", "50%,20%");
                    Driver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Up:
                    params1.Add("start", "50%,20%");
                    params1.Add("end", "50%,80%");
                    Driver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Left:
                    params1.Add("start", "20%,50%");
                    params1.Add("end", "80%,50%");
                    Driver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
                case eSwipeSide.Right:
                    params1.Add("start", "80%,50%");
                    params1.Add("end", "20%,50%");
                    Driver.ExecuteScript("mobile:touch:swipe", params1);
                    break;
            }
        }

        //Wait Action
        private void WaitAction(ActGenElement act) {
            try {
                int number = Int32.Parse(act.ValueForDriver);
                Thread.Sleep(number * 1000);
            }
            catch (FormatException) {

                //TODO: give message to user in grid
                //if format isn't right

            }
            catch (OverflowException) {
                //TODO: give message to user in grid
                //totally bogus value
            }
        }

        private void TakeScreenShot(Act act) {
            //Deal with screen shots here
        }

        public IWebElement LocateElement(Act act, bool AlwaysReturn = false) {
            eLocateBy LocatorType = act.LocateBy;
            string LocValue = act.LocateValueCalculated;
            IWebElement elem = null;
            
            if (LocatorType == eLocateBy.ByID) {
                elem = Driver.FindElementById(LocValue);
            }
            if (LocatorType == eLocateBy.ByXPath) {
                elem = Driver.FindElementByXPath(LocValue);
            }
            return elem;
        }

        public override ePlatformType Platform { get { return ePlatformType.Mobile; } }

        public override bool IsRunning() {
            return ConnectedToDevice;
        }

        private string GetKeyName(string skey) {
            switch (skey) {
                case "Keys.Alt":
                    return OpenQA.Selenium.Keys.Alt;
                case "Keys.ArrowDown":
                    return OpenQA.Selenium.Keys.ArrowDown;
                case "Keys.ArrowLeft":
                    return OpenQA.Selenium.Keys.ArrowLeft;
                case "Keys.ArrowRight":
                    return OpenQA.Selenium.Keys.ArrowRight;
                case "Keys.ArrowUp":
                    return OpenQA.Selenium.Keys.ArrowUp;
                case "Keys.Backspace":
                    return OpenQA.Selenium.Keys.Backspace;

                case "Keys.Cancel":
                    return OpenQA.Selenium.Keys.Cancel;

                case "Keys.Clear":
                    return OpenQA.Selenium.Keys.Clear;

                case "Keys.Command":
                    return OpenQA.Selenium.Keys.Command;

                case "Keys.Control":
                    return OpenQA.Selenium.Keys.Control;

                case "Keys.Decimal":
                    return OpenQA.Selenium.Keys.Decimal;

                case "Keys.Delete":
                    return OpenQA.Selenium.Keys.Delete;

                case "Keys.Divide":
                    return OpenQA.Selenium.Keys.Divide;

                case "Keys.Down":
                    return OpenQA.Selenium.Keys.Down;

                case "Keys.End":
                    return OpenQA.Selenium.Keys.End;

                case "Keys.Enter":
                    return OpenQA.Selenium.Keys.Enter;

                case "Keys.Equal":
                    return OpenQA.Selenium.Keys.Equal;

                case "Keys.Escape":
                    return OpenQA.Selenium.Keys.Escape;

                case "Keys.F1":
                    return OpenQA.Selenium.Keys.F1;

                case "Keys.F10":
                    return OpenQA.Selenium.Keys.F10;

                case "Keys.F11":
                    return OpenQA.Selenium.Keys.F11;

                case "Keys.F12":
                    return OpenQA.Selenium.Keys.F12;

                case "Keys.F2":
                    return OpenQA.Selenium.Keys.F2;

                case "Keys.F3":
                    return OpenQA.Selenium.Keys.F3;

                case "Keys.F4":
                    return OpenQA.Selenium.Keys.F4;

                case "Keys.F5":
                    return OpenQA.Selenium.Keys.F5;

                case "Keys.F6":
                    return OpenQA.Selenium.Keys.F6;

                case "Keys.F7":
                    return OpenQA.Selenium.Keys.F7;

                case "Keys.F8":
                    return OpenQA.Selenium.Keys.F8;

                case "Keys.F9":
                    return OpenQA.Selenium.Keys.F9;

                case "Keys.Help":
                    return OpenQA.Selenium.Keys.Help;

                case "Keys.Home":
                    return OpenQA.Selenium.Keys.Home;

                case "Keys.Insert":
                    return OpenQA.Selenium.Keys.Insert;

                case "Keys.Left":
                    return OpenQA.Selenium.Keys.Left;

                case "Keys.LeftAlt":
                    return OpenQA.Selenium.Keys.LeftAlt;

                case "Keys.LeftControl":
                    return OpenQA.Selenium.Keys.LeftControl;

                case "Keys.LeftShift":
                    return OpenQA.Selenium.Keys.LeftShift;

                case "Keys.Meta":
                    return OpenQA.Selenium.Keys.Meta;

                case "Keys.Multiply":
                    return OpenQA.Selenium.Keys.Multiply;

                case "Keys.Null":
                    return OpenQA.Selenium.Keys.Null;

                case "Keys.NumberPad0":
                    return OpenQA.Selenium.Keys.NumberPad0;

                case "Keys.NumberPad1":
                    return OpenQA.Selenium.Keys.NumberPad1;

                case "Keys.NumberPad2":
                    return OpenQA.Selenium.Keys.NumberPad2;

                case "Keys.NumberPad3":
                    return OpenQA.Selenium.Keys.NumberPad3;

                case "Keys.NumberPad4":
                    return OpenQA.Selenium.Keys.NumberPad4;

                case "Keys.NumberPad5":
                    return OpenQA.Selenium.Keys.NumberPad5;

                case "Keys.NumberPad6":
                    return OpenQA.Selenium.Keys.NumberPad6;

                case "Keys.NumberPad7":
                    return OpenQA.Selenium.Keys.NumberPad7;

                case "Keys.NumberPad8":
                    return OpenQA.Selenium.Keys.NumberPad8;

                case "Keys.NumberPad9":
                    return OpenQA.Selenium.Keys.NumberPad9;

                case "Keys.PageDown":
                    return OpenQA.Selenium.Keys.PageDown;

                case "Keys.PageUp":
                    return OpenQA.Selenium.Keys.PageUp;

                case "Keys.Pause":
                    return OpenQA.Selenium.Keys.Pause;

                case "Keys.Return":
                    return OpenQA.Selenium.Keys.Return;

                case "Keys.Right":
                    return OpenQA.Selenium.Keys.Right;

                case "Keys.Semicolon":
                    return OpenQA.Selenium.Keys.Semicolon;

                case "Keys.Separator":
                    return OpenQA.Selenium.Keys.Separator;

                case "Keys.Shift":
                    return OpenQA.Selenium.Keys.Shift;

                case "Keys.Space":
                    return OpenQA.Selenium.Keys.Space;

                case "Keys.Subtract":
                    return OpenQA.Selenium.Keys.Subtract;

                case "Keys.Tab":
                    return OpenQA.Selenium.Keys.Tab;

                case "Keys.Up":
                    return OpenQA.Selenium.Keys.Up;
                default:
                    return skey;

            }
        }


        public override Act GetCurrentElement() {
            try {
                Act act = null;
                IWebElement currentElement = Driver.SwitchTo().ActiveElement();

                string tagname = currentElement.TagName;

                if (tagname == "input") {
                    string ctlType = currentElement.GetAttribute("type");

                    switch (ctlType) {
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

                if (tagname == "a") {
                    act = getActLink(currentElement);
                    return act;
                }
                return null;
            }
            catch (Exception ex) {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");

                return null;
            }
        }

        //----------------------------getAct... methods---------------------------------------------
        //TODO:change to use ActUIElement
        private Act getActTextBox(IWebElement currentElement) {
            ActTextBox a = new ActTextBox();
            setActLocator(currentElement, a);
            a.TextBoxAction = ActTextBox.eTextBoxAction.SetValue;
            a.Value = currentElement.GetAttribute("value");
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
        }

        private Act getActButton(IWebElement currentElement) {
            ActButton act = new ActButton();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActPassword(IWebElement currentElement) {
            ActPassword a = new ActPassword();
            setActLocator(currentElement, a);
            a.PasswordAction = ActPassword.ePasswordAction.SetValue;
            a.Value = currentElement.GetAttribute("value");
            a.AddOrUpdateReturnParamActual("Actual", "Tag Name = " + currentElement.TagName);
            return a;
        }

        private Act getActCheckbox(IWebElement currentElement) {
            ActCheckbox act = new ActCheckbox();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActRadioButton(IWebElement currentElement) {
            ActRadioButton act = new ActRadioButton();
            string locVal = currentElement.GetAttribute("id");
            act.LocateBy = eLocateBy.ByID;
            act.LocateValue = locVal;
            return act;
        }

        private Act getActLink(IWebElement currentElement) {
            ActLink al = new ActLink();
            setActLocator(currentElement, al);
            al.Value = currentElement.Text;
            return al;
        }

        //---------------------End of getAct... methods-----------------------------

        public void setActLocator(IWebElement currentElement, Act act) {
            //order by priority

            // By ID
            string locVal = currentElement.GetAttribute("id");
            if (locVal != "") {
                act.LocateBy = eLocateBy.ByID;
                act.LocateValue = locVal;
                return;
            }

            // By name
            locVal = currentElement.GetAttribute("name");
            if (locVal != "") {
                act.LocateBy = eLocateBy.ByName;
                act.LocateValue = locVal;
                return;
            }

            //TODO: CSS....

            //By href
            locVal = currentElement.GetAttribute("href");
            if (locVal != "") {
                act.LocateBy = eLocateBy.ByHref;
                act.LocateValue = locVal;
                return;
            }

            //By Value
            locVal = currentElement.GetAttribute("value");
            if (locVal != "") {
                act.LocateBy = eLocateBy.ByValue;
                act.LocateValue = locVal;
                return;
            }

            // by text
            locVal = currentElement.Text;
            if (locVal != "") {
                act.LocateBy = eLocateBy.ByLinkText;
                act.LocateValue = locVal;
                return;
            }

            //TODO: add XPath
        }

        public override string GetURL() {
            return "TBD";
        }

        public override List<ActButton> GetAllButtons() {
            return null;
        }

        public override List<ActWindow> GetAllWindows() {
            return null;
        }

        public override List<ActLink> GetAllLinks() {
            return null;
        }

        public override void HighlightActElement(Act act) {
        }
    }
}