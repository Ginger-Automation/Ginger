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
using GingerCore;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class VariableTest 
    {
       
        [TestInitialize]
        public void TestInitialize()
        {
       
        }

        [TestMethod]
        public void VariableRandomNumber_Min5_Max10_Interval_1()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 5;
            VRN.Max = 10;
            VRN.Interval = 1;

            //Act
            VRN.GenerateAutoValue();


            //Assert
            Assert.IsTrue(decimal.Parse(VRN.Value) >= 5, "vn.Value>=5");
            Assert.IsTrue(decimal.Parse(VRN.Value) <= 10, "vn.Value<=10");
        }

        [TestMethod]
        public void VariableRandomNumber_Min10_Max100_Interval_5()
        {
            //We want to verify that the numbers be get are in interval of 5, can be 10,15,20 etc... but number like 17 is not valid

            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 10;
            VRN.Max = 100;            
            VRN.Interval = 5;

            //vn.ResetValue();

            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);
            VRN.GenerateAutoValue();
            decimal num2 = decimal.Parse(VRN.Value);
            VRN.GenerateAutoValue();
            decimal num3 = decimal.Parse(VRN.Value);

            //first verify all 3 numbers are in range
            Assert.IsTrue(num1 >= 10 && num1 <= 100, "num1 >= 10 && num1 <= 100");
            Assert.IsTrue(num2 >= 10 && num2 <= 100, "num2 >= 10 && num2 <= 100");
            Assert.IsTrue(num3 >= 10 && num3 <= 100, "num3 >= 10 && num3 <= 100");

            // Now check tha validy 5,10,15 etc..
            //Verify that the num modolu 5 is give remainder of 0 or 5
            Assert.IsTrue(num1 % 5 == 0 || num1 % 5 == 5, "num1 % 5 == 0 || num1 % 5 == 5");
            Assert.IsTrue(num2 % 5 == 0 || num2 % 5 == 5, "num1 % 5 == 0 || num1 % 5 == 5");
            Assert.IsTrue(num3 % 5 == 0 || num3 % 5 == 5, "num1 % 5 == 0 || num1 % 5 == 5");
            
            
        }
        [TestMethod]
        public void VariableRandomNumber_Min500_Max100()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 500;
            VRN.Max = 100;
            VRN.Interval = 40;            

            //Act
            VRN.GenerateAutoValue();          

            //Assert
           Assert.AreEqual(VRN.Value, "Error: Min > Max", "Error: Min > Max");
           Assert.AreEqual(VRN.Formula, "Error: Min>Max", "VRN.Formula = Error: Min>Max");
        }


        [TestMethod]
        public void var500_Interval_40()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 0;
            VRN.Max = 500;
            VRN.Interval = 40;

            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);
            
            //Verify numbers areof mutlipliers of 40 starting with 0
            Assert.IsTrue(num1 >= 0 && num1 <= 500, "num1>=0 && num1 <=500");
           Assert.AreEqual(num1 % 40, 0, "num1 % 40 = 0");            
        }

        [TestMethod]
        public void Random_Negative_Minus10_to_Minus_5()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = -10;
            VRN.Max = -5;
            
            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);

            //Verify numbers areof mutlipliers of 40 starting with 0
            Assert.IsTrue(num1 >= -10 && num1 <= -5, "num1>=-10 && num1 <=-5");            
        }

        [TestMethod]
        public void LowerString15char()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Max = 15;
            VRS.IsLowerCase = true;
            
            //Act
            VRS.GenerateAutoValue();

            //Assert
           Assert.AreEqual(VRS.Value, VRS.Value.ToLower(), "VRS.Value, VRS.Value.ToLower()");
        }

        [TestMethod]
        public void upperString15char()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Max = 15;
            VRS.IsUpperCase = true;
            
            //Act
            VRS.GenerateAutoValue();

            //Assert
           Assert.AreEqual(VRS.Value, VRS.Value.ToUpper(), "VRS.Value, VRS.Value.ToUpper()");
        }


        [TestMethod]
        public void bothupperlowerString15char_ShowError()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Max = 15;
            VRS.IsUpperCase = true;
            
            
            //Act

            //When the user change to to lower case make sure the Isupper case get turned off
            VRS.IsLowerCase = true;
            VRS.GenerateAutoValue();

            //Assert
           Assert.AreEqual(VRS.IsUpperCase, false, "VRS.IsUpperCase=false");
           Assert.AreEqual(VRS.Value, VRS.Value.ToLower());

            //TODO: verify the formula
            //Assert.AreEqual(VRS.Formula, "Error", "VRS.Formula=Error");
        }

        [TestMethod]
        public void RandomString_0_3_chars()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Min = 0;
            VRS.Max = 10;
            bool Hit0 = false;
            bool Hit5 = false;
            bool Hit10 = false;

            //Act
            
            for (int i = 0; i < 100;i++ )
            {
                VRS.GenerateAutoValue();
                Assert.IsTrue(VRS.Value.Length >= 0 && VRS.Value.Length <= 10, "VRS.Value.Length >= 0 && VRS.Value.Length <= 3");                
                if (VRS.Value.Length == 0) Hit0 = true;
                if (VRS.Value.Length == 5) Hit5 = true;
                if (VRS.Value.Length == 10) Hit10 = true;                
            }

            //Verify we hit the boundries at least once
            Assert.IsTrue(Hit0, "Hit0");
            Assert.IsTrue(Hit5, "Hit5");
            Assert.IsTrue(Hit10, "Hit10");

        }

        [TestMethod]
        public void DigitsString15char()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Max = 15;            
            VRS.IsDigit = true;
            
            //Act
            VRS.GenerateAutoValue();

            //Assert
           Assert.AreEqual(VRS.Value, VRS.Value.ToLower());
        }

        [TestMethod]
        public void BigNumbers_1111111111111__()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 1111111111111;
            VRN.Max = 9999999999999;
            
            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);

            //Assert            
            Assert.IsTrue(num1 >= 1111111111111 && num1 <= 9999999999999, "num1 >= 1111111111111 && num1 <= 9999999999999");            
        }

        [TestMethod]
        public void Random_Integer_1_10_checkNoFractions()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = 1;
            VRN.Max = 10;
            VRN.IsInteger = true;

            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);

            //Assert            
            //Round should give back the same number if no fraction exist
            Assert.IsTrue(Math.Round(num1, 0) == num1, "Math.Round(num1,0) == num1");
        }


        [TestMethod]
        public void Random_With_Fractions()
        {
            //Arrange
            VariableRandomNumber VRN = new VariableRandomNumber();
            VRN.Min = decimal.Parse("1.5");   // not using M to mark it decimal, using parse since this is ehat happen from UI
            VRN.Max = decimal.Parse("1.7"); ;

            //Act
            VRN.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRN.Value);

            //Assert            
            Assert.IsTrue(num1 >= 1.5M && num1 <= 1.7M, "num1 >= 1.5M && num1 <= 1.7M");            
        }

        //[Ignore]
        //[TestMethod]
        //public void Random_Check_Hit_All_Numbers()
        //{
        //    //Arrange
        //    // array to keep hits per number [0]-21, [1]-22 etc..
        //    int[] a = new int[20];

        //    VariableRandomNumber VRN = new VariableRandomNumber();
        //    VRN.Min = decimal.Parse("21");  
        //    VRN.Max = decimal.Parse("40");
        //    VRN.IsInteger = true;

        //    //Act
        //    for (int i = 0; i < 200;i++)
        //    {
        //        VRN.GenerateAutoValue();
        //        int num1 = int.Parse(VRN.Value);
        //        a[num1 - 21]++;
        //    }
                
        //    //Assert        

        //    // We check that each number was hit at least 2 times, and not more than 20 - should be more or less fare distribution 
        //    //TODO: calculate STD instead distance from 30 on avaergae should be small
        //    for (int i=0;i<20;i++)
        //    {
        //        //TODO: fix rnd algo - fair range should be >=4 <=16 -- or calc range that 1 in 1000 might fail = run 1000 tests and check distribution
        //        Assert.IsTrue(a[i] >= 0, "a[i]>=2");
        //        Assert.IsTrue(a[i] <= 18, "a[i]<=22");
        //    }
            
        //}

        [TestMethod]
        public void RandomStringDigit_5_8()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Min = 5;
            VRS.Max = 8;
            VRS.IsDigit = true;
            //Act
            VRS.GenerateAutoValue();
            decimal num1 = decimal.Parse(VRS.Value);

            //Assert                        
            Assert.IsTrue(VRS.Value.Length >= 5 && VRS.Value.Length <= 8, "VRS.Value.Length >= 5 && VRS.Value.Length <= 8");
        }

        [TestMethod]
        public void RandomStringDigit_6_10_HitAllRange()
        {
            //Arrange
            int[] a = new int[5];
            VariableRandomString VRS = new VariableRandomString();
            VRS.Min = 6;
            VRS.Max = 10;
            VRS.IsDigit = true;

            //Act
            for (int i = 0; i < 50; i++)
            {
                VRS.GenerateAutoValue();
                decimal num1 = decimal.Parse(VRS.Value);
                Assert.IsTrue(VRS.Value.Length >= 6 && VRS.Value.Length <= 10, "VRS.Value.Length >= 6 && VRS.Value.Length <= 10");
                a[VRS.Value.Length-6]++;
            }

            //Assert                        
            for (int i = 0; i < 5; i++)
            {
                Assert.IsTrue(a[i] > 2, "a[i] > 2"); // Check hit - expect at least 2 hits per, avg is 10
            }
        }

        [TestMethod]
        public void RandomString_0_5_Hit0()
        {
            //Arrange
            bool Hit0 = false;
            VariableRandomString VRS = new VariableRandomString();
            VRS.Min = 0;
            VRS.Max = 5;
            
            //Act
            for (int i = 0; i < 100; i++)
            {
                VRS.GenerateAutoValue();
                if (VRS.Value.Length == 0)
                {
                    Hit0 = true;
                    break;
                }
            }

            //Assert                        

            Assert.IsTrue(Hit0, "Hit0");             
        }

        [TestMethod]
        public void RandomStringDigit_13()
        {
            //Arrange
            VariableRandomString VRS = new VariableRandomString();
            VRS.Min = 13;
            VRS.Max = 13;
            VRS.IsDigit = true;

            for (int i = 0; i < 100; i++)
            {
                //Act
                VRS.GenerateAutoValue();
                decimal num1 = decimal.Parse(VRS.Value);

                //Assert            
               Assert.AreEqual(VRS.Value.Length, 13, "VRS.Value.Length=13");
                Assert.IsTrue(num1 >= 0 && num1 <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
            }
        }

        //[Ignore]
        //[TestMethod]
        //public void RandomString_CharsHit()
        //{
        //    //TODO: generate random string and check all chars expected are getting hit

        //    // string Chasrs[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"

        //    //Arrange
        //    //VariableRandomString VRS = new VariableRandomString();
        //    //VRS.Min = 1;
        //    //VRS.Max = 10;            

        //    //for (int i = 0; i < 100; i++)
        //    //{
        //    //    //Act
        //    //    VRS.GenerateAutoValue();
        //    //    decimal num1 = decimal.Parse(VRS.Value);

        //    //    //Assert            
        //    //   Assert.AreEqual(VRS.Value.Length, 13, "VRS.Value.Length=13");
        //    //    Assert.IsTrue(num1 >= 0 && num1 <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
        //    //}
        //}



    }
}
