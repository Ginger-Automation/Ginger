#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using QCRestClientStd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using TDAPIOLELib;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM.Repository
{
    public enum eQCItemType { TestCase, TestSet, Defect }

    class QCRepository : ALMRepository
    {
        Test matchingTC = null;
        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to QC server");
                return ALMIntegration.Instance.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                if (userMsgStyle == eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.QcConnectFailure, e.Message);
                }
                else if (userMsgStyle == eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);
                }
                Reporter.ToLog(eLogLevel.WARN, "Error connecting to QC server", e);
                return false;
            }
        }

        #region General
        public override IEnumerable<Object> SelectALMTestSets()
        {
            //show Test Lab browser for selecting the Test Set/s
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.Select);
            return (ObservableList<QCTestSetTreeItem>)win.ShowAsWindow(eWindowShowStyle.Dialog);
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
            return QCConnect.GetTestLabExplorer(path);
        }

        public override IEnumerable<Object> GetTestSetExplorer(string path)
        {
            return QCConnect.GetTestSetExplorer(path);
        }

        public override Object GetTSRunStatus(Object tsItem)
        {
            return QCConnect.GetTSRunStatus(tsItem);
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            return QCConnect.GetTestPlanExplorer(path);
        }

        #endregion General

        #region Import From QC
        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from QC");
            //set path to import to               
            if (importDestinationFolderPath == "")
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }
            //show Test Lab browser for selecting the Test Set/s to import
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.Import, importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> selectedTestSets)
        {
            if (selectedTestSets != null && selectedTestSets.Any())
            {
                ObservableList<QCTestSetTreeItem> testSetsItemsToImport = [];
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
                //Refresh Ginger repository and allow GingerQC to use it
                ALMIntegration.Instance.AlmCore.InitCoreObjs();

                foreach (QCTestSetTreeItem testSetItemtoImport in testSetsItemsToImport)
                {
                    try
                    {
                        //import test set data
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testSetItemtoImport.TestSetName);
                        ALMTestSet TS = new ALMTestSet
                        {
                            TestSetID = testSetItemtoImport.TestSetID,
                            TestSetName = testSetItemtoImport.TestSetName,
                            TestSetPath = testSetItemtoImport.Path
                        };
                        TS = ((QCCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((QCCore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);

                        if (WorkSpace.Instance.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                            {
                                if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                                {
                                    if (tsBusFlow.TargetApplications.FirstOrDefault(x => x.Name == activ.TargetApplication) == null)
                                    {
                                        ApplicationPlatform appAgent = WorkSpace.Instance.Solution.ApplicationPlatforms.FirstOrDefault(x => x.AppName == activ.TargetApplication);
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
            List<ALMTSTest> TCOfTestSetList = ((QCCore)ALMIntegration.Instance.AlmCore).GetTSQCTestsList(businessFlow.ExternalID.ToString(), TCsIDs.Select(x => x.Item2.ToString()).ToList());
            ((QCCore)ALMIntegration.Instance.AlmCore).UpdatedQCTestInBF(ref businessFlow, TCOfTestSetList);
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            List<ALMTSTest> TCOfTestSetList = ((QCCore)ALMIntegration.Instance.AlmCore).GetTSQCTestsList(businessFlow.ExternalID.ToString());
            ((QCCore)ALMIntegration.Instance.AlmCore).UpdateBusinessFlow(ref businessFlow, TCOfTestSetList);
        }
        #endregion Import From QC


        #region Export To QC
        public override void ExportBfActivitiesGroupsToALM(GingerCore.BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            bool askToSaveBF = false;
            foreach (ActivitiesGroup group in grdActivitiesGroups)
            {
                if (ExportActivitiesGroupToALM(group))
                {
                    askToSaveBF = true;
                }
            }

            if (askToSaveBF)
            {
                if (Reporter.ToUser(eUserMsgKey.AskIfToSaveBFAfterExport, businessFlow.Name) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name,
                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.HideStatusMessage();
                }
            }
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
        {
            if (activtiesGroup == null)
            {
                return false;
            }
            //if it is called from shared repository need to select path
            if (uploadPath == null)
            {
                QCTestPlanExplorerPage win = new QCTestPlanExplorerPage();
                win.xCreateBusinessFlowFolder.Visibility = Visibility.Collapsed;//no need to create separate folder
                uploadPath = win.ShowAsWindow(eWindowShowStyle.Dialog);
                if (uploadPath == null)
                {
                    return false;
                }
            }
            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;
            //TODO: retireve test case fields -->DONE
            ObservableList<ExternalItemFieldBase> testCaseFields = WorkSpace.Instance.Solution.ExternalItemsFields;
            ALMIntegration.Instance.RefreshALMItemFields(testCaseFields, true, null);
            var filterTestCaseFields = testCaseFields.Where(tc => tc.ItemType == eQCItemType.TestCase.ToString()).ToList();
            bool exportRes = ((QCCore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, uploadPath, new ObservableList<ExternalItemFieldBase>(filterTestCaseFields), ref res);
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
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, res);
            }
            return false;
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null)
            {
                return false;
            }
            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            TestSet matchingTS = null;

            Amdocs.Ginger.Common.eUserMsgSelection userSelec = Amdocs.Ginger.Common.eUserMsgSelection.None;
            //check if the businessFlow already mapped to QC Test Set
            if (String.IsNullOrEmpty(businessFlow.ExternalID) == false)
            {
                matchingTS = ((QCCore)ALMIntegration.Instance.AlmCore).GetQCTestSet(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continue
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, ((QCTestSetFolder)matchingTS.TestSetFolder).Path + "\\" + matchingTS.Name);
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

            //check if all of the business flow activities groups already exported to QC and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                //check if the ActivitiesGroup already mapped to QC Test Case
                matchingTC = null;
                if (String.IsNullOrEmpty(ag.ExternalID) == false)
                {
                    matchingTC = ((QCCore)ALMIntegration.Instance.AlmCore).GetQCTest(ag.ExternalID);
                    if (matchingTC != null)
                    {
                        //ask user if want to continue
                        Amdocs.Ginger.Common.eUserMsgSelection userSelect = Reporter.ToUser(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, ag.Name, matchingTC.FullPath + "\\" + matchingTC.Name);
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
                            testPlanUploadPath = matchingTC.FullPath;
                        }
                    }
                }

                //if user selected No and want to create new testplans to selected folder path
                if (matchingTC == null && String.IsNullOrEmpty(testPlanUploadPath))
                {
                    //get the QC Test Plan path to upload the activities group to
                    testPlanUploadPath = SelectALMTestPlanPath();
                    if (String.IsNullOrEmpty(testPlanUploadPath))
                    {
                        //no path to upload to
                        return false;
                    }

                    //create upload path if checked to create separete folder
                    if (QCTestPlanFolderTreeItem.IsCreateBusinessFlowFolder)
                    {
                        //create folder with BF name
                        try
                        {
                            if (QCConnect.CreateFolder(testPlanUploadPath, businessFlow.Name))
                            {
                                testPlanUploadPath += "\\" + businessFlow.Name;
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to get create folder for Test Plan", ex);
                        }
                    }
                }
                ExportActivitiesGroupToALM(ag, testPlanUploadPath);
            }

            if (matchingTS == null && string.IsNullOrEmpty(testLabUploadPath))
            {
                if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    Reporter.ToUser(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder);
                }
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
            //TODO : need to update to retrieve only Test Set Item Fields -->DONE
            ObservableList<ExternalItemFieldBase> testSetFields = WorkSpace.Instance.Solution.ExternalItemsFields;
            ALMIntegration.Instance.RefreshALMItemFields(testSetFields, true, null);
            var filterTestSetFields = testSetFields.Where(tc => tc.ItemType == eQCItemType.TestSet.ToString()).ToList();
            bool exportRes = ((QCCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToALM(businessFlow, matchingTS, testLabUploadPath, new ObservableList<ExternalItemFieldBase>(filterTestSetFields), ref res);
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
            else if (almConectStyle != eALMConnectType.Auto)
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }
            return false;
        }
        #endregion Export To QC

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            throw new NotImplementedException();
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

    }
}
