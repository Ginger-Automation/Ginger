#region License
/*
Copyright © 2014-2025 European Support Limited

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
using GingerCoreNET.GeneralLib;
using System;
using System.IO;

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

        QTestAPIStd.LoginApi connObj = new QTestAPIStd.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);

        public bool ConnectALMServer()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to qTest server");
                connObj = new QTestAPIStd.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);
                string granttype = "password";
                string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
                string accessToken = ALMCore.DefaultAlmConfig.ALMPassword;
                string tokenType = "bearer";

                if (ALMCore.DefaultAlmConfig.UseToken)
                {
                    QTestAPIStdModel.OAuthTokenStatusVM oAuthTokenStatusVM = connObj.TokenStatus(tokenType + " " + accessToken);
                    if (oAuthTokenStatusVM.ToString().ToLower().Contains("error"))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to connect qTest Server" + System.Environment.NewLine + oAuthTokenStatusVM.ToString());
                        return false;
                    }
                }
                else
                {
                    QTestAPIStdModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, authorization);
                    accessToken = response.AccessToken;
                    tokenType = response.TokenType;
                }
                connObj.Configuration.AccessToken = accessToken;
                connObj.Configuration.ApiKey.Add("Authorization", accessToken);
                connObj.Configuration.ApiKeyPrefix.Add("Authorization", tokenType);

                string almConfigPackageFolderPathCalculated = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath);
                if (General.IsConfigPackageExists(almConfigPackageFolderPathCalculated, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest))
                {
                    connObj.Configuration.MyAPIConfig.LoadSettingsFromConfig(Path.Combine(almConfigPackageFolderPathCalculated, "QTestSettings", "QTestSettings.json"));
                }
                else
                {
                    connObj.Configuration.MyAPIConfig = new QTestAPIStdClient.QTestClientConfig();
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

        public ObservableList<QTestAPIStdModel.TestCycleResource> GetQTestCyclesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestAPIStdModel.TestCycleResource> cyclestList;
            QTestAPIStd.TestcycleApi TestcycleApi = new QTestAPIStd.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestAPIStdModel.TestCycleResource>(TestcycleApi.GetTestCycles(Convert.ToInt32(qTestProject), null, null, "descendants"));
            return cyclestList;
        }

        public ObservableList<QTestAPIStdModel.ModuleResource> GetQTestModulesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestAPIStdModel.ModuleResource> modulesList;
            QTestAPIStd.ModuleApi moduleApi = new QTestAPIStd.ModuleApi(connObj.Configuration);
            modulesList = new ObservableList<QTestAPIStdModel.ModuleResource>(moduleApi.GetModules(Convert.ToInt32(qTestProject), null));
            return modulesList;
        }
        public string ConvertResourceType(AlmDataContractsStd.Enums.ResourceType resourceType)
        {
            var qTestResourceType = resourceType switch
            {
                AlmDataContractsStd.Enums.ResourceType.DEFECT => "defects",
                AlmDataContractsStd.Enums.ResourceType.TEST_CASE => "test-cases",
                AlmDataContractsStd.Enums.ResourceType.TEST_CYCLE => "test-cycles",
                AlmDataContractsStd.Enums.ResourceType.REQUIREMENT => "requirements",
                AlmDataContractsStd.Enums.ResourceType.TEST_FOLDERS => "modules",
                AlmDataContractsStd.Enums.ResourceType.RELEASES => "releases",
                AlmDataContractsStd.Enums.ResourceType.TEST_RUN => "test-runs",
                AlmDataContractsStd.Enums.ResourceType.TEST_SET => "test-suites",
                AlmDataContractsStd.Enums.ResourceType.DESIGN_STEP => "test-steps",
                AlmDataContractsStd.Enums.ResourceType.REQ_COVER => "linked-artifacts",
                AlmDataContractsStd.Enums.ResourceType.ENUMERATIONS => "fields",
                AlmDataContractsStd.Enums.ResourceType.TempSystem_Field => "fields",
                AlmDataContractsStd.Enums.ResourceType.USERS => "users",
                AlmDataContractsStd.Enums.ResourceType.LINK => "linked-artifacts",
                AlmDataContractsStd.Enums.ResourceType.TEST_CASE_PARAMETERS => "users",
                AlmDataContractsStd.Enums.ResourceType.PARAMETERS => "parameters",
                _ => "fields",
            };
            return qTestResourceType;
        }
    }
}
