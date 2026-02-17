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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ActionsLib.Webservices.Diameter;
using Amdocs.Ginger.CoreNET.DiameterLib;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Amdocs.Ginger.CoreNET.DiameterLib.DiameterEnums;

namespace GingerCoreNETUnitTest.Webservice.DiameterLib
{
    [TestClass]
    public class DiameterUtilsTest
    {
        private string testDirectory;
        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            // Init SR
            SolutionRepository mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            string TempRepositoryFolder = TestResources.GetTestTempFolder(Path.Combine("Solutions", "temp"));
            mSolutionRepository.Open(TempRepositoryFolder);
            Solution sol = new Solution
            {
                ContainingFolderFullPath = TempRepositoryFolder
            };
            WorkSpace.Instance.Solution = sol;
            if (WorkSpace.Instance.Solution.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
            WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder = Path.Combine(TempRepositoryFolder, "ExecutionResults");
        }
        [TestInitialize]
        public void TestInitialize()
        {
            testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(testDirectory);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(testDirectory, true);
        }
        [TestMethod]
        public void TestLoadAVPDictionary_Success()
        {
            // Arrange: Test XML file with valid content
            string xmlFilePath = Path.Combine(testDirectory, "AVPDictionary.xml");
            string xmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<DiameterAvps>\r\n\t<Avp name=\"AVP1\" code=\"264\" type=\"DiamIdent\" isMandatory=\"true\" isVendorSpecific=\"true\" enumName=\"eTest\"/>\r\n</DiameterAvps>\r\n";
            CreateTestXmlFile(xmlFilePath, xmlContent);
            string avpExpectedName = "AVP1";
            int avpExpectedCode = 264;
            eDiameterAvpDataType avpExpectedDataType = eDiameterAvpDataType.DiamIdent;
            string avpExpectedEnumName = "eTest";

            // Act: Call the LoadDictionary method with the test XML file path
            var avpDictionaryList = DiameterUtils.LoadAVPDictionary(xmlFilePath);
            var avp = avpDictionaryList[0];

            // Assert: Verify that the method returns a list of dictionary items with excatly 1 item and verify the values of that item
            Assert.IsNotNull(avpDictionaryList, "AVP dictionary list should not be null.");
            Assert.AreEqual(1, avpDictionaryList.Count, "AVP dictionary list should contain exactly 1 element");
            Assert.AreEqual(avpExpectedName, avp.Name, "AVP name should be 'AVP1'");
            Assert.AreEqual(avpExpectedCode, avp.Code, "AVP code should be 264");
            Assert.AreEqual(avpExpectedDataType, avp.AvpDataType, "AVP data type should be 'DiamIdent'");
            Assert.IsTrue(avp.IsMandatory, "AVP is mandatory should be true");
            Assert.IsTrue(avp.IsVendorSpecific, "AVP is vendor specific should be true");
            Assert.AreEqual(avpExpectedEnumName, avp.AvpEnumName, "AVP is vendor specific should be true");
        }
        [TestMethod]
        public void TestLoadAVPDictionary_FileNotFound()
        {
            // Arrange
            string nonExistingFilePath = "NonExistentFile.xml";

            // Act: Call the LoadDictionary method with a non-existent file path
            var avpDictionaryList = DiameterUtils.LoadAVPDictionary(nonExistingFilePath);

            // Assert: Verify that the method returns null due to file not found
            Assert.IsNull(avpDictionaryList, "AVP dictionary list should be null for file not found.");
        }
        [TestMethod]
        public void TestLoadAVPDictionary_InvalidXml()
        {
            // Arrange: Test XML file with invalid content
            string xmlFilePath = Path.Combine(testDirectory, "InvalidAVPDictionary.xml");
            string invalidXmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<DiameterAvps>";
            CreateTestXmlFile(xmlFilePath, invalidXmlContent);

            // Act
            var avpDictionaryList = DiameterUtils.LoadAVPDictionary(xmlFilePath);

            // Assert: Verify that the method returns null due to invalid XML
            Assert.IsNull(avpDictionaryList, "AVP dictionary list should be null for invalid XML.");
        }
        [TestMethod]
        public void TestLoadAVPDictionary_DeserializationError()
        {
            // Arrange: Test XML file with valid but incompatible content
            string xmlFilePath = Path.Combine(testDirectory, "IncompatibleAVPDictionary.xml");
            // Avp code should be integer, provide a string to cause deserialization error
            string xmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<DiameterAvps>\r\n\t<Avp name=\"Dynamic-Address-Flag\" code=\"DeserializationErrorTesting\" type=\"Enumerated\" isMandatory=\"true\" isVendorSpecific=\"true\" enumName=\"eDynamicAddressFlag\"/>\r\n</DiameterAvps>\r\n";
            CreateTestXmlFile(xmlFilePath, xmlContent);

            // Act: Call the LoadDictionary method with the test XML file path
            var avpDictionaryList = DiameterUtils.LoadAVPDictionary(xmlFilePath);

            // Assert: Verify that the method returns null due to deserialization error
            Assert.IsNull(avpDictionaryList, "AVP dictionary list should be null for deserialization error.");
        }
        [TestMethod]
        public void TestLoadAVPDictionary_EmptyDictionary()
        {
            // Arrange: Test XML file with empty content(no records)
            string xmlFilePath = Path.Combine(testDirectory, "EmptyAVPDictionary.xml");
            string xmlContent = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n<DiameterAvps>\r\n</DiameterAvps>\r\n";
            CreateTestXmlFile(xmlFilePath, xmlContent);
            int expectedDictionaryElements = 0;

            // Act: Call the LoadDictionary method with the test XML file path
            var avpDictionaryList = DiameterUtils.LoadAVPDictionary(xmlFilePath);

            // Assert: Verify that the method returns an empty list due to empty dictionary
            Assert.IsNotNull(avpDictionaryList, "AVP dictionary list should not be null for empty dictionary.");
            Assert.AreEqual(expectedDictionaryElements, avpDictionaryList.Count, "AVP dictionary list should be empty for empty dictionary.");
        }
        private void CreateTestXmlFile(string filePath, string xmlContent)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(xmlContent);
            }
        }
        [TestMethod]
        public void DeserializeAvpsPerMessageConfig_SuccessfulDeserialization()
        {
            // Arrange
            string jsonFilePath = Path.Combine(testDirectory, "ValidAvpsPerMessageConfigFile.json");
            string validConfigContent = "{ \"CER\": [\"AVP1\", \"AVP2\"], \"CCR\": [\"AVP3\", \"AVP4\"] }";
            CreateTestJsonConfigurationFile(jsonFilePath, validConfigContent);
            int expectedCERElements = 2;
            int expectedCCRElements = 2;

            // Act
            Dictionary<string, string[]> result = DiameterUtils.DeserializeAvpsPerMessageConfigFile(jsonFilePath);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ContainsKey("CER"), "AVPs per message dictionary should contain key CER");
            Assert.IsTrue(result.ContainsKey("CCR"), "AVPs per message dictionary should contain key CCR");
            Assert.AreEqual(expectedCERElements, result["CER"].Length, "The AVPs per message dictionary for key 'CER' should contain exactly 2 element.");
            Assert.AreEqual(expectedCCRElements, result["CCR"].Length, "The AVPs per message dictionary for key 'CCR' should contain exactly 2 element.");
        }

        [TestMethod]
        public void DeserializeAvpsPerMessageConfig_EmptyConfigurationFile()
        {
            // Arrange
            string jsonFilePath = Path.Combine(testDirectory, "EmptyAvpsPerMessageConfigFile.json");
            string emptyConfigContent = "{}";
            CreateTestJsonConfigurationFile(jsonFilePath, emptyConfigContent);

            // Act
            Dictionary<string, string[]> result = DiameterUtils.DeserializeAvpsPerMessageConfigFile(jsonFilePath);

            // Assert
            Assert.IsNotNull(result, "The AVPs per message dictionary should not be null");
            Assert.AreEqual(0, result.Count, "The AVPs per message dictionary should be empty for an empty configuration file");
        }
        [TestMethod]
        public void DeserializeAvpsPerMessageConfig_InvalidJsonContent()
        {
            // Arrange
            string jsonFilePath = Path.Combine(testDirectory, "InvalidJsonConfigContent.json");
            string invalidJsonContent = "Invalid JSON Content";
            CreateTestJsonConfigurationFile(jsonFilePath, invalidJsonContent);

            // Act
            Dictionary<string, string[]> result = DiameterUtils.DeserializeAvpsPerMessageConfigFile(jsonFilePath);

            // Assert
            Assert.IsNotNull(result, "The AVPs per message dictionary should not be null");
            Assert.AreEqual(0, result.Count, "The AVPs per message dictionary should be empty for invalid json content");
        }
        private void CreateTestJsonConfigurationFile(string filePath, string jsonContent)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.Write(jsonContent);
            }
        }

        [TestMethod]
        public void GetMandatoryAVPForMessage_CERMessageType_ReturnsExpectedAVPs()
        {
            // Arrange
            var messageType = eDiameterMessageType.CapabilitiesExchangeRequest;
            var expectedAVPs = new List<string>
            {
                "Vendor-Id",
                "Origin-Host",
                "Origin-Realm",
                "Host-IP-Address",
                "Product-Name",
                "Origin-State-Id"
            };
            string avpsPerMessageConfigFilename = "AvpsPerMessageConfiguration.json";
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GingerCoreNET", "DiameterLib", avpsPerMessageConfigFilename);

            // Act
            var avpList = DiameterUtils.GetMandatoryAVPForMessage(messageType, configPath);

            // Assert
            Assert.IsNotNull(avpList, "The AVP list should not be null.");
            Assert.AreEqual(6, avpList.Count, "The AVP list for CER message type should contain exactly 6 element");

            // Convert the avpList to a list of AVP names for easy comparison.
            var actualAVPs = avpList.Select(avp => avp.Name).ToList();

            CollectionAssert.AreEquivalent(expectedAVPs, actualAVPs, "The AVP list should match the expected AVPs for CER messageType.");
        }
        [TestMethod]
        public void GetMandatoryAVPForMessage_CCRMessageType_ReturnsExpectedAVPs()
        {
            // Arrange
            var messageType = eDiameterMessageType.CreditControlRequest;
            var expectedAVPs = new List<string>
            {
                "Session-Id",
                "Origin-Host",
                "Origin-Realm",
                "Destination-Realm",
                "Auth-Application-Id",
                "Service-Context-Id",
                "CC-Request-Type",
                "CC-Request-Number",
                "Destination-Host",
                "Origin-State-Id",
                "User-Name",
                "3GPP-RAT-Type",
                "Event-Timestamp"
            };
            string avpsPerMessageConfigFilename = "AvpsPerMessageConfiguration.json";
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GingerCoreNET", "DiameterLib", avpsPerMessageConfigFilename);

            // Act
            var avpList = DiameterUtils.GetMandatoryAVPForMessage(messageType, configPath);

            // Assert
            Assert.IsNotNull(avpList, "The AVP list should not be null.");
            Assert.AreEqual(13, avpList.Count, "The AVP list for CCR message type should contain exactly 13 element");

            // Convert the avpList to a list of AVP names for easy comparison.
            var actualAVPs = avpList.Select(avp => avp.Name).ToList();

            CollectionAssert.AreEquivalent(expectedAVPs, actualAVPs, "The AVP list should match the expected AVPs for CCR messageType.");
        }
        [TestMethod]
        public void GetMandatoryAVPForMessage_ReturnsEmptyList()
        {
            // Arrange
            var messageType = eDiameterMessageType.None;

            string avpsPerMessageConfigFilename = "AvpsPerMessageConfiguration.json";
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GingerCoreNET", "DiameterLib", avpsPerMessageConfigFilename);

            // Act
            var avpList = DiameterUtils.GetMandatoryAVPForMessage(messageType, configPath);

            // Assert
            Assert.IsNotNull(avpList, "The AVP list should not be null.");
            Assert.AreEqual(0, avpList.Count, "The AVP list for undefined message type should be empty");
        }

        [TestMethod]
        public void ConstructDiameterRequest_Success()
        {
            // Arrange
            ActDiameter actDiameter = CreateActDiameter();
            DiameterUtils diameterUtils = new DiameterUtils(new DiameterMessage());

            // Act
            bool result = diameterUtils.ConstructDiameterRequest(actDiameter);

            // Assert
            Assert.IsTrue(result, "ConstructDiameterRequest should return true for success.");

            // Assert that SetMessageProperty worked as expected and the Message properties are set to the expected values
            Assert.AreEqual(1, diameterUtils.Message.CommandCode, $"Message property 'CommandCode' should be set to '1'");
            Assert.IsTrue(diameterUtils.Message.IsRequestBitSet, $"Message property 'IsRequestBit' should be set to 'true'");
            Assert.AreEqual("Credit Control Request", diameterUtils.Message.Name, $"Message property 'Name' should be set to 'Credit Control Request'");
            Assert.AreEqual(2, diameterUtils.Message.AvpList.Count, $"Message property 'AvpList' should have exactly 2 elements");

            var actualAVPList = diameterUtils.Message.AvpList;
            CollectionAssert.AreEquivalent(actDiameter.RequestAvpList, actualAVPList, "Message property 'AvpList' should match the expected AVP list");
        }

        private ActDiameter CreateActDiameter()
        {
            ActDiameter actDiameter = new ActDiameter();
            GingerRunner gingerRunner = new GingerRunner();
            gingerRunner.Executor = new GingerExecutionEngine(gingerRunner);
            Context context = new Context { Runner = gingerRunner.Executor };
            actDiameter.Context = context;

            SetDiameterActionProperties(actDiameter);
            BuildDiameterAction(actDiameter);

            context = Context.GetAsContext(actDiameter.Context);
            context.Runner.PrepActionValueExpression(actDiameter, context.BusinessFlow);

            return actDiameter;

        }
        private void SetDiameterActionProperties(ActDiameter act)
        {
            act.CommandCode = 1;

            act.IsRequestBitSet = true;

            act.DiameterMessageType = eDiameterMessageType.CreditControlRequest;

            DiameterAVP avp1 = new DiameterAVP() { Name = "AVP1", Code = 1 };
            DiameterAVP avp2 = new DiameterAVP() { Name = "AVP2", Code = 2 };

            act.RequestAvpList.Add(avp1);
            act.RequestAvpList.Add(avp2);
        }

        private void BuildDiameterAction(ActDiameter act)
        {
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.ApplicationId), act.ApplicationId.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.EndToEndIdentifier), act.EndToEndIdentifier.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.HopByHopIdentifier), act.HopByHopIdentifier.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.CommandCode), act.CommandCode.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.DiameterMessageType), act.DiameterMessageType.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.IsRequestBitSet), act.IsRequestBitSet.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.IsProxiableBitSet), act.IsProxiableBitSet.ToString());
            act.AddOrUpdateInputParamValue(nameof(ActDiameter.IsErrorBitSet), act.IsProxiableBitSet.ToString());
        }
    }
}
