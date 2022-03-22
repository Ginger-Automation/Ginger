#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.Run.RunSetActions;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class GingerRunnerTest
    {
        BusinessFlow mBF;
        GingerRunner mGR;
        SolutionRepository SR;
        Solution solution;
        ProjEnvironment environment;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {
            mBF = new BusinessFlow();
            mBF.Name = "BF Test Fire Fox";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.AddActivity(activity);

            ActDummy action1 = new ActDummy();
            ActDummy action2 = new ActDummy();

            mBF.Activities[0].Acts.Add(action1);
            mBF.Activities[0].Acts.Add(action2);

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Name = "Test Runner";
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = mBF.Activities[0];

            environment = new ProjEnvironment();
            environment.Name = "Default";
            mBF.Environment = environment.Name;
            mGR.ProjEnvironment = environment;

            Agent a = new Agent();
            //a.DriverType = Agent.eDriverType.SeleniumFireFox;//have known firefox issues with selenium 3
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);
            // p2.Agent = a;

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.Executor.BusinessFlows.Add(mBF);
            mGR.SpecificEnvironmentName = environment.Name;
            mGR.UseSpecificEnvironment = false;

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" +Path.DirectorySeparatorChar + "BasicSimple"));
            string solutionFile = System.IO.Path.Combine(path, @"Ginger.Solution.xml");
            solution = SolutionOperations.LoadSolution(solutionFile);
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(path);
            WorkSpace.Instance.Solution = solution;
            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder;
            WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            WorkSpace.Instance.SolutionRepository = SR;

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            try
            {
                //Delete 'Report' folder which is crated after Dynamic Runset Execution
                System.IO.DirectoryInfo di = new DirectoryInfo(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "Reports" + Path.DirectorySeparatorChar + "Reports"));
                di.Delete(true);
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, e.Message);
            }

        }


        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void SCM_Login()
        //{

        //    ////Arrange

        //    //ResetBusinessFlow();

        //    //// mGR.SetSpeed(1);

        //    //Activity a1 = new Activity();
        //    //a1.Active = true;
        //    //a1.TargetApplication = "SCM";
        //    //mBF.Activities.Add(a1);

        //    //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    //a1.Acts.Add(act1);

        //    //ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    //a1.Acts.Add(act2);

        //    //ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    //a1.Acts.Add(act3);

        //    //ActSubmit act4 = new ActSubmit() { LocateBy = eLocateBy.ByValue, LocateValue = "Log in", Active = true };
        //    //a1.Acts.Add(act4);

        //    //VariableString v1 = (VariableString)mBF.GetVariable("v1");
        //    //v1.Value = "123";

        //    ////Act            
        //    //mGR.RunRunner();
        //    //// mGR.CurrentBusinessFlow = mBF;
        //    //// mGR.RunActivity(a1);

        //    ////Assert
        //    //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
        //    //Assert.AreEqual(a1.Status, eRunStatus.Passed);
        //    //Assert.AreEqual(act1.Status, eRunStatus.Passed);
        //    //Assert.AreEqual(act2.Status, eRunStatus.Passed);
        //    //Assert.AreEqual(act3.Status, eRunStatus.Passed);
        //    //Assert.AreEqual(act4.Status, eRunStatus.Passed);

        //    //Assert.AreEqual(v1.Value, "123");
        //}


        //// Test the time to enter data into text box
        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void SpeedTest()
        //{
        //    ////Arrange
        //    //ResetBusinessFlow();

        //    //Activity a0 = new Activity();
        //    //a0.Active = true;

        //    //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    //a0.Acts.Add(act1);

        //    //mBF.Activities.Add(a0);

        //    //Activity a1 = new Activity();
        //    //a1.Active = true;
        //    //mBF.Activities.Add(a1);

        //    //for (int i = 1; i < 10; i++)
        //    //{
        //    //    ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    //    a1.Acts.Add(act2);

        //    //    ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    //    a1.Acts.Add(act3);
        //    //}

        //    //mGR.RunRunner();

        //    ////Assert
        //    //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed, "mBF.RunStatus");
        //    //Assert.AreEqual(mBF.Activities.Count(), (from x in mBF.Activities where x.Status == eRunStatus.Passed select x).Count(), "All activities should Passed");
        //    //Assert.IsTrue(a1.Elapsed < 10000, "a1.Elapsed Time less than 10000 ms");
        //}


        //private void ResetBusinessFlow()
        //{
        //    mBF.Activities.Clear();
        //    mBF.RunStatus = eRunStatus.Pending;
        //}

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void TestVariableResetIssue()
        //{
        //    ////This was a tricky bug not repro every time.
        //    //// the issue was when seeting Biz flow for Agent a reset vars happened.


        //    ////Arrange
        //    //ResetBusinessFlow();

        //    //Activity a1 = new Activity();
        //    //a1.Active = true;
        //    //mBF.Activities.Add(a1);

        //    //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    //a1.Acts.Add(act1);

        //    //// Not happening with dummy
        //    ////ActDummy act1 = new ActDummy();
        //    ////a1.Acts.Add(act1);

        //    //VariableString v1 = (VariableString)mBF.GetVariable("v1");
        //    //v1.Value = "123";

        //    ////Act            
        //    //mGR.RunRunner();

        //    ////Assert
        //    //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
        //    //Assert.AreEqual(a1.Status, eRunStatus.Passed);

        //    //Assert.AreEqual(v1.Value, "123");  // <<< the importnat part as with this defect it turned to "1" - initial val
        //}


        [TestMethod]
        [Timeout(60000)]
        public void RunsetConfigBFVariablesTest()
        {
            //Arrange
            ObservableList<BusinessFlow> bfList = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF1 = bfList[0];

            ObservableList<Activity> activityList = BF1.Activities;

            Activity activity = activityList[0];

            ActDummy act1 = new ActDummy() { Value = "", Active = true };
            activity.Acts.Add(act1);

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "aaa" };
            BF1.AddVariable(v1);

            BF1.Active = true;

            mGR.Executor.BusinessFlows.Add(BF1);

            //Adding Same Business Flow Again to GingerRunner
            BusinessFlow bfToAdd = (BusinessFlow)BF1.CreateCopy(false);
            bfToAdd.ContainingFolder = BF1.ContainingFolder;
            bfToAdd.Active = true;
            bfToAdd.Reset();
            bfToAdd.InstanceGuid = Guid.NewGuid();
            mGR.Executor.BusinessFlows.Add(bfToAdd);

            WorkSpace.Instance.SolutionRepository = SR;

            //Act
            //Changing initial value of 2nd BF from BusinessFlow Config 
            mGR.Executor.BusinessFlows[2].Variables[2].Value = "bbb";
            mGR.Executor.BusinessFlows[2].Variables[2].DiffrentFromOrigin = true;

            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(BF1.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(activity.Status, eRunStatus.Passed);

            Assert.AreEqual(bfToAdd.RunStatus, eRunStatus.Passed);

            Assert.AreEqual(mGR.Executor.BusinessFlows[1].Variables[2].Value, "aaa");
            Assert.AreEqual(mGR.Executor.BusinessFlows[2].Variables[2].Value, "bbb");

        }


        [TestMethod]
        [Timeout(60000)]
        public void DyanamicRunsetXMLCreationTest()
        {
            //Arrange
            RunSetConfig runSetConfigurations = CreteRunsetWithOperations();

            RunsetExecutor GMR = new RunsetExecutor();
            GMR.RunsetExecutionEnvironment = environment;
            GMR.RunSetConfig = runSetConfigurations;

            CLIHelper cLIHelper = new CLIHelper();
            cLIHelper.RunAnalyzer = true;
            cLIHelper.ShowAutoRunWindow = false;
            cLIHelper.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration autoRunConfiguration = new RunSetAutoRunConfiguration(solution, GMR, cLIHelper);
            CLIDynamicFile mCLIDynamicXML = new CLIDynamicFile(CLIDynamicFile.eFileType.XML);
            autoRunConfiguration.SelectedCLI = mCLIDynamicXML;

            //Act
            //Creating XML file content from above configurations
            string file = autoRunConfiguration.SelectedCLI.CreateConfigurationsContent(solution, GMR, cLIHelper);

            //Assert
            //validate the 'AddRunsetOperation' tag
            XElement nodes = XElement.Parse(file);

            List<XElement> AddRunsetOPerationsNodes = nodes.Elements("AddRunsetOperation").ToList();

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(file);
            XmlNodeList runsetoperations = xDoc.GetElementsByTagName("AddRunsetOperation");

            //Assert
            Assert.AreEqual(runsetoperations.Count , 3);
            Assert.AreEqual(runsetoperations[0].FirstChild.Name, "MailFrom");
            Assert.AreEqual(runsetoperations[0].LastChild.Name, "IncludeAttachmentReport") ;
            Assert.AreEqual(runsetoperations[1].FirstChild.Name, "HTMLReportFolderName");
            Assert.AreEqual(runsetoperations[1].LastChild.Name, "isHTMLReportPermanentFolderNameUsed");
            Assert.AreEqual(runsetoperations[2].HasChildNodes,false);

        }

        [TestMethod]
        [Timeout(60000)]
        public void DynamicRunetExecutionTest()
        {
            //Arrange
            ObservableList<BusinessFlow> bfList = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF1 = bfList[0];

            ObservableList<Activity> activityList = BF1.Activities;

            Activity activity = activityList[0];

            BF1.Active = true;

            GingerRunner mGRForRunset = new GingerRunner();
            mGRForRunset.Executor = new GingerExecutionEngine(mGRForRunset);

            mGRForRunset.Name = "Test Runner";

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            ((GingerExecutionEngine)mGRForRunset.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGRForRunset.Executor).SolutionAgents.Add(a);

            mGRForRunset.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGRForRunset.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGRForRunset.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });

            mGRForRunset.Executor.BusinessFlows.Add(BF1);
            WorkSpace.Instance.SolutionRepository = SR;

            mGRForRunset.SpecificEnvironmentName = environment.Name;
            mGRForRunset.UseSpecificEnvironment = false;

            RunSetConfig runSetConfig1 = new RunSetConfig();
            mGRForRunset.Executor.IsUpdateBusinessFlowRunList = true;
            runSetConfig1.GingerRunners.Add(mGRForRunset);

            runSetConfig1.UpdateRunnersBusinessFlowRunsList();
            runSetConfig1.mRunModeParallel = false;

            RunSetActionHTMLReport produceHTML2 = CreateProduceHTMlOperation();
            runSetConfig1.RunSetActions.Add(produceHTML2);

            RunsetExecutor GMR1 = new RunsetExecutor();
            GMR1.RunsetExecutionEnvironment = environment;
            GMR1.RunSetConfig = runSetConfig1;
            WorkSpace.Instance.RunsetExecutor = GMR1;
            CLIHelper cLIHelper1 = new CLIHelper();
            cLIHelper1.RunAnalyzer = false;
            cLIHelper1.ShowAutoRunWindow = false;
            cLIHelper1.DownloadUpgradeSolutionFromSourceControl = false;

            RunSetAutoRunConfiguration autoRunConfiguration1 = new RunSetAutoRunConfiguration(solution, GMR1, cLIHelper1);
            CLIDynamicFile mCLIDynamicXML1= new CLIDynamicFile(CLIDynamicFile.eFileType.XML);
            autoRunConfiguration1.SelectedCLI = mCLIDynamicXML1;
            String xmlFile =autoRunConfiguration1.SelectedCLI.CreateConfigurationsContent(solution, GMR1, cLIHelper1);

            autoRunConfiguration1.CreateContentFile();

            CLIProcessor cLIProcessor = new CLIProcessor();
            string[] args = new string[]{ autoRunConfiguration1.SelectedCLI.Verb, "--" + CLIOptionClassHelper.FILENAME, autoRunConfiguration1.ConfigFileFullPath};
           
            //Act
            cLIProcessor.ExecuteArgs(args).Wait();

            //Assert
            string path = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "Reports");
            Assert.IsTrue(Directory.Exists(path));
        }

        public RunSetActionHTMLReport CreateProduceHTMlOperation()
        {
            RunSetActionHTMLReport produceHTML1 = new RunSetActionHTMLReport();
            produceHTML1.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
            produceHTML1.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            produceHTML1.isHTMLReportFolderNameUsed = true;
            produceHTML1.HTMLReportFolderName = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "BasicSimple" + Path.DirectorySeparatorChar + "Reports"));
            produceHTML1.isHTMLReportPermanentFolderNameUsed = false;
            produceHTML1.Active = true;
            return produceHTML1;
        }

        public RunSetConfig CreteRunsetWithOperations()
        {
            RunSetConfig runSetConfig = new RunSetConfig();
            runSetConfig.GingerRunners.Add(mGR);
            runSetConfig.mRunModeParallel = false;

            //added HTMl send mail action
            RunSetActionHTMLReportSendEmail sendMail = new RunSetActionHTMLReportSendEmail();
            sendMail.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
            sendMail.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            sendMail.MailFrom = "Test@gmail.com";
            sendMail.MailTo = "Test@gamil.com";
            sendMail.Email.EmailMethod = GingerCore.GeneralLib.Email.eEmailMethod.OUTLOOK;
            sendMail.Active = true;

            //added Produce Html action

            RunSetActionHTMLReport produceHTML = CreateProduceHTMlOperation();

            //added JSON action
            RunSetActionJSONSummary jsonReportOperation = new RunSetActionJSONSummary();
            jsonReportOperation.Name = "Json Report";
            jsonReportOperation.RunAt = RunSetActionBase.eRunAt.ExecutionEnd;
            jsonReportOperation.Condition = RunSetActionBase.eRunSetActionCondition.AlwaysRun;
            jsonReportOperation.Active = true;

            runSetConfig.RunSetActions.Add(sendMail);
            runSetConfig.RunSetActions.Add(produceHTML);
            runSetConfig.RunSetActions.Add(jsonReportOperation);

            return runSetConfig;
        }

        [TestMethod]
        [Timeout(60000)]
        public void RunDisabledActivityTest()
        {
            //Arrange
            Activity activity = GetActivityFromRepository();

            //Act
            mGR.Executor.RunActivity(activity);

            //Assert
            Assert.AreEqual(activity.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped);
        }

        [TestMethod]
        [Timeout(60000)]
        public void RunDisabledActionTest()
        {
            //Arrange
            Activity activity = GetActivityFromRepository();

            ObservableList<IAct> actionList = activity.Acts;
            Act action = (Act)actionList[0];
            action.Active = false;

            //Act

            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(action.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped);
            Assert.AreEqual("Action is not active.", action.ExInfo);

        }

        [TestMethod]
        [Timeout(60000)]
        public void RunActionWithFlowControlAndDontMoveNext()
        {
            //Arrange
            Activity activity = mBF.Activities[0];

            ActDummy act1 = new ActDummy();
            act1.Active = true;
            activity.Acts.Add(act1);

            ObservableList<IAct> actionList = activity.Acts;
            Act action = (Act)actionList[0];
            action.Active = true;
            FlowControl flowControl = new FlowControl();
            flowControl.Active = true;

            flowControl.Operator = eFCOperator.ActionPassed;
            flowControl.FlowControlAction = eFlowControlAction.GoToAction;
            flowControl.Value= act1.Guid + flowControl.GUID_NAME_SEPERATOR;

            action.FlowControls.Add(flowControl);

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(action.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem, mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts[2]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void RunActionWithoutFlowControlAndMoveNext()
        {
            //Arrange
            
            Activity activity = GetActivityFromRepository(); 
            ActDummy act1 = new ActDummy();
            act1.Active = true;
            activity.Acts.Add(act1);

            ObservableList<IAct> actionList = activity.Acts;
            Act action = (Act)actionList[0];
            action.Active = true;

            mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(action.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
            Assert.AreEqual(mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts.CurrentItem, mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts[1]);
        }
      
        public Activity GetActivityFromRepository()
        {
            Context context = new Context();

            ObservableList<BusinessFlow> bfList = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF1 = bfList[0];

            ObservableList<Activity> activityList = BF1.Activities;
            Activity activity = activityList[0];
            activity.Active = false;

            context.BusinessFlow = BF1;
            context.Activity = activity;
            mGR.Executor.CurrentBusinessFlow = BF1;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = activity;
            mGR.Executor.Context = context;

            return activity;
        }
    }
}
