using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.CoreNET;
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
        
        public AppiumDriverEditPage(Agent appiumAgent)
        {
            InitializeComponent();
            
            mAgent = appiumAgent;
            BindConfigurationsFields();
        }

        private void BindConfigurationsFields()
        {
            mAppiumServer = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumServer));
            BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.TextProperty, mAppiumServer, nameof(DriverConfigParam.Value));
            BindingHandler.ObjFieldBinding(xServerURLTextBox, TextBox.ToolTipProperty, mAppiumServer, nameof(DriverConfigParam.Description));

            BindingHandler.ObjFieldBinding(xLoadDeviceWindow, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.LoadDeviceWindow)), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());
            BindingHandler.ObjFieldBinding(xAutoRefreshDeviceWindow, CheckBox.IsCheckedProperty, mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AutoRefreshDeviceWindowScreenshot)), nameof(DriverConfigParam.Value), bindingConvertor: new CheckboxConfigConverter());

            mDevicePlatformType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.DevicePlatformType));
            BindingHandler.ObjFieldBinding(xAndroidRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: GenericAppiumDriver.eDevicePlatformType.Android.ToString());
            BindingHandler.ObjFieldBinding(xIOSRdBtn, RadioButton.IsCheckedProperty, mDevicePlatformType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: GenericAppiumDriver.eDevicePlatformType.iOS.ToString());

            mAppType = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppType));           
            BindingHandler.ObjFieldBinding(xNativeHybRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: GenericAppiumDriver.eAppType.NativeHybride.ToString());
            BindingHandler.ObjFieldBinding(xWebRdBtn, RadioButton.IsCheckedProperty, mAppType, nameof(DriverConfigParam.Value), bindingConvertor: new RadioBtnEnumConfigConverter(), converterParameter: GenericAppiumDriver.eAppType.Web.ToString());

            mAppiumCapabilities = mAgent.GetOrCreateParam(nameof(GenericAppiumDriver.AppiumCapabilities));
            if (mAppiumCapabilities.MultiValues == null || mAppiumCapabilities.MultiValues.Count == 0)
            {
                mAppiumCapabilities.MultiValues = new ObservableList<DriverConfigParam>();
                InitCapabilities();
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
            if (mDevicePlatformType.Value == GenericAppiumDriver.eDevicePlatformType.Android.ToString())
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

        private void SetApplicationCapabilities()
        {
            DriverConfigParam appPackage = new DriverConfigParam() { Parameter = "appPackage", Description = "Java package of the Android app you want to run" };
            DriverConfigParam appActivity = new DriverConfigParam() { Parameter = "appActivity", Description = "Activity name for the Android activity you want to launch from your package" };
            DriverConfigParam bundleId = new DriverConfigParam() { Parameter = "bundleId", Description = "Bundle ID of the app under test" };
            DriverConfigParam browserName = new DriverConfigParam() { Parameter = "browserName", Description = "Name of mobile web browser to automate" };
            if (mAppType.Value == GenericAppiumDriver.eAppType.NativeHybride.ToString())
            {
                if (mDevicePlatformType.Value == GenericAppiumDriver.eDevicePlatformType.Android.ToString())
                {
                    appPackage.Value = "com.android.settings";
                    appActivity.Value = "com.android.settings.Settings";
                    AddOrUpdateCapability(appPackage);
                    AddOrUpdateCapability(appActivity);
                    DeleteCapabilityIfExist(bundleId.Parameter);
                }
                else
                {
                    bundleId.Value = "com.apple.Preferences";
                    AddOrUpdateCapability(bundleId);
                    DeleteCapabilityIfExist(appPackage.Parameter);
                    DeleteCapabilityIfExist(appActivity.Parameter);                                      
                }
                DeleteCapabilityIfExist(browserName.Parameter);
            }
            else
            {
                if (mDevicePlatformType.Value == GenericAppiumDriver.eDevicePlatformType.Android.ToString())
                {
                    browserName.Value = "Chrome";
                }
                else
                {
                    browserName.Value = "Safari";
                }                
                AddOrUpdateCapability(browserName);
                DeleteCapabilityIfExist(bundleId.Parameter);
                DeleteCapabilityIfExist(appActivity.Parameter);
                DeleteCapabilityIfExist(browserName.Parameter);
            }
        }

        private void SetDeviceCapabilities()
        {
            DriverConfigParam deviceName = new DriverConfigParam() { Parameter = "deviceName", Description = "The kind of mobile device to use" };
            DriverConfigParam udid = new DriverConfigParam() { Parameter = "udid", Description = "Unique device identifier of the connected physical device" };
            deviceName.Value = xDeviceNameTextBox.Text;
            udid.Value = xDeviceIDTextBox.Text;            
            AddOrUpdateCapability(deviceName);
            AddOrUpdateCapability(udid);
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

        private void InitCapabilities()
        {
            SetDeviceCapabilities();
            SetPlatformCapabilities();
            SetApplicationCapabilities();            
        }

        private void ResetCapabilities(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "Capabilities list will be reset to default values, are you sure?") == eUserMsgSelection.Yes)
            {
                InitCapabilities();
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
            if (xAutoRefreshDeviceWindow == null)
            {
                return;
            }

            if (xLoadDeviceWindow.IsChecked == true)
            {
                xAutoRefreshDeviceWindow.Visibility = Visibility.Visible;
            }
            else
            {
                xAutoRefreshDeviceWindow.Visibility = Visibility.Hidden;
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
            if (xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetPlatformCapabilities();
            }
        }

        private void ActivityTypeSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (xAutoUpdateCapabiltiies != null && xAutoUpdateCapabiltiies.IsChecked == true)
            {
                SetApplicationCapabilities();
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
