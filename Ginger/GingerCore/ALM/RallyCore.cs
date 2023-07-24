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

//#region License
///*
//Copyright © 2014-2023 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using System;
//using System.Collections.Generic;
//using GingerCore.Activities;
//using System.ComponentModel;
//using RallyPack.Lib;
//using Rally.RestApi.Json;
//using Rally.RestApi.Response;
//using GingerCore.ALM.Rally;
//using Amdocs.Ginger.Repository;
//using System.Linq;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.ALMLib;

//namespace GingerCore.ALM
//{
//    /// <summary>
//    /// </summary>
//    public class RallyCore : ALMCore
//    {
//        public static string ALMProjectGroupName { get; set; }
//        public static string ALMProjectGuid { get; set; }
//        public static string ExportSettingsFilePath { get; set; }
//        RallyRestApi restApi = new RallyRestApi();

//        private static string mResourcesFolder;
//        public static string ResourcesFolder {
//            get
//            {
//                if(string.IsNullOrEmpty(mResourcesFolder))
//                    mResourcesFolder = System.IO.Path.Combine((System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)), @"ALM\Rally");
//                return mResourcesFolder;
//            }
//        }

//        private static string mRallySolutionFolder;
//        public static string RallySolutionFolder
//        {
//            get
//            {
//                if(string.IsNullOrEmpty(mRallySolutionFolder))
//                    mRallySolutionFolder = System.IO.Path.Combine(ALMCore.SolutionFolder, "Document", "ALM", "Rally");
//                return mRallySolutionFolder;
//            }
//        }

//        public override bool ConnectALMServer()
//        {
//            string username = ALMCore.DefaultAlmConfig.ALMUserName;
//            string password = ALMCore.DefaultAlmConfig.ALMPassword;
//            string serverUrl = ALMCore.DefaultAlmConfig.ALMServerURL;

//            try
//            {              
//                restApi.Authenticate(username, password, serverUrl, proxy: null, allowSSO: false);
//                return true;
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }            
//        }

//        public override bool ConnectALMProject()
//        {            
//            try
//            {
//                restApi.Authenticate(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, proxy: null, allowSSO: false);
//                if (restApi.AuthenticationState == RallyRestApi.AuthenticationResult.Authenticated)
//                {
//                    return true; 
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            return false;
//        }

//        public override void DisconnectALMServer()
//        {
//            //RallyConnect.Instance.DisconnectRallyServer();
//        }

//        public override bool DisconnectALMProjectStayLoggedIn()
//        {
//            return true;//RallyConnect.Instance.DisconnectALMProjectStayLoggedIn();
//        }

//        public override bool IsServerConnected()
//        {
//            return true;//RallyConnect.Instance.IsServerConnected();
//        }

//        public override List<string> GetALMDomains()
//        {
//            List<string> domains = new List<string>();
//            try
//            {
//                if (!string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMUserName) &&  !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMPassword) && !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMServerURL))
//                {
//                    if (restApi.AuthenticationState != RallyRestApi.AuthenticationResult.Authenticated)
//                    {
//                        restApi.Authenticate(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, proxy: null, allowSSO: false);
//                    }

//                    DynamicJsonObject sub = restApi.GetSubscription("Workspaces");

//                    Request wRequest = new Request(sub["Workspaces"]);
//                    wRequest.Limit = 1000;
//                    QueryResult queryResult = restApi.Query(wRequest);
//                    foreach (var result in queryResult.Results)
//                    {
//                        domains.Add(Convert.ToString(result["Name"]));
//                    } 
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            return domains;
//        }

//        public override Dictionary<string,string> GetALMDomainProjects(string ALMDomain)
//        {
//            List<string> projects = new List<string>();
//            try
//            {
//                if (!string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMUserName) && !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMPassword) && !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMServerURL))
//                {
//                    if (restApi.AuthenticationState != RallyRestApi.AuthenticationResult.Authenticated)
//                    {
//                        restApi.Authenticate(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, proxy: null, allowSSO: false);
//                    }

//                    DynamicJsonObject sub = restApi.GetSubscription("Workspaces");

//                    Request wRequest = new Request(sub["Workspaces"]);
//                    wRequest.Limit = 1000;
//                    QueryResult queryResult = restApi.Query(wRequest);
//                    foreach (var result in queryResult.Results)
//                    {
//                        Request projectsRequest = new Request(result["Projects"]);
//                        projectsRequest.Fetch = new List<string>()
//                    {
//                        "Name"
//                    };
//                        projectsRequest.Limit = 10000;
//                        QueryResult queryProjectResult = restApi.Query(projectsRequest);
//                        int projectsPerWorkspace = 0;
//                        foreach (var p in queryProjectResult.Results)
//                        {
//                            projectsPerWorkspace++;
//                            projects.Add(Convert.ToString(p["Name"]));
//                        }
//                    } 
//                }
//            }
//            catch (Exception ex)
//            {
//                throw ex;
//            }
//            return projects.ToDictionary(prj => prj, prj => prj); ;
//        }

//        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
//        {
//            return true;//ExportToRally.Instance.ExportExecutionDetailsToRally(bizFlow, ref result);
//        }
//        public bool ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups, ref string result)
//        {
//            return true;//ExportToRally.Instance.ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups, ref result);
//        }

//        public bool ExportBusinessFlowToRally(BusinessFlow businessFlow, ObservableList<ExternalItemFieldBase> ExternalItemsFields, ref string result)
//        {
//            return true;//ExportToRally.Instance.ExportBusinessFlowToRally(businessFlow, ExternalItemsFields, ref result);
//        }

//        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, AlmDataContractsStd.Enums.ResourceType resourceType)
//        {
//            return null;
//        }

//        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
//        {
//            return null;
//        }

//        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
//        {
//            get { return ImportFromRally.GingerActivitiesGroupsRepo; }
//            set { ImportFromRally.GingerActivitiesGroupsRepo = value; }
//        }

//        public override ObservableList<Activity> GingerActivitiesRepo
//        {
//            get { return ImportFromRally.GingerActivitiesRepo; }
//            set { ImportFromRally.GingerActivitiesRepo = value; }
//        }

//        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
//        {
//            get { return ImportFromRally.ApplicationPlatforms; }
//            set { ImportFromRally.ApplicationPlatforms = value; }
//        }

//        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.RALLY;

//        public BusinessFlow ConvertRallyTestPlanToBF(RallyTestPlan testPlan)
//        {
//            return ImportFromRally.ConvertRallyTestPlanToBF(testPlan);
//        }
//    }
//}
