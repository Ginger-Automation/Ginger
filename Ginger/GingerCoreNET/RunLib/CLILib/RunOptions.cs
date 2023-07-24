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

using CommandLine;
using GingerCoreNET.SourceControl;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("run", HelpText = "Open Solution and execute run set")]
    public class RunOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<RunOptions>();

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }

        [Option('m', "branch", Required = false, HelpText = "Set solution source control branch")]
        public string Branch { get; set; }

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



        [Option('t', "type", Required = false, HelpText = "Source Control Management type i.e: GIT, SVN")]
        public SourceControlBase.eSourceControlType SCMType { get; set; }

        [Option('h', "url", Required = false, HelpText = "Source Control URL")]
        public string URL { get; set; }

        [Option('u', "user", Required = false, HelpText = "Source Control User")]
        public string User { get; set; }

        [Option('p', "pass", Required = false, HelpText = "Source Control Pass")]
        public string Pass { get; set; }

        [Option('g', "encrypted", Required = false, Default = false, HelpText = "password is encrypted")]
        public bool PasswordEncrypted { get; set; }

        [Option('c', "ignoreCertificate", Required = false, HelpText = "Ignore certificate errors while cloning solution")]
        public bool ignoreCertificate { get; set; }

        [Option('g', "useScmShell", Required = false, HelpText = "Use shell Git Client")]
        public bool useScmShell { get; set; }

        [Option('z', "sourceControlProxyServer", Required = false, HelpText = "Proxy for Source cotnrol URL")]
        public string SourceControlProxyServer { get; set; }

        [Option('x', "sourceControlProxyPort", Required = false, HelpText = "Proxy port")]
        public string SourceControlProxyPort { get; set; }

        [Option('q', "executionId", Required = false, HelpText = "Execution Id")]
        public string RunSetExecutionId { get; set; }

        [Option('k', "encryptionKey", Required = false, HelpText = "Encryption key password vairables")]
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

    }

}
