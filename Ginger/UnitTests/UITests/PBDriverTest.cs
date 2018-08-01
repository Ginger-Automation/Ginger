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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;

namespace UnitTests.UITests.PBDriverTest
{
    //marked as Pending since it is not running when computer is in lock mode
    // can be reproduced by running UT and locking the machine

    [TestClass]
    public class PBDriverTest 
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
                mGR.CurrentSolution = new Ginger.Environments.Solution();
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

        #region Text box field
        [TestMethod]
        public void SetTextField_tb_lastname()
        {           
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;            
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.AddNewReturnParams = true;
            c.LocateValueCalculated = "sle_acc_text";         
            c.Value =  "Jenny";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c,false);


            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValueCalculated = "sle_acc_text";
            act.AddNewReturnParams = true;
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);


            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Jenny", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetTextField_tb_lastname()
        {
            //Arrange                        
            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "sle_acc_text";
            c.Value = "ABCDEF";
            mGR.RunAction(c);

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValueCalculated = "sle_acc_text";
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);

            //Assert
           Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            //Assert.AreEqual(act.ExInfo, txt, "ExInfo");
           Assert.AreEqual(act.Error, null, "Act.Error");
        }

        #endregion

        #region Windows
        [TestMethod]
        public void WindowVisualStateCheck()
        {

            // Window Maximize
            ActWindow actWinMax = new ActWindow();
            actWinMax.LocateBy = eLocateBy.ByTitle;
            actWinMax.LocateValueCalculated = "Simple Page";
            actWinMax.WindowActionType = ActWindow.eWindowActionType.Maximize;
            actWinMax.Active = true;
            
            mBF.CurrentActivity.Acts.Add(actWinMax);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMax;
            mGR.RunAction(actWinMax, false);

            //Assert
            Assert.AreEqual(actWinMax.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMax.Error, null, "Act.Error");

            // Window minimize
            ActWindow actWinMin = new ActWindow();
            actWinMin.LocateBy = eLocateBy.ByTitle;
            actWinMin.LocateValueCalculated = "Simple Page";
            actWinMin.WindowActionType = ActWindow.eWindowActionType.Minimize;
            actWinMin.Active = true;

            mBF.CurrentActivity.Acts.Add(actWinMin);
            mBF.CurrentActivity.Acts.CurrentItem = actWinMin;
            mGR.RunAction(actWinMin, false);

            //Assert
            Assert.AreEqual(actWinMin.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinMin.Error, null, "Act.Error");

            // Window Restore
            ActWindow actWinRes = new ActWindow();
            actWinRes.LocateBy = eLocateBy.ByTitle;
            actWinRes.LocateValueCalculated = "Simple Page";
            actWinRes.WindowActionType = ActWindow.eWindowActionType.Restore;
            actWinRes.Active = true;

            mBF.CurrentActivity.Acts.Add(actWinRes);
            mBF.CurrentActivity.Acts.CurrentItem = actWinRes;
            mGR.RunAction(actWinRes, false);

            //Assert
            Assert.AreEqual(actWinRes.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actWinRes.Error, null, "Act.Error");

        }
        #endregion
        #region Button

        [TestMethod]
        public void ClickButton_ByText()
        {
            
            // click click me button
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.Click;
            act.LocateValueCalculated = "Click Me";
            // act.LocateValueCalculated = "1021";
            act.Active = true;

            // Click OK on the msgbox
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "OK";
            c.Active = true;

            //Act

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);
         
            //Assert
           Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(act.Error, null, "Act.Error");
            //Assert.AreEqual(act.ExInfo, "", "act.ExInfo");

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

           Assert.AreEqual(c.Status, eRunStatus.Passed, "c.Status");
           Assert.AreEqual(c.Error, null, "c.Error");
        }

        #endregion

        #region CheckBox
        [TestMethod]
        public void SetCheckboxValue_ByName()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "Works in Amdocs";
            //c.ValueForDriver = "Checked";
            c.Value = "Checked";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);


            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValueCalculated = "Works in Amdocs";
            act.Active = true;
            act.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);

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
        public void GetCheckboxValue_ByName()
        {
            //Arrange      
            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "Works in Amdocs";
            c.Value = "Checked";
            c.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c,false);

            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValueCalculated = "Works in Amdocs";
            act.Active = true;
            act.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);

            //Assert
           Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
           Assert.AreEqual(actual, "Checked", "Ret Param Actual");
           Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void ToggleCheckboxValue_ByName()
        {//Arrange                        

            // Put value in the field
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "cbx_acc_check";
            c.ValueForDriver = "Checked";
            c.Value = "Checked";
            c.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c,false);  

            //toggle
            ActPBControl c1 = new ActPBControl();
            c1.LocateBy = eLocateBy.ByName;
            c1.ControlAction = ActPBControl.eControlAction.Toggle;
            c1.LocateValueCalculated = "cbx_acc_check";
            c1.AddNewReturnParams = true;
            c1.ItemName = "Toggle Checkbox cbx_acc_check";
            c1.Active = true;
            
            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;

            //Act
            mGR.RunAction(c1, false);


            // Prep Get Value Action
            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetValue;
            act.LocateValueCalculated = "cbx_acc_check";
            c1.ItemName = "Toggle Checkbox cbx_acc_check";
            act.AddNewReturnParams = true;
            act.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            mGR.RunAction(act, false);

            //Assert
           Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
           Assert.AreEqual(actual, "Unchecked", "Ret Param Actual");
           Assert.AreEqual(act.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsCheckboxEnabled_ByName()
        {
            //Arrange                        
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsEnabled;
            c.LocateValueCalculated = "Works in Amdocs";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.RunAction(c, false);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }

        #endregion

        #region radio button
        [TestMethod]
        public void SelectRadioButton_Bachelors()
        {
            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "Bachelors";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            ActPBControl act = new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.IsSelected;
            act.LocateValueCalculated = "Bachelors";
            act.AddNewReturnParams = true;
            act.Active = true;

            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);

            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");
            
            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetValueRadioButton_Bachelors()
        {
            ActPBControl c1 = new ActPBControl();
            c1.LocateBy = eLocateBy.ByName;
            c1.ControlAction = ActPBControl.eControlAction.Select;
            c1.LocateValueCalculated = "Bachelors";
            c1.Active = true;

            mBF.CurrentActivity.Acts.Add(c1);
            mBF.CurrentActivity.Acts.CurrentItem = c1;
            //Act
            mGR.RunAction(c1, false);
            
            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsSelected;
            c.LocateValueCalculated = "Bachelors";
            c.AddNewReturnParams = true;
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "True", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsSelectedRadioButton_Masters()
        {
            //Select the radio button
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "Masters";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.RunAction(c, false);


            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.IsSelected;
            c.LocateValueCalculated = "Masters";
            c.Active = true;

            mGR.RunAction(c, false);
            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region Scroll Bar

        #endregion

        #region Title Bar
        [TestMethod]
        public void GetTitleBarText_SimplePage()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByAutomationID;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "1010";
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;

            //Act
            mGR.RunAction(c, false);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");

        }

        [TestMethod]
        public void GetTextByName_name()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "name_t";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region combo box
        [TestMethod]
        public void SelectComboBoxItem_Indian()
        {
            
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "ddlb_acc_nationality";
            c.Value = "Indian";
            c.AddNewReturnParams = true;
            c.Active = true;

            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValueCalculated = "/[AutomationId:1021]";
            c.AddNewReturnParams = true;
            c.Active = true;

            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "Indian", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");

        }

        [TestMethod]
        public void GetSelectedItem_ComboBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "/[AutomationId:1021]";
            c.Value = "India";
            c.AddNewReturnParams = true;

            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValueCalculated = "/[AutomationId:1021]";
            c.AddNewReturnParams = true;
            c.Active = true;

            mGR.RunAction(c, false);
            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "Indian", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetAllItems_ComboBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValueCalculated = "/[AutomationId:1004]";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            //Assert
           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
           Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region List box
        [TestMethod]
        public void ClickListBoxItem_English()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "English";
            c.ItemName = "Select item English";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            ActPBControl act= new ActPBControl();
            act.LocateBy = eLocateBy.ByName;
            act.ControlAction = ActPBControl.eControlAction.GetSelected;
            act.LocateValue = "lb_acc_language";
            act.LocateValueCalculated = "lb_acc_language";
            act.AddNewReturnParams = true;
            act.Active = true;
            mBF.CurrentActivity.Acts.Add(act);
            mBF.CurrentActivity.Acts.CurrentItem = act;
            //Act
            mGR.RunAction(act, false);

            //Assert
            Assert.AreEqual(act.Status, eRunStatus.Passed, "Action Status");
            string actual = act.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "English", "Ret Param Actual");
            Assert.AreEqual(act.Error, null, "Act.Error");
        }

                     
        [TestMethod]
        public void GetSelectedItems_ListBox()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "English";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);
             
            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValueCalculated = "lb_acc_language";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);
            //Assert

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "English", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }


        [TestMethod]
        public void GetAllItems_ListBox()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "lb_acc_language";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

           Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Vertical, English, French, Hebrew, Hindi, Russian, Spanish", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        #endregion

        #region Dialog control
        [TestMethod]
        public void GetDialogTitle_ClickMeButton()
        {

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetTitle;
            c.LocateValueCalculated = "MsgBox Test";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Dialog Title");
            Assert.AreEqual(actual, "MsgBox Test", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
            
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "OK";
            c.Active = true;
            //Act
            mGR.RunAction(c, false);
        }

        [TestMethod]
        public void GetDialogText_ClickMeButton()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "MsgBox Test";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Button is Clicked", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");

            //To Handle click me button dialog 
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "OK";
            c.Active = true;
            //Act
            mGR.RunAction(c, false);
        }
        #endregion

        #region Menu bar
        [TestMethod]
        public void GetAllMenuBarElements()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "Application";
            c.AddNewReturnParams = true;
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "File", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }
        
        #endregion

        #region XPath locator
        [TestMethod]
        public void SetTextBoxValue_ByXpath()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "sle_acc_text";
            c.Value = "Hello From Xpath";
            c.AddNewReturnParams = true;
            c.Active = true;

            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "sle_acc_text";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Hello From Xpath", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetTextBoxValue_ByXpath()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "sle_acc_text";
            c.Value = "Hello From Xpath";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "[AutomationId:1026]";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Hello From Xpath", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetTextBoxValue_ByXpathWithValueProperty()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.SetValue;
            c.LocateValueCalculated = "sle_acc_text";
            c.Value = "Jinendra";
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetValue;
            c.LocateValueCalculated = "[Value:Jinendra]";
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            //Assert
            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Jinendra", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
        public void SelectCountry_ByXpathWithMultipleProperty()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.Select;
            c.LocateValueCalculated = "/[[AutomationId:1006][Name:]]";
            c.Value = "India";
            c.AddNewReturnParams = true;            
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);
            
            c = new ActPBControl();

            c.LocateBy = eLocateBy.ByXPath;
            c.ControlAction = ActPBControl.eControlAction.GetSelected;
            c.LocateValueCalculated = "/[[AutomationId:1006][Name:]]";            
            c.AddNewReturnParams = true;
            c.Active = true;
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            //Act
            mGR.RunAction(c, false);

            Assert.AreEqual(c.Status, eRunStatus.Passed, "Action Status");
            string actual = c.GetReturnParam("Selected Item");
            Assert.AreEqual(actual, "India", "Ret Param Actual");
            Assert.AreEqual(c.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;            
            mGR.RunAction(actGrid, false);
           Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");            
            string actual = actGrid.GetReturnParam("Actual");
           Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
           Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

         
         [TestMethod]
        public void SetText_TableActionGrid()
        {
            ActTableElement actGrid1 = new ActTableElement();
            actGrid1.ByRandRow = false;
            actGrid1.BySelectedRow = false;
            actGrid1.ByWhere = true;
            actGrid1.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColTitle;
            actGrid1.ControlAction = ActTableElement.eTableAction.SetValue;
            actGrid1.LocateColTitle = "address";

            actGrid1.WhereColumnTitle="name";
            actGrid1.WhereColumnValue = "Ravi";
            actGrid1.LocateRowType = "Where";
            actGrid1.LocateRowValue = "";
            actGrid1.RunActionOn = ActTableElement.eRunActionOn.OnCellRowNumColNum;
            actGrid1.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            
            actGrid1.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actGrid1.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actGrid1.LocateValueCalculated = "dw_acc_grd";
            actGrid1.LocateBy = eLocateBy.ByName;
            actGrid1.Value = "3909 Inverness Rd";
            actGrid1.Active = true;
            
            mBF.CurrentActivity.Acts.Add(actGrid1);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid1;
            //Act
            mGR.RunAction(actGrid1, false);

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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.AddNewReturnParams = true;
            actGrid.Active = true;

            mBF.CurrentActivity.Acts.Add(actGrid);

            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            //Act
            mGR.RunAction(actGrid, false);

            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual("3909 Inverness Rd",actual, "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
             actGrid.LocateValueCalculated = "dw_acc_grd";
             actGrid.LocateBy = eLocateBy.ByName;
             actGrid.Value = "Amdocs - Pune, India";
             actGrid.Active = true;


             mBF.CurrentActivity.Acts.Add(actGrid);

             mBF.CurrentActivity.Acts.CurrentItem = actGrid;
             //Act
             mGR.RunAction(actGrid, false);
            //Assert
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }
        #endregion

        #region Table Action on Grid
        //For Xpath with only name property
        [TestMethod]
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
            actGrid.LocateValueCalculated = "none";
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

            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual("Ravi",actual, "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "none";
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

            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Ravi", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "none";
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

            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            if(actual!=null)
            {
                actual = "pass";
            }
            Assert.AreEqual(actual, "pass", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "none";
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

            mGR.RunAction(actGrid, false);
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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }


        [TestMethod]
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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "dw_acc_grd";
            actGrid.LocateBy = eLocateBy.ByName;
            actGrid.Active = true;
            mBF.CurrentActivity.Acts.Add(actGrid);
            mBF.CurrentActivity.Acts.CurrentItem = actGrid;
            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "2109 Fox Dr, Champaign IL ", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        [TestMethod]
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
            actGrid.LocateValueCalculated = "none";
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

            mGR.RunAction(actGrid, false);
            Assert.AreEqual(actGrid.Status, eRunStatus.Passed, "Action Status");
            string actual = actGrid.GetReturnParam("Actual");
            Assert.AreEqual(actual, "3", "Ret Param Actual");
            Assert.AreEqual(actGrid.Error, null, "Act.Error");
        }

        #endregion

        #region UIElement Action- Click and Validate
        [TestMethod]
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

            mGR.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
        public void TestClickAndValidate_NotExist()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

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
            mGR.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
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

            mGR.RunAction(actUI, false);

            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "Exit";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);


            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }

        [TestMethod]
        public void TestClickAndValidate_LoopThroughClicks()
        {
            ActPBControl c = new ActPBControl();
            c.LocateBy = eLocateBy.ByName;
            c.ControlAction = ActPBControl.eControlAction.Click;
            c.LocateValueCalculated = "Click Me";
            c.Active = true;
            //Act
            mBF.CurrentActivity.Acts.Add(c);
            mBF.CurrentActivity.Acts.CurrentItem = c;
            mGR.RunAction(c, false);

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

            mGR.RunAction(actUI, false);
            Assert.AreEqual(actUI.Status, eRunStatus.Passed, "Action Status");
            string actual = actUI.GetReturnParam("Actual");
            Assert.AreEqual(actual, "Clicked Successfully using Invoke Pattern", "Ret Param Actual");
            Assert.AreEqual(actUI.Error, null, "Act.Error");
        }
        #endregion
    }
}
