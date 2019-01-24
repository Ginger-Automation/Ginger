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

//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace GingerCoreNETUnitTests.SolutionUpgrade
//{
//    [TestClass]
//    public class CompareTest
//    {

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {

//        }


//        [TestInitialize]
//        public void TestInitialize()
//        {

//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }


//        [TestMethod]  [Timeout(60000)]
//        public void ComapreSimpleBFwithNameChange()
//        {
//            //Arrange
//            BusinessFlow BFLeft = new BusinessFlow("BF1");
//            BusinessFlow BFRight = new BusinessFlow("BF1 Modified");
//            RepositoryItemsCompare RIC = new RepositoryItemsCompare();

//            // Act
//            RIC.CompareIt(BFLeft, BFRight);
//            List<CompareDiff> list = RIC.GetDiffs();

//            // Assert
//            CompareDiff CD1 = list.Find(x => x.PropertyName == "Name");
//        }

//    }
//}
