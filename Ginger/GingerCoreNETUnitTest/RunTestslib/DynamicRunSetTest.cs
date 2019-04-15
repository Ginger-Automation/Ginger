using Amdocs.Ginger.CoreNET.RunLib.DynamicRunSetLib;
using Ginger.Run;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

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


        [TestMethod]
        public void CreateDynamicRunSet()
        {
            //Arrange
            DynamicRunSet dynamicRunSet = new DynamicRunSet() { Name = "Dyn 1", Environemnt = "UAT" };
            AddRunner runner1 = new AddRunner() { Name = "Runner 1" };
            runner1.Agents.Add(new SetAgent() { TargetApplication = "App1", Agent = "Chrome 1" });
            runner1.Agents.Add(new SetAgent() { TargetApplication = "App2", Agent = "PB 1" });
            dynamicRunSet.Runners.Add(runner1);
            runner1.BusinessFlows.Add(new AddBusinessFlow() { Name = "BF 1" });
            runner1.BusinessFlows.Add(new AddBusinessFlow() { Name = "BF 2" });

            AddBusinessFlow BF3 = new AddBusinessFlow() { Name = "BF 3" };
            BF3.Variables = new List<SetBusinessFlowVariable>();
            BF3.Variables.Add(new SetBusinessFlowVariable() { Name = "v1", Value = "abc" });
            runner1.BusinessFlows.Add(BF3);

            //Act            
            DynamicRunSetManager.Save(dynamicRunSet, @"c:\temp\dynflow.xml");


            //Assert
            // Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");

        }

        [TestMethod]
        public void LoadDynamicRunSet()
        {
            //Arrange
            DynamicRunSet dynamicRunSet = DynamicRunSetManager.Load(@"c:\temp\dynflow.xml");

            RunsetExecutor runsetExecutor = new RunsetExecutor();


            //Act            
            DynamicRunSetManager.LoadRunSet(runsetExecutor, dynamicRunSet);


            //Assert
            // Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");

        }
    }
}
