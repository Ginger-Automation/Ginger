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
            ObservableList<QTestApiModel.TestCycleResource> cyclestList = new ObservableList<QTestApiModel.TestCycleResource>();

            QTestApi.LoginApi connObj = new QTestApi.LoginApi(ALMCore.AlmConfig.ALMServerURL);
            string granttype = "password";
            string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
            QTestApiModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, authorization);
            connObj.Configuration.AccessToken = response.AccessToken;
            connObj.Configuration.ApiKey.Add("Authorization", response.AccessToken);
            connObj.Configuration.ApiKeyPrefix.Add("Authorization", response.TokenType);

            QTestApi.TestcycleApi TestcycleApi = new QTestApi.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestApiModel.TestCycleResource>(TestcycleApi.GetTestCycles((long)Convert.ToInt32(qTestProject), null, null, "descendants"));

            return cyclestList;
        }
    }
}
