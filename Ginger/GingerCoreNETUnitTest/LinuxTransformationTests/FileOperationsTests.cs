#region License
/*
Copyright © 2014-2025 European Support Limited

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
            //Arrange
            string directoryPath = $"path{Path.DirectorySeparatorChar}to{Path.DirectorySeparatorChar}directory";

            // Create a test directory and its contents
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
