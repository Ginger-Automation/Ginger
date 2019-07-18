using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class Telemetry 
    {
        public Guid Guid { get; set; } // keep public
        public bool DoNotCollect { get; set; }  // keep public

        TelemetrySession TelemetrySession;

        static HttpClient client;

        public delegate void TelemetryEventHandler(object sender, TelemetryEventArgs e);
    
        public class TelemetryEventArgs  : EventArgs
        {
            public string name { get; set; }
        }

        public static TelemetryEventHandler eventHandler ;

        public static void Init()
        {
            Telemetry telemetry;
            
            string fileName = Path.Combine(TelemetryFolder, "Ginger.Telemetry.Config");
            if (!File.Exists(fileName))
            {
                telemetry = new Telemetry();
                telemetry.Guid = Guid.NewGuid();
                string txt = JsonConvert.SerializeObject(telemetry);
                File.WriteAllText(fileName, txt);                                
            }
            else
            {
                string txt = File.ReadAllText(fileName);
                telemetry = JsonConvert.DeserializeObject<Telemetry>(txt);
            }
            WorkSpace.Instance.Telemetry = telemetry;
            InitClient();
            
            // run it on task so startup is not impacted
            Task.Factory.StartNew(() => {
                Thread.Sleep(10000);  // Wait 10 seconds so mainwindow and others can load
                Telemetry.CheckVersionAndNews();
            });
            
        }

        

        private static void InitClient()
        {
            client = new HttpClient();            
            client.BaseAddress = new Uri("https://" + "gingertelemetry.azurewebsites.net" );            
        }


        void ResetClient()
        {
            // Use when needed to clear headers            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.ExpectContinue = true;
        }


        static string mTelemetryFolder;
        static string TelemetryFolder
        {
            get
            {
                if (mTelemetryFolder == null)
                {
                    mTelemetryFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "amdocs", "Ginger", "Telemetry");
                    if (!Directory.Exists(mTelemetryFolder))
                    {
                        Directory.CreateDirectory(mTelemetryFolder);
                    }
                }                
                return mTelemetryFolder;
            }
        }
        


        static bool NetworkAvailable
        {
            get
            {
                bool connection = NetworkInterface.GetIsNetworkAvailable();
                return connection;                
            }
        }

        public static DateTime Time { get { return DateTime.UtcNow; }  }


        public static string VersionAndNewsInfo { get; set; }        
        
        public static void CheckVersionAndNews()
        {
            if (!NetworkAvailable) return;
            VersionAndNewsInfo = CheckLatestVersionAndNews(ApplicationInfo.ApplicationVersion).Result;
            if (eventHandler != null)
            {
                eventHandler(null, new TelemetryEventArgs() { name = "CheckVersionAndNews" });
            }
        }

        

        static async Task<string> CheckLatestVersionAndNews(string currver)
        {            
            try
            {                
                HttpResponseMessage response = await client.GetAsync("api/version/" + currver);        
                if (response.IsSuccessStatusCode)
                {
                    string gingerVersionAndNews = await response.Content.ReadAsStringAsync();
                    return gingerVersionAndNews;
                }
                else
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                
            }
            catch(Exception ex)
            {
                // TODO:
            }
            return null;
        }


        public void SessionStarted()
        {
            if (WorkSpace.Instance.Telemetry.DoNotCollect)  return;

            TelemetrySession = new TelemetrySession(Guid);            
        }


        public void SessionEnd()
        {
            if (WorkSpace.Instance.Telemetry.DoNotCollect) return;

            TelemetrySession.EndTime = Time;
            SaveTelemetry("session", Guid.ToString(), TelemetrySession);

            Task.Factory.StartNew(() => {
                Compress();
                while (!done)  
                {
                    Thread.Sleep(100);
                }
            }).Wait(30000);  // Max 30 seconds to wait
        }


        string mTelemetryDataFolder;
        string TelemetryDataFolder
        {
            get
            {
                if (mTelemetryDataFolder == null)
                {
                    mTelemetryDataFolder = Path.Combine(TelemetryFolder, "Data");
                    if (!Directory.Exists(mTelemetryDataFolder))
                    {
                        Directory.CreateDirectory(mTelemetryDataFolder);
                    }
                }
                return mTelemetryDataFolder;
            }
        }

        

        public void SaveTelemetry(string type, string id, object data)
        {
            //TODO: run on task
            // save to same file
            // lock for multi thread

            TelemetryRecord telemetryRecord = new TelemetryRecord("telemetry", type, id) ;
            
            string txt = JsonConvert.SerializeObject(telemetryRecord) + Environment.NewLine;
            txt += JsonConvert.SerializeObject(data) + Environment.NewLine; 

            string fileName = Path.Combine(TelemetryDataFolder, Guid.ToString().Replace("-","") + "_" + DateTime.UtcNow.ToString("yyyymmddhhmmss"));
            File.WriteAllText(fileName, txt);
        }


        bool done;
        private async void Compress()
        {
            try
            {
                string zipFileName = Guid.ToString().Replace("-", "") + "_" + DateTime.UtcNow.ToString("yyyymmddhhmmss") + ".Data.zip";
                string zipFolder = Path.Combine(TelemetryFolder, "Zip");
                string LocalZipfileName = Path.Combine(zipFolder, zipFileName);
                
                ZipFile.CreateFromDirectory(TelemetryDataFolder, LocalZipfileName);                

                if (File.Exists(LocalZipfileName))
                {
                    foreach (string fn in Directory.GetFiles(TelemetryDataFolder))
                    {
                        File.Delete(fn);
                    }                    
                }

                if (!NetworkAvailable) return;

                foreach (string zipfile in Directory.GetFiles(zipFolder))
                {
                    FileStream fileStream = new FileStream(Path.Combine(TelemetryFolder, zipfile), FileMode.Open);
                    StreamContent content = new StreamContent(fileStream);
                    try
                    {
                        HttpResponseMessage response = await client.PostAsync("api/Telemetry/" + zipFileName.Replace(".", "_"), content);
                        string rc = await response.Content.ReadAsStringAsync();
                        fileStream.Close();

                        if (response.IsSuccessStatusCode)
                        {
                            if (rc == "OK")
                            {
                                System.IO.File.Delete(zipfile);
                            }
                        }
                        else
                        {
                            // 
                        }
                    }
                    catch
                    {
                        // Failed to upload
                    }
                    
                }
                
            }
            catch(Exception ex)
            {
                
            }

            done = true;
        }

        


    }
}
