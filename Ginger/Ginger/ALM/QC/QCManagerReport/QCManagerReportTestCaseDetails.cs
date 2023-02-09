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

namespace Ginger.ALM.QC
{
    class QCManagerReportTestCaseDetails
    {
        public static class Fields
        {
            public static string TestSetID = "TestSetID";
            public static string TestSetName = "TestSetName";
            public static string TestCaseID = "TestCaseID";
            public static string TestCaseName = "TestCaseName";
            public static string ActivitiesGroupID = "ActivitiesGroupID";
            public static string ActivitiesGroupName = "ActivitiesGroupName";
            public static string ActivitiesGroupAutomationPrecentage = "ActivitiesGroupAutomationPrecentage";
            public static string NumberOfExecutions = "NumberOfExecutions";
            public static string NumberOfPassedExecutions = "NumberOfPassedExecutions";
            public static string PassRate = "PassRate";
            public static string LastExecutionTime = "LastExecutionTime";
            public static string LastExecutionStatus = "LastExecutionStatus";
            public static string TesterName = "TesterName";
        }

        public string TestSetID { get; set; }
        public string TestSetName { get; set; }

        public string TestCaseID { get; set; }
        public string TestCaseName { get; set; }

        public Guid ActivitiesGroupID { get; set; }
        public string ActivitiesGroupName { get; set; }
        public string ActivitiesGroupAutomationPrecentage { get; set; }

        public int NumberOfExecutions { get; set; }
        public int NumberOfPassedExecutions { get; set; }
        public string PassRate
        {
            get
            {
                if (NumberOfPassedExecutions != 0 && NumberOfExecutions!= 0)
                {
                    return (Math.Floor(((double)NumberOfPassedExecutions / (double)NumberOfExecutions) * 100)).ToString() + "%";                    
                }
                else
                {
                    return "0%";
                }
            }
        }

        public string LastExecutionTime { get; set; }
        public string LastExecutionStatus { get; set; }

        public string TesterName { get; set; }
    }
}
