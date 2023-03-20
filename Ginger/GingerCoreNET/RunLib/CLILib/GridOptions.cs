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

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    [Verb("grid", HelpText = "Start Service Grid")]
    public class GridOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<GridOptions>(); } }

        // TODO: set default
        [Option('p', "port", Required = false, HelpText = "Port to listen", Default = 15001)]
        public int Port { get; set; }


        [Option('t', "tracker", HelpText = "Show nodes tracker window")]
        public bool Tracker { get; set; }
    }
}
