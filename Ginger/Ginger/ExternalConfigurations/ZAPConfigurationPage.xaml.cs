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
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using GingerCore;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for ZAPsConfigurationPage.xaml
    /// </summary>
    public partial class ZAPConfigurationPage : GingerUIPage
    {
        public ZAPConfiguration zAPConfiguration;
        public ZAPConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            zAPConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ZAPConfiguration>().Count == 0 ? new ZAPConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<ZAPConfiguration>();
            SetControls();
            zAPConfiguration.StartDirtyTracking();
            WorkSpace.Instance.CurrentSelectedItem = zAPConfiguration;

        }

        private void SetControls()
        {
            Context mContext = new();

            xZAPURLTextBox.Init(mContext, zAPConfiguration, nameof(ZAPConfiguration.ZAPUrl));
            xZAPAPIkeyTextBox.Init(mContext, zAPConfiguration, nameof(ZAPConfiguration.ZAPApiKey));
            ApplyValidationRules();

        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xZAPAPIkeyTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("ZAP URL cannot be empty"));
        }

        private async void xTestConBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowLoader();
            xTestConBtn.IsEnabled = false;
            if (AreRequiredFieldsEmpty())
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                HideLoader();
                xTestConBtn.IsEnabled = true;
                return;
            }

            bool isConnected = await TestZAPConnectionAsync(ValueExpression.PasswordCalculation(zAPConfiguration.ZAPUrl), ValueExpression.PasswordCalculation(zAPConfiguration.ZAPApiKey));
            ShowConnectionResult(isConnected);

            HideLoader();
            xTestConBtn.IsEnabled = true;
        }

        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(zAPConfiguration.ZAPUrl)
                || string.IsNullOrEmpty(zAPConfiguration.ZAPApiKey);
        }


        public static void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                Reporter.ToUser(eUserMsgKey.ZAPConnectionSuccess);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ZAPConnectionFail);
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

        private void xZAPAPIkeyTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xZAPAPIkeyTextBox.ValueTextBox.Text))
            {
                xZAPAPIkeyTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xZAPAPIkeyTextBox.ValueTextBox.Text) ? xZAPAPIkeyTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xZAPAPIkeyTextBox.ValueTextBox.Text);
            }
        }

        private async Task<bool> TestZAPConnectionAsync(string zapUrl, string apiKey)
        {
            try
            {
                // Ensure URL does not end with a slash
                if (zapUrl.EndsWith('/'))
                {
                    zapUrl = zapUrl.TrimEnd('/');
                }

                string testUrl = $"{zapUrl}/JSON/core/view/version/?apikey={apiKey}";

                using HttpClient client = new HttpClient();
                var response = await client.GetAsync(testUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"ZAP connection failed. Status code: {response.StatusCode}");
                    return false;
                }

                string content = await response.Content.ReadAsStringAsync();

                // Optionally, check if content contains "version" field
                bool hasVersion = content.Contains("version");
                Reporter.ToLog(eLogLevel.DEBUG, $"ZAP response contains 'version': {hasVersion}");
                return hasVersion;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Exception during ZAP connection test: {ex.Message}", ex);
                return false;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }
    }
}
