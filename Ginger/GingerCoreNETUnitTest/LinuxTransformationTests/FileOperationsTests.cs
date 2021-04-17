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
    public class FileOperationsTests
    {
        ActFileOperations actFileOperation = new ActFileOperations();
        [TestMethod]
        [Timeout(60000)]
        public void CopyTest()
        {
            //Arrange 
            actFileOperation.FileOperationMode = ActFileOperations.eFileoperations.Copy;
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("SourceFilePath", TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileToCopy.txt"));
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("DestinationFolder", TestResources.GetTestResourcesFile(@"Files" + Path.DirectorySeparatorChar + "textFileCopy.txt"));

            //Act
            actFileOperation.Execute();
            string copyFile = System.IO.File.ReadAllText(TestResources.GetTestResourcesFile(@"Files" + Path.DirectorySeparatorChar + "textFileToCopy.txt"));

            //Assert
            Assert.AreEqual(copyFile, "text file to copy");
        }
        [TestMethod]
        [Timeout(60000)]
        public void CheckFileExistsTest()
        {
            //Arrange 
            actFileOperation.FileOperationMode = ActFileOperations.eFileoperations.CheckFileExists;
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("SourceFilePath", TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileToCopy.txt"));
            
            //Act
            actFileOperation.Execute();

            //Assert
            Assert.AreNotEqual(actFileOperation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
        }

    }
}
