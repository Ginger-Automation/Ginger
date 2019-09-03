using CommandLine;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class BasicOptions  // without verb
    {
        //[Option('v', "version", Required = false, HelpText = "Display version information")]
        //public bool ShowVersion { get; set; }


        // !!!!!!!!!!!!!!!!!!!!!!!
        [Option('t', "test", Required = false, HelpText = "Test 123")]
        public bool Test { get; set; }
    }


    [Verb("run", HelpText = "execute run set")]
    public class RunOptions
    {
        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Required = true, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "environment", Required = true, HelpText = "Set environment name")]
        public string Environment { get; set; }


        [Option('a', "analyze", Required = false, HelpText = "runAnalyzer")]
        public bool RunAnalyzer { get; set; }

        // !!!! long switch not ok
        [Option('w', "showAutoRunWindow", Required = false, Default = true, HelpText = "showAutoRunWindow")]
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

    [Verb("configfile", HelpText = "Use config file")]
    public class ConfigFileOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Config file path")]
        public string FileName { get; set; }

    }

    [Verb("script", HelpText = "Run script file")]
    public class ScriptOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Script file path")]
        public string FileName { get; set; }

    }

    [Verb("dynamic", HelpText = "Run dynmaic xml")]
    public class DynamicOptions
    {
        [Option('f', "filename", Required = true, HelpText = "Dynamic xml file path")]
        public string FileName { get; set; }

    }


}
