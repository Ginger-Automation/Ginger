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
using System.Linq;
using static Ginger.Run.GingerRunner;

namespace Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib
{
    public class DynamicRunSetManager
    {

        public static string CreateDynamicRunSetXML(Solution solution, RunsetExecutor runsetExecutor, CLIHelper cliHelper)
        {
            runsetExecutor.RunSetConfig.UpdateRunnersBusinessFlowRunsList();

            DynamicRunSet dynamicRunSet = new DynamicRunSet();

            if (cliHelper.DownloadUpgradeSolutionFromSourceControl == true)
            {
                dynamicRunSet.SolutionSourceControlType = solution.SourceControl.GetSourceControlType.ToString();
                dynamicRunSet.SolutionSourceControlUrl = solution.SourceControl.SourceControlURL.ToString();
                dynamicRunSet.SolutionSourceControlUser = solution.SourceControl.SourceControlUser.ToString();
                dynamicRunSet.SolutionSourceControlPassword = solution.SourceControl.SourceControlPass.ToString();
                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT && solution.SourceControl.SourceControlProxyAddress.ToLower().ToString() == "true")
                {
                    dynamicRunSet.SolutionSourceControlProxyServer = solution.SourceControl.SourceControlProxyAddress.ToString();
                    dynamicRunSet.SolutionSourceControlProxyPort = solution.SourceControl.SourceControlProxyPort.ToString();
                }
            }
            dynamicRunSet.SolutionPath = solution.Folder;

            dynamicRunSet.Name = "Dynamic_" + runsetExecutor.RunSetConfig.Name;
            dynamicRunSet.Environemnt = runsetExecutor.RunsetExecutionEnvironment.Name;
            dynamicRunSet.RunAnalyzer = cliHelper.RunAnalyzer;
            dynamicRunSet.ShowAutoRunWindow = cliHelper.ShowAutoRunWindow;

            foreach (GingerRunner gingerRunner in runsetExecutor.RunSetConfig.GingerRunners)
            {
                Runner addRunner = new Runner();
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
                    addRunner.Agents.Add(new Agent() { AgentName = applicationAgent.AgentName, ApplicationName = applicationAgent.AppName });
                }

                foreach (BusinessFlowRun businessFlowRun in gingerRunner.BusinessFlowsRunList)
                {
                    BusinessFlow addBusinessFlow = new BusinessFlow();
                    addBusinessFlow.Name = businessFlowRun.BusinessFlowName;
                    foreach (VariableBase variableBase in businessFlowRun.BusinessFlowCustomizedRunVariables)
                    {
                        InputVariable setBusinessFlowVariable = new InputVariable();
                        if (variableBase.ParentType != "Business Flow")
                        {
                            setBusinessFlowVariable.VariableParentType = variableBase.ParentType;
                            setBusinessFlowVariable.VariableParentName = variableBase.ParentName;
                        }
                        setBusinessFlowVariable.VariableName = variableBase.Name;
                        setBusinessFlowVariable.VariableValue = variableBase.Value;

                        addBusinessFlow.InputVariables.Add(setBusinessFlowVariable);
                    }
                    addRunner.BusinessFlows.Add(addBusinessFlow);
                }
                dynamicRunSet.Runners.Add(addRunner);
            }

