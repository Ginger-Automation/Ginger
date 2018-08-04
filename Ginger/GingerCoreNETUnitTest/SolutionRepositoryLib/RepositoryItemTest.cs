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

//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace GingerCoreNETUnitTest.SolutionRepositoryLib
//{
//    [Level1]
//    [TestClass]
//    public class RepositoryItemTest
//    {
//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TC)
//        {

//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }

        
//        [Ignore]
//        [TestMethod]
//        public void CopyRepositoryItem()
//        {
//            // Arrange
//            Activity a = new Activity();
//            a.ActivityName = "A1";

//            // Act
//            Activity aCopy = (Activity)a.CreateCopy();

//            //Assert            
//            Assert.AreNotEqual(a, aCopy);  // we should get 2 different objects but with same info
//            Assert.AreEqual(a.ActivityName, aCopy.ActivityName);
//            Assert.AreNotEqual(a.Guid.ToString(), aCopy.Guid.ToString());
//            Assert.AreEqual(a.Guid.ToString(), aCopy.ParentGuid.ToString(), "aCopy.ParentGuid should be a.Guid");  
//            //TODO: verify actions and more - to make copy goes down
//            // Verify header !!!
//        }


//        [TestMethod]
//        public void DuplicateRepositoryItem()
//        {
//            // Arrange
//            Activity a = new Activity();
//            a.ActivityName = "A2";

//            // Act
//            Activity aCopy = (Activity)a.CreateCopy();

//            //Assert            
//            Assert.AreNotEqual(a, aCopy);  // we should get 2 different objects but with same info
//            Assert.AreEqual(a.ActivityName, aCopy.ActivityName);
//        }


//        //TODO: Add more test for all RI functions

//    }
//}
