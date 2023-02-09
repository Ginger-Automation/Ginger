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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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

        [TestMethod]  [Timeout(60000)]
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



