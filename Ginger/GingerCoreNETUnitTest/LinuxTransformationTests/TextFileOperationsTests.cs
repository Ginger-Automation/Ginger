using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
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
        GingerRunner gingerRunner = null;
        ActReadTextFile actReadTextFile = new ActReadTextFile();
        String txt = "<html>\n!@$!#@%$^%$&\rטקסט בעיברית\n ∂c๏∂ε\nÖ Ø ç Ñ ½ ¥";

        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
            gingerRunner = new GingerRunner(Amdocs.Ginger.Common.eExecutedFrom.Automation);
        }

        [TestMethod]
        [Timeout(60000)]
        public void TextFileWriteReadTest()
        {
            //Arrange      
            actReadTextFile.TextToWrite = txt;
            actReadTextFile.TextFilePath = TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileOperations.txt");
            actReadTextFile.ValueExpression = new ValueExpression(actReadTextFile, "");

            //Act
            actReadTextFile.TextFileEncoding = ActReadTextFile.eTextFileEncodings.UTF8;
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Write, false,true);
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Read, true,false);

            //Assert
            ActReturnValue returnValue = (ActReturnValue)actReadTextFile.ActReturnValues.ListItems.Find(x => (x as ActReturnValue).ItemName.Equals("File Content"));
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
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Write, false,true);
            actReadTextFile.TextToWrite = "Appended Text";
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Append, false, true, ActReadTextFile.eAppendAt.End);
            ExecuteActionOperation(actReadTextFile, ActReadTextFile.eTextFileActionMode.Read, true,false);

            //Assert
            ActReturnValue returnValue = (ActReturnValue)actReadTextFile.ActReturnValues.ListItems.Find(x => (x as ActReturnValue).ItemName.Equals("File Content"));
            Assert.AreEqual(returnValue.Actual, txt + actReadTextFile.TextToWrite);
        }
        private void ExecuteActionOperation(ActReadTextFile textFileOperation, ActReadTextFile.eTextFileActionMode actionMode, bool addNewReturnParams
            , bool isValueForDriver, ActReadTextFile.eAppendAt appendAt = ActReadTextFile.eAppendAt.End)
        {
            if(isValueForDriver)
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
