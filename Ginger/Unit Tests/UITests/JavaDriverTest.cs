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

using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using System;
using System.Linq;
using GingerCore.Drivers.CommunicationProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions.Common;
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using GingerTestHelper;
using Amdocs.Ginger.Common.UIElement;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace UnitTests.UITests.JavaDriverTest
{
    [TestClass]
    [Level3]
    
    public class JavaDriverTest 
    {
        static BusinessFlow mBF;

        // make it static for reuse so no need to init every time when running test by click test button
        static JavaDriver mDriver = null;
        static GingerRunner mGR = null;

        // added to generate random strings to avoid use of hard-coded strings in test cases
        private static string RandomString(int length)
        {
            Random random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
        
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            if (mGR == null)
            {                

                mGR = new GingerRunner();
                mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
                mBF = new BusinessFlow();
                mBF.Activities = new ObservableList<Activity>();
                mBF.Name = "BF Test Java Driver";
                Platform p = new Platform();
                p.PlatformType = ePlatformType.Java;
                mBF.Platforms = new ObservableList<Platform>();
                mBF.Platforms.Add(p);
                mBF.TargetApplications.Add(new TargetApplication() { AppName = "JavaTestApp" });
                Activity activity = new Activity();
                activity.TargetApplication = "JavaTestApp";
                mBF.Activities.Add(activity);
                mBF.CurrentActivity = activity;

                ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
                LJA.LaunchJavaApplication = true;
                LJA.LaunchWithAgent = true;
                LJA.WaitForWindowTitle = "Java";
                LJA.AddOrUpdateInputParamValue(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.Manual.ToString());
                LJA.Port ="9898";
                LJA.URL = TestResources.GetTestResourcesFile(@"JavaTestApp\JavaTestApp.jar");
                activity.Acts.Add(LJA);
                LJA.Execute();

                mDriver = new JavaDriver(mBF);
                mDriver.JavaAgentHost = "127.0.0.1";
                mDriver.JavaAgentPort =9898;
                mDriver.CommandTimeout = 120;
                mDriver.cancelAgentLoading = false;
                mDriver.DriverLoadWaitingTime = 30;
                mDriver.ImplicitWait = 30;
                mDriver.StartDriver();
                Agent a = new Agent();
                a.Active = true;
                a.DriverType = Agent.eDriverType.JavaDriver;
                
                a.Name = "Java Agent";
                a.Driver = mDriver;

                mGR.SolutionAgents = new ObservableList<Agent>();
                mGR.SolutionAgents.Add(a);

                ApplicationAgent AA = new ApplicationAgent();
                AA.AppName = "JavaTestApp";
                AA.Agent = a;
                mBF.CurrentActivity.CurrentAgent = a;
                mGR.ApplicationAgents.Add(AA);
                mGR.CurrentBusinessFlow = mBF;

                mGR.SetCurrentActivityAgent();

                GingerCore.Drivers.CommunicationProtocol.PayLoad PL = new GingerCore.Drivers.CommunicationProtocol.PayLoad("SwitchWindow");
                // PL.AddValue("ByTitle");
                PL.AddValue("Java Swing Test App");
                PL.ClosePackage();
                GingerCore.Drivers.CommunicationProtocol.PayLoad RC = mDriver.Send(PL);                
            }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            ActWindow AWC = new ActWindow();
            AWC.LocateBy = eLocateBy.ByTitle;
            AWC.LocateValue = "Java";
            AWC.WindowActionType = ActWindow.eWindowActionType.Close;
            mGR.RunAction(AWC,false);
            mGR.StopAgents();
            // mDriver.CloseDriver();
            mDriver = null;
            mGR = null;            
        }

        /**
         * TextField Test Cases  
        */
        [TestMethod]
        public void SetTextFieldValue()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "txtEmployeeID";
            a.ControlAction = ActJavaElement.eControlAction.SetValue;
            a.Value = RandomString(5);
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;

            mGR.RunAction(a, false);
            //TODO: Find a better way to get ExInfo.
            String ExInfo = a.ExInfo.Substring(a.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
            Assert.AreEqual(a.Error, null, "Act.Error");
            Assert.AreEqual(ExInfo, "Text Field Value Set to - " + a.Value, "ExInfo");          
        }

        [TestMethod]
        public void GetTextFieldValue()
        {
            PayLoad plText = new PayLoad("ElementAction");
            string setText = RandomString(5);
            plText.AddValue(ActJavaElement.eControlAction.SetValue.ToString());
            plText.AddEnumValue(ActJavaElement.eWaitForIdle.None);
            plText.AddValue(eLocateBy.ByName.ToString());
            plText.AddValue("txtEmployeeID");
            plText.AddValue(setText);
            plText.ClosePackage();
            mDriver.Send(plText);

            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "txtEmployeeID";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            a.AddNewReturnParams = true;

            mGR.RunAction(a, false);
           
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, setText, "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsTextFieldEnabled()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "txtEmployeeID";
            a.ControlAction = ActJavaElement.eControlAction.IsEnabled;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            a.AddNewReturnParams = true;

            mGR.RunAction(a, false);

            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }
       
        [TestMethod]
        public void AsyncDialogClickandDismiss()
        {
            ActJavaElement asyncClickAction = new ActJavaElement();
            asyncClickAction.LocateBy = eLocateBy.ByName;
            asyncClickAction.LocateValueCalculated = "btnSubmit";
            asyncClickAction.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            asyncClickAction.Active = true;
            mBF.CurrentActivity.Acts.Add(asyncClickAction);
            mBF.CurrentActivity.Acts.CurrentItem = asyncClickAction;

            mGR.RunAction(asyncClickAction, false);

            //TODO: Find a better way to get ExInfo.
            String ExInfo = asyncClickAction.ExInfo.Substring(asyncClickAction.ExInfo.LastIndexOf("M -") + 4);
                        
            Assert.AreEqual(ExInfo, "Click Activity Passed", "ExInfo");
            Assert.AreEqual(asyncClickAction.Error, null, "Act.Error");

            System.Threading.Thread.Sleep(500);
            ActJavaElement acceptdialogAction = new ActJavaElement();
            acceptdialogAction.LocateBy = eLocateBy.ByTitle;
            acceptdialogAction.LocateValueCalculated = "Message";
            acceptdialogAction.ControlAction = ActJavaElement.eControlAction.AcceptDialog;
            acceptdialogAction.Active = true;
            mBF.CurrentActivity.Acts.Add(acceptdialogAction);
            mBF.CurrentActivity.Acts.CurrentItem = acceptdialogAction;

            mGR.RunAction(acceptdialogAction, false);

            Assert.AreEqual(eRunStatus.Passed, acceptdialogAction.Status, "Action Status");
            Assert.AreEqual(acceptdialogAction.Error, null, "Act.Error");            
        }      
        
        [TestMethod]
        public void GetDialogText()
        {
            //*************Create Dialog*********************//
            ActJavaElement actClickSubmitBtn = new ActJavaElement();
            actClickSubmitBtn.LocateBy = eLocateBy.ByName;
            actClickSubmitBtn.LocateValueCalculated = "btnSubmit";
            actClickSubmitBtn.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            actClickSubmitBtn.Active = true;
            mBF.CurrentActivity.Acts.Add(actClickSubmitBtn);
            mBF.CurrentActivity.Acts.CurrentItem = actClickSubmitBtn;            
            mGR.RunAction(actClickSubmitBtn, false);

            // Remove Timestamp from ExInfo
            //TODO: Find a better way to get ExInfo.
            String ExInfo = actClickSubmitBtn.ExInfo.Substring(actClickSubmitBtn.ExInfo.LastIndexOf("M -") + 4);

            Assert.AreEqual(ExInfo, "Click Activity Passed", "ExInfo");
            Assert.AreEqual(actClickSubmitBtn.Error, null, "Act.Error");

            System.Threading.Thread.Sleep(500);

            //*************Get a Dialog Text*********************//
            ActJavaElement actGetDialogText = new ActJavaElement();
            actGetDialogText.LocateBy = eLocateBy.ByTitle;
            actGetDialogText.LocateValueCalculated = "Message";          
            actGetDialogText.ControlAction = ActJavaElement.eControlAction.GetDialogText;
            actGetDialogText.AddNewReturnParams = true;
            actGetDialogText.Active = true;
            mBF.CurrentActivity.Acts.Add(actGetDialogText);
            mBF.CurrentActivity.Acts.CurrentItem = actGetDialogText;            
            mGR.RunAction(actGetDialogText, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actGetDialogText.Status, "Action Status");
            Assert.AreEqual(actGetDialogText.ActReturnValues.FirstOrDefault().Actual, "Confirm submit Order: 12345A", "ExInfo");
            Assert.AreEqual(actGetDialogText.Error, null, "Act.Error");

            //*************Close Dialog*********************//
            ActJavaElement actAcceptDialog = new ActJavaElement();
            actAcceptDialog.LocateBy = eLocateBy.ByTitle;
            actAcceptDialog.LocateValueCalculated = "Message";
            actAcceptDialog.ControlAction = ActJavaElement.eControlAction.AcceptDialog;
            actAcceptDialog.Active = true;
            mBF.CurrentActivity.Acts.Add(actAcceptDialog);
            mBF.CurrentActivity.Acts.CurrentItem = actAcceptDialog;

            mGR.RunAction(actAcceptDialog, false);

            Assert.AreEqual(eRunStatus.Passed, actAcceptDialog.Status, "Action Status");
            Assert.AreEqual(actAcceptDialog.Error, null, "Act.Error");
        }

        [TestMethod]
        public void AcceptDialog()
        {
            //*************Create Dialog*********************//
            ActJavaElement actClickSubmitBtn = new ActJavaElement();
            actClickSubmitBtn.LocateBy = eLocateBy.ByName;
            actClickSubmitBtn.LocateValueCalculated = "btnSubmit";
            actClickSubmitBtn.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            actClickSubmitBtn.Active = true;
            mBF.CurrentActivity.Acts.Add(actClickSubmitBtn);
            mBF.CurrentActivity.Acts.CurrentItem = actClickSubmitBtn;
            mGR.RunAction(actClickSubmitBtn, false);

            //*************Accept Dialog*********************//
            ActJavaElement actAcceptDialog = new ActJavaElement();
            actAcceptDialog.LocateBy = eLocateBy.ByTitle;
            actAcceptDialog.LocateValueCalculated = "Message";
            actAcceptDialog.ControlAction = ActJavaElement.eControlAction.AcceptDialog;
            actAcceptDialog.Active = true;
            mBF.CurrentActivity.Acts.Add(actAcceptDialog);
            mBF.CurrentActivity.Acts.CurrentItem = actAcceptDialog;

            mGR.RunAction(actAcceptDialog, false);

            Assert.AreEqual(eRunStatus.Passed, actAcceptDialog.Status, "Action Status");
            Assert.AreEqual(actAcceptDialog.Error, null, "Act.Error");
        }

        [TestMethod]
        public void SelectComponentByXPathInDialog()
        {

            PayLoad PLSwitch = new PayLoad("SwitchWindow");
            PLSwitch.AddValue("Java Swing Test App");
            PLSwitch.ClosePackage();
            PayLoad RCSwitch = mDriver.Send(PLSwitch);

            ActJavaElement actSelectTab = new ActJavaElement();
            actSelectTab.LocateBy = eLocateBy.ByName;
            actSelectTab.LocateValueCalculated = "TabbedPane1";
            actSelectTab.ControlAction = ActJavaElement.eControlAction.Select;
            actSelectTab.Value = "tab1";

            actSelectTab.Active = true;
            mBF.CurrentActivity.Acts.Add(actSelectTab);
            mBF.CurrentActivity.Acts.CurrentItem = actSelectTab;
            //Act
            mGR.RunAction(actSelectTab, false);

            string xpath = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JOptionPane[0]/OptionPane.buttonArea/[[Name:OptionPane.button][ClassName:javax.swing.JButton][Value:No]]";
            //*************Create Dialog*********************//
            ActJavaElement actClickTabBtn = new ActJavaElement();
            actClickTabBtn.LocateBy = eLocateBy.ByName;
            actClickTabBtn.LocateValueCalculated = "btnClickTab1";
            actClickTabBtn.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            actClickTabBtn.Active = true;
            mBF.CurrentActivity.Acts.Add(actClickTabBtn);
            mBF.CurrentActivity.Acts.CurrentItem = actClickTabBtn;
            mGR.RunAction(actClickTabBtn, false);
            //*************Switch window to Dialog*********************
            GingerCore.Drivers.CommunicationProtocol.PayLoad PL = new GingerCore.Drivers.CommunicationProtocol.PayLoad("SwitchWindow");
            // PL.AddValue("ByTitle");
            PL.AddValue("Choose");
            PL.ClosePackage();
            GingerCore.Drivers.CommunicationProtocol.PayLoad RC = mDriver.Send(PL);
            //*************Highlight No Dialog Button*********************
            PayLoad Request = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());
            Request.AddEnumValue(JavaDriver.WindowExplorerOperationType.Highlight);
            Request.AddValue("ByXPath");
            Request.AddValue(xpath); 
            Request.ClosePackage();
            PayLoad Response = mDriver.Send(Request);
            //*************Choose No Button Dialog(Yes\No)*********************//
            ActJavaElement actChoosetDialog = new ActJavaElement();
            actChoosetDialog.LocateBy = eLocateBy.ByXPath;
            actChoosetDialog.LocateValueCalculated = xpath;
            actChoosetDialog.ControlAction = ActJavaElement.eControlAction.Click;
            actChoosetDialog.Active = true;
            mBF.CurrentActivity.Acts.Add(actChoosetDialog);
            mBF.CurrentActivity.Acts.CurrentItem = actChoosetDialog;

            mGR.RunAction(actChoosetDialog, false);

            Assert.AreEqual(eRunStatus.Passed, actChoosetDialog.Status, "Action Status");
            Assert.AreEqual(actChoosetDialog.Error, null, "Act.Error");


             PLSwitch = new PayLoad("SwitchWindow");
            // PL.AddValue("ByTitle");
            PLSwitch.AddValue("Java Swing Test App");
            PLSwitch.ClosePackage();
             RCSwitch = mDriver.Send(PLSwitch);
        }

        [TestMethod]
        public void SelectComponentByNameIndexInDialog()
        {
            PayLoad PLSwitch = new PayLoad("SwitchWindow");
            PLSwitch.AddValue("Java Swing Test App");
            PLSwitch.ClosePackage();
            PayLoad RCSwitch = mDriver.Send(PLSwitch);

            string nameIndex = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JOptionPane[0]/OptionPane.buttonArea/Name:OptionPane.button[1]";
            //*************Create Dialog*********************//
            ActJavaElement actClickTabBtn = new ActJavaElement();
            actClickTabBtn.LocateBy = eLocateBy.ByName;
            actClickTabBtn.LocateValueCalculated = "btnClickTab1";
            actClickTabBtn.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            actClickTabBtn.Active = true;
            mBF.CurrentActivity.Acts.Add(actClickTabBtn);
            mBF.CurrentActivity.Acts.CurrentItem = actClickTabBtn;
            mGR.RunAction(actClickTabBtn, false);
            //*************Switch window to Dialog*********************
            GingerCore.Drivers.CommunicationProtocol.PayLoad PL = new GingerCore.Drivers.CommunicationProtocol.PayLoad("SwitchWindow");
            // PL.AddValue("ByTitle");
            PL.AddValue("Choose");
            PL.ClosePackage();
            GingerCore.Drivers.CommunicationProtocol.PayLoad RC = mDriver.Send(PL);
            //*************Highlight No Dialog Button*********************
            PayLoad Request = new PayLoad(JavaDriver.CommandType.WindowExplorerOperation.ToString());                                                                                          
            Request.AddEnumValue(JavaDriver.WindowExplorerOperationType.Highlight);
            Request.AddValue("ByXPath");
            Request.AddValue(nameIndex);
            Request.ClosePackage();
            PayLoad Response = mDriver.Send(Request);
            //*************Choose No Button Dialog(Yes\No)*********************//
            ActJavaElement actChoosetDialog = new ActJavaElement();
            actChoosetDialog.LocateBy = eLocateBy.ByXPath;
            actChoosetDialog.LocateValueCalculated = nameIndex;
            actChoosetDialog.ControlAction = ActJavaElement.eControlAction.Click;
            actChoosetDialog.Active = true;
            mBF.CurrentActivity.Acts.Add(actChoosetDialog);
            mBF.CurrentActivity.Acts.CurrentItem = actChoosetDialog;

            mGR.RunAction(actChoosetDialog, false);

            Assert.AreEqual(eRunStatus.Passed, actChoosetDialog.Status, "Action Status");
            Assert.AreEqual(actChoosetDialog.Error, null, "Act.Error");

             PLSwitch = new PayLoad("SwitchWindow");
            PLSwitch.AddValue("Java Swing Test App");
            PLSwitch.ClosePackage();
             RCSwitch = mDriver.Send(PLSwitch);
        }

        /**
         * TreeNode Test Cases  
        */
        [TestMethod]
        public void GetValueTreeNode()
        {
            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.Click.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.AddValue("Canada");
            PLClick.ClosePackage();
            mDriver.Send(PLClick);

            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.GetValue;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            mGR.RunAction(actJavaElement, false);
            //TODO: Find a better way to get ExInfo.
            String ExInfo = actJavaElement.ExInfo.Substring(actJavaElement.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");
            Assert.AreEqual(actJavaElement.ActReturnValues.FirstOrDefault().Actual, "Canada", "ExInfo");
            Assert.AreEqual(actJavaElement.Error, null, "Act.Error");
        }

        [TestMethod]
        public void DoubleClickTreeNode()
        {
            ActJavaElement doubleClickAction = new ActJavaElement();
            doubleClickAction.LocateBy = eLocateBy.ByName;
            doubleClickAction.ControlAction = ActJavaElement.eControlAction.DoubleClick;
            doubleClickAction.Active = true;
            doubleClickAction.LocateValueCalculated = "countriesTree";
            doubleClickAction.Value = "US";
            mBF.CurrentActivity.Acts.Add(doubleClickAction);
            mBF.CurrentActivity.Acts.CurrentItem = doubleClickAction;

            mGR.RunAction(doubleClickAction, false);
            //TODO: Find a better way to get ExInfo.
            String ExInfo = doubleClickAction.ExInfo.Substring(doubleClickAction.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed, doubleClickAction.Status, "Action Status");
            Assert.AreEqual(ExInfo, "Click Activity Passed", "ExInfo");
            Assert.AreEqual(doubleClickAction.Error, null, "Act.Error");
        }

        /**
         * Menu Item Test Cases  
        */
        [TestMethod]
        public void ClickMenu()
        {
            ActJavaElement menuClickAction = new ActJavaElement();
            menuClickAction.LocateBy = eLocateBy.ByName;
            menuClickAction.LocateValueCalculated = "fileMenu";
            menuClickAction.Value = "";
            menuClickAction.ControlAction = ActJavaElement.eControlAction.Click;
            menuClickAction.Active = true;
            mBF.CurrentActivity.Acts.Add(menuClickAction);
            mBF.CurrentActivity.Acts.CurrentItem = menuClickAction;

            mGR.RunAction(menuClickAction, false);

            Assert.AreEqual(eRunStatus.Passed, menuClickAction.Status, "Action Status");
            Assert.AreEqual(menuClickAction.Error, null, "Act.Error");
             
            System.Threading.Thread.Sleep(100);

            ActJavaElement submenuClickAction_1 = new ActJavaElement();
            submenuClickAction_1.LocateBy = eLocateBy.ByName;
            submenuClickAction_1.LocateValueCalculated = "newMenuItem";
            submenuClickAction_1.Value = "";
            submenuClickAction_1.ControlAction = ActJavaElement.eControlAction.Click;
            submenuClickAction_1.Active = true;
            mBF.CurrentActivity.Acts.Add(submenuClickAction_1);
            mBF.CurrentActivity.Acts.CurrentItem = submenuClickAction_1;

            mGR.RunAction(submenuClickAction_1, false);

            Assert.AreEqual(submenuClickAction_1.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(submenuClickAction_1.Error, null, "Act.Error");

            System.Threading.Thread.Sleep(100);

            // Click Document SubMenu;
            ActJavaElement submenuClickAction_2 = new ActJavaElement();           
            submenuClickAction_2.LocateBy = eLocateBy.ByName;
            submenuClickAction_2.LocateValueCalculated = "documentSubMenuItem";
            submenuClickAction_2.Value = "";
            submenuClickAction_2.ControlAction = ActJavaElement.eControlAction.AsyncClick;
            submenuClickAction_2.Active = true;
            mBF.CurrentActivity.Acts.Add(submenuClickAction_2);
            mBF.CurrentActivity.Acts.CurrentItem = submenuClickAction_2;

            mGR.RunAction(submenuClickAction_2, false);

            Assert.AreEqual(submenuClickAction_2.Status, eRunStatus.Passed, "Action Status");
            Assert.AreEqual(submenuClickAction_2.Error, null, "Act.Error");

            System.Threading.Thread.Sleep(100);

            //*************Dismiss Dialog*********************//
            ActJavaElement actDismissDialog = new ActJavaElement();
            actDismissDialog.LocateBy = eLocateBy.ByTitle;
            actDismissDialog.LocateValueCalculated = "Menu Click Info";
            actDismissDialog.ControlAction = ActJavaElement.eControlAction.DismissDialog;
            actDismissDialog.Active = true;
            mBF.CurrentActivity.Acts.Add(actDismissDialog);
            mBF.CurrentActivity.Acts.CurrentItem = actDismissDialog;

            mGR.RunAction(actDismissDialog, false);

            Assert.AreEqual(eRunStatus.Passed, actDismissDialog.Status, "Action Status");
            Assert.AreEqual(actDismissDialog.Error, null, "Act.Error");
        }
        
        [TestMethod]
        public void GetButtonText()
        {

            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "btnSubmit";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;

            a.AddNewReturnParams = true;

            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Submit", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsButtonEnabled()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "btnSubmit";
            a.ControlAction = ActJavaElement.eControlAction.IsEnabled;

            a.AddNewReturnParams = true;

            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void ToggleCheckBox()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "cbWorksAtAmdocs";
            a.ControlAction = ActJavaElement.eControlAction.Toggle;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IfCheckboxChecked()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "cbWorksAtAmdocs";
            a.ControlAction = ActJavaElement.eControlAction.IsChecked;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            mGR.RunAction(a, false);
            //Assert
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
        }

        [TestMethod]
        public void GetCheckBoxValue()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "cbWorksAtAmdocs";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;
            a.AddNewReturnParams = true;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Works at Amdocs", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsCheckBoxEnabled()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "cbWorksAtAmdocs";
            a.ControlAction = ActJavaElement.eControlAction.IsEnabled;
            a.AddNewReturnParams = true;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }        

        [TestMethod]
        public void SelectRadioButton()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "rbBachelor";
            a.ControlAction = ActJavaElement.eControlAction.Select;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);
            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetRadioButtonValue()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "rbBachelor";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;
            a.AddNewReturnParams = true;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Bachelor", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void IsRadioButtonSelected()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "rbBachelor";
            a.ControlAction = ActJavaElement.eControlAction.IsEnabled;
            a.AddNewReturnParams = true;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void SelectComboBoxValue()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "Country";
            a.Value = "India";
            a.ControlAction = ActJavaElement.eControlAction.Select;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);
            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetComboBoxSelectedValue()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "Country";
            a.Value = "India";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;
            a.AddNewReturnParams = true;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);
            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "India", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        public void SetTabSelected()
        {
            ActJavaElement actSelectTab = new ActJavaElement();
            actSelectTab.LocateBy = eLocateBy.ByName;
            actSelectTab.LocateValueCalculated = "TabbedPane1";
            actSelectTab.ControlAction = ActJavaElement.eControlAction.Select;
            actSelectTab.Value = "tab2";
            actSelectTab.Active = true;
            mBF.CurrentActivity.Acts.Add(actSelectTab);
            mBF.CurrentActivity.Acts.CurrentItem = actSelectTab;
            //Act
            mGR.RunAction(actSelectTab, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,actSelectTab.Status, "Action Status");           
           Assert.AreEqual(actSelectTab.Error, null, "Act.Error");
        }
        
        [TestMethod]
        public void GetSelectedTabText()
        {
            ActJavaElement actSelectTab = new ActJavaElement();
            actSelectTab.LocateBy = eLocateBy.ByName;
            actSelectTab.LocateValueCalculated = "TabbedPane1";
            actSelectTab.ControlAction = ActJavaElement.eControlAction.Select;
            actSelectTab.Value = "tab2";
            actSelectTab.Active = true;
            mBF.CurrentActivity.Acts.Add(actSelectTab);
            mBF.CurrentActivity.Acts.CurrentItem = actSelectTab;
            //Act
            mGR.RunAction(actSelectTab, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSelectTab.Status, "Action Status");
            Assert.AreEqual(actSelectTab.Error, null, "Act.Error");

            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "TabbedPane1";
            a.ControlAction = ActJavaElement.eControlAction.GetValue;
            // a.Value = "98";
            //a.ValueForDriver = "ABC";
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            a.AddNewReturnParams = true;
            //Act
            mGR.RunAction(a, false);

            //Assert
           Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "tab2", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }
        
        [TestMethod]
        public void ClickButton()
        {
            ActJavaElement a = new ActJavaElement();
            a.LocateBy = eLocateBy.ByName;
            a.LocateValueCalculated = "btnClickMe5";
            a.ControlAction = ActJavaElement.eControlAction.Click;
            a.Active = true;
            mBF.CurrentActivity.Acts.Add(a);
            mBF.CurrentActivity.Acts.CurrentItem = a;
            //Act
            mGR.RunAction(a, false);

            //Assert
            //TODO: Find a better way to get ExInfo.
            String ExInfo = a.ExInfo.Substring(a.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed,a.Status, "Action Status");
           Assert.AreEqual(ExInfo, "Click Activity Passed", "ExInfo");
           Assert.AreEqual(a.Error, null, "Act.Error");
        }
        

        [TestMethod]
        public void SetValueInTableCell()
        {
            PayLoad PLTable = new PayLoad("TableAction");
            List<String> Locators = new List<string>();
            PLTable.AddValue(ActTableElement.eTableAction.DoubleClick.ToString());
            PLTable.AddValue(eLocateBy.ByXPath.ToString());
            PLTable.AddValue("/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]");
            PLTable.AddValue("");
            Locators.Add("Row Number");
            Locators.Add("1");
            Locators.Add(ActTableElement.eRunColSelectorValue.ColNum.ToString());
            Locators.Add("1");
            PLTable.AddValue(Locators);
            PLTable.ClosePackage();
            mDriver.Send(PLTable);

            ActTableElement actTableElement = new ActTableElement();
            actTableElement.ControlAction = ActTableElement.eTableAction.SetValue;
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ByRowNum = true;
            actTableElement.Value = RandomString(5);
            actTableElement.ValueForDriver = RandomString(5);
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "1";


            actTableElement.WhereColSelector = ActTableElement.eRunColSelectorValue.ColTitle;
            actTableElement.WhereColumnTitle = null;
            actTableElement.WhereColumnValue = null;
            actTableElement.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actTableElement.WhereProperty = ActTableElement.eRunColPropertyValue.Value;

            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            mGR.RunAction(actTableElement, false);
            //TODO: Find a better way to get ExInfo.
            //Notice: [IndexOf("-")+2] and [LastIndexOf("-")+2] not work with a different format of Date and String(in case we have additional '-'). 
            String ExInfo = actTableElement.ExInfo.Substring(actTableElement.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(ExInfo, "Text Field Value Set to - " + actTableElement.Value, "ExInfo");
        }

        [TestMethod]
        public void ActivateRowInTable()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "1";
            actTableElement.ByRowNum = true;
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ControlAction = ActTableElement.eTableAction.ActivateRow;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
        }

        [TestMethod]
        public void GetValueFromTable()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "0";
            actTableElement.ByRowNum = true;
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ControlAction = ActTableElement.eTableAction.GetValue;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(actTableElement.ActReturnValues.FirstOrDefault().Actual, "2", "ExInfo");
        }

        [TestMethod]
        public void GetRowCountFromTable()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "1";
            actTableElement.ByRowNum = true;
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ControlAction = ActTableElement.eTableAction.GetRowCount;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(actTableElement.ActReturnValues.FirstOrDefault().Actual, "4", "ExInfo");
        }

        [TestMethod]
        public void CheckIsCellVisibleInTable()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "1";
            actTableElement.ByRowNum = true;
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ControlAction = ActTableElement.eTableAction.IsVisible;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(actTableElement.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
        }

        [TestMethod]
        public void CheckIsCellEnabledInTable()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "1";
            actTableElement.ByRowNum = true;
            actTableElement.LocateRowType = "Row Number";
            actTableElement.LocateRowValue = "1";
            actTableElement.ControlAction = ActTableElement.eTableAction.IsCellEnabled;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(actTableElement.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
        }

        [TestMethod]
        public void GetValueFromTableUsingWhereClause()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "0";
            actTableElement.ByWhere = true;
            actTableElement.LocateRowType = "Where";
            actTableElement.LocateRowValue = "";
            actTableElement.WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.WhereColumnTitle = "1";
            actTableElement.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actTableElement.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actTableElement.WhereColumnValue = "Jinendra";
            actTableElement.ControlAction = ActTableElement.eTableAction.GetValue;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;
            actTableElement.AddNewReturnParams = true;

            mGR.RunAction(actTableElement, false);

            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(actTableElement.ActReturnValues.FirstOrDefault().Actual, "1", "ExInfo");
        }

        [TestMethod]
        public void DoubleClickOnTableCell()
        {
            ActTableElement actTableElement = new ActTableElement();
            actTableElement.LocateBy = eLocateBy.ByXPath;
            actTableElement.LocateValueCalculated = "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 2/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/javax.swing.JScrollPane[0]/javax.swing.JViewport[0]/[[Name:empDetails][ClassName:javax.swing.JTable]]";
            actTableElement.ColSelectorValue = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.LocateColTitle = "0";
            actTableElement.ByWhere = true;
            actTableElement.LocateRowType = "Where";
            actTableElement.LocateRowValue = "";
            actTableElement.WhereColSelector = ActTableElement.eRunColSelectorValue.ColNum;
            actTableElement.WhereColumnTitle = "1";
            actTableElement.WhereOperator = ActTableElement.eRunColOperator.Equals;
            actTableElement.WhereProperty = ActTableElement.eRunColPropertyValue.Value;
            actTableElement.WhereColumnValue = "Jinendra";
            actTableElement.ControlAction = ActTableElement.eTableAction.DoubleClick;
            actTableElement.Active = true;
            actTableElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actTableElement);
            mBF.CurrentActivity.Acts.CurrentItem = actTableElement;

            mGR.RunAction(actTableElement, false);
            //TODO: Find a better way to get ExInfo.
            //Notice: [IndexOf("-")+2] and [LastIndexOf("-")+2] not work with a different format of Date and String(in case we have additional '-'). 
            String ExInfo = actTableElement.ExInfo.Substring(actTableElement.ExInfo.LastIndexOf("M -") + 4);
            Assert.AreEqual(eRunStatus.Passed, actTableElement.Status, "Action Status");
            Assert.AreEqual(actTableElement.Error, null, "Act.Error");
            Assert.AreEqual(ExInfo, "Double Click Activity Passed", "ExInfo");
        }

        [TestMethod]
        public void GetSelectedInternalFrameTitle()
        {
            ActJavaElement actSelectInternalFrame = new ActJavaElement();
            actSelectInternalFrame.LocateBy = eLocateBy.ByName;
            actSelectInternalFrame.LocateValueCalculated = "Internal Frame 1";
            actSelectInternalFrame.ControlAction = ActJavaElement.eControlAction.GetName;
            actSelectInternalFrame.AddNewReturnParams = true;
            actSelectInternalFrame.Active = true;
            mBF.CurrentActivity.Acts.Add(actSelectInternalFrame);
            mBF.CurrentActivity.Acts.CurrentItem = actSelectInternalFrame;
            //Act
            mGR.RunAction(actSelectInternalFrame, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSelectInternalFrame.Status, "Action Status");
            Assert.AreEqual(actSelectInternalFrame.ActReturnValues.FirstOrDefault().Actual, "Internal Frame 1", "ExInfo");
            Assert.AreEqual(actSelectInternalFrame.Error, null, "Act.Error");
        }
    }
}
