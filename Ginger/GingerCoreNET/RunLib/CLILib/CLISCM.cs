using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using CommandLine;
using Ginger.Run;
using Ginger.SolutionGeneral;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLISCM : ICLI
    {
        bool ICLI.IsFileBasedConfig => false;

        string ICLI.Verb => SCMOptions.Verb;


        string ICLI.FileExtension
        {
            get
            {
                return null;
            }
        }

        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            SCMOptions options = new SCMOptions();
            //options.Solution = solution.Folder;
            //options.Runset = runsetExecutor.RunSetConfig.Name;
            //options.Environment = runsetExecutor.RunsetExecutionEnvironment.Name;
            //options.RunAnalyzer = cliHelper.RunAnalyzer;
            //options.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;


            var args = CommandLine.Parser.Default.FormatCommandLine<SCMOptions>(options);

            // !!!!!!!!!!!!!!!!!!!
            // TODO: we want to move SCM to another verb/action !!!!!

            //if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            //{
            //    Args += string.Format(" --sourceControlType {0}" , solution.SourceControl.GetSourceControlType.ToString());
            //    if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.SVN)//added for supporting Jenkins way of config creation- need to improve it
            //    {
            //        string modifiedURI = solution.SourceControl.SourceControlURL.TrimEnd(new char[] { '/' });
            //        int lastSlash = modifiedURI.LastIndexOf('/');
            //        modifiedURI = (lastSlash > -1) ? modifiedURI.Substring(0, lastSlash) : modifiedURI;
            //        Args += string.Format(" --sourceControlUrl {0}", modifiedURI);
            //    }
            //    else
            //    {
            //        Args += string.Format(" --sourceControlUrl {0}", solution.SourceControl.SourceControlURL);
            //    }
            //    Args += string.Format(" --sourceControlUser {0}" , solution.SourceControl.SourceControlUser);
            //    Args += string.Format(" --sourceControlPassword {0}" , EncryptionHandler.EncryptwithKey(solution.SourceControl.SourceControlPass));
            //    Args += string.Format(" --sourceControlPasswordEncrypted {0}" , "Y");
            //    if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
            //    {
            //        Args += string.Format(" --sourceControlProxyServer {0}" , solution.SourceControl.SourceControlProxyAddress.ToString());
            //        Args += string.Format(" --sourceControlProxyPort {0}" , solution.SourceControl.SourceControlProxyPort.ToString());
            //    }
            //}

            return args;
        }

        public void Download(SCMOptions scmOptions)
        {
            WorkSpace.Instance.UserProfile.SourceControlURL = scmOptions.URL;

            // TODO: need to update all other options but the download below use userprofile... TBD
            // SourceControlIntegration.DownloadSolution(scmOptions.Solution);

            Reporter.ToLog(eLogLevel.ERROR, "SCM options are not implemented yet");
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            // NA            
        }


        public void LoadContent(string args, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
                        
        }
    }
}
