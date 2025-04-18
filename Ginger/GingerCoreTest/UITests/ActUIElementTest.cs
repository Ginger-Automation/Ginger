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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.UITests
{
    [Ignore]
    [Level3]
    [TestClass]
    public class ActUIElementTest
    {

        static BusinessFlow mBF;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();

            mBF = new BusinessFlow
            {
                Activities = [],
                Name = "BF Test Chrome",
                Active = true
            };
            Platform p = new Platform
            {
                PlatformType = ePlatformType.Web
            };
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "WebApp" });

            VariableString v1 = new VariableString() { Name = "v1", InitialStringValue = "1" };
            mBF.AddVariable(v1);

            mGR = new GingerRunner();
            mGR.Executor = new GingerExecutionEngine(mGR)
            {
                CurrentSolution = new Ginger.SolutionGeneral.Solution()
            };

            Agent a = new Agent
            {
                DriverType = Agent.eDriverType.Selenium
            };
            DriverConfigParam browserTypeParam = a.GetOrCreateParam(parameter: nameof(GingerWebDriver.BrowserType), defaultValue: nameof(WebBrowserType.Chrome));
            browserTypeParam.Value = nameof(WebBrowserType.Chrome);

            ((GingerExecutionEngine)mGR.Executor).SolutionAgents = [a];

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "WebApp", Agent = a });
            mGR.Executor.SolutionApplications =
            [
                new ApplicationPlatform() { AppName = "WebApp", Platform = ePlatformType.Web, Description = "New application" },
            ];
            mGR.Executor.BusinessFlows.Add(mBF);
        }


        [TestMethod]
        [Timeout(60000)]
        public void DragAndDropSelenium()
        {
            // Arrange
            ResetBusinessFlow();

            Activity a1 = new Activity
            {
                Active = true,
                TargetApplication = "WebApp"
            };
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://demos.telerik.com/kendo-ui/dragdrop/index", Active = true };
            a1.Acts.Add(act1);


            ActUIElement act3 = new ActUIElement
            {
                ElementLocateBy = eLocateBy.ByXPath
            };
            // act2.LocateValue
            act3.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue, "//*[@id='draggable']");
            act3.ElementAction = ActUIElement.eElementAction.DragDrop;
            act3.GetOrCreateInputParam(ActUIElement.Fields.DragDropType, ActUIElement.eElementDragDropType.DragDropSelenium.ToString());
            act3.TargetLocateBy = eLocateBy.ByXPath;
            act3.GetOrCreateInputParam(ActUIElement.Fields.TargetLocateValue, "//*[@id='droptarget']");
            act3.Active = true;
            a1.Acts.Add(act3);

            // Act
            mGR.Executor.RunRunner();

            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act3.Status, eRunStatus.Passed);

        }


        [TestMethod]
        [Timeout(60000)]
        public void DragAndDropJS()
        {
            ResetBusinessFlow();

            Activity a1 = new Activity
            {
                Active = true,
                TargetApplication = "WebApp"
            };
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://www.w3schools.com/html/html5_draganddrop.asp", Active = true };
            a1.Acts.Add(act1);

            ActUIElement act2 = new ActUIElement
            {
                ElementLocateBy = eLocateBy.ByXPath
            };
            act2.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue, "//*[@id='drag1']");
            act2.ElementAction = ActUIElement.eElementAction.DragDrop;
            act2.GetOrCreateInputParam(ActUIElement.Fields.DragDropType, ActUIElement.eElementDragDropType.DragDropJS.ToString());
            act2.TargetLocateBy = eLocateBy.ByXPath;
            act2.GetOrCreateInputParam(ActUIElement.Fields.TargetLocateValue, "//*[@id='div2']");
            act2.Active = true;
            a1.Acts.Add(act2);

            mGR.Executor.RunRunner();
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act2.Status, eRunStatus.Passed);

        }


        [TestMethod]
        [Timeout(60000)]
        public void DoDragAndDropByOffSet()
        {
            ResetBusinessFlow();

            Activity a1 = new Activity
            {
                Active = true,
                TargetApplication = "WebApp"
            };
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://demos.telerik.com/kendo-ui/dragdrop/index", Active = true };
            a1.Acts.Add(act1);

            ActUIElement act3 = new ActUIElement
            {
                ElementLocateBy = eLocateBy.ByXPath
            };
            act3.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue, "//*[@id='draggable']");
            act3.ElementAction = ActUIElement.eElementAction.DragDrop;

            act3.TargetLocateBy = eLocateBy.ByXY;
            // act3.GetOrCreateInputParam(ActUIElement.Fields.TargetLocateValue, "1102,463");
            act3.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate, "1102");
            act3.GetOrCreateInputParam(ActUIElement.Fields.XCoordinate, "463");
            act3.Active = true;
            a1.Acts.Add(act3);

            mGR.Executor.RunRunner();
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act3.Status, eRunStatus.Passed);
        }


        [TestMethod]
        [Timeout(60000)]
        public void DrawObject()
        {
            ResetBusinessFlow();

            Activity a1 = new Activity
            {
                Active = true,
                TargetApplication = "WebApp"
            };
            mBF.Activities.Add(a1);

            ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "http://szimek.github.io/signature_pad/", Active = true };
            // ActGotoURL act1 = new ActGotoURL() { LocateBy = eLocateBy.NA, Value = "https://www.youidraw.com/apps/painter/", Active = true };
            a1.Acts.Add(act1);


            ActUIElement act3 = new ActUIElement
            {
                ElementLocateBy = eLocateBy.ByXPath
            };
            act3.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue, "//*[@id='signature-pad']/div[1]/canvas");
            // act3.GetOrCreateInputParam(ActUIElement.Fields.ElementLocateValue, "//*[@id='painter']");
            act3.ElementAction = ActUIElement.eElementAction.DrawObject;

            act3.Active = true;
            a1.Acts.Add(act3);

            mGR.Executor.RunRunner();
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);
            Assert.AreEqual(act1.Status, eRunStatus.Passed);
            Assert.AreEqual(act3.Status, eRunStatus.Passed);

        }
        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }
    }
}
