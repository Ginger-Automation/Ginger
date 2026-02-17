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
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Ginger.AnalyzerLib
{
    public class RunSetConfigAnalyzer : AnalyzerItemBase
    {
        public enum Check : uint
        {
            All = ~0u,
            None = 0u,
            NoRunners = 1 << 0,
            DuplicateAgents = 1 << 1,
            BusinessFlowVariablesAreValid = 1 << 2,
            ActivityOutputVariablesMappingValidity = 1 << 3
        }

        public RunSetConfig RunSetConfig { get; set; }

        public static List<AnalyzerItemBase> Analyze(RunSetConfig runSetConfig, Check checks = Check.All)
        {
            return Analyze(runSetConfig, solution: null, checks);
        }

        public static List<AnalyzerItemBase> Analyze(RunSetConfig RSC, Solution solution, Check checks = Check.All)
        {
            List<AnalyzerItemBase> IssuesList = [];
            // check that we have Runners
            if (checks.AreAllFlagsSet(Check.NoRunners) && !RSC.GingerRunners.Any())
            {
                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                AGR.Description = "Missing Runners";
                AGR.Details = "No Runners to run";
                AGR.HowToFix = "Add Runners";
                AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                AGR.IssueType = eType.Warning;
                AGR.Impact = "Nothing to run";
                AGR.Severity = eSeverity.Medium;
                AGR.Selected = false;
            }

            //check we do not have duplicates Agents, ignore the check for single runner 
            if (checks.AreAllFlagsSet(Check.DuplicateAgents) && RSC.RunModeParallel && RSC.GingerRunners.Count > 1)
            {
                List<Guid> Agents = [];
                foreach (GingerRunner GR in RSC.GingerRunners)
                {
                    foreach (ApplicationAgent AA in GR.ApplicationAgents)
                    {
                        if (AA.Agent != null)
                        {
                            if (AA.Agent.AgentOperations == null)
                            {
                                AA.Agent.AgentOperations = new AgentOperations(AA.Agent);
                            }
                        }
                        if (AA.Agent == null)
                        {
                            continue;//no Agent so skip it
                        }

                        Guid agnetGuide = (from x in Agents where x == AA.Agent.Guid select x).FirstOrDefault();
                        if (agnetGuide == Guid.Empty)
                        {
                            Agents.Add(AA.Agent.Guid);
                        }
                        else
                        {
                            if (!AA.Agent.SupportVirtualAgent())
                            {
                                //create error
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = GR.Name;
                                AGR.Description = "Same Agent was configured on more than one Runner";
                                AGR.Details = $"The '{GR.Name}' Runner '{AA.AppName}' {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} is mapped to the '{AA.AgentName}' Agent which is already configured on another Runner";
                                AGR.HowToFix = "Map the Target Application to different Agent";
                                AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution will fail.";
                                AGR.Severity = eSeverity.Critical;
                                AGR.Selected = false;
                            }
                        }
                    }
                }
            }

            //check all configured mapped data still valid
            if (checks.AreAllFlagsSet(Check.BusinessFlowVariablesAreValid))
            {
                foreach (GingerRunner GR in RSC.GingerRunners)
                {
                    foreach (BusinessFlow bf in GR.Executor.BusinessFlows)
                    {
                        List<VariableBase> inputVars = bf.GetBFandActivitiesVariabeles(true).ToList();
                        List<VariableBase> optionalVariables = null;
                        List<VariableBase> optionalOutputVariables = null;
                        foreach (VariableBase inputVar in inputVars)
                        {
                            bool issueExist = false;
                            Guid mappedGuid = Guid.Empty;
                            switch (inputVar.MappedOutputType)
                            {
                                case VariableBase.eOutputType.Variable:
                                    if (optionalVariables == null)
                                    {
                                        optionalVariables = ((GingerExecutionEngine)GR.Executor).GetPossibleOutputVariables(RSC, bf, includeGlobalVars: true, includePrevRunnersVars: false);
                                    }
                                    issueExist = optionalVariables.FirstOrDefault(x => x.Name == inputVar.MappedOutputValue) == null;
                                    break;
                                case VariableBase.eOutputType.OutputVariable:
                                    if (optionalOutputVariables == null)
                                    {
                                        optionalOutputVariables = ((GingerExecutionEngine)GR.Executor).GetPossibleOutputVariables(RSC, bf, includeGlobalVars: false, includePrevRunnersVars: true);
                                    }
                                    issueExist = optionalOutputVariables.FirstOrDefault(x => x.VariableInstanceInfo == inputVar.MappedOutputValue) == null;
                                    break;
                                case VariableBase.eOutputType.GlobalVariable:
                                    if (solution != null)
                                    {
                                        Guid.TryParse(inputVar.MappedOutputValue, out mappedGuid);
                                        issueExist = solution.Variables.FirstOrDefault(x => x.Guid == mappedGuid) == null;
                                    }
                                    break;
                                case VariableBase.eOutputType.ApplicationModelParameter:
                                    Guid.TryParse(inputVar.MappedOutputValue, out mappedGuid);
                                    issueExist = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<GlobalAppModelParameter>(mappedGuid) == null;
                                    break;
                                case VariableBase.eOutputType.DataSource:
                                    issueExist = string.IsNullOrEmpty(inputVar.MappedOutputValue);
                                    break;
                                case VariableBase.eOutputType.Value:
                                    // For Value type, any text input is valid, so no validation is needed
                                    issueExist = false;
                                    break;
                            }

                            if (issueExist)
                            {
                                //create error
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = GR.Name;
                                AGR.Description = string.Format("Configured input {0} data mapping from type '{1}' is missing", GingerDicser.GetTermResValue(eTermResKey.Variable), inputVar.MappedOutputType);
                                AGR.Details = string.Format("In '{0}' Runner, '{1}' {2}, the configured input {3} '{4}' data mapping from type '{5}' and value '{6}' is missing", GR.Name, bf.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), GingerDicser.GetTermResValue(eTermResKey.Variable), inputVar.Name, inputVar.MappedOutputType, inputVar.MappedOutputValue);
                                AGR.HowToFix = string.Format("Re-configure the missing input {0} data mapping", GingerDicser.GetTermResValue(eTermResKey.Variable));
                                AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution might fail due to wrong data mapping";
                                AGR.Severity = eSeverity.High;
                                AGR.Selected = false;
                            }
                        }
                    }
                }
            }

            if (checks.AreAllFlagsSet(Check.ActivityOutputVariablesMappingValidity))
            {
                foreach (GingerRunner runner in RSC.GingerRunners)
                {
                    foreach (BusinessFlowRun bfRun in runner.BusinessFlowsRunList)
                    {
                        BusinessFlow? bf = runner
                            .Executor
                            .BusinessFlows
                            .FirstOrDefault(b => b.Guid == bfRun.BusinessFlowGuid);

                        if (bf == null)
                        {
                            Reporter.ToLog(eLogLevel.WARN, $"No {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}({bfRun.BusinessFlowGuid}-{bfRun.BusinessFlowName}) was by id for analyzation.");
                            continue;
                        }

                        VariableBase[] customizedVars = bfRun
                            .BusinessFlowCustomizedRunVariables
                            .Where(var => var.MappedOutputType == VariableBase.eOutputType.ActivityOutputVariable)
                            .ToArray();

                        foreach (VariableBase customizedVar in customizedVars)
                        {
                            Activity? targetActivity = bf
                                .Activities
                                .FirstOrDefault(activity => activity.Variables.Any(var => var.Guid == customizedVar.Guid));

                            if (targetActivity == null)
                            {
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = bfRun.BusinessFlowName;
                                AGR.Description = $"No {GingerDicser.GetTermResValue(eTermResKey.Activity)}({customizedVar.Guid}) was found by id for mapping input {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                                AGR.Details = $"No {GingerDicser.GetTermResValue(eTermResKey.Activity)}({customizedVar.Guid}) was found by id for mapping input {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                                AGR.HowToFix = $"Provide the correct {GingerDicser.GetTermResValue(eTermResKey.Activity)} id whose input {GingerDicser.GetTermResValue(eTermResKey.Variable)} needs to be mapped.";
                                AGR.CanAutoFix = eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution will fail due to wrong data mapping";
                                AGR.Severity = eSeverity.High;
                                AGR.Selected = false;
                                continue;
                            }

                            Activity? sourceActivity = bf
                                .Activities
                                .FirstOrDefault(activity => activity.Guid == customizedVar.VariableReferenceEntity);

                            if (sourceActivity == null)
                            {
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = bfRun.BusinessFlowName;
                                AGR.Description = $"No {GingerDicser.GetTermResValue(eTermResKey.Activity)}({customizedVar.Guid}) was found by id for mapping output {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                                AGR.Details = $"No {GingerDicser.GetTermResValue(eTermResKey.Activity)}({customizedVar.Guid}) was found by id for mapping output {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                                AGR.HowToFix = $"Provide the correct {GingerDicser.GetTermResValue(eTermResKey.Activity)} id whose output {GingerDicser.GetTermResValue(eTermResKey.Variable)} needs to be mapped.";
                                AGR.CanAutoFix = eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution will fail due to wrong data mapping";
                                AGR.Severity = eSeverity.High;
                                AGR.Selected = false;
                                continue;
                            }

                            bool sourceVarGuidIsValid = Guid.TryParse(customizedVar.MappedOutputValue, out Guid sourceVarGuid);
                            if (!sourceVarGuidIsValid)
                            {
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = sourceActivity.ActivityName;
                                AGR.Description = $"Provided {GingerDicser.GetTermResValue(eTermResKey.Variable)} id '{customizedVar.MappedOutputValue}' is not a valid GUID, cannot use it for mapping input-output mapping.";
                                AGR.Details = $"Provided {GingerDicser.GetTermResValue(eTermResKey.Variable)} id '{customizedVar.MappedOutputValue}' is not a valid GUID, cannot use it for mapping input-output mapping.";
                                AGR.HowToFix = $"Provide valid GUID for {GingerDicser.GetTermResValue(eTermResKey.Variable)} id.";
                                AGR.CanAutoFix = eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution will fail due to wrong data mapping";
                                AGR.Severity = eSeverity.High;
                                AGR.Selected = false;
                                continue;
                            }

                            VariableBase? sourceVar = sourceActivity
                                .GetVariables()
                                .Where(var => var.SetAsOutputValue)
                                .FirstOrDefault(var => var.Guid == sourceVarGuid);

                            if (sourceVar == null)
                            {
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = sourceActivity.ActivityName;
                                AGR.Description = $"No output {GingerDicser.GetTermResValue(eTermResKey.Variable)} found with id '{sourceVarGuid}' for mapping it's output value.";
                                AGR.Details = $"No output {GingerDicser.GetTermResValue(eTermResKey.Variable)} found with id '{sourceVarGuid}' for mapping it's output value.";
                                AGR.HowToFix = $"Provide valid id for output {GingerDicser.GetTermResValue(eTermResKey.Variable)}.";
                                AGR.CanAutoFix = eCanFix.No;
                                AGR.IssueType = eType.Error;
                                AGR.Impact = "Execution will fail due to wrong data mapping";
                                AGR.Severity = eSeverity.High;
                                AGR.Selected = false;
                                continue;
                            }
                        }
                    }
                }
            }

            return IssuesList;
        }

        static RunSetConfigAnalyzer CreateNewIssue(List<AnalyzerItemBase> IssuesList, RunSetConfig RSC)
        {
            RunSetConfigAnalyzer RSCA = new RunSetConfigAnalyzer
            {
                Status = AnalyzerItemBase.eStatus.NeedFix,
                ItemName = RSC.Name,
                ItemClass = "Run Set"
            };
            IssuesList.Add(RSCA);
            return RSCA;
        }
    }
}
