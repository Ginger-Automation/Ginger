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
    public class BrainServiceSettings
    {
        private static readonly BrainServiceSettings _instance = new BrainServiceSettings();
        public BrainServiceData BrainSettingsObj { set; get; }

        public static BrainServiceSettings Instance
        {
            get
            {
                return _instance;
            }
        }

        public BrainServiceSettings()
        {
            LoadData();
        }

        private void LoadData()
        {
            
            try
            {
                //string settingsFile = "BrainApiSettings.json";
                //string jsonSettingsPath = File.ReadAllText(settingsFile);

                using (StreamReader r = new StreamReader("BrainApiSettings.json"))
                {
                   string json = r.ReadToEnd();
                   this.BrainSettingsObj = JsonConvert.DeserializeObject<BrainServiceData>(json);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "BrainApiSetting.json not found/unable to read", ex);
                return;
            }
        }
    }
}
