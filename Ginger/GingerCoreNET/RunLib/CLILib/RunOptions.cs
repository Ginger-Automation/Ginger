#region License
/*
Copyright © 2014-2021 European Support Limited

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

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("run", HelpText = "Open Solution and execute run set")]
    public class RunOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<RunOptions>();

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Required = true, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "env", Required = false, Default = "Default", HelpText = "Set environment name")]
        public string Environment { get; set; }
        

        [Option('d', "do-not-analyze", Required = false, HelpText = "runAnalyzer")]
        public bool DoNotAnalyze { get; set; }

        [Option('u', "showui", Required = false, Default = false, HelpText = "Show Auto Run Window UI - Windows only")]
        public bool ShowUI { get; set; }

        [Option('t', "artifacts-path", Required = false, HelpText = "Select Artifacts output directory")]
        public string TestArtifactsPath { get; set; }
    }
}
