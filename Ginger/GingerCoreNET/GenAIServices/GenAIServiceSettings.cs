using Amdocs.Ginger.Common;
using DocumentFormat.OpenXml.Wordprocessing;
using JiraRepositoryStd.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.GenAIServices
{
    public class GenAIServiceSettings
    {
        private static readonly GenAIServiceSettings _instance = new GenAIServiceSettings();
        public GenAIServiceData GenAIServiceSettingsData { set; get; }

        public static GenAIServiceSettings Instance
        {
            get
            {
                return _instance;
            }
        }

        public GenAIServiceSettings()
        {
            LoadData();
        }

        private void LoadData()
        {
            
            try
            {
                using (StreamReader r = new StreamReader("GenAIServiceSettings.json"))
                {
                   string json = r.ReadToEnd();
                   this.GenAIServiceSettingsData = JsonConvert.DeserializeObject<GenAIServiceData>(json);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "GenAIServiceSettings.json not found/unable to read", ex);
                return;
            }
        }

        //public bool IsChatEnabled()
        //{
        //    return _instance.GenAIServiceSettingsData.EnableChat;
        //}
    }
}
