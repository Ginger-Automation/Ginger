using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class DynamicVariableTest
    {

        [TestMethod]
        public void DynamicVar_TestAutoValue()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            variableDynamic.GenerateAutoValue();
            int num1 = Convert.ToInt32(variableDynamic.Value);

            //Assert            
            Assert.IsTrue(num1 >= 0 && num1 <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
        }

        [TestMethod]
        public void DynamicVar_TestVariableType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableType();

            //Assert            
            Assert.AreEqual(varType, "Dynamic", "Dynamic Variable Type mismatch");
        }

        [TestMethod]
        public void DynamicVar_TestVariableUIType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableUIType;

            //Assert            
            Assert.AreEqual(varType, "Variable Dynamic", "Dynamic Variable UI Type mismatch");
        }

        [TestMethod]
        public void DynamicVar_TestFormula()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();
            string varVal = "123";

            //Act
            variableDynamic.Name = "d1";
            variableDynamic.Value = varVal;
            string formulaVal = variableDynamic.GetFormula();

            //Assert            
            Assert.AreEqual(formulaVal, varVal, "Dynamic Variable Formula mismatch");
        }


    }
}
