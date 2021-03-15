using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class TextFileOperationsTests
    {
        [TestMethod]
        [Timeout(60000)]
        public void ConditionEqualCSTest()
        {
            //Arrange            
            ActReadTextFile actReadTextFile = new ActReadTextFile();
            String txt = "<html>\n!@$!#@%$^%$&\rטקסט בעיברית\n ∂c๏∂ε\nÖ Ø ç Ñ ½ ¥";
            actReadTextFile.TextFilePath = TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileOperations.txt");


            //Act
            actReadTextFile.FileActionMode = ActReadTextFile.eTextFileActionMode.Write;
            actReadTextFile.TextToWrite = txt;
            actReadTextFile.Execute();

            //Assert
            Assert.AreEqual(actReadTextFile.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }
    }
}
