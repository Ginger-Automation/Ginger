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

//using amdocs.ginger.GingerCoreNET;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNETUnitTest.RunTestslib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.WorkSpaceLib
//{
//    [TestClass]
//    public class UserProfileTest
//    {

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TestContext)
//        {
//            DummyWorkSpace ws = new DummyWorkSpace();
//            WorkSpace.Init(ws);            
//        }


//        [TestMethod]
//        public void NewProfileSaveLoad()
//        {
//            //Arrange                        
//            UserProfile UP = new UserProfile();
//            string UserProfileFileName = Path.Combine(Common.getGingerUnitTesterTempFolder(), @"UserProfile.Ginger.xml");                        
//            // UP.FileName = UserProfileFileName;            
//            WorkSpace.Instance.UserProfile = UP;

//            /// hard coded !!!!!!!!!!!!!!!!
//            string LastSolutionFolder = @"c:\ginger\sol1";  // just something to verify it is loaded later doesn't need to exist

//            //Act
//            UP.AddsolutionToRecent(LastSolutionFolder);   
//            WorkSpace.Instance.UserProfile.Save(UserProfileFileName);
//            UserProfile UP2 = UserProfile.LoadUserProfile(UserProfileFileName);

//            //Assert
//            Assert.AreEqual(LastSolutionFolder, UP2.RecentSolutions[0]);
//        }

//        [TestMethod]
//        public void CreateUserProfileFileName()
//        {
//            // Arrange            

//            //Act
//            string s = UserProfile.CreateUserProfileFileName();

//            //Assert
//            string username = Environment.UserName.ToLower();
//            Assert.AreEqual(@"C:\Users\" + username + @"\AppData\Roaming\Ginger\" + username + ".Ginger.UserProfile.xml", s);
//        }

//    }
//}
