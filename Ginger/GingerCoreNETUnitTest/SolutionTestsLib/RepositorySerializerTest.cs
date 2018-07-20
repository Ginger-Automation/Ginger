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

//using Amdocs.Ginger.Common.UIElement;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.Drivers.CommonActionsLib;
//using GingerCoreNET.RunLib;
//using GingerCoreNET.RunLib.RunSetActions;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerTestHelper;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using UnitTestsCP.GeneralLib;

//namespace GingerCoreNETUnitTests.SolutionTestsLib
//{
//    [Level1]
//    [TestClass]
//    public class RepositorySerializerTest
//    {
//        NewRepositorySerializer RS = new NewRepositorySerializer();

//        [ClassInitialize]
//        public static void ClassInitialize(TestContext TC)
//        {
//            // New            
//            //RepositorySerializerInitilizer2.InitClassTypesDictionary();

//            //Old
//            // RepositorySerializerInitilizer.InitClassTypesDictionary();
//        }

//        [TestCleanup]
//        public void TestCleanUp()
//        {

//        }

        
//        [TestMethod]
//        public void ConvertBFToString()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BF1");

//            //Act
//            string xml = RS.SerializeToString(BF);


//            /// to see the xml as file uncomment below line
//            // System.IO.File.WriteAllText(@"c:\temp\1.xml", xml);

//            //Assert

//            //String size should be minimal - any failure for size check means something was added
//            // Please double verify if the increase in size make send and is needed before changing this value of expected length
//            // Assert.AreEqual(xml.Length, 491);
            
//            Assert.AreEqual(491, xml.Length);

//            //Verify the major element of the expected xml
//            Assert.IsTrue(xml.Contains("utf-8"));
//            Assert.IsTrue(xml.Contains("<GingerRepositoryItem>"));
//            Assert.IsTrue(xml.Contains("CreatedBy"));
//            // Verify we get the short name: 'BusinessFlow'
//            // and not 'GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib.BusinessFlow'
//            Assert.IsTrue(xml.Contains("ItemType=\"BusinessFlow\""));
//            // Verify Object class written in short name again, and since we changed only the name no other attrs should be added
//            // We do not write all attribute only the one which changed from default value            
//            Assert.IsTrue(xml.Contains(" Name=\"BF1"));
//            Assert.IsTrue(xml.Contains("<BusinessFlow Guid="));
//            Assert.IsTrue(xml.Contains("<Activities>"));
//            // We need to have only one activity - make sure it is written squeezed to min
//            Assert.IsTrue(xml.Contains("<Activity ActivityName=\"Activity 1\""));
//            Assert.IsTrue(xml.Contains("</Activities>"));
//            Assert.IsTrue(xml.Contains("</BusinessFlow></GingerRepositoryItem>"));

//        }



        
//        [TestMethod]
//        public void BFToStringAndBack()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BF1");

//            //Act
//            string xml = RS.SerializeToString(BF);
//            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert

//            Assert.AreEqual(BF.Name, BF2.Name);


//        }

        
//        [TestMethod]
//        public void VerifyRepositoryItemHeaderSaveLoad()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BF123");

//            //Act
//            string xml = RS.SerializeToString(BF);
//            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreNotEqual(BF, BF2);   // need to be 2 dfferent ref, but with same data
//            Assert.AreEqual(BF.Name, BF2.Name);
//            Assert.AreEqual(BF.RepositoryItemHeader.CreatedBy, BF2.RepositoryItemHeader.CreatedBy);
//            Assert.AreEqual(BF.RepositoryItemHeader.Created, BF2.RepositoryItemHeader.Created);
//            Assert.AreEqual(BF.RepositoryItemHeader.Version, BF2.RepositoryItemHeader.Version);
//            Assert.AreEqual(BF.RepositoryItemHeader.LastUpdate, BF2.RepositoryItemHeader.LastUpdate);
//            Assert.AreEqual(BF.RepositoryItemHeader.LastUpdateBy, BF2.RepositoryItemHeader.LastUpdateBy);
//            Assert.AreEqual(BF.RepositoryItemHeader.GingerVersion, BF2.RepositoryItemHeader.GingerVersion);
//            Assert.AreEqual(BF.RepositoryItemHeader.ItemType, BF2.RepositoryItemHeader.ItemType);
//            Assert.AreEqual(BF.RepositoryItemHeader.ItemGuid, BF2.RepositoryItemHeader.ItemGuid);

