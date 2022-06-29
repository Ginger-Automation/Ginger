using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using CommandLine;
using Ginger.Run;
using GingerCore.Environments;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amdocs.Ginger.Common;
using Ginger.Run.RunSetActions;
using Amdocs.Ginger.CoreNET.RunLib.DynamicExecutionLib;
using Ginger.ExecuterService.Contracts.V1.ExecutionConfiguration;
using System.Threading.Tasks;
using GingerCoreNET.ALMLib;
using GingerCore;

namespace WorkspaceHold
{
    [Level3]
    [TestClass]
    public class CLITest
    {
        // TODO: run one by one as it used same run exc
        static string mTempFolder;
        static string mSolutionFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTempFolder = TestResources.GetTempFolder(nameof(CLITest));
            mSolutionFolder = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "CLI");

            // Hook console message
            Reporter.logToConsoleEvent += ConsoleMessageEvent;

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }


        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.LockWS();
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            WorkSpace.RelWS();            
        }


        [TestMethod]
        public void OLDCLIConfigTest()
        {
            
            // Arrange
            PrepareForCLICreationAndExecution();
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIConfigFile();
            runSetAutoRunConfiguration.CreateContentFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "ConfigFile=" + runSetAutoRunConfiguration.ConfigFileFullPath });
            }).Wait();
            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
            
        }

        [TestMethod]
        public void OLDCLIConfigRegressionTest()
        {
            
            //Arrange                
            //Create config file         
            PrepareForCLICreationAndExecution();
            string txt = string.Format("Solution={0}", mSolutionFolder) + Environment.NewLine;
            txt += string.Format("Env={0}", "Default") + Environment.NewLine;
            txt += string.Format("RunSet={0}", "Default Run Set") + Environment.NewLine;
            txt += string.Format("RunAnalyzer={0}", "True") + Environment.NewLine;
            txt += string.Format("ShowAutoRunWindow={0}", "False") + Environment.NewLine;
            string configFile = TestResources.GetTempFile("runset1.ginger.config");
            System.IO.File.WriteAllText(configFile, txt);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "ConfigFile=" + configFile });
            }).Wait();
            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            
        }


        [TestMethod]
        public void CLIVersion()
        {

            //Arrange                            
            PrepareForCLICreationAndExecution();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "-t" });
            }).Wait();
            // Assert            
            // ????

        }

        [TestMethod]
        public void CLIBadParams()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act            
               

                Task.Run(async () =>
                {
                    await CLI.ExecuteArgs(new string[] { "--blabla" });
                }).Wait();

                // Assert                            
                Assert.AreEqual(eLogLevel.ERROR, mConsoleMessages[0].LogLevel, "message loglevel is ERROR");
                Assert.AreEqual("Please fix the arguments and try again", mConsoleMessages[0].MessageToConsole, "console message");
            }

        }
       
        [TestMethod]
        public void CLIHelp()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act         
                 Task.Run(async () =>
                {
                    await CLI.ExecuteArgs(new string[] { "help" });
                }).Wait();
               

                // Assert            
                Assert.AreEqual(1, mConsoleMessages.Count, "message count");
                Assert.IsTrue(mConsoleMessages[0].MessageToConsole.Contains("Ginger support"), "help message");
            }
        }
       
        [TestMethod]
        public void CLIGridWithoutParams()
        {
            lock (mConsoleMessages)
            {
                //Arrange                            
                mConsoleMessages.Clear();
                CLIProcessor CLI = new CLIProcessor();

                // Act            
               
                Task.Run(async () =>
                {
                    await CLI.ExecuteArgs(new string[] { "grid" });
                }).Wait();
                // Assert            
                Assert.AreEqual(1, mConsoleMessages.Count, "There is 1 line of help");
                Assert.AreEqual(eLogLevel.INFO, mConsoleMessages[0].LogLevel, "message loglevel is ERROR");
                Assert.AreEqual("Starting Ginger Grid at port: 15001", mConsoleMessages[0].MessageToConsole, "console message");
            }

        }

        class ConsoleMessage
        {
            public eLogLevel LogLevel;
            public string MessageToConsole;
        }

        static List<ConsoleMessage> mConsoleMessages = new List<ConsoleMessage>();
        public static void ConsoleMessageEvent(eLogLevel logLevel, string messageToConsole)
        {
            mConsoleMessages.Add(new ConsoleMessage(){LogLevel = logLevel, MessageToConsole = messageToConsole});
        }        

        [TestMethod]
        public void CLIDynamicXMLTest()
        {
            // Arrange
            PrepareForCLICreationAndExecution();
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIDynamicFile(CLIDynamicFile.eFileType.XML);
            runSetAutoRunConfiguration.CreateContentFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", runSetAutoRunConfiguration.ConfigFileFullPath });
            }).Wait();
            // Assert            
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "BF RunStatus=Passed");
        }


        /// <summary>
        /// Testing JSON config creation and execution
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_CreateAndExecute_Test()
        {
            // Arrange
            PrepareForCLICreationAndExecution(runsetName:"Calc_Test");
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;
            
            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIDynamicFile(CLIDynamicFile.eFileType.JSON);
            runSetAutoRunConfiguration.CreateContentFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", runSetAutoRunConfiguration.ConfigFileFullPath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test", "Validating correct Run set was executed" );
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");
        }

        /// <summary>
        /// Testing JSON existing Runset with customized Values execution
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_ExistingCustomized_IDsAndNames_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-CustomExistingRunset.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test", "Validating correct Run set was executed");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env1", "Validating correct customized Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct customized Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "IE", "Validating correct customized Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoMultiply?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForSum").FirstOrDefault().Value, "44", "Validating Customized Activity level String Variable");           
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (same instance of BF)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoDivide?").FirstOrDefault().Value, "Yes", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForDivide").FirstOrDefault().Value, "1", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).MailTo, "bbb@amdocs.com", "Validating customized report mail Address");
            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).Subject, "Test44", "Validating customized report mail Subject");
        }

        /// <summary>
        /// Testing JSON existing Runset with customized Values execution only using items Names
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_ExistingCustomized_OnlyNames_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-CustomExistingRunset_OnlyNames.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
          
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test", "Validating correct Run set was executed");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env1", "Validating correct customized Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct customized Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "IE", "Validating correct customized Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoMultiply?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForSum").FirstOrDefault().Value, "44", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (same instance of BF)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoDivide?").FirstOrDefault().Value, "Yes", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForDivide").FirstOrDefault().Value, "1", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).MailTo, "bbb@amdocs.com", "Validating customized report mail Address");
            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).Subject, "Test44", "Validating customized report mail Subject");
        }

        /// <summary>
        /// Testing JSON existing Runset with customized Values execution while only items IDs is provided
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_ExistingCustomized_OnlyIDs_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-CustomExistingRunset_OnlyIDs.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test", "Validating correct Run set was executed");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env1", "Validating correct customized Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct customized Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "IE", "Validating correct customized Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoMultiply?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForSum").FirstOrDefault().Value, "44", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (same instance of BF)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoDivide?").FirstOrDefault().Value, "Yes", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForDivide").FirstOrDefault().Value, "1", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).MailTo, "bbb@amdocs.com", "Validating customized report mail Address");
            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).Subject, "Test44", "Validating customized report mail Subject");
        }

        /// <summary>
        /// Testing JSON non existing Runset 
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_NotExist_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-NotExisting.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test_Dynamic", "Validating correct Run set was executed");

            //Runner 
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner Dynamic", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env1", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "IE", "Validating correct Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoMultiply?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForSum").FirstOrDefault().Value, "44", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (same instance of BF)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoDivide?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForDivide").FirstOrDefault().Value, "1", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Stopped, "Validating BF execution Stopped");

            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).MailTo, "menik@amdocs.com", "Validating report mail Address");
            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).Subject, "AAA", "Validating report mail Subject");
        }

        /// <summary>
        /// Testing JSON non existing Runset with only items names
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_NotExist_OnlyNames_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-NotExisting_OnlyNames.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Calc_Test_Dynamic", "Validating correct Run set was executed");

            //Runner 
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner Dynamic", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env1", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "IE", "Validating correct Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoMultiply?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForSum").FirstOrDefault().Value, "44", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (same instance of BF)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Calculator_Test", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "DoDivide?").FirstOrDefault().Value, "No", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "SecondNum_ForDivide").FirstOrDefault().Value, "1", "Validating Customized Activity level String Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Stopped, "Validating BF execution Stopped");

            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).MailTo, "menik@amdocs.com", "Validating report mail Address");
            Assert.AreEqual(((RunSetActionHTMLReportSendEmail)(WorkSpace.Instance.RunsetExecutor.RunSetConfig.RunSetActions[0])).Subject, "AAA", "Validating report mail Subject");
        }

        /// <summary>
        /// Testing JSON - Existing Runset & Existing Runner with Virtual Business Flow exist in solution
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_VirtualBF_RunsetAndRunnerExist_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-VirtualBF_ExistingRunsetRunner.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Default Run Set", "Validating correct Run set was executed");

            //Runner
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner 1", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Default", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Default", "Validating correct Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "Chrome", "Validating correct Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow 1", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[0].ActivityName, "Activity 1", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[1].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (Virtual Business Flow existing in the Solution)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_D", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Validating BF execution Stopped");

            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivityName, "Test Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].Guid.ToString(), "bf285a86-6384-4b1f-acfb-e0c1b60eec12", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].Status, eRunStatus.Passed, "Validating BF execution Stopped");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivitiesGroupID, "Test Activity", "Validating Customized BF level Selection List Variable");
        }

        /// <summary>
        /// Testing JSON - Existing Runset & Existing Runner with Virtual Business Flow exist in solution
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_VirtualBF_RunsetAndRunnerExist_OnlyNames_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-VirtualBF_ExistingRunsetRunner_OnlyName.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Default Run Set", "Validating correct Run set was executed");

            //Runner
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner 1", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Default", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Default", "Validating correct Runner Environment");

            //Agent Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "Chrome", "Validating correct Runner Agent");

            //BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow 1", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[0].ActivityName, "Activity 1", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[1].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            //BF 2 Validation (Virtual Business Flow existing in the Solution)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_D", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Validating BF execution Stopped");

            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivityName, "Test Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].Guid.ToString(), "bf285a86-6384-4b1f-acfb-e0c1b60eec12", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].Status, eRunStatus.Passed, "Validating BF execution Stopped");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivitiesGroupID, "Test Activity", "Validating Customized BF level Selection List Variable");
        }

        /// <summary>
        /// Testing JSON - Virtual Runset & Runner with Virtual Business Flow (flow exist in solution)
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_VirtualBF_VirtualRunsetAndRunner_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-VirtualBF_VirtualRunsetAndRunner.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Virtual Run Set", "Validating correct Run set was executed");

            //Runner 
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Virtual Runner", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Default", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Guid.ToString(), "23ac9f62-72ed-446a-a6fc-01be97cb2b40", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Default", "Validating correct Runner Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Guid.ToString(), "23ac9f62-72ed-446a-a6fc-01be97cb2b40", "Validating correct Runner Environment");

            // Runner 1 BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow 1", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Guid.ToString(), "2db992cd-c5f7-43a6-beb6-cd4de10fece7", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[0].ActivityName, "Activity 1", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[1].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            // Runner 1 BF 2 Validation (Virtual Business Flow existing in the Solution)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_D", "Validating correct Business Flow was executed");
            Assert.AreNotEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Guid.ToString(), "86ad108b-bddc-4cce-ba17-c8bb50fe2c66", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Validating BF execution Stopped");
        }

        /// <summary>
        /// Testing JSON - Virtual Runset & Runner with Virtual Business Flow (flow exist in solution)
        /// </summary>
        [TestMethod]
        public void CLIDynamicJSON_VirtualBF_VirtualRunsetAndRunner_OnlyNames_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-VirtualBF_VirtualRunsetAndRunner_OnlyName.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Virtual Run Set", "Validating correct Run set was executed");

            //Runner 
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Virtual Runner", "Validating correct Runner Name");

            //Envs Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Default", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Guid.ToString(), "23ac9f62-72ed-446a-a6fc-01be97cb2b40", "Validating correct Run set Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Default", "Validating correct Runner Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Guid.ToString(), "23ac9f62-72ed-446a-a6fc-01be97cb2b40", "Validating correct Runner Environment");

            // Runner 1 BF 1 Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow 1", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Guid.ToString(), "2db992cd-c5f7-43a6-beb6-cd4de10fece7", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[0].ActivityName, "Activity 1", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Activities[1].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Validating BF execution Passed");

            // Runner 1 BF 2 Validation (Virtual Business Flow existing in the Solution)
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_D", "Validating correct Business Flow was executed");
            Assert.AreNotEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Guid.ToString(), "86ad108b-bddc-4cce-ba17-c8bb50fe2c66", "Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Activities[0].ActivityName, "User Detail Activity", "Validating Customized BF level Selection List Variable");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Validating BF execution Stopped");
        }

        /// <summary>
        /// Testing JSON existing Runset of outputs pass without customization
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_OutputsPass_Existing_NotCustomized_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-OutputsPass_Existing_NotCustomized.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Runset_DataPass", "Validating correct Run set was executed");

            //Flow A Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow_A", "Flow_A- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_ID").FirstOrDefault().Value, "1212", "A_ID Variable- Validating Run set configured value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Color").FirstOrDefault().Value, "Black", "A_Color Variable- Validating Run set configured value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Type").FirstOrDefault().Value, "222", "A_Type Variable- Validating Run set configured value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Name").FirstOrDefault().Value, "Meni", "A_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_A- Validating BF execution Passed");

            //Flow B Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_B", "Flow_B- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_ID").FirstOrDefault().Value, "1212", "B_ID Variable- Validating Run set configured output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Type").FirstOrDefault().Value, "222", "B_Type Variable- Validating Run set configured output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Name").FirstOrDefault().Value, "Meni", "B_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Flow_B- Validating BF execution Passed");

            //Flow C Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Flow_C", "Flow_C- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_ID").FirstOrDefault().Value, "1212", "C_ID Variable- Validating Run set configured output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Color").FirstOrDefault().Value, "Black", "C_Color Variable- Validating Run set configured value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Type").FirstOrDefault().Value, "222", "C_Type Variable- Validating Run set configured output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Name").FirstOrDefault().Value, "Meni", "C_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Passed, "Flow_C- Validating BF execution Passed");
        }

        /// <summary>
        /// Testing JSON existing Runset of outputs pass with regular input values customization
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_OutputsPass_Existing_RegularInputCustomized_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-OutputsPass_Existing_RegularInputCustomized.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Runset_DataPass", "Validating correct Run set was executed");

            //Flow A Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow_A", "Flow_A- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_ID").FirstOrDefault().Value, "676767", "A_ID Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Color").FirstOrDefault().Value, "Blue", "A_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Type").FirstOrDefault().Value, "444", "A_Type Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Name").FirstOrDefault().Value, "Meni", "A_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_A- Validating BF execution Passed");

            //Flow B Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_B", "Flow_B- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_ID").FirstOrDefault().Value, "676767", "B_ID Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Type").FirstOrDefault().Value, "444", "B_Type Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Name").FirstOrDefault().Value, "Meni", "B_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Flow_B- Validating BF execution Passed");

            //Flow C Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].Name, "Flow_C", "Flow_C- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_ID").FirstOrDefault().Value, "676767", "C_ID Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Color").FirstOrDefault().Value, "Blue", "C_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Type").FirstOrDefault().Value, "444", "C_Type Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Name").FirstOrDefault().Value, "Meni", "C_Name Variable- Validating default value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[2].RunStatus, eRunStatus.Passed, "Flow_C- Validating BF execution Passed");
        }

        /// <summary>
        /// Testing JSON existing Runset of data mapping with all supported types
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_OutputsPass_Existing_AllCustomizedTypes_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-OutputsPass_Existing_AllCustomizedTypes.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "DataMappingTest", "Validating correct Run set was executed");

            //Flow A Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow_A", "Flow_A- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_ID").FirstOrDefault().Value, "666666666", "A_ID Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Name").FirstOrDefault().Value, "Trump", "A_Name Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Color").FirstOrDefault().Value, "Black", "A_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Type").FirstOrDefault().Value, "444", "A_Type Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_A- Validating BF execution Passed");

            //Flow B Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_B", "Flow_B- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_ID").FirstOrDefault().Value, "666666666", "B_ID Variable- Validating customized output data pass worked as expected");
            //Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Name").FirstOrDefault().Value, "OK", "B_Name Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Color").FirstOrDefault().Value, "Black", "B_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Type").FirstOrDefault().Value, "444", "B_Type Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Flow_B- Validating BF execution Passed");

            //Flow C Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].Name, "Flow_C", "Flow_C- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_ID").FirstOrDefault().Value, "666666666", "C_ID Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Name").FirstOrDefault().Value, "Trump", "C_Name Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Color").FirstOrDefault().Value, "Black", "C_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Type").FirstOrDefault().Value, "555", "C_Type Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_C- Validating BF execution Passed");
        }

        /// <summary>
        /// Testing JSON virtual Runset of data mapping with all supported types
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_OutputsPass_Virtual_AllCustomizedTypes_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-OutputsPass_Virtual_AllCustomizedTypes.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();
            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "VirtualTest", "Validating correct Run set was executed");

            //Env Validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name, "Env2", "Validating correct Run set Environment");

            //Runner 1
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "VirtualRunner1", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ProjEnvironment.Name, "Env2", "Validating correct Runner Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].ApplicationAgents[0].AgentName, "Chrome", "Validating correct Runner Agent");
            //Flow A Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].Name, "Flow_A", "Flow_A- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_ID").FirstOrDefault().Value, "666666666", "A_ID Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Name").FirstOrDefault().Value, "Trump", "A_Name Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Color").FirstOrDefault().Value, "Black", "A_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "A_Type").FirstOrDefault().Value, "444", "A_Type Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_A- Validating BF execution Passed");
            //Flow B Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].Name, "Flow_B", "Flow_B- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_ID").FirstOrDefault().Value, "666666666", "B_ID Variable- Validating customized output data pass worked as expected");
            //Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Name").FirstOrDefault().Value, "OK", "B_Name Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Color").FirstOrDefault().Value, "Black", "B_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "B_Type").FirstOrDefault().Value, "444", "B_Type Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[1].RunStatus, eRunStatus.Passed, "Flow_B- Validating BF execution Passed");

            //Runner 2
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Name, "VirtualRunner2", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].ProjEnvironment.Name, "Env2", "Validating correct Runner Environment");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].ApplicationAgents[0].AgentName, "IE", "Validating correct Runner Agent");
            //Flow C Input validation
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].Name, "Flow_C", "Flow_C- Validating correct Business Flow was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_ID").FirstOrDefault().Value, "666666666", "C_ID Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Name").FirstOrDefault().Value, "Trump", "C_Name Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Color").FirstOrDefault().Value, "Black", "C_Color Variable- Validating customized value was used");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].GetBFandActivitiesVariabeles(false).Where(x => x.Name == "C_Type").FirstOrDefault().Value, "555", "C_Type Variable- Validating customized output data pass worked as expected");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Executor.BusinessFlows[0].RunStatus, eRunStatus.Passed, "Flow_C- Validating BF execution Passed");
        }

        /// <summary>
        /// Testing JSON existing Runset with multi Runners and StopOnFailure turn on
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_Existing_StopRunnersOnFailure_StopOn_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLIDynamicJSON_Existing_StopRunnersOnFailure_StopOn.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "MultiRunnersStopOnFailureTest", "Validating correct Run set was executed");

            //Runner 1
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner 1", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Status.ToString(), "Passed", "Validating correct Runner Status");

            //Runner 2
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Name, "Runner 2", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Status.ToString(), "Failed", "Validating correct Runner Status");

            //Runner 3
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Name, "Runner 3", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Status.ToString(), "Blocked", "Validating correct Runner Status");
        }

        /// <summary>
        /// Testing JSON existing Runset with multi Runners and StopOnFailure turn off
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_Existing_StopRunnersOnFailure_StopOff_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLIDynamicJSON_Existing_StopRunnersOnFailure_StopOff.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "MultiRunnersStopOnFailureTest", "Validating correct Run set was executed");

            //Runner 1
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Runner 1", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Status.ToString(), "Passed", "Validating correct Runner Status");

            //Runner 2
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Name, "Runner 2", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Status.ToString(), "Failed", "Validating correct Runner Status");

            //Runner 3
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Name, "Runner 3", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Status.ToString(), "Passed", "Validating correct Runner Status");
        }

        /// <summary>
        /// Testing JSON virtual Runset with multi Runners and StopOnFailure turn on
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_Virtual_StopRunnersOnFailure_StopOn_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLIDynamicJSON_Virtual_StopRunnersOnFailure_StopOn.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "Virtual MultiRunnersStopOnFailureTest", "Validating correct Run set was executed");

            //Runner 1
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Name, "Virtual Runner 1", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].Status.ToString(), "Passed", "Validating correct Runner Status");

            //Runner 2
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Name, "Virtual Runner 2", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[1].Status.ToString(), "Failed", "Validating correct Runner Status");

            //Runner 3
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Name, "Virtual Runner 3", "Validating correct Runner Name");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[2].Status.ToString(), "Blocked", "Validating correct Runner Status");
        }

        /// <summary>
        /// Testing JSON virtual Runset with ALM Configurations
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_ALM_Details_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLIDynamicJSON_Existing_ALM_Details.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            WorkSpace.Instance.DoNotResetWorkspaceArgsOnClose = true;
            CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });

            ALMConfig octaneSolConfig= WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm).FirstOrDefault();
            ALMUserConfig octaneUsrConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.Where(x => x.AlmType == ALMIntegrationEnums.eALMType.Octane).FirstOrDefault();
            ALMConfig qcSolConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == ALMIntegrationEnums.eALMType.QC).FirstOrDefault();
            ALMUserConfig qcUsrConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.Where(x => x.AlmType == ALMIntegrationEnums.eALMType.QC).FirstOrDefault();
            ALMConfig jiraSolConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == ALMIntegrationEnums.eALMType.Jira).FirstOrDefault();
            ALMUserConfig jiraUsrConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.Where(x => x.AlmType == ALMIntegrationEnums.eALMType.Jira).FirstOrDefault();

            // Assert      
            //Validate Octane ALM details on Solution and User profile ALM details which been used for actual ALM connection
            Assert.AreEqual(octaneSolConfig.ALMServerURL, @"https://Octane.com:47168", "Validating correct ALMServerURL");
            Assert.AreEqual(octaneUsrConfig.ALMServerURL, @"https://Octane.com:47168", "Validating correct ALMServerURL");
            Assert.AreEqual(octaneSolConfig.ALMUserName, "AAA_j06lwznjzw8ny", "Validating correct ALMUserName");
            Assert.AreEqual(octaneUsrConfig.ALMUserName, "AAA_j06lwznjzw8ny", "Validating correct ALMUserName");
            Assert.AreEqual(octaneSolConfig.ALMPassword, "@205372322126A", "Validating correct ALMPassword");
            Assert.AreEqual(octaneUsrConfig.ALMPassword, "@205372322126A", "Validating correct ALMPassword");
            Assert.AreEqual(octaneSolConfig.ALMDomain, "domainCLI", "Validating correct ALMDomain");
            Assert.AreEqual(octaneSolConfig.ALMProjectName, "projectCLI", "Validating correct ALMProjectName");
            Assert.AreEqual(octaneSolConfig.ALMConfigPackageFolderPath, "C:\\Temp\\Test Octane\\Octane Settings", "Validating correct ALMConfigPackageFolderPath");

            //Validate QC ALM details on Solution and User profile ALM details which been used for actual ALM connection
            Assert.AreEqual(qcSolConfig.ALMServerURL, @"http:/QC.CLI", "Validating correct ALMServerURL");
            Assert.AreEqual(qcUsrConfig.ALMServerURL, @"http:/QC.CLI", "Validating correct ALMServerURL");
            Assert.AreEqual(qcSolConfig.ALMUserName, "CLIUser", "Validating correct ALMUserName");
            Assert.AreEqual(qcUsrConfig.ALMUserName, "CLIUser", "Validating correct ALMUserName");
            Assert.AreEqual(qcSolConfig.ALMPassword, "CLIUPass", "Validating correct ALMPassword");
            Assert.AreEqual(qcUsrConfig.ALMPassword, "CLIUPass", "Validating correct ALMPassword");
            Assert.AreEqual(qcSolConfig.ALMDomain, "domainCLI", "Validating correct ALMDomain");
            Assert.AreEqual(qcSolConfig.ALMProjectName, "projectCLI", "Validating correct ALMProjectName");
            Assert.AreEqual(qcSolConfig.ALMProjectKey, "projectKeyCLI", "Validating correct ALMProjectKey");
            Assert.AreEqual(qcSolConfig.UseRest, true, "Validating correct UseRest");

            //Validate Jira ALM details on Solution and User profile ALM details which been used for actual ALM connection
            Assert.AreEqual(jiraSolConfig.ALMServerURL, @"http:/Jira.CLI", "Validating correct ALMServerURL");
            Assert.AreEqual(jiraUsrConfig.ALMServerURL, @"http:/Jira.CLI", "Validating correct ALMServerURL");
            Assert.AreEqual(jiraSolConfig.ALMUserName, "CLIUser", "Validating correct ALMUserName");
            Assert.AreEqual(jiraUsrConfig.ALMUserName, "CLIUser", "Validating correct ALMUserName");
            Assert.AreEqual(jiraSolConfig.ALMPassword, "JiraPassword", "Validating correct ALMPassword");
            Assert.AreEqual(jiraUsrConfig.ALMPassword, "JiraPassword", "Validating correct ALMPassword");
            Assert.AreEqual(jiraSolConfig.JiraTestingALM, ALMIntegrationEnums.eTestingALMType.Xray, "Validating correct JiraTestingALM");
        }


        /// <summary>
        /// Testing JSON Runset with setup of ExecutionID
        /// </summary>   
        [TestMethod]
        public void CLIDynamicJSON_CheckExecutionIDSet_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-CheckExecutionIDSet.Ginger.AutoRunConfigs.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();


            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, "DataMappingTest", "Validating correct Run set was executed");
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.RunSetConfig.ExecutionID.ToString(), "9ab6158b-05b4-9b9c-99f9-bde0f2299f9e", "Validating ExecutionID which was configured in JSON actually was used in run time");
        }

        /// <summary>
        /// Testing JSON Runset with setup of ExecutionID
        /// </summary>   
        [TestMethod]
        public void CLI_Sealights_JSON_Test()
        {
            // Arrange
            string jsonConfigFilePath = CreateTempJSONConfigFile(Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-Sealights_JSON.json"), mSolutionFolder);

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "dynamic", "-f", jsonConfigFilePath });
            }).Wait();

            // Assert        
            Assert.AreEqual(WorkSpace.Instance.Solution.SealightsConfiguration.SealightsBuildSessionID, "1648233090826", "Validating correct Sealights SessionID");
            Assert.AreEqual(WorkSpace.Instance.Solution.SealightsConfiguration.SealightsAgentToken, "112233", "Validating correct Sealights Token");
        }

        [TestMethod]
        public void OLDCLIDynamicRegressionTest()
        {
         
                //Arrange
                //Create config file       
                string fileName = Path.Combine(TestResources.GetTestResourcesFolder("CLI"), "CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
                string dynamicXML = System.IO.File.ReadAllText(fileName);
                dynamicXML = dynamicXML.Replace("SOLUTION_PATH", mSolutionFolder);
                string configFile = TestResources.GetTempFile("CLI-Default Run Set.Ginger.AutoRunConfigs.xml");
                System.IO.File.WriteAllText(configFile, dynamicXML);

                // Act            
                CLIProcessor CLI = new CLIProcessor();
                
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "Dynamic=" + configFile });
            }).Wait();
            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void OLDCLIConfigFileTest()
        {
            // Arrange
            PrepareForCLICreationAndExecution();
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIConfigFile();
            runSetAutoRunConfiguration.CreateContentFile();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
           
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "ConfigFile=" + runSetAutoRunConfiguration.ConfigFileFullPath });
            }).Wait();
            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIScriptRegressionTest()
        {
            //Arrange
            // PrepareForCLIExecution();
            // Create config file
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + mSolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += nameof(GingerScriptGlobals.CreateExecutionSummaryJSON) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json
            txt += "i" + Environment.NewLine;  // script rc
            File.WriteAllText(scriptFile, txt);

            // Act
            CLIProcessor CLI = new CLIProcessor();
         

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(new string[] { "script", "-f", scriptFile });
            }).Wait();

            // Assert
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIArgsTest()
        {
            // Arrange
            PrepareForCLICreationAndExecution();
            // Create config file
            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration runSetAutoRunConfiguration = new RunSetAutoRunConfiguration(WorkSpace.Instance.Solution, WorkSpace.Instance.RunsetExecutor, cLIHelper);
            runSetAutoRunConfiguration.ConfigFileFolderPath = mTempFolder;
            runSetAutoRunConfiguration.SelectedCLI = new CLIArgs();

            // Act            
            CLIProcessor CLI = new CLIProcessor();
            string[] args = CommandLineToStringArray(runSetAutoRunConfiguration.CLIContent).ToArray();
      
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(args);
            }).Wait();

            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
        }

        [TestMethod]
        public void CLIArgsWithDoNotAnalyzeTest()
        {
            //Arrange
            PrepareForCLICreationAndExecution();
            // Create args
            string[] args = { "run", "--solution", mSolutionFolder, "--env", "Default", "--runset", "Default Run Set", "--do-not-analyze"};
            
            // Act            
            CLIProcessor CLI = new CLIProcessor();
            

            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(args);
            }).Wait();

            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");

            // TODO: test analyze was run !!!!
        }


        private void PrepareForCLICreationAndExecution(string runsetName= "Default Run Set", string envName = "Default")
        {
            WorkSpace.Instance.OpenSolution(mSolutionFolder, EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            runsetExecutor.RunsetExecutionEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == envName select x).SingleOrDefault();
            runsetExecutor.RunSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == runsetName select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            WorkSpace.Instance.RunsetExecutor.InitRunners();
        }

        [TestMethod]
        public void NewCLIArgsRegressionTest()
        {            
            //Arrange
            PrepareForCLICreationAndExecution();

            // Create args
            List<string> args = new List<string>();

            args.Add("run");

            args.Add("--solution");
            args.Add(mSolutionFolder);

            args.Add("--env");
            args.Add("Default");

            args.Add("--runset");
            args.Add("Default Run Set");

            args.Add("--do-not-analyze");

            args.Add("--showui");            

            // Act            
            CLIProcessor CLI = new CLIProcessor();
         
            Task.Run(async () =>
            {
                await CLI.ExecuteArgs(args.ToArray());
            }).Wait();
            // Assert            
            Assert.AreEqual(eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Executor.BusinessFlows[0].RunStatus, "BF RunStatus=Passed");
            
        }

        [TestMethod]
        public void NewCreateCLIArgs()
        {
            //Arrange
            RunOptions options = new RunOptions() { Environment = "env1", DoNotAnalyze = true, Runset = "rs1" };


            // Act            
            var arguments = CommandLine.Parser.Default.FormatCommandLine<RunOptions>(options);

            // Assert            
            Assert.IsTrue(arguments.StartsWith("run"), "arguments Starts With run");
            Assert.IsTrue(arguments.Contains("--env env1"), "arguments Contains --env env1");
            Assert.IsTrue(arguments.Contains("--runset rs1"), "arguments Contains --runset rs1");
            Assert.IsTrue(arguments.Contains("--do-not-analyze"), "arguments Contains --do-not-analyze");                   
        }

        [TestMethod]
        public void ParseStringToArgs()
        {
            //Arrange
            string s = @"run -s c:\123 -e env1";

            // Act            
            var arguments = CommandLineToStringArray(s);

            // Assert            
            Assert.IsTrue(arguments.First() == "run");
            Assert.IsTrue(arguments.Contains("-s"), "arguments Contains -s");
            Assert.IsTrue(arguments.Contains(@"c:\123"), @"arguments Contains c:\123");
            Assert.IsTrue(arguments.Contains("-e"), "arguments Contains -e");
            Assert.IsTrue(arguments.Contains("env1"), "arguments Contains env1");
        }

        // Parse a command line with multiple switches to string list - used for test only!
        public static IEnumerable<string> CommandLineToStringArray(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                yield break;

            var sb = new StringBuilder();
            bool inQuote = false;
            foreach (char c in commandLine)
            {
                if (c == '"' && !inQuote)
                {
                    inQuote = true;
                    continue;
                }

                if (c != '"' && !(char.IsWhiteSpace(c) && !inQuote))
                {
                    sb.Append(c);
                    continue;
                }

                if (sb.Length > 0)
                {
                    var result = sb.ToString();
                    sb.Clear();
                    inQuote = false;
                    yield return result;
                }
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }


        private string CreateTempJSONConfigFile(string resourceJSONFilePath, string solutionPath)
        {
            GingerExecConfig config = DynamicExecutionManager.DeserializeDynamicExecutionFromJSON(System.IO.File.ReadAllText(resourceJSONFilePath));
            config.SolutionLocalPath = solutionPath;            
            string tempJSONConfigFilePath = TestResources.GetTempFile(System.IO.Path.GetFileName(resourceJSONFilePath));
            System.IO.File.WriteAllText(tempJSONConfigFilePath, DynamicExecutionManager.SerializeDynamicExecutionToJSON(config));

            return tempJSONConfigFilePath;
        }
    }
}
