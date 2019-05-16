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
                    //RefreshCollections();
                }
            }
        }
        public void FillDataGridView(IEnumerable<BsonDocument> documents)
        {
            //if (lb_Collections.Items.Contains("[QUERY]"))
            //{
            //    lb_Collections.Items.Remove("[QUERY]");
            //}
            //dataGridView.DataSource = null;
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
                //dataGridView.DataSource = dt;
            }
        }
        public override void Init(string sFilePath, string sMode = "Read")
        {
            mFilePath = sFilePath;
            using (Database = new LiteDatabase(sFilePath))
            {
                Console.Out.WriteLine("DB Created at the location: " + sFilePath);
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
                    //CopyTable.Upsert(doc);
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
                DataTable aa = dt;
                aa.Rows.Add(dt.Rows);
                aa.TableName = CopyTableName;
                return aa;
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

        public String GetQueryOutput(string query, string col, bool mark = false, string DSTableName= null)
        {
            DataTable ddd = GetQueryOutput(query);
            
            DataRow row = ddd.Rows[0];

            if (mark)
            {
                string rowID = row["GINGER_ID"].ToString();
                //string query = "db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"";
                DataTable d=GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"");
            }

            return  row[col].ToString();
        }

        public String GetQueryOutput(string query,string col, int rownumber , bool mark = false, string DSTableName = null)
        {
            DataTable ddd = GetQueryOutput(query);
            
            DataRow row = ddd.Rows[rownumber];
            if (mark)
            {
                string rowID = row["GINGER_ID"].ToString();
                ddd=GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"");
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
                    try
                    {
                        var resultdxs = db.Engine.Run(query);
                        
                        JArray array = new JArray();
                        foreach (BsonValue bs in db.Engine.Run(query))
                        {
                            string js = LiteDB.JsonSerializer.Serialize(bs, true, true);
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
                        dataTable = JsonConvert.DeserializeObject<DataTable>(array.ToString());
                        
                        var json = JsonConvert.SerializeObject(array);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("is not a valid shell command."))
                        {
                            string[] tokens = query.Split(new[] { "from" }, StringSplitOptions.None);
                            char[] splitchar = { ' ' };
                            string[] tablename = tokens[1].Split(splitchar);
                            
                            var res = db.GetCollection(tablename[1]).Find(Query.All(), 0).ToList();
                            var dt = new LiteDataTable(tablename[1].ToString());
                            foreach (var doc in res)
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
                                                dataTable.Columns.Add(property.Key, typeof(string));
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
                                   
                                    DataRow[] rowArray = aa.Select("GINGER_ID= 1");
                                    foreach (DataRow row in rowArray)
                                    {
                                        dataTable.ImportRow(row);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            return dataTable;
        }

        private void ConvertSQLShellCommand(string SQLcommand)
        {
            string[] tokens = SQLcommand.Split(new[] { "from" }, StringSplitOptions.None);
            char[] splitchar = { ' ' };
            string [] tablename = tokens[1].Split(splitchar);
            DataTable dataTable = new DataTable();
            using (var db = new LiteDatabase(mFilePath))
            {
                string query = "db." + tablename[1] + ".find limit 100";
                var resultdxs = db.Engine.Run(query);
                var docs = BsonMapper.Global.ToDocument(resultdxs[0]);

                JArray array = new JArray();
                foreach (BsonValue bs in db.Engine.Run(query))
                {
                    string js = LiteDB.JsonSerializer.Serialize(bs, true, true);
                    JObject jo = JObject.Parse(js);
                    JObject jo2 = new JObject();
                    foreach (JToken jt in jo.Children())
                    {
                        if ((jt as JProperty).Name != "_id")
                        {
                            jo2.Add(jt);
                        }
                    }
                    array.Add(jo2);
                }
                dataTable = JsonConvert.DeserializeObject<DataTable>(array.ToString());
                DataRow[] rowArray = dataTable.Select("GINGER_ID= 1");
                foreach (DataRow row in rowArray)
                {
                    dataTable.ImportRow(row);
                }
                //var result = db.GetCollection(tablename);

            }

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
        public DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);
            //Get all the properties by using reflection   
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {

                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
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
                var resultdxs = db.Engine.Run(query);
            }
        }

        public void RunQuery(string query, int LocateRowValue, string DSTableName, bool MarkUpdate=false, bool NextAvai= false)
        {
            DataTable dt = GetQueryOutput("db." + DSTableName + ".find");
            //int x = Int32.Parse(LocateRowValue);
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
                DataTable dta = GetQueryOutput("db." + DSTableName + ".find GINGER_USED = \"False\" limit 1");
                row = dta.Rows[LocateRowValue];
                string rowID = row["GINGER_ID"].ToString();
                //GetQueryOutput(query);
                query = query + " and GINGER_ID= \"" + rowID + "\"";
                GetQueryOutput(query);

                if (MarkUpdate)
                {
                    //string rowID = row["GINGER_ID"].ToString();
                    //string query = "db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"";
                    GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_USED =\"False\" and GINGER_ID= \"" + rowID + "\"");
                }
                return;
            }
            else
            {
                if (query.Contains("where") && query.Contains("GINGER_ID ="))
                {
                     GetQueryOutput(query);

                    string[] querysplit = query.Split(new[] { "GINGER_ID =" }, StringSplitOptions.None);

                    dt = GetQueryOutput("db." + DSTableName + ".find GINGER_ID=" + querysplit[1] );
                    //int x = Int32.Parse(LocateRowValue);
                    row = dt.Rows[0];
                }
            }
            if (MarkUpdate)
            {
                string rowID = row["GINGER_ID"].ToString();
                //string query = "db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"";
                GetQueryOutput("db." + DSTableName + ".update GINGER_USED = \"True\" where GINGER_ID= \"" + rowID + "\"");
            }
            return;
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
                string queryq = "db." + DSTableName + ".update GINGER_USED = \"True\" where "+ tokens[1];
                RunQuery(queryq);
            }
            return row[0].ToString();
        }

        public override void SaveTable(DataTable dataTable)
        {
            using (LiteDatabase db = new LiteDatabase(mFilePath))
            {
                var table = db.GetCollection(dataTable.ToString());
                var doc = BsonMapper.Global.ToDocument(table);
                
                //var dic = BsonSerializer.Deserialize(doc);
                
                DataTable dtChange = dataTable;
                //DataTable dtChange = dataTable;
                List<string> aa= GetColumnList(dataTable.ToString());
                //dtChange = datatable(dataTable.ToString());
                table.Delete(Query.All());
                List<BsonDocument> batch = new List<BsonDocument>();
                if (!(dtChange == null))
                {
                    foreach (DataRow dr in dtChange.Rows)
                    {
                        var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);
                       
                        var mapper = new BsonMapper();
                        var sd = mapper.ToDocument(dictionary);
                        
                        var nobj = mapper.ToObject<Dictionary<string, BsonValue>>(doc);

                        batch.Add(new BsonDocument(sd));
                        table.Upsert(batch);
                    }
                }
                

                var re = db.GetCollection(table.Name).Find(Query.All(), 0).ToList();

                if (dataTable.Rows.Count > re.Count)
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




