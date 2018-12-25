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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger.ALM.QC.TreeViewItems;
using Ginger.ALM.RQM;
using Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerCore.ALM.RQM;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows;

namespace Ginger.ALM.Repository
{
    class RQMRepository : ALMRepository
    {
        const string RQMID = "RQMID";
        const string RQMScriptID = "RQMScriptID";
        const string RQMRecordID = "RQMRecordID";
        const string BtsID = "BtsID";

        public override void ImportALMTests(String importDestinationFolderPath) //Import Test Plans
        {
            RQMPlansExplorerPage win = new RQMPlansExplorerPage(importDestinationFolderPath);
            win.ShowAsWindow();
        }
        public override void ImportALMTestsById(String importDestinationFolderPath) //Import Test Plans
        {
            RQMImportPlanByIdPage win = new RQMImportPlanByIdPage(importDestinationFolderPath);
            win.ShowAsWindow();
        }

        public override bool ShowImportReviewPage(string importDestinationFolderPath, object selectedTestPlan = null)
        {
            if (importDestinationFolderPath == "")
                importDestinationFolderPath = App.UserProfile.Solution.BusinessFlowsMainFolder;

            // get activities groups
            RQMImportReviewPage win = new RQMImportReviewPage(RQMConnect.Instance.GetRQMTestPlanFullData(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, (RQMTestPlan)selectedTestPlan), importDestinationFolderPath);
            win.ShowAsWindow();

            return true;
        }

        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Connecting to RQM server");
            try
            {
                isConnectSucc = ALMIntegration.Instance.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error connecting to RQM server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Could not connect to RQM server");
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailure);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings);
            }

            return isConnectSucc;
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> testPlanList)
        {
            if (testPlanList != null)
            {
                foreach (RQMTestPlan testPlan in testPlanList)
                {
                    //Refresh Ginger repository and allow GingerRQM to use it

                    ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                    ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

                    try
                    {
                        BusinessFlow existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == RQMID + "=" + testPlan.RQMID).FirstOrDefault();
                        if (existedBF != null)
                        {
                            MessageBoxResult userSelection = Reporter.ToUser(eUserMsgKeys.TestSetExists, testPlan.Name);
                            if (userSelection == MessageBoxResult.Yes)
                            {
                                File.Delete(existedBF.FileName);
                            }
                        }

                        Reporter.ToGingerHelper(eGingerHelperMsgKey.ALMTestSetImport, null, testPlan.Name);

                        // convert test set into BF
                        BusinessFlow tsBusFlow = ALMIntegration.Instance.AlmCore.ConvertRQMTestPlanToBF(testPlan);

                        if (App.UserProfile.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                                if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                                    if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                                    {
                                        ApplicationPlatform appAgent = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                        if (appAgent != null)
                                            tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                                    }
                            //handle non mapped Activities
                            if (tsBusFlow.TargetApplications.Count == 0)
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
                            foreach (Activity activ in tsBusFlow.Activities)
                                if (string.IsNullOrEmpty(activ.TargetApplication))
                                    activ.TargetApplication = tsBusFlow.MainApplication;
                        }
                        else
                        {
                            foreach (Activity activ in tsBusFlow.Activities)
                                activ.TargetApplication = null; // no app configured on solution level
                        }
                        
                        WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tsBusFlow);                        
                        Reporter.CloseGingerHelper();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKeys.ErrorInTestsetImport, testPlan.Name, ex.Message);
                    }

                    Reporter.ToUser(eUserMsgKeys.TestSetsImportedSuccessfully);
                }
                return true;
            }
            return false;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            foreach (RQMTestPlan testPlan in RQMConnect.Instance.GetRQMTestPlansByProject(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\ALM\RQM_Configs")).OrderByDescending(item => item.CreationDate))
            {
                if (testPlan.RQMID == ExportToRQM.GetExportedIDString(businessFlow.ExternalID, "RQMID"))
                {
                    RQMTestPlan currentRQMTestPlan = RQMConnect.Instance.GetRQMTestPlanFullData(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, (RQMTestPlan)testPlan);
                    ((RQMCore)ALMIntegration.Instance.AlmCore).UpdatedRQMTestInBF(ref businessFlow, currentRQMTestPlan, TCsIDs.Select(x => x.Item1.ToString()).ToList());
                }
            }
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            foreach (RQMTestPlan testPlan in RQMConnect.Instance.GetRQMTestPlansByProject(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\ALM\RQM_Configs")).OrderByDescending(item => item.CreationDate))
            {
                if (testPlan.RQMID == ExportToRQM.GetExportedIDString(businessFlow.ExternalID, "RQMID"))
                {
                    RQMTestPlan currentRQMTestPlan = RQMConnect.Instance.GetRQMTestPlanFullData(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, (RQMTestPlan)testPlan);
                    ((RQMCore)ALMIntegration.Instance.AlmCore).UpdateBusinessFlow(ref businessFlow, currentRQMTestPlan);
                }
            }
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            if (businessFlow == null) return;

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                return;
            }

            bool exportRes = false;

            string res = string.Empty;
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, "Selected Activities Groups");
            exportRes = ((RQMCore)ALMIntegration.Instance.AlmCore).ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups, ref res);

            if (exportRes)
            {
                //Check if we need to perform save
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.CloseGingerHelper();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null) return false;

            if (App.UserProfile.Solution.ExternalItemsFields.Where(x => x.ItemType == "TestCase").ToList().Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Current solution have no pre-difined values for RQM's mandatory fieds. Please configure before doing export. ('ALM'-'ALM Items Fields Configuration')");
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            bool exportRes = false;
            string res = string.Empty;
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, businessFlow.Name);

            exportRes = ((RQMCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToRQM(businessFlow, App.UserProfile.Solution.ExternalItemsFields, ref res);

            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);

                }
                if(almConectStyle != ALMIntegration.eALMConnectType.Auto && almConectStyle != ALMIntegration.eALMConnectType.Silence)
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.CloseGingerHelper();
            return exportRes;
        }

        #region External Item Fields

        public override eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKeys.AskIfToDownloadPossibleValues;
        }

        #endregion External Item Fields
        #region RQM Configurations Package
        public override bool LoadALMConfigurations()
        {
            System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
            dlg.DefaultExt = "*.zip";
            dlg.Filter = "zip Files (*.zip)|*.zip";
            dlg.Title = "Select RQM Configuration Zip File";

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if(!((RQMCore)ALMIntegration.Instance.AlmCore).ValidateConfigurationFile(dlg.FileName))
                    return false;

                string folderPath = Path.Combine(App.UserProfile.Solution.Folder, "Configurations");
                DirectoryInfo di = Directory.CreateDirectory(folderPath);

                folderPath = Path.Combine(folderPath, "RQMServerConfigurationsPackage");
                if (Directory.Exists(folderPath))
                    DeleteDirectoryAndFiles(folderPath);

                ZipFile.ExtractToDirectory(dlg.FileName, di.FullName);
                if (!((RQMCore)ALMIntegration.Instance.AlmCore).IsConfigPackageExists())
                    return false;

                ALMIntegration.Instance.SetALMCoreConfigurations();
            }
            return true; //Browse Dialog Canceled
        }

        private void DeleteDirectoryAndFiles(string directory)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        #endregion
        public override IEnumerable<Object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            return "";
        }

        public override string SelectALMTestLabPath()
        {
            return "";
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Object> GetTestSetExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override Object GetTSRunStatus(Object tsItem)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            throw new NotImplementedException();
        }
    }
}
