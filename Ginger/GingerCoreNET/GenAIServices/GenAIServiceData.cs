using JiraRepositoryStd.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNET.GenAIServices
{
    public class GenAIServiceData
    {
        public bool EnableChat { set; get; }
        public string Account { set; get; }
        public string DomainType { set; get; }

        public string Host { set; get; }
        public string TemperatureLevel = "0.1";
        public string MaxTokenValue = "2000";

        public string DataPath { set; get; }

        public string StartNewChat = "AQEQABot/Lisa/StartNewChat";

        public string ContinueChat = "AQEQABot/Lisa/ContinueChat";

        public string Token { get; set; }

        public string AuthenticationServiceURL = "https://ilrnaaqua03/identityserverms/";

        public string GrantType = "client_credentials";

        public string ClientId = "BrAIn";
        public string ClientSecret = "AQEBrAIn_secret";
        public GenAIServiceData()
        {

        }

    }
}

