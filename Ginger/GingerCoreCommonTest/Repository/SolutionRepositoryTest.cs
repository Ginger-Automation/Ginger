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
using Amdocs.Ginger.Repository;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace GingerCoreCommonTest.Repository
{


    // all tests are done on one simple RI name MyRepositoryItem - this is SR with one item type
    // Test Basic CRUD using SolutionRepository - Create/Read/Update/Delete
    // Test fetching items by GUID
    // Test fetching items by Name
    // Test sub folders 
    // Test File detect when change    
    // Test Multi thread getting the same data - repository folder
    // Test Big solution

    [TestClass]    
    [Level1]
    public class SolutionRepositoryTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        static SolutionRepository mSolutionRepository;
        static NewRepositorySerializer mRepositorySerializer;
        static string TempRepositoryFolder;

        

        


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);

            TempRepositoryFolder = TestResources.GetTestTempFolder("Solutions", "SRTestTemp");
            Console.WriteLine("SolutionRepositoryTest folder: " + TempRepositoryFolder);

            Console.WriteLine("===> Creating test solution");
            CreateTestSolution();
            Console.WriteLine("===> test solution created");

            mRepositorySerializer = new NewRepositorySerializer();
            // Init SR
            mSolutionRepository = new SolutionRepository();
            mSolutionRepository.AddItemInfo<MyRepositoryItem>( pattern: "*.Ginger.MyRepositoryItem.xml",   // Need to use for file name 
                                                               rootFolder: SolutionRepository.cSolutionRootFolderSign + "MyRepositoryItems", 
                                                               containRepositoryItems: true, 
                                                               displayName: "My Repository Item", 
                                                               PropertyNameForFileName: nameof(MyRepositoryItem.Name)
                                                               );

            NewRepositorySerializer RS = new NewRepositorySerializer();                        
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommonTest);            
            mSolutionRepository.Open(TempRepositoryFolder);            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        private static void CreateTestSolution()
        {
            // First we create a basic solution with some sample items
            SolutionRepository SR = new SolutionRepository();                        
            if (Directory.Exists(TempRepositoryFolder))
            {
                Directory.Delete(TempRepositoryFolder, true);
            }

            // Define the items types we want in our Repository
            //SR.AddItemInfo<MyRepositoryItem>("*.Ginger.MyRepositoryItem.xml", @"~\MyRepositoryItems", true, "My Repository Item", addToRootFolders: true, PropertyNameForFileName: nameof(MyRepositoryItem.Name));            
            SR.AddItemInfo<MyRepositoryItem>(pattern: "*.Ginger.MyRepositoryItem.xml",   // Need to use for file name 
                                                               rootFolder: SolutionRepository.cSolutionRootFolderSign + "MyRepositoryItems",
                                                               containRepositoryItems: true,
                                                               displayName: "My Repository Item",
                                                               PropertyNameForFileName: nameof(MyRepositoryItem.Name)
                                                               );


            SR.CreateRepository(TempRepositoryFolder);
            SR.Open(TempRepositoryFolder);            

            MyRepositoryItem A1 = new MyRepositoryItem("A1");
            SR.AddRepositoryItem(A1);

            MyRepositoryItem A2 = new MyRepositoryItem("A2");
            SR.AddRepositoryItem(A2);

            MyRepositoryItem A3 = new MyRepositoryItem("A3");
            SR.AddRepositoryItem(A3);


            RepositoryFolder<MyRepositoryItem> MyRepoRF = SR.GetRepositoryItemRootFolder<MyRepositoryItem>();
            RepositoryFolder<MyRepositoryItem> SubFolder1 = (RepositoryFolder<MyRepositoryItem>)MyRepoRF.AddSubFolder("SubFolder1");

            MyRepoRF.AddSubFolder("EmptySubFolder");

            MyRepositoryItem A4 = new MyRepositoryItem("A4");
            SubFolder1.AddRepositoryItem(A4);

            // Folder to delete later
            MyRepoRF.AddSubFolder("SubFolderForDelete");

            // Create folders tree
            RepositoryFolder<MyRepositoryItem> SF1 = (RepositoryFolder<MyRepositoryItem>)MyRepoRF.AddSubFolder("SF1");
            RepositoryFolder<MyRepositoryItem> SF2 = (RepositoryFolder<MyRepositoryItem>)SF1.AddSubFolder("SF1_SF2");
            RepositoryFolder<MyRepositoryItem> SF3 = (RepositoryFolder<MyRepositoryItem>)SF2.AddSubFolder("SF1_SF2_SF3");
            MyRepositoryItem BF5 = new MyRepositoryItem("A5");
            SubFolder1.AddRepositoryItem(BF5);

            MyRepositoryItem BF6 = new MyRepositoryItem("A6");
            SF3.AddRepositoryItem(BF6);

            //TODO: add more sample items for testing
            SR.Close();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
        }


        [TestMethod]
        public void GetAllRepositoryItems()
        {
            Console.WriteLine("===> Test 1 start");
            //Arrange

            //Act            
            ObservableList<MyRepositoryItem> allMRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();


            //Assert  

            // Verify we got item from root folder
            Assert.AreEqual("A1", (from x in allMRIs where x.Name == "A1" select x.Name).SingleOrDefault());
            // Verify we got item from sub/sub folder
            // Assert.AreEqual("A6", (from x in allMRIs where x.Name == "A6" select x.Name).SingleOrDefault());

            Console.WriteLine("===> Test 1 end");
        }


        [TestMethod]
        public void AddNewMyRepositoryItemAndVerifyFileSaved()
        {
            //Arrange
            string MRIName = "MRI Save 1";
            MyRepositoryItem MRI1 = new MyRepositoryItem(MRIName);

            //Act            
            mSolutionRepository.AddRepositoryItem(MRI1);
            MyRepositoryItem MRI2 = (MyRepositoryItem)mRepositorySerializer.DeserializeFromFile(MRI1.FilePath);

            //Assert            
            Assert.AreEqual(MRI1.Guid, MRI2.Guid);
            Assert.AreEqual(MRIName, MRI2.Name);
        }


        [TestMethod]
        public void GetRepositoryItemByName()
        {
            //Arrange            

            //Act            
            MyRepositoryItem MRI1 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A1" select x).FirstOrDefault();

            //Assert            
            Assert.IsTrue(MRI1 != null);
        }

        [TestMethod]
        public void UpdateRepositoryItem()
        {
            //Arrange  
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();
            MyRepositoryItem MRI2 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A2" select x).FirstOrDefault();

            //Act            
            MRI2.Name = "A2 New Name";
            mSolutionRepository.SaveRepositoryItem(MRI2);
            MyRepositoryItem MRI2FromDisk = (MyRepositoryItem)RepositorySerializer.DeserializeFromFile(MRI2.FilePath);


            //Assert            
            Assert.AreEqual(MRI2.Guid, MRI2FromDisk.Guid);
            Assert.AreEqual(MRI2.Name, MRI2FromDisk.Name);
        }

        [TestMethod]
        public void DeleteRepositoryItem()
        {
            //Arrange            
            MyRepositoryItem MRI3 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A3" select x).FirstOrDefault();

            //Act                        
            mSolutionRepository.DeleteRepositoryItem(MRI3);
            MyRepositoryItem MRI3AfterDelete = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A3" select x).FirstOrDefault();


            //Assert            
            Assert.IsTrue(!File.Exists(MRI3.FilePath));
            Assert.AreEqual(MRI3AfterDelete, null);
        }


        [TestMethod]
        public void AddNewRepositoryItemThenGetByName()
        {
            //Arrange
            string MRIName = "New MRI1";
            MyRepositoryItem MRI1 = new MyRepositoryItem(MRIName);

            //Act            
            mSolutionRepository.AddRepositoryItem(MRI1);


            // Get all files and find out just created BF by Name
            MyRepositoryItem MRI2 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == MRIName select x).FirstOrDefault();

            //Assert
            Assert.AreEqual(MRI1, MRI2);
            Assert.AreEqual(MRI1.Guid, MRI2.Guid);
        }

        [TestMethod]
        public void AddNewMyRepositoryItemAndGetByGuid()
        {
            //Arrange
            MyRepositoryItem MRI = new MyRepositoryItem("New MRI2");

            //Act            
            mSolutionRepository.AddRepositoryItem(MRI);
            MyRepositoryItem MRIByGuid = mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(MRI.Guid);

            //Assert
            Assert.AreEqual(MRI, MRIByGuid);
        }


        [TestMethod]
        public void AddAndDeleteRepositoryItem()
        {
            //Arrange
            MyRepositoryItem MRI = new MyRepositoryItem("New MRI to be deleted");
            Guid guid = MRI.Guid;
            mSolutionRepository.AddRepositoryItem(MRI);  // Here we should have the file on disk
            string FileName = MRI.FilePath;

            // Call get MRIs so the MRI will be also in a list cache, so we can validate removal from list too
            ObservableList<MyRepositoryItem> MRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
            //The added MRI in Arrange will appear in the list

            //Act            
            mSolutionRepository.DeleteRepositoryItem(MRI);

            // Try to get from cache which should bring null
            MyRepositoryItem MRI2 = (MyRepositoryItem)mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(guid);

            // Get it from all MRIs - will use the list cache - should be null too
            ObservableList<MyRepositoryItem> MRIs2 = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
            MyRepositoryItem MRI22 = (from x in MRIs2 where x.Guid == MRI.Guid select x).FirstOrDefault();

            //Assert            
            Assert.AreEqual(MRI2, null, "Delete RI not found from cache using Guid");
            Assert.AreEqual(File.Exists(FileName), false, "File not exist on disk");
            Assert.AreEqual(null, MRI22, "Deleted BF not found in Business Flows");
        }


        [TestMethod]
        public void GetMRIListThenAddNewMRI()
        {
            //Arrange

            MyRepositoryItem MRI = new MyRepositoryItem("Another MRI ");
            ObservableList<MyRepositoryItem> MRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();

            //Act            
            mSolutionRepository.AddRepositoryItem(MRI);
            // Now we get again all MRIs and want to see that new MRI is included  -checking the list cache
            ObservableList<MyRepositoryItem> MRIs2 = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
            MyRepositoryItem MRx = (from x in MRIs2 where x.Guid == MRI.Guid select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(MRI, MRx);
            Assert.AreEqual(MRIs, MRIs2);
        }


        [TestMethod]
        public void AddMyRepositoryItemToSubFolder()
        {
            //Arrange
            string MRIName = "AddMyRepositoryItemToSubFolder_New MRI";
            MyRepositoryItem MRI = new MyRepositoryItem(MRIName);
            RepositoryFolder<MyRepositoryItem> subfolder = (from x in mSolutionRepository.GetRepositoryItemRootFolder<MyRepositoryItem>().GetSubFolders() where x.DisplayName == "EmptySubFolder" select x).FirstOrDefault();

            //Act            
            subfolder.AddRepositoryItem(MRI);
            MyRepositoryItem MRI2 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == MRIName select x).FirstOrDefault();

            //Assert
            Assert.AreEqual(MRI, MRI2);
            Assert.AreEqual(MRI.ContainingFolder, subfolder.FolderRelativePath);
        }


        [TestMethod]
        public void DeleteMyRepositoryItemFromSubFolder()
        {
            //Arrange
            MyRepositoryItem MRI = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A4" select x).FirstOrDefault();

            //Act            
            mSolutionRepository.DeleteRepositoryItem(MRI);

            MyRepositoryItem MRI2 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A4" select x).FirstOrDefault();
            MyRepositoryItem MRI3 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Guid == MRI.Guid select x).FirstOrDefault();

            //Assert
            Assert.AreEqual(MRI2, null);
            Assert.AreEqual(MRI3, null);
            Assert.IsTrue(File.Exists(MRI.FilePath) == false);

        }

        [TestMethod]
        public void DeleteMyRepositoryItemFromSubSubsubFolder()
        {
            //Arrange
            MyRepositoryItem MRI = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A5" select x).FirstOrDefault();

            //Act            
            mSolutionRepository.DeleteRepositoryItem(MRI);

            MyRepositoryItem MRI2 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Name == "A5" select x).FirstOrDefault();
            MyRepositoryItem MRI3 = (from x in mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>() where x.Guid == MRI.Guid select x).FirstOrDefault();

            //Assert
            Assert.AreEqual(MRI2, null);
            Assert.AreEqual(MRI3, null);
            Assert.IsTrue(File.Exists(MRI.FilePath) == false);
        }

        //FIXME
        [TestMethod]
        public void DeleteMRIsSubFolder()
        {
            //Arrange            
            //add new sub folder with new bf's to be deleted
            RepositoryFolder<MyRepositoryItem> MRIRF = mSolutionRepository.GetRepositoryItemRootFolder<MyRepositoryItem>();
            RepositoryFolder<MyRepositoryItem> folderToDelete = (RepositoryFolder<MyRepositoryItem>)MRIRF.AddSubFolder("DeleteSubFolder_FolderForDelete");
            MyRepositoryItem MRI1 = new MyRepositoryItem("DeleteSubFolder_MRI1");
            folderToDelete.AddRepositoryItem(MRI1);
            MyRepositoryItem MRI2 = new MyRepositoryItem("DeleteSubFolder_MRI2");
            folderToDelete.AddRepositoryItem(MRI2);
            //add new sub-sub folder with new bf's to be deleted
            RepositoryFolder<MyRepositoryItem> subfolderToDelete = (RepositoryFolder<MyRepositoryItem>)folderToDelete.AddSubFolder("DeleteSubFolder_subfolderToDelete");
            MyRepositoryItem MRI3 = new MyRepositoryItem("DeleteSubFolder_MRI3");
            subfolderToDelete.AddRepositoryItem(MRI3);

            //Act  
            mSolutionRepository.DeleteRepositoryItemFolder(folderToDelete);

            //Assert
            Assert.IsTrue(Directory.Exists(folderToDelete.FolderFullPath) == false, "Verify Directory not exist");
            Assert.AreEqual((mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(MRI1.Guid)), null, "make sure all deleted folder items were removed from cache");
            Assert.AreEqual((mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(MRI3.Guid)), null, "make sure all deleted folder sub folder items were removed from cache");
        }


        //FIXME
        [TestMethod]
        public void DeleteBfsSubFolderWithAllItemsLoaded()
        {
            //Arrange       
            ObservableList<MyRepositoryItem> MRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
            //add new sub folder with new bf's to be deleted
            RepositoryFolder<MyRepositoryItem> MRIRF = mSolutionRepository.GetRepositoryItemRootFolder<MyRepositoryItem>();
            RepositoryFolder<MyRepositoryItem> folderToDelete = (RepositoryFolder<MyRepositoryItem>)MRIRF.AddSubFolder("DeleteSubFolder_FolderForDelete");
            MyRepositoryItem MRI1 = new MyRepositoryItem("DeleteSubFolder_MRI1");
            folderToDelete.AddRepositoryItem(MRI1);
            MyRepositoryItem MRI2 = new MyRepositoryItem("DeleteSubFolder_MRI2");
            folderToDelete.AddRepositoryItem(MRI2);
            //add new sub-sub folder with new MRI's to be deleted
            RepositoryFolder<MyRepositoryItem> subfolderToDelete = (RepositoryFolder<MyRepositoryItem>)folderToDelete.AddSubFolder("DeleteSubFolder_subfolderToDelete");
            MyRepositoryItem MRI3 = new MyRepositoryItem("DeleteSubFolder_MRI3");
            subfolderToDelete.AddRepositoryItem(MRI3);

            //Act  
            mSolutionRepository.DeleteRepositoryItemFolder(folderToDelete);

            //Assert
            Assert.IsTrue(Directory.Exists(folderToDelete.FolderFullPath) == false);
            Assert.AreEqual((mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(MRI1.Guid)), null, "make sure all deleted folder items were removed from cache");
            Assert.AreEqual((mSolutionRepository.GetRepositoryItemByGuid<MyRepositoryItem>(MRI3.Guid)), null, "make sure all deleted folder sub folder items were removed from cache");
        }

        //FIXME

        //[TestMethod]
        //public void RenameSubSubFolder()
        //{
        //    //Arrange            
        //    //add new sub folder with new bf's to be renamed
        //    RepositoryFolder<MyRepositoryItem> BFRF = mSolutionRepository.GetRepositoryItemRootFolder<MyRepositoryItem>();
        //    RepositoryFolder<MyRepositoryItem> folderToRename = (RepositoryFolder<MyRepositoryItem>)BFRF.AddSubFolder("RenameBfsSubFolder_FolderToRename");
        //    MyRepositoryItem RI1 = new MyRepositoryItem("FolderToRename_RI1");
        //    folderToRename.AddRepositoryItem(RI1);
        //    MyRepositoryItem RI2 = new MyRepositoryItem("FolderToRename_RI2");
        //    folderToRename.AddRepositoryItem(RI2);
        //    //add new sub-sub folder with RI under the folder which will be renamed
        //    RepositoryFolder<MyRepositoryItem> subfolderUnderRenamedFolder = (RepositoryFolder<MyRepositoryItem>)folderToRename.AddSubFolder("RenameBfsSubFolder_subfolderUnderRenamedFolder");
        //    MyRepositoryItem RI3 = new MyRepositoryItem("FolderToRename_RI3");
        //    subfolderUnderRenamedFolder.AddRepositoryItem(RI3);

        //    string newName = "RenameBfsSubFolder_NewName";

        //    //Act    
        //    Thread.Sleep(100);
        //    folderToRename.RenameFolder(newName);
        //    ObservableList<MyRepositoryItem> bfs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();

        //    //Assert
        //    Assert.IsTrue(folderToRename.FolderRelativePath.Contains(newName), "Validate folder relative path was updated");
        //    Assert.IsTrue(folderToRename.FolderFullPath.Contains(newName), "Validate folder full path was updated");
        //    Assert.IsTrue(Directory.Exists(folderToRename.FolderFullPath), "Validate folder full path is valid");
        //    Assert.AreEqual(folderToRename.DisplayName, newName, "Validate folder Display Name is correct");
        //    Assert.AreEqual(folderToRename.FolderName, newName, "Validate Folder Name is correct");
        //    Assert.IsTrue(subfolderUnderRenamedFolder.FolderRelativePath.Contains(newName), "Validate sub folder relative path was updated");
        //    Assert.IsTrue(subfolderUnderRenamedFolder.FolderFullPath.Contains(newName), "Validate sub folder full path was updated");
        //    Assert.IsTrue(Directory.Exists(subfolderUnderRenamedFolder.FolderFullPath), "Validate sub folder full path is valid");
        //    Assert.IsTrue(RI1.ContainingFolder.Contains(newName), "Validate level 1 BF ContainingFolder was updated");
        //    Assert.IsTrue(RI1.ContainingFolderFullPath.Contains(newName), "Validate level 1 BF ContainingFolderFullPath was updated");
        //    Assert.IsTrue(RI1.FilePath.Contains(newName), "Validate level 1 BF FilePath was updated");
        //    Assert.IsTrue(File.Exists(RI1.FilePath), "Validate level 1 BF FilePath is valid");
        //    Assert.IsTrue(RI3.ContainingFolder.Contains(newName), "Validate level 2 BF ContainingFolder was updated");
        //    Assert.IsTrue(RI3.ContainingFolderFullPath.Contains(newName), "Validate level 2 BF ContainingFolderFullPath was updated");
        //    Assert.IsTrue(RI3.FilePath.Contains(newName), "Validate level 2 BF FilePath was updated");
        //    Assert.IsTrue(File.Exists(RI3.FilePath), "Validate level 2 BF FilePath is valid");
        //    Assert.AreEqual(bfs.Where(x => x.Guid == RI1.Guid).ToList().Count, 1, "Make sure level 1 item is not loaded to cache more than once");
        //    Assert.AreEqual(bfs.Where(x => x.Guid == RI3.Guid).ToList().Count, 1, "Make sure level 2 item is not loaded to cache more than once");
        //}



        [TestMethod]
        public void VerifyBFRepositoryItemHeader()
        {
            //Arrange
            MyRepositoryItem MRI = new MyRepositoryItem("MRI New v1");

            //Act
            mSolutionRepository.AddRepositoryItem(MRI);

            //Assert
            Assert.AreEqual(MRI.RepositoryItemHeader.CreatedBy, Environment.UserName);
            Assert.AreEqual(MRI.RepositoryItemHeader.Version, 1);
            Assert.AreEqual(MRI.RepositoryItemHeader.LastUpdateBy, Environment.UserName);
        }

        [TestMethod]
        public void VerifyBFRepositoryItemHeaderVersionChangeAfterSave()
        {
            //Arrange
            MyRepositoryItem MRI = new MyRepositoryItem("MRI New v0");
            mSolutionRepository.AddRepositoryItem(MRI);

            //Act
            mSolutionRepository.SaveRepositoryItem(MRI);

            //Assert            
            Assert.AreEqual(MRI.RepositoryItemHeader.Version, 2);

        }


        [TestMethod]
        public void ValidatelongFileMorethan255()
        {
            //Arrange
            MyRepositoryItem mri = new MyRepositoryItem("My long name repository item My long name repository item My long name repository item My long name repository item  My long name repository item My long name repository itemMy long name repository item My long name repository item My long name repository item My long name repository item My long name repository item My long name repository item");
            mSolutionRepository.AddRepositoryItem(mri);

            //Act
            mSolutionRepository.SaveRepositoryItem(mri);

            //Assert            
            Assert.AreEqual(mri.RepositoryItemHeader.Version, 2);
        }

       

        [TestMethod]
        public void ValidateInvlidCharsinFilename()
        {
            // This behaviour is different on Windows and Linux - some chars are allowed on Linux for example '?' 

            //Arrange            
            string invalidPathChars = "?";
            MyRepositoryItem MRI = new MyRepositoryItem("MRI with invalid char for file name " + invalidPathChars);

            //Act
            mSolutionRepository.AddRepositoryItem(MRI);

            //Assert            
            Assert.IsTrue(File.Exists(MRI.FilePath));
            Assert.IsFalse(MRI.FilePath.Contains(invalidPathChars));
        }


        //[TestMethod]
        //public void SaveRIThenLoadChanegNameSaveCheckFileName()
        //{
        //    //Arrange
        //    MyRepositoryItem MRI = new MyRepositoryItem("MRI New v0");
        //    mSolutionRepository.AddRepositoryItem(MRI);

        //    //Act
        //    mSolutionRepository.SaveRepositoryItem(MRI);

        //    //Assert            
        //    Assert.AreEqual(MRI.RepositoryItemHeader.Version, 2);
        //}

        //[TestMethod]
        //public void ItemNotSerilizedIsNotSaved()
        //{
        //    //Arrange
        //    MyRepositoryItem BF = new MyRepositoryItem("BF New v0");
        //    mSolutionRepository.AddRepositoryItem(BF);

        //    //Act
        //    mSolutionRepository.SaveRepositoryItem(BF);

        //    //Assert            
        //    Assert.AreEqual(BF.RepositoryItemHeader.Version, 2);
        //}


        [TestMethod]
        public void FileWatcherChangeExisitingMRIOnDisk()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))//not needed on other OS types
            {
                return;
            }

            //Arrange
            ObservableList<MyRepositoryItem> allMRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();

            string name = "BF MRI file v0";
            string newName = "NEW MRI change name manually";
            MyRepositoryItem BF = new MyRepositoryItem(name);
            mSolutionRepository.AddRepositoryItem(BF);

            //Act
            //mSolutionRepository.StartFileWatcher();
            string txt = System.IO.File.ReadAllText(BF.FilePath);
            string txt2 = txt.Replace(name, newName);
            System.IO.File.WriteAllText(BF.FilePath, txt2);
            // Here we expect an event of file change

            // mSolutionRepository.StopFileWatcher();

            Stopwatch st = Stopwatch.StartNew();
            while (BF.Name != newName && st.ElapsedMilliseconds < 5000) // Max 5 seconds or time out
            {
                Thread.Sleep(100);
            }

            //Assert            
            Assert.AreEqual(newName, BF.Name, "Bf.Name is the one modified on file manually");
        }




    }
}
