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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Repository;
using Ginger.Run;
using Ginger.Run.RunSetActions;
using GingerCore;
using GingerCore.Actions;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using GingerTestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTests.NonUITests
{
    [TestClass]
    [Level1]
    public class RepositoryTest
    {
        [ClassInitialize]
        [Level1]
        public static void ClassInitialize(TestContext TC)
        {
            //??
            // RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();
            Ginger.App.InitClassTypesDictionary();
        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestMethod]  [Timeout(60000)]
        public void BizFlowSaveLoad()
        {

            //Arrange
            int ActivitiesToCreate = 5;

            //Act

            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Biz flow 1";
            BF.Description = "Desc 1";
            //BF.Status = BusinessFlow.eBusinessFlowStatus.Active; //TODOL do NOT write to XML if null or empty
            BF.Activities = new ObservableList<Activity>();

            for (int i = 1; i <= ActivitiesToCreate; i++)
            {
                Activity a = new Activity();
                a.ActivityName = "Activity number " + i;
                a.Description = "Desc - " + i;
                BF.Activities.Add(a);
                a.Status = eRunStatus.Passed;
                for (int j = 1; j <= 2; j++)
                {
                    ActTextBox t = new ActTextBox();
                    t.Description = "Set text box " + j;
                    t.LocateBy = eLocateBy.ByID;
                    t.LocateValue = "ID" + j;
                    a.Acts.Add(t);

                    ActGotoURL g = new ActGotoURL();
                    g.Description = "goto URL " + j;
                    g.LocateValue = "ID" + j;
                    a.Acts.Add(g);
                }
            }
            VariableString v = new VariableString();
            v.Name = "Var1";
            v.Description = "VDesc 1";
            BF.AddVariable(v);
            string FileName = TestResources.GetTempFile("bf1.xml");
            BF.RepositorySerializer.SaveToFile(BF, FileName);


            // Assert

            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), FileName);

            Assert.AreEqual(BF2.Name, BF.Name);
            Assert.AreEqual(BF2.Description, BF.Description);
            Assert.AreEqual(BF2.Activities.Count, ActivitiesToCreate);
            Assert.AreEqual(BF2.Variables.Count, 1);

            //Validations


        }



        [TestMethod]  [Timeout(60000)]
        public void BizFlowCheckIsDirtyFalse()
        {

            //Arrange
            string TempFilepath = TestResources.GetTempFile("bfIsDirtyTrue.xml");
            int ActivitiesToCreate = 2;

            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Biz flow 1";
            BF.Description = "Desc 1";
            //BF.Status = BusinessFlow.eBusinessFlowStatus.Active; //TODOL do NOT write to XML if null or empty
            BF.Activities = new ObservableList<Activity>();

            for (int i = 1; i <= ActivitiesToCreate; i++)
            {
                Activity a = new Activity();
                a.ActivityName = "Activity number " + i;
                a.Description = "Desc - " + i;
                BF.Activities.Add(a);
                a.Status = eRunStatus.Passed;
                for (int j = 1; j <= 2; j++)
                {
                    ActTextBox t = new ActTextBox();
                    t.Description = "Set text box " + j;
                    t.LocateBy = eLocateBy.ByID;
                    t.LocateValue = "ID" + j;
                    a.Acts.Add(t);

                    ActGotoURL g = new ActGotoURL();
                    g.Description = "goto URL " + j;
                    g.LocateValue = "ID" + j;
                    a.Acts.Add(g);
                }
            }
            VariableString v = new VariableString();
            v.Name = "Var1";
            v.Description = "VDesc 1";
            BF.AddVariable(v);


            //Act
            BF.RepositorySerializer.SaveToFile(BF, TempFilepath);


            // Assert
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), TempFilepath);
            Assert.IsTrue(BF2.DirtyStatus != Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified);
        }
        
        [TestMethod]  [Timeout(60000)]
        public void BizFlowClearBackup()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Businessflow1";
            BF.Description = "Test Clear Backup";
            BF.Activities = new ObservableList<Activity>();
            
            Activity a = new Activity() { ActivityName = "Activity 1", Description = "Desciption -1", Status = eRunStatus.Passed };            
            BF.Activities.Add(a);
            
            BF.SaveBackup();
            Activity b = new Activity() { ActivityName = "Activity 2", Description = "Desciption -2", Status = eRunStatus.Passed };            
            BF.Activities.Add(b);
            
            string TempFilepath = TestResources.GetTempFile("bfClearBackup.xml");

            //Act
            BF.RepositorySerializer.SaveToFile(BF, TempFilepath);
            BF.SaveBackup();
            BF.RestoreFromBackup();

            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), TempFilepath);
            BF2.SaveBackup();//dirty now just indicate if backup exist
            BF2.Description = "aaa";

            // Assert
            Assert.AreEqual(BF2.Activities.Count,BF.Activities.Count);
        }

        [TestMethod]  [Timeout(60000)]
        public void ActivitiesClearBackup()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow();
            string FileName = TestResources.GetTempFile("activityClearBackup.xml");
            BF.Name = "Businessflow1";
            BF.Description = "Test Clear Backup";
            BF.Activities = new ObservableList<Activity>();
            Activity a = new Activity() { ActivityName = "Activity 1", Description = "Desciption -1", Status = eRunStatus.Passed };            
            BF.Activities.Add(a);

            ActTextBox t = new ActTextBox() { Description = "Set text box ", LocateBy = eLocateBy.ByID, LocateValue = "ID" };                        
            a.Acts.Add(t);

            //Act
            BF.RepositorySerializer.SaveToFile(BF, FileName);   
            a.SaveBackup();
            ActGotoURL g = new ActGotoURL() { Description = "goto URL ", LocateValue = "ID" };            
            a.Acts.Add(g);
            BF.RepositorySerializer.SaveToFile(BF, FileName);
            a.SaveBackup();            
            a.RestoreFromBackup();

            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), FileName);            
            BF2.SaveBackup(); //dirty now just indicate if backup exist
            BF2.Description = "aaa";

            // Assert
            Assert.AreEqual(BF2.Activities[0].Acts.Count, BF.Activities[0].Acts.Count);            
        }

        [TestMethod]  [Timeout(60000)]
        public void ActionClearBackup()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Businessflow1";
            BF.Description = "Test Clear Backup";
            BF.Activities = new ObservableList<Activity>();
            Activity a = new Activity();
            a.ActivityName = "Activity 1";
            a.Description = "Desciption -1";
            BF.Activities.Add(a);
            a.Status = eRunStatus.Passed;

            ActGotoURL g = new ActGotoURL();
            g.Description = "goto URL ";
            g.LocateValue = "ID";
            a.Acts.Add(g);
            string TempFilepath = TestResources.GetTempFile("actionClearBackup.xml");

            //Act
            BF.RepositorySerializer.SaveToFile(BF, TempFilepath);
            a.SaveBackup();
            g.LocateValue = "ID1";
            BF.RepositorySerializer.SaveToFile(BF, TempFilepath);
            a.SaveBackup();
            a.RestoreFromBackup();

            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), TempFilepath);
            BF2.SaveBackup();//dirty now just indicate if backup exist
            BF2.Description = "aaa";

            // Assert
            Assert.AreEqual(BF2.Activities[0].Acts[0].LocateValue, BF.Activities[0].Acts[0].LocateValue);
        }


        [TestMethod]  [Timeout(60000)]
        public void BizFlowCheckIsDirtyTrue()
        {

            //Arrange
            int ActivitiesToCreate = 2;

            string TempFilepath = TestResources.GetTempFile("bfIsDirtyTrue.xml");

            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Biz flow 1";
            BF.Description = "Desc 1";
            //BF.Status = BusinessFlow.eBusinessFlowStatus.Active; //TODOL do NOT write to XML if null or empty
            BF.Activities = new ObservableList<Activity>();

            for (int i = 1; i <= ActivitiesToCreate; i++)
            {
                Activity a = new Activity();
                a.ActivityName = "Activity number " + i;
                a.Description = "Desc - " + i;
                BF.Activities.Add(a);
                a.Status = eRunStatus.Passed;
                for (int j = 1; j <= 2; j++)
                {
                    ActTextBox t = new ActTextBox();
                    t.Description = "Set text box " + j;
                    t.LocateBy = eLocateBy.ByID;
                    t.LocateValue = "ID" + j;
                    a.Acts.Add(t);

                    ActGotoURL g = new ActGotoURL();
                    g.Description = "goto URL " + j;
                    g.LocateValue = "ID" + j;
                    a.Acts.Add(g);
                }
            }
            VariableString v = new VariableString();
            v.Name = "Var1";
            v.Description = "VDesc 1";
            BF.AddVariable(v);


            //Act
            BF.RepositorySerializer.SaveToFile(BF, TempFilepath);


            // Assert
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), TempFilepath);
            BF2.StartDirtyTracking();
            BF2.Description = "aaa";
            Assert.IsTrue(BF2.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified);
        }
                
        [TestMethod]  [Timeout(60000)]
        public void RunSetConfigSaveLoad()
        {
            //Arrange"
            string TempFilepath = TestResources.GetTempFile("UTRSC1.xml");

            //Act
            RunSetConfig RSC = new RunSetConfig();
            RSC.Name = "UT RSC1";
            GingerRunner ARC1= new GingerRunner();
            ARC1.Name = " Agent 1";
            BusinessFlowRun BFR = new BusinessFlowRun();
            BFR.BusinessFlowName = "BF1";
            ARC1.BusinessFlowsRunList.Add(BFR);
            RSC.GingerRunners.Add(ARC1);
            
            RSC.RepositorySerializer.SaveToFile(RSC, TempFilepath);
            
            //Assert
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            RunSetConfig RSC2 = (RunSetConfig)newRepositorySerializer.DeserializeFromFile(typeof(RunSetConfig), TempFilepath);
        }

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]       
        //public void SaveLoadRunSetWithRunSetActionSendFreeEmailX2()
        //{
        //    //Arrange
        //    RepositorySerializer RepositorySerializer = new RepositorySerializer();
        //    string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC.Ginger.RunSetConfig.xml";

        //    RunSetConfig RSC = new RunSetConfig();
        //    RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
        //    RSC.RunSetActions.Add(new RunSetActionSendFreeEmail());
        //    // RSC.RunSetActions.Add(new RunSetActionSendEmail());                                    

        //    RSC.SaveToFile(FileName);
        //    //Act
        //    RunSetConfig RSC2 = (RunSetConfig)RepositorySerializer.DeserializeFromFile(FileName);

        //    //Assert
        //    Assert.AreEqual(RSC.RunSetActions.Count, RSC2.RunSetActions.Count);
        //}

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void SaveLoadRunSetWithRunSetActionSendFreeEmailValidateEmail()
        //{
        //    //Arrange
        //    RepositorySerializer RepositorySerializer = new RepositorySerializer();
        //    string FileName = Common.getGingerUnitTesterTempFolder() + @"RSC2.Ginger.RunSetConfig.xml";

        //    RunSetConfig RSC = new RunSetConfig();
        //    RunSetActionSendFreeEmail RAFE = new RunSetActionSendFreeEmail();
        //    RAFE.Email.MailTo = "meme";
        //    RSC.RunSetActions.Add(RAFE);
            

        //    RSC.SaveToFile(FileName);
        //    //Act
        //    RunSetConfig RSC2 = (RunSetConfig)RepositorySerializer.DeserializeFromFile(FileName);
        //    RunSetActionSendFreeEmail RAFE2 = (RunSetActionSendFreeEmail)RSC2.RunSetActions[0];

        //    //Assert
        //    Assert.AreEqual(1, RSC2.RunSetActions.Count);
        //    Assert.AreEqual("meme", RAFE2.Email.MailTo);
        //}

        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void LoadRunSetWith5Operations()
        //{
        //    //Arrange
        //    RepositorySerializer RepositorySerializer = new RepositorySerializer();
        //    string FileName = Common.getGingerUnitTesterDocumentsFolder() + @"Repository\Default Run Set.Ginger.RunSetConfig.xml";

        //    //Act
        //    RunSetConfig RSC = (RunSetConfig)RepositorySerializer.DeserializeFromFile(FileName);            

        //    //Assert
        //    Assert.AreEqual(5, RSC.RunSetActions.Count);
        //}


        [TestMethod]  [Timeout(60000)]
        public void BizFlowAddActivitiesFromSharedRepoSaveLoad()
        {

            //Arrange
            int ActivitiesToCreate = 5;
            string FileName = TestResources.GetTempFile("BFSaveLoad.xml");

            BusinessFlow BF = new BusinessFlow() { Name = "Biz flow 1", Description = "Desc 1"};
                
            BF.Activities = new ObservableList<Activity>();

            for (int i = 1; i <= ActivitiesToCreate; i++)
            {
                Activity a = new Activity() { ActivityName = "Activity " + i, Description = "desc" + i, Status = eRunStatus.Passed };
                BF.AddActivity(a);

                //for (int j = 1; j <= 2; j++)
                //{
                //    ActTextBox t = new ActTextBox();
                //    t.Description = "Set text box " + j;
                //    t.LocateBy = Act.eLocatorType.ByID;
                //    t.LocateValue = "ID" + j;
                //    a.Acts.Add(t);

                //    ActGotoURL g = new ActGotoURL();
                //    g.Description = "goto URL " + j;
                //    g.LocateValue = "ID" + j;
                //    a.Acts.Add(g);
                //}
            }
            VariableString v = new VariableString();
            v.Name = "Var1";
            v.Description = "VDesc 1";
            BF.AddVariable(v);

            //ValidationDB vdb = new ValidationDB() { Description ="DBV1", Expected ="exp1" };
            //BF.Activities[0].Asserts.Add(vdb);

            //Act
            BF.RepositorySerializer.SaveToFile(BF, FileName);

            // Assert
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), FileName);
           Assert.AreEqual(BF2.Activities.Count(), ActivitiesToCreate);
            //Assert.AreEqual(BF2. Activities[0].Asserts.Count(), 1);
            //BF2.Description = "aaa";

           


        }


        private BusinessFlow GetBizFlow(string Name, string Description)
        {            
            BusinessFlow BF = new BusinessFlow();
            //BF.Name = "Biz flow 1";
            //BF.Description = "Desc 1";
            ////BF.Status = BusinessFlow.eBusinessFlowStatus.Active; //TODOL do NOT write to XML if null or empty
            //BF.Activities = new ObservableList<Activity>();

            //for (int i = 1; i <= ActivitiesToCreate; i++)
            //{
            //    Activity a = new Activity();
            //    a.ActivityName = "Activity number " + i;
            //    a.Description = "Desc - " + i;
            //    BF.Activities.Add(a);
            //    a.Status = GingerCore.Activity.eActivityStatus.Pass;
            //    for (int j = 1; j <= 2; j++)
            //    {
            //        ActTextBox t = new ActTextBox();
            //        t.Description = "Set text box " + j;
            //        t.LocateBy = Act.eLocatorType.ByID;
            //        t.LocateValue = "ID" + j;
            //        a.Acts.Add(t);

            //        ActGotoURL g = new ActGotoURL();
            //        g.Description = "goto URL " + j;
            //        g.LocateValue = "ID" + j;
            //        a.Acts.Add(g);
            //    }
            //}
            //VariableString v = new VariableString();
            //v.Name = "Var1";
            //v.Description = "VDesc 1";
            //BF.AddVariable(v);


            return BF;
        }


        [TestMethod]  [Timeout(60000)]
        public void BackUpRestore()
        {
            //Arrange
            int ActivitiesToCreate = 5;
            string BizFlowName = "Biz flow Back/Rest";
            string BizFlowDescription = "Desc Back/Rest tester";

            BusinessFlow BF = new BusinessFlow() { Name = BizFlowName };
            BF.Status = BusinessFlow.eBusinessFlowStatus.Development;
            BF.Activities = new ObservableList<Activity>();
            ObservableList<Activity> OriginalActivitiesObj = BF.Activities;

            for (int i = 1; i <= ActivitiesToCreate; i++)
            {
                Activity a = new Activity() { ActivityName = "Activity " + i, Description = "desc" + i, Status = eRunStatus.Passed };
                BF.AddActivity(a);
            }

            // Create Activity to check ref
            Activity a6 = new Activity() { ActivityName = "a6" };
            BF.Activities.Add(a6);

            // Add one action to make sure backup drill down, and restore the ref item not a copy
            ActGotoURL act1 = new ActGotoURL();
            act1.Description = "Goto URL 1";
            a6.Acts.Add(act1);

            //add action with input/output vals
            act1.InputValues = new ObservableList<ActInputValue>();
            string firstInputValName="Param1";
            ActInputValue firstInputVal = new ActInputValue() { Param = firstInputValName };
            act1.InputValues.Add(firstInputVal);
            act1.InputValues.Add(new ActInputValue() { Param = "Param2" });

            //add flow control
            act1.FlowControls = new ObservableList<FlowControl>();
            act1.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "A=B", FlowControlAction =eFlowControlAction.GoToActivity });
            eFlowControlAction secondFlowControlAction =eFlowControlAction.RerunAction;
            GingerCore.FlowControlLib.FlowControl secondFlowControl = new GingerCore.FlowControlLib.FlowControl() { Condition = "C>123", FlowControlAction = secondFlowControlAction };
            act1.FlowControls.Add(secondFlowControl);
            act1.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "D=111", FlowControlAction =eFlowControlAction.StopRun });

            //BF Variables
            VariableString v = new VariableString();
            v.Name = "Var1";
            v.Description = "VDesc 1";
            BF.AddVariable(v);
            VariableSelectionList sl = new VariableSelectionList();
            sl.Name = "Var 2";
            sl.OptionalValuesList = new ObservableList<OptionalValue>();
            sl.OptionalValuesList.Add(new OptionalValue("11"));
            sl.OptionalValuesList.Add(new OptionalValue("22"));
            sl.OptionalValuesList.Add(new OptionalValue("33"));
            BF.AddVariable(sl);

            // BF.SaveBackup();            

            BF.SaveBackup();            

            //Erase/Modify some stuff
            BF.Name = "zzzz";
            BF.Description = BizFlowDescription;
            BF.Status = BusinessFlow.eBusinessFlowStatus.Retired;
            BF.Activities[1].Description = "AAAA";
            BF.Activities.Remove(BF.Activities[2]);
            BF.Activities.Remove(BF.Activities[3]);
            act1.Description = "ZZZZZ";

            act1.InputValues[0].Param = "qqq";
            act1.InputValues.Remove(act1.InputValues[1]);

            act1.FlowControls[1].FlowControlAction =eFlowControlAction.MessageBox;
            act1.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "Val=123" });
            act1.FlowControls.Add(new GingerCore.FlowControlLib.FlowControl() { Condition = "Val=555" });

            sl.OptionalValuesList[0].Value="aaaa";
            sl.OptionalValuesList.Add(new OptionalValue("44"));
            sl.OptionalValuesList.Add(new OptionalValue("55"));

            // BF.RestoreFromBackup();
            BF.RestoreFromBackup();

            // Assert            
           Assert.AreEqual(BF.Name, BizFlowName, "BF.Name");
           Assert.AreEqual(BF.Description, null, "BF.Description");

            // check enum restore
           Assert.AreEqual(BF.Status, BusinessFlow.eBusinessFlowStatus.Development, "BF.Status");
           Assert.AreEqual(BF.Activities.Count(), ActivitiesToCreate + 1, "BF.Activities.Count()");
            
            //check original list ref obj
           Assert.AreEqual(BF.Activities, OriginalActivitiesObj, "BF.Activities REF");
           Assert.AreEqual(BF.Activities[0].Description, "desc1", "BF.Activities[0].Description");
           Assert.AreEqual(BF.Activities[5].ActivityName, "a6", "BF.Activities[5].ActivityName");

            // Check original action ref is back                
           Assert.AreEqual(BF.Activities[5], a6, "BF.Activities[5] REF");
           Assert.AreEqual(act1.Description, "Goto URL 1", "act1.Description");
           Assert.AreEqual(a6.Acts[0], act1, "a6.Acts[0]");

            //check Action input values
           Assert.AreEqual(act1.InputValues.Count, 2, "act1.InputValues.Count");
           Assert.AreEqual(act1.InputValues[0], firstInputVal, "act1.InputValues[0] REF");
           Assert.AreEqual(act1.InputValues[0].Param, firstInputValName, "act1.InputValues[0].Param");

            //check Action flow control
           Assert.AreEqual(act1.FlowControls.Count, 3, "act1.FlowControls.Count");
           Assert.AreEqual(act1.FlowControls[1], secondFlowControl, "act1.FlowControls[1] REF");
           Assert.AreEqual(act1.FlowControls[1].FlowControlAction, secondFlowControlAction, "act1.FlowControls[1].FlowControlAction");

            //BF variables
           Assert.AreEqual(BF.Variables.Count, 2, "BF.Variables.Count");
           Assert.AreEqual(BF.Variables[1], sl, "BF.Variables[0] REF");
           Assert.AreEqual(((VariableSelectionList) BF.Variables[1]).OptionalValuesList[0].Value, "11", "BF.Variables[0].Value");
       }


        [TestMethod]  [Timeout(60000)]
        public void BackUpRestoreBFWithVariableSelectionList()
        {
            //Arrange
            BusinessFlow BF = new BusinessFlow() { Name = "Biz flow VariableSelectionList" };

            VariableSelectionList sl = new VariableSelectionList();
            sl.Name = "Var 2";
            sl.OptionalValuesList = new ObservableList<OptionalValue>();
            sl.OptionalValuesList.Add(new OptionalValue("11"));
            sl.OptionalValuesList.Add(new OptionalValue("22"));
            sl.OptionalValuesList.Add(new OptionalValue("33"));

            BF.AddVariable(sl);

            //Act
            BF.SaveBackup();

            // Modify the SL
            sl.OptionalValuesList[0].Value = "aaaa";
            sl.OptionalValuesList.Add(new OptionalValue("44"));
            sl.OptionalValuesList.Add(new OptionalValue("55"));

            BF.RestoreFromBackup();

            //Assert
            Assert.AreEqual(BF.Variables.Count, 1, "BF.Variables.Count");
            Assert.AreEqual(BF.Variables[0], sl, "BF.Variables[0] REF");
            Assert.AreEqual(((VariableSelectionList)BF.Variables[0]).OptionalValuesList[0].Value, "11", "BF.Variables[0].Value");
            Assert.AreEqual(((VariableSelectionList)BF.Variables[0]).OptionalValuesList.Count(), 3, "(VariableSelectionList)BF.Variables[0]).OptionalValuesList.Count()");
        }


        [TestMethod]  [Timeout(60000)]
        public void BackUpRestoreVariableSelectionList()
        {
            //Arrange            
                        
            VariableSelectionList sl = new VariableSelectionList();
            sl.Name = "Var 2";
            sl.OptionalValuesList = new ObservableList<OptionalValue>();
            sl.OptionalValuesList.Add(new OptionalValue("11"));            

            //Act
            sl.SaveBackup();

            // Modify the SL
            sl.OptionalValuesList[0].Value = "00";            
            
            sl.RestoreFromBackup();

            //Assert           
           Assert.AreEqual("11", sl.OptionalValuesList[0].Value, "OptionalValuesList[0].Value");           
        }

        //[TestMethod]  [Timeout(60000)]
        //public void ActivitiesReadSpeedTest()
        //{

        //    //Arrange
        //    LocalRepository LR = new LocalRepository();
        //    string folder = TestResources.GetTestResourcesFolder(@"Repository\Activities");
        //    ObservableList<Activity> list = new ObservableList<Activity>();

        //    Stopwatch st = new Stopwatch();
        //    st.Reset();

        //    //Act
        //    st.Start();
        //    LR.LoadObjectsToListIncludingSubFoldersForSpeedTest(folder, list, typeof(Activity));
        //    st.Stop();

        //    //Assert
        //    Assert.AreEqual(14, list.Count(), "list.Count()");
        //    Assert.IsTrue(st.ElapsedMilliseconds < 1000, "st.ElapsedMilliseconds <1000");

        //}

        [TestMethod]  [Timeout(60000)]
        public void TestObjectAttrofOneRepoItem()
        {
            //Check Save and Load of RunSetConfig with Send Email action - RunSetActionSendEmail have 'Email' field which is single object as field, if save load correctly test pass

            //Arrange
            RunSetConfig RSC = new RunSetConfig();
            RunSetActionSendEmail RSASE = new RunSetActionSendEmail();
            RSASE.Name = "Send Email";
            string MailFrom = "From.Me@amdocs.com";
            string MailTo = "To.You@amdocs.com";
            string MailCC = "CC.YouTo@amdocs.com";
            RSASE.Email.MailFrom = MailFrom;
            RSASE.Email.MailTo = MailTo;
            RSASE.Email.MailCC = MailCC;
            RSC.RunSetActions.Add(RSASE);

            //Act            
            string FileName = TestResources.GetTempFile("RunSetConfig1.xml");

            RSC.RepositorySerializer.SaveToFile(RSC, FileName);

            //
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            RunSetConfig RSC2 = (RunSetConfig)newRepositorySerializer.DeserializeFromFile(FileName);

            //Assert
            Assert.AreEqual(RSC.Name, RSC2.Name, "RSC.Name");
            RunSetActionSendEmail RSASE2 = (RunSetActionSendEmail)RSC.RunSetActions[0];
            Assert.AreEqual(RSASE2.Email.MailFrom, MailFrom, "RSC2.MailFrom");
            Assert.AreEqual(RSASE2.Email.MailTo, MailTo, "RSC2.MailTo");
            Assert.AreEqual(RSASE2.Email.MailCC, MailCC, "RSC2.MailCC");            
        }


        [TestMethod]  [Timeout(60000)]
        public void BizFlowWithTags()
        {

            //Arrange
            BusinessFlow BF = new BusinessFlow();
            BF.Name = "Biz flow With Tags";
            BF.Description = "Desc 1";
            BF.Activities = new ObservableList<Activity>();
            Guid g1 = Guid.NewGuid();
            Guid g2 = Guid.NewGuid();
            BF.Tags.Add(g1);            
            BF.Tags.Add(g2);

            Activity a = new Activity();
            a.ActivityName = "Activity number 1";
            a.Description = "Desc - 1";
            a.Status = eRunStatus.Passed;
            BF.Activities.Add(a);
            
            //Act

            string FileName = TestResources.GetTempFile("BFWithTags.xml");
            BF.RepositorySerializer.SaveToFile(BF, FileName);


            // Assert
            NewRepositorySerializer newRepositorySerializer = new NewRepositorySerializer();
            BusinessFlow BF2 = (BusinessFlow)newRepositorySerializer.DeserializeFromFile(typeof(BusinessFlow), FileName);

           Assert.AreEqual(BF2.Name, BF.Name);
           Assert.AreEqual(BF2.Description, BF.Description);
           Assert.AreEqual(BF2.Tags[0], g1);
           Assert.AreEqual(BF2.Tags[1], g2);

        }


        //[Ignore]
        //[TestMethod]  [Timeout(60000)]
        //public void BizFlowSaveLoadSpeedTest()
        //{

        //    //Arrange
        //    int ActivitiesToCreate = 50;

        //    //Act

            
        //    BusinessFlow BF = new BusinessFlow();
        //    BF.Name = "Biz flow " + ActivitiesToCreate;
        //    BF.Description = "Desc " + ActivitiesToCreate;
            
        //    BF.Activities = new ObservableList<Activity>();

        //    for (int i = 1; i <= ActivitiesToCreate; i++)
        //    {
        //        Activity a = new Activity();
        //        a.ActivityName = "Activity number " + i;
        //        a.Description = "Desc - " + i;
        //        BF.Activities.Add(a);
        //        a.Status = eRunStatus.Passed;
        //        for (int j = 1; j <= 20; j++)
        //        {
        //            ActTextBox t = new ActTextBox();
        //            t.Description = "Set text box " + j;
        //            t.LocateBy = eLocateBy.ByID;
        //            t.LocateValue = "ID" + j;
        //            a.Acts.Add(t);

        //            ActGotoURL g = new ActGotoURL();
        //            g.Description = "goto URL " + j;
        //            g.LocateValue = "ID" + j;
        //            a.Acts.Add(g);
        //        }
        //    }
        //    VariableString v = new VariableString();
        //    v.Name = "Var1";
        //    v.Description = "VDesc 1";
        //    BF.AddVariable(v);
        //    string FileName = Common.getGingerUnitTesterTempFolder() + "bf1.xml";
        //    BF.SaveToFile(FileName);
            

            
        //    Stopwatch st = new Stopwatch();
        //    st.Start();
        //    BusinessFlow BF2 = (BusinessFlow)RepositoryItem.LoadFromFile(typeof(BusinessFlow), FileName);
        //    st.Stop();


        //    // Assert
        //    Assert.IsTrue(st.ElapsedMilliseconds < 100, "Elapsed < 1000, actual=" + st.ElapsedMilliseconds);
        //    Assert.AreEqual(BF2.Name, BF.Name);
        //    Assert.AreEqual(BF2.Description, BF.Description);
        //    Assert.AreEqual(ActivitiesToCreate, BF2.Activities.Count);
        //    Assert.AreEqual(1, BF2.Variables.Count);

            
            

        //}

        //[TestMethod]  [Timeout(60000)]
        //public void BigBizFlowLoadSpeedTest()
        //{

        //    //Arrange
        //    string FileName = TestResources.GetTestResourcesFile(@"Repository\BigFlow1.Ginger.BusinessFlow.xml");

        //    //Act
            
            
        //    BusinessFlow BF1 = (BusinessFlow)RepositoryItem.LoadFromFile(typeof(BusinessFlow), FileName);

        //    int i = BF1.Activities.Count;


        //    // Assert
        //    //Assert.IsTrue(st.ElapsedMilliseconds < 100, "Elapsed < 1000, actual=" + st.ElapsedMilliseconds);
        //    Assert.AreEqual(BF1.Activities.Count, 78);
        //    //Assert.AreEqual(BF2.Description, BF.Description);
        //    //Assert.AreEqual(ActivitiesToCreate, BF2.Activities.Count);
        //    //Assert.AreEqual(1, BF2.Variables.Count);




        //}


        [TestMethod]  [Timeout(60000)]
        public void CopyAction()
        {

            //Arrange
            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "www.google.com";

            //Act
            ActGotoURL a2 = (ActGotoURL)actGotoURL.CreateCopy();

            //Assert
            Assert.AreEqual(actGotoURL.Description, a2.Description);
            

        }

        [TestMethod]  [Timeout(60000)]
        public void CreateDuplicationAction()
        {

            //Arrange
            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "www.google.com";

            //Act
            ActGotoURL a2 = (ActGotoURL)actGotoURL.CreateCopy();

            //Assert
            Assert.AreEqual(actGotoURL.Description, a2.Description);


        }


               
        [TestMethod]  [Timeout(60000)]
        public void FlowcontrolTest_WithBFCreateCopy()
        {
            //Arrange
            BusinessFlow bf = new BusinessFlow("Test");

            Activity activity = new Activity();
            activity.ActivityName = "Login";

            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "Launch";

            activity.Acts.Add(actGotoURL);

            Activity activity2 = new Activity();
            activity2.ActivityName = "Test";

            ActDummy act2 = new ActDummy();
            act2.Description = "WaitForApp";

            activity.Acts.Add(act2);

            FlowControl flowControl = new FlowControl();
            flowControl.Active = true;
            flowControl.Condition = "1=1";
            flowControl.FlowControlAction = eFlowControlAction.GoToActivity;
            flowControl.Value = activity2.Guid + flowControl.GUID_NAME_SEPERATOR + activity2.ItemName;

            FlowControl flowControl2 = new FlowControl();
            flowControl2.Active = true;
            flowControl2.Condition = "2=2";
            flowControl2.FlowControlAction = eFlowControlAction.GoToAction;
            flowControl2.Value = act2.Guid + flowControl.GUID_NAME_SEPERATOR + act2.ItemName;


            actGotoURL.FlowControls.Add(flowControl);
            actGotoURL.FlowControls.Add(flowControl2);

            bf.Activities.RemoveAt(0);
            bf.Activities.Add(activity);
            bf.Activities.Add(activity2);

            activity2.ActivityName = "Test_New";                        
            bf.RepositorySerializer.SaveToFile(bf, TestResources.GetTempFile("BF.xml"));

            //Act
            BusinessFlow bfCopy = (BusinessFlow)bf.CreateInstance();

            Guid newGuidOfActivity2 = bfCopy.Activities.Where(x => x.ItemName == "Test_New").FirstOrDefault().Guid;

            Guid newGuidOfAct2 = bfCopy.Activities[0].Acts.Where(x => x.ItemName == "WaitForApp").FirstOrDefault().Guid;


            //Assert
            Assert.AreEqual(bfCopy.Activities[0].Acts[0].FlowControls[1].GetGuidFromValue(), newGuidOfAct2);
            Assert.AreEqual(bfCopy.Activities[0].Acts[0].FlowControls[0].GetGuidFromValue(), newGuidOfActivity2);
           

        }
        

        [TestMethod]  [Timeout(60000)]
        public void FlowcontrolTest_WithActivityCreateInstance()
        {
            //Arrange
            Activity activity = new Activity();

            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "Launch";

            ActDummy act2 = new ActDummy();
            act2.Description = "WaitForApp";


            FlowControl flowControl = new FlowControl();
            flowControl.Active = true;
            flowControl.Condition = "1=1";
            flowControl.FlowControlAction = eFlowControlAction.GoToAction;
            flowControl.Value = act2.Guid + flowControl.GUID_NAME_SEPERATOR + act2.ItemName;


            actGotoURL.FlowControls.Add(flowControl);


            activity.Acts.Add(actGotoURL);
            activity.Acts.Add(act2);

            act2.Description = "WaitForApp_Copy";

            //Act
            Activity copyActivity = (Activity)activity.CreateInstance();
            Guid newGuidOfAct2 = copyActivity.Acts.Where(x => x.ItemName == "WaitForApp_Copy").FirstOrDefault().Guid;

            //Assert
            Assert.AreEqual(copyActivity.Acts[0].FlowControls[0].GetGuidFromValue(), newGuidOfAct2);


        }

        [TestMethod]  [Timeout(60000)]
        public void ActionVariableDependancyTest_WithCreateInstance()
        {
            //Arrange
            Activity activity = new Activity();

            VariableSelectionList selectionList = new VariableSelectionList();
            selectionList.OptionalValuesList.Add(new OptionalValue("a"));
            selectionList.OptionalValuesList.Add(new OptionalValue("b"));

            VariableDependency vd = new VariableDependency(selectionList.Guid, selectionList.ItemName, selectionList.Value);
           
            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "www.google.com";
            actGotoURL.VariablesDependencies.Add(vd);

            ActDummy act2 = new ActDummy();
            actGotoURL.Description = "www.google.com";
            actGotoURL.VariablesDependencies.Add(vd);


            activity.Variables.Add(selectionList);
            activity.Acts.Add(actGotoURL);
            activity.Acts.Add(act2);

          
            //Act
            Activity copyActivity = (Activity)activity.CreateInstance();

            //Assert
            Assert.AreEqual(copyActivity.Variables[0].Guid, copyActivity.Acts[0].VariablesDependencies[0].VariableGuid);


        }

        //[TestMethod]  [Timeout(60000)]
        //public void ActivityVariableDependancyTest_UnserializeFile()
        //{
        //    //Arrange
        //    string fileName = Path.Combine(TestResources.GetTestResourcesFile("Repository\\Business Flow_WithVariabledependancy.Ginger.BusinessFlow.xml"));
        //    BusinessFlow BF1 = (BusinessFlow)RepositoryItem.LoadFromFile(typeof(BusinessFlow), fileName);

        //    //Act
        //    BusinessFlow bf2 =(BusinessFlow)BF1.CreateInstance();

        //    //Assert
        //    Assert.AreNotEqual(bf2.Activities[0].Acts[0].VariablesDependencies[0].VariableGuid, BF1.Activities[0].Acts[0].VariablesDependencies[0].VariableGuid);
        //    Assert.AreEqual(bf2.Activities[0].Variables[0].Guid, bf2.Activities[0].Acts[0].VariablesDependencies[0].VariableGuid);
        //}



        [TestMethod]  [Timeout(60000)]
        public void ActivityVariableDependancyTest_WithCreateInstance()
        {
            //Arrange
            BusinessFlow bf = new BusinessFlow("Test");

            Activity activity = new Activity();         
            VariableSelectionList selectionList2 = new VariableSelectionList();
            selectionList2.Name = "activityVariable1";
            selectionList2.OptionalValuesList.Add(new OptionalValue("c"));
            selectionList2.OptionalValuesList.Add(new OptionalValue("d"));          

            VariableDependency vd = new VariableDependency(selectionList2.Guid, selectionList2.ItemName, selectionList2.Value);

            ActGotoURL actGotoURL = new ActGotoURL();
            actGotoURL.Description = "www.google.com";
            actGotoURL.VariablesDependencies.Add(vd);
            ActDummy actDummy = new ActDummy();
            actDummy.Description = "www.google.com";
            actDummy.VariablesDependencies.Add(vd);
            activity.Variables.Add(selectionList2);
            activity.Acts.Add(actGotoURL);
             activity.Acts.Add(actDummy);
            Activity activity2 = new Activity();
            ActDummy act2 = new ActDummy();
            act2.Description = "www.google.com";
            activity2.Acts.Add(act2);
            VariableSelectionList selectionList = new VariableSelectionList();
            selectionList.Name = "bfVariable1";
            selectionList.OptionalValuesList.Add(new OptionalValue("a"));
            selectionList.OptionalValuesList.Add(new OptionalValue("b"));

            bf.Variables.Add(selectionList);


            VariableDependency vd1 = new VariableDependency(selectionList.Guid, selectionList.ItemName, selectionList.Value);
                 
            activity.VariablesDependencies.Add(vd1);
            activity2.VariablesDependencies.Add(vd1);
     
            bf.Activities.RemoveAt(0);
            bf.Activities.Add(activity);
            bf.Activities.Add(activity2);

          
            //Act
            BusinessFlow bfCopy = (BusinessFlow)bf.CreateInstance();
         
            Guid newBFVarGuid = bfCopy.Variables.Where(x => x.Name == "bfVariable1").FirstOrDefault().Guid;
            Guid newActivityVarGuid = bfCopy.Activities[0].Variables[0].Guid;

            //Assert
            Assert.AreEqual(newBFVarGuid, bfCopy.Activities[0].VariablesDependencies[0].VariableGuid);
            Assert.AreEqual(newBFVarGuid, bfCopy.Activities[1].VariablesDependencies[0].VariableGuid);
            Assert.AreEqual(newActivityVarGuid, bfCopy.Activities[0].Acts[0].VariablesDependencies[0].VariableGuid);
            Assert.AreEqual(newActivityVarGuid, bfCopy.Activities[0].Acts[1].VariablesDependencies[0].VariableGuid);

        }


    }
}
