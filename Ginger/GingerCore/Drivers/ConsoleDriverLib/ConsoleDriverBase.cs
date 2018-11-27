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

using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.Actions;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Drawing;
using System.Reflection;
using System.Windows.Threading;
using Amdocs.Ginger.Common;

namespace GingerCore.Drivers.ConsoleDriverLib
{
    public abstract class ConsoleDriverBase : DriverBase
    {
        [UserConfigured]
        [UserConfiguredDefault("30")]
        [UserConfiguredDescription("Implicit Wait for Console Action Completion")]
        public int ImplicitWait { get; set; }

        public bool taskFinished = false;
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
            mConsoleDriverWindow = new ConsoleDriverWindow(BusinessFlow);
            mConsoleDriverWindow.mConsoleDriver = this;
            mConsoleDriverWindow.Title = ConsoleWindowTitle();
            mConsoleDriverWindow.Show();
            IsDriverConnected = Connect();

            if (IsDriverConnected)
            {
                Dispatcher = mConsoleDriverWindow.Dispatcher;
                Dispatcher.Invoke(new Action(() => OnDriverMessage(eDriverMessageType.DriverStatusChanged)));
                Dispatcher.Run();
            }
            else
            {
                mConsoleDriverWindow.Close();
                mConsoleDriverWindow = null; OnDriverMessage(eDriverMessageType.DriverStatusChanged);
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
            }
            catch (InvalidOperationException e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error when try to close Console Driver - " + ex.Message, ex);
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
                        act.AddOrUpdateReturnParamActual("GINGER_RC",command.Replace("GINGER_RC=", ""));
                    }
                    else
                    {
                        string sRC;
                        //Send the command via driver
                        if (ACC.ConsoleCommand == ActConsoleCommand.eConsoleCommand.Script)
                        {
                            //TODO: externalize static const for ~~~GINGER_RC_END~~~ and all hard coded multi use strings
                            sRC = mConsoleDriverWindow.RunConsoleCommand(command, "~~~GINGER_RC_END~~~");
                        }
                        else
                        {
                            sRC = mConsoleDriverWindow.RunConsoleCommand(command);
                        }
                        if (mExpString != null && sRC.Contains(mExpString) == false)
                        {
                            act.Error = @"Expected String """ + mExpString + @""" not found in command output";
                            act.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            return;
                        }

                        act.AddOrUpdateReturnParamActual("Result", sRC);

                        sRC =sRC.Replace("\r", "");
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
                                
                                if ((i > 0) && ( i != RCValue.IndexOf("==")) && (i != RCValue.IndexOf("!=") +1))
                                {
                                    Param = (RCValue.Substring(0, i)).Trim();
                                    //the rest is the value
                                    Value = RCValue.Substring(Param.Length + 1);
                                    
                                    Value = new string(Value.Where(ch => !char.IsControl(ch)).ToArray());
                                    act.AddOrUpdateReturnParamActual(Param, Value);
                                }
                            }
                        }
                    }
                    break;

                case "ActScreenShot":
                    TakeScreenShot(act);
                    break;

                default:
                    throw new Exception("Action unknown/Not Impl in Driver - " + this.GetType().ToString());
            }
        }

        public void TakeScreenShot(Act act)
        {
            try
            {
                int width = (int)mConsoleDriverWindow.Width;
                int height = (int)mConsoleDriverWindow.Height;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(mConsoleDriverWindow);
                
                //no need to create file (will be created later by the action) so creating only Bitmap
                using (MemoryStream stream = new MemoryStream())
                {
                    BitmapEncoder encoder = new BmpBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                    encoder.Save(stream);

                    using (Bitmap bitmap = new Bitmap(stream))
                    {
                        act.AddScreenShot(bitmap);
                    }
                }
            }
            catch(Exception ex)
            {
                act.Error = "Failed to create console window screenshot. Error= " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, act.Error, ex);
            }
        }

        public override string GetURL()
        {
            return "TBD";
        }

        public override List<ActWindow> GetAllWindows()
        {
            return null;
        }

        public override List<ActLink> GetAllLinks()
        {
            return null;
        }

        public override List<ActButton> GetAllButtons()
        {
            return null;
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
    }
}
