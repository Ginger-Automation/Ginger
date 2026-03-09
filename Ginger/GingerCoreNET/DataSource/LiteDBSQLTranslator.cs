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

using GingerCore.Actions;
using System;
using System.Text;

namespace Amdocs.Ginger.CoreNET.DataSource
{
    /// <summary>
    /// Purpose of this class is to convert the previous liteDB version's Shell command to SQL like command along with creating queries in 
    /// accordance with the LiteDB version 5.0.17 
    /// The translated queries are subsequently used in the LiteDB.cs class
    /// </summary>
    public class LiteDBSQLTranslator
    {
        private readonly ActDSTableElement actDSTableElement;
        private StringBuilder queryBuilder;
        public LiteDBSQLTranslator(ActDSTableElement actDSTableElement)
        {
            ArgumentNullException.ThrowIfNull(actDSTableElement);
            this.actDSTableElement = actDSTableElement;
            queryBuilder = new();
        }
        public string CreateValueExpression()
        {
            if (actDSTableElement.BySelectedCell)
            {
                return this.actDSTableElement.ValueExp;
            }

            queryBuilder = new();
            queryBuilder.Append('{');
            queryBuilder.Append($"DS Name={actDSTableElement.DSName} DST={actDSTableElement.DSTableName} MASD=");
            if (actDSTableElement.MarkUpdate)
            {
                queryBuilder.Append('Y');
            }
            else
            {
                queryBuilder.Append('N');
            }

            if (actDSTableElement.ByQuery)
            {
                queryBuilder.Append(" Query QUERY=");
                queryBuilder.Append(actDSTableElement.QueryValue);
                queryBuilder.Append('}');
                return queryBuilder.ToString();
            }

            CustomizedSQLQuery.InitForCustomized(actDSTableElement, queryBuilder);
            queryBuilder.Append(" Query QUERY=");
            switch (actDSTableElement.ControlAction)
            {
                case ActDSTableElement.eControlAction.ExportToExcel:
                    ValueExpressionForExportToExcel();
                    break;
                case ActDSTableElement.eControlAction.GetValue:
                    ValueExpressionForGetValue();
                    break;
                case ActDSTableElement.eControlAction.SetValue:
                    ValueExpressionForSetValue();
                    break;
                case ActDSTableElement.eControlAction.DeleteRow:
                    ValueExpressionForDeleteRow();
                    break;
                case ActDSTableElement.eControlAction.RowCount:
                    ValueExpressionForRowCount();
                    break;
                case ActDSTableElement.eControlAction.AvailableRowCount:
                    ValueExpressionForAvailableRowCount();
                    break;
                case ActDSTableElement.eControlAction.AddRow:
                    ValueExpressionForAddRow();
                    break;
                case ActDSTableElement.eControlAction.MarkAsDone:
                    ValueExpressionForMarkAsDone();
                    break;
                case ActDSTableElement.eControlAction.DeleteAll:
                    ValueExpressionForDeleteAll();
                    break;
                case ActDSTableElement.eControlAction.MarkAllUsed:
                    ValueExpressionForMarkAllUsed();
                    break;
                case ActDSTableElement.eControlAction.MarkAllUnUsed:
                    ValueExpressionForMarkAllUnused();
                    break;
            }
            queryBuilder.Append('}');

            return queryBuilder.ToString();
        }
        private void ValueExpressionForExportToExcel()
        {

            if (actDSTableElement.ExcelConfig == null)
            {
                queryBuilder.Append($"{actDSTableElement.ExcelPath},{actDSTableElement.ExcelSheetName}");
            }
            else
            {
                queryBuilder.Append($"{actDSTableElement.ExcelConfig.ExcelPath},{actDSTableElement.ExcelConfig.ExcelSheetName}");
            }
        }

        private void ValueExpressionForGetValue()
        {
            if (actDSTableElement.IsKeyValueTable)
            {
                GingerKeyValueSQLQuery.GetValueForGingerKeyValue(actDSTableElement, queryBuilder);
            }

            else if (actDSTableElement.Customized)
            {
                CustomizedSQLQuery.GetValueForCustomized(actDSTableElement, queryBuilder);
            }
        }

        private void ValueExpressionForSetValue()
        {
            if (actDSTableElement.IsKeyValueTable)
            {
                GingerKeyValueSQLQuery.SetValueForGingerKeyValue(actDSTableElement, queryBuilder);
            }
            else if (actDSTableElement.Customized)
            {
                CustomizedSQLQuery.SetValueForCustomized(actDSTableElement, queryBuilder);
            }
        }

        private void ValueExpressionForDeleteRow()
        {
            if (actDSTableElement.IsKeyValueTable)
            {
                GingerKeyValueSQLQuery.DeleteRowForGingerKeyValue(actDSTableElement, queryBuilder);
            }

            else if (actDSTableElement.Customized)
            {
                CustomizedSQLQuery.DeleteRowForForCustomized(actDSTableElement, queryBuilder);
            }
        }

