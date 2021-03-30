using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Mobile;
using Ginger.UserControls;
using GingerCore;
using GingerCore.GeneralLib;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Drivers
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

        public AppiumDriverEditPage(Agent appiumAgent)
        {
            InitializeComponent();
            
            mAgent = appiumAgent;
            BindConfigurationsFields();
        }

        private void BindConfigurationsFields()
        {
            mAppiumServer = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumServer));
            //BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.TextProperty, mAppiumServer, nameof(DriverConfigParam.Value));
            xServerURLTextBox.Init(null, mAppiumServer, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.ToolTipProperty, mAppiumServer, nameof(DriverConfigParam.Description));

            BindingHandler.ObjFieldBinding(xLoadDeviceWindow, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.LoadDeviceWindow)), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());
            mDeviceAutoScreenshotRefreshMode = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DeviceAutoScreenshotRefreshMode));
            BindingHandler.ObjFieldBinding(xContinualRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eAutoScreenshotRefreshMode.Continual.ToString());
            BindingHandler.ObjFieldBinding(xPostOperationRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eAutoScreenshotRefreshMode.PostOperation.ToString());
            BindingHandler.ObjFieldBinding(xDisabledRdBtn, RadioButton.IsCheckedProperty, mDeviceAutoScreenshotRefreshMode, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eAutoScreenshotRefreshMode.Disabled.ToString());

            BindingHandler.ObjFieldBinding(xLoadTimeoutTxtbox, TextBox.TextProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DriverLoadWaitingTime)), nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xLoadTimeoutTxtbox, TextBox.ToolTipProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DriverLoadWaitingTime)), nameof(DriverConfigParam.Description));

            mDevicePlatformType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DevicePlatformType));
            BindingHandler.ObjFieldBinding(xAndroidRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eDevicePlatformType.Android.ToString());
            BindingHandler.ObjFieldBinding(xIOSRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eDevicePlatformType.iOS.ToString());

            mAppType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppType));           
            BindingHandler.ObjFieldBinding(xNativeHybRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eAppType.NativeHybride.ToString());
            BindingHandler.ObjFieldBinding(xWebRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: eAppType.Web.ToString());

            mAppiumCapabilities = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumCapabilities));
            if (mAppiumCapabilities.MultiValues == null || mAppiumCapabilities.MultiValues.Count == 0)
            {
                mAppiumCapabilities.MultiValues = new ObservableList<DriverConfigParam>();
                AutoSetCapabilities(true);
            }
            SetCapabilitiesGridView();
        }

        private void SetCapabilitiesGridView()
        {
            xCapabilitiesGrid.SetTitleLightStyle = true;

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Parameter, Header = "Capability", WidthWeight = 20 });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Value, WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, MaxWidth = 35, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["ParamValueExpressionButton"] });
            view.GridColsView.Add(new GridColView() { Field = DriverConfigParam.Fields.Description, BindingMode = BindingMode.OneWay, WidthWeight = 45 });

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
            DriverConfigParam automationName = new DriverConfigParam() { Parameter = "automationName", Description = "Which automation engine to use" };
            if (mDevicePlatformType.Value == eDevicePlatformType.Android.ToString())
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

        private void SetApplicationCapabilities(bool init=false)
        {
            DriverConfigParam appPackage = new DriverConfigParam() { Parameter = "appPackage", Description = "Java package of the Android app you want to run", Value = "com.android.settings" };
            DriverConfigParam appActivity = new DriverConfigParam() { Parameter = "appActivity", Description = "Activity name for the Android activity you want to launch from your package", Value = "com.android.settings.Settings" };
            DriverConfigParam bundleId = new DriverConfigParam() { Parameter = "bundleId", Description = "Bundle ID of the app under test", Value = "com.apple.Preferences" };
            DriverConfigParam browserName = new DriverConfigParam() { Parameter = "browserName", Description = "Name of mobile web browser to automate" };
            if (mAppType.Value == eAppType.NativeHybride.ToString())
            {
                if (mDevicePlatformType.Value == eDevicePlatformType.Android.ToString())
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
            }
            else
            {
                if (mDevicePlatformType.Value == eDevicePlatformType.Android.ToString())
                {
                    browserName.Value = "Chrome";
                }
                else
                {
                    browserName.Value = "Safari";
                }                
                AddOrUpdateCapability(browserName);
                DeleteCapabilityIfExist(bundleId.Parameter);
                DeleteCapabilityIfExist(appPackage.Parameter);
                DeleteCapabilityIfExist(appActivity.Parameter);
            }
        }

        private void SetDeviceCapabilities(bool init = false)
        {
            DriverConfigParam deviceName = new DriverConfigParam() { Parameter = "deviceName", Value = string.Empty};
            DriverConfigParam udid = new DriverConfigParam() { Parameter = "udid", Description = "Unique device identifier of the connected physical device", Value = string.Empty };
            if (mDevicePlatformType.Value == eDevicePlatformType.Android.ToString())
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

        private void SetOtherCapabilities(bool init=false)
        {
            DriverConfigParam newCommandTimeout = new DriverConfigParam() { Parameter = "newCommandTimeout", Description = "How long (in seconds) Appium will wait for a new command from the client before assuming the client quit and ending the session", Value = "300" };
            if (!init)
            {
                SetCurrentCapabilityValue(newCommandTimeout);
            }
            AddOrUpdateCapability(newCommandTimeout);
        }

        private void AddOrUpdateCapability(DriverConfigParam capability)
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues.Where(x => x.Parameter == capability.Parameter).FirstOrDefault();
            if (existingCap != null)
            {
                existingCap.Value = capability.Value;
                existingCap.Description = capability.Description;
            }
            else
            {
                mAppiumCapabilities.MultiValues.Add(capability);
            }
        }

        private void SetCurrentCapabilityValue(DriverConfigParam capability)
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues.Where(x => x.Parameter == capability.Parameter).FirstOrDefault();
            if (existingCap != null && string.IsNullOrEmpty(existingCap.Value) == false)
            {
                capability.Value = existingCap.Value;
            }
        }

        private void DeleteCapabilityIfExist(string capabilityName)
        {
            DriverConfigParam existingCap = mAppiumCapabilities.MultiValues.Where(x => x.Parameter == capabilityName).FirstOrDefault();
            if (existingCap != null)
            {
                mAppiumCapabilities.MultiValues.Remove(existingCap);
            }
        }

        private void AddCapability(object sender, RoutedEventArgs e)
        {
            mAppiumCapabilities.MultiValues.Add(new DriverConfigParam());
        }

        private void AutoSetCapabilities(bool init=false)
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
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
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
