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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;
using GingerCore;
using Amdocs.Ginger.Common;

namespace Ginger.Reports
{
    /// <summary>
    /// Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        private bool IgnoreSkippedAct=false;
        public ReportInfo ReportInfo;

        public ReportPage(ReportInfo RI, string Xaml)
        {
            InitializeComponent();
            ReportInfo = RI;                       
            LoadReportData(Xaml);
        }

        private void LoadReportData(string ReportXaml)
        {
            try
            {
                //TODO: this can be done by adding a config area to Report, then use ConfigReport option, instead of embedding text

                //If add IgnoreSkippedAct element to report, then the inactive activities will be ignroed in PDF
                if (ReportXaml.IndexOf("IgnoreSkippedAct",StringComparison.CurrentCultureIgnoreCase)>0)
                    IgnoreSkippedAct=true;
                
                ReplaceBusinessFlowTemplate(ref ReportXaml, ReportInfo.BusinessFlows);
                FlowDocument content = (FlowDocument)XamlReader.Parse(ReportXaml);
                FlowDocumentReader.Document = content;

                FlowDocumentReader.DataContext = ReportInfo;
            }
            catch (Exception ex)
            {                
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Error in Customized report XML - " + ex.Message);
            }
        }

        private void ReplaceBusinessFlowTemplate(ref string ReportXaml, List<BusinessFlowReport> list)
        {
            StringBuilder BusinessFlowsData = new StringBuilder();
            string BusinessFlowTemplate = GetStringBetween(ReportXaml, "<!--BusinessFlowStart-->", "<!--BusinessFlowEnd-->");
            if (string.IsNullOrEmpty(BusinessFlowTemplate)) return;

            for (int i = 0; i < list.Count; i++)
            {
                string BusinessFlowData = BusinessFlowTemplate.Replace("BusinessFlows[i]", "BusinessFlows[" + i + "]");
                ReplaceVariableTemplate(ref BusinessFlowData, list[i]);
                ReplaceActivityTemplate(ref BusinessFlowData, list[i]);
                BusinessFlowsData.Append(BusinessFlowData);
            }
            ReportXaml = ReportXaml.Replace("<!--BusinessFlowStart-->" + BusinessFlowTemplate + "<!--BusinessFlowEnd-->", BusinessFlowsData.ToString());
        }

        private void ReplaceActivityTemplate(ref string BusinessFlowData, BusinessFlowReport BFR)
        {
            StringBuilder ActivitiesData = new StringBuilder();
            string ActivityTemplate = GetStringBetween(BusinessFlowData, "<!--ActivityStart-->", "<!--ActivityEnd-->");
            if (string.IsNullOrEmpty(ActivityTemplate)) return;

            for (int i = 0; i < BFR.Activities.Count; i++)
            {
                //If activity is skipped, then not put into the report
                if (IgnoreSkippedAct && BFR.Activities[i].Status == "Skipped")
                    continue;
                string ActivityData = ActivityTemplate.Replace("Activities[i]", "Activities[" + i + "]");
                ReplaceActionTemplate(ref ActivityData, BFR.Activities[i]);
                ActivitiesData.Append(ActivityData);
            }
            BusinessFlowData = BusinessFlowData.Replace("<!--ActivityStart-->" + ActivityTemplate + "<!--ActivityEnd-->", ActivitiesData.ToString());
        }

        private void ReplaceActionTemplate(ref string ActivityData, ActivityReport activity)
        {
            StringBuilder ActionsData = new StringBuilder();
            string ActionTemplate = GetStringBetween(ActivityData, "<!--ActionStart-->", "<!--ActionEnd-->");
            if (string.IsNullOrEmpty(ActionTemplate)) return;
            for (int i = 0; i < activity.Actions.Count; i++)
            {
                string ActionData = ActionTemplate.Replace("Actions[i]", "Actions[" + i + "]");
                ReplaceScreenshotsTemplate(ref ActionData, activity.Actions[i]);
                ReplaceReturnValueReportTemplate(ref ActionData, activity.Actions[i]);
                ActionsData.Append(ActionData);
            }
           
                ActivityData = ActivityData.Replace("<!--ActionStart-->" + ActionTemplate + "<!--ActionEnd-->", ActionsData.ToString());
        }

