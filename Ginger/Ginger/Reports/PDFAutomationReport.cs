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

using System;
using System.Linq;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.DocumentObjectModel.Shapes;
using System.IO;
using GingerCore;
using GingerCore.Actions;
using Amdocs.Ginger.Repository;

namespace Ginger.Reports
{
    public class PDFAutomationReport
    {
        private Document mPDFReport;

        private Table mLastTable;

        private String m_ReportFileName = null;

        public void Init()
        {
            m_ReportFileName = null;
            mPDFReport = new Document();            
            DefineStyles(mPDFReport);
            CreateTitle();
        }

        //FileName is optional if no value then create temp file
        public String Generate(string FileName = null)
        {
            // Generate only once, if already done then get the file
            if (m_ReportFileName == null)
            {
                mPDFReport.UseCmykColor = true;

                const bool unicode = false;
                const PdfFontEmbedding embedding = PdfFontEmbedding.Always;

                PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(unicode, embedding);

                pdfRenderer.Document = mPDFReport;

                // Layout and render document to PDF
                pdfRenderer.RenderDocument();

                // Save the document...
                //TODO: clean/check temp folder for the pdf created...
                if (FileName==null) FileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                pdfRenderer.PdfDocument.Save(FileName);
                m_ReportFileName = FileName;
            }
            return m_ReportFileName;
        }

        private void CreateTitle() {            
            // Add a section to the document
            Section section = mPDFReport.AddSection();
            section.PageSetup.LeftMargin = 20;
            // Add a paragraph to the section
            Paragraph paragraph = section.AddParagraph();
            paragraph.AddLineBreak();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddFormattedText("Automation Report - " + DateTime.Now, "Title");
            paragraph.AddLineBreak();
        }

        public void AddBusinessFlowTitle(String Activity) {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.Font.Bold = true; 
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + Activity);
            paragraph.AddLineBreak();
        }

        public void AddEnvTitle(String Env) {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color =Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Bold = true; 
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("Environment: " + Env);
            paragraph.AddLineBreak();
        }

        public void AddTotalElapsed(BusinessFlow bf)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddLineBreak();
            
            paragraph.AddFormattedText(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Elapsed(HH:MM:ss): " + string.Format("{0:00}:{1:00}:{2:00}", bf.ElapsedSecs / 3600, (bf.ElapsedSecs / 60) % 60, bf.ElapsedSecs % 60));
            paragraph.AddLineBreak();
        }

