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

    public class OptionsBase  // ' Base class for Options whihc execute run set
    {
        public enum eVerboseLevel
        {
            Debug,
            Info,
            Warning
        }

        [Option('v', "verbose", Required = false, HelpText = "Select Verbose level: Debug, Info, Warning")]
        public eVerboseLevel VerboseLevel { get; set; }
    }

        

    [Verb("example", HelpText = "Show example")]
    public class ExampleOptions  // 'ginger example -v run' display CLI examples for run
    {        
        [Option('b', "verb", Required = true, HelpText = "Select Verb to show example")]
        public string verb { get; set; }
    }

    [Verb("version", HelpText = "Show version information")]
    public class VersionOptions  // 'ginger version
    {
        
    }


    [Verb("run", HelpText = "execute run set")]
    public class RunOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<RunOptions>();

        [Option('s', "solution", Required = true, HelpText = "Set solution folder")]
        public string Solution { get; set; }


        [Option('r', "runset", Default = "Default Run Set", Required = true, HelpText = "Set runset name")]
        public string Runset { get; set; }

        [Option('e', "env",  Required = true, HelpText = "Set environment name")]
        public string Environment { get; set; }

        // causing issues so removing
        //[Opton(longName: "env", Required = false, HelpText = "Set environment name same like using --environment")]
        //public string Env { get { return Environment;  } set { Environment = value; } }


        [Option('a', "analyze", Required = false, HelpText = "runAnalyzer")]
        public bool RunAnalyzer { get; set; }
        
        [Option('u', "showui", Required = false, Default = false, HelpText = "Show Auto Run Window UI - Windows only")]
        public bool ShowAutoRunWindow { get; set; }

        [Option('t', "artifacts-path", Required = false, HelpText = "Select Artifacts output directory")]
        public string TestArtifactsPath { get; set; }
    }

    [Verb("scm", HelpText = "Execute source control management operations")]
    public class SCMOptions : OptionsBase
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
    public class GridOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<GridOptions>(); } }

        // TODO: set default
        [Option('p', "port", Required = false, HelpText = "Port to listen", Default = 15001)]
        public int Port { get; set; }


        [Option('t', "tracker", HelpText = "Show nodes tracker window")]
        public bool ShowAutoRunWindow { get; set; }
    }

    [Verb("configfile", HelpText = "Use config file")]
    public class ConfigFileOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ConfigFileOptions>(); } }        

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Config file path")]
        public string FileName { get; set; }

    }
    

    [Verb("script", HelpText = "Run script file")]
    public class ScriptOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<ScriptOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Script file path")]
        public string FileName { get; set; }

    }

    [Verb("dynamic", HelpText = "Run dynamic xml")]
    public class DynamicOptions : OptionsBase
    {
        public static string Verb { get { return CLIOptionClassHelper.GetClassVerb<DynamicOptions>(); } }

        [Option('f', CLIOptionClassHelper.FILENAME, Required = true, HelpText = "Dynamic xml file path")]
        public string FileName { get; set; }

    }


}
