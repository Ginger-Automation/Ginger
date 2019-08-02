#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Diagnostics;
public static class ShellHelper
{

    /// <summary>
    /// Shell and create new process which runs dotent and the args
    /// </summary>
    /// <param name="args"></param>
    public static Process Dotnet(string args)
    {
        ProcessStartInfo procStartInfo = new ProcessStartInfo();
        if (GingerUtils.OperatingSystem.IsWindows())
        {
            Console.WriteLine("*** OS is Windows ***");            
            procStartInfo.FileName = "cmd";
            procStartInfo.Arguments = "/c dotnet " + args;
            procStartInfo.UseShellExecute = true;
        }
        else if (GingerUtils.OperatingSystem.IsLinux())
        {

            Console.WriteLine("*** OS is Linux ***");
            var output = ShellHelper.Bash("dotnet --version");
            Console.WriteLine("=====================================================================");
            Console.WriteLine("<<<<<<<<<<<<<<<          Shell result                    >>>>>>>>>>>>");
            Console.WriteLine(output);
            Console.WriteLine("=====================================================================");


            // cmd = "-c \"gnome-terminal -x bash -ic 'cd $HOME; dotnet " + dll + " " + nodeFileName + "'\"";  // work for Redhat
            string cmd = args = "-c \"" + "dotnet " + args + "\"";
            Console.WriteLine("Command: " + cmd);            
            procStartInfo.FileName = "/bin/bash";
            procStartInfo.Arguments = args;
            procStartInfo.UseShellExecute = false;              
            procStartInfo.CreateNoWindow = false;
            procStartInfo.RedirectStandardOutput = true;
        }
        else if (GingerUtils.OperatingSystem.IsMacOS())
        {
            Console.WriteLine("*** OS is Mac ***");
            var output = ShellHelper.Bash("dotnet --version");
            Console.WriteLine("=====================================================================");
            Console.WriteLine("<<<<<<<<<<<<<<<          Shell result                    >>>>>>>>>>>>");
            Console.WriteLine(output);
            Console.WriteLine("=====================================================================");


            // cmd = "-c \"gnome-terminal -x bash -ic 'cd $HOME; dotnet " + dll + " " + nodeFileName + "'\"";  // work for Redhat
            string cmd = args = "-c \"" + "dotnet " + args + "\"";
            Console.WriteLine("Command: " + cmd);
            procStartInfo.FileName = "/bin/bash";
            procStartInfo.Arguments = args;

            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;

            procStartInfo.RedirectStandardOutput = true;
        }

        Process process = new Process();
        process.StartInfo = procStartInfo;

        Console.WriteLine("Starting Process..");
        bool started = process.Start();
        if (started)
        {
            Console.WriteLine("Error: Not able to start Process..");
        }
        else
        {
            Console.WriteLine("Starting Process..");
        }
        
        return process;
    }



    public static string Bash(string cmd)
    {
        var escapedArgs = cmd.Replace("\"", "\\\"");

        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{escapedArgs}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }
}