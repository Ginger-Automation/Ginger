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
    public class SelectionListVariableTests
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
        public void SelectionListVar_TestVariableType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
            string varType = variableSelectionList.VariableType();

            //Assert
            Assert.AreEqual("Selection List", varType, "Selection List Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void SelectionListVar_TestVariableUIType()
        {
            //Arrange
            VariableSelectionList variableSelectionList = new VariableSelectionList();

            //Act
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
            Assert.AreEqual(eImageType.VariableList, eImageType, "Image Type");
        }

    }
}
