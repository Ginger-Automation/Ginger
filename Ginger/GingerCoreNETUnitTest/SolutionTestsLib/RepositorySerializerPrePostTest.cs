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
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Drivers;
using GingerCoreNETUnitTest.WorkSpaceLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTests.SolutionTestsLib
{
    [Level1]
    [TestClass]
    public class RepositorySerializerPrePostTest
    {
        

        NewRepositorySerializer RS = new NewRepositorySerializer();
        

        [ClassInitialize]
        public static void ClassInitialize(TestContext TC)
        {
            WorkspaceHelper.CreateWorkspace2();            
            NewRepositorySerializer.AddClass(typeof(DummyAction).Name, typeof(DummyAction));          
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            
        }


        [TestCleanup]
        public void TestCleanUp()
        {
         
        }


        public class DummyAction : RepositoryItemBase
        {
            [IsSerializedForLocalRepository]
            public string Name { get; set; }
            public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

            [IsSerializedForLocalRepository]
            public double NewAge { get; set; }

            [IsSerializedForLocalRepository]
            public int Age { get; set; }


            public override void PreDeserialization()
            {
                NewAge = 1;
            }

            public override void PostDeserialization()
            {
                if (Age == 0)
                {
                    NewAge = -1;
                }

                if (Age == 999)
                {
                    Age = -1;
                }

            }

            public override bool SerializationError(SerializationErrorType errorType, string name, string value)
            {
                if (errorType == SerializationErrorType.PropertyNotFound)
                {
                    if (name == "OldAge")
                    {
                        Age = int.Parse(value);
                        return true;
                    }
                }

                return false;
            }
            
        }


        [TestMethod]        
        public void SimpleActionToStringAndBack()
        {
            //Arrange
            DummyAction dummyActionOriginal = new DummyAction() { Name = "d1", Age = 3, NewAge = 3.2};

            //Act
            string xml = RS.SerializeToString(dummyActionOriginal);
            DummyAction dummyActionCopy = (DummyAction)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(dummyActionOriginal.Name, dummyActionCopy.Name);
            Assert.AreEqual(dummyActionOriginal.Age, dummyActionCopy.Age);
            Assert.AreEqual(dummyActionOriginal.NewAge, dummyActionCopy.NewAge);
        }

        [TestMethod]
        public void PreSerialization()
        {
            //Arrange
            DummyAction dummyActionOriginal = new DummyAction() { Name = "prepre" };

            //Act
            string xml = RS.SerializeToString(dummyActionOriginal);
            DummyAction dummyActionCopy = (DummyAction)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(-1, dummyActionCopy.NewAge, "NewAge = -1");
        }

        [TestMethod]
        public void PostSerialization()
        {
            //Arrange
            DummyAction dummyActionOriginal = new DummyAction() { Name = "post", Age = 999 };

            //Act
            string xml = RS.SerializeToString(dummyActionOriginal);            
            DummyAction dummyActionCopy = (DummyAction)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(-1, dummyActionCopy.Age, "Age = -1");
        }

        [TestMethod]
        public void InvalidFieldInXMLHandling()
        {
            //Arrange
            string fileName = TestResources.GetTestResourcesFile(@"Repository" + Path.DirectorySeparatorChar + "dummyAction.xml");

            //Act
            DummyAction dummyAction = (DummyAction)RS.DeserializeFromFile(fileName);
            
            //Assert
            Assert.AreEqual(7, dummyAction.Age, "Age = 7");
        }

        [TestMethod]
        public void WindoowsDriverDefaultActionTimeoutUpgradeTest()
        {
            //Arrange
            string fileName = TestResources.GetTestResourcesFile(@"Repository" + Path.DirectorySeparatorChar + "Windows.Ginger.Agent.xml");

            //Act
            Agent windowsAgent = (Agent)RS.DeserializeFromFile(fileName);

            DriverConfigParam actionTimeoutParameter = windowsAgent.DriverConfiguration.Where(x => x.Parameter == nameof(DriverBase.ActionTimeout)).FirstOrDefault();

            //Assert
            Assert.AreEqual("30", actionTimeoutParameter.Value, "ActionTimeout = 30");
            Assert.AreEqual("Action Timeout - default is 30 seconds", actionTimeoutParameter.Description, "ActionTimeout Description Validation");
        }

     


    }
}
