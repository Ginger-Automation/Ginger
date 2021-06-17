using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using GingerCore.Actions;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Runtime.InteropServices;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActScriptTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }
        [TestMethod]
        [Timeout(60000)]
        public void VBSSum2ArgsTest()
        {
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
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

            //Assert;
            Assert.AreEqual(actScript.ReturnValues.Count, 2);
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Param).ToList()), "Hello ,Arg ");
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Actual).ToList()), " hello world, BatFile");
        }
        [TestMethod]
        [Timeout(60000)]
        public void BASHFileArgTest()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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

            //Assert;
            Assert.AreEqual(actScript.ReturnValues.Count, 2);
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Param).ToList()), "Value,Thanks");
            Assert.AreEqual(string.Join(',', actScript.ActReturnValues.Select(x => x.Actual).ToList()), "Shell,You");
        }
    }
}
