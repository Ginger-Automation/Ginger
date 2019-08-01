using Amdocs.Ginger.Common;
using Amdocs.Ginger.Plugin.Core.Database;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;



namespace MSAccessDB
{
    public class MSAccessDBCon : IDatabase
    {
        private OleDbConnection conn = null;
        SqlConnection sqlConnection;
        // private OdbcConnection conn;

        private DbTransaction tran = null;
        private DateTime LastConnectionUsedTime;
        public Dictionary<string, string> KeyvalParamatersList = new Dictionary<string, string>();

        public string Name => throw new NotImplementedException();

        public void CloseConnection()
        {
            try
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to close DB Connection", e);
                throw (e);
            }
            finally
            {
                conn?.Dispose();
            }
        }

        public DataTable DBQuery(string Query)
        {
            DataTable results = new DataTable();

            //using (OleDbConnection conn = new OleDbConnection())
            //{


            OleDbCommand cmd = new OleDbCommand(Query, conn);

                // conn.Open();

                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);

                adapter.Fill(results);
            //}

            return results;

            //using (SqlDataAdapter dataAdapter= new SqlDataAdapter(Query, sqlConnection))
            //{
            //    // create the DataSet 
            //    DataSet dataSet = new DataSet();
            //    // fill the DataSet using our DataAdapter 
            //    dataAdapter.Fill(dataSet);
            //}

            //// MakeSureConnectionIsOpen();
            //List<string> Headers = new List<string>();
            //List<List<string>> Records = new List<List<string>>();
            //bool IsConnected = false;
            //List<object> ReturnList = new List<object>();
            //DataTable dataTable = new DataTable();
            //DbDataReader reader = null;
            //try
            //{
            //    if (conn == null)
            //    {
            //        IsConnected = OpenConnection(KeyvalParamatersList);
            //    }
            //    if (IsConnected || conn != null)
            //    {
            //        DbCommand command = conn.CreateCommand();
            //        command.CommandText = Query;
            //        command.CommandType = CommandType.Text;

            //        // Retrieve the data.
            //        reader = command.ExecuteReader();

            //        // Create columns headers
            //        for (int i = 0; i < reader.FieldCount; i++)
            //        {
            //            Headers.Add(reader.GetName(i));
            //            dataTable.Columns.Add(reader.GetName(i));
            //        }

            //        while (reader.Read())
            //        {
            //            List<string> record = new List<string>();
            //            for (int i = 0; i < reader.FieldCount; i++)
            //            {
            //                record.Add(reader[i].ToString());
            //            }
            //            Records.Add(record);
            //            dataTable.Rows.Add(record);
            //        }
            //        ReturnList.Add(Headers);
            //        ReturnList.Add(Records);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to execute query:" + Query, e);
            //    throw e;
            //}
            //finally
            //{
            //    if (reader != null)
            //        reader.Close();
            //}
            
