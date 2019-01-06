using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore;
using GingerCore.Drivers;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using GingerCore.Actions;
using System.Threading.Tasks;
using GingerTestHelper;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerWPF.WizardLib;
using System.Linq;

namespace UnitTests.NonUITests.AutoPilot
{
    /// <summary>
    /// Summary description for POMLearning
    /// </summary>
    [TestClass]
    public class POMLearningTests
    {
        static ObservableList<ElementInfo> mElementsList;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            //Arrange
            mElementsList = new ObservableList<ElementInfo>();
            SeleniumDriver seleniumDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
            seleniumDriver.StartDriver();
            string html = TestResources.GetTestResourcesFile(@"HTML\SCMCusotmersIndex.HTML");
            seleniumDriver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = html });

            ((IWindowExplorer)seleniumDriver).GetVisibleControls(null, mElementsList, true);
        }



        [TestMethod]
        public void POMLearnAndStopTest()
        {
            //Arrange
            ObservableList<ElementInfo> elementsList = new ObservableList<ElementInfo>();
            SeleniumDriver seleniumDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
            seleniumDriver.StartDriver();
            string html = TestResources.GetTestResourcesFile(@"HTML\SCMCusotmersIndex.HTML");
            seleniumDriver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = html });
            //Act
            Learn(seleniumDriver, elementsList);
            while (elementsList.Count < 30)
            {
                //Let it learn
            }
           seleniumDriver.mStopProcess = true;

            while (seleniumDriver.IsDriverBusy)
            {
                //Let it stop
            }

            //Assert
            Assert.AreEqual(elementsList.Count, 30, "Is Elements Empty List");
        }

        private async void Learn(SeleniumDriver seleniumDriver, ObservableList<ElementInfo> mElementsList)
        {
            await Task.Run(() => ((IWindowExplorer)seleniumDriver).GetVisibleControls(null, mElementsList, true));
        }

        [TestMethod]
        public void POMLearnLocatorsAndPropertiesTest()
        {
            Assert.AreEqual(mElementsList.Count, 30, "Is Elements Empty List");
            Assert.AreEqual(mElementsList[11].Locators.Count, 1, "Is Locators Empty  List");
            Assert.AreEqual(mElementsList[11].Locators[0].LocateBy, eLocateBy.ByXPath, "Is Locator is the same");
            Assert.AreEqual(mElementsList[11].Locators[0].LocateValue, "html/body[1]/header[1]/div[1]/div[2]/section[1]/form[1]/a[1]", "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Locators[0].LocateBy, eLocateBy.ByXPath, "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Locators[0].LocateValue, "html/body[1]/header[1]/div[1]/div[2]/nav[1]", "Is Locator is the same");
            Assert.AreEqual(mElementsList[13].Locators[0].LocateBy, eLocateBy.ByID, "Is Locator is the same");
            Assert.AreEqual(mElementsList[13].Locators[0].LocateValue, "menu", "Is Locator is the same");
            Assert.AreEqual(mElementsList[11].Properties[0].Name, "Platform Element Type", "Is Property is the same");
            Assert.AreEqual(mElementsList[11].Properties[0].Value, "LINK", "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Properties[6].Name, "Value", "Is Property is the same");
            Assert.AreEqual(mElementsList[12].Properties[6].Value, "Home\r\nAdmin\r\nAbout\r\nContact", "Is Property is the same");
            Assert.AreEqual(mElementsList[13].Properties[7].Name, "Optional Values", "Is Property is the same");
            Assert.AreEqual(mElementsList[13].Properties[7].Value, "Home,Admin,About,Contact,", "Is Property is the same");
        }

        [TestMethod]
        public void POMLearnWithFilterTest()
        {
            //Arrange

            ApplicationPOMModel POM = new ApplicationPOMModel();
            POM.MappedUIElements = mElementsList;
            ActUIElement ActUI = new ActUIElement();
            ActUI.LocateBy = eLocateBy.POMElement;
            

            foreach (ElementInfo EI in mElementsList)
            {
                ActUI.LocateValueCalculated = POM.Guid.ToString() + "_" + EI.Guid.ToString();
            }


            Assert.AreEqual(mElementsList.Count, 30, "Is Elements Empty List");

            Assert.AreEqual(mElementsList[11].Locators.Count, 1, "Is Locators Empty  List");
            Assert.AreEqual(mElementsList[11].Locators[0].LocateBy, eLocateBy.ByXPath, "Is Locator is the same");
            Assert.AreEqual(mElementsList[11].Locators[0].LocateValue, "html/body[1]/header[1]/div[1]/div[2]/section[1]/form[1]/a[1]", "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Locators[0].LocateBy, eLocateBy.ByXPath, "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Locators[0].LocateValue, "html/body[1]/header[1]/div[1]/div[2]/nav[1]", "Is Locator is the same");
            Assert.AreEqual(mElementsList[13].Locators[0].LocateBy, eLocateBy.ByID, "Is Locator is the same");
            Assert.AreEqual(mElementsList[13].Locators[0].LocateValue, "menu", "Is Locator is the same");
            Assert.AreEqual(mElementsList[11].Properties[0].Name, "Platform Element Type", "Is Property is the same");
            Assert.AreEqual(mElementsList[11].Properties[0].Value, "LINK", "Is Locator is the same");
            Assert.AreEqual(mElementsList[12].Properties[6].Name, "Value", "Is Property is the same");
            Assert.AreEqual(mElementsList[12].Properties[6].Value, "Home\r\nAdmin\r\nAbout\r\nContact", "Is Property is the same");
            Assert.AreEqual(mElementsList[13].Properties[7].Name, "Optional Values", "Is Property is the same");
            Assert.AreEqual(mElementsList[13].Properties[7].Value, "Home,Admin,About,Contact,", "Is Property is the same");
        }




    }
}