//            Assert.AreEqual(BF.Guid, BF2.Guid);
//            Assert.AreEqual(BF.Guid, BF2.RepositoryItemHeader.ItemGuid);
//        }

//        [Ignore]
//        [TestMethod]
//        public void BFWithData()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BFData");
//            Activity a1 = new Activity();
//            a1.ActivityName = "a1";
//            ActUIElement act1 = new ActUIElement();
//            act1.ElementAction = ActUIElement.eElementAction.Click;
//            act1.ElementLocateBy = ActUIElement.eLocateBy.ByAutomationID;
//            act1.ElementLocateValue = "id123";
//            act1.Description = "Click Button";
//            FlowControl FC1 = new FlowControl();
//            FC1.Condition = "A=1";
//            act1.FlowControls.Add(FC1);
//            a1.Acts.Add(act1);
//            BF.Activities.Add(a1);

//            //Act
//            string xml = RS.SerializeToString(BF);

//            // System.IO.File.WriteAllText(@"c:\temp\1.xml", xml);

//            //Assert 

//            // We verify it is saved in compact way, if something caused it to grow we will catch it in the TC
//            // Assert.AreEqual(xml.Length, 936, "Serialized XML.Length");
//            // changed because we added ParentID to support old Ginger
//            Assert.AreEqual(1260, xml.Length,  "Serialized XML.Length");


//        }

        
//        [TestMethod]
//        public void VerifySerialzedAttrDefaultValue()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BF1");

//            //Act
//            string xml = RS.SerializeToString(BF);
//            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreEqual(BF.Source, BusinessFlow.eSource.Ginger);
//            Assert.AreEqual(BF.Active, BF2.Active);
//        }

        
//        [TestMethod]
//        public void VerifySerialzedAttrDefaultValueWithChange()
//        {
//            //Arrange
//            BusinessFlow BF = new BusinessFlow("BF2");
//            BF.Source = BusinessFlow.eSource.Gherkin;

//            //Act
//            string xml = RS.SerializeToString(BF);
//            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreEqual(BF.Source, BusinessFlow.eSource.Gherkin);
//            Assert.AreEqual(BF.Active, BF2.Active);
//        }

        
//        [TestMethod]
//        public void RepositoryItemKey()
//        {
//            //Arrange
//            ApplicationPOMModel POM = new ApplicationPOMModel();
//            POM.Name = "POM1";
//            RepositoryItemKey key = POM.Key;

//            //Act
//            string xml = RS.SerializeToString(POM);
//            ApplicationPOMModel POM2 = (ApplicationPOMModel)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreEqual(POM.Name, POM2.Name);
//            Assert.AreEqual(key.Guid, POM2.Key.Guid);
//            Assert.AreEqual(key.ItemName, POM2.Key.ItemName);
//        }

        
//        [TestMethod]
//        public void ApiModelTagItemKey()
//        {
//            //Arrange
//            ApplicationAPIModel API = new ApplicationAPIModel();
//            RepositoryItemKey Tag1 = new RepositoryItemKey();            
//            Tag1.ItemName = "Tag1";
//            Tag1.Guid = Guid.NewGuid();
//            API.TagsKeys.Add(Tag1);

//            RepositoryItemKey Tag2 = new RepositoryItemKey();
//            Tag2.ItemName = "Tag2";
//            Tag2.Guid = Guid.NewGuid();
//            API.TagsKeys.Add(Tag2);

//            //Act
//            string xml = RS.SerializeToString(API);
//            ApplicationAPIModel API2 = (ApplicationAPIModel)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreEqual(API.TagsKeys[0].Key, API2.TagsKeys[0].Key);
//            Assert.AreEqual(API.TagsKeys[1].Key, API2.TagsKeys[1].Key);
//        }

        
//        [TestMethod]
//        public void POMWithTargetApplicationKey()
//        {
//            //Arrange
//            ApplicationPlatform AP = new ApplicationPlatform();
//            AP.AppName = "App1";
//            RepositoryItemKey key = AP.Key;

