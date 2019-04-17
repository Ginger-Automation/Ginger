using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.RosLynLib;
using Amdocs.Ginger.CoreNET.RunLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) +  "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += nameof(GingerScriptGlobals.CreateExecutionSummaryJSON) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json


            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert
            // Assert.AreEqual("1")
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

        [TestMethod]
        public void ResultsJSON()
        {

        }

        [TestMethod]
        public void RunFlow()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.RunBusinessFlow) + "(\"Flow 1\");" + Environment.NewLine;    // Runset, env
            

            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert
            // Assert.AreEqual("1")
            // Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");
        }

        [Ignore]
        [TestMethod]
        public void TestRunSetHTMLReport()
        {
            // Arrange
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.RunningFromUnitTest = true;

            WorkSpace.Instance.InitWorkspace(new GingerUnitTestWorkspaceReporter(), new UnitTestRepositoryItemFactory());

            // Create script file

            // Generate a script which contains something like below and exeucte it
            // int i = 1;
            // i++;
            // OpenSolution(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI");
            // OpenRunSet("Default Run Set", "Default");
            // CreateExecutionSummaryJSON(@"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TempFolder\runset.json");
            // i

            string CLISolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\CLI");
            string scriptFile = TestResources.GetTempFile("runset1.ginger.script");
            string jsonFileName = TestResources.GetTempFile("runset.json");
            string txt = "int i=1;" + Environment.NewLine;
            txt += "i++;" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenSolution) + "(@\"" + CLISolutionFolder + "\");" + Environment.NewLine;
            txt += nameof(GingerScriptGlobals.OpenRunSet) + "(\"Default Run Set\", \"Default\");" + Environment.NewLine;    // Runset, env
            txt += nameof(GingerScriptGlobals.CreateExecutionHTMLReport) + "(@\"" + jsonFileName + "\");" + Environment.NewLine;    // summary json


            txt += "i" + Environment.NewLine;  // script rc
            System.IO.File.WriteAllText(scriptFile, txt);


            string arg = "ScriptFile=" + scriptFile;

            // Act
            CLIProcessor CLI = new CLIProcessor();
            CLI.ExecuteArgs(new string[] { arg });

            // Assert
            // Assert.AreEqual("1")
            Assert.AreEqual(WorkSpace.Instance.RunsetExecutor.Runners[0].BusinessFlows[0].RunStatus, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed, "BF RunStatus=Passed");

        }

    }
}
