#region License
/*
Copyright © 2014-2021 European Support Limited

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
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace GingerCore.ALM.Qtest
{
    /// <summary>
    /// </summary>
    public class QtestConnect
    {
        private QtestConnect()
        {

        }

        #region singlton
        private static readonly QtestConnect _instance = new QtestConnect();
        public static QtestConnect Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion singlton
        
        public ObservableList<QTestApiModel.TestCycleResource> GetQTestCyclesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ObservableList<QTestApiModel.TestCycleResource> cyclestList;

            QTestApi.LoginApi connObj = new QTestApi.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);
            string granttype = "password";
            string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
            QTestApiModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, authorization);
            connObj.Configuration.AccessToken = response.AccessToken;
            connObj.Configuration.ApiKey.Add("Authorization", response.AccessToken);
            connObj.Configuration.ApiKeyPrefix.Add("Authorization", response.TokenType);

            QTestApi.TestcycleApi TestcycleApi = new QTestApi.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestApiModel.TestCycleResource>(TestcycleApi.GetTestCycles((long)Convert.ToInt32(qTestProject), null, null, "descendants"));

            return cyclestList;
        }
        public string ConvertResourceType(ALM_Common.DataContracts.ResourceType resourceType)
        {
            string qTestResourceType;
            switch (resourceType)
            {
                case ALM_Common.DataContracts.ResourceType.DEFECT:
                    qTestResourceType = "defects";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_CASE:
                    qTestResourceType = "test-cases";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_CYCLE:
                    qTestResourceType = "test-cycles";
                    break;
                case ALM_Common.DataContracts.ResourceType.REQUIREMENT:
                    qTestResourceType = "requirements";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_FOLDERS:
                    qTestResourceType = "modules";
                    break;
                case ALM_Common.DataContracts.ResourceType.RELEASES:
                    qTestResourceType = "releases";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_RUN:
                    qTestResourceType = "test-runs";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_SET:
                    qTestResourceType = "test-suites";
                    break;
                case ALM_Common.DataContracts.ResourceType.DESIGN_STEP:
                    qTestResourceType = "test-steps";
                    break;
                case ALM_Common.DataContracts.ResourceType.REQ_COVER:
                    qTestResourceType = "linked-artifacts";
                    break;
                case ALM_Common.DataContracts.ResourceType.ENUMERATIONS:
                    qTestResourceType = "fields";
                    break;
                case ALM_Common.DataContracts.ResourceType.TempSystem_Field:
                    qTestResourceType = "fields";
                    break;
                case ALM_Common.DataContracts.ResourceType.USERS:
                    qTestResourceType = "users";
                    break;
                case ALM_Common.DataContracts.ResourceType.LINK:
                    qTestResourceType = "linked-artifacts";
                    break;
                case ALM_Common.DataContracts.ResourceType.TEST_CASE_PARAMETERS:
                    qTestResourceType = "users";
                    break;
                case ALM_Common.DataContracts.ResourceType.PARAMETERS:
                    qTestResourceType = "parameters";
                    break;
                default:
                    qTestResourceType = "fields";
                    break;
            }

            return qTestResourceType;
        }
    }
}
