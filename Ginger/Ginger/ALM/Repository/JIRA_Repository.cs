using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;

namespace Ginger.ALM.Repository
{
    class JIRA_Repository : ALMRepository
    {

        public ALMCore AlmCore { get; set; }

        public JIRA_Repository(ALMCore almCore)
        {
            this.AlmCore = almCore;
        }
        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Connecting to Jira server");
            try
            {
                isConnectSucc = this.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error connecting to Jira server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, "Could not connect to Jira server");
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailure);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings);
            }

            return isConnectSucc;
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
