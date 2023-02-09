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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace GingerCore.Actions
{
    public class ActScript : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Script Action"; } }
        public override string ActionUserDescription { get { return "Performs Script Action"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you want to perform any script actions on web page.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To perform a script action, Select Locate By type, e.g- ByID,ByCSS,ByXPath etc.Then enter the value of property " +
            "that you set in Locate By type then select script interpreter and script name to be execute on page and the enter the page url in value textbox and run the action.");
            TBH.AddLineBreak();
            TBH.AddText("For using CMD.exe as the interpreter, select interpreter type as Other, put the full path of CMD.exe in the Interpreter drop down list; select either free command or script " +
                "then put the command or cmd/bat file name in the proper field respectively.");
        }

        public override string ActionEditPage { get { return "ActScriptEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
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
            BAT,
            SH,
            Other,
        }
        [IsSerializedForLocalRepository]
        public eScriptAct ScriptCommand { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptInterpreter { get; set; }

        [IsSerializedForLocalRepository]
        public eScriptInterpreterType ScriptInterpreterType { get; set; }


        [IsSerializedForLocalRepository]
        public string ScriptName
        {
            get; set;
        }


        [IsSerializedForLocalRepository]
        public string ScriptPath { get; set; }

        string DataBuffer = "";
        string ErrorBuffer = "";
        public override String ActionType
        {
            get
            {
                return "Script - " + ScriptName;
            }
        }
        public string ExpectString { get; set; }

        public override eImageType Image { get { return eImageType.CodeFile; } }

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
            if (ScriptName == null && ScriptCommand == eScriptAct.Script)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Script file not Selected. Kindly select suitable file");
                this.Error = "Script file not loaded. Kindly select suitable file";
                return;
            }
            DataBuffer = "";
            ErrorBuffer = "";
            string TempFileName = "";

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            //TODO: user customerd script location

            switch (ScriptInterpreterType)
            {
                case eScriptInterpreterType.BAT:
                    break;
                case eScriptInterpreterType.JS:
                case eScriptInterpreterType.VBS:
                    if (File.Exists(GetSystemDirectory() + @"\cscript.exe"))
                    {
                        p.StartInfo.FileName = GetSystemDirectory() + @"\cscript.exe";
                    }
                    else
                    {
                        p.StartInfo.FileName = @"cscript";
                    }
                    break;
                case eScriptInterpreterType.SH:
                    p.StartInfo.FileName = "/bin/bash";
                    break;
                case eScriptInterpreterType.Other:
                    if (!string.IsNullOrEmpty(ScriptInterpreter))
                    {
                        p.StartInfo.FileName = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ScriptInterpreter);
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
            {
                if (Directory.Exists(Path.Combine(SolutionFolder, "Documents", "scripts")))
                {
                    p.StartInfo.WorkingDirectory = Path.Combine(SolutionFolder, "Documents", "scripts");
                }
                else
                {
                    p.StartInfo.WorkingDirectory = Path.Combine(SolutionFolder, "Documents", "Scripts");
                }
            }
            p.StartInfo.WorkingDirectory = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(p.StartInfo.WorkingDirectory);
            try
            {
                string Params = GetCommandText(this);
                if (ScriptCommand == eScriptAct.Script)
                {
                    if (ScriptInterpreterType == eScriptInterpreterType.BAT)
                    {
                        p.StartInfo.FileName = System.IO.Path.Combine(p.StartInfo.WorkingDirectory, ScriptName);
                        p.StartInfo.Arguments = Params;
                    }
                    else if (ScriptInterpreterType == eScriptInterpreterType.SH)
                    {
                        string filePath = Path.Combine(p.StartInfo.WorkingDirectory, ScriptName);
                        p.StartInfo.Arguments = filePath + Params;
                    }
                    else if (ScriptInterpreter != null && ScriptInterpreter.Contains("cmd.exe"))
                    {
                        p.StartInfo.Arguments = " /k " + ScriptName + " " + Params;
                    }
                    else
                    {
                        p.StartInfo.Arguments = "\"" + ScriptName + "\" " + Params;
                    }
                }
                else
                {
                    if (ScriptInterpreterType == eScriptInterpreterType.VBS)
                    {
                        TempFileName = CreateTempFile("vbs");
                    }
                    else if (ScriptInterpreterType == eScriptInterpreterType.BAT)
                    {
                        TempFileName = CreateTempFile("bat");
                        p.StartInfo.FileName = TempFileName;
                    }
                    else if (ScriptInterpreterType == eScriptInterpreterType.Other)
                    {
                        if (ScriptInterpreter != null && ScriptInterpreter.ToLower().Contains("cmd.exe"))
                        {
                            TempFileName = CreateTempFile("cmd");
                        }
                        else if (ScriptInterpreter != null && ScriptInterpreter.ToLower().Contains("powershell.exe"))
                        {
                            TempFileName = CreateTempFile("ps1");
                            p.StartInfo.Arguments = @"-executionpolicy bypass -file .\" + TempFileName + " " + Params;
                        }
                        else if (ScriptInterpreter != null && ScriptInterpreter.ToLower().Contains("python.exe"))
                        {
                            TempFileName = CreateTempFile("py");
                        }
                        else if (ScriptInterpreter != null && ScriptInterpreter.ToLower().Contains("perl.exe"))
                        {
                            TempFileName = CreateTempFile("pl");
                        }
                        else
                        {
                            //TODO: Need to create temp file based on the selected interpreter
                            this.Error = "This type of script is not supported.";
                        }
                    }

                    if (string.IsNullOrEmpty(p.StartInfo.Arguments) && ScriptInterpreterType != eScriptInterpreterType.BAT)
                    {
                        p.StartInfo.Arguments = "\"" + TempFileName + "\"";
                    }
                    File.WriteAllText(TempFileName, Params);
                }
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
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
                this.Error = "Failed to execute the script. Details: " + e.Message;
            }
            finally
            {
                DeleteTempFile(TempFileName);
            }
            if (!string.IsNullOrEmpty(ErrorBuffer))
            {
                Error += ErrorBuffer;
            }
        }

        [DllImport("shell32.dll")]
        public static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out] StringBuilder lpszPath, int nFolder, bool fCreate);

        private string GetSystemDirectory()
        {
            StringBuilder path = new StringBuilder(260);
            SHGetSpecialFolderPath(IntPtr.Zero, path, 0x0029, false);
            return path.ToString();
        }
        private void parseRC(string sRC)
        {
            Regex rg = new Regex(@"Microsoft.*\n.*All rights reserved.");
            sRC = rg.Replace(sRC, "");
            string GingerRCStart = "~~~GINGER_RC_START~~~";
            string GingerRCEnd = "~~~GINGER_RC_END~~~";

            int i = sRC.IndexOf(GingerRCStart);
            int i2 = -1;
            if (i >= 0)
            {
                i2 = sRC.IndexOf(GingerRCEnd, i);
                if (i2 > 0)
                {
                    sRC = sRC.Substring(i + GingerRCStart.Length, i2 - i - GingerRCEnd.Length - 2);
                }
            }
            if (i >= 0 && i2 > 0)
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
                    Reporter.ToUser(eUserMsgKey.UnknownConsoleCommand, act.ScriptCommand);
                    return "Error - unknown command";
            }
        }

        string CreateTempFile(string extenion)
        {
            string fileName = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(SolutionFolder))
                {
                    fileName = Path.Combine(SolutionFolder, @"Documents\scripts\", Guid.NewGuid().ToString() + "." + extenion);
                }
                else
                {
                    fileName = Path.GetTempPath() + Guid.NewGuid().ToString() + "." + extenion;
                }
            }
            catch (Exception ex)
            {
                //this to overcome speical IT settings which doesn't allow local personal folders
                fileName = "" + Environment.GetFolderPath(Environment.SpecialFolder.InternetCache) + @"\" + Guid.NewGuid().ToString() + "." + extenion;
            }
            return fileName;
        }

        void DeleteTempFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
                    }
                }
            }
        }
    }
}