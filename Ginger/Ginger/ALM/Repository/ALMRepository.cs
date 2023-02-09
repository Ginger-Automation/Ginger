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
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using GingerCore.ALM.QC;
using Amdocs.Ginger.Common.InterfacesLib;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;
using System.Windows.Controls;
using static Ginger.ALM.ZephyrEnt.ZephyrEntPlanningExplorerPage;

namespace Ginger.ALM.Repository
{
    abstract class ALMRepository
    {
        ALMItemsFieldsConfigurationPage mALMFieldsPage = null;
        ALMDefectsProfilesPage mALMDefectsProfilesPage = null;

        public abstract bool ConnectALMServer(eALMConnectType userMsgStyle);
        public abstract string SelectALMTestPlanPath();
        public abstract string SelectALMTestLabPath();
        public abstract bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, eALMConnectType almConectStyle = eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null);
        public abstract void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups);
        public abstract bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false, BusinessFlow businessFlow = null);
        public abstract void ImportALMTests(string importDestinationFolderPath);
        public virtual void ImportALMTestsById(string importDestinationFolderPath)
        {
            Reporter.ToUser(eUserMsgKey.OperationNotSupported, "Import by Id is not supported for configured ALM Type");
        }
        public abstract eUserMsgKey GetDownloadPossibleValuesMessage();
        public abstract IEnumerable<Object> SelectALMTestSets();
        public abstract bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> selectedTests);
        public abstract List<string> GetTestLabExplorer(string path);
        public abstract IEnumerable<Object> GetTestSetExplorer(string path);
        public abstract Object GetTSRunStatus(object tsItem);
        public abstract List<string> GetTestPlanExplorer(string path);
        public abstract bool ShowImportReviewPage(string importDestinationPath, object selectedTestPlan = null);
        public abstract bool LoadALMConfigurations();
        public abstract void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs);
        public abstract void UpdateBusinessFlow(ref BusinessFlow businessFlow);

        public void OpenALMItemsFieldsPage(eALMConfigType configType, eALMType type, ObservableList<ExternalItemFieldBase> ExternalItemsFields)
        {
            mALMFieldsPage = new ALMItemsFieldsConfigurationPage(configType, type, ExternalItemsFields);
            mALMFieldsPage.ShowAsWindow(true);
        }
        public void ALMDefectsProfilesPage()
        {
            if (mALMDefectsProfilesPage == null)
                mALMDefectsProfilesPage = new ALMDefectsProfilesPage();

            mALMDefectsProfilesPage.ShowAsWindow();
        }


        public void AddTestSetFlowToFolder(BusinessFlow businessFlow, string folderPath)
        {
            bool addItemToRootFolder = true;
            if (!string.IsNullOrEmpty(folderPath))
            {
                RepositoryFolderBase repositoryFolder = WorkSpace.Instance.SolutionRepository.GetRepositoryFolderByPath(folderPath);
                if (repositoryFolder != null)
                {
                    repositoryFolder.AddRepositoryItem(businessFlow);
                    addItemToRootFolder = false;
                }
            }
            if (addItemToRootFolder)
            {
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(businessFlow);
            }
        }
        public virtual Page GetALMTestSetsTreePage(string importDestinationPath = "")
        {
            throw new NotImplementedException();
        }
        public virtual Object GetSelectedImportTestSetData(Page page)
        {
            throw new NotImplementedException();
        }
        public virtual void GetALMTestSetData(ALMTestSet almTestSet)
        {
            throw new NotImplementedException();
        }
        public virtual ALMTestSet GetALMTestCasesToTestSetObject(ALMTestSet almTestSet)
        {
            throw new NotImplementedException();
        }
    }
}
