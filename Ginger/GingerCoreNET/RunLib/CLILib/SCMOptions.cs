using CommandLine;
using GingerCoreNET.SourceControl;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    [Verb("scm", HelpText = "Execute source control management operations")]
    public class SCMOptions : OptionsBase
    {
        public static string Verb => CLIOptionClassHelper.GetClassVerb<SCMOptions>();

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

        [Option('s', "solution", Required = true, HelpText = "Local solution folder")]
        public string Solution { get; set; }

        public enum SCMOperation
        {
            Download,   // lower case!!!
            GetLatest
        }

        [Option('o', "operation", Required = true, HelpText = "SCM Operation")]
        public SCMOperation Operation { get; set; }

    }
}
