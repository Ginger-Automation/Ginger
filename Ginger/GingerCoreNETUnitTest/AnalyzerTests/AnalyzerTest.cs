using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using GingerCore;
using GingerCoreNETUnitTest.RunTestslib;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.AnalyzerTests
{
    [TestClass]
    [Level1]
    public class AnalyzerTest
    {
        static SolutionRepository SR;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            string path = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution");
            SR = WorkspaceHelper.CreateWorkspaceAndOpenSolution("AnalyzerTest", path);            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            WorkspaceHelper.ReleaseWorkspace();
        }

        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzeBusinessFlowTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Demo Flow 01.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);

            ObservableList<AnalyzerItemBase> mIssues = new ObservableList<AnalyzerItemBase>();
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreNotEqual(0, mIssues.Count);

        }


    }

}
