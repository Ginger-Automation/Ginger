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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.Reports;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;
using Ginger.Configurations;
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
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
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
                GingerExecConfig exeConfiguration = DynamicExecutionManager.DeserializeDynamicExecutionFromJSON(content);
                cliHelper.SetEncryptionKey(exeConfiguration.EncryptionKey);
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

                    if (exeConfiguration.SolutionScmDetails.UndoSolutionLocalChanges != null)
                    {
                        cliHelper.UndoSolutionLocalChanges = (bool)exeConfiguration.SolutionScmDetails.UndoSolutionLocalChanges;
                    }
                    cliHelper.SetSourceControlBranch(exeConfiguration.SolutionScmDetails.Branch);

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
                }

                if (exeConfiguration.ShowAutoRunWindow != null && exeConfiguration.ShowAutoRunWindow == true)
                {
                    cliHelper.ShowAutoRunWindow = true;
                }
                if (!string.IsNullOrEmpty(exeConfiguration.ArtifactsPath))
                {
                    cliHelper.TestArtifactsFolder = exeConfiguration.ArtifactsPath;
                }
                if (exeConfiguration.VerboseLevel != null)
                {
                   CLIProcessor.SetVerboseLevel((OptionsBase.eVerboseLevel)Enum.Parse(typeof(OptionsBase.eVerboseLevel), exeConfiguration.VerboseLevel.ToString(), true));
                }
                if (exeConfiguration.SealightsDetails != null)
                {
                    cliHelper.SetSealightsEnable(exeConfiguration.SealightsDetails.SealightsEnable);
                    cliHelper.SetSealightsUrl(exeConfiguration.SealightsDetails.SealightsUrl);
                    cliHelper.SetSealightsAgentToken(exeConfiguration.SealightsDetails.SealightsAgentToken);
                    cliHelper.SetSealightsBuildSessionID(exeConfiguration.SealightsDetails.SealightsBSId);
                    cliHelper.SetSealightsLabID(exeConfiguration.SealightsDetails.SealightsLabId);
                    cliHelper.SetSealightsSessionTimeout(exeConfiguration.SealightsDetails.SealightsSessionTimeout);
                    cliHelper.SetSealightsTestStage(exeConfiguration.SealightsDetails.SealightsTestStage);
                    cliHelper.SetSealightsEntityLevel(exeConfiguration.SealightsDetails.SealightsEntityLevel);
                    cliHelper.SetSealightsTestRecommendations(exeConfiguration.SealightsDetails.SealightsTestRecommendations);
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
                GingerExecConfig exeConfiguration = DynamicExecutionManager.DeserializeDynamicExecutionFromJSON(content);
                RunsetExecConfig runset = exeConfiguration.Runset;
                if (exeConfiguration.AlmsDetails != null && exeConfiguration.AlmsDetails.Count > 0)
                {
                    LoadALMDetailsFromJSON(exeConfiguration);
                }
                if (exeConfiguration.SealightsDetails != null)
                {
                   LoadSealightsDetailsFromJSON(exeConfiguration);
                }
                if (runset.EnvironmentName != null || runset.EnvironmentID != null)
                {
                    ProjEnvironment env = DynamicExecutionManager.FindItemByIDAndName<ProjEnvironment>(
                                    new Tuple<string, Guid?>(nameof(ProjEnvironment.Guid), runset.EnvironmentID),
                                    new Tuple<string, string>(nameof(ProjEnvironment.Name), runset.EnvironmentName),
                                    WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>());
                    if (env != null)
                    {
                        cliHelper.Env = env.Name;
                    }
                }
                if (runset.RunAnalyzer != null)
                {
                    cliHelper.RunAnalyzer = (bool)runset.RunAnalyzer;
                }
                DynamicExecutionManager.CreateUpdateRunSetFromJSON(runsetExecutor, exeConfiguration);
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

        public async Task Execute(RunsetExecutor runsetExecutor)
        {
            await runsetExecutor.RunRunset();
        }

        
        private void LoadSealightsDetailsFromJSON(GingerExecConfig gingerExecConfig)
        {
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLog = (bool)gingerExecConfig.SealightsDetails.SealightsEnable ? SealightsConfiguration.eSealightsLog.Yes : SealightsConfiguration.eSealightsLog.No;
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsLabId = gingerExecConfig.SealightsDetails.SealightsLabId;
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID = gingerExecConfig.SealightsDetails.SealightsBSId;
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsTestStage = gingerExecConfig.SealightsDetails.SealightsTestStage;
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsSessionTimeout = gingerExecConfig.SealightsDetails.SealightsSessionTimeout.ToString();
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsReportedEntityLevel = (SealightsConfiguration.eSealightsEntityLevel)gingerExecConfig.SealightsDetails.SealightsEntityLevel;
            WorkSpace.Instance.Solution.SealightsConfiguration.SealightsAgentToken = gingerExecConfig.SealightsDetails.SealightsAgentToken;
        }

        private void LoadALMDetailsFromJSON(GingerExecConfig gingerExecConfig)
        {
            foreach (AlmDetails almDetails in gingerExecConfig.AlmsDetails)
            {
                ALMIntegrationEnums.eALMType almTypeToConfigure;
                if (Enum.TryParse(almDetails.ALMType.ToString(), out almTypeToConfigure))
                {
                    try
                    {
                        ALMConfig solutionAlmConfig = WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.AlmType == almTypeToConfigure);
                        if (solutionAlmConfig == null)
                        {
                            //ADD
                            solutionAlmConfig = new ALMConfig() { AlmType = almTypeToConfigure };
                            WorkSpace.Instance.Solution.ALMConfigs.Add(solutionAlmConfig);
                        }
                        ALMUserConfig userProfileAlmConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault(x => x.AlmType == almTypeToConfigure);
                        if (userProfileAlmConfig == null)
                        {
                            //ADD
                            userProfileAlmConfig = new ALMUserConfig() { AlmType = almTypeToConfigure };                           
                            WorkSpace.Instance.UserProfile.ALMUserConfigs.Add(userProfileAlmConfig);
                        }
                        if (almDetails.ALMSubType != null)
                        {
                            solutionAlmConfig.JiraTestingALM = (ALMIntegrationEnums.eTestingALMType)Enum.Parse(typeof(ALMIntegrationEnums.eTestingALMType), almDetails.ALMSubType);
                        }
                        if (almDetails.ServerURL != null)
                        {
                            solutionAlmConfig.ALMServerURL = almDetails.ServerURL;
                            userProfileAlmConfig.ALMServerURL = almDetails.ServerURL;
                        }
                        if (almDetails.User != null)
                        {
                            solutionAlmConfig.ALMUserName = almDetails.User;
                            userProfileAlmConfig.ALMUserName = almDetails.User;
                        }
                        if (almDetails.Password != null)
                        {
                            if (almDetails.PasswordEncrypted)
                            {
                                string pass = EncryptionHandler.DecryptwithKey(almDetails.Password);
                                solutionAlmConfig.ALMPassword = pass;
                                userProfileAlmConfig.ALMPassword = pass;
                            }
                            else
                            {
                                solutionAlmConfig.ALMPassword = almDetails.Password;
                                userProfileAlmConfig.ALMPassword = almDetails.Password;
                            }
                        }
                        if (almDetails.Domain != null)
                        {
                            solutionAlmConfig.ALMDomain = almDetails.Domain;
                        }
                        if (almDetails.Project != null)
                        {
                            solutionAlmConfig.ALMProjectName = almDetails.Project;
                        }
                        if (almDetails.ProjectKey != null)
                        {
                            solutionAlmConfig.ALMProjectKey = almDetails.ProjectKey;
                        }
                        if (almDetails.ConfigPackageFolderPath != null)
                        {
                            solutionAlmConfig.ALMConfigPackageFolderPath = almDetails.ConfigPackageFolderPath;
                            userProfileAlmConfig.ALMConfigPackageFolderPath = almDetails.ConfigPackageFolderPath;
                        }
                        if (almDetails.UseRest != null)
                        {
                            solutionAlmConfig.UseRest = (bool)almDetails.UseRest;
                        }
                        if (almDetails.IsDefault != null)
                        {
                            if (almDetails.IsDefault == true)
                            {
                                //clear previous default
                                ALMConfig currentDefAlm = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm == true).FirstOrDefault();
                                if(currentDefAlm != null)
                                {
                                    currentDefAlm.DefaultAlm = false;
                                }
                                else
                                {
                                    Reporter.ToLog(eLogLevel.WARN, (string.Format("Failed to load the ALM type: '{0}' details", almDetails.ALMType)));
                                }
                                
                            }
                            solutionAlmConfig.DefaultAlm = (bool)almDetails.IsDefault;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(string.Format("Failed to load the ALM type: '{0}' details, due to error: '{1}'", almDetails.ALMType, ex.Message), ex);
                    }
                }
                else
                {
                    throw new Exception(string.Format("Failed to find the ALM type: '{0}'", almDetails.ALMType));
                }
            }
        }
    }
}
