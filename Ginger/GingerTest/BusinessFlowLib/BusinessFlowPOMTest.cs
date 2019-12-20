
using Ginger;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerTest.POMs;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerTest.BusinessFlowLib
{
    [TestClass]
    [Level3]
    public class BusinessFlowPOMTest
    {
       static GingerAutomator mGingerAutomator;
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions"+ Path.DirectorySeparatorChar +"TestUndoBusinessFlow");
            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(sampleSolutionFolder);

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestMethod]
        public void UndoForSwitchBFTest()
        {
            BusinessFlowPOM businessflow = mGingerAutomator.MainWindowPOM.SelectBusinessFlow();
            BusinessFlow selectedBusinessFlow = businessflow.selectBusinessFlow("Flow 1");

            businessflow.AutomatePage("Flow 1");
            mGingerAutomator.MainWindowPOM.AddActivityToLIstView();

            BusinessFlow selectedBusinessFlow1 = businessflow.selectBusinessFlow("Flow 2");
            businessflow.AutomatePage("Flow 2");
            mGingerAutomator.MainWindowPOM.ClickOnBackToBFTreeBtn();

            BusinessFlow selectedBusinessFlow2 = businessflow.selectBusinessFlow("Flow 1");
            businessflow.AutomatePage("Flow 1");

            int activityCount = mGingerAutomator.MainWindowPOM.ClickOnUndoBtn();

            Assert.AreEqual(1, activityCount);
        }



    }
}




