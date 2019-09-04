#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Ginger.Run;
using Ginger.SolutionGeneral;

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
        
        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            RunOptions options = new RunOptions();
            options.Solution = solution.Folder;
            options.Runset = runsetExecutor.RunSetConfig.Name;
            options.Environment = runsetExecutor.RunsetExecutionEnvironment.Name;
            options.RunAnalyzer = cliHelper.RunAnalyzer;
            options.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;


            var args = CommandLine.Parser.Default.FormatCommandLine<RunOptions>(options);

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

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
       

        public void LoadContent(string args, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {      
            // NA

        //    //TODO: make -s --solution  work  but not -solution or -Solution !!!!!!!!!!!!!!!!!!!!!!!!!!!!

        //    List<Arg> argsList = SplitArgs(args);
            

        //    // - SeekOrigin -- split keep -

        //    foreach(Arg arg in argsList)
        //    {                             
        //        switch (arg.ArgName)
        //        {
        //            case "--sourceControlType":
        //                cliHelper.SetSourceControlType(arg.ArgValue);
        //                break;
        //            case "--sourceControlUrl":
        //                cliHelper.SetSourceControlURL(arg.ArgValue);
        //                break;
        //            case "--sourceControlUser":
        //                cliHelper.SetSourceControlUser(arg.ArgValue);
        //                break;
        //            case "--sourceControlPassword":
        //                cliHelper.SetSourceControlPassword(arg.ArgValue);
        //                break;
        //            case "--sourceControlPasswordEncrypted":
        //                cliHelper.PasswordEncrypted(arg.ArgValue);
        //                break;
        //            case "--sourceControlProxyServer":
        //                cliHelper.SourceControlProxyServer(arg.ArgValue);
        //                break;
        //            case "--sourceControlProxyPort":
        //                cliHelper.SourceControlProxyPort(arg.ArgValue);
        //                break;
        //            case "-s":
        //            case "--solution":
        //                cliHelper.Solution = arg.ArgValue;
        //                break;                     
        //            case "-e":
        //            case "--env":
        //            case "--environment":
        //                cliHelper.Env = arg.ArgValue;
        //                break;                        
        //            case "-r":
        //            case "--runset":
        //                cliHelper.Runset = arg.ArgValue;
        //                break;
        //            case "--runAnalyzer":
        //            case "--analyzer":
        //                cliHelper.RunAnalyzer = bool.Parse(arg.ArgValue);                        
        //                break;
        //            case "--showAutoRunWindow":
        //            case "--autoRunWindow":
        //                cliHelper.ShowAutoRunWindow = bool.Parse(arg.ArgValue);
        //                break;
        //            case "--testArtifactsFolder":
        //                cliHelper.TestArtifactsFolder = arg.ArgValue;
        //                break;
        //            default:
        //                Reporter.ToLog(eLogLevel.ERROR, "Unknown argument with '-' prefix: '" + arg + "'");
        //                throw new ArgumentException("Unknown argument: ", arg.ArgName);
        //        }
        //    }                
        }
       
      
    }
}
