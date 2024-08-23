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

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for GingerAnalyticsConfigurationPage.xaml
    /// </summary>
    public partial class GingerAnalyticsConfigurationPage : GingerUIPage
    {
        private GingerAnalyticsConfiguration gingerAnalyticsUserConfig;
        ValueExpression valueExpression;
        public GingerAnalyticsConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            gingerAnalyticsUserConfig = WorkSpace.Instance.Solution.GingerAnalyticsConfiguration;
            valueExpression = new ValueExpression();
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
            xGAAURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("URL cannot be empty"));
            xISURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("URL cannot be empty"));
            xClientIdTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("ClientID cannot be empty"));
            xClientSecretTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("ClientSecret cannot be empty"));
        }

        private async void xTestConBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(gingerAnalyticsUserConfig.AccountUrl) || string.IsNullOrEmpty(gingerAnalyticsUserConfig.IdentityServiceURL)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientId) || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientSecret))
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                return;
            }


            if (gingerAnalyticsUserConfig != null && gingerAnalyticsUserConfig.Token.Equals("token"))
            {
                string clientId = CredentialsCalculation(WorkSpace.Instance.Solution.GingerAnalyticsConfiguration.ClientId);

                string clientSecret = CredentialsCalculation(WorkSpace.Instance.Solution.GingerAnalyticsConfiguration.ClientSecret);

                string address = CredentialsCalculation(WorkSpace.Instance.Solution.GingerAnalyticsConfiguration.IdentityServiceURL);

                bool isAuthorized = await RequestToken(clientId, clientSecret, address);

                if (isAuthorized)
                {
                    Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionSuccess);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionFail);
                }
            }
            else if (gingerAnalyticsUserConfig != null && !gingerAnalyticsUserConfig.Token.Equals("token"))
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

        public async Task<bool> RequestToken(string clientId, string clientSecret, string address)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler() { UseProxy = false };

                var client = new HttpClient(handler);

                var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                {
                    Address = address,
                    Policy =
                       {
                       RequireHttps = true,
                       ValidateIssuerName = true
                       }
                });

                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = disco.TokenEndpoint,

                    ClientId = clientId,
                    ClientSecret = clientSecret,
                });

                if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    gingerAnalyticsUserConfig.Token = tokenResponse.AccessToken;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to connect to the server", ex);
                return false;
            }
        }
        /// <summary>
        /// Calculates the actual value from the input string based on its type.
        /// If the input is a value expression, it computes the expression to get the value.
        /// If the input is an encrypted string, it decrypts the string to retrieve the original value.
        /// Returns the input as is if it doesn't match the above conditions.
        /// </summary>
        /// <param name="value">The input string which might be a value expression or an encrypted string.</param>
        /// <returns>The calculated or decrypted value, or the input string if no processing is needed.</returns>
        private string CredentialsCalculation(string value)
        {

            if (ValueExpression.IsThisAValueExpression(value))
            {
                valueExpression.DecryptFlag = true;
                value = valueExpression.Calculate(value);
                valueExpression.DecryptFlag = false;
                return value;
            }
            else if (EncryptionHandler.IsStringEncrypted(value))
            {
                value = EncryptionHandler.DecryptwithKey(value);
                return value;
            }

            return value;
        }
    }
}
