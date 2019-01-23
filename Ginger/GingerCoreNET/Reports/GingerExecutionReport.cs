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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;


namespace Ginger.Reports.GingerExecutionReport
{
    public class GingerExecutionReport
    {
        public string StyleBundle = string.Empty;
        public string JSBundle = string.Empty;
        public string CompanyLogo = string.Empty;
        public string GingerLogo = string.Empty;
        public string BeatLogo = string.Empty;

        public string PrevItemImage = string.Empty;
        public string NextItemImage = string.Empty;

        public string ReportsCSS = string.Empty;
        public string ReportJS = string.Empty;
        public string TemplatesFolder = string.Empty;
        public string HTMLReportMainFolder = string.Empty;
        public HTMLReportConfiguration currentTemplate = new HTMLReportConfiguration();

        private string currentRunSetLinkText = string.Empty;
        private string currentGingerRunnerLinkText = string.Empty;
        private string currentBusinessFlowLinkText = string.Empty;
        private string currentActivityGroupLinkText = string.Empty;
        private string currentActivityLinkText = string.Empty;
        private string currentActionNameText = string.Empty;

        public const int logoWidth = 300;
        public const int logoHight = 75;
        public const int itemPrevNextWidth = 32;
        public const int itemPrevNextHight = 32;
        const int screenShotSampleWidth = 250;
        const int screenShotSampleHight = 150;
        const int screenShotFullWidth = 500;
        const int screenShotFullHight = 300;

