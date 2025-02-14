#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

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


        [TestMethod]
        [Timeout(60000)]
        public void Swagger2JsonCheckHeaderAndParamsCount()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "petstore_swagger.json");
            ObservableList<ApplicationAPIModel> requests = [];

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            ApplicationAPIModel RequestToTest = requests.Where(x => x.Name == @"Deletes a pet").ElementAt(0);

            //Assert
            Assert.AreEqual(1, RequestToTest.HttpHeaders.Count);


            RequestToTest = requests.Where(x => x.Name == @"Find purchase order by ID").ElementAt(0);
            Assert.AreEqual(6, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");


            RequestToTest = requests.Where(x => x.Name == @"Place an order for a pet").ElementAt(0);
            string requestBody = @"{" + Environment.NewLine + "\"id\": <ID>," + Environment.NewLine + "\"petId\": <PETID>," + Environment.NewLine + "\"quantity\": <QUANTITY>," + Environment.NewLine + "\"shipDate\": \"<SHIPDATE>\"," + Environment.NewLine + "\"status\": \"<STATUS>\"," + Environment.NewLine + "\"complete\": <COMPLETE>" + Environment.NewLine + "}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");
        }

        [TestMethod]
        [Timeout(60000)]
        public void OpenApi3JsonCheckHeaderAndParamsCount()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "petstore_versionthree.json");
            ObservableList<ApplicationAPIModel> requests = [];
            string requestBody;

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            ApplicationAPIModel RequestToTest = requests.Where(x => x.Name == @"Deletes a pet").ElementAt(0);

            //Assert
            Assert.AreEqual(1, RequestToTest.HttpHeaders.Count);


            RequestToTest = requests.Where(x => x.Name == @"Find purchase order by ID").ElementAt(0);
            Assert.AreEqual(6, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");

            RequestToTest = requests.Where(x => x.Name == @"Add a new pet to the store-JSON").ElementAt(0);
            Assert.AreEqual(8, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");
            Assert.AreEqual("/pet", RequestToTest.EndpointURL);

            RequestToTest = requests.Where(x => x.Name == @"Place an order for a pet-JSON").ElementAt(0);
            requestBody = @"{" + Environment.NewLine + "\"id\": <ID>," + Environment.NewLine + "\"petId\": <PETID>," + Environment.NewLine + "\"quantity\": <QUANTITY>," + Environment.NewLine + "\"shipDate\": \"<SHIPDATE>\"," + Environment.NewLine + "\"status\": \"<STATUS>\"," + Environment.NewLine + "\"complete\": <COMPLETE>" + Environment.NewLine + "}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");

            RequestToTest = requests.Where(x => x.Name == @"Create user-JSON").ElementAt(0);
            requestBody = @"{" + Environment.NewLine + "\"id\": <ID>," + Environment.NewLine + "\"username\": \"<USERNAME>\"," + Environment.NewLine + "\"firstName\": \"<FIRSTNAME>\"," + Environment.NewLine + "\"lastName\": \"<LASTNAME>\"," + Environment.NewLine + "\"email\": \"<EMAIL>\"," + Environment.NewLine + "\"password\": \"<PASSWORD>\"," + Environment.NewLine + "\"phone\": \"<PHONE>\"," + Environment.NewLine + "\"userStatus\": <USERSTATUS>" + Environment.NewLine + "}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");

        }

        [TestMethod]
        [Timeout(60000)]
        public void OpenApi3YamlCheckHeaderAndParamsCount()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "CommissionAPI.yml");
            ObservableList<ApplicationAPIModel> requests = [];

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            ApplicationAPIModel RequestToTest = requests.Where(x => x.Name == @"Get commission type list").ElementAt(0);

            //Assert
            Assert.AreEqual(6, RequestToTest.ReturnValues.Count);


            RequestToTest = requests.Where(x => x.Name == @"Create new account-JSON").ElementAt(0);
            Assert.AreEqual(3, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");

            RequestToTest = requests.Where(x => x.Name == @"Update account-JSON").ElementAt(0);
            Assert.AreEqual(3, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");
            string requestBody = @"{" + Environment.NewLine + "\"accountId\": \"<ACCOUNTID>\"," + Environment.NewLine + "\"name\": \"<NAME>\"," + Environment.NewLine + "\"parentAccount\": \"<PARENTACCOUNT>\"" + Environment.NewLine + "}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");

        }

        [TestMethod]
        [Timeout(60000)]
        public void Swagger2YamlCheckHeaderAndParamsCount()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "PetStoreOAS2.yml");
            ObservableList<ApplicationAPIModel> requests = [];

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            ApplicationAPIModel RequestToTest = requests.Where(x => x.Name == @"Deletes a pet").ElementAt(0);

            //Assert
            Assert.AreEqual(1, RequestToTest.HttpHeaders.Count);


            RequestToTest = requests.Where(x => x.Name == @"Find purchase order by ID").ElementAt(0);
            Assert.AreEqual(6, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");


            RequestToTest = requests.Where(x => x.Name == @"Place an order for a pet").ElementAt(0);
            string requestBody = @"{" + Environment.NewLine + "\"id\": <ID>," + Environment.NewLine + "\"petId\": <PETID>," + Environment.NewLine + "\"quantity\": <QUANTITY>," + Environment.NewLine + "\"shipDate\": \"<SHIPDATE>\"," + Environment.NewLine + "\"status\": \"<STATUS>\"," + Environment.NewLine + "\"complete\": <COMPLETE>" + Environment.NewLine + "}";
            Assert.AreEqual(requestBody.Replace(" ", ""), RequestToTest.RequestBody.Replace(" ", ""), "CheckResponseBody");


        }

        [TestMethod]
        [Timeout(60000)]
        public void ApiModelsOptionalValuesModelParamTest()
        {
            //Arrange
            SwaggerParser parserForBillingAccount = new SwaggerParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"Swagger" + Path.DirectorySeparatorChar + "petstore_versionthree.json");
            ObservableList<ApplicationAPIModel> requests = [];
            ObservableList<OptionalValue> optionalValueContainer;
            ApplicationAPIModel RequestToTest;

            //Act   
            requests = parserForBillingAccount.ParseDocument(createPaymentProfileFileName, requests);
            RequestToTest = requests.Where(x => x.Name == @"Create user-JSON").ElementAt(0);
            optionalValueContainer = RequestToTest.AppModelParameters.ElementAt(4).OptionalValuesList;

            //Assert
            Assert.IsTrue(optionalValueContainer.Any(item => item.Value == "john@email.com"));


            //Act
            RequestToTest = requests.Where(x => x.Name == @"Place an order for a pet-XML").ElementAt(0);
            optionalValueContainer = RequestToTest.AppModelParameters.ElementAt(1).OptionalValuesList;

            //Assert
            Assert.IsTrue(optionalValueContainer.Any(item => item.Value == "198772"));
        }
    }
}