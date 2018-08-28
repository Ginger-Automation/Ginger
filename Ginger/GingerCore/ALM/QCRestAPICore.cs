using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QCRestAPI;
using QCRestClient;

namespace GingerCore.ALM
{
    public class QCRestAPICore : ALMCore
    {
        public QCRestAPICore() { }

        public override bool ConnectALMServer()
        {
            return QCRestAPIConnect.ConnectQCServer(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword);
        }

        public override bool ConnectALMProject()
        {
            return QCRestAPIConnect.ConnectQCProject(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMDomain, ALMCore.AlmConfig.ALMProjectName);
        }

        public override void DisconnectALMServer()
        {
            QCRestAPIConnect.DisconnectQCServer();
        }

        public override List<string> GetALMDomains()
        {
            return QCRestAPIConnect.GetQCDomains();
        }

        public override List<string> GetALMDomainProjects(string ALMDomainName)
        {
            ALMCore.AlmConfig.ALMDomain = ALMDomainName;
            return QCRestAPIConnect.GetQCDomainProjects(ALMCore.AlmConfig.ALMDomain);
        }

        public QC.QCTestSet ImportTestSetData(QC.QCTestSet testSet)
        {
            return ImportFromQCRest.ImportTestSetData(testSet);
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet testSet)
        {
            return ImportFromQCRest.ConvertQCTestSetToBF(testSet);
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return QCRestAPIConnect.DisconnectQCProjectStayLoggedIn();
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, QCTestCase matchingTC, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string res)
        {
            return ExportToQCRestAPI.ExportActivitiesGroupToQC(activtiesGroup, matchingTC, uploadPath, testCaseFields, designStepsFields, designStepsParamsFields, ref res);
        }

        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ObservableList<ExternalItemFieldBase> testInstanceFields, ref string result)
        {
            return ExportToQCRestAPI.ExportBusinessFlowToQC(businessFlow, mappedTestSet, uploadPath, testSetFields, testInstanceFields, ref result);
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            ObservableList<ExternalItemFieldBase> runFields = GetALMItemFields(null, true, ALM_Common.DataContracts.ResourceType.TEST_RUN);
            return ExportToQCRestAPI.ExportExceutionDetailsToALM(bizFlow, ref result, runFields, exectutedFromAutomateTab, publishToALMConfig);
        }

        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get { return ImportFromQCRest.GingerActivitiesGroupsRepo; }
            set { ImportFromQCRest.GingerActivitiesGroupsRepo = value; }
        }

        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return ImportFromQCRest.GingerActivitiesRepo; }
            set { ImportFromQCRest.GingerActivitiesRepo = value; }
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ALM_Common.DataContracts.ResourceType resourceType)
        {
            return ImportFromQCRest.GetALMItemFields(resourceType);
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public QCTestSet GetQCTestSet(string testSetID)
        {
            return ImportFromQCRest.GetQCTestSet(testSetID);
        }

        public QCTestCase GetQCTest(string testID)
        {
            return ImportFromQCRest.GetQCTest(testID);
        }
    }
}
