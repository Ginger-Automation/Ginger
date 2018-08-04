//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using GingerCoreNET.RosLynLib;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace GingerCoreNETUnitTests.ValueExpressionTest
//{
//    [TestClass]
//    public class ValueExpressionTest
//    {
//        [TestInitialize]
//        public void TestInitialize()
//        {

//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }


//        [TestMethod]
//        public void SimpleSumEval()
//        {
//            //Arrange            
//            string s = "1+2";

//            //Act
//            CodeProcessor SCT = new CodeProcessor();
//            string rc = SCT.EvalExpression(s);

//            //Assert
//            Assert.AreEqual("3", rc);
//        }


//        [TestMethod]
//        public void EvalDateTime()
//        {

//            string dt = DateTime.Now.ToString("M/d/y");
//            //Arrange            
//            string s = "using System;  DateTime.Now.ToString(\"M/d/y\")";

//            //Act
//            CodeProcessor SCT = new CodeProcessor();
//            string rc = SCT.EvalExpression(s);

//            //Assert
//            Assert.AreEqual(rc, dt);
//        }

//        [TestMethod]
//        public void SumForLoopl()
//        {
//            //Arrange            
//            string s = "int t=0;  for(int i=0;i<5;i++) { t+=i;}; t  ";

//            //Act
//            CodeProcessor SCT = new CodeProcessor();
//            string rc = SCT.EvalExpression(s);

//            //Assert
//            Assert.AreEqual("10", rc);
//        }


//    }
//}
