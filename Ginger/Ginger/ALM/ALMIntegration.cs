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

using Amdocs.Ginger.Common;
using Ginger.ALM.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Repository;
using GingerCore.ALM.QC;
using GingerCore.ALM.QCRestAPI;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ALM
{
    public class ALMIntegration
    {
        public ALMCore AlmCore { get; set; }
        private ALMRepository AlmRepo;

        private ALMIntegration()
        {
            UpdateALMType(App.UserProfile.Solution.AlmType);
        }

        public void UpdateALMType(eALMType AlmType)
        {
            bool firstSync = false;
            if (AlmCore == null)
                firstSync = true;

            switch (AlmType)
            {
                case eALMType.QC:
                    if (!App.UserProfile.Solution.UseRest)
                    {
                        AlmCore = new QCCore();
                        AlmRepo = new QCRepository();
                    }
                    else
                    {
                        AlmCore = new QCRestAPICore();
                        AlmRepo = new QCRestAPIRepository();
                    }
                    break;

                case eALMType.RQM:
                    AlmCore = new RQMCore();
                    AlmRepo = new RQMRepository();
                    break;

                case eALMType.RALLY:
                    AlmCore = new RallyCore();
                    AlmRepo = new RallyRepository();
                    break;
            }
            if(firstSync)
                SetALMCoreConfigurations();
        }

        public void SetALMCoreConfigurations()
        {
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            AlmCore.SetALMConfigurations(App.UserProfile.Solution.ALMServerURL, App.UserProfile.Solution.UseRest,  App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMDomain, App.UserProfile.Solution.ALMProject);
            SyncConfigurations();
        }

        public void SyncConfigurations()
        {
            App.UserProfile.Solution.ALMServerURL = ALMCore.AlmConfig.ALMServerURL;
            App.UserProfile.Solution.UseRest = ALMCore.AlmConfig.UseRest;
            App.UserProfile.ALMUserName = ALMCore.AlmConfig.ALMUserName;
            App.UserProfile.ALMPassword = ALMCore.AlmConfig.ALMPassword;
            App.UserProfile.Solution.ALMDomain = ALMCore.AlmConfig.ALMDomain;
            App.UserProfile.Solution.ALMProject = ALMCore.AlmConfig.ALMProjectName;
        }

        public ALMConfig AlmConfigurations
        {
            get { return ALMCore.AlmConfig; }
        }

        #region SingleTon
        private static readonly ALMIntegration _instance = new ALMIntegration();
        public static ALMIntegration Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        public enum eALMType
        {
            QC = 1,
            RQM = 2,
            RALLY = 3
        }

        public enum eALMConnectType
        {
            Silence,
            Manual,
            Auto,
            SettingsPage
        }

        public string ALMPassword()
        {
            return AlmRepo.ALMPassword();
        }

        public void SetALMPassword(string newPassword)
        {
            AlmRepo.SetALMPassword(newPassword);
            ALMCore.AlmConfig.ALMPassword = newPassword;
        }

        public void SetALMProject(string newProject)
        {
            AlmRepo.SetALMProject(newProject);
        }

        public bool TestALMServerConn(eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            bool connResult = false;
            connResult = AlmRepo.ConnectALMServer(almConectStyle);
            if (connResult)
                DisconnectALMServer(almConectStyle);

            Mouse.OverrideCursor = null;
            return connResult;
        }

        public bool TestALMProjectConn(eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            bool connResult = false;
            try
            {
                connResult = AlmCore.ConnectALMProject();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error connecting to ALM project", e);
                if (almConectStyle == eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMLoginFailed, e.Message);
                else if (almConectStyle == eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings, e.Message);
            }

            if(connResult)
                DisconnectALMServer(almConectStyle);

            Mouse.OverrideCursor = null;
            return connResult;
        }

        private bool ConnectALMProject()
        {
            return AlmCore.ConnectALMProject();
        }


        public void DisconnectALMServer(eALMConnectType almConectStyle = eALMConnectType.Silence)
        {
            try
            {
                AlmCore.DisconnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error disconnecting from ALM", e);
                if (almConectStyle == eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMOperationFailed, "disconnect server", e.Message);
            }
        }

        public bool DisconnectALMProjectStayLoggedIn()
        {
            return AlmCore.DisconnectALMProjectStayLoggedIn();
        }

        public List<string> GetALMDomains(eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            List<string> domainList = new List<string>();
            if (AutoALMServerConnect(almConectStyle))
            {
                domainList = AlmCore.GetALMDomains();
                DisconnectALMServer(almConectStyle);
            }

            Mouse.OverrideCursor = null;
            return domainList;
        }

        public List<string> GetALMDomainProjects(string ALMDomain, eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            List<string> projectsList = new List<string>();
            if (AutoALMServerConnect(almConectStyle))
            {
                projectsList = AlmCore.GetALMDomainProjects(ALMDomain);
                DisconnectALMServer(almConectStyle);
            }
            Mouse.OverrideCursor = null;
            return projectsList;
        }


        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> BusinessFlows, ref string result, PublishToALMConfig publishToALMConfig, eALMConnectType almConnectionType, bool exectutedFromAutomateTab = false)
        {
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            bool isExportSucc = false;
            if (AutoALMProjectConnect(almConnectionType, false))
            {
                try
                {
                    foreach (BusinessFlow BizFlow in BusinessFlows) //Here going for each businessFlow
                    {
                        try
                        {
                            if (BizFlow.ExternalID != "0" && !String.IsNullOrEmpty(BizFlow.ExternalID))
                            {
                                Reporter.ToLog(eAppReporterLogLevel.INFO, "Executing RunSet Action Publish to ALM for Business flow: " + BizFlow.Name);
                                Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportExecutionDetails, null, BizFlow.Name, "ALM");

                                if (publishToALMConfig.ToAttachActivitiesGroupReport)
                                {
                                    Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateActivitiesGroupReportsOfBusinessFlow(BizFlow);
                                }
                                                   
                                isExportSucc = AlmCore.ExportExecutionDetailsToALM(BizFlow, ref result, exectutedFromAutomateTab, publishToALMConfig);
                                if (isExportSucc)
                                {
                                    BizFlow.PublishStatus = BusinessFlow.ePublishStatus.Published;
                                }
                                else
                                {
                                    if ((result == null) || (result == string.Empty))
                                        result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " - " + BizFlow.Name + " - Error when uploading to ALM." + Environment.NewLine;
                                    BizFlow.PublishStatus = BusinessFlow.ePublishStatus.PublishFailed;
                                }
                                Reporter.CloseGingerHelper();
                            }
                            else
                            {
                                BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                                result += GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " - " + BizFlow.Name + " - dosen't have ExternalID, cannot execute publish to ALM RunSet Action" + Environment.NewLine;
                                Reporter.ToLog(eAppReporterLogLevel.INFO, BizFlow.Name + " - dosen't have ExternalID, cannot execute publish to ALM RunSet Action");                               
                            }
                        }
                        catch (Exception ex)
                        {
                            result = ex.Message.ToString();
                            BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, BizFlow.Name + " - Export results to ALM failed due to exception", ex);
                        }
                    }

                    return isExportSucc;
                }
                finally
                {
                    DisconnectALMServer();
                }
            }
            else
            {
                if (exectutedFromAutomateTab)
                {
                    result += "Execution results were not published, Failed to connect to ALM";
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Execution results were not published, Failed to connect to ALM");
                }
                else
                {
                    foreach (BusinessFlow BizFlow in BusinessFlows)
                    {
                        BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                    }
                    result += "Didn't execute " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " action 'Publish to ALM', Failed to connect to ALM";
                    Reporter.ToLog(eAppReporterLogLevel.WARN, "Didn't execute " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " action Publish to ALM, Failed to connect to ALM");
                }
                return false;
            }
        }

        public void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Reporter.ToLog(eAppReporterLogLevel.INFO, ("Update selected Activities Groups of business flow: " + businessFlow.Name + " from ALM"));

            ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.UpdateActivitiesGroup(ref businessFlow, TCsIDs);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Reporter.ToLog(eAppReporterLogLevel.INFO, ("Update business flow: " + businessFlow.Name + " from ALM"));

            ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.UpdateBusinessFlow(ref businessFlow);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Reporter.ToLog(eAppReporterLogLevel.INFO, ("Exporting Activity Groups of business flow: " + businessFlow.Name + " to ALM"));
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, bool performSaveAfterExport = false)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();

            bool isExportSucc = false;
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                isExportSucc = AlmRepo.ExportActivitiesGroupToALM(activtiesGroup, null , performSaveAfterExport);
                DisconnectALMServer();
            }

            Mouse.OverrideCursor = null;
            return isExportSucc;
        }

        //Export Group of Business flows
        public bool ExportAllBusinessFlowsToALM(ObservableList<BusinessFlow> bfToExport, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            //Passing Solution Folder path to GingerCore
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();

            bool isExportSucc = true;
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                string testPlanUploadPath = AlmRepo.SelectALMTestPlanPath();
                if (testPlanUploadPath == null)
                    return false;
                string testLabUploadPath = AlmRepo.SelectALMTestLabPath();
                if (testLabUploadPath == null)
                    return false;
                foreach (BusinessFlow bf in bfToExport)
                {
                    if (!AlmRepo.ExportBusinessFlowToALM(bf, performSaveAfterExport, almConectStyle, testPlanUploadPath, testLabUploadPath))
                        isExportSucc = false;
                }
                DisconnectALMServer();
            }
            else isExportSucc = false;

            Mouse.OverrideCursor = null;
            return isExportSucc;
        }

        //Export Business Flow
        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            Reporter.ToLog(eAppReporterLogLevel.INFO, ("Exporting business flow: " + businessFlow.Name + " to ALM"));
            //Passing Solution Folder path to GingerCore
            ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();

            bool isExportSucc = false;
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                isExportSucc = AlmRepo.ExportBusinessFlowToALM(businessFlow, performSaveAfterExport);
                DisconnectALMServer();
            }

            Mouse.OverrideCursor = null;
            return isExportSucc;
        }

        public IEnumerable<Object> SelectALMTestSets()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            if (AutoALMProjectConnect())
            {
                IEnumerable<Object> selectedTestSets = AlmRepo.SelectALMTestSets();
                DisconnectALMServer();
                return selectedTestSets;
            }

            Mouse.OverrideCursor = null;
            return null;
        }

        public void ImportALMTests(string importDestinationFolderPath = null)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Importing Business flow from ALM");
            if (AutoALMProjectConnect(eALMConnectType.Auto, true, true))
            {
                ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
                AlmRepo.ImportALMTests(importDestinationFolderPath);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void ImportALMTestsById(string importDestinationFolderPath = "")
        {
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Importing Business flow from ALM By Id");
            if (AutoALMProjectConnect(eALMConnectType.Auto, true, true))
            {
                ALMCore.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
                AlmRepo.ImportALMTestsById(importDestinationFolderPath);
                DisconnectALMServer();
            }
        }

        public void RefreshAllGroupsFromALM(BusinessFlow businessFlow)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Refreshing All Activities Groups From ALM");
            ALMIntegration.Instance.UpdateBusinessFlow(ref businessFlow);
            Mouse.OverrideCursor = null;
        }

        public ObservableList<ExternalItemFieldBase> GetALMItemFieldsREST(bool online, ALM_Common.DataContracts.ResourceType resourceType, BackgroundWorker bw = null)
        {
            ObservableList<ExternalItemFieldBase> latestALMFieldsREST = new ObservableList<ExternalItemFieldBase>();
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                latestALMFieldsREST = AlmCore.GetALMItemFields(null ,online, resourceType);
            }
            return latestALMFieldsREST;
        }

        public Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening)
        {
            Dictionary<Guid, string> defectsOpeningResults = new Dictionary<Guid, string>();
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                defectsOpeningResults = AlmCore.CreateNewALMDefects(defectsForOpening, true);
            }
            return defectsOpeningResults;
        }

        public void RefreshALMItemFields(ObservableList<ExternalItemFieldBase> exitingFields, bool online, BackgroundWorker bw = null)
        {
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                //Get latestALMFields from ALMCore with Online flag
                ObservableList<ExternalItemFieldBase> latestALMFields = AlmCore.GetALMItemFields(bw, online);

                //Merging Latest with exiting
                if (exitingFields == null)
                    exitingFields = new ObservableList<ExternalItemFieldBase>();

                foreach (ExternalItemFieldBase latestField in latestALMFields)
                {
                    ExternalItemFieldBase existingField = exitingFields.Where(x => x.ID == latestField.ID).FirstOrDefault();
                    if (existingField != null)
                    {
                        existingField.Name = latestField.Name;
                        existingField.ItemType = latestField.ItemType;
                        existingField.Mandatory = latestField.Mandatory;
                        existingField.ExternalID = latestField.ExternalID;
                       
                        if (latestField.Mandatory == true)
                            existingField.ToUpdate = true;

                        existingField.PossibleValues = latestField.PossibleValues;
                        if (string.IsNullOrEmpty(existingField.SelectedValue) == false)
                        {                        
                            if (!latestField.PossibleValues.Contains(existingField.SelectedValue))
                                existingField.SelectedValue = latestField.SelectedValue;
                        }
                        else
                        {
                            existingField.SelectedValue = latestField.SelectedValue;
                        }
                    }
                    else
                        exitingFields.Add(latestField);
                }

                //Sorting the list
                ObservableList<ExternalItemFieldBase> sortedFields = new ObservableList<ExternalItemFieldBase>(from i in exitingFields orderby i.ItemType select i);
                exitingFields.ClearAll();
                exitingFields.Append(sortedFields);
            }
        }

        public bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            return AlmRepo.ShowImportReviewPage(importDestinationPath, selectedTestPlan);
        }

        public bool ImportSelectedTestSets(string importDestinationPath, IEnumerable<Object> selectedTests)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            bool importResult = AlmRepo.ImportSelectedTests(importDestinationPath, selectedTests);
            Mouse.OverrideCursor = null;
            return importResult;

        }

        public bool AutoALMProjectConnect(eALMConnectType almConnectStyle = eALMConnectType.Silence, bool showConnWin = true, bool asConnWin = false)
        {
            int retryConnect = 0;
            bool isConnected = false;
            while (!isConnected && retryConnect < 2)
            {
                try { isConnected = ConnectALMProject(); }
                catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}"); }
                retryConnect++;
            }
            if (!isConnected && almConnectStyle != eALMConnectType.Silence)
                if (showConnWin)
                {
                    ALMConnectionPage almConnPage = new ALMConnectionPage(almConnectStyle, asConnWin);
                    almConnPage.ShowAsWindow();
                    try { isConnected = ConnectALMProject(); }
                    catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}"); }
                }

            return isConnected;
        }

        private bool AutoALMServerConnect(eALMConnectType almConnectStyle)
        {
            return AlmRepo.ConnectALMServer(almConnectStyle);
        }

        public void OpenALMItemsFieldsPage()
        {
            AlmRepo.OpenALMItemsFieldsPage();
        }

        public void ALMDefectsProfilesPage()
        {
            AlmRepo.ALMDefectsProfilesPage();
        }

        public bool LoadALMConfigurations()
        {
            return AlmRepo.LoadALMConfigurations();
        }

        public eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            return AlmRepo.GetDownloadPossibleValuesMessage();
        }

        public List<string> GetTestLabExplorer(string path)
        {
            return AlmRepo.GetTestLabExplorer(path);
        }

        public IEnumerable<Object> GetTestSetExplorer(string path)
        {
            return AlmRepo.GetTestSetExplorer(path);
        }

        public QCTestSetSummary GetTSRunStatus(QCTestSetSummary tsItem)
        {
            return (QCTestSetSummary)AlmRepo.GetTSRunStatus(tsItem);
        }

        public List<string> GetTestPlanExplorer(string path)
        {
            return AlmRepo.GetTestPlanExplorer(path);
        }
    }
}
