#region License
/*
Copyright © 2014-2022 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using DocumentFormat.OpenXml.Wordprocessing;
using Ginger.Reports;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Variables;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using static GingerCoreNET.SourceControl.SourceControlBase;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class Telemetry : ITelemetry
    {
        public Guid Guid { get; set; } // keep public
        public bool DoNotCollect { get; set; } = false; // keep public, default is do not collect

        public TelemetrySession TelemetrySession { get; set; }

        public ITelemetry GetTelemetry
        {
            get
            {
                return this;
            }
        }

        public ITelemetry SetTelemetry
        {
            set
            {
                this = value;
            }
        }

        static HttpClient mClient;

        public delegate void TelemetryEventHandler(object sender, TelemetryEventArgs e);


        string mSessionFileName;
        string sessionFileName
        {
            get
            {
                if 
                    (mSessionFileName == null)
                {
                    mSessionFileName = Path.Combine(TelemetryDataFolder, Guid.ToString().Replace("-", "") + "_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss"));
                }                
                return mSessionFileName;
            }
        }
    
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
            
            //// run it on task so startup is not impacted
            //Task.Factory.StartNew(() => {
            //    Thread.Sleep(10000);  // Wait 10 seconds so main window and others can load
            //    Telemetry.CheckVersionAndNews();
            //});
            
            StartProcessing();
            
        }
        
        private static void StartProcessing()
        {
            // start a long running task to process telemetry queue
            Task task = new Task(() => WorkSpace.Instance.Telemetry.ProcessQueue(),                     
                    TaskCreationOptions.LongRunning);
            task.Start();               
        }

        private static void InitClient()
        {
            mClient = new HttpClient();            
            mClient.BaseAddress = new Uri("https://" + "gingertelemetry.azurewebsites.net" );            
        }


        void ResetClient()
        {
            // Use when needed to clear headers            
            mClient.DefaultRequestHeaders.Accept.Clear();
            mClient.DefaultRequestHeaders.ExpectContinue = true;
        }


        static string mTelemetryFolder;
        static string TelemetryFolder
        {
            get
            {
                if (mTelemetryFolder == null)
                {                    
                    mTelemetryFolder = Path.Combine(General.LocalUserApplicationDataFolderPath, "Telemetry");
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
                HttpResponseMessage response = await mClient.GetAsync("api/version/" + currver);        
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
                Console.WriteLine("Telemetry Ex: " + ex.Message);
            }
            return null;
        }


        public void SessionStarted()
        {
            TelemetrySession = new TelemetrySession(Guid);
            if (WorkSpace.Instance.Telemetry.DoNotCollect)  return;
        }

        public void CheckSolutionInstanceForUserData()
        {
            //bool isLogger = Enum.TryParse(WorkSpace.Instance.Solution.LoggerConfigurations.SelectedDataRepositoryMethod.ToString(), out TelemetrySession.GingerUsedFeatures selectedLogger);
            //if (isLogger)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(selectedLogger.ToString(), true, true);
            //}

            //if (WorkSpace.Instance.Solution.LoggerConfigurations.PublishLogToCentralDB == ePublishToCentralDB.Yes)
            //{
            //    bool isCentralizedDB = Enum.TryParse(WorkSpace.Instance.Solution.LoggerConfigurations.DataPublishingPhase.ToString(), out TelemetrySession.GingerUsedFeatures publishPhase);
            //    if (isCentralizedDB)
            //    {
            //        UsedFeatureDetail.AddOrModifyFeatureDetail(publishPhase.ToString(), true, true);
            //    }
            //}

            if (WorkSpace.Instance.Solution.SourceControl != null && WorkSpace.Instance.Solution.SourceControl.GetSourceControlType != eSourceControlType.None)
            {
                UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.SourceControl.ToString(), true, true);
            }

            #region Remove Code

            //if (WorkSpace.Instance.Solution.Tags.Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.Tags.ToString(), true, false);
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Count > 1)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.DataSource.ToString(), true, false);
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Count > 1)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.Environments.ToString(), true, false);
            //}

            //ObservableList<PluginPackage> pluginPackages = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            //if (pluginPackages.Count > 0)
            //{
            //    foreach(PluginPackage plugin in pluginPackages)
            //    {
            //        UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.Plugins.ToString(), true, false, plugin.ItemName);
            //    }
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.ModelParameters.ToString(), true, false);
            //}

            //ObservableList<ApplicationPOMModel> pomModels = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>();
            //if (pomModels.Count > 0)
            //{
            //    foreach (ApplicationPOMModel pomModel in pomModels)
            //    {
            //        var appPlatform = WorkSpace.Instance.Solution.GetTargetApplicationPlatform(pomModel.TargetApplicationKey);
            //        pomModel.SetItemImageType(appPlatform);
            //        UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.POM.ToString(), true, false, pomModel.ItemImageType.ToString());
            //    }
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationAPIModel>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.ApiModel.ToString(), true, false);
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Count > 1)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.CustomizedReportTemplates.ToString(), true, false);
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Where(x => x.MappedOutputType == VariableBase.eOutputType.GlobalVariable).ToList().Count > 1)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.GlobalVaraibles.ToString(), true, false);
            //}

            //if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.SharedRepository.ToString(), true, false);

            //}
            //else if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.SharedRepository.ToString(), true, false);
            //}
            //else if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Act>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.SharedRepository.ToString(), true, false);
            //}
            //else if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<VariableBase>().Count > 0)
            //{
            //    UsedFeatureDetail.AddOrModifyFeatureDetail(TelemetrySession.GingerUsedFeatures.SharedRepository.ToString(), true, false);
            //}

            #endregion

            if (WorkSpace.Instance.UserProfile != null)
            {
                if (!string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.UserType.ToString()))
                {
                    TelemetrySession.UserType = WorkSpace.Instance.UserProfile.UserType.ToString();
                }
                if (!string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.UserRole.ToString()))
                {
                    TelemetrySession.UserRole = WorkSpace.Instance.UserProfile.UserRole.ToString();
                }
                if (!string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.TerminologyDictionaryType.ToString()))
                {
                    TelemetrySession.Terminology = WorkSpace.Instance.UserProfile.TerminologyDictionaryType.ToString();

                }
            }
        }

        public void SessionEnd()
        {
            if (WorkSpace.Instance.Telemetry.DoNotCollect) return;
            TelemetrySession.EndTime = Time;
            TimeSpan ts = TelemetrySession.EndTime - TelemetrySession.StartTime;
            TelemetrySession.OverallSessionTime = ts.ToString(@"hh\:mm\:ss");

            TimeSpan sp = TimeSpan.FromMilliseconds((double)TelemetrySession.OverallExecutionTimeNumber);
            TelemetrySession.OverallExecutionTime = sp.ToString(@"hh\:mm\:ss");

            Add("sessionend", TelemetrySession);

            TelemetryRecords.CompleteAdding();
            
            
            Task.Factory.StartNew(() => {
                // TODO: add timeout to wait
                while(!TelemetryRecords.IsCompleted) // Wait for all records to process
                {
                    Thread.Sleep(100);
                }

                //Compress();
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


        // Multi thread safe
        BlockingCollection<object> TelemetryRecords = new BlockingCollection<object>();
        public void Add(string entityType, object data)
        {                      
            TelemetryRecord telemetryRecord = new TelemetryRecord(entityType, data) ;
            TelemetryRecords.Add(telemetryRecord);                                    
        }


        public void ProcessQueue()
        {
            foreach (TelemetryRecord telemetryRecord in TelemetryRecords.GetConsumingEnumerable())
            {                                
                string objJSON = JsonConvert.SerializeObject(telemetryRecord.getTelemetry(), Formatting.Indented, new JsonSerializerSettings
                {
                    DefaultValueHandling = DefaultValueHandling.Ignore
                });

                File.AppendAllText(sessionFileName, objJSON);                             
            }
        }

        public void AddException(Exception ex)
        {            
            Add("Exception", new { message = ex.Message, StackTrace = ex.StackTrace });
        }
        
        bool done;
        private async void Compress()
        {
            try
            {                
                string zipFileName = Guid.ToString().Replace("-", "") + "_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "_Data_zip";
                string zipFolder = Path.Combine(TelemetryFolder, "Zip");

                if (!Directory.Exists(zipFolder))
                {
                    Directory.CreateDirectory(zipFolder);
                }

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


                // TODO; run in parallel
                foreach (string zipfile in Directory.GetFiles(zipFolder))
                {
                    FileStream fileStream = new FileStream(Path.Combine(TelemetryFolder, zipfile), FileMode.Open);
                    StreamContent content = new StreamContent(fileStream);
                    try
                    {
                        HttpResponseMessage response = await mClient.PostAsync("api/Telemetry/" + zipFileName.Replace(".", "_"), content);
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
                Console.WriteLine("Telemetry Ex: " + ex.Message);
            }

            done = true;
        }

        


    }
}
