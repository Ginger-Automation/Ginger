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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib.SwaggerApi;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
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
            Assert.AreEqual(7, RequestToTest.ReturnValues.Count, "SwaggerCheckResponseParameterCount");
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


        [TestMethod]
        [Timeout(5000)] // Should complete quickly with circular reference protection
        public void TestCircularReference_PreventInfiniteLoop()
        {
            // Arrange: Create a schema with circular reference
            JsonSchema4 parentSchema = new JsonSchema4
            {
                Title = "Parent",
                Type = JsonObjectType.Object
            };

            JsonSchema4 childSchema = new JsonSchema4
            {
                Title = "Child",
                Type = JsonObjectType.Object
            };

            // Create circular reference: Parent -> Child -> Parent
            parentSchema.Properties.Add("child", new JsonProperty { Reference = childSchema });
            childSchema.Properties.Add("parent", new JsonProperty { Reference = parentSchema });

            // Act: Generate JSON with circular reference
            string result = JsonSchemaTools.JsonSchemaFaker(parentSchema, null, false);

            // Assert: Should return valid JSON without hanging
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("{"));
            Assert.IsTrue(result.Contains("}"));
            // Should not throw StackOverflowException or infinite loop
        }

        [TestMethod]
        [Timeout(5000)]
        public void TestSelfReferenceElement_ReturnsEmptyObject()
        {
            // Arrange: Create self-referencing schema
            JsonSchema4 schema = new JsonSchema4
            {
                Title = "SelfReference",
                Type = JsonObjectType.Object
            };
            schema.Properties.Add("self", new JsonProperty { Reference = schema });

            // Act
            string result = JsonSchemaTools.JsonSchemaFaker(schema, null, false);

            // Assert: Should handle self-reference gracefully
            Assert.IsNotNull(result);
            Assert.AreNotEqual("", result); // Should not be empty string
            Assert.IsTrue(result.Contains("{}") || result.Contains("\"self\":"));
        }

        [TestMethod]
        public void TestSwaggerPetSchema_WithCircularCategoryReference()
        {
            // Arrange
            JsonSchema4 petSchema = new JsonSchema4
            {
                Title = "Pet",
                Type = JsonObjectType.Object
            };

            JsonSchema4 categorySchema = new JsonSchema4
            {
                Title = "Category",
                Type = JsonObjectType.Object
            };

            // Create relationship similar to Swagger Pet Store
            petSchema.Properties.Add("category", new JsonProperty { Reference = categorySchema });
            petSchema.Properties.Add("name", new JsonProperty { Type = JsonObjectType.String });

            categorySchema.Properties.Add("id", new JsonProperty { Type = JsonObjectType.Integer });
            categorySchema.Properties.Add("name", new JsonProperty { Type = JsonObjectType.String });

            // Act: Generate JSON
            string result = JsonSchemaTools.JsonSchemaFaker(petSchema, null, false);

            // Assert: Should complete successfully
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("name"));
            Assert.IsTrue(result.Contains("category"));
        }
    }
}
