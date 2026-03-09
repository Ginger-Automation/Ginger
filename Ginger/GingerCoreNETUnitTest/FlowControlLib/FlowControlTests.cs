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

using Amdocs.Ginger.CoreNET.Execution;
using Ginger.Run;
using GingerCore.Actions;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            FlowControl FC = new FlowControl
            {
                Operator = eFCOperator.ActionPassed
            };


            Activity LastActivity = new Activity();
            ActDummy act = new ActDummy
            {
                Status = eRunStatus.Passed
            };
            bool FcStatus = GingerExecutionEngine.CalculateFlowControlStatus(act, LastActivity, null, FC.Operator, "");
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
