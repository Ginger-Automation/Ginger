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

        public  DataTable datatable(string tableName, string CopyTableName= null)
        {
            using (var db = new LiteDatabase(mFilePath))
            {
                var results = db.GetCollection(tableName).Find(Query.All(), 0);
                if (CopyTableName== null)
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
                                if (ads=="GINGER_ID")
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
        public byte[] ToByteArray<T>(T obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
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
                    //query = "db.yuj.select $ where sew='aa'";
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
                    try
                    {
                        dataTable = JsonConvert.DeserializeObject<DataTable>(array.ToString());
                        var json = JsonConvert.SerializeObject(array);
                    }
                    catch (Exception ex)
                    {


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
            throw new NotImplementedException();
        }

        public override void SaveTable(DataTable dataTable)
        {
            using (LiteDatabase db = new LiteDatabase(mFilePath))

            {
                var table = db.GetCollection(dataTable.ToString());
                var doc = BsonMapper.Global.ToDocument(table);
                
                //var dic = BsonSerializer.Deserialize(doc);
                DataTable dtChange = dataTable.GetChanges();
                List<string> aa= GetColumnList(dataTable.ToString());
                //dtChange = datatable(dataTable.ToString());
                table.Delete(Query.All());
                List<BsonDocument> batch = new List<BsonDocument>();
                if (!(dtChange == null))
                {
                    foreach (DataRow dr in dtChange.Rows)
                    {
                        var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);
                        //foreach(KeyValuePair<string , object> a in dictionary)
                        //{
                        //    if (a.Key.ToString()=="_id" && a.Value.ToString() == null)
                        //    {
                        //        Random r = new Random();
                        //        dictionary[a.Key] = (long)((r.NextDouble() * 2.0 - 1.0) * long.MaxValue);
                        //    }
                        //    if(a.Key.ToString()== "GINGER_LAST_UPDATED_BY")
                        //    {
                        //        dictionary[a.Key] = System.Environment.UserName;
                        //    }
                        //}
                        var mapper = new BsonMapper();
                        var sd = mapper.ToDocument(dictionary);
                        var nobj = mapper.ToObject<Dictionary<string, BsonValue>>(doc);

                        batch.Add(new BsonDocument(sd));
                        //table.Insert(sd);
                    }
                }
                
                   
                
                table.Upsert(batch);
                //table.InsertBulk(batch);
               
                var re = db.GetCollection(table.Name).Find(Query.All(), 0).ToList();

                //if (dtChange == null)
                //    return;
                //foreach (DataRow row in dataTable.Rows)
                //{
                //    if (row.RowState == DataRowState.Modified)
                //    {
                //        table.Upsert(doc);
                //    }
                //    else if (row.RowState == DataRowState.Added)
                //    {
                //        //db.Engine.Run("");
                //        table.Upsert(doc);
                //    }
                //}
                //dataTable.AcceptChanges();
                //Init(mFilePath);
                //var docs = table.Find(x => x.AsDocument);

                ////docs=
                //foreach(BsonDocument doc in docs)
                //{
                //    table.Update(doc);
                //}
            }
        }

        public override void UpdateTableList(ObservableList<DataSourceTable> dsTableList)
        {
            throw new NotImplementedException();
        }
    }
}




