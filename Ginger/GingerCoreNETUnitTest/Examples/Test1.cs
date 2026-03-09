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
using System.IO;

namespace GingerCoreNETUnitTest.Examples
{
    // Test class template with Test Helper examples

    [TestClass]
    public class Test1
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        [ClassInitialize]
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
            string s1 = "aaa";
            string s2 = "bbb";

            //Act
            string result = s1 + s2;

            //Assert
            Assert.AreEqual("aaabbb", result, "result");
        }


        [TestMethod]
        public void SampleTestWithLog()
        {
            //Arrange
            mTestHelper.Log("Checking string aaa+bbb");
            string s1 = "aaa";
            string s2 = "bbb";

            //Act
            mTestHelper.Log("Started calculating");
            string result = s1 + s2;
            mTestHelper.Log("result = " + result);

            //Assert
            Assert.AreEqual("aaabbb", result, "result");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SampleTestWithTimeOut()
        {
            //Arrange
            int i = 0;

            //Act
            while (i < 100000)
            {
                i++;
            }

            //Assert
            Assert.AreEqual(100000, i, "i");
        }

        [TestMethod]
        public void SampleTestArtifact()
        {
            //Arrange            
            string longstring = "abc" + Environment.NewLine;

            //Act
            while (longstring.Length < 1000)
            {
                longstring += longstring;
            }
            mTestHelper.Log("longstring.length = " + longstring.Length);

            //Artifacts
            mTestHelper.CreateTestArtifact("longstring.txt", longstring);

            //Assert
            Assert.IsTrue(longstring.Length > 1000, "longstring.Length");
        }

        [TestMethod]
        public void SampleTestArtifactFile()
        {
            //Arrange                        
            string fileName = mTestHelper.GetTempFileName("numbers.txt");

            //Act
            for (int i = 0; i < 10; i++)
            {
                System.IO.File.AppendAllText(fileName, "line " + i + "##");
            }

            //Artifacts
            mTestHelper.AddTestArtifact(fileName);

            //Assert
            long fileSize = new FileInfo(fileName).Length;

            Assert.AreEqual(80, fileSize, "fileSize");
        }
    }
}
