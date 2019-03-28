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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ALM_Common.DataContracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.JIRA;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Platforms;
using System.IO;
using System.IO.Compression;


namespace Ginger.ALM.Repository
{
    class JIRA_Repository : ALMRepository
    {

        public ALMCore AlmCore { get; set; }

        public JIRA_Repository(ALMCore almCore)
        {
            this.AlmCore = almCore;
        }
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eLogLevel.INFO, "Connecting to Jira server");
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
                Reporter.ToLog(eLogLevel.INFO, "Could not connect to Jira server");
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailure);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings);
            }

            return isConnectSucc;
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
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

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
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
                    var testCaseFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_CASE.ToString())&&(a.ToUpdate || a.Mandatory));
                    var testSetFields = allFields.Where(a => a.ItemType == (ResourceType.TEST_SET.ToString()) && (a.ToUpdate || a.Mandatory));
                    var testExecutionFields = allFields.Where(a => a.ItemType == "TEST_EXECUTION" && (a.ToUpdate || a.Mandatory));
                    var exportRes = ((JiraCore)this.AlmCore).ExportBfToAlm(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr);
                    if (exportRes)
                    {
                        if (performSaveAfterExport)
                        {
                            Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                            Reporter.HideStatusMessage();
                        }
                        if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                            Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                        return true;
                    }
                    else
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                        Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, responseStr);
                }
            }
            return result;
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
            JIRA.JiraImportReviewPage win = new JIRA.JiraImportReviewPage();
            win.ShowAsWindow();
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
                        
                        SetImportedTS(jiraImportedTSData);
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

        private void SetImportedTS(JiraTestSet importedTS)
        {
            ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            try
            {
                //import test set data
                Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, importedTS.Name);
                BusinessFlow tsBusFlow = ((JiraCore)ALMIntegration.Instance.AlmCore).ConvertJiraTestSetToBF(importedTS);
                SetBFPropertiesAfterImport(tsBusFlow);

                //save bf
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tsBusFlow);
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
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = "*.zip";
            dlg.Filter = "zip Files (*.zip)|*.zip";
            dlg.Title = "Select Jira Configuration Zip File";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (!((JiraCore)ALMIntegration.Instance.AlmCore).ValidateConfigurationFile(dlg.FileName))
                    return false;

                string folderPath = Path.Combine(WorkSpace.Instance.Solution.Folder, "Configurations");
                DirectoryInfo di = Directory.CreateDirectory(folderPath);

                folderPath = Path.Combine(folderPath, "JiraConfigurationsPackage");
                if (Directory.Exists(folderPath))
                    Amdocs.Ginger.Common.GeneralLib.General.ClearDirectoryContent(folderPath);

                ZipFile.ExtractToDirectory(dlg.FileName, folderPath);
                if (!((JiraCore)ALMIntegration.Instance.AlmCore).IsConfigPackageExists())
                    return false;

                ALMIntegration.Instance.SetALMCoreConfigurations();
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
