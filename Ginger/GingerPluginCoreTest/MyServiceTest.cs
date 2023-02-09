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

using Amdocs.Ginger.Plugin.Core;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
//using PluginExample;

namespace GingerPluginCoreTest
{
    [TestClass]
    public class MyServiceTest
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

        //        [TestMethod]
        //        public void runLikeDriver()
        //        {
        //            //GingerSocketServer2 gingerSocketServer2 = new GingerSocketServer2();
        //            //gingerSocketServer2.StartServer(15002);
        //            //gingerSocketServer2.MessageHandler += srvmess;


        //            //gingerSocketServer2.SendPayLoad("aaaa", "lplp");

        //            //GingerNodeStarter gingerNodeStarter = new GingerNodeStarter();
        //            //// gingerNodeStarter.StartNode()

        //        }

        //        [TestMethod]
        //        public void StartDriver()
        //        {
        //            ////Arrange
        //            //MyPlugin myPlugin = new MyPlugin();

        //            ////Act
        //            //myPlugin.StartDriver();

        //            ////assert
        //            //Assert.AreEqual(true, myPlugin.IsRunning , "IsRunning = true");
        //        }

        //        [TestMethod]
        //        public void CloseDriver()
        //        {
        //            ////Arrange
        //            //MyPlugin myPlugin = new MyPlugin();
        //            //myPlugin.StartDriver();

        //            ////Act
        //            //myPlugin.CloseDriver();

        //            ////assert
        //            //Assert.AreEqual(false, myPlugin.IsRunning, "IsRunning = false");
        //        }

        //[TestMethod]
        //public void RunAction()
        //{
        //    //Arrange
        //    MyService myService = new MyService();
        //    GingerAction GA = new GingerAction();

        //    //Act
        //    myService.Sum(GA, 2, 3);

        //    //assert
        //    Assert.AreEqual(null, GA.Errors, "Errors=null");
        //    //Assert.AreEqual(GA.Output["a"], "2", "a");
        //    //Assert.AreEqual(GA.Output["b"], "3", "b");
        //    //Assert.AreEqual(GA.Output["Total"], "5", "output Total");
        //}      

        [TestMethod]
        public void RunAction()
        {
            //Arrange
            SampleService1 myService = new SampleService1();
            GingerAction GA = new GingerAction();

            //Act
            myService.Concat(GA, "","aa", "", "");

            //assert
            Assert.AreEqual(null, GA.Errors, "Errors=null");
            //Assert.AreEqual(GA.Output["a"], "2", "a");
            //Assert.AreEqual(GA.Output["b"], "3", "b");
            //Assert.AreEqual(GA.Output["Total"], "5", "output Total");
        }
    }
}
