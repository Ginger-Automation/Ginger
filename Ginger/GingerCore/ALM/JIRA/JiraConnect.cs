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
using ALM_Common.Data_Contracts;
using JiraRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ALM_Common.Abstractions;
using JiraRepository.Data_Contracts;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace GingerCore.ALM.JIRA
{
    public class JiraConnect
    {
        const string BTS_ACTIVITY_ID = "bts_activity_data";
        const string BTS_ACTIVITY_STEPS_ID = "bts_step_data";

        bool connectedToServer;
        bool connectedToProject;
        IProjectDefinitions connectedProjectDefenition;
        AlmDomainColl jiraDomainsProjectsDataList;

        public JiraConnect(JiraRepository.JiraRepository jiraRep)
        {
            this.jiraRepositoryObj = jiraRep;
        }

        public void CreateJiraRepository()
        {
            
        }

        public bool SetJiraProjectFullDetails()
        {
            GetJiraDomainProjects();
            List<ProjectArea> currentProjects = jiraDomainsProjectsDataList.Where(x => x.DomainName.Equals(ALMCore.AlmConfig.ALMDomain)).Select(prjs => prjs.Projects).FirstOrDefault();
            IProjectDefinitions selectedProj = currentProjects.Where(prj => prj.Prefix.Equals(ALMCore.AlmConfig.ALMProjectKey)).FirstOrDefault();
            if (selectedProj != null)
            {
                //Save selected project details
                connectedProjectDefenition = selectedProj;
                ALMCore.AlmConfig.ALMProjectName = selectedProj.ProjectName;
                ALMCore.AlmConfig.ALMProjectKey = selectedProj.Prefix;
                JiraCore.ALMProjectGuid = selectedProj.Guid;
                JiraCore.ALMProjectGroupName = selectedProj.Prefix;
                return true;
            }
            return false;
        }

        private JiraRepository.JiraRepository jiraRepositoryObj;
        public bool ConnectJiraServer()
        {
            if (JiraConnectionTest())
            {
                connectedToServer = true;
                return connectedToServer;
            }
            return false;
        }

        public ObservableList<JiraTestSet> GetJiraTestSets()
        {
            AlmResponseWithData<List<JiraIssue>> getTestsSet = jiraRepositoryObj.GetJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMDomain, ResourceType.TEST_SET, null);
            
            ObservableList<JiraTestSet> jiratestset = new ObservableList<JiraTestSet>();
            List<string> testSetKeys = new List<string> { "reporter", "created", "summary", "project" };
            foreach (var item in getTestsSet.DataResult)
            {
                JiraTestSet issue = new JiraTestSet();
                issue.ID = item.id.ToString();
                issue.URLPath = item.self;
                issue.Key = item.key;
                
                foreach (string tsKey in testSetKeys)
                {
                    if (item.fields.ContainsKey(tsKey))
                    {
                        string fieldValue = getSelectedFieldValue(item.fields[tsKey], tsKey);
                        switch (tsKey)
                        {
                            case "created":
                                issue.DateCreated = fieldValue;
                                break;
                            case "summary":
                                issue.Name = fieldValue;
                                break;
                            case "reporter":
                                issue.CreatedBy = fieldValue;
                                break;
                            case "project":
                                issue.Project = fieldValue;
                                break;
                        }
                    }
                }
                jiratestset.Add(issue);
            }
            return jiratestset;
        }

        private string getSelectedFieldValue(dynamic fields, string fieldName)
        {
            Dictionary<string, object> array = new Dictionary<string, object>();
            FieledSchema temp = jiraRepositoryObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_SET, "DE", fieldName);
            if(temp == null)
            {
                return "";
            }
            switch(temp.type)
            {
                case "string":
                    return fields.Value;
                case "object":
                    var jsonTemplateObj = Newtonsoft.Json.Linq.JObject.Parse(temp.data);
                    return fields[((Newtonsoft.Json.Linq.JProperty)jsonTemplateObj.First).Name];
            }
            return "";
        }

        private string getFieldName(dynamic d)
        {
            string itemValue;
            object propValue = d.GetType().GetProperties();
            foreach (Newtonsoft.Json.Linq.JProperty property in d.Properties())
            {

            }
            switch (d.GetType().Name)
            {
                case "JObject":
                    //dynamic json = Newtonsoft.Json.Linq.JValue.Parse(d);
                    itemValue = d.name;
                    break;
                case "JValue":
                    itemValue = d;
                    break;
            }
            return "";
        }

        public bool IsServerConnected()
        {
            return connectedToServer;
        }

        private bool JiraConnectionTest()
        {
            bool isUserAuthen;

            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            isUserAuthen = jiraRepositoryObj.IsUserAuthenticated(loginData);

            return isUserAuthen;
        }

        internal Dictionary<string, string> GetJiraDomainProjects()
        {
            Dictionary<string, string> jiraProjects = new Dictionary<string, string>();
            List<ProjectArea> currentDomainProject = new List<ProjectArea>();
            if (jiraDomainsProjectsDataList == null)
            {
                GetJiraDomains();
            }
            if (jiraDomainsProjectsDataList.Count > 0)
            {
                currentDomainProject = jiraDomainsProjectsDataList.Where(dom => dom.DomainName.Equals(ALMCore.AlmConfig.ALMDomain)).Select(prj => prj.Projects).FirstOrDefault();
                foreach (var proj in currentDomainProject)
                {
                    jiraProjects.Add(proj.Prefix,proj.ProjectName);
                }
            }
            return jiraProjects;
        }
        
        internal List<string> GetJiraDomains()
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            jiraDomainsProjectsDataList = jiraRepositoryObj.GetLoginProjects(loginData.User, loginData.Password, loginData.Server).DataResult;
            List<string> jiraDomains = new List<string>();
            foreach (var domain in jiraDomainsProjectsDataList)
            {
                jiraDomains.Add(domain.DomainName);
            }
            return jiraDomains;
        }

        public void DisconnectJiraServer()
        {
            connectedToProject = false;
            connectedToServer = false;
        }

        public bool DisconnectALMProjectStayLoggedIn()
        {
            connectedToProject = false;
            return connectedToProject;
        }
    }
}
