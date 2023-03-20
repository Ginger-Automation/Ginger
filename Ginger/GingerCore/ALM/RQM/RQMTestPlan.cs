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

namespace GingerCore.ALM.RQM
{
    public class RQMTestPlan
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string RQMID = "RQMID";
            public static string CreatedBy = "CreatedBy";
            public static string CreationDate = "CreationDate";
            public static string URLPath = "URLPath";
        }

        public RQMTestPlan()
        {

        }
        public RQMTestPlan(string name, string uRLPath, string uRLPathVersioned, string rQMID, string createdBy, DateTime creationDate)
        {
            Name = name;
            URLPath = uRLPath;
            URLPathVersioned = uRLPathVersioned;
            RQMID = rQMID;
            CreatedBy = createdBy;
            CreationDate = creationDate;
            Description = string.Empty;
            TestCases = new ObservableList<RQMTestCase>();
            RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();
            TestSuites = new ObservableList<RQMTestSuite>();
        }

        public ObservableList<RQMTestCase> RQMTestCases()
        {
            if (this.TestCases == null)
            {
                this.TestCases = new ObservableList<RQMTestCase>();
            }
            return this.TestCases;
        }
        public string Seq { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string URLPath { get; set; }

        public string URLPathVersioned { get; set; }

        public string RQMID { get; set; }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public ObservableList<RQMTestCase> TestCases { get; set; }

        public ObservableList<RQMExecutionRecord> RQMExecutionRecords { get; set; }

        public ObservableList<RQMTestSuite> TestSuites { get; set; }
    }
}
