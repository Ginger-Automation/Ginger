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
    public class BrainAIServices
    {

        HttpClient _httpClient;
        readonly BrainServiceSettings _settings;
        private string token = null;
        public BrainAIServices()
        {
            _settings = new BrainServiceSettings();
            InitClient();
        }

        private void InitClient()
        {
            _httpClient = new HttpClient();
            var host = _settings.BrainSettingsObj.BrainHost;
            if (!string.IsNullOrEmpty(host))
            {
                host = !host.EndsWith("/") ? $"{host}/" : host;
                _httpClient.BaseAddress = new Uri(host);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Brain end point is null or empty. pls check configuration");
            }

        }

        private async Task<Boolean> GetOrVerifyToken()
        {
            if (string.IsNullOrEmpty(token))
            {
                token = await GetToken();
                if (!string.IsNullOrEmpty(token))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (IsTokenValid())
            {
                return true;
            }
            
          return false;
            
        }

        public bool IsTokenValid()
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

        public async Task<string> ContinueChat(string chatBotRequest)
        {
            bool tokenValid = await GetOrVerifyToken();
            if (tokenValid)
            {
                MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                var response = await _httpClient.PostAsync(_settings.BrainSettingsObj.CONTINUE_CHAT, content);
                return await ParseResponse(response);
            }
            else
            {
                return null;
            }
        }

        public async Task<string> StartNewChat(string chatBotRequest)
        {

           bool tokenValid= await GetOrVerifyToken();

            if (tokenValid)
            {
                MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", string.Format($"Bearer {token}"));
                var response = await _httpClient.PostAsync(_settings.BrainSettingsObj.START_NEW_CHAT, content);
                return await ParseResponse(response);
            }
            else
            {
                return null;
            }
        }

        public async Task<string> GetToken()
        {
            ChatBotResponseInfo responseInfo = new();
            try
            {
                var httpClient = new HttpClient();
                var host = _settings.BrainSettingsObj.AuthenticationServiceURL;
                if (!string.IsNullOrEmpty(host))
                {
                    host = !host.EndsWith("/") ? $"{host}/" : host;
                    httpClient.BaseAddress = new Uri(host);
                }
                var data = new[]
                {
                new KeyValuePair<string, string>("grant_type", _settings.BrainSettingsObj.GrantType),
                new KeyValuePair<string, string>("client_id", _settings.BrainSettingsObj.Client_Id),
                new KeyValuePair<string, string>("client_secret",  _settings.BrainSettingsObj.Client_Secret),
                };
                var response = await httpClient.PostAsync(_settings.BrainSettingsObj.GET_TOKEN, new FormUrlEncodedContent(data));
                var result = await response.Content.ReadAsAsync<dynamic>();
                responseInfo = result.ToObject<ChatBotResponseInfo>();
            }
            catch (Exception ex)
            {
                var error = "Failed to get access token";
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}");
                Reporter.ToLog(eLogLevel.ERROR, $"{error}, Error :{ex.Message}, InnerException:{ex.InnerException},StackTrace:{ex.StackTrace}");
                return null;
            }
            return responseInfo.AccessToken;
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
            content.Add(new StringContent(_settings.BrainSettingsObj.account), "account");
            content.Add(new StringContent(_settings.BrainSettingsObj.domainType), "domainType");
            content.Add(new StringContent(_settings.BrainSettingsObj.temperatureVal), "temperatureVal");
            content.Add(new StringContent(_settings.BrainSettingsObj.maxTokensVal), "maxTokensVal");
            content.Add(new StringContent(_settings.BrainSettingsObj.dataPath), "dataPath");
            return content;
        }
    }
}
