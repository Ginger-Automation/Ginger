#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class StringVariableTests
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
        public void StringVar_TestVariableType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            string varType = variableString.VariableType();

            //Assert            
            Assert.AreEqual(varType, "String", "String Variable Type mismatch");
        }

        [TestMethod]
        public void StringVar_TestVariableUIType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            string varType = variableString.VariableUIType;

            //Assert
            Assert.AreEqual(varType, "Variable String", "String Variable UI Type mismatch");
        }

        [TestMethod]
        public void StringVar_TestImageType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            eImageType eImageType = variableString.Image;

            //Assert
            Assert.AreEqual(eImageType.Variable, eImageType, "Image Type Mismatch");
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
        public void PasswordStringVar_TestVal()
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
        public void PasswordStringVar_TestResetValue()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();
            variablePasswordString.Name = "p1";
            variablePasswordString.Password = "testPass";

            //Act
            variablePasswordString.ResetValue();

            //Assert
            Assert.IsNull(variablePasswordString.Password, "Reset Value not null");
        }

        [TestMethod]
        public void PasswordStringVar_TestVariableUIType()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();

            //Act
            string varType = variablePasswordString.VariableUIType;

            //Assert            
            Assert.AreEqual(varType, "Variable Password String", "Password String Type mismatch");
        }

        [TestMethod]
        public void PasswordStringVar_TestVariableType()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();

            //Act
            string varType = variablePasswordString.VariableType();

            //Assert            
            Assert.AreEqual(varType, "PasswordString", "Password String Type mismatch");
        }

        [TestMethod]
        public void PasswordStringVar_TestFormula()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();
            string passwordVal = "testPass";

            //Act
            variablePasswordString.Name = "v2";
            variablePasswordString.Password = passwordVal;
            string formulaVal = variablePasswordString.GetFormula();

            //Assert            
            Assert.AreEqual(formulaVal, passwordVal, "Password Formula mismatch");
        }


    }
}
