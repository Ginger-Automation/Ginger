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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.Helpers;
using GingerCore.Properties;
using PdfSharp.Drawing;
using PdfSharp.Charting;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp;
using System.Diagnostics;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;

namespace GingerCore.Actions
{
    public class ActCreatePDFChart : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Create PDF Chart Action"; } }
        public override string ActionUserDescription { get { return "Create PDF Chart from CSV data"; } }
        private List<string> Params;
        private ChartFrame chartFrame=new ChartFrame();
        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to create PDF chart from CSV file.");
            TBH.AddLineBreak();
            TBH.AddText("This action contains list of options which will allow you to create PDF chart from given CSV file.");
            TBH.AddLineBreak();
            TBH.AddText("To create PDF chart from given CSV file first select the CSV file by clicking Browse button.Once you selects the CSV file, all columns will get bind to Param Name dropdownlist then select param and run the action.");
        }

        public override string ActionEditPage { get { return "ActCreatePDFChartEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return true; } }

        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    AddAllPlatforms();
                }
                return mPlatforms;
            }
        }
        public enum eExcelActionType
        {
            ReadData = 0,     
            WriteData = 1,              
        }

        public new static partial class Fields
        {
            public static string DataFileName = "DataFileName";
            public static string ParamName = "ParamName";
            public static string ParamList = "ParamList";
        }

        [IsSerializedForLocalRepository]
        public string DataFileName
        {
            get
            {
                return GetInputParamValue("DataFileName");
            }
            set
            {
                AddOrUpdateInputParamValue("DataFileName", value);
                OnPropertyChanged(Fields.DataFileName);                
            }            
        }
             
        [IsSerializedForLocalRepository]
        public string ParamName
        {
            get
            {
                return GetInputParamValue("ParamName");
            }
            set
            {
                AddOrUpdateInputParamValue("ParamName", value);
            }
        }

        private List<string> mParamList;
        public List<string> ParamList
        {
            get
            {
                return mParamList;
            }
            set
            {
                mParamList = value;
                OnPropertyChanged(nameof(ParamList));
            }
        }
        public override string ActionType
        {
            get { return "Create PDF Chart from CSV data"; }
        }

        public override System.Drawing.Image Image { get { return Resources.Excel16x16; } }
        
        public override void Execute()
        {
              GenerateChart();             
        }

        public int getIndexOfParam()
        {
            if(!string.IsNullOrEmpty(GetDataFileName()))
            {
                StreamReader sr = new StreamReader(GetDataFileName());
                var lines = new List<string[]>();

                while (!sr.EndOfStream && lines.Count<=0)
                {
                    string[] Line = sr.ReadLine().Split(',').Select(a=>a.Trim()).ToArray();
                    lines.Add(Line);
                }
                if (lines[0]!=null)
                    Params = lines[0].ToList();
             }
             if (Params != null)
                return Params.IndexOf(ParamName);
             else
                return -1;
        }

        public void GenerateChart()
        {
            try
            {
                Chart chart = new Chart(ChartType.Column2D);
                Series series = null;
                string dataFileName = GetDataFileName();
                if(string.IsNullOrEmpty(dataFileName))
                {
                    this.Error = " Empty File Name ";
                    return;
                }
                var daraSeries = GetRenderingData(dataFileName);

                if (daraSeries != null)
                {
                    foreach (string key in daraSeries.Keys)
                    {
                        series = chart.SeriesCollection.AddSeries();
                        series.Name = key;

                        series.Add(new double[] { Convert.ToDouble(daraSeries[key][Params.IndexOf(ParamName)]) });
                    }
                }

                //  chart.XAxis.TickLabels.Format = "00";
                chart.XAxis.MajorTickMark = TickMarkType.Outside;
                chart.XAxis.Title.Caption = Path.GetFileNameWithoutExtension(DataFileName);

                chart.YAxis.MajorTickMark = TickMarkType.Outside;
                chart.YAxis.HasMajorGridlines = true;

                chart.PlotArea.LineFormat.Color = XColors.DarkGray;
                chart.PlotArea.LineFormat.Width = 1;
                chart.PlotArea.LineFormat.Visible = true;

                chart.Legend.Docking = DockingType.Right;
                
                chart.DataLabel.Type = DataLabelType.Value;
                chart.DataLabel.Position = DataLabelPosition.OutsideEnd;


                string filename = System.IO.Path.Combine(SolutionFolder , @"Documents\", Guid.NewGuid().ToString().ToUpper() + ".pdf");
                PdfDocument document = new PdfDocument(filename);
                chartFrame.Location = new XPoint(30, 30);
                chartFrame.Size = new XSize(500, 600);
                chartFrame.Add(chart);

                PdfPage page = document.AddPage();
                page.Size = PageSize.Letter;        

                XGraphics gfx = XGraphics.FromPdfPage(page);
                chartFrame.Draw(gfx);
                document.Close();
                Process.Start(filename);
            }
            catch 
            {
                this.Error = "Something went wrong when generating the PDF reports";
            }
        }

        private List<string[]> readDataFromCSV(string filePath)
        {
            StreamReader sr = new StreamReader(filePath);
            var lines = new List<string[]>();
            
            while (!sr.EndOfStream)
            {
                string[] Line = sr.ReadLine().Split(',');
                string[] newLine=new string[Line.Length];
                string tempLine;
                int i = 0;
                foreach(string ln in Line)
                {
                    if (ln.StartsWith("\"") || ln.EndsWith("\""))
                    {
                        tempLine = ln.Replace("\"", "");
                    }
                    else
                        tempLine = ln;
                    newLine[i++] = tempLine;
                }
                lines.Add(newLine);            
            }

            Params=lines[0].Select(a=>a.Trim()).ToList();
            lines.RemoveAt(0);
            return lines;
        }

        private Dictionary<string, string[]> GetRenderingData(string filePath)
        {
            var data=readDataFromCSV(filePath);
            if (data == null)
                return null;
            Dictionary<string,string[]> tmp = new Dictionary<string,string[]>();

            foreach (var el in data)
            {
                if (el[0] != null && !tmp.Keys.Contains(el[0]))
                {
                    tmp.Add(el[0], el);
                }
                else if (el[0] != null && tmp.Keys.Contains(el[0]))
                {
                    try
                    {
                        if (Convert.ToDateTime(el[1]) >= Convert.ToDateTime(tmp[el[0]][1]))
                            tmp[el[0]] = el;
                    }
                    catch(Exception e)
                    {

                    }
                }
                else
                    continue;
            }
            return tmp;
        }
        public string GetDataFileName()
        {
            object tempFileName = GetInputParamValue("DataFileName");
            if (ReferenceEquals(tempFileName, null))
            {
                return null;
            }
            string ExcelFileNameAbsolutue = Convert.ToString(tempFileName);
            ExcelFileNameAbsolutue = ExcelFileNameAbsolutue.ToUpper();

            if (ExcelFileNameAbsolutue.Contains(@"~\"))
            {
                ExcelFileNameAbsolutue = ExcelFileNameAbsolutue.Replace(@"~\", SolutionFolder);
            }
            return ExcelFileNameAbsolutue;
        }
    }
}