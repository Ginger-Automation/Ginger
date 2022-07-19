#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class SelectionListVariableTests
    {

        #region Default Class/Test Initialize Methods
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            
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
        public void SelectionListVar_TestVariableType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            string varType = variableSelectionList.VariableType;

            //Assert
            Assert.AreEqual("Selection List", varType, "Selection List Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SelectionListVar_TestVariableUIType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            GingerTerminology.TERMINOLOGY_TYPE = eTerminologyType.Default;
            string varType = variableSelectionList.VariableUIType;

            //Assert
            Assert.AreEqual("Variable Selection List", varType, "Selection List Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SelectionListVar_TestImageType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            eImageType eImageType = variableSelectionList.Image;

            //Assert
            Assert.AreEqual(eImageType.MinusSquare, eImageType, "Image Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestFormula()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);

            List<string> list = new List<string>();
            foreach (OptionalValue val in variableSelectionList.OptionalValuesList)
            {
                list.Add(val.Value);
            }

            string formulaExpectedResult = "Options: " + String.Join(",", list.ToArray());

            //Act
            string formulaResult = variableSelectionList.GetFormula();

            //Assert
            Assert.AreEqual(formulaExpectedResult, formulaResult, "List Formula");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestFormulaForEmptyList()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            List<string> list = new List<string>();
            foreach (OptionalValue val in variableSelectionList.OptionalValuesList)
            {
                list.Add(val.Value);
            }

            string formulaExpectedResult = "Options: " + String.Join(",", list.ToArray());

            //Act
            string formulaResult = variableSelectionList.GetFormula();

            //Assert
            Assert.AreEqual(formulaExpectedResult, formulaResult, "List Formula");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestFormulaNoList()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            string formulaExpectedResult = "Options: ";

            //Act
            string formulaResult = variableSelectionList.GetFormula();

            //Assert
            Assert.AreEqual(formulaExpectedResult, formulaResult, "List Formula");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestSetValue()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            OptionalValue newVal3 = new OptionalValue("Three");
            variableSelectionList.OptionalValuesList.Add(newVal3);

            //Act
            variableSelectionList.SetValue("Two");
            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = "Two";

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestSetValueNotExistInList()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            OptionalValue newVal3 = new OptionalValue("Three");
            variableSelectionList.OptionalValuesList.Add(newVal3);

            //Act
            variableSelectionList.SetValue("Four");
            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = null;

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestResetValue()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            OptionalValue newVal3 = new OptionalValue("Three");
            variableSelectionList.OptionalValuesList.Add(newVal3);

            
            variableSelectionList.SetValue("Two");

            // Act
            variableSelectionList.ResetValue();

            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = "One";

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestResetValueEmptyList()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            // Act
            variableSelectionList.ResetValue();

            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = null;

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestGenerateAutoValue()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            OptionalValue newVal3 = new OptionalValue("Three");
            variableSelectionList.OptionalValuesList.Add(newVal3);

            // Act
            variableSelectionList.GenerateAutoValue();
            variableSelectionList.GenerateAutoValue();

            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = "Two";

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestGenerateAutoValueLoopToStart()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            variableSelectionList.IsLoopEnabled = true;

            // Act
            variableSelectionList.GenerateAutoValue();
            variableSelectionList.GenerateAutoValue();
            variableSelectionList.GenerateAutoValue();

            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = "One";

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListVar_TestGenerateAutoValueNoLoop()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();
            OptionalValue newVal1 = new OptionalValue("One");
            variableSelectionList.OptionalValuesList.Add(newVal1);
            OptionalValue newVal2 = new OptionalValue("Two");
            variableSelectionList.OptionalValuesList.Add(newVal2);
            variableSelectionList.IsLoopEnabled = false;
            //Todo Add no loop ticker

            // Act
            variableSelectionList.GenerateAutoValue();
            variableSelectionList.GenerateAutoValue();

            string setValueResult = variableSelectionList.Value;

            string setValueExpectedResult = "Value is at the last in the list and no looping chechkbox is not enabled";

            //Assert
            Assert.AreEqual(setValueExpectedResult, setValueResult, "Set Value Result");
        }
    }
}
