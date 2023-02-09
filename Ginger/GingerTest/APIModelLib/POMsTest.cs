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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerTest.POMs;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace GingerTest.APIModelLib
{
    [Ignore] // Temp because fail on Azure
    [TestClass]
    [Level3]
    public class POMsTest
    {
        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static POMsPOM mPOMsPOM = null;
        static ApplicationPOMModel mLearnedPOM = null;
        static List<ElementLocator> prioritizedLocatorsList = null;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            //Arrange  
            string name = "MyNewPOM";
            string description = "MyDescription";

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

            Agent mChromeAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "ChromeAgent" select x).SingleOrDefault();
            //Act
            prioritizedLocatorsList = new List<ElementLocator>()
            {
                new ElementLocator() { Active = false, LocateBy = eLocateBy.ByName },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByID },
                new ElementLocator() { Active = false, LocateBy = eLocateBy.ByXPath },
                new ElementLocator() { Active = true, LocateBy = eLocateBy.ByRelXPath }
            };
            mLearnedPOM = mPOMsPOM.CreatePOM(name, description, "MyWebApp", mChromeAgent, @"HTML\HTMLControls.html", new List<eElementType>() { eElementType.HyperLink, eElementType.Table, eElementType.ListItem }, prioritizedLocatorsList);
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestMethod]  [Timeout(60000)]
        public void ValidatePOMWasAddedToPOMsTree()
        {
            //Act
            mPOMsPOM.SelectPOM(mLearnedPOM.Name);
            ApplicationPOMModel treePOM = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == mLearnedPOM.Name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(mLearnedPOM.Name, treePOM.Name, "POM.Name is same");
        }


        [TestMethod]
        [Timeout(60000)]
        public void ValidatePOMGeneralDetails()
        {
            //Assert
            Assert.AreEqual(mLearnedPOM.Name, "MyNewPOM", "POM.Name check");
            Assert.AreEqual(mLearnedPOM.Description, "MyDescription", "POM.Description check");
            Assert.IsTrue(mLearnedPOM.PageURL.EndsWith("TestResources/HTML/HTMLControls.html"), "POM.URL check");
            Assert.AreEqual(mLearnedPOM.TargetApplicationKey.ToString(), "MyWebApp~843fb8a6-a844-45e9-b7ef-b045b6433c44", "POM.TargetApplicationKey is same");
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
            ElementInfo EI1 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "input radio country Mexico" && x.ElementTypeEnum == eElementType.RadioButton).FirstOrDefault();
            ElementInfo EI2 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "input text id123" && x.ElementTypeEnum == eElementType.TextBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(mLearnedPOM.MappedUIElements.Count, 25, "POM.MappedUIElements.Count check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements.Count, 94, "POM.UnMappedUIElements.Count check");
            Assert.IsNotNull(EI1, "POM.Element learned check");
            Assert.IsNotNull(EI2, "POM.Element learned check");
        }

        [TestMethod]
        //[Timeout(60000)]
        public void ValidateElementsProperties()
        {
            //Act  
            ElementInfo ButtonEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Button).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(ButtonEI.Properties.Count, 14, "POM.properties check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties, "Platform Element Type", "input"),"POM.property 0 check");
            //Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"Parent IFrame", ""), "POM.property 1 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"XPath", "/html[1]/body[1]/div[1]/input[1]"), "POM.property 2 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"Relative XPath", "//input[@id='button1']"), "POM.property 3 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"Height", "21"), "POM.property 4 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"Width", "64"), "POM.property 5 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"X", "346"), "POM.property 6 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"Y", "67"), "POM.property 7 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties,"id", "button1"), "POM.property 9 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties, "name", "btnnnnn1"), "POM.property 10 check");
            Assert.IsTrue(IsPropertyExist(ButtonEI.Properties, "type", "button"), "POM.property 11 check");

            Assert.AreEqual(ComboBoxEI.Properties.Count, 12, "POM.properties check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "Platform Element Type", "select"), "POM.property 0 check");
            //Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Parent IFrame", ""), "POM.property 1 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"XPath", "/html[1]/body[1]/div[9]/select[1]"), "POM.property 2 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Relative XPath", "//select[@id='sel1']"), "POM.property 3 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Height", "19"), "POM.property 4 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Width", "74"), "POM.property 5 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"X", "631"), "POM.property 6 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Y", "226"), "POM.property 7 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties,"Optional Values", "Ahhhh...,Got It!,Too far,OMG"), "POM.property 9 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "id", "sel1"), "POM.property 9 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "name", "sel1"), "POM.property 10 check");
            Assert.IsTrue(IsPropertyExist(ComboBoxEI.Properties, "onchange", "if ($('#sel1').val() == 'Got It!') $('#test9').addClass('TestPass');"), "POM.property 11 check");
            Assert.IsTrue(ComboBoxEI.OptionalValuesObjectsList[0].ItemName == "Ahhhh...");
            Assert.IsTrue(ComboBoxEI.OptionalValuesObjectsList[1].ItemName == "Got It!");
            Assert.IsTrue(ComboBoxEI.OptionalValuesObjectsList.Count == 4);
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

        [TestMethod]  [Timeout(60000)]
        public void ValidateElementsLocators()
        {
            //Act
            ElementInfo ButtonEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Button).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert
            Assert.AreEqual(ButtonEI.Locators.Count, 4, "POM.Locators check");
            Assert.IsTrue(IsLocatorExist(ButtonEI.Locators,eLocateBy.ByID, "button1"), "POM.Locator 0 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ButtonEI.Locators, eLocateBy.ByName, "btnnnnn1"), "POM.Locator 1 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ButtonEI.Locators, eLocateBy.ByRelXPath, "//input[@id='button1']"), "POM.Locator 2 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ButtonEI.Locators, eLocateBy.ByXPath, "/html[1]/body[1]/div[1]/input[1]"), "POM.Locator 3 .LocateBy check");

            Assert.AreEqual(ComboBoxEI.Locators.Count, 4, "POM.Locators check");
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByID, "sel1"), "POM.Locator 0 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByName, "sel1"), "POM.Locator 1 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByRelXPath, "//select[@id='sel1']"), "POM.Locator 2 .LocateBy check");
            Assert.IsTrue(IsLocatorExist(ComboBoxEI.Locators, eLocateBy.ByXPath, "/html[1]/body[1]/div[9]/select[1]"), "POM.Locator 3 .LocateBy check");
        }

        [TestMethod]
        [Timeout(60000)]
        public void ValidateLocatorsPriority()
        {
            //Act
            ElementInfo ButtonEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Button).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            #region Assert #1 Element : ButtonEI | POM.LocatorsPriority Check
            Assert.AreEqual(ButtonEI.Locators.Count, 4, "POM.LocatorsPriority check");

            ElementLocator elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByID);
            int locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ButtonEI.Locators, eLocateBy.ByID, "button1", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByName);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ButtonEI.Locators, eLocateBy.ByName, "btnnnnn1", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByRelXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ButtonEI.Locators, eLocateBy.ByRelXPath, "//input[@id='button1']", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ButtonEI.Locators, eLocateBy.ByXPath, "/html[1]/body[1]/div[1]/input[1]", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");
            #endregion

            # region Assert #2 Element : ComboBoxEI | POM.LocatorsPriority Check
            Assert.AreEqual(ComboBoxEI.Locators.Count, 4, "POM.LocatorsPriority check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByID);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByID, "sel1", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByName);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByName, "sel1", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByRelXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByRelXPath, "//select[@id='sel1']", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");

            elemLoc = prioritizedLocatorsList.Find(x => x.LocateBy == eLocateBy.ByXPath);
            locatorIndex = prioritizedLocatorsList.IndexOf(elemLoc);
            Assert.IsTrue(CheckLocatorPriority(ComboBoxEI.Locators, eLocateBy.ByXPath, "/html[1]/body[1]/div[9]/select[1]", elemLoc.Active, locatorIndex), "POM.LocatorPriority " + elemLoc.LocateBy.ToString() + " not indexed at '" + locatorIndex + "' check");
            #endregion
        }

        private TestContext testContextInstance;

        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
    }
}
