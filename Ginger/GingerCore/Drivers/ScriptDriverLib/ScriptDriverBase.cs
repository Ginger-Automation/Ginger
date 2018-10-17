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
using Amdocs.Ginger.Common;
using GingerCore.Actions;

namespace GingerCore.Drivers.ScriptDriverLib
{
    public abstract class ScriptDriverBase : DriverBase
    {
        protected string DataBuffer;
        protected string ErrorBuffer;

        protected ActScript mActVBS;

        // Get the action command to be send to the specific derived driver
        public abstract string GetCommandText(ActScript act);
        
        public override void StartDriver()
        {
        }

        public override void CloseDriver()
        {
            try
            {
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error when try to close Console Driver - " + ex.Message);
            }
        }

        protected virtual string RunScript(string cmd) { return cmd; }
       
        protected void Process_Exited(object sender, EventArgs e)
        {            
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Data=" + DataBuffer);
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Error=" + ErrorBuffer);
        }

        protected void AddError(string outLine)
        {
            ErrorBuffer += outLine;
        }

        protected void AddData(string outLine)
        {
            DataBuffer += outLine;
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
                case "ActVBScript":
                    ActScript ACC = (ActScript)act;
                    mActVBS = (ActScript)act;
                    string command = GetCommandText(ACC);
                    act.ExInfo = command;
                    if (command.StartsWith("GINGER_RC="))
                    {
                        //This is FTP command and we already have the result
                        act.AddOrUpdateReturnParamActual("GINGER_RC",command.Replace("GINGER_RC=", ""));
                    }
                    else
                    {
                        string sRC="";
                        //Send the command via driver
                        if (ACC.ScriptCommand == ActScript.eScriptAct.Script)
                        {
                            //TODO: externalize static const for ~~~GINGER_RC_END~~~ and all hard coded multi use strings
                            sRC = RunScript(command);
                        }
                        else
                        {
                            sRC = RunScript(command);
                        }
                        sRC =sRC.Replace("\r", "");
                        string[] RCValues = sRC.Split('\n');
                        foreach (string RCValue in RCValues)
                        {
                            if (RCValue.Length > 0) // Ignore empty lines
                            {                                                                
                                string Param;
                                string Value;
                                int i = RCValue.IndexOf('=');
                                if (i > 0) 
                                {
                                    Param = RCValue.Substring(0, i);
                                    //the rest is the value
                                    Value = RCValue.Substring(Param.Length + 1);
                                }
                                else
                                {
                                    // in case of bad RC not per Ginger style we show it as "?" with value
                                    Param = "???";
                                    Value = RCValue;
                                }
                                act.AddOrUpdateReturnParamActual(Param, Value);
                            }
                        }
                    }
                    break;
                default:
                    throw new Exception("Action unknown/Not Impl in Driver - " + this.GetType().ToString());
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
    }
}
