
using Ginger;
using Ginger.BusinessFlowWindows;
using GingerCore;
using GingerTest.POMs;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerTest.BusinessFlowLib
{
    [TestClass]
    [Level3]
    public class BusinessFlowUndoTest
    {
       static GingerAutomator mGingerAutomator;
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\TestUndoBusinessFlow");
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
             mGingerAutomator.MainWindowPOM.ClickAutomateButton();

            businessflow.AutomatePage("Flow 1");
        }

    }
}




