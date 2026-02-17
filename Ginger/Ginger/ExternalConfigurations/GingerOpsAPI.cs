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
using Ginger.Environments.GingerOpsEnvWizardLib;
using GingerCore;
using IdentityModel.Client;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static Ginger.Environments.GingerOpsEnvWizardLib.GingerOpsAPIResponseInfo;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// All the APIs for communicatig with GingerOps
    /// </summary>
    public class GingerOpsAPI
    {
        public static DateTime validTo = DateTime.MinValue;
        public static readonly GingerOpsConfiguration GingerOpsUserConfig = SetConfiguration();

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
                    GingerOpsUserConfig.Token = tokenResponse.AccessToken;
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
                if (string.IsNullOrEmpty(GingerOpsUserConfig.Token) || GingerOpsUserConfig.Token.Split('.').Length != 3)
                {
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(GingerOpsUserConfig.Token);
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

        public async Task<Dictionary<string, GingerOpsProject>> FetchProjectDataFromGOps(Dictionary<string, GingerOpsProject> projectListGOps)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerOpsAPI.IsTokenValid())
                {
                    bearerToken = GingerOpsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerOpsAPI.RequestToken(ValueExpression.PasswordCalculation(GingerOpsUserConfig.ClientId),
                        ValueExpression.PasswordCalculation(GingerOpsUserConfig.ClientSecret),
                        ValueExpression.PasswordCalculation(GingerOpsUserConfig.IdentityServiceURL));
                    if (resFlag)
                    {
                        bearerToken = GingerOpsUserConfig.Token;
                    }
                }

                string apiUrl = $"{GingerOpsUserConfig.AccountUrl}/Project/GetProjects";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
                var response = await client.GetStringAsync(apiUrl);
                var projectList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerOpsAPIResponseInfo.GingerOpsProject>>(response);

                if (projectList != null && projectList.Count > 0)
                {
                    projectListGOps.Clear();
                    projectListGOps = projectList.ToDictionary(k => k.Id);
                }
                return projectListGOps;

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from GingerOps API", ex);
                return [];
            }
        }
        public async Task<Dictionary<string, GingerOpsArchitectureB>> FetchEnvironmentDataFromGOps(string architectureId, Dictionary<string, GingerOpsArchitectureB> architectureListGOps, bool publishedEnvironment = true)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerOpsAPI.IsTokenValid())
                {
                    bearerToken = GingerOpsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerOpsAPI.RequestToken(GingerOpsUserConfig.ClientId, GingerOpsUserConfig.ClientSecret,
                        GingerOpsUserConfig.IdentityServiceURL);
                    if (resFlag)
                    {
                        bearerToken = GingerOpsUserConfig.Token;
                    }
                }

                string apiUrl = $"{GingerOpsUserConfig.AccountUrl}/Environment/GetEnvironments/{publishedEnvironment}?architectureIds={architectureId}";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await client.GetStringAsync(apiUrl);
                var envList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerOpsAPIResponseInfo.GingerOpsArchitectureB>>(response);

                if (envList != null && envList.Count > 0)
                {
                    architectureListGOps.Clear();
                    architectureListGOps = envList.ToDictionary(k => k.Id);
                }

                return architectureListGOps;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from GingerOps API", ex);
                return [];
            }
        }
        public async Task<Dictionary<string, GingerOpsEnvironmentB>> FetchApplicationDataFromGOps(string environmentId, Dictionary<string, GingerOpsEnvironmentB> environmentListGOps)
        {
            try
            {
                HttpClientHandler handler = new();
                var client = new HttpClient(handler);
                if (GingerOpsAPI.IsTokenValid())
                {
                    bearerToken = GingerOpsUserConfig.Token;
                }
                else
                {
                    bool resFlag = await GingerOpsAPI.RequestToken(GingerOpsUserConfig.ClientId, GingerOpsUserConfig.ClientSecret,
                        GingerOpsUserConfig.IdentityServiceURL);
                    if (resFlag)
                    {
                        bearerToken = GingerOpsUserConfig.Token;
                    }
                }

                string apiUrl = $"{GingerOpsUserConfig.AccountUrl}/Application/GetApplications?environmentIds={environmentId}";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);

                var response = await client.GetStringAsync(apiUrl);
                var envList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<GingerOpsAPIResponseInfo.GingerOpsEnvironmentB>>(response);

                if (envList != null && envList.Count > 0)
                {
                    environmentListGOps.Clear();
                    environmentListGOps = envList.ToDictionary(k => k.Id);
                }

                return environmentListGOps;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error fetching data from GingerOps API", ex);
                return [];
            }
        }

        private static GingerOpsConfiguration SetConfiguration()
        {
            var workSpace = WorkSpace.Instance;
            if (workSpace == null || workSpace.SolutionRepository == null)
            {
                return new GingerOpsConfiguration();
            }

            var item = WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerOpsConfiguration>();
            return item == null ? new GingerOpsConfiguration() : item;
        }
    }
}
