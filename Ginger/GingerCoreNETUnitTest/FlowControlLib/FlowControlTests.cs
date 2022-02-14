using Ginger.Run;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Amdocs.Ginger.CoreNET.Execution;
using GingerTestHelper;

namespace GingerCore.FlowControlLib.Tests
{
    [TestClass]
    [Level1]
    public class FlowControlTests
    {

        [TestMethod]
        [Timeout(60000)]
        public void FLowControlStatusCalculationTest()
        {
            FlowControl FC = new FlowControl();
            FC.Operator = eFCOperator.ActionPassed;


            Activity LastActivity = new Activity();
            ActDummy act = new ActDummy();

            act.Status = eRunStatus.Passed;
            bool FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity,null, FC.Operator, "");
            Assert.AreEqual(true, FcStatus);
            act.Status = eRunStatus.Failed;
            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(false, FcStatus);




            FC.Operator = eFCOperator.ActionFailed;

            act.Status = eRunStatus.Failed;
            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(true, FcStatus);

            act.Status = eRunStatus.Passed;
            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(false, FcStatus);





            FC.Operator = eFCOperator.LastActivityPassed;
            LastActivity.Status = eRunStatus.Passed;

            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(true, FcStatus);
            LastActivity.Status = eRunStatus.Failed;
            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(false, FcStatus);




            FC.Operator = eFCOperator.LastActivityFailed;
            LastActivity.Status = eRunStatus.Failed;

            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(true, FcStatus);
            LastActivity.Status = eRunStatus.Passed;

            FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
            Assert.AreEqual(false, FcStatus);



        }
    }
}
