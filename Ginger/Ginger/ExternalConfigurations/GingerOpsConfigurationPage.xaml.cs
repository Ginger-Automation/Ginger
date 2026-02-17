#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.Configurations;
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using GingerCore;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for GingerOpsConfigurationPage.xaml
    /// </summary>
    public partial class GingerOpsConfigurationPage : GingerUIPage
    {
        public GingerOpsConfiguration gingerOpsUserConfig;
        public GingerOpsAPI OpsAPI;
        public GingerOpsConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            OpsAPI = new GingerOpsAPI();
            gingerOpsUserConfig = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerOpsConfiguration>().Count == 0 ? new GingerOpsConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerOpsConfiguration>();
            gingerOpsUserConfig.StartDirtyTracking();
            WorkSpace.Instance.CurrentSelectedItem = gingerOpsUserConfig;
            SetControls();

        }

        private void SetControls()
        {
            Context mContext = new();

            xGAAURLTextBox.Init(mContext, gingerOpsUserConfig, nameof(GingerOpsConfiguration.AccountUrl));
            xISURLTextBox.Init(mContext, gingerOpsUserConfig, nameof(GingerOpsConfiguration.IdentityServiceURL));
            xClientIdTextBox.Init(mContext, gingerOpsUserConfig, nameof(GingerOpsConfiguration.ClientId));
            xClientSecretTextBox.Init(mContext, gingerOpsUserConfig, nameof(GingerOpsConfiguration.ClientSecret));
            ApplyValidationRules();

        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xGAAURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Report URL cannot be empty"));
            xISURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Identitiy Service URL cannot be empty"));
            xClientIdTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("ClientID cannot be empty"));
            xClientSecretTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("ClientSecret cannot be empty"));
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

            GingerCoreNET.GeneralLib.General.CreateGingerOpsConfiguration();


            bool isAuthorized = await HandleTokenAuthorization();
            ShowConnectionResult(isAuthorized);
            HideLoader();
            xTestConBtn.IsEnabled = true;
        }

        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerOpsUserConfig.AccountUrl)
                || string.IsNullOrEmpty(gingerOpsUserConfig.IdentityServiceURL)
                || string.IsNullOrEmpty(gingerOpsUserConfig.ClientId)
                || string.IsNullOrEmpty(gingerOpsUserConfig.ClientSecret);
        }

        public async Task<bool> HandleTokenAuthorization()
        {
            try
            {
                return await GingerOpsAPI.RequestToken(ValueExpression.PasswordCalculation(gingerOpsUserConfig.ClientId),
                                              ValueExpression.PasswordCalculation(gingerOpsUserConfig.ClientSecret),
                                              ValueExpression.PasswordCalculation(gingerOpsUserConfig.IdentityServiceURL));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to request GingerOps token", ex);
                return false;
            }
        }

        public static void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                Reporter.ToUser(eUserMsgKey.GingerOpsConnectionSuccess);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.GingerOpsConnectionFail);
            }
        }


        private void xClientSecretTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientSecretTextBox.ValueTextBox.Text))
            {
                xClientSecretTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientSecretTextBox.ValueTextBox.Text) ? xClientSecretTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientSecretTextBox.ValueTextBox.Text);

            }
        }

        private void xClientIdTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientIdTextBox.ValueTextBox.Text))
            {
                xClientIdTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientIdTextBox.ValueTextBox.Text) ? xClientIdTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientIdTextBox.ValueTextBox.Text);

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
    }
}
