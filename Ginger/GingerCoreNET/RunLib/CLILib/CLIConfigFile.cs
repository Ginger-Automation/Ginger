#region License
/*
Copyright © 2014-2023 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCoreNET.SourceControl;
using System;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    [ObsoleteAttribute("User newer options", false)]
    public class CLIConfigFile : ICLI
    {
        bool ICLI.IsFileBasedConfig { get { return true; } }

        public string Verb
        {
            get
            {                
                return ConfigFileOptions.Verb;
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                return "Config";
            }
        }

        public async Task Execute(RunsetExecutor runsetExecutor)
        {            
            await runsetExecutor.RunRunset();            
        }


        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string sConfig = null;
            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {               
                sConfig = "SourceControlType=" + solution.SourceControl.GetSourceControlType.ToString() + Environment.NewLine;
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.SVN)//added for supporting Jenkins way of config creation- need to improve it
                {
                    string modifiedURI = solution.SourceControl.SourceControlURL.TrimEnd(new char[] { '/' });
                    int lastSlash = modifiedURI.LastIndexOf('/');
                    modifiedURI = (lastSlash > -1) ? modifiedURI.Substring(0, lastSlash) : modifiedURI;
                    sConfig += "SourceControlUrl=" + modifiedURI + Environment.NewLine;
                }
                else
                {
                    sConfig += "SourceControlUrl=" + solution.SourceControl.SourceControlURL + Environment.NewLine;
                }
                if (solution.SourceControl.SourceControlUser != null && solution.SourceControl.SourceControlPass != null)
                {
                    sConfig += "SourceControlUser=" + solution.SourceControl.SourceControlUser + Environment.NewLine;
                    sConfig += "SourceControlPassword=" + EncryptionHandler.EncryptwithKey(solution.SourceControl.SourceControlPass) + Environment.NewLine;
                    sConfig += "PasswordEncrypted=" + "Y" + Environment.NewLine;
                }
                else
                {
                    sConfig += "SourceControlUser=N/A" + Environment.NewLine;
                    sConfig += "SourceControlPassword=N/A" + Environment.NewLine;
                }
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
                {
                    if (solution.SourceControl.SourceControlProxyAddress != null && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                    {
                        sConfig += "SourceControlProxyServer=" + solution.SourceControl.SourceControlProxyAddress.ToString() + Environment.NewLine;
                        sConfig += "SourceControlProxyPort=" + solution.SourceControl.SourceControlProxyPort.ToString() + Environment.NewLine;
                    }
                }
            }
            sConfig += "solution=" + solution.Folder + Environment.NewLine;
            sConfig += "env=" + runsetExecutor.RunsetExecutionEnvironment.Name + Environment.NewLine;
            sConfig += "runset=" + runsetExecutor.RunSetConfig.Name + Environment.NewLine;
            sConfig += "analyze=" + cliHelper.RunAnalyzer.ToString() + Environment.NewLine;
            if (!string.IsNullOrEmpty(cliHelper.TestArtifactsFolder))
            {
                sConfig += "artifacts-path=" + cliHelper.TestArtifactsFolder + Environment.NewLine;
            }
            
            //OLD sConfig += "ShowAutoRunWindow=" + cliHelper.ShowAutoRunWindow.ToString() + Environment.NewLine;
            sConfig += CLIOptionClassHelper.GetAttrLongName<RunOptions>(nameof(RunOptions.ShowUI)) + "=" + cliHelper.ShowAutoRunWindow.ToString() + Environment.NewLine;

            return sConfig;
        }

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            cliHelper.ShowAutoRunWindow = true; // // default is true to keep backward compatibility
            using (System.IO.StringReader reader = new System.IO.StringReader(content))
            {
                string arg;
                while ((arg = reader.ReadLine()) != null)
                {
                    int i = arg.IndexOf('=');
                    string param = arg.Substring(0, i).Trim();
                    string value = arg.Substring(i + 1).Trim();

                    switch (param)
                    {
                        case "SourceControlType":
                            cliHelper.SetSourceControlType(value);
                            break;
                        case "SourceControlUrl":
                            cliHelper.SetSourceControlURL(value);                            
                            break;
                        case "SourceControlUser":
                            cliHelper.SetSourceControlUser(value);                            
                            break;
                        case "SourceControlPassword":
                            cliHelper.SetSourceControlPassword(value);                            
                            break;
                        case "PasswordEncrypted":
                            cliHelper.PasswordEncrypted(value);                            
                            break;
                        case "SourceControlProxyServer":
                            cliHelper.SourceControlProxyServer(value);                            
                            break;
                        case "SourceControlProxyPort":
                            cliHelper.SourceControlProxyPort(value);                            
                            break;
                        case "Solution":
                        case "solution":
                            cliHelper.Solution = value;                            
                            break;                                              
                        case "ShowAutoRunWindow": // Support old style
                        case "showui": // TODO: use CLIOptionClassHelper.GetAttrLongName<RunOptions>(nameof(RunOptions.ShowUI)):
                            cliHelper.ShowAutoRunWindow = bool.Parse(value);
                            break;                       
                        case "artifacts-path":
                            cliHelper.TestArtifactsFolder = value;
                            break;
                        //default:
                        //    Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + param + "'");
                        //    throw new ArgumentException("Unknown argument", param);
                    }
                }                
            }            
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            using (System.IO.StringReader reader = new System.IO.StringReader(content))
            {
                string arg;
                while ((arg = reader.ReadLine()) != null)
                {
                    int i = arg.IndexOf('=');
                    string param = arg.Substring(0, i).Trim();
                    string value = arg.Substring(i + 1).Trim();

                    switch (param)
                    {
                        case "RunSet":
                        case "runset":
                            cliHelper.Runset = value;
                            break;
                        case "Env":
                        case "env":
                            cliHelper.Env = value;
                            break;
                        case "RunAnalyzer":
                        case "analyze":
                            cliHelper.RunAnalyzer = bool.Parse(value);
                            break;
                        //default:
                        //    Reporter.ToLog(eLogLevel.ERROR, "Unknown argument: '" + param + "'");
                        //    throw new ArgumentException("Unknown argument", param);
                    }
                }
            }
        }
    }
}
