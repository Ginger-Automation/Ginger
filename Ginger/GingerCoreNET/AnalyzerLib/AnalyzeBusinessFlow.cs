#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.AnalyzerLib;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.Variables;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Platforms;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#nullable enable
namespace Ginger.AnalyzerLib
{
    public class AnalyzeBusinessFlow : AnalyzerItemBase
    {
        public enum Check : uint
        {
            All = ~0u,
            None = 0u,
            MissingTargetApplications = 1 << 0,
            NoActivities = 1 << 1,
            MissingMandatoryInputValues = 1 << 2,
            LegacyOutputValidation = 1 << 3
        }

        private static readonly string MissingTargetApp = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} is missing {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}";
        private static readonly string MissingActivities = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " is missing " + GingerDicser.GetTermResValue(eTermResKey.Activities);
        public static readonly string LegacyOutPutValidationDescription = "Output validation contains legacy operators which are very slow and does not work on Linux";

        private static Regex rxVarPattern = new(@"{(\bVar Name=)\w+\b[^{}]*}", RegexOptions.Compiled);

        public BusinessFlow BusinessFlow { get; set; }
        public Solution Solution { get; set; }

        public List<ActReturnValue> ReturnValues { get; set; } = [];

        public static List<AnalyzerItemBase> Analyze(BusinessFlow businessFlow)
        {
            return Analyze(businessFlow, Check.All);
        }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow businessFlow, Check checks)
        {
            List<AnalyzerItemBase> issues = [.. AnalyzeIndependently(businessFlow, checks)];
            AnalyzeEnvApplication.AnalyzeEnvAppInBusinessFlows(businessFlow, issues);
            AnalyzeValueExpInBusinessFlow(businessFlow, ref issues);
            return issues;
        }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow businessFlow, Solution solution)
        {
            return Analyze(businessFlow, solution, Check.All);
        }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow businessFlow, Solution solution, Check checks)
        {
            List<AnalyzerItemBase> issues =
            [
                .. AnalyzeWithSolutionDependency(businessFlow, solution, checks),
                .. AnalyzeIndependently(businessFlow, checks),
            ];
            AnalyzeEnvApplication.AnalyzeEnvAppInBusinessFlows(businessFlow, issues);
            AnalyzeValueExpInBusinessFlow(businessFlow, ref issues);
            return issues;
        }

        private static List<AnalyzerItemBase> AnalyzeWithSolutionDependency(BusinessFlow businessFlow, Solution solution, Check checks)
        {
            List<AnalyzerItemBase> issues = [];

            if (checks.AreAllFlagsSet(Check.MissingTargetApplications) && HasMissingTargetApplications(businessFlow, solution, out AnalyzeBusinessFlow issue))
            {
                issues.Add(issue);
            }

            return issues;
        }

        private static List<AnalyzerItemBase> AnalyzeIndependently(BusinessFlow businessFlow, Check checks)
        {
            List<AnalyzerItemBase> issues = [];

            if (checks.AreAllFlagsSet(Check.NoActivities) && HasNoActivities(businessFlow, out AnalyzeBusinessFlow issue))
            {
                issues.Add(issue);
            }

            if (checks.AreAllFlagsSet(Check.MissingMandatoryInputValues) && HasMissingMandatoryInputValues(businessFlow, out List<AnalyzeBusinessFlow> issueList))
            {
                issues.AddRange(issueList);
            }

            if (checks.AreAllFlagsSet(Check.LegacyOutputValidation) && HasLegacyOutputValidations(businessFlow, out issue))
            {
                issues.Add(issue);
            }

            if (HasInvalidInputValueRules(businessFlow, out issueList))
            {
                issues.AddRange(issueList);
            }

            return issues;
        }



        private static bool HasMissingTargetApplications(BusinessFlow businessFlow, Solution solution, out AnalyzeBusinessFlow issue)
        {
            if (businessFlow.TargetApplications == null || !businessFlow.TargetApplications.Any())
            {
                issue = new()
                {
                    Description = MissingTargetApp,
                    UTDescription = "MissingTargetApp",
                    Details = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} doesn't have {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} defined",
                    HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " in solution tab and add target apps",
                    CanAutoFix = eCanFix.Yes,   //  take it from solution 
                    FixItHandler = FixMissingTargetApp,
                    Status = eStatus.NeedFix,
                    IssueType = eType.Error,
                    ItemParent = "NA",
                    BusinessFlow = businessFlow,
                    ItemName = businessFlow.Name,
                    Impact = $"Might be executed on wrong application if solution have more than 1 {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}",
                    Solution = solution,
                    ItemClass = nameof(GingerCore.BusinessFlow),
                    Severity = eSeverity.High,
                    Selected = true
                };
                return true;
            }
            else
            {
                issue = null!;
                return false;
            }
        }

        private static bool HasNoActivities(BusinessFlow businessFlow, out AnalyzeBusinessFlow issue)
        {
            if (!businessFlow.Activities.Any())
            {
                issue = new()
                {
                    UTDescription = "MissingActivities",
                    Description = MissingActivities,
                    Details = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " doesn't have " + GingerDicser.GetTermResValue(eTermResKey.Activities),
                    HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " and add " + GingerDicser.GetTermResValue(eTermResKey.Activities) + " or remove this " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow),
                    CanAutoFix = eCanFix.No,    // we can autofix by delete, but don't want to
                    Status = eStatus.NeedFix,

                    IssueType = eType.Warning,
                    ItemParent = "NA",
                    BusinessFlow = businessFlow,
                    Severity = eSeverity.Medium,
                    ItemClass = "BusinessFlow"
                };
                return true;
            }
            else
            {
                issue = null!;
                return false;
            }
        }

        public static bool HasMissingMandatoryInputValues(BusinessFlow businessFlow, out List<AnalyzeBusinessFlow> issueList)
        {
            issueList = [];
            foreach (VariableBase var in businessFlow.GetBFandActivitiesVariabeles(includeParentDetails: true, includeOnlySetAsInputValue: true, includeOnlyMandatoryInputs: true))
            {
                if (var.SetAsInputValue && var.MandatoryInput && string.IsNullOrWhiteSpace(var.Value) && var.MappedOutputType == VariableBase.eOutputType.None)
                {
                    AnalyzeBusinessFlow mandatoryInputIssue = new()
                    {
                        Description = string.Format("Mandatory Input {0} is missing a value", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                        UTDescription = "MissingMandatoryInputValue",
                        Details = string.Format("The {0} '{1}' was marked as 'Mandatory Input' but it has empty value and no dynamic mapped value.", GingerDicser.GetTermResValue(eTermResKey.Variable), var.Name),
                        HowToFix = string.Format("Set a value to the {0} before starting the execution.", GingerDicser.GetTermResValue(eTermResKey.Variable)),
                        CanAutoFix = eCanFix.No,
                        FixItHandler = null,
                        Status = eStatus.NeedFix,
                        IssueType = eType.Error,
                        ItemParent = var.ParentName,
                        BusinessFlow = businessFlow,
                        ItemName = var.Name,
                        Impact = "Execution probably will fail due to missing input value.",
                        Solution = WorkSpace.Instance.Solution,
                        ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable),
                        Severity = eSeverity.High,
                        Selected = true
                    };
                    issueList.Add(mandatoryInputIssue);
                }
            }

            return issueList.Any();
        }

        private static bool HasLegacyOutputValidations(BusinessFlow businessFlow, out AnalyzeBusinessFlow issue)
        {
            issue = new()
            {
                ReturnValues = []
            };

            IEnumerable<IAct> actions = businessFlow.Activities.SelectMany(x => x.Acts);

            foreach (Act action in actions.Cast<Act>())
            {
                issue.ReturnValues.AddRange(action.ActReturnValues.Where(returnValue => returnValue.Operator == Amdocs.Ginger.Common.Expressions.eOperator.Legacy));
            }

            if (issue.ReturnValues.Any())
            {
                issue.ItemName = businessFlow.Name;
                issue.Description = LegacyOutPutValidationDescription;
                issue.IssueType = eType.Warning;
                issue.BusinessFlow = businessFlow;
                issue.Severity = eSeverity.Medium;
                issue.ItemClass = "BusinessFlow";
                issue.CanAutoFix = eCanFix.Maybe;
                issue.Status = eStatus.NeedFix;
                issue.FixItHandler = FixOutputValidationIssue;
                return true;
            }
            else
            {
                issue = null!;
                return false;
            }
        }

        public static bool HasInvalidInputValueRules(BusinessFlow businessFlow, out List<AnalyzeBusinessFlow> issueList)
        {
            issueList = [];

            ObservableList<VariableBase> bfInputVariables = businessFlow.GetBFandActivitiesVariabeles(includeParentDetails: true, includeOnlySetAsInputValue: true);

            foreach (InputVariableRule rule in businessFlow.InputVariableRules)
            {
                if (rule.Active)
                {
                    if (HasInvalidSourceVariableInRule(businessFlow, bfInputVariables, rule, out AnalyzeBusinessFlow issue))
                    {
                        issueList.Add(issue);
                    }
                    if (HasInvalidTargetVariableInRule(businessFlow, bfInputVariables, rule, out issue))
                    {
                        issueList.Add(issue);
                    }
                }
            }

            if (issueList.Any())
            {
                return true;
            }
            else
            {
                issueList = null!;
                return false;
            }
        }

        private static bool HasInvalidSourceVariableInRule(BusinessFlow businessFlow, IEnumerable<VariableBase> businessFlowInputVariables, InputVariableRule rule, out AnalyzeBusinessFlow issue)
        {
            VariableBase? sourceVariable = businessFlowInputVariables.FirstOrDefault(v => v.Guid == rule.SourceVariableGuid);

            if (sourceVariable == null)
            {
                issue = new()
                {
                    Description = $"The 'Source {GingerDicser.GetTermResValue(eTermResKey.Variable)}' is missing in Input Variable Rule",
                    UTDescription = "MissingVariable",
                    Details = $"The 'Source {GingerDicser.GetTermResValue(eTermResKey.Variable)}' does not exist In Input Variable Rule",
                    HowToFix = $" Create new {GingerDicser.GetTermResValue(eTermResKey.Variable)} or delete it from the input variable rule",
                    CanAutoFix = eCanFix.No,
                    IssueType = eType.Error,
                    Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)}",
                    Severity = eSeverity.High,
                    IssueCategory = eIssueCategory.MissingVariable,
                    ItemParent = businessFlow.Name,
                    ItemClass = "InputVariableRule",
                    Status = eStatus.NeedFix
                };

                return true;
            }
            else if (sourceVariable is not null and VariableSelectionList sourceVariableSelectionList)
            {
                if (!sourceVariableSelectionList.OptionalValuesList.Any(x => x.Value == rule.TriggerValue))
                {
                    issue = new()
                    {
                        Description = $"The 'Source {GingerDicser.GetTermResValue(eTermResKey.Variable)}' trigger value used in Input Variable Rule is  missing in Selection List {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {sourceVariable.Name}",
                        UTDescription = "MissingVariableValue",
                        Details = $"The 'Source {GingerDicser.GetTermResValue(eTermResKey.Variable)} trigger value used in Input Variable Rule is  missing in Selection List {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {sourceVariable.Name}",
                        HowToFix = $"Change 'Source {GingerDicser.GetTermResValue(eTermResKey.Variable)} trigger value in Input Variable Rule",
                        CanAutoFix = eCanFix.No,
                        IssueType = eType.Error,
                        Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)} value",
                        Severity = eSeverity.High,
                        IssueCategory = eIssueCategory.MissingVariable,
                        ItemParent = businessFlow.Name,
                        ItemClass = "InputVariableRule",
                        Status = eStatus.NeedFix
                    };
                    return true;
                }
                issue = null!;
                return false;
            }
            else
            {
                issue = null!;
                return false;
            }
        }

        private static bool HasInvalidTargetVariableInRule(BusinessFlow businessFlow, IEnumerable<VariableBase> businessFlowInputVariables, InputVariableRule rule, out AnalyzeBusinessFlow issue)
        {
            VariableBase? targetVariable = businessFlowInputVariables.FirstOrDefault(v => v.Guid == rule.TargetVariableGuid);

            if (targetVariable == null)
            {
                issue = new()
                {
                    Description = $"The 'Target {GingerDicser.GetTermResValue(eTermResKey.Variable)}' is missing in Input Variable Rule",
                    UTDescription = "MissingVariable",
                    Details = $"The 'Target {GingerDicser.GetTermResValue(eTermResKey.Variable)}' does not exist In Input Variable Rule",
                    HowToFix = $" Create new {GingerDicser.GetTermResValue(eTermResKey.Variable)} or delete it from the input variable rule",
                    CanAutoFix = eCanFix.No,
                    IssueType = eType.Error,
                    Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)}",
                    Severity = eSeverity.High,
                    IssueCategory = eIssueCategory.MissingVariable,
                    ItemParent = businessFlow.Name,
                    ItemClass = "InputVariableRule",
                    Status = eStatus.NeedFix
                };
                return true;
            }


            else if (targetVariable is VariableSelectionList targetVariableSelectionList)
            {
                if (rule.OperationType is InputVariableRule.eInputVariableOperation.SetValue or InputVariableRule.eInputVariableOperation.SetOptionalValues)
                {
                    if (!targetVariableSelectionList.OptionalValuesList.Any(x => string.Equals(x.Value, rule.OperationValue)))
                    {
                        issue = new()
                        {
                            Description = $"The 'Target {GingerDicser.GetTermResValue(eTermResKey.Variable)}' trigger value used in Input Variable Rule is  missing in selection list {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {targetVariable.Name}",
                            UTDescription = "MissingVariableValue",
                            Details = $"The 'Target {GingerDicser.GetTermResValue(eTermResKey.Variable)}' trigger value used in Input Variable Rule is missing in selection list {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {targetVariable.Name}",
                            HowToFix = $"Change 'Target {GingerDicser.GetTermResValue(eTermResKey.Variable)}' trigger value in Input Variable Rule",
                            CanAutoFix = eCanFix.No,
                            IssueType = eType.Error,
                            Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)} Value",
                            Severity = eSeverity.High,
                            IssueCategory = eIssueCategory.MissingVariable,
                            ItemParent = businessFlow.Name,
                            ItemClass = "InputVariableRule",
                            Status = eStatus.NeedFix
                        };
                        return true;
                    }
                }
                issue = null!;
                return false;
            }
            else
            {
                issue = null!;
                return false;
            }
        }




        private static void FixOutputValidationIssue(object? sender, EventArgs e)
        {
            AnalyzeBusinessFlow ABF = (AnalyzeBusinessFlow)sender;

            int issueCount = ABF.ReturnValues.Count;
            int FixedIssueCount = 0;
            foreach (ActReturnValue ARV in ABF.ReturnValues)
            {
                if (!string.IsNullOrEmpty(ARV.Expected) && !ARV.Expected.Contains("{"))
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
                                FixedIssueCount++;
                                ARV.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
                            }
                        }


                    }
                }
            }

            if (FixedIssueCount == 0)
            {
                ABF.Status = eStatus.CannotFix;
            }
            else if (FixedIssueCount < issueCount)
            {
                ABF.Status = eStatus.PartiallyFixed;
            }
            else
            {
                ABF.Status = eStatus.Fixed;
            }
        }

        private static AnalyzeBusinessFlow GetOutputvalidationErros(BusinessFlow businessFlow)
        {
            AnalyzeBusinessFlow ABF = new AnalyzeBusinessFlow();

            IEnumerable<ObservableList<IAct>> ActionList = businessFlow.Activities.Select(x => x.Acts);

            foreach (ObservableList<IAct> ActList in ActionList)
            {
                foreach (Act action in ActList)
                {
                    ABF.ReturnValues.AddRange(action.ActReturnValues.Where(x => x.Operator == Amdocs.Ginger.Common.Expressions.eOperator.Legacy));
                }
            }
            return ABF;
        }

        private static void FixMissingTargetApp(object? sender, EventArgs e)
        {
            // Take the target app from the solution
            AnalyzeBusinessFlow ABF = (AnalyzeBusinessFlow)sender;
            // Do recheck
            if (!ABF.BusinessFlow.TargetApplications.Any())
            {
                if (!ABF.Solution.ApplicationPlatforms.Any())
                {
                    Reporter.ToUser(eUserMsgKey.MissingTargetApplication, $"The default Application Platform Info is missing, please go to Solution level to add at least one {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)}");
                    return;
                }
                string SAN = ABF.Solution.ApplicationPlatforms[0].AppName;
                ABF.BusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = SAN });
                ABF.Status = eStatus.Fixed;
            }
        }

        public static void AnalyzeValueExpInBusinessFlow(BusinessFlow businessFlow, ref List<AnalyzerItemBase> issuesList)
        {
            if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(businessFlow.RunDescription, businessFlow.Environment))
            {

                AnalyzeBusinessFlow issue = new()
                {
                    Description = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} RunDescription value expression does not exist in the Current Environment",
                    UTDescription = "MissingParameterInCurrentEnvironment",
                    Details = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} RunDescription value expression does not exist in the Current Environment",
                    CanAutoFix = eCanFix.No,
                    IssueType = eType.Error,
                    Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)} in Environment",
                    Severity = eSeverity.High,
                    IssueCategory = eIssueCategory.MissingVariable,
                    ItemParent = businessFlow.Name,
                    ItemClass = "BusinessFlow",
                    Status = eStatus.NeedFix
                };

                issuesList.Add(issue);
            }

            var FilteredVariables = businessFlow.Variables
                .Where((variable) =>
            {
                return variable is VariableDynamic variableDynamic && !AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(variableDynamic.ValueExpression, businessFlow.Environment);
            });


            foreach (var FilteredVariable in FilteredVariables)
            {

                AnalyzeBusinessFlow issue = new()
                {
                    Description = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {FilteredVariable.Name} value does not exist in the Current Environment",
                    UTDescription = "MissingVariableInCurrentEnvironment",
                    Details = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} {GingerDicser.GetTermResValue(eTermResKey.Variable)}: {FilteredVariable.Name} value does not exist in the Current Environment",
                    CanAutoFix = eCanFix.No,
                    IssueType = eType.Error,
                    Impact = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} will fail due to missing {GingerDicser.GetTermResValue(eTermResKey.Variable)} in Environment",
                    Severity = eSeverity.High,
                    IssueCategory = eIssueCategory.MissingVariable,
                    ItemParent = businessFlow.Name,
                    ItemClass = "VariableDynamic",
                    Status = eStatus.NeedFix
                };
                issuesList.Add(issue);
            }


        }
    }
}
