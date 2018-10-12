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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.Repository;
using GingerCore.Properties;
using GingerCore.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GingerCore.Platforms;
using System.Runtime.InteropServices;
using GingerCore.Helpers;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;

namespace GingerCore.Actions
{
    public class ActScript : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Script Action"; } }
        public override string ActionUserDescription { get { return "Performs Script Action"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to perform any script actions on web page.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a script action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property" +
            "that you set in Locate By type then select script interpreter and script name to be execute on page and the enter the page url in value textbox and run the action.");
            TBH.AddLineBreak();
           TBH.AddText("For using CMD.exe as the interpreter, select interpreter type as Other, put the full path of CMD.exe in the Interpreter drop down list; select either free command or script "+ 
               "then put the command or cmd/bat file name in the proper field respectively.");
        }        

        public override string ActionEditPage { get { return "ActScriptEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }
        
        public enum eScriptAct
        {
            FreeCommand = 1,           
            Script = 0
        }
        public enum eScriptInterpreterType
        {            
            VBS,
            JS,
            Other,
        }
        [IsSerializedForLocalRepository]
        public eScriptAct ScriptCommand { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptInterpreter { get; set; }

        [IsSerializedForLocalRepository]
        public eScriptInterpreterType ScriptInterpreterType { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptName { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptPath { get; set; }

        string DataBuffer="";
        string ErrorBuffer="";

        public override String ActionType
        {
            get
            {
                return "Script - " + ScriptName;
            }
        }
        public new static partial class Fields
        {
            public static string ScriptCommand = "ScriptCommand";     
            public static string ScriptName = "ScriptName";
            public static string ScriptInterpreter = "ScriptInterpreter";
            public static string ScriptInterpreterType = "ScriptInterpreterType";
         
        }
        public string ExpectString { get; set; }
 
        public override System.Drawing.Image Image { get { return Resources.Vbs16x16; } }

        protected void Process_Exited(object sender, EventArgs e)
        {
            parseRC(DataBuffer);
        }

        protected void AddError(string outLine)
        {
            if (!string.IsNullOrEmpty(outLine))
            {
                ErrorBuffer += outLine + "\n";
            }
        }

        protected void AddData(string outLine)
        {
            DataBuffer += outLine + "\n";
        }
        public override void Execute()
        {
            if (ScriptName == null)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Script file not Selected. Kindly select suitable file");
                this.Error = "Script file not loaded. Kindly select suitable file";
                return;
            }
            DataBuffer = "";
            ErrorBuffer = "";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            //TODO: user customerd script location

            switch (ScriptInterpreterType)
            {
                case eScriptInterpreterType.JS:
                case eScriptInterpreterType.VBS:
                   if(File.Exists(GetSystemDirectory()+@"\cscript.exe"))
                       p.StartInfo.FileName = GetSystemDirectory() + @"\cscript.exe";
                   else
                       p.StartInfo.FileName = @"cscript";
                    break;
                case eScriptInterpreterType.Other:
                    if (!string.IsNullOrEmpty(ScriptInterpreter))
                    {
                        
                        p.StartInfo.FileName = ScriptInterpreter.Replace(@"~\", this.SolutionFolder);
                    }
                    break;
             }
            p.OutputDataReceived += (proc, outLine) => { AddData(outLine.Data); };
            p.ErrorDataReceived += (proc, outLine) => { AddError(outLine.Data); };
            p.Exited += Process_Exited;
            if (string.IsNullOrEmpty(SolutionFolder))
            {
                if (string.IsNullOrEmpty(ScriptPath))
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(ScriptName);
                else
                    p.StartInfo.WorkingDirectory = ScriptPath;
            }
            else
                p.StartInfo.WorkingDirectory = System.IO.Path.Combine(SolutionFolder, @"Documents\scripts\"); 
            try
            {
                string Params = GetCommandText(this);
                p.StartInfo.Arguments ="\""+ ScriptName +"\" "+ Params;
                if (ScriptInterpreter != null && ScriptInterpreter.Contains("cmd.exe"))
                    p.StartInfo.Arguments = " /k " + ScriptName + " " + Params;
            
                    p.Start();
                           
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            while (!p.HasExited)
            {
                Thread.Sleep(100);
            }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
                this.Error = "Failed to execute the script. Details: " + e.Message;
            }
            if (!string.IsNullOrEmpty(ErrorBuffer))
            {
                Error += ErrorBuffer;
            }
        }

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

        private string GetSystemDirectory()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero,path,0x0029,false);
            return path.ToString();
        }
        private void parseRC(string sRC)
        {
            Regex rg = new Regex(@"Microsoft.*\n.*All rights reserved.");
            sRC=rg.Replace(sRC, "");
            string GingerRCStart = "~~~GINGER_RC_START~~~";
            string GingerRCEnd = "~~~GINGER_RC_END~~~";

            int i = sRC.IndexOf(GingerRCStart);
            int i2 = -1;
            if (i > 0)
            {
                i2 = sRC.IndexOf(GingerRCEnd, i);
                if (i2 > 0)
                {
                    sRC = sRC.Substring(i + GingerRCStart.Length , i2 - i - GingerRCEnd.Length-2);
                }
            }

            if (i>0 && i2> 0)
            {             
                    string[] RCValues = sRC.Split('\n');
                    foreach (string RCValue in RCValues)
                    {
                        if (RCValue.Length > 0) // Ignore empty lines
                        {                                                                
                            string Param;
                            string Value;
                            i = RCValue.IndexOf('=');
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
                            AddOrUpdateReturnParamActual(Param, Value);
                      }
                    }
              
                
            }
            else
            {
                //No params found so return the full output
                AddOrUpdateReturnParamActual("???", sRC);                
            }
        }
        public string GetCommandText(ActScript act)
        {

            string cmd = "";
            switch (act.ScriptCommand)
            {
                case ActScript.eScriptAct.FreeCommand:
                    return act.GetInputParamCalculatedValue("Free Command");

                case ActScript.eScriptAct.Script:
                    {
                        foreach (var p in act.InputValues)
                            if (!string.IsNullOrEmpty(p.ValueForDriver))
                                cmd += " " + p.ValueForDriver;
                        return cmd;
                    }
                default:
                    Reporter.ToUser(eUserMsgKeys.UnknownConsoleCommand, act.ScriptCommand);
                    return "Error - unknown command";
            }
        }
    }
}