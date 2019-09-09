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


using Amdocs.Ginger.CoreNET;
using Amdocs.Ginger.Plugin.Core.Database;
using Amdocs.Ginger.Plugin.Core.Reporter;
using Couchbase;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Couchbase.N1QL;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;

namespace CouchBase
{
    public class CouchBaseConnection : IDatabase
    {
        Cluster clusterCB = null;
        public Dictionary<string, string> KeyvalParamatersList = new Dictionary<string, string>();
        string ConnectionString = null;
        string User = null;
        string Password = null;
        string TNS = null;
        IQueryResult<dynamic> result = null;
        string bucketName = null;
        private IReporter mReporter;
        public string Name => throw new NotImplementedException();

        string IDatabase.ConnectionString { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void GetConnectionString(Dictionary<string, string> parameters)
        {
            ConnectionString = parameters.FirstOrDefault(pair => pair.Key == "ConnectionString").Value;
            User = parameters.FirstOrDefault(pair => pair.Key == "UserName").Value;
            Password = parameters.FirstOrDefault(pair => pair.Key == "Password").Value;
            TNS = parameters.FirstOrDefault(pair => pair.Key == "TNS").Value;
        }
        public List<string> GetKeyspaceList()
        {
            return null;
        }
        
        public bool OpenConnection(Dictionary<string, string> parameters)
        {
            KeyvalParamatersList = parameters;
            GetConnectionString(parameters);
            try
            {
                clusterCB = new Couchbase.Cluster(new ClientConfiguration
                {
                    ViewRequestTimeout = 45000,
                    Servers = new List<Uri> { new Uri(TNS) },
                });
                bool res = false;
                //TODO: need to decrypt the password in the Database->PassCalculated
                String deCryptValue = null; //EncryptionHandler.DecryptString(Password, ref res, false);
                if (res == true)
                {
                    clusterCB.Authenticate(User, deCryptValue);
                }
                else
                {
                    clusterCB.Authenticate(User, Password);
                }
                return true;
            }
            catch (Exception e)
            {
                mReporter.ToLog2(eLogLevel.ERROR, "Failed to connect to Couchbase DB", e);
                return false;
            }
        }

        public void CloseConnection()
        {
            clusterCB.Dispose();
        }
        /// <summary>
        /// Need to fix it. (convert IQueryResult<dynamic> to Datatable)
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        public DataTable DBQuery(string Query)
        {
            List<object> list = new List<object>();
            DoBeforeExecutionOperations(Query);
            result = clusterCB.Query<dynamic>(Query);
           
            for (int i = 0; i < result.Rows.Count; i++)
            {
                //FIXME!!!!!!!
                //list.Add(result.Rows[i]);
                //Act.ParseJSONToOutputValues(result.Rows[i].ToString(), i + 1);
            }
            DataTable dataTable = new DataTable();
            
            //put a breakpoint here and check datatable  
            return dataTable;
            
        }
        private string GetBucketName(string inputSQL, bool IftinputSQL= false)
        {
            string bucketName = string.Empty;
            
            if (IftinputSQL)
            {
                bucketName = inputSQL.Replace("`", "");
                bucketName = bucketName.Replace("'", "");
            }
            else
            {
                bucketName = inputSQL.Substring(inputSQL.IndexOf(" from ") + 6);
                bucketName = bucketName.Substring(0, bucketName.IndexOf(" ")).Replace("`", "");
            }
            return bucketName;
        }
        public bool ConnecttoBucket(string bucketName)
        {
            try
            {
                var bucket = clusterCB.OpenBucket(bucketName);
                string bucketpassword = bucket.Configuration.Password;
                ClassicAuthenticator classicAuthenticator = new ClassicAuthenticator(User, Password);
                classicAuthenticator.AddBucketCredential(bucketName, bucketpassword);
                clusterCB.Authenticate(classicAuthenticator);
                //TODO: need to check and true and flase on basis of connection
                return true;
            }
            catch (Exception ex)
            {
                mReporter.ToLog2(eLogLevel.ERROR, "Failed To Connect ConnectToBucket Method In GingerCouchBase DB", ex);
                return false;
            }
        }
        public void DoBeforeExecutionOperations(string SQL, bool ifTbalename= false)
        {
            if (!OpenConnection(KeyvalParamatersList))
            {
                return;
            }
            bucketName = GetBucketName(SQL, ifTbalename);

            if (!ConnecttoBucket(bucketName))
            {
                return;
            }
        }
        public int GetRecordCount(string Query)
        {
            DoBeforeExecutionOperations(Query, true);
            //FIXME!!!!!!!
            // result = clusterCB.Query<dynamic>("Select Count(*) as RECORDCOUNT from `" + bucketName + "`");
            // return int.Parse(result.Rows[0].ToString());
            return 0;
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
            var RS1 = clusterCB.Query<dynamic>(updateCmd);
            return RS1.ToString();
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
