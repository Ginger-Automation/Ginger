#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Drivers;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.GeneralLib;
using Microsoft.VisualStudio.Services.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static GingerCore.Environments.Database;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for WebAgentConfigEditPage.xaml
    /// </summary>
    public partial class WebAgentConfigEditPage : Page
    {
       
        Context context=null;
        Agent mAgent;


        /// <summary>
        /// Initializes a new instance of the WebAgentConfigEditPage class.
        /// </summary>
        /// <param name="mAgent">The Agent object.</param>
        public WebAgentConfigEditPage(Agent mAgent)
        {
            this.mAgent = mAgent;
            InitializeComponent();

            bindElement();

            DriverConfigParam? browserTypeParam = mAgent.DriverConfiguration.FirstOrDefault(p => string.Equals(p.Parameter, nameof(GingerWebDriver.BrowserType)));
            if (browserTypeParam != null)
            {
                browserTypeParam.PropertyChanged += BrowserTypeParam_PropertyChanged;
            }
            WebBrowserType browserType = Enum.Parse<WebBrowserType>(browserTypeParam?.Value);
            edgeIEPnlVisibility(browserType);
            chromePnlvisibilitly(browserType);
            chromeFirefoxPnlVisibility(browserType);
            chromeFirefoxIEPnl(browserType);
        }

        /// <summary>
        /// Binds the elements of the page.
        /// </summary>
        void bindElement()
        {
            
            #region ProxyConfigration

            //Check if the proxy is auto detect
            DriverConfigParam autoDetect = mAgent.GetOrCreateParam(nameof(SeleniumDriver.AutoDetect));
            BindingHandler.ObjFieldBinding(xAutoDetectProxyCB, CheckBox.IsCheckedProperty, autoDetect, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xAutoDetectProxyCB, CheckBox.ToolTipProperty, autoDetect, nameof(DriverConfigParam.Description));

            //Add the proxy 
            DriverConfigParam proxyName = mAgent.GetOrCreateParam(nameof(SeleniumDriver.Proxy));
            xProxyVE.Init(null, proxyName, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xProxyVE, TextBox.ToolTipProperty, proxyName, nameof(DriverConfigParam.Description));

            //Add the by proxy 
            DriverConfigParam byproxyName = mAgent.GetOrCreateParam(nameof(SeleniumDriver.ByPassProxy));
            xByPassProxyVE.Init(null, byproxyName, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xByPassProxyVE, TextBox.ToolTipProperty, byproxyName, nameof(DriverConfigParam.Description));

            //Proxy auto conf url
            DriverConfigParam autoconfigurl = mAgent.GetOrCreateParam(nameof(SeleniumDriver.ProxyAutoConfigUrl));
            xProxyAutoConfigUrlVE.Init(null, autoconfigurl, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xProxyAutoConfigUrlVE, TextBox.ToolTipProperty, autoconfigurl, nameof(DriverConfigParam.Description));
            #endregion

            #region sessionManagement
            //pageLoadStrategy
            GingerCore.General.FillComboFromEnumType(xPageLoadStrategyComboBox, typeof(SeleniumDriver.ePageLoadStrategy));
            DriverConfigParam pageLoadStrategy = mAgent.GetOrCreateParam(nameof(SeleniumDriver.PageLoadStrategy));
            BindingHandler.ObjFieldBinding(xPageLoadStrategyComboBox, ComboBox.TextProperty, pageLoadStrategy, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xPageLoadStrategyComboBox, ComboBox.ToolTipProperty, pageLoadStrategy, nameof(DriverConfigParam.Description));

            //ImplicitWait
            DriverConfigParam implicitWait = mAgent.GetOrCreateParam(nameof(SeleniumDriver.ImplicitWait));
            xImplicitWaitVE.Init(null, implicitWait, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xImplicitWaitVE, TextBox.ToolTipProperty, implicitWait, nameof(DriverConfigParam.Description));

            //http server timeout
            DriverConfigParam httpServertimeout = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HttpServerTimeOut));
            xHttpServerTimeOutVE.Init(null, httpServertimeout, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xHttpServerTimeOutVE, TextBox.ToolTipProperty, httpServertimeout, nameof(DriverConfigParam.Description));

            //Page Load timeout
            DriverConfigParam pageloadtimeout = mAgent.GetOrCreateParam(nameof(SeleniumDriver.PageLoadTimeOut));
            xPageLoadTimeOutVE.Init(null, pageloadtimeout, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xPageLoadTimeOutVE, TextBox.ToolTipProperty, pageloadtimeout, nameof(DriverConfigParam.Description));

            //Driverload Waiting time
            DriverConfigParam driverLoadWaitingTime = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DriverLoadWaitingTime));
            xDriverLoadWaitingTimeVE.Init(null, driverLoadWaitingTime, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDriverLoadWaitingTimeVE, TextBox.ToolTipProperty, driverLoadWaitingTime, nameof(DriverConfigParam.Description));
            #endregion

            #region BrowserConfigration

            //Browser Version
            DriverConfigParam browserVersion = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserVersion));
            xBrowserVersionVE.Init(null, browserVersion, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserVersionVE, TextBox.ToolTipProperty, browserVersion, nameof(DriverConfigParam.Description));


            //Emulation Device Name
            DriverConfigParam emulationDeviceName = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EmulationDeviceName));
            xEmulationDeviceNameVE.Init(null, emulationDeviceName, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEmulationDeviceNameVE, TextBox.ToolTipProperty, emulationDeviceName, nameof(DriverConfigParam.Description));

            //Browser User Agent
            DriverConfigParam browserUserAgent = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserUserAgent));
            xBrowserUserAgentVE.Init(null, browserUserAgent, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserUserAgentVE, TextBox.ToolTipProperty, browserUserAgent, nameof(DriverConfigParam.Description));

            //user Profile folder Path
            DriverConfigParam userProfileFolderPath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.UserProfileFolderPath));
            xUserProfileFolderPathVE.Init(null, userProfileFolderPath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUserProfileFolderPathVE, TextBox.ToolTipProperty, userProfileFolderPath, nameof(DriverConfigParam.Description));

            //Download Folder Path
            DriverConfigParam downloadFolderPath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DownloadFolderPath));
            xDownloadFolderPathVE.Init(null, downloadFolderPath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDownloadFolderPathVE, TextBox.ToolTipProperty, downloadFolderPath, nameof(DriverConfigParam.Description));

            //Browser Height
            DriverConfigParam browserHeight = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserHeight));
            xBrowserHeightVE.Init(null, browserHeight, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserHeightVE, TextBox.ToolTipProperty, browserHeight, nameof(DriverConfigParam.Description));

            //Browser Width
            DriverConfigParam browserWidth = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserWidth));
            xBrowserWidthVE.Init(null, browserWidth, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserWidthVE, TextBox.ToolTipProperty, browserWidth, nameof(DriverConfigParam.Description));

            //Browser Executable Path
            DriverConfigParam browserExecutablePath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserExecutablePath));
            xBrowserExecutablePathVE.Init(null, browserExecutablePath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserExecutablePathVE, TextBox.ToolTipProperty, browserExecutablePath, nameof(DriverConfigParam.Description));

            //Driver File Path
            DriverConfigParam driverFilePath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DriverFilePath));
            xDriverFilePathVE.Init(null, driverFilePath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDriverFilePathVE, TextBox.ToolTipProperty, driverFilePath, nameof(DriverConfigParam.Description));

            //Browser Private Mode
            DriverConfigParam browserPrivateMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserPrivateMode));
            BindingHandler.ObjFieldBinding(xBrowserPrivateModeCB, CheckBox.IsCheckedProperty, browserPrivateMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserPrivateModeCB, CheckBox.ToolTipProperty, browserPrivateMode, nameof(DriverConfigParam.Description));

            //Headless Browser Mode
            DriverConfigParam headlessBrowserMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HeadlessBrowserMode));
            BindingHandler.ObjFieldBinding(xHeadlessBrowserModeCB, CheckBox.IsCheckedProperty, headlessBrowserMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xHeadlessBrowserModeCB, CheckBox.ToolTipProperty, headlessBrowserMode, nameof(DriverConfigParam.Description));

            //Browser Minimized
            DriverConfigParam browserMinimize = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserMinimized));
            BindingHandler.ObjFieldBinding(xBrowserMinimizedCB, CheckBox.IsCheckedProperty, browserMinimize, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserMinimizedCB, CheckBox.ToolTipProperty, browserMinimize, nameof(DriverConfigParam.Description));

            #endregion

            #region Edge/IE Specific
            //use 64Bit browser
            DriverConfigParam use64Bitbrowser = mAgent.GetOrCreateParam(nameof(SeleniumDriver.Use64Bitbrowser));
            BindingHandler.ObjFieldBinding(xUse64BitbrowserCB, CheckBox.IsCheckedProperty, use64Bitbrowser, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUse64BitbrowserCB, CheckBox.ToolTipProperty, use64Bitbrowser, nameof(DriverConfigParam.Description));

            //Ensure Clean Session
            DriverConfigParam ensureCleanSession = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EnsureCleanSession));
            BindingHandler.ObjFieldBinding(xEnsureCleanSessionCB, CheckBox.IsCheckedProperty, ensureCleanSession, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnsureCleanSessionCB, CheckBox.ToolTipProperty, ensureCleanSession, nameof(DriverConfigParam.Description));            

            //ignore IE Protected Mode
            DriverConfigParam ignoreIEProtectedMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.IgnoreIEProtectedMode));
            BindingHandler.ObjFieldBinding(xIgnoreIEProtectedModeCB, CheckBox.IsCheckedProperty, ignoreIEProtectedMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xIgnoreIEProtectedModeCB, CheckBox.ToolTipProperty, ignoreIEProtectedMode, nameof(DriverConfigParam.Description));

            //Open IEMode In Edge
            DriverConfigParam openIEModeInEdge = mAgent.GetOrCreateParam(nameof(SeleniumDriver.OpenIEModeInEdge));
            BindingHandler.ObjFieldBinding(xOpenIEModeInEdgeCB, CheckBox.IsCheckedProperty, openIEModeInEdge, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xOpenIEModeInEdgeCB, CheckBox.ToolTipProperty, openIEModeInEdge, nameof(DriverConfigParam.Description));

            //Edge Executable Path
            DriverConfigParam edgeExcutablePath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EdgeExcutablePath));
            xEdgeExcutablePathVE.Init(null, edgeExcutablePath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEdgeExcutablePathVE, TextBox.ToolTipProperty, edgeExcutablePath, nameof(DriverConfigParam.Description));


            #endregion

            #region AdvancedConfigration

            //Selenium User Argument
            DriverConfigParam seleniumUserArguments = mAgent.GetOrCreateParam(nameof(SeleniumDriver.SeleniumUserArguments));
            xSeleniumUserArgumentsVE.Init(null, seleniumUserArguments, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSeleniumUserArgumentsVE, TextBox.ToolTipProperty, seleniumUserArguments, nameof(DriverConfigParam.Description));

            //Debug Address
            DriverConfigParam debugAddress = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DebugAddress));
            xDebugAddressVE.Init(null, debugAddress, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDebugAddressVE, TextBox.ToolTipProperty, debugAddress, nameof(DriverConfigParam.Description));

            //Start BMP BAT File
            DriverConfigParam startBMPBATFile = mAgent.GetOrCreateParam(nameof(SeleniumDriver.StartBMPBATFile));
            xStartBMPBATFileVE.Init(null, startBMPBATFile, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xStartBMPBATFileVE, TextBox.ToolTipProperty, startBMPBATFile, nameof(DriverConfigParam.Description));

            //Extention Path
            DriverConfigParam extentionPath = mAgent.GetOrCreateParam(nameof(SeleniumDriver.ExtensionPath));
            xExtensionPathVE.Init(null, extentionPath, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xExtensionPathVE, TextBox.ToolTipProperty, extentionPath, nameof(DriverConfigParam.Description));

            //xStart BMP PortVE
            DriverConfigParam startBMPPort = mAgent.GetOrCreateParam(nameof(SeleniumDriver.StartBMPPort));
            xStartBMPPortVE.Init(null, startBMPPort, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xStartBMPPortVE, TextBox.ToolTipProperty, startBMPPort, nameof(DriverConfigParam.Description));

            //Unhandled promt Behavior
            GingerCore.General.FillComboFromEnumType(xUnhandledPromptBehaviorComboBox, typeof(SeleniumDriver.eUnhandledPromptBehavior));
            DriverConfigParam UnhandledPromptBehavior = mAgent.GetOrCreateParam(nameof(SeleniumDriver.UnhandledPromptBehavior));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.TextProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.ToolTipProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Description));

            //browser Log Level
            GingerCore.General.FillComboFromList(xBrowserLogLevelComboBox, new List<string> { "0", "1", "2", "3", "4" });
            DriverConfigParam browserLogLevel = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserLogLevel));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.TextProperty, browserLogLevel, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.ToolTipProperty, browserLogLevel, nameof(DriverConfigParam.Description));

            //Disable Extension
            DriverConfigParam disableExtension = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DisableExtension));
            BindingHandler.ObjFieldBinding(xDisableExtensionCB, CheckBox.IsCheckedProperty, disableExtension, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDisableExtensionCB, CheckBox.ToolTipProperty, disableExtension, nameof(DriverConfigParam.Description));

            //Enable Native Events
            DriverConfigParam enableNativeEvents = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EnableNativeEvents));
            BindingHandler.ObjFieldBinding(xEnableNativeEventsCB, CheckBox.IsCheckedProperty, enableNativeEvents, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnableNativeEventsCB, CheckBox.ToolTipProperty, enableNativeEvents, nameof(DriverConfigParam.Description));

            //Start BMP
            DriverConfigParam startBMP = mAgent.GetOrCreateParam(nameof(SeleniumDriver.StartBMP));
            BindingHandler.ObjFieldBinding(xStartBMPCB, CheckBox.IsCheckedProperty, startBMP, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xStartBMPCB, CheckBox.ToolTipProperty, startBMP, nameof(DriverConfigParam.Description));

            //Hide Console Window
            DriverConfigParam hideConsoleWindow = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HideConsoleWindow));
            BindingHandler.ObjFieldBinding(xHideConsoleWindowCB, CheckBox.IsCheckedProperty, hideConsoleWindow, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xHideConsoleWindowCB, CheckBox.ToolTipProperty, hideConsoleWindow, nameof(DriverConfigParam.Description));

            //Take only Active Screenshot
            DriverConfigParam takeOnlyActiveScreenshot = mAgent.GetOrCreateParam(nameof(SeleniumDriver.TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure));
            BindingHandler.ObjFieldBinding(xFrameWindowScreenShotCB, CheckBox.IsCheckedProperty, takeOnlyActiveScreenshot, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xFrameWindowScreenShotCB, CheckBox.ToolTipProperty, takeOnlyActiveScreenshot, nameof(DriverConfigParam.Description));

            //Handle IFrame Shift auto
            DriverConfigParam handleIFrameShiftAuto = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HandelIFramShiftAutomaticallyForPomElement));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMCB, CheckBox.IsCheckedProperty, handleIFrameShiftAuto, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMCB, CheckBox.ToolTipProperty, handleIFrameShiftAuto, nameof(DriverConfigParam.Description));



            #endregion
        }

       

        /// <summary>
        /// Handles the PropertyChanged event of the BrowserTypeParam object.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event arguments.</param>
        private void BrowserTypeParam_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!string.Equals(e.PropertyName, nameof(DriverConfigParam.Value)))
            {
                return;
            }
            if (sender is not DriverConfigParam driverConfigParam)
            {
                return;
            }
            if (!string.Equals(driverConfigParam.Parameter, nameof(GingerWebDriver.BrowserType)))
            {
                return;
            }

            WebBrowserType browserType = Enum.Parse<WebBrowserType>(driverConfigParam.Value);
            edgeIEPnlVisibility(browserType);
            chromePnlvisibilitly(browserType);
            chromeFirefoxPnlVisibility(browserType);
            chromeFirefoxIEPnl(browserType);
        }


        /// <summary>
        /// Sets the visibility of the Edge/IE panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void edgeIEPnlVisibility(WebBrowserType result)
        {


            if (result == WebBrowserType.Edge || result == WebBrowserType.InternetExplorer)
            {
                xEdgeIE.Visibility = Visibility.Visible;
            }
            else
            {
                xEdgeIE.Visibility = Visibility.Collapsed;
            }

        }

        /// <summary>
        /// Sets the visibility of the Chrome panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void chromePnlvisibilitly(WebBrowserType result)
        {
            if (result == WebBrowserType.Chrome)
            {
                xChromePnl.Visibility = Visibility.Visible;
            }
            else
            {
                xChromePnl.Visibility = Visibility.Collapsed;
            }


        }
        /// <summary>
        /// Sets the visibility of the Chrome/Firefox panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void chromeFirefoxPnlVisibility(WebBrowserType result)
        {
            if (result == WebBrowserType.Chrome || result == WebBrowserType.FireFox)
            {
                xChromeFirefoxPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xChromeFirefoxPnl.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// Sets the visibility of the Chrome/Firefox/IE panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void chromeFirefoxIEPnl(WebBrowserType result)
        {
            if (result == WebBrowserType.Chrome || result == WebBrowserType.FireFox || result == WebBrowserType.InternetExplorer)
            {
                xChromeFirefoxIEPnl.Visibility = Visibility.Visible;
            }
            else
            {
                xChromeFirefoxIEPnl.Visibility = Visibility.Collapsed;
            }
        }
        private void xAutoDetectProxyCB_Checked(object sender, RoutedEventArgs e)
        {
            xProxyPnl.IsEnabled = false;
        }

        private void xAutoDetectProxyCB_Unchecked_1(object sender, RoutedEventArgs e)
        {
            xProxyPnl.IsEnabled = true;
        }


    }
}


