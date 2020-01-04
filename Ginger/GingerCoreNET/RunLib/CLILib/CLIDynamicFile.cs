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

using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerExecuterService.Contracts.V1.ExecutionConfigurations;

namespace Amdocs.Ginger.CoreNET.RunLib.CLILib
{
    public class CLIDynamicFile : ICLI
    {
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
                return "xml";
            }
        }

        public string CreateContent(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            string xml = DynamicExecutionManager.CreateDynamicRunSetXML(solution, runsetExecutor, cliHelper);
            return xml;            
        }


        public void LoadContent(string content, CLIHelper cliHelper, RunsetExecutor runsetExecutor)
        {
            if(DynamicExecutionManager.IsJson(content))
            {
                //Dynamic JSON
                ExecutionConfiguration exeConfiguration = DynamicExecutionManager.LoadDynamicExecutionFromJSON(content);
                if (exeConfiguration.SolutionDetails.SourceControlDetails != null)
                {
                    cliHelper.SetSourceControlType(exeConfiguration.SolutionDetails.SourceControlDetails.SCMType);
                    cliHelper.SetSourceControlURL(exeConfiguration.SolutionDetails.SourceControlDetails.SolutionRepositoryUrl);
                    cliHelper.SetSourceControlUser(exeConfiguration.SolutionDetails.SourceControlDetails.User);
                    cliHelper.SetSourceControlPassword(exeConfiguration.SolutionDetails.SourceControlDetails.Password);
                    cliHelper.PasswordEncrypted(exeConfiguration.SolutionDetails.SourceControlDetails.PasswordEncrypted.ToString());
                    if (string.IsNullOrEmpty(exeConfiguration.SolutionDetails.SourceControlDetails.ProxyServer) == false)
                    {
                        cliHelper.SourceControlProxyServer(exeConfiguration.SolutionDetails.SourceControlDetails.ProxyServer);
                        cliHelper.SourceControlProxyPort(exeConfiguration.SolutionDetails.SourceControlDetails.ProxyPort);
                    }
                }
                cliHelper.Solution = exeConfiguration.SolutionDetails.LocalPath;
                cliHelper.ShowAutoRunWindow = exeConfiguration.ShowAutoRunWindow;

                Runset runset = exeConfiguration.Runset;
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

                AddRunset addRunset = dynamicExecution.AddRunsets[0];//for now supporting only one Run set execution
                cliHelper.Env = addRunset.Environment;
                cliHelper.RunAnalyzer = addRunset.RunAnalyzer;

                DynamicExecutionManager.CreateRealRunSetFromXML(runsetExecutor, addRunset);
            }
        }



        public void Execute(RunsetExecutor runsetExecutor)
        {
            runsetExecutor.RunRunset();
        }
    }
}
