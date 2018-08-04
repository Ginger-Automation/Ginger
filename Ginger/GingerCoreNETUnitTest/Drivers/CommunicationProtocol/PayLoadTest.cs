//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common.UIElement;
//using GingerCoreNET.Drivers.CommonActionsLib;
//using GingerCoreNET.Drivers.CommunicationProtocol;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;



//namespace GingerCoreNETUnitTest.Drivers.CommunicationProtocol
//{
//    [TestClass]
//    [Level1]
//    public class PayLoadTest 
//    {
//        [TestInitialize]
//        public void TestInitialize()
//        {
//        }

//        [TestCleanup()]
//        public void TestCleanUp()
//        {
            
//        }

        
//        [TestMethod]
//        public void SimpleString()
//        {
//            //Arrange
//            string s0 = "Hello World";

//            NewPayLoad pl = new NewPayLoad("SimpleString");            
//            pl.AddValue(s0);            
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();


//            NewPayLoad pl2 = new NewPayLoad(b);            
//            string s1 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(s0, s1);
//           Assert.AreEqual(pl.Name, pl2.Name);
//        }

//        [TestMethod]
//        public void PayloadOnSameSide()
//        {
//            //Arrange
//            string s0 = "Hello World";

//            NewPayLoad pl = new NewPayLoad("SimpleString");
//            pl.AddValue(s0);
//            pl.ClosePackage();           


//            NewPayLoad pl2 = pl;          
//            string s1 = pl2.GetValueString();

//            //Assert
//            Assert.AreEqual(s0, s1);
//            Assert.AreEqual(pl.Name, pl2.Name);
//        }

//        [TestMethod]        
//        public void UTF16String()
//        {
//            //Arrange
//            string s0 = "Hello World גךעחךגכ ■N╜ !@#$!@#$% ÜÑ├µΦ";

//            NewPayLoad pl = new NewPayLoad("UTF16String");
//            pl.AddStringUTF16(s0);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            string s1 = pl2.GetStringUTF16();

//            //Assert
//           Assert.AreEqual(s0, s1);
//        }


//        [TestMethod]
//        public void LongString5000()
//        {
//            //Arrange
            
//            string s0 = "Hello World";
//            while (s0.Length<5000)
//            {
//                s0 += s0; // double the string
//            }

//            NewPayLoad pl = new NewPayLoad("LongString5000");
//            pl.AddValue(s0);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();


//            NewPayLoad pl2 = new NewPayLoad(b);            
//            string s1 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(s0, s1);
//        }


//        [TestMethod]
//        public void VeryLongString500000()
//        {
//            //Arrange

//            string s0 = "Hello World";
//            while (s0.Length < 500000)
//            {
//                s0 += s0;
//            }

//            NewPayLoad pl = new NewPayLoad("VeryLongString500000");
//            pl.AddValue(s0);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();;


//            NewPayLoad pl2 = new NewPayLoad(b);            
//            string s1 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(s0, s1);
//        }

//        [TestMethod]        
//        public void StringWithSpecialCharsUTF8()
//        {
//            //Arrange            
//            string s0 = @"ABC!@#$%^&*(){}[]~|\/<>,.~`XYZ";

//            NewPayLoad pl = new NewPayLoad("StringWithSpecialChars");
//            pl.AddValue(s0);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();


//            NewPayLoad pl2 = new NewPayLoad(b);            
//            string s1 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(s0, s1);
//        }


//        [TestMethod]
//        public void SimpleInt()
//        {
//            //Arrange
//            int val = 123;

//            NewPayLoad pl = new NewPayLoad("SimpleInt");            
//            pl.AddValue(val);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            int val2 = pl2.GetValueInt();

//            //Assert
//           Assert.AreEqual(val, val2);
//        }

//        [TestMethod]
//        public void NegativeInt()
//        {
//            //Arrange
//            int val = -123;

//            NewPayLoad pl = new NewPayLoad("NegativeInt");
//            pl.AddValue(val);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            int val2 = pl2.GetValueInt();

//            //Assert
//           Assert.AreEqual(val, val2);
//        }

//        [TestMethod]
//        public void IntMaxValue()
//        {
//            //Arrange
//            int val = Int16.MaxValue;

//            NewPayLoad pl = new NewPayLoad("IntMaxValue");
//            pl.AddValue(val);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            int val2 = pl2.GetValueInt();

//            //Assert
//           Assert.AreEqual(val, val2);
//        }




//        [TestMethod]
//        public void ComplexStringWith2Ints()
//        {
//            //Arrange
//            int vala = 1237435;
//            int valb = -185;
//            string vals = "Not so long String";

//            NewPayLoad pl = new NewPayLoad("ComplexStringWith2Ints");
//            pl.AddValue(vala);
//            pl.AddValue(valb);
//            pl.AddValue(vals);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            int vala2 = pl2.GetValueInt();
//            int valb2 = pl2.GetValueInt();
//            string vals2 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(vala, vala2);
//           Assert.AreEqual(valb, valb2);
//           Assert.AreEqual(vals, vals2);
//        }

//        [TestMethod]
//        public void ComplexEnumStringsInts()
//        {
//            //Arrange
//            int vala = 123;
//            int valb = 545;
//            string valsa = "String1";
//            string valsb = "ZXCVFDSW";
//            eLocateBy loc = eLocateBy.ByName;

//            NewPayLoad pl = new NewPayLoad("ComplexEnumStringsInts");
//            pl.AddValue(vala);
//            pl.AddValue(valb);
//            pl.AddValue(valsa);
//            pl.AddValue(valsb);
//            pl.AddEnumValue(loc);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);            
//            int vala2 = pl2.GetValueInt();
//            int valb2 = pl2.GetValueInt();
//            string valsa2 = pl2.GetValueString();
//            string valsb2 = pl2.GetValueString();
//            string Loc2 = pl2.GetValueEnum();

