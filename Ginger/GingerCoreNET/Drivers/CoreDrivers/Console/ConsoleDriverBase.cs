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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using static Amdocs.Ginger.CoreNET.ActionsLib.ActConsoleCommand;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow;
using GingerCore;
using GingerCore.Drivers;
using GingerValueExpression = GingerCore.ValueExpression;

namespace Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Console
{
    /// <summary>
    /// Dummy dispatcher implementation for headless console operations
    /// </summary>
    public class DummyDispatcher : IDispatcher
    {
        public void Invoke(Action callback)
        {
            // Execute the callback directly on the current thread
            callback?.Invoke();
        }

        public void BeginInvokeShutdown(dynamic dispatcherPriority)
        {
            // No-op for dummy dispatcher
        }
    }

    public abstract class ConsoleDriverBase : DriverBase, IVirtualDriver, IDriverWindow
    {
        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Implicit Wait for Console Action Completion")]
        public int ImplicitWait { get; set; }

        // Allow user to control if console window should be displayed (similar to mobile LoadDeviceWindow)
        [UserConfigured]
        [UserConfiguredDefault("true")]
        [UserConfiguredDescription("Determine if the console driver window will be loaded with the Agent")]
        public bool LoadConsoleWindow { get; set; } = true;

        public bool ShowWindow => LoadConsoleWindow;   // IDriverWindow implementation

        public string GetDriverWindowName(Agent.eDriverType driverSubType = Agent.eDriverType.NA)
        {
            // Window moved from GingerCore to Ginger project under Drivers/DriverWindow
            return "ConsoleDriverWindow";
        }

        public bool taskFinished = false;
        
        // Console buffer to capture command output when running without UI
        private StringBuilder mConsoleBuffer = new StringBuilder();

        // Recording buffer support (StartRecordingBuffer/StopRecordingBuffer/ReturnBufferContent)
        private bool mIsRecordingBufferActive = false;
        private StringBuilder mRecordingBuffer = new StringBuilder();
        
        public override bool IsSTAThread()
        {
            return true;
        }

        public abstract bool Connect();
        public abstract void Disconnect();
        public abstract bool IsBusy();
        public bool IsDriverConnected = false;

        protected int? mWait;
        protected string mExpString;
        // Derived class need to return the window title
        public abstract string ConsoleWindowTitle();

        // Send the command to the unix, dos etc...
        public abstract void SendCommand(string command);

        // Get the action command to be send to the specific derived driver
        public abstract string GetCommandText(ActConsoleCommand act);

