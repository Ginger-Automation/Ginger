//# Status=Cleaned; Comment=Cleaned on 3/29/18 
using Amdocs.Ginger.Common;

namespace GingerCore.ALM.RQM
{
    public class RQMTestSuiteResults
    {
        public static class Fields
        {
            public static string RQMID = "RQMID";
            public static string URLPathVersioned = "URLPathVersioned";
        }

        public RQMTestSuiteResults()
        {
            RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();
            ACL_TestSuiteLog_Copy = new ACL_Data_Contract.TestSuiteLog();
            ACL_TestSuiteLog_Copy.TestSuiteLogExecutionRecords = new System.Collections.Generic.List<ACL_Data_Contract.ExecutionResult>();
        }

        public RQMTestSuiteResults(string uRLPathVersioned)
        {
            URLPathVersioned = uRLPathVersioned;
            RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();
            ACL_TestSuiteLog_Copy = new ACL_Data_Contract.TestSuiteLog();
            ACL_TestSuiteLog_Copy.TestSuiteLogExecutionRecords = new System.Collections.Generic.List<ACL_Data_Contract.ExecutionResult>();
        }

        public string RQMID { get; set; }

        public string URLPathVersioned { get; set; }

        public ObservableList<RQMExecutionRecord> RQMExecutionRecords { get; set; }

        public ACL_Data_Contract.TestSuiteLog ACL_TestSuiteLog_Copy { get; set; }
    }
}
