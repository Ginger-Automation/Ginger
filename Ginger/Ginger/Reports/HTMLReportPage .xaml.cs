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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using GingerCore;

namespace Ginger.Reports
{
    /// <summary>
    ///     Interaction logic for ReportPage.xaml
    /// </summary>
    public partial class HTMLReportPage : Page
    {
        public string HTML;
        public ReportInfo ReportInfo;

        public HTMLReportPage(ReportInfo RI, string sHTML)
        {
            InitializeComponent();
            ReportInfo = RI;
            LoadReportData(ref sHTML);
            HTML = sHTML;
        }

        private void LoadReportData(ref string ReportHTML)
        {
            ReplaceBusinessFlowTemplate(ref ReportHTML, ReportInfo.BusinessFlows);
            ReplaceBusinessFlowRunData(ref ReportHTML);
        }

        private void ReplaceBusinessFlowRunData(ref string ReportHTML)
        {
            string[] skiplist = { "TotalBusinessFlows" };
            ReportHTML = ReplaceDataDyncmicallyWithSkip(ReportInfo, ReportHTML, skiplist);

            ReportHTML = ReportHTML.Replace("TotalBusinessFlows", ReportInfo.TotalBusinessFlows.ToString());
        }

        private void ReplaceBusinessFlowTemplate(ref string ReportHTML, List<BusinessFlowReport> list)
        {
            var BusinessFlowsData = "";
            var BusinessFlowTemplate = GetStringBetween(ReportHTML, "<!--BusinessFlowStart-->", "<!--BusinessFlowEnd-->");
            if (string.IsNullOrEmpty(BusinessFlowTemplate)) return;

            for (var i = 0; i < list.Count; i++)
            {
                var BFCurrentIterationData = string.Empty;


                BFCurrentIterationData = ReplaceDataDyncmically(list[i], BusinessFlowTemplate, @"BusinessFlows[i]");

                ReplaceActivityTemplate(ref BFCurrentIterationData, list[i]);
                ReplaceVariableTemplate(ref BFCurrentIterationData, list[i]);
                BusinessFlowsData += BFCurrentIterationData;
            }

            ReportHTML = ReportHTML.Replace(
                "<!--BusinessFlowStart-->" + BusinessFlowTemplate + "<!--BusinessFlowEnd-->", BusinessFlowsData);
        }

        private void ReplaceActivityTemplate(ref string BusinessFlowData, BusinessFlowReport BFR)
        {
            var ActivitiesData = "";
            var ActivityTemplate = GetStringBetween(BusinessFlowData, "<!--ActivityStart-->", "<!--ActivityEnd-->");
            if (string.IsNullOrEmpty(ActivityTemplate)) return;

            for (var i = 0; i < BFR.Activities.Count; i++)
            {
                var ActivityData = ReplaceDataDyncmically(BFR.Activities[i], ActivityTemplate,
                    @"BusinessFlows[i].Activities[i]");
                ReplaceActionTemplate(ref ActivityData, BFR.Activities[i]);
                ActivitiesData += ActivityData;
            }
            BusinessFlowData = BusinessFlowData.Replace(
                "<!--ActivityStart-->" + ActivityTemplate + "<!--ActivityEnd-->", ActivitiesData);
        }

        private void ReplaceActionTemplate(ref string ActivityData, ActivityReport activity)
        {
            var ActionsData = "";
            var ActionTemplate = GetStringBetween(ActivityData, "<!--ActionStart-->", "<!--ActionEnd-->");
            if (string.IsNullOrEmpty(ActionTemplate)) return;
            for (var i = 0; i < activity.Actions.Count; i++)
            {
                var ActionData = ReplaceDataDyncmically(activity.Actions[i], ActionTemplate,
                    @"BusinessFlows[i].Activities[i].Actions[i]");
                ReplaceScreenshotTemplate(ref ActionData, activity.Actions[i].ScreenShots);

                ActionsData += ActionData;
            }
            ActivityData = ActivityData.Replace("<!--ActionStart-->" + ActionTemplate + "<!--ActionEnd-->", ActionsData);
        }

