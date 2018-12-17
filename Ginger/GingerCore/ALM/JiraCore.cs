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
using TDAPIOLELib;
using ALM_Common.DataContracts;
using GingerCore.ALM.QC;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.IO.Compression;
using Newtonsoft.Json;
using GingerCore.External;
using Amdocs.Ginger.Repository;

namespace GingerCore.ALM
{
    public class JiraCore : ALMCore
    {
        public static string ALMProjectGroupName { get; set; }
        public static string ALMProjectGuid { get; set; }
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool ConnectALMProject()
        {
            return JiraConnect.Instance.SetJiraProjectFullDetails();
        }

        public override bool ConnectALMServer()
        {
            return JiraConnect.Instance.ConnectJiraServer();
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return JiraConnect.Instance.DisconnectALMProjectStayLoggedIn();
        }

        public override void DisconnectALMServer()
        {
            JiraConnect.Instance.DisconnectJiraServer();
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmConfig.ALMDomain = ALMDomainName;
            return JiraConnect.Instance.GetJiraDomainProjects();
        }

        public override List<string> GetALMDomains()
        {
            return JiraConnect.Instance.GetJiraDomains();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            return ImportFromJira.GetALMItemFields(bw, online);
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }
    }
}
