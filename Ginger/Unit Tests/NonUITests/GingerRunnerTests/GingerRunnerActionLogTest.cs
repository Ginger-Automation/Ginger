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
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [Level1]
    [TestClass]
    public class GingerRunnerActionLogTest
    {
        private const string INP_VAL_EXPECTED = "TestInputValue";
        private const string RET_VAL_EXPECTED = "123456";

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            EmptyTempActionLogFolder();
            Ginger.App.InitClassTypesDictionary();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // before every test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //after every test
        }

        [TestMethod]
        public void TestActionLog_Text()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_1.log");
            string actionLogText = "ActionLogTestText";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);

            ActDummy actDummy = new ActDummy();
            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.EnableActionLogConfig = true;
            actDummy.ActionLogConfig.ActionLogText = actionLogText;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, actionLogText));
        }

        [TestMethod]
        public void TestActionLog_InputValues()
        {
            //Arrange 
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_2.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the values in the action
            ActInputValue actInputValue = new ActInputValue();
            actInputValue.ItemName = "TestInput";
            actInputValue.Value = INP_VAL_EXPECTED;

            actDummy.InputValues.Add(actInputValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogInputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, INP_VAL_EXPECTED));            
        }

        [TestMethod]
        public void TestActionLog_ReturnValues()
        {
            //Arrange  
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_3.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the return values in the action
            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = RET_VAL_EXPECTED;
            actReturnValue.Actual = RET_VAL_EXPECTED;
            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogOutputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.AreEqual(FindTextOccurrencesInFile(fileName, RET_VAL_EXPECTED), 2);
        }

        [TestMethod]
        public void TestActionLog_InputAndReturnValues()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_4.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the values in the action
            ActInputValue actInputValue = new ActInputValue();
            actInputValue.ItemName = "TestInput";
            actInputValue.Value = INP_VAL_EXPECTED;
            actDummy.InputValues.Add(actInputValue);

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = RET_VAL_EXPECTED;
            actReturnValue.Actual = RET_VAL_EXPECTED;
            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogInputVariables = true;
            actDummy.ActionLogConfig.LogOutputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, INP_VAL_EXPECTED));
            Assert.IsTrue(IsFileContains(fileName, RET_VAL_EXPECTED));
        }

        [TestMethod]
        public void TestActionLog_RunStatusFailCheck()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_5.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogRunStatus = true;
            actDummy.EnableActionLogConfig = true;

            //actDummy.Execute();
            actDummy.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, "Failed"));
        }

        [TestMethod]
        public void TestActionLog_RunStatusPassCheck()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_6.log");
            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);

            ActDummy actDummy = new ActDummy();
            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogRunStatus = true;
            actDummy.ActionLogConfig.LogOutputVariables = true;
            actDummy.EnableActionLogConfig = true;

            // set action status to passed
            actDummy.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, "Passed"));
        }

        [TestMethod]
        public void TestActionLog_NoFileExistsOnDisableLog()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_7.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = RET_VAL_EXPECTED;
            actReturnValue.Actual = RET_VAL_EXPECTED;
            actDummy.ReturnValues.Add(actReturnValue);

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsFalse(IsFileExists(fileName));
        }

        [TestMethod]
        public void TestActionLog_MultipleOccurancesOfReturnValues()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_8.log");

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the values in the action
            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestForFirstReturnValueInTable";
            actReturnValue.Expected = RET_VAL_EXPECTED;
            actReturnValue.Actual = RET_VAL_EXPECTED;
            actDummy.ReturnValues.Add(actReturnValue);

            actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestForSecondReturnValueInTable";
            actReturnValue.Expected = RET_VAL_EXPECTED;
            actReturnValue.Actual = "WrongValue";
            actDummy.ReturnValues.Add(actReturnValue);

            actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestForThirdReturnValueInTable";
            actReturnValue.Expected = "ExpectedRightValue";
            actReturnValue.Actual = "WrongActualValue";
            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogOutputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.AreEqual(FindTextOccurrencesInFile(fileName, RET_VAL_EXPECTED), 3);
        }

        [TestMethod]
        public void TestActionLog_CheckActionLogEnableOptionSavedInBFXML()
        {
            //Arrange
            
            // define action
            ActDummy actDummy = new ActDummy();
            // set all the values in the action
            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.ActionLogText = "TestActionLog";
            actDummy.ActionLogConfig.LogRunStatus = true;
            actDummy.EnableActionLogConfig = true;

            // define activity 
            Activity activity = new Activity();
            activity.ActivityName = "Activity Number 1";

            // define business flow
            BusinessFlow businessFlowWrite = new BusinessFlow();
            businessFlowWrite.Name = "Business Flow 1";
            businessFlowWrite.Activities = new ObservableList<Activity>();

            activity.Acts.Add(actDummy);
            businessFlowWrite.Activities.Add(activity);

            //Act                                    
            string txt = businessFlowWrite.RepositorySerializer.SerializeToString(businessFlowWrite); 
            BusinessFlow businessFlowRead = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(txt);
            ActDummy actDummyRead = (ActDummy)businessFlowRead.Activities[0].Acts[0];

            // Assert
            Assert.AreEqual(actDummyRead.EnableActionLogConfig, actDummy.EnableActionLogConfig);
            Assert.AreEqual(actDummyRead.ActionLogConfig.ActionLogText, actDummy.ActionLogConfig.ActionLogText);
            Assert.AreEqual(actDummyRead.ActionLogConfig.LogRunStatus , actDummy.ActionLogConfig.LogRunStatus);
        }


        private bool IsFileContains(string fileName, string textToSearch)
        {
            return System.IO.File.ReadAllText(fileName).Contains(textToSearch);
        }

        private bool IsFileExists(string fileName)
        {
            return System.IO.File.Exists(fileName);
        }

        private int FindTextOccurrencesInFile(string fileName, string textToSearch)
        {
            string fileContent = System.IO.File.ReadAllText(fileName);
            string cCriteria = @"\b" + textToSearch + @"\b";
            System.Text.RegularExpressions.Regex oRegex = new
                System.Text.RegularExpressions.Regex(cCriteria, RegexOptions.IgnoreCase);
            return oRegex.Matches(fileContent).Count;
        }

        private static void EmptyTempActionLogFolder()
        {
            string tempFolder = TestResources.GetTempFile("") + "\\ActionLog";
            if (System.IO.Directory.Exists(tempFolder))
            {
                System.IO.DirectoryInfo directory = new DirectoryInfo(tempFolder);
                foreach (System.IO.FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(tempFolder);
            }
        }


    }
}
