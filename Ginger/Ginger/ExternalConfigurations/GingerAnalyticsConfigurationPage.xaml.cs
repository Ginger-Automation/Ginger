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
        public DateTime validTo = DateTime.MinValue;
        private GingerAnalyticsConfiguration gingerAnalyticsUserConfig = null;
        public GingerAnalyticsConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
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
            if (AreRequiredFieldsEmpty())
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                return;
            }

            GingerCoreNET.GeneralLib.General.CreateGingerAnalyticsConfiguration(gingerAnalyticsUserConfig);

            if (IsTokenValid())
            {
                Reporter.ToUser(eUserMsgKey.GingerAnalyticsConnectionSuccess);
                return;
            }

            bool isAuthorized = await HandleTokenAuthorization();
            ShowConnectionResult(isAuthorized);
        }

        private bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerAnalyticsUserConfig.AccountUrl)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.IdentityServiceURL)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientId)
                || string.IsNullOrEmpty(gingerAnalyticsUserConfig.ClientSecret);
        }

        private async Task<bool> HandleTokenAuthorization()
        {
            if (string.IsNullOrEmpty(gingerAnalyticsUserConfig.Token))
            {
                return await RequestToken(gingerAnalyticsUserConfig.ClientId, gingerAnalyticsUserConfig.ClientSecret, gingerAnalyticsUserConfig.IdentityServiceURL);
            }

            return true;
        }

        private static void ShowConnectionResult(bool isAuthorized)
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


        public async Task<bool> RequestToken(string clientId, string clientSecret, string address)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler() { UseProxy = false};

                using (var client = new HttpClient(handler))
                {
                    clientId = ValueExpression.CredentialsCalculation(gingerAnalyticsUserConfig.ClientId);
                    clientSecret = ValueExpression.CredentialsCalculation(gingerAnalyticsUserConfig.ClientSecret);
                    address = ValueExpression.CredentialsCalculation(gingerAnalyticsUserConfig.IdentityServiceURL);

                    var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
                    {
                        Address = address,
                        Policy =
                    {
                RequireHttps = true,
                ValidateIssuerName = true
                    }
                    });

                    if (disco.IsError)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Discovery document error: {disco.Error}");
                        return false;
                    }

                    var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                    {
                        Address = disco.TokenEndpoint,
                        ClientId = clientId,
                        ClientSecret = clientSecret
                    });

                    if (tokenResponse.IsError)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Token request error: {tokenResponse.Error}");
                        return false;
                    }

                    validTo = DateTime.UtcNow.AddMinutes(60);
                    gingerAnalyticsUserConfig.Token = tokenResponse.AccessToken;
                    return true;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Reporter.ToLog(eLogLevel.ERROR, "HTTP request failed", httpEx);
                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unexpected error during token request", ex);
                return false;
            }
        }

        public bool IsTokenValid()
        {
            try
            {
                if (string.IsNullOrEmpty(gingerAnalyticsUserConfig.Token) || gingerAnalyticsUserConfig.Token.Split('.').Length != 3)
                {
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(gingerAnalyticsUserConfig.Token);
                validTo = jwtToken.ValidTo;
                if (DateTime.UtcNow < validTo)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured in validate token", ex);
                return false;
            }
        }
    }
}
