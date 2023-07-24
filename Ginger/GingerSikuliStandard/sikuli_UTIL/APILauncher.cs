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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Amdocs.Ginger.Common;

namespace GingerSikuliStandard.sikuli_UTIL
{
    public class APILauncher
    {
        private Process APIProcess;
        private ProcessStartInfo APIProcessStartInfo;
        public String API_Output;

        private String APIJar;
        private String WorkingDir;
        private String APIPath;
		private String JarReleaseAddress;

        public APILauncher(bool Windowless = false)
        {
            APIJar = "sikulirestapi-1.0.jar";
			JarReleaseAddress = "http://sourceforge.net/projects/sikulirestapi/files/sikulirestapi-1.0.jar/download";
            WorkingDir = Directory.GetCurrentDirectory();
            APIPath = Path.Combine(WorkingDir, APIJar);
            if (Windowless)
            {
                APIProcessStartInfo = new ProcessStartInfo("java", "-jar \"" + APIPath + "\"");
            }
            else
            {
                APIProcessStartInfo = new ProcessStartInfo("javaw", "-jar \"" + APIPath + "\"");
            }
            APIProcess = new Process();
            APIProcess.StartInfo = APIProcessStartInfo;

            if (APIProcess.Start())
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("API Path - {0} \n Proces Info - Id - {1}, Title - {2}, Start Time : {3}", APIPath, APIProcess.Id, APIProcess.MainWindowTitle, APIProcess.StartTime));
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("API Path - {0} \n Error in starting Proces", APIPath));
            }
            //Console.WriteLine("API PATH: " + APIPath);
            //Console.WriteLine("java -jar \"" + APIPath + "\"");
        }

        public void Start()
        {
			VerifyJarExists();
			Util.Log.WriteLine("Starting jetty server...");
            if (APIProcess.HasExited)
            {
                APIProcess.Start();
            }
        }

        public void Stop()
        {
			Util.Log.WriteLine("Stopping jetty server...");
            APIProcess.Kill();
			Util.Log.WriteLine("Jetty server stopped!");
			Util.Log.WriteLine("Client log for this test run can be located at: "+Util.Log.LogPath);
			Util.Log.WriteLine("Exiting...");
        }
		
		public void VerifyJarExists()
		{
			if(File.Exists(APIPath))
			{
				Util.Log.WriteLine("Jar already downloaded, launching jetty server...");
			}
			else
			{
				Util.Log.WriteLine("Jar not downloaded, downloading jetty server jar from SourceForge...");
				WebClient client = new WebClient();
				client.DownloadFile(JarReleaseAddress,APIPath);
				Util.Log.WriteLine("File downloaded!");
			}
		}
    }
}
