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
using System.Xml;
using System.Xml.Linq;
using GingerCore;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common;

namespace Ginger.Reports
{
    // Base class for HTML report with helper functions to create nice HTML report
    public abstract class XMLReportBase : ITestNGResultReport
    {
        public ReportInfo RI;
        public abstract string CreateReport(ReportInfo RI, bool statusByGroupActivity, ObservableList<ActInputValue> DynamicParameters);
        public int Passcount { get { return RI.TotalBusinessFlowsPassed; } }
        public int Failcount { get { return RI.TotalBusinessFlowsFailed; } }
        public int ActivityCount { get { return RI.TotalActivitiesCount;} }

        public int ActivityPass { get { return RI.TotalActivitiesByRunStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed); } }
        public int ActivityFail { get { return RI.TotalActivitiesByRunStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed); } }
        public int ActivityOther { get { return ActivityCount - ActivityPass - ActivityFail; } } // ?? TODO: give details of all options

        public int ActionCount { get { return RI.TotalActionsCount(); } }
        public int ActionPass { get { return RI.TotalActionsCountByStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed); } }
        public int ActionFail { get { return RI.TotalActionsCountByStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed); } }
        public int ActionOther { get { return ActionCount - ActionPass - ActionFail; } }

        public int ValidationCount { get { return RI.TotalValidationsCount(); } }
        public int ValidationPass { get { return RI.TotalValidationsCountByStatus(ActReturnValue.eStatus.Passed); } }
        public int validationFail { get { return RI.TotalValidationsCountByStatus(ActReturnValue.eStatus.Failed); } }
        
        protected string BizFlowHTMLColumns(BusinessFlowReport BFR)
        {            
            BusinessFlow BF=(BusinessFlow) BFR.GetBusinessFlow();
            XElement xe = new XElement("div", BF.Name);
            xe.Add(new XElement("td",BF.ElapsedSecs));

            XElement xstatus = new XElement("td", BF.RunStatus );
            if (BF.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                xstatus.SetAttributeValue("bgColor", "green");
            }
            else
                if (BF.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                {
                    xstatus.SetAttributeValue("bgColor", "red");
                }                
            
            xe.Add(xstatus);           
            return xe.ToString();
        }

        protected XNode CreateHTMLTable<T>(IEnumerable<T> items, IEnumerable<string> header, params Func<T, string>[] columns)
        {
            if (!items.Any())
                return null;

            var html = items.Aggregate(new XElement("table", new XAttribute("border", 1)),
                (table, item) =>
                {
                    table.Add(columns.Aggregate(new XElement("tr"),
                        (row, cell) =>
                        {
                            row.Add(new XElement("td", EvalColumn(cell, item)));                            
                            return row;
                        }));
                    return table;
                });

            html.AddFirst(header.Aggregate(new XElement("tr"),
                (row, caption) => { row.Add(new XElement("th", caption)); return row; }));

            return html;
        }

        XNode EvalColumn<T>(Func<T, string> cell, T item)
        {
            var raw = cell(item);
            try
            {
                var xml = XElement.Parse(raw);
                return xml;
            }
            catch (XmlException)
            {
                return new XText(raw);
            }
        }
    }
}