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
using QCRestClient;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Activities;
using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using System.Windows;
using System.IO;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCore.Platforms;
using Ginger.Repository;
using Amdocs.Ginger.Repository;
using GingerCore.ALM.QC;
using GingerCore.ALM.QCRestAPI;
using ALM_Common.DataContracts;
using GingerCore.ALM;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.ALM.Repository
{
    class QCRestAPIRepository : ALMRepository
    {
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to QC server");
                if(ALMIntegration.Instance.AlmCore.ConnectALMServer())
                    return true;
                else
                {
                    if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                        Reporter.ToUser(eUserMsgKey.QcConnectFailureRestAPI);
                    else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                        Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings);

                    Reporter.ToLog(eLogLevel.ERROR, "Error connecting to QC server");
                    return false;
                }
            }
            catch (Exception e)
            {
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKey.QcConnectFailureRestAPI, e.Message);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);

                Reporter.ToLog(eLogLevel.ERROR, "Error connecting to QC server", e);
                return false;
            }
        }

        #region General
        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            //show Test Plan browser for selecting the Path
            QCTestPlanExplorerPage win = new QCTestPlanExplorerPage();
            return win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override string SelectALMTestLabPath()
        {
            //show Test Lab browser for selecting the Test Set/s
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            return QCRestAPIConnect.GetTestLabExplorer(path);
        }

        public override IEnumerable<Object> GetTestSetExplorer(string path)
        {
            return QCRestAPIConnect.GetTestSetExplorer(path);
        }

        public override Object GetTSRunStatus(Object tsItem)
        {
            return QCRestAPIConnect.GetTSRunStatus(tsItem);
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            return QCRestAPIConnect.GetTestPlanExplorer(path);
        }

        #endregion General

        #region Import From QC
        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from QC");
            //set path to import to               
            if (importDestinationFolderPath == "")
                importDestinationFolderPath =  WorkSpace.Instance.Solution.BusinessFlowsMainFolder;

            //show Test Lab browser for selecting the Test Set/s to import
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.Import, importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> selectedTestSets)
        {
            if (selectedTestSets != null && selectedTestSets.Count() > 0)
            {
                ObservableList<QCTestSetTreeItem> testSetsItemsToImport = new ObservableList<QCTestSetTreeItem>();
                bool bfsWereDeleted = false;
                foreach (QCTestSetTreeItem testSetItem in selectedTestSets)
                {
                    //check if some of the Test Set was already imported                
                    if (testSetItem.AlreadyImported == true)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, testSetItem.TestSetName);
                        if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            //Delete the mapped BF
                            File.Delete(testSetItem.MappedBusinessFlow.FileName);
                            bfsWereDeleted = true;
                            testSetsItemsToImport.Add(testSetItem);
                        }
                    }
                    else
                    {
                        testSetsItemsToImport.Add(testSetItem);
                    }
                }

                if (testSetsItemsToImport.Count == 0) return false; //noting to import

                //Refresh Ginger repository and allow GingerQC to use it                
                ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
                ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

                foreach (QCTestSetTreeItem testSetItemtoImport in testSetsItemsToImport)
                {
                    try
                    {
                        //import test set data
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testSetItemtoImport.TestSetName);
                        GingerCore.ALM.QC.QCTestSet TS = new GingerCore.ALM.QC.QCTestSet();
                        TS.TestSetID = testSetItemtoImport.TestSetID;
                        TS.TestSetName = testSetItemtoImport.TestSetName;
                        TS.TestSetPath = testSetItemtoImport.Path;
                        TS = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);

                        if ( WorkSpace.Instance.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                                if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                                    if (tsBusFlow.TargetApplications.Where(x => x.Name == activ.TargetApplication).FirstOrDefault() == null)
                                    {
                                        ApplicationPlatform appAgent =  WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                        if (appAgent != null)
                                            tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                                    }
                            //handle non mapped Activities
                            if (tsBusFlow.TargetApplications.Count == 0)
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName =  WorkSpace.Instance.Solution.MainApplication });
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
                        Reporter.HideStatusMessage();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, testSetItemtoImport.TestSetName, ex.Message);
                        Reporter.ToLog(eLogLevel.ERROR, "Error importing from QC", ex);
                    }
                }

                Reporter.ToUser(eUserMsgKey.TestSetsImportedSuccessfully);

                Reporter.ToLog(eLogLevel.DEBUG, "Imported from QC successfully");
                return true;
            }
            Reporter.ToLog(eLogLevel.ERROR, "Error importing from QC");
            return false;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
        }
        #endregion Import From QC

        #region Export To QC
        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            if (activtiesGroup == null) return false;
            QCTestCase matchingTC = null;

            //check if the ActivitiesGroup already mapped to QC Test Case
            if (String.IsNullOrEmpty(activtiesGroup.ExternalID) == false)
            {
                matchingTC = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).GetQCTest(activtiesGroup.ExternalID);
                if (matchingTC != null)
                {
                    //ask user if want to continute
                    Amdocs.Ginger.Common.eUserMsgSelection userSelec = Reporter.ToUser(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, activtiesGroup.Name, matchingTC.Name);
                    if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                        return false;
                    else if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                        matchingTC = null;
                }
            }

            if (matchingTC == null && String.IsNullOrEmpty(uploadPath))
            {
                //get the QC Test Plan path to upload the activities group to
                uploadPath = SelectALMTestPlanPath();
                if (String.IsNullOrEmpty(uploadPath))
                {
                    //no path to upload to
                    return false;
                }
            }

            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>( WorkSpace.Instance.Solution.ExternalItemsFields);
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            ObservableList<ExternalItemFieldBase> testCaseFields = CleanUnrelvantFields(allFields, ResourceType.TEST_CASE);
            ObservableList<ExternalItemFieldBase> designStepsFields = CleanUnrelvantFields(allFields, ResourceType.DESIGN_STEP);
            ObservableList<ExternalItemFieldBase> designStepsParamsFields = CleanUnrelvantFields(allFields, ResourceType.DESIGN_STEP_PARAMETERS);

            bool exportRes = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, uploadPath, testCaseFields, designStepsFields, designStepsParamsFields, ref res);

            Reporter.HideStatusMessage();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);                    
                    Reporter.HideStatusMessage();
                }
                return true;
            }
            else
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, res);

            return false;
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            throw new NotImplementedException();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null)
                return false;

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            QCRestClient.QCTestSet matchingTS = null;

            Amdocs.Ginger.Common.eUserMsgSelection userSelec = Amdocs.Ginger.Common.eUserMsgSelection.None;
            //check if the businessFlow already mapped to QC Test Set
            if (String.IsNullOrEmpty(businessFlow.ExternalID) == false)
            {
                matchingTS = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).GetQCTestSet(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continute
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.Name);
                    if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                        return false;
                    else if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                        matchingTS = null;
                }
            }

            //check if all of the business flow activities groups already exported to QC and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                if (string.IsNullOrEmpty(ag.ExternalID) == true || ((QCRestAPICore)ALMIntegration.Instance.AlmCore).GetQCTest(ag.ExternalID) == null)
                {
                    if (testPlanUploadPath == null)
                        testPlanUploadPath = SelectALMTestPlanPath();
                    if (string.IsNullOrEmpty(testPlanUploadPath) == false)
                        ExportActivitiesGroupToALM(ag, testPlanUploadPath);
                    else
                        return false;
                }
                else
                    ExportActivitiesGroupToALM(ag, testPlanUploadPath);
            }

            if (matchingTS == null && string.IsNullOrEmpty(testLabUploadPath))
            {
                if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                    Reporter.ToUser(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder);

                //get the QC Test Plan path to upload the activities group to
                testLabUploadPath = SelectALMTestLabPath();
                if (String.IsNullOrEmpty(testLabUploadPath))
                {
                    //no path to upload to
                    return false;
                }
            }

            //upload the business flow
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>( WorkSpace.Instance.Solution.ExternalItemsFields);
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            ObservableList<ExternalItemFieldBase> testSetFieldsFields = CleanUnrelvantFields(allFields, ResourceType.TEST_SET);
            ObservableList<ExternalItemFieldBase> testInstanceFields = CleanUnrelvantFields(allFields, ResourceType.TEST_CYCLE);

            bool exportRes = ((QCRestAPICore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToALM(businessFlow, matchingTS, testLabUploadPath, testSetFieldsFields, testInstanceFields, ref res);
            Reporter.HideStatusMessage();
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
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);

            return false;
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            throw new NotImplementedException();
        }

        private ObservableList<ExternalItemFieldBase> CleanUnrelvantFields(ObservableList<ExternalItemFieldBase> fields, ResourceType resourceType)
        {
            ObservableList<ExternalItemFieldBase> fieldsToReturn = new ObservableList<ExternalItemFieldBase>();

            string currentResource = QCRestAPIConnect.ConvertResourceType(resourceType);
            //Going through the fields to leave only Test Set fields
            for (int indx = 0; indx < fields.Count; indx++)
            {
                if (fields[indx].ItemType == currentResource)
                {
                    fieldsToReturn.Add(fields[indx]);
                }
            }

            return fieldsToReturn;
        }

        #endregion Export To QC
    }
}
