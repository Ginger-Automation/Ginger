#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UnitTests.NonUITests.AutoPilot
{
    [TestClass]
   public class ImportOptionalValuesTest
    {
        static ImportOptionalValuesForParameters ImportOptionalValues = null;
        static ObservableList<ApplicationAPIModel> createPaymentProfileModels;
        static string createPaymentProfileFileName;

        [ClassInitialize]
        public static void SetupTests(TestContext tc)
        {
            ImportOptionalValues = new ImportOptionalValuesForParameters();
            ImportOptionalValues.ShowMessage = false;
        }
        [Level2]
        [TestMethod]
        public void ImportOptionalFromExcelRegressionTest()
        {/*
            createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"AutoPilot\ImportOptionalValue\createPayment.xml");
            ImportOptionalValues.CreateParser(createPaymentProfileFileName);
            APIConfigurationsDocumentParserBase parser = ImportOptionalValues.CurrentParser;
     
            createPaymentProfileModels = parser.ParseDocument(createPaymentProfileFileName);
            ImportOptionalValues.ExcelFileName = TestResources.GetTestResourcesFile(@"AutoPilot\ImportOptionalValue\CretaePaymentOptinalValues.xlsx");
            ImportOptionalValues.ExcelSheetName = "wsdl_createPayment";
            DataTable dt = ImportOptionalValues.GetExceSheetlData();
            Dictionary<string, List<string>>  dic = ImportOptionalValues.UpdateParametersOptionalValuesFromCurrentExcelTable();
            ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(createPaymentProfileModels[0], createPaymentProfileModels[0].AppModelParameters.ToList(), dic);

            Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 26, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList.Count, 7, "Optional Values Count After Import From Excel");
            Assert.AreEqual(dic.Count, 9, "Count of Parameters With New Optional Values");
        */
        }
        [Level2]
        [TestMethod]
        public void ImportOptionalFromDBRegressionTest()
        {
           /* createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"AutoPilot\ImportOptionalValue\createPayment.xml");
            ImportOptionalValues.CreateParser(createPaymentProfileFileName);
            APIConfigurationsDocumentParserBase parser = ImportOptionalValues.CurrentParser;

            ObservableList<ApplicationAPIModel> createPaymentProfileModels = parser.ParseDocument(createPaymentProfileFileName);
           
            if (ImportOptionalValues.Connect())
            {
                string query = @"SELECT PARAM_1 AS ""{MESSAGETAG}"", PARAM_2 AS ""{APPLICATIONID}"" FROM ginger_importoptionalvalue";
                ImportOptionalValues.ExecuteFreeSQL(query);
                Dictionary<string, List<string>> dic =  ImportOptionalValues.UpdateParametersOptionalValuesFromDB();
                ImportOptionalValues.PopulateExcelDBOptionalValuesForAPIParametersExcelDB(createPaymentProfileModels[0], createPaymentProfileModels[0].AppModelParameters.ToList(), dic);
            }
            Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 26, "AppModelParameters count");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[0].OptionalValuesList.Count, 3, "First Optional Values Count After Import From DB");
            Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[1].OptionalValuesList.Count, 2, "Second Optional Values Count After Import From DB");
    */   
    }
    }
}
