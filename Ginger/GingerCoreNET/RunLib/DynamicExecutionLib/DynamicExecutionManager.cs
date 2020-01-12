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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SourceControl;
using GingerExecuterService.Contracts.V1.ExecutionConfigurations;
using GingerExecuterService.Contracts.V1.ExecutionConfigurations.RunsetOperations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

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
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
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
                if (gingerRunner.UseSpecificEnvironment == true && string.IsNullOrEmpty(gingerRunner.SpecificEnvironmentName))
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
            }

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }
        #endregion XML

        #region JSON
        public static string CreateDynamicRunSetJSON(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            runsetExecutor.RunSetConfig.UpdateRunnersBusinessFlowRunsList();

            //Create execution object
            ExecutionConfiguration executionConfig = new ExecutionConfiguration();
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
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    executionConfig.SolutionScmDetails.ProxyServer = solution.SourceControl.SourceControlProxyAddress.ToString();
                    executionConfig.SolutionScmDetails.ProxyPort = solution.SourceControl.SourceControlProxyPort.ToString();
                }
            }
            executionConfig.SolutionLocalPath = solution.Folder;
            executionConfig.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;

            Runset runset = new Runset();
            runset.Exist = true;
            runset.Name = runsetExecutor.RunSetConfig.Name;
            runset.ID = runsetExecutor.RunSetConfig.Guid;
            runset.Environment = runsetExecutor.RunsetExecutionEnvironment.Name;
            runset.RunAnalyzer = cliHelper.RunAnalyzer;
            runset.RunInParallel = runsetExecutor.RunSetConfig.RunModeParallel;

            if (runsetExecutor.RunSetConfig.GingerRunners.Count > 0)
            {
                runset.Runners = new List<Runner>();
            }
            foreach (GingerRunner gingerRunner in runsetExecutor.RunSetConfig.GingerRunners)
            {
                Runner runner = new Runner();
                runner.Name = gingerRunner.Name;
                runner.ID = gingerRunner.Guid;
                if (gingerRunner.UseSpecificEnvironment == true && string.IsNullOrEmpty(gingerRunner.SpecificEnvironmentName))
                {
                    runner.Environment = gingerRunner.SpecificEnvironmentName;
                }
                if (gingerRunner.RunOption != GingerRunner.eRunOptions.ContinueToRunall)
                {
                    runner.OnFailureRunOption = (Runner.eOnFailureRunOption)Enum.Parse(typeof(Runner.eOnFailureRunOption), gingerRunner.RunOption.ToString(), true);                    
                }

                if (gingerRunner.ApplicationAgents.Count > 0)
                {
                    runner.AppAgentMappings = new List<AppAgentMapping>();
                }
                foreach (ApplicationAgent applicationAgent in gingerRunner.ApplicationAgents)
                {
                    runner.AppAgentMappings.Add(new AppAgentMapping() { AgentName = applicationAgent.AgentName, AgentID = applicationAgent.AgentID, ApplicationName = applicationAgent.AppName, ApplicationID = applicationAgent.AppID});
                }

                if (gingerRunner.BusinessFlowsRunList.Count > 0)
                {
                    runner.BusinessFlows = new List<GingerExecuterService.Contracts.V1.ExecutionConfigurations.BusinessFlow>();
                }
                foreach (BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    GingerExecuterService.Contracts.V1.ExecutionConfigurations.BusinessFlow businessFlow = new GingerExecuterService.Contracts.V1.ExecutionConfigurations.BusinessFlow();
                    businessFlow.Name = businessFlowRun.BusinessFlowName;
                    businessFlow.ID = businessFlowRun.BusinessFlowGuid;//probably need to go with BusinessFlowInstanceGuid to support multi BF's from same type
                    businessFlow.Active = businessFlowRun.BusinessFlowIsActive;
                    if (businessFlowRun.BusinessFlowCustomizedRunVariables.Count > 0)
                    {
                        businessFlow.InputValues = new List<InputValue>();
                        foreach (VariableBase variableBase in businessFlowRun.BusinessFlowCustomizedRunVariables)
                        {
                            InputValue inputVal = new InputValue();
                            inputVal.VariableParentName = variableBase.ParentName;
                            if (variableBase.ParentType == "Business Flow")
                            {
                                inputVal.VariableParentID = businessFlowRun.BusinessFlowGuid;
                            }
                            else
                            {
                                //??? need the ID of the Activity
                            }

                            inputVal.VariableName = variableBase.Name;
                            inputVal.VariableID = variableBase.Guid;
                            inputVal.VariableCustomizedValue = variableBase.Value;

                            businessFlow.InputValues.Add(inputVal);
                        }
                    }
                    runner.BusinessFlows.Add(businessFlow);
                }
                runset.Runners.Add(runner);
            }

            if (runsetExecutor.RunSetConfig.RunSetActions.Count > 0)
            {
                runset.Operations = new List<Operation>();
            }
            foreach (RunSetActionBase runSetOperation in runsetExecutor.RunSetConfig.RunSetActions)
            {
                if (runSetOperation is RunSetActionHTMLReportSendEmail)
                {
                    RunSetActionHTMLReportSendEmail runsetMailReport = (RunSetActionHTMLReportSendEmail)runSetOperation;
                    MailReportOperation mailReportConfig = new MailReportOperation();
                    mailReportConfig.Name = runsetMailReport.Name;
                    mailReportConfig.ID = runsetMailReport.Guid;
                    mailReportConfig.Condition = (Operation.eOperationRunCondition)Enum.Parse(typeof(Operation.eOperationRunCondition), runsetMailReport.Condition.ToString(), true);
                    mailReportConfig.RunAt = (Operation.eOperationRunAt)Enum.Parse(typeof(Operation.eOperationRunAt), runsetMailReport.RunAt.ToString(), true);

                    if (runsetMailReport.Email.EmailMethod == GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK)
                    {
                        mailReportConfig.MailSettings.EmailMethod = SendMailSettings.eEmailMethod.OUTLOOK;
                    }
                    else
                    {
                        mailReportConfig.MailSettings.SmtpDetails = new GingerExecuterService.Contracts.V1.ExecutionConfigurations.RunsetOperations.SmtpDetails();
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
                    mailReportConfig.MailSettings.MailTo = runsetMailReport.MailTo;
                    mailReportConfig.MailSettings.MailCC = runsetMailReport.MailCC;
                    mailReportConfig.MailSettings.Subject = runsetMailReport.Subject;

                    mailReportConfig.Comments = runsetMailReport.Comments;
                    if (runsetMailReport.EmailAttachments.Where(x => x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault() != null)
                    {
                        mailReportConfig.IncludeAttachmentReport = true;
                    }
                    else
                    {
                        mailReportConfig.IncludeAttachmentReport = false;
                    }

                    runset.Operations.Add(mailReportConfig);
                }
                else if (runSetOperation is RunSetActionJSONSummary)
                {
                    JsonReportOperation jsonReportConfig = new JsonReportOperation();
                    jsonReportConfig.Name = runSetOperation.Name;
                    jsonReportConfig.ID = runSetOperation.Guid;
                    runset.Operations.Add(jsonReportConfig);
                }
            }
            executionConfig.Runset = runset;

            //serilize object to JSON String
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            return JsonConvert.SerializeObject(executionConfig, jsonSerializerSettings);
        }

        public static ExecutionConfiguration LoadDynamicExecutionFromJSON(string content)
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            return (ExecutionConfiguration)JsonConvert.DeserializeObject(content, typeof(ExecutionConfiguration), jsonSerializerSettings);
        }

        public static void CreateUpdateRunSetFromJSON(RunsetExecutor runsetExecutor, Runset dynamicRunsetConfigs)
        {
            RunSetConfig runSetConfig = null;
            if (dynamicRunsetConfigs.Exist)
            {
                //## Updating existing Runset
                runSetConfig = FindItemByIDAndName<RunSetConfig>(
                    new Tuple<string, Guid?>(nameof(RunSetConfig.Guid), dynamicRunsetConfigs.ID),
                    new Tuple<string, string>(nameof(RunSetConfig.Name), dynamicRunsetConfigs.Name),
                    WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>());
            }
            else
            {
                //## Creating new Runset
                runSetConfig = new RunSetConfig();
                runSetConfig.Name = dynamicRunsetConfigs.Name;
            }

            if (dynamicRunsetConfigs.RunAnalyzer != null)
            {
                runSetConfig.RunWithAnalyzer = (bool)dynamicRunsetConfigs.RunAnalyzer;
            }

            if (dynamicRunsetConfigs.RunInParallel != null)
            {
                runSetConfig.RunModeParallel = (bool)dynamicRunsetConfigs.RunInParallel;
            }

            //Add or Update Runners
            if (dynamicRunsetConfigs.Runners != null)
            {
                foreach (Runner runnerConfig in dynamicRunsetConfigs.Runners)
                {
                    GingerRunner gingerRunner = null;
                    if (dynamicRunsetConfigs.Exist)
                    {
                        gingerRunner = FindItemByIDAndName<GingerRunner>(
                            new Tuple<string, Guid?>(nameof(GingerRunner.Guid), runnerConfig.ID),
                            new Tuple<string, string>(nameof(GingerRunner.Name), runnerConfig.Name),
                            runSetConfig.GingerRunners);
                    }
                    else
                    {
                        gingerRunner = new GingerRunner();
                        gingerRunner.Name = runnerConfig.Name;
                    }

                    if (!string.IsNullOrEmpty(runnerConfig.Environment))
                    {
                        gingerRunner.UseSpecificEnvironment = true;
                        gingerRunner.SpecificEnvironmentName = runnerConfig.Environment;
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
                            GingerCore.Platforms.ApplicationAgent appAgent = null;
                            if (dynamicRunsetConfigs.Exist)
                            {
                                appAgent = (ApplicationAgent)FindItemByIDAndName<IApplicationAgent>(
                                                        new Tuple<string, Guid?>(nameof(IApplicationAgent.AppID), appAgentConfig.ApplicationID),
                                                        new Tuple<string, string>(nameof(IApplicationAgent.AppName), appAgentConfig.ApplicationName),
                                                        gingerRunner.ApplicationAgents);
                            }
                            else
                            {
                                appAgent = new ApplicationAgent();
                                appAgent.AppName = appAgentConfig.ApplicationName;
                                if (appAgentConfig.ApplicationID != null)
                                {
                                    appAgent.AppID = (Guid)appAgentConfig.ApplicationID;
                                }
                                gingerRunner.ApplicationAgents.Add(appAgent);
                            }

                            appAgent.AgentName = appAgentConfig.AgentName;
                            if (appAgentConfig.AgentID != null)
                            {
                                appAgent.AgentID = (Guid)appAgentConfig.AgentID;
                            }
                        }
                    }

                    //Add or Update BFs
                    if (runnerConfig.BusinessFlows != null)
                    {
                        foreach (GingerExecuterService.Contracts.V1.ExecutionConfigurations.BusinessFlow businessFlowConfig in runnerConfig.BusinessFlows)
                        {
                            BusinessFlowRun businessFlowRun = null;
                            if (dynamicRunsetConfigs.Exist)
                            {
                                businessFlowRun = FindItemByIDAndName<BusinessFlowRun>(
                                                new Tuple<string, Guid?>(nameof(BusinessFlowRun.BusinessFlowGuid), businessFlowConfig.ID),
                                                new Tuple<string, string>(nameof(BusinessFlowRun.BusinessFlowName), businessFlowConfig.Name),
                                                gingerRunner.BusinessFlowsRunList);
                            }
                            else
                            {
                                businessFlowRun = new BusinessFlowRun();
                                businessFlowRun.BusinessFlowName = businessFlowConfig.Name;
                                if (businessFlowConfig.ID != null)
                                {
                                    businessFlowRun.BusinessFlowGuid = (Guid)businessFlowConfig.ID;
                                }
                                businessFlowRun.BusinessFlowIsActive = true;
                            }

                            if (businessFlowConfig.Active != null)
                            {
                                businessFlowRun.BusinessFlowIsActive = (bool)businessFlowConfig.Active;
                            }

                            //Set/Update BF Input Variables
                            if (businessFlowConfig.InputValues != null)
                            {
                                foreach (InputValue inputValueConfig in businessFlowConfig.InputValues)
                                {
                                    VariableBase inputVar = null;
                                    if (dynamicRunsetConfigs.Exist)
                                    {
                                        inputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentGuid == inputValueConfig.VariableParentID && v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                        if (inputVar == null)
                                        {
                                            inputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.Guid == inputValueConfig.VariableID).FirstOrDefault();
                                            if (inputVar == null)
                                            {
                                                inputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.ParentName == inputValueConfig.VariableParentName && v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                if (inputVar == null)
                                                {
                                                    inputVar = businessFlowRun.BusinessFlowCustomizedRunVariables.Where(v => v.Name == inputValueConfig.VariableName).FirstOrDefault();
                                                }
                                            }
                                        }

                                        if (inputVar != null)
                                        {
                                            inputVar.DiffrentFromOrigin = true;
                                            inputVar.VarValChanged = true;
                                            inputVar.Value = inputValueConfig.VariableCustomizedValue;
                                        }
                                    }

                                    if (inputVar == null)
                                    {
                                        inputVar = new VariableString();//type is not matter
                                        inputVar.DiffrentFromOrigin = true;
                                        inputVar.VarValChanged = true;
                                        inputVar.ParentName = inputValueConfig.VariableParentName;
                                        if (inputValueConfig.VariableParentID != null)
                                        {
                                            inputVar.ParentGuid = (Guid)inputValueConfig.VariableParentID;
                                        }
                                        inputVar.Name = inputValueConfig.VariableName;
                                        if (inputValueConfig.VariableID != null)
                                        {
                                            inputVar.Guid = (Guid)inputValueConfig.VariableID;
                                        }
                                        inputVar.Value = inputValueConfig.VariableCustomizedValue;
                                        businessFlowRun.BusinessFlowCustomizedRunVariables.Add(inputVar);
                                    }
                                }
                            }

                            if (!dynamicRunsetConfigs.Exist)
                            {
                                gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);
                            }
                        }
                        if (!dynamicRunsetConfigs.Exist)
                        {
                            runSetConfig.GingerRunners.Add(gingerRunner);
                        }
                    }
                }
            }

            //Add/Update Runset Operations
            if (dynamicRunsetConfigs.Operations != null)
            {
                foreach (Operation runsetOperationConfig in dynamicRunsetConfigs.Operations)
                {
                    if (runsetOperationConfig is MailReportOperation)
                    {
                        MailReportOperation runsetOperationConfigMail = (MailReportOperation)runsetOperationConfig;
                        RunSetActionHTMLReportSendEmail mailOperation;
                        if (dynamicRunsetConfigs.Exist)
                        {
                            mailOperation = (RunSetActionHTMLReportSendEmail)FindItemByIDAndName<RunSetActionBase>(
                                                new Tuple<string, Guid?>(nameof(RunSetActionBase.Guid), runsetOperationConfigMail.ID),
                                                new Tuple<string, string>(nameof(RunSetActionBase.Name), runsetOperationConfigMail.Name),
                                                runSetConfig.RunSetActions);
                        }
                        else
                        {
                            mailOperation = new RunSetActionHTMLReportSendEmail();
                            mailOperation.Name = runsetOperationConfigMail.Name;
                            mailOperation.HTMLReportTemplate = RunSetActionHTMLReportSendEmail.eHTMLReportTemplate.HTMLReport;
                            mailOperation.selectedHTMLReportTemplateID = 100;//ID to mark defualt template
                            mailOperation.Email.IsBodyHTML = true;
                            mailOperation.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
                            mailOperation.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
                        }
                        if (runsetOperationConfigMail.Active != null)
                        {
                            mailOperation.Active = (bool)runsetOperationConfigMail.Active;
                        }
                        if (runsetOperationConfigMail.Condition != null)
                        {
                            mailOperation.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), runsetOperationConfigMail.Condition.ToString(), true);
                        }
                        if (runsetOperationConfigMail.RunAt != null)
                        {
                            mailOperation.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), runsetOperationConfigMail.RunAt.ToString(), true);
                        }

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
                        if (string.IsNullOrEmpty(runsetOperationConfigMail.MailSettings.MailFrom) == false)
                        {
                            mailOperation.MailFrom = runsetOperationConfigMail.MailSettings.MailFrom;
                            mailOperation.Email.MailFrom = runsetOperationConfigMail.MailSettings.MailFrom;
                        }
                        if (string.IsNullOrEmpty(runsetOperationConfigMail.MailSettings.MailTo) == false)
                        {
                            mailOperation.MailTo = runsetOperationConfigMail.MailSettings.MailTo;
                            mailOperation.Email.MailTo = runsetOperationConfigMail.MailSettings.MailTo;
                        }
                        if (string.IsNullOrEmpty(runsetOperationConfigMail.MailSettings.MailCC) == false)
                        {
                            mailOperation.MailCC = runsetOperationConfigMail.MailSettings.MailCC;
                        }
                        if (string.IsNullOrEmpty(runsetOperationConfigMail.MailSettings.Subject) == false)
                        {
                            mailOperation.Subject = runsetOperationConfigMail.MailSettings.Subject;
                            mailOperation.Email.Subject = runsetOperationConfigMail.MailSettings.Subject;
                        }

                        if (string.IsNullOrEmpty(runsetOperationConfigMail.Comments) == false)
                        {
                            mailOperation.Comments = runsetOperationConfigMail.Comments;
                        }
                        if (runsetOperationConfigMail.IncludeAttachmentReport != null && runsetOperationConfigMail.IncludeAttachmentReport == true)
                        {
                            EmailHtmlReportAttachment reportAttachment = new EmailHtmlReportAttachment();
                            reportAttachment.AttachmentType = EmailAttachment.eAttachmentType.Report;
                            reportAttachment.ZipIt = true;
                            mailOperation.EmailAttachments.Add(reportAttachment);
                        }
                        else
                        {
                            mailOperation.EmailAttachments.Clear();
                        }

                        if (dynamicRunsetConfigs.Exist)
                        {
                            runSetConfig.RunSetActions.Add(mailOperation);
                        }
                    }
                    else if (runsetOperationConfig is JsonReportOperation)
                    {
                        JsonReportOperation runsetOperationConfigJsonRepot = (JsonReportOperation)runsetOperationConfig;
                        RunSetActionJSONSummary jsonReportOperation;
                        if (dynamicRunsetConfigs.Exist)
                        {
                            jsonReportOperation = (RunSetActionJSONSummary)FindItemByIDAndName<RunSetActionBase>(
                                                new Tuple<string, Guid?>(nameof(RunSetActionBase.Guid), runsetOperationConfigJsonRepot.ID),
                                                new Tuple<string, string>(nameof(RunSetActionBase.Name), runsetOperationConfigJsonRepot.Name),
                                                runSetConfig.RunSetActions);
                        }
                        else
                        {
                            jsonReportOperation = new RunSetActionJSONSummary();
                            jsonReportOperation.Name = runsetOperationConfigJsonRepot.Name;
                            jsonReportOperation.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
                            jsonReportOperation.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
                        }

                        if (runsetOperationConfigJsonRepot.Active != null)
                        {
                            jsonReportOperation.Active = (bool)runsetOperationConfigJsonRepot.Active;
                        }
                        if (runsetOperationConfigJsonRepot.Condition != null)
                        {
                            jsonReportOperation.Condition = (RunSetActionBase.eRunSetActionCondition)Enum.Parse(typeof(RunSetActionBase.eRunSetActionCondition), runsetOperationConfigJsonRepot.Condition.ToString(), true);
                        }
                        if (runsetOperationConfigJsonRepot.RunAt != null)
                        {
                            jsonReportOperation.RunAt = (RunSetActionBase.eRunAt)Enum.Parse(typeof(RunSetActionBase.eRunAt), runsetOperationConfigJsonRepot.RunAt.ToString(), true);
                        }

                        if (dynamicRunsetConfigs.Exist)
                        {
                            runSetConfig.RunSetActions.Add(jsonReportOperation);
                        }
                    }
                }
            }

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }

        private static T FindItemByIDAndName<T>(Tuple<string, Guid?> id, Tuple<string, string> name, ObservableList<T> repoLibrary)
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
                    string error = string.Format("Failed to find {0} with the details '{0}/{1}'", typeof(T), name.Item2.ToLower(), id.Item2);
                    Reporter.ToLog(eLogLevel.ERROR, error);
                    throw new Exception(error);
                }
            }
            catch (Exception ex)
            {
                string error = string.Format("Failed to find {0} with the details '{0}/{1}'", typeof(T), name.Item2.ToLower(), id.Item2);
                Reporter.ToLog(eLogLevel.ERROR, error, ex);
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
