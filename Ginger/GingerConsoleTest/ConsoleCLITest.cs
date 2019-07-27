using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerConsoleTest
{

    [TestClass]
    public class ConsoleCLITest
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
            mTestHelper.Log("aaa");


            //Arrange
            string s1 = "aaa";
            string s2 = "bbb";

            //Act
            string result = s1 + s2;

            //Assert
            Assert.AreEqual("aaabbb", result, "result");
        }
    }
}
