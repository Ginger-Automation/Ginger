using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerTestHelper;
using GingerWPFUnitTest;
using System.IO;
using GingerTest.POMs;
using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System.Linq;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.UIElement;
using System.Windows.Media.Imaging;
using GingerCore.Drivers;

namespace GingerTest.APIModelLib
{
    [TestClass]
    [Level3]
    public class POMsTest
    {
        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static POMsPOM mPOMsPOM = null;
        static ApplicationPOMModel mLearnedPOM = null;


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
            mLearnedPOM = mPOMsPOM.CreatePOM(name, description, "MyWebApp", mChromeAgent, @"HTML\HTMLControls.html", new List<eElementType>() { eElementType.HyperLink, eElementType.Table, eElementType.ListItem });
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestMethod]
        public void ValidatePOMWasAddedToPOMsTree()
        {
            //Act
            mPOMsPOM.SelectPOM(mLearnedPOM.Name);
            ApplicationPOMModel treePOM = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == mLearnedPOM.Name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(mLearnedPOM.Name, treePOM.Name, "POM.Name is same");
        }


        [TestMethod]
        public void ValidatePOMGeneralDetails()
        {
            //Assert
            Assert.AreEqual(mLearnedPOM.Name, "MyNewPOM", "POM.Name check");
            Assert.AreEqual(mLearnedPOM.Description, "MyDescription", "POM.Description check");
            Assert.IsTrue(mLearnedPOM.PageURL.EndsWith("TestResources/HTML/HTMLControls.html"), "POM.URL check");
            Assert.AreEqual(mLearnedPOM.TargetApplicationKey.ToString(), "MyWebApp~843fb8a6-a844-45e9-b7ef-b045b6433c44", "POM.TargetApplicationKey is same");
        }


        [TestMethod]
        public void ValidatePOMScreenshotWasTaken()
        {
            //Act
            BitmapSource source = Ginger.General.GetImageStream(Ginger.General.Base64StringToImage(mLearnedPOM.ScreenShotImage.ToString()));
            //Assert  
            Assert.IsNotNull(source, "POM.ScreenShotImage converted to sourse check");
        }


