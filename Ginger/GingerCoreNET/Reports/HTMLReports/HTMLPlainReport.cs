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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using GingerCore;

namespace Ginger.Reports.HTMLReports
{
    class HTMLPlainReport : HTMLReportBase
    {
        public override string CreateReport(ReportInfo RI)
        {
            base.RI = RI;

            if (RI.BusinessFlows.Count == 0)
            {
                return "There are no " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " to create report";
            }

            string s = "";

            //Report Header
            XElement header = new XElement("h1", "Amdocs Ginger Automation Execution Report");
            header.SetAttributeValue("style", "color:blue");
            s += header.ToString();

            XElement title = new XElement("h2", " Summary Report - " + GingerDicser.GetTermResValue(eTermResKey.RunSet));
            title.SetAttributeValue("style", "color:brown");
            s += title.ToString();

            s += new XElement("h3", DateTime.Now.ToString()).ToString();

            //TODO: get exec time
            s += new XElement("h4", "Total Execution elapsed time: " + RI.TotalExecutionTime.ToString());

            s += new XElement("h5", "Total " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + ": " + RI.BusinessFlows.Count()).ToString();
            int Passcount = RI.TotalBusinessFlowsPassed;
            int Failcount = RI.TotalBusinessFlowsFailed;
            int StopCount = RI.TotalBusinessFlowsStopped;
            Failcount = Failcount + StopCount;

            XElement xPass = new XElement("h5", "Passed: " + Passcount);
            xPass.SetAttributeValue("style", "color:green");
            s += xPass.ToString();

            XElement xFail = new XElement("h5", "Failed: " + Failcount);
            if (Failcount == 0)
            {
                xFail.SetAttributeValue("style", "color:green");
            }
            else
            {
                xFail.SetAttributeValue("style", "color:red");
            }
            s += xFail.ToString();
            
            List<string> headers = new List<string>();
            headers.Add(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            headers.Add("Elapsed");
            headers.Add("Status");

            XNode m = CreateHTMLTable(RI.BusinessFlows, headers, BizFlowHTMLColumns);
            s += m.ToString();
            return s;
        }
    }
}
