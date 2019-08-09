#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Ginger.Repository.AddItemToRepositoryWizard;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    public class ValidateActivity : ItemValidationBase
    {
        public  static void Validate(Activity activity)
        {          
            if (activity == null)
                return;

            List<string> missingVariables = CheckMissingVariables(activity);
            if (missingVariables.Count>0)
            {
                ItemValidationBase VA = CreateNewIssue(activity);
                VA.IssueDescription = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + ":" + GetListNameString(missingVariables) + "  is/are missing";
                VA.mIssueType = eIssueType.MissingVariables;
                VA.missingVariablesList = missingVariables;
                VA.IssueResolution = "Missing " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " will be auto added to " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                VA.Selected = true;
                mIssuesList.Add(VA);
            }            
        }

        private static string GetListNameString(List<string> list)
        {
            string str = string.Empty;

            foreach(string s in list)
            {
                str += s+",";
            }

            if(!String.IsNullOrEmpty(str))
                str= str.Remove(str.LastIndexOf(','));

            return str;
        }

        static List<string> CheckMissingVariables(Activity activity)
        {
            List<string> usedVariables = new List<string>();
            foreach (Act action in activity.Acts)
                VariableBase.GetListOfUsedVariables(action, ref usedVariables);

            for (int indx = 0; indx < usedVariables.Count; indx++)
            {
                if (activity.Variables.Where(x => x.Name == usedVariables[indx]).FirstOrDefault() != null)
                {
                    usedVariables.RemoveAt(indx);
                    indx--;
                }
            }
            return usedVariables;
        }
    }
}
