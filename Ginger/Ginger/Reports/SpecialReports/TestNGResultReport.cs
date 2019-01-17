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

using System.Collections.Generic;
using System.Linq;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.Actions;
using Ginger.Reports;
using Amdocs.Ginger.Common;

namespace Ginger.Reports
{
    class TestNGResultReport : XMLReportBase
    {
        public override string CreateReport(ReportInfo RI, bool statusByGroupActivity)
        {
            base.RI = RI;
            string runSetName = "GingerRunSet";
            string runGroupName = "GingerRunGroup";
            List<BusinessFlowReport> BizFlows = RI.BusinessFlows;
            int passCount = 0, failedCount = 0, blockedCount = 0, totalCount = 0;
            if (BizFlows.Count == 0)
            {
                return "There are no " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlows) + " to create report";
            }

            string reportHeader = @"<?xml version=""1.0"" encoding=""UTF-8""?>";
            string reportGroups = @"
        <groups>
            <group name=""" + runGroupName + @""">";
            string reportClasses = "";
            string BFNames = "";

            foreach (BusinessFlowReport BFR in BizFlows)
            {
                string BFResults = "";
                BusinessFlow BF =(BusinessFlow)BFR.GetBusinessFlow();
                totalCount = totalCount + BF.Activities.Count;
                reportClasses = reportClasses + @"
            <class name=""" + BF.Name + @".Ginger Business Flow"">";
                if(!statusByGroupActivity)
                    BFResults = CraeteActivitiesStatusXml(BF, ref passCount, ref failedCount, ref blockedCount);
                else
                    BFResults = CraeteGroupActivitiesStatusXml(BF, ref passCount, ref failedCount, ref blockedCount);

                reportClasses = reportClasses + BFResults + @"
            </class>";             

                BFNames = BFNames + @"
                <method signature=""" + GingerCore.General.ConvertInvalidXMLCharacters(BF.Name) + @""" name=""" + GingerCore.General.ConvertInvalidXMLCharacters(BF.Name) + @""" class=""" + GingerCore.General.ConvertInvalidXMLCharacters(BF.Name) + ".Ginger Business Flow" + @"""/>";
            }
            reportHeader = reportHeader + @"
<testng-results blocked=""" + blockedCount + @""" failed=""" + failedCount + @""" total=""" + totalCount + @""" passed=""" + passCount + @""">";
            reportHeader = reportHeader + @"
    <suite name=""" + GingerCore.General.ConvertInvalidXMLCharacters(runSetName) + @""">";
            reportGroups = reportGroups + BFNames + @"
            </group>
        </groups>";
            string xml = "";

            xml = xml + reportHeader + reportGroups + @"
        <test name=""GingerTest"">" + reportClasses;
            xml = xml + @"
        </test>
    </suite>
</testng-results>
";
            return xml;
        }

        private string CraeteGroupActivitiesStatusXml(BusinessFlow BF, ref int passCount, ref int failedCount, ref int blockedCount)
        {
            string BFResults = string.Empty;
            long? elapsed = 0;
            if (BF.ActivitiesGroups.Count > 0)
            {
                BF.AttachActivitiesGroupsAndActivities();
                foreach (var item in BF.ActivitiesGroups)
                {
                    if (item.ActivitiesIdentifiers.Count > 0)
                    {
                        List<Activity> acts = item.ActivitiesIdentifiers.Select(a => a.IdentifiedActivity).ToList();
                        Amdocs.Ginger.CoreNET.Execution.eRunStatus status = getGroupActivityStatus(acts, ref elapsed);
                        BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, GingerCore.General.ConvertInvalidXMLCharacters(BF.Name), GingerCore.General.ConvertInvalidXMLCharacters(item.Name), GingerCore.General.ConvertInvalidXMLCharacters(item.Description), elapsed);
                    }
                }

                //create ungrouped for activities that are not in any group
                List<Activity> unGroupedAct = new List<Activity>();
                foreach (var item in BF.Activities)
                {
                    if (item.ActivitiesGroupID == string.Empty)
                    {
                        unGroupedAct.Add(item);
                    }
                }
                if (unGroupedAct.Count > 0)
                {
                    Amdocs.Ginger.CoreNET.Execution.eRunStatus status = getGroupActivityStatus(unGroupedAct, ref elapsed);
                    BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, GingerCore.General.ConvertInvalidXMLCharacters(BF.Name), "Ungrouped", "Ungrouped", elapsed);
                }
            }
            else//if there are no groups create default group
            {
                Amdocs.Ginger.CoreNET.Execution.eRunStatus status = getGroupActivityStatus(BF.Activities.ToList(), ref elapsed);
                BFResults += buildXml(status, ref passCount, ref failedCount, ref blockedCount, GingerCore.General.ConvertInvalidXMLCharacters(BF.Name), "Ungrouped", "Ungrouped", elapsed);
            }          

            return BFResults;
        }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus getGroupActivityStatus(List<Activity> activityList, ref long? elapsed)
        {
            Amdocs.Ginger.CoreNET.Execution.eRunStatus status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;

            //if there is one fail then Activity status is fail
            if (activityList.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).FirstOrDefault() != null)
                status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
            else
            {
                // If we have at least 1 pass then it passed, otherwise will remain Skipped
                if (activityList.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed).FirstOrDefault() != null)
                {
                    status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
            }

            foreach (var item in activityList)
            {
                elapsed += item.Elapsed;
            }

            return status;
        }


        private string CraeteActivitiesStatusXml(BusinessFlow BF, ref int passCount, ref int failedCount, ref int blockedCount)
        {            
            string BFResults = string.Empty;
            foreach (Activity activity in BF.Activities)
            {
                BFResults += buildXml(activity.Status, ref passCount, ref failedCount, ref blockedCount, GingerCore.General.ConvertInvalidXMLCharacters(BF.Name), GingerCore.General.ConvertInvalidXMLCharacters(activity.ActivityName), GingerCore.General.ConvertInvalidXMLCharacters(activity.Description), activity.Elapsed);
            }
            return BFResults;
        }

        private string buildXml(Amdocs.Ginger.CoreNET.Execution.eRunStatus? status, ref int passCount, ref int failedCount, ref int blockedCount, string BFName, string activityName, string description, long? elapsed)
        {
            string BFResults = string.Empty;
            if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed)
            {
                passCount++;
                BFResults = BFResults + @"
                 <test-method status=""PASS"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                 @""" duration-ms=""" + elapsed + @""" started-at="""" description=""" + description + @""" finished-at="""">
                 </test-method>";
            }
            else if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
            {
                failedCount++;
                BFResults = BFResults + @"
                 <test-method status=""FAIL"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                 @""" duration-ms=""" + elapsed + @""" started-at="""" description=""" + description + @""" finished-at="""">
                 </test-method>";
            }
            else if (status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked || status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending || status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped)
            {
                blockedCount++;
                BFResults = BFResults + @"
                 <test-method status=""SKIP"" signature=""Ginger RunSet " + BFName + @""" name=""" + activityName +
                  @""" duration-ms=""" + elapsed + @""" started-at="""" description=""" + description + @""" finished-at="""">
                 </test-method>";
            }
            return BFResults;
        }
    }
}
