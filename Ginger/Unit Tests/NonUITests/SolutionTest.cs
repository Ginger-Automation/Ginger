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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.Repository;
using GingerCore;
using Ginger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GingerTestHelper;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class SolutionTest 
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {

        }
        //test trigger unitest
        /// <summary>
        /// Test the new solution been created successfully
        /// </summary>
        [TestMethod]
        public void CreateNewSolution()
        {
            // Arrange
            Ginger.Environments.Solution createSol = new Ginger.Environments.Solution();
            createSol.Name = "NonUi Solution Test";
            createSol.Folder = @"C:\NonUI_Tests\";
            // createSol.MainPlatform = GingerCore.Platforms.Platform.eType.Web;
            if (!System.IO.Directory.Exists(createSol.Folder))
            {
                System.IO.Directory.CreateDirectory(createSol.Folder);
            }

            //Act
            createSol.SaveToFile(createSol.Folder + @"\Ginger.Solution.xml");
            Ginger.Environments.Solution loadSol = (Ginger.Environments.Solution)RepositoryItem.LoadFromFile(typeof(Ginger.Environments.Solution), createSol.Folder + @"\Ginger.Solution.xml");

            //Assert
           Assert.AreEqual(loadSol.Name, createSol.Name);
           Assert.AreEqual(loadSol.MainPlatform, createSol.MainPlatform);
        }
        [TestMethod]
        public void CreateNewSolutionWithMultiUnderscore()
        {
            // Arrange
            Ginger.Environments.Solution createSol = new Ginger.Environments.Solution();
            createSol.Name = "Non_Ui_Solution_Test";
            createSol.Folder = @"C:\temp\Non_Ui_Solution_Test\";
            // createSol.MainPlatform = GingerCore.Platforms.Platform.eType.Web;
            if (!System.IO.Directory.Exists(createSol.Folder))
            {
                System.IO.Directory.CreateDirectory(createSol.Folder);
            }

            //Act
            createSol.SaveToFile(createSol.Folder + @"\Non_Ui_Solution_Test.Solution.xml");
            Ginger.Environments.Solution loadSol = (Ginger.Environments.Solution)RepositoryItem.LoadFromFile(typeof(Ginger.Environments.Solution), createSol.Folder + @"\Non_Ui_Solution_Test.Solution.xml");

            //Assert
           Assert.AreEqual(loadSol.Name, createSol.Name);
           Assert.AreEqual(loadSol.MainPlatform, createSol.MainPlatform);
        }
    }
}
