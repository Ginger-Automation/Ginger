using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            //WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            string path = TestResources.GetTestResourcesFolder(@"Solutions\AnalyzerTestSolution");
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(path);
        }

        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzeBusinessFlowTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions\AnalyzerTestSolution\BusinessFlows\Demo Flow 01.Ginger.BusinessFlow.xml");

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
