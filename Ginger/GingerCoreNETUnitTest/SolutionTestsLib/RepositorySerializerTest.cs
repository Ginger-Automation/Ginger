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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace GingerCoreNETUnitTests.SolutionTestsLib
{
    [Level1]
    [TestClass]
    public class RepositorySerializerTest
    {
        static TestHelper mTestHelper = new TestHelper();
        public TestContext TestContext { get; set; }

        NewRepositorySerializer RS = new NewRepositorySerializer();

        string Separator = Path.DirectorySeparatorChar.ToString();

        public static void ClassInit(TestContext TestContext)
        {
            mTestHelper.ClassInitialize(TestContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            mTestHelper.ClassCleanup();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            mTestHelper.TestInitialize(TestContext);
            WorkSpace.LockWS();
        }


        [TestCleanup]
        public void TestCleanUp()
        {
            mTestHelper.TestCleanup();
            WorkSpace.RelWS();
        }

        // [Ignore] // different length on Linux Mac, Win...
        [TestMethod]
        [Timeout(60000)]
        public void ConvertBFToString()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow("BF1");

            // Since we can run this test on local user, Azure windows/Linux - we change the user name since we check total length and want it to be same
            BF.InitHeader();
            BF.RepositoryItemHeader.CreatedBy = "UnitTest";
            BF.RepositoryItemHeader.LastUpdateBy = "UnitTest";

            //Act
            string xml = RS.SerializeToString(BF);

            //Artifacts
            mTestHelper.CreateTestArtifact("BF1.txt", xml);

            //Assert

            //String size should be minimal - any failure for size check means something was added
            // Please double verify if the increase in size make sense and is needed before changing this value of expected length            
            int lt = xml.Count(f => f == '<');
            int gt = xml.Count(f => f == '>');
            Assert.IsTrue(xml.Length < 900, "Verify minimal xml is less than 900 bytes");   
            Assert.AreEqual(9, lt, "XML Elements count <"); 
            Assert.AreEqual(9, gt, "XML Elements count >"); 

            //Verify the major element of the expected xml
            Assert.IsTrue(xml.Contains("utf-8"));
            Assert.IsTrue(xml.Contains("<GingerRepositoryItem>"));
            Assert.IsTrue(xml.Contains("CreatedBy"));
            // Verify we get the short name: 'BusinessFlow'
            // and not 'GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib.BusinessFlow'
            Assert.IsTrue(xml.Contains("ItemType=\"BusinessFlow\""));
            // Verify Object class written in short name again, and since we changed only the name no other attrs should be added
            // We do not write all attribute only the one which changed from default value            
            Assert.IsTrue(xml.Contains(" Name=\"BF1"));
            Assert.IsTrue(xml.Contains("<BusinessFlow Guid="));
            Assert.IsTrue(xml.Contains("<Activities>"));
            // We need to have only one activity - make sure it is written squeezed to min
            Assert.IsTrue(xml.Contains("ActivityName=\"Activity 1\""));
            Assert.IsTrue(xml.Contains("</Activities>"));
            Assert.IsTrue(xml.Contains("</BusinessFlow></GingerRepositoryItem>"));

            //TODO: do not containts "00000000-0000-0000-0000-000000000000"

        }




        [TestMethod]
        [Timeout(60000)]
        public void BFToStringAndBack()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow("BF1");

            //Act
            string xml = RS.SerializeToString(BF);
            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert

            Assert.AreEqual(BF.Name, BF2.Name);


        }


        [TestMethod]
        [Timeout(60000)]
        public void VerifyRepositoryItemHeaderSaveLoad()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow("BF123");

            //Act
            string xml = RS.SerializeToString(BF);
            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreNotEqual(BF, BF2);   // need to be 2 dfferent ref, but with same data
            Assert.AreEqual(BF.Name, BF2.Name);
            Assert.AreEqual(BF.RepositoryItemHeader.CreatedBy, BF2.RepositoryItemHeader.CreatedBy);
            Assert.AreEqual(BF.RepositoryItemHeader.Created, BF2.RepositoryItemHeader.Created);
            Assert.AreEqual(BF.RepositoryItemHeader.Version, BF2.RepositoryItemHeader.Version);
            Assert.AreEqual(BF.RepositoryItemHeader.LastUpdate, BF2.RepositoryItemHeader.LastUpdate);
            Assert.AreEqual(BF.RepositoryItemHeader.LastUpdateBy, BF2.RepositoryItemHeader.LastUpdateBy);
            Assert.AreEqual(BF.RepositoryItemHeader.GingerVersion, BF2.RepositoryItemHeader.GingerVersion);
            Assert.AreEqual(BF.RepositoryItemHeader.ItemType, BF2.RepositoryItemHeader.ItemType);
            Assert.AreEqual(BF.RepositoryItemHeader.ItemGuid, BF2.RepositoryItemHeader.ItemGuid);

            Assert.AreEqual(BF.Guid, BF2.Guid);
            Assert.AreEqual(BF.Guid, BF2.RepositoryItemHeader.ItemGuid);
        }

        //[Ignore]
        //[TestMethod]
        //[Timeout(60000)]
        //public void BFWithData()
        //{
        //    //Arrange
        //    BusinessFlow BF = new BusinessFlow("BFData");
        //    Activity a1 = new Activity();
        //    a1.ActivityName = "a1";
        //    ActUIElement act1 = new ActUIElement();
        //    act1.ElementAction = ActUIElement.eElementAction.Click;
        //    act1.ElementLocateBy = ActUIElement.eLocateBy.ByAutomationID;
        //    act1.ElementLocateValue = "id123";
        //    act1.Description = "Click Button";
        //    FlowControl FC1 = new FlowControl();
        //    FC1.Condition = "A=1";
        //    act1.FlowControls.Add(FC1);
        //    a1.Acts.Add(act1);
        //    BF.Activities.Add(a1);

        //    //Act
        //    string xml = RS.SerializeToString(BF);

        //    // System.IO.File.WriteAllText(@"c:\temp\1.xml", xml);

        //    //Assert 

        //    // We verify it is saved in compact way, if something caused it to grow we will catch it in the TC
        //    // Assert.AreEqual(xml.Length, 936, "Serialized XML.Length");
        //    // changed because we added ParentID to support old Ginger
        //    Assert.AreEqual(1260, xml.Length, "Serialized XML.Length");


        //}


        [TestMethod]
        [Timeout(60000)]
        public void VerifySerialzedAttrDefaultValue()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow("BF1");

            //Act
            string xml = RS.SerializeToString(BF);
            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(BF.Source, BusinessFlow.eSource.Ginger);
            Assert.AreEqual(BF.Active, BF2.Active);
        }


        [TestMethod]
        [Timeout(60000)]
        public void VerifySerialzedAttrDefaultValueWithChange()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow("BF2");
            BF.Source = BusinessFlow.eSource.Gherkin;

            //Act
            string xml = RS.SerializeToString(BF);
            BusinessFlow BF2 = (BusinessFlow)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(BF.Source, BusinessFlow.eSource.Gherkin);
            Assert.AreEqual(BF.Active, BF2.Active);
        }


        [TestMethod]
        [Timeout(60000)]
        public void RepositoryItemKey()
        {
            //Arrange
            ApplicationPOMModel POM = new ApplicationPOMModel();
            POM.Name = "POM1";
            RepositoryItemKey key = POM.Key;

            //Act
            string xml = RS.SerializeToString(POM);
            ApplicationPOMModel POM2 = (ApplicationPOMModel)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(POM.Name, POM2.Name);
            Assert.AreEqual(key.Guid, POM2.Key.Guid);
            Assert.AreEqual(key.ItemName, POM2.Key.ItemName);
        }


        [TestMethod]
        [Timeout(60000)]
        public void ApiModelTagItemKey()
        {
            //Arrange
            ApplicationAPIModel API = new ApplicationAPIModel();
            RepositoryItemKey Tag1 = new RepositoryItemKey();
            Tag1.ItemName = "Tag1";
            Tag1.Guid = Guid.NewGuid();
            API.TagsKeys.Add(Tag1);

            RepositoryItemKey Tag2 = new RepositoryItemKey();
            Tag2.ItemName = "Tag2";
            Tag2.Guid = Guid.NewGuid();
            API.TagsKeys.Add(Tag2);

            //Act
            string xml = RS.SerializeToString(API);
            ApplicationAPIModel API2 = (ApplicationAPIModel)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(API.TagsKeys[0].Key, API2.TagsKeys[0].Key);
            Assert.AreEqual(API.TagsKeys[1].Key, API2.TagsKeys[1].Key);
        }


        [TestMethod]
        [Timeout(60000)]
        public void POMWithTargetApplicationKey()
        {
            //Arrange
            ApplicationPlatform AP = new ApplicationPlatform();
            AP.AppName = "App1";
            RepositoryItemKey key = AP.Key;

            ApplicationPOMModel POM = new ApplicationPOMModel();
            POM.Name = "POM1";
            POM.TargetApplicationKey = AP.Key;


            //Act
            string xml = RS.SerializeToString(POM);
            ApplicationPOMModel POM2 = (ApplicationPOMModel)NewRepositorySerializer.DeserializeFromText(xml);

            //Assert
            Assert.AreEqual(POM.Name, POM2.Name);
            Assert.AreEqual(POM2.TargetApplicationKey.Guid, key.Guid);
            Assert.AreEqual(POM2.TargetApplicationKey.ItemName, key.ItemName);
        }


        //-----------------------------------------------------------------------------------------------------------------------------------------
        // RunSetConfig
        //-----------------------------------------------------------------------------------------------------------------------------------------
        [TestMethod]
        [Timeout(60000)]
        public void RunSetConfigSaveLoad()
        {
            //Arrange

            //Act
            RunSetConfig RSC = new RunSetConfig();
            RSC.Name = "UT RSC1";
            GingerRunner ARC1 = new GingerRunner();
            ARC1.Name = " Agent 1";
            BusinessFlowRun BFR = new BusinessFlowRun();
            BFR.BusinessFlowName = "BF1";
            ARC1.BusinessFlowsRunList.Add(BFR);
            RSC.GingerRunners.Add(ARC1);
            // RSC .SaveToFile(@"c:\temp\UTRSC1.xml");

            //Assert

            // RunSetConfig RSC2 = (RunSetConfig)RepositoryItem.LoadFromFile(typeof(RunSetConfig), @"c:\temp\UTRSC1.xml");
        }

        [TestMethod]
        [Timeout(60000)]
        public void SaveLoadRunSetWithRunSetActionSendFreeEmailX2()
        {
            //Arrange

            //string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC.Ginger.RunSetConfig.xml";

            RunSetConfig RSC = new RunSetConfig();
            RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
            RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
            // RSC.RunSetActions.Add(new RunSetActionSendEmail());                                    

            string s = RS.SerializeToString(RSC);
            //Act
            RunSetConfig RSC2 = (RunSetConfig)NewRepositorySerializer.DeserializeFromText(s);

            //Assert
            Assert.AreEqual(RSC.RunSetActions.Count, RSC2.RunSetActions.Count);
        }


        [TestMethod]
        [Timeout(60000)]
        public void SaveLoadRunSetWithRunSetActionSendFreeEmailValidateEmail()
        {
            //Arrange
            //string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC2.Ginger.RunSetConfig.xml";

            RunSetConfig RSC = new RunSetConfig();
            RunSetActionSendFreeEmail RAFE = new RunSetActionSendFreeEmail();
            RAFE.Email.MailTo = "meme";
            RSC.RunSetActions.Add(RAFE);

            string s = RS.SerializeToString(RSC);

            //Act
            RunSetConfig RSC2 = (RunSetConfig)NewRepositorySerializer.DeserializeFromText(s);
            RunSetActionSendFreeEmail RAFE2 = (RunSetActionSendFreeEmail)RSC2.RunSetActions[0];

            //Assert
            Assert.AreEqual(1, RSC2.RunSetActions.Count);
            Assert.AreEqual("meme", RAFE2.Email.MailTo);
        }

        //// FIXME - change the xml to new RunSet the current is very old
        //[Ignore]
        //[TestMethod]
        //[Timeout(60000)]
        //public void LoadRunSetWith5Operations()
        //{
        //    NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();
        //    //Arrange
        //    string FileName = TestResources.GetTestResourcesFile(@"Repository\Default Run Set.Ginger.RunSetConfig.xml");

        //    //Act
        //    RunSetConfig RSC = (RunSetConfig)RepositorySerializer.DeserializeFromFile(FileName);

        //    //Assert
        //    Assert.AreEqual(5, RSC.RunSetActions.Count);
        //}


        [TestMethod]
        //[Timeout(60000)]
        public void BusinessFlowBrowserActionTest()
        {
            //Arrange
            //Put the BF in Test Resource
            NewRepositorySerializer RepositorySerializer = new NewRepositorySerializer();

            string FileName = TestResources.GetTestResourcesFile($"XML{Separator}Flow 1.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RepositorySerializer.DeserializeFromFile(FileName);

            //Asert
            Assert.AreEqual("GotoURL", (from aiv in businessFlow.Activities[0].Acts[0].InputValues where aiv.Param == "ControlAction" select aiv).FirstOrDefault().Value);
            Assert.AreEqual("NewTab", (from aiv in businessFlow.Activities[0].Acts[0].InputValues where aiv.Param == "GotoURLType" select aiv).FirstOrDefault().Value);
        }

        
        /// <summary>
        /// Activities Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]        
        public void ActivitiesLazyLoadViaStringData()
        {
            //arrange
            ObservableList<Activity> activities = new ObservableList<Activity>();
            string activityXml = File.ReadAllText(TestResources.GetTestResourcesFile(@"XML" + Path.DirectorySeparatorChar + "ActivityTest.Ginger.Activity.xml"));
            activities.LazyLoadDetails = new LazyLoadListDetails();
            activities.LazyLoadDetails.Config = new LazyLoadListConfig() { LazyLoadType = LazyLoadListConfig.eLazyLoadType.StringData };
            activities.LazyLoadDetails.DataAsString = activityXml;

            //act
            if (activities.LazyLoad)
            {
                activities.LoadLazyInfo();
            }

            //assert
            Assert.AreEqual(1, activities.Count);
            Assert.AreEqual(false, activities.LazyLoad);            
        }

        /// <summary>
        /// Activities Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void ActivitiesLazyLoadViaNodeLPath()
        {
            //arragne
            ObservableList<Activity> activities = new ObservableList<Activity>();
            activities.LazyLoadDetails = new LazyLoadListDetails() { XmlFilePath = Path.Combine(TestResources.GetTestResourcesFolder(@"XML"), "ActivityTest.Ginger.Activity.xml")};
            activities.LazyLoadDetails.Config = new LazyLoadListConfig() { LazyLoadType = LazyLoadListConfig.eLazyLoadType.NodePath, ListName=nameof(BusinessFlow.Activities)};

            //act
            if (activities.LazyLoad)
            {
                activities.LoadLazyInfo();
            }

            //assert            
            Assert.AreEqual(1, activities.Count);
            Assert.AreEqual(false, activities.LazyLoad);
        }

        /// <summary>
        /// Activities Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void BusinessFlowActivitiesLazyloadTest()
        {
            //Arrange
            //Put the BF in Test Resource
            string FileName = TestResources.GetTestResourcesFile(@"Solutions" + Path.DirectorySeparatorChar + "CLI" + Path.DirectorySeparatorChar + "BusinessFlows" + Path.DirectorySeparatorChar + "Flow 1.Ginger.BusinessFlow.xml");

            //Load BF
            BusinessFlow businessFlow = (BusinessFlow)RS.DeserializeFromFile(FileName);

            Assert.AreEqual(false, businessFlow.LazyLoadFlagForUnitTest);

            int count = businessFlow.Activities.Count();

            Assert.AreEqual(true, businessFlow.LazyLoadFlagForUnitTest);
            Assert.AreEqual(2, count);
        }

        /// <summary>
        /// Activities Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionActivitiesLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, true, "Validating Bf Activities were not loaded");
        }

        /// <summary>
        /// Activities Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionActivitiesLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<Activity> activities = bfs[0].Activities;

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, false, "Validating Bf Activities were loaded 1");
            Assert.AreEqual(activities.Count, 1, "Validating Bf Activities were loaded 2");
        }

        /// <summary>
        /// Actions Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionActionsLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<Activity> activities = bfs[0].Activities;

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, false, "Validating Bf Activities were loaded 1");
            Assert.AreEqual(activities.Count, 1, "Validating Bf Activities were loaded 2");
            Assert.AreEqual(activities[0].ActsLazyLoad, true, "Validating Activity Actions were not loaded");
        }

        /// <summary>
        /// Actions Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionActionsLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<Activity> activities = bfs[0].Activities;
            ObservableList<IAct> actions = activities[0].Acts;

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, false, "Validating Bf Activities were loaded 1");
            Assert.AreEqual(activities.Count, 1, "Validating Bf Activities were loaded 2");
            Assert.AreEqual(activities[0].ActsLazyLoad, false, "Validating Activity Actions were loaded 1");
            Assert.AreEqual(actions.Count, 2, "Validating Activity Actions were loaded 2 ");
        }

        /// <summary>
        /// POM Elements Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionPomElementsLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"),EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<ApplicationPOMModel> poms = SR.GetAllRepositoryItems<ApplicationPOMModel>();

            //Assert
            Assert.AreEqual(poms.Count, 1, "Validating POMs were loaded");
            Assert.AreEqual(poms[0].UnMappedUIElementsLazyLoad, true, "Validating POM Un Mappped Elements were not loaded");
            Assert.AreEqual(poms[0].MappedUIElementsLazyLoad, true, "Validating POM Mappped Elements were not loaded");
        }

        /// <summary>
        /// POM Elements Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void SolutionPomElementsLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"),EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<ApplicationPOMModel> poms = SR.GetAllRepositoryItems<ApplicationPOMModel>();
            ObservableList<ElementInfo> unMapped = poms[0].UnMappedUIElements;
            ObservableList<ElementInfo> mapped = poms[0].MappedUIElements;

            //Assert
            Assert.AreEqual(poms.Count, 1, "Validating POMs were loaded");
            Assert.AreEqual(poms[0].UnMappedUIElementsLazyLoad, false, "Validating POM Un Mappped Elements were loaded 1");
            //Assert.AreEqual(unMapped.Count, 1, "Validating POM Un Mappped Elements were loaded 2"); //TODO: move HtmlElementInfo to .NET core project for enabeling this Assert
            Assert.AreEqual(poms[0].MappedUIElementsLazyLoad, false, "Validating POM Mappped Elements were not loaded 1");
            //Assert.AreEqual(mapped.Count, 15, "Validating POM Mappped Elements were not loaded 2 "); //TODO: move HtmlElementInfo to .NET core project for enabeling this Assert
        }

        /// <summary>
        /// BF Variabels Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void BFVariablesLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();            

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].VariablesLazyLoad, true, "Validating Bf Variables were not loaded");
        }

        /// <summary>
        /// BF Variabels Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void BFVariablesLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<VariableBase> variables = bfs[0].Variables;
            
            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].VariablesLazyLoad, false, "Validating Bf Variables were loaded 1");
            Assert.AreEqual(variables.Count, 2, "Validating Bf Variables were loaded 2");
        }

        /// <summary>
        /// Activity Variables Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void ActivityVariablesLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<Activity> activities = bfs[0].Activities;

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, false, "Validating Bf Activities were loaded 1");
            Assert.AreEqual(activities.Count, 1, "Validating Bf Activities were loaded 2");
            Assert.AreEqual(activities[0].VariablesLazyLoad, true, "Validating Activity Variables were not loaded");
        }

        /// <summary>
        /// Activity Variables Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void ActivityVariablesLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<BusinessFlow> bfs = SR.GetAllRepositoryItems<BusinessFlow>();
            ObservableList<Activity> activities = bfs[0].Activities;
            ObservableList<VariableBase> variables = activities[0].Variables;

            //Assert
            Assert.AreEqual(bfs.Count, 1, "Validating Bfs were loaded");
            Assert.AreEqual(bfs[0].ActivitiesLazyLoad, false, "Validating Bf Activities were loaded 1");
            Assert.AreEqual(activities.Count, 1, "Validating Bf Activities were loaded 2");
            Assert.AreEqual(activities[0].VariablesLazyLoad, false, "Validating Activity variables were loaded 1");
            Assert.AreEqual(variables.Count, 1, "Validating Activity variables were loaded 2 ");
        }

        /// <summary>
        /// Run set Runners Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void RunsetRunnersLazyLoadTest_NotLoaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<RunSetConfig> runSetConfigs = SR.GetAllRepositoryItems<RunSetConfig>();

            //Assert
            Assert.AreEqual(runSetConfigs.Count, 1, "Validating run sets were loaded");
            Assert.AreEqual(runSetConfigs[0].GingerRunnersLazyLoad, true, "Validating run set runners were not loaded");
        }

        /// <summary>
        /// Run set Runners Lazy Load Test- CRITICAL- DO NOT AVOID ON FAILURE 
        /// </summary>
        [TestMethod]
        public void RunsetRunnersLazyLoadTest_Loaded()
        {
            //Arrange
            WorkSpace.Instance.OpenSolution(Path.Combine(TestResources.GetTestResourcesFolder(@"Solutions"), "BasicSimple"), EncryptionHandler.GetDefaultKey());
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;

            //Act
            ObservableList<RunSetConfig> runSetConfigs = SR.GetAllRepositoryItems<RunSetConfig>();
            ObservableList<GingerRunner> runners = runSetConfigs[0].GingerRunners;

            //Assert
            Assert.AreEqual(runSetConfigs.Count, 1, "Validating run sets were loaded");
            Assert.AreEqual(runSetConfigs[0].GingerRunnersLazyLoad, false, "Validating run set runners were loaded 1");
            Assert.AreEqual(runners.Count, 1, "Validating run set runners were loaded 2");
        }
    }
}
