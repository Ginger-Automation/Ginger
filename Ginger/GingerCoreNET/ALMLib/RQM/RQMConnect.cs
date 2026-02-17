#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
//Copyright Ã‚Â© 2014-2025 European Support Limited

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

using ALM_CommonStd.Abstractions;
using ALM_CommonStd.DataContracts;
using Amdocs.Ginger.Common;
using Newtonsoft.Json;
using RQM_RepositoryStd;
using RQM_RepositoryStd.Data_Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using System.Xml;
using System.Xml.Serialization;

namespace GingerCore.ALM.RQM
{
    public class RQMConnect
    {
        const string BTS_ACTIVITY_ID = "bts_activity_data";
        const string BTS_ACTIVITY_STEPS_ID = "bts_step_data";

        bool connectedToServer;
        bool connectedToProject;
        IProjectDefinitions connectedProjectDefenition;
        List<IProjectDefinitions> rqmProjectsDataList;
        private RQMConnect()
        {
        }

        public void CreateRQMRepository()
        {
            RQMRep = new RqmRepository(RQMCore.ConfigPackageFolderPath);
        }

        #region singlton
        private static readonly RQMConnect _instance = new RQMConnect();
        public static RQMConnect Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion singlton

        private RqmRepository mRQMRep;
        public RqmRepository RQMRep
        {
            get
            {
                if (mRQMRep == null)
                {
                    return new RqmRepository(RQMCore.ConfigPackageFolderPath);
                }

                return mRQMRep;
            }
            set { mRQMRep = value; }
        }

        public bool ConnectRQMServer()
        {
            if (RQMConnectionTest())
            {
                connectedToServer = true;
                return connectedToServer;
            }
            return false;
        }

        public bool IsServerConnected()
        {
            return connectedToServer;
        }

        private bool RQMConnectionTest()
        {
            bool isUserAuthen = false;
            try
            {
                LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
                isUserAuthen = RQMRep.IsUserAuthenticated(loginData);

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"RQM connection Failed {ex.Message}", ex);
            }
            return isUserAuthen;
        }

        public void DisconnectRQMServer()
        {
            connectedToProject = false;
            connectedToServer = false;
        }

        public bool DisconnectALMProjectStayLoggedIn()
        {
            connectedToProject = false;
            return connectedToProject;
        }

        public List<string> GetRQMDomains()
        {
            List<string> RQMDomains = ["RQM Domain"];
            return RQMDomains;

        }

        public Dictionary<string, string> GetRQMDomainProjects()
        {
            List<string> RQMProjects = [];
            try
            {
                LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
                if (rqmProjectsDataList == null || rqmProjectsDataList.Count == 0)
                {
                    IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);


                    rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                }

                if (rqmProjectsDataList.Count > 0)
                {
                    foreach (var proj in rqmProjectsDataList)
                    {
                        RQMProjects.Add(proj.ProjectName);
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Project not found");
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Project not found{ex.Message}", ex);
            }
            return RQMProjects.ToDictionary(x => x, x => x);
        }

        public bool SetRQMProjectFullDetails()
        {
            GetRQMDomainProjects();

            IProjectDefinitions selectedProj = rqmProjectsDataList.FirstOrDefault(x => x.ProjectName.Equals(ALMCore.DefaultAlmConfig.ALMProjectName));
            if (selectedProj != null)
            {
                //Save selected project details
                connectedProjectDefenition = selectedProj;
                ALMCore.DefaultAlmConfig.ALMProjectName = selectedProj.ProjectName;
                ALMCore.DefaultAlmConfig.ALMProjectGUID = selectedProj.Guid;
                RQMCore.ALMProjectGuid = selectedProj.Guid;
                RQMCore.ALMProjectGroupName = selectedProj.Prefix;
                return true;
            }

            return false;
        }

        RQMProjectListConfiguration RQMProjectListConfig;
        private readonly object fileLock = new object();
        XmlReader reader;
        private void GetRQMProjectListConfiguration()
        {
            try
            {
                lock (fileLock)
                {
                    if (RQMProjectListConfig != null)
                    { return; }
                    string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
                    if (File.Exists(importConfigTemplate))

                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(RQMProjectListConfiguration));

                        FileStream fs = new FileStream(importConfigTemplate, FileMode.Open, FileAccess.Read);
                        reader = XmlReader.Create(fs);
                        RQMProjectListConfig = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                        fs.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to import RQM test plans, RQM_ImportConfigs_Template.xml wasn't found {ex.Message} ", ex);
            }
        }

        public ObservableList<RQMTestPlan> GetRQMTestPlansByProject(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, string solutionFolder)
        {
            ObservableList<RQMTestPlan> RQMTestPlanList = [];
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, " In GetRQMTestPlansByProject");

                GetRQMProjectListConfiguration();

                if (RQMProjectListConfig != null)
                {
                    RQMProject currentRQMProjectMapping;
                    if (RQMProjectListConfig.RQMProjects.Count > 0)
                    {
                        currentRQMProjectMapping = RQMProjectListConfig.RQMProjects.FirstOrDefault(x => x.Name == RQMProject || x.Name == "DefaultProjectName");
                        if (currentRQMProjectMapping != null)
                        {
                            //
                            //
                            LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                            if (rqmProjectsDataList == null || rqmProjectsDataList.Count == 0)
                            {
                                IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                                rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                            }
                            IProjectDefinitions currentProj = rqmProjectsDataList.FirstOrDefault(x => x.ProjectName == RQMProject);
                            List<RqmResponseData> responseDataList = RQMRep.GetAllTestPlansByProject(loginData, currentProj.Guid, currentProj.Prefix);
                            foreach (RqmResponseData responseData in responseDataList)
                            {
                                try //skip result incase of error, defect #5164
                                {
                                    XmlDocument doc = new XmlDocument();
                                    doc.LoadXml(responseData.responseText.ToString());
                                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(reader.NameTable);
                                    currentRQMProjectMapping.RQMTestPlansListMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgr.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                    XmlNode responseDataNode = doc.DocumentElement;
                                    XmlNodeList testPlans = responseDataNode.SelectNodes(currentRQMProjectMapping.RQMTestPlansListMapping.XMLPathToTestPlansList, nsmgr);
                                    foreach (XmlNode testPlan in testPlans)
                                    {
                                        RQMTestPlanList.Add(new RQMTestPlan(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.Name, nsmgr).InnerText.ToString(),
                                                                            testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.URLPath, nsmgr).Attributes[0].InnerText.ToString(),
                                                                            testPlan.Attributes[0].InnerText.ToString(),
                                                                            testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.RQMID, nsmgr).InnerText.ToString(),
                                                                            testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreatedBy, nsmgr).Attributes[0].InnerText.Split('/').Last().ToString(),
                                                                            DateTime.Parse(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreationDate, nsmgr).InnerText.ToString()).ToLocalTime(), currentProj.Prefix));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to retrieve the following TestPlan page: {responseData.RequestUri} ");
                                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to import RQM test plans, RQM_ImportConfigs_Template.xml wasn't found");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Project Test Plan list not found {ex.Message} ", ex);
            }
            return RQMTestPlanList;
        }

