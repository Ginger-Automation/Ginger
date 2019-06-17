﻿
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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCore.Platforms.PlatformsInfo;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace UnitTests.UITests
{
    [TestClass]
    [Level3]
    public class WebDriverUnitTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR = null;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            SeleniumDriver mDriver = null;
            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Test Screen Shot Action";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            Platform p = new Platform();
            p.PlatformType = ePlatformType.Web;
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "WebApp" });
            mBF.CurrentActivity.TargetApplication = "WebApp";
            mDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
            mDriver.AutoDetect = true;
            mDriver.HttpServerTimeOut = 60;
            mDriver.StartDriver();

            Agent a = new Agent();
            a.Active = true;
            a.Driver = mDriver;
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            ApplicationAgent AA = new ApplicationAgent();
            AA.AppName = "WebApp";
            AA.Agent = a;

            mGR.ApplicationAgents.Add(AA);
            mGR.CurrentBusinessFlow = mBF;
            mGR.SetCurrentActivityAgent();

            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }


        [TestCleanup]
        public void TestCleanup()
        {
            mBF.CurrentActivity.Acts.ClearAll();
        }

        [TestMethod]
        public void TakeScreenShotAction()
        {
            //Arrange
            ActScreenShot action = new ActScreenShot();
            action.SaveToFileName = TestResources.GetTestResourcesFolder("ScreenShot");
            action.TakeScreenShot = true;
            action.Active = true;
            action.WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow;
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.RunAction(action, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, action.Status, "Action Status");

            //Delete folder files
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(TestResources.GetTestResourcesFolder("ScreenShot"));
            FileInfo[] files = di.GetFiles("*.jpg").Where(p => p.Extension == ".jpg").ToArray();

            foreach (System.IO.FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch (Exception e)
                {
                    throw new Exception("Error:", e);
                }
            }
        }

        [TestMethod]
        public void GotoURLWithCurrent()
        {
            //Arrange
            string inputURL = "www.gmail.com";
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", inputURL);
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");
        }

        public ActBrowserElement ValidatePageURL()
        {
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GetPageURL;
            actBrowser.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);

            return actBrowser;

        }

        [TestMethod]
        public void GotoURLWithNewTab()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");

        }

        [TestMethod]
        public void GotoURLWithNewWindow()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");

        }

        [Ignore]
        [TestMethod]
        public void CloseAll()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.CloseAll;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");

            //Validation
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Failed, actGetPageURL.Status, "Action Status");

        }

        [TestMethod]
        public void CloseTabExceptByURL()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.CloseTabExcept;
            actBrowser2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByUrl;
            actBrowser2.LocateValue = "https://opensource-demo.orangehrmlive.com/";

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");
        }

     
        [TestMethod]
        public void CloseTabExceptByTitle()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.CloseTabExcept;
            actBrowser2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle;
            actBrowser2.LocateValue = "OrangeHRM";

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");
        }

        [TestMethod]
        public void GetPageSource()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GetPageSource;
            actBrowser.AddNewReturnParams = true;
            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
            Assert.AreEqual(1, actBrowser.ReturnValues.Count);
        }

        [TestMethod]
        public void GetPageURL()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GetPageURL;
            actBrowser.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
            Assert.AreEqual(4, actBrowser.ReturnValues.Count);
        }

        [TestMethod]
        public void GetWindowTitle()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.GetWindowTitle;
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual("OrangeHRM", actBrowser1.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void MaximizeWindow()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());
            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.Maximize;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void NevigateBack()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.NavigateBack;

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.GetWindowTitle;
            actBrowser2.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
            Assert.AreNotEqual("OrangeHRM", actBrowser2.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void OpenURLInNewTab()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.OpenURLNewTab;
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");
        }

        [TestMethod]
        public void Refresh()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.Refresh;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [TestMethod]
        public void RunJavaScript()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.google.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.RunJavaScript;
            actBrowser1.GetOrCreateInputParam("Value", "document.getElementById('hplogo');");
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual(1, actBrowser1.ReturnValues.Count);

        }

        [TestMethod]
        public void InjectJavaScript()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.google.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.InjectJS;
            actBrowser1.GetOrCreateInputParam("Value", "document.getElementById('hplogo');");
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual(0, actBrowser1.ReturnValues.Count);

        }

        [TestMethod]
        public void WaitTillPageLoaded()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://ginger.amdocs.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.CheckPageLoaded;

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.GetWindowTitle;
            actBrowser2.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
            Assert.AreEqual("Ginger By Amdocs", actBrowser2.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchWindowUsingGoToURL()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://www.google.co.in/?gws_rd=ssl");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.SwitchWindow;
            actBrowser1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle;
            actBrowser1.LocateValue = "Google";

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.GetWindowTitle;
            actBrowser2.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual("Google", actBrowser2.ReturnValues[0].Actual);
        }


        [TestMethod]
        public void SwitchWindowAction()
        {
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://www.google.co.in/?gws_rd=ssl");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActSwitchWindow actSwitchWindow = new ActSwitchWindow();
            actSwitchWindow.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle;
            actSwitchWindow.LocateValue = "Google";
            actSwitchWindow.GetOrCreateInputParam(ActSwitchWindow.Fields.WaitTime,"10");

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.GetWindowTitle;
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actSwitchWindow, false);
            mGR.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSwitchWindow.Status, "Action Status");
            Assert.AreEqual("Google", actBrowser1.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            actBrowser2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actBrowser2.LocateValue = "//*[@id='iframeResult']";

            ActBrowserElement actBrowser3 = new ActBrowserElement();
            actBrowser3.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            actBrowser3.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actBrowser3.LocateValue = "/html/frameset/frame[1]";

            ActUIElement actUIElement = new ActUIElement();
            actUIElement.ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actUIElement.ElementLocateValue = "/html/body/p";
            actUIElement.ElementType = Amdocs.Ginger.Common.UIElement.eElementType.Text;
            actUIElement.ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.GetValue;
            actUIElement.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);
            mGR.RunAction(actBrowser3, false);
            mGR.RunAction(actUIElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actUIElement.Status, "Action Status");
            Assert.AreEqual("Note: The frameset, frame, and noframes elements are not supported in HTML5.", actUIElement.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchToDefaultFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame;

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            actBrowser2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actBrowser2.LocateValue = "//*[@id='iframeResult']";

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
        }

        [TestMethod]
        public void SwitchToParentFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement();
            actBrowser1.ControlAction = ActBrowserElement.eControlAction.SwitchToParentFrame;

            ActBrowserElement actBrowser2 = new ActBrowserElement();
            actBrowser2.ControlAction = ActBrowserElement.eControlAction.SwitchToParentFrame;

            ActBrowserElement actBrowser3 = new ActBrowserElement();
            actBrowser3.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            actBrowser3.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actBrowser3.LocateValue = "//*[@id='iframeResult']";

            ActBrowserElement actBrowser4 = new ActBrowserElement();
            actBrowser4.ControlAction = ActBrowserElement.eControlAction.SwitchFrame;
            actBrowser4.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actBrowser4.LocateValue = "/html/frameset/frame[2]";

            ActUIElement actUIElement = new ActUIElement();
            actUIElement.ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath;
            actUIElement.ElementLocateValue = "/html/body/h3";
            actUIElement.ElementType = Amdocs.Ginger.Common.UIElement.eElementType.Label;
            actUIElement.ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.GetValue;
            actUIElement.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actBrowser1, false);
            mGR.RunAction(actBrowser2, false);
            mGR.RunAction(actBrowser3, false);
            mGR.RunAction(actBrowser4, false);
            mGR.RunAction(actUIElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actUIElement.Status, "Action Status");
            Assert.AreEqual("Frame B", actUIElement.ReturnValues[0].Actual);
        }

        [Ignore]
        public void SmartSyncAction()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value",TestResources.GetTestResourcesFile("TestForm.htm"));
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActUIElement actUIElement = new ActUIElement();
            actUIElement.ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
            actUIElement.ElementLocateValue = "hyperLinkHover";
            actUIElement.ElementType = Amdocs.Ginger.Common.UIElement.eElementType.HyperLink;
            actUIElement.ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.Click;

            ActSmartSync actSmartSync = new ActSmartSync();
            actSmartSync.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
            actSmartSync.LocateValue = "txtBox_Google";
            actSmartSync.SmartSyncAction = GingerCore.Actions.ActSmartSync.eSmartSyncAction.WaitUntilDisapear;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actUIElement, false);
            mGR.RunAction(actSmartSync, false);
                
            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSmartSync.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void SmartSyncAction_WaitUntilDisapear()
        {
            SmartSyncAction();
        }

        [Ignore]
        [TestMethod]
        public void SmartSyncAction_WaitUntilDisplay()
        {
            SmartSyncAction();

            ActGenElement actGenElement = new ActGenElement();
            actGenElement.GenElementAction = GingerCore.Actions.ActGenElement.eGenElementAction.Back;

            ActSmartSync actSmartSync1 = new ActSmartSync();
            actSmartSync1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
            actSmartSync1.LocateValue = "txtBox_Google";
            actSmartSync1.SmartSyncAction = GingerCore.Actions.ActSmartSync.eSmartSyncAction.WaitUntilDisplay;
            actSmartSync1.WaitTime = 2;

            //Act
            mGR.RunAction(actGenElement, false);
            mGR.RunAction(actSmartSync1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSmartSync1.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void TwoWebElementsDistances()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GotoURL;
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", TestResources.GetTestResourcesFile("TestForm.htm"));
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActPWL actPWL = new ActPWL();
            actPWL.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
            actPWL.LocateValue = "Btn_Click";
            actPWL.PWLAction = GingerCore.Actions.ActPWL.ePWLAction.GetHDistanceRight2Left;
            actPWL.OLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID;
            actPWL.OLocateValue = "ddLst";

            ActGenElement actGenElement = new ActGenElement();
            actGenElement.GenElementAction = GingerCore.Actions.ActGenElement.eGenElementAction.RunJavaScript;
            actGenElement.GetOrCreateInputParam("Value", "document.getElementById('Btn_Click').getBoundingClientRect().top");
            actGenElement.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actBrowser, false);
            mGR.RunAction(actPWL, false);
            mGR.RunAction(actGenElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actGenElement.Status, "Action Status");
            Assert.AreEqual("8", actGenElement.ReturnValues[0].Actual);
        }

        [Ignore]
        [TestMethod]
        public void SetAlertBox()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.SetAlertBoxText;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void DeleteAllCookies()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.DeleteAllCookies;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void DismissMessageBox()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.DismissMessageBox;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void AcceptMessageBox()
        {

            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.AcceptMessageBox;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void GetMessageBoxText()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement();
            actBrowser.ControlAction = ActBrowserElement.eControlAction.GetMessageBoxText;

            //Act
            mGR.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }
    }
}

