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
using Tabula;
using Tabula.Detectors;
using Tabula.Extractors;
using UglyToad.PdfPig;
using Tesseract;
using Freeware;
using System.IO;
using System.Text;
using PageIterator = Tabula.PageIterator;

namespace GingerCore.GingerOCR
{
    public static class GingerOcrOperations
    {
        private static readonly object lockObject = new object();
        private static TesseractEngine instance = null;
        public static TesseractEngine Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        string exeFilePath = Path.GetDirectoryName(typeof(GingerOcrOperations).Assembly.Location);
                        string tessDataFilePath = Path.Combine(exeFilePath, "tessdata");
                        instance = new TesseractEngine(tessDataFilePath, "eng", EngineMode.Default);
                    }
                    return instance;
                }
            }
        }
        public static string ReadTextFromImage(string imageFilePath)
        {
            string textOutput = string.Empty;
            try
            {
                using (Pix img = Pix.LoadFromFile(imageFilePath))
                {
                    using (Page page = Instance.Process(img))
                    {
                        textOutput = page.GetText();
                        Reporter.ToLog(eLogLevel.INFO, string.Format("Text obtained from image {0} is successful with mean confidence: {1}",
                                        imageFilePath, page.GetMeanConfidence().ToString()));
                    }
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
            Page pageObj = GetPageObjectFromFilePath(imageFilePath);
            using (pageObj)
            {
                using (ResultIterator iter = pageObj.GetIterator())
                {
                    try
                    {
                        iter.Begin();
                        do
                        {
                            string lineTxt = iter.GetText(PageIteratorLevel.TextLine);
                            if (lineTxt.Contains(label))
                            {
                                int indexOf = lineTxt.IndexOf(label) + label.Length;
                                txtOutput = lineTxt.Substring(indexOf);
                                return txtOutput;
                            }
                        } while (iter.Next(PageIteratorLevel.TextLine));
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                    }
                }
            }
            return txtOutput;
        }

        public static string ReadTextFromImageBetweenStrings(string imageFilePath, string firstLabel, string secondLabel)
        {
            string resultTxt = string.Empty;
            Page pageObj = GetPageObjectFromFilePath(imageFilePath);
            int firstIndexOf = -1;
            int secondIndexOf = -1;

            ReadTextBetweenTwoLabels(firstLabel, secondLabel, ref resultTxt, ref firstIndexOf, ref secondIndexOf, pageObj);
            if (firstIndexOf != -1 && secondIndexOf != -1)
            {
                return resultTxt;
            }

            return resultTxt;
        }

        private static string ReadTextFromByteArray(byte[] byteArray)
        {
            string txtOutput = string.Empty;
            try
            {
                using (Pix img = Pix.LoadFromMemory(byteArray))
                {
                    using (Page page = Instance.Process(img))
                    {
                        txtOutput = page.GetText();
                        Reporter.ToLog(eLogLevel.INFO, string.Format("Text obtained is successful with mean confidence: {0}",
                                        page.GetMeanConfidence().ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return txtOutput;
        }

        private static List<byte[]> GetPngByteArrayFromPdf(string pdfFilePath, string pageNum, string password = null)
        {
            List<byte[]> lstPngByte = new List<byte[]>();
            byte[] pdfByteArray = File.ReadAllBytes(pdfFilePath);
            List<string> lstPageNum = GetListOfPageNos(pageNum);
            foreach (string pgNum in lstPageNum)
            {
                byte[] pngByte = Pdf2Png.Convert(pdfByteArray, int.Parse(pgNum), 300, password);
                lstPngByte.Add(pngByte);
            }
            return lstPngByte;
        }

        private static List<byte[]> GetListOfPngByteArrayFromPdf(string pdfFilePath, string password = null)
        {
            byte[] pdfByteArray = File.ReadAllBytes(pdfFilePath);
            List<byte[]> pngByte = Pdf2Png.ConvertAllPages(pdfByteArray, 300, password);
            return pngByte;
        }

        public static string ReadTextFromPdfSinglePage(string pdfFilePath, string pageNum, string password = null)
        {
            string txtOutput = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(pageNum))
                {
                    List<byte[]> lstPngByte = GetPngByteArrayFromPdf(pdfFilePath, pageNum, password);

                    foreach (byte[] pngByte in lstPngByte)
                    {
                        using (Page pageObj = GetPageObjectFromByteArray(pngByte))
                        {
                            txtOutput = string.Concat(txtOutput, pageObj.GetText(), Environment.NewLine);
                        }
                    }
                }
                else
                {
                    List<byte[]> lstByteArray = GetListOfPngByteArrayFromPdf(pdfFilePath, password);
                    foreach (byte[] byteArray in lstByteArray)
                    {
                        using (Page pageObj = GetPageObjectFromByteArray(byteArray))
                        {
                            txtOutput = string.Concat(txtOutput, pageObj.GetText(), Environment.NewLine);
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

        private static Page GetPageObjectFromByteArray(byte[] byteArray)
        {
            try
            {
                using (Pix img = Pix.LoadFromMemory(byteArray))
                {
                    Page page = Instance.Process(img);
                    return page;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return null;
        }

        private static Page GetPageObjectFromFilePath(string filePath)
        {
            try
            {
                using (Pix img = Pix.LoadFromFile(filePath))
                {
                    return Instance.Process(img);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return null;
        }

        public static string ReadTextAfterLabelPdf(string pdfFilePath, string label, string pageNum = "", string password = null)
        {
            string resultTxt = string.Empty;
            if (!string.IsNullOrEmpty(pageNum))
            {
                List<byte[]> lstPngByte = GetPngByteArrayFromPdf(pdfFilePath, pageNum, password);

                foreach (byte[] pngByte in lstPngByte)
                {
                    resultTxt = GetResultTextFromByteArray(label, resultTxt, pngByte);
                    if (!string.IsNullOrEmpty(resultTxt))
                    {
                        break;
                    }
                }
            }
            else
            {
                List<byte[]> lstByteArray = GetListOfPngByteArrayFromPdf(pdfFilePath, password);
                foreach (byte[] byteArray in lstByteArray)
                {
                    resultTxt = GetResultTextFromByteArray(label, resultTxt, byteArray);
                    if (!string.IsNullOrEmpty(resultTxt))
                    {
                        break;
                    }
                }
            }
            return resultTxt;
        }

        private static string GetResultTextFromByteArray(string label, string resultTxt, byte[] byteArray)
        {
            Page pageObj = GetPageObjectFromByteArray(byteArray);
            if (pageObj != null)
            {
                try
                {
                    using (pageObj)
                    {
                        using (ResultIterator iter = pageObj.GetIterator())
                        {
                            try
                            {
                                iter.Begin();
                                do
                                {
                                    string lineTxt = iter.GetText(PageIteratorLevel.TextLine);
                                    if (lineTxt.Contains(label))
                                    {
                                        int indexOf = lineTxt.IndexOf(label) + label.Length;
                                        resultTxt = lineTxt.Substring(indexOf);
                                        break;
                                    }
                                } while (iter.Next(PageIteratorLevel.TextLine));
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                }
                finally
                {
                    pageObj.Dispose();
                }
            }
            return resultTxt;
        }

        public static string ReadTextBetweenLabelsPdf(string pdfFilePath, string firstLabel, string secondLabel, string pageNum, string password = null)
        {
            string resultTxt = string.Empty;
            int firstIndexOf = -1;
            int secondIndexOf = -1;
            if (!string.IsNullOrEmpty(pageNum))
            {
                List<byte[]> lstPngByte = GetPngByteArrayFromPdf(pdfFilePath, pageNum, password);
                foreach (byte[] pngByte in lstPngByte)
                {
                    Page pageObj = GetPageObjectFromByteArray(pngByte);
                    ReadTextBetweenTwoLabels(firstLabel, secondLabel, ref resultTxt, ref firstIndexOf, ref secondIndexOf, pageObj);
                    if (firstIndexOf != -1 && secondIndexOf != -1)
                    {
                        return resultTxt;
                    }
                }
            }
            else
            {
                List<byte[]> lstByteArray = GetListOfPngByteArrayFromPdf(pdfFilePath, password);
                foreach (byte[] byteArray in lstByteArray)
                {
                    Page pageObj = GetPageObjectFromByteArray(byteArray);
                    ReadTextBetweenTwoLabels(firstLabel, secondLabel, ref resultTxt, ref firstIndexOf, ref secondIndexOf, pageObj);
                    if (firstIndexOf != -1 && secondIndexOf != -1)
                    {
                        return resultTxt;
                    }
                }
            }
            return string.Empty;
        }

        private static void ReadTextBetweenTwoLabels(string firstLabel, string secondLabel, ref string resultTxt, ref int firstIndexOf, ref int secondIndexOf, Page pageObj)
        {
            using (pageObj)
            {
                using (ResultIterator iter = pageObj.GetIterator())
                {
                    iter.Begin();
                    try
                    {
                        do
                        {
                            string lineTxt = iter.GetText(PageIteratorLevel.TextLine);
                            if (lineTxt.Contains(firstLabel))
                            {
                                firstIndexOf = lineTxt.IndexOf(firstLabel) + firstLabel.Length;
                                if (lineTxt.Contains(secondLabel))
                                {
                                    secondIndexOf = lineTxt.IndexOf(secondLabel);
                                    resultTxt = lineTxt.Substring(firstIndexOf, secondIndexOf - firstIndexOf);
                                    return;
                                }
                                else
                                {
                                    resultTxt = string.Concat(resultTxt, lineTxt.Substring(firstIndexOf));
                                    continue;
                                }
                            }
                            if (firstIndexOf != -1)
                            {
                                if (lineTxt.Contains(secondLabel))
                                {
                                    secondIndexOf = lineTxt.IndexOf(secondLabel);
                                    resultTxt = string.Concat(resultTxt, lineTxt.Substring(0, secondIndexOf));
                                    break;
                                }
                                else
                                {
                                    resultTxt = string.Concat(resultTxt, lineTxt);
                                }
                            }
                        } while (iter.Next(PageIteratorLevel.TextLine));
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                    }
                }
            }
        }

        public static Dictionary<string, object> ReadTextFromPdfAllPages(string pdfFilePath, string password = null)
        {
            Dictionary<string, object> dctOutput = new Dictionary<string, object>();

            try
            {
                byte[] pdfByteArray = File.ReadAllBytes(pdfFilePath);
                List<byte[]> pngArray = Pdf2Png.ConvertAllPages(pdfByteArray, 300, password);
                for (int i = 0; i < pngArray.Count; i++)
                {
                    string output = ReadTextFromByteArray(pngArray[i]);
                    dctOutput.Add("Page No. " + i, output);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
            return dctOutput;
        }

        public static string ReadTextFromPdfTable(string pdfFilePath, string columnName, string pageNumber, bool useRowNumber,
                                                  int rowNumber, ActOcr.eTableElementRunColOperator elementLocateBy, string conditionColumnName,
                                                  string conditionColumnValue, string password = null)
        {
            string txtOutput = string.Empty;
            try
            {
                using (PdfDocument document = PdfDocument.Open(pdfFilePath, new ParsingOptions() { ClipPaths = true, Password = password }))
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

            Reporter.ToLog(eLogLevel.ERROR, "Unable to find text in tables", null);
            return txtOutput;
        }

        private static void GetTableDataFromPageArea(string columnName, bool useRowNumber, int rowNumber, string conditionColumnName, string conditionColumnValue, ref string txtOutput, PageArea page, ActOcr.eTableElementRunColOperator elementLocateBy)
        {
            SpreadsheetDetectionAlgorithm detector = new SpreadsheetDetectionAlgorithm();

            IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();

            List<Table> tables = ea.Extract(page);
            foreach (Table table in tables)
            {
                if (useRowNumber)
                {
                    IReadOnlyList<IReadOnlyList<Cell>> rows = table.Rows;
                    for (int i = 0; i < rows[0].Count; i++)
                    {
                        Cell cellObj = rows[0][i];
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
                    for (i = 0; i < rows.Count; i++)
                    {
                        bool bIsConditionValFound = false;
                        for (j = 0; j < rows[i].Count; j++)
                        {
                            Cell cellObj = rows[i][j];
                            if (rows[0][j].GetText(false).Equals(conditionColumnName))
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
                                        if (!cellObj.GetText(false).Equals(conditionColumnValue))
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
                            txtOutput = rows[i][columnNameIndex].GetText(false);
                            return;
                        }
                    }
                }
            }
        }

        private static List<string> GetListOfPageNos(string pageNumber)
        {
            List<string> lstPageNos = new List<string>();
            try
            {
                bool isParse = true;

                if (pageNumber.Contains("-"))
                {
                    string[] pageArray = pageNumber.Split('-');
                    int i, j = 0;
                    isParse = int.TryParse(pageArray[0], out i);
                    if (isParse)
                    {
                        isParse = int.TryParse(pageArray[1], out j);
                    }
                    if (isParse)
                    {
                        for (; i <= j; i++)
                        {
                            lstPageNos.Add(i.ToString());
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Invalid Page Number");
                        return null;
                    }
                }
                else if (pageNumber.Contains(","))
                {
                    string[] pageArray = pageNumber.Split(',');
                    for (int i = 0; i < pageArray.Length; i++)
                    {
                        int j = 0;
                        isParse = int.TryParse(pageArray[i], out j);
                        if (!isParse)
                        {
                            Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Invalid Page Number");
                            return null;
                        }
                        lstPageNos.Add(pageArray[i]);
                    }
                }
                else
                {
                    int j = 0;
                    isParse = int.TryParse(pageNumber, out j);
                    if (!isParse)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Invalid Page Number");
                        return null;
                    }
                    lstPageNos.Add(pageNumber);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                return null;
            }
            return lstPageNos;
        }

    }
}
