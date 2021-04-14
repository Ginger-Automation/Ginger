#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Ginger.Repository;
using Ginger.Run;
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
            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Non-Driver Action Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.CurrentBusinessFlow = mBF;
            mGR.BusinessFlows.Add(mBF);
            
            
            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
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
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "Web", Agent = a });
            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "Web", Platform = ePlatformType.Web, Description = "New Web application" });
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
            mGR.RunAction(action, false);

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
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(1, action.ReturnValues.Count);
            Assert.AreEqual(true, action.ReturnValues[0].Actual.Contains("ginger"));
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        public void ActScriptTestWithIndexZero()
        {
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(@"Script\ScriptWithGingerOutputIndexZero.vbs");
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(2, actScript.ReturnValues.Count);
            Assert.AreEqual("OK", actScript.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void ActScriptTestWithGingerOutput()
        {
            //with index > 0
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(@"Script\ScriptWithGingerOutput.vbs");
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(2, actScript.ReturnValues.Count);
            Assert.AreEqual("OK", actScript.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void ActScriptTestWithoutOutput()
        {
            //with index=-1
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(@"Script\ScriptWithoutOutput.vbs");
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(1, actScript.ReturnValues.Count);
            Assert.AreEqual("\n\nHello\nSNO=1\n\n", actScript.ReturnValues[0].Actual);
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
            mGR.RunAction(actAgentManipulation);

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
            mGR.SetCurrentActivityAgent();

            //Act
            mGR.RunAction(actAgentManipulation);

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
            mGR.RunAction(actAgentManipulation);

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
            mGR.SetCurrentActivityAgent();

            //Act
            mGR.RunAction(actAgentManipulation);

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
            mGR.RunAction(action);

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
            mGR.RunAction(action);

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
            mGR.RunAction(action);

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
            mGR.RunAction(action);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, action.Status, "Action Status");
            Assert.AreEqual("Please provide a valid file path", action.Error);
        }
    }
}
