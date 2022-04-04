#region License
/*
Copyright © 2014-2022 European Support Limited

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
    [Verb("sealights", HelpText = "Sealights execution logger settings")]
    public class SealightsOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<SealightsOptions>();

        [Option('u', "sealights-url", Required = false, HelpText = "Set Sealights URL")]
        public string SealURL { get; set; }

        [Option('t', "sealights-agent-token", Required = false, HelpText = "Set Sealights Agent Token")]
        public string SealAgentToken { get; set; }

        [Option('b', "sealights-lab-id", Required = false, HelpText = "Set Sealights Lab ID")]
        public long SealLabID { get; set; }

        [Option('e', "sealights-session-id", Required = false, HelpText = "Set Sealights Session ID")]
        public long SealSessionID { get; set; }

        [Option('o', "sealights-session-timeout", Required = false, HelpText = "Set Sealights Session Time-out")]
        public long SealSessionTimeOut { get; set; }

        [Option('a', "sealights-test-stage", Required = false, HelpText = "Set Sealights Test Stage")]
        public string SealTestStage { get; set; }

        [Option('l', "sealights-entity-level", Required = false, HelpText = "Set Sealights Entity Level")]
        public ReportEntityLevel SealReportEntityLevel { get; set; }

        public enum ReportEntityLevel
        {
            Activity,
            ActivityGroup,
            BusinsessFLow
        }
    }

}
