
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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace UnitTests.UITests
{
    [Ignore] // temp faile on Azure
    [TestClass]
    [Level3]
    public class WebDriverUnitTest
    {

        static BusinessFlow mBF;
        static GingerRunner mGR = null;

        Mutex mutex = new Mutex();

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            SeleniumDriver mDriver = null;
            mGR = new GingerRunner();
            mGR.Executor.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF Test Screen Shot Action",
                Active = true
            };

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "WebApp" });
            mBF.CurrentActivity.TargetApplication = "WebApp";
            mDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome)
            {
                AutoDetect = true,
                HttpServerTimeOut = 60
            };
            mDriver.StartDriver();

            Agent a = new Agent
            {
                Active = true
            };
            ((AgentOperations)a.AgentOperations).Driver = mDriver;
            a.DriverType = Agent.eDriverType.Selenium;
            DriverConfigParam browserTypeParam = a.GetOrCreateParam(parameter: nameof(GingerWebDriver.BrowserType), defaultValue: nameof(WebBrowserType.Chrome));
            browserTypeParam.Value = nameof(WebBrowserType.Chrome);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [a];

            ApplicationAgent AA = new ApplicationAgent
            {
                AppName = "WebApp",
                Agent = a
            };

            mGR.ApplicationAgents.Add(AA);
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.SetCurrentActivityAgent();

            // use helper !!!!
            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {
            mutex.WaitOne();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mBF.CurrentActivity.Acts.ClearAll();
            mutex.ReleaseMutex();
        }

        [TestMethod]
        public void TakeScreenShotAction()
        {
            //Arrange
            ActScreenShot action = new ActScreenShot
            {
                SaveToFileName = TestResources.GetTestResourcesFolder("ScreenShot"),
                TakeScreenShot = true,
                Active = true,
                WindowsToCapture = Act.eWindowsToCapture.OnlyActiveWindow
            };
            mBF.CurrentActivity.Acts.Add(action);
            mBF.CurrentActivity.Acts.CurrentItem = action;

            //Act
            mGR.Executor.RunAction(action, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", inputURL);
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");

            //validate using getpageurl
            ActBrowserElement actGetPageURL = ValidatePageURL();
            Assert.AreEqual(eRunStatus.Passed, actGetPageURL.Status, "Action Status");
        }

        [TestMethod]
        public void GetBrowserLogTest()
        {
            //arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                Active = true,
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.GetOrCreateInputParam("Value", "www.facebook.com");

            mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts.Add(actBrowser);

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                Active = true,
                AddNewReturnParams = true,
                ControlAction = ActBrowserElement.eControlAction.GetBrowserLog
            };
            mGR.Executor.CurrentBusinessFlow.CurrentActivity.Acts.Add(actBrowser2);

            //act
            mGR.Executor.RunActivity(mGR.Executor.CurrentBusinessFlow.CurrentActivity);

            //assert
            Assert.AreNotEqual(0, actBrowser2.ReturnValues.Count);
            Assert.AreEqual(true, actBrowser2.ReturnValues.FirstOrDefault().FileName.Contains("www.facebook.com"));
        }

        public ActBrowserElement ValidatePageURL()
        {
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetPageURL,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            return actBrowser;

        }

        [TestMethod]
        public void GotoURLWithNewTab()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            //Act
            mGR.Executor.RunAction(actBrowser, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            //Act
            mGR.Executor.RunAction(actBrowser, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.CloseAll
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.CloseTabExcept,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByUrl,
                LocateValue = "https://opensource-demo.orangehrmlive.com/"
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.CloseTabExcept,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle,
                LocateValue = "OrangeHRM"
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetPageSource,
                AddNewReturnParams = true
            };
            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
            Assert.AreEqual(1, actBrowser.ReturnValues.Count);
        }

        [TestMethod]
        public void GetPageURL()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetPageURL,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
            Assert.AreEqual(4, actBrowser.ReturnValues.Count);
        }

        [TestMethod]
        public void GetWindowTitle()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual("OrangeHRM", actBrowser1.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void MaximizeWindow()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());
            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.Maximize
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void NevigateBack()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://opensource-demo.orangehrmlive.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.NavigateBack
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
            Assert.AreNotEqual("OrangeHRM", actBrowser2.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void OpenURLInNewTab()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.OpenURLNewTab
            };
            actBrowser.GetOrCreateInputParam("Value", "www.gmail.com");

            //Act
            mGR.Executor.RunAction(actBrowser, false);

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
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.Refresh
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [TestMethod]
        public void RunJavaScript()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.google.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.RunJavaScript
            };
            actBrowser1.GetOrCreateInputParam("Value", "document.getElementById('hplogo');");
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual(1, actBrowser1.ReturnValues.Count);

        }

        [TestMethod]
        public void InjectJavaScript()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.google.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewWindow.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.InjectJS
            };
            actBrowser1.GetOrCreateInputParam("Value", "document.getElementById('hplogo');");
            actBrowser1.AddNewReturnParams = true;

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual(0, actBrowser1.ReturnValues.Count);

        }

        [TestMethod]
        public void WaitTillPageLoaded()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://ginger.amdocs.com/");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.CheckPageLoaded
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
            Assert.AreEqual("Ginger By Amdocs", actBrowser2.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchWindowUsingGoToURL()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://www.google.co.in/?gws_rd=ssl");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchWindow,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle,
                LocateValue = "Google"
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser1.Status, "Action Status");
            Assert.AreEqual("Google", actBrowser2.ReturnValues[0].Actual);
        }

        [Ignore]  // failed
        [TestMethod]
        public void SwitchWindowByIndex()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "www.google.com");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser1.GetOrCreateInputParam("Value", "www.gmail.com");
            actBrowser1.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.NewTab.ToString());

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchWindow,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByIndex,
                LocateValue = "1"
            };

            //validation
            ActBrowserElement actBrowser3 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);
            mGR.Executor.RunAction(actBrowser3, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
            Assert.IsTrue(actBrowser3.ReturnValues[0].Actual.Contains("Gmail"), "Gmail");
        }


        [TestMethod]
        public void SwitchWindowAction()
        {
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "https://www.google.co.in/?gws_rd=ssl");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActSwitchWindow actSwitchWindow = new ActSwitchWindow
            {
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle,
                LocateValue = "Google"
            };
            actSwitchWindow.GetOrCreateInputParam(ActSwitchWindow.Fields.WaitTime, "10");

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetWindowTitle,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actSwitchWindow, false);
            mGR.Executor.RunAction(actBrowser1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSwitchWindow.Status, "Action Status");
            Assert.AreEqual("Google", actBrowser1.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                LocateValue = "//*[@id='iframeResult']"
            };

            ActBrowserElement actBrowser3 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                LocateValue = "/html/frameset/frame[1]"
            };

            ActUIElement actUIElement = new ActUIElement
            {
                ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                ElementLocateValue = "/html/body/p",
                ElementType = Amdocs.Ginger.Common.UIElement.eElementType.Text,
                ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.GetValue,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);
            mGR.Executor.RunAction(actBrowser3, false);
            mGR.Executor.RunAction(actUIElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actUIElement.Status, "Action Status");
            Assert.AreEqual("Note: The frameset, frame, and noframes elements are not supported in HTML5.", actUIElement.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void SwitchToDefaultFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchToDefaultFrame
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                LocateValue = "//*[@id='iframeResult']"
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser2.Status, "Action Status");
        }

        [TestMethod]
        public void SwitchToParentFrame()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", "http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_frame_cols");
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActBrowserElement actBrowser1 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchToParentFrame
            };

            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchToParentFrame
            };

            ActBrowserElement actBrowser3 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                LocateValue = "//*[@id='iframeResult']"
            };

            ActBrowserElement actBrowser4 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SwitchFrame,
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                LocateValue = "/html/frameset/frame[2]"
            };

            ActUIElement actUIElement = new ActUIElement
            {
                ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByXPath,
                ElementLocateValue = "/html/body/h3",
                ElementType = Amdocs.Ginger.Common.UIElement.eElementType.Label,
                ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.GetValue,
                AddNewReturnParams = true
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actBrowser1, false);
            mGR.Executor.RunAction(actBrowser2, false);
            mGR.Executor.RunAction(actBrowser3, false);
            mGR.Executor.RunAction(actBrowser4, false);
            mGR.Executor.RunAction(actUIElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actUIElement.Status, "Action Status");
            Assert.AreEqual("Frame B", actUIElement.ReturnValues[0].Actual);
        }

        [Ignore]
        public void SmartSyncAction()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", TestResources.GetTestResourcesFile("TestForm.htm"));
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActUIElement actUIElement = new ActUIElement
            {
                ElementLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID,
                ElementLocateValue = "hyperLinkHover",
                ElementType = Amdocs.Ginger.Common.UIElement.eElementType.HyperLink,
                ElementAction = GingerCore.Actions.Common.ActUIElement.eElementAction.Click
            };

            ActSmartSync actSmartSync = new ActSmartSync
            {
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID,
                LocateValue = "txtBox_Google",
                SmartSyncAction = GingerCore.Actions.ActSmartSync.eSmartSyncAction.WaitUntilDisapear
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actUIElement, false);
            mGR.Executor.RunAction(actSmartSync, false);

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

            ActGenElement actGenElement = new ActGenElement
            {
                GenElementAction = GingerCore.Actions.ActGenElement.eGenElementAction.Back
            };

            ActSmartSync actSmartSync1 = new ActSmartSync
            {
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID,
                LocateValue = "txtBox_Google",
                SmartSyncAction = GingerCore.Actions.ActSmartSync.eSmartSyncAction.WaitUntilDisplay,
                WaitTime = 2
            };

            //Act
            mGR.Executor.RunAction(actGenElement, false);
            mGR.Executor.RunAction(actSmartSync1, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actSmartSync1.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void TwoWebElementsDistances()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.URLSrc, ActBrowserElement.eURLSrc.Static.ToString());
            actBrowser.GetOrCreateInputParam("Value", TestResources.GetTestResourcesFile("TestForm.htm"));
            actBrowser.AddOrUpdateInputParamValue(ActBrowserElement.Fields.GotoURLType, ActBrowserElement.eGotoURLType.Current.ToString());

            ActPWL actPWL = new ActPWL
            {
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID,
                LocateValue = "Btn_Click",
                PWLAction = GingerCore.Actions.ActPWL.ePWLAction.GetHDistanceRight2Left,
                OLocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByID,
                OLocateValue = "ddLst"
            };

            ActGenElement actGenElement = new ActGenElement
            {
                GenElementAction = GingerCore.Actions.ActGenElement.eGenElementAction.RunJavaScript
            };
            actGenElement.GetOrCreateInputParam("Value", "document.getElementById('Btn_Click').getBoundingClientRect().top");
            actGenElement.AddNewReturnParams = true;

            //Act
            mGR.Executor.RunAction(actBrowser, false);
            mGR.Executor.RunAction(actPWL, false);
            mGR.Executor.RunAction(actGenElement, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actGenElement.Status, "Action Status");
            Assert.AreEqual("8", actGenElement.ReturnValues[0].Actual);
        }

        [Ignore]
        [TestMethod]
        public void SetAlertBox()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.SetAlertBoxText
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void DeleteAllCookies()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.DeleteAllCookies
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void DismissMessageBox()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.DismissMessageBox
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void AcceptMessageBox()
        {

            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.AcceptMessageBox
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [Ignore]
        [TestMethod]
        public void GetMessageBoxText()
        {
            //Arrange
            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GetMessageBoxText
            };

            //Act
            mGR.Executor.RunAction(actBrowser, false);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actBrowser.Status, "Action Status");
        }

        [TestMethod]
        public void SetEmulationDeviceNameTest()
        {
            //arrange
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "DeviceEmulation" });
            mBF.CurrentActivity.TargetApplication = "DeviceEmulation";

            var mDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome)
            {
                AutoDetect = true,
                HttpServerTimeOut = 60,
                //Set emulation device name
                EmulationDeviceName = "iPad"
            };
            mDriver.StartDriver();

            Agent agent = new Agent
            {
                Active = true
            };
            ((AgentOperations)agent.AgentOperations).Driver = mDriver;
            agent.DriverType = Agent.eDriverType.Selenium;
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(agent);

            ApplicationAgent AA = new ApplicationAgent
            {
                AppName = "DeviceEmulation",
                Agent = agent
            };

            mGR.ApplicationAgents.Add(AA);
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = activity;

            mGR.Executor.SetCurrentActivityAgent();

            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.GetOrCreateInputParam("Value", "http://www.google.com");
            actBrowser.Active = true;
            activity.Acts.Add(actBrowser);

            //act
            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.RunJavaScript
            };
            actBrowser2.GetOrCreateInputParam("Value", "navigator.userAgent");
            actBrowser2.Active = true;
            actBrowser2.AddNewReturnParams = true;
            activity.Acts.Add(actBrowser2);

            mGR.Executor.RunActivity(activity);

            //assert
            Assert.AreEqual(true, actBrowser2.ReturnValues.FirstOrDefault().Actual.Contains("iPad"));
        }

        [TestMethod]
        public void SetUserAgentTest()
        {
            //arrange
            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "DeviceEmulationUserAgent" });
            mBF.CurrentActivity.TargetApplication = "DeviceEmulationUserAgent";

            var mDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome)
            {
                AutoDetect = true,
                HttpServerTimeOut = 60,
                //Set Browser User Agent
                BrowserUserAgent = "Mozilla/5.0 (iPad; CPU OS 11_0 like Mac OS X) AppleWebKit/604.1.34 (KHTML, like Gecko) Version/11.0 Mobile/15A5341f Safari/604.1",
                BrowserHeight = "960",
                BrowserWidth = "600"
            };

            mDriver.StartDriver();

            Agent agent = new Agent
            {
                Active = true
            };
            ((AgentOperations)agent.AgentOperations).Driver = mDriver;
            agent.DriverType = Agent.eDriverType.Selenium;
            ((GingerExecutionEngine)mGR.Executor).SolutionAgents.Add(agent);

            ApplicationAgent AA = new ApplicationAgent
            {
                AppName = "DeviceEmulationUserAgent",
                Agent = agent
            };

            mGR.ApplicationAgents.Add(AA);
            mGR.Executor.CurrentBusinessFlow = mBF;
            mGR.Executor.CurrentBusinessFlow.CurrentActivity = activity;

            mGR.Executor.SetCurrentActivityAgent();

            ActBrowserElement actBrowser = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL
            };
            actBrowser.GetOrCreateInputParam("Value", "http://www.google.com");
            actBrowser.Active = true;
            activity.Acts.Add(actBrowser);

            //act

            //get user agent string
            ActBrowserElement actBrowser2 = new ActBrowserElement
            {
                ControlAction = ActBrowserElement.eControlAction.RunJavaScript
            };
            actBrowser2.GetOrCreateInputParam("Value", "navigator.userAgent");
            actBrowser2.Active = true;
            actBrowser2.AddNewReturnParams = true;
            activity.Acts.Add(actBrowser2);

            mGR.Executor.RunActivity(activity);

            //assert
            Assert.AreEqual(true, actBrowser2.ReturnValues.FirstOrDefault().Actual.Contains("iPad"));
        }


    }
}

