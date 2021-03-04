using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Environments;
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
    public class ActValidationTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
        }
        [TestCleanup]
        public void TestCleanUp()
        {

        }


        [TestMethod]
        [Timeout(60000)]
        public void ConditionEqualCSTest()
        {
            //Arrange            
            ActValidation actValidation = new ActValidation();
            actValidation.ValueExpression = new ValueExpression(actValidation, "");

            //Act
            actValidation.CalcEngineType = ActValidation.eCalcEngineType.CS;
            actValidation.Condition = "4==4";
            actValidation.Execute();

            //Assert
            Assert.AreEqual(actValidation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ConditionFalseEqualCSTest()
        {
            //Arrange            
            ActValidation actValidation = new ActValidation();
            actValidation.ValueExpression = new ValueExpression(actValidation, "");

            //Act
            actValidation.CalcEngineType = ActValidation.eCalcEngineType.CS;
            actValidation.Condition = "4=4";
            actValidation.Execute();

            //Assert
            Assert.AreEqual(actValidation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }
    }
}
