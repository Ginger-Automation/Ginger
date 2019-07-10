using Amdocs.Ginger.Common;
using GingerCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace PostgreSQL
{
    public class PostgreSQLConnection : Amdocs.Ginger.CoreNET.IDatabase
    {
        private DbConnection oConn = null;
        private DbTransaction tran = null;
        public Dictionary<string, string> KeyvalParamatersList = new Dictionary<string, string>();
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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to close DB Connection", e);
                throw (e);
            }
            finally
            {
                oConn?.Dispose();
            }
        }

        public List<object> DBQuery(string Query)
        {
            throw new NotImplementedException();
        }

        public int GetRecordCount(string Query)
        {
            throw new NotImplementedException();
        }

        public string GetSingleValue(string Table, string Column, string Where)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTablesColumns(string table)
        {
            throw new NotImplementedException();
        }

        public List<string> GetTablesList(string Name = null)
        {
            throw new NotImplementedException();
        }

        
        public string RunUpdateCommand(string updateCmd, bool commit = true)
        {
            throw new NotImplementedException();
        }
    }
}
