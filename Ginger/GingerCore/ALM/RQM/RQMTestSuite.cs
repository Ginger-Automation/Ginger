//# Status=Cleaned; Comment=Cleaned on 3/29/18 
#region License
/*
Copyright © 2014-2018 European Support Limited

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
    public class RQMTestSuite
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

        public RQMTestSuite()
        {

        }

        public RQMTestSuite(string uRLPathVersioned)
        {
            URLPathVersioned = uRLPathVersioned;
            TestCases = new ObservableList<RQMTestCase>();
            TestSuiteExecutionRecord = new RQMTestSuiteExecutionRecord();
        }

        public RQMTestSuite(string name, string rQMID, string createdBy, string description, DateTime creationDate, ObservableList<RQMTestCase> rQMTestCasesList)
        {
            Name = name;
            RQMID = rQMID;
            CreatedBy = createdBy;
            CreationDate = creationDate;
            Description = description;
            TestCases = rQMTestCasesList != null ? rQMTestCasesList : new ObservableList<RQMTestCase>();
            TestSuiteExecutionRecord = new RQMTestSuiteExecutionRecord();
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

        public RQMTestSuiteExecutionRecord TestSuiteExecutionRecord { get; set; }

        public ACL_Data_Contract.TestSuite ACL_TestSuite_Copy { get; set; }
    }
}