        private void ReplaceScreenshotsTemplate(ref string ActionData,ActionReport action)
        {
            string ScreenshotsData="";
            string ScreenshotTemplate = GetStringBetween(ActionData, "<!--Screenshots Start-->", "<!--Screenshots End-->");
            if (string.IsNullOrEmpty(ScreenshotTemplate)) return;
            for (int i = 0; i < action.ScreenShots.Count; i++)
            {
                string ScreenshotData = ScreenshotTemplate.Replace("ScreenShots[i]", "ScreenShots[" + i + "]");
                ScreenshotsData += ScreenshotData;
            }
            ActionData = ActionData.Replace("<!--Screenshots Start-->" + ScreenshotTemplate + "<!--Screenshots End-->", ScreenshotsData);
        }

        private void ReplaceReturnValueReportTemplate(ref string ActionData, ActionReport action)
        {
            StringBuilder ReturnValueReport = new StringBuilder();
            string ReturnValueReportTemplate = GetStringBetween(ActionData, "<!--ReturnValueReportStart-->", "<!--ReturnValueReportEnd-->");
            if (string.IsNullOrEmpty(ReturnValueReportTemplate)) return;
            for (int i = 0; i < action.ReturnValueReport.Count; i++)
            {
                string ReturnValueData = ReturnValueReportTemplate.Replace("ReturnValueReport[i]", "ReturnValueReport[" + i + "]");
                ReturnValueReport.Append(ReturnValueData);
            }
            ActionData = ActionData.Replace("<!--ReturnValueReportStart-->" + ReturnValueReportTemplate + "<!--ReturnValueReportEnd-->", ReturnValueReport.ToString());
        }

        private void ReplaceVariableTemplate(ref string BusinessFlowData, BusinessFlowReport BFR)
        {
            StringBuilder VariablesData = new StringBuilder();
            string VariableTemplate = GetStringBetween(BusinessFlowData, "<!--VariableStart-->", "<!--VariableEnd-->");
            if (string.IsNullOrEmpty(VariableTemplate)) return;
            for (int i = 0; i < BFR.Variables.Count; i++)
            {
                string VariableData = VariableTemplate.Replace("Variables[i]", "Variables[" + i + "]");
                VariablesData.Append(VariableData);
            }
            BusinessFlowData = BusinessFlowData.Replace("<!--VariableStart-->" + VariableTemplate + "<!--VariableEnd-->", VariablesData.ToString());
        }

        public string GetStringBetween(string STR, string FirstString, string LastString = null)
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

        public void SaveReport(string FileName)
        {
            FlowDocumentReader.Refresh();
            var DocContent = new TextRange(FlowDocumentReader.Document.ContentStart, FlowDocumentReader.Document.ContentEnd);

            if (DocContent.CanSave(DataFormats.Rtf))
            {
                using (var stream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    DocContent.Save(stream, DataFormats.Rtf, true);
                }
            }
            //TODO: add saving in different formats
            
            // DO NOT DELETE will be used later !!!!!!!!!!!!!!!!!!!!!!!
            // if (content2.CanSave(DataFormats.))
            //{
            //    using (var stream = new FileStream(@"c:\temp\2.xaml", FileMode.Create))
            //    {
            //        content2.Save(stream, DataFormats.Xaml, true);
            //    }
            //}

            // Saving with pic and smaller footprint
            //if (DocContent.CanSave(DataFormats.XamlPackage))
            //{
            //    using (var stream = new FileStream(@"c:\temp\2.xaml", FileMode.Create))
            //    {
            //        DocContent.Save(stream, DataFormats.XamlPackage, true);
            //    }
            //}

            ////Save to XPS
            //Attachment xps = new Attachment(FlowDocumentToXPS(), "FileName.xps", "application/vnd.ms-xpsdocument");
        }
    }
}
