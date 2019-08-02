using Amdocs.Ginger.Common;
using Amdocs.Ginger.Plugin.Core.Database;
using GingerCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace MySQLDatabase
{
    public class MYSQLDBConnection : IDatabase
    {
        private DbConnection oConn = null;
        private DbTransaction tran = null;
        public Dictionary<string, string> KeyvalParamatersList = new Dictionary<string, string>();

        public string Name => throw new NotImplementedException();

        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool OpenConnection(Dictionary<string, string> parameters)
        {
            KeyvalParamatersList = parameters;
            string connectConnectionString = GetConnectionString(parameters);
            try
            {
                oConn = new MySqlConnection();
                oConn.ConnectionString = connectConnectionString;
                oConn.Open();

                if ((oConn != null) && (oConn.State == ConnectionState.Open))
                {
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
        public string GetConnectionString(Dictionary<string, string> parameters)
        {
            string connStr = null;
            bool res;
            res = false;

            string ConnectionString = parameters.FirstOrDefault(pair => pair.Key == "ConnectionString").Value;
            string User = parameters.FirstOrDefault(pair => pair.Key == "UserName").Value;
            string Password = parameters.FirstOrDefault(pair => pair.Key == "Password").Value;
            string TNS = parameters.FirstOrDefault(pair => pair.Key == "TNS").Value;
            string Name= parameters.FirstOrDefault(pair => pair.Key == "Name").Value;

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
               
                connStr = "Data Source=" + TNS + ";User Id=" + User + ";";

                String deCryptValue = EncryptionHandler.DecryptString(Password, ref res, false);

                if (res == true) { connStr = connStr + "Password=" + deCryptValue + ";"; }
                else { connStr = connStr + "Password=" + Password + ";"; }

                connStr = "Server=" + TNS + ";Database=" + Name + ";UID=" + User + ";PWD=" + deCryptValue;
            }
            return connStr;
        }

        public void CloseConnection()
        {
            try
            {
                if (oConn != null)
                {
                    oConn.Close();
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to close DB Connection", e);
                throw (e);
            }
            finally
            {
                oConn?.Dispose();
            }
        }

        public DataTable DBQuery(string Query)
        {
            
            List<string> Headers = new List<string>();
            List<List<string>> Records = new List<List<string>>();
            bool IsConnected = false;
            List<object> ReturnList = new List<object>();
            DataTable dataTable = new DataTable();
            DbDataReader reader = null;
            try
            {
                if (oConn == null)
                {
                    IsConnected = OpenConnection(KeyvalParamatersList);
                }
                if (IsConnected || oConn != null)
                {
                    DbCommand command = oConn.CreateCommand();
                    command.CommandText = Query;
                    command.CommandType = CommandType.Text;

                    // Retrieve the data.
                    reader = command.ExecuteReader();

                    // Create columns headers
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Headers.Add(reader.GetName(i));
                        dataTable.Columns.Add(reader.GetName(i));
                    }

                    while (reader.Read())
                    {

                        List<string> record = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            record.Add(reader[i].ToString());
                        }
                        Records.Add(record);
                        dataTable.Rows.Add(record);
                    }

                    ReturnList.Add(Headers);
                    ReturnList.Add(Records);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to execute query:" + Query, e);
                throw e;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }

            return dataTable;
        }

        public int GetRecordCount(string Query)
        {
            string sql = "SELECT COUNT(1) FROM " + Query;

            String rc = null;
            DbDataReader reader = null;
            
                try
                {
                    DbCommand command = oConn.CreateCommand();
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
            
            return Convert.ToInt32(rc);
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            string sql = "SELECT {0} FROM {1} WHERE {2}";
            sql = String.Format(sql, Column, Table, Where);
            String rc = null;
            DbDataReader reader = null;
                try
                {
                    DbCommand command = oConn.CreateCommand();
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
            
            return rc;
        }

        public List<string> GetTablesColumns(string table)
        {
            DbDataReader reader = null;
            List<string> rc = new List<string>() { "" };
            if ((oConn == null || string.IsNullOrEmpty(table)))
            {
                return rc;
            }
            try
            {
                DbCommand command = oConn.CreateCommand();
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

        public List<string> GetTablesList(string Name = null)
        {
            List<string> rc = new List<string>() { "" };
            try
            {
                DataTable table = oConn.GetSchema("Tables");
                string tableName = "";
                foreach (DataRow row in table.Rows)
                {
                    tableName = (string)row[2];
                    rc.Add(tableName);
                }
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get table list for DB:" + this, e);
                throw (e);
            }
            return rc;
        }    
        
        public string RunUpdateCommand(string updateCmd, bool commit = true)
        {
            string result = "";
                using (DbCommand command = oConn.CreateCommand())
                {
                    try
                    {
                        if (commit)
                        {
                            tran = oConn.BeginTransaction();
                            // to Command object for a pending local transaction
                            command.Connection = oConn;
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
            return result;
        }

        public bool TestConnection()
        {
            throw new NotImplementedException();
        }
    }
}
