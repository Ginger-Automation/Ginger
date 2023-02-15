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
using amdocs.ginger.GingerCoreNET;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using GingerCoreNET.ALMLib;
using GingerWPF.WizardLib;
using System.Windows.Controls;
using System.Threading.Tasks;
using Ginger.Run.RunSetActions;

namespace Ginger.ALM
{
    public class ALMIntegration
    {
        public ALMCore AlmCore { get; set; }
        private ALMRepository AlmRepo;

        private ALMIntegration()
        {
        }

        public void UpdateALMType(eALMType AlmType, bool isOperationAlmType = false)
        {
            GingerCoreNET.ALMLib.ALMConfig CurrentAlmConfigurations = ALMCore.GetCurrentAlmConfig(AlmType);
            ALMCore.DefaultAlmConfig = CurrentAlmConfigurations;
            switch (AlmType)
            {
                case eALMType.QC:
                    if (!CurrentAlmConfigurations.UseRest)
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
                    //AlmCore = new RallyCore();
                    AlmRepo = new RallyRepository();
                    break;
                case eALMType.Jira:
                    AlmCore = new JiraCore();
                    AlmRepo = new JIRA_Repository(AlmCore);
                    break;
                case eALMType.Qtest:
                    AlmCore = new QtestCore();
                    AlmRepo = new QtestRepository();
                    break;
                case eALMType.Octane:
                    if(!(AlmCore is OctaneCore && AlmRepo is OctaneRepository))
                    {
                        AlmCore = new OctaneCore();
                        AlmRepo = new OctaneRepository(AlmCore);
                    }
                    break;
                case eALMType.ZephyrEnterprise:
                    AlmCore = new ZephyrEntCore();
                    AlmRepo = new ZephyrEnt_Repository(AlmCore);
                    break;
            }
            AlmCore.GetCurrentAlmConfig(isOperationAlmType);
            ALMCore.SetALMCoreConfigurations(AlmType, AlmCore);
        }


        public void SetALMCoreConfigurations(eALMType almType)
        {
            ALMCore.SetALMCoreConfigurations(almType, AlmCore);           
        }
        public ALMConfig GetDefaultAlmConfig()
        {
            if (AlmCore == null)
            {
                if (WorkSpace.Instance.Solution.ALMConfigs != null && WorkSpace.Instance.Solution.ALMConfigs.Count > 0)
                {
                    return WorkSpace.Instance.Solution.ALMConfigs.Where(alm => alm.DefaultAlm).FirstOrDefault();
                }
                else
                {
                    return new ALMConfig();
                }
            }
            return AlmCore.GetCurrentAlmConfig();
        }

        public bool SetDefaultAlmConfig(eALMType AlmType)
        {
            //set default on the solution
            foreach (ALMConfig alm in WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm).ToList())
            {
                alm.DefaultAlm = false;
            }
            ALMConfig almConfig = ALMCore.GetCurrentAlmConfig(AlmType);
            almConfig.DefaultAlm = true;

            //set default on almcore
            foreach (ALMConfig alm in ALMCore.AlmConfigs.Where(x => x.DefaultAlm).ToList())
            {
                alm.DefaultAlm = false;
            }
            ALMConfig DefaultAlm = ALMCore.AlmConfigs.FirstOrDefault(x => x.AlmType == AlmType);
            if (DefaultAlm == null)
            {
                DefaultAlm = new ALMConfig();
                DefaultAlm.AlmType = AlmType;
                ALMCore.AlmConfigs.Add(DefaultAlm);
            }
            DefaultAlm.DefaultAlm = true;

