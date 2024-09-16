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

using Ginger.Configurations;
using Ginger.UserControlsLib;
using Amdocs.Ginger.Common;
using System.Windows;
using GingerCore;
using amdocs.ginger.GingerCoreNET;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using IdentityModel.Client;
using Ginger.ValidationRules;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for GingerAnalyticsConfigurationPage.xaml
    /// </summary>
    public partial class GingerAnalyticsConfigurationPage : GingerUIPage
    {
        public GingerAnalyticsConfiguration gingerAnalyticsUserConfig;
        public GingerAnalyticsAPI analyticsAPI;
        public GingerAnalyticsConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            analyticsAPI = new GingerAnalyticsAPI();
            gingerAnalyticsUserConfig  = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerAnalyticsConfiguration>().Count == 0 ? new GingerAnalyticsConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerAnalyticsConfiguration>();
            gingerAnalyticsUserConfig.StartDirtyTracking();
            SetControls();

        }

        private void SetControls()
        {
            Context mContext = new();

            xGAAURLTextBox.Init(mContext, gingerAnalyticsUserConfig, nameof(GingerAnalyticsConfiguration.AccountUrl));
            xISURLTextBox.Init(mContext, gingerAnalyticsUserConfig, nameof(GingerAnalyticsConfiguration.IdentityServiceURL));
            xClientIdTextBox.Init(mContext, gingerAnalyticsUserConfig, nameof(GingerAnalyticsConfiguration.ClientId));
            xClientSecretTextBox.Init(mContext, gingerAnalyticsUserConfig, nameof(GingerAnalyticsConfiguration.ClientSecret));
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

            GingerCoreNET.GeneralLib.General.CreateGingerAnalyticsConfiguration(gingerAnalyticsUserConfig);

            if (GingerAnalyticsAPI.IsTokenValid())
            {
                Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionSuccess);
                HideLoader();
                xTestConBtn.IsEnabled = true;
                return;
            }

            bool isAuthorized = await HandleTokenAuthorization();
            ShowConnectionResult(isAuthorized);
            HideLoader();
            xTestConBtn.IsEnabled = true;
        }

        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerAnalyticsUserConfig.AccountUrl)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.IdentityServiceURL)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientId)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientSecret);
        }

        public async Task<bool> HandleTokenAuthorization()
        {
            if (string.IsNullOrEmpty(gingerAnalyticsUserConfig.Token))
            {
                return await GingerAnalyticsAPI.RequestToken(ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.ClientId),
                                          ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.ClientSecret),
                                          ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.IdentityServiceURL));
            }

            return true;
        }

        public static void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionSuccess);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionFail);
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
