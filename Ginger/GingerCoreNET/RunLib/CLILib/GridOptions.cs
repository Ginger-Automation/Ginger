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
