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

using Ginger;
using Ginger.SolutionGeneral;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace GingerCoreNETUnitTest.WorkSpaceLib
{
    [TestClass]
    public class UserProfileTest
    {
        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            WorkspaceHelper.CreateDummyWorkSpace(nameof(UserProfileTest));            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            
        }

        [Ignore]
        [TestMethod]
         // Failing !!! needs fix!!! in UserProfile have one list of recent as string and one as objects 
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
            // Arrange            

            //Act
            string userProfileFilePath = UserProfile.UserProfileFilePath;

            string expected;
            // if windows            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                expected = @"C:\Users\" + Environment.UserName.ToLower() + @"\AppData\Roaming\Amdocs\Ginger\" + "Ginger.UserProfile.xml";
            }
            else  // Linux /Mac
            {
                expected = @"/data/" + Environment.UserName + @"/AppData/Amdocs/Ginger/" + "Ginger.UserProfile.xml";
            }

            //Assert

            Assert.AreEqual(expected, userProfileFilePath, "UserProfileFilePath");   
        }

    

    }
}
