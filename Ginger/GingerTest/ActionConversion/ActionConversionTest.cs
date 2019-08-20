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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using static Amdocs.Ginger.CoreNET.BusinessFlowToConvert;

namespace GingerTest
{
    [TestClass]
    [Level1]
    public class ActionConversionTest
    {        

        static SolutionRepository mSolutionRepository;
        static BusinessFlow mBF;
        static ObservableList<BusinessFlow> mListBF;
        static string solutionName;
        static string MAPPED_TA_FOR_CONVERSION = "MyDosApp";

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            solutionName = "ActionConversionSol";
            CreateTestSolution();

            // Use helper !!!!!

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

        private static void GetMultipleBFActGenElementActions()
        {
            mListBF = new ObservableList<BusinessFlow>();
            BusinessFlow webBF = new BusinessFlow() { Name = "TestBFWebConversion", Active = true };
            BusinessFlow winBF = new BusinessFlow() { Name = "TestBFWinConversion", Active = true };
            BusinessFlow pbBF = new BusinessFlow() { Name = "TestBFPBConversion", Active = true };

            Activity webActivity = new Activity();
            webActivity.SelectedForConversion = true;
            ActGenElement gen1 = new ActGenElement();
            gen1.Active = true;
            gen1.Description = "Set Value : first_name input";
            gen1.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen1.LocateValue = "//input[@name='first_name']";
            gen1.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            webActivity.Acts.Add(gen1);

            webBF.AddActivity(webActivity);
            mListBF.Add(webBF);

            Activity winActivity = new Activity();
            winActivity.SelectedForConversion = true;
            ActGenElement gen2 = new ActGenElement();
            gen2.Active = true;
            gen2.Description = "Set Value : last_name input";
            gen2.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen2.LocateValue = "//input[@name='last_name']";
            gen2.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            winActivity.Acts.Add(gen2);

            winBF.AddActivity(winActivity);
            mListBF.Add(winBF);

            Activity pbActivity = new Activity();
            pbActivity.SelectedForConversion = true;
            ActGenElement gen3 = new ActGenElement();
            gen3.Active = true;
            gen3.Description = "Set Value : email input";
            gen3.LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath;
            gen3.LocateValue = "//input[@name='email']";
            gen3.GenElementAction = ActGenElement.eGenElementAction.SendKeys;
            pbActivity.Acts.Add(gen3);

            pbBF.AddActivity(pbActivity);
            mListBF.Add(pbBF);
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
                                                    bool convertToPOMAction = false, Guid selectedPOM = default(Guid))
        {
            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            ObservableList<Guid> poms = new ObservableList<Guid>() { selectedPOM };
            foreach (var item in lst)
            {
                item.Selected = true;
            }

            
            ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications = new ObservableList<ConvertableTargetApplicationDetails>();
            foreach (var act in mBF.Activities)
            {
                ConvertableTargetApplicationDetails tas = new ConvertableTargetApplicationDetails();
                tas.SourceTargetApplicationName = act.TargetApplication;
                tas.TargetTargetApplicationName = act.TargetApplication;
                convertableTargetApplications.Add(tas);
            }

            BusinessFlowToConvert statusLst = new BusinessFlowToConvert()
            {
                ConversionStatus = eConversionStatus.Pending,
                BusinessFlow = mBF
            };

            utils.ConvertToActions(statusLst, addNewActivity, lst, convertableTargetApplications, convertToPOMAction, poms);
        }

        private static void ExecuteActionConversionForMultipleBF(bool addNewActivity, bool convertoSameTA = true, 
                                                                 bool convertToPOMAction = false, Guid selectedPOM = default(Guid))
        {
            ObservableList<BusinessFlowToConvert> ListOfBusinessFlow = new ObservableList<BusinessFlowToConvert>();
            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lstCad = new ObservableList<ConvertableActionDetails>();
            foreach (var bf in mListBF)
            {
                ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(bf.Activities.ToList());

                foreach (ConvertableActionDetails cad in lst)
                {
                    cad.Selected = true;
                    lstCad.Add(cad);
                }

                BusinessFlowToConvert flowConversion = new BusinessFlowToConvert();
                flowConversion.BusinessFlow = bf;
                flowConversion.ConversionStatus = eConversionStatus.Pending;
                ListOfBusinessFlow.Add(flowConversion);
            }
            ObservableList<Guid> poms = new ObservableList<Guid>() { selectedPOM };
                        
            ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications = new ObservableList<ConvertableTargetApplicationDetails>();
            foreach (var bf in mListBF)
            {
                foreach (var act in bf.Activities)
                {
                    ConvertableTargetApplicationDetails tas = new ConvertableTargetApplicationDetails();
                    tas.SourceTargetApplicationName = act.TargetApplication;
                    if (convertoSameTA)
                    {
                        tas.TargetTargetApplicationName = act.TargetApplication; 
                    }
                    else
                    {
                        tas.TargetTargetApplicationName = MAPPED_TA_FOR_CONVERSION;
                    }
                    convertableTargetApplications.Add(tas);
                } 
            }

            utils.ListOfBusinessFlow = ListOfBusinessFlow;
            utils.ConvertActionsOfMultipleBusinessFlows(lstCad, addNewActivity, convertableTargetApplications, convertToPOMAction, poms);
        }
        
