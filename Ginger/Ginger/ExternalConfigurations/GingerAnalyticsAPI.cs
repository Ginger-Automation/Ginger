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

namespace Ginger.ExternalConfigurations
{
    public class GingerAnalyticsAPI
    {
        public static DateTime validTo = DateTime.MinValue;
        public static GingerAnalyticsConfiguration gingerAnalyticsUserConfig =
            WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerAnalyticsConfiguration>().Count == 0 ? new GingerAnalyticsConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerAnalyticsConfiguration>();
       

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
    }
}
