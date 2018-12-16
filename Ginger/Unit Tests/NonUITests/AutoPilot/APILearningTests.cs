using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerWPF.ApplicationModelsLib.APIModels;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System.Threading.Tasks;
using GingerTestHelper;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib.APIModelLib;

namespace UnitTests.NonUITests.AutoPilot
{
    /// <summary>
    /// Summary description for APILearningTests
    /// </summary>
    [TestClass]
    public class APILearningTests
    {

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void APILearnWSDLStockSimbolTest()
        {
            //Arrange
            WSDLParser wsdlParser = new WSDLParser();
            ObservableList<ApplicationAPIModel> AAMSList = new ObservableList<ApplicationAPIModel>();

            //Act
            Learn(AAMSList, wsdlParser);
            while (AAMSList.Count < 6)
            {
                //Let it learn
            }


            //Assert
            Assert.AreEqual(AAMSList.Count, 6, "Is API's equal to 6");
            Assert.AreEqual(AAMSList[0].EndpointURL, "http://ws.cdyne.com/delayedstockquote/delayedstockquote.asmx", "Is EndpointURL equal");
            Assert.AreEqual(AAMSList[0].SOAPAction, "http://ws.cdyne.com/GetQuote", "Is SOAPAction equal");
            Assert.AreEqual(String.IsNullOrEmpty(AAMSList[0].RequestBody), false, "Is body not empty equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters.Count, 2, "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[0].PlaceHolder, "{STOCKSYMBOL}", "Is parameter name equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[1].PlaceHolder, "{LICENSEKEY}", "Is parameter name equal");
        }

        private async void Learn(ObservableList<ApplicationAPIModel> AAMSList, WSDLParser wsdlParser)
        {
            string path = TestResources.GetTestResourcesFile(@"AutoPilot\WSDLs\delayedstockquote.xml");
            await Task.Run(() => wsdlParser.ParseDocument(path, AAMSList));
        }

        [TestMethod]
        public void XMLTemplateParesrCreatePaymentProfile()
        {
            //Arrange                        
            XMLTemplateParser xMLTemplateParser = new XMLTemplateParser();
            string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"AutoPilot\XMLTemplates\bankCode.xml");

            //Act   
            ObservableList<ApplicationAPIModel> createPaymentProfileModels = new ObservableList<ApplicationAPIModel>();
            xMLTemplateParser.ParseDocument(createPaymentProfileFileName, createPaymentProfileModels);
            TemplateFile TempleteFile = new TemplateFile() { FilePath = createPaymentProfileFileName };
            createPaymentProfileModels[0].OptionalValuesTemplates.Add(TempleteFile);
            Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
            ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
            ImportOptionalValues.ShowMessage = false;
            ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(createPaymentProfileModels[0], OptionalValuesPerParameterDict);
            ImportOptionalValues.PopulateOptionalValuesForAPIParameters(createPaymentProfileModels[0], OptionalValuesPerParameterDict);

            //Assert
            Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 1, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].PlaceHolder, "{BLZ}", "PlaceHolder name check");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList.Count, 1, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList[0].Value, "46451012", "OptionalValue check");
        }

        [TestMethod]
        public void JSONTemplateCreateBillingArrangement()
        {
            //Arrange                        
            JSONTemplateParser jSONTemplateParser = new JSONTemplateParser();
            string createBillingArrangementFileName = TestResources.GetTestResourcesFile(@"AutoPilot\JSONTemplates\login.json");

            //Act   
            ObservableList<ApplicationAPIModel> createPaymentProfileModels = new ObservableList<ApplicationAPIModel>();
            jSONTemplateParser.ParseDocument(createBillingArrangementFileName, createPaymentProfileModels);
            TemplateFile TempleteFile = new TemplateFile() { FilePath = createBillingArrangementFileName };
            createPaymentProfileModels[0].OptionalValuesTemplates.Add(TempleteFile);
            Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
            ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
            ImportOptionalValues.ShowMessage = false;
            ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(createPaymentProfileModels[0], OptionalValuesPerParameterDict);
            ImportOptionalValues.PopulateOptionalValuesForAPIParameters(createPaymentProfileModels[0], OptionalValuesPerParameterDict);

            //Assert
            Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 2, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].PlaceHolder, "<USER>", "PlaceHolder name check");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[1].PlaceHolder, "<PASSWORD>", "PlaceHolder name check");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList.Count, 1, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList[0].Value, "restOwner", "OptionalValue check");
        }

        [TestMethod]
        public void APILearnSwaggerMetroTest()
        {
            //Arrange
            SwaggerParser wsdlParser = new SwaggerParser();
            ObservableList<ApplicationAPIModel> AAMSList = new ObservableList<ApplicationAPIModel>();

            //Act
            string path = TestResources.GetTestResourcesFile(@"AutoPilot\Swagger\swagger.json");
            wsdlParser.ParseDocument(path, AAMSList);


            //Assert
            Assert.AreEqual(AAMSList.Count, 22, "Is API's equal to 6");
            Assert.AreEqual(AAMSList[0].EndpointURL, "https://petstore.swagger.io/v2/pet", "Is EndpointURL equal");
            Assert.AreEqual(String.IsNullOrEmpty(AAMSList[0].RequestBody), false, "Is body not empty equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters.Count, 7, "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[0].PlaceHolder, "<ID>", "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[1].PlaceHolder, "<CATEGORY>", "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[2].PlaceHolder, "<NAME>", "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[3].PlaceHolder, "<PHOTOURLS[0]>", "are parameters are equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[4].PlaceHolder, "<ID1>", "Is parameter name equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[5].PlaceHolder, "<NAME1>", "Is parameter name equal");
            Assert.AreEqual(AAMSList[0].AppModelParameters[6].PlaceHolder, "<STATUS>", "Is parameter name equal");
        }
    }
}
