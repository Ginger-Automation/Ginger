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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GingerCore.Helpers;
using GingerCore.Platforms;
using GingerCore.Properties;
using GingerCore.Repository;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common;

namespace GingerCore.Actions.Java
{
    public class ActJavaEXE : ActWithoutDriver
    {        
        public override string ActionDescription { get { return "Java Execution Action"; } }
        public override string ActionUserDescription { get { return "Execute Java Program with set of input params and process it output to be integrated with the entire flow."; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you want to execute java program (jar file using java.exe) and parse it results.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();            
            TBH.AddText(@"Place the jar file with 'main' function in \Documents\Java - i.e: sum.jar");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText(@"Make sure to support the -GingerHelp argument.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText(@"For more details and sample java code please look at ginger Support site.");
            TBH.AddLineBreak();                        
        }

        public override string ActionEditPage { get { return "Java.ActJavaEXEEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }
        public override bool IsSelectableAction { get { return true; } }
        public override System.Drawing.Image Image { get { return Resources.Java16x16; } }
        public override String ActionType
        {
            get
            {
                return "Script - " + ScriptName;
            }
        }
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

        public new static partial class Fields
        {
            public static string ScriptName = "ScriptName";
            public static string ScriptPath = "ScriptPath";
            public static string ScriptCommand = "ScriptCommand";
            public static string ScriptDecription = "ScriptDecription";
            public static string JavaWSEXEPath = "JavaWSEXEPath"; //contains the Java version path in case user do not want to use JAVA_HOME
        }

        //------------------- Java version to use args
        string mJavaWSEXEPath = string.Empty;
        string mJavaWSEXEPath_Calc = string.Empty;
        [IsSerializedForLocalRepository]
        public string JavaWSEXEPath //contains the Java version path in case user do not want to use JAVA_HOME
        {
            get
            {
                return mJavaWSEXEPath;
            }
            set
            {
                mJavaWSEXEPath = value;
                OnPropertyChanged(Fields.JavaWSEXEPath);
            }
        }

        [IsSerializedForLocalRepository]
        public string ScriptName { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptPath { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptDecription { get; set; }

        string DataBuffer = "";
        string ErrorBuffer = "";
      
        protected void Process_Exited(object sender, EventArgs e)
        {
            ParseRC(DataBuffer);
        }

        protected void AddError(string outLine)
        {
            ErrorBuffer += outLine;
        }

        protected void AddData(string outLine)
        {
            DataBuffer += outLine;
        }

        public override void Execute()
        {
            //calculate the arguments
            if (!CalculateArguments()) return;

            string Params = GetCommandText();
            RunCommand(Params);
        }

        public string GetCommandText()
        {
            string cmd = "";
            foreach (var p in InputValues)
            {
                if (!string.IsNullOrEmpty(p.ValueForDriver))
                    cmd += " " + p.ValueForDriver;
            }
            return cmd;            
        }

        public string RunCommand(string parms)
        {
            //calculate the arguments
            CalculateArguments();

            DataBuffer = "";
            ErrorBuffer = "";
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.CreateNoWindow = true;              // Do not create the black window
            process.StartInfo.UseShellExecute = false;            // Do not use shell for execution
            process.StartInfo.RedirectStandardOutput = true;      // redirect standard output 
            process.StartInfo.RedirectStandardError = true;       // redirect standard error
            
            string JavaEXE = Path.Combine(mJavaWSEXEPath_Calc, @"bin\java.exe");
            if (File.Exists(JavaEXE))
                process.StartInfo.FileName = JavaEXE;
            else
                process.StartInfo.FileName = "java.exe";

            process.OutputDataReceived += (proc, outLine) => { AddData(outLine.Data + "\n"); };
            process.ErrorDataReceived += (proc, outLine) => { AddError(outLine.Data + "\n"); };
            process.Exited += Process_Exited;
            if (string.IsNullOrEmpty(SolutionFolder))
            {
                if (string.IsNullOrEmpty(ScriptPath))
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(ScriptName);
                else
                    process.StartInfo.WorkingDirectory = ScriptPath;
            }
            else
                process.StartInfo.WorkingDirectory = System.IO.Path.Combine(SolutionFolder, @"Documents\Java\");
            try
            {
                process.StartInfo.Arguments = "-jar " + ScriptName + " " + parms;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                while (!process.HasExited)
                {
                    Thread.Sleep(100);
                }

                if (string.IsNullOrEmpty(ErrorBuffer) && ErrorBuffer != "\n")
                {
                    Error += ErrorBuffer;
                }

                return DataBuffer;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
                this.Error = "Failed to execute the script. Details: " + e.Message;

                return DataBuffer + "\n" + ErrorBuffer;
            }
        }

        private new void ParseRC(string sRC)
        {
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

        public string[] GetParamsWithGingerHelp()
        {
            string output = RunCommand("-GingerHelp");           
            string[] lines = DataBuffer.Split('\n');
            return lines;             
        }

        ValueExpression mVE = null;
        private string CalculateValue(string valueTocalc)
        {
            mVE = new ValueExpression(RunOnEnvironment, RunOnBusinessFlow, DSList);
            mVE.Value = valueTocalc;
            return mVE.ValueCalculated.Trim();
        }

        private bool CalculateArguments()
        {
            try
            {
                mJavaWSEXEPath_Calc = CalculateValue(mJavaWSEXEPath);
                if (string.IsNullOrEmpty(mJavaWSEXEPath_Calc))
                    mJavaWSEXEPath_Calc = CommonLib.GetJavaHome();               

                return true;
            }
            catch (Exception ex)
            {
                Error = "Error occurred while calculating the action arguments. Error=" + ex.Message;
                return false;
            }
        }
    }
}