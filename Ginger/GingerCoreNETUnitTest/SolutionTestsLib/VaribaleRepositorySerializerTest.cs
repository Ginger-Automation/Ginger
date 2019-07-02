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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Variables;
using GingerCoreNETUnitTest.RunTestslib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTests.SolutionTestsLib
{

    [Level1]
    [TestClass]
    public class VaribaleRepositorySerializerTest
    {
        public static NewRepositorySerializer RS = new NewRepositorySerializer();
        public static BusinessFlow BusinessFlow = new BusinessFlow();
        static SolutionRepository SR;
        static ObservableList<VariableBase> varList = new ObservableList<VariableBase>();
        VariableBase var = null;

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            NewRepositorySerializer.AddClassesFromAssembly(typeof(BusinessFlow).Assembly);
            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "Variables" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Business Flow 1.Ginger.BusinessFlow.xml");

            BusinessFlow = (BusinessFlow)RS.DeserializeFromFile(FileName);
            varList = BusinessFlow.GetVariables();
        }

        [TestMethod]
        public void VariablesCount()
        {
            // Assert   
            Assert.AreEqual(8, BusinessFlow.Variables.Count, "Variable Count");
        }

        [TestMethod]
        public void VariablePassword()
        {
            //Act
            var = BusinessFlow.GetVariable("TestPasswordVar");
            //Assert
            Assert.AreEqual("+S+B0OE+KDopqrquyaOF2Q==", var.Value, "var string");
        }

        [TestMethod]
        public void VariableDynamic()
        {
            //Act
            var = BusinessFlow.GetVariable("TestDynamicVar");
            //Assert
            Assert.AreEqual("12", var.Formula, "var dynamic");
        }
        [TestMethod]
        public void VariableSelectionList()
        {
            //Act
            var = BusinessFlow.GetVariable("TestListVar");
            //Assert
            Assert.AreEqual("Apple", var.Value, "List");
        }
        [TestMethod]
        public void VariableRandomNum()
        {
            //Act
            var = BusinessFlow.GetVariable("TestRandomNumVar");
            //Assert
            Assert.AreNotEqual(var.Formula, var.Value, "var Ramndom number");
        }

        [TestMethod]
        public void VariableRandomstr()
        {
            //Act
            var = BusinessFlow.GetVariable("TestRandomStrVar");
            //Assert
            Assert.AreNotEqual(var.Formula, var.Value, "var Ramndom number");
        }

        [TestMethod]
        public void VariableSequenceVar()
        {
            //Act
            var = BusinessFlow.GetVariable("TestSequenceVar");
            //Assert
            Assert.AreEqual("11", var.Value, "var sequence");
        }
        [TestMethod]
        public void VariableTimer()
        {
            //Act
            var = BusinessFlow.GetVariable("TestTimerVar");
            //Assert
            Assert.AreEqual("0", var.Value, "var Selection List");
        }
        [TestMethod]
        public void VariableString()
        {
            //Act
            var = BusinessFlow.GetVariable("TestStringVar");
            //Assert
            Assert.AreEqual("hello", var.Value, "var string");
        }
    }
}