        public RQMTestPlan GetRQMTestPlanByIdByProject(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, string RQMTestPlanId)
        {
            RQMTestPlan testPlanRes = null;
            try
            {
                GetRQMProjectListConfiguration();

                if (RQMProjectListConfig != null)
                {
                    RQMProject currentRQMProjectMapping;
                    if (RQMProjectListConfig.RQMProjects.Count > 0)
                    {
                        currentRQMProjectMapping = RQMProjectListConfig.RQMProjects.FirstOrDefault(x => x.Name == RQMProject || x.Name == "DefaultProjectName");
                        if (currentRQMProjectMapping != null)
                        {
                            //
                            //
                            LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                            if (rqmProjectsDataList == null || rqmProjectsDataList.Count == 0)
                            {
                                IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                                rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                            }

                            IProjectDefinitions currentProj = rqmProjectsDataList.FirstOrDefault(x => x.ProjectName == RQMProject);
                            RqmResponseData responseData = RQMRep.GetTestPlanByIdByProject(loginData, currentProj.Prefix, currentProj.Guid, RQMTestPlanId);
                            try //skip result incase of error, defect #5164
                            {
                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(responseData.responseText);
                                XmlNamespaceManager nsmgr = new XmlNamespaceManager(reader.NameTable);
                                currentRQMProjectMapping.RQMTestPlansListMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgr.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                XmlNode responseDataNode = doc.DocumentElement;
                                XmlNodeList testPlans = responseDataNode.SelectNodes(currentRQMProjectMapping.RQMTestPlansListMapping.XMLPathToTestPlansList, nsmgr);
                                foreach (XmlNode testPlan in testPlans)
                                {
                                    testPlanRes = new RQMTestPlan(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.Name, nsmgr).InnerText,
                                                                        testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.URLPath, nsmgr).Attributes[0].InnerText,
                                                                        testPlan.Attributes[0].InnerText,
                                                                        testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.RQMID, nsmgr).InnerText,
                                                                        testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreatedBy, nsmgr).Attributes[0].InnerText.Split('/').Last(),
                                                                        DateTime.Parse(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreationDate, nsmgr).InnerText).ToLocalTime(), currentProj.Prefix);
                                    XmlNodeList testSuitesURInodes = testPlan.SelectNodes(currentRQMProjectMapping.RQMTestPlansListMapping.ContainedTestSuitesList, nsmgr);
                                    foreach (XmlNode testSuitesURInode in testSuitesURInodes)
                                    {
                                        RQMTestSuite rQMTestSuite = new RQMTestSuite(testSuitesURInode.Attributes[0].InnerText);
                                        try
                                        {
                                            // TestSuite data from RQM
                                            RqmResponseData responseDataTestSuite = RQMRep.GetRqmResponse(loginData, new Uri(testSuitesURInode.Attributes[0].InnerText));
                                            XmlDocument docTestSuite = new XmlDocument();
                                            docTestSuite.LoadXml(responseDataTestSuite.responseText);
                                            XmlNamespaceManager nsmgrTestSuite = new XmlNamespaceManager(reader.NameTable);
                                            currentRQMProjectMapping.RQMTestSuiteAsItemMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTestSuite.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                            XmlNode responseDataNodeTestSuite = docTestSuite.DocumentElement;
                                            rQMTestSuite.RQMID = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteAsItemMapping.RQMID, nsmgrTestSuite).InnerText;
                                            rQMTestSuite.Name = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteAsItemMapping.Name, nsmgrTestSuite).InnerText;
                                            // TestSuite data from RQM
                                            RqmResponseData responseDataTestSuiteExecutionRecords = RQMRep.GetTestSuiteExecutionRecordsByTestSuite(loginData, currentProj.Prefix, currentProj.Guid, testSuitesURInode.Attributes[0].InnerText);
                                            XmlDocument docTestSuiteExecutionRecords = new XmlDocument();
                                            docTestSuiteExecutionRecords.LoadXml(responseDataTestSuiteExecutionRecords.responseText);
                                            XmlNamespaceManager nsmgrTestSuiteExecutionRecords = new XmlNamespaceManager(reader.NameTable);
                                            currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTestSuiteExecutionRecords.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                            XmlNode responseDataNodeExecutionRecords = docTestSuiteExecutionRecords.DocumentElement;
                                            try
                                            {
                                                rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.URLPathVersioned = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.CurrentTestSuiteResult, nsmgrTestSuiteExecutionRecords).Attributes[0].InnerText;
                                            }
                                            catch { }
                                            try
                                            {
                                                rQMTestSuite.TestSuiteExecutionRecord.URLPathVersioned = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.XMLTestSuiteExecutionRecord, nsmgrTestSuiteExecutionRecords).Attributes[0].InnerText;
                                                rQMTestSuite.TestSuiteExecutionRecord.RQMID = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.RQMID, nsmgrTestSuiteExecutionRecords).InnerText;
                                            }
                                            catch { }
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"RQMConnect  testSuitesURInodes loop : {JsonConvert.SerializeObject(ex)} ");
                                        }
                                        testPlanRes.TestSuites.Add(rQMTestSuite);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"RQM Connect GetTestPlanByIdByProject :{JsonConvert.SerializeObject(ex)}");
                                Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to retrieve TestPlan id:{RQMTestPlanId}");
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RQMConnect GetRQMTestPlanByIdByProject :{JsonConvert.SerializeObject(ex)}");
                Reporter.ToLog(eLogLevel.ERROR, $"Project Test Plan by Id not found {ex.Message}", ex);
            }

            return testPlanRes;
        }
        public RQMTestPlan GetRQMTestPlanFullData(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, RQMTestPlan testPlan)
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("in GetRQMTestPlanFullData :");
                
                GetRQMProjectListConfiguration();
                if (RQMProjectListConfig != null)
                {
                    RQMProject currentRQMProjectMapping;
                    if (RQMProjectListConfig.RQMProjects.Count > 0)
                    {
                        currentRQMProjectMapping = RQMProjectListConfig.RQMProjects.FirstOrDefault(x => x.Name == RQMProject || x.Name == "DefaultProjectName");
                        if (currentRQMProjectMapping != null)
                        {
                            //
                            // building a list of TestCases
                            LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                            if (rqmProjectsDataList == null || rqmProjectsDataList.Count == 0)
                            {
                                IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                                rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                            }
                            IProjectDefinitions currentProj = rqmProjectsDataList.FirstOrDefault(x => x.ProjectName == RQMProject);

                            RqmResponseData responseData = RQMRep.GetRqmResponse(loginData, new Uri(testPlan.URLPath));
                            XmlDocument docTP = new XmlDocument();
                            docTP.LoadXml(responseData.responseText.ToString());
                            XmlNamespaceManager nsmgrTP = new XmlNamespaceManager(reader.NameTable);
                            currentRQMProjectMapping.RQMTestPlanMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTP.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                            XmlNode responseDataNode = docTP.DocumentElement;
                            testPlan.Description = responseDataNode.SelectSingleNode(currentRQMProjectMapping.RQMTestPlanMapping.Description, nsmgrTP).InnerText.ToString();
                            // Building execution Results Dictionary - start
                            // - execution records that are seating on Test Plan
                            testPlan.RQMExecutionRecords = GetExecutionRecordsByTestPlan(loginData, reader, currentRQMProjectMapping, currentProj.Prefix, currentProj.Guid, testPlan.URLPathVersioned);
                            // building test cases lists of TestSuits (not on TestPlan)
                            // test suites should be created already by function GetRQMTestPlanByIdByProject()
                            // this is only enhancement that will add to them test cases and some values like description and so on
                            XmlNodeList testSuites = responseDataNode.SelectNodes(currentRQMProjectMapping.RQMTestPlanMapping.PathXMLToTestSuitesLists, nsmgrTP);
                            foreach (XmlNode testSuite in testSuites)
                            {
                                RqmResponseData responseDataTestSuite = RQMRep.GetRqmResponse(loginData, new Uri(testSuite.Attributes[0].InnerText.ToString()));
                                XmlDocument docTestSuite = new XmlDocument();
                                docTestSuite.LoadXml(responseDataTestSuite.responseText.ToString());
                                XmlNamespaceManager nsmgrTS = new XmlNamespaceManager(reader.NameTable);
                                currentRQMProjectMapping.RQMTestSuiteMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTS.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                XmlNode responseDataNodeTestSuite = docTestSuite.DocumentElement;
                                ObservableList<RQMTestCase> currentSuiteTestCases = BuildRQMTestCaseList(responseDataNodeTestSuite.SelectNodes(currentRQMProjectMapping.RQMTestSuiteMapping.PathXML, nsmgrTS), nsmgrTS, loginData, currentRQMProjectMapping, reader,
                                                                                                         responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.Name, nsmgrTS).InnerText.ToString(),
                                                                                                         responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.RQMID, nsmgrTS).InnerText.ToString());
                                RQMTestSuite currentTestSuite = testPlan.TestSuites.FirstOrDefault(z => z.RQMID == responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.RQMID, nsmgrTS).InnerText.ToString());
                                if (currentTestSuite != null)
                                {
                                    currentTestSuite.Name = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.Name, nsmgrTS).InnerText.ToString();
                                    currentTestSuite.CreatedBy = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.RQMID, nsmgrTS).InnerText.ToString();
                                    currentTestSuite.Description = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.Description, nsmgrTS).InnerText.ToString();
                                    currentTestSuite.CreationDate = DateTime.Parse(responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.CreationDate, nsmgrTS).InnerText.ToString()).ToLocalTime();
                                    currentTestSuite.TestCases = currentSuiteTestCases;
                                }
                                // adding current's test suite list of tests cases to overall (test plan's) list of test cases - to be presented at ginger together
                                currentSuiteTestCases.Where(y => !testPlan.TestCases.Select(x => x.RQMID).ToList().Contains(y.RQMID)).ToList().ForEach(z => testPlan.TestCases.Add(z));
                            }
                            //
                            // building test cases list directly on the TestPlan  
                            ObservableList<RQMTestCase> testPlansTestCases = BuildRQMTestCaseList(responseDataNode.SelectNodes(currentRQMProjectMapping.RQMTestPlanMapping.PathXML, nsmgrTP),
                                                                                                  nsmgrTP, loginData, currentRQMProjectMapping, reader, string.Empty, string.Empty);
                            // testPlan.TestCases = testPlansTestCases;
                            // temp? to review!
                            testPlansTestCases.Where(y => !testPlan.TestCases.Select(x => x.RQMID).ToList().Contains(y.RQMID)).ToList().ForEach(z => testPlan.TestCases.Add(z));
                        }
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Error while trying to import selected RQM test plan, RQM_ImportConfigs_Template.xml wasn't found");
                }
             
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Test Plan full data not found {ex.Message}", ex);
            }
            return testPlan;
        }
        public ObservableList<RQMTestCase> BuildRQMTestCaseList(XmlNodeList testCases, XmlNamespaceManager nsmgr, LoginDTO loginData, RQMProject currentRQMProjectMapping, XmlReader reader, string TestSuiteTitle, string TestSuiteId)
        {
            ObservableList<RQMTestCase> RQMTestCaseList = [];
            try
            {


                foreach (XmlNode testCase in testCases)
                {
                    RqmResponseData responseDataTC = RQMRep.GetRqmResponse(loginData, new Uri(testCase.Attributes[0].InnerText.ToString()));
                    System.Diagnostics.Trace.WriteLine($"in BuildRQMTestCaseList responseDataTC :{Newtonsoft.Json.JsonConvert.SerializeObject(responseDataTC)}");
                    XmlDocument doc = new XmlDocument();
                    XmlDocument docTC = new XmlDocument();
                    doc.LoadXml(responseDataTC.responseText.ToString());
                    nsmgr = new XmlNamespaceManager(reader.NameTable);
                    currentRQMProjectMapping.RQMTestCaseMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgr.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                    XmlNode responseDataNodeTC = doc.DocumentElement;

                    RQMTestCase rQMTestCase = new RQMTestCase(responseDataNodeTC.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.Name, nsmgr).InnerText.ToString(),
                                                              responseDataNodeTC.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.RQMID, nsmgr).InnerText.ToString(),
                                                              responseDataNodeTC.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.Description, nsmgr).InnerText.ToString(),
                                                              responseDataNodeTC.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.CreatedBy, nsmgr).Attributes[0].InnerText.Split('/').Last().ToString(),
                                                              DateTime.Parse(responseDataNodeTC.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.CreationDate, nsmgr).InnerText.ToString()).ToLocalTime(),
                                                              TestSuiteTitle,
                                                              TestSuiteId);

                    try
                    {
                        XmlNodeList variables = responseDataNodeTC.SelectNodes(currentRQMProjectMapping.RQMTestCaseMapping.XMLPathToVariablesList, nsmgr);
                        foreach (XmlNode variable in variables)
                        {
                            // looking for specific Variable from BTS
                            if (variable.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.VariableName, nsmgr).InnerText.ToString() == BTS_ACTIVITY_ID)
                            {
                                rQMTestCase.BTSID = variable.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.VariableValue, nsmgr).InnerText.ToString();
                                continue;
                            }

                            RQMTestParameter rQMTestParameter;
                            rQMTestParameter = new RQMTestParameter(variable.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.VariableName, nsmgr).InnerText.ToString(),
                                                                        variable.SelectSingleNode(currentRQMProjectMapping.RQMTestCaseMapping.VariableValue, nsmgr).InnerText.ToString());
                            rQMTestCase.Parameters.Add(rQMTestParameter);
                        }
                    }
                    catch { }

                    XmlNodeList testScripts = responseDataNodeTC.SelectNodes(currentRQMProjectMapping.RQMTestCaseMapping.PathXML, nsmgr);
                    foreach (XmlNode testScript in testScripts)
                    {
                        RqmResponseData responseDataTestScript = RQMRep.GetRqmResponse(loginData, new Uri(testScript.Attributes[0].InnerText.ToString()));
                        System.Diagnostics.Trace.WriteLine($"in BuildRQMTestCaseList responseDataTestScript :{Newtonsoft.Json.JsonConvert.SerializeObject(responseDataTestScript)}");
                        doc.LoadXml(responseDataTestScript.responseText.ToString());
                        nsmgr = new XmlNamespaceManager(reader.NameTable);
                        currentRQMProjectMapping.RQMTestScriptMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgr.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                        XmlNode responseDataNodeTestScript = doc.DocumentElement;

                        RQMTestScript newRQMTestScript = new RQMTestScript(responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.Name, nsmgr).InnerText.ToString(),
                                                                            responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.RQMID, nsmgr).InnerText.ToString(),
                                                                            responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.Description, nsmgr).InnerText.ToString(),
                                                                            responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.CreatedBy, nsmgr).Attributes[0].InnerText.Split('/').Last().ToString(),
                                                                            DateTime.Parse(responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.CreationDate, nsmgr).InnerText.ToString()).ToLocalTime());

                        try
                        {
                            XmlNodeList variables = responseDataNodeTestScript.SelectNodes(currentRQMProjectMapping.RQMTestScriptMapping.XMLPathToVariablesList, nsmgr);
                            foreach (XmlNode variable in variables)
                            {
                                // looking for specific Variable from BTS
                                if (variable.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.VariableName, nsmgr).InnerText.ToString() == BTS_ACTIVITY_STEPS_ID)
                                {
                                    newRQMTestScript.BTSStepsIDs = variable.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.VariableValue, nsmgr).InnerText.ToString();
                                    continue;
                                }

                                RQMTestParameter rQMTestParameter;
                                rQMTestParameter = new RQMTestParameter(variable.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.VariableName, nsmgr).InnerText.ToString(),
                                                                            variable.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.VariableValue, nsmgr).InnerText.ToString());
                                newRQMTestScript.Parameters.Add(rQMTestParameter);
                            }
                        }
                        catch { }

                        XmlNodeList steps = responseDataNodeTestScript.SelectNodes(currentRQMProjectMapping.RQMTestScriptMapping.PathXML, nsmgr);
                        foreach (XmlNode step in steps)
                        {
                            RQMStep newStep;
                            try
                            {
                                newStep = new RQMStep(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.Name, nsmgr).InnerText.ToString(),
                                                      responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.RQMID, nsmgr).InnerText.ToString() + "_" + step.Attributes[0].InnerText.ToString(),
                                                      ImportFromRQM.StripHTML(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.Description, nsmgr).InnerText.ToString()),
                                                      ImportFromRQM.StripHTML(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.ExpectedResult, nsmgr).InnerText.ToString())
                                                      );
                                newRQMTestScript.Steps.Add(newStep);
                            }
                            catch
                            {
                                try
                                {
                                    newStep = new RQMStep(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.Name, nsmgr).InnerText.ToString(),
                                                      responseDataNodeTestScript.SelectSingleNode(currentRQMProjectMapping.RQMTestScriptMapping.RQMID, nsmgr).InnerText.ToString() + "_" + step.Attributes[0].InnerText.ToString(),
                                                      ImportFromRQM.StripHTML(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.Description, nsmgr).InnerText.ToString()),
                                                      string.Empty);
                                    newRQMTestScript.Steps.Add(newStep);
                                }
                                catch { }
                            }
                        }

                        rQMTestCase.TestScripts.Add(newRQMTestScript);

                        if (rQMTestCase.TestScripts.Count > 0)
                        {
                            rQMTestCase.SelectedTestScriptName = rQMTestCase.TestScripts[0].Name.ToString();
                        }
                    }

                    RQMTestCaseList.Add(rQMTestCase);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Test Case list not found {ex.Message}", ex);
            }
            return RQMTestCaseList;
        }

        public ObservableList<RQMExecutionRecord> GetExecutionRecordsByTestPlan(LoginDTO loginData, XmlReader reader, RQMProject currentRQMProjectMapping, string currentProjPrefix, string currentProjGuid, string testPlanURLPathVersioned)
        {
            ObservableList<RQMExecutionRecord> RQMExecutionRecords = [];
            try
            {
                RqmResponseData responseDataExecutionRecords = RQMRep.GetExecutionByTestPlan(loginData, currentProjPrefix, currentProjGuid, testPlanURLPathVersioned);
                XmlDocument docExecutionRecords = new XmlDocument();
                docExecutionRecords.LoadXml(responseDataExecutionRecords.responseText);
                XmlNamespaceManager nsmgrExecutionRecords = new XmlNamespaceManager(reader.NameTable);
                currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrExecutionRecords.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                XmlNode responseDataNodeExecutionRecords = docExecutionRecords.DocumentElement;
                XmlNodeList executionRecords = responseDataNodeExecutionRecords.SelectNodes(currentRQMProjectMapping.RQMExecutionRecordsMapping.PathXML, nsmgrExecutionRecords);
                foreach (XmlNode executionRecord in executionRecords)
                {
                    try
                    {
                        string curentExecutionRecordUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RelatedTestCaseUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                        RqmResponseData responseDataVersionedTC = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordUri));
                        XmlDocument docVersionedTC = new XmlDocument();
                        docVersionedTC.LoadXml(responseDataVersionedTC.responseText);
                        XmlNode responseDataNodeVersionedTC = docVersionedTC.DocumentElement;

                        RqmResponseData responseDataVersionedTS;
                        try
                        {
                            string curentExecutionRecordScriptUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.ExecutesTestScriptUri, nsmgrExecutionRecords) != null ? executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.ExecutesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString() : string.Empty;
                            if (!string.IsNullOrEmpty(curentExecutionRecordScriptUri))
                            {
                                responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                            }
                            else
                            {
                                curentExecutionRecordScriptUri = responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.UsesTestScriptUri, nsmgrExecutionRecords) != null ? responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.UsesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString() : string.Empty;
                                if (!string.IsNullOrEmpty(curentExecutionRecordScriptUri))
                                {
                                    responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                                }
                                else
                                {
                                    responseDataVersionedTS = new RqmResponseData();
                                    Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Script by test plan not found");
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            responseDataVersionedTS = new RqmResponseData();
                            Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Script by test plan not found {ex.Message}", ex);
                        }
                        XmlDocument docVersionedTS = new XmlDocument();
                        docVersionedTS.LoadXml(!string.IsNullOrEmpty(responseDataVersionedTS.responseText) ? responseDataVersionedTS.responseText : string.Empty);
                        XmlNode responseDataNodeVersionedTS = docVersionedTS.DocumentElement;

                        if (RQMExecutionRecords.Where(x => x.RQMID == executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMID, nsmgrExecutionRecords).InnerText.ToString()).ToList().Count == 0)
                        {
                            RQMExecutionRecord rQMExecutionRecord = new RQMExecutionRecord(executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMID, nsmgrExecutionRecords).InnerText.ToString(),
                                                                                           responseDataNodeVersionedTS.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.VersioinedTestScriptXmlPathToID, nsmgrExecutionRecords).InnerText.ToString(),
                                                                                           responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.VersioinedTestCaseXmlPathToID, nsmgrExecutionRecords).InnerText.ToString());

                            RQMExecutionRecords.Add(rQMExecutionRecord);
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Case by test plan not found {ex.Message}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Execution Records by test plan not found {ex.Message}", ex);
            }
            return RQMExecutionRecords;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="loginData"></param>
        /// <param name="reader"></param>
        /// <param name="currentRQMProjectMapping"></param>
        /// <param name="currentProjPrefix"></param>
        /// <param name="currentProjGuid"></param>
        /// <param name="currentTestSuiteResultUri"></param>
        /// <returns></returns>


        public void GetExecutionRecordsByTestCase(LoginDTO loginData, XmlReader reader, RQMProject currentRQMProjectMapping, string currentProjPrefix, string currentProjGuid, string testPlanUrlVersioned, string testCaseURLPathVersioned, ref string erExportID)
        {
            try
            {
                RqmResponseData responseDataExecutionRecords = RQMRep.GetExecutionByTestPlanAndTestCase(loginData, currentProjPrefix, currentProjGuid, testPlanUrlVersioned, testCaseURLPathVersioned);

                XmlDocument docExecutionRecords = new XmlDocument();
                if (responseDataExecutionRecords.ErrorCode == 400)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - Test Case not found {responseDataExecutionRecords.ErrorCode}, {responseDataExecutionRecords.ErrorDesc}");
                }
                else if (responseDataExecutionRecords.ErrorCode != 0)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {responseDataExecutionRecords.ErrorCode}, {responseDataExecutionRecords.ErrorDesc}");
                }
                else
                {
                    docExecutionRecords.LoadXml(!string.IsNullOrEmpty(responseDataExecutionRecords.responseText) ? responseDataExecutionRecords.responseText : string.Empty);

                    XmlNamespaceManager nsmgrExecutionRecords = new XmlNamespaceManager(reader.NameTable);

                    currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrExecutionRecords.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));

                    XmlNode responseDataNodeExecutionRecords = docExecutionRecords.DocumentElement;

                    XmlNodeList executionRecords = responseDataNodeExecutionRecords.SelectNodes(currentRQMProjectMapping.RQMExecutionRecordsMapping.PathXML, nsmgrExecutionRecords);

                    foreach (XmlNode executionRecord in executionRecords)
                    {
                        //string curentExecutionRecordUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RelatedTestCaseUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                        //RqmResponseData responseDataVersionedTC = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordUri));
                        //XmlDocument docVersionedTC = new XmlDocument();
                        //docVersionedTC.LoadXml(responseDataVersionedTC.responseText);
                        //XmlNode responseDataNodeVersionedTC = docVersionedTC.DocumentElement;

                        //RqmResponseData responseDataVersionedTS;

                        //string relatedTestScriptId = "0";
                        //try
                        //{
                        //    string curentExecutionRecordScriptUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.ExecutesTestScriptUri, nsmgrExecutionRecords) != null ? executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.ExecutesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString() : string.Empty;
                        //    if (!string.IsNullOrEmpty(curentExecutionRecordScriptUri))
                        //    {
                        //        responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                        //    }
                        //    else
                        //    {
                        //        curentExecutionRecordScriptUri = responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.UsesTestScriptUri, nsmgrExecutionRecords) != null ? responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.UsesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString() : string.Empty;
                        //        if (!string.IsNullOrEmpty(curentExecutionRecordScriptUri))
                        //        {
                        //            responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                        //        }
                        //        else
                        //        {
                        //            responseDataVersionedTS = new RqmResponseData();
                        //            Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Script by test Case not found");
                        //        }

                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    responseDataVersionedTS = new RqmResponseData();
                        //    Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Script by test Case not found { ex.Message}");
                        //}
                        //try
                        //{
                        //    if (!string.IsNullOrEmpty(responseDataVersionedTS.responseText))
                        //    {
                        //        XmlDocument docVersionedTS = new XmlDocument();
                        //        docVersionedTS.LoadXml(responseDataVersionedTS.responseText);
                        //        XmlNode responseDataNodeVersionedTS = docVersionedTS.DocumentElement;
                        //        relatedTestScriptId = responseDataNodeVersionedTS.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.VersioinedTestScriptXmlPathToID, nsmgrExecutionRecords).InnerText;
                        //    }
                        //}
                        //catch { }

                        erExportID = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMID, nsmgrExecutionRecords).InnerText;
                        if (!string.IsNullOrEmpty(erExportID) && erExportID != "0")
                        {
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                erExportID = string.Empty;
                Reporter.ToLog(eLogLevel.ERROR, $"Execution Records by test Case not found ProjectId {currentProjGuid} TC url {testCaseURLPathVersioned}", ex);
            }
        }


        public ObservableList<RQMExecutionRecord> GetTestSuiteCurrentResult(LoginDTO loginData, XmlReader reader, RQMProject currentRQMProjectMapping, string currentProjPrefix, string currentProjGuid, string currentTestSuiteResultUri)
        {
            if ((currentTestSuiteResultUri == null) || (currentTestSuiteResultUri == string.Empty))
            {
                return null;
            }

            ObservableList<RQMExecutionRecord> RQMExecutionRecords = [];
            try
            {


                RqmResponseData responseDataCurrentTestSuiteResult = RQMRep.GetRqmResponse(loginData, new Uri(currentTestSuiteResultUri));
                XmlDocument docTestSuiteResult = new XmlDocument();
                docTestSuiteResult.LoadXml(responseDataCurrentTestSuiteResult.responseText);
                XmlNamespaceManager nsmgrTestSuiteResult = new XmlNamespaceManager(reader.NameTable);
                currentRQMProjectMapping.RQMTestSuiteResultsMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTestSuiteResult.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                XmlNode responseTestSuiteResult = docTestSuiteResult.DocumentElement;
                XmlNodeList executionRecordsURIs = responseTestSuiteResult.SelectNodes(currentRQMProjectMapping.RQMTestSuiteResultsMapping.XMLPathToResultsExecutionRecordsList, nsmgrTestSuiteResult);

                XmlNamespaceManager nsmgrExecutionRecords = new XmlNamespaceManager(reader.NameTable);
                currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrExecutionRecords.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));

                foreach (XmlNode executionRecordURI in executionRecordsURIs)
                {
                    try
                    {
                        RqmResponseData responseDataExecutionRecord = RQMRep.GetRqmResponse(loginData, new Uri(executionRecordURI.Attributes[0].InnerText.ToString()));
                        XmlDocument docExecutionRecords = new XmlDocument();
                        docExecutionRecords.LoadXml(responseDataExecutionRecord.responseText);
                        XmlNode responseDataNodeExecutionRecord = docExecutionRecords.DocumentElement;
                        XmlNode executionRecord = responseDataNodeExecutionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.XMLPathOfSingleSelectionCase, nsmgrExecutionRecords);

                        string curentExecutionRecordVersionedTCUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RelatedTestCaseUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                        RqmResponseData responseDataVersionedTC = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordVersionedTCUri));
                        XmlDocument docVersionedTC = new XmlDocument();
                        docVersionedTC.LoadXml(responseDataVersionedTC.responseText);
                        XmlNode responseDataNodeVersionedTC = docVersionedTC.DocumentElement;

                        RqmResponseData responseDataVersionedTS;
                        try
                        {
                            string curentExecutionRecordScriptUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.ExecutesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                            responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                        }
                        catch
                        {
                            string curentExecutionRecordScriptUri = responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.UsesTestScriptUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                            responseDataVersionedTS = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordScriptUri));
                        }
                        XmlDocument docVersionedTS = new XmlDocument();
                        docVersionedTS.LoadXml(responseDataVersionedTS.responseText);
                        XmlNode responseDataNodeVersionedTS = docVersionedTS.DocumentElement;

                        if (RQMExecutionRecords.Where(x => x.RQMID == executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMID, nsmgrExecutionRecords).InnerText.ToString()).ToList().Count == 0)
                        {
                            RQMExecutionRecord rQMExecutionRecord = new RQMExecutionRecord(executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RQMID, nsmgrExecutionRecords).InnerText.ToString(),
                                                                                           responseDataNodeVersionedTS.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.VersioinedTestScriptXmlPathToID, nsmgrExecutionRecords).InnerText.ToString(),
                                                                                           responseDataNodeVersionedTC.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.VersioinedTestCaseXmlPathToID, nsmgrExecutionRecords).InnerText.ToString());

                            RQMExecutionRecords.Add(rQMExecutionRecord);
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Test Suite result not found {ex.Message}", ex);
            }
            return RQMExecutionRecords;
        }

        public string GetTestCaseVersionURLByIdByProject(LoginDTO loginData, string PreFix, string txExportID)
        {
            try
            {
                RqmResponseData responseData = RQMRep.GetTestCaseByIdByProject(loginData, PreFix, RQMCore.ALMProjectGuid, txExportID);
                try //skip result incase of error, defect #5164
                {
                    XmlDocument doc = new XmlDocument();
                    if (responseData.ErrorCode == 400)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - Test Case not found {responseData.ErrorCode}, {responseData.ErrorDesc}");
                    }
                    else if (responseData.ErrorCode != 0)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {responseData.ErrorCode}, {responseData.ErrorDesc}");
                    }
                    else
                    {
                        doc.LoadXml(responseData.responseText);
                        XmlNodeList TesCaseList = doc.GetElementsByTagName("oslc_qm:testCase");
                        foreach (XmlNode TesCase in TesCaseList)
                        {
                            XmlNodeList innerNodes = TesCase.ChildNodes;
                            foreach (XmlNode innerNode in innerNodes)
                            {
                                if (innerNode.Name.Equals("oslc_qm:TestCase", StringComparison.OrdinalIgnoreCase))
                                {
                                    return innerNode.Attributes.Count > 0 ? innerNode.Attributes["rdf:about"].Value : string.Empty;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RQM Connect GetTestCaseByIdByProject :{JsonConvert.SerializeObject(ex)}");
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RQMConnect GetRQMTestPlanByIdByProject :{JsonConvert.SerializeObject(ex)}");
                Reporter.ToLog(eLogLevel.ERROR, $"Project Test Case by Id not found {ex.Message}", ex);
            }
            return string.Empty;
        }
    }
}
