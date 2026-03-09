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

using GingerCore;
using GingerCore.Actions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GingerCoreNETUnitTest.LinuxTransformationTests
{
    [TestClass]
    public class ActValidationTests
    {
        [TestMethod]
        [Timeout(60000)]
        public void ConditionEqualCSTest()
        {
            //Arrange            
            ActValidation actValidation = new ActValidation();
            actValidation.ValueExpression = new ValueExpression(actValidation, "");

            //Act
            actValidation.CalcEngineType = ActValidation.eCalcEngineType.CS;
            actValidation.Condition = "4==4";
            actValidation.Execute();

            //Assert
            Assert.AreEqual(actValidation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed);
        }

        [TestMethod]
        [Timeout(60000)]
        public void ConditionFalseEqualCSTest()
        {
            //Arrange            
            ActValidation actValidation = new ActValidation();
            actValidation.ValueExpression = new ValueExpression(actValidation, "");

            //Act
            actValidation.CalcEngineType = ActValidation.eCalcEngineType.CS;
            actValidation.Condition = "4=4";
            actValidation.Execute();

            //Assert
            Assert.AreEqual(actValidation.Status, Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
            Assert.IsTrue(actValidation.ExInfo.Contains("error"));
        }
    }
}
