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

using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [TestClass]
    public class ActReturnValueTest
    {
        static ActDummy act;
        static ActReturnValue ARC;
        static GingerRunner mGingerRunner;
        static BusinessFlow BF;
        static RunSetConfig runSetConfig;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            BF = new BusinessFlow("BF1");
            act = new ActDummy();
            ARC = new ActReturnValue
            {
                Param = "Test",
                Active = true
            };
            act.Active = true;
            act.AddNewReturnParams = true;
            act.SupportSimulation = true;
            mGingerRunner = new GingerRunner();
            mGingerRunner.Executor = new GingerExecutionEngine(mGingerRunner);

            mGingerRunner.RunInSimulationMode = true;
            BF.Activities[0].Acts.Add(act);

            mGingerRunner.Executor.BusinessFlows.Add(BF);

        }

        [TestMethod]
        public void OutputValueContainsTest()
        {
            ARC.SimulatedActual = "Test Contains";
            ARC.Expected = "Test";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Contains;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueDoesNotContainsTest()
        {
            ARC.SimulatedActual = "Test Contains";
            ARC.Expected = "ABC";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.DoesNotContains;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueEqualsTest()
        {
            ARC.SimulatedActual = "Test";
            ARC.Expected = "Test";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueEvaluateTest()
        {
            ARC.SimulatedActual = "Test";
            ARC.Expected = "{CS Exp=@\"Test\".Contains(@\"Test\")}";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Evaluate;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueGreateThanTest()
        {
            ARC.SimulatedActual = "10";
            ARC.Expected = "9";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.GreaterThan;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueGreaterThanEqualsTest()
        {
            ARC.SimulatedActual = "10";
            ARC.Expected = "10";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.GreaterThanEquals;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueLessThanTest()
        {
            ARC.SimulatedActual = "9";
            ARC.Expected = "10";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.LessThan;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueLessThanEqualsTest()
        {
            ARC.SimulatedActual = "10";
            ARC.Expected = "10";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.LessThanEquals;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        public void OutputValueNotEqualsTest()
        {
            ARC.SimulatedActual = "10";
            ARC.Expected = "9";
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.NotEquals;
            act.ActReturnValues.Add(ARC);
            mGingerRunner.Executor.RunRunner();
            Assert.AreEqual(ARC.Status, ActReturnValue.eStatus.Passed);
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

    }
}
