//# Status=Cleaned; Comment=Cleaned on 3/29/18 
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
