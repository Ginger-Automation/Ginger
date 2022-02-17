#region License
/*
Copyright © 2014-2022 European Support Limited

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

namespace GingerCoreNETUnitTests.CommandProcessorLib
{
    [Level3]
    [TestClass]
    public class CommandProcessorTest
    {
        static CodeProcessor mCodeProcessor;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {
            mCodeProcessor = new CodeProcessor();
            // warmup CodeProcessor
            mCodeProcessor.EvalExpression("1+1");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
           
        }


        [TestMethod]
        public void Eval1Plus1()
        {
            //Arrange
            // string VE = "{CS Eval=2>1;}";
            string VE = "{CS Eval(1+1)}";

            //Act
            object result = mCodeProcessor.EvalExpression(VE);

            //assert
            Assert.AreEqual(result.GetType(), typeof(int));
            Assert.AreEqual(result, 2);
        }

        [TestMethod]
        public void Eval2Plus2()
        {
            //Arrange
            // string VE = "{CS Eval=2>1;}";
            string VE = "{CS Eval(2+2)}";

            //Act
            object result = mCodeProcessor.EvalExpression(VE);

            //assert
            Assert.AreEqual(result.GetType(), typeof(int));
            Assert.AreEqual(result, 4);
        }

        [TestMethod]
        public void EvalConditionTrue()
        {
            //Arrange            
            string condition = "1==1";

            //Act
            bool result = CodeProcessor.EvalCondition(condition);

            //assert            
            Assert.AreEqual(true, result, "result is true");
        }

        [TestMethod]
        public void EvalConditionWithCalc()
        {
            //Arrange            
            string condition = "1+2*3==7";

            //Act
            bool result = CodeProcessor.EvalCondition(condition);

            //assert            
            Assert.AreEqual(true, result, "result is true");
        }

        [TestMethod]
        public void EvalConditionFalse()
        {
            //Arrange            
            string condition = "1+1==3";

            //Act
            bool result = CodeProcessor.EvalCondition(condition);

            //assert            
            Assert.AreEqual(false, result, "result is false");
        }

        //[TestMethod]
        //// [ExpectedException(typeof(System.AggregateException))]
        //[ExpectedException(typeof(Microsoft.CodeAnalysis.Scripting.CompilationErrorException))]        
        //public void EvalConditionBadCondition()
        //{
        //    //Arrange            
        //    string condition = "1=1";   // will think it is assignemnt

        //    //Act
        //    bool result = mCodeProcessor.EvalCondition(condition);

        //    //assert            
        //    // None should throw before 
        //}


        
        [TestMethod]
        public void Eval21Equel1()
        {
            //Arrange            
            string VE = "if (1==1) return true; else return false;";

            //Act
            object result = mCodeProcessor.EvalExpression(VE);

            //assert
            Assert.AreEqual(result.GetType(), typeof(bool));
            Assert.AreEqual(true, result);
            
        }

        
        [TestMethod]
        public void RunCode()
        {
            //Arrange
            //string runCode = "{CS Run=int a=0; int b=2; result=a+b;}";
            // string runCode = "int a=0; int b=2; result=a+b;";
            string runCode = "result=123";

            //Act
            object result = mCodeProcessor.RunCode2(runCode);

            //assert
            Assert.AreEqual(result.GetType(), typeof(int));
            Assert.AreEqual(result, 123);

        }


        [TestMethod]
        public void RunCode2()
        {
            //Arrange            
            string runCode = "int a=1; int b=2; result=a+b;";

            //Act
            object result = mCodeProcessor.RunCode2(runCode);

            //assert
            Assert.AreEqual(result.GetType(), typeof(int));
            Assert.AreEqual(result, 3);

        }


        [TestMethod]
        public void RunCodeString()
        {
            //Arrange            
            string runCode = "string a=\"hello\";  string b=\"world\"; result=a+b; ";

            //Act
            object result = mCodeProcessor.RunCode2(runCode);

            //assert
            Assert.AreEqual(result.GetType(), typeof(string));
            Assert.AreEqual(result, "helloworld");
        }

    }
}
