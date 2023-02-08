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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Run;
using GingerCore.Platforms;
using GingerCore;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace Ginger.AnalyzerLib
{
    class AnalyzeGingerRunner : AnalyzerItemBase
    {
        public GingerRunner GingerRunner { get; set; }

        public static List<AnalyzerItemBase> Analyze(GingerRunner GR, ObservableList<ApplicationPlatform> solutionApplicationPlatforms)
        {
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();

            // check that we have BFs
            if (GR.Executor.BusinessFlows.Count() == 0)
            {
                AnalyzeGingerRunner AGR = CreateNewIssue(IssuesList, GR);
                AGR.Description = "Runner is missing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows);
                AGR.Details = "Nothing to run";
                AGR.HowToFix = "Add " +GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + "to the Runner or delete the Runner";
                AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;                   
                AGR.IssueType = eType.Warning;
                AGR.Impact = "Waste of resources";
                AGR.Severity = eSeverity.Medium;
                AGR.Selected = false;
            }

            //check all Agents are configured            
            foreach (ApplicationAgent AA in GR.ApplicationAgents)
            {
                if (string.IsNullOrEmpty(AA.AgentName))
                {
                    if (GR.Executor.SolutionApplications.Where(x => (x.AppName == AA.AppName && x.Platform == ePlatformType.NA)).FirstOrDefault() != null)
                        continue;
                    //create error
                    AnalyzeGingerRunner AGR = CreateNewIssue(IssuesList, GR);
                    AGR.ItemParent = GR.Name;
                    AGR.Description = "Target Application is not mapped to an Agent";
                    AGR.Details = string.Format("The '{0}' Runner '{1}' Target Application is not mapped to any Agent", GR.Name, AA.AppName);
                    AGR.HowToFix = "Map the Target Application to an Agent";
                    AGR.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                    AGR.IssueType = eType.Error;
                    AGR.Impact = "Execution will fail.";
                    AGR.Severity = eSeverity.Critical;
                    AGR.Selected = false;
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
            AnalyzeGingerRunner AGR = new AnalyzeGingerRunner();
            AGR.Status = AnalyzerItemBase.eStatus.NeedFix;
            AGR.GingerRunner = GR;
            AGR.ItemName = GR.Name;
            AGR.ItemClass = "Runner";
            IssuesList.Add(AGR);
            return AGR;
        }
    }
}