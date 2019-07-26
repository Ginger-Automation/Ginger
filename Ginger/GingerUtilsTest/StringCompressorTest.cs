using GingerTestHelper;
using GingerUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerUtilsTest
{
    [TestClass]
    public class StringCompressorTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        [ClassInitialize()]
        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }

        [TestMethod]
        public void SampleTest1()
        {
            //Arrange
            string s1 = "abcdefghijklmnop";


            //Act
            string compressed = StringCompressor.CompressString(s1);
            string uncompressed = StringCompressor.DecompressString(compressed);

            //Assert
            Assert.AreEqual(uncompressed, s1, "uncompressed=s1");
        }
    }
}
