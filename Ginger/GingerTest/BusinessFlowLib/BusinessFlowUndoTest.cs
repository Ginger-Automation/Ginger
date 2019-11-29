
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerTest.BusinessFlowLib
{
    [TestClass]
    [Level3]
    public class BusinessFlowUndoTest
    {
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\TestUndoBusinessFlow");
            GingerAutomator mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(sampleSolutionFolder);
            mGingerAutomator.MainWindowPOM.ClickBusinessFlowRibbon();
            //mGingerAutomator.MainWindowPOM.SelectBusinessFlow();

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestMethod]
        public void UndoForSwitchBFTest()
        {

        }

    }
}




