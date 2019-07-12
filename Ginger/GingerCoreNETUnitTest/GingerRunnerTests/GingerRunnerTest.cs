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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNETUnitTest.RunTestslib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class GingerRunnerTest
    {

        static BusinessFlow mBF;
        static GingerRunner mGR;
        static SolutionRepository SR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("Unit Tests");
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test Fire Fox";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;            
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent a = new Agent();
            //a.DriverType = Agent.eDriverType.SeleniumFireFox;//have known firefox issues with selenium 3
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);
            // p2.Agent = a;

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web, Description = "New application" });
            mGR.BusinessFlows.Add(mBF);

            WorkspaceHelper.InitWS("GingerRunnerTest");            

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" +  Path.DirectorySeparatorChar + "BasicSimple"));
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(path);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            WorkspaceHelper.ReleaseWorkspace();
        }


        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void SCM_Login()
        {

            ////Arrange

            //ResetBusinessFlow();

            //// mGR.SetSpeed(1);

            //Activity a1 = new Activity();
            //a1.Active = true;
            //a1.TargetApplication = "SCM";
            //mBF.Activities.Add(a1);

            //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            //a1.Acts.Add(act1);

            //ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            //a1.Acts.Add(act2);

            //ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            //a1.Acts.Add(act3);

            //ActSubmit act4 = new ActSubmit() { LocateBy = eLocateBy.ByValue, LocateValue = "Log in", Active = true };
            //a1.Acts.Add(act4);

            //VariableString v1 = (VariableString)mBF.GetVariable("v1");
            //v1.Value = "123";

            ////Act            
            //mGR.RunRunner();
            //// mGR.CurrentBusinessFlow = mBF;
            //// mGR.RunActivity(a1);

            ////Assert
            //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            //Assert.AreEqual(a1.Status, eRunStatus.Passed);
            //Assert.AreEqual(act1.Status, eRunStatus.Passed);
            //Assert.AreEqual(act2.Status, eRunStatus.Passed);
            //Assert.AreEqual(act3.Status, eRunStatus.Passed);
            //Assert.AreEqual(act4.Status, eRunStatus.Passed);

            //Assert.AreEqual(v1.Value, "123");
        }


        // Test the time to enter data into text box
        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void SpeedTest()
        {
            ////Arrange
            //ResetBusinessFlow();

            //Activity a0 = new Activity();
            //a0.Active = true;

            //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            //a0.Acts.Add(act1);

            //mBF.Activities.Add(a0);

            //Activity a1 = new Activity();
            //a1.Active = true;
            //mBF.Activities.Add(a1);

            //for (int i = 1; i < 10; i++)
            //{
            //    ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            //    a1.Acts.Add(act2);

            //    ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            //    a1.Acts.Add(act3);
            //}

            //mGR.RunRunner();

            ////Assert
            //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed, "mBF.RunStatus");
            //Assert.AreEqual(mBF.Activities.Count(), (from x in mBF.Activities where x.Status == eRunStatus.Passed select x).Count(), "All activities should Passed");
            //Assert.IsTrue(a1.Elapsed < 10000, "a1.Elapsed Time less than 10000 ms");
        }


        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }

        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void TestVariableResetIssue()
        {
            ////This was a tricky bug not repro every time.
            //// the issue was when seeting Biz flow for Agent a reset vars happened.


            ////Arrange
            //ResetBusinessFlow();

            //Activity a1 = new Activity();
            //a1.Active = true;
            //mBF.Activities.Add(a1);

            //ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            //a1.Acts.Add(act1);

            //// Not happening with dummy
            ////ActDummy act1 = new ActDummy();
            ////a1.Acts.Add(act1);

            //VariableString v1 = (VariableString)mBF.GetVariable("v1");
            //v1.Value = "123";

            ////Act            
            //mGR.RunRunner();

            ////Assert
            //Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            //Assert.AreEqual(a1.Status, eRunStatus.Passed);

            //Assert.AreEqual(v1.Value, "123");  // <<< the importnat part as with this defect it turned to "1" - initial val
        }

        
        [TestMethod]
        [Timeout(60000)]
        public void TestRunsetConfigBFVariables()
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

            mGR.BusinessFlows.Add(BF1);

            //Adding Same Business Flow Again to GingerRunner
            BusinessFlow bfToAdd = (BusinessFlow)BF1.CreateCopy(false);
            bfToAdd.ContainingFolder = BF1.ContainingFolder;
            bfToAdd.Active = true;
            bfToAdd.Reset();
            bfToAdd.InstanceGuid = Guid.NewGuid();
            mGR.BusinessFlows.Add(bfToAdd);

            WorkSpace.Instance.SolutionRepository = SR;

            //Act
            //Changing initial value of 2nd BF from BusinessFlow Config 
            mGR.BusinessFlows[2].Variables[0].Value = "bbb";
            mGR.BusinessFlows[2].Variables[0].DiffrentFromOrigin = true;

            mGR.RunRunner();

            //Assert
            Assert.AreEqual(BF1.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(activity.Status, eRunStatus.Passed);

            Assert.AreEqual(bfToAdd.RunStatus, eRunStatus.Passed);

            Assert.AreEqual(mGR.BusinessFlows[1].Variables[0].Value, "aaa");
            Assert.AreEqual(mGR.BusinessFlows[2].Variables[0].Value, "bbb");

        }

    }
}
