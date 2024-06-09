using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using amdocs.ginger.GingerCoreNET;

namespace GingerCoreNET.GenAIServices
{
    public class GenAIServiceHelper
    {

        HttpClient _httpClient;
        private string token = null;
        public GenAIServiceHelper()
        {
            InitClient();
        }

        private void InitClient()
        {
            try
            {
                _httpClient = new HttpClient();
                var host = WorkSpace.Instance.UserProfile.AskLisaConfiguration.Host;
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith("/") ? $"{host}/" : host;
                    _httpClient.BaseAddress = new Uri(host);
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

        }



        private async Task<bool> GetToken()
        {
            try
            {
                ChatBotResponseInfo responseInfo = new();
                var httpClient = new HttpClient();
                var host = WorkSpace.Instance.UserProfile.AskLisaConfiguration.AuthenticationServiceURL;
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith("/") ? $"{host}/" : host;
                    httpClient.BaseAddress = new Uri(host);
                }
                var data = new[]
                {
                new KeyValuePair<string, string>("grant_type", WorkSpace.Instance.UserProfile.AskLisaConfiguration.GrantType),
                new KeyValuePair<string, string>("client_id", WorkSpace.Instance.UserProfile.AskLisaConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret",  WorkSpace.Instance.UserProfile.AskLisaConfiguration.ClientSecret),
                };
                var response = await httpClient.PostAsync(WorkSpace.Instance.UserProfile.AskLisaConfiguration.Token, new FormUrlEncodedContent(data));
                var result = await response.Content.ReadAsAsync<dynamic>();
                responseInfo = result.ToObject<ChatBotResponseInfo>();
                token = responseInfo.AccessToken;
                if (string.IsNullOrEmpty(token))
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get the token for Chat service");
                    return false;
                }
                else
                {
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
            // return responseInfo.AccessToken;
        }

        private async Task<Boolean> GetOrValidateToken()
        {
            if (string.IsNullOrEmpty(token))
            {
                return await GetToken();
            }
            else if (IsTokenValid())
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                var response = await _httpClient.PostAsync(WorkSpace.Instance.UserProfile.AskLisaConfiguration.ContinueChat, content);
                return await ParseResponse(response);
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
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                var response = await _httpClient.PostAsync(WorkSpace.Instance.UserProfile.AskLisaConfiguration.StartNewChat, content);
                return await ParseResponse(response);
            }
            else
            {
                return null;
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
        private MultipartFormDataContent PrepareRequestDetailsForChat(string Question)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Question), "question");
            content.Add(new StringContent(WorkSpace.Instance.UserProfile.AskLisaConfiguration.Account), "account");
            content.Add(new StringContent(WorkSpace.Instance.UserProfile.AskLisaConfiguration.DomainType), "domainType");
            content.Add(new StringContent(WorkSpace.Instance.UserProfile.AskLisaConfiguration.TemperatureLevel), "temperatureVal");
            content.Add(new StringContent(WorkSpace.Instance.UserProfile.AskLisaConfiguration.MaxTokenValue), "maxTokensVal");
            content.Add(new StringContent(WorkSpace.Instance.UserProfile.AskLisaConfiguration.DataPath), "dataPath");
            return content;
        }
    }
}
