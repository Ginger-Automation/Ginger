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

using Amdocs.Ginger.Common;
using Cassandra;
using GingerCore.Actions;
using GingerCore.NoSqlBase.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GingerCore.NoSqlBase
{
    public class GingerCassandra : NoSqlBase
    {
        Cluster cluster = null;
        ISession session = null;
        ActDBValidation Act = null;
        dynamic myclass = null;
        string mUDTName = null;

        public override bool Connect()
        {
            try
            {
                const string queryTimeoutString = "querytimeout=";
                const string sslString = "ssl=";
                int queryTimeout = 20000;//default timeout (20 seconds).
                string[] queryArray = Db.DatabaseOperations.TNSCalculated.ToLower().Split(';');
                SSLOptions sslOptions = null;
                for (int i = 1; i < queryArray.Length; i++)
                {
                    switch (queryArray[i])
                    {
                        case var str when str.Contains(queryTimeoutString):
                            string queryTimeoutValue = str[(str.IndexOf(queryTimeoutString) + queryTimeoutString.Length)..];
                            if (!int.TryParse(queryTimeoutValue, out int timeout))
                            {
                                throw new ArgumentException("Query timeout value is not a valid integer.");
                            }
                            queryTimeout = Convert.ToInt32(queryTimeoutValue) * 1000;
                            break;

                        case var str when str.Contains(sslString):
                            string sslValue = str[(str.IndexOf(sslString) + sslString.Length)..];
                            sslOptions = SetupSslOptions(sslValue);
                            break;
                        default:
                            throw new ArgumentException("Please check connection string.");
                    }
                }
                string[] HostKeySpace = queryArray[0].ToLower().Replace("http://", "").Replace("https://", "").Split('/');
                string[] HostPort = HostKeySpace[0].Split(':');

                if (HostPort.Length == 2)
                {
                    if (string.IsNullOrEmpty(Db.Pass) && string.IsNullOrEmpty(Db.User))
                    {
                        if (sslOptions == null)
                        {
                            cluster = Cluster.Builder().AddContactPoint(HostPort[0]).WithPort(Int32.Parse(HostPort[1])).WithQueryTimeout(queryTimeout).Build();
                        }
                        else
                        {
                            cluster = Cluster.Builder()
                                .AddContactPoint(HostPort[0])
                                .WithPort(Int32.Parse(HostPort[1]))
                                .WithSSL(sslOptions)
                                .WithQueryTimeout(queryTimeout)
                                .Build();
                        }
                    }
                    else
                    {
                        if (sslOptions == null)
                        {
                            cluster = Cluster.Builder().WithCredentials(Db.User.ToString(), Db.Pass.ToString()).AddContactPoint(HostPort[0]).WithPort(Int32.Parse(HostPort[1])).WithQueryTimeout(queryTimeout).Build();
                        }
                        else
                        {
                            cluster = Cluster.Builder()
                                .AddContactPoint(HostPort[0])
                                .WithPort(Int32.Parse(HostPort[1]))
                                .WithAuthProvider(new PlainTextAuthProvider(Db.User, Db.Pass))
                                .WithSSL(sslOptions)
                                .WithQueryTimeout(queryTimeout)
                                .Build();
                        }
                    }
                }
                else
                {
                    cluster = Cluster.Builder().AddContactPoint(Db.DatabaseOperations.TNSCalculated).WithQueryTimeout(queryTimeout).Build();
                }

                if (HostKeySpace.Length > 1)
                {
                    if (!HostKeySpace[1].ToLower().Contains(queryTimeoutString.ToLower()))
                    {
                        session = cluster.Connect(HostKeySpace[1]);
                    }
                }
                else
                {
                    session = cluster.Connect();
                }
                return true;
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to connect to Cassandra DB. Please check Connection String", e);
                throw;
            }
        }

        public override bool MakeSureConnectionIsOpen()
        {
            try
            {
                if (session != null && !session.IsDisposed)
                {
                    Metadata m = cluster.Metadata;
                    ICollection<string> Keyspaces = m.GetKeyspaces();
                    if (Keyspaces.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return Connect();
                    }
                }
                else
                {
                    return Connect();
                }
            }
            catch (Exception ex)
            {
                return Connect();
            }
        }

        //TODO: need this while checking Test Connection , need to find a better way
        public GingerCassandra(Environments.Database mDB)
        {
            this.Db = mDB;
        }

        public GingerCassandra(ActDBValidation.eDBValidationType DBValidationtype, Environments.Database mDB, ActDBValidation mact)
        {
            Action = DBValidationtype;
            this.Db = mDB;
            Act = mact;
        }

        public override List<string> GetKeyspaceList()
        {
            Connect();
            List<string> keyspaces = [];
            Metadata m = cluster.Metadata;
            string keyspace = null;
            ICollection<string> GetKeyspaces = m.GetKeyspaces();
            foreach (string k in GetKeyspaces)
            {
                //TODO: find a better way to filter default keyspaces
                if (!(k.Contains("system")))
                {
                    keyspace = k;
                    keyspaces.Add(keyspace);
                }
            }
            return keyspaces;
        }

        public override List<string> GetTableList(string keyspace)
        {
            Connect();
            List<string> table = [];
            Metadata m = cluster.Metadata;
            ICollection<string> tables = m.GetKeyspace(keyspace).GetTablesNames();

            foreach (string t in tables)
            {
                table.Add(t);
            }
            return table;
        }

        public override Task<List<string>> GetColumnList(string tablename)
        {
            Connect();

            List<string> cols = [];
            string sql = "Select * from " + tablename;
            RowSet r = session.Execute(sql);
            CqlColumn[] Cols = r.Columns;

            foreach (CqlColumn col in Cols)
            {
                cols.Add(col.Name);
            }
            return Task.FromResult(cols);
        }

        private void Disconnect()
        {
            try
            {
                session.Dispose();
                cluster.Dispose();
            }
            finally
            {
                session = null;
            }
        }

        public Type TypeConverter(RowSet RS, string Type1)
        {
            Type type = null;

            switch (Type1)
            {
                case "Varchar":
                    type = typeof(string);
                    return type;

                case "ascii":
                    type = typeof(string);
                    return type;

                case "text":
                    type = typeof(string);
                    return type;

                case "bigint":
                    type = typeof(long);
                    return type;

                case "counter":
                    type = typeof(long);
                    return type;

                case "custom":
                    type = typeof(byte[]);
                    return type;

                case "date":
                    type = typeof(LocalDate);
                    return type;

                case "list":
                    type = typeof(IEnumerable<>);
                    return type;

                case "map":
                    type = typeof(IDictionary<,>);
                    return type;

                case "Set":
                    type = typeof(IEnumerable<>);
                    return type;

                case "smallint":
                    type = typeof(short);
                    return type;

                case "timeuuid":
                    type = typeof(TimeUuid);
                    return type;

                case "tinyint":
                    type = typeof(sbyte);
                    return type;

                case "uuid":
                    type = typeof(Guid);
                    return type;

                case "decimal":
                    type = typeof(decimal);
                    return type;

                case "double":
                    type = typeof(double);
                    return type;

                case "float":
                    type = typeof(float);
                    return type;

                case "Int":
                    type = typeof(int);
                    return type;
            }
            return type;
        }

        public void CreateDynamicClass(Type TP, string classname, string[] stringarray, Type[] typearray)
        {
            MyClassBuilder MCB = new MyClassBuilder(classname);
            myclass = MCB.CreateObject(stringarray, typearray);
            TP = myclass.GetType();
            GingerCore.NoSqlBase.GingerUDTMap udt = new GingerUDTMap(TP, classname);
            session.UserDefinedTypes.Define(udt);
        }

        public void ConnectWithKeyspace(String name)
        {
            string[] name1 = name.Split('.');
            mUDTName = name1[1].ToString();
            string keyspace = name1[0].ToString();

            session = cluster.Connect(keyspace);
        }

        public void ListUDT(CqlColumn col, RowSet R)
        {
            if (((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeCode.ToString() == "Udt")
            {
                var name = ((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Name;
                ConnectWithKeyspace(name);

                int a = ((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Fields.Count;

                Dictionary<string, Type> keyvalue = [];

                Type t = null;
                for (int k = 0; k < a; k++)
                {
                    dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].Name).ToString();
                    var Fieldtype = ((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].TypeCode;
                    Type ks = Fieldtype.GetType();
                    t = TypeConverter(R, Fieldtype.ToString());
                    keyvalue.Add(Fieldname, t);
                }
                string[] stringarray = keyvalue.Keys.ToArray();
                Type[] typearray = keyvalue.Values.ToArray();
                CreateDynamicClass(t, mUDTName, stringarray, typearray);
            }
        }

        public void UDT(CqlColumn col, RowSet R)
        {
            var name = ((Cassandra.UdtColumnInfo)col.TypeInfo).Name;

            ConnectWithKeyspace(name);
            int a = ((Cassandra.UdtColumnInfo)col.TypeInfo).Fields.Count;

            Dictionary<string, Type> keyvalue = [];
            Type t = null;
            for (int k = 0; k < a; k++)
            {
                dynamic Fieldname = (((Cassandra.UdtColumnInfo)col.TypeInfo).Fields[k].Name).ToString();
                var Fieldtype = ((Cassandra.UdtColumnInfo)col.TypeInfo).Fields[k].TypeCode;
                Type ks = Fieldtype.GetType();
                t = TypeConverter(R, Fieldtype.ToString());
                keyvalue.Add(Fieldname, t);
            }

            string[] stringarray = keyvalue.Keys.ToArray();
            Type[] typearray = keyvalue.Values.ToArray();
            CreateDynamicClass(t, mUDTName, stringarray, typearray);
        }

        public void SetUDT(CqlColumn col, RowSet R)
        {
            if (((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeCode.ToString() == "Udt")
            {
                var name = ((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Name;
                ConnectWithKeyspace(name);
                int a = ((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Fields.Count;

                Dictionary<string, Type> keyvalue = [];
                Type t = null;
                for (int k = 0; k < a; k++)
                {
                    dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Fields[k].Name).ToString();
                    var Fieldtype = ((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Fields[k].TypeCode;
                    Type ks = Fieldtype.GetType();
                    t = TypeConverter(R, Fieldtype.ToString());
                    keyvalue.Add(Fieldname, t);
                }
                string[] stringarray = keyvalue.Keys.ToArray();
                Type[] typearray = keyvalue.Values.ToArray();
                CreateDynamicClass(t, mUDTName, stringarray, typearray);
            }
        }

        public void MapUDT(CqlColumn col, RowSet R)
        {
            try
            {
                if (((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeCode.ToString() == "Udt")
                {

                    var name = ((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Name;
                    ConnectWithKeyspace(name);

                    int a = ((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Fields.Count;

                    Dictionary<string, Type> keyvalue = [];
                    Type t = null;
                    for (int k = 0; k < a; k++)
                    {
                        dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].Name).ToString();
                        var Fieldtype = ((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].TypeCode;
                        Type ks = Fieldtype.GetType();
                        t = TypeConverter(R, Fieldtype.ToString());
                        keyvalue.Add(Fieldname, t);
                    }

                    string[] stringarray = keyvalue.Keys.ToArray();
                    Type[] typearray = keyvalue.Values.ToArray();
                    CreateDynamicClass(t, mUDTName, stringarray, typearray);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
        }

        public void UpdateRowSetWithUDT(RowSet R, string SQL)
        {
            IEnumerable<Row> Rows = R.GetRows();
            CqlColumn[] Cols = R.Columns;
            Type TP = null;
            int i = 0;
            RowSet s = null;
            foreach (Row r in Rows)
            {
                int j = 0;
                foreach (CqlColumn col in Cols)
                {
                    if (col.TypeCode.ToString() is "Udt" or "Set" or "Map" or "List")
                    {
                        dynamic value = r.GetValue(typeof(object), col.Name);

                        if (col.TypeCode.ToString() == "Udt")
                        {
                            UDT(col, R);
                        }

                        if (col.TypeCode.ToString() == "List")
                        {
                            ListUDT(col, R);

                        }
                        if (col.TypeCode.ToString() == "Set")
                        {
                            SetUDT(col, R);
                        }
                        if (col.TypeCode.ToString() == "Map")
                        {
                            MapUDT(col, R);
                        }
                    }
                    j++;
                }
                i++;
            }
            //TODO: Need to find a way to Execute the SQL once mnot twice
            s = session.Execute(SQL);
            UpdateOutValues(s, TP);
        }

        public bool IfUseJSON()
        {
            bool value = false;
            if (!(Db.DBVer.ToString().StartsWith("1") || Db.DBVer.ToString().StartsWith("2.1") || Db.DBVer.ToString().StartsWith("2.0") || Db.DBVer.ToString().StartsWith("0")))
            {
                value = true;
            }
            return value;
        }

        public override void PerformDBAction()
        {
            string SQL = Act.QueryValue;
            string keyspace = Act.Keyspace;
            ValueExpression VE = new ValueExpression(Db.ProjEnvironment, Db.BusinessFlow, Db.DSList)
            {
                Value = SQL
            };
            string SQLCalculated = VE.ValueCalculated;
            try
            {
                switch (Action)
                {
                    case Actions.ActDBValidation.eDBValidationType.FreeSQL:
                        //To Use JSON Appraoch only if Cassandra version is 2.2 or onwards
                        try
                        {
                            if (IfUseJSON() && (!SQLCalculated.ToLower().Contains("select json ")))
                            {
                                if (SQLCalculated.ToLower().Contains("select"))
                                {
                                    SQLCalculated = Regex.Replace(SQLCalculated, "select", "select json ", RegexOptions.IgnoreCase);
                                }
                                RowSet RS = session.Execute(SQLCalculated);
                                UpdateOutValues(RS);
                            }
                            else
                            {
                                RowSet RS = session.Execute(SQLCalculated);
                                UpdateRowSetWithUDT(RS, SQLCalculated);
                            }
                        }
                        catch (Exception e)
                        {
                            Act.Error = "Please check the version of the Database and update on the Environment(by default it will take 2.2)" + e;
                            Reporter.ToLog(eLogLevel.ERROR, e.Message);
                        }
                        break;

                    case Actions.ActDBValidation.eDBValidationType.RecordCount:

                        string SQLRecord = keyspace + "." + SQLCalculated;
                        int count = GetRecordCount(SQLRecord);
                        Act.AddOrUpdateReturnParamActual("Record Count", count.ToString());
                        break;

                    case Actions.ActDBValidation.eDBValidationType.SimpleSQLOneValue:
                        string where = Act.Where;
                        string table = Act.Table;
                        string col = Act.Column;
                        string sql = null;
                        try
                        {
                            if (IfUseJSON())
                            {
                                sql = "Select json " + col + " from " + keyspace + "." + table + " where " + where + ";";
                                RowSet R = session.Execute(sql);

                                UpdateOutValues(R);
                            }
                            else
                            {
                                sql = "Select " + col + " from " + keyspace + "." + table + " where " + where + ";";
                                RowSet R = session.Execute(sql);

                                UpdateRowSetWithUDT(R, sql);
                            }
                        }
                        catch (Exception e)
                        {
                            Act.Error = "Please check the version of the Database and update on the Environment(by default it will take 2.2)";
                            Reporter.ToLog(eLogLevel.ERROR, e.Message);
                        }
                        break;

                    case Actions.ActDBValidation.eDBValidationType.UpdateDB:
                        RowSet RS1 = session.Execute(SQLCalculated);
                        break;

                    default:
                        throw new Exception("Action Not SUpported");
                }
            }
            catch (Exception e)
            {
                Act.Error = "Failed to execute";
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            if (!Db.KeepConnectionOpen)
            {
                Disconnect();
            }
        }

        int GetRecordCount(string SQL)
        {
            string sql = "SELECT * FROM " + SQL;
            RowSet RS = session.Execute(sql);
            int c = 0;
            if (RS.IsFullyFetched)
            {
                foreach (object o in RS)
                {
                    c++;
                }
            }
            return c;
        }

        public void RetriveUDT(CqlColumn col, Row r, Type t, int i)
        {
            dynamic value = r.GetValue(typeof(object), col.Name);
            dynamic val = r.GetValue(t, col.Name);
            myclass = val;

            int a = ((Cassandra.UdtColumnInfo)col.TypeInfo).Fields.Count;
            Dictionary<string, object> keyvalue = [];
            Type TP = myclass.GetType();

            for (int k = 0; k < a; k++)
            {
                dynamic Fieldname = (((Cassandra.UdtColumnInfo)col.TypeInfo).Fields[k].Name).ToString();

                var nameOfProperty = Fieldname;
                var propertyInfo = value.GetType().GetProperty(nameOfProperty);
                var valueUDT = propertyInfo.GetValue(value, null);

                keyvalue.Add(Fieldname, t);

                Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty, valueUDT, i.ToString());

            }
        }

        public void RetriveSet(CqlColumn col, Row r, Type t, int i)
        {
            dynamic value = r.GetValue(typeof(object), col.Name);
            var value1 = r.GetValue(typeof(object), col.Name);

            myclass = value1;
            string am = ((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeCode.ToString();

            if (am == "Udt")
            {
                int a = ((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Fields.Count;
                Dictionary<string, object> keyvalue = [];
                Type TP = myclass.GetType();
                try
                {
                    for (int k = 0; k < a; k++)
                    {
                        dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.SetColumnInfo)col.TypeInfo).KeyTypeInfo).Fields[k].Name).ToString();
                        try
                        {
                            for (int s = 0; s < a; s++)
                            {
                                var nameOfProperty = Fieldname;
                                var propertyInfo = value[s].GetType().GetProperty(nameOfProperty);
                                var valueUDT = propertyInfo.GetValue(value[s], null);

                                if (k > 0)
                                {
                                    Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty + "." + s.ToString(), valueUDT, i.ToString());
                                }
                                else
                                {
                                    Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty, valueUDT, i.ToString());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                        }

                    }
                }
                catch (Exception e)
                { Console.WriteLine(e.StackTrace); }
            }
            else
            { // retrieve values of Set without UDTs
                int m = 0;

                foreach (object o in value)
                {
                    Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + m, o.ToString(), i.ToString());
                    m++;
                }
            }
        }

        public void RetriveList(CqlColumn col, Row r, Type t, int i)
        {
            dynamic value = r.GetValue(typeof(object), col.Name);
            var value1 = r.GetValue(typeof(object), col.Name);

            myclass = value1;

            if (((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeCode.ToString() == "Udt")
            {
                int a = ((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Fields.Count;
                for (int k = 0; k < a; k++)
                {
                    dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.ListColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].Name).ToString();
                    try
                    {
                        for (int s = 0; s < a; s++)
                        {
                            var nameOfProperty = Fieldname;
                            var propertyInfo = value[s].GetType().GetProperty(nameOfProperty);
                            var valueUDT = propertyInfo.GetValue(value[s], null);
                            if (k > 0)
                            {
                                Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty + "." + s.ToString(), valueUDT, i.ToString());
                            }
                            else
                            {
                                Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty, valueUDT, i.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                    }
                }
            }
            else
            { // retrieve list values without UDT
                int m = 0;

                foreach (object o in value)
                {
                    Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + m, o.ToString(), i.ToString());
                    m++;
                }
            }
        }

        public void RetriveMap(CqlColumn col, Row r, Type t, int i)
        {
            dynamic value = r.GetValue(typeof(object), col.Name);
            myclass = value;
            string IFUDT = ((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeCode.ToString();

            if (IFUDT == "Udt")
            {
                int a = ((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Fields.Count;
                //Dictionary<string, object> keyvalue = new Dictionary<string, object>();
                dynamic abc;
                try
                {
                    foreach (var item in myclass)
                    {
                        abc = item.Value;
                        for (int k = 0; k < a; k++)
                        {
                            try
                            {
                                dynamic Fieldname = (((Cassandra.UdtColumnInfo)((Cassandra.MapColumnInfo)col.TypeInfo).ValueTypeInfo).Fields[k].Name).ToString();
                                var nameOfProperty = Fieldname;
                                var propertyInfo = abc.GetType().GetProperty(nameOfProperty);
                                var valueUDT = propertyInfo.GetValue(abc, null);
                                Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + nameOfProperty, valueUDT.ToString(), i.ToString());
                            }
                            catch (Exception e)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                            }
                        }
                    }
                }
                catch (Exception e)
                { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }
            }
            else
            {// to retrieve values without udt
                int m = 0;
                foreach (object o in value)
                {
                    Act.AddOrUpdateReturnParamActualWithPath(col.Name + "." + m, o.ToString(), i.ToString());
                    m++;
                }
            }
        }

        void UpdateOutValues(RowSet RS, Type t = null)
        {
            IEnumerable<Row> Rows = RS.GetRows();
            CqlColumn[] Cols = RS.Columns;

            int i = 1;
            foreach (Row r in Rows)
            {
                int j = 1;
                foreach (CqlColumn col in Cols)
                {
                    if (col.Name == "[json]")
                    {
                        dynamic value1 = r.GetValue(typeof(object), col.Name);
                        Act.ParseJSONToOutputValues(value1, i);
                    }
                    else
                    {
                        dynamic value = r.GetValue(typeof(object), col.Name);
                        try
                        {
                            if (col.TypeCode.ToString() is "Udt" or "Set" or "Map" or "List")
                            {
                                if (col.TypeCode.ToString() == "Udt")
                                {
                                    RetriveUDT(col, r, t, i);
                                }
                                if (col.TypeCode.ToString() == "Set")
                                {
                                    RetriveSet(col, r, t, i);
                                }
                                if (col.TypeCode.ToString() == "List")
                                {
                                    RetriveList(col, r, t, i);
                                }
                                if (col.TypeCode.ToString() == "Map")
                                {
                                    RetriveMap(col, r, t, i);
                                }
                            }
                            else
                            {
                                Act.AddOrUpdateReturnParamActualWithPath(col.Name, value.ToString(), i.ToString());
                            }
                            j++;
                        }
                        catch (Exception e)
                        {//to return colunms with null values
                            if (e.Message.ToString() == "Cannot perform runtime binding on a null reference")
                            {
                                Act.AddOrUpdateReturnParamActualWithPath(col.Name, "", i.ToString());
                            }
                        }
                    }
                }
                i++;
            }
        }

        public static SSLOptions SetupSslOptions(string sslParamValue)
        {
            try
            {
                var sslProtocol = Enum.Parse<SslProtocols>(sslParamValue, ignoreCase: true);
                return new SSLOptions(sslProtocol, false, (_, _, _, _) => true);
            }
            catch (ArgumentException e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "SSL value not correct. Please enter SSL value like Default, None, Ssl2, Ssl3, Tls, Tls11, Tls12, Tls13", e);
                Reporter.ToLog(eLogLevel.ERROR, "Note - Also try removing ssl= parameter from connection string", e);
                throw new ArgumentException("SSL value not correct. Please enter SSL value like Default, None, Ssl2, Ssl3, Tls, Tls11, Tls12, Tls13.");
            }
        }
    }
}
