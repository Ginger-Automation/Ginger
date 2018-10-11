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
using ALM_Common.DataContracts;
using RQM_Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ALM_Common.Abstractions;
using RQM_Repository.Data_Contracts;
using System.IO;
using System.Reflection;
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
                    return new RqmRepository(RQMCore.ConfigPackageFolderPath);
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
            bool isUserAuthen;

            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            isUserAuthen = RQMRep.IsUserAuthenticated(loginData);

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
            List<string> RQMDomains = new List<string>();
            RQMDomains.Add("RQM Domain");
            return RQMDomains;

        }

        public List<string> GetRQMDomainProjects()
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
            rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;

            List<string> RQMProjects = new List<string>();
            foreach (var proj in rqmProjectsDataList)
            {
                RQMProjects.Add(proj.ProjectName);
            }

            return RQMProjects;
        }

        public bool SetRQMProjectFullDetails()
        {
            GetRQMDomainProjects();

            IProjectDefinitions selectedProj = rqmProjectsDataList.Where(x => x.ProjectName.Equals(ALMCore.AlmConfig.ALMProjectName) == true).FirstOrDefault();
            if (selectedProj != null)
            {
                //Save selected project details
                connectedProjectDefenition = selectedProj;
                ALMCore.AlmConfig.ALMProjectName = selectedProj.ProjectName;
                RQMCore.ALMProjectGuid = selectedProj.Guid;
                RQMCore.ALMProjectGroupName = selectedProj.Prefix;
                return true;
            }

            return false;
        }

        public ObservableList<RQMTestPlan> GetRQMTestPlansByProject(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, string solutionFolder)
        {
            ObservableList<RQMTestPlan> RQMTestPlanList = new ObservableList<RQMTestPlan>();


            string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
            if (File.Exists(importConfigTemplate))
            {
                XmlSerializer serializer = new
                XmlSerializer(typeof(RQMProjectListConfiguration));

                FileStream fs = new FileStream(importConfigTemplate, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                RQMProjectListConfiguration RQMProjectList;
                RQMProjectList = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                fs.Close();

                RQMProject currentRQMProjectMapping;
                if (RQMProjectList.RQMProjects.Count > 0)
                {
                    currentRQMProjectMapping = RQMProjectList.RQMProjects.Where(x => x.Name == RQMProject || x.Name == "DefaultProjectName").FirstOrDefault();
                    if (currentRQMProjectMapping != null)
                    {
                        //
                        //
                        LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                        IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                        rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                        IProjectDefinitions currentProj = rqmProjectsDataList.Where(x => x.ProjectName == RQMProject).FirstOrDefault();

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
                                                                        DateTime.Parse(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreationDate, nsmgr).InnerText.ToString()).ToLocalTime()));
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error while trying to retrieve the following TestPlan page:" + responseData.RequestUri);
                                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                            }
                        }
                    }
                }
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error while trying to import RQM test plans, RQM_ImportConfigs_Template.xml wasn't found at: " + importConfigTemplate);
            }

            return RQMTestPlanList;
        }

        public RQMTestPlan GetRQMTestPlanByIdByProject(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, string RQMTestPlanId)
        {
            RQMTestPlan testPlanRes = null;

            string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
            if (File.Exists(importConfigTemplate))
            {
                XmlSerializer serializer = new
                XmlSerializer(typeof(RQMProjectListConfiguration));

                FileStream fs = new FileStream(importConfigTemplate, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                RQMProjectListConfiguration RQMProjectList;
                RQMProjectList = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                fs.Close();
                RQMProject currentRQMProjectMapping;
                if (RQMProjectList.RQMProjects.Count > 0)
                {
                    currentRQMProjectMapping = RQMProjectList.RQMProjects.Where(x => x.Name == RQMProject || x.Name == "DefaultProjectName").FirstOrDefault();
                    if (currentRQMProjectMapping != null)
                    {
                        //
                        //
                        LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                        IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                        rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                        IProjectDefinitions currentProj = rqmProjectsDataList.Where(x => x.ProjectName == RQMProject).FirstOrDefault();
                        RqmResponseData responseData = RQMRep.GetTestPlanByIdByProject(loginData, currentProj.Prefix, currentProj.Guid, RQMTestPlanId);
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
                                testPlanRes = new RQMTestPlan(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.Name, nsmgr).InnerText.ToString(),
                                                                    testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.URLPath, nsmgr).Attributes[0].InnerText.ToString(),
                                                                    testPlan.Attributes[0].InnerText.ToString(),
                                                                    testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.RQMID, nsmgr).InnerText.ToString(),
                                                                    testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreatedBy, nsmgr).Attributes[0].InnerText.Split('/').Last().ToString(),
                                                                    DateTime.Parse(testPlan.SelectSingleNode(currentRQMProjectMapping.RQMTestPlansListMapping.CreationDate, nsmgr).InnerText.ToString()).ToLocalTime());

                                XmlNodeList testSuitesURInodes = testPlan.SelectNodes(currentRQMProjectMapping.RQMTestPlansListMapping.ContainedTestSuitesList, nsmgr);
                                foreach (XmlNode testSuitesURInode in testSuitesURInodes)
                                {
                                    RQMTestSuite rQMTestSuite = new RQMTestSuite(testSuitesURInode.Attributes[0].InnerText.ToString());

                                    try
                                    {
                                        // TestSuite data from RQM
                                        RqmResponseData responseDataTestSuite = RQMRep.GetRqmResponse(loginData, new Uri(testSuitesURInode.Attributes[0].InnerText.ToString()));
                                        XmlDocument docTestSuite = new XmlDocument();
                                        docTestSuite.LoadXml(responseDataTestSuite.responseText.ToString());
                                        XmlNamespaceManager nsmgrTestSuite = new XmlNamespaceManager(reader.NameTable);
                                        currentRQMProjectMapping.RQMTestSuiteAsItemMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTestSuite.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                        XmlNode responseDataNodeTestSuite = docTestSuite.DocumentElement;
                                        rQMTestSuite.RQMID = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteAsItemMapping.RQMID, nsmgrTestSuite).InnerText.ToString();
                                        rQMTestSuite.Name = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteAsItemMapping.Name, nsmgrTestSuite).InnerText.ToString();
                                        // TestSuite data from RQM
                                        RqmResponseData responseDataTestSuiteExecutionRecords = RQMRep.GetTestSuiteExecutionRecordsByTestSuite(loginData, currentProj.Prefix, currentProj.Guid, testSuitesURInode.Attributes[0].InnerText.ToString());
                                        XmlDocument docTestSuiteExecutionRecords = new XmlDocument();
                                        docTestSuiteExecutionRecords.LoadXml(responseDataTestSuiteExecutionRecords.responseText.ToString());
                                        XmlNamespaceManager nsmgrTestSuiteExecutionRecords = new XmlNamespaceManager(reader.NameTable);
                                        currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.RQMNameSpaces.RQMNameSpaceList.ForEach(y => nsmgrTestSuiteExecutionRecords.AddNamespace(y.RQMNameSpacePrefix, y.RQMNameSpaceName));
                                        XmlNode responseDataNodeExecutionRecords = docTestSuiteExecutionRecords.DocumentElement;
                                        try
                                        {
                                            rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.URLPathVersioned = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.CurrentTestSuiteResult, nsmgrTestSuiteExecutionRecords).Attributes[0].InnerText.ToString();
                                        }
                                        catch { }
                                        try
                                        {
                                            rQMTestSuite.TestSuiteExecutionRecord.URLPathVersioned = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.XMLTestSuiteExecutionRecord, nsmgrTestSuiteExecutionRecords).Attributes[0].InnerText.ToString();
                                            rQMTestSuite.TestSuiteExecutionRecord.RQMID = responseDataNodeExecutionRecords.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteExecutionRecordMapping.RQMID, nsmgrTestSuiteExecutionRecords).InnerText.ToString();
                                        }
                                        catch { }
                                    }
                                    catch { }
                                    testPlanRes.TestSuites.Add(rQMTestSuite);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error while trying to retrieve TestPlan id:" + RQMTestPlanId);
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                        }
                    }
                }
            }
            return testPlanRes;
        }

        public RQMTestPlan GetRQMTestPlanFullData(string RQMServerUrl, string RQMUserName, string RQMPassword, string RQMProject, RQMTestPlan testPlan)
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
            if (File.Exists(importConfigTemplate))
            {
                XmlSerializer serializer = new
                XmlSerializer(typeof(RQMProjectListConfiguration));

                FileStream fs = new FileStream(importConfigTemplate, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                RQMProjectListConfiguration RQMProjectList;
                RQMProjectList = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                fs.Close();

                RQMProject currentRQMProjectMapping;
                if (RQMProjectList.RQMProjects.Count > 0)
                {
                    currentRQMProjectMapping = RQMProjectList.RQMProjects.Where(x => x.Name == RQMProject || x.Name == "DefaultProjectName").FirstOrDefault();
                    if (currentRQMProjectMapping != null)
                    {
                        //
                        // building a list of TestCases
                        LoginDTO loginData = new LoginDTO() { User = RQMUserName, Password = RQMPassword, Server = RQMServerUrl };
                        IProjectData rqmProjectsData = RQMRep.GetVisibleProjects(loginData);
                        rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
                        IProjectDefinitions currentProj = rqmProjectsDataList.Where(x => x.ProjectName == RQMProject).FirstOrDefault();

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
                        // this is only enchancment that will add to them test cases and some values like descritpion and so on
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

                            RQMTestSuite currentTestSuite = testPlan.TestSuites.Where(z => z.RQMID == responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.RQMID, nsmgrTS).InnerText.ToString()).FirstOrDefault();

                            currentTestSuite.Name = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.Name, nsmgrTS).InnerText.ToString();
                            currentTestSuite.CreatedBy = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.RQMID, nsmgrTS).InnerText.ToString();
                            currentTestSuite.Description = responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.Description, nsmgrTS).InnerText.ToString();
                            currentTestSuite.CreationDate = DateTime.Parse(responseDataNodeTestSuite.SelectSingleNode(currentRQMProjectMapping.RQMTestSuiteMapping.CreationDate, nsmgrTS).InnerText.ToString()).ToLocalTime();
                            currentTestSuite.TestCases = currentSuiteTestCases;
                            
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error while trying to import selected RQM test plan, RQM_ImportConfigs_Template.xml wasn't found at: " + importConfigTemplate);
            }

            Mouse.OverrideCursor = null;
            return testPlan;
        }

        public ObservableList<RQMTestCase> BuildRQMTestCaseList(XmlNodeList testCases, XmlNamespaceManager nsmgr, LoginDTO loginData, RQMProject currentRQMProjectMapping, XmlReader reader, string TestSuiteTitle, string TestSuiteId)
        {
            ObservableList<RQMTestCase> RQMTestCaseList = new ObservableList<RQMTestCase>();
            foreach (XmlNode testCase in testCases)
            {
                RqmResponseData responseDataTC = RQMRep.GetRqmResponse(loginData, new Uri(testCase.Attributes[0].InnerText.ToString()));
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
                                                  ImportFromRQM.StripHTML(step.SelectSingleNode(currentRQMProjectMapping.RQMStepMapping.ExpectedResult, nsmgr).InnerText.ToString()));
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
            return RQMTestCaseList;
        }

        public ObservableList<RQMExecutionRecord> GetExecutionRecordsByTestPlan(LoginDTO loginData, XmlReader reader, RQMProject currentRQMProjectMapping, string currentProjPrefix, string currentProjGuid, string testPlanURLPathVersioned)
        {
            ObservableList<RQMExecutionRecord> RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();

            RqmResponseData responseDataExecutionRecords = RQMRep.GetExecutionByTestPlan(loginData, currentProjPrefix, currentProjGuid, testPlanURLPathVersioned);
            XmlDocument docExecutionRecords = new XmlDocument();
            docExecutionRecords.LoadXml(responseDataExecutionRecords.responseText.ToString());
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
                    docVersionedTC.LoadXml(responseDataVersionedTC.responseText.ToString());
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
                    docVersionedTS.LoadXml(responseDataVersionedTS.responseText.ToString());
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
            return RQMExecutionRecords;
        }

        public ObservableList<RQMExecutionRecord> GetTestSuiteCurrentResult(LoginDTO loginData, XmlReader reader, RQMProject currentRQMProjectMapping, string currentProjPrefix, string currentProjGuid, string currentTestSuiteResultUri)
        {
            if ((currentTestSuiteResultUri == null) || (currentTestSuiteResultUri == string.Empty))
                return null;

            ObservableList<RQMExecutionRecord> RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();

            RqmResponseData responseDataCurrentTestSuiteResult = RQMRep.GetRqmResponse(loginData, new Uri(currentTestSuiteResultUri));
            XmlDocument docTestSuiteResult = new XmlDocument();
            docTestSuiteResult.LoadXml(responseDataCurrentTestSuiteResult.responseText.ToString());
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
                    docExecutionRecords.LoadXml(responseDataExecutionRecord.responseText.ToString());
                    XmlNode responseDataNodeExecutionRecord = docExecutionRecords.DocumentElement;
                    XmlNode executionRecord = responseDataNodeExecutionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.XMLPathOfSingleSelectionCase, nsmgrExecutionRecords);

                    string curentExecutionRecordVersionedTCUri = executionRecord.SelectSingleNode(currentRQMProjectMapping.RQMExecutionRecordsMapping.RelatedTestCaseUri, nsmgrExecutionRecords).Attributes[0].InnerText.ToString();
                    RqmResponseData responseDataVersionedTC = RQMRep.GetRqmResponse(loginData, new Uri(curentExecutionRecordVersionedTCUri));
                    XmlDocument docVersionedTC = new XmlDocument();
                    docVersionedTC.LoadXml(responseDataVersionedTC.responseText.ToString());
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
                    docVersionedTS.LoadXml(responseDataVersionedTS.responseText.ToString());
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

            return RQMExecutionRecords;
        }
    }
}
