using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace GingerCoreNETUnitTest
{
    [TestClass]
    public class TestAssemblyInit
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            // Called once when the test assembly is loaded
            // We provide the assembly to GingerTestHelper.TestResources so it can locate the 'TestResources' folder path
            TestResources.Assembly = Assembly.GetExecutingAssembly();
            
            InitWS();
        }


        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            // Called once when the test assembly is done
            // WorkSpace.Instance.Close();
        }


        static void InitWS()
        {
            // TODO: check if ws is null

            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;
            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());
        }

    }
}