        [TestMethod]
        public void ValidateLearnedItems()
        {
            //Act
            ElementInfo EI1 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "Mexico INPUT.RADIO" && x.ElementTypeEnum == eElementType.RadioButton).FirstOrDefault();
            ElementInfo EI2 = mLearnedPOM.MappedUIElements.Where(x => x.ElementName == "id123 input" && x.ElementTypeEnum == eElementType.TextBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(mLearnedPOM.MappedUIElements.Count, 27, "POM.MappedUIElements.Coun check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements.Count, 92, "POM.UnMappedUIElements.Count check");
            Assert.IsNotNull(EI1, "POM.Element learned check");
            Assert.IsNotNull(EI2, "POM.Element learned check");
        }

        [TestMethod]
        public void ValidateElementsProperties()
        {
            //Act  
            ElementInfo ButtonEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Button).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert  
            Assert.AreEqual(ButtonEI.Properties.Count, 13, "POM.properties check");
            Assert.AreEqual(ButtonEI.Properties[0].Name, "Platform Element Type", "POM.property 0 check");
            Assert.AreEqual(ButtonEI.Properties[0].Value, "INPUT.SUBMIT" , "POM.property 0 check");
            Assert.AreEqual(ButtonEI.Properties[1].Name, "Parent IFrame", "POM.property 1 check");
            Assert.AreEqual(ButtonEI.Properties[1].Value, "", "POM.property 1 check");
            Assert.AreEqual(ButtonEI.Properties[2].Name, "XPath", "POM.property 2 check");
            Assert.AreEqual(ButtonEI.Properties[2].Value, "/html[1]/body[1]/div[19]/input[1]", "POM.property 2 check");
            Assert.AreEqual(ButtonEI.Properties[3].Name, "Relative XPath", "POM.property 3 check");
            Assert.AreEqual(ButtonEI.Properties[3].Value, "//input[@id='submit1']", "POM.property 3 check");
            Assert.AreEqual(ButtonEI.Properties[4].Name, "Height", "POM.property 4 check");
            Assert.AreEqual(ButtonEI.Properties[4].Value, "21", "POM.property 4 check");
            Assert.AreEqual(ButtonEI.Properties[5].Name, "Width", "POM.property 5 check");
            Assert.AreEqual(ButtonEI.Properties[5].Value, "95", "POM.property 5 check");
            Assert.AreEqual(ButtonEI.Properties[6].Name, "X", "POM.property 6 check");
            Assert.AreEqual(ButtonEI.Properties[6].Value, "8", "POM.property 6 check");
            Assert.AreEqual(ButtonEI.Properties[7].Name, "Y", "POM.property 7 check");
            Assert.AreEqual(ButtonEI.Properties[7].Value, "744", "POM.property 7 check");
            Assert.AreEqual(ButtonEI.Properties[8].Name, "Value", "POM.property 8 check");
            Assert.AreEqual(ButtonEI.Properties[8].Value, "Submit Order", "POM.property 8 check");
            Assert.AreEqual(ButtonEI.Properties[9].Name, "id", "POM.property 9 check");
            Assert.AreEqual(ButtonEI.Properties[9].Value, "submit1", "POM.property 9 check");
            Assert.AreEqual(ButtonEI.Properties[10].Name, "name", "POM.property 10 check");
            Assert.AreEqual(ButtonEI.Properties[10].Value, "submit1", "POM.property 10 check");
            Assert.AreEqual(ButtonEI.Properties[11].Name, "type", "POM.property 11 check");
            Assert.AreEqual(ButtonEI.Properties[11].Value, "submit", "POM.property 11 check");
            Assert.AreEqual(ButtonEI.Properties[12].Name, "value", "POM.property 12 check");
            Assert.AreEqual(ButtonEI.Properties[12].Value, "Submit Order", "POM.property 12 check");

            Assert.AreEqual(ComboBoxEI.Properties.Count, 13, "POM.properties check");
            Assert.AreEqual(ComboBoxEI.Properties[0].Name, "Platform Element Type", "POM.property 0 check");
            Assert.AreEqual(ComboBoxEI.Properties[0].Value, "SELECT", "POM.property 0 check");
            Assert.AreEqual(ComboBoxEI.Properties[1].Name, "Parent IFrame", "POM.property 1 check");
            Assert.AreEqual(ComboBoxEI.Properties[1].Value, "", "POM.property 1 check");
            Assert.AreEqual(ComboBoxEI.Properties[2].Name, "XPath", "POM.property 2 check");
            Assert.AreEqual(ComboBoxEI.Properties[2].Value, "/html[1]/body[1]/div[9]/select[1]", "POM.property 2 check");
            Assert.AreEqual(ComboBoxEI.Properties[3].Name, "Relative XPath", "POM.property 3 check");
            Assert.AreEqual(ComboBoxEI.Properties[3].Value, "//select[@id='sel1']", "POM.property 3 check");
            Assert.AreEqual(ComboBoxEI.Properties[4].Name, "Height", "POM.property 4 check");
            Assert.AreEqual(ComboBoxEI.Properties[4].Value, "19", "POM.property 4 check");
            Assert.AreEqual(ComboBoxEI.Properties[5].Name, "Width", "POM.property 5 check");
            Assert.AreEqual(ComboBoxEI.Properties[5].Value, "74", "POM.property 5 check");
            Assert.AreEqual(ComboBoxEI.Properties[6].Name, "X", "POM.property 6 check");
            Assert.AreEqual(ComboBoxEI.Properties[6].Value, "631", "POM.property 6 check");
            Assert.AreEqual(ComboBoxEI.Properties[7].Name, "Y", "POM.property 7 check");
            Assert.AreEqual(ComboBoxEI.Properties[7].Value, "226", "POM.property 7 check");
            Assert.AreEqual(ComboBoxEI.Properties[8].Name, "Value", "POM.property 8 check");
            Assert.AreEqual(ComboBoxEI.Properties[8].Value, "set to ", "POM.property 8 check");
            Assert.AreEqual(ComboBoxEI.Properties[9].Name, "Optional Values", "POM.property 9 check");
            Assert.AreEqual(ComboBoxEI.Properties[9].Value, ",Ahhhh...,Got It!,Too far,OMG,", "POM.property 9 check");
            Assert.AreEqual(ComboBoxEI.Properties[10].Name, "id", "POM.property 9 check");
            Assert.AreEqual(ComboBoxEI.Properties[10].Value, "sel1", "POM.property 9 check");
            Assert.AreEqual(ComboBoxEI.Properties[11].Name, "name", "POM.property 10 check");
            Assert.AreEqual(ComboBoxEI.Properties[11].Value, "sel1", "POM.property 10 check");
            Assert.AreEqual(ComboBoxEI.Properties[12].Name, "onchange", "POM.property 11 check");
            Assert.AreEqual(ComboBoxEI.Properties[12].Value, "if ($('#sel1').val() == 'Got It!') $('#test9').addClass('TestPass');", "POM.property 11 check");
        }


