#region License
/*
Copyright © 2014-2021 European Support Limited

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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GingerCore.Actions;
using GingerCore.Environments;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTests;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level3]
    public class ScriptTest 
    {

        [TestInitialize]
        public void TestInitialize()
        {
            
        }


        /// <summary>
        /// Running a free VBS command
        /// </summary>
        [TestMethod]  [Timeout(60000)]
        public void FreeCommandVBS()
        {
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            v.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            v.AddNewReturnParams = true;
            v.AddOrUpdateInputParamCalculatedValue("Free Command", "NumberB=10\r\nNumberA=20\r\nDim Result\r\nResult= int(NumberA) + int(NumberB)\r\nWscript.Echo \"Add=\" & Result");

            v.Execute();

            Assert.AreEqual(v.Error, null);
            Assert.AreEqual(v.ReturnValues[0].Actual.Contains("Add=30"), true);
        }

        /// <summary>
        /// Running a free VBS file with arguments
        /// </summary>
        [TestMethod]  [Timeout(60000)]
        public void RunScriptAPlusBVBS()
        {
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            v.AddNewReturnParams = true;
            v.ScriptCommand = ActScript.eScriptAct.Script;
            v.AddOrUpdateInputParamCalculatedValue("p1", "5");
            v.AddOrUpdateInputParamCalculatedValue("p2", "7");            

            v.ScriptName = "APlusB.vbs";
            v.ScriptPath = TestResources.GetTestResourcesFolder("");

            //Act
            v.Execute();

            //Assert
           Assert.AreEqual(v.Error , null);
           Assert.AreEqual(v.ReturnValues[0].Param, "TXT");
           Assert.AreEqual(v.ReturnValues[0].Actual, "5 + 7 = 12");

           Assert.AreEqual(v.ReturnValues[1].Param, "Total");
           Assert.AreEqual(v.ReturnValues[1].Actual, "12");

        }

        /// <summary>
        /// Running a free Batch command
        /// </summary>
        [TestMethod]  //[Timeout(60000)]
        public void FreeCommandBAT()
        {
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.BAT;
            v.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            v.AddNewReturnParams = true;
            v.AddOrUpdateInputParamCalculatedValue("Free Command", "@echo off \r\nSET /A a = 5 \r\nSET /A b = 10 \r\nSET /A c = %a% + %b% \r\necho Add=%c% ");

            //Act
            v.Execute();

            //Assert
            Assert.AreEqual(v.Error, null);
            Assert.AreEqual(v.ReturnValues[0].Actual.Contains("Add=15"), true);
        }

        /// <summary>
        /// Running a free Batch file with arguments
        /// </summary>
        [TestMethod]
        [Timeout(60000)]
        public void RunScriptAPlusBBAT()
        {
            // Arrange
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.BAT;
            v.AddNewReturnParams = true;
            v.ScriptCommand = ActScript.eScriptAct.Script;
            v.AddOrUpdateInputParamCalculatedValue("p1", "5");
            v.AddOrUpdateInputParamCalculatedValue("p2", "7");

            v.ScriptName = "BatchScriptWithArgs.bat";
            v.ScriptPath = TestResources.GetTestResourcesFolder("");

            //Act
            v.Execute();

            //Assert
            Assert.AreEqual(v.Error, null);

            Assert.AreEqual(v.ReturnValues[0].Param, "Result");
            Assert.AreEqual(v.ReturnValues[0].Actual, "12");

        }

    }
}
