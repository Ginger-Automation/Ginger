using JiraRepositoryStd.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class BrainServiceData
    {
        public string account { set; get; }
        public string domainType { set; get; }

        public string BrainHost { set; get; }
        public string temperatureVal { set; get; }
        public string maxTokensVal { set; get; }

        public string dataPath { set; get; }

        public string START_NEW_CHAT { set; get; }

        public string CONTINUE_CHAT { set; get; }

        public BrainServiceData()
        {

        }

    }
}

