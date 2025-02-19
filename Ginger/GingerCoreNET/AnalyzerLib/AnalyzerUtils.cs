#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Actions;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
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
            List<string> usedVariablesInSolution = [];

            //foreach (BusinessFlow BF in BFs)
            Parallel.ForEach(BFs, new ParallelOptions { MaxDegreeOfParallelism = 5 }, BF =>
            {
                MergeVariablesList(usedVariablesInSolution, RunBusinessFlowAnalyzer(BF, issuesList));
            });
            ReportUnusedVariables(solution, usedVariablesInSolution, issuesList);
        }

        public void RunRunSetConfigAnalyzer(RunSetConfig runsetConfig, RunSetConfigAnalyzer.Check runsetAnalyzerChecks, AnalyzeGingerRunner.Check runnerAnalyzerChecks, ObservableList<AnalyzerItemBase> issuesList)
        {
            RunRunSetConfigAnalyzer(runsetConfig, runsetAnalyzerChecks, runnerAnalyzerChecks, solution: null, issuesList);
        }

        public void RunRunSetConfigAnalyzer(RunSetConfig mRunSetConfig, RunSetConfigAnalyzer.Check runsetAnalyzerChecks, AnalyzeGingerRunner.Check runnerAnalyzerChecks, Solution solution, ObservableList<AnalyzerItemBase> issuesList)
        {
            foreach (AnalyzerItemBase issue in RunSetConfigAnalyzer.Analyze(mRunSetConfig, solution, runsetAnalyzerChecks))
            {
                AddIssue(issuesList, issue);
            }

            // Check all GRs BFS
            //foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
            List<Guid> checkedGuidList = [];
            Parallel.ForEach(mRunSetConfig.GingerRunners, new ParallelOptions { MaxDegreeOfParallelism = 5 }, GR =>
            {
                foreach (AnalyzerItemBase issue in AnalyzeGingerRunner.Analyze(GR, runnerAnalyzerChecks))
                {
                    AddIssue(issuesList, issue);
                }

                if (GR.Executor == null)
                {
                    return;
                }

                //Code to analyze Runner Unique Businessflow with Source BF
                Parallel.ForEach(GR.Executor.BusinessFlows, new ParallelOptions { MaxDegreeOfParallelism = 5 }, BF =>
                {
                    if (!checkedGuidList.Contains(BF.Guid))//check if it already was analyzed
                    {

                        checkedGuidList.Add(BF.Guid);
                        BusinessFlow actualBf = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<BusinessFlow>(BF.Guid);
                        actualBf.Environment = GR.SpecificEnvironmentName ?? WorkSpace.Instance.RunsetExecutor.RunsetExecutionEnvironment.Name;

                        if (actualBf != null)
                        {
                            IEnumerable<IApplicationAgent> applicationAgents = ((GingerExecutionEngine)GR.Executor).GingerRunner.ApplicationAgents;
                            if (applicationAgents == null)
                            {
                                applicationAgents = [];
                            }
                            RunBusinessFlowAnalyzer(actualBf, applicationAgents, solution, issuesList, AnalyzeBusinessFlow.Check.All.ExcludeFlags(AnalyzeBusinessFlow.Check.MissingMandatoryInputValues));
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


        public List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, Solution solution, ObservableList<AnalyzerItemBase> issuesList)
        {
            return RunBusinessFlowAnalyzer(businessFlow, applicationAgents: Array.Empty<IApplicationAgent>(), solution, issuesList, AnalyzeBusinessFlow.Check.All);
        }

        public List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, IEnumerable<IApplicationAgent> applicationAgents, Solution solution, ObservableList<AnalyzerItemBase> issuesList)
        {
            return RunBusinessFlowAnalyzer(businessFlow, applicationAgents, solution, issuesList, AnalyzeBusinessFlow.Check.All);
        }

        public List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, ObservableList<AnalyzerItemBase> issuesList)
        {
            return RunBusinessFlowAnalyzer(businessFlow, issuesList, AnalyzeBusinessFlow.Check.All);
        }

        public List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, ObservableList<AnalyzerItemBase> issuesList, AnalyzeBusinessFlow.Check checks)
        {
            return RunBusinessFlowAnalyzer(businessFlow, applicationAgents: Array.Empty<IApplicationAgent>(), solution: null, issuesList, checks);
        }

        private List<string> RunBusinessFlowAnalyzer(BusinessFlow businessFlow, IEnumerable<IApplicationAgent> applicationAgents,
            Solution solution, ObservableList<AnalyzerItemBase> issuesList, AnalyzeBusinessFlow.Check checks)
        {
            List<string> usedVariablesInBF = [];
            List<string> usedVariablesInActivity = [];
            List<AnalyzerItemBase> missingVariableIssueList = [];

            ObservableList<DataSourceBase> DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            if (solution == null)
            {
                foreach (AnalyzerItemBase issue in AnalyzeBusinessFlow.Analyze(businessFlow, checks))
                {
                    AddIssue(issuesList, issue);
                }
            }
            else
            {
                foreach (AnalyzerItemBase issue in AnalyzeBusinessFlow.Analyze(businessFlow, solution, checks))
                {
                    AddIssue(issuesList, issue);
                }
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

                    string actionDescription = iaction.Description;
                    IApplicationAgent appAgent = applicationAgents.FirstOrDefault(a => string.Equals(activity.TargetApplication, a.AppName));
                    DriverBase driver = null;
                    if (appAgent != null)
                    {
                        driver = GetDriverForApplicationAgent(appAgent);
                    }
                    Act action = (Act)iaction;
                    if (action.Active)
                    {
                        foreach (AnalyzerItemBase issue in AnalyzeAction.Analyze(businessFlow, activity, action, DSList, driver))
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

                    MergeVariablesList(usedVariablesInActivity, AnalyzeAction.GetUsedVariableFromAction(action));
                });


                MergeVariablesList(usedVariablesInActivity, AnalyzeActivity.GetUsedVariableFromActivity(activity));

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

        private DriverBase GetDriverForApplicationAgent(IApplicationAgent appAgent)
        {
            if (appAgent == null)
            {
                throw new ArgumentNullException(nameof(appAgent));
            }

            if (appAgent.Agent is not Agent agent)
            {
                return null;
            }

            if (agent.AgentOperations == null)
            {
                agent.AgentOperations = new AgentOperations(agent);
            }

            try
            {
                return ((AgentOperations)agent.AgentOperations).CreateDriverInstance();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void ReportUnusedVariables(object obj, List<string> usedVariables, ObservableList<AnalyzerItemBase> issuesList)
        {
            Solution solution = null;
            BusinessFlow businessFlow = null;
            Activity activity = null;
            string variableSourceType = "";
            string variableSourceName = "";
            ObservableList<VariableBase> AvailableAllVariables = [];
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
                        AnalyzeBusinessFlow aa = new AnalyzeBusinessFlow
                        {
                            Status = AnalyzerItemBase.eStatus.NeedFix,
                            ItemName = var.Name,
                            Description = var + " is Unused in " + variableSourceType + ": " + businessFlow.Name,
                            Details = variableSourceType,
                            BusinessFlow = businessFlow,
                            ItemParent = variableSourceName,
                            CanAutoFix = AnalyzerItemBase.eCanFix.Yes,
                            IssueType = AnalyzerItemBase.eType.Error,
                            FixItHandler = DeleteUnusedVariables,
                            Severity = AnalyzerItemBase.eSeverity.Medium,
                            ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable)
                        };
                        AddIssue(issuesList, aa);
                    }
                    else if (obj.GetType().Equals(typeof(Solution)))
                    {
                        AnalyzeSolution aa = new AnalyzeSolution
                        {
                            Status = AnalyzerItemBase.eStatus.NeedFix,
                            ItemName = var.Name,
                            Description = var + " is Unused in Solution",
                            ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable),
                            Details = variableSourceType,
                            ItemParent = variableSourceName,
                            CanAutoFix = AnalyzerItemBase.eCanFix.Yes,
                            IssueType = AnalyzerItemBase.eType.Error,
                            FixItHandler = DeleteUnusedVariables,
                            Severity = AnalyzerItemBase.eSeverity.Medium
                        };
                        AddIssue(issuesList, aa);
                    }
                    else
                    {
                        AnalyzeActivity aa = new(activity)
                        {
                            Status = AnalyzerItemBase.eStatus.NeedFix,
                            ItemName = var.Name,
                            Description = var + " is Unused in " + variableSourceType + ": " + activity.ActivityName,
                            Details = variableSourceType,
                            ItemClass = GingerDicser.GetTermResValue(eTermResKey.Variable),
                            //BusinessFlow = businessFlow;
                            ItemParent = variableSourceName,
                            CanAutoFix = AnalyzerItemBase.eCanFix.Yes,
                            IssueType = AnalyzerItemBase.eType.Error,
                            FixItHandler = DeleteUnusedVariables,
                            Severity = AnalyzerItemBase.eSeverity.Medium,
                        };
                        AddIssue(issuesList, aa);
                    }
                }
            }
        }

        private static void DeleteUnusedVariables(object sender, EventArgs e)
        {
            if (sender.GetType().Equals(typeof(AnalyzeActivity)))
            {
                Activity activity = ((AnalyzeActivity)sender).Activity;
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
                BusinessFlow BFlow = ((AnalyzeBusinessFlow)sender).BusinessFlow;
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
            ObservableList<AnalyzerItemBase> issues = [];
            RunRunSetConfigAnalyzer(runset, RunSetConfigAnalyzer.Check.All, AnalyzeGingerRunner.Check.All, issues);

            List<AnalyzerItemBase> highCriticalIssues = issues.Where(x => x.Severity is AnalyzerItemBase.eSeverity.High or AnalyzerItemBase.eSeverity.Critical).ToList();
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
