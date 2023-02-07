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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.NonUITests.AutoPilot
{
    [TestClass]
    public class SwaggetTests
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

        [Level2]
        [TestMethod]  [Timeout(60000)]
        public void SwaggerParseCheckHeaderAndParamsCount()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "petstore_swagger.json");
            ObservableList<ApplicationAPIModel> requests = new ObservableList<ApplicationAPIModel>();

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            ApplicationAPIModel RequestToTest = requests.Where(x => x.Name == @"Deletes a pet").ElementAt(0);

            //Assert
            Assert.AreEqual(1, RequestToTest.HttpHeaders.Count());


             RequestToTest = requests.Where(x => x.Name == @"Find purchase order by ID").ElementAt(0);
            Assert.AreEqual(6, RequestToTest.ReturnValues.Count(), "SwaggerCheckResponseParameterCount");


            RequestToTest = requests.Where(x => x.Name == @"Place an order for a pet").ElementAt(0);
            string requestBody= @"{"+ Environment.NewLine+"\"id\": <ID>,"+ Environment.NewLine+"\"petId\": <PETID>,"+ Environment.NewLine+"\"quantity\": <QUANTITY>,"+Environment.NewLine+"\"shipDate\": \"<SHIPDATE>\","+ Environment.NewLine + "\"status\": \"<STATUS>\","+ Environment.NewLine+"\"complete\": <COMPLETE>"+ Environment.NewLine+"}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");
        }

    }
}