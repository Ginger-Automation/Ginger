using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Ginger.Run;
using GingerCore.Actions;
using GingerCore.Drivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GingerTest.Run
{
    [TestClass]
    public class RunSetsExecutionsHistoryPageUnitTest
    {
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForFirstPage()
        {
            // Arrange
            int start, end;
            string pageAction="firstPage";
            int recordCount=25;
            int totalEntries=200;
            int itemsFetched=25;
            int currentCount = 25;
            int expectedStart=1;
            int expectedEnd = 25;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(pageAction, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForNextPage()
        {
            // Arrange
            int start, end;
            string pageAction = "nextPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 50;
            int currentCount = 25;
            int expectedStart = 26;
            int expectedEnd = 50;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(pageAction, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForPreviousPage()
        {
            // Arrange
            int start, end;
            string pageAction = "previousPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 25;
            int currentCount = 25;
            int expectedStart = 1;
            int expectedEnd = 25;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(pageAction, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForLastPage()
        {
            // Arrange
            int start, end;
            string pageAction = "lastPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 200;
            int currentCount = 25;
            int expectedStart = 176;
            int expectedEnd = 200;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(pageAction, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }

        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForFirstPage()
        {
            // Arrange
            int start = 1;
            string pageAction = "firstPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 25;
            int result;
            int expectedPageNo = 1;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber( pageAction,  recordCount,  totalEntries,  itemsFetched,  start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);
        
        }

        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForPreviousPage()
        {
            // Arrange
            int start = 26;
            string pageAction = "previousPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 50;
            int result;
            int expectedPageNo = 2;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(pageAction, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForNextPage()
        {
            // Arrange
            int start = 51;
            string pageAction = "nextPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 75;
            int result;
            int expectedPageNo = 3;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(pageAction, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForLastPage()
        {
            // Arrange
            int start = 175;
            string pageAction = "lastPage";
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 200;
            int result;
            int expectedPageNo = 8;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(pageAction, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }

    }
}


