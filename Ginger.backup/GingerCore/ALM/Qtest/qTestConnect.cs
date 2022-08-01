#region License
/*
Copyright © 2014-2022 European Support Limited

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
using System;
using System.Collections.Generic;
using System.IO;
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

        QTestApi.LoginApi connObj = new QTestApi.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);

        public bool ConnectALMServer()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to qTest server");
                connObj = new QTestApi.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);
                string granttype = "password";
                string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
                string accessToken = ALMCore.DefaultAlmConfig.ALMPassword;
                string tokenType = "bearer";

                if (ALMCore.DefaultAlmConfig.UseToken)
                {
                    QTestApiModel.OAuthTokenStatusVM oAuthTokenStatusVM = connObj.TokenStatus(tokenType + " " + accessToken);
                    if (oAuthTokenStatusVM.ToString().ToLower().Contains("error"))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to connect qTest Server" + System.Environment.NewLine + oAuthTokenStatusVM.ToString());
                        return false;
                    }
                }
                else
                {
                    QTestApiModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, authorization);
                    accessToken = response.AccessToken;
                    tokenType = response.TokenType;
                }
                connObj.Configuration.AccessToken = accessToken;
                connObj.Configuration.ApiKey.Add("Authorization", accessToken);
                connObj.Configuration.ApiKeyPrefix.Add("Authorization", tokenType);

                string almConfigPackageFolderPathCalculated = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath);
                if (GingerCore.General.IsConfigPackageExists(almConfigPackageFolderPathCalculated, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest))
                {
                    connObj.Configuration.MyAPIConfig.LoadSettingsFromConfig(Path.Combine(almConfigPackageFolderPathCalculated, "QTestSettings", "QTestSettings.json"));
                }
                else
                {
                    connObj.Configuration.MyAPIConfig = new QTestApiClient.QTestClientConfig();
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to qTest server", ex);
                connObj = null;
                return false;
            }
        }

        public ObservableList<QTestApiModel.TestCycleResource> GetQTestCyclesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestApiModel.TestCycleResource> cyclestList;
            QTestApi.TestcycleApi TestcycleApi = new QTestApi.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestApiModel.TestCycleResource>(TestcycleApi.GetTestCycles((long)Convert.ToInt32(qTestProject), null, null, "descendants"));
            return cyclestList;
        }

        public ObservableList<QTestApiModel.ModuleResource> GetQTestModulesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestApiModel.ModuleResource> modulesList;
            QTestApi.ModuleApi moduleApi = new QTestApi.ModuleApi(connObj.Configuration);
            modulesList = new ObservableList<QTestApiModel.ModuleResource>(moduleApi.GetModules((long)Convert.ToInt32(qTestProject), null));
            return modulesList;
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
