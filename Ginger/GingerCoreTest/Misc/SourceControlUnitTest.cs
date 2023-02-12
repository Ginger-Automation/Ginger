#region License
/*
Copyright © 2014-2023 European Support Limited

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
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;



namespace UnitTests.NonUITests
{
    [Ignore]     // Missing SharpSVN - do not want to add SharpSVN or GIT Nuget to GingerCoreNET core - need to be sepearte proj TODO: seperate as part of Linux proj
    [TestClass]
    public class SourceControlUnitTest
    {
        private static SourceControlBase SourceControl;
        string error = string.Empty;
        bool res = false;
        string SolutionFolder = TestResources.GetTempFolder("SourceControl");
   
        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] { "https://svn.riouxsvn.com/dummytest", "Dummy", "DontKnow", "SVN" };
            yield return new object[] { "https://github.com/NewDummyUser/Dummy", "NewDummyUser", "TempPassword!1234", "GIT" };
        }
        
        [DataTestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void GetSCType (string SourceControlURL, string SourceControlUser, string SourceControlPass, string SourceControlType)
        {
            if (SourceControlType == "SVN")
            {
                SourceControl = new SVNSourceControl();
            }
            else
            {
                SourceControl = new GITSourceControl();
            }
            SourceControl.SourceControlURL = SourceControlURL;
            SourceControl.SourceControlUser = SourceControlUser;
            SourceControl.SourceControlPass = SourceControlPass;
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void TestConnection(string SourceControlURL, string SourceControlUser, string SourceControlPass, string SourceControlType)
        {
            //Arrange
            GetSCType(SourceControlURL, SourceControlUser, SourceControlPass, SourceControlType);

            //Act
            res = SourceControl.TestConnection(ref error);

            //Assert
            Assert.AreEqual(res, true);
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void GetProjectList(string SourceControlURL, string SourceControlUser, string SourceControlPass, string SourceControlType)
        {
            //Arrange
            TestConnection(SourceControlURL, SourceControlUser, SourceControlPass, SourceControlType);
            ObservableList<SolutionInfo> SourceControlSolutions = new ObservableList<SolutionInfo>();

            //Act
            SourceControlSolutions = SourceControl.GetProjectsList();

            //Assert
            Assert.AreEqual(SourceControlSolutions.Count, 1);
        }
        private static void DeleteDirectory(string directory)
        {
            if (Directory.Exists(directory))
            {
                foreach (string subdirectory in Directory.EnumerateDirectories(directory))
                {
                    DeleteDirectory(subdirectory);
                }

                foreach (string fileName in Directory.EnumerateFiles(directory))
                {
                    var fileInfo = new FileInfo(fileName)
                    {
                        Attributes = FileAttributes.Normal
                    };
                    fileInfo.Delete();
                }

                Directory.Delete(directory);
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void GetProject(string SourceControlURL, string SourceControlUser, string SourceControlPass, string SourceControlType)
        {

            //Arrange
            string workingDirectory = Path.Combine(SolutionFolder, SourceControlType);

            DeleteDirectory(workingDirectory);
           
            TestConnection(SourceControlURL, SourceControlUser, SourceControlPass, SourceControlType);
            string error = string.Empty;
            bool getproj = false;

            //Act
            getproj = SourceControl.GetProject(workingDirectory, SourceControlURL, ref error);

            //Assert
            Assert.AreEqual(getproj, true);
        }

        [TestMethod]
        [DynamicData(nameof(GetData), DynamicDataSourceType.Method)]
        public void CheckIn(string SourceControlURL, string SourceControlUser, string SourceControlPass, string SourceControlType)
        {
            //Arrange
            TestConnection(SourceControlURL, SourceControlUser, SourceControlPass, SourceControlType);
            GetProject(SourceControlURL, SourceControlUser, SourceControlPass, SourceControlType);
            string workingDirectory = Path.Combine(SolutionFolder, SourceControlType);
            SourceControl.SolutionFolder = workingDirectory;
            File.WriteAllText(Path.Combine(workingDirectory, "Dummy.txt"), File.ReadAllText(Path.Combine(workingDirectory, "Dummy.txt")).Replace("123", "1234"));
            string error = string.Empty;
            bool checkin = false;
            List<string> pathsToCommit = new List<string>();
            pathsToCommit.Add(Path.Combine(workingDirectory, "Dummy.txt"));
            List<string> conflictsPaths = new List<string>();

            //Act
            checkin = SourceControl.CommitChanges(pathsToCommit, "aaa", ref error, ref conflictsPaths, false);

            //Assert
            Assert.AreEqual(checkin, true);
        }


    }
}
