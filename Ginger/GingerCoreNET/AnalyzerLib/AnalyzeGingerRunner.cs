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

using Amdocs.Ginger.Common.GeneralLib;
using Ginger.Run;
using GingerCore;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeGingerRunner : AnalyzerItemBase
    {
        public enum Check : uint
        {
            All = ~0u,
            None = 0u,
            NoBusinessFlows = 1 << 0,
            AgentsAreConfigured = 1 << 2,
        }

        public GingerRunner GingerRunner { get; set; }

        public static List<AnalyzerItemBase> Analyze(GingerRunner GR, Check checks = Check.All)
        {
            List<AnalyzerItemBase> IssuesList = [];

            // check that we have BFs
            if (checks.AreAllFlagsSet(Check.NoBusinessFlows) && !GR.Executor.BusinessFlows.Any())
            {
                AnalyzeGingerRunner AGR = CreateNewIssue(IssuesList, GR);
                AGR.Description = "Runner is missing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows);
                AGR.Details = "Nothing to run";
                AGR.HowToFix = "Add " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + "to the Runner or delete the Runner";
                AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                AGR.IssueType = eType.Warning;
                AGR.Impact = "Waste of resources";
                AGR.Severity = eSeverity.Medium;
                AGR.Selected = false;
            }

            if (checks.AreAllFlagsSet(Check.AgentsAreConfigured))
            {
                //check all Agents are configured            
                foreach (ApplicationAgent AA in GR.ApplicationAgents)
                {
                    if (string.IsNullOrEmpty(AA.AgentName))
                    {
                        if (GR.Executor.SolutionApplications.FirstOrDefault(x => (x.AppName == AA.AppName && x.Platform == ePlatformType.NA)) != null)
                        {
                            continue;
                        }
                        //create error
                        AnalyzeGingerRunner AGR = CreateNewIssue(IssuesList, GR);
                        AGR.ItemParent = GR.Name;
                        AGR.Description = $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} is not mapped to an Agent";
                        AGR.Details = $"The '{GR.Name}' Runner '{AA.AppName}' {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} is not mapped to any Agent";
                        AGR.HowToFix = $"Map the {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} to an Agent";
                        AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AGR.IssueType = eType.Error;
                        AGR.Impact = "Execution will fail.";
                        AGR.Severity = eSeverity.Critical;
                        AGR.Selected = false;
                    }
                }
            }
            return IssuesList;
        }

        private static void FixMissingAgentForApp(object sender, EventArgs e)
        {
            //TODO: auto map an agent based on platform
        }

        static AnalyzeGingerRunner CreateNewIssue(List<AnalyzerItemBase> IssuesList, GingerRunner GR)
        {
            AnalyzeGingerRunner AGR = new AnalyzeGingerRunner
            {
                Status = AnalyzerItemBase.eStatus.NeedFix,
                GingerRunner = GR,
                ItemName = GR.Name,
                ItemClass = "Runner"
            };
            IssuesList.Add(AGR);
            return AGR;
        }
    }
}