            return true;
        }
        public ALMUserConfig GetCurrentAlmUserConfig(eALMType almType)
        {
            ALMUserConfig AlmUserConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault(x => x.AlmType == almType);
            if (AlmUserConfig == null)
            {
                AlmUserConfig = new ALMUserConfig();
                AlmUserConfig.AlmType = almType;
                WorkSpace.Instance.UserProfile.ALMUserConfigs.Add(AlmUserConfig);
            }

            return AlmUserConfig;

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

        public bool TestALMServerConn(eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            bool connResult = false;
            connResult = AlmRepo.ConnectALMServer(almConectStyle);
            if (connResult)
                DisconnectALMServer(almConectStyle);

            Mouse.OverrideCursor = null;
            return connResult;
        }


        public Dictionary<string, string> GetSSOTokens()
        {
            return AlmCore.GetSSOTokens();
        }


        public Dictionary<string, string> GetConnectionInfo()
        {
            return AlmCore.GetConnectionInfo();
        }

        public bool IsServerConnected()
        {
            return AlmCore.IsServerConnected();
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
                Reporter.ToLog(eLogLevel.ERROR, "Error connecting to ALM project", e);
                if (almConectStyle == eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKey.ALMLoginFailed, e.Message);
                else if (almConectStyle == eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);
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
                Reporter.ToLog(eLogLevel.ERROR, "Error disconnecting from ALM", e);
                if (almConectStyle == eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKey.ALMOperationFailed, "disconnect server", e.Message);
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

        internal async Task<bool> MapBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false)
        {
            Reporter.ToLog(eLogLevel.INFO, "Mapping " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " to ALM");
            //Passing Solution Folder path to GingerCore
            ALMCore.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();

            bool isMapSucc = false;

            try
            {
                Reporter.ToStatus(eStatusMsgKey.ALMTestSetMap);
                bool isConnected = false;
                
                isConnected = AutoALMProjectConnect(eALMConnectType.Auto);
                
                if (isConnected)
                    {
                        if (GetALMType().Equals(eALMType.ZephyrEnterprise))
                        {
                            WizardWindow.ShowWizard(new MapToALMWizard.AddMapToALMWizard(businessFlow), 1200);
                            isMapSucc = true;
                            DisconnectALMServer();
                        }
                        else
                        {
                            Reporter.ToUser(eUserMsgKey.StaticWarnMessage, $"'Map To ALM' - not Supporting {GetALMType()}.");
                        }
                    }
                
            }
            finally
            {
                Reporter.HideStatusMessage();
            }
            return isMapSucc;
        }

        public List<string> GetJiraTestingALMs()
        {
            return ((JiraCore)AlmCore).GetJiraTestingALMs();
        }

        public object GetZephyrCycles(bool getFolders = false)
        {
            return ((JiraCore)AlmCore).GetZephyrCyclesWithFolders(getFolders);
        }
        
        public Dictionary<string, string> GetALMDomainProjects(string ALMDomain, eALMConnectType almConectStyle)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Dictionary<string, string> projectsList = new Dictionary<string, string>();
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
            ObservableList<ExternalItemFieldBase> solutionAlmFields = null;
            try
            {
                solutionAlmFields = SetALMTypeAndFieldsFromSolutionToOperation(publishToALMConfig, solutionAlmFields);
                ALMCore.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
                if (AutoALMProjectConnect(almConnectionType, false))
                {
                    return AlmCore.ExportBusinessFlowsResultToALM(BusinessFlows, ref result, publishToALMConfig, almConnectionType, exectutedFromAutomateTab);
                }
                else
                {
                    if (exectutedFromAutomateTab)
                    {
                        result += "Execution results were not published, Failed to connect to ALM";
                        Reporter.ToLog(eLogLevel.WARN, "Execution results were not published, Failed to connect to ALM");
                    }
                    else
                    {
                        foreach (BusinessFlow BizFlow in BusinessFlows)
                        {
                            BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                        }
                        result += "Didn't execute " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " action 'Publish to ALM', Failed to connect to ALM";
                        Reporter.ToLog(eLogLevel.WARN, "Didn't execute " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " action Publish to ALM, Failed to connect to ALM");
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                ResetALMTypeAndFieldsFromOperationToSolution(publishToALMConfig, solutionAlmFields);
            }
        }
        public void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Reporter.ToLog(eLogLevel.INFO, ("Update selected " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " of " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ":" + businessFlow.Name + " from ALM"));

            ALMIntegration._instance.AlmCore.InitCoreObjs();

            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.UpdateActivitiesGroup(ref businessFlow, TCsIDs);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Reporter.ToLog(eLogLevel.INFO, ("Update " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " from ALM"));

            ALMIntegration._instance.AlmCore.InitCoreObjs();

            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.UpdateBusinessFlow(ref businessFlow);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Reporter.ToLog(eLogLevel.INFO, ("Exporting "+ GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " of " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " to ALM"));
            ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                AlmRepo.ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, bool performSaveAfterExport = false)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

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
            Mouse.OverrideCursor = Cursors.Wait;
            //Passing Solution Folder path to GingerCore
            ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

            bool isExportSucc = true;
            if (AutoALMProjectConnect(eALMConnectType.Auto))
            {
                string testPlanUploadPath = null;
                string testLabUploadPath = null;
                if (ALMCore.GetDefaultAlmConfig().AlmType != eALMType.Jira)
                {
                    testPlanUploadPath = AlmRepo.SelectALMTestPlanPath();
                    if (testPlanUploadPath == null)
                        return false;
                    testLabUploadPath = AlmRepo.SelectALMTestLabPath();
                    if (testLabUploadPath == null)
                        return false;
                }
                
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
            //Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

            Reporter.ToLog(eLogLevel.INFO, ("Exporting " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " to ALM"));
            //Passing Solution Folder path to GingerCore
            ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();

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
            Reporter.ToLog(eLogLevel.DEBUG, "Importing "+ GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from ALM");
            if (AutoALMProjectConnect(eALMConnectType.Auto, true, true))
            {
                ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
                AlmRepo.ImportALMTests(importDestinationFolderPath);
                DisconnectALMServer();
            }
            Mouse.OverrideCursor = null;
        }

        public void ImportALMTestsById(string importDestinationFolderPath = "")
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Importing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " from ALM By Id");
            if (AutoALMProjectConnect(eALMConnectType.Auto, true, true))
            {
                ALMCore.SolutionFolder =  WorkSpace.Instance.Solution.Folder.ToUpper();
                AlmRepo.ImportALMTestsById(importDestinationFolderPath);
                DisconnectALMServer();
            }
        }

