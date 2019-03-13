#region License
/*
Copyright © 2014-2019 European Support Limited

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

        [TestMethod]  [Timeout(60000)]
        public void StringVar_TestVariableType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            string varType = variableString.VariableType();

            //Assert            
            Assert.AreEqual("String", varType, "String Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void StringVar_TestVariableUIType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            string varType = variableString.VariableUIType;

            //Assert
            Assert.AreEqual("Variable String", varType, "String Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void StringVar_TestImageType()
        {
            //Arrange
            VariableString variableString = new VariableString();

            //Act
            eImageType eImageType = variableString.Image;

            //Assert
            Assert.AreEqual(eImageType.Variable, eImageType, "Image Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void StringVar_TestFormulaVal()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act
            string formulaStr = variableString.GetFormula();

            //Assert
            Assert.AreEqual("Initial String Value=", formulaStr, "Mismatch with Default Formula String");
        }

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
        public void StringVar_TestVal()
        {
            //Arrange
            VariableString variableString = new VariableString();
            variableString.Name = "test";
            variableString.Value = "testVal";

            //Act

            //Assert
            Assert.AreEqual("testVal", variableString.Value, "String Value");
        }

        [TestMethod]  [Timeout(60000)]
        public void PasswordStringVar_TestVal()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();
            string passwordVal = "sampleTest";

            //Act
            variablePasswordString.Name = "v1";
            variablePasswordString.Value = passwordVal;

            //Assert            
            Assert.AreEqual(passwordVal, variablePasswordString.Value, "Mismatch with VariablePasswordString.Value");
        }

        [TestMethod]  [Timeout(60000)]
        public void PasswordStringVar_TestResetValue()
        {
            //Arrange
            string password = "testPass"; 
            VariablePasswordString variablePasswordString = new VariablePasswordString();
            variablePasswordString.Name = "p1";
            variablePasswordString.Password = password;

            //Act
            variablePasswordString.Value = password + "change";
            variablePasswordString.ResetValue();

            //Assert
            Assert.AreEqual(password, variablePasswordString.Value, "Password Reset Fail");
        }

        [TestMethod]  [Timeout(60000)]
        public void PasswordStringVar_TestVariableUIType()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();

            //Act
            string varType = variablePasswordString.VariableUIType;

            //Assert            
            Assert.IsTrue(varType.Contains("Password"), "Password String Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void PasswordStringVar_TestVariableType()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();

            //Act
            string varType = variablePasswordString.VariableType();

            //Assert            
            Assert.AreEqual("PasswordString", varType, "Password String Type");
        }

        [TestMethod]  [Timeout(60000)]
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
            Assert.AreEqual(passwordVal, formulaVal, "Password Formula");
        }

        [TestMethod]  [Timeout(60000)]
        public void PasswordStringVar_TestImageType()
        {
            //Arrange
            VariablePasswordString variablePasswordString = new VariablePasswordString();

            //Act
            eImageType eImageType = variablePasswordString.Image;

            //Assert
            Assert.AreEqual(eImageType.Password, eImageType, "Image Type");
        }

    }
}
