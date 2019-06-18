using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.RunListeners
{
    [Level2]
    [TestClass]
    public class DynamicRunSetTest
    {
        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            
        }

        

        [ClassCleanup]
        public static void ClassCleanup()
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }


        //[TestMethod]
        //public void CreateDynamicRunSet()
        //{
        //    //Arrange
        //    DynamicGingerExecution dynamicRunSet = new DynamicGingerExecution() { Name = "Dyn 1", Environemnt = "UAT" };
        //    AddRunner runner1 = new AddRunner() { Name = "Runner 1" };
        //    runner1.SetAgents.Add(new AgentMap() { ApplicationName = "App1", AgentName = "Chrome 1" });
        //    runner1.SetAgents.Add(new AgentMap() { ApplicationName = "App2", AgentName = "PB 1" });
        //    dynamicRunSet.Runners.Add(runner1);
        //    runner1.AddBusinessFlows.Add(new AddBusinessFlow() { Name = "BF 1" });
        //    runner1.AddBusinessFlows.Add(new AddBusinessFlow() { Name = "BF 2" });

        //    AddBusinessFlow BF3 = new AddBusinessFlow() { Name = "BF 3" };
        //    BF3.InputVariables = new List<InputVariable>();
        //    BF3.InputVariables.Add(new InputVariable() { VariableName = "v1", VariableValue = "abc" });
        //    runner1.AddBusinessFlows.Add(BF3);

        //    //Act            
        //    string fileName = TestResources.GetTempFile("dynflow.xml");
        //    DynamicRunSetManager.Save(dynamicRunSet, fileName);


        //    //Assert
        //    // Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");

        //}

        //[TestMethod]
        //public void LoadDynamicRunSet()
        //{
        //    //Arrange
        //    DynamicRunSet dynamicRunSet = DynamicRunSetManager.Load(@"c:\temp\dynflow.xml");

        //    RunsetExecutor runsetExecutor = new RunsetExecutor();


        //    //Act            
        //    DynamicRunSetManager.LoadRunSet(runsetExecutor, dynamicRunSet);


        //    //Assert
        //    // Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");

        //}
    }
}
