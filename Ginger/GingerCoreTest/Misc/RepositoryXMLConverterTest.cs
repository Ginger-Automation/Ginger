#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Repository;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Repository;
using GingerTestHelper;
using GingerWPF.WorkSpaceLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class RepositoryXMLConverterTest
    {                

        [ClassInitialize]        
        public static void ClassInitialize(TestContext TC)
        {
            TargetFrameworkHelper.Helper = new DotNetFrameworkHelper();
            WorkSpace.Init(new WorkSpaceEventHandler());                   
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            
        }

        [TestInitialize]
        public void TestInitialize()
        {
            
        }

        [Ignore] // need to add handle for old serializer event to handle old action 
        [TestMethod]  [Timeout(60000)]
        public void BusinessFlowDeserializationTest()
        {
            //Arrange            
            string sFileName = TestResources.GetTestResourcesFile(@"Converter" + Path.DirectorySeparatorChar + "IPDLSAM.Ginger.BusinessFlow.xml");
            string txt = File.ReadAllText(sFileName);

            //Act
            BusinessFlow BF = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(txt);

            //Assert
            //TODO: add more asserts
           Assert.AreEqual(14, BF.Activities.Count, "BF has 14 activities");
        }

        [TestMethod]  [Timeout(60000)]
        public void AgentDeserializationTest()
        {
            //Arrange
            RepositorySerializer RepositorySerializer = new RepositorySerializer();
            string sFileName = TestResources.GetTestResourcesFile( @"Converter" + Path.DirectorySeparatorChar + "IB1.Ginger.Agent.xml");

            //Act
            Agent agent = (Agent)RepositorySerializer.DeserializeFromFile(typeof(Agent), sFileName);

            //Assert
            //TODO: add more asserts
           Assert.AreEqual(agent.Name, "IB1");
           Assert.AreEqual(agent.DriverType,Agent.eDriverType.InternalBrowser);
        }
        
        [TestMethod]  [Timeout(60000)]
        public void EnvironmentDeserializationTest()
        {
            //Arrange
            RepositorySerializer RepositorySerializer = new RepositorySerializer();
            string sFileName = TestResources.GetTestResourcesFile(@"Converter" + Path.DirectorySeparatorChar + "CMI.Ginger.Environment.xml");

            //Act
            ProjEnvironment env = (ProjEnvironment)RepositorySerializer.DeserializeFromFile(typeof(ProjEnvironment), sFileName);

            //Assert            
           Assert.AreEqual(env.Name, "CMI IIS test server");
           Assert.AreEqual(env.Applications.Count, 2);
            //TODO: add more asserts
        }
      


    }
}
