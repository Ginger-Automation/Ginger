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
using GingerCoreNET.SourceControl;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class SourceControlOptions : OptionsBase
    {

        [Option('m', "branch", Required = false, HelpText = "Set solution source control branch")]
        public string Branch { get; set; }

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

        [Option('z', "sourceControlProxyServer", Required = false, HelpText = "Proxy for Source control URL")]
        public string SourceControlProxyServer { get; set; }

        [Option('x', "sourceControlProxyPort", Required = false, HelpText = "Proxy port")]
        public string SourceControlProxyPort { get; set; }
    }

}
