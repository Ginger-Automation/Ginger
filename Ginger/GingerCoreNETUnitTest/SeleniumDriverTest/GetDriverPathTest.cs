#region License
/*
Copyright © 2014-2026 European Support Limited

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

using GingerCore.Drivers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
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
        public async Task GetchromeDriverPath()
        {
            string DriverPath = await driver.GetDriverPath(SeleniumDriver.eBrowserType.Chrome);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }

        [TestMethod]
        public async Task GetFirfoxDriverPath()
        {
            string DriverPath = await driver.GetDriverPath(SeleniumDriver.eBrowserType.FireFox);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }

        [TestMethod]
        public async Task GetEdgexDriverPath()
        {
            string DriverPath = await driver.GetDriverPath(SeleniumDriver.eBrowserType.Edge);
            bool v = File.Exists(DriverPath);
            Assert.AreEqual(true, v);
        }
    }
}
