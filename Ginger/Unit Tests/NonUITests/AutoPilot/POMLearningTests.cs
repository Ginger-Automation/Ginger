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

namespace UnitTests.NonUITests.AutoPilot
{
    /// <summary>
    /// Summary description for POMLearning
    /// </summary>
    [TestClass]
    public class POMLearningTests
    {

        [TestMethod]
        public void POMLearnFacebookPageTest()
        {
            //Arrange
            ObservableList<ElementInfo> mElementsList = new ObservableList<ElementInfo>();
            SeleniumDriver seleniumDriver = new SeleniumDriver(GingerCore.Drivers.SeleniumDriver.eBrowserType.Chrome);
            seleniumDriver.StartDriver();
            seleniumDriver.RunAction(new ActBrowserElement() { ControlAction = ActBrowserElement.eControlAction.GotoURL, ValueForDriver = "http://www.facebook.com" });
            //Act
            Learn(seleniumDriver, mElementsList);
            while (mElementsList.Count < 30)
            {
                //Let it learn
            }
            seleniumDriver.mStopProcess = true;

            while (seleniumDriver.IsDriverBusy)
            {
                //Let it stop
            }

            //Assert
            Assert.AreNotEqual(mElementsList.Count, 0, "Is Elements Empty List");
            Assert.IsFalse(String.IsNullOrEmpty(mElementsList[10].XPath));
            Assert.AreNotEqual(mElementsList[11].Locators.Count,0, "Is Locators Empty  List");
            Assert.AreNotEqual(mElementsList[12].Properties.Count, 0, "Is Properties Empty  List");
        }

        private async void Learn(SeleniumDriver seleniumDriver, ObservableList<ElementInfo> mElementsList)
        {
            await Task.Run(() => ((IWindowExplorer)seleniumDriver).GetVisibleControls(null, mElementsList, true));
        }
    }
}
