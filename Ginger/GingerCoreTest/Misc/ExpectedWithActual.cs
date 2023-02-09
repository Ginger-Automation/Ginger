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

using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTests.NonUITests
{
    // This class will run only on windows since it uses VBS

    [TestClass]
    [Level1]
    public class ExpectedWithActual 
    {
        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedIsSimpleNumber()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "123";
            ARC.Expected = "123";

            //Act
            CalculateARC(ARC);
            
            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated, "123");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Passed);
        }

        private void CalculateARC(ActReturnValue ARC)
        {
            // We do only  simple calculation not including BF or proj env params so it will work 
            ARC.ExpectedCalculated = ARC.Expected;
            GingerExecutionEngine.ReplaceActualPlaceHolder(ARC);
            GingerExecutionEngine.CalculateARCStatus(ARC);
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedIsSimpleString()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "ABC";
            ARC.Expected = "ABC";

            //Act
            CalculateARC(ARC);

            //Assert            
           Assert.AreEqual(ARC.ExpectedCalculated, "ABC");
           Assert.AreEqual(ARC.Status,ActReturnValue.eStatus.Passed);
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedActualGreaterThanNumber()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "5";
            ARC.Expected = "{Actual} > 0";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated , "'5 > 0' is True");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Passed);            
        }


        [TestMethod]  [Timeout(60000)]
        public void CheckforStartingwithNot()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "Not Tough";
            ARC.Expected = "Not Tough";

            //Act
            CalculateARC(ARC);

            //Assert
           
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedActualLowerThanNumber()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "5";
            ARC.Expected = "{Actual} < 10";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated , "'5 < 10' is True");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Passed);            
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedActualLowerThanNumberIsFalse()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "11";
            ARC.Expected = "{Actual} < 10";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated , "'11 < 10' is False");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Failed);            
        }


        [TestMethod]  [Timeout(60000)]
        public void ExpectedActulContainsString()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "Hello World!";
            ARC.Expected = "InStr({Actual},\"Hello\")>0";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated, "'InStr(\"Hello World!\",\"Hello\")>0' is True");
           Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);            
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedActulContainsStringIsFalse()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "ABCDE";
            ARC.Expected = "InStr({Actual},\"ZZZ\") > 0";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated , "'InStr(\"ABCDE\",\"ZZZ\") > 0' is False");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Failed);            
        }


        [TestMethod]  [Timeout(60000)]
        public void ExpectedActulWithOrNumbers()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "7";
            ARC.Expected = "{Actual}=3 or {Actual}=7";

            //Act
            CalculateARC(ARC);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated , "'7=3 or 7=7' is True");
           Assert.AreEqual(ARC.Status , ActReturnValue.eStatus.Passed);    
        }

        [TestMethod]  [Timeout(60000)]
        public void ExpectedActulWithOrNumbersANDCond()
        {
            //Arrange
            ActReturnValue ARC = new ActReturnValue();
            ARC.Actual = "5";
            ARC.Expected = "{Actual}=1 or {Actual}=5";

            //Act
            CalculateARC(ARC);

            string Exp = ARC.Expected.Replace("{Actual}", ARC.Actual);

            //Assert
           Assert.AreEqual(ARC.ExpectedCalculated, "'" + Exp + "' is True");
           Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
        }

    }
}
