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
            Runner runner1 = new Runner() { Name = "Runner 1" };
            runner1.Agents.Add(new Agent() { ApplicationName = "App1", AgentName = "Chrome 1" });
            runner1.Agents.Add(new Agent() { ApplicationName = "App2", AgentName = "PB 1" });
            dynamicRunSet.Runners.Add(runner1);
            runner1.BusinessFlows.Add(new BusinessFlow() { Name = "BF 1" });
            runner1.BusinessFlows.Add(new BusinessFlow() { Name = "BF 2" });

            BusinessFlow BF3 = new BusinessFlow() { Name = "BF 3" };
            BF3.InputVariables = new List<InputVariable>();
            BF3.InputVariables.Add(new InputVariable() { VariableName = "v1", VariableValue = "abc" });
            runner1.BusinessFlows.Add(BF3);

            //Act            
            string fileName = TestResources.GetTempFile("dynflow.xml");
            DynamicRunSetManager.Save(dynamicRunSet, fileName);


            //Assert
            // Assert.IsTrue(Directory.Exists(BFDir), "BF directory exist");

        }

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
