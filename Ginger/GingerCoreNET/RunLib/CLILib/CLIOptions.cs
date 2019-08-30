using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("run", HelpText = "execute run set")]
    public class RunOptions
    {
        [Option('s', "solution", Required = false, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Required = false, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "environment", Required = false, HelpText = "Set environment name")]
        public string Environment { get; set; }


        [Option('a', "analyze", Required = false, HelpText = "runAnalyzer")]
        public bool RunAnalyzer { get; set; }


        [Option('w', "showAutoRunWindow", Required = false, HelpText = "showAutoRunWindow")]
        public bool ShowAutoRunWindow { get; set; }
    }

    [Verb("grid", HelpText = "Start Service Grid")]
    public class GridOptions
    {
        // TODO: set default
        [Option('p', "port", Required = false, HelpText = "Port to listen", Default = 15001)]
        public int Port { get; set; }


        [Option('t', "tracker", HelpText = "Show nodes tracker window")]
        public bool ShowAutoRunWindow { get; set; }
    }

    
}
