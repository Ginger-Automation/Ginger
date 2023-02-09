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
using System.Text;
using System.Xml.Linq;
using GingerCore;

namespace Ginger.Reports.HTMLReports
{
    public class NewHTMLReport : HTMLReportBase
    {
        StringBuilder mStringBuilder;
        public override string CreateReport(ReportInfo reportInfo)
        {
            base.RI = reportInfo;
            mStringBuilder = new StringBuilder();

            CreateReportHeader();

            switch (reportInfo.reportInfoLevel)
            {
                case ReportInfo.ReportInfoLevel.BussinesFlowLevel:
                    BusinessFlowReport businessFlowReport = (BusinessFlowReport)reportInfo.ReportInfoRootObject;
                    businessFlowReport.ExecutionLoggerIsEnabled = true;
                    CreateBusinessFlowReport(businessFlowReport);
                    break;

                    // TODO: all the rest
            }
            

            return mStringBuilder.ToString();

        }

        private void CreateBusinessFlowReport(BusinessFlowReport businessFlowReport)
        {            
            //TODO: use the new cool style and create in parallel

            List<string> headers = new List<string>();
            headers.Add(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
            headers.Add("Elapsed");
            headers.Add("Status");

            List<BusinessFlowReport> ll = new List<BusinessFlowReport>();
            ll.Add(businessFlowReport);
            XNode m = CreateHTMLTable(ll, headers, BizFlowHTMLColumns);
            mStringBuilder.Append(m.ToString());

            //append Activites
            List<string> headers2 = new List<string>();
            headers2.Add(GingerDicser.GetTermResValue(eTermResKey.Activities));
            headers2.Add(nameof(ActivityReport.ActivityName));
            headers2.Add(nameof(ActivityReport.RunStatus));
            headers2.Add(nameof(ActivityReport.ElapsedSecs));

            //XNode m2 = CreateHTMLTable(businessFlowReport.Activities, headers2, BizFlowHTMLColumns);
            //mStringBuilder.Append(m2.ToString());

            //foreach (ActivityReport activityReport in businessFlowReport.Activities)
            //{
            //    CreateActivityReport(activityReport);
            //}
        }

        private void CreateActivityReport(ActivityReport activityReport)
        {
            //List<string> headers = new List<string>();
            //headers.Add(GingerDicser.GetTermResValue(eTermResKey.Activities));
            //headers.Add(nameof(activityReport.ActivityName));
            //headers.Add(nameof(activityReport.RunStatus));
            //headers.Add(nameof(activityReport.ElapsedSecs));
            
            //XNode m = CreateHTMLTable(ll, headers, BizFlowHTMLColumns);
            //mStringBuilder.Append(m.ToString());
        }

        private void CreateReportHeader()
        {
            //Report Header
            string s = "";
            XElement header = new XElement("h1", "Amdocs Ginger Automation Execution Report");
            header.SetAttributeValue("style", "color:blue");
            s += header.ToString();

            XElement title = new XElement("h2", " Summary Report - " + GingerDicser.GetTermResValue(eTermResKey.RunSet));
            title.SetAttributeValue("style", "color:brown");
            s += title.ToString();

            s += new XElement("h3", DateTime.Now.ToString()).ToString();

            //TODO: get exec time
            s += new XElement("h4", "Total Execution elapsed time: " + RI.TotalExecutionTime.ToString());

            s += new XElement("h5", "Total " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + ": " + "1234");
            //int Passcount = RI.TotalBusinessFlowsPassed;
            //int Failcount = RI.TotalBusinessFlowsFailed;
            //int StopCount = RI.TotalBusinessFlowsStopped;
            //Failcount = Failcount + StopCount;

            XElement xPass = new XElement("h5", "Passed: 1234444");
            xPass.SetAttributeValue("style", "color:green");
            s += xPass.ToString();

            XElement xFail = new XElement("h5", "Failed: 2222");
            //if (Failcount == 0)
            //{
                xFail.SetAttributeValue("style", "color:green");
            //}
            //else
            //{
            //    xFail.SetAttributeValue("style", "color:red");
            //}
            s += xFail.ToString();

            
            mStringBuilder.Append(s);
        }
    }
}
