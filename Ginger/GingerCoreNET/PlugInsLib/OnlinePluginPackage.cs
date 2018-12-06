#region License
/*
Copyright © 2014-2018 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace Amdocs.Ginger.Repository
{
    public class OnlinePluginPackage : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }

        /// Calculated - if exist on machine: "Installed", if seelcted for solution and exits, if not exist on file system then...
        private string mStatus;
        public string Status
        {
            get
            {                
                return mStatus;
            }
            set
            {
                if (value != mStatus)
                {
                    mStatus = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        

        private ObservableList<OnlinePluginPackageRelease> mReleases;

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));         
            }
        }


        public ObservableList<OnlinePluginPackageRelease> Releases
        {
            get
            {
                if (mReleases == null)
                {
                    GetPluginReleases();
                }
                return mReleases;
            }
        }

        


        /// <summary>
        /// Get list of releases for plguin from Git        
        /// </summary>
        /// <param name="pluginRepositoryName"></param>
        /// Repositry name in Fit for example: Ginger-PACT-Plugin
        /// <returns></returns>
        void GetPluginReleases()
        {
            if (string.IsNullOrEmpty(URL))
            {
                return;
            }
            // string url = "https://api.github.com/repos/Ginger-Automation" + pluginRepositoryName +  "/releases";

            string releasesURL = URL.Replace("https://github.com/Ginger-Automation", "https://api.github.com/repos/Ginger-Automation") + "/releases";             
            string releases = GitHTTPClient.GetResponseString(releasesURL).Result;
            if (releases == "Error: Forbidden")
            {
                throw new IOException("Git API limit");
            }
            mReleases = JsonConvert.DeserializeObject<ObservableList<OnlinePluginPackageRelease>>(releases);
            
        }


        public string InstallPluginPackage(OnlinePluginPackageRelease release)
        {
            string pluginSubFolder = Path.Combine(Name, release.Version);
            string folder = DownloadPackage(release.assets[0].browser_download_url, pluginSubFolder).Result;
            return folder;
        }

        async Task<string> DownloadPackage(string url, string subfolder)
        {
            string localPluginPackageFolder = Path.Combine(PluginPackage.LocalPluginsFolder, subfolder); // Extract it to: \users\[user]\Ginger\PluginPackages/[PluginFolder]
            if (Directory.Exists(localPluginPackageFolder))
            {
                // Plugin already exist in file system no need to download
            }
            else
            {
                //TODO: show user some progress... update a shared string status
                using (var client = new HttpClient())
                {
                    var result = client.GetAsync(url).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        byte[] zipContent = await result.Content.ReadAsByteArrayAsync();  // Get the Plugin package zip content
                        string tempFileName = Path.GetTempFileName();  // temp file for the zip
                        File.WriteAllBytes(tempFileName, zipContent);  // save content to file                                                                                
                        ZipFile.ExtractToDirectory(tempFileName, localPluginPackageFolder); // Extract 
                        System.IO.File.Delete(tempFileName);

                    }
                    else
                    {
                        throw new Exception("Error downloading/installing Plugin Package: " + result.ReasonPhrase + Environment.NewLine + url);
                    }
                }
            }
            return localPluginPackageFolder;
        }


        

        

        
    }
}
