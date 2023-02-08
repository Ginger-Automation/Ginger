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

using AlmDataContractsStd.Enums;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM.JIRA;
using Ginger.ALM.JIRA.TreeViewItems;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.JIRA;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using JiraRepositoryStd.Data_Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM.Repository
{
    class JIRA_Repository : ALMRepository
    {

        public ALMCore AlmCore { get; set; }

        public JIRA_Repository(ALMCore almCore)
        {
            this.AlmCore = almCore;
        }
        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Jira server");
            try
            {
                isConnectSucc = this.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error connecting to Jira server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Could not connect to Jira server");
                if (userMsgStyle == eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailure);
                }
                else if (userMsgStyle == eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings);
                }
            }

            return isConnectSucc;
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
        {
            bool result = false;
            string responseStr=string.Empty;
            if (activtiesGroup != null)
            {
            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
                var testCaseFields = allFields.Where(a => a.ItemType == ResourceType.TEST_CASE.ToString());
                bool exportRes = ((JiraCore)this.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, testCaseFields, ref responseStr);

                Reporter.HideStatusMessage();
                if (exportRes)
                {
                    if (performSaveAfterExport)
                    {
                        Reporter.ToStatus(eStatusMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);
                        Reporter.HideStatusMessage();
                    }
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                    return true;
                }
                else
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, responseStr);
            }
            return result;
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            bool askToSaveBF = false;
            foreach (ActivitiesGroup group in grdActivitiesGroups)
            {
                if (ExportActivitiesGroupToALM(group))
                    askToSaveBF = true;
            }

            if (askToSaveBF)
                if (Reporter.ToUser(eUserMsgKey.AskIfToSaveBFAfterExport, businessFlow.Name) == eUserMsgSelection.Yes)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name,
                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.HideStatusMessage();
                }
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            bool result = false;
            string responseStr = string.Empty;

            if (businessFlow != null)
            {
                if (businessFlow.ActivitiesGroups.Count == 0)
                {
                    Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                    return false;
                }
                else
                {
                    ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
                    ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);
                    var testCaseFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_CASE.ToString()) && (a.ToUpdate || a.Mandatory));
                    var testSetFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_SET.ToString()) && (a.ToUpdate || a.Mandatory));
                    var testExecutionFields = allFields.Where(a => a.ItemType == "TEST_EXECUTION" && (a.ToUpdate || a.Mandatory));
                    Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
                    bool exportRes = false;
                    switch (ALMCore.DefaultAlmConfig.JiraTestingALM)
                    {
                        case eTestingALMType.Xray:
                            exportRes = ((JiraCore)this.AlmCore).ExportBfToAlm(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr);
                            break;
                        case eTestingALMType.Zephyr:
                            JiraZephyrTreeItem zephyrExportPath = SelectZephyrExportPath();
                            if (zephyrExportPath == null)
                            {
                                return true;
                            }
                            if (zephyrExportPath is JiraZephyrVersionTreeItem)
                            {
                                exportRes = ((JiraCore)this.AlmCore).ExportBfToZephyr(businessFlow, testCaseFields, testSetFields,
                                                                                        testExecutionFields, ref responseStr,
                                                                                        ((JiraZephyrVersionTreeItem)zephyrExportPath).VersionId.ToString(), string.Empty);
                            }
                            else if (zephyrExportPath is JiraZephyrCycleTreeItem)
                            {
                                exportRes = ((JiraCore)this.AlmCore).ExportBfToZephyr(businessFlow, testCaseFields, testSetFields,
                                                                                        testExecutionFields, ref responseStr,
                                                                                        ((JiraZephyrCycleTreeItem)zephyrExportPath).VersionId.ToString(),
                                                                                        ((JiraZephyrCycleTreeItem)zephyrExportPath).Id.ToString());
                            }
                            break;
                        default:
                            exportRes = ((JiraCore)this.AlmCore).ExportBfToAlm(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr);
                            break;
                    }

                    if (exportRes)
                    {
                        if (performSaveAfterExport)
                        {
                            Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                            Reporter.HideStatusMessage();
                        }
                        if (almConectStyle != eALMConnectType.Auto && almConectStyle != eALMConnectType.Silence)
                        {
                            Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                        }
                        return true;
                    }
                    else if (almConectStyle != eALMConnectType.Auto && almConectStyle != eALMConnectType.Silence)
                    {
                        Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, responseStr);
                    }
                }
                Reporter.HideStatusMessage();
            }
            return result;
        }

        public JiraZephyrTreeItem SelectZephyrExportPath()
        {
            //show Test Cycles browser for selecting the Path
            JiraZephyrCyclesExplorerPage win = new JiraZephyrCyclesExplorerPage(string.Empty, true);
            object selectedPathObject = win.ShowAsWindow(eWindowShowStyle.Dialog);
            if (selectedPathObject is JiraZephyrTreeItem)
            {
                return (JiraZephyrTreeItem)selectedPathObject;
            }
            else
            {
                return null;
            }
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            return ((JiraCore)this.AlmCore).GetJiraTestSets();
        }

        public override object GetTSRunStatus(object tsItem)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            if (ALMCore.DefaultAlmConfig.JiraTestingALM == eTestingALMType.Xray)
            {
                JIRA.JiraImportReviewPage win = new JIRA.JiraImportReviewPage(importDestinationPath:importDestinationFolderPath);
                win.ShowAsWindow();
            }
            if (ALMCore.DefaultAlmConfig.JiraTestingALM == eTestingALMType.Zephyr)
            {
                JIRA.JiraZephyrCyclesExplorerPage win = new JIRA.JiraZephyrCyclesExplorerPage(importDestinationFolderPath);
                win.ShowAsWindow();
            }
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            JIRA.JiraImportSetByIdPage win = new JIRA.JiraImportSetByIdPage();
            win.ShowAsWindow();
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<object> selectedTests)
        {
            if (selectedTests != null && selectedTests.Count() > 0)
            {
                ObservableList<JiraTestSet> testSetsItemsToImport = new ObservableList<JiraTestSet>();
                foreach (GingerCore.ALM.JIRA.JiraTestSet selectedTS in selectedTests)
                {
                    try
                    {
                        BusinessFlow existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == selectedTS.Key).FirstOrDefault();
                        if (existedBF != null)
                        {
                            Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, selectedTS.Name);
                            if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                            {
                                File.Delete(existedBF.FileName);
                            }
                        }
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, selectedTS.Name);
                        JiraTestSet jiraImportedTSData = ((JiraCore)this.AlmCore).GetJiraTestSetData(selectedTS);
                        
                        SetImportedTS(jiraImportedTSData, importDestinationPath);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, selectedTS.Name, ex.Message);
                    }
                }
                Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);
                return true;
            }
            return false;
        }

        public bool ImportSelectedZephyrCyclesAndFolders(string importDestinationPath, IEnumerable<object> selectedObjects)
        {
            if (selectedObjects != null && selectedObjects.Count() > 0)
            {
                foreach (JiraZephyrTreeItem obj in selectedObjects)
                {
                    BusinessFlow existedBF;
                    if (obj is JiraZephyrFolderTreeItem)
                    {
                        existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == ((JiraZephyrFolderTreeItem)obj).CycleId  && x.ExternalID2 == obj.Id).FirstOrDefault();
                    }
                    else
                    {
                        existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == obj.Id && x.ExternalID2 == null).FirstOrDefault();
                    }

                    if (existedBF != null)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, obj.Name);
                        if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            File.Delete(existedBF.FileName);
                        }
                    }
                    Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, obj.Name);
                }

                //Refresh Ginger repository and allow GingerQC to use it
                ALMIntegration.Instance.AlmCore.InitCoreObjs();

                foreach (JiraZephyrTreeItem obj in selectedObjects)
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, obj.Name);
                        JiraZephyrCycle currentCycle;
                        BusinessFlow tsBusFlow;
                        if (obj is JiraZephyrFolderTreeItem)
                        {
                            currentCycle = ((JiraCore)ALMIntegration.Instance.AlmCore).GetZephyrCycleOrFolderWithIssuesAndStepsAsCycle(obj.VersionId, ((JiraZephyrFolderTreeItem)obj).CycleId, ((JiraZephyrFolderTreeItem)obj).Id);
                            currentCycle.name = obj.Name;
                            currentCycle.description = ((JiraZephyrFolderTreeItem)obj).Description;
                            tsBusFlow = ((JiraCore)ALMIntegration.Instance.AlmCore).ConvertJiraZypherCycleToBF(currentCycle);
                            tsBusFlow.ExternalID = ((JiraZephyrFolderTreeItem)obj).CycleId;
                            tsBusFlow.ExternalID2 = obj.Id;
                        }
                        else
                        {
                            currentCycle = ((JiraCore)ALMIntegration.Instance.AlmCore).GetZephyrCycleOrFolderWithIssuesAndStepsAsCycle(obj.VersionId, obj.Id);
                            tsBusFlow = ((JiraCore)ALMIntegration.Instance.AlmCore).ConvertJiraZypherCycleToBF(currentCycle);
                        }

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
                                activ.Active = true;
                            }
                        }
                        else
                        {
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                activ.TargetApplication = null; // no app configured on solution level
                            }
                        }

                        //save bf
                        AddTestSetFlowToFolder(tsBusFlow, importDestinationPath);
                        Reporter.HideStatusMessage();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, obj.Name, ex.Message);
                        Reporter.ToLog(eLogLevel.ERROR, "Error importing from Jira-Zephyr", ex);
                    }
                }

                Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);

                Reporter.ToLog(eLogLevel.DEBUG, "Imported from Jira-Zephyr successfully");
                return true;
            }
            Reporter.ToLog(eLogLevel.ERROR, "Error importing from Jira-Zephyr");
            return false;
        }

        private void SetImportedTS(JiraTestSet importedTS, string importDestinationPath)
        {
            ALMIntegration.Instance.AlmCore.InitCoreObjs();
            try
            {
                //import test set data
                Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, importedTS.Name);
                BusinessFlow tsBusFlow = ((JiraCore)ALMIntegration.Instance.AlmCore).ConvertJiraTestSetToBF(importedTS);
                SetBFPropertiesAfterImport(tsBusFlow);

                //save bf
                AddTestSetFlowToFolder(tsBusFlow, importDestinationPath);
                Reporter.HideStatusMessage();
            }
            catch { }
            
        }

        private void SetBFPropertiesAfterImport(BusinessFlow tsBusFlow)
        {
            if (WorkSpace.Instance.Solution.MainApplication != null)
            {
                //add the applications mapped to the Activities
                foreach (Activity activ in tsBusFlow.Activities)
                    if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                        if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                        {
                            ApplicationPlatform appAgent = WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                            if (appAgent != null)
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                        }
                //handle non mapped Activities
                if (tsBusFlow.TargetApplications.Count == 0)
                    tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                foreach (Activity activ in tsBusFlow.Activities)
                {
                    if (string.IsNullOrEmpty(activ.TargetApplication))
                        activ.TargetApplication = tsBusFlow.MainApplication;
                    activ.Active = true;
                }
            }
            else
            {
                foreach (Activity activ in tsBusFlow.Activities)
                    activ.TargetApplication = null; // no app configured on solution level
            }
        }

        public override bool LoadALMConfigurations()
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.zip",
                Filter = "zip Files (*.zip)|*.zip",
                Title = "Select Jira Configuration Zip File"
            }, false) is string fileName)
            {
                if (!GingerCore.General.LoadALMSettings(fileName, eALMType.Jira))
                {
                    return false;
                }
                ((JiraCore)ALMIntegration.Instance.AlmCore).CreateJiraRepository();
                ALMIntegration.Instance.SetALMCoreConfigurations(eALMType.Jira);
            }
            return true; //Browse Dialog Canceled
        }

        public override string SelectALMTestLabPath()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            return true;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            if (TCsIDs.Count > 0)
            {
                Dictionary<string,JiraTest> getActivitiesGroupUpdatedData = ((JiraCore)ALMIntegration.Instance.AlmCore).GetJiraTestsUpdatedData(businessFlow.ExternalID, TCsIDs.Select(x => x.Item1.ToString()).ToList());
                ((JiraCore)ALMIntegration.Instance.AlmCore).UpdateBFSelectedAG(ref businessFlow, getActivitiesGroupUpdatedData);
                SetBFPropertiesAfterImport(businessFlow);
            }
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            ((JiraCore)ALMIntegration.Instance.AlmCore).UpdateBussinessFlow(ref businessFlow);
            SetBFPropertiesAfterImport(businessFlow);
        }
    }
}
