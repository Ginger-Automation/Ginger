using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web.Playwright;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Drivers
{
    [TestClass]
    [TestCategory(TestCategory.IntegrationTest)]
    public class PlaywrightDriverTests
    {
        [TestMethod]
        public void GetVisualElementsInfo()
        {
            PlaywrightDriver driver = new()
            {
                HeadlessBrowserMode = false,
            };
            driver.StartDriver();
            ActBrowserElement gotoURLAction = new()
            {
                ControlAction = ActBrowserElement.eControlAction.GotoURL,
                Value = "https://www.saucedemo.com/",
            };
            driver.RunAction(gotoURLAction);

            var visualElements = driver.GetVisualElementsInfo();
            
        }
    }
}
