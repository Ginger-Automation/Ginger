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

using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore;
using GingerCore.Variables;
using GingerCore.Actions;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeActivity : AnalyzerItemBase
    {
        public Activity mActivity { get; set; }
        public BusinessFlow mBusinessFlow { get; set; }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow BusinessFlow, Activity Activity)
        {
            // Put all tests on Activity here
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();

            // Check Activity have Target App            
            if (string.IsNullOrEmpty(Activity.TargetApplication))
            {
                AnalyzeActivity AA = CreateNewIssue(BusinessFlow, Activity);
                AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " is missing target Application";
                AA.Details = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " doesn't have Target Application(s) defined";
                AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: "s") + " in solution tab and add target apps";
                AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;   //  take it from solution 
                AA.FixItHandler = FixTargetApplicationHandler;
                AA.IssueType = eType.Error;
                AA.Impact = "Might be executed on wrong application or not executed at all and will fail at run time";
                AA.Severity = eSeverity.High;
                AA.Selected = true;

                IssuesList.Add(AA);
            }

            // Check Activity have actions
            if (Activity.Acts.Count() == 0)
            {
                AnalyzeActivity AA = CreateNewIssue(BusinessFlow, Activity);
                AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " is missing Actions";
                AA.Details = GingerDicser.GetTermResValue(eTermResKey.Activity) + " doesn't have Actions";
                AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and add actions or remove this " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // we can autofix by delete, but don't want to                
                AA.IssueType = eType.Warning;
                AA.Impact = "Will be marked as pass and can give wrong impression while nothing is executed";
                AA.Severity = eSeverity.Medium;
                IssuesList.Add(AA);
            }

            // Check Activity target app exist in the BF target app
            //Check only when there is  target app, since the user will get no target err before
            if (!string.IsNullOrEmpty(Activity.TargetApplication))
            {
                string AppName = (from x in BusinessFlow.TargetApplications where x.Name == Activity.TargetApplication select x.Name).FirstOrDefault();
                if (string.IsNullOrEmpty(AppName))
                {
                    string BFApps = string.Join(";", BusinessFlow.TargetApplications.Select(p => p.Name).ToList());

                    AnalyzeActivity AA = CreateNewIssue(BusinessFlow, Activity);
                    AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " target application not found in " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    AA.Details = GingerDicser.GetTermResValue(eTermResKey.Activity) + " target application = '" + Activity.TargetApplication + "' while " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " target app(s) is: '" + BFApps + "'";
                    AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and add set correct target application";
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // yes if we have one target app, or just set the first 
                    AA.IssueType = eType.Error;
                    AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will not be executed and will fail";
                    AA.Severity = eSeverity.Critical;

                    IssuesList.Add(AA);
                }
            }
            return IssuesList;
        }

        private static void FixTargetApplicationHandler(object sender, EventArgs e)
        {
            //Do recheck
            AnalyzeActivity AA = (AnalyzeActivity)sender;
            if (string.IsNullOrEmpty(AA.mActivity.TargetApplication))
            {
                AA.mActivity.TargetApplication = AA.mBusinessFlow.TargetApplications[0].Name;
                AA.Status = eStatus.Fixed;
            }
        }

        static AnalyzeActivity CreateNewIssue(BusinessFlow BusinessFlow, Activity Activity)
        {
            AnalyzeActivity AA = new AnalyzeActivity();
            AA.Status = AnalyzerItemBase.eStatus.NeedFix;
            AA.mActivity = Activity;
            AA.ItemName = Activity.ActivityName;
            AA.ItemParent = BusinessFlow.Name;
            AA.mBusinessFlow = BusinessFlow;
            AA.ItemClass = "Activity";
           
            return AA;
        }
        public static List<string> GetUsedVariableFromActivity(Activity activity)
        {
            List<string> activityUsedVariables = new List<string>();
            VariableBase.GetListOfUsedVariables(activity, ref activityUsedVariables);
            return activityUsedVariables;
        }
    }
}
