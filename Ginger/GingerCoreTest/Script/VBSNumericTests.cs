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

using GingerCore.GeneralLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.Script
{    
    [TestClass]
    [Level1]
    public class VBSNumericTests
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
        public void SimpleExpressionTest()
        {
            //Arrange
            string evalExpr = "1+1";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert            
            Assert.AreEqual("2", result, "Simple Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void RoundExpressionTest()
        {
            //Arrange
            string evalExpr_1 = "Round(24.13278)";
            string expectedResult_1 = "24";
            string evalExpr_2 = "Round(24.75122)";
            string expectedResult_2 = "25";
            string evalExpr_3 = "Round(24.13278,2)";
            string expectedResult_3 = "24.13";

            //Act
            string result_1 = VBS.ExecuteVBSEval(evalExpr_1);
            string result_2 = VBS.ExecuteVBSEval(evalExpr_2);
            string result_3 = VBS.ExecuteVBSEval(evalExpr_3);

            //Assert            
            Assert.AreEqual(expectedResult_1, result_1, "Round Expression Test");
            Assert.AreEqual(expectedResult_2, result_2, "Round Expression Test");
            Assert.AreEqual(expectedResult_3, result_3, "Round Expression Test");
        }


        [TestMethod]  [Timeout(60000)]
        public void IntExpressionTest()
        {
            //Arrange
            string evalExpr_1 = "Int(-6.13443)";
            string expectedResult_1 = "-7";
            string evalExpr_2 = "Int(6.83227)";
            string expectedResult_2 = "6";

            //Act
            string result_1 = VBS.ExecuteVBSEval(evalExpr_1);
            string result_2 = VBS.ExecuteVBSEval(evalExpr_2);

            //Assert            
            Assert.AreEqual(expectedResult_1, result_1, "Int Expression Test");
            Assert.AreEqual(expectedResult_2, result_2, "Int Expression Test");
        }


    }

}

