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
using System.Xml;
using System.Linq;
using System.IO;
using Amdocs.Ginger.Common;
using System.Text.RegularExpressions;
using Amdocs.Ginger.Common.GeneralLib;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Data.OleDb;
using GingerCore;
using Amdocs.Ginger.Common.APIModelLib;
using GingerCore.Environments;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using amdocs.ginger.GingerCoreNET;
using System.Text;
using GingerCore.DataSource;

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
   public class ImportOptionalValuesForParameters
    {
        public enum eParameterType
        {
            [EnumValueDescription("Local")]
            Local,
            [EnumValueDescription("Global")]
            Global
        }

        const string CURRENT_VAL_PARAMETER = "{Current Value}";
        const string PARAMETER_NAME = "Parameter Name";
        const string DESCRIPTION = "Description";

        public bool ShowMessage { get; set; }//For Non UI Unit Test
        public eParameterType ParameterType { get; set; }
        public ImportOptionalValuesForParameters() { ShowMessage = true; }
  
        #region XML&JSON
        private APIConfigurationsDocumentParserBase currentParser;
        public void CreateParser(string FilePath)
        {
            string FileType = System.IO.Path.GetExtension(FilePath);
            InitParser(FileType);
            if (currentParser == null)
            {
                FileType = APIConfigurationsDocumentParserBase.ParserTypeByContent(FilePath);
                InitParser(FileType);
            }
        }
        private void InitParser(string type)
        {
            switch (type)
            {
                case ".xml":
                case ".wsdl":
                    currentParser = new XMLTemplateParser();
                    break;
                case ".json":
                    currentParser = new JSONTemplateParser();
                    break;
                default:
                    currentParser = null;
                    break;
            }
        }
        public APIConfigurationsDocumentParserBase CurrentParser
        {
            get { return currentParser; }
            set { currentParser = value; }
        }
        public void GetAllOptionalValuesFromExamplesFiles(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            foreach (TemplateFile TF in ((ApplicationAPIModel)AAM).OptionalValuesTemplates)
            {
                CreateParser(TF.FilePath);
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    GetXMLAllOptionalValuesFromExamplesFile(TF, OptionalValuesPerParameterDict);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    GetJSONAllOptionalValuesFromExamplesFile(TF, OptionalValuesPerParameterDict);
                }
            }
        }
        /// <summary>
        /// Update all parameter optional values
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        public void PopulateOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            if (currentParser != null)
            {
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    PopulateXMLOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    PopulateJSONOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict);
                }
            }
        }
        /// <summary>
        /// Update optional values only for selected parameters
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, List<AppModelParameter> SelectedParametersGridList)
        {
            if (currentParser != null)
            {
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    PopulateXMLOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict, SelectedParametersGridList);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    PopulateJSONOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict, SelectedParametersGridList);
                }
            }
        }
        #endregion

        #region XML 
        private void GetXMLAllOptionalValuesFromExamplesFile(TemplateFile XMLTemplateFile, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            XmlDocument XmlDocument = new XmlDocument();
            string FileContent = File.ReadAllText(XMLTemplateFile.FilePath);
            XmlDocument.LoadXml(FileContent);
            XMLDocExtended XDE = new XMLDocExtended(XmlDocument);
            IEnumerable<XMLDocExtended> NodeList = XDE.GetEndingNodes(false);
            foreach (XMLDocExtended XDN in NodeList)
            {
                AddXMLValueToOptionalValuesPerParameterDict(OptionalValuesPerParameterDict, XDN);
            }
        }
        private void AddXMLValueToOptionalValuesPerParameterDict(Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, XMLDocExtended XDN)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(XDN.LocalName, XDN.XPath);
            string Value = XDN.Value;

            if (OptionalValuesPerParameterDict.ContainsKey(tuple))
            {
                OptionalValuesPerParameterDict[tuple].Add(Value);
            }
            else
            {
                OptionalValuesPerParameterDict.Add(tuple, new List<string>() { Value });
            }

            foreach (XmlAttribute attribute in XDN.Attributes)
            {

                Tuple<string, string> attributetuple = new Tuple<string, string>(attribute.LocalName, XDN.XPath);
                string attributeValue = attribute.Value;

                if (OptionalValuesPerParameterDict.ContainsKey(attributetuple))
                {
                    OptionalValuesPerParameterDict[attributetuple].Add(attributeValue);
                }
                else
                {
                    OptionalValuesPerParameterDict.Add(attributetuple, new List<string> { attributeValue });
                }
            }
        }
        /// <summary>
        /// Update all parameters optional values according to xml file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        public void PopulateXMLOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            int UpdatedParametersCounter = 0;
            foreach (AppModelParameter AMP in AAM.AppModelParameters)
            {
                string result = Regex.Match(AMP.Path, @"(.)*soapenv:Body\[1\]\/([a-zA-Z]|\d)*:").Value;
                if (string.IsNullOrEmpty(result))
                {
                    result = Regex.Match(AMP.Path, @"(.)*soapenv:Body\[1\]\/").Value;
                }

                string VAXBXPath = string.Empty;
                if (!string.IsNullOrEmpty(result))
                    VAXBXPath = AMP.Path.Replace(result, "//*[name()='vaxb:VAXB']/vaxb:");

                Tuple<string, string> tuple = new Tuple<string, string>(AMP.TagName, AMP.Path);
                Tuple<string, string> relativePathTuple = new Tuple<string, string>(AMP.TagName, VAXBXPath);
                if (OptionalValuesPerParameterDict.ContainsKey(tuple))
                {
                    currentParser.PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, tuple);
                }
                if (OptionalValuesPerParameterDict.ContainsKey(relativePathTuple))
                {
                    currentParser.PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, relativePathTuple);
                }
                if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                {
                    UpdatedParametersCounter++;
                    APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                }
            }
            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParametersCounter);
        }
        /// <summary>
        /// Update optional values only for selected parameters  according to xml file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateXMLOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, List<AppModelParameter> SelectedParametersGridList)
        {
            int UpdatedParametersCounter = 0;
            foreach (var tuple in SelectedParametersGridList.Zip(AAM.AppModelParameters, (x, y) => (x, y)))
            {
                if (tuple.x.RequiredAsInput)//selected
                {
                    string result = Regex.Match(tuple.y.Path, @"(.)*soapenv:Body\[1\]\/([a-zA-Z]|\d)*:").Value;
                    if (string.IsNullOrEmpty(result))
                    {
                        result = Regex.Match(tuple.y.Path, @"(.)*soapenv:Body\[1\]\/").Value;
                    }

                    string VAXBXPath = string.Empty;
                    if (!string.IsNullOrEmpty(result))
                    { VAXBXPath = tuple.y.Path.Replace(result, "//*[name()='vaxb:VAXB']/vaxb:"); }

                    Tuple<string, string> tupleKey = new Tuple<string, string>(tuple.y.TagName, tuple.y.Path);
                    Tuple<string, string> relativePathTuple = new Tuple<string, string>(tuple.y.TagName, VAXBXPath);
                    if (OptionalValuesPerParameterDict.ContainsKey(tupleKey))
                    {
                        currentParser.PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, tupleKey);
                    }
                    if (OptionalValuesPerParameterDict.ContainsKey(relativePathTuple))
                    {
                        currentParser.PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, relativePathTuple);
                    }
                    if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                    {
                        UpdatedParametersCounter++;
                        APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                    }
                }
            }
            if (ShowMessage)
            { Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParametersCounter); }
        }
        #endregion

        #region JSON
        private void GetJSONAllOptionalValuesFromExamplesFile(TemplateFile xMLTemplateFile, Dictionary<Tuple<string, string>, List<string>> optionalValuesPerParameterDict)
        {
            string FileContent = File.ReadAllText(xMLTemplateFile.FilePath);
            JsonExtended JE = new JsonExtended(FileContent);
            foreach (JsonExtended JTN in JE.GetEndingNodes())
            {
                try
                {
                    AddJSONValueToOptionalValuesPerParameterDict(optionalValuesPerParameterDict, JTN.GetToken());
                }
                catch(Exception ex)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
                }
            }
        }
        private void AddJSONValueToOptionalValuesPerParameterDict(Dictionary<Tuple<string, string>, List<string>> optionalValuesPerParameterDict, JToken xDN)
        {
            Tuple<string, string> tuple = new Tuple<string, string>(xDN.Path.Split('.').LastOrDefault(), xDN.Path);
            string Value = "";
            try
            {
                Value = ((JProperty)xDN).Value.ToString();
            }
            catch
            {
                Value = xDN.ToString();
            }
            if (optionalValuesPerParameterDict.ContainsKey(tuple))
            {

                optionalValuesPerParameterDict[tuple].Add(Value);
            }
            else
            {
                optionalValuesPerParameterDict.Add(tuple, new List<string>() { Value });
            }
        }
        /// <summary>
        /// Update all parameters optional values according to json file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        public void PopulateJSONOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict)
        {
            int UpdatedParametersCounter = 0;
            foreach (AppModelParameter AMP in AAM.AppModelParameters)
            {
                string result = AMP.Path;
                Tuple<string, string> tuple = new Tuple<string, string>(AMP.TagName, AMP.Path);
                if (OptionalValuesPerParameterDict.ContainsKey(tuple))
                {
                    currentParser.PopulateOptionalValuesByTuple(AMP, OptionalValuesPerParameterDict, tuple);
                    if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                    {
                        UpdatedParametersCounter++;
                        APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                    }
                }
            }
            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParametersCounter);
        }
        /// <summary>
        /// Update optional values only for selected parameter according to json file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="OptionalValuesPerParameterDict"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateJSONOptionalValuesForAPIParameters(ApplicationModelBase AAM, Dictionary<Tuple<string, string>, List<string>> OptionalValuesPerParameterDict, List<AppModelParameter> SelectedParametersGridList)
        {
            int UpdatedParametersCounter = 0;
            foreach (var tuple in SelectedParametersGridList.Zip(AAM.AppModelParameters, (x, y) => (x, y)))
            {
                if (tuple.x.RequiredAsInput)//selected
                {
                    Tuple<string, string> tupleKey = new Tuple<string, string>(tuple.y.TagName, tuple.y.Path);
                    if (OptionalValuesPerParameterDict.ContainsKey(tupleKey))
                    {
                        currentParser.PopulateOptionalValuesByTuple(tuple.y, OptionalValuesPerParameterDict, tupleKey);
                        if (APIConfigurationsDocumentParserBase.ParameterValuesUpdated)
                        {
                            UpdatedParametersCounter++;
                            APIConfigurationsDocumentParserBase.ParameterValuesUpdated = false;
                        }
                    }
                }
            }
            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParametersCounter);
        }
        #endregion

        #region DB&Excel
        /// <summary>
        /// Update optional values only for selected Local parameters according to DB\Excel file
        /// </summary>
        /// <param name="AAM"></param>
        /// <param name="SelectedParametersGridList"></param>
        public void PopulateExcelDBOptionalValuesForAPIParametersExcelDB(ApplicationModelBase AAM, List<AppModelParameter> SelectedParametersGridList, List<ParameterValues> ParameterValuesByNameDic)
        {
            int UpdatedParameters = 0;
            bool IsUpdate;
            List<AppModelParameter> RelevantParameterList = new List<AppModelParameter>();
            foreach(AppModelParameter prm in SelectedParametersGridList)
            {
                AppModelParameter pOriginal = AAM.AppModelParameters.FirstOrDefault(p => p.ItemName == prm.ItemName);
                if (pOriginal != null)
                {
                    RelevantParameterList.Add(pOriginal);
                }
                else
                {
                    RelevantParameterList.Add(prm);
                }
            }
            
            foreach (var tuple in SelectedParametersGridList.Zip(RelevantParameterList, (x, y) => (x, y)))
            {
                
                IsUpdate = false;
                if (tuple.x.RequiredAsInput) //selected
                {
                    var item = ParameterValuesByNameDic.FirstOrDefault(o => o.ParamName == tuple.y.ItemName);
                    if (item != null)
                    {
                        tuple.y.OptionalValuesList = new ObservableList<OptionalValue>();
                        foreach (string val in item.ParameterValuesByNameDic)
                        {
                            if (!string.IsNullOrEmpty(val))
                            {
                                OptionalValue OptionalValue = new OptionalValue();
                                OptionalValue.IsDefault = val.Contains("*") ? true : false;
                                OptionalValue.Value = val.Replace("*", "");
                                tuple.y.OptionalValuesList.Add(OptionalValue);
                                IsUpdate = true; 
                            }
                        }
                    }
                }

                AppModelParameter cur = AAM.AppModelParameters.FirstOrDefault(p => p.ItemName == tuple.y.ItemName);
                if (cur == null)
                {
                    AAM.AppModelParameters.Add(tuple.y);
                    IsUpdate = true;
                }

                int countDefault = tuple.y.OptionalValuesList.Where(t => t.IsDefault == true).Count();
                if (countDefault > 1)
                {
                    int indx = 1;
                    foreach (var opVal in tuple.y.OptionalValuesList.Where(t => t.IsDefault == true))
                    {
                        if(indx < countDefault)
                        {
                            opVal.IsDefault = false;
                        }
                        indx++;
                    }
                }

                if (IsUpdate)
                { UpdatedParameters++; }
            }
            
            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParameters);
        }
 
        public void PopulateExcelDBOptionalValuesForAPIParametersExcelDB(ObservableList<GlobalAppModelParameter> mGlobalParamterList, List<GlobalAppModelParameter> SelectedParametersGridList, List<ParameterValues> ParameterValuesByNameDic)
        {
            int UpdatedParameters = 0;
            bool IsUpdate;
            List<GlobalAppModelParameter> RelevantParameterList = new List<GlobalAppModelParameter>();
            foreach (GlobalAppModelParameter prm in SelectedParametersGridList)
            {
                GlobalAppModelParameter pOriginal = mGlobalParamterList.FirstOrDefault(p => p.ItemName == prm.ItemName);
                if (pOriginal != null)
                {
                    RelevantParameterList.Add(pOriginal);
                }
                else
                {
                    RelevantParameterList.Add(prm);
                }
            }
            foreach (var tuple in SelectedParametersGridList.Zip(RelevantParameterList, (x, y) => (x, y)))
            {
                IsUpdate = false;
                if (tuple.x.RequiredAsInput)//selected
                {
                    var item = ParameterValuesByNameDic.FirstOrDefault(o => o.ParamName == tuple.y.ItemName);
                    if (item != null)
                    {
                        string str = string.Empty;

                        if(ParameterValuesByNameDic.FirstOrDefault(x => x.ParamName == CURRENT_VAL_PARAMETER) != null)
                        {
                            str = ParameterValuesByNameDic.FirstOrDefault(x => x.ParamName == CURRENT_VAL_PARAMETER).ParamName;
                        }

                        tuple.y.OptionalValuesList = new ObservableList<OptionalValue>();
                        if (string.IsNullOrEmpty(str))
                        {
                            tuple.y.OptionalValuesList.Add(new OptionalValue { Value = CURRENT_VAL_PARAMETER, IsDefault = true });
                        }
                        
                        foreach (string val in item.ParameterValuesByNameDic)
                        {
                            if (!string.IsNullOrEmpty(val))
                            {
                                OptionalValue OptionalValue = new OptionalValue
                                {
                                    Value = val.Replace("*", "")
                                };
                                OptionalValue.IsDefault = val.Contains("*") ? true : false;

                                tuple.y.OptionalValuesList.Add(OptionalValue);
                                IsUpdate = true; 
                            }
                        }
                    }
                }

                GlobalAppModelParameter cur = mGlobalParamterList.FirstOrDefault(p => p.ItemName == tuple.y.ItemName);
                if (cur == null)
                {
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(tuple.y);
                    IsUpdate = true;
                }

                int countDefault = tuple.y.OptionalValuesList.Where(t => t.IsDefault == true).Count();
                if (countDefault > 1)
                {
                    int indx = 1;
                    foreach (var opVal in tuple.y.OptionalValuesList.Where(t => t.IsDefault == true))
                    {
                        if (indx < countDefault)
                        {
                            opVal.IsDefault = false;
                        }
                        indx++;
                    }
                }

                if (IsUpdate)
                {
                    tuple.y.SaveBackup();
                    UpdatedParameters++;                    
                }                    
            }


            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParameters);
        }
        #endregion

        #region EXCEL
        public string ExcelFileName { get; set; }
        public string ExcelSheetName { get; set; }
        public string ExcelWhereCondition { get; set; }

        private DataTable dtCurrentExcelTable;
        public DataTable GetExceSheetlData(bool WithWhere)
        {
            DataTable dt = GetDataFromSheet(WithWhere);
            return dt;
        }

        public DataTable GetExceSheetlData()
        {
            DataTable dt = GetDataFromSheet(false);
            return dt;
        }

        private DataTable GetDataFromSheet(bool WithWhere)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(ExcelFileName))
            {
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(ExcelFileName, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                        Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(s => ExcelSheetName.Equals(s.Name));
                        if (sheet != null)
                        {
                            string relId = sheet.Id;
                            WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relId);
                            if (worksheetPart != null)
                            {
                                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                                if (sheetData != null)
                                {
                                    IEnumerable<Row> rows = sheetData.Descendants<Row>();
                                    if (rows != null && rows.Count() > 0)
                                    {
                                        foreach (Cell cell in rows.ElementAt(0))
                                        {
                                            dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                                        }

                                        foreach (Row row in rows)
                                        {
                                            DataRow tempRow = dt.NewRow();
                                            int i = 0;
                                            int preColIndx = 0;
                                            foreach (Cell cel in row.Descendants<Cell>())
                                            {
                                                int curColIndx = GetColumnIndexFromColumnName(cel.CellReference);
                                                if ((preColIndx + 1) < curColIndx)
                                                {
                                                    i++;
                                                }
                                                tempRow[i] = GetCellValue(spreadSheetDocument, cel);
                                                preColIndx = curColIndx;
                                                i++;
                                            }
                                            dt.Rows.Add(tempRow);
                                        }
                                        dt.Rows.RemoveAt(0);

                                        if (WithWhere && !string.IsNullOrEmpty(ExcelWhereCondition))
                                        {
                                            dt = dt.Select(ExcelWhereCondition).CopyToDataTable();
                                        }
                                        dtCurrentExcelTable = dt;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        switch (ex.Message)
                        {
                            case "Syntax error in FROM clause.":
                                break;
                            case "No value given for one or more required parameters.":
                                if (ShowMessage)
                                    Reporter.ToUser(eUserMsgKeys.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                    System.Windows.MessageBox.Show(ex.Message);
                                break;
                        }
                        return null;
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// This method is used to get the cell value
        /// </summary>
        /// <param name="document"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            string value = string.Empty;
            try
            {
                SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
                value = cell.CellValue.InnerText;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return value;
        }

        /// <summary>
        /// This method is used to reve the integer from the cell refrence
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private string RemoveIntegerFromColumnName(string columnName)
        {
            StringBuilder cName = new StringBuilder();
            try
            {
                if (String.IsNullOrEmpty(columnName))
                {
                    return "";
                }
                columnName = columnName.Replace("#", "");
                foreach (char ch in columnName)
                {
                    int num = 0;
                    if (int.TryParse(Convert.ToString(ch), out num))
                    {
                        break;
                    }
                    else
                    {
                        cName.Append(Convert.ToString(ch));
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return cName.ToString();
        }

        /// <summary>
        /// This method is used to Get Column Index From ColumnName
        /// </summary>
        /// <param name="colName"></param>
        /// <returns></returns>
        private int GetColumnIndexFromColumnName(string colName)
        {
            int res = 0;
            try
            {
                string colAdress = RemoveIntegerFromColumnName(colName);
                int[] digits = new int[colAdress.Length];
                for (int i = 0; i < colAdress.Length; ++i)
                {
                    digits[i] = Convert.ToInt32(colAdress[i]) - 64;
                }
                int mul = 1;
                for (int pos = digits.Length - 1; pos >= 0; --pos)
                {
                    res += digits[pos] * mul;
                    mul *= 26;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return res;
        }

        /// <summary>
        /// This method is used to get the sheets list
        /// </summary>
        /// <param name="validateSheet"></param>
        /// <returns></returns>
        public List<string> GetSheets(bool validateSheet)
        {
            List<string> lst = new List<string>();
            if (!string.IsNullOrEmpty(ExcelFileName))
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(ExcelFileName, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    IEnumerable<WorksheetPart> worksheetPart = workbookPart.WorksheetParts;
                    foreach (var item in workbookPart.Workbook.Descendants<Sheet>())
                    {
                        string sheetName = item.Name;
                        DataTable dt = GetExcelSheetDataBySheetName(sheetName);
                        if (validateSheet)
                        {
                            bool isValid = CheckIsSheetDataValid(dt);
                            if (isValid)
                            {
                                lst.Add(sheetName);
                            } 
                        }
                        else
                        {
                            lst.Add(sheetName);
                        }
                    }
                } 
            }
            return lst;
        }

        /// <summary>
        /// This method is used to check the datatable valid or not for ModelParameter import/export
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private bool CheckIsSheetDataValid(DataTable dt)
        {
            bool isValid = false;
            try
            {
                int indx = 1;
                foreach (DataColumn col in dt.Columns)
                {
                    if ((indx == 1 && col.ColumnName == PARAMETER_NAME) ||
                        (indx == 2 && col.ColumnName == DESCRIPTION))
                    {
                        isValid = true;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                    indx++;
                    if (indx > 2)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return isValid;
        }

        /// <summary>
        /// This method is used to get the sheed data for the sheet
        /// </summary>
        /// <param name="shName"></param>
        /// <returns></returns>
        private DataTable GetExcelSheetDataBySheetName(string shName)
        {
            DataTable dt = new DataTable();
            if (!string.IsNullOrEmpty(ExcelFileName))
            {
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(ExcelFileName, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                        Sheet sheet = workbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == shName).FirstOrDefault();
                        if (sheet != null)
                        {
                            string relId = sheet.Id;
                            WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relId);
                            if (worksheetPart != null)
                            {
                                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                                if (sheetData != null)
                                {
                                    IEnumerable<Row> rows = sheetData.Descendants<Row>();
                                    if (rows != null && rows.Count() > 0)
                                    {
                                        foreach (Cell cell in rows.ElementAt(0))
                                        {
                                            dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        switch (ex.Message)
                        {
                            case "Syntax error in FROM clause.":
                                break;
                            case "No value given for one or more required parameters.":
                                if (ShowMessage)
                                    Reporter.ToUser(eUserMsgKeys.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                {
                                    Reporter.ToUser(eUserMsgKeys.StaticErrorMessage, ex.Message);
                                }
                                break;
                        }
                        return null;
                    }
                }
            }
            return dt;
        }

        private string GetConnectionString()
        {
            string connString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"",ExcelFileName);
            return connString;
        }
        public List<ParameterValues> UpdateParametersOptionalValuesFromCurrentExcelTable()
        {
            if (dtCurrentExcelTable == null)//added as quick fix
            {
                dtCurrentExcelTable = GetExceSheetlData(false);
            }
            return GetParametersAndValuesDictionary(dtCurrentExcelTable);
        }
        private List<ParameterValues> GetParametersAndValuesDictionary(DataTable dt)
        {
            List<ParameterValues> ParameterValues = new List<ParameterValues>();
            //First column is parameter name and other columns are values
            foreach (DataRow row in dt.Rows)
            {
                ParameterValues parmVal = new ParameterValues();
                if (!string.IsNullOrEmpty(row[0].ToString()))
                {
                    string key = row[0].ToString();
                    List<string> ValueList = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (row[col] != null && row[0] != row[col] && !col.ColumnName.StartsWith("Desc"))
                        {
                            string value = Convert.ToString(row[col]);
                            ValueList.Add(value);
                        }
                        else if (row[0] == row[col])
                        {
                            parmVal.ParamName = Convert.ToString(row[col]);
                        }
                        else if(col.ColumnName.StartsWith("Desc"))
                        {
                            parmVal.Description = Convert.ToString(row[col]);
                        }
                    }
                    ValueList = ValueList.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                    parmVal.ParameterValuesByNameDic = ValueList;
                    ParameterValues.Add(parmVal);
                }
            }
            return ParameterValues;
        }

        /// <summary>
        /// This method is used to export the parameter values to export
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ExportParametersToExcelFile(List<AppParameters> parameters, string fileName)
        {
            string filePath = string.Empty;
            filePath = ExportParametersToFile(parameters, fileName, string.Empty, filePath);
            return filePath;
        }

        /// <summary>
        /// This method is used to export the parameter values to export
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string ExportParametersToExcelFile(List<AppParameters> parameters, string fileName, string fPath)
        {
            string filePath = string.Empty;
            filePath = ExportParametersToFile(parameters, fileName, fPath, filePath);
            return filePath;
        }

        /// <summary>
        /// This is sub method used handle the export process
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="fileName"></param>
        /// <param name="fPath"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private string ExportParametersToFile(List<AppParameters> parameters, string fileName, string fPath, string filePath)
        {
            try
            {
                string tableName = ParameterType == eParameterType.Global ? "GlobalModelParameters" : "ModelParameters";

                DataTable dtTemplate = ExportParametertoDataTable(parameters, tableName, true);

                if (!string.IsNullOrEmpty(fPath))
                {
                    filePath = fPath;
                }
                else
                {
                    filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName + ".xlsx");
                }

                bool isExportSuccess = ExportToExcel(dtTemplate, filePath, dtTemplate.TableName);
                if (isExportSuccess && ShowMessage)
                {
                    Reporter.ToUser(eUserMsgKeys.ExportDetails, "Excel File");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR,"Error while exporting Paramters to File", ex);
            }

            return filePath;
        }

        private DataTable ExportParametertoDataTable(List<AppParameters> parameters,string tableName, bool isExcelExport)
        {            
            int colCount = isExcelExport ? 100 : 0;
            foreach (var paramVal in parameters)
            {
                if (paramVal.OptionalValuesString.Contains(CURRENT_VAL_PARAMETER))
                {
                    colCount = (paramVal.OptionalValuesList.Count - 1) > colCount ? (paramVal.OptionalValuesList.Count - 1) : colCount;
                }
                else
                {
                    colCount = (paramVal.OptionalValuesList.Count) > colCount ? (paramVal.OptionalValuesList.Count) : colCount;
                }
            }

            DataTable dtTemplate = new DataTable(tableName);
            dtTemplate.Columns.Add(PARAMETER_NAME, typeof(string));
            dtTemplate.Columns.Add(DESCRIPTION, typeof(string));
                        
            for (int index = 1; index <= colCount; index++)
            {
                dtTemplate.Columns.Add(string.Format("Value {0}", index), typeof(string));
            }

            foreach (AppParameters prm in parameters)
            {
                DataRow dr = dtTemplate.NewRow();
                dr[0] = Convert.ToString(prm.ItemName);
                dr[1] = Convert.ToString(prm.Description);

                if (prm.OptionalValuesList.Count > 0)
                {
                    int index = 2;
                    foreach (var item in prm.OptionalValuesList)
                    {
                        if (item.Value == null)
                        {
                            item.Value = "";
                        }
                        if (!item.Value.StartsWith(CURRENT_VAL_PARAMETER))
                        {
                            if (isExcelExport)
                            {
                                dr[index] = Convert.ToString(item.Value) + (item.IsDefault ? "*" : ""); 
                            }
                            else
                            {
                                dr[index] = Convert.ToString(item.Value);
                            }
                            index++;
                        }                        
                    }
                    if (!isExcelExport)
                    {
                        while (index < colCount + 2)
                        {
                            dr[index] = Convert.ToString(prm.OptionalValuesList.Where(x => x.IsDefault == true && x.Value != CURRENT_VAL_PARAMETER).Select(x => x.Value).FirstOrDefault());
                            index++;
                        } 
                    }
                }

                dtTemplate.Rows.Add(dr);
            }
            return dtTemplate;            
        }

        public bool ExportTemplateExcelFileForImportOptionalValues(List<AppModelParameter> Parameters,string PathToExport)
        {
            DataTable dtTemplate = new DataTable(ParameterType.ToString());
            dtTemplate.Columns.Add("Parameter Name", typeof(string));
            dtTemplate.Columns.Add("Value 1", typeof(string));
            dtTemplate.Columns.Add("Value 2", typeof(string));
            foreach (AppModelParameter prm in Parameters)
            {
                dtTemplate.Rows.Add(prm.ItemName.ToString(), string.Empty);
            }
            bool IsExportSuccess = ExportToExcel(dtTemplate, PathToExport, dtTemplate.TableName);
            if (IsExportSuccess && ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ExportDetails , "Excel File");
            return IsExportSuccess;
        }

        private bool ExportToExcel(DataTable table, string sFilePath, string sSheetName = "")
        {
            try
            {
                SpreadsheetDocument workbook;

                if (sSheetName == "")
                    sSheetName = table.TableName;

                if (File.Exists(sFilePath))
                {
                    File.Delete(sFilePath);
                }
                                
                workbook = SpreadsheetDocument.Create(sFilePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                WorkbookPart workbookPart = workbook.AddWorkbookPart();
                workbook.WorkbookPart.Workbook = new Workbook();
                workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                                                
                if (workbook.WorkbookPart.WorkbookStylesPart == null)
                {
                    WorkbookStylesPart stylePart = workbook.WorkbookPart.AddNewPart<WorkbookStylesPart>();
                    stylePart.Stylesheet = GenerateStylesheet();
                    stylePart.Stylesheet.Save(); 
                }

                uint sheetId = 1;
                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);
                Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                Sheet oSheet = sheets.Elements<Sheet>().Where(s => s.Name == sSheetName).FirstOrDefault();
                if (oSheet != null)
                {
                    sSheetName += "_" + sheets.Elements<Sheet>().Count();
                }

                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }

                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sSheetName };                
                sheets.Append(sheet);
                
                Row headerRow = new Row();
                List<string> columns = new List<string>();
                int indx = 0;
                foreach (DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);
                    headerRow.AppendChild(GetCell(column.ColumnName, CellValues.String, 2));
                    indx++;
                }
                sheetData.AppendChild(headerRow);
                
                foreach (DataRow dsrow in table.Rows)
                {
                    Row newRow = new Row();          
                    foreach (String col in columns)
                    {
                        newRow.AppendChild(GetCell(Convert.ToString(dsrow[col]), CellValues.String, 1));
                    }
                    sheetData.AppendChild(newRow);
                }
                workbook.Close();
                return true;
            }
            catch(Exception ex)
            {
                if (ShowMessage)
                    Reporter.ToUser(eUserMsgKeys.ExportFailed, "Excel File", ex.Message.ToString());
            }
            return false;
        }

        /// <summary>
        /// This method is used to Get the new cell
        /// </summary>
        /// <param name="value"></param>
        /// <param name="dataType"></param>
        /// <param name="styleIndex"></param>
        /// <returns></returns>
        private Cell GetCell(string value, CellValues dataType, uint styleIndex = 0)
        {
            Cell cl = new Cell()
            {
                DataType = new EnumValue<CellValues>(dataType),
                StyleIndex = styleIndex
            };

            long lng;
            string val = value;
            if (long.TryParse(value, out lng))
            {
                cl.DataType = CellValues.Number;
            }
            cl.CellValue = new CellValue(val);
            
            return cl;
        }

        /// <summary>
        /// This method is used to add the stylesheets to the workbook
        /// </summary>
        /// <returns></returns>
        private Stylesheet GenerateStylesheet()
        {
            Stylesheet styleSheet = null;

            Fonts fonts = new Fonts(
                new Font( // Index 0 - default
                    new FontSize { Val = 11 }

                ),
                new Font( // Index 1 - header
                    new FontSize { Val = 11 },
                    new Bold()

                ));

            Fills fills = new Fills(
                    new Fill(new PatternFill { PatternType = PatternValues.None }), // Index 0 - default
                    new Fill(new PatternFill { PatternType = PatternValues.None })
                );

            Borders borders = new Borders(
                    new Border(), // index 0 default
                    new Border()
                );

            CellFormats cellFormats = new CellFormats(
                    new CellFormat(), // default
                    new CellFormat { FontId = 0 }, // body
                    new CellFormat { FontId = 1 } // header
                );

            styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);

            return styleSheet;
        }

        /// <summary>
        /// This method is used to add the parameter to the list
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="prms"></param>
        public void AddNewParameterToList(List<AppParameters> parameters, AppModelParameter prms)
        {
            if (prms != null)
            {
                AppParameters par = new AppParameters();
                par.ItemName = prms.ItemName;
                par.OptionalValuesList = prms.OptionalValuesList;
                par.OptionalValuesString = prms.OptionalValuesString;
                par.Description = prms.Description;
                parameters.Add(par);
            }
        }

        /// <summary>
        /// This method is used to get the excel sheet data
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="isFirstRowHeader"></param>
        /// <returns></returns>
        public DataSet GetExcelAllSheetData(string sheetName, bool isFirstRowHeader)
        {
            return GetExcelAllSheetsData(sheetName, isFirstRowHeader);
        }

        /// <summary>
        /// This method is used to get the excel sheet data
        /// </summary>
        /// <returns></returns>
        public DataSet GetExcelAllSheetData(string sheetName, bool isFirstRowHeader, bool exactValues, bool pivoteTable)
        {
            DataSet ds = GetExcelAllSheetsData(sheetName, isFirstRowHeader);

            DataSet dsExact = new DataSet();
            if(exactValues)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    List<string> lstColumn = new List<string>();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        bool colEmpty = true;
                        foreach (DataRow dr in dt.Rows)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dr[dc.ColumnName])))
                            {
                                colEmpty = false;
                            }
                        }
                        if(colEmpty)
                        {
                            lstColumn.Add(dc.ColumnName);
                        }
                    }

                    foreach (string colName in lstColumn)
                    {
                        dt.Columns.Remove(colName);
                    }

                    DataTable dtNew = dt.Copy();
                    dtNew.TableName = dt.TableName;
                    dsExact.Tables.Add(dtNew);
                }
            }

            DataSet dsPivote = new DataSet();
            if (pivoteTable)
            {
                foreach (DataTable dt in ds.Tables)
                {
                    DataTable dtNew = PivotTable(dt, isFirstRowHeader);
                    //Removing Paramter Name Column
                    if (dtNew.Columns.Contains(PARAMETER_NAME))
                    {                                             
                        dtNew.Columns.Remove(PARAMETER_NAME);
                    }                        
                    dsPivote.Tables.Add(dtNew);
                }
                dsExact = dsPivote;
            }

            return dsExact;
        }

        /// <summary>
        /// This method pivot table i.e. convert rows to column
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private DataTable PivotTable(DataTable dt, bool isFirstRowHeader)
        {
            DataTable dtNew = new DataTable();
            dtNew.TableName = dt.TableName;
            //adding columns    
            int cols = 0;
            for (; cols < dt.Rows.Count; cols++)
            {
                string colName = Convert.ToString(dt.Rows[cols].ItemArray[0]).Replace("[", "_").Replace("]", "").Replace("{","").Replace("}","");
                dtNew.Columns.Add(colName);
            }

            //Adding Row Data
            cols = 2;
            for (; cols < dt.Columns.Count; cols++)
            {
                DataRow row = dtNew.NewRow();
                bool emptyRow = true;
                int indx = isFirstRowHeader ? 0 : 1;
                for (; indx < dt.Rows.Count; indx++)
                {
                    if (Convert.ToString(dt.Rows[indx][cols]) != "")
                    {
                        emptyRow = false;
                    }                        
                    row[indx] = Convert.ToString(dt.Rows[indx][cols]).Replace("*", "");
                }

                if (emptyRow == false)
                {
                    dtNew.Rows.Add(row);
                }                    
            }

            UpdateDefauktValuesInBlankCell(dtNew, dt);
            return dtNew;
        }

        /// <summary>
        /// This method is used to update the default values in the blank cells
        /// </summary>
        /// <param name="dtNew"></param>
        /// <param name="dt"></param>
        private void UpdateDefauktValuesInBlankCell(DataTable dtNew, DataTable dt)
        {
            try
            {
                for (int newCols = 1; newCols < dtNew.Columns.Count; newCols++)
                {
                    string defaultValue = string.Empty;
                    for (int olCols = 1; olCols < dt.Columns.Count; olCols++)
                    {
                        if(Convert.ToString(dt.Rows[newCols][olCols]).EndsWith("*"))
                        {
                            defaultValue = Convert.ToString(dt.Rows[newCols][olCols]).Replace("*", "");
                            break;
                        }
                    }

                    if(string.IsNullOrEmpty(defaultValue))
                    {
                        defaultValue = Convert.ToString(dt.Rows[newCols][2]);
                    }

                    for (int newRows = 1; newRows < dtNew.Rows.Count; newRows++)
                    {
                        if (string.IsNullOrEmpty(Convert.ToString(dtNew.Rows[newRows][newCols])))
                        {
                            dtNew.Rows[newRows][newCols] = defaultValue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error while exporting Paramters to File", ex);
            }
        }

        /// <summary>
        /// This method is used to get the excel sheet data
        /// </summary>
        /// <param name="sheetName"></param>
        /// <param name="isFirstRowHeader"></param>
        /// <returns></returns>
        private DataSet GetExcelAllSheetsData(string sheetName, bool isFirstRowHeader)
        {
            DataSet ds = new DataSet();
            if (!string.IsNullOrEmpty(ExcelFileName))
            {
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(ExcelFileName, false))
                {
                    try
                    {
                        WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                        IEnumerable<Sheet> sheets = workbookPart.Workbook.Descendants<Sheet>();
                        if (!string.IsNullOrEmpty(sheetName) && sheetName != "-- All --")
                        {
                            sheets = workbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName);
                        }
                        foreach (Sheet sheet in sheets)
                        {
                            if (sheet != null)
                            {
                                DataTable dt = new DataTable(sheet.Name);
                                string relId = sheet.Id;
                                WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relId);
                                if (worksheetPart != null)
                                {
                                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                                    if (sheetData != null)
                                    {
                                        IEnumerable<Row> rows = sheetData.Descendants<Row>();
                                        if (rows != null && rows.Count() > 0)
                                        {
                                            int i = 1;
                                            foreach (Cell cell in rows.ElementAt(0))
                                            {
                                                if (isFirstRowHeader)
                                                {
                                                    dt.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                                                }
                                                else
                                                {
                                                    dt.Columns.Add(string.Format("Field_{0}", i));
                                                }
                                                i++;
                                            }

                                            foreach (Row row in rows)
                                            {
                                                DataRow tempRow = dt.NewRow();
                                                i = 0;
                                                int preColIndx = 0;
                                                foreach (Cell cel in row.Descendants<Cell>())
                                                {
                                                    if (dt.Columns.Count <= i)
                                                    {
                                                        int cnt = dt.Columns.Count + 1;
                                                        dt.Columns.Add(string.Format("Field_{0}", cnt));
                                                    }

                                                    int curColIndx = GetColumnIndexFromColumnName(cel.CellReference);
                                                    if ((preColIndx + 1) < curColIndx)
                                                    {
                                                        i++;
                                                    }
                                                    tempRow[i] = GetCellValue(spreadSheetDocument, cel);
                                                    preColIndx = curColIndx;
                                                    i++;
                                                }
                                                dt.Rows.Add(tempRow);
                                            }
                                            if (isFirstRowHeader)
                                            {
                                                dt.Rows.RemoveAt(0);
                                            }
                                            ds.Tables.Add(dt);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        switch (ex.Message)
                        {
                            case "Syntax error in FROM clause.":
                                break;
                            case "No value given for one or more required parameters.":
                                if (ShowMessage)
                                    Reporter.ToUser(eUserMsgKeys.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                    System.Windows.MessageBox.Show(ex.Message);
                                break;
                        }
                        return null;
                    }
                }
            }
            return ds;
        }

        /// <summary>
        /// This method will export the parameters to DataSource
        /// </summary>
        /// <param name="parameters"></param>
        public void ExportSelectedParametersToDataSouce(List<AppParameters> parameters, AccessDataSource mDSDetails, string tableName)
        {
            try
            {
                if (mDSDetails != null)
                {                    
                    List<string> colList = mDSDetails.GetColumnList(tableName);
                    List<string> defColList = GetDefaultColumnNameListForTableCreation();                    
                    foreach (AppParameters appParam in parameters)
                    {
                        string sColName = appParam.ItemName.Replace("[", "_").Replace("]", "").Replace("{", "").Replace("}", "");
                        if (!colList.Contains(sColName))
                        {                            
                            mDSDetails.AddColumn(tableName, sColName, "Text");
                        }
                    }
                    if (!colList.Contains("GINGER_USED"))
                    { 
                        mDSDetails.AddColumn(tableName, "GINGER_USED", "Text");
                    }

                    DataTable dtTemplate = ExportParametertoDataTable(parameters, tableName, false);                    
                    dtTemplate = PivotTable(dtTemplate, true);
                    //Removing Paramter Name Column                    
                    if (dtTemplate.Columns.Contains(PARAMETER_NAME))
                    {                                                
                        dtTemplate.Columns.Remove(PARAMETER_NAME);
                    }                        

                    foreach (string colName in defColList)
                    {
                        if (!dtTemplate.Columns.Contains(colName))
                        { 
                            dtTemplate.Columns.Add(colName);
                        }
                    }                    
                    foreach (DataRow dr in dtTemplate.Rows)
                    {
                        dr["GINGER_USED"] = "False";
                    }                    
                    mDSDetails.SaveTable(dtTemplate);                    
                    Reporter.ToUser(eUserMsgKeys.ExportDetails,"Data Source");
                }                
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR,"Failed to Export to Data Source",ex);
                Reporter.ToUser(eUserMsgKeys.ExportFailed, "Data Source");
            }
        }

        /// <summary>
        /// This method is used to get the default columnList for exporting the parameters to datasource
        /// </summary>
        /// <returns></returns>
        private List<string> GetDefaultColumnNameListForTableCreation()
        {
            List<string> defColList = new List<string>();
            try
            {
                defColList.Add("GINGER_ID");
                defColList.Add("GINGER_USED");
                defColList.Add("GINGER_LAST_UPDATED_BY");
                defColList.Add("GINGER_LAST_UPDATE_DATETIME");
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
            return defColList;
        }

        /// <summary>
        /// This method is used to insert the data parameter data in table
        /// </summary>
        /// <param name="mDSDetails"></param>
        /// <param name="itemName"></param>
        /// <param name="description"></param>
        /// <param name="optionalValuesString"></param>
        private void InsertParameterData(AccessDataSource mDSDetails, string tableName, string itemName, string description, string optionalValuesString)
        {
            try
            {
                string query = GetInsertQueryForParameter(tableName, itemName, description, optionalValuesString);
                mDSDetails.RunQuery(query);
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.StackTrace);
            }
        }

        /// <summary>
        /// This method is used to get the columnList for exporting the parameters to datasource
        /// </summary>
        /// <returns></returns>
        private string GetInsertQueryForParameter(string tableName, string itemName, string description, string optionalValuesString)
        {
            string query = string.Format("INSERT INTO {0} (GINGER_USED, GINGER_LAST_UPDATED_BY, GINGER_LAST_UPDATE_DATETIME, ItemName, Description, OptionalValuesString) "
                                       + " VALUES ('True', '{1}', '{2}', '{3}', '{4}', '{5}')", tableName, System.Environment.UserName, DateTime.Now.ToString(), itemName, description, optionalValuesString);
            return query;
        }

        #endregion

        #region DB
        Database db;
        List<object> SQLResult = new List<object>();
        DataTable dtDB = new DataTable();
        public void SetDBDetails(string dbType, string host, string user, string password)
        {
            db = new Database();
            db.TNS = host;
            db.User = user;
            db.Pass = password;
            db.DBType = (Database.eDBTypes)Enum.Parse(typeof(Database.eDBTypes), dbType);
        }
        public List<string> GetDBTypeList()
        {
            return Database.DbTypes;
        }
        public bool Connect()
        {
            if (db.Connect())
            {
                return true;
            }
            return false;
        }
        public void ExecuteFreeSQL(string command)
        {
             SQLResult = db.FreeSQL(command);
        }
        public List<ParameterValues> UpdateParametersOptionalValuesFromDB()
        {
            Dictionary<string,List<string>> ParamAndValues = new Dictionary<string, List<string>>();
            List<string> ParamNameList = (List<string>)SQLResult.ElementAt(0);
            List<List<string>> Record = (List<List<string>>)SQLResult.ElementAt(1);
            string[][] ParamValues = new string[ParamNameList.Count][];//create matrix ParamName -> ParamValues(List)
            for (int i = 0; i < ParamNameList.Count; i++)
                ParamValues[i] = new string[Record.Count];
            int rowValue = 0;
            for(int i = 0; i < Record.Count; i++)
            {
                string[] current = (Record.ElementAt<List<string>>(i)).ToArray<string>();
                for (int j = 0; j < current.Length; j++)
                {
                    if (current[j] != null)
                        ParamValues[rowValue++][i] = current[j];
                    else
                        ParamValues[rowValue++][i] = string.Empty;
                }
                rowValue=0;
            }

            List<ParameterValues>  ParameterValuesByNameDic = new List<ParameterValues>();
            dtDB = new DataTable();
            dtDB.Columns.Add(PARAMETER_NAME, typeof(string));
            for (int i =0; i < ParamNameList.Count; i++)
            {
                List<string> values = ParamValues[i].OfType<string>().ToList<string>();
                values = values.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                var item = ParameterValuesByNameDic.FirstOrDefault(o => o.ParamName == ParamNameList.ElementAt(i));
                if (item == null)
                {
                    ParameterValuesByNameDic.Add(new ParameterValues() { ParamName = ParamNameList.ElementAt(i), ParameterValuesByNameDic = values });
                    DataRow row = dtDB.NewRow();
                    row["Parameter Name"] = ParamNameList[i].ToString();
                    dtDB.Rows.Add(row);
                }      
                else//different columns with the same alias name
                {
                    List<string> extraValues = ParameterValuesByNameDic.Select(p => p.ParamName).ToList();
                    if (extraValues != null)
                    {
                        extraValues.AddRange(values);
                        values = extraValues.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                        ParameterValuesByNameDic.RemoveAt(i);
                        ParameterValuesByNameDic.Add(new ParameterValues() { ParamName = ParamNameList.ElementAt(i), ParameterValuesByNameDic = values });
                    }
                }  
            }
            return ParameterValuesByNameDic;
        }
        #endregion

    }
}
