#region License
/*
Copyright © 2014-2019 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Ginger;
using Ginger.SolutionGeneral;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace GingerCoreNETUnitTest.WorkSpaceLib
{
    [TestClass]
    public class UserProfileTest
    {

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            DummyWorkSpace ws = new DummyWorkSpace();
            WorkSpace.Init(ws);
        }


        [TestMethod]
        [Ignore] // Failing !!! needs fix!!! in UserProfile have one list of recent
        public void NewProfileSaveLoad()
        {
            //Arrange                        
            UserProfile userProfile = new UserProfile();
            // string UserProfileFileName = Path.Combine(TestResources.GetTempFile("UserProfile.Ginger.xml"));
            // UP.FileName = UserProfileFileName;            
            // WorkSpace.Instance.UserProfile = userProfile;

            
            string LastSolutionFolder = @"c:\ginger\sol1";  
            Solution solution = new Solution() {Name = "sol1" , Folder = LastSolutionFolder }; // just something to verify it is loaded later doesn't need to exist


            //Act
            userProfile.AddSolutionToRecent(solution);
            userProfile.SaveUserProfile();

            // WorkSpace.Instance.UserProfile = new UserProfile

            UserProfile UP2 = UserProfile.LoadUserProfile();

            //Assert
            Assert.AreEqual(LastSolutionFolder, UP2.RecentSolutions[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CreateUserProfileFileName()
        {
            //// Arrange            

            ////Act
            //string s = UserProfile.crea.CreateUserProfileFileName();

            ////Assert
            //string username = Environment.UserName.ToLower();
            //Assert.AreEqual(@"C:\Users\" + username + @"\AppData\Roaming\Ginger\" + username + ".Ginger.UserProfile.xml", s);
        }

    }
}
