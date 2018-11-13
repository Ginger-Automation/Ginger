using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions.PlugIns;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [TestClass]
    [Level1]
    public class GingerRunnerPluginDriver
    {
        static BusinessFlow mBF;
        static GingerRunner mGR;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            AutoLogProxy.Init("Unit Tests");
            
            // Create new solution

            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "MyDriver BF";
            mBF.Active = true;
            Platform p = new Platform();
            p.PlatformType = ePlatformType.PluginOther;   // !!!!!!!!!!!!!!!!!!!!!!!!!!
            mBF.Platforms = new ObservableList<Platform>();
            mBF.Platforms.Add(p);
            mBF.TargetApplications.Add(new TargetApplication() { AppName = "Memo app" });

            mGR = new GingerRunner();
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();

            Agent a = new Agent();
            a.DriverType = Agent.eDriverType.Plugin;   // ??????????????

            mGR.SolutionAgents = new ObservableList<Agent>();
            mGR.SolutionAgents.Add(a);

            mGR.ApplicationAgents.Add(new ApplicationAgent() { AppName = "Memo app", Agent = a });
            mGR.SolutionApplications = new ObservableList<ApplicationPlatform>();
            mGR.SolutionApplications.Add(new ApplicationPlatform() { AppName = "Memo app", Platform = ePlatformType.PluginOther });
            mGR.BusinessFlows.Add(mBF);


            // Add the plugin to solution
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = Ginger.App.CreateGingerSolutionRepository();

            Directory.Delete(@"c:\temp\sol1", true);
            WorkSpace.Instance.SolutionRepository.CreateRepository(@"c:\temp\sol1");  // !!!!!!!!!!!!!!!!!
            WorkSpace.Instance.SolutionRepository.Open(@"c:\temp\sol1");
            
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            WorkSpace.Instance.PlugInsManager.AddPluginPackage(@"C:\Users\yaronwe\source\repos\Ginger-Core-Plugin\PluginDriverExample4\bin\Debug\netcoreapp2.1");
 
        }

        [TestMethod]
        public void PluginSay()
        {
            //Arrange
            ResetBusinessFlow();

            Activity a1 = new Activity() { Active = true, TargetApplication = "Memo app"};                        
            mBF.Activities.Add(a1);

            ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "MemoService", ActionId = "Say",  Active = true };
            act1.AddOrUpdateInputParamValue("text", "hello");
            a1.Acts.Add(act1);

            //Act            
            mGR.RunRunner();
            string outVal = act1.GetReturnValue("I said").Actual;

            //Assert
            Assert.AreEqual("hello", outVal, "outVal=hello");
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed);
            Assert.AreEqual(a1.Status, eRunStatus.Passed);            
        }



        [TestMethod]
        public void SpeedTest()
        {
            //Arrange
            ResetBusinessFlow();

            Activity a0 = new Activity() { Active = true, TargetApplication = "Memo app" };
            mBF.Activities.Add(a0);
            
            for (int i = 0; i < 1000; i++)
            {
                ActPlugIn act1 = new ActPlugIn() { PluginId = "Memo", ServiceId = "MemoService", ActionId = "Say", Active = true };
                act1.AddOrUpdateInputParamValue("text", "hello " + i);
                a0.Acts.Add(act1);
            }
            
            //Act
            mGR.RunRunner();


            //Assert
            Assert.AreEqual(mBF.RunStatus, eRunStatus.Passed, "mBF.RunStatus");
            Assert.AreEqual(mBF.Activities.Count(), (from x in mBF.Activities where x.Status == eRunStatus.Passed select x).Count(), "All activities should Passed");
            Assert.IsTrue(a0.Elapsed < 10000, "a0.Elapsed Time less than 10000ms/10sec");
        }


        private void ResetBusinessFlow()
        {
            mBF.Activities.Clear();
            mBF.RunStatus = eRunStatus.Pending;
        }


    }
}
