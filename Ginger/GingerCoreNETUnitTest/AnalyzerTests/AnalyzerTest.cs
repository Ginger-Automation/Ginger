using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.AnalyzerLib;
using GingerCore;
using GingerCoreNETUnitTest.RunTestslib;
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
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            //WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            string path = TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution");
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

        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzerVariableUsedOnlyInSetVariableTest()
        {
            //Arrange
            //Put the BF in Test Resource
             NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

             string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "BF_MissingVariableTest.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);

                   
            ObservableList<AnalyzerItemBase> mIssues = new ObservableList<AnalyzerItemBase>();
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreEqual(0, mIssues.Count);

        }


        [TestMethod]
        //[Timeout(60000)]
        public void AnalyzerVariableMisssingTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "AnalyzerTestSolution" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "BF_MissingVariableTest.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);


            ObservableList<AnalyzerItemBase> mIssues = new ObservableList<AnalyzerItemBase>();
            AnalyzerUtils mAnalyzerUtils = new AnalyzerUtils();
            WorkSpace.Instance.SolutionRepository = SR;

            businessFlow.Variables.Remove(businessFlow.GetVariable("username"));


            //Run Analyzer
            mAnalyzerUtils.RunBusinessFlowAnalyzer(businessFlow, mIssues);
            //Asert
            Assert.AreEqual(1, mIssues.Count);
            Assert.AreEqual(AnalyzerItemBase.eSeverity.High, mIssues[0].Severity);
            Assert.AreEqual("The Variable 'username' is missing", mIssues[0].Description);
            Assert.AreEqual(AnalyzerItemBase.eType.Error, mIssues[0].IssueType);
            Assert.AreEqual(AnalyzerItemBase.eCanFix.Yes, mIssues[0].CanAutoFix);

        }


    }

}
