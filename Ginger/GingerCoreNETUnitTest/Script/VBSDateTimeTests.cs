using GingerCore.GeneralLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GingerCoreNETUnitTest.Script
{
    [TestClass]
    [Level1]
    public class VBSDateTimeTests
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
        public void NowExpressionTest()
        {
            //Arrange
            string evalExpr = "now";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            //12/31/2018 7:38:56 PM
            DateTime localDate = DateTime.Now;

            //Assert            
            Assert.AreEqual(localDate.ToString(), result, "Now Expression Test");
        }


        [TestMethod]
        public void WeekdayExpressionTest()
        {
            //Arrange
            string evalExpr = "Weekday(Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);

            int weekDay = ((int)DateTime.Now.DayOfWeek == 0) ? 7 : (int)DateTime.Now.DayOfWeek + 1;

            //Assert            
            Assert.AreEqual(weekDay + "", result, "Weekday Expression Test");
        }



    }
}



