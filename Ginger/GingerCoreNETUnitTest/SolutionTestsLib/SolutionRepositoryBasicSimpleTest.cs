//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.EnvironmentsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.IO;
//using System.Threading;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.SolutionTestsLib
//{
//    [Level1]
//    [TestClass]
//    public class SolutionRepositoryBasicSimpleTest
//    {
        
//        static SolutionRepository SR;
//        // Using Mutex for BF because Backup and restore can modify the same instance, so make sure we run one test at a time
//        static Mutex mBusinessFlowsMutex = new Mutex();

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TC)
//        {            
//            // BasicSimple is solution which is created automatically when creating new solution, no other items added.
//            string path = Path.Combine(Common.GetTestResourcesFolder(), @"Solutions\BasicSimple");
//            SR = new SolutionRepository();
//            SR.Open(path);            
//        }

//        [ClassCleanup]
//        public static void ClassCleanup()
//        {

//        }

//        [TestInitialize]
//        public void TestInitialize()
//        {
//            mBusinessFlowsMutex.WaitOne();
//        }

//        [TestCleanup]
//        public void TestCleanup()
//        {
//            mBusinessFlowsMutex.ReleaseMutex();
//        }


        
//        [TestMethod]
//        public void GetSolutionFolders()
//        {
//            //Arrange            

//            //Act
//            //List<RepositoryFolder> list = SR.GetRootFolders();

//            ////Assert
//            //// Make sure folders are coming in the right order
//            //Assert.AreEqual(list[0].FolderPath, @"~\Applications Models\API Models");            

//            ////No matter the user terminology seletced we always keep the folder on disk as BusinessFlows !!!!!!!!!!!!            
//            //Assert.AreEqual(@"~\BusinessFlows", list[1].FolderPath);
//            //Assert.AreEqual(@"~\Environments", list[2].FolderPath );
//            //Assert.AreEqual(@"~\Agents", list[3].FolderPath);
//            //Assert.AreEqual(@"~\SharedRepository", list[4].FolderPath );
//            //Assert.AreEqual(@"~\RunSetConfigs", list[5].FolderPath );
//            //Assert.AreEqual(@"~\HTMLReportConfigurations", list[6].FolderPath );
//            //Assert.AreEqual(@"~\Documents", list[7].FolderPath);

//        }

//        [TestMethod]
//        public void FoldersTerminologyTest()
//        {

//            //TODO: test with different terminology, make sure when we work as QC or BDD the folder name on disk remains the same 'Business Flows', but the user see - 'Test Sets' etc..

//            //Arrange            

//            //Act


//            //Assert
            


//        }

      


       

//        [TestMethod]
//        public void GetSolutionEnvs()
//        {
//            //Arrange            

//            //Act
//            ObservableList<ProjEnvironment> list = SR.GetAllRepositoryItems<ProjEnvironment>();

//            //Assert
//            Assert.AreEqual(list.Count, 1);
//            Assert.AreEqual(list[0].Name, "Default");

//        }
        
//        [TestMethod]
//        public void UpdateBFInMemoryAndRequest()
//        {
//            //Arrange                   
//            ObservableList<BusinessFlow> list1 = SR.GetAllRepositoryItems<BusinessFlow>();
//            BusinessFlow BF1 = list1[0];            
//            string BFNewName = "BFF";

//            //Act
//            BF1.Name = BFNewName;

//            // request again the BFs - we expect to get same object 

//            ObservableList<BusinessFlow> list2 = SR.GetAllRepositoryItems<BusinessFlow>();
//            BusinessFlow BF2 = list2[0];

//            //Assert
//            Assert.AreEqual(BF2.Name, BFNewName);   
//            Assert.AreEqual(BF1, BF2); // Make sure it is same ref
            


//        }

        

//        [TestMethod]
//        public void GetSolutionBusinessFlows()
//        {
//            //Arrange            
            
//            //Act
//            ObservableList<BusinessFlow> list = SR.GetAllRepositoryItems<BusinessFlow>();

//            //Assert
//            Assert.AreEqual(1, list.Count, "list.Count=1");
//            Assert.AreEqual("4484405c-0cf7-4d8b-95e9-97df761fdcc3", list[0].Guid.ToString());
//            Assert.AreEqual(1, list[0].Activities.Count, "list[0].Activities.Count=1");


//        }
        
//        [TestMethod]
//        public void BF_BackupAndRestore()
//        {
//            //Arrange                        
//            ObservableList<BusinessFlow> list1 = SR.GetAllRepositoryItems<BusinessFlow>();
//            BusinessFlow BF1 = list1[0];

//            //Act            
//            BF1.SaveBackup();
//            string BFOriginalName = BF1.Name;
//            string BFNewName = "Temp Junk name - DO Not Save or show to any!";
//            BF1.Name = BFNewName;


//            BF1.RestoreFromBackup();
         
//            ObservableList<BusinessFlow> list2 = SR.GetAllRepositoryItems<BusinessFlow>();
//            BusinessFlow BF2 = list2[0];

//            //Assert
//            Assert.AreEqual(BF2.Name, BFOriginalName);  // Make sure we didn't get the one from mem                        
//        }
//    }
//}
