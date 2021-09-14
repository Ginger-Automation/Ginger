using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [Level1]
    [TestClass]
    public class VariableInputOutoutMappingTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "OutputVariableOldSolution"));

            WorkSpace.Instance.OpenSolution(path, EncryptionHandler.GetDefaultKey());

        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
            WorkSpace.LockWS();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            mTestHelper.TestCleanup();
            WorkSpace.RelWS();
        }

        //Enable once set variable and  condition validation action supported on linux
        [Ignore]
        [TestMethod]
        [Timeout(60000)]
        public void OutputVariableOldMappingValueExecutionTest()
        {
            //Arrange         

            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            RunSetConfig runSetConfig = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor.RunSetConfig = runSetConfig;

            WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault();
            WorkSpace.Instance.RunsetExecutor.InitRunners();

            //Act
            WorkSpace.Instance.RunsetExecutor.RunRunset().ConfigureAwait(false);

            //Assert
            Assert.AreEqual(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[0].Status, "Runner 1 Status");
            Assert.AreEqual(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[1].Status, "Runner 2 Status");
            Assert.AreEqual(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, WorkSpace.Instance.RunsetExecutor.Runners[2].Status, "Runner 3 Status");
        }

        [TestMethod]
        [Timeout(60000)]
        public void OutputVariableOldMappingValueRunsetConfigLoadTest()
        {
            //Arrange         
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

            //Act
            RunSetConfig runSetConfig = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor.RunSetConfig = runSetConfig;

            WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault();
            WorkSpace.Instance.RunsetExecutor.InitRunners();

            //Assert
            Assert.AreEqual("2887671c-4f0c-4be8-9233-ca2f20ed1f04_b3ed569d-c99b-4a5e-8bb3-1462d83a837f", runSetConfig.GingerRunners[0].BusinessFlowsRunList[2].BusinessFlowCustomizedRunVariables[0].MappedOutputValue, "outputvariables with same name from different flow");
            Assert.AreEqual("d1677d32-b215-4fd2-82aa-1d64c92317ab_b3ed569d-c99b-4a5e-8bb3-1462d83a837f", runSetConfig.GingerRunners[1].BusinessFlowsRunList[2].BusinessFlowCustomizedRunVariables[0].MappedOutputValue, "outputvariables with same name from different instances of same flow");
            Assert.AreEqual("d1677d32-b215-4fd2-82aa-1d64c92317ab_b3ed569d-c99b-4a5e-8bb3-1462d83a837f", runSetConfig.GingerRunners[2].BusinessFlowsRunList[0].BusinessFlowCustomizedRunVariables[0].MappedOutputValue, "outputvariables with same name from different instances of same flow from prev runner");

        }

        [TestMethod]
        [Timeout(60000)]
        public void InputVariableChangeFromSourceTest()
        {
            //Arrange         
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow targetBusinessFlow = businessFlows.Where(b => b.Name.Contains("Check Variables")).FirstOrDefault();
            ObservableList<GingerCore.Variables.VariableBase> lstVariables = targetBusinessFlow.Activities[0].Variables;
            //Act
            RunSetConfig runSetConfig = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Check Variables Run Set" select x).SingleOrDefault();
            WorkSpace.Instance.RunsetExecutor.RunSetConfig = runSetConfig;
            WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().FirstOrDefault();
            WorkSpace.Instance.RunsetExecutor.InitRunners();
            ObservableList<GingerCore.Variables.VariableBase> lstRunSetVariables = runSetConfig.GingerRunners[0].BusinessFlowsRunList[0].BusinessFlowCustomizedRunVariables;
            
            //Assert
            Assert.AreEqual(null, lstRunSetVariables[0].Value, "Var A is supposed to be equal to null");
            Assert.AreNotEqual(lstVariables[1].Value, lstRunSetVariables[1].Value, "Var B should be different in Run Set and Business Flow");
        }

    }
}
