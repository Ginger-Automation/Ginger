#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Actions.Java;
using GingerCore.Environments;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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
            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();
            WorkSpace.Instance.SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();

            // Init SR
            mSolutionRepository = WorkSpace.Instance.SolutionRepository;
            Ginger.App.InitClassTypesDictionary();
            string TempRepositoryFolder = TestResources.GetTestTempFolder(@"Solutions\" + solutionName);
            mSolutionRepository.Open(TempRepositoryFolder);

            Ginger.SolutionGeneral.Solution sol = new Ginger.SolutionGeneral.Solution
            {
                ApplicationPlatforms =
            [
                new GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ApplicationPlatform()
                {
                    AppName = "Web-App",
                    Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Web
                },
                new GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ApplicationPlatform()
                {
                    AppName = "Java-App",
                    Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Java
                },
                new GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ApplicationPlatform()
                {
                    AppName = "MyJavaApp",
                    Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Java
                },
                new GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ApplicationPlatform()
                {
                    AppName = "Window-App",
                    Platform = GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib.ePlatformType.Windows
                },
            ]
            };

            WorkSpace.Instance.Solution = sol;
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

            ProjEnvironment E1 = new ProjEnvironment() { Name = "E1" };
            SR.AddRepositoryItem(E1);

            SR.Close();
        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        private static void GetActivityWithActGenElementActions()
        {
            mBF = new BusinessFlow() { Name = "TestBFConversion", Active = true };

            Activity activity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = "Web-App"
            };
            ActGenElement gen1 = new ActGenElement
            {
                Active = true,
                Description = "Set Value : first_name input",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath,
                LocateValue = "//input[@name='first_name']",
                GenElementAction = ActGenElement.eGenElementAction.SendKeys
            };
            activity.Acts.Add(gen1);

            mBF.AddActivity(activity);
        }

        private static void GetMultipleBFActGenElementActions()
        {
            mListBF = [];
            BusinessFlow webBF = new BusinessFlow() { Name = "TestBFWebConversion", Active = true };
            BusinessFlow winBF = new BusinessFlow() { Name = "TestBFWinConversion", Active = true };
            BusinessFlow pbBF = new BusinessFlow() { Name = "TestBFPBConversion", Active = true };

            Activity webActivity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = "Web-App"
            };
            ActGenElement gen1 = new ActGenElement
            {
                Active = true,
                Description = "Set Value : first_name input",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath,
                LocateValue = "//input[@name='first_name']",
                GenElementAction = ActGenElement.eGenElementAction.SendKeys
            };
            webActivity.Acts.Add(gen1);

            webBF.AddActivity(webActivity);
            mListBF.Add(webBF);

            Activity winActivity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = "Web-App"
            };
            ActGenElement gen2 = new ActGenElement
            {
                Active = true,
                Description = "Set Value : last_name input",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath,
                LocateValue = "//input[@name='last_name']",
                GenElementAction = ActGenElement.eGenElementAction.SendKeys
            };
            winActivity.Acts.Add(gen2);

            winBF.AddActivity(winActivity);
            mListBF.Add(winBF);

            Activity pbActivity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = "Web-App"
            };
            ActGenElement gen3 = new ActGenElement
            {
                Active = true,
                Description = "Set Value : email input",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath,
                LocateValue = "//input[@name='email']",
                GenElementAction = ActGenElement.eGenElementAction.SendKeys
            };
            pbActivity.Acts.Add(gen3);

            pbBF.AddActivity(pbActivity);
            mListBF.Add(pbBF);
        }

        private static void GetActivityWithActUIElementActions()
        {
            mBF = new BusinessFlow() { Name = "TestBFConversion", Active = true };

            Activity activity = new Activity
            {
                SelectedForConversion = true
            };

            ActUIElement gen1 = new ActUIElement
            {
                Active = true,
                Description = "Set Value : first_name input",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByRelXPath,
                LocateValue = "//input[@name='first_name']",
                ElementType = Amdocs.Ginger.Common.UIElement.eElementType.TextBox,
                ElementAction = ActUIElement.eElementAction.SendKeys,
                ElementLocateValue = ""
            };
            activity.Acts.Add(gen1);

            mBF.AddActivity(activity);
        }

        private static void ExecuteActionConversion(bool addNewActivity, bool isDefaultTargetApp, string strTargetApp,
                                                    bool convertToPOMAction = false, Guid selectedPOM = default(Guid))
        {
            ActionConversionUtils utils = new ActionConversionUtils()
            {
                ActUIElementClassName = nameof(ActUIElement),
                ActUIElementElementLocateByField = nameof(ActUIElement.ElementLocateBy),
                ActUIElementLocateValueField = nameof(ActUIElement.ElementLocateValue),
                ActUIElementElementLocateValueField = nameof(ActUIElement.ElementLocateValue),
                ActUIElementElementTypeField = nameof(ActUIElement.ElementType)
            };
            ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(mBF.Activities.ToList());
            ObservableList<Guid> poms = [selectedPOM];
            foreach (var item in lst)
            {
                item.Selected = true;
            }


            ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications = [];
            foreach (var act in mBF.Activities)
            {
                ConvertableTargetApplicationDetails tas = new ConvertableTargetApplicationDetails
                {
                    SourceTargetApplicationName = act.TargetApplication,
                    TargetTargetApplicationName = act.TargetApplication
                };
                convertableTargetApplications.Add(tas);
            }

            BusinessFlowToConvert statusLst = new BusinessFlowToConvert()
            {
                ConversionStatus = eConversionStatus.Pending,
                BusinessFlow = mBF,
                TotalProcessingActionsCount = mBF.Activities[0].Acts.Count
            };

            utils.ConvertBusinessFlowLegacyActions(statusLst, addNewActivity, lst, convertableTargetApplications, convertToPOMAction, poms);
        }

        private static void ExecuteActionConversionForMultipleBF(bool addNewActivity, bool convertoSameTA = true,
                                                                 bool convertToPOMAction = false, Guid selectedPOM = default(Guid))
        {
            ObservableList<BusinessFlowToConvert> ListOfBusinessFlowToConvert = [];
            ActionConversionUtils utils = new ActionConversionUtils();
            ObservableList<ConvertableActionDetails> lstCad = [];
            foreach (var bf in mListBF)
            {
                ObservableList<ConvertableActionDetails> lst = utils.GetConvertableActivityActions(bf.Activities.ToList());

                foreach (ConvertableActionDetails cad in lst)
                {
                    cad.Selected = true;
                    lstCad.Add(cad);
                }

                BusinessFlowToConvert flowConversion = new BusinessFlowToConvert
                {
                    BusinessFlow = bf,
                    ConversionStatus = eConversionStatus.Pending,
                    TotalProcessingActionsCount = lst.Count
                };
                ListOfBusinessFlowToConvert.Add(flowConversion);
            }
            ObservableList<Guid> poms = [selectedPOM];

            ObservableList<ConvertableTargetApplicationDetails> convertableTargetApplications = [];
            foreach (var bf in mListBF)
            {
                foreach (var act in bf.Activities)
                {
                    ConvertableTargetApplicationDetails tas = new ConvertableTargetApplicationDetails
                    {
                        SourceTargetApplicationName = act.TargetApplication
                    };
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

            utils.ListOfBusinessFlowsToConvert = ListOfBusinessFlowToConvert;
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
                    string convertedLocateType = ((GingerCore.Actions.Common.ActUIElement)activity.Acts[1]).ElementLocateBy.ToString();
                    if (targetLocateType != convertedLocateType)
                    {
                        isValid = false;
                        break;
                    }

                    string targetLocateValue = ((GingerCore.Actions.Act)activity.Acts[0]).LocateValue.ToString();
                    string convertedLocateValue = ((GingerCore.Actions.Common.ActUIElement)activity.Acts[1]).ElementLocateValue.ToString();
                    if (targetLocateValue != convertedLocateValue)
                    {
                        isValid = false;
                        break;
                    }

                    if (!isTASame)
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
                string convertedLocateType = ((GingerCore.Actions.Common.ActUIElement)targetActivity.Acts[0]).ElementLocateBy.ToString();
                if (targetLocateType != convertedLocateType)
                {
                    isValid = false;
                    break;
                }

                string targetLocateValue = ((GingerCore.Actions.Act)sourceActivity.Acts[0]).LocateValue.ToString();
                string convertedLocateValue = ((GingerCore.Actions.Common.ActUIElement)targetActivity.Acts[0]).ElementLocateValue.ToString();
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
            string convertedLocateType = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count, 1);
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
            Assert.AreEqual(mBF.Activities[0].Acts.Count, 1);
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
            string convertedLocateType = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[1].Acts[0]).ElementLocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[1].Acts[0]).ElementLocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count, 2);
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
            Assert.AreEqual(mBF.Activities.Count, 1);
            Assert.AreEqual(mBF.Activities[0].Acts.Count, 1);
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
            string convertedLocateType = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString();

            string targetLocateValue = ((GingerCore.Actions.Act)mBF.Activities[0].Acts[0]).LocateValue.ToString();
            string convertedLocateValue = ((GingerCore.Actions.Common.ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString();

            ////Assert
            Assert.AreEqual(mBF.Activities.Count, 1);
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
            Assert.AreEqual(mBF.Activities.Count, 1);
            Assert.AreEqual(mBF.Activities[0].Acts.Count, 1);
        }

        #region Web actions convertor Test

        [TestMethod]
        [Timeout(60000)]
        public void WebActSwitchWindowToUISwitchWindow()
        {
            Activity activity = GetActivityforConversionTest("Web-App");
            ActSwitchWindowTOUISwitchWindowConvertor(activity);
        }

        #endregion
        private static Activity GetActivityforConversionTest(string targetApp)
        {
            mBF = new BusinessFlow() { Name = "TestConversion", Active = true };

            Activity activity = new Activity
            {
                Active = true,
                SelectedForConversion = true,
                TargetApplication = targetApp
            };
            return activity;
        }

        private static void ActSwitchWindowTOUISwitchWindowConvertor(Activity activity)
        {
            ActSwitchWindow actSwitchWindow = new ActSwitchWindow
            {
                Active = true,
                Description = "Switch Window",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByTitle,
                LocateValue = "FaceBook"
            };
            activity.Acts.Add(actSwitchWindow);

            mBF.AddActivity(activity);

            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), actSwitchWindow.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), actSwitchWindow.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.Switch.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementType.ToString(), eElementType.Window.ToString());
        }

        #region Window actions convertot Test

        [TestMethod]
        [Timeout(60000)]
        public void WindowActSwitchWindowToUISwitchWindow()
        {
            Activity activity = GetActivityforConversionTest("Window-App");
            ActSwitchWindowTOUISwitchWindowConvertor(activity);
        }

        #endregion

        #region java Convertor Test

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericSetValueToUIElementSetValue()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Set Value : txtEmployeeID",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "txtEmployeeID",
                Value = "12321",
                GenElementAction = ActGenElement.eGenElementAction.SetValue
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericGetValueToUIElementGetValue()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Get Value : txtEmployeeID",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "txtEmployeeID",
                GenElementAction = ActGenElement.eGenElementAction.GetValue
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaGenericSwitchFromToActBroswerSwitchFrame()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Switch Frame",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "frame1",
                GenElementAction = ActGenElement.eGenElementAction.SwitchFrame
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToBrowserActionConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActRunJavaScriptToActBroswerRunJavaScript()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Run JavaScript",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.Unknown,
                LocateValue = "",
                GenElementAction = ActGenElement.eGenElementAction.RunJavaScript,
                Value = "document.getElementById('id1')"
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToBrowserActionConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericClickToUIElementClick()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Click: Submit",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "btnSumit",
                GenElementAction = ActGenElement.eGenElementAction.Click
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaGenericScrollUpToUIElementScrollUp()
        {
            Activity activity = GetActivityforConversionTest("Java-App");
            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Scroll up",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "scroller1",
                GenElementAction = ActGenElement.eGenElementAction.ScrollUp
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaGenericScrollDownToUIElementScrollDown()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Scroll Down",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "scroller1",
                GenElementAction = ActGenElement.eGenElementAction.ScrollDown
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaGenericVisibleToUIElementIsVisible()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "is visible",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "btnSumit",
                GenElementAction = ActGenElement.eGenElementAction.Visible
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);
            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.IsVisible.ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaGenericEnableToUIElementIsEnable()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "is enabled",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "btnSumit",
                GenElementAction = ActGenElement.eGenElementAction.Enabled
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);
            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.IsEnabled.ToString());
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericAsyncClickToUIElementAsyncClick()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Click: Submit",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "btnSumit",
                GenElementAction = ActGenElement.eGenElementAction.AsyncClick
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            // Act & Assert
            JavaGenericToUIElementConversionActAndAssert(genAction);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActFireMouseEventToUIElementTriggerJavaScriptEvent()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "FireMouseEvent: onmouseout",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "selectCountry",
                GenElementAction = ActGenElement.eGenElementAction.FireMouseEvent,
                Value = "onmouseout"
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.TriggerJavaScriptEvent.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.ValueToSelect), genAction.Value);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActFireSpecialEventToUIElementTriggerJavaScriptEvent()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "FireSpecialEvent: onblur",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "selectCountry",
                GenElementAction = ActGenElement.eGenElementAction.FireSpecialEvent,
                Value = "onblur"
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.TriggerJavaScriptEvent.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.ValueToSelect), genAction.Value);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericSelectFromDropDownByIndexToUIElementSelectByIndex()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Select : Country",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "selectCountry",
                Value = "0",
                GenElementAction = ActGenElement.eGenElementAction.SelectFromDropDownByIndex
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.SelectByIndex.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.ValueToSelect), genAction.Value);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActGenericSelectFromDropDownToUIElementSelect()
        {
            Activity activity = GetActivityforConversionTest("Java-App");

            ActGenElement genAction = new ActGenElement
            {
                Active = true,
                Description = "Select : Country",
                LocateBy = Amdocs.Ginger.Common.UIElement.eLocateBy.ByName,
                LocateValue = "selectCountry",
                Value = "india",
                GenElementAction = ActGenElement.eGenElementAction.SelectFromDropDown
            };
            activity.Acts.Add(genAction);

            mBF.AddActivity(activity);

            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), ActUIElement.eElementAction.Select.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.ValueToSelect), genAction.Value);
        }

        [TestMethod]
        [Timeout(60000)]
        public void JavaActSwitchWindowToUISwitchWindow()
        {
            Activity activity = GetActivityforConversionTest("Java-App");
            ActSwitchWindowTOUISwitchWindowConvertor(activity);
        }

        [TestMethod]
        [Timeout(100000)]
        public void JavaLegacyPOMConversionTest()
        {
            //Arrange
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();
            var businessFlowFile = TestResources.GetTestResourcesFile(@"JavaLegacyToPOMxml" + Path.DirectorySeparatorChar + "Java_Legacy_Actions_BF.Ginger.BusinessFlow.xml");
            var javaPomFile = TestResources.GetTestResourcesFile(@"JavaLegacyToPOMxml\Java Swing Test App.Ginger.ApplicationPOMModel.xml");

            //Load BF
            mBF = (BusinessFlow)RepositorySerializer.DeserializeFromFile(businessFlowFile);
            mBF.Activities[0].SelectedForConversion = true;

            //Load POM
            ApplicationPOMModel applicationPOM = (ApplicationPOMModel)RepositorySerializer.DeserializeFromFile(javaPomFile);
            applicationPOM.FilePath = applicationPOM.Name;//so new file will be created for it
            mSolutionRepository.AddRepositoryItem(applicationPOM);
            ExecuteActionConversion(true, true, string.Empty, true, applicationPOM.Guid);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[1].Acts[1]).ElementLocateBy.ToString(), eLocateBy.POMElement.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[1].Acts[1]).ElementAction.ToString(), ((ActJavaElement)mBF.Activities[0].Acts[1]).ControlAction.ToString());
        }

        private static void JavaGenericToUIElementConversionActAndAssert(ActGenElement genAction)
        {
            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).GetInputParamValue(ActUIElement.Fields.IsWidgetsElement), "true");
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementLocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).ElementAction.ToString(), genAction.GenElementAction.ToString());
            Assert.AreEqual(((ActUIElement)mBF.Activities[0].Acts[1]).Value, genAction.Value);
        }


        private void JavaGenericToBrowserActionConversionActAndAssert(ActGenElement genAction)
        {
            //Act
            ExecuteActionConversion(false, true, string.Empty);

            //Assert
            Assert.AreEqual(((ActBrowserElement)mBF.Activities[0].Acts[1]).LocateBy.ToString(), genAction.LocateBy.ToString());
            Assert.AreEqual(((ActBrowserElement)mBF.Activities[0].Acts[1]).LocateValue.ToString(), genAction.LocateValue.ToString());
            Assert.AreEqual(((ActBrowserElement)mBF.Activities[0].Acts[1]).ControlAction.ToString(), genAction.GenElementAction.ToString());
            Assert.AreEqual(((ActBrowserElement)mBF.Activities[0].Acts[1]).Value, genAction.Value);
        }

        [TestMethod]
        public void GetActionsWithoutDescription()
        {
        }

        #endregion
    }
}
