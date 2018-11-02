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
        public void DynamicVar_TestAutoValue()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            variableDynamic.GenerateAutoValue();
            int num1 = Convert.ToInt32(variableDynamic.Value);

            //Assert            
            Assert.IsTrue(num1 >= 0 && num1 <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
        }

        [TestMethod]
        public void DynamicVar_TestVariableType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableType();

            //Assert            
            Assert.AreEqual(varType, "Dynamic", "Dynamic Variable Type mismatch");
        }

        [TestMethod]
        public void DynamicVar_TestVariableUIType()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();

            //Act
            string varType = variableDynamic.VariableUIType;

            //Assert            
            Assert.AreEqual(varType, "Variable Dynamic", "Dynamic Variable UI Type mismatch");
        }

        [TestMethod]
        public void DynamicVar_TestFormula()
        {
            //Arrange
            VariableDynamic variableDynamic = new VariableDynamic();
            string varVal = "123";

            //Act
            variableDynamic.Name = "d1";
            variableDynamic.Value = varVal;
            string formulaVal = variableDynamic.GetFormula();

            //Assert            
            Assert.AreEqual(formulaVal, varVal, "Dynamic Variable Formula mismatch");
        }


    }
}
