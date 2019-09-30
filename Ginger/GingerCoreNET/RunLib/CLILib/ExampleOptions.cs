using CommandLine;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("example", HelpText = "Show example")]
    public class ExampleOptions  // 'ginger example -v run' display CLI examples for run
    {
        [Option('b', "verb", Required = true, HelpText = "Select Verb to show example")]
        public string verb { get; set; }
    }
}