            foreach (RunSetActionBase runSetAction in runsetExecutor.RunSetConfig.RunSetActions)
            {
                if (runSetAction is RunSetActionHTMLReportSendEmail)
                {
                    RunSetActionHTMLReportSendEmail runsetMailReport = (RunSetActionHTMLReportSendEmail)runSetAction;
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
                        dynamicMailReport.SmtpServer = runsetMailReport.Email.SMTPMailHost;
                        dynamicMailReport.SmtpPort = runsetMailReport.Email.SMTPPort.ToString();
                        dynamicMailReport.SmtpEnableSSL = runsetMailReport.Email.EnableSSL.ToString();
                        if (runsetMailReport.Email.ConfigureCredential)
                        {
                            dynamicMailReport.SmtpUser = runsetMailReport.Email.SMTPUser;
                            dynamicMailReport.SmtpPassword = runsetMailReport.Email.SMTPPass;
                        }
                    }

                    if(runsetMailReport.EmailAttachments.Where(x=>x.IsReport==true).FirstOrDefault() != null)
                    {
                        dynamicMailReport.IncludeAttachmentReport = true;
                    }
                    else
                    {
                        dynamicMailReport.IncludeAttachmentReport = false;
                    }

                    dynamicRunSet.RunsetOperations.Add(dynamicMailReport);
                }
            }
            string content = ConvertDynamicRunsetToXML(dynamicRunSet);
            return content;
        }

        private static string ConvertDynamicRunsetToXML(DynamicRunSet dynamicRunSet)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.StringWriter stringWriter = new System.IO.StringWriter();
            writer.Serialize(stringWriter, dynamicRunSet);
            stringWriter.Close();
            return stringWriter.GetStringBuilder().ToString();                
        }

        public static DynamicRunSet LoadDynamicRunsetFromXML(string content)
        {
            System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
            System.IO.StringReader stringReader = new System.IO.StringReader(content);
            DynamicRunSet dynamicRunSet = (DynamicRunSet)reader.Deserialize(stringReader);
            stringReader.Close();
            return dynamicRunSet;
        }

        public static void LoadRealRunSetFromDynamic(RunsetExecutor runsetExecutor, DynamicRunSet dynamicRunSet)
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.Name = dynamicRunSet.Name;
            runSetConfig.RunWithAnalyzer = dynamicRunSet.RunAnalyzer;
            runSetConfig.RunModeParallel = dynamicRunSet.RunInParallel;

            // Add runners
            foreach (Runner dynamicRunner in dynamicRunSet.Runners)
            {
                GingerRunner gingerRunner = new GingerRunner();
                gingerRunner.Name = dynamicRunner.Name;

                if (!string.IsNullOrEmpty(dynamicRunner.RunMode))
                {
                    gingerRunner.RunOption = (eRunOptions)Enum.Parse(typeof(eRunOptions), dynamicRunner.RunMode, true);
                }

                if (!string.IsNullOrEmpty(dynamicRunner.Environment))
                {
                    gingerRunner.UseSpecificEnvironment = true;
                    gingerRunner.SpecificEnvironmentName = dynamicRunner.Environment;
                }

                //add Agents
                foreach (Agent dynamicAgent in dynamicRunner.Agents)
                {
                    ApplicationAgent appAgent = new ApplicationAgent();
                    appAgent.AppName = dynamicAgent.ApplicationName;
                    appAgent.AgentName = dynamicAgent.AgentName;
                    gingerRunner.ApplicationAgents.Add(appAgent);
                }

                // Add BFs
                foreach (BusinessFlow dynamicBusinessFlow in dynamicRunner.BusinessFlows)
                {
                    BusinessFlowRun businessFlowRun = new BusinessFlowRun();
                    businessFlowRun.BusinessFlowName = dynamicBusinessFlow.Name;
                    businessFlowRun.BusinessFlowIsActive = true;

                    // set BF Variables
                    foreach (InputVariable dynamicVariabel in dynamicBusinessFlow.InputVariables)
                    {
                        businessFlowRun.BusinessFlowCustomizedRunVariables.Add(new VariableString() { ParentType = dynamicVariabel.VariableParentType, ParentName = dynamicVariabel.VariableParentName, Name = dynamicVariabel.VariableName, Value = dynamicVariabel.VariableValue });
                    }
                    gingerRunner.BusinessFlowsRunList.Add(businessFlowRun);
                }
                runSetConfig.GingerRunners.Add(gingerRunner);
            }

            //Add mail Report handling
            foreach (RunsetOperationBase dynamicOperation in dynamicRunSet.RunsetOperations)
            {
                if (dynamicOperation is MailReport)
                {
                    MailReport dynamicMailOperation = (MailReport)dynamicOperation;
                    RunSetActionHTMLReportSendEmail mailOperation = new RunSetActionHTMLReportSendEmail();
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
                        mailOperation.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.SMTP;
                        mailOperation.Email.SMTPMailHost = dynamicMailOperation.SmtpServer;
                        mailOperation.Email.SMTPPort = int.Parse(dynamicMailOperation.SmtpPort);
                        mailOperation.Email.EnableSSL = bool.Parse(dynamicMailOperation.SmtpEnableSSL);
                        if (string.IsNullOrEmpty(dynamicMailOperation.SmtpUser) == false)
                        {
                            mailOperation.Email.ConfigureCredential = true;
                            mailOperation.Email.SMTPUser = dynamicMailOperation.SmtpUser;
                            mailOperation.Email.SMTPPass = dynamicMailOperation.SmtpPassword;
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
        

        public static void Save(DynamicRunSet dynamicRunSet, string fileName)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DynamicRunSet));
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
