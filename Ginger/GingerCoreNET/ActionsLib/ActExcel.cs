#region License
/*
Copyright © 2014-2021 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GingerCore.Actions
{
    public class ActExcel : ActWithoutDriver
    {
        public override string ActionDescription { get { return "Excel Action"; } }
        public override string ActionUserDescription { get { return "Read/Write Excel"; } }

        public override void ActionUserRecommendedUseCase(ITextBoxFormatter TBH)
        {
            TBH.AddText("Use this action in case you need to Read/Write/etc. excel sheet from/on a system drives.");
            TBH.AddLineBreak();
            TBH.AddText("This action contains list of options which will allow you to read/write excel file and also read excel rows with Where conditions.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Read Excel :- If you want to read excel sheet from system then select Read data from Excel Action type dropdown, Then browse the file by clicking Browse button.Once you " +
             "browse the file then all the sheets will get bind to Sheet Name dropdown,Select sheet name and click on view button.If you want to put where condition on excel sheet then type column name and it's value e.g. ColName=1 for integer column and ColName='colname' for string column");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Write Excel :- Similar as read action,to write an excel, select Write data from Excel Action type dropdown, Then browse the file by clicking Browse button,select sheet name and then add where condition on column which you want to write, Then add primary key column if you have any.Then finally to write an excel add column name =Variable name in Variable to col textbox and run the action");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("Note:- Column name should not be the same name as the variable name.");
        }

        public override string ActionEditPage { get { return "ActExcelEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return false; } }
        public override bool ValueConfigsNeeded { get { return false; } }

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
            [EnumValueDescription("Read Data")]
            ReadData,
            [EnumValueDescription("Write Data")]
            WriteData,
            [EnumValueDescription("Read Cell Data")]
            ReadCellData
        }
        public string ExcelFileName
        {
            get
            {
                return GetInputParamValue("ExcelFileName");
            }
            set
            {
                AddOrUpdateInputParamValue("ExcelFileName", value);
                OnPropertyChanged(nameof(ExcelFileName));
            }
        }
        public string SheetName
        {
            get
            {
                return GetInputParamValue("SheetName");
                //return GetInputParamCalculatedValue("SheetName").Trim();
            }
            set
            {
                AddOrUpdateInputParamValue("SheetName", value);
                OnPropertyChanged(nameof(SheetName));
            }
        }
        public string SelectRowsWhere
        {
            get
            {
                return GetInputParamValue("SelectRowsWhere");
                //return GetInputParamCalculatedValue("SelectRowsWhere");
            }
            set
            {
                AddOrUpdateInputParamValue("SelectRowsWhere", value);
                OnPropertyChanged(nameof(SelectRowsWhere));
            }
        }
        public string PrimaryKeyColumn
        {
            get
            {
                return GetInputParamValue("PrimaryKeyColumn");
            }
            set
            {
                AddOrUpdateInputParamValue("PrimaryKeyColumn", value);
                OnPropertyChanged(nameof(PrimaryKeyColumn));
            }
        }
        public string SetDataUsed
        {
            get
            {
                return GetInputParamValue("SetDataUsed");
            }
            set
            {
                AddOrUpdateInputParamValue("SetDataUsed", value);
                OnPropertyChanged(nameof(SetDataUsed));
            }
        }
        [IsSerializedForLocalRepository]
        public eExcelActionType ExcelActionType { set; get; }

        [IsSerializedForLocalRepository]
        public bool SelectAllRows { set; get; }
        public string ColMappingRules
        {
            get
            {
                return GetInputParamValue("ColMappingRules");
            }
            set
            {
                AddOrUpdateInputParamValue("ColMappingRules", value);
                OnPropertyChanged(nameof(ColMappingRules));
            }
        }

        public override string ActionType
        {
            get { return "Excel" + ExcelActionType.ToString(); }
        }

        public override eImageType Image { get { return eImageType.ExcelFile; } }

        IExcelOperations excelOperator = null;

        public override void Execute()
        {
            if(!CheckMandatoryFieldsExists())
            {
                return;
            }
            excelOperator = new ExcelNPOIOperations();
            switch (ExcelActionType)
            {
                case eExcelActionType.ReadData:
                    ReadData();
                    break;
                case eExcelActionType.WriteData:
                    WriteData();
                    break;
                case eExcelActionType.ReadCellData:
                    ReadCellData();
                    break;

                default:

                    break;
            }
        }

        private bool CheckMandatoryFieldsExists()
        {
            if(String.IsNullOrWhiteSpace(ExcelFileName))
            {
                this.Error += eUserMsgKey.MissingExcelDetails;
                Reporter.ToUser(eUserMsgKey.MissingExcelDetails);
                return false;
            }
            if (String.IsNullOrWhiteSpace(SheetName))  
            {
                this.Error += eUserMsgKey.ExcelNoWorksheetSelected;
                Reporter.ToUser(eUserMsgKey.ExcelNoWorksheetSelected);
                return false;
            }
            return true;
        }

        private void ReadCellData()
        {
            DataTable excelDataTable = excelOperator.ReadCellData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SelectAllRows);
            try
            {
                if (!string.IsNullOrEmpty(SelectRowsWhere) && SelectAllRows == false)
                {
                    string CellValue = excelDataTable.Rows[0][0].ToString();
                    AddOrUpdateReturnParamActual("Actual", CellValue);
                }
                else
                {
                    for (int j = 0; j < excelDataTable.Rows.Count; j++)
                    {
                        DataRow r = excelDataTable.Rows[j];
                        //Read data to return values
                        // in case the user didn't select cols then get all excel output columns
                        for (int i = 0; i < excelDataTable.Columns.Count; i++)
                        {
                            AddOrUpdateReturnParamActualWithPath("Actual", ((object)r[i]).ToString(), (j + 1).ToString() + (i + 1).ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = ex.Message;
            }

            if (excelDataTable.Rows.Count == 0)
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = "No rows found in excel file matching criteria - ";
            }

        }

        public List<string> GetSheets()
        {
            List<string> returnList = new List<string>();
            var fileExtension = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ExcelFileName);
            excelOperator = new ExcelNPOIOperations()
            {
                FileName = fileExtension
            };
            return excelOperator.GetSheets().OrderBy(itm => itm).ToList();
        }
        public void ReadData()
        {
            DataTable excelDataTable = excelOperator.ReadData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SelectAllRows, PrimaryKeyColumn, SetDataUsed);
            try
            {
                if(excelDataTable != null && excelDataTable.Rows.Count > 0)
                {
                    for (int j = 0; j < excelDataTable.Rows.Count; j++)
                    {
                        DataRow r = excelDataTable.Rows[j];
                        for (int i = 0; i < excelDataTable.Columns.Count; i++)
                        {
                            
                            if (SelectAllRows)
                            {
                                AddOrUpdateReturnParamActualWithPath(excelDataTable.Columns[i].ColumnName, ((object)r[i]).ToString(), "" + (j + 1).ToString());
                                
                            }
                            else
                            {
                                AddOrUpdateReturnParamActual(excelDataTable.Columns[i].ColumnName, ((object)r[i]).ToString());
                            }

                        }
                        
                    }
                    bool isUpdated = true;
                    if (SelectAllRows)
                    {
                        if (!String.IsNullOrWhiteSpace(SetDataUsed))
                        {
                            isUpdated = excelOperator.updateExcelData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SetDataUsed);
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(SetDataUsed))
                        {
                            if (string.IsNullOrWhiteSpace(PrimaryKeyColumn))
                            {
                                Error += "Missing or Invalid Primary Key"; 
                                Reporter.ToLog(eLogLevel.WARN, Error);
                                return; // send error message PK missing
                            }
                            string convertPKtoFilter = PrimaryKeyColumn + "=" + excelDataTable.Rows[0][GetInputParamCalculatedValue("PrimaryKeyColumn")].ToString(); //// should take from typeof

                            isUpdated = excelOperator.updateExcelData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SetDataUsed, convertPKtoFilter);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = ex.Message;
            }

            if (excelDataTable.Rows.Count == 0)
            {
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                Error = "No rows found in excel file matching criteria - ";
            }
        }

        public bool check_file_open(String name)
        {
            try
            {
                FileStream fs = new FileStream(name, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                fs.Close();
                return true;
            }
            catch (Exception ex)
            {
                Error = "Write Operation canceled due to error: " + ex.Message;
                return false;
            }
        }


        public void WriteData()
        {
            List<Tuple<string, object>> updateCellValuesList = new List<Tuple<string, object>>();
            DataTable excelDataTable = excelOperator.ReadData(ExcelFileName, SheetName, SelectRowsWhere, SelectAllRows, PrimaryKeyColumn, SetDataUsed);
            
            string result = System.Text.RegularExpressions.Regex.Replace(GetInputParamCalculatedValue("ColMappingRules"), @",(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-COMMA-REPLACE^~");
            string[] varColMaps = result.Split(',');
            string sSetDataUsed = "";

            try
            {

                if (!string.IsNullOrEmpty(GetInputParamCalculatedValue("SetDataUsed")))
                    sSetDataUsed = @", " + GetInputParamCalculatedValue("SetDataUsed");

                // we expect only 1 record
                if (excelDataTable.Rows.Count == 1 && SelectAllRows == false)
                {
                    DataRow r = excelDataTable.Rows[0];
                    string strPrimaryKeyColumn = GetInputParamCalculatedValue("PrimaryKeyColumn");
                    if (strPrimaryKeyColumn.Contains("`")) strPrimaryKeyColumn = strPrimaryKeyColumn.Replace("`", "");
                    string rowKey = r[strPrimaryKeyColumn].ToString();
                    //Read data to variables
                    foreach (string vc in varColMaps)
                    {
                        int res;
                        int.TryParse(rowKey, out res);

                        if (res == 0 || r[strPrimaryKeyColumn].GetType() == typeof(System.String))
                        {
                            rowKey = "'" + rowKey + "'";
                        }

                        //TODO: fix me in OO Style

                        //Do mapping
                        string ColName = vc.Split('=')[0];
                        string Value = vc.Split('=')[1];
                        Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
                        string txt = Value;

                        //keeping the translation of vars to support previous implementation
                        VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                        if (var != null)
                        {
                            var.Value = ValueExpression.Calculate(var.Value);
                            txt = var.Value;
                        }

                        //remove '' from value
                        txt = txt.TrimStart(new char[] { '\'' });
                        txt = txt.TrimEnd(new char[] { '\'' });
                        updateCellValuesList.Add(new Tuple<string, object>(ColName, txt));
                    }
                    string setCellData = string.Join(",", updateCellValuesList.Select(st => st.Item1 + " = '" + st.Item2 + "' ,")).TrimEnd(',');

                    this.ExInfo = "Write action done";
                    if (rowKey != null && updateCellValuesList.Count > 0)
                    {
                        bool isUpdated = string.IsNullOrEmpty(rowKey) ? excelOperator.WriteData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SetDataUsed, updateCellValuesList) : 
                            excelOperator.WriteData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SetDataUsed, updateCellValuesList, PrimaryKeyColumn, rowKey);
                    }
                }
                else if (excelDataTable.Rows.Count > 0 && SelectAllRows == true)
                {
                    foreach (string vc in varColMaps)
                    {
                        //Do mapping
                        string ColName = vc.Split('=')[0];
                        string Value = vc.Split('=')[1];
                        Value = Value.Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",");
                        string txt = Value;

                        //keeping the translation of vars to support previous implementation
                        VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                        if (var != null)
                        {

                            var.Value = ValueExpression.Calculate(var.Value);
                            if (var != null)
                                txt = var.Value;
                            else
                                txt = Value;
                        }

                        //remove '' from value
                        txt = txt.TrimStart(new char[] { '\'' });
                        txt = txt.TrimEnd(new char[] { '\'' });


                        updateCellValuesList.Add(new Tuple<string, object>(ColName, txt));
                    }
                    bool isUpdated = excelOperator.WriteData(GetExcelFileNameForDriver(), SheetName, SelectRowsWhere, SetDataUsed, updateCellValuesList);
                    
                    this.ExInfo += "write action done";
                }
                else if (excelDataTable.Rows.Count == 0)
                {
                    this.ExInfo = "No Rows updated with given criteria";
                }
            }
            catch (Exception ex)
            {
                // Reporter.ToLog(eAppReporterLogLevel.ERROR, "Writing into excel got error " + ex.Message);
                this.Error = "Error when trying to update the excel: " + ex.Message + Environment.NewLine + "UpdateSQL=";
            }
            finally
            {

            }

            // then show a message if needed
            if (excelDataTable.Rows.Count == 0)
            {
                //TODO: reporter
                // Reporter.ToUser("No rows found in excel file matching criteria - " + sql);                
                //  throw new Exception("No rows found in excel file matching criteria - " + sql);
            }
            
        }

        public DataTable GetExcelSheetData(string where)
        {
            try
            {
                if(!CheckMandatoryFieldsExists())
                {
                    return null;
                }
                if(string.IsNullOrEmpty(SheetName))
                {
                    string missingSheetName = "Invalid or missing sheet name";
                    Reporter.ToLog(eLogLevel.WARN, missingSheetName);
                    throw new Exception(missingSheetName);
                }
                if(where != null && ExcelActionType == eExcelActionType.ReadCellData)
                {
                    return excelOperator.ReadCellData(ExcelFileName, SheetName, where, true);
                }
                return excelOperator.ReadData(ExcelFileName, SheetName, where, true, "", "");
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "Syntax error in FROM clause.":
                        break;
                    case "No value given for one or more required parameters.":
                        Reporter.ToUser(eUserMsgKey.ExcelBadWhereClause);
                        break;
                    default:
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
                        break;
                }
                return null;
            }
        }

        internal static ObservableList<ActReturnValue> GetVarColsFromString(string sVarCols)
        {
            ObservableList<ActReturnValue> VarCols = new ObservableList<ActReturnValue>();

            string[] VarColMap = sVarCols.Split(',');


            foreach (var c in VarColMap)
            {
                VarCols.Add(new ActReturnValue() { Param = c });
            }



            return VarCols;
        }
        internal static string[] GetVarColsMapsFromString(string sVarCols)
        {


            string[] VarColMap = sVarCols.Split(',');



            return VarColMap;
        }

        public string GetExcelFileNameForDriver()
        {
            string ExcelFileNameAbsolutue = GetInputParamCalculatedValue(nameof(ExcelFileName));

            if (string.IsNullOrWhiteSpace(ExcelFileNameAbsolutue))
            {
                return "";
            }
            ExcelFileNameAbsolutue = WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(ExcelFileNameAbsolutue);
            
            return ExcelFileNameAbsolutue;
        }

        public DataTable GetExcelSheetDataWithWhere()
        {
            return GetExcelSheetData(GetInputParamCalculatedValue("SelectRowsWhere"));
        }
        private DataTable GetFilteredDataTable(DataTable dataTable, bool selectAllRows)
        {
            return selectAllRows ? dataTable.DefaultView.ToTable() : dataTable.DefaultView.ToTable().AsEnumerable().Take(1).CopyToDataTable();
        }
    }
}
