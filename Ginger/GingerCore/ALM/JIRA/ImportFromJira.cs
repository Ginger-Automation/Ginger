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

using ALM_Common.Abstractions;
using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.External;
using GingerCore.Variables;
using Newtonsoft.Json;
using ALM_Common.Data_Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using TDAPIOLELib;
using JiraRepository;

namespace GingerCore.ALM.JIRA
{
    class ImportFromJira
    {
        internal static ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            try
            {
                JiraRepository.JiraRepository jiraRep = new JiraRepository.JiraRepository();
                LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
                AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testCaseFieldsList;
                testCaseFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMProjectName, ALM_Common.DataContracts.ResourceType.TEST_CASE);

                //Get Test Case fields
                foreach (var field in testCaseFieldsList.DataResult)
                {
                    if (string.IsNullOrEmpty(field.name)) continue;

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.name;
                    itemfield.Name = field.name;
                    itemfield.Mandatory = field.required;
                    if (itemfield.Mandatory)
                        itemfield.ToUpdate = true;
                    itemfield.ItemType = ResourceType.TEST_CASE.ToString();

                    if (field.allowedValues.Count > 0)
                    {
                        itemfield.SelectedValue = field.allowedValues[0].name;
                    }
                    else
                    {
                        itemfield.SelectedValue = "Unassigned";
                    }
                    fields.Add(itemfield);
                }
            }
            catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }

            return fields;
        }
    }
}
