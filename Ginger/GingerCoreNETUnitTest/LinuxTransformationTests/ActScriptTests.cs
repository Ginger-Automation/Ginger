using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
        public void RunVBSFile()
        {
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
            Tuple<string, string> tuple = new Tuple<string, string>();

            //Assert;
            Assert.AreEqual(actScript.Status, null);
            Assert.AreEqual(actScript.ReturnValues.Count, 3);
            Assert.AreEqual(actScript.ReturnValues)

        }

        [TestMethod]
        [Timeout(60000)]
        public void ConditionFalseEqualCSTest()
        {
            //Arrange            

            //Act

            //Assert
        }
    }
}
