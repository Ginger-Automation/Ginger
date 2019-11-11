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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.Java;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTests.UITests.JavaDriverTest
{    
    [Ignore]  // temp
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
                mBF.TargetApplications.Add(new TargetApplication() { AppName = "JavaTestApp" });
                Activity activity = new Activity();
                activity.TargetApplication = "JavaTestApp";
                mBF.Activities.Add(activity);
                mBF.CurrentActivity = activity;

                ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
                LJA.LaunchJavaApplication = true;
                LJA.LaunchWithAgent = true;
                LJA.WaitForWindowTitle = "Java Swing";
                LJA.AddOrUpdateInputParamValue(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.Manual.ToString());
                LJA.Port = "9898";
                LJA.URL = TestResources.GetTestResourcesFile(@"JavaTestApp\JavaTestApp.jar");
                activity.Acts.Add(LJA);
                mGR.PrepActionValueExpression(LJA);
                LJA.Execute();
                // TODO: add wait till action done and check status
                //if (!string.IsNullOrEmpty(LJA.Error))
                //{
                //   throw new Exception(LJA.Error);
                //}

                mDriver = new JavaDriver(mBF);
                mDriver.JavaAgentHost = "127.0.0.1";
                mDriver.JavaAgentPort = 9898;
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

                PayLoad PL = new PayLoad("SwitchWindow");
                PL.AddValue("Java Swing Test App");
                PL.ClosePackage();
                PayLoad RC = mDriver.Send(PL);
                if (RC.IsErrorPayLoad())
                {
                    throw new Exception("Error cannot start Java driver - " + RC.GetValueString());
                }
            }
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            ActWindow AWC = new ActWindow();
            AWC.LocateBy = eLocateBy.ByTitle;
            AWC.LocateValue = "Java";
            AWC.WindowActionType = ActWindow.eWindowActionType.Close;
            mGR.RunAction(AWC, false);
            mGR.StopAgents();
            // mDriver.CloseDriver();
            mDriver = null;
            mGR = null;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mBF.CurrentActivity.Acts.ClearAll();
        }

        #region Unit Test For ActJavaElement
        /**
         * TextField Test Cases  
        */
        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.Error, null, "Act.Error");
            Assert.AreEqual(ExInfo, "Text Field Value Set to - " + a.Value, "ExInfo");
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
        public void ClickTreeNodeWithSlash()
        {
            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "US/California//Texas";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("California/Texas", response.GetListString().FirstOrDefault(), "Selected node value");

        }


        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodeNegativeTest()
        {

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Canada/Texas";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Failed, actJavaElement.Status, "Action Status");
            Assert.IsTrue(actJavaElement.Error.Contains("Node: Texas was not found under: [Root, Canada]"), "Node not found validation");
        }

        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodeNotExistNegativeTest()
        {

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "India";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Failed, actJavaElement.Status, "Action Status");
            Assert.IsTrue(actJavaElement.Error.Contains("Node: India was not found"), "Node not found validation");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodeExactMatchTest()
        {
            // There are 2 nodes matching the substring
            //1. Texas & Florida
            //2. Texas
            //Texas should be selected and not the first one

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Us/Texas";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Texas", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodePartialMatchTest()
        {
            //If Tree is like below
            //-US
            //|--Florida
            //|--Texa
            //|--Texas & Florida
            //|--Texas


            //And user gave partial value Us/Texas &
            //Then Texas & Florida should be selected

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Us/Texas &";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Texas & Florida", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodePartialMatchScenarioTwoTest()
        {
            //If Tree is like below
            //-US
            //|--Florida
            //|--Texa
            //|--Texas & Florida
            //|--Texas

            //And user gave partial value Us/Flori
            //Then Florida should be selected

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Us/Flori";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Florida", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodePartialMatchScenarioThreeTest()
        {
            //If Tree is like below
            //-US
            //|--Florida
            //|--Texa
            //|--Texas & Florida
            //|--Texas

            //And user gave partial value Us/Tex
            //Then Texa should be selected

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Us/Tex";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Texa", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodePartialMatchScenarioFourTest()
        {
            //If Tree is like below
            //-US
            //|--Florida
            //|--Texa
            //|--Texas & Florida
            //|--Texas

            //And user gave partial value Can/Ontario
            //Then Ontario should be selected

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Can/Ontario";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Ontario", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodePartialMatchScenarioFiveTest()
        {
            //If Tree is like below
            //-US
            //|--Florida
            //|--Texa
            //|--Texas & Florida
            //|--Texas

            //And user gave partial value U/Texas
            //Then Texas should be selected

            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "U/Texas";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);


            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Texas", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        
        [TestMethod]
        [Timeout(60000)]
        public void ClickTreeNodeSingleNodeValueTest()
        {

            //Arrange

            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "US";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PayLoad PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("US", response.GetListString().FirstOrDefault(), "Selected node value");

        }

        [TestMethod]
        [Timeout(60000)]
        public void ClickChildTreeNodeSingleNodeValueTest()
        {
            //Arrange

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
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "Ontario";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(actJavaElement);
            mBF.CurrentActivity.Acts.CurrentItem = actJavaElement;

            //Act
            mGR.RunAction(actJavaElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actJavaElement.Status, "Action Status");

            PLClick = new PayLoad("ElementAction");
            PLClick.AddValue(ActJavaElement.eControlAction.GetValue.ToString());
            PLClick.AddEnumValue(ActJavaElement.eWaitForIdle.Medium);
            PLClick.AddValue(eLocateBy.ByName.ToString());
            PLClick.AddValue("countriesTree");
            PLClick.ClosePackage();
            PayLoad response = mDriver.Send(PLClick);

            Assert.IsFalse(response.IsErrorPayLoad());
            Assert.AreEqual("Ontario", response.GetListString().FirstOrDefault(), "Selected node value");

        }


        [TestMethod]
        [Timeout(60000)]
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

        [TestMethod]
        [Timeout(180000)]
        public void ClickNotExistChildNodeValidationTest()
        {
            //Arrange
            ActJavaElement actJavaElement = new ActJavaElement();
            actJavaElement.LocateBy = eLocateBy.ByName;
            actJavaElement.ControlAction = ActJavaElement.eControlAction.Click;
            actJavaElement.LocateValueCalculated = "countriesTree";
            actJavaElement.Value = "US/Ontario";
            actJavaElement.Active = true;
            actJavaElement.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(actJavaElement);

            //Act
            mGR.RunAction(actJavaElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Failed, actJavaElement.Status, "Action Status");           
            Assert.IsTrue(actJavaElement.Error.Contains("Node: Ontario was not found under: [Root, US]"), "Node not found validation");

        }
               
        /**
         * Menu Item Test Cases  
        */
        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Submit", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Works at Amdocs", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "Bachelor", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "India", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, actSelectTab.Status, "Action Status");
            Assert.AreEqual(actSelectTab.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(a.ActReturnValues.FirstOrDefault().Actual, "tab2", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
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
            Assert.AreEqual(eRunStatus.Passed, a.Status, "Action Status");
            Assert.AreEqual(ExInfo, "Click Activity Passed", "ExInfo");
            Assert.AreEqual(a.Error, null, "Act.Error");
        }


        [TestMethod]
        [Timeout(60000)]
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
        #endregion

        #region Unit test for ActTableElement

        [TestMethod]
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        [Timeout(60000)]
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
        #endregion


        #region Unit test for ActUIElement
        [TestMethod]
        [Timeout(60000)]
        public void ACtUIElementGetValue()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByName;
            action.ElementLocateValue = "jtxtArea";
            action.ValueForDriver = "jtxtArea";
            action.AddNewReturnParams = true;

            action.ElementAction = ActUIElement.eElementAction.GetValue;
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.ReturnValues.FirstOrDefault().Actual, "Sample", "ExInfo");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ACtUIElementSetValue()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByName;
            action.ElementLocateValue = "jtxtArea";
            action.ValueForDriver = "jtxtArea";

            action.ElementAction = ActUIElement.eElementAction.SetValue;
            action.Value = "Testing";
            action.ValueForDriver = "Testing";
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            var IsExpectedExInfo = action.ExInfo.Contains(@"Text Area Value Set to - Testing");
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(true, IsExpectedExInfo);
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ACtUIElementIsEnabled()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByName;
            action.ElementLocateValue = "btnClickMe5";
            action.ValueForDriver = "btnClickMe5";
            action.ElementType = eElementType.Button;
            action.ElementAction = ActUIElement.eElementAction.IsEnabled;
            action.Active = true;
            action.AddNewReturnParams = true;

            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.ReturnValues.FirstOrDefault().Actual, "true", "ExInfo");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ACtUIElementButtonClick()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByName;
            action.ElementLocateValue = "btnClickMe5";
            action.ValueForDriver = "btnClickMe5";
            action.ElementType = eElementType.Button;
            action.ElementAction = ActUIElement.eElementAction.Click;
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            var IsExpectedExInfo = action.ExInfo.Contains(@"Click Activity Passed");
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(true, IsExpectedExInfo);
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ACtUIElementSelectComboBoxValue()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByName;
            action.ElementLocateValue = "Country";
            action.Value = "India";
            action.ElementAction = ActUIElement.eElementAction.Select;
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;
            //Act
            mGR.RunAction(action, false);
            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }
        #endregion

        #region Unit Test For SwitchWindow

        [TestMethod]
        [Timeout(60000)]
        public void ActUISwitchWindowActionTest()
        {
            ActUIElement action = new ActUIElement();
            action.ElementLocateBy = eLocateBy.ByTitle;
            action.ElementLocateValue = "Java Swing";
            action.Active = true;
            action.ElementAction = ActUIElement.eElementAction.Switch;
            action.ElementType = eElementType.Window;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActSwitchWindowActionWithNegativeSynTimeTest()
        {
            ActSwitchWindow action = new ActSwitchWindow();
            action.LocateBy = eLocateBy.ByTitle;
            action.LocateValue = "Java Swing";
            action.Active = true;
            action.WaitTime = -1;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
            Assert.AreEqual(30, action.WaitTime);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActSwitchWindowActionWithSynTimeTest()
        {
            ActSwitchWindow action = new ActSwitchWindow();
            action.LocateBy = eLocateBy.ByTitle;
            action.LocateValue = "Java Swing";
            action.Active = true;
            action.WaitTime = 90;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
            Assert.AreEqual(90, action.WaitTime);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActSwitchWindowActionWithNoSyncTimeTest()
        {
            ActSwitchWindow action = new ActSwitchWindow();
            action.LocateBy = eLocateBy.ByTitle;
            action.LocateValue = "Java Swing";
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
            Assert.AreEqual(30, action.WaitTime);
        }

        #endregion

        #region Unit Test For SmartSync Action

        [TestMethod]
        [Timeout(60000)]
        public void ActSmartSyncTest()
        {
            //Arrange
            ActSmartSync action = new ActSmartSync();
            action.LocateBy = eLocateBy.ByName;
            action.LocateValue = "jtxtArea";
            action.SmartSyncAction = ActSmartSync.eSmartSyncAction.WaitUntilDisplay;

            action.WaitTime = 20;
            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        #endregion

        #region Unit Test For ScreenShot Action
        [TestMethod]
        [Timeout(60000)]
        public void ActScreenShootAction()
        {
            CleanTempFolder();
            ActScreenShot action = new ActScreenShot();
            action.TakeScreenShot = true;
            action.WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow;
            action.AddOrUpdateInputParamValueAndCalculatedValue(ActScreenShot.Fields.SaveToFileName, TestResources.GetTestResourcesFolder(@"Temp"));

            action.Active = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        private void CleanTempFolder()
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(TestResources.GetTestResourcesFolder(@"Temp"));

            foreach (FileInfo file in di.GetFiles())
            {
                if (file.Extension.ToLower().Contains(@".jpg"))
                {
                    file.Delete();
                }
            }
        }
        #endregion
        #region Unit Test For ActWindow
        [TestMethod]
        [Timeout(60000)]
        public void ActWindowActionIsExistWindowTest()
        {
            ActWindow action = new ActWindow();
            action.LocateBy = eLocateBy.ByTitle;
            action.LocateValue = "Java Swing";
            action.WindowActionType = ActWindow.eWindowActionType.IsExist;
            action.Active = true;
            action.AddNewReturnParams = true;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
            Assert.AreEqual(1, action.ReturnValues.Count);
            Assert.AreEqual("true", action.ReturnValues[0].Actual);
            Assert.AreEqual(action.Error, null, "Act.Error");
        }
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void ActWindowActionCloseWindowTest()
        {
            ActWindow action = new ActWindow();
            action.LocateBy = eLocateBy.ByTitle;
            action.LocateValue = "Java Swing";
            action.WindowActionType = ActWindow.eWindowActionType.Close;
            action.Active = true;
            action.AddNewReturnParams = true;
            action.StatusConverter = eStatusConverterOptions.AlwaysPass;

            //action.StatusConverter = eStatusConverterOptions.
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");
        }
        #endregion
    }
}
