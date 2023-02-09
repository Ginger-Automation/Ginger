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
using GingerCore.Activities;
using GingerCore.ALM.QC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TDAPIOLELib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.ALMLib;
using AlmDataContractsStd.Enums;

namespace GingerCore.ALM
{
    public class QCCore : ALMCore
    {
        public QCCore() { }

        public override bool ConnectALMServer()
        {
            return QCConnect.ConnectQCServer(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword);
        }

        public override bool ConnectALMProject()
        {
            ALMCore.DefaultAlmConfig.ALMProjectName = ALMCore.DefaultAlmConfig.ALMProjectKey;
            return QCConnect.ConnectQCProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMDomain, ALMCore.DefaultAlmConfig.ALMProjectKey);
        }

        public override Boolean IsServerConnected()
        {
            return QCConnect.IsServerConnected;
        }

        public override void DisconnectALMServer()
        {
            QCConnect.DisconnectQCServer();
        }

        public override List<string> GetALMDomains()
        {
            return QCConnect.GetQCDomains();
        }
        public override Dictionary<string,string> GetALMDomainProjects(string ALMDomain)
        {
            ALMCore.DefaultAlmConfig.ALMDomain = ALMDomain;
            return QCConnect.GetQCDomainProjects(ALMCore.DefaultAlmConfig.ALMDomain);
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return QCConnect.DisconnectQCProjectStayLoggedIn();
        }

        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get { return ImportFromQC.GingerActivitiesGroupsRepo; }
            set { ImportFromQC.GingerActivitiesGroupsRepo = value; }
        }

        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return ImportFromQC.GingerActivitiesRepo; }
            set { ImportFromQC.GingerActivitiesRepo = value; }
        }
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get { return ImportFromQC.ApplicationPlatforms; }
            set { ImportFromQC.ApplicationPlatforms = value; }
        }

        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.QC;

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType)
        {
            return UpdatedAlmFields(ImportFromQC.GetALMItemFields());
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
        {
            return new Dictionary<Guid, string>();     
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            return ExportToQC.ExportExecutionDetailsToQC(bizFlow, ref result, publishToALMConfig);
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, Test mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
        {
            return ExportToQC.ExportActivitiesGroupToQC(activitiesGroup, mappedTest, uploadPath, testCaseFields, ref result);
        }

        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, TestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ref string result)
        {
            return ExportToQC.ExportBusinessFlowToQC(businessFlow, mappedTestSet, uploadPath, testSetFields, ref result);
        }

        public ALMTestSet ImportTestSetData(ALMTestSet TS)
        {
            return ImportFromQC.ImportTestSetData(TS);
        }

        public void UpdatedQCTestInBF(ref BusinessFlow businessFlow, List<ALMTSTest> tcsList)
        {
            ImportFromQC.UpdatedQCTestInBF(ref businessFlow, tcsList);
        }

        public void UpdateBusinessFlow(ref BusinessFlow businessFlow, List<ALMTSTest> tcsList)
        {
            ImportFromQC.UpdateBusinessFlow(ref businessFlow, tcsList);
        }

        public BusinessFlow ConvertQCTestSetToBF(ALMTestSet testSet)
        {
            return ImportFromQC.ConvertQCTestSetToBF(testSet);
        }

        public Test GetQCTest(string testID)
        {
            return ImportFromQC.GetQCTest(testID);
        }

        public TestSet GetQCTestSet(string testSetID)
        {
            return ImportFromQC.GetQCTestSet(testSetID);
        }

        public List<ALMTSTest> GetTSQCTestsList(string testSetID, List<string> TCsIDs = null)
        {
            return ImportFromQC.GetTSQCTestsList(testSetID, TCsIDs);
        }
    }
}
