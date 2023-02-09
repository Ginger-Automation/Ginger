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
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    
    [Verb("do", HelpText = "Solution Operations like: analyze, clean and more for list run 'ginger help solution")]
    public class DoOptions  // 'ginger do --operation analyze' run analyzer on solution
    {
        public enum DoOperation
        {
            analyze,
            info,
            clean
        }
               
        [Option('o', "operation", Required = true, HelpText = "Select operation to run on solution")]
        public DoOperation Operation { get; set; }

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }

    }

}
