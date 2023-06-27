using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
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
        [TestInitialize]
        public void TestInitialize()
        {
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
        }
        ActFileOperations actFileOperation = new ActFileOperations();
        [TestMethod]
        [Timeout(60000)]
        public void CopyTest()
        {
            String fileCopy = TestResources.GetTestResourcesFile(@"Files" + Path.DirectorySeparatorChar + "textFileCopy.txt");
            //Arrange 
            if (System.IO.File.Exists(fileCopy))
            {
                System.IO.File.Delete(fileCopy);
            }
            actFileOperation.FileOperationMode = ActFileOperations.eFileoperations.Copy;
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("SourceFilePath", TestResources.GetTestResourcesFile(@"TextFiles" + Path.DirectorySeparatorChar + "textFileToCopy.txt"));
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("DestinationFolder", fileCopy);

            //Act
            actFileOperation.Execute();
            string copyFile = System.IO.File.ReadAllText(fileCopy);

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

        [TestMethod]
        [Timeout(60000)]
        public void DeleteDirectoryTest()
        {
            // Arrange
            string directoryPath = "/path/to/directory";

            // We are creating a test directory and contents in it
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(Path.Combine(directoryPath, "file1.txt"), "File 1 content");
            File.WriteAllText(Path.Combine(directoryPath, "file2.txt"), "File 2 content");

            actFileOperation.FileOperationMode = ActFileOperations.eFileoperations.DeleteDirectory;
            actFileOperation.AddOrUpdateInputParamValueAndCalculatedValue("SourceFilePath", directoryPath);

            // Act
            actFileOperation.Execute();

            // Assert
            Assert.AreNotEqual(actFileOperation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
            Assert.IsFalse(Directory.Exists(directoryPath), "The directory should be deleted.");
            Assert.IsFalse(File.Exists(Path.Combine(directoryPath, "file1.txt")), "File 1 should be deleted.");
            Assert.IsFalse(File.Exists(Path.Combine(directoryPath, "file2.txt")), "File 2 should be deleted.");
        }


    }
}
