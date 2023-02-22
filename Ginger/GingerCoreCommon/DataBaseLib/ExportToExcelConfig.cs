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
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DataSource
{
    public class ExportToExcelConfig : ActInputValue
    {
        private string mExcelSheetName = string.Empty;

        [IsSerializedForLocalRepository]
        public string ExcelSheetName 
        {
            get
            {
                return mExcelSheetName;
            }
            set 
            {
                mExcelSheetName = value;
                OnPropertyChanged("ExcelSheetName");
            }
        }

        private string mExcelPath = string.Empty;

       [IsSerializedForLocalRepository]
        public string ExcelPath 
        { 
            get
            {
                return mExcelPath;
            }
            set
            {
                mExcelPath = value;

                OnPropertyChanged("ExcelPath");
            }
        } 

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
            var selectedColumn = new StringBuilder();
            if (selectedColumnList.Count == 0)
            {
                return string.Empty;
            }
            foreach (var column in selectedColumnList)
            {
                selectedColumn.Append(column.ColumnText.ToLower());
                if (selectedColumnList.Count > 1)
                {
                    selectedColumn.Append(",");
                }
            }

            if (selectedColumn[selectedColumn.Length - 1].Equals(','))
            {
                selectedColumn = selectedColumn.Remove((selectedColumn.Length - 1),1);
            }

            var query = "";

            if (dSType.Equals(DataSourceBase.eDSType.LiteDataBase))
            {
                query = selectedColumn.ToString();
            }
            else
            {
                query =string.Concat("Select ", selectedColumn," from ", tableName);
            }

            return query;
        }


        public string CreateQueryWithWhereList(List<ColumnCheckListItem> mColumnList, ObservableList<WhereConditionItem> whereConditionList, string tableName, DataSourceBase.eDSType dSType)
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
                var wCond = whereConditionList[i].Condition.ToLower();
                var wColVal = whereConditionList[i].TableColumn.Trim();
                var wOpr = whereConditionList[i].Opertor;
                var wRowVal = whereConditionList[i].RowValue;

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
                        wQuery = string.Concat(wQuery," ", wCond," ",wColVal," <> ", wRowVal);
                    }
                    else
                    {
                        wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " <>  \"", wRowVal, "\"");
                    }
                }
                else if (wOpr == "Contains")
                {
                    wQuery = string.Concat(wQuery, " ", wCond , " ", wColVal, " Like ", "\"%", wRowVal, "%\"");
                }
                else if (wOpr == "NotContains")
                {
                    wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " Not Like ", "\"%", wRowVal, "%\"");
                }
                else if (wOpr == "StartsWith")
                {
                    wQuery = string.Concat(wQuery , " ", wCond, " ", wColVal, " like ", "\"", wRowVal, "%\"");
                }
                else if (wOpr == "NotStartsWith")
                {
                    wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " Not Like ", "\"", wRowVal, "%\"");
                }
                else if (wOpr == "EndsWith")
                {
                    wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " like ", "\"%", wRowVal, "\"");
                }
                else if (wOpr == "NotEndsWith")
                {
                    wQuery = string.Concat(wQuery, " ", wCond, " ", wColVal, " not like ", "\"%", wRowVal, "\"");
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

            if (mDataTable == null)
            {
                return dsConditionList;
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
