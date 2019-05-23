using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Ginger.Run;
using Ginger.SolutionGeneral;
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

            dynamicRunSet.MailReport = new MailReport();
            dynamicRunSet.MailReport.MailFrom = "a@aaa.com";
            dynamicRunSet.MailReport.MailTo = "b@bbb.com";
            dynamicRunSet.MailReport.Subject = "Ginger Execution Report";
            dynamicRunSet.MailReport.ReportTemplateName = "Defualt";
            dynamicRunSet.MailReport.SmtpServer = "111.222.333";
            dynamicRunSet.MailReport.SmtpPort = "25";
            dynamicRunSet.MailReport.SmtpUser = "";
            dynamicRunSet.MailReport.SmtpPassword = "";

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
            if (dynamicRunSet.MailReport != null)
            {
                //TODO create mail report runset operation
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
