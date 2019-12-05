#region License
/*
Copyright © 2014-2019 European Support Limited

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
using GingerCore.Actions;
using GingerCore.DataSource;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using static GingerCore.Actions.ActDSTableElement;

namespace GingerCoreNET.DataSource
{
    public class GingerLiteDB : DataSourceBase
    {
        int count = 1;
        private LiteDatabase _database;

        private LiteDatabase Database
        {
            get => _database;
            set
            {
                if (_database != value)
                {
                    _database = value;
                    FillDataGridView(null);
                }
            }
        }
        public void FillDataGridView(IEnumerable<BsonDocument> documents)
        {
            if (documents != null)
            {
                var dt = new LiteDataTable(documents.ToString());
                foreach (var doc in documents)
                {
                    var dr = dt.NewRow() as LiteDataRow;
                    if (dr != null)
                    {
                        dr.UnderlyingValue = doc;
                        foreach (var property in doc.RawValue)
                        {
                            if (!property.Value.IsMaxValue && !property.Value.IsMinValue)
                            {
                                if (!dt.Columns.Contains(property.Key))
                                {
                                    dt.Columns.Add(new DataColumn(property.Key, typeof(string)));
                                }
                                switch (property.Value.Type)
                                {
                                    case BsonType.Null:
                                        dr[property.Key] = "[NULL]";
                                        break;
                                    case BsonType.Document:
                                        dr[property.Key] = property.Value.AsDocument.RawValue.ContainsKey("_type")
                                            ? $"[OBJECT: {property.Value.AsDocument.RawValue["_type"]}]"
                                            : "[OBJECT]";
                                        break;
                                    case BsonType.Array:
                                        dr[property.Key] = $"[ARRAY({property.Value.AsArray.Count})]";
                                        break;
                                    case BsonType.Binary:
                                        dr[property.Key] = $"[BINARY({property.Value.AsBinary.Length})]";
                                        break;
                                    case BsonType.DateTime:
                                        dr[property.Key] = property.Value.AsDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        break;
                                    case BsonType.String:
                                        dr[property.Key] = property.Value.AsString;
                                        break;
                                    case BsonType.Int32:
                                    case BsonType.Int64:
                                        dr[property.Key] = property.Value.AsInt64.ToString();
                                        break;
                                    case BsonType.Decimal:
                                    case BsonType.Double:
                                        dr[property.Key] = property.Value.AsDecimal.ToString(CultureInfo.InvariantCulture);
                                        break;
                                    default:
                                        dr[property.Key] = property.Value.ToString();
                                        break;
                                }
                            }
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }
        }
        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            using (var db = new LiteDatabase(FileFullPath))
            {
                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
                var table = db.GetCollection(tableName);
                foreach (var doc in results)
                {
                    doc.Add(columnName, "");
                    table.Update(doc);
                }
            }
        }

        public override void AddTable(string tableName, string columnList = "")
        {
            using (var db = new LiteDatabase(FileFullPath))
            {
                var table = db.GetCollection(tableName);

                string[] List = columnList.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var doc = new BsonDocument();
                if (columnList.Contains("KEY_VAL"))
                {
                    doc[List[0]] = 1;
                    doc[List[1]] = "";
                    doc[List[2]] = "";
                    doc[List[3]] = DateTime.Now;
                    doc[List[4]] = "";

                }
                else
                {
                    doc[List[0]] = 1;
                    doc[List[1]] = "";
                    doc[List[2]] = DateTime.Now;
                    doc[List[3]] = "";
                }
                table.Insert(doc);
            }
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
                using (var db = new LiteDatabase(FileFullPath))
                {
                    var CopyTable = db.GetCollection(CopyTableName);
                    var table = db.GetCollection(tableName);

                    DataTable dtChange = new DataTable(CopyTableName);
                    dtChange = datatable(tableName, CopyTableName);
                    SaveTable(dtChange);
                }
            }

            return CopyTableName;
        }

        public DataTable datatable(string tableName, string CopyTableName = null)
        {
            using (var db = new LiteDatabase(FileFullPath))
            {
                var results = db.GetCollection(tableName).Find(Query.All(), 0);
                if (CopyTableName == null)
                {
                    CopyTableName = results.ToString();
                }
                var dt = new LiteDataTable(CopyTableName);
                foreach (var doc in results)
                {
                    var dr = dt.NewRow() as LiteDataRow;
                    if (dr != null)
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
                                switch (property.Value.Type)
                                {
                                    case BsonType.Null:
                                        dr[property.Key] = "[NULL]";
                                        break;
                                    case BsonType.Document:
                                        dr[property.Key] = property.Value.AsDocument.RawValue.ContainsKey("_type")
                                            ? $"[OBJECT: {property.Value.AsDocument.RawValue["_type"]}]"
                                            : "[OBJECT]";
                                        break;
                                    case BsonType.Array:
                                        dr[property.Key] = $"[ARRAY({property.Value.AsArray.Count})]";
                                        break;
                                    case BsonType.Binary:
                                        dr[property.Key] = $"[BINARY({property.Value.AsBinary.Length})]";
                                        break;
                                    case BsonType.DateTime:
                                        dr[property.Key] = property.Value.AsDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        break;
                                    case BsonType.String:
                                        dr[property.Key] = property.Value.AsString;
                                        break;
                                    case BsonType.Int32:
                                    case BsonType.Int64:
                                        dr[property.Key] = property.Value.AsInt64.ToString();
                                        break;
                                    case BsonType.Decimal:
                                    case BsonType.Double:
                                        dr[property.Key] = property.Value.AsDecimal.ToString(CultureInfo.InvariantCulture);
                                        break;
                                    default:
                                        dr[property.Key] = property.Value.ToString();
                                        break;
                                }
                                string ads = property.Key.ToString();
                                if (ads == "GINGER_ID")
                                {
                                    if (property.Value.AsString == "")
                                    {
                                        dr[property.Key] = count.ToString();
                                        count++;
                                    }
                                }
                                else
                                {
                                    dr[property.Key] = property.Value.AsString;
                                }
                                if (ads == "GINGER_USED")
                                {
                                    if (property.Value.AsString == "")
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
        }

        public override void DeleteTable(string tableName)
        {
            using (var db = new LiteDatabase(FileFullPath))
            {
                db.DropCollection(tableName);
            }
        }

        public override bool ExporttoExcel(string TableName, string sExcelPath, string sSheetName)
        {
            DataTable table = GetTable(TableName);
            table.Columns.Remove("_id");

            return GingerCoreNET.GeneralLib.General.ExportToExcel(table, sExcelPath, sSheetName);
        }

        public override List<string> GetColumnList(string tableName)
        {
            List<string> mColumnNames = new List<string>();

            using (var db = new LiteDatabase(FileFullPath))
            {
                if (tableName == "")
                { return mColumnNames; }

                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
                var dt = new LiteDataTable(results.ToString());
                foreach (var doc in results)
                {
                    var dr = dt.NewRow() as LiteDataRow;
                    if (dr != null)
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
                                switch (property.Value.Type)
                                {
                                    case BsonType.Null:
                                        dr[property.Key] = "[NULL]";
                                        break;
                                    case BsonType.Document:
                                        dr[property.Key] = property.Value.AsDocument.RawValue.ContainsKey("_type")
                                            ? $"[OBJECT: {property.Value.AsDocument.RawValue["_type"]}]"
                                            : "[OBJECT]";
                                        break;
                                    case BsonType.Array:
                                        dr[property.Key] = $"[ARRAY({property.Value.AsArray.Count})]";
                                        break;
                                    case BsonType.Binary:
                                        dr[property.Key] = $"[BINARY({property.Value.AsBinary.Length})]";
                                        break;
                                    case BsonType.DateTime:
                                        dr[property.Key] = property.Value.AsDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        break;
                                    case BsonType.String:
                                        dr[property.Key] = property.Value.AsString;
                                        break;
                                    case BsonType.Int32:
                                    case BsonType.Int64:
                                        dr[property.Key] = property.Value.AsInt64.ToString();
                                        break;
                                    case BsonType.Decimal:
                                    case BsonType.Double:
                                        dr[property.Key] = property.Value.AsDecimal.ToString(CultureInfo.InvariantCulture);
                                        break;
                                    default:
                                        dr[property.Key] = property.Value.ToString();
                                        break;
                                }
                                string ads = property.Key.ToString();
                                if (ads == "GINGER_ID")
                                {
                                    if (property.Value.AsString == "")
                                    {
                                        dr[property.Key] = count.ToString();
                                        count++;
                                    }

                                }
                                else
                                {
                                    dr[property.Key] = property.Value.AsString;
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
                var name = mColumnNames.RemoveAll(i => i.Contains("Name"));

            }
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
                    datatble = GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= " + rowID);
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
            List<string> mColumnNames = new List<string>();
            DataTable dataTable = new DataTable();
            bool duplicate = false;
            using (var db = new LiteDatabase(FileFullPath))
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
                            var dr = dt.NewRow() as LiteDataRow;
                            if (dr != null)
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
                                        if (property.Value.AsString == "System.Collections.Generic.Dictionary`2[System.String,BsonValue]" || property.Value.AsString == "System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]")
                                        {
                                            dr[property.Key] = "";
                                        }
                                        else if (property.Value.AsString == "System.Data.DataRowCollection" || property.Value.AsString == "System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]")
                                        {
                                            duplicate = true;
                                        }
                                        else
                                        {
                                            dr[property.Key] = property.Value.AsString;
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
                            var resultdxs = db.Engine.Run(query);

                            // Converting BSON to JSON 
                            JArray array = new JArray();
                            foreach (BsonValue bs in db.Engine.Run(query))
                            {
                                string js = LiteDB.JsonSerializer.Serialize(bs, true, true);
                                if (js == "0")
                                {
                                    return dataTable;
                                }
                                JObject jo = JObject.Parse(js);
                                JObject jo2 = new JObject();
                                foreach (JToken jt in jo.Children())
                                {
                                    if ((jt as JProperty).Name != "_id")
                                    {
                                        string sData = jt.ToString();
                                        if (sData.Contains(": {\r\n  \"_type\": \"System.DBNull, mscorlib\"\r\n}"))
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
                            DataTable dt = dataTable;
                            bool dosort = true;
                            DataTable dt2 = dataTable.Clone();
                            dt2.Columns["GINGER_ID"].DataType = Type.GetType("System.Int32");

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
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Exception Occurred: ", ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Exception Occurred: ", ex);
                }
            }
            dataTable.AcceptChanges();
            return dataTable;
        }


        private DataSourceTable CheckDSTableDesign(DataTable dtTable)
        {
            string tablename = dtTable.TableName;
            DataSourceTable sTableDetail = new DataSourceTable();
            sTableDetail.Name = tablename;

            int iCount = 0;
            int iIdCount = 0;
            int iUpdateCount = 0;
            foreach (DataColumn column in dtTable.Columns)
            {

                if (column.ToString() == "GINGER_KEY_NAME" || column.ToString() == "GINGER_KEY_VALUE")
                { iCount++; }
                else if (column.ToString() == "GINGER_ID")
                { iIdCount++; }
                else if (column.ToString() == "GINGER_LAST_UPDATE_DATETIME" || column.ToString() == "GINGER_LAST_UPDATED_BY")
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
            ObservableList<DataSourceTable> mDataSourceTableDetails = new ObservableList<DataSourceTable>();
            using (var db = new LiteDatabase(FileFullPath))
            {
                IEnumerable<string> Tables = db.GetCollectionNames();
                foreach (string table in Tables)
                {
                    var results = db.GetCollection(table).Find(Query.All(), 0).ToList();
                    var dt = new LiteDataTable(results.ToString());
                    foreach (var doc in results)
                    {
                        var dr = dt.NewRow() as LiteDataRow;
                        if (dr != null)
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
                                    dr[property.Key] = property.Value.AsString;
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
            using (var db = new LiteDatabase(FileFullPath))
            {
                IEnumerable<string> Tables = db.GetCollectionNames();
                foreach (string table in Tables)
                {
                    if (tableName == table)
                    { return true; }
                }
            }
            return false;
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            using (var db = new LiteDatabase(FileFullPath))
            {
                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
                var table = db.GetCollection(tableName);
                foreach (var doc in results)
                {
                    doc.Remove(columnName);
                    table.Update(doc);
                }
            }
        }

        public override void RenameTable(string tableName, string newTableName)
        {
            bool renameSuccess = false;
            bool tableExist = false;
            using (var db = new LiteDatabase(FileFullPath))
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

        public override bool RunQuery(string query)
        {
            using (LiteDatabase db = new LiteDatabase(FileFullPath))
            {
                var result = db.Engine.Run(query);
            }

            return true;
        }

        public void RunQuery(string query, int LocateRowValue, string DSTableName, bool MarkUpdate = false, bool NextAvai = false)
        {
            DataTable dt = GetQueryOutput("db." + DSTableName + ".find");
            DataRow row = dt.Rows[LocateRowValue];
            string rowValue = Convert.ToString(row["GINGER_ID"]);

            //Rownumber
            if (!query.Contains("where"))
            {
                GetQueryOutput(query + " where GINGER_ID = " + rowValue);
            }
            //Nextavailable
            else if (NextAvai)
            {
                DataTable datatble = GetQueryOutput("db." + DSTableName + ".find GINGER_USED = \"False\" limit 1");
                row = datatble.Rows[LocateRowValue];
                string rowID = Convert.ToString(row["GINGER_ID"]);

                string[] Stringsplit = query.Split(new[] { "where " }, StringSplitOptions.None);
                query = Stringsplit[0] + " where GINGER_ID = " + rowID;
                GetQueryOutput(query);

                if (MarkUpdate)
                {
                    GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_USED =\"False\" and GINGER_ID= " + rowID);
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

                    dt = GetQueryOutput("db." + DSTableName + ".find GINGER_ID=" + querysplit[1]);

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

                GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= " + rowID);
            }

        }

        public string GetResultString(string query)
        {
            string result = null;
            using (LiteDatabase db = new LiteDatabase(FileFullPath))
            {
                var resultdxs = db.Engine.Run(query);
                foreach (BsonValue bs in resultdxs)
                {
                    BsonDocument aa = bs.AsDocument;
                    foreach (KeyValuePair<string, BsonValue> keyval in aa.RawValue)
                    {
                        result = keyval.Value.AsString;
                    }
                }
            }
            return result;
        }

        public object GetResult(string query)
        {
            object result = null;
            using (LiteDatabase db = new LiteDatabase(FileFullPath))
            {
                var resultdxs = db.Engine.Run(query);
                foreach (BsonValue bs in resultdxs)
                {
                    result = bs.AsString;
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
                string rowID = row[0].ToString();
                string Newquery = "db." + DSTableName + ".update GINGER_USED = \"True\" where " + tokens[1];
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

            using (LiteDatabase db = new LiteDatabase(FileFullPath))
            {
                dataTable.DefaultView.Sort = "GINGER_ID";
                var table = db.GetCollection(dataTable.ToString());
                var doc = BsonMapper.Global.ToDocument(table);

                DataTable changed = dataTable.GetChanges();
                DataTable dtChange = dataTable;
                //if datatable is empty
                if (dtChange.Rows.Count == 0 && changed == null)
                {
                    return;
                }
                table.Delete(Query.All());
                List<BsonDocument> batch = new List<BsonDocument>();
                if ((dtChange != null))
                {
                    foreach (DataRow dr in dtChange.Rows)
                    {
                        dr["GINGER_LAST_UPDATED_BY"] = System.Environment.UserName;
                        dr["GINGER_LAST_UPDATE_DATETIME"] = DateTime.Now.ToString();
                        if (dr["GINGER_ID"] != null || string.IsNullOrWhiteSpace((Convert.ToString(dr["GINGER_ID"]))))
                        {
                            dr["GINGER_ID"] = dtChange.Rows.IndexOf(dr) + 1;
                        }

                        var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);

                        var mapper = new BsonMapper();
                        var sd = mapper.ToDocument(dictionary);

                        var nobj = mapper.ToObject<Dictionary<string, BsonValue>>(doc);

                        batch.Add(new BsonDocument(sd));
                        table.Upsert(batch);
                    }
                }
                dtChange.AcceptChanges();
                var result = db.GetCollection(table.Name).Find(Query.All()).ToList();

                if (dataTable.Rows.Count > result.Count)
                {
                    table.Upsert(batch);
                }

                var rea = db.GetCollection(table.Name).Find(Query.All(), 0).ToList();
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
                if (sColName != "GINGER_ID" && sColName != "GINGER_LAST_UPDATED_BY" && sColName != "GINGER_LAST_UPDATE_DATETIME")
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
                    if (sColName != "GINGER_ID" && sColName != "GINGER_LAST_UPDATED_BY" && sColName != "GINGER_LAST_UPDATE_DATETIME")
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
                        dt = GetQueryOutput(Query);
                        dt.TableName = actDSTable.DSTableName;
                        DataRow row = dt.Rows[0];

                        if (actDSTable.MarkUpdate)
                        {
                            string[] tokens = Query.Split(new[] { "where" }, StringSplitOptions.None);
                            DataTable dataTable = GetQueryOutput("db." + actDSTable.DSTableName + ".find" + tokens[1]);
                            dataTable.TableName = actDSTable.DSTableName;
                            DataRow rown = dataTable.Rows[0];
                            string rowID = Convert.ToString(rown["GINGER_ID"]);
                            string query = "db." + actDSTable.DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"";
                            RunQuery(query);
                        }
                        actDSTable.AddOrUpdateReturnParamActual(actDSTable.VarName, row[0].ToString());
                    }
                    break;

                case eControlAction.MarkAsDone:
                case eControlAction.SetValue:
                    if (actDSTable.IsKeyValueTable)
                    {
                        RunQuery(Query);
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    else if (actDSTable.ByRowNum)
                    {
                        RunQuery(Query, Int32.Parse(actDSTable.LocateRowValue), actDSTable.DSTableName, actDSTable.MarkUpdate);
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    else
                    {
                        if (actDSTable.ByNextAvailable)
                        {
                            RunQuery(Query, 0, actDSTable.DSTableName, actDSTable.MarkUpdate, true);
                        }
                        else
                        {
                            RunQuery(Query, 0, actDSTable.DSTableName, actDSTable.MarkUpdate);
                        }
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    break;

                case eControlAction.MarkAllUsed:
                case eControlAction.MarkAllUnUsed:
                    var aa = GetResult(Query);
                    actDSTable.AddOrUpdateReturnParamActual("Result", aa.ToString());
                    break;

                case eControlAction.RowCount:
                    var a = GetResult(Query);
                    actDSTable.AddOrUpdateReturnParamActual("Count", a.ToString());
                    break;
                case eControlAction.AvailableRowCount:
                    dt = GetQueryOutput(Query);

                    actDSTable.AddOrUpdateReturnParamActual("Count", dt.Rows.Count.ToString());
                    break;
                case eControlAction.ExportToExcel:
                    string[] token = Query.Split(new[] { "," }, StringSplitOptions.None);
                    ExporttoExcel(actDSTable.DSTableName, token[0], token[1]);
                    break;
                case eControlAction.DeleteRow:
                    if (actDSTable.IsKeyValueTable)
                    {
                        RunQuery(Query);
                        actDSTable.AddOrUpdateReturnParamActual("Deleted Record", "1");
                    }
                    else if (actDSTable.ByRowNum)
                    {
                        if (actDSTable.BySelectedCell)
                        {
                            string[] tokens = Query.Split(new[] { "where" }, StringSplitOptions.None);
                            RunQuery("db." + actDSTable.DSTableName + ".delete " + tokens[1]);
                        }
                        else
                        {
                            dt = GetQueryOutput("db." + actDSTable.DSTableName + ".find");
                            int x = Int32.Parse(actDSTable.LocateRowValue);
                            DataRow row = dt.Rows[x];
                            string rowValue = Convert.ToString(row["GINGER_ID"]);
                            RunQuery(Query + " GINGER_ID = \"" + rowValue + "\"");
                            actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                        }
                    }
                    else if (actDSTable.ByNextAvailable)
                    {
                        dt = GetQueryOutput("db." + actDSTable.DSTableName + ".find GINGER_USED=\"False\"");
                        DataRow row = dt.Rows[0];
                        string rowValue = Convert.ToString(row["GINGER_ID"]);
                        string query = "db." + actDSTable.DSTableName + ".delete GINGER_ID=\"" + rowValue + "\"";
                        GetResult(query);
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    else if (actDSTable.BySelectedCell)
                    {
                        string[] tokens = Query.Split(new[] { "where" }, StringSplitOptions.None);
                        RunQuery("db." + actDSTable.DSTableName + ".delete " + tokens[1]);
                    }
                    else
                    {
                        GetResult(Query);
                        actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    }
                    break;
                case eControlAction.DeleteAll:
                    List<object> AllItemsList = null;
                    DeleteAll(AllItemsList, actDSTable.DSTableName);

                    actDSTable.AddOrUpdateReturnParamActual("Output", "Success");
                    break;
                default:

                    break;
            }
            return;
        }

        public override void InitConnection()
        {
            DataSourceBase ADC;
            ADC = new GingerLiteDB();
            ADC.DSType = DataSourceBase.eDSType.LiteDataBase;
            if (FilePath != null)
            {
                FileFullPath = amdocs.ginger.GingerCoreNET.WorkSpace.Instance.SolutionRepository.ConvertSolutionRelativePath(FilePath);
            }
        }

        public override string AddNewCustomizedTableQuery()
        {
            return "GINGER_ID,GINGER_USED,GINGER_LAST_UPDATED_BY,GINGER_LAST_UPDATE_DATETIME";
        }

        public override int GetRowCount(string TableName)
        {
            return GetQueryOutput("db." + TableName + ".find").Rows.Count;
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
            return GetQueryOutput("db." + mDSTableName + ".select GINGER_KEY_NAME where GINGER_KEY_NAME != null");
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

        private void DeleteDBTableContents(string TName)
        {
            using (LiteDatabase db = new LiteDatabase(FileFullPath))
            {
                var table = db.GetCollection(TName);
                List<string> ColumnList = GetColumnList(TName);
                table.Delete(Query.All());
                List<BsonDocument> batch = new List<BsonDocument>();

                //bool b = ColumnList.Any(s => s.Contains("GINGER_USED"));
                //string[] List = null;
                //if (b)
                //{
                //    List = AddNewCustomizedTableQuery().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //}
                //else
                //{
                //    List = AddNewKeyValueTableQuery().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //}

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
            string[] collist = sColList.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] vallist = sColVals.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
            query = "db." + Name + ".insert {" + colvalues + "GINGER_LAST_UPDATED_BY:\"" + System.Environment.UserName 
                                 + "\"" + ",GINGER_LAST_UPDATE_DATETIME:\"" + DateTime.Now.ToString() + "\"" + ",GINGER_USED:\"False\" }";

            return query;
        }
    }
}




