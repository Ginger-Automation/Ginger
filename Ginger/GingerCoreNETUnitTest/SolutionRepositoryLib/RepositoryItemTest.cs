#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Activities;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GingerCoreNETUnitTest.SolutionRepositoryLib
{    
    [Level1]
    [TestClass]
    public class RepositoryItemTest
    {
        static TestHelper mTestHelper = new TestHelper();

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            WorkspaceHelper.CreateWorkspace2();
            mTestHelper.ClassInitialize(TC);

            string path = Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions" + Path.DirectorySeparatorChar + "RepositoryItemTest"));
            WorkSpace.Instance.OpenSolution(path, EncryptionHandler.GetDefaultKey());
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            
        }

        [TestMethod]
        [Timeout(60000)]
        public void BusinessFlowWithOrginalSRItemsCreateCopyTest()
        {
            //Arrange
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow originalFlow = businessFlows.Where(x => x.Name == "Flow 1").FirstOrDefault();

            //Act
            BusinessFlow businessFlowCopy = (BusinessFlow)originalFlow.CreateCopy(true);

            //Assert
            Assert.AreEqual(originalFlow.ActivitiesGroups[0].Guid, businessFlowCopy.ActivitiesGroups[0].ParentGuid);
            Assert.AreEqual(originalFlow.Activities[0].Guid, businessFlowCopy.Activities[0].ParentGuid);
            Assert.AreEqual(originalFlow.Activities[0].Variables[0].Guid, businessFlowCopy.Activities[0].Variables[0].ParentGuid);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BusinessFlowWithSRItemsInstanceCreateCopyTest()
        {
            //Arrange
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow originalFlow = businessFlows.Where(x => x.Name == "Flow 2").FirstOrDefault();

            //Act
            BusinessFlow businessFlowCopy = (BusinessFlow)originalFlow.CreateCopy(true);

            //Assert
            Assert.AreEqual(originalFlow.ActivitiesGroups[0].ParentGuid, businessFlowCopy.ActivitiesGroups[0].ParentGuid);
            Assert.AreEqual(originalFlow.Activities[0].ParentGuid, businessFlowCopy.Activities[0].ParentGuid);
            Assert.AreEqual(originalFlow.Activities[0].Variables[0].ParentGuid, businessFlowCopy.Activities[0].Variables[0].ParentGuid);
        }


        [TestMethod]
        [Timeout(60000)]
        public void ActivityWithOrginalSRItemsCreateCopyTest()
        {
            //Arrange
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            Activity originalActivity = businessFlows.Where(x => x.Name == "Flow 1").FirstOrDefault().Activities[0];

            //Act
            Activity duplicatedActivity = (Activity)originalActivity.CreateCopy(true);

            //Assert
            Assert.AreEqual(Guid.Empty, duplicatedActivity.ParentGuid);            
            Assert.AreEqual(originalActivity.Variables[0].Guid, duplicatedActivity.Variables[0].ParentGuid);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActivityWithSRItemsInstanceCreateCopyTest()
        {
            //Arrange
            ObservableList<BusinessFlow> businessFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            Activity originalActivity = businessFlows.Where(x => x.Name == "Flow 2").FirstOrDefault().Activities[0];

            //Act
            Activity duplicatedActivity = (Activity)originalActivity.CreateCopy(true);

            //Assert
            Assert.AreEqual(Guid.Empty, duplicatedActivity.ParentGuid);
            Assert.AreEqual(originalActivity.Variables[0].ParentGuid, duplicatedActivity.Variables[0].ParentGuid);
        }


        [Ignore]  // FIXME !!!?? Check RepositoryItemBase.CreateCopy - the parent GUID is not copied 
        [TestMethod]
        [Timeout(60000)]
        public void CopyRepositoryItem()
        {
            // Arrange
            Activity a = new Activity();
            a.ActivityName = "A1";

            // Act
            Activity aCopy = (Activity)a.CreateCopy();

            //Assert            
            Assert.AreNotEqual(a, aCopy);  // we should get 2 different objects but with same info
            Assert.AreEqual(a.ActivityName, aCopy.ActivityName);
            Assert.AreNotEqual(a.Guid.ToString(), aCopy.Guid.ToString());
            Assert.AreEqual(a.Guid.ToString(), aCopy.ParentGuid.ToString(), "aCopy.ParentGuid should be a.Guid");   // Fail because parent GUID is not copied
            //TODO: verify actions and more - to make sure copy drill down
            // Verify header !!!
        }


        [TestMethod]
        [Timeout(60000)]
        public void DuplicateRepositoryItem()
        {
            // Arrange
            Activity a = new Activity();
            a.ActivityName = "A2";

            // Act
            Activity aCopy = (Activity)a.CreateCopy();

            //Assert            
            Assert.AreNotEqual(a, aCopy);  // we should get 2 different objects but with same info
            Assert.AreEqual(a.ActivityName, aCopy.ActivityName);
        }


        //TODO: Add more test for all RI functions


      
    }
}