//            //Assert
//           Assert.AreEqual(vala, vala2);
//           Assert.AreEqual(valb, valb2);
//           Assert.AreEqual(valsa, valsa2);
//           Assert.AreEqual(valsb, valsb2);
//           Assert.AreEqual(loc.ToString(), Loc2);
//        }


//        //[TestMethod]
//        //public void DumpTest()
//        //{
//        //    //Arrange
//        //    ActJavaElement act = new ActJavaElement();
//        //    act.LocateBy = eLocateBy.ByName;
//        //    act.LocateValue = "ABC";
//        //    act.Value = "123";
//        //    act.ControlAction = ActJavaElement.eControlAction.SetValue;            

//        //    PayLoad pl = act.Pack();
//        //    byte[] b = pl.GetPackage();
//        //    PayLoad pl2 = new PayLoad(b);            

//        //    //Act
//        //    pl2.DumpToConsole();

//        //    //Assert
//        //}

//        //[TestMethod]
//        //public void ActJavaElementAction()
//        //{
//        //    //Arrange
//        //    ActJavaElement act = new ActJavaElement();
//        //    act.WaitforIdle = ActJavaElement.eWaitForIdle.Medium;
//        //    act.LocateBy = eLocateBy.ByName;
//        //    act.LocateValue = "ABC";
//        //    act.Value = "123";
//        //    act.ControlAction = ActJavaElement.eControlAction.SetValue;
           

//        //    //Act
//        //    PayLoad pl = act.Pack();
//        //    byte[] b = pl.GetPackage();

//        //    PayLoad pl2 = new PayLoad(b);            
//        //    pl2.DumpToConsole();
//        //    string WaitForIdle = pl2.GetValueEnum();
//        //    string LocateBy = pl2.GetValueEnum();
//        //    string LocateValue = pl2.GetValueString();
//        //    string Value = pl2.GetValueString();
//        //    string ControlAction = pl2.GetValueEnum();

//        //    //Assert
//        //   Assert.AreEqual(act.LocateBy.ToString(), LocateBy);
//        //   Assert.AreEqual(act.LocateValue, LocateValue);
//        //   Assert.AreEqual(act.Value, Value);
//        //   Assert.AreEqual(act.ControlAction.ToString(), ControlAction);

//        //}

//        [TestMethod]
//        public void SpeedTestSimpleStringX100()
//        {
//            //Arrange
//            Stopwatch st = new Stopwatch();
//            st.Start();
//            string s0 = "ABCDEFGHIJ";

//            // Act
//            for (int i = 0; i < 100; i++)
//            {
//                NewPayLoad pl = new NewPayLoad("SpeedTestSimpleStringX100");
//                pl.AddValue(s0);
//                pl.ClosePackage();

//                byte[] b = pl.GetPackage();

//                NewPayLoad pl2 = new NewPayLoad(b);                
//                string s1 = pl2.GetValueString();
//            }

//            st.Stop();

//            //Assert
//            Assert.IsTrue(st.ElapsedMilliseconds < 30);
//        }


//        [TestMethod]
//        public void NullTest()
//        {
//            //Arrange
//            string s0 = null;

//            NewPayLoad pl = new NewPayLoad("NullString");
//            pl.AddValue(s0);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);
//            string s1 = pl2.GetValueString();

//            //Assert
//           Assert.AreEqual(s0, s1);
//           Assert.AreEqual(pl.Name, pl2.Name);            
//        }


//        [TestMethod]
//        public void PayLoadList()
//        {

//            //Arrange
//            NewPayLoad pl = new NewPayLoad("Package wth list of Payloads");

//            List<NewPayLoad> list = new List<NewPayLoad>();
            
//            NewPayLoad pl1 = new NewPayLoad("PL1");
//            pl1.AddValue("ABC");
//            pl1.AddValue("DEF");
//            pl1.ClosePackage();
//            list.Add(pl1);

//            NewPayLoad pl2 = new NewPayLoad("PL2");
//            pl2.AddValue("GHI");
//            pl2.AddValue("JKL");
//            pl2.ClosePackage();
//            list.Add(pl2);

//            pl.AddListPayLoad(list);
//            pl.ClosePackage();

            
//            // Act
//            byte[] b = pl.GetPackage();
//            NewPayLoad plc = new NewPayLoad(b);
//            List<NewPayLoad> list2 = plc.GetListPayLoad();            

//            //Assert
//           Assert.AreEqual(2, list2.Count, "list2.Count=2");
     
//           Assert.AreEqual("PL1",list2[0].Name, "list2[0].Name =PL1");
//           Assert.AreEqual("PL2", list2[1].Name,  "list2[1].Name =PL2");

//            //Assert.AreEqual(pl1.Name, pl2.Name);
//        }


//        [TestMethod]
//        public void Guid()
//        {
//            //Arrange
//            Guid guid = System.Guid.NewGuid();


//            NewPayLoad pl = new NewPayLoad("SimpleGuid");
//            pl.AddValue(guid);
//            pl.ClosePackage();

//            // Act
//            byte[] b = pl.GetPackage();

//            NewPayLoad pl2 = new NewPayLoad(b);
//            Guid g1 = pl2.GetGuid();

//            //Assert
//            Assert.AreEqual(guid.ToString(), g1.ToString());
//            Assert.AreEqual(pl.Name, pl2.Name);
//        }


//    }
//}
