#region License
/*
Copyright © 2014-2023 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Reflection;

namespace GingerCore.Drivers.Updater
{
    public class ChromeDriverUpdater
    {
        private HttpClient client = new HttpClient();
        private static readonly string LatestVersionUrl = @"https://chromedriver.storage.googleapis.com/LATEST_RELEASE";

        public bool UpdateDriver()
        {
            try
            {
                string Version = GetLatestVersion();


                string DownloadUrl = @"https://chromedriver.storage.googleapis.com/" + Version + "/chromedriver_win32.zip";
                var response = client.GetAsync(DownloadUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    string gingerlocation = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    string chromedriverpath = Path.Combine(gingerlocation, "chromedriver.exe");
                    var chromeDriverProcesses = System.Diagnostics.Process.GetProcesses().Where(pr => pr.ProcessName == "chromedriver"); // without '.exe'
                   //killing all active chromedriver
                    foreach (var process in chromeDriverProcesses)
                    {
                        foreach (System.Diagnostics.ProcessModule module in process.Modules)
                        {
                            if (module.FileName.Equals(chromedriverpath))
                            {
                                process.Kill();
                            }
                        }

                    }


                    string zipfilepath = Path.GetTempFileName();
                    var fs = new FileStream(zipfilepath, FileMode.OpenOrCreate);
                    response.Content.CopyToAsync(fs).Wait();
                    fs.Close();
                    string temdirectory = Path.Combine(Path.GetTempPath(), DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), "Chromedriver");
                    ZipFile.ExtractToDirectory(zipfilepath, temdirectory);


                    System.IO.File.Copy(Path.Combine(temdirectory, "chromedriver.exe"), chromedriverpath, true);
                    Directory.Delete(temdirectory, true);
                    File.Delete(zipfilepath);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                //failed to update 
                return false;
            }

            return true;
        }


        private string GetLatestVersion()
        {
            return client.GetAsync(LatestVersionUrl).Result.Content.ReadAsStringAsync().Result;

        }
    }
}
