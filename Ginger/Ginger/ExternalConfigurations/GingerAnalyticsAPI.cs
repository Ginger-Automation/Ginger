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
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using IdentityModel.Client;
using System.IdentityModel.Tokens.Jwt;
using Ginger.Environments.GingerAnalyticsEnvWizardLib;
using GingerCore;
using static Ginger.Environments.GingerAnalyticsEnvWizardLib.GingerAnalyticsAPIResponseInfo;
using System.Collections.Generic;
using Ginger.Configurations;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Text.Json;
using Amdocs.Ginger.Common;
using amdocs.ginger.GingerCoreNET;
using Ginger.ExternalConfigurations;
using Newtonsoft.Json;
using System.Linq;
using static Ginger.Environments.GingerAnalyticsEnvWizardLib.GingerAnalyticsAPIResponseInfo;
using Microsoft.CodeAnalysis;
using Ginger.Environments.AddEnvironmentWizardLib;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTest.WizardLib;
using System.Runtime.InteropServices;
using Ginger.UserControlsLib;
using GingerCore.GeneralLib;
using GingerCore;
using OpenQA.Selenium;
using System.Windows;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// All the APIs for communicatig with Ginger Analytics
    /// </summary>
    public class GingerAnalyticsAPI
    {
        public static DateTime validTo = DateTime.MinValue;
        public static GingerAnalyticsConfiguration gingerAnalyticsUserConfig = SetConfigutation();

        private string bearerToken = string.Empty;

        public static async Task<bool> RequestToken(string clientId, string clientSecret, string address)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler() { UseProxy = false };

                using (var client = new HttpClient(handler))
                {

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

        public static bool IsTokenValid()
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

        public async Task<Dictionary<string, GingerAnalyticsProject>> FetchProjectDataFromGA(Dictionary<string, GingerAnalyticsProject> projectListGA)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerAnalyticsAPI.IsTokenValid())
                {
                    bearerToken = gingerAnalyticsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerAnalyticsAPI.RequestToken(ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.ClientId),
                        ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.ClientSecret),
                        ValueExpression.PasswordCalculation(gingerAnalyticsUserConfig.IdentityServiceURL));
                    if (resFlag)
                    {
                        bearerToken = gingerAnalyticsUserConfig.Token;
                    }
                }

                string apiUrl = $"{gingerAnalyticsUserConfig.AccountUrl}/Report/GetGingerAnalyticsProjects";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                var response = await client.GetStringAsync(apiUrl);
                var projectList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerAnalyticsAPIResponseInfo.GingerAnalyticsProject>>(response);

                if (projectList != null && projectList.Count > 0)
                {
                    projectListGA.Clear();
                    projectListGA = projectList.ToDictionary(k => k.Id);
                }
                return projectListGA;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from Ginger Analytics API", ex);
                return new Dictionary<string, GingerAnalyticsProject>();
            }
        }
        public async Task<Dictionary<string, GingerAnalyticsArchitectureB>> FetchEnvironmentDataFromGA(string architectureId, Dictionary<string, GingerAnalyticsArchitectureB> architectureListGA, bool publishedEnvironment = true)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerAnalyticsAPI.IsTokenValid())
                {
                    bearerToken = gingerAnalyticsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerAnalyticsAPI.RequestToken(gingerAnalyticsUserConfig.ClientId, gingerAnalyticsUserConfig.ClientSecret,
                        gingerAnalyticsUserConfig.IdentityServiceURL);
                    if (resFlag)
                    {
                        bearerToken = gingerAnalyticsUserConfig.Token;
                    }
                }

                string apiUrl = $"{gingerAnalyticsUserConfig.AccountUrl}/Report/GetGingerAnalyticsEnvironmentsByArchitecture/{publishedEnvironment}?architectureIds={architectureId}";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await client.GetStringAsync(apiUrl);
                var envList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerAnalyticsAPIResponseInfo.GingerAnalyticsArchitectureB>>(response);

                if (envList != null && envList.Count > 0)
                {
                    architectureListGA.Clear();
                    architectureListGA = envList.ToDictionary(k => k.Id);
                }

                return architectureListGA;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from Ginger Analytics API", ex);
                return new Dictionary<string, GingerAnalyticsArchitectureB>();
            }
        }
        public async Task<Dictionary<string, GingerAnalyticsEnvironmentB>> FetchApplicationDataFromGA(string environmentId, Dictionary<string, GingerAnalyticsEnvironmentB> environmentListGA)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerAnalyticsAPI.IsTokenValid())
                {
                    bearerToken = gingerAnalyticsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerAnalyticsAPI.RequestToken(gingerAnalyticsUserConfig.ClientId, gingerAnalyticsUserConfig.ClientSecret,
                        gingerAnalyticsUserConfig.IdentityServiceURL);
                    if (resFlag)
                    {
                        bearerToken = gingerAnalyticsUserConfig.Token;
                    }
                }

                string apiUrl = $"{gingerAnalyticsUserConfig.AccountUrl}/Report/GetGingerAnalyticsApplicationByEnvironment?environmentIds={environmentId}";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await client.GetStringAsync(apiUrl);
                var envList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerAnalyticsAPIResponseInfo.GingerAnalyticsEnvironmentB>>(response);

                if (envList != null && envList.Count > 0)
                {
                    environmentListGA.Clear();
                    environmentListGA = envList.ToDictionary(k => k.Id);
                }

                return environmentListGA;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from Ginger Analytics API", ex);
                return new Dictionary<string, GingerAnalyticsEnvironmentB>();
            }
        }

        private static GingerAnalyticsConfiguration SetConfigutation()
        {
            var workSpace = WorkSpace.Instance;
            if (workSpace == null || workSpace.SolutionRepository == null)
            {
                return new GingerAnalyticsConfiguration();
            }

            var item = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerAnalyticsConfiguration>(); ;
            return item == null ? new GingerAnalyticsConfiguration() : item;
        }
    }
}
