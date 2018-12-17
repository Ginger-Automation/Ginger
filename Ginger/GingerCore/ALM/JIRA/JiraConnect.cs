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
using RQM_Repository.Data_Contracts;
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
        AlmDomainColl JiraProjectsDataList;

        private JiraConnect()
        {
        }

        public void CreateJiraRepository()
        {
            
        }

        public bool SetJiraProjectFullDetails()
        {
            GetJiraDomainProjects();

            //IProjectDefinitions selectedProj = JiraProjectsDataList.Where(x => x.ProjectName.Equals(ALMCore.AlmConfig.ALMProjectName) == true).FirstOrDefault();
            //if (selectedProj != null)
            //{
            //    //Save selected project details
            //    connectedProjectDefenition = selectedProj;
            //    ALMCore.AlmConfig.ALMProjectName = selectedProj.ProjectName;
            //    JiraCore.ALMProjectGuid = selectedProj.Guid;
            //    JiraCore.ALMProjectGroupName = selectedProj.Prefix;
            //    return true;
            //}
            return false;
        }

        #region singlton
        private static readonly JiraConnect _instance = new JiraConnect();
        public static JiraConnect Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion singlton

        private JiraRepository.JiraRepository mJiraRep;
        public JiraRepository.JiraRepository JiraRep
        {
            get
            {
                if (mJiraRep == null)
                    return new JiraRepository.JiraRepository();
                return mJiraRep;
            }
            set { mJiraRep = value; }
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

            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            isUserAuthen = JiraRep.IsUserAuthenticated(loginData);

            return isUserAuthen;
        }

        internal List<string> GetJiraDomainProjects()
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            ALM_Common.DataContracts.AlmResponseWithData<AlmDomainColl> jiraProjectsData = JiraRep.GetLoginProjects(loginData.User, loginData.Password,loginData.Server);
            JiraProjectsDataList = jiraProjectsData.DataResult;

            List<string> JiraProjects = new List<string>();
            foreach (var proj in JiraProjectsDataList)
            {
                //JiraProjects.Add(proj.ProjectName);
            }

            return JiraProjects;
        }

        internal List<string> GetJiraDomains()
        {
            List<string> RQMDomains = new List<string>();
            RQMDomains.Add("JIRA Domain");
            return RQMDomains;
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
