#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests.UITests.PBDriverTest
{
    [Ignore]  // Fail on Windows, too heavy and need imrpovemnets
              // TODO: add AAA, class init should not have assert and make it samller - check timings
    [TestClass]
    public class PBDriverWidgetTest
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
                proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = @"pb_test_app.exe";
                proc.StartInfo.WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp");
                Console.WriteLine(proc.StartInfo.WorkingDirectory);
                Console.WriteLine(proc.StartInfo.FileName);
                proc.Start();

                GingerCore.General.DoEvents();
                GingerCore.General.DoEvents();
            }

            mGR = new GingerRunner();
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();
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
            Agent a = new Agent
            {
                Active = true
            };
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
                LocateValueCalculated = "Simple Page",
                WaitTime = 10
            };
            mDriver.RunAction(c);
            //if(c.Status.Value==eRunStatus.Failed)
            //{
            //     c = new ActSwitchWindow();
            //    c.LocateBy = eLocateBy.ByTitle;
            //    c.LocateValueCalculated = "Simple Page";
            //    c.WaitTime = 10;
            //    mDriver.RunAction(c);

            //}

            ActPBControl action = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.SetValue,
                AddNewReturnParams = true,
                Wait = 4,
                LocateValueCalculated = "/[AutomationId:1001]",
                Value = proc.StartInfo.WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp") + @"\Browser.html",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.Executor.RunAction(action, false);

            action = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.SetValue,
                LocateValueCalculated = "Launch Widget Window",
                Active = true
            };

            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.Executor.RunAction(action, false);

            c = new ActSwitchWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValueCalculated = "CSM Widgets Test Applicaiton",
                WaitTime = 10
            };
            mDriver.RunAction(c);



            string actual = "";
            do
            {
                action = new ActPBControl
                {
                    LocateBy = eLocateBy.ByName,
                    ControlAction = ActPBControl.eControlAction.IsExist,
                    LocateValueCalculated = "Script Error",
                    AddNewReturnParams = true,
                    Timeout = 10,
                    Active = true
                };

                mBF.CurrentActivity.Acts.Add(action);
                mBF.CurrentActivity.Acts.CurrentItem = action;
                //Act
                mGR.Executor.RunAction(action, false);

                Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");
                actual = action.GetReturnParam("Actual");
                if (actual.Equals("True"))
                {
                    ActPBControl PbAct = new ActPBControl
                    {
                        LocateBy = eLocateBy.ByXPath,
                        ControlAction = ActPBControl.eControlAction.Click,
                        LocateValueCalculated = @"/Script Error/[LocalizedControlType:title bar]/Close",
                        Active = true
                    };
                    mBF.CurrentActivity.Acts.Add(PbAct);
                    mBF.CurrentActivity.Acts.CurrentItem = PbAct;
                    mGR.Executor.RunAction(PbAct, false);
                }
            } while (actual.Equals("True"));

            //proceed for switch window and initialize browser
            c = new ActSwitchWindow
            {
                LocateBy = eLocateBy.ByTitle,
                LocateValueCalculated = "CSM Widgets Test Applicaiton",
                WaitTime = 2
            };
            mDriver.RunAction(c);

            int count = 1;
            ActBrowserElement actBrowser = new ActBrowserElement();
            do
            {

                actBrowser.LocateBy = eLocateBy.ByXPath;
                actBrowser.LocateValue = @"/[AutomationId:1000]/[LocalizedControlType:pane]/[LocalizedControlType:pane]/[LocalizedControlType:pane]";

                actBrowser.ControlAction = ActBrowserElement.eControlAction.InitializeBrowser;
                actBrowser.Wait = 2;
                actBrowser.Timeout = 10;
                actBrowser.Active = true;
                mBF.CurrentActivity.Acts.Add(actBrowser);
                mBF.CurrentActivity.Acts.CurrentItem = actBrowser;
                mGR.Executor.RunAction(actBrowser, false);
                count--;
            } while (actBrowser.Status.Equals(eRunStatus.Failed) && count > 0);
            if (actBrowser.Status.Equals(eRunStatus.Failed))
            {

                Assert.AreEqual(actBrowser.Status, eRunStatus.Passed, "actBrowser.Status");
                Assert.AreEqual(actBrowser.Error, null, "actBrowser.Error");
            }
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


        }

        [TestMethod]
        [Timeout(60000)]
        public void SetValue_textbox()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.SetValue,
                LocateValueCalculated = "firstName",
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetValue,
                LocateValueCalculated = "firstName",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ginger", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        [Timeout(60000)]
        public void GetValue_Textbox()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.SetValue,
                LocateValueCalculated = "firstName",
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetValue,
                LocateValueCalculated = "firstName",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ginger", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [Ignore]
        public void SendKeys_textbox()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.Click,
                LocateValueCalculated = "ssnNumber",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.SendKeys,
                LocateValueCalculated = "ssnNumber",
                Value = "1234",
                Wait = 4,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetValue,
                LocateValueCalculated = "ssnNumber",
                AddNewReturnParams = true,
                Wait = 2,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "1234", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectFromDropdown()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.SelectFromDropDown,
                LocateValueCalculated = "accountSubType",
                Value = "EMPLOYEE",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.GetValue,
                LocateValueCalculated = "accountSubType",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void IsEnabled_Textbox()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.Enabled,
                LocateValueCalculated = "firstName",
                AddNewReturnParams = true,
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void IsVisible_Textbox()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.Visible,
                LocateValueCalculated = "firstName",
                AddNewReturnParams = true,
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetInnerText_Dropdown()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.GetInnerText,
                LocateValueCalculated = "accountSubType",
                AddNewReturnParams = true,
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "RETAILEMPLOYEEDEMOTESTDEALER EMPLOYEEVIPBUSINESSHOUSE", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetStyle_Dropdown()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetStyle,
                LocateValueCalculated = "accountSubType",
                AddNewReturnParams = true,
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "width: 140px;", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void SwitchFrame()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.SwitchFrame,
                LocateValueCalculated = "accountSubType",
                Value = "Ginger",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTest()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByXPath,
                GenElementAction = ActGenElement.eGenElementAction.Click,
                LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[6]/td[1]/a[1]",
                Active = true,
                AddNewReturnParams = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.IsExist,
                AddNewReturnParams = true,
                LocateValueCalculated = "/Message from webpage/OK",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            if (actual.Equals("True"))
            {
                c = new ActPBControl
                {
                    LocateBy = eLocateBy.ByXPath,
                    ControlAction = ActPBControl.eControlAction.Click,
                    AddNewReturnParams = true,
                    LocateValueCalculated = "/Message from webpage/OK",
                    Active = true
                };
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.Executor.RunAction(c, false);
            }
            else
            {
                Assert.AreEqual(actual, "true", "True");
                Assert.AreEqual(act.Error, null, "Act.Error");
            }
        }

        [Ignore]
        public void ClickAt_Test()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByXPath,
                GenElementAction = ActGenElement.eGenElementAction.ClickAt,
                LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[5]/td[1]",
                Value = "30,30",
                Active = true,
                AddNewReturnParams = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            ActPBControl c = new ActPBControl
            {
                LocateBy = eLocateBy.ByXPath,
                ControlAction = ActPBControl.eControlAction.IsExist,
                AddNewReturnParams = true,
                LocateValueCalculated = "/Message from webpage/OK",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.Executor.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            if (actual.Equals("True"))
            {
                c = new ActPBControl
                {
                    LocateBy = eLocateBy.ByXPath,
                    ControlAction = ActPBControl.eControlAction.Click,
                    AddNewReturnParams = true,
                    LocateValueCalculated = "/Message from webpage/OK",
                    Active = true
                };
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.Executor.RunAction(c, false);
            }
            else
            {
                Assert.AreEqual(actual, "true", "True");
                Assert.AreEqual(act.Error, null, "Act.Error");
            }
        }

        [Ignore]
        public void RightClick_Test()
        {
            ActGenElement act = new ActGenElement();
            //act.LocateBy = eLocateBy.ByName;
            //act.GenElementAction = ActGenElement.eGenElementAction.Hover;
            //act.LocateValueCalculated = "lastName";            
            //act.Active = true;
            //mBF.CurrentActivity.Acts.Add(act);
            //mBF.CurrentActivity.Acts.CurrentItem = act;
            //mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.SetValue,
                LocateValueCalculated = "lastName",
                Value = "CoreTeam",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.SetValue,
                LocateValueCalculated = "lastName",
                Value = "abc@gmail.com",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.RightClick,
                LocateValueCalculated = "lastName",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByXPath,
                GenElementAction = ActGenElement.eGenElementAction.SendKeys,
                LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[3]/td[3]/input[1]",
                Value = "u",
                Wait = 2,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetValue,
                LocateValueCalculated = "lastName",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "CoreTeam", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        [Timeout(60000)]
        public void GetAttributeValue_Size()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.GetCustomAttribute,
                LocateValueCalculated = "firstName",
                Value = "size",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "20", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void SwitchFrame_GenElement()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.SwitchFrame,
                LocateValueCalculated = "accountSubType",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void Hover_Test()
        {
            // TODO: 
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.Hover,
                LocateValueCalculated = "stateForStandard",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);
            ActPBControl action = new ActPBControl();


            string actual = "";
            action = new ActPBControl
            {
                LocateBy = eLocateBy.ByName,
                ControlAction = ActPBControl.eControlAction.IsExist,
                LocateValueCalculated = "Script Error",
                AddNewReturnParams = true,
                Timeout = 10,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.Executor.RunAction(action, false);


            Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");
            actual = action.GetReturnParam("Actual");
            if (actual.Equals("True"))
            {
                Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");

                while (!String.IsNullOrEmpty(actual) && actual.Equals("True"))
                {
                    ActPBControl PbAct = new ActPBControl
                    {
                        LocateBy = eLocateBy.ByXPath,
                        ControlAction = ActPBControl.eControlAction.Click,
                        LocateValueCalculated = @"/Script Error/[LocalizedControlType:title bar]/Close",
                        Active = true,
                        AddNewReturnParams = true
                    };
                    mBF.CurrentActivity.Acts.Add(PbAct);
                    mBF.CurrentActivity.Acts.CurrentItem = PbAct;
                    mGR.Executor.RunAction(PbAct, false);


                    action = new ActPBControl
                    {
                        LocateBy = eLocateBy.ByName,
                        ControlAction = ActPBControl.eControlAction.IsExist,
                        LocateValueCalculated = "Script Error",
                        AddNewReturnParams = true,
                        Timeout = 10,
                        Active = true
                    };
                    mBF.CurrentActivity.Acts.Add(action);
                    mBF.CurrentActivity.Acts.CurrentItem = action;
                    //Act
                    mGR.Executor.RunAction(action, false);
                    actual = PbAct.GetReturnParam("Actual");
                }
            }
            else
            {
                Assert.AreEqual(action.Status, eRunStatus.Failed, "Action Status");
            }

            ActGenElement actGen = new ActGenElement
            {
                LocateBy = eLocateBy.ByName,
                GenElementAction = ActGenElement.eGenElementAction.Click,
                LocateValueCalculated = "lastName",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(actGen);
            mBF.CurrentActivity.Acts.CurrentItem = actGen;
            mGR.Executor.RunAction(actGen, false);
        }

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void GetURL_Testing()
        //{
        //    ActBrowserElement act = new ActBrowserElement();
        //    act.LocateBy = eLocateBy.ByName;
        //    act.ControlAction = ActBrowserElement.eControlAction.GetPageURL;            
        //    act.AddNewReturnParams = true;
        //    act.Active = true;
        //    mBF.CurrentActivity.Acts.Add(act);
        //    mBF.CurrentActivity.Acts.CurrentItem = act;
        //    mGR.Executor.RunAction(act, false);

        //    Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
        //    string actual = act.GetReturnParam("Actual");
        //    string expected = @"file://" + TestResources.GetTestResourcesFolder("PBTestApp") + @"\Browser.html"; 

        //    Assert.AreEqual(actual, expected, "True");
        //    Assert.AreEqual(act.Error, null, "Act.Error");
        //}


        [Ignore]
        public void SwitchFrame_Testing()
        {
            ActBrowserElement act = new ActBrowserElement
            {
                LocateBy = eLocateBy.ByClassName,
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                Value = "Internet Explorer_Server",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SwitchToDefaultFrame_Testing()
        {
            ActBrowserElement act = new ActBrowserElement
            {
                LocateBy = eLocateBy.ByClassName,
                ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame,
                Value = "Internet Explorer_Server",
                AddNewReturnParams = true,
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetElementAttribute_Dropdown()
        {
            ActGenElement act = new ActGenElement
            {
                LocateBy = eLocateBy.ByID,
                GenElementAction = ActGenElement.eGenElementAction.GetElementAttributeValue,
                LocateValueCalculated = "accountSubType",
                AddNewReturnParams = true,
                Value = "Ginger",
                Active = true
            };
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.Executor.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "RETAILEMPLOYEEDEMOTESTDEALER EMPLOYEEVIPBUSINESSHOUSE", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }





    }
}
