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

using CommandLine;
using GingerCoreNET.SourceControl;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("run", HelpText = "Open Solution and execute run set")]
    public class RunOptions : SourceControlOptions
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

        [Option('i', "showui", Required = false, Default = false, HelpText = "Show Auto Run Window UI - Windows only")]
        public bool ShowUI { get; set; }

        [Option('a', "artifacts-path", Required = false, HelpText = "Select Artifacts output directory")]
        public string TestArtifactsPath { get; set; }

        [Option('q', "executionId", Required = false, HelpText = "Execution Id")]
        public string RunSetExecutionId { get; set; }

        [Option('k', "encryptionKey", Required = false, HelpText = "Encryption key password variables")]
        public string EncryptionKey { get; set; }

        [Option('b', "selfHealingCheckInConfigured", Required = false, HelpText = "SelfHealing setting for save and check-in changes.")]
        public bool SelfHealingCheckInConfigured { get; set; }

        // Gideon
        [Option("sealightsEnable", Required = false, HelpText = "Set Sealights Enable")]
        public bool SealightsEnable { get; set; }

        [Option("sealightsUrl", Required = false, HelpText = "Set Sealights URL")]
        public string SealightsUrl { get; set; }

        [Option("sealightsAgentToken", Required = false, HelpText = "Set Sealights Agent Token")]
        public string SealightsAgentToken { get; set; }

        [Option("sealightsLabId", Required = false, HelpText = "Set Sealights Lab ID")]
        public string SealightsLabID { get; set; }

        [Option("sealightsBSId", Required = false, HelpText = "Set Sealights Session ID")]
        public string SealightsSessionID { get; set; }

        [Option("sealightsSessionTimeout", Required = false, HelpText = "Set Sealights Session Time-out")]
        public string SealightsSessionTimeOut { get; set; }

        [Option("sealightsTestStage", Required = false, HelpText = "Set Sealights Test Stage")]
        public string SealightsTestStage { get; set; }

        [Option("sealightsEntityLevel", Required = false, HelpText = "Set Sealights Entity Level")]
        public string SealightsEntityLevel { get; set; }

        [Option("sealightsTestRecommendations", Required = false, HelpText = "Set Sealights Test Recommendations")]
        public bool SealightsTestRecommendations { get; set; }

        // Pravin
        [Option("ReRunFailed", Required = false, HelpText = "Set Rerun Configuration Enable")]
        public bool ReRunFailed { get; set; }

        [Option("ReferenceExecutionID", Required = false, HelpText = "Set Reference ExecutionID")]
        public string ReferenceExecutionID { get; set; }

        [Option("RerunLevel", Required = false, HelpText = "Set RerunLevel")]
        public string RerunLevel { get; set; }

        // Adding to support account level execution details
        [Option("sourceApplication", Required = false, HelpText = "Set Source Application name")]
        public string SourceApplication { get; set; }

        [Option("sourceApplicationUser", Required = false, HelpText = "Set Source Application username")]
        public string SourceApplicationUser { get; set; }

        [Option("useTempFolder", Required = false, HelpText = "Use a temporary folder for execution artifacts")]
        public bool UseTempFolder { get; set; }

       
    }

}
