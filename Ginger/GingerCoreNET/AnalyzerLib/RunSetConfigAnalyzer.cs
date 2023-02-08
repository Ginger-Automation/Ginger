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

using System;
using System.Collections.Generic;
using System.Linq;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;

namespace Ginger.AnalyzerLib
{
    class RunSetConfigAnalyzer : AnalyzerItemBase
    {
        public RunSetConfig RunSetConfig { get; set; }

        public static List<AnalyzerItemBase> Analyze(RunSetConfig RSC)
        {
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();
            // check that we have Runners
            if (RSC.GingerRunners.Count() == 0)
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

            //check we do not have duplicates Agents
            if (RSC.RunModeParallel)
            {
                List<Guid> Agents = new List<Guid>();
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
                        if (AA.Agent == null) continue;//no Agent so skip it

                        Guid agnetGuide = (from x in Agents where x == AA.Agent.Guid select x).FirstOrDefault();
                        if (agnetGuide == Guid.Empty)
                            Agents.Add(AA.Agent.Guid);
                        else
                        {
                            if (!AA.Agent.SupportVirtualAgent())
                            {
                                //create error
                                RunSetConfigAnalyzer AGR = CreateNewIssue(IssuesList, RSC);
                                AGR.ItemParent = GR.Name;
                                AGR.Description = "Same Agent was configured on more than one Runner";
                                AGR.Details = string.Format("The '{0}' Runner '{1}' Target Application is mapped to the '{2}' Agent which is already configured on another Runner", GR.Name, AA.AppName, AA.AgentName);
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
                                issueExist = optionalVariables.Where(x => x.Name == inputVar.MappedOutputValue).FirstOrDefault() == null;
                                break;
                            case VariableBase.eOutputType.OutputVariable:
                                if (optionalOutputVariables == null)
                                {
                                    optionalOutputVariables = ((GingerExecutionEngine)GR.Executor).GetPossibleOutputVariables(RSC, bf, includeGlobalVars: false, includePrevRunnersVars: true);
                                }                              
                                issueExist = optionalOutputVariables.Where(x => x.VariableInstanceInfo == inputVar.MappedOutputValue).FirstOrDefault() == null;
                                break;
                            case VariableBase.eOutputType.GlobalVariable:                                
                                Guid.TryParse(inputVar.MappedOutputValue, out mappedGuid);
                                issueExist = WorkSpace.Instance.Solution.Variables.Where(x => x.Guid == mappedGuid).FirstOrDefault() == null;
                                break;
                            case VariableBase.eOutputType.ApplicationModelParameter:
                                Guid.TryParse(inputVar.MappedOutputValue, out mappedGuid);
                                issueExist = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>().Where(x => x.Guid == mappedGuid).FirstOrDefault() == null;
                                break;
                            case VariableBase.eOutputType.DataSource:
                                issueExist = string.IsNullOrEmpty(inputVar.MappedOutputValue);
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

            return IssuesList;
        }        

        static RunSetConfigAnalyzer CreateNewIssue(List<AnalyzerItemBase> IssuesList, RunSetConfig RSC)
        {
            RunSetConfigAnalyzer RSCA = new RunSetConfigAnalyzer();
            RSCA.Status = AnalyzerItemBase.eStatus.NeedFix;            
            RSCA.ItemName = RSC.Name;
            RSCA.ItemClass = "Run Set";
            IssuesList.Add(RSCA);
            return RSCA;
        }
    }
}
