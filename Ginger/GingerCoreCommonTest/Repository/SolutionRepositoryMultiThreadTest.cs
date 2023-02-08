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
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GingerCoreCommonTest.Repository
{

    // Tests to verify that Solution Repository is multi threaded 
    
    [TestClass]    
    [Level1]
    public class SolutionRepositoryMultiThreadTest
    {
        static SolutionRepository mSolutionRepository;
        static NewRepositorySerializer mRepositorySerializer;
        static string TempRepositoryFolder;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {            
            TempRepositoryFolder = TestResources.GetTestTempFolder("Solutions", "SRMultiThreadTestTemp");
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

        private static void CreateTestSolution()
        {
            // First we create a basic solution with some sample items
            SolutionRepository SR = new SolutionRepository();                        
            if (Directory.Exists(TempRepositoryFolder))
            {
                Directory.Delete(TempRepositoryFolder, true);
            }

            // Define the items types we want in our Repository            
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

            RepositoryFolder<MyRepositoryItem> bigFolder = (RepositoryFolder<MyRepositoryItem>)MyRepoRF.AddSubFolder("BigfolderWith100Items");


            // Add another 100 big item so it will take time loading and we can check multi thread access while load in progress
            for (int i=0;i<100;i++)
            {
                MyRepositoryItem bigItem = new MyRepositoryItem("Big Item #" + i);
                bigItem.BigStringHolderSlowSet = new string('a', 150000);
                bigFolder.AddRepositoryItem(bigItem);
            }


            //TODO: add more sample items for testing
            SR.Close();
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }


        [TestMethod]
        public void zz()
        {
            //Arrange
            int t1Count = 0;


            //Act            

            ObservableList<MyRepositoryItem> allMRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
            t1Count = allMRIs.Count;
        }

        // run two threads which does GetAllitems
        [TestMethod]
        public void GetAllRepositoryItemsUsingParallel()
        {
            //Arrange
            int t1Count=0;
            int t2Count=0;

            //Act            
            Task t1 = new Task(() => 
            {
                ObservableList<MyRepositoryItem> allMRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
                t1Count = allMRIs.Count;
                Console.WriteLine("t1.End");
            });
            Task t2 = new Task(() =>
            {
                ObservableList<MyRepositoryItem> allMRIs = mSolutionRepository.GetAllRepositoryItems<MyRepositoryItem>();
                t2Count = allMRIs.Count;
                Console.WriteLine("t2.End");
            });

            t1.Start();
            Console.WriteLine("t1.Start");

            // Let the first thread start and init the list, so second should wait and verify we don't get partial list            
            Thread.Sleep(50);
            // since each item load take ~10ms (using sleep in set of attr) the first thread will load about 5 items
            // then we kick off the second thread
            t2.Start();
            // Note t2 should wait for t1 to complete get all items, if it goes back too fast it means it got partial list
            Console.WriteLine("t2.Start");
            Task.WaitAll(t1, t2);

            //Assert  

            Assert.IsTrue(t1Count == t2Count, "t1=t2 Count");
            Assert.AreEqual(t1Count, 106, "t1Count");
            Assert.AreEqual(t2Count, 106, "t2Count");

        }


       




    }
}
