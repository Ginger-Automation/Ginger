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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Ginger.UserControls;
using Ginger.ValidationRules;
using GingerCore;
using GingerCore.GeneralLib;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for AppiumDriverEditPage.xaml
    /// </summary>
    public partial class AppiumDriverEditPage : Page
    {
        Agent mAgent = null;
        DriverConfigParam mAppiumServer;
        DriverConfigParam mDevicePlatformType;
        DriverConfigParam mAppType;
        DriverConfigParam mAppiumCapabilities;
        DriverConfigParam mDeviceAutoScreenshotRefreshMode;
        DriverConfigParam mDeviceSource;


        public AppiumDriverEditPage(Agent appiumAgent)
        {
            InitializeComponent();

            mAgent = appiumAgent;
            BindConfigurationsFields();
        }

        private void BindConfigurationsFields()
        {
            mAppiumServer = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumServer), @"http://127.0.0.1:4723/");
            //BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.TextProperty, mAppiumServer, nameof(DriverConfigParam.Value));
            xServerURLTextBox.Init(null, mAppiumServer, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.ToolTipProperty, mAppiumServer, nameof(DriverConfigParam.Description));

            BindingHandler.ObjFieldBinding(xLoadDeviceWindow, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.LoadDeviceWindow), "true"), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());

            DriverConfigParam proxy = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.Proxy));
            xProxyTextBox.Init(null, proxy, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xProxyTextBox, TextBox.ToolTipProperty, proxy, nameof(DriverConfigParam.Description));
            xUseProxyChkBox.IsChecked = !string.IsNullOrEmpty(proxy.Value);

            //DriverConfigParam screenScaleFactorCorrectionX = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenScaleFactorCorrectionX));
            //xScreenScaleFactorCorrectionXTextBox.Init(null, screenScaleFactorCorrectionX, nameof(DriverConfigParam.Value));
            //BindingHandler.ObjFieldBinding(xScreenScaleFactorCorrectionXTextBox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenScaleFactorCorrectionX)), nameof(DriverConfigParam.Description));

            //DriverConfigParam screenScaleFactorCorrectionY = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenScaleFactorCorrectionY));
            //xScreenScaleFactorCorrectionYTextBox.Init(null, screenScaleFactorCorrectionY, nameof(DriverConfigParam.Value));
            //BindingHandler.ObjFieldBinding(xScreenScaleFactorCorrectionYTextBox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenScaleFactorCorrectionY)), nameof(DriverConfigParam.Description));

            DriverConfigParam screenshotHeight = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenshotHeight));
            xScreenshotHeightTextBox.Init(null, screenshotHeight, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xScreenshotHeightTextBox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenshotHeight)), nameof(DriverConfigParam.Description));

            DriverConfigParam screenshotWidth = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenshotWidth));
            xScreenshotWidthTextBox.Init(null, screenshotWidth, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xScreenshotWidthTextBox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.ScreenshotWidth)), nameof(DriverConfigParam.Description));

            xLoadTimeoutTxtbox.Init(null, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DriverLoadWaitingTime)), nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xLoadTimeoutTxtbox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DriverLoadWaitingTime)), nameof(DriverConfigParam.Description));

            BindingHandler.ObjFieldBinding(xAutoUpdateCapabiltiies, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AutoSetCapabilities), "true"), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());
            BindingHandler.ObjFieldBinding(xUFTMSupportSimulations, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.UFTMSupportSimulationsCapabiliy)), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());

            mAppiumCapabilities = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumCapabilities));

            BindRadioButtons();

            ApplyValidationRules();

            if (mAppiumCapabilities.MultiValues == null || mAppiumCapabilities.MultiValues.Count == 0)
            {
                mAppiumCapabilities.MultiValues = [];
                AutoSetCapabilities(true);
            }
            SetCapabilitiesGridView();
        }

        private void ApplyValidationRules()
        {
            xServerURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("URL cannot be empty"));
            xLoadTimeoutTxtbox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Load Timeout cannot be empty"));
            xScreenshotHeightTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Height cannot be empty"));
            xScreenshotWidthTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Width cannot be empty"));

            CallConfigPropertyChange();
        }

        private void CallConfigPropertyChange()
        {
            // need in order to trigger the validation's rules 
            mAgent.OnPropertyChanged(nameof(GenericAppiumDriver.AppiumServer));
            mAgent.OnPropertyChanged(nameof(GenericAppiumDriver.LoadDeviceWindow));
            mAgent.OnPropertyChanged(nameof(GenericAppiumDriver.ScreenshotHeight));
            mAgent.OnPropertyChanged(nameof(GenericAppiumDriver.ScreenshotWidth));
        }

        private void BindRadioButtons()
        {
            mDeviceAutoScreenshotRefreshMode = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DeviceAutoScreenshotRefreshMode), nameof(eAutoScreenshotRefreshMode.Live));
            BindingHandler.ObjFieldBinding(xLiveRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eAutoScreenshotRefreshMode.Live));
            BindingHandler.ObjFieldBinding(xPostOperationRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eAutoScreenshotRefreshMode.PostOperation));
            BindingHandler.ObjFieldBinding(xDisabledRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eAutoScreenshotRefreshMode.Disabled));

            mDevicePlatformType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DevicePlatformType));
            BindingHandler.ObjFieldBinding(xAndroidRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eDevicePlatformType.Android));
            BindingHandler.ObjFieldBinding(xIOSRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eDevicePlatformType.iOS));

            if (IsUFTCapabilityExist())
            {
                mDeviceSource = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DeviceSource), nameof(eDeviceSource.MicroFoucsUFTMLab));
            }
            else
            {
                mDeviceSource = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DeviceSource));
            }
            BindingHandler.ObjFieldBinding(xLocalAppiumRdBtn, RadioButton.IsCheckedProperty, mDeviceSource, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eDeviceSource.LocalAppium));
            BindingHandler.ObjFieldBinding(xUFTRdBtn, RadioButton.IsCheckedProperty, mDeviceSource, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eDeviceSource.MicroFoucsUFTMLab));
            BindingHandler.ObjFieldBinding(xKobitonRdBtn, RadioButton.IsCheckedProperty, mDeviceSource, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eDeviceSource.Kobiton));

            mAppType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppType));
            BindingHandler.ObjFieldBinding(xNativeHybRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eAppType.NativeHybride));
            BindingHandler.ObjFieldBinding(xWebRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: nameof(eAppType.Web));
        }

        private void SetCapabilitiesGridView()
        {
            xCapabilitiesGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName)
            {
                GridColsView =
            [
                new GridColView() { Field = DriverConfigParam.Fields.Parameter, Header = "Capability", WidthWeight = 20 },
                new GridColView() { Field = DriverConfigParam.Fields.Value, WidthWeight = 30 },
                new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ParamValueExpressionButton"] },
                new GridColView() { Field = DriverConfigParam.Fields.Description, BindingMode = BindingMode.OneWay, WidthWeight = 45 },
            ]
            };

            xCapabilitiesGrid.SetAllColumnsDefaultView(view);
            xCapabilitiesGrid.InitViewItems();

            xCapabilitiesGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddCapability));
            xCapabilitiesGrid.AddToolbarTool(eImageType.Reset, "Reset Capabilities", new RoutedEventHandler(ResetCapabilities));
            xCapabilitiesGrid.ShowRefresh = Visibility.Collapsed;
            xCapabilitiesGrid.DataSourceList = mAppiumCapabilities.MultiValues;
        }

        private void SetPlatformCapabilities()
        {
            DriverConfigParam platformName = new DriverConfigParam() { Parameter = "platformName", Description = "Which mobile OS platform to use" };
            DriverConfigParam automationName = new DriverConfigParam() { Parameter = "appium:automationName", Description = "Which automation engine to use" };
            if (mDevicePlatformType.Value == nameof(eDevicePlatformType.Android))
            {
                platformName.Value = "Android";
                automationName.Value = "UiAutomator2";
            }
            else
            {
                platformName.Value = "iOS";
                automationName.Value = "XCUITest";
            }
            AddOrUpdateCapability(platformName);
            AddOrUpdateCapability(automationName);
        }

        private void SetDeviceSourceCapabilities()
        {
            if (mDeviceSource.Value == nameof(eDeviceSource.LocalAppium))
            {
                DeleteUFTMServerCapabilities();
                DeleteUFTMSupportSimulationsCapabilities();
                DeleteKobitonServerCapabilities();

            }
            else if (mDeviceSource.Value == nameof(eDeviceSource.MicroFoucsUFTMLab) && !IsUFTCapabilityExist())
            {
                DriverConfigParam uftAppiumVersion = new DriverConfigParam() { Parameter = "uftm:appiumVersion", Value = "v2.x", Description = "Appium server version to use in UFT '1.x' or '2.x'" };
                DriverConfigParam uftClientId = new DriverConfigParam() { Parameter = "uftm:oauthClientId", Description = "UFT Execution key Client Id" };
                DriverConfigParam uftClientSecret = new DriverConfigParam() { Parameter = "uftm:oauthClientSecret", Description = "UFT Execution key Client Password" };
                DriverConfigParam uftTenantId = new DriverConfigParam() { Parameter = "uftm:tenantId", Value = "\"999999999\"", Description = "Default value (Need to change only when using UFT shared spaces))" };

                DeleteKobitonServerCapabilities();

                AddOrUpdateCapability(uftAppiumVersion);
                AddOrUpdateCapability(uftClientId);
                AddOrUpdateCapability(uftClientSecret);
                AddOrUpdateCapability(uftTenantId);

                if (xUFTMSupportSimulations.IsChecked == true)
                {
                    SetUFTMSupportSimulationsCapabilities();
                }
            }
            else if (mDeviceSource.Value == nameof(eDeviceSource.Kobiton) && !IsKobitonCapabilityExist())
            {
                DriverConfigParam kobitonUserName = new DriverConfigParam() { Parameter = "kobiton:username", Description = "Kobiton account User Name" };
                DriverConfigParam kobitonAccessKey = new DriverConfigParam() { Parameter = "kobiton:accessKey", Description = "Kobiton account Access Key" };
                DriverConfigParam KobitonSessionName = new DriverConfigParam() { Parameter = "kobiton:sessionName", Value = "Mobile testing via Ginger by Amdocs", Description = "Testing session name which will appear on kobiton" };
                DriverConfigParam kobitonDeviceGroup = new DriverConfigParam() { Parameter = "kobiton:deviceGroup", Value = "KOBITON", Description = "The device group within the Kobiton test session metadata" };

                DeleteUFTMServerCapabilities();
                DeleteUFTMSupportSimulationsCapabilities();

                AddOrUpdateCapability(kobitonUserName);
                AddOrUpdateCapability(kobitonAccessKey);
                AddOrUpdateCapability(KobitonSessionName);
                AddOrUpdateCapability(kobitonDeviceGroup);
            }
        }
        private void SetUFTMSupportSimulationsCapabilities()
        {
            DriverConfigParam installPackagedApp = new DriverConfigParam() { Parameter = "uftm:installPackagedApp", Value = "true", Description = "Install app that supports UFT simulations" };

            AddOrUpdateCapability(installPackagedApp);
        }

        private void SetApplicationCapabilities(bool init = false)
        {
            DriverConfigParam appPackage = new DriverConfigParam() { Parameter = "appium:appPackage", Description = "Java package of the Android application you want to run", Value = "com.android.settings" };
            DriverConfigParam appActivity = new DriverConfigParam() { Parameter = "appium:appActivity", Description = "Activity name for the Android activity you want to launch from your package", Value = "com.android.settings.Settings" };
            DriverConfigParam bundleId = new DriverConfigParam() { Parameter = "appium:bundleId", Description = "Bundle ID of the application under test", Value = "com.apple.Preferences" };
            DriverConfigParam browserName = new DriverConfigParam() { Parameter = "browserName", Description = "Name of mobile web browser to automate" };
            DriverConfigParam defualtURL = new DriverConfigParam() { Parameter = "ginger:defaultURL", Description = "Ginger Capability | Default URL to load on browser connection", Value = "https://ginger.amdocs.com/" };
            if (mAppType.Value == nameof(eAppType.NativeHybride))
            {
                if (mDevicePlatformType.Value == nameof(eDevicePlatformType.Android))
                {
                    if (!init)
                    {
                        SetCurrentCapabilityValue(appPackage);
                        SetCurrentCapabilityValue(appActivity);
                    }
                    AddOrUpdateCapability(appPackage);
                    AddOrUpdateCapability(appActivity);
                    DeleteCapabilityIfExist(bundleId.Parameter);
                }
                else
                {
                    if (!init)
                    {
                        SetCurrentCapabilityValue(bundleId);
                    }
                    AddOrUpdateCapability(bundleId);
                    DeleteCapabilityIfExist(appPackage.Parameter);
                    DeleteCapabilityIfExist(appActivity.Parameter);
                }
                DeleteCapabilityIfExist(browserName.Parameter);
                DeleteCapabilityIfExist(defualtURL.Parameter);
            }
            else
            {
                if (mDevicePlatformType.Value == nameof(eDevicePlatformType.Android))
                {
                    browserName.Value = "Chrome";
                }
                else
                {
                    browserName.Value = "Safari";
                }
                AddOrUpdateCapability(browserName);
                AddOrUpdateCapability(defualtURL);
                DeleteCapabilityIfExist(bundleId.Parameter);
                DeleteCapabilityIfExist(appPackage.Parameter);
                DeleteCapabilityIfExist(appActivity.Parameter);
            }
        }

        private void SetDeviceCapabilities(bool init = false)
        {
            DriverConfigParam deviceName = new DriverConfigParam() { Parameter = "appium:deviceName", Value = string.Empty };
            DriverConfigParam udid = new DriverConfigParam() { Parameter = "appium:udid", Description = "Unique device identifier of the connected physical device", Value = string.Empty };
            if (mDevicePlatformType.Value == nameof(eDevicePlatformType.Android))
            {
                deviceName.Description = "The kind of mobile device to use, for example 'Galaxy S21'";
            }
            else
            {
                deviceName.Description = "The kind of mobile device to use, for example 'iPhone 12'";
            }
            if (!init)
            {
                SetCurrentCapabilityValue(deviceName);
                SetCurrentCapabilityValue(udid);
            }
            AddOrUpdateCapability(deviceName);
            AddOrUpdateCapability(udid);
        }

        private void SetOtherCapabilities(bool init = false)
        {
            DriverConfigParam newCommandTimeout = new DriverConfigParam() { Parameter = "appium:newCommandTimeout", Description = "How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session", Value = "300" };
            DriverConfigParam noReset = new DriverConfigParam() { Parameter = "appium:noReset", Description = "If true, instruct an Appium driver to avoid its usual reset logic during session start and cleanup", Value = "false" };
            if (!init)
            {
                SetCurrentCapabilityValue(newCommandTimeout);
                SetCurrentCapabilityValue(noReset);
            }
            AddOrUpdateCapability(newCommandTimeout);
            if (mAppType.Value == nameof(eAppType.NativeHybride))
            {
                AddOrUpdateCapability(noReset);
            }
        }

        private void AddOrUpdateCapability(DriverConfigParam capability)
        {
            DriverConfigParam existingCap = FindExistingCapability(capability.Parameter);
            if (existingCap != null)
            {
                existingCap.Parameter = capability.Parameter;
                existingCap.Value = capability.Value;
                existingCap.Description = capability.Description;
            }
            else
            {
                mAppiumCapabilities.MultiValues.Add(capability);
            }
        }

        private bool IsUFTCapabilityExist()
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues?.FirstOrDefault(x => x.Parameter == "uftm:oauthClientSecret");
            if (existingCap != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool IsKobitonCapabilityExist()
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues?.FirstOrDefault(x => x.Parameter == "username" && x.Description == "Kobiton account User Name");
            if (existingCap != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetCurrentCapabilityValue(DriverConfigParam capability)
        {
            DriverConfigParam existingCap = FindExistingCapability(capability.Parameter);
            if (existingCap != null && string.IsNullOrEmpty(existingCap.Value) == false)
            {
                capability.Value = existingCap.Value;
            }
        }

        private void DeleteCapabilityIfExist(string capabilityName)
        {
            DriverConfigParam existingCap = FindExistingCapability(capabilityName);
            if (existingCap != null)
            {
                mAppiumCapabilities.MultiValues.Remove(existingCap);
            }
        }

        private DriverConfigParam FindExistingCapability(string capabilityName)
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues.FirstOrDefault(x => x.Parameter == capabilityName);
            if (existingCap == null)
            {
                existingCap = mAppiumCapabilities.MultiValues.FirstOrDefault(x => x.Parameter == capabilityName.Replace("appium:", string.Empty));
            }
            return existingCap;
        }

        private void AddCapability(object sender, RoutedEventArgs e)
        {
            mAppiumCapabilities.MultiValues.Add(new DriverConfigParam());
        }

        private void AutoSetCapabilities(bool init = false)
        {
            if (init)
            {
                mAppiumCapabilities.MultiValues.Clear();
            }
            SetPlatformCapabilities();
            SetDeviceCapabilities(init);
            SetApplicationCapabilities(init);
            SetOtherCapabilities(init);
        }

        private void ResetCapabilities(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.StaticQuestionsMessage, "Capabilities list will be reset to default values, are you sure?") == eUserMsgSelection.Yes)
            {
                AutoSetCapabilities(true);
            }
        }

        private void CapabilitiesGridVEButton_Click(object sender, RoutedEventArgs e)
        {
            DriverConfigParam capability = (DriverConfigParam)xCapabilitiesGrid.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(capability, DriverConfigParam.Fields.Value, new Context());
            VEEW.ShowAsWindow();
        }

        private void AppiumCapabilities_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo() { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }

        private void xLoadDeviceWindow_Checked(object sender, RoutedEventArgs e)
        {
            if (xAutoRefreshModePnl == null)
            {
                return;
            }

            if (xLoadDeviceWindow.IsChecked == true)
            {
                xAutoRefreshModePnl.Visibility = Visibility.Visible;
            }
            else
            {
                xAutoRefreshModePnl.Visibility = Visibility.Hidden;
            }
        }

        private void DeviceDetailsChanged(object sender, TextChangedEventArgs e)
        {
            if (xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetDeviceCapabilities();
            }
        }

        private void PlatformSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetPlatformCapabilities();
                SetApplicationCapabilities();
            }
        }

        private async void DeviceSourceSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetDeviceSourceCapabilities();
                if (mDeviceSource != null && mDeviceSource.Value == nameof(eDeviceSource.MicroFoucsUFTMLab))
                {
                    await FetchUFTMDevicesAsync();
                }
            }
        }

        private void ActivityTypeSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded && xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetApplicationCapabilities();
            }
        }

        private void xAutoUpdateCapabiltiies_Checked(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded)
            {
                AutoSetCapabilities();
            }
        }

        private void xUseProxyChkBox_Checked(object sender, RoutedEventArgs e)
        {
            if (xProxyTextBox != null)
            {
                xProxyTextBox.IsEnabled = true;
            }
        }

        private void xUseProxyChkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (xProxyTextBox != null)
            {
                xProxyTextBox.IsEnabled = false;
                xProxyTextBox.ValueTextBox.Text = string.Empty;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((xAndroidRdBtn.IsChecked == null || xAndroidRdBtn.IsChecked == false) && (xIOSRdBtn.IsChecked == null || xIOSRdBtn.IsChecked == false))
            {
                //TODO: binding lost from some reason- need to find out why
                BindRadioButtons();
            }
        }

        private void DeleteUFTMServerCapabilities()
        {
            DeleteCapabilityIfExist("uftm:appiumVersion");
            DeleteCapabilityIfExist("uftm:oauthClientId");
            DeleteCapabilityIfExist("uftm:oauthClientSecret");
            DeleteCapabilityIfExist("uftm:tenantId");
        }

        private void DeleteKobitonServerCapabilities()
        {
            DeleteCapabilityIfExist("kobiton:username");
            DeleteCapabilityIfExist("kobiton:accessKey");
            DeleteCapabilityIfExist("kobiton:sessionName");
            DeleteCapabilityIfExist("kobiton:deviceGroup");

            //for backward support
            DeleteCapabilityIfExist("username");
            DeleteCapabilityIfExist("accessKey");
            DeleteCapabilityIfExist("sessionName");
            DeleteCapabilityIfExist("deviceGroup");
        }

        private void xSupportSimulations_Click(object sender, RoutedEventArgs e)
        {
            if (xAutoUpdateCapabiltiies.IsChecked == true)
            {
                if (xUFTMSupportSimulations.IsChecked == true)
                {
                    SetUFTMSupportSimulationsCapabilities();
                }
                else
                {
                    DeleteUFTMSupportSimulationsCapabilities();
                }
            }
        }

        private void DeleteUFTMSupportSimulationsCapabilities()
        {
            DeleteCapabilityIfExist("uftm:installPackagedApp");
        }
        private void FetchDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            if (mAppiumCapabilities?.MultiValues == null)
            {
                return;
            }

            var credentials = EnsureUftCredentialParams();
            string currentDeviceName = FindExistingCapability("appium:deviceName")?.Value;
            string currentDeviceUuid = FindExistingCapability("appium:udid")?.Value;



            DriverConfigParam serverParam = mAppiumServer;
            UFTCredentialsDialog dialog = new UFTCredentialsDialog(serverParam, credentials.clientId, credentials.clientSecret, credentials.tenantId, FetchUFTMDevicesAsync, currentDeviceName, currentDeviceUuid);

            Window owner = Window.GetWindow(this);

            bool? dialogResult = dialog.ShowDialog(owner);
            if (dialogResult == true)
            {
                ApplySelectedPhoneDetails(dialog);
            }
        }

        private void ApplySelectedPhoneDetails(UFTCredentialsDialog dialog)
        {
            if (dialog == null)
            {
                return;
            }

            bool wasUpdated = false;

            // Apply the dialog selection state as-is.
            // If user unselected the device, clear the capabilities.
            wasUpdated |= UpdateCapabilityValue("appium:deviceName", dialog.SelectedPhoneName ?? string.Empty);
            wasUpdated |= UpdateCapabilityValue("appium:udid", dialog.SelectedPhoneUuid ?? string.Empty);

            bool platformUpdated = ApplyPlatformFromSelection(dialog.SelectedPhonePlatform);

            if (wasUpdated || platformUpdated)
            {
                xCapabilitiesGrid.Grid?.Items.Refresh();
            }
        }

        private bool ApplyPlatformFromSelection(string selectedPlatform)
        {
            if (string.IsNullOrWhiteSpace(selectedPlatform))
            {
                return false;
            }

            if (!TryResolvePlatformType(selectedPlatform, out eDevicePlatformType platformType))
            {
                return false;
            }

            string platformName = platformType == eDevicePlatformType.Android
                ? nameof(eDevicePlatformType.Android)
                : nameof(eDevicePlatformType.iOS);

            if (string.Equals(mDevicePlatformType?.Value, platformName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            mDevicePlatformType.Value = platformName;

            if (xAutoUpdateCapabiltiies?.IsChecked == true)
            {
                SetPlatformCapabilities();
                SetApplicationCapabilities();
            }

            xAndroidRdBtn.IsChecked = platformType == eDevicePlatformType.Android;
            xIOSRdBtn.IsChecked = platformType == eDevicePlatformType.iOS;

            return true;
        }

        private static bool TryResolvePlatformType(string platformValue, out eDevicePlatformType platformType)
        {
            platformType = eDevicePlatformType.Android;

            if (string.IsNullOrWhiteSpace(platformValue))
            {
                return false;
            }

            string normalized = platformValue.Trim().ToLowerInvariant();

            if (normalized.Contains("ios") || normalized.Contains("iphone") || normalized.Contains("ipad") || normalized.Contains("apple"))
            {
                platformType = eDevicePlatformType.iOS;
                return true;
            }

            if (normalized.Contains("android"))
            {
                platformType = eDevicePlatformType.Android;
                return true;
            }

            return false;
        }

        private bool UpdateCapabilityValue(string parameterName, string value)
        {
            if (string.IsNullOrWhiteSpace(parameterName) || value == null)
            {
                return false;
            }

            DriverConfigParam capability = FindExistingCapability(parameterName);
            if (capability == null)
            {
                capability = new DriverConfigParam() { Parameter = parameterName };
                mAppiumCapabilities.MultiValues.Add(capability);
            }

            if (capability.Value == value)
            {
                return false;
            }

            capability.Value = value;
            return true;
        }

        private (DriverConfigParam clientId, DriverConfigParam clientSecret, DriverConfigParam tenantId) EnsureUftCredentialParams()
        {
            DriverConfigParam clientId = EnsureCapabilityExists("uftm:oauthClientId", "UFT Execution key Client Id");
            DriverConfigParam clientSecret = EnsureCapabilityExists("uftm:oauthClientSecret", "UFT Execution key Client Password");
            DriverConfigParam tenantId = EnsureCapabilityExists("uftm:tenantId", "Default value (Need to change only when using UFT shared spaces))");
            if (string.IsNullOrEmpty(tenantId.Value))
            {
                tenantId.Value = "\"999999999\"";
            }

            return (clientId, clientSecret, tenantId);
        }

        private DriverConfigParam EnsureCapabilityExists(string parameter, string description)
        {
            DriverConfigParam capability = FindExistingCapability(parameter);
            if (capability == null)
            {
                capability = new DriverConfigParam() { Parameter = parameter, Description = description };
                mAppiumCapabilities.MultiValues.Add(capability);
            }
            else if (string.IsNullOrEmpty(capability.Description))
            {
                capability.Description = description;
            }

            return capability;
        }

        // Performs OAuth2 token request and then fetches device content from UFT Mobile
        private async Task<string> FetchUFTMDevicesAsync()
        {
            try
            {
                //, remove the url ending after :7500
                var baseUrl = mAppiumServer?.Value?.TrimEnd('/') ?? string.Empty;
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    int portIdx = baseUrl.IndexOf(":7500", StringComparison.OrdinalIgnoreCase);
                    if (portIdx >= 0)
                    {
                        // keep everything up to and including ':7500'
                        baseUrl = baseUrl.Substring(0, portIdx + 5).TrimEnd('/');
                    }
                }
                if (string.IsNullOrEmpty(baseUrl))
                {
                    return "UFTM server URL is empty.";
                }

                string clientId = FindExistingCapability("uftm:oauthClientId")?.Value;
                string clientSecret = FindExistingCapability("uftm:oauthClientSecret")?.Value;
                string tenantStr = FindExistingCapability("uftm:tenantId")?.Value;

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret) || string.IsNullOrWhiteSpace(tenantStr))
                {
                    return "Missing UFTM OAuth credentials (clientId/secret/tenant).";
                }

                if (tenantStr.StartsWith("\"") && tenantStr.EndsWith("\""))
                {
                    tenantStr = tenantStr.Trim('"');
                }
                if (!int.TryParse(tenantStr, out var tenantId))
                {
                    return "Invalid tenant id.";
                }


                // 1) Authenticate: POST /rest/oauth2/token (same pattern as GenericAppiumDriver)
                var tokenUrl = baseUrl + "/rest/oauth2/token";
                var authClient = new RestClient(tokenUrl);
                var authRequest = new RestRequest(tokenUrl, Method.Post);

                var oauthPayload = new { client = clientId, secret = clientSecret, tenant = tenantId };
                authRequest.AddJsonBody(oauthPayload);

                var authResponse = await authClient.ExecuteAsync(authRequest);
                if (!authResponse.IsSuccessful)
                {
                    return $"Auth failed: {(int)authResponse.StatusCode} {authResponse.StatusDescription}\n{authResponse.Content}";
                }

                // Extract tokens from auth response
                // 1) access token from JSON content
                string accessToken = null;
                try
                {
                    using var doc = JsonDocument.Parse(authResponse.Content ?? "{}");
                    if (doc.RootElement.TryGetProperty("access_token", out var tokenProp))
                    {
                        accessToken = tokenProp.GetString();
                    }
                    else if (doc.RootElement.TryGetProperty("token", out var tokenProp2))
                    {
                        accessToken = tokenProp2.GetString();
                    }
                }
                catch { }

                // 2) csrf token (hp4msecret) and 3) JSESSIONID from cookies
                string cookieHp4mSecret = authResponse.Cookies?.FirstOrDefault(c => string.Equals(c.Name, "x-hp4msecret", StringComparison.OrdinalIgnoreCase))?.Value;
                if (string.IsNullOrEmpty(cookieHp4mSecret))
                {
                    // Some servers set hp4msecret as 'hp4msecret' without the 'x-'
                    cookieHp4mSecret = authResponse.Cookies?.FirstOrDefault(c => string.Equals(c.Name, "hp4msecret", StringComparison.OrdinalIgnoreCase))?.Value;
                }
                string cookieSessionId = authResponse.Cookies?.FirstOrDefault(c => string.Equals(c.Name, "JSESSIONID", StringComparison.OrdinalIgnoreCase))?.Value;

                

               

                // 3) Fetch device content: GET /rest/deviceContent
                var devicesUrl = baseUrl + "/rest/deviceContent";
                var devicesClient = new RestClient(devicesUrl);
                var devicesRequest = new RestRequest(devicesUrl, Method.Get);

                // Add required headers
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    devicesRequest.AddHeader("Authorization", $"Bearer {accessToken}");
                }
                if (!string.IsNullOrWhiteSpace(cookieHp4mSecret))
                {
                    devicesRequest.AddHeader("x-hp4msecret", cookieHp4mSecret);
                }
                if (!string.IsNullOrWhiteSpace(cookieSessionId))
                {
                    devicesRequest.AddHeader("JSESSIONID", cookieSessionId);
                }
                devicesRequest.AddHeader("Accept", "*/*");
                devicesRequest.AddHeader("tenant-id", tenantId.ToString());

                var devicesResponse = await devicesClient.ExecuteAsync(devicesRequest);
                


                return devicesResponse.Content;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "UFTM device fetch failed", ex);
                return $"Error: {ex.Message}";
            }
        }
    }


    public class RadioBtnEnumConfigConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : Binding.DoNothing;
        }
    }

    public class CheckboxConfigConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            bool res = false;
            Boolean.TryParse(value.ToString(), out res);
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
