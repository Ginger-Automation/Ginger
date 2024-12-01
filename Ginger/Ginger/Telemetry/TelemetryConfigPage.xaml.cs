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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Telemetry;
using GingerCore.GeneralLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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
