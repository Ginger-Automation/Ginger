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
                AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testSetFieldsList;
                AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testExecutionFieldsList;

                testSetFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMDomain, ALM_Common.DataContracts.ResourceType.TEST_SET);
                testCaseFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMDomain, ALM_Common.DataContracts.ResourceType.TEST_CASE);
                testExecutionFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMDomain, ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS);
                
                fields.Append(SetALMItemsFields(testSetFieldsList, ResourceType.TEST_SET));
                fields.Append(SetALMItemsFields(testCaseFieldsList, ResourceType.TEST_CASE));
                fields.Append(SetALMItemsFields(testExecutionFieldsList, ResourceType.TEST_CASE_EXECUTION_RECORDS));
            }
            catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }

            return fields;
        }

        private static ObservableList<ExternalItemFieldBase> SetALMItemsFields( AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testCaseFieldsList, ResourceType fieldResourceType)
        {
            ObservableList<ExternalItemFieldBase> resourceFields = new ObservableList<ExternalItemFieldBase>();
            string fieldResourceTypeToString = fieldResourceType.ToString();
            if (fieldResourceType == ResourceType.TEST_CASE_EXECUTION_RECORDS)
            {
                fieldResourceTypeToString = "TEST_EXECUTION";
            }
            foreach (var field in testCaseFieldsList.DataResult)
            {
                if (string.IsNullOrEmpty(field.name)) continue;

                ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                itemfield.ID = field.name;
                itemfield.Name = field.name;
                itemfield.Mandatory = field.required;
                itemfield.ItemType = fieldResourceTypeToString;

                if (field.allowedValues.Count > 0)
                {
                    itemfield.SelectedValue = (field.allowedValues[0].name != null) ? field.allowedValues[0].name : field.allowedValues[0].value;
                    foreach (var item in field.allowedValues)
                    {
                        itemfield.PossibleValues.Add((item.name != null) ? item.name : item.value);
                    }
                }
                else
                {
                    itemfield.SelectedValue = "Unassigned";
                }
                resourceFields.Add(itemfield);
            }
            return resourceFields;
        }
    }
}
