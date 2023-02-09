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
using Amdocs.Ginger.Repository;
using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using Ginger.ALM.Qtest;
using Ginger.ALM.Qtest.TreeViewItems;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.QC;
using GingerCore.ALM.Qtest;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM.Repository
{
    class QtestRepository : ALMRepository
    {
        QtestTest matchingTC = null;
        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
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
                if (userMsgStyle == eALMConnectType.Manual)
                {
                    Reporter.ToUser(eUserMsgKey.QcConnectFailure, e.Message);
                }
                else if (userMsgStyle == eALMConnectType.Auto)
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
            //show Test Modules for selecting the Path
            QtestModuleExplorerPage win = new QtestModuleExplorerPage();
            object selectedPathObject = win.ShowAsWindow(eWindowShowStyle.Dialog);
            if (selectedPathObject is QtestModuleTreeItem)
            {
                return ((QtestModuleTreeItem)selectedPathObject).ID.ToString();
            }
            return string.Empty;
        }

        public override string SelectALMTestLabPath()
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
                ALMIntegration.Instance.AlmCore.InitCoreObjs();

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
        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
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
                if (Reporter.ToUser(eUserMsgKey.AskIfToSaveBFAfterExport, businessFlow.Name) == eUserMsgSelection.Yes)
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
            for(var i = 0; i < activtiesGroup.ActivitiesIdentifiers.Where(ai => ai.IdentifiedActivity == null).ToList().Count; i++)
            {
                if (activtiesGroup.ActivitiesIdentifiers[i].IdentifiedActivity == null)
                {
                    activtiesGroup.ActivitiesIdentifiers[i].IdentifiedActivity = businessFlow.Activities.FirstOrDefault(x => x.Guid == activtiesGroup.ActivitiesIdentifiers[i].ActivityGuid);
                }
            }
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

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Silence, string parentObjectId = null, string testCaseParentObjectId = null)
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

            eUserMsgSelection userSelec = eUserMsgSelection.None;
            //check if the businessFlow already mapped to QC Test Set
            if (!String.IsNullOrEmpty(businessFlow.ExternalID))
            {
                matchingTS = ((QtestCore)ALMIntegration.Instance.AlmCore).GetQtestTestSuite(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continue
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.Name);
                    if (userSelec == eUserMsgSelection.Cancel)
                    {
                        return false;
                    }
                    else if (userSelec == eUserMsgSelection.No)
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
                        eUserMsgSelection userSelect = Reporter.ToUser(eUserMsgKey.ActivitiesGroupAlreadyMappedToTC, ag.Name, matchingTC.TestName);
                        if (userSelect == eUserMsgSelection.Cancel)
                        { 
                            return false; 
                        }
                        else if (userSelect == eUserMsgSelection.No)
                        { 
                            matchingTC = null; 
                        }
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
                    testCaseParentObjectId = SelectALMTestPlanPath();
                    if (String.IsNullOrEmpty(testCaseParentObjectId))
                    {
                        //no path to upload to
                        return false;
                    }
                    ExportActivitiesGroupToALM(ag, testCaseParentObjectId, false, businessFlow);
                }
                else
                {
                    ExportActivitiesGroupToALM(ag, parentObjectId, false, businessFlow);
                }

            }

            if (matchingTS == null && string.IsNullOrEmpty(parentObjectId))
            {
                if (userSelec == eUserMsgSelection.No)
                {
                    Reporter.ToUser(eUserMsgKey.ExportQCNewTestSetSelectDiffFolder);
                }
                parentObjectId = SelectALMTestLabPath();
                if (String.IsNullOrEmpty(parentObjectId))
                {
                    //no path to upload to
                    return false;
                }
            }
            else
            {
                parentObjectId = testCaseParentObjectId;
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
                if (almConectStyle != eALMConnectType.Auto)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
                return true;
            }
            else
            {
                if (almConectStyle != eALMConnectType.Auto)
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
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.zip",
                Filter = "zip Files (*.zip)|*.zip",
                Title = "Select Jira Configuration Zip File"
            }, false) is string fileName)
            {
                if (!GingerCore.General.LoadALMSettings(fileName, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest))
                {
                    return false;
                }
                ALMIntegration.Instance.SetALMCoreConfigurations(GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest);
            }
            return true;
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }
         
    }
}
