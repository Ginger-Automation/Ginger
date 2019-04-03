
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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Ginger.Actions.UCValueExpression;

namespace UnitTests.UITests
{
    [TestClass]
    [Level3]
   public class WebDriverUnitTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR = null;
        static SeleniumDriver mDriver = null;

       [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //AutoLogProxy.Init("Screen Shot Action");
            //RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();
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

        [TestMethod]
        public void TakeScreenShot()
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
            Assert.AreEqual(action.Error, null, "Act.Error");
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(TestResources.GetTestResourcesFolder("ScreenShot"));

            foreach (System.IO.FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        s}
        
    }
}

