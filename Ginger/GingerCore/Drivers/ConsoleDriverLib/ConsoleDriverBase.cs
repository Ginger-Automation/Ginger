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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

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

    public abstract class ConsoleDriverBase : DriverBase, IVirtualDriver
    {
        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Implicit Wait for Console Action Completion")]
        public int ImplicitWait { get; set; }

        public bool taskFinished = false;
        
        // Console buffer to capture command output when running without UI
        private StringBuilder mConsoleBuffer = new StringBuilder();
        
        public override bool IsSTAThread()
        {
            return true;
        }

        public ConsoleDriverWindow mConsoleDriverWindow;

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
            CreateSTA(ShowDriverWindow);
        }

        public void ShowDriverWindow()
        {
            // Use dummy dispatcher instead of creating UI window
            IsDriverConnected = Connect();

            if (IsDriverConnected)
            {
                Dispatcher = new DummyDispatcher();
                Dispatcher.Invoke(new Action(() => OnDriverMessage(eDriverMessageType.DriverStatusChanged)));
            }
            else
            {
                OnDriverMessage(eDriverMessageType.DriverStatusChanged);
            }
        }

        public override void CloseDriver()
        {
            try
            {
                if (mConsoleDriverWindow != null)
                {
                    mConsoleDriverWindow.Close();
                    mConsoleDriverWindow = null;
                }
                
                if (Dispatcher != null)
                {
                    Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background);
                    Thread.Sleep(100);
                }
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

        public override void RunAction(Act act)
        {
            //TODO: add func to Act + Enum for switch
            string actClass = act.GetType().ToString();

            //TODO: avoid hard coded string...
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
                        //This is FTP command and we already have the result
                        act.AddOrUpdateReturnParamActual("GINGER_RC", command.Replace("GINGER_RC=", ""));
                    }
                    else
                    {
                        string sRC;
                        //Send the command via driver - now directly through base class
                        if (ACC.ConsoleCommand == ActConsoleCommand.eConsoleCommand.Script)
                        {
                            //TODO: externalize static const for ~~~GINGER_RC_END~~~ and all hard coded multi use strings
                            sRC = RunConsoleCommand(command, "~~~GINGER_RC_END~~~");
                        }
                        else
                        {
                            sRC = RunConsoleCommand(command);
                        }
                        if (mExpString != null && sRC.Contains(mExpString) == false)
                        {
                            act.Error = @"Expected String """ + mExpString + @""" not found in command output";
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
                                    //the rest is the value
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
            
            // Log the command being executed
            Reporter.ToLog(eLogLevel.DEBUG, $"Executing console command: {command}");
            
            // Add platform-specific line ending and execute the command
            string commandWithLineEnding;
            if (Platform.ToString() == "Unix")
            {
                commandWithLineEnding = command + "\n";
            }
            else
            {
                commandWithLineEnding = command + System.Environment.NewLine;
            }
            
            taskFinished = false;
            SendCommand(commandWithLineEnding);

            // Get the result from the buffer
            string rc = mConsoleBuffer.ToString();
            
            // Process Ginger-specific markers if present
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

        /// <summary>
        /// Method for writing command output to the console buffer (to be called by derived drivers)
        /// </summary>
        /// <param name="text">Text to append to buffer</param>
        public virtual void WriteToConsoleBuffer(string text)
        {
            //mConsoleBuffer.Append(text + System.Environment.NewLine);
            
            //// Also write to console window if it exists
            //if (mConsoleDriverWindow != null)
            //{
            //    mConsoleDriverWindow.ConsoleWriteText(text, true);
            //}
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
