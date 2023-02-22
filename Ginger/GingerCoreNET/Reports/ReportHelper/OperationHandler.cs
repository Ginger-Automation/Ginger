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

using Ginger.Reports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Reports.ReportHelper
{
   public class OperationHandler
    {
        public void CreateCustomerLogo(object a, string tempFolder)
        {
            HTMLReportConfiguration currentTemplate = (HTMLReportConfiguration)a;
            System.Drawing.Image CustomerLogo = this.Base64StringToImage(currentTemplate.LogoBase64Image.ToString());
            CustomerLogo.Save(Path.Combine(tempFolder,"CustomerLogo.png"));
            HTMLReportConfigurationOperations.EnchancingLoadedFieldsWithDataAndValidating(currentTemplate);
        }

        public System.Drawing.Image Base64StringToImage(string base64String)
        {
            // Convert base 64 string to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            // Convert byte[] to Image
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                return image;
            }
        }

        public void CreateChart(List<KeyValuePair<int, int>> y, string chartName, string Title, string tempFolder)
        {
            //Chart Chart1 = new Chart();
            //List<string> x = new List<string>() { "Passed", "Failed", "Stopped", "Other" };
            //List<int> yList = (from ylist in y select ylist.Key).ToList();
            //int xAxis = 0;
            //string total = "";
            //Chart1.BackColor = System.Drawing.Color.AliceBlue;
            //Chart1.BackColor = System.Drawing.Color.White;
            //Chart1.Series.Add(new Series());
            //ChartArea a1 = new ChartArea();
            //a1.Name = "Area";
            //Chart1.ChartAreas.Add(a1);
            //a1.InnerPlotPosition = new ElementPosition(12, 10, 78, 78);
            //Chart1.Series[0].ChartArea = "Area";
            //Chart1.Series[0].Points.DataBindXY(x, yList);
            //Chart1.Series["Series1"].Label = "#VALX (#VALY)";
            //Chart1.Series[0].ChartType = SeriesChartType.Doughnut;
            //Chart1.Series[0]["DoughnutRadius"] = "20";
            //Chart1.Series[0]["DoughnutInnerRadius"] = "99";
            //Chart1.Series[0]["PieLabelStyle"] = "Outside";
            //Chart1.Series[0].BorderWidth = 1;
            //Chart1.Series[0].BorderDashStyle = ChartDashStyle.Dot;
            //Chart1.Series[0].BorderColor = System.Drawing.Color.FromArgb(200, 26, 59, 105);
            //foreach (KeyValuePair<int, int> l in y)
            //{
            //    if (l.Key == 0)
            //    {
            //        Chart1.Series[0].Points[l.Value].BorderColor = System.Drawing.Color.White;
            //        Chart1.Series["Series1"].Points[l.Value].AxisLabel = "";
            //        Chart1.Series["Series1"].Points[l.Value].Label = "";
            //    }
            //}
            //Chart1.Series[0].Points[0].Color = Chart1.Series[0].Points[0].LabelForeColor = GingerCore.General.makeColor("#008000");
            //Chart1.Series[0].Points[1].Color = Chart1.Series[0].Points[1].LabelForeColor = GingerCore.General.makeColor("#FF0000");
            //Chart1.Series[0].Points[2].Color = Chart1.Series[0].Points[2].LabelForeColor = GingerCore.General.makeColor("#ff57ab");
            //Chart1.Series[0].Points[3].Color = Chart1.Series[0].Points[3].LabelForeColor = GingerCore.General.makeColor("#1B3651");
            //Chart1.Series[0].Font = new Font("sans-serif", 9, System.Drawing.FontStyle.Bold);
            //Chart1.Height = 180;
            //Chart1.Width = 310;
            //System.Drawing.SolidBrush myBrush = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#e3dfdb"));
            //System.Drawing.SolidBrush myBrush1 = new System.Drawing.SolidBrush(GingerCore.General.makeColor("#1B3651"));
            //Chart1.Titles.Add("NewTitle");
            //Chart1.Titles["Title1"].Text = Title;
            //Chart1.Titles["Title1"].Font = new Font("sans-serif", 11, System.Drawing.FontStyle.Bold);
            //Chart1.Titles["Title1"].ForeColor = GingerCore.General.makeColor("#1B3651");
            //MemoryStream m = new MemoryStream();
            //Chart1.SaveImage(m, ChartImageFormat.Png);
            //Bitmap bitMapImage = new System.Drawing.Bitmap(m);
            //Graphics graphicImage = Graphics.FromImage(bitMapImage);
            //graphicImage.SmoothingMode = SmoothingMode.AntiAlias;
            //graphicImage.FillEllipse(myBrush, 132, 75, 50, 50);
            //total = yList.Sum().ToString();
            //if (total.Length == 1)
            //{
            //    xAxis = 151;
            //}
            //else if (total.Length == 2)
            //{
            //    xAxis = 145;
            //}
            //else if (total.Length == 3)
            //{
            //    xAxis = 142;
            //}
            //else if (total.Length == 4)
            //{
            //    xAxis = 140;
            //}
            //graphicImage.DrawString(yList.Sum().ToString(), new Font("sans-serif", 9, System.Drawing.FontStyle.Bold), myBrush1, new System.Drawing.Point(xAxis, 91));
            //m = new MemoryStream();
            //bitMapImage.Save(tempFolder + "\\" + chartName, ImageFormat.Jpeg);
            //graphicImage.Dispose();
            //bitMapImage.Dispose();
        }

    }
}
