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
