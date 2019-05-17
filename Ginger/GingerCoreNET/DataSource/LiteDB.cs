using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using GingerCore.DataSource;
using LiteDB;
using System.Threading;
using LiteDB.Shell;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GingerCoreNET.DataSource
{
    public class GingerLiteDB : DataSourceBase
    {
        string mFilePath = "";
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
        public override void Init(string sFilePath, string sMode = "Read")
        {
            mFilePath = sFilePath;
            try
            {
                Database = new LiteDatabase(sFilePath);
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Cannot Connect to Datasource. Exception Occured : ", ex);
            }
        }
        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            using (var db = new LiteDatabase(mFilePath))
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
            using (var db = new LiteDatabase(mFilePath))
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

        public override void Close()
        {
            Database = new LiteDatabase(mFilePath);
            Database.Dispose();
        }

        public override string CopyTable(string tableName)
        {
            string CopyTableName = tableName;

            while (IsTableExist(CopyTableName))
                CopyTableName = CopyTableName + "_Copy";
            if (CopyTableName != tableName)
            {
                using (var db = new LiteDatabase(mFilePath))
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
            using (var db = new LiteDatabase(mFilePath))
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
                                    //mColumnNames.Add(dt.Columns.ToString());
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
            using (var db = new LiteDatabase(mFilePath))
            {
                db.DropCollection(tableName);
            }
        }

        public override bool ExporttoExcel(string TableName, string sExcelPath, string sSheetName)
        {
            throw new NotImplementedException();
        }

        public override List<string> GetColumnList(string tableName)
        {
            List<string> mColumnNames = new List<string>();

            using (var db = new LiteDatabase(mFilePath))
            {
                if (tableName == "")
                    return mColumnNames;
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
                //aa.Rows.Add(dt.Rows);
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


        public String GetQueryOutput(string query,string col, int rownumber , bool mark = false, string DSTableName = null)
        {
            DataTable datatble = GetQueryOutput(query);
            DataRow row = datatble.Rows[rownumber];

            if (mark)
            {
                string rowID = row["GINGER_ID"].ToString();
                datatble=GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"");
            }
            return row[col].ToString();
        }

        public override DataTable GetQueryOutput(string query)
        {
            List<string> mColumnNames = new List<string>();
            DataTable dataTable = new DataTable();
            using (var db = new LiteDatabase(mFilePath))
            {
                var results = db.GetCollection(query).Find(Query.All(), 0).ToList();
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
                                    if (property.Value.AsString == "System.Collections.Generic.Dictionary`2[System.String,LiteDB.BsonValue]")
                                    {
                                        dr[property.Key] = "";
                                    }
                                    else
                                    {
                                        dr[property.Key] = property.Value.AsString;
                                    }
                                }
                            }
                            dt.Rows.Add(dr);

                            DataTable aa = dt;
                            aa.TableName = query;
                            dataTable = aa;
                        }
                    }
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
                                return dataTable;
                            JObject jo = JObject.Parse(js);
                            JObject jo2 = new JObject();
                            foreach (JToken jt in jo.Children())
                            {
                                if ((jt as JProperty).Name != "_id")
                                {
                                    string sData = jt.ToString();
                                    if (sData.Contains(": {\r\n  \"_type\": \"System.DBNull, mscorlib\"\r\n}"))
                                    {
                                        if (jt.HasValues )
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
                        var json = JsonConvert.SerializeObject(array);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR,"Exception Occured: ",ex);
                    }
                }
            }
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
                    iCount++;
                else if (column.ToString() == "GINGER_ID")
                    iIdCount++;
                else if (column.ToString() == "GINGER_LAST_UPDATE_DATETIME" || column.ToString() == "GINGER_LAST_UPDATED_BY")
                    iUpdateCount++;
            }
            if (iCount == 2 && dtTable.Columns.Count == 2 + iIdCount + iUpdateCount)
                sTableDetail.DSTableType = DataSourceTable.eDSTableType.GingerKeyValue;
            else
                sTableDetail.DSTableType = DataSourceTable.eDSTableType.Customized;
           
            sTableDetail.DSC = this;
            return sTableDetail;
        }
        
        public override ObservableList<DataSourceTable> GetTablesList()
        {
            DataTable Datatable = new DataTable();
            ObservableList<DataSourceTable> mDataSourceTableDetails = new ObservableList<DataSourceTable>();
            using (var db = new LiteDatabase(mFilePath))
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
                                foreach(var property in doc.RawValue)
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
            using (var db = new LiteDatabase(mFilePath))
            {
                IEnumerable<string> Tables = db.GetCollectionNames();
                foreach (string table in Tables)
                {
                    if (tableName == table)
                        return true;
                }
            }
            return false;
        }

        public override void RemoveColumn(string tableName, string columnName)
        {
            using (var db = new LiteDatabase(mFilePath))
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
            using (var db = new LiteDatabase(mFilePath))
            {
                db.RenameCollection(tableName, newTableName);
            }
        }

        public override void RunQuery(string query)
        {
            using (LiteDatabase db = new LiteDatabase(mFilePath))
            {
                var result = db.Engine.Run(query);
            }
        }

        public void RunQuery(string query, int LocateRowValue, string DSTableName, bool MarkUpdate=false, bool NextAvai= false)
        {
            DataTable dt = GetQueryOutput("db." + DSTableName + ".find");
            DataRow row = dt.Rows[LocateRowValue];
            string rowValue = row["GINGER_ID"].ToString();

            //Rownumber
            if (!query.Contains("where"))
            {
                GetQueryOutput(query + " where GINGER_ID = \"" + rowValue + "\"");
            }
            //Nextavailable
            else if (NextAvai)
            {
                DataTable datatble = GetQueryOutput("db." + DSTableName + ".find GINGER_USED = \"False\" limit 1");
                row = datatble.Rows[LocateRowValue];
                string rowID = row["GINGER_ID"].ToString();
               
                query = query + " and GINGER_ID= \"" + rowID + "\"";
                GetQueryOutput(query);

                if (MarkUpdate)
                {
                    GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_USED =\"False\" and GINGER_ID= \"" + rowID + "\"");
                }
                return;
            }
            // Where Condition
            else
            {
                if (query.Contains("where") && query.Contains("GINGER_ID ="))
                {
                     GetQueryOutput(query);

                    string[] querysplit = query.Split(new[] { "GINGER_ID =" }, StringSplitOptions.None);

                    dt = GetQueryOutput("db." + DSTableName + ".find GINGER_ID=" + querysplit[1] );
                    
                    row = dt.Rows[0];
                }
                else if (query.Contains("where"))
                {
                    GetQueryOutput(query);
                    //string[] querysplit = query.Split(new[] { "where =" }, StringSplitOptions.None);
                    //dt = GetQueryOutput("db." + DSTableName + ".find GINGER_ID=" + querysplit[1]);
                }
            }
            if (MarkUpdate)
            {
                string rowID = row["GINGER_ID"].ToString();
                
                GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"");
            }
            
        }

        public object GetResult (string query)
        {
            object result = null;
            using (LiteDatabase db = new LiteDatabase(mFilePath))
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
                string Newquery = "db." + DSTableName + ".update GINGER_USED = \"True\" where "+ tokens[1];
                RunQuery(Newquery);
            }
            return row[0].ToString();
        }

        public override void SaveTable(DataTable dataTable)
        {
            using (LiteDatabase db = new LiteDatabase(mFilePath))
            {
                var table = db.GetCollection(dataTable.ToString());
                var doc = BsonMapper.Global.ToDocument(table);

                DataTable changed = dataTable.GetChanges();
                DataTable dtChange = dataTable;
                
                table.Delete(Query.All());
                List<BsonDocument> batch = new List<BsonDocument>();
                if (!(dtChange == null))
                {
                    foreach (DataRow dr in dtChange.Rows)
                    {
                        dr["GINGER_LAST_UPDATED_BY"] = System.Environment.UserName;
                        dr["GINGER_LAST_UPDATE_DATETIME"] = DateTime.Now.ToString();
                        var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);
                       
                        var mapper = new BsonMapper();
                        var sd = mapper.ToDocument(dictionary);
                       
                        var nobj = mapper.ToObject<Dictionary<string, BsonValue>>(doc);

                        batch.Add(new BsonDocument(sd));
                        table.Upsert(batch);
                    }
                }

                var result = db.GetCollection(table.Name).Find(Query.All(), 0).ToList();

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
    }
}




