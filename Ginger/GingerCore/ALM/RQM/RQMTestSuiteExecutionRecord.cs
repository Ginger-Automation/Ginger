//# Status=Cleaned; Comment=Cleaned on 04/03/18  
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
