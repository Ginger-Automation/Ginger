using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNETUnitTest.RunTestslib
{
    [TestClass]
    public class ActReturnValueTest
    {
        static ActDummy act;
        static ActReturnValue ARC;
        static GingerRunner mGingerRunner;
        static BusinessFlow BF;
        
        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            BF = new BusinessFlow("BF1");
            act = new ActDummy();
            ARC = new ActReturnValue();
            ARC.Param = "Test";
            ARC.Active = true;
            act.Active = true;
            act.AddNewReturnParams = true;
            mGingerRunner = new GingerRunner();
            BF.Activities[0].Acts.Add(act);
            mGingerRunner.BusinessFlows.Add(BF);
        }

        [Ignore]
        [TestMethod]
        public void Contains()
        {
            ARC.Actual = "Test Contains";
            ARC.Expected = "Test";            
            ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Contains;
            act.ActReturnValues.Add(ARC);                         
            mGingerRunner.RunRunner();
            Assert.AreEqual(act.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [Ignore]
        [TestMethod]
        public void DoesNotContains()
        {

        }

        [Ignore]
        [TestMethod]
        public void Equals()
        {

        }

        [Ignore]
        [TestMethod]
        public void Evaluate()
        {

        }

        [Ignore]
        [TestMethod]
        public void GreateThan()
        {

        }

        [Ignore]
        [TestMethod]
        public void GreaterThanEquals()
        {

        }

        [Ignore]
        [TestMethod]
        public void LessThan()
        {

        }

        [Ignore]
        [TestMethod]
        public void LessThanEquals()
        {

        }

        [Ignore]
        [TestMethod]
        public void NotEquals()
        {

        }

    }
}
