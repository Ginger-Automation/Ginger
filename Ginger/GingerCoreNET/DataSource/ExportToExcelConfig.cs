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

        [IsSerializedForLocalRepository]
        public string ExportQueryValue { get; set; } = string.Empty;

        [IsSerializedForLocalRepository]
        public ObservableList<ColumnCheckListItem> ColumnList { get; set; }
       
        public ObservableList<ActDSConditon> WhereConditionList { get; set; }


        [IsSerializedForLocalRepository]
        public bool IsCustomExport { get; set; } = true;

        [IsSerializedForLocalRepository]
        public bool ExportByWhere { get; set; } = false;

        [IsSerializedForLocalRepository]
        public bool IsExportByQuery { get; set; } = false;

        public string CreateQueryWithColumnList(List<ColumnCheckListItem> selectedColumnList, string tableName)
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

            var query = "Select " + selectedColumn + " from " + tableName;

            return query;
        }

        public string CreateQueryWithWhereList(List<ColumnCheckListItem> mColumnList, ObservableList<GingerCore.DataSource.ActDSConditon> whereConditionList, string tableName)
        {
            var query = CreateQueryWithColumnList(mColumnList, tableName);

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
    }

    public class ColumnCheckListItem : ActInputValue
    {
        [IsSerializedForLocalRepository]
        public string ColumnText { get; set; }

        [IsSerializedForLocalRepository]
        public bool IsSelected { get; set; }
    }
}
