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
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    public class DOSConsoleDriver : ConsoleDriverBase
    {
        [UserConfigured]
        [UserConfiguredDefault(@"C:\windows\system32\cmd.exe")]  // file to open dos console
        [UserConfiguredDescription("Location of cmd.exe which open a DOS console")]
        public string CMDFileName {get; set;}

        ProcessStartInfo processStartInfo;
        System.IO.StreamReader reader;
        System.IO.StreamWriter writer;
        System.IO.StreamReader errorReader;
        
        string mOutputs = string.Empty;
        int mlastOutputsLength = 0;

        public DOSConsoleDriver(BusinessFlow BusinessFlow)
        {
            this.BusinessFlow = BusinessFlow;
        }

        public override bool Connect()
        {
            RestartOutputs();
            if (CMDFileName == null || CMDFileName.Length == 0)
            {               
                Reporter.ToLog(eAppReporterLogLevel.INFO, "DOS Console Agent missing CMD FileName");
                ErrorMessageFromDriver = "DOS Console Agent missing CMD FileName";
                return false;
            }

            processStartInfo = new ProcessStartInfo
            {                
                //TODO: driver config
                FileName = CMDFileName,             
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                UseShellExecute = false, 
                CreateNoWindow = true,
                ErrorDialog = false
            };

            var process = Process.Start(processStartInfo);
            if (process == null)
            {
                return false;
            }

            reader = process.StandardOutput;
            writer = process.StandardInput;
            errorReader = process.StandardError;

            Task mOutputReaderTask = ReadConsoleOutputAsync();
            Task mErrorReaderTask = ReadConsoleErrorAsync();
            WriteOutputsToConsole();
            return true;
        }

        public override void Disconnect()
        {
            // TODO: kill processStartInfo.
        }

        public override string ConsoleWindowTitle()
        {
            return "DOS Console Driver - " + CMDFileName;
        }        

        public override bool IsBusy()
        {
            if (reader.EndOfStream)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task ReadConsoleOutputAsync()
        {
            while (true)
            {
                string s = await reader.ReadLineAsync();
                mOutputs += s + System.Environment.NewLine;
            }            
        }

        private async Task ReadConsoleErrorAsync()
        {
            while (true)
            {
                string s = await errorReader.ReadLineAsync();
                mOutputs += s + System.Environment.NewLine;
            }
        }

        public override void SendCommand(string command)
        {
            RestartOutputs();
            DateTime startingTime = DateTime.Now;
            
            writer.Write(command);

            if (mWait == null)
                mWait = 5;

            //waiting for expected output
            if (string.IsNullOrEmpty(mExpString) == false && mWait != null)
            {
                while (mOutputs.Contains(mExpString) == false && ((DateTime.Now - startingTime).TotalSeconds <= mWait))
                {
                    if (!mConsoleDriverWindow.mConsoleDriver.IsDriverRunning) break;
                    Thread.Sleep(500);
                }                                            
                WriteOutputsToConsole(false);                
            }
            else
                WriteOutputsToConsole();            
        }

        private void RestartOutputs()
        {
            mOutputs = string.Empty;
            mlastOutputsLength = 0;
        }

        private void WriteOutputsToConsole(bool waitForRestOfOutputs=true)
        {
            Thread.Sleep(1000);
            while (mOutputs.Length > mlastOutputsLength)
            {
                if (!mConsoleDriverWindow.mConsoleDriver.IsDriverRunning) break;
                mConsoleDriverWindow.ConsoleWriteText(mOutputs.Substring(mlastOutputsLength, (mOutputs.Length - mlastOutputsLength)));//get the output addition to console

                if (waitForRestOfOutputs)
                {
                    mlastOutputsLength = mOutputs.Length;
                    Thread.Sleep(500);
                }
                else
                {
                    return;
                }
            }
        }
        
        public override string GetCommandText(ActConsoleCommand act)
        {
            //TODO: Enhance Handling for commands copyfile, dir
            switch (act.ConsoleCommand)
            {
                case ActConsoleCommand.eConsoleCommand.FreeCommand:
                    return GetParameterizedCommand(act);
                case ActConsoleCommand.eConsoleCommand.CopyFile:
                    return "copy " + GetParameterizedCommand(act);
                case ActConsoleCommand.eConsoleCommand.IsFileExist:      
                    return "dir " + GetParameterizedCommand(act);                   
                default:                    
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "Unkonwn Console command");
                    ErrorMessageFromDriver = "Unkonwn Console command";
                    return "Error - unknown command";                    
            }
        }

        private string GetParameterizedCommand(ActConsoleCommand act)
        {
            string s = act.Command;
                        
            foreach (ActInputValue AIV in act.InputValues)
            {
                if (s != null)
                    s = s + " " + AIV.ValueForDriver;
                else
                    s = AIV.ValueForDriver;
            }
            return s;
        }

        public override ePlatformType Platform { get { return ePlatformType.DOS; } }
    }
}
