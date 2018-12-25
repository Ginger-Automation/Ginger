#region License
/*
Copyright © 2014-2018 European Support Limited

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
using GingerCore.Variables;
using GingerTest.POMs;
using GingerTestHelper;
using GingerWPFUnitTest;
using GingerWPFUnitTest.GeneralLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.VariablesLib
{
    [TestClass]
    [Level3]
    public class VariablesTest
    {

        static GingerWPF.WorkSpaceLib.WorkSpaceEventHandler WSEH = new GingerWPF.WorkSpaceLib.WorkSpaceEventHandler();

        static TestContext mTC;
        static string SolutionFolder;
        static GingerAutomator mGingerAutomator;

        [ClassInitialize]
        public static void ClassInit(TestContext TC)
        {
            mTC = TC;

            string sampleSolutionFolder = TestResources.GetTestResourcesFolder(@"Solutions\BasicSimple");
            SolutionFolder = TestResources.getGingerUnitTesterTempFolder(@"Solutions\VariablesTest");
            if (Directory.Exists(SolutionFolder))
            {
                Directory.Delete(SolutionFolder, true);
            }

            CopyDir.Copy(sampleSolutionFolder, SolutionFolder);

            mGingerAutomator = GingerAutomator.StartSession();
            mGingerAutomator.OpenSolution(SolutionFolder);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            GingerAutomator.EndSession();
        }


        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        
        [TestMethod]
        public void AddGlobalStringVariable()
        {
            //Arrange
            string name = "str1";
            
            //Act                        
            GlobalVariablesPOM globalVariablesPOM = mGingerAutomator.MainWindowPOM.GotoGlobalVariables();
            globalVariablesPOM.AddStringVariable(name);
            VariableBase v = (from x in Ginger.App.UserProfile.Solution.Variables where x.Name == name select x).SingleOrDefault();

            //Assert
            Assert.AreEqual(name, v.Name, "Var Name");
        }
    }
}
