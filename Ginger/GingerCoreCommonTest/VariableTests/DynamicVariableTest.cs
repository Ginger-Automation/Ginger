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
        public void DynamicVar_Test1()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            variableDynamic.GenerateAutoValue();
            int num1 = Convert.ToInt32(variableDynamic.Value);

            //Assert            
            Assert.IsTrue(num1 >= 0 && num1 <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
        }

    }
}
