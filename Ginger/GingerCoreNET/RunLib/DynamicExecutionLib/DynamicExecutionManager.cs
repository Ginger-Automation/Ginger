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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.ExecuterService.Contracts;
using Ginger.ExecuterService.Contracts.V1.ExecuterHandler.Requests;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration.RunsetOperations;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using RunsetOperations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Ginger.ExecuterService.Contracts;
using static Ginger.Configurations.SealightsConfiguration;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib
{
    public class DynamicExecutionManager
    {

        #region XML
        public static string CreateDynamicRunSetXML(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            runsetExecutor.RunSetConfig.UpdateRunnersBusinessFlowRunsList();

            //Create execution object
            DynamicGingerExecution dynamicExecution = new DynamicGingerExecution();
            dynamicExecution.SolutionDetails = new SolutionDetails();
            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {
                dynamicExecution.SolutionDetails.SourceControlDetails = new SourceControlDetails();
                dynamicExecution.SolutionDetails.SourceControlDetails.Type = solution.SourceControl.GetSourceControlType.ToString();
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.SVN)//added for supporting Jenkins way of config creation- need to improve it
                {
                    string modifiedURI = solution.SourceControl.SourceControlURL.TrimEnd(new char[] { '/' });
                    int lastSlash = modifiedURI.LastIndexOf('/');
                    modifiedURI = (lastSlash > -1) ? modifiedURI.Substring(0, lastSlash) : modifiedURI;
                    dynamicExecution.SolutionDetails.SourceControlDetails.Url = modifiedURI;
                }
                else
                {
                    dynamicExecution.SolutionDetails.SourceControlDetails.Url = solution.SourceControl.SourceControlURL.ToString();
                }
                if (solution.SourceControl.SourceControlUser != null && solution.SourceControl.SourceControlPass != null)
                {
                    dynamicExecution.SolutionDetails.SourceControlDetails.User = solution.SourceControl.SourceControlUser;
                    dynamicExecution.SolutionDetails.SourceControlDetails.Password = EncryptionHandler.EncryptwithKey(solution.SourceControl.SourceControlPass);
                    dynamicExecution.SolutionDetails.SourceControlDetails.PasswordEncrypted = "Y";
                }
                else
                {
                    dynamicExecution.SolutionDetails.SourceControlDetails.User = "N/A";
                    dynamicExecution.SolutionDetails.SourceControlDetails.Password = "N/A";
                }
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress != null && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer = solution.SourceControl.SourceControlProxyAddress.ToString();
                    dynamicExecution.SolutionDetails.SourceControlDetails.ProxyPort = solution.SourceControl.SourceControlProxyPort.ToString();
                }
            }
            dynamicExecution.SolutionDetails.Path = solution.Folder;
            dynamicExecution.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;

            AddRunset addRunset = new AddRunset();
            addRunset.Name = "Dynamic_" + runsetExecutor.RunSetConfig.Name;
            addRunset.Environment = runsetExecutor.RunsetExecutionEnvironment.Name;
            addRunset.RunAnalyzer = cliHelper.RunAnalyzer;
            addRunset.RunInParallel = runsetExecutor.RunSetConfig.RunModeParallel;

            foreach (GingerRunner gingerRunner in runsetExecutor.RunSetConfig.GingerRunners)
            {
                AddRunner addRunner = new AddRunner();
                addRunner.Name = gingerRunner.Name;
                if (gingerRunner.UseSpecificEnvironment == true && string.IsNullOrEmpty(gingerRunner.SpecificEnvironmentName) == false)
                {
                    addRunner.Environment = gingerRunner.SpecificEnvironmentName;
                }
                if (gingerRunner.RunOption != GingerRunner.eRunOptions.ContinueToRunall)
                {
                    addRunner.RunMode = gingerRunner.RunOption.ToString();
                }

                foreach (GingerCore.Platforms.ApplicationAgent applicationAgent in gingerRunner.ApplicationAgents)
                {
                    addRunner.SetAgents.Add(new SetAgent() { AgentName = applicationAgent.AgentName, ApplicationName = applicationAgent.AppName });
                }

                foreach (BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    AddBusinessFlow addBusinessFlow = new AddBusinessFlow();
                    addBusinessFlow.Name = businessFlowRun.BusinessFlowName;
                    if (businessFlowRun.BusinessFlowCustomizedRunVariables.Count > 0)
                    {
                        addBusinessFlow.InputVariables = new System.Collections.Generic.List<InputVariable>();
                        foreach (VariableBase variableBase in businessFlowRun.BusinessFlowCustomizedRunVariables)
                        {
                            InputVariable inputVar = new InputVariable();
                            if (variableBase.ParentType != "Business Flow")
                            {
                                inputVar.VariableParentType = variableBase.ParentType;
                                inputVar.VariableParentName = variableBase.ParentName;
                            }
                            inputVar.VariableName = variableBase.Name;
                            inputVar.VariableValue = variableBase.Value;

                            addBusinessFlow.InputVariables.Add(inputVar);
                        }
                    }
                    addRunner.AddBusinessFlows.Add(addBusinessFlow);
                }
                addRunset.AddRunners.Add(addRunner);
            }

            foreach (RunSetActionBase runSetOperation in runsetExecutor.RunSetConfig.RunSetActions)
            {
                if (runSetOperation is RunSetActionHTMLReportSendEmail)
                {
                    RunSetActionHTMLReportSendEmail runsetMailReport = (RunSetActionHTMLReportSendEmail)runSetOperation;
                    MailReport dynamicMailReport = new MailReport();
                    dynamicMailReport.Condition = runsetMailReport.Condition.ToString();
                    dynamicMailReport.RunAt = runsetMailReport.RunAt.ToString();

                    dynamicMailReport.MailFrom = runsetMailReport.MailFrom;
                    dynamicMailReport.MailTo = runsetMailReport.MailTo;
                    dynamicMailReport.MailCC = runsetMailReport.MailCC;

                    dynamicMailReport.Subject = runsetMailReport.Subject;
                    dynamicMailReport.Comments = runsetMailReport.Comments;

                    if (runsetMailReport.Email.EmailMethod == GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK)
                    {
                        dynamicMailReport.SendViaOutlook = true;
                    }
                    else
                    {
                        dynamicMailReport.SmtpDetails = new SmtpDetails();
                        dynamicMailReport.SmtpDetails.Server = runsetMailReport.Email.SMTPMailHost;
                        dynamicMailReport.SmtpDetails.Port = runsetMailReport.Email.SMTPPort.ToString();
                        dynamicMailReport.SmtpDetails.EnableSSL = runsetMailReport.Email.EnableSSL.ToString();
                        if (runsetMailReport.Email.ConfigureCredential)
                        {
                            dynamicMailReport.SmtpDetails.User = runsetMailReport.Email.SMTPUser;
                            dynamicMailReport.SmtpDetails.Password = runsetMailReport.Email.SMTPPass;
                        }
                    }

                    if (runsetMailReport.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault() != null)
                    {
                        dynamicMailReport.IncludeAttachmentReport = true;
                    }
                    else
                    {
                        dynamicMailReport.IncludeAttachmentReport = false;
                    }

                    addRunset.AddRunsetOperations.Add(dynamicMailReport);
                }
                else if (runSetOperation is RunSetActionJSONSummary)
                {
                    JsonReport dynamicJsonReport = new JsonReport();
                    addRunset.AddRunsetOperations.Add(dynamicJsonReport);
                }
                else if (runSetOperation is RunSetActionHTMLReport)
                {
                    RunSetActionHTMLReport runsetActionProduceHTMLReport = (RunSetActionHTMLReport)runSetOperation;
                    ProduceHTML dynamicReport = new ProduceHTML();
                    dynamicReport.Condition = runsetActionProduceHTMLReport.Condition.ToString();
                    dynamicReport.RunAt = runsetActionProduceHTMLReport.RunAt.ToString();
                    dynamicReport.selectedHTMLReportTemplateID = runsetActionProduceHTMLReport.selectedHTMLReportTemplateID;
                    dynamicReport.isHTMLReportFolderNameUsed = runsetActionProduceHTMLReport.isHTMLReportFolderNameUsed;
                    if (runsetActionProduceHTMLReport.isHTMLReportFolderNameUsed)
                    {
                        dynamicReport.HTMLReportFolderName = runsetActionProduceHTMLReport.HTMLReportFolderName;
                    }
                    dynamicReport.isHTMLReportPermanentFolderNameUsed = runsetActionProduceHTMLReport.isHTMLReportPermanentFolderNameUsed;

                    addRunset.AddRunsetOperations.Add(dynamicReport);
                }
            }
            dynamicExecution.AddRunsets.Add(addRunset);

            //Serilize to XML String
            XmlSerializer writer = new XmlSerializer(typeof(DynamicGingerExecution));
            StringWriter stringWriter = new StringWriter();
            writer.Serialize(stringWriter, dynamicExecution);
            stringWriter.Close();
            return stringWriter.GetStringBuilder().ToString();
        }

        public static DynamicGingerExecution LoadDynamicExecutionFromXML(string content)
        {
            XmlSerializer reader = new XmlSerializer(typeof(DynamicGingerExecution));
            StringReader stringReader = new System.IO.StringReader(content);
            DynamicGingerExecution dynamicRunSet = (DynamicGingerExecution)reader.Deserialize(stringReader);
            stringReader.Close();
            return dynamicRunSet;
        }

        public static void CreateRunSetFromXML(RunsetExecutor runsetExecutor, AddRunset dynamicRunsetConfigs)
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = dynamicRunsetConfigs.Name;
            runSetConfig.RunWithAnalyzer = dynamicRunsetConfigs.RunAnalyzer;
            runSetConfig.RunModeParallel = dynamicRunsetConfigs.RunInParallel;

            // Add runners
            foreach (AddRunner addRunner in dynamicRunsetConfigs.AddRunners)
            {
                GingerRunner gingerRunner = new GingerRunner();
                gingerRunner.Name = addRunner.Name;

                if (!string.IsNullOrEmpty(addRunner.RunMode))
                {
                    gingerRunner.RunOption = (GingerRunner.eRunOptions)Enum.Parse(typeof(GingerRunner.eRunOptions), addRunner.RunMode, true);
                }

                if (!string.IsNullOrEmpty(addRunner.Environment))
                {
                    gingerRunner.UseSpecificEnvironment = true;
                    gingerRunner.SpecificEnvironmentName = addRunner.Environment;
                }

                //add Agents
                foreach (SetAgent setAgent in addRunner.SetAgents)
                {
                    GingerCore.Platforms.ApplicationAgent appAgent = new GingerCore.Platforms.ApplicationAgent();
                    appAgent.AppName = setAgent.ApplicationName;
                    appAgent.AgentName = setAgent.AgentName;
                    gingerRunner.ApplicationAgents.Add(appAgent);
                }

                // Add BFs
                foreach (AddBusinessFlow addBusinessFlow in addRunner.AddBusinessFlows)
                {
                    BusinessFlowRun businessFlowRun = new BusinessFlowRun();
                    businessFlowRun.BusinessFlowName = addBusinessFlow.Name;
                    businessFlowRun.BusinessFlowIsActive = true;

                    // set BF Variables
                    if (addBusinessFlow.InputVariables != null)
                    {
                        foreach (InputVariable inputVariabel in addBusinessFlow.InputVariables)
                        {
                            businessFlowRun.BusinessFlowCustomizedRunVariables.Add(new VariableString() { DiffrentFromOrigin = true, VarValChanged = true, ParentType = inputVariabel.VariableParentType, ParentName = inputVariabel.VariableParentName, Name = inputVariabel.VariableName, InitialStringValue = inputVariabel.VariableValue, Value = inputVariabel.VariableValue });
                        }
                    }
                    gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);
                }
                runSetConfig.GingerRunners.Add(gingerRunner);
            }

            //Add mail Report handling
            foreach (AddRunsetOperation addOperation in dynamicRunsetConfigs.AddRunsetOperations)
            {
                if (addOperation is MailReport)
                {
                    MailReport dynamicMailOperation = (MailReport)addOperation;
                    RunSetActionHTMLReportSendEmail mailOperation = new RunSetActionHTMLReportSendEmail();

                    mailOperation.Name = "Dynamic Mail Report";
                    mailOperation.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), dynamicMailOperation.Condition, true);
                    mailOperation.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), dynamicMailOperation.RunAt, true);
                    mailOperation.Active = true;

                    mailOperation.MailFrom = dynamicMailOperation.MailFrom;
                    mailOperation.MailTo = dynamicMailOperation.MailTo;
                    mailOperation.MailCC = dynamicMailOperation.MailCC;

                    mailOperation.Subject = dynamicMailOperation.Subject;
                    mailOperation.Comments = dynamicMailOperation.Comments;
                    //mailOperation.Comments = string.Format("Dynamic {0} Execution Report" + GingerDicser.GetTermResValue(eTermResKey.RunSet));

                    mailOperation.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport;
                    mailOperation.selectedHTMLReportTemplateID = 100;//ID to mark defualt template

                    mailOperation.Email.IsBodyHTML = true;
                    mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.SMTP;
                    mailOperation.Email.MailFrom = dynamicMailOperation.MailFrom;
                    mailOperation.Email.MailTo = dynamicMailOperation.MailTo;
                    mailOperation.Email.Subject = dynamicMailOperation.Subject;
                    if (dynamicMailOperation.SendViaOutlook)
                    {
                        mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK;
                    }
                    else
                    {
                        if (dynamicMailOperation.SmtpDetails != null)
                        {
                            mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.SMTP;
                            mailOperation.Email.SMTPMailHost = dynamicMailOperation.SmtpDetails.Server;
                            mailOperation.Email.SMTPPort = int.Parse(dynamicMailOperation.SmtpDetails.Port);
                            mailOperation.Email.EnableSSL = bool.Parse(dynamicMailOperation.SmtpDetails.EnableSSL);
                            if (string.IsNullOrEmpty(dynamicMailOperation.SmtpDetails.User) == false)
                            {
                                mailOperation.Email.ConfigureCredential = true;
                                mailOperation.Email.SMTPUser = dynamicMailOperation.SmtpDetails.User;
                                mailOperation.Email.SMTPPass = dynamicMailOperation.SmtpDetails.Password;
                            }
                        }
                    }

                    if (dynamicMailOperation.IncludeAttachmentReport)
                    {
                        EmailHtmlReportAttachment reportAttachment = new EmailHtmlReportAttachment();
                        reportAttachment.AttachmentType = EmailAttachment.eAttachmentType.Report;
                        reportAttachment.ZipIt = true;
                        mailOperation.EmailAttachments.Add(reportAttachment);
                    }

                    runSetConfig.RunSetActions.Add(mailOperation);
                }
                else if (addOperation is JsonReport)
                {
                    JsonReport dynamicJsonReport = (JsonReport)addOperation;
                    RunSetActionJSONSummary jsonReportOperation = new RunSetActionJSONSummary();

                    jsonReportOperation.Name = "Dynamic Json Report";
                    jsonReportOperation.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), dynamicJsonReport.Condition, true);
                    jsonReportOperation.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), dynamicJsonReport.RunAt, true);
                    jsonReportOperation.Active = true;

                    runSetConfig.RunSetActions.Add(jsonReportOperation);
                }
                else if (addOperation is ProduceHTML)
                {
                    ProduceHTML dynamicReport = (ProduceHTML)addOperation;
                    RunSetActionHTMLReport reportOpertaion = new RunSetActionHTMLReport();

                    reportOpertaion.Name = "Dynamic HTML Report";
                    reportOpertaion.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), dynamicReport.Condition, true);
                    reportOpertaion.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), dynamicReport.RunAt, true);
                    reportOpertaion.Active = true;
                    reportOpertaion.selectedHTMLReportTemplateID = dynamicReport.selectedHTMLReportTemplateID;
                    reportOpertaion.isHTMLReportFolderNameUsed = dynamicReport.isHTMLReportFolderNameUsed;
                    if (dynamicReport.isHTMLReportFolderNameUsed)
                    {
                        reportOpertaion.HTMLReportFolderName = dynamicReport.HTMLReportFolderName;
                    }
                    reportOpertaion.isHTMLReportPermanentFolderNameUsed = dynamicReport.isHTMLReportPermanentFolderNameUsed;


                    runSetConfig.RunSetActions.Add(reportOpertaion);
                }
            }

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }
        #endregion XML

        #region JSON

        public static string CreateRemoteExecutionRequestJSON(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            //create execution object for RemoteExecution
            AddExecutionRequest remoteExecutionRequest = new AddExecutionRequest();
            remoteExecutionRequest.Priority = eExecutionPriority.Normal;
            remoteExecutionRequest.Title = runsetExecutor.RunSetConfig.Name;
            remoteExecutionRequest.Description = "Execution Requested from Ginger Desktop";
            remoteExecutionRequest.RequestingUser = Environment.UserName;
            remoteExecutionRequest.RequestingApplication = "Ginger Desktop";
            remoteExecutionRequest.PreferredAgentId = null;
            remoteExecutionRequest.TagName = string.Empty;
            remoteExecutionRequest.ExecutionConfigurations = GetGingerExecConfigurationObject(solution, runsetExecutor, cliHelper);

            return SerializeDynamicExecutionToJSON(remoteExecutionRequest);

        }
        public static string CreateDynamicRunSetJSON(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            runsetExecutor.RunSetConfig.UpdateRunnersBusinessFlowRunsList();

            //Create execution object
            GingerExecConfig executionConfig = GetGingerExecConfigurationObject(solution, runsetExecutor, cliHelper);

            //serilize object to JSON String
            return SerializeDynamicExecutionToJSON(executionConfig);
        }

        private static GingerExecConfig GetGingerExecConfigurationObject(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            GingerExecConfig executionConfig = new GingerExecConfig();

            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {
                executionConfig.SolutionScmDetails = new ScmDetails();
                executionConfig.SolutionScmDetails.SCMType = (ScmDetails.eSCMType)Enum.Parse(typeof(ScmDetails.eSCMType), solution.SourceControl.GetSourceControlType.ToString(), true);
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.SVN)//added for supporting Jenkins way of config creation- need to improve it
                {
                    string modifiedURI = solution.SourceControl.SourceControlURL.TrimEnd(new char[] { '/' });
                    int lastSlash = modifiedURI.LastIndexOf('/');
                    modifiedURI = (lastSlash > -1) ? modifiedURI.Substring(0, lastSlash) : modifiedURI;
                    executionConfig.SolutionScmDetails.SolutionRepositoryUrl = modifiedURI;
                }
                else
                {
                    executionConfig.SolutionScmDetails.SolutionRepositoryUrl = solution.SourceControl.SourceControlURL.ToString();
                }
                if (solution.SourceControl.SourceControlUser != null && solution.SourceControl.SourceControlPass != null)
                {
                    executionConfig.SolutionScmDetails.User = solution.SourceControl.SourceControlUser;
                    executionConfig.SolutionScmDetails.Password = EncryptionHandler.EncryptwithKey(solution.SourceControl.SourceControlPass);
                    executionConfig.SolutionScmDetails.PasswordEncrypted = true;
                }
                else
                {
                    executionConfig.SolutionScmDetails.User = "N/A";
                    executionConfig.SolutionScmDetails.Password = "N/A";
                }
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress != null && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    executionConfig.SolutionScmDetails.ProxyServer = solution.SourceControl.SourceControlProxyAddress.ToString();
                    executionConfig.SolutionScmDetails.ProxyPort = solution.SourceControl.SourceControlProxyPort.ToString();
                }
                executionConfig.SolutionScmDetails.UndoSolutionLocalChanges = false;
                executionConfig.SolutionScmDetails.Branch = solution.SourceControl.SourceControlBranch;
            }
            if (cliHelper.SetAlmConnectionDetails == true)
            {
                if (WorkSpace.Instance.Solution.ALMConfigs != null && WorkSpace.Instance.Solution.ALMConfigs.Count > 0)
                {
                    executionConfig.AlmsDetails = new List<AlmDetails>();
                    foreach (ALMConfig solutionAlmConfig in WorkSpace.Instance.Solution.ALMConfigs)
                    {
                        ALMUserConfig userProfileAlmConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault(x => x.AlmType == solutionAlmConfig.AlmType);
                        AlmDetails almDetails = new AlmDetails();
                        almDetails.ALMType = solutionAlmConfig.AlmType.ToString();
                        if (solutionAlmConfig.JiraTestingALM != ALMIntegrationEnums.eTestingALMType.None)
                        {
                            almDetails.ALMSubType = solutionAlmConfig.JiraTestingALM.ToString();
                        }
                        if (!string.IsNullOrEmpty(solutionAlmConfig.ALMServerURL))
                        {
                            almDetails.ServerURL = solutionAlmConfig.ALMServerURL;
                        }
                        if (userProfileAlmConfig != null)
                        {
                            almDetails.User = userProfileAlmConfig.ALMUserName;
                            almDetails.Password = EncryptionHandler.EncryptwithKey(userProfileAlmConfig.ALMPassword);
                            almDetails.PasswordEncrypted = true;
                        }
                        else
                        {
                            almDetails.User = solutionAlmConfig.ALMUserName;
                            almDetails.Password = EncryptionHandler.EncryptwithKey(solutionAlmConfig.ALMPassword);
                            almDetails.PasswordEncrypted = true;
                        }
                        if (!string.IsNullOrEmpty(solutionAlmConfig.ALMDomain))
                        {
                            almDetails.Domain = solutionAlmConfig.ALMDomain;
                        }
                        if (!string.IsNullOrEmpty(solutionAlmConfig.ALMProjectName))
                        {
                            almDetails.Project = solutionAlmConfig.ALMProjectName;
                        }
                        if (!string.IsNullOrEmpty(solutionAlmConfig.ALMProjectKey))
                        {
                            almDetails.ProjectKey = solutionAlmConfig.ALMProjectKey;
                        }
                        if (!string.IsNullOrEmpty(solutionAlmConfig.ALMConfigPackageFolderPath))
                        {
                            almDetails.ConfigPackageFolderPath = solutionAlmConfig.ALMConfigPackageFolderPath;
                        }
                        if (solutionAlmConfig.UseRest)
                        {
                            almDetails.UseRest = true;
                        }
                        if (solutionAlmConfig.DefaultAlm)
                        {
                            almDetails.IsDefault = true;
                        }
                        executionConfig.AlmsDetails.Add(almDetails);
                    }
                }
            }

            if (cliHelper.SetSealightsSettings)
            {
                SealightsDetails sealightsDetails = new SealightsDetails();

                sealightsDetails.SealightsEnable = solution.SealightsConfiguration.SealightsLog == eSealightsLog.Yes;
                sealightsDetails.SealightsUrl = solution.SealightsConfiguration.SealightsURL;
                sealightsDetails.SealightsLabId = solution.SealightsConfiguration.SealightsLabId;
                sealightsDetails.SealightsBSId = solution.SealightsConfiguration.SealightsBuildSessionID;
                sealightsDetails.SealightsTestStage = solution.SealightsConfiguration.SealightsTestStage;
                sealightsDetails.SealightsSessionTimeout = Convert.ToInt32(solution.SealightsConfiguration.SealightsSessionTimeout);
                sealightsDetails.SealightsEntityLevel = (SealightsDetails.eSealightsEntityLevel)solution.SealightsConfiguration.SealightsReportedEntityLevel;
                sealightsDetails.SealightsAgentToken = solution.SealightsConfiguration.SealightsAgentToken;
                sealightsDetails.SealightsTestRecommendations = solution.SealightsConfiguration.SealightsTestRecommendations == eSealightsTestRecommendations.Yes;


                //  Check Sealights's values on run-set levels
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsLabId != null)
                {
                    sealightsDetails.SealightsLabId = runsetExecutor.RunSetConfig.SealightsLabId;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsBuildSessionID != null)
                {
                    sealightsDetails.SealightsBSId = runsetExecutor.RunSetConfig.SealightsBuildSessionID;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestStage != null)
                {
                    sealightsDetails.SealightsTestStage = runsetExecutor.RunSetConfig.SealightsTestStage;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SealightsTestRecommendationsRunsetOverrideFlag)
                {
                    sealightsDetails.SealightsTestRecommendations = runsetExecutor.RunSetConfig.SealightsTestRecommendations == eSealightsTestRecommendations.Yes;
                }

                executionConfig.SealightsDetails = sealightsDetails;
            }

            executionConfig.SolutionLocalPath = solution.Folder;

            executionConfig.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;
            executionConfig.VerboseLevel = GingerExecConfig.eVerboseLevel.normal;
            executionConfig.EncryptionKey = solution.EncryptionKey;



            RunsetExecConfig runset = new RunsetExecConfig();
            runset.Exist = true;
            runset.Name = runsetExecutor.RunSetConfig.Name;
            runset.ID = runsetExecutor.RunSetConfig.Guid;
            runset.Description = runsetExecutor.RunSetConfig.Description;

            runset.EnvironmentName = runsetExecutor.RunsetExecutionEnvironment.Name;
            runset.EnvironmentID = runsetExecutor.RunsetExecutionEnvironment.Guid;

            runset.RunAnalyzer = cliHelper.RunAnalyzer;
            runset.RunInParallel = runsetExecutor.RunSetConfig.RunModeParallel;
            runset.StopRunnersOnFailure = runsetExecutor.RunSetConfig.StopRunnersOnFailure;

            SelfHealingConfig selfHealingConfiguration = new SelfHealingConfig()
            {
                AutoFixAnalyzerIssue = runsetExecutor.RunSetConfig.SelfHealingConfiguration.AutoFixAnalyzerIssue,
                ReprioritizePOMLocators = runsetExecutor.RunSetConfig.SelfHealingConfiguration.ReprioritizePOMLocators,
                AutoUpdateApplicationModel = runsetExecutor.RunSetConfig.SelfHealingConfiguration.AutoUpdateApplicationModel,
                SaveChangesInSourceControl = runsetExecutor.RunSetConfig.SelfHealingConfiguration.SaveChangesInSourceControl,
                AutoExecuteInSimulationMode = runsetExecutor.RunSetConfig.SelfHealingConfiguration.AutoExecuteInSimulationMode
            };

            runset.SelfHealingConfiguration = selfHealingConfiguration;


            if (runsetExecutor.RunSetConfig.GingerRunners.Count > 0)
            {
                runset.Runners = new List<RunnerExecConfig>();
            }
            foreach (GingerRunner gingerRunner in runsetExecutor.RunSetConfig.GingerRunners)
            {
                RunnerExecConfig runner = new RunnerExecConfig();
                runner.Name = gingerRunner.Name;
                runner.ID = gingerRunner.Guid;
                runner.Active = gingerRunner.Active;
                runner.Exist = true;
                if (gingerRunner.UseSpecificEnvironment == true && string.IsNullOrEmpty(gingerRunner.SpecificEnvironmentName) == false)
                {
                    ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name == gingerRunner.SpecificEnvironmentName).FirstOrDefault();
                    if (env != null)
                    {
                        runner.EnvironmentName = env.Name;
                        runner.EnvironmentID = env.Guid;
                    }
                }
                //if (gingerRunner.RunOption != GingerRunner.eRunOptions.ContinueToRunall)
                //{
                runner.OnFailureRunOption = (RunnerExecConfig.eOnFailureRunOption)Enum.Parse(typeof(RunnerExecConfig.eOnFailureRunOption), gingerRunner.RunOption.ToString(), true);
                //}

                if (gingerRunner.ApplicationAgents.Count > 0)
                {
                    runner.AppAgentMappings = new List<AppAgentMapping>();
                }
                foreach (ApplicationAgent applicationAgent in gingerRunner.ApplicationAgents)
                {
                    ApplicationAgentOperations applicationAgentOperations = new ApplicationAgentOperations(applicationAgent);
                    applicationAgent.ApplicationAgentOperations = applicationAgentOperations;
                    if (applicationAgent.Agent == null)
                    {
                        continue;//probably target app without platform or no such Agent
                    }
                    runner.AppAgentMappings.Add(new AppAgentMapping() { AgentName = applicationAgent.AgentName, AgentID = applicationAgent.AgentID, ApplicationName = applicationAgent.AppName, ApplicationID = applicationAgent.AppID });
                }

                //
                runner.RunInSimulationMode = gingerRunner.RunInSimulationMode;
                runner.RunInVisualTestingMode = gingerRunner.RunInVisualTestingMode;
                runner.KeepAgentsOpen = gingerRunner.KeepAgentsOn;

                if (gingerRunner.BusinessFlowsRunList.Count > 0)
                {
                    runner.BusinessFlows = new List<BusinessFlowExecConfig>();
                }
                foreach (BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    BusinessFlowExecConfig businessFlow = new BusinessFlowExecConfig();
                    businessFlow.Name = businessFlowRun.BusinessFlowName;
                    businessFlow.ID = businessFlowRun.BusinessFlowGuid;
                    businessFlow.InstanceID = businessFlowRun.BusinessFlowInstanceGuid;
                    businessFlow.Exist = true;

                    if (gingerRunner.BusinessFlowsRunList.Where(x => x.BusinessFlowGuid == businessFlowRun.BusinessFlowGuid).ToList().Count > 1)
                    {
                        businessFlow.Instance = gingerRunner.BusinessFlowsRunList.Where(x => x.BusinessFlowGuid == businessFlowRun.BusinessFlowGuid).ToList().IndexOf(businessFlowRun) + 1;
                    }
                    else
                    {
                        businessFlow.Instance = null;
                    }
                    businessFlow.Active = businessFlowRun.BusinessFlowIsActive;
                    if (businessFlowRun.BusinessFlowCustomizedRunVariables.Count > 0)
                    {
                        ObservableList<VariableBase> allInputVars = null;
                        BusinessFlow parentBF = FindItemByIDAndName<BusinessFlow>(
                                    new Tuple<string, Guid?>(nameof(BusinessFlow.Guid), businessFlowRun.BusinessFlowGuid),
                                    new Tuple<string, string>(nameof(BusinessFlow.Name), businessFlowRun.BusinessFlowName),
                                    WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>());
                        if (parentBF != null)
                        {
                            allInputVars = parentBF.GetBFandActivitiesVariabeles(includeParentDetails: true, includeOnlySetAsInputValue: true);
                        }
                        businessFlow.InputValues = new List<InputValue>();
                        foreach (VariableBase customizedVar in businessFlowRun.BusinessFlowCustomizedRunVariables)
                        {
                            InputValue jsonInputVar = new InputValue();
                            VariableBase originalVar = FindItemByIDAndName<VariableBase>(
                                           new Tuple<string, Guid?>(nameof(VariableBase.Guid), customizedVar.Guid),
                                           new Tuple<string, string>(nameof(VariableBase.Name), customizedVar.Name),
                                           allInputVars);
                            if (originalVar != null)
                            {
                                jsonInputVar.VariableParentName = originalVar.ParentName;
                                jsonInputVar.VariableParentID = originalVar.ParentGuid;
                                jsonInputVar.VariableName = originalVar.Name;
                                jsonInputVar.VariableID = originalVar.Guid;

                                switch (customizedVar.MappedOutputType)
                                {
                                    case VariableBase.eOutputType.OutputVariable:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.OutputVariable;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.MappedOutputValue;
                                        break;
                                    case VariableBase.eOutputType.GlobalVariable:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.GlobalVariable;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.MappedOutputValue;
                                        break;
                                    case VariableBase.eOutputType.ApplicationModelParameter:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.ApplicationModelParameter;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.MappedOutputValue;
                                        break;
                                    case VariableBase.eOutputType.DataSource:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.DataSource;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.MappedOutputValue;
                                        break;
                                    case VariableBase.eOutputType.Variable:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.Variable;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.MappedOutputValue;
                                        break;
                                    default:
                                        jsonInputVar.VariableCustomizationType = InputValue.eVariableCustomizationType.Value;
                                        jsonInputVar.VariableCustomizedValue = customizedVar.Value;
                                        break;
                                }

                                businessFlow.InputValues.Add(jsonInputVar);
                            }
                        }
                    }
                    runner.BusinessFlows.Add(businessFlow);
                }
                runset.Runners.Add(runner);
            }

            if (runsetExecutor.RunSetConfig.RunSetActions.Count > 0)
            {
                runset.Operations = new List<OperationExecConfigBase>();
            }
            foreach (RunSetActionBase runSetOperation in runsetExecutor.RunSetConfig.RunSetActions)
            {
                if (runSetOperation is RunSetActionHTMLReportSendEmail)
                {
                    RunSetActionHTMLReportSendEmail runsetMailReport = (RunSetActionHTMLReportSendEmail)runSetOperation;
                    MailReportOperationExecConfig mailReportConfig = new MailReportOperationExecConfig();
                    mailReportConfig.Name = runsetMailReport.Name;
                    mailReportConfig.ID = runsetMailReport.Guid;
                    mailReportConfig.Condition = (OperationExecConfigBase.eOperationRunCondition)Enum.Parse(typeof(OperationExecConfigBase.eOperationRunCondition), runsetMailReport.Condition.ToString(), true);
                    mailReportConfig.RunAt = (OperationExecConfigBase.eOperationRunAt)Enum.Parse(typeof(OperationExecConfigBase.eOperationRunAt), runsetMailReport.RunAt.ToString(), true);
                    mailReportConfig.Active = runsetMailReport.Active;

                    //standard mail
                    mailReportConfig.MailSettings = new SendMailSettings();
                    if (runsetMailReport.Email.EmailMethod == GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK)
                    {
                        mailReportConfig.MailSettings.EmailMethod = SendMailSettings.eEmailMethod.OUTLOOK;
                    }
                    else
                    {
                        mailReportConfig.MailSettings.EmailMethod = SendMailSettings.eEmailMethod.SMTP;
                        mailReportConfig.MailSettings.SmtpDetails = new MailSmtpDetails();
                        mailReportConfig.MailSettings.SmtpDetails.Server = runsetMailReport.Email.SMTPMailHost;
                        mailReportConfig.MailSettings.SmtpDetails.Port = runsetMailReport.Email.SMTPPort.ToString();
                        mailReportConfig.MailSettings.SmtpDetails.EnableSSL = runsetMailReport.Email.EnableSSL;
                        if (runsetMailReport.Email.ConfigureCredential)
                        {
                            mailReportConfig.MailSettings.SmtpDetails.User = runsetMailReport.Email.SMTPUser;
                            mailReportConfig.MailSettings.SmtpDetails.Password = runsetMailReport.Email.SMTPPass;
                        }
                    }
                    mailReportConfig.MailSettings.MailFrom = runsetMailReport.MailFrom;
                    mailReportConfig.MailSettings.MailFromDisplayName = runsetMailReport.MailFromDisplayName;
                    mailReportConfig.MailSettings.MailTo = runsetMailReport.MailTo;
                    mailReportConfig.MailSettings.MailCC = runsetMailReport.MailCC;
                    mailReportConfig.MailSettings.Subject = runsetMailReport.Subject;
                    //Report mail content
                    mailReportConfig.Comments = runsetMailReport.Comments;
                    switch (runsetMailReport.HTMLReportTemplate)
                    {
                        case RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport:
                            mailReportConfig.BodyContentType = MailReportOperationExecConfig.eBodyContentType.HTMLReport;
                            mailReportConfig.BodyReportTemplateID = runsetMailReport.selectedHTMLReportTemplateID;
                            break;
                        case RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.FreeText:
                            mailReportConfig.BodyContentType = MailReportOperationExecConfig.eBodyContentType.FreeText;
                            mailReportConfig.BodyTextContent = runsetMailReport.Bodytext;
                            break;
                    }
                    //Report mail attachments     
                    mailReportConfig.IncludeAttachmentReport = false;
                    foreach (EmailAttachment mailAttachment in runsetMailReport.EmailAttachments)
                    {
                        if (mailAttachment.AttachmentType == EmailAttachment.eAttachmentType.Report)
                        {
                            EmailHtmlReportAttachment reportAttachment = ((EmailHtmlReportAttachment)mailAttachment);
                            mailReportConfig.IncludeAttachmentReport = true;
                            mailReportConfig.AttachmentReportTemplateID = reportAttachment.SelectedHTMLReportTemplateID;
                            if (reportAttachment.IsLinkEnabled)
                            {
                                mailReportConfig.AttachmentReportAttachType = MailReportOperationExecConfig.ReportAttachType.Link;
                            }
                            else
                            {
                                mailReportConfig.AttachmentReportAttachType = MailReportOperationExecConfig.ReportAttachType.Zip;
                            }
                            if (reportAttachment.IsAlternameFolderUsed && !string.IsNullOrEmpty(reportAttachment.ExtraInformation))
                            {
                                mailReportConfig.CustomizedReportFolder = reportAttachment.ExtraInformation;
                            }
                        }
                        else
                        {
                            if (mailReportConfig.FilesPathToAttach == null)
                            {
                                mailReportConfig.FilesPathToAttach = new List<string>();
                            }
                            mailReportConfig.FilesPathToAttach.Add(mailAttachment.Name);
                        }
                    }

                    runset.Operations.Add(mailReportConfig);
                }
                else if (runSetOperation is RunSetActionJSONSummary)
                {
                    JsonReportOperationExecConfig jsonReportConfig = new JsonReportOperationExecConfig();
                    jsonReportConfig.Name = runSetOperation.Name;
                    jsonReportConfig.ID = runSetOperation.Guid;
                    jsonReportConfig.Condition = (OperationExecConfigBase.eOperationRunCondition)Enum.Parse(typeof(OperationExecConfigBase.eOperationRunCondition), runSetOperation.Condition.ToString(), true);
                    jsonReportConfig.RunAt = (OperationExecConfigBase.eOperationRunAt)Enum.Parse(typeof(OperationExecConfigBase.eOperationRunAt), runSetOperation.RunAt.ToString(), true);
                    jsonReportConfig.Active = runSetOperation.Active;
                    runset.Operations.Add(jsonReportConfig);
                }
                else if (runSetOperation is RunSetActionPublishToQC)
                {
                    RunSetActionPublishToQC publishToQCAction = runSetOperation as RunSetActionPublishToQC;

                    AlmPublishOperationExecConfig operationConfigPublishToALM = new AlmPublishOperationExecConfig();

                    operationConfigPublishToALM.ALMType = publishToQCAction.PublishALMType;
                    operationConfigPublishToALM.Name = publishToQCAction.Name;
                    operationConfigPublishToALM.ID = publishToQCAction.Guid;
                    operationConfigPublishToALM.Condition = (OperationExecConfigBase.eOperationRunCondition?)publishToQCAction.Condition;
                    operationConfigPublishToALM.RunAt = (OperationExecConfigBase.eOperationRunAt?)publishToQCAction.RunAt;
                    operationConfigPublishToALM.AlmTestSetLevel = (AlmPublishOperationExecConfig.eAlmTestSetLevel?)publishToQCAction.ALMTestSetLevel;
                    operationConfigPublishToALM.ExportType = (AlmPublishOperationExecConfig.eExportType?)Enum.Parse(typeof(AlmPublishOperationExecConfig.eExportType), publishToQCAction.ExportType.ToString());
                    operationConfigPublishToALM.TestsetExportDestination= publishToQCAction.TestSetFolderDestination;
                    operationConfigPublishToALM.TestcasesExportDestination = publishToQCAction.TestCaseFolderDestination;
                    operationConfigPublishToALM.TestCasesResultsToExport = (AlmPublishOperationExecConfig.eTestCasesResultsToExport?)Enum.Parse(typeof(AlmPublishOperationExecConfig.eTestCasesResultsToExport), publishToQCAction.FilterStatus.ToString());
                    operationConfigPublishToALM.AttachActivitiesGroupsReport = publishToQCAction.toAttachActivitiesGroupReport;
                    operationConfigPublishToALM.UseUserVariableInRunInstanceName = publishToQCAction.isVariableInTCRunUsed;
                    operationConfigPublishToALM.UserVariableInRunInstance = publishToQCAction.VariableForTCRunName;

                    if (publishToQCAction.AlmFields != null && publishToQCAction.AlmFields.Count > 0)
                    {
                        foreach (var almField in publishToQCAction.AlmFields)
                        {
                            ALMFieldConfig almFieldConfig;

                            if (!string.IsNullOrEmpty(almField.ItemType))
                            {
                                almFieldConfig = new ALMFieldConfig()
                                {
                                    FieldParentType = almField.ItemType,
                                    FieldID = almField.ID,
                                    FieldBackendName = almField.ExternalID,
                                    FieldFrontendName = almField.Name,
                                    IsSystemField = almField.SystemFieled,
                                    FieldType = almField.Type,
                                    FieldValue = almField.SelectedValue
                                };

                                if (operationConfigPublishToALM.AlmFieldsConfig == null)
                                {
                                    operationConfigPublishToALM.AlmFieldsConfig = new List<ALMFieldConfig>();
                                }
                                operationConfigPublishToALM.AlmFieldsConfig.Add(almFieldConfig);
                            }
                        }
                    }

                    runset.Operations.Add(operationConfigPublishToALM);
                }
            }
            executionConfig.Runset = runset;
            return executionConfig;
        }

        public static GingerExecConfig DeserializeDynamicExecutionFromJSON(string content)
        {
            return NewtonsoftJsonUtils.DeserializeObject<GingerExecConfig>(content);
        }

        public static AddExecutionRequest DeserializeRequestExecutionFromJSON(string content)
        {
            return NewtonsoftJsonUtils.DeserializeObject<AddExecutionRequest>(content);
        }

        public static string SerializeDynamicExecutionToJSON(dynamic gingerExecConfig)
        {
            return NewtonsoftJsonUtils.SerializeObject(gingerExecConfig);
        }


        public static void CreateUpdateRunSetFromJSON(RunsetExecutor runsetExecutor, GingerExecConfig gingerExecConfig)
        {
            RunsetExecConfig dynamicRunsetConfigs = gingerExecConfig.Runset;
            RunSetConfig runSetConfig = null;

            if (dynamicRunsetConfigs.Exist)
            {
                //## Updating existing Runset
                runSetConfig = FindItemByIDAndName<RunSetConfig>(
                    new Tuple<string, Guid?>(nameof(RunSetConfig.Guid), dynamicRunsetConfigs.ID),
                    new Tuple<string, string>(nameof(RunSetConfig.Name), dynamicRunsetConfigs.Name),
                    WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>());
            }
            /// Dynamic/Virtual Runset Execution Request
            else
            {
                //## Creating new Runset
                runSetConfig = new RunSetConfig();
                runSetConfig.Name = dynamicRunsetConfigs.Name;
                runSetConfig.AddCategories();
            }

            if (gingerExecConfig.ExecutionID != null)
            {
                runSetConfig.ExecutionID = (Guid)gingerExecConfig.ExecutionID;
                Reporter.ToLog(eLogLevel.INFO, string.Format("Using provided ExecutionID '{0}'.", runSetConfig.ExecutionID.ToString()));
            }

            if (gingerExecConfig.SealightsDetails != null)
            {
                runSetConfig.SealightsBuildSessionID = gingerExecConfig.SealightsDetails.SealightsBSId;
                runSetConfig.SealightsLabId = gingerExecConfig.SealightsDetails.SealightsLabId;
                runSetConfig.SealightsTestStage = gingerExecConfig.SealightsDetails.SealightsTestStage;
                if (gingerExecConfig.SealightsDetails.SealightsTestRecommendations != null)
                {
                    runSetConfig.SealightsTestRecommendations = (bool)gingerExecConfig.SealightsDetails.SealightsTestRecommendations ? eSealightsTestRecommendations.Yes : eSealightsTestRecommendations.No;
                }
                else
                {
                    runSetConfig.SealightsTestRecommendations = eSealightsTestRecommendations.No;
                }
            }

            if (!String.IsNullOrEmpty(gingerExecConfig.Runset.Description))
            {
                runSetConfig.Description = gingerExecConfig.Runset.Description;
            }

            if (dynamicRunsetConfigs.RunAnalyzer != null)
            {
                runSetConfig.RunWithAnalyzer = (bool)dynamicRunsetConfigs.RunAnalyzer;
            }

            if (dynamicRunsetConfigs.RunInParallel != null)
            {
                runSetConfig.RunModeParallel = (bool)dynamicRunsetConfigs.RunInParallel;
            }

            if (dynamicRunsetConfigs.StopRunnersOnFailure != null)
            {
                runSetConfig.StopRunnersOnFailure = (bool)dynamicRunsetConfigs.StopRunnersOnFailure;
            }

            //Add or Update Runners
            if (dynamicRunsetConfigs.Runners != null)
            {
                foreach (RunnerExecConfig runnerConfig in dynamicRunsetConfigs.Runners)
                {
                    GingerRunner gingerRunner = null;

                    /// Not Dynamic/Virtual
                    if (dynamicRunsetConfigs.Exist && (!runnerConfig.Exist.HasValue || runnerConfig.Exist.Value))
                    {
                        gingerRunner = FindItemByIDAndName<GingerRunner>(
                            new Tuple<string, Guid?>(nameof(GingerRunner.Guid), runnerConfig.ID),
                            new Tuple<string, string>(nameof(GingerRunner.Name), runnerConfig.Name),
                            runSetConfig.GingerRunners);
                    }
                    /// Initialize Virtual Runner
                    else
                    {
                        gingerRunner = new GingerRunner();
                        gingerRunner.Name = runnerConfig.Name;
                        (runSetConfig).GingerRunners.Add(gingerRunner);
                    }

                    if (runnerConfig.Active.HasValue)
                    {
                        gingerRunner.Active = runnerConfig.Active.Value;
                    }

                    if (runnerConfig.RunInSimulationMode.HasValue)
                    {
                        gingerRunner.RunInSimulationMode = runnerConfig.RunInSimulationMode.Value;
                    }
                    if (runnerConfig.RunInVisualTestingMode.HasValue)
                    {
                        gingerRunner.RunInVisualTestingMode = runnerConfig.RunInVisualTestingMode.Value;
                    }
                    if (runnerConfig.KeepAgentsOpen.HasValue)
                    {
                        gingerRunner.KeepAgentsOn = runnerConfig.KeepAgentsOpen.Value;
                    }

                    if (runnerConfig.EnvironmentName != null || runnerConfig.EnvironmentID != null)
                    {
                        ProjEnvironment env = DynamicExecutionManager.FindItemByIDAndName<ProjEnvironment>(
                                        new Tuple<string, Guid?>(nameof(ProjEnvironment.Guid), runnerConfig.EnvironmentID),
                                        new Tuple<string, string>(nameof(ProjEnvironment.Name), runnerConfig.EnvironmentName),
                                        WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>());
                        if (env != null)
                        {
                            gingerRunner.UseSpecificEnvironment = true;
                            gingerRunner.SpecificEnvironmentName = env.Name;
                        }
                    }

                    if (runnerConfig.OnFailureRunOption != null)
                    {
                        gingerRunner.RunOption = (GingerRunner.eRunOptions)Enum.Parse(typeof(GingerRunner.eRunOptions), runnerConfig.OnFailureRunOption.ToString(), true);
                    }

                    //Add or Update Agents mapping
                    if (runnerConfig.AppAgentMappings != null)
                    {
                        foreach (AppAgentMapping appAgentConfig in runnerConfig.AppAgentMappings)
                        {
                            ApplicationAgent appAgent = null;
                            /// Use existing AppAgents for only Non-Virtual Runsets & Runners
                            if (dynamicRunsetConfigs.Exist && (!runnerConfig.Exist.HasValue || runnerConfig.Exist.Value))
                            {
                                appAgent = (ApplicationAgent)FindItemByIDAndName<IApplicationAgent>(
                                                        new Tuple<string, Guid?>(nameof(IApplicationAgent.AppID), appAgentConfig.ApplicationID),
                                                        new Tuple<string, string>(nameof(IApplicationAgent.AppName), appAgentConfig.ApplicationName),
                                                        gingerRunner.ApplicationAgents);
                            }
                            /// Create new temporary AppAgent for this Execution Request
                            else
                            {
                                appAgent = new ApplicationAgent();
                                gingerRunner.ApplicationAgents.Add(appAgent);
                            }

                            ApplicationPlatform app = (ApplicationPlatform)FindItemByIDAndName<ApplicationPlatform>(
                            new Tuple<string, Guid?>(nameof(ApplicationPlatform.Guid), appAgentConfig.ApplicationID),
                            new Tuple<string, string>(nameof(ApplicationPlatform.AppName), appAgentConfig.ApplicationName),
                            WorkSpace.Instance.Solution.ApplicationPlatforms);

                            Agent agent = (Agent)FindItemByIDAndName<Agent>(
                                                        new Tuple<string, Guid?>(nameof(Agent.Guid), appAgentConfig.AgentID),
                                                        new Tuple<string, string>(nameof(Agent.Name), appAgentConfig.AgentName),
                                                        WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>());

                            ApplicationAgentOperations applicationAgentOperations = new ApplicationAgentOperations(appAgent);
                            appAgent.ApplicationAgentOperations = applicationAgentOperations;
                            appAgent.Agent = agent;
                            appAgent.AppName = app.AppName;
                            appAgent.AppID = app.Guid;
                            appAgent.AgentName = agent.Name;
                            appAgent.AgentID = agent.Guid;
                        }
                    }

                    //Add or Update BFs
                    if (runnerConfig.BusinessFlows != null)
                    {
                        foreach (BusinessFlowExecConfig businessFlowConfig in runnerConfig.BusinessFlows)
                        {
                            BusinessFlow bf = null;
                            BusinessFlowRun businessFlowRun = null;

                            /// Look for existing Business Flows and create BusinessFlow Run for the same
                            /// Checking Exist HasValue (Nullable) for supporting old request JSONs without 'Exist' field for this property
                            if (!businessFlowConfig.Exist.HasValue || businessFlowConfig.Exist.Value)
                            {
                                //using existing BF
                                bf = FindItemByIDAndName<BusinessFlow>(
                                        new Tuple<string, Guid?>(nameof(BusinessFlow.Guid), businessFlowConfig.ID),
                                        new Tuple<string, string>(nameof(BusinessFlow.Name), businessFlowConfig.Name),
                                        WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>());

                                List<BusinessFlowRun> businessFlowRunList = gingerRunner.BusinessFlowsRunList.Where(x => x.BusinessFlowGuid == bf.Guid).ToList();
                                if (businessFlowRunList == null || businessFlowRunList.Count == 0)
                                {
                                    businessFlowRunList = gingerRunner.BusinessFlowsRunList.Where(x => x.BusinessFlowName == bf.Name).ToList();
                                }

                                if (businessFlowRunList.Count > 0)
                                {
                                    if (businessFlowConfig.Instance != null)
                                    {
                                        if (businessFlowRunList.Count >= (int)businessFlowConfig.Instance)
                                        {
                                            businessFlowRun = businessFlowRunList[(int)businessFlowConfig.Instance - 1];
                                        }
                                    }
                                    else
                                    {
                                        businessFlowRun = businessFlowRunList[0];//taking first instance as default
                                    }
                                }
                            }
                            else
                            {
                                //using Virtual BF
                                bf = new BusinessFlow() { Name = businessFlowConfig.Name };
                                ///Add Shared Activities 
                                if (businessFlowConfig.SharedActivities != null && businessFlowConfig.SharedActivities.Count > 0)
                                {
                                    foreach (var sharedActivity in businessFlowConfig.SharedActivities)
                                    {
                                        GingerCore.Activities.ActivitiesGroup actGrp = new GingerCore.Activities.ActivitiesGroup()
                                        {
                                            Name = sharedActivity.SharedActivityName
                                        };
                                        bf.AddActivitiesGroup(actGrp);

                                        var SolutionActivities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                                        Activity shActivity = FindItemByIDAndName<Activity>(
                                         new Tuple<string, Guid?>(nameof(Activity.Guid), sharedActivity.SharedActivityID),
                                         new Tuple<string, string>(nameof(Activity.ActivityName), sharedActivity.SharedActivityName),
                                         WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>());
                                        bf.AddActivity(shActivity, actGrp);
                                    }
                                }
                                else
                                {
                                    string error = string.Format("Failed to find shared Activities to be added to the Virtual Business Flow");
                                    throw new Exception(error);
                                }
                                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(bf, true);
                            }

                            if (businessFlowRun == null)
                            {
                                businessFlowRun = new BusinessFlowRun();
                                businessFlowRun.BusinessFlowGuid = bf.Guid;
                                businessFlowRun.BusinessFlowName = bf.Name;
                                businessFlowRun.BusinessFlowIsActive = true;
                                businessFlowRun.BusinessFlowInstanceGuid = businessFlowConfig.InstanceID.HasValue ? businessFlowConfig.InstanceID.Value : Guid.NewGuid();
                                gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);
                            }
                           
                            if (businessFlowConfig.Active != null)
                            {
                                businessFlowRun.BusinessFlowIsActive = (bool)businessFlowConfig.Active;
                            }

                            //Set/Update BF Input Variables
                            if (businessFlowConfig.InputValues != null)
                            {
                                ObservableList<VariableBase> allInputVars = null;
                                allInputVars = bf.GetBFandActivitiesVariabeles(includeParentDetails: true, includeOnlySetAsInputValue: true);

                                foreach (InputValue inputValueConfig in businessFlowConfig.InputValues)
                                {
                                    VariableBase customizedInputVar = null;
                                    if (dynamicRunsetConfigs.Exist && businessFlowRun.BusinessFlowCustomizedRunVariables.Count > 0)
                                    {
                                        customizedInputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentGuid == inputValueConfig.VariableParentID && v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                        if (customizedInputVar == null)
                                        {
                                            customizedInputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                            if (customizedInputVar == null)
                                            {
                                                customizedInputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentName == inputValueConfig.VariableParentName && v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                if (customizedInputVar == null)
                                                {
                                                    customizedInputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                }
                                            }
                                        }
                                    }

                                    if (customizedInputVar == null && allInputVars != null)
                                    {
                                        VariableBase inputVar = null;
                                        inputVar = allInputVars.Where(v => v.ParentGuid == inputValueConfig.VariableParentID && v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                        if (inputVar == null)
                                        {
                                            inputVar = allInputVars.Where(v => v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                            if (inputVar == null)
                                            {
                                                inputVar = allInputVars.Where(v => v.ParentName == inputValueConfig.VariableParentName && v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                if (inputVar == null)
                                                {
                                                    inputVar = allInputVars.Where(v => v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                }
                                            }
                                        }
                                        if (inputVar != null)
                                        {
                                            customizedInputVar = (VariableBase)inputVar.CreateCopy(false);
                                            businessFlowRun.BusinessFlowCustomizedRunVariables.Add(customizedInputVar);
                                        }
                                    }

                                    if (customizedInputVar != null)
                                    {
                                        customizedInputVar.DiffrentFromOrigin = true;
                                        switch (inputValueConfig.VariableCustomizationType)
                                        {
                                            case InputValue.eVariableCustomizationType.Value:
                                                customizedInputVar.VarValChanged = true;
                                                customizedInputVar.Value = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            case InputValue.eVariableCustomizationType.Variable://saving variable by Name- Legacy
                                                customizedInputVar.MappedOutputType = VariableBase.eOutputType.Variable;
                                                customizedInputVar.MappedOutputValue = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            case InputValue.eVariableCustomizationType.OutputVariable:
                                                customizedInputVar.MappedOutputType = VariableBase.eOutputType.OutputVariable;
                                                customizedInputVar.MappedOutputValue = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            case InputValue.eVariableCustomizationType.GlobalVariable:
                                                customizedInputVar.MappedOutputType = VariableBase.eOutputType.GlobalVariable;
                                                customizedInputVar.MappedOutputValue = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            case InputValue.eVariableCustomizationType.ApplicationModelParameter:
                                                customizedInputVar.MappedOutputType = VariableBase.eOutputType.ApplicationModelParameter;
                                                customizedInputVar.MappedOutputValue = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            case InputValue.eVariableCustomizationType.DataSource:
                                                customizedInputVar.MappedOutputType = VariableBase.eOutputType.DataSource;
                                                customizedInputVar.MappedOutputValue = inputValueConfig.VariableCustomizedValue;
                                                break;
                                            default:
                                                customizedInputVar.VarValChanged = true;
                                                customizedInputVar.Value = inputValueConfig.VariableCustomizedValue;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        string error = string.Format("Failed to find Input Variable with the details '{0}/{1}'", inputValueConfig.VariableName, inputValueConfig.VariableID);
                                        throw new Exception(error);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Add/Update Runset Operations
            if (dynamicRunsetConfigs.Operations != null)
            {
                foreach (OperationExecConfigBase runsetOperationConfig in dynamicRunsetConfigs.Operations)
                {
                    RunSetActionBase runSetOperation = null;
                    if (runsetOperationConfig is MailReportOperationExecConfig)
                    {
                        MailReportOperationExecConfig runsetOperationConfigMail = (MailReportOperationExecConfig)runsetOperationConfig;
                        RunSetActionHTMLReportSendEmail mailOperation = null;
                        if (dynamicRunsetConfigs.Exist)
                        {
                            try
                            {
                                RunSetActionBase oper = FindItemByIDAndName<RunSetActionBase>(
                                                    new Tuple<string, Guid?>(nameof(RunSetActionBase.Guid), runsetOperationConfigMail.ID),
                                                    new Tuple<string, string>(nameof(RunSetActionBase.Name), runsetOperationConfigMail.Name),
                                                    runSetConfig.RunSetActions);
                                if (oper != null)
                                {
                                    mailOperation = (RunSetActionHTMLReportSendEmail)oper;
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.INFO, string.Format("{0} operation was not found so configuring new one", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                            }
                        }

                        if (mailOperation == null)//not found 
                        {
                            mailOperation = new RunSetActionHTMLReportSendEmail();
                            //defualt settings
                            mailOperation.selectedHTMLReportTemplateID = 100;//ID to mark defualt template
                        }

                        //mail settings
                        if (runsetOperationConfigMail.MailSettings.EmailMethod != null)
                        {
                            if (runsetOperationConfigMail.MailSettings.EmailMethod == SendMailSettings.eEmailMethod.OUTLOOK)
                            {
                                mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK;
                            }
                            else
                            {
                                mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.SMTP;
                                if (runsetOperationConfigMail.MailSettings.SmtpDetails != null)
                                {
                                    if (runsetOperationConfigMail.MailSettings.SmtpDetails.Server != null)
                                    {
                                        mailOperation.Email.SMTPMailHost = runsetOperationConfigMail.MailSettings.SmtpDetails.Server;
                                    }
                                    if (runsetOperationConfigMail.MailSettings.SmtpDetails.Port != null)
                                    {
                                        mailOperation.Email.SMTPPort = int.Parse(runsetOperationConfigMail.MailSettings.SmtpDetails.Port);
                                    }
                                    if (runsetOperationConfigMail.MailSettings.SmtpDetails.EnableSSL != null)
                                    {
                                        mailOperation.Email.EnableSSL = (bool)runsetOperationConfigMail.MailSettings.SmtpDetails.EnableSSL;
                                    }
                                    if (string.IsNullOrEmpty(runsetOperationConfigMail.MailSettings.SmtpDetails.User) == false)
                                    {
                                        mailOperation.Email.ConfigureCredential = true;
                                        mailOperation.Email.SMTPUser = runsetOperationConfigMail.MailSettings.SmtpDetails.User;
                                        mailOperation.Email.SMTPPass = runsetOperationConfigMail.MailSettings.SmtpDetails.Password;
                                    }
                                }
                            }
                        }
                        if (runsetOperationConfigMail.MailSettings.MailFrom != null)
                        {
                            mailOperation.MailFrom = runsetOperationConfigMail.MailSettings.MailFrom;
                            mailOperation.Email.MailFrom = runsetOperationConfigMail.MailSettings.MailFrom;
                        }
                        if (runsetOperationConfigMail.MailSettings.MailFromDisplayName != null)
                        {
                            mailOperation.MailFromDisplayName = runsetOperationConfigMail.MailSettings.MailFromDisplayName;
                            mailOperation.Email.MailFromDisplayName = runsetOperationConfigMail.MailSettings.MailFromDisplayName;
                        }
                        if (runsetOperationConfigMail.MailSettings.MailTo != null)
                        {
                            mailOperation.MailTo = runsetOperationConfigMail.MailSettings.MailTo;
                            mailOperation.Email.MailTo = runsetOperationConfigMail.MailSettings.MailTo;
                        }
                        if (runsetOperationConfigMail.MailSettings.MailCC != null)
                        {
                            mailOperation.MailCC = runsetOperationConfigMail.MailSettings.MailCC;
                        }
                        if (runsetOperationConfigMail.MailSettings.Subject != null)
                        {
                            mailOperation.Subject = runsetOperationConfigMail.MailSettings.Subject;
                            mailOperation.Email.Subject = runsetOperationConfigMail.MailSettings.Subject;
                        }

                        //report mail content
                        if (runsetOperationConfigMail.Comments != null)
                        {
                            mailOperation.Comments = runsetOperationConfigMail.Comments;
                        }
                        if (runsetOperationConfigMail.BodyContentType != null)
                        {
                            switch (runsetOperationConfigMail.BodyContentType)
                            {
                                case MailReportOperationExecConfig.eBodyContentType.HTMLReport:
                                    mailOperation.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport;
                                    break;
                                case MailReportOperationExecConfig.eBodyContentType.FreeText:
                                    mailOperation.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.FreeText;
                                    break;
                            }
                        }
                        if (runsetOperationConfigMail.BodyReportTemplateID != null)
                        {
                            mailOperation.selectedHTMLReportTemplateID = (int)runsetOperationConfigMail.BodyReportTemplateID;
                        }
                        if (runsetOperationConfigMail.BodyTextContent != null)
                        {
                            mailOperation.Bodytext = runsetOperationConfigMail.BodyTextContent;
                        }

                        //report attachments
                        EmailAttachment reportAttachment = mailOperation.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault();
                        if (runsetOperationConfigMail.IncludeAttachmentReport != null)
                        {
                            if (runsetOperationConfigMail.IncludeAttachmentReport == true && reportAttachment == null)
                            {
                                //defualt settings for report attachment
                                reportAttachment = new EmailHtmlReportAttachment();
                                reportAttachment.AttachmentType = EmailAttachment.eAttachmentType.Report;
                                ((EmailHtmlReportAttachment)reportAttachment).SelectedHTMLReportTemplateID = 100;//default report template
                                reportAttachment.ZipIt = true;
                                mailOperation.EmailAttachments.Add(reportAttachment);
                            }
                            else if (runsetOperationConfigMail.IncludeAttachmentReport == false && reportAttachment != null)
                            {
                                mailOperation.EmailAttachments.Remove(reportAttachment);
                                reportAttachment = null;
                            }
                        }
                        if (reportAttachment != null && runsetOperationConfigMail.AttachmentReportTemplateID != null)
                        {
                            ((EmailHtmlReportAttachment)reportAttachment).SelectedHTMLReportTemplateID = (int)runsetOperationConfigMail.AttachmentReportTemplateID;
                        }
                        if (reportAttachment != null && runsetOperationConfigMail.AttachmentReportAttachType != null)
                        {
                            switch (runsetOperationConfigMail.AttachmentReportAttachType)
                            {
                                case MailReportOperationExecConfig.ReportAttachType.Zip:
                                    ((EmailHtmlReportAttachment)reportAttachment).ZipIt = true;
                                    ((EmailHtmlReportAttachment)reportAttachment).IsLinkEnabled = false;
                                    break;
                                case MailReportOperationExecConfig.ReportAttachType.Link:
                                    ((EmailHtmlReportAttachment)reportAttachment).ZipIt = false;
                                    ((EmailHtmlReportAttachment)reportAttachment).IsLinkEnabled = true;
                                    break;
                            }
                        }
                        if (reportAttachment != null && runsetOperationConfigMail.CustomizedReportFolder != null)
                        {
                            ((EmailHtmlReportAttachment)reportAttachment).ExtraInformation = runsetOperationConfigMail.CustomizedReportFolder;
                            if (string.IsNullOrEmpty(runsetOperationConfigMail.CustomizedReportFolder))
                            {
                                ((EmailHtmlReportAttachment)reportAttachment).IsAlternameFolderUsed = false;
                            }
                            else
                            {
                                ((EmailHtmlReportAttachment)reportAttachment).IsAlternameFolderUsed = true;
                            }
                        }

                        if (runsetOperationConfigMail.FilesPathToAttach != null)
                        {
                            foreach (string attachmentPath in runsetOperationConfigMail.FilesPathToAttach)
                            {
                                EmailAttachment exsitingAttachment = mailOperation.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.File && x.Name.Trim().ToLower() == attachmentPath.Trim().ToLower()).FirstOrDefault();
                                if (exsitingAttachment == null)
                                {
                                    mailOperation.EmailAttachments.Add(new EmailAttachment() { AttachmentType = EmailAttachment.eAttachmentType.File, Name = attachmentPath });
                                }
                            }
                        }
                        runSetOperation = mailOperation;
                    }
                    else if (runsetOperationConfig is JsonReportOperationExecConfig)
                    {
                        JsonReportOperationExecConfig runsetOperationConfigJsonRepot = (JsonReportOperationExecConfig)runsetOperationConfig;
                        RunSetActionJSONSummary jsonReportOperation = null;
                        if (dynamicRunsetConfigs.Exist)
                        {
                            RunSetActionBase oper = FindItemByIDAndName<RunSetActionBase>(
                                                new Tuple<string, Guid?>(nameof(RunSetActionBase.Guid), runsetOperationConfigJsonRepot.ID),
                                                new Tuple<string, string>(nameof(RunSetActionBase.Name), runsetOperationConfigJsonRepot.Name),
                                                runSetConfig.RunSetActions);

                            if (oper != null)
                            {
                                jsonReportOperation = (RunSetActionJSONSummary)oper;
                            }
                        }
                        else
                        {
                            jsonReportOperation = new RunSetActionJSONSummary();
                        }
                        runSetOperation = jsonReportOperation;
                    }
                    else if (runsetOperationConfig is AlmPublishOperationExecConfig)
                    {
                        AlmPublishOperationExecConfig publishToALMOperationExecConfig = runsetOperationConfig as AlmPublishOperationExecConfig;

                        RunSetActionPublishToQC publishToQCRunSetOperation = null;
                        if (dynamicRunsetConfigs.Exist)
                        {
                            try
                            {
                                RunSetActionBase oper = FindItemByIDAndName<RunSetActionBase>(
                                                    new Tuple<string, Guid?>(nameof(RunSetActionBase.Guid), publishToALMOperationExecConfig.ID),
                                                    new Tuple<string, string>(nameof(RunSetActionBase.Name), publishToALMOperationExecConfig.Name),
                                                    runSetConfig.RunSetActions);
                                if (oper != null)
                                {
                                    publishToQCRunSetOperation = (RunSetActionPublishToQC)oper;
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.INFO, string.Format("{0} operation was not found so configuring new one", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                            }
                        }

                        if (publishToQCRunSetOperation == null)//Not Found so adding new one
                        {
                            publishToQCRunSetOperation = new RunSetActionPublishToQC();
                        }
                        if(publishToALMOperationExecConfig.ALMType.ToLower() == "default")
                        {
                            publishToQCRunSetOperation.PublishALMType = gingerExecConfig.AlmsDetails.Where(x=> x.IsDefault !=null && x.IsDefault.Value== true).FirstOrDefault().ALMType;
                        }
                        else
                        {
                            publishToQCRunSetOperation.PublishALMType = publishToALMOperationExecConfig.ALMType;
                        }

                        if (publishToALMOperationExecConfig.AlmTestSetLevel != null)
                        {
                            publishToQCRunSetOperation.ALMTestSetLevel = (GingerCore.ALM.PublishToALMConfig.eALMTestSetLevel)publishToALMOperationExecConfig.AlmTestSetLevel.Value;
                        }

                        if (publishToALMOperationExecConfig.ExportType != null)
                        {
                            publishToQCRunSetOperation.ExportType = (GingerCore.ALM.PublishToALMConfig.eExportType)publishToALMOperationExecConfig.ExportType.Value;
                        }

                        if (!string.IsNullOrEmpty(publishToALMOperationExecConfig.TestsetExportDestination))
                        {
                            publishToQCRunSetOperation.TestSetFolderDestination = publishToALMOperationExecConfig.TestsetExportDestination;
                        }

                        if (!string.IsNullOrEmpty(publishToALMOperationExecConfig.TestcasesExportDestination))
                        {
                            publishToQCRunSetOperation.TestCaseFolderDestination = publishToALMOperationExecConfig.TestcasesExportDestination;
                        }

                        if (publishToALMOperationExecConfig.TestCasesResultsToExport != null)
                        {
                             publishToQCRunSetOperation.FilterStatus = (GingerCore.ALM.FilterByStatus)publishToALMOperationExecConfig.TestCasesResultsToExport.Value;
                        }

                        publishToQCRunSetOperation.toAttachActivitiesGroupReport = publishToALMOperationExecConfig.AttachActivitiesGroupsReport;

                        publishToQCRunSetOperation.isVariableInTCRunUsed = publishToALMOperationExecConfig.UseUserVariableInRunInstanceName;

                        if (!string.IsNullOrEmpty(publishToALMOperationExecConfig.UserVariableInRunInstance))
                        {
                            publishToQCRunSetOperation.VariableForTCRunName = publishToALMOperationExecConfig.UserVariableInRunInstance;
                        }

                        if(publishToALMOperationExecConfig.AlmFieldsConfig != null && publishToALMOperationExecConfig.AlmFieldsConfig.Count > 0)
                        {
                            foreach(var almFieldConfig in publishToALMOperationExecConfig.AlmFieldsConfig)
                            {
                                ExternalItemFieldBase extItemFieldBase;

                                if (!string.IsNullOrEmpty(almFieldConfig.FieldParentType))
                                {
                                    extItemFieldBase = new ExternalItemFieldBase()
                                    {
                                        ItemType = almFieldConfig.FieldParentType
                                    };
                                }
                                else
                                {
                                    continue;
                                }

                                extItemFieldBase.ExternalID = almFieldConfig.FieldBackendName;
                                extItemFieldBase.ID = almFieldConfig.FieldID;
                                extItemFieldBase.Name = almFieldConfig.FieldFrontendName;
                                extItemFieldBase.SystemFieled = almFieldConfig.IsSystemField;
                                extItemFieldBase.Type = almFieldConfig.FieldType;
                                extItemFieldBase.SelectedValue = almFieldConfig.FieldValue;

                                publishToQCRunSetOperation.AlmFields.Add(extItemFieldBase);
                            }
                        }

                        runSetOperation = publishToQCRunSetOperation;
                    }

                    //Generic settings
                    if (runSetOperation != null)
                    {
                        runSetOperation.Name = runsetOperationConfig.Name;
                        if (runsetOperationConfig.Active != null)
                        {
                            runSetOperation.Active = (bool)runsetOperationConfig.Active;
                        }
                        if (runsetOperationConfig.Condition != null)
                        {
                            runSetOperation.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), runsetOperationConfig.Condition.ToString(), true);
                        }
                        if (runsetOperationConfig.RunAt != null)
                        {
                            runSetOperation.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), runsetOperationConfig.RunAt.ToString(), true);
                        }

                        if (!runSetConfig.RunSetActions.Contains(runSetOperation))
                        {
                            runSetConfig.RunSetActions.Add(runSetOperation);
                        }
                    }
                }
            }

            // add selfhealing configs
            if (dynamicRunsetConfigs.SelfHealingConfiguration != null)
            {
                runSetConfig.SelfHealingConfiguration.AutoFixAnalyzerIssue = dynamicRunsetConfigs.SelfHealingConfiguration.AutoFixAnalyzerIssue;
                runSetConfig.SelfHealingConfiguration.AutoUpdateApplicationModel = dynamicRunsetConfigs.SelfHealingConfiguration.AutoUpdateApplicationModel;
                runSetConfig.SelfHealingConfiguration.SaveChangesInSourceControl = dynamicRunsetConfigs.SelfHealingConfiguration.SaveChangesInSourceControl;
                runSetConfig.SelfHealingConfiguration.ReprioritizePOMLocators = dynamicRunsetConfigs.SelfHealingConfiguration.ReprioritizePOMLocators;
                runSetConfig.SelfHealingConfiguration.AutoExecuteInSimulationMode = dynamicRunsetConfigs.SelfHealingConfiguration.AutoExecuteInSimulationMode;
            }
            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }

        public static T FindItemByIDAndName<T>(Tuple<string, Guid?> id, Tuple<string, string> name, ObservableList<T> repoLibrary)
        {
            T item = default(T);

            try
            {
                if (id.Item2 != null && id.Item2 != Guid.Empty)
                {
                    if (typeof(T).GetProperty(id.Item1) != null)
                    {
                        item = repoLibrary.Where(x => (Guid)(typeof(T).GetProperty(id.Item1).GetValue(x)) == id.Item2).FirstOrDefault();
                    }
                    else if (typeof(T).GetField(id.Item1) != null)
                    {
                        item = repoLibrary.Where(x => (Guid)(typeof(T).GetField(id.Item1).GetValue(x)) == id.Item2).FirstOrDefault();
                    }
                }

                if (item == null && !string.IsNullOrEmpty(name.Item2))
                {
                    if (typeof(T).GetProperty(name.Item1) != null)
                    {
                        item = repoLibrary.Where(x => typeof(T).GetProperty(name.Item1).GetValue(x).ToString().ToLower() == name.Item2.ToLower()).FirstOrDefault();
                    }
                    else if (typeof(T).GetField(name.Item1) != null)
                    {
                        item = repoLibrary.Where(x => typeof(T).GetField(name.Item1).GetValue(x).ToString().ToLower() == name.Item2.ToLower()).FirstOrDefault();
                    }
                }

                if (item != null)
                {
                    return item;
                }
                else
                {
                    string error = string.Format("Failed to find {0} with the details '{1}/{2}'", typeof(T), name.Item2.ToLower(), id.Item2);
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to find {0} with the details '{1}/{2}'", typeof(T), name.Item2.ToLower(), id.Item2);
                throw new Exception(error, ex);
            }
        }

        public static bool IsJson(string content)
        {
            content = content.Trim();
            return content.StartsWith("{") && content.EndsWith("}")
                   || content.StartsWith("[") && content.EndsWith("]");
        }
        #endregion JSON        
    }
}
