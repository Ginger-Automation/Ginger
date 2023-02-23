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

using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Amdocs.Ginger.Common;
using System.Data;
using Amdocs.Ginger.Common.APIModelLib;
using GingerCore.Environments;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Amdocs.Ginger.Common.Repository.ApplicationModelLib;
using amdocs.ginger.GingerCoreNET;
using System.Text;
using GingerCore.DataSource;
using GingerAutoPilot.APIModelLib;

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
        const string PARAMETER_NAME = "Parameter_Name";
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
            ImportParametersOptionalValues impOpVals = new ImportParametersOptionalValues();
            foreach (TemplateFile TF in ((ApplicationAPIModel)AAM).OptionalValuesTemplates)
            {
                CreateParser(TF.FilePath);
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    impOpVals.GetXMLAllOptionalValuesFromExamplesFile(TF, OptionalValuesPerParameterDict);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    impOpVals.GetJSONAllOptionalValuesFromExamplesFile(TF, OptionalValuesPerParameterDict);
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
            ImportParametersOptionalValues impOpVals = new ImportParametersOptionalValues();
            if (currentParser != null)
            {
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    impOpVals.PopulateXMLOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    impOpVals.PopulateJSONOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict);
                }

                if (ShowMessage)
                    Reporter.ToUser(eUserMsgKey.ParameterOptionalValues, OptionalValuesPerParameterDict.Count());
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
            ImportParametersOptionalValues impOpVals = new ImportParametersOptionalValues();
            if (currentParser != null)
            {
                if (currentParser.GetType() == typeof(XMLTemplateParser))
                {
                    impOpVals.PopulateXMLOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict, SelectedParametersGridList);
                }
                else if (currentParser.GetType() == typeof(JSONTemplateParser))
                {
                    impOpVals.PopulateJSONOptionalValuesForAPIParameters(AAM, OptionalValuesPerParameterDict, SelectedParametersGridList);
                }

                if (ShowMessage)
                    Reporter.ToUser(eUserMsgKey.ParameterOptionalValues, OptionalValuesPerParameterDict.Count());
            }
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
            foreach (AppModelParameter prm in SelectedParametersGridList)
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
                        if (indx < countDefault)
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
                Reporter.ToUser(eUserMsgKey.ParameterOptionalValues, UpdatedParameters);
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
                    var item = ParameterValuesByNameDic.Find(o => o.ParamName == tuple.y.ItemName);
                    if (item != null)
                    {
                        string str = ParameterValuesByNameDic.Where(x => x.ParamName == CURRENT_VAL_PARAMETER).Select(x => x.ParamName).FirstOrDefault();
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
                Reporter.ToUser(eUserMsgKey.ParameterOptionalValues, UpdatedParameters);
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
                                    Reporter.ToUser(eUserMsgKey.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
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
                if (cell.CellValue == null || cell.CellValue.InnerText == null)
                {
                    return String.Empty;
                }
                value = cell.CellValue.InnerText;

                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    value = stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
                                    Reporter.ToUser(eUserMsgKey.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                {
                                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
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
            string connString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"", ExcelFileName);
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
                        else if (col.ColumnName.StartsWith("Desc"))
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
                    Reporter.ToUser(eUserMsgKey.ExportDetails, "Excel File");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error while exporting Parameters to File", ex);
            }

            return filePath;
        }

        private DataTable ExportParametertoDataTable(List<AppParameters> parameters, string tableName, bool isExcelExport)
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
                dtTemplate.Columns.Add(string.Format("Value_{0}", index), typeof(string));
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

        public bool ExportTemplateExcelFileForImportOptionalValues(List<AppModelParameter> Parameters, string PathToExport)
        {
            DataTable dtTemplate = new DataTable(ParameterType.ToString());
            dtTemplate.Columns.Add(PARAMETER_NAME, typeof(string));
            dtTemplate.Columns.Add("Value_1", typeof(string));
            dtTemplate.Columns.Add("Value_2", typeof(string));
            foreach (AppModelParameter prm in Parameters)
            {
                dtTemplate.Rows.Add(prm.ItemName.ToString(), string.Empty);
            }
            bool IsExportSuccess = ExportToExcel(dtTemplate, PathToExport, dtTemplate.TableName);
            if (IsExportSuccess && ShowMessage)
                Reporter.ToUser(eUserMsgKey.ExportDetails, "Excel File");
            return IsExportSuccess;
        }

        private bool ExportToExcel(DataTable table, string sFilePath, string sSheetName)
        {
            return GingerCoreNET.GeneralLib.General.ExportToExcel(table, sFilePath, sSheetName);
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
        public DataSet GetExcelAllSheetData(string sheetName, bool isFirstRowHeader, bool isImportEmptyColumns, bool pivoteTable)
        {
            DataSet ds = GetExcelAllSheetsData(sheetName, isFirstRowHeader);

            DataSet dsExact = new DataSet();
            if (!isImportEmptyColumns)
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
                        if (colEmpty)
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
            else
            {
                dsExact = ds;
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
                string colName = Convert.ToString(dt.Rows[cols].ItemArray[0]).Replace("[", "_").Replace("]", "").Replace("{", "").Replace("}", "");
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
                        if (Convert.ToString(dt.Rows[newCols][olCols]).EndsWith("*"))
                        {
                            defaultValue = Convert.ToString(dt.Rows[newCols][olCols]).Replace("*", "");
                            break;
                        }
                    }

                    if (string.IsNullOrEmpty(defaultValue))
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
                Reporter.ToLog(eLogLevel.ERROR, "Error while exporting Parameters to File", ex);
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
                                    Reporter.ToUser(eUserMsgKey.ExcelBadWhereClause);
                                break;
                            default:
                                if (ShowMessage)
                                    Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
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
        public void ExportSelectedParametersToDataSouce(List<AppParameters> parameters, DataSourceBase mDSDetails, string tableName)
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
                    Reporter.ToUser(eUserMsgKey.ExportDetails, "Data Source");
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Export to Data Source", ex);
                Reporter.ToUser(eUserMsgKey.ExportFailed, "Data Source");
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
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
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
        public void SetDBDetails(string dbType, string host, string user, string password, string connectionString = null)
        {
            db = new Database();
            db.TNS = host;
            db.User = user;
            db.Pass = password;
            if (!string.IsNullOrEmpty(connectionString))
                db.ConnectionString = connectionString;
            db.DBType = (Database.eDBTypes)Enum.Parse(typeof(Database.eDBTypes), dbType);

            DatabaseOperations databaseOperations = new DatabaseOperations(db);
            db.DatabaseOperations = databaseOperations;
        }
        public List<string> GetDBTypeList()
        {
            return Database.DbTypes;
        }
        public bool Connect()
        {
            if (db.DatabaseOperations.Connect())
            {
                return true;
            }
            return false;
        }
        public void ExecuteFreeSQL(string command)
        {
            SQLResult = db.DatabaseOperations.FreeSQL(command);
        }
        public List<ParameterValues> UpdateParametersOptionalValuesFromDB()
        {
            Dictionary<string, List<string>> ParamAndValues = new Dictionary<string, List<string>>();
            List<string> ParamNameList = (List<string>)SQLResult.ElementAt(0);
            List<List<string>> Record = (List<List<string>>)SQLResult.ElementAt(1);
            string[][] ParamValues = new string[ParamNameList.Count][];//create matrix ParamName -> ParamValues(List)
            for (int i = 0; i < ParamNameList.Count; i++)
                ParamValues[i] = new string[Record.Count];
            int rowValue = 0;
            for (int i = 0; i < Record.Count; i++)
            {
                string[] current = (Record.ElementAt<List<string>>(i)).ToArray<string>();
                for (int j = 0; j < current.Length; j++)
                {
                    if (current[j] != null)
                        ParamValues[rowValue++][i] = current[j];
                    else
                        ParamValues[rowValue++][i] = string.Empty;
                }
                rowValue = 0;
            }

            List<ParameterValues> ParameterValuesByNameDic = new List<ParameterValues>();
            dtDB = new DataTable();
            dtDB.Columns.Add(PARAMETER_NAME, typeof(string));
            for (int i = 0; i < ParamNameList.Count; i++)
            {
                List<string> values = ParamValues[i].OfType<string>().ToList<string>();
                values = values.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();

                var item = ParameterValuesByNameDic.FirstOrDefault(o => o.ParamName == ParamNameList.ElementAt(i));
                if (item == null)
                {
                    ParameterValuesByNameDic.Add(new ParameterValues() { ParamName = ParamNameList.ElementAt(i), ParameterValuesByNameDic = values });
                    DataRow row = dtDB.NewRow();
                    row[PARAMETER_NAME] = ParamNameList[i].ToString();
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
