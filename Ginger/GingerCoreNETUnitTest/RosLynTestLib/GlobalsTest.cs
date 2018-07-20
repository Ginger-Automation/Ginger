#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.RosLynTestLib
{
    [TestClass]
    [Level1]
    public class GlobalsTest
    {
        

        [ClassInitialize]        
        public static void ClassInitialize(TestContext TestContext)
        {
              
        }
        

        //[TestMethod]
        //public void StartFireFoxDriver()
        //{
        //    //Arrange            
        //    Globals g = new Globals();

        //    // Assembly a1 = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\SeleniumPlugin\bin\Debug\netstandard2.0\WebDriver.dll");

        //    //Act            
        //    // g.LoadPluginPackage(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\PluginPackages\SeleniumPluginPackage.1.0.0");
        //    g.LoadPluginPackage(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\SeleniumPlugin\bin\Debug\netstandard2.0");
            
        //    g.StartNode("Selenium FireFox Driver", "Selenium 1");
            

        //    //TODO: add asserts and clean + close driver

        //    // g.StartDriver("Selenium Chrome Driver", "Selenium 1");
        //    // g.StartDriver("Selenium Internet Explorer Driver", "Selenium 1");


        //    //Assert

        //    // Assert.IsTrue(string.IsNullOrEmpty(GA.Errors));
        //    // Assert.AreEqual(GNA.IsConnected, false);

        //}
    }
}
