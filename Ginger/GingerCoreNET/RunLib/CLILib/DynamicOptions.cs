using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{


    [Verb("dynamic", HelpText = "Run dynamic xml")]
    public class DynamicOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<DynamicOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Dynamic xml file path")]
        public string FileName { get; set; }

    }
}
