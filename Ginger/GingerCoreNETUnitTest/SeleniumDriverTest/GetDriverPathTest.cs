using GingerCore.Drivers;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.SeleniumDriverTest
{
    [TestClass]
    [Ignore]
    public class GetDriverPathTest
    {
        SeleniumDriver driver;
        [TestInitialize]
        public void TestInitialize()
        {
            driver = new SeleniumDriver();
        }

        [TestMethod]
        public void GetchromeDriverPath()
        {
            string DriverPath = driver.GetDriverPath(SeleniumDriver.eBrowserType.Chrome);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }

        [TestMethod]
        public void GetFirfoxDriverPath()
        {
            string DriverPath = driver.GetDriverPath(SeleniumDriver.eBrowserType.FireFox);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }

        [TestMethod]
        public void GetEdgexDriverPath()
        {
            string DriverPath = driver.GetDriverPath(SeleniumDriver.eBrowserType.Edge);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }
    }
}
