using GingerTestHelper;
using GingerUtils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GingerConsoleTest
{
    [TestClass]
    public class CLIGingerConsoleTest
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

        [Ignore]
        [TestMethod]
        public void RunCLI()
        {
            // Arrange
            string[] args = new string[] { "help"};


            //Act
            Amdocs.Ginger.GingerRuntime.Program.Main(args);

            //Assert
            Assert.AreEqual(0, Environment.ExitCode, "Environment.ExitCode");

        }

        [Ignore]
        [TestMethod]
        public void BadCLIArg()
        {
            // Arrange
            string[] args = new string[] { "koko" };


            //Act
            Amdocs.Ginger.GingerRuntime.Program.Main(args);

            //Assert
            Assert.AreEqual(1, Environment.ExitCode, "Environment.ExitCode");

        }
    }
}