        private static bool ValidateMultipleBFConversionInSameActivity(bool isTASame = true, string mapTA = "")
        {
            bool isValid = true;
            foreach (BusinessFlow bf in mListBF)
            {
                foreach (var activity in bf.Activities)
                {
                    string targetType = ((IObsoleteAction)activity.Acts[0]).TargetAction().ToString();
                    string convertedType = ((GingerCore.Actions.Act)activity.Acts[1]).ActClass;                    
                    if (targetType != convertedType)
                    {
                        isValid = false;
                        break;
                    }

                    string targetLocateType = ((GingerCore.Actions.Act)activity.Acts[0]).LocateBy.ToString();
                    string convertedLocateType = ((GingerCore.Actions.Act)activity.Acts[1]).LocateBy.ToString();
                    if (targetLocateType != convertedLocateType)
                    {
                        isValid = false;
                        break;
                    }

                    string targetLocateValue = ((GingerCore.Actions.Act)activity.Acts[0]).LocateValue.ToString();
                    string convertedLocateValue = ((GingerCore.Actions.Act)activity.Acts[1]).LocateValue.ToString();
                    if (targetLocateValue != convertedLocateValue)
                    {
                        isValid = false;
                        break;
                    }

                    if(!isTASame)
                    {
                        string actTA = activity.TargetApplication;
                        if (mapTA != actTA)
                        {
                            isValid = false;
                            break;
                        }
                    }
                }
            }

            return isValid;
        }

        private static bool ValidateMultipleBFConversionInNewActivity(bool isTASame = true, string mapTA = "")
        {
            bool isValid = true;
            foreach (BusinessFlow bf in mListBF)
            {
                Activity sourceActivity = bf.Activities[0];
                Activity targetActivity = bf.Activities[1];
                string targetType = ((IObsoleteAction)sourceActivity.Acts[0]).TargetAction().ToString();
                string convertedType = ((GingerCore.Actions.Act)targetActivity.Acts[0]).ActClass;
                if (targetType != convertedType)
                {
                    isValid = false;
                    break;
                }

                string targetLocateType = ((GingerCore.Actions.Act)sourceActivity.Acts[0]).LocateBy.ToString();
                string convertedLocateType = ((GingerCore.Actions.Act)targetActivity.Acts[0]).LocateBy.ToString();
                if (targetLocateType != convertedLocateType)
                {
                    isValid = false;
                    break;
                }

                string targetLocateValue = ((GingerCore.Actions.Act)sourceActivity.Acts[0]).LocateValue.ToString();
                string convertedLocateValue = ((GingerCore.Actions.Act)targetActivity.Acts[0]).LocateValue.ToString();
                if (targetLocateValue != convertedLocateValue)
                {
                    isValid = false;
                    break;
                }

                if (!isTASame)
                {
                    string actTA = targetActivity.TargetApplication;
                    if (mapTA != actTA)
                    {
                        isValid = false;
                        break;
                    }
                }
            }

            return isValid;
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToSameActivityWithSameTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(false);
                        
            bool isValid = ValidateMultipleBFConversionInSameActivity();

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToSameActivityWithDiffTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(false, false, true, default(Guid));

            bool isValid = ValidateMultipleBFConversionInSameActivity(false, MAPPED_TA_FOR_CONVERSION);

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToSameActivityWithPOMWithSameTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(false, convertToPOMAction: true, selectedPOM: default(Guid));

            bool isValid = ValidateMultipleBFConversionInSameActivity();

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToSameActivityWithPOMWithDiffTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(false, false, convertToPOMAction: true, selectedPOM: default(Guid));

            bool isValid = ValidateMultipleBFConversionInSameActivity(false, MAPPED_TA_FOR_CONVERSION);

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToNewActivityWithSameTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(true);

            bool isValid = ValidateMultipleBFConversionInNewActivity();

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToNewActivityWithDiffTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(true, false);

            bool isValid = ValidateMultipleBFConversionInNewActivity(false, MAPPED_TA_FOR_CONVERSION);

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToNewActivityWithPOMWithSameTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(true, convertToPOMAction: true, selectedPOM: default(Guid));

            bool isValid = ValidateMultipleBFConversionInNewActivity();

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void MultipleBFActionConversionToNewActivityWithPOMWithDiffTATest()
        {
            GetMultipleBFActGenElementActions();

            ExecuteActionConversionForMultipleBF(true, false, convertToPOMAction: true, selectedPOM: default(Guid));

            bool isValid = ValidateMultipleBFConversionInNewActivity(false, MAPPED_TA_FOR_CONVERSION);

            ////Assert
            Assert.AreEqual(isValid, true);
        }

        [TestMethod]
        [Timeout(60000)]
        public void SingleBFActionConversionToSameActivityTest()
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
        public void SingleBFNoActionConversionToSameActivityTest()
        {
            GetActivityWithActUIElementActions();
            
            ExecuteActionConversion(false, true, string.Empty);
            
            ////Assert
            Assert.IsFalse(mBF.Activities[0].Acts[0] is IObsoleteAction);
            Assert.AreEqual(mBF.Activities[0].Acts.Count(), 1);
        }        

        [TestMethod]
        [Timeout(60000)]
        public void SingleBFActionConversionToNewActivityTest()
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
        public void SingleBFNoActionConversionToNewActivityTest()
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
        public void SingleBFActionConversionToSameActivityPOMActionTest()
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
        public void SingleBFNoActionConversionToSameActivityPOMActionTest()
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
