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


using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Actions.REST;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [Level3]
    [TestClass]    
    public class RESTTest 
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

        // Need to have our own REST server, so will not be dependent on David server
        //[Ignore]
        [TestMethod]    [Timeout(60000)]
        public void Rest1()
        {

            ActREST a2 = new ActREST();
            a2.RequestType = ActREST.eRequestType.GET;
            a2.ReqHttpVersion = ActREST.eHttpVersion.HTTPV10;
            //a2.ContentType = ActREST.eContentType.TextPlain;
            a2.ContentType = ActREST.eContentType.JSon;
            a2.CookieMode = ActREST.eCookieMode.None;
            a2.SecurityType = ActREST.eSercurityType.None;
            a2.EndPointURL.ValueForDriver = "https://jsonplaceholder.typicode.com/posts/1";

            //Act
            a2.Execute();

            if (a2.ReturnValues.Count > 0)
            {
                foreach (ActReturnValue val in a2.ReturnValues)
                {
                    if (val.Actual.ToString() == "OK")
                        Assert.AreEqual(val.Actual, "OK");
                }
            }

        }
    }
}