//            ApplicationPOMModel POM = new ApplicationPOMModel();
//            POM.Name = "POM1";
//            POM.TargetApplicationKey = AP.Key;
            

//            //Act
//            string xml = RS.SerializeToString(POM);
//            ApplicationPOMModel POM2 = (ApplicationPOMModel)NewRepositorySerializer.DeserializeFromText(xml);

//            //Assert
//            Assert.AreEqual(POM.Name, POM2.Name);
//            Assert.AreEqual(POM2.TargetApplicationKey.Guid, key.Guid);
//            Assert.AreEqual(POM2.TargetApplicationKey.ItemName, key.ItemName);
//        }


//        //-----------------------------------------------------------------------------------------------------------------------------------------
//        // RunSetConfig
//        //-----------------------------------------------------------------------------------------------------------------------------------------
//        [TestMethod]
//        public void RunSetConfigSaveLoad()
//        {
//            //Arrange

//            //Act
//            RunSetConfig RSC = new RunSetConfig();
//            RSC.Name = "UT RSC1";
//            GingerRunner ARC1 = new GingerRunner();
//            ARC1.Name = " Agent 1";
//            BusinessFlowRun BFR = new BusinessFlowRun();
//            BFR.BusinessFlowName = "BF1";
//            ARC1.BusinessFlowsRunList.Add(BFR);
//            RSC.GingerRunners.Add(ARC1);
//            // RSC .SaveToFile(@"c:\temp\UTRSC1.xml");

//            //Assert

//            // RunSetConfig RSC2 = (RunSetConfig)RepositoryItem.LoadFromFile(typeof(RunSetConfig), @"c:\temp\UTRSC1.xml");
//        }
        
//        [TestMethod]
//        public void SaveLoadRunSetWithRunSetActionSendFreeEmailX2()
//        {
//            //Arrange

//            //string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC.Ginger.RunSetConfig.xml";

//            RunSetConfig RSC = new RunSetConfig();
//            RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
//            RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
//            // RSC.RunSetActions.Add(new RunSetActionSendEmail());                                    

//            string s = RS.SerializeToString(RSC);
//            //Act
//            RunSetConfig RSC2 = (RunSetConfig)NewRepositorySerializer.DeserializeFromText(s);

//            //Assert
//            Assert.AreEqual(RSC.RunSetActions.Count, RSC2.RunSetActions.Count);
//        }


//        [TestMethod]
//        public void SaveLoadRunSetWithRunSetActionSendFreeEmailValidateEmail()
//        {
//            //Arrange
//            //string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC2.Ginger.RunSetConfig.xml";

//            RunSetConfig RSC = new RunSetConfig();
//            RunSetActionSendFreeEmail RAFE = new RunSetActionSendFreeEmail();
//            RAFE.Email.MailTo = "meme";
//            RSC.RunSetActions.Add(RAFE);

//            string s = RS.SerializeToString(RSC);

//            //Act
//            RunSetConfig RSC2 = (RunSetConfig)NewRepositorySerializer.DeserializeFromText(s);
//            RunSetActionSendFreeEmail RAFE2 = (RunSetActionSendFreeEmail)RSC2.RunSetActions[0];

//            //Assert
//            Assert.AreEqual(1, RSC2.RunSetActions.Count);
//            Assert.AreEqual("meme", RAFE2.Email.MailTo);
//        }


//        [TestMethod]
//        public void LoadRunSetWith5Operations()
//        {
//            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();
//            //Arrange
//            string FileName = Common.GetTestResourcesFolder() + @"\Repository\Default Run Set.Ginger.RunSetConfig.xml";

//            //Act
//            RunSetConfig RSC = (RunSetConfig)RepositorySerializer.DeserializeFromFile(FileName);

//            //Assert
//            Assert.AreEqual(5, RSC.RunSetActions.Count);
//        }


//    }
//}
