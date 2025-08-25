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


using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright;
using GingerCore;
using GingerCore.Drivers;
using GingerCore.GeneralLib;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for WebAgentConfigEditPage.xaml
    /// </summary>
    public partial class WebAgentConfigEditPage : Page
    {

        Agent mAgent;


        /// <summary>
        /// Initializes a new instance of the WebAgentConfigEditPage class.
        /// </summary>
        /// <param name="mAgent">The Agent object.</param>
        public WebAgentConfigEditPage(Agent mAgent)
        {
            this.mAgent = mAgent;
            InitializeComponent();

            BindElement();

            DriverConfigParam? browserTypeParam = mAgent.DriverConfiguration.FirstOrDefault(p => string.Equals(p.Parameter, nameof(GingerWebDriver.BrowserType)));
            if (browserTypeParam != null)
            {
                browserTypeParam.PropertyChanged += BrowserTypeParam_PropertyChanged;
            }
            WebBrowserType browserType = Enum.Parse<WebBrowserType>(browserTypeParam?.Value);
            EdgeIEPnlVisibility(browserType);
            ChromePnlvisibilitly(browserType);
            ChromeFirefoxPnlVisibility(browserType);
            AllBrowserNotBravePnl(browserType);
            ProxyPnlVisbility();
            PlaywightDriverParamVisibility();
        }

        private void RecordVideoDirTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ReplaceSolutionDirPathInVideoDirPath();
        }
        private void ReplaceSolutionDirPathInVideoDirPath()
        {
            if (xRecordVideoDirVE.ValueTextBox.Text.StartsWith(@"~\"))
            {
                return;
            }

            if (string.IsNullOrEmpty(xRecordVideoDirVE.ValueTextBox.Text))
            {
                xRecordVideoDirVE.ValueTextBox.Text = @"~\\ExecutionResults\VideoRecordings";
                return;
            }

            string solutionFolder = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.Solution.Folder;
            string directoryPath = xRecordVideoDirVE.ValueTextBox.Text;
            if (directoryPath.Contains(solutionFolder))
            {
                directoryPath = directoryPath.Replace(solutionFolder, @"~\", StringComparison.InvariantCultureIgnoreCase);
            }

            xRecordVideoDirVE.ValueTextBox.Text = directoryPath;
        }

        private void RecordVideoDirTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textChange = e.Changes.FirstOrDefault();

            if (textChange != null && (textChange.AddedLength > 1 || textChange.RemovedLength > 1))
            {
                ReplaceSolutionDirPathInVideoDirPath();
            }
        }

        /// <summary>
        /// Binds the elements of the page.
        /// </summary>
        void BindElement()
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
            BindingHandler.ObjFieldBinding(xPageLoadStrategyComboBox, ComboBox.SelectedValueProperty, pageLoadStrategy, nameof(DriverConfigParam.Value));
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



            #endregion

            #region AdvancedConfigration

            //Selenium User Argument
            DriverConfigParam seleniumUserArguments = mAgent.GetOrCreateParam(nameof(SeleniumDriver.SeleniumUserArguments));
            xSeleniumUserArgumentsVE.Init(null, seleniumUserArguments, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xSeleniumUserArgumentsVE, TextBox.ToolTipProperty, seleniumUserArguments, nameof(DriverConfigParam.Description));

            //Remote Web Driver Url
            DriverConfigParam RemoteWebDriverUrl = mAgent.GetOrCreateParam(nameof(SeleniumDriver.RemoteWebDriverUrl));
            xRemoteWebDriverUrlVE.Init(null, RemoteWebDriverUrl, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xRemoteWebDriverUrlVE, TextBox.ToolTipProperty, RemoteWebDriverUrl, nameof(DriverConfigParam.Description));

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
            GingerCore.General.FillComboFromEnumObj(xUnhandledPromptBehaviorComboBox, new SeleniumDriver().UnhandledPromptBehavior1);
            DriverConfigParam UnhandledPromptBehavior = mAgent.GetOrCreateParam(nameof(SeleniumDriver.UnhandledPromptBehavior));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.SelectedValueProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.ToolTipProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Description));

            //browser Log Level
            GingerCore.General.FillComboFromEnumType(xBrowserLogLevelComboBox, typeof(SeleniumDriver.eBrowserLogLevel));
            DriverConfigParam browserLogLevel = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserLogLevel));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.SelectedValueProperty, browserLogLevel, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.ToolTipProperty, browserLogLevel, nameof(DriverConfigParam.Description));

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


            //Start network monitoring log
            DriverConfigParam networkMonitoring = mAgent.GetOrCreateParam(nameof(SeleniumDriver.StartNetworkMonitoring));
            BindingHandler.ObjFieldBinding(xNetworklogCB, CheckBox.IsCheckedProperty, networkMonitoring, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xNetworklogCB, CheckBox.ToolTipProperty, networkMonitoring, nameof(DriverConfigParam.Description));

            //Handle IFrame Shift auto
            DriverConfigParam handleIFrameShiftAuto = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HandelIFramShiftAutomaticallyForPomElement));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMCB, CheckBox.IsCheckedProperty, handleIFrameShiftAuto, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMCB, CheckBox.ToolTipProperty, handleIFrameShiftAuto, nameof(DriverConfigParam.Description));

            #region VideoRecordingConfigurations

            DriverConfigParam enableVideoRecording = mAgent.GetOrCreateParam(nameof(PlaywrightDriver.EnableVideoRecording), "false");
            BindingHandler.ObjFieldBinding(xEnableVideoRecordingCheckBox, CheckBox.IsCheckedProperty, enableVideoRecording, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnableVideoRecordingCheckBox, CheckBox.ToolTipProperty, enableVideoRecording, nameof(DriverConfigParam.Description));

            DriverConfigParam recordVideoDir = mAgent.GetOrCreateParam(nameof(PlaywrightDriver.RecordVideoDir));
            xRecordVideoDirVE.Init(null, recordVideoDir, nameof(DriverConfigParam.Value), true, true, Ginger.BusinessFlowWindows.UCValueExpression.eBrowserType.Folder, "*.*", null);
            BindingHandler.ObjFieldBinding(xRecordVideoDirVE, TextBox.ToolTipProperty, recordVideoDir, nameof(DriverConfigParam.Description));

            DriverConfigParam vidoeHeight = mAgent.GetOrCreateParam(nameof(PlaywrightDriver.VideoHeight));
            xVideoHeight.Init(null, vidoeHeight, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xVideoHeight, TextBox.ToolTipProperty, vidoeHeight, nameof(DriverConfigParam.Description));

            DriverConfigParam videoWidth = mAgent.GetOrCreateParam(nameof(PlaywrightDriver.VideoWidth));
            xVideoWidth.Init(null, videoWidth, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xVideoWidth, TextBox.ToolTipProperty, videoWidth, nameof(DriverConfigParam.Description));
            #endregion

            #endregion


            #region ZAP Config
            DriverConfigParam useZAP = mAgent.GetOrCreateParam(nameof(SeleniumDriver.UseZAP));
            BindingHandler.ObjFieldBinding(xEnableZAPCheckBox, CheckBox.IsCheckedProperty, useZAP, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnableZAPCheckBox, CheckBox.ToolTipProperty, useZAP, nameof(DriverConfigParam.Description));
            #endregion
            if (!string.IsNullOrEmpty(proxyName.Value))
            {
                xAutoDetectProxyCB.IsChecked = false;
            }
        }

        /// <summary>
        /// Sets the visibility of the Playwright driver parameters based on the specified driver type.
        /// </summary>
        void PlaywightDriverParamVisibility()
        {
            if (mAgent.DriverType == Agent.eDriverType.Playwright)
            {
                // Proxy Settings
                xAutoDetectProxyCB.Visibility = Visibility.Collapsed;
                xProxyPanel.Visibility = Visibility.Visible;
                xByPassProxyPanel.Visibility = Visibility.Visible;
                xProxyAutoConfigUrlPanel.Visibility = Visibility.Collapsed;

                // Browser Configuration
                xBrowserMinimizedCB.Visibility = Visibility.Collapsed;
                xHeadlessBrowserModePanel.Visibility = Visibility.Visible;
                xBrowserPrivateModePanel.Visibility = Visibility.Visible;
                xBrowserHeightPanel.Visibility = Visibility.Collapsed;
                xBrowserWidthPanel.Visibility = Visibility.Collapsed;
                xBrowserExecutablePathPanel.Visibility = Visibility.Collapsed;
                xDriverFilePathPanel.Visibility = Visibility.Collapsed;
                xUserProfileFolderPathPanel.Visibility = Visibility.Collapsed;
                xIgnoreIEProtectedModePanel.Visibility = Visibility.Collapsed;
                xEnsureCleanSessionPanel.Visibility = Visibility.Collapsed;
                xEnableNativeEventsPanel.Visibility = Visibility.Collapsed;
                xEmulationDeviceNamePanel.Visibility = Visibility.Collapsed;
                xDownloadFolderPathPanel.Visibility = Visibility.Collapsed;
                xChromeFirefoxPnl.Visibility = Visibility.Collapsed;
                xAllBrowserNotBravePnl.Visibility = Visibility.Collapsed;
                xBrowserSpecificConfiguration.Visibility = Visibility.Collapsed;

                // Session Management
                xPageLoadStrategyComboBoxPanel.Visibility = Visibility.Collapsed;
                xImplicitWaitPanel.Visibility = Visibility.Collapsed;
                xHttpServerTimeOutPanel.Visibility = Visibility.Collapsed;
                xPageLoadTimeOutPanel.Visibility = Visibility.Collapsed;
                xDriverLoadWaitingTimePanel.Visibility = Visibility.Visible;

                // Advanced Settings
                xAdvanceSetting.Visibility = Visibility.Visible;
                xSeleniumUserArgumentsPanel.Visibility = Visibility.Collapsed;
                xRemoteWebDriverUrlPanel.Visibility = Visibility.Collapsed;
                xStartBMPCBPanel.Visibility = Visibility.Collapsed;
                xHideConsoleWindowPanel.Visibility = Visibility.Collapsed;
                xDebugAddressPanel.Visibility = Visibility.Collapsed;
                xStartBMPBATFilePanel.Visibility = Visibility.Collapsed;
                xExtensionPathPanel.Visibility = Visibility.Collapsed;
                xStartBMPPortPanel.Visibility = Visibility.Collapsed;
                xUnhandledPromptBehaviorComboBoxPanel.Visibility = Visibility.Collapsed;
                xBrowserLogLevelComboBoxPanel.Visibility = Visibility.Collapsed;
                xVideoRecordingPnl.Visibility = Visibility.Visible;

                xRecordVideoDirVE.ValueTextBox.TextChanged += RecordVideoDirTextBox_TextChanged;
                xRecordVideoDirVE.ValueTextBox.LostFocus += RecordVideoDirTextBox_LostFocus;
                xVideoRecordingControlsPnl.IsEnabled = (bool)xEnableVideoRecordingCheckBox.IsChecked;
            }
            else
            {
                xVideoRecordingPnl.Visibility = Visibility.Collapsed;
            }
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
            EdgeIEPnlVisibility(browserType);
            ChromePnlvisibilitly(browserType);
            ChromeFirefoxPnlVisibility(browserType);
            AllBrowserNotBravePnl(browserType);
        }

        /// <summary>
        /// Sets the visibility of the Edge/IE panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void AllBrowserNotBravePnl(WebBrowserType result)
        {


            if (result is WebBrowserType.Brave or WebBrowserType.InternetExplorer)
            {
                xAllBrowserNotBravePnl.Visibility = Visibility.Collapsed;
            }
            else
            {
                xAllBrowserNotBravePnl.Visibility = Visibility.Visible;
            }

        }


        /// <summary>
        /// Sets the visibility of the Edge/IE panel based on the specified browser type.
        /// </summary>
        /// <param name="result">The browser type.</param>
        void EdgeIEPnlVisibility(WebBrowserType result)
        {


            if (result is WebBrowserType.Edge or WebBrowserType.InternetExplorer)
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
        void ChromePnlvisibilitly(WebBrowserType result)
        {
            if (result is WebBrowserType.Chrome or WebBrowserType.Brave)
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
        void ChromeFirefoxPnlVisibility(WebBrowserType result)
        {
            if (result is WebBrowserType.Chrome or WebBrowserType.FireFox or WebBrowserType.Brave)
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

        void ProxyPnlVisbility()
        {
            if (xAutoDetectProxyCB.IsChecked == false)
            {
                xProxyPnl.IsEnabled = true;
            }
            else
            {
                xProxyPnl.IsEnabled = false;

            }
        }
        private void xAutoDetectProxyCB_Click(object sender, RoutedEventArgs e)
        {
            ProxyPnlVisbility();
            xProxyVE.ValueTextBox.Text = "";
        }

        private void xEnableVideoRecordingCheckBox_Click(object sender, RoutedEventArgs e)
        {
            xVideoRecordingControlsPnl.IsEnabled = (bool)xEnableVideoRecordingCheckBox.IsChecked;
        }
    }
}


