#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion
using Amdocs.Ginger.Common.Enums;
using GingerCore;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class NumberVariableTest
    {
        #region Default Class/Test Initialize Methods
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            //
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // before every test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //after every test
        }
        #endregion

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestVariableType()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();

            //Act
            string varType = variableNumber.VariableType;

            //Assert            
            Assert.AreEqual("Number", varType, "Number Variable Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestVariableUIType()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();

            //Act
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            string varType = variableNumber.VariableUIType;

            //Assert
            Assert.AreEqual("Variable Number", varType, "Number Variable UI Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestImageType()
        {
            //Arrange
            VariableNumber variableString = new VariableNumber();

            //Act
            eImageType eimageType = variableString.Image;

            //Assert
            Assert.AreEqual(eImageType.SequentialExecution, eimageType, "Image Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestFormulaVal()
        {
            //Arrange
            VariableNumber NumberVar = new VariableNumber();
            NumberVar.Name = "test";
            NumberVar.Value = "123";

            //Act
            string formulaStr = NumberVar.GetFormula();

            //Assert
            Assert.AreEqual("Initial Value=0", formulaStr, "Mismatch with Default Formula String");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestResetVal()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "1234";

            //Act
            variableNumber.ResetValue();

            //Assert
            Assert.IsNull(variableNumber.Value, "Reset Value not null");
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_Test_SetValue_Int()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "99";
            variableNumber.IsDecimalValue = false;
            variableNumber.MinValue = "-100";
            variableNumber.MaxValue = "100";

            //Act
            var result = variableNumber.SetValue("66");

            //Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_Negative_Test_SetValue_Int()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "99";
            variableNumber.IsDecimalValue = false;
            variableNumber.MinValue = "-100";
            variableNumber.MaxValue = "100";

            //Act
            var result = variableNumber.SetValue("1233");

            //Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_Test_SetValue_Float()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "99.99";
            variableNumber.IsDecimalValue = true;
            variableNumber.MinValue = "-100.60";
            variableNumber.MaxValue = "100.55";

            //Act
            var result = variableNumber.SetValue("100.3");

            //Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_Negative_Test_SetValue_Float()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "99.99";
            variableNumber.IsDecimalValue = true;
            variableNumber.MinValue = "-100.60";
            variableNumber.MaxValue = "100.55";

            //Act
            var result = variableNumber.SetValue("123.3");

            //Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NumberVar_TestVal()
        {
            //Arrange
            VariableNumber variableNumber = new VariableNumber();
            variableNumber.Name = "test";
            variableNumber.Value = "123";

            //Act

            //Assert
            Assert.AreEqual("123", variableNumber.Value, "Number Value");
        }

    }
}