using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

namespace GingerPluginCoreTest.CommunicationProtocol
{
    [TestClass]
    [Level1]
    class PayloadWithStructTest
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
                StructTest();
                // NoStructTest();
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

            // Act
            byte[] b = pl.GetPackage();
            NewPayLoad pl2 = new NewPayLoad(b);
            nums p2 = pl2.GetValue<nums>();

            //Assert
            Assert.AreEqual(p1.num1, p2.num1);
            Assert.AreEqual(p1.num2, p2.num2);
            Assert.AreEqual(p1.num3, p2.num3);

        }


    }
}
