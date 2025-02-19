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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCoreNET.Application_Models;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTest.AutoPilot
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
        static int testExecutedCount = 0;

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

            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            WorkSpace.Instance.SolutionRepository.CreateRepository(TempSolutionFolder);

            NewRepositorySerializer RS = new NewRepositorySerializer();
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommon);
            WorkSpace.Instance.SolutionRepository.Open(TempSolutionFolder);

            // Initialize ApplicationAPIModels XML file names to be fetched from TestResources
            xmlFiles =
            [
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "Create_User.Ginger.ApplicationAPIModel.xml",
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "Delete_User.Ginger.ApplicationAPIModel.xml",
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "PhoneVerifySOAP_CheckPhoneNumber.Ginger.ApplicationAPIModel.xml",
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "Update_User.Ginger.ApplicationAPIModel.xml",
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "PhoneVerifySOAP_CheckPhoneNumbers.Ginger.ApplicationAPIModel.xml",
                @"Repository" + Path.DirectorySeparatorChar + "SampleAPIModels" + Path.DirectorySeparatorChar + "GetQuote_DelayedStockQuoteSoap.Ginger.ApplicationAPIModel.xml",
            ];

            existingAPIsList = [];
            learnedAPIsList = [];

            // Importing API Models from XML files (listed in xmlFiles)
            foreach (String fileName in xmlFiles)
            {
                var tempFile = TestResources.GetTestResourcesFile(fileName);
                ApplicationAPIModel appModel = RS.DeserializeFromFile(typeof(ApplicationAPIModel), tempFile) as ApplicationAPIModel;
                appModel.FilePath = appModel.Name;//so it will get new file path when been added to repository later
                if (appModel != null)
                {
                    existingAPIsList.Add(appModel);
                    learnedAPIsList.Add(appModel.CreateCopy() as ApplicationAPIModel);
                }
            }

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
            if (testExecutedCount == 3)
            {
                xmlFiles.Clear();
                learnedAPIsList.Clear();
                existingAPIsList.Clear();
                ResetExecutionCounter();
            }
            else
            {
                testExecutedCount++;
            }
        }

        static void ResetExecutionCounter()
        {
            testExecutedCount = 0;
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
            ObservableList<ApplicationAPIModel> learnedModels = [learnedAPIsList[0], learnedAPIsList[5]];

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
            ObservableList<ApplicationAPIModel> learnedModels = [learnedAPIsList[1], learnedAPIsList[3]];

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
            ObservableList<ApplicationAPIModel> learnedModels = [learnedAPIsList[2], learnedAPIsList[4]];

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
