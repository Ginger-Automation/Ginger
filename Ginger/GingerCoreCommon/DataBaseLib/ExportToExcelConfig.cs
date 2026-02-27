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
using Amdocs.Ginger.Repository;
using GingerCore.DataSource;
using System;
using System.Collections.Generic;
using System.Linq;
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
                selectedColumn.Append(column.ColumnText);
                if (selectedColumnList.Count > 1)
                {
                    selectedColumn.Append(",");
                }
            }

            if (selectedColumn[^1].Equals(','))
            {
                selectedColumn = selectedColumn.Remove((selectedColumn.Length - 1), 1);
            }

            var query = "";

            if (dSType.Equals(DataSourceBase.eDSType.LiteDataBase))
            {
                query = selectedColumn.ToString();
            }
            else
            {
                query = string.Concat("Select ", selectedColumn, " from ", tableName);
            }

            return query;
        }

        public string CreateQueryWithWhereList(List<ColumnCheckListItem> mColumnList,ObservableList<WhereConditionItem> whereConditionList,string tableName,DataSourceBase.eDSType dSType)
        {
            // Build column list (NO SQL, just col1,col2,col3)
            string columnList = string.Join(",",
                mColumnList.Where(x => x.IsSelected).Select(x => x.ColumnText));

            // No conditions → return column list only
            if (whereConditionList == null || whereConditionList.Count == 0)
            {
                return columnList;
            }

            // Build WHERE clause in Ginger format
            string whereClause = "";
            for (int i = 0; i < whereConditionList.Count; i++)
            {
                var item = whereConditionList[i];

                if (string.IsNullOrEmpty(item.TableColumn) ||
                    string.IsNullOrEmpty(item.RowValue))
                    continue;

                string predicate = "";

                switch (item.Opertor)
                {
                    case "Equals":
                        predicate = $"{item.TableColumn} = \"{item.RowValue}\"";
                        break;
                    case "NotEquals":
                        predicate = $"{item.TableColumn} <> \"{item.RowValue}\"";
                        break;
                    case "Contains":
                        predicate = $"{item.TableColumn} Like \"%{item.RowValue}%\"";
                        break;
                    case "NotContains":
                        predicate = $"{item.TableColumn} Not Like \"%{item.RowValue}%\"";
                        break;
                    case "StartsWith":
                        predicate = $"{item.TableColumn} Like \"{item.RowValue}%\"";
                        break;
                    case "EndsWith":
                        predicate = $"{item.TableColumn} Like \"%{item.RowValue}\"";
                        break;
                }

                if (i > 0)
                    whereClause += " AND ";

                whereClause += predicate;
            }

            // Return Ginger expected string: "col1,col2 where condition"
            return $"{columnList} where {whereClause}";
        }
        public ObservableList<GingerCore.DataSource.ActDSConditon> GetConditons(ObservableList<WhereConditionItem> conditonStringList, System.Data.DataTable mDataTable)
        {
            var dsConditionList = new ObservableList<GingerCore.DataSource.ActDSConditon>();

            ObservableList<string> possibleCondition = [];
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

            List<string> tableColsValue = [];
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
            var conditionStringList = new ObservableList<WhereConditionItem>();
            if (whereConditionList != null)
            {
                for (int i = 0; i < whereConditionList.Count; i++)
                {
                    var wCond = whereConditionList[i].wCondition.ToString().ToLower();
                    var wColVal = whereConditionList[i].wTableColumn.ToString().Trim();
                    var wOpr = whereConditionList[i].wOperator.ToString();
                    var wRowVal = Convert.ToString(whereConditionList[i].wValue);

                    conditionStringList.Add(new WhereConditionItem() { Condition = wCond, TableColumn = wColVal, Opertor = wOpr, RowValue = wRowVal });
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
