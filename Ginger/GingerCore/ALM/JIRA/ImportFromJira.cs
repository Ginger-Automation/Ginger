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
                //itemfield.ItemType = eQCItemType.TestCase.ToString();

                if (field.allowedValues != null) // field.List.RootNode.Children.Count > 0
                {
                    //CustomizationListNode lnode = field.allowedValues;
                    //List cNodes = lnode.Children;
                    //foreach (CustomizationListNode ccNode in cNodes)
                    //{
                    //    //adds list of valid selections of Field
                    //    itemfield.PossibleValues.Add(ccNode.Name);
                    //}
                }

                //if (itemfield.PossibleValues.Count > 0)
                //    itemfield.SelectedValue = itemfield.PossibleValues[0];
                //else
                //    itemfield.SelectedValue = "NA";

                //fields.Add(itemfield);
            }

            return fields;

            return null;
        }
    }
}
