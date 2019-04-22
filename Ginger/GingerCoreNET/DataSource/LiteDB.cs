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

namespace GingerCoreNET.DataSource
{
    public class GingerLiteDB : DataSourceBase
    {
        string mFilePath = "";

        int count = 1;
      
        public override void Init(string sFilePath, string sMode = "Read")
        {
            mFilePath = sFilePath;
            using (var db = new LiteDatabase(sFilePath))
            {
                Console.Out.WriteLine("DB Created at the location: " + sFilePath);
            }
        }
        public override void AddColumn(string tableName, string columnName, string columnType)
        {
            using (var db = new LiteDatabase(mFilePath))
            {
                if(tableName== "MyCustomizedDataTable")
                {
                    var table = db.GetCollection("MyCustomizedDataTable");
                    table.Insert(new BsonDocument {[columnName] ="" });
                    
                }
                else
                {
                    var table = db.GetCollection(tableName);
                    table.Insert(new BsonDocument { [columnName] = "" });
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
            throw new NotImplementedException();
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
                var results = db.GetCollection(tableName).Find(Query.All(), 0).ToList();
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
                aa.Rows.Add(dt.Rows);
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

        public override DataTable GetQueryOutput(string query)
        {
            List<string> mColumnNames = new List<string>();
            DataTable dtTable = new DataTable();
            using (var db = new LiteDatabase(mFilePath))
            {
                if (query.Contains("from"))
                {
                    query = query.Split(new string[] { "from" }, StringSplitOptions.None).Last();
                    query = query.Split(new char[] { ' ' })[1];
                }
                else if (query.Contains("FROM"))
                {
                    query = query.Split(new string[] { "FROM" }, StringSplitOptions.None).Last();
                    //query = query.Split(new char[] { ' ' })[1];
                }
                var results = db.GetCollection(query).Find(Query.All(), 0).ToList();

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

                        DataTable aa = dt;
                        aa.TableName = query;
                        dtTable = aa;
                    }

                }


                //dtTable.TableName = tableName;
            }
            return dtTable;
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
                var table = db.GetCollection(tableName);
                table.Delete(x => x.ContainsKey(columnName));
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
                using (var aaaa = new LiteEngine(mFilePath))
                {
                    
                }
                    var table = db.GetCollection(dataTable.ToString());

                var doc = BsonMapper.Global.ToDocument(table);
                DataTable dtChange = dataTable.GetChanges();
                List<string> aa= GetColumnList(dataTable.ToString());
                dtChange = datatable(dataTable.ToString());
                List<BsonDocument> batch = new List<BsonDocument>();
                foreach (DataRow dr in dtChange.Rows)
                {
                    var dictionary = dr.Table.Columns.Cast<DataColumn>().ToDictionary(col => col.ColumnName, col => dr[col.ColumnName]);
                    batch.Add(doc);
                }
                table.Upsert(batch);
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




