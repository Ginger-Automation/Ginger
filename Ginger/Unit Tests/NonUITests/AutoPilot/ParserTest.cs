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
using Amdocs.Ginger.Common.APIModelLib;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using GingerTestHelper;
using GingerWPF.ApplicationModelsLib.APIModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests.NonUITests.AutoPilot
{

    [TestClass]
    public class ParserTest
    {

        //[TestMethod]  [Timeout(60000)]
        //public void WSDLParserRechargeServices()
        //{
        //    //Arrange                        
        //    WSDLParser wSDLParser = new WSDLParser();
        //    string RechargeServicesFileName = TestResources.GetTestResourcesFile(@"AutoPilot\WSDLs\RechargeServices.xml");

        //    //Act            
        //    ObservableList<ApplicationAPIModel> RechargeServicesAPIModels = wSDLParser.ParseDocument(RechargeServicesFileName);

        //    //Assert
        //    Assert.AreEqual(RechargeServicesAPIModels.Count, 8, "APIModels count");
        //    Assert.AreEqual(RechargeServicesAPIModels[0].EndpointURL, "http://mwhlvsp319:42013/axis/services/RechargeServices", "EndpointURL validation");
        //    Assert.AreEqual(RechargeServicesAPIModels[0].AppModelParameters.Count, 41, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[1].AppModelParameters.Count, 56, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[2].AppModelParameters.Count, 59, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[3].AppModelParameters.Count, 59, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[4].AppModelParameters.Count, 53, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[5].AppModelParameters.Count, 84, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[6].AppModelParameters.Count, 61, "AppModelParameters count");
        //    Assert.AreEqual(RechargeServicesAPIModels[7].AppModelParameters.Count, 37, "AppModelParameters count");
        //}



        //[TestMethod]  [Timeout(60000)]
        //public void XMLTemplateParesrCreatePaymentProfile()
        //{
        //    //Arrange                        
        //    XMLTemplateParser xMLTemplateParser = new XMLTemplateParser();            
        //    string createPaymentProfileFileName = TestResources.GetTestResourcesFile(@"AutoPilot\XMLTemplates\createPaymentProfile_request_2.xml");

        //    //Act   
        //    ObservableList<ApplicationAPIModel> createPaymentProfileModels = xMLTemplateParser.ParseDocument(createPaymentProfileFileName);
        //    TemplateFile TempleteFile = new TemplateFile() { FilePath = createPaymentProfileFileName };
        //    createPaymentProfileModels[0].OptionalValuesTemplates.Add(TempleteFile);
        //    Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
        //    ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
        //    ImportOptionalValues.ShowMessage = false;
        //    ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(createPaymentProfileModels[0], OptionalValuesPerParameterDict);
        //    ImportOptionalValues.PopulateOptionalValuesForAPIParameters(createPaymentProfileModels[0], OptionalValuesPerParameterDict);

        //    //Assert
        //    Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
        //    Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 26, "AppModelParameters count");
        //    Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[1].OptionalValuesList.Count, 1, "AppModelParameters count");
        //}

        //[TestMethod]  [Timeout(60000)]
        //public void JSONTemplateCREATE_BILLING_ARRANGEMENT()
        //{
        //    //Arrange                        
        //    JSONTemplateParser jSONTemplateParser = new JSONTemplateParser();
        //    string createBillingArrangementFileName = TestResources.GetTestResourcesFile(@"AutoPilot\JSONTemplates\CREATE_BILLING_ARRANGEMENT_3_12_2015_15_33_47.txt");

        //    //Act   
        //    ObservableList<ApplicationAPIModel> createPaymentProfileModels = jSONTemplateParser.ParseDocument(createBillingArrangementFileName);
        //    TemplateFile TempleteFile = new TemplateFile() { FilePath = createBillingArrangementFileName };
        //    createPaymentProfileModels[0].OptionalValuesTemplates.Add(TempleteFile);
        //    Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict = new Dictionary<Tuple<string, string>, List<string>>();
        //    ImportOptionalValuesForParameters ImportOptionalValues = new ImportOptionalValuesForParameters();
        //    ImportOptionalValues.ShowMessage = false;
        //    ImportOptionalValues.GetAllOptionalValuesFromExamplesFiles(createPaymentProfileModels[0], OptionalValuesPerParameterDict);
        //    ImportOptionalValues.PopulateOptionalValuesForAPIParameters(createPaymentProfileModels[0], OptionalValuesPerParameterDict);

        //    //Assert
        //    Assert.AreEqual(createPaymentProfileModels.Count, 1, "APIModels count");
        //    Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters.Count, 7, "AppModelParameters count");
        //    Assert.AreEqual(createPaymentProfileModels[0].AppModelParameters[5].OptionalValuesList.Count, 1, "AppModelParameters count");
        //}
    }
}
