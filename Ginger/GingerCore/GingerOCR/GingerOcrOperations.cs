#region License
/*
Copyright © 2014-2025 European Support Limited

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
using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Local;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Tabula;
using UglyToad.PdfPig.Content;
using PdfDocument = PdfiumViewer.PdfDocument;
using PdfPigDocument = UglyToad.PdfPig.PdfDocument;

namespace GingerCore.GingerOCR
{
    public static class GingerOcrOperations
    {
        private static readonly object lockObject = new object();
        private static PaddleOcrAll instance = null;

        public static PaddleOcrAll Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        FullOcrModel model = LocalFullModels.EnglishV4;
                        instance = new PaddleOcrAll(model)
                        {
                            AllowRotateDetection = true,
                            Enable180Classification = false,

                        };
                    }
                    return instance;
                }
            }
        }

        private static PaddleOcrResult SafeRun(Mat mat)
        {
            lock (lockObject)
            {
                return Instance.Run(mat);
            }
        }

        public static string ReadTextFromImage(string imageFilePath)
        {
            string textOutput = string.Empty;
            try
            {
                using (var img = new Bitmap(imageFilePath))
                using (Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img))
                {
                    var ocrResult = SafeRun(mat);
                    textOutput = ocrResult.Text;
                    Reporter.ToLog(eLogLevel.INFO, $"Text obtained from image {imageFilePath} is successful.");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return textOutput;
        }

        public static string ReadTextFromImageAfterLabel(string imageFilePath, string label)
        {
            string txtOutput = string.Empty;
            try
            {
                using (var img = new Bitmap(imageFilePath))
                using (Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img))
                {
                    var ocrResult = SafeRun(mat);

                    foreach (var region in ocrResult.Regions)
                    {
                        if (region.Text.Contains(label))
                        {
                            int indexOf = region.Text.IndexOf(label) + label.Length;
                            txtOutput = region.Text.Substring(indexOf).Trim();
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return txtOutput;
        }

        public static string ReadTextFromImageBetweenStrings(string imageFilePath, string firstLabel, string secondLabel, ref string err)
        {
            using (var img = new Bitmap(imageFilePath))
            using (Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img))
            {
                var ocrResult = SafeRun(mat);
                return ReadTextBetweenTwoLabels(firstLabel, secondLabel, ocrResult, ref err);
            }
        }

        public static string ReadTextFromByteArray(byte[] byteArray)
        {
            string txtOutput = string.Empty;
            try
            {
                using (Mat src = Cv2.ImDecode(byteArray, ImreadModes.Color))
                {
                    var result = SafeRun(src);
                    txtOutput = result.Text;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return txtOutput;
        }

        private static PaddleOcrResult ReadOcrResultFromByteArray(byte[] byteArray)
        {
            try
            {
                using (var ms = new MemoryStream(byteArray))
                using (var img = (Bitmap)Image.FromStream(ms))
                using (Mat mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(img))
                {
                    return SafeRun(mat);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                return null;
            }
        }

        private static string ReadTextBetweenTwoLabels(string firstLabel, string secondLabel, PaddleOcrResult ocrResult, ref string err)
        {
            StringBuilder resultTxt = new StringBuilder();
            bool startedReading = false;

            if (ocrResult == null || string.IsNullOrWhiteSpace(ocrResult.Text))
            {
                err = "OCR result is empty.";
                return string.Empty;
            }

            try
            {
                foreach (var region in ocrResult.Regions)
                {
                    string lineTxt = region.Text;

                    if (!startedReading)
                    {
                        int firstIndexOf = lineTxt.IndexOf(firstLabel);
                        if (firstIndexOf != -1)
                        {
                            startedReading = true;
                            int secondIndexOf = lineTxt.IndexOf(secondLabel, firstIndexOf + firstLabel.Length);

                            if (secondIndexOf != -1)
                            {
                                resultTxt.Append(lineTxt.Substring(firstIndexOf + firstLabel.Length, secondIndexOf - (firstIndexOf + firstLabel.Length)));
                                break;
                            }
                            else
                            {
                                resultTxt.Append(lineTxt.Substring(firstIndexOf + firstLabel.Length));
                            }
                        }
                    }
                    else
                    {
                        int secondIndexOf = lineTxt.IndexOf(secondLabel);
                        if (secondIndexOf != -1)
                        {
                            resultTxt.Append(lineTxt.Substring(0, secondIndexOf));
                            break;
                        }
                        else
                        {
                            resultTxt.Append(lineTxt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                err = "Unable to read text from Image";
            }

            return resultTxt.ToString();
        }

        // ---------------- PDF → Image Conversion Helpers ----------------

        private static List<string> GetListOfPageNos(string pageNumber)
        {
            return pageNumber.Split(',')
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrEmpty(p))
                             .ToList();
        }

        private static List<byte[]> GetPngByteArrayFromPdf(string pdfFilePath, string pageNum, int dpi, string password = null)
        {
            var list = new List<byte[]>();
            var pageNumbers = GetListOfPageNos(pageNum);

            using (var pdf = string.IsNullOrEmpty(password) ?
                             PdfDocument.Load(pdfFilePath) :
                             PdfDocument.Load(pdfFilePath, password))
            {
                foreach (var p in pageNumbers)
                {
                    if (int.TryParse(p, out int pageIndex))
                    {
                        if (pageIndex > 0 && pageIndex <= pdf.PageCount)
                        {
                            using (var img = pdf.Render(pageIndex - 1, dpi, dpi, true))
                            using (var ms = new MemoryStream())
                            {
                                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                list.Add(ms.ToArray());
                            }
                        }
                    }
                }
            }
            return list;
        }

        private static List<byte[]> GetListOfPngByteArrayFromPdf(string pdfFilePath, int dpi, string password = null)
        {
            var list = new List<byte[]>();
            using (var pdf = string.IsNullOrEmpty(password) ?
                             PdfDocument.Load(pdfFilePath) :
                             PdfDocument.Load(pdfFilePath, password))
            {
                for (int i = 0; i < pdf.PageCount; i++)
                {
                    using (var img = pdf.Render(i, dpi, dpi, true))
                    using (var ms = new MemoryStream())
                    {
                        img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        list.Add(ms.ToArray());
                    }
                }
            }
            return list;
        }

        // ---------------- PDF OCR Methods ----------------

        public static string ReadTextFromPdfSinglePage(string pdfFilePath, string pageNum, int dpi, string password = null)
        {
            StringBuilder result = new StringBuilder();
            try
            {
                var pages = GetPngByteArrayFromPdf(pdfFilePath, pageNum, dpi, password);
                foreach (var pageBytes in pages)
                {
                    using (Mat src = Cv2.ImDecode(pageBytes, ImreadModes.Color))
                    {
                        var ocrResult = SafeRun(src);
                        result.AppendLine(string.Join(Environment.NewLine, ocrResult.Regions.Select(r => r.Text)));
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return result.ToString();
        }

        public static string ReadTextAfterLabelPdf(string pdfFilePath, string label, int dpi, string pageNum = "", string password = null)
        {
            string resultTxt = string.Empty;
            try
            {
                var pages = !string.IsNullOrEmpty(pageNum)
                    ? GetPngByteArrayFromPdf(pdfFilePath, pageNum, dpi, password)
                    : GetListOfPngByteArrayFromPdf(pdfFilePath, dpi, password);

                foreach (var pngByte in pages)
                {
                    using (Mat src = Cv2.ImDecode(pngByte, ImreadModes.Color))
                    {
                        var ocrResult = SafeRun(src);

                        foreach (var region in ocrResult.Regions)
                        {
                            if (region.Text.Contains(label))
                            {
                                int indexOf = region.Text.IndexOf(label) + label.Length;
                                resultTxt = region.Text.Substring(indexOf).Trim();
                                return resultTxt;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return resultTxt;
        }

        public static string ReadTextBetweenLabelsPdf(string pdfFilePath, string firstLabel, string secondLabel, string pageNum, int dpi, ref string err, string password = null)
        {
            StringBuilder resultTxt = new StringBuilder();
            try
            {
                var pages = !string.IsNullOrEmpty(pageNum)
                    ? GetPngByteArrayFromPdf(pdfFilePath, pageNum, dpi, password)
                    : GetListOfPngByteArrayFromPdf(pdfFilePath, dpi, password);

                foreach (var pngByte in pages)
                {
                    using (Mat src = Cv2.ImDecode(pngByte, ImreadModes.Color))
                    {
                        var ocrResult = SafeRun(src);
                        bool started = false;

                        foreach (var region in ocrResult.Regions)
                        {
                            string lineTxt = region.Text;

                            if (!started)
                            {
                                int idx1 = lineTxt.IndexOf(firstLabel);
                                if (idx1 != -1)
                                {
                                    started = true;
                                    int idx2 = lineTxt.IndexOf(secondLabel, idx1 + firstLabel.Length);
                                    if (idx2 != -1)
                                    {
                                        resultTxt.Append(lineTxt.Substring(idx1 + firstLabel.Length, idx2 - (idx1 + firstLabel.Length)));
                                        return resultTxt.ToString();
                                    }
                                    else
                                    {
                                        resultTxt.Append(lineTxt.Substring(idx1 + firstLabel.Length));
                                    }
                                }
                            }
                            else
                            {
                                int idx2 = lineTxt.IndexOf(secondLabel);
                                if (idx2 != -1)
                                {
                                    resultTxt.Append(lineTxt.Substring(0, idx2));
                                    return resultTxt.ToString();
                                }
                                else
                                {
                                    resultTxt.Append(lineTxt);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                err = "Unable to read text between labels.";
            }
            return string.Empty;
        }

        public static Dictionary<string, object> ReadTextFromPdfAllPages(string pdfFilePath, int dpi, string password = null)
        {
            var result = new Dictionary<string, object>();
            try
            {
                var pages = GetListOfPngByteArrayFromPdf(pdfFilePath, dpi, password);

                for (int i = 0; i < pages.Count; i++)
                {
                    using (Mat src = Cv2.ImDecode(pages[i], ImreadModes.Color))
                    {
                        var ocrResult = SafeRun(src);
                        result[$"Page_{i + 1}"] = string.Join(Environment.NewLine, ocrResult.Regions.Select(r => r.Text));
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return result;
        }

        // ---------------- Table extraction (with Tabula) ----------------
        public static string ReadTextFromPdfTable(
      string pdfFilePath,
      string columnName,
      string pageNumber,
      bool useRowNumber,
      int rowNumber,
      ActOcr.eTableElementRunColOperator elementLocateBy,
      string conditionColumnName,
      string conditionColumnValue,
      string password = null)
        {
            try
            {
                using (PdfPigDocument document = PdfPigDocument.Open(pdfFilePath))
                {
                    int pageIndex = string.IsNullOrEmpty(pageNumber) ? 1 : int.Parse(pageNumber);
                    Page page = document.GetPage(pageIndex);

                    // Extract words with bounding boxes
                    var words = page.GetWords();

                    // Group words by line (using Y coordinate)
                    var rows = words
                        .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1)) // group by Y
                        .OrderByDescending(g => g.Key) // top to bottom
                        .ToList();

                    // Convert to row -> list of words (sorted left to right)
                    var tableRows = rows.Select(r => r.OrderBy(w => w.BoundingBox.Left).ToList()).ToList();

                    // Try to locate column index by columnName
                    int colIndex = -1;
                    if (!string.IsNullOrEmpty(columnName) && tableRows.Count > 0)
                    {
                        var headerRow = tableRows.First();
                        colIndex = headerRow.FindIndex(w => w.Text.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    }

                    if (colIndex == -1)
                    {
                        Reporter.ToLog(eLogLevel.WARN, $"Column '{columnName}' not found in PDF table.");
                        return string.Empty;
                    }

                    if (useRowNumber)
                    {
                        if (rowNumber < tableRows.Count)
                        {
                            var targetRow = tableRows[rowNumber];
                            if (colIndex < targetRow.Count)
                                return targetRow[colIndex].Text;
                        }
                    }
                    else if (!string.IsNullOrEmpty(conditionColumnName) && !string.IsNullOrEmpty(conditionColumnValue))
                    {
                        // Locate condition column index
                        int condIndex = tableRows.First().FindIndex(w => w.Text.Equals(conditionColumnName, StringComparison.OrdinalIgnoreCase));

                        if (condIndex != -1)
                        {
                            foreach (var row in tableRows.Skip(1))
                            {
                                if (condIndex < row.Count && row[condIndex].Text.Contains(conditionColumnValue))
                                {
                                    if (colIndex < row.Count)
                                        return row[colIndex].Text;
                                }
                            }
                        }
                    }

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error reading table from PDF: {ex.Message}", ex);
                return string.Empty;
            }
        }

        private static void GetTableDataFromPageArea(string columnName, bool useRowNumber, int rowNumber, string conditionColumnName, string conditionColumnValue, ref string txtOutput, Table table, ActOcr.eTableElementRunColOperator elementLocateBy)
        {
            var rows = table.Rows;
            if (rows.Count == 0) return;

            var header = rows.First().Select(cell => cell.GetText()).ToList();
            if (!header.Contains(columnName)) return;

            int colIndex = header.IndexOf(columnName);

            for (int i = 1; i < rows.Count; i++)
            {
                var row = rows[i];

                if (useRowNumber && i == rowNumber)
                {
                    txtOutput = row[colIndex].GetText();
                    break;
                }

                if (!string.IsNullOrEmpty(conditionColumnName) && header.Contains(conditionColumnName))
                {
                    int condIndex = header.IndexOf(conditionColumnName);
                    if (row[condIndex].GetText().Equals(conditionColumnValue, StringComparison.OrdinalIgnoreCase))
                    {
                        txtOutput = row[colIndex].GetText();
                        break;
                    }
                }
            }
        }
    }
}
