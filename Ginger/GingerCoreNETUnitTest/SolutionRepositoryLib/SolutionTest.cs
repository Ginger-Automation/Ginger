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
using Ginger.SolutionGeneral;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.NonUITests
{    
    [TestClass]
    [Level1]
    public class SolutionTest 
    {        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            WorkspaceHelper.CreateWorkspace2();                     
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }


        


        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void TestCleanUp()
        {

        }

        //test trigger unit test
        /// <summary>
        /// Test the new solution been created successfully
        /// </summary>
        [TestMethod]  [Timeout(60000)]
        public void CreateNewSolution()
        {
            // Arrange
            Solution createSol = new Solution();
            createSol.Name = "NonUi Solution Test";
            
            string SolFile = TestResources.GetTempFile("Solution1.Ginger.Solution.xml");

            //Act
            createSol.RepositorySerializer.SaveToFile(createSol, SolFile);
            Solution loadSol = SolutionOperations.LoadSolution(SolFile, false);

            //Assert
           Assert.AreEqual(loadSol.Name, createSol.Name);
           Assert.AreEqual(loadSol.MainPlatform, createSol.MainPlatform);
        }


        [TestMethod]  [Timeout(60000)]
        public void CreateNewSolutionWithMultiUnderscore()
        {
            // Arrange
            Solution createSol = new Solution();
            createSol.Name = "Non_Ui_Solution_Test";
            string solFile = TestResources.GetTempFile("Solution2.Ginger.Solution.xml");
            
            //Act
            createSol.RepositorySerializer.SaveToFile(createSol, solFile);
            Solution loadSol = SolutionOperations.LoadSolution(solFile, false);

            //Assert
           Assert.AreEqual(loadSol.Name, createSol.Name);
           Assert.AreEqual(loadSol.MainPlatform, createSol.MainPlatform);
        }

        

    }
}
