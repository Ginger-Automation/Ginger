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
using System;
using System.Collections.Generic;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class ListVariableTests
    {
        List<string> lstTemp = new List<string>();

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

        public ListVariableTests()
        {
            lstTemp.Add("Apple");
            lstTemp.Add("Blue");
            lstTemp.Add("Green");
            lstTemp.Add("Yellow");
        }

        [TestMethod]
        public void ListVar_TestVariableType()
        {
            //Arrange
            VariableList variableList = new VariableList();

            //Act
            string varType = variableList.VariableType();

            //Assert
            Assert.AreEqual(varType, "List", "List Variable Type mismatch");
        }

        [TestMethod]
        public void ListVar_TestVariableUIType()
        {
            //Arrange
            VariableList variableList = new VariableList();

            //Act
            string varType = variableList.VariableUIType;

            //Assert
            Assert.AreEqual(varType, "Variable List", "List Variable UI Type mismatch");
        }

        [TestMethod]
        public void ListVar_TestImageType()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            eImageType eImageType = variableList.Image;

            //Assert
            Assert.AreEqual(eImageType.VariableList, eImageType, "Image Type Mismatch");
        }

        [TestMethod]
        public void ListVar_TestGenerateAutoValue()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            variableList.RandomOrder = false;
            variableList.GenerateAutoValue();
            string strValue = variableList.Value;

            //Assert
            Assert.AreEqual(strValue, "Apple", "GenerateAutoValue mismatch");
        }

        [TestMethod]
        public void ListVar_TestGenerateAutoValuesSequence()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = false;

            for (int iVar=0; iVar<lstTemp.Count; iVar++)
            {
                //Act
                variableList.GenerateAutoValue();
                string strValue = variableList.Value;

                //Assert
                Assert.AreEqual(strValue, lstTemp[iVar], "GenerateAutoValue mismatch");
            }
        }

        [TestMethod]
        public void ListVar_TestRandomGenerateAutoValue()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = true;

            //Act
            variableList.GenerateAutoValue();
            string strValue = variableList.Value;

            //Assert
            Assert.IsTrue(lstTemp.Contains(strValue), "Random GenerateAutoValue mismatch");
        }

        [TestMethod]
        public void ListVar_TestRandomGenerateAutoValueNotExists()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = true;

            //Act
            variableList.GenerateAutoValue();

            //Assert
            Assert.IsFalse(lstTemp.Contains("Dummy"), "Random GenerateAutoValue mismatch");
        }

        [TestMethod]
        public void ListVar_TestFormula()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);
            string formulaExpectedResult = String.Join(",", lstTemp.ToArray());

            //Act
            string formulaResult = variableList.GetFormula();

            //Assert
            Assert.AreEqual(formulaResult, formulaExpectedResult, "List Formula mismatch");
        }

        [TestMethod]
        public void ListVar_TestResetIndexValue()
        {
            //Arrange
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            variableList.ResetValue();

            //Assert
            Assert.AreEqual(variableList.CurrentValueIndex, 0, "On Reset Index Value not equal to 0");
            Assert.AreEqual(variableList.Value, lstTemp[0], "Reset Index Value Mismatch");
        }


    }
}
