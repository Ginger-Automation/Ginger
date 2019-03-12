#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerCore.Drivers.CommunicationProtocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerCore.Actions.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerTestHelper;
using Amdocs.Ginger.Common.UIElement;

namespace UnitTests.NonUITests.PayLoadTest
{
    [TestClass]
    [Level1]
    public class PayLoadTest 
    {

        //static JavaDriver mDriver = null;
        //static String AppName="Foxtel Test App";

        //[ClassInitialize()]
        //public static void ClassInit(TestContext context)
        //{

        //    ActLaunchJavaWSApplication LJA = new ActLaunchJavaWSApplication();
        //    LJA.LaunchJavaApplication = true;
        //    LJA.LaunchWithAgent = true;
        //    LJA.WaitForWindowTitle = AppName;
        //    LJA.Port = "7575";
        //    LJA.URL = Common.getGingerUnitTesterDocumentsFolder() + @"JavaTestApp\Foxtel Test App.jar";
        //    LJA.Execute();

        //    mDriver = new JavaDriver(null);
        //    mDriver.JavaAgentHost = "127.0.0.1";
        //    mDriver.JavaAgentPort = 7575;
        //    mDriver.CommandTimeout = 120;
        //    mDriver.cancelAgentLoading = false;
        //    mDriver.DriverLoadWaitingTime = 30;
        //    mDriver.ImplicitWait = 30;
        //    mDriver.StartDriver();
        //}

        //[ClassCleanup()]
        //public static void ClassCleanup()
        //{
        //    PayLoad PLClose = new PayLoad("WindowAction");
        //    PLClose.AddValue("CloseWindow");
        //    // PLClose.AddEnumValue(AJTE.WaitforIdle);
        //    PLClose.AddValue(eLocateBy.ByTitle.ToString());
        //    PLClose.AddValue(AppName);
        //    // PLClose.AddValue(AJTE.ValueForDriver.ToString()); // TODO: remove if not needed
        //    PLClose.ClosePackage();
        //   mDriver.Send(PLClose);
        //}

        #region PayLoad on C# Side
        [TestMethod]  [Timeout(60000)]
        public void SimpleString()
        {
            //Arrange
            string s0 = "Hello World";

            PayLoad pl = new PayLoad("SimpleString");            
            pl.AddValue(s0);            
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();


            PayLoad pl2 = new PayLoad(b);            
            string s1 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(s0, s1);
           Assert.AreEqual(pl.Name, pl2.Name);
        }

        [TestMethod]  [Timeout(60000)]
        public void PayloadOnSameSide()
        {
            //Arrange
            string s0 = "Hello World";

            PayLoad pl = new PayLoad("SimpleString");
            pl.AddValue(s0);
            pl.ClosePackage();           


            PayLoad pl2 = pl;          
            string s1 = pl2.GetValueString();

            //Assert
            Assert.AreEqual(s0, s1);
            Assert.AreEqual(pl.Name, pl2.Name);
        }

        [TestMethod]  [Timeout(60000)]        
        public void UTF16String()
        {
            //Arrange
            string s0 = "Hello World גךעחךגכ ■N╜ !@#$!@#$% ÜÑ├µΦ";

            PayLoad pl = new PayLoad("UTF16String");
            pl.AddStringUTF16(s0);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            string s1 = pl2.GetStringUTF16();

            //Assert
           Assert.AreEqual(s0, s1);
        }


        [TestMethod]  [Timeout(60000)]
        public void LongString5000()
        {
            //Arrange
            
            string s0 = "Hello World";
            while (s0.Length<5000)
            {
                s0 += s0; // double the string
            }

            PayLoad pl = new PayLoad("LongString5000");
            pl.AddValue(s0);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();


            PayLoad pl2 = new PayLoad(b);            
            string s1 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(s0, s1);
        }


        [TestMethod]  [Timeout(60000)]
        public void VeryLongString500000()
        {
            //Arrange

            string s0 = "Hello World";
            while (s0.Length < 500000)
            {
                s0 += s0;
            }

            PayLoad pl = new PayLoad("VeryLongString500000");
            pl.AddValue(s0);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();;


            PayLoad pl2 = new PayLoad(b);            
            string s1 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(s0, s1);
        }

        [TestMethod]  [Timeout(60000)]        
        public void StringWithSpecialCharsUTF8()
        {
            //Arrange            
            string s0 = @"ABC!@#$%^&*(){}[]~|\/<>,.~`XYZ";

            PayLoad pl = new PayLoad("StringWithSpecialChars");
            pl.AddValue(s0);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();


            PayLoad pl2 = new PayLoad(b);            
            string s1 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(s0, s1);
        }


