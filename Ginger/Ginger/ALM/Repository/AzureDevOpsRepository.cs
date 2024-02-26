#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.CoreNET.ALMLib.DataContract;
using Amdocs.Ginger.Repository;
using Ginger.ALM.QC;
using Ginger.ALM.QC.TreeViewItems;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using System;
using System.Collections.Generic;
using System.Windows;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace Ginger.ALM.Repository
{
    class AzureDevOpsRepository : ALMRepository
    {
        AzureDevOpsCore AzureCore;
        ALMTestCase matchingTC = null;

        public AzureDevOpsRepository(ALMCore almcore) 
        { 
            AzureCore = (AzureDevOpsCore)almcore;
        }
        public override bool ConnectALMServer(eALMConnectType userMsgStyle)
        {
            return AzureCore.ConnectALMProject();
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null)
        {
            if (activtiesGroup == null) { return false; }
            //if it is called from shared repository need to select path
          
            //upload the Activities Group
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, activtiesGroup.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);

            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            ObservableList<ExternalItemFieldBase> testCaseFields = CleanUnrelvantFields(allFields, "Test Case");

            bool exportRes = ((AzureDevOpsCore)ALMIntegration.Instance.AlmCore).ExportActivitiesGroupToALM(activtiesGroup, matchingTC, uploadPath, testCaseFields, null, null, ref res);

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

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            throw new NotImplementedException();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null)
            {
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0 && almConectStyle != eALMConnectType.Silence)
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }
           

            ALMTestSetData matchingTS = null;

            eUserMsgSelection userSelec;
            //TO DO MaheshK : check if the businessFlow already mapped to Octane Test Suite
            if (!String.IsNullOrEmpty(businessFlow.ExternalID))
            {
                matchingTS = ((AzureDevOpsCore)ALMIntegration.Instance.AlmCore).GetTestSuiteById(businessFlow.ExternalID);
                if (matchingTS != null)
                {
                    //ask user if want to continute
                    userSelec = Reporter.ToUser(eUserMsgKey.BusinessFlowAlreadyMappedToTC, businessFlow.Name, matchingTS.Name);
                    if (userSelec == eUserMsgSelection.Cancel)
                    {
                        return false;
                    }
                    else if (userSelec == eUserMsgSelection.No)
                    {
                        matchingTS = null;
                    }
                    else
                    {
                        if (String.IsNullOrEmpty(testPlanUploadPath))
                        {
                            testPlanUploadPath = matchingTS.ParentId;
                        }
                    }
                }
            }


            bool performSave = false;

           
            //check if all of the business flow activities groups already exported to Octane and export the ones which not
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                ExportActivitiesGroupToALM(ag, testPlanUploadPath, performSave, businessFlow);
            }
            testLabUploadPath = testPlanUploadPath;

            //upload the business flow
            Reporter.ToStatus(eStatusMsgKey.ExportItemToALM, null, businessFlow.Name);
            string res = string.Empty;

            ObservableList<ExternalItemFieldBase> allFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
            ALMIntegration.Instance.RefreshALMItemFields(allFields, true, null);

            ObservableList<ExternalItemFieldBase> testSetFieldsFields = CleanUnrelvantFields(allFields, "Test Suite");

            bool exportRes = ((AzureDevOpsCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlow(businessFlow, matchingTS, testLabUploadPath, testSetFieldsFields, null, ref res);

            Reporter.HideStatusMessage();
            if (exportRes)
            {
                ((AzureDevOpsCore)ALMIntegration.Instance.AlmCore).TestCaseEntryInSuite(businessFlow);
                if (performSaveAfterExport)
                {
                    Reporter.ToStatus(eStatusMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                    Reporter.HideStatusMessage();
                }
                if (almConectStyle != eALMConnectType.Auto && almConectStyle != eALMConnectType.Silence)
                {
                    Reporter.ToUser(eUserMsgKey.ExportItemToALMSucceed);
                }
                return true;
            }
            else
                if (almConectStyle != eALMConnectType.Auto && almConectStyle != eALMConnectType.Silence)
            {
                Reporter.ToUser(eUserMsgKey.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            return false;
            
            
        }

        public override eUserMsgKey GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKey.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            return AzureCore.GetTestLabExplorer(path);
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            return AzureCore.GetTestLabExplorer(path);
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            return AzureCore.GetTestSetExplorer(path);
        }



        public override object GetTSRunStatus(object tsItem)
        {
            return AzureCore.GetTSRunStatus(tsItem);
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Start importing from Azure");
            //set path to import to               
            if (importDestinationFolderPath == "")
            {
                importDestinationFolderPath = WorkSpace.Instance.Solution.BusinessFlowsMainFolder;
            }
            //show Test Lab browser for selecting the Test Set/s to import
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.Import, importDestinationFolderPath);
            win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<object> selectedTests)
        {
            throw new NotImplementedException();
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestLabPath()
        {
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override string SelectALMTestPlanPath()
        {
            QCTestLabExplorerPage win = new QCTestLabExplorerPage(QCTestLabExplorerPage.eExplorerTestLabPageUsageType.BrowseFolders);
            return (string)win.ShowAsWindow(eWindowShowStyle.Dialog);
        }

        public override IEnumerable<object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null)
        {
            throw new NotImplementedException();
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            throw new NotImplementedException();
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            throw new NotImplementedException();
        }
        private ObservableList<ExternalItemFieldBase> CleanUnrelvantFields(ObservableList<ExternalItemFieldBase> fields, string resourceType)
        {
            ObservableList<ExternalItemFieldBase> fieldsToReturn = new ObservableList<ExternalItemFieldBase>();

            //Going through the fields to leave only Test Set fields
            for (int indx = 0; indx < fields.Count; indx++)
            {
                if (fields[indx].ItemType == resourceType)
                {
                    fieldsToReturn.Add(fields[indx]);
                }
            }

            return fieldsToReturn;
        }
    }
}
