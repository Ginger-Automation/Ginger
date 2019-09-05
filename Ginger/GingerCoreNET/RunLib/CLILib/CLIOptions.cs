using System;
using System.Reflection;
using CommandLine;
using GingerCoreNET.SourceControl;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{

    class CLIOptionClassHelper
    {
        public const string FILENAME = "filename";

        public static string GetClassVerb<T>()
        {
            VerbAttribute verbAttr = typeof(T).GetCustomAttribute<VerbAttribute>();
            return verbAttr.Name;
        }

        public static string GetAttrShorName<T>(string name)
        {
            OptionAttribute attrOption = typeof(T).GetProperty(name).GetCustomAttribute<OptionAttribute>();
            return attrOption.ShortName;
        }
    }

    public class BasicOptions  // without verb
    {
        //[Option('v', "version", Required = false, HelpText = "Display version information")]
        //public bool ShowVersion { get; set; }

        [Option('e', "examples", Required = false, HelpText = "Display example list of Command Line arguments")]
        public bool ShowExamples { get; set; }
    }


    [Verb("run", HelpText = "execute run set")]
    public class RunOptions
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<RunOptions>();

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Default = "Default Run Set", Required = true, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "environment",  Required = true, HelpText = "Set environment name")]
        public string Environment { get; set; }


        [Option('a', "analyze", Required = false, HelpText = "runAnalyzer")]
        public bool RunAnalyzer { get; set; }

        // !!!! long switch not ok
        [Option('w', "showAutoRunWindow", Required = false, Default = true, HelpText = "showAutoRunWindow")]
        public bool ShowAutoRunWindow { get; set; }
    }

    [Verb("scm", HelpText = "Execute source control management operations")]
    public class SCMOptions
    {
        public static string Verb =>  CLIOptionClassHelper.GetClassVerb<SCMOptions>(); 

        [Option('t', "type", Required = true, HelpText = "Source Control Management type i.e: GIT, SVN")]
        public SourceControlBase.eSourceControlType SCMType { get; set; }

        [Option('l', "url", Required = true, HelpText = "Source Control URL")]
        public string URL { get; set; }

        [Option('u', "user", Required = true, HelpText = "Source Control User")]
        public string User { get; set; }

        [Option('p', "pass", Required = true, HelpText = "Source Control Pass")]
        public string Pass { get; set; }

        [Option('e', "encrypted", Required = false, HelpText = "password is encrypted")]
        public bool Encrypted { get; set; }

        [Option('m', "encrypted-mechanism", Required = false, HelpText = "password decryption mechanism")]
        public bool DecryptMechanism { get; set; }

        public enum SCMOperation
        {
            Download,   // lower case!!!
            GetLatest
        }

        [Option('o', "operation", Required = true, HelpText = "SCM Operation")]
        public SCMOperation Operation { get; set; }

    }


    [Verb("grid", HelpText = "Start Service Grid")]
    public class GridOptions
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<GridOptions>(); } }

        // TODO: set default
        [Option('p', "port", Required = false, HelpText = "Port to listen", Default = 15001)]
        public int Port { get; set; }


        [Option('t', "tracker", HelpText = "Show nodes tracker window")]
        public bool ShowAutoRunWindow { get; set; }
    }

    [Verb("configfile", HelpText = "Use config file")]
    public class ConfigFileOptions
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ConfigFileOptions>(); } }        

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Config file path")]
        public string FileName { get; set; }

    }
    

    [Verb("script", HelpText = "Run script file")]
    public class ScriptOptions
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ScriptOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Script file path")]
        public string FileName { get; set; }

    }

    [Verb("dynamic", HelpText = "Run dynmaic xml")]
    public class DynamicOptions
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<DynamicOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Dynamic xml file path")]
        public string FileName { get; set; }

    }


}
