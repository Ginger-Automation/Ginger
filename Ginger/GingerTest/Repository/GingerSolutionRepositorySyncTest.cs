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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using Ginger.Repository.ItemToRepositoryWizard;
using GingerCore;
using GingerCore.Variables;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerTest
{    
    [Ignore] // get stuck
    [TestClass]
    [Level1]
    public class GingerSolutionRepositorySyncTest
    {        

        static SolutionRepository mSolutionRepository;
        static BusinessFlow mBF;        
        static string solutionName; 

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            solutionName = "SRVarSync";
            CreateTestSolution();

            // Creating workspace
            WorkSpace.Init(new WorkSpaceEventHandler());
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            Ginger.App.InitClassTypesDictionary();
            string TempRepositoryFolder = TestResources.GetTestTempFolder(@"Solutions\" + solutionName);
            mSolutionRepository.Open(TempRepositoryFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {            
            

        }


        [TestCleanup]
        public void TestCleanUp()
        {

        }

        private static void CreateTestSolution()
        {
            // First we create a basic solution with some sample items
            SolutionRepository SR = new SolutionRepository();
            string TempRepositoryFolder = TestResources.GetTestTempFolder(@"Solutions\" + solutionName);
            if (Directory.Exists(TempRepositoryFolder))
            {
                Directory.Delete(TempRepositoryFolder, true);
            }

            SR = GingerSolutionRepository.CreateGingerSolutionRepository();
            SR.Open(TempRepositoryFolder);

            SR.Close();
        }


        [TestMethod]  [Timeout(60000)]
        public void TestBusinessFlowVariableSyncWithRepo()
        {
            string variableName = "BFV1";
            string initialValue = "123";
            string updatedValue = "abc123";

            mBF = new BusinessFlow() { Name= "TestBFVarSync", Active=true };

            VariableString V1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };
            mBF.AddVariable(V1);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            // prepare to add the variable to the shared repository
            UploadItemSelection uploadItemSelection = new UploadItemSelection() { UsageItem = V1, ItemUploadType = UploadItemSelection.eItemUploadType.New};
            (new SharedRepositoryOperations()).UploadItemToRepository(new Context() { BusinessFlow = mBF }, uploadItemSelection);

            // find the newly added variable from the shared repo
            VariableBase sharedVariableBase = (from x in mSolutionRepository.GetAllRepositoryItems<VariableBase>() where x.Name == variableName select x).SingleOrDefault();
            VariableString sharedV1 = (VariableString)sharedVariableBase;

            //update the new value in the shared repo variable
            sharedV1.InitialStringValue = updatedValue;

            //sync the updated instance with the business flow instance
            sharedV1.UpdateInstance(V1, "All", mBF);

            // get the updated value from the business flow
            VariableString V2 = (VariableString)mBF.Variables[0];

            //Assert
            Assert.AreEqual(1, mBF.Variables.Count);
            Assert.AreNotSame(V1, V2);
            Assert.AreEqual(updatedValue, V2.InitialStringValue);
        }

        [TestMethod]  [Timeout(60000)]
        public void TestActivityVariablesSyncWithRepo()
        {
            string variableName = "ACTVAR1";
            string initialValue = "123";
            string updatedValue = "abc123";

            mBF = new BusinessFlow() { Name = "TestActvVarSync", Active = true };
            mBF.Activities = new ObservableList<Activity>();

            VariableString V1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };

            // add variable to the activity
            Activity activity = new Activity() { ActivityName = "Activity1" };
            activity.AddVariable(V1);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            // prepare to add the variable to the shared repository
            UploadItemSelection uploadItemSelection = new UploadItemSelection() { UsageItem = V1, ItemUploadType = UploadItemSelection.eItemUploadType.New };
            (new SharedRepositoryOperations()).UploadItemToRepository(new Context() { BusinessFlow = mBF }, uploadItemSelection);

            // find the newly added variable in the shared repo
            VariableBase sharedVariableBase = (from x in mSolutionRepository.GetAllRepositoryItems<VariableBase>() where x.Name == variableName select x).SingleOrDefault();
            VariableString sharedV1 = (VariableString)sharedVariableBase;

            //update the new value in the shared repo variable
            sharedV1.InitialStringValue = updatedValue;

            //sync the updated instance with the business flow instance
            sharedV1.UpdateInstance(V1, "All", activity);

            // get the updated value from the business flow
            VariableString V2 = (VariableString) activity.Variables[0];

            //Assert
            Assert.AreEqual(1, activity.Variables.Count);
            Assert.AreNotSame(V1, V2);
            Assert.AreEqual(updatedValue, V2.InitialStringValue);
        }


        [TestMethod]  [Timeout(60000)]
        public void TestActivityVariablesSyncWithRepo_v2()
        {
            string variableName = "ACTVAR2";
            string initialValue = "123";
            string updatedValue = "abc123";

            mBF = new BusinessFlow() { Name = "TestActvVarSyncV2", Active = true };
            mBF.Activities = new ObservableList<Activity>();

            VariableString V1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };

            // add variable to the activity
            Activity activity = new Activity() { ActivityName = "Activity1" };
            activity.AddVariable(V1);
            mBF.Activities.Add(activity);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            // prepare to add the variable to the shared repository
            UploadItemSelection uploadItemSelection = new UploadItemSelection() { UsageItem = V1, ItemUploadType = UploadItemSelection.eItemUploadType.New };
            (new SharedRepositoryOperations()).UploadItemToRepository(new Context() { BusinessFlow = mBF }, uploadItemSelection);

            // find the newly added variable in the shared repo
            VariableBase sharedVariableBase = (from x in mSolutionRepository.GetAllRepositoryItems<VariableBase>() where x.Name == variableName select x).SingleOrDefault();
            VariableString sharedV1 = (VariableString)sharedVariableBase;

            //update the new value in the shared repo variable
            sharedV1.InitialStringValue = updatedValue;

            //sync the updated instance with the business flow instance
            sharedV1.UpdateInstance(V1, "All", mBF.Activities[0]);

            // get the updated value from the business flow
            VariableString V2 = (VariableString)mBF.Activities[0].Variables[0];

            //Assert
            Assert.AreEqual(1, mBF.Activities.Count);
            Assert.AreEqual(1, mBF.Activities[0].Variables.Count);
            Assert.AreNotSame(V1, V2);
            Assert.AreEqual(updatedValue, V2.InitialStringValue);
        }


        //[TestMethod]  [Timeout(60000)]
        //public void TestSolutionVariablesSyncWithRepo()
        //{
        //    string variableName = "SOLVAR1";
        //    string initialValue = "123";
        //    string updatedValue = "abc123";

        //    mBF = new BusinessFlow() { Name = "TestSolutionVarSync", Active = true };
        //    mBF.Activities = new ObservableList<Activity>();

        //    VariableString V1 = new VariableString() { Name = variableName, InitialStringValue = initialValue };

        //    // add variable to the activity
        //    mSolutionRepository.AddRepositoryItem(V1);

        //    // prepare to add the variable to the shared repository
        //    UploadItemSelection uploadItemSelection = new UploadItemSelection() { UsageItem = V1, ItemUploadType = UploadItemSelection.eItemUploadType.New };
        //    SharedRepositoryOperations.UploadItemToRepository(uploadItemSelection);

        //    // find the newly added variable in the shared repo
        //    VariableBase sharedVariableBase = (from x in mSolutionRepository.GetAllRepositoryItems<VariableBase>() where x.Name == variableName select x).SingleOrDefault();
        //    VariableString sharedV1 = (VariableString)sharedVariableBase;

        //    //update the new value in the shared repo variable
        //    sharedV1.InitialStringValue = updatedValue;

        //    //sync the updated instance with the business flow instance
        //    sharedV1.UpdateInstance(V1, "All", mSolutionRepository.);

        //    // get the updated value from the business flow
        //    VariableString V2 = (VariableString)mBF.Activities[0].Variables[0];

        //    //Assert
        //    Assert.AreEqual(1, mBF.Activities.Count);
        //    Assert.AreEqual(1, mBF.Activities[0].Variables.Count);
        //    Assert.AreNotSame(V1, V2);
        //    Assert.AreEqual(updatedValue, V2.InitialStringValue);
        //}
      
    }
}
