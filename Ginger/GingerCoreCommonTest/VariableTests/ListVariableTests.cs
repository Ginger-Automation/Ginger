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
        public void ListVar_TestVariableType()
        {
            //Arrange
            VariableList variableList = new VariableList();

            //Act
            string varType = variableList.VariableType;

            //Assert
            Assert.AreEqual("List", varType, "List Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestVariableUIType()
        {
            //Arrange
            VariableList variableList = new VariableList();

            //Act
            string varType = variableList.VariableUIType;

            //Assert
            Assert.IsTrue(varType.Contains("List"), "List Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestImageType()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("Jupiter");
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            eImageType eImageType = variableList.Image;

            //Assert
            Assert.AreEqual(eImageType.VariableList, eImageType, "Image Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestGenerateAutoValue()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("Apple");
            lstTemp.Add("Blue");
            lstTemp.Add("Green");
            lstTemp.Add("Yellow");
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            variableList.RandomOrder = false;
            string errorMsg = string.Empty;
            variableList.GenerateAutoValue(ref errorMsg);
            string strValue = variableList.Value;

            //Assert
            Assert.AreEqual("Apple", strValue, "GenerateAutoValue");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestGenerateAutoValuesSequence()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("Apple");
            lstTemp.Add("Blue");
            lstTemp.Add("Green");
            lstTemp.Add("Yellow");
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = false;

            for (int iVar=0; iVar<lstTemp.Count; iVar++)
            {
                //Act
                string errorMsg = string.Empty;
                variableList.GenerateAutoValue(ref errorMsg);
                string strValue = variableList.Value;

                //Assert
                Assert.AreEqual(lstTemp[iVar], strValue, "GenerateAutoValue");
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestRandomGenerateAutoValue()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("Jupiter");
            lstTemp.Add("Saturn");
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = true;

            //Act
            string errorMsg = string.Empty;
            variableList.GenerateAutoValue(ref errorMsg);
            string strValue = variableList.Value;

            //Assert
            Assert.IsTrue(lstTemp.Contains(strValue), "Random GenerateAutoValue");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestRandomGenerateAutoValueNotExists()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            VariableList variableList = new VariableList("TestList", lstTemp);
            variableList.RandomOrder = true;

            //Act
            string errorMsg = string.Empty;
            variableList.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.IsFalse(lstTemp.Contains("Dummy"), "Random GenerateAutoValue");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestFormula()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("One");
            lstTemp.Add("Two");
            VariableList variableList = new VariableList("TestList", lstTemp);
            string formulaExpectedResult = String.Join(",", lstTemp.ToArray());

            //Act
            string formulaResult = variableList.GetFormula();

            //Assert
            Assert.AreEqual(formulaExpectedResult, formulaResult, "List Formula");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestFormulaForEmptyList()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            VariableList variableList = new VariableList("TestList", lstTemp);
            string formulaExpectedResult = String.Join(",", lstTemp.ToArray());

            //Act
            string formulaResult = variableList.GetFormula();

            //Assert
            Assert.AreEqual(formulaExpectedResult, formulaResult, "List Formula");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestFormulaNoList()
        {
            //Arrange
            VariableList variableList = new VariableList();

            //Act
            string formulaResult = variableList.GetFormula();

            //Assert
            Assert.AreEqual(string.Empty, formulaResult, "List Formula");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestResetIndexValue()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            lstTemp.Add("Friend");
            lstTemp.Add("Love");
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            variableList.ResetValue();

            //Assert
            Assert.AreEqual(0, variableList.CurrentValueIndex, "On Reset Index Value repositioned to 0");
            Assert.AreEqual(lstTemp[0], variableList.Value, "Reset Index Value");
        }

        [TestMethod]  [Timeout(60000)]
        public void ListVar_TestResetIndexValueForEmptyList()
        {
            //Arrange
            List<string> lstTemp = new List<string>();
            VariableList variableList = new VariableList("TestList", lstTemp);

            //Act
            variableList.ResetValue();

            //Assert
            Assert.AreEqual(0, variableList.CurrentValueIndex, "On Reset Index Value repositioned to 0");
        }


    }
}
