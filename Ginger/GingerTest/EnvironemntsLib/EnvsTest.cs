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

using GingerCore.Environments;
using GingerTestHelper;
using GingerWPFUnitTest.POMs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Threading;

namespace GingerTest
{
    [Ignore]  // temp fail on Azure on class init
    [TestClass]
    [Level3]
    
    public class EnvsTest
    {        
        static GingerAutomator mGingerAutomator; 
        static string SolutionFolder;
        static Mutex mutex = new Mutex();

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {     
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\EnvsTest");
            SolutionFolder = TestResources.GetTestTempFolder(@"Solutions\EnvsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }
            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator = GingerAutomator.StartSession();            
            mGingerAutomator.OpenSolution(SolutionFolder);
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {            
            GingerAutomator.EndSession(); 
        }

        // Run before each test
        [TestInitialize]
        public void TestInitialize()
        {
            mutex.WaitOne();            
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            mutex.ReleaseMutex();            
        }


        

        [TestMethod]  [Timeout(60000)]
        public void VerifyEnvsShowinTree()
        {
            //Arrange            
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();

            //Act
            ProjEnvironment env1 = EnvsPOM.SelectEnvironment("Default");
            EnvsPOM.SelectEnvironment("UAT");
            EnvsPOM.SelectEnvironment("ST");

            // assert
            Assert.AreEqual("Default", env1.Name);
        }


        [TestMethod]  [Timeout(60000)]
        public void AddEnvUsingWizard()
        {
            //Arrange            
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();

            //Act
            EnvsPOM.CreateEnvironment("bbb");
            EnvsPOM.SelectEnvironment("bbb");
            ProjEnvironment bbbEnv = (from x in amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "bbb" select x).SingleOrDefault();

            // assert
            Assert.AreEqual("bbb", bbbEnv.Name);
        }


        [TestMethod]  [Timeout(60000)]
        public void AddEnvToFileSystemWillShowinEnvsTree()
        {
            // Arrange                                                
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();

            // Act

            // Create env on disk
            ProjEnvironment e1 = new ProjEnvironment() { Name = "aaa" };
            string txt = e1.RepositorySerializer.SerializeToString(e1);
            string fileName = Path.Combine(SolutionFolder, @"Environments\aaa.Ginger.Environment.xml");
            File.WriteAllText(fileName, txt);

            // Verify it show in treeview - FileWatcher should detect the new file on disk
            ProjEnvironment aaa = EnvsPOM.SelectEnvironment("aaa");

            //Assert
            Assert.AreEqual("aaa", aaa.Name);
        }



        [TestMethod]  [Timeout(60000)]
        public void DeleteEnvFromFileSystem()
        {
            // Arrange            
            string envName = "Env to del 1";
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();

            // Act
            EnvsPOM.CreateEnvironment(envName);

            string fileName = Path.Combine(SolutionFolder, @"Environments\" + envName + ".Ginger.Environment.xml");
            File.Delete(fileName);

            // Verify it is not in treeview - FileWatcher should detect the delete on disk
            bool envNotExist = EnvsPOM.EnvironmentsTree.IsItemNotExist(envName);

            //Assert
            Assert.AreEqual(true, envNotExist);


        }

        [Ignore] // TODO: FIXME not showing in tree b is false
        [TestMethod]  [Timeout(60000)]
        public void ChangeEnvNameOnDiskUpdateObjandShowinTree()
        {
            //Arrange
            string EnvName = "Env to rename";
            string EnvNewName = "Env to rename ZZZ";
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();
            EnvsPOM.CreateEnvironment(EnvName);
            ProjEnvironment env = EnvsPOM.SelectEnvironment(EnvName);

            //Act
            string txt = File.ReadAllText(env.FilePath);
            txt = txt.Replace(EnvName, EnvNewName);
            File.WriteAllText(env.FilePath, txt);            
            bool b = EnvsPOM.EnvironmentsTree.IsItemExist(EnvNewName);

            // assert
            Assert.AreEqual(EnvNewName, env.Name);
            Assert.IsTrue(b);
        }

       
        [Level3]
        [TestMethod]  [Timeout(60000)]
        public void AddEnvFolderShowinTree()
        {
            //Arrange
            string folderName = "f1";
            string subFolder = Path.Combine(SolutionFolder, "Environments", folderName);
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();


            //Act
            Directory.CreateDirectory(subFolder);
            bool b = EnvsPOM.EnvironmentsTree.IsItemExist(folderName);

            // assert            
            Assert.IsTrue(b);
        }



        [Level3]
        [TestMethod]  [Timeout(60000)]
        public void DeleteEnvFolderRemovedfromTree()
        {
            //Arrange
            string folderName = "folder to delete";
            string subFolder = Path.Combine(SolutionFolder, "Environments", folderName);
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();
            Directory.CreateDirectory(subFolder);

            //Act
            Thread.Sleep(2000);
            bool existBeforeDelete = EnvsPOM.EnvironmentsTree.IsItemExist(folderName);
            Directory.Delete(subFolder);
            Thread.Sleep(2000);
            bool notExistAfterDelete = EnvsPOM.EnvironmentsTree.IsItemNotExist(folderName);

            // assert            
            Assert.IsTrue(existBeforeDelete);
            Assert.IsTrue(notExistAfterDelete);
        }

        [Ignore] //TODO: FIXME 2nd assert fail
        [TestMethod]  [Timeout(60000)]
        public void RenameEnvFolderSyncWithTree()
        {
            //Arrange
            string folderName = "folder to rename";
            string NewfolderName = "My new Name";
            string subFolder = Path.Combine(SolutionFolder, "Environments", folderName);
            string subFolderNewName = Path.Combine(SolutionFolder, "Environments", NewfolderName);
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();
            Directory.CreateDirectory(subFolder);

            //Act
            bool existBeforeDelete = EnvsPOM.EnvironmentsTree.IsItemExist(folderName);
            Directory.Move(subFolder, subFolderNewName);
            bool existAfterRename = EnvsPOM.EnvironmentsTree.IsItemExist(NewfolderName);

            // assert            
            Assert.IsTrue(existBeforeDelete);
            Assert.IsTrue(existAfterRename);
        }

    }
}
