#region License
/*
Copyright © 2014-2018 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger;
using GingerTestHelper;

namespace UnitTests.NonUITests
{
    // Test all statues after execution: Action, Action Return Value (Expected vs Actual), Activity, BF.

    [TestClass]
    [Level1]
    public class StatusResultTest 
    {

        BusinessFlow mBF;
        GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("Unit Tests");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Status Result Test";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.Platforms = new ObservableList<Platform>();
            mBF.Platforms.Add(p);
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.GiveUserFeedback = true;
           
            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;
            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "SCM", Platform = ePlatformType.Web});
            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });
          
            mGR.BusinessFlows.Add(mBF);
        }

        [TestMethod]
        public void Simple_Act()
        {
            //Arrange
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };

            //Act
            mGR.CalculateActionFinalStatus(act1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(act1.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Passed");
        }

        [TestMethod]
        public void Simple_Act_With_Error()
        {
            //Arrange
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            act1.Error = "Cannot go to URL";

            //Act
            mGR.CalculateActionFinalStatus(act1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(act1.Status, eRunStatus.Failed, "act1.Status=eRunStatus.Failed");
        }

        [TestMethod]
        public void Simple_Act_ReturnValue_As_Expected()
        {
            //Arrange
            string ParamName = "p1";
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            act1.AddNewReturnParams = true;
            act1.AddOrUpdateReturnParamActual(ParamName, "ABC");
            act1.AddOrUpdateReturnParamExpected(ParamName, "ABC");

            //Act
            mGR.CalculateActionFinalStatus(act1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(act1.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Passed");
            ActReturnValue RV = act1.GetReturnValue(ParamName);
            Assert.AreEqual(RV.Status, ActReturnValue.eStatus.Passed, "RV.Status, ActReturnValue.eStatus.Passed");
        }

        [TestMethod]
        public void Simple_Act_ReturnValue_Not_As_Expected()
        {
            //Arrange
            string ParamName = "p1";
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            act1.AddNewReturnParams = true;
            act1.AddOrUpdateReturnParamActual(ParamName, "378");
            act1.AddOrUpdateReturnParamExpected(ParamName, "37");  // failed due to regex

            //Act
            mGR.CalculateActionFinalStatus(act1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(act1.Status, eRunStatus.Failed, "act1.Status=eRunStatus.Failed");
            ActReturnValue RV = act1.GetReturnValue(ParamName);
            Assert.AreEqual(RV.Status, ActReturnValue.eStatus.Failed, "RV.Status, ActReturnValue.eStatus.Failed");
        }

        [TestMethod]
        public void Activity_With_Action_Pass()
        {
            //Arrange
            //string ParamName = "p1";
            //ActGotoURL act1 = new ActGotoURL() { LocateBy = Act.eLocatorType.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            //act1.AddOrUpdateReturnParam(ParamName, "378");
            //act1.AddOrUpdateExpectedParam(ParamName, "37");  // failed due to regex

            ////Act
            //mGR.CalculateActionFinalStatus(act1);

            ////Assert
            //// since there is no failure we assume pass
            //Assert.AreEqual(act1.Status, eRunStatus.Fail, "act1.Status=eRunStatus.Fail");
            //ActReturnValue RV = act1.GetReturnValue(ParamName);
            //Assert.AreEqual(RV.Status, ActReturnValue.eStatus.Fail, "RV.Status, ActReturnValue.eStatus.Fail");

            Activity a1 = new Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            a1.Acts.Add(act1);

            ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act2);

            ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act3);


            mGR.RunRunner();


            Assert.AreEqual(a1.Status, eRunStatus.Passed, "a1.Status=eRunStatus.Passed");
        }

        [TestMethod]
        [Ignore]
        public void Activity_With_Action_Fail_And_RunOption_Stop()
        {
            Activity a1 = new Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            a1.Acts.Add(act1);

            //Intentionally put incorrect locate Value
            ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName1", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act2);

            ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act3);


            mGR.RunRunner();


            Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Failed");
            Assert.AreEqual(act1.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Passed");
            Assert.AreEqual(act2.Status, eRunStatus.Failed, "act2.Status=eRunStatus.Failed");
            Assert.AreEqual(act3.Status, eRunStatus.Blocked, "act3.Status=eRunStatus.Blocked");
        }

        [TestMethod]
        public void Activity_With_Action_Fail_And_RunOption_Continue()
        {
            Activity a1 = new Activity();
            a1.Active = true;
            a1.ActionRunOption = Activity.eActionRunOption.ContinueActionsRunOnFailure;
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
            a1.Acts.Add(act1);

            //Intentionally put incorrect locate Value
            ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName1", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act2);


            ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
            a1.Acts.Add(act3);


            mGR.RunRunner();


            Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Failed");
            Assert.AreEqual(act1.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Passed");
            Assert.AreEqual(act2.Status, eRunStatus.Failed, "act1.Status=eRunStatus.Failed");
            Assert.AreEqual(act3.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Passed");
        }

        [TestMethod]
        public void BF_No_Activities()
        {
            //Arrange
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Status Test";
            mBF.Elapsed = 0;
            //mBF.Active = true;
            // Platform p = new Platform();
            // p.PlatformType = Platform.eType.Web;
            // mBF.Platforms = new ObservableList<Platform>();
            // mBF.Platforms.Add(p);
            // mBF.TargetApplications.Add(new TargetApplication() { AppName = "SCM" });

            // Agent a = new Agent();
            // a.DriverType = Agent.eDriverType.SeleniumFireFox;
            // p2.Agent = a;

            // mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "SCM", Agent = a });

            // mGR.Platforms.Add(p2);
            // mGR.BusinessFlows.Add(BF);

            // mGR.UpdateApplicationAgents();

            //Act
            mGR.CalculateBusinessFlowFinalStatus(mBF);

            //Assert
           Assert.AreEqual(mBF.RunStatus, eRunStatus.Skipped, "BF.RunStatus=Skipped");
        }

      
        //[TestMethod]
        //[Ignore]
        //public void BF_Activities_All_Pass()
        //{
        //    Activity a1 = new Activity();
        //    a1.Active = true;
        //    mBF.Activities.Add(a1);

        //    ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    a1.Acts.Add(act1);

            
         
            
        //    Activity a2 = new Activity();
        //    a2.Active = true;
        //    mBF.Activities.Add(a2);

        //    ActGotoURL act4 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "http://www.google.com/", Active = true };
        //    a2.Acts.Add(act4);
        //    mGR.RunRunner();

            
        //   Assert.AreEqual(a1.Status, eRunStatus.Passed, "a1.Status=eRunStatus.Passed");
        //   Assert.AreEqual(a2.Status, eRunStatus.Passed, "a2.Status=eRunStatus.Passed");
        //   Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed, "mBF.RunStatus=eRunStatus.Passed");
        //}

        //[TestMethod]
        //[Ignore]
        //public void BF_With_Activity_Fail_And_Mandatory()
        //{
        //    Activity a1 = new Activity() { };
        //    a1.Active = true;
        //    a1.Mandatory = true;
        //    mBF.Activities.Add(a1);

        //    ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    a1.Acts.Add(act1);

        //    //Intentionally put incorrect locate Value
        //    ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName1", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    a1.Acts.Add(act2);


        //    ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    a1.Acts.Add(act3);


        //    Activity a2 = new Activity();
        //    a2.Active = true;
        //    mBF.Activities.Add(a2);

        //    ActGotoURL act4 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "http://www.google.com/", Active = true };
        //    a2.Acts.Add(act4);
            
        //    mGR.RunRunner();


        //   Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Failed");
        //   Assert.AreEqual(a2.Status, eRunStatus.Blocked, "a2.Status=eRunStatus.Blocked");
        //   Assert.AreEqual(act4.Status, eRunStatus.Blocked, "act4.Status=eRunStatus.Blocked");
        //   Assert.AreEqual(mBF.RunStatus, eRunStatus.Failed, "mBF.RunStatus=eRunStatus.Failed");

        //}

        //[TestMethod]
        //[Ignore]
        //public void BF_With_Activity_Fail_But_Not_Manadatory()
        //{
        //    Activity a1 = new Activity() { };
        //    a1.Active = true;
         
         

        //    ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    a1.Acts.Add(act1);

        //    //Intentionally put incorrect locate Value
        //    ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName1", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    a1.Acts.Add(act2);


        //    ActTextBox act3 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "Password", Value = "123456", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true };
        //    a1.Acts.Add(act3);


        //    Activity a2 = new Activity();
        //    a2.Active = true;
        //    mBF.Activities.Add(a2);
        //    mBF.Activities.Add(a1);

        //    ActGotoURL act4 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "http://www.google.com/", Active = true };
        //    a2.Acts.Add(act4);
        //    mGR.RunRunner();


        //   Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Failed");
        //   Assert.AreEqual(a2.Status, eRunStatus.Passed, "a1.Status=eRunStatus.Passed");
        //   Assert.AreEqual(act4.Status, eRunStatus.Passed, "act4.Status=eRunStatus.Passed");
        //   Assert.AreEqual(mBF.RunStatus, eRunStatus.Failed, "mBF.RunStatus=eRunStatus.Failed");

        //}

        //[TestMethod]
        //[Ignore]
        //public void Timeout_WithAgent()
        //{            
        //    Activity a1 = new Activity();
        //    a1.Active = true;
        //    mBF.Activities.Add(a1);

        //    ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true };
        //    a1.Acts.Add(act1);

        //    //Intentionally put incorrect locate Value
        //    ActTextBox act2 = new ActTextBox() { LocateBy = eLocateBy.ByID, LocateValue = "UserName15555", Value = "Yaron", TextBoxAction = ActTextBox.eTextBoxAction.SetValue, Active = true, Timeout=5 };
        //    a1.Acts.Add(act2);            

        //    mGR.RunRunner();

        //   Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Fail");
        //   Assert.AreEqual(act1.Status, eRunStatus.Passed, "act1.Status=eRunStatus.Pass");
        //   Assert.AreEqual(act2.Status, eRunStatus.Failed, "act2.Status=eRunStatus.Fail");
        //    Assert.IsTrue(act2.Error.Contains("Time out"), "act2.Error Contains 'Time Out'");
        //    Assert.IsTrue(act2.ElapsedSecs < 6, "act2.ElapsedSecs < Timeout Time (5 seconds)");
        //}


        [TestMethod]
        public void Test_CalculateActivityStatus_Failed_Action()
        {

            Activity a1 = new Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            //Arrange
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true, Status = eRunStatus.Failed };
            act1.Error = "Cannot go to URL";
            a1.Acts.Add(act1);
            //Act
            mGR.CalculateActivityFinalStatus(a1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(a1.Status, eRunStatus.Failed, "a1.Status=eRunStatus.Failed");
        }
        [TestMethod]
        public void Test_CalculateActivityStatus_FailedAction_Then_Stop()
        {

            Activity a1 = new Activity();
            a1.Active = true;
            mBF.Activities.Add(a1);

            //Arrange
            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://ginger-automation.github.io/test.html", Active = true, Status = eRunStatus.Failed };
            act1.Error = "Cannot go to URL";
            a1.Acts.Add(act1);

            ActDummy act3 = new ActDummy() { Description = "A3", Active = true, Status = eRunStatus.Stopped };
            a1.Acts.Add(act3);
            //act3.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "1=1", FlowControlAction = GingerCore.FlowControlLib.FlowControl.eFlowControlAction.StopRun, Active = true });

            //Act
            mGR.CalculateActivityFinalStatus(a1);

            //Assert
            // since there is no failure we assume pass
            Assert.AreEqual(a1.Status, eRunStatus.Stopped, "a1.Status=eRunStatus.Stopped");
        }

        [TestMethod]
        public void Test_CalculateBFStatus_FailedActivity()
        {

            AutoLogProxy.Init("UT Build");
            Activity a1 = new Activity();
            a1.Active = true;
            a1.Status = eRunStatus.Failed;
            mBF.Activities.Add(a1);

            Activity a2 = new Activity();
            a2.Active = true;
            a2.Status = eRunStatus.Passed;
            mBF.Activities.Add(a2);

            mGR.CalculateBusinessFlowFinalStatus(mBF);

            Assert.AreEqual(mBF.RunStatus, eRunStatus.Failed);


        }

        [TestMethod]
        public void Test_CalculateBFStatus_FailedActivity_ThenStopped()
        {

            AutoLogProxy.Init("UT Build");
            Activity a1 = new Activity();
            a1.Active = true;
            a1.Status = eRunStatus.Failed;
            mBF.Activities.Add(a1);

            Activity a2 = new Activity();
            a2.Active = true;
            a2.Status = eRunStatus.Stopped;
            mBF.Activities.Add(a2);

            mGR.CalculateBusinessFlowFinalStatus(mBF);

            Assert.AreEqual(mBF.RunStatus, eRunStatus.Stopped);

        }


        //LastExecutedActivityStatus
        public void LastExecutedActivityStatusTest()
        {
            ActDummy a1 = new ActDummy();
            a1.Active = true;

        }
      


        //LastExecutedActivityStatus_WhenRunFromSharedRepository

        //LastExecutedActivityStatus_WithGoToActivity


    }
}
