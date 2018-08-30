using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace UnitTests.NonUITests.GingerRunnerTests
{
    [Level1]
    [TestClass]
    public class GingerRunnerActionLogTest
    {

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            EmptyTempActionLogFolder();
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
        public void TestActionLogText()
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
        public void TestActionLogInputValues()
        {
            //Arrange 
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_2.log");
            string inputValExpected = "TestInputValue";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the values in the action
            ActInputValue actInputValue = new ActInputValue();
            actInputValue.ItemName = "TestInput";
            actInputValue.Value = inputValExpected;

            actDummy.InputValues.Add(actInputValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogInputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, inputValExpected));            
        }

        [TestMethod]
        public void TestActionLogReturnValues()
        {
            //Arrange  
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_3.log");
            string returnValExpected = "123456";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the return values in the action
            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = returnValExpected;
            actReturnValue.Actual = returnValExpected;
            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogOutputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.AreEqual(FindTextOccurrencesInFile(fileName, returnValExpected), 2);
        }

        [TestMethod]
        public void TestActionLogInputAndReturnValues()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_4.log");
            string inputValExpected = "TestInputValue";
            string returnValExpected = "123456";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            // set all the values in the action
            ActInputValue actInputValue = new ActInputValue();
            actInputValue.ItemName = "TestInput";
            actInputValue.Value = inputValExpected;

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = returnValExpected;
            actReturnValue.Actual = returnValExpected;

            actDummy.InputValues.Add(actInputValue);
            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogInputVariables = true;
            actDummy.ActionLogConfig.LogOutputVariables = true;

            actDummy.EnableActionLogConfig = true;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, inputValExpected));
            Assert.IsTrue(IsFileContains(fileName, returnValExpected));
        }

        [TestMethod]
        public void TestActionLogStatusFailCheck()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_5.log");
            string returnValExpected = "123456";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = returnValExpected;
            actReturnValue.Actual = string.Empty;

            actDummy.ReturnValues.Add(actReturnValue);

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
        public void TestActionLogStatusPassCheck()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_6.log");
            string returnValExpected = "123456";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = returnValExpected;
            actReturnValue.Actual = returnValExpected;

            actDummy.ReturnValues.Add(actReturnValue);

            actDummy.ActionLogConfig = new ActionLogConfig();
            actDummy.ActionLogConfig.LogRunStatus = true;
            actDummy.ActionLogConfig.LogOutputVariables = true;
            actDummy.EnableActionLogConfig = true;

            //actDummy.Execute();
            actDummy.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsTrue(IsFileContains(fileName, "Passed"));
            Assert.IsTrue(IsFileContains(fileName, returnValExpected));
        }

        [TestMethod]
        public void TestActionLogDisableLog()
        {
            //Arrange
            string fileName = TestResources.GetTempFile("ActionLog\\ActionLogTest_7.log");
            string returnValExpected = "123456";

            GingerRunnerLogger gingerRunnerLogger = new GingerRunnerLogger(fileName);
            ActDummy actDummy = new ActDummy();

            ActReturnValue actReturnValue = new ActReturnValue();
            actReturnValue.ItemName = "TestReturn";
            actReturnValue.Expected = returnValExpected;
            actReturnValue.Actual = returnValExpected;
            actDummy.ReturnValues.Add(actReturnValue);

            //Act
            gingerRunnerLogger.LogAction(actDummy);

            //Assert
            Assert.IsFalse(IsFileExists(fileName));
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
                foreach (System.IO.FileInfo file in directory.GetFiles()) file.Delete();
            }
        }


    }
}