        [TestMethod]  [Timeout(60000)]
        public void SimpleInt()
        {
            //Arrange
            int val = 123;

            PayLoad pl = new PayLoad("SimpleInt");            
            pl.AddValue(val);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            int val2 = pl2.GetValueInt();

            //Assert
           Assert.AreEqual(val, val2);
        }

        [TestMethod]  [Timeout(60000)]
        public void NegativeInt()
        {
            //Arrange
            int val = -123;

            PayLoad pl = new PayLoad("NegativeInt");
            pl.AddValue(val);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            int val2 = pl2.GetValueInt();

            //Assert
           Assert.AreEqual(val, val2);
        }

        [TestMethod]  [Timeout(60000)]
        public void IntMaxValue()
        {
            //Arrange
            int val = Int16.MaxValue;

            PayLoad pl = new PayLoad("IntMaxValue");
            pl.AddValue(val);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            int val2 = pl2.GetValueInt();

            //Assert
           Assert.AreEqual(val, val2);
        }




        [TestMethod]  [Timeout(60000)]
        public void ComplexStringWith2Ints()
        {
            //Arrange
            int vala = 1237435;
            int valb = -185;
            string vals = "Not so long String";

            PayLoad pl = new PayLoad("ComplexStringWith2Ints");
            pl.AddValue(vala);
            pl.AddValue(valb);
            pl.AddValue(vals);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            int vala2 = pl2.GetValueInt();
            int valb2 = pl2.GetValueInt();
            string vals2 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(vala, vala2);
           Assert.AreEqual(valb, valb2);
           Assert.AreEqual(vals, vals2);
        }

        [TestMethod]  [Timeout(60000)]
        public void ComplexEnumStringsInts()
        {
            //Arrange
            int vala = 123;
            int valb = 545;
            string valsa = "String1";
            string valsb = "ZXCVFDSW";
            eLocateBy loc = eLocateBy.ByName;

            PayLoad pl = new PayLoad("ComplexEnumStringsInts");
            pl.AddValue(vala);
            pl.AddValue(valb);
            pl.AddValue(valsa);
            pl.AddValue(valsb);
            pl.AddEnumValue(loc);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            int vala2 = pl2.GetValueInt();
            int valb2 = pl2.GetValueInt();
            string valsa2 = pl2.GetValueString();
            string valsb2 = pl2.GetValueString();
            string Loc2 = pl2.GetValueEnum();

            //Assert
           Assert.AreEqual(vala, vala2);
           Assert.AreEqual(valb, valb2);
           Assert.AreEqual(valsa, valsa2);
           Assert.AreEqual(valsb, valsb2);
           Assert.AreEqual(loc.ToString(), Loc2);
        }


        [TestMethod]  [Timeout(60000)]
        public void DumpTest()
        {
            //Arrange
            ActJavaElement act = new ActJavaElement();
            act.LocateBy = eLocateBy.ByName;
            act.LocateValue = "ABC";
            act.Value = "123";
            act.ControlAction = ActJavaElement.eControlAction.SetValue;            

            PayLoad pl = act.Pack();
            byte[] b = pl.GetPackage();
            PayLoad pl2 = new PayLoad(b);            

            //Act
            pl2.DumpToConsole();

            //Assert
        }

        [TestMethod]  [Timeout(60000)]
        public void ActJavaElementAction()
        {
            //Arrange
            ActJavaElement act = new ActJavaElement();
            act.WaitforIdle = ActJavaElement.eWaitForIdle.Medium;
            act.LocateBy = eLocateBy.ByName;
            act.LocateValue = "ABC";
            act.Value = "123";
            act.ControlAction = ActJavaElement.eControlAction.SetValue;
           

            //Act
            PayLoad pl = act.Pack();
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);            
            pl2.DumpToConsole();
            string WaitForIdle = pl2.GetValueEnum();
            string LocateBy = pl2.GetValueEnum();
            string LocateValue = pl2.GetValueString();
            string Value = pl2.GetValueString();
            string ControlAction = pl2.GetValueEnum();

            //Assert
           Assert.AreEqual(act.LocateBy.ToString(), LocateBy);
           Assert.AreEqual(act.LocateValue, LocateValue);
           Assert.AreEqual(act.Value, Value);
           Assert.AreEqual(act.ControlAction.ToString(), ControlAction);

        }

        [TestMethod]  [Timeout(60000)]
        public void SpeedTestSimpleStringX100()
        {
            //Arrange
            Stopwatch st = new Stopwatch();
            st.Start();
            string s0 = "ABCDEFGHIJ";

            // Act
            for (int i = 0; i < 100; i++)
            {
                PayLoad pl = new PayLoad("SpeedTestSimpleStringX100");
                pl.AddValue(s0);
                pl.ClosePackage();

                byte[] b = pl.GetPackage();

                PayLoad pl2 = new PayLoad(b);                
                string s1 = pl2.GetValueString();
            }

            st.Stop();

            //Assert
            Assert.IsTrue(st.ElapsedMilliseconds < 30);
        }


