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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.MultiTouch;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace GingerCore.Drivers.Appium
{


    public class SeleniumAppiumDriver : DriverBase, IWindowExplorer, Amdocs.Ginger.Plugin.Core.IRecord
    {
        public override bool IsSTAThread()
        {
            return true;
        }

        public enum eDeviceType
        {
            Phone = 0,
            Tablet = 1
        }

        public enum eSeleniumPlatformType
        {
            Android = 0,
            iOS = 1,
            AndroidBrowser = 2,
            iOSBrowser = 3,
        }

        public enum eSwipeSide
        {
            Up, Down, Left, Right
        }
        public bool ConnectedToDevice=false;
        
        public override ePlatformType Platform { get { return ePlatformType.Mobile; }} 

        //Mobile Agent configurations
        [UserConfigured]
        [UserConfiguredDefault("False")]
        [UserConfiguredDescription("Set to 'True' if you want Appium Server to start Automatically - Available only if Ginger and Appium installed and running on the same machine")]
        public Boolean StartAppiumServerAutomatically { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("127.0.0.1")]
        [UserConfiguredDescription("Appium server location IP address - It can be on the local or remote host")]
        public String AppiumServerIP { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("4723")]
        [UserConfiguredDescription("Set specific Appium server Port Or 'Dynamic' for auto port allocation (Start searching from port 4723 with jumps of 2)")]
        public String AppiumServerPort { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("C:/Program Files (x86)/Appium")]
        [UserConfiguredDescription("Appium installation folder Path")]
        public String AppiumInstallationFolderPath { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("")]   //Default was http://127.0.0.1:4723/wd/hub
        [UserConfiguredDescription("Deprecated - Keep empty and use AppiumServerIP and AppiumServerPort instead")]
        public String AppiumServer { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("For Android set any name. For iOS, this should be the actual device name")]
        public String DeviceName { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("The tested device operating system version- e.g.: 4.4.4, 5.0")]
        public String DevicePlatformVersion { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("The tested device Unique identifier")]
        public String DeviceID { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Phone")] 
        [UserConfiguredDescription("The device type, set it to 'Phone' or 'Tablet'")]
        public String DeviceType { get; set; }


        [UserConfigured]
        [UserConfiguredDescription("The absolute local path or remote http URL to an .ipa (iOS) or .apk (Android) file, or a .zip containing one of these. Appium will attempt to install this app on the appropriate device first")]
        public String AppInstallerPath { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Android Only | Only if 'AppInstallerPath' is empty. Java package of the Android app you want to run, e.g. \"com.example.android.myApp\" or \"com.android.dialer\"")]
        public String InstalledAppPackage { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("Android Only | Only if 'InstalledAppPackage' was populated. The Android activity you want to launch from your package. This often needs to be preceded by a '.' (e.g., .MainActivity instead of MainActivity)")]
        public String InstalledAppActivity { get; set; }

        [UserConfigured]
        [UserConfiguredDescription("iOS Only | Only if 'AppInstallerPath' is empty. Bundle ID of the app under test. Useful for starting an installed app on a real device or for using other caps which require the bundle ID during test startup.")]
        public String InstalledAppBundleID { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("Yes")]
        [UserConfiguredDescription("Set to 'Yes' or 'No', determine if the device window in Ginger will refresh the device screenshot after each action been performed")]
        public String RefreshDeviceScreenShots { get; set; }

        [UserConfigured]
        [UserConfiguredDefault("10")]
        [UserConfiguredDescription("Implicit Wait for Appium Action Completion")]
        public int ImplicitWait { get; set; }

        private static TimeSpan INIT_TIMEOUT_SEC = TimeSpan.FromSeconds(90);
        private static TimeSpan IMPLICIT_TIMEOUT_SEC = TimeSpan.FromSeconds(10);
        private static TimeSpan SERVER_TIMEOUT_SEC = TimeSpan.FromSeconds(600);
        
        private AppiumDriver<AppiumWebElement> Driver;//appium on top selenium
        private SeleniumDriver mSeleniumDriver;//selenium base
        public eSeleniumPlatformType DriverPlatformType;
        public eDeviceType DriverDeviceType;
        AppiumDriverWindow DriverWindow;

        public SeleniumAppiumDriver(eSeleniumPlatformType platformType, BusinessFlow BF)
        {
            DriverPlatformType = platformType;
            BusinessFlow = BF;
        }

        private static List<int> reservedPorts = new List<int>();
        private int bootstrapPort = 0;
        private int chromeDriverPort = 0;
        public override void StartDriver()
        {
            CreateSTA(ShowDriverWindow);
        }

        public void ShowDriverWindow()
        {
            //show mobile window
            DriverWindow = new AppiumDriverWindow();
            DriverWindow.BF = BusinessFlow;
            DriverWindow.AppiumDriver = this;
            DriverWindow.DesignWindowInitialLook();
            DriverWindow.Show();
            for (int i = 0; i < 100; i++)
            {
                Thread.Sleep(100);
            }
      
            ConnectedToDevice = ConnectToAppium();
            if (ConnectedToDevice)
            {               
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);
                Dispatcher = new DriverWindowDispatcher(DriverWindow.Dispatcher);
                System.Windows.Threading.Dispatcher.Run();
            
            }
            else
            {
                if (DriverWindow != null)
                {
                    DriverWindow.Close();
                    DriverWindow = null;
                }
            }
        }

        public bool ConnectToAppium()
        {
            try//Adding back the Try-Catch because without it in case of connection issue Ginger crash
            {
                //Check if we need to use old IP and Port settings
                if (String.IsNullOrEmpty(AppiumServer)==false) {
                    //Get IP address and port from old settings - AppiumServer string value
                    GetPortAndIPFromOldSettings();
                }

                //Check if the user want to launch Appium server automatically
                if (StartAppiumServerAutomatically) {
                    //Start Appium server
                    //return false if Appium server failed to launch
                    if (StartAppiumServer() == false) {
                        return false;
                    }
                }

                //If not starting Appium Server automatic and Port set to dynamic - Throw exception and tell the user to insert a valid port number - The same as in manually started appium sever
                if (!StartAppiumServerAutomatically && AppiumServerPort.Equals("dynamic", StringComparison.InvariantCultureIgnoreCase)) {
                    throw new Exception("Insert Valid Appium Server Port number");
                }

                //set params
                Uri serverUri = new Uri("http://" + AppiumServerIP + ":" + AppiumServerPort + "/wd/hub");
                if (DeviceType != null && DeviceType.Trim().ToUpper() == "TABLET")
                    DriverDeviceType = eDeviceType.Tablet;
                else
                    DriverDeviceType = eDeviceType.Phone;

                //Setting capabilities
                DriverOptions driverOptions = this.GetCapabilities();

                driverOptions.AddAdditionalCapability("newCommandTimeout", INIT_TIMEOUT_SEC.TotalSeconds);

                foreach (DriverConfigParam UserCapability in AdvanceDriverConfigurations)
                {
                 
                    driverOptions.AddAdditionalCapability(UserCapability.Parameter, UserCapability.Value);
                }
                //creating driver
                switch (DriverPlatformType)
                {
                    case eSeleniumPlatformType.Android:
                        Driver = new AndroidDriver<AppiumWebElement>(serverUri, driverOptions, INIT_TIMEOUT_SEC);
                        break;
                    case eSeleniumPlatformType.iOS:
                        Driver = new IOSDriver<AppiumWebElement>(serverUri, driverOptions, INIT_TIMEOUT_SEC);
                        break;
                    case eSeleniumPlatformType.AndroidBrowser:
                        Driver = new AndroidDriver<AppiumWebElement>(serverUri, driverOptions, INIT_TIMEOUT_SEC);
                        Driver.Navigate().GoToUrl("http://www.google.com");
                        break;
                    case eSeleniumPlatformType.iOSBrowser:
                        //TODO: start ios-web-proxy automatically
                        Driver = new IOSDriver<AppiumWebElement>(serverUri, driverOptions, INIT_TIMEOUT_SEC);
                        break;
                }
                mSeleniumDriver = new SeleniumDriver(Driver); //used for running regular Selenium actions
                if (ImplicitWait == 0)
                    ImplicitWait = 10;
              //  Driver.Manage().Timeouts().ImplicitWait=TimeSpan.FromSeconds((int)ImplicitWait);

                return DriverWindow.LoadMobileScreenImage(false, 0);
            }
            catch (Exception ex)
            {
                    Reporter.ToUser(eUserMsgKey.MobileConnectionFailed, ex.Message);
                return false;
            }      
        }

        //Get IP address and port from old settings - AppiumServer string value
        //Return false if couldn't extract IP or port
        private void GetPortAndIPFromOldSettings() {
            try {
                var uri = new Uri(AppiumServer);
                //Extract IP address from String and check if it is valid
                var IPFromString = uri.GetComponents(UriComponents.Host, UriFormat.UriEscaped);
                if (String.IsNullOrEmpty(IPFromString) == false) {
                    AppiumServerIP = IPFromString;
                }

                //Extract PORT from String and check if it is valid
                var PortFromString = uri.GetComponents(UriComponents.Port, UriFormat.UriEscaped);
                if (String.IsNullOrEmpty(PortFromString) == false) {
                    AppiumServerPort = PortFromString;
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        private Boolean StartAppiumServer() {
            String AppiumNodeSuffixPath = "/node.exe";
            String AppiumJSSuffixPath = "/node_modules/appium/bin/appium.js";

            String AppiumNodePath = AppiumInstallationFolderPath + AppiumNodeSuffixPath;
            String AppiumJSPath = "\""+AppiumInstallationFolderPath + AppiumJSSuffixPath + "\"";

            String userFolder = System.Environment.GetEnvironmentVariable("USERPROFILE");
            System.IO.Directory.CreateDirectory(userFolder+"\\Ginger\\Appium");
            String logPathDirectory = userFolder + "\\Ginger\\Appium\\";
            DeleteOldLogs(logPathDirectory);

            DateTime currentTime = DateTime.Now;
            String AppiumLogFileName = "AppiumLog"+currentTime.ToString("ddMMyyyyHHmm")+".txt";

            String logPath = logPathDirectory+AppiumLogFileName;

            //Check if the user set Dynamic port or specefic port number
            if (AppiumServerPort.Equals("dynamic", StringComparison.InvariantCultureIgnoreCase) || !IsPortFree(AppiumServerPort)) {
                //Choose free port for launching current Appium server
                AppiumServerPort =  AllocateAppiumPort(4723).ToString();

            }

            //Start Appium Server launching process
            System.Diagnostics.Process process;
            process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden; //Change to Maximized or Hidden to debug Appium
            startInfo.FileName = AppiumNodePath;
            bootstrapPort = AllocateAppiumPort();
            chromeDriverPort = AllocateAppiumPort();
   
            startInfo.Arguments = AppiumJSPath + " --address " + AppiumServerIP + " --port " + AppiumServerPort + "--bootstrap-port "+ bootstrapPort.ToString ()+ "--chromedriver-port"+ chromeDriverPort.ToString() + " --automation-name Appium --log-no-color --session-override --log " + logPath;
            process.StartInfo = startInfo;
            process.Start();

            Thread.Sleep(1000);
            for (int i = 0; i < DriverLoadWaitingTime; i++) {
                Thread.Sleep(1000);
                Boolean? IsAppiumLaunched = CheckIfAppiumLaunched(logPath);
                //TRUE - found text in log: "listener started on"
                //FALSE - found text "error"
                //NULL - still didn't find anything
                if (IsAppiumLaunched != null && IsAppiumLaunched == true) {
                    return true;
                }
            }
            //Appium server didn't launched successfully - timeout
            return false;
        }

        //Delete Appium log files that are older of today's date
        private void DeleteOldLogs(String appiumLogsPath)
        {
            string[] logFileNames = Directory.GetFiles(appiumLogsPath);
            foreach (String currFileName in logFileNames) {
                DateTime dt = File.GetLastWriteTime(currFileName);
                if ((int)DateTime.Now.Subtract(dt).TotalDays > 0) {
                    try {
                        File.Delete(currFileName);
                    }catch(Exception e) {
                        //Couldn't delete log file
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                    }
                }
            }
        }

        //Read Appium log file and check Appium server launch status
        private Boolean? CheckIfAppiumLaunched(String logPath) {
            try{
                String textFromLog;
                var fileStream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (var streamReader = new StreamReader(fileStream)) {
                    textFromLog = streamReader.ReadToEnd();
                }
                if (textFromLog.Contains("listener started on")) {
                    //Appium Server Launched
                    return true;
                }else if(textFromLog.Contains("error")){
                    //Error launching Appium server
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex) {
                //Access denied - probably when file is being used by another process
                Thread.Sleep(2000);
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
            catch(Exception e){
                //Still launching Appium Server Or reading file exception
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                return null;
            }
            //Still launching Appium Server
            return null;
        }


        //When the user set 'Dynamic' port
        //Search and choose free port and Launch Appium server with that port
        private int AllocateAppiumPort(int defaultPort=4800) {
            Boolean foundFreePort = false;
            int portStartValue = defaultPort;

            while (!foundFreePort) {

                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
                Boolean portTaken = false;
                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray) {
                    if (tcpi.LocalEndPoint.Port == portStartValue) {
                        portTaken = true;
                        break;
                    }
                }
                if(portTaken || reservedPorts.Contains(portStartValue)){
                    portStartValue += 2;
                }else{
                    foundFreePort = true;
                    reservedPorts.Add(portStartValue);
                }
            }
            return portStartValue;
        }

        //Check if specific port is free before starting Appium
        private Boolean IsPortFree(String portToCheck) {
            Boolean portIsFree = true;
            int intPortToCheck;
            Int32.TryParse(portToCheck, out intPortToCheck);
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();
            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray) {
                Console.WriteLine("port is: " + tcpi.LocalEndPoint.Port);
                if (tcpi.LocalEndPoint.Port == intPortToCheck) {
                    portIsFree = false;
                    break;
                }
            }
            return portIsFree;
        }

        private DriverOptions GetCapabilities()
        {
            DriverOptions driverOptions = new AppiumOptions();

            //see http://appium.io/slate/en/master/?csharp#appium-server-capabilities for full list of capabilities values

            switch (DriverPlatformType)
            {
                case eSeleniumPlatformType.Android:
                    //Appium capabilities
                    //capabilities.SetCapability("automationName", "Appium");
                    //capabilities.SetCapability("platformName", "Android");                    
                    driverOptions.AddAdditionalCapability("newCommandTimeout", SERVER_TIMEOUT_SEC); //How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session

                    //Tested device capabilities
                    driverOptions.AddAdditionalCapability("deviceName", DeviceName); //don't really impact anything, can be used for AppPackage+AppActivity switch case based on device type
                    driverOptions.AddAdditionalCapability("platformVersion", DevicePlatformVersion);//Mobile OS version                  
                    if (!string.IsNullOrEmpty(DeviceID))
                        driverOptions.AddAdditionalCapability("udid", DeviceID);

                    //Tested application capabilities     
                    if (!string.IsNullOrEmpty(AppInstallerPath))
                    {
                        driverOptions.AddAdditionalCapability("app", AppInstallerPath);
                        driverOptions.AddAdditionalCapability("browserName", "");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(InstalledAppPackage))
                        {
                            driverOptions.AddAdditionalCapability("appPackage", InstalledAppPackage);
                            driverOptions.AddAdditionalCapability("appActivity", InstalledAppActivity);
                            driverOptions.AddAdditionalCapability("appWaitPackage", InstalledAppPackage);
                            driverOptions.AddAdditionalCapability("appWaitActivity", InstalledAppActivity);
                        }
                    }
                    break;

                case eSeleniumPlatformType.iOS:
                    //Appium capabilities
                    //capabilities.SetCapability("automationName", "Appium");                    
                    //capabilities.SetCapability("platformName", "iOS");
                    driverOptions.AddAdditionalCapability("newCommandTimeout", SERVER_TIMEOUT_SEC); //How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session

                    //Tested device capabilities
                    driverOptions.AddAdditionalCapability("deviceName", DeviceName); 
                    driverOptions.AddAdditionalCapability("platformVersion", DevicePlatformVersion);//Mobile OS version 
                    if (!string.IsNullOrEmpty(DeviceID))
                        driverOptions.AddAdditionalCapability("udid", DeviceID);

                    //Tested application capabilities     
                    if (!string.IsNullOrEmpty(AppInstallerPath))
                    {
                        driverOptions.AddAdditionalCapability("app", AppInstallerPath);
                        driverOptions.AddAdditionalCapability("browserName", "");
                    }
                    if (!string.IsNullOrEmpty(InstalledAppBundleID))
                        driverOptions.AddAdditionalCapability("bundleId", InstalledAppBundleID);                   
                    break;

                case eSeleniumPlatformType.AndroidBrowser:
                    //Appium capabilities
                    driverOptions.AddAdditionalCapability("newCommandTimeout", SERVER_TIMEOUT_SEC); //How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session

                    //Tested device capabilities
                    driverOptions.AddAdditionalCapability("deviceName", DeviceName); //don't really impact anything, can be used for AppPackage+AppActivity switch case based on device type
                    driverOptions.AddAdditionalCapability("platformVersion", DevicePlatformVersion);//Mobile OS version  
                    if (!string.IsNullOrEmpty(DeviceID))
                        driverOptions.AddAdditionalCapability("udid", DeviceID);

                    //Tested application capabilities 
                    driverOptions.AddAdditionalCapability("browserName", "chrome");
                    //capabilities.SetCapability("chromedriverExecutable", ""); //The absolute local path to webdriver executable (if Chromium embedder provides its own webdriver, it should be used instead of original chromedriver bundled with Appium); exp: /abs/path/to/webdriver
                    //capabilities.SetCapability("chromeOptions", ""); //Allows passing chromeOptions capability for chrome driver. For more information see chromeOptions: https://sites.google.com/a/chromium.org/chromedriver/capabilities
                    break;

                case eSeleniumPlatformType.iOSBrowser:
                    //Appium capabilities
                    driverOptions.AddAdditionalCapability("newCommandTimeout", SERVER_TIMEOUT_SEC); //How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session

                    //Tested device capabilities
                    driverOptions.AddAdditionalCapability("deviceName", DeviceName);
                    driverOptions.AddAdditionalCapability("platformVersion", DevicePlatformVersion);//Mobile OS version 
                    if (!string.IsNullOrEmpty(DeviceID))
                        driverOptions.AddAdditionalCapability("udid", DeviceID);

                    //Tested application capabilities 
                    driverOptions.AddAdditionalCapability("browserName", "safari");
                    //capabilities.setCapability("safariInitialUrl", "");  //(Sim-only) (>= 8.1) Initial safari url, default is a local welcome page
                    driverOptions.AddAdditionalCapability("safariAllowPopups", false);  //(Sim-only) Prevent Safari from showing a fraudulent website warning. Default keeps current sim setting.
                    //capabilities.setCapability("safariOpenLinksInBackground", false);  //(Sim-only) Whether Safari should allow links to open in new windows. Default keeps current sim setting.
                    if (!string.IsNullOrEmpty(InstalledAppBundleID))
                        driverOptions.AddAdditionalCapability("bundleId", InstalledAppBundleID);
                    break;
            }

            return driverOptions;                        
        }


        public override void CloseDriver()
        {
            reservedPorts.Remove(chromeDriverPort);
            reservedPorts.Remove(bootstrapPort);

            try { 
                if (DriverWindow != null){
                    DriverWindow.Close();
                    DriverWindow = null;
                }
            } catch (InvalidOperationException e) {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }

            if (Driver != null){
                Driver.Quit();

                //Check if needed also to close Appium server
                if (StartAppiumServerAutomatically) {
                    //Close Appium connection
                    CloseAppiumConnection();
                }
            }

            ConnectedToDevice = false;
        }
        
        //Close Appium connection
        private Boolean CloseAppiumConnection() {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "CMD.exe";
            String[] stopServerCommand = new String[]{"cmd", "/c",
                    "echo off & FOR /F \"usebackq tokens=5\" %a in (`netstat -nao ^| findstr /R /C:\""
                    + AppiumServerPort
                    + " \"`) do (FOR /F \"usebackq\" %b in (`TASKLIST /FI \"PID eq %a\" ^| findstr /I node.exe`) do taskkill /F /PID %a)"};
            startInfo.Arguments = string.Join("", stopServerCommand);
            process.StartInfo = startInfo;
            process.Start();

            return true;
        }
        
        public List<IWebElement> LocateElements(eLocateBy LocatorType, string LocValue)
        {
            if (DriverPlatformType == eSeleniumPlatformType.AndroidBrowser || DriverPlatformType == eSeleniumPlatformType.iOSBrowser)
                return mSeleniumDriver.LocateElements(LocatorType, LocValue);

            IReadOnlyCollection<IWebElement> elem = null;

            switch (LocatorType)
            {
                //need to override regular selenium driver locator if needed, 
                //if not then to run the regular selenium driver locator for it to avoid duplication

                default:
                    elem = mSeleniumDriver.LocateElements(LocatorType, LocValue);
                    break;
            }

            return elem.ToList();
        }

        public IWebElement LocateElement(Act act)
        {
                if (DriverPlatformType == eSeleniumPlatformType.AndroidBrowser || DriverPlatformType == eSeleniumPlatformType.iOSBrowser)
                    return mSeleniumDriver.LocateElement(act);

            eLocateBy LocatorType = act.LocateBy;
                IWebElement elem = null;

                switch (LocatorType)
                {
                    case eLocateBy.ByResourceID:
                    {
                        elem = Driver.FindElementById(act.LocateValue);
                        break;
                    }
                    //need to override regular selenium driver locator if needed, 
                    //if not then to run the regular selenium driver locator for it to avoid duplication                

                    default:
                        elem = mSeleniumDriver.LocateElement(act);
                        break;
                }

                return elem;
        }

        public override Act GetCurrentElement()
        {
            return mSeleniumDriver.GetCurrentElement();
        }

        public override void RunAction(Act act)
        {
            try
            {
                if (DriverPlatformType == eSeleniumPlatformType.AndroidBrowser || DriverPlatformType == eSeleniumPlatformType.iOSBrowser)
                {
                    if (DriverPlatformType == eSeleniumPlatformType.AndroidBrowser)
                        DriverWindow.ShowActionEfect(false); //ios connection is crashing if to much screen shots taken
                    mSeleniumDriver.RunAction(act);
                    DriverWindow.ShowActionEfect(true, 1500);
                    return;
                }
                //No need to take screen shot before running action
                // DriverWindow.ShowActionEfect(false);
                // Thread.Sleep(10);
                Type ActType = act.GetType();

                if (ActType == typeof(ActMobileDevice))
                {
                    MobileDeviceActionHandler((ActMobileDevice)act);
                    return;
                }
                if (ActType == typeof(ActGenElement))
                {
                    GenElementHandler((ActGenElement)act);
                    return;
                }

                if (ActType == typeof(ActSmartSync))
                {
                    mSeleniumDriver.SmartSyncHandler((ActSmartSync)act);
                    return;
                }
                if (ActType == typeof(ActScreenShot))
                {
                    TakeScreenShot(act);
                    return;
                }
     
                act.Error = "Run Action Failed due to unrecognized action type: '" + ActType.ToString() + "'";
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            catch(Exception ex)
            {
                act.Error = "Run Action Failed, Error details: " + ex.Message;
                act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
        }
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
                            DriverWindow.ShowActionEfect(true, 1000);
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
                            DriverWindow.ShowActionEfect(true, 1000);
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
                            DriverWindow.ShowActionEfect(true, 1000);
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
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            switch (DriverPlatformType)
                            {
                                case SeleniumAppiumDriver.eSeleniumPlatformType.Android:
                                    e.SendKeys(act.GetInputParamCalculatedValue("Value"));                                    
                                    break;
                                case SeleniumAppiumDriver.eSeleniumPlatformType.iOS:
                                    ((IOSElement)e).SetImmediateValue(act.GetInputParamCalculatedValue("Value"));
                                    break;
                            }
                            DriverWindow.ShowActionEfect(true, 100);
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
                                        act.AddOrUpdateReturnParamActual ("source", this.GetPageSource ().Result);
                                        return;

                                    case "x":
                                    case "X":
                                        ActGenElement tempact = new ActGenElement ();                                      
                                        act.AddOrUpdateReturnParamActual ("X", e.Location.X.ToString());
                                        return;
                                    case "y":
                                    case "Y":
                                        act.AddOrUpdateReturnParamActual ("Y", e.Location.Y.ToString ());
                                        return; 
                                    default:
                                        if (act.LocateBy == eLocateBy.ByXPath)
                                        {
                                            XmlDocument PageSourceXml = new XmlDocument ();
                                            PageSourceXml.LoadXml (this.GetPageSource ().Result);
                                            XmlNode node = PageSourceXml.SelectSingleNode (act.LocateValueCalculated);

                                            foreach(XmlAttribute XA in node.Attributes)
                                            {
                                                if(XA.Name==act.ValueForDriver)
                                                {
                                                    act.AddOrUpdateReturnParamActual ("Actual", XA.Value);
                                                    break;
                                                }
                                            }
                                        }
                                        return;
                                }                               
                                attribute = e.GetAttribute(value);
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                            }
                            act.AddOrUpdateReturnParamActual("Actual", attribute);
                        }
                        else
                        {
                            act.Error = "Error: Element not found - " + act.LocateBy + "- '" + act.LocateValueCalculated + "'";
                            return;
                        }
                        break;

                    //case ActGenElement.eGenElementAction.ApplitoolsCheckPoint:
                    //    //TODO: add dynamic name for checkpoint
                    //    //eyes.CheckWindow(TimeSpan.FromSeconds(5),"Checkpoint name");
                    //break;

                    default:
                        mSeleniumDriver.GenElementHandler(act);
                        DriverWindow.ShowActionEfect(true, 1000);
                        break;
                }
            }
            catch(Exception ex)
            {
                act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
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
                        try
                        {
                            tc.Press(Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[0]), Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[1]));
                            DriverWindow.ShowActionEfect(true, 1000);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                        tc = new TouchAction(Driver);
                        try
                        {
                            tc.LongPress(Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[0]), Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[1]));
                            DriverWindow.ShowActionEfect(true, 1000);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;

                    case ActMobileDevice.eMobileDeviceAction.TapXY:
                        try
                        {
                            TapXY(Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[0]), Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[1]));
                            DriverWindow.ShowActionEfect(true, 1000);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;                   

                    case ActMobileDevice.eMobileDeviceAction.PressBackButton:
                        PressBackBtn();
                        DriverWindow.ShowActionEfect(true, 200);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressHomeButton:
                        PressHomebtn();
                        DriverWindow.ShowActionEfect(true, 200);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.PressMenuButton:
                        PressMenubtn();
                        DriverWindow.ShowActionEfect(true, 200);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeDown:
                        SwipeScreen(eSwipeSide.Down);
                        DriverWindow.ShowActionEfect(true, 300);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeUp:
                        SwipeScreen(eSwipeSide.Up);
                        DriverWindow.ShowActionEfect(true, 300);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeLeft:
                        SwipeScreen(eSwipeSide.Left);
                        DriverWindow.ShowActionEfect(true, 300);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeRight:
                        SwipeScreen(eSwipeSide.Right);
                        DriverWindow.ShowActionEfect(true, 300);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.Wait:
                        Thread.Sleep(Convert.ToInt32(act.GetInputParamCalculatedValue("Value")) * 1000);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.TakeScreenShot:
                        TakeScreenShot(act);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.RefreshDeviceScreenImage:
                        DriverWindow.LoadMobileScreenImage(false);
                        break;

                    case ActMobileDevice.eMobileDeviceAction.DragXYXY:                                             
                        try
                        {
                            DoDrag(     Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[0]),
                                        Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[1]),
                                        Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[2]),
                                        Convert.ToInt32(act.GetInputParamCalculatedValue("Value").Split(',')[3]));
                            DriverWindow.ShowActionEfect(true, 300);
                        }
                        catch (Exception ex)
                        {
                            act.Error = "Error: Action failed to be performed, Details: " + ex.Message;
                        }
                        break;
                    case ActMobileDevice.eMobileDeviceAction.OpenAppByName:
                        Driver.LaunchApp();
                        break;

                    case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:

                       
                            string[] arr = act.ValueForDriver.Split(',');
                            int x1 = Int32.Parse(arr[0]);
                        int y1 = Int32.Parse( arr[1]);
                        int x2 = Int32.Parse( arr[2]);
                        int y2 = Int32.Parse( arr[3]);
                        ITouchAction swipe;

                        swipe = BuildDragAction(Driver, x1, y1,x2,y2, 1000);
                        swipe.Perform();

                    
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

        private void TakeScreenShot(Act act)
        {
            try
            {
                ActScreenShot actss = (ActScreenShot)act;
                if (actss.WindowsToCapture == Act.eWindowsToCapture.OnlyActiveWindow && actss.Status != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    createScreenShot(act);
                }
                else
                {
                    String currentWindow;
                    currentWindow = Driver.CurrentWindowHandle;
                    ReadOnlyCollection<string> openWindows = Driver.WindowHandles;
                    foreach (String winHandle in openWindows)
                    {  
                        createScreenShot(act);
                        Driver.SwitchTo().Window(currentWindow);
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                act.Error = "Screen shot Error: Action failed to be performed, Details: " + ex.Message;
            }
        }

        private void createScreenShot(Act act)
        {
            Screenshot ss = ((ITakesScreenshot)Driver).GetScreenshot();
            string filename = Path.GetTempFileName();
            ss.SaveAsFile(filename, ScreenshotImageFormat.Png);

            Bitmap tmp = new System.Drawing.Bitmap(filename);
            try
            {
                DriverWindow.UpdateDriverImageFromScreenshot(ss);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            act.AddScreenShot(tmp);
        }
        
        public Screenshot GetScreenShot()
        {
            Screenshot ss=null;
            try
            {
                ss = Driver.GetScreenshot ();

            }
            catch
            {
                Bitmap bmp = new Bitmap (1024, 768);

                var ms = new MemoryStream ();


                bmp.Save (ms, System.Drawing.Imaging.ImageFormat.Png);
                var byteImage = ms.ToArray ();
                ss = new Screenshot (Convert.ToBase64String (byteImage));

            }
             return ss;
        }

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

        public void PressBackBtn()
        {
            switch (DriverPlatformType)
            {
                case eSeleniumPlatformType.Android:
                    Driver.Navigate().Back();
                    break;
                case eSeleniumPlatformType.iOS:
                    Reporter.ToUser(eUserMsgKey.MissingImplementation2);
                    break;
            }
        }

        public void PressHomebtn()
        {               
            switch (DriverPlatformType)
            {
                case eSeleniumPlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Home);
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(3);
                    break;
                case eSeleniumPlatformType.iOS:
                    Reporter.ToUser(eUserMsgKey.MissingImplementation2);
                    break;
            }
        }

        public void PressMenubtn()
        {
            switch (DriverPlatformType)
            {
                case eSeleniumPlatformType.Android:
                    ((AndroidDriver<AppiumWebElement>)Driver).PressKeyCode(AndroidKeyCode.Menu);
                    break;
                case eSeleniumPlatformType.iOS:
                    Reporter.ToUser(eUserMsgKey.MissingImplementation2);
                    break;
            }
        }

        public async Task<string> GetPageSource()
        {
            string Pagesource = String.Empty;
            await Task.Run(() =>
             {
                 Pagesource = Driver.PageSource;
             });
            return Pagesource;                  
        }

        public void SwipeScreen(eSwipeSide side)
        {
            ITouchAction swipe;
            System.Drawing.Size sz = Driver.Manage().Window.Size;

            switch (side)
            {
                case eSwipeSide.Down:
                    swipe = BuildDragAction(Driver, (int)(sz.Width * 0.5), (int)(sz.Height * 0.3), (int)(sz.Width * 0.5), (int)(sz.Height * 0.7), 1000);
                    swipe.Perform();
                    break;

                case eSwipeSide.Up:
                    swipe = BuildDragAction(Driver, (int)(sz.Width * 0.5), (int)(sz.Height * 0.7), (int)(sz.Width * 0.5), (int)(sz.Height * 0.3), 1000);
                    swipe.Perform();
                    break;

                case eSwipeSide.Left:
                    swipe = BuildDragAction(Driver, (int)(sz.Width * 0.8), (int)(sz.Height * 0.5), (int)(sz.Width * 0.1), (int)(sz.Height * 0.5), 1000);
                    swipe.Perform();
                    break;

                case eSwipeSide.Right:
                    swipe = BuildDragAction(Driver, (int)(sz.Width * 0.1), (int)(sz.Height * 0.5), (int)(sz.Width * 0.8), (int)(sz.Height * 0.5), 1000);
                    swipe.Perform();
                    break;
            }
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
            TouchAction drag =  new TouchAction(Driver);
            drag.Press(startX, startY).MoveTo(endX, endY).Release();
            drag.Perform();
        }

        public override string GetURL()
        {
            return "TBD";
        }

        

        public override bool IsRunning()
        {
            return ConnectedToDevice;           
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
            AW.Title = "Device";   // TODO: add device name and info
            

            list.Add(AW);
            
            return list;
        }

        void IWindowExplorer.SwitchWindow(string Title)
        {
            //NA
        }

        void IWindowExplorer.HighLightElement(ElementInfo ElementInfo, bool locateElementByItLocators = false)
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.HighLightElement((AppiumElementInfo)ElementInfo);
            });
        }

        string IWindowExplorer.GetFocusedControl()
        {
            return null;
        }

        ElementInfo IWindowExplorer.GetControlFromMousePosition()
        {           
            AppiumElementInfo AEI = null;
            Dispatcher.Invoke(() =>
            {
                XmlNode node = DriverWindow.GetElementXmlNodeFromMouse();
                if (node != null)
                {
                    AEI = new AppiumElementInfo();
                    AEI.XPath = GetXPathToNode(node);
                    AEI.XmlNode = node;
                }
            });

            return AEI;
        }

        AppWindow IWindowExplorer.GetActiveWindow()
        {
            return null;
        }
        List<ElementInfo> IWindowExplorer.GetVisibleControls(List<eElementType> filteredElementType, ObservableList<ElementInfo> foundElementsList = null, bool isPOMLearn = false)
        {
            List<ElementInfo> list = new List<ElementInfo>();

            string pageSourceString = Driver.PageSource;
            XmlDocument pageSourceXml = new XmlDocument();
            pageSourceXml.LoadXml(pageSourceString);


            //Get all elements but only clickable elements= user can interact with them
            XmlNodeList nodes = pageSourceXml.SelectNodes("//*");  
            for (int i = 0; i < nodes.Count; i++)
            {
                //Show only clickable elements
                if (nodes[i].Attributes != null)
                {
                    var cattr = nodes[i].Attributes["clickable"];
                    if (cattr != null)
                    {
                        if (cattr.Value == "false") continue;
                    }
                }
                AppiumElementInfo AEI = GetElementInfoforXmlNode(nodes[i]);                
                list.Add(AEI);
            }

            return list;
        }

        private AppiumElementInfo GetElementInfoforXmlNode(XmlNode xmlNode)
        {
            AppiumElementInfo AEI = new AppiumElementInfo();
            AEI.ElementTitle = GetNameFor(xmlNode);            
            AEI.ElementType = GetAttrValue(xmlNode, "class");
            AEI.Value = GetAttrValue(xmlNode, "text");
            if (string.IsNullOrEmpty(AEI.Value)) 
                {
                    AEI.Value = GetAttrValue(xmlNode, "content-desc");
                }
            AEI.XmlNode = xmlNode;
            AEI.XPath = GetNodeXPath(xmlNode);
            AEI.WindowExplorer = this;

            return AEI;
        }

        private string GetNodeXPath(XmlNode xmlNode)
        {
            string XPath = GetXPathToNode(xmlNode);
            return XPath;
        }

        /// Gets the X-Path to a given Node
        /// </summary>
        /// <param name="node">The Node to get the X-Path from</param>
        /// <returns>The X-Path of the Node</returns>
        public string GetXPathToNode(XmlNode node)
        {
            //TODO: verify XPath return 1 item back to same xmlnode.

            string resid = GetAttrValue(node, "resource-id");            
            if (!string.IsNullOrEmpty(resid))
            {
                return string.Format("//*[@resource-id='{0}']", resid);
            }
            
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
            return String.Format("{0}/{1}[{2}]", GetXPathToNode(node.ParentNode), node.Name, indexInParent);
        }

        List<ElementInfo> IWindowExplorer.GetElementChildren(ElementInfo ElementInfo)
        {            

            AppiumElementInfo EI = (AppiumElementInfo)ElementInfo;
            XmlNode node = EI.XmlNode;

            List<ElementInfo> list = new List<ElementInfo>();

          
            XmlNodeList nodes = node.ChildNodes;
            for(int i=0;i<nodes.Count;i++)
            {
                AppiumElementInfo AEI = GetElementInfoforXmlNode(nodes[i]);                                
                list.Add(AEI);
            }
            return list;
        }

        private string GetNameFor(XmlNode xmlNode)
        {
            string Name = GetAttrValue(xmlNode, "content-desc");
            if (!string.IsNullOrEmpty(Name)) return Name;

            string resid = GetAttrValue(xmlNode, "resource-id");
            if (!string.IsNullOrEmpty(resid))
            {
                // if we have resource id then get just the id out of it
                string[] a = resid.Split('/');
                Name = a[a.Length-1];
                return Name;
            }

            Name = GetAttrValue(xmlNode, "text");
            if (!string.IsNullOrEmpty(Name)) return Name;
            
            return xmlNode.Name;
        }

        string GetAttrValue(XmlNode xmlNode, string attr)
        {
            if (xmlNode.Attributes == null) return null;
            if (xmlNode.Attributes[attr] == null) return null;
            if (string.IsNullOrEmpty(xmlNode.Attributes[attr].Value)) return null;
            return xmlNode.Attributes[attr].Value;
        }
        
        
        ObservableList<ElementLocator> IWindowExplorer.GetElementLocators(ElementInfo ElementInfo)
        {
            ObservableList<ElementLocator> list = new ObservableList<ElementLocator>();

            AppiumElementInfo AEI = (AppiumElementInfo)ElementInfo;

            // Show XPath, can have relative info
            list.Add(new ElementLocator(){
                 LocateBy = eLocateBy.ByXPath,
                 LocateValue = AEI.XPath,
                 Help = "Highly Recommended when resourceid exist, long path with relative information is sensitive to screen changes"
            });


            //Only by Resource ID
            string resid = GetAttrValue(AEI.XmlNode, "resource-id");
            string residXpath = string.Format("//*[@resource-id='{0}']", resid);
            if (residXpath != AEI.XPath) // We show by res id when it is different then the elem XPath, so not to show twice the same, the AE.Apath can include relative info
            {
            list.Add(new ElementLocator()
            {
                LocateBy = eLocateBy.ByXPath,
                LocateValue = residXpath,
                Help = "Use Resource id only when you don't want XPath with relative info, but the resource-id is unique"
            });
            }

            //By Content-desc
            string contentdesc = GetAttrValue(AEI.XmlNode, "content-desc");
            if (!string.IsNullOrEmpty(contentdesc))
            {
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = string.Format("//*[@content-desc='{0}']", contentdesc),
                    Help = "content-desc is Recommended when resource-id not exist"
                });
            }

            // By Class and text
            string eClass = GetAttrValue(AEI.XmlNode, "class");
            string eText = GetAttrValue(AEI.XmlNode, "text");
            if (!string.IsNullOrEmpty(eClass) && !string.IsNullOrEmpty(eText))
            {
                list.Add(new ElementLocator()
                {
                    LocateBy = eLocateBy.ByXPath,
                    LocateValue = string.Format("//{0}[@text='{1}']", eClass, eText),    // like: //android.widget.RadioButton[@text='Ginger']" 
                    Help = "use class and text when you have list of items and no resource-id to use"
                });
            }
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
            XmlNode node = ((AppiumElementInfo)ElementInfo).XmlNode;

            ObservableList<ControlProperty> list = new ObservableList<ControlProperty>();

            if (node == null) return list;

            XmlAttributeCollection attrs = node.Attributes;

            if (attrs == null) return list;

            for (int i = 0; i < attrs.Count;i++ )
            {
                ControlProperty CP = new ControlProperty();
                CP.Name = attrs[i].Name;
                CP.Value = attrs[i].Value;
                list.Add(CP);
            }

            return list;
        }
        

        public event Amdocs.Ginger.Plugin.Core.RecordingEventHandler RecordingEvent;

        void Amdocs.Ginger.Plugin.Core.IRecord.ResetRecordingEventHandler()
        {
            RecordingEvent = null;
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.StartRecording(bool learnAdditionalChanges)
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StartRecording();
            });
        }

        void Amdocs.Ginger.Plugin.Core.IRecord.StopRecording()
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StopRecording();
            });
        }

        public override void StartRecording()
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StartRecording();
            });            
        }

        public override void StopRecording()
        {
            Dispatcher.Invoke(() =>
            {
                DriverWindow.StopRecording();
            });   
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
            return ((IWindowExplorer)mSeleniumDriver).IsElementObjectValid(obj);
        }

        public void UnHighLightElements()
        {
            throw new NotImplementedException();
        }

        public bool TestElementLocators(ElementInfo EI, bool GetOutAfterFoundElement = false)
        {
            throw new NotImplementedException();
        }

        public void CollectOriginalElementsDataForDeltaCheck(ObservableList<ElementInfo> originalList)
        {
            throw new NotImplementedException();
        }

        public ElementInfo GetMatchingElement(ElementInfo latestElement, ObservableList<ElementInfo> originalElements)
        {
            throw new NotImplementedException();
        }

        public void StartSpying()
        {
            throw new NotImplementedException();
        }
        public ElementInfo LearnElementInfoDetails(ElementInfo EI)
        {
            return EI;
        }

        ObservableList<OptionalValue> IWindowExplorer.GetOptionalValuesList(ElementInfo ElementInfo, eLocateBy elementLocateBy, string elementLocateValue)
        {
            throw new NotImplementedException();
        }
    }
}