        private void ReplaceScreenshotTemplate(ref string ActivityData, List<string> ScreenShots)
        {
            var Screenshots = string.Empty;
            ObservableList<Bitmap> Bitmp = new ObservableList<Bitmap>();
            foreach (String bitmapsource in ScreenShots)
            {
                Bitmap bmp = Ginger.Utils.BitmapManager.FileToBitmapImage(bitmapsource);
                var txt = string.Empty;
             
                //Bitmap bitmap;
                using (var outStream = new MemoryStream())
                {
                    Bitmp.Add(bmp);
                    var ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Png);
                    var byteImage = ms.ToArray();
                    txt = @"data:image/png;base64," + Convert.ToBase64String(byteImage);
                }
                var html = @"<img src=" + txt + " >";
                Screenshots = Screenshots + "<br/>" + html;
            }
            ActivityData = ActivityData.Replace("BusinessFlows[i].Activities[i].Actions[i].ScreenShots", Screenshots);
        }

        private void ReplaceVariableTemplate(ref string BusinessFlowData, BusinessFlowReport BFR)
        {
            var VariablesData = "";
            var VariableTemplate = GetStringBetween(BusinessFlowData, "<!--VariableStart-->", "<!--VariableEnd-->");
            if (string.IsNullOrEmpty(VariableTemplate)) return;
            for (var i = 0; i < BFR.Variables.Count; i++)
            {
                var VariableData = VariableTemplate.Replace("BusinessFlows[i].Variables[i].Seq",
                    BFR.Variables[i].Seq.ToString());
                VariableData = ReplaceDataDyncmically(BFR.Variables[i], VariableTemplate,
                    @"BusinessFlows[i].Variables[i]");
                VariablesData += VariableData;
            }
            BusinessFlowData = BusinessFlowData.Replace(
                "<!--VariableStart-->" + VariableTemplate + "<!--VariableEnd-->", VariablesData);
        }

        public string GetStringBetween(string STR, string FirstString, string LastString = null)
        {
            var str = "";
            var Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
            int Pos2;
            if (LastString != null)
            {
                Pos2 = STR.IndexOf(LastString, Pos1);
            }
            else
            {
                Pos2 = STR.Length;
            }

            if (Pos2 - Pos1 > 0)
            {
                str = STR.Substring(Pos1, Pos2 - Pos1);
                return str;
            }
            return "";
        }

        public void SaveReport(string FileName)
        {
            FlowDocumentReader.Refresh();
            var DocContent = new TextRange(FlowDocumentReader.Document.ContentStart,
                FlowDocumentReader.Document.ContentEnd);

            if (DocContent.CanSave(DataFormats.Rtf))
            {
                using (var stream = new FileStream(FileName, FileMode.Create, FileAccess.Write))
                {
                    DocContent.Save(stream, DataFormats.Rtf, true);
                }
            }
        }

        public string ReplaceDataDyncmically(object Obj, string inputhtml, string value)
        {
            var properties = Obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                var TypeFullName = property.PropertyType.FullName;
                if (TypeFullName == "System.Int32" || TypeFullName == "System.String" ||
                    TypeFullName == "System.Boolean" || TypeFullName == "System.TimeSpan" ||
                    TypeFullName.StartsWith(@"System.Nullable`1[[System.Single"))
                {
                    if (property.GetValue(Obj) != null)
                        inputhtml = inputhtml.Replace(value + "." + property.Name, property.GetValue(Obj).ToString());
                    else
                        inputhtml = inputhtml.Replace(value + "." + property.Name, "");
                }
            }
            return inputhtml;
        }

        public string ReplaceDataDyncmicallyWithSkip(object Obj, string inputhtml,string[] skiplist)
        {
            var properties = Obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties)
            {
                if (!(Array.Exists(skiplist, element => element == property.Name)))
                {
                    var TypeFullName = property.PropertyType.FullName;
                    if (TypeFullName == "System.Int32" || TypeFullName == "System.String" ||
                        TypeFullName == "System.Boolean" || TypeFullName == "System.TimeSpan" ||
                        TypeFullName.StartsWith(@"System.Nullable`1[[System.Single"))
                    {
                        if (property.GetValue(Obj) != null)
                            inputhtml = inputhtml.Replace(property.Name, property.GetValue(Obj).ToString());
                        else
                            inputhtml = inputhtml.Replace(property.Name, "");
                    }
                }
            }
            return inputhtml;
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
