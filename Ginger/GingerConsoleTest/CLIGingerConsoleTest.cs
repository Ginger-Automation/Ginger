#region License
/*
Copyright © 2014-2026 European Support Limited

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

using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            string[] args = new string[] { "help" };


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
