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
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Activities;

namespace Ginger.ALM.Repository
{
    class JiraRepository : ALMRepository
    {
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            throw new NotImplementedException();
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            throw new NotImplementedException();
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            throw new NotImplementedException();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            throw new NotImplementedException();
        }

        public override eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTestLabExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetTestPlanExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<object> GetTestSetExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override object GetTSRunStatus(object tsItem)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTests(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            throw new NotImplementedException();
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
    }
}
