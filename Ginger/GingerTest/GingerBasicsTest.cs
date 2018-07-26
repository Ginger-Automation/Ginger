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

using GingerCore.Environments;
using GingerTestHelper;
using GingerWPFUnitTest.GeneralLib;
using GingerWPFUnitTest.POMs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static GingerWPFUnitTest.Scenarios;

namespace GingerWPFUnitTest
{
    [TestClass]
    //[Level1]
    public class GingerBasicsTest
    {        
        static TestContext mTC;
        static string LogFile;
        static GingerAutomator mGingerAutomator = new GingerAutomator();
        static string SolutionFolder;
        Mutex mutex = new Mutex();

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {            
            mTC = TC;
            mGingerAutomator.StartGinger();
            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\EnvsTest");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\EnvsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder,true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);
            
            mGingerAutomator.OpenSolution(SolutionFolder);            

            LogFile = mTC.TestLogsDir + @"\Ginger_BasicsTest.txt";         
        }

        static void LogTest(Scenario scenario)
        {
            string s = "GingerWPF," + scenario.ID + "," + scenario.Area + "," + scenario.SubArea + "," + scenario.Description + "," + scenario.Priority + Environment.NewLine;
            File.AppendAllText(LogFile,s);
        }

        [ClassCleanup]        
        public static void ClassCleanup()
        {
            //File.Copy(LogFile, @"c:\temp\GingerWPF\" + @"\GingerWPF_BasicsTest.txt");
            //string s = mTC.TestDir;
            //string s2 = mTC.TestRunResultsDirectory;
            mGingerAutomator.CloseGinger();
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



        [TestMethod]  
        [Ignore]
        public void CheckTabsWhenSolutionClosed()
        {
            //Arrange

            //Act            
            mGingerAutomator.CloseSolution();
            List<string> visibileTabs = mGingerAutomator.MainWindowPOM.GetVisibleRibbonTabs();
            string tabs = string.Join(",", visibileTabs);

            //Assert

            Assert.AreEqual("HomeRibbon,SolutionRibbon,SupportRibbon", tabs);
        }

        [TestMethod]
        public void CheckTabsWhenSolutionOpen()
        {
            //Arrange

            //Act            
            mGingerAutomator.OpenSolution(SolutionFolder);
            List<string> visibileTabs = mGingerAutomator.MainWindowPOM.GetVisibleRibbonTabs();
            string tabs = string.Join(",", visibileTabs);

            //Assert
            Assert.AreEqual("HomeRibbon,SolutionRibbon,AutomateRibbon,RunRibbon,xConfigurations,xResources,SupportRibbon", tabs);
        }



        [TestMethod]
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


        [TestMethod]
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


        [TestMethod]
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


        
        [TestMethod]
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
            bool b = EnvsPOM.EnvironmentsTree.IsItemExist(envName);

            //Assert
            Assert.AreEqual(false, b);


        }

        [TestMethod]
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
            txt = txt.Replace(EnvName, EnvName + " ZZZ");
            File.WriteAllText(env.FilePath, txt);
            bool b = EnvsPOM.EnvironmentsTree.IsItemExist(EnvNewName);

            // assert
            Assert.AreEqual(EnvNewName, env.Name);
            Assert.IsTrue(b);
        }

        [TestMethod]
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


        [TestMethod]
        public void DeleteEnvFolderRemovedfromTree()
        {
            //Arrange
            string folderName = "folder to delete";
            string subFolder = Path.Combine(SolutionFolder, "Environments", folderName);
            EnvironmentsPOM EnvsPOM = mGingerAutomator.MainWindowPOM.GotoEnvironments();
            Directory.CreateDirectory(subFolder);

            //Act
            bool existBeforeDelete = EnvsPOM.EnvironmentsTree.IsItemExist(folderName);
            Directory.Delete(subFolder);
            bool existAfterDelete = EnvsPOM.EnvironmentsTree.IsItemExist(folderName);

            // assert            
            Assert.IsTrue(existBeforeDelete);
            Assert.IsFalse(existAfterDelete);
        }


        [TestMethod]
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
