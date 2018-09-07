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

using Amdocs.Ginger.Common.GeneralLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace GingerCoreNETUnitTest.GeneralLib.JSON
{
    [TestClass]
   public class JSONExtendedTests
    {
        private static JsonExtended XDE = null;


        [ClassInitialize]
        public static void ClassInitialize(TestContext TestContext)
        {

        }

        [Level2]
        [TestMethod]
        public void XMLDocGetAllNodesTest()
        {
            //Arrange
            string jsonfilepath = TestResources.GetTestResourcesFile(@"JSON\sample.json");
            string JSOnText = System.IO.File.ReadAllText(jsonfilepath);

            //Act
            XDE = new JsonExtended(JSOnText);            
            int nodesCount = XDE.GetAllNodes().Count;
            var endingNodes = XDE.GetEndingNodes();            

            //Assert
            Assert.AreEqual(12, nodesCount);            
        }

        
    }
}
