#region License
/*
Copyright © 2014-2026 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{


    [Verb("dynamic", HelpText = "Run dynamic xml/json")]
    public class DynamicOptions : OptionsBase, IOptionValidation
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<DynamicOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Group = "source", HelpText = "Dynamic xml/json file path")]
        public string FileName { get; set; }

        [Option('u', "execConfigSourceUrl", Group = "source", HelpText = "URL to fetch Execution Configurations")]
        public Uri Url { get; set; }

        public bool Validate()
        {
            if (!string.IsNullOrEmpty(FileName) && Url != null)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Validation failed for '{DynamicOptions.Verb}' command-line arguments.");
                Reporter.ToLog(eLogLevel.ERROR, $"'{CLIOptionClassHelper.FILENAME}' & 'execConfigSourceUrl' are mutually exclusive.");
                return false;
            }

            return true;
        }
    }
}
