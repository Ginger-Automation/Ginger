//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.IO;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTest.Conversion
//{

//    [TestClass]
//    [Level1]
//    public class ConversionUnitTest
//    {
//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TC)
//        {
//            //RepositorySerializerInitilizer2.InitClassTypesDictionary(); 
//        }

//        [Ignore]
//        [TestMethod]
//        public void VerifyActionConvertedHaveOldClassName()
//        {
//            //Arrange
//            NewRepositorySerializer RS = new NewRepositorySerializer();
//            string FileName = Path.Combine(Common.GetTestResourcesFolder(), "Conversion", "SCM - Create Customer 1.Ginger.BusinessFlow.xml");

//            // Act            
//            BusinessFlow BF = (BusinessFlow)RS.DeserializeFromFile(FileName);

            
//            //FIXME !!!!
//            // temp removed me and it will break !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
//            int i = BF.Activities.Count;
//            Act a0 = BF.Activities[0].Acts[0];

//            Act a1 = BF.Activities[1].Acts[0];

//            // Assert            


//            Assert.AreEqual("GingerCore.Actions.ActGotoURL", a0.OldClassName, "Verify OldClassName");
//            Assert.AreEqual("URL", a0.InputValues[0].Param, "Verify Param name 'Value' changed to 'URL'");

//            Assert.AreEqual("GingerCore.Actions.ActTextBox", a1.OldClassName, "Verify OldClassName");
//            Assert.AreEqual("LocateBy", a1.InputValues[0].Param);
//            Assert.AreEqual("ByID", a1.InputValues[0].Value);

//            Assert.AreEqual("LocateValue", a1.InputValues[1].Param);
//            Assert.AreEqual("UserName", a1.InputValues[1].Value);

//            Assert.AreEqual("Action", a1.InputValues[2].Param);
//            Assert.AreEqual("SetValue", a1.InputValues[2].Value);

//            Assert.AreEqual("Value", a1.InputValues[3].Param);
//            Assert.AreEqual("Yaron", a1.InputValues[3].Value);


//        }
//    }
//}
