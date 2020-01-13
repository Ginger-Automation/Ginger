using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.Drivers.CommunicationProtocol;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GingerTest.POMs
{
    [TestClass]
    [Level3]
    public class JavaPOMsTest
    {
        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static POMsPOM mPOMsPOM = null;
        static ApplicationPOMModel mLearnedPOM = null;
        static List<ElementLocator> prioritizedLocatorsList = null;

        static BusinessFlow mBF;

        // make it static for reuse so no need to init every time when running test by click test button
        static JavaDriver mDriver = null;
        static GingerRunner mGR = null;
        static Agent mJavaAgent = null;
        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            //Arrange  
            string name = "MyNewJavaPOM";
            string description = "This is JAVA POM";

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\POMsTest");
            string SolutionFolder = TestResources.GetTestTempFolder(@"Solutions\POMsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);
            GingerAutomator mGingerAutomator = GingerAutomator.StartSession();

            mGingerAutomator.OpenSolution(SolutionFolder);

            mPOMsPOM = mGingerAutomator.MainWindowPOM.GotoPOMs();

            mJavaAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "JavaAgent" select x).SingleOrDefault();
            //Act
            prioritizedLocatorsList = new List<ElementLocator>()
            {
                new ElementLocator() { Active = false, LocateBy = eLocateBy.ByName },                
                new ElementLocator() { Active = false, LocateBy = eLocateBy.ByXPath }                
            };
            StartAgent();
            mLearnedPOM = mPOMsPOM.CreatePOM(name, description, "MyJavaApp", mJavaAgent, "", new List<eElementType>() { eElementType.HyperLink, eElementType.Table, eElementType.ListItem }, prioritizedLocatorsList);
            
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            ActWindow AWC = new ActWindow();
            AWC.LocateBy = eLocateBy.ByTitle;
            AWC.LocateValue = "Java";
            AWC.WindowActionType = ActWindow.eWindowActionType.Close;
            mGR.RunAction(AWC, false);
            mGR.StopAgents();            
            mDriver = null;
            mGR = null;
            GingerAutomator.EndSession();
        }

        [TestMethod]
        [Timeout(60000)]
        public void ValidateJavaPOMWasAddedToPOMsTree()
        {
            //Act
            mPOMsPOM.SelectPOM(mLearnedPOM.Name);
            ApplicationPOMModel treePOM = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == mLearnedPOM.Name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(mLearnedPOM.Name, treePOM.Name, "POM.Name is same");
        }


        [TestMethod]
        [Timeout(60000)]
        public void ValidateJavaPOMGeneralDetails()
        {
            //Assert
            Assert.AreEqual(mLearnedPOM.Name, "MyNewJavaPOM", "POM.Name check");
            Assert.AreEqual(mLearnedPOM.Description, "This is JAVA POM", "POM.Description check");
            Assert.AreEqual(mLearnedPOM.PageURL, "Java Swing Test App", "POM.Title check");
            Assert.AreEqual(mLearnedPOM.TargetApplicationKey.ToString(), "MyJavaApp~ac680118-d675-4135-b84c-86661df9aab9", "POM.TargetApplicationKey is same");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ValidatePOMScreenshotWasTaken()
        {
            //Act
            BitmapSource source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mLearnedPOM.ScreenShotImage.ToString()));
            //Assert  
            Assert.IsNotNull(source, "POM.ScreenShotImage converted to sourse check");
        }


        [TestMethod]
        [Timeout(60000)]
        public void ValidateLearnedItems()
        {
            //Act
            ElementInfo EI1 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "lblCountry" && x.ElementTypeEnum == eElementType.Label).FirstOrDefault();
            ElementInfo EI2 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "Country" && x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(mLearnedPOM.MappedUIElements.Count, 28, "POM.MappedUIElements.Count check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements.Count, 25, "POM.UnMappedUIElements.Count check");
            Assert.IsNotNull(EI1, "POM.Element learned check");
            Assert.IsNotNull(EI2, "POM.Element learned check");
        }

        [TestMethod]
        //[Timeout(60000)]
        public void ValidateElementsProperties()
        {
            //Act  
            ElementInfo LabelEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Label).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(LabelEI.Properties.Count, 8, "POM.properties check");           
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Name", "lblCountry"), "POM.property 1 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Value", "Country"), "POM.property 2 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Width", "44"), "POM.property 3 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Height", "16"), "POM.property 4 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "X Coordinate", "174"), "POM.property 5 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Y Coordinate", "167"), "POM.property 6 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Class", "javax.swing.JLabel"), "POM.property 7 check");
            Assert.IsTrue(IsPropertyExist(LabelEI.Properties, "Swing Class", "javax.swing.JLabel"), "POM.property 8 check");
            

            Assert.AreEqual(ComboBoxEI.Properties.Count, 8, "POM.properties check");            
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Name", "Country"), "POM.property 1 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Value", "India"), "POM.property 2 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Width", "64"), "POM.property 3 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Height", "25"), "POM.property 4 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "X Coordinate", "238"), "POM.property 5 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Y Coordinate", "163"), "POM.property 6 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Class", "javax.swing.JComboBox"), "POM.property 7 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Swing Class", "javax.swing.JComboBox"), "POM.property 8 check");
        }

        private bool IsPropertyExist(ObservableList<ControlProperty> Properties, string PropName, string PropValue)
        {
            ControlProperty property = Properties.Where(x => x.Name == PropName && x.Value == PropValue).FirstOrDefault();

            if (property != null)
            {
                return true;

            }
            else
            {
                return false;
            }
        }

        private bool IsLocatorExist(ObservableList<ElementLocator> locators, eLocateBy locateBy, string locateValue)
        {
            ElementLocator locator = locators.Where(x => x.LocateBy == locateBy && x.LocateValue == locateValue).FirstOrDefault();

            if (locator != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool CheckLocatorPriority(ObservableList<ElementLocator> locators, eLocateBy locateBy, string locateValue, bool isActive, int priorityIndexValue)
        {
            ElementLocator locator = locators.Where(x => x.LocateBy == locateBy && x.LocateValue == locateValue).FirstOrDefault();

            if (locator != null && locator.Active == isActive && locators.IndexOf(locator) == priorityIndexValue)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [TestMethod]
        [Timeout(60000)]
        public void ValidateElementsLocators()
        {
            //Act
            ElementInfo LabelEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Label).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert
            Assert.AreEqual(LabelEI.Locators.Count, 2, "POM.Locators check");            
            Assert.IsTrue(IsLocatorExist(LabelEI.Locators, eLocateBy.ByName, "lblCountry"), "POM.Locator 1 .LocateBy check");            
            Assert.IsTrue(IsLocatorExist(LabelEI.Locators, eLocateBy.ByXPath, "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 1/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/JPanel1/[[Name:lblCountry][ClassName:javax.swing.JLabel]]"), "POM.Locator 2 .LocateBy check");

            Assert.AreEqual(ComboBoxEI.Locators.Count, 2, "POM.Locators check");            
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByName, "Country"), "POM.Locator 1 .LocateBy check");            
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByXPath, "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 1/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/JPanel1/[[Name:Country][ClassName:javax.swing.JComboBox]]"), "POM.Locator 2 .LocateBy check");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ValidateLocatorsPriority()
        {
            //Act
            ElementInfo LabelEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Label).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            #region Assert #1 Element : ButtonEI | POM.LocatorsPriority Check
            Assert.AreEqual(LabelEI.Locators.Count, 2, "POM.LocatorsPriority check");

            ElementLocator elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByName);
            int locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(LabelEI.Locators, eLocateBy.ByName, "lblCountry", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");            

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(LabelEI.Locators, eLocateBy.ByXPath, "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 1/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/JPanel1/[[Name:lblCountry][ClassName:javax.swing.JLabel]]", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");
            #endregion

            # region Assert #2 Element : ComboBoxEI | POM.LocatorsPriority Check
            Assert.AreEqual(ComboBoxEI.Locators.Count, 2, "POM.LocatorsPriority check");            

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByName);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByName, "Country", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");            

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByXPath, "/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/Internal Frame 1/javax.swing.JRootPane[0]/null.layeredPane/null.contentPane/JPanel1/[[Name:Country][ClassName:javax.swing.JComboBox]]", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");
            #endregion
        }

        [TestMethod]        
        public void TestAllMappedElements()
        {
            //Arrange
            int TotalElements = mLearnedPOM.MappedUIElements.Count;
            int TotalFails = 0;
            
            foreach (ElementInfo EI in mLearnedPOM.MappedUIElements)
            {
                EI.ElementStatus = ElementInfo.eElementStatus.Pending;
                foreach(ElementLocator elementLocators in EI.Locators)
                {
                    elementLocators.Active = true;
                }
            }

            //Act
            foreach (ElementInfo EI in mLearnedPOM.MappedUIElements)
            {
                if (((IWindowExplorer)mJavaAgent.Driver).TestElementLocators(EI, true))
                {
                    EI.ElementStatus = ElementInfo.eElementStatus.Passed;
                }
                else
                {
                    TotalFails++;
                    EI.ElementStatus = ElementInfo.eElementStatus.Failed;
                }                
            }

            //Assert
            Assert.AreEqual(TotalFails, 0, "POM.TestAllMappedElements check");
        }


        [TestMethod]
        public void ValidateOptionalValues()
        {
            //Act
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();           

            ElementInfo JListEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.List).FirstOrDefault();

            //Assert
            Assert.IsTrue(ComboBoxEI.OptionalValuesObjectsList.Count > 0, "POM.PossibleValues check");
            Assert.IsTrue(JListEI.OptionalValuesObjectsList.Count > 0, "POM.PossibleValues check");
        }

        [TestMethod]
        public void TestPOMSetValue()
        {
            //Arrange
            ElementInfo TextBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.TextBox).FirstOrDefault();

            ActUIElement actUIElement = new ActUIElement();
            actUIElement.ElementLocateBy = eLocateBy.POMElement;
            actUIElement.ElementType = eElementType.TextBox;
            actUIElement.ElementAction = ActUIElement.eElementAction.SetValue;
            actUIElement.ElementLocateValue = mLearnedPOM.Guid + "_" + TextBoxEI.Guid;
            actUIElement.Value = "This is Text Area";
            actUIElement.Active = true;

            //Act
            mGR.RunAction(actUIElement);

            //Assert
            Assert.AreEqual(actUIElement.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        private static void StartAgent()
        {
            if (mGR == null)
            {

                mGR = new GingerRunner();
                mGR.CurrentSolution = new Ginger.SolutionGeneral.Solution();
                mBF = new BusinessFlow();
                mBF.Activities = new ObservableList<Activity>();
                mBF.Name = "BF Test Java Driver";
                Platform p = new Platform();
                p.PlatformType = ePlatformType.Java;
                mBF.TargetApplications.Add(new TargetApplication() { AppName = "JavaTestApp" });
                Activity activity = new Activity();
                activity.TargetApplication = "JavaTestApp";
                mBF.Activities.Add(activity);
                mBF.CurrentActivity = activity;

                ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
                LJA.LaunchJavaApplication = true;
                LJA.LaunchWithAgent = true;
                LJA.WaitForWindowTitle = "Java Swing";
                LJA.AddOrUpdateInputParamValue(ActLaunchJavaWSApplication.Fields.PortConfigParam, ActLaunchJavaWSApplication.ePortConfigType.Manual.ToString());
                LJA.Port = "9898";
                LJA.URL = TestResources.GetTestResourcesFile(@"JavaTestApp\JavaTestApp.jar");
                activity.Acts.Add(LJA);
                mGR.PrepActionValueExpression(LJA);
                LJA.Execute();
                // TODO: add wait till action done and check status
                //if (!string.IsNullOrEmpty(LJA.Error))
                //{
                //   throw new Exception(LJA.Error);
                //}

                mDriver = new JavaDriver(mBF);
                mDriver.JavaAgentHost = "127.0.0.1";
                mDriver.JavaAgentPort = 9898;
                mDriver.CommandTimeout = 120;
                mDriver.cancelAgentLoading = false;
                mDriver.DriverLoadWaitingTime = 30;
                mDriver.ImplicitWait = 30;
                
                //Agent a = new Agent();
                //a.Active = true;
                //a.DriverType = Agent.eDriverType.JavaDriver;

                //a.Name = "Java Agent";
                //a.Driver = mDriver;
                mJavaAgent.Driver = mDriver;
                mDriver.StartDriver();
                mGR.SolutionAgents = new ObservableList<Agent>();
                mGR.SolutionAgents.Add(mJavaAgent);

                ApplicationAgent AA = new ApplicationAgent();
                AA.AppName = "JavaTestApp";
                AA.Agent = mJavaAgent;
                mBF.CurrentActivity.CurrentAgent = mJavaAgent;
                mGR.ApplicationAgents.Add(AA);
                mGR.CurrentBusinessFlow = mBF;
                //mJavaAgent.StartDriver();
                mGR.SetCurrentActivityAgent();                
                //PayLoad PL = new PayLoad("SwitchWindow");
                //PL.AddValue("Java Swing Test App");
                //PL.ClosePackage();
                //PayLoad RC = mDriver.Send(PL);
                //if (RC.IsErrorPayLoad())
                //{
                //    throw new Exception("Error cannot start Java driver - " + RC.GetErrorValue());
                //}
            }
        }
    }
}
