﻿#region License
/*
Copyright © 2014-2020 European Support Limited

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

using ALM_Common.DataContracts;
using ALM_Common.Data_Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using ALM_Common.Abstractions;

namespace GingerCore.ALM.JIRA
{
    public class JiraConnectManager
    {
        const string BTS_ACTIVITY_ID = "bts_activity_data";
        const string BTS_ACTIVITY_STEPS_ID = "bts_step_data";

        bool connectedToServer;
        bool connectedToProject;
        IProjectDefinitions connectedProjectDefenition;
        AlmDomainColl jiraDomainsProjectsDataList;
        private JiraRepository.JiraRepository jiraRepositoryObj;

        public JiraConnectManager(JiraRepository.JiraRepository jiraRep)
        {
            this.jiraRepositoryObj = jiraRep;
        }
        public void CreateJiraRepository()
        {
            jiraRepositoryObj = new JiraRepository.JiraRepository(ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath);
        }
        public bool SetJiraProjectFullDetails()
        {
            GetJiraDomainProjects();
            List<ProjectArea> currentProjects = jiraDomainsProjectsDataList.Where(x => x.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).Select(prjs => prjs.Projects).FirstOrDefault();
            IProjectDefinitions selectedProj = currentProjects.Where(prj => prj.ProjectId.ToString() == ALMCore.DefaultAlmConfig.ALMProjectKey.ToString()).FirstOrDefault();
            if (selectedProj != null)
            {
                //Save selected project details
                connectedProjectDefenition = selectedProj;
                ALMCore.DefaultAlmConfig.ALMProjectName = selectedProj.ProjectName;
                ALMCore.DefaultAlmConfig.ALMProjectKey = selectedProj.ProjectId.ToString();
                JiraCore.ALMProjectGuid = selectedProj.Guid;
                JiraCore.ALMProjectGroupName = selectedProj.ProjectId.ToString();
                return true;
            }
            return false;
        }
        public bool ConnectJiraServer()
        {
            if (JiraConnectionTest())
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

        private bool JiraConnectionTest()
        {
            bool isUserAuthen;

            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
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
                currentDomainProject = jiraDomainsProjectsDataList.Where(dom => dom.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).Select(prj => prj.Projects).FirstOrDefault();
                jiraProjects = currentDomainProject.ToDictionary(x => x.ProjectId.ToString(), x => x.ProjectName);          
            }
            return jiraProjects;
        }
        
        internal List<string> GetJiraDomains()
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
            jiraDomainsProjectsDataList = jiraRepositoryObj.GetLoginProjects(loginData.User, loginData.Password, loginData.Server).DataResult;
            List<string> jiraDomains = new List<string>();
            foreach (var domain in jiraDomainsProjectsDataList)
            {
                if (!domain.DomainName.Equals(String.Empty))
                {
                    jiraDomains.Add(domain.DomainName);
                }
            }
            return jiraDomains;
        }

        internal List<string> GetJiraTestingALMs()
        {
            return jiraRepositoryObj.GetJiraTestALM();
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
