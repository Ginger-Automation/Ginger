using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Drivers.WebServicesDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.Application_Models;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using GingerWPF.ApplicationModelsLib.APIModels;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests.NonUITests.AutoPilot
{
    /// <summary>
    /// API Models Comparison Utility Test
    /// </summary>
    /// 

    [TestClass]
    public class APIModelsCompareTest
    {
        static GingerRunner mGR = null;

        static List<string> xmlFiles;
        static List<ApplicationAPIModel> learnedAPIsList;
        static ObservableList<ApplicationAPIModel> existingAPIsList;
        static int textExecutedCount = 0;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Creating the GingerCoreNET WorkSpace");
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);

            string TempSolutionFolder = TestResources.GetTestTempFolder(@"Solutions" + Path.DirectorySeparatorChar + "APIModelsComparisonUtilityTest");
            if (Directory.Exists(TempSolutionFolder))
            {
                Directory.Delete(TempSolutionFolder, true);
            }

            // Initialize XML Parser 
            XMLTemplateParser xmlParser = new XMLTemplateParser();

            // Initialize XML file names to be fetched from TestResources
            xmlFiles = new List<string>()
            {
                @"XML\Create_User.xml",
                @"XML\Delete_User.xml",
                @"XML\PhoneVerifySOAP_CheckPhoneNumber.xml",
                @"XML\Update_User.xml",
                @"XML\PhoneVerifySOAP_CheckPhoneNumbers.xml",
                @"XML\GetQuote_DelayedStockQuoteSoap.xml",
            };

            existingAPIsList = new ObservableList<ApplicationAPIModel>();

            // Importing API Models from XML files (listed in xmlFiles)
            foreach (String fileName in xmlFiles)
            {
                var tempFile = TestResources.GetTestResourcesFile(fileName);
                existingAPIsList = xmlParser.ParseDocument(tempFile, existingAPIsList);
                existingAPIsList.Last().EndpointURL = fileName;
            };

            // storing the learned API Models in Existing Models to keep intact while using for testing the Compaarison feature
            learnedAPIsList = new List<ApplicationAPIModel>();
            foreach (ApplicationAPIModel apiModel in existingAPIsList)
            {
                learnedAPIsList.Add(apiModel.CreateCopy() as ApplicationAPIModel);
            }

            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            WorkSpace.Instance.SolutionRepository.CreateRepository(TempSolutionFolder);

            NewRepositorySerializer RS = new NewRepositorySerializer();
            NewRepositorySerializer.AddClassesFromAssembly(typeof(ApplicationAPIModel).Assembly);
            WorkSpace.Instance.SolutionRepository.Open(TempSolutionFolder);

            // Modifying certain API Models for testing different Comparison status and scenarios
            existingAPIsList[1].RequestBody = existingAPIsList[1].RequestBody + "This is modified";
            existingAPIsList[3].RequestBody = existingAPIsList[3].RequestBody + "This is also modified";

            // Storing the API Models in the Solution Repository as will be utilized during Comparison process
            foreach (ApplicationAPIModel apiModel in existingAPIsList.Skip(1).Take(4))
            {
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(apiModel);
            }
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            if (textExecutedCount == 3)
            {
                xmlFiles.Clear();
                learnedAPIsList.Clear();
                existingAPIsList.Clear();
                textExecutedCount = 0;
            }
            else
                textExecutedCount++;
        }

        [TestMethod]
        [Timeout(60000)]
        public void APIComparison_MatchingTest()
        {
            // Arrange
            ObservableList<ApplicationAPIModel> learnedModels = new ObservableList<ApplicationAPIModel>(learnedAPIsList);

            //Act
            ObservableList<DeltaAPIModel> deltaAPIsList = APIDeltaUtils.DoAPIModelsCompare(learnedModels);

            //Assert - Check Count
            Assert.AreEqual(deltaAPIsList.Count, 6, "Is Delta Models count is 6");

            Assert.AreEqual(deltaAPIsList.Where(d => d.matchingAPIModel != null).Count(), 4, "Confirm if 4 API Models already exists in the solution.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void New_APIComparisonStatusTest()
        {
            // Arrange
            ObservableList<ApplicationAPIModel> learnedModels = new ObservableList<ApplicationAPIModel>() { learnedAPIsList[0], learnedAPIsList[5] };

            //Act
            ObservableList<DeltaAPIModel> deltaAPIsList = APIDeltaUtils.DoAPIModelsCompare(learnedModels);

            // Assert - Confirm Default Selected Operation = Add New, for API with Comparison Status = New
            Assert.AreEqual(deltaAPIsList[0].comparisonStatus, DeltaAPIModel.eComparisonOutput.New, deltaAPIsList[0].learnedAPI.Name + " is NEW API.");
            Assert.AreEqual(deltaAPIsList[0].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.Add, deltaAPIsList[0].learnedAPI.Name + " is New API and thus, Default Selected Operation is 'Add New'.");

            Assert.AreEqual(deltaAPIsList[1].comparisonStatus, DeltaAPIModel.eComparisonOutput.New, deltaAPIsList[1].learnedAPI.Name + " is NEW API.");
            Assert.AreEqual(deltaAPIsList[1].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.Add, deltaAPIsList[1].learnedAPI.Name + " is New API and thus, Default Selected Operation is 'Add New'.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void Modified_APIComparisonStatusTest()
        {
            // Arrange
            ObservableList<ApplicationAPIModel> learnedModels = new ObservableList<ApplicationAPIModel>() { learnedAPIsList[1], learnedAPIsList[3] };

            //Act
            ObservableList<DeltaAPIModel> deltaAPIsList = APIDeltaUtils.DoAPIModelsCompare(learnedModels);

            // Assert - Confirm Default Selected Operation = Replace Existing with New, for API with Comparison Status = Modified
            Assert.AreEqual(deltaAPIsList[0].comparisonStatus, DeltaAPIModel.eComparisonOutput.Modified, deltaAPIsList[0].learnedAPI.Name + " is Modified and having modified RequestBody.");
            Assert.AreEqual(deltaAPIsList[0].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.ReplaceExisting, deltaAPIsList[0].learnedAPI.Name + " is Modified and thus Default Selected Operation is 'Replace Existing with New'.");

            Assert.AreEqual(deltaAPIsList[1].comparisonStatus, DeltaAPIModel.eComparisonOutput.Modified, deltaAPIsList[1].learnedAPI.Name + " is Modified and having modified RequestBody.");
            Assert.AreEqual(deltaAPIsList[1].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.ReplaceExisting, deltaAPIsList[1].learnedAPI.Name + " is Modified and thus Default Selected Operation is 'Replace Existing with New'.");
        }

        [TestMethod]
        [Timeout(60000)]
        public void Unchanged_APIComparisonStatusTest()
        {
            // Arrange
            ObservableList<ApplicationAPIModel> learnedModels = new ObservableList<ApplicationAPIModel>() { learnedAPIsList[2], learnedAPIsList[4] };

            //Act
            ObservableList<DeltaAPIModel> deltaAPIsList = APIDeltaUtils.DoAPIModelsCompare(learnedModels);

            // Assert - Confirm Default Selected Operation = Do Not Add, for API with Comparison Status = UnChanged
            Assert.AreEqual(deltaAPIsList[0].comparisonStatus, DeltaAPIModel.eComparisonOutput.Unchanged, deltaAPIsList[0].learnedAPI.Name + " is UnChanged.");
            Assert.AreEqual(deltaAPIsList[0].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.DoNotAdd, deltaAPIsList[0].learnedAPI.Name + " is UnChanged and thus Default Selected Operation is 'Do Not Add'.");

            Assert.AreEqual(deltaAPIsList[1].comparisonStatus, DeltaAPIModel.eComparisonOutput.Unchanged, deltaAPIsList[1].learnedAPI.Name + " is UnChanged.");
            Assert.AreEqual(deltaAPIsList[1].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.DoNotAdd, deltaAPIsList[1].learnedAPI.Name + " is UnChanged and thus Default Selected Operation is 'Do Not Add'.");
        }
    }
}
