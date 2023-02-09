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
using Ginger.ALM.ZephyrEnt;
using Ginger.ALM.ZephyrEnt.TreeViewItems;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerCore.ALM.ZephyrEnt.Bll;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ZephyrEntStdSDK.Models;
using ZephyrEntStdSDK.Models.Base;
using static Ginger.ALM.ZephyrEnt.ZephyrEntPlanningExplorerPage;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM.Repository
{
    class ZephyrEnt_Repository : ALMRepository
    {
        public ALMCore AlmCore { get; set; }

        List<BaseResponseItem> matchingTC = null;
        List<TestCaseResource> tcsRepositoryList = new List<TestCaseResource>();
        string bfEntityType = string.Empty;
        string moduleParentId = string.Empty;
        string folderCycleId = string.Empty;
        public ZephyrEnt_Repository(ALMCore almCore)
        {
            this.AlmCore = almCore;
        }
        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
        {
            if (AlmCore.IsConnectValidationDone)
            {
                return true;
            }
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Zephyr server");
                if (ALMIntegration.Instance.AlmCore.ConnectALMServer())
                {
                    AlmCore.IsConnectValidationDone = true;

                    return true;
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, "Bad credentials");
                    return false;
                }
            }
            catch (Exception e)
            {
                if (userMsgStyle == eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.QcConnectFailure, e.Message); //TODO: Fix message
                }
                else if (userMsgStyle == eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);
                }
                Reporter.ToLog(eLogLevel.WARN, "Error connecting to Zephyr server", e);
                return false;
            }
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath, bool performSaveAfterExport, BusinessFlow businessFlow = null)
        {
            TestCaseResource currentTC;
            if (activtiesGroup == null)
            {
                return false;
            }
            //if it is called from shared repository need to select path
            if (uploadPath == null)
            {
                ZephyrEntRepositoryExplorerPage win = new ZephyrEntRepositoryExplorerPage();
                win.xCreateBusinessFlowFolder.Visibility = Visibility.Collapsed;//no need to create separate folder
                uploadPath = win.ShowAsWindow(eWindowShowStyle.Dialog);
            }
            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            if (matchingTC == null)
            {
                ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
                ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);
                Dictionary<string, string> testInstanceFields = CleanUnrelvantFields(allFields, EntityName.testcase);

                Reporter.ToLog(eLogLevel.INFO, "Starting export to Zephyr Ent");
                currentTC = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateTestCase(Convert.ToInt64(uploadPath), activtiesGroup, testInstanceFields);
                // Check if BF already exist and Mapped new TC to Zephyr TS
                if (!String.IsNullOrEmpty(businessFlow.ExternalID))
                {
                    ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).AssigningTestCasesToCyclePhase(new List<long>() { currentTC.id }, Convert.ToInt64(businessFlow.ExternalID2), Convert.ToInt64(businessFlow.ExternalID));
                }
            }
            else
            {
                currentTC = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).UpdateTestCase(Convert.ToInt64(uploadPath), activtiesGroup, matchingTC);
            }
            Reporter.HideStatusMessage();

            if (currentTC != null)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);
                    Reporter.HideStatusMessage();
                }
                tcsRepositoryList.Add(currentTC);
                return true;
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, false);
            }

            return false;
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            throw new NotImplementedException();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            tcsRepositoryList = new List<TestCaseResource>();
            if (businessFlow == null)
            {
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            List<BaseResponseItem> matchingTS = null;

            Amdocs.Ginger.Common.eUserMsgSelection userSelec = Amdocs.Ginger.Common.eUserMsgSelection.None;
            //check if the businessFlow already mapped to Zephyr ent. Test Set
            if (!String.IsNullOrEmpty(businessFlow.ExternalID))
            {
                matchingTS = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntPhaseById(Convert.ToInt32(businessFlow.ExternalID2));
                if (matchingTS != null && matchingTS.Count > 0)
                {
                    //ask user if want to continute
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS[0].TryGetItem("name").ToString());
                    if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                    {
                        return false;
                    }
                    else if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    {
                        matchingTS = null;
                    }
                }
            }


            //check if all of the business flow activities groups already exported to zephyr and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                matchingTC = null;
                //check if the ActivitiesGroup already mapped to zephyr Test Case
                if (!String.IsNullOrEmpty(ag.ExternalID))
                {
                    matchingTC = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntTest(ag.ExternalID2);
                    if (matchingTC != null && matchingTC.Count > 0)
                    {
                        //ask user if want to continue
                        Amdocs.Ginger.Common.eUserMsgSelection userSelect = Reporter.ToUser(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, ag.Name, matchingTC[0].TryGetItem("name").ToString());
                        if (userSelect == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                        {
                            return false;
                        }
                        else if (userSelect == Amdocs.Ginger.Common.eUserMsgSelection.No)
                        {
                            matchingTC = null;
                        }
                        else
                        {
                            if (String.IsNullOrEmpty(testPlanUploadPath))
                            {
                                testPlanUploadPath = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetRepositoryTreeIdByTestcaseId(Convert.ToInt32(ag.ExternalID)).ToString();
                            }
                        }
                    }
                }

                //if user selected No and want to create new testplans to selected folder path
                if (matchingTC == null && String.IsNullOrEmpty(testPlanUploadPath))
                {
                    //get the zephyr Test repository path to upload the activities group to
                    testPlanUploadPath = SelectALMTestPlanPath();
                    if (String.IsNullOrEmpty(testPlanUploadPath))
                    {
                        //no path to upload to
                        return false;
                    }
                    //create upload path if checked to create separete folder
                    if (TestRepositoryFolderTreeItem.IsCreateBusinessFlowFolder)
                    {
                        try
                        {
                            TreeNode treeNode = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateTreeNode();
                            testPlanUploadPath = treeNode.id.ToString();
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Failed to get create folder for Test Repository with Zephyr Ent.", ex);
                        }
                    }
                }

                ExportActivitiesGroupToALM(ag, testPlanUploadPath, false, businessFlow);
            }

            if (matchingTS == null && string.IsNullOrEmpty(testLabUploadPath))
            {
                if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    Reporter.ToUser(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder);
                }

                //get the zephyr Test Planning path to upload the activities group to
                string uploadData = SelectALMTestLabPath();
                if (String.IsNullOrEmpty(uploadData))
                {
                    //no path to upload to
                    return false;
                }
                string[] getTypeAndId = uploadData.Split('#');
                testLabUploadPath = getTypeAndId[1];
                bfEntityType = getTypeAndId[0];
                moduleParentId = getTypeAndId[2] == null ? string.Empty : getTypeAndId[2];
                folderCycleId = getTypeAndId[3];
            }

            //upload the business flow
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            Dictionary<string, string> testSetFieldsFields = CleanUnrelvantFields(allFields, EntityName.cycle);
            Dictionary<string, string> testInstanceFields = CleanUnrelvantFields(allFields, EntityName.testcase);

            bool exportRes = ExportBusinessFlowToTestPlanning(businessFlow, matchingTS, testLabUploadPath, testSetFieldsFields, testInstanceFields, ref res);
            Reporter.HideStatusMessage();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.HideStatusMessage();
                }
                if (almConectStyle != eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
                return true;
            }
            else
            {
                if (almConectStyle != eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
                }
            }

            return exportRes;
        }
        private Dictionary<string, string> CleanUnrelvantFields(ObservableList<ExternalItemFieldBase> fields, EntityName entityName)
        {
            ObservableList<ExternalItemFieldBase> fieldsToReturn = new ObservableList<ExternalItemFieldBase>();
            Dictionary<string, string> selectedFields = new Dictionary<string, string>();
            string currentResource = entityName.ToString();
            fields.ToList().ForEach(item =>
            {
                if (item.ItemType.ToLower().Equals(entityName.ToString().ToLower()))
                {
                    fieldsToReturn.Add(item);
                }
            });
            fieldsToReturn.ToList().ForEach(tst =>
            {
                if (!String.IsNullOrEmpty(tst.ID) && !String.IsNullOrEmpty(tst.SelectedValue) && !selectedFields.ContainsKey(tst.ID))
                {
                    int index = tst.SelectedValue.IndexOf("#");
                    selectedFields.Add(tst.ID, index == -1 ? tst.SelectedValue : tst.SelectedValue.Remove(tst.SelectedValue.IndexOf("#")));
                }
            });
            return selectedFields;
        }
        private bool ExportBusinessFlowToTestPlanning(BusinessFlow businessFlow, List<BaseResponseItem> matchingTS, string testLabUploadPath, Dictionary<string, string> testSetFieldsFields, Dictionary<string, string> testInstanceFields, ref string res)
        {
            Cycle cycle = null;
            CyclePhase cyclePhase = null;
            long folderId;
            long tcrCatalogTreeId;
            long testerId = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetCurrentUser();
            bool isUpdate = false;
            // Update testset
            if (matchingTS != null && matchingTS.Count > 0)
            {
                testLabUploadPath = businessFlow.ExternalID;
                moduleParentId = businessFlow.ExternalID2;
                bfEntityType = matchingTS[0].TryGetItem("type").ToString();
                if (bfEntityType.Equals(EntityFolderType.Phase.ToString()))
                {
                    folderCycleId = moduleParentId;
                }
                isUpdate = true;
            }
            if (String.IsNullOrEmpty(testLabUploadPath))
            {
                cycle = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateNewTestCycle();
                cyclePhase = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateNewTestCyclePhase(cycle, businessFlow.Name);
                folderId = cyclePhase.id;
                tcrCatalogTreeId = cyclePhase.tcrCatalogTreeId;
            }
            else if (bfEntityType.Equals(EntityFolderType.Cycle.ToString()) || bfEntityType.Equals(EntityFolderType.CyclePhase.ToString()))
            {
                cycle = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntCycleById(Convert.ToInt32(testLabUploadPath));
                cyclePhase = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateNewTestCyclePhase(cycle, businessFlow.Name);
                folderId = cyclePhase.id;
                tcrCatalogTreeId = cyclePhase.tcrCatalogTreeId;
            }
            else if (bfEntityType.Equals(EntityFolderType.Phase.ToString()))
            {
                List<BaseResponseItem> phase = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntPhaseById(Convert.ToInt32(folderCycleId));
                BaseResponseItem item = phase.FirstOrDefault(md => md.id.ToString().Equals(testLabUploadPath));
                if (isUpdate)
                {
                    ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).UpdateTestPlanningFolder(Convert.ToInt64(folderCycleId), Convert.ToInt64(item.TryGetItem("id")), businessFlow);
                    return true;
                }
                dynamic treeNode = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateNewTestPlanningFolder(Convert.ToInt64(folderCycleId), Convert.ToInt64(item.TryGetItem("id"))
                    , businessFlow.Name, String.IsNullOrEmpty(businessFlow.Description) ? businessFlow.Name + " description" : businessFlow.Description);
                folderId = Convert.ToInt64(folderCycleId);
                tcrCatalogTreeId = treeNode.id;
            }
            else
            {
                List<BaseResponseItem> module = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTreeByCretiria(EntityFolderType.Module.ToString(), Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), 180, Convert.ToInt32(moduleParentId));
                BaseResponseItem item = module.FirstOrDefault(md => md.id.ToString().Equals(testLabUploadPath));
                if (isUpdate)
                {
                    ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).UpdateTestPlanningFolder(Convert.ToInt64(folderCycleId), Convert.ToInt64(item.TryGetItem("id")), businessFlow);
                    return true;
                }
                dynamic treeNode = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).CreateNewTestPlanningFolder(Convert.ToInt64(folderCycleId), Convert.ToInt64(item.TryGetItem("id"))
                    , businessFlow.Name, String.IsNullOrEmpty(businessFlow.Description) ? businessFlow.Name + " description" : businessFlow.Description);
                folderId = Convert.ToInt64(folderCycleId);
                tcrCatalogTreeId = treeNode.id;
            }
            ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).AssigningTestCasesToCyclePhase(tcsRepositoryList.Select(z => z.id).ToList(),
                folderId, tcrCatalogTreeId);
            List<TestCaseResource> tcsPlanningList
                = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTestCasesByAssignmentTree((int)tcrCatalogTreeId);
            List<Execution> assignsList = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).AssigningTestCasesToTesterForExecution(
                tcsPlanningList.Select(z => z.tct.id).ToList(), folderId, testerId, tcrCatalogTreeId);
            ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ExecuteTestCases(assignsList, testerId, businessFlow.ActivitiesGroups);
            businessFlow.ExternalID = tcrCatalogTreeId.ToString();
            businessFlow.ExternalID2 = folderId.ToString();
            return true;
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            return ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTestLabExplorer(path);
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            return new List<string>();
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override object GetTSRunStatus(object tsItem)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from Zephyr");
            //set path to import to               
            if (importDestinationFolderPath == "")
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }
            //show Test Lab browser for selecting the Test Set/s to import
            ZephyrEntPlanningExplorerPage win = new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<object> selectedTestSets)
        {
            if (selectedTestSets != null && selectedTestSets.Count() > 0)
            {
                ObservableList<ZephyrEntPhaseTreeItem> testSetsItemsToImport = new ObservableList<ZephyrEntPhaseTreeItem>();
                foreach (ZephyrEntPhaseTreeItem testSetItem in selectedTestSets)
                {
                    //check if some of the Test Set was already imported                
                    if (testSetItem.AlreadyImported)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, testSetItem.Name);
                        if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            //Delete the mapped BF
                            File.Delete(testSetItem.MappedBusinessFlow.FileName);
                            testSetsItemsToImport.Add(testSetItem);
                        }
                    }
                    else
                    {
                        testSetsItemsToImport.Add(testSetItem);
                    }
                }

                if (testSetsItemsToImport.Count == 0)
                {
                    return false; //noting to import
                }

                //Refresh Ginger repository
                ALMIntegration.Instance.AlmCore.InitCoreObjs();

                foreach (ZephyrEntPhaseTreeItem testSetItemtoImport in testSetsItemsToImport)
                {
                    try
                    {
                        //import test set data
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testSetItemtoImport.Name);
                        GingerCore.ALM.QC.ALMTestSet TS = new GingerCore.ALM.QC.ALMTestSet();
                        TS.TestSetID = testSetItemtoImport.Id;
                        TS.TestSetName = testSetItemtoImport.Name;
                        TS.TestSetPath = testSetItemtoImport.Path;
                        TS = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);
                        if (testSetItemtoImport.entityType == EntityFolderType.Cycle)
                        {
                            testSetItemtoImport.FatherId = testSetItemtoImport.TestSetID;
                        }
                        tsBusFlow.ExternalID2 = testSetItemtoImport.FatherId;
                        if (WorkSpace.Instance.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (!string.IsNullOrEmpty(activ.TargetApplication))
                                {
                                    if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                                    {
                                        ApplicationPlatform appAgent = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                        if (appAgent != null)
                                        {
                                            tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                                        }
                                    }
                                }
                            }
                            //handle non mapped Activities
                            if (tsBusFlow.TargetApplications.Count == 0)
                            {
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                            }
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (string.IsNullOrEmpty(activ.TargetApplication))
                                {
                                    activ.TargetApplication = tsBusFlow.MainApplication;
                                }
                            }
                        }
                        else
                        {
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                activ.TargetApplication = null; // no app configured on solution level
                            }
                        }

                        AddTestSetFlowToFolder(tsBusFlow, importDestinationPath);

                        Reporter.HideStatusMessage();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, testSetItemtoImport.Name, ex.Message);
                        Reporter.ToLog(eLogLevel.ERROR, "Error importing from Zephyr Ent", ex);
                    }
                }

                Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);

                Reporter.ToLog(eLogLevel.DEBUG, "Imported from Zephyr Ent successfully");
                return true;
            }
            Reporter.ToLog(eLogLevel.ERROR, "Error importing from Zephyr Ent");
            return false;
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestLabPath()
        {
            //show Test Lab browser for selecting the Test Set/s
            ZephyrEntPlanningExplorerPage win = new ZephyrEntPlanningExplorerPage(ZephyrEntPlanningExplorerPage.eExplorerTestPlanningPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override string SelectALMTestPlanPath()
        {
            //show Test Plan browser for selecting the Path
            ZephyrEntRepositoryExplorerPage win = new ZephyrEntRepositoryExplorerPage();
            return win.ShowAsWindow(eWindowShowStyle.Dialog);
        }
        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            throw new NotImplementedException();
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            return; // todo refresh
        }
        public override Page GetALMTestSetsTreePage(string importDestinationPath = "")
        {
            return new ZephyrEntPlanningExplorerPage(eExplorerTestPlanningPageUsageType.Import, importDestinationPath);
        }
        public override Object GetSelectedImportTestSetData(Page page)
        {
            return (page as ZephyrEntPlanningExplorerPage).CurrentSelectedTestSets.FirstOrDefault();
        }
        public override void GetALMTestSetData(ALMTestSet almTestSet)
        {
            try
            {
                //Get Test Set Name
                if (almTestSet.TestSetID != almTestSet.TestSetInternalID2)
                {
                    TreeNode treeNode = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetTestRepositoryFolderById(Convert.ToInt32(almTestSet.TestSetID));
                    if(treeNode != null)
                    {
                        almTestSet.TestSetName = treeNode.name;
                    }
                }
                else
                {
                    List<BaseResponseItem>  testSetResponseItems = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).GetZephyrEntPhaseById(Convert.ToInt32(almTestSet.TestSetID));
                    if (testSetResponseItems is not null && testSetResponseItems.Count > 0)
                    {
                        almTestSet.TestSetName = testSetResponseItems[0].TryGetItem("name").ToString();
                    }
                }
                
                // Add test cases
                almTestSet = ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(almTestSet);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to get Test Set data, {System.Reflection.MethodBase.GetCurrentMethod().Name}: Error - {ex.Message} ");
            }
        }
        public override ALMTestSet GetALMTestCasesToTestSetObject(ALMTestSet almTestSet)
        {
            return ((ZephyrEntCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(almTestSet);
        }
    }
}

