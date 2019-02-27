#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.Actions;
using Amdocs.Ginger.CoreNET;
using System.Collections.Generic;
using GingerCore.Actions.Common;

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
        
        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToSameActivityTest()
        {
            mBF = new BusinessFlow() { Name= "TestBFConversion", Active=true };
            
            Activity activity = new Activity();
            activity.SelectedForConversion = true;
            ActGenElement gen1 = new ActGenElement();
            gen1.Active = true;
            gen1.Description = "Set Value : first_name input";
            gen1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen1.LocateValue = "//input[@name='first_name']";
            gen1.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            activity.Acts.Add(gen1);

            ActGenElement gen2 = new ActGenElement();
            gen2.Active = true;
            gen2.Description = "Set Value : last_name input";
            gen2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName;
            gen2.LocateValue = "last_name";
            gen2.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            activity.Acts.Add(gen2);

            mBF.AddActivity(activity);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(false, mBF, lst, true, string.Empty);

            Activity newActivity = mBF.Activities[0];
            int count = newActivity.Acts.Count();

            ////Assert
            Assert.AreEqual(count, 4);            
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToSameActivityTest()
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

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(false, mBF, lst, true, string.Empty);

            Activity newActivity = mBF.Activities[0];
            int count = newActivity.Acts.Count();

            ////Assert
            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToNewActivityTest()
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

            ActGenElement gen2 = new ActGenElement();
            gen2.Active = true;
            gen2.Description = "Set Value : last_name input";
            gen2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName;
            gen2.LocateValue = "last_name";
            gen2.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            activity.Acts.Add(gen2);

            mBF.AddActivity(activity);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(true, mBF, lst, true, string.Empty);

            int count = mBF.Activities.Count();

            ////Assert
            Assert.AreEqual(count, 2);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToNewActivityTest()
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

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(true, mBF, lst, true, string.Empty);

            Activity newActivity = mBF.Activities[0];
            int count = newActivity.Acts.Count();

            ////Assert
            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ActionConversionToSameActivityPOMActionTest()
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

            ActGenElement gen2 = new ActGenElement();
            gen2.Active = true;
            gen2.Description = "Set Value : last_name input";
            gen2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName;
            gen2.LocateValue = "last_name";
            gen2.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            activity.Acts.Add(gen2);

            mBF.AddActivity(activity);

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(false, mBF, lst, true, string.Empty, true);

            Activity newActivity = mBF.Activities[0];
            int count = newActivity.Acts.Count();

            ////Assert
            Assert.AreEqual(count, 4);
        }

        [TestMethod]
        [Timeout(60000)]
        public void NoActionConversionToSameActivityPOMActionTest()
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

            // add business flow to the solution repository
            mSolutionRepository.AddRepositoryItem(mBF);

            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            foreach (var item in lst)
            {
                item.Selected = true;
            }
            utils.ConvertToActions(false, mBF, lst, true, string.Empty, true);

            Activity newActivity = mBF.Activities[0];
            int count = newActivity.Acts.Count();

            ////Assert
            Assert.AreEqual(count, 1);
        }
    }
}