        private void ValueExpressionForRowCount()
        {
            queryBuilder.Append($"Select COUNT(*) FROM {actDSTableElement.DSTableName}");
        }

        private void ValueExpressionForAddRow()
        {
            queryBuilder.Append($"INSERT INTO {actDSTableElement.DSTableName}");
        }

        private void ValueExpressionForAvailableRowCount()
        {
            queryBuilder.Append($"SELECT $ FROM {actDSTableElement.DSTableName} where GINGER_USED= \"False\"");
        }

        private void ValueExpressionForMarkAsDone()
        {
            queryBuilder.Append($"UPDATE {actDSTableElement.DSTableName} SET GINGER_USED= \"True\"");
            if (actDSTableElement.ByWhere)
            {
                queryBuilder.Append(" where ");
                CustomizedSQLQuery.SetWhereConditions(actDSTableElement, queryBuilder);
            }
        }

        private void ValueExpressionForDeleteAll()
        {
            queryBuilder.Append($"DELETE {actDSTableElement.DSTableName}");
        }

        private void ValueExpressionForMarkAllUsed()
        {
            queryBuilder.Append($"UPDATE {actDSTableElement.DSTableName} SET GINGER_USED= \"True\"");
        }

        private void ValueExpressionForMarkAllUnused()
        {
            queryBuilder.Append($"UPDATE {actDSTableElement.DSTableName} SET GINGER_USED = \"False\"");
        }
    }

    static class GingerKeyValueSQLQuery
    {
        public static void GetValueForGingerKeyValue(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            // db.MyKeyValueDataTable.select GINGER_KEY_VALUE where GINGER_KEY_NAME="Name"
            string ValueExp = actDSTableElement.ValueExp;
            string Query = string.IsNullOrEmpty(ValueExp) ? string.Empty :
                ValueExp.Substring(ValueExp.IndexOf("QUERY=") + 6, ValueExp.Length - (ValueExp.IndexOf("QUERY=") + 7));

            if (!string.IsNullOrEmpty(Query) && Query.StartsWith("db"))
            {
                string GingerKeyName = Query.Split("GINGER_KEY_NAME=")?[1];

                queryBuilder.Append($"SELECT Ginger_KEY_VALUE FROM {actDSTableElement.DSTableName} where GINGER_KEY_NAME={GingerKeyName}");
            }
            else
            {
                queryBuilder.Append($"SELECT Ginger_KEY_VALUE FROM {actDSTableElement.DSTableName} where GINGER_KEY_NAME={actDSTableElement.KeyName}");
            }

        }

        public static void SetValueForGingerKeyValue(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            //$"Update {mDSTable.Name} SET GINGER_KEY_VALUE = \"{mActDSTblElem.ValueUC}\" where GINGER_KEY_NAME=\"{cmbKeyName.Text}\""
            string ValueExp = actDSTableElement.ValueExp;
            string Query = string.IsNullOrEmpty(ValueExp) ? string.Empty :
                ValueExp.Substring(ValueExp.IndexOf("QUERY=") + 6, ValueExp.Length - (ValueExp.IndexOf("QUERY=") + 7));

            if (!string.IsNullOrEmpty(Query) && Query.StartsWith("db"))
            {
                string GingerKeyName = Query.Split("GINGER_KEY_NAME=")?[1];

                queryBuilder.Append($"UPDATE {actDSTableElement.DSTableName} SET GINGER_KEY_VALUE = \"{actDSTableElement.ValueUC}\" where GINGER_KEY_NAME={GingerKeyName}");
            }
            else
            {
                queryBuilder.Append($"UPDATE {actDSTableElement.DSTableName} SET GINGER_KEY_VALUE = \"{actDSTableElement.ValueUC}\" where GINGER_KEY_NAME=\"{actDSTableElement.KeyName}\"");
            }
        }

        public static void DeleteRowForGingerKeyValue(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            string ValueExp = actDSTableElement.ValueExp;
            string Query = string.IsNullOrEmpty(ValueExp) ? string.Empty :
                ValueExp.Substring(ValueExp.IndexOf("QUERY=") + 6, ValueExp.Length - (ValueExp.IndexOf("QUERY=") + 7));

            if (!string.IsNullOrEmpty(Query) && Query.StartsWith("db"))
            {
                string GingerKeyName = Query.Split("GINGER_KEY_NAME=")?[1];
                queryBuilder.Append($"Delete {actDSTableElement.DSTableName} where GINGER_KEY_NAME={GingerKeyName}");
            }
            else
            {
                queryBuilder.Append($"Delete {actDSTableElement.DSTableName} where GINGER_KEY_NAME=\"{actDSTableElement.KeyName}\"");
            }
        }
    }


