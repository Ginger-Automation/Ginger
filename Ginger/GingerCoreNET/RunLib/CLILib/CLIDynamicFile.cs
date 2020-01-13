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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Run;
using Ginger.SolutionGeneral;
using System;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIDynamicFile : ICLI
    {
        public enum eFileType { XML,JSON}

        bool ICLI.IsFileBasedConfig { get { return true; } }

        public string Verb
        {
            get
            {
                return DynamicOptions.Verb;
            }
        }

        string ICLI.FileExtension
        {
            get
            {
                if (FileType == eFileType.JSON)
                {
                    return "json";
                }
                else
                {
                    return "xml";
                }
            }
        }

        public eFileType FileType { get; set; }

        public CLIDynamicFile(eFileType fileType)
        {
            FileType = fileType;
        }

        public string CreateConfigurationsContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            if (FileType == eFileType.JSON)
            {
                string json = DynamicExecutionManager.CreateDynamicRunSetJSON(solution, runsetExecutor, cliHelper);
                return json;
            }
            else
            {
                string xml = DynamicExecutionManager.CreateDynamicRunSetXML(solution, runsetExecutor, cliHelper);
                return xml;
            }
        }

        public void LoadGeneralConfigurations(string content, CLIHelper cliHelper)
        {
            if(FileType == eFileType.JSON)
            {
                //Dynamic JSON
                GingerExecConfig exeConfiguration = DynamicExecutionManager.LoadDynamicExecutionFromJSON(content);
                if (exeConfiguration.SolutionScmDetails != null)
                {
                    cliHelper.SetSourceControlType(exeConfiguration.SolutionScmDetails.SCMType.ToString());
                    cliHelper.SetSourceControlURL(exeConfiguration.SolutionScmDetails.SolutionRepositoryUrl);
                    cliHelper.SetSourceControlUser(exeConfiguration.SolutionScmDetails.User);
                    cliHelper.SetSourceControlPassword(exeConfiguration.SolutionScmDetails.Password);
                    cliHelper.PasswordEncrypted(exeConfiguration.SolutionScmDetails.PasswordEncrypted.ToString());
                    if (string.IsNullOrEmpty(exeConfiguration.SolutionScmDetails.ProxyServer) == false)
                    {
                        cliHelper.SourceControlProxyServer(exeConfiguration.SolutionScmDetails.ProxyServer);
                        cliHelper.SourceControlProxyPort(exeConfiguration.SolutionScmDetails.ProxyPort);
                    }
                }
                if (!string.IsNullOrEmpty(exeConfiguration.SolutionLocalPath))
                {
                    if (System.IO.Directory.Exists(exeConfiguration.SolutionLocalPath))
                    {
                        cliHelper.Solution = exeConfiguration.SolutionLocalPath;
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.INFO, string.Format("Solution local path: '{0}' was not found so creating it", exeConfiguration.SolutionLocalPath));
                        try
                        {
                            System.IO.Directory.CreateDirectory(exeConfiguration.SolutionLocalPath);
                            cliHelper.Solution = exeConfiguration.SolutionLocalPath;
                        }
                        catch(Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Falied to create the Solution local path: '{0}'", exeConfiguration.SolutionLocalPath), ex);
                        }
                    }
                    if (cliHelper.Solution == null)
                    {

                    }
                }

                if (exeConfiguration.ShowAutoRunWindow != null && exeConfiguration.ShowAutoRunWindow == true)
                {
                    cliHelper.ShowAutoRunWindow = true;
                }
                if (!string.IsNullOrEmpty(exeConfiguration.ArtifactsPath))
                {
                    cliHelper.TestArtifactsFolder = exeConfiguration.ArtifactsPath;
                }                
            }
            else
            {
                //Dynamic XML
                DynamicGingerExecution dynamicExecution = DynamicExecutionManager.LoadDynamicExecutionFromXML(content);
                if (dynamicExecution.SolutionDetails.SourceControlDetails != null)
                {
                    cliHelper.SetSourceControlType(dynamicExecution.SolutionDetails.SourceControlDetails.Type);
                    cliHelper.SetSourceControlURL(dynamicExecution.SolutionDetails.SourceControlDetails.Url);
                    cliHelper.SetSourceControlUser(dynamicExecution.SolutionDetails.SourceControlDetails.User);
                    cliHelper.SetSourceControlPassword(dynamicExecution.SolutionDetails.SourceControlDetails.Password);
                    cliHelper.PasswordEncrypted(dynamicExecution.SolutionDetails.SourceControlDetails.PasswordEncrypted);
                    if (string.IsNullOrEmpty(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer) == false)
                    {
                        cliHelper.SourceControlProxyServer(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer);
                        cliHelper.SourceControlProxyPort(dynamicExecution.SolutionDetails.SourceControlDetails.ProxyPort);
                    }
                }
                cliHelper.Solution = dynamicExecution.SolutionDetails.Path;
                cliHelper.ShowAutoRunWindow = dynamicExecution.ShowAutoRunWindow;               
            }
        }

        public void LoadRunsetConfigurations(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            if (FileType == eFileType.JSON)
            {
                //Dynamic JSON
                GingerExecConfig exeConfiguration = DynamicExecutionManager.LoadDynamicExecutionFromJSON(content);
                RunsetExecConfig runset = exeConfiguration.Runset;
                cliHelper.Env = runset.Environment;
                if (runset.RunAnalyzer != null)
                {
                    cliHelper.RunAnalyzer = (bool)runset.RunAnalyzer;
                }
                DynamicExecutionManager.CreateUpdateRunSetFromJSON(runsetExecutor, runset);
            }
            else
            {
                //Dynamic XML
                DynamicGingerExecution dynamicExecution = DynamicExecutionManager.LoadDynamicExecutionFromXML(content);
                AddRunset addRunset = dynamicExecution.AddRunsets[0];//for now supporting only one Run set execution
                cliHelper.Env = addRunset.Environment;
                cliHelper.RunAnalyzer = addRunset.RunAnalyzer;
                DynamicExecutionManager.CreateRunSetFromXML(runsetExecutor, addRunset);
            }
        }

        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
    }
}
