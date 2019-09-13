using CommandLine;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("run", HelpText = "Open Solution and execute run set")]
    public class RunOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<RunOptions>();

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Required = true, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "env", Required = false, Default = "Default", HelpText = "Set environment name")]
        public string Environment { get; set; }
        

        [Option('a', "analyze", Required = false, HelpText = "runAnalyzer")]
        public bool RunAnalyzer { get; set; }

        [Option('u', "showui", Required = false, Default = false, HelpText = "Show Auto Run Window UI - Windows only")]
        public bool ShowUI { get; set; }

        [Option('t', "artifacts-path", Required = false, HelpText = "Select Artifacts output directory")]
        public string TestArtifactsPath { get; set; }
    }
}
