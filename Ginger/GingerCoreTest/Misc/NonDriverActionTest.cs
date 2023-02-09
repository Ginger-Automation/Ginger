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
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.XML;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class NonDriverActionTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;
        string Separator = Path.DirectorySeparatorChar.ToString();

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {            
            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Non-Driver Action Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.BusinessFlows.Add(mBF);
            
            
            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            
            if (((Solution)mGR.Executor.CurrentSolution).SolutionOperations == null)
            {
                ((Solution)mGR.Executor.CurrentSolution).SolutionOperations = new SolutionOperations((Solution)mGR.Executor.CurrentSolution);
            }
            if (WorkSpace.Instance.Solution == null)
            {
                WorkSpace.Instance.Solution = (Solution)mGR.Executor.CurrentSolution;
            }
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            
        }

        public static void AddApplicationAgent()
        {
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "Web" });

            Agent a = new Agent();
            AgentOperations agentOperations = new AgentOperations(a);
            a.AgentOperations = agentOperations;

            a.DriverType = Agent.eDriverType.SeleniumChrome;

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "Web", Agent = a });
            mGR.Executor.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.Executor.SolutionApplications.Add(new ApplicationPlatform() { AppName = "Web", Platform = ePlatformType.Web, Description = "New Web application" });
        }


        [Ignore] // not stable
        [TestMethod]
        [Timeout(60000)]
        public void ActXMLProcessingTest()
        {
            //Arrange
            ActXMLProcessing action = new ActXMLProcessing();

            var templateFile = TestResources.GetTestResourcesFile(@"XML\TemplateVU.xml");
            action.GetOrCreateInputParam(ActXMLProcessing.Fields.TemplateFileName, templateFile);
            action.TemplateFileName.ValueForDriver = templateFile;

            var targetFile = TestResources.GetTestResourcesFile(@"XML\TargetFile.xml");
            action.GetOrCreateInputParam(ActXMLProcessing.Fields.TargetFileName, targetFile);
            action.TargetFileName.ValueForDriver = targetFile;

            VariableString stringVar = new VariableString();
            stringVar.Name = "env";
            stringVar.Value = "xyz";

            mBF.CurrentActivity.AddVariable(stringVar);

            ObservableList<ActInputValue> paramList = new ObservableList<ActInputValue>();
            paramList.Add(new ActInputValue() { Param = "PAR_ENV", Value = "{Var Name=env}" });
            paramList.Add(new ActInputValue() { Param = "PAR_USER", Value = "abc" });
            paramList.Add(new ActInputValue() { Param = "PAR_PASS", Value = "abc123" });
            paramList.Add(new ActInputValue() { Param = "PAR_BUCKET", Value = "pqrst" });
            paramList.Add(new ActInputValue() { Param = "PAR_QUERY", Value = "test1234" });

            action.DynamicElements = paramList;
            action.Active = true;
            action.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.Executor.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(7, action.ReturnValues.Count);
            Assert.AreEqual("xyz", action.ReturnValues[0].Actual);
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActFileNonXmlProcessingTest()
        {
            //Arrange
            ActXMLProcessing action = new ActXMLProcessing();

            var templateFile = TestResources.GetTestResourcesFile(@"XML\BATCH_TEMPLATE.XML");
            action.GetOrCreateInputParam(ActXMLProcessing.Fields.TemplateFileName, templateFile);
            action.TemplateFileName.ValueForDriver = templateFile;

            var targetFile = TestResources.GetTestResourcesFile(@"XML\BATCH_FILE.BAT");
            action.GetOrCreateInputParam(ActXMLProcessing.Fields.TargetFileName, targetFile);
            action.TargetFileName.ValueForDriver = targetFile;

            ObservableList<ActInputValue> paramList = new ObservableList<ActInputValue>();
            paramList.Add(new ActInputValue() { Param = "VAR_DIR_NAME", Value = "ginger" });
            paramList.Add(new ActInputValue() { Param = "VAR_ENV_COMMAND", Value = "abc" });
            paramList.Add(new ActInputValue() { Param = "VAR_CLASS_FILE_NAME", Value = "abc123" });
            paramList.Add(new ActInputValue() { Param = "VAR_IDENTIFIER_NAME", Value = "pqrst" });

            action.DynamicElements = paramList;
            action.Active = true;
            action.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.Executor.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(1, action.ReturnValues.Count);
            Assert.AreEqual(true, action.ReturnValues[0].Actual.Contains("ginger"));
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        

        [TestMethod]
        public void CloseAgentNullTest()
        {
            //Arrange
            ActAgentManipulation actAgentManipulation = new ActAgentManipulation();
            actAgentManipulation.GetOrCreateInputParam(ActAgentManipulation.Fields.AgentManipulationActionType);
            actAgentManipulation.Active = true;
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            //Act
            mGR.Executor.RunAction(actAgentManipulation);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, actAgentManipulation.Status, "Action Status");
        }

        [TestMethod]
        public void CloseAgentNotRunningTest()
        {
            //Arrange
            ActAgentManipulation actAgentManipulation = new ActAgentManipulation();
            actAgentManipulation.GetOrCreateInputParam(ActAgentManipulation.Fields.AgentManipulationActionType);
            actAgentManipulation.Active = true;
            AddApplicationAgent();
            mGR.Executor.SetCurrentActivityAgent();

            //Act
            mGR.Executor.RunAction(actAgentManipulation);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actAgentManipulation.Status, "Action Status");
            Assert.IsTrue(actAgentManipulation.ExInfo.Contains("Agent is not running"));
        }

        [TestMethod]
        public void RestartAgentNullTest()
        {
            //Arrange
            ActAgentManipulation actAgentManipulation = new ActAgentManipulation();
            actAgentManipulation.GetOrCreateInputParam(ActAgentManipulation.Fields.AgentManipulationActionType, "RestartAgent");
            actAgentManipulation.Active = true;
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            //Act
            mGR.Executor.RunAction(actAgentManipulation);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, actAgentManipulation.Status, "Action Status");
        }

        [Ignore]  // keep a browser open FIXME
        [TestMethod]
        public void RestartAgentNotRunningTest()
        {
            //Arrange
            ActAgentManipulation actAgentManipulation = new ActAgentManipulation();
            actAgentManipulation.GetOrCreateInputParam(ActAgentManipulation.Fields.AgentManipulationActionType, "RestartAgent");
            actAgentManipulation.Active = true;
            AddApplicationAgent();
            mGR.Executor.SetCurrentActivityAgent();

            //Act
            mGR.Executor.RunAction(actAgentManipulation);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actAgentManipulation.Status, "Action Status");
            Assert.IsTrue(actAgentManipulation.ExInfo.Contains("Agent is not running"));
        }

        [TestMethod]
        public void XMLRequestFromFileCheckedTestForPath()
        {
            //Arrange
            ActXMLTagValidation action = new ActXMLTagValidation();
            action.DocumentType = GingerCore.Actions.XML.ActXMLTagValidation.eDocumentType.XML;
            action.ReqisFromFile = true;
            action.InputFile.Value = TestResources.GetTestResourcesFile($"XML{Separator}book.xml");
            ActInputValue AIV = new ActInputValue();
            AIV.FilePath = "/catalog/book[2]";
            AIV.Value = "publisher";
            AIV.Param= "/catalog/book[2]";
            action.DynamicElements.Add(AIV);
            action.Active = true;
            action.AddNewReturnParams = true;

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual("amdocs", action.ReturnValues[1].Actual);
        }

        [TestMethod]
        public void XMLRequestFromFileCheckedTestForContent()
        {
            //Arrange
            ActXMLTagValidation action = new ActXMLTagValidation();
            action.DocumentType = GingerCore.Actions.XML.ActXMLTagValidation.eDocumentType.XML;
            action.ReqisFromFile = false;
            string xmlFileContent = File.ReadAllText(TestResources.GetTestResourcesFile($"XML{Separator}book.xml"));
            action.InputFile.Value = xmlFileContent;
            action.Active = true;

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
        }

        [TestMethod]
        public void XMLRequestFromFileUnCheckedTestForPath()
        {
            //Arrange
            ActXMLTagValidation action = new ActXMLTagValidation();
            action.DocumentType = GingerCore.Actions.XML.ActXMLTagValidation.eDocumentType.XML;
            action.ReqisFromFile = false;
            action.InputFile.Value = TestResources.GetTestResourcesFile($"XML{Separator}book.xml");
            ActInputValue AIV = new ActInputValue();
            AIV.FilePath = "/catalog/book[2]";
            AIV.Value = "publisher";
            AIV.Param = "/catalog/book[2]";
            action.DynamicElements.Add(AIV);
            action.Active = true;
            action.AddNewReturnParams = true;

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, action.Status, "Action Status");
            Assert.AreEqual("Please provide a valid file content", action.Error);
        }

        [TestMethod]
        public void XMLRequestFromFileUnCheckedTestForContent()
        {
            //Arrange
            ActXMLTagValidation action = new ActXMLTagValidation();
            action.DocumentType = GingerCore.Actions.XML.ActXMLTagValidation.eDocumentType.XML;
            action.ReqisFromFile = true;
            string xmlFileContent = File.ReadAllText(TestResources.GetTestResourcesFile($"XML{Separator}book.xml"));
            action.InputFile.Value = xmlFileContent;
            action.Active = true;

            //Act
            mGR.Executor.RunAction(action);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, action.Status, "Action Status");
            Assert.AreEqual("Please provide a valid file path", action.Error);
        }
    }
}
