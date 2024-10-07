#region License
/*
Copyright © 2014-2024 European Support Limited

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

using Amdocs.Ginger.CoreNET.AnalyzerLib;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeActivity : AnalyzerItemBase
    {
        public Activity Activity { get; private set; }
        public BusinessFlow BusinessFlow { get; private set; }

        public AnalyzeActivity(Activity activity)
        {
            Activity = activity;
        }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow businessFlow, Activity activity)
        {
            // Put all tests on Activity here
            List<AnalyzerItemBase> issues = [];

            CheckIfHasTargetAppliction(activity, businessFlow, ref issues);

            CheckIfHasAnyAction(activity, businessFlow, ref issues);

            CheckIfTargetApplicationExistInBusinessFlow(activity, businessFlow, ref issues);

            CheckIfExistsInAnyActivityGroup(activity, businessFlow, ref issues);

            AnalyzeValueExpInActivity(activity, businessFlow, ref issues);
            return issues;
        }

        /// <summary>
        /// Check if Activity has a TargetApplication.
        /// </summary>
        private static void CheckIfHasTargetAppliction(Activity activity, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            if (string.IsNullOrEmpty(activity.TargetApplication))
            {
                AnalyzeActivity issue = CreateNewIssue(businessFlow, activity);
                issue.Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} is missing {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}";
                issue.Details = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} doesn't have {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} defined";
                issue.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: "s") + " in solution tab and add target apps";
                issue.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;   //  take it from solution 
                issue.FixItHandler = FixTargetApplicationHandler;
                issue.IssueType = eType.Error;
                issue.Impact = "Might be executed on wrong application or not executed at all and will fail at run time";
                issue.Severity = eSeverity.High;
                issue.Selected = true;

                issues.Add(issue);
            }
        }

        /// <summary>
        /// Check if Activity has any Action
        /// </summary>
        private static void CheckIfHasAnyAction(Activity activity, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            if (!activity.Acts.Any())
            {
                AnalyzeActivity AA = CreateNewIssue(businessFlow, activity);
                AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " is missing Actions";
                AA.Details = GingerDicser.GetTermResValue(eTermResKey.Activity) + " doesn't have Actions";
                AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and add actions or remove this " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // we can autofix by delete, but don't want to                
                AA.IssueType = eType.Warning;
                AA.Impact = "Will be marked as pass and can give wrong impression while nothing is executed";
                AA.Severity = eSeverity.Medium;

                issues.Add(AA);
            }
        }

        /// <summary>
        /// Check if Activity's TargetApplication exist in BusinessFlow.
        /// </summary>
        private static void CheckIfTargetApplicationExistInBusinessFlow(Activity activity, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            //Check only when there is  target app, since the user will get no target err before
            if (!string.IsNullOrEmpty(activity.TargetApplication))
            {
                string AppName = businessFlow.TargetApplications
                    .Select(targetApp => targetApp.Name)
                    .FirstOrDefault(targetAppName => string.Equals(targetAppName, activity.TargetApplication));
                if (string.IsNullOrEmpty(AppName))
                {
                    string BFApps = string.Join(";", businessFlow.TargetApplications.Select(p => p.Name).ToList());

                    AnalyzeActivity AA = CreateNewIssue(businessFlow, activity);
                    AA.Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}  not found in  {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}";
                    AA.Details = GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.TargetApplication) + "= '" + activity.TargetApplication + "' while " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " target app(s) is: '" + BFApps + "'";
                    AA.HowToFix = $"Open the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}  {GingerDicser.GetTermResValue(eTermResKey.Activity)}  and add set correct {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}";
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // yes if we have one target app, or just set the first 
                    AA.IssueType = eType.Error;
                    AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will not be executed and will fail";
                    AA.Severity = eSeverity.Critical;

                    issues.Add(AA);
                }
            }
        }

        /// <summary>
        /// Check if Activity exists in any ActivityGroup of the BusinessFlow.
        /// </summary>
        private static void CheckIfExistsInAnyActivityGroup(Activity activity, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issues)
        {
            bool existsInAnyActivityGroup = false;

            foreach (ActivitiesGroup activityGoup in businessFlow.ActivitiesGroups)
            {
                IEnumerable<ActivityIdentifiers> activityIdentifiers = activityGoup.ActivitiesIdentifiers;
                foreach (ActivityIdentifiers activityIdentifier in activityIdentifiers)
                {
                    if (activityIdentifier.ActivityGuid == activity.Guid && activityIdentifier.ActivityName == activity.ActivityName)
                    {
                        existsInAnyActivityGroup = true;
                        break;
                    }
                    else if (activityIdentifier.ActivityGuid == activity.Guid)
                    {
                        existsInAnyActivityGroup = true;
                        break;
                    }
                    else if (activityIdentifier.ActivityName == activity.ActivityName)
                    {
                        existsInAnyActivityGroup = true;
                        break;
                    }
                }

                if (existsInAnyActivityGroup)
                {
                    break;
                }
            }

            if (!existsInAnyActivityGroup)
            {
                AnalyzeActivity issue = CreateNewIssue(businessFlow, activity);

                string activityTerm = GingerDicser.GetTermResValue(eTermResKey.Activity);
                string businessFlowTerm = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                string activityGroupTerm = GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);

                issue.Description = $"{activityTerm} doesn't exist in any {activityGroupTerm}.";
                issue.Details = $"Every {activityTerm} in a {businessFlowTerm} must be a part of {activityGroupTerm}.";
                issue.HowToFix = $"If you got this issue during merge conflict, make sure you are selecting both {activity} and ActivityIdentifiers while merging.";
                issue.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                issue.IssueType = eType.Error;
                issue.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will not be executed and will fail.";
                issue.Severity = eSeverity.Critical;

                issues.Add(issue);
            }
        }

        private static void FixTargetApplicationHandler(object sender, EventArgs e)
        {
            //Do recheck
            AnalyzeActivity AA = (AnalyzeActivity)sender;
            if (string.IsNullOrEmpty(AA.Activity.TargetApplication))
            {
                AA.Activity.TargetApplication = AA.BusinessFlow.TargetApplications[0].Name;
                AA.Status = eStatus.Fixed;
            }
        }

        static AnalyzeActivity CreateNewIssue(BusinessFlow businessFlow, Activity activity)
        {
            AnalyzeActivity issue = new(activity)
            {
                Status = AnalyzerItemBase.eStatus.NeedFix,
                Activity = activity,
                ItemName = activity.ActivityName,
                ItemParent = businessFlow.Name,
                BusinessFlow = businessFlow,
                ItemClass = "Activity"
            };

            return issue;
        }

        public static List<string> GetUsedVariableFromActivity(Activity activity)
        {
            List<string> activityUsedVariables = [];
            VariableBase.GetListOfUsedVariables(activity, ref activityUsedVariables);
            return activityUsedVariables;
        }

        public static void AnalyzeValueExpInActivity(Activity activity, BusinessFlow businessFlow, ref List<AnalyzerItemBase> issuesList)
        {
            if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(activity.RunDescription, businessFlow.Environment))
            {
                var analyzeActivity = CreateNewIssue(businessFlow, activity);
                analyzeActivity.Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} Run Description uses an Environment Parameter which does not exist in the Current Environment";
                analyzeActivity.Severity = eSeverity.Medium;

                issuesList.Add(analyzeActivity);
            }

            var filteredVariables = activity.Variables
                 .Where((variable) => variable is VariableDynamic variableDynamic && !AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(variableDynamic.ValueExpression, businessFlow.Environment));


            foreach (var filteredVariable in filteredVariables)
            {
                var analyzeActivity = CreateNewIssue(businessFlow, activity);
                analyzeActivity.Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} {GingerDicser.GetTermResValue(eTermResKey.Variable)} {filteredVariable.Name} uses an Environment Parameter which does not exist in the Current Environment";
                analyzeActivity.Severity = eSeverity.High;
                issuesList.Add(analyzeActivity);
            }
        }
    }
}
