using System;
using System.Text;
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
            mTC = TC;

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\POMsTest");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\POMsTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(SolutionFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
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
        public void AddPOMUsingWizard()
        {
            //Arrange  
            string name = "MyNewPOM";
            POMsPOM pOMsPOM = mGingerAutomator.MainWindowPOM.GotoPOMs();


            Agent ChromeAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "ChromeAgent" select x).SingleOrDefault();
            //Act
            pOMsPOM.CreatePOM(name, "MyWebApp", ChromeAgent, @"HTML\HTMLControls.html",new List<eElementType>() {eElementType.Button,eElementType.Canvas,eElementType.Label });
            pOMsPOM.SelectPOM(name);
            ApplicationPOMModel pom = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(name, pom.Name, "POM.Name is same");
        }


        [TestMethod]
        public void POMValidateElementsLearnigTest()
        {
            //Arrange  
            string name = "MyNewPOM";
            POMsPOM pOMsPOM = mGingerAutomator.MainWindowPOM.GotoPOMs();


            Agent ChromeAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "ChromeAgent" select x).SingleOrDefault();
            //Act
            pOMsPOM.CreatePOM(name, "MyWebApp", ChromeAgent, @"HTML\HTMLControls.html", new List<eElementType>() { eElementType.Button, eElementType.Canvas, eElementType.Label });
            pOMsPOM.SelectPOM(name);
            ApplicationPOMModel pom = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(pom.MappedUIElements.Count, "42", "POM.Name is same");
            Assert.AreEqual(pom.UnMappedUIElements.Count, "77", "POM.Name is same");
        }

        public void POMValidaetPropertiesLearnigTest()
        {
            //Arrange  
            string name = "MyNewPOM";
            POMsPOM pOMsPOM = mGingerAutomator.MainWindowPOM.GotoPOMs();


            Agent ChromeAgent = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == "ChromeAgent" select x).SingleOrDefault();
            //Act
            pOMsPOM.CreatePOM(name, "MyWebApp", ChromeAgent, @"HTML\HTMLControls.html", new List<eElementType>() { eElementType.Button, eElementType.Canvas, eElementType.Label });
            pOMsPOM.SelectPOM(name);
            ApplicationPOMModel pom = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(pom.MappedUIElements.Count, "42", "POM.Name is same");
            Assert.AreEqual(pom.UnMappedUIElements.Count, "77", "POM.Name is same");
        }



        public POMsTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestMethod1()
        {
            //
            // TODO: Add test logic here
            //
        }
    }
}
