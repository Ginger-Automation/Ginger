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

using Amdocs.Ginger.Common.Enums;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;

namespace GingerCoreCommonTest.VariableTests
{
    [TestClass]
    [Level1]
    public class TimerVariableTests
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


        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestVariableType()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();

            //Act
            string varType = variableTimer.VariableType;

            //Assert
            Assert.AreEqual("Timer", varType, "Timer Variable Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestVariableUIType()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();

            //Act
            string varType = variableTimer.VariableUIType;

            //Assert
            Assert.IsTrue(varType.Contains("Timer"), "Timer Variable UI Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestImageType()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();

            //Act
            eImageType eImageType = variableTimer.Image;

            //Assert
            Assert.AreEqual(eImageType.Timer, eImageType, "Image Type");
        }

        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestTimerInMS()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();
            variableTimer.TimerUnit = VariableTimer.eTimerUnit.MilliSeconds;

            //Act
            variableTimer.StartTimer();
            Thread.Sleep(150); //wait for 150ms 

            //Assert
            string restVal = variableTimer.Value;
            Assert.IsNotNull(restVal);
        }


        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestTimerInSec()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();
            variableTimer.TimerUnit = VariableTimer.eTimerUnit.Seconds;

            //Act
            variableTimer.StartTimer();
            Thread.Sleep(2000); //wait for 2 seconds

            //Assert
            string restVal = variableTimer.Value;
            Assert.IsNotNull(restVal);
        }

        [TestMethod]  [Timeout(60000)]
        public void TimerVar_TestTimerInMin()
        {
            //Arrange
            VariableTimer variableTimer = new VariableTimer();
            variableTimer.TimerUnit = VariableTimer.eTimerUnit.Minutes;

            //Act
            variableTimer.StartTimer();
            Thread.Sleep(1); //wait for 1 ms

            //Assert
            string restVal = variableTimer.Value;
            Assert.IsNotNull(restVal);
        }

    }
}
