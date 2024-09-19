using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Telemetry;
using GingerCore.GeneralLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.Telemetry
{
    /// <summary>
    /// Interaction logic for TelemetryConfigPage.xaml
    /// </summary>
    public partial class TelemetryConfigPage : Page
    {
        private GenericWindow? _genericWindow;

        public TelemetryConfigPage()
        {
            InitializeComponent();
            _genericWindow = null;
            InitializeControls();
        }

        private void InitializeControls()
        {
            InitializeTelemetryTrackingRadioButtons();
            ITelemetryQueueManager.Config config = WorkSpace.Instance.UserProfile.TelemetryConfig;

            BindingHandler.ObjFieldBinding(BufferSizeTextBox, TextBox.TextProperty, config, nameof(ITelemetryQueueManager.Config.BufferSize));
            BindingHandler.ObjFieldBinding(CollectorURLTextBox, TextBox.TextProperty, config, nameof(ITelemetryQueueManager.Config.CollectorURL));
            GingerCore.General.FillComboFromEnumType(LogLevelComboBox, typeof(eLogLevel), values: Enum.GetValues<eLogLevel>().Order().Cast<object>().ToList(), textWiseSorting: false);
            BindingHandler.ObjFieldBinding(LogLevelComboBox, ComboBox.SelectedValueProperty, config, nameof(ITelemetryQueueManager.Config.MinLogLevel));
            BindingHandler.ObjFieldBinding(RetryIntervalTextBox, TextBox.TextProperty, config, nameof(ITelemetryQueueManager.Config.RetryIntervalInSeconds));
            BindingHandler.ObjFieldBinding(RetryPollingSizeTextBox, TextBox.TextProperty, config, nameof(ITelemetryQueueManager.Config.RetryPollingSize));
        }

        private void InitializeTelemetryTrackingRadioButtons()
        {
            TelemetryTrackingOnRadioButton.IsChecked = WorkSpace.Instance.UserProfile.EnableTelemetry;
            TelemetryTrackingOffRadioButton.IsChecked = !WorkSpace.Instance.UserProfile.EnableTelemetry;
        }

        public void ShowAsWindow()
        {
            GingerCore.General.LoadGenericWindow(ref _genericWindow, App.MainWindow, Ginger.eWindowShowStyle.Dialog, this.Title, this);
            WorkSpace.Instance.UserProfile.UserProfileOperations.SaveUserProfile();
        }

        private void TelemetryTrackingRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == TelemetryTrackingOnRadioButton)
            {
                WorkSpace.Instance.UserProfile.EnableTelemetry = true;
                ShowConfig();
            }
            else if (sender == TelemetryTrackingOffRadioButton)
            {
                WorkSpace.Instance.UserProfile.EnableTelemetry = false;
                HideConfig();
            }
        }

        private void ShowConfig()
        {
            BufferSizeWrapper.Visibility = Visibility.Visible;
            CollectorURLWrapper.Visibility = Visibility.Visible;
            MinimumLogLevelWrapper.Visibility = Visibility.Visible;
            RetryIntervalWrapper.Visibility = Visibility.Visible;
            RetryPollingSizeWrapper.Visibility = Visibility.Visible;
        }

        private void HideConfig()
        {
            BufferSizeWrapper.Visibility = Visibility.Collapsed;
            CollectorURLWrapper.Visibility = Visibility.Collapsed;
            MinimumLogLevelWrapper.Visibility = Visibility.Collapsed;
            RetryIntervalWrapper.Visibility = Visibility.Collapsed;
            RetryPollingSizeWrapper.Visibility = Visibility.Collapsed;
        }
    }
}
