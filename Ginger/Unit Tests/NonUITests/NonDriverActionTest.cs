using System;
using System.Collections.Generic;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.XML;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class NonDriverActionTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("NonDriverActionTests");
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
    }
}
