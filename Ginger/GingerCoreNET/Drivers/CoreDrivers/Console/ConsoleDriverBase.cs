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
using Amdocs.Ginger.CoreNET.Drivers.DriversWindow; // Added for IDriverWindow
using GingerCore.Actions;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;


namespace GingerCore.Drivers.ConsoleDriverLib
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

        public void BeginInvokeShutdown(dynamic dispatherPriority)
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
            return "Ginger.Drivers.DriverWindow.ConsoleDriverWindow";
        }

        public bool taskFinished = false;
        
        // Console buffer to capture command output when running without UI
        private StringBuilder mConsoleBuffer = new StringBuilder();
        
        public override bool IsSTAThread()
        {
            return true;
        }

        public abstract bool Connect();
        public abstract void Disconnect();
        public abstract bool IsBusy();
        bool IsDriverConnected = false;

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

            if (!ShowWindow)
            {
                // Headless mode: provide dummy dispatcher so actions relying on dispatcher still work
                Dispatcher = new DummyDispatcher();
            }

            OnDriverMessage(eDriverMessageType.DriverStatusChanged);
        }

        // Legacy method retained for backward compatibility (not used by new flow)
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
                if (Dispatcher != null && !ShowWindow)
                {
                    Thread.Sleep(50);
                }
                Disconnect();
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
            string actClass = act.GetType().ToString();
            actClass = actClass.Replace("GingerCore.Actions.", "");
            switch (actClass)
            {
                case "ActConsoleCommand":
                    mWait = ((ActConsoleCommand)act).WaitTime;

                    ValueExpression VE = new ValueExpression(this.Environment, this.BusinessFlow);

                    string ExpString = ((ActConsoleCommand)act).ExpString;
                    VE.Value = ExpString;
                    mExpString = VE.ValueCalculated;
                    ActConsoleCommand ACC = (ActConsoleCommand)act;
                    string command = GetCommandText(ACC);
                    act.ExInfo = command;
                    taskFinished = false;
                    if (command.StartsWith("GINGER_RC="))
                    {
                        act.AddOrUpdateReturnParamActual("GINGER_RC", command.Replace("GINGER_RC=", ""));
                    }
                    else
                    {
                        string sRC;
                        if (ACC.ConsoleCommand == ActConsoleCommand.eConsoleCommand.Script)
                        {
                            sRC = RunConsoleCommand(command, "~~~GINGER_RC_END~~~");
                        }
                        else
                        {
                            if (Platform == GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Unix)
                            {
                                command = $"{command}{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            else if(ACC.CommandEndKey== eCommandEndKey.Enter)
                            {
                                command = $"{command}\r{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            else
                            {
                                command = $"{command}{GetCommandValue(ACC.CommandEndKey)}";
                            }
                            sRC = RunConsoleCommand(command);
                        }
                        if (mExpString != null && sRC.Contains(mExpString) == false)
                        {
                            act.Error = "Expected String \"" + mExpString + "\" not found in command output";
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            return;
                        }

                        act.AddOrUpdateReturnParamActual("Result", sRC);

                        sRC = sRC.Replace("\r", "");
                        sRC = sRC.Replace("\t", "");
                        string[] RCValues = sRC.Split('\n');
                        foreach (string RCValue in RCValues)
                        {
                            if (RCValue.Trim().Length > 0)
                            {
                                string Param;
                                string Value;
                                int i = -1;
                                if (!string.IsNullOrEmpty(ACC.Delimiter))
                                {
                                    i = RCValue.IndexOf(ACC.Delimiter);
                                }
                                else
                                {
                                    i = RCValue.IndexOf('=');
                                }

                                if ((i > 0) && (i != RCValue.IndexOf("==")) && (i != RCValue.IndexOf("!=") + 1))
                                {
                                    Param = (RCValue[..i]).Trim();
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
                    throw new Exception("Action unknown/not implemented for the Driver: " + this.GetType().ToString());
            }
        }

        public virtual string RunConsoleCommand(string command, string waitForText = null)
        {
            mConsoleBuffer.Clear();
            Reporter.ToLog(eLogLevel.DEBUG, $"Executing console command: {command}");
            string commandWithLineEnding = Platform.ToString() == "Unix" ? command + "\n" : command + System.Environment.NewLine;
            taskFinished = false;
            SendCommand(commandWithLineEnding);
            string rc = mConsoleBuffer.ToString();
            string GingerRCStart = "~~~GINGER_RC_START~~~";
            string GingerRCEnd = "~~~GINGER_RC_END~~~";
            int i = rc.IndexOf(GingerRCStart);
            if (i > 0)
            {
                int i2 = rc.IndexOf(GingerRCEnd, i);
                if (i2 > 0)
                {
                    rc = rc.Substring(i + GingerRCStart.Length + 1, i2 - i - GingerRCEnd.Length - 4);
                }
            }
            mConsoleBuffer.Clear();
            return rc;
        }

        public virtual void WriteToConsoleBuffer(string text)
        {
            mConsoleBuffer.Append(text + System.Environment.NewLine);
        }

        protected virtual string GetParameterizedCommand(ActConsoleCommand act)
        {
            string command = act.Command;
            foreach (ActInputValue AIV in act.InputValues)
            {
                string calcValue = AIV.ValueForDriver;
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
            return "TBD";
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
