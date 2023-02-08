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

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class SequenceVariableTests
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
        public void SequenceVar_TestVariableType()
        {
            //Arrange
            VariableSequence variableSequence = new VariableSequence();

            //Act
            string varType = variableSequence.VariableType;

            //Assert            
            Assert.AreEqual("Sequence", varType, "Sequence Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_TestVariableUIType()
        {
            //Arrange
            VariableSequence variableSequence = new VariableSequence();

            //Act
            string varType = variableSequence.VariableUIType;

            //Assert            
            Assert.IsTrue(varType.Contains("Sequence"), "Sequence Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_TestImageType()
        {
            //Arrange
            VariableSequence variableSequence = new VariableSequence();

            //Act
            eImageType eImageType = variableSequence.Image;

            //Assert
            Assert.AreEqual(eImageType.SequentialExecution, eImageType, "Image Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_TestDefaultAutoValue()
        {
            //Arrange
            VariableSequence variableSequence = new VariableSequence();

            //Act
            string errorMsg = string.Empty;
            variableSequence.GenerateAutoValue(ref errorMsg);
            int curSeqVal = Convert.ToInt32(variableSequence.Value);

            //Assert            
            Assert.IsTrue(curSeqVal >= 1 && curSeqVal <= 999, "num1 >= 0 && num1 <= 999");
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_Min5_Max10_Interval_2()
        {
            //Arrange
            VariableSequence variableSequence = new VariableSequence();
            variableSequence.Min = 5;
            variableSequence.Max = 10;
            variableSequence.Interval = 2;

            //Act
            string errorMsg = string.Empty;
            variableSequence.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableSequence.Value);
            variableSequence.GenerateAutoValue(ref errorMsg);
            decimal num2 = decimal.Parse(variableSequence.Value);

            //Assert
            Assert.IsTrue(num1 >= 5, "vs.Value>=5");
            Assert.IsTrue(num2 <= 10, "vs.Value<=10");
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_Digit_13()
        {
            // same number as minimum and maximum and it should return the same number when we autogenerate
            //Arrange
            int sameNum = 13;
            VariableSequence variableSequence = new VariableSequence();
            variableSequence.Min = sameNum;
            variableSequence.Max = sameNum;

            for (int i = 0; i < 10; i++)
            {
                //Act
                string errorMsg = string.Empty;
                variableSequence.GenerateAutoValue(ref errorMsg);
                decimal decNum = decimal.Parse(variableSequence.Value);

                //Assert            
                Assert.AreEqual(decNum, sameNum, "variableRandomString.Value=" + sameNum);
                Assert.IsTrue(decNum >= sameNum && decNum <= sameNum, "num1 >= " + sameNum + " && num1 <= " + sameNum);
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_Range_99_999_interval_9()
        {
            //Arrange
            int minNum = 99;
            int maxNum = 999;
            VariableSequence variableSequence = new VariableSequence();
            variableSequence.Min = minNum;
            variableSequence.Max = maxNum;
            variableSequence.Interval = 9;

            for (int i = 0; i < 100; i++)
            {
                //Act
                string errorMsg = string.Empty;
                variableSequence.GenerateAutoValue(ref errorMsg);
                decimal num1 = decimal.Parse(variableSequence.Value);

                //Assert            
                Assert.IsTrue(num1 >= minNum && num1 <= maxNum, "num1 >= " + minNum  + " && num1 <= " + maxNum);
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_TestSequence()
        {
            //Arrange
            int[] numArr = new int[10];
            int minNum = 0;
            int maxNum = 10;
            int itrCount = 10;

            VariableSequence variableSequence = new VariableSequence();
            variableSequence.Min = minNum;
            variableSequence.Max = maxNum;
            variableSequence.Interval = 1;

            //Act
            string errorMsg = string.Empty;
            for (int i = 0; i < itrCount; i++)
            {
                variableSequence.GenerateAutoValue(ref errorMsg);
                numArr[i] = Convert.ToInt32(variableSequence.Value);
            }

            //Assert
            for (int i = 0; i < itrCount; i++)
            {
                Assert.AreEqual(i + 1, numArr[i], "Sequence Issue");
                Assert.IsTrue(numArr[i] >= minNum && numArr[i] <= maxNum, "num1 >= " + minNum + " && num1 <= " + maxNum);
            }
        }

        [TestMethod]  [Timeout(60000)]
        public void SequenceVar_TestFormula()
        {
            //Arrange
            int minNum = 0;
            int maxNum = 10;
            int interval = 1;
            string expectedFormulaStr = minNum + "-" + maxNum + " Interval " + interval;

            VariableSequence variableSequence = new VariableSequence();
            variableSequence.Min = minNum;
            variableSequence.Max = maxNum;
            variableSequence.Interval = interval;            

            //Act
            string formulaVal = variableSequence.GetFormula();

            //Assert
            Assert.AreEqual(expectedFormulaStr, formulaVal, "Sequence Formula");
        }

    }
}
