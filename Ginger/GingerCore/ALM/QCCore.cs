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
using GingerCore.Activities;
using GingerCore.ALM.QC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TDAPIOLELib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;

namespace GingerCore.ALM
{
    public class QCCore : ALMCore
    {
        public QCCore() { }

        public override bool ConnectALMServer()
        {
            return QCConnect.ConnectQCServer(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword);
        }

        public override bool ConnectALMProject()
        {
            ALMCore.AlmConfig.ALMProjectName = ALMCore.AlmConfig.ALMProjectKey;
            return QCConnect.ConnectQCProject(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMDomain, ALMCore.AlmConfig.ALMProjectKey);
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
            ALMCore.AlmConfig.ALMDomain = ALMDomain;
            return QCConnect.GetQCDomainProjects(ALMCore.AlmConfig.ALMDomain);
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

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ALM_Common.DataContracts.ResourceType resourceType)
        {
            return UpdatedAlmFields(ImportFromQC.GetALMItemFields());
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
        {
            if (!useREST)
            {
                // do nothing
                return new Dictionary<Guid, string>();
            }
            else
            {
                return ImportFromQC.CreateNewDefectQCREST(defectsForOpening);
            }
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

        public QCTestSet ImportTestSetData(QCTestSet TS)
        {
            return ImportFromQC.ImportTestSetData(TS);
        }

        public void UpdatedQCTestInBF(ref BusinessFlow businessFlow, List<QCTSTest> tcsList)
        {
            ImportFromQC.UpdatedQCTestInBF(ref businessFlow, tcsList);
        }

        public void UpdateBusinessFlow(ref BusinessFlow businessFlow, List<QCTSTest> tcsList)
        {
            ImportFromQC.UpdateBusinessFlow(ref businessFlow, tcsList);
        }

        public BusinessFlow ConvertQCTestSetToBF(QCTestSet testSet)
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

        public List<QCTSTest> GetTSQCTestsList(string testSetID, List<string> TCsIDs = null)
        {
            return ImportFromQC.GetTSQCTestsList(testSetID, TCsIDs);
        }
    }
}
