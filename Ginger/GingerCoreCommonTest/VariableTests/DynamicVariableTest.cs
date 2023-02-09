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
using System;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class DynamicVariableTest
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
        public void DynamicVar_TestVariableType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableType;

            //Assert            
            Assert.AreEqual("Dynamic", varType, "Dynamic Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void DynamicVar_TestVariableUIType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableUIType;

            //Assert
            Assert.IsTrue(varType.Contains("Dynamic"), "Dynamic Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void DynamicVar_TestImageType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            eImageType eImageType = variableDynamic.Image;

            //Assert
            Assert.AreEqual(eImageType.CSS3Text, eImageType, "Image Type");
        }

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void DynamicVar_TestFormula()
        //{
        //    //Arrange
        //    string valExpr = "{VBS Eval=2+2}";
        //    string expectedResult = "4";

        //    VariableDynamic variableDynamic = new VariableDynamic();
        //    variableDynamic.Name = "d1";
        //    variableDynamic.Value = valExpr;

        //    //Act
        //    string formulaVal = variableDynamic.GetFormula();

        //    //Assert            
        //    Assert.AreEqual(expectedResult, formulaVal, "Dynamic Variable Formula");
        //}

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void DynamicVar_TestAutoValue()
        //{
        //    //Arrange
        //    string valExpr = "{VBS Eval=1+1}";
        //    VariableDynamic variableDynamic = new VariableDynamic();
        //    variableDynamic.Name = "d1";
        //    variableDynamic.Value = valExpr;

        //    //Act
        //    variableDynamic.GenerateAutoValue();
        //    int num1 = Convert.ToInt32(variableDynamic.Value);

        //    //Assert            
        //    Assert.IsTrue(num1 >= 0 && num1 <= int.MaxValue, "num1 >= 0 && num1 <= int.max");
        //}

    }
}
