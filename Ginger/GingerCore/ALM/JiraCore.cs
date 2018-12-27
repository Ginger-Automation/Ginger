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
using GingerCore.ALM.JIRA;
using Ginger;
using System;
using System.Collections.Generic;
using GingerCore.Activities;
using ALM_Common.DataContracts;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.IO.Compression;
using Newtonsoft.Json;
using GingerCore.External;
using Amdocs.Ginger.Repository;
using GingerCore.ALM.JIRA.Bll;

namespace GingerCore.ALM
{
    public class JiraCore : ALMCore
    {
        private JiraExportManager exportMananger;
        private JiraConnect jiraConnectObj;
        private JiraRepository.JiraRepository jiraRepObj;
        public static string ALMProjectGroupName { get; set; }
        public static string ALMProjectGuid { get; set; }
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<ExternalItemFieldBase> almItemFields { get ; set ; }
        public JiraCore()
        {
            jiraRepObj = new JiraRepository.JiraRepository();
            exportMananger = new JiraExportManager(jiraRepObj);
            jiraConnectObj = new JiraConnect(jiraRepObj);
        }
        public override bool ConnectALMProject()
        {
            return jiraConnectObj.SetJiraProjectFullDetails();
        }

        public override bool ConnectALMServer()
        {
            return jiraConnectObj.ConnectJiraServer();
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return jiraConnectObj.DisconnectALMProjectStayLoggedIn();
        }

        public override void DisconnectALMServer()
        {
            jiraConnectObj.DisconnectJiraServer();
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmConfig.ALMDomain = ALMDomainName;
            return jiraConnectObj.GetJiraDomainProjects();
        }

        public override List<string> GetALMDomains()
        {
            return jiraConnectObj.GetJiraDomains();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> tempFieldsList = ImportFromJira.GetALMItemFields(bw, online);
            almItemFields = new ObservableList<ExternalItemFieldBase>();
            foreach (ExternalItemFieldBase item in tempFieldsList)
            {
                almItemFields.Add((ExternalItemFieldBase)item.CreateCopy());
            }
            return tempFieldsList;
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup,string uploadPath, IEnumerable<ExternalItemFieldBase> testCaseFields, IEnumerable<ExternalItemFieldBase> designStepsFields, ref string res)
        {
            return true;
        }
    }
}
