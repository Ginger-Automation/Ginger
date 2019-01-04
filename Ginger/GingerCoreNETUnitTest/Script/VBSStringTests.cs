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


        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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


        [TestMethod]
        public void SplitExpressionTest()
        {
            //Arrange
            string evalExpr = "Split(\"My-TeslaTest\",\"-\")";
            string expectedResult = "My";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert            
            Assert.AreEqual(expectedResult, result, "MID Expression Test");
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

