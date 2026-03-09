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
