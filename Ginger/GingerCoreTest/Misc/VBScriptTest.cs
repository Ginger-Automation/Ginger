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
    public class VBScript 
    {

        [TestInitialize]
        public void TestInitialize()
        {
            
        }


        /// <summary>
        /// Running a free VBS command
        /// </summary>
        [TestMethod]  [Timeout(60000)]
        public void FreeCommand()
        {
            ActScript v = new ActScript();
            v.ScriptInterpreterType = ActScript.eScriptInterpreterType.VBS;
            v.ScriptCommand = ActScript.eScriptAct.FreeCommand;
            v.AddOrUpdateInputParamValue("Free Command","Wscript.echo \"Hello\"");

            v.Execute();

            //Assert.AreEqual(v.ReturnValues[0].Actual, "5");

        }


        
        [TestMethod]  [Timeout(60000)]
        public void RunScriptAPlusB()
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



    }
}
