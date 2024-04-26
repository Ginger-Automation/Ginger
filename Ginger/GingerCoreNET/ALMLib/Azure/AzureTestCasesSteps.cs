using GingerCore.ALM.JIRA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amdocs.Ginger.CoreNET.ALMLib.Azure
{
    public class AzureTestCasesSteps
    {

        public AzureTestCasesSteps(string StepName,string StepID)
        {

            this.StepName = StepName;
            this.StepID = StepID;
        }


        public string StepName { get; set; }
        public string StepID  { get; set; }
}
}
