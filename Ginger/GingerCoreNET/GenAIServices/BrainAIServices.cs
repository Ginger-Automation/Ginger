using Amdocs.Ginger.Common;
using DocumentFormat.OpenXml.Office2013.Excel;
using Pb;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class BrainAIServices
    {

        HttpClient _httpClient;
        readonly BrainServiceSettings _settings;
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

        public async Task<string> ContinueChat(string chatBotRequest)
        {
            MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
            var response = await _httpClient.PostAsync(_settings.BrainSettingsObj.CONTINUE_CHAT, content);
            return await ParseResponse(response);
        }

        public async Task<string> StartNewChat(string chatBotRequest)
        {
            MultipartFormDataContent content = PrepareRequestDetailsForChat(chatBotRequest);
            var response = await _httpClient.PostAsync(_settings.BrainSettingsObj.START_NEW_CHAT, content);
            return await ParseResponse(response);
        }

        private static async Task<string> ParseResponse(HttpResponseMessage response)
        {
            var result = await response.Content.ReadAsAsync<dynamic>();
            ChatBotResponseInfo responseInfo = JsonSerializer.Deserialize<ChatBotResponseInfo>(result);

            if (responseInfo != null)
            {
                return responseInfo.message;
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
