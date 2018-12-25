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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Functionalities;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.WebServices;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Reflection;
using static Amdocs.Ginger.Common.Functionalities.FindAndReplaceUtils;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level2]
    public class FindAndReplaceTest
    {

        static ObservableList<BusinessFlow> mBFList = new ObservableList<BusinessFlow>();
        static ObservableList<Act> mRepoActions = new ObservableList<Act>();
        static ObservableList<ApplicationAPIModel> mApplicationAPIModels = new ObservableList<ApplicationAPIModel>();
        static Activity a1 = new Activity();
        static SearchConfig mSearchConfig1;
        static SearchConfig mSearchConfig2;
        static SearchConfig mSearchConfig3;
        static SearchConfig mSearchConfig4;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {

            BusinessFlow mBF = new BusinessFlow();

            AutoLogProxy.Init("Unit Tests");
            mBF = new BusinessFlow();
            mBF.Activities = new ObservableList<Activity>();
            mBF.Name = "BF1";
           
            a1.ActivityName = "Activity1";
            a1.Active = true;
            a1.TargetApplication = "WebApp";
            mBF.Activities.Add(a1);
            mBFList.Add(mBF);

            mRepoActions = a1.Acts;

            mSearchConfig1 = new SearchConfig() { MatchCase = false, MatchAllWord = false };
            mSearchConfig2 = new SearchConfig() { MatchCase = true, MatchAllWord = false };
            mSearchConfig3 = new SearchConfig() { MatchCase = false, MatchAllWord = true };
            mSearchConfig4 = new SearchConfig() { MatchCase = true, MatchAllWord = true };

        }



        public void ResetActionList()
        {
            a1.Acts.Clear();

            ActWebAPISoap actWebAPISoap = new ActWebAPISoap();
            actWebAPISoap.ItemName = "Action1";
            actWebAPISoap.ActInputValues.Add(new Amdocs.Ginger.Repository.ActInputValue() { Param = ActWebAPIBase.Fields.EndPointURL, Value = "bla bli bla VTFInsideList bla bla bla" });
            actWebAPISoap.ActReturnValues.Add(new Amdocs.Ginger.Repository.ActReturnValue() { Param = "ReturnValue1", Expected = "I expect you to VTFInsideList behave" });
            actWebAPISoap.Active = true;
            a1.Acts.Add(actWebAPISoap);

            ActClearAllVariables actClearAllVariables = new ActClearAllVariables();
            actClearAllVariables.ItemName = "Action2";
            actClearAllVariables.VariableName = "My Variable is VTFStringField";
            a1.Acts.Add(actClearAllVariables);

            ActScript actScript = new ActScript();
            actScript.ItemName = "Action3";
            actScript.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            actScript.Wait = 13132424;
            a1.Acts.Add(actScript);

            mApplicationAPIModels.Clear();

            ApplicationAPIModel applicationAPIModel = new ApplicationAPIModel();
            applicationAPIModel.APIType = ApplicationAPIUtils.eWebApiType.SOAP;
            applicationAPIModel.EndpointURL = "VTF";
            applicationAPIModel.DoNotFailActionOnBadRespose = true;
            applicationAPIModel.AppModelParameters.Add(new AppModelParameter() { PlaceHolder = "VTF", Path = "VTF/Path/Path/Path", OptionalValuesList = new ObservableList<OptionalValue>() { new OptionalValue() { Value = "VTF1" }, new OptionalValue() { Value = "VTF2" } } });
            applicationAPIModel.HttpHeaders.Add(new APIModelKeyValue() { Param = "Content-Type", Value = "Applicaiton/VTF" });
            mApplicationAPIModels.Add(applicationAPIModel);
        }



        [TestMethod]
        public void FindEqualStringValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTF", "Content-Type", };
            FindFieldsFromAllActionsOnApplicationModelsList(foundItemsList, ValuesToFind, mSearchConfig4);

            Assert.AreEqual(foundItemsList.Count, 3, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "EndpointURL", "Name Validation");
            Assert.AreEqual(foundItemsList[1].FieldName, "PlaceHolder", "Name Validation");
            Assert.AreEqual(foundItemsList[2].FieldName, "Param", "Name Validation");

        }

        [TestMethod]
        public void FindStringValueMatchCaseNotAllWordTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTf", "Content-Type", };
            FindFieldsFromAllActionsOnApplicationModelsList(foundItemsList, ValuesToFind, mSearchConfig2);

            Assert.AreEqual(foundItemsList.Count, 1, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "Param", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "Content-Type", "Name Validation");

        }

        [TestMethod]
        public void FindStringValueNotMatchCaseButAllWordTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTF" };
            FindFieldsFromAllActionsOnApplicationModelsList(foundItemsList, ValuesToFind, mSearchConfig3);

            Assert.AreEqual(foundItemsList.Count, 2, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "EndpointURL", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "VTF", "Name Validation");
            Assert.AreEqual(foundItemsList[1].FieldName, "PlaceHolder", "Name Validation");
            Assert.AreEqual(foundItemsList[1].FieldValue, "VTF", "Name Validation");

        }



        [TestMethod]
        public void ReplaceRootedStringValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTFStringField" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            FoundItem FI = foundItemsList[0];
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            findAndReplaceUtils.ReplaceItem(mSearchConfig1, "VTFStringField", FI, "Changed String");
            PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);
            dynamic value = PI.GetValue(FI.ItemObject);
            Assert.AreEqual(value, "My Variable is Changed String", "string Value Validation");
        }

        [TestMethod]
        public void ReplaceInnerListStringValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTFInsideList" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            FoundItem FI = foundItemsList[0];
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            findAndReplaceUtils.ReplaceItem(mSearchConfig1, "VTFInsideList", FI, "Changed String");
            PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);
            dynamic value = PI.GetValue(FI.ItemObject);
            Assert.AreEqual(value, "bla bli bla Changed String bla bla bla", "InnerList string Value Validation");
        }

        [TestMethod]
        public void ReplaceBoolValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "True" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            FoundItem FI = foundItemsList[0];
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            findAndReplaceUtils.ReplaceItem(mSearchConfig1, "True", FI, "false");
            PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);
            dynamic value = PI.GetValue(FI.ItemObject);
            Assert.AreEqual(value, false, "bool Value Validation");
        }

        [TestMethod]
        public void ReplaceEnumValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "FreeCommand" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            FoundItem FI = foundItemsList[0];
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            findAndReplaceUtils.ReplaceItem(mSearchConfig1, "FreeCommand" , FI, ActScript.eScriptAct.Script.ToString());
            PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);
            dynamic value = PI.GetValue(FI.ItemObject);
            Assert.AreEqual(value, ActScript.eScriptAct.Script, "Enum Value Validation");
        }

        
        [TestMethod]
        public void ReplaceIntValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "13132424" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            FoundItem FI = foundItemsList[0];
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            findAndReplaceUtils.ReplaceItem(mSearchConfig1, "13132424",FI, "333444");
            PropertyInfo PI = FI.ItemObject.GetType().GetProperty(FI.FieldName);
            dynamic value = PI.GetValue(FI.ItemObject);
            Assert.AreEqual(value, "333444", "Int Value Validation");
        }




        [TestMethod]
        public void FindValuesFromRootedStringField_NameAndValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTFStringField" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            Assert.AreEqual(foundItemsList.Count, 1, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "VariableName", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "My Variable is VTFStringField", "Value Validation");

        }

        [TestMethod]
        public void FindValuesFromRootedIntField_NameAndValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "13132424" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            Assert.AreEqual(foundItemsList.Count, 1, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "WaitVE", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "13132424", "Value Validation");
        }


        [TestMethod]
        public void FindValuesFromRootedEnumField_NameAndValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "FreeCommand"};
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            Assert.AreEqual(foundItemsList.Count, 1, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "ScriptCommand", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "FreeCommand", "Value Validation");
        }


        [TestMethod]
        public void FindValuesFromListField_NameAndValueTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTFInsideList" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            Assert.AreEqual(foundItemsList.Count, 2, "Found items count");
            Assert.AreEqual(foundItemsList[0].FieldName, "Value", "Name Validation");
            Assert.AreEqual(foundItemsList[0].FieldValue, "bla bli bla VTFInsideList bla bla bla", "Value Validation");
            Assert.AreEqual(foundItemsList[1].FieldName, "mExpected", "Name Validation");
            Assert.AreEqual(foundItemsList[1].FieldValue, "I expect you to VTFInsideList behave", "Value Validation");
        }

        [TestMethod]
        public void FindValuesFromList_PathGenerationTest()
        {
            ResetActionList();

            ObservableList<FoundItem> foundItemsList = new ObservableList<FoundItem>();
            List<string> ValuesToFind = new List<string>() { "VTFInsideList" };
            FindFieldsFromAllActionsOnBusinessFlowsList(foundItemsList, ValuesToFind, mSearchConfig1);

            Assert.AreEqual(foundItemsList.Count, 2, "Found items count");
            Assert.AreEqual(foundItemsList[0].FoundField, "InputValues[1]\\EndPointURL\\Value", "Path Validation");
            Assert.AreEqual(foundItemsList[1].FoundField, "ReturnValues[1]\\ReturnValue1\\mExpected", "Path Validation");
        }

        private void FindFieldsFromAllActionsOnBusinessFlowsList(ObservableList<FoundItem> foundItemsList, List<string> ValuesToFind, SearchConfig searchConfig)
        {
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            foreach (BusinessFlow BF in mBFList)
            {
                foreach (Activity activitiy in BF.Activities)
                {
                    foreach (Act action in activitiy.Acts)
                    {
                        string Path = activitiy.ActivityName + @"\" + action.ItemName;

                        foreach (string VTF in ValuesToFind)
                        {
                            findAndReplaceUtils.FindItemsByReflection(action, action, foundItemsList, VTF, searchConfig, BF, Path, string.Empty);
                        }

                    }
                }
            }

        }

        private void FindFieldsFromAllActionsOnApplicationModelsList(ObservableList<FoundItem> foundItemsList, List<string> ValuesToFind, SearchConfig searchConfig)
        {
            FindAndReplaceUtils findAndReplaceUtils = new FindAndReplaceUtils();
            foreach (ApplicationAPIModel AAM in mApplicationAPIModels)
            {
                foreach (string VTF in ValuesToFind)
                {
                    string path = "API Models" + @"\" + AAM.ItemName;
                    findAndReplaceUtils.FindItemsByReflection(AAM, AAM, foundItemsList, VTF, searchConfig, AAM, path, string.Empty);
                }
            }

        }


    }
}
