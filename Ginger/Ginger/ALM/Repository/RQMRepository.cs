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
using Ginger.ALM.RQM;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.RQM;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

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
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }
            // get activities groups
            RQMImportReviewPage win = new RQMImportReviewPage(RQMConnect.Instance.GetRQMTestPlanFullData(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectKey, (RQMTestPlan)selectedTestPlan), importDestinationFolderPath);
            win.ShowAsWindow();

            return true;
        }

        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eLogLevel.DEBUG, "Connecting to RQM server");
            try
            {
                isConnectSucc = ALMIntegration.Instance.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error connecting to RQM server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eLogLevel.WARN, "Could not connect to RQM server");
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

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> testPlanList)
        {
            if (testPlanList != null)
            {
                foreach (RQMTestPlan testPlan in testPlanList)
                {
                    //Refresh Ginger repository and allow GingerRQM to use it
                    ALMIntegration.Instance.AlmCore.InitCoreObjs();

                    try
                    {
                        BusinessFlow existedBF = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.ExternalID == RQMID + "=" + testPlan.RQMID).FirstOrDefault();
                        if (existedBF != null)
                        {
                            eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, testPlan.Name);
                            if (userSelection == eUserMsgSelection.Yes)
                            {
                                File.Delete(existedBF.FileName);
                            }
                        }

                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testPlan.Name);

                        // convert test set into BF
                        BusinessFlow tsBusFlow = ((RQMCore)ALMIntegration.Instance.AlmCore).ConvertRQMTestPlanToBF(testPlan);

                        if ( WorkSpace.Instance.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (string.IsNullOrEmpty(activ.TargetApplication) == false)
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
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, testPlan.Name, ex.Message);
                    }

                    Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);
                }
                return true;
            }
            return false;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            foreach (RQMTestPlan testPlan in RQMConnect.Instance.GetRQMTestPlansByProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName, System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\ALM\RQM_Configs")).OrderByDescending(item => item.CreationDate))
            {
                if (testPlan.RQMID == ExportToRQM.GetExportedIDString(businessFlow.ExternalID, "RQMID"))
                {
                    RQMTestPlan currentRQMTestPlan = RQMConnect.Instance.GetRQMTestPlanFullData(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName, testPlan);
                    ((RQMCore)ALMIntegration.Instance.AlmCore).UpdatedRQMTestInBF(ref businessFlow, currentRQMTestPlan, TCsIDs.Select(x => x.Item1.ToString()).ToList());
                }
            }
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            foreach (RQMTestPlan testPlan in RQMConnect.Instance.GetRQMTestPlansByProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName, System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\ALM\RQM_Configs")).OrderByDescending(item => item.CreationDate))
            {
                if (testPlan.RQMID == ExportToRQM.GetExportedIDString(businessFlow.ExternalID, "RQMID"))
                {
                    RQMTestPlan currentRQMTestPlan = RQMConnect.Instance.GetRQMTestPlanFullData(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName, testPlan);
                    ((RQMCore)ALMIntegration.Instance.AlmCore).UpdateBusinessFlow(ref businessFlow, currentRQMTestPlan);
                }
            }
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            if (businessFlow == null)
            {
                return;
            }
            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }

            bool exportRes = false;

            string res = string.Empty;
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, "Selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups));
            exportRes = ((RQMCore)ALMIntegration.Instance.AlmCore).ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups, ref res);

            if (exportRes)
            {
                //Check if we need to perform save
                Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.HideStatusMessage();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null)
            {
                return false;
            }
            if ( WorkSpace.Instance.Solution.ExternalItemsFields.Where(x => x.ItemType == "TestCase").ToList().Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Current solution have no predefined values for RQM's mandatory fields. Please configure before doing export. ('ALM'-'ALM Items Fields Configuration')");
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            bool exportRes = false;
            string res = string.Empty;
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);

            exportRes = ((RQMCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToRQM(businessFlow,  WorkSpace.Instance.Solution.ExternalItemsFields, ref res);

            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);

                }
                if (almConectStyle != eALMConnectType.Auto && almConectStyle != eALMConnectType.Silence)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.HideStatusMessage();
            return exportRes;
        }

        #region External Item Fields

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValues;
        }

        #endregion External Item Fields
        #region RQM Configurations Package
        public override bool LoadALMConfigurations()
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.zip",
                Filter = "zip Files (*.zip)|*.zip",
                Title = "Select RQM Configuration Zip File"
            }, false) is string fileName)
            {
                if (!((RQMCore)ALMIntegration.Instance.AlmCore).ValidateConfigurationFile(fileName))
                    return false;

                string folderPath = Path.Combine( WorkSpace.Instance.Solution.Folder, "Configurations");
                DirectoryInfo di = Directory.CreateDirectory(folderPath);

                folderPath = Path.Combine(folderPath, "RQMServerConfigurationsPackage");
                if (Directory.Exists(folderPath))
                    DeleteDirectoryAndFiles(folderPath);

                ZipFile.ExtractToDirectory(fileName, di.FullName);
                if (!((RQMCore)ALMIntegration.Instance.AlmCore).IsConfigPackageExists())
                    return false;

                ALMIntegration.Instance.SetALMCoreConfigurations(GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.RQM);
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

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
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
