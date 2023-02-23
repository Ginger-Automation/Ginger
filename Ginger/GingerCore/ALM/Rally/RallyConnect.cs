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
//using System.Windows.Input;
//using Rally.RestApi;
//using Rally.RestApi.Json;
//using Rally.RestApi.Response;

//namespace GingerCore.ALM.Rally
//{
//    /// <summary>
//    /// </summary>
//    public class RallyConnect
//    {
//        private RallyConnect()
//        {

//        }

//        #region singlton
//        private static readonly RallyConnect _instance = new RallyConnect();
//        public static RallyConnect Instance
//        {
//            get
//            {
//                return _instance;
//            }
//        }
//        #endregion singlton
        
//        public ObservableList<RallyTestPlan> GetRallyTestPlansByProject(string RallyServerUrl, string RallyUserName, string RallyPassword, string RallyProject, string solutionFolder, string projName)
//        {
//            ObservableList<RallyTestPlan> rallyTestPlanList = new ObservableList<RallyTestPlan>();

//            RallyRestApi restApi = new RallyRestApi();
//            restApi.Authenticate(RallyUserName, RallyPassword, RallyServerUrl, proxy: null, allowSSO: false);

//            if (restApi.AuthenticationState == RallyRestApi.AuthenticationResult.Authenticated)
//            {
//                DynamicJsonObject sub = restApi.GetSubscription("Workspaces");
//                Request wRequest = new Request(sub["Workspaces"]);
//                wRequest.Limit = 1000;
//                int projectId = 0;
//                QueryResult queryWSResult = restApi.Query(wRequest);
//                foreach (var result in queryWSResult.Results)
//                {
//                    Request projectsRequest = new Request(result["Projects"]);
//                    projectsRequest.Fetch = new List<string>()
//                            {
//                                "Name", "ObjectID"
//                            };
//                    projectsRequest.Limit = 10000;
//                    QueryResult queryProjectResult = restApi.Query(projectsRequest);
//                    foreach (var p in queryProjectResult.Results)
//                    {
//                        if (Convert.ToString(p["Name"]) == projName)
//                        {
//                            int.TryParse(Convert.ToString(p["ObjectID"]), out projectId);
//                            break;
//                        }
//                    }
//                }

//                if (projectId > 0)
//                {
//                    //Query for items
//                    Request request = new Request("testset");
//                    var projectRef = "/project/" + projectId;
//                    request.Query = new Query("Project", Query.Operator.Equals, projectRef);

//                    QueryResult queryTestSetResult = restApi.Query(request);
//                    foreach (var result in queryTestSetResult.Results)
//                    {
//                        RallyTestPlan plan = new RallyTestPlan();
//                        int testSetId = 0;
//                        int.TryParse(Convert.ToString(result["ObjectID"]), out testSetId);

//                        if (testSetId > 0)
//                        {
//                            plan.Name = Convert.ToString(result["Name"]);
//                            plan.RallyID = Convert.ToString(result["ObjectID"]);
//                            plan.Description = Convert.ToString(result["Description"]);
//                            plan.CreatedBy = Convert.ToString(result["Owner"]["_refObjectName"]);
//                            plan.CreationDate = Convert.ToDateTime(result["CreationDate"]);
//                            plan.TestCases = new ObservableList<RallyTestCase>();
//                        }
//                        rallyTestPlanList.Add(plan);
//                    }
//                } 
//            }


//            return rallyTestPlanList;
//        }

//        public RallyTestPlan GetRallyTestPlanFullData(string RallyServerUrl, string RallyUserName, string RallyPassword, string RallyProject, RallyTestPlan testPlan)
//        {
//            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

//            RallyTestPlan plan = new RallyTestPlan();

//            RallyRestApi restApi = new RallyRestApi();
//            restApi.Authenticate(RallyUserName, RallyPassword, RallyServerUrl, proxy: null, allowSSO: false);

//            if (restApi.AuthenticationState == RallyRestApi.AuthenticationResult.Authenticated)
//            {
//                DynamicJsonObject sub = restApi.GetSubscription("Workspaces");
//                Request wRequest = new Request(sub["Workspaces"]);
//                wRequest.Limit = 1000;
//                int projectId = 0;
//                QueryResult queryWSResult = restApi.Query(wRequest);
//                foreach (var result in queryWSResult.Results)
//                {
//                    Request projectsRequest = new Request(result["Projects"]);
//                    projectsRequest.Fetch = new List<string>()
//                            {
//                                "Name", "ObjectID"
//                            };
//                    projectsRequest.Limit = 10000;
//                    QueryResult queryProjectResult = restApi.Query(projectsRequest);
//                    foreach (var p in queryProjectResult.Results)
//                    {
//                        if (Convert.ToString(p["Name"]) == RallyProject)
//                        {
//                            int.TryParse(Convert.ToString(p["ObjectID"]), out projectId);
//                            break;
//                        }
//                    }
//                }

//                if (projectId > 0)
//                {
//                    //Query for items
//                    Request request = new Request("testset");
//                    var projectRef = "/project/" + projectId;
//                    request.Query = new Query("Project", Query.Operator.Equals, projectRef);

//                    QueryResult queryTestSetResult = restApi.Query(request);
//                    foreach (var result in queryTestSetResult.Results)
//                    {
//                        int testSetId = 0;
//                        int.TryParse(Convert.ToString(result["ObjectID"]), out testSetId);

//                        if (testSetId > 0 && testPlan.RallyID == Convert.ToString(result["ObjectID"]))
//                        {
//                            plan.Name = Convert.ToString(result["Name"]);
//                            plan.RallyID = Convert.ToString(result["ObjectID"]);
//                            plan.Description = Convert.ToString(result["Description"]);
//                            plan.CreatedBy = Convert.ToString(result["Owner"]["_refObjectName"]);
//                            plan.CreationDate = Convert.ToDateTime(result["CreationDate"]);
//                            plan.TestCases = new ObservableList<RallyTestCase>();

//                            Request reqTestCase = new Request("testcase");
//                            var testSetIdRef = "/testset/" + testSetId;
//                            reqTestCase.Query = new Query("TestSets", Query.Operator.Equals, testSetIdRef);

//                            QueryResult queryTestcaseResult = restApi.Query(reqTestCase);
//                            foreach (var testCaseResult in queryTestcaseResult.Results)
//                            {
//                                string name = Convert.ToString(testCaseResult["Name"]);
//                                string id = Convert.ToString(testCaseResult["ObjectID"]);
//                                string desc = Convert.ToString(testCaseResult["Description"]);
//                                string owner = Convert.ToString(testCaseResult["Owner"]["_refObjectName"]);
//                                DateTime createDate = Convert.ToDateTime(testCaseResult["CreationDate"]);

//                                RallyTestCase tcase = new RallyTestCase(name, id, desc, owner, createDate);
//                                string scriptdesc = Convert.ToString(testCaseResult["ValidationInput"]);
//                                RallyTestStep script = new RallyTestStep("Test Step 1", string.Empty, scriptdesc, owner);
//                                tcase.TestSteps.Add(script);

//                                plan.TestCases.Add(tcase);
//                            }
//                        }
//                    }
//                } 
//            }

//            Mouse.OverrideCursor = null;
//            return plan;
//        }
//    }
//}
