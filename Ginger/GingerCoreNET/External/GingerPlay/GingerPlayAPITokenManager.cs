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
using GingerCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.External.GingerPlay
{
    public class GingerPlayAPITokenManager
    {
        HttpClient _httpClient;
        GingerCore.ValueExpression valueExpression;
        private GingerPlayConfiguration GingerPlayConfiguration;

        private async Task<bool> GetToken()
        {
            try
            {
                GingerPlayAPITokenResponseInfo responseInfo;
                valueExpression = new();

                _httpClient = new HttpClient();
                var host = CredentialsCalculation(GingerPlayEndPointManager.GetGenerateTokenUrl());
                if (!string.IsNullOrEmpty(host))
                {
                    _httpClient.BaseAddress = new Uri(host);
                }
                var data = new[]
               {
                new KeyValuePair<string, string>("grant_type", CredentialsCalculation(GingerPlayConfiguration.GrantType)),
                new KeyValuePair<string, string>("client_id", CredentialsCalculation(GingerPlayConfiguration.GingerPlayClientId)),
                new KeyValuePair<string, string>("client_secret", CredentialsCalculation(GingerPlayConfiguration.GingerPlayClientSecret)),
                };

                HttpResponseMessage response = await _httpClient.PostAsync("", new FormUrlEncodedContent(data));
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<dynamic>();
                    responseInfo = result.ToObject<GingerPlayAPITokenResponseInfo>();
                    GingerPlayConfiguration.Token = responseInfo.access_token;
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the token for GingerPlay service, Please Check your Credentials");
                    return false;
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to get access token";
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}");
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}, InnerException:{ex.InnerException},StackTrace:{ex.StackTrace}");
                return false;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.Dispose();
            }
        }

        public async Task<Boolean> GetOrValidateToken()
        {
            if (IsTokenValid())
            {
                return true;
            }
            else
            {
                return await GetToken();
            }
        }

        private bool IsTokenValid()
        {
            try
            {
                GingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0
      ? new GingerPlayConfiguration()
      : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
                if (string.IsNullOrEmpty(GingerPlayConfiguration.Token) || GingerPlayConfiguration.Token.Split('.').Length != 3)
                {
                    return false;
                }

                DateTime validTo;
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(GingerPlayConfiguration.Token);
                validTo = jwtToken.ValidTo;
                if (DateTime.UtcNow <= validTo)
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
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred in validate token", ex);
                return false;
            }
        }

        public async Task<bool> IsServiceHealthyAsync(string healthUrl)
        {
            try
            {
                using var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(healthUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var status = doc.RootElement.GetProperty("status").GetString();
                return string.Equals(status, "Healthy", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static async Task<string> ParseResponse(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(result))
            {
                return result;
            }
            else
            {
                return null;
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

            if (GingerCore.ValueExpression.IsThisAValueExpression(value))
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

        public string GetValidToken()
        {
            if (GingerPlayConfiguration == null)
                    {
                GingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0
                            ? new GingerPlayConfiguration()
                            : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
                    }
                return  GingerPlayConfiguration?.Token ?? string.Empty;
        }
    }
}
