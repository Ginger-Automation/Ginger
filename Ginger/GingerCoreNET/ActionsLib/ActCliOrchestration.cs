using GingerCore.Actions;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Applitools.Utils;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GingerCore.Actions.ActCLIOrchestration;
using CliWrap;
using DocumentFormat.OpenXml.Office.Word;
using CliWrap.Buffered;
using CliWrap.EventStream;
using System.Text.RegularExpressions;
using System.IO;
using Applitools;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using System.Diagnostics;
using GingerExternal;

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

        public override string ActionUserDescription { get { return "Perform File operations like Check if File Exists, Execute a file "; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action to perform File operations ");

        }

        public new static partial class Fields
        {
            public static string SourceFilePath = "SourceFilePath";
            public static string FileOperationMode = "FileOperationMode";
            public static string DestinationFolder = "DestinationFolder";
        }

        [IsSerializedForLocalRepository]
        public string ScriptInterpreter { get; set; }

        [IsSerializedForLocalRepository]
        public string ScriptPath { get; set; }

        [IsSerializedForLocalRepository]
        public bool WaitForProcess { get; set; }

        [IsSerializedForLocalRepository]
        public bool ParseResult { get; set; }

        [IsSerializedForLocalRepository]
        public string Delimiter { get; set; }

        String DataBuffer;
        public override async void Execute()
        {
            DataBuffer = string.Empty;
            if (ParseResult && string.IsNullOrEmpty(ValueExpression.Calculate(Delimiter)))
            {
                Error = "Delimiter is Empty";
                return;
            }

            if(WaitForProcess)
            {
                await ExecuteCliProcess();
            }
            else
            {
               ExecuteCliProcess();
            }
        }

        private async Task ExecuteCliProcess()
        {
            StringBuilder argumentsstring = new StringBuilder();
            foreach (var p in this.InputValues)
            {
                argumentsstring.Append(p.Param).Append(" ");
                if (!string.IsNullOrEmpty(p.ValueForDriver))
                {
                    argumentsstring.Append(p.ValueForDriver).Append(" ");
                }
            }

            if (!string.IsNullOrEmpty(this.ScriptInterpreter))
            {
                string path = String.Empty;
                Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                if (WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder != null)
                {
                    string folderPath = $"{WorkSpace.Instance.Solution.Folder}{Path.DirectorySeparatorChar}Documents{Path.DirectorySeparatorChar}CLIOrchestration";
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    string DatetimeFormate = DateTime.Now.ToString("ddMMyyyy_HHmmssfff");
                    string Filename = $"{ItemName}_{DatetimeFormate}.txt";
                    path = $"{folderPath}{Path.DirectorySeparatorChar}{Filename}";
                    AddOrUpdateReturnParamActual(ParamName: "CLIOrchestration LogFile", ActualValue: path);
                }

                var cmd = Cli.Wrap(this.ScriptInterpreter)
                                .WithArguments(argumentsstring.ToString()) | (PipeTarget.ToDelegate(parseRcwithDelimiter));
                try
                {
                    var Result = await cmd.ExecuteAsync();
                    if (Result.ExitCode == 0)
                    {
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                    }
                    else
                    {
                        Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    }
                    WriteTofile(path, DataBuffer);
                    ExInfo += $"ExitCode: {Result.ExitCode}";
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Action failed due some invalid arguments", ex);
                    Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    ExInfo += $"ExitCode: 1 { ex.Message}";
                }

            }
        }

        private void WriteTofile(string filepath,string data)
        {
            using (var writer = new StreamWriter(filepath, true))
            {
                writer.WriteLine(data);
            }
        }

        private void parseRcwithDelimiter(string sRC)
        {
            DataBuffer += $"{sRC}{Environment.NewLine}";
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
    }
}
