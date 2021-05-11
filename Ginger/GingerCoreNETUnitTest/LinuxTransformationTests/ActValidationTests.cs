using GingerCore;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActValidationTests
    {
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
            Assert.IsTrue(actValidation.ExInfo.Contains("error"));
        }
    }
}
