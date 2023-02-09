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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class ValueExpressionTest 
    {
        BusinessFlow mBF;
        ProjEnvironment mEnv;

        string v1Value = "Val1";
        string v2Value = "Val2";
        string v3Value = "Your new brightspot Mobile Phone number is: 404-200-3848";
        string v4Value = "Welcome! Your new phone number is (555) 555-5555";
        [TestInitialize]
        public void TestInitialize()
        {
            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();
            mEnv = new ProjEnvironment();
            EnvApplication app1 = new EnvApplication();
            app1.Name = "App1";
            app1.Url = "URL123";
            mEnv.Applications.Add(app1);
            Database db1 = new Database();
            db1.Name = "DB1";
            app1.Dbs.Add(db1);
            GeneralParam GP1 = new GeneralParam();
            GP1.Name = "GP1";
            GP1.Value = "GP1Value";
            app1.GeneralParams.Add(GP1);

            mBF = new BusinessFlow();

            VariableString v1 = new VariableString();
            v1.Name = "v1";
            v1.Value = v1Value;
            //mBF.Variables.Add(v1);
            mBF.AddVariable(v1);

            VariableString v2 = new VariableString();
            v2.Name = "v2";
            v2.Value = v2Value;
            //mBF.Variables.Add(v2);
            mBF.AddVariable(v2);

            VariableString v3 = new VariableString();
            v3.Name = "v3";
            v3.Value = v3Value;
            //mBF.Variables.Add(v3);
            mBF.AddVariable(v3);

            VariableString v4 = new VariableString();
            v4.Name = "v4";
            v4.Value = v4Value;
            //mBF.Variables.Add(v4);
            mBF.AddVariable(v4);

            VariableSelectionList v5 = new VariableSelectionList();
            v5.Name = "v5";
            v5.OptionalValuesList.Add(new OptionalValue("value1"));
            v5.OptionalValuesList.Add(new OptionalValue("value2"));
            v5.Value = v5.OptionalValuesList[0].Value;
            //mBF.Variables.Add(v4);
            mBF.AddVariable(v5);

            VariableString v6 = new VariableString();
            v6.Name = "v6 ";
            v6.Value = "OK";
            //mBF.Variables.Add(v1);
            mBF.AddVariable(v6);
        }

        [TestMethod]  [Timeout(60000)]
        public void SimpleString()
        {
            //Arrange            
            string s = "Simple string";

            ValueExpression VE = new ValueExpression(mEnv, mBF);            
            VE.Value = s;

            //Act            
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, s);
        }


        [TestMethod]
        [Timeout(60000)]
        public void SimpleStringWithSpace()
        {
            //Arrange            
            string s = "  Simple string  ";

            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = s;

            //Act            
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, s);
        }

        [TestMethod]  [Timeout(60000)]
        public void GetVarV1()
        {
            //Arrange            
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Var Name=v1}";

            //Act            
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, v1Value);
        }

        [TestMethod]  [Timeout(60000)]
        public void GetVBSNow()
        {
            //Arrange            
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            //2 = vbShortDate
            VE.Value = "{VBS Eval=FormatDateTime(NOW(),2)}";

            //Act            
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, DateTime.Now.ToString("M/d/yyyy"));
        }

        [TestMethod]  [Timeout(60000)]
        public void GetNowPlus1()
        {
            //Arrange            
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{VBS Eval=DATE()+1}";

            //Act            
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, DateTime.Now.AddDays(1).ToString("M/d/yyyy"));
        }

        //[Ignore]// sometime fails due to timings
        //[TestMethod]  [Timeout(60000)]  
        //public void GetVarNowMinus1()
        //{
        //    //Arrange            
        //    ValueExpression VE = new ValueExpression(mEnv, mBF);
        //    VE.Value = "{VBS Eval=NOW()-1}";

        //    //Act            
        //    string v = VE.ValueCalculated;

        //    //Assert
        //   Assert.AreEqual(v, DateTime.Now.AddDays(-1).ToString());
        //}
        // [TestMethod]  [Timeout(60000)]
        //public void GetVarNowNeg()
        //{
        //    ValueExpression VE = new ValueExpression(mEnv, mBF);
        //    VE.Value = "{Var Name=NOW()jdlkas}";
        //    string v = VE.ValueCalculated;

        //   Assert.AreEqual(v, DateTime.Now.ToString());
        //}
        //[TestMethod]  [Timeout(60000)]
        //public void GetVarNowFormat1()
        //{
        //    ValueExpression VE = new ValueExpression(mEnv, mBF);
        //    VE.Value = "{Var Name=NOW(MM/dd/yyyy HH:mm:ss)}";
        //    string v = VE.ValueCalculated;

        //   Assert.AreEqual(v, DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        //}
        //[TestMethod]  [Timeout(60000)]
        //public void GetVarNowFormatP5()
        //{
        //    ValueExpression VE = new ValueExpression(mEnv, mBF);
        //    VE.Value = "{Var Name=NOW(MM/dd/yyyy HH:mm:ss)+3}";
        //    string v = VE.ValueCalculated;

        //   Assert.AreEqual(v, DateTime.Now.AddDays(+3).ToString("MM/dd/yyyy HH:mm:ss"));
        //}
        //[TestMethod]  [Timeout(60000)]
        //public void GetVarNowFormatM5()
        //{
        //    ValueExpression VE = new ValueExpression(mEnv, mBF);
        //    VE.Value = "{Var Name=NOW(MM/dd/yyyy HH:mm:ss)-3}";
        //    string v = VE.ValueCalculated;

        //   Assert.AreEqual(v, DateTime.Now.AddDays(-3).ToString("MM/dd/yyyy HH:mm:ss"));
        //}
        [TestMethod]  [Timeout(60000)]
        public void VarV1WithPrefix()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "ABC {Var Name=v1}";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "ABC " + v1Value);
        }

        [TestMethod]  [Timeout(60000)]
        public void VarV1WithPostfix()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Var Name=v1} ABC";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, v1Value + " ABC");
        }

        [TestMethod]  [Timeout(60000)]
        public void VarV1WithPreAndPostfix()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "ABC {Var Name=v1} DEF";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "ABC " + v1Value + " DEF");
        }

        [TestMethod]  [Timeout(60000)]
        public void VarV1WithPreAndPostfixUsedMany()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "ABC {Var Name=v1} DEF {Var Name=v1} GHI";

            //Act     
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "ABC " + v1Value + " DEF " + v1Value + " GHI");
        }

        [TestMethod]  [Timeout(60000)]
        public void MultiVars()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "ABC {Var Name=v1} DEF {Var Name=v2} GHI";

            //Act
            string v = VE.ValueCalculated;

           Assert.AreEqual(v, "ABC " + v1Value + " DEF " + v2Value + " GHI");
        }

        [TestMethod]  [Timeout(60000)]
        public void MultiVarsCompact()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "A{Var Name=v1}B{Var Name=v2}C";

            //Act
            string v = VE.ValueCalculated;

           Assert.AreEqual(v, "A" + v1Value + "B" + v2Value + "C");
        }

        [TestMethod]  [Timeout(60000)]
        public void GetVarNotExistWillFail()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{Var Name=v99}";

            //Act
            string v = VE.ValueCalculated;

            //due to Dicser Variable can eb soemthing else
            //TODO: make dicser working for UT - load the dict
            Assert.IsTrue(v.Contains("ERROR: The Variable Name=v99 was not found"), "ERROR: The Variable Name=v99 was not found");
        }

        //[TestMethod]  [Timeout(60000)]
        //public void GetVarV1x10000_LowerThan100ms()
        //{
        //    //first time valueexpression will take more time to setup regex operations
        //    ValueExpression VES = new ValueExpression(mEnv, mBF);
        //    VES.Value = "{Var Name=v1}";
        //    string vS = VES.ValueCalculated;
            
        //    //Arrange
        //    Stopwatch ST = new Stopwatch();
        //    ST.Reset();
        //    ST.Start();

        //    //Act
        //    //TODO: measure that it is less then 30ms
        //    for (int i = 0; i < 10000; i++)
        //    {
        //        ValueExpression VE = new ValueExpression(mEnv, mBF);
        //        VE.Value = "{Var Name=v1}";
        //        string v = VE.ValueCalculated;
                
        //    }
        //    ST.Stop();
            
        //    //Assert
        //        Assert.IsTrue(ST.ElapsedMilliseconds < 100);
        //}


        [TestMethod]  [Timeout(60000)]
        public void EnvParam()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{EnvParam App=App1 Param=GP1}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "GP1Value");

        }

        [TestMethod]  [Timeout(60000)]
        public void EnvAppURL()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{EnvURL App=App1}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "URL123");

        }
        [TestMethod]  [Timeout(60000)]
        public void RegMatch()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=IsMatch Pat=\d{3}-\d{3}-\d{4} P1=212-555-6666}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "True");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegMatchNeg()
        {


            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=IsMatch Pat=\d{3}-\d{3}-\d{4} P1=212-555-666}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "False");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegReplacesSurfix()
        {            
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\d{3}-\d{3}-\d{4} P1=212-555-6666any string P2=new string}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "new stringany string");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegReplacesPrefix()
        {
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\d{3}-\d{3}-\d{4} P1=any string212-555-6666 P2=new string}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "any stringnew string");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegReplacesMulti()
        {
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\d{3}-\d{3}-\d{4} P1=any str212-555-6666ing212-555-6666 P2=}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "any string");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegReplacesNeg()
        {
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\d{3}-\d{3}-\d{4} P1=212-555-6666any string}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "any string");
        }

        [TestMethod]  [Timeout(60000)]
        public void RegGroup0()
        {
            //Arrange     
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=0 Pat=(\d{3})-(\d{3}-\d{4}) P1=212-555-6666ing212-555-6666}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "212-555-6666");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup1()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
         //   VE.Value = @"{RegEx Fun=1 Pat=(\d{3})-(\d{3}-\d{4}) P1=212-555-6666ing212-555-6666}";
            VE.Value = @"{RegEx Fun=1 Pat=access code is (\w{8}) P1=Your en.mi.univisionmobile.com access code is 4gT3hDEh. This code is case sensitive and expires in 72 hours. }";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "4gT3hDEh");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup2()
        {
            //Arrange  
        //    string s = "Simple string";

            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=2 Pat=(\d{3})-(\d{3}-\d{4}) P1=das212-555-6666ing}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "555-6666");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup3()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=3 Pat=(\d{3})(-)(\d{3})(-\d)(\d{3}) P1=fsd212-555-6666ing}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "555");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup4()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=4 Pat=(\d{3})(-)(\d{3})(-\d)(\d{3}) P1=sdfds212-555-6666ing}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "-6");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup5()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=5 Pat=(\d{3})(-)(\d{3})(-\d)(\d{2})(\d) P1=sdfds212-555-6666ing}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "66");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroup6()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=6 Pat=(\d{3})(-)(\d{3})(-\d)(\d{2})(\d) P1=sdfds212-555-6666ing}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "6");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroupTMO()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"In{RegEx Fun=0 Pat=(\d{3}-\d{3}-\d{4}) P1={Var Name=v3}}dex";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "In404-200-3848dex");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegQueryTMO()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"EN:UVM:HOM$";

            //Act
            string v = VE.ValueCalculated;
            Regex re = new Regex(v);
           Assert.AreEqual(re.IsMatch("EN:UVM:HOME"), false);


        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroupTMO2()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"In{RegEx Fun=Replace Pat=\D+:\s P1={Var Name=v3}}dex";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "In404-200-3848dex");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroupTMO5()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\D+ P1={Var Name=v4}}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "5555555555");
        }
        [TestMethod]  [Timeout(60000)]
        public void RegGroupTMO3()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=Replace Pat=\D+:\s P1={Var Name=v3}}{Var Name=v3}{Var Name=v3}{Var Name=v3}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848");
        }

        [TestMethod]  [Timeout(60000)]
        public void RegGroupTMO4()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"In{RegEx Fun=Replace Pat=\D+:\s P1={Var Name=v3}}{Var Name=v3}{Var Name=v3}{Var Name=v3}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "In404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848Your new brightspot Mobile Phone number is: 404-200-3848");
        }

        [TestMethod]
        [Timeout(60000)]
        public void RegExtractLastChars()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=  1 Pat=.+([\d\D]{4})$ P1=abcdefghijkl}";
            //Act
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "ijkl");
        }
        [TestMethod]
        [Timeout(60000)]
        public void RegExtractLastCharsWithNewLine()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{RegEx Fun=  1 Pat=.+([\d\D]{4})$ P1=abcdef" + Environment.NewLine + "ghijkl}";
            //Act
            string v = VE.ValueCalculated;

            //Assert
            Assert.AreEqual(v, "ijkl");
        }
        [TestMethod]  [Timeout(60000)]
        public void VBS_2_Plus_2()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{VBS Eval=2+2*5}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "12");
        }

        [TestMethod]  [Timeout(60000)]
        public void VBS_Substring_MID()
        {
            //Arrange  
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{VBS Eval=mid(\"hello\",1,2)}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "he");
        }

        [TestMethod]  [Timeout(60000)]
        public void VBS_CalcWithVars()
        {
            //Arrange
            //mBF.Variables.Add(new VariableString() { Name= "Param1", Value="5"});
            //mBF.Variables.Add(new VariableString() { Name = "Param2", Value = "3" });
            mBF.AddVariable(new VariableString() { Name= "Param1", Value="5"});
            mBF.AddVariable(new VariableString() { Name = "Param2", Value = "3" });
            
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{VBS Eval={Var Name=Param1} + {Var Name=Param2}}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, "8");
        }
        [TestMethod]  [Timeout(60000)]
        public void VBS_CalwithVBSSpecialfolder()
        {
            //Arrange
          
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{VBS Eval=WScript.CreateObject(\"WScript.Shell\").SpecialFolders(\"Desktop\")}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           Assert.AreEqual(v, Environment.GetFolderPath(Environment.SpecialFolder.Desktop));
        }
        
        [TestMethod]  [Timeout(60000)]
        public void VBS_NOW()
        {
            //Arrange
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = "{VBS Eval=DATE()}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
            string date = DateTime.Now.ToString("M/d/yyyy");
           Assert.AreEqual(v, date);
        }


        [TestMethod]  [Timeout(60000)]
        public void VBS_Split1()
        {
            //Arrange
            //mBF.Variables.Add(new VariableString() { Name = "Param555", Value = "555-555-5555" + Environment.NewLine + "None " });
            mBF.AddVariable(new VariableString() { Name = "Param555", Value = "555-555-5555" + Environment.NewLine + "None " });

            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{VBS Eval=Split(""{Var Name=Param555}"",""vbCrLf"")(0)}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
           // string date = DateTime.Now.ToString("b");
           Assert.AreEqual(v, "555-555-5555");
        }
        [TestMethod]  [Timeout(60000)]
        public void VBS_SpecialEnvVariable()
        {
            //Arrange

            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{VBS Eval=WScript.CreateObject(""WScript.Network"").UserName}";

            //Act
            string v = VE.ValueCalculated;

            //Assert
            // string date = DateTime.Now.ToString("b");
           Assert.AreEqual(v, System.Environment.UserName);
        }
 [TestMethod]  [Timeout(60000)]
        public void VBS_Date2Epoch()
        {
            //Arrange
           
            ValueExpression VE = new ValueExpression(mEnv, mBF);
            VE.Value = @"{VBS Eval=DateDiff(""s"", ""12/31/1969 20:00:00"", date())}";

            //Act
            string v = VE.ValueCalculated;
         //  DateTime dt= new DateTime(1969, 12, 31, 20, 0, 0, DateTimeKind.Utc);
            //Assert
           // string date = DateTime.Now.ToString("b");
           //Assert.AreEqual(v, dt.);
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListWithIndexBasedAccess()
        {
            //Arrange

            ValueExpression VE1 = new ValueExpression(mEnv, mBF);
            VE1.Value = @"{Var Name=v5, Index=1}";

            ValueExpression VE2 = new ValueExpression(mEnv, mBF);
            VE2.Value = @"{Var Name=v5, Index=2}";

            //Act
            string v1 = VE1.ValueCalculated;
            string v2 = VE2.ValueCalculated;

            //Assert
            Assert.AreEqual(v1, "value1");
            Assert.AreEqual(v2, "value2");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SelectionListGetLength()
        {
            //Arrange

            ValueExpression VE1 = new ValueExpression(mEnv, mBF);
            VE1.Value = @"{Var Name=v5, GetLength=True}";

            ValueExpression VE2 = new ValueExpression(mEnv, mBF);
            VE2.Value = @"{Var Name=v5, GetLength=False}";

            //Act
            string v1 = VE1.ValueCalculated;
            string v2 = VE2.ValueCalculated;

            //Assert
            Assert.AreEqual(v1, "2");
            Assert.AreEqual(v2, "value1");
        }

        [TestMethod]
        [Timeout(60000)]
        public void IdentifyVaraibleNameWithSpaces()
        {
            //Arrange

            ValueExpression VE1 = new ValueExpression(mEnv, mBF);
            VE1.Value = @"{Var Name=v6 }";

            //Act
            string v1 = VE1.ValueCalculated;

            //Assert
            Assert.AreEqual(v1, "OK");
        }
    }
}