        public void RefreshAllGroupsFromALM(BusinessFlow businessFlow)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            Reporter.ToLog(eLogLevel.DEBUG, "Refreshing All " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " From ALM");
            ALMIntegration.Instance.UpdateBusinessFlow(ref businessFlow);
            Mouse.OverrideCursor = null;
        }

        public ObservableList<ExternalItemFieldBase> GetALMItemFieldsREST(bool online, AlmDataContractsStd.Enums.ResourceType resourceType, BackgroundWorker bw = null)
        {
            ObservableList<ExternalItemFieldBase> latestALMFieldsREST = new ObservableList<ExternalItemFieldBase>();
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                latestALMFieldsREST = AlmCore.GetALMItemFields(null ,online, (AlmDataContractsStd.Enums.ResourceType)resourceType);
            }
            return latestALMFieldsREST;
        }

        public Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening,List<ExternalItemFieldBase> defectsFields)
        {
            Dictionary<Guid, string> defectsOpeningResults = new Dictionary<Guid, string>();
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                defectsOpeningResults = AlmCore.CreateNewALMDefects(defectsForOpening, defectsFields, true);
            }
            return defectsOpeningResults;
        }

        public void RefreshALMItemFields(ObservableList<ExternalItemFieldBase> exitingFields, bool online, BackgroundWorker bw = null)
        {
            if (ALMIntegration.Instance.AutoALMProjectConnect())
            {
                //Get latestALMFields from ALMCore with Online flag
                ObservableList<ExternalItemFieldBase> latestALMFields = AlmCore.GetALMItemFields(bw, online);
                ObservableList<ExternalItemFieldBase> mergedFields = AlmCore.RefreshALMItemFields(exitingFields, latestALMFields);
            }
        }
        internal ObservableList<ExternalItemFieldBase> GetUpdatedFields(ObservableList<ExternalItemFieldBase> mItemsFields, bool online, BackgroundWorker bw = null)
        {
            ObservableList<ExternalItemFieldBase> updatedFields = new ObservableList<ExternalItemFieldBase>();
            if (ALMCore.AlmItemFields != null)
            {
                foreach (ExternalItemFieldBase defaultField in ALMCore.AlmItemFields)
                {
                    ExternalItemFieldBase currentField = mItemsFields.Where(x => x.ID == defaultField.ID && x.ItemType == defaultField.ItemType).FirstOrDefault();
                    if (currentField != null)
                    {
                        currentField.ToUpdate = false;
                        if (string.IsNullOrEmpty(currentField.SelectedValue) == false)
                        {
                            if ((defaultField.PossibleValues.Count == 0 && currentField.SelectedValue != defaultField.SelectedValue) || (defaultField.PossibleValues.Count > 0 && defaultField.PossibleValues.Contains(currentField.SelectedValue) && currentField.SelectedValue != defaultField.PossibleValues[0]))
                            {
                                currentField.ToUpdate = true;
                                updatedFields.Add(currentField);
                            }
                            else
                            {
                                currentField.SelectedValue = defaultField.SelectedValue;
                                currentField.ToUpdate = false;
                            }
                        }
                        else
                        {
                            currentField.SelectedValue = defaultField.SelectedValue;
                            currentField.ToUpdate = false;
                        }
                    }
                }
            }
            return updatedFields;
        }
        public bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            return AlmRepo.ShowImportReviewPage(importDestinationPath, selectedTestPlan);
        }

        public bool ImportSelectedTestSets(string importDestinationPath, IEnumerable<Object> selectedTests)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            bool importResult = AlmRepo.ImportSelectedTests(importDestinationPath, selectedTests);
            Mouse.OverrideCursor = null;
            return importResult;
        }

        public bool ImportZephyrObject(string importDestinationPath, IEnumerable<Object> selectedObject)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            bool importResult = ((JIRA_Repository)AlmRepo).ImportSelectedZephyrCyclesAndFolders(importDestinationPath, selectedObject);
            Mouse.OverrideCursor = null;
            return importResult;
        }

        public bool AutoALMProjectConnect(eALMConnectType almConnectStyle = eALMConnectType.Silence, bool showConnWin = true, bool asConnWin = false)
        {
            if (AlmCore == null)//added because when running from CLI the AlmCore is Null on connection
            {
                UpdateALMType(ALMCore.GetDefaultAlmConfig().AlmType);
            }

            int retryConnect = 0;
            bool isConnected = false;
            while (!isConnected && retryConnect < 2)
            {
                try { isConnected = ConnectALMProject(); }
                catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }
                retryConnect++;
            }
            if (!isConnected && almConnectStyle != eALMConnectType.Silence)
                if (showConnWin)
                {
                    App.MainWindow.Dispatcher.Invoke(() =>
                    {
                        ALMConnectionPage almConnPage = new ALMConnectionPage(almConnectStyle, asConnWin);
                        almConnPage.ShowAsWindow();
                    });
                    try { isConnected = ConnectALMProject(); }
                    catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }
                }

            return isConnected;
        }

        private bool AutoALMServerConnect(eALMConnectType almConnectStyle)
        {
            return AlmRepo.ConnectALMServer(almConnectStyle);
        }

        public void OpenALMItemsFieldsPage(eALMConfigType configType, eALMType type, ObservableList<ExternalItemFieldBase> almItemsFields)
        {
            if (AlmRepo == null)
            {
                UpdateALMType(type);
            }
            AlmRepo.OpenALMItemsFieldsPage(configType, type, almItemsFields);
        }
        
        public bool LoadALMConfigurations()
        {
            return AlmRepo.LoadALMConfigurations();
        }

        public eUserMsgKey GetDownloadPossibleValuesMessage()
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

        public ALMTestSetSummary GetTSRunStatus(ALMTestSetSummary tsItem)
        {
            return (ALMTestSetSummary)AlmRepo.GetTSRunStatus(tsItem);
        }

        public List<string> GetTestPlanExplorer(string path)
        {
            return AlmRepo.GetTestPlanExplorer(path);
        }
        public eALMType GetALMType()
        {
            return ALMCore.GetDefaultAlmConfig().AlmType;
        }
        public Page GetALMTestSetsTreePage(string importDestinationPath = "")
        {
            return AlmRepo.GetALMTestSetsTreePage(importDestinationPath);
        }
        public Object GetSelectedImportTestSetData(Page page)
        {
            return AlmRepo.GetSelectedImportTestSetData(page);
        }
        public void GetALMTestSetData(ALMTestSet almTestSet)
        {
            AlmRepo.GetALMTestSetData(almTestSet);
        }
        public ALMTestSet GetALMTestCases(ALMTestSet almTestSet)
        {
            return AlmRepo.GetALMTestCasesToTestSetObject(almTestSet);
        }
        public bool ExportVirtualBusinessFlowToALM(BusinessFlow businessFlow, PublishToALMConfig publishToALMConfig, bool performSaveAfterExport = false, eALMConnectType almConnectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            bool isExportSucc = false;
            ObservableList<ExternalItemFieldBase> solutionAlmFields = null;
            try
            {
                solutionAlmFields = SetALMTypeAndFieldsFromSolutionToOperation(publishToALMConfig, solutionAlmFields);
                Reporter.ToLog(eLogLevel.INFO, ("Exporting virtual " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " to ALM"));
                if (AutoALMProjectConnect(eALMConnectType.Silence, false))
                {
                    isExportSucc = AlmRepo.ExportBusinessFlowToALM(businessFlow, performSaveAfterExport, almConnectStyle, testPlanUploadPath, testLabUploadPath);
                    DisconnectALMServer();
                }
                return isExportSucc;
            }
            catch(Exception ex)
            {
                return isExportSucc;
            }
            finally
            {
                ResetALMTypeAndFieldsFromOperationToSolution(publishToALMConfig, solutionAlmFields);
            }
        }

        private static void ResetALMTypeAndFieldsFromOperationToSolution(PublishToALMConfig publishToALMConfig, ObservableList<ExternalItemFieldBase> solutionAlmFields)
        {
            if (!publishToALMConfig.PublishALMType.ToString().Equals(RunSetActionPublishToQC.AlmTypeDefault)
                                && publishToALMConfig.ALMTestSetLevel == PublishToALMConfig.eALMTestSetLevel.RunSet)
            {
                ALMIntegration.Instance.UpdateALMType(ALMCore.GetDefaultAlmConfig().AlmType);
                foreach (ExternalItemFieldBase field in publishToALMConfig.AlmFields)
                {
                    WorkSpace.Instance.Solution.ExternalItemsFields.Remove(field);
                }
                foreach (ExternalItemFieldBase field in solutionAlmFields)
                {
                    WorkSpace.Instance.Solution.ExternalItemsFields.Add(field);
                }
            }
            Reporter.ToLog(eLogLevel.INFO, "ALM Type and Fields change from operation to solution configuration");
        }

        private static ObservableList<ExternalItemFieldBase> SetALMTypeAndFieldsFromSolutionToOperation(PublishToALMConfig publishToALMConfig, ObservableList<ExternalItemFieldBase> solutionAlmFields)
        {
            // Set ALMType and Fields to operation publishToALMConfig selection, for Run Set only (virtual BF)
            if (!publishToALMConfig.PublishALMType.ToString().Equals(RunSetActionPublishToQC.AlmTypeDefault)
                && publishToALMConfig.ALMTestSetLevel == PublishToALMConfig.eALMTestSetLevel.RunSet)
            {
                ALMIntegration.Instance.UpdateALMType(publishToALMConfig.PublishALMType, true);
                // Set ExternalItemsFields as selected publishToALMConfig ALMFields and at 'finally' return to the solution fields.
                solutionAlmFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
                foreach (ExternalItemFieldBase field in solutionAlmFields)
                {
                    WorkSpace.Instance.Solution.ExternalItemsFields.Remove(field);
                }
                foreach (ExternalItemFieldBase field in publishToALMConfig.AlmFields)
                {
                    WorkSpace.Instance.Solution.ExternalItemsFields.Add(field);
                }
            }
            Reporter.ToLog(eLogLevel.INFO, "ALM Type and Fields change from solution to operation configuration");
            return solutionAlmFields;
        }
    }
}
