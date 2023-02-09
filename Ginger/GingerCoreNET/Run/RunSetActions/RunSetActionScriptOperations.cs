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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using Ginger.Reports;
using GingerCore.Actions;
using Amdocs.Ginger.Common.InterfacesLib;
using System.Runtime.InteropServices;
using static Ginger.Run.RunSetActions.RunSetActionBase;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.Run.RunSetActions
{
    public class RunSetActionScriptOperations : IRunSetActionScriptOperations
    {
        public RunSetActionScript RunSetActionScript;
        public RunSetActionScriptOperations(RunSetActionScript runSetActionScript)
        {
            this.RunSetActionScript = runSetActionScript;
            this.RunSetActionScript.RunSetActionScriptOperations = this;
        }
        public void Execute(IReportInfo RI)
        {
            try
            {
                ActScript act = new ActScript();
                string FileName = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(RunSetActionScript.ScriptFileName);
                VerifySolutionFloder(RunSetActionScript.SolutionFolder, FileName);
                act.ScriptName = FileName;
                act.ScriptInterpreterType = (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    ? ActScript.eScriptInterpreterType.VBS : ActScript.eScriptInterpreterType.SH;
                act.Execute();
            }
            catch (Exception ex)
            {
                RunSetActionScript.Errors = ex.Message.ToString();
                RunSetActionScript.Status = eRunSetActionStatus.Failed;
            }
        }

        private void VerifySolutionFloder(string SolutionFolder, string FileName)
        {
            if (string.IsNullOrEmpty(SolutionFolder))
            {
                RunSetActionScript.Errors = "Script path not provided.";
                RunSetActionScript.Status = eRunSetActionStatus.Failed;
                return;
            }
            if (!System.IO.File.Exists(FileName))
            {
                RunSetActionScript.Errors = "File Not found: " + FileName;
                RunSetActionScript.Status = eRunSetActionStatus.Failed;
                return;
            }
        }

    }
}
