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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    [Level3]
    [TestClass]
    public class PBDriverTest
    {

        public static BusinessFlow mBF;
        static System.Diagnostics.Process proc;
        // make it static for reuse so no need to init every time when running test by click test button
        static PBDriver mDriver = null;
        static GingerRunner mGR = null;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);

            // launch PB Test App
            if (proc == null || proc.HasExited)
            {
                Console.WriteLine(TestResources.GetTestResourcesFolder("PBTestApp"));
                Console.WriteLine("pb_test_app.exe");


                proc = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = "pb_test_app.exe", WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp"), UseShellExecute = true });
                GingerCore.General.DoEvents();
                GingerCore.General.DoEvents();
            }

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR);

            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test PB Driver";
            Platform p = new Platform();
            p.PlatformType = ePlatformType.PowerBuilder;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "PBTestAPP" });
            Activity activity = new Activity();
            activity.TargetApplication = "PBTestApp";
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mDriver = new PBDriver(mBF);
            mDriver.StartDriver();
            Agent a = new Agent();
            AgentOperations agentOperations = new AgentOperations(a);
            a.AgentOperations = agentOperations;
            a.Active = true;
            ((AgentOperations)a.AgentOperations).Driver = mDriver;
            a.DriverType = Agent.eDriverType.PowerBuilder;

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = new ObservableList<Agent>();
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(a);

            ApplicationAgent AA = new ApplicationAgent();
            AA.AppName = "PBTestApp";
            AA.Agent = a;
            mGR.ApplicationAgents.Add(AA);
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.SetCurrentActivityAgent();
            // Do Switch Window, to be ready for actions
            ActSwitchWindow c = new ActSwitchWindow();
            c.LocateBy = eLocateBy.ByTitle;
            c.LocateValue = "Simple Page";
            c.WaitTime = 10;
            mGR.Executor.RunAction(c, false);
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            mGR.Executor.StopAgents();
            mDriver = null;
            mGR = null;
            try
            {
                proc.Kill();
            }
            catch
            {

            }
            finally
            {

            }



        }

        #region Text box field
        [TestMethod]
        [Timeout(60000)]
        public void SetTextField_tb_lastname()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.AddNewReturnParams = true;
            c.LocateValue = "sle_acc_text";
            c.Value = "Jenny";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "sle_acc_text";
            act.AddNewReturnParams = true;
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);


            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Jenny", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetTextField_tb_lastname()
        {
            //Arrange                        
            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "sle_acc_text";
            c.Value = "ABCDEF";
            mGR.Executor.RunAction(c);

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "sle_acc_text";
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            //Assert.AreEqual(act.ExInfo, txt, "ExInfo");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void Senkeys_textbox()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SendKeys;
            c.AddNewReturnParams = true;
            c.LocateValue = "/mle_acc_notes";
            c.Value = "Ginger";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByXPath;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "/mle_acc_notes";
            act.AddNewReturnParams = true;
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);


            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Gingernotes", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_Value()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "address";
            c.Value = "Value";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        #endregion

        #region Windows
        [TestMethod]
        [Timeout(60000)]
        public void WindowVisualStateCheck()
        {

            // Window Maximize
            ActWindow actWinMax = new ActWindow();
            actWinMax.LocateBy = eLocateBy.ByTitle;
            actWinMax.LocateValue = "Simple Page";
            actWinMax.WindowActionType = ActWindow.eWindowActionType.Maximize;
            actWinMax.Active = true;

            mBF.CurrentActivity.Acts.Add(actWinMax);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMax;
            mGR.Executor.RunAction(actWinMax, false);

            //Assert
            Assert.AreEqual(actWinMax.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMax.Error, null, "Act.Error");

            // Window minimize
            ActWindow actWinMin = new ActWindow();
            actWinMin.LocateBy = eLocateBy.ByTitle;
            actWinMin.LocateValue = "Simple Page";
            actWinMin.WindowActionType = ActWindow.eWindowActionType.Minimize;
            actWinMin.Active = true;

            mBF.CurrentActivity.Acts.Add(actWinMin);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMin;
            mGR.Executor.RunAction(actWinMin, false);

            //Assert
            Assert.AreEqual(actWinMin.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMin.Error, null, "Act.Error");

            // Window Restore
            ActWindow actWinRes = new ActWindow();
            actWinRes.LocateBy = eLocateBy.ByTitle;
            actWinRes.LocateValue = "Simple Page";
            actWinRes.WindowActionType = ActWindow.eWindowActionType.Restore;
            actWinRes.Active = true;

            mBF.CurrentActivity.Acts.Add(actWinRes);
            mBF.CurrentActivity.Acts.CurrentItem = actWinRes;
            mGR.Executor.RunAction(actWinRes, false);

            //Assert
            Assert.AreEqual(actWinRes.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinRes.Error, null, "Act.Error");

        }
        #endregion
        #region Button

        [TestMethod]
        [Timeout(60000)]
        public void ClickButton_ByText()
        {

            // click click me button
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.Click;
            act.LocateValue = "Click Me";
            // act.LocateValueCalculated = "1021";
            act.Active = true;

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "OK";
            c.Active = true;

            //Act

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
            //Assert.AreEqual(act.ExInfo, "", "act.ExInfo");

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");
        }

        [Ignore]
        public void ClickByXY_Button()
        {

            // click click me button
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.ClickXY;
            act.LocateValue = "Click Me";
            act.Value = "2,2";
            // act.LocateValueCalculated = "1021";
            act.Active = true;

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "OK";
            c.Active = true;

            //Act

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
            //Assert.AreEqual(act.ExInfo, "", "act.ExInfo");

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");
        }

        [Ignore]
        public void DoubleClick_Button()
        {

            // click click me button
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.DoubleClick;
            act.LocateValue = "Click Me";
            // act.LocateValueCalculated = "1021";
            act.Active = true;

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "OK";
            c.Active = true;

            //Act

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
            //Assert.AreEqual(act.ExInfo, "", "act.ExInfo");

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_AutomationId_Button()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Click Me";
            c.Value = "AutomationId";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "1025", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_ClassName_Button()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Click Me";
            c.Value = "ClassName";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Button", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_IsKeyboardFocusable_Button()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Click Me";
            c.Value = "IsKeyboardFocusable";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void IsExist_Button()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsExist;
            c.AddNewReturnParams = true;
            c.LocateValue = "Tabbed";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "True");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SmartSync_WaitUntilDisplay_Button()
        {
            ActSmartSync act = new ActSmartSync();
            act.LocateBy = eLocateBy.ByName;
            act.SmartSyncAction = ActSmartSync.eSmartSyncAction.WaitUntilDisplay;
            act.LocateValue = "Click Me";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
            //Assert.AreEqual(act.ExInfo, "", "act.ExInfo");            
        }

        #endregion

        #region CheckBox
        [TestMethod]
        [Timeout(60000)]
        public void SetCheckboxValue_ByName()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "Works in Amdocs";
            //c.ValueForDriver = "Checked";
            c.Value = "Checked";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "Works in Amdocs";
            act.Active = true;
            act.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            //  Assert.AreEqual(act.ExInfo, "Checked", "ExInfo");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Checked", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");


            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            //Assert.AreEqual(c.ExInfo, "Checked set", "ExInfo");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetCheckboxValue_ByName()
        {
            //Arrange      
            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "Works in Amdocs";
            c.Value = "Checked";
            c.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "Works in Amdocs";
            act.Active = true;
            act.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Checked", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ToggleCheckboxValue_ByName()
        {//Arrange                        

            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "cbx_acc_check";
            c.ValueForDriver = "Checked";
            c.Value = "Checked";
            c.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            //toggle
            ActPBControl c1 = new ActPBControl();
            c1.LocateBy = eLocateBy.ByName;
            c1.ControlAction = ActPBControl.eControlAction.Toggle;
            c1.LocateValue = "cbx_acc_check";
            c1.AddNewReturnParams = true;
            c1.ItemName = "Toggle Checkbox cbx_acc_check";
            c1.Active = true;

            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;

            //Act
            mGR.Executor.RunAction(c1, false);


            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "cbx_acc_check";
            c1.ItemName = "Toggle Checkbox cbx_acc_check";
            act.AddNewReturnParams = true;
            act.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Unchecked", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");

        }

        [TestMethod]
        [Timeout(60000)]
        public void IsCheckboxEnabled_ByName()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsEnabled;
            c.LocateValue = "Works in Amdocs";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_ToggleState()
        {
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValue = "Illinois Resident";
            act.Active = true;
            act.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            //  Assert.AreEqual(act.ExInfo, "Checked", "ExInfo");
            string actual = act.GetReturnParam("Actual");
            if (actual.Equals("Checked"))
            {
                act = new ActPBControl();
                act.LocateBy = eLocateBy.ByName;
                act.ControlAction = ActPBControl.eControlAction.SetValue;
                act.LocateValue = "Illinois Resident";
                act.Value = "Unchecked";
                act.Active = true;
                act.AddNewReturnParams = true;

                mBF.CurrentActivity.Acts.Add(act);
                mBF.CurrentActivity.Acts.CurrentItem = act;
                //Act
                mGR.Executor.RunAction(act, false);

            }
        }

        #endregion

        #region radio button
        [TestMethod]
        [Timeout(60000)]
        public void SelectRadioButton_Bachelors()
        {
            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "Bachelors";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.IsSelected;
            act.LocateValue = "Bachelors";
            act.AddNewReturnParams = true;
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetValueRadioButton_Bachelors()
        {
            ActPBControl c1 = new ActPBControl();
            c1.LocateBy = eLocateBy.ByName;
            c1.ControlAction = ActPBControl.eControlAction.Select;
            c1.LocateValue = "Bachelors";
            c1.Active = true;

            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;
            //Act
            mGR.Executor.RunAction(c1, false);

            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsSelected;
            c.LocateValue = "Bachelors";
            c.AddNewReturnParams = true;
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void IsSelectedRadioButton_Masters()
        {
            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "Masters";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.Executor.RunAction(c, false);


            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsSelected;
            c.LocateValue = "Masters";
            c.Active = true;

            mGR.Executor.RunAction(c, false);
            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region Scroll Bar

        #endregion

        #region Title Bar
        [TestMethod]
        [Timeout(60000)]
        public void GetTitleBarText_SimplePage()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByAutomationID;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "1010";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");

        }

        [TestMethod]
        [Timeout(60000)]
        public void GetTextByName_name()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "name_t";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region combo box
        [Ignore]
        public void SelectComboBoxItem_Indian()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "ddlb_acc_nationality";
            c.Value = "Indian";
            c.AddNewReturnParams = true;
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValue = "/[AutomationId:1021]";
            c.AddNewReturnParams = true;
            c.Active = true;

            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "Indian", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");

        }

        [TestMethod]
        [Timeout(60000)]
        public void GetSelectedItem_ComboBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "/[AutomationId:1021]";
            c.Value = "India";
            c.AddNewReturnParams = true;

            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValue = "/[AutomationId:1021]";
            c.AddNewReturnParams = true;
            c.Active = true;

            mGR.Executor.RunAction(c, false);
            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "Indian", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetAllItems_ComboBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValue = "/[AutomationId:1004]";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region List box
        [TestMethod]
        [Timeout(60000)]
        public void ClickListBoxItem_English()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "English";
            c.ItemName = "Select item English";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetSelected;
            act.LocateValue = "lb_acc_language";
            act.LocateValue = "lb_acc_language";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.Executor.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "English", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }


        [Ignore]
        public void GetSelectedItems_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "English";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValue = "lb_acc_language";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);
            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "English", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }


        [TestMethod]
        [Timeout(60000)]
        public void GetAllItems_ListBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "lb_acc_language";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Vertical, English, French, Hebrew, Hindi, Russian, Spanish", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_IsOffScreen_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "IsOffScreen";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_NameProperty_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "Name";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Spanish", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_LocalizedControlType_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "LocalizedControlType";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "list item", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_IsPassword_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "IsPassword";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "False", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_IsEnabled_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "IsEnabled";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_IsSelected_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "IsSelected";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "False", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_Xpath_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "Spanish";
            c.Value = "XPATH";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "/lb_acc_language/Spanish", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_GetFieldValue_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetFieldValue;
            c.AddNewReturnParams = true;
            c.LocateValue = "lb_acc_language";
            c.Value = "English";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "French", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region Dialog control
        [TestMethod]
        [Timeout(60000)]
        public void GetDialogTitle_ClickMeButton()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetTitle;
            c.LocateValue = "MsgBox Test";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Dialog Title");
            Assert.AreEqual(actual, "MsgBox Test", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");

            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "OK";
            c.Active = true;
            //Act
            mGR.Executor.RunAction(c, false);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetDialogText_ClickMeButton()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "MsgBox Test";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);


            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Button is Clicked", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");

            //To Handle click me button dialog 
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "OK";
            c.Active = true;
            //Act
            mGR.Executor.RunAction(c, false);
        }
        #endregion

        #region Menu bar
        [TestMethod]
        [Timeout(60000)]
        public void GetAllMenuBarElements()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "Application";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "File", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [Ignore]
        public void ClickMenuItem()
        {
            ActMenuItem c = new ActMenuItem();
            c.LocateBy = eLocateBy.ByName;
            c.MenuAction = ActMenuItem.eMenuAction.Click;
            c.LocateValue = "File|New";
            c.AddNewReturnParams = true;
            c.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");

            ActPBControl pbAct = new ActPBControl();
            pbAct.LocateBy = eLocateBy.ByXPath;
            pbAct.ControlAction = ActPBControl.eControlAction.Click;
            pbAct.LocateValue = "/Menu/OK";
            pbAct.Wait = 5;
            pbAct.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(pbAct);
            mBF.CurrentActivity.Acts.CurrentItem = pbAct;
            mGR.Executor.RunAction(pbAct, false);

            Assert.AreEqual(pbAct.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(pbAct.Error, null, "c.Error");


        }

        [Ignore]
        public void ExpandMenuItem()
        {
            ActMenuItem c = new ActMenuItem();
            c.LocateBy = eLocateBy.ByName;
            c.MenuAction = ActMenuItem.eMenuAction.Expand;
            c.LocateValue = "File";
            c.AddNewReturnParams = true;
            c.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");

            ActPBControl pbAct = new ActPBControl();
            pbAct.LocateBy = eLocateBy.ByName;
            pbAct.ControlAction = ActPBControl.eControlAction.IsExist;
            pbAct.LocateValue = "New";
            pbAct.Wait = 5;
            pbAct.Active = true;
            pbAct.AddNewReturnParams = true;
            //Act
            mBF.CurrentActivity.Acts.Add(pbAct);
            mBF.CurrentActivity.Acts.CurrentItem = pbAct;
            mGR.Executor.RunAction(pbAct, false);

            Assert.AreEqual(pbAct.Status, eRunStatus.Passed, "Action Status");
            string actual = pbAct.GetReturnParam("Actual");
            Assert.AreEqual("True", actual, "Ret Param Actual");
        }

        #endregion

        #region XPath locator
        [TestMethod]
        [Timeout(60000)]
        public void SetTextBoxValue_ByXpath()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "sle_acc_text";
            c.Value = "Hello From Xpath";
            c.AddNewReturnParams = true;
            c.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "sle_acc_text";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Hello From Xpath", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetTextBoxValue_ByXpath()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "sle_acc_text";
            c.Value = "Hello From Xpath";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "[AutomationId:1026]";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Hello From Xpath", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetTextBoxValue_ByXpathWithValueProperty()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValue = "sle_acc_text";
            c.Value = "Jinendra";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValue = "[Value:Jinendra]";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Jinendra", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [Ignore]
        public void SelectCountry_ByXpathWithMultipleProperty()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValue = "/[[AutomationId:1006][Name:]]";
            c.Value = "India";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();

            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValue = "/[[AutomationId:1006][Name:]]";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "India", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "address";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.AddNewReturnParams = true;
            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Yaron";
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }


        [TestMethod]
        [Timeout(60000)]
        public void SetText_TableActionGrid()
        {
            ActTableElement actGrid1 = new ActTableElement();
            actGrid1.ByRandRow = false;
            actGrid1.BySelectedRow = false;
            actGrid1.ByWhere = true;
            actGrid1.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid1.ControlAction = ActTableElement.eTableAction.SetValue;
            actGrid1.LocateColTitle = "address";

            actGrid1.WhereColumnTitle = "name";
            actGrid1.WhereColumnValue = "Ravi";
            actGrid1.LocateRowType = "Where";
            actGrid1.LocateRowValue = "";
            actGrid1.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid1.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;

            actGrid1.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid1.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid1.LocateValue = "dw_acc_grd";
            actGrid1.LocateBy = eLocateBy.ByName;
            actGrid1.Value = "3909 Inverness Rd";
            actGrid1.Active = true;

            mBF.CurrentActivity.Acts.Add(actGrid1);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid1;
            //Act
            mGR.Executor.RunAction(actGrid1, false);

            //Assert
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "address";

            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Ravi";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = "name";

            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.AddNewReturnParams = true;
            actGrid.Active = true;

            mBF.CurrentActivity.Acts.Add(actGrid);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            //Act
            mGR.Executor.RunAction(actGrid, false);

            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual("3909 Inverness Rd", actual, "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void Click_TableAction()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.Click;
            actGrid.LocateColTitle = "address";

            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Ravi";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = "name";

            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Value = "Amdocs - Pune, India";
            actGrid.Active = true;


            mBF.CurrentActivity.Acts.Add(actGrid);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            //Act
            mGR.Executor.RunAction(actGrid, false);
            //Assert
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void DoubleClick_TableAction()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.DoubleClick;
            actGrid.LocateColTitle = "address";

            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Ravi";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = "name";

            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;


            mBF.CurrentActivity.Acts.Add(actGrid);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            //Act
            mGR.Executor.RunAction(actGrid, false);
            //Assert
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickXY_TableAction()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.ClickXY;
            actGrid.Value = "2,2";
            actGrid.LocateColTitle = "address";

            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Ravi";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = "name";

            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;


            mBF.CurrentActivity.Acts.Add(actGrid);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            //Act
            mGR.Executor.RunAction(actGrid, false);
            //Assert
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }
        #endregion

        #region Table Action on Grid
        //For Xpath with only name property
        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnRowNumColNum()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.AddNewReturnParams = true;
            actGrid.ByRandRow = false;
            actGrid.ByRowNum = true;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = false;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.LocateColTitle = "0";
            actGrid.LocateRowType = "Row Number";//
            actGrid.LocateRowValue = "1";//
            actGrid.LocateValue = "none";

            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = null;
            actGrid.WhereColumnValue = null;
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;

            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual("Ravi", actual, "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnRowNumColTitle()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.AddNewReturnParams = true;
            actGrid.ByRandRow = false;
            actGrid.ByRowNum = true;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.LocateColTitle = "name";
            actGrid.LocateRowType = "Row Number";//
            actGrid.LocateRowValue = "1";//       
            actGrid.LocateValue = "none";

            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = null;
            actGrid.WhereColumnValue = null;
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;

            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ravi", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnAnyRowColNum()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.AddNewReturnParams = true;
            actGrid.ByRandRow = false;
            actGrid.ByRowNum = true;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.LocateColTitle = "1";
            actGrid.LocateRowType = "Any Row";// 
            actGrid.LocateValue = "none";

            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = null;
            actGrid.WhereColumnValue = null;
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;

            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            if (actual != null)
            {
                actual = "pass";
            }
            Assert.AreEqual(actual, "pass", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnAnyRowColtitle()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.AddNewReturnParams = true;
            actGrid.ByRandRow = false;
            actGrid.ByRowNum = true;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;

            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.LocateColTitle = "name";
            actGrid.LocateRowType = "Any Row";// 
            actGrid.LocateValue = "none";

            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = null;
            actGrid.WhereColumnValue = null;
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;

            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            if (actual != null)
            {
                actual = "pass";
            }
            Assert.AreEqual(actual, "pass", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]

        public void GetText_TableActionGrid_OnColNumwhereColTitle()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "1";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.AddNewReturnParams = true;
            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Yaron";
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }


        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnColTitlewhereColTitle()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "address";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.AddNewReturnParams = true;
            actGrid.WhereColumnTitle = "name";
            actGrid.WhereColumnValue = "Yaron";
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnColNumwhereColNum()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "1";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.AddNewReturnParams = true;
            actGrid.WhereColumnTitle = "0";
            actGrid.WhereColumnValue = "Yaron";
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Contains;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetText_TableActionGrid_OnColTitlewhereColNum()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.ByRandRow = false;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.ControlAction = ActTableElement.eTableAction.GetValue;
            actGrid.LocateColTitle = "address";
            actGrid.LocateRowType = "Where";
            actGrid.LocateRowValue = "";
            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.AddNewReturnParams = true;
            actGrid.WhereColumnTitle = "0";
            actGrid.WhereColumnValue = "Yaron";
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Contains;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid.LocateValue = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetRowCount_TableActionGrid()
        {
            ActTableElement actGrid = new ActTableElement();
            actGrid.AddNewReturnParams = true;
            actGrid.ByRandRow = false;
            actGrid.ByRowNum = true;
            actGrid.BySelectedRow = false;
            actGrid.ByWhere = true;
            actGrid.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actGrid.ControlAction = ActTableElement.eTableAction.GetRowCount;
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.LocateColTitle = "1";
            actGrid.LocateRowType = "Row Number";//
            actGrid.LocateRowValue = "1";//
            actGrid.LocateValue = "none";

            actGrid.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid.WhereColumnTitle = null;
            actGrid.WhereColumnValue = null;
            actGrid.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;

            mGR.Executor.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "3", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        #endregion

        #region UIElement Action- Click and Validate
        [TestMethod]
        [Timeout(60000)]
        public void TestClickAndValidate_isExist()
        {
            ActUIElement actUI = new ActUIElement();
            actUI.Active = true;
            actUI.AddNewReturnParams = true;
            actUI.ElementAction = ActUIElement.eElementAction.ClickAndValidate;
            actUI.ElementLocateBy = eLocateBy.ByName;
            actUI.ElementLocateValue = "Click Me";
            actUI.ElementType = eElementType.Button;
            actUI.LocateValue = "Click Me";
            actUI.LocateBy = eLocateBy.ByName;


            ActInputValue iv = new ActInputValue();
            iv.ItemName = "ClickType";
            iv.Param = "ClickType";
            iv.Value = "InvokeClick";
            iv.ValueForDriver = "InvokeClick";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationType";
            iv.Param = "ValidationType";
            iv.Value = "Exist";
            iv.ValueForDriver = "Exist";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElement";
            iv.Param = "ValidationElement";
            iv.Value = "Text";
            iv.ValueForDriver = "Text";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocateBy";
            iv.Param = "ValidationElementLocateBy";
            iv.Value = "ByName";
            iv.ValueForDriver = "ByName";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocatorValue";
            iv.Param = "ValidationElementLocatorValue";
            iv.Value = "Button is Clicked";
            iv.ValueForDriver = "Button is Clicked";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "LoopThroughClicks";
            iv.Param = "LoopThroughClicks";
            iv.Value = "false";
            iv.ValueForDriver = "false";
            actUI.ActInputValues.Add(iv);

            mBF.CurrentActivity.Acts.Add(actUI);
            mBF.CurrentActivity.Acts.CurrentItem = actUI;

            mGR.Executor.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestClickAndValidate_NotExist()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            ActUIElement actUI = new ActUIElement();
            actUI.Active = true;
            actUI.AddNewReturnParams = true;
            actUI.ElementAction = ActUIElement.eElementAction.ClickAndValidate;
            actUI.ElementLocateBy = eLocateBy.ByXPath;
            actUI.ElementLocateValue = "/MsgBox Test/OK";
            actUI.ElementType = eElementType.Button;
            actUI.LocateValue = "/MsgBox Test/OK";
            actUI.LocateBy = eLocateBy.ByXPath;

            ActInputValue iv = new ActInputValue();
            iv.ItemName = "ClickType";
            iv.Param = "ClickType";
            iv.Value = "InvokeClick";
            iv.ValueForDriver = "InvokeClick";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationType";
            iv.Param = "ValidationType";
            iv.Value = "NotExist";
            iv.ValueForDriver = "NotExist";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElement";
            iv.Param = "ValidationElement";
            iv.Value = "Text";
            iv.ValueForDriver = "Text";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocateBy";
            iv.Param = "ValidationElementLocateBy";
            iv.Value = "ByName";
            iv.ValueForDriver = "ByName";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocatorValue";
            iv.Param = "ValidationElementLocatorValue";
            iv.Value = "Button is Clicked";
            iv.ValueForDriver = "Button is Clicked";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "LoopThroughClicks";
            iv.Param = "LoopThroughClicks";
            iv.Value = "false";
            iv.ValueForDriver = "false";
            actUI.ActInputValues.Add(iv);

            mBF.CurrentActivity.Acts.Add(actUI);
            mBF.CurrentActivity.Acts.CurrentItem = actUI;
            mGR.Executor.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestClickAndValidate_isEnabled()
        {
            ActUIElement actUI = new ActUIElement();
            actUI.Active = true;
            actUI.AddNewReturnParams = true;
            actUI.ElementAction = ActUIElement.eElementAction.ClickAndValidate;
            actUI.ElementLocateBy = eLocateBy.ByName;
            actUI.ElementLocateValue = "Tabbed";
            actUI.ElementType = eElementType.Button;
            actUI.LocateBy = eLocateBy.ByName;
            actUI.LocateValue = actUI.ElementLocateValue;

            ActInputValue iv = new ActInputValue();
            iv.ItemName = "ClickType";
            iv.Param = "ClickType";
            iv.Value = "InvokeClick";
            iv.ValueForDriver = "InvokeClick";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationType";
            iv.Param = "ValidationType";
            iv.Value = "IsEnabled";
            iv.ValueForDriver = "IsEnabled";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElement";
            iv.Param = "ValidationElement";
            iv.Value = "Button";
            iv.ValueForDriver = "Button";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocateBy";
            iv.Param = "ValidationElementLocateBy";
            iv.Value = "ByName";
            iv.ValueForDriver = "ByName";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocatorValue";
            iv.Param = "ValidationElementLocatorValue";
            iv.Value = "Exit";
            iv.ValueForDriver = "Exit";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "LoopThroughClicks";
            iv.Param = "LoopThroughClicks";
            iv.Value = "false";
            iv.ValueForDriver = "false";
            actUI.ActInputValues.Add(iv);

            mBF.CurrentActivity.Acts.Add(actUI);
            mBF.CurrentActivity.Acts.CurrentItem = actUI;

            mGR.Executor.RunAction(actUI, false);

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "Exit";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);


            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void TestClickAndValidate_LoopThroughClicks()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValue = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            ActUIElement actUI = new ActUIElement();
            actUI.Active = true;
            actUI.AddNewReturnParams = true;
            actUI.ElementAction = ActUIElement.eElementAction.ClickAndValidate;
            actUI.ElementLocateBy = eLocateBy.ByXPath;
            actUI.ElementLocateValue = "/MsgBox Test/OK";
            actUI.ElementType = eElementType.Button;
            actUI.LocateBy = eLocateBy.ByXPath;
            actUI.LocateValue = actUI.ElementLocateValue;

            ActInputValue iv = new ActInputValue();
            iv.ItemName = "ClickType";
            iv.Param = "ClickType";
            iv.Value = "InvokeClick";
            iv.ValueForDriver = "InvokeClick";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationType";
            iv.Param = "ValidationType";
            iv.Value = "NotExist";
            iv.ValueForDriver = "NotExist";
            actUI.ActInputValues.Add(iv);


            iv = new ActInputValue();
            iv.ItemName = "ValidationElement";
            iv.Param = "ValidationElement";
            iv.Value = "button";
            iv.ValueForDriver = "button";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocateBy";
            iv.Param = "ValidationElementLocateBy";
            iv.Value = "ByName";
            iv.ValueForDriver = "ByName";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "ValidationElementLocatorValue";
            iv.Param = "ValidationElementLocatorValue";
            iv.Value = "OK";
            iv.ValueForDriver = "OK";
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue();
            iv.ItemName = "LoopThroughClicks";
            iv.Param = "LoopThroughClicks";
            iv.Value = "true";
            iv.ValueForDriver = "true";
            actUI.ActInputValues.Add(iv);

            mBF.CurrentActivity.Acts.Add(actUI);
            mBF.CurrentActivity.Acts.CurrentItem = actUI;

            mGR.Executor.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }
        #endregion

        #region Text
        [TestMethod]
        [Timeout(60000)]
        public void GetControlProperty_Text_Text()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetControlProperty;
            c.AddNewReturnParams = true;
            c.LocateValue = "name_t";
            c.Value = "Text";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Name", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetTitle_Text()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetTitle;
            c.AddNewReturnParams = true;
            c.LocateValue = "/st_acc_label";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Dialog Title");
            Assert.AreEqual(actual, "st_acc_label", "True");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region Tab Control
        [TestMethod]
        [Timeout(60000)]
        public void SelectByIndex_Tab()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.AddNewReturnParams = true;
            c.LocateValue = "Tabbed";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsExist;
            c.AddNewReturnParams = true;
            c.LocateValue = "Check if you live in USA";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");

            if (actual.Equals("True"))
            {
                c = new ActPBControl();
                c.LocateBy = eLocateBy.ByXPath;
                c.ControlAction = ActPBControl.eControlAction.SelectByIndex;
                c.AddNewReturnParams = true;
                c.LocateValue = "/Tabbed Page/[AutomationId:1002]";
                c.Value = "2";
                c.Active = true;
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.Executor.RunAction(c, false);

                c = new ActPBControl();
                c.LocateBy = eLocateBy.ByName;
                c.ControlAction = ActPBControl.eControlAction.IsExist;
                c.AddNewReturnParams = true;
                c.LocateValue = "none";
                c.Active = true;
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.Executor.RunAction(c, false);
                Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
                actual = c.GetReturnParam("Actual");
                Assert.AreEqual(actual, "True", "True");
            }
            else
            {
                Assert.AreEqual(actual, "True", "True");
            }

            ActPBControl c1 = new ActPBControl();
            c1.LocateBy = eLocateBy.ByName;
            c1.ControlAction = ActPBControl.eControlAction.Click;
            c1.LocateValue = "Exit";
            c1.AddNewReturnParams = true;
            c1.Active = true;
            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;
            mGR.Executor.RunAction(c1, false);

        }



        #endregion
    }
}