            //return dataTable;
        }
       
        public string GetConnectionString(Dictionary<string,string> parameters)
        {
            string connStr = null;
            bool res;
            res = false;

            string ConnectionString = parameters.FirstOrDefault(pair => pair.Key == "ConnectionString").Value;
            string User = parameters.FirstOrDefault(pair => pair.Key == "UserName").Value;
            string Password = parameters.FirstOrDefault(pair => pair.Key == "Password").Value;
            string TNS = parameters.FirstOrDefault(pair => pair.Key == "TNS").Value;
            if (String.IsNullOrEmpty(ConnectionString) == false)
            {

                connStr = ConnectionString.Replace("{USER}", User);

                String deCryptValue = EncryptionHandler.DecryptString(Password, ref res, false);
                if (res == true)
                { connStr = connStr.Replace("{PASS}", deCryptValue); }
                else
                { connStr = connStr.Replace("{PASS}", Password); }
            }
            else
            {
                String strConnString = TNS;
                String strProvider;
                connStr = "Data Source=" + TNS + ";User Id=" + User + ";";

                String deCryptValue = EncryptionHandler.DecryptString(Password, ref res, false);

                if (res == true) { connStr = connStr + "Password=" + deCryptValue + ";"; }
                else { connStr = connStr + "Password=" + Password + ";"; }


                if (strConnString.Contains(".accdb"))
                {
                    strProvider = "Provider=Microsoft.ACE.OLEDB.12.0;";
                }
                else { strProvider = "Provider=Microsoft.ACE.OLEDB.12.0;"; }

                connStr = strProvider + connStr;
            }
            return connStr;
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            string sql = "SELECT {0} FROM {1} WHERE {2}";
            sql = String.Format(sql, Column, Table, Where);
            String rc = null;
            DbDataReader reader = null;
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    DbCommand command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Retrieve the data.
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        rc = reader[0].ToString();
                        break; // We read only first row
                    }
                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    reader.Close();
                }
            }
            return rc;
        }

        public List<string> GetTablesColumns(string table)
        {
            DbDataReader reader = null;
            List<string> rc = new List<string>() { "" };
                try
                {
                    DbCommand command = conn.CreateCommand();
                    // Do select with zero records
                    command.CommandText = "select * from " + table + " where 1 = 0";
                    command.CommandType = CommandType.Text;

                    reader = command.ExecuteReader();
                    // Get the schema and read the cols
                    DataTable schemaTable = reader.GetSchemaTable();
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        string ColName = (string)row[0];
                        rc.Add(ColName);
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "", e);
                    throw (e);
                }
                finally
                {
                    reader.Close();
                }
            return rc;
        }

        public bool MakeSureConnectionIsOpen()
        {
            Boolean isCoonected = true;

            if ((conn == null) || (conn.State != ConnectionState.Open))
            {
                isCoonected = OpenConnection(KeyvalParamatersList);
            }
            return isCoonected;
        }

        public bool OpenConnection(Dictionary<string, string> parameters)
        {
            // Work with DbConnection;
            //DbConnection
            


            KeyvalParamatersList = parameters;
            
            // string connectConnectionString = GetConnectionString(parameters);
            string connectConnectionString = parameters.FirstOrDefault(pair => pair.Key == "ConnectionString").Value;
            try
            {
                // SqlConnection sqlConnection = new SqlConnection(connectConnectionString);

                // DataTable dt =  System.Data.Common.DbProviderFactories.GetFactoryClasses();  // get installed provider oledb, odbc, sql serv oracle


                // conn = new OdbcConnection(connectConnectionString);

                // sqlConnection = new SqlConnection(connectConnectionString);

                 conn = new OleDbConnection(connectConnectionString);
                conn.Open();
                
                if ((conn != null) && (conn.State == ConnectionState.Open))
                {
                    LastConnectionUsedTime = DateTime.Now;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "DB connection failed, Connection String =" + connectConnectionString, ex);
                throw (ex);
            }
            return false;
        }

        public string RunUpdateCommand(string updateCmd, bool commit = true)
        {
            string result = "";
            
            if (MakeSureConnectionIsOpen())
            {
                using (DbCommand command = conn.CreateCommand())
                {
                    try
                    {
                        if (commit)
                        {
                            tran = conn.BeginTransaction();
                            // to Command object for a pending local transaction
                            command.Connection = conn;
                            command.Transaction = tran;
                        }
                        command.CommandText = updateCmd;
                        command.CommandType = CommandType.Text;

                        result = command.ExecuteNonQuery().ToString();
                        if (commit)
                        {
                            tran.Commit();
                        }
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        Reporter.ToLog(eLogLevel.ERROR, "Commit failed for:" + updateCmd, e);
                        throw e;
                    }
                }
            }
            return result;
        }

        public int GetRecordCount(string Query)
        {
            string sql = "SELECT COUNT(1) FROM " + Query;

            String rc = null;
            DbDataReader reader = null;
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    DbCommand command = conn.CreateCommand();
                    command.CommandText = sql;
                    command.CommandType = CommandType.Text;

                    // Retrieve the data.
                    reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        rc = reader[0].ToString();
                        break; // We read only first row = count of records
                    }
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to execute query:" + sql, e);
                    throw e;
                }
                finally
                {
                    reader.Close();
                }
            }

            return Convert.ToInt32(rc);
        }

        public List<string> GetTablesList(string Name = null)
        {
            List<string> rc = new List<string>();
            if (MakeSureConnectionIsOpen())
            {
                try
                {
                    DataTable table = conn.GetSchema("Tables");
                    string tableName = "";
                    foreach (DataRow row in table.Rows)
                    {
                        tableName = (string)row[2];
                        if (!tableName.StartsWith("MSys"))  // ignore access system table
                        {
                            rc.Add(tableName);
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to get table list " + e);
                    throw (e);
                }
            }
            return rc;
        }
    }
}
