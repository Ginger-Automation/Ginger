using Amdocs.Ginger.Common;
using DocumentFormat.OpenXml.Office2013.Excel;
using Pb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class GenAIServiceHelper
    {

        HttpClient _httpClient;
        readonly GenAIServiceSettings _settings;
        private string token = null;
        public GenAIServiceHelper()
        {
            _settings = new GenAIServiceSettings();
            InitClient();
        }

        private void InitClient()
        {
            try
            {
                _httpClient = new HttpClient();
                var host = _settings.GenAIServiceSettingsData.Host;
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
                var host = _settings.GenAIServiceSettingsData.AuthenticationServiceURL;
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith("/") ? $"{host}/" : host;
                    httpClient.BaseAddress = new Uri(host);
                }
                var data = new[]
                {
                new KeyValuePair<string, string>("grant_type", _settings.GenAIServiceSettingsData.GrantType),
                new KeyValuePair<string, string>("client_id", _settings.GenAIServiceSettingsData.ClientId),
                new KeyValuePair<string, string>("client_secret",  _settings.GenAIServiceSettingsData.ClientSecret),
                };
                var response = await httpClient.PostAsync(_settings.GenAIServiceSettingsData.GetToken, new FormUrlEncodedContent(data));
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
                Reporter.ToLog(eLogLevel.ERROR,"Error occured in validate token", ex);
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
                var response = await _httpClient.PostAsync(_settings.GenAIServiceSettingsData.ContinueChat, content);
                return await ParseResponse(response);
            }
            else
            {
                return null;
            }
        }

        public async Task<string> StartNewChat(string chatBotRequest)
        {

           bool tokenValid= await GetOrValidateToken();

            if (tokenValid)
            {
                MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                var response = await _httpClient.PostAsync(_settings.GenAIServiceSettingsData.StartNewChat, content);
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
            if(!string.IsNullOrEmpty(result))
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
            content.Add(new StringContent(_settings.GenAIServiceSettingsData.Account), "account");
            content.Add(new StringContent(_settings.GenAIServiceSettingsData.DomainType), "domainType");
            content.Add(new StringContent(_settings.GenAIServiceSettingsData.TemperatureLevel), "temperatureVal");
            content.Add(new StringContent(_settings.GenAIServiceSettingsData.MaxTokenValue), "maxTokensVal");
            content.Add(new StringContent(_settings.GenAIServiceSettingsData.DataPath), "dataPath");
            return content;
        }
    }
}
