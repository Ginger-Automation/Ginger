#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Ginger.Run;
using GingerCore.Platforms;

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
                        if (AA.Agent == null) continue;//no Agent so skip it

                        Guid agnetGuide = (from x in Agents where x == AA.Agent.Guid select x).FirstOrDefault();
                        if (agnetGuide == Guid.Empty)
                            Agents.Add(AA.Agent.Guid);
                        else
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
