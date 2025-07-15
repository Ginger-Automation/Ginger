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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.CoreNET.External.GingerPlay;
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using GingerCore;
using GingerCore.GeneralLib;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for GingerPlayConfigurationpage.xaml
    /// </summary>
    public partial class GingerPlayConfigurationpage : GingerUIPage
    {
        private GingerPlayConfiguration gingerPlayConfiguration;
        private GingerPlayAPITokenManager GingerPlayAPITokenManager = new();

        public GingerPlayConfigurationpage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            GingerCoreNET.GeneralLib.General.CreateGingerPlayConfiguration();
            gingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0 ? new GingerPlayConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
            gingerPlayConfiguration.StartDirtyTracking();
            SetControls();
        }

        private void SetControls()
        {
            Context mContext = new();
            xGatewayURLTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayGatewayUrl));
            xClientIdTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientId));
            xClientSecretTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientSecret));
            BindingHandler.ObjFieldBinding(xAllowGingerPlayCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayEnabled));
            BindingHandler.ObjFieldBinding(xGatewayURLTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayGatewayUrl));
            BindingHandler.ObjFieldBinding(xClientIdTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientId));
            BindingHandler.ObjFieldBinding(xClientSecretTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientSecret));
            BindingHandler.ObjFieldBinding(xExecutionServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayExecutionServiceEnabled));
            BindingHandler.ObjFieldBinding(xReportServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayReportServiceEnabled));
            BindingHandler.ObjFieldBinding(xAIServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayAIServiceEnabled));
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xGatewayURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Gateway URL cannot be empty"));
        }


        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerPlayConfiguration.GingerPlayGatewayUrl);
        }

        public bool AreCredentialsFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerPlayConfiguration.GingerPlayClientId) || string.IsNullOrEmpty(gingerPlayConfiguration.GingerPlayClientSecret);
        }

        private void xAllowGingerPlayCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            UpdateControlStates();
        }

        private async void xTestConBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoader();
                xTestConBtn.IsEnabled = false;
                if (AreRequiredFieldsEmpty())
                {
                    Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                    return;
                }

                if (!AreCredentialsFieldsEmpty())
                {
                    bool isAuthorized = await GingerPlayAPITokenManager.GetOrValidateToken();
                    // Show main connection result
                    ShowConnectionResult(isAuthorized);
                }
                ShowServicesHealth();
                return;
            }
            finally
            {
                HideLoader();
                xTestConBtn.IsEnabled = true;
            }
        }

        private async Task ShowServicesHealth()
        {
            // Check health for selected services
            var healthMessages = new List<string>();

            if ((bool)xReportServiceCheckBox.IsChecked)
            {
                bool isReportServiceHealthy = await GingerPlayAPITokenManager.IsServiceHealthyAsync(GingerPlayEndPointManager.GetReportServiceHealthUrl());
                healthMessages.Add($"Report Service: {(isReportServiceHealthy ? "UP" : "DOWN")}");
            }
            if ((bool)xExecutionServiceCheckBox.IsChecked)
            {
                bool isExecutionServiceHealthy = await GingerPlayAPITokenManager.IsServiceHealthyAsync(GingerPlayEndPointManager.GetExecutionServiceHealthUrl());
                healthMessages.Add($"Execution Service: {(isExecutionServiceHealthy ? "UP" : "DOWN")}");
            }
            if ((bool)xAIServiceCheckBox.IsChecked)
            {
                bool isAIServiceHealthy = await GingerPlayAPITokenManager.IsServiceHealthyAsync(GingerPlayEndPointManager.GetAIServiceHealthUrl());
                healthMessages.Add($"AI Service: {(isAIServiceHealthy ? "UP" : "DOWN")}");
            }

            if (healthMessages.Count > 0)
            {
                string summary = string.Join("\n", healthMessages);
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, $"Service Health Status:\n{summary}");
            }
        }

        private static void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                Reporter.ToUser(eUserMsgKey.GingerPlayConnectionSuccess);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.GingerPlayConnectionFail);
            }
        }
        public void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Hidden;
            });
        }

        public void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Visible;
            });
        }

        private void xAllowGingerPlayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateControlStates();
        }
        private void UpdateControlStates()
        {
            bool isChecked = (bool)xAllowGingerPlayCheckBox.IsChecked;
            gingerPlayConfiguration.GingerPlayEnabled = isChecked;
            xGatewayURLTextBox.IsEnabled = isChecked;
            xClientIdTextBox.IsEnabled = isChecked;
            xClientSecretTextBox.IsEnabled = isChecked;
            xReportServiceCheckBox.IsEnabled = isChecked;
            xAIServiceCheckBox.IsEnabled = isChecked;
            xExecutionServiceCheckBox.IsEnabled = isChecked;
            xTestConBtn.IsEnabled = isChecked;
        }
        private void xClientIdTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientIdTextBox.ValueTextBox.Text))
            {
                xClientIdTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientIdTextBox.ValueTextBox.Text) ? xClientIdTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientIdTextBox.ValueTextBox.Text);
            }
        }

        private void xClientSecretTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientSecretTextBox.ValueTextBox.Text))
            {
                xClientSecretTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientSecretTextBox.ValueTextBox.Text) ? xClientSecretTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientSecretTextBox.ValueTextBox.Text);
            }
        }
    }
}

