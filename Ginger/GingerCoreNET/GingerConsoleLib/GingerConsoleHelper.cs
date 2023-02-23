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
using System.IO;
using System.Reflection;

namespace Amdocs.Ginger.CoreNET.GingerConsoleLib
{
    public class GingerConsoleHelper
    {
        static string GetGingerConsoleDLL()
        {
            //TODO: cache
            
            Assembly a = Assembly.GetExecutingAssembly();
            string s = a.Location;
            string dir = Path.GetDirectoryName(s);

            string filename = Path.Combine(dir, "GingerConsole.dll");
            if (File.Exists(filename))
            {
                return filename;
            }

            // If we run from Ginger debug
            if (filename.Contains(@"Ginger\bin\Debug"))
            {
                filename = filename.Replace(@"Ginger\bin\Debug", @"GingerConsole\bin\Debug");
                filename = filename.Replace(@"\bin\Debug", @"\bin\Debug\netcoreapp2.1");

            }
            if (File.Exists(filename))
            {
                return filename;
            }

            // If we run from unit test
            if (filename.Contains("GingerCoreNETUnitTest"))
            {
                filename = filename.Replace("GingerCoreNETUnitTest", "GingerConsole");
            }
            if (File.Exists(filename))
            {
                return filename;
            }

            throw new Exception("Cannot find GingerConsole.dll");
        }

        public static System.Diagnostics.Process Execute(string script)
        {

            string GingerConsoleDLL = GetGingerConsoleDLL();
            //TODO: delete the temp file later 
            // TODO: need to add args or config file what to do
            string tempfile = Path.GetTempFileName();
            System.IO.File.WriteAllText(tempfile, script);
            string cmd = "dotnet " + GingerConsoleDLL + " " + tempfile;
            //TODO: add option to send output to file 

            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + cmd);

            // The following commands are needed to redirect the standard output.
            // This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.UseShellExecute = true; // false
            // Do not create the black window.
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            // Get the output into a string
            return proc;
        }
    }
}