        public void AddBFCounts(BusinessFlow bf)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("Total " + GingerDicser.GetTermResValue(eTermResKey.Activities) + ": " + bf.Activities.Count);
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("Passed: " + bf.Activities.Where(p => p.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed).Count() + " Failed: "
                + bf.Activities.Where(p => p.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).Count() + " Blocked: "
                + bf.Activities.Where(p => p.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked).Count() + " Skipped: " + bf.Activities.Where(p => p.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped).Count());            
            paragraph.AddLineBreak();
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("Total Actions: " + bf.Activities.Sum(p=>p.Acts.Count));
            paragraph.AddLineBreak();
            paragraph.AddFormattedText("Passed:" + bf.Activities.Sum(p => p.Acts.Where(t => t.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed).Count()) + " Failed:"
                + bf.Activities.Sum(p => p.Acts.Where(t => t.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).Count()) + " Blocked:"
                + bf.Activities.Sum(p => p.Acts.Where(t => t.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked).Count()) + " Skipped:" + bf.Activities.Sum(p => p.Acts.Where(t => t.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped).Count()));            
            paragraph.AddLineBreak();
        }

        public void AddAgents(BusinessFlow bf)
        {            
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Bold = true;
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddLineBreak();

            //TODO: put it in nice table
            if (bf.TargetApplications != null)
            {
                paragraph.AddFormattedText("Application(s): " + string.Join(";", bf.TargetApplications.Select(p => p.Name).ToList()));
                paragraph.AddLineBreak();
                
                // TODO: fix me to add at BF which Agent used to run it
                paragraph.AddFormattedText("Agent(s): " + string.Join(";", bf.TargetApplications.Select(p => p.LastExecutingAgentName).ToList()));
                paragraph.AddLineBreak();
            }
        }

        public void AddActivityTitle(String Activity) {            
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 50);
            paragraph.Format.Font.Underline = Underline.Single;
            paragraph.Format.Font.Size = 14;
            paragraph.Format.Font.Bold = true; 
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.Format.PageBreakBefore = true; 
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(GingerDicser.GetTermResValue(eTermResKey.Activity) + " - " + Activity);
            paragraph.AddLineBreak();
        }

        public void AddTableTitle(String Title)  {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();
            paragraph.Format.Font.Color = Color.FromCmyk(100, 30, 20, 20);
            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddLineBreak();
            paragraph.AddFormattedText(Title);
            paragraph.AddLineBreak();
        }

        public void DefineStyles(Document document) {
            String defaultFontName = "Calibri"; 

            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = defaultFontName;

            // Heading1 to Heading9 are predefined styles with an outline level. An outline level
            // other than OutlineLevel.BodyText automatically creates the outline (or bookmarks) 
            // in PDF.
            style = document.Styles.AddStyle("Currency", "Normal");
            style.ParagraphFormat.Font.Color = Colors.Blue;

            //PMInfo style
            style = document.Styles.AddStyle("Title", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 18;
            style.Font.Bold = true;
            style.Font.Color = Colors.Blue;

            //LeftHeader style
            style = document.Styles.AddStyle("LeftHeader", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 10;
            style.Font.Bold = false;
            style.Font.Color = Colors.Blue;

            //RightData style
            style = document.Styles.AddStyle("RightData", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 10;
            style.Font.Bold = true;
            style.Font.Color = Colors.Black;

            //GreenBalance style
            style = document.Styles.AddStyle("GreenBalance", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 12;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            // style.Font.Bold = true;
            style.Font.Color = Colors.Green;

            //RedBalance style
            style = document.Styles.AddStyle("RedBalance", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 12;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            // style.Font.Bold = true;
            style.Font.Color = Colors.Red;
            
            //RightNumber style
            style = document.Styles.AddStyle("RightNumber", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 10;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            //RightNumberTotal style
            style = document.Styles.AddStyle("RightNumberTotal", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 12;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            style.Font.Bold = true;
            
            style = document.Styles.AddStyle("HeaderCell", "Normal");
            style.Font.Size = 6;
            style.Font.Bold = true;
            style.ParagraphFormat.PageBreakBefore = false;
            style.ParagraphFormat.SpaceBefore = 6;
            style.ParagraphFormat.SpaceAfter = 6;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;

            style = document.Styles.AddStyle("Section", "Normal");
            style.Font.Size = 12;
            style.Font.Bold = true;
            style.Font.Italic = true;
            style.ParagraphFormat.SpaceBefore = 2;
            style.ParagraphFormat.SpaceAfter = 3;

            style = document.Styles.AddStyle("CurrencyInTable", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            //Test Pass style
            style = document.Styles.AddStyle("TestPass", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.Font.Bold = true;
            style.Font.Color = Colors.Green;
            
            //Test Fail style
            style = document.Styles.AddStyle("TestFail", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 10;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.Font.Bold = true;
            style.Font.Color = Colors.Red;
  
            //Test Header style
            style = document.Styles.AddStyle("TestHeader", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 14;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            style.Font.Bold = true;
            style.Font.Color = Colors.Brown;

            //Assert style
            style = document.Styles.AddStyle("Assert", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 12;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.Font.Bold = true;
            style.Font.Color = Colors.Red;

            //cellStyle style
            style = document.Styles.AddStyle("cellStyle", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;            
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                    
            //RigthAlignCellStyle style
            style = document.Styles.AddStyle("RigthAliagnCellStyle", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            //RedStatusCellStyle style
            style = document.Styles.AddStyle("RedStatusCellStyle", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;
            style.Font.Bold = true;
            style.Font.Color = Colors.White;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.ParagraphFormat.Shading.Color = Colors.Red;

            //GreenStatusCellStyle style
            style = document.Styles.AddStyle("GreenStatusCellStyle", "Normal");
            style.Font.Name = defaultFontName;
            style.Font.Size = 6;
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.ParagraphFormat.Shading.Color = Colors.LightGreen;
        }

        public void addTable(String Columns)
        {
            // Text,width centimeter
            // example:  "Cycle,3~!>Step,2~!>..."

            mLastTable = new Table();
            mLastTable.Borders.Width = 0.5;

            string[] sep=new string[]{"~!>"};
            String[] a = Columns.Split(sep,StringSplitOptions.None);            
            foreach (String s in a)
            {
                String[] colInfo = s.Split(',');            
                Column column = mLastTable.AddColumn(Unit.FromCentimeter((Unit)colInfo[1]));
                column.Format.Alignment = ParagraphAlignment.Center;
                column.Style = "HeaderCell";                
            }            

            Row row = mLastTable.AddRow();           

            //ugly repeating code of split - find better code
            for (int i = 0; i < a.Length; i++)
            {
                String[] b = a[i].Split(',');            
                SetCellTextAndStyle(row, i, b[0]);
            }            
        }

        //Allowing to define specific style to any cell according to it data
        public void SetCellTextAndStyle(Row r, int cellIndex, string text, string style = "cellStyle")
        {
            Cell cell = r.Cells[cellIndex];
            //cell.Style = "cellStyle"; 
            if (mPDFReport.Styles[style] != null)
            {
                cell.Style = style;
                cell.Shading.Color = mPDFReport.Styles[style].ParagraphFormat.Shading.Color;
            }
            Paragraph p = cell.AddParagraph(text + "");
        }
        
        public void addTableRow(String RowData)
        {            
            Row row = mLastTable.AddRow();
           
            string[] sep=new string[]{"~!>"};
            String[] a = RowData.Split(sep, StringSplitOptions.None);    
            int i = 0;
            foreach (String s in a)
            {
                //Allowing to define specific style to any cell according to it data
                String[] cellValues = s.Split(new string[] { "<#>" }, StringSplitOptions.None);
                if (cellValues.Length > 1)
                    SetCellTextAndStyle(row, i, cellValues[1], cellValues[0]);
                else
                    SetCellTextAndStyle(row, i, cellValues[0]);
                i++;
            }                        
        }

        public void FinalizeTable()
        {
            mLastTable.SetEdge(0, 0, mLastTable.Columns.Count, mLastTable.Rows.Count, Edge.Box, BorderStyle.Single, 1.5, Colors.Blue);
            mPDFReport.LastSection.Add(mLastTable);
        }

        public void AddTestHeader(String Method, string Assert)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();

            paragraph.Style = "TestHeader";            
            paragraph.AddFormattedText(Method);            
            paragraph.AddLineBreak();

            Paragraph AssertParagraph = mPDFReport.LastSection.AddParagraph();

            if (Assert != null)
            {
                AssertParagraph.Style = "Assert";
                AssertParagraph.AddFormattedText(Assert);
                AssertParagraph.AddLineBreak();
            }
        }

        public void AddTestInfo(String info)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();

            paragraph.Format.Alignment = ParagraphAlignment.Left;
            paragraph.AddFormattedText(info);
            paragraph.AddLineBreak();
        }

        public void AddTestFailInfo(String info)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();

            paragraph.Style = "TestFail";            
            paragraph.AddFormattedText(info);
            paragraph.AddLineBreak();
        }

        public void AddTestPassInfo(String info)
        {
            Paragraph paragraph = mPDFReport.LastSection.AddParagraph();

            paragraph.Style = "TestPass";
            paragraph.AddFormattedText(info);
            paragraph.AddLineBreak();
        }
        
        private void addBitmap(System.Drawing.Bitmap bitmap)
        {
            if (bitmap == null) return;
            Row row = mLastTable.Rows[mLastTable.Rows.Count-1];
            //TODO: add col for bitmap
            setCellBitmap(row, 7, bitmap);            
        }

        public void setCellBitmap(Row r, int cellIndex, System.Drawing.Bitmap bitmap)
        {
            Cell cell = r.Cells[cellIndex];

            string tempFile = Path.GetTempFileName();
            bitmap.Save(tempFile);
           Image img = cell.AddImage(tempFile);
            img.Height = 100;
            img.Width = 150;
        }

        private string GeneratePDF(BusinessFlow BF, string FileName = null)
        {
            AddBusinessFlowTitle(BF.Name);

            //TODO: remove from here this and clean environment from BF, when moved to ReportTemplates
            AddEnvTitle(BF.Environment);
            AddTotalElapsed(BF);
            AddBFCounts(BF);
            AddAgents(BF);
            int Seq = 0;
            foreach (Activity a in BF.Activities)
            {
                AddActivityTitle(a.ActivityName);
                foreach (Act act in a.Acts)
                {
                    Seq++;
                    // add Actions table
                    AddTableTitle("Action:");

                    addTable("#,0.60~!>Auto,0.65~!>Description,3.2~!>Value,4.3~!>Status,1.0~!>Elapsed,1~!>Error,3.5~!>ScreenShot,6");
                    //Allowing to define specific style to any cell according to it data             
                    string statusCellStyle = "";
                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed) statusCellStyle = "RedStatusCellStyle";
                    if (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed) statusCellStyle = "GreenStatusCellStyle";
                    addTableRow(Seq + "~!>" + act.Active + "~!>" + act.Description + "~!>" + act.GetInputParamCalculatedValue("Value", false) + "~!>" + statusCellStyle + "<#>" + act.Status + "~!>" + "RigthAliagnCellStyle" + "<#>" + Math.Round(Convert.ToDouble(act.ElapsedSecs), 1) + "~!>" + act.Error + "~!>");
                    
                    foreach(String path in act.ScreenShots)
                    {
                        System.Drawing.Bitmap bmp = GingerCore.General.FileToBitmapImage(path);
                        addBitmap(bmp);
                    }
                         
                    FinalizeTable();

                    //TODO: add Validations table for each action
                    if (act.ReturnValues.Count() > 0)
                    {
                        AddTableTitle("Return Values Validations:");
                        addTable("Auto,0.65~!>Param,2.8~!>Staus,1.4~!>Expected,4.7~!>Expected Calculated,4.7~!>Actual,4.7~!>StoreToVar,1.3");
                        foreach (ActReturnValue v in act.ReturnValues)
                        {
                            //Allowing to define specific style to any cell according to it data
                            string resultCellStyle = "";
                            if (v.Status == ActReturnValue.eStatus.Failed) resultCellStyle = "RedStatusCellStyle";
                            if (v.Status == ActReturnValue.eStatus.Passed) resultCellStyle = "GreenStatusCellStyle";
                            addTableRow(v.Active + "~!>" + v.Param + "~!>" + resultCellStyle + "<#>" + v.Status + "~!>" + "RigthAliagnCellStyle" + "<#>" + v.Expected + "~!>" + v.ExpectedCalculated + "~!>" + v.Actual+"~!>"+v.StoreToValue);//v.StoreToVariable
                        }
                        FinalizeTable();
                    }

                } //end of foreach Act

            } //end of foreach Activity

            string fileName = Generate(FileName);
            return fileName;
        }

         internal void SavePDFReport(Run.BusinessFlowExecutionSummary bf, string filename)
         {
             GeneratePDF(bf.BusinessFlow, filename);
         }
    }
}
