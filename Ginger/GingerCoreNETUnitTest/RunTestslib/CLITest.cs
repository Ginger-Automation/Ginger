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
    [Level3]
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

            // Create script file
            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");                        
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;            
            txt += "OpenSolution(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += "OpenRunSet(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor.ExecuteArgs(new string[] { arg });

            // Assert
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

        [TestMethod]
        public void ResultsJSON()
        {

        }

        

    }
}
