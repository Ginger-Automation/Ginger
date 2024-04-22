using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.ALMLib.Azure
{
    public class AzureTestPlan
    {
        public static partial class Fields
        {
            public static string Name = "Name";
            public static string AzureID = "AzureID";
            public static string State = "State";
            public static string Project = "Project";
        }
        public AzureTestPlan()
        {
            this.TestCases = new List<AzureTestCases>();
        }
        public string Name { get; set; }
        public string AzureID { get; set; }
        public string Project { get; set; }
        public string State { get; set; }
        public string TestLink { get; set; }
        public string Description { get; set; }
        public List<AzureTestCases> TestCases { get; set; }
    }
}
