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

using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.ALM.QC;
using System.Windows;
using System.IO;
using GingerCore.Platforms;
using Ginger.Repository;
using TDAPIOLELib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ALM.Repository
{
    class QCRepository : ALMRepository
    {
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            try
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Connecting to QC server");
                return ALMIntegration.Instance.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.QcConnectFailure, e.Message);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings, e.Message);

                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error connecting to QC server", e);
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
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Start importing from QC");
            //set path to import to               
            if (importDestinationFolderPath == "")
                importDestinationFolderPath = App.UserProfile.Solution.BusinessFlowsMainFolder;

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
                        MessageBoxResult userSelection = Reporter.ToUser(eUserMsgKeys.TestSetExists, testSetItem.TestSetName);
                        if (userSelection == MessageBoxResult.Yes)
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
                        Reporter.ToGingerHelper(eGingerHelperMsgKey.ALMTestSetImport, null, testSetItemtoImport.TestSetName);
                        QCTestSet TS = new QCTestSet();
                        TS.TestSetID = testSetItemtoImport.TestSetID;
                        TS.TestSetName = testSetItemtoImport.TestSetName;
                        TS.TestSetPath = testSetItemtoImport.Path;
                        TS = ((QCCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((QCCore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);

                        //Set BF/Activities target application
                        //if (App.UserProfile.Solution.MainApplication != null)
                        //{
                        //    if (tsBusFlow.TargetApplications.Count == 0)
                        //    {
                        //        tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
                        //        if (tsBusFlow.TargetApplications.Count > 0)
                        //            foreach (Activity activ in tsBusFlow.Activities)
                        //                activ.TargetApplication = tsBusFlow.TargetApplications[0].AppName;
                        //    }
                        //}
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

                        //save bf
                        WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tsBusFlow);                        
                        Reporter.CloseGingerHelper();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKeys.ErrorInTestsetImport, testSetItemtoImport.TestSetName, ex.Message);
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error importing from QC", ex);
                    }
                }
                
                Reporter.ToUser(eUserMsgKeys.TestSetsImportedSuccessfully);

                Reporter.ToLog(eAppReporterLogLevel.INFO, "Imported from QC successfully");
                return true;
            }
            Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error importing from QC");
            return false;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            List<QCTSTest> TCOfTestSetList = ((QCCore)ALMIntegration.Instance.AlmCore).GetTSQCTestsList(businessFlow.ExternalID.ToString(), TCsIDs.Select(x => x.Item2.ToString()).ToList());
            ((QCCore)ALMIntegration.Instance.AlmCore).UpdatedQCTestInBF(ref businessFlow, TCOfTestSetList);
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            List<QCTSTest> TCOfTestSetList = ((QCCore)ALMIntegration.Instance.AlmCore).GetTSQCTestsList(businessFlow.ExternalID.ToString());
            ((QCCore)ALMIntegration.Instance.AlmCore).UpdateBusinessFlow(ref businessFlow, TCOfTestSetList);
        }
        #endregion Import From QC


        #region Export To QC
        public override void ExportBfActivitiesGroupsToALM(GingerCore.BusinessFlow businessFlow, ObservableList<GingerCore.Activities.ActivitiesGroup> grdActivitiesGroups)
        {
            bool askToSaveBF = false;
            foreach (ActivitiesGroup group in grdActivitiesGroups)
            {
                if (ExportActivitiesGroupToALM(group))
                    askToSaveBF = true;
            }

            if (askToSaveBF)
                if (Reporter.ToUser(eUserMsgKeys.AskIfToSaveBFAfterExport, businessFlow.Name) == MessageBoxResult.Yes)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name,
                      GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.CloseGingerHelper();
                }
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            if (activtiesGroup == null) return false;
            Test matchingTC = null;

            //check if the ActivitiesGroup already mapped to QC Test Case
            if (String.IsNullOrEmpty(activtiesGroup.ExternalID) == false)
            {
                matchingTC = ((QCCore)ALMIntegration.Instance.AlmCore).GetQCTest(activtiesGroup.ExternalID);
                if (matchingTC != null)
                {
                    //ask user if want to continue
                    MessageBoxResult userSelec = Reporter.ToUser(eUserMsgKeys.ActivitiesGroupAlreadyMappedToTC, activtiesGroup.Name, matchingTC["TS_SUBJECT"].Path + "\\" + matchingTC.Name);
                    if (userSelec == MessageBoxResult.Cancel)
                        return false;
                    else if (userSelec == MessageBoxResult.No)
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
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;
            //TODO: retireve test case fields -->DONE
            ObservableList<ExternalItemFieldBase> testCaseFields = App.UserProfile.Solution.ExternalItemsFields;
            ALMIntegration.Instance.RefreshALMItemFields(testCaseFields, true, null);

            //Going through the fields to leave only Test Case fields
            for (int indx = 0; indx < testCaseFields.Count; indx++)
            {
                if(testCaseFields[indx].Name == "Version")
                {
                }
                if (testCaseFields[indx].ItemType != eQCItemType.TestCase.ToString())
                {
                    testCaseFields.RemoveAt(indx);
                    indx--;
                }
            }
            
            bool exportRes = ((QCCore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, uploadPath, testCaseFields, ref res);
            Reporter.CloseGingerHelper();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, activtiesGroup.Name, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));                    
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(activtiesGroup);
                    Reporter.CloseGingerHelper();
                }
                return true;
            }
            else
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), activtiesGroup.Name, res);
 
            return false;
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Silence, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null) return false;

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            TestSet matchingTS = null;

            MessageBoxResult userSelec = MessageBoxResult.None;
            //check if the businessFlow already mapped to QC Test Set
            if (String.IsNullOrEmpty(businessFlow.ExternalID) == false)
            {
                matchingTS = ((QCCore)ALMIntegration.Instance.AlmCore).GetQCTestSet(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continue
                    userSelec = Reporter.ToUser(eUserMsgKeys.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.TestSetFolder.Path + "\\" + matchingTS.Name);
                    if (userSelec == MessageBoxResult.Cancel)
                        return false;
                    else if (userSelec == MessageBoxResult.No)
                        matchingTS = null;
                }
            }

            //check if all of the business flow activities groups already exported to QC and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                if (string.IsNullOrEmpty(ag.ExternalID) == true || ((QCCore)ALMIntegration.Instance.AlmCore).GetQCTest(ag.ExternalID) == null)
                {
                    if (testPlanUploadPath == null)
                        testPlanUploadPath = SelectALMTestPlanPath();
                    if (string.IsNullOrEmpty(testPlanUploadPath) == false)
                        ExportActivitiesGroupToALM(ag, testPlanUploadPath);
                }
            }

            if (matchingTS == null && string.IsNullOrEmpty(testLabUploadPath))
            {
                if(userSelec == MessageBoxResult.No)
                    Reporter.ToUser(eUserMsgKeys.ExportQCNewTestSetSelectDiffFolder);

                //get the QC Test Plan path to upload the activities group to
                testLabUploadPath = SelectALMTestLabPath();
                if (String.IsNullOrEmpty(testLabUploadPath))
                {
                    //no path to upload to
                    return false;
                }
            }

            //upload the business flow
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;
            //TODO : need to update to retrieve only Test Set Item Fields -->DONE
            ObservableList<ExternalItemFieldBase> testSetFields = App.UserProfile.Solution.ExternalItemsFields;
            ALMIntegration.Instance.RefreshALMItemFields(testSetFields, true, null);

            for (int indx = 0; indx < testSetFields.Count; indx++)
            {
                if (testSetFields[indx].Name == "Version")
                {                    
                }

                if (testSetFields[indx].ItemType != eQCItemType.TestSet.ToString())
                {
                    testSetFields.RemoveAt(indx);
                    indx--;
                }
            }

            bool exportRes = ((QCCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToALM(businessFlow, matchingTS, testLabUploadPath, testSetFields, ref res);
            Reporter.CloseGingerHelper();
            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.CloseGingerHelper();
                }
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
                return true;
            }
            else
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            //}

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

        public override eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKeys.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
        }
    }
}
