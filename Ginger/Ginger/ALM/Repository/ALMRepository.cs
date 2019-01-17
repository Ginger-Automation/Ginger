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
using GingerCore;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using GingerCore.ALM.QC;
using Amdocs.Ginger.Common.InterfacesLib;
using amdocs.ginger.GingerCoreNET;

namespace Ginger.ALM.Repository
{
    abstract class ALMRepository
    {
        ALMItemsFieldsConfigurationPage mALMFieldsPage = null;
        ALMDefectsProfilesPage mALMDefectsProfilesPage = null;

        public string ALMPassword()
        {
            return  WorkSpace.UserProfile.ALMPassword;
        }

        public void SetALMPassword(string newPassword)
        {
             WorkSpace.UserProfile.ALMPassword = newPassword;
        }

        public void SetALMProject(KeyValuePair<string, string> project)
        {
            WorkSpace.UserProfile.Solution.ALMProject = project.Value;
            WorkSpace.UserProfile.Solution.ALMProjectKey = project.Key;
        }

        public abstract bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle);
        public abstract string SelectALMTestPlanPath();
        public abstract string SelectALMTestLabPath();
        public abstract bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null);
        public abstract void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups);
        public abstract bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false);
        public abstract void ImportALMTests(string importDestinationFolderPath);
        public abstract void ImportALMTestsById(string importDestinationFolderPath);
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
        public void OpenALMItemsFieldsPage()
        {
            if (mALMFieldsPage == null)
                mALMFieldsPage = new ALMItemsFieldsConfigurationPage();

            mALMFieldsPage.ShowAsWindow();
        }
        public void ALMDefectsProfilesPage()
        {
            if (mALMDefectsProfilesPage == null)
                mALMDefectsProfilesPage = new ALMDefectsProfilesPage();

            mALMDefectsProfilesPage.ShowAsWindow();
        }

    }
}