        [TestMethod]  [Timeout(60000)]
        public void NullTest()
        {
            //Arrange
            string s0 = null;

            PayLoad pl = new PayLoad("NullString");
            pl.AddValue(s0);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();

            PayLoad pl2 = new PayLoad(b);
            string s1 = pl2.GetValueString();

            //Assert
           Assert.AreEqual(s0, s1);
           Assert.AreEqual(pl.Name, pl2.Name);            
        }


        [TestMethod]  [Timeout(60000)]
        public void PayLoadList()
        {

            //Arrange
            PayLoad pl = new PayLoad("Package wth list of Payloads");

            List<PayLoad> list = new List<PayLoad>();
            
            PayLoad pl1 = new PayLoad("PL1");
            pl1.AddValue("ABC");
            pl1.AddValue("DEF");
            pl1.ClosePackage();
            list.Add(pl1);

            PayLoad pl2 = new PayLoad("PL2");
            pl2.AddValue("GHI");
            pl2.AddValue("JKL");
            pl2.ClosePackage();
            list.Add(pl2);

            pl.AddListPayLoad(list);
            pl.ClosePackage();

            
            // Act
            byte[] b = pl.GetPackage();
            PayLoad plc = new PayLoad(b);
            List<PayLoad> list2 = plc.GetListPayLoad();            

            //Assert
           Assert.AreEqual(2, list2.Count, "list2.Count=2");
     
           Assert.AreEqual("PL1",list2[0].Name, "list2[0].Name =PL1");
           Assert.AreEqual("PL2", list2[1].Name,  "list2[1].Name =PL2");

            //Assert.AreEqual(pl1.Name, pl2.Name);
        }

        #endregion

        #region Validate Payload Response from Side

        //[TestMethod]  [Timeout(60000)]
        //public void SimpleInt_ResponseFromJava()
        //{
        //    //Arrange
        //    int val = 123;

        //    PayLoad pl = new PayLoad("UnitTest");
        //    pl.AddValue("IntegerValueTest");
        //    pl.AddValue(val);
        //    pl.ClosePackage();

        //    PayLoad pl2 = mDriver.Send(pl);
        //    int val2 = pl2.GetValueInt();

        //    //Assert
        //    Assert.AreEqual(val, val2);
        //}

        //[TestMethod]  [Timeout(60000)]
        //public void NegativeInt_ResponseFromJava()
        //{
        //    //Arrange
        //    int val = -123;

        //    PayLoad pl = new PayLoad("UnitTest");
        //    pl.AddValue("IntegerValueTest");
        //    pl.AddValue(val);
        //    pl.ClosePackage();


        //    PayLoad pl2 = mDriver.Send(pl);
        //    int val2 = pl2.GetValueInt();

        //    //Assert
        //    Assert.AreEqual(val, val2);
        //}

        //[TestMethod]  [Timeout(60000)]
        //public void IntMinValue_ResponseFromJava()
        //{
        //    //Arrange
        //    int val = Int16.MinValue;

        //    PayLoad pl = new PayLoad("UnitTest");
        //    pl.AddValue("IntegerValueTest");
        //    pl.AddValue(val);
        //    pl.ClosePackage();


        //    PayLoad pl2 = mDriver.Send(pl);
        //    int val2 = pl2.GetValueInt();

        //    //Assert
        //    Assert.AreEqual(val, val2);
        //}

        //[TestMethod]  [Timeout(60000)]
        //public void IntMaxValue_ResponseFromJava()
        //{
        //    //Arrange
        //    int val = Int16.MaxValue;

        //    PayLoad pl = new PayLoad("UnitTest");
        //    pl.AddValue("IntegerValueTest");
        //    pl.AddValue(val);
        //    pl.ClosePackage();


        //    PayLoad pl2 = mDriver.Send(pl);
        //    int val2 = pl2.GetValueInt();

        //    //Assert
        //    Assert.AreEqual(val, val2);
        //}



        //[TestMethod]  [Timeout(60000)]
        //public void IntRangeOfNumberTest()
        //{
        //    for (int i = -500; i < 500; i++)
        //    {
        //        PayLoad pl = new PayLoad("UnitTest");
        //        pl.AddValue("IntegerValueTest");
        //        pl.AddValue(i);
        //        pl.ClosePackage();


        //        PayLoad pl2 = mDriver.Send(pl);
        //        int val2 = pl2.GetValueInt();
        //        Assert.AreEqual(i, val2);
        //    }
        //}


        ////TODO: Add more tests to cover validation for other value types


        #endregion
    }
}
