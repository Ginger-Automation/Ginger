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
using Amdocs.Ginger.Common;
using GingerCore;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using GingerCore.Actions;
using Amdocs.Ginger.CoreNET;
using GingerCore.Actions.Common;
using System;
using Amdocs.Ginger.Repository;
using GingerWPF.WorkSpaceLib;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using System.IO;

namespace GingerTest
{
    [TestClass]
    [Level1]
    public class ActionConversionTest
    {
        static SolutionRepository mSolutionRepository;
        static BusinessFlow mBF; 
        static string solutionName;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            solutionName = "ActionConversionSol";
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

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        private static void GetActivityWithActGenElementActions()
        {
            mBF = new BusinessFlow() { Name = "TestBFConversion", Active = true };

            Activity activity = new Activity();
            activity.SelectedForConversion = true;
            ActGenElement gen1 = new ActGenElement();
            gen1.Active = true;
            gen1.Description = "Set Value : first_name input";
            gen1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen1.LocateValue = "//input[@name='first_name']";
            gen1.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            activity.Acts.Add(gen1);

            mBF.AddActivity(activity);
        }

        private static void GetActivityWithActUIElementActions()
        {
            mBF = new BusinessFlow() { Name = "TestBFConversion", Active = true };

            Activity activity = new Activity();
            activity.SelectedForConversion = true;

            ActUIElement gen1 = new ActUIElement();
            gen1.Active = true;
            gen1.Description = "Set Value : first_name input";
            gen1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen1.LocateValue = "//input[@name='first_name']";
            gen1.ElementType = Amdocs.Ginger.Common.UIElement.eElementType.TextBox;
            gen1.ElementAction = ActUIElement.eElementAction.SendKeys;
            gen1.ElementLocateValue = "";
            activity.Acts.Add(gen1);

            mBF.AddActivity(activity);
        }

        private static void ExecuteActionConversion(bool addNewActivity, bool isDefaultTargetApp, string strTargetApp,
                                                    bool convertToPOMAction = false, string selectedPOMObjectName = "")
        {
            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(addNewActivity, mBF, lst, isDefaultTargetApp, strTargetApp, convertToPOMAction, selectedPOMObjectName);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToSameActivityTest()
        {
            GetActivityWithActGenElementActions();
            
            ExecuteActionConversion(false, true, string.Empty);

            string targetType = ((IObsoleteAction)mBF.Activities[0].Acts[0]).TargetAction().ToString();
            string convertedType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).ActClass;

            string targetLocateType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateBy.ToString();
            string convertedLocateType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).LocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).LocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count(), 1);
            Assert.AreEqual(targetType, convertedType);
            Assert.AreEqual(targetLocateType, convertedLocateType);
            Assert.AreEqual(targetLocateValue, convertedLocateValue);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToSameActivityTest()
        {
            GetActivityWithActUIElementActions();
            
            ExecuteActionConversion(false, true, string.Empty);
            
            ////Assert
            Assert.IsFalse(mBF.Activities[0].Acts[0] is IObsoleteAction);
            Assert.AreEqual(mBF.Activities[0].Acts.Count(), 1);
        }        

        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToNewActivityTest()
        {
            GetActivityWithActGenElementActions();
            
            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ExecuteActionConversion(true, true, string.Empty);

            string targetType = ((IObsoleteAction)mBF.Activities[0].Acts[0]).TargetAction().ToString();
            string convertedType = ((GingerCore.Actions.Act)mBF.Activities[1].Acts[0]).ActClass;

            string targetLocateType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateBy.ToString();
            string convertedLocateType = ((GingerCore.Actions.Act)mBF.Activities[1].Acts[0]).LocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Act)mBF.Activities[1].Acts[0]).LocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count(), 2);
            Assert.AreEqual(targetType, convertedType);
            Assert.AreEqual(targetLocateType, convertedLocateType);
            Assert.AreEqual(targetLocateValue, convertedLocateValue);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToNewActivityTest()
        {
            GetActivityWithActUIElementActions();
            
            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ExecuteActionConversion(true, true, string.Empty);

            ////Assert
            Assert.IsFalse(mBF.Activities[0].Acts[0] is IObsoleteAction);
            Assert.AreEqual(mBF.Activities.Count(), 1);
            Assert.AreEqual(mBF.Activities[0].Acts.Count(), 1);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToSameActivityPOMActionTest()
        {
            GetActivityWithActGenElementActions();
            
            ExecuteActionConversion(false, true, string.Empty, true);

            string targetType = ((IObsoleteAction)mBF.Activities[0].Acts[0]).TargetAction().ToString();
            string convertedType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).ActClass;

            string targetLocateType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateBy.ToString();
            string convertedLocateType = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).LocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[1]).LocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count(), 1);
            Assert.AreEqual(targetType, convertedType);
            Assert.AreEqual(targetLocateType, convertedLocateType);
            Assert.AreEqual(targetLocateValue, convertedLocateValue);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToSameActivityPOMActionTest()
        {
            GetActivityWithActUIElementActions();
            
            ExecuteActionConversion(false, true, string.Empty, true);

            ////Assert
            Assert.IsFalse(mBF.Activities[0].Acts[0] is IObsoleteAction);
            Assert.AreEqual(mBF.Activities.Count(), 1);
            Assert.AreEqual(mBF.Activities[0].Acts.Count(), 1);
        }
    }
}