        [TestMethod]
        public void ValidateElementsLocators()
        {
            //Act
            ElementInfo ButtonEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.Button).FirstOrDefault();
            ElementInfo ComboBoxEI = mLearnedPOM.MappedUIElements.Where(x => x.ElementTypeEnum == eElementType.ComboBox).FirstOrDefault();

            //Assert
            Assert.AreEqual(ButtonEI.Locators.Count, 4, "POM.Locators check");
            Assert.AreEqual(ButtonEI.Locators[0].LocateBy, eLocateBy.ByID, "POM.Locator 0 .LocateBy check");
            Assert.AreEqual(ButtonEI.Locators[0].LocateValue, "submit1", "POM.Locator 0 . LocateValue check");
            Assert.AreEqual(ButtonEI.Locators[1].LocateBy, eLocateBy.ByName, "POM.Locator 1 .LocateBy check");
            Assert.AreEqual(ButtonEI.Locators[1].LocateValue, "submit1", "POM.Locator 1 . LocateValue check");
            Assert.AreEqual(ButtonEI.Locators[2].LocateBy, eLocateBy.ByRelXPath, "POM.Locator 2 .LocateBy check");
            Assert.AreEqual(ButtonEI.Locators[2].LocateValue, "//input[@id='submit1']", "POM.Locator 2 . LocateValue check");
            Assert.AreEqual(ButtonEI.Locators[3].LocateBy, eLocateBy.ByXPath, "POM.Locator 3 .LocateBy check");
            Assert.AreEqual(ButtonEI.Locators[3].LocateValue, "/html[1]/body[1]/div[19]/input[1]", "POM.Locator 3 . LocateValue check");

            Assert.AreEqual(ComboBoxEI.Locators.Count, 4, "POM.Locators check");
            Assert.AreEqual(ComboBoxEI.Locators[0].LocateBy, eLocateBy.ByID, "POM.Locator 0 .LocateBy check");
            Assert.AreEqual(ComboBoxEI.Locators[0].LocateValue, "sel1", "POM.Locator 0 . LocateValue check");
            Assert.AreEqual(ComboBoxEI.Locators[1].LocateBy, eLocateBy.ByName, "POM.Locator 1 .LocateBy check");
            Assert.AreEqual(ComboBoxEI.Locators[1].LocateValue, "sel1", "POM.Locator 1 . LocateValue check");
            Assert.AreEqual(ComboBoxEI.Locators[2].LocateBy, eLocateBy.ByRelXPath, "POM.Locator 2 .LocateBy check");
            Assert.AreEqual(ComboBoxEI.Locators[2].LocateValue, "//select[@id='sel1']", "POM.Locator 2 . LocateValue check");
            Assert.AreEqual(ComboBoxEI.Locators[3].LocateBy, eLocateBy.ByXPath, "POM.Locator 3 .LocateBy check");
            Assert.AreEqual(ComboBoxEI.Locators[3].LocateValue, "/html[1]/body[1]/div[9]/select[1]", "POM.Locator 3 . LocateValue check");
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
