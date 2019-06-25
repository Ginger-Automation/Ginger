using System;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class BusinessFlowTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("BusinessFlowFunalityTest");
            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();
                      
            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            //Add defalut BF
            mBF = new BusinessFlow();
            mBF.Name = "Test BF";
            mBF.Active = true;
            mGR.BusinessFlows.Add(mBF);

            mBF.Activities = new ObservableList<Activity>();
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }
        [TestMethod]
        [Timeout(60000)]
        public void AddNewBusinessFlowTest()
        {
            BusinessFlow mBF1 = new BusinessFlow("BF1");

            //Add BF Level Variable
            VariableBase variable = new VariableString() { Name = "StrVar",Value="test123" };
            mBF1.AddVariable(variable);
            mGR.BusinessFlows.Add(mBF1);

            //Assert
            Assert.AreEqual(2, mGR.BusinessFlows.Count);
            Assert.AreEqual("BF1", mBF1.Name);
            Assert.AreEqual(1, mBF1.Activities.Count);
            Assert.AreEqual("Activity 1", mBF1.Activities[0].ActivityName);
            Assert.AreEqual(1, mBF1.Variables.Count);
            Assert.AreEqual("test123", mBF1.Variables[0].Value);
            Assert.AreEqual("StrVar", mBF1.Variables[0].Name);
          
        }

        [TestMethod]
        [Timeout(60000)]
        public void DeleteBusinessFlowTest()
        {
            var bfCount = mGR.TotalBusinessflow;
            BusinessFlow newBF = new BusinessFlow("Test BF");
            mGR.BusinessFlows.Add(newBF);

            Assert.AreEqual(bfCount+1, mGR.TotalBusinessflow);

            //update the bf count
            bfCount = mGR.TotalBusinessflow;

            // delete BF
            mGR.BusinessFlows.RemoveAt(mGR.TotalBusinessflow - 1);

            mGR.RunRunner();
            Assert.AreEqual(bfCount-1, mGR.TotalBusinessflow);
            
        }

        [TestMethod]
        [Timeout(60000)]
        public void EditBusinessFlowTest()
        {
            mBF.Name = "BF Test";
            mBF.Description = "Test Business Flow";
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "WebApp" });
            mBF.CurrentActivity.ActivityName = "Test Activity";

            mGR.RunRunner();
            //Assert
            Assert.AreEqual("Test Business Flow", mBF.Description);
        }

        [TestMethod]
        [Timeout(60000)]
        public void AddNewActivityTest()
        {
            mBF.AddActivity(new Activity() { ActivityName = "New Activity", Active = true });

            //Assert
            Assert.AreEqual(2, mBF.Activities.Count);
            Assert.AreEqual("New Activity", mBF.Activities[1].ActivityName);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MapAgentToActivityTest()
        {
            Agent agent = new Agent() { DriverType = Agent.eDriverType.SeleniumChrome,Name="chrome" };
            mBF.Activities[0].CurrentAgent = agent;

            //Assert
            Assert.AreEqual("chrome", mBF.Activities[0].CurrentAgent.Name.ToString());
            Assert.AreEqual("Web", mBF.Activities[0].CurrentAgent.Platform.ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        public void AddNewActionTest()
        {
            ActBrowserElement actBrowserElement = new ActBrowserElement() { Description="Go Url Action" ,ControlAction = ActBrowserElement.eControlAction.GotoURL,Value= "https://ginger.amdocs.com/" };
            mBF.AddAct(actBrowserElement);

            //Assert
            Assert.AreNotEqual(0, mBF.CurrentActivity.Acts.Count);
            Assert.AreEqual("Go Url Action", mBF.CurrentActivity.Acts[0].Description);
            Assert.AreEqual("Browser Action", mBF.CurrentActivity.Acts[0].ActionDescription);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CreateCopyBFTest()
        {
            //arrange
            BusinessFlow bf1 = new BusinessFlow("Test bf1");
            bf1.Description = "Test BF1";

            //act
            BusinessFlow bf2 =(BusinessFlow) bf1.CreateCopy();
            
            //Assert
            Assert.AreEqual(bf1.Description, bf2.Description);
            Assert.AreEqual(bf1.Name, bf2.Name);
        }
 
    }
}
