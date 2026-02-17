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
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class TextFileOperationsTests
    {
        GingerExecutionEngine gingerRunner = null;
        ActReadTextFile actReadTextFile = new ActReadTextFile();
        String txt = "<html>\n!@$!#@%$^%$&\rÃ—ËœÃ—Â§Ã—Â¡Ã—Ëœ Ã—â€˜Ã—Â¢Ã—â„¢Ã—â€˜Ã—Â¨Ã—â„¢Ã—Âª\n Ã¢Ë†â€šcÃ Â¹ÂÃ¢Ë†â€šÃŽÂµ\nÃƒâ€“ ÃƒËœ ÃƒÂ§ Ãƒâ€˜ Ã‚Â½ Ã‚Â¥";

        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            gingerRunner = new GingerExecutionEngine(new GingerRunner(), Amdocs.Ginger.Common.eExecutedFrom.Automation);
            if (WorkSpace.Instance.Solution?.SolutionOperations == null)
            {
                WorkSpace.Instance.Solution.SolutionOperations = new SolutionOperations(WorkSpace.Instance.Solution);
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void TextFileWriteTest()
        {
            //Arrange      
            actReadTextFile.TextToWrite = txt;
            actReadTextFile.TextFilePath = TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileOperations.txt");
            actReadTextFile.ValueExpression = new ValueExpression(actReadTextFile, "");

            //Act
            actReadTextFile.TextFileEncoding = ActReadTextFile.eTextFileEncodings.UTF8;
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Write, false, true);
            string textWriteToFile = System.IO.File.ReadAllText(actReadTextFile.TextFilePath);

            //Assert
            Assert.AreEqual(textWriteToFile, txt);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TextFileReadTest()
        {
            //Arrange      
            actReadTextFile.TextToWrite = txt;
            actReadTextFile.TextFilePath = TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileOperations.txt");
            actReadTextFile.ValueExpression = new ValueExpression(actReadTextFile, "");

            //Act
            actReadTextFile.TextFileEncoding = ActReadTextFile.eTextFileEncodings.UTF8;
            File.WriteAllText(actReadTextFile.TextFilePath, txt);
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Read, true, true);
            ActReturnValue returnValue = (ActReturnValue)actReadTextFile.ActReturnValues.ListItems.Find(x => (x as ActReturnValue).ItemName.Equals("File Content"));

            //Assert
            Assert.AreEqual(returnValue.Actual, txt);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TextAppendTest()
        {
            //Arrange
            actReadTextFile.TextToWrite = txt;
            actReadTextFile.TextFilePath = TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileOperations.txt");
            actReadTextFile.ValueExpression = new ValueExpression(actReadTextFile, "");

            //Act
            actReadTextFile.TextFileEncoding = ActReadTextFile.eTextFileEncodings.UTF8;
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Write, false, true);
            actReadTextFile.TextToWrite = "Appended Text";
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Append, false, true, ActReadTextFile.eAppendAt.End);
            string textAppendedToFile = System.IO.File.ReadAllText(actReadTextFile.TextFilePath);

            //Assert
            Assert.AreEqual(textAppendedToFile, txt + actReadTextFile.TextToWrite);
        }
        private void ExecuteActionOperation(ActReadTextFile textFileOperation, ActReadTextFile.eTextFileActionMode actionMode, bool addNewReturnParams
            , bool isValueForDriver, ActReadTextFile.eAppendAt appendAt = ActReadTextFile.eAppendAt.End)
        {
            if (isValueForDriver)
            {
                gingerRunner.ProcessInputValueForDriver(textFileOperation);
            }
            textFileOperation.AppendAt = appendAt;
            textFileOperation.FileActionMode = actionMode;
            textFileOperation.AddNewReturnParams = addNewReturnParams;
            textFileOperation.Execute();
        }
    }
}
