#region License
/*
Copyright © 2014-2025 European Support Limited

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
            mGR.Executor = new GingerExecutionEngine(mGR)
            {
                CurrentSolution = new Ginger.SolutionGeneral.Solution()
            };
            mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF Test PB Driver"
            };
            Platform p = new Platform
            {
                PlatformType = ePlatformType.PowerBuilder
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "PBTestAPP" });
            Activity activity = new Activity
            {
                TargetApplication = "PBTestApp"
            };
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

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [a];

            ApplicationAgent AA = new ApplicationAgent
            {
                AppName = "PBTestApp",
                Agent = a
            };
            mGR.ApplicationAgents.Add(AA);
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.SetCurrentActivityAgent();
            // Do Switch Window, to be ready for actions
            ActSwitchWindow c = new ActSwitchWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValue = "Simple Page",
                WaitTime = 10
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                AddNewReturnParams = true,
                LocateValue = "sle_acc_text",
                Value = "Jenny",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "sle_acc_text",
                AddNewReturnParams = true,
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "sle_acc_text",
                Value = "ABCDEF"
            };
            mGR.Executor.RunAction(c);

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            // Prep Get Value Action
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "sle_acc_text",
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.SendKeys,
                AddNewReturnParams = true,
                LocateValue = "/mle_acc_notes",
                Value = "Ginger",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "/mle_acc_notes",
                AddNewReturnParams = true,
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "address",
                Value = "Value",
                Active = true
            };
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
            ActWindow actWinMax = new ActWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValue = "Simple Page",
                WindowActionType = ActWindow.eWindowActionType.Maximize,
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(actWinMax);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMax;
            mGR.Executor.RunAction(actWinMax, false);

            //Assert
            Assert.AreEqual(actWinMax.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMax.Error, null, "Act.Error");

            // Window minimize
            ActWindow actWinMin = new ActWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValue = "Simple Page",
                WindowActionType = ActWindow.eWindowActionType.Minimize,
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(actWinMin);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMin;
            mGR.Executor.RunAction(actWinMin, false);

            //Assert
            Assert.AreEqual(actWinMin.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMin.Error, null, "Act.Error");

            // Window Restore
            ActWindow actWinRes = new ActWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValue = "Simple Page",
                WindowActionType = ActWindow.eWindowActionType.Restore,
                Active = true
            };

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
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Click Me",
                // act.LocateValueCalculated = "1021";
                Active = true
            };

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "OK",
                Active = true
            };

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
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.ClickXY,
                LocateValue = "Click Me",
                Value = "2,2",
                // act.LocateValueCalculated = "1021";
                Active = true
            };

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "OK",
                Active = true
            };

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
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.DoubleClick,
                LocateValue = "Click Me",
                // act.LocateValueCalculated = "1021";
                Active = true
            };

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "OK",
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Click Me",
                Value = "AutomationId",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Click Me",
                Value = "ClassName",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Click Me",
                Value = "IsKeyboardFocusable",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsExist,
                AddNewReturnParams = true,
                LocateValue = "Tabbed",
                Active = true
            };
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
            ActSmartSync act = new ActSmartSync
            {
                LocateBy = eLocateBy.ByName,
                SmartSyncAction = ActSmartSync.eSmartSyncAction.WaitUntilDisplay,
                LocateValue = "Click Me",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "Works in Amdocs",
                //c.ValueForDriver = "Checked";
                Value = "Checked",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);


            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "Works in Amdocs",
                Active = true,
                AddNewReturnParams = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "Works in Amdocs",
                Value = "Checked",
                AddNewReturnParams = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            // Prep Get Value Action
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "Works in Amdocs",
                Active = true,
                AddNewReturnParams = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "cbx_acc_check",
                ValueForDriver = "Checked",
                Value = "Checked",
                AddNewReturnParams = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            //toggle
            ActPBControl c1 = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Toggle,
                LocateValue = "cbx_acc_check",
                AddNewReturnParams = true,
                ItemName = "Toggle Checkbox cbx_acc_check",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;

            //Act
            mGR.Executor.RunAction(c1, false);


            // Prep Get Value Action
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "cbx_acc_check"
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsEnabled,
                LocateValue = "Works in Amdocs",
                Active = true
            };

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
            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "Illinois Resident",
                Active = true,
                AddNewReturnParams = true
            };

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
                act = new ActPBControl
                {
                    LocateBy = eLocateBy.ByName,
                    ControlAction = ActPBControl.eControlAction.SetValue,
                    LocateValue = "Illinois Resident",
                    Value = "Unchecked",
                    Active = true,
                    AddNewReturnParams = true
                };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "Bachelors",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsSelected,
                LocateValue = "Bachelors",
                AddNewReturnParams = true,
                Active = true
            };

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
            ActPBControl c1 = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "Bachelors",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;
            //Act
            mGR.Executor.RunAction(c1, false);

            //Select the radio button
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsSelected,
                LocateValue = "Bachelors",
                AddNewReturnParams = true,
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "Masters",
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByAutomationID,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "1010",
                Active = true
            };

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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "name_t",
                Active = true
            };
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

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "ddlb_acc_nationality",
                Value = "Indian",
                AddNewReturnParams = true,
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(c.Error, null, "Act.Error");

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "/[AutomationId:1021]",
                AddNewReturnParams = true,
                Active = true
            };

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

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "/[AutomationId:1021]",
                Value = "India",
                AddNewReturnParams = true,

                Active = true
            };
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "/[AutomationId:1021]",
                AddNewReturnParams = true,
                Active = true
            };

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

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "/[AutomationId:1004]",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "English",
                ItemName = "Select item English",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            ActPBControl act = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "lb_acc_language"
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "English",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "lb_acc_language",
                AddNewReturnParams = true,
                Active = true
            };
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

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "lb_acc_language",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "IsOffScreen",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "Name",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "LocalizedControlType",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "IsPassword",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "IsEnabled",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "IsSelected",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "Spanish",
                Value = "XPATH",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetFieldValue,
                AddNewReturnParams = true,
                LocateValue = "lb_acc_language",
                Value = "English",
                Active = true
            };
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

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Click Me",
                Active = true
            };
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetTitle,
                LocateValue = "MsgBox Test",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Click Me",
                Active = true
            };
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "MsgBox Test",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "Application",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActMenuItem c = new ActMenuItem
            {
                LocateBy = eLocateBy.ByName,
                MenuAction = ActMenuItem.eMenuAction.Click,
                LocateValue = "File|New",
                AddNewReturnParams = true,
                Active = true
            };

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");

            ActPBControl pbAct = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "/Menu/OK",
                Wait = 5,
                Active = true
            };
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
            ActMenuItem c = new ActMenuItem
            {
                LocateBy = eLocateBy.ByName,
                MenuAction = ActMenuItem.eMenuAction.Expand,
                LocateValue = "File",
                AddNewReturnParams = true,
                Active = true
            };

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
            Assert.AreEqual(c.Error, null, "c.Error");

            ActPBControl pbAct = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsExist,
                LocateValue = "New",
                Wait = 5,
                Active = true,
                AddNewReturnParams = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "sle_acc_text",
                Value = "Hello From Xpath",
                AddNewReturnParams = true,
                Active = true
            };

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "sle_acc_text",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "sle_acc_text",
                Value = "Hello From Xpath",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "[AutomationId:1026]",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValue = "sle_acc_text",
                Value = "Jinendra",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetValue,
                LocateValue = "[Value:Jinendra]",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.Select,
                LocateValue = "/[[AutomationId:1006][Name:]]",
                Value = "India",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetSelected,
                LocateValue = "/[[AutomationId:1006][Name:]]",
                AddNewReturnParams = true,
                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "address",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                AddNewReturnParams = true,
                WhereColumnTitle = "name",
                WhereColumnValue = "Yaron",
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Active = true
            };
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
            ActTableElement actGrid1 = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.SetValue,
                LocateColTitle = "address",

                WhereColumnTitle = "name",
                WhereColumnValue = "Ravi",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,

                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Value = "3909 Inverness Rd",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(actGrid1);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid1;
            //Act
            mGR.Executor.RunAction(actGrid1, false);

            //Assert
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "address",

                WhereColumnTitle = "name",
                WhereColumnValue = "Ravi",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.Click,
                LocateColTitle = "address",

                WhereColumnTitle = "name",
                WhereColumnValue = "Ravi",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.DoubleClick,
                LocateColTitle = "address",

                WhereColumnTitle = "name",
                WhereColumnValue = "Ravi",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.ClickXY,
                Value = "2,2",
                LocateColTitle = "address",

                WhereColumnTitle = "name",
                WhereColumnValue = "Ravi",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                AddNewReturnParams = true,
                ByRandRow = false,
                ByRowNum = true,
                BySelectedRow = false,
                ByWhere = false,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateBy = eLocateBy.ByName,
                LocateColTitle = "0",
                LocateRowType = "Row Number",//
                LocateRowValue = "1",//
                LocateValue = "none",

                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                WhereColumnTitle = null,
                WhereColumnValue = null,
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,

                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                AddNewReturnParams = true,
                ByRandRow = false,
                ByRowNum = true,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateBy = eLocateBy.ByName,
                LocateColTitle = "name",
                LocateRowType = "Row Number",//
                LocateRowValue = "1",//       
                LocateValue = "none",

                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                WhereColumnTitle = null,
                WhereColumnValue = null,
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,

                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                AddNewReturnParams = true,
                ByRandRow = false,
                ByRowNum = true,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateBy = eLocateBy.ByName,
                LocateColTitle = "1",
                LocateRowType = "Any Row",// 
                LocateValue = "none",

                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                WhereColumnTitle = null,
                WhereColumnValue = null,
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,

                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                AddNewReturnParams = true,
                ByRandRow = false,
                ByRowNum = true,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,

                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateBy = eLocateBy.ByName,
                LocateColTitle = "name",
                LocateRowType = "Any Row",// 
                LocateValue = "none",

                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                WhereColumnTitle = null,
                WhereColumnValue = null,
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,

                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "1",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                AddNewReturnParams = true,
                WhereColumnTitle = "name",
                WhereColumnValue = "Yaron",
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "address",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                AddNewReturnParams = true,
                WhereColumnTitle = "name",
                WhereColumnValue = "Yaron",
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "1",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum,
                AddNewReturnParams = true,
                WhereColumnTitle = "0",
                WhereColumnValue = "Yaron",
                WhereOperator = ActTableElement.eRunColOperator.Contains,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                ByRandRow = false,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle,
                ControlAction = ActTableElement.eTableAction.GetValue,
                LocateColTitle = "address",
                LocateRowType = "Where",
                LocateRowValue = "",
                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum,
                AddNewReturnParams = true,
                WhereColumnTitle = "0",
                WhereColumnValue = "Yaron",
                WhereOperator = ActTableElement.eRunColOperator.Contains,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,
                LocateValue = "dw_acc_grd",
                LocateBy = eLocateBy.ByName,
                Active = true
            };
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
            ActTableElement actGrid = new ActTableElement
            {
                AddNewReturnParams = true,
                ByRandRow = false,
                ByRowNum = true,
                BySelectedRow = false,
                ByWhere = true,
                ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum,
                ControlAction = ActTableElement.eTableAction.GetRowCount,
                LocateBy = eLocateBy.ByName,
                LocateColTitle = "1",
                LocateRowType = "Row Number",//
                LocateRowValue = "1",//
                LocateValue = "none",

                RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum,
                WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle,
                WhereColumnTitle = null,
                WhereColumnValue = null,
                WhereOperator = ActTableElement.eRunColOperator.Equals,
                WhereProperty = ActTableElement.eRunColPropertyValue.Value,

                Active = true
            };
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
            ActUIElement actUI = new ActUIElement
            {
                Active = true,
                AddNewReturnParams = true,
                ElementAction = ActUIElement.eElementAction.ClickAndValidate,
                ElementLocateBy = eLocateBy.ByName,
                ElementLocateValue = "Click Me",
                ElementType = eElementType.Button,
                LocateValue = "Click Me",
                LocateBy = eLocateBy.ByName
            };


            ActInputValue iv = new ActInputValue
            {
                ItemName = "ClickType",
                Param = "ClickType",
                Value = "InvokeClick",
                ValueForDriver = "InvokeClick"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationType",
                Param = "ValidationType",
                Value = "Exist",
                ValueForDriver = "Exist"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElement",
                Param = "ValidationElement",
                Value = "Text",
                ValueForDriver = "Text"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocateBy",
                Param = "ValidationElementLocateBy",
                Value = "ByName",
                ValueForDriver = "ByName"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocatorValue",
                Param = "ValidationElementLocatorValue",
                Value = "Button is Clicked",
                ValueForDriver = "Button is Clicked"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "LoopThroughClicks",
                Param = "LoopThroughClicks",
                Value = "false",
                ValueForDriver = "false"
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Click Me",
                Active = true
            };
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            ActUIElement actUI = new ActUIElement
            {
                Active = true,
                AddNewReturnParams = true,
                ElementAction = ActUIElement.eElementAction.ClickAndValidate,
                ElementLocateBy = eLocateBy.ByXPath,
                ElementLocateValue = "/MsgBox Test/OK",
                ElementType = eElementType.Button,
                LocateValue = "/MsgBox Test/OK",
                LocateBy = eLocateBy.ByXPath
            };

            ActInputValue iv = new ActInputValue
            {
                ItemName = "ClickType",
                Param = "ClickType",
                Value = "InvokeClick",
                ValueForDriver = "InvokeClick"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationType",
                Param = "ValidationType",
                Value = "NotExist",
                ValueForDriver = "NotExist"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElement",
                Param = "ValidationElement",
                Value = "Text",
                ValueForDriver = "Text"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocateBy",
                Param = "ValidationElementLocateBy",
                Value = "ByName",
                ValueForDriver = "ByName"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocatorValue",
                Param = "ValidationElementLocatorValue",
                Value = "Button is Clicked",
                ValueForDriver = "Button is Clicked"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "LoopThroughClicks",
                Param = "LoopThroughClicks",
                Value = "false",
                ValueForDriver = "false"
            };
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
            ActUIElement actUI = new ActUIElement
            {
                Active = true,
                AddNewReturnParams = true,
                ElementAction = ActUIElement.eElementAction.ClickAndValidate,
                ElementLocateBy = eLocateBy.ByName,
                ElementLocateValue = "Tabbed",
                ElementType = eElementType.Button,
                LocateBy = eLocateBy.ByName
            };
            actUI.LocateValue = actUI.ElementLocateValue;

            ActInputValue iv = new ActInputValue
            {
                ItemName = "ClickType",
                Param = "ClickType",
                Value = "InvokeClick",
                ValueForDriver = "InvokeClick"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationType",
                Param = "ValidationType",
                Value = "IsEnabled",
                ValueForDriver = "IsEnabled"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElement",
                Param = "ValidationElement",
                Value = "Button",
                ValueForDriver = "Button"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocateBy",
                Param = "ValidationElementLocateBy",
                Value = "ByName",
                ValueForDriver = "ByName"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocatorValue",
                Param = "ValidationElementLocatorValue",
                Value = "Exit",
                ValueForDriver = "Exit"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "LoopThroughClicks",
                Param = "LoopThroughClicks",
                Value = "false",
                ValueForDriver = "false"
            };
            actUI.ActInputValues.Add(iv);

            mBF.CurrentActivity.Acts.Add(actUI);
            mBF.CurrentActivity.Acts.CurrentItem = actUI;

            mGR.Executor.RunAction(actUI, false);

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Exit",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Click Me",
                Active = true
            };
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.Executor.RunAction(c, false);

            ActUIElement actUI = new ActUIElement
            {
                Active = true,
                AddNewReturnParams = true,
                ElementAction = ActUIElement.eElementAction.ClickAndValidate,
                ElementLocateBy = eLocateBy.ByXPath,
                ElementLocateValue = "/MsgBox Test/OK",
                ElementType = eElementType.Button,
                LocateBy = eLocateBy.ByXPath
            };
            actUI.LocateValue = actUI.ElementLocateValue;

            ActInputValue iv = new ActInputValue
            {
                ItemName = "ClickType",
                Param = "ClickType",
                Value = "InvokeClick",
                ValueForDriver = "InvokeClick"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationType",
                Param = "ValidationType",
                Value = "NotExist",
                ValueForDriver = "NotExist"
            };
            actUI.ActInputValues.Add(iv);


            iv = new ActInputValue
            {
                ItemName = "ValidationElement",
                Param = "ValidationElement",
                Value = "button",
                ValueForDriver = "button"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocateBy",
                Param = "ValidationElementLocateBy",
                Value = "ByName",
                ValueForDriver = "ByName"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "ValidationElementLocatorValue",
                Param = "ValidationElementLocatorValue",
                Value = "OK",
                ValueForDriver = "OK"
            };
            actUI.ActInputValues.Add(iv);

            iv = new ActInputValue
            {
                ItemName = "LoopThroughClicks",
                Param = "LoopThroughClicks",
                Value = "true",
                ValueForDriver = "true"
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.GetControlProperty,
                AddNewReturnParams = true,
                LocateValue = "name_t",
                Value = "Text",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.GetTitle,
                AddNewReturnParams = true,
                LocateValue = "/st_acc_label",
                Active = true
            };
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
            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                AddNewReturnParams = true,
                LocateValue = "Tabbed",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            c = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsExist,
                AddNewReturnParams = true,
                LocateValue = "Check if you live in USA",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");

            if (actual.Equals("True"))
            {
                c = new ActPBControl
                {
                    LocateBy = eLocateBy.ByXPath,
                    ControlAction = ActPBControl.eControlAction.SelectByIndex,
                    AddNewReturnParams = true,
                    LocateValue = "/Tabbed Page/[AutomationId:1002]",
                    Value = "2",
                    Active = true
                };
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.Executor.RunAction(c, false);

                c = new ActPBControl
                {
                    LocateBy = eLocateBy.ByName,
                    ControlAction = ActPBControl.eControlAction.IsExist,
                    AddNewReturnParams = true,
                    LocateValue = "none",
                    Active = true
                };
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

            ActPBControl c1 = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.Click,
                LocateValue = "Exit",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;
            mGR.Executor.RunAction(c1, false);

        }



        #endregion
    }
}
