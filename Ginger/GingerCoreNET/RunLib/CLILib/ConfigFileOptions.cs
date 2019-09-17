using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{   
    [Verb("configfile", HelpText = "Use config file")]
    public class ConfigFileOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ConfigFileOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Config file path")]
        public string FileName { get; set; }
    }
}
