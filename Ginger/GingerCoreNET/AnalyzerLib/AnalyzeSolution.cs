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

using System.Collections.Generic;
using System.Linq;
using Ginger.SolutionGeneral;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeSolution : AnalyzerItemBase
    {
        public static List<AnalyzerItemBase> Analyze(Solution Solution)
        {
            // Put all tests on Solution here
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();

            if (Solution.ApplicationPlatforms == null || Solution.ApplicationPlatforms.Count() == 0)
            {
                AnalyzeSolution AS = new AnalyzeSolution();
                AS.Details = "Solution doesn't have Applications/Platforms defined";
                AS.Description = "Solution doesn't have Applications/Platforms defined";
                AS.HowToFix = "Goto to Solution tab, select the solution tree item, add application to the grid";
                AS.Status = AnalyzerItemBase.eStatus.NeedFix;
                AS.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                AS.Impact = "Activities will not run and will fail";
                AS.ItemParent = "NA";
                AS.ItemName = "No Applications / Platforms defined";
                AS.ItemClass = "Solution";
                AS.Severity = eSeverity.Critical;
                AS.Selected = false;
                IssuesList.Add(AS);
            }
            return IssuesList;
        }
    }
}