using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class StringVariableTests
    {
        [TestMethod]
        public void PasswordStringVar_TestPasswordVal()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();
            string passwordVal = "sampleTest";

            //Act
            variablePasswordString.Name = "v1";
            variablePasswordString.Value = passwordVal;

            //Assert            
            Assert.AreEqual(variablePasswordString.Value, passwordVal, "Mismatch with VariablePasswordString.Value");
        }

        [TestMethod]
        public void StringVar_TestFormulaVal()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act
            string formulaStr = variableString.GetFormula();

            //Assert
            Assert.AreEqual(formulaStr, "Initial String Value=", "Mismatch with Default Formula String");
        }

        [TestMethod]
        public void StringVar_TestResetVal()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act
            variableString.ResetValue();

            //Assert
            Assert.IsNull(variableString.Value, "Reset Value not null");
        }

        [TestMethod]
        public void StringVar_TestVal()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act

            //Assert
            Assert.AreEqual(variableString.Value, "testVal", "String Value mismatch");
        }

        [TestMethod]
        public void StringVar_TestVariableType()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act
            string varType = variableString.VariableUIType;

            //Assert
            Assert.AreEqual(varType, "Variable String", "String Type mismatch");
        }


    }
}