        public void CreateRunSetLevelReport(ReportInfo RI)
        {
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("GingerExecutionReport.html", TemplatesFolder);
            StyleBundle = string.Empty;
            JSBundle = string.Empty;
            ReportsCSS = string.Empty;
            ReportJS = string.Empty;
            CompanyLogo = string.Empty;
            GingerLogo = string.Empty;
            BeatLogo = string.Empty;
            StyleBundle = CreateStylePath();
            JSBundle = CreateJavaScriptPath();
            BeatLogo = CreateBeatLogo();
            CompanyLogo = CreateCompanyLogo();
            GingerLogo = CreateGingerLogo();
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (currentTemplate.UseLocalStoredStyling)
            {
                ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
            }
            ReportJS = ExtensionMethods.GetHTMLTemplate("circlechart.js", TemplatesFolder + "/assets/js/");
            // populating "Run Set General Details"
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.RunSetFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");

                if (selectedField.FieldKey == RunSetReport.Fields.RunSetExecutionStatus)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString() + "'>" + ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString() + "</label></td>");
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.GingerRunnersPassRate)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) + " %</td>");
                }
                else if ((selectedField.FieldKey == RunSetReport.Fields.StartTimeStamp) || (selectedField.FieldKey == RunSetReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) + "</td>");
                }


                if (selectedField.FieldKey == RunSetReport.Fields.Name)
                {
                    currentRunSetLinkText = ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString();
                }
            }
            string P = ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty("GingerRunnersPassRate").GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString();
            ReportHTML = ReportHTML.Replace("{PassPercent}", ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty("GingerRunnersPassRate").GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString());
            ReportHTML = ReportHTML.Replace("{FailPercent}", ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty("GingerRunnersFailRate").GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString());
            ReportHTML = ReportHTML.Replace("{StoppedPercent}", ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty("GingerRunnersStoppedRate").GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString());
            ReportHTML = ReportHTML.Replace("{OtherPercent}", ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty("GingerRunnersOtherRate").GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString());
            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name}", currentRunSetLinkText);
            ReportHTML = ReportHTML.Replace("{RunSetGeneralDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{RunSetGeneralDetails_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", "");

            fieldsNamesHTMLTableCells = new StringBuilder();
            fieldsValuesHTMLTableCells = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
            }
            foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.OrderBy(x => x.Seq))
            {
                fieldsValuesHTMLTableCells.Append("<tr>");
                foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                {
                    if ((selectedField.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                    {
                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(GR.GetType().GetProperty(GingerReport.Fields.Seq).GetValue(GR).ToString() + " " + GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR).ToString())
                                                     + @"\GingerRunnerReport.html'>" + GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR) + @"</a></td>");
                    }
                    else if (selectedField.FieldKey == GingerReport.Fields.GingerExecutionStatus)
                    {
                        fieldsValuesHTMLTableCells.Append("<td><label class='Status" + GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR) + "'>" + GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR) + "</label></td>");
                    }
                    else if (selectedField.FieldKey == GingerReport.Fields.BusinessFlowsPassRate)
                    {
                        fieldsValuesHTMLTableCells.Append("<td>" + GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR) + " %</td>");
                    }
                    else if ((selectedField.FieldKey == GingerReport.Fields.StartTimeStamp) || (selectedField.FieldKey == GingerReport.Fields.EndTimeStamp))
                    {
                        fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse((GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR)).ToString()).ToLocalTime().ToString() + "</td>");
                    }
                    else
                    {
                        if (GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR) != null)
                        {
                            fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(GR.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(GR).ToString()) + "</td>");
                        }
                        else
                        {
                            fieldsValuesHTMLTableCells.Append("<td></td>");
                        }
                    }
                }
                fieldsValuesHTMLTableCells.Append("</tr>");

                if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                {
                    string prevGingerSeq = string.Empty;
                    string nextGingerSeq = string.Empty;
                    string prevGingerName = string.Empty;
                    string nextGingerName = string.Empty;
                    if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1) != null)
                    {
                        prevGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Name;
                        prevGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Seq.ToString();
                    }
                    if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1) != null)
                    {
                        nextGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Name;
                        nextGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Seq.ToString();
                    }

                    this.CreateGingerLevelReport(GR, "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevGingerSeq, prevGingerName), new Tuple<string, string>(nextGingerSeq, nextGingerName)));
                }
            }

            ReportHTML = ReportHTML.Replace("{RunSetGingerRunners_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{RunSetGingerRunners_Data}", fieldsValuesHTMLTableCells.ToString());
            System.IO.File.WriteAllText(HTMLReportMainFolder + "\\" + "GingerExecutionReport.html", ReportHTML);

            RI = null;
            ReportHTML = null;
        }

        public void CreateSummaryViewReport(ReportInfo RI)
        {
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("GingerExecutionReport.html", TemplatesFolder);
            string CanvasJs = string.Empty;
            StyleBundle = string.Empty;
            JSBundle = string.Empty;
            ReportsCSS = string.Empty;
            ReportJS = string.Empty;
            CompanyLogo = string.Empty;
            GingerLogo = string.Empty;
            BeatLogo = string.Empty;
            StyleBundle = CreateStylePath();
            JSBundle = CreateJavaScriptPath();
            CompanyLogo = CreateCompanyLogo();
            GingerLogo = CreateGingerLogo();
            BeatLogo = CreateBeatLogo();
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (currentTemplate.UseLocalStoredStyling)
            {
                ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
            }

            CanvasJs = ExtensionMethods.GetHTMLTemplate("donutchart.js", TemplatesFolder + "/assets/js/");
            CanvasJs = "<script type='text/jsx'>" + CanvasJs + "</script>";
            // populating "Run Set General Details"
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableOneCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableOneCells = new StringBuilder();
            StringBuilder fieldsNamesHTMLTableTwoCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableTwoCells = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.RunSetFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                if (selectedField.FieldName.Contains("Executed"))
                {
                    fieldsNamesHTMLTableTwoCells.Append("<td>" + selectedField.FieldName + "</td>");
                    if (((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) == null)
                    {
                        fieldsValuesHTMLTableTwoCells.Append("<td> N/A </td>");
                    }
                    else
                    {
                        fieldsValuesHTMLTableTwoCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()) + "</td>");
                    }
                }
                else
                {
                    fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField.FieldName + "</td>");

                    if (((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)) == null)
                    {
                        if (selectedField.FieldKey == RunSetReport.Fields.EnvironmentsDetails)
                        {
                            StringBuilder environmentNames_str = new StringBuilder();
                            ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Where(x => x.EnvironmentName != null && x.EnvironmentName != string.Empty).GroupBy(q => q.EnvironmentName).Select(q => q.First()).ToList().ForEach(x => environmentNames_str.Append(x.EnvironmentName + ", "));
                            fieldsValuesHTMLTableOneCells.Append("<td>" + environmentNames_str.ToString().TrimEnd(',', ' ') + "</td>");
                            environmentNames_str.Remove(0, environmentNames_str.Length);
                        }
                        else
                        {
                            fieldsValuesHTMLTableOneCells.Append("<td> N/A </td>");
                        }
                    }
                    else
                    {
                        if (selectedField.FieldKey == RunSetReport.Fields.ExecutionDuration)
                        {
                            fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(General.TimeConvert(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString())) + "</td>");
                        }
                        else if ((selectedField.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField.FieldKey == ActionReport.Fields.EndTimeStamp))
                        {
                            fieldsValuesHTMLTableOneCells.Append("<td>" + DateTime.Parse(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()).ToLocalTime().ToString() + "</td>");
                        }
                        else
                        {
                            fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString()) + "</td>");
                        }
                        if (selectedField.FieldKey == RunSetReport.Fields.Name)
                        {
                            currentRunSetLinkText = ((RunSetReport)RI.ReportInfoRootObject).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((RunSetReport)RI.ReportInfoRootObject)).ToString();
                        }

                    }
                }
            }

            ReportHTML = ReportHTML.Replace("{GeneralDetails_Headers}", fieldsNamesHTMLTableTwoCells.ToString());
            ReportHTML = ReportHTML.Replace("{GeneralDetails_Data}", fieldsValuesHTMLTableTwoCells.ToString());
            ReportHTML = ReportHTML.Replace("{ExecutionGeneralDetails_Headers}", fieldsNamesHTMLTableOneCells.ToString());
            ReportHTML = ReportHTML.Replace("{ExecutionGeneralDetails_Data}", fieldsValuesHTMLTableOneCells.ToString());
            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{canvas_path}", CanvasJs);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", "");
            fieldsNamesHTMLTableOneCells.Remove(0, fieldsNamesHTMLTableOneCells.Length);
            fieldsValuesHTMLTableOneCells.Remove(0, fieldsValuesHTMLTableOneCells.Length);
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.RunSetFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {

                if (selectedField.FieldKey == RunSetReport.Fields.ExecutionStatisticsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionStatisticsDetails_Start-->", "<!--ExecutionStatisticsDetails_End-->");
                        ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                    }
                    else
                    {
                        // Ginger Runners Place Holders
                        ReportHTML = ReportHTML.Replace("{GingerRunnersTotalPass}", ((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersPassed.ToString());
                        ReportHTML = ReportHTML.Replace("{GingerRunnersTotalFail}", ((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersFailed.ToString());
                        ReportHTML = ReportHTML.Replace("{GingerRunnersTotalStopped}", ((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersStopped.ToString());
                        ReportHTML = ReportHTML.Replace("{GingerRunnersTotalOther}", ((RunSetReport)RI.ReportInfoRootObject).TotalGingerRunnersOther.ToString());

                        // Business Flows Place Holders
                        ReportHTML = ReportHTML.Replace("{BusinessFlowsTotalPass}", ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsPassed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{BusinessFlowsTotalFail}", ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsFailed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{BusinessFlowsTotalStopped}", ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsStopped).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{BusinessFlowsTotalOther}", ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Select(x => x.TotalBusinessFlowsOther).ToList().Sum().ToString());

                        List<BusinessFlowReport> bfTotalList = new List<BusinessFlowReport>();
                        ((RunSetReport)RI.ReportInfoRootObject).GingerReports.ForEach(x => x.BusinessFlowReports.ForEach(y => bfTotalList.Add(y)));
                        // Activities Place Holders
                        ReportHTML = ReportHTML.Replace("{ActivitiesTotalPass}", bfTotalList.Select(x => x.TotalActivitiesPassed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActivitiesTotalFail}", bfTotalList.Select(x => x.TotalActivitiesFailed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActivitiesTotalStopped}", bfTotalList.Select(x => x.TotalActivitiesStopped).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActivitiesTotalOther}", bfTotalList.Select(x => x.TotalActivitiesOther).ToList().Sum().ToString());

                        List<ActivityReport> activitiesTotalList = new List<ActivityReport>();
                        bfTotalList.ForEach(x => x.Activities.ForEach(y => activitiesTotalList.Add(y)));
                        // Actions Place Holders
                        ReportHTML = ReportHTML.Replace("{ActionsTotalPass}", activitiesTotalList.Select(x => x.TotalActionsPassed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActionsTotalFail}", activitiesTotalList.Select(x => x.TotalActionsFailed).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActionsTotalStopped}", activitiesTotalList.Select(x => x.TotalActionsStopped).ToList().Sum().ToString());
                        ReportHTML = ReportHTML.Replace("{ActionsTotalOther}", activitiesTotalList.Select(x => x.TotalActionsOther).ToList().Sum().ToString());
                    }
                    ReportHTML = ReportHTML.Replace("{ExecutionStatisticsIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{ExecutionStatisticscollapseStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.ExecutedBusinessFlowsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionBusinessFlowsDetails_Start-->", "<!--ExecutionBusinessFlowsDetails_End-->");
                        ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");

                        foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.OrderBy(x => x.Seq))
                        {
                            string prevGingerSeq = string.Empty;
                            string nextGingerSeq = string.Empty;
                            string prevGingerName = string.Empty;
                            string nextGingerName = string.Empty;
                            if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1) != null)
                            {
                                prevGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Name;
                                prevGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Seq.ToString();
                            }
                            if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1) != null)
                            {
                                nextGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Name;
                                nextGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Seq.ToString();
                            }

                            this.CreateGingerLevelReport(GR, "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevGingerSeq, prevGingerName), new Tuple<string, string>(nextGingerSeq, nextGingerName)));
                        }
                    }
                    else
                    {
                        fieldsNamesHTMLTableOneCells = new StringBuilder();
                        fieldsValuesHTMLTableOneCells = new StringBuilder();
                        List<int> listOfHandledGingerRunnersReport = new List<int>();
                        bool firstIteration = true;

                        foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.OrderBy(x => x.Seq))
                        {
                            GR.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                            foreach (BusinessFlowReport br in GR.BusinessFlowReports)
                            {
                                br.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                                fieldsValuesHTMLTableOneCells.Append("<tr>");
                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if ((selectedField_internal.FieldKey == GingerReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString()))
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(GR.GetType().GetProperty(GingerReport.Fields.Seq).GetValue(GR).ToString() + " " + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString())
                                                                 + @"\GingerRunnerReport.html'>" + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) + @"</a></td>");
                                    }
                                    else if (selectedField_internal.FieldKey == GingerReport.Fields.EnvironmentName)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) != null ? GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString() : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == GingerReport.Fields.Seq)
                                    {
                                        if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.SummaryViewLevel.ToString())
                                        {
                                            int currentSeq = Convert.ToInt32(GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString());
                                            if (!listOfHandledGingerRunnersReport.Contains(currentSeq))
                                            {
                                                string prevGingerSeq = string.Empty;
                                                string nextGingerSeq = string.Empty;
                                                string prevGingerName = string.Empty;
                                                string nextGingerName = string.Empty;
                                                if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1) != null)
                                                {
                                                    prevGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Name;
                                                    prevGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) - 1].Seq.ToString();
                                                }
                                                if (((RunSetReport)RI.ReportInfoRootObject).GingerReports.ElementAtOrDefault(((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1) != null)
                                                {
                                                    nextGingerName = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Name;
                                                    nextGingerSeq = ((RunSetReport)RI.ReportInfoRootObject).GingerReports[((RunSetReport)RI.ReportInfoRootObject).GingerReports.FindIndex(x => x.GUID == GR.GUID) + 1].Seq.ToString();
                                                }

                                                this.CreateGingerLevelReport(GR, "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevGingerSeq, prevGingerName), new Tuple<string, string>(nextGingerSeq, nextGingerName)));
                                                listOfHandledGingerRunnersReport.Add(currentSeq);
                                            }
                                        }
                                    }
                                }

                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                    }
                                    if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                        {
                                            fieldsValuesHTMLTableOneCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(GR.GetType().GetProperty(GingerReport.Fields.Seq).GetValue(GR).ToString() + " " + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR).ToString())
                                                                                                 + @"\" + ExtensionMethods.folderNameNormalazing(br.GetType().GetProperty(BusinessFlowReport.Fields.Seq).GetValue(br) + " " + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString())
                                                                                                 + @"\BusinessFlowReport.html'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + @"</a></td>");
                                        }
                                        else
                                        {
                                            fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                        }
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Description)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString() : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunDescription)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString() : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.ExecutionDuration)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null ? General.TimeConvert(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) : string.Empty) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunStatus)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td><label class='Status" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</label></td>");
                                    }
                                    else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.PassPercent)
                                    {
                                        if (firstIteration)
                                        {
                                            fieldsNamesHTMLTableOneCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                                        }
                                        fieldsValuesHTMLTableOneCells.Append("<td>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + " %</td>");
                                    }
                                }
                                fieldsValuesHTMLTableOneCells.Append("</tr>");

                                if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                                {
                                    string prevBusinessFlowSeq = string.Empty;
                                    string nextBusinessFlowSeq = string.Empty;
                                    string prevBusinessFlowName = string.Empty;
                                    string nextBusinessFlowName = string.Empty;
                                    if (GR.BusinessFlowReports.ElementAtOrDefault(GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1) != null)
                                    {
                                        prevBusinessFlowName = GR.BusinessFlowReports[GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Name;
                                        prevBusinessFlowSeq = GR.BusinessFlowReports[GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Seq.ToString();
                                    }
                                    if (GR.BusinessFlowReports.ElementAtOrDefault(GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1) != null)
                                    {
                                        nextBusinessFlowName = GR.BusinessFlowReports[GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Name;
                                        nextBusinessFlowSeq = GR.BusinessFlowReports[GR.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Seq.ToString();
                                    }

                                    CreateBusinessFlowLevelReport(br, HTMLReportMainFolder + @"\" + ExtensionMethods.folderNameNormalazing(GR.Seq + " " + GR.Name) + @"\", "../" + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevBusinessFlowSeq, prevBusinessFlowName), new Tuple<string, string>(nextBusinessFlowSeq, nextBusinessFlowName)));
                                }
                                firstIteration = false;
                            }
                        }

                        ReportHTML = ReportHTML.Replace("{BusinessFlowsDetails_Headers}", fieldsNamesHTMLTableOneCells.ToString());
                        ReportHTML = ReportHTML.Replace("{BusinessFlowsDetails_Data}", fieldsValuesHTMLTableOneCells.ToString());
                        fieldsNamesHTMLTableOneCells.Remove(0, fieldsNamesHTMLTableOneCells.Length);
                        fieldsValuesHTMLTableOneCells.Remove(0, fieldsValuesHTMLTableOneCells.Length);
                    }
                    ReportHTML = ReportHTML.Replace("{BusinessFlowsDetailsIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{BusinessFlowsDetailsStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.ExecutedActivitiesDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string executionStatisticsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ExecutionActivitiesDetails_Start-->", "<!--ExecutionActivitiesDetails_End-->");
                        if (!executionStatisticsSection.Equals(string.Empty))
                            ReportHTML = ReportHTML.Replace(executionStatisticsSection, "");
                    }
                    else
                    {
                        // TO DO - handle the summary view section.
                    }
                }
                else if (selectedField.FieldKey == RunSetReport.Fields.FailuresDetails)
                {
                    bool isFailuresDetailsExists = false;
                    StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
                    StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
                    if (!selectedField.IsSelected)
                    {
                        string failureDetailsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--FailuresDetails_Start-->", "<!--FailuresDetails_End-->");
                        ReportHTML = ReportHTML.Replace(failureDetailsSection, "");
                    }
                    else
                    {
                        bool firstIteration = true;
                        foreach (GingerReport GR in ((RunSetReport)RI.ReportInfoRootObject).GingerReports.Where(x => x.GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).OrderBy(x => x.Seq))
                        {
                            foreach (BusinessFlowReport br in GR.BusinessFlowReports.Where(x => x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()))
                            {
                                foreach (ActivityReport ac in br.Activities.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
                                {
                                    foreach (ActionReport act in ac.ActionReports.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()).OrderBy(x => x.Seq))
                                    {
                                        isFailuresDetailsExists = true;

                                        fieldsValuesHTMLTableCells.Append("<tr>");
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == GingerReport.Fields.Name)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append(@"<td style='padding: 10px; border: 1px solid #dddddd'>" + GR.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(GR) + @"</td>");
                                            }
                                        }

                                        // Businessflow Level
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Business Flow Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                            }
                                        }

                                        // Activity Level 
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Activity Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                            }
                                        }

                                        // Action Level
                                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                        {
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Seq)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Execution Sequence</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Description)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>Action Description</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if (selectedField_internal.FieldKey == ActionReport.Fields.Error)
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd; color:red;white-space:pre-wrap;white-space:-moz-pre-wrap;white-space:-pre-wrap;white-space:-o-pre-wrap;word-break: break-all;'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                            if ((selectedField_internal.FieldKey == ActionReport.Fields.ElapsedSecs) ||
                                                (selectedField_internal.FieldKey == ActionReport.Fields.CurrentRetryIteration) ||
                                                (selectedField_internal.FieldKey == ActionReport.Fields.ExInfo))
                                            {
                                                if (firstIteration)
                                                {
                                                    fieldsNamesHTMLTableCells.Append("<td bgcolor='#1B3651' style='color:#fff;padding:10px;border-right:1px solid #fff'>" + selectedField_internal.FieldName + "</td>");
                                                }
                                                fieldsValuesHTMLTableCells.Append("<td style='padding: 10px; border: 1px solid #dddddd'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                            }
                                        }
                                        fieldsValuesHTMLTableCells.Append("</tr>");
                                        firstIteration = false;
                                    }
                                }
                            }
                        }
                        if (isFailuresDetailsExists)
                        {
                            ReportHTML = ReportHTML.Replace("{FailuresDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
                            ReportHTML = ReportHTML.Replace("{FailuresDetails_Data}", fieldsValuesHTMLTableCells.ToString());
                        }
                        else
                        {
                            string failureDetailsSection = Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetStringBetween(ReportHTML, "<!--FailuresDetails_Start-->", "<!--FailuresDetails_End-->");
                            ReportHTML = ReportHTML.Replace(failureDetailsSection, "");
                        }
                    }
                }
            }

            System.IO.File.WriteAllText(HTMLReportMainFolder + "\\" + "GingerExecutionReport.html", ReportHTML);

            RI = null;
            ReportHTML = null;
        }

        public void CreateGingerLevelReport(GingerReport gr, string ReportLevel, bool calledAsRoot = false, Tuple<Tuple<string, string>, Tuple<string, string>> nextPrevGingerName = null)
        {
            string currentHTMLReportsFolder = HTMLReportMainFolder + @"\" + ExtensionMethods.folderNameNormalazing(gr.Seq + " " + gr.Name) + @"\";
            System.IO.Directory.CreateDirectory(currentHTMLReportsFolder);

            string lastseq = string.Empty;
            string lastbusinessflow = string.Empty;
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("GingerRunnerReport.html", TemplatesFolder);

            if (calledAsRoot)
            {
                gr.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                StyleBundle = string.Empty;
                JSBundle = string.Empty;
                ReportsCSS = string.Empty;
                ReportJS = string.Empty;
                BeatLogo = string.Empty;
                CompanyLogo = string.Empty;
                GingerLogo = string.Empty;
                ReportLevel = "./";
            }

            if (StyleBundle == string.Empty || StyleBundle == "")
            {
                StyleBundle = CreateStylePath();
            }
            if (JSBundle == string.Empty || JSBundle == "")
            {
                JSBundle = CreateJavaScriptPath();
            }
            if (CompanyLogo == string.Empty || CompanyLogo == "")
            {
                CompanyLogo = CreateCompanyLogo();
            }
            if (GingerLogo == string.Empty || GingerLogo == "")
            {
                GingerLogo = CreateGingerLogo();
            }
            if (BeatLogo == string.Empty || BeatLogo == "")
            {
                BeatLogo = CreateBeatLogo();
            }
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (ReportJS == string.Empty || ReportJS == "")
            {
                ReportJS = ExtensionMethods.GetHTMLTemplate("circlechart.js", TemplatesFolder + "/assets/js/");
            }
            if (currentTemplate.UseLocalStoredStyling)
            {
                if (ReportsCSS == string.Empty || ReportsCSS == "")
                {
                    ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
                }
            }
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
                if (((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)) == null)
                {
                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                    continue;
                }

                if (selectedField.FieldKey == GingerReport.Fields.GingerExecutionStatus)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)).ToString() + "'>" + ((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)).ToString() + "</label></td>");
                }
                else if (selectedField.FieldKey == GingerReport.Fields.BusinessFlowsPassRate)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)) + " %</td>");
                }
                else if ((selectedField.FieldKey == GingerReport.Fields.StartTimeStamp) || (selectedField.FieldKey == GingerReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse((gr.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(gr)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else if ((selectedField.FieldKey == GingerReport.Fields.Elapsed))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(gr.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(gr).ToString()) + "</td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)) + "</td>");
                }

                if (selectedField.FieldKey == GingerReport.Fields.Name)
                {
                    currentGingerRunnerLinkText = ((GingerReport)gr).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((GingerReport)gr)).ToString();
                }
            }
            ReportHTML = ReportHTML.Replace("{GingerRunnerDetails_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{GingerRunnerDetails_Data}", fieldsValuesHTMLTableCells.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.GingerRunnerFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if (selectedField.FieldKey == GingerReport.Fields.ApplicationAgentsMapping)
                {
                    if (!selectedField.IsSelected)
                    {
                        string applicationAgentsMappingSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ApplicationAgentsMapping_Start-->", "<!--ApplicationAgentsMapping_End-->");
                        ReportHTML = ReportHTML.Replace(applicationAgentsMappingSection, "");
                    }
                    else
                    {
                        ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_ApplicationAgentsMapping-->",
                                                     ConvertingDatatableToHTML((DataTable)gr.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(gr), "Target Application - Agents Mapping", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                    }
                }
                else if (selectedField.FieldKey == GingerReport.Fields.BusinessFlowsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string businessFlowsDetailsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--BusinessFlowsDetails_Start-->", "<!--BusinessFlowsDetails_End-->");
                        ReportHTML = ReportHTML.Replace(businessFlowsDetailsSection, "");

                        foreach (BusinessFlowReport br in gr.BusinessFlowReports.OrderBy(x => x.Seq))
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                            {
                                string prevBusinessFlowSeq = string.Empty;
                                string nextBusinessFlowSeq = string.Empty;
                                string prevBusinessFlowName = string.Empty;
                                string nextBusinessFlowName = string.Empty;
                                if (gr.BusinessFlowReports.ElementAtOrDefault(gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1) != null)
                                {
                                    prevBusinessFlowName = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Name;
                                    prevBusinessFlowSeq = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Seq.ToString();
                                }
                                if (gr.BusinessFlowReports.ElementAtOrDefault(gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1) != null)
                                {
                                    nextBusinessFlowName = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Name;
                                    nextBusinessFlowSeq = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Seq.ToString();
                                }

                                CreateBusinessFlowLevelReport(br, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevBusinessFlowSeq, prevBusinessFlowName), new Tuple<string, string>(nextBusinessFlowSeq, nextBusinessFlowName)));
                            }
                        }
                    }
                    else
                    {
                        ReportHTML = ReportHTML.Replace("{PassPercent}", gr.BusinessFlowsPassRate.ToString());
                        ReportHTML = ReportHTML.Replace("{FailPercent}", gr.BusinessFlowsFailRate.ToString());
                        ReportHTML = ReportHTML.Replace("{StoppedPercent}", gr.BusinessFlowsStoppedRate.ToString());
                        ReportHTML = ReportHTML.Replace("{OtherPercent}", gr.BusinessFlowsOtherRate.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalPass}", gr.TotalBusinessFlowsPassed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalFail}", gr.TotalBusinessFlowsFailed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalStopped}", gr.TotalBusinessFlowsStopped.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalOther}", gr.TotalBusinessFlowsOther.ToString());
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                        {
                            fieldsNamesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                        }

                        foreach (BusinessFlowReport br in gr.BusinessFlowReports.OrderBy(x => x.Seq))
                        {
                            br.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                            fieldsValuesHTMLTableCells.Append("<tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                if (br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) == null)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                    continue;
                                }

                                if ((selectedField_internal.FieldKey == BusinessFlowReport.Fields.Name) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString()))
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(br.GetType().GetProperty(BusinessFlowReport.Fields.Seq).GetValue(br) + " " + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString())
                                                                                         + @"\BusinessFlowReport.html'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + @"</a></td>");
                                }
                                else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.PassPercent)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + " %</td>");
                                }
                                else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.RunStatus)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "'>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</label></td>");
                                }
                                else if ((selectedField_internal.FieldKey == BusinessFlowReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == BusinessFlowReport.Fields.EndTimeStamp))
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()).ToLocalTime().ToString() + "</td>");
                                }
                                else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Elapsed)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + " </td>");
                                }
                                else if (selectedField_internal.FieldKey == BusinessFlowReport.Fields.Seq)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><a class='accordion-toggle' data-toggle='collapse' href='#BusinessFlow" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "Collapse'><i class='ace-icon fa fa-angle-right bigger202' data-icon-hide='ace-icon fa fa-angle-down' data-icon-show='ace-icon fa fa-angle-right' style='padding:0px 5px 0px 0px;font-size:25px;color:coral'></i></a>" + br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) + "</td>");
                                    lastseq = br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString();
                                }
                                else
                                {
                                    if (br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br) != null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(br.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(br).ToString()) + "</td>");
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td></td>");
                                    }
                                }
                            }
                            fieldsValuesHTMLTableCells.Append("</tr>");
                            fieldsValuesHTMLTableCells.Append("<tr id='BusinessFlow" + lastseq + "Collapse' class='collapse'><td colspan='11' style='padding-left:40px'><table class='table table-striped table-bordered table-hover'>");
                            fieldsValuesHTMLTableCells.Append(" <thead class='table-space'><tr>");
                            lastbusinessflow = ExtensionMethods.folderNameNormalazing(br.Seq + " " + br.Name);
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                fieldsValuesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                            }
                            fieldsValuesHTMLTableCells.Append("</tr></thead>");
                            foreach (ActivityReport ac in br.Activities.OrderBy(x => x.Seq))
                            {
                                ac.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                                fieldsValuesHTMLTableCells.Append("<tr>");
                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) == null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                        continue;
                                    }

                                    if ((selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString()))
                                    {
                                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + lastbusinessflow + "\\" + ExtensionMethods.folderNameNormalazing(ac.GetType().GetProperty(ActivityReport.Fields.Seq).GetValue(ac) + " " + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())
                                                                                             + @"\ActivityReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "'>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</label></td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.PassPercent)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + " %</td>");
                                    }
                                    else if ((selectedField_internal.FieldKey == ActivityReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActivityReport.Fields.EndTimeStamp))
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()).ToLocalTime().ToString() + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + " </td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</td>");
                                        lastseq = ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString();
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.Description)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName)
                                    {
                                        if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) != null)
                                        {
                                            fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + lastbusinessflow + "\\" + @"ActivityGroups\" + ExtensionMethods.folderNameNormalazing(ac.ActivityGroupSeq + " " + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())) +
                                                                                          @"\ActivityGroupReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                        }
                                        else
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td></td>");
                                        }
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</td>");
                                    }
                                }
                            }
                            fieldsValuesHTMLTableCells.Append("</table></td></tr>");
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.GingerRunnerLevel.ToString())
                            {
                                string prevBusinessFlowSeq = string.Empty;
                                string nextBusinessFlowSeq = string.Empty;
                                string prevBusinessFlowName = string.Empty;
                                string nextBusinessFlowName = string.Empty;
                                if (gr.BusinessFlowReports.ElementAtOrDefault(gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1) != null)
                                {
                                    prevBusinessFlowName = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Name;
                                    prevBusinessFlowSeq = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) - 1].Seq.ToString();
                                }
                                if (gr.BusinessFlowReports.ElementAtOrDefault(gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1) != null)
                                {
                                    nextBusinessFlowName = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Name;
                                    nextBusinessFlowSeq = gr.BusinessFlowReports[gr.BusinessFlowReports.FindIndex(x => x.Seq == br.Seq) + 1].Seq.ToString();
                                }

                                CreateBusinessFlowLevelReport(br, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevBusinessFlowSeq, prevBusinessFlowName), new Tuple<string, string>(nextBusinessFlowSeq, nextBusinessFlowName)));
                            }
                        }
                    }
                    ReportHTML = ReportHTML.Replace("{BusinessFlowIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{BusinessFlowStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
            }

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", currentRunSetLinkText == string.Empty ? string.Empty : "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_delimiter}", currentRunSetLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");

            // handling Next/Prev Items Buttons
            if (nextPrevGingerName != null)
            {
                if ((nextPrevGingerName != null) && (nextPrevGingerName.Item2 != null) && (nextPrevGingerName.Item2.Item1.ToString() != string.Empty) && (nextPrevGingerName.Item2.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_next}", NextItemImage);
                    ReportHTML = ReportHTML.Replace("{Next_GingerFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevGingerName.Item2.Item1.ToString() + " " + nextPrevGingerName.Item2.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemNextToolTip}", "Next Ginger - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevGingerName.Item2.Item2.ToString()));
                }
                else
                {
                    string nextGingerLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_GingerLink_Start-->", "<!--Next_GingerLink_End-->");
                    ReportHTML = ReportHTML.Replace(nextGingerLink, "");
                }

                if ((nextPrevGingerName != null) && (nextPrevGingerName.Item1 != null) && (nextPrevGingerName.Item1.Item1.ToString() != string.Empty) && (nextPrevGingerName.Item1.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_prev}", PrevItemImage);
                    ReportHTML = ReportHTML.Replace("{Prev_GingerFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevGingerName.Item1.Item1.ToString() + " " + nextPrevGingerName.Item1.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemPrevToolTip}", "Previous Ginger - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevGingerName.Item1.Item2.ToString()));
                }
                else
                {
                    string prevGingerLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_GingerLink_Start-->", "<!--Prev_GingerLink_End-->");
                    ReportHTML = ReportHTML.Replace(prevGingerLink, "");
                }
            }
            else
            {
                string nextGingerLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_GingerLink_Start-->", "<!--Next_GingerLink_End-->");
                ReportHTML = ReportHTML.Replace(nextGingerLink, "");
                string prevGingerLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_GingerLink_Start-->", "<!--Prev_GingerLink_End-->");
                ReportHTML = ReportHTML.Replace(prevGingerLink, "");
            }

            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_Name}", currentGingerRunnerLinkText);
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", ReportLevel);

            ReportHTML = ReportHTML.Replace("{BusinessFlows_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{BusinessFlows_Data}", fieldsValuesHTMLTableCells.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            System.IO.File.WriteAllText(currentHTMLReportsFolder + "GingerRunnerReport.html", ReportHTML);

            gr = null;
            ReportHTML = null;
        }

        public void CreateBusinessFlowLevelReport(BusinessFlowReport BusinessFlowReport, string currentHTMLReportsFolder = "", string ReportLevel = "", bool calledAsRoot = false, Tuple<Tuple<string, string>, Tuple<string, string>> nextPrevBFName = null)
        {
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("BusinessFlowReport.html", TemplatesFolder);
            string ReportsCSS = string.Empty;
            string ReportJS = string.Empty;

            string lastseq = string.Empty;
            string lastActivity = string.Empty;
            if (calledAsRoot)
            {
                BusinessFlowReport.ExecutionLoggerIsEnabled = true;
                BusinessFlowReport.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(HTMLReportMainFolder.Replace("{name_to_replace}", ExtensionMethods.folderNameNormalazing(BusinessFlowReport.Name))
                                               .Replace("{date_to_replace}", DateTime.Now.ToString("MMddyyyy_HHmmss"))
                                               .Replace("{objectType_to_replace}", typeof(BusinessFlowReport).Name.ToString()));
                currentHTMLReportsFolder = HTMLReportMainFolder;
                ReportLevel = "./";
                StyleBundle = string.Empty;
                JSBundle = string.Empty;
                ReportsCSS = string.Empty;
                ReportJS = string.Empty;
                BeatLogo = string.Empty;
                CompanyLogo = string.Empty;
                GingerLogo = string.Empty;
            }
            else
            {
                currentHTMLReportsFolder = currentHTMLReportsFolder + ExtensionMethods.folderNameNormalazing(BusinessFlowReport.Seq + " " + BusinessFlowReport.Name);
            }
            System.IO.Directory.CreateDirectory(currentHTMLReportsFolder);

            if (StyleBundle == string.Empty || StyleBundle == "")
            {
                StyleBundle = CreateStylePath();
            }
            if (JSBundle == string.Empty || JSBundle == "")
            {
                JSBundle = CreateJavaScriptPath();
            }
            if (CompanyLogo == string.Empty || CompanyLogo == "")
            {
                CompanyLogo = CreateCompanyLogo();
            }
            if (GingerLogo == string.Empty || GingerLogo == "")
            {
                GingerLogo = CreateGingerLogo();
            }
            if (BeatLogo == string.Empty || BeatLogo == "")
            {
                BeatLogo = CreateBeatLogo();
            }
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (ReportJS == string.Empty || ReportJS == "")
            {
                ReportJS = ExtensionMethods.GetHTMLTemplate("circlechart.js", TemplatesFolder + "/assets/js/");
            }
            if (currentTemplate.UseLocalStoredStyling)
            {
                if (ReportsCSS == string.Empty || ReportsCSS == "")
                {
                    ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
                }
            }
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldPerecentHTMLTableCells = new StringBuilder();

            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
                if (((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)) == null)
                {
                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                    continue;
                }

                if (selectedField.FieldKey == BusinessFlowReport.Fields.RunStatus)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString() + "'>" + ((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString() + "</label></td>");
                }
                else if (selectedField.FieldKey == BusinessFlowReport.Fields.PassPercent)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString() + " %</td>");
                }
                else if ((selectedField.FieldKey == BusinessFlowReport.Fields.StartTimeStamp) || (selectedField.FieldKey == BusinessFlowReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else if (selectedField.FieldKey == BusinessFlowReport.Fields.Elapsed)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString()) + " </td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)) + "</td>");
                }

                if (selectedField.FieldKey == BusinessFlowReport.Fields.Name)
                {
                    currentBusinessFlowLinkText = ((BusinessFlowReport)BusinessFlowReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((BusinessFlowReport)BusinessFlowReport)).ToString();
                }
            }

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", currentRunSetLinkText == string.Empty ? string.Empty : "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_Name_Link}", currentGingerRunnerLinkText == string.Empty ? string.Empty : currentGingerRunnerLinkText);
            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_delimiter}", currentRunSetLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_delimiter}", currentGingerRunnerLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");

            // handling Next/Prev Items Buttons
            if (nextPrevBFName != null)
            {
                if ((nextPrevBFName != null) && (nextPrevBFName.Item2 != null) && (nextPrevBFName.Item2.Item1.ToString() != string.Empty) && (nextPrevBFName.Item2.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_next}", NextItemImage);
                    ReportHTML = ReportHTML.Replace("{Next_BusinessFlowFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevBFName.Item2.Item1.ToString() + " " + nextPrevBFName.Item2.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemNextToolTip}", "Next BusinessFlow - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevBFName.Item2.Item2.ToString()));
                }
                else
                {
                    string nextBusinessFlowLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_BusinessFlowLink_Start-->", "<!--Next_BusinessFlowLink_End-->");
                    ReportHTML = ReportHTML.Replace(nextBusinessFlowLink, "");
                }

                if ((nextPrevBFName != null) && (nextPrevBFName.Item1 != null) && (nextPrevBFName.Item1.Item1.ToString() != string.Empty) && (nextPrevBFName.Item1.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_prev}", PrevItemImage);
                    ReportHTML = ReportHTML.Replace("{Prev_BusinessFlowFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevBFName.Item1.Item1.ToString() + " " + nextPrevBFName.Item1.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemPrevToolTip}", "Previous Business Flow - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevBFName.Item1.Item2.ToString()));
                }
                else
                {
                    string prevBusinessFlowLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_BusinessFlowLink_Start-->", "<!--Prev_BusinessFlowLink_End-->");
                    ReportHTML = ReportHTML.Replace(prevBusinessFlowLink, "");
                }
            }
            else
            {
                string nextBusinessFlowLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_BusinessFlowLink_Start-->", "<!--Next_BusinessFlowLink_End-->");
                ReportHTML = ReportHTML.Replace(nextBusinessFlowLink, "");
                string prevBusinessFlowLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_BusinessFlowLink_Start-->", "<!--Prev_BusinessFlowLink_End-->");
                ReportHTML = ReportHTML.Replace(prevBusinessFlowLink, "");
            }

            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_Name}", currentBusinessFlowLinkText);
            ReportHTML = ReportHTML.Replace("{BusinessFlow_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{BusinessFlow_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", ReportLevel);
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            fieldPerecentHTMLTableCells.Remove(0, fieldPerecentHTMLTableCells.Length);
            // adding Sections
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.BusinessFlowFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if ((selectedField.FieldKey == BusinessFlowReport.Fields.SolutionVariablesDetails) && (selectedField.IsSelected == true))
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_SolutionVariablesDetails-->",
                                                     ConvertingDatatableToHTML((DataTable)BusinessFlowReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(BusinessFlowReport), "Solution Variables Details", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if ((selectedField.FieldKey == BusinessFlowReport.Fields.VariablesDetails) && (selectedField.IsSelected == true))
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_VariablesDetails-->",
                                                     ConvertingDatatableToHTML((DataTable)BusinessFlowReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(BusinessFlowReport), "Business Flow Variables Details", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if ((selectedField.FieldKey == BusinessFlowReport.Fields.BFFlowControlDT) && (selectedField.IsSelected == true))
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_FlowControl-->",
                                                     ConvertingDatatableToHTML((DataTable)BusinessFlowReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(BusinessFlowReport), "Business Flow Control", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if (selectedField.FieldKey == BusinessFlowReport.Fields.ActivityDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string activitiesDetailsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ActivitiesDetails_Start-->", "<!--ActivitiesDetails_End-->");
                        ReportHTML = ReportHTML.Replace(activitiesDetailsSection, "");

                        foreach (ActivityReport ac in BusinessFlowReport.Activities.OrderBy(x => x.Seq))
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString())
                            {
                                string prevActivitySeq = string.Empty;
                                string nextActivitySeq = string.Empty;
                                string prevActivityName = string.Empty;
                                string nextActivityName = string.Empty;
                                if (BusinessFlowReport.Activities.ElementAtOrDefault(BusinessFlowReport.Activities.IndexOf(ac) - 1) != null)
                                {
                                    prevActivityName = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) - 1].ActivityName;
                                    prevActivitySeq = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) - 1].Seq.ToString();
                                }
                                if (BusinessFlowReport.Activities.ElementAtOrDefault(BusinessFlowReport.Activities.IndexOf(ac) + 1) != null)
                                {
                                    nextActivityName = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) + 1].ActivityName;
                                    nextActivitySeq = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) + 1].Seq.ToString();
                                }

                                CreateActivityLevelReport(ac, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitySeq, prevActivityName), new Tuple<string, string>(nextActivitySeq, nextActivityName)));
                            }
                        }
                    }
                    else
                    {
                        ReportHTML = ReportHTML.Replace("{PassPercent}", BusinessFlowReport.PassPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{FailPercent}", BusinessFlowReport.FailPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{StoppedPercent}", BusinessFlowReport.StoppedPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{OtherPercent}", BusinessFlowReport.OtherPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalPass}", BusinessFlowReport.TotalActivitiesPassed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalFail}", BusinessFlowReport.TotalActivitiesFailed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalStopped}", BusinessFlowReport.TotalActivitiesStopped.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalOther}", BusinessFlowReport.TotalActivitiesOther.ToString());

                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                        {
                            fieldsNamesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                        }
                        foreach (ActivityReport ac in BusinessFlowReport.Activities.OrderBy(x => x.Seq))
                        {
                            ac.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                            fieldsValuesHTMLTableCells.Append("<tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) == null)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                    continue;
                                }

                                if ((selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityGroupLevel.ToString()))
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + @"ActivityGroups\" + ExtensionMethods.folderNameNormalazing(ac.ActivityGroupSeq + " " + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())) +
                                                                                           @"\ActivityGroupReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                }
                                else if ((selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString()))
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(ac.GetType().GetProperty(ActivityReport.Fields.Seq).GetValue(ac) + " " + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())
                                                                                         + @"\ActivityReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "'>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</label></td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.PassPercent)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + " %</td>");
                                }
                                else if ((selectedField_internal.FieldKey == ActivityReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActivityReport.Fields.EndTimeStamp))
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()).ToLocalTime().ToString() + "</td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + " </td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><a class='accordion-toggle' data-toggle='collapse' href='#Activity" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "Collapse'><i class='ace-icon fa fa-angle-right bigger202' data-icon-hide='ace-icon fa fa-angle-down' data-icon-show='ace-icon fa fa-angle-right' style='padding:0px 5px 0px 0px;font-size:25px;color:coral'></i></a>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</td>");
                                    lastseq = ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString();
                                }
                                else
                                {
                                    if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) != null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td></td>");
                                    }
                                }
                            }
                            lastActivity = ExtensionMethods.folderNameNormalazing(ac.Seq + " " + ac.ActivityName);
                            fieldsValuesHTMLTableCells.Append("</tr>");
                            fieldsValuesHTMLTableCells.Append("<tr id='Activity" + lastseq + "Collapse' class='collapse'><td colspan='10' style='padding-left:40px'><table class='table table-striped table-bordered table-hover'>");
                            fieldsValuesHTMLTableCells.Append("<thead class='table-space'><tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                fieldsValuesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                            }
                            fieldsValuesHTMLTableCells.Append("</tr></thead>");
                            foreach (ActionReport act in ac.ActionReports.OrderBy(x => x.Seq))
                            {
                                fieldsValuesHTMLTableCells.Append("<tr>");
                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) == null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                        continue;
                                    }

                                    if ((selectedField_internal.FieldKey == ActionReport.Fields.Description) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString()))
                                    {
                                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + lastActivity + "\\" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString())
                                                                                             + @"\ActionReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + @"</a></td>");
                                        currentActionNameText = act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString();
                                    }
                                    else if (selectedField_internal.FieldKey == ActionReport.Fields.Status)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td><label class='Status" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "'>" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "</label></td>");
                                    }
                                    else if ((selectedField_internal.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActionReport.Fields.EndTimeStamp))
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()).ToLocalTime().ToString() + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActionReport.Fields.ScreenShot)
                                    {
                                        try
                                        {
                                            int ScreenshotCount = 0;
                                            foreach (string txt_file in System.IO.Directory.GetFiles(act.LogFolder))
                                            {
                                                string fileName = System.IO.Path.GetFileName(txt_file);
                                                if (fileName.Contains("ScreenShot_"))
                                                {
                                                    ScreenshotCount++;
                                                }
                                            }

                                            Tuple<int, int> sizesPreview = General.RecalculatingSizeWithKeptRatio(General.GetImageHeightWidth(act.LogFolder + @"\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png"), screenShotSampleWidth, screenShotSampleHight);
                                            string id_str = @"ScreenShot_" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + act.Seq.ToString() + "_" + ScreenshotCount;
                                            fieldsValuesHTMLTableCells.Append(@"<td align='center'><img style='display:block;' src='" + @".\" + lastActivity + "\\" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + @"\Screenshots\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png' alt='" + act.Description + " - Action - Screenshot" + "' width='" + sizesPreview.Item1.ToString() + "' height='" + sizesPreview.Item2.ToString() + "' id='" + id_str + "' onclick='show_modal(\"" + id_str + "\")'></img></td>");
                                        }
                                        catch
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td></td>");
                                        }
                                    }
                                    else
                                    {
                                        if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) != null)
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                        }
                                        else
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td></td>");
                                        }
                                    }
                                }
                                fieldsValuesHTMLTableCells.Append("</tr>");
                            }
                            fieldsValuesHTMLTableCells.Append("</table></td></tr>");
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.BusinessFlowLevel.ToString())
                            {
                                string prevActivitySeq = string.Empty;
                                string nextActivitySeq = string.Empty;
                                string prevActivityName = string.Empty;
                                string nextActivityName = string.Empty;
                                if (BusinessFlowReport.Activities.ElementAtOrDefault(BusinessFlowReport.Activities.IndexOf(ac) - 1) != null)
                                {
                                    prevActivityName = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) - 1].ActivityName;
                                    prevActivitySeq = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) - 1].Seq.ToString();
                                }
                                if (BusinessFlowReport.Activities.ElementAtOrDefault(BusinessFlowReport.Activities.IndexOf(ac) + 1) != null)
                                {
                                    nextActivityName = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) + 1].ActivityName;
                                    nextActivitySeq = BusinessFlowReport.Activities[BusinessFlowReport.Activities.IndexOf(ac) + 1].Seq.ToString();
                                }

                                CreateActivityLevelReport(ac, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitySeq, prevActivityName), new Tuple<string, string>(nextActivitySeq, nextActivityName)));
                            }
                        }
                    }
                    ReportHTML = ReportHTML.Replace("{ActivitiesIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{ActivitiesStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
            }

            if (BusinessFlowReport.ActivitiesGroupReports != null)
            {
                foreach (ActivityGroupReport ag in BusinessFlowReport.ActivitiesGroupReports.OrderBy(x => x.Seq))
                {
                    if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityGroupLevel.ToString())
                    {
                        string prevActivitiesGroupSeq = string.Empty;
                        string nextActivitiesGroupSeq = string.Empty;
                        string prevActivitiesGroupName = string.Empty;
                        string nextActivitiesGroupName = string.Empty;
                        if (BusinessFlowReport.ActivitiesGroupReports.ElementAtOrDefault(BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1) != null)
                        {
                            prevActivitiesGroupName = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1].Name;
                            prevActivitiesGroupSeq = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1].Seq.ToString();
                        }
                        if (BusinessFlowReport.ActivitiesGroupReports.ElementAtOrDefault(BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1) != null)
                        {
                            nextActivitiesGroupName = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1].Name;
                            nextActivitiesGroupSeq = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1].Seq.ToString();
                        }

                        CreateActivityGroupLevelReport(ag, BusinessFlowReport, currentHTMLReportsFolder, ReportLevel + "../../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitiesGroupSeq, prevActivitiesGroupName), new Tuple<string, string>(nextActivitiesGroupSeq, nextActivitiesGroupName)));
                    }
                }
            }

            ReportHTML = ReportHTML.Replace("{Activities_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{Activities_Data}", fieldsValuesHTMLTableCells.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            // Save the HTML            
            string FileName = currentHTMLReportsFolder + @"\BusinessFlowReport.html";
            System.IO.File.WriteAllText(FileName, ReportHTML);

            BusinessFlowReport = null;
            ReportHTML = null;
        }

        public void CreateActivitiesGroupReportsOfBusinessFlow(BusinessFlowReport BusinessFlowReport, ref BusinessFlow BF, string currentHTMLReportsFolder = "", string ReportLevel = "", bool calledAsRoot = false)
        {
            if (BusinessFlowReport.ActivitiesGroupReports != null)
            {
                BusinessFlowReport.ExecutionLoggerIsEnabled = true;
                foreach (ActivityGroupReport ag in BusinessFlowReport.ActivitiesGroupReports.OrderBy(x => x.Seq))
                {
                    if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityGroupLevel.ToString())
                    {
                        HTMLReportMainFolder = currentHTMLReportsFolder + @"\" + ExtensionMethods.folderNameNormalazing(ag.Seq + " " + ag.Name);

                        string prevActivitiesGroupSeq = string.Empty;
                        string nextActivitiesGroupSeq = string.Empty;
                        string prevActivitiesGroupName = string.Empty;
                        string nextActivitiesGroupName = string.Empty;
                        if (BusinessFlowReport.ActivitiesGroupReports.ElementAtOrDefault(BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1) != null)
                        {
                            prevActivitiesGroupName = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1].Name;
                            prevActivitiesGroupSeq = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) - 1].Seq.ToString();
                        }
                        if (BusinessFlowReport.ActivitiesGroupReports.ElementAtOrDefault(BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1) != null)
                        {
                            nextActivitiesGroupName = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1].Name;
                            nextActivitiesGroupSeq = BusinessFlowReport.ActivitiesGroupReports[BusinessFlowReport.ActivitiesGroupReports.FindIndex(x => x.GUID == ag.GUID) + 1].Seq.ToString();
                        }

                        CreateActivityGroupLevelReport(ag, BusinessFlowReport, currentHTMLReportsFolder, ReportLevel + "../../", true, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitiesGroupSeq, prevActivitiesGroupName), new Tuple<string, string>(nextActivitiesGroupSeq, nextActivitiesGroupName)));

                        try
                        {
                            BF.ActivitiesGroups.Where(x => x.Guid.ToString() == ag.SourceGuid).FirstOrDefault().TempReportFolder = currentHTMLReportsFolder + @"\" + ExtensionMethods.folderNameNormalazing(ag.Seq + " " + ag.Name);
                        }
                        catch { }
                        foreach (ActivityReport ac in BusinessFlowReport.Activities.Where(x => ag.ExecutedActivitiesGUID.Select(y => y.ToString()).Contains(x.SourceGuid)).OrderBy(x => x.Seq))
                        {
                            CreateActivityLevelReport(ac, currentHTMLReportsFolder + @"\" + ExtensionMethods.folderNameNormalazing(ag.Seq + " " + ag.Name), ReportLevel + "../");
                        }
                    }
                }
            }
        }

        public void CreateActivityGroupLevelReport(ActivityGroupReport ActivityGroupReport, BusinessFlowReport BusinessFlowReport, string currentHTMLReportsFolder = "", string ReportLevel = "", bool calledAsRoot = false, Tuple<Tuple<string, string>, Tuple<string, string>> nextPrevActivitiesGroupName = null)
        {
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("ActivityGroupReport.html", TemplatesFolder);
            string ReportsCSS = string.Empty;
            string ReportJS = string.Empty;

            string lastseq = string.Empty;
            string lastActivity = string.Empty;
            if (calledAsRoot)
            {
                ActivityGroupReport.ExecutionLoggerIsEnabled = true;
                HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(HTMLReportMainFolder.Replace("{name_to_replace}", ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Seq + " " + ActivityGroupReport.Name))
                                               .Replace("{date_to_replace}", DateTime.Now.ToString("MMddyyyy_HHmmss"))
                                               .Replace("{objectType_to_replace}", typeof(ActivityGroupReport).Name.ToString()));
                currentHTMLReportsFolder = HTMLReportMainFolder;
                ReportLevel = "./";
                StyleBundle = string.Empty;
                JSBundle = string.Empty;
                ReportsCSS = string.Empty;
                ReportJS = string.Empty;
                BeatLogo = string.Empty;
                CompanyLogo = string.Empty;
                GingerLogo = string.Empty;
            }
            else
            {
                currentHTMLReportsFolder = currentHTMLReportsFolder + @"\ActivityGroups\" + ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Seq + " " + ActivityGroupReport.Name);
            }
            System.IO.Directory.CreateDirectory(currentHTMLReportsFolder);

            if (StyleBundle == string.Empty || StyleBundle == "")
            {
                StyleBundle = CreateStylePath();
            }
            if (JSBundle == string.Empty || JSBundle == "")
            {
                JSBundle = CreateJavaScriptPath();
            }
            if (CompanyLogo == string.Empty || CompanyLogo == "")
            {
                CompanyLogo = CreateCompanyLogo();
            }
            if (GingerLogo == string.Empty || GingerLogo == "")
            {
                GingerLogo = CreateGingerLogo();
            }
            if (BeatLogo == string.Empty || BeatLogo == "")
            {
                BeatLogo = CreateBeatLogo();
            }
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (ReportJS == string.Empty || ReportJS == "")
            {
                ReportJS = ExtensionMethods.GetHTMLTemplate("circlechart.js", TemplatesFolder + "/assets/js/");
            }
            if (currentTemplate.UseLocalStoredStyling)
            {
                if (ReportsCSS == string.Empty || ReportsCSS == "")
                {
                    ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
                }
            }
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldPerecentHTMLTableCells = new StringBuilder();

            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActivityGroupFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
                if (((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)) == null)
                {
                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                    continue;
                }

                if (selectedField.FieldKey == ActivityGroupReport.Fields.RunStatus)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString() + "'>" + ((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString() + "</label></td>");
                }
                else if (selectedField.FieldKey == ActivityGroupReport.Fields.PassPercent)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString() + " %</td>");
                }
                else if ((selectedField.FieldKey == ActivityGroupReport.Fields.StartTimeStamp) || (selectedField.FieldKey == ActivityGroupReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else if (selectedField.FieldKey == ActivityGroupReport.Fields.Elapsed)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString()) + " </td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)) + "</td>");
                }

                if (selectedField.FieldKey == ActivityGroupReport.Fields.Name)
                {
                    currentActivityGroupLinkText = ((ActivityGroupReport)ActivityGroupReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityGroupReport)ActivityGroupReport)).ToString();
                }
            }

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", currentRunSetLinkText == string.Empty ? string.Empty : "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_Name_Link}", currentGingerRunnerLinkText == string.Empty ? string.Empty : currentGingerRunnerLinkText);
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_Name_Link}", currentBusinessFlowLinkText == string.Empty ? string.Empty : currentBusinessFlowLinkText);

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_delimiter}", currentRunSetLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_delimiter}", currentGingerRunnerLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_delimiter}", currentBusinessFlowLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");

            // handling Next/Prev Items Buttons
            if (nextPrevActivitiesGroupName != null)
            {
                if ((nextPrevActivitiesGroupName != null) && (nextPrevActivitiesGroupName.Item2 != null) && (nextPrevActivitiesGroupName.Item2.Item1.ToString() != string.Empty) && (nextPrevActivitiesGroupName.Item2.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_next}", NextItemImage);
                    ReportHTML = ReportHTML.Replace("{Next_ActivitiesGroupFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActivitiesGroupName.Item2.Item1.ToString() + " " + nextPrevActivitiesGroupName.Item2.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemNextToolTip}", "Next ActivitiesGroup - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActivitiesGroupName.Item2.Item2.ToString()));
                }
                else
                {
                    string nextActivitiesGroupLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActivitiesGroupLink_Start-->", "<!--Next_ActivitiesGroupLink_End-->");
                    ReportHTML = ReportHTML.Replace(nextActivitiesGroupLink, "");
                }

                if ((nextPrevActivitiesGroupName != null) && (nextPrevActivitiesGroupName.Item1 != null) && (nextPrevActivitiesGroupName.Item1.Item1.ToString() != string.Empty) && (nextPrevActivitiesGroupName.Item1.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_prev}", PrevItemImage);
                    ReportHTML = ReportHTML.Replace("{Prev_ActivitiesGroupFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActivitiesGroupName.Item1.Item1.ToString() + " " + nextPrevActivitiesGroupName.Item1.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemPrevToolTip}", "Previous Activities Group - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActivitiesGroupName.Item1.Item2.ToString()));
                }
                else
                {
                    string prevActivitiesGroupLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActivitiesGroupLink_Start-->", "<!--Prev_ActivitiesGroupLink_End-->");
                    ReportHTML = ReportHTML.Replace(prevActivitiesGroupLink, "");
                }
            }
            else
            {
                string nextActivitiesGroupLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActivitiesGroupLink_Start-->", "<!--Next_ActivitiesGroupLink_End-->");
                ReportHTML = ReportHTML.Replace(nextActivitiesGroupLink, "");
                string prevActivitiesGroupLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActivitiesGroupLink_Start-->", "<!--Prev_ActivitiesGroupLink_End-->");
                ReportHTML = ReportHTML.Replace(prevActivitiesGroupLink, "");
            }

            ReportHTML = ReportHTML.Replace("{Parent_ActivityGroup_Name}", currentActivityGroupLinkText);
            ReportHTML = ReportHTML.Replace("{ActivityGroup_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{ActivityGroup_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", ReportLevel);
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            fieldPerecentHTMLTableCells.Remove(0, fieldPerecentHTMLTableCells.Length);

            // adding Sections
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActivityGroupFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if (selectedField.FieldKey == ActivityGroupReport.Fields.ActivityDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string activitiesDetailsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ActivitiesDetails_Start-->", "<!--ActivitiesDetails_End-->");
                        ReportHTML = ReportHTML.Replace(activitiesDetailsSection, "");
                    }
                    else
                    {
                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                        {
                            if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName)
                            {
                                // do nothing
                            }
                            else
                            {
                                fieldsNamesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                            }
                        }

                        foreach (ActivityReport ac in BusinessFlowReport.Activities.Where(x => ActivityGroupReport.ExecutedActivitiesGUID.Select(y => y.ToString()).Contains(x.SourceGuid)).OrderBy(x => x.Seq))
                        {
                            ac.AllIterationElements = currentTemplate.ShowAllIterationsElements;

                            fieldsValuesHTMLTableCells.Append("<tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldName != "ScreenShot" && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) == null)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                    continue;
                                }

                                if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityGroupName)
                                {
                                    // do nothing
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.ActivityName)
                                {
                                    if (calledAsRoot)
                                    {
                                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(ac.GetType().GetProperty(ActivityReport.Fields.Seq).GetValue(ac) + " " + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())
                                                                                             + @"\ActivityReport.html?Parent_ActivitiesGroup_Folder_Name=" + ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Seq + " " + ActivityGroupReport.Name)
                                                                                                                   + "&Parent_ActivitiesGroup_Name=" + ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Name)
                                                                                                                   + "&ActivitiesGroupCalledAsRoot=true" + "'>"
                                                                                                                   + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\..\..\" + ExtensionMethods.folderNameNormalazing(ac.GetType().GetProperty(ActivityReport.Fields.Seq).GetValue(ac) + " " + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString())
                                                                                         + @"\ActivityReport.html?Parent_ActivitiesGroup_Folder_Name=" + ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Seq + " " + ActivityGroupReport.Name)
                                                                                                                   + "&Parent_ActivitiesGroup_Name=" + ExtensionMethods.folderNameNormalazing(ActivityGroupReport.Name)
                                                                                                                   + "&ActivitiesGroupCalledAsRoot=false" + "'>"
                                                                                                                   + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + @"</a></td>");
                                    }
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.RunStatus)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "'>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</label></td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.PassPercent)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + " %</td>");
                                }
                                else if ((selectedField_internal.FieldKey == ActivityReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActivityReport.Fields.EndTimeStamp))
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()).ToLocalTime().ToString() + "</td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.ElapsedSecs)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + " </td>");
                                }
                                else if (selectedField_internal.FieldKey == ActivityReport.Fields.Seq)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><a class='accordion-toggle' data-toggle='collapse' href='#Activity" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "Collapse'><i class='ace-icon fa fa-angle-right bigger202' data-icon-hide='ace-icon fa fa-angle-down' data-icon-show='ace-icon fa fa-angle-right' style='padding:0px 5px 0px 0px;font-size:25px;color:coral'></i></a>" + ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) + "</td>");
                                    lastseq = ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString();
                                }
                                else
                                {
                                    if (ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac) != null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(ac.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(ac).ToString()) + "</td>");
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td></td>");
                                    }
                                }
                            }
                            lastActivity = ExtensionMethods.folderNameNormalazing(ac.Seq + " " + ac.ActivityName);
                            fieldsValuesHTMLTableCells.Append("</tr>");
                            fieldsValuesHTMLTableCells.Append("<tr id='Activity" + lastseq + "Collapse' class='collapse'><td colspan='10' style='padding-left:40px'><table class='table table-striped table-bordered table-hover'>");
                            fieldsValuesHTMLTableCells.Append("<thead class='table-space'><tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                fieldsValuesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                            }
                            fieldsValuesHTMLTableCells.Append("</tr></thead>");
                            foreach (ActionReport act in ac.ActionReports.OrderBy(x => x.Seq))
                            {
                                fieldsValuesHTMLTableCells.Append("<tr>");
                                foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                                {
                                    if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) == null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                        continue;
                                    }

                                    if ((selectedField_internal.FieldKey == ActionReport.Fields.Description) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString()))
                                    {
                                        fieldsValuesHTMLTableCells.Append(@"<td><a href='.\..\..\" + lastActivity + "\\" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString())
                                                                                             + @"\ActionReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + @"</a></td>");
                                        currentActionNameText = act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString();
                                    }
                                    else if (selectedField_internal.FieldKey == ActionReport.Fields.Status)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td><label class='Status" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "'>" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "</label></td>");
                                    }
                                    else if ((selectedField_internal.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActionReport.Fields.EndTimeStamp))
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()).ToLocalTime().ToString() + "</td>");
                                    }
                                    else if (selectedField_internal.FieldKey == ActionReport.Fields.ScreenShot)
                                    {
                                        try
                                        {
                                            int ScreenshotCount = 0;
                                            foreach (string txt_file in System.IO.Directory.GetFiles(act.LogFolder))
                                            {
                                                string fileName = System.IO.Path.GetFileName(txt_file);
                                                if (fileName.Contains("ScreenShot_"))
                                                {
                                                    ScreenshotCount++;
                                                }
                                            }
                                            Tuple<int, int> sizesPreview = General.RecalculatingSizeWithKeptRatio(General.GetImageHeightWidth(act.LogFolder + @"\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png"), screenShotSampleWidth, screenShotSampleHight);
                                            string id_str = @"ScreenShot_" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + act.Seq.ToString() + "_" + ScreenshotCount;
                                            fieldsValuesHTMLTableCells.Append(@"<td align='center'><img style='display:block;' src='" + @"..\..\" + lastActivity + "\\" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + @"\Screenshots\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png' alt='" + act.Description + " - Action - Screenshot" + "' width='" + sizesPreview.Item1.ToString() + "' height='" + sizesPreview.Item2.ToString() + "' id='" + id_str + "' onclick='show_modal(\"" + id_str + "\")'></img></td>");
                                        }
                                        catch
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td></td>");
                                        }
                                    }
                                    else
                                    {
                                        if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) != null)
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                        }
                                        else
                                        {
                                            fieldsValuesHTMLTableCells.Append("<td></td>");
                                        }
                                    }
                                }
                                fieldsValuesHTMLTableCells.Append("</tr>");
                            }
                            fieldsValuesHTMLTableCells.Append("</table></td></tr>");
                        }
                    }
                    ReportHTML = ReportHTML.Replace("{ActivitiesIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{ActivitiesStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
            }

            ReportHTML = ReportHTML.Replace("{Activities_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{Activities_Data}", fieldsValuesHTMLTableCells.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            // Save the HTML            
            string FileName = currentHTMLReportsFolder + @"\ActivityGroupReport.html";
            System.IO.File.WriteAllText(FileName, ReportHTML);

            BusinessFlowReport = null;
            ReportHTML = null;
        }

        public void CreateActivityLevelReport(ActivityReport ActivityReport, string currentHTMLReportsFolder = "", string ReportLevel = "", bool calledAsRoot = false, Tuple<Tuple<string, string>, Tuple<string, string>> nextPrevActivityName = null)
        {
            // read template
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("ActivityReport.html", TemplatesFolder);
            string ReportsCSS = string.Empty;
            string ReportJS = string.Empty;

            if (calledAsRoot)
            {
                ActivityReport.AllIterationElements = currentTemplate.ShowAllIterationsElements;
                HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(HTMLReportMainFolder.Replace("{name_to_replace}", ExtensionMethods.folderNameNormalazing(ActivityReport.ActivityName))
                                                     .Replace("{date_to_replace}", DateTime.Now.ToString("MMddyyyy_HHmmss"))
                                                     .Replace("{objectType_to_replace}", typeof(ActivityReport).Name.ToString()));
                currentHTMLReportsFolder = HTMLReportMainFolder;
                ReportLevel = "./";
                StyleBundle = string.Empty;
                JSBundle = string.Empty;
                ReportsCSS = string.Empty;
                ReportJS = string.Empty;
                BeatLogo = string.Empty;
                PrevItemImage = string.Empty;
                NextItemImage = string.Empty;
                CompanyLogo = string.Empty;
                GingerLogo = string.Empty;
            }
            else
            {
                currentHTMLReportsFolder = currentHTMLReportsFolder + @"\" + ExtensionMethods.folderNameNormalazing(ActivityReport.Seq + " " + ActivityReport.ActivityName);
            }
            System.IO.Directory.CreateDirectory(currentHTMLReportsFolder);

            if (StyleBundle == string.Empty || StyleBundle == "")
            {
                StyleBundle = CreateStylePath();
            }
            if (JSBundle == string.Empty || JSBundle == "")
            {
                JSBundle = CreateJavaScriptPath();
            }
            if (CompanyLogo == string.Empty || CompanyLogo == "")
            {
                CompanyLogo = CreateCompanyLogo();
            }
            if (GingerLogo == string.Empty || GingerLogo == "")
            {
                GingerLogo = CreateGingerLogo();
            }
            if (BeatLogo == string.Empty || BeatLogo == "")
            {
                BeatLogo = CreateBeatLogo();
            }
            if (PrevItemImage == string.Empty || PrevItemImage == "")
            {
                PrevItemImage = CreateItemPrevImage();
            }
            if (NextItemImage == string.Empty || NextItemImage == "")
            {
                NextItemImage = CreateItemNextImage();
            }

            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (ReportJS == string.Empty || ReportJS == "")
            {
                ReportJS = ExtensionMethods.GetHTMLTemplate("circlechart.js", TemplatesFolder + "/assets/js/");
            }
            if (currentTemplate.UseLocalStoredStyling)
            {
                if (ReportsCSS == string.Empty || ReportsCSS == "")
                {
                    ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
                }
            }
            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            StringBuilder fieldPerecentHTMLTableCells = new StringBuilder();

            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActivityFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
                if (((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue((ActivityReport)ActivityReport) == null)
                {
                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                    continue;
                }

                if (selectedField.FieldKey == ActivityReport.Fields.RunStatus)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString() + "'>" + ((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString() + "</label></td>");
                }
                else if (selectedField.FieldKey == ActivityReport.Fields.PassPercent)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)) + " %</td>");
                }
                else if ((selectedField.FieldKey == ActivityReport.Fields.StartTimeStamp) || (selectedField.FieldKey == ActivityReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else if (selectedField.FieldKey == ActivityReport.Fields.ElapsedSecs)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + General.TimeConvert(((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString()) + " </td>");
                }
                else if (selectedField.FieldKey == ActivityReport.Fields.ActivityName)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString()) + "</td>");
                }
                else if (selectedField.FieldKey == ActivityReport.Fields.Description)
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)))) + "</td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)) + "</td>");
                }

                if (selectedField.FieldKey == ActivityReport.Fields.ActivityName)
                {
                    currentActivityLinkText = ExtensionMethods.OverrideHTMLRelatedCharacters(((ActivityReport)ActivityReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActivityReport)ActivityReport)).ToString());
                }
            }

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", currentRunSetLinkText == string.Empty ? string.Empty : "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_Name_Link}", currentGingerRunnerLinkText == string.Empty ? string.Empty : currentGingerRunnerLinkText);
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_Name_Link}", currentBusinessFlowLinkText == string.Empty ? string.Empty : currentBusinessFlowLinkText);

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_delimiter}", currentRunSetLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_delimiter}", currentGingerRunnerLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_delimiter}", currentBusinessFlowLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");

            // handling Next/Prev Items Buttons
            if (nextPrevActivityName != null)
            {
                if ((nextPrevActivityName != null) && (nextPrevActivityName.Item2 != null) && (nextPrevActivityName.Item2.Item1.ToString() != string.Empty) && (nextPrevActivityName.Item2.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_next}", NextItemImage);
                    ReportHTML = ReportHTML.Replace("{Next_ActivityFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActivityName.Item2.Item1.ToString() + " " + nextPrevActivityName.Item2.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemNextToolTip}", "Next Activity - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActivityName.Item2.Item2.ToString()));
                }
                else
                {
                    string nextActivityLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActivityLink_Start-->", "<!--Next_ActivityLink_End-->");
                    ReportHTML = ReportHTML.Replace(nextActivityLink, "");
                }

                if ((nextPrevActivityName != null) && (nextPrevActivityName.Item1 != null) && (nextPrevActivityName.Item1.Item1.ToString() != string.Empty) && (nextPrevActivityName.Item1.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_prev}", PrevItemImage);
                    ReportHTML = ReportHTML.Replace("{Prev_ActivityFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActivityName.Item1.Item1.ToString() + " " + nextPrevActivityName.Item1.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemPrevToolTip}", "Previous Activity - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActivityName.Item1.Item2.ToString()));
                }
                else
                {
                    string prevActivityLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActivityLink_Start-->", "<!--Prev_ActivityLink_End-->");
                    ReportHTML = ReportHTML.Replace(prevActivityLink, "");
                }
            }
            else
            {
                string nextActivityLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActivityLink_Start-->", "<!--Next_ActivityLink_End-->");
                ReportHTML = ReportHTML.Replace(nextActivityLink, "");
                string prevActivityLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActivityLink_Start-->", "<!--Prev_ActivityLink_End-->");
                ReportHTML = ReportHTML.Replace(prevActivityLink, "");
            }

            ReportHTML = ReportHTML.Replace("{Parent_Activity_Name}", currentActivityLinkText);
            ReportHTML = ReportHTML.Replace("{Activity_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{Activity_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", ReportLevel);
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            fieldPerecentHTMLTableCells.Remove(0, fieldPerecentHTMLTableCells.Length);

            // adding Sections
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActivityFieldsToSelect.Where(x => (x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if ((selectedField.FieldKey == ActivityReport.Fields.VariablesDetails) && (selectedField.IsSelected == true))
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_VariablesDetails-->",
                                                     ConvertingDatatableToHTML((DataTable)ActivityReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(ActivityReport), "Activity Variables Details", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if (selectedField.FieldKey == ActivityReport.Fields.ActionsDetails)
                {
                    if (!selectedField.IsSelected)
                    {
                        string activitiesDetailsSection = ExtensionMethods.GetStringBetween(ReportHTML, "<!--ActionsDetails_Start-->", "<!--ActionsDetails_End-->");
                        ReportHTML = ReportHTML.Replace(activitiesDetailsSection, "");

                        foreach (ActionReport act in ActivityReport.ActionReports.OrderBy(x => x.Seq))
                        {
                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString())
                            {
                                string prevActivitySeq = string.Empty;
                                string nextActivitySeq = string.Empty;
                                string prevActivityName = string.Empty;
                                string nextActivityName = string.Empty;
                                if (ActivityReport.ActionReports.ElementAtOrDefault(ActivityReport.ActionReports.IndexOf(act) - 1) != null)
                                {
                                    prevActivityName = ActivityReport.ActionReports[ActivityReport.ActionReports.IndexOf(act) - 1].Name;
                                    prevActivitySeq = ActivityReport.ActionReports[ActivityReport.ActionReports.IndexOf(act) - 1].Seq.ToString();
                                }
                                if (ActivityReport.ActionReports.ElementAtOrDefault(ActivityReport.ActionReports.IndexOf(act) + 1) != null)
                                {
                                    nextActivityName = ActivityReport.ActionReports[ActivityReport.ActionReports.IndexOf(act) + 1].Name;
                                    nextActivitySeq = ActivityReport.ActionReports[ActivityReport.ActionReports.IndexOf(act) + 1].Seq.ToString();
                                }

                                CreateActionLevelReport(act, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitySeq, prevActivityName), new Tuple<string, string>(nextActivitySeq, nextActivityName)));
                            }
                        }
                    }
                    else
                    {
                        ReportHTML = ReportHTML.Replace("{PassPercent}", ActivityReport.PassPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{FailPercent}", ActivityReport.FailPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{StoppedPercent}", ActivityReport.StoppedPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{OtherPercent}", ActivityReport.OtherPercent.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalPass}", ActivityReport.TotalActionsPassed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalFail}", ActivityReport.TotalActionsFailed.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalStopped}", ActivityReport.TotalActionsStopped.ToString());
                        ReportHTML = ReportHTML.Replace("{TotalOther}", ActivityReport.TotalActionsOther.ToString());

                        fieldsNamesHTMLTableCells = new StringBuilder();
                        fieldsValuesHTMLTableCells = new StringBuilder();
                        foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                        {
                            fieldsNamesHTMLTableCells.Append("<td>" + selectedField_internal.FieldName + "</td>");
                        }
                        foreach (ActionReport act in ActivityReport.ActionReports.OrderBy(x => x.Seq))
                        {
                            fieldsValuesHTMLTableCells.Append("<tr>");
                            foreach (HTMLReportConfigFieldToSelect selectedField_internal in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
                            {
                                if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) == null)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                                    continue;
                                }

                                if ((selectedField_internal.FieldKey == ActionReport.Fields.Description) && (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString()))
                                {
                                    fieldsValuesHTMLTableCells.Append(@"<td><a href='.\" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString())
                                                                                         + @"\ActionReport.html'>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + @"</a></td>");

                                    currentActionNameText = ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString());
                                }
                                else if (selectedField_internal.FieldKey == ActionReport.Fields.Status)
                                {
                                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "'>" + act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) + "</label></td>");
                                }
                                else if ((selectedField_internal.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField_internal.FieldKey == ActionReport.Fields.EndTimeStamp))
                                {
                                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()).ToLocalTime().ToString() + "</td>");
                                }
                                else if (selectedField_internal.FieldKey == ActionReport.Fields.ScreenShot)
                                {
                                    try
                                    {
                                        int ScreenshotCount = 0;
                                        foreach (string txt_file in System.IO.Directory.GetFiles(act.LogFolder))
                                        {
                                            string fileName = System.IO.Path.GetFileName(txt_file);
                                            if (fileName.Contains("ScreenShot_"))
                                            {
                                                ScreenshotCount++;
                                            }
                                        }
                                        Tuple<int, int> sizesPreview = General.RecalculatingSizeWithKeptRatio(General.GetImageHeightWidth(act.LogFolder + @"\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png"), screenShotSampleWidth, screenShotSampleHight);
                                        string id_str = @"ScreenShot_" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + act.Seq.ToString() + "_" + ScreenshotCount;
                                        fieldsValuesHTMLTableCells.Append(@"<td align='center'><img style='display:block;' src='" + ExtensionMethods.folderNameNormalazing(act.GetType().GetProperty(ActionReport.Fields.Seq).GetValue(act) + " " + act.GetType().GetProperty(ActionReport.Fields.Description).GetValue(act).ToString()) + @"\Screenshots\ScreenShot_" + act.Seq.ToString() + "_" + ScreenshotCount + ".png' alt='" + act.Description + " - Action - Screenshot" + "' width='" + sizesPreview.Item1.ToString() + "' height='" + sizesPreview.Item2.ToString() + "' id='" + id_str + "' onclick='show_modal(\"" + id_str + "\")'></img></td>");
                                    }
                                    catch
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td></td>");
                                    }
                                }
                                else
                                {
                                    if (act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act) != null)
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(act.GetType().GetProperty(selectedField_internal.FieldKey.ToString()).GetValue(act).ToString()) + "</td>");
                                    }
                                    else
                                    {
                                        fieldsValuesHTMLTableCells.Append("<td></td>");
                                    }
                                }
                            }
                            fieldsValuesHTMLTableCells.Append("</tr>");

                            if (currentTemplate.ReportLowerLevelToShow != HTMLReportConfiguration.ReportsLevel.ActivityLevel.ToString())
                            {
                                string prevActivitySeq = string.Empty;
                                string nextActivitySeq = string.Empty;
                                string prevActivityName = string.Empty;
                                string nextActivityName = string.Empty;
                                if (ActivityReport.ActionReports.ElementAtOrDefault(ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) - 1) != null)
                                {
                                    prevActivityName = ActivityReport.ActionReports[ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) - 1].Name;
                                    prevActivitySeq = ActivityReport.ActionReports[ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) - 1].Seq.ToString();
                                }
                                if (ActivityReport.ActionReports.ElementAtOrDefault(ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) + 1) != null)
                                {
                                    nextActivityName = ActivityReport.ActionReports[ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) + 1].Name;
                                    nextActivitySeq = ActivityReport.ActionReports[ActivityReport.ActionReports.FindIndex(x => x.GUID == act.GUID) + 1].Seq.ToString();
                                }

                                CreateActionLevelReport(act, currentHTMLReportsFolder, ReportLevel + "../", false, new Tuple<Tuple<string, string>, Tuple<string, string>>(new Tuple<string, string>(prevActivitySeq, prevActivityName), new Tuple<string, string>(nextActivitySeq, nextActivityName)));
                            }
                        }
                    }
                    ReportHTML = ReportHTML.Replace("{ActionIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{ActionStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
            }

            ReportHTML = ReportHTML.Replace("{Actions_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{Actions_Data}", fieldsValuesHTMLTableCells.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            // Save the HTML            
            string FileName = currentHTMLReportsFolder + @"\ActivityReport.html";
            System.IO.File.WriteAllText(FileName, ReportHTML);

            ActivityReport = null;
            ReportHTML = null;
        }

        public void CreateActionLevelReport(ActionReport ActionReport, string currentHTMLReportsFolder = "", string ReportLevel = "", bool calledAsRoot = false, Tuple<Tuple<string, string>, Tuple<string, string>> nextPrevActionName = null)
        {
            // read template
            string ReportHTML = ExtensionMethods.GetHTMLTemplate("ActionReport.html", TemplatesFolder);
            string ReportsCSS = string.Empty;
            string ReportJS = string.Empty;

            if (calledAsRoot)
            {
                HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(HTMLReportMainFolder.Replace("{name_to_replace}", ExtensionMethods.folderNameNormalazing(ActionReport.Description))
                                                     .Replace("{date_to_replace}", DateTime.Now.ToString("MMddyyyy_HHmmss"))
                                                     .Replace("{objectType_to_replace}", typeof(ActionReport).Name.ToString()));
                currentHTMLReportsFolder = HTMLReportMainFolder;
                System.IO.Directory.CreateDirectory(currentHTMLReportsFolder + @"\Screenshots\");
                ReportLevel = "./";
                StyleBundle = string.Empty;
                JSBundle = string.Empty;
                ReportsCSS = string.Empty;
                ReportJS = string.Empty;
                BeatLogo = string.Empty;
                CompanyLogo = string.Empty;
                GingerLogo = string.Empty;
            }
            else
            {
                currentHTMLReportsFolder = currentHTMLReportsFolder + @"\" + ExtensionMethods.folderNameNormalazing(ActionReport.Seq + " " + ActionReport.Description);
                System.IO.Directory.CreateDirectory(currentHTMLReportsFolder);
            }

            if (StyleBundle == string.Empty || StyleBundle == "")
            {
                StyleBundle = CreateStylePath();
            }
            if (JSBundle == string.Empty || JSBundle == "")
            {
                JSBundle = CreateJavaScriptPath();
            }
            if (CompanyLogo == string.Empty || CompanyLogo == "")
            {
                CompanyLogo = CreateCompanyLogo();
            }
            if (GingerLogo == string.Empty || GingerLogo == "")
            {
                GingerLogo = CreateGingerLogo();
            }
            if (BeatLogo == string.Empty || BeatLogo == "")
            {
                BeatLogo = CreateBeatLogo();
            }
            ReportHTML = ReportHTML.Replace("{beat_logo}", BeatLogo);
            ReportHTML = ReportHTML.Replace("{company_logo}", CompanyLogo);
            ReportHTML = ReportHTML.Replace("{ginger_logo}", GingerLogo);
            if (currentTemplate.UseLocalStoredStyling)
            {
                if (ReportsCSS == string.Empty || ReportsCSS == "")
                {
                    ReportsCSS = ExtensionMethods.GetHTMLTemplate("Styles.css", TemplatesFolder + "/assets/css/");
                }
            }

            // running on all selected fields and getting this fields names AND values from the Report file (both into separate html-table string)
            StringBuilder fieldsNamesHTMLTableCells = new StringBuilder();
            StringBuilder fieldsValuesHTMLTableCells = new StringBuilder();
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldKey != ActionReport.Fields.ScreenShot && x.FieldType == Ginger.Reports.FieldsType.Field.ToString())))
            {
                fieldsNamesHTMLTableCells.Append("<td>" + selectedField.FieldName + "</td>");
                if (((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)) == null)
                {
                    fieldsValuesHTMLTableCells.Append("<td> N/A </td>");
                    continue;
                }

                if (selectedField.FieldKey == ActionReport.Fields.Description)
                {
                    if ((currentActionNameText == null) || (currentActionNameText.ToString() == string.Empty))
                    {
                        currentActionNameText = ExtensionMethods.OverrideHTMLRelatedCharacters(((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)).ToString());
                    }
                }

                if (selectedField.FieldKey == ActionReport.Fields.Status)
                {
                    fieldsValuesHTMLTableCells.Append("<td><label class='Status" + ((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)) + "'>" + ((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)) + "</label></td>");
                }
                else if ((selectedField.FieldKey == ActionReport.Fields.StartTimeStamp) || (selectedField.FieldKey == ActionReport.Fields.EndTimeStamp))
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + DateTime.Parse(((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)).ToString()).ToLocalTime().ToString() + "</td>");
                }
                else
                {
                    fieldsValuesHTMLTableCells.Append("<td>" + ExtensionMethods.OverrideHTMLRelatedCharacters(Convert.ToString(((ActionReport)ActionReport).GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(((ActionReport)ActionReport)))) + "</td>");
                }
            }

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_Name_Link}", currentRunSetLinkText == string.Empty ? string.Empty : "SUMMARY VIEW");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_Name_Link}", currentGingerRunnerLinkText == string.Empty ? string.Empty : currentGingerRunnerLinkText);
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_Name_Link}", currentBusinessFlowLinkText == string.Empty ? string.Empty : currentBusinessFlowLinkText);
            ReportHTML = ReportHTML.Replace("{Parent_Activity_Name_Link}", currentActivityLinkText == string.Empty ? string.Empty : currentActivityLinkText);

            ReportHTML = ReportHTML.Replace("{Parent_RunSetReport_delimiter}", currentRunSetLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_GingerRunner_delimiter}", currentGingerRunnerLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_BusinessFlow_delimiter}", currentBusinessFlowLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");
            ReportHTML = ReportHTML.Replace("{Parent_Activity_delimiter}", currentActivityLinkText == string.Empty ? string.Empty : "<i class='ace-icon fa fa-angle-double-right clsc1'></i>");

            // handling Next/Prev Items Buttons
            if (nextPrevActionName != null)
            {
                if ((nextPrevActionName != null) && (nextPrevActionName.Item2 != null) && (nextPrevActionName.Item2.Item1.ToString() != string.Empty) && (nextPrevActionName.Item2.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_next}", NextItemImage);
                    ReportHTML = ReportHTML.Replace("{Next_ActionFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActionName.Item2.Item1.ToString() + " " + nextPrevActionName.Item2.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemNextToolTip}", "Next Action - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActionName.Item2.Item2.ToString()));
                }
                else
                {
                    string nextActionLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActionLink_Start-->", "<!--Next_ActionLink_End-->");
                    ReportHTML = ReportHTML.Replace(nextActionLink, "");
                }

                if ((nextPrevActionName != null) && (nextPrevActionName.Item1 != null) && (nextPrevActionName.Item1.Item1.ToString() != string.Empty) && (nextPrevActionName.Item1.Item2.ToString() != string.Empty))
                {
                    ReportHTML = ReportHTML.Replace("{item_prev}", PrevItemImage);
                    ReportHTML = ReportHTML.Replace("{Prev_ActionFolder_Link}", ExtensionMethods.folderNameNormalazing(nextPrevActionName.Item1.Item1.ToString() + " " + nextPrevActionName.Item1.Item2.ToString()));
                    ReportHTML = ReportHTML.Replace("{ItemPrevToolTip}", "Previous Action - " + ExtensionMethods.OverrideHTMLRelatedCharacters(nextPrevActionName.Item1.Item2.ToString()));
                }
                else
                {
                    string prevActionLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActionLink_Start-->", "<!--Prev_ActionLink_End-->");
                    ReportHTML = ReportHTML.Replace(prevActionLink, "");
                }
            }
            else
            {
                string nextActionLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Next_ActionLink_Start-->", "<!--Next_ActionLink_End-->");
                ReportHTML = ReportHTML.Replace(nextActionLink, "");
                string prevActionLink = ExtensionMethods.GetStringBetween(ReportHTML, "<!--Prev_ActionLink_Start-->", "<!--Prev_ActionLink_End-->");
                ReportHTML = ReportHTML.Replace(prevActionLink, "");
            }

            ReportHTML = ReportHTML.Replace("{Parent_Action_Name}", currentActionNameText);
            ReportHTML = ReportHTML.Replace("{Action_Headers}", fieldsNamesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{Action_Data}", fieldsValuesHTMLTableCells.ToString());
            ReportHTML = ReportHTML.Replace("{css_to_place}", ReportsCSS);
            ReportHTML = ReportHTML.Replace("{css_path}", StyleBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_path}", JSBundle.ToString());
            ReportHTML = ReportHTML.Replace("{js_to_place}", ReportJS);
            ReportHTML = ReportHTML.Replace("{ReportLevel}", ReportLevel);
            ReportHTML = ReportHTML.Replace("{ReportCreated}", "Created By Ginger Version : " + WorkSpace.AppVersion.ToString() + " | Used Report Template : '" + currentTemplate.Name + "' | Report Creation Time : " + DateTime.Now.ToString());
            fieldsNamesHTMLTableCells.Remove(0, fieldsNamesHTMLTableCells.Length);
            fieldsValuesHTMLTableCells.Remove(0, fieldsValuesHTMLTableCells.Length);
            // adding Sections 
            foreach (HTMLReportConfigFieldToSelect selectedField in currentTemplate.ActionFieldsToSelect.Where(x => (x.IsSelected == true && x.FieldType == Ginger.Reports.FieldsType.Section.ToString())))
            {
                if (selectedField.FieldKey == ActionReport.Fields.InputValuesDT)
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_InputValues-->",
                                                     ConvertingDatatableToHTML((DataTable)ActionReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(ActionReport), "Action Input Values", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if (selectedField.FieldKey == ActionReport.Fields.OutputValuesDT)
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_OutputValues-->",
                                                     ConvertingDatatableToHTML((DataTable)ActionReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(ActionReport), "Action Output Values", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if (selectedField.FieldKey == ActionReport.Fields.FlowControlDT)
                {
                    ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_FlowControl-->",
                                                     ConvertingDatatableToHTML((DataTable)ActionReport.GetType().GetProperty(selectedField.FieldKey.ToString()).GetValue(ActionReport), "Action Flow Control", "table table-striped table-bordered table-hover", selectedField.IsSectionCollapsed));
                }
                else if (selectedField.FieldKey == ActionReport.Fields.ScreenShots)
                {
                    StringBuilder strHTMLBuilder = new StringBuilder();
                    strHTMLBuilder.Append(@"<tbody>");
                    int screenshotCount = 0;
                    string fileName = string.Empty;
                    foreach (string txt_file in System.IO.Directory.GetFiles(ActionReport.LogFolder))
                    {
                        fileName = System.IO.Path.GetFileName(txt_file);
                        if (fileName.Contains("ScreenShot_"))
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(currentHTMLReportsFolder + @"\Screenshots");
                                System.IO.File.Copy(txt_file, currentHTMLReportsFolder + @"\Screenshots\" + fileName, true);
                                screenshotCount++;
                            }
                            catch
                            {
                                System.Threading.Thread.Sleep(500);
                                try
                                {
                                    System.IO.Directory.CreateDirectory(currentHTMLReportsFolder + @"\Screenshots");
                                    System.IO.File.Copy(txt_file, currentHTMLReportsFolder + @"\Screenshots\" + fileName, true);
                                    screenshotCount++;
                                }
                                catch { }
                            }
                        }
                    }
                    if (screenshotCount > 0)
                    {
                        foreach (string txt_file in System.IO.Directory.GetFiles(currentHTMLReportsFolder + @"\Screenshots\"))
                        {
                            fileName = System.IO.Path.GetFileName(txt_file);
                            if (fileName.Contains("ScreenShot_"))
                            {
                                Tuple<int, int> sizesPreview = General.RecalculatingSizeWithKeptRatio(General.GetImageHeightWidth(txt_file), screenShotFullWidth, screenShotFullHight);
                                strHTMLBuilder.Append(@"<tr><td align='center'><img style='display:block;' src='.\Screenshots\" + fileName.ToString() + "' alt='" + ActionReport.Description + " - Action - Screenshot" + screenshotCount.ToString() + "' width='" + sizesPreview.Item1.ToString() + "' height='" + sizesPreview.Item2.ToString() + "' id='" + fileName + "' onclick='show_modal(\"" + fileName + "\")'></img></td></tr>");
                            }
                        }
                        strHTMLBuilder.Append("</tbody>");
                        ReportHTML = ReportHTML.Replace("<!--Section_PlaceHolder_ActionScreenShots-->", strHTMLBuilder.ToString());
                        strHTMLBuilder.Remove(0, strHTMLBuilder.Length);
                    }
                    ReportHTML = ReportHTML.Replace("{screenshotIscollapse}", selectedField.IsSectionCollapsed ? "collapse" : "collapse in");
                    ReportHTML = ReportHTML.Replace("{screenshotStyle}", selectedField.IsSectionCollapsed ? "ace-icon fa fa-angle-right bigger202" : "ace-icon fa fa-angle-down bigger202");
                }
            }
            // Save the HTML            
            string FileName = currentHTMLReportsFolder + @"\ActionReport.html";
            System.IO.File.WriteAllText(FileName, ReportHTML);

            ActionReport = null;
            ReportHTML = null;
        }

        private static string ConvertingDatatableToHTML(DataTable dt, string tableCaption, string tableClass, bool isCollapsed = false)
        {
            string Htmltext = string.Empty, collapse = string.Empty, tableID = string.Empty, collpaseStyle = string.Empty;
            if (isCollapsed)
            {
                collapse = "collapse";
                collpaseStyle = "ace-icon fa fa-angle-right bigger202";
            }
            else
            {
                collapse = "collapse in";
                collpaseStyle = "ace-icon fa fa-angle-down bigger202";
            }
            tableID = tableCaption.Replace(" ", "");
            if ((dt != null) && (dt.Rows.Count > 0))
            {
                int columnCount = 0;
                StringBuilder strHTMLBuilder = new StringBuilder();
                strHTMLBuilder.Append(@"<div class='panel-heading'>");
                strHTMLBuilder.Append(@"<a class='accordion-toggle' data-toggle=" + collapse + " href =#" + tableID + "> ");
                strHTMLBuilder.Append(@"<i class='" + collpaseStyle + "' data-icon-hide='ace-icon fa fa-angle-down' data-icon-show='ace-icon fa fa-angle-right'></i>");
                strHTMLBuilder.Append(@"<h4 class='orangetxt'>" + tableCaption + "</h4></a></div>");
                strHTMLBuilder.Append(@"<div class='" + collapse + " tableScroll' id=" + tableID + ">");
                strHTMLBuilder.Append("<table class='" + tableClass + "' summary='" + tableCaption + "'>");
                strHTMLBuilder.Append(@"<thead>");

                strHTMLBuilder.Append("<tr>");
                foreach (DataColumn myColumn in dt.Columns)
                {
                    strHTMLBuilder.Append("<th>");
                    strHTMLBuilder.Append(myColumn.Caption);
                    strHTMLBuilder.Append("</th>");
                    columnCount++;
                }
                strHTMLBuilder.Append("</tr></thead><tbody>");

                foreach (DataRow myRow in dt.Rows)
                {
                    strHTMLBuilder.Append("<tr>");
                    foreach (DataColumn myColumn in dt.Columns)
                    {
                        strHTMLBuilder.Append("<td>");
                        strHTMLBuilder.Append(myRow[myColumn.ColumnName].ToString());
                        strHTMLBuilder.Append("</td>");
                    }
                    strHTMLBuilder.Append("</tr>");
                }

                //Close tags.  
                strHTMLBuilder.Append("</tbody></table></div>");
                Htmltext = strHTMLBuilder.ToString().Replace("{colspan}", columnCount.ToString());
                strHTMLBuilder.Remove(0, strHTMLBuilder.Length);
            }
            return Htmltext;
        }

        private string CreateBeatLogo()
        {
            string beatLogo = string.Empty;
            if (!currentTemplate.UseLocalStoredStyling)
            {
                beatLogo = "<img alt='Embedded Image' width='274px' height='74px' src='{ReportLevel}assets/img/@BeatLogo.png' style='padding-left:70px'/>";
            }
            else
            {
                Image Logoimage=Bitmap.FromFile((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/images/" + "@BeatLogo.jpg"));
                //beatSource.
                Tuple<int, int> sizes=General.RecalculatingSizeWithKeptRatio(Logoimage, logoWidth, logoHight);

                beatLogo = "<img alt='Embedded Image' width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='" + "data:image/png;base64," + General.ImagetoBase64String(Logoimage) + "' style='padding-left:70px'/>";
            }
            return beatLogo;
        }

        private string CreateGingerLogo()
        {
            string gingerLogo = string.Empty;
            if (!currentTemplate.UseLocalStoredStyling)
            {
                gingerLogo = "<img alt='Embedded Image' width='274px' height='74px' src='{ReportLevel}assets/img/@Ginger.png' style='float:right;padding-left:70px'/>";
            }
            else
            {
                Image gingerSource =  Bitmap.FromFile((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) +"/images/" +"@GingerLogo_lowRes.jpg"));
                Tuple<int, int> sizes = General.RecalculatingSizeWithKeptRatio(gingerSource, logoWidth, logoHight);
                gingerLogo = "<img alt='Embedded Image' width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='" + "data:image/png;base64," + General.ImagetoBase64String(gingerSource) + "' style='float:right;padding-left:70px' />";
            }
            return gingerLogo;
        }

        private string CreateCompanyLogo()
        {
            string customerLogo = string.Empty;
            if (!currentTemplate.UseLocalStoredStyling)
            {
                Image CustomerLogo = General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());

                Tuple<int, int> sizes = General.RecalculatingSizeWithKeptRatio(CustomerLogo, logoWidth, logoHight);
                if (Directory.Exists(HTMLReportMainFolder + "/assets/img"))
                {
                    CustomerLogo.Save(HTMLReportMainFolder + "/assets/img/CustomerLogo.png");
                }
                customerLogo = "<img alt='Embedded Image' width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='{ReportLevel}assets/img/CustomerLogo.png' />";
            }
            else
            {
                Image customerSource = General.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
                Tuple<int, int> sizes = General.RecalculatingSizeWithKeptRatio(customerSource, logoWidth, logoHight);
                customerLogo = "<img alt='Embedded Image' width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='" + "data:image/png;base64," + currentTemplate.LogoBase64Image.ToString() + "' style='padding-right:70px'/>";
            }
            return customerLogo;
        }

        private string CreateItemNextImage()
        {
            string itemNextImage = string.Empty;
            if (!currentTemplate.UseLocalStoredStyling)
            {
                itemNextImage = "<img width='" + itemPrevNextWidth.ToString() + "px' height='" + itemPrevNextHight.ToString() + "px' src='{ReportLevel}assets/img/@item_next.png'/>";
            }
            else
            {
                Image nextImage = Image.FromFile((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/images/" + "@ItemNext.jpg"));
                Tuple<int, int> sizes = General.RecalculatingSizeWithKeptRatio(nextImage, itemPrevNextWidth, itemPrevNextHight);
                itemNextImage = "<img width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='" + "data:image/png;base64," + General.ImagetoBase64String(nextImage) + "' style='padding-left:1px'/>";
            }
            return itemNextImage;
        }

        private string CreateItemPrevImage()
        {
            string itemPrevImage = string.Empty;
            if (!currentTemplate.UseLocalStoredStyling)
            {
                itemPrevImage = "<img width='" + itemPrevNextWidth.ToString() + "px' height='" + itemPrevNextHight.ToString() + "px' src='{ReportLevel}assets/img/@item_prev.png'/>";
            }
            else
            {
                Image prevImage = Image.FromFile((Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/images/"+ "@ItemPrev.jpg"));
                Tuple<int, int> sizes = General.RecalculatingSizeWithKeptRatio(prevImage, itemPrevNextWidth, itemPrevNextHight);
                itemPrevImage = "<img width='" + sizes.Item1.ToString() + "' height='" + sizes.Item2.ToString() + "' src='" + "data:image/png;base64," + General.ImagetoBase64String(prevImage)+ "' style='padding-left:1px'/>";
            }
            return itemPrevImage;
        }

        private string CreateStylePath()
        {
            StringBuilder mStyleBundle = new StringBuilder();
            if (!currentTemplate.UseLocalStoredStyling)
            {
                //1. copy the assets folder from installation folder to the root report folder
                //2. convert logos back to images and place them in the assets or logos/images
                //3. update links and style to be relative to the above       
                if (!Directory.Exists(HTMLReportMainFolder + "/assets"))
                {
                    General.DirectoryCopy(TemplatesFolder + "/assets/", HTMLReportMainFolder + "/assets", true);
                }
                mStyleBundle.Append(@"<style>@font-face {font-family:Source Sans Pro;src: url('{ReportLevel}assets/fonts/SourceSansPro-Regular.ttf');}</style>");
                mStyleBundle.Append(@"<link rel='stylesheet' href='{ReportLevel}assets/css/bootstrap.css' />");
                mStyleBundle.Append(@"<link rel = 'stylesheet' href ='{ReportLevel}assets/css/styles.css' /> ");
                mStyleBundle.Append(@"<link rel='stylesheet' href='{ReportLevel}assets/css/font-awesome.css' />");
            }
            else
            {
                mStyleBundle.Append(@"<link href='https://fonts.googleapis.com/css?family=Source+Sans+Pro' rel='stylesheet'>");
                mStyleBundle.Append(@"<link rel='stylesheet' href='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.2/css/bootstrap.css' />");
                mStyleBundle.Append(@"<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/font-awesome/4.7.0/css/font-awesome.min.css' />");
            }
            return mStyleBundle.ToString();
        }
        private string CreateJavaScriptPath()
        {
            StringBuilder mJSBundle = new StringBuilder();
            if (!currentTemplate.UseLocalStoredStyling)
            {
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/jquery.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/bootstrap.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/babel.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/react-with-addons.min.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/react-dom.min.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/prop-types.min.js'></script>");
                mJSBundle.Append(@"<script src='{ReportLevel}assets/js/Recharts.min.js'></script>");
            }
            else
            {
                mJSBundle.Append(@"<script src='https://cdnjs.cloudflare.com/ajax/libs/jquery/3.1.1/jquery.min.js'></script>");
                mJSBundle.Append(@"<script src='https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.0.2/js/bootstrap.js'></script>");
                mJSBundle.Append(@"<script src='https://cdnjs.cloudflare.com/ajax/libs/babel-standalone/6.26.0/babel.js'></script>");
                mJSBundle.Append(@"<script src='https://unpkg.com/react@15/dist/react-with-addons.js'></script>");
                mJSBundle.Append(@"<script src='https://unpkg.com/react-dom@15.6.1/dist/react-dom.min.js'></script>");
                mJSBundle.Append(@"<script src='https://cdnjs.cloudflare.com/ajax/libs/prop-types/15.6.2/prop-types.min.js'></script>");
                mJSBundle.Append(@"<script src='https://cdnjs.cloudflare.com/ajax/libs/recharts/1.1.0/Recharts.min.js'></script>");
            }
            return mJSBundle.ToString();
        }


    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public static class ExtensionMethods
    {
        public static string defaultAutomationTabReportName = "{name_to_replace}_{date_to_replace}_AutomationTab_{objectType_to_replace}";

        public static string CreateGingerExecutionReport(ReportInfo RI, bool calledFromAutomateTab = false, HTMLReportConfiguration SelectedHTMLReportConfiguration = null, string mHTMLReportsFolder = null, bool isHTMLReportPermanentFolderNameUsed = false, long maxFolderSize = 0)
        {
            GingerExecutionReport gingerExecutionReport = new GingerExecutionReport();
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!! use path combine
            gingerExecutionReport.TemplatesFolder = (ExtensionMethods.getGingerEXEFileName() + @"Reports\GingerExecutionReport\").Replace("Ginger.exe", "");

            if (SelectedHTMLReportConfiguration != null)
            {
                gingerExecutionReport.currentTemplate = SelectedHTMLReportConfiguration;
            }
            else
            {
                var HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                HTMLReportConfiguration defualtConfig = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
                // TODO - need to delete, template always should be initialize with fields.
                if (defualtConfig != null)
                {
                    gingerExecutionReport.currentTemplate = HTMLReportConfiguration.EnchancingLoadedFieldsWithDataAndValidating(defualtConfig);
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Missing report template configuration");
                    return null;
                }
            }

            if ((RI.ReportInfoRootObject == null) || (RI.ReportInfoRootObject.GetType() == typeof(Object)))
            {
                return string.Empty;
            }

            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            if (!calledFromAutomateTab)
            {
                if ((mHTMLReportsFolder != null) && (mHTMLReportsFolder != string.Empty))
                {
                    if (isHTMLReportPermanentFolderNameUsed)
                    {
                        mHTMLReportsFolder = ExtensionMethods.GetReportDirectory(mHTMLReportsFolder + System.IO.Path.GetFileName(((RunSetReport)RI.ReportInfoRootObject).Name));
                    }
                    gingerExecutionReport.HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(mHTMLReportsFolder);
                }
                else
                {
                    if (!isHTMLReportPermanentFolderNameUsed)
                    {
                        gingerExecutionReport.HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder + "\\" + System.IO.Path.GetFileName(((RunSetReport)RI.ReportInfoRootObject).LogFolder));
                    }
                    else
                    {
                        gingerExecutionReport.HTMLReportMainFolder = ExtensionMethods.GetReportDirectory(currentConf.HTMLReportsFolder + "\\" + System.IO.Path.GetFileName(((RunSetReport)RI.ReportInfoRootObject).Name));
                    }
                }
            }
            else
            {
                gingerExecutionReport.HTMLReportMainFolder = currentConf.HTMLReportsFolder + "\\" + defaultAutomationTabReportName;
            }

            if (Directory.Exists(gingerExecutionReport.HTMLReportMainFolder))
            {
                CleanDirectory(gingerExecutionReport.HTMLReportMainFolder);
            }
            string Folder = WorkSpace.Instance.Solution.Folder.ToString() + "\\HTMLReports\\";

            if (currentConf.LimitReportFolderSize)
            {
                DeleteFolderContentBySizeLimit DeleteFolderContentBySizeLimit = new DeleteFolderContentBySizeLimit(Folder, maxFolderSize);
            }

            switch (RI.reportInfoLevel)
            {
                case ReportInfo.ReportInfoLevel.RunSetLevel:
                    gingerExecutionReport.CreateSummaryViewReport(RI);
                    break;
                case ReportInfo.ReportInfoLevel.GingerLevel:
                    gingerExecutionReport.CreateGingerLevelReport((GingerReport)((ReportInfo)RI).ReportInfoRootObject, "", true);
                    break;
                case ReportInfo.ReportInfoLevel.BussinesFlowLevel:
                    gingerExecutionReport.CreateBusinessFlowLevelReport((BusinessFlowReport)((ReportInfo)RI).ReportInfoRootObject, "", "", true);
                    break;
                case ReportInfo.ReportInfoLevel.ActivityLevel:
                    gingerExecutionReport.CreateActivityLevelReport((ActivityReport)((ReportInfo)RI).ReportInfoRootObject, "", "", true);
                    break;
                case ReportInfo.ReportInfoLevel.ActionLevel:
                    gingerExecutionReport.CreateActionLevelReport((ActionReport)((ReportInfo)RI).ReportInfoRootObject, "", "", true);
                    break;
                default:
                    return string.Empty;
            }
            return gingerExecutionReport.HTMLReportMainFolder;
        }

        public static string CreateActivitiesGroupReportsOfBusinessFlow(ProjEnvironment environment, BusinessFlow BF, bool calledFromAutomateTab = false, HTMLReportConfiguration SelectedHTMLReportConfiguration = null, string mHTMLReportsFolder = null)
        {
            Ginger.Reports.GingerExecutionReport.GingerExecutionReport l = new Ginger.Reports.GingerExecutionReport.GingerExecutionReport();
            l.TemplatesFolder = (ExtensionMethods.getGingerEXEFileName() + @"Reports\GingerExecutionReport\").Replace("Ginger.exe", "");

            if (SelectedHTMLReportConfiguration != null)
            {
                l.currentTemplate = SelectedHTMLReportConfiguration;
            }
            else
            {
                ObservableList<HTMLReportConfiguration> HTMLReportConfigurations = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
                l.currentTemplate = HTMLReportConfigurations.Where(x => (x.IsDefault == true)).FirstOrDefault();
            }
            if (string.IsNullOrEmpty(BF.ExecutionFullLogFolder))
            {
                string exec_folder = string.Empty;
                exec_folder = Ginger.Run.ExecutionLogger.GenerateBusinessflowOfflineExecutionLogger(environment, BF);
                if (string.IsNullOrEmpty(exec_folder))
                {
                    return string.Empty;
                }
                BF.ExecutionFullLogFolder = exec_folder;
            }
            ReportInfo RI = new ReportInfo(BF.ExecutionFullLogFolder);
            if ((RI.ReportInfoRootObject == null) || (RI.ReportInfoRootObject.GetType() == typeof(Object)))
            {
                return string.Empty;
            }

            l.HTMLReportMainFolder = Path.GetTempPath() + "TempHTMLReportFolder";
            if (Directory.Exists(l.HTMLReportMainFolder))
            {
                ExtensionMethods.CleanDirectory(l.HTMLReportMainFolder);
            }

            switch (RI.reportInfoLevel)
            {
                case ReportInfo.ReportInfoLevel.BussinesFlowLevel:
                    l.CreateActivitiesGroupReportsOfBusinessFlow((BusinessFlowReport)((ReportInfo)RI).ReportInfoRootObject, ref BF, l.HTMLReportMainFolder, "", true);
                    break;
            }

            return l.HTMLReportMainFolder;
        }

        public static string getGingerEXEFileName()
        {
            //TODO: Currently we return the Ginger EXE in GingerUnitTest, later on need to be the deployed one
            string GingerEXEFileName = Assembly.GetExecutingAssembly().Location;

            GingerEXEFileName = GingerEXEFileName.Replace(Path.GetFileName(GingerEXEFileName), "Ginger.exe");
            return GingerEXEFileName;
        }

        public static string GetHTMLTemplate(string HTMLFileName, string TemplatesFolder)
        {

            //TODO: cache templates - Speed !


            //FIXME hard coded
            string htmlfile = TemplatesFolder + HTMLFileName;
            string HTML = System.IO.File.ReadAllText(htmlfile);
            return HTML;
        }

        public static string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            string str = "";
            int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if ((Pos2 - Pos1) > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            else
            {
                return "";
            }
        }

        public static void CleanDirectory(string folderName)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folderName);

            try
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
            }
            catch (Exception ex)
            {

                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        public static string GetReportDirectory(string logsFolder)
        {
            try
            {
                logsFolder = logsFolder.Replace(@"~", WorkSpace.Instance.Solution.Folder);
                if (Directory.Exists(logsFolder))
                {
                    return logsFolder;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(logsFolder);
                    return logsFolder;
                }
            }
            catch (Exception)
            {
                logsFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"HTMLReports\");
                System.IO.Directory.CreateDirectory(logsFolder);
                WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationHTMLReportsFolder = @"~\HTMLReports\";
            }

            return logsFolder;
        }

        public static string folderNameNormalazing(string folderName)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            folderName = folderName.Replace(@".", "");
            folderName = folderName.Replace(@"'", "");
            folderName = folderName.Replace("\"", "");
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            if (folderName.Length > 30)
            {
                folderName = folderName.Substring(0, 30);
            }
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            return folderName;
        }

        public static string OverrideHTMLRelatedCharacters(string text)
        {
            try
            {
                text = text.Replace(@"<", "&#60;");
                text = text.Replace(@">", "&#62;");
                text = text.Replace(@"$", "&#36;");
                text = text.Replace(@"%", "&#37;");
                return text;
            }
            catch
            {
                return text;
            }
        }

        public static ObservableList<HTMLReportConfiguration> GetSolutionHTMLReportConfigurations()
        {
            var list = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>();
            list.ToList().ForEach(y => y = HTMLReportConfiguration.EnchancingLoadedFieldsWithDataAndValidating(y));
            return list;
        }

        public static void SetTemplateAsDefault(HTMLReportConfiguration templateToSetAsDefualt)
        {
            //            if (Reporter.ToUser(eUserMsgKey.ReportsTemplatesSaveWarn) != MessageBoxResult.Yes) return;

            templateToSetAsDefualt.IsDefault = true;
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(templateToSetAsDefualt);

            var defuatlTemplates = GetSolutionHTMLReportConfigurations().Where(x => (x.IsDefault == true)).ToList();
            foreach (HTMLReportConfiguration template in defuatlTemplates)
                if (template != templateToSetAsDefualt)
                {
                    template.IsDefault = false;
                    WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(template);
                }
        }
    }
}
