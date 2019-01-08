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
            DateTime localDate = DateTime.Parse(result);

            //Assert            
            Assert.IsTrue(!string.IsNullOrEmpty(result), "result not empty");
            Assert.AreEqual(localDate.ToString(), result, "Now Expression Test");
        }

        [TestMethod]
        public void WeekdayExpressionTest()
        {
            //Arrange
            string evalExpr = "Weekday(Now)";

            //Act
            int result = int.Parse(VBS.ExecuteVBSEval(evalExpr)); 
            int weekDay = (int)DateTime.Now.DayOfWeek + 1;

            //Assert            
            Assert.AreEqual(weekDay , result, "Weekday Expression Test");
        }

        [TestMethod]
        public void MonthExpressionTest()
        {
            //Arrange
            string evalExpr = "Month(Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string monthNum = DateTime.Now.Month.ToString();

            //Assert            
            Assert.AreEqual(monthNum, result, "Month Expression Test");
        }

        [TestMethod]
        public void YearExpressionTest()
        {
            //Arrange
            string evalExpr = "Year(Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string year = DateTime.Now.Year.ToString();

            //Assert            
            Assert.AreEqual(year, result, "Year Expression Test");
        }

        [TestMethod]
        public void HourExpressionTest()
        {
            //Arrange
            string evalExpr = "Hour(Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string hour = DateTime.Now.Hour.ToString();

            //Assert            
            Assert.AreEqual(hour, result, "Hour Expression Test");
        }

        [TestMethod]
        public void MinuteExpressionTest()
        {
            //Arrange
            string evalExpr = "Minute(Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string minute = DateTime.Now.Minute.ToString();

            //Assert            
            Assert.AreEqual(minute, result, "Minute Expression Test");
        }

        [TestMethod]
        public void DatePartExpressionTest_Year()
        {
            //Arrange
            string evalExpr = "DatePart(\"yyyy\", Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string year = DateTime.Now.Year.ToString();

            //Assert            
            Assert.AreEqual(year, result, "DatePart Expression Test");
        }

        [TestMethod]
        public void DatePartExpressionTest_Month()
        {
            //Arrange
            string evalExpr = "DatePart(\"m\", Now)";

            //Act
            string result = VBS.ExecuteVBSEval(evalExpr);
            string month = DateTime.Now.Month.ToString();

            //Assert            
            Assert.AreEqual(month, result, "DatePart Expression Test");
        }

    }
}



