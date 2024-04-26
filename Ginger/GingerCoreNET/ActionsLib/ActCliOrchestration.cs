#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CliWrap;
using System.IO;
using GingerCoreNET.GeneralLib;
using System.Runtime.InteropServices;

namespace GingerCore.Actions
{
    public class ActCLIOrchestration : ActWithoutDriver
    {
        public override String ActionType
        {
            get
            {
                return "CLI Orchestration";
            }
        }

        public override string ActionDescription { get { return "CLI Orchestration"; } }

        public override bool ObjectLocatorConfigsNeeded { get { return false; } }

        public override bool ValueConfigsNeeded { get { return true; } }

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

        public override string ActionEditPage { get { return "ActCLIOrchestrationEditPage"; } }

        public override string ActionUserDescription { get { return "Perform CLI Orchestration Execute a file, parse output if required "; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to perform CLI Orchestration");

        }

        [IsSerializedForLocalRepository]
        public string FilePath { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptPath { get; set; }

        [IsSerializedForLocalRepository]
        public bool WaitForProcessToFinish { get; set; }

        [IsSerializedForLocalRepository]
        public bool ParseResult { get; set; }

        [IsSerializedForLocalRepository]
        public string Delimiter { get; set; }

        StringBuilder DataBuffer;
        public override void Execute()
        {
            DataBuffer = new StringBuilder();
            if (ParseResult && string.IsNullOrEmpty(ValueExpression.Calculate(Delimiter)))
            {
                Error = "Delimiter is Empty";
                return;
            }

            var task = Task.Run(() =>
                ExecuteCliProcess()
                );

            if (WaitForProcessToFinish)
            {
                task.Wait();
            }
        }

        private async Task ExecuteCliProcess()
        {
            bool isVBSScript = false;
            StringBuilder arguments = new ();
            if ((Path.GetExtension(this.FilePath)).Equals(".vbs", StringComparison.InvariantCultureIgnoreCase) || (Path.GetExtension(this.FilePath)).Equals(".js",StringComparison.InvariantCultureIgnoreCase))
            {
                arguments.Append(this.FilePath).Append(" ");
                isVBSScript = true;
            }
            foreach (var p in this.InputValues)
            {
                arguments.Append(p.Param).Append(" ");
                if (!string.IsNullOrEmpty(p.ValueForDriver))
                {
                    arguments.Append(p.ValueForDriver).Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(this.FilePath))
            {
                string path = String.Empty;
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                if (WorkSpace.Instance.Solution.Folder != null && WaitForProcessToFinish)
                {
                    string folderPath = $"{WorkSpace.Instance.Solution.Folder}{Path.DirectorySeparatorChar}Documents{Path.DirectorySeparatorChar}CLIOrchestration";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    path = General.GenerateFilePath(folderPath,ItemName);
                    AddOrUpdateReturnParamActual(ParamName: "Output logfile", ActualValue: path);
                }

                try
                {
                    string filePath = this.FilePath;
                    if(isVBSScript)
                    {
                        filePath = GetSystemDirectory() + $"{Path.DirectorySeparatorChar}cscript.exe";
                        if(!File.Exists(filePath))
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Error: Interpretor is not available for file {this.FilePath}");
                            Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                            return;
                        }
                    }
                    if (WaitForProcessToFinish)
                    {
                        if (ParseResult)
                        {
                            var cmd = Cli.Wrap(filePath)
                                .WithArguments(arguments.ToString()) | (PipeTarget.ToDelegate(parseRcwithDelimiter));
                            CommandResult Result = await cmd.ExecuteAsync();
                            UpdateActionStatus(Result);
                        }
                        else
                        {
                            var cmd = Cli.Wrap(filePath)
                                .WithArguments(arguments.ToString()) | PipeTarget.ToStringBuilder(DataBuffer);
                            CommandResult Result = await cmd.ExecuteAsync();
                            UpdateActionStatus(Result);
                        }
                        WriteTofile(path, DataBuffer);
                    }
                    else
                    {
                        var cmd = Cli.Wrap(filePath)
                            .WithArguments(arguments.ToString());
                        cmd.ExecuteAsync();
                    }                }
                catch(Exception ex)
                {
                    Error = "Error: during CLI Orchestration:" + ex.Message;
                    Reporter.ToLog(eLogLevel.ERROR, "Error: during CLI Orchestration", ex);
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    return;
                }
            }
        }
        private void UpdateActionStatus(CommandResult result)
        {
            if (result.ExitCode == 0)
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
            }
            else
            {
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            }
            ExInfo += $"ExitCode: {result.ExitCode}";
        }

        private void WriteTofile(string filepath,StringBuilder data)
        {
            using (var writer = new StreamWriter(filepath, true))
            {
                writer.WriteLine(data);
            }
        }

        private void parseRcwithDelimiter(string sRC)
        {
            DataBuffer.Append($"{sRC}{Environment.NewLine}");
            string[] RCValues = sRC.Split('\n');
            foreach (string RCValue in RCValues)
            {
                if (RCValue.Length > 0) // Ignore empty lines
                {
                    string Param;
                    string Value;
                    int i = RCValue.IndexOf(this.Delimiter);
                    if (i > 0)
                    {
                        Param = RCValue.Substring(0, i);
                        //the rest is the value
                        Value = RCValue.Substring(Param.Length + 1);
                        AddOrUpdateReturnParamActual(Param, Value);
                    }
                    
                }
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
    }
}
