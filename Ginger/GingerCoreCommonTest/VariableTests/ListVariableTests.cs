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

    }
}
