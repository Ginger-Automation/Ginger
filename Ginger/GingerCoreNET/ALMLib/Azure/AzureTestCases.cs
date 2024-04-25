using GingerCore.ALM.JIRA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.ALMLib.Azure
{
    public class AzureTestCases
    {
        public AzureTestCases()
        {
            this.Steps = new List<AzureTestCasesSteps>();
        }

        public AzureTestCases(string testId, string labels, string description, List<AzureTestCasesSteps> steps)
        {
            this.TestName = labels;
            this.TestID = testId;
            this.Description = description;
            this.Steps = steps;
        }

        public string TestName { get; set; }
        public string TestID { get; set; }
        public string Description { get; set; }
        public string Project { get; set; }

        //Called Test cases Steps
        public List<AzureTestCasesSteps> Steps { get; set; }
    }
}
