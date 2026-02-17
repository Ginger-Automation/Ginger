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
using Amdocs.Ginger.Common.Enums;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class RandomVariableTest
    {

        #region Default Class/Test Initialize Methods
        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            //
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            //
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // before every test
        }

        [TestCleanup]
        public void TestCleanUp()
        {
            //after every test
        }
        #endregion

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_TestImageType()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber();

            //Act
            eImageType eImageType = variableRandomNumber.Image;

            //Assert
            Assert.AreEqual(eImageType.Random, eImageType, "RandomNumber Variable Image Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_TestImageType()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString();

            //Act
            eImageType eImageType = variableRandomString.Image;

            //Assert
            Assert.AreEqual(eImageType.Languages, eImageType, "RandomString Variable Image Type");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_Min5_Max10_Interval_1()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = 5,
                Max = 10,
                Interval = 1
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.IsTrue(decimal.Parse(variableRandomNumber.Value) >= 5, "vn.Value>=5");
            Assert.IsTrue(decimal.Parse(variableRandomNumber.Value) <= 10, "vn.Value<=10");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_Min10_Max100_Interval_5()
        {
            //We want to verify that the numbers we get are in interval of 5, can be 10,15,20 etc... but number like 17 is not valid

            int min = 10;
            int max = 100;
            int interval = 5;

            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = min,
                Max = max,
                Interval = interval
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num2 = decimal.Parse(variableRandomNumber.Value);
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num3 = decimal.Parse(variableRandomNumber.Value);

            //first verify all 3 numbers are in range
            Assert.IsTrue(num1 >= min && num1 <= max, "num1 >= " + min + " && num1 <= " + max);
            Assert.IsTrue(num2 >= min && num2 <= max, "num2 >= " + min + " && num2 <= " + max);
            Assert.IsTrue(num3 >= min && num3 <= max, "num3 >= " + min + " && num3 <= " + max);

            // Now check the validy 5,10,15 etc..
            //Verify that the num modolu 5 is give remainder of 0 or 5
            Assert.IsTrue(num1 % 5 is 0 or 5, "num1 % 5 == 0 || num1 % 5 == 5");
            Assert.IsTrue(num2 % 5 is 0 or 5, "num1 % 5 == 0 || num1 % 5 == 5");
            Assert.IsTrue(num3 % 5 is 0 or 5, "num1 % 5 == 0 || num1 % 5 == 5");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_Min500_Max100()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = 500,
                Max = 100,
                Interval = 40
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.AreEqual(variableRandomNumber.Value, "Error: Min > Max", "Error: Min > Max");
            Assert.AreEqual(variableRandomNumber.Formula, "Error: Min>Max", "variableRandomNumber.Formula = Error: Min>Max");
        }


        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_var500_Interval_40()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = 0,
                Max = 500,
                Interval = 40
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);

            //Verify numbers areof mutlipliers of 40 starting with 0
            Assert.IsTrue(num1 is >= 0 and <= 500, "num1>=0 && num1 <=500");
            Assert.AreEqual(num1 % 40, 0, "num1 % 40 = 0");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_Negative_Minus10_to_Minus_5()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = -10,
                Max = -5
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);

            //Verify numbers areof mutlipliers of 40 starting with 0
            Assert.IsTrue(num1 is >= (-10) and <= (-5), "num1>=-10 && num1 <=-5");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_LowerString15char()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Max = 15,
                IsLowerCase = true
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomString.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.AreEqual(variableRandomString.Value, variableRandomString.Value.ToLower(), "variableRandomString.Value, variableRandomString.Value.ToLower()");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_UpperString15char()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Max = 15,
                IsUpperCase = true
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomString.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.AreEqual(variableRandomString.Value.ToUpper(), variableRandomString.Value, "variableRandomString.Value, variableRandomString.Value.ToUpper()");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_BothupperlowerString15char_ShowError()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Max = 15,
                IsUpperCase = true,

                //Act
                //When the user change to lower case make sure the Isupper case get turned off
                IsLowerCase = true
            };
            string errorMsg = string.Empty;
            variableRandomString.GenerateAutoValue(ref errorMsg);

            //Assert
            Assert.AreEqual(false, variableRandomString.IsUpperCase, "variableRandomString.IsUpperCase=false");
            Assert.AreEqual(variableRandomString.Value.ToLower(), variableRandomString.Value);

            //TODO: verify the formula
            //Assert.AreEqual(variableRandomString.Formula, "Error", "variableRandomString.Formula=Error");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_0_10_chars()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Min = 0,
                Max = 10
            };
            bool Hit0 = false;
            bool Hit5 = false;
            bool Hit10 = false;

            //Act   
            Stopwatch stopwatch = Stopwatch.StartNew();
            // Run until we hit all numbers or 10 seconds
            while (!(Hit0 && Hit5 && Hit10) && stopwatch.ElapsedMilliseconds < 10000)
            {
                string errorMsg = string.Empty;
                variableRandomString.GenerateAutoValue(ref errorMsg);
                Assert.IsTrue(variableRandomString.Value.Length is >= 0 and <= 10, "variableRandomString.Value.Length >= 0 && variableRandomString.Value.Length <= 10");
                if (variableRandomString.Value.Length == 0)
                {
                    Hit0 = true;
                }

                if (variableRandomString.Value.Length == 5)
                {
                    Hit5 = true;
                }

                if (variableRandomString.Value.Length == 10)
                {
                    Hit10 = true;
                }
            }

            //Verify we hit the boundaries at least once
            Assert.IsTrue(Hit0, "Hit0");
            Assert.IsTrue(Hit5, "Hit5");
            Assert.IsTrue(Hit10, "Hit10");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_DigitsString15char()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Max = 15,
                IsDigit = true
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomString.GenerateAutoValue(ref errorMsg);

            //Assert
            string curValue = variableRandomString.Value;
            bool isDigits = curValue.All(c => char.IsDigit(c));
            Assert.IsTrue(isDigits, "String does not contain digits");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_BigNumbers_1111111111111__()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = 1111111111111,
                Max = 9999999999999
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);

            //Assert            
            Assert.IsTrue(num1 is >= 1111111111111 and <= 9999999999999, "num1 >= 1111111111111 && num1 <= 9999999999999");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_Integer_1_10_checkNoFractions()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = 1,
                Max = 10,
                IsInteger = true
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);

            //Assert            
            //Round should give back the same number if no fraction exist
            Assert.IsTrue(Math.Round(num1, 0) == num1, "Math.Round(num1,0) == num1");
        }


        [TestMethod]
        [Timeout(60000)]
        public void RandomNumberVar_With_Fractions()
        {
            //Arrange
            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = decimal.Parse("1.5"),   // not using M to mark it decimal, using parse since this is ehat happen from UI
                Max = decimal.Parse("1.7")
            };

            //Act
            string errorMsg = string.Empty;
            variableRandomNumber.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomNumber.Value);

            //Assert            
            Assert.IsTrue(num1 is >= 1.5M and <= 1.7M, "num1 >= 1.5M && num1 <= 1.7M");
        }


        [TestMethod]
        [Timeout(60000)]
        public void Random_Check_Hit_All_Numbers()
        {
            //Arrange
            // array to keep hits per number [0]-21, [1]-22 etc..
            int[] numbers = new int[20];

            VariableRandomNumber variableRandomNumber = new VariableRandomNumber
            {
                Min = decimal.Parse("21"),
                Max = decimal.Parse("40"),
                IsInteger = true
            };

            //Act

            Stopwatch stopwatch = Stopwatch.StartNew();
            // Run until we hit all numbers or 10 seconds
            while (!CheckHits(numbers) && stopwatch.ElapsedMilliseconds < 10000)
            {
                string errorMsg = string.Empty;
                variableRandomNumber.GenerateAutoValue(ref errorMsg);
                int num1 = int.Parse(variableRandomNumber.Value);
                numbers[num1 - 21]++;
            }

            //Assert        

            // We check that each number was hit            
            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(numbers[i] > 0, "Hit count a[i]>0 i=" + i);
            }
        }



        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_Digit_5_8()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Min = 5,
                Max = 8,
                IsDigit = true
            };
            //Act
            string errorMsg = string.Empty;
            variableRandomString.GenerateAutoValue(ref errorMsg);
            decimal num1 = decimal.Parse(variableRandomString.Value);

            //Assert                        
            Assert.IsTrue(variableRandomString.Value.Length is >= 5 and <= 8, "variableRandomString.Value.Length >= 5 && variableRandomString.Value.Length <= 8");
        }


        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_Digit_6_10_HitAllRange()
        {
            //Arrange
            int[] a = new int[5];
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Min = 6,
                Max = 10,
                IsDigit = true
            };

            //Act
            Stopwatch stopwatch = Stopwatch.StartNew();
            string errorMsg = string.Empty;
            while (stopwatch.ElapsedMilliseconds < 5000 && !CheckHits(a))   // max 5 seconds
            {
                variableRandomString.GenerateAutoValue(ref errorMsg);
                decimal num1 = decimal.Parse(variableRandomString.Value);
                Assert.IsTrue(variableRandomString.Value.Length is >= 6 and <= 10, "variableRandomString.Value.Length >= 6 && variableRandomString.Value.Length <= 10");
                a[variableRandomString.Value.Length - 6]++;
            }

            //Assert                                    
            Assert.IsTrue(CheckHits(a), "all items in array hit");
        }

        /// <summary>
        /// Check if all items in array > 0
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        private bool CheckHits(int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_0_5_Hit0()
        {
            //Arrange
            bool Hit0 = false;
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Min = 0,
                Max = 5
            };

            //Act
            string errorMsg = string.Empty;
            for (int i = 0; i < 100; i++)
            {
                variableRandomString.GenerateAutoValue(ref errorMsg);
                if (variableRandomString.Value.Length == 0)
                {
                    Hit0 = true;
                    break;
                }
            }

            //Assert
            Assert.IsTrue(Hit0, "Hit0");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RandomStringVar_Digit_13()
        {
            //Arrange
            VariableRandomString variableRandomString = new VariableRandomString
            {
                Min = 13,
                Max = 13,
                IsDigit = true
            };

            string errorMsg = string.Empty;
            for (int i = 0; i < 100; i++)
            {
                //Act
                variableRandomString.GenerateAutoValue(ref errorMsg);
                decimal num1 = decimal.Parse(variableRandomString.Value);

                //Assert            
                Assert.AreEqual(variableRandomString.Value.Length, 13, "variableRandomString.Value.Length=13");
                Assert.IsTrue(num1 is >= 0 and <= 9999999999999, "num1 >= 0 && num1 <= 9999999999999");
            }
        }


    }
}
