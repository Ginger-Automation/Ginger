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

namespace GingerCore.ALM.JIRA
{
    public class JiraTest
    {
        public JiraTest()
        {
            this.Parameters = new List<JiraTestParameter>();
            this.Steps = new List<JiraTestStep>();
        }

        public JiraTest(string testKey, string labels, string description, List<JiraTestStep> steps)
        {
            this.Parameters = new List<JiraTestParameter>();

            this.TestName = labels;
            this.Steps = new List<JiraTestStep>();
            this.TestKey = testKey;
            this.Labels = labels;
            this.Description = description;
            this.Steps = steps;
        }

        public string TestName { get; set; }
        public string TestID { get; set; }
        public string TestKey { get; set; }
        public string TestPath { get; set; }
        public string LinkedTestID { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public string Project { get; set; }
        public string Labels { get; set; }

        //Called Test Parameters
        public List<JiraTestParameter> Parameters { get; set; }  
        public List<JiraTestStep> Steps { get; set; } 
    }
}
