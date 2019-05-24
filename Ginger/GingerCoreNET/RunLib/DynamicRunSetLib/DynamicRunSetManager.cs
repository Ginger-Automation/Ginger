using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SourceControl;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using static Ginger.Run.GingerRunner;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class DynamicRunSetManager
    {

        public static string CreateDynamicRunSetXML(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            runsetExecutor.RunSetConfig.UpdateRunnersBusinessFlowRunsList();

            DynamicGingerExecution dynamicExecution = new DynamicGingerExecution();

            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {
                dynamicExecution.SolutionDetails.SourceControlDetails = new SourceControlDetails();
                dynamicExecution.SolutionDetails.SourceControlDetails.Type = solution.SourceControl.GetSourceControlType.ToString();
                dynamicExecution.SolutionDetails.SourceControlDetails.Url = solution.SourceControl.SourceControlURL.ToString();
                dynamicExecution.SolutionDetails.SourceControlDetails.User = solution.SourceControl.SourceControlUser.ToString();
                dynamicExecution.SolutionDetails.SourceControlDetails.Password = solution.SourceControl.SourceControlPass.ToString();
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    dynamicExecution.SolutionDetails.SourceControlDetails.ProxyServer = solution.SourceControl.SourceControlProxyAddress.ToString();
                    dynamicExecution.SolutionDetails.SourceControlDetails.ProxyPort = solution.SourceControl.SourceControlProxyPort.ToString();
                }
            }
            dynamicExecution.SolutionDetails = new SolutionDetails();
            dynamicExecution.SolutionDetails.Path = solution.Folder;
            dynamicExecution.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;

            AddRunset addRunset = new AddRunset();
            addRunset.Name = "Dynamic_" + runsetExecutor.RunSetConfig.Name;
            addRunset.Environment = runsetExecutor.RunsetExecutionEnvironment.Name;
            addRunset.RunAnalyzer = cliHelper.RunAnalyzer;
            
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

                foreach (ApplicationAgent applicationAgent in gingerRunner.ApplicationAgents)
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

                    if(runsetMailReport.EmailAttachments.Where(x=> x.AttachmentType == EmailAttachment.eAttachmentType.Report).FirstOrDefault() != null)
                    {
                        dynamicMailReport.IncludeAttachmentReport = true;
                    }
                    else
                    {
                        dynamicMailReport.IncludeAttachmentReport = false;
                    }

                    addRunset.AddRunsetOperations.Add(dynamicMailReport);
                }
            }
            dynamicExecution.AddRunsets.Add(addRunset);

            string content = ConvertDynamicExecutionToXML(dynamicExecution);
            return content;
        }

        private static string ConvertDynamicExecutionToXML(DynamicGingerExecution dynamicRunSet)
        {
            XmlSerializer writer = new XmlSerializer(typeof(DynamicGingerExecution));
            StringWriter stringWriter = new StringWriter();
            //XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            //ns.Add("", "");
            //writer.Serialize(stringWriter, dynamicRunSet, ns);
            writer.Serialize(stringWriter, dynamicRunSet);
            stringWriter.Close();
            return stringWriter.GetStringBuilder().ToString();
        }

        public static DynamicGingerExecution LoadDynamicExecutionFromXML(string content)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DynamicGingerExecution));
            System.IO.StringReader stringReader = new System.IO.StringReader(content);
            DynamicGingerExecution dynamicRunSet = (DynamicGingerExecution)reader.Deserialize(stringReader);
            stringReader.Close();
            return dynamicRunSet;
        }

        public static void CreateRealRunSetFromDynamic(RunsetExecutor runsetExecutor, AddRunset dynamicRunset)
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = dynamicRunset.Name;
            runSetConfig.RunWithAnalyzer = dynamicRunset.RunAnalyzer;
            runSetConfig.RunModeParallel = dynamicRunset.RunInParallel;

            // Add runners
            foreach (AddRunner addRunner in dynamicRunset.AddRunners)
            {
                GingerRunner gingerRunner = new GingerRunner();
                gingerRunner.Name = addRunner.Name;

                if (!string.IsNullOrEmpty(addRunner.RunMode))
                {
                    gingerRunner.RunOption = (eRunOptions)Enum.Parse(typeof(eRunOptions), addRunner.RunMode, true);
                }

                if (!string.IsNullOrEmpty(addRunner.Environment))
                {
                    gingerRunner.UseSpecificEnvironment = true;
                    gingerRunner.SpecificEnvironmentName = addRunner.Environment;
                }

                //add Agents
                foreach (SetAgent setAgent in addRunner.SetAgents)
                {
                    ApplicationAgent appAgent = new ApplicationAgent();
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
                            businessFlowRun.BusinessFlowCustomizedRunVariables.Add(new VariableString() {DiffrentFromOrigin=true, VarValChanged = true, ParentType = inputVariabel.VariableParentType, ParentName = inputVariabel.VariableParentName, Name = inputVariabel.VariableName, InitialStringValue= inputVariabel.VariableValue, Value = inputVariabel.VariableValue });
                        }
                    }
                    gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);
                }
                runSetConfig.GingerRunners.Add(gingerRunner);
            }

            //Add mail Report handling
            foreach (AddRunsetOperation addOperation in dynamicRunset.AddRunsetOperations)
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
            }

            // Set config
            runsetExecutor.RunSetConfig = runSetConfig;
        }
        

        public static void Save(DynamicGingerExecution dynamicRunSet, string fileName)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DynamicGingerExecution));
            System.IO.FileStream file = System.IO.File.Create(fileName);
            writer.Serialize(file, dynamicRunSet);
            file.Close();
        }

        //public static DynamicRunSet Load(string fileName)
        //{
        //    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
        //    System.IO.StreamReader file = new System.IO.StreamReader(fileName);
        //    DynamicRunSet dynamicRunSet = (DynamicRunSet)reader.Deserialize(file);
        //    file.Close();
        //    return dynamicRunSet;
        //}

    }
}
