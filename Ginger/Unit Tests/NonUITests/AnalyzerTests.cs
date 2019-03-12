#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ginger.SolutionGeneral;
using Ginger.AnalyzerLib;
using System.Linq;

namespace UnitTests.NonUITests
{
    [TestClass]
    public class AnalyzerTests
    {
        private static BusinessFlow BF;
        private static Solution Solution;

        private static AnalyzeBusinessFlow ABF;

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
             Solution = new Solution();
            

             BF = new BusinessFlow();

            VariableString VarString = new VariableString();
            VarString.Name = "NewVarString";
            BF.Variables.Add(VarString);

            Activity Acty = new Activity();
            Acty.ActivityName = "Act1";

            BF.AddActivity(Acty);
            VariableString VarString2 = new VariableString();
            VarString2.Name = "NewVarString";
            Acty.AddVariable(VarString2);

            VariableString VarString3 = new VariableString();
            VarString3.Name = "NewVarString3";
            Acty.AddVariable(VarString3);

            VariableString VarString4 = new VariableString();
            VarString4.Name = "NewVarString";
            Acty.AddVariable(VarString4);

            VariableString VarString5 = new VariableString();
            VarString5.Name = "NewVarString";
            Acty.AddVariable(VarString5);


            VariableString VarString6 = new VariableString();
            VarString6.Name = "NewVarString";
            Acty.AddVariable(VarString6);

            ActDummy DummyAction = new ActDummy();

            Acty.Acts.Add(DummyAction);

            //ActReturnValue with static value
            ActReturnValue ARV1 = new ActReturnValue
            {
                Param = "Value1",
                Expected = "Test1"
            };
            DummyAction.ReturnValues.Add(ARV1);


            
            //ActReturnValue with static value and variable
           
            ActReturnValue ARV2 = new ActReturnValue
            {
                Param = "Value2",
                Expected = "Test1{Var Name=NewVarString}"
            };
            DummyAction.ReturnValues.Add(ARV2);


            //ActReturnValue with two variables
            ActReturnValue ARV3 = new ActReturnValue
            {
                Param = "Value3",
                Expected = "{Var Name=NewVarString3}{Var Name=NewVarString}"
            };
            DummyAction.ReturnValues.Add(ARV3);
            //ActReturnValue with  variables follwed by static value
            ActReturnValue ARV4 = new ActReturnValue
            {
                Param = "Value4",
                Expected = "{Var Name=NewVarString3}test"
            };
            DummyAction.ReturnValues.Add(ARV4);

            ActReturnValue ARV5 = new ActReturnValue
            {
                Param = "Value5",
                Expected = "{Var Name=NewVarString3}"
            };
            DummyAction.ReturnValues.Add(ARV5);

            ABF =  (AnalyzeBusinessFlow)AnalyzeBusinessFlow.Analyze(Solution, BF).Where(x=>x.Description.Equals(AnalyzeBusinessFlow.LegacyOutPutValidationDescription)).First();


        }

        [TestMethod]
        public void ValidateLegacyOutPutValidationInBusinessflow()
        {

            Assert.AreEqual(3, ABF.ReturnValues.Count);

        }

        [TestMethod]
        public void ValidateOutPutValidationPostFixeValues()
        {

            ABF.FixItHandler.Invoke(ABF, null);
            var ReturnValues = ABF.ReturnValues;

            ActReturnValue ARV1 = ReturnValues.Where(x => x.Param.Equals("Value1")).FirstOrDefault();
            Assert.AreEqual(Amdocs.Ginger.Common.Expressions.eOperator.Equals, ARV1.Operator);


            ActReturnValue ARV2 = ReturnValues.Where(x => x.Param.Equals("Value2")).FirstOrDefault();
            Assert.AreEqual(Amdocs.Ginger.Common.Expressions.eOperator.Legacy, ARV2.Operator);

            ActReturnValue ARV3 = ReturnValues.Where(x => x.Param.Equals("Value3")).FirstOrDefault();
            Assert.AreEqual(Amdocs.Ginger.Common.Expressions.eOperator.Legacy, ARV3.Operator);

            ActReturnValue ARV4 = ReturnValues.Where(x => x.Param.Equals("Value4")).FirstOrDefault();
            Assert.AreEqual(Amdocs.Ginger.Common.Expressions.eOperator.Legacy, ARV4.Operator);

            ActReturnValue ARV5 = ReturnValues.Where(x => x.Param.Equals("Value5")).FirstOrDefault();
            Assert.AreEqual(Amdocs.Ginger.Common.Expressions.eOperator.Equals, ARV5.Operator);

        }
    }
}
