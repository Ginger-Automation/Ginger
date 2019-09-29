using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class OptionsBase // ' Base class for Options which execute run set
    {
        public enum eVerboseLevel
        {
            normal,
            debug
        }

        [Option('v', "verbose", Required = false, Default = eVerboseLevel.normal, HelpText = "Select Verbose level: normal, debug")]
        public eVerboseLevel VerboseLevel { get; set; }
    }
}
