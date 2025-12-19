#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;

using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console
{
    public class DOSConsoleDriver : ConsoleDriverBase
    {
        [UserConfigured]
        [UserConfiguredDefault(@"C:\windows\system32\cmd.exe")]  // file to open dos console
        [UserConfiguredDescription("Location of cmd.exe which open a DOS console")]
        public string CMDFileName { get; set; }

        ProcessStartInfo processStartInfo;
        System.IO.StreamWriter writer;
        Process process;
        private readonly StringBuilder mOutputsBuilder = new StringBuilder();
        private int mProcessedLength = 0;

        // Add synchronization lock
        private readonly object mOutputsLock = new object();

        public DOSConsoleDriver(BusinessFlow BusinessFlow)
        {
            this.BusinessFlow = BusinessFlow;
        }

        public override bool Connect()
        {
            RestartOutputs();
            if (CMDFileName == null || CMDFileName.Length == 0)
            {
                Reporter.ToLog(eLogLevel.WARN, "DOS Console Agent missing CMD FileName");
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

            process = Process.Start(processStartInfo);
            if (process == null)
            {
                return false;
            }
            writer = process.StandardInput;

            process.OutputDataReceived += Process_OutputDataReceivedAsync;
            process.BeginOutputReadLine();

            process.ErrorDataReceived += Process_ErrorDataReceivedAsync;
            process.BeginErrorReadLine();

            WriteOutputsToConsole();
            return true;
        }

        private void Process_ErrorDataReceivedAsync(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                lock (mOutputsLock)
                {
                    mOutputsBuilder.AppendLine(e.Data);
                }

                if (ShowWindow && Dispatcher != null)
                {
                    try
                    {
                        string dataToSend = e.Data;
                        Dispatcher.Invoke(() =>
                        {
                            OnDriverMessage(eDriverMessageType.ConsoleBufferUpdate, dataToSend);
                        });
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error updating console with error data", ex);
                    }
                }
            }
        }

        private void Process_OutputDataReceivedAsync(object sender, DataReceivedEventArgs e)
        {

            if (!string.IsNullOrEmpty(e.Data))
            {
                lock (mOutputsLock)
                {
                    mOutputsBuilder.AppendLine(e.Data);
                }

                if (ShowWindow && Dispatcher != null)
                {
                    try
                    {
                        string dataToSend = e.Data;
                        Dispatcher.Invoke(() =>
                        {
                            OnDriverMessage(eDriverMessageType.ConsoleBufferUpdate, dataToSend);
                        });
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error updating console with output data", ex);
                    }
                }
            }
        }

        public override void Disconnect()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Starting DOS Console Driver disconnect");

                // Close writer stream
                if (writer != null)
                {
                    try
                    {
                        writer.Close();
                        writer.Dispose();
                        writer = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error disposing writer stream", ex);
                    }
                }

                // Stop process data collection
                if (process != null)
                {
                    try
                    {
                        process.CancelErrorRead();
                        process.CancelOutputRead();
                        process.Close();
                        process.Dispose();
                        process = null;
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing DOS process", ex);
                    }
                }

                // Clear process configuration
                if (processStartInfo != null)
                {
                    processStartInfo.Environment?.Clear();
                    processStartInfo = null;
                }

                // Clear output buffers
                RestartOutputs();

                Reporter.ToLog(eLogLevel.INFO, "DOS Console Driver disconnected successfully");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unexpected error during disconnect", ex);
            }
        }

        public override string ConsoleWindowTitle()
        {
            return "DOS Console Driver - " + CMDFileName;
        }

        public override void CloseDriver()
        {
            try
            {
                // First disconnect from DOS process
                Disconnect();

                // Handle window closure if in windowed mode
                if (ShowWindow && Dispatcher != null)
                {
                    try
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Requesting console window closure");

                        // Signal window to close via dispatcher mechanism
                        Dispatcher.Invoke(() =>
                        {
                            OnDriverMessage(eDriverMessageType.CloseDriverWindow);
                        });

                        // Allow window time for graceful shutdown
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error signaling window closure", ex);
                    }
                }

                // Clear dispatcher reference
                Dispatcher = null;
                IsDriverConnected = false;

                // Notify status change
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);

                Reporter.ToLog(eLogLevel.INFO, "DOS Console Driver fully closed");
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in CloseDriver", ex);
            }
        }

        public override bool IsBusy()
        {
            return process != null && !process.HasExited;
        }

        public override void SendCommand(string command)
        {
            RestartOutputs();
            DateTime startingTime = DateTime.Now;

            // Send command with dispatcher support
            if (ShowWindow && Dispatcher != null)
            {
                Dispatcher.Invoke(() => writer.Write(command));
            }
            else
            {
                writer.Write(command);
            }

            if (mWait == null)
            {
                mWait = 5;
            }

            // Wait for expected output with thread safety
            if (!string.IsNullOrEmpty(mExpString) && mWait != null)
            {
                while (!CheckForExpectedString() &&
                       (DateTime.Now - startingTime).TotalSeconds <= mWait)
                {
                    if (!IsDriverRunning)
                    {
                        break;
                    }

                    Thread.Sleep(500);
                }
                WriteOutputsToConsole(false);
            }
            else
            {
                WriteOutputsToConsole();
            }
        }

        private bool CheckForExpectedString()
        {
            lock (mOutputsLock)
            {
                return mOutputsBuilder.ToString().Contains(mExpString);
            }
        }

        private void RestartOutputs()
        {
            lock (mOutputsLock)
            {
                mOutputsBuilder.Clear();
                mProcessedLength = 0;
            }
        }

        private void WriteOutputsToConsole(bool waitForRestOfOutputs = true)
        {
            Thread.Sleep(1000);

            while (true)
            {
                if (!IsDriverRunning)
                {
                    break;
                }

                string pendingOutput;
                int currentLength;

                lock (mOutputsLock)
                {
                    currentLength = mOutputsBuilder.Length;
                    if (currentLength > mProcessedLength)
                    {
                        int chunkLength = currentLength - mProcessedLength;
                        pendingOutput = mOutputsBuilder.ToString(mProcessedLength, chunkLength);
                        mProcessedLength = currentLength;
                    }
                    else
                    {
                        pendingOutput = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(pendingOutput))
                {
                    break;
                }

                if (ShowWindow && Dispatcher != null)
                {
                    string outputCopy = pendingOutput;
                    Dispatcher.Invoke(() => WriteToConsoleBuffer(outputCopy));
                }
                else
                {
                    WriteToConsoleBuffer(pendingOutput);
                }

                if (!waitForRestOfOutputs)
                {
                    break;
                }

                Thread.Sleep(500);
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
                    Reporter.ToLog(eLogLevel.DEBUG, "Unknown Console command");
                    ErrorMessageFromDriver = "Unknown Console command";
                    return "Error - unknown command";
            }
        }

        private string GetParameterizedCommand(ActConsoleCommand act)
        {
            string s = act.Command;

            foreach (ActInputValue AIV in act.InputValues)
            {
                if (s != null)
                {
                    s = s + " " + AIV.ValueForDriver;
                }
                else
                {
                    s = AIV.ValueForDriver;
                }
            }
            return s;
        }

        public override ePlatformType Platform { get { return ePlatformType.DOS; } }
    }
}
