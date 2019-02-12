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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCore.Actions;
using GingerCore.Drivers;
using GingerCore.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    [Ignore]
    public class JSPayloadTest 
    {

        SeleniumDriver mDriver;

        [TestInitialize]
        public void TestInitialize()
        {
            mDriver = new SeleniumDriver(SeleniumDriver.eBrowserType.Chrome);//having Firefox issue on CMI server
            mDriver.StartDriver();

            ActGotoURL a = new ActGotoURL();
            a.ValueForDriver = TestResources.GetTestResourcesFile(@"HTML\JSPayLoad.html");

            mDriver.RunAction(a);

            mDriver.InjectGingerHTMLHelper();
        }

        [TestCleanup()]
        public void TestCleanUp()
        {
            mDriver.CloseDriver();
        }

        [TestMethod]  [Timeout(60000)]
        public void EchoSimpleString()
        {
            // Arrange            
            string txt = "ABC";

            PayLoad PLRequest = new PayLoad("Echo");
            PLRequest.AddValue(txt);
            PLRequest.ClosePackage();

            //Act            
            PayLoad PLRC = mDriver.ExceuteJavaScriptPayLoad(PLRequest);            
            string txtRC = PLRC.GetValueString();

            //Assert
           Assert.AreEqual(PLRC.Name, "Echo Response");
           Assert.AreEqual(txt, txtRC, "txt=txtRC");            
        }

        [TestMethod]  [Timeout(60000)]
        public void EchoSringwithSymbols()
        {
            // Arrange            
            string txt = "ABC } $- = 123 !@#$%(^&'~*)_+{";
            PayLoad PLRequest = new PayLoad("Echo");
            PLRequest.AddValue(txt);
            PLRequest.ClosePackage();

            //Act
            PayLoad PLRC = mDriver.ExceuteJavaScriptPayLoad(PLRequest);            
            string txtRC = PLRC.GetValueString();

            //Assert
           Assert.AreEqual(PLRC.Name, "Echo Response");
           Assert.AreEqual(txt, txtRC, "txt=txtRC");

        }

        //[TestMethod]  [Timeout(60000)]
        //[Ignore]
        //public void EchoSringwithHebrew()
        //{
        //    // Arrange            
        //    string txt = "ABC שלום גךדכחע ,גלשדףלרעחדקראוםקרוא לךגחכעחד ג";
        //    PayLoad PLRequest = new PayLoad("Echo");
        //    PLRequest.AddValue(txt);
        //    PLRequest.ClosePackage();

        //    //Act
        //    PayLoad PLRC = mDriver.ExceuteJavaScriptPayLoad(PLRequest);            
        //    string txtRC = PLRC.GetValueString();

        //    //Assert
        //   Assert.AreEqual(PLRC.Name, "Echo Response");
        //   Assert.AreEqual(txt, txtRC, "txt=txtRC");

        //}

        [TestMethod]  [Timeout(60000)]
        public void EchoLongSring()
        {
            // Arrange            
            string txt = "AAAAAAAA";
            while (txt.Length< 5000)
            {
                txt += txt;
            }
            PayLoad PLRequest = new PayLoad("Echo");
            PLRequest.AddValue(txt);
            PLRequest.ClosePackage();

            //Act
            PayLoad PLRC = mDriver.ExceuteJavaScriptPayLoad(PLRequest);            
            string txtRC = PLRC.GetValueString();

            //Assert
           Assert.AreEqual(PLRC.Name, "Echo Response");
           Assert.AreEqual(txt, txtRC, "txt=txtRC");

        }

        [TestMethod]  [Timeout(60000)]        
        public void GetVisibleElements()
        {            
            PayLoad PLRequest = new PayLoad("GetVisibleElements");         
            PLRequest.ClosePackage();

            //Act
            PayLoad PLRC = mDriver.ExceuteJavaScriptPayLoad(PLRequest);
            List<PayLoad> Elements = PLRC.GetListPayLoad();
            
            //Assert
           Assert.AreEqual(PLRC.Name, "HTML Elements");
           Assert.AreEqual(Elements.Count, 11, "Elements.Count = 11");
            

        }

    }
}
