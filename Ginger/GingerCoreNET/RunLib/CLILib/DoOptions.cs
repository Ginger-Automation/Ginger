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

using CommandLine;
using System;
using System.IO;
using System.Net;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    [Verb("do", HelpText = "Solution Operations like: analyze, clean and more for list run 'ginger help solution")]
    public class DoOptions:SourceControlOptions  // 'ginger do --operation analyze' run analyzer on solution
    {
        public enum DoOperation
        {
            analyze,
            info,
            clean,
            open,
            openSourceControl
        }

        [Option('o', "operation", Required = true, HelpText = "Select operation to run on solution")]
        public DoOperation Operation { get; set; }

        private string _solution;

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
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


        [Option('e', "encryptionKey", Required = true, HelpText = "Provide the solution encyrption key")]
        public string EncryptionKey { get; set; }


    }

}
