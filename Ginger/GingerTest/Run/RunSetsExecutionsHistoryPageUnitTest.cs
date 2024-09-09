﻿using Amdocs.Ginger.CoreNET.Application_Models.Execution.POM;
using Ginger.Run;
using GingerCore.Actions;
using GingerCore.Drivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Ginger.Run.RunSetsExecutionsHistoryPage;

namespace GingerTest.Run
{
    [TestClass]
    public class RunSetsExecutionsHistoryPageUnitTest
    {
        /// <summary>
        /// Test method to calculate the page range for the first page.
        /// </summary>
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForFirstPage()
        {
            // Arrange
            int start, end;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 25;
            int currentCount = 25;
            int expectedStart = 1;
            int expectedEnd = 25;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(ePageAction.firstPage, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }

        /// <summary>
        /// Test method to calculate the page range for the next page.
        /// </summary>
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForNextPage()
        {
            // Arrange
            int start, end;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 50;
            int currentCount = 25;
            int expectedStart = 26;
            int expectedEnd = 50;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(ePageAction.nextPage, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }

        /// <summary>
        /// Test method to calculate the page range for the previous page.
        /// </summary>
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForPreviousPage()
        {
            // Arrange
            int start, end;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 25;
            int currentCount = 25;
            int expectedStart = 1;
            int expectedEnd = 25;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(ePageAction.previousPage, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }

        /// <summary>
        /// Test method to calculate the page range for the last page.
        /// </summary>
        [TestMethod]
        public void CalculatePageRange_ShouldReturnCorrectRangeForLastPage()
        {
            // Arrange
            int start, end;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 200;
            int currentCount = 25;
            int expectedStart = 176;
            int expectedEnd = 200;

            // Act
            RunSetsExecutionsHistoryPage.CalculatePageRange(ePageAction.lastPage, recordCount, totalEntries, itemsFetched, currentCount, out start, out end);

            // Assert
            Assert.AreEqual(expectedStart, start);
            Assert.AreEqual(expectedEnd, end);
        }

        /// <summary>
        /// Test method to calculate the page number for the first page.
        /// </summary>
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForFirstPage()
        {
            // Arrange
            int start = 1;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 25;
            int result;
            int expectedPageNo = 1;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(ePageAction.firstPage, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }

        /// <summary>
        /// Test method to calculate the page number for the previous page.
        /// </summary>
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForPreviousPage()
        {
            // Arrange
            int start = 26;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 50;
            int result;
            int expectedPageNo = 2;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(ePageAction.previousPage, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }

        /// <summary>
        /// Test method to calculate the page number for the next page.
        /// </summary>
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForNextPage()
        {
            // Arrange
            int start = 51;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 75;
            int result;
            int expectedPageNo = 3;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(ePageAction.nextPage, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }

        /// <summary>
        /// Test method to calculate the page number for the last page.
        /// </summary>
        [TestMethod]
        public void CalculatePageNo_ShouldReturnCorrectRangeForLastPage()
        {
            // Arrange
            int start = 175;
            int recordCount = 25;
            int totalEntries = 200;
            int itemsFetched = 200;
            int result;
            int expectedPageNo = 8;

            // Act
            result = RunSetsExecutionsHistoryPage.CalculatePageNumber(ePageAction.lastPage, recordCount, totalEntries, itemsFetched, start);

            // Assert
            Assert.AreEqual(expectedPageNo, result);

        }

    }
}


