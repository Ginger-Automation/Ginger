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
