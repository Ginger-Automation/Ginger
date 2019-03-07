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
using System.Text.RegularExpressions;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeBusinessFlow : AnalyzerItemBase
    {
        static string MissingTargetApp = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " is missing target Application(s)";
        static string MissingActivities = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " is missing " + GingerDicser.GetTermResValue(eTermResKey.Activities);
        public  static readonly string LegacyOutPutValidationDescription = "Output validation contains legacy operators which are very slow and does not work on Linux";
        private static Regex rxVarPattern = new Regex(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
        public BusinessFlow mBusinessFlow { get; set; }
        private Solution mSolution { get; set; }

        public List<ActReturnValue> ReturnValues { get; set; } = new List<ActReturnValue>();
        public static List<AnalyzerItemBase> Analyze(Solution Solution, BusinessFlow BusinessFlow)
        {
            // Put all tests on BF here
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();

            // Check BF have Target Apps            
            if (BusinessFlow.TargetApplications == null || BusinessFlow.TargetApplications.Count() == 0)
            {
                AnalyzeBusinessFlow ABF = new AnalyzeBusinessFlow();                    
                ABF.Description = MissingTargetApp;
                ABF.UTDescription = "MissingTargetApp";
                ABF.Details = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " doesn't have Target Application(s) defined";
                ABF.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " in solution tab and add target apps";
                ABF.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;   //  take it from solution 
                ABF.FixItHandler = FixMissingTargetApp;
                ABF.Status = AnalyzerItemBase.eStatus.NeedFix;
                ABF.IssueType = eType.Error;
                ABF.ItemParent = "NA";
                ABF.mBusinessFlow = BusinessFlow;
                ABF.ItemName = BusinessFlow.Name;
                ABF.Impact = "Might be executed on wrong application if solution have more than 1 target application";
                ABF.mSolution = Solution;
                ABF.ItemClass = "BusinessFlow";
                ABF.Severity = eSeverity.High;
                ABF.Selected = true;
                IssuesList.Add(ABF);
            }

            // Check BF have activities
            if (BusinessFlow.Activities.Count() == 0)
            {
                AnalyzeBusinessFlow ABF = new AnalyzeBusinessFlow();
                ABF.UTDescription = "MissingActivities";
                ABF.Description = MissingActivities;
                ABF.Details = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " doesn't have " + GingerDicser.GetTermResValue(eTermResKey.Activities);
                ABF.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and add " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " or remove this " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                ABF.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // we can autofix by delete, but don't want to
                ABF.Status = AnalyzerItemBase.eStatus.NeedFix;

                ABF.IssueType = eType.Warning;
                ABF.ItemParent = "NA";
                ABF.mBusinessFlow = BusinessFlow;
                ABF.Severity = eSeverity.Medium;
                ABF.ItemClass = "BusinessFlow";
                IssuesList.Add(ABF);
            }

            // Check BF have actions with legacy outputValidation

            AnalyzeBusinessFlow OutputValidationIssue = GetOutputvalidationErros(BusinessFlow);
            if (OutputValidationIssue.ReturnValues.Count > 0)
            {
                OutputValidationIssue.Description = LegacyOutPutValidationDescription;
                OutputValidationIssue.IssueType = eType.Warning;
                OutputValidationIssue.mBusinessFlow = BusinessFlow;
                OutputValidationIssue.Severity = eSeverity.Medium;
                OutputValidationIssue.ItemClass = "BusinessFlow";
                OutputValidationIssue.CanAutoFix = eCanFix.Yes;
                OutputValidationIssue.Status = AnalyzerItemBase.eStatus.NeedFix;
                IssuesList.Add(OutputValidationIssue);
                OutputValidationIssue.FixItHandler = FixOutputValidationIssue;
                IssuesList.Add(OutputValidationIssue);
            }
            return IssuesList;
        }

        private static void FixOutputValidationIssue(object sender, EventArgs e)
        {
            AnalyzeBusinessFlow ABF = (AnalyzeBusinessFlow)sender;
            foreach(ActReturnValue ARV in ABF.ReturnValues)
            {
                if (!string.IsNullOrEmpty(ARV.Expected)&&!ARV.Expected.Contains("{"))
                {
                    ARV.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
                }
                else
                {

                    if (!string.IsNullOrEmpty(ARV.Expected) && ARV.Expected.StartsWith(@"{Var "))
                    {
                        MatchCollection matches = rxVarPattern.Matches(ARV.Expected);
                        if (matches.Count == 1)
                        {
                            if (matches[0].Value.Equals(ARV.Expected))
                            {
                                ARV.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
                            }
                        }

                       
                    }
                }
            }

        }

        private static AnalyzeBusinessFlow GetOutputvalidationErros(BusinessFlow businessFlow)
        {
            AnalyzeBusinessFlow ABF = new AnalyzeBusinessFlow();

            IEnumerable<ObservableList<IAct>> ActionList = businessFlow.Activities.Select(x => x.Acts);

            foreach(ObservableList<IAct> ActList in ActionList)
            {
                foreach(Act action in ActList)
                {
                    ABF.ReturnValues.AddRange(action.ActReturnValues.Where(x => x.Operator == Amdocs.Ginger.Common.Expressions.eOperator.Legacy));
                }
            }
            return ABF;
        }

        private static void FixMissingTargetApp(object sender, EventArgs e)
        {
            // Take the target app from the solution
            AnalyzeBusinessFlow ABF = (AnalyzeBusinessFlow)sender;
            // Do recheck
            if (ABF.mBusinessFlow.TargetApplications.Count() == 0)
            {
                if (ABF.mSolution.ApplicationPlatforms.Count == 0)
                {                    
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, "The default Application Platform Info is missing, please go to Solution level to add at least one Target Application.");
                    return;
                }
                string SAN = ABF.mSolution.ApplicationPlatforms[0].AppName;
                ABF.mBusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = SAN });
                ABF.Status = eStatus.Fixed;
            }
        }        
    }
}
