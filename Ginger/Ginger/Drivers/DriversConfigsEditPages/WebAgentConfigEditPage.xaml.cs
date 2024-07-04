
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Drivers;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.GeneralLib;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;
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
        private readonly IEnumerable<DriverConfigParam> _driverConfigParams;
        Context context=null;
        Agent mAgent;

        public WebAgentConfigEditPage(Agent mAgent,Agent.eDriverType driverType, Context context, IEnumerable<DriverConfigParam> driverConfigParams)
        {
            _driverConfigParams = driverConfigParams;
            this.context = context;
            this.mAgent = mAgent;
            InitializeComponent();
          

            bindElement();


        }
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
            GingerCore.General.FillComboFromList(xBrowserPrivateModeComboBox, new List<string> { "true", "false" });
            DriverConfigParam browserPrivateMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserPrivateMode));
            BindingHandler.ObjFieldBinding(xBrowserPrivateModeComboBox, ComboBox.TextProperty, browserPrivateMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserPrivateModeComboBox, ComboBox.ToolTipProperty, browserPrivateMode, nameof(DriverConfigParam.Description));

            //Headless Browser Mode
            GingerCore.General.FillComboFromList(xHeadlessBrowserModeComboBox, new List<string> { "true", "false" });
            DriverConfigParam headlessBrowserMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HeadlessBrowserMode));
            BindingHandler.ObjFieldBinding(xHeadlessBrowserModeComboBox, ComboBox.TextProperty, headlessBrowserMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xHeadlessBrowserModeComboBox, ComboBox.ToolTipProperty, headlessBrowserMode, nameof(DriverConfigParam.Description));

            //Browser Minimized
            GingerCore.General.FillComboFromList(xBrowserMinimizedComboBox, new List<string> { "true", "false" });
            DriverConfigParam browserMinimized = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserMinimized));
            BindingHandler.ObjFieldBinding(xBrowserMinimizedComboBox, ComboBox.TextProperty, browserMinimized, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserMinimizedComboBox, ComboBox.ToolTipProperty, browserMinimized, nameof(DriverConfigParam.Description));

            #endregion

            #region Edge/IE Specific
            //use 64Bit browser
            DriverConfigParam use64Bitbrowser = mAgent.GetOrCreateParam(nameof(SeleniumDriver.Use64Bitbrowser));
            BindingHandler.ObjFieldBinding(xUse64BitbrowserCB, CheckBox.IsCheckedProperty, use64Bitbrowser, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUse64BitbrowserCB, CheckBox.ToolTipProperty, use64Bitbrowser, nameof(DriverConfigParam.Description));

            //Ensure Clean Session
            GingerCore.General.FillComboFromList(xEnsureCleanSessionComboBox, new List<string> { "true", "false" });
            DriverConfigParam ensureCleanSession = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EnsureCleanSession));
            BindingHandler.ObjFieldBinding(xEnsureCleanSessionComboBox, ComboBox.TextProperty, ensureCleanSession, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnsureCleanSessionComboBox, ComboBox.ToolTipProperty, ensureCleanSession, nameof(DriverConfigParam.Description));

            //ignore IE Protected Mode
            DriverConfigParam ignoreIEProtectedMode = mAgent.GetOrCreateParam(nameof(SeleniumDriver.IgnoreIEProtectedMode));
            BindingHandler.ObjFieldBinding(xIgnoreIEProtectedModeCB, CheckBox.IsCheckedProperty, ignoreIEProtectedMode, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xIgnoreIEProtectedModeCB, CheckBox.ToolTipProperty, ignoreIEProtectedMode, nameof(DriverConfigParam.Description));

            //Open IEMode In Edge
            GingerCore.General.FillComboFromList(xOpenIEModeInEdgeComboBox, new List<string> { "true", "false" });
            DriverConfigParam openIEModeInEdge = mAgent.GetOrCreateParam(nameof(SeleniumDriver.OpenIEModeInEdge));
            BindingHandler.ObjFieldBinding(xOpenIEModeInEdgeComboBox, ComboBox.TextProperty, openIEModeInEdge, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xOpenIEModeInEdgeComboBox, ComboBox.ToolTipProperty, openIEModeInEdge, nameof(DriverConfigParam.Description));

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

            //TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure
            GingerCore.General.FillComboFromList(xFrameWindowScreenShotComboBox, new List<string> { "true", "false" });
            DriverConfigParam frameWindowScreenShot = mAgent.GetOrCreateParam(nameof(SeleniumDriver.TakeOnlyActiveFrameOrWindowScreenShotInCaseOfFailure));
            BindingHandler.ObjFieldBinding(xFrameWindowScreenShotComboBox, ComboBox.TextProperty, frameWindowScreenShot, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xFrameWindowScreenShotComboBox, ComboBox.ToolTipProperty, frameWindowScreenShot, nameof(DriverConfigParam.Description));

            //HandelIFramShiftAutomaticallyForPomElement
            GingerCore.General.FillComboFromList(xAutoFrameShiftForPOMComboBox, new List<string> { "true", "false" });
            DriverConfigParam autoFrameShiftForPOM = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HandelIFramShiftAutomaticallyForPomElement));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMComboBox, ComboBox.TextProperty, autoFrameShiftForPOM, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xAutoFrameShiftForPOMComboBox, ComboBox.ToolTipProperty, autoFrameShiftForPOM, nameof(DriverConfigParam.Description));

            //Disable Extention
            GingerCore.General.FillComboFromList(xDisableExtensionComboBox, new List<string> { "true", "false" });
            DriverConfigParam disableExtention = mAgent.GetOrCreateParam(nameof(SeleniumDriver.DisableExtension));
            BindingHandler.ObjFieldBinding(xDisableExtensionComboBox, ComboBox.TextProperty, disableExtention, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xDisableExtensionComboBox, ComboBox.ToolTipProperty, disableExtention, nameof(DriverConfigParam.Description));

            //Enable Native Events
            GingerCore.General.FillComboFromList(xEnableNativeEventsComboBox, new List<string> { "true", "false" });
            DriverConfigParam enableNativeEvents = mAgent.GetOrCreateParam(nameof(SeleniumDriver.EnableNativeEvents));
            BindingHandler.ObjFieldBinding(xEnableNativeEventsComboBox, ComboBox.TextProperty, enableNativeEvents, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xEnableNativeEventsComboBox, ComboBox.ToolTipProperty, enableNativeEvents, nameof(DriverConfigParam.Description));

            //Start BMP
            GingerCore.General.FillComboFromList(xStartBMPComboBox, new List<string> { "true", "false" });
            DriverConfigParam startBMP = mAgent.GetOrCreateParam(nameof(SeleniumDriver.StartBMP));
            BindingHandler.ObjFieldBinding(xStartBMPComboBox, ComboBox.TextProperty, startBMP, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xStartBMPComboBox, ComboBox.ToolTipProperty, startBMP, nameof(DriverConfigParam.Description));

            //Hide Console Window
            GingerCore.General.FillComboFromList(xHideConsoleWindowComboBox, new List<string> { "true", "false" });
            DriverConfigParam hideConsoleWindow = mAgent.GetOrCreateParam(nameof(SeleniumDriver.HideConsoleWindow));
            BindingHandler.ObjFieldBinding(xHideConsoleWindowComboBox, ComboBox.TextProperty, hideConsoleWindow, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xHideConsoleWindowComboBox, ComboBox.ToolTipProperty, hideConsoleWindow, nameof(DriverConfigParam.Description));

            //Unhandled promt Behavior
            GingerCore.General.FillComboFromEnumType(xUnhandledPromptBehaviorComboBox, typeof(SeleniumDriver.eUnhandledPromptBehavior));
            DriverConfigParam UnhandledPromptBehavior = mAgent.GetOrCreateParam(nameof(SeleniumDriver.UnhandledPromptBehavior));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.TextProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xUnhandledPromptBehaviorComboBox, ComboBox.ToolTipProperty, UnhandledPromptBehavior, nameof(DriverConfigParam.Description));

            //browser Log Level
            GingerCore.General.FillComboFromEnumType(xBrowserLogLevelComboBox, typeof(SeleniumDriver.eBrowserLogLevel));
            DriverConfigParam browserLogLevel = mAgent.GetOrCreateParam(nameof(SeleniumDriver.BrowserLogLevel));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.TextProperty, browserLogLevel, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xBrowserLogLevelComboBox, ComboBox.ToolTipProperty, browserLogLevel, nameof(DriverConfigParam.Description));
            #endregion
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
