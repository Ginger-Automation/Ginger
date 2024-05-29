using JiraRepositoryStd.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class GenAIServiceData
    {
        public bool EnableChat { set; get; }
        public string Account { set; get; }
        public string DomainType { set; get; }

        public string Host { set; get; }
        public string TemperatureLevel { set; get; }
        public string MaxTokenValue { set; get; }

        public string DataPath { set; get; }

        public string StartNewChat { set; get; }

        public string ContinueChat { set; get; }

        public string GetToken { get; set; }

        public string AuthenticationServiceURL { set; get; }

        public string GrantType {set; get; }  

        public string   ClientId { set; get; }
        public string ClientSecret { set; get; }
        public GenAIServiceData()
        {

        }

    }
}

