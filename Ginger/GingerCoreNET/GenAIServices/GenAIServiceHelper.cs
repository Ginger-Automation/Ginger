﻿using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using amdocs.ginger.GingerCoreNET;
using GingerCore;
using GingerCore.Environments;
using Cassandra;
using Microsoft.TeamFoundation.Build.WebApi;
using Ginger.Configurations;
using Amazon.Runtime;
using Grpc.Core;

namespace GingerCoreNET.GenAIServices
{
    public class GenAIServiceHelper
    {
        ValueExpression valueExpression;
        HttpClient _httpClient;
        private string token = null;
        public GenAIServiceHelper()
        {
        }
        
        public async Task<bool> InitClient()
        {
            try
            {
                _httpClient = new HttpClient();
                valueExpression = new ValueExpression();
                var host = CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.Host);
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith('/') ? $"{host}/" : host;
                    _httpClient.BaseAddress = new Uri(host);

                    await GetOrValidateToken();
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Chat bot service end point is null or empty. pls check configuration");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Chat bot service initialization failed", ex);

            }
            return false;
        }



        private async Task<bool> GetToken()
        {
            try
            {
                ChatBotResponseInfo responseInfo = new();
                var httpClient = new HttpClient();
                var host = CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.AuthenticationServiceURL);
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith('/') ? $"{host}/" : host;
                    httpClient.BaseAddress = new Uri(host);
                }
                var data = new[]
               {
                new KeyValuePair<string, string>("grant_type", CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.GrantType)),
                new KeyValuePair<string, string>("client_id", CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.ClientId)),
                new KeyValuePair<string, string>("client_secret", CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.ClientSecret)),
                };

                var response = await httpClient.PostAsync(WorkSpace.Instance.Solution.AskLisaConfiguration.Token, new FormUrlEncodedContent(data));
                var result = await response.Content.ReadAsAsync<dynamic>();
                responseInfo = result.ToObject<ChatBotResponseInfo>();
                token = responseInfo.AccessToken;
                _httpClient.DefaultRequestHeaders.Clear();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                    return true;
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the token for Chat service, Please Check your Credentials");
                    return true;
                }
            }
            catch (Exception ex)
            {
                var error = "Failed to get access token";
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}");
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}, InnerException:{ex.InnerException},StackTrace:{ex.StackTrace}");
                return false;
            }
        }

        private async Task<Boolean> GetOrValidateToken()
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
                if (string.IsNullOrEmpty(token) || token.Split('.').Length != 3)
                {
                    return false;
                }

                DateTime validTo;
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
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
                Reporter.ToLog(eLogLevel.ERROR, "Error occured in validate token", ex);
                return false;
            }
        }

        public async Task<string> ContinueChat(string chatBotRequest)
        {
            bool tokenValid = await GetOrValidateToken();
            if (tokenValid)
            {
                MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);


                var response = await _httpClient.PostAsync(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.ContinueChat), content);
                if (response.IsSuccessStatusCode)
                {
                    return await ParseResponse(response);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public async Task<string> StartNewChat(string chatBotRequest)
        {

            bool tokenValid = await GetOrValidateToken();

            if (tokenValid)
            {
                MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
                var response = await _httpClient.PostAsync(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.StartNewChat), content);
                if (response.IsSuccessStatusCode)
                {
                    return await ParseResponse(response);
                }
                else
                {
                    return null;
                }
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
        private MultipartFormDataContent PrepareRequestDetailsForChat(string Question)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Question), "question");
            content.Add(new StringContent(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.Account)), "account");
            content.Add(new StringContent(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.DomainType)), "domainType");
            content.Add(new StringContent(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.TemperatureLevel)), "temperatureVal");
            content.Add(new StringContent(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.MaxTokenValue)), "maxTokensVal");
            content.Add(new StringContent(CredentialsCalculation(WorkSpace.Instance.Solution.AskLisaConfiguration.DataPath)), "dataPath");
            return content;
        }
    }
}
