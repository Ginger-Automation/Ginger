using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DataSource
{
    public class ExportToExcelConfig : ActInputValue
    {
        [IsSerializedForLocalRepository]
        public string ExcelSheetName { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public string ExcelPath { get; set; } = string.Empty;

        private string mExportQueryValue = string.Empty;

        [IsSerializedForLocalRepository]
        public string ExportQueryValue
        {
            get
            {
                return mExportQueryValue;
            }
            set
            {
                mExportQueryValue = value;
                OnPropertyChanged("ExportQueryValue");
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ColumnCheckListItem> ColumnList { get; set; }
       
        //public ObservableList<ActDSConditon> WhereConditionList { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<WhereConditionItem> WhereConditionStringList { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsCustomExport { get; set; } = true;

        [IsSerializedForLocalRepository]
        public bool ExportByWhere { get; set; } = false;

        [IsSerializedForLocalRepository]
        public bool IsExportByQuery { get; set; } = false;

        public string CreateQueryWithColumnList(List<ColumnCheckListItem> selectedColumnList, string tableName, DataSourceBase.eDSType dSType)
        {
            var selectedColumn = string.Empty;
            foreach (var column in selectedColumnList)
            {
                selectedColumn += column.ColumnText.ToLower();
                if (selectedColumnList.Count > 1)
                {
                    selectedColumn += ",";
                }
            }

            if (selectedColumn.EndsWith(","))
            {
                selectedColumn = selectedColumn.Remove(selectedColumn.Length - 1);
            }

            var query = "";

            if (dSType.Equals(DataSourceBase.eDSType.LiteDataBase))
            {
                query = selectedColumn;
            }
            else
            {
                query = "Select " + selectedColumn + " from " + tableName;
            }

            return query;
        }


        public string CreateQueryWithWhereList(List<ColumnCheckListItem> mColumnList, ObservableList<GingerCore.DataSource.ActDSConditon> whereConditionList, string tableName, DataSourceBase.eDSType dSType)
        {
            var query = CreateQueryWithColumnList(mColumnList, tableName, dSType);

            if (whereConditionList == null)
            {
                return query;
            }
            var whereQuery = string.Empty;

            for (int i = 0; i < whereConditionList.Count; i++)
            {
                var wQuery = "";
                var wCond = whereConditionList[i].wCondition.ToString().ToLower();
                var wColVal = whereConditionList[i].wTableColumn.ToString().Trim();
                var wOpr = whereConditionList[i].wOperator.ToString();
                var wRowVal = Convert.ToString(whereConditionList[i].wValue);

                if (string.IsNullOrEmpty(wRowVal))
                {
                    continue;
                }

                if (wCond == "empty")
                {
                    wCond = "";
                }

                if (wOpr == "Equals")
                {
                    if (wColVal == "GINGER_ID")
                    {
                        wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " = ", wRowVal);
                    }
                    else
                    {
                        wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " = \"", wRowVal, "\"");
                    }
                }
                else if (wOpr == "NotEquals")
                {
                    if (wColVal == "GINGER_ID")
                    {
                        wQuery = string.Concat(wQuery," ", wCond," ",wColVal," != ", wRowVal);
                    }
                    else
                    {
                        wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " !=  \"", wRowVal, "\"");
                    }
                }
                else if (wOpr == "Contains")
                {
                    wQuery = string.Concat(wQuery, " ", wCond , " ", wColVal, " contains ", "\"", wRowVal, "\"");
                }
                else if (wOpr == "StartsWith")
                {
                    wQuery = string.Concat(wQuery , " ", wCond, " ", wColVal, " like ", "\"", wRowVal, "\"");
                }
                    
                
                whereQuery = string.Concat(whereQuery, wQuery);
            }
            if (whereQuery != string.Empty)
            {
                query += " Where " + whereQuery;
            }
            return query;
        }

        public ObservableList<GingerCore.DataSource.ActDSConditon> GetConditons(ObservableList<WhereConditionItem> conditonStringList, System.Data.DataTable mDataTable)
        {
            var dsConditionList = new ObservableList<GingerCore.DataSource.ActDSConditon>();

            ObservableList<string> possibleCondition = new ObservableList<string>();
            foreach (ActDSConditon.eCondition item in Enum.GetValues(typeof(ActDSConditon.eCondition)))
            {
                if (item.ToString() != "EMPTY")
                {
                    possibleCondition.Add(item.ToString());
                }
            }

          
            List<string> tableColsValue = new List<string>();
            var columns = mDataTable.Columns;
            foreach (var item in columns)
            {
                tableColsValue.Add(item.ToString());
            }

            if (conditonStringList != null)
            {
                foreach (var item in conditonStringList)
                {

                    ActDSConditon.eCondition eCondition;
                    if (string.IsNullOrEmpty(item.Condition) || item.Condition.ToLower().Equals("empty"))
                    {
                        eCondition = ActDSConditon.eCondition.EMPTY;
                    }
                    else
                    {
                        Enum.TryParse(item.Condition, out eCondition);
                    }
                    Enum.TryParse(item.Opertor, out ActDSConditon.eOperator eOperator);
                    dsConditionList.Add(new ActDSConditon()
                    {
                        PossibleCondValues = possibleCondition,
                        PossibleColumnValues = tableColsValue,
                        wCondition = eCondition,
                        wTableColumn = item.TableColumn,
                        wOperator = eOperator,
                        wValue = item.RowValue
                    });
                }
            }

            return dsConditionList;
        }

        public ObservableList<WhereConditionItem> CreateConditionStringList(ObservableList<GingerCore.DataSource.ActDSConditon> whereConditionList)
        {
            var   conditionStringList = new ObservableList<WhereConditionItem>();
            if (whereConditionList != null)
            {
                for (int i = 0; i < whereConditionList.Count; i++)
                {
                    var wCond = whereConditionList[i].wCondition.ToString().ToLower();
                    var wColVal = whereConditionList[i].wTableColumn.ToString().Trim();
                    var wOpr = whereConditionList[i].wOperator.ToString();
                    var wRowVal = Convert.ToString(whereConditionList[i].wValue);

                    conditionStringList.Add(new WhereConditionItem() { Condition = wCond,TableColumn= wColVal,Opertor= wOpr, RowValue= wRowVal });
                }

            }
            return conditionStringList;
        }
    }

    public class ColumnCheckListItem : ActInputValue
    {
        [IsSerializedForLocalRepository]
        public string ColumnText { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }
    }

    public class WhereConditionItem : ActInputValue
    {
        [IsSerializedForLocalRepository]
        public string Condition { get; set; }

        [IsSerializedForLocalRepository]
        public string TableColumn { get; set; }

        [IsSerializedForLocalRepository]
        public string Opertor { get; set; }

        [IsSerializedForLocalRepository]
        public string RowValue { get; set; }
    }
}
