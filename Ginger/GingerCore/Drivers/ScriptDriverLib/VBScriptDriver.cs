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
using System.Threading;
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Drivers.ScriptDriverLib
{
    public class VBScriptDriver : ScriptDriverBase
    {
        protected override string RunScript(string vbscmd)
        {
            DataBuffer = "";
            ErrorBuffer = "";
  
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.FileName = @"cscript"; 
            p.OutputDataReceived += (proc, outLine) => { AddData(outLine.Data); };
            p.ErrorDataReceived += (proc, outLine) => { AddError(outLine.Data); };
            p.Exited += Process_Exited;
            p.StartInfo.WorkingDirectory = mActVBS.ScriptPath;
            try
            {
                p.StartInfo.Arguments =  mActVBS.ScriptName+" " + vbscmd;;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, e.Message);
            }
            p.Start();

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            while (!p.HasExited)
            {
                Thread.Sleep(100);
            }
            return DataBuffer;
        }

        public override string GetCommandText(ActScript act)
        {
            string cmd="";
            switch (act.ScriptCommand)
            {
                case ActScript.eScriptAct.FreeCommand:
                    return act.GetInputParamCalculatedValue("p1");
              
                case ActScript.eScriptAct.Script:
                    {
                        foreach (var p in act.InputValues)
                            if (!string.IsNullOrEmpty(p.Value))
                                cmd += " " + p.ValueForDriver;
                     return cmd; 
                    }
                default:
                    Reporter.ToUser(eUserMsgKeys.UnknownConsoleCommand, act.ScriptCommand);
                    return "Error - unknown command";
            }
        }

        public override ePlatformType Platform { get { return ePlatformType.VBScript; } }
        
        public override bool IsRunning()
        {
            return true;
        }
    }
}
