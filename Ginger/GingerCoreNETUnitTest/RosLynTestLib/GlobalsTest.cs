#region License
/*
Copyright © 2014-2025 European Support Limited

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

using GingerCoreNET.RosLynLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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


        //[TestMethod]  [Timeout(60000)]
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

        [TestMethod]
        public void TestBogusData_AddressCountry_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Address(@\"en\").Country();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_AddressfullAddress_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Address(@\"en\").FullAddress();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_AddressCountryCode_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Address(@\"en\").CountryCode();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchPastDateOnly_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").PastDateOnly(1);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchFutureDateOnly_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").FutureDateOnly(1);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchBetweenDateOnly_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").BetweenDateOnly(PastDateOnly(1),FutureDateOnly(1));}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchPastDate_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").Past(1);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchFutureDate_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").Future(1);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchBetweenDate_IsNotnullAndEmpty()
        {
            DateTime startdate = DateTime.Now.AddYears(-1);
            DateTime enddate = DateTime.Now.AddYears(1);
            string Expression = "{MockDataExp Fun=Date(@\"en\").Between(Past(1),Future(1));}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchMonth_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").Month();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void PastDate_FetchWeekday_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Date(@\"en\").Weekday();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }


        [TestMethod]
        public void TestBogusData_FinanceFinanceAccountNumber_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().Account();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceTransactionType_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().TransactionType();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceCreditCardNumber_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().CreditCardNumber();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceCreditCardCvv_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().CreditCardCvv();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceBIC_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().Bic();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceIBAN_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().Iban();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceCurrencyCode_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().Currency().Code;}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_FinanceCurrencySymbol_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Finance().Currency().Symbol;}";
            string error = string.Empty;
            var output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null);
        }

        [TestMethod]
        public void TestBogusData_InternetEmail_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Internet(@\"en\").Email();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_InternetUserName_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Internet(@\"en\").UserName();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_InternetPassword_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Internet(@\"en\").Password();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_NameFirstName_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Name(@\"en\").FirstName();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_NameLastName_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Name(@\"en\").LastName();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_NameFullName_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Name(@\"en\").FullName();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_PhoneNumbersPhoneNumbers_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=PhoneNumbers(@\"en\").PhoneNumber();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_RandomizerNumber_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Randomizer().Number(1,10);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty) && Regex.IsMatch(output, @"\d"));
        }

        [TestMethod]
        public void TestBogusData_RandomizerDigits_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Randomizer().Digits(3,0,9);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty) && output.Split(",").Select(i => i.Trim()).All(x => Regex.IsMatch(x, @"\d")));
        }

        [TestMethod]
        public void TestBogusData_RandomizerDecimal_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Randomizer().Decimal();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty) && Regex.IsMatch(output, "^[+-]?(\\d*\\.)?\\d+$"));
        }

        [TestMethod]
        public void TestBogusData_RandomizerString_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Randomizer().String();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_RandomizerAlphaNumeric_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Randomizer().AlphaNumeric(3);}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_complexExpression()
        {
            string Expression = "{MockDataExp Fun=Database().Column();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_WrongExpression()
        {
            string Expression = "{MockDataExp Fun=Randomizer().;}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && output.Equals(string.Empty));
        }

        [TestMethod]
        public void TestBogusData_MultipleExpression_IsNotnullAndEmpty()
        {
            string Expression = "{MockDataExp Fun=Address(@\"en\").FullAddress();}\r\n{MockDataExp Fun=Address(@\"en\").Country();}";
            string error = string.Empty;
            string output = CodeProcessor.GetBogusDataGenerateresult(Expression);
            Assert.IsTrue(output != null && !output.Equals(string.Empty));
        }
    }
}
