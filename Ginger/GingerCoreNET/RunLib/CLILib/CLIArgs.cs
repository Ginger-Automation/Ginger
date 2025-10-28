#region License
/*
Copyright © 2014-2025 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using CommandLine;
using Ginger.Configurations;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using System.Threading.Tasks;
namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIArgs : ICLI
    {
        bool ICLI.IsFileBasedConfig => false;

        string ICLI.Verb => RunOptions.Verb;


        string ICLI.FileExtension
        {
            get
            {
                return null;
            }
        }

        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            RunOptions options = new RunOptions
            {
                Solution = solution.Folder,
                Runset = runsetExecutor.RunSetConfig.Name,
                Environment = runsetExecutor.RunsetExecutionEnvironment.Name,
                DoNotAnalyze = !cliHelper.RunAnalyzer,
                ShowUI = cliHelper.ShowAutoRunWindow,
                TestArtifactsPath = cliHelper.TestArtifactsFolder,

                EncryptionKey = solution.EncryptionKey,

                URL = cliHelper.SourceControlURL,
                User = cliHelper.SourcecontrolUser,
                Pass = cliHelper.sourceControlPass,

                PasswordEncrypted = cliHelper.sourceControlPassEncrypted,
                SCMType = cliHelper.sourceControlType,

                SourceApplication = cliHelper.SourceApplication,
                SourceApplicationUser = cliHelper.SourceApplicationUser
            };
            if (string.IsNullOrEmpty(options.SourceApplication))
            {
                options.SourceApplication = "Ginger CLI";
            }
            if (string.IsNullOrEmpty(options.SourceApplicationUser))
            {
                options.SourceApplicationUser = System.Environment.UserName;
            }

            if (cliHelper.DownloadUpgradeSolutionFromSourceControl)
            {

                options.URL = solution.SourceControl.URL;
                options.User = solution.SourceControl.Username;

                options.Pass = EncryptionHandler.EncryptwithKey(solution.SourceControl.Password);

                options.PasswordEncrypted = true;
                options.SCMType = solution.SourceControl.GetSourceControlType;
                options.Branch = solution.SourceControl.Branch;
            }

            if (runsetExecutor.RunSetConfig.SelfHealingConfiguration.SaveChangesInSourceControl)
            {
                options.SelfHealingCheckInConfigured = true;
            }

          
            var args = CommandLine.Parser.Default.FormatCommandLine<RunOptions>(options);
            args = args.Replace(solution.EncryptionKey, "\"" + solution.EncryptionKey + "\"");
            if (options.PasswordEncrypted && !string.IsNullOrEmpty(options.Pass))
            {
                args = args.Replace(options.Pass, "\"" + options.Pass + "\"");
            }

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

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            cliHelper.SetSourceControlPassword(cliHelper.sourceControlPass);
            cliHelper.PasswordEncrypted(cliHelper.sourceControlPassEncrypted.ToString());
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
        }

        public async Task Execute(RunsetExecutor runsetExecutor)
        {
            await runsetExecutor.RunRunset();
        }


    }
}
