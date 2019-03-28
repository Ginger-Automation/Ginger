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

namespace UnitTests.UITests
{
    [TestClass]
    [Level3]
   public class WebDriverTest
    {
        static BusinessFlow mBF;
        static GingerRunner mGR = null;
        static SeleniumDriver mDriver = null;

       [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("Screen Shot Action");
            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();

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

            mDriver = new SeleniumDriver(mBF);
            mDriver.HttpServerTimeOut = 120;
            mDriver.PageLoadTimeOut = 90;
            mDriver.ImplicitWait = 30;
            mDriver.StartDriver();
            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.SeleniumChrome;

            a.Name = "Web Agent";
            a.Driver = mDriver;

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            //mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "WebApp", Agent = a });
            //mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            //mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "WebApp", Platform = ePlatformType.Web, Description = "New application" });

            ApplicationAgent AA = new ApplicationAgent();
            AA.AppName = "WebTestApp";
            AA.Agent = a;
            mBF.CurrentActivity.CurrentAgent = a;
            mGR.ApplicationAgents.Add(AA);
            mGR.CurrentBusinessFlow = mBF;

            mGR.SetCurrentActivityAgent();

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.CurrentBusinessFlow = mBF;
            mGR.BusinessFlows.Add(mBF);

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
            action.SaveToFileName = "C:\\Users\\aditijag\\Desktop\\SS";
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
    }
}

