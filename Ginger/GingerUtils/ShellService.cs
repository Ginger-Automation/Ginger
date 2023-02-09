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
using System.Diagnostics;

namespace Ginger.Utils
{
    public class ShellService
    {
        public static string ExecuteShellCommandLinux(string commandExecutable,
               string commandArguments,
               bool standardOutput = false,
               bool standardError = false,
               bool throwOnError = false)
        {
            // This will be out return string
            string standardOutputString = string.Empty;
            string standardErrorString = string.Empty;

            // Use process
            Process process;

            Console.WriteLine("Execute Shell Command...");
            Console.WriteLine("Command: [" + commandExecutable + "]");
            Console.WriteLine("Command Parameters: [" + commandArguments + "]");

            try
            {
                // Setup our process with the executable and it's arguments
                process = new Process();

                if (string.IsNullOrEmpty(commandArguments))
                {
                    process.StartInfo = new ProcessStartInfo(commandExecutable);
                } else
                {
                    process.StartInfo = new ProcessStartInfo(commandExecutable, commandArguments);
                }

                // To get IO streams set use shell to false
                process.StartInfo.UseShellExecute = false;

                // If we want to return the output then redirect standard output
                if (standardOutput) process.StartInfo.RedirectStandardOutput = true;

                // If we std error or to throw on error then redirect error
                if (standardError || throwOnError) process.StartInfo.RedirectStandardError = true;

                // Run the process
                process.Start();

                // Get the standard error
                if (standardError || throwOnError) standardErrorString = process.StandardError.ReadToEnd();

                // If we want to throw on error and there is an error
                if (throwOnError && !string.IsNullOrEmpty(standardErrorString))
                throw new Exception(
                    string.Format("Error in ConsoleCommand while executing {0} with arguments {1}.",
                    commandExecutable, commandArguments, Environment.NewLine, standardErrorString));

                // If we want to return the output then get it
                if (standardOutput) standardOutputString = process.StandardOutput.ReadToEnd();

                // If we want standard error then append it to our output string
                if (standardError) standardOutputString += standardErrorString;

                // Wait for the process to finish
                process.WaitForExit();
            }
            catch (Exception e)
            {
                // Encapsulate and throw
                throw new Exception(
                    string.Format("Error in ConsoleCommand while executing {0} with arguments {1}.", commandExecutable, commandArguments), e);
            }

            // Return the output string
            return standardOutputString;
        }

        public static string ExecuteShellCommandWindows(string commandStr,
               string commandArguments,
               bool standardInput = false,
               bool standardOutput = false,
               bool standardError = false)
        {
            // This will be out return string
            string standardOutputString = string.Empty;
            string standardErrorString = string.Empty;

            // Use process
            Process process;

            Console.WriteLine("Execute Shell Command - Windows...");
            Console.WriteLine("Command: [" + commandStr + "]");
            Console.WriteLine("Command Parameters: [" + commandArguments + "]");

            try
            {
                // Setup our process with the executable and it's arguments
                process = new Process();

                process.StartInfo.FileName= "cmd.exe";

                // To get IO streams set use shell to false
                process.StartInfo.UseShellExecute = false;

                // To set the standard input
                if (standardInput) process.StartInfo.RedirectStandardInput = true;

                // If we want to return the output then redirect standard output
                if (standardOutput) process.StartInfo.RedirectStandardOutput = true;

                // No Window
                process.StartInfo.CreateNoWindow = true;

                // If we std error or to throw on error then redirect error
                if (standardError) process.StartInfo.RedirectStandardError = true;

                // Run the process
                process.Start();

                string commandLineStr = string.Empty;
                if (string.IsNullOrEmpty(commandArguments))
                {
                    commandLineStr = commandStr;
                }
                else
                {
                    commandLineStr = commandStr + " " + commandArguments;
                }

                process.StandardInput.WriteLine(commandLineStr);
                process.StandardInput.Flush();
                process.StandardInput.Close();

                // If we want to return the output then get it
                if (standardOutput) standardOutputString = process.StandardOutput.ReadToEnd();

                // If we want standard error then append it to our output string
                if (standardError) standardOutputString += process.StandardError.ReadToEnd();

                // Wait for the process to finish
                process.WaitForExit();
            }
            catch (Exception e)
            {
                // Encapsulate and throw
                throw new Exception(
                    string.Format("Error in ConsoleCommand while executing {0} with arguments {1}.", commandStr, commandArguments), e);
            }

            // Return the output string
            return standardOutputString;
        }

    }



}
