#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GingerPluginCoreTest.CommunicationProtocol
{
    [TestClass]
    [Level1]
    public class PayloadWithStructTest
    {
        
        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestCleanup()]
        public void TestCleanUp()
        {

        }
  


        struct ppl
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
            public string first;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
            public string last;

            public int age;
        }

        
        [TestMethod]
        public void StructTestSpeed()
        {
            // for per checks of using struct and no struct
            for (int i = 0; i < 500000; i++)
            {
                // StructTest();
                // NoStructTest();
                ListsStructTestJSON();
            }
        }

        [TestMethod]
        public void NoStructTest()
        {
            string first = "fffffffffffffffffffffffffff";
            string last = "lllllllllllllllllllllllllll";
            int age = 25;

            //Arrange

            NewPayLoad pl = new NewPayLoad("ppl");
            pl.AddValue(first);
            pl.AddValue(last);
            pl.AddValue(age);
            pl.ClosePackage();

            // Act
            byte[] b = pl.GetPackage();
            NewPayLoad pl2 = new NewPayLoad(b);
            string first2 = pl2.GetValueString();
            string last2 = pl2.GetValueString();
            int age2 = pl2.GetValueInt();

            //Assert
            Assert.AreEqual(first, first2, "first");
            Assert.AreEqual(last, last2, "last");
            Assert.AreEqual(age, age2, "age");
        }


        [TestMethod]
        public void StructTest()
        {
            string first = "fffffffffffffffffffffffffff";
            string last = "lllllllllllllllllllllllllll";
            int age = 25;

            //Arrange
            ppl p1 = new ppl() { first = first, last = last, age = age };
            NewPayLoad pl = new NewPayLoad("ppl");
            pl.AddValue<ppl>(p1);
            pl.ClosePackage();

            // destroy the data as string is pointer to memory so we want to verify we didn't copy a pointer
            p1.first = "a";
            p1.last = "b";
            p1.age = 1;

            // Act
            byte[] b = pl.GetPackage();
            NewPayLoad pl2 = new NewPayLoad(b);
            ppl p2 = pl2.GetValue<ppl>();

            //Assert
            Assert.AreEqual(first, p2.first, "first");
            Assert.AreEqual(last, p2.last, "last");
            Assert.AreEqual(age, p2.age, "age");
        }

        struct nums
        {
            public int num1;
            public float num2;
            public Single num3;

        }

        [TestMethod]
        public void NumsStructTest()
        {
            //Arrange
            nums p1 = new nums() { num1 = 7, num2 = 5.123F, num3 = 7.344F };
            NewPayLoad pl = new NewPayLoad("ppl");
            pl.AddValue<nums>(p1);
            pl.ClosePackage();

            string s134 = pl.BufferInfo;

            // Act
            byte[] b = pl.GetPackage();
            NewPayLoad pl2 = new NewPayLoad(b);
            nums p2 = pl2.GetValue<nums>();

            //Assert
            Assert.AreEqual(p1.num1, p2.num1);
            Assert.AreEqual(p1.num2, p2.num2);
            Assert.AreEqual(p1.num3, p2.num3);

        }


        struct complexStructwithStrings
        {            
            public string a;         
            public List<string> strings;            
            public string b;
        }

        [TestMethod]
        public void ListsStructTestJSON()
        {
            //Arrange            
            complexStructwithStrings s1 = new complexStructwithStrings() { a = "aaa", strings = new List<string>() { "aaa", "bbb", "ccc" }, b = "ggg" };

            // Act
            NewPayLoad pl = new NewPayLoad("ll");
            pl.AddJSONValue(s1);
            pl.ClosePackage();

            string s123 = pl.BufferInfo;

            byte[] b = pl.GetPackage();
            NewPayLoad pl2 = new NewPayLoad(b);

            complexStructwithStrings s2 = pl2.GetJSONValue<complexStructwithStrings>();

            //Assert
            Assert.AreEqual(s1.a, s2.a);
            Assert.AreEqual(s2.strings.Count, s2.strings.Count);
            Assert.AreEqual(s2.strings[0], s2.strings[0]);
            Assert.AreEqual(s2.strings[1], s2.strings[1]);
            Assert.AreEqual(s2.strings[2], s2.strings[2]);
            Assert.AreEqual(s1.b, s2.b);
        }


    }
}
