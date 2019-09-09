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


using Amdocs.Ginger.Plugin.Core.Database;
using Amdocs.Ginger.Plugin.Core.Reporter;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace PostgreSQL
{
    public class PostgreSQLConnection : IDatabase
    {
        private DbConnection oConn = null;
        private DbTransaction tran = null;
        public Dictionary<string, string> KeyvalParamatersList = new Dictionary<string, string>();
        private IReporter mReporter;
        public string Name => throw new NotImplementedException();

        public string ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string GetConnectionString(Dictionary<string, string> parameters)
        {
            string connStr = null;
            bool res;
            res = false;
            string ConnectionString = parameters.FirstOrDefault(pair => pair.Key == "ConnectionString").Value;
            string User = parameters.FirstOrDefault(pair => pair.Key == "UserName").Value;
            string Password = parameters.FirstOrDefault(pair => pair.Key == "Password").Value;
            string TNS = parameters.FirstOrDefault(pair => pair.Key == "TNS").Value;
            string Name = parameters.FirstOrDefault(pair => pair.Key == "Name").Value;

            if (String.IsNullOrEmpty(ConnectionString) == false)
            {
                connStr = ConnectionString.Replace("{USER}", User);

                String deCryptValue = null; //EncryptionHandler.DecryptString(Password, ref res, false);
                if (res == true)
                { connStr = connStr.Replace("{PASS}", deCryptValue); }
                else
                { connStr = connStr.Replace("{PASS}", Password); }
            }
            else
            {
                String strConnString = TNS;
               
                connStr = "Data Source=" + TNS + ";User Id=" + User + ";";

                String deCryptValue = null;// EncryptionHandler.DecryptString(Password, ref res, false);
                string[] host = TNS.Split(':');
                if (host.Length == 2)
                {
                    connStr = String.Format("Server ={0};Port={1};User Id={2}; Password={3};Database={4};", host[0], host[1], User, deCryptValue, Name);
                }
                else
                {
                    connStr = String.Format("Server ={0};User Id={1}; Password={2};Database={3};", TNS, User, deCryptValue, Name);
                }
            }

            return connStr;
        }
        public bool OpenConnection(Dictionary<string, string> parameters)
        {
            KeyvalParamatersList = parameters;
            string connectConnectionString = GetConnectionString(parameters);
            oConn = new NpgsqlConnection(connectConnectionString);
            oConn.Open();
            return true;
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
                mReporter.ToLog2(eLogLevel.ERROR, "Failed to close DB Connection", e);
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
                mReporter.ToLog2(eLogLevel.ERROR, "Failed to execute query:" + Query, e);
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
            if (OpenConnection(KeyvalParamatersList))
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    command.CommandText = sql;
                   
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
                    mReporter.ToLog2(eLogLevel.ERROR, "Failed to execute query:" + sql, e);
                    throw e;
                }
                finally
                {
                    reader.Close();
                }
            }

            return int.Parse(rc);
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            string sql = "SELECT {0} FROM {1} WHERE {2}";
            sql = String.Format(sql, Column, Table, Where);
            String rc = null;
            DbDataReader reader = null;
            if (OpenConnection(KeyvalParamatersList))
            {
                try
                {
                    DbCommand command = oConn.CreateCommand();
                    command.CommandText = sql;
                   
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
                mReporter.ToLog2(eLogLevel.ERROR, "", e);
                //Reporter.ToUser(eUserMsgKey.DbTableError, "table columns", e.Message);
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
            DataTable table = oConn.GetSchema("Tables");
            string tableName = "";
            foreach (DataRow row in table.Rows)
            {
                tableName = (string)row[2];
            }
            rc.Add(tableName);
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
                    mReporter.ToLog2(eLogLevel.ERROR, "Commit failed for:" + updateCmd, e);
                    throw e;
                }
            }
            return result;
        }

        public bool TestConnection()
        {
            throw new NotImplementedException();
        }

        public void InitReporter(IReporter reporter)
        {
            mReporter = reporter;
        }
    }
}
