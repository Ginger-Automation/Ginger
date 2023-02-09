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
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.Script
{
    [TestClass]
    [Level1]
    public class VBSStringTests
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
        public void MidExpressionTest_OnlyStartPoint()
        {
            //Arrange
            string evalExpr = "Mid(\"MyTest\",3)";
            string expectedResult = "Test";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "MID Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void MidExpressionTest_StartPointAndEndPoint()
        {
            //Arrange
            string evalExpr = "Mid(\"MyTeslaTest\",3,5)";
            string expectedResult = "Tesla";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "MID Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void MidExpressionTest_FullString()
        {
            //Arrange
            string evalExpr = "Mid(\"MyTeslaTest\",1)";
            string expectedResult = "MyTeslaTest";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "MID Expression Test");
        }


        [TestMethod]  [Timeout(60000)]
        public void SplitExpressionTest()
        {
            //Arrange
            string evalExpr = "Split(\"My-Tesla-Test\",\"-\")(1)";
            string expectedResult = "Tesla";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Split Expression Test");
        }

        [TestMethod]
        public void SplitExpressionTest_V2()
        {
            //Arrange
            string evalExpr = "Split(\"My-Tesla-Test\",\"-\")(0)";
            string expectedResult = "My";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Split Expression Test");
        }

        [TestMethod]
        public void SplitExpressionTest_ReturnsEmpty()
        {
            //Arrange
            string evalExpr = "Split(\"My-Tesla-Test\",\"-\")(3)";
            string expectedResult = "";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Split Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void ReplaceExpressionTest()
        {
            //Arrange
            string evalExpr = "Replace(\"MyTeslaTest\",\"Tesla\",\"Toyota\")";
            string expectedResult = "MyToyotaTest";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Replace Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void ReplaceExpressionTest_StringNotFound()
        {
            //Arrange
            string evalExpr = "Replace(\"MyTeslaTest\",\"Toyota\",\"Tesla\")";
            string expectedResult = "MyTeslaTest";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Replace Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void ReplaceExpressionTest_ReplaceOnlyOneOccurance()
        {
            //Arrange
            string evalExpr = "Replace(\"MyTeslaTestAndTeslaTest\",\"Tesla\",\"Toyota\",1,1)";
            string expectedResult = "MyToyotaTestAndTeslaTest";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Replace Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void InStrExpressionTest_FindStringPos()
        {
            //Arrange
            string evalExpr = "InStr(\"This is a beautiful day!\",\"beautiful\")";
            string expectedResult = "11";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "InStr Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void InStrExpressionTest_UsingDifferentStartingPositions()
        {
            //Arrange
            string evalExpr = "InStr(1, \"This is a beautiful day!\",\"i\")";
            string expectedResult = "3";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "InStr Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void InStrExpressionTest_UsingDifferentStartingPositions2()
        {
            //Arrange
            string evalExpr = "InStr(7, \"This is a beautiful day!\",\"i\")";
            string expectedResult = "16";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "InStr Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void InStrRevExpressionTest_FindStringPos()
        {
            //Arrange
            string evalExpr = "InStrRev(\"This is a beautiful day!\",\"beautiful\")";
            string expectedResult = "11";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "InStrRev Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void InStrRevExpressionTest_FindLetterUsingDifferentStartPos()
        {
            //Arrange
            string evalExpr = "InStrRev(\"This is a beautiful day!\",\"T\",-1,1)";
            string expectedResult = "15";
            string evalExpr1 = "InStrRev(\"This is a beautiful day!\",\"T\",-1,0)";
            string expectedResult1 = "1";


            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string result1 = VBS.ExecuteVBSEval(evalExpr1);

            //Assert
            Assert.AreEqual(expectedResult, result, "InStrRev Expression Test");
            Assert.AreEqual(expectedResult1, result1, "InStrRev Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void LeftExpressionTest()
        {
            //Arrange
            string evalExpr = "Left(\"This is a beautiful day!\",15)";
            string expectedResult = "This is a beaut";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Left Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void RightExpressionTest()
        {
            //Arrange
            string evalExpr = "Right(\"This is a beautiful day!\",10)";
            string expectedResult = "tiful day!";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Right Expression Test");
        }

        [Ignore]
        [TestMethod]  [Timeout(60000)]
        public void LTrimExpressionTest()
        {
            //Arrange
            string evalExpr = "LTrim(\"  Hello  \")";
            string expectedResult = "Hello  ";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "LTrim Expression Test");
        }

        [TestMethod]  [Timeout(60000)]
        public void TrimExpressionTest()
        {
            //Arrange
            string evalExpr = "Trim(\"  Hello  \")";
            string expectedResult = "Hello";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Trim Expression Test");
        }

        [TestMethod]  [Timeout(60000)]

        [Ignore]
        public void RTrimExpressionTest()
        {
            //Arrange
            string evalExpr = "RTrim(\"  Hello  \")";
            string expectedResult = "  Hello";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert
            Assert.AreEqual(expectedResult, result, "Trim Expression Test");
        }



    }

}

