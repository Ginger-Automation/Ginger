using Amdocs.Ginger.Common;
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

            // Init Reporter
            Reporter.WorkSpaceReporter = new UnitTestWorkspaceReporter();
        }
    }
}
