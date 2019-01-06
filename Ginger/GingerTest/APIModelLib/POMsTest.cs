using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerTestHelper;
using GingerWPFUnitTest;
using System.IO;
using GingerWPFUnitTest.GeneralLib;
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

        static TestContext mTC;
        static string SolutionFolder;
        static GingerAutomator mGingerAutomator;

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            //Arrange  
            mTC = TC;
            string name = "MyNewPOM";
            string description = "MyDescription";

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\POMsTest");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\POMsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);
            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(SolutionFolder);
            mPOMsPOM = mGingerAutomator.MainWindowPOM.GotoPOMs();

            mChromeAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "ChromeAgent" select x).SingleOrDefault();
            //Act
            mLearnedPOM = mPOMsPOM.CreatePOM(name, description, "MyWebApp", mChromeAgent, @"HTML\HTMLControls.html", new List<eElementType>() { eElementType.Button, eElementType.Canvas, eElementType.Label });
        }

        static Agent mChromeAgent = null;
        static POMsPOM mPOMsPOM = null;
        static ApplicationPOMModel mLearnedPOM = null;

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }

        [TestInitialize]
        public void TestInitialize()
        {}

        [TestCleanup]
        public void TestCleanUp()
        {}

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
            Assert.AreEqual(mLearnedPOM.MappedUIElements.Count, 42, "POM.MappedUIElements.Coun check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements.Count, 77, "POM.UnMappedUIElements.Count check");
            Assert.IsNotNull(EI1, "POM.Element learned check");
            Assert.IsNotNull(EI2, "POM.Element learned check");
        }

        [TestMethod]
        public void ValidateElementsProperties()
        {
            //Assert  
            Assert.AreEqual(mLearnedPOM.MappedUIElements[0].Properties.Count, 8, "POM.properties check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements[1].Properties.Count, 7, "POM.properties check");
        }


        [TestMethod]
        public void ValidateElementsLocators()
        {
            //Assert  
            Assert.AreEqual(mLearnedPOM.MappedUIElements[3].Locators.Count, 2, "POM.Locators check");
            Assert.AreEqual(mLearnedPOM.UnMappedUIElements[4].Locators.Count, 3, "POM.Locators check");
        }

        public POMsTest()
        {}

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
