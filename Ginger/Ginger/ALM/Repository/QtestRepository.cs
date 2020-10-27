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

using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using GingerCore;
using GingerCore.ALM;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.ALM.QC;
using Ginger.ALM.Qtest;
using System.Windows;
using System.IO;
using GingerCore.Platforms;
using Ginger.Repository;
using TDAPIOLELib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCore.ALM.Qtest;
using Ginger.ALM.Qtest.TreeViewItems;

namespace Ginger.ALM.Repository
{
    class QtestRepository : ALMRepository
    {
        QtestTest matchingTC = null;
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to QTest server");
                if (ALMIntegration.Instance.AlmCore.ConnectALMServer())
                {
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
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.QcConnectFailure, e.Message);
                }
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ALMConnectFailureWithCurrSettings, e.Message);
                }
                Reporter.ToLog(eLogLevel.WARN, "Error connecting to QTest server", e);
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
            //show Test Cycles browser for selecting the Path
            QtestCyclesExplorerPage win = new QtestCyclesExplorerPage(string.Empty, true);
            object selectedPathObject = win.ShowAsWindow(eWindowShowStyle.Dialog);
            if (selectedPathObject is QtestSuiteTreeItem)
            {
                return ((QtestSuiteTreeItem)selectedPathObject).ID.ToString();
            }
            if (selectedPathObject is QtestCycleTreeItem)
            {
                return ((QtestCycleTreeItem)selectedPathObject).ID.ToString();
            }
            return string.Empty;
        }

        public override string SelectALMTestLabPath()
        {
            //show Test Lab browser for selecting the Test Set/s
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
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

        #endregion General

        #region Import From Qtest
        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from Qtest");
            //set path to import to               
            if (importDestinationFolderPath == "")
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }

            //show Test Lab browser for selecting the Test Set/s to import
            QtestCyclesExplorerPage win = new QtestCyclesExplorerPage(importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> selectedTestSuites)
        {
            if (selectedTestSuites != null && selectedTestSuites.Count() > 0)
            {
                ObservableList<QtestSuiteTreeItem> testSuitesItemsToImport = new ObservableList<QtestSuiteTreeItem>();                
                foreach (QtestSuiteTreeItem testSuiteItem in selectedTestSuites)
                {
                    //check if some of the Test Set was already imported                
                    if (testSuiteItem.AlreadyImported)
                    {
                        Amdocs.Ginger.Common.eUserMsgSelection userSelection = Reporter.ToUser(eUserMsgKey.TestSetExists, testSuiteItem.Name);
                        if (userSelection == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            //Delete the mapped BF
                            File.Delete(testSuiteItem.MappedBusinessFlow.FileName);
                            testSuitesItemsToImport.Add(testSuiteItem);
                        }
                    }
                    else
                    {
                        testSuitesItemsToImport.Add(testSuiteItem);
                    }
                }

                if (testSuitesItemsToImport.Count == 0)
                {
                    return false; //noting to import
                }

                //Refresh Ginger repository and allow GingerQC to use it
                ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();               
                ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();

                foreach (QtestSuiteTreeItem testSetItemtoImport in testSuitesItemsToImport)
                {
                    try
                    {
                        Reporter.ToStatus(eStatusMsgKey.ALMTestSetImport, null, testSetItemtoImport.Name);
                        QtestTestSuite TS = new QtestTestSuite();
                        TS.ID = testSetItemtoImport.ID;
                        TS.Name = testSetItemtoImport.Name;
                        TS = ((QtestCore)ALMIntegration.Instance.AlmCore).ImportTestSetData(TS);

                        //convert test set into BF
                        BusinessFlow tsBusFlow = ((QtestCore)ALMIntegration.Instance.AlmCore).ConvertQCTestSetToBF(TS);

                        if ( WorkSpace.Instance.Solution.MainApplication != null)
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
                        Reporter.ToUser(eUserMsgKey.ErrorInTestsetImport, testSetItemtoImport.Name, ex.Message);
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

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string parentObjectId = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
        {
            if (activtiesGroup == null)
            {
                return false;
            }
            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;
            //TODO: retireve test case fields -->DONE
            ObservableList<ExternalItemFieldBase> testCaseFields =  WorkSpace.Instance.Solution.ExternalItemsFields;
            var filterTestCaseFields = testCaseFields.Where(tc => tc.ItemType == eQCItemType.TestCase.ToString()).ToList();
            bool exportRes = ((QtestCore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, parentObjectId, new ObservableList<ExternalItemFieldBase>(filterTestCaseFields), ref res);
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

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Silence, string parentObjectId = null, string testLabUploadPath = null)
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

            QtestTestSuite matchingTS = null;

            Amdocs.Ginger.Common.eUserMsgSelection userSelec = Amdocs.Ginger.Common.eUserMsgSelection.None;
            //check if the businessFlow already mapped to QC Test Set
            if (!String.IsNullOrEmpty(businessFlow.ExternalID))
            {
                matchingTS = ((QtestCore)ALMIntegration.Instance.AlmCore).GetQtestTestSuite(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continue
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.Name);
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
                //check if the ActivitiesGroup already mapped to Qtest Test Case
                matchingTC = null;
                if (!String.IsNullOrEmpty(ag.ExternalID))
                {
                    matchingTC = ((QtestCore)ALMIntegration.Instance.AlmCore).GetQtestTest((long)Convert.ToInt32(ag.ExternalID));
                    if (matchingTC != null)
                    {
                        //ask user if want to continue
                        Amdocs.Ginger.Common.eUserMsgSelection userSelect = Reporter.ToUser(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, ag.Name, matchingTC.TestName);
                        if (userSelect == Amdocs.Ginger.Common.eUserMsgSelection.Cancel)
                        { return false; }
                        else if (userSelect == Amdocs.Ginger.Common.eUserMsgSelection.No)
                        { matchingTC = null; }
                        else
                        {
                            parentObjectId = matchingTC.TestID;
                        }
                    }
                }

                //if user selected No and want to create new testplans to selected folder path
                if (matchingTC == null && String.IsNullOrEmpty(parentObjectId))
                {
                    //get the QC Test Plan path to upload the activities group to
                    parentObjectId = SelectALMTestPlanPath();
                    if (String.IsNullOrEmpty(parentObjectId))
                    {
                        //no path to upload to
                        return false;
                    }
                }
                ExportActivitiesGroupToALM(ag, parentObjectId);
            }

            if (matchingTS == null && string.IsNullOrEmpty(parentObjectId))
            {
                if (userSelec == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    Reporter.ToUser(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder);
                }
            }

            //upload the business flow
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;
            //TODO : need to update to retrieve only Test Set Item Fields -->DONE
            ObservableList<ExternalItemFieldBase> testSetFields =  WorkSpace.Instance.Solution.ExternalItemsFields;
            // ALMIntegration.Instance.RefreshALMItemFields(testSetFields, true, null);    // Arvind to merge it from here
            var filterTestSetFields = testSetFields.Where(tc => tc.ItemType == eQCItemType.TestSet.ToString()).ToList();
            bool exportRes = ((QtestCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToALM(businessFlow, matchingTS, parentObjectId, new ObservableList<ExternalItemFieldBase> (filterTestSetFields), ref res);
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
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
                return true;
            }
            else
            {
                if (almConectStyle != ALMIntegration.eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
                }
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

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            return;
        }
    }
}