    static class CustomizedSQLQuery
    {
        public static void InitForCustomized(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            if (!actDSTableElement.Customized)
            {
                return;
            }

            queryBuilder.Append($" IDEN=Cust ICOLVAL={actDSTableElement.LocateColTitle} IROW=");

            if (actDSTableElement.ByRowNum)
            {
                queryBuilder.Append($"RowNum ROWNUM={actDSTableElement.LocateRowValue}");
            }
            else if (actDSTableElement.ByNextAvailable)
            {
                queryBuilder.Append("NxtAvail");
            }
            else if (actDSTableElement.ByWhere)
            {
                queryBuilder.Append("Where COND=");
                SetWhereConditions(actDSTableElement, queryBuilder);
            }
        }
        public static void GetValueForCustomized(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {

            if (actDSTableElement.ByWhere)
            {
                queryBuilder.Append($"SELECT $ FROM {actDSTableElement.DSTableName} where ");
                SetWhereConditions(actDSTableElement, queryBuilder);

            }
            else if (actDSTableElement.ByNextAvailable)
            {
                queryBuilder.Append($"SELECT $ FROM {actDSTableElement.DSTableName} where GINGER_USED =\"False\"");
            }
            else if (actDSTableElement.ByRowNum)
            {
                queryBuilder.Append($"SELECT $ FROM {actDSTableElement.DSTableName}");
            }
        }
        public static void SetValueForCustomized(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            if (actDSTableElement.ByWhere)
            {
                queryBuilder.Append($"Update {actDSTableElement.DSTableName} SET {actDSTableElement.LocateColTitle} = \"{actDSTableElement.ValueUC}\" where ");
                SetWhereConditions(actDSTableElement, queryBuilder);
            }
            else if (actDSTableElement.ByRowNum)
            {
                queryBuilder.Append($"Update {actDSTableElement.DSTableName} SET {actDSTableElement.LocateColTitle} = \"{actDSTableElement.ValueUC}\"");

            }
            else if (actDSTableElement.ByNextAvailable)
            {
                queryBuilder.Append($"Update {actDSTableElement.DSTableName} SET {actDSTableElement.LocateColTitle} = \"{actDSTableElement.ValueUC}\" where GINGER_USED =\"False\"");
            }
        }

        public static void DeleteRowForForCustomized(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            if (actDSTableElement.ByWhere)
            {
                queryBuilder.Append($"DELETE {actDSTableElement.DSTableName} where ");
                SetWhereConditions(actDSTableElement, queryBuilder);
            }

            else if (actDSTableElement.ByNextAvailable)
            {
                queryBuilder.Append($"DELETE {actDSTableElement.DSTableName} where GINGER_USED =\"False\"");
            }

            else if (actDSTableElement.ByRowNum)
            {
                queryBuilder.Append($"DELETE {actDSTableElement.DSTableName} where");
            }

        }
        public static void SetWhereConditions(ActDSTableElement actDSTableElement, StringBuilder queryBuilder)
        {
            if (actDSTableElement.WhereConditions == null)
            {
                return;
            }

            for (int i = 0; i < actDSTableElement.WhereConditions.Count; i++)
            {
                string wQuery = "";
                string wCond = actDSTableElement.WhereConditions[i].wCondition.ToString().ToLower();
                string wColVal = actDSTableElement.WhereConditions[i].wTableColumn.ToString().Trim();
                string wOpr = actDSTableElement.WhereConditions[i].wOperator.ToString();
                string wRowVal = actDSTableElement.WhereConditions[i].wValue.Replace("\"", string.Empty);

                if (wCond == "empty")
                {
                    wCond = "";
                }

                if (wOpr == "Equals")
                {
                    if (wColVal == "GINGER_ID")
                    {
                        wQuery = wQuery + " " + wCond + " " + wColVal + " = " + wRowVal;
                    }
                    else
                    {
                        wQuery = wQuery + " " + wCond + " " + wColVal + " = \"" + wRowVal + "\"";
                    }
                }
                else if (wOpr == "NotEquals")
                {
                    if (wColVal == "GINGER_ID")
                    {
                        wQuery = wQuery + " " + wCond + " " + wColVal + " != " + wRowVal;
                    }
                    else
                    {
                        wQuery = wQuery + " " + wCond + " " + wColVal + " !=  \"" + wRowVal + "\"";
                    }
                }
                else if (wOpr == "Contains")
                {
                    wQuery = wQuery + " " + wCond + " " + wColVal + " contains " + "\"" + wRowVal + "\"";
                }
                else if (wOpr == "StartsWith")
                {
                    wQuery = wQuery + " " + wCond + " " + wColVal + " like " + "\"" + wRowVal + "\"";
                }
                queryBuilder.Append(wQuery);
            }

        }
    }

}
