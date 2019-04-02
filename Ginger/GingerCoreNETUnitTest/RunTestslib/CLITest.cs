using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Environments;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [Level2]
    [TestClass]
    public class CLITest
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
        public void RunRunSet()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);

            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);
            NewRepositorySerializer.AddClassesFromAssembly(typeof(ProjEnvironment).Assembly);
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RunSetConfig).Assembly);


            // Act
            CLIProcessor.ExecuteArgs(new string[] { "aaa" });

            // Assert
            WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;

        }

        [TestMethod]
        public void ResultsJSON()
        {

        }

        

    }
}