        public override void StartDriver()
        {
            // Connect on current thread; DriverWindowHandler will open the window asynchronously if ShowWindow == true
            IsDriverConnected = Connect();

            // In headless mode (ShowWindow == false), provide dummy dispatcher so actions relying on dispatcher still work
            if (!ShowWindow)
            {
                Dispatcher = new DummyDispatcher();
            }
            else
            {
                // When ShowWindow == true, initialize with DummyDispatcher temporarily
                // The real dispatcher will be set by DriverWindowHandler when the window is created
                if (Dispatcher == null)
                {
                    Dispatcher = new DummyDispatcher();
                }
            }

            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        // Legacy method retained for backward compatibility (not used by new flow)
        [Obsolete("Use StartDriver() instead. This method will be removed in a future version.")]
        public void ShowDriverWindow()
        {
            IsDriverConnected = Connect();
            if (!ShowWindow)
            {
                Dispatcher = new DummyDispatcher();
            }
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        public override void CloseDriver()
        {
            try
            {
                Disconnect();
                // Close driver window if it exists and ShowWindow is enabled
                if (ShowWindow && Dispatcher != null)
                {
                    try
                    {
                        // Signal window to close via dispatcher
                        Dispatcher.Invoke(() =>
                        {
                            OnDriverMessage(eDriverMessageType.CloseDriverWindow);
                        });

                        // Give window time to close gracefully
                        Thread.Sleep(500);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Error closing driver window from driver", ex);
                    }
                }

                // Clean up dispatcher
                if (Dispatcher != null)
                {
                    if (Dispatcher is DummyDispatcher)
                    {
                        // DummyDispatcher doesn't need cleanup
                        Dispatcher = null;
                    }
                    else
                    {
                        // Real dispatcher - allow window to handle cleanup
                        Dispatcher = null;
                    }
                }

                IsDriverConnected = false;
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);

            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error when try to close Console Driver - " + ex.Message, ex);
            }
            IsDriverConnected = false;
            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        public override Act GetCurrentElement()
        {
            return null;
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<eCommandEndKey, string> s_cmdMap = new();
        public static string GetCommandValue(eCommandEndKey key)
        {
           return s_cmdMap.GetOrAdd(key, k =>
            {
                var type = typeof(eCommandEndKey);
                var members = type.GetMember(k.ToString());
                if (members == null || members.Length == 0) 
                { 
                    return string.Empty; 
                }
                var attrs = members[0].GetCustomAttributes(typeof(CommandValueAttribute), false);
                return attrs.Length > 0 ? ((CommandValueAttribute)attrs[0]).Value : string.Empty;
            });
        }


        public override void RunAction(Act act)
        {
            var actClass = act.GetType().ToString();
            actClass = actClass.Replace("Amdocs.Ginger.CoreNET.ActionsLib.", "");
            switch (actClass)
            {
                case "ActConsoleCommand":
                    var ACC = (ActConsoleCommand)act;

                    bool skipExecution = false; // will be set for buffer control commands

                    // Switch on the actual console command (no early returns)
                    switch (ACC.ConsoleCommand)
                    {
                        case eConsoleCommand.StartRecordingConsoleLogs:
                            mRecordingBuffer.Clear();
                            mIsRecordingBufferActive = true;
                            act.AddOrUpdateReturnParamActual("RecordingConsoleLogsStatus", "Started");
                            act.ExInfo = "Console output recording started";
                            skipExecution = true;
                            break;

                        case eConsoleCommand.StopRecordingConsoleLogs:
                            mIsRecordingBufferActive = false;
                            act.AddOrUpdateReturnParamActual("RecordingConsoleLogsStatus", "Stopped");
                            act.ExInfo = "Console output recording stopped";
                            skipExecution = true;
                            break;

                        case eConsoleCommand.GetRecordedConsoleLogs:
                            act.AddOrUpdateReturnParamActual("Result", mRecordingBuffer.ToString());
                            act.ExInfo = "Returned recorded console buffer content";
                            skipExecution = true;
                            break;
                    }

                    if (skipExecution)
                    {
                        // Do not execute the generic command send/parse logic
                        break;
                    }

                    mWait = ACC.WaitTime;

                    var VE = new GingerValueExpression(Environment, BusinessFlow);
                    var ExpString = ACC.ExpString;
                    VE.Value = ExpString;
                    mExpString = VE.ValueCalculated;

                    var command = GetCommandText(ACC);
                    act.ExInfo = command;
                    taskFinished = false;
                    if (command.StartsWith("GINGER_RC="))
                    {
                        act.AddOrUpdateReturnParamActual("GINGER_RC", command.Replace("GINGER_RC=", ""));
                    }
                    else
                    {
                        string sRC;
                        if (ACC.ConsoleCommand == eConsoleCommand.Script)
                        {
                            sRC = RunConsoleCommand(command, "~~~GINGER_RC_END~~~");
                        }
                        else
                        {
                            if (Platform == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Unix)
                            {
                                command = $"{command}{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            else if (ACC.CommandEndKey == eCommandEndKey.Enter)
                            {
                                command = $"{command}\r{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            else
                            {
                                command = $"{command}{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            sRC = RunConsoleCommand(command);
                        }
                        if (mExpString != null && !sRC.Contains(mExpString))
                        {
                            act.Error = "Expected String \"" + mExpString + "\" not found in command output";
                            act.Status = Execution.eRunStatus.Failed;
                            break;
                        }

                        act.AddOrUpdateReturnParamActual("Result", sRC);

                        sRC = sRC.Replace("\r", "");
                        sRC = sRC.Replace("\t", "");
                        var RCValues = sRC.Split('\n');
                        foreach (var RCValue in RCValues)
                        {
                            if (RCValue.Trim().Length > 0)
                            {
                                string Param;
                                string Value;
                                var i = -1;
                                if (!string.IsNullOrEmpty(ACC.Delimiter))
                                {
                                    i = RCValue.IndexOf(ACC.Delimiter);
                                }
                                else
                                {
                                    i = RCValue.IndexOf('=');
                                }

                                if (i > 0 && i != RCValue.IndexOf("==") && i != RCValue.IndexOf("!=") + 1)
                                {
                                    Param = RCValue[..i].Trim();
                                    Value = RCValue[(Param.Length + 1)..];
                                    Value = new string(Value.Where(ch => !char.IsControl(ch)).ToArray());
                                    act.AddOrUpdateReturnParamActual(Param, Value);
                                }
                            }
                        }
                    }
                    break;

                case "ActScreenShot":
                    break;

                default:
                    Reporter.ToLog(eLogLevel.ERROR, $"Action unknown/not implemented for the Driver: {GetType().ToString()}");
                    break;
            }
        }

        public virtual string RunConsoleCommand(string command, string waitForText = null)
        {
            try
            {
                mConsoleBuffer.Clear();
                Reporter.ToLog(eLogLevel.DEBUG, $"Executing console command: {command}");
                var commandWithLineEnding = Platform.ToString() == "Unix" ? command + "\n" : command + System.Environment.NewLine;
                taskFinished = false;
                // Send command - use dispatcher if available
                if (Dispatcher != null)
                {
                    Dispatcher.Invoke(() => SendCommand(commandWithLineEnding));
                }
                else
                {
                    SendCommand(commandWithLineEnding);
                }
                // Wait for command to complete
                int timeout = mWait.HasValue ? mWait.Value * 1000 : ImplicitWait * 1000;
                int elapsed = 0;
                int checkInterval = 100; // Check every 100ms

                while (!taskFinished && elapsed < timeout)
                {
                    Thread.Sleep(checkInterval);
                    elapsed += checkInterval;

                    // If waiting for specific text, check if it's in the buffer
                    if (!string.IsNullOrEmpty(waitForText) && mConsoleBuffer.ToString().Contains(waitForText))
                    {
                        break;
                    }
                }

                var rc = mConsoleBuffer.ToString();
                var GingerRCStart = "~~~GINGER_RC_START~~~";
                var GingerRCEnd = "~~~GINGER_RC_END~~~";
                var i = rc.IndexOf(GingerRCStart);
                if (i > 0)
                {
                    var i2 = rc.IndexOf(GingerRCEnd, i);
                    if (i2 > 0)
                    {
                        rc = rc.Substring(i + GingerRCStart.Length + 1, i2 - i - GingerRCEnd.Length - 4);
                    }
                }
                mConsoleBuffer.Clear();
                return rc;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error executing command: {command}", ex);
                return $"Error: {ex.Message}";
            }
        }

        public virtual void WriteToConsoleBuffer(string text)
        {
            mConsoleBuffer.Append(text + System.Environment.NewLine);
            if (mIsRecordingBufferActive)
            {
                mRecordingBuffer.Append(text + System.Environment.NewLine);
            }
            OnDriverMessage(eDriverMessageType.ConsoleBufferUpdate, text + System.Environment.NewLine);
        }

        protected virtual string GetParameterizedCommand(ActConsoleCommand act)
        {
            var command = act.Command;
            foreach (var AIV in act.InputValues)
            {
                var calcValue = AIV.ValueForDriver;
                if (command != null)
                {
                    command = command + " " + calcValue;
                }
                else
                {
                    command = calcValue;
                }
            }
            return command;
        }

        public override string GetURL()
        {
            return string.Empty;
        }

        public override void HighlightActElement(Act act)
        {
        }

        public override bool IsRunning()
        {
            return IsDriverConnected;
        }

        public void CleanExpectedString()
        {
            mWait = null;
            mExpString = string.Empty;
        }

        public override void ActionCompleted(Act act)
        {
            taskFinished = true;
        }
        
        public bool CanStartAnotherInstance(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
