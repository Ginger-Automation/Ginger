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
        System.Diagnostics.ProcessStartInfo procStartInfo = null;
        if (GingerUtils.OperatingSystem.IsWindows())
        {
            Console.WriteLine("*** OS is Windows ***");
            procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + args);
            procStartInfo.UseShellExecute = true;
        }
        else if (GingerUtils.OperatingSystem.IsLinux())
        {
            Console.WriteLine("*** OS is Linux ***");
            var output = ShellHelper.Bash(args);
            Console.WriteLine("=====================================================================");
            Console.WriteLine("<<<<<<<<<<<<<<<          Shell result                    >>>>>>>>>>>>");
            
            Console.WriteLine(output);
            Console.WriteLine("=====================================================================");


            // cmd = "-c \"gnome-terminal -x bash -ic 'cd $HOME; dotnet " + dll + " " + nodeFileName + "'\"";  // work for Redhat
            string cmd = args = "-c \"'dotnet " + args;  
            Console.WriteLine("Command: " + args);
            procStartInfo = new System.Diagnostics.ProcessStartInfo();
            procStartInfo.FileName = "/bin/bash";
            procStartInfo.Arguments = args;

            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = false;
            procStartInfo.RedirectStandardOutput = true;
        }

        Process process = new System.Diagnostics.Process();
        process.StartInfo = procStartInfo;

        Console.WriteLine("Starting Process..");
        process.Start();
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