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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace GingerCoreNETUnitTest.SolutionTestsLib
{
    [TestClass]
    [Level1]
    public class ApplicationAPIModelTest
    {
        static SolutionRepository SR;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {            

            string TempSolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\APIModelsTest");
            if (Directory.Exists(TempSolutionFolder))
            {
                Directory.Delete(TempSolutionFolder, true);
            }
            SR = new SolutionRepository();

            SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", @"~\Applications Models\API Models", true, "API Models", PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
            // SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", @"~\Applications Models\Global Models Parameters", true, "Global Model Parameters", addToRootFolders: true, PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));

            SR.CreateRepository(TempSolutionFolder);

            NewRepositorySerializer RS = new NewRepositorySerializer();
            NewRepositorySerializer.AddClassesFromAssembly(typeof(ApplicationAPIModel).Assembly);
            SR.Open(TempSolutionFolder);            
        }

        [TestCleanup]
        public void TestCleanUp()
        {
        
        }

        [TestMethod]
        public void VerifyApplicationAPIModelFileExtension()
        {
            // Need to verify ext is coming from ApplicationAPIModel and file name will not have ApplicationAPIModel 

            // Arrange
            ApplicationAPIModel AAMS1 = new ApplicationAPIModel();

            // Act
            string ext = AAMS1.ObjFileExt;

            //Assert            
            Assert.AreEqual(ext, "Ginger.ApplicationAPIModel");
        }

        [TestMethod]
        public void AddAPIFromXMLAndAvoidDuplicateNodesTest()
        {
            //Arrange
            string XmlfFilePath = TestResources.GetTestResourcesFile(@"XML\createPaymentRequest2.xml");
            ObservableList<ApplicationAPIModel> AAMTempList = new ObservableList<ApplicationAPIModel>();
            XmlDocument doc = new XmlDocument();
            List<XMLDocExtended> xmlElements = new List<XMLDocExtended>();
            List<XMLDocExtended> xmlElementsAvoidDuplicatesNodes = new List<XMLDocExtended>();
            List<AppModelParameter> AppModelParameters = new List<AppModelParameter>();
            List<AppModelParameter> AppModelParametersAvoidNodes = new List<AppModelParameter>();

            //Act
            AAMTempList = new XMLTemplateParser().ParseDocument(XmlfFilePath, AAMTempList, false);
            doc.LoadXml(AAMTempList[0].RequestBody);
            xmlElements = new XMLDocExtended(doc).GetAllNodes().Where(x => x.Name == "tag").ToList();
            AppModelParameters = AAMTempList[0].AppModelParameters.Where(x => x.TagName == "tag").ToList();

            AAMTempList = new XMLTemplateParser().ParseDocument(XmlfFilePath, AAMTempList, true);
            doc.LoadXml(AAMTempList[0].RequestBody);
            xmlElementsAvoidDuplicatesNodes = new XMLDocExtended(doc).GetAllNodes().Where(x => x.Name == "tag").ToList();
            AppModelParametersAvoidNodes = AAMTempList[0].AppModelParameters.Where(x => x.TagName == "tag").ToList();

            //Assert
            Assert.AreEqual(xmlElements.Count, 4);
            Assert.AreEqual(xmlElementsAvoidDuplicatesNodes.Count, 1);
            Assert.AreEqual(AppModelParameters.Count, 4);
            Assert.AreEqual(AppModelParametersAvoidNodes.Count, 1);
        }

        [TestMethod]
        public void AddAPIFromJSONAndAvoidDuplicateNodesTest()
        {
            //Arrange
            string JsonFilePath = TestResources.GetTestResourcesFile(@"JSON\Request JSON.TXT");
            ObservableList<ApplicationAPIModel> AAMTempList = new ObservableList<ApplicationAPIModel>();
            List<JsonExtended> jsonElements = new List<JsonExtended>();
            List<JsonExtended> jsonElementsAvoidDuplicatesNodes = new List<JsonExtended>();
            List<AppModelParameter> AppModelParameters = new List<AppModelParameter>();
            List<AppModelParameter> AppModelParametersAvoidDuplicatesNodes = new List<AppModelParameter>();

            //Act
            AAMTempList = new JSONTemplateParser().ParseDocument(JsonFilePath, AAMTempList, false);
            jsonElements = new JsonExtended(AAMTempList[0].RequestBody).GetAllNodes().Where(x => x.Name == "id").ToList();
            AppModelParameters = AAMTempList[0].AppModelParameters.Where(x => x.TagName == "id").ToList();

            AAMTempList = new JSONTemplateParser().ParseDocument(JsonFilePath, AAMTempList, true);
            jsonElementsAvoidDuplicatesNodes = new JsonExtended(AAMTempList[0].RequestBody).GetAllNodes().Where(x => x.Name == "id").ToList();
            AppModelParametersAvoidDuplicatesNodes = AAMTempList[0].AppModelParameters.Where(x => x.TagName == "id").ToList();

            //Assert
            Assert.AreEqual(jsonElements.Count, 499);
            Assert.AreEqual(jsonElementsAvoidDuplicatesNodes.Count, 1);
            Assert.AreEqual(AppModelParameters.Count, 499);
            Assert.AreEqual(AppModelParametersAvoidDuplicatesNodes.Count, 1);
        }



        [TestMethod]
        public void ApplicationAPIModelVerifySavedFile()
        {
            // Arrange
            ApplicationAPIModel AAMS1 = new ApplicationAPIModel();
            //AAMS1.GroupName = "Group1";
            AAMS1.Name = "Group1_Operation1";
            SR.AddRepositoryItem(AAMS1);            

            //Act            
            ObservableList<ApplicationAPIModel> AAMBList = SR.GetAllRepositoryItems<ApplicationAPIModel>();
            ApplicationAPIModel AAMS2 = (ApplicationAPIModel)(from x in AAMBList where x.Guid == AAMS1.Guid select x).FirstOrDefault();

            NewRepositorySerializer RS = new NewRepositorySerializer();
            ApplicationAPIModel ApplicationAPIModelFromDisk = (ApplicationAPIModel)RS.DeserializeFromFile(AAMS1.FilePath);

            //Assert
            Assert.IsTrue(AAMS2 != null, "New API Model found in AllItems");
            Assert.AreEqual(AAMS1, AAMS2, "Same object is retrieved");
            
            Assert.AreEqual(AAMS1.Name, ApplicationAPIModelFromDisk.Name, "Name saved to file");
            Assert.AreEqual(AAMS1.Guid, ApplicationAPIModelFromDisk.Guid, "Guid saved to file");
            // TODO: can add more validation

        }

        [TestMethod]
        public void ApplicationAPIModelMixSoapAndRestSaveAndLoad()
        {
            // Arrange
            ApplicationAPIModel AAMS1 = new ApplicationAPIModel();
            //AAMS1.GroupName = "Group1";
            AAMS1.Name = "Group1_Operation1";
            SR.AddRepositoryItem(AAMS1);

            ApplicationAPIModel AAMR1 = new ApplicationAPIModel();
            //AAMR1.GroupName = "Group2";
            AAMR1.Name = "Group2_Operation1";
            SR.AddRepositoryItem(AAMR1);

            
            //Act
            ObservableList<ApplicationAPIModel> AAMBList = SR.GetAllRepositoryItems<ApplicationAPIModel>();
            ApplicationAPIModel AAMS2 = (ApplicationAPIModel)(from x in AAMBList where x.Guid == AAMS1.Guid select x).FirstOrDefault();
            ApplicationAPIModel AAMR2 = (ApplicationAPIModel)(from x in AAMBList where x.Guid == AAMR1.Guid select x).FirstOrDefault();

            //Assert
            Assert.IsTrue(AAMS2 != null, "API SOAP Model loaded from disk");
            Assert.IsTrue(AAMR2 != null, "API REST Model loaded from disk");

        }

        
        [TestMethod]
        public void ApplicationAPIModelMultipleSoapAndRestSaveAndLoad()
        {
            // Arrange
            RepositoryFolder<ApplicationAPIModel> RFRoot = SR.GetRepositoryItemRootFolder<ApplicationAPIModel>();
            RepositoryFolder<ApplicationAPIModel> subFolder = (RepositoryFolder<ApplicationAPIModel>)RFRoot.AddSubFolder("SecondFolder");

            //Act
            //add items to root folder
            ApplicationAPIModel AAMS1 = new ApplicationAPIModel() { Name = "Group1_Operation1" };
            SR.AddRepositoryItem(AAMS1);
            ApplicationAPIModel AAMS2 = new ApplicationAPIModel() { Name = "Group1_Operation2" };
            SR.AddRepositoryItem(AAMS2);
            ApplicationAPIModel AAMS3 = new ApplicationAPIModel() { Name = "Group1_Operation3" };
            SR.AddRepositoryItem(AAMS3);
            //add items to sub folder items
            ApplicationAPIModel AAMR1 = new ApplicationAPIModel() { Name = "Group2_Operation1" };
            subFolder.AddRepositoryItem(AAMR1);
            ApplicationAPIModel AAMR2 = new ApplicationAPIModel() { Name = "Group2_Operation2" };
            subFolder.AddRepositoryItem(AAMR2);
            ApplicationAPIModel AAMR3 = new ApplicationAPIModel() { Name = "Group2_Operation3" };
            subFolder.AddRepositoryItem(AAMR3);
            
            ObservableList<ApplicationAPIModel> AAMBList = SR.GetAllRepositoryItems<ApplicationAPIModel>();          
            RepositoryFolder<ApplicationAPIModel> SecondFolder = RFRoot.GetSubFolder("SecondFolder");
            ObservableList<ApplicationAPIModel> AAMBListSubFolder = SecondFolder.GetFolderItems();


            //Assert
            // Assert.AreEqual(AAMBList.Count, 6, "All appllication models including sub folders"); - cannot compare since we run several tests in parallel
            Assert.AreEqual(AAMBListSubFolder.Count,3, "Second Folder should have 3 files");
        }

        [TestMethod]
        public void APIParsingSavingAndPulling()
        {
            // Arrange

            //Act

            ApplicationAPIModel AAMS1 = new ApplicationAPIModel();
            AAMS1.Name = "Soap1";
            AAMS1.Description = "Description";
            AAMS1.EndpointURL = "EndpointURL";
            AAMS1.ReqHttpVersion = ApplicationAPIUtils.eHttpVersion.HTTPV10;
            AppModelParameter AMDP = new AppModelParameter() { PlaceHolder = "placeholder", Description = "Description" };
            OptionalValue OV1 = new OptionalValue("Value1");
            OptionalValue OV2 = new OptionalValue("Value2");
            AMDP.OptionalValuesList.Add(OV1);
            AMDP.OptionalValuesList.Add(OV2);
            AAMS1.AppModelParameters = new ObservableList<AppModelParameter>();
            AAMS1.AppModelParameters.Add(AMDP);

            APIModelKeyValue AMKV = new APIModelKeyValue("param", "value");

            //AAMS1.APIModelKeyValueHeaders = new ObservableList<APIModelKeyValue>();
            //AAMS1.APIModelKeyValueHeaders.Add(AMKV);

            AAMS1.NetworkCredentials = ApplicationAPIUtils.eNetworkCredentials.Custom;
            AAMS1.URLUser = "URLUser";
            AAMS1.URLDomain = "URLDomain";
            AAMS1.URLPass = "URLPass";
            AAMS1.DoNotFailActionOnBadRespose = true;


            AAMS1.HttpHeaders = new ObservableList<APIModelKeyValue>();
            AAMS1.HttpHeaders.Add(AMKV);
            AAMS1.RequestBodyType = ApplicationAPIUtils.eRequestBodyType.FreeText;
            AAMS1.CertificateType = ApplicationAPIUtils.eCretificateType.AllSSL;
            AAMS1.CertificatePath = "CertificatePath";
            AAMS1.ImportCetificateFile = true;
            AAMS1.CertificatePassword = "CertificatePassword";
            AAMS1.SecurityType = ApplicationAPIUtils.eSercurityType.Ssl3;
            AAMS1.AuthorizationType = ApplicationAPIUtils.eAuthType.BasicAuthentication;

            AAMS1.TemplateFileNameFileBrowser = "TemplateFileNameFileBrowser";
            AAMS1.ImportRequestFile = "ImportRequestFile";
            AAMS1.AuthUsername = "AuthUsername";
            AAMS1.AuthPassword = "AuthPassword";

            //APIParameter APIP = new APIParameter("parameterName", "parameterType", 1, 2);
            //AAMS1.ParametersList = new ObservableList<APIParameter>();
            //AAMS1.ParametersList.Add(APIP);
            AAMS1.SOAPAction = "SOAPAction";

            SR.AddRepositoryItem(AAMS1);

            //SR.ClearRepositoryItemsCache<ApplicationAPIModel>();    //TODO: we need to make sure this test run as stand alone or it will mess other UT

            ObservableList<ApplicationAPIModel> AAMBList = SR.GetAllRepositoryItems<ApplicationAPIModel>();
            ApplicationAPIModel AAMS2 = (ApplicationAPIModel)(from x in AAMBList where x.Guid == AAMS1.Guid select x).FirstOrDefault();

            // TODO: change 'Value Check' to the name of the field

            //Assert
            Assert.AreEqual(AAMS2.Name, "Soap1", "Value Check");
            Assert.AreEqual(AAMS2.Description, "Description", "Value Check");
            Assert.AreEqual(AAMS2.EndpointURL, "EndpointURL", "Value Check");
            Assert.AreEqual(AAMS2.ReqHttpVersion, ApplicationAPIUtils.eHttpVersion.HTTPV10, "Value Check");
            Assert.AreEqual(AAMS2.AppModelParameters.Count, 1, "Value Check");
            Assert.AreEqual(AAMS2.AppModelParameters[0].OptionalValuesList.Count, 2, "Value Check");
            //Assert.AreEqual(AAMS2.APIModelKeyValueHeaders.Count, 1, "Value Check");
            Assert.AreEqual(AAMS2.NetworkCredentials, ApplicationAPIUtils.eNetworkCredentials.Custom, "Value Check");
            Assert.AreEqual(AAMS2.URLUser, "URLUser", "Value Check");
            Assert.AreEqual(AAMS2.URLDomain, "URLDomain", "Value Check");
            Assert.AreEqual(AAMS2.URLPass, "URLPass", "Value Check");
            Assert.AreEqual(AAMS2.DoNotFailActionOnBadRespose, true, "Value Check");
            Assert.AreEqual(AAMS2.HttpHeaders.Count, 1, "Value Check");
            Assert.AreEqual(AAMS2.RequestBodyType, ApplicationAPIUtils.eRequestBodyType.FreeText, "Value Check");
            Assert.AreEqual(AAMS2.CertificateType, ApplicationAPIUtils.eCretificateType.AllSSL, "Value Check");
            Assert.AreEqual(AAMS2.CertificatePath, "CertificatePath", "Value Check");
            Assert.AreEqual(AAMS2.ImportCetificateFile, true, "Value Check");
            Assert.AreEqual(AAMS2.CertificatePassword, "CertificatePassword", "Value Check");
            Assert.AreEqual(AAMS2.SecurityType, ApplicationAPIUtils.eSercurityType.Ssl3, "Value Check");
            Assert.AreEqual(AAMS2.AuthorizationType, ApplicationAPIUtils.eAuthType.BasicAuthentication, "Value Check");
            Assert.AreEqual(AAMS2.TemplateFileNameFileBrowser, "TemplateFileNameFileBrowser", "Value Check");
            Assert.AreEqual(AAMS2.ImportRequestFile, "ImportRequestFile", "Value Check");
            Assert.AreEqual(AAMS2.AuthUsername, "AuthUsername", "Value Check");
            Assert.AreEqual(AAMS2.AuthPassword, "AuthPassword", "Value Check");
            //Assert.AreEqual(AAMS2.ParametersList.Count, 1, "Value Check");
            Assert.AreEqual(AAMS2.SOAPAction, "SOAPAction", "Value Check");

        }
    }
}
