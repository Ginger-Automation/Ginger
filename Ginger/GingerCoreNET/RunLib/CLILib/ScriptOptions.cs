using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{


    [Verb("script", HelpText = "Run script file")]
    public class ScriptOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ScriptOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Script file path")]
        public string FileName { get; set; }

    }
}
