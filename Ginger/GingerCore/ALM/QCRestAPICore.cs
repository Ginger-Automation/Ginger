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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Linq;
//using ALM_Common.DataContracts;
using AlmDataContractsStd.Enums;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QCRestAPI;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using QCRestClientStd;

namespace GingerCore.ALM
{
    public class QCRestAPICore : ALMCore
    {
        public QCRestAPICore() { }

        public override bool ConnectALMServer()
        {
            return QCRestAPIConnect.ConnectQCServer(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword);
        }

        public override bool ConnectALMProject()
        {
            ALMCore.DefaultAlmConfig.ALMProjectName = ALMCore.DefaultAlmConfig.ALMProjectKey;
            return QCRestAPIConnect.ConnectQCProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMDomain, ALMCore.DefaultAlmConfig.ALMProjectName);
        }

        public override void DisconnectALMServer()
        {
            QCRestAPIConnect.DisconnectQCServer();
        }

        public override List<string> GetALMDomains()
        {
            return QCRestAPIConnect.GetQCDomains();
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            ALMCore.DefaultAlmConfig.ALMDomain = ALMDomainName;
            return QCRestAPIConnect.GetQCDomainProjects(ALMCore.DefaultAlmConfig.ALMDomain);
        }

        public QC.ALMTestSet ImportTestSetData(QC.ALMTestSet testSet)
        {
            return ImportFromQCRest.ImportTestSetData(testSet);
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.ALMTestSet testSet)
        {
            return ImportFromQCRest.ConvertQCTestSetToBF(testSet);
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            return ImportFromQCRest.CreateNewDefectQCREST(defectsForOpening);
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
            ResourceType resource = (ResourceType)AlmDataContractsStd.Enums.ResourceType.TEST_RUN;
            ObservableList<ExternalItemFieldBase> runFields = GetALMItemFields(null, true, (AlmDataContractsStd.Enums.ResourceType)resource);
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
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get { return ImportFromQCRest.ApplicationPlatforms; }
            set { ImportFromQCRest.ApplicationPlatforms = value; }
        }

        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.QC;

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

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, AlmDataContractsStd.Enums.ResourceType resourceType = AlmDataContractsStd.Enums.ResourceType.ALL)
        {
            return UpdatedAlmFields(ImportFromQCRest.GetALMItemFields((ResourceType)resourceType));
        }
    }
}
