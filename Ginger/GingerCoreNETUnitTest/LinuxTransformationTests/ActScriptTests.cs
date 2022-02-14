using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActScriptTests
    {
        GingerExecutionEngine mGR;
        bool isOSWindows = true;
        
        [TestInitialize]
        public void TestInitialize()
        {
            BusinessFlow mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF Non-Driver Action Test";
            mBF.Active = true;

            Activity activity = new Activity();
            mBF.Activities.Add(activity);
            mBF.CurrentActivity = activity;

            mGR = new GingerExecutionEngine(new GingerRunner());
            mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
            mGR.CurrentBusinessFlow = mBF;
            mGR.BusinessFlows.Add(mBF);


            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            isOSWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
        [TestMethod]
        [Timeout(60000)]
        public void VBSSum2ArgsTest()
        {
            if(!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            //Arrange
            ActScript actScript = new ActScript()
            {
                ScriptName = "VBSSum2Args.vbs",
                ScriptCommand = ActScript.eScriptAct.Script
            };
            actScript.AddOrUpdateInputParamValueAndCalculatedValue("Var1", "56");
            actScript.AddOrUpdateInputParamValueAndCalculatedValue("Var2", "77");
            actScript.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            actScript.ScriptPath = TestResources.GetTestResourcesFolder(@"Files");
            actScript.AddNewReturnParams = true;

            //Act
            actScript.Execute();

            //Assert
            Assert.AreEqual(actScript.ReturnValues.Count, 3);
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Param).ToList()), "Var1,Var2,sum");
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Actual).ToList()), "56,77,133");
        }

        [TestMethod]
        [Timeout(60000)]
        public void BATFile1ArgTest()
        {
            if (!isOSWindows)
            {
                return;
            }
            //Arrange
            ActScript actScript = new ActScript()
            {
                ScriptName = "BATReturnParam.bat",
                ScriptCommand = ActScript.eScriptAct.Script
            };
            actScript.AddOrUpdateInputParamValueAndCalculatedValue("Param", "BatFile");
            actScript.ScriptInterpreterType = ActScript.eScriptInterpreterType.BAT;
            actScript.ScriptPath = TestResources.GetTestResourcesFolder(@"Files");
            actScript.AddNewReturnParams = true;

            //Act
            actScript.Execute();

            //Assert
            Assert.AreEqual(actScript.ReturnValues.Count, 2);
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Param).ToList()), "Hello ,Arg ");
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Actual).ToList()), " hello world, BatFile");
        }
        [TestMethod]
        [Timeout(60000)]
        public void BASHFileArgTest()
        {
            if (isOSWindows)
            {
                return;
            }
            //Arrange
            ActScript actScript = new ActScript()
            {
                ScriptName = "BASHWithArgs.sh",
                ScriptCommand = ActScript.eScriptAct.Script
            };
            actScript.AddOrUpdateInputParamValueAndCalculatedValue("v1", "Shell");
            actScript.AddOrUpdateInputParamValueAndCalculatedValue("v2", "You");
            actScript.ScriptInterpreterType = ActScript.eScriptInterpreterType.SH;
            actScript.ScriptPath = TestResources.GetTestResourcesFolder(@"Files");
            actScript.AddNewReturnParams = true;

            //Act
            actScript.Execute();

            //Assert
            Assert.AreEqual(actScript.ReturnValues.Count, 2);
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Param).ToList()), "Value,Thanks");
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Actual).ToList()), "Shell,You");
        }
        [TestMethod]
        public void ActScriptTestWithIndexZero()
        {
            if (!isOSWindows)
            {
                return;
            }
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(Path.Combine(@"Files",@"ScriptWithGingerOutputIndexZero.vbs"));
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(2, actScript.ReturnValues.Count);
            Assert.AreEqual("OK", actScript.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void ActScriptTestWithGingerOutput()
        {
            if (!isOSWindows)
            {
                return;
            }
            //with index > 0
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(Path.Combine(@"Files", @"ScriptWithGingerOutput.vbs"));
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(2, actScript.ReturnValues.Count);
            Assert.AreEqual("OK", actScript.ReturnValues[0].Actual);
        }

        [TestMethod]
        public void ActScriptTestWithoutOutput()
        {
            if (!isOSWindows)
            {
                return;
            }
            //with index=-1
            //Arrange
            ActScript actScript = new ActScript();
            actScript.ScriptInterpreter = ActScript.eScriptInterpreterType.VBS.ToString();
            actScript.ScriptCommand = ActScript.eScriptAct.Script;
            actScript.ScriptName = TestResources.GetTestResourcesFile(Path.Combine(@"Files", @"ScriptWithoutOutput.vbs"));
            actScript.Active = true;
            actScript.AddNewReturnParams = true;

            //Act
            mGR.RunAction(actScript);

            //Assert
            Assert.AreEqual(eRunStatus.Passed, actScript.Status, "Action Status");
            Assert.AreEqual(1, actScript.ReturnValues.Count);
            Assert.AreEqual("\n\nHello\nSNO=1\n\n", actScript.ReturnValues[0].Actual);
        }
        /// <summary>
        /// Running a free VBS command
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        public void FreeCommandVBS()
        {
            if (!isOSWindows)
            {
                return;
            }
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            v.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            v.AddNewReturnParams = true;
            v.AddOrUpdateInputParamCalculatedValue("Free Command", "NumberB=10\r\nNumberA=20\r\nDim Result\r\nResult= int(NumberA) + int(NumberB)\r\nWscript.Echo \"Add=\" & Result");

            v.Execute();

            Assert.AreEqual(v.Error, null);
            Assert.AreEqual(v.ReturnValues[0].Actual.Contains("Add=30"), true);
        }

        /// <summary>
        /// Running a free VBS file with arguments
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        public void RunScriptAPlusBVBS()
        {
            if (!isOSWindows)
            {
                return;
            }
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            v.AddNewReturnParams = true;
            v.ScriptCommand = ActScript.eScriptAct.Script;
            v.AddOrUpdateInputParamCalculatedValue("p1", "5");
            v.AddOrUpdateInputParamCalculatedValue("p2", "7");

            v.ScriptName = "APlusB.vbs";
            v.ScriptPath = TestResources.GetTestResourcesFolder("Files");

            //Act
            v.Execute();

            //Assert
            Assert.AreEqual(v.Error, null);
            Assert.AreEqual(v.ReturnValues[0].Param, "TXT");
            Assert.AreEqual(v.ReturnValues[0].Actual, "5 + 7 = 12");

            Assert.AreEqual(v.ReturnValues[1].Param, "Total");
            Assert.AreEqual(v.ReturnValues[1].Actual, "12");

        }

        /// <summary>
        /// Running a free Batch command
        /// </summary>
        [Timeout(60000)]
        [TestMethod]  
        public void FreeCommandBAT()
        {
            if (!isOSWindows)
            {
                return;
            }
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.BAT;
            v.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            v.AddNewReturnParams = true;
            v.AddOrUpdateInputParamCalculatedValue("Free Command", "@echo off \r\nSET /A a = 5 \r\nSET /A b = 10 \r\nSET /A c = %a% + %b% \r\necho Add=%c% ");

            //Act
            v.Execute();

            //Assert
            Assert.AreEqual(v.Error, null);
            Assert.AreEqual(v.ReturnValues[0].Actual.Contains("Add=15"), true);
        }

        /// <summary>
        /// Running a free Batch file with arguments
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        public void RunScriptAPlusBBAT()
        {
            if (!isOSWindows)
            {
                return;
            }
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.BAT;
            v.AddNewReturnParams = true;
            v.ScriptCommand = ActScript.eScriptAct.Script;
            v.AddOrUpdateInputParamCalculatedValue("p1", "5");
            v.AddOrUpdateInputParamCalculatedValue("p2", "7");

            v.ScriptName = "BatchScriptWithArgs.bat";
            v.ScriptPath = TestResources.GetTestResourcesFolder("Files");

            //Act
            v.Execute();

            //Assert
            Assert.AreEqual(v.Error, null);

            Assert.AreEqual(v.ReturnValues[0].Param, "Result");
            Assert.AreEqual(v.ReturnValues[0].Actual, "12");

        }
    }
}
