#region License
/*
Copyright © 2014-2025 European Support Limited

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
using GingerCore.Actions;
using GingerCore.DataSource;
using LiteDB;
using LiteDB.Engine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using static GingerCore.Actions.ActDSTableElement;

namespace GingerCoreNET.DataSource
{
    public class GingerLiteDB : DataSourceBase
    {
        int count = 1;
        private ConnectionString ConnectionString
        {
            get
            {
                return new ConnectionString()
                {
                    Filename = FileFullPath,
                    Connection = ConnectionType.Shared
                };
            }
        }

        public override string FileFullPath
        {
            get => base.FileFullPath;
            set
            {
                base.FileFullPath = value;
                TryUpgradeDataFile();
            }
        }

        private static readonly AutoResetEvent UpgradeDataFileSyncEvent = new(true);

        public GingerLiteDB()
        {
            if (!string.IsNullOrEmpty(ConnectionString.Filename))
            {
                TryUpgradeDataFile();
            }
        }


        private bool TryUpgradeDataFile()
        {
            try
            {
                UpgradeDataFileSyncEvent.WaitOne();
                string dbFilePath = ConnectionString.Filename;
                return LiteEngine.Upgrade(dbFilePath);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while trying to upgrade data file.", ex);
                return false;
            }
            finally
            {
                UpgradeDataFileSyncEvent.Set();
            }
        }

        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            try
            {
                using var db = new LiteDatabase(ConnectionString);
                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
                var table = db.GetCollection(tableName);
                foreach (var doc in results)
                {
                    doc.Add(columnName, "");
                    table.Update(doc);
                }
            }
            catch (Exception)
            {
                Reporter.ToUser(eUserMsgKey.StaticErrorMessage, "Please enter valid column name");
            }
        }

        public override void AddTable(string tableName, string columnList = "")
        {
            using var db = new LiteDatabase(ConnectionString);
            var table = db.GetCollection(tableName);

            string[] List = columnList.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var doc = new BsonDocument();
            if (columnList.Contains("KEY_VAL"))
            {
                doc[List[0]] = 1;
                doc[List[1]] = "";
                doc[List[2]] = "";
                doc[List[3]] = "";
                doc[List[4]] = DateTime.Now.ToString();

            }
            else
            {
                doc[List[0]] = 1;
                doc[List[1]] = "False";
                doc[List[2]] = "";
                doc[List[3]] = DateTime.Now.ToString();
            }
            table.Insert(doc);
        }

        public override string CopyTable(string tableName)
        {
            string CopyTableName = tableName;

            while (IsTableExist(CopyTableName))
            {
                CopyTableName = CopyTableName + "_Copy";
            }
            if (CopyTableName != tableName)
            {
                using var db = new LiteDatabase(ConnectionString);
                var CopyTable = db.GetCollection(CopyTableName);
                var table = db.GetCollection(tableName);

                DataTable dtChange = new DataTable(CopyTableName);
                dtChange = datatable(table.FindAll(), CopyTableName);
                SaveTable(dtChange, CopyTable);
            }

            return CopyTableName;
        }

        public DataTable datatable(IEnumerable<BsonDocument> results, string CopyTableName = null)
        {
            if (CopyTableName == null)
            {
                CopyTableName = results.ToString();
            }
            var dt = new LiteDataTable(CopyTableName);
            foreach (var doc in results)
            {
                if (dt.NewRow() is LiteDataRow dr)
                {
                    dr.UnderlyingValue = doc;
                    foreach (var property in doc.RawValue)
                    {
                        if (!property.Value.IsMaxValue && !property.Value.IsMinValue)
                        {
                            if (!dt.Columns.Contains(property.Key))
                            {
                                dt.Columns.Add(property.Key, typeof(string));

                            }
                            SetDataRow(dr, property);
                            string ads = property.Key.ToString();
                            if (ads == "GINGER_ID")
                            {
                                if (property.Value.ToString() == "")
                                {
                                    dr[property.Key] = count.ToString();
                                    count++;
                                }
                            }
                            else
                            {
                                dr[property.Key] = property.Value.RawValue.ToString();
                            }
                            if (ads == "GINGER_USED")
                            {
                                if (property.Value.RawValue.ToString() == "")
                                {
                                    dr[property.Key] = "False";
                                }
                            }
                        }
                    }
                    dt.Rows.Add(dr.ItemArray);
                }
            }
            // converting Litedatatable to Datatable
            DataTable dttable = dt;
            dttable.Rows.Add(dt.Rows);
            dttable.TableName = CopyTableName;
            return dttable;
        }

        public override void DeleteTable(string tableName)
        {
            using var db = new LiteDatabase(ConnectionString);
            db.DropCollection(tableName);
        }

        public override bool ExporttoExcel(string TableName, string sExcelPath, string sSheetName, string sTableQueryValue = "")
        {
            var dataTable = GetTable(TableName);

            if (!string.IsNullOrEmpty(sTableQueryValue))
            {
                dataTable = GetQueryOutput(TableName);
                var whereCond = string.Empty;
                var selectedColumn = string.Empty;
                if (sTableQueryValue.Contains(" where "))
                {
                    var index = sTableQueryValue.IndexOf(" where ");
                    whereCond = sTableQueryValue[index..];

                    selectedColumn = sTableQueryValue[..index];

                    var filter = whereCond.Remove(0, 6).Trim();
                    if (filter.Contains("\""))
                    {
                        filter = filter.Replace("\"", "'");
                    }
                    DataView dv = new DataView(dataTable)
                    {
                        RowFilter = filter
                    };
                    dataTable = dv.ToTable();

                    dataTable = dataTable.DefaultView.ToTable(false, selectedColumn.Split(','));
                }
                else
                {
                    dataTable = dataTable.DefaultView.ToTable(false, sTableQueryValue.Trim().Split(','));
                }

            }
            else
            {
                dataTable.Columns.Remove("_id");
            }

            return GingerCoreNET.GeneralLib.General.ExportToExcel(dataTable, sExcelPath, sSheetName);
        }

        public override List<string> GetColumnList(string tableName)
        {
            List<string> mColumnNames = [];
            using (var db = new LiteDatabase(ConnectionString))
            {
                if (tableName == "")
                { return mColumnNames; }

                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
                mColumnNames = GetColumnList(results, tableName);
                //var name = mColumnNames.RemoveAll(i => i.Contains("Name")); Commented this as we are not able to see columnNames which contain "Name" keyword in it.

            }
            return mColumnNames;
        }

        private List<string> GetColumnList(IList<BsonDocument> results, string tableName)
        {
            var dt = new LiteDataTable(results.ToString());
            List<string> mColumnNames = [];

            foreach (var doc in results)
            {
                if (dt.NewRow() is LiteDataRow dr)
                {
                    dr.UnderlyingValue = doc;
                    foreach (var property in doc.RawValue)
                    {
                        if (!property.Value.IsMaxValue && !property.Value.IsMinValue)
                        {
                            if (!dt.Columns.Contains(property.Key))
                            {
                                dt.Columns.Add(property.Key, typeof(string));
                                mColumnNames.Add(dt.Columns.ToString());
                            }

                            SetDataRow(dr, property);
                            string ads = property.Key.ToString();
                            if (ads == "GINGER_ID")
                            {
                                if (property.Value.ToString() == "")
                                {
                                    dr[property.Key] = count.ToString();
                                    count++;
                                }
                            }
                            else
                            {
                                dr[property.Key] = property.Value.RawValue == null ? string.Empty : property.Value.RawValue.ToString();
                            }
                        }
                    }
                    dt.Rows.Add(dr.ItemArray);
                }
            }
            DataTable aa = dt;
            aa.TableName = tableName;

            foreach (DataColumn column in aa.Columns)
            {
                if (column.ColumnName != "System.Data.DataColumnCollection")
                {
                    mColumnNames.Add(column.ToString());
                }
            }

            var itemToRemove = mColumnNames.RemoveAll(x => x.Contains("System.Data.DataColumnCollection"));
            var s = mColumnNames.RemoveAll(a => a.Contains("_id"));
            return mColumnNames;
        }


        public String GetQueryOutput(string query, string col, int rownumber, bool mark = false, string DSTableName = null)
        {
            string returnVal = string.Empty;
            DataTable datatble = GetQueryOutput(query);
            if (datatble != null && datatble.Rows.Count > 0)
            {
                DataRow row = datatble.Rows[rownumber];

                if (mark)
                {
                    string rowID = row["GINGER_ID"].ToString();
                    GetQueryOutput($"UPDATE {DSTableName} SET GINGER_USED = \"True\" where GINGER_ID = {rowID}");
                }
                returnVal = Convert.ToString(row[col]);
            }
            else
            {
                returnVal = "No rows found";
            }
            return returnVal;
        }

        public override DataTable GetQueryOutput(string query)
        {
            List<string> mColumnNames = [];
            DataTable dataTable = new DataTable();
            bool duplicate = false;
            using (var db = new LiteDatabase(ConnectionString))
            {
                var results = db.GetCollection(query).Find(Query.All(), 0).ToList();
                try
                {
                    if (results.Count > 0)
                    {
                        var result = db.GetCollection<BsonDocument>(query);

                        var dt = new LiteDataTable(results.ToString());
                        foreach (var doc in results)
                        {
                            if (dt.NewRow() is LiteDataRow dr)
                            {
                                dr.UnderlyingValue = doc;
                                foreach (var property in doc.RawValue)
                                {
                                    if (!property.Value.IsMaxValue && !property.Value.IsMinValue)
                                    {
                                        if (!dt.Columns.Contains(property.Key))
                                        {
                                            dt.Columns.Add(property.Key, typeof(string));
                                        }
                                        if (property.Value.RawValue != null)
                                        {
                                            if (property.Value.RawValue.ToString() is "System.Collections.Generic.Dictionary`2[System.String,BsonValue]" or "System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]")
                                            {
                                                dr[property.Key] = "";
                                            }
                                            else if (property.Value.RawValue.ToString() is "System.Data.DataRowCollection" or "System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]")
                                            {
                                                duplicate = true;
                                            }
                                            else
                                            {
                                                dr[property.Key] = property.Value.RawValue.ToString();
                                            }
                                        }
                                        else
                                        {
                                            dr[property.Key] = string.Empty;

                                        }
                                    }
                                }
                                dt.Rows.Add(dr);
                            }
                        }

                        DataTable aa = dt;
                        bool dosort = true;
                        if (duplicate)
                        {
                            dt.Rows.RemoveAt(dt.Rows.Count - 1);
                        }
                        DataTable dt2 = dt.Clone();
                        dt2.Columns["GINGER_ID"].DataType = Type.GetType("System.Int32");

                        foreach (DataRow dr in dt.Rows)
                        {
                            if (Convert.ToString(dr["GINGER_ID"]) == "")
                            {
                                dosort = false;
                            }
                            else
                            {
                                dt2.ImportRow(dr);
                            }
                        }
                        if (dosort)
                        {
                            dt2.AcceptChanges();
                            DataView dv = dt2.DefaultView;
                            dv.Sort = "GINGER_ID ASC";

                            aa = dv.ToTable();
                        }
                        else
                        {
                            aa.Rows.RemoveAt(0);
                        }

                        aa.TableName = query;
                        dataTable = aa;
                    }
                    else
                    {
                        // If we need to run a direct query
                        try
                        {
                            // Converting BSON to JSON 
                            JArray array = [];
                            // This query needs SQL Command
                            BsonValue[] result = db.Execute(query).ToArray();
                            foreach (BsonValue bs in result)
                            {
                                string js = LiteDB.JsonSerializer.Serialize(bs);
                                if (js is "0" or "1")
                                {
                                    return dataTable;
                                }
                                JObject jo = JObject.Parse(js);
                                JObject jo2 = [];
                                foreach (JToken jt in jo.Children())
                                {
                                    if ((jt as JProperty).Name != "_id")
                                    {
                                        string sData = jt.ToString();
                                        Regex regex = new Regex(@": {(\r|\n| )*""_type"": ""System.DBNull*");
                                        Match match = regex.Match(sData);
                                        if (match.Success)
                                        {
                                            if (jt.HasValues)
                                            {
                                                string name = (jt as JProperty).Name;
                                                var aa = jt as JProperty;
                                                aa.Value = "";
                                            }
                                            jo2.Add(jt);
                                        }
                                        else
                                        {
                                            jo2.Add(jt);
                                        }
                                    }
                                }
                                array.Add(jo2);
                            }
                            // JSON to Datatable
                            dataTable = JsonConvert.DeserializeObject<DataTable>(array.ToString());
                            if (dataTable != null && dataTable.Columns.Count > 0)
                            {
                                DataTable dt = dataTable;
                                bool dosort = true;
                                DataTable dt2 = dataTable.Clone();
                                foreach (DataRow dr in dataTable.Rows)
                                {
                                    if (Convert.ToString(dr["GINGER_ID"]) == "")
                                    {
                                        dosort = false;
                                    }
                                    else
                                    {
                                        dt2.ImportRow(dr);
                                    }
                                }
                                if (dosort)
                                {
                                    dt2.AcceptChanges();
                                    DataView dv = dt2.DefaultView;
                                    dv.Sort = "GINGER_ID ASC";

                                    dt = dv.ToTable();
                                }
                                else
                                {
                                    dt.Rows.RemoveAt(0);
                                }
                                dt.TableName = query;
                                dataTable = dt;
                                var json = JsonConvert.SerializeObject(array);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.WARN, "Exception Occurred while doing LiteDB GetQueryOutput", ex);
                            db.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception Occurred while doing LiteDB GetQueryOutput\n" + ex.StackTrace, ex);
                    db.Dispose();

                }
            }
            dataTable.AcceptChanges();
            return dataTable;
        }



        private DataSourceTable CheckDSTableDesign(DataTable dtTable)
        {
            string tablename = dtTable.TableName;
            DataSourceTable sTableDetail = new DataSourceTable
            {
                Name = tablename
            };

            int iCount = 0;
            int iIdCount = 0;
            int iUpdateCount = 0;
            foreach (DataColumn column in dtTable.Columns)
            {

                if (column.ToString() is "GINGER_KEY_NAME" or "GINGER_KEY_VALUE")
                { iCount++; }
                else if (column.ToString() == "GINGER_ID")
                { iIdCount++; }
                else if (column.ToString() is "GINGER_LAST_UPDATE_DATETIME" or "GINGER_LAST_UPDATED_BY")
                { iUpdateCount++; }
            }
            if (iCount == 2 && dtTable.Columns.Count == 3 + iIdCount + iUpdateCount)
            { sTableDetail.DSTableType = DataSourceTable.eDSTableType.GingerKeyValue; }
            else
            { sTableDetail.DSTableType = DataSourceTable.eDSTableType.Customized; }

            sTableDetail.DSC = this;
            return sTableDetail;
        }

        public override ObservableList<DataSourceTable> GetTablesList()
        {
            DataTable Datatable = new DataTable();
            ObservableList<DataSourceTable> mDataSourceTableDetails = [];
            using (var db = new LiteDatabase(ConnectionString))
            {
                IEnumerable<string> Tables = db.GetCollectionNames();
                foreach (string table in Tables)
                {
                    var results = db.GetCollection(table).Find(Query.All(), 0).ToList();
                    var dt = new LiteDataTable(results.ToString());
                    foreach (var doc in results)
                    {
                        if (dt.NewRow() is LiteDataRow dr)
                        {
                            dr.UnderlyingValue = doc;
                            foreach (var property in doc.RawValue)
                            {
                                if (!property.Value.IsMaxValue && !property.Value.IsMinValue)
                                {
                                    if (!dt.Columns.Contains(property.Key))
                                    {
                                        dt.Columns.Add(property.Key, typeof(string));
                                    }
                                    dr[property.Key] = property.Value.RawValue == null ? string.Empty : property.Value.RawValue.ToString();
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                        Datatable = dt;
                        Datatable.TableName = table;
                    }
                    mDataSourceTableDetails.Add(CheckDSTableDesign(Datatable));
                }
            }
            return mDataSourceTableDetails;
        }
        public override bool IsTableExist(string tableName)
        {
            using var db = new LiteDatabase(ConnectionString);
            return db.CollectionExists(tableName);
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            using var db = new LiteDatabase(ConnectionString);
            var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
            var table = db.GetCollection(tableName);
            foreach (var doc in results)
            {
                doc.Remove(columnName);
                table.Update(doc);
            }
        }

        public override void RenameTable(string tableName, string newTableName)
        {
            bool renameSuccess = false;
            bool tableExist = false;
            try
            {
                using (var db = new LiteDatabase(ConnectionString))
                {
                    tableExist = db.CollectionExists(newTableName);
                    if (!tableExist)
                    {
                        renameSuccess = db.RenameCollection(tableName, newTableName);
                    }
                }
                if (renameSuccess)
                {
                    this.UpdateDSNameChangeInItem(this, tableName, newTableName, ref renameSuccess);
                }
                else if (tableExist)
                {
                    Reporter.ToUser(eUserMsgKey.DbTableNameError, newTableName);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Error occurred while renaming the table {tableName}", ex);
            }
        }

        public override bool RunQuery(string query)
        {
            using LiteDatabase db = new LiteDatabase(ConnectionString);
            // SQL Command needed here:
            var result = db.Execute(query);

            return true;
        }

        public void RunQuery(string query, int LocateRowValue, string DSTableName, bool MarkUpdate = false, bool NextAvai = false)
        {
            //db." + DSTableName + ".find"
            DataTable dt = GetQueryOutput($"SELECT $ FROM {DSTableName}");
            if (dt.Rows.Count == 0)
            {
                throw new Exception($"No row found in the datasource table: {DSTableName}");                
            }
            DataRow row = dt.Rows[LocateRowValue];
            string rowValue = Convert.ToString(row["GINGER_ID"]);

            //Rownumber
            if (!query.Contains("where") && !NextAvai)
            {
                GetQueryOutput(query + " where GINGER_ID= " + rowValue);
            }
            //Nextavailable
            else if (NextAvai)
            {
                //"db." + DSTableName + ".find 
                DataTable datatble = GetQueryOutput($"SELECT $ FROM {DSTableName} WHERE GINGER_USED = \"False\" LIMIT 1");
                if (datatble != null && datatble.Columns.Count > 0)
                {
                    row = datatble.Rows[LocateRowValue];
                    string rowID = Convert.ToString(row["GINGER_ID"]);

                    string[] Stringsplit = query.Split(new[] { "where " }, StringSplitOptions.None);
                    query = Stringsplit[0] + " where GINGER_ID = " + rowID;
                    GetQueryOutput(query);

                    if (MarkUpdate)
                    {
                        // "db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_USED =\"False\" and GINGER_ID= " + rowID
                        GetQueryOutput($"UPDATE {DSTableName} SET GINGER_USED = \"True\" WHERE GINGER_USED =\"False\" and GINGER_ID= {rowID}");
                    }
                }
                return;
            }
            // Where Condition
            else
            {
                if (query.Contains("where") && query.Contains("GINGER_ID ="))
                {
                    RunQuery(query);
                    string[] querysplit = query.Split(new[] { "GINGER_ID =" }, StringSplitOptions.None);
                    //"db." + DSTableName + ".find GINGER_ID=" + querysplit[1]
                    dt = GetQueryOutput($"SELECT $ FROM {DSTableName} WHERE GINGER_ID = {querysplit[1]}");

                    row = dt.Rows[0];
                }
                else if (query.Contains("where"))
                {
                    GetQueryOutput(query);
                }
            }
            if (MarkUpdate)
            {
                string rowID = Convert.ToString(row["GINGER_ID"]);
                // "db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= " + rowID
                GetQueryOutput($"UPDATE {DSTableName} SET GINGER_USED =  \"True\" WHERE GINGER_ID= {rowID} ");
            }

        }

        public string GetResultString(string query)
        {
            string result = null;
            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                // SQL query needed here
                var resultdxs = db.Execute(query).ToArray();
                foreach (BsonValue bs in resultdxs)
                {
                    BsonDocument aa = bs.AsDocument;
                    foreach (KeyValuePair<string, BsonValue> keyval in aa.RawValue)
                    {
                        result = keyval.Value.RawValue.ToString();
                    }
                }
            }
            return result;
        }

        public object GetResult(string query)
        {
            object result = null;
            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                //SQL command needed here

                var resultdxs = db.Execute(query).ToArray();
                foreach (BsonValue bs in resultdxs)
                {
                    result = bs.RawValue;
                }
            }
            return result;
        }


        public string GetResut(string query, string DSTableName, bool MarkUpdate)
        {
            DataTable dt = GetQueryOutput(query);
            dt.TableName = DSTableName;
            DataRow row = dt.Rows[0];

            if (MarkUpdate)
            {
                string[] tokens = query.Split(new[] { "where" }, StringSplitOptions.None);
                string Newquery = $"UPDATE {DSTableName} SET GINGER_USED = \"True\" WHERE {tokens[1]}";
                RunQuery(Newquery);
            }
            return row[0].ToString();
        }

        public override void SaveTable(DataTable dataTable)
        {
            if (isDeleteAllExecuted)
            {
                DeleteDBTableContents(dataTable.TableName);
                dataTable.AcceptChanges();
            }

            using LiteDatabase db = new LiteDatabase(ConnectionString);
            dataTable.DefaultView.Sort = "GINGER_ID";
            var table = db.GetCollection(dataTable.ToString());
            SaveTable(dataTable, table);
        }
        private void SaveTable(DataTable dataTable, ILiteCollection<BsonDocument> table)
        {
            var doc = BsonMapper.Global.ToDocument(table);

            DataTable changed = dataTable.GetChanges();
            DataTable dtChange = dataTable;
            //if datatable is empty
            if (dtChange.Rows.Count == 0 && changed == null)
            {
                return;
            }

            table.DeleteAll();

            List<BsonDocument> batch = [];
            if ((dtChange != null))
            {
                foreach (DataRow dr in dtChange.Rows)
                {
                    if (dr.RowState != DataRowState.Deleted)//Commit after row is deleted 
                    {
                        if (dr.RowState.Equals(DataRowState.Added) || dr.RowState.Equals(DataRowState.Modified))
                        {
                            dr["GINGER_LAST_UPDATED_BY"] = System.Environment.UserName;
                            dr["GINGER_LAST_UPDATE_DATETIME"] = DateTime.Now.ToString();
                        }

                        if (dr["GINGER_ID"] != null || string.IsNullOrWhiteSpace((Convert.ToString(dr["GINGER_ID"]))))
                        {
                            dr["GINGER_ID"] = dtChange.Rows.IndexOf(dr) + 1;
                        }

                        var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);

                        var mapper = new BsonMapper();
                        var sd = mapper.ToDocument(dictionary);
                        batch.Add(new BsonDocument(sd));
                        table.Upsert(batch);
                    }
                }
            }
            dtChange.AcceptChanges();
            var result = table.Find(Query.All()).ToList();

            if (dataTable.Rows.Count > result.Count)
            {
                table.Upsert(batch);
            }

        }
        public override void UpdateTableList(ObservableList<DataSourceTable> dsTableList)
        {
            throw new NotImplementedException();
        }

        public override DataTable GetTable(string TableName)
        {
            return GetQueryOutput(TableName);
        }

        public override void AddRow(List<string> mColumnNames, DataSourceTable mDSTableDetails)
        {
            DataRow dr = mDSTableDetails.DataTable.NewRow();
            dr[0] = Guid.NewGuid();
            mColumnNames = this.GetColumnList(mDSTableDetails.Name);
            foreach (string sColName in mColumnNames)
            {
                if (sColName is not "GINGER_ID" and not "GINGER_LAST_UPDATED_BY" and not "GINGER_LAST_UPDATE_DATETIME")
                {
                    if (sColName == "GINGER_USED")
                    {
                        dr[sColName] = "False";
                    }
                }
                else if (sColName == "GINGER_ID")
                {
                    int count = mDSTableDetails.DataTable.Rows.Count;
                    dr[sColName] = count + 1;
                }
            }
            mDSTableDetails.DataTable.Rows.Add(dr);
            mDSTableDetails.DataTable.DefaultView.Sort = "GINGER_ID";
        }

        public override void DuplicateRow(List<string> mColumnNames, List<object> SelectedItemsList, DataSourceTable mDSTableDetails)
        {
            mColumnNames = this.GetColumnList(mDSTableDetails.Name);
            foreach (object o in SelectedItemsList)
            {
                DataRow row = (((DataRowView)o).Row);
                DataRow dr = mDSTableDetails.DataTable.NewRow();
                dr[0] = Guid.NewGuid();

                foreach (string sColName in mColumnNames)
                {
                    if (sColName is not "GINGER_ID" and not "GINGER_LAST_UPDATED_BY" and not "GINGER_LAST_UPDATE_DATETIME")
                    {
                        if (sColName == "GINGER_USED")
                        {
                            object a = row[sColName].GetType();
                            if (a.ToString().Contains("System.DBNull"))
                            {
                                dr[sColName] = "False";
                            }
                            else
                            {
                                dr[sColName] = row[sColName];
                            }
                        }
                        else
                        {
                            dr[sColName] = row[sColName];
                        }
                    }
                    else
                    {
                        if (sColName == "GINGER_ID")
                        {
                            int count = mDSTableDetails.DataTable.Rows.Count;
                            dr[sColName] = count + 1;
                        }
                        else
                        {
                            dr[sColName] = row[sColName];
                        }
                    }
                }
                mDSTableDetails.DataTable.Rows.Add(dr);
            }
        }

        public void Execute(ActDSTableElement actDSTable, string Query)
        {
            int DSCondition = 0;
            if (actDSTable.ActDSConditions != null)
            {
                DSCondition = actDSTable.ActDSConditions.Count;
            }
            DataTable dt = new DataTable();

            switch (actDSTable.ControlAction)
            {
                case ActDSTableElement.eControlAction.GetValue:
                    // Customized Query
                    if (actDSTable.Customized)
                    {
                        string col = actDSTable.LocateColTitle;
                        bool nextavail = actDSTable.ByNextAvailable;
                        if (actDSTable.IsKeyValueTable)
                        {
                            string result = GetResultString(Query);
                            actDSTable.AddOrUpdateReturnParamActual("Output", result);
                        }
                        else if (nextavail)
                        {
                            string op = GetQueryOutput(Query, col, 0, actDSTable.MarkUpdate, actDSTable.DSTableName);
                            actDSTable.AddOrUpdateReturnParamActual(col, op);
                        }
                        else if (actDSTable.ByRowNum)
                        {
                            string op = GetQueryOutput(Query, col, Int32.Parse(actDSTable.LocateRowValue), actDSTable.MarkUpdate, actDSTable.DSTableName);
                            actDSTable.AddOrUpdateReturnParamActual(col, op);
                        }
                        else
                        {
                            string op = GetQueryOutput(Query, col, 0, actDSTable.MarkUpdate, actDSTable.DSTableName);
                            actDSTable.AddOrUpdateReturnParamActual(col, op);
                        }
                    }
                    // By Query given by User
                    else if (actDSTable.ByQuery)
                    {
                        dt = GetQueryOutput(Query);
                        foreach (DataRow row in dt.Rows)
                        {
                            foreach (DataColumn colunm in dt.Columns)
                            {
                                actDSTable.AddOrUpdateReturnParamActual(colunm.ToString(), row[colunm].ToString());
                            }
                        }
                    }
                    // By Selected Cell
                    else
                    {
                        string[] tokens = Query.Split(new[] { "where" }, StringSplitOptions.None);

                        dt = GetQueryOutput($"SELECT $ FROM {actDSTable.DSTableName} where {tokens[1]}");
                        dt.TableName = actDSTable.DSTableName;
                        DataRow row = dt.Rows[0];

                        if (actDSTable.MarkUpdate)
                        {
                            string rowID = Convert.ToString(row["GINGER_ID"]);
                            RunQuery($"UPDATE {actDSTable.DSTableName} SET GINGER_USED = \"True\" WHERE GINGER_ID= \"{rowID}\"");
                        }
                        actDSTable.AddOrUpdateReturnParamActual(actDSTable.VarName, row[0].ToString());
                    }
                    break;

                case eControlAction.MarkAsDone:
                case eControlAction.SetValue:
                    if (actDSTable.IsKeyValueTable)
                    {
                        RunQuery(Query);
                    }
                    //By Selected Cell
                    if (actDSTable.BySelectedCell)
                    {
                        string[] tokens = Query.ToLower().Split(new[] { "where" }, StringSplitOptions.None);
                        string updateParamValue = tokens[0][(tokens[0].LastIndexOf(".") + 1)..] + "= \"" + actDSTable.Value + "\" where";
                        RunQuery($"UPDATE {actDSTable.DSTableName} SET {updateParamValue} {tokens[1]}");
                    }
                    // Customized Query
                    else if (actDSTable.Customized)
                    {
                        if (actDSTable.ByRowNum)
                        {
                            RunQuery(Query, Int32.Parse(actDSTable.LocateRowValue), actDSTable.DSTableName, actDSTable.MarkUpdate);
                        }
                        else if (actDSTable.ByNextAvailable)
                        {
                            RunQuery(Query, 0, actDSTable.DSTableName, actDSTable.MarkUpdate, true);
                        }
                        else
                        {
                            RunQuery(Query, 0, actDSTable.DSTableName, actDSTable.MarkUpdate);
                        }
                    }
                    // By Query given by User
                    else
                    {
                        RunQuery(Query);
                    }
                    actDSTable.AddOrUpdateReturnParamActual("Output", "Success");

                    break;

                case eControlAction.MarkAllUsed:
                case eControlAction.MarkAllUnUsed:
                    var aa = GetResult(Query);
                    actDSTable.AddOrUpdateReturnParamActual("Result", aa.ToString());
                    break;

                case eControlAction.RowCount:
                    var a = GetRowCount(actDSTable.DSTableName);
                    actDSTable.AddOrUpdateReturnParamActual("Count", a.ToString());
                    break;
                case eControlAction.AvailableRowCount:
                    dt = GetQueryOutput(Query);

                    actDSTable.AddOrUpdateReturnParamActual("Count", dt.Rows.Count.ToString());
                    break;
                case eControlAction.ExportToExcel:
                    if (actDSTable.ExcelConfig != null)
                    {
                        ExporttoExcel(actDSTable.DSTableName, actDSTable.ExcelConfig.ExcelPath, actDSTable.ExcelConfig.ExcelSheetName, actDSTable.ExcelConfig.ExportQueryValue);
                    }
                    else
                    {
                        string[] token = Query.Split(new[] { "," }, StringSplitOptions.None);
                        ExporttoExcel(actDSTable.DSTableName, token[0], token[1]);
                    }

                    break;
                case eControlAction.DeleteRow:
                    if (actDSTable.IsKeyValueTable)
                    {
                        RunQuery(Query);
                        actDSTable.AddOrUpdateReturnParamActual("Deleted Record", "1");
                    }
                    //By Selected Cell
                    else if (actDSTable.BySelectedCell)
                    {
                        string[] tokens = Query.ToLower().Split(new[] { "where" }, StringSplitOptions.None);
                        //"db." + actDSTable.DSTableName + ".delete " + tokens[1]
                        RunQuery($"DELETE {actDSTable.DSTableName} WHERE {tokens[1]}");
                    }
                    // Customized Query
                    else if (actDSTable.Customized)
                    {
                        if (actDSTable.ByRowNum)
                        {
                            dt = GetQueryOutput($"Select $ FROM {actDSTable.DSTableName}");
                            int x = Int32.Parse(actDSTable.LocateRowValue);
                            DataRow row = dt.Rows[x];
                            string rowValue = Convert.ToString(row["GINGER_ID"]);
                            RunQuery(Query + " GINGER_ID = " + rowValue);
                            actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                        }
                        else if (actDSTable.ByNextAvailable)
                        {
                            dt = GetQueryOutput($"SELECT $ FROM {actDSTable.DSTableName} WHERE GINGER_USED=\"False\"");
                            DataRow row = dt.Rows[0];
                            string rowValue = Convert.ToString(row["GINGER_ID"]);
                            GetResult($"DELETE {actDSTable.DSTableName} WHERE GINGER_ID = {rowValue}");
                            actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                        }
                        else
                        {
                            GetResult(Query);
                            actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                        }
                    }
                    // By Query given by User
                    else
                    {
                        GetResult(Query);
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    break;
                case eControlAction.DeleteAll:
                    DataTable dtCurrent = GetQueryOutput(actDSTable.DSTableName);
                    isDeleteAllExecuted = true;
                    dtCurrent.Rows.Clear();
                    SaveTable(dtCurrent);

                    actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    break;

                case eControlAction.AddRow:
                    DataSourceTable DSTable = null;
                    ObservableList<DataSourceTable> dstTables = GetTablesList();
                    foreach (DataSourceTable dst in dstTables)
                    {
                        if (dst.Name == actDSTable.DSTableName)
                        {
                            DSTable = dst;
                            DSTable.DataTable = dst.DSC.GetTable(actDSTable.DSTableName);
                            break;
                        }
                    }
                    List<string> mColumnNames = GetColumnList(actDSTable.DSTableName);
                    AddRow(mColumnNames, DSTable);
                    SaveTable(DSTable.DataTable);
                    //Get GingerId
                    dt = GetTable(actDSTable.DSTableName);
                    DataRow dr = dt.Rows[^1];
                    string GingerId = Convert.ToString(dr["GINGER_ID"]);
                    actDSTable.AddOrUpdateReturnParamActual("GINGER_ID", GingerId);
                    break;
                default:

                    break;
            }
            return;
        }

        public override void InitConnection()
        {
            if (FilePath != null)
            {
                FileFullPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(FilePath);
            }
        }

        public override string AddNewCustomizedTableQuery()
        {
            return "GINGER_ID,GINGER_USED,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME";
        }

        public override int GetRowCount(string TableName)
        {
            return GetQueryOutput($"SELECT $ FROM {TableName}").Rows.Count;
        }

        public override string AddNewKeyValueTableQuery()
        {
            return "GINGER_ID, GINGER_KEY_NAME, GINGER_KEY_VALUE, GINGER_LAST_UPDATED_BY, GINGER_LAST_UPDATE_DATETIME";
        }

        public override string GetExtension()
        {
            return ".db";
        }
        public override DataTable GetKeyName(string mDSTableName)
        {
            return GetQueryOutput($"SELECT GINGER_KEY_NAME FROM {mDSTableName} WHERE GINGER_KEY_NAME != NULL");
        }

        bool isDeleteAllExecuted = false;
        public override void DeleteAll(List<object> AllItemsList, string TName = null)
        {
            foreach (object o in AllItemsList)
            {
                ((DataRowView)o).Delete();
            }

            isDeleteAllExecuted = true;
        }

        public void DeleteDBTableContents(string TName)
        {
            using (LiteDatabase db = new LiteDatabase(ConnectionString))
            {
                var table = db.GetCollection(TName);

                List<string> ColumnList = GetColumnList(table.FindAll().ToList(), TName);
                table.DeleteAll();
                List<BsonDocument> batch = [];

                var doc = new BsonDocument();

                for (int i = 0; i < ColumnList.Count; i++)
                {
                    doc[ColumnList[i]] = "";
                }
                table.Insert(doc);

            }

            isDeleteAllExecuted = false;
        }

        public override string AddColumnName(string colName)
        {
            return colName;
        }

        public override string UpdateDSReturnValues(string Name, string sColList, string sColVals)
        {
            string[] collist = sColList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string[] vallist = sColVals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            string query = null;
            string colvalues = null;

            if (collist != null && vallist != null)
            {
                int i = collist.Length;
                if (collist.Length == vallist.Length)
                {
                    string[] items2 = vallist.Select(x => x.Replace("'", "\"")).ToArray();

                    for (int j = 0; j < i; j++)
                    {
                        colvalues = colvalues + collist[j] + ":" + items2[j] + ",";
                    }
                }
            }
            query = $"INSERT INTO {Name} VALUES {{ {colvalues} GINGER_LAST_UPDATED_BY: \"{System.Environment.UserName}\",GINGER_LAST_UPDATE_DATETIME:\"{DateTime.Now}\"}}";
            return query;
        }

        public static void SetDataRow(DataRow dr, KeyValuePair<string, BsonValue> property)
        {

            dr[property.Key] = property.Value.Type switch
            {
                BsonType.Null => "[NULL]",
                BsonType.Document => property.Value.AsDocument.RawValue.ContainsKey("_type")
                                        ? $"[OBJECT: {property.Value.AsDocument.RawValue["_type"]}]"
                                        : "[OBJECT]",
                BsonType.Array => $"[ARRAY({property.Value.AsArray.Count})]",
                BsonType.Binary => $"[BINARY({property.Value.AsBinary.Length})]",
                BsonType.DateTime => property.Value.AsDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                BsonType.String => property.Value.AsString,
                BsonType.Int32 or BsonType.Int64 => property.Value.AsInt64.ToString(),
                BsonType.Decimal or BsonType.Double => property.Value.AsDecimal.ToString(CultureInfo.InvariantCulture),
                _ => property.Value.RawValue == null ? string.Empty : property.Value.RawValue.ToString(),
            };
        }
    }
}




