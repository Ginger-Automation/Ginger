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
    [Verb("scm", HelpText = "Execute source control management operations")]
    public class SCMOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<SCMOptions>();

        [Option('t', "type", Required = true, HelpText = "Source Control Management type i.e: GIT, SVN")]
        public SourceControlBase.eSourceControlType SCMType { get; set; }

        [Option('l', "url", Required = true, HelpText = "Source Control URL")]
        public string URL { get; set; }

        [Option('u', "user", Required = true, HelpText = "Source Control User")]
        public string User { get; set; }

        [Option('p', "pass", Required = true, HelpText = "Source Control Pass")]
        public string Pass { get; set; }

        [Option('e', "encrypted", Required = false, HelpText = "password is encrypted")]
        public bool Encrypted { get; set; }

        [Option('m', "encrypted-mechanism", Required = false, HelpText = "password decryption mechanism")]
        public bool DecryptMechanism { get; set; }

        [Option('s', "solution", Required = true, HelpText = "Local solution folder")]
        public string Solution { get; set; }

        public enum SCMOperation
        {
            Download,   // lower case!!!
            GetLatest
        }

        [Option('o', "operation", Required = true, HelpText = "SCM Operation")]
        public SCMOperation Operation { get; set; }

    }
}
