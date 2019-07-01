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
            string TempSolutionFolder = TestResources.GetTestTempFolder(@"Solutions"+ Path.DirectorySeparatorChar+ "Variables");
            if (Directory.Exists(TempSolutionFolder))
            {
                Directory.Delete(TempSolutionFolder, true);
            }
            SR = GingerSolutionRepository.CreateGingerSolutionRepository();

            SR.CreateRepository(TempSolutionFolder);
            NewRepositorySerializer.AddClassesFromAssembly(typeof(BusinessFlow).Assembly);
            SR.Open(TempSolutionFolder);
            string FileName = TestResources.GetTestResourcesFile(@"Solutions"+ Path.DirectorySeparatorChar+ "Variables" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Business Flow 1.Ginger.BusinessFlow.xml");

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
        public void VariableString()
        {
            foreach (VariableBase variable in varList)
            {
                switch (variable.VariableUIType)
                {
                    case "Variable String":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("hello", var.Value, "var string");

                        break;

                    case "Variable Dynamic":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("12", var.Formula, "var dynamic");

                        break;

                    case "Variable List":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("Apple", var.Value, "var List");

                        break;
                    case "Variable Random Number":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreNotEqual(var.Formula, var.Value, "var Ramndom number");

                        break;

                    case "Variable Random String":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreNotEqual(var.Formula, var.Value, "var Ramndom string");

                        break;

                    case "Variable Sequence":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("11", var.Value, "var sequence");

                        break;
                    case "Variable Timer":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("0", var.Value, "var Timer");

                        break;
                    case "Variable Selection List":
                        //Act
                        var = variable;

                        //Assert
                        Assert.AreEqual("a", var.Value, "var Selection List");

                        break;
                }
            }
        }
    }
}



