//# Status=Cleaned; Comment=Cleaned on 04/03/18  
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

namespace GingerCore.ALM.RQM
{
    public class RQMTestSuiteExecutionRecord
    {
        public static class Fields
        {
            public static string RQMID = "RQMID";
            public static string URLPathVersioned = "URLPathVersioned";
            public static string TestSuiteResults = "TestSuiteResults";
            public static string CurrentTestSuiteResult = "CurrentTestSuiteResult";
        }

        public RQMTestSuiteExecutionRecord()
        {
            CurrentTestSuiteResult = new RQMTestSuiteResults();
            TestSuiteResults = new ObservableList<RQMTestSuiteResults>();
            CurrentTestSuiteResult = new RQMTestSuiteResults();
            CurrentTestSuiteResult.RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();
        }

        public RQMTestSuiteExecutionRecord(string uRLPathVersioned)
        {
            URLPathVersioned = uRLPathVersioned;
        }

        public string RQMID { get; set; }

        public string URLPathVersioned { get; set; }

        public ObservableList<RQMTestSuiteResults> TestSuiteResults { get; set; }

        public RQMTestSuiteResults CurrentTestSuiteResult { get; set; }

    }
}
