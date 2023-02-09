#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.AnalyzerLib
{
    public class AnalyzerUtils
    {
        private readonly object mAddIssuesLock = new object();
        private readonly object mUsedVariableLock = new object();
        private readonly object mMissingVariableIssueLock = new object();

        public bool SelfHealingAutoFixIssue { get; set; }

        public void RunSolutionAnalyzer(Solution solution, ObservableList<AnalyzerItemBase> issuesList)
        {
            foreach (AnalyzerItemBase issue in AnalyzeSolution.Analyze(solution))
            {
                AddIssue(issuesList, issue);
            }

            //TODO: once this analyzer is taking long time due to many checks, run it using parallel
            ObservableList<BusinessFlow> BFs = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            List<string> usedVariablesInSolution = new List<string>();

            //foreach (BusinessFlow BF in BFs)
            Parallel.ForEach(BFs, new ParallelOptions { MaxDegreeOfParallelism = 5 }, BF =>
            {
                List<string> tempList = RunBusinessFlowAnalyzer(BF, issuesList);
                MergeVariablesList(usedVariablesInSolution, tempList);
            });
            ReportUnusedVariables(solution, usedVariablesInSolution, issuesList);
        }

        public void RunRunSetConfigAnalyzer(RunSetConfig mRunSetConfig, ObservableList<AnalyzerItemBase> issuesList)
        {
            foreach (AnalyzerItemBase issue in RunSetConfigAnalyzer.Analyze(mRunSetConfig))
            {
                AddIssue(issuesList, issue);
            }

            // Check all GRs BFS
            //foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
            List<Guid> checkedGuidList = new List<Guid>();
            Parallel.ForEach(mRunSetConfig.GingerRunners, new ParallelOptions { MaxDegreeOfParallelism = 5 }, GR =>
            {
                foreach (AnalyzerItemBase issue in AnalyzeGingerRunner.Analyze(GR, WorkSpace.Instance.Solution.ApplicationPlatforms))
                {
                    AddIssue(issuesList, issue);
                }

                //Code to analyze Runner Unique Businessflow with Source BF
                Parallel.ForEach(GR.Executor.BusinessFlows, new ParallelOptions { MaxDegreeOfParallelism = 5 }, BF =>
                {
                    if (!checkedGuidList.Contains(BF.Guid))//check if it already was analyzed
                    {
                        checkedGuidList.Add(BF.Guid);
                        BusinessFlow actualBf = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>().Where(x => x.Guid == BF.Guid).FirstOrDefault();
                        if (actualBf != null)
                        {
                            RunBusinessFlowAnalyzer(actualBf, issuesList, includeMandatoryInputsAnalyze:false);
                        }
                    }
                });

                //Code to analyze Runner BF i.e. BFFlowControls
                //foreach (BusinessFlow BF in GR.BusinessFlows)
                Parallel.ForEach(GR.Executor.BusinessFlows, new ParallelOptions { MaxDegreeOfParallelism = 5 }, BF =>
                {
                    foreach (AnalyzerItemBase issue in AnalyzeRunnerBusinessFlow.Analyze((GingerExecutionEngine)GR.Executor, BF))
                    {
                        AddIssue(issuesList, issue);
                    }
                });
            });
        }

        public List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, ObservableList<AnalyzerItemBase> issuesList, bool includeMandatoryInputsAnalyze = true)
        {
            List<string> usedVariablesInBF = new List<string>();
            List<string> usedVariablesInActivity = new List<string>();
            List<AnalyzerItemBase> missingVariableIssueList = new List<AnalyzerItemBase>();

            ObservableList<DataSourceBase> DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            foreach (AnalyzerItemBase issue in AnalyzeBusinessFlow.Analyze(WorkSpace.Instance.Solution, businessFlow, includeMandatoryInputsAnalyze: includeMandatoryInputsAnalyze))
            {
                AddIssue(issuesList, issue);
            }

            Parallel.ForEach(businessFlow.Activities, new ParallelOptions { MaxDegreeOfParallelism = 5 }, activity =>
            {
                if (activity.Active)
                {
                    foreach (AnalyzerItemBase issue in AnalyzeActivity.Analyze(businessFlow, activity))
                    {
                        AddIssue(issuesList, issue);
                    }
                }

                Parallel.ForEach(activity.Acts, new ParallelOptions { MaxDegreeOfParallelism = 5 }, iaction =>
                {

                    Act action = (Act)iaction;
                    if (action.Active)
                    {
                        foreach (AnalyzerItemBase issue in AnalyzeAction.Analyze(businessFlow, activity, action, DSList))
                        {
                            AddIssue(issuesList, issue);
                            if (issue.IssueCategory == AnalyzerItemBase.eIssueCategory.MissingVariable)
                            {
                                lock (mMissingVariableIssueLock)
                                {
                                    missingVariableIssueList.Add(issue);
                                }
                            }

                        }

                    }

                    List<string> tempList = AnalyzeAction.GetUsedVariableFromAction(action);
                    MergeVariablesList(usedVariablesInActivity, tempList);
                });

                List<string> activityVarList = AnalyzeActivity.GetUsedVariableFromActivity(activity);

                MergeVariablesList(usedVariablesInActivity, activityVarList);
                ReportUnusedVariables(activity, usedVariablesInActivity, issuesList);
                MergeVariablesList(usedVariablesInBF, usedVariablesInActivity);


                usedVariablesInActivity.Clear();
            });

            //Get all the missing variable issues Grouped by Variable name
            lock (mMissingVariableIssueLock)
            {
                if (missingVariableIssueList.Count != 0)
                {
                    var missingVariableIssuesGroupList = missingVariableIssueList.GroupBy(x => x.IssueReferenceObject);

                    foreach (var variableIssueGroup in missingVariableIssuesGroupList)
                    {
                        //If for specific variable, all the issues are for set variable action then we support Auto Fix
                        var canAutoFix = variableIssueGroup.All(x => x is AnalyzeAction && ((AnalyzeAction)x).mAction.GetType() == typeof(ActSetVariableValue));

                        if (canAutoFix)
                        {
                            foreach (AnalyzeAction issue in variableIssueGroup)
                            {
                                issue.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                                issue.FixItHandler += MarkSetVariableActionAsInactive;
                            }
                        }

                    }
                }

            }

            ReportUnusedVariables(businessFlow, usedVariablesInBF, issuesList);
            return usedVariablesInBF;
        }

        public void ReportUnusedVariables(object obj, List<string> usedVariables, ObservableList<AnalyzerItemBase> issuesList)
        {
            Solution solution = null;
            BusinessFlow businessFlow = null;
            Activity activity = null;
            string variableSourceType = "";
            string variableSourceName = "";
            ObservableList<VariableBase> AvailableAllVariables = new ObservableList<VariableBase>();
            if (typeof(BusinessFlow).Equals(obj.GetType()))
            {
                businessFlow = (BusinessFlow)obj;
                if (businessFlow.Variables.Count > 0)
                {
                    AvailableAllVariables = businessFlow.Variables;
                    variableSourceType = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                    variableSourceName = businessFlow.Name;
                }
            }
            else if (typeof(Activity).Equals(obj.GetType()))
            {
                activity = (Activity)obj;
                if (activity.Variables.Count > 0)
                {
                    AvailableAllVariables = activity.Variables;
                    variableSourceType = GingerDicser.GetTermResValue(eTermResKey.Activity);
                    variableSourceName = activity.ActivityName;
                }
            }
            else if (typeof(Solution).Equals(obj.GetType()))
            {
                solution = (Solution)obj;
                AvailableAllVariables = solution.Variables;
                variableSourceType = "Solution";
                variableSourceName = solution.Name;
            }

            foreach (VariableBase var in AvailableAllVariables)
            {
                if (usedVariables != null && (!usedVariables.Contains(var.Name)))
                {
                    if (obj.GetType().Equals(typeof(BusinessFlow)))
                    {
                        AnalyzeBusinessFlow aa = new AnalyzeBusinessFlow();
                        aa.Status = AnalyzerItemBase.eStatus.NeedFix;
                        aa.ItemName = var.Name;
                        aa.Description = var + " is Unused in " + variableSourceType + ": " + businessFlow.Name;
                        aa.Details = variableSourceType;
                        aa.mBusinessFlow = businessFlow;
                        aa.ItemParent = variableSourceName;
                        aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                        aa.IssueType = AnalyzerItemBase.eType.Error;
                        aa.FixItHandler = DeleteUnusedVariables;
                        aa.Severity = AnalyzerItemBase.eSeverity.Medium;
                        aa.ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable);
                        AddIssue(issuesList, aa);
                    }
                    else if (obj.GetType().Equals(typeof(Solution)))
                    {
                        AnalyzeSolution aa = new AnalyzeSolution();
                        aa.Status = AnalyzerItemBase.eStatus.NeedFix;
                        aa.ItemName = var.Name;
                        aa.Description = var + " is Unused in Solution";
                        aa.ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable);
                        aa.Details = variableSourceType;
                        aa.ItemParent = variableSourceName;
                        aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                        aa.IssueType = AnalyzerItemBase.eType.Error;
                        aa.FixItHandler = DeleteUnusedVariables;
                        aa.Severity = AnalyzerItemBase.eSeverity.Medium;
                        AddIssue(issuesList, aa);
                    }
                    else
                    {
                        AnalyzeActivity aa = new AnalyzeActivity();
                        aa.Status = AnalyzerItemBase.eStatus.NeedFix;
                        aa.ItemName = var.Name;
                        aa.Description = var + " is Unused in " + variableSourceType + ": " + activity.ActivityName;
                        aa.Details = variableSourceType;
                        aa.mActivity = activity;
                        aa.ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable);
                        //aa.mBusinessFlow = businessFlow;
                        aa.ItemParent = variableSourceName;
                        aa.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                        aa.IssueType = AnalyzerItemBase.eType.Error;
                        aa.FixItHandler = DeleteUnusedVariables;
                        aa.Severity = AnalyzerItemBase.eSeverity.Medium;
                        AddIssue(issuesList, aa);
                    }
                }
            }
        }

        private static void DeleteUnusedVariables(object sender, EventArgs e)
        {
            if (sender.GetType().Equals(typeof(AnalyzeActivity)))
            {
                Activity activity = ((AnalyzeActivity)sender).mActivity;
                foreach (VariableBase var in activity.Variables)
                {
                    if (var.Name.Equals(((AnalyzeActivity)sender).ItemName))
                    {
                        activity.Variables.Remove(var);
                        activity.RefreshVariablesNames();
                        ((AnalyzeActivity)sender).Status = AnalyzerItemBase.eStatus.Fixed;
                        break;
                    }
                }
            }
            else if (sender.GetType().Equals(typeof(AnalyzeBusinessFlow)))
            {
                BusinessFlow BFlow = ((AnalyzeBusinessFlow)sender).mBusinessFlow;
                foreach (VariableBase var in BFlow.Variables)
                {
                    if (var.Name.Equals(((AnalyzeBusinessFlow)sender).ItemName))
                    {
                        BFlow.Variables.Remove(var);
                        ((AnalyzeBusinessFlow)sender).Status = AnalyzerItemBase.eStatus.Fixed;
                        break;
                    }
                }
            }
            else if (sender.GetType().Equals(typeof(AnalyzeSolution)))
            {
                foreach (VariableBase var in BusinessFlow.SolutionVariables)
                {
                    if (var.Name.Equals(((AnalyzeSolution)sender).ItemName))
                    {
                        BusinessFlow.SolutionVariables.Remove(var);
                        ((AnalyzeSolution)sender).Status = AnalyzerItemBase.eStatus.Fixed;
                        break;
                    }
                }
            }
        }

        private static void MarkSetVariableActionAsInactive(object sender, EventArgs e)
        {
            Act action = ((AnalyzeAction)sender).mAction;
            action.Active = false;
            ((AnalyzeAction)sender).Status = AnalyzerItemBase.eStatus.Fixed;
        }

        private void AddIssue(ObservableList<AnalyzerItemBase> issuesList, AnalyzerItemBase issue)
        {
            lock (mAddIssuesLock)
            {
                issuesList.Add(issue);
            }
        }


        private void MergeVariablesList(List<string> sourceList, List<string> listToMerge)
        {
            lock (mUsedVariableLock)
            {
                sourceList.AddRange(listToMerge);
            }
        }
        /// <summary>
        /// Run Runset Analyzer process and return true in case High+ issues were found, also allows to write the High+ found issues to log/console
        /// </summary>
        /// <param name="runset"></param>
        /// <param name="reportIssues"></param>
        /// <returns></returns>
        public bool AnalyzeRunset(RunSetConfig runset, bool reportIssues)
        {
            ObservableList<AnalyzerItemBase> issues = new ObservableList<AnalyzerItemBase>();
            RunRunSetConfigAnalyzer(runset, issues);

            List<AnalyzerItemBase> highCriticalIssues = issues.Where(x => x.Severity == AnalyzerItemBase.eSeverity.High || x.Severity == AnalyzerItemBase.eSeverity.Critical).ToList();
            if (highCriticalIssues != null && highCriticalIssues.Count > 0)
            {
                if (reportIssues)
                {
                    foreach (AnalyzerItemBase issue in highCriticalIssues)
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("Analyzer High+ Issue found: {0}/{1}[{2}] => {3}", issue.ItemParent, issue.ItemName, issue.ItemClass, issue.Description));
                    }
                }

                return true;//High+ issues exist
            }
            else
            {
                return false;
            }
        }
    }
}
