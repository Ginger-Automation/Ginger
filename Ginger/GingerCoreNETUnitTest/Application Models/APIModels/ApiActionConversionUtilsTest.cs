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
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.ActionsLib.ActionsConversion;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.Environments;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest
{
    /// <summary>
    /// API Action Conversion Utils Test
    /// </summary>
    [TestClass]
    public class ApiActionConversionUtilsTest
    {
        static BusinessFlow mBF;
        static string solutionName;
        static SolutionRepository mSolutionRepository;
        static ObservableList<BusinessFlow> mListBF;
        static RepositoryFolder<ApplicationAPIModel> apiModelsFolder;
        static string webService = "WebServices";

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            solutionName = "ActionConversionSol";
            CreateTestSolution();

            // Use helper !!!!!

            // Creating workspace
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            string TempRepositoryFolder = TestResources.GetTestTempFolder(@"Solutions\" + solutionName);
            mSolutionRepository.Open(TempRepositoryFolder);

            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution
            {
                ApplicationPlatforms =
            [
                new GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ApplicationPlatform()
                {
                    AppName = "WebServices",
                    Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.WebServices
                },
            ]
            };

            WorkSpace.Instance.Solution = sol;

            mListBF = [];
            mBF = new BusinessFlow
            {
                Name = "TestBFConversion",
                Active = true,
                TargetApplications = WorkSpace.Instance.Solution.GetSolutionTargetApplications()
            };
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            if (apiModelsFolder != null)
            {
                ObservableList<ApplicationAPIModel> list = apiModelsFolder.GetFolderItems();
                for (int i = 0; i < list.Count;)
                {
                    WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(list[i]);
                }
            }
        }

        private static void CreateTestSolution()
        {
            // First we create a basic solution with some sample items
            SolutionRepository SR = new SolutionRepository();
            string TempRepositoryFolder = TestResources.GetTestTempFolder(@"Solutions\" + solutionName);
            if (Directory.Exists(TempRepositoryFolder))
            {
                Directory.Delete(TempRepositoryFolder, true);
            }

            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(TempRepositoryFolder);

            ProjEnvironment E1 = new ProjEnvironment() { Name = "E1" };
            SR.AddRepositoryItem(E1);

            SR.Close();
        }

        #region 1st Test

        [TestMethod]
        [Timeout(60000)]
        public void APIActionConversionNewModelTest()
        {
            GetActivityWithActWebApiActions();

            ExecuteActionConversion(false, false);

            bool isValid = ValidateApiActionConversion();

            //Assert
            Assert.AreEqual(isValid, true);
        }

        private static void GetActivityWithActWebApiActions()
        {
            Activity activity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = webService
            };
            ActWebAPIRest actRest = new ActWebAPIRest
            {
                Active = true,
                Description = "WebAPI REST Action-XMLBody"
            };

            SetRestActionInputValues(actRest, nameof(ActWebAPIBase.Fields.RequestBody), GetXMLBody());
            SetRestActionInputValues(actRest, nameof(ActWebAPIBase.Fields.EndPointURL), "https://jsonplaceholder.typicode.com/posts/1");

            activity.Acts.Add(actRest);

            ActWebAPISoap actSoapJson = new ActWebAPISoap
            {
                Active = true,
                Description = "WebAPI Soap Action-JsonBody"
            };

            SetRestActionInputValues(actSoapJson, nameof(ActWebAPIBase.Fields.RequestBody), GetJsonBody());
            SetRestActionInputValues(actSoapJson, nameof(ActWebAPIBase.Fields.EndPointURL), "http://ws.cdyne.com/delayedstockquote/delayedstockquote.asmx");

            activity.Acts.Add(actSoapJson);

            mBF.AddActivity(activity);
            mListBF.Add(mBF);
        }

        private static bool ValidateApiActionConversion()
        {
            bool isValid = true;

            ObservableList<ApplicationAPIModel> list = apiModelsFolder.GetFolderItems();
            if (list.Count <= 0)
            {
                isValid = false;
            }

            if (list[0].AppModelParameters.Count > 0)
            {
                isValid = false;
            }

            if (isValid)
            {
                foreach (BusinessFlow bf in mListBF)
                {
                    if (bf.Activities[0].Acts.Count < 4)
                    {
                        isValid = false;
                        break;
                    }

                    foreach (var activity in bf.Activities)
                    {
                        string sourceDescription = activity.Acts[0].Description;
                        string convertedDescription = activity.Acts[1].Description;
                        if (sourceDescription != convertedDescription)
                        {
                            isValid = false;
                            break;
                        }

                        string sourceActType = activity.Acts[0].GetType().Name.ToString();
                        string convertedActType = activity.Acts[1].GetType().Name.ToString();
                        if (sourceActType != typeof(ActWebAPIRest).Name && convertedActType != typeof(ActWebAPIModel).Name)
                        {
                            isValid = false;
                            break;
                        }

                        sourceActType = activity.Acts[2].GetType().Name.ToString();
                        convertedActType = activity.Acts[3].GetType().Name.ToString();
                        if (sourceActType != typeof(ActWebAPISoap).Name && convertedActType != typeof(ActWebAPIModel).Name)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
            }

            return isValid;
        }

        #endregion

        #region 2nd Test

        [TestMethod]
        //[Timeout(60000)]
        public void APIActionConversionOptionalValuesTest()
        {
            GetActivityWithActionsWithSameParametersDiffernetOptionalValues(false);

            ExecuteActionConversion(true, false);

            bool isValid = ValidateApiActionConversionOptionalValues();

            //Assert
            Assert.AreEqual(isValid, true);
        }

        private static void GetActivityWithActionsWithSameParametersDiffernetOptionalValues(bool onlyOneAction)
        {
            Activity activity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = webService
            };
            ActWebAPIRest actRest = new ActWebAPIRest
            {
                Active = true,
                Description = "WebAPI REST Action-XMLBody"
            };

            SetRestActionInputValues(actRest, nameof(ActWebAPIBase.Fields.RequestBody), GetXMLBody());
            SetRestActionInputValues(actRest, nameof(ActWebAPIBase.Fields.EndPointURL), "https://jsonplaceholder.typicode.com/posts/1");

            ObservableList<ActReturnValue> retVal =
            [
                new ActReturnValue() { Param = "RetCode_1st", Expected = "Ok", StoreToValue = ActReturnValue.eStoreTo.ApplicationModelParameter.ToString() },
                new ActReturnValue() { Param = "RetCode_2nd", Expected = null, StoreToValue = ActReturnValue.eStoreTo.ApplicationModelParameter.ToString() },
                new ActReturnValue() { Param = "RetCode_3rd", Expected = "Ok", StoreToValue = ActReturnValue.eStoreTo.DataSource.ToString() },
            ];
            actRest.ReturnValues = retVal;

            activity.Acts.Add(actRest);

            if (!onlyOneAction)
            {
                ActWebAPIRest actRestDiff = new ActWebAPIRest
                {
                    Active = true,
                    Description = "WebAPI REST Action-XMLBody 2"
                };

                SetRestActionInputValues(actRestDiff, nameof(ActWebAPIBase.Fields.RequestBody), GetXMLBodyDifferentOptionalValues());
                SetRestActionInputValues(actRestDiff, nameof(ActWebAPIBase.Fields.EndPointURL), "https://jsonplaceholder.typicode.com/posts/1");

                activity.Acts.Add(actRestDiff);
            }

            mBF.AddActivity(activity);
            mListBF.Add(mBF);
        }

        private static bool ValidateApiActionConversionOptionalValues()
        {
            bool isValid = true;

            ObservableList<ApplicationAPIModel> list = apiModelsFolder.GetFolderItems();
            if (list.Count <= 0)
            {
                isValid = false;
            }

            if (isValid && list[0].ReturnValues.Count > 0)
            {
                isValid = false;
            }

            if (isValid && list.Count == 1)
            {
                if (list[0].AppModelParameters.Count != 2)
                {
                    isValid = false;
                }
                else if (list[0].AppModelParameters.Count == 2)
                {
                    if (list[0].AppModelParameters[0].OptionalValuesList.Count != 2 && list[0].AppModelParameters[0].OptionalValuesString == "11*,33")
                    {
                        isValid = false;
                    }
                    if (list[0].AppModelParameters[1].OptionalValuesList.Count != 2 && list[0].AppModelParameters[1].OptionalValuesString == "22*,44")
                    {
                        isValid = false;
                    }
                }

            }

            return isValid;
        }

        #endregion

        #region 3rd Test

        [TestMethod]
        [Timeout(60000)]
        public void APIActionConversionParameterizeBodyTest()
        {
            GetActivityWithActionsWithSameParametersDiffernetOptionalValues(true);

            ExecuteActionConversion(true, false);

            bool isValid = ValidateApiActionConversionParameterizeBody();

            //Assert
            Assert.AreEqual(isValid, true);
        }

        private static bool ValidateApiActionConversionParameterizeBody()
        {
            bool isValid = true;

            ObservableList<ApplicationAPIModel> list = apiModelsFolder.GetFolderItems();
            if (list.Count <= 0)
            {
                isValid = false;
            }
            else if (list.Count == 1)
            {
                string paramName = list[0].AppModelParameters[0].ItemName.Replace("{", "").Replace("}", "");
                if (!list[0].RequestBody.Contains("{" + paramName + "}"))
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        #endregion

        #region 4th Test

        [TestMethod]
        [Timeout(60000)]
        public void APIActionConversionReturnValueTest()
        {
            GetActivityWithActionsWithSameParametersDiffernetOptionalValues(true);

            ExecuteActionConversion(false, true);

            bool isValid = ValidateApiActionConversionReturnValue();

            //Assert
            Assert.AreEqual(isValid, true);
        }

        private static bool ValidateApiActionConversionReturnValue()
        {
            bool isValid = true;

            ObservableList<ApplicationAPIModel> list = apiModelsFolder.GetFolderItems();
            if (list.Count <= 0)
            {
                isValid = false;
            }
            else if (list.Count == 1)
            {
                if (list[0].ReturnValues == null || list[0].ReturnValues.Count <= 0)
                {
                    isValid = false;
                }
            }

            return isValid;
        }

        #endregion

        #region Execution

        private static void ExecuteActionConversion(bool parameterizeRequestBody, bool pullValidations)
        {
            ObservableList<BusinessFlowToConvert> ListOfBusinessFlowToConvert = [];
            ApiActionConversionUtils utils = new ApiActionConversionUtils();
            foreach (var bf in mListBF)
            {
                foreach (var act in bf.Activities)
                {
                    act.SelectedForConversion = true;
                }

                BusinessFlowToConvert flowConversion = new BusinessFlowToConvert
                {
                    BusinessFlow = bf,
                    ConversionStatus = eConversionStatus.Pending,
                    TotalProcessingActionsCount = utils.GetConvertibleActionsCountFromBusinessFlow(bf)
                };
                ListOfBusinessFlowToConvert.Add(flowConversion);
            }

            apiModelsFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryItemRootFolder<ApplicationAPIModel>();
            utils.ConvertToApiActionsFromBusinessFlows(ListOfBusinessFlowToConvert, parameterizeRequestBody, pullValidations, apiModelsFolder);
        }

        #endregion

        private static string GetXMLBody()
        {
            return "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:ws='http://ws.cdyne.com/'>" +
                   "    <soapenv:Header />" +
                   "    <soapenv:Body>" +
                   "        <ws:GetQuickQuote>" +
                   "            <!--Optional:-->" +
                   "            <ws:StockSymbol >11</ws:StockSymbol>" +
                   "            <!--Optional:-->" +
                   "            <ws:LicenseKey >22</ws:LicenseKey>" +
                   "        </ws:GetQuickQuote>" +
                   "    </soapenv:Body>" +
                   "</soapenv:Envelope>";
        }

        private static string GetXMLBodyDifferentOptionalValues()
        {
            return "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:ws='http://ws.cdyne.com/'>" +
                   "    <soapenv:Header />" +
                   "    <soapenv:Body>" +
                   "        <ws:GetQuickQuote>" +
                   "            <!--Optional:-->" +
                   "            <ws:StockSymbol >33</ws:StockSymbol>" +
                   "            <!--Optional:-->" +
                   "            <ws:LicenseKey >44</ws:LicenseKey>" +
                   "        </ws:GetQuickQuote>" +
                   "    </soapenv:Body>" +
                   "</soapenv:Envelope>";
        }

        private static string GetJsonBody()
        {
            return "{ \"direcTVNow\":{ \"offerType\":[\"basepackage\",\"standalone\"],\"zipCode\":\"53122\"},\"channel\":\"OPUS\"}";
        }

        private static void SetRestActionInputValues(Act act, string sFileName, string sValue)
        {
            ActInputValue inputValue = new ActInputValue() { FileName = sFileName, ItemName = sFileName, Param = sFileName, Value = sValue };
            act.InputValues.Add(inputValue);
        }
    }
}
