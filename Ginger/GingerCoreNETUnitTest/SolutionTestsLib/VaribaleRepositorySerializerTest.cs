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
        private static NewRepositorySerializer RS = new NewRepositorySerializer();
        private static BusinessFlow businessFlow = new BusinessFlow();
        static ObservableList<VariableBase> varList = new ObservableList<VariableBase>();

        public VariableBase Var { get; set; } = null;
        public static BusinessFlow BusinessFlow { get => businessFlow; set => businessFlow = value; }

        public static ObservableList<VariableBase> GetVarList()
        {
            return varList;
        }

        public void SetVarList(ObservableList<VariableBase> value)
        {
            varList = value;
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommon);
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
            Var = BusinessFlow.GetVariable("TestPasswordVar");
            //Assert
            Assert.AreEqual("+S+B0OE+KDopqrquyaOF2Q==", Var.Value, "var string");
        }

        [TestMethod]
        public void VariableDynamic()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestDynamicVar");
            //Assert
            Assert.AreEqual("12", Var.Formula, "var dynamic");
        }
        [TestMethod]
        public void VariableSelectionList()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestListVar");
            //Assert
            Assert.AreEqual("Apple", Var.Value, "List");
        }
        [TestMethod]
        public void VariableRandomNum()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestRandomNumVar");
            //Assert
            Assert.AreNotEqual(Var.Formula, Var.Value, "var Random number");
        }

        [TestMethod]
        public void VariableRandomstr()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestRandomStrVar");
            //Assert
            Assert.AreNotEqual(Var.Formula, Var.Value, "var Random number");
        }

        [TestMethod]
        public void VariableSequenceVar()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestSequenceVar");
            //Assert
            Assert.AreEqual("11", Var.Value, "var sequence");
        }
        [TestMethod]
        public void VariableTimer()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestTimerVar");
            //Assert
            Assert.AreEqual("0", Var.Value, "var Selection List");
        }
        [TestMethod]
        public void VariableString()
        {
            //Act
            Var = BusinessFlow.GetVariable("TestStringVar");
            //Assert
            Assert.AreEqual("hello", Var.Value, "var string");
        }
    }
}




