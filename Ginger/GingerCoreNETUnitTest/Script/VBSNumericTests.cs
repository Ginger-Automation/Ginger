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

        [TestMethod]
        public void SimpleExpressionTest()
        {
            //Arrange
            string evalExpr = "1+1";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            //Assert            
            Assert.AreEqual("2", result, "Simple Expression Test");
        }

        [TestMethod]
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



    }

}

