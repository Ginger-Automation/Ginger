#region License
/*
Copyright © 2014-2026 European Support Limited

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
using GingerCore.Actions;
using OpenCvSharp;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models;
using Sdcb.PaddleOCR.Models.Online;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;
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
                        instance = new PaddleOcrAll(GetOCRModel())
                        {
                            AllowRotateDetection = true,
                            Enable180Classification = false,


                        };
                    }
                    return instance;
                }
            }
        }

        public static FullOcrModel GetOCRModel()
        {
            try
            {
                // Path to the local Paddle OCR model directory
                Settings.GlobalModelDirectory = Path.Combine(Path.GetDirectoryName(typeof(GingerOcrOperations).Assembly.Location), "OcrModels");
                var downloadTask = OnlineFullModels.EnglishV4.DownloadAsync();
                return downloadTask.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to download OCR model: {ex.Message}", ex);
                throw new InvalidOperationException("OCR model download failed. Please check internet connection and try again.", ex);
            }

        }


        public static string ReadTextFromImage(string imageFilePath)
        {
            string textOutput = string.Empty;
            try
            {
                using (Mat mat = Cv2.ImRead(imageFilePath, ImreadModes.Color))
                {
                    var ocrResult = Instance.Run(mat);
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
                using (Mat mat = Cv2.ImRead(imageFilePath, ImreadModes.Color))
                {
                    var ocrResult = Instance.Run(mat);

                    foreach (var region in ocrResult.Regions)
                    {
                        var txt = region.Text ?? string.Empty;
                        int indexOf = txt.IndexOf(label, StringComparison.OrdinalIgnoreCase);
                        if (indexOf >= 0)
                        {
                            txtOutput = txt.Substring(indexOf + label.Length).Trim();
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
            using (Mat mat = Cv2.ImRead(imageFilePath, ImreadModes.Color))
            {
                var ocrResult = Instance.Run(mat);
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
                    var result = Instance.Run(src);
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
                    return Instance.Run(mat);
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
                    string lineTxt = region.Text ?? string.Empty;

                    if (!startedReading)
                    {
                        int firstIndexOf = lineTxt.IndexOf(firstLabel, StringComparison.OrdinalIgnoreCase);
                        if (firstIndexOf != -1)
                        {
                            startedReading = true;
                            int secondIndexOf = lineTxt.IndexOf(secondLabel, firstIndexOf + firstLabel.Length, StringComparison.OrdinalIgnoreCase);

                            if (secondIndexOf != -1)
                            {
                                resultTxt.Append(lineTxt.Substring(firstIndexOf + firstLabel.Length, secondIndexOf - (firstIndexOf + firstLabel.Length)));
                                break;
                            }
                            else
                            {
                                resultTxt.Append(lineTxt.Substring(firstIndexOf + firstLabel.Length));
                                resultTxt.Append(' '); // Adding the  space after every region
                            }
                        }
                    }
                    else
                    {
                        int secondIndexOf = lineTxt.IndexOf(secondLabel, StringComparison.OrdinalIgnoreCase);
                        if (secondIndexOf != -1)
                        {
                            resultTxt.Append(lineTxt.Substring(0, secondIndexOf));
                            break;
                        }
                        else
                        {
                            resultTxt.Append(lineTxt);
                            resultTxt.Append(' '); // Adding the space after every region
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                err = "Unable to read text from Image";
            }

            return resultTxt.ToString().Trim();
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
                if (!string.IsNullOrEmpty(pageNum))
                {

                    List<byte[]> pages = GetPngByteArrayFromPdf(pdfFilePath, pageNum, dpi, password);
                    foreach (byte[] pageBytes in pages)
                    {
                        using (Mat src = Cv2.ImDecode(pageBytes, ImreadModes.Color))
                        {
                            var ocrResult = Instance.Run(src);
                            foreach (var region in ocrResult.Regions)
                            {
                                if (!string.IsNullOrEmpty(region.Text))
                                {
                                    result.AppendLine(region.Text);
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<byte[]> lstByteArray = GetListOfPngByteArrayFromPdf(pdfFilePath, dpi, password);
                    foreach (byte[] byteArray in lstByteArray)
                    {
                        using (Mat src = Cv2.ImDecode(byteArray, ImreadModes.Color))
                        {
                            var ocrResult = Instance.Run(src);
                            result.AppendLine(string.Join(Environment.NewLine, ocrResult.Regions.Select(r => r.Text)));
                        }
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
                        var ocrResult = Instance.Run(src);

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
                        var ocrResult = Instance.Run(src);
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
                        var ocrResult = Instance.Run(src);
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
        public static string ReadTextFromPdfTable(string pdfFilePath, string columnName, string pageNumber, bool useRowNumber,
                                                 int rowNumber, ActOcr.eTableElementRunColOperator elementLocateBy, string conditionColumnName,
                                                 string conditionColumnValue, string password = null)
        {
            string txtOutput = string.Empty;
            try
            {
                using (PdfPigDocument document = PdfPigDocument.Open(pdfFilePath, new ParsingOptions() { ClipPaths = true, Password = password }))
                {
                    ObjectExtractor oe = new ObjectExtractor(document);
                    if (!string.IsNullOrEmpty(pageNumber))
                    {
                        List<string> lstPageNum = GetListOfPageNos(pageNumber);
                        foreach (string pageNum in lstPageNum)
                        {
                            PageArea page = oe.Extract(int.Parse(pageNum));

                            // detect canditate table zones
                            GetTableDataFromPageArea(columnName, useRowNumber, rowNumber, conditionColumnName, conditionColumnValue, ref txtOutput, page, elementLocateBy);
                            if (!string.IsNullOrEmpty(txtOutput))
                            {
                                return txtOutput;
                            }
                        }

                    }
                    else
                    {
                        PageIterator pgIterator = oe.Extract();
                        using (pgIterator)
                        {
                            pgIterator.MoveNext();
                            while (pgIterator.Current != null)
                            {
                                PageArea pgArea = pgIterator.Current;
                                GetTableDataFromPageArea(columnName, useRowNumber, rowNumber, conditionColumnName, conditionColumnValue, ref txtOutput, pgArea, elementLocateBy);
                                if (!string.IsNullOrEmpty(txtOutput))
                                {
                                    return txtOutput;
                                }
                                if (!pgIterator.MoveNext())
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }

            if (string.IsNullOrEmpty(txtOutput))
            {
                Reporter.ToLog(eLogLevel.INFO, $"No matching text found in PDF table for column '{columnName}' with specified conditions");
            }
            return txtOutput;
        }


        private static void GetTableDataFromPageArea(string columnName, bool useRowNumber, int rowNumber, string conditionColumnName, string conditionColumnValue, ref string txtOutput, PageArea page, ActOcr.eTableElementRunColOperator elementLocateBy)
        {

            SpreadsheetExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();

            List<Table> tables = ea.Extract(page);
            foreach (Table table in tables)
            {
                if (useRowNumber)
                {
                    IReadOnlyList<IReadOnlyList<Cell>> rows = table.Rows;
                    if (rows.Count < 2 || rowNumber + 1 >= rows.Count)
                    {
                        continue; // Skip this table if not enough rows
                    }
                    for (int i = 0; i < rows[0].Count; i++)
                    {
                        Cell cellObj = rows[0][i];
                        if (cellObj.GetText(false).Equals(columnName))
                        {
                            txtOutput = rows[rowNumber + 1][i].GetText(false);
                            return;
                        }
                    }

                    for (int i = 0; i < rows[1].Count; i++)
                    {
                        Cell cellObj = rows[1][i];
                        if (cellObj.GetText(false).Equals(columnName))
                        {
                            txtOutput = rows[rowNumber + 1][i].GetText(false);
                            return;
                        }
                    }
                }
                else
                {
                    IReadOnlyList<IReadOnlyList<Cell>> rows = table.Rows;

                    int columnNameIndex = -1;
                    int i, j;
                    for (i = 0; i < rows[0].Count; i++)
                    {
                        Cell cellObj = rows[0][i];
                        if (cellObj.GetText(false).Equals(columnName))
                        {
                            columnNameIndex = i;
                            break;
                        }
                    }
                    for (i = 0; i < rows[1].Count; i++)
                    {
                        Cell cellObj = rows[1][i];
                        if (cellObj.GetText(false).Equals(columnName))
                        {
                            columnNameIndex = i;
                            break;
                        }
                    }
                    for (i = 0; i < rows.Count; i++)
                    {
                        bool bIsConditionValFound = false;
                        for (j = 0; j < rows[i].Count; j++)
                        {
                            Cell cellObj = rows[i][j];
                            if (rows.Count > 0 && j < rows[0].Count && rows[0][j].GetText(false).Equals(conditionColumnName))
                            {
                                switch (elementLocateBy)
                                {
                                    case ActOcr.eTableElementRunColOperator.Equals:
                                        if (cellObj.GetText(false).Equals(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.NotEquals:
                                        if (!cellObj.GetText(false).Equals(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.Contains:
                                        if (cellObj.GetText(false).Contains(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.NotContains:
                                        if (!cellObj.GetText(false).Contains(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.StartsWith:
                                        if (cellObj.GetText(false).StartsWith(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.NotStartsWith:
                                        if (!cellObj.GetText(false).StartsWith(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.EndsWith:
                                        if (cellObj.GetText(false).EndsWith(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    case ActOcr.eTableElementRunColOperator.NotEndsWith:
                                        if (!cellObj.GetText(false).EndsWith(conditionColumnValue))
                                        {
                                            bIsConditionValFound = true;
                                            break;
                                        }
                                        break;
                                    default:
                                        //do nothing
                                        break;
                                }
                            }
                            if (bIsConditionValFound)
                            {
                                break;
                            }
                        }
                        if (bIsConditionValFound)
                        {
                            if (columnNameIndex == -1)
                            {
                                break; // col not found
                            }
                                txtOutput = rows[i][columnNameIndex].GetText(false);
                            return;
                        }
                    }
                }
            }
        }
    }
}
