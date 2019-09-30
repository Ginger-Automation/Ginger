using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
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
        static string wsdlURL = @"http://ws.cdyne.com/delayedstockquote/delayedstockquote.asmx?wsdl";

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
            NewRepositorySerializer.AddClassesFromAssembly(typeof(ApplicationAPIModel).Assembly);
            WorkSpace.Instance.SolutionRepository.Open(TempSolutionFolder);
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        public void APILearnWSDLDelayedStockQuote()
        {
            //Arrange
            ObservableList<ApplicationAPIModel> apiModelsList = new ObservableList<ApplicationAPIModel>();

            //Act
            Learn(apiModelsList);
            while (apiModelsList.Count < 6)
            {
                //Let it learn
            }

            apiModelsList[1].RequestBody = apiModelsList[1].RequestBody + "This is modified";
            foreach (ApplicationAPIModel apiModel in apiModelsList.Take(2))
            {
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(apiModel);
            }
        }

        private async void Learn(ObservableList<ApplicationAPIModel> AAMSList)
        {
            WSDLParser wsdlParser = new WSDLParser();
            await Task.Run(() => wsdlParser.ParseDocument(wsdlURL, AAMSList));
        }

        [TestMethod]
        [Timeout(60000)]
        public void APIComparisonWSDLDelayedStockQuoteTest()
        {
            APILearnWSDLDelayedStockQuote();

            ObservableList<ApplicationAPIModel> learnedAPIsList = new ObservableList<ApplicationAPIModel>();
            Learn(learnedAPIsList);
            while(learnedAPIsList.Count < 6)
            {
                // Let the learning finish
            }

            ObservableList<DeltaAPIModel> deltaAPIsList = APIDeltaUtils.DoAPIModelsCompare(learnedAPIsList);

            //Assert - Check Count
            Assert.AreEqual(deltaAPIsList.Count, 6, "Is Delta Models counts to 6");

            // Assert - Confirm Default Selected Operation = Do Not Add, for API with Comparison Status = UnChanged
            Assert.AreEqual(deltaAPIsList[0].comparisonStatus, DeltaAPIModel.eComparisonOutput.Unchanged, deltaAPIsList[0].learnedAPI.Name + " is UnChanged.");
            Assert.AreEqual(deltaAPIsList[0].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.DoNotAdd, deltaAPIsList[0].learnedAPI.Name + " is UnChanged and thus Default Selected Operation is 'Do Not Add'.");

            // Assert - Confirm Default Selected Operation = Replace Existing with New, for API with Comparison Status = Modified
            Assert.AreEqual(deltaAPIsList[1].comparisonStatus, DeltaAPIModel.eComparisonOutput.Modified, deltaAPIsList[1].learnedAPI.Name + " is Modified and having modified RequestBody.");
            Assert.AreEqual(deltaAPIsList[1].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.ReplaceExisting , deltaAPIsList[1].learnedAPI.Name + " is Modified and thus Default Selected Operation is 'Replace Existing with New'.");

            // Assert - Confirm Default Selected Operation = Add New, for API with Comparison Status = New
            Assert.AreEqual(deltaAPIsList[2].comparisonStatus, DeltaAPIModel.eComparisonOutput.New, deltaAPIsList[2].learnedAPI.Name + " is NEW API.");
            Assert.AreEqual(deltaAPIsList[2].SelectedOperationEnum, DeltaAPIModel.eHandlingOperations.Add, deltaAPIsList[2].learnedAPI.Name + " is New API and thus, Default Selected Operation is 'Add New'.");
        }
    }
}
