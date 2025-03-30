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
using Amdocs.Ginger.CoreNET.RunLib.CLILib.Interfaces;
using CommandLine;
using System;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    [Verb("do", HelpText = "Solution Operations like: open, analyze, info and more 'ginger help solution")]
    public class DoOptions : SourceControlOptions, IOptionValidation  // 'ginger do --operation analyze' run analyzer on solution
    {
        public enum DoOperation
        {
            analyze,
            info,
            clean,
            open
        }

        [Option('o', "operation", Required = true, HelpText = "Select operation to run on solution")]
        public DoOperation Operation { get; set; }

        private string _solution;

        [Option('s', "solution", Group = "solution path", HelpText = "Provide solution folder path")]
        public string Solution
        {
            get => _solution;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                if (value.IndexOf("Ginger.Solution.xml", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    value = Path.GetDirectoryName(value)?.Trim() ?? string.Empty;

                }
                _solution = value;
            }
        }


        [Option("encryptionKey", Required = false, HelpText = "(Optional) Encryption key of your solution.")]
        public string EncryptionKey { get; set; }

        [Option("savecredentials", Required = false, Default = false, HelpText = "(Optional) To save the Credentials")]
        public bool SaveCredentials { get; set; }

        [Option("executionId", Required = false, HelpText = "(Optional) Id of a RunSet execution.")]
        public string ExecutionId { get; set; }

        [Option("runSetId", Required = false, HelpText = "(Optional) Id of the RunSet to be opened.")]
        public string RunSetId { get; set; }

        [Option("runSetName", Required = false, HelpText = "(Optional) Name of the RunSet to be opened.")]
        public string RunSetName { get; set; }

        [Option("businessFlowId", Required = false, HelpText = "(Optional) Id of the Business Flow to be opened.")]
        public string BusinessFlowId { get; set; }

        [Option("businessFlowName", Required = false, HelpText = "(Optional) Name of the Business Flow to be opened.")]
        public string BusinessFlowName { get; set; }

        [Option("sharedActivityName", Required = false, HelpText = "(Optional) Name of the Shared Repository Activity to be opened.")]
        public string SharedActivityName { get; set; }

        [Option("sharedActivityId", Required = false, HelpText = "(Optional) Id of the Shared Repository Activity to be opened.")]
        public string SharedActivityId { get; set; }

        [Option("useTempFolder", Group = "solution path", HelpText = "(Optional) Use the Temp folder as the solution folder path.")]
        public bool UseTempFolder { get; set; }

        [Option("executionConfigSourceUrl", Required = false, HelpText = "(Optional) URL to fetch the execution configurations from.")]
        public Uri ExecutionConfigurationSourceUrl { get; set; }

        public bool Validate()
        {
            if (UseTempFolder && !string.IsNullOrEmpty(Solution))
            {
                Reporter.ToLog(eLogLevel.ERROR, $"'solution' & 'useTempFolder' are mutually exclusive.");
                return false;
            }

            return true;
        }
    }

}
