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
using System.Diagnostics;
using Amdocs.Ginger.Common;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers.PBDriver;
using GingerCore.Platforms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.UIElement;
using GingerTestHelper;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace UnitTests.UITests.PBDriverTest
{
       
    [TestClass]
    public class PBDriverWidgetTest
    {
        public static BusinessFlow mBF;
        static Process proc;
        // make it static for reuse so no need to init every time when running test by click test button
        static PBDriver mDriver = null;
        static GingerRunner mGR = null;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            // launch PB Test App
            if (proc == null || proc.HasExited)
            {
                proc = new Process();
                proc.StartInfo.FileName = @"pb_test_app.exe";
                proc.StartInfo.WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp");
                Console.WriteLine(proc.StartInfo.WorkingDirectory);
                Console.WriteLine(proc.StartInfo.FileName);
                proc.Start();

                GingerCore.General.DoEvents();
                GingerCore.General.DoEvents();
            }

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test PB Driver";
            Platform p = new Platform();
            p.PlatformType = ePlatformType.PowerBuilder;
            mBF.Platforms = new ObservableList<Platform>();
            mBF.Platforms.Add(p);
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "PBTestAPP" });
            Activity activity = new Activity();
            activity.TargetApplication = "PBTestApp";
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mDriver = new PBDriver(mBF);
            mDriver.StartDriver();
            Agent a = new Agent();
            a.Active = true;
            a.Driver = mDriver;
            a.DriverType = Agent.eDriverType.PowerBuilder;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            

            ApplicationAgent AA = new ApplicationAgent();
            AA.AppName = "PBTestApp";
            AA.Agent = a;
            mGR.ApplicationAgents.Add(AA);
            mGR.CurrentBusinessFlow = mBF;
            mGR.SetCurrentActivityAgent();
            // Do Switch Window, to be ready for actions
            ActSwitchWindow c = new ActSwitchWindow();
            c.LocateBy = eLocateBy.ByTitle;
            c.LocateValueCalculated = "Simple Page";
            c.WaitTime = 10;
            mDriver.RunAction(c);
            //if(c.Status.Value==eRunStatus.Failed)
            //{
            //     c = new ActSwitchWindow();
            //    c.LocateBy = eLocateBy.ByTitle;
            //    c.LocateValueCalculated = "Simple Page";
            //    c.WaitTime = 10;
            //    mDriver.RunAction(c);

            //}

            ActPBControl action = new ActPBControl();
            action.LocateBy = eLocateBy.ByXPath;
            action.ControlAction = ActPBControl.eControlAction.SetValue;
            action.AddNewReturnParams = true;
            action.Wait = 4;
            action.LocateValueCalculated = "/[AutomationId:1001]";
            action.Value = proc.StartInfo.WorkingDirectory = TestResources.GetTestResourcesFolder("PBTestApp") + @"\Browser.html";
            action.Active = true;

            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.RunAction(action, false);

            action = new ActPBControl();
            action.LocateBy = eLocateBy.ByName;
            action.ControlAction = ActPBControl.eControlAction.SetValue;
            action.LocateValueCalculated = "Launch Widget Window";
            action.Active = true;

            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.RunAction(action, false);

            c = new ActSwitchWindow();
            c.LocateBy = eLocateBy.ByTitle;
            c.LocateValueCalculated = "CSM Widgets Test Applicaiton";            
            c.WaitTime = 10;
            mDriver.RunAction(c);



            string actual = "";
            do
            {
                action = new ActPBControl();
                action.LocateBy = eLocateBy.ByName;
                action.ControlAction = ActPBControl.eControlAction.IsExist;
                action.LocateValueCalculated = "Script Error";
                action.AddNewReturnParams = true;
                action.Timeout = 10;
                action.Active = true;

                mBF.CurrentActivity.Acts.Add(action);
                mBF.CurrentActivity.Acts.CurrentItem = action;
                //Act
                mGR.RunAction(action, false);

                Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");
                actual = action.GetReturnParam("Actual");
                if (actual.Equals("True"))
                {
                    ActPBControl PbAct = new ActPBControl();
                    PbAct.LocateBy = eLocateBy.ByXPath;
                    PbAct.ControlAction = ActPBControl.eControlAction.Click;
                    PbAct.LocateValueCalculated = @"/Script Error/[LocalizedControlType:title bar]/Close";
                    PbAct.Active = true;                    
                    mBF.CurrentActivity.Acts.Add(PbAct);
                    mBF.CurrentActivity.Acts.CurrentItem = PbAct;
                    mGR.RunAction(PbAct, false);
                }
            } while (actual.Equals("True"));

            //proceed for switch window and initialize browser
            c = new ActSwitchWindow();
            c.LocateBy = eLocateBy.ByTitle;
            c.LocateValueCalculated = "CSM Widgets Test Applicaiton";
            c.WaitTime = 2;
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
                mGR.RunAction(actBrowser, false);                
                count--;
            } while (actBrowser.Status.Equals(eRunStatus.Failed) && count > 0);
            if(actBrowser.Status.Equals(eRunStatus.Failed))
            {
                
                Assert.AreEqual(actBrowser.Status, eRunStatus.Passed, "actBrowser.Status");
                Assert.AreEqual(actBrowser.Error, null, "actBrowser.Error");
            }            
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            mGR.StopAgents();
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
        public void SetValue_textbox()
        {            
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
            act.LocateValueCalculated = "firstName";
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
            
            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetValue;
            act.LocateValueCalculated = "firstName";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ginger", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        public void GetValue_Textbox()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
            act.LocateValueCalculated = "firstName";
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetValue;
            act.LocateValueCalculated = "firstName";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ginger", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [Ignore]
        public void SendKeys_textbox()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.Click;
            act.LocateValueCalculated = "ssnNumber";            
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            act.LocateValueCalculated = "ssnNumber";
            act.Value = "1234";
            act.Wait = 4;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetValue;
            act.LocateValueCalculated = "ssnNumber";
            act.AddNewReturnParams = true;
            act.Wait = 2;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "1234", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        public void SelectFromDropdown()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.SelectFromDropDown;
            act.LocateValueCalculated = "accountSubType";
            act.Value = "EMPLOYEE";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.GetValue;
            act.LocateValueCalculated = "accountSubType";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsEnabled_Textbox()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.Enabled;
            act.LocateValueCalculated = "firstName";
            act.AddNewReturnParams = true;
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
            
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsVisible_Textbox()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.Visible;
            act.LocateValueCalculated = "firstName";
            act.AddNewReturnParams = true;
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetInnerText_Dropdown()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.GetInnerText;
            act.LocateValueCalculated = "accountSubType";
            act.AddNewReturnParams = true;
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "RETAILEMPLOYEEDEMOTESTDEALER EMPLOYEEVIPBUSINESSHOUSE", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetStyle_Dropdown()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetStyle;
            act.LocateValueCalculated = "accountSubType";
            act.AddNewReturnParams = true;
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "width: 140px;", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void SwitchFrame()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.SwitchFrame;
            act.LocateValueCalculated = "accountSubType";
            act.Value = "Ginger";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void ClickTest()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByXPath;
            act.GenElementAction = ActGenElement.eGenElementAction.Click;
            act.LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[6]/td[1]/a[1]";
            act.Active = true;
            act.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.IsExist;
            c.AddNewReturnParams = true;
            c.LocateValueCalculated = "/Message from webpage/OK";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");            
            if(actual.Equals("True"))
            {
                c = new ActPBControl();
                c.LocateBy = eLocateBy.ByXPath;
                c.ControlAction = ActPBControl.eControlAction.Click;
                c.AddNewReturnParams = true;
                c.LocateValueCalculated = "/Message from webpage/OK";
                c.Active = true;
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.RunAction(c, false);
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
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByXPath;
            act.GenElementAction = ActGenElement.eGenElementAction.ClickAt;
            act.LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[5]/td[1]";
            act.Value = "30,30";
            act.Active = true;
            act.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.IsExist;
            c.AddNewReturnParams = true;
            c.LocateValueCalculated = "/Message from webpage/OK";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            if (actual.Equals("True"))
            {
                c = new ActPBControl();
                c.LocateBy = eLocateBy.ByXPath;
                c.ControlAction = ActPBControl.eControlAction.Click;
                c.AddNewReturnParams = true;
                c.LocateValueCalculated = "/Message from webpage/OK";
                c.Active = true;
                mBF.CurrentActivity.Acts.Add(c);
                mBF.CurrentActivity.Acts.CurrentItem = c;
                //Act
                mGR.RunAction(c, false);
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
            //mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
            act.LocateValueCalculated = "lastName";
            act.Value = "CoreTeam";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

             act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.SetValue;
            act.LocateValueCalculated = "lastName";
            act.Value = "abc@gmail.com";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.RightClick;
            act.LocateValueCalculated = "lastName";            
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByXPath;
            act.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            act.LocateValueCalculated = "/html[1]/body[1]/div[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[1]/td[3]/div[1]/form[1]/table[1]/tbody[1]/tr[3]/td[1]/table[1]/tbody[1]/tr[2]/td[1]/table[1]/tbody[1]/tr[2]/td[2]/table[1]/tbody[1]/tr[1]/td[1]/table[1]/tbody[1]/tr[3]/td[3]/input[1]";
            act.Value = "u";
            act.Wait = 2;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetValue;
            act.LocateValueCalculated = "lastName";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "CoreTeam", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");


        }

        [TestMethod]
        public void GetAttributeValue_Size()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.GetCustomAttribute;
            act.LocateValueCalculated = "firstName";
            act.Value = "size";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "20", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void SwitchFrame_GenElement()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.SwitchFrame;
            act.LocateValueCalculated = "accountSubType";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "true", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [Ignore]
        public void Hover_Test()
        {
            // TODO: 
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByName;
            act.GenElementAction = ActGenElement.eGenElementAction.Hover;
            act.LocateValueCalculated = "stateForStandard";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
            ActPBControl action = new ActPBControl();


            string actual = "";
            action = new ActPBControl();
            action.LocateBy = eLocateBy.ByName;
            action.ControlAction = ActPBControl.eControlAction.IsExist;
            action.LocateValueCalculated = "Script Error";
            action.AddNewReturnParams = true;
            action.Timeout = 10;
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.RunAction(action, false);


            Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");
            actual = action.GetReturnParam("Actual");
            if (actual.Equals("True"))
            {
                Assert.AreEqual(action.Status, eRunStatus.Passed, "Action Status");

                while (!String.IsNullOrEmpty(actual) && actual.Equals("True"))
                {
                    ActPBControl PbAct = new ActPBControl();
                    PbAct.LocateBy = eLocateBy.ByXPath;
                    PbAct.ControlAction = ActPBControl.eControlAction.Click;
                    PbAct.LocateValueCalculated = @"/Script Error/[LocalizedControlType:title bar]/Close";
                    PbAct.Active = true;
                    PbAct.AddNewReturnParams = true;
                    mBF.CurrentActivity.Acts.Add(PbAct);
                    mBF.CurrentActivity.Acts.CurrentItem = PbAct;
                    mGR.RunAction(PbAct, false);


                    action = new ActPBControl();
                    action.LocateBy = eLocateBy.ByName;
                    action.ControlAction = ActPBControl.eControlAction.IsExist;
                    action.LocateValueCalculated = "Script Error";
                    action.AddNewReturnParams = true;
                    action.Timeout = 10;
                    action.Active = true;
                    mBF.CurrentActivity.Acts.Add(action);
                    mBF.CurrentActivity.Acts.CurrentItem = action;
                    //Act
                    mGR.RunAction(action, false);
                    actual = PbAct.GetReturnParam("Actual");
                }
            }
            else
            {
                Assert.AreEqual(action.Status, eRunStatus.Failed, "Action Status");
            }

            ActGenElement actGen = new ActGenElement();
            actGen.LocateBy = eLocateBy.ByName;
            actGen.GenElementAction = ActGenElement.eGenElementAction.Click;
            actGen.LocateValueCalculated = "lastName";
            actGen.Active = true;
            mBF.CurrentActivity.Acts.Add(actGen);
            mBF.CurrentActivity.Acts.CurrentItem = actGen;
            mGR.RunAction(actGen, false);
        }


        [TestMethod]
        public void GetURL_Testing()
        {
            ActBrowserElement act = new ActBrowserElement();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActBrowserElement.eControlAction.GetPageURL;            
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            string expected = @"file://" + TestResources.GetTestResourcesFolder("PBTestApp") + @"\Browser.html"; 

            Assert.AreEqual(actual, expected, "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }


        [Ignore]
        public void SwitchFrame_Testing()
        {
            ActBrowserElement act = new ActBrowserElement();
            act.LocateBy = eLocateBy.ByClassName;
            act.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            act.Value = "Internet Explorer_Server";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void SwitchToDefaultFrame_Testing()
        {
            ActBrowserElement act = new ActBrowserElement();
            act.LocateBy = eLocateBy.ByClassName;
            act.ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;
            act.Value = "Internet Explorer_Server";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetElementAttribute_Dropdown()
        {
            ActGenElement act = new ActGenElement();
            act.LocateBy = eLocateBy.ByID;
            act.GenElementAction = ActGenElement.eGenElementAction.GetElementAttributeValue;
            act.LocateValueCalculated = "accountSubType";
            act.AddNewReturnParams = true;
            act.Value = "Ginger";
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "RETAILEMPLOYEEDEMOTESTDEALER EMPLOYEEVIPBUSINESSHOUSE", "True");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }






    }
}
