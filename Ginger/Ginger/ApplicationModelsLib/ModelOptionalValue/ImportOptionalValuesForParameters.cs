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
               
                    OptionalValuesPerParameterDict.Add(attributetuple, new List<string> { attributeValue });
                

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
                        VAXBXPath = tuple.y.Path.Replace(result, "//*[name()='vaxb:VAXB']/vaxb:");

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
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParametersCounter);
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
                catch
                {

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
        public void PopulateExcelDBOptionalValuesForAPIParametersExcelDB(ApplicationModelBase AAM, List<AppModelParameter> SelectedParametersGridList, Dictionary<string, List<string>> ParameterValuesByNameDic)
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
            }
            foreach (var tuple in SelectedParametersGridList.Zip(RelevantParameterList, (x, y) => (x, y)))
            {
                IsUpdate = false;
                if (tuple.x.RequiredAsInput)//selected
                {
                    if (ParameterValuesByNameDic.ContainsKey(tuple.y.ItemName))
                    {
                        string[] paramExistingvalues = tuple.y.OptionalValuesString.Split(','); ;
                        OptionalValue defaultValue = tuple.y.OptionalValuesList.FirstOrDefault(x => x.IsDefault);
                        foreach (string val in ParameterValuesByNameDic[tuple.y.ItemName])
                        {
                            bool ValueAlreadyExists = paramExistingvalues.Any(x => x == val);
                            if ((paramExistingvalues != null && !ValueAlreadyExists) && (defaultValue == null || defaultValue.Value != val))
                            {
                                OptionalValue OptionalValue = new OptionalValue() { Value = val };
                                if (tuple.y.OptionalValuesList.Count == 0)
                                    OptionalValue.IsDefault = true;
                                tuple.y.OptionalValuesList.Add(OptionalValue);
                                IsUpdate = true;
                            }
                        }
                    }
                }
                if (IsUpdate)
                    UpdatedParameters++;
            }
            if (ShowMessage)
                Reporter.ToUser(eUserMsgKeys.ParameterOptionalValues, UpdatedParameters);
        }
 
        public void PopulateExcelDBOptionalValuesForAPIParametersExcelDB(ObservableList<GlobalAppModelParameter> mGlobalParamterList, List<GlobalAppModelParameter> SelectedParametersGridList, Dictionary<string, List<string>> ParameterValuesByNameDic)
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
            }
            foreach (var tuple in SelectedParametersGridList.Zip(RelevantParameterList, (x, y) => (x, y)))
            {
                IsUpdate = false;
                if (tuple.x.RequiredAsInput)//selected
                {
                    if (ParameterValuesByNameDic.ContainsKey(tuple.y.ItemName))
                    {
                        string[] paramExistingvalues = tuple.y.OptionalValuesString.Split(','); ;
                        OptionalValue defaultValue = tuple.y.OptionalValuesList.FirstOrDefault(x => x.IsDefault);
                        foreach (string val in ParameterValuesByNameDic[tuple.y.ItemName])
                        {
                            bool ValueAlreadyExists = paramExistingvalues.Any(x => x == val);
                            if ((paramExistingvalues != null && !ValueAlreadyExists) && (defaultValue == null || defaultValue.Value != val))
                            {
                                OptionalValue OptionalValue = new OptionalValue() { Value = val };
                                if (tuple.y.OptionalValuesList.Count == 0)
                                    OptionalValue.IsDefault = true;
                                tuple.y.OptionalValuesList.Add(OptionalValue);
                                IsUpdate = true;
                            }
                        }
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
        public DataTable GetExceSheetlData(bool WithWhere = false)
        {
            //TODO: check what is required on the machine and maybe support for other version
            string ConnString = GetConnectionString();
            string sSheetName = "";

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();

                    OleDbCommand Cmd = new OleDbCommand();
                    Cmd.Connection = Conn;
                    if (ExcelSheetName == null)
                        return new DataTable();
                    if (ExcelSheetName.Trim().IndexOf("$") == ExcelSheetName.Trim().Length - 1)
                        sSheetName = ExcelSheetName;
                    else
                        sSheetName = ExcelSheetName + "$";
                    string sql = "Select * from [" + sSheetName + "]";
                    if (WithWhere)
                    {
                        sql = sql + " WHERE " + ExcelWhereCondition;
                    }
                    Cmd.CommandText = sql;
                    DataTable dt = new DataTable();

                    OleDbDataAdapter da = new OleDbDataAdapter();
                    da.SelectCommand = Cmd;

                    da.Fill(dt);
                    dtCurrentExcelTable = dt;
                    return dt;
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
        public List<string> GetSheets()
        {
            string ConnString = GetConnectionString();

            using (OleDbConnection Conn = new OleDbConnection(ConnString))
            {
                try
                {
                    Conn.Open();
                    DataTable c = Conn.GetSchema("Tables");
                    // remove the last $ sign = not user friendly
                    List<string> returnList = c.AsEnumerable().Select(r => r.Field<string>("TABLE_NAME").Substring(0, r.Field<string>("TABLE_NAME").Length - 1))
                               .ToList();
                    return returnList;

                }
                catch
                {
                    return new List<string>();
                }
            }
        }
        private string GetConnectionString()
        {
            string connString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;\"",ExcelFileName);
            return connString;
        }
        public Dictionary<string, List<string>> UpdateParametersOptionalValuesFromCurrentExcelTable()
        {
            if (dtCurrentExcelTable == null)//added as quick fix
                GetExceSheetlData(false);
            return GetParametersAndValuesDictionary(dtCurrentExcelTable);
        }
        private Dictionary<string, List<string>> GetParametersAndValuesDictionary(DataTable dt)
        {
            Dictionary<string, List<string>> ParameterValuesByNameDic = new Dictionary<string, List<string>>();
            //First column is parameter name and other columns are values
            foreach (DataRow row in dt.Rows)
            {
                if (!string.IsNullOrEmpty(row[0].ToString()))
                {
                    string key = row[0].ToString();
                    List<string> ValueList = new List<string>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        if (row[col] != null && row[0] != row[col])
                        {
                            string value = row[col].ToString();
                            ValueList.Add(value);
                        }
                    }
                    ValueList = ValueList.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                    if (ParameterValuesByNameDic.ContainsKey(key))
                    {
                        List<string> list = ParameterValuesByNameDic[key];
                        list = ValueList.Union(list).ToList();
                    }
                    else
                    {
                        ParameterValuesByNameDic.Add(key, ValueList);
                    }
                }
            }
            return ParameterValuesByNameDic;
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
                Reporter.ToUser(eUserMsgKeys.ExportExcelFileDetails);
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
                    workbook = SpreadsheetDocument.Open(sFilePath, true);
                else
                {
                    workbook = SpreadsheetDocument.Create(sFilePath, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
                    WorkbookPart workbookPart = workbook.AddWorkbookPart();
                    workbook.WorkbookPart.Workbook = new Workbook();
                    workbook.WorkbookPart.Workbook.Sheets = new Sheets();
                }
       
                uint sheetId = 1;
                var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                sheetPart.Worksheet = new Worksheet(sheetData);
                Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                Sheet oSheet = sheets.Elements<Sheet>().Where(s => s.Name == sSheetName).FirstOrDefault();
                if (oSheet != null)
                    oSheet.Remove();

                if (sheets.Elements<Sheet>().Count() > 0)
                {
                    sheetId =
                        sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                }
                Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sSheetName };
                sheets.Append(sheet);
                Row headerRow = new Row();
                List<string> columns = new List<string>();
                foreach (DataColumn column in table.Columns)
                {
                    columns.Add(column.ColumnName);
                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);
                    headerRow.AppendChild(cell);
                }
                sheetData.AppendChild(headerRow);
                foreach (DataRow dsrow in table.Rows)
                {
                    Row newRow = new Row();          
                    foreach (String col in columns)
                    {
                        Cell cell = new Cell() ; 
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString());
                        newRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(newRow);
                }
                workbook.Close();
                return true;
            }
            catch(Exception ex)
            {
                if (ShowMessage)
                    Reporter.ToUser(eUserMsgKeys.ExportExcelFileFailed, ex.Message.ToString());
            }
            return false;
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
        public Dictionary<string, List<string>> UpdateParametersOptionalValuesFromDB()
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
            Dictionary<string, List<string>>  ParameterValuesByNameDic =new Dictionary<string, List<string>>();
            dtDB = new DataTable();
            dtDB.Columns.Add("Parameter Name", typeof(string));
            for (int i =0; i < ParamNameList.Count; i++)
            {
                List<string> values = ParamValues[i].OfType<string>().ToList<string>();
                values = values.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                if (!ParameterValuesByNameDic.ContainsKey(ParamNameList.ElementAt(i)))
                {
                    ParameterValuesByNameDic.Add(ParamNameList.ElementAt(i), values);
                    DataRow row = dtDB.NewRow();
                    row["Parameter Name"] = ParamNameList[i].ToString();
                    dtDB.Rows.Add(row);
                }      
                else//different columns with the same alias name
                {
                    List<string> extraValues = ParameterValuesByNameDic[ParamNameList.ElementAt(i)];
                    extraValues.AddRange(values);
                    values = extraValues.Where(s => !string.IsNullOrEmpty(s)).Distinct().ToList();
                    ParameterValuesByNameDic.Remove(ParamNameList.ElementAt(i));
                    ParameterValuesByNameDic.Add(ParamNameList.ElementAt(i), values);
                }  
            }
            return ParameterValuesByNameDic;
        }
        #endregion

    }
}
