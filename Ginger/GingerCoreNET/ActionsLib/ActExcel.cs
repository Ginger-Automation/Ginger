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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using Amdocs.Ginger.Repository;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
            TBH.AddText("Read Excel Cells:- User option to Read Excel data by cells, Select Read Cell Data from Action type dropdown, Then browse the file by clicking Browse button.Once you " +
             "browse the file then all the sheets will get bind to Sheet Name dropdown,Select sheet name and click on view button.If you want to put where condition on excel sheet then type cell location in the excel. for one cell: Like A2, for multi cells: Like A2:D4");
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
                return GetInputParamValue(nameof(ExcelFileName));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ExcelFileName), value);
                OnPropertyChanged(nameof(ExcelFileName));
            }
        }
        public string CalculatedFileName
        {
            get
            {
                string file = GetInputParamCalculatedValue(nameof(ExcelFileName)) ?? "";
                return WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(file);
            }
        }

        private string CalculatedColMappingRules
        {
            get
            {
                string mapping = GetInputParamCalculatedValue(nameof(ColMappingRules));
                return mapping == null ? mapping : mapping.Replace("\"", "'");
            }
        }
        public string SheetName
        {
            get
            {
                return GetInputParamValue(nameof(SheetName));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SheetName), value);
                OnPropertyChanged(nameof(SheetName));
            }
        }
        public string CalculatedSheetName
        {
            get
            {
                string sheet = GetInputParamCalculatedValue(nameof(SheetName)) ?? "";
                return sheet.Trim();
            }
        }
        public string SelectRowsWhere
        {
            get
            {
                return GetInputParamValue(nameof(SelectRowsWhere));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SelectRowsWhere), value);
                OnPropertyChanged(nameof(SelectRowsWhere));
            }
        }
        public string CalculatedFilter
        {
            get
            {
                string filter = GetInputParamCalculatedValue(nameof(SelectRowsWhere));
                return filter == null ? filter : filter.Replace("\"", "'");
            }
        }
        public string PrimaryKeyColumn
        {
            get
            {
                return GetInputParamValue(nameof(PrimaryKeyColumn));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(PrimaryKeyColumn), value);
                OnPropertyChanged(nameof(PrimaryKeyColumn));
            }
        }
        public string SetDataUsed
        {
            get
            {
                return GetInputParamValue(nameof(SetDataUsed));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SetDataUsed), value);
                OnPropertyChanged(nameof(SetDataUsed));
            }
        }
        private string CalculatedSetDataUsed
        {
            get
            {
                string setData = GetInputParamCalculatedValue(nameof(SetDataUsed));
                return setData == null ? setData : setData.Replace("\"", "'");
            }
        }
        public eExcelActionType ExcelActionType
        {
            get
            {
                return (eExcelActionType)GetOrCreateInputParam<eExcelActionType>(nameof(ExcelActionType), eExcelActionType.ReadData);
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ExcelActionType), value.ToString());
            }
        }

        public bool SelectAllRows
        {
            get
            {
                bool value = false;
                bool.TryParse(GetOrCreateInputParam(nameof(SelectAllRows)).Value, out value);
                return value;
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(SelectAllRows), value.ToString());
            }
        }
        public string ColMappingRules
        {
            get
            {
                return GetInputParamValue(nameof(ColMappingRules));
            }
            set
            {
                AddOrUpdateInputParamValue(nameof(ColMappingRules), value);
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
            if (!CheckMandatoryFieldsExists(new List<string>() { nameof(CalculatedFileName), nameof(CalculatedSheetName) }))
            {
                return;
            }
            using (excelOperator = new ExcelNPOIOperations())
            {
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
                        Reporter.ToLog(eLogLevel.INFO, "Only action type can be selected");
                        break;
                }
            }
        }
        public object this[string propertyName]
        {
            get { return this.GetType().GetProperty(propertyName).GetValue(this, null); }
        }

        public bool CheckMandatoryFieldsExists(List<string> fields)
        {
            foreach (string field in fields)
            {
                if (String.IsNullOrWhiteSpace((string)this[field]))
                {
                    this.Error += eUserMsgKey.ExcelInvalidFieldData;
                    return false;
                }
            }
            return true;
        }

        private void ReadCellData()
        {
            try
            {
                DataTable excelDataTable = excelOperator.ReadCellData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows);
                if (!string.IsNullOrEmpty(SelectRowsWhere) && !SelectAllRows)
                {
                    string CellValue = excelDataTable.Rows[0][0].ToString();
                    AddOrUpdateReturnParamActual(excelDataTable.Columns[0].ColumnName, CellValue);
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
                            AddOrUpdateReturnParamActualWithPath(excelDataTable.Columns[i].ColumnName, ((object)r[i]).ToString(), (j + 1).ToString() + (i + 1).ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Error += eUserMsgKey.ExcelInvalidFieldData + ", " + ex.Message;
            }
        }
        public void ReadData()
        {
            try
            {
                DataTable excelDataTable = excelOperator.ReadData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows);
                if (excelDataTable != null && excelDataTable.Rows.Count > 0)
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
                            isUpdated = excelOperator.UpdateExcelData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, FieldsValueToTupleList(CalculatedSetDataUsed));
                        }
                    }
                    else
                    {
                        if (!String.IsNullOrWhiteSpace(SetDataUsed))
                        {
                            if (string.IsNullOrWhiteSpace(PrimaryKeyColumn))
                            {
                                Error += "Missing or Invalid Primary Key";
                                return;
                            }
                            isUpdated = excelOperator.UpdateExcelData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, FieldsValueToTupleList(CalculatedSetDataUsed), CalculatedPrimaryKeyFilter(excelDataTable.Rows[0]));
                        }
                    }
                    if (!isUpdated)
                    {
                        Error = "Failed updated excel data: " + CalculatedSetDataUsed;
                    }
                }
                else
                {
                    Error = SelectAllRows ? "No Rows Found" : "No Rows Found with given filter";
                }
            }
            catch (Exception ex)
            {
                Error += eUserMsgKey.ExcelInvalidFieldData + ", " + ex.Message;
            }
        }

        public void WriteData()
        {
            try
            {
                List<Tuple<string, object>> cellValuesToUpdateList = new List<Tuple<string, object>>();
                cellValuesToUpdateList.AddRange(FieldsValueToTupleList(CalculatedSetDataUsed));
                cellValuesToUpdateList.AddRange(FieldsValueToTupleList(CalculatedColMappingRules));
                DataTable excelDataTable = excelOperator.ReadData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, SelectAllRows);
                if (excelDataTable == null)
                {
                    Error = "Table return no Rows with given filter";
                    return;
                }
                // we expect only 1 record
                if (excelDataTable.Rows.Count == 1 && !SelectAllRows)
                {
                    DataRow r = excelDataTable.Rows[0];
                    this.ExInfo = "Write action done";
                    if (cellValuesToUpdateList.Count > 0)
                    {
                        bool isUpdated = string.IsNullOrEmpty(CalculatedPrimaryKeyFilter(r)) ? excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList) :
                            excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList, CalculatedPrimaryKeyFilter(r));
                    }
                }
                else if (excelDataTable.Rows.Count > 0 && SelectAllRows)
                {
                    bool isUpdated = excelOperator.WriteData(CalculatedFileName, CalculatedSheetName, CalculatedFilter, "", cellValuesToUpdateList);

                    this.ExInfo += "write action done";
                }
                else if (excelDataTable.Rows.Count == 0)
                {
                    this.ExInfo = "No Rows updated with given criteria";
                }
            }
            catch (Exception ex)
            {
                this.Error = "Error when trying to update the excel, Please check write data content";
            }
        }
        private List<Tuple<string, object>> FieldsValueToTupleList(string updatedFieldsValue)
        {
            List<Tuple<string, object>> columnNameAndValue = new List<Tuple<string, object>>();

            if (String.IsNullOrEmpty(updatedFieldsValue))
            {
                return columnNameAndValue;
            }
            bool isError = false;
            string result = System.Text.RegularExpressions.Regex.Replace(updatedFieldsValue, @",(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-COMMA-REPLACE^~");
            string[] varColMaps = result.Split(',');
            varColMaps.ToList().ForEach(c =>
            {
                result = System.Text.RegularExpressions.Regex.Replace(c, @"=(?=[^']*'(?:[^']*'[^']*')*[^']*$)", "~^GINGER-EXCEL-EQUAL-REPLACE^~");
                string[] setData = result.Split('=');

                if (setData.Length == 2)
                {
                    string rowToSet = setData[0].Replace("[", "").Replace("]", "");
                    string valueToSet = setData[1].Replace("~^GINGER-EXCEL-COMMA-REPLACE^~", ",").Replace("~^GINGER-EXCEL-EQUAL-REPLACE^~", "=")
                                        .TrimStart('\'').TrimEnd('\'');
                    string fieldValue = valueToSet;
                    //keeping the translation of vars to support previous implementation
                    VariableBase var = RunOnBusinessFlow.GetHierarchyVariableByName(Value);
                    if (var != null)
                    {
                        var.Value = ValueExpression.Calculate(valueToSet);
                        fieldValue = var.Value;
                    }
                    columnNameAndValue.Add(new Tuple<string, object>(rowToSet, fieldValue));
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, "Invalid data added to 'SetDataUsed' text box");
                    isError = true;
                }

            });
            return isError ? null : columnNameAndValue;
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

        private DataTable GetFilteredDataTable(DataTable dataTable, bool selectAllRows)
        {
            return selectAllRows ? dataTable.DefaultView.ToTable() : dataTable.DefaultView.ToTable().AsEnumerable().Take(1).CopyToDataTable();
        }
        private string CalculatedPrimaryKeyFilter(DataRow dataRow)
        {
            string pk = GetInputParamCalculatedValue(nameof(PrimaryKeyColumn)).Replace("`", "");
            return dataRow[pk].GetType() == typeof(int) ? pk + "=" + dataRow[pk].ToString() : pk + "=" + "'" + dataRow[pk].ToString() + "'";
        }
    }
}
