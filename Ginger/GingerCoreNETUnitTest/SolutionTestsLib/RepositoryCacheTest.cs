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

//using Amdocs.Ginger.Repository;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace GingerCoreNETUnitTest.SolutionTestsLib
//{
//    [Level1]
//    [TestClass]
//    public class RepositoryCacheTest
//    {
//        RepositoryCache RC = new RepositoryCache(typeof(BusinessFlow));

//        [TestInitialize]
//        public void TestInitialize()
//        {

//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }

//        //[TestMethod]
//        //public void WeakReferenceTest()
//        //{
//        //    Stopwatch st = new Stopwatch();
//        //        //BusinessFlow BF = new BusinessFlow();
//        //    WeakReference WR = new WeakReference(st);
//        //    st = null;
//        //    while(WR.IsAlive)
//        //    {
//        //        Thread.Sleep(100);
//        //    }

//        //    Assert.IsTrue(WR.IsAlive == false);            
//        //}
        
//        [TestMethod]
//        public void CachecSimpleInOut()
//        {
//            //Arrange            
//            BusinessFlow BF = new BusinessFlow();
//            BF.Name = "hello";

//            //Act
//            RC[BF.Name] = BF;
//            BusinessFlow BF2 = (BusinessFlow)RC[BF.Name];

//            //Assert            
//            Assert.AreEqual(BF, BF2); // make sure they are same object/ref
//        }

//        //[TestMethod]
//        //public void CachecWithObjRelease()
//        //{
//        //    //Arrange            
//        //    string key = "CachecWithObjRelease";

//        //    BusinessFlow BF = new BusinessFlow();
//        //    BF.Name = "My BF 1";

//        //    //Act
//        //    RC[key] = BF;
//        //    BF = null; // release
            
//        //    BusinessFlow BF2 = (BusinessFlow)RC[key];

//        //    //Assert            
//        //    Assert.IsTrue(BF2 == null, "BF2 == null"); // make sure obj not found in cache and is released
//        //}


//        //[TestMethod]
//        //public void CachecWithObjReleaseAndGCCollect()
//        //{
//        //    //Arrange            
//        //    string key = "CachecWithObjReleaseAndGCCollect";

//        //    BusinessFlow BF = new BusinessFlow();
//        //    BF.Name = "My BF 1";

//        //    //Act
//        //    RC[key] = BF;
//        //    BF = null; // release

//        //    GC.Collect(); // ((GC.MaxGeneration, GCCollectionMode.Forced);

//        //    BusinessFlow BF2 = (BusinessFlow)RC[key];

//        //    //Assert            
//        //    Assert.IsTrue(BF2==null, "BF2 == null"); // make sure obj not found in cache is released
//        //}
//    }
//}
